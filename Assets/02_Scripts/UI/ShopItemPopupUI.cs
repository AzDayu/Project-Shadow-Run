using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ShopItemPopupUI : UIBase
{
    public static ShopItemPopupUI Inst;

    [SerializeField] private GameObject Gobj_ShopItemPopupUI;
    [SerializeField] private Image Image_ItemIcon;
    [SerializeField] private TMP_Text Text_ItemName;
    [SerializeField] private TMP_Text Text_ItemDescription;
    [SerializeField] private TMP_Text Text_ItemSellingPrice; 

    private RectTransform _popupRectTransform;

    private void Awake() // 추후 UIManager가 만들어지면 씬에서 뺄 예정.
    {
        if (Inst == null)
        {
            Inst = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (Gobj_ShopItemPopupUI == null) return;

        _popupRectTransform = Gobj_ShopItemPopupUI.GetComponent<RectTransform>();

        HidePopup();
    }

    private void Update()
    {
        if(Gobj_ShopItemPopupUI.activeSelf)
        {
            UpdatePopupPosition();
        }
    }

    public void ShowPopup() // 파라미터로 아이템 정보를 가져오게 할것. 아이템 아이디 넘겨받고 정보를 가져오면 될까.
    {
        UpdatePopupPosition();

        Gobj_ShopItemPopupUI.SetActive(true);
    }

    public void HidePopup()
    {
        Gobj_ShopItemPopupUI.SetActive(false);
    }

    private void UpdatePopupPosition()
    {
        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 offset = new Vector2(10f, -10f);

        Vector2 targetPos = mousePos + offset;

        float popupWidth = _popupRectTransform.rect.width;
        float popupHeight = _popupRectTransform.rect.height;

        float minY = popupHeight * _popupRectTransform.pivot.y;
        float maxY = Screen.height - (popupHeight * (1f - _popupRectTransform.pivot.y));
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        float minX = popupWidth * _popupRectTransform.pivot.x;
        float maxX = Screen.width - (popupWidth * (1f - _popupRectTransform.pivot.x));
        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);

        Gobj_ShopItemPopupUI.transform.position = targetPos;
    }
}
