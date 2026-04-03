using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Server;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace StatRewards.Services;

/// <summary>
/// Hooks into /client/game/profile/list which fires after raid-end when the player
/// returns to the main menu. At this point, the profile OverallCounters are fully
/// updated with the raid's session counters.
/// </summary>
[Injectable]
public sealed class MilestoneRouter(
    JsonUtil jsonUtil,
    ProfileHelper profileHelper,
    MailSendService mailSend,
    StatChecker statChecker,
    RewardGenerator rewardGenerator,
    ProgressStore progressStore,
    State state,
    ISptLogger<MilestoneRouter> logger
) : StaticRouter(jsonUtil, [
    new RouteAction<object>(
        "/client/game/profile/list",
        (url, requestData, sessionId, output) =>
            ProcessProfileList(sessionId, output, profileHelper, mailSend,
                statChecker, rewardGenerator, progressStore, state, logger)
    )
])
{
    private static ValueTask<string> ProcessProfileList(
        MongoId sessionId, string? output,
        ProfileHelper profileHelper, MailSendService mailSend,
        StatChecker statChecker, RewardGenerator rewardGenerator,
        ProgressStore progressStore, State state,
        ISptLogger<MilestoneRouter> logger)
    {
        if (!state.IsLoaded)
            return ValueTask.FromResult(output ?? string.Empty);

        try
        {
            var pmcData = profileHelper.GetPmcProfile(sessionId);
            if (pmcData == null)
                return ValueTask.FromResult(output ?? string.Empty);

            var profileId = sessionId.ToString();
            var progress = progressStore.Load(profileId);
            var triggered = statChecker.Check(state.Config, progress, pmcData, state.Config.debug);

            // Always save progress (even if nothing triggered, first-run seeding may have occurred)
            progressStore.Save(profileId, progress);

            if (triggered.Count == 0)
                return ValueTask.FromResult(output ?? string.Empty);

            foreach (var (milestone, times) in triggered)
            {
                var items = rewardGenerator.Generate(milestone, times);
                if (items.Count == 0) continue;

                var sender = !string.IsNullOrEmpty(milestone.sender)
                    ? milestone.sender
                    : "54cb50c76803fa8b248b4571"; // Prapor fallback

                mailSend.SendDirectNpcMessageToPlayer(
                    sessionId,
                    sender,
                    MessageType.MessageWithItems,
                    milestone.message,
                    items);

                logger.Info($"[StatRewards] '{milestone.id}' triggered {times}x for {profileId}, sent {items.Count} item(s) via {sender}");
            }
        }
        catch (Exception ex)
        {
            logger.Error($"[StatRewards] Error processing raid end: {ex.Message}\n{ex.StackTrace}");
        }

        return ValueTask.FromResult(output ?? string.Empty);
    }
}
