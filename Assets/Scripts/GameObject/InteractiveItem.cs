using UnityEngine;
using System.Collections.Generic;

public class InteractiveItem : InteractableObject
{
    [SerializeField] protected string lockedMessage = "This is locked.";
    [SerializeField] protected string defaultMessage = "Nothing interesting here.";
    [SerializeField] protected bool isLocked = false;
    [SerializeField] protected string requiredItemId;
    [SerializeField] protected string itemToGiveId;
    [SerializeField] protected string hintMessage;

    protected bool hasBeenInteracted = false;
    protected InventoryManager inventoryManager;

    protected virtual void Start()
    {
        inventoryManager = GameUIManager.Instance.GetComponent<InventoryManager>();
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager not found!");
        }
    }

    public override void Interact()
    {
        if (isLocked)
        {
            if (inventoryManager.HasItem(requiredItemId))
            {
                Unlock();
            }
            else
            {
                GameUIManager.Instance.ShowMessage(lockedMessage);
            }
        }
        else
        {
            HandleInteraction();
        }
    }

    protected virtual void Unlock()
    {
        isLocked = false;
        hasBeenInteracted = true;
        if (!string.IsNullOrEmpty(itemToGiveId))
        {
            inventoryManager.AddItem(itemToGiveId);
        }
    }

    protected virtual void HandleInteraction()
    {
        if (!string.IsNullOrEmpty(hintMessage))
        {
            GameUIManager.Instance.ShowMessage(hintMessage);
        }
        else
        {
            GameUIManager.Instance.ShowMessage(defaultMessage);
        }
    }
} 