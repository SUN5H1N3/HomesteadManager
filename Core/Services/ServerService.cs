using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Services;

public class ServerService(IRconService rcon, INssmService nssm, IConfiguration config, ILogger<ServerService> logger) : IServerService {
    private readonly string _serviceName = config["Server:ServiceName"] ?? "Minecraft";

    public void Start() {
        logger.LogInformation("Starting server");
        nssm.Start(_serviceName);
    }

    public void Stop() {
        logger.LogInformation("Stopping server");
        rcon.Stop();
        nssm.Stop(_serviceName);
    }
}