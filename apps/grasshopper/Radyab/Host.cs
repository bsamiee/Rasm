using Rhino.PlugIns;

namespace Radyab;

// --- [EXPORTS] -------------------------------------------------------------------------

/// <summary>
/// Rhino plug-in shell required by HostUtils.CreatePlugIn for the .rhp loader. Components register
/// through <see cref="Library"/> (the Grasshopper2 plugin); this type exists solely to satisfy the
/// Rhino host contract.
/// </summary>
public sealed class Host : PlugIn {
    protected override LoadReturnCode OnLoad(ref string errorMessage) =>
        LoadReturnCode.Success;
}
