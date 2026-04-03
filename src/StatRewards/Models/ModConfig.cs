namespace StatRewards.Models;

public record ModConfig
{
    public bool debug { get; init; } = false;
    public double threshold_multiplier { get; init; } = 1.0; // Global multiplier applied to all milestone "every" values
    public List<MilestoneDefinition> milestones { get; init; } = new();
}

public record MilestoneDefinition
{
    public string id { get; init; } = "";
    public string stat_key { get; init; } = "";
    public string? stat_subkey { get; init; }
    public int every { get; init; } = 25;
    public int divisor { get; init; } = 1; // Set to 100 for Float-type counters (CauseArmorDamage, Heal, BloodLoss, etc.)
    public string sender { get; init; } = "54cb50c76803fa8b248b4571"; // Prapor
    public string message { get; init; } = "Congratulations on your milestone!";
    public int reward_count { get; init; } = 1;
    public List<RewardPoolEntry> reward_pool { get; init; } = new();
}

public record RewardPoolEntry
{
    // Either a specific item template_id OR a category name (not both)
    public string? template_id { get; init; }
    public string? category { get; init; }
    // Supported categories:
    //   "meds"           — medkits, drugs, stimulators
    //   "ammo"           — all ammunition
    //   "food"           — food items
    //   "drink"          — drink items
    //   "keys"           — mechanical keys (no keycards)
    //   "barter"         — barter items
    //   "weapon_preset"  — weapon with default preset (mods/attachments)
    //   "gear"           — equipment (armor, rigs, helmets, backpacks)
    //   "money"          — roubles (use min_stack/max_stack for amount)

    public int min_stack { get; init; } = 1;
    public int max_stack { get; init; } = 1;
    public double weight { get; init; } = 1.0;
}
