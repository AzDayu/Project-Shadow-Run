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

    public WeaponData WeaponData => _weaponData;
    public int RemainBullets => _weaponData?.RemainBullets ?? 0;

    [SerializeField] private Transform Muzzle;
    public Transform FirePoint => Muzzle;

    // TODO[안우재, 07/11]:
    // 테스트용 Awake메서드
    public void Awake()
    {
        // Initialize();
    }
    public virtual void Initialize(WeaponData weaponData)
    {
        _weaponData = weaponData;
    }

    //public abstract bool CanFire { get; }

    public virtual void Fire(Vector3 firePosition, Vector3 direction)
    {
        if (_weaponData == null)
            return;

        if (_weaponData.RemainBullets <= 0)
            return;

        _weaponData.RemainBullets--;

        if (Physics.Raycast(firePosition, direction.normalized, out RaycastHit hit, _weaponData.Range))
        {
            if (hit.transform.TryGetComponent(
                out IDamageable damageable))
            {
                damageable.TakeDamage(_weaponData.Damage);

                Debug.Log(
                    $"{hit.transform.name}이 " +
                    $"{_weaponData.Damage}만큼 피해를 입음"
                );
            }
        }
        else
        {
            Debug.Log("빗나감");
        }

    }

    public bool CanLoadAmmo(AmmoData ammoData)
    {
        if (ammoData == null)
            return false;

        return _weaponData.CompatibleCaliber ==
               ammoData.Caliber;
    }

    public virtual int Reload(AmmoData ammoData, int bulletAmount)
    {
        if (_weaponData == null || ammoData == null || bulletAmount <= 0)
        {
            return bulletAmount;
        }

        if (_weaponData.CompatibleCaliber != ammoData.Caliber)
            return bulletAmount;

        if (_weaponData.RemainBullets > 0 && !string.IsNullOrEmpty(_weaponData.LoadedAmmoItemId) &&
            _weaponData.LoadedAmmoItemId != ammoData.ItemId)
        {
            return bulletAmount;
        }

        int loadableCount = _weaponData.MagazineSize - _weaponData.RemainBullets;

        if (loadableCount <= 0)
            return bulletAmount;

        int loadedCount = Mathf.Min(loadableCount, bulletAmount);

        _weaponData.LoadedAmmoItemId = ammoData.ItemId;

        _weaponData.RemainBullets += loadedCount;

        return bulletAmount - loadedCount;
    }

    // TODO[안우재, 07/11]:
    // 파츠 아이템들 구현 완료 후 아래 메서드 확인 및 구축 필요
    // 아마 WeaponStat을 정하는 부분이라서 ItemData.cs의 WeaponData클래스에 들어갈 가능성 존재
    /*
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
    */
}
