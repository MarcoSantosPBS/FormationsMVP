using UnityEngine;

public class Squad : MonoBehaviour
{
    [SerializeField] private SquadController controller;
    [SerializeField] private GameObject debugEnemySquad;
    [SerializeField] public SquadFriendlyType type;

    private bool isEngaged;

    private void Start()
    {
        controller.GenerateUnits();
        isEngaged = false;
    }

    private void Update()
    {
        bool hasCollided = UnitCollider.Instance.CheckCollision(controller.units, HandleCollision, true);

        if (!hasCollided) Desengage();

        controller.UpdateUnitsPositions();

        if (debugEnemySquad)
        {
            controller.RotateToEnemySquad(debugEnemySquad);
        }
    }

    private void HandleCollision()
    {
        isEngaged = true;
        UnitCollider.Instance.GetTarget(controller.units);
    }

    public void Desengage()
    {
        if (!isEngaged) return;

        foreach (Unit unit in controller.units)
        {
            unit.SetTargetUnit(null);
        }

        isEngaged = false;
        Debug.Log("Desengajou");
    }

    public void RemoveUnit(Unit unit) => controller.RemoveUnit(unit);
}
