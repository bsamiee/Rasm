using DrawingBitmap = System.Drawing.Bitmap;
using EmbeddedFile = global::Rhino.FileIO.File3dmEmbeddedFile;
using File3dmModel = global::Rhino.FileIO.File3dm;
using IODirectory = System.IO.Directory;
using IOPath = System.IO.Path;
using RenderContent = global::Rhino.FileIO.File3dmRenderContent;
using RenderTexture = global::Rhino.FileIO.File3dmRenderTexture;

namespace Rasm.Rhino.Exchange;

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record FileArchive(FileEndpoint Source, FileArchiveMetadata Metadata, FileResourceGraph Resources);

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
    int Manifest,
    Seq<string> EmbeddedFileNames,
    Seq<string> LinkedBlockArchives,
    Seq<string> RenderTextureFiles,
    Seq<string> FileReferences);

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class FileArchiveOps {
    internal static Fin<FileReport> Read(FileEndpoint source, ArchiveProfile profile) =>
        from endpoint in source.Input(op: Op.Of(name: nameof(Read)))
        from archive in UseArchive(source: endpoint, profile: profile, op: Op.Of(name: nameof(Read)), use: (model, log) => Snapshot(source: endpoint, model: model).Map(archive => (Archive: archive, Log: log)))
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
        from folder in EnsureFolder(endpoint: target, op: Op.Of(name: nameof(Extract)))
        from extracted in UseArchive(source: endpoint, profile: profile, op: Op.Of(name: nameof(Extract)), use: (model, log) =>
            toSeq(model.EmbeddedFiles)
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

    internal static Fin<FileReport> Insert(FileEndpoint source, FileEndpoint embedded, FileEndpoint target, ArchiveProfile profile) =>
        from endpoint in source.Input(op: Op.Of(name: nameof(Insert)))
        from payload in embedded.Input(op: Op.Of(name: nameof(Insert)))
        from output in target.WithFormat(format: FileFormat.ThreeDm).Output(op: Op.Of(name: nameof(Insert)))
        from result in UseArchive(source: endpoint, profile: profile, op: Op.Of(name: nameof(Insert)), use: (model, readLog) =>
            model.EmbeddedFiles.Add(filename: payload.Path) switch {
                true => WriteArchive(model: model, target: output, profile: profile, op: Op.Of(name: nameof(Insert))).Map(written => (ReadLog: readLog, WriteLog: written)),
                false => Fin.Fail<(string ReadLog, string WriteLog)>(error: Op.Of(name: nameof(Insert)).InvalidResult()),
            })
        select FileReport.Of(
            phase: FilePhase.ArchiveInsert,
            source: Some(endpoint),
            target: Some(output),
            format: output.Format,
            issues: IssueLog(log: result.ReadLog) + IssueLog(log: result.WriteLog),
            nativeLog: LogOption(log: string.Join(separator: System.Environment.NewLine, values: Seq(result.ReadLog, result.WriteLog).Filter(static value => !string.IsNullOrWhiteSpace(value: value)).AsIterable())));

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
            ArchiveSlice.Objects => (true, File3dmModel.TableTypeFilter.ObjectTable, File3dmModel.ObjectTypeFilter.Any),
            ArchiveSlice.Resources => (true, File3dmModel.TableTypeFilter.Bitmap | File3dmModel.TableTypeFilter.Material | File3dmModel.TableTypeFilter.Layer | File3dmModel.TableTypeFilter.Group | File3dmModel.TableTypeFilter.InstanceDefinition | File3dmModel.TableTypeFilter.UserTable, File3dmModel.ObjectTypeFilter.None),
            _ => (false, File3dmModel.TableTypeFilter.None, File3dmModel.ObjectTypeFilter.None),
        };

    private static Fin<FileArchive> Snapshot(FileEndpoint source, File3dmModel model) =>
        from metadata in Metadata(source: source)
        let embedded = toSeq(model.EmbeddedFiles).Map(static file => file.Filename)
        let linked = toSeq(model.AllInstanceDefinitions).Bind(static definition => NonBlankSeq(value: definition.SourceArchive)).Distinct()
        let renderMaterials = toSeq(model.RenderMaterials)
        let renderEnvironments = toSeq(model.RenderEnvironments)
        let renderTextures = toSeq(model.RenderTextures)
        let textures = TextureFiles(textures: renderTextures).Distinct()
        select new FileArchive(
            Source: source,
            Metadata: metadata,
            Resources: new FileResourceGraph(
                Objects: model.Objects.Count,
                Layers: model.AllLayers.Count,
                Materials: model.AllMaterials.Count,
                Groups: model.AllGroups.Count,
                Blocks: model.AllInstanceDefinitions.Count,
                Views: model.AllViews.Count,
                NamedViews: model.AllNamedViews.Count,
                Strings: model.Strings.Count,
                PlugInData: model.PlugInData.Count,
                EmbeddedFiles: toSeq(model.EmbeddedFiles).Count,
                RenderMaterials: renderMaterials.Count,
                RenderEnvironments: renderEnvironments.Count,
                RenderTextures: renderTextures.Count,
                Manifest: model.Manifest.Count,
                EmbeddedFileNames: embedded,
                LinkedBlockArchives: linked,
                RenderTextureFiles: textures,
                FileReferences: (linked + textures).Distinct()));

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
                Notes: Optional(File3dmModel.ReadNotes(path: source.Path)).Filter(static value => !string.IsNullOrWhiteSpace(value: value)),
                ApplicationName: Optional(applicationName).Filter(static value => !string.IsNullOrWhiteSpace(value: value)),
                ApplicationUrl: Optional(applicationUrl).Filter(static value => !string.IsNullOrWhiteSpace(value: value)),
                ApplicationDetails: Optional(applicationDetails).Filter(static value => !string.IsNullOrWhiteSpace(value: value)),
                Revision: revised ? Some(revision) : Option<int>.None,
                CreatedBy: revised ? Optional(createdBy).Filter(static value => !string.IsNullOrWhiteSpace(value: value)) : Option<string>.None,
                LastEditedBy: revised ? Optional(lastEditedBy).Filter(static value => !string.IsNullOrWhiteSpace(value: value)) : Option<string>.None,
                CreatedOn: revised ? Some(createdOn) : Option<DateTime>.None,
                LastEditedOn: revised ? Some(lastEditedOn) : Option<DateTime>.None,
                PageViews: toSeq(File3dmModel.ReadPageViews(path: source.Path)).Count,
                Preview: preview is not null);
        })
            .Run()
            .MapFail(_ => Op.Of(name: nameof(Metadata)).InvalidResult());

    private static Fin<FileEndpoint> EnsureFolder(FileEndpoint endpoint, Op op) =>
        Try.lift<Fin<FileEndpoint>>(f: () => {
            _ = IODirectory.CreateDirectory(path: endpoint.Path);
            return IODirectory.Exists(path: endpoint.Path) switch {
                true => Fin.Succ(value: endpoint),
                false => Fin.Fail<FileEndpoint>(error: op.InvalidResult()),
            };
        })
            .Run()
            .MapFail(_ => op.InvalidResult())
            .Bind(static result => result);

    private static Fin<FileEndpoint> ExtractFile(EmbeddedFile file, FileEndpoint folder, Op op) =>
        FileEndpoint.From(path: IOPath.Combine(path1: folder.Path, path2: IOPath.GetFileName(path: file.Filename)))
            .Bind(target => target.Output(op: op))
            .Bind(target => file.SaveToFile(filename: target.Path) switch {
                true => Fin.Succ(value: target),
                false => Fin.Fail<FileEndpoint>(error: op.InvalidResult()),
            });

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

    private static Seq<string> TextureFiles(Seq<RenderTexture> textures) =>
        toSeq(textures).Bind(static texture => TextureFiles(content: texture));

    private static Seq<string> TextureFiles(RenderContent content) =>
        TextureFile(content: content) + toSeq(content.Children).Bind(static child => TextureFiles(content: child));

    private static Seq<string> TextureFile(RenderContent content) =>
        content switch {
            RenderTexture texture => NonBlankSeq(value: texture.Filename),
            _ => Seq<string>(),
        };

    private static Seq<string> NonBlankSeq(string value) =>
        string.IsNullOrWhiteSpace(value: value) switch {
            false => Seq(value),
            true => Seq<string>(),
        };
}
