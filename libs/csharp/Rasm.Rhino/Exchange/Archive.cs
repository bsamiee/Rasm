using Rasm.Rhino.Commands;
using DrawingBitmap = System.Drawing.Bitmap;
using EmbeddedFile = Rhino.FileIO.File3dmEmbeddedFile;
using File3dmModel = Rhino.FileIO.File3dm;
using File3dmNotes = Rhino.FileIO.File3dmNotes;
using File3dmObject = Rhino.FileIO.File3dmObject;
using InstanceReferenceGeometry = Rhino.Geometry.InstanceReferenceGeometry;
using IOPath = System.IO.Path;
using RenderContent = Rhino.FileIO.File3dmRenderContent;
using RenderEnvironment = Rhino.FileIO.File3dmRenderEnvironment;
using RenderMaterial = Rhino.FileIO.File3dmRenderMaterial;
using RenderTexture = Rhino.FileIO.File3dmRenderTexture;

namespace Rasm.Rhino.Exchange;

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record FileArchive(FileEndpoint Source, FileArchiveMetadata Metadata, FileResourceGraph Resources, Seq<FileObjectManifest> Objects);

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
    bool Preview);

public readonly record struct FileResourceEntry(
    string Kind,
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
    string FromKind,
    Option<Guid> FromId,
    string ToKind,
    Option<Guid> ToId,
    string Role,
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
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class FileArchiveOps {
    internal static Fin<FileReport> Read(FileEndpoint source, ArchiveProfile profile) =>
        from endpoint in source.Input(op: Op.Of(name: nameof(Read)))
        from archive in UseArchive(source: endpoint, profile: profile, op: Op.Of(name: nameof(Read)), use: (model, log) => Snapshot(source: endpoint, model: model, profile: profile).Map(archive => (Archive: archive, Log: log)))
        select FileReport.Of(
            phase: FilePhase.ArchiveRead,
            source: Some(endpoint),
            target: Option<FileEndpoint>.None,
            format: endpoint.Format,
            issues: IssueLog(log: archive.Log),
            nativeLog: LogOption(log: archive.Log),
            archive: Some(archive.Archive));

    internal static Fin<FileReport> Extract(FileEndpoint source, FileEndpoint target, ArchiveProfile profile) =>
        from endpoint in source.Input(op: Op.Of(name: nameof(Extract)))
        from folder in target.Folder(op: Op.Of(name: nameof(Extract)))
        let full = profile with { Slice = ArchiveSlice.Full }
        from extracted in UseArchive(source: endpoint, profile: full, op: Op.Of(name: nameof(Extract)), use: (model, log) =>
            toSeq(model.EmbeddedFiles)
                .Filter(file => profile.Includes(file: file.Filename))
                .TraverseM(file => ExtractFile(file: file, folder: folder, op: Op.Of(name: nameof(Extract))))
                .As()
                .Map(paths => (Paths: paths, Log: log)))
        select FileReport.Of(
            phase: FilePhase.ArchiveExtract,
            source: Some(endpoint),
            target: Some(folder),
            format: endpoint.Format,
            issues: extracted.Paths.IsEmpty ? Seq(FileIssue.Native(message: "archive contains no embedded files")) : IssueLog(log: extracted.Log),
            nativeLog: LogOption(log: extracted.Log),
            receipt: Some(Receipt(changes: extracted.Paths.Map(static endpoint => new DocumentResourceChange(Kind: DocumentResourceKind.Table, Name: endpoint.Path)))));

    internal static Fin<FileReport> Update(FileEndpoint source, FileEndpoint target, ArchiveUpdate update, ArchiveProfile profile) =>
        from endpoint in source.Input(op: Op.Of(name: nameof(Update)))
        from output in target.WithFormat(format: FileFormat.ThreeDm).Output(op: Op.Of(name: nameof(Update)))
        from result in UseArchive(source: endpoint, profile: profile with { Slice = ArchiveSlice.Full }, op: Op.Of(name: nameof(Update)), use: (model, readLog) =>
            Patch(model: model, update: update, op: Op.Of(name: nameof(Update)))
                .Bind(patched => WriteArchive(model: model, target: output, profile: profile, op: Op.Of(name: nameof(Update)))
                    .Bind(writeLog => ExtractSelected(model: model, names: update.Extract, target: output, op: Op.Of(name: nameof(Update)))
                        .Bind(extracted => Snapshot(source: output, model: model, profile: profile)
                            .Map(archive => (ReadLog: readLog, WriteLog: writeLog, Archive: archive, Receipt: patched + Receipt(changes: extracted.Map(static path => new DocumentResourceChange(Kind: DocumentResourceKind.Table, Name: path)))))))))
        select FileReport.Of(
            phase: FilePhase.ArchiveUpdate,
            source: Some(endpoint),
            target: Some(output),
            format: output.Format,
            issues: IssueLog(log: result.ReadLog) + IssueLog(log: result.WriteLog),
            nativeLog: LogOption(log: string.Join(separator: System.Environment.NewLine, values: Seq(result.ReadLog, result.WriteLog).Filter(static value => !string.IsNullOrWhiteSpace(value: value)).AsIterable())),
            archive: Some(result.Archive),
            receipt: Some(result.Receipt));

    internal static Fin<byte[]> Bytes(FileEndpoint source, ArchiveProfile profile) =>
        from endpoint in source.Input(op: Op.Of(name: nameof(Bytes)))
        from bytes in UseArchive(source: endpoint, profile: profile, op: Op.Of(name: nameof(Bytes)), use: (model, _) =>
            Optional(model.ToByteArray(options: FileFormatProjection.ArchiveWriteOptions(profile: profile)))
                .Filter(static value => value.Length > 0)
                .ToFin(Fail: Op.Of(name: nameof(Bytes)).InvalidResult()))
        select bytes;

    private static Fin<T> UseArchive<T>(FileEndpoint source, ArchiveProfile profile, Op op, Func<File3dmModel, string, Fin<T>> use) =>
        Try.lift<Fin<T>>(f: () => {
            File3dmModel? model = null;
            string log = string.Empty;
            // BOUNDARY ADAPTER — File3dm is unmanaged archive state and must be disposed after projection.
            try {
                model = ReadModel(source: source, profile: profile, log: out log);
                return Optional(model).ToFin(Fail: op.InvalidResult()).Bind(active => use(arg1: active, arg2: log));
            } finally {
                model?.Dispose();
            }
        })
            .Run()
            .MapFail(_ => op.InvalidResult())
            .Bind(static result => result);

    private static File3dmModel? ReadModel(FileEndpoint source, ArchiveProfile profile, out string log) =>
        NativeFilter(profile: profile) switch {
            (false, _, _) => File3dmModel.ReadWithLog(path: source.Path, errorLog: out log),
            (true, File3dmModel.TableTypeFilter tables, File3dmModel.ObjectTypeFilter objects) => File3dmModel.ReadWithLog(path: source.Path, tableTypeFilterFilter: tables, objectTypeFilter: objects, errorLog: out log),
        };

    private static (bool Filtered, File3dmModel.TableTypeFilter Tables, File3dmModel.ObjectTypeFilter Objects) NativeFilter(ArchiveProfile profile) =>
        profile.Slice switch {
            ArchiveSlice.Metadata => (true, File3dmModel.TableTypeFilter.Properties | File3dmModel.TableTypeFilter.Settings, File3dmModel.ObjectTypeFilter.None),
            ArchiveSlice.Objects => (true, File3dmModel.TableTypeFilter.ObjectTable | File3dmModel.TableTypeFilter.Layer | File3dmModel.TableTypeFilter.Material, File3dmModel.ObjectTypeFilter.Any),
            ArchiveSlice.Resources => (
                true,
                File3dmModel.TableTypeFilter.Properties
                | File3dmModel.TableTypeFilter.Settings
                | File3dmModel.TableTypeFilter.Bitmap
                | File3dmModel.TableTypeFilter.TextureMapping
                | File3dmModel.TableTypeFilter.Material
                | File3dmModel.TableTypeFilter.Linetype
                | File3dmModel.TableTypeFilter.Layer
                | File3dmModel.TableTypeFilter.Group
                | File3dmModel.TableTypeFilter.Dimstyle
                | File3dmModel.TableTypeFilter.Hatchpattern
                | File3dmModel.TableTypeFilter.InstanceDefinition
                | File3dmModel.TableTypeFilter.ObjectTable
                | File3dmModel.TableTypeFilter.UserTable,
                File3dmModel.ObjectTypeFilter.Any),
            _ => (false, File3dmModel.TableTypeFilter.None, File3dmModel.ObjectTypeFilter.None),
        };

    private static Fin<FileArchive> Snapshot(FileEndpoint source, File3dmModel model, ArchiveProfile profile) =>
        profile.Projection switch {
            FileArchiveProjection.Full =>
                Metadata(source: source, model: model).Map(metadata => new FileArchive(Source: source, Metadata: metadata, Resources: Resources(model: model), Objects: Objects(model: model))),
            FileArchiveProjection.MetadataAndGraph =>
                Metadata(source: source, model: model).Map(metadata => new FileArchive(Source: source, Metadata: metadata, Resources: Resources(model: model), Objects: Seq<FileObjectManifest>())),
            FileArchiveProjection.Metadata =>
                Metadata(source: source, model: model).Map(metadata => new FileArchive(Source: source, Metadata: metadata, Resources: EmptyResources, Objects: Seq<FileObjectManifest>())),
            FileArchiveProjection.Graph =>
                Fin.Succ(value: new FileArchive(Source: source, Metadata: EmptyMetadata, Resources: Resources(model: model), Objects: Seq<FileObjectManifest>())),
            FileArchiveProjection.Objects =>
                Fin.Succ(value: new FileArchive(Source: source, Metadata: EmptyMetadata, Resources: EmptyResources, Objects: Objects(model: model))),
            _ =>
                Fin.Fail<FileArchive>(error: Op.Of(name: nameof(Snapshot)).InvalidInput()),
        };

    private static FileResourceGraph Resources(File3dmModel model) =>
        (
            Embedded: toSeq(model.EmbeddedFiles).Map(static file => file.Filename),
            Linked: toSeq(model.AllInstanceDefinitions).Bind(static definition => Text(value: definition.SourceArchive).Map(Seq).IfNone(Seq<string>())).Distinct(),
            RenderMaterials: toSeq(model.RenderMaterials),
            RenderEnvironments: toSeq(model.RenderEnvironments),
            RenderTextures: toSeq(model.RenderTextures),
            TextureFiles: TextureFiles(contents:
                toSeq(model.RenderMaterials).Map(static material => (RenderContent)material)
                + toSeq(model.RenderEnvironments).Map(static environment => (RenderContent)environment)
                + toSeq(model.RenderTextures).Map(static texture => (RenderContent)texture)).Distinct(),
            Entries: ResourceEntries(model: model) + OrganizationEntries(model: model),
            Edges: ResourceEdges(model: model)
        ) switch {
            (Seq<string> embedded, Seq<string> linked, Seq<RenderMaterial> renderMaterials, Seq<RenderEnvironment> renderEnvironments, Seq<RenderTexture> renderTextures, Seq<string> textures, Seq<FileResourceEntry> entries, Seq<FileResourceEdge> edges) => new FileResourceGraph(
                Objects: model.Objects.Count,
                Layers: model.AllLayers.Count,
                Materials: model.AllMaterials.Count,
                Groups: model.AllGroups.Count,
                Blocks: model.AllInstanceDefinitions.Count,
                Views: model.Views.Count,
                NamedViews: model.NamedViews.Count,
                Strings: model.Strings.Count,
                PlugInData: model.PlugInData.Count,
                EmbeddedFiles: embedded.Count,
                RenderMaterials: renderMaterials.Count,
                RenderEnvironments: renderEnvironments.Count,
                RenderTextures: renderTextures.Count,
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
                Edges: edges),
        };

    private static FileArchiveMetadata EmptyMetadata => default;

    private static FileResourceGraph EmptyResources =>
        new(
            Objects: 0,
            Layers: 0,
            Materials: 0,
            Groups: 0,
            Blocks: 0,
            Views: 0,
            NamedViews: 0,
            Strings: 0,
            PlugInData: 0,
            EmbeddedFiles: 0,
            RenderMaterials: 0,
            RenderEnvironments: 0,
            RenderTextures: 0,
            Linetypes: 0,
            DimensionStyles: 0,
            HatchPatterns: 0,
            NamedConstructionPlanes: 0,
            Manifest: 0,
            Relations: 0,
            EmbeddedFileNames: Seq<string>(),
            LinkedBlockArchives: Seq<string>(),
            RenderTextureFiles: Seq<string>(),
            FileReferences: Seq<string>(),
            Entries: Seq<FileResourceEntry>(),
            Edges: Seq<FileResourceEdge>());

    private static Seq<FileObjectManifest> Objects(File3dmModel model) =>
        toSeq(model.Objects).Map(fileObject => new FileObjectManifest(
            Id: fileObject.Attributes.ObjectId,
            Name: Text(value: fileObject.Attributes.Name),
            Layer: Text(value: model.AllLayers.FindIndex(index: fileObject.Attributes.LayerIndex)?.FullPath),
            ObjectType: fileObject.Geometry.ObjectType,
            Material: MaterialOf(model: model, fileObject: fileObject).Bind(material => Text(value: material.Name)),
            UserStrings: UserStrings(fileObject: fileObject)));

    private static Seq<string> UserStrings(File3dmObject fileObject) =>
        (fileObject.Attributes.GetUserStrings()?.AllKeys switch {
            string[] keys => toSeq(keys).Choose(Text),
            _ => Seq<string>(),
        }) + (fileObject.Geometry.GetUserStrings()?.AllKeys switch {
            string[] keys => toSeq(keys).Choose(Text),
            _ => Seq<string>(),
        });

    private static Seq<FileResourceEntry> ResourceEntries(File3dmModel model) =>
        toSeq(model.EmbeddedFiles).Map(file => new FileResourceEntry(Kind: "embedded", Name: Text(value: IOPath.GetFileName(path: file.Filename)), Path: Text(value: file.Filename), Id: Option<Guid>.None, Source: Text(value: file.ComponentType.ToString())))
        + toSeq(model.PlugInData).Map(data => new FileResourceEntry(Kind: "plugin", Name: Option<string>.None, Path: Option<string>.None, Id: GuidOption(value: data.PlugInId), PlugInId: GuidOption(value: data.PlugInId), Source: Some("File3dmPlugInData")))
        + toSeq(Enumerable.Range(start: 0, count: model.Strings.Count))
            .Map(index => new FileResourceEntry(Kind: "string", Name: Text(value: model.Strings.GetKey(i: index)), Path: Option<string>.None, Id: Option<Guid>.None, Value: Text(value: model.Strings.GetValue(i: index))))
            .Filter(static entry => entry.Name.Case is string || entry.Value.Case is string)
        + RenderEntries(contents:
            toSeq(model.RenderMaterials).Map(static material => (RenderContent)material)
            + toSeq(model.RenderEnvironments).Map(static environment => (RenderContent)environment)
            + toSeq(model.RenderTextures).Map(static texture => (RenderContent)texture))
        + toSeq(Enum.GetValues<ModelComponentType>())
            .Filter(static type => type is not ModelComponentType.Unset and not ModelComponentType.Mixed)
            .Map(type => new FileResourceEntry(Kind: type.ToString(), Name: Option<string>.None, Path: Option<string>.None, Id: Option<Guid>.None, Count: model.Manifest.ActiveObjectCount(type: type), Source: Some("manifest")))
            .Filter(static entry => entry.Count > 0);

    private static Seq<FileResourceEntry> OrganizationEntries(File3dmModel model) =>
        toSeq(model.AllLayers).Map(layer => new FileResourceEntry(Kind: "layer", Name: Text(value: layer.FullPath), Path: Option<string>.None, Id: GuidOption(value: layer.Id), Source: Some("layer")))
        + toSeq(model.AllMaterials).Map(material => new FileResourceEntry(Kind: "material", Name: Text(value: material.Name), Path: Option<string>.None, Id: GuidOption(value: material.Id), Source: Some("material")))
        + toSeq(model.AllLinetypes).Map(linetype => new FileResourceEntry(Kind: "linetype", Name: Text(value: linetype.Name), Path: Option<string>.None, Id: GuidOption(value: linetype.Id), Source: Some("linetype")))
        + toSeq(model.AllDimStyles).Map(style => new FileResourceEntry(Kind: "dimension-style", Name: Text(value: style.Name), Path: Option<string>.None, Id: GuidOption(value: style.Id), Source: Some("dimension-style")))
        + toSeq(model.AllHatchPatterns).Map(pattern => new FileResourceEntry(Kind: "hatch-pattern", Name: Text(value: pattern.Name), Path: Option<string>.None, Id: GuidOption(value: pattern.Id), Source: Some("hatch-pattern")))
        + toSeq(model.AllNamedConstructionPlanes).Map(plane => new FileResourceEntry(Kind: "named-cplane", Name: Text(value: plane.Name), Path: Option<string>.None, Id: Option<Guid>.None, Source: Some("named-cplane")));

    private static Seq<FileResourceEdge> ResourceEdges(File3dmModel model) =>
        toSeq(model.Objects).Bind(fileObject =>
            Seq(new FileResourceEdge(FromKind: "object", FromId: GuidOption(value: fileObject.Attributes.ObjectId), ToKind: "layer", ToId: Optional(model.AllLayers.FindIndex(index: fileObject.Attributes.LayerIndex)).Bind(layer => GuidOption(value: layer.Id)), Role: "layer"))
            + MaterialEdge(model: model, fileObject: fileObject).Map(Seq).IfNone(Seq<FileResourceEdge>())
            + LinetypeEdge(model: model, fileObject: fileObject).Map(Seq).IfNone(Seq<FileResourceEdge>())
            + GroupEdges(model: model, fileObject: fileObject)
            + BlockInstanceEdge(fileObject: fileObject).Map(Seq).IfNone(Seq<FileResourceEdge>()))
        + toSeq(model.AllInstanceDefinitions).Bind(definition =>
            Text(value: definition.SourceArchive)
                .Map(path => Seq(new FileResourceEdge(FromKind: "block", FromId: GuidOption(value: definition.Id), ToKind: "archive", ToId: Option<Guid>.None, Role: "linked", Path: Some(path))))
                .IfNone(Seq<FileResourceEdge>())
            + toSeq(definition.GetObjectIds()).Map(id => new FileResourceEdge(FromKind: "block", FromId: GuidOption(value: definition.Id), ToKind: "object", ToId: GuidOption(value: id), Role: "member")))
        + RenderEdges(contents:
            toSeq(model.RenderMaterials).Map(static material => (RenderContent)material)
            + toSeq(model.RenderEnvironments).Map(static environment => (RenderContent)environment)
            + toSeq(model.RenderTextures).Map(static texture => (RenderContent)texture));

    private static Option<FileResourceEdge> MaterialEdge(File3dmModel model, File3dmObject fileObject) =>
        MaterialOf(model: model, fileObject: fileObject)
            .Bind(material => GuidOption(value: material.Id))
            .Map(id => new FileResourceEdge(FromKind: "object", FromId: GuidOption(value: fileObject.Attributes.ObjectId), ToKind: "material", ToId: Some(id), Role: "material"));

    private static Option<Material> MaterialOf(File3dmModel model, File3dmObject fileObject) =>
        fileObject.Attributes.MaterialSource switch {
            ObjectMaterialSource.MaterialFromObject => Optional(model.AllMaterials.FindIndex(index: fileObject.Attributes.MaterialIndex)),
            ObjectMaterialSource.MaterialFromLayer => Optional(model.AllLayers.FindIndex(index: fileObject.Attributes.LayerIndex)?.RenderMaterialIndex)
                .Bind(index => Optional(model.AllMaterials.FindIndex(index: index))),
            _ => Option<Material>.None,
        };

    private static Option<FileResourceEdge> LinetypeEdge(File3dmModel model, File3dmObject fileObject) =>
        (fileObject.Attributes.LinetypeSource switch {
            ObjectLinetypeSource.LinetypeFromObject => Optional(model.AllLinetypes.FindIndex(index: fileObject.Attributes.LinetypeIndex)),
            ObjectLinetypeSource.LinetypeFromLayer => Optional(model.AllLayers.FindIndex(index: fileObject.Attributes.LayerIndex)?.LinetypeIndex)
                .Bind(index => Optional(model.AllLinetypes.FindIndex(index: index))),
            _ => Option<Linetype>.None,
        })
        .Bind(linetype => GuidOption(value: linetype.Id))
        .Map(id => new FileResourceEdge(FromKind: "object", FromId: GuidOption(value: fileObject.Attributes.ObjectId), ToKind: "linetype", ToId: Some(id), Role: "linetype"));

    private static Seq<FileResourceEdge> GroupEdges(File3dmModel model, File3dmObject fileObject) =>
        toSeq(fileObject.Attributes.GetGroupList() ?? [])
            .Choose(index => Optional(model.AllGroups.FindIndex(index))
                .Bind(group => GuidOption(value: group.Id))
                .Map(id => new FileResourceEdge(FromKind: "object", FromId: GuidOption(value: fileObject.Attributes.ObjectId), ToKind: "group", ToId: Some(id), Role: "group")));

    private static Option<FileResourceEdge> BlockInstanceEdge(File3dmObject fileObject) =>
        fileObject.Geometry switch {
            InstanceReferenceGeometry reference => GuidOption(value: reference.ParentIdefId)
                .Map(id => new FileResourceEdge(FromKind: "object", FromId: GuidOption(value: fileObject.Attributes.ObjectId), ToKind: "block", ToId: Some(id), Role: "instance")),
            _ => Option<FileResourceEdge>.None,
        };

    private static Seq<FileResourceEntry> RenderEntries(Seq<RenderContent> contents) =>
        toSeq(contents).Bind(static content => RenderEntries(content: content));

    private static Seq<FileResourceEntry> RenderEntries(RenderContent content) =>
        Seq(RenderEntry(kind: RenderKind(content: content), content: content))
        + toSeq(content.Children).Bind(static child => RenderEntries(content: child));

    private static Seq<FileResourceEdge> RenderEdges(Seq<RenderContent> contents) =>
        toSeq(contents).Bind(static content => RenderEdges(content: content));

    private static Seq<FileResourceEdge> RenderEdges(RenderContent content) =>
        (content switch {
            RenderTexture texture => Text(value: texture.Filename)
                .Map(path => Seq(new FileResourceEdge(FromKind: RenderKind(content: texture), FromId: GuidOption(value: texture.Id), ToKind: "file", ToId: Option<Guid>.None, Role: "texture", Path: Some(path))))
                .IfNone(Seq<FileResourceEdge>()),
            _ => Seq<FileResourceEdge>(),
        })
        + toSeq(content.Children).Bind(child =>
            Seq(new FileResourceEdge(FromKind: RenderKind(content: content), FromId: GuidOption(value: content.Id), ToKind: RenderKind(content: child), ToId: GuidOption(value: child.Id), Role: Text(value: child.ChildSlotName).IfNone("child")))
            + RenderEdges(content: child));

    private static string RenderKind(RenderContent content) =>
        content switch {
            RenderMaterial => "render-material",
            RenderEnvironment => "render-environment",
            RenderTexture => "render-texture",
            _ => "render-content",
        };

    private static FileResourceEntry RenderEntry(string kind, RenderContent content) =>
        new(
            Kind: kind,
            Name: Text(value: content.TypeName),
            Path: content switch {
                RenderTexture texture => Text(value: texture.Filename),
                _ => Option<string>.None,
            },
            Id: GuidOption(value: content.Id),
            Value: Text(value: content.Kind.ToString()),
            TypeId: GuidOption(value: content.TypeId),
            PlugInId: GuidOption(value: content.PlugInId),
            RenderEngineId: GuidOption(value: content.RenderEngineId),
            GroupId: GuidOption(value: content.GroupId),
            Source: content.Reference ? Some("reference") : Some("embedded"));

    private static Fin<FileArchiveMetadata> Metadata(FileEndpoint source, File3dmModel model) =>
        Try.lift(f: () => {
            DateTime createdOn = model.Created;
            DateTime lastEditedOn = model.LastEdited;
            DrawingBitmap? preview = model.GetPreviewImage();
            preview?.Dispose();
            return new FileArchiveMetadata(
                ArchiveVersion: model.ArchiveVersion,
                Notes: Text(value: model.Notes.Notes),
                ApplicationName: Text(value: model.ApplicationName),
                ApplicationUrl: Text(value: model.ApplicationUrl),
                ApplicationDetails: Text(value: model.ApplicationDetails),
                Revision: model.Revision > 0 ? Some(model.Revision) : Option<int>.None,
                CreatedBy: Text(value: model.CreatedBy),
                LastEditedBy: Text(value: model.LastEditedBy),
                CreatedOn: createdOn == DateTime.MinValue ? Option<DateTime>.None : Some(createdOn),
                LastEditedOn: lastEditedOn == DateTime.MinValue ? Option<DateTime>.None : Some(lastEditedOn),
                PageViews: toSeq(File3dmModel.ReadPageViews(path: source.Path)).Count,
                Preview: preview is not null);
        })
            .Run()
            .MapFail(_ => Op.Of(name: nameof(Metadata)).InvalidResult());

    private static Fin<FileEndpoint> ExtractFile(EmbeddedFile file, FileEndpoint folder, Op op) =>
        FileEndpoint.From(path: IOPath.Combine(path1: folder.Path, path2: IOPath.GetFileName(path: file.Filename)))
            .Bind(target => target.Output(op: op))
            .Bind(target => file.SaveToFile(filename: target.Path) switch {
                true => Fin.Succ(value: target),
                false => Fin.Fail<FileEndpoint>(error: op.InvalidResult()),
            });

    private static Fin<DocumentReceipt> Patch(File3dmModel model, ArchiveUpdate update, Op op) =>
        from metadata in update.Metadata.Case switch {
            FileArchiveMetadataPatch patch => ApplyMetadata(model: model, patch: patch),
            _ => Fin.Succ(unit),
        }
        from embedded in update.Embed.TraverseM(endpoint =>
            endpoint.Input(op: op).Bind(payload => model.EmbeddedFiles.Add(filename: payload.Path) switch {
                true => Fin.Succ(new DocumentResourceChange(Kind: DocumentResourceKind.Table, Name: payload.Path)),
                false => Fin.Fail<DocumentResourceChange>(error: op.InvalidResult()),
            }))
        from links in update.LinkBlocks.TraverseM(endpoint =>
            endpoint.Input(op: op).Bind(link => model.AllInstanceDefinitions.AddLinked(filename: link.Path, name: IOPath.GetFileNameWithoutExtension(path: link.Path), description: string.Empty) switch {
                >= 0 => Fin.Succ(new DocumentResourceChange(Kind: DocumentResourceKind.Block, Name: link.Path)),
                _ => Fin.Fail<DocumentResourceChange>(error: op.InvalidResult()),
            }))
        select Receipt(changes: MetadataReceipt(update: update) + embedded + links);

    private static Fin<Seq<string>> ExtractSelected(File3dmModel model, Seq<string> names, FileEndpoint target, Op op) =>
        from folder in names.IsEmpty switch {
            true => Fin.Succ(value: target.WithPath(path: IOPath.GetDirectoryName(path: target.Path) ?? target.Path)),
            false => target.WithPath(path: IOPath.GetDirectoryName(path: target.Path) ?? target.Path).Folder(op: op),
        }
        from extracted in toSeq(model.EmbeddedFiles)
            .Filter(file => (ArchiveProfile.Full with { Embedded = names }).Includes(file: file.Filename))
            .TraverseM(file => ExtractFile(file: file, folder: folder, op: op).Map(static endpoint => endpoint.Path))
        select extracted;

    private static DocumentReceipt Receipt(Seq<DocumentResourceChange> changes) =>
        DocumentReceipt.Empty with { ResourceChanged = changes };

    private static Seq<DocumentResourceChange> MetadataReceipt(ArchiveUpdate update) =>
        update.Metadata.Case switch {
            FileArchiveMetadataPatch => Seq(new DocumentResourceChange(Kind: DocumentResourceKind.Table, Name: "metadata")),
            _ => Seq<DocumentResourceChange>(),
        };

    private static Fin<Unit> ApplyMetadata(File3dmModel model, FileArchiveMetadataPatch patch) =>
        Try.lift(f: () => {
            _ = patch.Notes.Map(value => model.Notes = new File3dmNotes { Notes = value });
            _ = patch.ApplicationName.Map(value => model.ApplicationName = value);
            _ = patch.ApplicationUrl.Map(value => model.ApplicationUrl = value);
            _ = patch.ApplicationDetails.Map(value => model.ApplicationDetails = value);
            return unit;
        })
            .Run()
            .MapFail(_ => Op.Of(name: nameof(ApplyMetadata)).InvalidResult());

    private static Fin<string> WriteArchive(File3dmModel model, FileEndpoint target, ArchiveProfile profile, Op op) =>
        Try.lift<Fin<string>>(f: () => {
            string log = string.Empty;
            return model.WriteWithLog(path: target.Path, options: FileFormatProjection.ArchiveWriteOptions(profile: profile), errorLog: out log) switch {
                true => Fin.Succ(value: log),
                false => Fin.Fail<string>(error: op.InvalidResult()),
            };
        })
            .Run()
            .MapFail(_ => op.InvalidResult())
            .Bind(static result => result);

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

    private static Seq<string> TextureFiles(Seq<RenderContent> contents) =>
        toSeq(contents).Bind(static content => TextureFiles(content: content));

    private static Seq<string> TextureFiles(RenderContent content) =>
        TextureFile(content: content) + toSeq(content.Children).Bind(static child => TextureFiles(content: child));

    private static Seq<string> TextureFile(RenderContent content) =>
        content switch {
            RenderTexture texture => Text(value: texture.Filename).Map(Seq).IfNone(Seq<string>()),
            _ => Seq<string>(),
        };

    private static Option<string> Text(string? value) =>
        Optional(value).Filter(static text => !string.IsNullOrWhiteSpace(value: text)).Map(static text => text.Trim());

    private static Option<Guid> GuidOption(Guid value) =>
        value switch {
            Guid id when id != Guid.Empty => Some(id),
            _ => Option<Guid>.None,
        };
}
