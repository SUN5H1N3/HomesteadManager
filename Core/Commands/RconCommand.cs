using Core.Services;

namespace Core.Commands;

public class RconCommand(IRconService rcon) : ICommand {
    public string Name => "rcon";
    public string Description => "Send RCON commands or enter interactive RCON mode";

    public void Execute(string[] args) {
        if (args.Length > 0) {
            rcon.Raw(string.Join(" ", args));
        } else {
            Console.WriteLine("Connected to RCON. Type 'exit' to quit.");
            while (true) {
                Console.Write("rcon > ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input) || input == "exit")
                    break;
                rcon.Raw(input);
            }
        }
    }
}  