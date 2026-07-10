using UnityEngine;
public abstract class BaseEnemyState : IState
{
    // 자식 상태 클래스들이 공용으로 사용할 상태머신 참조
    protected EnemyStateMachine stateMachine;

    // 기본 감지 반경 세팅 (필요시 자식에서 따로 정의 가능)
    protected float detectionRadius = 15f;
    float currentSpeed;
    // 생성자
    public BaseEnemyState(EnemyStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();

    /// <summary>
    /// 모든 자식 상태(Idle, Patrol, Investigate 등)가 쓸 수 있는 플레이어 감지 함수
    /// </summary>
    protected bool DetectPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(stateMachine.transform.position, detectionRadius, stateMachine._playerLayerMask);

        if (hitColliders.Length > 0)
        {
            Transform foundPlayer = hitColliders[0].transform;

            Vector3 startPos = stateMachine.transform.position + Vector3.up * 0.5f;

            Vector3 targetPos = foundPlayer.position + Vector3.up * 0.5f;
            Vector3 direction = (targetPos - startPos).normalized;
            float distance = Vector3.Distance(startPos, targetPos);

           // Debug.Log($"[디버그] 1단계 오버랩 성공. {foundPlayer.name} 조준 중. 거리: {distance}");

            bool isHit = Physics.Raycast(startPos, direction, out RaycastHit hit, distance);

            if (isHit)
            {
                Color rayColor = hit.transform.CompareTag("Player") ? Color.green : Color.red;
                Debug.DrawLine(startPos, hit.point, rayColor);

                if (hit.transform.CompareTag("Player"))
                {
                    stateMachine._targetPlayer = foundPlayer;
                    Debug.Log($"[{this.GetType().Name}] 성공: 플레이어를 직접 눈으로 확인했습니다!");
                    return true;
                }
                else
                {
                    Debug.Log($"[{this.GetType().Name}] 실패: 플레이어 방향을 보았으나 중간에 [{hit.transform.name}]이(가) 가로막고 있습니다. 태그: {hit.transform.tag}");
                }
            }
            else
            {
                Debug.DrawRay(startPos, direction * distance, Color.yellow);
                Debug.Log($"[{this.GetType().Name}] 실패: 플레이어 방향으로 레이저를 쐈으나 아무것도 맞지 않고 빗나갔습니다.");
            }
        }

        return false;
    }
    protected void UpdateAnimation() 
    {
        currentSpeed = stateMachine._agent.velocity.magnitude;
        if (currentSpeed < 0.2f)
        {
            currentSpeed = 0f;
        }
        stateMachine._animator.SetFloat("MoveSpeed", currentSpeed);
    }
}
public class EnemyIdleState : BaseEnemyState
{
    public EnemyIdleState(EnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
        Debug.Log("상태 진입: [Idle] 대기상태.");
    }

    public override void UpdateState()
    {
        stateMachine.ChangeState(stateMachine._patrolState);
    }

    public override void ExitState() { }
}
public class EnemyPatrolState : BaseEnemyState
{
    public EnemyPatrolState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
        this.detectionRadius = 15f;
    }

    public override void EnterState()
    {
        Debug.Log("상태 진입: [Patrol] 순찰을 시작합니다.");
        stateMachine._targetPlayer = null;
    }

    public override void UpdateState()
    {
        UpdateAnimation();
        if (DetectPlayer())
        {
            stateMachine.ChangeState(stateMachine._chaseState);
            return;
        }
    }

    public override void ExitState() { }
}
public class EnemyInvestigateState : BaseEnemyState
{
    // 생성자
    public EnemyInvestigateState(EnemyStateMachine stateMachine) : base(stateMachine) 
    { 
        this.detectionRadius = 20f; 
    }
    public override void EnterState() { }  // 이 상태로 처음 들어왔을 때 
    public override void UpdateState() { UpdateAnimation(); } // 매 프레임 실행될 로직 
    public override void ExitState() { }   // 이 상태에서 빠져나갈 때
}
public class EnemyChaseState : BaseEnemyState
{
    private float lostTimer;
    private float maxLostTime = 5.0f; // 시야에서 사라진 후 4초 동안은 마지막 위치로 추격

    // 이미 추적 중일 때는 더 넓은 범위(놓치는 범위)를 적용하기 위해 부모의 반경을 덮어씌움
    private float chaseLoseRadius = 25f;

    public EnemyChaseState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
        // 추적 중일 때의 감지 반경 설정
        this.detectionRadius = chaseLoseRadius;
    }

    public override void EnterState()
    {
        Debug.Log("[Chase] 추적 시작. 실시간 추격을 가동합니다.");
        lostTimer = 0f;

        // 진입 순간의 속도를 추격 속도로 올려줍니다
        stateMachine._agent.speed = 5f;
    }

    public override void UpdateState()
    {
        UpdateAnimation();
        if (stateMachine._targetPlayer == null)
        {
            stateMachine.ChangeState(stateMachine._patrolState);
            return;
        }
       
        if (DetectPlayer())
        {
            lostTimer = 0f;
            
            stateMachine._lastDetectPosition = stateMachine._targetPlayer.position;
            //사격중에 마지막 위치를 업데이트 하지 않는 문제 있음
            stateMachine._agent.SetDestination(stateMachine._lastDetectPosition);


            if (Vector3.Distance(stateMachine.transform.position, stateMachine._targetPlayer.position) <= 10f)
            {
                Debug.Log("[Chase] 플레이어를 포착했습니다. 공격(Attack) 상태로 전환합니다.");
                stateMachine.ChangeState(stateMachine._attackState);
            }
        }
        else
        {
            lostTimer += Time.deltaTime;
            
            stateMachine._agent.SetDestination(stateMachine._lastDetectPosition);
           
            if (lostTimer >= maxLostTime)
            {
                Debug.Log("[Chase] 플레이어를 완전히 놓쳤습니다. 수색(Investigate) 상태로 전환합니다.");
               
               // stateMachine.ChangeState(stateMachine.investigateState);
                stateMachine.ChangeState(stateMachine._patrolState);
                return;
            }
        }
    }

    public override void ExitState()
    {
        Debug.Log("[Chase] 추적 종료.");
    }
}
public enum AttackPhase
{
    Aim,
    Cooldown
}

public class EnemyAttackState : BaseEnemyState
{
    private float aimTimer; //조준 후에 발사까지 걸리는 시간 
    private float aimDuration = 0.5f; //추후에 무기 등에서 값을 받아올수도 있음 *EnemyData에 필요

    private float cooldownTimer;//다음 공격까지 걸리는 시간 *EnemyData에 필요
    private float attackInterval = 1.0f;

    private Vector3 aimPosition;

    private float targetLostTimer = 0f;
    private float maxTargetLostTime = 0.8f;

    private AttackPhase currentPhase;

    public EnemyAttackState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
        this.detectionRadius = 20f; // 사격 감지 범위
    }

    public override void EnterState()
    {
        Debug.Log("[Attack] 상태 시작");

        stateMachine._agent.ResetPath();

        targetLostTimer = 0f;

        StartAim();
    }

    public override void UpdateState()
    {
        if (stateMachine._targetPlayer == null)
        {
            stateMachine.ChangeState(stateMachine._patrolState);
            return;
        }
        RotateTowardsTarget(aimPosition);

        if (!DetectPlayer())
        {
            targetLostTimer += Time.deltaTime;

            if (targetLostTimer >= maxTargetLostTime)
            {
                stateMachine.ChangeState(stateMachine._chaseState);
                Debug.Log("[Attack] 플레이어가 시야에서 완전히 사라졌습니다. 추격으로 전환합니다.");
            }

            return;
        }

        targetLostTimer = 0f;
        stateMachine._lastDetectPosition = stateMachine._targetPlayer.position;
        switch (currentPhase)
        {
            case AttackPhase.Aim:

                aimTimer += Time.deltaTime;

                if (aimTimer >= aimDuration)
                {
                    EnemyAttack();

                    currentPhase = AttackPhase.Cooldown;
                    cooldownTimer = 0f;

                   // stateMachine.animator.SetBool("IsAiming", false);
                }

                break;

            case AttackPhase.Cooldown:

                cooldownTimer += Time.deltaTime;

                if (cooldownTimer >= attackInterval)
                {
                    StartAim();
                }

                break;
        }
    }

    public override void ExitState()
    {
        Debug.Log("[Attack] 상태 중지.");
    }

    public void EnemyAttack()
    {
        stateMachine._animator.SetTrigger("IsAttack");

        Vector3 muzzle = stateMachine.transform.position + Vector3.up * 1.5f;

        Vector3 dir = (aimPosition - muzzle).normalized;
        Debug.Log("[Attack] 발사.");
        stateMachine._enemyBase.CurrentWeapon.Fire(muzzle,dir);
       
    }

    private void RotateTowardsTarget(Vector3 aimPosition)
    {
        Vector3 direction = aimPosition.normalized;
        direction.y = 0; // 몬스터가 하늘이나 바닥으로 기울어지는 것 방지

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            stateMachine.transform.rotation = Quaternion.Slerp(stateMachine.transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
    private void StartAim()
    {
        currentPhase = AttackPhase.Aim;

        aimTimer = 0f;

        // 플레이어 현재 위치를 저장
        aimPosition = stateMachine._targetPlayer.position + Vector3.up * 0.5f;

        // 약간의 오차 추가
        //aimPosition += new Vector3(Random.Range(-0.25f, 0.25f),Random.Range(-0.1f, 0.1f), Random.Range(-0.25f, 0.25f));

        // 조준 애니메이션
       // stateMachine.animator.SetBool("IsAiming", true);
    }
}
public class EnemyRetreatState : BaseEnemyState
{
    // 생성자
    public EnemyRetreatState(EnemyStateMachine stateMachine) : base(stateMachine) { }
    public override void EnterState() { }  // 이 상태로 처음 들어왔을 때 
    public override void UpdateState() { } // 매 프레임 실행될 로직 
    public override void ExitState() { }   // 이 상태에서 빠져나갈 때
}
public class EnemyDeadState : BaseEnemyState
{
    // 생성자
    public EnemyDeadState(EnemyStateMachine stateMachine) : base(stateMachine) { }
    public override void EnterState() { }  // 이 상태로 처음 들어왔을 때 
    public override void UpdateState() { } // 매 프레임 실행될 로직 
    public override void ExitState() { }   // 이 상태에서 빠져나갈 때
}
