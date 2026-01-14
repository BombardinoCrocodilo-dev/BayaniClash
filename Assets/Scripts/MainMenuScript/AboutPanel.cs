using UnityEngine;

public class AboutPanel : MonoBehaviour
{
    [SerializeField] private GameObject aboutStorymodePanel;
    [SerializeField] private GameObject aboutPVEPanel;
    [SerializeField] private GameObject aboutPVPPanel;
    void Start()
    {
        aboutStorymodePanel.SetActive(false);
        aboutPVEPanel.SetActive(false);
        aboutPVPPanel.SetActive(false);
    }
    public void OnActiveStoryPanel()
    {
        aboutStorymodePanel.SetActive(true);
    }
    public void OnActivePVEPanel()
    {
        aboutPVEPanel.SetActive(true);
    }
    public void OnActivePVPPanel()
    {
        aboutPVPPanel.SetActive(true);
    }

}
