using UnityEngine;

public class FieldItem : MonoBehaviour
{
    [SerializeField] private string ItemId;
    [SerializeField] private int Count = 1;

    public bool TryPickup()
    {
        if (InventoryManager.Instance == null || DataManager.Instance == null)
            return false;

        ItemData itemData = DataManager.Instance.GetItemData(ItemId);

        if (itemData == null)
            return false;

        if (itemData is WeaponData weaponData)
            return TryPickupWeapon(weaponData);

        int remainingCount = InventoryManager.Instance.TryAddItem(itemData, Count);
        bool pickedUpAny = remainingCount < Count;
        Count = remainingCount;

        if (Count <= 0)
            Destroy(gameObject);

        return pickedUpAny;
    }

    private bool TryPickupWeapon(WeaponData weaponData)
    {
        if (Count != 1)
        {
            Debug.LogWarning("필드 무기의 Count는 1이어야 합니다.", this);
            return false;
        }

        string instanceId = gameObject.GetInstanceID().ToString();

        if (!InventoryManager.Instance.TryAddWeapon(weaponData, instanceId))
            return false;

        Count = 0;
        Destroy(gameObject);
        return true;
    }
}
