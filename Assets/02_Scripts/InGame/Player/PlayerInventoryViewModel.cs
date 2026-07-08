using System.Collections.Generic;
using UnityEngine;

public class HadItemSlotData
{
    public ItemStack ItemCountAndStack;
    public WeaponSlotRuntimeData WeaponData;
}

public class WeaponSlotRuntimeData
{
    public List<string> PartList = new();
    public string EquippedAmmoId;
    public int CurrentMagazineAmmo;
}

public class InventoryModel
{
    private int _maxInvenCount = 30;
    public List<HadItemSlotData> HadItemSlotDataList { get; private set; } = new();

    public bool GetItem(string getItemDataId, int getItemCount)
    {
        if (string.IsNullOrEmpty(getItemDataId))
            return false;

        if (getItemCount <= 0)
            return false;

        ItemData getItemData = GameDataManager.Instance.GetItemDataById(getItemDataId);

        if (getItemData == null)
            return false;

        int remainCount = getItemCount;
        int maxStackSize = getItemData.MaxStackSize;

        if (maxStackSize <= 0)
            maxStackSize = 1;

        bool isStackable = maxStackSize > 1;

        if (isStackable)
        {
            foreach (HadItemSlotData slot in HadItemSlotDataList)
            {
                if (slot == null)
                    continue;

                if (slot.ItemCountAndStack == null)
                    continue;

                if (slot.ItemCountAndStack.Item == null)
                    continue;

                if (slot.ItemCountAndStack.Item.ItemId != getItemData.ItemId)
                    continue;

                if (slot.ItemCountAndStack.StackCount >= maxStackSize)
                    continue;

                int canAddCount = maxStackSize - slot.ItemCountAndStack.StackCount;
                int addCount = Mathf.Min(canAddCount, remainCount);

                slot.ItemCountAndStack.StackCount += addCount;
                remainCount -= addCount;

                if (remainCount <= 0)
                    return true;
            }
        }

        while (remainCount > 0)
        {
            if (HadItemSlotDataList.Count >= _maxInvenCount)
            {
                // TODO: 못 들어간 개수(remainCount)를 처리하는 로직 필요
                //       바닥에 버린다던가, UI를 띄우는 등의 처리 필요
                return false;
            }

            int addCount = isStackable
                ? Mathf.Min(maxStackSize, remainCount)
                : 1;

            HadItemSlotData newSlot = new HadItemSlotData
            {
                ItemCountAndStack = new ItemStack
                {
                    Item = getItemData,
                    StackCount = addCount
                },
                WeaponData = CreateWeaponDataIfNeeded(getItemData)
            };

            HadItemSlotDataList.Add(newSlot);

            remainCount -= addCount;
        }

        return true;
    }

    private WeaponSlotRuntimeData CreateWeaponDataIfNeeded(ItemData itemData)
    {
        if (itemData.ItemType != "Weapon")
            return null;

        return new WeaponSlotRuntimeData
        {
            PartList = new List<string>(),
            EquippedAmmoId = "",
            CurrentMagazineAmmo = 0
        };
    }
}

public class PlayerInventoryViewModel : ViewModelBase
{
    private InventoryModel _inventoryModel;

    public IReadOnlyList<HadItemSlotData> Slots => _inventoryModel.HadItemSlotDataList;

    public PlayerInventoryViewModel(InventoryModel inventoryModel)
    {
        _inventoryModel = inventoryModel;
    }

    public void AddItem(string itemDataId, int itemCount)
    {
        _inventoryModel.GetItem(itemDataId, itemCount);

        OnPropertyChanged(nameof(Slots));
    }
}
