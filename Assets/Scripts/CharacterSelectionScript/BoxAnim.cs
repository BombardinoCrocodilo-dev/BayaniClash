using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Outline))]
[RequireComponent (typeof(NetworkObject))]
public class BoxAnim : NetworkBehaviour, IPointerClickHandler
{
    public Outline outline;
    void Start()
    {
        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }
        outline.enabled = false;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.PlayClick();
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsListening)
        {
            StartCoroutine(FlashOutline(Color.green));
            return;
        }
        bool clickedByClient = !IsServer;
        ClickBoxServerRpc(clickedByClient);
        Debug.Log(eventData.pointerId);
        
    }
    [ServerRpc(RequireOwnership = false)]
    private void ClickBoxServerRpc(bool clickedByClient)
    {
        ShowOutlineClientRpc(clickedByClient);
    }
    [ClientRpc]
    private void ShowOutlineClientRpc(bool clickedByClient)
    {
        Color flashColor = clickedByClient ? Color.red : Color.green;
        StartCoroutine(FlashOutline(flashColor));
    }
    private IEnumerator FlashOutline(Color color)
    {
        outline.enabled = !outline.enabled;
        outline.effectColor = color;
        outline.effectDistance = new Vector2(15, 15);
        yield return new WaitForSeconds(0.5f);
        outline.enabled = !outline.enabled;
    }

}
