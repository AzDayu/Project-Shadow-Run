using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput _controls;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }

    private void Awake()
    {
        _controls = new PlayerInput();
    }

    private void OnEnable()
    {
        _controls.Enable();

        _controls.InGame.Move2D.performed += OnMove;
        _controls.InGame.Move2D.canceled += OnMoveCanceled;

        _controls.InGame.Look.performed += OnLook;
        _controls.InGame.Look.canceled += OnLookCanceled;
    }

    private void OnDisable()
    {
        _controls.InGame.Move2D.performed -= OnMove;
        _controls.InGame.Move2D.canceled -= OnMoveCanceled;

        _controls.InGame.Look.performed -= OnLook;
        _controls.InGame.Look.canceled -= OnLookCanceled;

        _controls.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        MoveInput = Vector2.zero;
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        LookInput = context.ReadValue<Vector2>();
    }

    private void OnLookCanceled(InputAction.CallbackContext context)
    {
        LookInput = Vector2.zero;
    }
}