using Agentic.ACPLibrary.Client;

namespace Agentic.Desktop.Services;

/// <summary>
/// UI implementation of IFileSystemHandler. Includes path validation to ensure the Agent can only access files within the working directory.
/// </summary>
public class DesktopFileSystemHandler : IFileSystemHandler
{
    private readonly string _workingDirectory;

    public DesktopFileSystemHandler(string workingDirectory)
    {
        _workingDirectory = Path.GetFullPath(workingDirectory);
    }

    public async Task<string> ReadTextFileAsync(string path, CancellationToken ct = default)
    {
        ValidatePath(path);
        return await File.ReadAllTextAsync(path, ct);
    }

    public async Task WriteTextFileAsync(string path, string content, CancellationToken ct = default)
    {
        ValidatePath(path);
        // Ensure directory exists
        var dir = Path.GetDirectoryName(path);
        if (dir is not null) Directory.CreateDirectory(dir);
        await File.WriteAllTextAsync(path, content, ct);
    }

    private void ValidatePath(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (!fullPath.StartsWith(_workingDirectory, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException(
                LocalizationService.Format("AccessDeniedMessage", path));
        }
    }
}
