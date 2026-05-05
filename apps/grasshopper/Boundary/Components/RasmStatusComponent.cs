using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using GrasshopperIO;

namespace Rasm.Grasshopper.Boundary.Components;

// --- [COMPONENTS] ----------------------------------------------------------------------

[IoId("ff8d99a5-d1e0-47c9-a0b0-cbee6f542d3c")]
public sealed class RasmStatusComponent : Component {
    public RasmStatusComponent()
        : base(new Nomen(
            "Rasm Status",
            "Reports Rasm Grasshopper runtime status.",
            "Rasm",
            "Core")) { }

    public RasmStatusComponent(IReader reader)
        : base(reader: reader) { }

    protected override void AddInputs(InputAdder inputs) { }

    protected override void AddOutputs(OutputAdder outputs) {
        ArgumentNullException.ThrowIfNull(outputs);
        _ = outputs.AddText(
            "Status",
            "S",
            "Runtime status.",
            Access.Item);
    }

    protected override void Process(IDataAccess access) {
        ArgumentNullException.ThrowIfNull(access);
        access.SetItem(0, "Rasm Grasshopper loaded.");
    }
}
