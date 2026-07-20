using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryDebugTester : MonoBehaviour
{
    [Serializable]
    private class TestItemEntry
    {
        public string ItemId;

        [Min(1)]
        public int Count = 1;
    }

    [SerializeField] private bool AddOnStart = true;
    [SerializeField] private List<TestItemEntry> TestItems = new();

    private IEnumerator Start()
    {
        if (!AddOnStart)
            yield break;

        // 다른 Manager들의 Awake/Start 초기화가 끝난 뒤 테스트 데이터를 넣는다.
        yield return null;
        AddTestItems();
    }

    [ContextMenu("Add Test Items")]
    public void AddTestItems()
    {
        Debug.Log($"현재 DataManager에 로드된 아이템 총 개수: {DataManager.Instance._itemDataDic.Count}개");

        if (!Application.isPlaying)
        {
            Debug.LogWarning("테스트 아이템 추가는 Play Mode에서 실행해야 합니다.");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager.Instance가 없습니다.");
            return;
        }

        if (DataManager.Instance == null)
        {
            Debug.LogError("DataManager.Instance가 없습니다.");
            return;
        }

        foreach (TestItemEntry entry in TestItems)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId))
                continue;

            int count = Mathf.Max(1, entry.Count);
            ItemData itemData = DataManager.Instance.GetItemData(entry.ItemId);

            if (itemData == null)
            {
                Debug.LogWarning($"테스트 아이템 데이터를 찾지 못했습니다. ItemId: {entry.ItemId}");
                continue;
            }

            if (itemData is WeaponData weaponData)
            {
                AddWeapons(weaponData, count);
                continue;
            }

            int remainingCount = InventoryManager.Instance.TryAddItem(itemData, count);

            Debug.Log(
                $"테스트 아이템 추가: {itemData.Name}, " +
                $"추가: {count - remainingCount}, 남음: {remainingCount}"
            );
        }
    }

    private void AddWeapons(WeaponData weaponData, int count)
    {
        int addedCount = 0;

        for (int i = 0; i < count; i++)
        {
            if (!InventoryManager.Instance.TryAddWeapon(weaponData))
                break;

            addedCount++;
        }

        Debug.Log(
            $"테스트 무기 추가: {weaponData.Name}, " +
            $"추가: {addedCount}, 남음: {count - addedCount}"
        );
    }
}