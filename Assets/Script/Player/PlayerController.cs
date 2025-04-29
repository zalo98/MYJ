using UnityEngine;

public class PlayerController : MonoBehaviour, ITarget
{
    private FSM fsm;
    private Rigidbody rb;
    private PlayerAnimationController animController;
    
    private PlayerIdleState idleState;
    private PlayerWalkState walkState;
    private PlayerRunState runState;
    
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [SerializeField] private float groundDrag = 5f;

    private Vector3 moveDirection;

    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private LayerMask interactionLayer;
    private Collider[] interactables = new Collider[10];

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animController = GetComponent<PlayerAnimationController>();

        if (animController == null)
        {
            animController = gameObject.AddComponent<PlayerAnimationController>();
        }
        
        ConfigureRigidbody();
        
        InitializeFSM();
    }

    private void ConfigureRigidbody()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearDamping = groundDrag;
    }

    private void InitializeFSM()
    {
        fsm = new FSM();
        
        idleState = new PlayerIdleState(fsm, this, animController);
        walkState = new PlayerWalkState(fsm, this, animController);
        runState = new PlayerRunState(fsm, this, animController);
        
        idleState.AddTransition(StateEnum.PlayerWalk, walkState);
        idleState.AddTransition(StateEnum.PlayerRun, runState);

        walkState.AddTransition(StateEnum.PlayerIdle, idleState);
        walkState.AddTransition(StateEnum.PlayerRun, runState);

        runState.AddTransition(StateEnum.PlayerIdle, idleState);
        runState.AddTransition(StateEnum.PlayerWalk, walkState);
        
        fsm.SetInit(idleState);
    }

    private void Update()
    {
        fsm.Update();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    private void ApplyMovement()
    {
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            
            float currentSpeed = GetCurrentSpeed();
            Vector3 newVelocity = moveDirection * currentSpeed;
            
            newVelocity.y = rb.linearVelocity.y;
            
            rb.linearVelocity = newVelocity;
        }
        else
        {
            Vector3 newVelocity = rb.linearVelocity;
            newVelocity.x = 0;
            newVelocity.z = 0;
            rb.linearVelocity = newVelocity;
        }
    }

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
    
    public Transform GetTransform
    {
        get { return transform; }
    }
    
    public string GetCurrentStateName()
    {
        State currentState = fsm.GetCurrentState();

        if (currentState == idleState) return "Idle";
        if (currentState == walkState) return "Walk";
        if (currentState == runState) return "Run";

        return "Unknown";
    }
}
