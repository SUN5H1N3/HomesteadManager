using Core.Services;

namespace Core.Commands;

public class StartCommand(IServerService server, ILogsService logs) : ICommand {
    public string Name => "start";
    public string Description => "Start the server and follow logs unless -silent is passed";

    public void Execute(string[] args) {
        server.Start();
        if (!args.Contains("-silent")) {
            logs.Follow();
        }
    }
}