using Rasm.Rhino.Commands;
using DrawingBitmap = System.Drawing.Bitmap;
using EmbeddedFile = Rhino.FileIO.File3dmEmbeddedFile;
using File3dmModel = Rhino.FileIO.File3dm;
using File3dmNotes = Rhino.FileIO.File3dmNotes;
using File3dmObject = Rhino.FileIO.File3dmObject;
using File3dmSettings = Rhino.FileIO.File3dmSettings;
using File3dmStrings = Rhino.FileIO.File3dmStringTable;
using IOPath = System.IO.Path;
using RenderContent = Rhino.FileIO.File3dmRenderContent;
using RenderEnvironment = Rhino.FileIO.File3dmRenderEnvironment;
using RenderMaterial = Rhino.FileIO.File3dmRenderMaterial;
using RenderTexture = Rhino.FileIO.File3dmRenderTexture;

namespace Rasm.Rhino.Exchange;

// --- [MODELS] -----------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context")]
public abstract partial record FileArchiveSource {
    private FileArchiveSource() { }
    public sealed record Path(FileEndpoint Value) : FileArchiveSource;
    public sealed record Bytes(ReadOnlyMemory<byte> Value) : FileArchiveSource;
}

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

// EarthAnchorPoint getters allocate per call; storing the wrapper past File3dm.Dispose dangles m_ptr — project scalars + KML triple here.
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

public readonly record struct FileResourceEdge(
    DocumentResourceKind FromKind,
    Option<Guid> FromId,
    DocumentResourceKind ToKind,
    Option<Guid> ToId,
    FileResourceRole Role,
    Option<string> Path = default);

public readonly record struct FileResourceGraph(
    int Objects,
    int Layers,
    int Materials,
    int Groups,
    int Blocks,
    int Views,
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
        Seq(Objects, Layers, Materials, Groups, Blocks, Views, NamedViews, Strings, PlugInData, EmbeddedFiles, RenderMaterials, RenderEnvironments, RenderTextures, Linetypes, DimensionStyles, HatchPatterns, NamedConstructionPlanes, Manifest, Relations)
            .TraverseM(count => count >= 0 ? Fin.Succ(value: (double)count) : Fin.Fail<double>(error: op.InvalidResult()))
            .As()
            .Bind(values => Stat.Of(values: values, key: op));

    public Fin<Seq<FileIssue>> Validate(FileArchiveSource source, IoScheduler scheduler) {
        Fin<FileArchiveSource> activeSource = Optional(source).ToFin(Fail: Op.Of(name: nameof(Validate)).InvalidInput());
        Fin<IoScheduler> activeScheduler = Optional(scheduler).ToFin(Fail: Op.Of(name: nameof(Validate)).InvalidInput());
        Option<string> folder = Optional(source).Bind(value => value switch {
            FileArchiveSource.Path path => Optional(IOPath.GetDirectoryName(path: path.Value.Path)),
            _ => Option<string>.None,
        });
        (string Raw, string Path)[] normalized = [.. (LinkedBlockArchives + RenderTextureFiles + FileReferences)
            .Filter(static path => !string.IsNullOrWhiteSpace(value: path))
            .Map(path => (Raw: path, Path: IOPath.IsPathRooted(path: path)
                ? path
                : folder.Map(root => IOPath.GetFullPath(path: IOPath.Combine(path1: root, path2: path))).IfNone(path)))
            .AsIterable()];
        (string Raw, string Path)[] paths = [.. Enumerable.GroupBy(source: normalized, keySelector: static item => item.Path, comparer: StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())];
        return from _ in activeSource
               from issues in paths.Length switch {
                   0 => Fin.Succ(Seq<FileIssue>()),
                   _ => activeScheduler.Bind(valid => valid.Filter(
                    source: paths,
                    predicate: static item => !File.Exists(path: item.Path),
                    map: static item => FileIssue.Of(code: FileIssueCode.BrokenLink, message: $"missing linked resource: {item.Raw} -> {item.Path}"))),
               }
               select issues;
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
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
            receipt: Some(DocumentReceipt.Empty with { ResourceChanged = extracted.Paths.Map(static endpoint => new DocumentResourceChange(Kind: DocumentResourceKind.EmbeddedFile, Name: endpoint.Path)) }));

    internal static Fin<FileReport> Update(FileArchiveSource source, FileEndpoint target, ArchiveUpdate update, ArchiveProfile profile) =>
        from output in target.WithFormat(format: FileFormat.ThreeDm).Output(op: Op.Of(name: nameof(Update)))
        let op = Op.Of(name: nameof(Update))
        from result in UseArchive(source: source, profile: profile with { Slice = ArchiveSlice.Full }, op: op, use: (endpoint, model, readLog) =>
            from metadataChange in update.Metadata.Case switch {
                FileArchiveMetadataPatch patch => op.Catch(() => {
                    _ = patch.Notes.Map(value => model.Notes = new File3dmNotes { Notes = value });
                    _ = patch.ApplicationName.Map(value => model.ApplicationName = value);
                    _ = patch.ApplicationUrl.Map(value => model.ApplicationUrl = value);
                    _ = patch.ApplicationDetails.Map(value => model.ApplicationDetails = value);
                    _ = patch.StartComments.Map(value => model.StartSectionComments = value);
                    // `Value None` deletes; `Section None` targets the flat key table. SetString returns the prior value (discarded).
                    _ = patch.UserStrings.Fold(unit, (_, entry) => (entry.Section.Case, entry.Value.Case) switch {
                        (string section, string value) => Op.Side(() => model.Strings.SetString(section: section, entry: entry.Key, value: value)),
                        (string section, _) => Op.Side(() => model.Strings.Delete(section: section, entry: entry.Key)),
                        (_, string value) => Op.Side(() => model.Strings.SetString(key: entry.Key, value: value)),
                        _ => Op.Side(() => model.Strings.Delete(key: entry.Key)),
                    });
                    return Fin.Succ(value: Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Metadata, Name: "metadata"))
                        + patch.UserStrings.Map(static entry => new DocumentResourceChange(Kind: DocumentResourceKind.Text, Name: entry.Key)));
                }),
                _ => Fin.Succ(value: Seq<DocumentResourceChange>()),
            }
            from embedded in update.Embed.TraverseM(endpoint =>
                endpoint.Input(op: op).Bind(payload => model.EmbeddedFiles.Add(filename: payload.Path) switch {
                    true => Fin.Succ(new DocumentResourceChange(Kind: DocumentResourceKind.EmbeddedFile, Name: payload.Path)),
                    false => Fin.Fail<DocumentResourceChange>(error: op.InvalidResult()),
                })).As()
            from writeLog in op.Catch(() => model.WriteWithLog(path: output.Path, options: FileFormatProjection.ArchiveWriteOptions(profile: profile), errorLog: out string log) switch {
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
                Receipt: DocumentReceipt.Empty with {
                    ResourceChanged = metadataChange + embedded + extracted.Map(static path => new DocumentResourceChange(Kind: DocumentResourceKind.EmbeddedFile, Name: path)),
                }))
        select FileReport.Of(
            phase: FilePhase.ArchiveUpdate,
            source: result.Endpoint,
            target: Some(output),
            format: output.Format,
            issues: result.SnapshotIssues + IssueLog(log: result.ReadLog) + IssueLog(log: result.WriteLog),
            nativeLog: LogOption(log: string.Join(separator: System.Environment.NewLine, values: Seq(result.ReadLog, result.WriteLog).Filter(static value => !string.IsNullOrWhiteSpace(value: value)).AsIterable())),
            archive: Some(result.Archive),
            receipt: Some(result.Receipt));

    internal static Fin<byte[]> Bytes(FileArchiveSource source, ArchiveProfile profile) =>
        UseArchive(source: source, profile: profile, op: Op.Of(name: nameof(Bytes)), use: (_, model, _) =>
            Optional(model.ToByteArray(options: FileFormatProjection.ArchiveWriteOptions(profile: profile)))
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
            use: (_, model, _) => Metadata(source: new FileArchiveSource.Path(Value: path), model: model))
        select metadata;

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
                    return Optional(owned).ToFin(Fail: ctx.Op.InvalidResult()).Bind(active => ctx.Use(arg1: Some(endpoint), arg2: active, arg3: log));
                })
                select value,
            bytes: static (ctx, source) =>
                from memory in source.Value.IsEmpty
                    ? Fin.Fail<ReadOnlyMemory<byte>>(error: ctx.Op.InvalidInput())
                    : Fin.Succ(value: source.Value)
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
                from metadata in (projection & FileArchiveProjection.Metadata) != FileArchiveProjection.None
                    ? Metadata(source: source, model: model)
                    : Fin.Succ(value: default(FileArchiveMetadata))
                from resources in (projection & FileArchiveProjection.Graph) != FileArchiveProjection.None
                    ? Resources(model: model, source: source)
                    : Fin.Succ(value: (Graph: default(FileResourceGraph), Issues: Seq<FileIssue>()))
                select (
                    Archive: new FileArchive(
                        Source: source,
                        Metadata: metadata,
                        Resources: resources.Graph,
                        Objects: (projection & FileArchiveProjection.Objects) != FileArchiveProjection.None ? Objects(model: model).Strict() : Seq<FileObjectManifest>()),
                    resources.Issues),
        };

    private static Option<FileFormat> FormatOf(Option<FileEndpoint> source) =>
        source.Bind(static endpoint => endpoint.Format).Case switch {
            FileFormat format => Some(format),
            _ => Some(FileFormat.ThreeDm),
        };

    // Block-graph + linked-archive closure on the Fin rail: a failed graph build or broken closure surfaces as a
    // FileReport issue instead of silently degrading to Graph.Empty / an empty link set (which masked missing blocks
    // and unreadable archives). FileArchiveSource → archivePath discrimination lives at the Resources call site.
    private static Fin<(Blocks.Archive.Graph Graph, Seq<string> Linked, Seq<FileIssue> Issues)> BlockArchiveResources(File3dmModel model, Option<string> archivePath, Op key) =>
        from graph in archivePath
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
        select (graph, closure.Linked, closure.Issues);

    // Stage 1 (fallible): resolve block archive resources on the Fin rail; metadata/object resources must survive block graph faults.
    private static Fin<(FileResourceGraph Graph, Seq<FileIssue> Issues)> Resources(File3dmModel model, FileArchiveSource source) {
        Op key = Op.Of(name: nameof(Resources));
        Option<string> archivePath = source switch {
            FileArchiveSource.Path path => Some(path.Value.Path),
            _ => Option<string>.None,
        };
        return BlockArchiveResources(model: model, archivePath: archivePath, key: key)
            .BiBind(
                Succ: resources => key.Catch(() => Fin.Succ(value: (
                    ResourceGraphOf(model: model, blockGraph: resources.Graph, linked: resources.Linked),
                    resources.Issues))),
                Fail: error => key.Catch(() => Fin.Succ(value: (
                    Graph: ResourceGraphOf(model: model, blockGraph: Blocks.Archive.Graph.Empty, linked: Seq<string>()),
                    Issues: Seq(FileIssue.Native(message: error.Message))))))
            .BindFail(static error => Fin.Succ(value: (Graph: default(FileResourceGraph), Issues: Seq(FileIssue.Native(message: error.Message)))));
    }

    // Stage 2 (pure projection): ResourceGraphOf stays pure; Resources owns graph/closure failure containment.
    private static FileResourceGraph ResourceGraphOf(File3dmModel model, Blocks.Archive.Graph blockGraph, Seq<string> linked) {
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
                .Filter(static entry => entry.Name.Case is string || entry.Value.Case is string))
            .IfNone(Seq<FileResourceEntry>());
        Seq<FileResourceEntry> entries =
            embeddedFiles.Map(file => new FileResourceEntry(Kind: DocumentResourceKind.EmbeddedFile, Name: TextOption(value: IOPath.GetFileName(path: file.Filename)), Path: TextOption(value: file.Filename), Id: Option<Guid>.None, Source: TextOption(value: file.ComponentType.ToString())))
            + plugInData.Map(data => new FileResourceEntry(Kind: DocumentResourceKind.Metadata, Name: Option<string>.None, Path: Option<string>.None, Id: GuidOption(value: data.PlugInId), PlugInId: GuidOption(value: data.PlugInId), Source: Some("File3dmPlugInData")))
            + textEntries
            + renderEntries
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
            Objects: ObjectCount(model: model, objects: objects),
            Layers: layers.Count,
            Materials: materials.Count,
            Groups: groups.Count,
            Blocks: blockGraph.Definitions.Length,
            Views: views.Count,
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

    private static Seq<FileObjectManifest> Objects(File3dmModel model) =>
        Objects(model: model, objects: Rows(() => model.Objects), layers: Rows(() => model.AllLayers), materials: Rows(() => model.AllMaterials));

    private static Seq<FileObjectManifest> Objects(File3dmModel model, Seq<File3dmObject> objects, Seq<Layer> layers, Seq<Material> materials) =>
        objects.Map(fileObject => {
            // File3dmObject.Geometry returns null when the native geometry pointer is zero (GeometryBase.CreateGeometryHelper); proxy objects (named-position snapshots) carry no realized geometry.
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

    private static int ObjectCount(File3dmModel model, Seq<File3dmObject> objects) =>
        Math.Max(
            val1: objects.Count,
            val2: Native(read: () => model.Manifest).Map(static manifest => manifest.ActiveObjectCount(type: ModelComponentType.ModelGeometry)).IfNone(0));

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

    private static Fin<FileArchiveMetadata> Metadata(FileArchiveSource source, File3dmModel model) =>
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
                Revision: model.Revision > 0 ? Some(model.Revision) : Option<int>.None,
                CreatedBy: TextOption(value: model.CreatedBy),
                LastEditedBy: TextOption(value: model.LastEditedBy),
                CreatedOn: createdOn,
                LastEditedOn: lastEditedOn,
                // Page-view count has no in-memory accessor; bytes sources have no path, so page count is unknown.
                PageViews: source switch {
                    FileArchiveSource.Path path => Some(Rows(File3dmModel.ReadPageViews(path: path.Value.Path)).Count),
                    _ => Option<int>.None,
                },
                Preview: hasPreview,
                ModelUnits: settings.ModelUnitSystem == UnitSystem.None ? Option<UnitSystem>.None : Some(settings.ModelUnitSystem),
                PageUnits: settings.PageUnitSystem == UnitSystem.None ? Option<UnitSystem>.None : Some(settings.PageUnitSystem),
                ModelAbsoluteTolerance: settings.ModelAbsoluteTolerance > 0.0 ? Some(settings.ModelAbsoluteTolerance) : Option<double>.None,
                ModelAngleToleranceRadians: settings.ModelAngleToleranceRadians > 0.0 ? Some(settings.ModelAngleToleranceRadians) : Option<double>.None,
                ModelUrl: TextOption(value: settings.ModelUrl),
                ModelBasePoint: settings.ModelBasepoint == Point3d.Origin ? Option<Point3d>.None : Some(settings.ModelBasepoint),
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
        string.IsNullOrWhiteSpace(value: log) switch {
            false => Seq(FileIssue.Native(message: log)),
            true => Seq<FileIssue>(),
        };

    private static Option<string> LogOption(string log) =>
        string.IsNullOrWhiteSpace(value: log) switch {
            false => Some(log),
            true => Option<string>.None,
        };

    internal static Option<string> TextOption(string? value) =>
        Optional(value).Filter(static text => !string.IsNullOrWhiteSpace(value: text)).Map(static text => text.Trim());

    internal static Option<Guid> GuidOption(Guid value) =>
        value switch {
            Guid id when id != Guid.Empty => Some(id),
            _ => Option<Guid>.None,
        };

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
