using SPTarkov.Server.Core.Models.Spt.Mod;
using SemVerVersion = SemanticVersioning.Version;
using SemVerRange = SemanticVersioning.Range;

namespace StatRewards.Mod;

public sealed record StatRewardsMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.chazut.statrewards";
    public override string Name { get; init; } = "StatRewards";
    public override string Author { get; init; } = "Chazut";
    public override List<string>? Contributors { get; init; } = null;
    public override SemVerVersion Version { get; init; } = new("1.1.1");
    public override SemVerRange SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; } = null;
    public override Dictionary<string, SemVerRange>? ModDependencies { get; init; } = null;
    public override string Url { get; init; } = "https://github.com/Chazut/StatRewards";
    public override bool? IsBundleMod { get; init; } = false;
    public override string? License { get; init; } = "MIT";
}
