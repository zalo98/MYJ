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

    [SerializeField] private Transform interactionPoint; // Punto desde donde se inicia la interaccion
    [SerializeField] private float interactionRadius = 2f; // Radio de interacción
    [SerializeField] private LayerMask interactionLayer; // Capa de objetos interactuables
    private Collider[] interactables = new Collider[10];

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
        rb.linearDamping = groundDrag; // Configurar la fricción
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

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
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
            // Rotación del personaje - solo si nos estamos moviendo
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

            // Aplicar movimiento con velocidad basada en el estado actual
            float currentSpeed = GetCurrentSpeed();
            Vector3 newVelocity = moveDirection * currentSpeed;

            // Mantener la componente Y de la velocidad (gravedad)
            newVelocity.y = rb.linearVelocity.y;

            // Asignar la velocidad al Rigidbody
            rb.linearVelocity = newVelocity;
        }
        else
        {
            // Si no hay dirección de movimiento, detener el movimiento horizontal
            Vector3 newVelocity = rb.linearVelocity;
            newVelocity.x = 0;
            newVelocity.z = 0;
            rb.linearVelocity = newVelocity;
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

    private void TryInteract()
    {
        Debug.Log("Tried Interacting");

        int elements = Physics.OverlapSphereNonAlloc(interactionPoint.position, interactionRadius, interactables, interactionLayer);

        if (elements == 0)
        {
            Debug.Log("No interactables found");
            return;
        }

        for (int i = 0; i < interactables.Length; i++)
        {
            var interactable = interactables[i];
            var interactableComponent = interactable.GetComponent<IInteractable>();

            if (interactableComponent != null)
            {
                interactableComponent.Interact();
                return;
            }
        }
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
