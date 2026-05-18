using Core.Services;

namespace Core.Commands;

public class StopCommand(IServerService server) : ICommand {
    public string Name => "stop";

    public void Execute(string[] args) => server.Stop();
}