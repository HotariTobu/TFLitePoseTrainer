using UnityEngine;
using UnityEngine.Events;

public class FilenameExtractor : MonoBehaviour
{
    [SerializeField] protected UnityEvent<string> _onFilenameExtracted = default!;

    public void Dispatch(string path)
    {
        var filename = System.IO.Path.GetFileName(path);
        _onFilenameExtracted.Invoke(filename);
    }
}
