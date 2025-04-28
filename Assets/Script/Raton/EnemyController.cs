using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // Referencias a componentes
    [HideInInspector] public FSM StateMachine;
    [HideInInspector] public ILineOfSight LineOfSight;
    [HideInInspector] public EnemySteering Steering;
    [HideInInspector] public WaypointSystem WaypointSystem;
    [HideInInspector] public Transform PlayerTransform;

    // Referencias a estados
    [HideInInspector] public State PatrolState;
    [HideInInspector] public State AlertState;
    [HideInInspector] public State AttackState;
    [HideInInspector] public State EscapeState;

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

    void Awake()
    {
        if (enemyVision == null)
        {
            enemyVision = GetComponent<EnemyVision>();
        }
        LineOfSight = GetComponent<ILineOfSight>();
        Steering = GetComponent<EnemySteering>();
        EnemyAnimator = GetComponent<Animator>();
        WaypointSystem = GetComponent<WaypointSystem>();
        audioSource = GetComponent<AudioSource>();
        
        if (Steering != null)
        {
            Steering.Initialize();
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
        
        PatrolState.AddTransition(StateEnum.EnemyAlert, AlertState);
        PatrolState.AddTransition(StateEnum.Attack, AttackState);
        AlertState.AddTransition(StateEnum.EnemyPatrol, PatrolState);
        AlertState.AddTransition(StateEnum.Attack, AttackState);
        AttackState.AddTransition(StateEnum.EnemyEscape, EscapeState);
        AttackState.AddTransition(StateEnum.EnemyAlert, AlertState);
        EscapeState.AddTransition(StateEnum.EnemyAlert, AlertState);
    }

    void Start()
    {
        PlayerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        
        if (Steering != null)
        {
            Steering.Initialize();
        }
        
        enemyVision.UpdateDetection();
        
        if (enemyVision.usePeripheralVision)
        {
            StateMachine.SetInit(PatrolState);
        }
        else
        {
            StateMachine.SetInit(PatrolState);
        }
    }

    void Update()
    {
        enemyVision.UpdateDetection();
        
        StateMachine.Update();
    }
}