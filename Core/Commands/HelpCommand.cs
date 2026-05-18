using Microsoft.Extensions.DependencyInjection;

namespace Core.Commands;

public class HelpCommand(IServiceProvider provider) : ICommand {
    public string Name => "help";

    public void Execute(string[] args) {
        var commands = provider.GetServices<ICommand>();
        Console.WriteLine("Available commands:");
        foreach (var command in commands.OrderBy(c => c.Name))
            Console.WriteLine($"  {command.Name}");
    }
}