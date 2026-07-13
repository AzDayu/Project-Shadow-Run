using System.Collections.Generic;
using UnityEngine;

public class ShopViewModel : ViewModelBase
{
    public List<ShopItemSlotViewModel> ShopItemSlotList { get; } = new List<ShopItemSlotViewModel>();
    public List<ShopItemSlotViewModel> InventoryItemSlotList { get; } = new List<ShopItemSlotViewModel>();
    public List<ShopItemSlotViewModel> StashItemSlotList { get; } = new List<ShopItemSlotViewModel>();

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(CurPlayerCredit));
        OnPropertyChanged(nameof(HoveredItem));
    }

    private int _curPlayerCredit;
    public int CurPlayerCredit
    {
        get => _curPlayerCredit;
        set
        {
            if (_curPlayerCredit != value)
            {
                _curPlayerCredit = value;
                OnPropertyChanged(nameof(CurPlayerCredit));
            }
        }
    }

    private ItemData _hoveredItem;
    public ItemData HoveredItem
    {
        get => _hoveredItem;
        set
        {
            if (_hoveredItem != value)
            {
                _hoveredItem = value;
                OnPropertyChanged(nameof(HoveredItem));
            }
        }
    }

    public ShopViewModel()
    {
        // 최초 10개씩 빈 슬롯 데이터 생성
        for (int i = 0; i < 10; i++)
        {
            ShopItemSlotList.Add(new ShopItemSlotViewModel { SlotIndex = i, SlotType = ShopItemSlotType.Shop });
            InventoryItemSlotList.Add(new ShopItemSlotViewModel { SlotIndex = i, SlotType = ShopItemSlotType.Inventory });
            StashItemSlotList.Add(new ShopItemSlotViewModel { SlotIndex = i, SlotType = ShopItemSlotType.Stash });
        }
    }

    public void OnSlotPointerEnter(ItemData itemData)
    {
        HoveredItem = itemData;
    }

    public void OnSlotPointerExit()
    {
        HoveredItem = null;
    }
}
