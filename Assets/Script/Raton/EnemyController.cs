using UnityEngine;
using UnityEngine.Serialization;

public class EnemyController : MonoBehaviour
{
    // Referencias a componentes
    [HideInInspector] public FSM StateMachine;
    [HideInInspector] public ILineOfSight LineOfSight;
    [HideInInspector] public EnemySteering steering;
    [HideInInspector] public WaypointSystem WaypointSystem;
    [HideInInspector] public Transform PlayerTransform;

    // Referencias a estados
    [HideInInspector] public State PatrolState;
    [HideInInspector] public State AlertState;
    [HideInInspector] public State AttackState;
    [HideInInspector] public State EscapeState;
    [HideInInspector] public State EnemySeek;
    [HideInInspector] public State EnemylookingState;
    [HideInInspector] public State MousePatrolState;
    [HideInInspector] public State MouseLookingState;
    [HideInInspector] public State MouseEscapeState;

    // Referencias a componentes internos
    [HideInInspector] public Animator EnemyAnimator;
    [HideInInspector] public AudioSource audioSource;

    // Configuración general
    [Header("Configuración General")]
    public float walkSpeed;
    public float runSpeed;
    public float rotationSpeed;
    public AudioClip escapeSound;
    public EnemyVision enemyVision;
    public ObstacleAvoidance obstacleAvoidance;

    private void Awake()
    {
        if (enemyVision == null)
        {
            enemyVision = GetComponent<EnemyVision>();
        }
        LineOfSight = GetComponent<ILineOfSight>();
        steering = GetComponent<EnemySteering>();
        EnemyAnimator = GetComponent<Animator>();
        WaypointSystem = GetComponent<WaypointSystem>();
        audioSource = GetComponent<AudioSource>();

        if (steering != null)
        {
            steering.Initialize();
        }
        else
        {
            Debug.LogError("EnemySteering no encontrado en el objeto.");
        }

        StateMachine = new FSM();
        
        PatrolState = new PatrolState(this, StateMachine);
        AlertState = new AlertState(this, StateMachine);
        AttackState = new AttackState(this, StateMachine);
        EscapeState = new EscapeState(this, StateMachine);
        EnemylookingState = new EnemyLookingState(this, StateMachine);

        MousePatrolState = new MousePatrolState(this, StateMachine);
        MouseLookingState = new LookingState(this, StateMachine);
        MouseEscapeState = new MouseEscapeState(this, StateMachine);
        
        PatrolState.AddTransition(StateEnum.EnemyAlert, AlertState);
        PatrolState.AddTransition(StateEnum.Attack, AttackState);
        PatrolState.AddTransition(StateEnum.EnemyLookingState, EnemylookingState);
        
        AlertState.AddTransition(StateEnum.EnemyPatrol, PatrolState);
        AlertState.AddTransition(StateEnum.Attack, AttackState);
        
        AttackState.AddTransition(StateEnum.EnemyEscape, EscapeState);
        AttackState.AddTransition(StateEnum.EnemyAlert, AlertState);
        AttackState.AddTransition(StateEnum.EnemyPatrol, PatrolState);
        
        EscapeState.AddTransition(StateEnum.EnemyAlert, AlertState);
        
        MousePatrolState.AddTransition(StateEnum.MouseLookingState, MouseLookingState);
        MousePatrolState.AddTransition(StateEnum.MouseEscapeState, MouseEscapeState);
        
        MouseLookingState.AddTransition(StateEnum.MouseEscapeState, MouseEscapeState);
        MouseLookingState.AddTransition(StateEnum.MousePatrolState, MousePatrolState);
        
        MouseEscapeState.AddTransition(StateEnum.MousePatrolState, MousePatrolState);
        MouseEscapeState.AddTransition(StateEnum.MouseLookingState, MouseLookingState);
    }

    private void Start()
    {
        PlayerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        
        if (steering != null)
        {
            steering.Initialize();
        }
        
        enemyVision.UpdateDetection();
        
        if (enemyVision.usePeripheralVision)
        {
            StateMachine.SetInit(PatrolState);
        }
        else
        {
            StateMachine.SetInit(MousePatrolState);
        }
    }

    private void Update()
    {
        enemyVision.UpdateDetection();
        
        StateMachine.Update();
    }
}