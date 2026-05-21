using Rasm.Rhino.Commands;
using Rasm.Rhino.UI;

namespace Rasm.Rhino.Exchange;

// --- [MODELS] -----------------------------------------------------------------------------
internal readonly record struct FileRuntime(
    Option<RhinoDoc> Document,
    RunMode Mode,
    Option<Context> Domain,
    Option<DocumentEdit> Edit,
    Option<RhinoUi> Ui);

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed class RhinoFiles {
    private readonly Option<RhinoDoc> document;
    private readonly RunMode mode;

    private RhinoFiles(Option<RhinoDoc> document, RunMode mode) =>
        (this.document, this.mode) = (document, mode);

    public static RhinoFiles Live(RhinoDoc document, RunMode mode = RunMode.Interactive) =>
        new(document: Optional(document), mode: mode);

    public static RhinoFiles Offline(RunMode mode = RunMode.Scripted) =>
        new(document: Option<RhinoDoc>.None, mode: mode);

    public Fin<T> Run<T>(FileOp<T> operation) =>
        from active in Runtime()
        from valid in Optional(operation).ToFin(Fail: Op.Of(name: nameof(Run)).InvalidInput())
        from result in valid.Apply(runtime: active)
        select result;

    private Fin<FileRuntime> Runtime() =>
        document.Case switch {
            RhinoDoc value => LiveRuntime(document: value, mode: mode),
            _ => Fin.Succ(value: new FileRuntime(
                Document: Option<RhinoDoc>.None,
                Mode: mode,
                Domain: Option<Context>.None,
                Edit: Option<DocumentEdit>.None,
                Ui: Option<RhinoUi>.None)),
        };

    private static Fin<FileRuntime> LiveRuntime(RhinoDoc document, RunMode mode) =>
        document switch {
            { IsAvailable: true, IsClosing: false, IsInitializing: false, IsOpening: false } active =>
                Context.Of(doc: active).ToFin().Map(domain => new FileRuntime(
                    Document: Some(active),
                    Mode: mode,
                    Domain: Some(domain),
                    Edit: Some(new DocumentEdit(document: active, domain: domain)),
                    Ui: Some(new RhinoUi(document: active, mode: mode)))),
            _ => Fin.Fail<FileRuntime>(error: Op.Of(name: nameof(RhinoFiles)).MissingContext()),
        };
}
