using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Main
{
    public class QuitCanceler : MonoBehaviour
    {
        void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.wantsToQuit += OnWantsToQuit;
#else
        Application.wantsToQuit += OnWantsToQuit;
#endif
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.wantsToQuit -= OnWantsToQuit;
#else
        Application.wantsToQuit -= OnWantsToQuit;
#endif
        }

        bool OnWantsToQuit()
        {
            SceneManager.LoadScene("End");
            return false;
        }
    }
}
