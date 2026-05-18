namespace Core.Services;

public interface ISleeper {
    void Sleep(TimeSpan duration);
}

public class Sleeper : ISleeper {
    public void Sleep(TimeSpan duration) => Thread.Sleep(duration);
}
