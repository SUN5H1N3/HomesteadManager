namespace Core.Services;

public enum RestartOutcome {
    Restarted,
    Rebooting
}

public interface IRestartService {
    RestartOutcome Restart(int? warningMinutes = null);
}
