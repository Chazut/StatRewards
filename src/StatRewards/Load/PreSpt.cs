using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Utils;
using StatRewards.Services;

namespace StatRewards.Load;

[Injectable(TypePriority = OnLoadOrder.PreSptModLoader + 10)]
public sealed class PreSpt : IOnLoad
{
    private readonly ISptLogger<PreSpt> _logger;
    private readonly ConfigLoader _loader;
    private readonly State _state;

    public PreSpt(ISptLogger<PreSpt> logger, ConfigLoader loader, State state)
    {
        _logger = logger;
        _loader = loader;
        _state = state;
    }

    public Task OnLoad()
    {
        try
        {
            var configPath = FindConfigPath();
            var config = _loader.LoadModConfig(configPath);
            _state.Set(config);
            _logger.Info($"[StatRewards] Loaded {config.milestones.Count} milestone definitions");

            if (config.debug)
            {
                foreach (var m in config.milestones)
                    _logger.Info($"[StatRewards]   {m.id}: {m.stat_key} every {m.every}, {m.reward_pool.Count} reward(s)");
            }
        }
        catch (Exception ex)
        {
            _logger.Warning($"[StatRewards] Failed to load config: {ex.Message}");
        }
        return Task.CompletedTask;
    }

    private static string FindConfigPath()
    {
        var asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? AppContext.BaseDirectory;

        // Walk up from assembly location looking for config/mod_config.jsonc
        var dir = new DirectoryInfo(asmDir);
        for (int i = 0; i < 12 && dir != null; i++, dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "config", "mod_config.jsonc");
            if (File.Exists(candidate)) return candidate;

            // Direct in current dir
            candidate = Path.Combine(dir.FullName, "mod_config.jsonc");
            if (File.Exists(candidate)) return candidate;
        }

        throw new FileNotFoundException(
            "Could not find config/mod_config.jsonc. " +
            "Ensure StatRewards is installed in SPT/user/mods/StatRewards/ with a config/ subfolder.");
    }
}
