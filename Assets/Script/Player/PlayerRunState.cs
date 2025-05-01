using UnityEngine;

public class PlayerRunState : State
{
    private PlayerController playerController;
    private PlayerAnimationController animController;

    public PlayerRunState(FSM fsm, PlayerController controller, PlayerAnimationController animController) : base(fsm)
    {
        this.playerController = controller;
        this.animController = animController;
    }

    public override void Awake()
    {
        Debug.Log("Entrando en estado Run");
        animController.PlayRunAnimation();
    }

    public override void Execute()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        
        if (Mathf.Abs(horizontalInput) < 0.1f && Mathf.Abs(verticalInput) < 0.1f)
        {
            playerController.SetMoveDirection(Vector3.zero);
            fsm.Transition(StateEnum.PlayerIdle);
            return;
        }
        
        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
        {
            fsm.Transition(StateEnum.PlayerWalk);
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            fsm.Transition(StateEnum.PlayerInvisible);
            return;
        }
        
        Vector3 movement = CalculateCameraRelativeMovement(horizontalInput, verticalInput);
        
        playerController.SetMoveDirection(movement);
    }

    private Vector3 CalculateCameraRelativeMovement(float horizontalInput, float verticalInput)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return new Vector3(horizontalInput, 0, verticalInput).normalized;
        }
        
        Vector3 forward = mainCamera.transform.forward;
        Vector3 right = mainCamera.transform.right;
        
        forward.y = 0;
        right.y = 0;
        
        if (forward.magnitude > 0.1f) forward.Normalize();
        if (right.magnitude > 0.1f) right.Normalize();
        
        Vector3 desiredDirection = forward * verticalInput + right * horizontalInput;

        if (desiredDirection.magnitude > 0.1f)
        {
            desiredDirection.Normalize();
        }

        return desiredDirection;
    }

    public override void Sleep()
    {
        Debug.Log("Saliendo del estado Run");
    }
}
