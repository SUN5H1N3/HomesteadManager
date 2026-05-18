using System.IO.Compression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Services;

public class BackupService(IConfiguration config, ILogger<BackupService> logger) : IBackupService {
    private readonly string _worldPath = config["Backup:WorldPath"]!;
    private readonly string _backupPath = config["Backup:BackupPath"]!;
    private readonly int _maxBackups = int.Parse(config["Backup:MaxBackups"]!);

    public void CreateBackup() {
        Directory.CreateDirectory(_backupPath);
        var date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var archive = Path.Combine(_backupPath, $"world_{date}.zip");

        logger.LogInformation("Creating backup: {Archive}", archive);
        ZipFile.CreateFromDirectory(_worldPath, archive);
        logger.LogInformation("Backup complete");
    }

    public void CleanupBackups() {
        var backups = Directory.GetFiles(_backupPath, "*.zip")
            .Select(f => new FileInfo(f))
            .OrderBy(f => f.LastWriteTime)
            .ToList();

        while (backups.Count > _maxBackups) {
            logger.LogInformation("Removing old backup: {Name}", backups[0].Name);
            backups[0].Delete();
            backups.RemoveAt(0);
        }
    }
}