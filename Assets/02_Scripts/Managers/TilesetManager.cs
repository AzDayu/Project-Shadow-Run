//using Cysharp.Threading.Tasks;
//using NUnit.Framework;
//using System.Collections.Generic;
//using UnityEngine;

//public class TilesetManager : MonoBehaviour
//{
//    public static TilesetManager Instance { get; private set; }

//    private void Awake()
//    {
//        Instance = this;
//    }

//    public async UniTask<GameObject> GetRandomTilePrefabAsync(string type) // 리소스 매니저에 맞게 비동기 메서드로
//    {
//        if (GameDataManager.Instance == null)
//        {
//            Debug.LogError("TilesetManager: GameDataManager.Instance가 null입니다. 씬에 배치되었는지 확인하세요.");
//            return null;
//        }

//        List<TileData> candidates = GameDataManager.Instance.GetTileDataListByType(type); // 데이터 매니저에게서 타입에 맞는 후보군을 가져오기 

//        if (candidates == null || candidates.Count == 0)
//        {
//            Debug.LogError($"TilesetManager: {type} 타입 타일 데이터가 없습니다");
//            return null;
//        }

//        TileData selectedTile = SelectTileByWeight(candidates); // 가중치 계산

//        GameObject tilePrefab = await ResourceManager.Inst.LoadAsset<GameObject>(selectedTile.PrefabPath); // 리소스 매니저에게서 프리팹 로드

//        if (tilePrefab == null)
//        {
//            Debug.LogError($"TilesetManager: {selectedTile.PrefabPath} 경로의 타일 프리팹을 로드하지 못했습니다");
//            return null;
//        }

//        return tilePrefab;
//    }

//    private TileData SelectTileByWeight(List<TileData> candidates)
//    {
//        // AI피셜) [구현 팁]: 후보군의 SpawnWegiht 합계를 구하고, Random.Range(0, totalWeight)를 돌려 리스트를 순회하며 선택되는 항목을 찾는 로직을 넣어보세요.
//        return candidates[0]; // 우선은 첫 번째 것만 리턴
//    }
//}

using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class TilesetManager : MonoBehaviour
{
    public static TilesetManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public async UniTask<GameObject> GetRandomTilePrefabAsync(string type)
    {
        List<TileData> candidates = DataManager.Instance.GetTileDataListByType(type);

        if (candidates == null || candidates.Count == 0)
        {
            Debug.LogError($"TilesetManager: {type} 타입 타일 데이터가 없습니다");
            return null;
        }

        // [수정] 이제 첫 번째 요소 대신 가중치 계산을 거친 안전한 타일을 선택합니다.
        TileData selectedTile = SelectTileByWeight(candidates);

        if (selectedTile == null) return null;

        GameObject tilePrefab = await ResourceManager.Inst.LoadAsset<GameObject>(selectedTile.PrefabPath);

        if (tilePrefab == null)
        {
            Debug.LogError($"TilesetManager: {selectedTile.PrefabPath} 경로의 타일 프리팹을 로드하지 못했습니다");
            return null;
        }

        return tilePrefab;
    }

    // [수정] 기획 데이터의 SpawnWeight를 기반으로 무작위 타일을 선택하는 알고리즘 구현
    private TileData SelectTileByWeight(List<TileData> candidates)
    {
        int totalWeight = 0;

        // 1. 모든 후보군의 가중치 총합을 구합니다.
        foreach (var tile in candidates)
        {
            // 방어 코드: 혹시 기획자가 실수로 가중치를 0 이하로 적었다면 최소 1로 보정해 줍니다.
            int weight = Mathf.Max(1, tile.SpawnWeight);
            totalWeight += weight;
        }

        // 2. 0부터 totalWeight 사이의 난수를 생성합니다.
        int randomPick = Random.Range(0, totalWeight);
        int currentAccumulatedWeight = 0;

        // 3. 다시 리스트를 순회하며 누적 가중치를 계산하여 당첨자를 선택합니다.
        foreach (var tile in candidates)
        {
            currentAccumulatedWeight += Mathf.Max(1, tile.SpawnWeight);

            if (randomPick < currentAccumulatedWeight)
            {
                return tile; // 당첨된 타일 데이터 반환
            }
        }

        // 방어 코드: 만약 루프가 비정상적으로 끝났다면 마지막 요소라도 반환합니다.
        return candidates[candidates.Count - 1];
    }
}