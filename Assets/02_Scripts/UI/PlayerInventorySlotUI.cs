using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class PlayerInventorySlotUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("UI")]
    [SerializeField] private Image ImageItemIcon;
    [SerializeField] private TMP_Text TextItemCount;
    [SerializeField] private GameObject SelectedFrame;

    private PlayerInventoryPopUpUI _owner;
    private int _slotIndex;
    private ItemStack _itemStack;

    public int SlotIndex => _slotIndex;
    public ItemStack ItemStack => _itemStack;

    public bool HasItem => _itemStack != null && _itemStack.Item != null && _itemStack.StackCount > 0;

    public void Init(int slotIndex, PlayerInventoryPopUpUI owner)
    {
        _slotIndex = slotIndex;
        _owner = owner;

        Clear();
    }

    public void SetItem(ItemStack itemStack)
    {
        _itemStack = itemStack;
        Refresh();
    }

    public void Clear()
    {
        _itemStack = null;
        Refresh();
        SetSelected(false);
    }

    private void Refresh()
    {
        if (!HasItem)
        {
            if (ImageItemIcon != null)
            {
                ImageItemIcon.sprite = null;
                ImageItemIcon.enabled = false;
            }

            if (TextItemCount != null)
            {
                TextItemCount.text = string.Empty;
                TextItemCount.enabled = false;
            }

            return;
        }

        if (ImageItemIcon != null)
        {
            Sprite iconSprite = Resources.Load<Sprite>(_itemStack.Item.IconPath);

            ImageItemIcon.sprite = iconSprite;
            ImageItemIcon.enabled = iconSprite != null;
        }

        if (TextItemCount != null)
        {
            bool showCount = _itemStack.StackCount > 1;

            TextItemCount.text = showCount ? _itemStack.StackCount.ToString() : string.Empty;
            TextItemCount.enabled = showCount;
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (SelectedFrame != null)
            SelectedFrame.SetActive(isSelected);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!HasItem)
            return;

        _owner.ShowItemTooltip(this, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _owner.HideItemTooltip(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!HasItem)
            return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (eventData.clickCount >= 2)
            {
                _owner.TryUseItem(this);
            }
            else
            {
                _owner.SelectSlot(this);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            _owner.SelectSlot(this);
            _owner.OpenContextMenu(this, eventData.position);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!HasItem)
            return;

        _owner.BeginDragSlot(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!HasItem)
            return;

        _owner.DragSlot(this, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!HasItem)
            return;

        _owner.EndDragSlot(this, eventData);
    }
}