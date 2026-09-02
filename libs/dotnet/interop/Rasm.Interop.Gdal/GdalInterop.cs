using MaxRev.Gdal.Core;

namespace Rasm.Interop.Gdal;

/// <summary>Provides the GDAL driver and PROJ registration that executables run once at the composition root</summary>
/// <remarks>
/// <para>Every dataset open fails until <see cref="GdalBase.ConfigureAll"/> runs, repeated and concurrent calls return at once, and a later call retries a failed attempt</para>
/// <para>The native libraries sit in the MaxRev.Gdal runtime packages that each executable references for the RIDs it publishes</para>
/// </remarks>
public static class GdalInterop {
    /// <summary>Registers every GDAL raster and vector driver and sets the GDAL data and PROJ resource paths</summary>
    public static void Initialize() => GdalBase.ConfigureAll();
}
