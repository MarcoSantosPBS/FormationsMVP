using UnityEngine;

public class teste : MonoBehaviour
{
    [SerializeField] private Squad squad;
    [SerializeField] private Squad enemySquad;
    [SerializeField] private GameObject pivo;

    [SerializeField] public int newLines;
    [SerializeField] public int newColumns;

    public bool updaetCom;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            squad.controller.UpdatePositionToCombat(enemySquad, pivo.transform.position);
        }
    }
}
