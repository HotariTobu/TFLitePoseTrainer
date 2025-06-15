using K4AdotNet.BodyTracking;

namespace TFLitePoseTrainer.Record;

public class SkeletonItem(BodyId bodyId, Skeleton skeleton) : SharedWPF.ViewModelBase
{
    #region == BodyId ==

    public readonly BodyId BodyId = bodyId;

    #endregion
    #region == Skeleton ==

    private Skeleton _skeleton = skeleton;

    public Skeleton Skeleton
    {
        get => _skeleton;
        set
        {
            _skeleton = value;
            RaisePropertyChanged(nameof(Skeleton));
        }
    }

    #endregion
}
