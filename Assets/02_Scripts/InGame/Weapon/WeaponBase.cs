using System.Collections.Generic;
using UnityEngine;
public enum WeaponPartsType 
{

}
public struct WeaponStat
{
    public float Damage;
    public float AttackInterval;
    public int MagazineSize;
    public float Accuracy;
    public float Range;
    public float ReloadTime;
}
public interface IDamageable
{
    void TakeDamage(float damage);//DamageInfo구조체를 만들어 전달하면 더 많은 정보를 전달할수 있음
}
public interface IWeaponOwner 
{
    void UseWeapon();//무기를 사용할때 공격자를 전달할수있음
}
public abstract class WeaponBase : MonoBehaviour
{
    protected WeaponData _weaponData;

    protected WeaponStat _baseWeaponStat = new WeaponStat();
    protected WeaponStat _currentWeaponStat=new WeaponStat();
    protected int _remainBullets=0;

    protected Dictionary<WeaponPartsType,WeaponPartsData> _weaponPartsDic = new Dictionary<WeaponPartsType, WeaponPartsData>();
    public virtual void Initialize(WeaponData data) 
    {
        _weaponData=data;

        _baseWeaponStat.Damage = data.Damage;
        _baseWeaponStat.AttackInterval = data.AttackInterval;
        _baseWeaponStat.MagazineSize = data.MagazineSize;
        _baseWeaponStat.Accuracy = data.Accuracy;
        _baseWeaponStat.Range = data.Range;
        _baseWeaponStat.ReloadTime = data.ReloadTime;
        _currentWeaponStat= _baseWeaponStat;
    }

    //public abstract bool CanFire { get; }

    public virtual void Fire(Vector3 firePosition, Vector3 direction)//현재는 사용자의 위치에서 총이 발사됨, 추후에 총의 위치에서 발사되도록 수정될수있음
    {
        if (_remainBullets <= 0) 
        {
            return;
        }
        _remainBullets--;

        //Vector3 direction = (targetPosition - firePosition).normalized;
        if (Physics.Raycast(firePosition, direction.normalized, out RaycastHit hit, _currentWeaponStat.Range))
        {
            Debug.DrawRay(firePosition, direction * hit.distance, Color.red, _currentWeaponStat.Range);

            if (hit.transform.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(_currentWeaponStat.Damage);
            }

            else
            {
                Debug.Log("빗나감");
            }
        }
        
    }

    public virtual int Reload(int bulletAmount)
    {
        int newBulletAmonut = _remainBullets + bulletAmount;

        if (bulletAmount <= 0)
        {
            return bulletAmount;
        }

        if (_remainBullets >= _currentWeaponStat.MagazineSize)
        {
            return bulletAmount;
        }

        if (newBulletAmonut >= _currentWeaponStat.MagazineSize)
        {
            _remainBullets = _currentWeaponStat.MagazineSize;
            return newBulletAmonut - _currentWeaponStat.MagazineSize;
        }
        else 
        {
            _remainBullets = newBulletAmonut;
            return 0;
        }

    }
    public virtual void CalculateCurrentWeaponStat() 
    {
        _currentWeaponStat = WeaponStatCalculator.CalculateWeaponStat(_baseWeaponStat, _weaponPartsDic);
    }
    public virtual WeaponPartsData EquipWeaponPart(WeaponPartsData newPart)
    {
        WeaponPartsData oldPart = null;

        if (_weaponPartsDic.TryGetValue(newPart.PartsType, out oldPart))
        {
            _weaponPartsDic[newPart.PartsType] = newPart;
        }
        else
        {
            _weaponPartsDic.Add(newPart.PartsType, newPart);
        }

        CalculateCurrentWeaponStat();

        return oldPart;
    }
}