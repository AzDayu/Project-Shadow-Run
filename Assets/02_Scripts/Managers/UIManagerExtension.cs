using System;
using UnityEngine;

public enum UIRootType
{
    None = 0,
    BackgroundUI,
    MainUI,
    ContentUI,
    PopupUI,
    VeryFrontUI
}

public enum UIType
{
    MainUI,
    Inventory,
    LoadingUI,
    DialogueUI,
    LobbyUI,
    GameBookUI,
    HudUI,
    LocalPlayerProfileUI,
    MVVMTestUI,
    ShopUI,
    StashUI
}

public static class UIManagerExtension
{
    public static string GetUIPath(this UIManager uiManager, UIRootType uiRootType, UIType uiType)
    {
        string path = string.Empty; // "" == string.Empty

        // 신규UI추가 2) Resources.Load를 할 경로를 직접 명시한다
        // 해당 경로는 프로젝트창에서 Resources/Prefabs/UI폴더 내에 있는 RootType 폴더명과 UIType 프리팹 이름과 동일해야 한다! (ex. ContentUI/DNMyProfilePopup)
        path = $"Prefabs/UI/{uiRootType}/{uiType}";
        return path;
    }

    public static void ShowStartupUIOnGameStart(this UIManager uiManager)
    {
        //uiManager.OpenLoadingUI();
       //uiManager.OpenContentUI(UIType.LobbyUI);
        // uiManager.OpenUI(UIRootType.MainUI, UIType.HudUI);
        //uiManager.OpenUI(UIRootType.MainUI, UIType.MainUI);
        // 게임 로비 UI를 여기서 오픈해주자 -> uiManager.
    }

    public static void OpenInventoryPopup(this UIManager uiManger)
    {
        var uiBase = uiManger.OpenContentUI(UIType.Inventory);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }

    public static void OpenLoadingUI(this UIManager uiManager)
    {
        var uiBase = uiManager.OpenUI(UIRootType.VeryFrontUI, UIType.LoadingUI);
        if (uiBase == null)
        {
            Debug.LogWarning($"UI가 생성되지 않았습니다");
            return;
        }
    }

    public static void CloseLoadingUI(this UIManager uiManager)
    {
        uiManager.CloseUI(UIRootType.VeryFrontUI, UIType.LoadingUI);
    }
}
