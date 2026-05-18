using Core.Services;

namespace Core.Commands;

public class RestartCommand(IRestartService restart, ILogsService logs) : ICommand {
    public string Name => "restart";
    public string Description => "Warn players, stop the server, back up, and start it again";

    public void Execute(string[] args) {
        int? warningMinutes = args.Length > 0 && int.TryParse(args[0], out var parsed)
            ? parsed
            : null;

        var outcome = restart.Restart(warningMinutes);

        if (outcome == RestartOutcome.Restarted && !args.Contains("--without-server-logs")) {
            logs.Follow();
        }
    }
}
