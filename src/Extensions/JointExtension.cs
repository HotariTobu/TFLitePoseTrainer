using System.Numerics;

using K4AdotNet.BodyTracking;

namespace TFLitePoseTrainer.Extensions;

static class JointExtension
{
    internal static Vector3 GetPos(this in Joint joint)
    {
        return new Vector3(joint.PositionMm.X, joint.PositionMm.Y, joint.PositionMm.Z);
    }
}
