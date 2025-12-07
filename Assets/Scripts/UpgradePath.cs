using UnityEngine;

[CreateAssetMenu(menuName = "Troops/Upgrade Path")]
public class UpgradePath : ScriptableObject
{
    [Header("Path Info")]
    public string pathName;
    public Color pathColor = Color.green;

    [Header("Upgrades (Max 5)")]
    [SerializeField] private UpgradeData[] upgrades = new UpgradeData[5];

    public UpgradeData GetUpgrade(int tier)
    {
        if (tier < 0 || tier >= upgrades.Length) return null;
        return upgrades[tier];
    }

    public int GetMaxTier() => upgrades.Length;
}