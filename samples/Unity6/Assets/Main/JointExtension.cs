using UnityEngine;

static class JointExtension
{
    public static Vector3 GetPos(this K4AdotNet.BodyTracking.Joint joint)
    {
        return new(joint.PositionMm.X, joint.PositionMm.Y, joint.PositionMm.Z);
    }
}
