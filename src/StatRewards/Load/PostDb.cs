using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using StatRewards.Services;

namespace StatRewards.Load;

[Injectable(TypePriority = OnLoadOrder.Database + 50)]
public sealed class PostDb : IOnLoad
{
    private readonly ISptLogger<PostDb> _logger;
    private readonly State _state;
    private readonly DatabaseService _db;
    private readonly ProgressStore _progressStore;

    public PostDb(ISptLogger<PostDb> logger, State state, DatabaseService db, ProgressStore progressStore)
    {
        _logger = logger;
        _state = state;
        _db = db;
        _progressStore = progressStore;
    }

    public Task OnLoad()
    {
        if (!_state.IsLoaded)
        {
            _logger.Warning("[StatRewards] Config not loaded, mod is disabled.");
            return Task.CompletedTask;
        }

        // Validate configured trader IDs exist
        var tables = _db.GetTables();
        foreach (var milestone in _state.Config.milestones)
        {
            var traderId = !string.IsNullOrEmpty(milestone.sender)
                ? milestone.sender
                : "54cb50c76803fa8b248b4571";

            if (tables?.Traders?.ContainsKey(traderId) != true)
                _logger.Warning($"[StatRewards] Milestone '{milestone.id}': trader '{traderId}' not found in DB");
        }

        _progressStore.EnsureDirectory();

        _logger.Info($"[StatRewards] Ready. {_state.Config.milestones.Count} milestones will be checked after each raid.");
        return Task.CompletedTask;
    }
}
