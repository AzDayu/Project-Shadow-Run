using UnityEngine;

public class WeaponModification : MonoBehaviour
{
    public string attachmentName;
    public string slotType; // 예: "Muzzle" (총구), "Sight" (조준경), "Magazine" (탄창)

    // 스탯 변동치 (더하는 값은 기본 0, 곱하는 값은 기본 1.0)
    public int DamageModifier = 0;
    public float FireRateModifier = 1.0f;
    public float RangeModifier = 1.0f;
    public int MagazineAmmoModifier = 0;
    public float ReloadTimeModifier = 1.0f;

    private PlayerWeapon _weapon;

    void Awake( )
    {
        _weapon = GetComponent<PlayerWeapon>();
    }


}