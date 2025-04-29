using UnityEngine;

public class PlayerIdleState : State
{
    private PlayerController playerController;
    private PlayerAnimationController animController;

    public PlayerIdleState(FSM fsm, PlayerController controller, PlayerAnimationController animController) : base(fsm)
    {
        this.playerController = controller;
        this.animController = animController;
    }

    public override void Awake()
    {
        Debug.Log("Entrando en estado Idle");
        animController.PlayIdleAnimation();

        // Aseguramos que no haya movimiento
        playerController.SetMoveDirection(Vector3.zero);
    }

    public override void Execute()
    {
        // Verificar si el jugador está intentando moverse
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        if (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f)
        {
            // Si está presionando Shift, transicionar a correr, sino a caminar
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                fsm.Transition(StateEnum.PlayerRun);
            }
            else
            {
                fsm.Transition(StateEnum.PlayerWalk);
            }
        }
    }

    public override void Sleep()
    {
        Debug.Log("Saliendo del estado Idle");
    }
}
