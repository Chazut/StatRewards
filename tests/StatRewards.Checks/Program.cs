using SPTarkov.Server.Core.Models.Common;
using StatRewards.Checks;
using StatRewards.Models;
using StatRewards.Services;
using StatRewards.Mod;

int checks = 0;
void Check(bool value, string name)
{
    if (!value) throw new Exception(name);
    checks++;
}

MongoId A = "aaaaaaaaaaaaaaaaaaaaaaaa", B = "bbbbbbbbbbbbbbbbbbbbbbbb", C = "cccccccccccccccccccccccc";
RewardSource Source(string group, double weight, params MongoId[] ids) => new(new() { category=group, weight=weight }, group, ids);
List<(string?, MongoId)> Draw(VariedParcel parcel, int n)
{
    var list = new List<(string?, MongoId)>();
    for (int i=0; i<n; i++)
        if (!parcel.Draw((e, id) => { list.Add((e.category, id)); return true; })) break;
    return list;
}

for (int seed = 0; seed < 32; seed++)
{
    var parcel = new VariedParcel([Source("meds",100,A,B), Source("meds",20,A), Source("ammo",1,C)],new(seed));
    var draws = Draw(parcel, 6);
    Check(draws[0].Item1 != draws[1].Item1, "Prefer an unused category");
    Check(draws.Take(3).Select(x => x.Item2).Distinct().Count() == 3, "Unique actual items, including overlapping entries");
    Check(draws.Skip(3).Select(x => x.Item2).Distinct().Count() == 3, "Refill after exhaustion");
}
var exhausted = new VariedParcel([Source("gear",1,A), Source("meds",1,B)],new(1));
Check(exhausted.Draw((_,id) => id == B) && exhausted.Draw((_,id) => id == B), "Unavailable preset yields to viable rewards");
Check(!new VariedParcel([Source("gear",1,A)],new(1)).Draw((_,_)=>false), "Fully unavailable pool terminates");
Check(Draw(new VariedParcel([Source("gear",1,A)],new(1)),4).Count == 4, "One-item pool fills every draw");
var rng = new Random(28);
var counts = Enumerable.Range(0,10000).Count(_ => Draw(new VariedParcel([Source("meds",3,A,B),Source("ammo",1,C)],rng),1)[0].Item1 == "meds");
Check(counts is > 7250 and < 7750, "First draw respects weights and is independent of category size");

string config = Path.GetFullPath("config/mod_config.jsonc");
var fixture = RuntimeFixture.Load(args[0]);
foreach (var cfg in new[] { config,Path.GetFullPath("addons/random_loot/mod_config.jsonc") })
{
    var file = SettingsFile.Open(cfg);
    SettingsFile.Validate(file.Config);
    foreach (var milestone in file.Config.milestones)
    {
        var preview = fixture.Generator.Preview(milestone, true);
        Check(preview.Count == Math.Min(6,milestone.reward_count), milestone.id + " real database preview");
        Check(preview.All(p => fixture.Templates.Items.ContainsKey(p.Template) && p.Amount >= 1), milestone.id + " real templates");
    }
}
var fixedCash = new RewardPoolEntry {template_id="5449016a4bdc2d6f028b456f", min_stack=10,max_stack=10};
var ammo = new RewardPoolEntry {category="ammo"};
var cash = new RewardPoolEntry {category="money",min_stack=10,max_stack=10};
var mail = new MilestoneDefinition { reward_count=2, reward_pool=[fixedCash,cash,ammo] };
for(int i=0;i<12;i++)
    Check(fixture.Generator.Generate(mail,1,true).Select(it=>it.Template).Distinct().Count()==2,"Fixed cash and money share a group");
var guns = fixture.Generator.Generate(new() { reward_count=2, reward_pool=[new() {category="weapon_preset"}] });
var roots = guns.Where(i=>string.IsNullOrEmpty(i.ParentId)).ToList();
Check(roots.Count==2 && roots.Select(i=>i.Template).Distinct().Count()==2 && guns.Count > 2,"Weapon presets include attachments and distinct roots");
var secondParcel = fixture.Generator.Generate(new() { reward_count=2, reward_pool=[new() {category="weapon_preset"}] });
Check(!guns.Select(i=>i.Id).Intersect(secondParcel.Select(i=>i.Id)).Any(), "Preset item IDs are unique across parcels");
var medIds = fixture.Generator.Preview(new() {reward_count=2, reward_pool=[new() {category="meds"}]},true);
var fixedMeds = medIds.Select(x => new RewardPoolEntry {template_id=x.Template.ToString()}).ToList();
var mixedFixed = new MilestoneDefinition {reward_count=2, reward_pool=[..fixedMeds,fixedCash]};
for (int i = 0; i < 12; i++)
    Check(fixture.Generator.Preview(mixedFixed,true).Any(x=>x.Template.ToString()==fixedCash.template_id),"Fixed medical items share their real category");
var stacks = fixture.Generator.Generate(new() {reward_count=1,reward_pool=[ammo]});
Check(stacks.Single().Upd?.StackObjectsCount == fixture.Templates.Items[stacks[0].Template].Properties!.StackMaxSize,"Ammo is still a full stack");
Check(fixture.Generator.Generate(new() {reward_count=2,reward_pool=[fixedCash]},2,false).Count==4,"Classic mode and combined triggers");
Check(new StatRewardsMetadata().HomePage == "/statrewards", "SPT landing page");
Check(SettingsFile.Parse("{ \"debug\": true }").varied_parcels,"Legacy configs default to variety");

var checker = new StatChecker(new TestLogger<StatChecker>());
var rule = new MilestoneDefinition {id="test", stat_key="Kills", every=75};
var cfgForChecker = new ModConfig {milestones=[rule]};
var pmc = new SPTarkov.Server.Core.Models.Eft.Common.PmcData {
    Stats = new() {Eft = new() {OverallCounters = new() {Items = [new() {Key=["Kills"],Value=150}]}}}
};
var progress = new MilestoneProgress();
Check(checker.Check(cfgForChecker,progress,pmc,false).Count==0 && progress.milestones["test"]==150, "First check seeds progress");
progress.milestones["test"]=100;
Check(checker.Check(cfgForChecker,progress,pmc,false).Single().timesTriggered==1, "A changed interval gives the next crossed threshold");
Check(checker.Check(cfgForChecker,progress,pmc,false).Count==0, "Repeated check does not deliver the parcel twice");

var temp=Directory.CreateTempSubdirectory();
try
{
    var path=Path.Combine(temp.FullName,"test.jsonc");
    File.WriteAllText(path,"{ // root note\n \"DEBUG\":false, \"milestones\":[{\"id\":\"a\",\"custom\":42, \"reward_pool\":[{\"category\":\"gear\",\"weight\":3, /* weight */ \"future\":\"https://example.org\"}]}],}");
    var original=SettingsFile.Open(path);
    var edit=SettingsFile.Parse(original.Source);
    edit.debug=true;
    edit.milestones[0].reward_pool[0].weight=5;
    edit.milestones[0].message="Hello // PMC";
    var saved=original.Save(path,edit);
    Check(saved.Source.Contains("/* weight */") && saved.Source.Contains("// root note") && saved.Source.Contains("\"DEBUG\":true"),"Preserve JSONC during scalar edits");
    Check(saved.Config.milestones[0].message == "Hello // PMC","URLs and comment markers in strings");
    edit.milestones[0].reward_pool.Add(ammo);
    saved=saved.Save(path,edit);
    Check(saved.Source.Contains("future") && saved.Source.Contains("https://example.org") && saved.Source.Contains("\"custom\":42"),"Add pool entry while retaining extension fields");
    bool conflict=false;
    try { original.Save(path,edit); } catch(InvalidOperationException) { conflict=true; }
    Check(conflict,"Stale tab cannot overwrite another save");
    edit.milestones[0].reward_pool.RemoveAt(0);
    edit.milestones[0].reward_pool.Add(fixedCash);
    saved=saved.Save(path,edit);
    Check(!saved.Source.Contains("future") && saved.Source.Contains("\"custom\":42"),"Replacing a reward removes only its own extension fields");
    edit.milestones[0].reward_pool[0].weight=-1;
    bool invalid=false;
    try { saved.Save(path,edit); } catch(ArgumentException) { invalid=true; }
    Check(invalid && File.ReadAllText(path)==saved.Source,"Invalid settings keep last valid file");
    File.WriteAllText(path, "  ");
    var blank=SettingsFile.Open(path);
    var changedBlank=new ModConfig {debug=true, milestones=[new() {every=0, reward_pool=[]}]};
    Check(blank.Save(path,changedBlank).Config.debug, "Blank legacy settings can be edited and paused milestones saved");
}
finally { temp.Delete(true); }
Console.WriteLine($"{checks} StatRewards checks passed.");
