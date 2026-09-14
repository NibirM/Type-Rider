using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LiveryManager : MonoBehaviour
{
    [Header("References")]
    public LapManager lapManager;
    public Renderer carRenderer;
    public TextMeshProUGUI tierResultText;

    [Header("Material Slot Exclusions")]
    public int[] excludedMaterialIndices = new int[0];
    public List<string> excludedMaterialNames = new List<string>();

    [Header("Time Thresholds (seconds)")]
    public float goldTimeThreshold = 150f;
    public float silverTimeThreshold = 180f;
    public float bronzeTimeThreshold = 210f;

    [Header("Car Colors")]
    public Color goldColor = new Color(1f, 0.84f, 0f);
    public Color silverColor = new Color(0.75f, 0.75f, 0.78f);
    public Color bronzeColor = new Color(0.8f, 0.5f, 0.2f);
    public Color defaultColor = Color.white;

    [Header("Result Text Colors")]
    public Color goldTextColor = new Color(1f, 0.84f, 0f);
    public Color silverTextColor = new Color(0.75f, 0.75f, 0.78f);
    public Color bronzeTextColor = new Color(0.8f, 0.5f, 0.2f);
    public Color defaultTextColor = Color.white;

    private void Awake()
    {
        if (tierResultText != null)
        {
            tierResultText.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (lapManager != null)
        {
            lapManager.onRaceFinished.AddListener(AssignTier);
        }
        else
        {
            Debug.LogWarning(
                "LiveryManager: LapManager reference not assigned — tiers won't trigger.",
                this
            );
        }
    }

    private void OnDisable()
    {
        if (lapManager != null)
        {
            lapManager.onRaceFinished.RemoveListener(AssignTier);
        }
    }

    public void ResetLivery()
    {
        if (tierResultText != null)
        {
            tierResultText.gameObject.SetActive(false);
        }
    }

    public void AssignTier(float finalTime)
    {
        Color carColor;
        Color textColor;
        string tierName;

        if (finalTime <= goldTimeThreshold)
        {
            carColor = goldColor;
            textColor = goldTextColor;
            tierName = "GOLD";
        }
        else if (finalTime <= silverTimeThreshold)
        {
            carColor = silverColor;
            textColor = silverTextColor;
            tierName = "SILVER";
        }
        else if (finalTime <= bronzeTimeThreshold)
        {
            carColor = bronzeColor;
            textColor = bronzeTextColor;
            tierName = "BRONZE";
        }
        else
        {
            carColor = defaultColor;
            textColor = defaultTextColor;
            tierName = "Finish!";
        }

        ApplyCarColor(carColor);

        if (tierResultText != null)
        {
            tierResultText.gameObject.SetActive(true);
            tierResultText.text = tierName;
            tierResultText.color = textColor;
        }

        Debug.Log($"LiveryManager: finished in {finalTime:0.00}s -> {tierName}");
    }

    private void ApplyCarColor(Color color)
    {
        if (carRenderer == null)
        {
            Debug.LogWarning(
                "LiveryManager: CarRenderer not assigned, can't apply car color.",
                this
            );

            return;
        }

        Material[] mats = carRenderer.materials;
        bool appliedToAny = false;

        for (int i = 0; i < mats.Length; i++)
        {
            Material mat = mats[i];

            if (System.Array.IndexOf(excludedMaterialIndices, i) >= 0)
            {
                continue;
            }

            bool excludedByName = false;
            foreach (string excludedName in excludedMaterialNames)
            {
                if (!string.IsNullOrEmpty(excludedName) &&
                    mat.name.IndexOf(excludedName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    excludedByName = true;
                    break;
                }
            }

            if (excludedByName)
            {
                continue;
            }

            bool appliedToThis = false;

            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
                appliedToThis = true;
            }

            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", color);
                appliedToThis = true;
            }

            if (appliedToThis)
            {
                appliedToAny = true;
            }
            else
            {
                Debug.LogWarning(
                    "LiveryManager: material '" + mat.name +
                    "' (slot " + i + ") on '" + carRenderer.gameObject.name +
                    "' uses shader '" + mat.shader.name +
                    "' which has neither _BaseColor nor _Color. " +
                    "Open that shader/material in the Inspector and tell me " +
                    "the actual color property name shown there.",
                    this
                );
            }
        }

        if (!appliedToAny)
        {
            Debug.LogWarning(
                "LiveryManager: could not apply color to ANY material slot on '" +
                carRenderer.gameObject.name + "'.", this
            );
        }
    }
}