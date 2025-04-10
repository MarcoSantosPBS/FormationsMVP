using UnityEngine;

public class teste : MonoBehaviour
{
    [SerializeField] private SquadController squad;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.E))
        {
            squad.GetRandomUnit().TakeDamage(100000);
        }
    }
}
