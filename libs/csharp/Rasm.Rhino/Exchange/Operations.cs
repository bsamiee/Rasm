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
            from valid in Optional(project).ToFin(Fail: Op.Of(name: nameof(Map)).InvalidInput())
            from value in Apply(runtime: runtime)
            select valid(arg: value));

    public FileOp<TNext> Bind<TNext>(Func<T, FileOp<TNext>> bind) =>
        FileOp<TNext>.Of(run: runtime =>
            from valid in Optional(bind).ToFin(Fail: Op.Of(name: nameof(Bind)).InvalidInput())
            from value in Apply(runtime: runtime)
            from next in Optional(valid(arg: value)).ToFin(Fail: Op.Of(name: nameof(Bind)).InvalidInput())
            from result in next.Apply(runtime: runtime)
            select result);
}

internal enum FileWriteIntent { Save, SaveAs, WriteFile, Write3dm, Template, Export }

[Union]
public abstract partial record FileExchange {
    private FileExchange() { }
    public sealed record Open(FileEndpoint Source, FileProfile Profile) : FileExchange;
    public sealed record Import(FileEndpoint Source, FileProfile Profile) : FileExchange;
    public sealed record Export(FileEndpoint Target, Option<DocumentTarget> Objects, FileProfile Profile) : FileExchange;
    public sealed record Save(FileProfile Profile) : FileExchange;
    public sealed record SaveAs(FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record WriteFile(FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record Write3dmFile(FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record SaveTemplate(FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record ArchiveRead(FileEndpoint Source, ArchiveProfile Profile) : FileExchange;
    public sealed record ArchiveExtract(FileEndpoint Source, FileEndpoint Target, ArchiveProfile Profile) : FileExchange;
    public sealed record ArchiveUpdate(FileEndpoint Source, FileEndpoint Target, global::Rasm.Rhino.Exchange.ArchiveUpdate Update, ArchiveProfile Profile) : FileExchange;
    public sealed record Batch(Seq<FileExchange> Items, FileBatchPolicy Policy) : FileExchange;
}

[Union]
public abstract partial record HeadlessExchange {
    private HeadlessExchange() { }
    public sealed record Create : HeadlessExchange;
    public sealed record CreateFromTemplate(FileEndpoint Template) : HeadlessExchange;
    public sealed record Open(FileEndpoint Source, FileProfile Profile) : HeadlessExchange;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class FileOp {
    public static FileOp<FileReport> Do(FileExchange exchange) =>
        FileOp<FileReport>.Of(run: runtime =>
            from active in Optional(exchange).ToFin(Fail: Op.Of(name: nameof(Do)).InvalidInput())
            from result in active.Switch(
                open: open => Open(source: open.Source, profile: open.Profile),
                import: import => Import(runtime: runtime, source: import.Source, profile: import.Profile),
                export: export => Export(runtime: runtime, target: export.Target, objects: export.Objects, profile: export.Profile),
                save: save => Write(runtime: runtime, target: Option<FileEndpoint>.None, profile: save.Profile, intent: FileWriteIntent.Save),
                saveAs: saveAs => Write(runtime: runtime, target: Some(saveAs.Target), profile: saveAs.Profile, intent: FileWriteIntent.SaveAs),
                writeFile: write => Write(runtime: runtime, target: Some(write.Target), profile: write.Profile, intent: FileWriteIntent.WriteFile),
                write3dmFile: write => Write(runtime: runtime, target: Some(write.Target), profile: write.Profile, intent: FileWriteIntent.Write3dm),
                saveTemplate: template => Write(runtime: runtime, target: Some(template.Target), profile: template.Profile, intent: FileWriteIntent.Template),
                archiveRead: archive => FileArchiveOps.Read(source: archive.Source, profile: archive.Profile),
                archiveExtract: archive => FileArchiveOps.Extract(source: archive.Source, target: archive.Target, profile: archive.Profile),
                archiveUpdate: archive => FileArchiveOps.Update(source: archive.Source, target: archive.Target, update: archive.Update, profile: archive.Profile),
                batch: batch => Batch(runtime: runtime, items: batch.Items, policy: batch.Policy))
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
        from format in FileFormatProjection.Require(endpoint: endpoint, profile: profile, phase: FilePhase.Open, op: Op.Of(name: nameof(Open)))
        from _ in format == FileFormat.ThreeDm ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: Op.Of(name: nameof(Open)).InvalidInput())
        from report in Try.lift<Fin<FileReport>>(f: () => {
            RhinoDoc? opened = RhinoDoc.Open(filePath: endpoint.Path, wasAlreadyOpen: out bool wasAlreadyOpen);
            return Optional(opened)
                .ToFin(Fail: Op.Of(name: nameof(Open)).InvalidResult())
                .Map(document => FileReport.Of(
                    phase: FilePhase.Open,
                    source: Some(endpoint),
                    target: Option<FileEndpoint>.None,
                    format: Some(format),
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

    private static Fin<FileReport> Export(FileRuntime runtime, FileEndpoint target, Option<DocumentTarget> objects, FileProfile profile) =>
        from live in Live(runtime: runtime, op: Op.Of(name: nameof(Export)))
        from endpoint in target.Output(op: Op.Of(name: nameof(Export)))
        from receipt in live.Edit.Commit(
            name: nameof(Export),
            redraw: DocumentRedraw.None,
            undoRecorded: false,
            run: (document, _, op) => objects.Case switch {
                DocumentTarget selection => ExportTarget(document: document, target: selection, endpoint: endpoint, profile: profile, op: op),
                _ => FileFormatProjection.Write(document: document, target: Some(endpoint), profile: profile, intent: FileWriteIntent.Export, selected: false, op: op).Map(static _ => DocumentReceipt.Empty),
            })
        select FileReport.Of(phase: FilePhase.Export, source: Option<FileEndpoint>.None, target: Some(endpoint), format: FileFormatProjection.Resolve(endpoint: endpoint, profile: profile), receipt: Some(receipt));

    private static Fin<FileReport> Write(FileRuntime runtime, Option<FileEndpoint> target, FileProfile profile, FileWriteIntent intent) =>
        from live in Live(runtime: runtime, op: Op.Of(name: nameof(Write)))
        from endpoint in WriteEndpoint(target: target, intent: intent)
        from receipt in live.Edit.Commit(
            name: intent.ToString(),
            redraw: DocumentRedraw.None,
            undoRecorded: false,
            run: (document, _, op) =>
                FileFormatProjection.Write(document: document, target: endpoint, profile: profile, intent: intent, selected: false, op: op)
                    .Map(static _ => DocumentReceipt.Empty))
        select FileReport.Of(
            phase: PhaseOf(intent: intent),
            target: endpoint,
            format: endpoint.Case switch {
                FileEndpoint value => FileFormatProjection.Resolve(endpoint: value, profile: profile),
                _ => profile.Format,
            },
            receipt: Some(receipt));

    private static Fin<Option<FileEndpoint>> WriteEndpoint(Option<FileEndpoint> target, FileWriteIntent intent) =>
        intent switch {
            FileWriteIntent.Save => Fin.Succ(Option<FileEndpoint>.None),
            FileWriteIntent.SaveAs or FileWriteIntent.Write3dm or FileWriteIntent.Template =>
                target.ToFin(Fail: Op.Of(name: nameof(WriteEndpoint)).InvalidInput())
                    .Bind(endpoint => endpoint.WithFormat(format: FileFormat.ThreeDm).Output(op: Op.Of(name: nameof(WriteEndpoint))))
                    .Map(Some),
            _ => target.ToFin(Fail: Op.Of(name: nameof(WriteEndpoint)).InvalidInput())
                .Bind(endpoint => endpoint.Output(op: Op.Of(name: nameof(WriteEndpoint))))
                .Map(Some),
        };

    private static FilePhase PhaseOf(FileWriteIntent intent) =>
        intent switch {
            FileWriteIntent.Save => FilePhase.Save,
            FileWriteIntent.SaveAs => FilePhase.SaveAs,
            FileWriteIntent.WriteFile => FilePhase.WriteFile,
            FileWriteIntent.Write3dm => FilePhase.Write3dmFile,
            FileWriteIntent.Template => FilePhase.SaveTemplate,
            FileWriteIntent.Export => FilePhase.Export,
            _ => FilePhase.Batch,
        };

    private static Fin<FileReport> Batch(FileRuntime runtime, Seq<FileExchange> items, FileBatchPolicy policy) =>
        items switch {
            Seq<FileExchange> values when !values.IsEmpty => values.TraverseM(exchange =>
                Do(exchange: exchange).Apply(runtime: runtime).BindFail(error => policy.ContinueOnError switch {
                    true => Fin.Succ(value: FileReport.Empty(phase: FilePhase.Batch) with { Issues = Seq(new FileIssue(Code: error.Category(), Message: error.Message)) }),
                    false => Fin.Fail<FileReport>(error: error),
                })).As().Map(reports => FileReport.Of(phase: FilePhase.Batch, issues: reports.Bind(static report => report.Issues))),
            _ => Fin.Fail<FileReport>(error: Op.Of(name: nameof(Batch)).InvalidInput()),
        };

    private static Fin<T> HeadlessScope<T>(HeadlessExchange scope, FileOp<T> body) =>
        Try.lift<Fin<T>>(f: () => {
            RhinoDoc? headless = null;
            // BOUNDARY ADAPTER — headless RhinoDoc is native disposable document state.
            try {
                Fin<RhinoDoc> opened = scope.Switch(
                    create: static _ =>
                        Optional(RhinoDoc.CreateHeadless(file3dmTemplatePath: null)).ToFin(Fail: Op.Of(name: nameof(Headless)).InvalidResult()),
                    createFromTemplate: static create =>
                        from template in create.Template.Input(op: Op.Of(name: nameof(Headless)))
                        from document in Optional(RhinoDoc.CreateHeadless(file3dmTemplatePath: template.Path)).ToFin(Fail: Op.Of(name: nameof(Headless)).InvalidResult())
                        select document,
                    open: static open =>
                        from source in open.Source.Input(op: Op.Of(name: nameof(Headless)))
                        let options = FileFormatProjection.Dictionary(endpoint: source, profile: open.Profile, phase: FilePhase.Headless, op: Op.Of(name: nameof(Headless))).IfFail(_ => new ArchivableDictionary())
                        from document in Optional(RhinoDoc.OpenHeadless(filePath: source.Path, options: options)).ToFin(Fail: Op.Of(name: nameof(Headless)).InvalidResult())
                        select document);
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
                    from ___ in FileFormatProjection.Write(document: document, target: Some(endpoint), profile: profile, intent: FileWriteIntent.Export, selected: true, op: op)
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

}
