using System.IO;
using System.Windows.Media.Imaging;

using Google.Protobuf;

using TFLitePoseTrainer.Interfaces;

namespace TFLitePoseTrainer.Data;

public class PoseData
{
    private static readonly string RootPath = Path.GetFullPath(@"pose-data");
    private static string DirectoryPathFormat(string id) => Path.Join(RootPath, id);
    private static string ThumbnailPathFormat(string directoryPath) => Path.Join(directoryPath, "thumbnail.png");
    private static string LabelPathFormat(string directoryPath) => Path.Join(directoryPath, "label.txt");
    private static string DataPathFormat(string directoryPath) => Path.Join(directoryPath, "data");

    static PoseData()
    {
        Directory.CreateDirectory(RootPath);
    }

    public readonly string Id;
    public readonly DateTime CreatedAt;

    private readonly string _directoryPath;
    private readonly string _thumbnailPath;
    private readonly string _labelPath;

    public string DataPath { get; }
    public string? Label { get; private set; }

    private PoseData() : this(Guid.NewGuid().ToString(), DateTime.Now) { }

    private PoseData(string id, DateTime createdAt)
    {
        Id = id;
        CreatedAt = createdAt;

        _directoryPath = DirectoryPathFormat(id);
        _thumbnailPath = ThumbnailPathFormat(_directoryPath);
        _labelPath = LabelPathFormat(_directoryPath);

        DataPath = DataPathFormat(_directoryPath);
    }

    public BitmapSource GetThumbnailSource() => new BitmapImage(new(_thumbnailPath));

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

    public static PoseData? Create(BitmapSource thumbnailSource, IPoseSample sample)
    {
        var poseData = new PoseData();

        if (Directory.Exists(poseData._directoryPath))
        {
            Console.Error.WriteLine($"Failed creating PoseData: Exist id: {poseData.Id}");
            return null;
        }

        try
        {
            Directory.CreateDirectory(poseData._directoryPath);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed creating PoseData: Failed creating directory: {e}");
            return null;
        }

        try
        {
            using var thumbnailStream = new FileStream(poseData._thumbnailPath, FileMode.Create);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(thumbnailSource));
            encoder.Save(thumbnailStream);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed creating PoseData: Failed writing thumbnail: {e}");
            return null;
        }

        try
        {
            var poseSampleMessage = ToMessage(sample);
            using var output = File.Create(poseData.DataPath);
            poseSampleMessage.WriteTo(output);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed creating PoseData: Failed writing data: {e}");
            return null;
        }

        return poseData;
    }

    public static IEnumerable<PoseData>? List()
    {
        try
        {
            var poseDataList = new List<PoseData>();

            foreach (var directoryPath in Directory.EnumerateDirectories(RootPath))
            {
                var id = Path.GetFileName(directoryPath);
                if (id is null)
                {
                    continue;
                }

                var createdAt = Directory.GetCreationTime(directoryPath);
                var poseData = new PoseData(id, createdAt);

                if (!File.Exists(poseData._thumbnailPath))
                {
                    continue;
                }

                if (File.Exists(poseData._labelPath))
                {
                    poseData.Label = File.ReadAllText(poseData._labelPath);
                }

                poseDataList.Add(poseData);
            }

            poseDataList.Sort((a, b) => a.CreatedAt.CompareTo(b.CreatedAt));
            return poseDataList;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed listing PoseData: {e}");
            return null;
        }
    }

    private static Messages.PoseSample ToMessage(IPoseSample sample)
    {
        var poseData = new Messages.PoseSample();

        foreach (var frame in sample.Frames)
        {
            var poseFrame = new Messages.PoseFrame();

            foreach (var jointVector in frame.JointVectors)
            {
                poseFrame.JointVectors.Add(new Messages.Vector3
                {
                    X = jointVector.X,
                    Y = jointVector.Y,
                    Z = jointVector.Z
                });
            }

            poseData.Frames.Add(poseFrame);
        }

        return poseData;
    }
}
