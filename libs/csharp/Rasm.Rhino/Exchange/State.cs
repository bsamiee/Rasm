using Rasm.Rhino.Commands;
using IODirectory = System.IO.Directory;
using IOPath = System.IO.Path;

namespace Rasm.Rhino.Exchange;

// --- [TYPES] ------------------------------------------------------------------------------
public enum FilePromptMode { OpenOne, OpenMany, Save, Folder }
public enum FileOverwritePolicy { Fail, Replace }
public enum FileDirectoryPolicy { Create, Existing }
public enum FileCollisionPolicy { Preserve, AppendNumber, Replace }
public enum FileFidelity { Model, Small, GeometryOnly }
public enum FileResourcePolicy { Reference, Embed, Copy }
public enum FileGrouping { Document, File, Layer, ObjectName, ObjectType, Material, Block, UserString }
public enum FileSort { Stable, File, Layer, ObjectName, ObjectType, Material, Block, UserString }
public enum FilePhase { Prompt, Open, Headless, Import, Export, Publish, NamedLayerState, Save, SaveAs, WriteFile, Write3dmFile, SaveTemplate, ArchiveRead, ArchiveExtract, ArchiveUpdate, Batch }
public enum ArchiveSlice { Full, Metadata, Objects, Resources }
public enum FileArchiveProjection { Full, MetadataAndGraph, Metadata, Graph, Objects }

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct FileNamePolicy(FileCollisionPolicy Collision = FileCollisionPolicy.Preserve, Option<string> Extension = default) {
    public static FileNamePolicy Default => default;
    internal FileNamePolicy Normalized => this;
}

public readonly record struct FileWritePolicy(
    FileOverwritePolicy Overwrite = FileOverwritePolicy.Fail,
    FileDirectoryPolicy Directory = FileDirectoryPolicy.Create,
    bool IncludeRenderMeshes = true,
    bool IncludePreviewImage = true,
    bool IncludeBitmapTable = true,
    bool IncludeHistory = true,
    bool WriteUserData = true,
    bool GeometryOnly = false,
    bool UseCompression = true,
    bool CreateBackupFiles = true,
    bool CreateOtherBackupFiles = true,
    Option<Transform> Xform = default,
    int Version = 0) {
    public static FileWritePolicy Default => new(IncludeRenderMeshes: true, IncludePreviewImage: true, IncludeBitmapTable: true, IncludeHistory: true, WriteUserData: true, UseCompression: true, CreateBackupFiles: true, CreateOtherBackupFiles: true);
    internal FileWritePolicy Normalized => this == default ? Default : this;
}

public readonly record struct FileBatchPolicy(bool ContinueOnError = false) {
    public static FileBatchPolicy StopOnFirstError { get; } = new(ContinueOnError: false);
}

public readonly record struct FileIssue(string Code, string Message) {
    public static FileIssue Native(string message) =>
        new(Code: "native", Message: string.IsNullOrWhiteSpace(value: message) ? "No native log." : message);
}

public sealed record FilePrompt {
    private FilePrompt(FilePromptMode mode, string title, string filter, Option<string> fileName, Option<string> initialDirectory, Option<string> defaultExtension, FileNamePolicy name, FileWritePolicy write) =>
        (Mode, Title, Filter, FileName, InitialDirectory, DefaultExtension, Name, Write) = (mode, title, filter, fileName, initialDirectory, defaultExtension, PromptName(name: name, defaultExtension: defaultExtension), write.Normalized);

    public FilePromptMode Mode { get; }
    public string Title { get; }
    public string Filter { get; }
    public Option<string> FileName { get; }
    public Option<string> InitialDirectory { get; }
    public Option<string> DefaultExtension { get; }
    public FileNamePolicy Name { get; }
    public FileWritePolicy Write { get; }

    public static Fin<FilePrompt> Open(string title, string filter = "", bool multiple = false, Option<string> fileName = default, Option<string> initialDirectory = default, Option<string> defaultExtension = default) =>
        Create(mode: multiple ? FilePromptMode.OpenMany : FilePromptMode.OpenOne, title: title, filter: string.IsNullOrWhiteSpace(value: filter) ? FileFormat.Filter(phase: FilePhase.Import) : filter, fileName: fileName, initialDirectory: initialDirectory, defaultExtension: defaultExtension);

    public static Fin<FilePrompt> Save(string title, string filter = "", Option<string> fileName = default, Option<string> initialDirectory = default, Option<string> defaultExtension = default, FileWritePolicy write = default, FilePhase phase = FilePhase.Export) =>
        phase switch {
            FilePhase.Export or FilePhase.Publish => Create(
                mode: FilePromptMode.Save,
                title: title,
                filter: string.IsNullOrWhiteSpace(value: filter) ? FileFormat.Filter(phase: phase) : filter,
                fileName: fileName,
                initialDirectory: initialDirectory,
                defaultExtension: defaultExtension,
                write: write),
            _ => Fin.Fail<FilePrompt>(error: Op.Of(name: nameof(FilePrompt)).InvalidInput()),
        };

    public static Fin<FilePrompt> Folder(string title, Option<string> initialDirectory = default) =>
        Create(mode: FilePromptMode.Folder, title: title, filter: string.Empty, initialDirectory: initialDirectory);

    internal Fin<Seq<FileEndpoint>> Defaults(Op op) =>
        DefaultPath().Case switch {
            string value => FileEndpoint.From(path: value, name: Name, write: Write).Map(endpoint => Seq(endpoint)),
            _ => Fin.Fail<Seq<FileEndpoint>>(error: op.InvalidInput()),
        };

    private static Fin<FilePrompt> Create(
        FilePromptMode mode,
        string title,
        string filter,
        Option<string> fileName = default,
        Option<string> initialDirectory = default,
        Option<string> defaultExtension = default,
        FileNamePolicy name = default,
        FileWritePolicy write = default) =>
        from validTitle in FileEndpoint.NonBlank(value: title, op: Op.Of(name: nameof(FilePrompt)))
        from validFileName in TextOption(value: fileName, op: Op.Of(name: nameof(FilePrompt)))
        from validDirectory in TextOption(value: initialDirectory, op: Op.Of(name: nameof(FilePrompt)))
        from validExtension in TextOption(value: defaultExtension, op: Op.Of(name: nameof(FilePrompt)))
        select new FilePrompt(
            mode: mode,
            title: validTitle,
            filter: filter ?? string.Empty,
            fileName: validFileName,
            initialDirectory: validDirectory,
            defaultExtension: validExtension,
            name: name,
            write: write);

    private static Fin<Option<string>> TextOption(Option<string> value, Op op) =>
        value.Case switch {
            string text => op.AcceptText(value: text).Map(Some).MapFail(_ => op.InvalidInput()),
            _ => Fin.Succ(Option<string>.None),
        };

    private Option<string> DefaultPath() =>
        (Mode, FileName.Case, InitialDirectory.Case) switch {
            (FilePromptMode.Folder, _, string directory) => Some(directory),
            (_, string fileName, string directory) when !IOPath.IsPathRooted(path: fileName) => Some(IOPath.Combine(path1: directory, path2: fileName)),
            (_, string fileName, _) => Some(fileName),
            _ => Option<string>.None,
        };

    private static FileNamePolicy PromptName(FileNamePolicy name, Option<string> defaultExtension) =>
        name.Normalized.Extension.Case switch {
            string => name.Normalized,
            _ => defaultExtension.Case switch {
                string value => name.Normalized with { Extension = Some(value) },
                _ => name.Normalized,
            },
        };
}

public sealed record FileEndpoint {
    private FileEndpoint(string path, Option<FileFormat> format, FileNamePolicy name, FileWritePolicy write) =>
        (Path, Format, Name, Write) = (path, format, name.Normalized, write.Normalized);

    public string Path { get; }
    public Option<FileFormat> Format { get; }
    public FileNamePolicy Name { get; }
    public FileWritePolicy Write { get; }

    public static Fin<FileEndpoint> From(string path, Option<FileFormat> format = default, FileNamePolicy name = default, FileWritePolicy write = default) =>
        from raw in NonBlank(value: path, op: Op.Of(name: nameof(FileEndpoint)))
        from normalized in NormalizePath(path: raw, op: Op.Of(name: nameof(FileEndpoint)))
        let named = ApplyExtension(path: normalized, format: Option<FileFormat>.None, name: name)
        let detected = format.Case switch {
            FileFormat known => Some(known),
            _ => FileFormat.Detect(path: named),
        }
        select new FileEndpoint(path: ApplyExtension(path: named, format: detected, name: FileNamePolicy.Default), format: detected, name: name, write: write);

    internal FileEndpoint WithPath(string path) =>
        new(path: path, format: Format, name: Name, write: Write);

    internal FileEndpoint WithFormat(FileFormat format) =>
        Optional(format).Case switch {
            FileFormat known => new(path: known.EnsureExtension(path: Path), format: Some(known), name: Name with { Extension = Some(known.Extensions[0]) }, write: Write),
            _ => this,
        };

    internal Fin<FileEndpoint> Input(Op op) =>
        IOPath.Exists(path: Path) switch {
            true => Fin.Succ(value: this),
            false => Fin.Fail<FileEndpoint>(error: op.InvalidInput()),
        };

    internal Fin<FileEndpoint> Output(Op op) =>
        from endpoint in Fin.Succ(value: WithPath(path: ApplyExtension(path: Path, format: Format, name: Name)))
        from _ in endpoint.EnsureDirectory(op: op)
        from resolved in endpoint.ResolveCollision(op: op)
        select resolved;

    internal Fin<FileEndpoint> Folder(Op op) =>
        Try.lift<Fin<FileEndpoint>>(f: () => {
            _ = IODirectory.CreateDirectory(path: Path);
            return IODirectory.Exists(path: Path) switch {
                true => Fin.Succ(value: this),
                false => Fin.Fail<FileEndpoint>(error: op.InvalidResult()),
            };
        })
            .Run()
            .MapFail(_ => op.InvalidResult())
            .Flatten();

    internal static Fin<string> NonBlank(string value, Op op) =>
        op.AcceptText(value: value).MapFail(_ => op.InvalidInput());

    private Fin<Unit> EnsureDirectory(Op op) =>
        Try.lift<Fin<Unit>>(f: () => {
            string? directory = IOPath.GetDirectoryName(path: Path);
            return directory switch {
                string value when string.IsNullOrWhiteSpace(value: value) => Fin.Succ(value: unit),
                string value => Write.Normalized.Directory switch {
                    FileDirectoryPolicy.Create => Fin.Succ(value: ((Func<Unit>)(() => { _ = IODirectory.CreateDirectory(path: value); return unit; }))()),
                    FileDirectoryPolicy.Existing when IODirectory.Exists(path: value) => Fin.Succ(value: unit),
                    _ => Fin.Fail<Unit>(error: op.InvalidInput()),
                },
                _ => Fin.Fail<Unit>(error: op.InvalidInput()),
            };
        })
            .Run()
            .MapFail(_ => op.InvalidInput())
            .Flatten();

    private Fin<FileEndpoint> ResolveCollision(Op op) =>
        (Name.Normalized.Collision, Write.Normalized.Overwrite, Exists(path: Path)) switch {
            (_, FileOverwritePolicy.Replace, _) => Fin.Succ(value: this),
            (FileCollisionPolicy.Replace, _, _) => Fin.Succ(value: this),
            (FileCollisionPolicy.AppendNumber, _, true) => NextAvailable(op: op),
            (_, _, false) => Fin.Succ(value: this),
            _ => Fin.Fail<FileEndpoint>(error: op.InvalidInput()),
        };

    private Fin<FileEndpoint> NextAvailable(Op op) =>
        toSeq(Enumerable.Range(start: 1, count: 9999))
            .Map(index => WithPath(path: NumberedPath(path: Path, index: index)))
            .Find(static endpoint => !Exists(path: endpoint.Path))
            .ToFin(Fail: op.InvalidInput());

    private static Fin<string> NormalizePath(string path, Op op) =>
        Try.lift(f: () => IOPath.GetFullPath(path: path))
            .Run()
            .MapFail(_ => op.InvalidInput());

    private static string ApplyExtension(string path, Option<FileFormat> format, FileNamePolicy name) =>
        name.Normalized.Extension.Case switch {
            string extension => ExtensionPath(path: path, extension: extension),
            _ => format.Case switch {
                FileFormat known => known.EnsureExtension(path: path),
                _ => path,
            },
        };

    private static string ExtensionPath(string path, string extension) =>
        string.IsNullOrWhiteSpace(value: extension) switch {
            false => IOPath.ChangeExtension(path: path, extension: extension.TrimStart('.')),
            true => path,
        };

    private static bool Exists(string path) =>
        IOPath.Exists(path: path);

    private static string NumberedPath(string path, int index) =>
        IOPath.Combine(
            path1: IOPath.GetDirectoryName(path: path) ?? string.Empty,
            path2: $"{IOPath.GetFileNameWithoutExtension(path: path)}-{index:000}{IOPath.GetExtension(path: path)}");
}

public sealed record FileProfile {
    private FileProfile(FileFidelity fidelity, FileResourcePolicy resources, FileGrouping grouping, FileSort sort, Option<FileFormat> format) =>
        (Fidelity, Resources, Grouping, Sort, Format) = (fidelity, resources, grouping, sort, format);

    public FileFidelity Fidelity { get; }
    public FileResourcePolicy Resources { get; }
    public FileGrouping Grouping { get; }
    public FileSort Sort { get; }
    public Option<FileFormat> Format { get; }

    public static FileProfile Model { get; } = new(fidelity: FileFidelity.Model, resources: FileResourcePolicy.Reference, grouping: FileGrouping.Document, sort: FileSort.Stable, format: Option<FileFormat>.None);

    public FileProfile With(FileFidelity? fidelity = null, FileResourcePolicy? resources = null, FileGrouping? grouping = null, FileSort? sort = null, Option<FileFormat> format = default) =>
        new(
            fidelity: fidelity ?? Fidelity,
            resources: resources ?? Resources,
            grouping: grouping ?? Grouping,
            sort: sort ?? Sort,
            format: format.Case switch {
                FileFormat value => Some(value),
                _ => Format,
            });
}

public sealed record ArchiveProfile(ArchiveSlice Slice, FileArchiveProjection Projection, FileWritePolicy Write, Seq<string> Embedded = default) {
    public static ArchiveProfile Full { get; } = new(Slice: ArchiveSlice.Full, Projection: FileArchiveProjection.Full, Write: FileWritePolicy.Default);

    internal bool Includes(string file) =>
        Embedded.IsEmpty || Embedded.Exists(name =>
            string.Equals(a: name, b: file, comparisonType: StringComparison.OrdinalIgnoreCase)
            || string.Equals(a: name, b: IOPath.GetFileName(path: file), comparisonType: StringComparison.OrdinalIgnoreCase));
}

public sealed record ArchiveUpdate(
    Option<FileArchiveMetadataPatch> Metadata = default,
    Seq<FileEndpoint> Embed = default,
    Seq<string> Extract = default,
    Seq<FileEndpoint> LinkBlocks = default);

public sealed record FilePublish(FilePublishTarget Target, Seq<FileSheet> Sheets, FileProfile Profile, bool Layers = true);

public sealed record FileSheet(
    Option<Guid> Id = default,
    Option<string> Name = default,
    Option<string> Group = default,
    double Dpi = 300.0,
    bool PrintWidths = true,
    bool Raster = false,
    ViewCaptureSettings.ColorMode Color = ViewCaptureSettings.ColorMode.DisplayColor) {
    internal Fin<ViewCaptureSettings> Settings(RhinoPageView page, Op op) =>
        from active in Optional(page).ToFin(Fail: op.InvalidInput())
        from dpi in (double.IsFinite(d: Dpi), Dpi) switch {
            (true, > 0.0) => Fin.Succ(value: Dpi),
            _ => Fin.Fail<double>(error: op.InvalidInput()),
        }
        select new ViewCaptureSettings(sourcePageView: active, dpi: dpi) {
            UsePrintWidths = PrintWidths,
            RasterMode = Raster,
            OutputColor = Color,
        };
}

internal readonly record struct FileSheetPage(RhinoPageView Page, FileSheet Sheet);
internal readonly record struct FilePublishResult(Option<FileEndpoint> Target, Option<FileFormat> Format, DocumentReceipt Receipt, Seq<FileIssue> Issues = default, Option<string> NativeLog = default);

public readonly record struct FileArchiveMetadataPatch(
    Option<string> Notes,
    Option<string> ApplicationName,
    Option<string> ApplicationUrl,
    Option<string> ApplicationDetails);

public readonly record struct FileObjectManifest(
    Guid Id,
    Option<string> Name,
    Option<string> Layer,
    ObjectType ObjectType,
    Option<string> Material,
    Seq<string> UserStrings);

public sealed record FileReport(
    Option<FileEndpoint> Source,
    Option<FileEndpoint> Target,
    Option<FileFormat> Format,
    FilePhase Phase,
    Option<DocumentReceipt> Receipt,
    Seq<FileIssue> Issues,
    Option<string> NativeLog,
    Option<FileArchive> Archive = default,
    Seq<FileReport> Children = default) {
    public static FileReport Empty(FilePhase phase) =>
        new(Source: Option<FileEndpoint>.None, Target: Option<FileEndpoint>.None, Format: Option<FileFormat>.None, Phase: phase, Receipt: Option<DocumentReceipt>.None, Issues: Seq<FileIssue>(), NativeLog: Option<string>.None, Children: Seq<FileReport>());

    internal static FileReport Of(FilePhase phase, Option<FileEndpoint> source = default, Option<FileEndpoint> target = default, Option<FileFormat> format = default, Option<DocumentReceipt> receipt = default, Seq<FileIssue> issues = default, Option<string> nativeLog = default, Option<FileArchive> archive = default, Seq<FileReport> children = default) =>
        new(Source: source, Target: target, Format: format, Phase: phase, Receipt: receipt, Issues: issues.IsEmpty ? Seq<FileIssue>() : issues, NativeLog: nativeLog, Archive: archive, Children: children.IsEmpty ? Seq<FileReport>() : children);
}
