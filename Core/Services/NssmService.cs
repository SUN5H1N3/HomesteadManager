using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Services;

public class NssmService(IConfiguration config, ILogger<NssmService> logger) : INssmService {
    private readonly string _nssm = config["Nssm:Path"]!;

    public void Start(string service) {
        Run("start", service);
    }

    public void Stop(string service) {
        Run("stop", service);
    }

    public void Restart(string service) {
        Run("restart", service);
    }

    private void Run(string action, string service) {
        logger.LogInformation("NSSM: {Action} {Service}", action, service);
        Process.Start(new ProcessStartInfo {
                FileName = _nssm, 
                Arguments = $"{action} {service}", 
                UseShellExecute = false
            })
            ?.WaitForExit();
    }
}