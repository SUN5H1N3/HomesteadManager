using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Services;

public class PlayerStatsService(IConfiguration config, ILogger<PlayerStatsService> logger) : IPlayerStatsService {
    private readonly string _statsPath = config["StatsPath"]!;
    private readonly string _cachePath = config["PlayerUuidsPath"]!;

    public IReadOnlyList<PlayerStats> Collect() {
        var cache = LoadCache();

        var players = Directory.GetFiles(_statsPath, "*.json")
            .Select(file => {
                var uuid = Path.GetFileNameWithoutExtension(file).Replace("-", "");
                var json = JsonNode.Parse(File.ReadAllText(file));
                var custom = json?["stats"]?["minecraft:custom"]?.AsObject();
                var ticks = custom?["minecraft:play_time"]?.GetValue<long>() ?? 0;
                var hours = Math.Round(ticks / 20.0 / 3600.0, 2);
                var mined = json?["stats"]?["minecraft:mined"]?.AsObject();
                var blocksMined = mined?.Sum(kvp => kvp.Value?.GetValue<long>() ?? 0) ?? 0;
                var used = json?["stats"]?["minecraft:used"]?.AsObject();
                var itemsUsed = used?.Sum(kvp => kvp.Value?.GetValue<long>() ?? 0) ?? 0;
                var movement = ExtractMovement(custom);

                var damageDealt = Math.Round((custom?["minecraft:damage_dealt"]?.GetValue<long>() ?? 0) / 10.0, 1);
                var damageTaken = Math.Round((custom?["minecraft:damage_taken"]?.GetValue<long>() ?? 0) / 10.0, 1);

                var name = ResolveUsername(uuid, cache);
                return new PlayerStats(name, hours, blocksMined, itemsUsed, movement, damageDealt, damageTaken);
            })
            .ToList();

        SaveCache(cache);

        return players;
    }

    private static IReadOnlyDictionary<string, long> ExtractMovement(JsonObject? custom) {
        var result = new Dictionary<string, long>();
        if (custom == null) return result;

        const string prefix = "minecraft:";
        const string suffix = "_one_cm";

        foreach (var kvp in custom) {
            if (!kvp.Key.StartsWith(prefix) || !kvp.Key.EndsWith(suffix)) continue;
            var type = kvp.Key[prefix.Length..^suffix.Length];
            result[type] = kvp.Value?.GetValue<long>() ?? 0;
        }

        return result;
    }

    private string ResolveUsername(string uuid, List<CacheEntry> cache) {
        var entry = cache.FirstOrDefault(e => e.Uuid == uuid);

        if (entry?.Nickname != null) {
            return entry.Nickname;
        }

        try {
            var process = new System.Diagnostics.Process {
                StartInfo = new System.Diagnostics.ProcessStartInfo {
                    FileName = "curl",
                    Arguments = $"-s https://api.mojang.com/user/profile/{uuid}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            var body = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var json = JsonNode.Parse(body);
            var name = json?["name"]?.GetValue<string>();

            if (name == null) {
                throw new Exception("No name in response");
            }

            if (entry != null) {
                entry.Nickname = name;
            } else {
                cache.Add(new CacheEntry { Uuid = uuid, Nickname = name });
            }

            return name;
        } catch (Exception e) {
            logger.LogWarning("Failed to resolve username for {Uuid}: {Error}", uuid, e.Message);

            if (entry == null) {
                cache.Add(new CacheEntry { Uuid = uuid });
            }

            return uuid;
        }
    }

    private List<CacheEntry> LoadCache() {
        if (!File.Exists(_cachePath)) {
            return new List<CacheEntry>();
        }

        var json = File.ReadAllText(_cachePath);
        return JsonSerializer.Deserialize<List<CacheEntry>>(json) ?? new List<CacheEntry>();
    }

    private void SaveCache(List<CacheEntry> cache) {
        var json = JsonSerializer.Serialize(cache, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_cachePath, json);
    }
}

internal class CacheEntry {
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = "";

    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }
}
