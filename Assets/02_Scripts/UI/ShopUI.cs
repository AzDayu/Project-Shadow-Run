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
    [SerializeField] private ShopItemSlotUI Prefab_ShopItemSlotUI;
    [SerializeField] private TMP_Text Text_CurPlayerCredit;
    [SerializeField] private Image Image_DragIcon;
    [SerializeField] private Button Button_CloseSelf;
    [SerializeField] private ShopItemPopupUI ShopItemPopup;

    [SerializeField] private Transform Transform_InventoryContent;
    [SerializeField] private Transform Transform_StashContent;
    [SerializeField] private Transform Transform_ShopContent;

    private ShopViewModel _vm;

    public void BindViewModel(ShopViewModel vm)
    {
        if (_vm != null)
        {
            _vm.PropertyChanged -= OnPropertyChanged_View;
        }

        _vm = vm;
        _vm.PropertyChanged += OnPropertyChanged_View;
        _vm.InvokeOnceOnInit();
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
