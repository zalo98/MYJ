using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class InvisibilityBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;
    private PlayerController playerController;
    private PlayerInvisibleState invisibleState;
    private const float maxInvisibleTime = 10f;
    private bool isInvisible = false;
    public static InvisibilityBar Instance { get; private set; }

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
        FindPlayerAndUpdateState();
    }

    private void FindPlayerAndUpdateState()
    {
        playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            invisibleState = playerController.GetInvisibleState();
            UpdateTimeText(maxInvisibleTime);
        }
        else
        {
            Debug.LogWarning("No se encontró el PlayerController en la escena");
        }
    }

    private void Start()
    {
        if (timeText == null)
        {
            timeText = GetComponent<TextMeshProUGUI>();
            if (timeText == null)
            {
                Debug.LogError("No se encontró el componente TextMeshProUGUI en la barra de invisibilidad");
                return;
            }
        }

        FindPlayerAndUpdateState();
    }

    private void Update()
    {
        if (timeText == null || invisibleState == null)
        {
            return;
        }
        
        float remainingTime = invisibleState.GetRemainingInvisibleTime();
        bool currentInvisibleState = remainingTime > 0;
        
        if (currentInvisibleState != isInvisible || currentInvisibleState)
        {
            UpdateTimeText(remainingTime);
            isInvisible = currentInvisibleState;
        }
    }

    private void UpdateTimeText(float time)
    {
        if (time <= 0)
        {
            timeText.text = "remaining invisibility: 0.0s";
        }
        else
        {
            timeText.text = $"Tiempo restante: {time:F1}s";
        }
    }
}
