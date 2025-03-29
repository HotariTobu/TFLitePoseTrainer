namespace TFLitePoseTrainer.Data;

public record PoseData(
    string Id,
    DateTime CreatedAt
)
{
    public PoseData() : this(Guid.NewGuid().ToString(), DateTime.Now) { }
}
