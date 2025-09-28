using UnityEngine;
using UnityEngine.SceneManagement;

public class SdkChecker : MonoBehaviour
{
    void Awake()
    {
        try
        {
            var mode = K4AdotNet.Sdk.ComboMode;
            Debug.Log(mode);
        }
        catch
        {
            SceneManager.LoadScene("Start");
        }
    }
}
