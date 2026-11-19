using SPTarkov.Server.Core.Models.Common;
using StatRewards.Models;

namespace StatRewards.Services;

internal sealed record RewardSource(RewardPoolEntry Entry, string Group, IReadOnlyList<MongoId> Templates);

// One mail can include several milestone triggers. The entire parcel shares these two cycles.
internal sealed class VariedParcel(IReadOnlyList<RewardSource> sources, Random random)
{
    private readonly HashSet<string> usedGroups = [];
    private readonly HashSet<MongoId> usedItems = [];
    private readonly HashSet<(string?, MongoId)> unavailable = [];

    public bool Draw(Func<RewardPoolEntry, MongoId, bool> deliver)
    {
        while (true)
        {
            var candidates = Available();
            if (candidates.Count == 0 && usedItems.Count > 0)
            {
                usedItems.Clear();
                candidates = Available();
            }
            if (candidates.Count == 0) return false;

            var fresh = candidates.Where(s => !usedGroups.Contains(s.Group)).ToList();
            if (fresh.Count == 0) usedGroups.Clear();
            else candidates = fresh;

            // A larger category has the same first-draw odds as a smaller category of equal weight.
            var pick = PickWeighted(candidates);
            IReadOnlyList<MongoId> templates = usedItems.Count == 0 && unavailable.Count == 0
                ? pick.Templates : pick.Templates.Where(id => Eligible(pick, id)).ToArray();
            var template = templates[random.Next(templates.Count)];
            if (deliver(pick.Entry, template))
            {
                usedItems.Add(template);
                usedGroups.Add(pick.Group);
                return true;
            }

            // Some modded gear has no usable preset, but can still be given as a fixed item.
            unavailable.Add((pick.Entry.category, template));
        }
    }

    private bool Eligible(RewardSource source, MongoId id) =>
        !usedItems.Contains(id) && !unavailable.Contains((source.Entry.category, id));

    private List<RewardSource> Available() => sources
        .Where(s => s.Templates.Any(id => Eligible(s, id))).ToList();

    private RewardSource PickWeighted(List<RewardSource> candidates)
    {
        double max = candidates.Max(s => s.Entry.weight);
        double roll = random.NextDouble() * candidates.Sum(s => s.Entry.weight / max);
        foreach (var source in candidates)
        {
            roll -= source.Entry.weight / max;
            if (roll < 0) return source;
        }
        return candidates[^1];
    }
}
