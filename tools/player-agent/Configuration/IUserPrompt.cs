namespace SpaceAge.PlayerAgent.Configuration;

public interface IUserPrompt
{
    bool Confirm(string message);
}

public sealed class ConsoleUserPrompt : IUserPrompt
{
    public bool Confirm(string message)
    {
        Console.Error.Write(message);
        var response = Console.ReadLine();
        return string.Equals(response, "y", StringComparison.OrdinalIgnoreCase)
            || string.Equals(response, "yes", StringComparison.OrdinalIgnoreCase);
    }
}
