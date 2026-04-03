using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using StatRewards.Models;

namespace StatRewards.Services;

[Injectable]
public sealed class RewardGenerator
{
    private readonly DatabaseService _db;
    private readonly ItemHelper _itemHelper;
    private readonly PresetHelper _presetHelper;
    private readonly RandomUtil _randomUtil;
    private readonly ISptLogger<RewardGenerator> _logger;

    // Cached category pools (lazy-loaded)
    private Dictionary<string, List<MongoId>>? _categoryPools;

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
        DatabaseService db,
        ItemHelper itemHelper,
        PresetHelper presetHelper,
        RandomUtil randomUtil,
        ISptLogger<RewardGenerator> logger)
    {
        _db = db;
        _itemHelper = itemHelper;
        _presetHelper = presetHelper;
        _randomUtil = randomUtil;
        _logger = logger;
    }

    public List<Item> Generate(MilestoneDefinition milestone, int times = 1)
    {
        var items = new List<Item>();
        if (milestone.reward_pool.Count == 0) return items;

        var random = new Random();
        var totalWeight = milestone.reward_pool.Sum(r => r.weight);
        if (totalWeight <= 0) return items;

        for (int t = 0; t < times; t++)
        {
            for (int i = 0; i < milestone.reward_count; i++)
            {
                var selected = PickWeighted(milestone.reward_pool, totalWeight, random);
                if (selected == null) continue;

                if (!string.IsNullOrEmpty(selected.category))
                    GenerateCategoryReward(items, selected, random);
                else if (!string.IsNullOrEmpty(selected.template_id))
                    GenerateSpecificReward(items, selected, random);
            }
        }
        return items;
    }

    private void GenerateCategoryReward(List<Item> items, RewardPoolEntry entry, Random random)
    {
        var category = entry.category!;

        // Special case: money
        if (category == "money")
        {
            var amount = entry.min_stack == entry.max_stack
                ? entry.min_stack
                : random.Next(entry.min_stack, entry.max_stack + 1);
            AddStackableItem(items, RoublesTpl, amount);
            return;
        }

        // Special case: weapon with preset
        if (category == "weapon_preset")
        {
            GenerateWeaponPresetReward(items);
            return;
        }

        // Special case: gear — use preset for armor with plates/inserts
        if (category == "gear")
        {
            GenerateGearReward(items);
            return;
        }

        // Standard category: pick random item from pool
        var pool = GetCategoryPool(category);
        if (pool.Count == 0)
        {
            _logger.Warning($"[StatRewards] Empty pool for category '{category}'");
            return;
        }

        var tpl = _randomUtil.GetArrayValue(pool);

        // Ammo: always give a full stack
        if (category == "ammo")
        {
            var maxStack = GetMaxStackSize(tpl.ToString());
            AddStackableItem(items, tpl.ToString(), maxStack);
            return;
        }

        var stack = entry.min_stack == entry.max_stack
            ? entry.min_stack
            : random.Next(entry.min_stack, entry.max_stack + 1);

        AddStackableItem(items, tpl.ToString(), stack);
    }

    private void GenerateWeaponPresetReward(List<Item> items)
    {
        // Build weapon pool if needed, then pick a random weapon that has a default preset
        var weaponPool = GetCategoryPool("_weapons_with_presets");
        if (weaponPool.Count == 0)
        {
            _logger.Warning("[StatRewards] No weapons with presets found");
            return;
        }

        // Try up to 10 times to find a weapon with a valid preset
        for (int attempt = 0; attempt < 10; attempt++)
        {
            var weaponTpl = _randomUtil.GetArrayValue(weaponPool);
            var preset = _presetHelper.GetDefaultPreset(weaponTpl);
            if (preset?.Items == null || preset.Items.Count == 0) continue;

            var presetItems = preset.Items.ReplaceIDs().ToList();
            presetItems.RemapRootItemId();
            _itemHelper.SetFoundInRaid(presetItems);
            items.AddRange(presetItems);
            return;
        }
    }

    private void GenerateGearReward(List<Item> items)
    {
        var pool = GetCategoryPool("gear");
        if (pool.Count == 0) return;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            var tpl = _randomUtil.GetArrayValue(pool);

            // Check if armor needs plates/inserts — use preset if available
            if (_itemHelper.ArmorItemHasRemovableOrSoftInsertSlots(tpl))
            {
                var preset = _presetHelper.GetDefaultPreset(tpl);
                if (preset?.Items == null || preset.Items.Count == 0)
                {
                    // No preset for this armor, try another
                    continue;
                }

                var presetItems = preset.Items.ReplaceIDs().ToList();
                presetItems.RemapRootItemId();
                _itemHelper.SetFoundInRaid(presetItems);
                items.AddRange(presetItems);
                return;
            }

            // Non-modular gear (backpack, headwear, simple rig) — just add it
            items.Add(new Item
            {
                Id = new MongoId(Guid.NewGuid().ToString("N")[..24]),
                Template = tpl,
                Upd = new Upd { StackObjectsCount = 1 }
            });
            return;
        }
    }

    private void GenerateSpecificReward(List<Item> items, RewardPoolEntry entry, Random random)
    {
        var stack = entry.min_stack == entry.max_stack
            ? entry.min_stack
            : random.Next(entry.min_stack, entry.max_stack + 1);

        AddStackableItem(items, entry.template_id!, stack);
    }

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
        _categoryPools ??= new Dictionary<string, List<MongoId>>();

        if (_categoryPools.TryGetValue(category, out var cached))
            return cached;

        var pool = category == "_weapons_with_presets"
            ? BuildWeaponPresetPool()
            : BuildBaseClassPool(category);

        _categoryPools[category] = pool;
        _logger.Info($"[StatRewards] Built pool for '{category}': {pool.Count} items");
        return pool;
    }

    private List<MongoId> BuildBaseClassPool(string category)
    {
        if (!CategoryBaseClasses.TryGetValue(category, out var baseClassIds) || baseClassIds.Length == 0)
            return new List<MongoId>();

        var items = _db.GetItems();
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
        var items = _db.GetItems();
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
        var tables = _db.GetTables();
        if (tables?.Templates?.Items?.TryGetValue(templateId, out var template) == true)
            return template.Properties?.StackMaxSize ?? 1;
        return 1;
    }
}
