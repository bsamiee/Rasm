using Rasm.Rhino.Commands;
using DrawingBitmap = System.Drawing.Bitmap;
using EmbeddedFile = Rhino.FileIO.File3dmEmbeddedFile;
using File3dmModel = Rhino.FileIO.File3dm;
using File3dmNotes = Rhino.FileIO.File3dmNotes;
using File3dmObject = Rhino.FileIO.File3dmObject;
using File3dmSettings = Rhino.FileIO.File3dmSettings;
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
    int PageViews,
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
public readonly record struct FileGeoLocation(
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

    public Seq<FileIssue> Validate(FileArchiveSource source, IoScheduler scheduler) {
        ArgumentNullException.ThrowIfNull(argument: scheduler);
        Option<string> folder = source switch {
            FileArchiveSource.Path path => Optional(IOPath.GetDirectoryName(path: path.Value.Path)),
            _ => Option<string>.None,
        };
        (string Raw, string Path)[] paths = [.. (LinkedBlockArchives + RenderTextureFiles + FileReferences)
            .Distinct()
            .Filter(static path => !string.IsNullOrWhiteSpace(value: path))
            .Map(path => (Raw: path, Path: IOPath.IsPathRooted(path: path)
                ? path
                : folder.Map(root => IOPath.GetFullPath(path: IOPath.Combine(path1: root, path2: path))).IfNone(path)))
            .AsIterable()];
        return paths.Length switch {
            0 => Seq<FileIssue>(),
            _ => scheduler.Filter(
                source: paths,
                predicate: static item => !File.Exists(path: item.Path),
                map: static item => FileIssue.Of(code: FileIssueCode.BrokenLink, message: $"missing linked resource: {item.Raw} -> {item.Path}")),
        };
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class FileArchiveOps {
    internal static Fin<FileReport> Read(FileArchiveSource source, ArchiveProfile profile) =>
        from archive in UseArchive(source: source, profile: profile, op: Op.Of(name: nameof(Read)), use: (endpoint, model, log) => Snapshot(source: source, model: model, profile: profile).Map(archive => (Endpoint: endpoint, Archive: archive, Log: log)))
        select FileReport.Of(
            phase: FilePhase.ArchiveRead,
            source: archive.Endpoint,
            target: Option<FileEndpoint>.None,
            format: FormatOf(source: archive.Endpoint),
            issues: IssueLog(log: archive.Log),
            nativeLog: LogOption(log: archive.Log),
            archive: Some(archive.Archive));

    internal static Fin<FileReport> Extract(FileArchiveSource source, FileEndpoint target, ArchiveProfile profile) =>
        from folder in target.Folder(op: Op.Of(name: nameof(Extract)))
        let full = profile with { Slice = ArchiveSlice.Full }
        from extracted in UseArchive(source: source, profile: full, op: Op.Of(name: nameof(Extract)), use: (endpoint, model, log) =>
            toSeq(model.EmbeddedFiles)
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
                    return Fin.Succ(value: Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Metadata, Name: "metadata")));
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
            from extracted in toSeq(model.EmbeddedFiles)
                .Filter(file => (ArchiveProfile.Full with { Embedded = update.Extract }).Includes(file: file.Filename))
                .TraverseM(file => ExtractFile(file: file, folder: extractFolder, op: op).Map(static endpoint => endpoint.Path)).As()
            from snapshot in Snapshot(source: new FileArchiveSource.Path(Value: output), model: model, profile: profile)
            select (Endpoint: endpoint, ReadLog: readLog, WriteLog: writeLog, Archive: snapshot,
                Receipt: DocumentReceipt.Empty with {
                    ResourceChanged = metadataChange + embedded + extracted.Map(static path => new DocumentResourceChange(Kind: DocumentResourceKind.EmbeddedFile, Name: path)),
                }))
        select FileReport.Of(
            phase: FilePhase.ArchiveUpdate,
            source: result.Endpoint,
            target: Some(output),
            format: output.Format,
            issues: IssueLog(log: result.ReadLog) + IssueLog(log: result.WriteLog),
            nativeLog: LogOption(log: string.Join(separator: System.Environment.NewLine, values: Seq(result.ReadLog, result.WriteLog).Filter(static value => !string.IsNullOrWhiteSpace(value: value)).AsIterable())),
            archive: Some(result.Archive),
            receipt: Some(result.Receipt));

    internal static Fin<byte[]> Bytes(FileArchiveSource source, ArchiveProfile profile) =>
        UseArchive(source: source, profile: profile, op: Op.Of(name: nameof(Bytes)), use: (_, model, _) =>
            Optional(model.ToByteArray(options: FileFormatProjection.ArchiveWriteOptions(profile: profile)))
                .Filter(static value => value.Length > 0)
                .ToFin(Fail: Op.Of(name: nameof(Bytes)).InvalidResult()));

    internal static Fin<FileReport> Validate(FileArchiveSource source, ArchiveProfile profile, IoScheduler scheduler) =>
        from result in UseArchive(source: source, profile: profile with { Projection = FileArchiveProjection.Graph }, op: Op.Of(name: nameof(Validate)), use: (endpoint, model, log) =>
            Snapshot(source: source, model: model, profile: profile with { Projection = FileArchiveProjection.Graph })
                .Map(archive => (Endpoint: endpoint, Archive: archive, Log: log, Issues: archive.Resources.Validate(source: archive.Source, scheduler: scheduler))))
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
                        : (File3dmModel.ReadWithLog(path: endpoint.Path, errorLog: out string fullLog), fullLog);
                    using File3dmModel? owned = model;
                    Fin<T> work = Optional(owned).ToFin(Fail: ctx.Op.InvalidResult()).Bind(active => ctx.Use(arg1: Some(endpoint), arg2: active, arg3: log));
                    return work.Match(
                        Fail: static fail => Fin.Fail<T>(fail),
                        Succ: static succ => Fin.Succ(succ));
                })
                select value,
            bytes: static (ctx, source) =>
                from memory in source.Value.IsEmpty
                    ? Fin.Fail<ReadOnlyMemory<byte>>(error: ctx.Op.InvalidInput())
                    : Fin.Succ(value: source.Value)
                from value in ctx.Op.Catch(() => {
                    using File3dmModel? model = File3dmModel.FromByteArray(bytes: memory.ToArray());
                    Fin<T> work = Optional(model).ToFin(Fail: ctx.Op.InvalidResult()).Bind(active => ctx.Use(arg1: Option<FileEndpoint>.None, arg2: active, arg3: string.Empty));
                    return work.Match(
                        Fail: static fail => Fin.Fail<T>(fail),
                        Succ: static succ => Fin.Succ(succ));
                })
                select value)
        select result;

    private static Fin<FileArchive> Snapshot(FileArchiveSource source, File3dmModel model, ArchiveProfile profile) =>
        profile.Projection switch {
            FileArchiveProjection.None => Fin.Fail<FileArchive>(error: Op.Of(name: nameof(Snapshot)).InvalidInput()),
            FileArchiveProjection projection =>
                ((projection & FileArchiveProjection.Metadata) != FileArchiveProjection.None
                    ? Metadata(source: source, model: model)
                    : Fin.Succ(value: default(FileArchiveMetadata))).Map(metadata => new FileArchive(
                        Source: source,
                        Metadata: metadata,
                        Resources: (projection & FileArchiveProjection.Graph) != FileArchiveProjection.None ? Resources(model: model, source: source) : default,
                        Objects: (projection & FileArchiveProjection.Objects) != FileArchiveProjection.None ? Objects(model: model) : Seq<FileObjectManifest>())),
        };

    private static Option<FileFormat> FormatOf(Option<FileEndpoint> source) =>
        source.Bind(static endpoint => endpoint.Format).Case switch {
            FileFormat format => Some(format),
            _ => Some(FileFormat.ThreeDm),
        };

    private static (Blocks.Archive.Graph Graph, Seq<string> Linked) BlockArchiveResources(File3dmModel model, FileArchiveSource source, Op key) =>
        source switch {
            FileArchiveSource.Path path => BlockArchiveResources(model: model, archivePath: Some(path.Value.Path), key: key),
            _ => BlockArchiveResources(model: model, archivePath: Option<string>.None, key: key),
        };

    private static (Blocks.Archive.Graph Graph, Seq<string> Linked) BlockArchiveResources(File3dmModel model, Option<string> archivePath, Op key) {
        Blocks.Archive.Graph graph = archivePath
            .Map(path => Blocks.Archive.From(model: model, archivePath: Some(path), key: key))
            .IfNone(() => Blocks.Archive.From(model: model, key: key))
            .IfFail(_ => Blocks.Archive.Graph.Empty);
        Seq<string> linked = archivePath
            .Map(path => Blocks.Archive.ValidateArchiveClosure(root: model, rootPath: path, key: key)
                .Map(report => report.Edges.Map(static edge => edge.Link.Full.Value).Distinct())
                .IfFail(_ => Seq<string>()))
            .IfNone(() => toSeq(graph.LinkedArchives.AsEnumerable()).Map(static link => link.Stored));
        return (Graph: graph, Linked: linked);
    }

    private static FileResourceGraph Resources(File3dmModel model, FileArchiveSource source) {
        Op key = Op.Of(name: nameof(Resources));
        (Blocks.Archive.Graph blockGraph, Seq<string> linked) = BlockArchiveResources(model: model, source: source, key: key);
        Seq<RenderContent> renderTree =
            toSeq(model.RenderMaterials).Map(static material => (RenderContent)material)
            + toSeq(model.RenderEnvironments).Map(static environment => (RenderContent)environment)
            + toSeq(model.RenderTextures).Map(static texture => (RenderContent)texture);
        Seq<string> embedded = toSeq(model.EmbeddedFiles).Map(static file => file.Filename);
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
        Seq<FileResourceEntry> entries =
            toSeq(model.EmbeddedFiles).Map(file => new FileResourceEntry(Kind: DocumentResourceKind.EmbeddedFile, Name: TextOption(value: IOPath.GetFileName(path: file.Filename)), Path: TextOption(value: file.Filename), Id: Option<Guid>.None, Source: TextOption(value: file.ComponentType.ToString())))
            + toSeq(model.PlugInData).Map(data => new FileResourceEntry(Kind: DocumentResourceKind.Metadata, Name: Option<string>.None, Path: Option<string>.None, Id: GuidOption(value: data.PlugInId), PlugInId: GuidOption(value: data.PlugInId), Source: Some("File3dmPlugInData")))
            + toSeq(model.Strings.GetSectionNames() ?? [])
                .Bind(section => toSeq(model.Strings.GetEntryNames(section: section) ?? [])
                    .Map(entry => new FileResourceEntry(
                        Kind: DocumentResourceKind.Text,
                        Name: TextOption(value: entry),
                        Path: TextOption(value: section),
                        Id: Option<Guid>.None,
                        Value: TextOption(value: model.Strings.GetValue(section: section, entry: entry)),
                        Source: Some("section"))))
                .Filter(static entry => entry.Name.Case is string || entry.Value.Case is string)
            + toSeq(Enumerable.Range(start: 0, count: model.Strings.Count))
                .Map(index => new FileResourceEntry(Kind: DocumentResourceKind.Text, Name: TextOption(value: model.Strings.GetKey(i: index)), Path: Option<string>.None, Id: Option<Guid>.None, Value: TextOption(value: model.Strings.GetValue(i: index)), Source: Some("flat")))
                .Filter(static entry => entry.Name.Case is string || entry.Value.Case is string)
            + renderEntries
            + ManifestEntries(model: model)
            + Entries(source: model.AllLayers, kind: DocumentResourceKind.Layer, name: static l => TextOption(value: l.FullPath), id: static l => GuidOption(value: l.Id), label: "layer")
            + Entries(source: model.AllMaterials, kind: DocumentResourceKind.Material, name: static m => TextOption(value: m.Name), id: static m => GuidOption(value: m.Id), label: "material")
            + Entries(source: model.AllGroups, kind: DocumentResourceKind.Group, name: static g => TextOption(value: g.Name), id: static g => GuidOption(value: g.Id), label: "group")
            + Blocks.Archive.ToFileResourceEntries(graph: blockGraph)
            + Entries(source: model.AllLinetypes, kind: DocumentResourceKind.Linetype, name: static l => TextOption(value: l.Name), id: static l => GuidOption(value: l.Id), label: "linetype")
            + Entries(source: model.AllDimStyles, kind: DocumentResourceKind.DimensionStyle, name: static s => TextOption(value: s.Name), id: static s => GuidOption(value: s.Id), label: "dimension-style")
            + Entries(source: model.AllHatchPatterns, kind: DocumentResourceKind.Hatch, name: static p => TextOption(value: p.Name), id: static p => GuidOption(value: p.Id), label: "hatch-pattern")
            + Entries(source: model.AllNamedConstructionPlanes, kind: DocumentResourceKind.ConstructionPlane, name: static p => TextOption(value: p.Name), id: static _ => Option<Guid>.None, label: "named-cplane")
            + Entries(source: model.Views, kind: DocumentResourceKind.View, name: static v => TextOption(value: v.Name), id: static _ => Option<Guid>.None, label: "view", path: static v => TextOption(value: v.WallpaperFilename))
            + Entries(source: model.NamedViews, kind: DocumentResourceKind.NamedView, name: static v => TextOption(value: v.Name), id: static v => GuidOption(value: v.NamedViewId), label: "named-view", path: static v => TextOption(value: v.WallpaperFilename));
        Seq<FileResourceEdge> renderEdges = renderTree.Bind(static content => TraverseRender(content: content, parent: Option<RenderContent>.None, project: static (cur, par) => {
            Seq<FileResourceEdge> parentEdge = par.Map(p => Seq(new FileResourceEdge(FromKind: RenderKind(content: p), FromId: GuidOption(value: p.Id), ToKind: RenderKind(content: cur), ToId: GuidOption(value: cur.Id), Role: FileResourceRole.Child))).IfNone(Seq<FileResourceEdge>());
            Seq<FileResourceEdge> textureEdge = cur is RenderTexture texture
                ? TextOption(value: texture.Filename).Map(path => Seq(new FileResourceEdge(FromKind: RenderKind(content: texture), FromId: GuidOption(value: texture.Id), ToKind: DocumentResourceKind.FileReference, ToId: Option<Guid>.None, Role: FileResourceRole.Texture, Path: Some(path)))).IfNone(Seq<FileResourceEdge>())
                : Seq<FileResourceEdge>();
            return parentEdge + textureEdge;
        }));
        Seq<FileResourceEdge> edges = toSeq(model.Objects).Bind(fileObject => ObjectEdges(model: model, fileObject: fileObject))
            + Blocks.Archive.ToFileResourceEdges(graph: blockGraph)
            + renderEdges;
        return new FileResourceGraph(
            Objects: model.Objects.Count,
            Layers: model.AllLayers.Count,
            Materials: model.AllMaterials.Count,
            Groups: model.AllGroups.Count,
            Blocks: blockGraph.Definitions.Length,
            Views: model.Views.Count,
            NamedViews: model.NamedViews.Count,
            Strings: model.Strings.Count,
            PlugInData: model.PlugInData.Count,
            EmbeddedFiles: embedded.Count,
            RenderMaterials: toSeq(model.RenderMaterials).Count,
            RenderEnvironments: toSeq(model.RenderEnvironments).Count,
            RenderTextures: toSeq(model.RenderTextures).Count,
            Linetypes: model.AllLinetypes.Count,
            DimensionStyles: model.AllDimStyles.Count,
            HatchPatterns: model.AllHatchPatterns.Count,
            NamedConstructionPlanes: model.AllNamedConstructionPlanes.Count,
            Manifest: model.Manifest.Count,
            Relations: edges.Count,
            EmbeddedFileNames: embedded,
            LinkedBlockArchives: linked,
            RenderTextureFiles: textures,
            FileReferences: (linked + textures + edges.Bind(static edge => edge.Path.Map(Seq).IfNone(Seq<string>()))).Distinct(),
            Entries: entries,
            Edges: edges);
    }

    private static Seq<FileObjectManifest> Objects(File3dmModel model) =>
        toSeq(model.Objects).Map(fileObject => new FileObjectManifest(
            Id: fileObject.Attributes.ObjectId,
            Name: TextOption(value: fileObject.Attributes.Name),
            Layer: TextOption(value: model.AllLayers.FindIndex(index: fileObject.Attributes.LayerIndex)?.FullPath),
            ObjectType: fileObject.Geometry.ObjectType,
            Material: MaterialOf(model: model, fileObject: fileObject).Bind(material => TextOption(value: material.Name)),
            UserStrings: (fileObject.Attributes.GetUserStrings()?.AllKeys switch {
                string[] keys => toSeq(keys).Choose(static value => TextOption(value: value)),
                _ => Seq<string>(),
            }) + (fileObject.Geometry.GetUserStrings()?.AllKeys switch {
                string[] keys => toSeq(keys).Choose(static value => TextOption(value: value)),
                _ => Seq<string>(),
            })));

    private static Seq<FileResourceEntry> ManifestEntries(File3dmModel model) =>
        toSeq(Enum.GetValues<ModelComponentType>())
            .Choose(type => DocumentResourceKind.ForComponentType(type: type).Map(kind => (Kind: kind, Type: type)))
            .Map(row => new FileResourceEntry(Kind: row.Kind, Name: TextOption(value: row.Type.ToString()), Path: Option<string>.None, Id: Option<Guid>.None, Count: model.Manifest.ActiveObjectCount(type: row.Type), Source: Some("manifest")))
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

    private static Seq<FileResourceEdge> ObjectEdges(File3dmModel model, File3dmObject fileObject) {
        Option<Guid> objectId = GuidOption(value: fileObject.Attributes.ObjectId);
        Seq<FileResourceEdge> layerEdge = Seq(new FileResourceEdge(
            FromKind: DocumentResourceKind.Object,
            FromId: objectId,
            ToKind: DocumentResourceKind.Layer,
            ToId: Optional(model.AllLayers.FindIndex(index: fileObject.Attributes.LayerIndex)).Bind(layer => GuidOption(value: layer.Id)),
            Role: FileResourceRole.Layer));
        Seq<FileResourceEdge> materialEdge = MaterialOf(model: model, fileObject: fileObject)
            .Bind(material => GuidOption(value: material.Id))
            .Map(id => Seq(new FileResourceEdge(FromKind: DocumentResourceKind.Object, FromId: objectId, ToKind: DocumentResourceKind.Material, ToId: Some(id), Role: FileResourceRole.Material)))
            .IfNone(Seq<FileResourceEdge>());
        Seq<FileResourceEdge> linetypeEdge = (fileObject.Attributes.LinetypeSource switch {
            ObjectLinetypeSource.LinetypeFromObject => Optional(model.AllLinetypes.FindIndex(index: fileObject.Attributes.LinetypeIndex)),
            ObjectLinetypeSource.LinetypeFromLayer => Optional(model.AllLayers.FindIndex(index: fileObject.Attributes.LayerIndex)?.LinetypeIndex)
                .Bind(index => Optional(model.AllLinetypes.FindIndex(index: index))),
            _ => Option<Linetype>.None,
        })
        .Bind(linetype => GuidOption(value: linetype.Id))
        .Map(id => Seq(new FileResourceEdge(FromKind: DocumentResourceKind.Object, FromId: objectId, ToKind: DocumentResourceKind.Linetype, ToId: Some(id), Role: FileResourceRole.Linetype)))
        .IfNone(Seq<FileResourceEdge>());
        Seq<FileResourceEdge> groupEdges = toSeq(fileObject.Attributes.GetGroupList() ?? [])
            .Choose(index => Optional(model.AllGroups.FindIndex(index))
                .Bind(group => GuidOption(value: group.Id))
                .Map(id => new FileResourceEdge(FromKind: DocumentResourceKind.Object, FromId: objectId, ToKind: DocumentResourceKind.Group, ToId: Some(id), Role: FileResourceRole.Group)));
        return layerEdge + materialEdge + linetypeEdge + groupEdges;
    }

    private static Option<Material> MaterialOf(File3dmModel model, File3dmObject fileObject) =>
        fileObject.Attributes.MaterialSource switch {
            ObjectMaterialSource.MaterialFromObject => Optional(model.AllMaterials.FindIndex(index: fileObject.Attributes.MaterialIndex)),
            ObjectMaterialSource.MaterialFromLayer => Optional(model.AllLayers.FindIndex(index: fileObject.Attributes.LayerIndex)?.RenderMaterialIndex)
                .Bind(index => Optional(model.AllMaterials.FindIndex(index: index))),
            _ => Option<Material>.None,
        };

    private static Seq<T> TraverseRender<T>(RenderContent content, Option<RenderContent> parent, Func<RenderContent, Option<RenderContent>, Seq<T>> project) =>
        project(arg1: content, arg2: parent)
        + toSeq(content.Children).Bind(child => TraverseRender(content: child, parent: Some(content), project: project));

    private static DocumentResourceKind RenderKind(RenderContent content) =>
        content switch {
            RenderMaterial => DocumentResourceKind.RenderMaterial,
            RenderEnvironment => DocumentResourceKind.RenderEnvironment,
            RenderTexture => DocumentResourceKind.RenderTexture,
            _ => DocumentResourceKind.RenderContent,
        };

    private static Fin<FileArchiveMetadata> Metadata(FileArchiveSource source, File3dmModel model) =>
        Op.Of(name: nameof(Metadata)).Catch(() => {
            DateTime createdOn = model.Created;
            DateTime lastEditedOn = model.LastEdited;
            DrawingBitmap? preview = model.GetPreviewImage();
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
                CreatedOn: DateTimeOption(value: createdOn),
                LastEditedOn: DateTimeOption(value: lastEditedOn),
                PageViews: source switch {
                    FileArchiveSource.Path path => toSeq(File3dmModel.ReadPageViews(path: path.Value.Path)).Count,
                    _ => 0,
                },
                Preview: preview is not null,
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
}
