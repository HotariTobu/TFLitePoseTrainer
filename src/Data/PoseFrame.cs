using System.Numerics;

using TFLitePoseTrainer.Interfaces;

namespace TFLitePoseTrainer.Data;

record PoseFrame : IPoseFrame
{
    public IEnumerable<Vector3> JointVectors { get; init; }

    internal PoseFrame(IEnumerable<Vector3> jointVectors)
    {
        var jointCount = jointVectors.Count();
        if (jointCount != Constants.PoseJointCount)
        {
            throw new ArgumentException($"Expected {Constants.PoseJointCount} joint vectors, but got {jointCount}.");
        }

        JointVectors = jointVectors;
    }
}
