using System.Collections;
using UnityEngine;

public class ActivateMedicine : MonoBehaviour, IQuickSlotConsumeHandler
{
    private Coroutine _healingCoroutine;

    private void OnDisable( )
    {
        if (_healingCoroutine != null)
        {
            StopCoroutine(_healingCoroutine);
            _healingCoroutine = null;
        }
    }

    public bool CanHandleType( string useItemType )
    {
        return useItemType == "Medicine";
    }

    public void UseItem( ItemData itemData )
    {
        if (itemData == null)
            return;

        if (_healingCoroutine != null)
        {
            StopCoroutine(_healingCoroutine);
        }

        OnHealing(itemData);
    }

    public void OnHealing( ItemData itemData )
    {
        if (itemData == null)
        {
            return;
        }

        if (_healingCoroutine != null)
        {
            StopCoroutine(_healingCoroutine);
        }

        _healingCoroutine = StartCoroutine(RecoveryRoutine(itemData));
    }

    private IEnumerator RecoveryRoutine( ItemData itemData )
    {
        float useDelay = itemData.PreUseDelay; // 대기 시간
        int totalHealAmount = itemData.HpVariation; // 총 회복량
        float healingDuration = itemData.Duration; // 회복 지속 시간

        //  대기 시간
        if (useDelay > 0f)
        {
            yield return new WaitForSeconds(useDelay);
        }

        //  체력 회복
        if (healingDuration <= 0f)
        {
            // 즉시 회복
            if (PlayerStatus.Instance != null)
            {
                PlayerStatus.Instance.RecoverHP(totalHealAmount);
            }
        }
        else
        {
            // 지속 회복
            float timer = 0f;
            while (timer < healingDuration)
            {
                timer += Time.deltaTime;
                float perHealTick = ( totalHealAmount / healingDuration ) * Time.deltaTime;

                if (PlayerStatus.Instance != null && PlayerStatus.Instance.Model != null)
                {
                    PlayerStatus.Instance.RecoverHP(perHealTick);
                }
                yield return null;
            }
        }

        _healingCoroutine = null;

    }
}
