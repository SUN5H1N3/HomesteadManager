namespace Core.Services;

public interface ILogsService {
    public void Tail(int lines);
    public void Follow();
}