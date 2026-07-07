using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput _controls;

    public Vector2 MoveInput { get; private set; }

    private void Awake()
    {
        _controls = new PlayerInput();
    }

    private void OnEnable()
    {
        _controls.Enable();

        _controls.MoveMap.Move2D.performed += OnMove;
        _controls.MoveMap.Move2D.canceled += OnMoveCanceled;
    }

    private void OnDisable()
    {
        _controls.MoveMap.Move2D.performed -= OnMove;
        _controls.MoveMap.Move2D.canceled -= OnMoveCanceled;

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
}