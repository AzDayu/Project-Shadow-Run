using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    private static GameDataManager _instance;
    public static GameDataManager Instance { get { return _instance; } }

    private Dictionary<string, ItemData> _itemDataDict = new Dictionary<string, ItemData>();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
            LoadStaticDataFromJson();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void LoadStaticDataFromJson()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("Data/ItemData");

        if (jsonText == null)
        {
            Debug.LogError("GameDataManager: ItemData.json 파일을 찾을 수 없습니다! Resources/Data 폴더를 확인하세요.");
            return;
        }

        ItemDataList dataList = JsonUtility.FromJson<ItemDataList>(jsonText.text);

        foreach (ItemData item in dataList.Items)
        {
            if (!_itemDataDict.ContainsKey(item.Id))
            {
                _itemDataDict.Add(item.Id, item);
            }
        }

        Debug.Log($"GameDataManager: JSON 데이터 파싱 성공! 총 {_itemDataDict.Count}개의 아이템 데이터를 로드했습니다.");
    }

    public ItemData GetItemDataById(string itemId)
    {
        if (_itemDataDict.TryGetValue(itemId, out ItemData data))
        {
            return data;
        }

        Debug.LogError($"GameDataManager: ID {itemId}에 해당하는 아이템 데이터가 없습니다!");
        return null;
    }
}
