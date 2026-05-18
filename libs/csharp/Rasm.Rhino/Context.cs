namespace Rasm.Rhino;

// --- [MODELS] ---------------------------------------------------------------------------
public sealed record RhinoCommandContext(
    RhinoDoc Document,
    RunMode Mode,
    Option<RhinoView> ActiveView,
    Analyze.Scope Scope,
    CommandInput Input,
    DocumentEdit Edit) {
    public static RhinoCommandContext Of(RhinoDoc doc, RunMode mode) {
        ArgumentNullException.ThrowIfNull(argument: doc);
        return new(
            Document: doc,
            Mode: mode,
            ActiveView: Optional(doc.Views.ActiveView),
            Scope: Analyze.From(doc: doc),
            Input: new CommandInput(document: doc),
            Edit: new DocumentEdit(document: doc));
    }
}
