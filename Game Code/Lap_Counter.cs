using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class LapManager : MonoBehaviour
{
    [Header("Race Settings")]
    public int totalLaps = 3;
    public string carTag = "Player";

    public CarMovement car;

    public float minSecondsBetweenLaps = 3f;

    [Header("UI (optional)")]
    public TextMeshProUGUI lapText;
    public TextMeshProUGUI timerText;
    public GameObject resultUI;

    [Header("Events")]
    public UnityEvent<float> onRaceFinished;

    private int currentLap = 0;
    private float raceTime = 0f;
    private float lastLapTimestamp = -999f;
    private bool raceFinished = false;
    private bool raceStarted = false;
    private bool hasLeftStartZone = false;

    private void Start()
    {
        if (car == null)
        {
            car = FindObjectOfType<CarMovement>();

            if (car != null)
            {
                Debug.Log("LapManager: auto-found CarMovement on '" + car.gameObject.name + "'.", this);
            }
            else
            {
                Debug.LogWarning(
                    "LapManager: could not find any CarMovement in the scene — " +
                    "timer will start immediately instead of waiting for the countdown.",
                    this
                );
            }
        }

        raceStarted = (car == null);

        raceTime = 0f;
        UpdateLapUI();
        if (resultUI != null) resultUI.SetActive(false);
    }

    private void Update()
    {
        if (!raceStarted)
        {
            if (car != null && car.RaceStarted)
            {
                raceStarted = true;
            }
            else
            {
                return;
            }
        }

        if (raceStarted && !raceFinished)
        {
            raceTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    public void ResetRace()
    {
        currentLap = 0;
        raceTime = 0f;
        lastLapTimestamp = -999f;
        raceFinished = false;
        hasLeftStartZone = false;

        raceStarted = (car == null);

        UpdateLapUI();
        UpdateTimerUI();

        if (resultUI != null)
        {
            resultUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (raceFinished) return;
        if (!other.CompareTag(carTag)) return;
        if (!hasLeftStartZone) return;

        if (Time.time - lastLapTimestamp < minSecondsBetweenLaps) return;
        lastLapTimestamp = Time.time;

        currentLap++;
        UpdateLapUI();

        if (currentLap >= totalLaps)
        {
            FinishRace();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(carTag)) return;
        hasLeftStartZone = true;
    }

    private void FinishRace()
    {
        raceFinished = true;

        if (resultUI != null)
        {
            resultUI.SetActive(true);
        }

        onRaceFinished?.Invoke(raceTime);

        Debug.Log($"Race finished in {FormatTime(raceTime)}");
    }

    private void UpdateLapUI()
    {
        if (lapText != null)
        {
            int displayLap = Mathf.Min(currentLap + 1, totalLaps);
            lapText.text = $"Lap {displayLap}/{totalLaps}";
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = FormatTime(raceTime);
        }
    }

    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        float remainingSeconds = seconds % 60f;
        return $"{minutes:00}:{remainingSeconds:00.00}";
    }

    public float FinalRaceTime => raceTime;
    public bool RaceFinished => raceFinished;
}