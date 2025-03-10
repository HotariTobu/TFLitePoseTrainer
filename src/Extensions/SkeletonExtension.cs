using System.Numerics;

using K4AdotNet.BodyTracking;

namespace TFLitePoseTrainer.Extensions;

public static class SkeletonExtension
{
    public static IEnumerable<Vector3> GetNormalizedJointVectors(Skeleton bodySkelton)
    {
        var jointPositions = from joint in bodySkelton
                             select joint.GetPos();

        var jointPositionOrigin = bodySkelton.Pelvis.GetPos();
        var jointVectorFactorBase = bodySkelton.Neck.GetPos();
        var jointVectorFactor = 1 / (jointPositionOrigin - jointVectorFactorBase).Length();

        var jointNormalizedVectors = from jointPosition in jointPositions
                                     let jointVector = jointPosition - jointPositionOrigin
                                     select jointVector * jointVectorFactor;

        return jointNormalizedVectors;
    }
}
