using System.IO;

namespace TFLitePoseTrainer.Data;

class ModelData
{
    static readonly string RootPath = Path.GetFullPath(@"model-data");
    static string DirectoryPathFormat(string id) => Path.Join(RootPath, id);
    static string LabelPathFormat(string directoryPath) => Path.Join(directoryPath, "label.txt");
    static string DataPathFormat(string directoryPath) => Path.Join(directoryPath, "data");
    static string PoseLabelsPathFormat(string directoryPath) => Path.Join(directoryPath, "pose-labels.txt");

    static ModelData()
    {
        Directory.CreateDirectory(RootPath);
    }

    internal readonly string Id;
    internal readonly DateTime CreatedAt;

    readonly string _directoryPath;
    readonly string _labelPath;
    readonly string _poseLabelsPath;

    internal string DataPath { get; }
    internal string? Label { get; private set; }
    internal IReadOnlyList<string> PoseLabels { get; private set; } = [];

    ModelData() : this(Guid.NewGuid().ToString(), DateTime.Now) { }

    ModelData(string id, DateTime createdAt)
    {
        Id = id;
        CreatedAt = createdAt;

        _directoryPath = DirectoryPathFormat(id);
        _labelPath = LabelPathFormat(_directoryPath);
        _poseLabelsPath = PoseLabelsPathFormat(_directoryPath);

        DataPath = DataPathFormat(_directoryPath);
    }

    internal Result UpdateLabel(string label)
    {
        try
        {
            File.WriteAllText(_labelPath, label);
            Label = label;
            return Result.Success;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    internal Result Delete()
    {
        try
        {
            Directory.Delete(_directoryPath, true);
            return Result.Success;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    internal static Result<ModelData> Create(IEnumerable<string> poseLabels)
    {
        var modelData = new ModelData()
        {
            PoseLabels = [.. poseLabels],
        };

        if (Directory.Exists(modelData._directoryPath))
        {
            return new Exception($"Exist model directory with id: {modelData.Id}");
        }

        try
        {
            Directory.CreateDirectory(modelData._directoryPath);
        }
        catch (Exception e)
        {
            return new Exception($"Failed creating model directory", e);
        }

        try
        {
            File.WriteAllLines(modelData._poseLabelsPath, poseLabels);
        }
        catch (Exception e)
        {
            return new Exception($"Failed storing pose labels", e);
        }

        return modelData;
    }

    internal Result Export(string destinationPath)
    {
        if (!Directory.Exists(destinationPath))
        {
            return new Exception($"Destination directory not found: {destinationPath}");
        }

        if (Directory.EnumerateFileSystemEntries(destinationPath).Any())
        {
            return new Exception($"Destination directory is not empty: {destinationPath}");
        }

        try
        {
            File.Copy(DataPath, DataPathFormat(destinationPath));
            File.Copy(_poseLabelsPath, PoseLabelsPathFormat(destinationPath));
            return Result.Success;
        }
        catch (Exception e)
        {
            return new Exception($"Failed exporting model data", e);
        }
    }

    internal static Result<IEnumerable<ModelData>> List()
    {
        try
        {
            var modelDataList = new List<ModelData>();

            foreach (var directoryPath in Directory.EnumerateDirectories(RootPath))
            {
                var id = Path.GetFileName(directoryPath);
                if (id is null)
                {
                    continue;
                }

                var createdAt = Directory.GetCreationTime(directoryPath);
                var modelData = new ModelData(id, createdAt);

                if (File.Exists(modelData._labelPath))
                {
                    modelData.Label = File.ReadAllText(modelData._labelPath);
                }

                if (File.Exists(modelData._poseLabelsPath))
                {
                    modelData.PoseLabels = File.ReadAllLines(modelData._poseLabelsPath);
                }
                else
                {
                    continue;
                }

                modelDataList.Add(modelData);
            }

            modelDataList.Sort((a, b) => a.CreatedAt.CompareTo(b.CreatedAt));
            return modelDataList;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}
