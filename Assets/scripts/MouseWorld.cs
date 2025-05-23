using UnityEngine;
using UnityEngine.EventSystems;

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

    public T GetTInMousePosition<T>(LayerMask layerMask) where T : MonoBehaviour
    {
        bool hasHit = Physics.Raycast(GetCameraRay(), out RaycastHit hitInfo, float.MaxValue, layerMask);

        if (hasHit)
            return hitInfo.collider.GetComponent<T>();

        return null;
    }

    public bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
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
