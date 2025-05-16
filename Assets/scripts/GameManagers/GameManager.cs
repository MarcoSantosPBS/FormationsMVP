using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private LayerMask lineMask;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            LineSpawner line = MouseWorld.Instance.GetTInMousePosition<LineSpawner>(lineMask);

            if (line == null) 
                return;

            Debug.Log(line.GetAllySpawner());
        }
    }

}
