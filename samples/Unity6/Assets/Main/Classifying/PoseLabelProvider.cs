using System.Collections.Generic;
using System.IO;
using System.Linq;

using K4AdotNet.BodyTracking;

using UnityEngine;
using UnityEngine.Events;

using Assets.Extensions;

#nullable enable

namespace Assets.Main.Classifying
{
    public class PoseLabelProvider : MonoBehaviour
    {
        private const string PoseLabelFilenameTemplate = "{0}.pose.txt";

        [SerializeField] protected UnityEvent<Dictionary<BodyId, string?>> _onPoseLabelDictionaryUpdated = default!;

        private PoseClassifier? _classifier;
        private IReadOnlyList<string>? _poseLabelList;

        void OnDestroy()
        {
            _classifier?.Dispose();
            _classifier = null;
            _poseLabelList = null;

            _onPoseLabelDictionaryUpdated.Invoke(new());
        }

        public void UpdateClassifier(string modelFilePath)
        {
            _classifier?.Dispose();
            _classifier = new PoseClassifier(modelFilePath);
            _poseLabelList = LoadPoseLabelList(modelFilePath);

            _onPoseLabelDictionaryUpdated.Invoke(new());
        }

        public void UpdatePoseLabelDictionary(BodyFrame bodyFrame)
        {
            if (_classifier is null)
            {
                return;
            }

            var poseLabelDictionary = new Dictionary<BodyId, string?>();

            foreach (var bodyIndex in Enumerable.Range(0, bodyFrame.BodyCount))
            {
                var bodyId = bodyFrame.GetBodyId(bodyIndex);

                bodyFrame.GetBodySkeleton(bodyIndex, out var skeleton);
                var jointNormalizedVectors = skeleton.GetNormalizedJointVectors();

                var poseIndex = _classifier.Classify(jointNormalizedVectors);
                var poseLabel = GetPoseLabel(poseIndex);

                poseLabelDictionary.Add(bodyId, poseLabel);
            }

            _onPoseLabelDictionaryUpdated.Invoke(poseLabelDictionary);
        }

        private string? GetPoseLabel(int? poseIndex)
        {
            if (!poseIndex.HasValue)
            {
                return null;
            }

            if (_poseLabelList is null || poseIndex.Value < 0 || _poseLabelList.Count <= poseIndex.Value)
            {
                return poseIndex.Value.ToString();
            }

            return _poseLabelList[poseIndex.Value];
        }

        private static string[] LoadPoseLabelList(string modelFilePath)
        {
            var modelDirectory = Path.GetDirectoryName(modelFilePath);
            var modelStem = Path.GetFileNameWithoutExtension(modelFilePath);
            var poseFilenameBasePath = Path.Combine(modelDirectory, modelStem);
            var poseFilename = string.Format(PoseLabelFilenameTemplate, poseFilenameBasePath);
            if (File.Exists(poseFilename))
            {
                return File.ReadAllLines(poseFilename);
            }
            else
            {
                return new string[0];
            }
        }
    }
}
