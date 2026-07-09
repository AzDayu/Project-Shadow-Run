using UnityEngine;

public class PlayerInventoryInputController : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler InputHandler;

    private void OnEnable()
    {
        if (InputHandler != null)
            InputHandler.InventoryPerformed += ToggleInventory;
    }

    private void OnDisable()
    {
        if (InputHandler != null)
            InputHandler.InventoryPerformed -= ToggleInventory;
    }

    private void ToggleInventory()
    {
        if (UIManager.Instance == null || InputHandler == null)
            return;

        UIManager.Instance.ToggleInventoryPopup();

        bool isInventoryOpened = UIManager.Instance.IsInventoryOpened();

        InputHandler.SetGameplayInputBlocked(isInventoryOpened);
    }
}