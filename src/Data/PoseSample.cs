using TFLitePoseTrainer.Interfaces;

namespace TFLitePoseTrainer.Data;

record PoseSample : IPoseSample
{
    public IEnumerable<IPoseFrame> Frames { get; init; }

    internal PoseSample(IEnumerable<IPoseFrame> frames)
    {
        var frameCount = frames.Count();
        if (frameCount != Constants.PoseFrameCount)
        {
            throw new ArgumentException($"Expected {Constants.PoseFrameCount} frames, but got {frameCount}.");
        }

        Frames = frames;
    }
}
