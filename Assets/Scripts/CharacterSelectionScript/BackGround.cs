using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BackGround : NetworkBehaviour
{
    private GameData gameData => Data.GameData;

    [SerializeField] private Image image;
    [SerializeField] private Sprite[] bgImage;

    private int imageIndex;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            imageIndex = gameData.selectedMapIndex;

            // Safety check
            if (imageIndex < 0 || imageIndex >= bgImage.Length)
            {
                Debug.LogError("Invalid selectedMapIndex!");
                return;
            }

            image.sprite = bgImage[imageIndex];
            DisplayBgClientRpc(imageIndex);
        }
    }

    [ClientRpc]
    private void DisplayBgClientRpc(int index)
    {
        if (index < 0 || index >= bgImage.Length)
        {
            Debug.LogError("Client received invalid map index!");
            return;
        }

        image.sprite = bgImage[index];
    }
}
