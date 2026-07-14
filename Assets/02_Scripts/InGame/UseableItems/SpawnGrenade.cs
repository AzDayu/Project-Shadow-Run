using UnityEngine;
using System.Collections;

public class SpawnGrenade : MonoBehaviour
{
    [SerializeField] private float damage;           // 폭발 피해
    [SerializeField] private float delay;            // 폭발 지연 시간
    [SerializeField] private float explosionRadius;  // 폭발 반경
    [SerializeField] private float moveForce;        // 폭발 넉백 위력
    [SerializeField] private GameObject explosionEffect;   // 폭발 이펙트 프리랩

    private void OnEnable( )
    {
        // 재활성화 시, 이전 물리 리셋
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;      // 이동 속도 초기화
            rb.angularVelocity = Vector3.zero;     // 회전 속도 초기화
        }
    }

    public void ReadyToThrow( )
    {
        StartCoroutine(FuseRoutine()); // 폭발 타이머 가동
    }


    private IEnumerator FuseRoutine( )
    {
        // 지정된 시간만큼 대기
        yield return new WaitForSeconds(delay);

        // 대기 완료 후 폭발 함수 호출
        Explode();
    }

    void Explode( )
    {
        // 폭발 이펙트(파티클) 생성
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, transform.rotation);
        }

        // 폭발 반경 내의 모든 콜라이더 검출
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        // 검출된 오브젝트들을 순회하며 데미지 처리
        for (int i = 0; i < colliders.Length; i++)
        {
            // 수류탄 본인 객체 제외
            if (colliders[i].gameObject == gameObject)
            {
                continue;
            }

            //// 대상에게 적 체력 컴포넌트가 있는지 확인
            //EnemyHealth enemy = colliders[i].GetComponent<EnemyHealth>();
            //
            //if (enemy != null)
            //{
            //    enemy.TakeDamage(damage);
            //}

            // 거리에 따른 3D 입체 방사형 넉백 처리
            Rigidbody rb = colliders[i].GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 3D 방향 계산 및 정규화 (길이를 1로 만듦)
                Vector3 pushDirection = colliders[i].transform.position - transform.position;
                pushDirection = pushDirection.normalized;

                // 폭발 중심점과 적 사이의 실제 거리 측정
                float distance = Vector3.Distance(transform.position, colliders[i].transform.position);

                // 거리에 따른 힘의 감쇠 비율 계산 (가까울수록 1, 반경 끝자락이면 0)
                float forceRatio = 1f - ( distance / explosionRadius );
                if (forceRatio < 0f) forceRatio = 0f;

                // 방향 * (기본 힘 * 거리 비율) 계산 후 즉시 충격 부여
                rb.AddForce(pushDirection * ( moveForce * forceRatio ), ForceMode.Impulse);
            }
        }

        // 오브젝트 풀링을 위한 비활성화 처리
        gameObject.SetActive(false);
    }
}