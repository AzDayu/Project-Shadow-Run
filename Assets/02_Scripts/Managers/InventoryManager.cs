using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private int MaxSlotCount = 30;
    [SerializeField] private int QuickSlotCount = 3;

    private readonly ItemModel[] _quickSlotList = new ItemModel[3];
    public IReadOnlyList<ItemModel> QuickSlotList => _quickSlotList;

    public event Action OnQuickSlotChanged;

    private readonly List<ItemModel> _itemList = new();

    public IReadOnlyList<ItemModel> ItemList => _itemList;
    public event Action OnInventoryChanged;

    private int _selectedQuickSlotIndex = -1;
    public int SelectedQuickSlotIndex => _selectedQuickSlotIndex;
    public event Action OnSelectedQuickSlotChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public int TryAddItem(ItemData item, int count)
    {
        if (item == null)
            return count;

        if (count <= 0)
            return count;

        if (item.MaxStackSize <= 0)
            return count;

        int remainCount = count;

        for (int i = 0; i < _itemList.Count; i++)
        {
            ItemModel stack = _itemList[i];

            if (stack.ItemId != item.Id)
                continue;

            if (stack.CurrentStackCount >= item.MaxStackSize)
                continue;

            int addableCount = item.MaxStackSize - stack.CurrentStackCount;
            int addCount = Mathf.Min(addableCount, remainCount);

            stack.CurrentStackCount += addCount;
            remainCount -= addCount;

            if (remainCount <= 0)
            {
                OnInventoryChanged?.Invoke();
                return 0;
            }
        }

        while (remainCount > 0)
        {
            if (_itemList.Count >= MaxSlotCount)
            {
                OnInventoryChanged?.Invoke();
                return remainCount;
            }

            int addCount = Mathf.Min(item.MaxStackSize, remainCount);

            _itemList.Add(new ItemModel{ItemId = item.Id, CurrentStackCount = addCount});

            remainCount -= addCount;
        }

        OnInventoryChanged?.Invoke();
        return 0;
    }

    public bool TryRemoveItem(string itemId, int count)
    {
        if (string.IsNullOrEmpty(itemId))
            return false;

        if (count <= 0)
            return false;

        if (!HasItem(itemId, count))
            return false;

        int remainCount = count;

        for (int i = _itemList.Count - 1; i >= 0; i--)
        {
            ItemModel stack = _itemList[i];

            if (stack.ItemId != itemId)
                continue;

            int removeCount = Mathf.Min(stack.CurrentStackCount, remainCount);

            stack.CurrentStackCount -= removeCount;
            remainCount -= removeCount;

            if (stack.CurrentStackCount <= 0)
                _itemList.RemoveAt(i);

            if (remainCount <= 0)
            {
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool HasItem(string itemId, int count)
    {
        if (string.IsNullOrEmpty(itemId))
            return false;

        if (count <= 0)
            return false;

        int totalCount = 0;

        foreach (ItemModel stack in _itemList)
        {
            if (stack.ItemId != itemId)
                continue;

            totalCount += stack.CurrentStackCount;

            if (totalCount >= count)
                return true;
        }

        return false;
    }

    public ItemModel GetItemModel(int index)
    {
        if (index < 0 || index >= _itemList.Count)
            return null;

        return _itemList[index];
    }

    public bool TryUseItem(int slotIndex)
    {
        ItemModel stack = GetItemModel(slotIndex);

        if (!IsValidStack(stack))
        {
            Debug.LogWarning($"사용할 수 없는 슬롯입니다. Index: {slotIndex}");
            return false;
        }

        string itemType = GameDataManager.Instance.GetItemDataById(stack.ItemId).ItemType;

        switch (itemType)
        {
            case "Consumable":
                return TryUseConsumable(stack);

            default:
                Debug.LogWarning($"사용할 수 없는 아이템 타입입니다. Item: {GameDataManager.Instance.GetItemDataById(stack.ItemId).ItemName}, Type: {itemType}");
                return false;
        }
    }

    public bool TryDropItem(int slotIndex, int count = 1)
    {
        ItemModel stack = GetItemModel(slotIndex);

        if (!IsValidStack(stack))
        {
            Debug.LogWarning($"버릴 수 없는 슬롯입니다. Index: {slotIndex}");
            return false;
        }

        if (count <= 0)
            return false;

        if (stack.CurrentStackCount < count)
        {
            Debug.LogWarning($"버릴 개수가 부족합니다. Item: {GameDataManager.Instance.GetItemDataById(stack.ItemId).ItemName}, 보유: {stack.CurrentStackCount}, 요청: {count}");
            return false;
        }

        Debug.Log($"아이템 드랍 요청: {GameDataManager.Instance.GetItemDataById(stack.ItemId).ItemName} / Count: {count}");

        // TODO: 월드 드랍 오브젝트 생성
        return TryRemoveItem(stack.ItemId, count);
    }

    public bool TryRegisterQuickSlot(int inventorySlotIndex, int quickSlotIndex)
    {
        if (quickSlotIndex < 0 || quickSlotIndex >= _quickSlotList.Length)
        {
            Debug.LogWarning($"잘못된 퀵슬롯 인덱스입니다. Index: {quickSlotIndex}");
            return false;
        }

        ItemModel stack = GetItemModel(inventorySlotIndex);

        if (!IsValidStack(stack))
        {
            Debug.LogWarning($"퀵슬롯에 등록할 수 없는 인벤토리 슬롯입니다. Index: {inventorySlotIndex}");
            return false;
        }

        if (!CanRegisterQuickSlot(stack))
        {
            Debug.LogWarning($"퀵슬롯 등록 불가 아이템입니다. Item: {GameDataManager.Instance.GetItemDataById(stack.ItemId).ItemName}, Type: {GameDataManager.Instance.GetItemDataById(stack.ItemId).ItemType}");
            return false;
        }

        _quickSlotList[quickSlotIndex] = stack;

        Debug.Log($"퀵슬롯 등록 완료: QuickSlot {quickSlotIndex}, Item: {GameDataManager.Instance.GetItemDataById(stack.ItemId).ItemName}");

        OnQuickSlotChanged?.Invoke();

        return true;
    }

    public bool TryRegisterQuickSlot(int inventorySlotIndex)
    {
        return TryRegisterQuickSlot(inventorySlotIndex, 0);
    }

    private bool TryUseConsumable(ItemModel stack)
    {
        Debug.Log($"소모품 사용 요청: {GameDataManager.Instance.GetItemDataById(stack.ItemId).ItemName}");

        // TODO: UseItemType / UseItemParameterList 기준으로 효과 적용
        bool removed = TryRemoveItem(stack.ItemId, 1);

        if (removed)
            OnQuickSlotChanged?.Invoke();

        return removed;
    }


    private bool CanRegisterQuickSlot(ItemModel item)
    {
        if (item == null)
            return false;

        return GameDataManager.Instance.GetItemDataById(item.ItemId).ItemType == "Weapon" || 
            GameDataManager.Instance.GetItemDataById(item.ItemId).ItemType == "Consumable";
    }

    private bool IsValidStack(ItemModel stack)
    {
        return stack != null && stack.CurrentStackCount > 0;
    }

    public bool TrySelectQuickSlot(int quickSlotIndex)
    {
        if (quickSlotIndex < 0 || quickSlotIndex >= _quickSlotList.Length)
        {
            Debug.LogWarning($"잘못된 퀵슬롯 선택입니다. Index: {quickSlotIndex}");
            return false;
        }

        ItemModel stack = _quickSlotList[quickSlotIndex];

        if (!IsValidStack(stack))
        {
            Debug.LogWarning($"비어있는 퀵슬롯입니다. Index: {quickSlotIndex}");
            return false;
        }

        _selectedQuickSlotIndex = quickSlotIndex;

        Debug.Log($"퀵슬롯 선택: {quickSlotIndex}, Item: {GameDataManager.Instance.GetItemDataById(stack.ItemId).ItemName}");

        OnSelectedQuickSlotChanged?.Invoke();

        return true;
    }

    public bool TryUseSelectedQuickSlotItem()
    {
        if (_selectedQuickSlotIndex < 0 || _selectedQuickSlotIndex >= _quickSlotList.Length)
            return false;

        ItemModel stack = _quickSlotList[_selectedQuickSlotIndex];

        if (!IsValidStack(stack))
            return false;

        switch (GameDataManager.Instance.GetItemDataById(stack.ItemId).ItemType)
        {
            case "Weapon":
                Debug.Log($"선택 무기 사용 요청: {GameDataManager.Instance.GetItemDataById(stack.ItemId).ItemName}");
                // TODO: WeaponManager 없이 갈 거면 나중에 여기서 장착/발사 요청 연결
                return true;

            case "Consumable":
                Debug.Log($"선택 소모품 사용 요청: {GameDataManager.Instance.GetItemDataById(stack.ItemId).ItemName}");
                return TryUseConsumable(stack);

            default:
                Debug.LogWarning($"퀵슬롯에서 사용할 수 없는 아이템 타입입니다. Type: {GameDataManager.Instance.GetItemDataById(stack.ItemId).ItemType}");
                return false;
        }
    }

    public bool TryAddWeapon(WeaponData weaponData)
    {
        if (weaponData == null)
            return false;

        if (weaponData.ItemType != "Weapon")
            return false;

        if (_itemList.Count >= MaxSlotCount)
            return false;

        ItemModel weaponStack = new ItemModel
        {
            CurrentStackCount = 1
        };

        _itemList.Add(weaponStack);

        TryRegisterWeaponToEmptyQuickSlot(weaponStack);

        OnInventoryChanged?.Invoke();

        return true;
    }

    private bool TryRegisterWeaponToEmptyQuickSlot(ItemModel weaponStack)
    {
        if (!IsValidStack(weaponStack))
            return false;

        if (GameDataManager.Instance.GetItemDataById(weaponStack.ItemId).ItemType != "Weapon")
            return false;

        for (int i = 0; i < _quickSlotList.Length; i++)
        {
            if (IsValidStack(_quickSlotList[i]))
                continue;

            _quickSlotList[i] = weaponStack;

            OnQuickSlotChanged?.Invoke();

            if (_selectedQuickSlotIndex < 0)
            {
                _selectedQuickSlotIndex = i;
                OnSelectedQuickSlotChanged?.Invoke();
            }

            Debug.Log($"무기 자동 퀵슬롯 등록: QuickSlot {i}, Item: {GameDataManager.Instance.GetItemDataById(weaponStack.ItemId).ItemName}");

            return true;
        }

        return false;
    }

    public ItemModel GetSelectedQuickSlotStack()
    {
        if (_selectedQuickSlotIndex < 0 || _selectedQuickSlotIndex >= _quickSlotList.Length)
        {
            return null;
        }

        ItemModel stack = _quickSlotList[_selectedQuickSlotIndex];

        if (!IsValidStack(stack))
            return null;

        return stack;
    }
}