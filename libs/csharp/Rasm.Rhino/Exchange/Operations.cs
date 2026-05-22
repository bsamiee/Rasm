using Rasm.Rhino.Commands;
using Rasm.Rhino.UI;
using Rhino.Collections;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;
using IOFileInfo = System.IO.FileInfo;

namespace Rasm.Rhino.Exchange;

// --- [MODELS] -----------------------------------------------------------------------------
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
    public sealed record Publish(FilePublish Spec) : FileExchange;
    public sealed record NamedLayerState(FileLayerState Change) : FileExchange;
    public sealed record ArchiveRead(FileArchiveSource Source, ArchiveProfile Profile) : FileExchange;
    public sealed record ArchiveExtract(FileArchiveSource Source, FileEndpoint Target, ArchiveProfile Profile) : FileExchange;
    public sealed record ArchiveUpdate(FileArchiveSource Source, FileEndpoint Target, Exchange.ArchiveUpdate Update, ArchiveProfile Profile) : FileExchange;
}

public abstract record FilePublishTarget {
    private FilePublishTarget() { }
    internal abstract Fin<FilePublishResult> Write(Seq<FileSheetPage> pages, bool layers, Op op);

    public sealed record Pdf(FileEndpoint Target) : FilePublishTarget {
        internal override Fin<FilePublishResult> Write(Seq<FileSheetPage> pages, bool layers, Op op) =>
            from endpoint in Target.WithFormat(format: FileFormat.Pdf).Output(op: op)
            from pdf in Optional(FilePdf.Create()).ToFin(Fail: op.InvalidResult())
            from added in pages.TraverseM(page => Add(pdf: pdf, page: page, layers: layers, op: op)).As()
            from _ in op.Catch(() => { pdf.Write(filename: endpoint.Path); return Fin.Succ(value: unit); })
            from __ in Optional(new IOFileInfo(fileName: endpoint.Path))
                .Filter(static info => info.Exists && info.Length > 0)
                .ToFin(Fail: op.InvalidResult())
            select new FilePublishResult(
                Target: Some(endpoint),
                Format: Some(FileFormat.Pdf),
                Receipt: DocumentReceipt.Empty);

        private static Fin<Unit> Add(FilePdf pdf, FileSheetPage page, bool layers, Op op) =>
            from settings in page.Sheet.Settings(page: page.Page, op: op)
            from added in op.Catch(() => {
                using ViewCaptureSettings owned = settings;
                pdf.LayersAsOptionalContentGroups = layers && !owned.RasterMode;
                return pdf.AddPage(settings: owned) switch {
                    >= 0 => Fin.Succ(value: unit),
                    _ => Fin.Fail<Unit>(error: op.InvalidResult()),
                };
            })
            select added;
    }

    public sealed record Printer(string Name, int Copies = 1) : FilePublishTarget {
        internal override Fin<FilePublishResult> Write(Seq<FileSheetPage> pages, bool layers, Op op) =>
            from name in FileEndpoint.NonBlank(value: Name, op: op)
            from copies in Copies switch {
                > 0 => Fin.Succ(value: Copies),
                _ => Fin.Fail<int>(error: op.InvalidInput()),
            }
            from settings in pages.TraverseM(page => page.Sheet.Settings(page: page.Page, op: op)).As()
            from result in op.Catch(() => {
                ViewCaptureSettings[] captures = [.. settings];
                try {
                    return op.Confirm(success: ViewCapture.SendToPrinter(printerName: name, settings: captures, copies: copies))
                        .Map(_ => new FilePublishResult(
                            Target: Option<FileEndpoint>.None,
                            Format: Option<FileFormat>.None,
                            Receipt: DocumentReceipt.Empty,
                            NativeLog: Some($"printer:{name};copies:{copies};sheets:{pages.Count}")));
                } finally {
                    _ = toSeq(captures).Iter(static settings => settings.Dispose());
                }
            })
            select result;
    }

}

[Union(SwitchMapStateParameterName = "state")]
public abstract partial record FileLayerState {
    private FileLayerState() { }
    public sealed record Save(string Name, Option<Guid> Viewport = default) : FileLayerState;
    public sealed record Restore(string Name, RestoreLayerProperties Properties, Option<Guid> Viewport = default) : FileLayerState;
    public sealed record Rename(string Name, string Next) : FileLayerState;
    public sealed record Delete(string Name) : FileLayerState;
    public sealed record Import(FileEndpoint Source) : FileLayerState;

    internal Fin<DocumentResourceChange> Apply(RhinoDoc document, Op op) =>
        Switch(
            (Document: document, Op: op),
            save: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from _ in state.Document.NamedLayerStates.Save(name: name, viewportId: change.Viewport.IfNone(Guid.Empty)) switch {
                    >= 0 => Fin.Succ(value: unit),
                    _ => Fin.Fail<Unit>(error: state.Op.InvalidResult()),
                }
                select new DocumentResourceChange(Kind: DocumentResourceKind.NamedLayerState, Name: name),
            restore: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from _ in state.Op.Confirm(success: state.Document.NamedLayerStates.Restore(name: name, properties: change.Properties, viewportId: change.Viewport.IfNone(Guid.Empty)))
                select new DocumentResourceChange(Kind: DocumentResourceKind.NamedLayerState, Name: name),
            rename: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from next in FileEndpoint.NonBlank(value: change.Next, op: state.Op)
                from _ in state.Op.Confirm(success: state.Document.NamedLayerStates.Rename(oldName: name, newName: next))
                select new DocumentResourceChange(Kind: DocumentResourceKind.NamedLayerState, Name: next),
            delete: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from _ in state.Op.Confirm(success: state.Document.NamedLayerStates.Delete(name: name))
                select new DocumentResourceChange(Kind: DocumentResourceKind.NamedLayerState, Name: name),
            import: static (state, change) =>
                from source in change.Source.Input(op: state.Op)
                from _ in state.Document.NamedLayerStates.Import(filename: source.Path) switch {
                    >= 0 => Fin.Succ(value: unit),
                    _ => Fin.Fail<Unit>(error: state.Op.InvalidResult()),
                }
                select new DocumentResourceChange(Kind: DocumentResourceKind.NamedLayerState, Name: source.Path));
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
    internal static Eff<FileRuntime, T> Lift<T>(Func<FileRuntime, Fin<T>> run) =>
        Eff<FileRuntime, T>.Lift(runtime =>
            Optional(run)
                .ToFin(Fail: Op.Of(name: nameof(Lift)).InvalidInput())
                .Bind(valid => valid(arg: runtime)));

    public static Eff<FileRuntime, FileReport> Do(FileExchange exchange) =>
        Lift(run: runtime =>
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
                publish: static (runtime, publish) => PublishCore(runtime: runtime, publish: publish.Spec),
                namedLayerState: static (runtime, state) => NamedLayerStateCore(runtime: runtime, change: state.Change),
                archiveRead: static (_, archive) => FileArchiveOps.Read(source: archive.Source, profile: archive.Profile),
                archiveExtract: static (_, archive) => FileArchiveOps.Extract(source: archive.Source, target: archive.Target, profile: archive.Profile),
                archiveUpdate: static (_, archive) => FileArchiveOps.Update(source: archive.Source, target: archive.Target, update: archive.Update, profile: archive.Profile))
            select result);

    public static Eff<FileRuntime, byte[]> ArchiveBytes(FileArchiveSource source, ArchiveProfile profile) =>
        Lift(run: _ => FileArchiveOps.Bytes(source: source, profile: profile));

    public static Eff<FileRuntime, FileReport> Batch(Seq<Eff<FileRuntime, FileReport>> items, FileBatchPolicy policy) =>
        Lift(run: runtime => BatchOps(runtime: runtime, items: items, policy: policy));

    public static Eff<FileRuntime, T> Headless<T>(Eff<FileRuntime, T> body) =>
        Headless(scope: new HeadlessExchange.Create(), body: body);

    public static Eff<FileRuntime, T> Headless<T>(FileEndpoint source, FileProfile profile, Eff<FileRuntime, T> body) =>
        Headless(scope: new HeadlessExchange.Open(Source: source, Profile: profile), body: body);

    public static Eff<FileRuntime, T> HeadlessTemplate<T>(FileEndpoint template, Eff<FileRuntime, T> body) =>
        Headless(scope: new HeadlessExchange.CreateFromTemplate(Template: template), body: body);

    internal static Eff<FileRuntime, T> Headless<T>(HeadlessExchange scope, Eff<FileRuntime, T> body) =>
        Lift(run: runtime =>
            from active in Optional(scope).ToFin(Fail: Op.Of(name: nameof(Headless)).InvalidInput())
            from result in HeadlessScope(scope: active, body: body)
            select result);

    public static Eff<FileRuntime, Seq<FileEndpoint>> Prompt(FilePrompt prompt) =>
        Lift(run: runtime =>
            from active in Optional(prompt).ToFin(Fail: Op.Of(name: nameof(Prompt)).InvalidInput())
            from ui in runtime.Ui.ToFin(Fail: Op.Of(name: nameof(Prompt)).MissingContext())
            from result in ui.Use(intent: UiIntent.ExchangeFile(prompt: active))
            select result);

    private static Fin<FileReport> OpenCore(FileEndpoint source, FileProfile profile) =>
        from endpoint in source.Input(op: Op.Of(name: nameof(FileExchange.Open)))
        from format in FileFormatProjection.Require(endpoint: endpoint, profile: profile, phase: FilePhase.Open, op: Op.Of(name: nameof(FileExchange.Open)))
        from _ in format == FileFormat.ThreeDm ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: Op.Of(name: nameof(FileExchange.Open)).InvalidInput())
        from report in Op.Of(name: nameof(FileExchange.Open)).Catch(() => {
            RhinoDoc? opened = RhinoDoc.Open(filePath: endpoint.Path, wasAlreadyOpen: out bool wasAlreadyOpen);
            return Optional(opened)
                .ToFin(Fail: Op.Of(name: nameof(FileExchange.Open)).InvalidResult())
                .Map(document => FileReport.Of(
                    phase: FilePhase.Open,
                    source: Some(endpoint),
                    target: Option<FileEndpoint>.None,
                    format: Some(format),
                    issues: wasAlreadyOpen ? Seq(new FileIssue(Code: "already-open", Message: "Document was already open.")) : Seq<FileIssue>()));
        })
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

    private static Fin<FileReport> PublishCore(FileRuntime runtime, FilePublish publish) =>
        from live in Live(runtime: runtime, op: Op.Of(name: nameof(FileExchange.Publish)))
        from active in Optional(publish).ToFin(Fail: Op.Of(name: nameof(FileExchange.Publish)).InvalidInput())
        from target in Optional(active.Target).ToFin(Fail: Op.Of(name: nameof(FileExchange.Publish)).InvalidInput())
        from sheets in SheetPages(document: live.Document, publish: active, op: Op.Of(name: nameof(FileExchange.Publish)))
        from result in RhinoUi.Protect(valid: () => target.Write(pages: sheets, layers: active.Layers, op: Op.Of(name: nameof(FileExchange.Publish))))
        select FileReport.Of(phase: FilePhase.Publish, target: result.Target, format: result.Format, issues: result.Issues, nativeLog: result.NativeLog, receipt: Some(result.Receipt));

    private static Fin<FileReport> NamedLayerStateCore(FileRuntime runtime, FileLayerState change) =>
        from live in Live(runtime: runtime, op: Op.Of(name: nameof(FileExchange.NamedLayerState)))
        from active in Optional(change).ToFin(Fail: Op.Of(name: nameof(FileExchange.NamedLayerState)).InvalidInput())
        from receipt in live.Edit.Commit(
            name: nameof(FileExchange.NamedLayerState),
            redraw: DocumentRedraw.After,
            undoRecorded: true,
            run: (document, _, op) =>
                active.Apply(document: document, op: op)
                    .Map(static changed => DocumentReceipt.Empty with { ResourceChanged = Seq(changed) }))
        select FileReport.Of(phase: FilePhase.NamedLayerState, receipt: Some(receipt));

    private static Fin<Seq<FileSheetPage>> SheetPages(RhinoDoc document, FilePublish publish, Op op) =>
        from views in Optional(toSeq(document.Views.GetPageViews()))
            .Filter(static pages => !pages.IsEmpty)
            .ToFin(Fail: op.InvalidInput())
        let sheets = publish.Sheets.IsEmpty ? views.Map(static page => new FileSheet(Id: Some(page.MainViewport.Id))) : publish.Sheets
        from pages in sheets.TraverseM(sheet =>
                SheetMatches(document: document, views: views, sheet: sheet) switch {
                    Seq<FileSheetPage> matches when !matches.IsEmpty => Fin.Succ(value: matches),
                    _ => Fin.Fail<Seq<FileSheetPage>>(error: op.InvalidInput()),
                })
            .As()
            .Map(static groups => groups.Bind(static items => items))
        from _ in guard(!pages.IsEmpty, op.InvalidInput())
        select pages;

    private static Seq<FileSheetPage> SheetMatches(RhinoDoc document, Seq<RhinoPageView> views, FileSheet sheet) {
        Option<int> group = sheet.Group.Bind(name => Optional(document.PageViewGroups.FindName(name: name)).Map(static active => active.Index));
        return (sheet.Id.Case, sheet.Name.Case, sheet.Group.Case, group.Case) switch {
            (_, _, string, not int) => Seq<FileSheetPage>(),
            (not Guid, not string, not string, _) => views.Head.Map(page => new FileSheetPage(Page: page, Sheet: sheet)).ToSeq(),
            _ => views
                .Filter(page =>
                    sheet.Id.Map(id => page.MainViewport.Id == id).IfNone(true)
                    && sheet.Name.Map(name => string.Equals(a: page.PageName, b: name, comparisonType: StringComparison.OrdinalIgnoreCase)).IfNone(true)
                    && group.Map(index => page.IsInPageViewGroup(pageViewGroupIndex: index)).IfNone(true))
                .Map(page => new FileSheetPage(Page: page, Sheet: sheet)),
        };
    }

    private static Fin<FileReport> BatchOps(FileRuntime runtime, Seq<Eff<FileRuntime, FileReport>> items, FileBatchPolicy policy) =>
        items switch {
            Seq<Eff<FileRuntime, FileReport>> values when !values.IsEmpty => values.TraverseM(operation =>
                operation.Run(runtime).BindFail(error => policy.ContinueOnError switch {
                    true => Fin.Succ(value: FileReport.Empty(phase: FilePhase.Batch) with { Issues = Seq(new FileIssue(Code: error.Category(), Message: error.Message)) }),
                    false => Fin.Fail<FileReport>(error: error),
                })).As().Map(reports => FileReport.Of(phase: FilePhase.Batch, issues: reports.Bind(static report => report.Issues), children: reports)),
            _ => Fin.Fail<FileReport>(error: Op.Of(name: nameof(Batch)).InvalidInput()),
        };

    private static Fin<T> HeadlessScope<T>(HeadlessExchange scope, Eff<FileRuntime, T> body) =>
        Op.Of(name: nameof(Headless)).Catch(() => {
            RhinoDoc? headless = null;
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
                    return Context.Of(doc: document).ToFin().Bind(domain => body.Run(new FileRuntime(
                        document: Some(document),
                        mode: RunMode.Scripted,
                        domain: Some(domain),
                        edit: Some(new DocumentEdit(document: document, domain: domain)),
                        ui: Some(new RhinoUi(document: document, mode: RunMode.Scripted)))));
                });
            } finally {
                headless?.Dispose();
            }
        });

    private static Fin<DocumentReceipt> ExportTarget(RhinoDoc document, DocumentTarget target, FileEndpoint endpoint, FileProfile profile, Op op) =>
        op.Catch(() => {
            Seq<Guid> before = Seq<Guid>();
            Fin<DocumentReceipt> result = Fin.Fail<DocumentReceipt>(error: op.InvalidResult());
            // BOUNDARY ADAPTER — export selected mutates Rhino selection temporarily and must restore it.
            try {
                result =
                    from selected in DocumentEdit.SelectedIds(document: document, op: op)
                    let capture = ((Func<Seq<Guid>, Unit>)(ids => { before = ids; return unit; }))(selected)
                    from ids in target.Ids(document: document, op: op)
                    from _ in op.Confirm(success: document.Objects.UnselectAll(ignorePersistentSelections: false) >= 0)
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
        });

    private static Fin<(RhinoDoc Document, Context Domain, DocumentEdit Edit)> Live(FileRuntime runtime, Op op) =>
        (runtime.Document.Case, runtime.Domain.Case, runtime.Edit.Case) switch {
            (RhinoDoc document, Context domain, DocumentEdit edit) => Fin.Succ(value: (document, domain, edit)),
            _ => Fin.Fail<(RhinoDoc Document, Context Domain, DocumentEdit Edit)>(error: op.MissingContext()),
        };

}
