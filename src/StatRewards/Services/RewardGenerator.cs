using System.Collections.Concurrent;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Utils;
using StatRewards.Models;

namespace StatRewards.Services;

[Injectable]
public sealed class RewardGenerator
{
    private readonly TemplateTable _templates;
    private readonly ItemHelper _itemHelper;
    private readonly PresetHelper _presetHelper;
    private readonly RandomUtil _randomUtil;
    private readonly ISptLogger<RewardGenerator> _logger;

    // Cached category pools (lazy-loaded)
    private readonly ConcurrentDictionary<string, List<MongoId>> _categoryPools = new();
    private readonly ConcurrentDictionary<MongoId, string> _fixedGroups = new();

    // Base class IDs for each category
    private static readonly Dictionary<string, string[]> CategoryBaseClasses = new()
    {
        ["meds"] = [
            "5448f39d4bdc2d0a728b4568", // Medkit
            "5448f3a14bdc2d27728b4569", // Drug
            "5448f3a64bdc2d60728b456a"  // Stimulator
        ],
        ["ammo"] = [
            "5485a8684bdc2da71d8b4567"  // Ammo
        ],
        ["food"] = [
            "5448e8d04bdc2ddf718b4569"  // Food
        ],
        ["drink"] = [
            "5448e8d64bdc2dce718b4568"  // Drink
        ],
        ["keys"] = [
            "5c99f98d86f7745c314214b3"  // Mechanical key
        ],
        ["barter"] = [
            "57864a3d24597754843f8721"  // Barter item
        ],
        ["gear"] = [
            "5448e5284bdc2dcb718b4567", // Vest (rigs)
            "5448e54d4bdc2dcc718b4568", // Armor
            "5a341c4086f77401f2541505", // Headwear
            "5448e53e4bdc2d60728b4567"  // Backpack
        ],
        ["weapon_preset"] = [
            // Handled specially — picks a weapon and applies its default preset
        ]
    };

    // Rouble template ID
    private const string RoublesTpl = "5449016a4bdc2d6f028b456f";

    public RewardGenerator(
        TemplateTable templates,
        ItemHelper itemHelper,
        PresetHelper presetHelper,
        RandomUtil randomUtil,
        ISptLogger<RewardGenerator> logger)
    {
        _templates = templates;
        _itemHelper = itemHelper;
        _presetHelper = presetHelper;
        _randomUtil = randomUtil;
        _logger = logger;
    }

    public List<Item> Generate(MilestoneDefinition milestone, int times = 1, bool varied = true)
    {
        var items = new List<Item>();
        Roll(milestone, times, varied, long.MaxValue, (template, amount, preset) =>
        {
            if (preset == null) AddStackableItem(items, template.ToString(), amount);
            else
            {
                var presetItems = preset.ReplaceIDs().ToList();
                presetItems.RemapRootItemId();
                _itemHelper.SetFoundInRaid(presetItems);
                items.AddRange(presetItems);
            }
        });
        return items;
    }

    public List<(MongoId Template, int Amount)> Preview(MilestoneDefinition milestone, bool varied)
    {
        var sample = new List<(MongoId, int)>();
        Roll(milestone, 1, varied, 6, (id, amount, _) => sample.Add((id, amount)));
        return sample;
    }

    private void Roll(MilestoneDefinition milestone, int times, bool varied, long limit,
        Action<MongoId, int, List<Item>?> accept)
    {
        if (milestone.reward_pool.Count == 0) return;
        var random = Random.Shared;
        var draws = Math.Min(limit, (long)times * milestone.reward_count);
        if (varied)
        {
            var sources = milestone.reward_pool.Where(e => e.weight > 0 && double.IsFinite(e.weight))
                .Select(ResolveSource).ToList();
            var parcel = new VariedParcel(sources, random);
            for (long i = 0; i < draws; i++)
                if (!parcel.Draw((entry, template) => TryRoll(entry, template, random, accept))) break;
            return;
        }

        var totalWeight = milestone.reward_pool.Sum(r => r.weight);
        if (totalWeight <= 0) return;
        for (long i = 0; i < draws; i++)
        {
            var entry = PickWeighted(milestone.reward_pool, totalWeight, random);
            if (entry == null) continue;
            if (string.IsNullOrEmpty(entry.category))
            {
                if (!string.IsNullOrEmpty(entry.template_id))
                    TryRoll(entry, new MongoId(entry.template_id), random, accept);
                continue;
            }
            var source = ResolveSource(entry);
            if (source.Templates.Count == 0)
            {
                _logger.Warning($"[StatRewards] Empty pool for category '{entry.category}'");
                continue;
            }
            int attempts = entry.category is "gear" or "weapon_preset" ? 10 : 1;
            for (int attempt = 0; attempt < attempts; attempt++)
            {
                var template = source.Templates.Count == 1 ? source.Templates[0] : _randomUtil.GetArrayValue(source.Templates);
                if (TryRoll(entry, template, random, accept)) break;
            }
        }
    }

    private RewardSource ResolveSource(RewardPoolEntry entry)
    {
        if (string.IsNullOrEmpty(entry.category))
        {
            if (entry.template_id is { } id && MongoId.IsValidMongoId(id) && _templates.Items.ContainsKey(id))
                return new(entry, _fixedGroups.GetOrAdd(id, ItemGroup), [new MongoId(id)]);
            return new(entry, "", []);
        }
        return entry.category switch
        {
            "money" => new(entry, "money", [new MongoId(RoublesTpl)]),
            "weapon_preset" => new(entry, "weapon_preset", GetCategoryPool("_weapons_with_presets")),
            _ => new(entry, entry.category, GetCategoryPool(entry.category))
        };
    }

    private string ItemGroup(MongoId template)
    {
        if (template == new MongoId(RoublesTpl)) return "money";
        if (_itemHelper.IsOfBaseclass(template, BaseClasses.WEAPON)) return "weapon_preset";
        foreach (var (category, parents) in CategoryBaseClasses)
            if (parents.Any(parent => _itemHelper.IsOfBaseclass(template, parent))) return category;
        return _templates.Items[template].Parent.ToString();
    }

    private bool TryRoll(RewardPoolEntry entry, MongoId template, Random random, Action<MongoId, int, List<Item>?> accept)
    {
        bool gear = entry.category == "gear";
        if (entry.category == "weapon_preset" || (gear && _itemHelper.ArmorItemHasRemovableOrSoftInsertSlots(template)))
        {
            var preset = _presetHelper.GetDefaultPreset(template);
            if (preset?.Items is not { Count: > 0 }) return false;
            accept(template, 1, preset.Items);
            return true;
        }

        var amount = entry.category switch
        {
            "gear" => 1,
            "ammo" => GetMaxStackSize(template.ToString()),
            _ => StackSize(entry, random)
        };
        accept(template, amount, null);
        return true;
    }

    private static int StackSize(RewardPoolEntry entry, Random random) =>
        entry.min_stack == entry.max_stack ? entry.min_stack
            : (int)random.NextInt64(entry.min_stack, (long)entry.max_stack + 1);

    private void AddStackableItem(List<Item> items, string templateId, int totalStack)
    {
        var maxStack = GetMaxStackSize(templateId);
        var remaining = totalStack;
        while (remaining > 0)
        {
            var stackSize = Math.Min(remaining, maxStack);
            items.Add(new Item
            {
                Id = new MongoId(Guid.NewGuid().ToString("N")[..24]),
                Template = new MongoId(templateId),
                Upd = new Upd { StackObjectsCount = stackSize }
            });
            remaining -= stackSize;
        }
    }

    private List<MongoId> GetCategoryPool(string category)
    {
        return _categoryPools.GetOrAdd(category, key =>
        {
            var pool = key == "_weapons_with_presets" ? BuildWeaponPresetPool() : BuildBaseClassPool(key);
            _logger.Info($"[StatRewards] Built pool for '{key}': {pool.Count} items");
            return pool;
        });
    }

    private List<MongoId> BuildBaseClassPool(string category)
    {
        if (!CategoryBaseClasses.TryGetValue(category, out var baseClassIds) || baseClassIds.Length == 0)
            return new List<MongoId>();

        var items = _templates.Items;
        var pool = new List<MongoId>();

        foreach (var kvp in items)
        {
            var template = kvp.Value;
            if (template.Type != "Item") continue;
            if (template.Properties?.QuestItem == true) continue;
            if (template.Properties?.IsUnbuyable == true) continue;

            foreach (var baseClass in baseClassIds)
            {
                if (_itemHelper.IsOfBaseclass(kvp.Key, baseClass))
                {
                    pool.Add(kvp.Key);
                    break;
                }
            }
        }

        return pool;
    }

    private List<MongoId> BuildWeaponPresetPool()
    {
        var items = _templates.Items;
        var pool = new List<MongoId>();

        foreach (var kvp in items)
        {
            var template = kvp.Value;
            if (template.Type != "Item") continue;
            if (template.Properties?.QuestItem == true) continue;
            if (template.Properties?.IsUnbuyable == true) continue;

            if (_itemHelper.IsOfBaseclass(kvp.Key, BaseClasses.WEAPON))
            {
                // Only include weapons that have a default preset
                var preset = _presetHelper.GetDefaultPreset(kvp.Key);
                if (preset?.Items != null && preset.Items.Count > 1)
                    pool.Add(kvp.Key);
            }
        }

        return pool;
    }

    private static RewardPoolEntry? PickWeighted(
        List<RewardPoolEntry> pool, double totalWeight, Random random)
    {
        var roll = random.NextDouble() * totalWeight;
        double cumulative = 0;
        foreach (var entry in pool)
        {
            cumulative += entry.weight;
            if (roll < cumulative) return entry;
        }
        return pool.Last();
    }

    private int GetMaxStackSize(string templateId)
    {
        if (_templates.Items.TryGetValue(templateId, out var template))
            return Math.Max(1, template.Properties?.StackMaxSize ?? 1);
        return 1;
    }
}
