using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItemSlotUI : UIBase, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image Image_ItemIcon;
    [SerializeField] private TMP_Text Text_ItemPrice;

    private ShopItemSlotViewModel _slotVm;
    public Action<string> _onHoverEnter;
    public Action _onHoverExit;

    public ShopItemSlotType _curSlotType { get; set; }
    public int _slotIdx { get; set; }
    public int _itemDataId { get; set; }

    public void Bind(ShopItemSlotViewModel slotVm, Action<string> onHoverEnter, Action onHoverExit)
    {
        if (_slotVm != null)
        {
            _slotVm.PropertyChanged -= OnSlotPropertyChanged;
        }

        _slotVm = slotVm;

        _onHoverEnter = onHoverEnter;
        _onHoverExit = onHoverExit;

        _slotVm.PropertyChanged += OnSlotPropertyChanged;
        UpdateSlotUI();
    }

    private void OnSlotPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // 슬롯 데이터가 바뀌면 UI 새로고침
        UpdateSlotUI();
    }

    private void UpdateSlotUI()
    {
        if (Text_ItemPrice == null || Image_ItemIcon == null) return;

        // 1. 빈 슬롯이거나 데이터가 없으면 투명하게 만들거나 숨기기
        if (_slotVm.IsSlotEmpty == true)
        {
            Image_ItemIcon.enabled = false;  // 아이콘 숨기기
            Text_ItemPrice.text = string.Empty; // 가격 글자 지우기
            return;
        }

        // 2. 아이템이 존재할 때 데이터 채우고 켜기
        Image_ItemIcon.enabled = true;

        if (_slotVm.SlotType == ShopItemSlotType.Shop)
            Text_ItemPrice.text = $"{_slotVm.ItemStackCount} Credit";
        else
            Text_ItemPrice.text = $"{_slotVm.ItemSellingPrice} Credit";

        // Image_ItemIcon.sprite = Resources.Load<Sprite>(_slotVm.ItemData.IconPath);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_slotVm != null && !_slotVm.IsSlotEmpty)
        {
            _onHoverEnter?.Invoke(_slotVm.ItemDataId);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _onHoverExit?.Invoke();
    }

    private void OnDestroy()
    {
        if (_slotVm != null) _slotVm.PropertyChanged -= OnSlotPropertyChanged;
    }

}
