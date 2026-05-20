using Rasm.Rhino.Commands;
using Rasm.Rhino.UI;
using Rhino.Collections;

namespace Rasm.Rhino.Exchange;

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record FileOp<T> {
    private readonly Func<FileRuntime, Fin<T>> run;

    private FileOp(Func<FileRuntime, Fin<T>> run) =>
        this.run = run;

    internal static FileOp<T> Of(Func<FileRuntime, Fin<T>> run) =>
        new(run: run);

    internal Fin<T> Apply(FileRuntime runtime) =>
        run(arg: runtime);

    public FileOp<TNext> Map<TNext>(Func<T, TNext> project) =>
        FileOp<TNext>.Of(run: runtime =>
            from value in Apply(runtime: runtime)
            from valid in Optional(project).ToFin(Fail: Op.Of(name: nameof(Map)).InvalidInput())
            select valid(arg: value));

    public FileOp<TNext> Bind<TNext>(Func<T, FileOp<TNext>> bind) =>
        FileOp<TNext>.Of(run: runtime =>
            from value in Apply(runtime: runtime)
            from valid in Optional(bind).ToFin(Fail: Op.Of(name: nameof(Bind)).InvalidInput())
            from next in Optional(valid(arg: value)).ToFin(Fail: Op.Of(name: nameof(Bind)).InvalidInput())
            from result in next.Apply(runtime: runtime)
            select result);
}

public abstract record FileExchange {
    private FileExchange() { }
    public sealed record Open(FileEndpoint Source, FileProfile Profile) : FileExchange;
    public sealed record Import(FileEndpoint Source, FileProfile Profile) : FileExchange;
    public sealed record Export(FileEndpoint Target, DocumentTarget Objects, FileProfile Profile) : FileExchange;
    public sealed record Save(FileProfile Profile) : FileExchange;
    public sealed record SaveAs(FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record WriteFile(FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record Write3dmFile(FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record SaveTemplate(FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record ArchiveRead(FileEndpoint Source, ArchiveProfile Profile) : FileExchange;
    public sealed record ArchiveExtract(FileEndpoint Source, FileEndpoint Target, ArchiveProfile Profile) : FileExchange;
    public sealed record ArchiveInsert(FileEndpoint Source, FileEndpoint Embedded, FileEndpoint Target, ArchiveProfile Profile) : FileExchange;
    public sealed record Batch(Seq<FileExchange> Items, FileBatchPolicy Policy) : FileExchange;
}

public abstract record HeadlessExchange {
    private HeadlessExchange() { }
    public sealed record CreateFromTemplate(FileEndpoint Template) : HeadlessExchange;
    public sealed record Open(FileEndpoint Source, FileProfile Profile) : HeadlessExchange;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class FileOp {
    public static FileOp<FileReport> Do(FileExchange exchange) =>
        FileOp<FileReport>.Of(run: runtime =>
            from active in Optional(exchange).ToFin(Fail: Op.Of(name: nameof(Do)).InvalidInput())
            from result in active switch {
                FileExchange.Open open => Open(source: open.Source, profile: open.Profile),
                FileExchange.Import import => Import(runtime: runtime, source: import.Source, profile: import.Profile),
                FileExchange.Export export => Export(runtime: runtime, target: export.Target, objects: export.Objects, profile: export.Profile),
                FileExchange.Save save => Save(runtime: runtime, profile: save.Profile),
                FileExchange.SaveAs saveAs => SaveAs(runtime: runtime, target: saveAs.Target, profile: saveAs.Profile),
                FileExchange.WriteFile write => WriteFile(runtime: runtime, target: write.Target, profile: write.Profile),
                FileExchange.Write3dmFile write => Write3dmFile(runtime: runtime, target: write.Target, profile: write.Profile),
                FileExchange.SaveTemplate template => SaveTemplate(runtime: runtime, target: template.Target, profile: template.Profile),
                FileExchange.ArchiveRead archive => FileArchiveOps.Read(source: archive.Source, profile: archive.Profile),
                FileExchange.ArchiveExtract archive => FileArchiveOps.Extract(source: archive.Source, target: archive.Target, profile: archive.Profile),
                FileExchange.ArchiveInsert archive => FileArchiveOps.Insert(source: archive.Source, embedded: archive.Embedded, target: archive.Target, profile: archive.Profile),
                FileExchange.Batch batch => Batch(runtime: runtime, items: batch.Items, policy: batch.Policy),
                _ => Fin.Fail<FileReport>(error: Op.Of(name: nameof(Do)).InvalidInput()),
            }
            select result);

    public static FileOp<T> Headless<T>(HeadlessExchange scope, FileOp<T> body) =>
        FileOp<T>.Of(run: runtime =>
            from active in Optional(scope).ToFin(Fail: Op.Of(name: nameof(Headless)).InvalidInput())
            from operation in Optional(body).ToFin(Fail: Op.Of(name: nameof(Headless)).InvalidInput())
            from result in HeadlessScope(scope: active, body: operation)
            select result);

    public static FileOp<Seq<FileEndpoint>> Prompt(FilePrompt prompt) =>
        FileOp<Seq<FileEndpoint>>.Of(run: runtime =>
            from active in Optional(prompt).ToFin(Fail: Op.Of(name: nameof(Prompt)).InvalidInput())
            from ui in runtime.Ui.ToFin(Fail: Op.Of(name: nameof(Prompt)).MissingContext())
            from result in ui.Use(intent: UiIntent.ExchangeFile(prompt: active))
            select result);

    private static Fin<FileReport> Open(FileEndpoint source, FileProfile profile) =>
        from endpoint in source.Input(op: Op.Of(name: nameof(Open)))
        from report in Try.lift<Fin<FileReport>>(f: () => {
            RhinoDoc? opened = RhinoDoc.Open(filePath: endpoint.Path, wasAlreadyOpen: out bool wasAlreadyOpen);
            return Optional(opened)
                .ToFin(Fail: Op.Of(name: nameof(Open)).InvalidResult())
                .Map(document => FileReport.Of(
                    phase: FilePhase.Open,
                    source: Some(endpoint),
                    target: Option<FileEndpoint>.None,
                    format: FileFormatProjection.Resolve(endpoint: endpoint, profile: profile),
                    issues: wasAlreadyOpen ? Seq(new FileIssue(Code: "already-open", Message: "Document was already open.")) : Seq<FileIssue>()));
        }).Run().MapFail(_ => Op.Of(name: nameof(Open)).InvalidResult()).Bind(static result => result)
        select report;

    private static Fin<FileReport> Import(FileRuntime runtime, FileEndpoint source, FileProfile profile) =>
        from live in Live(runtime: runtime, op: Op.Of(name: nameof(Import)))
        from endpoint in source.Input(op: Op.Of(name: nameof(Import)))
        from receipt in live.Edit.Commit(
            name: nameof(Import),
            redraw: DocumentRedraw.After,
            undoRecorded: true,
            run: (document, _, op) =>
                from before in DocumentEdit.LiveObjectIds(document: document)
                from imported in FileFormatProjection.Import(document: document, source: endpoint, profile: profile, op: op)
                from after in DocumentEdit.LiveObjectIds(document: document)
                select DocumentReceipt.Empty with { Created = after.Filter(id => !before.Exists(item => item == id)) })
        select FileReport.Of(phase: FilePhase.Import, source: Some(endpoint), target: Option<FileEndpoint>.None, format: FileFormatProjection.Resolve(endpoint: endpoint, profile: profile), receipt: Some(receipt));

    private static Fin<FileReport> Export(FileRuntime runtime, FileEndpoint target, DocumentTarget objects, FileProfile profile) =>
        from live in Live(runtime: runtime, op: Op.Of(name: nameof(Export)))
        from endpoint in target.Output(op: Op.Of(name: nameof(Export)))
        from selection in Optional(objects).ToFin(Fail: Op.Of(name: nameof(Export)).InvalidInput())
        from receipt in live.Edit.Commit(
            name: nameof(Export),
            redraw: DocumentRedraw.None,
            undoRecorded: false,
            run: (document, _, op) => ExportTarget(document: document, target: selection, endpoint: endpoint, profile: profile, op: op))
        select FileReport.Of(phase: FilePhase.Export, source: Option<FileEndpoint>.None, target: Some(endpoint), format: FileFormatProjection.Resolve(endpoint: endpoint, profile: profile), receipt: Some(receipt));

    private static Fin<FileReport> Save(FileRuntime runtime, FileProfile profile) =>
        from live in Live(runtime: runtime, op: Op.Of(name: nameof(Save)))
        from receipt in live.Edit.Commit(
            name: nameof(Save),
            redraw: DocumentRedraw.None,
            undoRecorded: false,
            run: (document, _, op) => TryUnit(run: document.Save, op: op).Map(_ => DocumentReceipt.Empty))
        select FileReport.Of(phase: FilePhase.Save, format: profile.Format, receipt: Some(receipt));

    private static Fin<FileReport> SaveAs(FileRuntime runtime, FileEndpoint target, FileProfile profile) =>
        from live in Live(runtime: runtime, op: Op.Of(name: nameof(SaveAs)))
        from endpoint in target.WithFormat(format: FileFormat.ThreeDm).Output(op: Op.Of(name: nameof(SaveAs)))
        from receipt in live.Edit.Commit(
            name: nameof(SaveAs),
            redraw: DocumentRedraw.None,
            undoRecorded: false,
            run: (document, _, op) => FileFormatProjection.SaveAs(document: document, target: endpoint, profile: profile, op: op).Map(_ => DocumentReceipt.Empty))
        select FileReport.Of(phase: FilePhase.SaveAs, target: Some(endpoint), format: FileFormatProjection.Resolve(endpoint: endpoint, profile: profile), receipt: Some(receipt));

    private static Fin<FileReport> WriteFile(FileRuntime runtime, FileEndpoint target, FileProfile profile) =>
        from live in Live(runtime: runtime, op: Op.Of(name: nameof(WriteFile)))
        from endpoint in target.Output(op: Op.Of(name: nameof(WriteFile)))
        from receipt in live.Edit.Commit(
            name: nameof(WriteFile),
            redraw: DocumentRedraw.None,
            undoRecorded: false,
            run: (document, _, op) => FileFormatProjection.WriteFile(document: document, target: endpoint, profile: profile, selected: false, updatePath: false, op: op).Map(_ => DocumentReceipt.Empty))
        select FileReport.Of(phase: FilePhase.WriteFile, target: Some(endpoint), format: FileFormatProjection.Resolve(endpoint: endpoint, profile: profile), receipt: Some(receipt));

    private static Fin<FileReport> Write3dmFile(FileRuntime runtime, FileEndpoint target, FileProfile profile) =>
        from live in Live(runtime: runtime, op: Op.Of(name: nameof(Write3dmFile)))
        from endpoint in target.WithFormat(format: FileFormat.ThreeDm).Output(op: Op.Of(name: nameof(Write3dmFile)))
        from receipt in live.Edit.Commit(
            name: nameof(Write3dmFile),
            redraw: DocumentRedraw.None,
            undoRecorded: false,
            run: (document, _, op) => FileFormatProjection.Write3dmFile(document: document, target: endpoint, profile: profile, selected: false, updatePath: false, op: op).Map(_ => DocumentReceipt.Empty))
        select FileReport.Of(phase: FilePhase.Write3dmFile, target: Some(endpoint), format: FileFormatProjection.Resolve(endpoint: endpoint, profile: profile), receipt: Some(receipt));

    private static Fin<FileReport> SaveTemplate(FileRuntime runtime, FileEndpoint target, FileProfile profile) =>
        from live in Live(runtime: runtime, op: Op.Of(name: nameof(SaveTemplate)))
        from endpoint in target.WithFormat(format: FileFormat.ThreeDm).Output(op: Op.Of(name: nameof(SaveTemplate)))
        from receipt in live.Edit.Commit(
            name: nameof(SaveTemplate),
            redraw: DocumentRedraw.None,
            undoRecorded: false,
            run: (document, _, op) => FileFormatProjection.SaveTemplate(document: document, target: endpoint, op: op).Map(_ => DocumentReceipt.Empty))
        select FileReport.Of(phase: FilePhase.SaveTemplate, target: Some(endpoint), format: FileFormatProjection.Resolve(endpoint: endpoint, profile: profile), receipt: Some(receipt));

    private static Fin<FileReport> Batch(FileRuntime runtime, Seq<FileExchange> items, FileBatchPolicy policy) =>
        items switch {
            Seq<FileExchange> values when !values.IsEmpty => values.Fold(
                Fin.Succ(value: Seq<FileReport>()),
                (reports, exchange) => policy.ContinueOnError switch {
                    false => from state in reports from report in Do(exchange: exchange).Apply(runtime: runtime) select state + Seq(report),
                    true => reports.Bind(state => Do(exchange: exchange).Apply(runtime: runtime).Match(
                        Succ: report => Fin.Succ(value: state + Seq(report)),
                        Fail: error => Fin.Succ(value: state + Seq(FileReport.Empty(phase: FilePhase.Batch) with { Issues = Seq(new FileIssue(Code: error.Category(), Message: error.Message)) })))),
                }).Map(reports => FileReport.Of(phase: FilePhase.Batch, issues: reports.Bind(static report => report.Issues))),
            _ => Fin.Fail<FileReport>(error: Op.Of(name: nameof(Batch)).InvalidInput()),
        };

    private static Fin<T> HeadlessScope<T>(HeadlessExchange scope, FileOp<T> body) =>
        Try.lift<Fin<T>>(f: () => {
            RhinoDoc? headless = null;
            // BOUNDARY ADAPTER — headless RhinoDoc is native disposable document state.
            try {
                Fin<RhinoDoc> opened = scope switch {
                    HeadlessExchange.CreateFromTemplate create =>
                        from template in create.Template.Input(op: Op.Of(name: nameof(Headless)))
                        from document in Optional(RhinoDoc.CreateHeadless(file3dmTemplatePath: template.Path)).ToFin(Fail: Op.Of(name: nameof(Headless)).InvalidResult())
                        select document,
                    HeadlessExchange.Open open =>
                        from source in open.Source.Input(op: Op.Of(name: nameof(Headless)))
                        let options = FileFormatProjection.Dictionary(endpoint: source, profile: open.Profile, phase: FilePhase.Open, op: Op.Of(name: nameof(Headless))).IfFail(_ => new ArchivableDictionary())
                        from document in Optional(RhinoDoc.OpenHeadless(filePath: source.Path, options: options)).ToFin(Fail: Op.Of(name: nameof(Headless)).InvalidResult())
                        select document,
                    _ => Fin.Fail<RhinoDoc>(error: Op.Of(name: nameof(Headless)).InvalidInput()),
                };
                return opened.Bind(document => {
                    headless = document;
                    return Rasm.Domain.Context.Of(doc: document).ToFin().Bind(domain => body.Apply(runtime: new FileRuntime(
                        Document: Some(document),
                        Mode: RunMode.Scripted,
                        Domain: Some(domain),
                        Edit: Some(new DocumentEdit(document: document, domain: domain)),
                        Ui: Some(new RhinoUi(document: document, mode: RunMode.Scripted)))));
                });
            } finally {
                headless?.Dispose();
            }
        })
            .Run()
            .MapFail(_ => Op.Of(name: nameof(Headless)).InvalidResult())
            .Bind(static result => result);

    private static Fin<DocumentReceipt> ExportTarget(RhinoDoc document, DocumentTarget target, FileEndpoint endpoint, FileProfile profile, Op op) =>
        Try.lift<Fin<DocumentReceipt>>(f: () => {
            Seq<Guid> before = Seq<Guid>();
            Fin<DocumentReceipt> result = Fin.Fail<DocumentReceipt>(error: op.InvalidResult());
            // BOUNDARY ADAPTER — export selected mutates Rhino selection temporarily and must restore it.
            try {
                result =
                    from selected in DocumentEdit.SelectedIds(document: document, op: op)
                    let capture = ((Func<Seq<Guid>, Unit>)(ids => { before = ids; return unit; }))(selected)
                    from _ in DocumentEdit.UnitResult(success: document.Objects.UnselectAll(ignorePersistentSelections: true) >= 0, op: op)
                    from __ in target.Select(document: document, selected: true, policy: DocumentSelectionPolicy.Default, op: op)
                    from ___ in FileFormatProjection.Export(document: document, target: endpoint, profile: profile, selected: true, op: op)
                    select DocumentReceipt.Empty;
            } finally {
                _ = document.Objects.UnselectAll(ignorePersistentSelections: true);
                _ = before.IsEmpty switch {
                    true => 0,
                    false => document.Objects.SetSelectedObjects(objectIds: before.AsIterable(), syncHighlight: true, persistentSelect: true, ignoreGripsState: true, ignoreLayerLocking: false, ignoreLayerVisibility: false),
                };
            }
            return result;
        })
            .Run()
            .MapFail(_ => op.InvalidResult())
            .Bind(static result => result);

    private static Fin<(RhinoDoc Document, Rasm.Domain.Context Domain, DocumentEdit Edit)> Live(FileRuntime runtime, Op op) =>
        (runtime.Document.Case, runtime.Domain.Case, runtime.Edit.Case) switch {
            (RhinoDoc document, Rasm.Domain.Context domain, DocumentEdit edit) => Fin.Succ(value: (document, domain, edit)),
            _ => Fin.Fail<(RhinoDoc Document, Rasm.Domain.Context Domain, DocumentEdit Edit)>(error: op.MissingContext()),
        };

    private static Fin<Unit> TryUnit(Func<bool> run, Op op) =>
        Try.lift<bool>(f: run)
            .Run()
            .MapFail(_ => op.InvalidResult())
            .Bind(success => success switch {
                true => Fin.Succ(value: unit),
                false => Fin.Fail<Unit>(error: op.InvalidResult()),
            });
}
