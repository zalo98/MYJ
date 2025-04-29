using UnityEngine;

public class PlayerWalkState : State
{
    private PlayerController playerController;
    private PlayerAnimationController animController;

    public PlayerWalkState(FSM fsm, PlayerController controller, PlayerAnimationController animController) : base(fsm)
    {
        this.playerController = controller;
        this.animController = animController;
    }

    public override void Awake()
    {
        Debug.Log("Entrando en estado Walk");
        animController.PlayWalkAnimation();
    }

    public override void Execute()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Verificar si el jugador está presionando teclas de movimiento
        if (Mathf.Abs(horizontalInput) < 0.1f && Mathf.Abs(verticalInput) < 0.1f)
        {
            playerController.SetMoveDirection(Vector3.zero);
            fsm.Transition(StateEnum.PlayerIdle);
            return;
        }

        // Verificar si el jugador está corriendo
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            fsm.Transition(StateEnum.PlayerRun);
            return;
        }

        // Calcular dirección de movimiento
        Vector3 movement = CalculateMovementDirection(horizontalInput, verticalInput);

        // Establecer la dirección de movimiento
        playerController.SetMoveDirection(movement);
    }

    private Vector3 CalculateMovementDirection(float horizontalInput, float verticalInput)
    {
        // Movimiento relativo al mundo (no a la cámara)
        Vector3 desiredDirection = new Vector3(horizontalInput, 0, verticalInput);

        return desiredDirection.normalized;
    }

    public override void Sleep()
    {
        Debug.Log("Saliendo del estado Walk");
    }
}
