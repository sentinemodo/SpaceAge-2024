using System.Security.Cryptography;
using System.Text;

namespace SpaceAge.PlayerAgent.Rag;

public static class ContentHash
{
    public static string Compute(TextChunk chunk)
    {
        var payload = string.Join(
            '\n',
            chunk.Metadata.SourcePath,
            chunk.Metadata.Doc,
            chunk.Metadata.Verb ?? string.Empty,
            chunk.Metadata.Mode ?? string.Empty,
            chunk.Metadata.Heading ?? string.Empty,
            chunk.Content);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash);
    }
}
