using System.Collections.Generic;
using UnityEngine;

public class TileGenerator_Old : MonoBehaviour
{
    [Header("Map Generation Settings")]
    [SerializeField] private int _maxRoomCount = 50; // 생성할 최대 방의 개수
    [SerializeField] private GameObject _startRoomPrefab; // 시작 방 프리팹
    [SerializeField] private GameObject[] _normalRoomPrefabs; // 방이 될 수 있는 후보군

    [Header("Physics Settings")]
    [SerializeField] private LayerMask _tileLayerMask;

    [Header("Generation Safety Settings")]
    [SerializeField] private int _maxRetryCount = 50;

    // 열려있는 소켓들을 관리할 리스트
    private List<Transform> _openSockets = new List<Transform>();
    private int _currentRoomCount = 1;


    private void Start()
    {
        GenerateInitialMap();

        int totalAttempts = 0;   // 시도 횟수 안전장치

        // 방이 다 차거나, 시도 제한에 걸릴 때까지 반복
        while (_currentRoomCount < _maxRoomCount && totalAttempts < _maxRetryCount)
        {
            totalAttempts++;
            if (TryConnectRandomRoom())
            {
                _currentRoomCount++; // 성공 시에만 멤버 변수 카운트를 올립니다.
            }
        }

        // 방어 코드: 루프 종료 후 제대로 생성되었는지 검증
        if (_currentRoomCount < _maxRoomCount)
        {
            Debug.LogWarning($"TileGenerator [WARNING]: 공간 부족으로 목표치({_maxRoomCount})만큼 방을 생성하지 못했습니다. 최종 생성: {_currentRoomCount}개 (시도 횟수: {totalAttempts})");
        }
        else
        {
            Debug.Log($"TileGenerator: 절차적 맵 생성 완료! 총 {_currentRoomCount}개의 방이 정상 배치되었습니다.");
        }
    }


    public void GenerateInitialMap() // 초기 맵 생성을 담당
    {
        if (_startRoomPrefab == null)
        {
            Debug.LogError("TileGenerator [CRITICAL]: 시작 타일 프리팹이 등록되지 않았습니다! 인스펙터를 확인해주세요.");
            return;
        }

        if (GameObjectManager.Instance == null)
        {
            Debug.LogError("TileGenerator [CRITICAL]: GameObjectManager 인스턴스를 찾을 수 없습니다! 씬에 GameObjectManager가 배치되어 있는지 확인하세요.");
            return;
        }

        GameObject startRoom = GameObjectManager.Instance.SpawnObject(_startRoomPrefab, Vector3.zero, Quaternion.identity);

        if (startRoom == null)
        {
            Debug.LogError("TileGenerator [CRITICAL]: 시작 방 생성에 실패했습니다.");
            return;
        }
        else
        {
            Debug.Log($"시작 방 생성 완료: ID {startRoom.name}");
        }

        AddSocketsFromRoom(startRoom);

        // [구조 수정]: 첫 번째 무작위 방 연결에 성공한 경우에만 룸 카운트를 정상적으로 동기화합니다.
        if (TryConnectRandomRoom())
        {
            _currentRoomCount++;
        }
    }


    // ============================== 소켓 탐색 메서드 ==============================
    private void AddSocketsFromRoom(GameObject room) // 방 안의 소켓들을 열려있는 소켓 리스트에 추가하는 메서드
    {
        if (room == null)
        {
            Debug.LogWarning("TileGenerator [WARNING]: AddSocketsFromRoom() 호출 중 대상 room이 null입니다.");
            return;
        }

        // 자식들 중, Socket 오브젝트 수집
        foreach (Transform child in room.GetComponentsInChildren<Transform>())
        {
            if (child == room.transform) continue; // 방 자기 자신은 건너뛰기

            if (child.name.Contains("Socket") && child.gameObject.activeInHierarchy)
            {
                _openSockets.Add(child);
            }
        }
    }

    private Transform FindFirstSocket(GameObject room) // 방 안에서 첫 번째 소켓을 찾는 메서드
    {
        if (room == null)
        {
            Debug.LogError("TileGenerator [ERROR]: FindFirstSocket() 호출 중 대상 room이 null입니다.");
            return null;
        }

        Transform[] allChildren = room.GetComponentsInChildren<Transform>(); // 모든 자식을 배열로 가져옴

        foreach (Transform child in allChildren)
        {
            if (child == room.transform) continue; // 방 자기 자신은 건너뛰기

            if (child.name.Contains("Socket")) return child;
        }
        return null;
    }

    private void AlignNextRoom(Transform roomTransform, Transform socketA, Transform socketB) // 소켓 A와 소켓 B를 맞춰서 방을 정렬하는 메서드
    {
        if (roomTransform == null || socketA == null || socketB == null)
        {
            Debug.LogError("TileGenerator [ERROR]: AlignNextRoom() 매개변수 중 null이 존재하여 정렬을 수행할 수 없습니다.");
            return;
        }

        // 회전 맞추기
        Quaternion targetRotation = Quaternion.LookRotation(-socketA.forward, socketA.up);

        Quaternion differenceRotation = targetRotation * Quaternion.Inverse(socketB.rotation);

        roomTransform.rotation = differenceRotation * roomTransform.rotation;

        // 위치 맞추기
        Vector3 offset = socketA.position - socketB.position;
        roomTransform.position += offset;
    }


    // ============================== 방 생성, 확률 제어 메서드 ==============================
    public bool TryConnectRandomRoom() // 연결 프로세스 시작을 담당, 연결할 소켓 무작위 선정
    {
        // 방어 코드: 더 이상 붙일 문(소켓)이 없다면 조기 리턴
        if (_openSockets == null || _openSockets.Count == 0)
        {
            Debug.LogWarning("TileGenerator [WARNING]: 연결 가능한 소켓이 없습니다.");
            return false;
        }

        // 1. 기준이 될 소켓 A를 무작위로 선택
        int randomIndex = Random.Range(0, _openSockets.Count);
        Transform socketA = _openSockets[randomIndex];

        if (socketA == null)
        {
            Debug.LogError($"TileGenerator [ERROR]: 선택된 소켓 A가 null입니다. 리스트의 {randomIndex}번째 소켓 정보를 유실했습니다.");
            _openSockets.RemoveAt(randomIndex); // 손상된 참조는 리스트에서 즉시 소멸시켜 시스템 마비 예방
            return false;
        }

        // 2. 해당 소켓에 방 스폰을 시도하고 결과를 상위 루프에 보고
        return TrySpawnRoomAtSocket(socketA);
    }

    private GameObject GetRandomPrefab() // 일반 방 후보군에서 무작위 제공
    {
        if (_normalRoomPrefabs == null || _normalRoomPrefabs.Length == 0)
        {
            Debug.LogError("TileGenerator [CRITICAL]: _normalRoomPrefabs 후보군이 세팅되지 않았습니다!");
            return null;
        }
        return _normalRoomPrefabs[Random.Range(0, _normalRoomPrefabs.Length)];
    }


    // ============================== 물리 검사, 사후 처리 메서드 ==============================
    private bool TrySpawnRoomAtSocket(Transform socketA) // 방하나가 배치될 때까지의 과정을 총괄하는 메서드
    {
        if (socketA == null)
        {
            Debug.LogError("TileGenerator [ERROR]: TrySpawnRoomAtSocket()의 socketA 매개변수가 null입니다.");
            return false;
        }

        // 1. 생성 시도
        GameObject randomPrefab = GetRandomPrefab();
        if (randomPrefab == null) return false;

        if (GameObjectManager.Instance == null)
        {
            Debug.LogError("TileGenerator [CRITICAL]: GameObjectManager 인스턴스가 존재하지 않아 방을 스폰할 수 없습니다.");
            return false;
        }

        GameObject nextRoom = GameObjectManager.Instance.SpawnObject(randomPrefab, Vector3.zero, Quaternion.identity);
        if (nextRoom == null)
        {
            Debug.LogError($"TileGenerator [ERROR]: GameObjectManager를 통한 {randomPrefab.name} 생성이 실패했습니다.");
            return false;
        }

        Transform socketB = FindFirstSocket(nextRoom);

        if (socketB == null)
        {
            Debug.LogError($"TileGenerator [ERROR]: {nextRoom.name}에 연결할 소켓(socketB)이 없습니다.");
            GameObjectManager.Instance.RemoveObject(nextRoom.GetInstanceID());
            return false;
        }

        AlignNextRoom(nextRoom.transform, socketA, socketB);

        // 2. 2중 검사 (물리적 겹침 검사 OR 맵 조기 끊김 방어막 가동)
        if (IsRoomBlocked(nextRoom) || IsMapCutOffPrematurely(nextRoom, socketB))
        {
            HandleSpawnFailure(nextRoom, socketA);
            return false;
        }

        // 3. 성공 처리
        FinalizeRoomPlacement(nextRoom, socketA, socketB);
        return true;
    }

    private bool IsRoomBlocked(GameObject room) // 방이 다른 오브젝트와 겹치는지 검사하는 메서드
    {
        if (room == null)
        {
            Debug.LogError("TileGenerator [ERROR]: IsRoomBlocked() 검사 대상 room이 null입니다.");
            return false;
        }

        BoxCollider roomCollider = room.GetComponent<BoxCollider>();
        if (roomCollider == null)
        {
            Debug.LogWarning($"TileGenerator [WARNING]: {room.name}에 BoxCollider 컴포넌트가 존재하지 않아 물리 겹침 검사를 생략합니다.");
            return false;
        }

        return Physics.CheckBox(
            room.transform.position + roomCollider.center,
            roomCollider.size / 2,
            room.transform.rotation,
            _tileLayerMask
        );
    }

    private void HandleSpawnFailure(GameObject room, Transform socketA) // 방 생성 실패 시 처리하는 메서드
    {
        if (room == null)
        {
            Debug.LogError("TileGenerator [ERROR]: HandleSpawnFailure()의 room 매개변수가 null입니다.");
            return;
        }

        Debug.LogWarning($"TileGenerator [WARNING]: {room.name} ({room.transform.position})가 겹침/끊김 위험 판정을 받아 조립을 중단하고 즉시 회수합니다.");

        if (GameObjectManager.Instance != null)
        {
            // [주의]: GameObjectManager의 인스턴스 ID 리스트와 Unity GetInstanceID() 매칭 로직을 최종 확인해 주세요.
            GameObjectManager.Instance.RemoveObject(room.GetInstanceID());
        }
        else
        {
            Debug.LogError("TileGenerator [CRITICAL]: 방 회수 중 GameObjectManager가 누락되어 일반 Destroy(room)를 긴급 수행합니다.");
            Destroy(room);
        }

        if (socketA != null)
        {
            TileSocket passage = socketA.GetComponent<TileSocket>();
            if (passage != null) passage.SetPassageState(false);
        }
    }

    private void FinalizeRoomPlacement(GameObject nextRoom, Transform socketA, Transform socketB) // 방 생성 성공 시 처리하는 메서드
    {
        if (nextRoom == null || socketA == null || socketB == null)
        {
            Debug.LogError("TileGenerator [ERROR]: FinalizeRoomPlacement() 중 필수 매개변수가 null입니다. 성공 처리가 완전히 완료되지 않았을 수 있습니다.");
            return;
        }

        TileSocket nextRoomPassage = socketB.GetComponent<TileSocket>();
        if (nextRoomPassage != null) nextRoomPassage.SetPassageState(true);

        TileSocket currentRoomPassage = socketA.GetComponent<TileSocket>();
        if (currentRoomPassage != null) currentRoomPassage.SetPassageState(true);

        _openSockets.Remove(socketA);
        AddSocketsFromRoom(nextRoom);
        _openSockets.Remove(socketB);
    }

    private bool IsMapCutOffPrematurely(GameObject nextRoom, Transform socketB)
    {
        if (nextRoom == null || socketB == null)
        {
            Debug.LogError("TileGenerator [ERROR]: IsMapCutOffPrematurely() 중 매개변수가 null입니다.");
            return false;
        }

        // 1. 새로 스폰된 방이 가지고 있는 총 소켓 개수를 조사합니다.
        int nextRoomSockets = CountActiveSockets(nextRoom);

        // 2. 이번 배치로 인해 "새롭게 추가될" 소켓의 개수를 구합니다.
        // (스폰된 방의 소켓들 중, 연결에 사용된 socketB는 소모되므로 1을 뺍니다)
        int newlyAddedSockets = Mathf.Max(0, nextRoomSockets - 1);

        // 3. 이 방을 확정 배치했을 때 최종적으로 남게 될 예상 열린 소켓 수를 계산합니다.
        // (현재 열린 소켓 수 - 소켓A 소모(1개) + 새로 추가되는 소켓 수)
        int expectedOpenSockets = _openSockets.Count - 1 + newlyAddedSockets;

        // 4. [골인 지점 판정 검문소]
        // 아직 마지막 방(_maxRoomCount - 1 번째)을 배치할 차례가 아닌데, 
        // 이 방을 놓으면 남은 소켓이 0개 이하가 되어 맵 확장이 영구히 멈춰버리는 상황인지 검사합니다.
        if (_currentRoomCount < _maxRoomCount - 1 && expectedOpenSockets <= 0)
        {
            // 방어 코드: 맵이 중간에 대책 없이 끊기는 상황이므로 '위험 상황(true)'을 반환합니다.
            Debug.LogWarning($"TileGenerator [WARNING]: 맵 끊김 감지! {nextRoom.name} 배치 시 예상 소켓이 0개가 되므로 스폰을 반려합니다.");
            return true;
        }

        return false; // 안전하게 더 뻗어나갈 수 있는 상태입니다.
    }

    private int CountActiveSockets(GameObject room)
    {
        if (room == null)
        {
            Debug.LogError("TileGenerator [ERROR]: CountActiveSockets()의 대상 room이 null입니다.");
            return 0;
        }

        int count = 0;
        Transform[] allChildren = room.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildren)
        {
            if (child == room.transform) continue;

            // 이름에 "Socket"이 포함되어 있고 활성화된 소켓의 개수를 카운트합니다.
            if (child.name.Contains("Socket") && child.gameObject.activeInHierarchy)
            {
                count++;
            }
        }
        return count;
    }
}