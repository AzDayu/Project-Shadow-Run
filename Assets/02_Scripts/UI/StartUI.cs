using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class StartUI :UIBase
{

    [SerializeField] private TMP_Text Text_Title;
    [SerializeField] private Button Button_NewStart;
    [SerializeField] private Button Button_Load;



    public void StartNewGame()
    {
        SaveManager.Instance.CreateNewPlayerData();
        UIManager.Instance.CloseStartUI();
    }

    public void LoadGame()
    {
        SaveManager.Instance.LoadPlayerData();
        UIManager.Instance.CloseStartUI();
    }

}
