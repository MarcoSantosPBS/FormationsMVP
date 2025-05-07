using UnityEngine;

public class Squad : MonoBehaviour
{
    [SerializeField] public SquadController controller;
    [SerializeField] private GameObject debugEnemySquad;
    [SerializeField] public SquadFriendlyType type;

    private bool isEngaged;

    public int GetCollumns() => controller.Columns;
    public int GetLines() => controller.Lines;
    public void RemoveUnit(Unit unit) => controller.RemoveUnit(unit);
}
