using System.Data.Common;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelection : NetworkBehaviour
{
    public Button lockButton;
    public CanvasGroup selectionContainer;
    public TextMeshProUGUI timerText;
    void Start()
    {

    }

    void Update()
    {
        if (timerText.text.Equals("0"))
        {
            selectionContainer.interactable = false;
            selectionContainer.blocksRaycasts = false;
            selectionContainer.alpha = .5f;
        }
    }

}
