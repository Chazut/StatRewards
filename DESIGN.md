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
| `["BloodLoss"]` | Liters of blood lost | Float |
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

