using Rasm.Analysis;
using BlocksApi = Rasm.Rhino.Blocks;

namespace Rasm.Rhino.Commands;

// --- [MODELS] -----------------------------------------------------------------------------
public sealed class RhinoCommandContext {
    private RhinoCommandContext(RhinoDoc document, RunMode mode, Context domain) {
        Document = document;
        Mode = mode;
        Domain = domain;
        Scope = Analyze.In(context: domain);
        Input = new(document: document, domain: domain);
        Edit = new(document: document, domain: domain);
        Ui = new(document: document, mode: mode);
        Camera = new(document: document);
        Files = Exchange.RhinoFiles.Live(document: document, mode: mode);
        Blocks = BlocksApi.RhinoBlocks.Live(document: document, mode: mode);
    }

    public RhinoDoc Document { get; }
    public RunMode Mode { get; }
    public Context Domain { get; }
    public Analyze.Scope Scope { get; }
    public CommandInput Input { get; }
    public DocumentEdit Edit { get; }
    public UI.RhinoUi Ui { get; }
    public Camera.RhinoCamera Camera { get; }
    public Exchange.RhinoFiles Files { get; }
    public BlocksApi.RhinoBlocks Blocks { get; }

    public static Fin<RhinoCommandContext> Of(RhinoDoc doc, RunMode mode) =>
        Optional(doc)
            .ToFin(Fail: Op.Of(name: nameof(RhinoCommandContext)).MissingContext())
            .Bind(document => document switch {
                { IsAvailable: true, IsClosing: false, IsInitializing: false, IsOpening: false } =>
                    Context.Of(doc: document).ToFin().Map(domain => new RhinoCommandContext(document: document, mode: mode, domain: domain)),
                _ => Fin.Fail<RhinoCommandContext>(error: Op.Of(name: nameof(RhinoCommandContext)).MissingContext()),
            });

    public Fin<CommandGet<TValue>> Get<TValue>(params CommandInputPolicy[] policies) => Input.Get(request: CommandInputs.Get<TValue>(policies: policies));

    public Fin<RhinoCommandContext> Require(RunMode mode, string? name = null) =>
        (Mode == mode) switch {
            true => Fin.Succ(value: this),
            _ => Fin.Fail<RhinoCommandContext>(error: Op.Of(name: string.IsNullOrWhiteSpace(value: name) ? nameof(Require) : name).InvalidInput()),
        };

}
