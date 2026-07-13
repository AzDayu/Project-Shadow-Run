using System.Collections.Generic;
using UnityEngine;

public enum WeaponPartsType
{

}
public struct WeaponStat
{
    public float Damage;
    public float AttackInterval;
    public int MagazineSize;
    public float Accuracy;
    public float Range;
    public float ReloadTime;
}

[System.Serializable]
public class ItemData
{
    public string ItemId;
    public string ItemName;
    public string ItemDescription;
    public string ItemType;
    public string Grade;
    public int MaxStackSize;
    public int SellingPrice;

    public string IconPath;
    public string PrefabPath;

    public string UseItemType;
    public string[] UseItemParameterList;
}

[System.Serializable]
public class ItemDataList
{
    public ItemData[] Items;
}

[System.Serializable]
public class ItemStack
{
    public ItemData Item;
    public int StackCount;
}
[System.Serializable]
public class WeaponData : ItemData
{
    public float Damage;
    public int MagazineSize;
    public float AttackInterval;
    public float Accuracy;
    public float Range;
    public float ReloadTime;
}
[System.Serializable]
public class WeaponPartsData : ItemData
{
    public WeaponPartsType PartsType;
    public WeaponStatType StatType;
    public WeaponStatModifierType ModifierType;
    public float Value;
}

[System.Serializable]
public class ItemModel
{
    public string InstanceId;      // 생성될 때마다 발급받는 고유 ID (예: Guid)
    public string ItemId;          // DataManager에서 원본 ItemData를 찾기 위한 Key
    public int CurrentStackCount;  // 현재 겹쳐진 개수 (ItemStack 대체)
}

[System.Serializable]
public class WeaponModel : ItemModel
{
    public int CurrentAmmo;        // 현재 장전된 총알 수
    public float CurrentDurability;// 현재 내구도
    public List<ItemModel> AttachedParts; // 장착된 파츠들
}