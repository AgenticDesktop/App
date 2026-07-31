using Agentic.ACPLibrary.Client;
using Agentic.ACPLibrary.Models;
using Microsoft.UI.Dispatching;

namespace Agentic.Desktop.Services;

/// <summary>
/// UI implementation of IPermissionHandler.
/// When the Agent requests permission, notifies the ViewModel via event to show a dialog.
/// </summary>
public class DesktopPermissionHandler : IPermissionHandler
{
    private readonly DispatcherQueue _dispatcherQueue;

    /// <summary>
    /// Raised when a permission dialog needs to be shown.
    /// The ViewModel subscribes to this event, shows a ContentDialog, then invokes the complete callback.
    /// </summary>
    public event Func<PermissionRequestEventArgs, Task>? PermissionRequested;

    public DesktopPermissionHandler(DispatcherQueue dispatcherQueue)
    {
        _dispatcherQueue = dispatcherQueue;
    }

    public async Task<RequestPermissionResponse> HandlePermissionRequestAsync(
        RequestPermissionRequest request, CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<RequestPermissionResponse>();

        var args = new PermissionRequestEventArgs
        {
            Request = request,
            OnComplete = response => tcs.TrySetResult(response)
        };

        // Dispatch to UI thread to trigger dialog
        _dispatcherQueue.TryEnqueue(() =>
        {
            _ = PermissionRequested?.Invoke(args);
        });

        return await tcs.Task;
    }
}

public class PermissionRequestEventArgs : EventArgs
{
    public required RequestPermissionRequest Request { get; init; }
    public required Action<RequestPermissionResponse> OnComplete { get; init; }
}
