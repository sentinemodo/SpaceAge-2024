namespace SpaceAge.PlayerAgent.Rag;

public static class VectorIndexPaths
{
    public static string SharedSqlitePath(string sharedIndexDirectory) =>
        Path.Combine(sharedIndexDirectory, "shared.sqlite");

    public static string FactionSqlitePath(string factionIndexDirectory) =>
        Path.Combine(factionIndexDirectory, "faction.sqlite");
}
