using System.Numerics;

using K4AdotNet.BodyTracking;

namespace TFLitePoseTrainer.Extensions;

public static class JointExtension
{
    public static Vector3 GetPos(this Joint joint)
    {
        return new Vector3(joint.PositionMm.X, joint.PositionMm.Y, joint.PositionMm.Z);
    }
}
