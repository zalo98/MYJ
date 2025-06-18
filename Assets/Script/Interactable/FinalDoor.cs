using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerController playerController;
    
    public void Interact()
    {
        if (playerController.hasKey == true)
        {
            SceneManager.LoadScene("WinScene");
        }
    }
}
