using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;

namespace Rasm.Interop.SqliteVec;

// --- [ERRORS] --------------------------------------------------------------------------
/// <summary>Holds the error codes of the sqlite-vec facade</summary>
public static class Codes {
    /// <summary>No native search directory holds the vec0 loadable for the runtime identifier</summary>
    public const int NotFound = 3101;
    /// <summary>SQLite rejected the vec0 loadable</summary>
    public const int LoadFailed = 3102;
}

/// <summary>The error returned when no native search directory holds the vec0 loadable</summary>
/// <param name="RuntimeIdentifier">The runtime identifier of the host</param>
/// <param name="Searched">The directories searched for the loadable</param>
public sealed record SqliteVecNotFound(string RuntimeIdentifier, Seq<string> Searched) : Expected("vec0 loadable library not found for the runtime identifier", Codes.NotFound);

/// <summary>The error returned when SQLite rejects the vec0 loadable</summary>
public sealed record SqliteVecLoadFailed : Expected {
    /// <summary>Initializes a new instance of the <see cref="SqliteVecLoadFailed"/> class with the rejected path and the SQLite error</summary>
    /// <param name="libraryPath">The full path of the rejected loadable</param>
    /// <param name="cause">The error SQLite raised</param>
    public SqliteVecLoadFailed(string libraryPath, Error cause) : base("vec0 failed to load", Codes.LoadFailed, cause) => LibraryPath = libraryPath;

    /// <summary>Gets the full path of the rejected loadable</summary>
    /// <value>The full path of the loadable</value>
    public string LibraryPath { get; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
/// <summary>Provides the sqlite-vec extension load that callers run on every open <see cref="SqliteConnection"/> before its first vector query</summary>
/// <remarks>
/// <para>SQLite resolves a loadable through the operating system loader, outside the .NET native library probing, and the load passes the full path of vec0 from the Rasm.Native.SqliteVec runtimes directory</para>
/// <para>The search covers the host's NATIVE_DLL_SEARCH_DIRECTORIES, then runtimes/&lt;rid&gt;/native and the root under the facade assembly directory and the application base directory. The assembly directory covers plugin hosts that load the assembly outside their own deps.json</para>
/// <para>SQLitePCLRaw exposes no sqlite3_auto_extension, the load is per connection, and <see cref="SqliteConnection.LoadExtension"/> keeps it across a close and reopen of the same connection</para>
/// </remarks>
public static class SqliteVecInterop {
    /// <summary>The entry point vec0 exports</summary>
    public const string EntryPoint = "sqlite3_vec_init";

    private const string SearchDirectoriesProperty = "NATIVE_DLL_SEARCH_DIRECTORIES";
    private static readonly string FileName = OperatingSystem.IsWindows() ? "vec0.dll" : OperatingSystem.IsMacOS() ? "vec0.dylib" : "vec0.so";

    /// <summary>Loads vec0 into the connection</summary>
    /// <param name="connection">The open connection that receives the extension</param>
    /// <returns>Unit, or a <see cref="SqliteVecNotFound"/> error when no search directory holds the loadable, or a <see cref="SqliteVecLoadFailed"/> error when SQLite rejects it</returns>
    public static Fin<Unit> Load(SqliteConnection connection) {
        ArgumentNullException.ThrowIfNull(connection);
        return from path in Locate()
               from _ in Try.lift(() => { connection.LoadExtension(path, EntryPoint); return unit; }).Run().MapFail(error => new SqliteVecLoadFailed(path, error))
               select unit;
    }

    private static Fin<string> Locate() {
        string runtimeIdentifier = RuntimeInformation.RuntimeIdentifier;
        Seq<string> directories = SearchDirectories(runtimeIdentifier);
        return directories.Map(static directory => Path.Combine(directory, FileName)).Filter(File.Exists).Head.ToFin(new SqliteVecNotFound(runtimeIdentifier, directories));
    }

    private static Seq<string> SearchDirectories(string runtimeIdentifier) =>
        HostDirectories() + Roots().Bind(root => Seq(Path.Combine(root, "runtimes", runtimeIdentifier, "native"), root));

    private static Seq<string> HostDirectories() =>
        AppContext.GetData(SearchDirectoriesProperty) is string list
            ? toSeq(list.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            : Seq<string>();

    private static Seq<string> Roots() =>
        Optional(Path.GetDirectoryName(typeof(SqliteVecInterop).Assembly.Location)).ToSeq().Add(AppContext.BaseDirectory);
}
