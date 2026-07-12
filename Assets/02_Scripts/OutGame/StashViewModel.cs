using System;
using System.Collections.Generic;
using UnityEngine;

public class StashViewModel : ViewModelBase, IContainerPropertyChanged<long>
{
    public event Action<string, ContainerEventType, long> ContainerPropertyChanged;

    private Dictionary<long, StashItemSlotViewModel> _itemList = new Dictionary<long, StashItemSlotViewModel>();
    public Dictionary<long, StashItemSlotViewModel> ItemList
    {
        get => _itemList;
        set
        {
            if (_itemList != value)
            {
                _itemList = value;
                OnPropertyChanged(nameof(ItemList));
            }
        }
    }

    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(ItemList));
    }

    public void AddItemSlotViewModel(StashItemSlotViewModel slotVm)
    {
        _itemList.Add(slotVm.ItemUniqueId, slotVm);
        ContainerPropertyChanged?.Invoke(nameof(ItemList), ContainerEventType.Add, slotVm.ItemUniqueId);
    }

    public void RemoveItemSlotViewModel(long uniqueId)
    {
        if (_itemList.ContainsKey(uniqueId) == true)
        {
            _itemList.Remove(uniqueId);
        }

        ContainerPropertyChanged?.Invoke(nameof(ItemList), ContainerEventType.Remove, uniqueId);
    }
}
