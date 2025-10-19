using UnityEngine;
using UnityEngine.Events;

#nullable enable

namespace Assets.Main.UI
{
    public class UIEventHandler : MonoBehaviour
    {
        private static readonly FileOpenDialog.FileType[] FileTypes = { new("Pose Model File", "*.tflite") };

        [SerializeField] protected UnityEvent<string> _onModelFilePathSelected = default!;

        private FileOpenDialog? _fileOpenDialog;

        void Awake()
        {
            _fileOpenDialog = new FileOpenDialog();
            _fileOpenDialog.SetFileTypes(FileTypes);
        }

        void OnDestroy()
        {
            _fileOpenDialog?.Dispose();
        }

        public void OnShowFileOpenDialogButtonClick()
        {
            var filePath = _fileOpenDialog?.Show();
            if (filePath is null)
            {
                return;
            }

            _onModelFilePathSelected.Invoke(filePath);
        }
    }
}
