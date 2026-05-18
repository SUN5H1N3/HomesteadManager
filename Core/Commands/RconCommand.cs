using Core.Services;

namespace Core.Commands;

public class RconCommand(IRconService rcon) : ICommand {
    public string Name => "rcon";

    public void Execute(string[] args) {
        if (args.Length > 0) {
            rcon.Raw(string.Join(" ", args));
        } else {
            Console.WriteLine("Connected to RCON. Type 'exit' to quit.");
            while (true) {
                Console.Write("> ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input) || input == "exit")
                    break;
                rcon.Raw(input);
            }
        }
    }
}