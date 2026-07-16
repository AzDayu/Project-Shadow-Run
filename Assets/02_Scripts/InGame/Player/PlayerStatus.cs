using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("Initial Status")]
    [SerializeField] private float MaxHP = 100f;
    [SerializeField] private float MaxStamina = 100f;

    public PlayerStatusModel Model { get; private set; }
    public PlayerStatusViewModel ViewModel { get; private set; }

    private void Awake()
    {
        InitPlayerStatus();
    }

    private void InitPlayerStatus()
    {
        Model = new PlayerStatusModel();
        Model.InitPlayer(MaxHP, MaxStamina);

        ViewModel = new PlayerStatusViewModel();
        ViewModel.InitPlayerViewModel(Model);
    }

}
