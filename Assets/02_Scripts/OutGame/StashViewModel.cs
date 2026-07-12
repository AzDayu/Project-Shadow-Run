using System;
using System.Collections.Generic;
using UnityEngine;

public class StashViewModel : ViewModelBase, IContainerPropertyChanged<long>
{
    public event Action<string, ContainerEventType, long> ContainerPropertyChanged;

    public List<ShopItemSlotViewModel> InventoryItemSlotList { get; } = new List<ShopItemSlotViewModel>();
    public List<ShopItemSlotViewModel> StashItemSlotList { get; } = new List<ShopItemSlotViewModel>();

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(StashItemList));
        OnPropertyChanged(nameof(HoveredItem));
    }

    private Dictionary<long, StashItemSlotViewModel> _stashItemList = new Dictionary<long, StashItemSlotViewModel>();
    public Dictionary<long, StashItemSlotViewModel> StashItemList
    {
        get => _stashItemList;
        set
        {
            if (_stashItemList != value)
            {
                _stashItemList = value;
                OnPropertyChanged(nameof(StashItemList));
            }
        }
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

    public void AddItemSlotViewModel(StashItemSlotViewModel slotVm)
    {
        _stashItemList.Add(slotVm.ItemUniqueId, slotVm);
        ContainerPropertyChanged?.Invoke(nameof(StashItemList), ContainerEventType.Add, slotVm.ItemUniqueId);
    }

    public void RemoveItemSlotViewModel(long uniqueId)
    {
        if (_stashItemList.ContainsKey(uniqueId) == true)
        {
            _stashItemList.Remove(uniqueId);
        }

        ContainerPropertyChanged?.Invoke(nameof(StashItemList), ContainerEventType.Remove, uniqueId);
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
