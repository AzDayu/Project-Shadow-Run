using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ShopItemSlotType
{
    Inventory,
    Stash,
    Shop
}

public class ShopUI : UIBase 
{
    [SerializeField] private TMP_Text Text_CurPlayerCredit;
    [SerializeField] private Button Button_CloseSelf;
    [SerializeField] private ShopItemPopupUI ShopItemPopup;

    [SerializeField] private ShopItemSlotUI Prefab_ShopItemSlotUI;
    [SerializeField] private Transform Transform_ShopContent;
    [SerializeField] private Transform Transform_InventoryContent;
    [SerializeField] private Transform Transform_StashContent;

    [SerializeField] private ShopItemSlotUI DragSlotUI;

    private ShopItemSlotViewModel _originSlotVm;
    private ShopItemSlotViewModel _dragSlotVm;
    private int _heldStackCount = 0;

    private ShopViewModel _shopVm;

    private bool _isInitialized = false;

    private void OnEnable()
    {
        Button_CloseSelf.onClick.RemoveAllListeners();
        Button_CloseSelf.onClick.AddListener(OnClick_CloseButton);
    }

    private void OnDestroy()
    {
        if (_shopVm != null)
        {
            _shopVm.PropertyChanged -= OnPropertyChanged_View;
        }
    }

    private void Update()
    {
        if (_dragSlotVm != null && !_dragSlotVm.IsSlotEmpty)
        {
            DragSlotUI.transform.position = Input.mousePosition;
        }
    }

    private void OnPropertyChanged_View(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ShopViewModel.CurPlayerCredit):
                {
                    Text_CurPlayerCredit.text = $"Player Credit : {_shopVm.CurPlayerCredit}";
                }
                break;
            case nameof(ShopViewModel.HoveredItemId):
                if (_shopVm.HoveredItemId != null)
                {
                    //ShopItemPopup.SetItemData(_shopVm.HoveredItemId);
                }
                else
                {
                    //ShopItemPopup.HidePopup();
                }
                break;
        }
    }

    public void BindViewModel(ShopViewModel vm)
    {
        if(_shopVm != null)
        {
            _shopVm.PropertyChanged -= OnPropertyChanged_View;
        }

        _shopVm = vm;
        _shopVm.PropertyChanged += OnPropertyChanged_View;

        if(_dragSlotVm == null)
        {
            _dragSlotVm = new ShopItemSlotViewModel { IsSlotEmpty = true };
            DragSlotUI.Bind(_dragSlotVm, null, null, null);
        }

        if (!_isInitialized)
        {
            InitSlotsZone(_shopVm.ShopItemSlotList, Transform_ShopContent);
            InitSlotsZone(_shopVm.InventoryItemSlotList, Transform_InventoryContent);
            InitSlotsZone(_shopVm.StashItemSlotList, Transform_StashContent);
            _isInitialized = true;
        }

        _shopVm.InvokeOnceOnInit();
    }

    private void InitSlotsZone(List<ShopItemSlotViewModel> slotVms, Transform parentContent)
    {
        foreach (var slotVm in slotVms)
        {
            ShopItemSlotUI slotUi = Instantiate(Prefab_ShopItemSlotUI, parentContent);
            slotUi.Bind(slotVm, _shopVm.OnSlotPointerEnter, _shopVm.OnSlotPointerExit, OnSlotClicked);
        }
    }


    private void OnClick_CloseButton()
    {
        CloseShopUI();
    }

    public void CloseShopUI()
    {
        UIManager.Instance.CloseContentUI(UIType.ShopUI);

        //if (ShopItemPopup != null)
        //{
        //    ShopItemPopup.HidePopup();
        //}
    }

    // ==========================================
    // 마우스 클릭 및 드래그 앤 드롭 로직 (창고와 동일)
    // ==========================================
    private void OnSlotClicked(ShopItemSlotViewModel clickedSlotVm, PointerEventData.InputButton button)
    {
        if (button == PointerEventData.InputButton.Left)
        {
            HandleLeftClick(clickedSlotVm);
        }
        else if (button == PointerEventData.InputButton.Right)
        {
            HandleRightClick(clickedSlotVm);
        }
        DragSlotUI.UpdatePriceDisplay(_dragSlotVm.ItemSellingPrice, _heldStackCount);
    }

    private void HandleLeftClick(ShopItemSlotViewModel clickedSlot)
    {
        if (_heldStackCount == 0)
        {
            if (!clickedSlot.IsSlotEmpty)
            {
                PickupAll(clickedSlot);
            }
        }
        else
        {
            if (clickedSlot.IsSlotEmpty)
            {
                PlaceAll(clickedSlot);
            }
            else if (clickedSlot.ItemDataId == _dragSlotVm.ItemDataId)
            {
                MergeAll(clickedSlot);
            }
            else
            {
                SwapItems(clickedSlot);
            }
        }
    }

    private void HandleRightClick(ShopItemSlotViewModel clickedSlot)
    {
        if (_heldStackCount == 0)
        {
            if (!clickedSlot.IsSlotEmpty)
            {
                PickupOne(clickedSlot);
            }
        }
        else
        {
            if (clickedSlot.IsSlotEmpty)
            {
                PlaceOne(clickedSlot);
            }
            else if (clickedSlot.ItemDataId == _dragSlotVm.ItemDataId)
            {
                PickupOne(clickedSlot); // 같은 아이템이면 더 집어오기
            }
        }


    }

    private void RestoreItemToOrigin()
    {
        if (_originSlotVm == null) return;

        _originSlotVm.IsSlotEmpty = false;
        _originSlotVm.ItemStackCount += _heldStackCount;

        _originSlotVm.ItemDataId = _dragSlotVm.ItemDataId;
        _originSlotVm.ItemSellingPrice = _dragSlotVm.ItemSellingPrice;
        
        ClearCursorItem();
    }

    private void PickupAll(ShopItemSlotViewModel slotVm)
    {
        _heldStackCount = slotVm.ItemStackCount;
        _originSlotVm = slotVm;

        DragSlotUI.gameObject.SetActive(true);

        _dragSlotVm.ItemDataId = slotVm.ItemDataId;
        _dragSlotVm.ItemUniqueId = slotVm.ItemUniqueId;
        _dragSlotVm.ItemSellingPrice = slotVm.ItemSellingPrice;
        _dragSlotVm.ItemStackCount = _heldStackCount;
        _dragSlotVm.IsSlotEmpty = false;


        ClearSlotData(slotVm);
    }

    private void PlaceAll(ShopItemSlotViewModel targetSlot)
    {
        // 1. 거래(구매) 시도 시 항상 DB에서 실시간 가격 조회
        var itemData = DataManager.Instance.GetItemData(_dragSlotVm.ItemDataId);
        if (itemData == null) 
        { 
            Debug.LogError("아이템 데이터 없음!"); RestoreItemToOrigin(); return; 
        }

        // ==========================================
        // 구매 로직
        // ==========================================
        if (_originSlotVm.SlotType == ShopItemSlotType.Shop && targetSlot.SlotType != ShopItemSlotType.Shop)
        {
            int totalPrice = itemData.SellingPrice * _heldStackCount; 

            if (_shopVm.CurPlayerCredit >= totalPrice)
            {
                _shopVm.CurPlayerCredit -= totalPrice;
                targetSlot.ItemUniqueId = System.Guid.NewGuid().ToString();
            }
            else
            {
                Debug.LogWarning("크레딧이 부족합니다!");
                RestoreItemToOrigin(); 
                return;
            }
        }
        // ==========================================
        // 판매 로직
        // ==========================================
        else if (_originSlotVm.SlotType != ShopItemSlotType.Shop && targetSlot.SlotType == ShopItemSlotType.Shop)
        {
            int earnCredit = itemData.SellingPrice * _heldStackCount;
            _shopVm.CurPlayerCredit += earnCredit;
            ClearCursorItem();
            return;
        }

        // 공통: 슬롯 데이터 적용
        targetSlot.ItemDataId = _dragSlotVm.ItemDataId;
        targetSlot.ItemSellingPrice = itemData.SellingPrice;
        targetSlot.ItemStackCount = _heldStackCount;
        targetSlot.IsSlotEmpty = false;

        ClearCursorItem();
    }

    private void MergeAll(ShopItemSlotViewModel targetSlot)
    {
        // 1. 상점 -> 유저 구역 (합치면서 구매)
        if (_originSlotVm.SlotType == ShopItemSlotType.Shop && targetSlot.SlotType != ShopItemSlotType.Shop)
        {
            int totalPrice = _dragSlotVm.ItemSellingPrice * _heldStackCount;
            if (_shopVm.CurPlayerCredit >= totalPrice)
            {
                _shopVm.CurPlayerCredit -= totalPrice;
                Debug.Log($"[{_dragSlotVm.ItemDataId}] 합치기 구매 완료! (-{totalPrice} C)");
            }
            else
            {
                Debug.LogWarning("크레딧이 부족합니다!");
                RestoreItemToOrigin();
                return;
            }
        }
        // 2. 유저 구역 -> 상점 (합치면서 판매)
        else if (_originSlotVm.SlotType != ShopItemSlotType.Shop && targetSlot.SlotType == ShopItemSlotType.Shop)
        {
            int earnCredit = _dragSlotVm.ItemSellingPrice * _heldStackCount;
            _shopVm.CurPlayerCredit += earnCredit;
            Debug.Log($"[{_dragSlotVm.ItemDataId}] 전체 판매 완료! (+{earnCredit} C)");
            ClearCursorItem();
            return;
        }

        targetSlot.ItemStackCount += _heldStackCount;
        ClearCursorItem();
    }

    private void SwapItems(ShopItemSlotViewModel targetSlot)
    {
        if (_originSlotVm.SlotType == ShopItemSlotType.Shop || targetSlot.SlotType == ShopItemSlotType.Shop)
        {
            Debug.LogWarning("상점 물품과는 위치를 스왑(맞바꾸기) 할 수 없습니다!");
            return; 
        }

        // 기존 인벤/창고 스왑 로직
        string tempId = targetSlot.ItemDataId;
        string tempUniqueId = targetSlot.ItemUniqueId;
        int tempCount = targetSlot.ItemStackCount;
        int tempPrice = targetSlot.ItemSellingPrice;

        targetSlot.ItemDataId = _dragSlotVm.ItemDataId;
        targetSlot.ItemUniqueId = _dragSlotVm.ItemUniqueId;
        targetSlot.ItemStackCount = _heldStackCount;
        targetSlot.ItemSellingPrice = _dragSlotVm.ItemSellingPrice;

        _dragSlotVm.ItemDataId = tempId;
        _dragSlotVm.ItemUniqueId = tempUniqueId;
        _heldStackCount = tempCount;
        _dragSlotVm.ItemStackCount = _heldStackCount;
        _dragSlotVm.ItemSellingPrice = tempPrice;
        _originSlotVm = targetSlot;
    }

    private void PickupOne(ShopItemSlotViewModel slotVm)
    {
        if (_heldStackCount == 0)
        {
            _originSlotVm = slotVm;

            DragSlotUI.gameObject.SetActive(true);

            _dragSlotVm.ItemDataId = slotVm.ItemDataId;
            _dragSlotVm.ItemSellingPrice = slotVm.ItemSellingPrice;
            _dragSlotVm.IsSlotEmpty = false;
        }

        _heldStackCount++;
        slotVm.ItemStackCount--;

        if (slotVm.ItemStackCount == 0)
            ClearSlotData(slotVm);
    }

    private void PlaceOne(ShopItemSlotViewModel targetSlot)
    {
        // ==========================================
        // 1. 상점 -> 유저 구역 (1개 구매)
        // ==========================================
        if (_originSlotVm.SlotType == ShopItemSlotType.Shop && targetSlot.SlotType != ShopItemSlotType.Shop)
        {
            int price = _dragSlotVm.ItemSellingPrice; // 1개 가격
            if (_shopVm.CurPlayerCredit >= price)
            {
                _shopVm.CurPlayerCredit -= price;
                Debug.Log($"[{_dragSlotVm.ItemDataId}] 1개 구매 완료! (-{price} C)");

                if (targetSlot.IsSlotEmpty)
                {
                    targetSlot.ItemUniqueId = System.Guid.NewGuid().ToString();
                }
            }
            else
            {
                Debug.LogWarning("크레딧이 부족합니다!");
                return; // 돈이 없으면 놓기 취소 (마우스에 든 채로 유지됨)
            }
        }
        // ==========================================
        // 2. 유저 구역 -> 상점 (1개 판매)
        // ==========================================
        else if (_originSlotVm.SlotType != ShopItemSlotType.Shop && targetSlot.SlotType == ShopItemSlotType.Shop)
        {
            int earnCredit = _dragSlotVm.ItemSellingPrice;
            _shopVm.CurPlayerCredit += earnCredit;
            Debug.Log($"[{_dragSlotVm.ItemDataId}] 1개 판매 완료! (+{earnCredit} C)");

            // 판매된 1개만 마우스에서 깎아내고 슬롯에는 추가하지 않음 (증발시킴)
            _heldStackCount--;

            if (_heldStackCount == 0)
            {
                ClearCursorItem();
            }
            else
            {
                _dragSlotVm.ItemStackCount = _heldStackCount;
            }

            return;
        }
        // ==========================================
        // 3. 단순 이동
        // ==========================================
        else
        {
            if (targetSlot.IsSlotEmpty)
            {
                targetSlot.ItemUniqueId = _dragSlotVm.ItemUniqueId;
            }
        }

        // 공통 데이터 덮어쓰기 (빈 슬롯일 경우)
        if (targetSlot.IsSlotEmpty)
        {
            targetSlot.ItemDataId = _dragSlotVm.ItemDataId;
            targetSlot.ItemSellingPrice = _dragSlotVm.ItemSellingPrice;
            targetSlot.ItemStackCount = 0;
            targetSlot.IsSlotEmpty = false;
        }

        targetSlot.ItemStackCount++;
        _heldStackCount--;

        if (_heldStackCount == 0)
        {
            ClearCursorItem();
        }
        else
        {
            _dragSlotVm.ItemStackCount = _heldStackCount;
        }
    }

    private void ClearCursorItem()
    {
        _heldStackCount = 0;
        _originSlotVm = null;
        ClearSlotData(_dragSlotVm);
        DragSlotUI.gameObject.SetActive(false);
    }

    private void ClearSlotData(ShopItemSlotViewModel slotVm)
    {
        slotVm.IsSlotEmpty = true;
        slotVm.ItemDataId = string.Empty;
        slotVm.ItemStackCount = 0;
        slotVm.ItemSellingPrice = 0;
    }
}
