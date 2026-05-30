using System.Globalization;
using Rasm.Rhino.Commands;
using Rasm.Rhino.UI;
using Rhino.Collections;
using Rhino.DocObjects.Tables;

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
    public sealed record NamedPosition(FileNamedPosition Change) : FileExchange;
    public sealed record ArchiveRead(FileArchiveSource Source, ArchiveProfile Profile) : FileExchange;
    public sealed record ArchiveExtract(FileArchiveSource Source, FileEndpoint Target, ArchiveProfile Profile) : FileExchange;
    public sealed record ArchiveUpdate(FileArchiveSource Source, FileEndpoint Target, Exchange.ArchiveUpdate Update, ArchiveProfile Profile) : FileExchange;
    public sealed record ArchiveInspect(FileEndpoint Source) : FileExchange;
    public sealed record ArchiveValidate(FileArchiveSource Source, ArchiveProfile Profile) : FileExchange;
    public sealed record SheetEdit(FileSheetEdit Edit) : FileExchange;
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

[Union(SwitchMapStateParameterName = "state")]
public abstract partial record FileNamedPosition {
    private FileNamedPosition() { }
    public sealed record Save(string Name, DocumentTarget Objects) : FileNamedPosition;
    public sealed record Restore(string Name) : FileNamedPosition;
    public sealed record Update(string Name) : FileNamedPosition;
    public sealed record Rename(string Name, string Next) : FileNamedPosition;
    public sealed record Delete(string Name) : FileNamedPosition;
    public sealed record Append(string Name, DocumentTarget Objects) : FileNamedPosition;

    internal Fin<DocumentResourceChange> Apply(RhinoDoc document, Op op) =>
        Switch(
            (Document: document, Op: op),
            save: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from ids in change.Objects.Ids(document: state.Document, op: state.Op)
                from _ in state.Document.NamedPositions.Save(name: name, objectIds: ids.AsIterable()) switch {
                    Guid id when id != Guid.Empty => Fin.Succ(value: unit),
                    _ => Fin.Fail<Unit>(error: state.Op.InvalidResult()),
                }
                select new DocumentResourceChange(Kind: DocumentResourceKind.NamedPosition, Name: name),
            restore: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from _ in state.Op.Confirm(success: state.Document.NamedPositions.Restore(name: name))
                select new DocumentResourceChange(Kind: DocumentResourceKind.NamedPosition, Name: name),
            update: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from _ in state.Op.Confirm(success: state.Document.NamedPositions.Update(name: name))
                select new DocumentResourceChange(Kind: DocumentResourceKind.NamedPosition, Name: name),
            rename: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from next in FileEndpoint.NonBlank(value: change.Next, op: state.Op)
                from _ in state.Op.Confirm(success: state.Document.NamedPositions.Rename(oldName: name, name: next))
                select new DocumentResourceChange(Kind: DocumentResourceKind.NamedPosition, Name: next),
            delete: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from _ in state.Op.Confirm(success: state.Document.NamedPositions.Delete(name: name))
                select new DocumentResourceChange(Kind: DocumentResourceKind.NamedPosition, Name: name),
            append: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from ids in change.Objects.Ids(document: state.Document, op: state.Op)
                from _ in state.Op.Confirm(success: state.Document.NamedPositions.Append(name: name, objectIds: ids.AsIterable()))
                select new DocumentResourceChange(Kind: DocumentResourceKind.NamedPosition, Name: name));
}

public readonly record struct FilePositionReport(string Name, Guid Id, Seq<Guid> Objects);

[Union]
internal abstract partial record HeadlessExchange {
    private HeadlessExchange() { }
    public sealed record Create : HeadlessExchange;
    public sealed record CreateFromTemplate(FileEndpoint Template) : HeadlessExchange;
    public sealed record Open(FileEndpoint Source, FileProfile Profile) : HeadlessExchange;
}

public sealed record FileWatch(
    Option<Action<DocumentOpenEventArgs>> OnBeginOpen = default,
    Option<Action<DocumentOpenEventArgs>> OnEndOpen = default,
    Option<Action<DocumentSaveEventArgs>> OnBeginSave = default,
    Option<Action<DocumentSaveEventArgs>> OnEndSave = default,
    Option<Action<DocumentEventArgs>> OnClose = default,
    Option<Action<DocumentEventArgs>> OnNew = default);

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
                namedLayerState: static (runtime, state) => Commit(runtime: runtime, phase: FilePhase.NamedLayerState, name: nameof(FileExchange.NamedLayerState), change: state.Change, apply: static (document, change, op) => change.Apply(document: document, op: op).Map(static changed => DocumentReceipt.Empty with { ResourceChanged = Seq(changed) })),
                namedPosition: static (runtime, state) => Commit(runtime: runtime, phase: FilePhase.NamedPosition, name: nameof(FileExchange.NamedPosition), change: state.Change, apply: static (document, change, op) => change.Apply(document: document, op: op).Map(static changed => DocumentReceipt.Empty with { ResourceChanged = Seq(changed) })),
                archiveRead: static (_, archive) => FileArchiveOps.Read(source: archive.Source, profile: archive.Profile),
                archiveExtract: static (_, archive) => FileArchiveOps.Extract(source: archive.Source, target: archive.Target, profile: archive.Profile),
                archiveUpdate: static (_, archive) => FileArchiveOps.Update(source: archive.Source, target: archive.Target, update: archive.Update, profile: archive.Profile),
                archiveInspect: static (_, archive) => FileArchiveOps.Inspect(source: archive.Source).Map(metadata => FileReport.Of(
                    phase: FilePhase.ArchiveInspect,
                    source: Some(archive.Source),
                    archive: Some(new FileArchive(Source: new FileArchiveSource.Path(Value: archive.Source), Metadata: metadata, Resources: default, Objects: Seq<FileObjectManifest>())))),
                archiveValidate: static (runtime, archive) => FileArchiveOps.Validate(source: archive.Source, profile: archive.Profile, scheduler: runtime.Scheduler),
                sheetEdit: static (runtime, sheet) => Commit(runtime: runtime, phase: FilePhase.SheetEdit, name: nameof(FileExchange.SheetEdit), change: sheet.Edit, apply: SheetOps.Apply))
            select result);

    public static Eff<FileRuntime, byte[]> ArchiveBytes(FileArchiveSource source, ArchiveProfile profile) =>
        Lift(run: _ => FileArchiveOps.Bytes(source: source, profile: profile));

    public static Eff<FileRuntime, FileReport> Batch(Seq<Eff<FileRuntime, FileReport>> items, FileBatchPolicy policy) =>
        Lift(run: runtime => items switch {
            Seq<Eff<FileRuntime, FileReport>> values when !values.IsEmpty => values.TraverseM(operation =>
                operation.Run(runtime).BindFail(error => policy.ContinueOnError switch {
                    true => Fin.Succ(value: FileReport.Empty(phase: FilePhase.Batch) with { Issues = Seq(FileIssue.Of(code: FileIssueCode.BatchFailure, message: error.Message)) }),
                    false => Fin.Fail<FileReport>(error: error),
                })).As().Map(reports => FileReport.Of(phase: FilePhase.Batch, issues: reports.Bind(static report => report.Issues), children: reports)),
            _ => Fin.Fail<FileReport>(error: Op.Of(name: nameof(Batch)).InvalidInput()),
        });

    public static Eff<FileRuntime, T> Headless<T>(Eff<FileRuntime, T> body) =>
        HeadlessOp(scope: new HeadlessExchange.Create(), body: body);

    public static Eff<FileRuntime, T> Headless<T>(FileEndpoint source, FileProfile profile, Eff<FileRuntime, T> body) =>
        HeadlessOp(scope: new HeadlessExchange.Open(Source: source, Profile: profile), body: body);

    public static Eff<FileRuntime, T> HeadlessTemplate<T>(FileEndpoint template, Eff<FileRuntime, T> body) =>
        HeadlessOp(scope: new HeadlessExchange.CreateFromTemplate(Template: template), body: body);

    public static Eff<FileRuntime, Seq<FileEndpoint>> Prompt(FilePrompt prompt) =>
        Lift(run: runtime =>
            from active in Optional(prompt).ToFin(Fail: Op.Of(name: nameof(Prompt)).InvalidInput())
            from ui in runtime.Ui.ToFin(Fail: Op.Of(name: nameof(Prompt)).MissingContext())
            from result in ui.Use(intent: UiIntent.ExchangeFile(prompt: active))
            select result);

    public static Eff<FileRuntime, IDisposable> Subscribe(FileWatch watch) =>
        Lift(run: _ => FileWatchSubscription.Of(watch: watch));

    // Read concerns — different return shape, so they cannot fold into the Fin<DocumentReceipt> mutation unions.
    public static Eff<FileRuntime, Seq<FileSheetReport>> Sheets(SheetQuery query) =>
        Lift(run: runtime => Live(runtime: runtime, op: Op.Of(name: nameof(Sheets))).Bind(live => SheetOps.Inspect(document: live.Document, query: query, op: Op.Of(name: nameof(Sheets)))));

    public static Eff<FileRuntime, Seq<string>> NamedLayerStates() =>
        Lift(run: runtime => Live(runtime: runtime, op: Op.Of(name: nameof(NamedLayerStates))).Map(static live => toSeq(live.Document.NamedLayerStates.Names)));

    public static Eff<FileRuntime, Seq<FilePositionReport>> NamedPositions() =>
        Lift(run: runtime => Live(runtime: runtime, op: Op.Of(name: nameof(NamedPositions)))
            .Map(static live => toSeq(live.Document.NamedPositions.Ids)
                .Map(id => new FilePositionReport(
                    Name: live.Document.NamedPositions.Name(id: id) ?? string.Empty,
                    Id: id,
                    Objects: toSeq(live.Document.NamedPositions.ObjectIds(id: id))))));

    internal static Eff<FileRuntime, T> HeadlessOp<T>(HeadlessExchange scope, Eff<FileRuntime, T> body) =>
        Lift(run: runtime => HeadlessScope(scope: scope, body: body, scheduler: runtime.Scheduler));

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
                    issues: wasAlreadyOpen ? Seq(FileIssue.Of(code: FileIssueCode.AlreadyOpen, message: "Document was already open.")) : Seq<FileIssue>()));
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

    private static Fin<FileReport> WriteCore(FileRuntime runtime, Option<FileEndpoint> target, FileProfile profile, FilePhase phase) {
        Op op = Op.Of(name: nameof(WriteCore));
        bool archive = phase == FilePhase.SaveAs || phase == FilePhase.Write3dmFile || phase == FilePhase.SaveTemplate;
        return from live in Live(runtime: runtime, op: op)
               from endpoint in phase == FilePhase.Save
                   ? Fin.Succ(Option<FileEndpoint>.None)
                   : target.ToFin(Fail: op.InvalidInput())
                       .Bind(value => (archive ? value.WithFormat(format: FileFormat.ThreeDm) : value).Output(op: op))
                       .Map(Some)
               from receipt in live.Edit.Commit(
                   name: nameof(WriteCore),
                   redraw: DocumentRedraw.None,
                   undoRecorded: false,
                   run: (document, _, inner) => FileFormatProjection.Write(document: document, target: endpoint, profile: profile, phase: phase, selected: false, op: inner).Map(static _ => DocumentReceipt.Empty))
               select FileReport.Of(
                   phase: phase,
                   target: endpoint,
                   format: endpoint.Case switch {
                       FileEndpoint value => FileFormatProjection.Resolve(endpoint: value, profile: profile),
                       _ => profile.Format,
                   },
                   receipt: Some(receipt));
    }

    private static Fin<FileReport> PublishCore(FileRuntime runtime, FilePublish publish) {
        Op op = Op.Of(name: nameof(FileExchange.Publish));
        return from live in Live(runtime: runtime, op: op)
               from active in Optional(publish).ToFin(Fail: op.InvalidInput())
               from target in Optional(active.Target).ToFin(Fail: op.InvalidInput())
               let views = active.Views.IsEmpty ? Seq(new FileView(Source: new FileViewSource.Pages())) : active.Views
               from pages in views.TraverseM(view => view.Source.Resolve(document: live.Document, spec: view, op: op)).As().Map(static groups => groups.Bind(static items => items))
               from _ in guard(!pages.IsEmpty, op.InvalidInput())
               from result in active.Snapshot.Case switch {
                   string snapshot => InSnapshot(document: live.Document, snapshotName: snapshot, op: op, body: () => RhinoUi.Protect(valid: () => target.Write(pages: pages, layers: active.Layers, op: op))),
                   _ => RhinoUi.Protect(valid: () => target.Write(pages: pages, layers: active.Layers, op: op)),
               }
               select FileReport.Of(phase: FilePhase.Publish, target: result.Target, format: result.Format, issues: result.Issues, nativeLog: result.NativeLog, receipt: Some(result.Receipt), views: result.Views);
    }

    // SnapshotTable exposes only `Names[]`; save/restore/delete route through `RhinoApp.RunScript("-_Snapshot ...")`.
    // Guid-N sentinel preserves the user's snapshot table across the restore-target body bracket.
    private static Fin<T> InSnapshot<T>(RhinoDoc document, string snapshotName, Op op, Func<Fin<T>> body) =>
        from target in FileEndpoint.NonBlank(value: snapshotName, op: op)
        from _exists in toSeq(document.Snapshots.Names).Exists(name => string.Equals(a: name, b: target, comparisonType: StringComparison.Ordinal))
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: op.InvalidInput())
        let sentinel = string.Create(CultureInfo.InvariantCulture, $"__rasm_publish_{Guid.NewGuid():N}")
        from _saved in SnapshotScript(serial: document.RuntimeSerialNumber, verb: "_Save", name: sentinel, op: op)
        from result in op.Catch(() => {
            try {
                return from _restored in SnapshotScript(serial: document.RuntimeSerialNumber, verb: "_Restore", name: target, op: op)
                       from value in Optional(body).ToFin(Fail: op.InvalidInput()).Bind(valid => valid())
                       select value;
            } finally {
                _ = SnapshotScript(serial: document.RuntimeSerialNumber, verb: "_Restore", name: sentinel, op: op);
                _ = SnapshotScript(serial: document.RuntimeSerialNumber, verb: "_Delete", name: sentinel, op: op);
            }
        })
        select result;

    private static Fin<Unit> SnapshotScript(uint serial, string verb, string name, Op op) =>
        op.Catch(() => op.Confirm(success: RhinoApp.RunScript(documentSerialNumber: serial, script: string.Create(CultureInfo.InvariantCulture, $"-_Snapshot {verb} _Name {name} _Enter"), echo: false)));

    // One mutation shell for every doc-resource edit: resolve live runtime, null-guard the change, run the typed
    // apply inside an undo-recorded commit, project the receipt onto the phase report. T : class so Optional binds.
    private static Fin<FileReport> Commit<T>(FileRuntime runtime, FilePhase phase, string name, T change, Func<RhinoDoc, T, Op, Fin<DocumentReceipt>> apply) where T : class =>
        from live in Live(runtime: runtime, op: Op.Of(name: name))
        from active in Optional(change).ToFin(Fail: Op.Of(name: name).InvalidInput())
        from receipt in live.Edit.Commit(name: name, redraw: DocumentRedraw.After, undoRecorded: true, run: (document, _, op) => apply(arg1: document, arg2: active, arg3: op))
        select FileReport.Of(phase: phase, receipt: Some(receipt));

    private static Fin<T> HeadlessScope<T>(HeadlessExchange scope, Eff<FileRuntime, T> body, IoScheduler scheduler) =>
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
                            .BindFail(error => (source.Format.Case, open.Profile.Scale.IsSome) switch {
                                (FileFormat, _) or (_, true) => Fin.Fail<ArchivableDictionary>(error: error),
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
                        ui: Some(new RhinoUi(document: document, mode: RunMode.Scripted)),
                        scheduler: scheduler)));
                });
            } finally {
                headless?.Dispose();
            }
        });

    private static Fin<DocumentReceipt> ExportTarget(RhinoDoc document, DocumentTarget target, FileEndpoint endpoint, FileProfile profile, Op op) =>
        // BOUNDARY ADAPTER — export selected mutates Rhino selection temporarily and must restore it.
        op.Catch(() => DocumentEdit.SelectedIds(document: document, op: op).Bind(before => {
            try {
                // The `count == ids.Count` guard on SetSelectedObjects already confirms the full set was selected;
                // a post-set ordered readback breaks on locked/grip ids Rhino refuses and on duplicate-id multisets.
                return from ids in target.Ids(document: document, op: op)
                       from _ in op.Confirm(success: document.Objects.UnselectAll(ignorePersistentSelections: false) >= 0)
                       from __ in document.Objects.SetSelectedObjects(objectIds: ids.AsIterable(), syncHighlight: true, persistentSelect: false, ignoreGripsState: true, ignoreLayerLocking: false, ignoreLayerVisibility: false) switch {
                           int count when count == ids.Count => Fin.Succ(value: unit),
                           _ => Fin.Fail<Unit>(error: op.InvalidResult()),
                       }
                       from ___ in FileFormatProjection.Write(document: document, target: Some(endpoint), profile: profile, phase: FilePhase.Export, selected: true, op: op)
                       select DocumentReceipt.Empty with { Selected = ids };
            } finally {
                _ = document.Objects.UnselectAll(ignorePersistentSelections: false);
                _ = before.IsEmpty
                    ? 0
                    : document.Objects.SetSelectedObjects(objectIds: before.AsIterable(), syncHighlight: true, persistentSelect: true, ignoreGripsState: true, ignoreLayerLocking: false, ignoreLayerVisibility: false);
            }
        }));

    private static Fin<(RhinoDoc Document, Context Domain, DocumentEdit Edit)> Live(FileRuntime runtime, Op op) =>
        (runtime.Document.Case, runtime.Domain.Case, runtime.Edit.Case) switch {
            (RhinoDoc document, Context domain, DocumentEdit edit) => Fin.Succ(value: (document, domain, edit)),
            _ => Fin.Fail<(RhinoDoc Document, Context Domain, DocumentEdit Edit)>(error: op.MissingContext()),
        };
}

// --- [COMPOSITION] ------------------------------------------------------------------------

// `GetModelToEarthTransform(LengthUnit)` is canonical; the `UnitSystem` overload is `[Obsolete]`.
// `SyncSun` encodes the Sun.North↔ModelNorth invariant `_SetGeoLocation` does not honour.
public static class FileEarthOps {
    public static Fin<Option<FileGeoLocation>> Read(RhinoDoc document) {
        Op op = Op.Of(name: nameof(Read));
        return Optional(document).ToFin(Fail: op.InvalidInput()).Bind(active => op.Catch(() => {
            using EarthAnchorPoint? anchor = active.EarthAnchorPoint;
            return Fin.Succ(value: FileGeoLocation.From(anchor: anchor));
        }));
    }

    public static Fin<DocumentReceipt> Set(RhinoDoc document, FileGeoLocation location) {
        Op op = Op.Of(name: nameof(Set));
        return Optional(document).ToFin(Fail: op.InvalidInput()).Bind(active => op.Catch(() => {
            using EarthAnchorPoint anchor = new();
            _ = location.Latitude.Iter(value => anchor.EarthBasepointLatitude = value);
            _ = location.Longitude.Iter(value => anchor.EarthBasepointLongitude = value);
            _ = location.Elevation.Iter(value => anchor.EarthBasepointElevation = value);
            anchor.EarthBasepointElevationCoordinateSystem = location.ElevationCoordinateSystem;
            _ = location.ModelBasePoint.Iter(value => anchor.ModelBasePoint = value);
            _ = location.ModelNorth.Iter(value => anchor.ModelNorth = value);
            _ = location.ModelEast.Iter(value => anchor.ModelEast = value);
            _ = location.Name.Iter(value => anchor.Name = value);
            _ = location.Description.Iter(value => anchor.Description = value);
            active.EarthAnchorPoint = anchor;
            return Fin.Succ(value: DocumentReceipt.Empty with {
                ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.EarthAnchor, Name: location.Name.IfNone(noneValue: "earth-anchor"))),
            });
        }));
    }

    // `GetModelToEarthTransform` maps model XYZ -> earth frame `E.x=latitude°, E.y=longitude°, E.z=elevation(m)`;
    // the `(Latitude, Longitude, Elevation)` tuple labels mirror that frame and are NOT transposed.
    public static Fin<Seq<(double Latitude, double Longitude, double Elevation)>> ProjectToEarth(RhinoDoc document, Seq<Point3d> points, LengthUnit modelUnits) =>
        UseAnchor(document: document, op: Op.Of(name: nameof(ProjectToEarth)), requireEarth: true, use: (anchor, op) => {
            Transform xform = anchor.GetModelToEarthTransform(modelUnits: modelUnits);
            return xform.IsValid switch {
                false => Fin.Fail<Seq<(double, double, double)>>(error: op.InvalidResult()),
                true => Fin.Succ(value: points.Map(point => {
                    Point3d projected = point;
                    projected.Transform(xform: xform);
                    return (projected.X, projected.Y, projected.Z);
                })),
            };
        });

    public static Fin<Seq<Point3d>> ProjectToModel(RhinoDoc document, Seq<(double Latitude, double Longitude, double Elevation)> coordinates, LengthUnit modelUnits) =>
        UseAnchor(document: document, op: Op.Of(name: nameof(ProjectToModel)), requireEarth: true, use: (anchor, op) => {
            Transform xform = anchor.GetModelToEarthTransform(modelUnits: modelUnits);
            return xform.TryGetInverse(inverseTransform: out Transform inverse) switch {
                false => Fin.Fail<Seq<Point3d>>(error: op.InvalidResult()),
                true => Fin.Succ(value: coordinates.Map(coordinate => {
                    Point3d earth = new(x: coordinate.Latitude, y: coordinate.Longitude, z: coordinate.Elevation);
                    earth.Transform(xform: inverse);
                    return earth;
                })),
            };
        });

    public static Fin<Plane> Compass(RhinoDoc document) =>
        UseAnchor(document: document, op: Op.Of(name: nameof(Compass)), requireModel: true, use: static (anchor, op) =>
            anchor.GetModelCompass() switch {
                Plane plane when plane.IsValid => Fin.Succ(value: plane),
                _ => Fin.Fail<Plane>(error: op.InvalidResult()),
            });

    public static Fin<Plane> AnchorPlane(RhinoDoc document) =>
        UseAnchor(document: document, op: Op.Of(name: nameof(AnchorPlane)), requireModel: true, use: static (anchor, op) =>
            anchor.GetEarthAnchorPlane(anchorNorth: out _) switch {
                Plane plane when plane.IsValid => Fin.Succ(value: plane),
                _ => Fin.Fail<Plane>(error: op.InvalidResult()),
            });

    public static Fin<Transform> OrientPlane(RhinoDoc document, Plane source) =>
        UseAnchor(document: document, op: Op.Of(name: nameof(OrientPlane)), requireModel: true, use: (anchor, op) => {
            Plane target = anchor.GetEarthAnchorPlane(anchorNorth: out _);
            return (source.IsValid, target.IsValid) switch {
                (true, true) => Fin.Succ(value: Transform.PlaneToPlane(plane0: source, plane1: target)),
                _ => Fin.Fail<Transform>(error: op.InvalidInput()),
            };
        });

    // `Sun.North` is degrees on world XY; project from `ModelNorth` via `Atan2(Y, X)` so sun studies
    // reflect anchor-defined true north. `_SetGeoLocation` does NOT — call SyncSun after.
    public static Fin<DocumentReceipt> SyncSun(RhinoDoc document) =>
        UseAnchor(document: document, op: Op.Of(name: nameof(SyncSun)), requireEarth: true, requireModel: true, use: (anchor, op) => op.Catch(() => {
            global::Rhino.Render.Sun sun = document.RenderSettings.Sun;
            Vector3d north = anchor.ModelNorth;
            sun.Latitude = anchor.EarthBasepointLatitude;
            sun.Longitude = anchor.EarthBasepointLongitude;
            sun.North = Math.Atan2(y: north.Y, x: north.X) * (180.0 / Math.PI);
            return Fin.Succ(value: DocumentReceipt.Empty with {
                ResourceChanged = Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Sun, Name: "sun")),
            });
        }));

    private static Fin<T> UseAnchor<T>(RhinoDoc document, Op op, Func<EarthAnchorPoint, Op, Fin<T>> use, bool requireEarth = false, bool requireModel = false) =>
        Optional(document).ToFin(Fail: op.InvalidInput()).Bind(active => op.Catch(() => {
            using EarthAnchorPoint? anchor = active.EarthAnchorPoint;
            return Optional(anchor).ToFin(Fail: op.InvalidResult()).Bind(valid =>
                (requireEarth && !valid.EarthLocationIsSet(), requireModel && !valid.ModelLocationIsSet()) switch {
                    (false, false) => use(arg1: valid, arg2: op),
                    _ => Fin.Fail<T>(error: op.InvalidInput()),
                });
        }));
}

internal sealed class FileWatchSubscription : IDisposable {
    private readonly Seq<Action> detachers;
    private FileWatchSubscription(Seq<Action> detachers) => this.detachers = detachers;

    internal static Fin<IDisposable> Of(FileWatch watch) =>
        Optional(watch).ToFin(Fail: Op.Of(name: nameof(FileWatchSubscription)).InvalidInput())
            .Map<IDisposable>(active => new FileWatchSubscription(detachers:
                Attach(handler: active.OnBeginOpen, add: static h => RhinoDoc.BeginOpenDocument += h, remove: static h => RhinoDoc.BeginOpenDocument -= h)
                + Attach(handler: active.OnEndOpen, add: static h => RhinoDoc.EndOpenDocument += h, remove: static h => RhinoDoc.EndOpenDocument -= h)
                + Attach(handler: active.OnBeginSave, add: static h => RhinoDoc.BeginSaveDocument += h, remove: static h => RhinoDoc.BeginSaveDocument -= h)
                + Attach(handler: active.OnEndSave, add: static h => RhinoDoc.EndSaveDocument += h, remove: static h => RhinoDoc.EndSaveDocument -= h)
                + Attach(handler: active.OnClose, add: static h => RhinoDoc.CloseDocument += h, remove: static h => RhinoDoc.CloseDocument -= h)
                + Attach(handler: active.OnNew, add: static h => RhinoDoc.NewDocument += h, remove: static h => RhinoDoc.NewDocument -= h)));

    private static Seq<Action> Attach<TArgs>(Option<Action<TArgs>> handler, Action<EventHandler<TArgs>> add, Action<EventHandler<TArgs>> remove) where TArgs : EventArgs =>
        handler.Map(action => {
            void Wrapper(object? _, TArgs args) => action(args);
            add(Wrapper);
            return Seq(() => remove(Wrapper));
        }).IfNone(Seq<Action>());

    public void Dispose() => detachers.Iter(detach => detach()).Ignore();
}
