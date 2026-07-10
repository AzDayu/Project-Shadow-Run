using System.Collections.Generic;
using UnityEngine;

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
    public float AttackInterval;
    public int MagazineSize;
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
