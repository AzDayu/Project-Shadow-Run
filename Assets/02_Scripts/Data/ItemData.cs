using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    public int ItemId;
    public string ItemName;
    public string Description;
    public int MaxStackCount;

    public string IconPath;
    public string PrefabPath;
}

[System.Serializable]
public class ItemDataList
{
    public List<ItemData> Items;
}
