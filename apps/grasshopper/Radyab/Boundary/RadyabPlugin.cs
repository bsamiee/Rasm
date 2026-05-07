using Rhino;
using Rhino.PlugIns;

namespace Radyab.Boundary;

// --- [PLUGIN] --------------------------------------------------------------------------

public sealed class RadyabPlugin : PlugIn {
    protected override LoadReturnCode OnLoad(ref string errorMessage) {
        RhinoApp.WriteLine("Radyab loaded. Settings directory: {0}", SettingsDirectory);
        return LoadReturnCode.Success;
    }
}
