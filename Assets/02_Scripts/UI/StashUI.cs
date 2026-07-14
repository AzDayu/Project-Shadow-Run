using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StashUI : UIBase
{
    [SerializeField] private TMP_Text Text_CurPlayerCredit;
    [SerializeField] private Image Image_DragIcon;
    [SerializeField] private Button Button_CloseSelf;
    [SerializeField] private ShopItemPopupUI ShopItemPopup; 

    [SerializeField] private StashItemSlotUI Prefab_StashItemSlotUI; 
    [SerializeField] private Transform Transform_InventoryContent;
    [SerializeField] private Transform Transform_StashContent;

    private List<StashItemSlotUI> _slotUIList = new List<StashItemSlotUI>();

    private StashViewModel _stashVm;

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
        }
    }

    private void SetStashItemSlotOnEnable()
    {
        var stashVm = NetworkManager.Inst.StashService.GetStashViewModel();
        _stashVm = stashVm;
        _stashVm.PropertyChanged += OnPropChanged_View;
        _stashVm.InvokeOnceOnInit();
        InitStashSlotUIs();
    }

    private void InitStashSlotUIs()
    {
        // 이미 슬롯 UI를 생성했다면 바인딩만 다시 해주거나 스킵합니다.
        if (_slotUIList.Count == 0)
        {
            // ViewModel에 미리 정의된 60개의 빈 슬롯 데이터를 기반으로 UI를 생성합니다.
            foreach (var slotVm in _stashVm.StashSlots)
            {
                var slotUI = Instantiate(Prefab_StashItemSlotUI, Transform_StashContent);

                // 생성한 UI에 ViewModel을 묶어줍니다 (이 순간 UI가 비어있는 상태로 세팅됨)
                slotUI.Bind(slotVm, OnSlotHoverEnter, OnSlotHoverExit);

                _slotUIList.Add(slotUI);
            }
        }
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
}
