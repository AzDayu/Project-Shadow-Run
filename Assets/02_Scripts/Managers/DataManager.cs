using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;


public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private JsonSerializerSettings _settings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto
    };

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

    public Dictionary<string, ShopItemData> _shopItemDataDic { get; private set; } = new Dictionary<string, ShopItemData>();

    private Dictionary<string, T> LoadData<T>(string tableName) where T : BaseData
    {
        string resourcePath = $"Data/{tableName}";
        TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);

        if (jsonAsset == null)
        {
            Debug.LogError($"[DataManager] {tableName} JSON 파일을 찾을 수 없습니다.");
            return new Dictionary<string, T>();
        }

        try
        {
            List<T> dataList = JsonConvert.DeserializeObject<List<T>>(jsonAsset.text, _settings);

            if (dataList != null)
            {
                Debug.Log($"[DataManager] {tableName} 데이터를 {dataList.Count}개 로드했습니다.");
                Dictionary<string, T> dic = new Dictionary<string, T>();
                foreach (var item in dataList)
                {
                    string key = item.Id.ToString();
                    if (!dic.ContainsKey(key))
                    {
                        dic.Add(key, item);
                    }
                    else
                    {
                        Debug.LogWarning($"[DataManager] 중복된 ID 발견! 덮어쓰기를 무시합니다: {key}");
                    }
                }
                return dic;
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
    public void LoadShopItemData(string jsonPath) => _shopItemDataDic = LoadData<ShopItemData>(jsonPath);

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
        LoadShopItemData("ShopItemData");
    }
}