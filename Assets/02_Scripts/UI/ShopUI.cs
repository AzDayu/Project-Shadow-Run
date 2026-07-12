using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
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
    [SerializeField] private Image Image_DragIcon;
    [SerializeField] private Button Button_CloseSelf;
    [SerializeField] private ShopItemPopupUI ShopItemPopup;

    [SerializeField] private ShopItemSlotUI Prefab_ShopItemSlotUI;
    [SerializeField] private Transform Transform_ShopContent;
    [SerializeField] private Transform Transform_InventoryContent;
    [SerializeField] private Transform Transform_StashContent;

    private ShopViewModel _vm;
    private List<ShopItemSlotUI> _instantiatedSlots = new List<ShopItemSlotUI>();
    private bool _isInitialized = false;

    public void BindViewModel(ShopViewModel vm)
    {
        if (_vm != null)
        {
            _vm.PropertyChanged -= OnPropertyChanged_View;
        }

        _vm = vm;
        _vm.PropertyChanged += OnPropertyChanged_View;

        if (!_isInitialized)
        {
            SpawnSlotsZone(_vm.ShopItemSlotList, Transform_ShopContent);
            SpawnSlotsZone(_vm.InventoryItemSlotList, Transform_InventoryContent);
            SpawnSlotsZone(_vm.StashItemSlotList, Transform_StashContent);
            _isInitialized = true;
        }
        else
        {
            // [개선] 이미 UI가 있다면 데이터만 새로 매핑하여 UI를 갱신합니다.
            RebindSlotsZone(_vm.ShopItemSlotList, Transform_ShopContent);
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
        // parentContent 자식으로 붙어있는 ShopItemSlotUI들을 순색하며 새로 바인딩
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
            case nameof(ShopViewModel.CurPlayerCredit):
                {
                    Text_CurPlayerCredit.text = $"Player Credit : {_vm.CurPlayerCredit}";
                }
                break;
            case nameof(ShopViewModel.HoveredItem):
                //테스트용으로 임시 작성한 코드. 아이템 데이터 만들어지면 즉시 수정 필요.
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
        CloseShopUI();
    }

    public void CloseShopUI()
    {
        UIManager.Instance.CloseContentUI(UIType.ShopUI);

        if (ShopItemPopup != null)
        {
            ShopItemPopup.HidePopup();
        }
    }

}
