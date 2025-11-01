using UnityEngine;

namespace Assets.End
{
    public class Quiter : MonoBehaviour
    {
        [SerializeField] protected float _secondsToQuit = 5f;

        async Awaitable Start()
        {
            await Awaitable.WaitForSecondsAsync(_secondsToQuit);
            Quit();
        }

        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}
