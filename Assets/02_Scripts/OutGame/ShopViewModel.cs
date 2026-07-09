using System.Collections.Generic;
using UnityEngine;

public class ShopViewModel : ViewModelBase
{
    private Dictionary<long, ShopItemSlotViewModel> _itemList = new Dictionary<long, ShopItemSlotViewModel>();

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

    public void OnSlotPointerEnter(ItemData itemData)
    {
        HoveredItem = itemData;
    }

    public void OnSlotPointerExit()
    {
        HoveredItem = null;
    }
}
