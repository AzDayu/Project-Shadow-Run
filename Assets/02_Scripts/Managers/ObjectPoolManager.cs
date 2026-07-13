using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    private Dictionary<string, Queue<GameObject>> _poolDic = new();
    private Dictionary<string, GameObject> _prefabDic = new();
    public static ObjectPoolManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    public void Initialize()
    {
        foreach (var data in DataManager.Instance._objectPoolDataDic.Values)
        {
           CreatePool(data.Address, data.PoolSize);
        }
    }
    public void CreatePool(string address, int count)
    {
        GameObject prefab = ResourceManager.Inst.GetLoadedAsset<GameObject>(address);

      
        if (prefab == null)
        {
            Debug.LogError($"{address} 프리팹이 프리로드되지 않았습니다.");
            return;
        }

        _prefabDic[address] = prefab;

        Queue<GameObject> queue = new Queue<GameObject>();

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab);

            obj.SetActive(false);

            queue.Enqueue(obj);
        }
        if (_poolDic.ContainsKey(address))
        {
            Debug.LogWarning($"{address} Pool은 이미 존재합니다.");
            return;
        }
        _poolDic.Add(address, queue);
    }

    public GameObject GetFromPool(string address)
    {
        if (!_poolDic.TryGetValue(address, out Queue<GameObject> queue))
        {
            Debug.LogError($"Pool이 존재하지 않습니다 : {address}");
            return null;
        }

        if (queue.Count == 0)
        {
            GameObject obj = Instantiate(_prefabDic[address]);
            return obj;
        }

        GameObject pooled = queue.Dequeue();

        pooled.SetActive(true);

        return pooled;
    }

    public void ReturnToPool(string address, GameObject obj)
    {
        obj.SetActive(false);

        _poolDic[address].Enqueue(obj);
    }
    public void ClearPool()
    {
        foreach (var queue in _poolDic.Values)
        {
            while (queue.Count > 0)
            {
                Destroy(queue.Dequeue());
            }
        }

        _poolDic.Clear();
        _prefabDic.Clear();
    }

    public bool HasPool(string address)
    {
        return _poolDic.ContainsKey(address);
    }
}
