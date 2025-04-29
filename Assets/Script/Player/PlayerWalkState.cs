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

        // Verificar si el jugador est� presionando teclas de movimiento
        if (Mathf.Abs(horizontalInput) < 0.1f && Mathf.Abs(verticalInput) < 0.1f)
        {
            playerController.SetMoveDirection(Vector3.zero);
            fsm.Transition(StateEnum.PlayerIdle);
            return;
        }

        // Verificar si el jugador est� corriendo
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            fsm.Transition(StateEnum.PlayerRun);
            return;
        }

        // Calcular direcci�n de movimiento relativa a la c�mara
        Vector3 movement = CalculateCameraRelativeMovement(horizontalInput, verticalInput);

        // Establecer la direcci�n de movimiento
        playerController.SetMoveDirection(movement);
    }

    private Vector3 CalculateCameraRelativeMovement(float horizontalInput, float verticalInput)
    {
        // Obtener la c�mara para movimiento relativo a ella
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            // Si no hay c�mara, usar movimiento global
            return new Vector3(horizontalInput, 0, verticalInput).normalized;
        }

        // Obtener direcciones de la c�mara
        Vector3 forward = mainCamera.transform.forward;
        Vector3 right = mainCamera.transform.right;

        // Proyectar en plano horizontal para evitar movimiento vertical
        forward.y = 0;
        right.y = 0;

        // Normalizar si tienen magnitud
        if (forward.magnitude > 0.1f) forward.Normalize();
        if (right.magnitude > 0.1f) right.Normalize();

        // Calcular direcci�n relativa a la c�mara
        Vector3 desiredDirection = forward * verticalInput + right * horizontalInput;

        // Normalizar si tiene magnitud
        if (desiredDirection.magnitude > 0.1f)
            desiredDirection.Normalize();

        return desiredDirection;
    }

    public override void Sleep()
    {
        Debug.Log("Saliendo del estado Walk");
    }
}
