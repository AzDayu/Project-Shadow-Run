using System.Collections.Generic;

[System.Serializable]
public class PlayerModel
{
    public string PlayerName { get; set; }
    public int CurrentCredit { get; set; }

    public List<ItemModel> InventoryItems { get; set; }
    public List<ItemModel> StashItems { get; set; }

    public PlayerModel()
    {
        InventoryItems = new List<ItemModel>();
        StashItems = new List<ItemModel>();
    }
}
