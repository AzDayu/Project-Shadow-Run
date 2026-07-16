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
    private StashItemSlotViewModel _dragSlotVm;
    private int _heldStackCount = 0;


    private List<StashItemSlotUI> _stashSlotUIList = new List<StashItemSlotUI>();
    private List<StashItemSlotUI> _invenSlotUIList = new List<StashItemSlotUI>();

    private StashViewModel _stashVm;
    //private InventoryViewModel _inventoryVm;

    private void OnEnable()
    {
        Button_CloseSelf.onClick.RemoveAllListeners();
        Button_CloseSelf.onClick.AddListener(OnClick_CloseButton);
        BindViewModel();
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
        if (!_dragSlotVm.IsSlotEmpty)
        {
            DragSlotUI.transform.position = Input.mousePosition;
        }
    }

    private void BindViewModel()
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
        if (_heldStackCount == 0)
        {
            // 1. 맨손일 때: 클릭한 슬롯의 아이템 '전부' 집어들기
            if (!clickedSlot.IsSlotEmpty)
            {
                PickupAll(clickedSlot);
            }
        }
        else
        {
            // 2. 아이템을 들고 있을 때
            if (clickedSlot.IsSlotEmpty)
            {
                // 2-1. 빈 슬롯: 전부 내려놓기
                PlaceAll(clickedSlot);
            }
            else if (clickedSlot.ItemDataId == _dragSlotVm.ItemDataId)
            {
                // 2-2. 같은 아이템: 전부 합치기
                MergeAll(clickedSlot);
            }
            else
            {
                // 2-3. 다른 아이템: 서로 스왑(교체)하기
                SwapItems(clickedSlot);
            }
        }
    }

    private void HandleRightClick(StashItemSlotViewModel clickedSlot)
    {
        if (_heldStackCount == 0)
        {
            // 1. 맨손일 때: 클릭한 슬롯에서 '1개만' 집기
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
                PickupOne(clickedSlot);
            }
        }
    }

    // ==========================================
    // 아래는 위 조작을 수행하는 헬퍼 메서드들입니다.
    // ==========================================

    private void PickupAll(StashItemSlotViewModel slotVm)
    {
        _heldStackCount = slotVm.ItemStackCount;
        _originSlotVm = slotVm;

        DragSlotUI.gameObject.SetActive(true);
        _dragSlotVm.ItemDataId = slotVm.ItemDataId;
        _dragSlotVm.ItemUniqueId = slotVm.ItemUniqueId;
        _dragSlotVm.ItemStackCount = _heldStackCount;
        _dragSlotVm.IsSlotEmpty = false;

        ClearSlotData(slotVm);
    }

    private void PlaceAll(StashItemSlotViewModel targetSlot)
    {
        targetSlot.ItemDataId = _dragSlotVm.ItemDataId;
        targetSlot.ItemUniqueId = _dragSlotVm.ItemUniqueId;
        targetSlot.ItemStackCount = _heldStackCount;
        targetSlot.IsSlotEmpty = false;

        ClearCursorItem();
    }

    private void MergeAll(StashItemSlotViewModel targetSlot)
    {
        // TODO: 향후 GameDataManager를 통해 ItemData의 MaxStackSize를 가져와 한계치까지만 합치고 남은 건 마우스에 남기는 로직을 추가할 수 있음.

        targetSlot.ItemStackCount += _heldStackCount;
        ClearCursorItem();
    }

    private void SwapItems(StashItemSlotViewModel targetSlot)
    {
        // 타겟 슬롯의 데이터 임시 저장
        string tempId = targetSlot.ItemDataId;
        string tempUniqueId = targetSlot.ItemUniqueId;
        int tempCount = targetSlot.ItemStackCount;

        // 타겟 슬롯에 마우스 데이터 덮어쓰기
        targetSlot.ItemDataId = _dragSlotVm.ItemDataId;
        targetSlot.ItemUniqueId = _dragSlotVm.ItemUniqueId;
        targetSlot.ItemStackCount = _heldStackCount;

        // 마우스 커서에 임시 저장한 타겟 데이터 넣기
        _dragSlotVm.ItemDataId = tempId;
        _dragSlotVm.ItemUniqueId = tempUniqueId;
        _heldStackCount = tempCount;
        _dragSlotVm.ItemStackCount = _heldStackCount;

        _originSlotVm = targetSlot;
    }

    private void PickupOne(StashItemSlotViewModel slotVm)
    {
        if (_heldStackCount == 0)
        {
            _originSlotVm = slotVm;
            _dragSlotVm.ItemDataId = slotVm.ItemDataId;
            _dragSlotVm.ItemUniqueId = slotVm.ItemUniqueId;
            _dragSlotVm.IsSlotEmpty = false;
            DragSlotUI.gameObject.SetActive(true);
        }

        _heldStackCount++;
        _dragSlotVm.ItemStackCount = _heldStackCount;
        slotVm.ItemStackCount--;

        if (slotVm.ItemStackCount == 0)
        {
            ClearSlotData(slotVm);
        }
    }

    private void PlaceOne(StashItemSlotViewModel targetSlot)
    {
        if (targetSlot.IsSlotEmpty)
        {
            targetSlot.ItemDataId = _dragSlotVm.ItemDataId;
            targetSlot.ItemUniqueId = _dragSlotVm.ItemUniqueId;
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
        _dragSlotVm.ItemDataId = string.Empty;
        _dragSlotVm.ItemUniqueId = string.Empty;
        _dragSlotVm.ItemStackCount = 0;
        _dragSlotVm.IsSlotEmpty = true;
        DragSlotUI.gameObject.SetActive(false);
    }

    private void ClearSlotData(StashItemSlotViewModel slotVm)
    {
        slotVm.IsSlotEmpty = true;
        slotVm.ItemDataId = string.Empty;
        slotVm.ItemUniqueId = string.Empty;
        slotVm.ItemStackCount = 0;
    }
}
