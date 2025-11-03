using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Start
{
    class SceneLoader : MonoBehaviour
    {
        public void Load(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
