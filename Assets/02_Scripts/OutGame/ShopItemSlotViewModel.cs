using System.Collections.Generic;
using UnityEngine;

public class ShopItemSlotViewModel : ViewModelBase
{
    public int SlotIndex { get; set; }
    public ShopItemSlotType SlotType { get; set; }

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(ItemUniqueId));
        OnPropertyChanged(nameof(ItemDataId));
        OnPropertyChanged(nameof(ItemDataWithStack));
        OnPropertyChanged(nameof(ItemData));
        OnPropertyChanged(nameof(IsSlotEmpty));
    }

    private long _itemUniqueId;
    public long ItemUniqueId
    {
        get => _itemUniqueId;
        set
        {
            if (_itemUniqueId != value)
            {
                _itemUniqueId = value;
                OnPropertyChanged(nameof(ItemUniqueId));
            }
        }
    }

    private string _itemDataId;
    public string ItemDataId
    {
        get => _itemDataId;
        set
        {
            if (_itemDataId != value)
            {
                _itemDataId = value;
                OnPropertyChanged(nameof(ItemDataId));
            }
        }
    }

    private ItemStack _itemDataWithStack;
    public ItemStack ItemDataWithStack
    {
        get => _itemDataWithStack;
        set
        {
            if (_itemDataWithStack != value)
            {
                _itemDataWithStack = value;
                OnPropertyChanged(nameof(ItemDataWithStack));
            }
        }
    }

    private bool _isSlotEmpty = true;
    public bool IsSlotEmpty
    {
        get => _isSlotEmpty;
        set
        {
            if(_isSlotEmpty != value)
            {
                _isSlotEmpty = value;
                OnPropertyChanged(nameof(IsSlotEmpty));
            }
        }
    }

    public void Clear()
    {
        ItemUniqueId = 0;
        ItemDataId = string.Empty;
        ItemDataWithStack = null;
        IsSlotEmpty = true;
    }

    public void SetItem(long uniqueId, ItemStack itemData)
    {
        ItemUniqueId = uniqueId;
        ItemDataId = itemData.Item.ItemId;
        ItemDataWithStack.Item = itemData.Item;
        ItemDataWithStack.StackCount = itemData.StackCount;
        IsSlotEmpty = false;
    }
}
