using Agentic.Desktop.Services;
using Xunit;

namespace Agentic.Desktop.Tests;

public class FileSystemHandlerTests : IDisposable
{
    private readonly string _testDir;
    private readonly DesktopFileSystemHandler _handler;

    public FileSystemHandlerTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "agentic_fs_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);
        _handler = new DesktopFileSystemHandler(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, recursive: true);
    }

    [Fact]
    public async Task WriteAndRead_RoundTripsContent()
    {
        var path = Path.Combine(_testDir, "test.txt");
        await _handler.WriteTextFileAsync(path, "hello world");

        var content = await _handler.ReadTextFileAsync(path);
        Assert.Equal("hello world", content);
    }

    [Fact]
    public async Task WriteTextFile_CreatesSubdirectory()
    {
        var path = Path.Combine(_testDir, "sub", "dir", "file.txt");
        await _handler.WriteTextFileAsync(path, "nested");

        Assert.True(File.Exists(path));
        Assert.Equal("nested", await File.ReadAllTextAsync(path));
    }

    [Fact]
    public async Task ReadTextFile_OutsideWorkingDir_Throws()
    {
        var outsidePath = Path.Combine(Path.GetTempPath(), "outside_" + Guid.NewGuid().ToString("N") + ".txt");
        await File.WriteAllTextAsync(outsidePath, "secret");

        try
        {
            await Assert.ThrowsAnyAsync<Exception>(
                () => _handler.ReadTextFileAsync(outsidePath));
        }
        finally
        {
            File.Delete(outsidePath);
        }
    }

    [Fact]
    public async Task WriteTextFile_OutsideWorkingDir_Throws()
    {
        var outsidePath = Path.Combine(Path.GetTempPath(), "outside_" + Guid.NewGuid().ToString("N") + ".txt");

        await Assert.ThrowsAnyAsync<Exception>(
            () => _handler.WriteTextFileAsync(outsidePath, "data"));

        Assert.False(File.Exists(outsidePath));
    }

    [Fact]
    public async Task ReadTextFile_InsideWorkingDir_Succeeds()
    {
        var path = Path.Combine(_testDir, "existing.txt");
        await File.WriteAllTextAsync(path, "pre-existing");

        var content = await _handler.ReadTextFileAsync(path);
        Assert.Equal("pre-existing", content);
    }

    [Fact]
    public async Task WriteTextFile_OverwritesExistingContent()
    {
        var path = Path.Combine(_testDir, "overwrite.txt");
        await _handler.WriteTextFileAsync(path, "first");
        await _handler.WriteTextFileAsync(path, "second");

        var content = await _handler.ReadTextFileAsync(path);
        Assert.Equal("second", content);
    }

    [Fact]
    public async Task ReadTextFile_EmptyFile_ReturnsEmpty()
    {
        var path = Path.Combine(_testDir, "empty.txt");
        await File.WriteAllTextAsync(path, "");

        var content = await _handler.ReadTextFileAsync(path);
        Assert.Equal("", content);
    }
}
