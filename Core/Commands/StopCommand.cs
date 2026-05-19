using Core.Services;
using Spectre.Console.Cli;

namespace Core.Commands;

public sealed class StopCommand(IServerService server) : Command {
    protected override int Execute(CommandContext context, CancellationToken cancellation) {
        server.Stop();
        return 0;
    }
}
