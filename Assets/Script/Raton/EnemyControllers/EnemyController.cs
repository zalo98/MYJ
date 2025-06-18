using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [HideInInspector] public FSM StateMachine;
    [HideInInspector] public ILineOfSight LineOfSight;
    [HideInInspector] public EnemySteering steering;
    [HideInInspector] public MouseMovement mouseMovement;
    [HideInInspector] public Transform PlayerTransform;
    [HideInInspector] public State PatrolState;
    [HideInInspector] public State AlertState;
    [HideInInspector] public State AttackState;
    [HideInInspector] public State EscapeState;
    [HideInInspector] public State EnemylookingState;
    
    [HideInInspector] public Animator EnemyAnimator;
    [HideInInspector] public AudioSource audioSource;

    [Header("Configuración General")]
    public float walkSpeed;
    public float runSpeed;
    public float rotationSpeed;
    public AudioClip escapeSound;
    public EnemyVision enemyVision;
    public ObstacleAvoidance obstacleAvoidance;

    protected virtual void InitializeStates() { }

    private void Awake()
    {
        if (enemyVision == null)
            enemyVision = GetComponent<EnemyVision>();
        LineOfSight = GetComponent<ILineOfSight>();
        steering = GetComponent<EnemySteering>();
        EnemyAnimator = GetComponent<Animator>();
        mouseMovement = GetComponent<MouseMovement>();
        audioSource = GetComponent<AudioSource>();

        if (steering != null)
            steering.Initialize();
        else
            Debug.LogError("EnemySteering no encontrado en el objeto.");

        StateMachine = new FSM();

        InitializeStates();
    }

    public void Start()
    {
        PlayerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (steering != null)
            steering.Initialize();

        enemyVision.UpdateDetection();
    }

    private void Update()
    {
        enemyVision.UpdateDetection();
        StateMachine.Update();
    }
}