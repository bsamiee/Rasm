using PureHDF.Filters;

namespace Rasm.Interop.Hdf5;

/// <summary>Provides the PureHDF external filter registration that executables run once at the composition root</summary>
/// <remarks>
/// <para>Compressed chunk reads and writes throw until their filter enters the process-global <see cref="H5Filter"/> registry, and a repeated <see cref="H5Filter.Register"/> call overwrites the entry</para>
/// <para>The Blosc2 filter loads libblosc2 on first use</para>
/// </remarks>
public static class Hdf5Interop {
    /// <summary>Registers the Blosc2, BZip2, and LZF compression filters with PureHDF</summary>
    public static void Initialize() {
        H5Filter.Register(new Blosc2Filter());
        H5Filter.Register(new BZip2SharpZipLibFilter());
        H5Filter.Register(new LzfFilter());
    }
}
