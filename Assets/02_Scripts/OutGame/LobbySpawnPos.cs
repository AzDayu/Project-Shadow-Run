using UnityEngine;

public class LobbySpawnPos : MonoBehaviour
{
    [Header("로비 스폰 설정")]
    [Tooltip("로비 씬이 열렸을 때 플레이어나 카메라가 위치할 트랜스폼")]
    [SerializeField] private Transform _lobbySpawnPos;

    public Transform LobbySpawnPoint => _lobbySpawnPos;

    private void Awake()
    {
        if (_lobbySpawnPos == null)
        {
            Debug.LogError($"[OutGameWorkspace] LobbySpawnPoint가 할당되지 않았습니다! 프리팹 내부에서 위치 오브젝트를 할당해주세요.");
        }
    }
}
