# StatRewards 1.2.0

## SPT 4.1 support

StatRewards now runs on SPT 4.1.x (built against 4.1.5 and the .NET 10 server). Staying on 4.0.x? Keep using 1.1.1.

SPT 4.1.5 turned the profile list route into a streamed one, which broke every profile load with an UNHANDLED RESPONSE when the milestone hook was in the chain. The hook is now streamed as well, so it works whichever mod's router runs last.

## Milestone rebalance

Some milestones were firing way too often and others could never fire at all. Every threshold has been retuned against real long-term playthrough data so rewards land at a comparable pace across the board:

- **equipment_collector** was the worst offender (rewarding roughly 10x more often than the average milestone) — now in line with the rest
- Over-eager milestones toned down: body_wrecker, scav_raider, scavenger, headhunter, armor_breaker, rat_king, accuracy_king and friends
- Nearly-unreachable ones are now actually attainable: millionaire (500k instead of 1M roubles), dollar/euro milestones, marksman, shotgun_surgeon, grenadier
- **door_kicker and locksmith removed**: the game never increments their counters, so they could never trigger — they were pure noise in the milestone list
- Knife and grenade-launcher milestones stay as deliberate challenges, with lowered thresholds

The `threshold_multiplier` config option still lets you scale everything to taste.

## Random loot addon

`addons/random_loot/mod_config.jsonc` is an alternative config where every milestone draws 3 random items from one universal pool. Replace `config/mod_config.jsonc` with it if you want that flavour.

## Install

Extract the zip into your SPT root folder, it lands in `SPT_Runtime/user/mods/StatRewards`. Your existing config and progress files carry over.
