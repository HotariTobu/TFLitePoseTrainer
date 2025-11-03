// For some reason, the app freezes when quitting.
// It seems all the DLL files must be in the same directory; otherwise, the cleanup process fails.
// Everything else works fine, though.
// Therefore, copy all the DLL files before loading with this script.
// This issue should be due to Unity.
// I really don't get why Unity is so widely accepted globally.

using System.IO;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Events;

#nullable enable

namespace Assets.Start
{
    class K4ABTDllInitializer : MonoBehaviour
    {
        const string K4ABTDllDirectoryPath = @"C:\Program Files\Azure Kinect Body Tracking SDK\tools";

        [SerializeField] protected UnityEvent _onInitialized = default!;

        async void Start()
        {
#if UNITY_EDITOR
            return;
#endif

            var pluginsPath = Path.Combine(Application.dataPath, "Plugins", "x86_64");

            await Task.Run(() => CopyFilesWithoutOverwrite(K4ABTDllDirectoryPath, pluginsPath));

            _onInitialized.Invoke();
        }

        static void CopyFilesWithoutOverwrite(string sourceDirectory, string destinationDirectory)
        {
            var files = Directory.GetFiles(sourceDirectory);

            foreach (string sourceFilePath in files)
            {
                var filename = Path.GetFileName(sourceFilePath);
                var destinationFilePath = Path.Combine(destinationDirectory, filename);

                if (File.Exists(destinationFilePath))
                {
                    continue;
                }

                File.Copy(sourceFilePath, destinationFilePath);
            }
        }
    }
}
