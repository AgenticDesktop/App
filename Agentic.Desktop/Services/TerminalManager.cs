using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Agentic.ACPLibrary.Client;

namespace Agentic.Desktop.Services;

/// <summary>
/// UI implementation of ITerminalHandler. Manages multiple terminal process instances.
/// </summary>
public class TerminalManager : ITerminalHandler, IDisposable
{
    private readonly ConcurrentDictionary<string, TerminalInstance> _terminals = new();
    private int _nextId;

    public Task<string> CreateTerminalAsync(string command, string? workingDirectory, CancellationToken ct = default)
    {
        var id = $"term_{Interlocked.Increment(ref _nextId)}";
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = GetShell(),
                Arguments = GetShellArguments(command),
                WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        var instance = new TerminalInstance(process);
        process.Start();

        // Asynchronously read stdout into buffer
        _ = Task.Run(async () =>
        {
            try
            {
                while (await process.StandardOutput.ReadLineAsync(ct) is { } line)
                {
                    instance.AppendOutput(line + "\n");
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception) { }
        }, ct);

        // Asynchronously read stderr into buffer
        _ = Task.Run(async () =>
        {
            try
            {
                while (await process.StandardError.ReadLineAsync(ct) is { } line)
                {
                    instance.AppendOutput(LocalizationService.Get("StderrPrefix") + line + "\n");
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception) { }
        }, ct);

        _terminals[id] = instance;
        return Task.FromResult(id);
    }

    public Task<string> GetOutputAsync(string terminalId, CancellationToken ct = default)
    {
        if (_terminals.TryGetValue(terminalId, out var instance))
            return Task.FromResult(instance.GetOutput());
        return Task.FromResult(string.Empty);
    }

    public async Task<int> WaitForExitAsync(string terminalId, CancellationToken ct = default)
    {
        if (!_terminals.TryGetValue(terminalId, out var instance))
            return -1;

        await instance.Process.WaitForExitAsync(ct);
        return instance.Process.ExitCode;
    }

    public Task KillTerminalAsync(string terminalId, CancellationToken ct = default)
    {
        if (_terminals.TryGetValue(terminalId, out var instance))
        {
            try
            {
                if (!instance.Process.HasExited)
                    instance.Process.Kill(entireProcessTree: true);
            }
            catch { }
        }
        return Task.CompletedTask;
    }

    public Task ReleaseTerminalAsync(string terminalId, CancellationToken ct = default)
    {
        if (_terminals.TryRemove(terminalId, out var instance))
        {
            try
            {
                if (!instance.Process.HasExited)
                    instance.Process.Kill(entireProcessTree: true);
            }
            catch { }
            instance.Process.Dispose();
        }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        foreach (var (_, instance) in _terminals)
        {
            try
            {
                if (!instance.Process.HasExited)
                    instance.Process.Kill(entireProcessTree: true);
            }
            catch { }
            instance.Process.Dispose();
        }
        _terminals.Clear();
    }

    private static string GetShell() =>
        OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh";

    private static string GetShellArguments(string command) =>
        OperatingSystem.IsWindows() ? $"/c {command}" : $"-c \"{command}\"";
}

internal class TerminalInstance
{
    public Process Process { get; }
    private readonly StringBuilder _output = new();
    private readonly object _lock = new();

    public TerminalInstance(Process process) => Process = process;

    public void AppendOutput(string text)
    {
        lock (_lock)
        {
            _output.Append(text);
        }
    }

    public string GetOutput()
    {
        lock (_lock)
        {
            return _output.ToString();
        }
    }
}
