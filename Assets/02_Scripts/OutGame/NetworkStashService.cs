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

    public void AddItem(string itemDataId, int addItemCount)
    {
        // 저장할때 고유값 ID를 부여하기 위해 사용
        long uniqueId = GameUtil.GenerateUniqueId();

        // TODO : 우선 쉽게 사용할 수 있도록 중복 처리는 빼두었다. 습득할때마다 아이템이 하나씩 추가되도록 해두고
        // 추후에 중복값은 StackCount가 다 찰때까지 누적해줄 수 있도록 로직을 추가하자
        var newItemVm = new StashItemSlotViewModel();
        newItemVm.ItemUniqueId = uniqueId;
        newItemVm.ItemDataId = itemDataId;
        newItemVm.ItemStackCount = addItemCount;

        var stashVm = GetStashViewModel();
        stashVm.AddItemSlotViewModel(newItemVm);

        //NetworkManager.Inst.SaveLoadService.RequstSaveData();
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
