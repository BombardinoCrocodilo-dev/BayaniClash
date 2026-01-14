using UnityEngine;

public class SceneLoaderMemoryCleaner : MonoBehaviour
{
    void Awake()
    {
        // Destroy any lingering SceneLoader objects
        foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (obj.name.Contains("SceneLoader"))
            {
                Destroy(obj);
                Debug.Log("Destroyed lingering SceneLoader: " + obj.name);
            }
        }
    }
}
