using PureHDF.Filters;

namespace Rasm.Interop.Hdf5;

/// <summary>PureHDF external filter registration. Executables call <see cref="Initialize"/> once at the composition root.</summary>
/// <remarks>
/// Reading or writing a BZip2- or LZF-compressed chunk throws until the matching filter enters the process-global
/// <see cref="H5Filter"/> registry. <see cref="H5Filter.Register"/> stores each filter under its identifier with an
/// overwriting AddOrUpdate, and repeated registration replaces the entry instead of throwing.
/// </remarks>
public static class Hdf5Interop {
    /// <summary>Registers the BZip2 and LZF compression filters with PureHDF.</summary>
    public static void Initialize() {
        H5Filter.Register(new BZip2SharpZipLibFilter());
        H5Filter.Register(new LzfFilter());
    }
}
