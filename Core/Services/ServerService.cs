using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Services;

public class ServerService(IRconService rcon, INssmService nssm, IConfiguration config, ILogger<ServerService> logger, ISleeper sleeper) : IServerService {
    private readonly string _serviceName = config["Nssm:Service"]!;

    public void Start() {
        logger.LogInformation("Starting server");
        nssm.Start(_serviceName);
    }

    public void Stop() {
        logger.LogInformation("Stopping server");
        rcon.Stop();
        sleeper.Sleep(TimeSpan.FromSeconds(30));
        nssm.Stop(_serviceName);
    }
}