using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Tables;
using StatRewards.Services;

namespace StatRewards.Load;

[Injectable(TypePriority = OnLoadOrder.PostLoad + 50)]
public sealed class PostDb : IOnLoad
{
    private readonly ISptLogger<PostDb> _logger;
    private readonly State _state;
    private readonly TradersTable _traders;
    private readonly ProgressStore _progressStore;

    public PostDb(ISptLogger<PostDb> logger, State state, TradersTable traders, ProgressStore progressStore)
    {
        _logger = logger;
        _state = state;
        _traders = traders;
        _progressStore = progressStore;
    }

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        if (!_state.IsLoaded)
        {
            _logger.Warning("[StatRewards] Config not loaded, mod is disabled.");
            return Task.CompletedTask;
        }

        // Validate configured trader IDs exist
        foreach (var milestone in _state.Config.milestones)
        {
            var traderId = !string.IsNullOrEmpty(milestone.sender)
                ? milestone.sender
                : "54cb50c76803fa8b248b4571";

            if (!_traders.ContainsKey(traderId))
                _logger.Warning($"[StatRewards] Milestone '{milestone.id}': trader '{traderId}' not found in DB");
        }

        _progressStore.EnsureDirectory();

        _logger.Info($"[StatRewards] Ready. {_state.Config.milestones.Count} milestones will be checked after each raid.");
        return Task.CompletedTask;
    }
}
