using UnityEngine;
using UnityEngine.AI; 

public class EnemyBase : MonoBehaviour,IDamageable,IWeaponOwner
{
    public TestWeaponBase CurrentWeapon { get; private set; }
    [SerializeField] Transform _weaponSpawnPo;
    [SerializeField] TestWeaponBase _testWeapon;
    public float HP { get; private set; }
    public bool IsDead { get; private set; } = false;

    public float FrontDetectDistance = 20f; //추후에 데이터에서 받아와 사용
    public float SideDetectDistance = 10f;
    public float BackDetectDistance = 3f;

    //public EnemyData Data { get; private set; }
    public void Awake()
    {
        SetWeapon(_testWeapon);
    }
    void Update()
    {
        Debug.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * 5f, Color.blue);
    }
    public void SetWeapon(TestWeaponBase weapon)
    {
        CurrentWeapon = weapon;

        weapon.transform.SetParent(_weaponSpawnPo, false);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
    }

    public void TakeDamage(float damage) { }
    public void UseWeapon() { }
    public void Initialize(EnemyData enemyData) { }
}