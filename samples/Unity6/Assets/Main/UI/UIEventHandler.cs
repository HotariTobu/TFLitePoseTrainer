using UnityEngine;
using UnityEngine.Events;

#nullable enable

namespace Assets.Main.UI
{
    public class UIEventHandler : MonoBehaviour
    {
        [SerializeField] protected UnityEvent<string> _onModelFilePathSelected = default!;

        private FileOpenDialog? _fileOpenDialog;

        void Awake()
        {
            _fileOpenDialog = new FileOpenDialog();
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
