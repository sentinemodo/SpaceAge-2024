namespace SpaceAge.PlayerAgent;

public enum PlayMode
{
    Test,
    Campaign,
}

public static class PlayModeParser
{
    public static bool TryParse(string? value, out PlayMode mode)
    {
        mode = PlayMode.Test;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        switch (value.Trim().ToLowerInvariant())
        {
            case "test":
            case "sample":
                mode = PlayMode.Test;
                return true;
            case "campaign":
                mode = PlayMode.Campaign;
                return true;
            default:
                return false;
        }
    }

    public static string ToCliValue(PlayMode mode) =>
        mode == PlayMode.Campaign ? "campaign" : "test";
}
