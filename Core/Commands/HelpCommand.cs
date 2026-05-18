using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace Core.Commands;

public class HelpCommand(IServiceProvider provider) : ICommand {
    public string Name => "help";
    public string Description => "List available commands";

    public void Execute(string[] args) {
        var table = new Table();
        table.AddColumn("Command");
        table.AddColumn("Description");
        table.Border(TableBorder.Rounded);

        foreach (var command in provider.GetServices<ICommand>().OrderBy(c => c.Name))
            table.AddRow(command.Name, command.Description);

        AnsiConsole.Write(table);
    }
}