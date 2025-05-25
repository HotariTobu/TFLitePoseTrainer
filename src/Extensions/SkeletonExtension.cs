using System.Numerics;

using K4AdotNet.BodyTracking;

namespace TFLitePoseTrainer.Extensions;

public static class SkeletonExtension
{
    private static readonly JointType OriginJointType = JointType.Pelvis;
    private static readonly JointType FactorBaseJointType = JointType.Neck;

    public static IEnumerable<Vector3> GetNormalizedJointVectors(this in Skeleton skelton)
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
