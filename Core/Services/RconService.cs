using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Services;

public class RconService(IConfiguration config, ILogger<RconService> logger) : IRconService {
    private readonly string _mcrcon = config["Rcon:Path"]!;
    private readonly string _host = config["Rcon:Host"]!;
    private readonly string _port = config["Rcon:Port"]!;
    private readonly string _password = Environment.GetEnvironmentVariable("RconPassword")
        ?? config["Rcon:Password"]!;

    public void Say(string message) {
        Send($"say {message}");
    }

    public void Stop() {
        Send("stop");
    }

    public void Raw(string command) {
        Send(command);
    }

    private void Send(string command) {
        logger.LogInformation("RCON: {Command}", command);
        Process.Start(
                new ProcessStartInfo {
                    FileName = _mcrcon,
                    Arguments = $"-H {_host} -P {_port} -p \"{_password}\" \"{command}\"",
                    UseShellExecute = false
                }
            )
            ?.WaitForExit();
    }
}