using SPTarkov.DI.Annotations;
using StatRewards.Models;

namespace StatRewards.Services;

[Injectable]
public sealed class State
{
    public ModConfig Config { get; private set; } = new();
    public bool IsLoaded { get; private set; }

    public void Set(ModConfig config)
    {
        Config = config;
        IsLoaded = true;
    }
}
