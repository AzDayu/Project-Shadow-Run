using System.Runtime.Serialization;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private CameraBinder PlayerCinemachineCamera;

    void Start()
    {
        GameObject player = GameObjectManager.Instance.SpawnObject(PlayerPrefab, this.transform.position, this.transform.rotation);
        if(player == null)
        {
            Debug.LogError("PlayerPrefab이 할당되지 않았거나 생성에 실패했습니다.");
            return;
        }

        Transform cameraTarget = player.GetComponent<PlayerSight>().GetPlayerSightTransform();

        PlayerCinemachineCamera.Bind(cameraTarget);
    }
}
