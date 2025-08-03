using System.Diagnostics;
using System.IO;

using TFLitePoseTrainer.Data;

namespace TFLitePoseTrainer.Main;

partial class Window
{
    static readonly string ModelLabelFormat = "Pose {0}";
    static readonly Microsoft.Win32.SaveFileDialog ExportModelDialog = new()
    {
        Title = "Export Model Data",
        Filter = "Model Data Files|*.tflite",
        DefaultExt = ".tflite"
    };

    async void InitializeModelItems()
    {
        var result = await Task.Run(ModelData.List);
        if (!result.TryGetValue(out var modelDataList))
        {
            throw new("Failed loading model data", result.Exception);
        }

        var modelItems = _dataSource.ModelItems;

        foreach (var modelData in modelDataList)
        {
            var modelItem = new ModelItem(modelData);
            modelItems.Add(modelItem);
        }
    }

    async void AddModelItem(IReadOnlyCollection<PoseItem> poseItems)
    {
        Debug.Assert(poseItems.Count >= 2);

        var poseLabels = poseItems.Select(p => p.Label);

        var createModelDataResult = ModelData.Create(poseLabels);
        if (!createModelDataResult.TryGetValue(out var modelData))
        {
            throw new("Failed creating model data", createModelDataResult.Exception);
        }

        var initialLabel = GetInitialModelLabel(poseLabels);
        modelData.UpdateLabel(initialLabel);

        var trainingModelItem = new TrainingModelItem(modelData);
        _dataSource.TrainingModelItems.Add(trainingModelItem);

        var poseDataPaths = poseItems.Select(p => p.DataPath);
        var trainingResult = await Trainer.Train(modelData.DataPath, poseDataPaths,
         (progress) => trainingModelItem.ProgressValue = progress);

        _dataSource.TrainingModelItems.Remove(trainingModelItem);

        if (trainingResult.HasException)
        {
            var deleteModelDataResult = modelData.Delete();
            if (deleteModelDataResult.HasException)
            {
                throw new AggregateException("Failed training model, and deleting model", trainingResult.Exception, deleteModelDataResult.Exception);
            }
            else
            {
                throw new("Failed training model", trainingResult.Exception);
            }
        }

        var modelItem = new ModelItem(modelData);
        _dataSource.ModelItems.Add(modelItem);
    }

    Result RemoveModelItem(ModelItem modelItem)
    {
        _dataSource.ModelItems.Remove(modelItem);

        var result = modelItem.Delete();
        return result.HasException ? result : Result.Success;
    }

    static string GetInitialModelLabel(IEnumerable<string> selectedPoseLabels)
    {
        var poseLabelRegex = PoseLabelRegex();

        var poseNames = selectedPoseLabels.Select(label =>
        {
            var match = poseLabelRegex.Match(label);
            if (match.Success)
            {
                var poseIndex = int.Parse(match.Groups[1].Value);
                return poseIndex.ToString();
            }
            else
            {
                return label;
            }

        });

        return string.Format(ModelLabelFormat, string.Join(", ", poseNames));
    }

    static void ExportModel(ModelItem modelItem)
    {
        ExportModelDialog.FileName = modelItem.Label;

        if (ExportModelDialog.ShowDialog() != true)
        {
            return;
        }

        var result = modelItem.Export(ExportModelDialog.FileName);
        if (result.HasException)
        {
            throw new("Failed exporting model data", result.Exception);
        }
    }
}
