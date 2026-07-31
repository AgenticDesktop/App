using Windows.ApplicationModel.Resources;

namespace Agentic.Desktop.Services;

/// <summary>
/// Provides localized strings from .resw resource files.
/// </summary>
public static class LocalizationService
{
    private static readonly ResourceLoader _loader = new("Resources");

    /// <summary>
    /// Gets a localized string by its resource key.
    /// </summary>
    public static string Get(string key) => _loader.GetString(key);

    /// <summary>
    /// Gets a localized string and formats it with the provided arguments.
    /// </summary>
    public static string Format(string key, params object[] args)
        => string.Format(_loader.GetString(key), args);
}
