using UnityEngine;

public class teste : MonoBehaviour
{
    [SerializeField] private SquadController squad;

    [SerializeField] public int newLines;
    [SerializeField] public int newColumns;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.E))
        {
            Unit unit = squad.GetRandomUnit();
            if (unit.isAlive) { unit.TakeDamage(100000); }
        }

        if (Input.GetKeyUp(KeyCode.S))
        {
            squad.UpdateFormationSize(newLines, newColumns);
        }
    }
}
