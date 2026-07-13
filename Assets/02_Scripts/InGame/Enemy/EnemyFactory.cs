using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    public EnemyBase CreateEnemy(string enemyID) 
    {
        if (!DataManager.Instance._enemyDataDic.TryGetValue(enemyID, out EnemyData enemyData))
        {
            Debug.LogError($"EnemyData가 존재하지 않습니다. ID : {enemyID}");
            return null;
        }
        GameObject enemyObject = ObjectPoolManager.Instance.GetFromPool(enemyData.PrefabAddress);
        if (enemyObject == null)
        {
            Debug.LogError($"Enemy Pool 생성 실패 : {enemyData.PrefabAddress}");
            return null;
        }
        EnemyBase enemy = enemyObject.GetComponent<EnemyBase>();
        if (enemy == null)
        {
            Debug.LogError($"{enemyData.PrefabAddress} 프리팹에 EnemyBase가 없습니다.");
            return null;
        }

        enemy.Initialize(enemyData);

        return enemy;
    }
}
