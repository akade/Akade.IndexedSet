using System.Diagnostics;

namespace Akade.IndexedSet.SampleData;

public static class PathHelper
{
    public static async Task<string> GetGitRootPathAsync(string? workingDirectory = null)
    {
        workingDirectory ??= Environment.CurrentDirectory;

        // Start git rev-parse --show-toplevel
        using Process process = new()
        {
            StartInfo = new()
            {
                FileName = "git",
                Arguments = "rev-parse --show-toplevel",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory
            }
        };

        _ = process.Start();
        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if(process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Git command failed with exit code {process.ExitCode}. Error: {error}");
        }

        return output.Trim();
    }
}
