using UnityEngine;

public class PanelCloser : MonoBehaviour
{
    [Header("Panel To Close")]
    public GameObject panelToClose;
    public void ClosePanel()
    {
        if (panelToClose != null && panelToClose.activeSelf)
        {
            panelToClose.SetActive(false);
        }
        AudioManager.Instance.PlayClick();
    }
}
