using System.Runtime.InteropServices;

namespace Agentic.Desktop.Services;

/// <summary>
/// Resolves command names to their full paths by searching the system PATH environment variable.
/// Useful for allowing users to enter simple command names like "npx" instead of full paths.
/// </summary>
public static class CommandResolver
{
    /// <summary>
    /// Resolves a command name to its full path. 
    /// If the input is already a full path or contains path separators, returns it as-is.
    /// Otherwise, searches the system PATH for the command.
    /// </summary>
    /// <param name="command">The command name (e.g., "npx") or full path</param>
    /// <returns>The resolved full path, or the original command if not found</returns>
    public static string ResolveCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return command;

        // If the command already contains path separators, it's likely a full path
        if (command.Contains(Path.DirectorySeparatorChar) || command.Contains(Path.AltDirectorySeparatorChar))
            return command;

        // If it's a full path that exists, return it
        if (File.Exists(command))
            return command;

        // Search in PATH environment variable
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv))
            return command;

        var pathDirs = pathEnv.Split(Path.PathSeparator);
        var searchExtensions = GetSearchExtensions();

        foreach (var dir in pathDirs)
        {
            if (string.IsNullOrWhiteSpace(dir))
                continue;

            foreach (var ext in searchExtensions)
            {
                var fullPath = Path.Combine(dir, command + ext);
                if (File.Exists(fullPath))
                    return fullPath;
            }
        }

        // Not found in PATH, return original command and let it fail with a more meaningful error
        return command;
    }

    /// <summary>
    /// Gets the file extensions to search for based on the operating system.
    /// On Windows, searches for .exe, .cmd, .bat, and no extension.
    /// On Unix-like systems, searches for no extension only.
    /// </summary>
    private static string[] GetSearchExtensions()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new[] { ".exe", ".cmd", ".bat", ".com", string.Empty };
        }
        return new[] { string.Empty };
    }
}
