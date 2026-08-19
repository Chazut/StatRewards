using System.Text.Json;
using System.Text.Json.Nodes;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Utils;

namespace StatRewards.Services;

/// <summary>
/// Injects the StatRewards bot into the /client/friend/list response.
/// Still needed in 4.1: DialogueController.GetActiveChatBots only lists bots
/// whitelisted in CoreConfig ChatbotFeatures.EnabledBots, which mod bots are not.
/// </summary>
[Injectable]
public sealed class FriendListRouter(
    JsonUtil jsonUtil,
    ISptLogger<FriendListRouter> logger
) : StaticRouter(jsonUtil, [
    new RouteAction<EmptyRequestData>(
        "/client/friend/list",
        (url, requestData, sessionId, output, cancellationToken) =>
        {
            if (string.IsNullOrEmpty(output))
                return ValueTask.FromResult(output ?? string.Empty);

            try
            {
                var node = JsonNode.Parse(output);
                var friends = node?["data"]?["Friends"]?.AsArray();
                if (friends == null)
                    return ValueTask.FromResult(output);

                // Check if already injected
                const string botId = "aaa000aaa000aaa000aaa000";
                foreach (var f in friends)
                {
                    if (f?["_id"]?.GetValue<string>() == botId)
                        return ValueTask.FromResult(output);
                }

                // Inject our bot
                friends.Add(JsonNode.Parse("""
                {
                    "_id": "aaa000aaa000aaa000aaa000",
                    "aid": 9999900,
                    "Info": {
                        "Nickname": "StatRewards",
                        "Side": "Usec",
                        "Level": 99,
                        "MemberCategory": 1,
                        "SelectedMemberCategory": 1
                    }
                }
                """));

                var result = node!.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
                logger.Info("[StatRewards] Injected chatbot into friend list");
                return ValueTask.FromResult(result);
            }
            catch
            {
                return ValueTask.FromResult(output ?? string.Empty);
            }
        }
    )
])
{
}
