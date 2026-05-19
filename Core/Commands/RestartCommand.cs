using System.ComponentModel;
using Core.Services;
using Spectre.Console.Cli;

namespace Core.Commands;

public sealed class RestartCommand(IRestartService restart, ILogsService logs)
    : Command<RestartCommand.Settings> {
    public sealed class Settings : CommandSettings {
        [CommandArgument(0, "[minutes]")]
        [Description("Minutes to warn players before restart (default: none)")]
        public int? WarningMinutes { get; init; }

        [CommandOption("--without-server-logs")]
        [Description("Do not follow server logs after restart")]
        public bool WithoutServerLogs { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation) {
        var outcome = restart.Restart(settings.WarningMinutes);

        if (outcome == RestartOutcome.Restarted && !settings.WithoutServerLogs) {
            logs.Follow();
        }

        return 0;
    }
}
