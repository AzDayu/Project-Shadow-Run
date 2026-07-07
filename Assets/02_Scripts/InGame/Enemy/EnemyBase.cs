using UnityEngine;
using UnityEngine.AI; 

public class EnemyBase : MonoBehaviour
{
    // 플레이어의 위치를 드래그 앤 드롭할 변수
    public Transform playerTransform;

    private NavMeshAgent agent;

    void Start()
    {
        // 내 오브젝트에 붙어있는 NavMeshAgent를 가져옵니다.
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (playerTransform != null)
        {
            // 실시간으로 플레이어의 위치를 향해 경로를 계산하고 이동합니다.
           // agent.SetDestination(playerTransform.position);
        }
    }
}