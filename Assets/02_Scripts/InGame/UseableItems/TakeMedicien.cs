using System.Collections;
using UnityEngine;

public class TakeMedicine : MonoBehaviour
{
    [SerializeField] private int healAmount; // 초당 회복량
    [SerializeField] private float healRate; // 회복 지속 시간
    [SerializeField] private int healDelay;  // 적용 대기 시간

    private Coroutine _healingCoroutine;

    private PlayerStatusModel _playerStatus;

    /// <summary>
    /// 외부에서 모델을 주입해 연동
    /// </summary>
    public void Setup( PlayerStatusModel playerStatus )
    {
        _playerStatus = playerStatus;
    }

    public void StartHealing( int amount, float rate, int delay )
    {
        // Null 예외 방지 안전장치
        if (_playerStatus == null)
        {
            Debug.LogError("PlayerStatusModel is null. Setup() 먼저 실행.");
            return;
        }

        healAmount = amount;
        healRate = rate;
        healDelay = delay;

        // 기존 실행 중 코루틴 중지
        if (_healingCoroutine != null)
        {
            StopCoroutine(_healingCoroutine);
        }

        _healingCoroutine = StartCoroutine(RecoveryRoutine());
    }

    private IEnumerator RecoveryRoutine( )
    {
        // 설정한 대기 시간만큼 대기
        yield return new WaitForSeconds(healDelay);

        float _elapsed = 0f; // 경과 시간 체크용 변수

        // 지속 시간(healRate)이 경과할 때까지 1초마다 반복
        while (_elapsed < healRate)
        {
            // PlayerStatusModel.cs의 회복 함수 호출
            _playerStatus.RecoverHP(healAmount); // 즉시 회복 
            _elapsed = _elapsed + 1f;            // 시간 누적 1초 간격

            Debug.Log($"[회복 중] 현재 체력: {_playerStatus.CurrentHP} | 남은 지속 시간: {healRate - _elapsed}초");

            yield return new WaitForSeconds(1f); // 회복 후 1초 간격
        }
        _healingCoroutine = null;
    }

}