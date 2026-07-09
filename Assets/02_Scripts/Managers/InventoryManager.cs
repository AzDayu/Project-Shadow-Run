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
}