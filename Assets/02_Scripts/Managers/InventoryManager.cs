using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private int MaxSlotCount = 30;
    [SerializeField] private int QuickSlotCount = 3;

    private readonly ItemStack[] _quickSlotList = new ItemStack[4];
    public IReadOnlyList<ItemStack> QuickSlotList => _quickSlotList;

    public event Action OnQuickSlotChanged;

    private readonly List<ItemStack> _itemList = new();

    public IReadOnlyList<ItemStack> ItemList => _itemList;
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
            ItemStack stack = _itemList[i];

            if (stack.Item.Id != item.Id)
                continue;

            if (stack.StackCount >= item.MaxStackSize)
                continue;

            int addableCount = item.MaxStackSize - stack.StackCount;
            int addCount = Mathf.Min(addableCount, remainCount);

            stack.StackCount += addCount;
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

            _itemList.Add(new ItemStack
            {
                Item = item,
                StackCount = addCount
            });

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
            ItemStack stack = _itemList[i];

            if (stack.Item.Id != itemId)
                continue;

            int removeCount = Mathf.Min(stack.StackCount, remainCount);

            stack.StackCount -= removeCount;
            remainCount -= removeCount;

            if (stack.StackCount <= 0)
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

        foreach (ItemStack stack in _itemList)
        {
            if (stack.Item.Id != itemId)
                continue;

            totalCount += stack.StackCount;

            if (totalCount >= count)
                return true;
        }

        return false;
    }

    public ItemStack GetItemStack(int index)
    {
        if (index < 0 || index >= _itemList.Count)
            return null;

        return _itemList[index];
    }

    public bool TryUseItem(int slotIndex)
    {
        ItemStack stack = GetItemStack(slotIndex);

        if (!IsValidStack(stack))
        {
            Debug.LogWarning($"사용할 수 없는 슬롯입니다. Index: {slotIndex}");
            return false;
        }

        string itemType = stack.Item.ItemType;

        switch (itemType)
        {
            case "Consumable":
                return TryUseConsumable(stack);

            default:
                Debug.LogWarning($"사용할 수 없는 아이템 타입입니다. Item: {stack.Item.ItemName}, Type: {itemType}");
                return false;
        }
    }

    public bool TryDropItem(int slotIndex, int count = 1)
    {
        ItemStack stack = GetItemStack(slotIndex);

        if (!IsValidStack(stack))
        {
            Debug.LogWarning($"버릴 수 없는 슬롯입니다. Index: {slotIndex}");
            return false;
        }

        if (count <= 0)
            return false;

        if (stack.StackCount < count)
        {
            Debug.LogWarning($"버릴 개수가 부족합니다. Item: {stack.Item.ItemName}, 보유: {stack.StackCount}, 요청: {count}");
            return false;
        }

        Debug.Log($"아이템 드랍 요청: {stack.Item.ItemName} / Count: {count}");

        // TODO: 월드 드랍 오브젝트 생성
        return TryRemoveItem(stack.Item.Id, count);
    }

    public bool TryRegisterQuickSlot(int inventorySlotIndex, int quickSlotIndex)
    {
        if (quickSlotIndex < 0 || quickSlotIndex >= _quickSlotList.Length)
        {
            Debug.LogWarning($"잘못된 퀵슬롯 인덱스입니다. Index: {quickSlotIndex}");
            return false;
        }

        ItemStack stack = GetItemStack(inventorySlotIndex);

        if (!IsValidStack(stack))
        {
            Debug.LogWarning($"퀵슬롯에 등록할 수 없는 인벤토리 슬롯입니다. Index: {inventorySlotIndex}");
            return false;
        }

        if (!CanRegisterQuickSlot(stack.Item))
        {
            Debug.LogWarning($"퀵슬롯 등록 불가 아이템입니다. Item: {stack.Item.ItemName}, Type: {stack.Item.ItemType}");
            return false;
        }

        _quickSlotList[quickSlotIndex] = stack;

        Debug.Log($"퀵슬롯 등록 완료: QuickSlot {quickSlotIndex}, Item: {stack.Item.ItemName}");

        OnQuickSlotChanged?.Invoke();

        return true;
    }

    public bool TryRegisterQuickSlot(int inventorySlotIndex)
    {
        return TryRegisterQuickSlot(inventorySlotIndex, 0);
    }

    private bool TryUseConsumable(ItemStack stack)
    {
        Debug.Log($"소모품 사용 요청: {stack.Item.ItemName}");

        // TODO: UseItemType / UseItemParameterList 기준으로 효과 적용

        bool removed = TryRemoveItem(stack.Item.Id, 1);

        if (removed)
            OnQuickSlotChanged?.Invoke();

        return removed;
    }


    private bool CanRegisterQuickSlot(ItemData item)
    {
        if (item == null)
            return false;

        return item.ItemType == "Weapon" ||
               item.ItemType == "Consumable";
    }

    private bool IsValidStack(ItemStack stack)
    {
        return stack != null && stack.Item != null && stack.StackCount > 0;
    }

    public bool TrySelectQuickSlot(int quickSlotIndex)
    {
        if (quickSlotIndex < 0 || quickSlotIndex >= _quickSlotList.Length)
        {
            Debug.LogWarning($"잘못된 퀵슬롯 선택입니다. Index: {quickSlotIndex}");
            return false;
        }

        ItemStack stack = _quickSlotList[quickSlotIndex];

        if (!IsValidStack(stack))
        {
            Debug.LogWarning($"비어있는 퀵슬롯입니다. Index: {quickSlotIndex}");
            return false;
        }

        _selectedQuickSlotIndex = quickSlotIndex;

        Debug.Log($"퀵슬롯 선택: {quickSlotIndex}, Item: {stack.Item.ItemName}");

        OnSelectedQuickSlotChanged?.Invoke();

        return true;
    }

    public bool TryUseSelectedQuickSlotItem()
    {
        if (_selectedQuickSlotIndex < 0 || _selectedQuickSlotIndex >= _quickSlotList.Length)
            return false;

        ItemStack stack = _quickSlotList[_selectedQuickSlotIndex];

        if (!IsValidStack(stack))
            return false;

        switch (stack.Item.ItemType)
        {
            case "Weapon":
                Debug.Log($"선택 무기 사용 요청: {stack.Item.ItemName}");
                // TODO: WeaponManager 없이 갈 거면 나중에 여기서 장착/발사 요청 연결
                return true;

            case "Consumable":
                Debug.Log($"선택 소모품 사용 요청: {stack.Item.ItemName}");
                return TryUseConsumable(stack);

            default:
                Debug.LogWarning($"퀵슬롯에서 사용할 수 없는 아이템 타입입니다. Type: {stack.Item.ItemType}");
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

        ItemStack weaponStack = new ItemStack
        {
            Item = weaponData,
            StackCount = 1
        };

        _itemList.Add(weaponStack);

        TryRegisterWeaponToEmptyQuickSlot(weaponStack);

        OnInventoryChanged?.Invoke();

        return true;
    }

    private bool TryRegisterWeaponToEmptyQuickSlot(ItemStack weaponStack)
    {
        if (!IsValidStack(weaponStack))
            return false;

        if (weaponStack.Item.ItemType != "Weapon")
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

            Debug.Log($"무기 자동 퀵슬롯 등록: QuickSlot {i}, Item: {weaponStack.Item.ItemName}");

            return true;
        }

        return false;
    }

    public ItemStack GetSelectedQuickSlotStack()
    {
        if (_selectedQuickSlotIndex < 0 || _selectedQuickSlotIndex >= _quickSlotList.Length)
        {
            return null;
        }

        ItemStack stack = _quickSlotList[_selectedQuickSlotIndex];

        if (!IsValidStack(stack))
            return null;

        return stack;
    }
}