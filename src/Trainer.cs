using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace TFLitePoseTrainer;

public static partial class Trainer
{
    private static string? s_trainerPath;

    [GeneratedRegex(@"^Progress: (?<progress>\d*\.\d*)$")]
    private static partial Regex ProgressRegex();

    public static async Task<Exception?> Setup()
    {
        if (s_trainerPath != null)
        {
            return new Exception("Trainer already setup");
        }

        var trainerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "trainer");
        if (!Directory.Exists(trainerPath))
        {
            return new DirectoryNotFoundException($"Trainer directory not found at: {trainerPath}");
        }

        var processInfo = new ProcessStartInfo
        {
            FileName = "sh",
            Arguments = "setup.sh",
            WorkingDirectory = trainerPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = false
        };

        using var process = Process.Start(processInfo);
        if (process == null)
        {
            return new Exception("Failed to start setup process");
        }

        await process.WaitForExitAsync();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        Console.WriteLine(output);
        Console.Error.WriteLine(error);

        if (process.ExitCode != 0)
        {
            return new Exception($"Setup script failed with exit code {process.ExitCode}");
        }

        s_trainerPath = trainerPath;

        return null;
    }

    public static async Task<Exception?> Train(string outputPath, IEnumerable<string> poseDataPaths, Action<float> progressCallback)
    {
        Debug.Assert(poseDataPaths.Any(), "No samples provided for training");

        if (s_trainerPath == null)
        {
            return new Exception("Trainer not setup");
        }

        var processInfo = new ProcessStartInfo
        {
            FileName = "uv",
            Arguments = $"run main.py {outputPath} {string.Join(" ", poseDataPaths)}",
            WorkingDirectory = s_trainerPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = false
        };

        using var process = new Process
        {
            StartInfo = processInfo,
        };

        var progressRegex = ProgressRegex();

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            var line = e.Data;
            if (line == null)
            {
                return;
            }

            var match = progressRegex.Match(line);
            if (match.Success)
            {
                var progress = float.Parse(match.Groups["progress"].Value);
                progressCallback(progress);
            }
            else
            {
                outputBuilder.AppendLine(line);
            }
        };
        process.ErrorDataReceived += (_, e) => errorBuilder.AppendLine(e.Data);

        try
        {
            if (!process.Start())
            {
                return new Exception("Failed to start training process");
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            Console.WriteLine(outputBuilder.ToString());
            Console.Error.WriteLine(errorBuilder.ToString());

            if (process.ExitCode != 0)
            {
                return new Exception($"Training process failed with exit code {process.ExitCode}");
            }
        }
        catch (Exception e)
        {
            return new Exception($"Failed to train", e);
        }

        return null;
    }
}
