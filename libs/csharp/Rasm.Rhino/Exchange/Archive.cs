using Rasm.Rhino.Commands;
using DrawingBitmap = System.Drawing.Bitmap;
using EmbeddedFile = global::Rhino.FileIO.File3dmEmbeddedFile;
using File3dmModel = global::Rhino.FileIO.File3dm;
using File3dmNotes = global::Rhino.FileIO.File3dmNotes;
using IOPath = System.IO.Path;
using RenderContent = global::Rhino.FileIO.File3dmRenderContent;
using RenderEnvironment = global::Rhino.FileIO.File3dmRenderEnvironment;
using RenderMaterial = global::Rhino.FileIO.File3dmRenderMaterial;
using RenderTexture = global::Rhino.FileIO.File3dmRenderTexture;

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
    Seq<string> EmbeddedFileNames,
    Seq<string> LinkedBlockArchives,
    Seq<string> RenderTextureFiles,
    Seq<string> FileReferences);

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
        from extracted in UseArchive(source: endpoint, profile: profile, op: Op.Of(name: nameof(Extract)), use: (model, log) =>
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
            nativeLog: LogOption(log: extracted.Log));

    internal static Fin<FileReport> Update(FileEndpoint source, FileEndpoint target, ArchiveUpdate update, ArchiveProfile profile) =>
        from endpoint in source.Input(op: Op.Of(name: nameof(Update)))
        from output in target.WithFormat(format: FileFormat.ThreeDm).Output(op: Op.Of(name: nameof(Update)))
        from result in UseArchive(source: endpoint, profile: profile with { Slice = ArchiveSlice.Full }, op: Op.Of(name: nameof(Update)), use: (model, readLog) =>
            Patch(model: model, update: update, op: Op.Of(name: nameof(Update)))
                .Bind(patched => WriteArchive(model: model, target: output, profile: profile, op: Op.Of(name: nameof(Update)))
                    .Bind(writeLog => ExtractSelected(model: model, names: update.Extract, target: output, op: Op.Of(name: nameof(Update)))
                        .Bind(extracted => Snapshot(source: output, model: model, profile: profile)
                            .Map(archive => (ReadLog: readLog, WriteLog: writeLog, Archive: archive, Receipt: patched + Receipt(paths: extracted)))))))
        select FileReport.Of(
            phase: FilePhase.ArchiveUpdate,
            source: Some(endpoint),
            target: Some(output),
            format: output.Format,
            issues: IssueLog(log: result.ReadLog) + IssueLog(log: result.WriteLog),
            nativeLog: LogOption(log: string.Join(separator: System.Environment.NewLine, values: Seq(result.ReadLog, result.WriteLog).Filter(static value => !string.IsNullOrWhiteSpace(value: value)).AsIterable())),
            archive: Some(result.Archive),
            receipt: Some(result.Receipt));

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
            ArchiveSlice.Resources => (false, File3dmModel.TableTypeFilter.None, File3dmModel.ObjectTypeFilter.None),
            _ => (false, File3dmModel.TableTypeFilter.None, File3dmModel.ObjectTypeFilter.None),
        };

    private static Fin<FileArchive> Snapshot(FileEndpoint source, File3dmModel model, ArchiveProfile profile) =>
        profile.Projection switch {
            FileArchiveProjection.Full =>
                Metadata(source: source).Map(metadata => new FileArchive(Source: source, Metadata: metadata, Resources: Resources(model: model), Objects: Objects(model: model))),
            FileArchiveProjection.MetadataAndGraph =>
                Metadata(source: source).Map(metadata => new FileArchive(Source: source, Metadata: metadata, Resources: Resources(model: model), Objects: Seq<FileObjectManifest>())),
            FileArchiveProjection.Metadata =>
                Metadata(source: source).Map(metadata => new FileArchive(Source: source, Metadata: metadata, Resources: EmptyResources, Objects: Seq<FileObjectManifest>())),
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
                + toSeq(model.RenderTextures).Map(static texture => (RenderContent)texture)).Distinct()
        ) switch {
            (Seq<string> embedded, Seq<string> linked, Seq<RenderMaterial> renderMaterials, Seq<RenderEnvironment> renderEnvironments, Seq<RenderTexture> renderTextures, Seq<string> textures) => new FileResourceGraph(
                Objects: model.Objects.Count,
                Layers: model.AllLayers.Count,
                Materials: model.AllMaterials.Count,
                Groups: model.AllGroups.Count,
                Blocks: model.AllInstanceDefinitions.Count,
                Views: model.AllViews.Count,
                NamedViews: model.AllNamedViews.Count,
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
                EmbeddedFileNames: embedded,
                LinkedBlockArchives: linked,
                RenderTextureFiles: textures,
                FileReferences: (linked + textures).Distinct()),
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
            EmbeddedFileNames: Seq<string>(),
            LinkedBlockArchives: Seq<string>(),
            RenderTextureFiles: Seq<string>(),
            FileReferences: Seq<string>());

    private static Seq<FileObjectManifest> Objects(File3dmModel model) =>
        toSeq(model.Objects).Map(fileObject => new FileObjectManifest(
            Id: fileObject.Attributes.ObjectId,
            Name: Text(value: fileObject.Attributes.Name),
            Layer: Text(value: model.AllLayers.FindIndex(index: fileObject.Attributes.LayerIndex)?.FullPath),
            ObjectType: fileObject.Geometry.ObjectType,
            Material: fileObject.Attributes.MaterialSource switch {
                ObjectMaterialSource.MaterialFromObject => Text(value: model.AllMaterials.FindIndex(index: fileObject.Attributes.MaterialIndex)?.Name),
                ObjectMaterialSource.MaterialFromLayer => Optional(model.AllLayers.FindIndex(index: fileObject.Attributes.LayerIndex)?.RenderMaterialIndex).Bind(index => Text(value: model.AllMaterials.FindIndex(index: index)?.Name)),
                _ => Option<string>.None,
            },
            UserStrings: UserStrings(fileObject: fileObject)));

    private static Seq<string> UserStrings(global::Rhino.FileIO.File3dmObject fileObject) =>
        fileObject.Attributes.GetUserStrings()?.AllKeys switch {
            string[] keys => toSeq(keys).Choose(Text),
            _ => Seq<string>(),
        };

    private static Fin<FileArchiveMetadata> Metadata(FileEndpoint source) =>
        Try.lift<FileArchiveMetadata>(f: () => {
            string createdBy = string.Empty;
            string lastEditedBy = string.Empty;
            File3dmModel.ReadApplicationData(path: source.Path, applicationName: out string applicationName, applicationUrl: out string applicationUrl, applicationDetails: out string applicationDetails);
            bool revised = File3dmModel.ReadRevisionHistory(path: source.Path, createdBy: out createdBy, lastEditedBy: out lastEditedBy, revision: out int revision, createdOn: out DateTime createdOn, lastEditedOn: out DateTime lastEditedOn);
            DrawingBitmap? preview = File3dmModel.ReadPreviewImage(path: source.Path);
            preview?.Dispose();
            return new FileArchiveMetadata(
                ArchiveVersion: File3dmModel.ReadArchiveVersion(path: source.Path),
                Notes: Text(value: File3dmModel.ReadNotes(path: source.Path)),
                ApplicationName: Text(value: applicationName),
                ApplicationUrl: Text(value: applicationUrl),
                ApplicationDetails: Text(value: applicationDetails),
                Revision: revised ? Some(revision) : Option<int>.None,
                CreatedBy: revised ? Text(value: createdBy) : Option<string>.None,
                LastEditedBy: revised ? Text(value: lastEditedBy) : Option<string>.None,
                CreatedOn: revised ? Some(createdOn) : Option<DateTime>.None,
                LastEditedOn: revised ? Some(lastEditedOn) : Option<DateTime>.None,
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
                true => Fin.Succ(payload.Path),
                false => Fin.Fail<string>(error: op.InvalidResult()),
            }))
        from links in update.LinkBlocks.TraverseM(endpoint =>
            endpoint.Input(op: op).Bind(link => model.AllInstanceDefinitions.AddLinked(filename: link.Path, name: IOPath.GetFileNameWithoutExtension(path: link.Path), description: string.Empty) switch {
                >= 0 => Fin.Succ(link.Path),
                _ => Fin.Fail<string>(error: op.InvalidResult()),
            }))
        select Receipt(paths: MetadataReceipt(update: update) + embedded + links);

    private static Fin<Seq<string>> ExtractSelected(File3dmModel model, Seq<string> names, FileEndpoint target, Op op) =>
        from folder in names.IsEmpty switch {
            true => Fin.Succ(value: target.WithPath(path: IOPath.GetDirectoryName(path: target.Path) ?? target.Path)),
            false => target.WithPath(path: IOPath.GetDirectoryName(path: target.Path) ?? target.Path).Folder(op: op),
        }
        from extracted in toSeq(model.EmbeddedFiles)
            .Filter(file => (ArchiveProfile.Full with { Embedded = names }).Includes(file: file.Filename))
            .TraverseM(file => ExtractFile(file: file, folder: folder, op: op).Map(static endpoint => endpoint.Path))
        select extracted;

    private static DocumentReceipt Receipt(Seq<string> paths) =>
        DocumentReceipt.Empty with { ResourceChanged = paths.Map(static path => new DocumentResourceChange(Kind: DocumentResourceKind.Table, Name: path)) };

    private static Seq<string> MetadataReceipt(ArchiveUpdate update) =>
        update.Metadata.Case switch {
            FileArchiveMetadataPatch => Seq("metadata"),
            _ => Seq<string>(),
        };

    private static Fin<Unit> ApplyMetadata(File3dmModel model, FileArchiveMetadataPatch patch) =>
        Try.lift<Unit>(f: () => {
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
}
