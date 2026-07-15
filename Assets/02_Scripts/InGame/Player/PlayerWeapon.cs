using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerWeapon : MonoBehaviour
{
    // 무기 기본 스탯 변수
    public string weaponName;
    public WeaponStat stat;
    public int currentMag;

    public Transform cameraTarget;
    public PlayerMagazine reloadSystem;
    public PlayerShoot shootSystem;
    public WeaponModification modificationSystem;
    public PlayerBuff buffSystem;

    void Awake( )
    {
        // 동적 생성 시 자동으로 자기 자신에게 붙은 컴포넌트들을 찾아 연결
        reloadSystem = GetComponent<PlayerMagazine>();
        shootSystem = GetComponent<PlayerShoot>();
        buffSystem = GetComponent<PlayerBuff>();
        modificationSystem = GetComponent<WeaponModification>();
    }

    public void Init( ItemData weaponData )
    {
        weaponName = weaponData.ItemName;

        stat.Damage = 0f;
        stat.AttackInterval = 0f;
        stat.Range = 0f;
        stat.MagazineSize = 0;
        stat.ReloadTime = 0f;

        // UseItemParameterList의 문자열 데이터를 숫자로 변환하여 할당
        if (weaponData.UseItemParameterList != null && weaponData.UseItemParameterList.Length >= 5)
        {
          //int.Parse(weaponData.UseItemParameterList[0]);
          //float.Parse(weaponData.UseItemParameterList[1]);
          //float.Parse(weaponData.UseItemParameterList[2]);
          //int.Parse(weaponData.UseItemParameterList[3]);
          //float.Parse(weaponData.UseItemParameterList[4]);
        }

        currentMag = stat.MagazineSize;
    }

}