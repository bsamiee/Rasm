namespace Rasm.Rhino.Commands;

public sealed class RhinoCommandContext {
    private RhinoCommandContext(RhinoDoc document, RunMode mode) {
        Document = document;
        Mode = mode;
        Scope = Analyze.From(doc: document);
        Input = new(document: document);
        Edit = new(document: document);
        Ui = new(document: document, mode: mode);
    }

    public RhinoDoc Document { get; }
    public RunMode Mode { get; }
    public Analyze.Scope Scope { get; }
    public CommandInput Input { get; }
    public DocumentEdit Edit { get; }
    public Rasm.Rhino.UI.RhinoUi Ui { get; }

    public static Fin<RhinoCommandContext> Of(RhinoDoc doc, RunMode mode) => Optional(doc).ToFin(Fail: Op.Of(name: nameof(RhinoCommandContext)).MissingContext()).Map(document => new RhinoCommandContext(document: document, mode: mode));

    public Fin<CommandGet<TValue>> Get<TValue>(params CommandInputPolicy[] policies) => Input.Get(request: CommandInputs.Get<TValue>(policies: policies));

    public Fin<RhinoView> View() => Optional(Document.Views.ActiveView).ToFin(Fail: Op.Of(name: nameof(View)).MissingContext());

    public Fin<RhinoView> View(Guid mainViewportId) =>
        mainViewportId switch {
            Guid id when id != Guid.Empty => Optional(Document.Views.Find(mainViewportId: id)).ToFin(Fail: Op.Of(name: nameof(View)).MissingContext()),
            _ => Fin.Fail<RhinoView>(error: Op.Of(name: nameof(View)).InvalidInput()),
        };
}
