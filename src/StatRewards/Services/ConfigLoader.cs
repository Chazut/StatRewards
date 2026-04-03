using System.Text.Json;
using SPTarkov.DI.Annotations;
using StatRewards.Models;
using StatRewards.Utils;

namespace StatRewards.Services;

[Injectable]
public sealed class ConfigLoader
{
    private readonly JsonSerializerOptions _opts = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public ModConfig LoadModConfig(string path)
    {
        var raw = File.ReadAllText(path);
        var json = Jsonc.Strip(raw);
        return JsonSerializer.Deserialize<ModConfig>(json, _opts) ?? new ModConfig();
    }
}
