using Rhino;
using Rhino.Commands;

namespace Rasm.Rhino.Boundary;

// --- [COMMANDS] ------------------------------------------------------------------------

public sealed class RasmStatusCommand : Command {
    public override string EnglishName => "RasmStatus";

    protected override Result RunCommand(RhinoDoc doc, RunMode mode) {
        ArgumentNullException.ThrowIfNull(doc);

        return RasmRhinoPlugin.Instance switch {
            RasmRhinoPlugin plugin => plugin.ReportStatus(doc: doc, mode: mode),
            _ => Result.Failure,
        };
    }
}
