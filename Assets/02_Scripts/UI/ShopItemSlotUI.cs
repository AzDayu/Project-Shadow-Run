using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItemSlotUI : UIBase, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image Image_ItemIcon;
    [SerializeField] private TMP_Text Text_ItemPrice;

    // 데이터는 나중에 모델뷰로 뺄 것!
    public ShopItemSlotType CurSlotType { get; set; }
    public int _slotIdx { get; set; }
    public int _itemDataId { get; set; }

    public void InitSlot(ShopItemSlotType slotType, int slotIdx, int itemDataId)
    {
        CurSlotType = slotType;
        _slotIdx = slotIdx;
        _itemDataId = itemDataId;
        // 이미지와 가격 텍스트는 아이템 아이디로 가져온 정보로 초기화해준다.
    }



    // ======================== 슬롯 팝업 관련 메서드 ========================

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShopItemPopupUI.Inst.ShowPopup();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ShopItemPopupUI.Inst.HidePopup();
    }

}
