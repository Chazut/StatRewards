using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using StatRewards.Models;

namespace StatRewards.Services;

[Injectable]
public sealed class StatChecker
{
    private readonly ISptLogger<StatChecker> _logger;

    public StatChecker(ISptLogger<StatChecker> logger)
    {
        _logger = logger;
    }

    public List<(MilestoneDefinition milestone, int timesTriggered)> Check(
        ModConfig config,
        MilestoneProgress progress,
        PmcData pmcData,
        bool debug)
    {
        var triggered = new List<(MilestoneDefinition, int)>();
        var counters = pmcData.Stats?.Eft?.OverallCounters?.Items;
        if (counters == null) return triggered;

        foreach (var milestone in config.milestones)
        {
            if (string.IsNullOrEmpty(milestone.stat_key) || milestone.every <= 0)
                continue;

            var rawValue = FindStatValue(counters, milestone.stat_key, milestone.stat_subkey);
            if (rawValue <= 0) continue;

            // Float-type counters (damage, heal, blood loss) are stored as value * 100
            var currentValue = milestone.divisor > 1 ? rawValue / milestone.divisor : rawValue;

            // Apply global threshold multiplier (e.g. 0.5 = half thresholds, 2.0 = double)
            var effectiveEvery = Math.Max(1, (long)(milestone.every * config.threshold_multiplier));
            var currentThreshold = (currentValue / effectiveEvery) * effectiveEvery;

            // First time seeing this milestone: seed to current value, no retroactive rewards
            if (!progress.milestones.ContainsKey(milestone.id))
            {
                progress.milestones[milestone.id] = currentThreshold;
                if (debug)
                    _logger.Info($"[StatRewards] Seeded '{milestone.id}' to {currentThreshold} (current: {currentValue})");
                continue;
            }

            var lastThreshold = progress.milestones[milestone.id];
            if (currentThreshold > lastThreshold)
            {
                var times = (int)((currentThreshold - lastThreshold) / effectiveEvery);
                triggered.Add((milestone, times));
                progress.milestones[milestone.id] = currentThreshold;

                if (debug)
                    _logger.Info($"[StatRewards] '{milestone.id}' triggered {times}x (stat={currentValue}, prev={lastThreshold}, new={currentThreshold})");
            }
        }
        return triggered;
    }

    private static long FindStatValue(
        IEnumerable<CounterKeyValue> counters,
        string statKey,
        string? statSubkey)
    {
        foreach (var counter in counters)
        {
            if (counter.Key == null || counter.Key.Count == 0)
                continue;

            if (!counter.Key.Contains(statKey))
                continue;

            if (statSubkey == null)
            {
                // Simple key match: ["Kills"], ["HeadShots"], etc.
                if (counter.Key.Count == 1)
                    return (long)(counter.Value ?? 0);
            }
            else
            {
                // Compound key match: ["ExitStatus", "Survived", ...], ["Money", "RUB"], etc.
                if (counter.Key.Count > 1 && counter.Key.Contains(statSubkey))
                    return (long)(counter.Value ?? 0);
            }
        }
        return 0;
    }
}
