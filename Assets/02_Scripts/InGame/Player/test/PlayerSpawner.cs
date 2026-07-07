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

        PlayerCinemachineCamera.Bind(player.transform);
    }


}
