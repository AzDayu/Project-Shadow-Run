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
    [SerializeField] private ShopItemPopupUI ShopItemPopup; //창고 전용 팝업을 따로 만들지 생각중

    [SerializeField] private ShopItemSlotUI Prefab_ShopItemSlotUI; //이건 확실하게 상정 슬롯과 분리해야할 듯(가격이 없는 버전으로)
    [SerializeField] private Transform Transform_InventoryContent;
    [SerializeField] private Transform Transform_StashContent;

    private StashViewModel _vm;
    private List<ShopItemSlotUI> _instantiatedSlots = new List<ShopItemSlotUI>();
    private bool _isInitialized = false;

    public void BindViewModel(StashViewModel vm)
    {
        if (_vm != null)
        {
            _vm.PropertyChanged -= OnPropertyChanged_View;
        }

        _vm = vm;
        _vm.PropertyChanged += OnPropertyChanged_View;

        if (!_isInitialized)
        {
            SpawnSlotsZone(_vm.InventoryItemSlotList, Transform_InventoryContent);
            SpawnSlotsZone(_vm.StashItemSlotList, Transform_StashContent);
            _isInitialized = true;
        }
        else
        {
            // [개선] 이미 UI가 있다면 데이터만 새로 매핑하여 UI를 갱신합니다.
            RebindSlotsZone(_vm.InventoryItemSlotList, Transform_InventoryContent);
            RebindSlotsZone(_vm.StashItemSlotList, Transform_StashContent);
        }

        _vm.InvokeOnceOnInit();
    }

    private void SpawnSlotsZone(List<ShopItemSlotViewModel> slotVms, Transform parentContent)
    {
        foreach (var slotVm in slotVms)
        {
            ShopItemSlotUI slotUi = Instantiate(Prefab_ShopItemSlotUI, parentContent);
            slotUi.Bind(slotVm, _vm.OnSlotPointerEnter, _vm.OnSlotPointerExit);
            _instantiatedSlots.Add(slotUi);
        }
    }

    private void RebindSlotsZone(List<ShopItemSlotViewModel> slotVms, Transform parentContent)
    {
        // parentContent 자식으로 붙어있는 ShopItemSlotUI들을 탐색하며 새로 바인딩
        int index = 0;
        foreach (Transform child in parentContent)
        {
            if (index >= slotVms.Count) break;

            if (child.TryGetComponent<ShopItemSlotUI>(out var slotUi))
            {
                slotUi.Bind(slotVms[index], _vm.OnSlotPointerEnter, _vm.OnSlotPointerExit);
                index++;
            }
        }
    }

    private void ClearSpawnedSlots()
    {
        foreach (var slot in _instantiatedSlots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        _instantiatedSlots.Clear();
    }

    private void OnEnable()
    {
        Button_CloseSelf.onClick.RemoveAllListeners();
        Debug.Log("버튼 이벤트 초기화");

        if (Button_CloseSelf != null)
        {
            Button_CloseSelf.onClick.AddListener(OnClick_CloseButton);
            Debug.Log("버튼이벤트 등록");
        }
    }

    private void OnDestroy()
    {
        if (_vm != null)
        {
            _vm.PropertyChanged -= OnPropertyChanged_View;
        }
    }

    private void OnPropertyChanged_View(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(StashViewModel.CurPlayerCredit):
                {
                    Text_CurPlayerCredit.text = $"Player Credit : {_vm.CurPlayerCredit}";
                }
                break;
            case nameof(StashViewModel.HoveredItem):
                if (_vm.HoveredItem != null)
                {
                    ShopItemPopup.SetItemData(_vm.HoveredItem);
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
}
