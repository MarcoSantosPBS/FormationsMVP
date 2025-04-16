using UnityEngine;

public class teste : MonoBehaviour
{
    [SerializeField] private SquadController squad;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.E))
        {
            Unit unit = squad.GetRandomUnit();
            if (unit.isAlive) { unit.TakeDamage(100000); }
        }
    }
}
