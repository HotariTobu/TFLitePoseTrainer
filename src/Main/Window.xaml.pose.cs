using TFLitePoseTrainer.Data;

namespace TFLitePoseTrainer.Main;

partial class Window
{
    async void InitializePoseItems()
    {
        var result = await Task.Run(PoseData.List);
        if (!result.TryGetValue(out var poseDataList))
        {
            throw new("Failed loading pose data", result.Exception);
        }

        var poseItems = _dataSource.PoseItems;

        foreach (var poseData in poseDataList)
        {
            var poseItem = new PoseItem(poseData);
            poseItems.Add(poseItem);
        }
    }

    void AddPoseItem(PoseData poseData)
    {
        var poseLabels = _dataSource.PoseItems.Select(p => p.Label);
        var poseItem = new PoseItem(poseData)
        {
            Label = GetInitialPoseLabel(poseLabels)
        };
        _dataSource.PoseItems.Add(poseItem);
    }

    Result RemovePoseItem(PoseItem poseItem)
    {
        _dataSource.PoseItems.Remove(poseItem);

        var result = poseItem.Delete();
        return result.HasException ? result : Result.Success;
    }

    static string GetInitialPoseLabel(IEnumerable<string> poseLabels)
    {
        var poseLabelRegex = PoseLabelRegex();

        var lastPoseLabel = poseLabels.LastOrDefault(poseLabelRegex.IsMatch);
        var lastPoseIndex = 0;

        if (lastPoseLabel is not null)
        {
            var match = poseLabelRegex.Match(lastPoseLabel);
            lastPoseIndex = int.Parse(match.Groups[1].Value);
        }

        return string.Format(PoseLabelFormat, lastPoseIndex + 1);
    }
}
