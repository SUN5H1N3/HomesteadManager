using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Core.Commands;

public class PlaytimeCommand(IConfiguration config, ILogger<PlaytimeCommand> logger) : ICommand {
    public string Name => "playtime";

    private readonly string _statsPath = config["StatsPath"]!;
    private readonly string _cachePath = config["PlayerUuidsPath"]!;

    public void Execute(string[] args) {
        var cache = LoadCache();

        var players = Directory.GetFiles(_statsPath, "*.json")
            .Select(file => {
                var uuid = Path.GetFileNameWithoutExtension(file).Replace("-", "");
                var json = JsonNode.Parse(File.ReadAllText(file));
                var ticks = json?["stats"]?["minecraft:custom"]?["minecraft:play_time"]?.GetValue<long>() ?? 0;
                var hours = Math.Round(ticks / 20.0 / 3600.0, 2);
                var name = ResolveUsername(uuid, cache);
                return new { Player = name, Hours = hours };
            })
            .OrderByDescending(p => p.Hours)
            .ToList();

        SaveCache(cache);

        var table = new Table();
        table.AddColumn("Player");
        table.AddColumn(new TableColumn("Hours").RightAligned());
        table.Border(TableBorder.Rounded);

        foreach (var p in players) {
            table.AddRow(p.Player, p.Hours.ToString(CultureInfo.InvariantCulture));
        }

        AnsiConsole.Write(table);
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

public class CacheEntry {
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = "";

    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }
}