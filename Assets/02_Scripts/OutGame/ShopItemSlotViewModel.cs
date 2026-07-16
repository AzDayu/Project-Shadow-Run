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
        OnPropertyChanged(nameof(ItemStackCount));
        OnPropertyChanged(nameof(ItemSellingPrice));
        OnPropertyChanged(nameof(IsSlotEmpty));
    }
    

    private string _itemUniqueId;
    public string ItemUniqueId
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

    private int _itemStackCount;
    public int ItemStackCount
    {
        get => _itemStackCount;
        set
        {
            if (_itemStackCount != value)
            {
                _itemStackCount = value;
                OnPropertyChanged(nameof(ItemStackCount));
            }
        }
    }

    private int _itemSellingPrice;
    public int ItemSellingPrice
    {
        get => _itemSellingPrice;
        set
        {
            if (_itemSellingPrice != value)
            {
                _itemSellingPrice = value;
                OnPropertyChanged(nameof(ItemSellingPrice));
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
}
