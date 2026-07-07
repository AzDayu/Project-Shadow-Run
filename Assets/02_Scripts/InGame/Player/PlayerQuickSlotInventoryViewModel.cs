using System.Collections.Generic;
using UnityEngine;

public class PlayerQuickSlotInventoryModel
{
    public HadItemSlotData[] _quickSlots { get; private set; } = new HadItemSlotData[3];

    public HadItemSlotData _equippedWeaponSlot { get; private set; }

    public void EquipQuickSlotItem(HadItemSlotData slotData, int quickSlotIndex)
    {
        if (quickSlotIndex < 0 || quickSlotIndex >= _quickSlots.Length)
        {
            Debug.LogError("Invalid quick slot index: " + quickSlotIndex);
            return;
        }

        _quickSlots[quickSlotIndex] = slotData;
        _equippedWeaponSlot = slotData;
    }
    public void UnEquipWeapon()
    {
        _equippedWeaponSlot = null;
    }

    public void ClearQuickSlot(int quickSlotIndex)
    {
        if (quickSlotIndex < 0 || quickSlotIndex >= _quickSlots.Length)
        {
            Debug.LogError("Invalid quick slot index: " + quickSlotIndex);
            return;
        }

        if (_quickSlots[quickSlotIndex] == _equippedWeaponSlot)
            _equippedWeaponSlot = null;

        _quickSlots[quickSlotIndex] = null;
    }
}

public class PlayerQuickSlotInventoryViewModel : ViewModelBase
{
    private readonly InventoryModel _inventoryModel;
    private readonly PlayerQuickSlotInventoryModel _quickSlotModel;

    public IReadOnlyList<HadItemSlotData> InventorySlots => _inventoryModel.hadItemSlotDataList;

    public IReadOnlyList<HadItemSlotData> QuickSlots => _quickSlotModel._quickSlots;

    public HadItemSlotData EquippedWeaponSlot => _quickSlotModel._equippedWeaponSlot;

    public PlayerQuickSlotInventoryViewModel(
        InventoryModel inventoryModel,
        PlayerQuickSlotInventoryModel quickSlotModel)
    {
        _inventoryModel = inventoryModel;
        _quickSlotModel = quickSlotModel;
    }

    public void EquipQuickSlotItem(int inventorySlotIndex, int quickSlotIndex)
    {
        if (inventorySlotIndex < 0 || inventorySlotIndex >= InventorySlots.Count)
            return;

        HadItemSlotData slotData = InventorySlots[inventorySlotIndex];

        if (slotData == null || slotData.itemData == null)
            return;

        _quickSlotModel.EquipQuickSlotItem(slotData, quickSlotIndex);

        OnPropertyChanged(nameof(QuickSlots));
        OnPropertyChanged(nameof(EquippedWeaponSlot));
    }

    public void UnEquipWeapon()
    {
        _quickSlotModel.UnEquipWeapon();

        OnPropertyChanged(nameof(EquippedWeaponSlot));
    }

    public void ClearQuickSlot(int quickSlotIndex)
    {
        _quickSlotModel.ClearQuickSlot(quickSlotIndex);

        OnPropertyChanged(nameof(QuickSlots));
        OnPropertyChanged(nameof(EquippedWeaponSlot));
    }
}
