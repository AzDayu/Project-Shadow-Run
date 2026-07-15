using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    [Header("Map Generation Settings")]
    [SerializeField] private int _maxRoomCount = 10; // 생성할 최대 방의 개수
    [SerializeField] private GameObject _startRoomPrefab; // 시작 방 프리팹
    [SerializeField] private GameObject[] _normalRoomPrefabs; // 방이 될 수 있는 후보군

    // 열려있는 소켓들을 관리할 리스트
    private List<Transform> _openSockets = new List<Transform>();


    private void Start()
    {
        GenerateInitialMap();

        for (int i = 1; i < _maxRoomCount; i++) // 남은 방 갯수만큼 방 연결 시도
        {
            TryConnectRandomRoom();
        }
    }


    public void GenerateInitialMap()
    {
        if (_startRoomPrefab == null)
        {
            Debug.LogError("TileGenerator : 시작 타일이 등록되지 않았습니다!");
            return;
        }

        GameObject startRoom = GameObjectManager.Instance.SpawnObject(_startRoomPrefab, Vector3.zero, Quaternion.identity);

        if (startRoom != null)
        {
            Debug.Log($"시작 방 생성 완료: ID {startRoom.name}");
        }

        AddSocketsFromRoom(startRoom);

        TryConnectRandomRoom();
    }


    public void TryConnectRandomRoom()
    {
        if (_openSockets.Count == 0 || _normalRoomPrefabs.Length == 0)
        {
            Debug.LogWarning("TileGenerator : 연결 가능한 소켓이 없거나 방 후보군이 없습니다.");
            return;
        }

        // 기준이 될 소켓 A를 무작위로 선택
        Transform socketA = _openSockets[Random.Range(0, _openSockets.Count)];

        // 랜덤 프리펩 선택
        GameObject randomPrefab = _normalRoomPrefabs[Random.Range(0, _normalRoomPrefabs.Length)];

        // 새로운 방 소환
        GameObject nextRoom = GameObjectManager.Instance.SpawnObject(randomPrefab, Vector3.zero, Quaternion.identity);


        // 새 방의 소켓 B 찾기
        if (nextRoom == null)
        {
            Debug.LogError("TileGenerator : 방 생성 실패");
            return;
        }

        Transform socketB = FindFirstSocket(nextRoom);
        if (socketB == null)
        {
            Debug.LogError($"{nextRoom.name}에서 소켓을 찾을 수 없습니다.");
            return;
        }

        if (socketB != null)
        {
            // 워프레임식 정렬 실행
            AlignNextRoom(nextRoom.transform, socketA, socketB);

            _openSockets.Remove(socketA); // 소켓 A는 이제 닫힘
            AddSocketsFromRoom(nextRoom); // 새 방의 소켓들을 열려있는 소켓 리스트에 추가
            _openSockets.Remove(socketB); // 소켓 B도 닫힘
        }
    }

    private Transform FindFirstSocket(GameObject room)
    {
        Transform[] allChildren = room.GetComponentsInChildren<Transform>(); // 모든 자식을 배열로 가져옴 < 이거 안해서 방이 소켓 못 찾고 겹치는 버그 터졌었음

        foreach (Transform child in allChildren)
        {
            if (child == room.transform) continue; // 방 자기 자신은 건너뛰기

            if (child.name.Contains("Socket")) return child;
        }
        return null;
    }

    private void AlignNextRoom(Transform roomTransform, Transform socketA, Transform socketB)
    {
        // 회전 맞추기
        Quaternion targetRotation = Quaternion.LookRotation(-socketA.forward, socketA.up);

        Quaternion differenceRotation = targetRotation * Quaternion.Inverse(socketB.rotation);

        roomTransform.rotation = differenceRotation * roomTransform.rotation;

        // 위치 맞추기
        Vector3 offset = socketA.position - socketB.position;
        roomTransform.position += offset;
    }

    private void AddSocketsFromRoom(GameObject room)
    {
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
}
