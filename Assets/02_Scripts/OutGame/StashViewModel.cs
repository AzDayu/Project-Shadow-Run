using System;
using System.Collections.Generic;
using UnityEngine;

public class StashViewModel : ViewModelBase, IContainerPropertyChanged<long>
{
    public event Action<string, ContainerEventType, long> ContainerPropertyChanged;

    public List<StashItemSlotViewModel> InventoryItemSlotList { get; } = new List<StashItemSlotViewModel>();
    public List<StashItemSlotViewModel> StashItemSlotList { get; } = new List<StashItemSlotViewModel>();

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(StashItemList));
        OnPropertyChanged(nameof(HoveredItemId));
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

    private string _hoveredItemId;
    public string HoveredItemId
    {
        get => _hoveredItemId;
        set
        {
            if (_hoveredItemId != value)
            {
                _hoveredItemId = value;
                OnPropertyChanged(nameof(HoveredItemId));
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

    public void OnSlotPointerEnter(string itemDataId)
    {
        HoveredItemId = itemDataId;
    }

    public void OnSlotPointerExit()
    {
        HoveredItemId = null;
    }
}
