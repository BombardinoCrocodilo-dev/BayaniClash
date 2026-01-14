using System.Collections;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadingManager : NetworkBehaviour
{
    [Header("Loading Settings")]
    [SerializeField] private float waitBeforeLoad = 2f; // seconds to wait

    private NetworkVariable<FixedString64Bytes> targetScene =
        new NetworkVariable<FixedString64Bytes>(
            default(FixedString64Bytes),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private bool isLoading = false;

    private void Start()
    {
        // Subscribe to changes
        targetScene.OnValueChanged += OnTargetSceneChanged;

        if (IsHost)
        {
            if (!string.IsNullOrEmpty(TargetSceneName))
            {
                targetScene.Value = TargetSceneName;
                StartCoroutine(LoadSceneWithDelay());
            }
        }
        else
        {
            // Client: check if targetScene already has value
            CheckAndStartLoading();
        }
    }

    private void CheckAndStartLoading()
    {
        if (!isLoading && !string.IsNullOrEmpty(targetScene.Value.ToString()))
        {
            StartCoroutine(LoadSceneWithDelay());
        }
    }

    private void OnTargetSceneChanged(FixedString64Bytes oldValue, FixedString64Bytes newValue)
    {
        if (!string.IsNullOrEmpty(newValue.ToString()) && !isLoading)
        {
            StartCoroutine(LoadSceneWithDelay());
        }
    }

    public void SetTargetScene(string sceneName)
    {
        if (IsHost)
        {
            targetScene.Value = sceneName;
        }
        else
        {
            RequestSceneChangeServerRpc(sceneName);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSceneChangeServerRpc(string sceneName, ServerRpcParams rpcParams = default)
    {
        targetScene.Value = sceneName;
    }

    private IEnumerator LoadSceneWithDelay()
    {
        isLoading = true;

        // Wait before loading
        yield return new WaitForSeconds(waitBeforeLoad);

        isLoading = false;

        if (IsHost)
        {
            NetworkManager.SceneManager.LoadScene(targetScene.Value.ToString(), LoadSceneMode.Single);
        }
    }

    public static string TargetSceneName;
}
