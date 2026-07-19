using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum EquipmentSlotType
{
    None,
    Head,
    Body
}

public class PlayerEquipmentSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Equipment Slot")]
    [SerializeField] private EquipmentSlotType SlotType;
    public EquipmentSlotType EquipmentType => SlotType;

    [Header("UI")]
    [SerializeField] private Image ImageItemIcon;

    [Header("Tooltip")]
    [SerializeField] private PlayerInventoryTooltipController ItemTooltip;

    private ItemModel ItemModelSlot
    {
        get
        {
            if (InventoryManager.Instance == null)
                return null;

            return InventoryManager.Instance.GetEquippedItem(SlotType);
        }
    }

    private bool HasItem => ItemModelSlot != null && ItemModelSlot.CurrentStackCount > 0;

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnEquipmentChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnEquipmentChanged -= Refresh;

        if (ItemTooltip != null)
            ItemTooltip.Hide();
    }

    private void Refresh()
    {
        if (!HasItem)
        {
            Clear();
            return;
        }

        ItemData itemData = DataManager.Instance.GetItemData(ItemModelSlot.ItemId);
        Sprite icon = ItemIconLoader.LoadIcon(itemData);

        if (ImageItemIcon != null)
        {
            ImageItemIcon.sprite = icon;
            ImageItemIcon.enabled = icon != null;
        }
    }

    private void Clear()
    {
        if (ImageItemIcon == null)
            return;

        ImageItemIcon.sprite = null;
        ImageItemIcon.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
            return;

        if (!HasItem || InventoryManager.Instance == null)
            return;

        InventoryManager.Instance.TryUnequipItem(SlotType);

        if (ItemTooltip != null)
            ItemTooltip.Hide();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!HasItem || ItemTooltip == null)
            return;

        ItemTooltip.Show(ItemModelSlot, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemTooltip != null)
            ItemTooltip.Hide();
    }
}