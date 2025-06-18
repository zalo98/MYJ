using System;
using UnityEngine;

public class Key : MonoBehaviour, IInteractable
{
    private PlayerController playerController;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    public void Interact()
    {
        playerController.GetKey();
        Destroy(gameObject);
    }
}