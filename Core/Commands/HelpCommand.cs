using Microsoft.Extensions.DependencyInjection;

namespace Core.Commands;

public class HelpCommand(IServiceProvider provider) : ICommand {
    public string Name => "help";
    public string Description => "List available commands";

    public void Execute(string[] args) {
        var commands = provider.GetServices<ICommand>().OrderBy(c => c.Name).ToList();
        var width = commands.Max(c => c.Name.Length);
        Console.WriteLine("Available commands:");
        foreach (var command in commands)
            Console.WriteLine($"  {command.Name.PadRight(width)}  {command.Description}");
    }
}