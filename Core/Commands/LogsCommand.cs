using Core.Services;

namespace Core.Commands;

public class LogsCommand(ILogsService logs) : ICommand {
    public string Name => "logs";

    public void Execute(string[] args) {
        var follow = args.Contains("-f");
        var lines = 50;

        var linesArg = args.FirstOrDefault(a => a.StartsWith("-n"));
        if (linesArg != null) {
            int.TryParse(linesArg.Replace("-n", ""), out lines);
        }

        if (follow) {
            logs.Follow();
        } else {
            logs.Tail(lines);
        }
    }
}