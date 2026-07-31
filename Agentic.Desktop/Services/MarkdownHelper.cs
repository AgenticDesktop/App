using Markdig;

namespace Agentic.Desktop.Services;

/// <summary>
/// Markdown 处理工具类。
/// 当前 WinUI 3 的 TextBlock 不支持富文本/HTML 渲染，
/// 后续可升级为 WebView2 渲染完整 Markdown。
/// </summary>
public static class MarkdownHelper
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    /// <summary>
    /// 将 Markdown 转换为 HTML（可用于 WebView2 渲染）。
    /// </summary>
    public static string ToHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        return Markdig.Markdown.ToHtml(markdown, Pipeline);
    }

    /// <summary>
    /// 将 Markdown 转换为纯文本（去除格式标记）。
    /// 当前作为临时方案，直到集成 WebView2 进行富文本渲染。
    /// </summary>
    public static string ToPlainText(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        // 简单去除常见 Markdown 格式标记
        var text = markdown;
        // 去除标题标记 (# ## ### 等)
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^#{1,6}\s+", "", System.Text.RegularExpressions.RegexOptions.Multiline);
        // 去除粗体/斜体标记
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\*{1,3}([^*]+)\*{1,3}", "$1");
        // 去除代码块标记
        text = System.Text.RegularExpressions.Regex.Replace(text, @"```\w*\n?", "");
        // 去除行内代码标记
        text = System.Text.RegularExpressions.Regex.Replace(text, @"`([^`]+)`", "$1");
        // 去除链接标记 [text](url)
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\[([^\]]+)\]\([^)]+\)", "$1");

        return text.Trim();
    }
}
