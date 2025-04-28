using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // Referencias a componentes
    [HideInInspector] public EnemyStateMachine StateMachine;
    [HideInInspector] public EnemyLineOfSight LineOfSight;
    [HideInInspector] public EnemySteering Steering;
    [HideInInspector] public WaypointSystem WaypointSystem;

    // Referencias a estados
    [HideInInspector] public IEnemyState PatrolState;
    [HideInInspector] public IEnemyState EscapingState;
    [HideInInspector] public IEnemyState LookingState;

    // Referencias a componentes internos
    [HideInInspector] public Transform PlayerTransform;
    [HideInInspector] public Animator EnemyAnimator;

    [HideInInspector] public AudioSource audioSource;

    // Configuración general
    [Header("Configuración General")]
    public float walkSpeed;
    public float runSpeed;
    public float rotationSpeed;
    public AudioClip escapeSound;

    void Awake()
    {
        // Obtener referencias a los componentes
        LineOfSight = GetComponent<EnemyLineOfSight>();
        Steering = GetComponent<EnemySteering>();
        EnemyAnimator = GetComponent<Animator>();
        WaypointSystem = GetComponent<WaypointSystem>();
        audioSource = GetComponent<AudioSource>();

        //audioSource.loop = true;

        // Crear la máquina de estados
        StateMachine = new EnemyStateMachine();

        // Crear los estados
        PatrolState = new PatrolState();
        EscapingState = new EscapeState();
        LookingState = new LookingState(this); // Pasamos 'this' como host para coroutines
    }

    void Start()
    {
        // Encontrar al jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerTransform = player.transform;
        }

        // Inicializar componentes
        StateMachine.Initialize(this);
        LineOfSight.Initialize();
        Steering.Initialize();
    }

    void Update()
    {
        // Actualizar la máquina de estados
        StateMachine.UpdateState();
    }
}