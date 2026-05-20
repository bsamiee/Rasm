using Rasm.Rhino.Commands;
using Rasm.Rhino.UI;
using Rhino.Collections;

namespace Rasm.Rhino.Exchange;

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record FileOp<T> {
    private readonly Eff<FileRuntime, T> effect;

    private FileOp(Eff<FileRuntime, T> effect) =>
        this.effect = effect;

    internal static FileOp<T> Of(Func<FileRuntime, Fin<T>> run) =>
        new(effect: Eff<FileRuntime, T>.Lift(run));

    internal Fin<T> Apply(FileRuntime runtime) =>
        effect.Run(runtime);

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
internal abstract partial record FileExchange {
    private FileExchange() { }
    public sealed record Open(FileEndpoint Source, FileProfile Profile) : FileExchange;
    public sealed record Import(FileEndpoint Source, FileProfile Profile) : FileExchange;
    public sealed record Export(FileEndpoint Target, Option<DocumentTarget> Objects, FileProfile Profile) : FileExchange;
    public sealed record Save : FileExchange;
    public sealed record SaveAs(FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record WriteFile(FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record Write3dmFile(FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record SaveTemplate(FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record ArchiveRead(FileEndpoint Source, ArchiveProfile Profile) : FileExchange;
    public sealed record ArchiveExtract(FileEndpoint Source, FileEndpoint Target, ArchiveProfile Profile) : FileExchange;
    public sealed record ArchiveUpdate(FileEndpoint Source, FileEndpoint Target, global::Rasm.Rhino.Exchange.ArchiveUpdate Update, ArchiveProfile Profile) : FileExchange;
}

[Union]
internal abstract partial record HeadlessExchange {
    private HeadlessExchange() { }
    public sealed record Create : HeadlessExchange;
    public sealed record CreateFromTemplate(FileEndpoint Template) : HeadlessExchange;
    public sealed record Open(FileEndpoint Source, FileProfile Profile) : HeadlessExchange;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class FileOp {
    public static FileOp<FileReport> Open(FileEndpoint source, FileProfile profile) =>
        Do(exchange: new FileExchange.Open(Source: source, Profile: profile));

    public static FileOp<FileReport> Import(FileEndpoint source, FileProfile profile) =>
        Do(exchange: new FileExchange.Import(Source: source, Profile: profile));

    public static FileOp<FileReport> Export(FileEndpoint target, Option<DocumentTarget> objects, FileProfile profile) =>
        Do(exchange: new FileExchange.Export(Target: target, Objects: objects, Profile: profile));

    public static FileOp<FileReport> Save() =>
        Do(exchange: new FileExchange.Save());

    public static FileOp<FileReport> SaveAs(FileEndpoint target, FileProfile profile) =>
        Do(exchange: new FileExchange.SaveAs(Target: target, Profile: profile));

    public static FileOp<FileReport> WriteFile(FileEndpoint target, FileProfile profile) =>
        Do(exchange: new FileExchange.WriteFile(Target: target, Profile: profile));

    public static FileOp<FileReport> Write3dmFile(FileEndpoint target, FileProfile profile) =>
        Do(exchange: new FileExchange.Write3dmFile(Target: target, Profile: profile));

    public static FileOp<FileReport> SaveTemplate(FileEndpoint target, FileProfile profile) =>
        Do(exchange: new FileExchange.SaveTemplate(Target: target, Profile: profile));

    public static FileOp<FileReport> ArchiveRead(FileEndpoint source, ArchiveProfile profile) =>
        Do(exchange: new FileExchange.ArchiveRead(Source: source, Profile: profile));

    public static FileOp<FileReport> ArchiveExtract(FileEndpoint source, FileEndpoint target, ArchiveProfile profile) =>
        Do(exchange: new FileExchange.ArchiveExtract(Source: source, Target: target, Profile: profile));

    public static FileOp<FileReport> ArchiveUpdate(FileEndpoint source, FileEndpoint target, ArchiveUpdate update, ArchiveProfile profile) =>
        Do(exchange: new FileExchange.ArchiveUpdate(Source: source, Target: target, Update: update, Profile: profile));

    public static FileOp<byte[]> ArchiveBytes(FileEndpoint source, ArchiveProfile profile) =>
        FileOp<byte[]>.Of(run: _ => FileArchiveOps.Bytes(source: source, profile: profile));

    public static FileOp<FileReport> Batch(Seq<FileOp<FileReport>> items, FileBatchPolicy policy) =>
        FileOp<FileReport>.Of(run: runtime => BatchOps(runtime: runtime, items: items, policy: policy));

    internal static FileOp<FileReport> Do(FileExchange exchange) =>
        FileOp<FileReport>.Of(run: runtime =>
            from active in Optional(exchange).ToFin(Fail: Op.Of(name: nameof(Do)).InvalidInput())
            from result in active.Switch(
                open: open => OpenCore(source: open.Source, profile: open.Profile),
                import: import => ImportCore(runtime: runtime, source: import.Source, profile: import.Profile),
                export: export => ExportCore(runtime: runtime, target: export.Target, objects: export.Objects, profile: export.Profile),
                save: _ => WriteCore(runtime: runtime, target: Option<FileEndpoint>.None, profile: FileProfile.Model, intent: FileWriteIntent.Save),
                saveAs: saveAs => WriteCore(runtime: runtime, target: Some(saveAs.Target), profile: saveAs.Profile, intent: FileWriteIntent.SaveAs),
                writeFile: write => WriteCore(runtime: runtime, target: Some(write.Target), profile: write.Profile, intent: FileWriteIntent.WriteFile),
                write3dmFile: write => WriteCore(runtime: runtime, target: Some(write.Target), profile: write.Profile, intent: FileWriteIntent.Write3dm),
                saveTemplate: template => WriteCore(runtime: runtime, target: Some(template.Target), profile: template.Profile, intent: FileWriteIntent.Template),
                archiveRead: archive => FileArchiveOps.Read(source: archive.Source, profile: archive.Profile),
                archiveExtract: archive => FileArchiveOps.Extract(source: archive.Source, target: archive.Target, profile: archive.Profile),
                archiveUpdate: archive => FileArchiveOps.Update(source: archive.Source, target: archive.Target, update: archive.Update, profile: archive.Profile))
            select result);

    public static FileOp<T> Headless<T>(FileOp<T> body) =>
        Headless(scope: new HeadlessExchange.Create(), body: body);

    public static FileOp<T> Headless<T>(FileEndpoint source, FileProfile profile, FileOp<T> body) =>
        Headless(scope: new HeadlessExchange.Open(Source: source, Profile: profile), body: body);

    public static FileOp<T> HeadlessTemplate<T>(FileEndpoint template, FileOp<T> body) =>
        Headless(scope: new HeadlessExchange.CreateFromTemplate(Template: template), body: body);

    internal static FileOp<T> Headless<T>(HeadlessExchange scope, FileOp<T> body) =>
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

    private static Fin<FileReport> OpenCore(FileEndpoint source, FileProfile profile) =>
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

    private static Fin<FileReport> ImportCore(FileRuntime runtime, FileEndpoint source, FileProfile profile) =>
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

    private static Fin<FileReport> ExportCore(FileRuntime runtime, FileEndpoint target, Option<DocumentTarget> objects, FileProfile profile) =>
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

    private static Fin<FileReport> WriteCore(FileRuntime runtime, Option<FileEndpoint> target, FileProfile profile, FileWriteIntent intent) =>
        from live in Live(runtime: runtime, op: Op.Of(name: nameof(WriteCore)))
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

    private static Fin<FileReport> BatchOps(FileRuntime runtime, Seq<FileOp<FileReport>> items, FileBatchPolicy policy) =>
        items switch {
            Seq<FileOp<FileReport>> values when !values.IsEmpty => values.TraverseM(operation =>
                operation.Apply(runtime: runtime).BindFail(error => policy.ContinueOnError switch {
                    true => Fin.Succ(value: FileReport.Empty(phase: FilePhase.Batch) with { Issues = Seq(new FileIssue(Code: error.Category(), Message: error.Message)) }),
                    false => Fin.Fail<FileReport>(error: error),
                })).As().Map(reports => FileReport.Of(phase: FilePhase.Batch, issues: reports.Bind(static report => report.Issues), children: reports)),
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
                        from options in FileFormatProjection.Dictionary(endpoint: source, profile: open.Profile, phase: FilePhase.Headless, op: Op.Of(name: nameof(Headless)))
                            .BindFail(error => source.Format.Case switch {
                                FileFormat => Fin.Fail<ArchivableDictionary>(error: error),
                                _ => Fin.Succ(value: new ArchivableDictionary()),
                            })
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
                    from ids in target.Ids(document: document, op: op)
                    from _ in DocumentEdit.UnitResult(success: document.Objects.UnselectAll(ignorePersistentSelections: false) >= 0, op: op)
                    from __ in document.Objects.SetSelectedObjects(objectIds: ids.AsIterable(), syncHighlight: true, persistentSelect: false, ignoreGripsState: true, ignoreLayerLocking: false, ignoreLayerVisibility: false) switch {
                        int count when count == ids.Count => Fin.Succ(value: unit),
                        _ => Fin.Fail<Unit>(error: op.InvalidResult()),
                    }
                    from ___ in DocumentEdit.SelectedIds(document: document, op: op).Bind(active => active.OrderBy(static id => id).AsIterable().SequenceEqual(second: ids.OrderBy(static id => id).AsIterable()) ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: op.InvalidResult()))
                    from ____ in FileFormatProjection.Write(document: document, target: Some(endpoint), profile: profile, intent: FileWriteIntent.Export, selected: true, op: op)
                    select DocumentReceipt.Empty with { Selected = ids };
            } finally {
                _ = document.Objects.UnselectAll(ignorePersistentSelections: false);
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
