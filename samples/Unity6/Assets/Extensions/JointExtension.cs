namespace Assets.Extensions
{
    public static class JointExtension
    {
        public static System.Numerics.Vector3 GetPos(this K4AdotNet.BodyTracking.Joint joint)
        {
            return new(joint.PositionMm.X, joint.PositionMm.Y, joint.PositionMm.Z);
        }

        public static UnityEngine.Vector3 GetUnityPos(this K4AdotNet.BodyTracking.Joint joint)
        {
            return new(joint.PositionMm.X, joint.PositionMm.Y, joint.PositionMm.Z);
        }
    }
}
