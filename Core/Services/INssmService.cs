namespace Core.Services;

public interface INssmService {
    public void Start(string service);
    public void Stop(string service);
    public void Restart(string service);
}