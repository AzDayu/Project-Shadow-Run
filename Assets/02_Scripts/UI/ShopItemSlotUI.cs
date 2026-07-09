using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItemSlotUI : UIBase, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image Image_ItemIcon;
    [SerializeField] private TMP_Text Text_ItemPrice;

    public Action<ItemData> OnHoverEnter;
    public Action OnHoverExit;

    public ItemData ItemData { get; set; }

    public ShopItemSlotType _curSlotType { get; set; }
    public int _slotIdx { get; set; }
    public int _itemDataId { get; set; }

    public void Bind(ItemData itemData, Action<ItemData> onHoverEnter, Action onHoverExit)
    {
        ItemData = itemData;
        OnHoverEnter = onHoverEnter;
        OnHoverExit = onHoverExit;

        if (ItemData != null)
        {
            Text_ItemPrice.text = $"{ItemData.SellingPrice} Gold";
            // Image_ItemIcon.sprite = Resources.Load<Sprite>(ItemData.IconPath); 등 아이콘 로드
        }
    }

    public void InitSlot(ShopItemSlotType slotType, int slotIdx, int itemDataId)
    {
        _curSlotType = slotType;
        _slotIdx = slotIdx;
        _itemDataId = itemDataId;
        // 이미지와 가격 텍스트는 아이템 아이디로 가져온 정보로 초기화해준다.
    }



    // ======================== 슬롯 팝업 관련 메서드 ========================
    //현재 테스트 코드 적용중. 추후 수정 필요.
    public void OnPointerEnter(PointerEventData eventData)
    {
        ItemData testData;

        if (ItemData != null)
        {
            testData = ItemData;
        }
        else
        {
            testData = new ItemData
            {
                ItemName = "테스트 아이템",
                ItemDescription = "아이템 데이터가 없을 때 표시되는 테스트 팝업입니다.",
                SellingPrice = 100
            };
        }

        OnHoverEnter?.Invoke(testData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnHoverExit?.Invoke();
    }

}
