using System;
using System.IO;
using System.Text.Json;

namespace ShutterFace;

/// <summary>
/// Application configuration persisted to %LOCALAPPDATA%\ShutterFace\config.json.
/// </summary>
public readonly record struct AppConfigData(
    int BigPixels,
    int GridCellSizePixels,
    float ConfidenceThreshold);

/// <summary>
/// Static helpers for loading and saving application configuration.
/// </summary>
public static class AppConfig
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true
    };

    private static string ConfigPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ShutterFace",
            "config.json");

    public static AppConfigData LoadOrCreate()
    {
        int gridCellSize = 96;
        float confidenceThreshold = 0.5f;

        if (File.Exists(ConfigPath))
        {
            try
            {
                var json = File.ReadAllText(ConfigPath);
                var data = JsonSerializer.Deserialize<AppConfigData?>(json, JsonOpts);
                if (data is AppConfigData d)
                {
                    gridCellSize = d.GridCellSizePixels;
                    confidenceThreshold = d.ConfidenceThreshold;
                }
            }
            catch
            {
                // Fall through to defaults
            }
        }

        if (gridCellSize <= 0)
            gridCellSize = 96;

        Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
        var config = new AppConfigData(16, gridCellSize, confidenceThreshold);
        Save(config);
        return config;
    }

    public static void Save(AppConfigData config)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }

        string json = JsonSerializer.Serialize(config, JsonOpts);
        File.WriteAllText(ConfigPath, json);
    }
}
