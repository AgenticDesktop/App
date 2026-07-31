using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Agentic.ACPLibrary.Models;

namespace Agentic.Desktop.Views;

public sealed partial class PermissionDialog : ContentDialog
{
    private readonly TaskCompletionSource<RequestPermissionResponse> _tcs = new();
    private readonly RequestPermissionRequest _request;

    public Task<RequestPermissionResponse> Result => _tcs.Task;

    public PermissionDialog(RequestPermissionRequest request)
    {
        InitializeComponent();
        _request = request;

        ToolTitle.Text = request.ToolCall?.Title ?? "Unknown action";
        ToolKind.Text = request.ToolCall?.Kind?.ToString() ?? "";

        // 设置选项
        OptionsRepeater.ItemsSource = request.Options;

        PrimaryButtonClick += (_, _) =>
        {
            // 默认 primary = 第一个 allow 选项
            var allowOption = request.Options.FirstOrDefault(o =>
                o.Kind.Contains("allow", StringComparison.OrdinalIgnoreCase));
            if (allowOption is not null)
                _tcs.TrySetResult(new RequestPermissionResponse
                {
                    Outcome = PermissionOutcome.Selected(allowOption.OptionId)
                });
            else
                _tcs.TrySetResult(new RequestPermissionResponse
                {
                    Outcome = PermissionOutcome.Cancelled()
                });
        };

        CloseButtonClick += (_, _) =>
        {
            _tcs.TrySetResult(new RequestPermissionResponse
            {
                Outcome = PermissionOutcome.Cancelled()
            });
        };
    }

    private void OnOptionClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string optionId)
        {
            _tcs.TrySetResult(new RequestPermissionResponse
            {
                Outcome = PermissionOutcome.Selected(optionId)
            });
            Hide();
        }
    }
}
