using System.IO;
using System.Windows.Media.Imaging;

using Google.Protobuf;

using TFLitePoseTrainer.Interfaces;

namespace TFLitePoseTrainer.Data;

class PoseData
{
    static readonly string RootPath = Path.GetFullPath(@"pose-data");
    static string DirectoryPathFormat(string id) => Path.Join(RootPath, id);
    static string ThumbnailPathFormat(string directoryPath) => Path.Join(directoryPath, "thumbnail.png");
    static string LabelPathFormat(string directoryPath) => Path.Join(directoryPath, "label.txt");
    static string DataPathFormat(string directoryPath) => Path.Join(directoryPath, "data");

    static PoseData()
    {
        Directory.CreateDirectory(RootPath);
    }

    internal readonly string Id;
    internal readonly DateTime CreatedAt;

    readonly string _directoryPath;
    readonly string _thumbnailPath;
    readonly string _labelPath;

    internal string DataPath { get; }
    internal string? Label { get; private set; }

    PoseData() : this(Guid.NewGuid().ToString(), DateTime.Now) { }

    PoseData(string id, DateTime createdAt)
    {
        Id = id;
        CreatedAt = createdAt;

        _directoryPath = DirectoryPathFormat(id);
        _thumbnailPath = ThumbnailPathFormat(_directoryPath);
        _labelPath = LabelPathFormat(_directoryPath);

        DataPath = DataPathFormat(_directoryPath);
    }

    internal BitmapSource GetThumbnailSource()
    {
        var bitmapImage = new BitmapImage();

        bitmapImage.BeginInit();
        bitmapImage.UriSource = new(_thumbnailPath);
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();

        bitmapImage.Freeze();

        return bitmapImage;
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

    internal static Result<PoseData> Create(BitmapSource thumbnailSource, IPoseSample sample)
    {
        var poseData = new PoseData();

        if (Directory.Exists(poseData._directoryPath))
        {
            return new Exception($"Exist pose directory with id: {poseData.Id}");
        }

        try
        {
            Directory.CreateDirectory(poseData._directoryPath);
        }
        catch (Exception e)
        {
            return new Exception($"Failed creating pose directory", e);
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
            return new Exception($"Failed writing pose thumbnail", e);
        }

        try
        {
            var poseSampleMessage = ToMessage(sample);
            using var output = File.Create(poseData.DataPath);
            poseSampleMessage.WriteTo(output);
        }
        catch (Exception e)
        {
            return new Exception($"Failed writing pose data", e);
        }

        return poseData;
    }

    internal static Result<IEnumerable<PoseData>> List()
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
            return e;
        }
    }

    static Messages.PoseSample ToMessage(IPoseSample sample)
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
