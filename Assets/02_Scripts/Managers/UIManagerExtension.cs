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
    StashUI,
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
        uiManager.OpenLoadingUI();
        uiManager.OpenContentUI(UIType.LobbyUI);
        // uiManager.OpenUI(UIRootType.ContentUI, UIType.LobbyUI); // 위랑 똑같은 원리
        // uiManager.OpenUI(UIRootType.MainUI, UIType.HudUI);
        uiManager.OpenUI(UIRootType.MainUI, UIType.MainUI);
        // 게임 로비 UI를 여기서 오픈해주자 -> uiManager.
    }

    //public static void OpenSimplePopup(this UIManager uiManager, string msg)
    //{
    //    var uiBase = uiManager.OpenPopupUI(UIType.SimplePopup);
    //    if (uiBase == null)
    //    {
    //        Debug.LogWarning($"UI가 생성되지 않았습니다");
    //        return;
    //    }

    //    if (uiBase is SimplePopup simplePopup)
    //    {
    //        simplePopup.SetUI(msg);
    //    }
    //}

    // 신규UI추가 3) 이렇게 어떤 팝업을 열고, 열때 전달해야하는 파라미터가 있다면 이렇게 전달한다.
    // 추가하기 편하게 그냥 빼둔 확장 메서드이므로, uiManager과 this는 우선 넘어가자
    //public static void OpenMyProfilePopup(this UIManager uiManager, string characterDataId)
    //{
    //    // 신규UI추가 4) 이렇게 UI 타입을 던져서 UI 생성을 요청한다
    //    var uiBase = uiManager.OpenPopupUI(UIType.MyProfilePopup);
    //    if (uiBase == null)
    //    {
    //        Debug.LogWarning($"UI가 생성되지 않았습니다");
    //        return;
    //    }

    //    if (uiBase is MyProfilePopup myProfilePopup)
    //    {
    //        myProfilePopup.RefreshCharacterUI(characterDataId);
    //    }
    //}

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

    //public static void OpenDialogueUI(this UIManager uiManager, string startDialogueId)
    //{
    //    var uiBase = uiManager.OpenContentUI(UIType.DialogueUI);
    //    if (uiBase == null)
    //    {
    //        Debug.LogWarning($"UI가 생성되지 않았습니다");
    //        return;
    //    }

    //    if (uiBase is DialogueUI dialogueUi)
    //    {
    //        dialogueUi.StartDialogue(startDialogueId);
    //    }
    //}

    //public static void AddHudSlot(this UIManager uiManager, int instanceId, Transform targetTransform)
    //{
    //    var uiBase = uiManager.GetOpenedUI(UIRootType.MainUI, UIType.HudUI);
    //    if (uiBase == null) return;

    //    // 기존에 GetComponent를 하던 부분이 클래스 형변환을 해도 되도록 개선되었다 (UIBase를 상속받기 때문)
    //    if (uiBase is HudUI hudUi)
    //    {
    //        // 그 대상이 생성됬을 때 호출
    //        // 몬스터 동적생성이 선행적으로 구조가 잘 잡혀있으므로 그걸 이용할 수 있다
    //        hudUi.AddHudSlot(instanceId, targetTransform);
    //    }
    //}

    // 그 대상이 죽었을때 호출
    //public static void RemoveHudSlot(this UIManager uiManager, int instanceId)
    //{
    //    var uiBase = uiManager.GetOpenedUI(UIRootType.MainUI, UIType.HudUI);
    //    if (uiBase == null) return;

    //    // 기존에 GetComponent를 하던 부분이 클래스 형변환을 해도 되도록 개선되었다 (UIBase를 상속받기 때문)
    //    if (uiBase is HudUI hudUi)
    //    {
    //        // 그 대상이 생성됬을 때 호출
    //        // 몬스터 동적생성이 선행적으로 구조가 잘 잡혀있으므로 그걸 이용할 수 있다
    //        hudUi.RemoveHudSlot(instanceId);
    //    }
    //}


    //public static void AddInteractionSlot(this UIManager uiManager, int instanceId, string interactionKey, string interactionTitle, Transform targetTransform, Action<string> onClickCallback = null)
    //{
    //    var uiBase = uiManager.GetOpenedUI(UIRootType.MainUI, UIType.HudUI);
    //    if (uiBase == null) return;

    //    if (uiBase is HudUI hudUi)
    //    {
    //        hudUi.AddInteractionSlot(instanceId, interactionKey, interactionTitle, targetTransform, onClickCallback);
    //    }
    //}

    //public static void RemoveInteractionSlot(this UIManager uiManager, int instanceId)
    //{
    //    var uiBase = uiManager.GetOpenedUI(UIRootType.MainUI, UIType.HudUI);
    //    if (uiBase == null) return;

    //    if (uiBase is HudUI hudUi)
    //    {
    //        hudUi.RemoveIteractionSlot(instanceId);
    //    }
    //}
}
