using System.Numerics;
using Rasm.Rhino.Commands;
using DrawingBitmap = System.Drawing.Bitmap;
using EmbeddedFile = Rhino.FileIO.File3dmEmbeddedFile;
using File3dmModel = Rhino.FileIO.File3dm;
using File3dmObject = Rhino.FileIO.File3dmObject;
using File3dmSettings = Rhino.FileIO.File3dmSettings;
using File3dmStrings = Rhino.FileIO.File3dmStringTable;
using File3dmViewTable = Rhino.FileIO.File3dmViewTable;
using IOPath = System.IO.Path;
using RenderContent = Rhino.FileIO.File3dmRenderContent;
using RenderEnvironment = Rhino.FileIO.File3dmRenderEnvironment;
using RenderMaterial = Rhino.FileIO.File3dmRenderMaterial;
using RenderTexture = Rhino.FileIO.File3dmRenderTexture;

namespace Rasm.Rhino.Exchange;

// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context")]
public abstract partial record FileArchiveSource {
    private FileArchiveSource() { }
    public sealed record Path(FileEndpoint Value) : FileArchiveSource;
    public sealed record Bytes(ReadOnlyMemory<byte> Value) : FileArchiveSource;
}

[SmartEnum<string>]
public sealed partial class FileResourceRole {
    public static readonly FileResourceRole Layer = new(key: "layer");
    public static readonly FileResourceRole Material = new(key: "material");
    public static readonly FileResourceRole Linetype = new(key: "linetype");
    public static readonly FileResourceRole Group = new(key: "group");
    public static readonly FileResourceRole Block = new(key: "block");
    public static readonly FileResourceRole Instance = new(key: "instance");
    public static readonly FileResourceRole Member = new(key: "member");
    public static readonly FileResourceRole Linked = new(key: "linked");
    public static readonly FileResourceRole Texture = new(key: "texture");
    public static readonly FileResourceRole Child = new(key: "child");
}

[Union(SwitchMapStateParameterName = "ctx")]
public abstract partial record FileArchiveQuery {
    private FileArchiveQuery() { }
    public sealed record Layers : FileArchiveQuery;
    public sealed record LayoutPages : FileArchiveQuery;
    public sealed record ArchiveMetadata : FileArchiveQuery;
    public sealed record Strings : FileArchiveQuery;
}

[Union]
public abstract partial record FileQueryResult {
    private FileQueryResult() { }
    public sealed record EntryResult(DocumentResourceKind Kind, Seq<FileResourceEntry> Entries) : FileQueryResult;
    public sealed record MetadataResult(FileArchiveMetadata Metadata) : FileQueryResult;
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record FileArchive(FileArchiveSource Source, FileArchiveMetadata Metadata, FileResourceGraph Resources, Seq<FileObjectManifest> Objects);

public readonly record struct FileArchiveMetadata(
    int ArchiveVersion,
    Option<string> Notes,
    Option<string> ApplicationName,
    Option<string> ApplicationUrl,
    Option<string> ApplicationDetails,
    Option<int> Revision,
    Option<string> CreatedBy,
    Option<string> LastEditedBy,
    Option<DateTime> CreatedOn,
    Option<DateTime> LastEditedOn,
    Option<int> PageViews,
    bool Preview,
    Option<UnitSystem> ModelUnits = default,
    Option<UnitSystem> PageUnits = default,
    Option<double> ModelAbsoluteTolerance = default,
    Option<double> ModelAngleToleranceRadians = default,
    Option<string> ModelUrl = default,
    Option<Point3d> ModelBasePoint = default,
    Option<FileGeoLocation> EarthAnchor = default,
    Option<string> StartComments = default);

public readonly partial record struct FileGeoLocation(
    Option<double> Latitude,
    Option<double> Longitude,
    Option<double> Elevation,
    EarthCoordinateSystem ElevationCoordinateSystem,
    Option<Point3d> ModelBasePoint,
    Option<Vector3d> ModelNorth,
    Option<Vector3d> ModelEast,
    Option<string> Name,
    Option<string> Description,
    Option<double> KmlHeadingDegrees,
    Option<double> KmlTiltDegrees,
    Option<double> KmlRollDegrees,
    bool EarthLocationIsSet = false,
    bool ModelLocationIsSet = false) {
    public static Option<FileGeoLocation> From(EarthAnchorPoint? anchor) =>
        Optional(anchor).Bind(active => (active.EarthLocationIsSet(), active.ModelLocationIsSet()) switch {
            (false, false) => Option<FileGeoLocation>.None,
            (bool earth, bool model) => Some(new FileGeoLocation(
                Latitude: earth ? Some(active.EarthBasepointLatitude) : Option<double>.None,
                Longitude: earth ? Some(active.EarthBasepointLongitude) : Option<double>.None,
                Elevation: earth ? Some(active.EarthBasepointElevation) : Option<double>.None,
                ElevationCoordinateSystem: active.EarthBasepointElevationCoordinateSystem,
                ModelBasePoint: model ? Some(active.ModelBasePoint) : Option<Point3d>.None,
                ModelNorth: model ? Some(active.ModelNorth) : Option<Vector3d>.None,
                ModelEast: model ? Some(active.ModelEast) : Option<Vector3d>.None,
                Name: FileArchiveOps.TextOption(value: active.Name),
                Description: FileArchiveOps.TextOption(value: active.Description),
                KmlHeadingDegrees: model ? Some(active.KMLOrientationHeadingAngleDegrees) : Option<double>.None,
                KmlTiltDegrees: model ? Some(active.KMLOrientationTiltAngleDegrees) : Option<double>.None,
                KmlRollDegrees: model ? Some(active.KMLOrientationRollAngleDegrees) : Option<double>.None,
                EarthLocationIsSet: earth,
                ModelLocationIsSet: model)),
        });

}
public readonly record struct FileResourceEdge(
    DocumentResourceKind FromKind,
    Option<Guid> FromId,
    DocumentResourceKind ToKind,
    Option<Guid> ToId,
    FileResourceRole Role,
    Option<string> Path = default);

public readonly record struct FileResourceEntry(
    DocumentResourceKind Kind,
    Option<string> Name,
    Option<string> Path,
    Option<Guid> Id,
    int Count = 1,
    Option<string> Value = default,
    Option<Guid> TypeId = default,
    Option<Guid> PlugInId = default,
    Option<Guid> RenderEngineId = default,
    Option<Guid> GroupId = default,
    Option<string> Source = default);

public readonly record struct FileResourceGraph(
    int Objects,
    int Layers,
    int Materials,
    int Groups,
    int Blocks,
    int ModelViews,
    int LayoutViews,
    int NamedViews,
    int Strings,
    int PlugInData,
    int EmbeddedFiles,
    int RenderMaterials,
    int RenderEnvironments,
    int RenderTextures,
    int Linetypes,
    int DimensionStyles,
    int HatchPatterns,
    int NamedConstructionPlanes,
    int Manifest,
    int Relations,
    Seq<string> EmbeddedFileNames,
    Seq<string> LinkedBlockArchives,
    Seq<string> RenderTextureFiles,
    Seq<string> FileReferences,
    Seq<FileResourceEntry> Entries,
    Seq<FileResourceEdge> Edges) {
    public Fin<Stat> Summary(Op op) =>
        Seq(Objects, Layers, Materials, Groups, Blocks, ModelViews, LayoutViews, NamedViews, Strings, PlugInData, EmbeddedFiles, RenderMaterials, RenderEnvironments, RenderTextures, Linetypes, DimensionStyles, HatchPatterns, NamedConstructionPlanes, Manifest, Relations)
            .TraverseM(count => count >= 0 ? Fin.Succ(value: (double)count) : Fin.Fail<double>(error: op.InvalidResult()))
            .As()
            .Bind(values => Stat.Of(values: values, key: op));

    public Fin<Seq<FileIssue>> Validate(FileArchiveSource source, IoScheduler scheduler) {
        Op op = Op.Of(name: nameof(Validate));
        Option<string> folder = source switch {
            FileArchiveSource.Path path => Optional(IOPath.GetDirectoryName(path: path.Value.Path)),
            _ => Option<string>.None,
        };
        (string Raw, string Path)[] paths = [.. (LinkedBlockArchives + RenderTextureFiles + FileReferences)
            .Filter(static path => !string.IsNullOrWhiteSpace(value: path))
            .Map(path => (Raw: path, Path: IOPath.IsPathRooted(path: path)
                ? path
                : folder.Map(root => IOPath.GetFullPath(path: IOPath.Combine(path1: root, path2: path))).IfNone(path)))
            .AsEnumerable()
            .DistinctBy(item => item.Path, StringComparer.OrdinalIgnoreCase)];
        return from _ in Optional(source).ToFin(Fail: op.InvalidInput())
               from issues in paths.Length == 0
                   ? Fin.Succ(Seq<FileIssue>())
                   : Optional(scheduler).ToFin(Fail: op.InvalidInput()).Bind(valid => valid.Filter(
                       source: paths,
                       predicate: static item => !File.Exists(path: item.Path),
                       map: static item => FileIssue.Of(code: FileIssueCode.BrokenLink, message: $"missing linked resource: {item.Raw} -> {item.Path}")))
               select issues;
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public readonly record struct FileArchiveDiff(
    Seq<FileResourceEntry> Added,
    Seq<FileResourceEntry> Removed,
    Seq<FileResourceEntry> Changed) {
    // structural delta of two archives' resource graphs; changed = same key, different entry value.
    internal static FileArchiveDiff Of(Seq<FileResourceEntry> before, Seq<FileResourceEntry> after) {
        // id-first so duplicate kind+name resources with distinct ids stay distinct slots; kind:name only when no id exists.
        static string Key(FileResourceEntry entry) => entry.Id.Map(static id => id.ToString()).IfNone($"{entry.Kind}:{entry.Name.IfNone(string.Empty)}");
        // why: Source (free-text provenance tag) and Count (manifest volume) are non-structural drift, excluded so a re-read-path or count-delta yields no spurious Changed entry.
        static bool StructurallyEqual(FileResourceEntry a, FileResourceEntry b) =>
            a.Kind == b.Kind && a.Name == b.Name && a.Path == b.Path && a.Id == b.Id
            && a.Value == b.Value && a.TypeId == b.TypeId && a.PlugInId == b.PlugInId
            && a.RenderEngineId == b.RenderEngineId && a.GroupId == b.GroupId;
        HashMap<string, FileResourceEntry> priors = before.Fold(HashMap<string, FileResourceEntry>(), (acc, entry) => acc.AddOrUpdate(key: Key(entry: entry), value: entry));
        HashMap<string, FileResourceEntry> nexts = after.Fold(HashMap<string, FileResourceEntry>(), (acc, entry) => acc.AddOrUpdate(key: Key(entry: entry), value: entry));
        return new(
            Added: after.Filter(entry => !priors.ContainsKey(key: Key(entry: entry))),
            Removed: before.Filter(entry => !nexts.ContainsKey(key: Key(entry: entry))),
            Changed: after.Filter(entry => priors.Find(key: Key(entry: entry)).Map(prior => !StructurallyEqual(a: prior, b: entry)).IfNone(noneValue: false)));
    }
}

internal static class FileArchiveOps {
    internal static Fin<FileReport> Read(FileArchiveSource source, ArchiveProfile profile) =>
        from archive in UseArchive(source: source, profile: profile, op: Op.Of(name: nameof(Read)), use: (endpoint, model, log) => Snapshot(source: source, model: model, profile: profile).Map(result => (Endpoint: endpoint, result.Archive, result.Issues, Log: log)))
        select FileReport.Of(
            phase: FilePhase.ArchiveRead,
            source: archive.Endpoint,
            target: Option<FileEndpoint>.None,
            format: FormatOf(source: archive.Endpoint),
            issues: archive.Issues + IssueLog(log: archive.Log),
            nativeLog: LogOption(log: archive.Log),
            archive: Some(archive.Archive));

    internal static Fin<FileReport> Extract(FileArchiveSource source, FileEndpoint target, ArchiveProfile profile) =>
        from folder in target.Folder(op: Op.Of(name: nameof(Extract)))
        let full = profile with { Slice = ArchiveSlice.Full }
        from extracted in UseArchive(source: source, profile: full, op: Op.Of(name: nameof(Extract)), use: (endpoint, model, log) =>
                Rows(() => model.EmbeddedFiles)
                .Filter(file => profile.Includes(file: file.Filename))
                .TraverseM(file => ExtractFile(file: file, folder: folder, op: Op.Of(name: nameof(Extract))))
                .As()
                .Map(paths => (Endpoint: endpoint, Paths: paths, Log: log)))
        select FileReport.Of(
            phase: FilePhase.ArchiveExtract,
            source: extracted.Endpoint,
            target: Some(folder),
            format: FormatOf(source: extracted.Endpoint),
            issues: extracted.Paths.IsEmpty ? Seq(FileIssue.Of(code: FileIssueCode.EmptyArchive, message: "archive contains no embedded files")) : IssueLog(log: extracted.Log),
            nativeLog: LogOption(log: extracted.Log),
            receipt: Some(DocumentReceipt.Resources(changes: extracted.Paths.Map(static endpoint => DocumentResourceKind.EmbeddedFile.Change(name: endpoint.Path)))));

    internal static Fin<FileReport> Update(FileArchiveSource source, FileEndpoint target, ArchiveUpdate update, ArchiveProfile profile) =>
        from fmt3dm in FileFormat.KnownFormat(key: "3dm", op: Op.Of(name: nameof(Update)))
        from output in target.WithFormat(format: fmt3dm).Output(op: Op.Of(name: nameof(Update)))
        let op = Op.Of(name: nameof(Update))
        from result in UseArchive(source: source, profile: profile with { Slice = ArchiveSlice.Full }, op: op, use: (endpoint, model, readLog) =>
            from metadataChange in update.Metadata.Case switch {
                FileArchiveMetadataPatch patch => op.Catch(() => {
                    _ = patch.Notes.Map(value => { model.Notes.Notes = value; return unit; });
                    _ = patch.ApplicationName.Map(value => model.ApplicationName = value);
                    _ = patch.ApplicationUrl.Map(value => model.ApplicationUrl = value);
                    _ = patch.ApplicationDetails.Map(value => model.ApplicationDetails = value);
                    _ = patch.StartComments.Map(value => model.StartSectionComments = value);
                    _ = patch.UserStrings.Fold(unit, (_, entry) => (entry.Section.Case, entry.Value.Case) switch {
                        (string section, string value) => Op.Side(() => model.Strings.SetString(section: section, entry: entry.Key, value: value)),
                        (string section, _) => Op.Side(() => model.Strings.Delete(section: section, entry: entry.Key)),
                        (_, string value) => Op.Side(() => model.Strings.SetString(key: entry.Key, value: value)),
                        _ => Op.Side(() => model.Strings.Delete(key: entry.Key)),
                    });
                    return Fin.Succ(value: Seq(DocumentResourceKind.Metadata.Change(name: "metadata"))
                        + patch.UserStrings.Map(static entry => DocumentResourceKind.Text.Change(name: entry.Key)));
                }),
                _ => Fin.Succ(value: Seq<DocumentResourceChange>()),
            }
            from namedViewChange in update.NamedViews.TraverseM(patch => ApplyNamedViewPatch(table: model.AllNamedViews, patch: patch, op: op)).As().Map(static changes => changes.Somes())
            from settingsChange in update.Settings.Case switch {
                FileArchiveSettingsPatch patch => ApplySettingsPatch(settings: model.Settings, patch: patch, op: op),
                _ => Fin.Succ(value: Seq<DocumentResourceChange>()),
            }
            from previewChange in update.PreviewImage.Case switch {
                FileEndpoint endpoint => ApplyPreviewImage(model: model, endpoint: endpoint, op: op),
                _ => Fin.Succ(value: Seq<DocumentResourceChange>()),
            }
            from embedded in update.Embed.TraverseM(endpoint =>
                endpoint.Input(op: op).Bind(payload => model.EmbeddedFiles.Add(filename: payload.Path) switch {
                    true => Fin.Succ(DocumentResourceKind.EmbeddedFile.Change(name: payload.Path)),
                    false => Fin.Fail<DocumentResourceChange>(error: op.InvalidResult()),
                })).As()
            from writeLog in op.Catch(() => model.WriteWithLog(path: output.Path, options: FileFormat.ArchiveWriteOptions(profile: profile), errorLog: out string log) switch {
                true => Fin.Succ(value: log),
                false => Fin.Fail<string>(error: op.InvalidResult()),
            })
            from extractFolder in update.Extract.IsEmpty
                ? Fin.Succ(value: output.WithPath(path: IOPath.GetDirectoryName(path: output.Path) ?? output.Path))
                : output.WithPath(path: IOPath.GetDirectoryName(path: output.Path) ?? output.Path).Folder(op: op)
            from extracted in Rows(() => model.EmbeddedFiles)
                .Filter(file => (ArchiveProfile.Full with { Embedded = update.Extract }).Includes(file: file.Filename))
                .TraverseM(file => ExtractFile(file: file, folder: extractFolder, op: op).Map(static endpoint => endpoint.Path)).As()
            from snapshot in Snapshot(source: new FileArchiveSource.Path(output), model: model, profile: profile)
            select (Endpoint: endpoint, ReadLog: readLog, WriteLog: writeLog, snapshot.Archive, SnapshotIssues: snapshot.Issues,
                Receipt: DocumentReceipt.Resources(changes: metadataChange + namedViewChange + settingsChange + previewChange + embedded + extracted.Map(static path => DocumentResourceKind.EmbeddedFile.Change(name: path)))))
        select FileReport.Of(
            phase: FilePhase.ArchiveUpdate,
            source: result.Endpoint,
            target: Some(output),
            format: output.Format,
            issues: result.SnapshotIssues + IssueLog(log: result.ReadLog) + IssueLog(log: result.WriteLog),
            nativeLog: LogOption(log: string.Join(separator: System.Environment.NewLine, values: Seq(result.ReadLog, result.WriteLog).Filter(static value => !string.IsNullOrWhiteSpace(value: value)).AsIterable())),
            archive: Some(result.Archive),
            receipt: Some(result.Receipt));

    private static Fin<Option<DocumentResourceChange>> ApplyNamedViewPatch(File3dmViewTable table, FileNamedViewPatch patch, Op op) =>
        from name in FileEndpoint.NonBlank(value: patch.Name, op: op)
        from index in NamedViewIndex(table: table, name: name).ToFin(Fail: op.MissingContext())
        from change in (patch.Delete, patch.Rename.Case) switch {
            (true, _) => op.Confirm(success: table.Delete(index: index))
                .Map(_ => Some(DocumentResourceKind.NamedView.Change(name: name))),
            (false, string next) => op.Catch(() => {
                // ViewInfo edits do not persist through the live table reference; reinsert the renamed copy.
                ViewInfo view = table[index];
                view.Name = next;
                return op.Confirm(success: table.Delete(index: index))
                    .Map(_ => Op.Side(() => table.Add(item: view)))
                    .Map(_ => Some(DocumentResourceKind.NamedView.Change(name: next)));
            }),
            _ => Fin.Succ(value: Option<DocumentResourceChange>.None),
        }
        select change;

    private static Option<int> NamedViewIndex(File3dmViewTable table, string name) =>
        toSeq(Enumerable.Range(start: 0, count: Native(read: () => table.Count).IfNone(0)))
            .Find(index => string.Equals(a: table[index].Name, b: name, comparisonType: StringComparison.OrdinalIgnoreCase));

    private static Fin<Seq<DocumentResourceChange>> ApplySettingsPatch(File3dmSettings settings, FileArchiveSettingsPatch patch, Op op) =>
        op.Catch(() => {
            _ = patch.ModelUnitSystem.Iter(value => settings.ModelUnitSystem = value);
            _ = patch.PageUnitSystem.Iter(value => settings.PageUnitSystem = value);
            _ = patch.ModelAbsoluteTolerance.Iter(value => settings.ModelAbsoluteTolerance = value);
            _ = patch.ModelAngleToleranceRadians.Iter(value => settings.ModelAngleToleranceRadians = value);
            _ = patch.ModelUrl.Iter(value => settings.ModelUrl = value);
            return Fin.Succ(value: Seq(DocumentResourceKind.Metadata.Change(name: "settings")));
        });

    private static Fin<Seq<DocumentResourceChange>> ApplyPreviewImage(File3dmModel model, FileEndpoint endpoint, Op op) =>
        from path in endpoint.Input(op: op)
        from change in op.Catch(() => {
            using DrawingBitmap bitmap = new(filename: path.Path);
            model.SetPreviewImage(image: bitmap);
            return Fin.Succ(value: Seq(DocumentResourceKind.Metadata.Change(name: "preview")));
        })
        select change;

    internal static Fin<byte[]> Bytes(FileArchiveSource source, ArchiveProfile profile) =>
        UseArchive(source: source, profile: profile, op: Op.Of(name: nameof(Bytes)), use: (_, model, _) =>
            Optional(model.ToByteArray(options: FileFormat.ArchiveWriteOptions(profile: profile)))
                .Filter(static value => value.Length > 0)
                .ToFin(Fail: Op.Of(name: nameof(Bytes)).InvalidResult()));

    internal static Fin<FileReport> Validate(FileArchiveSource source, ArchiveProfile profile, IoScheduler scheduler) =>
        from result in UseArchive(source: source, profile: profile with { Slice = ArchiveSlice.Resources, Projection = FileArchiveProjection.Graph }, op: Op.Of(name: nameof(Validate)), use: (endpoint, model, log) =>
            from snapshot in Snapshot(source: source, model: model, profile: profile with { Slice = ArchiveSlice.Resources, Projection = FileArchiveProjection.Graph })
            from resourceIssues in snapshot.Archive.Resources.Validate(source: snapshot.Archive.Source, scheduler: scheduler)
            select (Endpoint: endpoint, snapshot.Archive, Issues: snapshot.Issues + resourceIssues, Log: log))
        select FileReport.Of(
            phase: FilePhase.ArchiveValidate,
            source: result.Endpoint,
            target: Option<FileEndpoint>.None,
            format: FormatOf(source: result.Endpoint),
            issues: result.Issues + IssueLog(log: result.Log),
            nativeLog: LogOption(log: result.Log),
            archive: Some(result.Archive));

    internal static Fin<FileArchiveMetadata> Inspect(FileEndpoint source) =>
        from path in source.Input(op: Op.Of(name: nameof(Inspect)))
        from metadata in UseArchive(
            source: new FileArchiveSource.Path(Value: path),
            profile: ArchiveProfile.Full with { Projection = FileArchiveProjection.Metadata },
            op: Op.Of(name: nameof(Inspect)),
            use: (_, model, _) => Metadata(source: new FileArchiveSource.Path(Value: path), model: model, layouts: ReadLayouts(source: new FileArchiveSource.Path(Value: path))))
        select metadata;

    internal static Fin<FileArchiveDiff> Diff(FileEndpoint source, FileEndpoint other) =>
        from before in GraphOf(endpoint: source, op: Op.Of(name: nameof(Diff)))
        from after in GraphOf(endpoint: other, op: Op.Of(name: nameof(Diff)))
        select FileArchiveDiff.Of(before: before.Entries, after: after.Entries);

    private static Fin<FileResourceGraph> GraphOf(FileEndpoint endpoint, Op op) =>
        from path in endpoint.Input(op: op)
        from graph in UseArchive(
            source: new FileArchiveSource.Path(Value: path),
            profile: ArchiveProfile.Full with { Slice = ArchiveSlice.Resources, Projection = FileArchiveProjection.Graph },
            op: op,
            use: (_, model, _) => Snapshot(
                source: new FileArchiveSource.Path(Value: path),
                model: model,
                profile: ArchiveProfile.Full with { Slice = ArchiveSlice.Resources, Projection = FileArchiveProjection.Graph }).Map(static result => result.Archive.Resources))
        select graph;

    internal static Fin<FileQueryResult> Query(FileArchiveSource source, FileArchiveQuery query, Op op) =>
        query.Switch(
            (Source: source, Op: op),
            layers: static (ctx, _) => EntryQuery(source: ctx.Source, slice: ArchiveSlice.LayerTable, kind: DocumentResourceKind.Layer, op: ctx.Op, project: static model =>
                Rows(() => model.AllLayers).Map(layer => new FileResourceEntry(
                    Kind: DocumentResourceKind.Layer,
                    Name: TextOption(value: layer.FullPath),
                    Path: Option<string>.None,
                    Id: GuidOption(value: layer.Id),
                    Source: Some("layer")))),
            layoutPages: static (ctx, _) => ctx.Source is FileArchiveSource.Path
                ? Fin.Succ<FileQueryResult>(value: new FileQueryResult.EntryResult(Kind: DocumentResourceKind.Layout, Entries: LayoutEntries(layouts: ReadLayouts(source: ctx.Source))))
                : Fin.Fail<FileQueryResult>(error: ctx.Op.InvalidInput()),
            archiveMetadata: static (ctx, _) => ctx.Source switch {
                FileArchiveSource.Path path => ArchiveMetadataOf(path: path.Value.Path, op: ctx.Op).Map(static metadata => (FileQueryResult)new FileQueryResult.MetadataResult(Metadata: metadata)),
                _ => Fin.Fail<FileQueryResult>(error: ctx.Op.InvalidInput()),
            },
            strings: static (ctx, _) => EntryQuery(source: ctx.Source, slice: ArchiveSlice.StringTable, kind: DocumentResourceKind.Text, op: ctx.Op, project: static model =>
                Native(read: () => model.Strings).Map(table =>
                    Rows(() => table.GetSectionNames()).Bind(section =>
                        Rows(() => table.GetEntryNames(section: section)).Map(entry => new FileResourceEntry(
                            Kind: DocumentResourceKind.Text,
                            Name: TextOption(value: entry),
                            Path: TextOption(value: section),
                            Id: Option<Guid>.None,
                            Value: TextOption(value: table.GetValue(section: section, entry: entry)),
                            Source: Some("section"))))).IfNone(Seq<FileResourceEntry>())));

    private static Fin<FileQueryResult> EntryQuery(FileArchiveSource source, ArchiveSlice slice, DocumentResourceKind kind, Op op, Func<File3dmModel, Seq<FileResourceEntry>> project) =>
        UseArchive(
            source: source,
            profile: new ArchiveProfile(Slice: slice, Projection: FileArchiveProjection.Graph, Write: FileWritePolicy.Default),
            op: op,
            use: (_, model, _) => Fin.Succ<FileQueryResult>(value: new FileQueryResult.EntryResult(Kind: kind, Entries: project(arg: model))));

    private static Fin<FileArchiveMetadata> ArchiveMetadataOf(string path, Op op) =>
        op.Catch(() => {
            // Partial-read statics open the archive header only — no model table load, O(1) vs full parse.
            bool revisioned = File3dmModel.ReadRevisionHistory(path: path, createdBy: out string createdBy, lastEditedBy: out string lastEditedBy, revision: out int revision, createdOn: out DateTime createdOn, lastEditedOn: out DateTime lastEditedOn);
            File3dmModel.ReadApplicationData(path: path, applicationName: out string applicationName, applicationUrl: out string applicationUrl, applicationDetails: out string applicationDetails);
            return Fin.Succ(value: new FileArchiveMetadata(
                ArchiveVersion: File3dmModel.ReadArchiveVersion(path: path),
                Notes: TextOption(value: File3dmModel.ReadNotes(path: path)),
                ApplicationName: TextOption(value: applicationName),
                ApplicationUrl: TextOption(value: applicationUrl),
                ApplicationDetails: TextOption(value: applicationDetails),
                Revision: revisioned && revision > 0 ? Some(revision) : Option<int>.None,
                CreatedBy: TextOption(value: createdBy),
                LastEditedBy: TextOption(value: lastEditedBy),
                CreatedOn: DateTimeOption(value: createdOn),
                LastEditedOn: DateTimeOption(value: lastEditedOn),
                PageViews: Option<int>.None,
                Preview: false));
        });

    internal static Fin<T> UseArchive<T>(FileArchiveSource source, ArchiveProfile profile, Op op, Func<Option<FileEndpoint>, File3dmModel, string, Fin<T>> use) =>
        from active in Optional(source).ToFin(Fail: op.InvalidInput())
        from valid in Optional(use).ToFin(Fail: op.InvalidInput())
        from result in active.Switch(
            (Profile: profile, Op: op, Use: valid),
            path: static (ctx, source) =>
                from endpoint in source.Value.Input(op: ctx.Op)
                from value in ctx.Op.Catch(() => {
                    ArchiveSlice slice = ctx.Profile.Slice;
                    (File3dmModel? model, string log) = slice.Filtered
                        ? (File3dmModel.ReadWithLog(path: endpoint.Path, tableTypeFilterFilter: slice.Tables, objectTypeFilter: slice.ObjectFilter, errorLog: out string filteredLog), filteredLog)
                        : (File3dmModel.ReadWithLog(endpoint.Path, out string fullLog), fullLog);
                    using File3dmModel? owned = model;
                    return Optional(owned).ToFin(Fail: ctx.Op.InvalidResult(detail: $"File3dm read failed: '{endpoint.Path}'{(string.IsNullOrEmpty(value: log) ? string.Empty : $" — {log}")}")).Bind(active => ctx.Use(arg1: Some(endpoint), arg2: active, arg3: log));
                })
                select value,
            bytes: static (ctx, source) =>
                from memory in guard(!source.Value.IsEmpty, ctx.Op.InvalidInput()).ToFin().Map(_ => source.Value)
                from value in ctx.Op.Catch(() => {
                    using File3dmModel? model = File3dmModel.FromByteArray(bytes: memory.ToArray());
                    return Optional(model).ToFin(Fail: ctx.Op.InvalidResult()).Bind(active => ctx.Use(arg1: Option<FileEndpoint>.None, arg2: active, arg3: string.Empty));
                })
                select value)
        select result;

    private static Fin<(FileArchive Archive, Seq<FileIssue> Issues)> Snapshot(FileArchiveSource source, File3dmModel model, ArchiveProfile profile) =>
        profile.Projection switch {
            FileArchiveProjection.None => Fin.Fail<(FileArchive Archive, Seq<FileIssue> Issues)>(error: Op.Of(name: nameof(Snapshot)).InvalidInput()),
            FileArchiveProjection projection =>
                from layouts in Fin.Succ(value: (projection & (FileArchiveProjection.Metadata | FileArchiveProjection.Graph)) != FileArchiveProjection.None
                    ? ReadLayouts(source: source)
                    : Seq<(string Name, Guid Id)>())
                from metadata in (projection & FileArchiveProjection.Metadata) != FileArchiveProjection.None
                    ? Metadata(source: source, model: model, layouts: layouts)
                    : Fin.Succ(value: default(FileArchiveMetadata))
                from resources in (projection & FileArchiveProjection.Graph) != FileArchiveProjection.None
                    ? Resources(model: model, source: source, layouts: layouts)
                    : Fin.Succ(value: (Graph: default(FileResourceGraph), Issues: Seq<FileIssue>()))
                select (
                    Archive: new FileArchive(
                        Source: source,
                        Metadata: metadata,
                        Resources: resources.Graph,
                        Objects: (projection & FileArchiveProjection.Objects) != FileArchiveProjection.None
                            ? Objects(
                                model: model,
                                objects: Rows(() => model.Objects),
                                layers: Rows(() => model.AllLayers),
                                materials: Rows(() => model.AllMaterials)).Strict()
                            : Seq<FileObjectManifest>()),
                    resources.Issues),
        };

    private static Option<FileFormat> FormatOf(Option<FileEndpoint> source) =>
        source.Bind(static endpoint => endpoint.Format).Case switch {
            FileFormat format => Some(format),
            // "3dm" is a guaranteed builtin key; the fail rail is unreachable but keeps FormatOf total.
            _ => FileFormat.KnownFormat(key: "3dm", op: Op.Of(name: nameof(FormatOf))).ToOption(),
        };
    private static Fin<(FileResourceGraph Graph, Seq<FileIssue> Issues)> Resources(File3dmModel model, FileArchiveSource source, Seq<(string Name, Guid Id)> layouts) {
        Op key = Op.Of(name: nameof(Resources));
        Option<string> archivePath = source switch {
            FileArchiveSource.Path path => Some(path.Value.Path),
            _ => Option<string>.None,
        };
        return (from graph in archivePath
                .Map(path => Blocks.Archive.From(model: model, archivePath: Some(path), key: key))
                .IfNone(() => Blocks.Archive.From(model: model, key: key))
                from closure in archivePath.Case switch {
                    string path => Blocks.Archive.ValidateArchiveClosure(root: model, rootPath: path, key: key)
                        .Map(report => (
                            Linked: report.Edges.Map(static edge => edge.Link.Full.Value).Distinct(),
                            Issues: report.Valid
                                ? Seq<FileIssue>()
                                : report.Broken.Map(static path => FileIssue.Of(code: FileIssueCode.BrokenLink, message: $"broken linked archive: {path.Value}"))
                                  + report.Cycles.Map(static cycle => FileIssue.Native(message: $"linked archive cycle: {string.Join(separator: " -> ", values: cycle.Map(static path => path.Value).AsIterable())}"))
                                  + report.Truncated.Map(static path => FileIssue.Native(message: $"linked archive validation truncated: {path.Value}")))),
                    _ => Fin.Succ(value: (
                        Linked: toSeq(graph.LinkedArchives.AsEnumerable()).Map(static link => link.Stored),
                        Issues: Seq<FileIssue>())),
                }
                select (Graph: graph, closure.Linked, closure.Issues))
            .BiBind(
                Succ: resources => key.Catch(() => Fin.Succ(value: (
                    ResourceGraphOf(model: model, blockGraph: resources.Graph, linked: resources.Linked, layouts: layouts),
                    resources.Issues + LayoutIssues(source: source)))),
                Fail: error => key.Catch(() => Fin.Succ(value: (
                    Graph: ResourceGraphOf(model: model, blockGraph: Blocks.Archive.Graph.Empty, linked: Seq<string>(), layouts: layouts),
                    Issues: Seq(FileIssue.Native(message: error.Message)) + LayoutIssues(source: source)))))
            .BindFail(static error => Fin.Succ(value: (Graph: default(FileResourceGraph), Issues: Seq(FileIssue.Native(message: error.Message)))));
    }
    private static FileResourceGraph ResourceGraphOf(File3dmModel model, Blocks.Archive.Graph blockGraph, Seq<string> linked, Seq<(string Name, Guid Id)> layouts) {
        Seq<File3dmObject> objects = Rows(() => model.Objects);
        Seq<Layer> layers = Rows(() => model.AllLayers);
        Seq<Material> materials = Rows(() => model.AllMaterials);
        Seq<Group> groups = Rows(() => model.AllGroups);
        Seq<Linetype> linetypes = Rows(() => model.AllLinetypes);
        Seq<DimensionStyle> dimStyles = Rows(() => model.AllDimStyles);
        Seq<HatchPattern> hatchPatterns = Rows(() => model.AllHatchPatterns);
        Seq<ConstructionPlane> cplanes = Rows(() => model.AllNamedConstructionPlanes);
        Seq<ViewInfo> views = Rows(() => model.Views);
        Seq<ViewInfo> namedViews = Rows(() => model.NamedViews);
        Seq<global::Rhino.FileIO.File3dmPlugInData> plugInData = Rows(() => model.PlugInData);
        Seq<RenderMaterial> renderMaterials = Rows(() => model.RenderMaterials);
        Seq<RenderEnvironment> renderEnvironments = Rows(() => model.RenderEnvironments);
        Seq<RenderTexture> renderTextures = Rows(() => model.RenderTextures);
        Seq<RenderContent> renderTree =
            renderMaterials.Map(static material => (RenderContent)material)
            + renderEnvironments.Map(static environment => (RenderContent)environment)
            + renderTextures.Map(static texture => (RenderContent)texture);
        Seq<EmbeddedFile> embeddedFiles = Rows(() => model.EmbeddedFiles);
        Seq<string> embedded = embeddedFiles.Map(static file => file.Filename);
        Option<File3dmStrings> strings = Native(read: () => model.Strings);
        Seq<string> textures = renderTree.Bind(static content => TraverseRender(content: content, parent: Option<RenderContent>.None, project: static (cur, _) => cur switch {
            RenderTexture texture => TextOption(value: texture.Filename).Map(Seq).IfNone(Seq<string>()),
            _ => Seq<string>(),
        })).Distinct();
        Seq<FileResourceEntry> renderEntries = renderTree.Bind(static content => TraverseRender(content: content, parent: Option<RenderContent>.None, project: static (cur, _) => Seq(new FileResourceEntry(
            Kind: RenderKind(content: cur),
            Name: TextOption(value: cur.TypeName),
            Path: cur switch {
                RenderTexture texture => TextOption(value: texture.Filename),
                _ => Option<string>.None,
            },
            Id: GuidOption(value: cur.Id),
            Value: TextOption(value: cur.Kind),
            TypeId: GuidOption(value: cur.TypeId),
            PlugInId: GuidOption(value: cur.PlugInId),
            RenderEngineId: GuidOption(value: cur.RenderEngineId),
            GroupId: GuidOption(value: cur.GroupId),
            Source: cur.Reference ? Some("reference") : Some("embedded")))));
        Seq<FileResourceEntry> textEntries = strings.Map(table =>
            Rows(() => table.GetSectionNames())
                .Bind(section => Rows(() => table.GetEntryNames(section: section))
                    .Map(entry => new FileResourceEntry(
                        Kind: DocumentResourceKind.Text,
                        Name: TextOption(value: entry),
                        Path: TextOption(value: section),
                        Id: Option<Guid>.None,
                        Value: TextOption(value: table.GetValue(section: section, entry: entry)),
                        Source: Some("section"))))
                .Filter(static entry => entry.Name.Case is string || entry.Value.Case is string)
            + toSeq(Enumerable.Range(start: 0, count: Native(read: () => table.Count).IfNone(0)))
                .Map(index => new FileResourceEntry(Kind: DocumentResourceKind.Text, Name: TextOption(value: table.GetKey(i: index)), Path: Option<string>.None, Id: Option<Guid>.None, Value: TextOption(value: table.GetValue(i: index)), Source: Some("flat")))
                .Filter(static entry => (entry.Name.Case is string name && !name.Contains('\\', StringComparison.Ordinal)) || (entry.Name.Case is not string && entry.Value.Case is string)))
            .IfNone(Seq<FileResourceEntry>());
        Seq<FileResourceEntry> entries =
            embeddedFiles.Map(file => new FileResourceEntry(Kind: DocumentResourceKind.EmbeddedFile, Name: TextOption(value: IOPath.GetFileName(path: file.Filename)), Path: TextOption(value: file.Filename), Id: Option<Guid>.None, Source: TextOption(value: file.ComponentType.ToString())))
            + plugInData.Map(data => new FileResourceEntry(Kind: DocumentResourceKind.Metadata, Name: Option<string>.None, Path: Option<string>.None, Id: GuidOption(value: data.PlugInId), PlugInId: GuidOption(value: data.PlugInId), Source: Some("File3dmPlugInData")))
            + textEntries
            + renderEntries
            + LayoutEntries(layouts: layouts)
            + ManifestEntries(model: model)
            + Entries(source: layers, kind: DocumentResourceKind.Layer, name: static l => TextOption(value: l.FullPath), id: static l => GuidOption(value: l.Id), label: "layer")
            + Entries(source: materials, kind: DocumentResourceKind.Material, name: static m => TextOption(value: m.Name), id: static m => GuidOption(value: m.Id), label: "material")
            + Entries(source: groups, kind: DocumentResourceKind.Group, name: static g => TextOption(value: g.Name), id: static g => GuidOption(value: g.Id), label: "group")
            + Blocks.Archive.ToFileResourceEntries(graph: blockGraph)
            + Entries(source: linetypes, kind: DocumentResourceKind.Linetype, name: static l => TextOption(value: l.Name), id: static l => GuidOption(value: l.Id), label: "linetype")
            + Entries(source: dimStyles, kind: DocumentResourceKind.DimensionStyle, name: static s => TextOption(value: s.Name), id: static s => GuidOption(value: s.Id), label: "dimension-style")
            + Entries(source: hatchPatterns, kind: DocumentResourceKind.Hatch, name: static p => TextOption(value: p.Name), id: static p => GuidOption(value: p.Id), label: "hatch-pattern")
            + Entries(source: cplanes, kind: DocumentResourceKind.ConstructionPlane, name: static p => TextOption(value: p.Name), id: static _ => Option<Guid>.None, label: "named-cplane")
            + Entries(source: views, kind: DocumentResourceKind.View, name: static v => TextOption(value: v.Name), id: static _ => Option<Guid>.None, label: "view", path: static v => TextOption(value: v.WallpaperFilename))
            + Entries(source: namedViews, kind: DocumentResourceKind.NamedView, name: static v => TextOption(value: v.Name), id: static v => GuidOption(value: v.NamedViewId), label: "named-view", path: static v => TextOption(value: v.WallpaperFilename));
        Seq<FileResourceEdge> renderEdges = renderTree.Bind(static content => TraverseRender(content: content, parent: Option<RenderContent>.None, project: static (cur, par) => {
            Seq<FileResourceEdge> parentEdge = par.Map(p => Seq(new FileResourceEdge(FromKind: RenderKind(content: p), FromId: GuidOption(value: p.Id), ToKind: RenderKind(content: cur), ToId: GuidOption(value: cur.Id), Role: FileResourceRole.Child))).IfNone(Seq<FileResourceEdge>());
            Seq<FileResourceEdge> textureEdge = cur is RenderTexture texture
                ? TextOption(value: texture.Filename).Map(path => Seq(new FileResourceEdge(FromKind: RenderKind(content: texture), FromId: GuidOption(value: texture.Id), ToKind: DocumentResourceKind.FileReference, ToId: Option<Guid>.None, Role: FileResourceRole.Texture, Path: Some(path)))).IfNone(Seq<FileResourceEdge>())
                : Seq<FileResourceEdge>();
            return parentEdge + textureEdge;
        }));
        Seq<FileResourceEdge> edges = objects.Bind(fileObject => ObjectEdges(fileObject: fileObject, layers: layers, materials: materials, linetypes: linetypes, groups: groups))
            + Blocks.Archive.ToFileResourceEdges(graph: blockGraph)
            + renderEdges;
        return new FileResourceGraph(
            Objects: ObjectCount(model: model),
            Layers: layers.Count,
            Materials: materials.Count,
            Groups: groups.Count,
            Blocks: blockGraph.Definitions.Length,
            ModelViews: views.Count,
            LayoutViews: layouts.Count,
            NamedViews: namedViews.Count,
            Strings: strings.Map(static table => table.Count).IfNone(0),
            PlugInData: plugInData.Count,
            EmbeddedFiles: embedded.Count,
            RenderMaterials: renderMaterials.Count,
            RenderEnvironments: renderEnvironments.Count,
            RenderTextures: renderTextures.Count,
            Linetypes: linetypes.Count,
            DimensionStyles: dimStyles.Count,
            HatchPatterns: hatchPatterns.Count,
            NamedConstructionPlanes: cplanes.Count,
            Manifest: Native(read: () => model.Manifest).Map(static manifest => manifest.Count).IfNone(0),
            Relations: edges.Count,
            EmbeddedFileNames: embedded.Strict(),
            LinkedBlockArchives: linked.Strict(),
            RenderTextureFiles: textures.Strict(),
            FileReferences: (linked + textures + edges.Bind(static edge => edge.Path.Map(Seq).IfNone(Seq<string>()))).Distinct().Strict(),
            Entries: entries.Strict(),
            Edges: edges.Strict());
    }

    private static Seq<FileResourceEntry> LayoutEntries(Seq<(string Name, Guid Id)> layouts) =>
        layouts.Map(static layout => new FileResourceEntry(
            Kind: DocumentResourceKind.Layout,
            Name: TextOption(value: layout.Name),
            Path: Option<string>.None,
            Id: GuidOption(value: layout.Id),
            Source: Some("ReadPageViews")));

    private static Seq<(string Name, Guid Id)> ReadLayouts(FileArchiveSource source) =>
        source switch {
            FileArchiveSource.Path path => Native(read: () => File3dmModel.ReadPageViews(path: path.Value.Path))
                .Map(views => toSeq(views).Map(static view => {
                    try { return (view.Name, Id: view.NamedViewId); } finally { view.Dispose(); }
                }).Strict())
                .IfNone(Seq<(string, Guid)>()),
            _ => Seq<(string, Guid)>(),
        };

    private static Seq<FileIssue> LayoutIssues(FileArchiveSource source) =>
        source is FileArchiveSource.Path
            ? Seq<FileIssue>()
            : Seq(FileIssue.Native(message: "File3dm.ReadPageViews requires a file path; byte archive layout manifests and page-view counts are unavailable through public RhinoCommon."));

    private static Seq<FileObjectManifest> Objects(File3dmModel model, Seq<File3dmObject> objects, Seq<Layer> layers, Seq<Material> materials) =>
        objects.Map(fileObject => {
            Option<GeometryBase> geometry = Optional(fileObject.Geometry);
            Option<ObjectAttributes> attributes = Optional(fileObject.Attributes);
            return new FileObjectManifest(
                Id: attributes.Map(static a => a.ObjectId).IfNone(Guid.Empty),
                Name: attributes.Bind(static a => TextOption(value: a.Name)),
                Layer: attributes.Bind(a => Component(source: layers, index: a.LayerIndex).Bind(layer => TextOption(value: layer.FullPath))),
                ObjectType: geometry.Map(static g => g.ObjectType).IfNone(ObjectType.None),
                Material: attributes.Bind(a => MaterialOf(layers: layers, materials: materials, attributes: a)).Bind(material => TextOption(value: material.Name)),
                UserStrings: (attributes.Bind(static a => Optional(a.GetUserStrings()?.AllKeys)).Case switch {
                    string[] keys => toSeq(keys).Choose(static value => TextOption(value: value)),
                    _ => Seq<string>(),
                }) + (geometry.Bind(static g => Optional(g.GetUserStrings()?.AllKeys)).Case switch {
                    string[] keys => toSeq(keys).Choose(static value => TextOption(value: value)),
                    _ => Seq<string>(),
                }));
        });

    private static int ObjectCount(File3dmModel model) =>
        Native(read: () => model.Manifest)
            .Map(static manifest => manifest.ActiveObjectCount(type: ModelComponentType.ModelGeometry))
            .IfNone(0);

    private static Seq<FileResourceEntry> ManifestEntries(File3dmModel model) =>
        toSeq(Enum.GetValues<ModelComponentType>())
            .Choose(type => DocumentResourceKind.ForComponentType(type: type).Map(kind => (Kind: kind, Type: type)))
            .Map(row => new FileResourceEntry(Kind: row.Kind, Name: TextOption(value: row.Type.ToString()), Path: Option<string>.None, Id: Option<Guid>.None, Count: Native(read: () => model.Manifest).Map(manifest => manifest.ActiveObjectCount(type: row.Type)).IfNone(0), Source: Some("manifest")))
            .Filter(static entry => entry.Count > 0);

    private static Seq<FileResourceEntry> Entries<T>(
        IEnumerable<T> source,
        DocumentResourceKind kind,
        Func<T, Option<string>> name,
        Func<T, Option<Guid>> id,
        string label,
        Func<T, Option<string>>? path = null) =>
        toSeq(source).Map(item => new FileResourceEntry(
            Kind: kind,
            Name: name(arg: item),
            Path: path is null ? Option<string>.None : path(arg: item),
            Id: id(arg: item),
            Source: Some(label)));

    private static Seq<T> Rows<T>(IEnumerable<T>? source) =>
        source is null ? Seq<T>() : toSeq(source).Filter(static item => item is not null);

    private static Seq<T> Rows<T>(Func<IEnumerable<T>?> read) =>
        Native(read: read).Map(source => Rows(source: source)).IfNone(Seq<T>());

    private static Option<T> Native<T>(Func<T> read) {
        try {
            return Optional(read()).Filter(static value => value is not null);
        } catch (NullReferenceException) {
            return Option<T>.None;
        }
    }

    private static Option<T> Component<T>(Seq<T> source, int index)
        where T : ModelComponent =>
        source.Find(item => item.Index == index);

    private static Seq<FileResourceEdge> ObjectEdges(File3dmObject fileObject, Seq<Layer> layers, Seq<Material> materials, Seq<Linetype> linetypes, Seq<Group> groups) =>
        Optional(fileObject.Attributes).Map(attributes => {
            Option<Guid> objectId = GuidOption(value: attributes.ObjectId);
            Seq<FileResourceEdge> layerEdge = Seq(new FileResourceEdge(
                FromKind: DocumentResourceKind.Object,
                FromId: objectId,
                ToKind: DocumentResourceKind.Layer,
                ToId: Component(source: layers, index: attributes.LayerIndex).Bind(layer => GuidOption(value: layer.Id)),
                Role: FileResourceRole.Layer));
            Seq<FileResourceEdge> materialEdge = MaterialOf(layers: layers, materials: materials, attributes: attributes)
                .Bind(material => GuidOption(value: material.Id))
                .Map(id => Seq(new FileResourceEdge(FromKind: DocumentResourceKind.Object, FromId: objectId, ToKind: DocumentResourceKind.Material, ToId: Some(id), Role: FileResourceRole.Material)))
                .IfNone(Seq<FileResourceEdge>());
            Seq<FileResourceEdge> linetypeEdge = (attributes.LinetypeSource switch {
                ObjectLinetypeSource.LinetypeFromObject => Component(source: linetypes, index: attributes.LinetypeIndex),
                ObjectLinetypeSource.LinetypeFromLayer => Component(source: layers, index: attributes.LayerIndex)
                    .Bind(layer => Component(source: linetypes, index: layer.LinetypeIndex)),
                _ => Option<Linetype>.None,
            })
            .Bind(linetype => GuidOption(value: linetype.Id))
            .Map(id => Seq(new FileResourceEdge(FromKind: DocumentResourceKind.Object, FromId: objectId, ToKind: DocumentResourceKind.Linetype, ToId: Some(id), Role: FileResourceRole.Linetype)))
            .IfNone(Seq<FileResourceEdge>());
            Seq<FileResourceEdge> groupEdges = toSeq(attributes.GetGroupList() ?? [])
                .Choose(index => Component(source: groups, index: index)
                    .Bind(group => GuidOption(value: group.Id))
                    .Map(id => new FileResourceEdge(FromKind: DocumentResourceKind.Object, FromId: objectId, ToKind: DocumentResourceKind.Group, ToId: Some(id), Role: FileResourceRole.Group)));
            return layerEdge + materialEdge + linetypeEdge + groupEdges;
        }).IfNone(Seq<FileResourceEdge>());

    private static Option<Material> MaterialOf(Seq<Layer> layers, Seq<Material> materials, ObjectAttributes attributes) =>
        attributes.MaterialSource switch {
            ObjectMaterialSource.MaterialFromObject => Component(source: materials, index: attributes.MaterialIndex),
            ObjectMaterialSource.MaterialFromLayer => Component(source: layers, index: attributes.LayerIndex)
                .Bind(layer => Component(source: materials, index: layer.RenderMaterialIndex)),
            _ => Option<Material>.None,
        };

    private static Seq<T> TraverseRender<T>(RenderContent content, Option<RenderContent> parent, Func<RenderContent, Option<RenderContent>, Seq<T>> project) =>
        project(arg1: content, arg2: parent)
        + Rows(() => content.Children).Bind(child => TraverseRender(content: child, parent: Some(content), project: project));

    private static DocumentResourceKind RenderKind(RenderContent content) =>
        content switch {
            RenderMaterial => DocumentResourceKind.RenderMaterial,
            RenderEnvironment => DocumentResourceKind.RenderEnvironment,
            RenderTexture => DocumentResourceKind.RenderTexture,
            _ => DocumentResourceKind.RenderContent,
        };

    private static Fin<FileArchiveMetadata> Metadata(FileArchiveSource source, File3dmModel model, Seq<(string Name, Guid Id)> layouts) =>
        Op.Of(name: nameof(Metadata)).Catch(() => {
            Option<DateTime> createdOn = DateTimeOption(read: () => model.Created);
            Option<DateTime> lastEditedOn = DateTimeOption(read: () => model.LastEdited);
            DrawingBitmap? preview = model.GetPreviewImage();
            bool hasPreview = preview is not null;
            preview?.Dispose();
            File3dmSettings settings = model.Settings;
            using EarthAnchorPoint? anchor = model.EarthAnchorPoint;
            return Fin.Succ(new FileArchiveMetadata(
                ArchiveVersion: model.ArchiveVersion,
                Notes: TextOption(value: model.Notes.Notes),
                ApplicationName: TextOption(value: model.ApplicationName),
                ApplicationUrl: TextOption(value: model.ApplicationUrl),
                ApplicationDetails: TextOption(value: model.ApplicationDetails),
                Revision: PositiveOption(model.Revision),
                CreatedBy: TextOption(value: model.CreatedBy),
                LastEditedBy: TextOption(value: model.LastEditedBy),
                CreatedOn: createdOn,
                LastEditedOn: lastEditedOn,
                PageViews: source is FileArchiveSource.Path ? Some(layouts.Count) : Option<int>.None,
                Preview: hasPreview,
                ModelUnits: NotEqualOption(settings.ModelUnitSystem, UnitSystem.None),
                PageUnits: NotEqualOption(settings.PageUnitSystem, UnitSystem.None),
                ModelAbsoluteTolerance: PositiveOption(settings.ModelAbsoluteTolerance),
                ModelAngleToleranceRadians: PositiveOption(settings.ModelAngleToleranceRadians),
                ModelUrl: TextOption(value: settings.ModelUrl),
                ModelBasePoint: NotEqualOption(settings.ModelBasepoint, Point3d.Origin),
                EarthAnchor: FileGeoLocation.From(anchor: anchor),
                StartComments: TextOption(value: model.StartSectionComments)));
        });

    private static Fin<FileEndpoint> ExtractFile(EmbeddedFile file, FileEndpoint folder, Op op) =>
        FileEndpoint.From(path: IOPath.Combine(path1: folder.Path, path2: IOPath.GetFileName(path: file.Filename)))
            .Bind(target => target.Output(op: op))
            .Bind(target => file.SaveToFile(filename: target.Path) switch {
                true => Fin.Succ(value: target),
                false => Fin.Fail<FileEndpoint>(error: op.InvalidResult()),
            });

    private static Seq<FileIssue> IssueLog(string log) =>
        string.IsNullOrWhiteSpace(value: log) ? Seq<FileIssue>() : Seq(FileIssue.Native(message: log));

    private static Option<string> LogOption(string log) =>
        string.IsNullOrWhiteSpace(value: log) ? Option<string>.None : Some(log);

    internal static Option<string> TextOption(string? value) =>
        Optional(value).Filter(static text => !string.IsNullOrWhiteSpace(value: text)).Map(static text => text.Trim());

    internal static Option<Guid> GuidOption(Guid value) =>
        value switch {
            Guid id when id != Guid.Empty => Some(id),
            _ => Option<Guid>.None,
        };

    private static Option<T> PositiveOption<T>(T value) where T : INumber<T> =>
        value > T.Zero ? Some(value) : Option<T>.None;

    // Constraint is struct + default comparer (not IEquatable<T>): UnitSystem is an enum and
    // C# enum types do not implement IEquatable<TEnum>, while Point3d is a struct — both are value types.
    private static Option<T> NotEqualOption<T>(T value, T sentinel) where T : struct =>
        EqualityComparer<T>.Default.Equals(x: value, y: sentinel) ? Option<T>.None : Some(value);

    internal static Option<DateTime> DateTimeOption(DateTime value) =>
        value == DateTime.MinValue ? Option<DateTime>.None : Some(value);

    internal static Option<DateTime> DateTimeOption(Func<DateTime> read) {
        try {
            return Optional(read).Bind(active => DateTimeOption(value: active()));
        } catch (ArgumentOutOfRangeException) {
            return Option<DateTime>.None;
        }
    }
}
