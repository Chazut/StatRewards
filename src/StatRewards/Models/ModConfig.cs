namespace StatRewards.Models;

public record ModConfig
{
    public bool varied_parcels { get; set; } = true;
    public bool debug { get; set; } = false;
    public double threshold_multiplier { get; set; } = 1.0; // Global multiplier applied to all milestone "every" values
    public List<MilestoneDefinition> milestones { get; set; } = new();
}

public record MilestoneDefinition
{
    public long Interval(double multiplier) => Math.Max(1, (long)(every * multiplier));
    public string id { get; set; } = "";
    public string name { get; set; } = ""; // Display name shown in mail (e.g. "Novice Hunter")
    public string stat_key { get; set; } = "";
    public string? stat_subkey { get; set; }
    public int every { get; set; } = 25;
    public int divisor { get; set; } = 1; // Set to 100 for Float-type counters (CauseArmorDamage, Heal, BloodLoss, etc.)
    public string sender { get; set; } = "54cb50c76803fa8b248b4571"; // Prapor
    public string message { get; set; } = "Congratulations on your milestone!";
    public int reward_count { get; set; } = 1;
    public List<RewardPoolEntry> reward_pool { get; set; } = new();
}

public record RewardPoolEntry
{
    [System.Text.Json.Serialization.JsonExtensionData]
    public Dictionary<string, System.Text.Json.JsonElement>? Extensions { get; set; }

    // Either a specific item template_id OR a category name (not both)
    public string? template_id { get; set; }
    public string? category { get; set; }
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

    public int min_stack { get; set; } = 1;
    public int max_stack { get; set; } = 1;
    public double weight { get; set; } = 1.0;
}
