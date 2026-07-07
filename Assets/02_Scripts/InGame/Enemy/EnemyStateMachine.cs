using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Idle,
    Patrol,      // 1. 순찰
    Investigate, // 2. 수색 (소리 감지)
    Chase,       // 3. 추적 (눈으로 발견)
    Attack,      // 4. 공격
    Retreat      // 5. 도망 및 치료
}
public interface IState
{
    void EnterState();  // 이 상태로 처음 들어왔을 때 
    void UpdateState(); // 매 프레임 실행될 로직 
    void ExitState();   // 이 상태에서 빠져나갈 때 
}
public class EnemyStateMachine : MonoBehaviour
{
    // 전술한 5가지 상태 리스트를 여기서 관리
    public IState CurrentState { get; private set; }

    [SerializeField] private string currentStatusName;
    public LayerMask playerLayerMask;
    // 다른 상태 클래스들이 공통으로 쓸 유니티 컴포넌트들을 미리 캐싱
    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public Transform targetPlayer;
    [HideInInspector] public Vector3 lastKnownPosition; // 플레이어를 마지막으로 목격한 위치
    public Animator animator;
    // 예시용 상태 객체들
    public EnemyIdleState idleState; 
    public EnemyPatrolState patrolState;
    public EnemyInvestigateState investigateState;
    public EnemyChaseState chaseState;
    public EnemyAttackState attackState;   
    public EnemyRetreatState retreatState;
    public EnemyDeadState deadState;
    
    
    // ... 다른 상태들도 추가 가능

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        // 상태 인스턴스 생성 (생성자를 통해 이 스크립트의 참조를 넘김)
        idleState = new EnemyIdleState(this);
        patrolState = new EnemyPatrolState(this);
        investigateState = new EnemyInvestigateState(this);
        chaseState = new EnemyChaseState(this);
        attackState = new EnemyAttackState(this);
        retreatState = new EnemyRetreatState(this);
        deadState = new EnemyDeadState(this);
    }

    private void Start()
    {
        // 시작 상태 지정 
        ChangeState(idleState);
    }

    private void Update()
    {
        // 현재 활성화된 상태의 Update를 매 프레임 실행
        CurrentState?.UpdateState();
    }

    // 상태를 바꾸는 함수
    public void ChangeState(IState newState)
    {
        if (CurrentState == newState) return;

        CurrentState?.ExitState();  // 1. 기존 상태 나가기
        CurrentState = newState; // 2. 상태 교체
       
        currentStatusName = newState.GetType().Name;
        CurrentState?.EnterState(); // 3. 새 상태 들어가기
    }
}