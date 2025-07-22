namespace TFLitePoseTrainer.Exceptions;

class FatalException(int exitCode, string message, Exception? innerException = null) : Exception(message, innerException)
{
    internal readonly int ExitCode = exitCode;
}
