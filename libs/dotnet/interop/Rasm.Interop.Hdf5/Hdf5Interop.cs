using PureHDF.Filters;

namespace Rasm.Interop.Hdf5;

/// <summary>PureHDF external filter registration for executables to call once at the composition root</summary>
/// <remarks>
/// Reading or writing a Blosc2-, BZip2-, or LZF-compressed chunk throws until the matching filter enters the process-global <see cref="H5Filter"/> registry. <see cref="H5Filter.Register"/> stores each filter under its identifier through AddOrUpdate, repeated registration overwrites the entry instead of throwing. The Blosc2 filter loads libblosc2 from Rasm.Native.Blosc2 on first use
/// </remarks>
public static class Hdf5Interop {
    /// <summary>Registers the Blosc2, BZip2, and LZF compression filters with PureHDF</summary>
    public static void Initialize() {
        H5Filter.Register(new Blosc2Filter());
        H5Filter.Register(new BZip2SharpZipLibFilter());
        H5Filter.Register(new LzfFilter());
    }
}
