using System.Text;
using Microsoft.AspNetCore.Components;
using SPTarkov.Server.Core.Services.Locales;
using StatRewards.Load;
using StatRewards.Models;
using StatRewards.Services;

namespace StatRewards.Web;

public partial class Settings
{
    [Inject] private RewardGenerator Generator { get; set; } = null!;
    [Inject] private LocaleService Locales { get; set; } = null!;
    private static readonly string Styles = Resource("Settings.css");
    private static readonly string Icon = "data:image/svg+xml;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(Resource("Icon.svg")));
    private static readonly Dictionary<string, string> Kinds = new()
    {
        ["meds"] = "Medical supplies", ["ammo"] = "Ammunition", ["food"] = "Food", ["drink"] = "Drinks",
        ["keys"] = "Keys", ["barter"] = "Barter items", ["weapon_preset"] = "Weapon presets",
        ["gear"] = "Equipment", ["money"] = "Roubles", [""] = "Specific item"
    };
    private SettingsFile? document;
    private ModConfig? draft;
    private MilestoneDefinition? selected;
    private List<string>? sample;
    private string path = "", search = "", status = "";

    private IEnumerable<MilestoneDefinition> Filtered => draft!.milestones.Where(m =>
        Name(m).Contains(search, StringComparison.OrdinalIgnoreCase) || Stat(m).Contains(search, StringComparison.OrdinalIgnoreCase)
        || m.id.Contains(search, StringComparison.OrdinalIgnoreCase));

    protected override void OnInitialized() => Reload();

    private static string Resource(string name)
    {
        using var stream = typeof(Settings).Assembly.GetManifestResourceStream("StatRewards." + name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
    private static string Name(MilestoneDefinition m) => string.IsNullOrWhiteSpace(m.name) ? m.id : m.name;
    private static string Stat(MilestoneDefinition m) => string.IsNullOrEmpty(m.stat_subkey) ? m.stat_key : $"{m.stat_key} / {m.stat_subkey}";
    private static string Chance(MilestoneDefinition m, RewardPoolEntry e)
    {
        double max = m.reward_pool.Max(p => p.weight);
        if (max <= 0) return "0%";
        return (e.weight / max / m.reward_pool.Sum(p => p.weight / max)).ToString("0.#%");
    }
    private static string StackCaption(RewardPoolEntry e) => e.category switch
    {
        "ammo" => "Full ammo stack", "weapon_preset" or "gear" => "One equipment item",
        null or "" => "Item and quantity", _ => e.min_stack == e.max_stack ? $"Quantity · {e.min_stack:N0}" : $"Quantity · {e.min_stack:N0}–{e.max_stack:N0}"
    };

    private void Reload()
    {
        try
        {
            path = PreSpt.FindConfigPath();
            document = SettingsFile.Open(path);
            draft = SettingsFile.Parse(document.Source);
            Select(draft.milestones.FirstOrDefault(m => m.id == selected?.id) ?? draft.milestones.FirstOrDefault());
            status = "";
        }
        catch (Exception ex) { draft = null; status = $"Could not load settings. {ex.Message}"; }
    }

    private void Select(MilestoneDefinition? m) { selected = m; ClearSample(); }
    private void ClearSample() { sample = null; status = ""; }
    private void AddReward() { selected!.reward_pool.Add(new() { category = "meds" }); ClearSample(); }
    private void RemoveReward(int index) { selected!.reward_pool.RemoveAt(index); ClearSample(); }
    private void SetKind(RewardPoolEntry entry, string? category)
    {
        entry.category = string.IsNullOrEmpty(category) ? null : category;
        entry.template_id = null;
        ClearSample();
    }
    private void Sample()
    {
        if (selected == null || draft == null) return;
        try
        {
            SettingsFile.Validate(draft);
            var names = Locales.GetLocaleDb("en");
            sample = Generator.Preview(selected, draft.varied_parcels)
                .Select(item => $"{names.GetValueOrDefault(item.Template + " Name", item.Template.ToString())} × {item.Amount:N0}").ToList();
            status = sample.Count == 0 ? "No available items in this pool." : "";
        }
        catch (Exception ex) { sample = null; status = ex.Message; }
    }

    private void Save()
    {
        if (document == null || draft == null) return;
        try { document = document.Save(path, draft); status = "Settings saved."; }
        catch (Exception ex) { status = ex.Message; }
    }
}
