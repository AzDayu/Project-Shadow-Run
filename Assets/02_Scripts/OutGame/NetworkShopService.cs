using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class NetworkShopService
{
    private ShopViewModel _shopViewModel;

    public ShopViewModel GetShopViewModel()
    {
        if (_shopViewModel == null)
        {
            CreateShopViewModel();
        }

        return _shopViewModel;
    }

    private ShopViewModel CreateShopViewModel()
    {
        var shopVm = new ShopViewModel();
        _shopViewModel = shopVm;
        return shopVm;
    }

    public void InitShopData()
    {
        var vm = GetShopViewModel();

        //Test용 하드코딩. 추후 별도의 테이블 작성 후 JSON 변환할것.
        SetShopItemSlot(vm.ShopItemSlotList[0], "Item_Medical_Kit_01", 99);
        SetShopItemSlot(vm.ShopItemSlotList[1], "Item_Ammo_556", 999);
        SetShopItemSlot(vm.ShopItemSlotList[2], "Item_Weapon_AR_01", 5);

        PlayerModel playerData = SaveManager.Instance.LoadPlayerData();
        vm.CurPlayerCredit = playerData.CurrentCredit;

        LoadPlayerItemsToShopZone(playerData.InventoryItems, vm.InventoryItemSlotList);
        LoadPlayerItemsToShopZone(playerData.StashItems, vm.StashItemSlotList);
    }

    private void SetShopItemSlot(ShopItemSlotViewModel slot, string dataId, int count)
    {
        var itemData = GameDataManager.Instance.GetItemDataById(dataId);

        slot.ItemUniqueId = string.Empty;
        slot.ItemDataId = itemData.Id;
        slot.ItemStackCount = count;
        slot.ItemSellingPrice = itemData.SellingPrice; // 구매가
        slot.IsSlotEmpty = false;
    }

    private void LoadPlayerItemsToShopZone(List<ItemModel> savedItems, List<ShopItemSlotViewModel> targetSlots)
    {
        foreach (var slot in targetSlots) slot.IsSlotEmpty = true;

        for (int i = 0; i < savedItems.Count; i++)
        {
            if (i >= targetSlots.Count) break;

            var savedItem = savedItems[i];
            var itemData = GameDataManager.Instance.GetItemDataById(savedItem.ItemId);

            targetSlots[i].ItemUniqueId = savedItem.InstanceId; // 유저 아이템은 유니크 ID 유지
            targetSlots[i].ItemDataId = savedItem.ItemId;
            targetSlots[i].ItemStackCount = savedItem.CurrentStackCount;
            targetSlots[i].ItemSellingPrice = itemData != null ? itemData.SellingPrice : 0; // 판매가
            targetSlots[i].IsSlotEmpty = false;
        }
    }

    //플레이어가 상점에서 아이템을 구매할 때 사용되는 함수
    public void RequestBuyItem(int shopSlotIndex)
    {
        var vm = GetShopViewModel();
        var targetSlot = vm.ShopItemSlotList[shopSlotIndex];

        if (targetSlot.IsSlotEmpty) return;

        int price = targetSlot.ItemSellingPrice;

        if (vm.CurPlayerCredit < price)
        {
            Debug.LogError("크레딧이 부족합니다.");
            return;
        }

        // 2. 인벤토리나 창고에 빈 공간이 있는지 찾기. 인벤토리, 창고 구현 후 주석 해제
        //ShopItemSlotViewModel emptySlot = vm.InventorySlots.Find(s => s.IsEmpty);
        //if (emptySlot == null)
        //{
        //    emptySlot = vm.StashItemSlotList.Find(s => s.IsSlotEmpty);
        //}

        //if (emptySlot == null)
        //{
        //    Debug.LogError("아이템을 넣을 공간이 없습니다.");
        //    return;
        //}

        // 3. 구매 처리
        //vm.CurPlayerCredit -= price;
        //long generatedUniqueId = System.DateTime.Now.Ticks; // 임시 유니크 ID 생성

        //emptySlot.SetItem(generatedUniqueId, targetSlot.ItemData, 1);

        Debug.Log($"구매 성공!");

    }

    //플레이어가 상점에 아이템을 판매할 때 사용되는 함수
    public void RequestSellItem(ShopItemSlotType fromZone, int slotIndex)
    {
        //var vm = GetShopViewModel();
        //List<ShopItemSlotViewModel> targetZone = (fromZone == ShopItemSlotType.Inventory) ? vm.InventorySlots : vm.StashSlots;

        //var slot = targetZone[slotIndex];
        //if (slot.IsSlotEmpty) return;

        //// 가격 지급 및 슬롯 초기화
        //vm.CurPlayerCredit += slot.ItemData.SellingPrice;
        //slot.Clear();

        Debug.Log("아이템 판매 성공!");
    }
}
