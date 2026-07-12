using UnityEngine;

public class StashItemSlotViewModel : ViewModelBase //상점 아이템 슬롯에서 가격 없는 버전.
{
    public int SlotIndex { get; set; }
    public ShopItemSlotType SlotType { get; set; }

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(ItemUniqueId));
        OnPropertyChanged(nameof(ItemDataId));
        OnPropertyChanged(nameof(ItemStackCount));
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

    private ItemData _itemData;
    public ItemData ItemData
    {
        get => _itemData;
        set
        {
            if (_itemData != value)
            {
                _itemData = value;
                OnPropertyChanged(nameof(ItemData));
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
        ItemStackCount = 0;
        ItemData = null;
        IsSlotEmpty = true;
    }

    //개수 제한 있을 때
    public void SetItem(ItemData data, int count)
    {
        ItemDataId = data.ItemId;
        ItemData = data;
        ItemStackCount = count;
        IsSlotEmpty = false;
    }

    //판매 아이템 개수 제한 없을 때. 유니크 아이디는 인벤토리에 들어갈 때 추가하거나 할 것.
    public void SetItem(ItemData data)
    {
        ItemDataId = data.ItemId;
        ItemData = data;
        ItemStackCount = 9999; //UI에서는 ItemStackCount가 9999면 숫자가 안보이게, 서비스에서는 줄어들지 않게 변경하면 될듯. 더 좋은 방법 연구중.
        IsSlotEmpty = false;
    }
}
