using UnityEngine;
using UnityEngine.AI; 

public class EnemyBase : MonoBehaviour,IDamageable,IWeaponOwner
{
    public TestWeaponBase CurrentWeapon { get; private set; }
    [SerializeField] Transform _weaponSpawnPo;
    [SerializeField] TestWeaponBase _testWeapon;
    public float HP { get; private set; }
    //public EnemyData Data { get; private set; }
    public void Awake()
    {
        SetWeapon(_testWeapon);
    }
    public void SetWeapon(TestWeaponBase weapon)
    {
        CurrentWeapon = weapon;

        weapon.transform.SetParent(_weaponSpawnPo, false);
    }

    public void TakeDamage(float damage) { }
    public void UseWeapon() { }
    
}