using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // Referencias a componentes
    [HideInInspector] public EnemyStateMachine StateMachine;
    [HideInInspector] public EnemyLineOfSight LineOfSight;
    [HideInInspector] public EnemySteering Steering;

    // Referencias a estados
    [HideInInspector] public IEnemyState PatrolState;
    [HideInInspector] public IEnemyState EscapingState;
    [HideInInspector] public IEnemyState LookingState;

    // Referencias a componentes internos
    [HideInInspector] public Transform PlayerTransform;
    [HideInInspector] public Animator EnemyAnimator;

    // Configuración general
    [Header("Configuración General")]
    public float walkSpeed = 2.0f;
    public float runSpeed = 5.0f;
    public float rotationSpeed = 120.0f;

    void Awake()
    {
        // Obtener referencias a los componentes
        StateMachine = GetComponent<EnemyStateMachine>();
        LineOfSight = GetComponent<EnemyLineOfSight>();
        Steering = GetComponent<EnemySteering>();
        EnemyAnimator = GetComponent<Animator>();

        // Obtener referencias a los estados
        PatrolState = GetComponent<PatrolState>();
        EscapingState = GetComponent<EscapeState>();
        LookingState = GetComponent<LookingState>();

        // Verificar que todos los componentes necesarios existen
        if (StateMachine == null || LineOfSight == null || Steering == null || EnemyAnimator == null)
            Debug.LogError("Falta algún componente necesario en EnemyController");

        if (PatrolState == null || EscapingState == null || LookingState == null)
            Debug.LogError("Falta algún componente de estado en EnemyController");
    }

    void Start()
    {
        // Encontrar al jugador
        PlayerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // Inicializar componentes
        StateMachine.Initialize(this);
        LineOfSight.Initialize();
        Steering.Initialize();
    }
}
