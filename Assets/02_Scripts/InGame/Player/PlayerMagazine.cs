using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMagazine : MonoBehaviour
{
    public bool isReloading = false;
    private PlayerWeapon weapon;

    void Start( )
    {
        weapon = GetComponent<PlayerWeapon>();
    }

    void Update( )
    {
        // 이미 재장전 중이면 무시
        if (isReloading) return;

        // R 키를 누르면 재장전
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            TryReload();
        }
    }

    // 총알이 남아있는지 확인
    public bool HasAmmo( )
    {
        if (weapon == null) return false;
        return weapon.currentMag > 0;
    }

    // 총알 소모
    public void ConsumeAmmo( )
    {
        if (weapon == null) return;

        if (weapon.currentMag > 0)
        {
            weapon.currentMag--;
        }
    }
    public void TryReload( )
    {
        if (weapon == null) return;
        if (weapon.currentMag == weapon.stat.MagazineSize || isReloading) return;

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine( )
    {
        isReloading = true;

        // BuffSystem이 있다면 버프가 적용된 실시간 재장전 시간 계산
        float finalReloadTime = weapon.stat.ReloadTime;
        if (weapon.buffSystem != null)
        {
            finalReloadTime = weapon.stat.ReloadTime * weapon.buffSystem.reloadTimeMultiplier;
        }

        Debug.Log($"재장전 시작: {finalReloadTime}초 대기");
        yield return new WaitForSeconds(finalReloadTime);

        weapon.currentMag = weapon.stat.MagazineSize;
        isReloading = false;
        Debug.Log("재장전 완료: " + weapon.currentMag + "발");
    }
   
}
