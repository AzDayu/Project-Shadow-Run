using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
        Debug.Log("GameManager: 코어 시스템 초기화 완료. 아웃게임(로비)으로 진입합니다.");

        StartCoroutine(LoadOutGameRoutine());
    }

    public void ReturnToOutGame()
    {
        if (_currentGameState == GameState.OutGame) return;
        StartCoroutine(LoadOutGameRoutine());
    }

    private IEnumerator LoadOutGameRoutine()
    {
        _currentGameState = GameState.OutGame;

        if (SceneManager.GetSceneByName("Scene_InGame").isLoaded)
            yield return SceneManager.UnloadSceneAsync("Scene_InGame");
        if (SceneManager.GetSceneByName("Scene_Environment").isLoaded)
            yield return SceneManager.UnloadSceneAsync("Scene_Environment");

        if (!SceneManager.GetSceneByName("Scene_OutGame").isLoaded)
            yield return SceneManager.LoadSceneAsync("Scene_OutGame", LoadSceneMode.Additive);

        Debug.Log("GameManager: 아웃게임(로비) 로드 완료");
    }

    public void StartInGame()
    {
        if (_currentGameState == GameState.InGame) return;
        StartCoroutine(LoadInGameRoutine());
    }

    private IEnumerator LoadInGameRoutine()
    {
        _currentGameState = GameState.InGame;

        if (SceneManager.GetSceneByName("Scene_OutGame").isLoaded)
            yield return SceneManager.UnloadSceneAsync("Scene_OutGame");

        yield return SceneManager.LoadSceneAsync("Scene_Environment", LoadSceneMode.Additive);
        yield return SceneManager.LoadSceneAsync("Scene_InGame", LoadSceneMode.Additive);

        Debug.Log("GameManager: 인게임 맵 진입 완료 (Environment + InGame 로드)");
    }
}