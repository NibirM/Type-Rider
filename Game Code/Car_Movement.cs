using System.Collections;
using UnityEngine;
using TMPro;

public class CarMovement : MonoBehaviour
{
    [Header("Waypoint Setup")]
    public string waypointParentName = "Waypoints";

    private Transform[] waypoints;

    [Header("Speed Tuning")]
    public float baseSpeed = 35f;
    public float maxSpeed = 100f;
    public float decayRate = 2f;
    public float decayGraceWindow = 0.6f;
    public float missGraceWindow = 0.3f;
    public float correctWordBoost = 18f;
    public float missedWordPenalty = 13f;

    [Header("Rotation")]
    public float turnSpeed = 5f;

    [Header("Track Pacing")]
    public float worldSpeedMultiplier = 1.6f;

    [Header("Head Start Countdown")]
    public TextMeshProUGUI countdownText;
    public float countdownDuration = 3f;
    public float goDisplayDuration = 0.5f;

    private float currentSpeed;
    private float graceTimer = 0f;
    private bool raceStarted = false;
    private Coroutine countdownCoroutine;

    public bool RaceStarted => raceStarted;

    private int currentSegment = 0;
    private float segmentT = 0f;
    private float[] segmentLengths;

    public float CurrentSpeed => currentSpeed;
    public float SpeedNormalized => Mathf.InverseLerp(baseSpeed, maxSpeed, currentSpeed);

    private void Start()
    {
        currentSpeed = baseSpeed;

        FindWaypoints();

        if (waypoints == null || waypoints.Length < 4)
        {
            Debug.LogWarning(
                "CarMovement: need at least 4 waypoint children under the GameObject named '" +
                waypointParentName + "'. Car will not move."
            );
            return;
        }

        PrecomputeSegmentLengths();
        transform.position = waypoints[0].position;

        countdownCoroutine = StartCoroutine(RunCountdown());
    }

    public void RestartRace()
    {
        if (waypoints == null || waypoints.Length < 4) return;

        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }

        currentSpeed = baseSpeed;
        graceTimer = 0f;
        currentSegment = 0;
        segmentT = 0f;
        raceStarted = false;

        transform.position = waypoints[0].position;

        if (waypoints.Length > 1)
        {
            Vector3 initialDirection = waypoints[1].position - waypoints[0].position;
            initialDirection.y = 0f;

            if (initialDirection.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(initialDirection);
            }
        }

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        countdownCoroutine = StartCoroutine(RunCountdown());
    }

    private IEnumerator RunCountdown()
    {
        float remaining = countdownDuration;

        while (remaining > 0f)
        {
            if (countdownText != null)
            {
                countdownText.text = Mathf.CeilToInt(remaining).ToString();
            }

            yield return null;
            remaining -= Time.deltaTime;
        }

        if (countdownText != null)
        {
            countdownText.text = "GO!";
        }

        yield return new WaitForSeconds(goDisplayDuration);

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        raceStarted = true;
    }

    private void FindWaypoints()
    {
        GameObject waypointParent = GameObject.Find(waypointParentName);

        if (waypointParent == null)
        {
            Debug.LogError("CarMovement: Could not find a GameObject named '" + waypointParentName + "'.");
            waypoints = null;
            return;
        }

        int childCount = waypointParent.transform.childCount;

        if (childCount == 0)
        {
            Debug.LogError("CarMovement: The '" + waypointParentName + "' GameObject has no waypoint children.");
            waypoints = null;
            return;
        }

        waypoints = new Transform[childCount];

        for (int i = 0; i < childCount; i++)
        {
            waypoints[i] = waypointParent.transform.GetChild(i);
        }

        Debug.Log("CarMovement: Found " + waypoints.Length + " waypoints under '" + waypointParentName + "'.");
    }

    private void Update()
    {
        if (!raceStarted) return;
        if (waypoints == null || waypoints.Length < 4) return;

        HandleSpeedDecay();
        MoveAlongSpline();
    }

    private void HandleSpeedDecay()
    {
        if (graceTimer > 0f)
        {
            graceTimer -= Time.deltaTime;
            return;
        }

        if (currentSpeed > baseSpeed)
        {
            currentSpeed -= decayRate * Time.deltaTime;
            currentSpeed = Mathf.Max(currentSpeed, baseSpeed);
        }
    }

    private void MoveAlongSpline()
    {
        int count = waypoints.Length;

        float distanceThisFrame = currentSpeed * worldSpeedMultiplier * Time.deltaTime;

        float length = Mathf.Max(segmentLengths[currentSegment], 0.01f);
        segmentT += distanceThisFrame / length;

        while (segmentT >= 1f)
        {
            segmentT -= 1f;
            currentSegment = (currentSegment + 1) % count;
        }

        Vector3 previousPos = transform.position;
        Vector3 newPos = GetCatmullRomPosition(segmentT, currentSegment);
        transform.position = newPos;

        Vector3 direction = newPos - previousPos;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    private Vector3 GetCatmullRomPosition(float t, int segmentIndex)
    {
        int count = waypoints.Length;

        int p0 = ((segmentIndex - 1) % count + count) % count;
        int p1 = segmentIndex % count;
        int p2 = (segmentIndex + 1) % count;
        int p3 = (segmentIndex + 2) % count;

        Vector3 a = waypoints[p0].position;
        Vector3 b = waypoints[p1].position;
        Vector3 c = waypoints[p2].position;
        Vector3 d = waypoints[p3].position;

        Vector3 pos = 0.5f * (
            (2f * b) +
            (-a + c) * t +
            (2f * a - 5f * b + 4f * c - d) * (t * t) +
            (-a + 3f * b - 3f * c + d) * (t * t * t)
        );

        return pos;
    }

    private void PrecomputeSegmentLengths()
    {
        int count = waypoints.Length;
        segmentLengths = new float[count];

        const int samples = 10;

        for (int i = 0; i < count; i++)
        {
            float length = 0f;
            Vector3 previous = GetCatmullRomPosition(0f, i);

            for (int s = 1; s <= samples; s++)
            {
                float t = s / (float)samples;
                Vector3 next = GetCatmullRomPosition(t, i);
                length += Vector3.Distance(previous, next);
                previous = next;
            }

            segmentLengths[i] = length;
        }
    }

    public void OnCorrectWord()
    {
        currentSpeed = Mathf.Min(currentSpeed + correctWordBoost, maxSpeed);
        graceTimer = decayGraceWindow;
    }

    public void OnMissedWord()
    {
        currentSpeed = Mathf.Max(currentSpeed - missedWordPenalty, baseSpeed);
        graceTimer = missGraceWindow;
    }

    public void ExtendGrace()
    {
        graceTimer = decayGraceWindow;
    }

    private void OnDrawGizmos()
    {
        Transform[] editorWaypoints = GetEditorWaypoints();

        if (editorWaypoints == null || editorWaypoints.Length < 4) return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < editorWaypoints.Length; i++)
        {
            if (editorWaypoints[i] == null) continue;
            Gizmos.DrawWireSphere(editorWaypoints[i].position, 0.5f);
        }

        Gizmos.color = Color.yellow;

        for (int i = 0; i < editorWaypoints.Length; i++)
        {
            Vector3 previous = GetEditorCatmullRomPosition(0f, i, editorWaypoints);
            const int steps = 12;

            for (int s = 1; s <= steps; s++)
            {
                float t = s / (float)steps;
                Vector3 next = GetEditorCatmullRomPosition(t, i, editorWaypoints);
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }
    }

    private Transform[] GetEditorWaypoints()
    {
        GameObject waypointParent = GameObject.Find(waypointParentName);
        if (waypointParent == null) return null;

        int childCount = waypointParent.transform.childCount;
        if (childCount < 4) return null;

        Transform[] result = new Transform[childCount];

        for (int i = 0; i < childCount; i++)
        {
            result[i] = waypointParent.transform.GetChild(i);
        }

        return result;
    }

    private Vector3 GetEditorCatmullRomPosition(float t, int segmentIndex, Transform[] points)
    {
        int count = points.Length;

        int p0 = ((segmentIndex - 1) % count + count) % count;
        int p1 = segmentIndex % count;
        int p2 = (segmentIndex + 1) % count;
        int p3 = (segmentIndex + 2) % count;

        Vector3 a = points[p0].position;
        Vector3 b = points[p1].position;
        Vector3 c = points[p2].position;
        Vector3 d = points[p3].position;

        return 0.5f * (
            (2f * b) +
            (-a + c) * t +
            (2f * a - 5f * b + 4f * c - d) * (t * t) +
            (-a + 3f * b - 3f * c + d) * (t * t * t)
        );
    }
}