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
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                PickupOne(clickedSlot);
            }
            else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                PickupHalf(clickedSlot);
            }
            else
            {
                PickupAll(clickedSlot);
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                // 다른 아이템에 복사되는 버그 방지 조건
                if (clickedSlot.ItemDataId == _dragSlotVm.ItemDataId)
                {
                    PickupOne(clickedSlot);
                }
            }
            else if (clickedSlot.IsSlotEmpty)
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

    private void HandleRightClick(StashItemSlotViewModel clickedSlot)
    {
        if (_heldStackCount == 0)
        {
            return;
        }
        else
        {
            if (clickedSlot.IsSlotEmpty || clickedSlot.ItemDataId == _dragSlotVm.ItemDataId)
            {
                PlaceOne(clickedSlot);
            }
        }
    }

    // ==========================================
    // 아래는 위 조작을 수행하는 헬퍼 메서드들입니다.
    // ==========================================

    private void PickupOne(StashItemSlotViewModel slotVm)
    {
        if (slotVm == null || slotVm.IsSlotEmpty) return;

        if (_heldStackCount == 0)
        {
            _originSlotVm = slotVm;
            DragSlotUI.gameObject.SetActive(true);

            _dragSlotVm.ItemDataId = slotVm.ItemDataId;
            _dragSlotVm.ItemUniqueId = slotVm.ItemUniqueId;
            _dragSlotVm.IsSlotEmpty = false;
        }

        _heldStackCount++;
        _dragSlotVm.ItemStackCount = _heldStackCount; // 드래그 슬롯 UI 실시간 갱신
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

    private void PickupHalf(StashItemSlotViewModel slotVm)
    {
        if (slotVm == null || slotVm.IsSlotEmpty)
        {
            return;
        }

        int halfAmount = Mathf.CeilToInt(slotVm.ItemStackCount / 2.0f);
        _heldStackCount = halfAmount;
        _originSlotVm = slotVm;

        DragSlotUI.gameObject.SetActive(true);
        _dragSlotVm.ItemDataId = slotVm.ItemDataId;
        _dragSlotVm.ItemUniqueId = slotVm.ItemUniqueId;
        _dragSlotVm.ItemStackCount = _heldStackCount;
        _dragSlotVm.IsSlotEmpty = false;

        slotVm.ItemStackCount -= halfAmount;

        if (slotVm.ItemStackCount == 0)
        {
            ClearSlotData(slotVm);
        }
    }

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
        string tempId = targetSlot.ItemDataId;
        string tempUniqueId = targetSlot.ItemUniqueId;
        int tempCount = targetSlot.ItemStackCount;

        targetSlot.ItemDataId = _dragSlotVm.ItemDataId;
        targetSlot.ItemUniqueId = _dragSlotVm.ItemUniqueId;
        targetSlot.ItemStackCount = _heldStackCount;

        _dragSlotVm.ItemDataId = tempId;
        _dragSlotVm.ItemUniqueId = tempUniqueId;
        _heldStackCount = tempCount;
        _dragSlotVm.ItemStackCount = _heldStackCount;

        _originSlotVm = targetSlot;
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
