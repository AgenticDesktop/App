using Agentic.ACPLibrary.Client;

namespace Agentic.Desktop.Services;

/// <summary>
/// IFileSystemHandler 的 UI 实现。包含路径验证确保 Agent 只能访问工作目录内的文件。
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
        // 确保目录存在
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
                $"Access denied: '{path}' is outside working directory '{_workingDirectory}'");
        }
    }
}
