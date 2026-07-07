using UnityEngine;

[System.Serializable]
public class ItemData
{
    public int ItemId;
    public string ItemName;
    public string ItemDescription;
    public int MaxStackCount;

    public string IconPath;
    public string PrefabPath;
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