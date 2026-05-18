namespace Rasm.Rhino;

// --- [MODELS] ---------------------------------------------------------------------------
public sealed class RhinoCommandContext {
    private RhinoCommandContext(RhinoDoc document, RunMode mode) {
        Document = document;
        Mode = mode;
        ActiveView = Optional(document.Views.ActiveView);
        Scope = Analyze.From(doc: document);
        Input = new(document: document);
        Edit = new(document: document);
    }

    public RhinoDoc Document { get; }
    public RunMode Mode { get; }
    public Option<RhinoView> ActiveView { get; }
    public Analyze.Scope Scope { get; }
    public CommandInput Input { get; }
    public DocumentEdit Edit { get; }

    public static RhinoCommandContext Of(RhinoDoc doc, RunMode mode) {
        ArgumentNullException.ThrowIfNull(argument: doc);
        return new(document: doc, mode: mode);
    }
}
