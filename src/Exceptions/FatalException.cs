namespace TFLitePoseTrainer.Exceptions;

class FatalException(int exitCode, string message, Exception? innerException = null) : Exception(message, innerException)
{
    public readonly int ExitCode = exitCode;
}
