using UnityEngine;
using UnityEngine.UI;

public class InvisibilityBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private PlayerController playerController;
    
    private PlayerInvisibleState invisibleState;
    private const float maxInvisibleTime = 10f;

    private void Start()
    {
        if (fillImage == null)
        {
            fillImage = GetComponent<Image>();
            if (fillImage == null)
            {
                Debug.LogError("No se encontró el componente Image en la barra de invisibilidad");
                return;
            }
        }

        if (playerController == null)
        {
            Debug.LogError("No se asignó el PlayerController en el Inspector");
            return;
        }
        
        invisibleState = playerController.GetInvisibleState();
        if (invisibleState == null)
        {
            Debug.LogError("No se pudo obtener el estado invisible del PlayerController");
            return;
        }
    }

    private void Update()
    {
        if (fillImage == null || invisibleState == null)
        {
            return;
        }
        
        float remainingTime = invisibleState.GetRemainingInvisibleTime();
        float fillAmount = remainingTime / maxInvisibleTime;
        fillAmount = Mathf.Clamp01(fillAmount);
        fillImage.fillAmount = fillAmount;
    }
}
