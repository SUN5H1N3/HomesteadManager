using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Services;

public class RestartService(
    IRconService rcon,
    IServerService server,
    IBackupService backup,
    IConfiguration config,
    ILogger<RestartService> logger,
    IShutdownService shutdown,
    ISleeper sleeper
) : IRestartService {
    public RestartOutcome Restart(int? warningMinutes = null) {
        var minutes = warningMinutes ?? int.Parse(config["Restart:WarningMinutes"] ?? "10");

        if (minutes > 0) {
            var remaining = TimeSpan.FromMinutes(minutes);
            rcon.Say($"Server will restart in {minutes} minutes!");

            while (remaining.TotalSeconds > 0) {
                TimeSpan delay;

                if (remaining.TotalMinutes > 10) {
                    delay = TimeSpan.FromMinutes(10);
                } else if (remaining.TotalSeconds > 60) {
                    delay = TimeSpan.FromMinutes(1);
                } else {
                    delay = TimeSpan.FromSeconds(10);
                }

                sleeper.Sleep(delay);
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
            return RestartOutcome.Rebooting;
        }

        server.Start();
        return RestartOutcome.Restarted;
    }
}
