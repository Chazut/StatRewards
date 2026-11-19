# StatRewards

<img src="branding/stat-rewards.png" alt="StatRewards orbital supply parcel" width="88" />

![Stars](https://img.shields.io/github/stars/Chazut/StatRewards?style=flat-square&label=STARS&color=007ec6)
![Issues](https://img.shields.io/github/issues/Chazut/StatRewards?style=flat-square&label=ISSUES&color=44cc11)
![Downloads](https://img.shields.io/github/downloads/Chazut/StatRewards/total?style=flat-square&label=DOWNLOADS&color=44cc11)

StatRewards gives your raid stats a payoff. Traders send loot by in-game mail when you reach repeatable milestones. The default setup includes **46 milestones**, from surviving raids and killing bosses to treating injuries and looting bodies.

## Installation

1. Extract the [release ZIP](https://github.com/Chazut/StatRewards/releases) into your SPT 4.1.x game folder, next to `EscapeFromTarkov.exe`. The mod runs on the server.
2. Start the server and load your profile. StatRewards records the thresholds you've already reached. Future thresholds earn rewards.
3. Send any message to **StatRewards** in your in-game friends list to see your 10 closest milestones and how far you are from each one.

When upgrading, keep `config/mod_config.jsonc` and `progress/` inside `SPT_Runtime/user/mods/StatRewards/`. The file holds your settings. The folder records the last threshold reached for each milestone on each profile.

## When a trader sends a parcel

StatRewards checks your stats when you return to the menu after a raid. These examples use the default settings.

| Milestone | Repeats every | Sender | Parcel |
| --- | --- | --- | --- |
| Survivor | 15 PMC raids survived | Prapor | 2 draws from roubles, barter items and weapons |
| Boss Hunter | 3 boss kills | Jaeger | 3 draws from weapons, gear, keys and roubles |
| Die Hard | 20 deaths | Therapist | 2 draws from meds, gear and roubles |

For example, if you install with 27 PMC raids survived, your next **Survivor** parcel is due at 30, then 45. Crossing several thresholds for the same milestone between checks puts those rewards into one trader message.

## How loot is picked

Each milestone has a weighted pool and a number of draws per payout. With **Varied parcels** on by default, a parcel prefers categories it hasn't used yet and avoids repeating an item while other items are available. For example, a three-draw pool with meds, ammo and roubles gives one of each when all three are available. A pool containing only meds can still give several different medical items.

Weights choose between eligible entries. Turning **Varied parcels** off makes each draw independent. The weapon category gives guns with their default presets, and the ammo category gives full stacks.

## Configure rewards

Open **StatRewards** in the SPT web menu as an administrator, or visit `/statrewards` on your server. Search by milestone name or stat, then choose what to change.

| Setting | What it controls |
| --- | --- |
| **Milestone interval multiplier** | Changes reward frequency across the board. At `2`, Survivor takes 30 successful raids instead of 15. |
| **Base interval** | Changes how often one milestone pays out. Set it to `0` to pause that milestone. |
| **Draws and reward pool** | Choose how many draws go in a parcel, then set categories, specific items, quantities and weights. |
| **Milestone name and trader mail** | Choose the sender and write the message the player receives. |

Choose **Preview parcel** to sample up to six draws using your server's item database. Save your settings and restart the server to apply them.

### Custom milestones and optional preset

To add a milestone, copy an entry in the [default configuration](config/mod_config.jsonc) and give it a unique `id`. The ID is how StatRewards tracks that milestone across sessions. Use `stat_key` to select a profile counter and `stat_subkey` for a qualifier, such as `ExitStatus` with `Survived`. The `divisor` converts the raw counter into the units you want to reward. See the [profile stat reference](DESIGN.md#available-profile-stats).

The ZIP also includes a [random loot preset](addons/random_loot/mod_config.jsonc). Its 48 milestones each draw three times from the same pool. To use it, back up your active config, copy the preset over `config/mod_config.jsonc`, then restart the server.

## Build and test

From the repository root, use the .NET 10 SDK and point `$spt` at your SPT runtime. The empty deployment path keeps the build in the repository.

```powershell
$spt = "C:/Games/SPT-4.1/SPT_Runtime"
dotnet build src/StatRewards/StatRewards.csproj -c Release "-p:SPT_DIR=$spt" -p:ModDeployDir=
dotnet run --project tests/StatRewards.Checks/StatRewards.Checks.csproj -c Release "-p:SPT_DIR=$spt" -p:ModDeployDir= -- $spt
```

[Release notes](RELEASE_NOTES_v1.3.0.md) · [Report an issue](https://github.com/Chazut/StatRewards/issues) · [MIT license](LICENSE)
