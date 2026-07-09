using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private int MaxSlotCount = 30;

    private readonly List<ItemStack> _itemList = new();

    public IReadOnlyList<ItemStack> ItemList => _itemList;

    public event Action OnInventoryChanged;

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

            if (stack.Item.ItemId != item.ItemId)
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

            if (stack.Item.ItemId != itemId)
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
            if (stack.Item.ItemId != itemId)
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
        return TryRemoveItem(stack.Item.ItemId, count);
    }

    public bool TryRegisterQuickSlot(int slotIndex)
    {
        ItemStack stack = GetItemStack(slotIndex);

        if (!IsValidStack(stack))
        {
            Debug.LogWarning($"퀵슬롯에 등록할 수 없는 슬롯입니다. Index: {slotIndex}");
            return false;
        }

        if (!CanRegisterQuickSlot(stack.Item))
        {
            Debug.LogWarning($"퀵슬롯 등록 불가 아이템입니다. Item: {stack.Item.ItemName}, Type: {stack.Item.ItemType}");
            return false;
        }

        Debug.Log($"퀵슬롯 등록 요청: {stack.Item.ItemName}");

        // TODO: QuickSlotManager 연결
        return true;
    }

    private bool TryUseConsumable(ItemStack stack)
    {
        Debug.Log($"소모품 사용 요청: {stack.Item.ItemName}");

        // TODO: UseItemType / UseItemParameterList 기준으로 효과 적용
        // 예: Heal:30, Stamina:20 등

        return TryRemoveItem(stack.Item.ItemId, 1);
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
        return stack != null &&
               stack.Item != null &&
               stack.StackCount > 0;
    }
}