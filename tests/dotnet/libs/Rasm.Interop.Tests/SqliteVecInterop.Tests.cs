using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyModel;
using Rasm.Interop.SqliteVec;
using Rasm.TestSupport;

namespace Rasm.Interop.Tests;

// --- [CONSTANTS] -----------------------------------------------------------------------
internal static class SqliteVecFixtures {
    public const string ConnectionString = "Data Source=:memory:";
    public const int MaxDimension = 16;
    public const int MaxRows = 32;
    public static readonly Tolerance Float32 = Tolerance.Combined(absolute: 1.0e-4, relative: 1.0e-5); // vec0 stores float32 and computes every distance in float32
    // The deps.json of the test host records the version restore resolved for Rasm.Native.SqliteVec from Directory.Packages.props
    public static readonly string PinnedVersion = DependencyContext.Default!.RuntimeLibraries.Single(static library => string.Equals(library.Name, "Rasm.Native.SqliteVec", StringComparison.Ordinal)).Version;
}

// --- [GENERATORS] ----------------------------------------------------------------------
internal static class SqliteVecGenerators {
    public static readonly Gen<(float[] Left, float[] Right)> Pair =
        Gen.Int[1, SqliteVecFixtures.MaxDimension].SelectMany(static dimension => Vector(dimension).Select(Vector(dimension), static (left, right) => (Left: left, Right: right)));
    public static Gen<(float[][] Rows, float[] Query, int K)> Neighborhood(int dimension) =>
        Vector(dimension).Array[1, SqliteVecFixtures.MaxRows].Select(Vector(dimension), Gen.Int[1, SqliteVecFixtures.MaxRows], static (rows, query, k) => (Rows: rows, Query: query, K: k));

    private static Gen<float[]> Vector(int dimension) => Gen.Float[-100.0f, 100.0f].Array[dimension];
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class SqliteVecInteropTests {
    // A stale staged loadable packed under a newer manifest version fails here
    [Fact]
    public void LoadedExtensionReportsThePinnedPackageVersion() {
        using SqliteConnection connection = OpenWithExtension();
        Assert.Equal($"v{SqliteVecFixtures.PinnedVersion}", Version(connection));
    }

    // The facade documents that Microsoft.Data.Sqlite keeps the extension across a close and reopen of the same connection
    [Fact]
    public void LoadSurvivesCloseAndReopenOfTheConnection() {
        using SqliteConnection connection = OpenWithExtension();
        connection.Close();
        connection.Open();
        Assert.Equal($"v{SqliteVecFixtures.PinnedVersion}", Version(connection));
    }

    [Fact]
    public void L2DistanceMatchesTheOracleForEveryPair() =>
        TestAssertions.ForAll(SqliteVecGenerators.Pair, static pair => {
            using SqliteConnection connection = OpenWithExtension();
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = "select vec_distance_L2(@left, @right)";
            _ = command.Parameters.AddWithValue("@left", Blob(pair.Left));
            _ = command.Parameters.AddWithValue("@right", Blob(pair.Right));
            TestAssertions.Equal(Assert.IsType<double>(command.ExecuteScalar()), Distance(pair.Left, pair.Right), SqliteVecFixtures.Float32, label: "vec_distance_L2");
        });

    [Fact]
    public void KnnOverOneDimensionReturnsTheNearestRows() =>
        Knn(1, static create => create.CommandText = "create virtual table items using vec0(embedding float[1])");

    [Fact]
    public void KnnOverThreeDimensionsReturnsTheNearestRows() =>
        Knn(3, static create => create.CommandText = "create virtual table items using vec0(embedding float[3])");

    [Fact]
    public void KnnOverSixteenDimensionsReturnsTheNearestRows() =>
        Knn(SqliteVecFixtures.MaxDimension, static create => create.CommandText = "create virtual table items using vec0(embedding float[16])");

    // vec0 takes its dimension as a DDL literal, each caller assigns its own command text
    private static void Knn(int dimension, Action<SqliteCommand> declareTable) =>
        TestAssertions.ForAll(SqliteVecGenerators.Neighborhood(dimension), example => {
            using SqliteConnection connection = OpenWithExtension();
            CreateTable(connection, declareTable, example.Rows);
            (long RowId, double Distance)[] actual = Nearest(connection, example.Query, example.K);
            double[] oracle = [.. example.Rows.Select(row => Distance(row, example.Query))];
            double[] expected = [.. oracle.Order().Take(example.K)];
            Assert.Equal(expected.Length, actual.Length);
            TestAssertions.Equal([.. actual.Select(static hit => hit.Distance)], expected, SqliteVecFixtures.Float32, label: "knn distances");
            TestAssertions.Equal([.. actual.Select(hit => oracle[hit.RowId - 1])], expected, SqliteVecFixtures.Float32, label: "knn rows");
        });

    private static SqliteConnection OpenWithExtension() {
        SqliteConnection connection = new(SqliteVecFixtures.ConnectionString);
        connection.Open();
        TestAssertions.Succ(SqliteVecInterop.Load(connection));
        return connection;
    }

    private static string Version(SqliteConnection connection) {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "select vec_version()";
        return Assert.IsType<string>(command.ExecuteScalar());
    }

    private static void CreateTable(SqliteConnection connection, Action<SqliteCommand> declareTable, float[][] rows) {
        using SqliteCommand create = connection.CreateCommand();
        declareTable(create);
        _ = create.ExecuteNonQuery();
        using SqliteCommand insert = connection.CreateCommand();
        insert.CommandText = "insert into items(rowid, embedding) values (@rowid, @embedding)";
        SqliteParameter rowId = insert.Parameters.Add("@rowid", SqliteType.Integer);
        SqliteParameter embedding = insert.Parameters.Add("@embedding", SqliteType.Blob);
        foreach ((int index, float[] row) in rows.Index()) {
            rowId.Value = index + 1L;
            embedding.Value = Blob(row);
            _ = insert.ExecuteNonQuery();
        }
    }

    private static (long RowId, double Distance)[] Nearest(SqliteConnection connection, float[] query, int k) {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "select rowid, distance from items where embedding match @query and k = @k order by distance";
        _ = command.Parameters.AddWithValue("@query", Blob(query));
        _ = command.Parameters.AddWithValue("@k", k);
        using SqliteDataReader reader = command.ExecuteReader();
        List<(long RowId, double Distance)> hits = [];
        while (reader.Read()) hits.Add((reader.GetInt64(0), reader.GetDouble(1)));
        return [.. hits];
    }

    private static byte[] Blob(float[] vector) => MemoryMarshal.AsBytes(vector.AsSpan()).ToArray();

    private static double Distance(float[] left, float[] right) =>
        NumericOracles.Distance([.. left.Select(static x => (double)x)], [.. right.Select(static x => (double)x)]);
}
