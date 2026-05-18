using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Services;

public class LogsService(IConfiguration config, ILogger<LogsService> logger) : ILogsService {
    private readonly string _logPath = config["Logs:Path"]!;

    public void Tail(int lines) {
        if (!File.Exists(_logPath)) {
            Console.WriteLine("Log file not found.");
            return;
        }

        using var stream = new FileStream(_logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);

        var allLines = new List<string>();
        while (!reader.EndOfStream)
            allLines.Add(reader.ReadLine()!);

        foreach (var line in allLines.TakeLast(lines))
            Console.WriteLine(line);
    }

    public void Follow() {
        if (!File.Exists(_logPath)) {
            Console.WriteLine("Log file not found.");
            return;
        }

        Console.WriteLine("Following logs... Press Ctrl+C to stop.");

        using var stream = new FileStream(_logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);

        // Перемотать в конец
        stream.Seek(0, SeekOrigin.End);

        while (true) {
            var line = reader.ReadLine();
            if (line != null)
                Console.WriteLine(line);
            else
                Thread.Sleep(200);
        }
    }
}