using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput _controls;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public event Action JumpPerformed;
    public event Action FirePerformed;

    public event Action InventoryPerformed;
    public bool IsGameplayInputBlocked { get; private set; }

    public event Action<int> QuickSlotPerformed;

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

        _controls.InGame.Jump.performed += OnJump;

        _controls.InGame.Fire.performed += OnFire;

        _controls.InGame.Inventory.performed += OnInventory;

        _controls.InGame.QuickSlot1.performed += OnQuickSlot1;
        _controls.InGame.QuickSlot2.performed += OnQuickSlot2;
        _controls.InGame.QuickSlot3.performed += OnQuickSlot3;
    }

    private void OnDisable()
    {
        _controls.InGame.Move2D.performed -= OnMove;
        _controls.InGame.Move2D.canceled -= OnMoveCanceled;

        _controls.InGame.Look.performed -= OnLook;
        _controls.InGame.Look.canceled -= OnLookCanceled;

        _controls.InGame.Jump.performed -= OnJump;

        _controls.InGame.Fire.performed -= OnFire;

        _controls.InGame.Inventory.performed -= OnInventory;

        _controls.InGame.QuickSlot1.performed -= OnQuickSlot1;
        _controls.InGame.QuickSlot2.performed -= OnQuickSlot2;
        _controls.InGame.QuickSlot3.performed -= OnQuickSlot3;

        _controls.Disable();
    }

    public void SetGameplayInputBlocked(bool isBlocked)
    {
        IsGameplayInputBlocked = isBlocked;

        if (isBlocked)
        {
            MoveInput = Vector2.zero;
            LookInput = Vector2.zero;
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (IsGameplayInputBlocked)
        {
            MoveInput = Vector2.zero;
            return;
        }

        MoveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        MoveInput = Vector2.zero;
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        if (IsGameplayInputBlocked)
        {
            LookInput = Vector2.zero;
            return;
        }

        LookInput = context.ReadValue<Vector2>();
    }

    private void OnLookCanceled(InputAction.CallbackContext context)
    {
        LookInput = Vector2.zero;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (IsGameplayInputBlocked)
            return;

        JumpPerformed?.Invoke();
    }

    private void OnInventory(InputAction.CallbackContext context)
    {
        InventoryPerformed?.Invoke();
    }

    private void OnFire(InputAction.CallbackContext context)
    {
        if (IsGameplayInputBlocked)
            return;

        FirePerformed?.Invoke();
    }

    private void OnQuickSlot1(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (IsGameplayInputBlocked)
            return;

        QuickSlotPerformed?.Invoke(0);
    }

    private void OnQuickSlot2(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (IsGameplayInputBlocked)
            return;

        QuickSlotPerformed?.Invoke(1);
    }

    private void OnQuickSlot3(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (IsGameplayInputBlocked)
            return;

        QuickSlotPerformed?.Invoke(2);
    }
}