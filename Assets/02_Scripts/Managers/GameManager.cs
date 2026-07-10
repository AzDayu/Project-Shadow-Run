using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("@GameManager");
                _instance = go.AddComponent<GameManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public enum GameState { Core, OutGame, InGame }
    private GameState _currentGameState;
    public GameState CurrentGameState
    {
        get { return _currentGameState; }
        set { _currentGameState = value; }
    }

    [Header("작업 공간 프리팹 설정")]
    [SerializeField] private GameObject _outGameWorkspacePrefab;
    [SerializeField] private GameObject _inGameWorkspacePrefab;

    private GameObject _currentWorkspaceInstance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);

        InitializeGame();
    }

    private void InitializeGame()
    {
        _currentGameState = GameState.Core;
        Debug.Log("GameManager: 코어 시스템 초기화 완료. 아웃게임(로비) 프리팹을 생성합니다.");

        ChangeState(GameState.OutGame);
    }

    public void ChangeState(GameState newState)
    {
        if (_currentGameState == newState && _currentWorkspaceInstance != null) return;

        _currentGameState = newState;

        if (_currentWorkspaceInstance != null)
        {
            Destroy(_currentWorkspaceInstance);
            _currentWorkspaceInstance = null;
        }

        switch (_currentGameState)
        {
            case GameState.OutGame:
                if (_outGameWorkspacePrefab != null)
                {
                    _currentWorkspaceInstance = Instantiate(_outGameWorkspacePrefab);
                    _currentWorkspaceInstance.name = "[Workspace] OutGame_Lobby";
                    Debug.Log("GameManager: 아웃게임 프리팹 배치 완료.");
                }
                else
                {
                    Debug.LogError("GameManager: OutGame 프리팹이 인스펙터에 할당되지 않았습니다!");
                }
                break;

            case GameState.InGame:
                if (GameObjectManager.Instance != null)
                {
                    GameObjectManager.Instance.ClearAllSpawnedObjects();
                }

                if (_inGameWorkspacePrefab != null)
                {
                    _currentWorkspaceInstance = Instantiate(_inGameWorkspacePrefab);
                    _currentWorkspaceInstance.name = "[Workspace] InGame_Battle";
                    Debug.Log("GameManager: 인게임 프리팹(맵+플레이어) 배치 완료.");
                }
                else
                {
                    Debug.LogError("GameManager: InGame 프리팹이 인스펙터에 할당되지 않았습니다!");
                }
                break;
        }
    }

    public void ReturnToOutGame()
    {
        ChangeState(GameState.OutGame);
    }

    public void StartInGame()
    {
        ChangeState(GameState.InGame);
    }
}