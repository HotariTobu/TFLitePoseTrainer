using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

public static class K4AdotNetPatch
{
    private static readonly string ActualRuntimeRelativePath = Path.Combine("..", "..", "runtimes", "win-x64", "native");
    private static readonly string[] NativeLibSubdirs = new[] { K4AdotNet.Sdk.Azure.NATIVE_LIB_SUBDIR, K4AdotNet.Sdk.Orbbec.NATIVE_LIB_SUBDIR };

    private static bool s_isPatchApplying = false;
    private static bool s_isPatchApplied = false;

    public static async Task<Result> ApplyPatch()
    {
        if (s_isPatchApplying)
        {
            return new Exception("Patch is already applying");
        }

        if (s_isPatchApplied)
        {
            return new Exception("Patch is already applied");
        }

        s_isPatchApplying = true;

        var patchTasks = from nativeLibSubdir in NativeLibSubdirs
                         select PatchGetFullPathToSubdir(nativeLibSubdir);

        var patchResults = await Task.WhenAll(patchTasks);

        s_isPatchApplied = true;
        s_isPatchApplying = false;

        var failedPatchResults = patchResults.Where(result => result.HasException);
        if (failedPatchResults.Any())
        {
            var innerException = new AggregateException(failedPatchResults.Select(result => result.Exception));
            return new Exception("Failed to apply K4AdotNet patch", innerException);
        }

        return Result.Success;
    }

    private static async Task<Result> PatchGetFullPathToSubdir(string nativeLibSubdir)
    {
        var helperPathResult = GetHelperPath();
        if (!helperPathResult.TryGetValue(out var helperPath))
        {
            return new Exception("Failed to get K4AdotNet helper path", helperPathResult.Exception);
        }

        var expectedRuntimePath = Path.Combine(helperPath, nativeLibSubdir);
        var actualRuntimePath = Path.Combine(helperPath, ActualRuntimeRelativePath, nativeLibSubdir);

        return await CreateSymbolicLink(expectedRuntimePath, actualRuntimePath);
    }

    private static Result<string> GetHelperPath()
    {
        try
        {
            var k4aAssembly = typeof(K4AdotNet.Sdk).Assembly;
            var helpersType = k4aAssembly.GetType("K4AdotNet.Helpers");
            var getFullPathToSubdirMethod = helpersType.GetMethod("GetFullPathToSubdir", BindingFlags.Public | BindingFlags.Static) ?? throw new Exception("Not found: GetFullPathToSubdir");

            var helperPath = getFullPathToSubdirMethod.Invoke(null, new[] { "" });
            return helperPath is string helperPathString
                ? helperPathString
                : new Exception($"GetFullPathToSubdir returned not string: {helperPath}");
        }
        catch (Exception e)
        {
            return e;
        }
    }

    private static async Task<Result> CreateSymbolicLink(string target, string source)
    {
        var cleanupResult = CleanupSymbolicLink(target);
        if (cleanupResult.HasException)
        {
            return cleanupResult.Exception;
        }

        if (!Directory.Exists(source))
        {
            return new Exception($"Not found: {source}");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/C mklink /D \"{target}\" \"{source}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        try
        {
            using var process = new Process()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            var tcs = new TaskCompletionSource<int>();
            process.Exited += (sender, args) => tcs.SetResult(process.ExitCode);

            process.Start();
            var exitCode = await tcs.Task;

            if (exitCode == 0)
            {
                return Result.Success;
            }

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            return new Exception($"mklink failed: ExitCode: {exitCode}\nOutput: {output}\nError: {error}");
        }
        catch (Exception e)
        {
            return e;
        }
    }

    private static Result CleanupSymbolicLink(string target)
    {
        try
        {
            if (File.Exists(target))
            {
                File.Delete(target);
            }

            if (Directory.Exists(target))
            {
                Directory.Delete(target, true);
            }

            return Result.Success;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}
