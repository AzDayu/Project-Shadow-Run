using System.Collections.Generic;
using System.Xml;
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

    public void InitStashAndInventoryData()
    {
        var stashVm = GetStashViewModel();
        PlayerModel playerData = SaveManager.Instance.LoadPlayerData();

        foreach (var slotVm in stashVm.StashSlots)
        {
            slotVm.IsSlotEmpty = true;
        }

        for (int i = 0; i < playerData.StashItems.Count; i++)
        {
            if (i >= stashVm._maxStashSlot)
            {
                Debug.LogWarning("NetworkStashService: 보관함 최대 슬롯을 초과하는 데이터가 있습니다!");
                break;
            }

            var savedItem = playerData.StashItems[i];
            var targetSlot = stashVm.StashSlots[i];

            targetSlot.ItemUniqueId = savedItem.InstanceId;
            targetSlot.ItemDataId = savedItem.ItemId;
            targetSlot.ItemStackCount = savedItem.CurrentStackCount;
            targetSlot.IsSlotEmpty = false;
        }

        var inventoryItems = InventoryManager.Instance.ItemList;
        foreach (var slotVm in stashVm.InventorySlots) slotVm.IsSlotEmpty = true;

        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (i >= stashVm._maxInventorySlot) break;

            var savedItem = inventoryItems[i];
            var targetSlot = stashVm.InventorySlots[i];

            targetSlot.ItemUniqueId = savedItem.InstanceId;
            targetSlot.ItemDataId = savedItem.ItemId;
            targetSlot.ItemStackCount = savedItem.CurrentStackCount;
            targetSlot.IsSlotEmpty = false;
        }
    }

    public void SyncDataOnClose()
    {
        var stashVm = GetStashViewModel();
        PlayerModel playerData = SaveManager.Instance.LoadPlayerData();

        List<ItemModel> newStash = new List<ItemModel>();
        foreach (var slotVm in stashVm.StashSlots)
        {
            if (!slotVm.IsSlotEmpty)
            {
                newStash.Add(new ItemModel
                {
                    InstanceId = slotVm.ItemUniqueId,
                    ItemId = slotVm.ItemDataId,
                    CurrentStackCount = slotVm.ItemStackCount
                });
            }
        }

        playerData.StashItems = newStash;
        SaveManager.Instance.SavePlayerData(playerData);

        List<ItemModel> newInventory = new List<ItemModel>();
        foreach (var slotVm in stashVm.InventorySlots)
        {
            if (!slotVm.IsSlotEmpty)
            {
                newInventory.Add(new ItemModel
                {
                    InstanceId = slotVm.ItemUniqueId,
                    ItemId = slotVm.ItemDataId,
                    CurrentStackCount = slotVm.ItemStackCount
                });
            }
        }
        //InventoryManager.Instance.SyncInventoryFromUI(newInventory);
    }

    public void AddItem(string itemDataId, int addItemCount)
    {
        var stashVm = GetStashViewModel();
        string uniqueId = System.Guid.NewGuid().ToString();

        foreach(var slotVm in stashVm.StashSlots)
        {
            if (slotVm.IsSlotEmpty)
            {
                slotVm.ItemUniqueId = uniqueId;
                slotVm.ItemDataId = itemDataId;
                slotVm.ItemStackCount = addItemCount;
                slotVm.IsSlotEmpty = false;

                return;
            }
        }
    }

    private void RequestRemoveItem(string removeTargetUniqueId)
    {
        var stashVm = GetStashViewModel();

        foreach(var slotVm in stashVm.StashSlots)
        {
            if((slotVm.IsSlotEmpty == false) && (slotVm.ItemUniqueId == removeTargetUniqueId))
            {
                slotVm.ItemUniqueId = string.Empty;
                slotVm.ItemDataId = string.Empty;
                slotVm.ItemStackCount = 0;
                slotVm.IsSlotEmpty = true;

                return;
            }
        }
        //NetworkManager.Inst.SaveLoadService.RequstSaveData();
    }

}
