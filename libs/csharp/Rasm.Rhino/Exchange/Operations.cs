using System.Globalization;
using Rasm.Rhino.Commands;
using Rasm.Rhino.UI;
using Rhino.Collections;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Exchange;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class FileNativePositionKind {
    public static readonly FileNativePositionKind Restore = new(key: 0, apply: static (table, name) => table.Restore(name: name));
    public static readonly FileNativePositionKind Update = new(key: 1, apply: static (table, name) => table.Update(name: name));
    public static readonly FileNativePositionKind Delete = new(key: 2, apply: static (table, name) => table.Delete(name: name));

    [UseDelegateFromConstructor]
    internal partial bool Apply(NamedPositionTable table, string name);
}

[Union(SwitchMapStateParameterName = "state")]
public abstract partial record FileNativeTable {
    private FileNativeTable() { }
    public sealed record SaveLayerState(string Name, Option<Guid> Viewport = default) : FileNativeTable;
    public sealed record RestoreLayerState(string Name, RestoreLayerProperties Properties, Option<Guid> Viewport = default) : FileNativeTable;
    public sealed record RenameLayerState(string Name, string Next) : FileNativeTable;
    public sealed record DeleteLayerState(string Name) : FileNativeTable;
    public sealed record ImportLayerState(FileEndpoint Source) : FileNativeTable;
    public sealed record SavePosition(string Name, DocumentTarget Objects) : FileNativeTable;
    public sealed record Position(string Name, FileNativePositionKind Kind) : FileNativeTable;
    public sealed record RenamePosition(string Name, string Next) : FileNativeTable;
    public sealed record AppendPosition(string Name, DocumentTarget Objects) : FileNativeTable;

    internal FilePhase Phase => this is SaveLayerState or RestoreLayerState or RenameLayerState or DeleteLayerState or ImportLayerState ? FilePhase.NamedLayerState : FilePhase.NamedPosition;

    internal Fin<DocumentResourceChange> Apply(RhinoDoc document, Op op) =>
        Switch(
            (Document: document, Op: op),
            saveLayerState: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from _ in state.Op.Confirm(success: state.Document.NamedLayerStates.Save(name: name, viewportId: change.Viewport.IfNone(Guid.Empty)) >= 0)
                select Changed(kind: DocumentResourceKind.NamedLayerState, name: name),
            restoreLayerState: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from _ in state.Op.Confirm(success: state.Document.NamedLayerStates.Restore(name: name, properties: change.Properties, viewportId: change.Viewport.IfNone(Guid.Empty)))
                select Changed(kind: DocumentResourceKind.NamedLayerState, name: name),
            renameLayerState: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from next in FileEndpoint.NonBlank(value: change.Next, op: state.Op)
                from _ in state.Op.Confirm(success: state.Document.NamedLayerStates.Rename(oldName: name, newName: next))
                select Changed(kind: DocumentResourceKind.NamedLayerState, name: next),
            deleteLayerState: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from _ in state.Op.Confirm(success: state.Document.NamedLayerStates.Delete(name: name))
                select Changed(kind: DocumentResourceKind.NamedLayerState, name: name),
            importLayerState: static (state, change) =>
                from source in change.Source.Input(op: state.Op)
                from _ in state.Op.Confirm(success: state.Document.NamedLayerStates.Import(filename: source.Path) >= 0)
                select Changed(kind: DocumentResourceKind.NamedLayerState, name: source.Path),
            savePosition: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from ids in change.Objects.Ids(document: state.Document, op: state.Op)
                from _ in state.Op.Confirm(success: state.Document.NamedPositions.Save(name: name, objectIds: ids.AsIterable()) != Guid.Empty)
                select Changed(kind: DocumentResourceKind.NamedPosition, name: name),
            position: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from _ in state.Op.Confirm(success: change.Kind.Apply(table: state.Document.NamedPositions, name: name))
                select Changed(kind: DocumentResourceKind.NamedPosition, name: name),
            renamePosition: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from next in FileEndpoint.NonBlank(value: change.Next, op: state.Op)
                from _ in state.Op.Confirm(success: state.Document.NamedPositions.Rename(oldName: name, name: next))
                select Changed(kind: DocumentResourceKind.NamedPosition, name: next),
            appendPosition: static (state, change) =>
                from name in FileEndpoint.NonBlank(value: change.Name, op: state.Op)
                from ids in change.Objects.Ids(document: state.Document, op: state.Op)
                from _ in state.Op.Confirm(success: state.Document.NamedPositions.Append(name: name, objectIds: ids.AsIterable()))
                select Changed(kind: DocumentResourceKind.NamedPosition, name: name));

    private static DocumentResourceChange Changed(DocumentResourceKind kind, string name) => new(Kind: kind, Name: name);
}

[Union(SwitchMapStateParameterName = "runtime")]
public abstract partial record FileExchange {
    private FileExchange() { }
    public sealed record Open(FileEndpoint Source, FileProfile Profile) : FileExchange;
    public sealed record Import(FileEndpoint Source, FileProfile Profile) : FileExchange;
    public sealed record Export(FileEndpoint Target, Option<DocumentTarget> Objects, FileProfile Profile) : FileExchange;
    public sealed record Save : FileExchange;
    public sealed record Write(FilePhase Phase, FileEndpoint Target, FileProfile Profile) : FileExchange;
    public sealed record Publish(FilePublish Spec) : FileExchange;
    public sealed record NativeTable(FileNativeTable Change) : FileExchange;
    public sealed record ArchiveRead(FileArchiveSource Source, ArchiveProfile Profile) : FileExchange;
    public sealed record ArchiveExtract(FileArchiveSource Source, FileEndpoint Target, ArchiveProfile Profile) : FileExchange;
    public sealed record ArchiveUpdate(FileArchiveSource Source, FileEndpoint Target, Exchange.ArchiveUpdate Update, ArchiveProfile Profile) : FileExchange;
    public sealed record ArchiveInspect(FileEndpoint Source) : FileExchange;
    public sealed record ArchiveDiff(FileEndpoint Source, FileEndpoint Other) : FileExchange;
    public sealed record ArchiveValidate(FileArchiveSource Source, ArchiveProfile Profile) : FileExchange;
    public sealed record SheetEdit(FileSheetEdit Edit) : FileExchange;
}

[Union]
public abstract partial record HeadlessExchange {
    private HeadlessExchange() { }
    public sealed record Create : HeadlessExchange;
    public sealed record CreateFromTemplate(FileEndpoint Template) : HeadlessExchange;
    public sealed record Open(FileEndpoint Source, FileProfile Profile) : HeadlessExchange;
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct FilePositionReport(string Name, Guid Id, Seq<Guid> Objects);

// --- [OPERATIONS] -------------------------------------------------------------------------
public readonly partial record struct FileGeoLocation {
    public static Fin<Option<FileGeoLocation>> Read(RhinoDoc document) {
        Op op = Op.Of(name: nameof(Read));
        return Optional(document).ToFin(Fail: op.InvalidInput()).Bind(active => op.Catch(() => {
            using EarthAnchorPoint? anchor = active.EarthAnchorPoint;
            return Fin.Succ(value: From(anchor: anchor));
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
            return Fin.Succ(value: DocumentReceipt.Resource(kind: DocumentResourceKind.EarthAnchor, name: location.Name.IfNone(noneValue: "earth-anchor")));
        }));
    }

    public static Fin<Plane> AnchorPlane(RhinoDoc document) =>
        UseAnchor(document: document, op: Op.Of(name: nameof(AnchorPlane)), requireModel: true, use: static (anchor, op) =>
            op.AcceptValue(value: anchor.GetEarthAnchorPlane(anchorNorth: out _)));

    public static Fin<Plane> Compass(RhinoDoc document) =>
        UseAnchor(document: document, op: Op.Of(name: nameof(Compass)), requireModel: true, use: static (anchor, op) =>
            op.AcceptValue(value: anchor.GetModelCompass()));

    public static Fin<Transform> OrientPlane(RhinoDoc document, Plane source) =>
        UseAnchor(document: document, op: Op.Of(name: nameof(OrientPlane)), requireModel: true, use: (anchor, op) => {
            Plane target = anchor.GetEarthAnchorPlane(anchorNorth: out _);
            return (source.IsValid, target.IsValid) switch {
                (true, true) => Fin.Succ(value: Transform.PlaneToPlane(plane0: source, plane1: target)),
                _ => Fin.Fail<Transform>(error: op.InvalidInput()),
            };
        });

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

    public static Fin<DocumentReceipt> SyncSun(RhinoDoc document) =>
        UseAnchor(document: document, op: Op.Of(name: nameof(SyncSun)), requireEarth: true, requireModel: true, use: (anchor, op) => op.Catch(() => {
            global::Rhino.Render.Sun sun = document.RenderSettings.Sun;
            Vector3d north = anchor.ModelNorth;
            sun.Latitude = anchor.EarthBasepointLatitude;
            sun.Longitude = anchor.EarthBasepointLongitude;
            sun.North = Math.Atan2(y: north.Y, x: north.X) * (180.0 / Math.PI);
            return Fin.Succ(value: DocumentReceipt.Resource(kind: DocumentResourceKind.Sun, name: "sun"));
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

public static class FileOp {
    public static Eff<FileRuntime, FileReport> Do(FileExchange exchange) =>
        Lift(run: runtime =>
            from active in Optional(exchange).ToFin(Fail: Op.Of(name: nameof(Do)).InvalidInput())
            from result in active.Switch(
                runtime,
                open: static (_, open) => OpenCore(source: open.Source, profile: open.Profile),
                import: static (runtime, import) => ImportCore(runtime: runtime, source: import.Source, profile: import.Profile),
                export: static (runtime, export) => WriteCore(runtime: runtime, target: Some(export.Target), objects: export.Objects, profile: export.Profile, phase: FilePhase.Export),
                save: static (runtime, _) => WriteCore(runtime: runtime, target: Option<FileEndpoint>.None, profile: FileProfile.Model, phase: FilePhase.Save),
                write: static (runtime, write) => WriteCore(runtime: runtime, target: Some(write.Target), profile: write.Profile, phase: write.Phase),
                publish: static (runtime, publish) => PublishCore(runtime: runtime, publish: publish.Spec),
                nativeTable: static (runtime, state) => Commit(runtime: runtime, phase: state.Change.Phase, name: nameof(FileExchange.NativeTable), change: state.Change, apply: static (document, change, op) => change.Apply(document: document, op: op).Map(static changed => DocumentReceipt.Resources(changes: Seq(changed)))),
                archiveRead: static (_, archive) => FileArchiveOps.Read(source: archive.Source, profile: archive.Profile),
                archiveExtract: static (_, archive) => FileArchiveOps.Extract(source: archive.Source, target: archive.Target, profile: archive.Profile),
                archiveUpdate: static (_, archive) => FileArchiveOps.Update(source: archive.Source, target: archive.Target, update: archive.Update, profile: archive.Profile),
                archiveInspect: static (_, archive) => FileArchiveOps.Inspect(source: archive.Source).Map(metadata => FileReport.Of(
                    phase: FilePhase.ArchiveInspect,
                    source: Some(archive.Source),
                    archive: Some(new FileArchive(Source: new FileArchiveSource.Path(Value: archive.Source), Metadata: metadata, Resources: default, Objects: Seq<FileObjectManifest>())))),
                archiveDiff: static (_, diff) => FileArchiveOps.Diff(source: diff.Source, other: diff.Other).Map(result => FileReport.Of(
                    phase: FilePhase.ArchiveDiff,
                    source: Some(diff.Source),
                    target: Some(diff.Other),
                    diff: Some(result))),
                archiveValidate: static (runtime, archive) => FileArchiveOps.Validate(source: archive.Source, profile: archive.Profile, scheduler: runtime.Scheduler),
                sheetEdit: static (runtime, sheet) => Commit(runtime: runtime, phase: FilePhase.SheetEdit, name: nameof(FileExchange.SheetEdit), change: sheet.Edit, apply: SheetOps.Apply))
            select result);

    public static Eff<FileRuntime, Seq<FileEndpoint>> Prompt(FilePrompt prompt) =>
        Lift(run: runtime =>
            from active in Optional(prompt).ToFin(Fail: Op.Of(name: nameof(Prompt)).InvalidInput())
            from ui in runtime.Ui.ToFin(Fail: Op.Of(name: nameof(Prompt)).MissingContext())
            from result in ui.Use(intent: UiIntent.ExchangeFile(prompt: active))
            select result);

    public static Eff<FileRuntime, T> Headless<T>(HeadlessExchange scope, Eff<FileRuntime, T> body) =>
        Lift(run: runtime =>
            from active in Optional(scope).ToFin(Fail: Op.Of(name: nameof(Headless)).InvalidInput())
            from result in HeadlessScope(scope: active, body: body, scheduler: runtime.Scheduler)
            select result);

    public static Eff<FileRuntime, FileReport> Batch(Seq<Eff<FileRuntime, FileReport>> items, FileBatchPolicy policy) =>
        Lift(run: runtime => items switch {
            Seq<Eff<FileRuntime, FileReport>> values when !values.IsEmpty => values.TraverseM(operation =>
                operation.Run(runtime).BindFail(error => guard(policy.ContinueOnError, error).ToFin()
                    .Map(_ => FileReport.Empty(phase: FilePhase.Batch) with { Issues = Seq(FileIssue.Of(code: FileIssueCode.BatchFailure, message: error.Message)) }))).As()
                    .Map(reports => FileReport.Of(phase: FilePhase.Batch, issues: reports.Bind(static report => report.Issues), children: reports)),
            _ => Fin.Fail<FileReport>(error: Op.Of(name: nameof(Batch)).InvalidInput()),
        });

    public static Eff<FileRuntime, FileReport> PublishPlan(FilePublish publish) =>
        Lift(run: runtime =>
            from live in Live(runtime: runtime, op: Op.Of(name: nameof(PublishPlan)))
            from active in Optional(publish).ToFin(Fail: Op.Of(name: nameof(PublishPlan)).InvalidInput())
            let views = active.Views.IsEmpty ? Seq(new FileView(Source: new FileViewSource.Pages())) : active.Views
            from pages in views.TraverseM(view => view.Source.Resolve(document: live.Document, spec: view, op: Op.Of(name: nameof(PublishPlan)))).As().Map(static groups => groups.Bind(static items => items))
            from _ in guard(!pages.IsEmpty, Op.Of(name: nameof(PublishPlan)).InvalidInput())
            from reports in pages.TraverseM(page =>
                from settings in page.Settings(op: Op.Of(name: nameof(PublishPlan)))
                from report in Op.Of(name: nameof(PublishPlan)).Catch(() => {
                    try {
                        return Fin.Succ(value: page.ReportOf(settings: settings));
                    } finally {
                        settings.Dispose();
                    }
                })
                select report).As()
            let meta = PublishMeta(target: active.Target, views: views)
            select FileReport.Of(
                phase: FilePhase.Publish,
                target: meta.Target,
                format: meta.Format,
                issues: meta.Issues,
                nativeLog: Some(string.Create(CultureInfo.InvariantCulture, $"publish-plan;views:{pages.Count};layers:{active.Layers};snapshot:{active.Snapshot.IsSome}")),
                views: reports));

    public static Eff<FileRuntime, byte[]> ArchiveBytes(FileArchiveSource source, ArchiveProfile profile) =>
        Lift(run: _ => FileArchiveOps.Bytes(source: source, profile: profile));

    public static Eff<FileRuntime, Seq<FileSheetReport>> Sheets(SheetQuery query, DetailQuery detail = default) =>
        Lift(run: runtime =>
            Live(runtime: runtime, op: Op.Of(name: nameof(Sheets)))
                .Bind(live => SheetOps.Inspect(document: live.Document, query: query, detail: detail, op: Op.Of(name: nameof(Sheets)))));

    public static Eff<FileRuntime, Seq<string>> NamedLayerStates() =>
        Lift(run: runtime => Live(runtime: runtime, op: Op.Of(name: nameof(NamedLayerStates))).Map(static live => toSeq(live.Document.NamedLayerStates.Names)));

    public static Eff<FileRuntime, Seq<FilePositionReport>> NamedPositions() =>
        Lift(run: runtime => Live(runtime: runtime, op: Op.Of(name: nameof(NamedPositions)))
            .Map(static live => toSeq(live.Document.NamedPositions.Ids)
                .Map(id => new FilePositionReport(
                    Name: live.Document.NamedPositions.Name(id: id) ?? string.Empty,
                    Id: id,
                    Objects: toSeq(live.Document.NamedPositions.ObjectIds(id: id))))));

    private static Fin<T> HeadlessScope<T>(HeadlessExchange scope, Eff<FileRuntime, T> body, IoScheduler scheduler) =>
        Op.Of(name: nameof(Headless)).Catch(() => {
            // BOUNDARY ADAPTER — RhinoDoc.CreateHeadless/OpenHeadless yields an unmanaged document; the finally disposes it even when body fails.
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
                        from options in FileFormat.Dictionary(endpoint: source, profile: open.Profile, phase: FilePhase.Headless, op: Op.Of(name: nameof(Headless)))
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

    internal static Eff<FileRuntime, T> Lift<T>(Func<FileRuntime, Fin<T>> run) =>
        Eff<FileRuntime, T>.Lift(runtime =>
            Optional(run)
                .ToFin(Fail: Op.Of(name: nameof(Lift)).InvalidInput())
                .Bind(valid => valid(arg: runtime)));

    private static Fin<(RhinoDoc Document, Context Domain, DocumentEdit Edit)> Live(FileRuntime runtime, Op op) =>
        (runtime.Document.Case, runtime.Domain.Case, runtime.Edit.Case) switch {
            (RhinoDoc document, Context domain, DocumentEdit edit) => Fin.Succ(value: (document, domain, edit)),
            _ => Fin.Fail<(RhinoDoc Document, Context Domain, DocumentEdit Edit)>(error: op.MissingContext()),
        };

    private static Fin<FileReport> OpenCore(FileEndpoint source, FileProfile profile) =>
        from endpoint in source.Input(op: Op.Of(name: nameof(FileExchange.Open)))
        from format in FileFormat.Require(endpoint: endpoint, profile: profile, phase: FilePhase.Open, op: Op.Of(name: nameof(FileExchange.Open)))
        from _ in guard(format.Is(key: "3dm"), Op.Of(name: nameof(FileExchange.Open)).InvalidInput())
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
                from imported in FileFormat.Import(document: document, source: endpoint, profile: profile, op: op)
                from after in DocumentEdit.LiveObjectIds(document: document)
                select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Created, ids: after.Filter(id => !before.Exists(item => item == id))))
        select FileReport.Of(phase: FilePhase.Import, source: Some(endpoint), target: Option<FileEndpoint>.None, format: FileFormat.Resolve(endpoint: endpoint, profile: profile), receipt: Some(receipt));

    private static Fin<FileReport> WriteCore(FileRuntime runtime, Option<FileEndpoint> target, FileProfile profile, FilePhase phase, Option<DocumentTarget> objects = default) {
        Op op = Op.Of(name: nameof(WriteCore));
        bool archive = phase == FilePhase.SaveAs || phase == FilePhase.Write3dmFile || phase == FilePhase.SaveTemplate;
        return from live in Live(runtime: runtime, op: op)
               from endpoint in phase == FilePhase.Save
                   ? Fin.Succ(Option<FileEndpoint>.None)
                   : target.ToFin(Fail: op.InvalidInput())
                       .Bind(value => (archive
                           ? FileFormat.KnownFormat(key: "3dm", op: op).Map(value.WithFormat)
                           : Fin.Succ(value: value)).Bind(resolved => resolved.Output(op: op)))
                       .Map(Some)
               from receipt in live.Edit.Commit(
                   name: nameof(WriteCore),
                   redraw: DocumentRedraw.None,
                   undoRecorded: false,
                   run: (document, _, inner) => (phase == FilePhase.Export, objects.Case, endpoint.Case) switch {
                       (true, DocumentTarget selection, FileEndpoint output) => ExportTarget(document: document, target: selection, endpoint: output, profile: profile, op: inner),
                       _ => FileFormat.Write(document: document, target: endpoint, profile: profile, phase: phase, selected: false, op: inner).Map(static _ => DocumentReceipt.Empty),
                   })
               select FileReport.Of(
                   phase: phase,
                   target: endpoint,
                   format: endpoint.Case switch {
                       FileEndpoint value => FileFormat.Resolve(endpoint: value, profile: profile),
                       _ => profile.Format,
                   },
                   receipt: Some(receipt));
    }

    private static Fin<DocumentReceipt> ExportTarget(RhinoDoc document, DocumentTarget target, FileEndpoint endpoint, FileProfile profile, Op op) =>
        op.Catch(() => DocumentEdit.SelectedIds(document: document, op: op).Bind(before => {
            // BOUNDARY ADAPTER — ExportSelected mutates global selection state; the finally eagerly relocks the prior persistent/transient partition even on export failure.
            const int persistentSelectionState = 2;
            static bool Persistent(RhinoObject native) => native.IsSelected(checkSubObjects: false) == persistentSelectionState;
            Seq<Guid> persistent = before.Filter(id => Optional(document.Objects.FindId(id)).Map(Persistent).IfNone(noneValue: false));
            Seq<Guid> transient = before.Filter(id => !persistent.Exists(item => item == id));
            Seq<(Seq<Guid> Ids, bool Persistent)> restore = Seq((transient, false), (persistent, true));
            Fin<Unit> restored = Fin.Succ(value: unit);
            Fin<DocumentReceipt> exported = Fin.Fail<DocumentReceipt>(error: op.InvalidResult());
            try {
                exported = from ids in target.Ids(document: document, op: op)
                           from _ in op.Confirm(success: document.Objects.UnselectAll(ignorePersistentSelections: false) >= 0)
                           from __ in op.Confirm(success: document.Objects.SetSelectedObjects(objectIds: ids.AsIterable(), syncHighlight: true, persistentSelect: false, ignoreGripsState: true, ignoreLayerLocking: false, ignoreLayerVisibility: false) == ids.Count)
                           from ___ in FileFormat.Write(document: document, target: Some(endpoint), profile: profile, phase: FilePhase.Export, selected: true, op: op)
                           select DocumentReceipt.Objects(slot: DocumentReceiptSlot.Selected, ids: ids);
            } finally {
                restored =
                    op.Catch(() => op.Confirm(success: document.Objects.UnselectAll(ignorePersistentSelections: false) >= 0))
                        .Bind(_ => restore.Filter(static item => !item.Ids.IsEmpty).TraverseM(item => op.Catch(() => {
                            int count = document.Objects.Select(
                                objectIds: item.Ids.AsIterable(),
                                select: true,
                                syncHighlight: true,
                                persistentSelect: item.Persistent,
                                ignoreGripsState: true,
                                ignoreLayerLocking: false,
                                ignoreLayerVisibility: false);
                            return guard(count == item.Ids.Count, op.InvalidResult()).ToFin();
                        })).As().Map(static _ => unit));
            }
            return exported.Bind(receipt => restored.Map(_ => receipt));
        }));

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

    private static (Option<FileEndpoint> Target, Option<FileFormat> Format, Seq<FileIssue> Issues) PublishMeta(FilePublishTarget target, Seq<FileView> views) =>
        target switch {
            FilePublishTarget.Pdf value => (
                Target: Some(value.Target),
                Format: Some(FileFormat.KnownFormat(key: "pdf", op: Op.Of(name: nameof(PublishMeta))).ThrowIfFail()),
                Issues: Seq(FileIssue.Native(message: "RhinoCommon FilePdf exposes no public metadata, bookmark, outline, encryption, or password surface."))),
            FilePublishTarget.Raster value => (
                Target: Some(value.Target),
                Format: Some(value.ResolvedEncoding.Format),
                Issues: (value.Settings.Transparent && !value.ResolvedEncoding.SupportsAlpha
                    ? Seq(FileIssue.Native(message: "Transparent raster output requires PNG or TIFF encoding."))
                    : Seq<FileIssue>())
                    + (value.Settings.Transparent && views.Exists(static view =>
                        view.Recipe.Decor.Bind(static decor => decor.Layout).Bind(static layout => layout.Area).Case
                            is CaptureArea.Extents or CaptureArea.ScreenWindow or CaptureArea.WorldWindow)
                        ? Seq(
                            FileIssue.Native(message: "Transparent raster capture uses the ViewCapture instance path; the ViewArea Extents/Window mapping is silently ignored and the visible view extent is captured instead."),
                            FileIssue.Of(code: FileIssueCode.Native, message: "Set CaptureArea to Viewport or leave it unset when Transparent = true."))
                        : Seq<FileIssue>())),
            FilePublishTarget.Svg value => (
                Target: Some(value.Target),
                Format: Some(FileFormat.KnownFormat(key: "svg", op: Op.Of(name: nameof(PublishMeta))).ThrowIfFail()),
                Issues: Seq<FileIssue>()),
            _ => (Target: Option<FileEndpoint>.None, Format: Option<FileFormat>.None, Issues: Seq<FileIssue>()),
        };

    private static Fin<T> InSnapshot<T>(RhinoDoc document, string snapshotName, Op op, Func<Fin<T>> body) =>
        from target in FileEndpoint.NonBlank(value: snapshotName, op: op)
        from _exists in guard(toSeq(document.Snapshots.Names).Exists(name => string.Equals(a: name, b: target, comparisonType: StringComparison.Ordinal)), op.InvalidInput())
        let sentinel = $"__rasm_publish_{Guid.NewGuid():N}"
        from _saved in SnapshotScript(serial: document.RuntimeSerialNumber, verb: "_Save", name: sentinel, op: op)
        from result in op.Catch(() => {
            // BOUNDARY ADAPTER — sentinel snapshot saves live state, restores target, then the finally restores+deletes the sentinel via RhinoApp.RunScript (no managed snapshot mutation API).
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
        op.Catch(() => op.Confirm(success: RhinoApp.RunScript(documentSerialNumber: serial, script: $"-_Snapshot {verb} _Name \"{name.Replace(oldValue: "\"", newValue: "\\\"", comparisonType: StringComparison.Ordinal)}\" _Enter", echo: false)));

    private static Fin<FileReport> Commit<T>(FileRuntime runtime, FilePhase phase, string name, T change, Func<RhinoDoc, T, Op, Fin<DocumentReceipt>> apply) where T : class =>
        from live in Live(runtime: runtime, op: Op.Of(name: name))
        from active in Optional(change).ToFin(Fail: Op.Of(name: name).InvalidInput())
        from receipt in live.Edit.Commit(name: name, redraw: DocumentRedraw.After, undoRecorded: true, run: (document, _, op) => apply(arg1: document, arg2: active, arg3: op))
        select FileReport.Of(phase: phase, receipt: Some(receipt));
}
