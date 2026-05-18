using System.Globalization;
using Core.Services;
using Spectre.Console;

namespace Core.Commands;

public class PlayerStatsCommand(IPlayerStatsService stats) : ICommand {
    public string Name => "player-stats";
    public string Description => "Show player statistics";

    public void Execute(string[] args) {
        var rawPlayers = stats.Collect();
        var detailed = args.Contains("--detailed-distance");
        var ascending = args.Contains("--sort-asc");
        var players = SortPlayers(rawPlayers, args);

        var totalHours = players.Sum(p => p.Hours);
        var totalMined = players.Sum(p => p.BlocksMined);
        var totalUsed = players.Sum(p => p.ItemsUsed);
        var totalDistanceCm = players.Sum(p => p.MovementByType.Values.Sum());

        var (hoursMin, hoursMax) = MinMax(players, p => p.Hours);
        var (minedMin, minedMax) = MinMax(players, p => p.BlocksMined);
        var (usedMin, usedMax) = MinMax(players, p => p.ItemsUsed);
        var (distanceMin, distanceMax) = MinMax(players, p => p.MovementByType.Values.Sum());

        var typeTotals = detailed
            ? players
                .SelectMany(p => p.MovementByType)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.Sum(kvp => kvp.Value))
            : new Dictionary<string, long>();

        var orderedTypes = typeTotals
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();

        var minMaxByType = orderedTypes.ToDictionary(
            t => t,
            t => MinMax(players, p => p.MovementByType.TryGetValue(t, out var v) ? v : 0L)
        );

        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn("Player");
        table.AddColumn(new TableColumn("Hours").RightAligned());
        table.AddColumn(new TableColumn("Blocks Mined").RightAligned());
        table.AddColumn(new TableColumn("Items Used").RightAligned());
        table.AddColumn(new TableColumn("Distance").RightAligned());
        foreach (var type in orderedTypes) {
            table.AddColumn(new TableColumn(ToHeader(type)).RightAligned());
        }

        foreach (var p in players) {
            var distanceCm = p.MovementByType.Values.Sum();
            var row = new List<string> {
                p.Player,
                Highlight(FormatHoursShare(p.Hours, totalHours), IsTop(p.Hours, hoursMin, hoursMax, ascending)),
                Highlight(FormatShare(p.BlocksMined, totalMined), IsTop(p.BlocksMined, minedMin, minedMax, ascending)),
                Highlight(FormatShare(p.ItemsUsed, totalUsed), IsTop(p.ItemsUsed, usedMin, usedMax, ascending)),
                Highlight(FormatKm(distanceCm, totalDistanceCm), IsTop(distanceCm, distanceMin, distanceMax, ascending))
            };

            foreach (var type in orderedTypes) {
                var cm = p.MovementByType.TryGetValue(type, out var v) ? v : 0;
                var (minT, maxT) = minMaxByType[type];
                row.Add(Highlight(FormatKm(cm, typeTotals[type]), IsTop(cm, minT, maxT, ascending)));
            }

            table.AddRow(row.ToArray());
        }

        table.AddEmptyRow();
        var totalRow = new List<string> {
            "[bold]Total[/]",
            $"[bold]{Math.Round(totalHours, 2).ToString(CultureInfo.InvariantCulture)}[/]",
            $"[bold]{totalMined.ToString("N0", CultureInfo.InvariantCulture)}[/]",
            $"[bold]{totalUsed.ToString("N0", CultureInfo.InvariantCulture)}[/]",
            $"[bold]{(totalDistanceCm / 100_000.0).ToString("F2", CultureInfo.InvariantCulture)} km[/]"
        };
        foreach (var type in orderedTypes) {
            totalRow.Add($"[bold]{(typeTotals[type] / 100_000.0).ToString("F2", CultureInfo.InvariantCulture)} km[/]");
        }
        table.AddRow(totalRow.ToArray());

        AnsiConsole.Write(table);
    }

    private static string FormatHoursShare(double hours, double total) {
        var share = total > 0 ? hours / total * 100 : 0;
        return $"{hours.ToString(CultureInfo.InvariantCulture)} ({share.ToString("F1", CultureInfo.InvariantCulture)}%)";
    }

    private static string FormatShare(long value, long total) {
        var share = total > 0 ? (double)value / total * 100 : 0;
        return $"{value.ToString("N0", CultureInfo.InvariantCulture)} ({share.ToString("F1", CultureInfo.InvariantCulture)}%)";
    }

    private static string FormatKm(long cm, long totalCm) {
        var km = cm / 100_000.0;
        var share = totalCm > 0 ? (double)cm / totalCm * 100 : 0;
        return $"{km.ToString("F2", CultureInfo.InvariantCulture)} km ({share.ToString("F1", CultureInfo.InvariantCulture)}%)";
    }

    private static string ToHeader(string snake) =>
        string.Join(" ", snake.Split('_').Select(s => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..]));

    private static string Highlight(string cell, bool isTop) =>
        isTop ? $"[green]{cell}[/]" : cell;

    private static (double Min, double Max) MinMax<T>(IReadOnlyList<T> items, Func<T, double> selector) =>
        items.Count == 0 ? (0, 0) : (items.Min(selector), items.Max(selector));

    private static (long Min, long Max) MinMax<T>(IReadOnlyList<T> items, Func<T, long> selector) =>
        items.Count == 0 ? (0, 0) : (items.Min(selector), items.Max(selector));

    private static bool IsTop(double value, double min, double max, bool ascending) =>
        max > min && value == (ascending ? min : max);

    private static bool IsTop(long value, long min, long max, bool ascending) =>
        max > min && value == (ascending ? min : max);

    private static IReadOnlyList<PlayerStats> SortPlayers(IReadOnlyList<PlayerStats> players, string[] args) {
        var column = (args
            .Where(a => a.StartsWith("--sort="))
            .Select(a => a["--sort=".Length..])
            .FirstOrDefault() ?? "hours").ToLowerInvariant();
        var asc = args.Contains("--sort-asc");

        Func<PlayerStats, IComparable> key = column switch {
            "player" => p => p.Player,
            "hours" => p => p.Hours,
            "mined" or "blocks-mined" => p => p.BlocksMined,
            "used" or "items-used" => p => p.ItemsUsed,
            "distance" => p => p.MovementByType.Values.Sum(),
            _ => p => p.MovementByType.TryGetValue(column.Replace('-', '_'), out var v) ? v : 0L
        };

        return asc
            ? players.OrderBy(key).ToList()
            : players.OrderByDescending(key).ToList();
    }
}
