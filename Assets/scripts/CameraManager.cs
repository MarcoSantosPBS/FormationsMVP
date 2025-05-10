using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private float cameraMovementSpeed;
    [SerializeField] private float cameraRotationSpeed;
    [SerializeField] private float cameraZoomSpeed;
    [SerializeField] private float cameraShiftSpeedMultiplayer;
    [SerializeField] private float followOffsetMaxValue;
    [SerializeField] private float followOffsetMinValue;
    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;

    private CinemachineTransposer cinemachineTransposer;
    private Vector3 targetFollowOffset;

    private void Start()
    {
        cinemachineTransposer = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        targetFollowOffset = cinemachineTransposer.m_FollowOffset;
    }

    private void Update()
    {
        bool isPressingShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        float actualCameraMoveSpeed = isPressingShift ? cameraMovementSpeed * cameraShiftSpeedMultiplayer : cameraMovementSpeed;
        float actualCameraRotationSpeed = isPressingShift ? cameraRotationSpeed * cameraShiftSpeedMultiplayer : cameraRotationSpeed;

        GetMovementInputVector(out Vector3 movementInputVector);
        Vector3 moveDirection = transform.forward * movementInputVector.z + transform.right * movementInputVector.x;
        transform.position += moveDirection * Time.deltaTime * actualCameraMoveSpeed;

        GetRotationVector(out Vector3 rotationInputVector);
        transform.eulerAngles += rotationInputVector * actualCameraRotationSpeed * Time.deltaTime;

        targetFollowOffset = GetZoomInputVector(targetFollowOffset);
        cinemachineTransposer.m_FollowOffset = Vector3.Lerp(cinemachineTransposer.m_FollowOffset, targetFollowOffset, Time.deltaTime * cameraZoomSpeed);
    }

    private Vector3 GetZoomInputVector(Vector3 followOffset)
    {
        if (Input.mouseScrollDelta.y > 0) { followOffset.y -= 1f; }
        if (Input.mouseScrollDelta.y < 0) { followOffset.y += 1f; }

        followOffset.y = Mathf.Clamp(followOffset.y, followOffsetMinValue, followOffsetMaxValue);
        return followOffset;
    }

    private static void GetRotationVector(out Vector3 rotationInputVector)
    {
        rotationInputVector = Vector3.zero;

        if (Input.GetKey(KeyCode.Q)) { rotationInputVector.y = -1f; }
        if (Input.GetKey(KeyCode.E)) { rotationInputVector.y = +1f; }
    }

    private static void GetMovementInputVector(out Vector3 movementInputVector)
    {
        movementInputVector = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) { movementInputVector.z = 1f; }
        if (Input.GetKey(KeyCode.S)) { movementInputVector.z = -1f; }
        if (Input.GetKey(KeyCode.A)) { movementInputVector.x = -1f; }
        if (Input.GetKey(KeyCode.D)) { movementInputVector.x = 1f; }
    }
}