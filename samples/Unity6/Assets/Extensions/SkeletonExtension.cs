using System.Collections.Generic;
using System.Linq;

namespace Assets.Extensions
{
    public static class SkeletonExtension
    {
        static readonly K4AdotNet.BodyTracking.JointType OriginJointType = K4AdotNet.BodyTracking.JointType.Pelvis;
        static readonly K4AdotNet.BodyTracking.JointType FactorBaseJointType = K4AdotNet.BodyTracking.JointType.Neck;

        public static IEnumerable<System.Numerics.Vector3> GetNormalizedJointVectors(this in K4AdotNet.BodyTracking.Skeleton skelton)
        {
            var jointPositions = from joint in skelton
                                 select joint.GetPos();

            var jointPositionOrigin = skelton[OriginJointType].GetPos();
            var jointVectorFactorBase = skelton[FactorBaseJointType].GetPos();
            var jointVectorFactor = 1 / (jointPositionOrigin - jointVectorFactorBase).Length();

            var jointNormalizedVectors = from jointPosition in jointPositions
                                         let jointVector = jointPosition - jointPositionOrigin
                                         select jointVector * jointVectorFactor;

            return jointNormalizedVectors;
        }
    }
}
