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
    [SerializeField] private GameObject winPanel;
    [SerializeField] private Button playAgainWinPanelButton;
    [SerializeField] private Button mainMenuWinPanelButton;
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
        if (playAgainWinPanelButton != null)
        {
            playAgainWinPanelButton.onClick.AddListener(() =>
            {
                winPanel.SetActive(false);
                SceneLoader.Instance.ReloadCurrentScene();
            });
        }
        if (mainMenuWinPanelButton != null)
        {
            mainMenuWinPanelButton.onClick.AddListener(() =>
            {
                winPanel.SetActive(false);
                SceneLoader.Instance.LoadScene("MainMenuScene");
            });
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
                            dialogueLines.Enqueue("The cupboard is empty.");
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
                                dialogueLines.Enqueue("You heard a click sound and the room suddenly become dark!");
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
                            dialogueLines.Enqueue("You look inside the vase!");
                            dialogueLines.Enqueue("there is a things shaped like key hidden inside the vase!");
                            dialogueLines.Enqueue("You smash open the vase!");
                            dialogueLines.Enqueue("Found a small key with rounded tip inside the vase!");
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
                            dialogueLines.Enqueue("You inspect the book hidden compartment again.");
                            dialogueLines.Enqueue("nothing there.");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                        }
                        else if (hasPhoneClue != null)
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("You flipped the book and found a hidden compartment!");
                            dialogueLines.Enqueue("You pry open the compartment and found small book shaped key!");
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
                            dialogueLines.Enqueue("You check the clue content again.");
                            dialogueLines.Enqueue("`Light reveals the truth beneath the fish tank.`");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                        }
                        else
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("You inspect pile of books on the table.");
                            dialogueLines.Enqueue("You noticed there is an oddly shaped book hidden between the stack.");
                            dialogueLines.Enqueue("You opened the book and found the clue `Light reveals the truth beneath the fish tank.`");
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
                            dialogueLines.Enqueue("The numbers 781 are clearly visible");
                            StartCoroutine(DialogueRoutine(() =>
                            {
                                HideDialog();
                            }));
                        }
                        else if (hasTorch != null)
                        {
                            dialogueLines.Clear();
                             dialogueLines.Enqueue("You take out the torch you found earlier from cupboard");
                            dialogueLines.Enqueue("after shoning on the back of fishtank, you can see the numbers 781 clearly");
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
                            dialogueLines.Enqueue("Multi colored fish is swimming in the fish tank.");
                            dialogueLines.Enqueue("There seems to be some numbers hidden here, but it's too dark to see them clearly.");
                            dialogueLines.Enqueue("Maybe if you use any light source, the content can be seen.");
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
                            dialogueLines.Enqueue("You open the phone and dialed the hidden number from fishtank.");
                            dialogueLines.Enqueue("You waited a while, and after some time passed there is faint sound.");
                            dialogueLines.Enqueue("`Lift the book again and knock twice.`");
                            dialogueLines.Enqueue("The sound then faded away after spouting the sentence, you wrote down the clue in a paper.");
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
                            dialogueLines.Enqueue("`You open the phone and dialed random number.`");
                            dialogueLines.Enqueue("The phone is not responding and seems to be asking for specific number...");
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
                            dialogueLines.Enqueue("You try to insert unique book shaped key found earlier into the door keyhole.");
                            dialogueLines.Enqueue("You hear a click sound and the door is now unlocked!");
                            dialogueLines.Enqueue("Congratulations! You managed to escape the room!");
                            StartCoroutine(DialogueRoutine(() => {
                                HideDialog();
                                winPanel.SetActive(true);
                            }));
                        }
                        else
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("You try to open the door but it is not budging, the door is locked.");
                            dialogueLines.Enqueue("You inspect the oddly shaped keyhole and noticed the shape resemble book.");
                            dialogueLines.Enqueue("Maybe there is a key hidden somewhere in the room with this particular shape");
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
                            dialogueLines.Enqueue("`You take down the portrait picture from the wall.`");
                            dialogueLines.Enqueue("You inspect the back and to your surprise you found a hidden switch!.");
                            dialogueLines.Enqueue("You click the switch and the light in the room turn on.");
                            StartCoroutine(DialogueRoutine(() => {
                                globalLightObject.intensity = 0.5f;
                                HideDialog();
                            }));
                        }
                        else
                        {
                            dialogueLines.Clear();
                            dialogueLines.Enqueue("There is a portrait picture on the wall.");
                            dialogueLines.Enqueue("Inside of the display is a picture of woman with her dog.");
                            StartCoroutine(DialogueRoutine(() => {
                                HideDialog();
                            }));
                        }
                        break;
                    }
                case "boxes":
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("There is a lot of boxes stacked in here.");
                        dialogueLines.Enqueue("You wondered if there is someone moving here recently.");
                        StartCoroutine(DialogueRoutine(() => {
                            HideDialog();
                        }));
                        break;
                    }
                case "clock":
                    {
                        dialogueLines.Clear();
                        dialogueLines.Enqueue("A clock is hanging on the wall.");
                        dialogueLines.Enqueue("The clock is not functioning, the time stopped at 12.12.");
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
                            dialogueLines.Enqueue("You turn on the torch.");
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
                            dialogueLines.Enqueue("Odd book shaped key.");
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
                            dialogueLines.Enqueue("inside written: `Light reveals the truth beneath the fish tank.`.");
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
                            dialogueLines.Enqueue("Number clue from fish tank, where should i use this?.");
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
                            dialogueLines.Enqueue("Your handwritten note from phone clue.");
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
