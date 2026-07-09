using UnityEngine;

public class ShopObj : MonoBehaviour
{
    [SerializeField] private ShopUI _shopUI; //지금은 직접참조로 작동만 확인. 추후 UIManager 만들어지면 동적생성으로 뺄 것. Close도 마찬가지

    [SerializeField] private KeyCode _interactKey = KeyCode.E;

    private bool _isPlayerInside = false;
    //private PlayerController _playerController; 나중에 플레이어 컨트롤러가 추가되면 주석해제. 플레이어가 상점UI를 열고있을 때 플레이어의 움직임을 막기 위함.

    private void Update()
    {
        if (_isPlayerInside && Input.GetKeyDown(_interactKey))
        {
            if (_shopUI.gameObject.activeSelf)
            {
                return;
            }
            else
            {
                OpenShop();
            }
        }

        if (_shopUI != null && _shopUI.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInside = true;
            //_playerController = other.GetComponent<PlayerController>(); 
            Debug.Log("ShopObj: 플레이어가 상점 범위에 진입했습니다.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 플레이어가 영역 밖으로 나가면 상점 닫기 및 감지 해제
        if (other.CompareTag("Player"))
        {
            _isPlayerInside = false;
            Debug.Log("ShopObj: 플레이어가 상점 범위를 벗어났습니다.");

            CloseShop();
        }
    }

    private void OpenShop()
    {
        if (_shopUI != null)
        {
            _shopUI.gameObject.SetActive(true);

            // ViewModel 바인딩이 필요한 경우 여기서 처리하거나 ShopUI 내부 OnEnable에서 처리
            _shopUI.BindViewModel(new ShopViewModel());
        }
        else
        {
            Debug.LogWarning("ShopObj: 연결된 ShopUI가 없습니다!");
        }
    }

    private void CloseShop()
    {
        if (_shopUI != null && _shopUI.gameObject.activeSelf)
        {
            _shopUI.gameObject.SetActive(false);
        }
    }
}
