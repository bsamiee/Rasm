using Rhino;
using Rhino.Commands;
using Rhino.PlugIns;

namespace Rasm.Rhino.Boundary;

// --- [PLUGIN] --------------------------------------------------------------------------

public sealed class RasmRhinoPlugin : PlugIn {
    public RasmRhinoPlugin() =>
        Instance = this;

    public static RasmRhinoPlugin? Instance { get; private set; }

    protected override LoadReturnCode OnLoad(ref string errorMessage) {
        RhinoApp.WriteLine("Rasm Rhino loaded. Settings directory: {0}", SettingsDirectory);
        return LoadReturnCode.Success;
    }

    internal Result ReportStatus(RhinoDoc doc, RunMode mode) {
        _ = Directory.CreateDirectory(path: SettingsDirectory);
        Settings.SetString(key: "RuntimeTarget", value: "RhinoWIP macOS");
        Settings.SetString(key: "RuntimeSurface", value: "RhinoCommon");
        SaveSettings();
        RhinoApp.WriteLine(
            "Rasm Rhino status: mode={0}; document={1}; settings={2}",
            mode,
            doc.RuntimeSerialNumber,
            SettingsDirectory);
        return Result.Success;
    }
}
