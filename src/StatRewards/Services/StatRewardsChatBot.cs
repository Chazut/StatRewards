using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Helpers.Dialog.Commando;
using SPTarkov.Server.Core.Helpers.Dialogue;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using StatRewards.Models;

using SPTarkov.DI.Annotations;

namespace StatRewards.Services;

[Injectable]
public class StatRewardsChatBot : AbstractDialogChatBot
{
    private readonly ProfileHelper _profileHelper;
    private readonly ProgressStore _progressStore;
    private readonly State _state;
    private readonly MailSendService _mail;

    public StatRewardsChatBot(
        ISptLogger<AbstractDialogChatBot> logger,
        MailSendService mailSendService,
        ServerLocalisationService localisationService,
        IEnumerable<IChatCommand> chatCommands,
        ProfileHelper profileHelper,
        ProgressStore progressStore,
        State state
    ) : base(logger, mailSendService, localisationService, chatCommands)
    {
        _profileHelper = profileHelper;
        _progressStore = progressStore;
        _state = state;
        _mail = mailSendService;
    }

    public override UserDialogInfo GetChatBot()
    {
        return new UserDialogInfo
        {
            Id = "aaa000aaa000aaa000aaa000",
            Aid = 9999900,
            Info = new UserDialogDetails
            {
                Nickname = "StatRewards",
                Side = "Usec",
                Level = 99,
                MemberCategory = MemberCategory.Developer,
                SelectedMemberCategory = MemberCategory.Developer
            }
        };
    }

    public override ValueTask<string> HandleMessage(MongoId sessionId, SendMessageRequest request)
    {
        string response;

        if (!_state.IsLoaded)
        {
            response = "StatRewards is not loaded.";
        }
        else
        {
            var pmcData = _profileHelper.GetPmcProfile(sessionId);
            if (pmcData == null)
            {
                response = "Could not load your profile.";
            }
            else
            {
                var profileId = sessionId.ToString();
                var progress = _progressStore.Load(profileId);
                var counters = pmcData.Stats?.Eft?.OverallCounters?.Items;
                response = BuildProgressReport(progress, counters);
            }
        }

        _mail.SendUserMessageToPlayer(sessionId, GetChatBot(), response);
        return ValueTask.FromResult(request.DialogId);
    }

    protected override string GetUnrecognizedCommandMessage()
    {
        return "Send me any message to see your milestone progress!";
    }

    private string BuildProgressReport(MilestoneProgress progress, IEnumerable<CounterKeyValue>? counters)
    {
        var lines = new List<string> { "=== StatRewards Progress ===", "" };
        var closest = new List<(string name, string stat, long current, long next, double pct)>();

        foreach (var milestone in _state.Config.milestones)
        {
            if (string.IsNullOrEmpty(milestone.stat_key) || milestone.every <= 0)
                continue;

            var effectiveEvery = Math.Max(1, (long)(milestone.every * _state.Config.threshold_multiplier));

            long currentValue = 0;
            if (counters != null)
                currentValue = FindStatValue(counters, milestone.stat_key, milestone.stat_subkey);
            if (milestone.divisor > 1)
                currentValue /= milestone.divisor;

            var lastThreshold = progress.milestones.GetValueOrDefault(milestone.id, 0);
            var nextThreshold = lastThreshold + effectiveEvery;
            var progressToNext = currentValue - lastThreshold;
            var pct = (double)progressToNext / effectiveEvery * 100;
            if (pct < 0) pct = 0;
            if (pct > 100) pct = 100;

            var displayName = !string.IsNullOrEmpty(milestone.name) ? milestone.name : milestone.id;
            closest.Add((displayName, milestone.stat_key, currentValue, nextThreshold, pct));
        }

        closest.Sort((a, b) => b.pct.CompareTo(a.pct));

        var top = closest.Take(10).ToList();
        foreach (var (name, stat, current, next, pct) in top)
        {
            lines.Add($"{pct:F0}% — {name} ({stat}) — {current}/{next}");
        }

        if (closest.Count > 10)
            lines.Add($"\n+{closest.Count - 10} more");

        return string.Join("\n", lines);
    }

    private static string BuildBar(double pct)
    {
        var filled = (int)(pct / 10);
        var empty = 10 - filled;
        return "[" + new string('#', filled) + new string('-', empty) + "]";
    }

    private static long FindStatValue(IEnumerable<CounterKeyValue> counters, string statKey, string? statSubkey)
    {
        foreach (var counter in counters)
        {
            if (counter.Key == null || counter.Key.Count == 0) continue;
            if (!counter.Key.Contains(statKey)) continue;

            if (statSubkey == null)
            {
                if (counter.Key.Count == 1)
                    return (long)(counter.Value ?? 0);
            }
            else
            {
                if (counter.Key.Count > 1 && counter.Key.Contains(statSubkey))
                    return (long)(counter.Value ?? 0);
            }
        }
        return 0;
    }
}
