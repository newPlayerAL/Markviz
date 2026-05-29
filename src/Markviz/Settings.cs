using System;
using System.IO;
using System.Text.Json;

namespace Markviz;

/// <summary>
/// Persistent user preferences. Stored at %LOCALAPPDATA%\Markviz\settings.json.
/// </summary>
internal sealed class Settings
{
    /// <summary>Language code: "zh", "en", or null = not yet chosen (first run).</summary>
    public string? Language { get; set; }

    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Markviz",
        "settings.json");

    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    public static Settings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }
        }
        catch
        {
            // Corrupt file or no permission — fall back to defaults rather than crash.
        }
        return new Settings();
    }

    public void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(FilePath);
            if (dir != null) Directory.CreateDirectory(dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, JsonOpts));
        }
        catch
        {
            // Best-effort: not worth aborting the app over a failed settings write.
        }
    }
}
