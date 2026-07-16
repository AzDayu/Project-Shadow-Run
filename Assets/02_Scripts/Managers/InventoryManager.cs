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

        if (item.MaxStackCount <= 0)
            return count;

        int remainCount = count;

        for (int i = 0; i < _itemList.Count; i++)
        {
            ItemModel stack = _itemList[i];

            if (stack.ItemId != item.Id)
                continue;

            if (stack.CurrentStackCount >= item.MaxStackCount)
                continue;

            int addableCount = item.MaxStackCount - stack.CurrentStackCount;
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

            int addCount = Mathf.Min(item.MaxStackCount, remainCount);

            _itemList.Add(new ItemModel{ItemId = item.Id,CurrentStackCount = addCount});

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

        string itemType = DataManager.Instance.GetItemData(stack.ItemId).ItemType;

        switch (itemType)
        {
            case "Consumable":
                return TryUseConsumable(stack);

            default:
                Debug.LogWarning($"사용할 수 없는 아이템 타입입니다. Item: {DataManager.Instance.GetItemData(stack.ItemId).Name}, Type: {itemType}");
                return false;
        }
    }

    public bool TryDropItem(int slotIndex, int count = 1)
    {
        ItemModel itemModel = GetItemModel(slotIndex);

        if (itemModel == null)
            return false;

        if (!IsValidStack(itemModel))
            return false;

        if (count <= 0)
            return false;

        if (itemModel.CurrentStackCount < count)
            return false;

        bool wasRegistered = IsRegisteredInQuickSlot(itemModel);

        itemModel.CurrentStackCount -= count;

        if (itemModel.CurrentStackCount <= 0)
        {
            bool removed = _itemList.Remove(itemModel);
            UnregisterItemFromQuickSlots(itemModel);
        }
        else if (wasRegistered)
            OnQuickSlotChanged?.Invoke();
        
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool TryRegisterQuickSlot(int inventorySlotIndex, int quickSlotIndex)
    {
        if (quickSlotIndex < 0 || quickSlotIndex >= _quickSlotList.Length)
        {
            Debug.LogWarning($"잘못된 퀵슬롯 인덱스입니다. Index: {quickSlotIndex}");
            return false;
        }

        ItemModel itemModel = GetItemModel(inventorySlotIndex);

        if (!IsValidStack(itemModel))
        {
            Debug.LogWarning($"퀵슬롯에 등록할 수 없는 인벤토리 슬롯입니다. Index: {inventorySlotIndex}");
            return false;
        }

        if (!CanRegisterQuickSlot(itemModel))
        {
            Debug.LogWarning($"퀵슬롯 등록 불가 아이템입니다. Item: {DataManager.Instance.GetItemData(itemModel.ItemId).Name}, Type: {DataManager.Instance.GetItemData(itemModel.ItemId).ItemType}");
            return false;
        }

        bool selectedItemMoved = false;

        for (int i = 0; i < _quickSlotList.Length; i++)
        {
            if (i == quickSlotIndex)
                continue;

            if (!IsSameItemModel(_quickSlotList[i], itemModel))
                continue;

            _quickSlotList[i] = null;

            // 현재 선택된 아이템을 옮긴 경우 선택 위치도 따라간다.
            if (_selectedQuickSlotIndex == i)
            {
                _selectedQuickSlotIndex = quickSlotIndex;
                selectedItemMoved = true;
            }
        }

        bool selectedSlotReplaced = _selectedQuickSlotIndex == quickSlotIndex;

        _quickSlotList[quickSlotIndex] = itemModel;

        OnQuickSlotChanged?.Invoke();

        if (selectedItemMoved || selectedSlotReplaced)
            OnSelectedQuickSlotChanged?.Invoke();

        Debug.Log($"퀵슬롯 등록 완료: QuickSlot {quickSlotIndex}, ItemId: {itemModel.ItemId}");

        return true;
    }

    public bool TryRegisterQuickSlot(int inventorySlotIndex)
    {
        ItemModel itemModel = GetItemModel(inventorySlotIndex);

        if (!IsValidStack(itemModel))
            return false;

        if (!CanRegisterQuickSlot(itemModel))
            return false;

        for (int i = 0; i < _quickSlotList.Length; i++)
        {
            if (!IsSameItemModel(_quickSlotList[i], itemModel))
                continue;
            return true;
        }

        for (int i = 0; i < _quickSlotList.Length; i++)
        {
            if (IsValidStack(_quickSlotList[i]))
                continue;

            return TryRegisterQuickSlot(inventorySlotIndex, i);
        }

        return false;
    }

    private bool TryUseConsumable(ItemModel stack)
    {
        Debug.Log($"소모품 사용 요청: {DataManager.Instance.GetItemData(stack.ItemId).Name}");

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

        return DataManager.Instance.GetItemData(item.ItemId).ItemType == "Weapon" || 
            DataManager.Instance.GetItemData(item.ItemId).ItemType == "Consumable";
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

        Debug.Log($"퀵슬롯 선택: {quickSlotIndex}, Item: {DataManager.Instance.GetItemData(stack.ItemId).Name}");

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

        switch (DataManager.Instance.GetItemData(stack.ItemId).ItemType)
        {
            case "Weapon":
                Debug.Log($"선택 무기 사용 요청: {DataManager.Instance.GetItemData(stack.ItemId).Name}");
                // TODO: WeaponManager 없이 갈 거면 나중에 여기서 장착/발사 요청 연결
                return true;

            case "Consumable":
                Debug.Log($"선택 소모품 사용 요청: {DataManager.Instance.GetItemData(stack.ItemId).Name}");
                return TryUseConsumable(stack);

            default:
                Debug.LogWarning($"퀵슬롯에서 사용할 수 없는 아이템 타입입니다. Type: {DataManager.Instance.GetItemData(stack.ItemId).ItemType}");
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
            // TODO : 임시 ID 부여 나중에는 불러오는 Weapon의 데이터를 넣어야함. 
            //       현재로써는 그냥 임시 ID 부여
            InstanceId = "11111111",
            ItemId = weaponData.Id,
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

        if (DataManager.Instance.GetItemData(weaponStack.ItemId).ItemType != "Weapon")
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

            Debug.Log($"무기 자동 퀵슬롯 등록: QuickSlot {i}, Item: {DataManager.Instance.GetItemData(weaponStack.ItemId).Name}");

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

    private bool IsSameItemModel(ItemModel first, ItemModel second)
    {
        if (ReferenceEquals(first, second))
            return true;

        if (first == null || second == null)
            return false;

        if (string.IsNullOrWhiteSpace(first.InstanceId) ||
            string.IsNullOrWhiteSpace(second.InstanceId))
            return false;

        return first.InstanceId == second.InstanceId;
    }

    private bool IsRegisteredInQuickSlot(ItemModel itemModel)
    {
        for (int i = 0; i < _quickSlotList.Length; i++)
        {
            if (IsSameItemModel(_quickSlotList[i], itemModel))
                return true;
        }

        return false;
    }

    private void UnregisterItemFromQuickSlots(ItemModel itemModel)
    {
        bool quickSlotChanged = false;
        bool selectedQuickSlotChanged = false;

        for (int i = 0; i < _quickSlotList.Length; i++)
        {
            if (!IsSameItemModel(_quickSlotList[i], itemModel))
                continue;

            _quickSlotList[i] = null;
            quickSlotChanged = true;

            if (_selectedQuickSlotIndex == i)
            {
                _selectedQuickSlotIndex = -1;
                selectedQuickSlotChanged = true;
            }
        }

        if (quickSlotChanged)
            OnQuickSlotChanged?.Invoke();

        if (selectedQuickSlotChanged)
            OnSelectedQuickSlotChanged?.Invoke();
    }

    public bool TryUnregisterQuickSlot(int quickSlotIndex)
    {
        if (quickSlotIndex < 0 || quickSlotIndex >= _quickSlotList.Length)
            return false;

        ItemModel itemModel = _quickSlotList[quickSlotIndex];

        if (!IsValidStack(itemModel))
            return false;

        _quickSlotList[quickSlotIndex] = null;

        bool selectedSlotUnregistered = (_selectedQuickSlotIndex == quickSlotIndex);

        if (selectedSlotUnregistered)
            _selectedQuickSlotIndex = -1;

        OnQuickSlotChanged?.Invoke();

        if (selectedSlotUnregistered)
            OnSelectedQuickSlotChanged?.Invoke();

        return true;
    }
}