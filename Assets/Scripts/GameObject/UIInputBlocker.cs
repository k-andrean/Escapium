using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIInputBlocker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public static bool IsUIBeingPressed = false;
    // Start is called before the first frame update
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        GameUIManager.Instance.SetUIBlockState(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GameUIManager.Instance.SetUIBlockState(false);
    }
}
