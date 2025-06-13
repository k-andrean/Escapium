using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] public string interactableObjectName;
    [SerializeField] private string basePromptMessage = "To Interact";
    public string PromptMessage
    {
        get
        {
            if (Application.isMobilePlatform)
                return $"Tap Interact Button {basePromptMessage}";
            else
                return $"Press E {basePromptMessage}";
        }
    }
    private bool playerInRange = false;
    // Start is called before the first frame update
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
        if (Application.isMobilePlatform) return;
        if (playerInRange && IsInteractPressed())
        {
            Interact();
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SetPlayerInRange(true);
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SetPlayerInRange(false);
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            SetPlayerInRange(true);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            SetPlayerInRange(false);
        }
    }
    void SetPlayerInRange(bool value)
    {
        playerInRange = value;
        if (Application.isMobilePlatform || Application.isEditor)
        {
            if (value)
                GameUIManager.Instance.ShowMobileInteract(this);
            else
                GameUIManager.Instance.HideMobileInteract();
        }
        if (value)
        {
            Debug.Log($"Player Near {gameObject.name} - {PromptMessage}");
        }
    }
    bool IsInteractPressed()
    {
        if (Input.GetKeyDown(KeyCode.E))
            return true;
        return false;
    }
    public virtual void Interact()
    {
        Debug.Log($"Interacted With {gameObject.name}");
        /*
        GameUIManager.Instance.InteractFlow();
        */
        GameUIManager.Instance.InteractFlow("itemInteraction");
        GameUIManager.Instance.playerMovement.SetMovementPaused(true);
    }
}
