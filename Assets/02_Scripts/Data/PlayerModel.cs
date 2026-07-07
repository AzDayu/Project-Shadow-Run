using System.Collections.Generic;

[System.Serializable]
public class PlayerModel
{
    public string PlayerName { get; set; }
    public int CurrentCredit { get; set; }
    public List<int> InventoryItemIds { get; set; }

    public PlayerModel()
    {
        InventoryItemIds = new List<int>();
    }
}
