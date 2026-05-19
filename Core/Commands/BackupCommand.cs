using Core.Services;
using Spectre.Console.Cli;

namespace Core.Commands;

public sealed class BackupCommand(IBackupService backup) : Command {
    protected override int Execute(CommandContext context, CancellationToken cancellation) {
        backup.CreateBackup();
        backup.CleanupBackups();
        return 0;
    }
}
