using Core.Services;

namespace Core.Commands;

public class BackupCommand(IBackupService backup) : ICommand {
    public string Name => "backup";
    public string Description => "Create a backup and clean up old backups";

    public void Execute(string[] args) {
        backup.CreateBackup();
        backup.CleanupBackups();
    }
}