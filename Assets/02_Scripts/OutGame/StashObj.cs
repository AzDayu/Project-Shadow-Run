using UnityEngine;

public class StashObj : MonoBehaviour
{
    [SerializeField] private KeyCode _interactKey = KeyCode.T;

    private bool _isPlayerInside = false;
    //private PlayerController _playerController; 나중에 플레이어 컨트롤러가 추가되면 주석해제. 플레이어가 상점UI를 열고있을 때 플레이어의 움직임을 막기 위함.

    private void Update()
    {
        if (/*_isPlayerInside &&*/ Input.GetKeyDown(_interactKey))
        {
            if (UIManager.Instance.IsUIOpened(UIType.StashUI))
            {
                Debug.Log("StashObj: 창고 UI가 이미 열려 있습니다.");

                return;
            }
            {
                OpenStash();
                Debug.Log("StashObj: 창고 UI를 열었습니다.");
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIManager.Instance.IsUIOpened(UIType.StashUI))
            {
                CloseStash();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInside = true;
            //_playerController = other.GetComponent<PlayerController>(); 
            Debug.Log("StashObj: 플레이어가 창고 범위에 진입했습니다.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 플레이어가 영역 밖으로 나가면 상점 닫기 및 감지 해제
        if (other.CompareTag("Player"))
        {
            _isPlayerInside = false;
            Debug.Log("StashObj: 플레이어가 창고 범위를 벗어났습니다.");

            CloseStash();
        }
    }

    private void OpenStash()
    {
        UIManager.Instance.OpenContentUI(UIType.ShopUI);

        var stashUI = UIManager.Instance.GetOpenedUI(UIRootType.ContentUI, UIType.StashUI) as StashUI;
        if (stashUI != null)
        {
            //stashUI.BindViewModel(new StashViewModel());
            Debug.Log("ViewModel 바인딩 성공!");
        }
        else
        {
            Debug.LogError("stashUI를 가져오지 못했습니다!");
        }
    }

    private void CloseStash()
    {
        UIManager.Instance.CloseUI(UIRootType.ContentUI, UIType.StashUI);
    }
}
