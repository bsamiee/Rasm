using Rhino;
using Rhino.PlugIns;

namespace Rasm.Grasshopper.Boundary;

// --- [PLUGIN] --------------------------------------------------------------------------

public sealed class RasmGrasshopperPlugin : PlugIn {
    public RasmGrasshopperPlugin() =>
        Instance = this;

    public static RasmGrasshopperPlugin? Instance { get; private set; }

    protected override LoadReturnCode OnLoad(ref string errorMessage) {
        RhinoApp.WriteLine("Rasm Grasshopper loaded. Settings directory: {0}", SettingsDirectory);
        return LoadReturnCode.Success;
    }

    internal string ReportStatus() {
        _ = Directory.CreateDirectory(path: SettingsDirectory);
        Settings.SetString(key: "RuntimeTarget", value: "RhinoWIP macOS");
        Settings.SetString(key: "RuntimeSurface", value: "Grasshopper2");
        SaveSettings();
        return $"Rasm Grasshopper loaded. Settings: {SettingsDirectory}";
    }
}
