using K4AdotNet.BodyTracking;

static class JointTypeExtension
{
    public static JointType? GetParent(this JointType jointType)
    {
        try
        {
            return JointTypes.GetParent(jointType);
        }
        catch (System.ArgumentOutOfRangeException)
        {
            return null;
        }
    }
}
