using System.Runtime.Serialization;

namespace TFLitePoseTrainer.Main;

public class PoseItem : SharedWPF.ViewModelBase, ISerializable
{
    public required Data.PoseData Data { get; init; }

    #region == Label ==

    private string _label = "";
    public string Label
    {
        get => _label;
        set
        {
            if (_label != value)
            {
                _label = value;
                RaisePropertyChanged(nameof(Label));
            }
        }
    }

    #endregion

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        throw new NotImplementedException();
    }
}