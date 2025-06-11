using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUIManager : MonoBehaviour
{
    [SerializeField] private Button playButton;
    // Start is called before the first frame update
    void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(() =>
            {
                SceneLoader.Instance.LoadScene("GameScene");
            });
        }
    }
    // Update is called once per frame
    void Update()
    {
    }
}
