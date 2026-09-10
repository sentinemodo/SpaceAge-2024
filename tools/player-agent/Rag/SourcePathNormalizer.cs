namespace SpaceAge.PlayerAgent.Rag;

public static class SourcePathNormalizer
{
    public static string Normalize(string sourcePath) =>
        Path.GetFullPath(sourcePath);
}
