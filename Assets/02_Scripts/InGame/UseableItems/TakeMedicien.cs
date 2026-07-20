using System.Collections;
using UnityEngine;

public class TakeMedicine : MonoBehaviour
{
    [SerializeField] private int healAmount; // 초당 회복량
    [SerializeField] private uint healDuration; // 회복 지속 시간

    private Coroutine _healingCoroutine;
    private PlayerStatusModel _playerStatus;
    private float _tickInterval = 0.2f; // 회복 틱 간격 
    private WaitForSeconds _waitTick; // GC 방지용 캐시 변수

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

    public void StartHealing( int amount, uint duration )
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
            _playerStatus.RecoverHP(amount);
            Debug.Log($"[즉시 회복] 현재 체력: {_playerStatus.CurrentHP}");

            // 실행 중인 _healingCoroutine 중지 
            /*
            if (_healingCoroutine != null)
            {
                StopCoroutine(_healingCoroutine);
                _healingCoroutine = null;
            }
            */
            return; 
        }

        healAmount = amount;
        healDuration = duration;

        // 기존 실행 코루틴 중지
        if (_healingCoroutine != null)
        {
            StopCoroutine(_healingCoroutine);
        }

        _healingCoroutine = StartCoroutine(RecoveryRoutine());
    }

    private IEnumerator RecoveryRoutine( )
    {
        // 총 실행할 틱(Tick) 횟수 계산 
        int totalTicks = Mathf.RoundToInt(healDuration / _tickInterval);

        // 한 틱당 들어가야 할 소수점 단위의 실질적 힐량
        float hpPerTick = healAmount * _tickInterval;
        // 소수점 단위의 힐량을 누적할 변수
        int tickCount = 0;

        // 정확한 횟수만큼 루프를 돌려 렉이 걸려도 힐 횟수가 무조건 보장됩니다.
        while (tickCount < totalTicks)
        {
            _playerStatus.RecoverHP(hpPerTick);

            tickCount++;

            float remainingTime = (float)healDuration - ( tickCount * _tickInterval );
            Debug.Log($"[회복 중] 현재 체력: {_playerStatus.CurrentHP} | 남은 지속 시간: {Mathf.Max(0f, remainingTime):F1}초");

            // 설정한 틱 간격만큼 대기
            yield return _waitTick;
        }     

        _healingCoroutine = null;
    }

}