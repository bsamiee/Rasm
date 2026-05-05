using Rhino.PlugIns;

namespace Rasm.Rhino.Boundary;

// --- [PLUGIN] --------------------------------------------------------------------------

public sealed class RasmRhinoPlugin : PlugIn {
    public RasmRhinoPlugin() =>
        Instance = this;

    public static RasmRhinoPlugin? Instance { get; private set; }
}
