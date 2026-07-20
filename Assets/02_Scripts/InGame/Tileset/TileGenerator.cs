//using System.Collections.Generic;
//using UnityEngine;

//public class TileGenerator : MonoBehaviour // 전체 다시 검토해서 다시 짜야할수도 있음
//{
//    [Header("Map Generation Settings")]
//    [SerializeField] private int _maxRoomCount = 50; // 생성할 최대 방의 개수
//    [SerializeField] private GameObject _startRoomPrefab; // 시작 방 프리팹
//    [SerializeField] private GameObject[] _normalRoomPrefabs; // 방이 될 수 있는 후보군

//    [Header("Physics Settings")]
//    [SerializeField] private LayerMask _tileLayerMask;

//    [Header("Generation Safety Settings")]
//    [SerializeField] private int _maxRetryCount = 50;

//    // 열려있는 소켓들을 관리할 리스트
//    private int _currentRoomCount = 1;


//    private void Start()
//    {
//        GenerateInitialMap();
//    }


//    public void GenerateInitialMap() // 초기 맵 생성을 담당
//    {
//        GameObjectManager.Instance.SpawnObject(_startRoomPrefab, Vector3.zero, Quaternion.identity);

//    }


//    // ============================== 방 생성, 확률 제어 메서드 ==============================
//    //public bool TryConnectRandomRoom() // 연결 프로세스 시작을 담당, 연결할 소켓 무작위 선정
//    //{

//    //}

//    //private GameObject GetRandomPrefab() // 일반 방 후보군에서 무작위 제공
//    //{

//    //}


//    // ============================== 물리 검사, 사후 처리 메서드 ==============================
//    //private bool TrySpawnRoomAtSocket(Transform socketA) // 방하나가 배치될 때까지의 과정을 총괄하는 메서드
//    //{

//    //}

//    private void HandleSpawnFailure(GameObject room, Transform socketA) // 방 생성 실패 시 처리하는 메서드
//    {

//    }

//    private void FinalizeRoomPlacement(GameObject nextRoom, Transform socketA, Transform socketB) // 방 생성 성공 시 처리하는 메서드
//    {

//    }

//    //private bool IsMapCutOffPrematurely(GameObject nextRoom, Transform socketB)
//    //{

//    //}
//}

using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks; // 비동기 처리를 위한 네임스페이스 추가

public class TileGenerator : MonoBehaviour
{
    [Header("Map Generation Settings")]
    [SerializeField] private int _maxRoomCount = 50;
    [SerializeField] private GameObject _startRoomPrefab; // 시작 방은 기존처럼 인스펙터 할당 유지
    [SerializeField] private string _normalRoomType = "Normal"; // 배열 대신 데이터 테이블 검색용 타입 문자열로 변경

    [Header("Physics Settings")]
    [SerializeField] private LayerMask _tileLayerMask;

    [Header("Generation Safety Settings")]
    [SerializeField] private int _maxRetryCount = 50;

    // [개선] Transform 리스트에서 TileSocket 타입 리스트로 변경하여 타입 안정성 확보
    private List<TileSocket> _globalOpenSockets = new List<TileSocket>();
    private int _currentRoomCount = 0;


    private void Start()
    {
        // 비동기 맵 생성 루틴 시작
        InitializeAndGenerate().Forget();
    }

    private async UniTaskVoid InitializeAndGenerate()
    {
        // GameDataManager가 JSON을 파싱할 때까지 아주 잠깐(1프레임 혹은 데이터 체크) 대기합니다.
        // 만약 데이터 매니저에 IsInitialized 같은 bool 변수가 있다면 그것을 체크하는 것이 가장 좋습니다.
        await UniTask.Yield(PlayerLoopTiming.Update);

        // 이제 데이터가 확실히 들어왔으므로 맵 생성을 시작합니다.
        GenerateMapAsync().Forget();
    }


    public async UniTaskVoid GenerateMapAsync()
    {
        // 1. 시작 방 생성 및 초기화
        GameObject startRoomObj = GameObjectManager.Instance.SpawnObject(_startRoomPrefab, Vector3.zero, Quaternion.identity);
        TileController startRoom = startRoomObj.GetComponent<TileController>();

        if (startRoom == null)
        {
            Debug.LogError("[CRITICAL] TileGenerator: 시작 방 프리팹에 TileController가 없습니다!");
            return;
        }

        _currentRoomCount++;
        _globalOpenSockets.AddRange(startRoom.OpenSockets); // TileController의 프로퍼티 활용
        Debug.Log($"시작 방 생성 완료: ID {startRoomObj.name}");

        // 2. 맵 생성 메인 루프 (Layer 1)
        int totalAttempts = 0;

        while (_currentRoomCount < _maxRoomCount && totalAttempts < _maxRetryCount)
        {
            totalAttempts++;
            bool success = await TryConnectRandomRoomAsync();
            if (success) _currentRoomCount++;
        }

        // 결과 리포트
        if (_currentRoomCount < _maxRoomCount)
            Debug.LogWarning($"[WARNING] TileGenerator: 공간 부족으로 목표치 미달. 최종: {_currentRoomCount}개 (시도: {totalAttempts})");
        else
            Debug.Log($"[SUCCESS] TileGenerator: 절차적 맵 생성 완료! 총 {_currentRoomCount}개 방 배치됨.");

        // 🚨 [추가된 부분] 맵 생성이 끝난 후, 짝을 찾지 못해 남겨진 모든 외부 소켓(문)을 닫아줍니다!
        foreach (TileSocket remainingSocket in _globalOpenSockets)
        {
            if (remainingSocket != null)
            {
                remainingSocket.SetPassageState(false);
            }
        }

        Debug.Log($"[TileGenerator] 남은 외부 연결 통로 {_globalOpenSockets.Count}개를 모두 안전하게 폐쇄했습니다.");
    }


    // ============================== 방 생성 및 스폰 (Layer 2) ==============================
    private async UniTask<bool> TryConnectRandomRoomAsync()
    {
        if (_globalOpenSockets.Count == 0) return false;

        // 1. 무작위 기준 소켓 A 선정
        int randomIndex = Random.Range(0, _globalOpenSockets.Count);
        TileSocket socketA = _globalOpenSockets[randomIndex];

        // 2. TilesetManager를 통한 비동기 프리팹 데이터 로드
        GameObject randomPrefab = await TilesetManager.Instance.GetRandomTilePrefabAsync(_normalRoomType);
        if (randomPrefab == null) return false;

        // 3. GameObjectManager를 통한 스폰
        GameObject nextRoomObj = GameObjectManager.Instance.SpawnObject(randomPrefab, Vector3.zero, Quaternion.identity);
        TileController nextRoom = nextRoomObj.GetComponent<TileController>();

        if (nextRoom == null)
        {
            Destroy(nextRoomObj); // 유효하지 않은 프리팹 즉시 폐기
            return false;
        }

        return ProcessRoomPlacement(nextRoom, socketA);
    }


    // ============================== 물리 검사 및 사후 처리 (Layer 3) ==============================
    private bool ProcessRoomPlacement(TileController nextRoom, TileSocket socketA)
    {
        TileSocket socketB = nextRoom.GetFirstSocket();

        if (socketB == null)
        {
            HandleSpawnFailure(nextRoom.gameObject);
            return false;
        }

        // 1. TilePhysics를 이용한 정렬
        TilePhysics.AlignRoom(nextRoom, socketA, socketB);

        // 2. TilePhysics를 이용한 겹침 검사 및 조기 끊김 검사
        if (TilePhysics.IsRoomOverlapping(nextRoom, _tileLayerMask) || IsMapCutOffPrematurely(nextRoom, socketB))
        {
            HandleSpawnFailure(nextRoom.gameObject);
            return false;
        }

        // 3. 성공 시 최종 배치 처리
        FinalizeRoomPlacement(nextRoom, socketA, socketB);
        return true;
    }


    private void HandleSpawnFailure(GameObject roomObj)
    {
        // 겹침 판정 시 임시 스폰된 방을 즉시 파괴하여 흔적을 지웁니다.
        // (GameObjectManager의 ID 체계와 꼬이지 않도록 엔진 Destroy 사용 권장)
        Destroy(roomObj);
    }


    private void FinalizeRoomPlacement(TileController nextRoom, TileSocket socketA, TileSocket socketB)
    {
        // 통로 상태 갱신
        socketA.SetPassageState(true);
        socketB.SetPassageState(true);

        // 연결 상태 갱신
        socketA.IsConnected = true;
        socketB.IsConnected = true;

        // 사용된 소켓은 글로벌 리스트에서 제거하고, 새 방의 남은 소켓들을 추가
        _globalOpenSockets.Remove(socketA);

        foreach (var socket in nextRoom.OpenSockets)
        {
            if (socket != socketB) // 방금 연결에 사용된 소켓 B를 제외한 나머지 추가
            {
                _globalOpenSockets.Add(socket);
            }
        }
    }


    private bool IsMapCutOffPrematurely(TileController nextRoom, TileSocket socketB)
    {
        // 새 방에서 추가될 유효 소켓 수 계산 (연결에 쓰인 socketB 제외)
        int newlyAddedSockets = nextRoom.OpenSockets.Count - 1;

        // 현재 열린 전체 소켓 수 - 이번 연결에 소모될 socketA(1개) + 새 방의 소켓들
        int expectedOpenSockets = _globalOpenSockets.Count - 1 + newlyAddedSockets;

        // 아직 마지막 방을 배치할 차례가 아닌데, 예상 잔여 소켓이 0개 이하라면 맵 확장이 영구 정지됨
        if (_currentRoomCount < _maxRoomCount - 1 && expectedOpenSockets <= 0)
        {
            Debug.LogWarning($"[WARNING] TileGenerator: 맵 조기 끊김 감지! {nextRoom.gameObject.name} 배치를 반려합니다.");
            return true;
        }

        return false;
    }
}