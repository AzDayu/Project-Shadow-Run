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

    // 상점의 판매 상품 리스트를 불러오고 해당 아이템들을 슬롯에 넣어주는 함수
    public void SetShopItem(string itemDataId)
    {
        var newItem = new ShopItemSlotViewModel();
    }

    //플레이어가 상점에서 아이템을 구매할 때 사용되는 함수
    public void BuyShopItem(string itemDataId)
    {
        //플레이어가 구매를 시도했을 때 플레이어의 크래딧과 상점 아이템의 가치를 비교하고, 인벤토리에 자리가 있는지, 창고에 자리가 있는지 판단하고 조건이 불만족되면 구매실패 처리.

        //조건을 통과하면 플레이어의 크래딧을 아이템의 가격만큼 깎은 후, 구매한 아이템에게 유니크Id를 부여하고 인벤토리에 추가하는 함수. 만약 인벤토리가 꽉차있거나 창고로 드래그드롭할 경우에는 창고에 추가.

    }

    //플레이어가 상점에 아이템을 판매할 때 사용되는 함수
    public void SellItemToShop()
    {
        //플레이어의 인벤토리에서 아이템을 삭제하는 함수. 아직은 상점에 해당 아이템이 넘어가는 것은 구현하지 않을 예정.

        //플레이어에게 해당 아이템의 가치에 해당하는 크레딧을 지급하는 함수
    }

}
