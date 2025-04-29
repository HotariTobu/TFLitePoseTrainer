using System.IO;

namespace TFLitePoseTrainer.Data;

public class ModelData
{
    private static readonly string RootPath = Path.GetFullPath(@"model-data");
    private static readonly string LabelPathFormat = Path.Join("{0}", "label.txt");
    private static readonly string DataPathFormat = Path.Join("{0}", "data");


    static ModelData()
    {
        Directory.CreateDirectory(RootPath);
    }

    public readonly string Id;
    public readonly DateTime CreatedAt;

    private readonly string _labelPath;
    private readonly string _dataPath;

    public string? Label { get; private set; }

    private ModelData() : this(Guid.NewGuid().ToString(), DateTime.Now) { }

    private ModelData(string id, DateTime createdAt)
    {
        Id = id;
        CreatedAt = createdAt;

        _labelPath = Path.Join(RootPath, string.Format(LabelPathFormat, id));
        _dataPath = Path.Join(RootPath, string.Format(DataPathFormat, id));
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

    public static ModelData? Create()
    {
        var modelData = new ModelData();
        var basePath = Path.Join(RootPath, modelData.Id);

        if (Directory.Exists(basePath))
        {
            Console.Error.WriteLine($"Failed creating ModelData: Exist id: {modelData.Id}");
            return null;
        }

        try
        {
            Directory.CreateDirectory(basePath);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed creating ModelData: Failed creating directory: {e}");
            return null;
        }

        return modelData;
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
