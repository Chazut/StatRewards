using Microsoft.Extensions.Logging;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services.Items;
using SPTarkov.Server.Core.Services.Locales;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json;
using StatRewards.Services;
using Color = Spectre.Console.Color;
using Path = System.IO.Path;

namespace StatRewards.Checks;

// Read the game's item data into an isolated in-memory catalogue, without loading any player profiles.
public sealed record RuntimeFixture(RewardGenerator Generator, LocaleService Locales, TemplateTable Templates)
{
    public static RuntimeFixture Load(string root)
    {
        var db = Path.Combine(root, "SPT_Data", "database");
        var json = new JsonUtil([new SptJsonConverterRegistrator()]);
        var templates = new TemplateTable {
            Items = json.DeserializeFromFile<Dictionary<MongoId, TemplateItem>>(Path.Combine(db, "templates", "items.json"))!,
            Quests = [], Character = [], CustomisationStorage = [], Prestige = null!, RepeatableQuests = null!, Handbook = null!,
            Customization = [], Dialogue = null!, Profiles = [], Prices = [], DefaultEquipmentPresets = [],
            Achievements = [], CustomAchievements = [], LocationServices = null!
        };
        var en = json.DeserializeFromFile<GlobalLocaleDictionary>(Path.Combine(db, "locales","global","en.json"))!;
        var locales = new LocaleService(new TestLogger<LocaleService>(), new LocaleTable {
            Global = new() { ["en"] = new LazyLoad<GlobalLocaleDictionary>(() => en, true) }, Menu = [], Languages = []
        }, new LocaleConfig { GameLocale = "en", ServerLocale = "en", ServerSupportedLocales = ["en"], Fallbacks = [] });
        var bases = new ItemBaseClassService(new TestLogger<ItemBaseClassService>(), templates, null!);
        bases.HydrateItemBaseClassCache();
        var cloner = new SPTarkov.Server.Core.Utils.Cloners.FastCloner();
        var random = new RandomUtil(new TestLogger<RandomUtil>(), cloner);
        var items = new ItemHelper(new TestLogger<ItemHelper>(), templates, random, null!, bases, null!, null!, locales, cloner);
        var globals = json.DeserializeFromFile<GlobalTable>(Path.Combine(db, "globals.json"))!;
        var presets = new PresetHelper(globals, items, cloner);
        presets.GetDefaultPresets();
        new PresetController(new TestLogger<PresetController>(), globals, presets).Initialize();
        return new(new RewardGenerator(templates, items, presets, random, new TestLogger<RewardGenerator>()), locales, templates);
    }
}

public sealed class TestLogger<T> : ISptLogger<T>
{
    public void Info(string data, Exception? ex = null) { }
    public void Success(string data, Exception? ex = null) { }
    public void Debug(string data, Exception? ex = null) { }
    public void Warning(string data, Exception? ex = null) { }
    public void Error(string data, Exception? ex = null) => throw new Exception(data, ex);
    public void Critical(string data, Exception? ex = null) => throw new Exception(data, ex);
    public void LogWithColor(string data, Color? textColor = null, Color? backgroundColor = null, Exception? ex = null) { }
    public void Log(LogLevel level, string data, Color? textColor = null, Color? backgroundColor = null, Exception? ex = null) { }
    public bool IsLogEnabled(LogLevel level) => true;
}
