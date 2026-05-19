using Microsoft.Extensions.Logging;

namespace Core.Services;

public interface ISleeper {
    void Sleep(TimeSpan duration);
}

public class Sleeper(ILogger<Sleeper> logger) : ISleeper {
    public void Sleep(TimeSpan duration) {
        logger.LogInformation("Sleeping for {Duration}", duration);
        Thread.Sleep(duration);
    }
}
