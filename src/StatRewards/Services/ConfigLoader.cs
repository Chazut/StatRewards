using SPTarkov.DI.Annotations;
using StatRewards.Models;

namespace StatRewards.Services;

[Injectable]
public sealed class ConfigLoader
{
    public ModConfig LoadModConfig(string path) => SettingsFile.Open(path).Config;
}
