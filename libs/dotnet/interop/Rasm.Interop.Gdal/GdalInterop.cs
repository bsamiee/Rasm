using MaxRev.Gdal.Core;

namespace Rasm.Interop.Gdal;

/// <summary>GDAL driver and PROJ registration for executables to call once at the composition root</summary>
/// <remarks>
/// Every dataset open fails until <see cref="GdalBase.ConfigureAll"/> registers the raster and vector drivers and points PROJ at its resource database.
/// Double-checked locking returns repeated and concurrent calls immediately. The completion flag sets only after success, leaving a failed
/// attempt retryable. The facade references MaxRev.Gdal.Core alone because the native libraries travel in the runtime packages
/// MaxRev.Gdal.MacosRuntime.Minimal.arm64, MaxRev.Gdal.LinuxRuntime.Minimal, and MaxRev.Gdal.WindowsRuntime.Minimal, referenced by each executable to
/// match the RIDs it publishes for
/// </remarks>
public static class GdalInterop {
    /// <summary>Registers every GDAL raster and vector driver and configures the PROJ database path</summary>
    public static void Initialize() => GdalBase.ConfigureAll();
}
