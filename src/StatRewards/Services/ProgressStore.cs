using System.Reflection;
using System.Text.Json;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using StatRewards.Models;

namespace StatRewards.Services;

[Injectable]
public sealed class ProgressStore
{
    private readonly ISptLogger<ProgressStore> _logger;
    private string? _progressDir;

    private readonly JsonSerializerOptions _opts = new()
    {
        WriteIndented = true
    };

    public ProgressStore(ISptLogger<ProgressStore> logger)
    {
        _logger = logger;
    }

    public void EnsureDirectory()
    {
        var dir = GetProgressDir();
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
            _logger.Info($"[StatRewards] Created progress directory: {dir}");
        }
    }

    public MilestoneProgress Load(string profileId)
    {
        var path = GetFilePath(profileId);
        if (!File.Exists(path))
            return new MilestoneProgress();

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<MilestoneProgress>(json, _opts)
                ?? new MilestoneProgress();
        }
        catch (Exception ex)
        {
            _logger.Warning($"[StatRewards] Failed to load progress for {profileId}: {ex.Message}");
            return new MilestoneProgress();
        }
    }

    public void Save(string profileId, MilestoneProgress progress)
    {
        var path = GetFilePath(profileId);
        var tmpPath = path + ".tmp";
        try
        {
            var json = JsonSerializer.Serialize(progress, _opts);
            File.WriteAllText(tmpPath, json);
            File.Move(tmpPath, path, overwrite: true);
        }
        catch (Exception ex)
        {
            _logger.Warning($"[StatRewards] Failed to save progress for {profileId}: {ex.Message}");
        }
    }

    private string GetFilePath(string profileId)
    {
        return Path.Combine(GetProgressDir(), $"{profileId}.json");
    }

    private string GetProgressDir()
    {
        if (_progressDir != null) return _progressDir;

        var asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? AppContext.BaseDirectory;

        // Walk up to find the mod root (directory containing config/)
        var dir = new DirectoryInfo(asmDir);
        for (int i = 0; i < 12 && dir != null; i++, dir = dir.Parent)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "config")))
            {
                _progressDir = Path.Combine(dir.FullName, "progress");
                return _progressDir;
            }
        }

        // Fallback: next to assembly
        _progressDir = Path.Combine(asmDir, "progress");
        return _progressDir;
    }
}
