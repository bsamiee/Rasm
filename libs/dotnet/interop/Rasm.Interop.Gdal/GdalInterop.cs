using MaxRev.Gdal.Core;

namespace Rasm.Interop.Gdal;

/// <summary>GDAL driver and PROJ registration. Executables call <see cref="Initialize"/> once at the composition root.</summary>
/// <remarks>
/// Every dataset open fails until <see cref="GdalBase.ConfigureAll"/> registers the raster and vector drivers and
/// points PROJ at its resource database. Double-checked locking makes repeated and concurrent calls return
/// immediately, and the completion flag sets only after success, leaving a failed attempt retryable. The facade
/// references MaxRev.Gdal.Core alone: the native libraries travel in the runtime packages
/// MaxRev.Gdal.MacosRuntime.Minimal.arm64, MaxRev.Gdal.LinuxRuntime.Minimal, and MaxRev.Gdal.WindowsRuntime.Minimal,
/// and each executable references the packages matching the RIDs it publishes for.
/// </remarks>
public static class GdalInterop {
    /// <summary>Registers every GDAL raster and vector driver and configures the PROJ database path.</summary>
    public static void Initialize() => GdalBase.ConfigureAll();
}
