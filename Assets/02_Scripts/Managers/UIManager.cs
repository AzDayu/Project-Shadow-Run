using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] Canvas Canvas_BgRoot;
    [SerializeField] Canvas Canvas_MainRoot;
    [SerializeField] Canvas Canvas_ContentRoot;
    [SerializeField] Canvas Canvas_PopupRoot;
    [SerializeField] Canvas Canvas_VeryFrontRoot;

    public static UIManager Instance { get; set; }

    // 얘는 생성과 제거에 관한 부분 -> Instancing과 가비지컬렉터와 연관이 있는 애
    private Dictionary<UIType, UIBase> _createdUIDic = new Dictionary<UIType, UIBase>();
    // 얘는 활성과 비활성에 관한 부분 -> SetActive
    private HashSet<UIType> _openedUIDic = new HashSet<UIType>();


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 매니저들이 탄생한 후에 UI매니저가 처음으로 게임이 실행될 때 필요한 UI들을 오픈해준다!
        this.ShowStartupUIOnGameStart();
    }

    public UIBase OpenUI(UIRootType uiRootType, UIType uiType, bool isInitialHide = false)
    {
        // 딱히 요청이 있진 않고 오픈만 하면 되는 UI에서 사용

        var openedUI = GetCreatedUI(uiRootType, uiType);

        bool isSetActiveOnOpen = (isInitialHide == false); // 열었을 때 기본적으로 숨겨서 열 것인지 체크

        if (_openedUIDic.Contains(uiType) == false)
        {
            _openedUIDic.Add(uiType);
        }

        openedUI.gameObject.SetActive(isSetActiveOnOpen);

        return openedUI;
    }

    public void CloseUI(UIRootType uiRootType, UIType uiType)
    {
        if (_openedUIDic.Contains(uiType))
        {
            var openedUi = _createdUIDic[uiType];
            openedUi.gameObject.SetActive(false);
            _openedUIDic.Remove(uiType);
        }
    }

    private Transform GetRootTransform(UIRootType uiRootType)
    {
        Transform root = null;
        switch (uiRootType)
        {
            case UIRootType.BackgroundUI:
                root = Canvas_BgRoot.transform;
                break;
            case UIRootType.MainUI:
                root = Canvas_MainRoot.transform;
                break;
            case UIRootType.ContentUI:
                root = Canvas_ContentRoot.transform;
                break;
            case UIRootType.PopupUI:
                root = Canvas_PopupRoot.transform;
                break;
            case UIRootType.VeryFrontUI:
                root = Canvas_VeryFrontRoot.transform;
                break;
        }
        return root;
    }

    private void CreateUI(UIRootType uiRootType, UIType uiType)
    {
        if (_createdUIDic.ContainsKey(uiType) == false)
        {
            string path = this.GetUIPath(uiRootType, uiType);
            GameObject loadedObj = (GameObject)Resources.Load(path);
            Transform root = GetRootTransform(uiRootType);
            GameObject gObj = Instantiate(loadedObj, root);
            if (gObj != null)
            {
                var uiBase = gObj.GetComponent<UIBase>();
                _createdUIDic.Add(uiType, uiBase);
            }
        }
    }

    public UIBase GetOpenedUI(UIRootType uiRootType, UIType uiType)
    {
        return GetCreatedUI(uiRootType, uiType);
    }

    private UIBase GetCreatedUI(UIRootType uiRootType, UIType uiType)
    {
        if (_createdUIDic.ContainsKey(uiType) == false)
        {
            CreateUI(uiRootType, uiType);
        }
        return _createdUIDic[uiType];
    }

    public bool IsUIOpened(UIType uiType)
    {
        return _openedUIDic.Contains(uiType);
    }

    public UIBase OpenContentUI(UIType uiType)
    {
        return OpenUI(UIRootType.ContentUI, uiType);
    }

    public UIBase OpenPopupUI(UIType uiType)
    {
        return OpenUI(UIRootType.PopupUI, uiType);
    }

    public void CloseContentUI(UIType uiType)
    {
        CloseUI(UIRootType.ContentUI, uiType);
    }

    public void ClosePopupUI(UIType uiType)
    {
        CloseUI(UIRootType.PopupUI, uiType);
    }
}
