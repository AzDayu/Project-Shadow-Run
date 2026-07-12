using System.Collections.Generic;
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

    // 상점의 판매 상품 리스트를 불러오고 해당 아이템들을 슬롯에 넣어주는 함수. 개수 무한버전.
    public void SetShopItem(int slotIndex, ItemData itemData)
    {
        var shopVm = GetShopViewModel();
        if ((slotIndex >= 0) && slotIndex < shopVm.ShopItemSlotList.Count)
        {
            shopVm.ShopItemSlotList[slotIndex].SetItem(itemData);
        }
    }

    //플레이어가 상점에서 아이템을 구매할 때 사용되는 함수
    public void BuyShopItem(int shopSlotIndex)
    {
        var vm = GetShopViewModel();
        var targetSlot = vm.ShopItemSlotList[shopSlotIndex];

        if (targetSlot.IsSlotEmpty) return;

        int price = targetSlot.ItemData.SellingPrice;

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

        Debug.Log($"{targetSlot.ItemData.ItemName} 구매 성공!");

    }

    //플레이어가 상점에 아이템을 판매할 때 사용되는 함수
    public void SellItemToShop(ShopItemSlotType fromZone, int slotIndex)
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
