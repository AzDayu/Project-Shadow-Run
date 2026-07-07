using System.IO;
using UnityEngine;

public class SaveManager
{
    private static SaveManager _instance;
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SaveManager();
            return _instance;
        }
    }

    private string _savePath;

    private SaveManager()
    {
        _savePath = Path.Combine(Application.persistentDataPath, "PlayerData.json");
        Debug.Log($"SaveManager: 세이브 파일 경로 - {_savePath}");
    }

    public PlayerModel LoadPlayerData()
    {
        if (!File.Exists(_savePath))
        {
            Debug.Log("SaveManager: 세이브 파일이 없습니다. 신규 유저 데이터를 생성합니다.");
            return CreateNewPlayerData();
        }

        try
        {
            string json = File.ReadAllText(_savePath);
            PlayerModel data = JsonUtility.FromJson<PlayerModel>(json);
            Debug.Log("SaveManager: 유저 데이터 로드 성공!");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SaveManager: 세이브 파일이 손상되었습니다! - {e.Message}");
            return CreateNewPlayerData();
        }
    }

    public void SavePlayerData(PlayerModel data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(_savePath, json);
            Debug.Log("SaveManager: 유저 데이터 저장 완료!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SaveManager: 데이터 저장 실패 - {e.Message}");
        }
    }

    private PlayerModel CreateNewPlayerData()
    {
        PlayerModel newPlayer = new PlayerModel
        {
            PlayerName = "Survivor",
            CurrentCredit = 15000
        };

        newPlayer.InventoryItemIds.Add(101);

        SavePlayerData(newPlayer);
        return newPlayer;
    }
}