namespace Core.Services;

public record PlayerStats(
    string Player,
    double Hours,
    long BlocksMined,
    long ItemsUsed,
    IReadOnlyDictionary<string, long> MovementByType,
    double DamageDealt,
    double DamageTaken
);

public interface IPlayerStatsService {
    IReadOnlyList<PlayerStats> Collect();
}