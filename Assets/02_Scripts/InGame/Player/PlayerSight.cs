using UnityEngine;

public class PlayerSight : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler InputHandler;
    [SerializeField] private Transform CameraTarget;

    [SerializeField] private float MouseSensitivity = 0.1f;
    [SerializeField] private float MinPitch = -80f;
    [SerializeField] private float MaxPitch = 80f;

    private float _pitch;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Look();
    }

    private void Look()
    {
        Vector2 lookInput = InputHandler.LookInput;

        float mouseX = lookInput.x * MouseSensitivity;
        float mouseY = lookInput.y * MouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, MinPitch, MaxPitch);

        CameraTarget.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }
}
