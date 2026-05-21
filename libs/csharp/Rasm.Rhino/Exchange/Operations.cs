using System.Globalization;
using Rasm.Rhino.Commands;
using Rasm.Rhino.UI;
using Rhino.Collections;
using Rhino.FileIO;
using IOFileInfo = System.IO.FileInfo;

namespace Rasm.Rhino.Exchange;

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record FileOp<T> {
    private readonly Eff<FileRuntime, T> effect;

    private FileOp(Eff<FileRuntime, T> effect) =>
        this.effect = effect;

    internal static FileOp<T> Lift(Func<FileRuntime, Fin<T>> run) =>
        new(effect: Eff<FileRuntime, T>.Lift(run));

    internal Fin<T> Apply(FileRuntime runtime) =>
        effect.Run(runtime);

    public FileOp<TNext> Map<TNext>(Func<T, TNext> project) =>
        FileOp<TNext>.Lift(run: runtime =>
            from valid in Optional(project).ToFin(Fail: Op.Of(name: nameof(Map)).InvalidInput())
            from value in Apply(runtime: runtime)
            select valid(arg: value));

    public FileOp<TNext> Bind<TNext>(Func<T, FileOp<TNext>> bind) =>
        FileOp<TNext>.Lift(run: runtime =>
            from valid in Optional(bind).ToFin(Fail: Op.Of(name: nameof(Bind)).InvalidInput())
            from value in Apply(runtime: runtime)
            from next in Optional(valid(arg: value)).ToFin(Fail: Op.Of(name: nameof(Bind)).InvalidInput())
            from result in next.Apply(runtime: runtime)
            select result);

    public FileOp<TNext> Select<TNext>(Func<T, TNext> project) =>
        Map(project: project);

    public FileOp<TResult> SelectMany<TNext, TResult>(Func<T, FileOp<TNext>> bind, Func<T, TNext, TResult> project) =>
        FileOp<TResult>.Lift(run: runtime =>
            from validBind in Optional(bind).ToFin(Fail: Op.Of(name: nameof(SelectMany)).InvalidInput())
            from validProject in Optional(project).ToFin(Fail: Op.Of(name: nameof(SelectMany)).InvalidInput())
            from value in Apply(runtime: runtime)
            from next in Optional(validBind(arg: value)).ToFin(Fail: Op.Of(name: nameof(SelectMany)).InvalidInput())
            from result in next.Apply(runtime: runtime)
            select validProject(arg1: value, arg2: result));
}

[Union(SwitchMapStateParameterName = "runtime")]
public abstract partial record FileExchange {
    private FileExchange() { }
    public sealed record Open(FileEndpoint Source, FileProfile Profile) : FileExchange;
    public sealed record Import(FileEndpoint Source, FileProfile Profile) : FileExchange;
    public sealed record Export(FileEndpoint Target, Option<DocumentTarget> Objects, FileProfile Profile) : FileExchange;
    public sealed record Save : FileExchange;
    public sealed record SaveAs(FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record WriteFile(FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record Write3dmFile(FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record SaveTemplate(FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record Publish(FileEndpoint Target, FilePublish Spec) : FileExchange;
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
    public static FileOp<FileReport> Do(FileExchange exchange) =>
        FileOp<FileReport>.Lift(run: runtime =>
            from active in Optional(exchange).ToFin(Fail: Op.Of(name: nameof(Do)).InvalidInput())
            from result in active.Switch(
                runtime,
                open: static (_, open) => OpenCore(source: open.Source, profile: open.Profile),
                import: static (runtime, import) => ImportCore(runtime: runtime, source: import.Source, profile: import.Profile),
                export: static (runtime, export) => ExportCore(runtime: runtime, target: export.Target, objects: export.Objects, profile: export.Profile),
                save: static (runtime, _) => WriteCore(runtime: runtime, target: Option<FileEndpoint>.None, profile: FileProfile.Model, phase: FilePhase.Save),
                saveAs: static (runtime, saveAs) => WriteCore(runtime: runtime, target: Some(saveAs.Target), profile: saveAs.Profile, phase: FilePhase.SaveAs),
                writeFile: static (runtime, write) => WriteCore(runtime: runtime, target: Some(write.Target), profile: write.Profile, phase: FilePhase.WriteFile),
                write3dmFile: static (runtime, write) => WriteCore(runtime: runtime, target: Some(write.Target), profile: write.Profile, phase: FilePhase.Write3dmFile),
                saveTemplate: static (runtime, template) => WriteCore(runtime: runtime, target: Some(template.Target), profile: template.Profile, phase: FilePhase.SaveTemplate),
                publish: static (runtime, publish) => PublishCore(runtime: runtime, target: publish.Target, publish: publish.Spec),
                archiveRead: static (_, archive) => FileArchiveOps.Read(source: archive.Source, profile: archive.Profile),
                archiveExtract: static (_, archive) => FileArchiveOps.Extract(source: archive.Source, target: archive.Target, profile: archive.Profile),
                archiveUpdate: static (_, archive) => FileArchiveOps.Update(source: archive.Source, target: archive.Target, update: archive.Update, profile: archive.Profile))
            select result);

    public static FileOp<byte[]> ArchiveBytes(FileEndpoint source, ArchiveProfile profile) =>
        FileOp<byte[]>.Lift(run: _ => FileArchiveOps.Bytes(source: source, profile: profile));

    public static FileOp<FileReport> Batch(Seq<FileOp<FileReport>> items, FileBatchPolicy policy) =>
        FileOp<FileReport>.Lift(run: runtime => BatchOps(runtime: runtime, items: items, policy: policy));

    public static FileOp<T> Headless<T>(FileOp<T> body) =>
        Headless(scope: new HeadlessExchange.Create(), body: body);

    public static FileOp<T> Headless<T>(FileEndpoint source, FileProfile profile, FileOp<T> body) =>
        Headless(scope: new HeadlessExchange.Open(Source: source, Profile: profile), body: body);

    public static FileOp<T> HeadlessTemplate<T>(FileEndpoint template, FileOp<T> body) =>
        Headless(scope: new HeadlessExchange.CreateFromTemplate(Template: template), body: body);

    internal static FileOp<T> Headless<T>(HeadlessExchange scope, FileOp<T> body) =>
        FileOp<T>.Lift(run: runtime =>
            from active in Optional(scope).ToFin(Fail: Op.Of(name: nameof(Headless)).InvalidInput())
            from operation in Optional(body).ToFin(Fail: Op.Of(name: nameof(Headless)).InvalidInput())
            from result in HeadlessScope(scope: active, body: operation)
            select result);

    public static FileOp<Seq<FileEndpoint>> Prompt(FilePrompt prompt) =>
        FileOp<Seq<FileEndpoint>>.Lift(run: runtime =>
            from active in Optional(prompt).ToFin(Fail: Op.Of(name: nameof(Prompt)).InvalidInput())
            from ui in runtime.Ui.ToFin(Fail: Op.Of(name: nameof(Prompt)).MissingContext())
            from result in ui.Use(intent: UiIntent.ExchangeFile(prompt: active))
            select result);

    private static Fin<FileReport> OpenCore(FileEndpoint source, FileProfile profile) =>
        from endpoint in source.Input(op: Op.Of(name: nameof(FileExchange.Open)))
        from format in FileFormatProjection.Require(endpoint: endpoint, profile: profile, phase: FilePhase.Open, op: Op.Of(name: nameof(FileExchange.Open)))
        from _ in format == FileFormat.ThreeDm ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: Op.Of(name: nameof(FileExchange.Open)).InvalidInput())
        from report in Try.lift<Fin<FileReport>>(f: () => {
            RhinoDoc? opened = RhinoDoc.Open(filePath: endpoint.Path, wasAlreadyOpen: out bool wasAlreadyOpen);
            return Optional(opened)
                .ToFin(Fail: Op.Of(name: nameof(FileExchange.Open)).InvalidResult())
                .Map(document => FileReport.Of(
                    phase: FilePhase.Open,
                    source: Some(endpoint),
                    target: Option<FileEndpoint>.None,
                    format: Some(format),
                    issues: wasAlreadyOpen ? Seq(new FileIssue(Code: "already-open", Message: "Document was already open.")) : Seq<FileIssue>()));
        }).Run().MapFail(_ => Op.Of(name: nameof(FileExchange.Open)).InvalidResult()).Bind(static result => result)
        select report;

    private static Fin<FileReport> ImportCore(FileRuntime runtime, FileEndpoint source, FileProfile profile) =>
        from live in Live(runtime: runtime, op: Op.Of(name: nameof(FileExchange.Import)))
        from endpoint in source.Input(op: Op.Of(name: nameof(FileExchange.Import)))
        from receipt in live.Edit.Commit(
            name: nameof(FileExchange.Import),
            redraw: DocumentRedraw.After,
            undoRecorded: true,
            run: (document, _, op) =>
                from before in DocumentEdit.LiveObjectIds(document: document)
                from imported in FileFormatProjection.Import(document: document, source: endpoint, profile: profile, op: op)
                from after in DocumentEdit.LiveObjectIds(document: document)
                select DocumentReceipt.Empty with { Created = after.Filter(id => !before.Exists(item => item == id)) })
        select FileReport.Of(phase: FilePhase.Import, source: Some(endpoint), target: Option<FileEndpoint>.None, format: FileFormatProjection.Resolve(endpoint: endpoint, profile: profile), receipt: Some(receipt));

    private static Fin<FileReport> ExportCore(FileRuntime runtime, FileEndpoint target, Option<DocumentTarget> objects, FileProfile profile) =>
        from live in Live(runtime: runtime, op: Op.Of(name: nameof(FileExchange.Export)))
        from endpoint in target.Output(op: Op.Of(name: nameof(FileExchange.Export)))
        from receipt in live.Edit.Commit(
            name: nameof(FileExchange.Export),
            redraw: DocumentRedraw.None,
            undoRecorded: false,
            run: (document, _, op) => objects.Case switch {
                DocumentTarget selection => ExportTarget(document: document, target: selection, endpoint: endpoint, profile: profile, op: op),
                _ => FileFormatProjection.Write(document: document, target: Some(endpoint), profile: profile, phase: FilePhase.Export, selected: false, op: op).Map(static _ => DocumentReceipt.Empty),
            })
        select FileReport.Of(phase: FilePhase.Export, source: Option<FileEndpoint>.None, target: Some(endpoint), format: FileFormatProjection.Resolve(endpoint: endpoint, profile: profile), receipt: Some(receipt));

    private static Fin<FileReport> WriteCore(FileRuntime runtime, Option<FileEndpoint> target, FileProfile profile, FilePhase phase) =>
        from live in Live(runtime: runtime, op: Op.Of(name: nameof(WriteCore)))
        from endpoint in WriteEndpoint(target: target, phase: phase)
        from receipt in live.Edit.Commit(
            name: phase.ToString(),
            redraw: DocumentRedraw.None,
            undoRecorded: false,
            run: (document, _, op) =>
                FileFormatProjection.Write(document: document, target: endpoint, profile: profile, phase: phase, selected: false, op: op)
                    .Map(static _ => DocumentReceipt.Empty))
        select FileReport.Of(
            phase: phase,
            target: endpoint,
            format: endpoint.Case switch {
                FileEndpoint value => FileFormatProjection.Resolve(endpoint: value, profile: profile),
                _ => profile.Format,
            },
            receipt: Some(receipt));

    private static Fin<Option<FileEndpoint>> WriteEndpoint(Option<FileEndpoint> target, FilePhase phase) =>
        phase switch {
            FilePhase.Save => Fin.Succ(Option<FileEndpoint>.None),
            FilePhase.SaveAs or FilePhase.Write3dmFile or FilePhase.SaveTemplate =>
                target.ToFin(Fail: Op.Of(name: nameof(WriteEndpoint)).InvalidInput())
                    .Bind(endpoint => endpoint.WithFormat(format: FileFormat.ThreeDm).Output(op: Op.Of(name: nameof(WriteEndpoint))))
                    .Map(Some),
            _ => target.ToFin(Fail: Op.Of(name: nameof(WriteEndpoint)).InvalidInput())
                .Bind(endpoint => endpoint.Output(op: Op.Of(name: nameof(WriteEndpoint))))
                .Map(Some),
        };

    private static Fin<FileReport> PublishCore(FileRuntime runtime, FileEndpoint target, FilePublish publish) =>
        from live in Live(runtime: runtime, op: Op.Of(name: nameof(FileExchange.Publish)))
        from active in Optional(publish).ToFin(Fail: Op.Of(name: nameof(FileExchange.Publish)).InvalidInput())
        from endpoint in target.WithFormat(format: FileFormat.Pdf).Output(op: Op.Of(name: nameof(FileExchange.Publish)))
        from format in FileFormatProjection.Require(endpoint: endpoint, profile: active.Profile, phase: FilePhase.Publish, op: Op.Of(name: nameof(FileExchange.Publish)))
        from receipt in live.Edit.Commit(
            name: nameof(FileExchange.Publish),
            redraw: DocumentRedraw.None,
            undoRecorded: false,
            run: (document, _, op) => PublishPdf(document: document, endpoint: endpoint, publish: active, op: op))
        select FileReport.Of(phase: FilePhase.Publish, target: Some(endpoint), format: Some(format), receipt: Some(receipt));

    private static Fin<DocumentReceipt> PublishPdf(RhinoDoc document, FileEndpoint endpoint, FilePublish publish, Op op) =>
        from views in Optional(toSeq(document.Views.GetPageViews()))
            .Filter(static pages => !pages.IsEmpty)
            .ToFin(Fail: op.InvalidInput())
        let sheets = publish.Sheets.IsEmpty ? views.Map(static page => new FileSheet(Id: Some(page.MainViewport.Id))) : publish.Sheets
        from pages in sheets.TraverseM(sheet =>
            (sheet.Id.Case, sheet.Name.Case) switch {
                (Guid, _) or (_, string) => views.Find(page =>
                        sheet.Id.Map(id => page.MainViewport.Id == id).IfNone(false)
                        || sheet.Name.Map(name => string.Equals(a: page.PageName, b: name, comparisonType: StringComparison.OrdinalIgnoreCase)).IfNone(false))
                    .Map(page => (Page: page, Sheet: sheet))
                    .ToFin(Fail: op.InvalidInput()),
                _ => views.Head.Map(page => (Page: page, Sheet: sheet)).ToFin(Fail: op.InvalidInput()),
            }).As()
        from pdf in Optional(FilePdf.Create()).ToFin(Fail: op.InvalidResult())
        from names in pages.TraverseM(page => AddPdfSheet(pdf: pdf, page: page.Page, sheet: page.Sheet, layers: publish.Layers, op: op)).As()
        from _ in Try.lift<Unit>(f: () => { pdf.Write(filename: endpoint.Path); return unit; }).Run().MapFail(_ => op.InvalidResult())
        from __ in Optional(new IOFileInfo(fileName: endpoint.Path))
            .Filter(static info => info.Exists && info.Length > 0)
            .ToFin(Fail: op.InvalidResult())
        select DocumentReceipt.Empty with { ResourceChanged = names.Map(static page => new DocumentResourceChange(Kind: DocumentResourceKind.View, Name: page)) };

    private static Fin<string> AddPdfSheet(FilePdf pdf, RhinoPageView page, FileSheet sheet, bool layers, Op op) =>
        from _ in guard(double.IsFinite(d: sheet.Dpi) && sheet.Dpi > 0, op.InvalidInput())
        from added in Try.lift<Fin<string>>(f: () => {
            // BOUNDARY ADAPTER — FilePdf mutates native page state before writing the final document.
            using ViewCaptureSettings settings = new(sourcePageView: page, dpi: sheet.Dpi) { UsePrintWidths = sheet.PrintWidths, RasterMode = sheet.Raster, OutputColor = sheet.Color };
            pdf.LayersAsOptionalContentGroups = layers && !settings.RasterMode;
            return pdf.AddPage(settings: settings) switch {
                int pageNumber when pageNumber >= 0 => Fin.Succ(value: string.IsNullOrWhiteSpace(value: page.PageName) ? pageNumber.ToString(provider: CultureInfo.InvariantCulture) : page.PageName),
                _ => Fin.Fail<string>(error: op.InvalidResult()),
            };
        }).Run().MapFail(_ => op.InvalidResult()).Bind(static result => result)
        select added;

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
                    from ____ in FileFormatProjection.Write(document: document, target: Some(endpoint), profile: profile, phase: FilePhase.Export, selected: true, op: op)
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
