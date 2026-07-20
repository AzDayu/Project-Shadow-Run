using System.Collections;
using UnityEngine;

public class PlayerTakeMedicine : MonoBehaviour, InterfaceUseItem
{
    private Coroutine _healingCoroutine;
    private PlayerStatusModel _playerStatus;
    private float _tickInterval = 0.2f; // 회복 틱 간격 
    private WaitForSeconds _waitTick; // GC 방지용 캐시 변수
    private float _nextUsableTime = 0f; // 쿨타임 계산 변수

    private void Awake()
    {
        // _waitTick 캐시 생성으로 메모리 파편화 방지
        _waitTick = new WaitForSeconds(_tickInterval);
    }
    
    public void Setup( PlayerStatusModel playerStatus )
    {
        _playerStatus = playerStatus;
    }

    private void OnDisable( )
    {
        if (_healingCoroutine != null)
        {
            StopCoroutine(_healingCoroutine);
            _healingCoroutine = null;
        }
    }

    private void OnEnable( )
    {
        // InventoryManager에 자신이 아이템 사용 주체임을 등록
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SetItemUser(this);
        }
    }

    // InterfaceUseItem 인터페이스 구현
    public bool TryUseItem( UseableItem itemData )
    {
        return TryUse(itemData);
    }

    // InventoryManager에서 UseableItem 데이터를 전달받아 사용 시도
    public bool TryUse( UseableItem itemData )
    {
        if (_playerStatus == null || itemData == null)
        {
            return false;
        }

        if (Time.time < _nextUsableTime)
        {
            Debug.LogWarning("재사용 대기 중.");
            return false;
        }

        // 쿨타임 갱신
        _nextUsableTime = Time.time + itemData.ReUseCoolTime;

        StartHealing(itemData.HpPerVariation, itemData.Duration);
        return true;
    }


    public void StartHealing( float perAmount, float duration )
    {
        // Null 예외 방지 안전장치
        if (_playerStatus == null)
        {
            Debug.LogError("PlayerStatusModel is null. Setup() 먼저 실행.");
            return;
        }


        // 지속 시간이 0인 경우 코루틴 없이 즉시 회복 후 종료
        if (duration <= 0)
        {
            _playerStatus.RecoverHP(perAmount);
            Debug.Log($"[즉시 회복] 현재 체력: {_playerStatus.CurrentHP}");

            return; 
        }

        // 기존 실행 코루틴 중지
        if (_healingCoroutine != null)
        {
            StopCoroutine(_healingCoroutine);
        }

        _healingCoroutine = StartCoroutine(RecoveryRoutine(perAmount, duration));
    }

    private IEnumerator RecoveryRoutine( float perAmount, float duration )
    {
        // 총 실행할 틱(Tick) 횟수 계산 
        int totalTicks = Mathf.RoundToInt(duration / _tickInterval);

        // 한 틱당 들어가야 할 소수점 단위의 실질적 힐량
        float tickAmount = perAmount * _tickInterval;
        // 소수점 단위의 힐량을 누적할 변수
        int tickCount = 0;

        // 렉결려도 최소 회복 보장
        while (tickCount < totalTicks)
        {
            _playerStatus.RecoverHP(tickAmount);

            tickCount++;

            float remainingTime = duration - ( tickCount * _tickInterval );
            Debug.Log($"[회복 중] 현재 체력: {_playerStatus.CurrentHP} | 남은 지속 시간: {Mathf.Max(0f, remainingTime):F1}초");

            // 설정한 틱 간격만큼 대기
            yield return _waitTick;
        }     

        _healingCoroutine = null;
    }

}