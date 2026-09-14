using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TypingSystem : MonoBehaviour
{
    [Header("References")]
    public CarMovement car;
    public LapManager lapManager;
    public TextMeshProUGUI wordDisplay;

    [Header("Tier Progression")]
    public int shortTierThreshold = 6;
    public int mediumTierThreshold = 9;
    public int longTierThreshold = 10;

    [Header("Word Pools")]
    public List<string> shortWords = new List<string> {
        "nitro", "drift", "dash", "zoom", "gear", "lane", "turn", "race",
        "bolt", "dust", "fire", "heat", "wind", "rush", "jolt", "warp"
    };
    public List<string> mediumWords = new List<string> {
        "engine", "exhaust", "chassis", "traction", "overtake", "sprinter",
        "ignition", "throttle", "cockpit", "circuit", "podium", "rivalry"
    };
    public List<string> longWords = new List<string> {
        "acceleration", "championship", "aerodynamic", "adrenaline",
        "turbocharged", "speedometer", "slipstream", "treacherous",
        "spectacular", "breakneck"
    };

    public float adaptiveWeightPerMistake = 1.5f;
    public bool newWordOnMiss = true;

    private enum Tier { Short = 0, Medium = 1, Long = 2 }
    private Tier currentTier = Tier.Short;
    private int tierProgress = 0;

    private string currentWord = "";
    private int typedIndex = 0;

    private Dictionary<char, int> mistakeCounts = new Dictionary<char, int>();

    [Header("Miss Feedback")]
    public Color missFlashColor = new Color(0.9f, 0.2f, 0.2f);
    public float missFlashDuration = 0.15f;
    private Coroutine missFlashCoroutine;

    private void Start()
    {
        if (car == null) Debug.LogWarning("TypingSystem: Car reference not assigned.", this);
        if (wordDisplay == null) Debug.LogWarning("TypingSystem: WordDisplay reference not assigned.", this);

        if (wordDisplay != null)
        {
            wordDisplay.textWrappingMode = TextWrappingModes.NoWrap;
            wordDisplay.overflowMode = TextOverflowModes.Overflow;
            wordDisplay.enableAutoSizing = false;
        }

        PickNewWord();
    }

    private void Update()
    {
        if (car == null || string.IsNullOrEmpty(currentWord)) return;
        if (!car.RaceStarted) return;
        if (lapManager != null && lapManager.RaceFinished) return;

        foreach (char c in Input.inputString)
        {
            if (char.IsControl(c)) continue;
            ProcessTypedCharacter(c);
        }
    }

    private void ProcessTypedCharacter(char typed)
    {
        char expected = currentWord[typedIndex];

        if (char.ToLowerInvariant(typed) == char.ToLowerInvariant(expected))
        {
            typedIndex++;
            UpdateWordDisplay();
            car.ExtendGrace();

            if (typedIndex >= currentWord.Length)
            {
                OnWordCompleted();
            }
        }
        else
        {
            OnWordMissed(expected);
        }
    }

    private void OnWordCompleted()
    {
        car.OnCorrectWord();

        tierProgress++;
        int threshold = currentTier == Tier.Short ? shortTierThreshold : mediumTierThreshold;

        if (currentTier != Tier.Long && tierProgress >= threshold)
        {
            currentTier++;
            tierProgress = 0;
        }

        PickNewWord();
    }

    private void OnWordMissed(char expectedChar)
    {
        string failedWord = currentWord;

        RecordMistake(expectedChar);
        car.OnMissedWord();

        if (currentTier != Tier.Short)
        {
            currentTier--;
        }
        tierProgress = 0;

        if (newWordOnMiss)
        {
            PickNewWord();
        }
        else
        {
            typedIndex = 0;
            UpdateWordDisplay();
        }

        if (missFlashCoroutine != null) StopCoroutine(missFlashCoroutine);
        missFlashCoroutine = StartCoroutine(FlashMiss(failedWord));
    }

    private IEnumerator FlashMiss(string failedWord)
    {
        if (wordDisplay != null)
        {
            string hex = ColorUtility.ToHtmlStringRGB(missFlashColor);
            wordDisplay.text = $"<color=#{hex}>{failedWord}</color>";
        }

        yield return new WaitForSeconds(missFlashDuration);

        UpdateWordDisplay();
    }

    private void RecordMistake(char expectedChar)
    {
        char key = char.ToLowerInvariant(expectedChar);
        if (!mistakeCounts.ContainsKey(key)) mistakeCounts[key] = 0;
        mistakeCounts[key]++;
    }

    private void PickNewWord()
    {
        List<string> pool = GetPoolForTier(currentTier);
        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning($"TypingSystem: word pool for tier {currentTier} is empty.", this);
            return;
        }

        currentWord = WeightedRandomWord(pool);
        typedIndex = 0;
        UpdateWordDisplay();
    }

    private List<string> GetPoolForTier(Tier tier)
    {
        switch (tier)
        {
            case Tier.Short: return shortWords;
            case Tier.Medium: return mediumWords;
            case Tier.Long: return longWords;
            default: return shortWords;
        }
    }

    private string WeightedRandomWord(List<string> pool)
    {
        List<string> candidates = pool.Count > 1
            ? pool.FindAll(w => w != currentWord)
            : pool;

        float[] weights = new float[candidates.Count];
        float totalWeight = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            float weight = 1f;
            foreach (char c in candidates[i].ToLowerInvariant())
            {
                if (mistakeCounts.TryGetValue(c, out int count))
                {
                    weight += count * adaptiveWeightPerMistake;
                }
            }
            weights[i] = weight;
            totalWeight += weight;
        }

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        for (int i = 0; i < candidates.Count; i++)
        {
            cumulative += weights[i];
            if (roll <= cumulative) return candidates[i];
        }

        return candidates[candidates.Count - 1];
    }

    private void UpdateWordDisplay()
    {
        if (wordDisplay == null) return;

        string typedPart = currentWord.Substring(0, typedIndex);
        string remainingPart = currentWord.Substring(typedIndex);

        wordDisplay.text = $"<color=#4CAF50>{typedPart}</color>{remainingPart}";
    }

    public void ResetSession()
    {
        currentTier = Tier.Short;
        tierProgress = 0;
        mistakeCounts.Clear();
        PickNewWord();
    }
}