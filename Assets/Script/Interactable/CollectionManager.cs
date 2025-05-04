using UnityEngine;
using UnityEngine.SceneManagement;

public class CollectionManager : MonoBehaviour
{
    [SerializeField] private int captureCount = 0;
    [SerializeField] private int victoryTarget = 5;
    [SerializeField] private TMPro.TextMeshProUGUI countText;
    public static CollectionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }

        ResetCount();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetCount();
    }

    public void IncrementCount()
    {
        captureCount++;
        UpdateCountText();
        Debug.Log($"Contador de capturas: {captureCount}");
        
        if (captureCount >= victoryTarget)
        {
            LoadVictoryScene();
        }
    }

    private void UpdateCountText()
    {
        if (countText != null)
        {
            countText.text = $"Capturados: {captureCount}/{victoryTarget}";
        }
    }

    public int GetCaptureCount()
    {
        return captureCount;
    }

    public void ResetCount()
    {
        captureCount = 0;
        UpdateCountText();
    }

    private void LoadVictoryScene()
    {
        SceneManager.LoadScene("WinScene");
    }
}