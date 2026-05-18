using Core.Services;

namespace Core.Commands;

public class StartCommand(IServerService server, ILogsService logs) : ICommand {
    public string Name => "start";

    public void Execute(string[] args) {
        server.Start();
        if (!args.Contains("-silent")) {
            logs.Follow();
        }
    }
}