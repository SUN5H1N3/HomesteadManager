namespace Core.Services;

public record PlayerStats(string Player, double Hours, long BlocksMined, long ItemsUsed);

public interface IPlayerStatsService {
    IReadOnlyList<PlayerStats> Collect();
}
