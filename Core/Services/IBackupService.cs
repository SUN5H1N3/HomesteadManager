namespace Core.Services;

public interface IBackupService {
    public void CreateBackup();
    public void CleanupBackups();
}