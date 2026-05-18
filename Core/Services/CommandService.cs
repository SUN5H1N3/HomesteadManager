using Core.Commands;
using Microsoft.Extensions.Logging;

namespace Core.Services;

public class CommandService : ICommandService {
    private readonly Dictionary<string, ICommand> _commands;
    private readonly ILogger<CommandService> _logger;

    public CommandService(IEnumerable<ICommand> commands, ILogger<CommandService> logger) {
        _commands = commands.ToDictionary(c => c.Name);
        _logger = logger;
    }

    public void Execute(string[] args) {
        var commandName = args.FirstOrDefault() ?? "";
        var commandArgs = args.Skip(1).ToArray();

        if (!_commands.TryGetValue(commandName, out var command)) {
            _logger.LogError("Unknown command: {Command}", commandName);
            Console.WriteLine($"Unknown command: {commandName}");
            Console.WriteLine($"Available commands: {string.Join(" | ", _commands.Keys)}");
            return;
        }

        _logger.LogInformation("Executing command: {Command}", commandName);
        command.Execute(commandArgs);
    }
}