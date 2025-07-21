using System.IO;

namespace TFLitePoseTrainer.Data;

public class ModelData
{
    private static readonly string RootPath = Path.GetFullPath(@"model-data");
    private static string DirectoryPathFormat(string id) => Path.Join(RootPath, id);
    private static string LabelPathFormat(string directoryPath) => Path.Join(directoryPath, "label.txt");
    private static string DataPathFormat(string directoryPath) => Path.Join(directoryPath, "data");
    private static string PoseLabelsPathFormat(string directoryPath) => Path.Join(directoryPath, "pose-labels.txt");

    static ModelData()
    {
        Directory.CreateDirectory(RootPath);
    }

    public readonly string Id;
    public readonly DateTime CreatedAt;

    private readonly string _directoryPath;
    private readonly string _labelPath;
    private readonly string _poseLabelsPath;

    public string DataPath { get; }
    public string? Label { get; private set; }
    public IReadOnlyList<string> PoseLabels { get; private set; } = [];

    private ModelData() : this(Guid.NewGuid().ToString(), DateTime.Now) { }

    private ModelData(string id, DateTime createdAt)
    {
        Id = id;
        CreatedAt = createdAt;

        _directoryPath = DirectoryPathFormat(id);
        _labelPath = LabelPathFormat(_directoryPath);
        _poseLabelsPath = PoseLabelsPathFormat(_directoryPath);

        DataPath = DataPathFormat(_directoryPath);
    }

    public bool UpdateLabel(string label)
    {
        try
        {
            File.WriteAllText(_labelPath, label);
            Label = label;
            return true;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed updating label: {e}");
            return false;
        }
    }

    public Exception? Delete()
    {
        try
        {
            Directory.Delete(_directoryPath, true);
            return null;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public static (ModelData?, Exception?) Create(IEnumerable<string> poseLabels)
    {
        var modelData = new ModelData()
        {
            PoseLabels = [.. poseLabels],
        };

        if (Directory.Exists(modelData._directoryPath))
        {
            return (null, new Exception($"Exist id: {modelData.Id}"));
        }

        try
        {
            Directory.CreateDirectory(modelData._directoryPath);
        }
        catch (Exception e)
        {
            return (null, new Exception($"Failed creating directory", e));
        }

        try
        {
            File.WriteAllLines(modelData._poseLabelsPath, poseLabels);
        }
        catch (Exception e)
        {
            return (null, new Exception($"Failed storing pose labels", e));
        }

        return (modelData, null);
    }

    public static IEnumerable<ModelData>? List()
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
            Console.Error.WriteLine($"Failed listing ModelData: {e}");
            return null;
        }
    }
}
