using Core.Commands;
using Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

var config = new ConfigurationBuilder()
    .AddJsonFile(AppContext.BaseDirectory + "/appsettings.json")
    .Build();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        AppContext.BaseDirectory + "/logs/hm-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7
    )
    .CreateLogger();

var services = new ServiceCollection()
    .AddSingleton<IConfiguration>(config)
    .AddLogging(b => b.AddSerilog())
    .AddSingleton<IRconService, RconService>()
    .AddSingleton<IBackupService, BackupService>()
    .AddSingleton<IServerService, ServerService>()
    .AddSingleton<INssmService, NssmService>()
    .AddSingleton<ILogsService, LogsService>()
    .AddSingleton<ICommandService, CommandService>()
    .AddSingleton<IShutdownService, ShutdownService>()
    .AddSingleton<ICommand, RestartCommand>()
    .AddSingleton<ICommand, StartCommand>()
    .AddSingleton<ICommand, StopCommand>()
    .AddSingleton<ICommand, BackupCommand>()
    .AddSingleton<ICommand, RconCommand>()
    .AddSingleton<ICommand, LogsCommand>()
    .AddSingleton<ICommand, HelpCommand>()
    .BuildServiceProvider();

if (args.Length == 0) {
    Console.WriteLine("Homestead Manager. Type 'help' to list commands, 'exit' to quit.");
    while (true) {
        Console.Write("> ");
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input)) continue;
        if (input == "exit") break;

        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        services.GetRequiredService<ICommandService>().Execute(parts);
    }
} else {
    services.GetRequiredService<ICommandService>().Execute(args);
}