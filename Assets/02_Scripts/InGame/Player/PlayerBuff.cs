using UnityEngine;

public class PlayerBuff : MonoBehaviour
{
    // 1.0이 기본값 (예: 1.2면 20% 버프, 0.8이면 20% 디버프)
    public float damageMultiplier = 1.0f;
    public float fireRateMultiplier = 1.0f;
    public float rangeMultiplier = 1.0f;
    public float reloadTimeMultiplier = 1.0f;


    // 버프가 적용된 최종 데미지를 반환하는 함수
    public int GetBuffedDamage( int baseDamage )
    {
        float finalDamage = baseDamage * damageMultiplier;
        return Mathf.RoundToInt(finalDamage);
    }

    // 버프가 적용된 최종 연사 속도(딜레이)를 반환하는 함수
    public float GetBuffedFireRate( float baseFireRate )
    {
        // 연사 간격은 작아질수록 총이 빨리 나감
        return baseFireRate * fireRateMultiplier;
    }


}