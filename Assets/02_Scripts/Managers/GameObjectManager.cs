using System.Collections.Generic;
using UnityEngine;

public class GameObjectManager : MonoBehaviour
{
    private static GameObjectManager _instance;
    public static GameObjectManager Instance { get { return _instance; } }

    private Dictionary<int, GameObject> _spawnedObjects = new Dictionary<int, GameObject>();

    private int _instanceIdCounter = 1;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public GameObject SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject newObj = Instantiate(prefab, position, rotation);

        int newId = GetNextInstanceId();
        _spawnedObjects.Add(newId, newObj);

        // EntityController를 만들경우 주석 해제
        // newObj.GetComponent<EntityController>().SetInstanceId(newId);

        return newObj;
    }

    public void RemoveObject(int instanceId)
    {
        if (_spawnedObjects.TryGetValue(instanceId, out GameObject objToRemove))
        {
            _spawnedObjects.Remove(instanceId);
            Destroy(objToRemove);
        }
        else
        {
            Debug.LogWarning($"GameObjectManager: ID {instanceId} 객체를 찾을 수 없어 삭제에 실패했습니다.");
        }
    }

    public void ClearAllSpawnedObjects()
    {
        foreach (var obj in _spawnedObjects.Values)
        {
            if (obj != null) Destroy(obj);
        }
        _spawnedObjects.Clear();
        _instanceIdCounter = 1;
        Debug.Log("GameObjectManager: 씬 내의 모든 동적 객체 클리어 완료");
    }

    private int GetNextInstanceId()
    {
        return _instanceIdCounter++;
    }
}