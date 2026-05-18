namespace Core.Services;

public interface IRconService {
    public void Say(string message);
    public void Stop();
    public void Raw(string command);
}