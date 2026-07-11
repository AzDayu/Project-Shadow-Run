using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    public Transform cameraTarget;
    private PlayerWeapon weapon;
    private float nextFireTime = 0f;

    private void Start( )
    {
        // 최상위 Weapon 컴포넌트 가져오기
        weapon = GetComponent<PlayerWeapon>();

        // 외부에서 지정되지 않았다면 자동으로 시야 타겟을 찾아 연결
        if (cameraTarget == null)
        {
            // 무기가 속한 최상위 오브젝트(Player) 하위에서 "PlayerCameraTarget"이라는 이름의 트랜스폼을 검색
            Transform _root = transform.root;
            Transform[] _allChildren = _root.GetComponentsInChildren<Transform>(true);

            foreach (Transform _child in _allChildren)
            {
                if (_child.name == "PlayerCameraTarget")
                {
                    cameraTarget = _child;
                    break;
                }
            }

            // 만약 이름으로 찾지 못했다면, 결국 플레이어의 시선과 일치하는 Main Camera를 대안으로 설정
            if (cameraTarget == null && Camera.main != null)
            {
                cameraTarget = Camera.main.transform;
                Debug.LogWarning($"{gameObject.name}: CameraTarget 없음, Main Camera 연결");
            }
        }
    }

    private void Update( )
    {
        // 사격 타겟이 없으면 에러 방지를 위해 실행 안 함
        if (cameraTarget == null){ Debug.LogError("CameraTarget is empty."); return; }
        if (weapon == null) return; // 예외 방지

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            // BuffSystem이 있다면 버프가 적용된 연사 속도 계산
            float currentFireRate = weapon.stat.AttackInterval;
            if (weapon.buffSystem != null)
            {
                currentFireRate = weapon.buffSystem.GetBuffedFireRate(weapon.stat.AttackInterval);
            }

            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + currentFireRate;
                TryShoot();
            }
        }
    }

    private void TryShoot( )
    {
        if (weapon == null || weapon.reloadSystem == null) return;

        // 재장전 도중 사격 차단
        if (weapon.reloadSystem.isReloading)
        {
            Debug.Log("재장전 도중 사격 불가");
            return;
        }
        // 장탄수 0일 때 사격 불가
        if (!weapon.reloadSystem.HasAmmo())
        {
            Debug.Log("장탄수 0");
            return;
        }

        weapon.reloadSystem.ConsumeAmmo();

        // BuffSystem이 있다면 버프가 적용된 데미지 계산
        int baseDamage = Mathf.RoundToInt(weapon.stat.Damage);
        int finalDamage = baseDamage;
        if (weapon.buffSystem != null)
        {
            finalDamage = weapon.buffSystem.GetBuffedDamage(baseDamage);
        }

        // 실제 사격 수행
        ExecuteRaycast(finalDamage);
    }

    void ExecuteRaycast( int _damage )
    {
        RaycastHit hit;
        Vector3 rayOrigin = cameraTarget.position;
        Vector3 rayDirection = cameraTarget.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, weapon.stat.Range))
        {
            Debug.Log(hit.transform.name + " 적중 데미지: " + _damage);

            //Enemy enemy = hit.transform.GetComponent<Enemy>();
            //if (enemy != null)
            //{
            //    enemy.TakeDamage(damage);
            //}
        }
    }
}
