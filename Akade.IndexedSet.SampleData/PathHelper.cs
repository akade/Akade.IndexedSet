using System.Diagnostics;

namespace Akade.IndexedSet.SampleData;

public static class PathHelper
{
    public static async Task<string> GetGitRootPathAsync(string? workingDirectory = null)
    {
        workingDirectory ??= Environment.CurrentDirectory;


        // Start git rev-parse --show-toplevel
        Process process = new()
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

        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync();
        return output.Trim();
    }
}
