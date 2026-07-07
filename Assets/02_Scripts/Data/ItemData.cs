using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

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
