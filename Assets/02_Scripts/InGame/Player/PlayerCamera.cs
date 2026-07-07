using UnityEngine;
using Unity.Cinemachine;

public class CameraBinder : MonoBehaviour
{
    [SerializeField] private CinemachineCamera PlayerCinemachineCamera;

    // Test: 실제로는 Bind()를 플레이어 생성부분에서 호출해야 함
    [SerializeField] private Transform PlayerTransfrom;

    // Test: 실제로는 Bind()를 플레이어 생성부분에서 호출해야 함
    private void Start()
    {
        Bind(PlayerTransfrom);
    }

    public void Bind(Transform cameraTarget)
    {
        if (PlayerCinemachineCamera == null)
        {
            Debug.LogError("CinemachineCamera가 할당되지 않았습니다.");
            return;
        }

        if (cameraTarget == null)
        {
            Debug.LogError("CameraTarget이 없습니다.");
            return;
        }

        PlayerCinemachineCamera.Follow = cameraTarget;
        PlayerCinemachineCamera.LookAt = cameraTarget;
    }
}