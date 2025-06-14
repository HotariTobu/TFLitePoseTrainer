using System.IO;
using System.Numerics;
using System.Windows.Media.Imaging;

using TFLitePoseTrainer.Interfaces;

namespace TFLitePoseTrainer.Data;

public class PoseData
{
    private static readonly string RootPath = Path.GetFullPath(@"pose-data");
    private static readonly string ThumbnailPathFormat = Path.Join("{0}", "thumbnail.png");
    private static readonly string LabelPathFormat = Path.Join("{0}", "label.txt");
    private static readonly string DataPathFormat = Path.Join("{0}", "data");

    static PoseData()
    {
        Directory.CreateDirectory(RootPath);
    }

    public readonly string Id;
    public readonly DateTime CreatedAt;

    private readonly string _thumbnailPath;
    private readonly string _labelPath;
    private readonly string _dataPath;
    private IPoseSample? _sample;

    public string? Label { get; private set; }

    private PoseData() : this(Guid.NewGuid().ToString(), DateTime.Now) { }

    private PoseData(string id, DateTime createdAt)
    {
        Id = id;
        CreatedAt = createdAt;

        _thumbnailPath = Path.Join(RootPath, string.Format(ThumbnailPathFormat, id));
        _labelPath = Path.Join(RootPath, string.Format(LabelPathFormat, id));
        _dataPath = Path.Join(RootPath, string.Format(DataPathFormat, id));
    }

    public BitmapSource GetThumbnailSource() => new BitmapImage(new(_thumbnailPath));

    public IPoseSample? Sample
    {
        get
        {
            if (_sample is null)
            {
                try
                {
                    using var input = File.OpenRead(_dataPath);
                    var poseSampleMessage = Messages.PoseSample.Parser.ParseFrom(input);
                    _sample = FromMessage(poseSampleMessage);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Failed reading pose data: {e}");
                }
            }

            return _sample;
        }
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

    public static PoseData? Create(BitmapSource thumbnailSource, IPoseSample sample)
    {
        var poseSampleMessage = ToMessage(sample);

        var poseData = new PoseData()
        {
            _sample = sample
        };
        var basePath = Path.Join(RootPath, poseData.Id);

        if (Directory.Exists(basePath))
        {
            Console.Error.WriteLine($"Failed creating PoseData: Exist id: {poseData.Id}");
            return null;
        }

        try
        {
            Directory.CreateDirectory(basePath);
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
            using var output = File.Create(poseData._dataPath);
            poseSampleMessage.WriteTo(new(output));
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

    private static PoseSample? FromMessage(Messages.PoseSample poseSampleMessage)
    {
        try
        {
            return new PoseSample(
                from frameMessage in poseSampleMessage.Frames
                select new PoseFrame(
                    from jointVector in frameMessage.JointVectors
                    select new Vector3(jointVector.X, jointVector.Y, jointVector.Z)
                )
            );
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed parsing pose frames: {e}");
            return null;
        }
    }
}
