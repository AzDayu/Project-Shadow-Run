using UnityEngine;

public class PlayerQuickSlotInputController : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler PlayerUsingInputHandler;

    private void OnEnable()
    {
        if (TestInputHandler != null)
            TestInputHandler.QuickSlotPerformed += OnQuickSlotPerformed;
    }
    private void OnDisable()
    {
        if (TestInputHandler != null)
            TestInputHandler.QuickSlotPerformed -= OnQuickSlotPerformed;
    }

    private void OnQuickSlotPerformed(int quickSlotIndex)
    {
        if (InventoryManager.Instance == null)
            return;

        InventoryManager.Instance.TrySelectQuickSlot(quickSlotIndex);
    }
}