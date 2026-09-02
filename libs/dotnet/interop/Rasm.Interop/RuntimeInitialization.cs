using Rasm.Interop.Excel;
using Rasm.Interop.Gdal;
using Rasm.Interop.Hdf5;
using Rasm.Interop.OpenCv;
using Rasm.Interop.Pdf;

namespace Rasm.Interop;

/// <summary>Aggregate runtime initialization, called once at the composition root for every interop facade</summary>
/// <remarks>
/// <para>Repeated calls are harmless, and a failed call retries after a late <see cref="Initialize"/> because failure before initialization poisons no state, except in OpenCV, where the CLR caches the failed type initializer for the process lifetime</para>
/// <para>Libraries without a facade keep their setup at the composition root. PuppeteerSharp routes its browser cache under the workspace .cache/ directory through BrowserFetcherOptions.Path with a pinned browser build. Plotly.NET.ImageExport constructs its own BrowserFetcher, ignores BrowserFetcherOptions, and takes only PuppeteerSharpRendererOptions.localBrowserExecutablePath set from a composition-root download, and chart data comes from named or static readonly arrays because CA1861 fails inline array literals in chart calls under warnings-as-errors. FFmpeg needs ffmpeg.RootPath on the legacy FFmpeg.AutoGen facade and DynamicallyLoadedBindings.Initialize() on the Abstractions facade</para>
/// <para>Per-connection and per-container registrations belong to the composition root and get no facade:</para>
/// <list type="bullet">
/// <item><description>AWS: register clients through AddDefaultAWSOptions(configuration.GetAWSOptions()) then AddAWSService&lt;T&gt;(), bound from the AWS configuration section</description></item>
/// <item><description>DuckDB: SET extension_directory to the Rasm.Native.DuckDBExtensions contentFiles directory duckdb_extensions/ under the application base directory, then LOAD each extension per connection with autoinstall and autoload off</description></item>
/// <item><description>sqlite-vec: every open SqliteConnection calls LoadExtension with the Rasm.Native.SqliteVec vec0 library before its first vector query</description></item>
/// <item><description>Npgsql: NpgsqlDataSourceBuilder registers UseNetTopologySuite, UseNodaTime, and UseVector, and Pgvector needs UseVector on both the data source builder and the EF Core options</description></item>
/// </list>
/// </remarks>
public static class RuntimeInitialization {
    /// <summary>Runs every interop facade initialization</summary>
    public static void Initialize() {
        ExcelInterop.Initialize();
        GdalInterop.Initialize();
        Hdf5Interop.Initialize();
        OpenCvInterop.Initialize();
        PdfInterop.Initialize();
    }
}
