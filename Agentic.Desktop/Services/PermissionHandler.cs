using Agentic.ACPLibrary.Client;
using Agentic.ACPLibrary.Models;
using Microsoft.UI.Dispatching;

namespace Agentic.Desktop.Services;

/// <summary>
/// IPermissionHandler 的 UI 实现。
/// 当 Agent 请求权限时，通过事件通知 ViewModel 弹出对话框。
/// </summary>
public class DesktopPermissionHandler : IPermissionHandler
{
    private readonly DispatcherQueue _dispatcherQueue;

    /// <summary>
    /// 当需要显示权限对话框时触发。
    /// ViewModel 订阅此事件，弹出 ContentDialog，然后调用 complete 回调。
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

        // 调度到 UI 线程触发对话框
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
