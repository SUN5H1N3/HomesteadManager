using System.ComponentModel;
using Core.Services;
using Spectre.Console.Cli;

namespace Core.Commands;

public sealed class RconCommand(IRconService rcon) : Command<RconCommand.Settings> {
    public sealed class Settings : CommandSettings {
        [CommandArgument(0, "[text]")]
        [Description("RCON command text (omit to enter interactive mode)")]
        public string[]? Text { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation) {
        if (settings.Text is { Length: > 0 }) {
            rcon.Raw(string.Join(" ", settings.Text));
            return 0;
        }

        Console.WriteLine("Connected to RCON. Type 'exit' to quit.");
        while (true) {
            Console.Write("rcon > ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input) || input == "exit")
                break;
            rcon.Raw(input);
        }
        return 0;
    }
}
