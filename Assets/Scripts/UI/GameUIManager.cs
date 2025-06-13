using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;
    [System.Serializable]
    private struct InventoryItemSprite
    {
        public string itemId;
        public Sprite sprite;
    }
    [Header("Main")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button dialogueForegroundClickAnywhere;
    [SerializeField] private float typingSpeed = 0.05f;

    [Header("Mobile UI")]
    [SerializeField] private Button interactButton;
    [SerializeField] private GameObject itemContainerPrefab;
    [SerializeField] private Transform inventoryPanel;
    [SerializeField] private InventoryItemSprite[] inventoryItemSprites;
    private List<Dictionary<string, object>> inventory = new List<Dictionary<string, object>>();
    private Dictionary<string, Sprite> spriteLookup;
    private InteractableObject currentInteractable;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool skipToNextLine = false;
    private string currentLine = "";
    private Queue<string> dialogueLines = new Queue<string>();
    public bool IsPointerOverUIElement { get; private set; } = false;
    public bool isMoving = false;
    private InventoryItem selectedItem = null;

    // Start is called before the first frame update
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
    }
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        if (interactButton != null)
        {
            interactButton.gameObject.SetActive(false);
            interactButton.onClick.AddListener(OnMobileInteractPressed);
        }
        if (dialogueForegroundClickAnywhere != null)
        {
            dialogueForegroundClickAnywhere.onClick.AddListener(OnDialogClick);
        }
        spriteLookup = new Dictionary<string, Sprite>();
        foreach (var item in inventoryItemSprites)
        {
            spriteLookup[item.itemId] = item.sprite;
        }
    }
    void DisplayInventory()
    {
        foreach (Transform child in inventoryPanel)
        {
            Destroy(child.gameObject);
        }
        foreach (var item in inventory)
        {
            string itemId = item["itemId"].ToString();
            string itemName = item["itemName"].ToString();
            bool isUsed = (bool)item["isUsed"];
            GameObject newItem = Instantiate(itemContainerPrefab, inventoryPanel);
            InventoryItem inventoryItem = newItem.GetComponent<InventoryItem>();
            if (inventoryItem != null && spriteLookup.ContainsKey(itemId))
            {
                inventoryItem.Setup(itemId, itemName, spriteLookup[itemId]);
            }
            Image image = newItem.GetComponentInChildren<Image>();
            if (image != null && spriteLookup.ContainsKey(itemId))
            {
                image.sprite = spriteLookup[itemId];
            }
        }
    }
    void ShowDialog()
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = "";
        IsPointerOverUIElement = true;
    }
    void HideDialog()
    {
        dialoguePanel.SetActive(false);
        IsPointerOverUIElement = false;
    }
    void OnMobileInteractPressed()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }
    public void ShowMobileInteract(InteractableObject interactable)
    {
        currentInteractable = interactable;
        interactButton.gameObject.SetActive(true);
    }
    public void HideMobileInteract()
    {
        currentInteractable = null;
        interactButton.gameObject.SetActive(false);
    }
    public void SetUIBlockState(bool state)
    {
        IsPointerOverUIElement = state;
    }
    public void DeselectAllItems()
    {
        if (selectedItem != null)
        {
            selectedItem.Deselect();
            selectedItem = null;
        }
    }
    public void SetSelectedItem(InventoryItem item)
    {
        selectedItem = item;
    }
    /*
    public void InteractFlow()
    */
    public void InteractFlow(string interactionType)
    {
        if (dialoguePanel.activeSelf)
        {
            OnDialogClick();
            return;
        }

        ShowDialog();
        switch (interactionType)
        {
            case "cupboard":
                {
                    var hasVaseKey = inventory.Find(item => (string)item["itemId"] == "vase_key");
                    var hasTorch = inventory.Find(item => (string)item["itemId"] == "torch");
                    
                    if (hasTorch != null)
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("You already have the torch.");
                    }
                    else if (hasVaseKey != null)
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("You unlocked the cupboard and found a torch!");
                        StartCoroutine(DialogueRoutine(() =>
                        {
                            var newItem = new Dictionary<string, object>
                            {
                                { "itemId", "torch" },
                                { "itemName", "Torch" },
                                { "isUsed", false },
                            };
                            inventory.Add(newItem);
                            DisplayInventory();
                            HideDialog();
                        }));
                    }
                    else
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("The cupboard is locked. Maybe there's a key somewhere...");
                        StartCoroutine(DialogueRoutine(() => HideDialog()));
                    }
                    break;
                }
            case "vase":
                {
                    var hasVaseKey = inventory.Find(item => (string)item["itemId"] == "vase_key");
                    if (hasVaseKey != null)
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("The vase is empty.");
                        StartCoroutine(DialogueRoutine(() => HideDialog()));
                    }
                    else
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("You found a small key inside the vase!");
                        StartCoroutine(DialogueRoutine(() =>
                        {
                            var newItem = new Dictionary<string, object>
                            {
                                { "itemId", "vase_key" },
                                { "itemName", "Vase Key" },
                                { "isUsed", false },
                            };
                            inventory.Add(newItem);
                            DisplayInventory();
                            HideDialog();
                        }));
                    }
                    break;
                }
            case "book":
                {
                    var hasNumberClue = inventory.Find(item => (string)item["itemId"] == "number_clue");
                    var hasDoorKey = inventory.Find(item => (string)item["itemId"] == "door_key");
                    
                    if (hasDoorKey != null)
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("You already have the door key.");
                        StartCoroutine(DialogueRoutine(() => HideDialog()));
                    }
                    else if (hasNumberClue != null)
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("You found a door key hidden in the book!");
                        StartCoroutine(DialogueRoutine(() =>
                        {
                            var newItem = new Dictionary<string, object>
                            {
                                { "itemId", "door_key" },
                                { "itemName", "Door Key" },
                                { "isUsed", false },
                            };
                            inventory.Add(newItem);
                            DisplayInventory();
                            HideDialog();
                        }));
                    }
                    else
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("Light reveals the truth beneath the fishtank.");
                        StartCoroutine(DialogueRoutine(() => HideDialog()));
                    }
                    break;
                }
            case "fishtank":
                {
                    var hasTorch = inventory.Find(item => (string)item["itemId"] == "torch");
                    var hasNumberClue = inventory.Find(item => (string)item["itemId"] == "number_clue");
                    
                    if (hasNumberClue != null)
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("The numbers are clearly visible: 781");
                        StartCoroutine(DialogueRoutine(() => HideDialog()));
                    }
                    else if (hasTorch != null)
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("With the torch, you can see the numbers clearly: 781");
                        StartCoroutine(DialogueRoutine(() =>
                        {
                            var newItem = new Dictionary<string, object>
                            {
                                { "itemId", "number_clue" },
                                { "itemName", "Number Clue" },
                                { "isUsed", false },
                            };
                            inventory.Add(newItem);
                            DisplayInventory();
                            HideDialog();
                        }));
                    }
                    else
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("There seems to be some numbers hidden here, but it's too dark to see them clearly.");
                        StartCoroutine(DialogueRoutine(() => HideDialog()));
                    }
                    break;
                }
            case "phone":
                {
                    var hasNumberClue = inventory.Find(item => (string)item["itemId"] == "number_clue");
                    if (hasNumberClue != null)
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("Lift the book again and knock twice.");
                        StartCoroutine(DialogueRoutine(() => HideDialog()));
                    }
                    else
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("The phone seems to be asking for a number...");
                        StartCoroutine(DialogueRoutine(() => HideDialog()));
                    }
                    break;
                }
            case "door":
                {
                    var hasDoorKey = inventory.Find(item => (string)item["itemId"] == "door_key");
                    if (hasDoorKey != null)
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("Congratulations! You've opened the door and escaped!");
                        StartCoroutine(DialogueRoutine(() => HideDialog()));
                    }
                    else
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("The door is locked. You need a key to open it.");
                        StartCoroutine(DialogueRoutine(() => HideDialog()));
                    }
                    break;
                }
            default:
                {
                    break;
                }
        }
    }
    private void OnDialogClick()
    {
        if (isTyping)
        {
            isTyping = false;
        }
        else
        {
            skipToNextLine = true;
        }
    }
    private IEnumerator DialogueRoutine(System.Action onComplete)
    {
        skipToNextLine = false;
        while (dialogueLines.Count > 0)
        {
            string line = dialogueLines.Dequeue();
            yield return StartCoroutine(TypeText(line));
            skipToNextLine = false;
            yield return new WaitUntil(() => skipToNextLine);
        }
        onComplete?.Invoke();
    }
    IEnumerator TypeText(string message)
    {
        isTyping = true;
        currentLine = message;
        dialogueText.text = "";
        for (int i = 0; i < message.Length; i++)
        {
            if (!isTyping)
            {
                dialogueText.text = message;
                yield break;
            }
            dialogueText.text += message[i];
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }
}
