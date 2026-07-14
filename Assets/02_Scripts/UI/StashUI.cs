using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StashUI : UIBase
{
    [SerializeField] private TMP_Text Text_CurPlayerCredit;
    [SerializeField] private Button Button_CloseSelf;
    [SerializeField] private ShopItemPopupUI ShopItemPopup; 

    [SerializeField] private StashItemSlotUI Prefab_StashItemSlotUI; 
    [SerializeField] private Transform Transform_InventoryContent;
    [SerializeField] private Transform Transform_StashContent;

    [SerializeField] private StashItemSlotUI DragSlotUI;

    private StashItemSlotViewModel _originSlotVm; 
    private int _heldStackCount = 0;

    private StashItemSlotViewModel _dragSlotVm;

    private List<StashItemSlotUI> _stashSlotUIList = new List<StashItemSlotUI>();
    private List<StashItemSlotUI> _invenSlotUIList = new List<StashItemSlotUI>();


    private StashViewModel _stashVm;
    //private InventoryViewModel _inventoryVm;

    private void OnEnable()
    {
        Button_CloseSelf.onClick.RemoveAllListeners();
        Button_CloseSelf.onClick.AddListener(OnClick_CloseButton);
        SetStashItemSlotOnEnable();
    }

    private void OnDisable()
    {
        if (_stashVm != null)
        {
            _stashVm.PropertyChanged -= OnPropChanged_View;
            //if (_inventoryVm != null) _inventoryVm.PropertyChanged -= OnPropChanged_View;
        }
    }

    private void Update()
    {
        // 들고 있는 아이템이 있다면 마우스 위치로 이동
        if (!_dragSlotVm.IsSlotEmpty)
        {
            DragSlotUI.transform.position = Input.mousePosition;
        }
    }

    private void SetStashItemSlotOnEnable()
    {
        var stashVm = NetworkManager.Inst.StashService.GetStashViewModel();
        _stashVm = stashVm;
        _stashVm.PropertyChanged += OnPropChanged_View;
        _stashVm.InvokeOnceOnInit();

        //_inventoryVm = NetworkManager.Inst.InventoryService.GetInventoryViewModel();
        //_inventoryVm.PropertyChanged += OnPropChanged_View;
        //_inventoryVm.InvokeOnceOnInit();

        if (_dragSlotVm == null)
        {
            _dragSlotVm = new StashItemSlotViewModel { IsSlotEmpty = true };
            // 마우스 커서 슬롯은 호버나 클릭 이벤트가 필요 없으므로 null을 넘김.
            DragSlotUI.Bind(_dragSlotVm, null, null, null);
        }

        InitStashSlotUIs();
    }

    private void InitStashSlotUIs()
    {
        // 이미 슬롯 UI를 생성했다면 바인딩만 다시 해주거나 스킵.
        if (_stashSlotUIList.Count == 0)
        {
            foreach (var slotVm in _stashVm.StashSlots)
            {
                var slotUI = Instantiate(Prefab_StashItemSlotUI, Transform_StashContent);

                slotUI.Bind(slotVm, OnSlotHoverEnter, OnSlotHoverExit, OnSlotClicked);

                _stashSlotUIList.Add(slotUI);
            }
        }

        //if (_inventorySlotUIList.Count == 0)
        //{
        //    foreach (var slotVm in _inventoryVm.InventorySlots) // ⭐ 인벤토리 VM에서 데이터 가져옴
        //    {
        //        var slotUI = Instantiate(Prefab_ShopItemSlotUI, Transform_InventoryContent);
        //        slotUI.Bind(slotVm, OnSlotHoverEnter, OnSlotHoverExit, OnSlotClicked);
        //        _inventorySlotUIList.Add(slotUI);
        //    }
        //}
    }

    private void OnPropChanged_View(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(StashViewModel.CurPlayerCredit):
                {
                    Text_CurPlayerCredit.text = $"Player Credit : {_stashVm.CurPlayerCredit}";
                }
                break;
            case nameof(StashViewModel.HoveredItemId):
                if (_stashVm.HoveredItemId != null)
                {
                    ShopItemPopup.SetItemData(_stashVm.HoveredItemId);
                }
                else
                {
                    ShopItemPopup.HidePopup();
                }
                break;
        }
    }

    private void OnSlotClicked(StashItemSlotViewModel clickedSlotVm, PointerEventData.InputButton button)
    {
        if (button == PointerEventData.InputButton.Left)
        {
            HandleLeftClick(clickedSlotVm);
        }
        else if (button == PointerEventData.InputButton.Right)
        {
            HandleRightClick(clickedSlotVm);
        }
    }

    private void HandleLeftClick(StashItemSlotViewModel clickedSlot)
    {
        // 1. 아무것도 안 들고 있는데 아이템이 있는 슬롯을 클릭 -> 1개 줍기
        if (_heldStackCount == 0 && !clickedSlot.IsSlotEmpty)
        {
            PickupItem(clickedSlot);
        }
        // 2. 이미 들고 있는데 원래 꺼냈던 슬롯을 또 클릭 -> 1개 더 줍기
        else if (_heldStackCount > 0 && clickedSlot == _originSlotVm)
        {
            // 원래 슬롯에 남은 갯수가 있어야만 더 주울 수 있음
            if (_originSlotVm.ItemStackCount > 0)
            {
                PickupItem(clickedSlot);
            }
        }
        // 3. 들고 있는데 '다른 슬롯(또는 빈 공간)'을 클릭 -> 해당 구역으로 내려놓기
        else if (_heldStackCount > 0 && clickedSlot != _originSlotVm)
        {
            DropItemToArea(clickedSlot.SlotType);
        }
    }

    private void HandleRightClick(StashItemSlotViewModel clickedSlot)
    {
        // 들고 있는 상태에서 우클릭 -> 1개씩 취소(원래 자리로 되돌려놓기)
        if (_heldStackCount > 0)
        {
            ReturnOneItemToOrigin();
        }
    }

    private void PickupItem(StashItemSlotViewModel slotVm)
    {
        if (_heldStackCount == 0)
        {
            _originSlotVm = slotVm;
            DragSlotUI.gameObject.SetActive(true);

            _dragSlotVm.ItemDataId = slotVm.ItemDataId;
            _dragSlotVm.ItemUniqueId = slotVm.ItemUniqueId;
        }

        // 마우스 스택 증가, 슬롯 스택 감소
        _heldStackCount++;
        _originSlotVm.ItemStackCount--;

        // 슬롯의 아이템을 다 집었다면 빈 슬롯 처리
        if (_originSlotVm.ItemStackCount == 0)
        {
            _originSlotVm.IsSlotEmpty = true;
        }

        _dragSlotVm.ItemStackCount = _heldStackCount;
        _dragSlotVm.IsSlotEmpty = false;
    }

    private void ReturnOneItemToOrigin()
    {
        _heldStackCount--;

        // 원래 자리가 비어있었다면 다시 채워줌
        if (_originSlotVm.IsSlotEmpty)
        {
            _originSlotVm.IsSlotEmpty = false;
        }

        _originSlotVm.ItemStackCount++;

        // 다 돌려놨으면 마우스 아이콘 숨김
        if (_heldStackCount == 0)
        {
            ClearCursorItem();
        }
        else
        {
            _dragSlotVm.ItemStackCount = _heldStackCount;
        }
    }

    private void DropItemToArea(ShopItemSlotType targetAreaType)
    {
        // 확장을 위해 클릭한 구역이 인벤토리인지, 보관함인지 판단.
        var targetSlots = _stashVm.StashSlots; //(targetAreaType == ShopItemSlotType.Inventory) ? _inventoryVm.InventorySlots : _stashVm.StashSlots;

        // 해당 구역에서 빈 슬롯 찾기
        foreach (var slot in targetSlots)
        {
            if (slot.IsSlotEmpty)
            {
                slot.ItemUniqueId = _originSlotVm.ItemUniqueId; 
                slot.ItemDataId = _originSlotVm.ItemDataId;
                slot.ItemStackCount = _heldStackCount;
                slot.IsSlotEmpty = false;

                // 원래 슬롯과의 연관성을 완전히 끊고 마우스 초기화
                ClearCursorItem();
                return;
            }
        }

        // 대상 구역에 빈 슬롯이 없다면 취소(원래 자리로 복귀).
        Debug.LogWarning("해당 구역에 빈 슬롯이 없습니다!");
        while (_heldStackCount > 0)
        {
            ReturnOneItemToOrigin();
        }
    }

    private void ClearCursorItem()
    {
        _originSlotVm = null;
        _heldStackCount = 0;

        _dragSlotVm.IsSlotEmpty = true;
        DragSlotUI.gameObject.SetActive(false);
    }

    private void OnClick_CloseButton()
    {
        CloseStashUI();
    }

    public void CloseStashUI()
    {
        UIManager.Instance.CloseContentUI(UIType.StashUI);

        if (ShopItemPopup != null)
        {
            ShopItemPopup.HidePopup();
        }
    }

    private void OnSlotHoverEnter(string dataId)
    {
        _stashVm.HoveredItemId = dataId;
    }

    private void OnSlotHoverExit()
    {
        _stashVm.HoveredItemId = null;
    }
}
