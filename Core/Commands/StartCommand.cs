using System.ComponentModel;
using Core.Services;
using Spectre.Console.Cli;

namespace Core.Commands;

public sealed class StartCommand(IServerService server, ILogsService logs)
    : Command<StartCommand.Settings> {
    public sealed class Settings : CommandSettings {
        [CommandOption("--silent")]
        [Description("Do not follow logs after starting")]
        public bool Silent { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation) {
        server.Start();
        if (!settings.Silent) {
            logs.Follow();
        }
        return 0;
    }
}
