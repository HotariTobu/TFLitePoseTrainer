namespace TFLitePoseTrainer.Data;

public record PoseData(
    string Id,
    DateTime CreatedAt
)
{
    public PoseData(string id) : this(id, DateTime.Now) { }
}
