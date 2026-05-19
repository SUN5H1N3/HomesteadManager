using System.Globalization;
using Core.Cli;
using Core.Commands;
using Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console.Cli;

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
    .AddSingleton<IShutdownService, ShutdownService>()
    .AddSingleton<IPlayerStatsService, PlayerStatsService>()
    .AddSingleton<IRestartService, RestartService>()
    .AddSingleton<ISleeper, Sleeper>();

var app = new CommandApp(new TypeRegistrar(services));
app.Configure(c => {
    c.SetApplicationName("hm");
    c.AddCommand<RestartCommand>("restart")
        .WithDescription("Warn players, stop the server, back up, and start it again");
    c.AddCommand<StartCommand>("start")
        .WithDescription("Start the server and follow logs unless --silent is passed");
    c.AddCommand<StopCommand>("stop")
        .WithDescription("Stop the server");
    c.AddCommand<BackupCommand>("backup")
        .WithDescription("Create a backup and clean up old backups");
    c.AddCommand<RconCommand>("rcon")
        .WithDescription("Send RCON commands or enter interactive RCON mode");
    c.AddCommand<LogsCommand>("logs")
        .WithDescription("Show server logs (-f to follow, -n to set line count)");
    c.AddCommand<PlayerStatsCommand>("player-stats")
        .WithDescription("Show player statistics");
    c.SetApplicationCulture(CultureInfo.InvariantCulture);
});

if (args.Length == 0) {
    Console.WriteLine("Homestead Manager. Type 'help' to list commands, 'exit' to quit.");
    while (true) {
        Console.Write("hm > ");
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input)) continue;
        if (input == "exit") break;

        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts is ["help"]) parts = ["--help"];
        app.Run(parts);
    }
    return 0;
}

return app.Run(args);
