using System.Collections.Generic;
using System.Linq;

using K4AdotNet.BodyTracking;

using UnityEngine;

#nullable enable

public class BodyVisualizer : MonoBehaviour
{
    [SerializeField] protected GameObject _bodyPrefab = default!;

    private readonly Dictionary<BodyId, GameObject> _bodyObjectDictionary = new();

    void Awake()
    {
        Debug.Assert(_bodyPrefab is not null, "Body Prefab must be set");
    }

    public void UpdateWith(BodyFrame bodyFrame)
    {
        var removedBodyIdSet = new HashSet<BodyId>(_bodyObjectDictionary.Keys);

        foreach (var bodyIndex in Enumerable.Range(0, bodyFrame.BodyCount))
        {
            var bodyId = bodyFrame.GetBodyId(bodyIndex);
            removedBodyIdSet.Remove(bodyId);

            bodyFrame.GetBodySkeleton(bodyIndex, out var skeleton);

            if (!_bodyObjectDictionary.TryGetValue(bodyId, out var bodyObject))
            {
                bodyObject = Instantiate(_bodyPrefab, this.transform);
                bodyObject.AddComponent<SkeletonUpdater>();
                _bodyObjectDictionary.Add(bodyId, bodyObject);
            }

            var skeletonUpdater = bodyObject.GetComponent<SkeletonUpdater>();
            skeletonUpdater.UpdateWith(skeleton);
        }

        foreach (var removedBodyId in removedBodyIdSet)
        {
            Destroy(_bodyObjectDictionary[removedBodyId]);
            _bodyObjectDictionary.Remove(removedBodyId);
        }
    }
}
