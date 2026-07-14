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
        Instance = this;


        LoadFullData();
    }

    [Serializable]
    private class SerializationWrapper<T>
    {
        public List<T> items; // JSON 파일의 루트 키 이름이 "items"여야 함
    }
    
    
    public Dictionary<string, PreloadData> _preloadDataDic { get; private set; } = new Dictionary<string, PreloadData>();
    public Dictionary<string, ObjectPoolData> _objectPoolDataDic { get; private set; } = new Dictionary<string, ObjectPoolData>();
    public Dictionary<string, EnemyData> _enemyDataDic { get; private set; } = new Dictionary<string, EnemyData>();


    private Dictionary<string, T> LoadData<T>(string tableName) where T : BaseData
    {
        // 1. 경로 설정 (확장자 .json 제외!)
        // Resources/JsonOutput 폴더
        string resourcePath = $"JsonOutput/{tableName}";

        // 2. 리소스 로드
        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);

        // 3. 파일 존재 여부 체크
        if (textAsset == null)
        {
            Debug.LogError($"[Error] 리소스를 찾을 수 없습니다: Resources/{resourcePath}");
            return new Dictionary<string, T>();
        }

        try
        {
            string jsonString = textAsset.text;

            // 4. JsonUtility용 Wrapper 트릭 적용
            string wrappedJson = "{\"items\":" + jsonString + "}";
            SerializationWrapper<T> wrapper = JsonUtility.FromJson<SerializationWrapper<T>>(wrappedJson);

            if (wrapper != null && wrapper.items != null)
            {
                Debug.Log($"{typeof(T).Name} 데이터를 {wrapper.items.Count}개 로드했습니다.");
                // ToDictionary를 사용하려면 각 클래스(T)에 Id 필드가 있어야 합니다.
                return wrapper.items.ToDictionary(item => item.Id.ToString());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{typeof(T).Name} JSON 로드 오류] {ex.Message}");
        }

        return new Dictionary<string, T>();
    }
    
    
    public void LoadPreloadData(string jsonPath)
    {
        _preloadDataDic = LoadData<PreloadData>(jsonPath);
    }
    public void LoadObjectPoolData(string jsonPath)
    {
        _objectPoolDataDic = LoadData<ObjectPoolData>(jsonPath);
    }
    public void LoadEnemyData(string jsonPath)
    {
        _enemyDataDic = LoadData<EnemyData>(jsonPath);
    }

    public PreloadData GetPreloadData(string id)
    {

        if (_preloadDataDic == null || string.IsNullOrEmpty(id)) return null;
        return _preloadDataDic.TryGetValue(id, out var data) ? data : null;

    }
    public ObjectPoolData GetObjectPoolData(string id)
    {

        if (_objectPoolDataDic == null || string.IsNullOrEmpty(id)) return null;
        return _objectPoolDataDic.TryGetValue(id, out var data) ? data : null;

    }
    public EnemyData GetEnemyData(string id)
    {

        if (_enemyDataDic == null || string.IsNullOrEmpty(id)) return null;
        return _enemyDataDic.TryGetValue(id, out var data) ? data : null;

    }
    public void LoadFullData()
    {
        LoadPreloadData("PreloadData");
        LoadObjectPoolData("ObjectPoolData");
        LoadEnemyData("EnemyData");
    }
}
