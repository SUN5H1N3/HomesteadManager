using Core.Services;

namespace Core.Commands;

public class BackupCommand(IBackupService backup) : ICommand {
    public string Name => "backup";

    public void Execute(string[] args) {
        backup.CreateBackup();
        backup.CleanupBackups();
    }
}