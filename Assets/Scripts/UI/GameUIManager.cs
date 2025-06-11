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
        /*
        if (currentInteractable == null) return;
        if (isMoving) return;
        if (interactButton.gameObject.activeSelf && dialoguePanel.activeSelf)
        */
        if (dialoguePanel.activeSelf)
        {
            OnDialogClick();
            return;
        }
        /*
        switch (currentInteractable.interactableObjectName)
        {
            case "paperHint":
                {
                    ShowDialog();
                    var itemPaperHintGuide = inventory.Find(item => (string)item["itemId"] == "paperHintGuide");
                    if (itemPaperHintGuide != null)
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("You have read the hint and the hint is on the wall picture.");
                        StartCoroutine(DialogueRoutine(() =>
                        {
                            HideDialog();
                        }));
                    }
                    else
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("There is a paper in the floor and there is some writing on it.");
                        dialogueLines.Enqueue("\"You are in the room and you have to escape from here.\"");
                        dialogueLines.Enqueue("Find the key to escape from here.");
                        dialogueLines.Enqueue("The hint is on the wall picture.");
                        StartCoroutine(DialogueRoutine(() =>
                        {
                            var newItem = new Dictionary<string, object>
                            {
                                { "itemId", "paperHintGuide" },
                                { "itemName", "Paper Hint Guide" },
                                { "isUsed", false },
                            };
                            inventory.Add(newItem);
                            DisplayInventory();
                            HideDialog();
                        }));
                    }
                    break;
                }
            case "wallPicture":
                {
                    ShowDialog();
                    var itemPaperHintGuide = inventory.Find(item => (string)item["itemId"] == "paperHintGuide");
                    var itemWallPictureKey = inventory.Find(item => (string)item["itemId"] == "wallPictureKey");
                    if (itemWallPictureKey != null)
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("You already have the key from wall picture.");
                        StartCoroutine(DialogueRoutine(() =>
                        {
                            HideDialog();
                        }));
                    }
                    else if (itemPaperHintGuide != null)
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("From the hint that check on the picture wall.");
                        dialogueLines.Enqueue("But after looking at the picture, you realize that there is gap between the wall and the picture.");
                        dialogueLines.Enqueue("After check on the back of the picture. You found a key.");
                        dialogueLines.Enqueue("Is it the key to escape from here?");
                        StartCoroutine(DialogueRoutine(() =>
                        {
                            var newItem = new Dictionary<string, object>
                            {
                                { "itemId", "wallPictureKey" },
                                { "itemName", "Wall Picture Key" },
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
                        dialogueLines.Enqueue("It is a picture wall and have a beautiful women art.");
                        StartCoroutine(DialogueRoutine(() =>
                        {
                            HideDialog();
                        }));
                    }
                    break;
                }
            case "cupBoard":
                {
                    ShowDialog();
                    var itemWallPictureKey = inventory.Find(item => (string)item["itemId"] == "wallPictureKey");
                    var itemCupBoardPaper = inventory.Find(item => (string)item["itemId"] == "cupBoardPaper");
                    if (itemCupBoardPaper != null)
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("You already have the paper from cupboard.");
                        StartCoroutine(DialogueRoutine(() =>
                        {
                            HideDialog();
                        }));
                    }
                    else if (itemWallPictureKey != null)
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("It is a cupboard, but it is locked.");
                        dialogueLines.Enqueue("Maybe I can use the key that I have found before.");
                        dialogueLines.Enqueue("\"Click\"");
                        dialogueLines.Enqueue("Wah the cupboard is opened. Let's check it.");
                        dialogueLines.Enqueue("You found two paper in the cupboard.");
                        StartCoroutine(DialogueRoutine(() =>
                        {
                            var newItem = new Dictionary<string, object>
                            {
                                { "itemId", "cupBoardPaper" },
                                { "itemName", "Cupboard Papers" },
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
                        dialogueLines.Enqueue("It is a cupboard, but it is locked.");
                        StartCoroutine(DialogueRoutine(() =>
                        {
                            HideDialog();
                        }));
                    }
                    break;
                }
            case "tableWithSquarePanel":
                {
                    ShowDialog();
                    var itemCupBoardPaper = inventory.Find(item => (string)item["itemId"] == "cupBoardPaper");
                    var itemTablePuzzleKey = inventory.Find(item => (string)item["itemId"] == "tablePuzzleKey");
                    if (itemTablePuzzleKey != null)
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("It is just a paper with unknown words.");
                        StartCoroutine(DialogueRoutine(() =>
                        {
                            HideDialog();
                        }));
                    }
                    else if (itemCupBoardPaper != null)
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("It is a table, and there is a piece of paper at top of the table.");
                        dialogueLines.Enqueue("After check, it still need another piece of paper to able to read the content.");
                        dialogueLines.Enqueue("\"Placing the papers\"");
                        dialogueLines.Enqueue("\"The table shaking\"");
                        dialogueLines.Enqueue("Wah the table shaking.");
                        dialogueLines.Enqueue("Wait the bottom part of the table is opened.");
                        dialogueLines.Enqueue("I found the key, hopefully it is the key to open the door.");
                        StartCoroutine(DialogueRoutine(() =>
                        {
                            var newItem = new Dictionary<string, object>
                            {
                                { "itemId", "tablePuzzleKey" },
                                { "itemName", "Table Puzzle Key" },
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
                        dialogueLines.Enqueue("It is a table, and there is a piece of paper at top of the table.");
                        dialogueLines.Enqueue("After check, it still need another piece of paper to able to read the content.");
                        StartCoroutine(DialogueRoutine(() =>
                        {
                            HideDialog();
                        }));
                    }
                    break;
                }
            case "door":
                {
                    ShowDialog();
                    var itemWallPictureKey = inventory.Find(item => (string)item["itemId"] == "wallPictureKey");
                    var itemTablePuzzleKey = inventory.Find(item => (string)item["itemId"] == "tablePuzzleKey");
                    if (itemWallPictureKey != null && itemTablePuzzleKey != null)
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("It is door and looking from the key hole is the field.");
                        dialogueLines.Enqueue("It seems locked. I have find way to escape from here.");
                        dialogueLines.Enqueue("I have found keys before. Hopefully one of the key can escape from here.");
                        dialogueLines.Enqueue("\"Put key to the key hole\"");
                        dialogueLines.Enqueue("\"Click\"");
                        dialogueLines.Enqueue("Wah the door is opened.");
                        dialogueLines.Enqueue("Congratulations. You have won the game.");
                        StartCoroutine(DialogueRoutine(() =>
                        {
                            HideDialog();
                            SceneLoader.Instance.LoadScene("MainMenuScene");
                        }));
                    }
                    else if (itemWallPictureKey != null)
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("It is door and looking from the key hole is the field.");
                        dialogueLines.Enqueue("It seems locked. I have find way to escape from here.");
                        dialogueLines.Enqueue("I have found a key before. Hopefully one of the key can escape from here.");
                        dialogueLines.Enqueue("\"Put key to the key hole\"");
                        dialogueLines.Enqueue("No the key can not turn. Seems it is not the key for the door.");
                        dialogueLines.Enqueue("I have to continue explore.");
                        StartCoroutine(DialogueRoutine(() =>
                        {
                            HideDialog();
                        }));
                    }
                    else
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("It is door and looking from the key hole is the field.");
                        dialogueLines.Enqueue("It seems locked. I have find way to escape from here.");
                        StartCoroutine(DialogueRoutine(() =>
                        {
                            HideDialog();
                        }));
                    }
                    break;
                }
            default:
                {
                    break;
                }
        }
        */
        if (interactionType == "itemInteraction")
        {
            if (currentInteractable == null) return;
            switch (currentInteractable.interactableObjectName)
            {
                case "paperHint":
                    {
                        ShowDialog();
                        var itemPaperHintGuide = inventory.Find(item => (string)item["itemId"] == "paperHintGuide");
                        if (itemPaperHintGuide != null)
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("You have read the hint and the hint is on the wall picture.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                        }
                        else
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("There is a paper in the floor and there is some writing on it.");
                            dialogueLines.Enqueue("\"You are in the room and you have to escape from here.\"");
                            dialogueLines.Enqueue("Find the key to escape from here.");
                            dialogueLines.Enqueue("The hint is on the wall picture.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                var newItem = new Dictionary<string, object>
                                {
                                    { "itemId", "paperHintGuide" },
                                    { "itemName", "Paper Hint Guide" },
                                    { "isUsed", false },
                                };
                                inventory.Add(newItem);
                                DisplayInventory();
                                HideDialog();
                            }));
                        }
                        break;
                    }
                case "wallPicture":
                    {
                        ShowDialog();
                        var itemPaperHintGuide = inventory.Find(item => (string)item["itemId"] == "paperHintGuide");
                        var itemWallPictureKey = inventory.Find(item => (string)item["itemId"] == "wallPictureKey");
                        if (itemWallPictureKey != null)
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("You already have the key from wall picture.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                        }
                        else if (itemPaperHintGuide != null)
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("From the hint that check on the picture wall.");
                            dialogueLines.Enqueue("But after looking at the picture, you realize that there is gap between the wall and the picture.");
                            dialogueLines.Enqueue("After check on the back of the picture. You found a key.");
                            dialogueLines.Enqueue("Is it the key to escape from here?");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                var newItem = new Dictionary<string, object>
                                {
                                    { "itemId", "wallPictureKey" },
                                    { "itemName", "Wall Picture Key" },
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
                            dialogueLines.Enqueue("It is a picture wall and have a beautiful women art.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                        }
                        break;
                    }
                case "cupBoard":
                    {
                        ShowDialog();
                        var itemWallPictureKey = inventory.Find(item => (string)item["itemId"] == "wallPictureKey");
                        var itemCupBoardPaper = inventory.Find(item => (string)item["itemId"] == "cupBoardPaper");
                        if (itemCupBoardPaper != null)
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("You already have the paper from cupboard.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                        }
                        else if (itemWallPictureKey != null)
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("It is a cupboard, but it is locked.");
                            dialogueLines.Enqueue("Maybe I can use the key that I have found before.");
                            dialogueLines.Enqueue("\"Click\"");
                            dialogueLines.Enqueue("Wah the cupboard is opened. Let's check it.");
                            dialogueLines.Enqueue("You found two paper in the cupboard.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                var newItem = new Dictionary<string, object>
                                {
                                    { "itemId", "cupBoardPaper" },
                                    { "itemName", "Cupboard Papers" },
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
                            dialogueLines.Enqueue("It is a cupboard, but it is locked.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                        }
                        break;
                    }
                case "tableWithSquarePanel":
                    {
                        ShowDialog();
                        var itemCupBoardPaper = inventory.Find(item => (string)item["itemId"] == "cupBoardPaper");
                        var itemTablePuzzleKey = inventory.Find(item => (string)item["itemId"] == "tablePuzzleKey");
                        if (itemTablePuzzleKey != null)
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("It is just a paper with unknown words.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                        }
                        else if (itemCupBoardPaper != null)
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("It is a table, and there is a piece of paper at top of the table.");
                            dialogueLines.Enqueue("After check, it still need another piece of paper to able to read the content.");
                            dialogueLines.Enqueue("\"Placing the papers\"");
                            dialogueLines.Enqueue("\"The table shaking\"");
                            dialogueLines.Enqueue("Wah the table shaking.");
                            dialogueLines.Enqueue("Wait the bottom part of the table is opened.");
                            dialogueLines.Enqueue("I found the key, hopefully it is the key to open the door.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                var newItem = new Dictionary<string, object>
                                {
                                    { "itemId", "tablePuzzleKey" },
                                    { "itemName", "Table Puzzle Key" },
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
                            dialogueLines.Enqueue("It is a table, and there is a piece of paper at top of the table.");
                            dialogueLines.Enqueue("After check, it still need another piece of paper to able to read the content.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                        }
                        break;
                    }
                case "door":
                    {
                        ShowDialog();
                        var itemWallPictureKey = inventory.Find(item => (string)item["itemId"] == "wallPictureKey");
                        var itemTablePuzzleKey = inventory.Find(item => (string)item["itemId"] == "tablePuzzleKey");
                        if (itemWallPictureKey != null && itemTablePuzzleKey != null)
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("It is door and looking from the key hole is the field.");
                            dialogueLines.Enqueue("It seems locked. I have find way to escape from here.");
                            dialogueLines.Enqueue("I have found keys before. Hopefully one of the key can escape from here.");
                            dialogueLines.Enqueue("\"Put key to the key hole\"");
                            dialogueLines.Enqueue("\"Click\"");
                            dialogueLines.Enqueue("Wah the door is opened.");
                            dialogueLines.Enqueue("Congratulations. You have won the game.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                                SceneLoader.Instance.LoadScene("MainMenuScene");
                            }));
                        }
                        else if (itemWallPictureKey != null)
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("It is door and looking from the key hole is the field.");
                            dialogueLines.Enqueue("It seems locked. I have find way to escape from here.");
                            dialogueLines.Enqueue("I have found a key before. Hopefully one of the key can escape from here.");
                            dialogueLines.Enqueue("\"Put key to the key hole\"");
                            dialogueLines.Enqueue("No the key can not turn. Seems it is not the key for the door.");
                            dialogueLines.Enqueue("I have to continue explore.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                        }
                        else
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("It is door and looking from the key hole is the field.");
                            dialogueLines.Enqueue("It seems locked. I have find way to escape from here.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                        }
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
                    case "paperHintGuide":
                        {
                            ShowDialog();
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("You have found a paper with text on it before.");
                            dialogueLines.Enqueue("\"You are in the room and you have to escape from here.\"");
                            dialogueLines.Enqueue("Find the key to escape from here.");
                            dialogueLines.Enqueue("The hint is on the wall picture.");
                            Debug.Log($"is selected123: {selectedItem.itemId}, itemId: {selectedItem.itemId}, itemName: {selectedItem.itemName}");
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
