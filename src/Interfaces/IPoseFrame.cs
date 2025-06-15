using System.Numerics;

namespace TFLitePoseTrainer.Interfaces;

public interface IPoseFrame
{
    public IEnumerable<Vector3> JointVectors { get; }
}
