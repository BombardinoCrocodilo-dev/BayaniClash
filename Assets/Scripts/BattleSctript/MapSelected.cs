using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class MapSelected : NetworkBehaviour
{
    private GameData gameData => Data.GameData;
    [SerializeField] private GameObject[] mapObject;

    private int selectedIndex;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            int index = gameData.selectedMapIndex;
            if (index < 0 || index >= mapObject.Length)
            {
                Debug.LogError("Invalid selectedMapIndex on server!");
                return;
            }
            ActivateMap(index);
            ActivateMapClientRpc(index);
        }
    }

    private void ActivateMap(int index)
    {
        Debug.Log($"Activating map index: {index}");

        for (int i = 0; i < mapObject.Length; i++)
        {
            mapObject[i].SetActive(i == index);
        }
    }
    [ClientRpc]
    private void ActivateMapClientRpc(int index)
    {
        for (int i = 0; i < mapObject.Length; i++)
        {
            mapObject[i].SetActive(i == index);
        }
    }
}
