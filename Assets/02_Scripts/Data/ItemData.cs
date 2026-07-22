using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public enum WeaponPartsType
{
    None,       // 없음
    Muzzle,     // 총구
    Scope,      // 조준경
    Magazine,   // 탄창
    Grip,       // 손잡이
    Stock       // 개머리판
}

public enum WeaponStatType
{
    Damage,
    AttackInterval,
    MagazineSize,
    Accuracy,
    Range,
    ReloadTime
}

public enum WeaponStatModifierType
{
    Add,        // 합연산
    Multiply,   // 곱연산
    Override    // 덮어쓰기
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

[System.Serializable]
public class ItemData : BaseData
{
    public string Name;
    public string ItemDescription;
    public string ItemType;
    public string Grade;
    public int MaxStackCount;
    public int SellingPrice;

    public string IconPath;
    public string PrefabPath;

    public string UseItemType;              // [0] 아이템 타입
    public string UseItemParameterList;
    public string[] UseItemParameters;

    public float PreUseDelay;               // [1] 사용 준비 시간 (초)
    public int   HpVariation;               // [2] 체력 변화량
    public float Duration;                  // [3] 지속 및 대기 시간    
    public float EffectRange;               // [4] 범위 (폭발 등)

    public void ParseUseItemParameters( )
    {
        /*if (UseItemParameterList == null || UseItemParameterList == "")
            UseItemParameters = Array.Empty<string>();
        else
            UseItemParameters = UseItemParameterList.Split(',');*/

        UseItemType = null;
        PreUseDelay = 0f;
        HpVariation = 0;
        Duration = 0f;
        EffectRange = 0f;
        if (string.IsNullOrWhiteSpace(UseItemParameterList))
        {
            UseItemParameters = Array.Empty<string>();

            return;
        }

        UseItemParameters = UseItemParameterList.Split(',');

        // 사용 아이템 타입 (string)
        if (UseItemParameters.Length > 0)
        {
            UseItemType = UseItemParameters[0].Trim();
        }

        // 사용 준비 시간 (float)
        if (UseItemParameters.Length > 1)
        {
            float.TryParse(UseItemParameters[1], out PreUseDelay);
        }

        // 데미지 / 회복량 (int)
        if (UseItemParameters.Length > 2)
        {
            int.TryParse(UseItemParameters[2], out HpVariation);
        }

        // 지속 및 대기 시간 (float)
        if (UseItemParameters.Length > 3)
        {
            float.TryParse(UseItemParameters[3], out Duration);
        }

        // 아이템 효과 범위 (float)
        if (UseItemParameters.Length > 4)
        {
            float.TryParse(UseItemParameters[4], out EffectRange);

        }
    }

}


[System.Serializable]
public class WeaponData : ItemData
{
    public float Damage;
    public int MagazineSize;
    public string AmmoType;
    public float AttackInterval;
    public float Accuracy;
    public float Range;
    public float ReloadTime;
    public float MaxDurability;
}

[System.Serializable]
public class WeaponPartsData : ItemData
{
    public WeaponPartsType PartsType;
    public WeaponStatType StatType;
    public WeaponStatModifierType ModifierType;
    public float Value;
}

[System.Serializable]
public class ItemModel
{
    public string InstanceId;      // 생성될 때마다 발급받는 고유 ID
    public string ItemId;          // DataManager에서 원본 ItemData를 찾기 위한 Key
    public int CurrentStackCount;  // 현재 겹쳐진 개수
}

[System.Serializable]
public class WeaponModel : ItemModel
{
    public int CurrentAmmo;                 // 현재 장전된 총알 수
    public float CurrentDurability;         // 현재 내구도
    public List<ItemModel> AttachedParts;   // 장착된 파츠들
}

[System.Serializable]
public class ShopItemData : BaseData 
{
    public string ItemId;        
    public int StockCount;       
}