using System;
using UnityEngine;

public class PlayerCombatViewModel
{
    private readonly PlayerQuickSlotInventoryModel _quickSlotModel;

    public event Action<ItemData, Vector3> FireRequested;

    public PlayerCombatViewModel(PlayerQuickSlotInventoryModel quickSlotModel)
    {
        _quickSlotModel = quickSlotModel;
    }

    public void RequestFire(Vector3 fireDirection)
    {
        // ToDo: 무기 관련 클래스 구축 후 갱신 필요
    }
}
