using UnityEngine;
using UnityEngine.UI;

public class SpeedBarUI : MonoBehaviour
{
    public CarMovement car;
    public Image fillImage;

    [Header("Smoothing")]
    [Tooltip("Higher = bar reacts closer to instantly. Lower = smoother but laggier, " +
             "feels disconnected from what you're actually doing. Set to 0 for a hard " +
             "snap with zero smoothing at all.")]
    public float fillSmoothSpeed = 25f;

    [Header("Color Gradient (optional)")]
    [Tooltip("If true, the bar's color shifts between lowSpeedColor and highSpeedColor based on current speed.")]
    public bool useColorGradient = true;
    public Color lowSpeedColor = new Color(0.85f, 0.25f, 0.2f);   // reddish, near base speed
    public Color midSpeedColor = new Color(0.95f, 0.75f, 0.15f);  // amber, mid speed
    public Color highSpeedColor = new Color(0.25f, 0.8f, 0.35f);  // green, near max speed

    private float displayedFill;

    private void Start()
    {
        if (car == null) Debug.LogWarning("SpeedBarUI: Car reference not assigned.", this);
        if (fillImage == null) Debug.LogWarning("SpeedBarUI: FillImage reference not assigned.", this);

        if (car != null)
        {
            displayedFill = car.SpeedNormalized;
            if (fillImage != null) fillImage.fillAmount = displayedFill;
        }
    }

    private void Update()
    {
        if (car == null || fillImage == null) return;

        float targetFill = car.SpeedNormalized; // 0-1, already exposed by CarMovement

        displayedFill = fillSmoothSpeed <= 0f
            ? targetFill
            : Mathf.Lerp(displayedFill, targetFill, fillSmoothSpeed * Time.deltaTime);

        fillImage.fillAmount = displayedFill;

        if (useColorGradient)
        {
            fillImage.color = displayedFill < 0.5f
                ? Color.Lerp(lowSpeedColor, midSpeedColor, displayedFill / 0.5f)
                : Color.Lerp(midSpeedColor, highSpeedColor, (displayedFill - 0.5f) / 0.5f);
        }
    }
}
