using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Main.Tracking
{
    [DefaultExecutionOrder(-100)]
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
}
