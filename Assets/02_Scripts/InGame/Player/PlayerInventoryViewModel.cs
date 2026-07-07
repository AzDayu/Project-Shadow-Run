using System.Collections.Generic;

public class HadItemSlotData
{
    public int itemCount;
    public ItemData itemData;
}

public class InventoryModel
{
    private int maxInvenCount = 30;
    public List<HadItemSlotData> hadItemSlotDataList { get; private set; } = new();

    public void AddItemSlot(string getItemDataId, int getItemCount)
    {
        if (hadItemSlotDataList.Count >= maxInvenCount)
        {
            // TODO: 인벤토리 슬롯이 가득 찼을 때 처리 로직 추가

            return;
        }
        HadItemSlotData getItemSlotData = new HadItemSlotData();
        // TODO: 데이터 ID string 형태로 변환 시 반영 예정
        /*
        ItemData getItemData = GameDataManager.Instance.GetItemDataById(getItemDataId);
        getItemSlotData.itemCount = getItemCount;

        hadItemSlotDataDictionary.Add(getItemSlotData);
        // TODO: 수량이 Max를 넘긴 상태에서 추가 시, 슬롯을 추가해야 하므로 해당 메서드의 반환형을
        //      bool로 변경하고, GetItem()에서 슬롯 추가 여부를 판단하도록 변경 필요
        
        */
    }

    public void GetItem(string getItemDataId, int getItemCount)
    {

    }
}

public class PlayerInventoryViewModel : ViewModelBase
{
    private InventoryModel _inventoryModel;

    public IReadOnlyList<HadItemSlotData> Slots => _inventoryModel.hadItemSlotDataList;

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
