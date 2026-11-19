using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Services.Commerce;
using SPTarkov.Server.Core.Utils;

namespace StatRewards.Services;

/// <summary>
/// Hooks into /client/game/profile/list which fires after raid-end when the player
/// returns to the main menu. At this point, the profile OverallCounters are fully
/// updated with the raid's session counters.
///
/// Since SPT 4.1.5 this route is STREAMED (StreamedRouteAction in ProfileStaticRouter) and a
/// streamed payload cannot be passed through between routers - a plain RouteAction returning a
/// string here wiped the vanilla response and broke profile loading with an UNHANDLED RESPONSE.
/// The hook therefore runs its side effects and then produces the payload itself by calling the
/// vanilla callback, which is correct whichever router runs last.
/// </summary>
[Injectable]
public sealed class MilestoneRouter(
    JsonUtil jsonUtil,
    ProfileCallbacks profileCallbacks,
    ProfileHelper profileHelper,
    MailSendService mailSend,
    StatChecker statChecker,
    RewardGenerator rewardGenerator,
    ProgressStore progressStore,
    State state,
    ISptLogger<MilestoneRouter> logger
) : StaticRouter(jsonUtil, [
    new StreamedRouteAction<EmptyRequestData>(
        "/client/game/profile/list",
        async (url, requestData, sessionId, cancellationToken) =>
        {
            RunMilestoneChecks(sessionId, profileHelper, mailSend,
                statChecker, rewardGenerator, progressStore, state, logger);
            return await profileCallbacks.GetProfileData(url, requestData, sessionId);
        }
    )
])
{
    private static void RunMilestoneChecks(
        MongoId sessionId,
        ProfileHelper profileHelper, MailSendService mailSend,
        StatChecker statChecker, RewardGenerator rewardGenerator,
        ProgressStore progressStore, State state,
        ISptLogger<MilestoneRouter> logger)
    {
        if (!state.IsLoaded)
            return;

        try
        {
            var pmcData = profileHelper.GetPmcProfile(sessionId);
            if (pmcData == null)
                return;

            var profileId = sessionId.ToString();
            var progress = progressStore.Load(profileId);
            var triggered = statChecker.Check(state.Config, progress, pmcData, state.Config.debug);

            // Always save progress (even if nothing triggered, first-run seeding may have occurred)
            progressStore.Save(profileId, progress);

            if (triggered.Count == 0)
                return;

            foreach (var (milestone, times) in triggered)
            {
                var items = rewardGenerator.Generate(milestone, times, state.Config.varied_parcels);
                if (items.Count == 0) continue;

                var sender = !string.IsNullOrEmpty(milestone.sender)
                    ? milestone.sender
                    : "54cb50c76803fa8b248b4571"; // Prapor fallback

                // Calculate milestone number from saved progress
                var effectiveEvery = milestone.Interval(state.Config.threshold_multiplier);
                var milestoneNumber = progress.milestones.TryGetValue(milestone.id, out var threshold)
                    ? threshold / effectiveEvery
                    : 0;

                // Build mail message: name + number + stat + custom message
                var displayName = !string.IsNullOrEmpty(milestone.name) ? milestone.name : milestone.id;
                var statDisplay = milestone.stat_subkey != null
                    ? $"{milestone.stat_key} > {milestone.stat_subkey}"
                    : milestone.stat_key;
                var mailMessage = $"{displayName} — Milestone x{milestoneNumber}\n[{statDisplay}: {threshold:N0}]\n\n{milestone.message}";

                mailSend.SendDirectNpcMessageToPlayer(
                    sessionId,
                    sender,
                    MessageType.MessageWithItems,
                    mailMessage,
                    items);

                logger.Info($"[StatRewards] '{milestone.id}' #{milestoneNumber} triggered {times}x for {profileId}, sent {items.Count} item(s) via {sender}");
            }
        }
        catch (Exception ex)
        {
            logger.Error($"[StatRewards] Error processing raid end: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
