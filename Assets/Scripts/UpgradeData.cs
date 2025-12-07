using UnityEngine;

[CreateAssetMenu(menuName = "Troops/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Upgrade Info")]
    public string upgradeName;
    [TextArea(2, 4)]
    public string description;
    public int cost;
    public Sprite icon;

    [Header("Stat Modifications")]
    public StatModification[] statModifications;
}

[System.Serializable]
public class StatModification
{
    public StatType statType;
    public ModificationType modificationType;
    public float value;
}

public enum StatType
{
    Damage,
    FireRate,
    Range,
    Pierce,      // For future use
    ProjectileSpeed // For future use
}

public enum ModificationType
{
    Add,         // Adds flat value
    Multiply,    // Multiplies by value (1.5 = +50%)
    Set          // Sets to exact value
}