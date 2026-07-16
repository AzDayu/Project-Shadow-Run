using System.Runtime.Serialization;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private CameraBinder PlayerCinemachineCamera;

    private void Start()
    {
        GameObject player = GameObjectManager.Instance.SpawnObject(
            PlayerPrefab,
            transform.position,
            transform.rotation
        );

        if (player == null)
        {
            Debug.LogError("PlayerPrefab이 할당되지 않았거나 생성에 실패했습니다.");

            return;
        }

        BindPlayerCamera(player);
        BindPlayerHUD(player);
    }

    private void BindPlayerCamera(GameObject player)
    {
        PlayerSight playerSight =
            player.GetComponent<PlayerSight>();

        if (playerSight == null)
        {
            Debug.LogError("생성된 Player에 PlayerSight가 없습니다.");

            return;
        }

        Transform cameraTarget = playerSight.GetPlayerSightTransform();

        PlayerCinemachineCamera.Bind(cameraTarget);
    }

    private void BindPlayerHUD(GameObject player)
    {
        PlayerStatus playerStatus = player.GetComponent<PlayerStatus>();

        if (playerStatus == null)
        {
            Debug.LogError("생성된 Player에 PlayerStatus가 없습니다.");

            return;
        }

        if (UIManager.Instance == null)
        {
            Debug.LogError("UIManager 인스턴스가 존재하지 않습니다.");

            return;
        }

        UIBase uiBase = UIManager.Instance.OpenUI(UIRootType.MainUI, UIType.HudUI);

        PlayerHUDView hudView = uiBase as PlayerHUDView;

        if (hudView == null)
        {
            Debug.LogError("HudUI 루트에 PlayerHUDView가 없습니다.");

            return;
        }

        hudView.BindViewModel(playerStatus.ViewModel);
    }
}
