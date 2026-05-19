using System.ComponentModel;
using Core.Services;
using Spectre.Console.Cli;

namespace Core.Commands;

public sealed class LogsCommand(ILogsService logs) : Command<LogsCommand.Settings> {
    public sealed class Settings : CommandSettings {
        [CommandOption("-f|--follow")]
        [Description("Follow the log instead of printing the tail")]
        public bool Follow { get; init; }

        [CommandOption("-n|--lines=<COUNT>")]
        [Description("Number of trailing lines to print")]
        [DefaultValue(50)]
        public int Lines { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation) {
        logs.Tail(settings.Lines);
        if (settings.Follow) {
            logs.Follow();
        } 
        return 0;
    }
}
