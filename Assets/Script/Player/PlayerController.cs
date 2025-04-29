using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private FSM fsm;
    private Rigidbody rb;
    private PlayerAnimationController animController;

    // Estados del jugador
    private PlayerIdleState idleState;
    private PlayerWalkState walkState;
    private PlayerRunState runState;

    // Configuración del movimiento
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;

    // Control de fisicas
    [SerializeField] private float groundDrag = 5f;

    private Vector3 moveDirection;

    private void Awake()
    {
        // Obtener componentes
        rb = GetComponent<Rigidbody>();
        animController = GetComponent<PlayerAnimationController>();

        if (animController == null)
        {
            animController = gameObject.AddComponent<PlayerAnimationController>();
        }

        // Configurar el Rigidbody
        ConfigureRigidbody();

        // Inicializar FSM
        InitializeFSM();
    }

    private void ConfigureRigidbody()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Evita que el personaje rote por físicas
        rb.drag = groundDrag; // Configurar la fricción
    }

    private void InitializeFSM()
    {
        fsm = new FSM();

        // Crear estados
        idleState = new PlayerIdleState(fsm, this, animController);
        walkState = new PlayerWalkState(fsm, this, animController);
        runState = new PlayerRunState(fsm, this, animController);

        // Configurar transiciones
        idleState.AddTransition(StateEnum.PlayerWalk, walkState);
        idleState.AddTransition(StateEnum.PlayerRun, runState);

        walkState.AddTransition(StateEnum.PlayerIdle, idleState);
        walkState.AddTransition(StateEnum.PlayerRun, runState);

        runState.AddTransition(StateEnum.PlayerIdle, idleState);
        runState.AddTransition(StateEnum.PlayerWalk, walkState);

        // Establecer estado inicial
        fsm.SetInit(idleState);
    }

    private void Update()
    {
        // Actualizar la máquina de estados
        fsm.Update();
    }

    private void FixedUpdate()
    {
        // Aplicar el movimiento final al Rigidbody
        ApplyMovement();
    }

    private void ApplyMovement()
    {
        if (moveDirection != Vector3.zero)
        {
            // Rotación del personaje
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

            // Aplicar movimiento
            float currentSpeed = GetCurrentSpeed();
            Vector3 newVelocity = moveDirection * currentSpeed;
            newVelocity.y = rb.velocity.y; // Mantener la caída natural por gravedad
            rb.velocity = newVelocity;
        }
        else
        {
            // Detener movimiento horizontal pero mantener el vertical
            Vector3 newVelocity = rb.velocity;
            newVelocity.x = 0;
            newVelocity.z = 0;
            rb.velocity = newVelocity;
        }
    }

    // Métodos públicos para ser llamados por los estados

    public void SetMoveDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
    }

    public float GetWalkSpeed()
    {
        return walkSpeed;
    }

    public float GetRunSpeed()
    {
        return runSpeed;
    }

    private float GetCurrentSpeed()
    {
        State currentState = fsm.GetCurrentState();

        if (currentState == runState) return runSpeed;
        if (currentState == walkState) return walkSpeed;

        return 0f;
    }

    // Para debug o UI, obtener el nombre del estado actual
    public string GetCurrentStateName()
    {
        State currentState = fsm.GetCurrentState();

        if (currentState == idleState) return "Idle";
        if (currentState == walkState) return "Walk";
        if (currentState == runState) return "Run";

        return "Unknown";
    }
}
