namespace TFLitePoseTrainer.Extensions;

public static class TimerExtension
{
    public static bool Stop(this Timer timer)
    {
        return timer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    public static bool Start(this Timer timer, int interval)
    {
        return timer.Change(0, interval);
    }
}