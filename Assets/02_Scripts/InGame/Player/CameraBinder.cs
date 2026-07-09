using UnityEngine;
using Unity.Cinemachine;

public class CameraBinder : MonoBehaviour
{
    [SerializeField] private CinemachineCamera PlayerCinemachineCamera;

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