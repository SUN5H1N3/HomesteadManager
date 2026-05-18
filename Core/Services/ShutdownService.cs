using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Core.Services;

public class ShutdownService(ILogger<ShutdownService> logger) : IShutdownService {
    public void Reboot() {
        logger.LogInformation("Shutting down...");
        Process.Start(new ProcessStartInfo {
            FileName = "shutdown", 
            Arguments = "/r /t 0", 
            UseShellExecute = false
        });
    }
}