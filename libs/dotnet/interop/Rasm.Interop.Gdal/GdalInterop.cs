using MaxRev.Gdal.Core;

namespace Rasm.Interop.Gdal;

/// <summary>GDAL driver and PROJ registration for executables to call once at the composition root</summary>
/// <remarks>
/// Every dataset open fails until <see cref="GdalBase.ConfigureAll"/> runs. Double-checked locking returns repeated and concurrent calls at once, and the completion flag sets only after success, a failed attempt retries.
/// The facade references MaxRev.Gdal.Core alone, the native libraries sit in the runtime packages MaxRev.Gdal.MacosRuntime.Minimal.arm64, MaxRev.Gdal.LinuxRuntime.Minimal, and MaxRev.Gdal.WindowsRuntime.Minimal that each executable references for the RIDs it publishes
/// </remarks>
public static class GdalInterop {
    /// <summary>Registers every GDAL raster and vector driver and points PROJ at its resource database</summary>
    public static void Initialize() => GdalBase.ConfigureAll();
}
