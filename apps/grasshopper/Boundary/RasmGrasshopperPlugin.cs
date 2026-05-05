using Rhino.PlugIns;

namespace Rasm.Grasshopper.Boundary;

// --- [PLUGIN] --------------------------------------------------------------------------

public sealed class RasmGrasshopperPlugin : PlugIn {
    public RasmGrasshopperPlugin() =>
        Instance = this;

    public static RasmGrasshopperPlugin? Instance { get; private set; }
}
