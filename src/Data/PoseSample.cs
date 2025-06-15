using TFLitePoseTrainer.Interfaces;

namespace TFLitePoseTrainer.Data;

public record PoseSample : IPoseSample
{
    public IEnumerable<IPoseFrame> Frames { get; init; }

    public PoseSample(IEnumerable<IPoseFrame> frames)
    {
        var frameCount = frames.Count();
        if (frameCount != Constants.PoseFrameCount)
        {
            throw new ArgumentException($"Expected {Constants.PoseFrameCount} frames, but got {frameCount}.");
        }

        Frames = frames;
    }
}
