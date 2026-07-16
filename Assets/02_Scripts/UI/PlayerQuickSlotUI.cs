using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class ItemIconLoader
{
    private static readonly Dictionary<string, Sprite> _iconCache = new();

    public static Sprite LoadIcon(ItemData item)
    {
        if (item == null)
            return null;

        if (string.IsNullOrEmpty(item.IconPath))
            return null;

        if (_iconCache.TryGetValue(item.IconPath, out Sprite cachedSprite))
            return cachedSprite;

        Sprite sprite = Resources.Load<Sprite>(item.IconPath);

        if (sprite == null)
        {
            Debug.LogWarning($"아이콘 로드 실패: {item.IconPath}");
            return null;
        }

        _iconCache.Add(item.IconPath, sprite);
        return sprite;
    }
}

public class PlayerQuickSlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private int QuickSlotIndex;

    [Header("UI")]
    [SerializeField] private Image IconImage;
    [SerializeField] private TMP_Text TextCount;
    [SerializeField] private GameObject SelectedFrame;

    public ItemModel ItemModelSlot
    {
        get
        {
            if (InventoryManager.Instance == null)
                return null;

            IReadOnlyList<ItemModel> quickSlots = InventoryManager.Instance.QuickSlotList;

            if (QuickSlotIndex < 0 || (QuickSlotIndex >= quickSlots.Count))
                return null;

            return quickSlots[QuickSlotIndex];
        }
    }

    public bool HasItem => ItemModelSlot != null && ItemModelSlot.CurrentStackCount > 0;

    public int SlotIndex => QuickSlotIndex;

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnQuickSlotChanged += Refresh;
            InventoryManager.Instance.OnSelectedQuickSlotChanged += RefreshSelected;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnQuickSlotChanged -= Refresh;
            InventoryManager.Instance.OnSelectedQuickSlotChanged -= RefreshSelected;
        }
    }

    private void Refresh()
    {
        if (InventoryManager.Instance == null)
        {
            Clear();
            return;
        }

        IReadOnlyList<ItemModel> quickSlots = InventoryManager.Instance.QuickSlotList;

        if (QuickSlotIndex < 0 || QuickSlotIndex >= quickSlots.Count)
        {
            Clear();
            return;
        }

        ItemModel stack = quickSlots[QuickSlotIndex];

        if (stack == null ||  stack.CurrentStackCount <= 0)
        {
            Clear();
            return;
        }

        SetItem(stack);
        RefreshSelected();
    }

    private void SetItem(ItemModel stack)
    {
        if (IconImage != null)
        {
            Sprite icon = ItemIconLoader.LoadIcon(DataManager.Instance.GetItemData(stack.ItemId));

            IconImage.sprite = icon;
            IconImage.enabled = icon != null;
        }

        if (TextCount != null)
        {
            bool showCount = stack.CurrentStackCount > 1;

            TextCount.text = showCount ? stack.CurrentStackCount.ToString() : string.Empty;

            TextCount.enabled = showCount;
        }
    }

    private void Clear()
    {
        if (IconImage != null)
        {
            IconImage.sprite = null;
            IconImage.enabled = false;
        }

        if (TextCount != null)
        {
            TextCount.text = string.Empty;
            TextCount.enabled = false;
        }

        RefreshSelected();
    }

    private void RefreshSelected()
    {
        if (InventoryManager.Instance == null)
        {
            if (SelectedFrame != null)
                SelectedFrame.SetActive(false);

            return;
        }

        bool selected = InventoryManager.Instance.SelectedQuickSlotIndex == QuickSlotIndex;

        if (SelectedFrame != null)
            SelectedFrame.SetActive(selected);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
            return;

        if (!HasItem)
            return;

        if (InventoryManager.Instance == null)
            return;

        InventoryManager.Instance.TryUnregisterQuickSlot(QuickSlotIndex);
    }
}