using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ShopItemPopupUI : UIBase //생각해보니까 이거 그냥 인벤토리나 창고에서 돌려써도 괜찮을것 같음.
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

    public void SetItemData(string itemDataId)
    {
        if (itemDataId == null)
        {
            HidePopup();
        }
        else
        {
            var itemData = DataManager.Instance.GetItemData(itemDataId);

            Text_ItemName.text = itemData.Name;
            Text_ItemDescription.text = itemData.ItemDescription;
            Text_ItemSellingPrice.text = $"{itemData.SellingPrice} Credit";
            ShowPopup();
        }
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
