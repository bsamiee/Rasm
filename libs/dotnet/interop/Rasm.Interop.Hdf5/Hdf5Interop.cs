using PureHDF.Filters;

namespace Rasm.Interop.Hdf5;

/// <summary>PureHDF external filter registration; executables call <see cref="Initialize"/> once at the composition root</summary>
/// <remarks>
/// Reading or writing a Blosc2-, BZip2-, or LZF-compressed chunk throws until the matching filter enters the process-global <see cref="H5Filter"/> registry; <see cref="H5Filter.Register"/> stores
/// each filter under its identifier with an overwriting AddOrUpdate, repeated registration replaces the entry instead of throwing; the Blosc2 filter loads the Rasm.Native.Blosc2 libblosc2 library on first use
/// </remarks>
public static class Hdf5Interop {
    /// <summary>Registers the Blosc2, BZip2, and LZF compression filters with PureHDF</summary>
    public static void Initialize() {
        H5Filter.Register(new Blosc2Filter());
        H5Filter.Register(new BZip2SharpZipLibFilter());
        H5Filter.Register(new LzfFilter());
    }
}
