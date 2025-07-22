using System.Numerics;

namespace TFLitePoseTrainer.Interfaces;

interface IPoseFrame
{
    IEnumerable<Vector3> JointVectors { get; }
}
