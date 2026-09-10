using Microsoft.Data.Sqlite;

namespace SpaceAge.PlayerAgent.Rag;

public sealed class SqliteVectorStore : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteVectorStore(string sqlitePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(sqlitePath)!);
        _connection = new SqliteConnection($"Data Source={sqlitePath}");
        _connection.Open();
        EnsureSchema();
    }

    public void ClearAll()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = "DELETE FROM chunks;";
        command.ExecuteNonQuery();
    }

    public void ReplaceSource(string sourcePath, IReadOnlyList<(TextChunk Chunk, float[] Embedding)> rows)
    {
        var normalizedPath = SourcePathNormalizer.Normalize(sourcePath);
        using var transaction = _connection.BeginTransaction();

        using (var delete = _connection.CreateCommand())
        {
            delete.Transaction = transaction;
            delete.CommandText = """
                DELETE FROM chunks
                WHERE lower(source_path) = lower($source_path);
                """;
            delete.Parameters.AddWithValue("$source_path", normalizedPath);
            delete.ExecuteNonQuery();
        }

        foreach (var (chunk, embedding) in rows)
        {
            InsertChunk(WithNormalizedSource(chunk, normalizedPath), embedding, transaction);
        }

        transaction.Commit();
    }

    public IReadOnlyList<StoredChunk> ListAll()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = """
            SELECT id, source_path, doc, heading, verb, mode, content, embedding
            FROM chunks;
            """;

        var rows = new List<StoredChunk>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rows.Add(ReadStoredChunk(reader));
        }

        return rows;
    }

    public int Count() =>
        Convert.ToInt32(Scalar("SELECT COUNT(*) FROM chunks;"));

    public IReadOnlyList<string> ListSourcePaths()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = """
            SELECT DISTINCT source_path
            FROM chunks
            ORDER BY source_path;
            """;

        var paths = new List<string>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            paths.Add(reader.GetString(0));
        }

        return paths;
    }

    public int DeleteSources(IReadOnlyList<string> sourcePaths)
    {
        if (sourcePaths.Count == 0)
        {
            return 0;
        }

        var removed = 0;
        using var transaction = _connection.BeginTransaction();
        foreach (var sourcePath in sourcePaths)
        {
            using var delete = _connection.CreateCommand();
            delete.Transaction = transaction;
            delete.CommandText = """
                DELETE FROM chunks
                WHERE lower(source_path) = lower($source_path);
                """;
            delete.Parameters.AddWithValue("$source_path", SourcePathNormalizer.Normalize(sourcePath));
            removed += delete.ExecuteNonQuery();
        }

        transaction.Commit();
        return removed;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private void EnsureSchema()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS chunks (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                source_path TEXT NOT NULL,
                doc TEXT NOT NULL,
                heading TEXT,
                verb TEXT,
                mode TEXT,
                content TEXT NOT NULL,
                content_hash TEXT NOT NULL UNIQUE,
                embedding BLOB NOT NULL,
                ingested_at TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS idx_chunks_doc ON chunks(doc);
            CREATE INDEX IF NOT EXISTS idx_chunks_verb ON chunks(verb);
            CREATE INDEX IF NOT EXISTS idx_chunks_source ON chunks(source_path);
            """;
        command.ExecuteNonQuery();
    }

    private static TextChunk WithNormalizedSource(TextChunk chunk, string normalizedSourcePath) =>
        chunk with
        {
            Metadata = chunk.Metadata with { SourcePath = normalizedSourcePath },
        };

    private void InsertChunk(TextChunk chunk, float[] embedding, SqliteTransaction transaction)
    {
        using var command = _connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO chunks (
                source_path, doc, heading, verb, mode, content, content_hash, embedding, ingested_at
            ) VALUES (
                $source_path, $doc, $heading, $verb, $mode, $content, $content_hash, $embedding, $ingested_at
            );
            """;
        command.Parameters.AddWithValue("$source_path", chunk.Metadata.SourcePath);
        command.Parameters.AddWithValue("$doc", chunk.Metadata.Doc);
        command.Parameters.AddWithValue("$heading", (object?)chunk.Metadata.Heading ?? DBNull.Value);
        command.Parameters.AddWithValue("$verb", (object?)chunk.Metadata.Verb ?? DBNull.Value);
        command.Parameters.AddWithValue("$mode", (object?)chunk.Metadata.Mode ?? DBNull.Value);
        command.Parameters.AddWithValue("$content", chunk.Content);
        command.Parameters.AddWithValue("$content_hash", ContentHash.Compute(chunk));
        command.Parameters.AddWithValue("$embedding", VectorMath.ToBytes(embedding));
        command.Parameters.AddWithValue("$ingested_at", DateTimeOffset.UtcNow.ToString("O"));
        command.ExecuteNonQuery();
    }

    private static StoredChunk ReadStoredChunk(SqliteDataReader reader)
    {
        var metadata = new ChunkMetadata(
            reader.GetString(2),
            reader.IsDBNull(4) ? null : reader.GetString(4),
            reader.IsDBNull(5) ? null : reader.GetString(5),
            reader.GetString(1),
            reader.IsDBNull(3) ? null : reader.GetString(3));

        var chunk = new TextChunk(reader.GetString(6), metadata);
        var embedding = VectorMath.FromBytes((byte[])reader.GetValue(7));
        return new StoredChunk(reader.GetInt64(0), chunk, embedding);
    }

    private object Scalar(string sql)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteScalar() ?? 0;
    }
}
