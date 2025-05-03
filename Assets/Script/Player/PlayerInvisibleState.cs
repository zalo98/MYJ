using UnityEngine;

public class PlayerInvisibleState : State
{
    private PlayerController playerController;
    private PlayerAnimationController animController;
    private bool isDetectable = false;
    private readonly float invisibleSpeed = 3f;
    private const float invisible_Alpha = 0.5f;
    private const float visible_Alpha = 1f;
    private const float maxInvisibleTime = 10f;
    private float currentInvisibleTime;

    public PlayerInvisibleState(FSM fsm, PlayerController controller, PlayerAnimationController animController) : base(fsm)
    {
        this.playerController = controller;
        this.animController = animController;
        currentInvisibleTime = maxInvisibleTime;
    }

    public override void Awake()
    {
        Debug.Log("Entrando en estado Invisible");
        animController.PlayInvisibleAnimation();
        isDetectable = false;
        
        foreach (Renderer renderer in playerController.playerRenderers)
        {
            if (renderer is SkinnedMeshRenderer skinnedRenderer)
            {
                Material[] materials = skinnedRenderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material material = materials[i];
                    
                    material.SetFloat("_Mode", 3);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                    
                    if (material.HasProperty("_Color"))
                    {
                        Color color = material.GetColor("_Color");
                        color.a = invisible_Alpha;
                        material.SetColor("_Color", color);
                    }
                    
                    if (material.HasProperty("_BaseColor"))
                    {
                        Color color = material.GetColor("_BaseColor");
                        color.a = invisible_Alpha;
                        material.SetColor("_BaseColor", color);
                    }
                    
                    Color mainColor = material.color;
                    mainColor.a = invisible_Alpha;
                    material.color = mainColor;
                    
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetFloat("_Surface", 1);
                    material.SetFloat("_Blend", 0);
                }
                skinnedRenderer.materials = materials;
            }
        }
    }

    public override void Execute()
    {
        // Actualizar el timer
        currentInvisibleTime -= Time.deltaTime;
        
        // Si el timer llega a 0, cambiar a IdleState
        if (currentInvisibleTime <= 0)
        {
            fsm.Transition(StateEnum.PlayerIdle);
            return;
        }

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        if (Mathf.Abs(horizontalInput) < 0.1f && Mathf.Abs(verticalInput) < 0.1f)
        {
            playerController.SetMoveDirection(Vector3.zero);
            animController.PlayIdleAnimation();
        }
        else
        {
            Vector3 movement = CalculateCameraRelativeMovement(horizontalInput, verticalInput);
            playerController.SetMoveDirection(movement);
            animController.PlayWalkAnimation();
        }
        
        if (Input.GetKeyDown(KeyCode.Space) && playerController.CanProcessSpaceInput())
        {
            fsm.Transition(StateEnum.PlayerIdle);
        }
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
        Debug.Log("Saliendo del estado Invisible");
        isDetectable = true;
        
        foreach (Renderer renderer in playerController.playerRenderers)
        {
            if (renderer is SkinnedMeshRenderer skinnedRenderer)
            {
                Material[] materials = skinnedRenderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material material = materials[i];
                    material.SetFloat("_Mode", 0);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = -1;
                    
                    if (material.HasProperty("_Color"))
                    {
                        Color color = material.GetColor("_Color");
                        color.a = visible_Alpha;
                        material.SetColor("_Color", color);
                    }
                    
                    if (material.HasProperty("_BaseColor"))
                    {
                        Color color = material.GetColor("_BaseColor");
                        color.a = visible_Alpha;
                        material.SetColor("_BaseColor", color);
                    }

                    Color mainColor = material.color;
                    mainColor.a = visible_Alpha;
                    material.color = mainColor;
                    
                    material.SetOverrideTag("RenderType", "Opaque");
                    material.SetFloat("_Surface", 0);
                    material.SetFloat("_Blend", 0);
                }
                skinnedRenderer.materials = materials;
            }
        }
    }

    public bool IsDetectable()
    {
        return isDetectable;
    }

    public float GetInvisibleSpeed()
    {
        return invisibleSpeed;
    }

    public bool CanEnterInvisibleState()
    {
        return currentInvisibleTime > 0;
    }

    public float GetRemainingInvisibleTime()
    {
        return currentInvisibleTime;
    }

    public void ResetInvisibleTime()
    {
        currentInvisibleTime = maxInvisibleTime;
    }
} 