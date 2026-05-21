using Rasm.Rhino.Commands;
using Rasm.Rhino.UI;

namespace Rasm.Rhino.Exchange;

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct FileRuntime {
    internal FileRuntime(Option<RhinoDoc> document, RunMode mode, Option<Context> domain, Option<DocumentEdit> edit, Option<RhinoUi> ui) =>
        (Document, Mode, Domain, Edit, Ui) = (document, mode, domain, edit, ui);

    internal Option<RhinoDoc> Document { get; }
    internal RunMode Mode { get; }
    internal Option<Context> Domain { get; }
    internal Option<DocumentEdit> Edit { get; }
    internal Option<RhinoUi> Ui { get; }
}

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

    public Fin<T> Run<T>(Eff<FileRuntime, T> operation) =>
        Runtime().Bind(active => operation.Run(active));

    private Fin<FileRuntime> Runtime() =>
        document.Case switch {
            RhinoDoc value => LiveRuntime(document: value, mode: mode),
            _ => Fin.Succ(value: new FileRuntime(
                document: Option<RhinoDoc>.None,
                mode: mode,
                domain: Option<Context>.None,
                edit: Option<DocumentEdit>.None,
                ui: Option<RhinoUi>.None)),
        };

    private static Fin<FileRuntime> LiveRuntime(RhinoDoc document, RunMode mode) =>
        document switch {
            { IsAvailable: true, IsClosing: false, IsInitializing: false, IsOpening: false } active =>
                Context.Of(doc: active).ToFin().Map(domain => new FileRuntime(
                    document: Some(active),
                    mode: mode,
                    domain: Some(domain),
                    edit: Some(new DocumentEdit(document: active, domain: domain)),
                    ui: Some(new RhinoUi(document: active, mode: mode)))),
            _ => Fin.Fail<FileRuntime>(error: Op.Of(name: nameof(RhinoFiles)).MissingContext()),
        };
}
