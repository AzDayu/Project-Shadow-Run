using UnityEngine;

public class TileData
{
    public string Id; // 각 타일의 ID 입니다
    public string Type; // 타일이 어떤 역할을 하는지 정의합니다, 예) 시작 시점, 탈출 지점 등
    public string Scale; // 타일의 규모를 정의합니다

    public float SpawnWegiht; // 해당 타일의 스폰 확률을 정합니다
    public int MinCount; // 해당 타일이 맵에 최소 몇개 나와야 하는지를 정합니다
    public int MaxCount; // 해당 타일이 맵에 최대 몇개까지 나올 수 있는지를 정합니다
    public int SocketCount; // 해당 타일에 존재하는 연결지점이 몇개 존재하는지를 정의합니다

    public string PrefabPath; // 프리펩의 위치입니다 (어드레서블 사용)
}
