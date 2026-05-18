using System.Globalization;
using Core.Services;
using Spectre.Console;

namespace Core.Commands;

public class PlayerStatsCommand(IPlayerStatsService stats) : ICommand {
    public string Name => "player-stats";
    public string Description => "Show player statistics";

    public void Execute(string[] args) {
        var players = stats.Collect();

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
}
