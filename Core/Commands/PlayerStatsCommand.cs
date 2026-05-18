using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Core.Commands;

public class PlayerStatsCommand(IConfiguration config, ILogger<PlayerStatsCommand> logger) : ICommand {
    public string Name => "player-stats";
    public string Description => "Show player statistics";

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
                var mined = json?["stats"]?["minecraft:mined"]?.AsObject();
                var blocksMined = mined?.Sum(kvp => kvp.Value?.GetValue<long>() ?? 0) ?? 0;
                var used = json?["stats"]?["minecraft:used"]?.AsObject();
                var itemsUsed = used?.Sum(kvp => kvp.Value?.GetValue<long>() ?? 0) ?? 0;
                var name = ResolveUsername(uuid, cache);
                return new { Player = name, Hours = hours, BlocksMined = blocksMined, ItemsUsed = itemsUsed };
            })
            .OrderByDescending(p => p.Hours)
            .ToList();

        SaveCache(cache);

        var totalHours = players.Sum(p => p.Hours);
        var totalMined = players.Sum(p => p.BlocksMined);
        var totalUsed = players.Sum(p => p.ItemsUsed);

        var table = new Table();
        table.AddColumn("Player");
        table.AddColumn(new TableColumn("Hours").RightAligned());
        table.AddColumn(new TableColumn("Blocks Mined").RightAligned());
        table.AddColumn(new TableColumn("Items Used").RightAligned());
        table.Border(TableBorder.Rounded);

        foreach (var p in players) {
            var hoursShare = totalHours > 0 ? p.Hours / totalHours * 100 : 0;
            var minedShare = totalMined > 0 ? (double)p.BlocksMined / totalMined * 100 : 0;
            var usedShare = totalUsed > 0 ? (double)p.ItemsUsed / totalUsed * 100 : 0;
            table.AddRow(
                p.Player,
                $"{p.Hours.ToString(CultureInfo.InvariantCulture)} ({hoursShare.ToString("F1", CultureInfo.InvariantCulture)}%)",
                $"{p.BlocksMined.ToString("N0", CultureInfo.InvariantCulture)} ({minedShare.ToString("F1", CultureInfo.InvariantCulture)}%)",
                $"{p.ItemsUsed.ToString("N0", CultureInfo.InvariantCulture)} ({usedShare.ToString("F1", CultureInfo.InvariantCulture)}%)"
            );
        }

        table.AddEmptyRow();
        table.AddRow(
            "[bold]Total[/]",
            $"[bold]{Math.Round(totalHours, 2).ToString(CultureInfo.InvariantCulture)}[/]",
            $"[bold]{totalMined.ToString("N0", CultureInfo.InvariantCulture)}[/]",
            $"[bold]{totalUsed.ToString("N0", CultureInfo.InvariantCulture)}[/]"
        );

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