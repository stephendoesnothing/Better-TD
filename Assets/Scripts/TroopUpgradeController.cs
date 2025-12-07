using UnityEngine;
using System.Collections.Generic;

public class TroopUpgradeController : MonoBehaviour
{
    [Header("Upgrade Paths")]
    [SerializeField] private UpgradePath path1;
    [SerializeField] private UpgradePath path2;
    [SerializeField] private UpgradePath path3;

    [Header("Crosspath Settings")]
    [SerializeField] private int maxCrosspathTier = 2; // BTD6 style: max tier 2 on crosspath

    // Current upgrade tiers (0-5 for each path)
    private int[] currentTiers = new int[3];

    // Track which path is the main path (highest tier)
    private int mainPathIndex = -1;

    private Troop troopScript;
    private TroopData troopData;

    private void Awake()
    {
        troopScript = GetComponent<Troop>();
    }

    public void Initialize(TroopData data)
    {
        troopData = data;
        currentTiers = new int[3];
        mainPathIndex = -1;
    }

    public bool CanUpgrade(int pathIndex)
    {
        if (pathIndex < 0 || pathIndex > 2) return false;

        UpgradePath path = GetPath(pathIndex);
        if (path == null) return false;

        int currentTier = currentTiers[pathIndex];

        // Check if already at max tier
        if (currentTier >= 5) return false;

        // Check if upgrade exists
        UpgradeData upgrade = path.GetUpgrade(currentTier);
        if (upgrade == null) return false;

        // Check crosspath rules
        if (!IsCrosspathValid(pathIndex))
        {
            return false;
        }

        // Check if player can afford
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null && gm.CurrentCash < upgrade.cost)
        {
            return false;
        }

        return true;
    }

    private bool IsCrosspathValid(int pathIndex)
    {
        // First upgrade in any path is always valid
        if (currentTiers[pathIndex] == 0)
        {
            return true;
        }

        // If this is the first path being upgraded
        if (mainPathIndex == -1)
        {
            return true;
        }

        // If this is the main path, always allow
        if (pathIndex == mainPathIndex)
        {
            return true;
        }

        // Check if this would exceed crosspath limit
        int newTier = currentTiers[pathIndex] + 1;
        if (newTier > maxCrosspathTier)
        {
            return false;
        }

        // Check if another crosspath already exists at tier 2
        for (int i = 0; i < 3; i++)
        {
            if (i != pathIndex && i != mainPathIndex && currentTiers[i] >= maxCrosspathTier)
            {
                return false;
            }
        }

        return true;
    }

    public void PurchaseUpgrade(int pathIndex)
    {
        if (!CanUpgrade(pathIndex)) return;

        UpgradePath path = GetPath(pathIndex);
        UpgradeData upgrade = path.GetUpgrade(currentTiers[pathIndex]);

        // Deduct cost
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.SpendCash(upgrade.cost);
        }

        // Apply upgrade
        ApplyUpgrade(upgrade);

        // Increment tier
        currentTiers[pathIndex]++;

        // Update main path
        UpdateMainPath();

        Debug.Log($"Upgraded {troopData.troopName} - Path {pathIndex + 1} to Tier {currentTiers[pathIndex]}");
    }

    private void UpdateMainPath()
    {
        int highestTier = 0;
        int highestIndex = -1;

        for (int i = 0; i < 3; i++)
        {
            if (currentTiers[i] > highestTier)
            {
                highestTier = currentTiers[i];
                highestIndex = i;
            }
        }

        mainPathIndex = highestIndex;
    }

    private void ApplyUpgrade(UpgradeData upgrade)
    {
        if (troopScript == null || upgrade.statModifications == null) return;

        foreach (StatModification mod in upgrade.statModifications)
        {
            ApplyStatModification(mod);
        }
    }

    private void ApplyStatModification(StatModification mod)
    {
        switch (mod.statType)
        {
            case StatType.Damage:
                float currentDamage = troopScript.GetDamage();
                troopScript.SetDamage(CalculateNewValue(currentDamage, mod));
                break;

            case StatType.FireRate:
                float currentFireRate = troopScript.GetFireRate();
                troopScript.SetFireRate(CalculateNewValue(currentFireRate, mod));
                break;

            case StatType.Range:
                float currentRange = troopScript.GetRange();
                troopScript.SetRange(CalculateNewValue(currentRange, mod));
                break;
        }
    }

    private float CalculateNewValue(float currentValue, StatModification mod)
    {
        switch (mod.modificationType)
        {
            case ModificationType.Add:
                return currentValue + mod.value;
            case ModificationType.Multiply:
                return currentValue * mod.value;
            case ModificationType.Set:
                return mod.value;
            default:
                return currentValue;
        }
    }

    // Getters
    public UpgradePath GetPath(int index)
    {
        return index switch
        {
            0 => path1,
            1 => path2,
            2 => path3,
            _ => null
        };
    }

    public int GetCurrentTier(int pathIndex)
    {
        if (pathIndex < 0 || pathIndex > 2) return 0;
        return currentTiers[pathIndex];
    }

    public int GetMainPathIndex() => mainPathIndex;

    public int GetTotalSpent()
    {
        int total = 0;

        for (int pathIndex = 0; pathIndex < 3; pathIndex++)
        {
            UpgradePath path = GetPath(pathIndex);
            if (path == null) continue;

            for (int tier = 0; tier < currentTiers[pathIndex]; tier++)
            {
                UpgradeData upgrade = path.GetUpgrade(tier);
                if (upgrade != null)
                {
                    total += upgrade.cost;
                }
            }
        }

        return total;
    }

    public int GetSellValue()
    {
        // Make sure troopData is not null
        if (troopData == null) return 0;

        int totalSpent = troopData.cost + GetTotalSpent();
        return Mathf.RoundToInt(totalSpent * 0.7f); // 70% sell value
    }
}