using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private void Awake()
    {
        Debug.Log("[DataManager] Awake 실행");

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[DataManager] 중복 인스턴스 제거");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Debug.Log("[DataManager] Instance 등록 완료");

        LoadFullData();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }


    public Dictionary<string, PreloadData> _preloadDataDic { get; private set; } = new Dictionary<string, PreloadData>();
    public Dictionary<string, ObjectPoolData> _objectPoolDataDic { get; private set; } = new Dictionary<string, ObjectPoolData>();
    public Dictionary<string, EnemyData> _enemyDataDic { get; private set; } = new Dictionary<string, EnemyData>();

    public Dictionary<string, ItemData> _itemDataDic { get; private set; } = new Dictionary<string, ItemData>();

    private Dictionary<string, T> LoadData<T>(string tableName) where T : BaseData
    {
        string resourcePath = $"Data/{tableName}";
        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);

        if (textAsset == null)
        {
            Debug.LogError($"[Error] 리소스를 찾을 수 없습니다: Resources/{resourcePath}");
            return new Dictionary<string, T>();
        }

        try
        {
            List<T> dataList = JsonConvert.DeserializeObject<List<T>>(textAsset.text);

            if (dataList != null)
            {
                Debug.Log($"{typeof(T).Name} 데이터를 {dataList.Count}개 로드했습니다.");
                return dataList.ToDictionary(item => item.Id.ToString());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{typeof(T).Name} JSON 로드 오류] {ex.Message}");
        }

        return new Dictionary<string, T>();
    }

    public void LoadPreloadData(string jsonPath) => _preloadDataDic = LoadData<PreloadData>(jsonPath);
    public void LoadObjectPoolData(string jsonPath) => _objectPoolDataDic = LoadData<ObjectPoolData>(jsonPath);
    public void LoadEnemyData(string jsonPath) => _enemyDataDic = LoadData<EnemyData>(jsonPath);
    public void LoadItemData(string jsonPath) => _itemDataDic = LoadData<ItemData>(jsonPath);

    public PreloadData GetPreloadData(string id) => _preloadDataDic != null && _preloadDataDic.TryGetValue(id, out var data) ? data : null;
    public ObjectPoolData GetObjectPoolData(string id) => _objectPoolDataDic != null && _objectPoolDataDic.TryGetValue(id, out var data) ? data : null;
    public EnemyData GetEnemyData(string id) => _enemyDataDic != null && _enemyDataDic.TryGetValue(id, out var data) ? data : null;
    public ItemData GetItemData(string id) => _itemDataDic != null && _itemDataDic.TryGetValue(id, out var data) ? data : null;

    public void LoadFullData()
    {
        LoadPreloadData("PreloadData");
        LoadObjectPoolData("ObjectPoolData");
        LoadEnemyData("EnemyData");
        LoadItemData("ItemData");
    }
}