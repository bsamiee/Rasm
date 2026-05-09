using Rhino.PlugIns;

namespace Radyab;

// --- [PLUGIN] --------------------------------------------------------------------------

/// <summary>
/// Rhino plug-in shell required by <c>HostUtils.CreatePlugIn</c> for the <c>.rhp</c> loader.
/// Components register through <see cref="RadyabLibrary"/> (the Grasshopper2 plugin); this
/// type exists solely to satisfy the Rhino host contract.
/// </summary>
public sealed class RadyabPlugin : PlugIn {
    protected override LoadReturnCode OnLoad(ref string errorMessage) =>
        LoadReturnCode.Success;
}
