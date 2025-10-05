using System.Collections.Generic;
using System.Linq;

using K4AdotNet.BodyTracking;

using UnityEngine;
using UnityEngine.Splines;

#nullable enable

public class SkeletonUpdater : MonoBehaviour
{
    private static readonly IReadOnlyDictionary<JointType, JointType> ParentJointDictionary = GetParentJointDictionary();

    private SplineContainer SkeletonContainer => GetComponent<SplineContainer>();

    void Awake()
    {
        Debug.Assert(SkeletonContainer is not null, "Spline Container must be attached");

        InitializeBoneSpineContainer();
    }

    private void InitializeBoneSpineContainer()
    {
        var boneSplines = from _ in ParentJointDictionary
                      select new Spline();

        SkeletonContainer.Splines = boneSplines.ToArray();
    }

    public void UpdateWith(Skeleton skeleton)
    {
        var boneSpines = SkeletonContainer.Splines;

        foreach (var (childJointType, parentJointType) in ParentJointDictionary)
        {
            var splineIndex = (int)childJointType;
            var boneSpline = boneSpines[splineIndex];

            var childJoint = skeleton[childJointType];
            var parentJoint = skeleton[parentJointType];

            boneSpline.Knots = new[] {
                new BezierKnot(childJoint.GetPos()),
                new BezierKnot(parentJoint.GetPos())
            };
        }
    }

    private static Dictionary<JointType, JointType> GetParentJointDictionary()
    {
        var parentJointDictionary = new Dictionary<JointType, JointType>();

        foreach (var jointType in JointTypes.All)
        {
            var parentJointType = jointType.GetParent();
            if (!parentJointType.HasValue)
            {
                continue;
            }

            parentJointDictionary[jointType] = parentJointType.Value;
        }

        return parentJointDictionary;
    }
}
