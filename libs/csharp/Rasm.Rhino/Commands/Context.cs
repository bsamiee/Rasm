using Rasm.Analysis;
using Rasm.Rhino.Camera;
using BlocksApi = Rasm.Rhino.Blocks;

namespace Rasm.Rhino.Commands;

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed class RhinoCommandContext {
    private RhinoCommandContext(RhinoDoc document, RunMode mode, Context domain) {
        Document = document;
        Mode = mode;
        Domain = domain;
        Scope = Analyze.In(context: domain);
        Input = new(document: document, domain: domain);
        Edit = new(document: document, domain: domain);
        Ui = new(document: document, mode: mode);
        Camera = RhinoCamera.Live(document: document, mode: mode);
        Files = Exchange.RhinoFiles.Live(document: document, mode: mode);
        Blocks = BlocksApi.RhinoBlocks.Live(document: document, mode: mode);
    }

    public static Fin<RhinoCommandContext> Of(RhinoDoc doc, RunMode mode) =>
        Optional(doc)
            .ToFin(Fail: Op.Of(name: nameof(RhinoCommandContext)).MissingContext())
            .Bind(document => document.IsReady()
                ? Context.Of(doc: document).ToFin().Map(domain => new RhinoCommandContext(document: document, mode: mode, domain: domain))
                : Fin.Fail<RhinoCommandContext>(error: Op.Of(name: nameof(RhinoCommandContext)).MissingContext()));

    public RhinoDoc Document { get; }
    public RunMode Mode { get; }
    public Context Domain { get; }
    public Analyze.Scope Scope { get; }
    public CommandInput Input { get; }
    public DocumentEdit Edit { get; }
    public UI.RhinoUi Ui { get; }
    public RhinoCamera Camera { get; }
    public Exchange.RhinoFiles Files { get; }
    public BlocksApi.RhinoBlocks Blocks { get; }

}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class RhinoDocState {
    internal static bool IsReady(this RhinoDoc doc) =>
        doc is { IsAvailable: true, IsClosing: false, IsInitializing: false, IsOpening: false, IsCreating: false };
}
