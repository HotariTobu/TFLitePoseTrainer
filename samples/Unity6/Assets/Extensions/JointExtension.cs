namespace Assets.Extensions
{
    static class JointExtension
    {
        public static UnityEngine.Vector3 GetPos(this K4AdotNet.BodyTracking.Joint joint)
        {
            return new(joint.PositionMm.X, joint.PositionMm.Y, joint.PositionMm.Z);
        }
    }
}
