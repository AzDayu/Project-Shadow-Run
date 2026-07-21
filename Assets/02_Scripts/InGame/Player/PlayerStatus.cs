using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    public static PlayerStatus Instance { get; set; }

    [Header("Initial Status")]
    [SerializeField] private float MaxHP = 100f;
    [SerializeField] private float MaxStamina = 100f;

    public PlayerModel Model { get; private set; }
    public PlayerStatusViewModel ViewModel { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    private void InitPlayerStatus()
    {
        Model = new PlayerModel();
        Model.MaxHP = MaxHP;
        Model.MaxStamina = MaxStamina;

        ViewModel = new PlayerStatusViewModel();
        ViewModel.InitPlayerViewModel(Model);
    }

}
