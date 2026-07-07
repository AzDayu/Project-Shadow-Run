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
        Collider[] hitColliders = Physics.OverlapSphere(stateMachine.transform.position, detectionRadius, stateMachine.playerLayerMask);

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
                    stateMachine.targetPlayer = foundPlayer;
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
        currentSpeed = stateMachine.agent.velocity.magnitude;
        if (currentSpeed < 0.2f)
        {
            currentSpeed = 0f;
        }
        stateMachine.animator.SetFloat("MoveSpeed", currentSpeed);
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
        stateMachine.ChangeState(stateMachine.patrolState);
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
        stateMachine.targetPlayer = null;
    }

    public override void UpdateState()
    {
        UpdateAnimation();
        if (DetectPlayer())
        {
            stateMachine.ChangeState(stateMachine.chaseState);
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
        stateMachine.agent.speed = 5f;
    }

    public override void UpdateState()
    {
        UpdateAnimation();
        if (stateMachine.targetPlayer == null)
        {
            stateMachine.ChangeState(stateMachine.patrolState);
            return;
        }
       
        if (DetectPlayer())
        {
            lostTimer = 0f;
            
            stateMachine.lastKnownPosition = stateMachine.targetPlayer.position;
            //사격중에 마지막 위치를 업데이트 하지 않는 문제 있음
            stateMachine.agent.SetDestination(stateMachine.lastKnownPosition);


            if (Vector3.Distance(stateMachine.transform.position, stateMachine.targetPlayer.position) <= 10f)
            {
                Debug.Log("[Chase] 플레이어를 포착했습니다. 공격(Attack) 상태로 전환합니다.");
                stateMachine.ChangeState(stateMachine.attackState);
            }
        }
        else
        {
            lostTimer += Time.deltaTime;
            
            stateMachine.agent.SetDestination(stateMachine.lastKnownPosition);
           
            if (lostTimer >= maxLostTime)
            {
                Debug.Log("[Chase] 플레이어를 완전히 놓쳤습니다. 수색(Investigate) 상태로 전환합니다.");
               
               // stateMachine.ChangeState(stateMachine.investigateState);
                stateMachine.ChangeState(stateMachine.patrolState);
                return;
            }
        }
    }

    public override void ExitState()
    {
        Debug.Log("[Chase] 추적 종료.");
    }
}
public class EnemyAttackState : BaseEnemyState
{
    private float lastAttackTimer = 0f;
    private float attackInterval = 1.0f; // 공격 간격 (쿨타임)

    private float targetLostTimer = 0f;
    private float maxTargetLostTime = 0.8f; // 시야에서 사라져도 사격 자세를 유지할 유예 시간

    public EnemyAttackState(EnemyStateMachine stateMachine) : base(stateMachine)
    {
        this.detectionRadius = 20f; // 사격 감지 범위
    }

    public override void EnterState()
    {
        Debug.Log("[Attack] 사격 개시.");
        lastAttackTimer = attackInterval;
        targetLostTimer = 0f;
        stateMachine.agent.ResetPath();
    }

    public override void UpdateState()
    {
        if (stateMachine.targetPlayer == null)
        {
            stateMachine.ChangeState(stateMachine.patrolState);
            return;
        }
        RotateTowardsTarget();

        if (DetectPlayer())
        {
            // 플레이어가 잘 보이면 상실 타이머 리셋
            targetLostTimer = 0f;

            // 공격 쿨타임 계산
            if (lastAttackTimer >= attackInterval)
            {
                EnemyAttack();
                lastAttackTimer = 0f; // 쿨타임 리셋
            }
            lastAttackTimer += Time.deltaTime;
        }
        else
        {
            targetLostTimer += Time.deltaTime;

            if (targetLostTimer >= maxTargetLostTime)
            {
                Debug.Log("[Attack] 플레이어가 시야에서 완전히 사라졌습니다. 추격으로 전환합니다.");
                stateMachine.ChangeState(stateMachine.chaseState);
                return;
            }
        }
    }

    public override void ExitState()
    {
        Debug.Log("[Attack] 사격 중지.");
    }

    public void EnemyAttack()
    {
        Debug.Log("플레이어에게 총을 발사했습니다.");
        // TODO: 실제 레이캐스트 데미지를 플레이어에게 주는 로직 구현
        stateMachine.animator.SetTrigger("IsAttack");
    }

    private void RotateTowardsTarget()
    {
        Vector3 direction = (stateMachine.targetPlayer.position - stateMachine.transform.position).normalized;
        direction.y = 0; // 몬스터가 하늘이나 바닥으로 기울어지는 것 방지

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            stateMachine.transform.rotation = Quaternion.Slerp(stateMachine.transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
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
