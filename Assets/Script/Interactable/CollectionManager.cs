using UnityEngine;

public class CollectionManager : MonoBehaviour
{
    [SerializeField] private int captureCount = 0;
    [SerializeField] private TMPro.TextMeshProUGUI countText; // Referencia al texto UI para mostrar el contador

    // Singleton para acceso fácil
    public static CollectionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        UpdateCountText();
    }

    public void IncrementCount()
    {
        captureCount++;
        UpdateCountText();
        Debug.Log($"Contador de capturas: {captureCount}");
    }

    private void UpdateCountText()
    {
        if (countText != null)
        {
            countText.text = $"Capturados: {captureCount}";
        }
    }

    public int GetCaptureCount()
    {
        return captureCount;
    }
}
