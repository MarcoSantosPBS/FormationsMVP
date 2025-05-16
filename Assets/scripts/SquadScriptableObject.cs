using UnityEngine;

[CreateAssetMenu(fileName = "SquadSO", menuName = "ScriptableObjects/SquadScriptableObject")]
public class SquadScriptableObject : ScriptableObject
{
    public GameObject unitPrefab;
    public SquadFriendlyType type;
}
