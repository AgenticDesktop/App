using Markdig;

namespace Agentic.Desktop.Services;

/// <summary>
/// Markdown utility class.
/// Current WinUI 3 TextBlock does not support rich text/HTML rendering;
/// can be upgraded to WebView2 for full Markdown rendering in the future.
/// </summary>
public static class MarkdownHelper
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    /// <summary>
    /// Converts Markdown to HTML (can be used for WebView2 rendering).
    /// </summary>
    public static string ToHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        return Markdig.Markdown.ToHtml(markdown, Pipeline);
    }

    /// <summary>
    /// Converts Markdown to plain text (strips formatting markers).
    /// This is a temporary solution until WebView2 is integrated for rich text rendering.
    /// </summary>
    public static string ToPlainText(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        // Strip common Markdown formatting markers
        var text = markdown;
        // Remove heading markers (# ## ### etc.)
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^#{1,6}\s+", "", System.Text.RegularExpressions.RegexOptions.Multiline);
        // Remove bold/italic markers
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\*{1,3}([^*]+)\*{1,3}", "$1");
        // Remove code block markers
        text = System.Text.RegularExpressions.Regex.Replace(text, @"```\w*\n?", "");
        // Remove inline code markers
        text = System.Text.RegularExpressions.Regex.Replace(text, @"`([^`]+)`", "$1");
        // Remove link markers [text](url)
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\[([^\]]+)\]\([^)]+\)", "$1");

        return text.Trim();
    }
}
