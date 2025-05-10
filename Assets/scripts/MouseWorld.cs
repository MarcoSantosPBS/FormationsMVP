using UnityEngine;

public class MouseWorld : MonoBehaviour
{
    private Camera _main;
    public static MouseWorld Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }

        Instance = this;
    }

    void Start()
    {
        _main = Camera.main;
    }

    public Vector3 GetMousePosition()
    {
        Physics.Raycast(GetCameraRay(), out RaycastHit hitInfo);
        return hitInfo.point;
    }

    private Ray GetCameraRay()
    {
        return _main.ScreenPointToRay(Input.mousePosition);
    }
}
