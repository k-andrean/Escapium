using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    public string itemId;
    public string itemName;
    private bool isSelected = false;
    private Image background;
    private Button button;
    void Awake()
    {
        background = GetComponent<Image>();
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnItemClick);
        }
        Deselect();
    }
    // Start is called before the first frame update
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
    }
    public void Setup(string id, string name, Sprite icon)
    {
        itemId = id;
        itemName = name;
        var iconImage = GetComponentInChildren<Image>();
        if (iconImage != null) iconImage.sprite = icon;
        isSelected = false;
        UpdateHighlight();
    }
    public void OnItemClick()
    {
        if (isSelected)
        {
            GameUIManager.Instance.InteractFlow("inventoryItemInteraction");
        }
        else
        {
            GameUIManager.Instance.DeselectAllItems();
            Select();
            GameUIManager.Instance.SetSelectedItem(this);
        }
    }
    public void Select()
    {
        isSelected = true;
        UpdateHighlight();
    }
    public void Deselect()
    {
        isSelected = false;
        UpdateHighlight();
    }
    private void UpdateHighlight()
    {
        if (background != null)
        {
            background.color = isSelected ? new Color(1f, 1f, 0.5f, 1f) : Color.white;
        }
    }
}
