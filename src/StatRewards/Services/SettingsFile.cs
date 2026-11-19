using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using StatRewards.Models;

namespace StatRewards.Services;

internal sealed record SettingsFile(string Source, ModConfig Config)
{
    private static readonly object Gate = new();
    internal static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };
    private static readonly JsonSerializerOptions Pretty = new() { WriteIndented = true };

    internal static ModConfig Parse(string text) =>
        string.IsNullOrWhiteSpace(text) ? new ModConfig() : JsonSerializer.Deserialize<ModConfig>(text, Json) ?? throw new JsonException("Expected a settings object.");

    internal static SettingsFile Open(string path)
    {
        var text = File.ReadAllText(path);
        return new(text, Parse(text));
    }

    internal SettingsFile Save(string path, ModConfig draft)
    {
        Validate(draft);
        lock (Gate)
        {
            if (File.ReadAllText(path) != Source)
                throw new InvalidOperationException("The file has changed. Reload saved settings before saving again.");
            var old = JsonSerializer.SerializeToNode(Config)!;
            var updated = JsonSerializer.SerializeToNode(draft)!;
            if (JsonNode.DeepEquals(old, updated)) return this;
            var bytes = Encoding.UTF8.GetBytes(string.IsNullOrWhiteSpace(Source) ? "{}" : Source);
            var reader = new Utf8JsonReader(bytes, new JsonReaderOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
            reader.Read();
            var edits = new List<(int Start, int End, string Text)>();
            CollectEdits(ref reader, old, updated, edits);
            var result = new StringBuilder();
            int position = 0;
            foreach (var e in edits.OrderBy(e => e.Start))
            {
                result.Append(Encoding.UTF8.GetString(bytes, position, e.Start - position)).Append(e.Text);
                position = e.End;
            }
            var text = result.Append(Encoding.UTF8.GetString(bytes, position, bytes.Length - position)).ToString();
            var saved = new SettingsFile(text, Parse(text));
            var tmp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try { File.WriteAllText(tmp, text); File.Move(tmp, path, true); }
            finally { File.Delete(tmp); }
            return saved;
        }
    }

    // Edit only changed tokens. This preserves comments, extension fields and the original spelling of keys.
    private static void CollectEdits(ref Utf8JsonReader reader, JsonNode? before, JsonNode? after,
        List<(int Start, int End, string Text)> edits)
    {
        if (JsonNode.DeepEquals(before, after)) { reader.Skip(); return; }
        if (before is JsonObject old && after is JsonObject next && reader.TokenType == JsonTokenType.StartObject &&
            old.All(p => next.ContainsKey(p.Key)))
        {
            int insertAt = (int)reader.BytesConsumed;
            var changed = next.Where(p => !JsonNode.DeepEquals(old[p.Key], p.Value))
                .ToDictionary(p => p.Key, p => p.Key, StringComparer.OrdinalIgnoreCase);
            bool hasFields = false;
            while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
            {
                hasFields = true;
                var key = reader.GetString()!;
                reader.Read();
                if (changed.Remove(key, out var canonical)) CollectEdits(ref reader, old[canonical], next[canonical], edits);
                else reader.Skip();
            }
            if (changed.Count > 0)
            {
                var added = string.Join(", ", changed.Values.Select(k => JsonSerializer.Serialize(k) + ": " + (next[k]?.ToJsonString() ?? "null")));
                edits.Add((insertAt, insertAt, " " + added + (hasFields ? "," : " ")));
            }
            return;
        }
        if (before is JsonArray a && after is JsonArray b && a.Count == b.Count && reader.TokenType == JsonTokenType.StartArray)
        {
            for (int i = 0; i < a.Count; i++)
            {
                reader.Read();
                CollectEdits(ref reader, a[i], b[i], edits);
            }
            reader.Read();
            return;
        }
        int start = (int)reader.TokenStartIndex;
        reader.Skip();
        edits.Add((start, (int)reader.BytesConsumed, after?.ToJsonString(after is JsonArray ? Pretty : null) ?? "null"));
    }

    internal static void Validate(ModConfig config)
    {
        if (!double.IsFinite(config.threshold_multiplier) || config.threshold_multiplier <= 0)
            throw new ArgumentException("The milestone interval multiplier must be positive.");
        foreach (var m in config.milestones)
        {
            if (m.every < 0 || m.every * config.threshold_multiplier >= long.MaxValue || m.reward_count < 0)
                throw new ArgumentException($"Check the interval and number of draws for {m.id}.");
            if (!string.IsNullOrEmpty(m.sender) && !SPTarkov.Server.Core.Models.Common.MongoId.IsValidMongoId(m.sender))
                throw new ArgumentException($"Enter a 24-character trader ID for {m.id}.");
            foreach (var e in m.reward_pool)
            {
                if (!double.IsFinite(e.weight) || e.weight < 0 || e.min_stack < 1 || e.max_stack < e.min_stack)
                    throw new ArgumentException($"Check the weight and stack range in {m.id}.");
                if (string.IsNullOrWhiteSpace(e.category) &&
                    (string.IsNullOrEmpty(e.template_id) || !SPTarkov.Server.Core.Models.Common.MongoId.IsValidMongoId(e.template_id)))
                    throw new ArgumentException($"Choose a category or enter a valid item template ID in {m.id}.");
            }
        }
    }
}
