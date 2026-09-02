using Rasm.Interop.Excel;
using Rasm.Interop.Gdal;
using Rasm.Interop.Hdf5;
using Rasm.Interop.OpenCv;
using Rasm.Interop.Pdf;

namespace Rasm.Interop;

/// <summary>Provides the initialization of every interop facade that executables run once at the composition root</summary>
/// <remarks>
/// <para>Repeated calls are harmless, and a later call retries a failed one because no facade keeps state from a failed attempt, except OpenCV, where the CLR caches the failed type initializer</para>
/// <para>Libraries without a facade keep their process-global, per-connection, and per-container setup at the composition root:</para>
/// <list type="bullet">
/// <item><description>PuppeteerSharp: set BrowserFetcherOptions.Path under the workspace .cache/ directory and pin the browser build</description></item>
/// <item><description>Plotly.NET.ImageExport: set PuppeteerSharpRendererOptions.localBrowserExecutablePath from a composition-root download, its own BrowserFetcher ignores BrowserFetcherOptions</description></item>
/// <item><description>FFmpeg: set ffmpeg.RootPath on the legacy FFmpeg.AutoGen facade and call DynamicallyLoadedBindings.Initialize() on the Abstractions facade</description></item>
/// <item><description>AWS: register clients through AddDefaultAWSOptions(configuration.GetAWSOptions()) then AddAWSService&lt;T&gt;(), bound from the AWS configuration section</description></item>
/// <item><description>DuckDB: SET extension_directory to duckdb_extensions/ under the application base directory, then LOAD each extension per connection with autoinstall and autoload off</description></item>
/// <item><description>sqlite-vec: call LoadExtension with the Rasm.Native.SqliteVec vec0 library on every open SqliteConnection before its first vector query</description></item>
/// <item><description>Npgsql: call UseNetTopologySuite, UseNodaTime, and UseVector on NpgsqlDataSourceBuilder, and Pgvector needs UseVector on the EF Core options too</description></item>
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
