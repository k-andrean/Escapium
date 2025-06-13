using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
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
    [SerializeField] private Light2D globalLightObject;
    [SerializeField] private Light2D playerLightObject;

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
        if (interactionType == "itemInteraction")
        {
            if (currentInteractable == null) return;
            switch (currentInteractable.interactableObjectName)
            {
                case "cupboard":
                    {
                        var hasVaseKey = inventory.Find(item => (string)item["itemId"] == "vaseKey");
                        var hasTorch = inventory.Find(item => (string)item["itemId"] == "torch");
                        if (hasTorch != null)
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("The cupboard is already empty.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
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
                                globalLightObject.intensity = 0.07f;
                                dialogueLines.Clear();
                                dialogueLines.Enqueue("Wah, suddenly you heard a click sound and the room become dark!");
                                StartCoroutine(DialogueRoutine(() =>
                                {
                                    HideDialog();
                                }));
                            }));
                        }
                        else
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("The cupboard is locked. Maybe there's a key somewhere...");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
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
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                        }
                        else
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("There's a vase on the cupboard!");
                            dialogueLines.Enqueue("After you check inside of the vase, you found there is a key inside the vase!");
                            dialogueLines.Enqueue("You use the superman power to break the vase!");
                            dialogueLines.Enqueue("You found a small key inside the vase!");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                var newItem = new Dictionary<string, object>
                                {
                                    { "itemId", "vaseKey" },
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
                        var hasPhoneClue = inventory.Find(item => (string)item["itemId"] == "phoneClue");
                        var hasBookClue = inventory.Find(item => (string)item["itemId"] == "bookClue");
                        var hasBookKey = inventory.Find(item => (string)item["itemId"] == "bookKey");
                        if (hasBookKey != null)
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("You already have the book key.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                        }
                        else if (hasPhoneClue != null)
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("You found a book key hidden in the book!");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                var newItem = new Dictionary<string, object>
                                {
                                    { "itemId", "bookKey" },
                                    { "itemName", "Book Key" },
                                    { "isUsed", false },
                                };
                                inventory.Add(newItem);
                                DisplayInventory();
                                HideDialog();
                            }));
                        }
                        else if (hasBookClue != null)
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("You already have the book clue.");
                            dialogueLines.Enqueue("The clue wrote `Light reveals the truth beneath the fish tank.`");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                        }
                        else
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("There's a book on the table.");
                            dialogueLines.Enqueue("The book wrote `Light reveals the truth beneath the fish tank.`");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                var newItem = new Dictionary<string, object>
                                {
                                    { "itemId", "bookClue" },
                                    { "itemName", "Book Clue" },
                                    { "isUsed", false },
                                };
                                inventory.Add(newItem);
                                DisplayInventory();
                                HideDialog();
                            }));
                        }
                        break;
                    }
                case "fishTank":
                    {
                        var hasTorch = inventory.Find(item => (string)item["itemId"] == "torch");
                        var hasNumberClue = inventory.Find(item => (string)item["itemId"] == "fishTankNumberClue");
                        if (hasNumberClue != null)
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("The numbers are clearly visible: 781");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                        }
                        else if (hasTorch != null)
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("With the torch, you can see the numbers clearly: 781");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                var newItem = new Dictionary<string, object>
                                {
                                    { "itemId", "fishTankNumberClue" },
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
                            dialogueLines.Enqueue("There is a fish in the fish tank.");
                            dialogueLines.Enqueue("There seems to be some numbers hidden here, but it's too dark to see them clearly.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                        }
                        break;
                    }
                case "phone":
                    {
                        var hasNumberClue = inventory.Find(item => (string)item["itemId"] == "fishTankNumberClue");
                        if (hasNumberClue != null)
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("You input the number you found previously.");
                            dialogueLines.Enqueue("The phone response `Lift the book again and knock twice.`");
                            StartCoroutine(DialogueRoutine(() => {
                                var newItem = new Dictionary<string, object>
                                {
                                    { "itemId", "phoneClue" },
                                    { "itemName", "Phone Clue" },
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
                            dialogueLines.Enqueue("The phone seems to be asking for a number...");
                            StartCoroutine(DialogueRoutine(() => {
                                HideDialog();
                            }));
                        }
                        break;
                    }
                case "door":
                    {
                        var hasBookKey = inventory.Find(item => (string)item["itemId"] == "bookKey");
                        if (hasBookKey != null)
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("You try to insert the key you found before.");
                            dialogueLines.Enqueue("You hear a click sound and the door is now unlocked!");
                            dialogueLines.Enqueue("Congratulations! You've opened the door and escaped!");
                            StartCoroutine(DialogueRoutine(() => {
                                HideDialog();
                            }));
                        }
                        else
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("The door is locked. You need a key to open it.");
                            StartCoroutine(DialogueRoutine(() => {
                                HideDialog();
                            }));
                        }
                        break;
                    }
                case "portraitPicture":
                    {
                        if (globalLightObject.intensity == 0.07f)
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("There is a portrait picture.");
                            dialogueLines.Enqueue("Suddenly you found a hidden switch.");
                            dialogueLines.Enqueue("You click the switch and the light in the room turn on.");
                            StartCoroutine(DialogueRoutine(() => {
                                globalLightObject.intensity = 1f;
                                HideDialog();
                            }));
                        }
                        else
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("There is a portrait picture.");
                            StartCoroutine(DialogueRoutine(() => {
                                HideDialog();
                            }));
                        }
                        break;
                    }
                case "boxes":
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("There is a lot of box in here.");
                        StartCoroutine(DialogueRoutine(() => {
                            HideDialog();
                        }));
                        break;
                    }
                case "clock":
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("There is a clock on the wall.");
                        StartCoroutine(DialogueRoutine(() => {
                            HideDialog();
                        }));
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
        else if (interactionType == "inventoryItemInteraction")
        {
            if (selectedItem != null)
            {
                switch (selectedItem.itemId)
                {
                    case "torch":
                        {
                            ShowDialog();
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("You turn on the torch you have found before.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                playerLightObject.gameObject.SetActive(true);
                                HideDialog();
                            }));
                            break;
                        }
                    case "vaseKey":
                        {
                            ShowDialog();
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("This is a key you found from vase.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                            break;
                        }
                    case "bookKey":
                        {
                            ShowDialog();
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("This is a key you found from book.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                            break;
                        }
                    case "bookClue":
                        {
                            ShowDialog();
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("This is clue you note from the book.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                            break;
                        }
                    case "fishTankNumberClue":
                        {
                            ShowDialog();
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("This is clue you note from the fish tank.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                            break;
                        }
                    case "phoneClue":
                        {
                            ShowDialog();
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("This is clue you note from the phone.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
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
