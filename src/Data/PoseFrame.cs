using System.Numerics;

using TFLitePoseTrainer.Interfaces;

namespace TFLitePoseTrainer.Data;

public record PoseFrame : IPoseFrame
{
    public IEnumerable<Vector3> JointVectors { get; init; }

    public PoseFrame(IEnumerable<Vector3> jointVectors)
    {
        var jointCount = jointVectors.Count();
        if (jointCount != Constants.PoseJointCount)
        {
            throw new ArgumentException($"Expected {Constants.PoseJointCount} joint vectors, but got {jointCount}.");
        }

        JointVectors = jointVectors;
    }
}
