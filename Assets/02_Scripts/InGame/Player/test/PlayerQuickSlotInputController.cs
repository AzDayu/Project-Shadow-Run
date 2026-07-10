using UnityEngine;

public class PlayerQuickSlotInputController : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler _playerInputHandler;

    private void OnEnable()
    {
        if (_playerInputHandler != null)
        {
            _playerInputHandler.QuickSlotPerformed += OnQuickSlotPerformed;
            _playerInputHandler.FirePerformed += OnFirePerformed;
        }
    }

    private void OnDisable()
    {
        if (_playerInputHandler != null)
        {
            _playerInputHandler.QuickSlotPerformed -= OnQuickSlotPerformed;
            _playerInputHandler.FirePerformed -= OnFirePerformed;
        }
    }

    private void OnQuickSlotPerformed(int quickSlotIndex)
    {
        if (InventoryManager.Instance == null)
            return;

        InventoryManager.Instance.TrySelectQuickSlot(quickSlotIndex);
    }

    private void OnFirePerformed()
    {
        if (InventoryManager.Instance == null)
            return;

        InventoryManager.Instance.TryUseSelectedQuickSlotItem();
    }
}