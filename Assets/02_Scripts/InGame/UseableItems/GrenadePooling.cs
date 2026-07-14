using UnityEngine;

public class GrenadePooling : MonoBehaviour
{
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private int poolSize = 10; // 미리 만들어둘 수류탄 개수

    private GameObject[] _pool;

    private void Start( )
    {
        // 배열 크기 할당 및 수류탄 미리 생성
        _pool = new GameObject[poolSize];

        // 물리 재질을 루프 바깥에서 1회 생성
        PhysicsMaterial _sharedMaterial = new PhysicsMaterial("SampleSharedGrenadeBounce");
        _sharedMaterial.bounciness = 0.7f;
        _sharedMaterial.bounceCombine = PhysicsMaterialCombine.Maximum;
        _sharedMaterial.dynamicFriction = 0.1f;

        for (int i = 0; i < poolSize; i++)
        {
            _pool[i] = Instantiate(grenadePrefab);
            _pool[i].SetActive(false); // 처음은 비활성화

            // 생성된 수류탄들에게 생성된 물리 재질을 똑같이 주입
            Collider col = _pool[i].GetComponent<Collider>();
            if (col != null)
            {
                col.material = _sharedMaterial;
            }
        }
    }

    // 플레이어가 수류탄을 던지려고 할 때 호출할 함수
    public GameObject RightsOnFuzeGrenade( )
    {
        // 배열을 순회하며 현재 꺼져있는(사용 가능한) 수류탄을 찾음
        for (int i = 0; i < _pool.Length; i++)
        {
            if (_pool[i].activeSelf == false)
            {
                return _pool[i]; // 찾은 수류탄 반환
            }
        }

        // 남는 배열이 없으면 null 반환 (또는 새로 생성)
        return null;
    }
}