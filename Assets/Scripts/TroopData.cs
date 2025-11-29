using UnityEngine;

[CreateAssetMenu(menuName = "Troops/Troop Data")]

[System.Serializable]
public class TroopData : ScriptableObject
{
    public string troopName;
    public GameObject prefab;
    public int cost;
    public Sprite icon;
}
