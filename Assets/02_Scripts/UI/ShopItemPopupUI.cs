using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ShopItemPopupUI : UIBase
{
    [SerializeField] private GameObject Gobj_ShopItemPopupUI;
    [SerializeField] private Image Image_ItemIcon;
    [SerializeField] private TMP_Text Text_ItemName;
    [SerializeField] private TMP_Text Text_ItemDescription;
    [SerializeField] private TMP_Text Text_ItemSellingPrice; 

    private RectTransform _popupRectTransform;

    private void Awake() 
    {
        if (Gobj_ShopItemPopupUI != null)
        {
            _popupRectTransform = Gobj_ShopItemPopupUI.GetComponent<RectTransform>();
        }
        HidePopup();
    }

    private void Update()
    {
        if(Gobj_ShopItemPopupUI.activeSelf)
        {
            UpdatePopupPosition();
        }
    }

    public void SetItemData(ItemData itemData)
    {
        if (itemData == null)
        {
            /*
            HidePopup();
            Debug.LogWarning("아이템데이터가 없습니다.");
            return;
            */
            Text_ItemName.text = "이름 없음 (Test)";
            Text_ItemDescription.text = "설명 없음 (Test)";
            Text_ItemSellingPrice.text = "0 Credit";
        }

        Text_ItemName.text = itemData.ItemName;
        Text_ItemDescription.text = itemData.ItemDescription;
        Text_ItemSellingPrice.text = $"{itemData.SellingPrice} Credit";

        ShowPopup();
    }

    public void ShowPopup() 
    {
        UpdatePopupPosition();
        Gobj_ShopItemPopupUI.SetActive(true);
    }

    public void HidePopup()
    {
        if (Gobj_ShopItemPopupUI == null) return;
        Gobj_ShopItemPopupUI.SetActive(false);
    }

    private void UpdatePopupPosition()
    {
        if (Mouse.current == null || _popupRectTransform == null) return;

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

        _popupRectTransform.position = targetPos;
    }
}
