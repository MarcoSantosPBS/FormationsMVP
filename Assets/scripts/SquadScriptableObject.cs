using UnityEngine;

[CreateAssetMenu(fileName = "SquadSO", menuName = "ScriptableObjects/SquadScriptableObject")]
public class SquadScriptableObject : ScriptableObject
{
    [SerializeField] public GameObject unitPrefab;
    [SerializeField] public Factions Faction;
    [SerializeField] public int Columns = 8;
    [SerializeField] public int Lines = 5;
    [SerializeField] public int UnitSpacing = 1;
}
