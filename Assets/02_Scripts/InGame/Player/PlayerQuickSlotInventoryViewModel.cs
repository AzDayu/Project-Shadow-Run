using System.Collections.Generic;

public class PlayerQuickSlotInventoryModel
{
    public HadItemSlotData[] _quickSlots = new HadItemSlotData[3];
    public IReadOnlyList<HadItemSlotData> QuickSlots => _quickSlots;

    public HadItemSlotData EquippedSlot { get; private set; }

    public bool TrySetQuickSlot(int index, HadItemSlotData itemSlot)
    {
        if (index < 0 || index >= _quickSlots.Length)
            return false;

        if (itemSlot == null)
            return false;

        if (!CanRegisterQuickSlot(itemSlot))
            return false;

        _quickSlots[index] = itemSlot;
        return true;
    }

    public void EquipQuickSlot(int index)
    {
        EquippedSlot = _quickSlots[index];
    }

    public void UnEquip()
    {
        EquippedSlot = null;
    }

    public void ClearQuickSlot(int index)
    {
        if (_quickSlots[index] == EquippedSlot)
            UnEquip();

        _quickSlots[index] = null;
    }

    private bool CanRegisterQuickSlot(HadItemSlotData itemSlot)
    {
        return itemSlot.ItemCountAndStack.Item.ItemType == "Weapon"
            || itemSlot.ItemCountAndStack.Item.ItemType == "Consumable";
    }
}

public class PlayerQuickSlotInventoryViewModel : ViewModelBase
{
    private readonly InventoryModel _inventoryModel;
    private readonly PlayerQuickSlotInventoryModel _quickSlotModel;

    public IReadOnlyList<HadItemSlotData> InventorySlots => _inventoryModel.HadItemSlotDataList;

    public IReadOnlyList<HadItemSlotData> QuickSlots => _quickSlotModel.QuickSlots;

    public HadItemSlotData EquippedItemSlot => _quickSlotModel.EquippedSlot;

    public PlayerQuickSlotInventoryViewModel(
        InventoryModel inventoryModel,
        PlayerQuickSlotInventoryModel quickSlotModel)
    {
        _inventoryModel = inventoryModel;
        _quickSlotModel = quickSlotModel;
    }

    public void SetQuickSlotItem(int inventorySlotIndex, int quickSlotIndex)
    {
        if (inventorySlotIndex < 0 || inventorySlotIndex >= InventorySlots.Count)
            return;

        if (quickSlotIndex < 0 || quickSlotIndex >= QuickSlots.Count)
            return;

        HadItemSlotData slotData = InventorySlots[inventorySlotIndex];

        bool success = _quickSlotModel.TrySetQuickSlot(quickSlotIndex, slotData);

        if (!success)
            return;

        OnPropertyChanged(nameof(QuickSlots));
    }

    public void EquipQuickSlot(int quickSlotIndex)
    {
        if (quickSlotIndex < 0 || quickSlotIndex >= QuickSlots.Count)
            return;

        _quickSlotModel.EquipQuickSlot(quickSlotIndex);

        OnPropertyChanged(nameof(EquippedItemSlot));
    }

    public void UnEquip()
    {
        _quickSlotModel.UnEquip();

        OnPropertyChanged(nameof(EquippedItemSlot));
    }

    public void ClearQuickSlot(int quickSlotIndex)
    {
        if (quickSlotIndex < 0 || quickSlotIndex >= QuickSlots.Count)
            return;

        _quickSlotModel.ClearQuickSlot(quickSlotIndex);

        OnPropertyChanged(nameof(QuickSlots));
        OnPropertyChanged(nameof(EquippedItemSlot));
    }
}
