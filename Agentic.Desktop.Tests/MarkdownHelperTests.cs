using Agentic.Desktop.Services;
using Xunit;

namespace Agentic.Desktop.Tests;

public class MarkdownHelperTests
{
    [Fact]
    public void ToHtml_WithBoldMarkdown_ProducesStrongTag()
    {
        var result = MarkdownHelper.ToHtml("**bold**");
        Assert.Contains("<strong>bold</strong>", result);
    }

    [Fact]
    public void ToHtml_WithHeading_ProducesHeadingTag()
    {
        var result = MarkdownHelper.ToHtml("# Title");
        Assert.Contains("<h1", result);
        Assert.Contains("Title", result);
        Assert.Contains("</h1>", result);
    }

    [Fact]
    public void ToHtml_EmptyInput_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, MarkdownHelper.ToHtml(""));
    }

    [Fact]
    public void ToHtml_NullOrWhiteSpace_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, MarkdownHelper.ToHtml("   "));
    }

    [Fact]
    public void ToPlainText_RemovesHeadingMarkers()
    {
        var result = MarkdownHelper.ToPlainText("# Heading");
        Assert.Equal("Heading", result);
    }

    [Fact]
    public void ToPlainText_RemovesBoldMarkers()
    {
        var result = MarkdownHelper.ToPlainText("**bold**");
        Assert.Equal("bold", result);
    }

    [Fact]
    public void ToPlainText_RemovesItalicMarkers()
    {
        var result = MarkdownHelper.ToPlainText("*italic*");
        Assert.Equal("italic", result);
    }

    [Fact]
    public void ToPlainText_RemovesCodeBlockMarkers()
    {
        var result = MarkdownHelper.ToPlainText("```csharp\ncode\n```");
        Assert.Contains("code", result);
        Assert.DoesNotContain("```", result);
    }

    [Fact]
    public void ToPlainText_RemovesInlineCodeMarkers()
    {
        var result = MarkdownHelper.ToPlainText("`code`");
        Assert.Equal("code", result);
    }

    [Fact]
    public void ToPlainText_RemovesLinkMarkers()
    {
        var result = MarkdownHelper.ToPlainText("[text](https://example.com)");
        Assert.Equal("text", result);
    }

    [Fact]
    public void ToPlainText_EmptyInput_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, MarkdownHelper.ToPlainText(""));
    }

    [Fact]
    public void ToPlainText_NullOrWhiteSpace_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, MarkdownHelper.ToPlainText("   "));
    }

    [Fact]
    public void ToPlainText_MixedMarkdown_StripsAllFormatting()
    {
        var markdown = "# Title\n**bold** and *italic*\n`code`";
        var result = MarkdownHelper.ToPlainText(markdown);
        Assert.DoesNotContain("#", result);
        Assert.DoesNotContain("**", result);
        Assert.DoesNotContain("`", result);
    }
}
