using UnityEngine;

public class PlayerController : MonoBehaviour, ITarget
{
    private FSM fsm;
    private Rigidbody rb;
    private PlayerAnimationController animController;
    
    private PlayerIdleState idleState;
    private PlayerWalkState walkState;
    private PlayerRunState runState;
    private PlayerInvisibleState invisibleState;
    
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [SerializeField] private float groundDrag = 5f;

    private Vector3 moveDirection;
    private bool spacePressedThisFrame = false;

    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private LayerMask interactionLayer;
    private Collider[] interactables = new Collider[10];

    public Renderer[] playerRenderers;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animController = GetComponent<PlayerAnimationController>();
        playerRenderers = GetComponentsInChildren<Renderer>();

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
        invisibleState = new PlayerInvisibleState(fsm, this, animController);
        
        idleState.AddTransition(StateEnum.PlayerWalk, walkState);
        idleState.AddTransition(StateEnum.PlayerRun, runState);
        idleState.AddTransition(StateEnum.PlayerInvisible, invisibleState);
        
        walkState.AddTransition(StateEnum.PlayerIdle, idleState);
        walkState.AddTransition(StateEnum.PlayerRun, runState);
        walkState.AddTransition(StateEnum.PlayerInvisible, invisibleState);
        
        runState.AddTransition(StateEnum.PlayerIdle, idleState);
        runState.AddTransition(StateEnum.PlayerWalk, walkState);
        runState.AddTransition(StateEnum.PlayerInvisible, invisibleState);
        
        invisibleState.AddTransition(StateEnum.PlayerIdle, idleState);
        
        fsm.SetInit(idleState);
    }

    private void Update()
    {
        spacePressedThisFrame = false;
        
        fsm.Update();
        
        if (Input.GetKeyDown(KeyCode.Space) && fsm.GetCurrentState() != invisibleState && !spacePressedThisFrame)
        {
            spacePressedThisFrame = true;
            fsm.Transition(StateEnum.PlayerInvisible);
        }
        
        if (Input.GetKeyDown(KeyCode.E) && fsm.GetCurrentState() != invisibleState)
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
        if (currentState == invisibleState) return ((PlayerInvisibleState)currentState).GetInvisibleSpeed();

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
        if (currentState == invisibleState) return "Invisible";

        return "Unknown";
    }

    public bool IsDetectable()
    {
        State currentState = fsm.GetCurrentState();
        if (currentState == invisibleState)
        {
            return ((PlayerInvisibleState)currentState).IsDetectable();
        }
        return true;
    }

    public bool CanProcessSpaceInput()
    {
        if (spacePressedThisFrame)
        {
            return false;
        }
        spacePressedThisFrame = true;
        return true;
    }
}
