namespace TFLitePoseTrainer.Interfaces;

interface IPoseSample
{
    IEnumerable<IPoseFrame> Frames { get; }
}
