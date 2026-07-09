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

    [SerializeField] private Transform Transform_InventoryContent;
    [SerializeField] private Transform Transform_StashContent;
    [SerializeField] private Transform Transform_ShopContent;

    private ShopViewModel _vm;
    private ShopItemSlotUI _draggedSlot;

    public void BindViewModel(ShopViewModel shopViewModel)
    {
        _vm = shopViewModel;
        _vm.PropertyChanged += OnPropertyChanged_View;
        _vm.InvokeOnceOnInit();
    }

    private void Awake()
    {
        if (Button_CloseSelf != null)
        {
            Button_CloseSelf.onClick.AddListener(CloseShopUI);
        }
    }

    private void OnDisable()
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
        }
    }

    public void CloseShopUI()
    {
        UIManager.Instance.CloseContentUI(UIType.ShopUI);

        if (ShopItemPopupUI.Inst != null)
        {
            ShopItemPopupUI.Inst.HidePopup();
        }
    }
}
