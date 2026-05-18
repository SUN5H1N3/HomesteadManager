using Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Commands;

public class RestartCommand(
    IRconService rcon,
    IServerService server,
    IBackupService backup,
    IConfiguration config,
    ILogger<RestartCommand> logger,
    ILogsService logs,
    IShutdownService shutdown
) : ICommand {
    public string Name => "restart";
    public string Description => "Warn players, stop the server, back up, and start it again";

    public void Execute(string[] args) {    
        var defaultMinutes = int.Parse(config["Restart:WarningMinutes"] ?? "10");
        var warningMinutes = args.Length > 0 && int.TryParse(args[0], out var parsed)
            ? parsed
            : defaultMinutes;

        if (warningMinutes > 0) {
            var remaining = TimeSpan.FromMinutes(warningMinutes);
            rcon.Say($"Server will restart in {warningMinutes} minutes!");

            while (remaining.TotalSeconds > 0) {
                TimeSpan delay;

                if (remaining.TotalMinutes > 10) {
                    delay = TimeSpan.FromMinutes(10);
                } else if (remaining.TotalSeconds > 60) {
                    delay = TimeSpan.FromMinutes(1);
                } else {
                    delay = TimeSpan.FromSeconds(10);
                }

                Thread.Sleep(delay);
                remaining -= delay;

                if (remaining.TotalSeconds <= 0) {
                    break;
                }

                var message = remaining.TotalMinutes >= 1
                    ? $"Server will restart in {(int)remaining.TotalMinutes} {((int)remaining.TotalMinutes > 1 ? "minutes" : "minute")}!"
                    : $"Server will restart in {(int)remaining.TotalSeconds} seconds!";

                rcon.Say(message);
            }
        }

        rcon.Say("Restarting now!");

        server.Stop();
        try {
            backup.CreateBackup();
            backup.CleanupBackups();
        } catch (Exception e) {
            logger.LogError(e, "Failed to create backup");
        }

        if (bool.Parse(config["Restart:RebootAfterRestart"] ?? "false")) {
            shutdown.Reboot();
        } else {
            server.Start();
        
            if (!args.Contains("--without-server-logs")) {
                logs.Follow();
            }
        }
    }
}