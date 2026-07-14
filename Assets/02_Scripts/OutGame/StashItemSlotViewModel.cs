using UnityEngine;

public class StashItemSlotViewModel : ViewModelBase 
{
    public int SlotIndex { get; set; }
    public ShopItemSlotType SlotType { get; set; }

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(ItemUniqueId));
        OnPropertyChanged(nameof(ItemDataId));
        OnPropertyChanged(nameof(ItemDataWithStack));
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
            if (_isSlotEmpty != value)
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

    public void SetItem(long uniqueId, ItemStack itemData) //일단은 유니크 아이디를 사용해서 개별적인 아이템의 구분을 하도록 로직을 작성했으나 이게 어떻게 될런지.
    {
        ItemUniqueId = uniqueId;
        ItemDataId = itemData.Item.Id;
        ItemDataWithStack.Item = itemData.Item;
        ItemDataWithStack.StackCount = itemData.StackCount;
        IsSlotEmpty = false;
    }
}
