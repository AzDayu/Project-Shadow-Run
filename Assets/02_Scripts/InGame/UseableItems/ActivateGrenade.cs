using UnityEngine;
using System.Collections;

public class ActivateGrenade : MonoBehaviour
{
    [SerializeField] private GameObject _explosionEffect; // 폭발 이펙트 프리팹 (선택)

    // 생성 직후 투척력 전달 및 폭발 코루틴 시작
    public void InitGrenade( ItemData itemData, Vector3 throwDirection, float throwForce )
    {
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        if (rigidBody != null)
        {
            rigidBody.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        }

        StartCoroutine(ExplodeRoutine(itemData));
    }

    private IEnumerator ExplodeRoutine( ItemData itemData )
    {
        int damage = itemData.HpVariation;        // [2] 데미지 (피해량)
        float fuseTime = itemData.Duration;       // [3] 폭발 지연 시간 (신관 타이머)
        float radius = itemData.EffectRange;      // [4] 폭발 범위 (반경)

        //  신관 타이머 대기
        if (fuseTime > 0f)
        {
            yield return new WaitForSeconds(fuseTime);
        }

        //  폭발 처리
        ProcessExplosion(damage, radius);
    }

    private void ProcessExplosion( float damage, float radius )
    {
        // 폭발 이펙트 생성
        if (_explosionEffect != null)
        {
            Instantiate(_explosionEffect, transform.position, Quaternion.identity);
        }

        // 구체(Sphere) 범위 안의 모든 콜라이더 감지
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

        for (int i = 0; i < hitColliders.Length; i++)
        {
            // TODO: 적(Enemy)이나 플레이어 등 피해 대상에게 데미지 전달
            // 예시: 
            // EnemyHealth enemy = hitColliders[i].GetComponent<EnemyHealth>();
            // if (enemy != null) { enemy.TakeDamage(damage); }
        }

        // 수류탄 오브젝트 삭제
        Destroy(gameObject);
    }
}
    