using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Agentic.ACPLibrary.Models;
using Agentic.Desktop.Services;

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

        ToolTitle.Text = request.ToolCall?.Title ?? LocalizationService.Get("UnknownAction");
        ToolKind.Text = request.ToolCall?.Kind?.ToString() ?? "";

        // Set options
        OptionsRepeater.ItemsSource = request.Options;

        PrimaryButtonClick += (_, _) =>
        {
            // Default primary = first allow option
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
