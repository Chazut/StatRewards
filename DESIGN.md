# StatRewards — Stats & Milestones Design

## Available Profile Stats

All counters are in `characters.pmc.Stats.Eft.OverallCounters.Items[]` as `{Key: string[], Value: long}`.

### Combat — Kills

| Counter Key | Description | Value Type |
|-------------|-------------|------------|
| `["Kills"]` | Total kills (all types) | Long |
| `["KilledPmc"]` | PMC kills | Long |
| `["KilledBear"]` | Bear kills | Long |
| `["KilledUsec"]` | Usec kills | Long |
| `["KilledSavage"]` | Scav kills | Long |
| `["KilledBoss"]` | Boss kills | Long |
| `["KilledCasualSavage"]` | Casual scav kills | Long |
| `["KilledTeammate"]` | Friendly fire kills (N/A — SPT is solo) | Long |
| `["KilledBossName", "{bossId}"]` | Kills of a specific boss | Long |
| `["HeadShots"]` | Headshot kills | Long |
| `["LongShots"]` | Long-range kills (200m+) | Long |
| `["LongestKillShot"]` | Longest kill distance (m) | Float |
| `["LongestKillShotOnBot"]` | Longest bot kill distance (m) | Float |
| `["LongestShot"]` | Longest shot fired (m) | Float |
| `["LongestKillStreak"]` | Max consecutive kills in session | Long |

### Combat — Kills by Weapon Type

| Counter Key | Weapon Class |
|-------------|-------------|
| `["KilledWithKnife"]` | Melee |
| `["KilledWithPistol"]` | Pistol |
| `["KilledWithSmg"]` | SMG |
| `["KilledWithShotgun"]` | Shotgun |
| `["KilledWithAssaultRifle"]` | Assault Rifle |
| `["KilledWithAssaultCarbine"]` | Assault Carbine |
| `["KilledWithMarksmanRifle"]` | DMR |
| `["KilledWithSniperRifle"]` | Sniper Rifle |
| `["KilledWithGrenadeLauncher"]` | Grenade Launcher |
| `["KilledWithMachineGun"]` | LMG / MG |
| `["KilledWithSpecialWeapon"]` | Special Weapon |
| `["KilledWithThrowWeapon"]` | Grenade / Throwable |
| `["KilledWithTripwires"]` | Tripwire |
| `["KilledWithTemplate", "{weaponTplId}"]` | Specific weapon template |

### Combat — Kills by Victim Level

| Counter Key | Level Range |
|-------------|-------------|
| `["KilledLevel0010"]` | 0–10 |
| `["KilledLevel1030"]` | 10–30 |
| `["KilledLevel3050"]` | 30–50 |
| `["KilledLevel5070"]` | 50–70 |
| `["KilledLevel7099"]` | 70–99 |
| `["KilledLevel100"]` | 100+ |

### Combat — Damage & Shooting

| Counter Key | Description | Value Type |
|-------------|-------------|------------|
| `["CauseBodyDamage"]` | Total flesh damage dealt | Float |
| `["CauseArmorDamage"]` | Total armor damage dealt | Float |
| `["CombatDamage"]` | Total combat damage taken | Float |
| `["HitCount"]` | Total shots that hit | Long |
| `["AmmoUsed"]` | Total rounds fired | Long |
| `["AmmoReached"]` | Rounds that reached target | Long |
| `["RSPShoot"]` | Stationary weapon shots | Long |
| `["BodyPartDamage", "Head"]` | Damage to head | Float |
| `["BodyPartDamage", "Chest"]` | Damage to chest | Float |
| `["BodyPartDamage", "Stomach"]` | Damage to stomach | Float |
| `["BodyPartDamage", "LeftArm"]` | Damage to left arm | Float |
| `["BodyPartDamage", "RightArm"]` | Damage to right arm | Float |
| `["BodyPartDamage", "LeftLeg"]` | Damage to left leg | Float |
| `["BodyPartDamage", "RightLeg"]` | Damage to right leg | Float |

### Health & Physical Condition

| Counter Key | Description | Value Type |
|-------------|-------------|------------|
| `["BloodLoss"]` | Liters of blood lost | Long |
| `["Heal"]` | Total HP healed | Float |
| `["Fractures"]` | Fractures sustained | Long |
| `["Contusions"]` | Contusions sustained | Long |
| `["Dehydrations"]` | Times dehydrated | Long |
| `["Exhaustions"]` | Times exhausted | Long |
| `["Medicines"]` | Medical items used | Long |
| `["BodyPartsDestroyed"]` | Limbs destroyed (caused) | Long |

### Survival & Raids

| Counter Key | Description | Value Type |
|-------------|-------------|------------|
| `["Sessions", "Pmc"]` | Raids played (PMC) | Long |
| `["Sessions", "Scav"]` | Raids played (Scav) | Long |
| `["ExitStatus", "Survived", "Pmc"]` | Raids survived (PMC) | Long |
| `["ExitStatus", "Killed", "Pmc"]` | Raids KIA (PMC) | Long |
| `["Deaths"]` | Total deaths | Long |
| `["LifeTime", "Pmc"]` | Time in raid, seconds (PMC) | Long |
| `["LifeTime", "Scav"]` | Time in raid, seconds (Scav) | Long |
| `["CurrentWinStreak", "Pmc"]` | Current survive streak | Long |
| `["LongestWinStreak", "Pmc"]` | Best survive streak | Long |

### Movement & Consumables

| Counter Key | Description | Value Type |
|-------------|-------------|------------|
| `["Pedometer"]` | Distance walked (meters) | Long |
| `["UsedFoods"]` | Food items consumed | Long |
| `["UsedDrinks"]` | Drink items consumed | Long |

### Looting

| Counter Key | Description | Value Type |
|-------------|-------------|------------|
| `["BodiesLooted"]` | Corpses looted | Long |
| `["SafeLooted"]` | Safes opened | Long |
| `["LockableContainers"]` | Locked containers opened | Long |
| `["MobContainers"]` | Mob containers looted | Long |
| `["Weapons"]` | Weapons looted | Long |
| `["Ammunitions"]` | Ammo looted | Long |
| `["Mods"]` | Mods looted | Long |
| `["ThrowWeapons"]` | Grenades looted | Long |
| `["SpecialItems"]` | Special items looted | Long |
| `["FoodDrinks"]` | Food/Drink looted | Long |
| `["Keys"]` | Keys looted | Long |
| `["BartItems"]` | Barter items looted | Long |
| `["Equipments"]` | Equipment looted | Long |
| `["LootItem", "{templateId}"]` | Specific item looted | Long |
| `["Triggers"]` | Trigger zones visited | Long |
| `["TriggerVisited", "{triggerId}"]` | Specific trigger visit | Long |

### Economy

| Counter Key | Description | Value Type |
|-------------|-------------|------------|
| `["Money", "RUB"]` | Rubles earned | Long |
| `["Money", "EUR"]` | Euros earned | Long |
| `["Money", "USD"]` | Dollars earned | Long |

### Experience Breakdown

| Counter Key | Description |
|-------------|-------------|
| `["Exp", "ExpKill"]` | XP from kills |
| `["Exp", "ExpKill", "ExpKillBase"]` | Base kill XP |
| `["Exp", "ExpKill", "ExpKillBodyPartBonus"]` | Headshot/limb bonus XP |
| `["Exp", "ExpKill", "ExpKillStreakBonus"]` | Kill streak bonus XP |
| `["Exp", "ExpLooting"]` | XP from looting |
| `["Exp", "ExpLooting", "ExpItemLooting"]` | XP from items |
| `["Exp", "ExpLooting", "ExpContainerLooting"]` | XP from containers |
| `["Exp", "ExpLooting", "ExpTrigger"]` | XP from triggers |
| `["Exp", "ExpLooting", "ExpStationaryContainer"]` | XP from stationary containers |
| `["Exp", "ExpDamage"]` | XP from damage |
| `["Exp", "ExpHeal"]` | XP from healing |
| `["Exp", "ExpEnergy"]` | XP from energy management |
| `["Exp", "ExpHydration"]` | XP from hydration management |
| `["Exp", "ExpExitStatus"]` | XP from extraction |
| `["Exp", "ExpDoorUnlocked"]` | XP from unlocking doors |
| `["Exp", "ExpDoorBreached"]` | XP from breaching doors |
| `["Exp", "ExpExamine"]` | XP from examining items |

### Quests & Dailies

| Counter Key | Description |
|-------------|-------------|
| `["RepeatableQuest", "DailyTotalCompleteCount"]` | Total dailies completed |
| `["RepeatableQuest", "DailyVeryEasyCount"]` | Very Easy dailies |
| `["RepeatableQuest", "DailyEasyCount"]` | Easy dailies |
| `["RepeatableQuest", "DailyMediumCount"]` | Medium dailies |
| `["RepeatableQuest", "DailyHardCount"]` | Hard dailies |
| `["RepeatableQuest", "DailyVeryHardCount"]` | Very Hard dailies |
| `["RepeatableQuest", "DailyMaxCompleteStreak"]` | Best daily streak |
| `["RepeatableQuest", "DailyCurrentCompleteStreak"]` | Current daily streak |
| `["RepeatableQuest", "DailyTotalFailCount"]` | Failed dailies |
| `["RepeatableQuest", "DailyMaxFailStreak"]` | Worst fail streak |
| `["RepeatableQuest", "DailyMoneyEarned"]` | Money from dailies |
| `["RepeatableQuest", "DailyExpEarned"]` | XP from dailies |

### Misc

| Counter Key | Description |
|-------------|-------------|
| `["FenceStanding"]` | Fence reputation |
| `["ShootingRangePoints"]` | Shooting range score |
| `["UsecRaidRemainKills"]` | USEC faction remain kills |

### Direct Profile Fields (not in OverallCounters)

| Path | Description |
|------|-------------|
| `characters.pmc.Info.Level` | Player level |
| `characters.pmc.Info.Experience` | Total XP |
| `characters.pmc.Info.PrestigeLevel` | Prestige level |

---

## Milestone Ideas

### Tier 1 — Easy / Early game (rewards: basic supplies)

| Name | Stat | Every | Sender | Mail Flavor |
|------|------|-------|--------|-------------|
| Novice Hunter | `Kills` | 25 | Prapor | "Pas mal pour un bleu. Tiens, de quoi continuer." |
| First Steps | `Pedometer` | 10000 (10km) | Jaeger | "Tu connais bien la forêt maintenant." |
| Field Medic | `Medicines` | 20 | Therapist | "Un bon soldat prend soin de lui." |
| Scavenger | `BodiesLooted` | 25 | Fence | "T'as les doigts agiles, hein ?" |
| Lead Spitter | `AmmoUsed` | 1000 | Mechanic | "Tu fais tourner le business." |
| Rat Race | `BartItems` | 100 | Fence | "Beau butin. En voilà un peu en retour." |

### Tier 2 — Mid game (rewards: decent gear, mods, ammo)

| Name | Stat | Every | Sender | Mail Flavor |
|------|------|-------|--------|-------------|
| PMC Slayer | `KilledPmc` | 25 | Prapor | "Les PMC te craignent. Continue." |
| Surgeon | `Heal` | 50000 | Therapist | "Tu aurais pu être médecin..." |
| Headhunter | `HeadShots` | 25 | Peacekeeper | "Precision is an art. Well done." |
| Marathonien | `Pedometer` | 50000 (50km) | Jaeger | "Même les loups n'ont pas ton endurance." |
| Safe Cracker | `SafeLooted` | 25 | Fence | "On m'a dit que tu avais la main heureuse." |
| Iron Stomach | `UsedFoods` + `UsedDrinks` | 50 (combined) | Therapist | "Au moins tu te nourris bien." |
| Bullet Sponge | `CombatDamage` | 100000 | Therapist | "Comment es-tu encore vivant ?" |

### Tier 3 — Hard / Late game (rewards: rare items, keys, cases)

| Name | Stat | Every | Sender | Mail Flavor |
|------|------|-------|--------|-------------|
| Bloodbath | `BloodLoss` | 100 | Therapist | "Tu devrais être mort. Plusieurs fois." |
| War Machine | `Kills` | 250 | Prapor | "Tu es une légende sur le terrain." |
| Veteran | `Sessions` (Pmc) | 100 | Prapor | "Peu survivent aussi longtemps que toi." |
| Sharpshooter | `LongShots` | 25 | Peacekeeper | "Long range specialist. Impressive." |
| Bone Crusher | `BodyPartsDestroyed` | 50 | Jaeger | "La nature est cruelle. Toi aussi." |
| Boss Hunter | `KilledBoss` | 5 | Jaeger | "Même les chefs tombent devant toi." |
| Hemorrhage | `BloodLoss` | 500 | Therapist | "Je ne comprends pas ta biologie." |
| Ironman | `LongestWinStreak` (Pmc) | 10 | Prapor | "Imbattable. Voici ta récompense." |

### Tier 4 — Prestige / Fun (rewards: rare, cosmetic, unique)

| Name | Stat | Every | Sender | Mail Flavor |
|------|------|-------|--------|-------------|
| Millionaire | `Money` (RUB) | 5000000 | Ragman | "L'argent appelle l'argent, mon ami." |
| Knife Maniac | `KilledWithKnife` | 10 | Jaeger | "Un vrai sauvage." |
| Dehydration Survivor | `Dehydrations` | 10 | Therapist | "Bois de l'eau, je t'en supplie." |
| Demolition Man | `KilledWithGrenadeLauncher` + `KilledWithThrowWeapon` | 25 | Prapor | "BOOM. Joli." |
| Sniper Elite | `LongestKillShot` | 500 (meters threshold) | Peacekeeper | "New record. Uncle Sam is proud." |
| Full Auto Addict | `KilledWithMachineGun` | 25 | Mechanic | "Brrrrt. La musique de la guerre." |
| Hiking Champion | `Pedometer` | 200000 (200km) | Jaeger | "Tu connais Tarkov mieux que moi." |

---

## Reward Pool Ideas

### Pool: Supplies (Tier 1)
- Bandages, AI-2, splints
- Crackers, water, MRE
- Basic ammo (PS, FMJ)
- Low-tier barter items

### Pool: Gear (Tier 2)
- Mid-tier ammo (BP, BT, M80)
- Weapon mods (scopes, grips, suppressors)
- Meds (IFAK, morphine, SJ6)
- Mid-tier barter items

### Pool: Premium (Tier 3)
- Top-tier ammo (M61, BS, MAI AP)
- Rare keys
- Stims (ETG, Obdolbos, SJ12)
- Containers, cases
- High-value barter items

### Pool: Prestige (Tier 4)
- Labs keycards
- Rare cases (THICC, Weapons case)
- Boss loot (Killa armor, Tagilla mask, etc.)
- Ultra-rare items

---

## Config Structure (Draft)

```jsonc
{
  // Global settings
  "enabled": true,
  "checkIntervalSeconds": 0, // 0 = check on raid end only

  // Milestone definitions
  "milestones": [
    {
      "id": "novice_hunter",
      "name": "Novice Hunter",
      "stat": ["Kills"],
      "every": 25,              // repeatable every 25
      "rewardPool": "supplies",
      "sender": "Prapor",       // trader who sends the mail
      "mailSubject": "Commendation",
      "mailBody": "Pas mal pour un bleu. Tiens, de quoi continuer."
    }
  ],

  // Reward pools (weighted loot tables)
  "rewardPools": {
    "supplies": {
      "minItems": 1,
      "maxItems": 3,
      "items": [
        { "tpl": "5af0484c86f7740f02001f7f", "weight": 10, "min": 1, "max": 2 },
        { "tpl": "...", "weight": 5, "min": 1, "max": 1 }
      ]
    }
  }
}
```
