using UnityEngine;

public class ShopViewModel : ViewModelBase
{
    public void InvokeOnceOnInit()
    {
        OnPropertyChanged(nameof(CurPlayerCredit));
    }

    private int _curPlayerCredit;
    public int CurPlayerCredit
    {
        get => _curPlayerCredit;
        set
        {
            if (_curPlayerCredit != value)
            {
                _curPlayerCredit = value;
                OnPropertyChanged(nameof(CurPlayerCredit));
            }
        }
    }




}
