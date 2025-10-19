using System.Collections.Generic;
using System.Linq;

using K4AdotNet.BodyTracking;

using UnityEngine;
using UnityEngine.Splines;

using Assets.Extensions;

#nullable enable

namespace Assets.Main.Rendering
{
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
                new BezierKnot(childJoint.GetUnityPos()),
                new BezierKnot(parentJoint.GetUnityPos())
            };
            }
        }

        private static Dictionary<JointType, JointType> GetParentJointDictionary()
        {
            var parentJointDictionary = new Dictionary<JointType, JointType>();

            foreach (var jointType in JointTypes.All)
            {
                try
                {
                    var parentJointType = jointType.GetParent();
                    parentJointDictionary[jointType] = parentJointType;
                }
                catch { }
            }

            return parentJointDictionary;
        }
    }
}
