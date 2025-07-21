namespace TFLitePoseTrainer.Interfaces;

public interface IPoseSample
{
    public IEnumerable<IPoseFrame> Frames { get; }
}
