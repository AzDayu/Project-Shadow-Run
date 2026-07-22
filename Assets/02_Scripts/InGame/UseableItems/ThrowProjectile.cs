using UnityEngine;

public class ThrowProjectile : MonoBehaviour, IQuickSlotConsumeHandler
{
    [SerializeField] private Transform _throwPoint;      // 투척 시작 위치 (카메라 앞/플레이어 손)
    [SerializeField] private float _defaultThrowForce = 15f; // 기본 투척력

    public bool CanHandleType( string useItemType )
    {
        return useItemType == "Grenade" || useItemType == "Dagger";
    }

    public void UseItem( ItemData itemData )
    {
        OnThrowConsumable(itemData);
    }
    public void OnThrowConsumable( ItemData itemData )
    {
        if (itemData == null || string.IsNullOrWhiteSpace(itemData.PrefabPath))
        {
            return;
        }

        // ItemData의 PrefabPath를 이용해 해당하는 프리팹 불러오기
        GameObject prefab = Resources.Load<GameObject>(itemData.PrefabPath);
        //!todo: 추후 오브젝트풀링을 사용
        
        if (prefab == null)
        {
            Debug.LogWarning("프리팹을 찾을 수 없습니다: " + itemData.PrefabPath);
            return;
        }

        // 투척 위치 계산
        Vector3 spawnPosition = transform.position;
        if (_throwPoint != null)
        {
            spawnPosition = _throwPoint.position;
        }

        // 투척용 프리팹 생성
        GameObject grenadeObject = Instantiate(prefab, spawnPosition, Quaternion.identity);

        // 수류탄 생성 시 파라미터 데이터 및 투척 방향 전달
        ActivateGrenade activeGrenade = grenadeObject.GetComponent<ActivateGrenade>();
        if (activeGrenade != null)
        {
            Vector3 throwDirection = transform.forward;
            activeGrenade.InitGrenade(itemData, throwDirection, _defaultThrowForce);
        }

        // 추후 구현할 HitDagger 대응 예시
        /*
        HitDagger dagger = spawnedObject.GetComponent<HitDagger>();
        if (dagger != null)
        {
            dagger.InitDagger(itemData, throwDirection, _defaultThrowForce);
            return;
        }
        */
    }
}