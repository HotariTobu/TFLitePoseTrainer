using UnityEngine;
using UnityEngine.SceneManagement;

public class UIEventHandler : MonoBehaviour
{
    static UIEventHandler()
    {
        K4AdotNetPatch.ApplyPatch().ContinueWith(task =>
        {
            if (task.Result.HasException)
            {
                Debug.LogError(task.Result.Exception);
            }
        });
    }

    public async void OnAzureButtonClick()
    {
        K4AdotNet.Sdk.Init(K4AdotNet.ComboMode.Azure);
        await SceneManager.LoadSceneAsync("Main");
    }

    public async void OnOrbbecButtonClick()
    {
        K4AdotNet.Sdk.Init(K4AdotNet.ComboMode.Orbbec);
        await SceneManager.LoadSceneAsync("Main");
    }
}
