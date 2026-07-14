using System.Collections.Generic;
using UnityEngine;

public class NetworkStashService
{
    private StashViewModel _stashViewModel;

    public StashViewModel GetStashViewModel()
    {
        if (_stashViewModel == null)
        {
            CreateStashViewModel();
        }

        return _stashViewModel;
    }

    private StashViewModel CreateStashViewModel()
    {
        var stashVm = new StashViewModel();
        _stashViewModel = stashVm;
        return stashVm;
    }

    private void RequestRemoveItem(long removeTargetUniqueId)
    {
        var stashVm = GetStashViewModel();
        stashVm.RemoveItemSlotViewModel(removeTargetUniqueId);

        //NetworkManager.Inst.SaveLoadService.RequstSaveData();
    }

    public Dictionary<long, StashItemSlotViewModel> GetStashItemList()
    {
        var stashVm = GetStashViewModel();
        return stashVm.StashItemList;
    }
}
