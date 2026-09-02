using Rasm.Interop.Excel;
using Rasm.Interop.Gdal;
using Rasm.Interop.Hdf5;
using Rasm.Interop.Pdf;

namespace Rasm.Interop;

/// <summary>Aggregate runtime initialization. One call at the composition root covers every interop facade.</summary>
/// <remarks>
/// <para>Repeated calls are harmless, and no library poisons state when it fails before initialization:
/// retry after a late <see cref="Initialize"/> succeeds.</para>
/// <para>Libraries without a facade keep their setup at the composition root: PuppeteerSharp routes its browser
/// cache under the workspace .cache/ directory through BrowserFetcherOptions.Path with a pinned browser build;
/// FFmpeg needs ffmpeg.RootPath on the legacy FFmpeg.AutoGen facade and DynamicallyLoadedBindings.Initialize()
/// on the modern Abstractions facade.</para>
/// <para>Per-connection registrations belong to the composition root and get no facade:</para>
/// <list type="bullet">
/// <item><description>DuckDB: SET extension_directory to the Rasm.Native.DuckDBExtensions contentFiles payload at
/// duckdb_extensions/ under the application base directory, then LOAD each extension per connection, autoinstall
/// and autoload off</description></item>
/// <item><description>sqlite-vec: every open SqliteConnection calls LoadExtension with the packaged vec0 library
/// before its first vector query</description></item>
/// <item><description>Npgsql: NpgsqlDataSourceBuilder registers UseNetTopologySuite, UseNodaTime, and UseVector,
/// and Pgvector needs UseVector on both the data source builder and the EF Core options</description></item>
/// </list>
/// </remarks>
public static class RuntimeInitialization {
    /// <summary>Runs every interop facade initialization.</summary>
    public static void Initialize() {
        ExcelInterop.Initialize();
        GdalInterop.Initialize();
        Hdf5Interop.Initialize();
        PdfInterop.Initialize();
    }
}
