using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterDescriptionViewer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Character Info")]
    [TextArea(1, 3)]
    [SerializeField] private string[] characterNames;
    [SerializeField] private GameObject[] characterPrefabs;


    [TextArea(10, 30)]
    [SerializeField] private string[] characterDescriptions;

    private int currentIndex = 0;

    private void Start()
    {
        ShowCharacter(currentIndex);
    }

    private void ShowCharacter(int index)
    {
        for (int i = 0; i < characterPrefabs.Length; i++)
        {
            characterPrefabs[i].SetActive(i == index);
        }
        if (index >= 0 && index < characterNames.Length)
        {
            nameText.text = characterNames[index];
            descriptionText.text = characterDescriptions[index];
        }
    }

    public void OnNextButton()
    {
        AudioManager.Instance.PlayClick();
        if (currentIndex < characterNames.Length - 1)
        {
            currentIndex++;
        }
        else
        {
            currentIndex = 0;
        }
        ShowCharacter(currentIndex);
    }

    public void OnBackButton()
    {
        AudioManager.Instance.PlayClick();
        Debug.Log("Left button");
        if (currentIndex > 0)
        {
            currentIndex--;
        }
        else
        {
            currentIndex = 9;
        }
        ShowCharacter(currentIndex);
    }
}