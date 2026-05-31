using System.Globalization;
using System.Runtime.InteropServices;
using Rasm.Rhino.Commands;
using Rhino.FileIO;
using IODirectory = System.IO.Directory;
using IOFile = System.IO.File;
using IOPath = System.IO.Path;
using ObjectTypeFilter = Rhino.FileIO.File3dm.ObjectTypeFilter;
using TableTypeFilter = Rhino.FileIO.File3dm.TableTypeFilter;

namespace Rasm.Rhino.Exchange;

// --- [TYPES] ------------------------------------------------------------------------------
public enum FilePromptMode { OpenSingle, OpenMultiple, Save, Folder }
public enum FileOverwritePolicy { Fail, Replace }
public enum FileDirectoryPolicy { Create, Existing }
public enum FileCollisionPolicy { Preserve, AppendNumber, Replace }
public enum FileFidelity { Model, Small, GeometryOnly }
public enum FileResourcePolicy { Reference, Embed, Copy }
public enum FileAxis { Stable, Document, File, Layer, ObjectName, ObjectType, Material, Block, UserString }
public enum FileTiffCompression { Default, None, Lzw, Ccitt3, Ccitt4, Rle }

[Flags]
public enum FileArchiveProjection {
    None = 0,
    Metadata = 1,
    Graph = 2,
    Objects = 4,
    MetadataAndGraph = Metadata | Graph,
    Full = Metadata | Graph | Objects,
}

[SmartEnum<int>]
public sealed partial class FilePhase {
    public static readonly FilePhase Prompt = new(key: 0, requires: FileCapability.None);
    public static readonly FilePhase Open = new(key: 1, requires: FileCapability.Archive);
    public static readonly FilePhase Headless = new(key: 2, requires: FileCapability.Archive | FileCapability.Import);
    public static readonly FilePhase Import = new(key: 3, requires: FileCapability.Import);
    public static readonly FilePhase Export = new(key: 4, requires: FileCapability.Export);
    public static readonly FilePhase Publish = new(key: 5, requires: FileCapability.Publish);
    public static readonly FilePhase NamedLayerState = new(key: 6, requires: FileCapability.None);
    public static readonly FilePhase Save = new(key: 7, requires: FileCapability.None);
    public static readonly FilePhase SaveAs = new(key: 8, requires: FileCapability.Archive);
    public static readonly FilePhase WriteFile = new(key: 9, requires: FileCapability.Export);
    public static readonly FilePhase Write3dmFile = new(key: 10, requires: FileCapability.Archive);
    public static readonly FilePhase SaveTemplate = new(key: 11, requires: FileCapability.Archive);
    public static readonly FilePhase ArchiveRead = new(key: 12, requires: FileCapability.None);
    public static readonly FilePhase ArchiveExtract = new(key: 13, requires: FileCapability.None);
    public static readonly FilePhase ArchiveUpdate = new(key: 14, requires: FileCapability.None);
    public static readonly FilePhase ArchiveInspect = new(key: 15, requires: FileCapability.None);
    public static readonly FilePhase ArchiveValidate = new(key: 16, requires: FileCapability.None);
    public static readonly FilePhase Batch = new(key: 17, requires: FileCapability.None);
    public static readonly FilePhase SheetEdit = new(key: 18, requires: FileCapability.None);
    public static readonly FilePhase NamedPosition = new(key: 19, requires: FileCapability.None);

    public FileCapability Requires { get; }

    internal bool Allows(FileCapability capability) =>
        Requires == FileCapability.None || (Requires & capability) != FileCapability.None;
}

[SmartEnum<int>]
public sealed partial class ArchiveSlice {
    public static readonly ArchiveSlice Full = new(key: 0, tables: TableTypeFilter.None, objectFilter: ObjectTypeFilter.None, filtered: false);
    public static readonly ArchiveSlice Metadata = new(key: 1,
        tables: TableTypeFilter.Properties | TableTypeFilter.Settings,
        objectFilter: ObjectTypeFilter.None,
        filtered: true);
    public static readonly ArchiveSlice Objects = new(key: 2,
        tables: TableTypeFilter.ObjectTable | TableTypeFilter.Layer | TableTypeFilter.Material,
        objectFilter: ObjectTypeFilter.Any,
        filtered: true);
    public static readonly ArchiveSlice Resources = new(key: 3,
        tables: TableTypeFilter.Properties | TableTypeFilter.Settings | TableTypeFilter.Bitmap | TableTypeFilter.Font
              | TableTypeFilter.FutureFont | TableTypeFilter.Light | TableTypeFilter.Historyrecord
              | TableTypeFilter.TextureMapping | TableTypeFilter.Material | TableTypeFilter.Linetype
              | TableTypeFilter.Layer | TableTypeFilter.Group | TableTypeFilter.Dimstyle
              | TableTypeFilter.Hatchpattern | TableTypeFilter.InstanceDefinition | TableTypeFilter.ObjectTable
              | TableTypeFilter.UserTable,
        objectFilter: ObjectTypeFilter.Any,
        filtered: true);

    public TableTypeFilter Tables { get; }
    public ObjectTypeFilter ObjectFilter { get; }
    public bool Filtered { get; }
}

[SmartEnum<string>]
public sealed partial class FileIssueCode {
    public static readonly FileIssueCode Native = new(key: "native");
    public static readonly FileIssueCode AlreadyOpen = new(key: "already-open");
    public static readonly FileIssueCode BatchFailure = new(key: "batch-failure");
    public static readonly FileIssueCode EmptyArchive = new(key: "empty-archive");
    public static readonly FileIssueCode BrokenLink = new(key: "broken-link");
}

[SmartEnum<int>]
public sealed partial class FileVectorUnit {
    public static readonly FileVectorUnit Inches = new(
        key: 0,
        pdf: FilePdfReadOptions.PDF_UNITS.inches,
        aiRead: FileAiReadOptions.Units.Inches,
        aiWrite: FileAiWriteOptions.Units.Inches,
        eps: FileEpsReadOptions.Units.Inches);
    public static readonly FileVectorUnit Centimeters = new(
        key: 1,
        pdf: FilePdfReadOptions.PDF_UNITS.centimeters,
        aiRead: FileAiReadOptions.Units.Centimeters,
        aiWrite: FileAiWriteOptions.Units.Centimeters,
        eps: FileEpsReadOptions.Units.Centimeters);
    public static readonly FileVectorUnit Millimeters = new(
        key: 2,
        pdf: FilePdfReadOptions.PDF_UNITS.millimeters,
        aiRead: FileAiReadOptions.Units.Millimeters,
        aiWrite: FileAiWriteOptions.Units.Millimeters,
        eps: FileEpsReadOptions.Units.Millimeters);
    public static readonly FileVectorUnit Points = new(
        key: 3,
        pdf: FilePdfReadOptions.PDF_UNITS.points,
        aiRead: FileAiReadOptions.Units.Points,
        aiWrite: FileAiWriteOptions.Units.Points,
        eps: FileEpsReadOptions.Units.Points);

    internal FilePdfReadOptions.PDF_UNITS Pdf { get; }
    internal FileAiReadOptions.Units AiRead { get; }
    internal FileAiWriteOptions.Units AiWrite { get; }
    internal FileEpsReadOptions.Units Eps { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct FileNamePolicy(FileCollisionPolicy Collision = FileCollisionPolicy.Preserve, Option<string> Extension = default) {
    public static FileNamePolicy Default => default;
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

public readonly record struct FileIssue(FileIssueCode Code, string Message) {
    public static FileIssue Native(string message) =>
        new(Code: FileIssueCode.Native, Message: string.IsNullOrWhiteSpace(value: message) ? "No native log." : message);

    public static FileIssue Of(FileIssueCode code, string message) {
        ArgumentNullException.ThrowIfNull(argument: code);
        return new(Code: code, Message: string.IsNullOrWhiteSpace(value: message) ? code.Key : message);
    }
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
        Create(mode: multiple ? FilePromptMode.OpenMultiple : FilePromptMode.OpenSingle, title: title, filter: string.IsNullOrWhiteSpace(value: filter) ? FileFormat.Filter(phase: FilePhase.Import) : filter, fileName: fileName, initialDirectory: initialDirectory, defaultExtension: defaultExtension);

    public static Fin<FilePrompt> Save(string title, string filter = "", Option<string> fileName = default, Option<string> initialDirectory = default, Option<string> defaultExtension = default, FileWritePolicy write = default, FilePhase? phase = null) {
        FilePhase active = phase ?? FilePhase.Export;
        return active == FilePhase.Export || active == FilePhase.Publish
            ? Create(
                mode: FilePromptMode.Save,
                title: title,
                filter: string.IsNullOrWhiteSpace(value: filter) ? FileFormat.Filter(phase: active) : filter,
                fileName: fileName,
                initialDirectory: initialDirectory,
                defaultExtension: defaultExtension,
                write: write)
            : Fin.Fail<FilePrompt>(error: Op.Of(name: nameof(FilePrompt)).InvalidInput());
    }

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
        name.Extension.Case switch {
            string => name,
            _ => defaultExtension.Case switch {
                string value => name with { Extension = Some(value) },
                _ => name,
            },
        };
}

public sealed record FileEndpoint {
    private FileEndpoint(string path, Option<FileFormat> format, FileNamePolicy name, FileWritePolicy write, Option<string> relative) =>
        (Path, Format, Name, Write, Relative) = (path, format, name, write.Normalized, relative);

    public string Path { get; }
    public Option<FileFormat> Format { get; }
    public FileNamePolicy Name { get; }
    public FileWritePolicy Write { get; }
    public Option<string> Relative { get; }
    public string StoredLinkPath => Relative.IfNone(noneValue: Path);

    public static Fin<FileEndpoint> From(
        string path,
        Option<FileFormat> format = default,
        FileNamePolicy name = default,
        FileWritePolicy write = default,
        Option<string> relative = default) =>
        from raw in NonBlank(value: path, op: Op.Of(name: nameof(FileEndpoint)))
        from normalized in NormalizePath(path: raw, op: Op.Of(name: nameof(FileEndpoint)))
        from rel in RelativeOption(value: relative, op: Op.Of(name: nameof(FileEndpoint)))
        let named = ApplyExtension(path: normalized, format: Option<FileFormat>.None, name: name)
        let detected = format.Case switch {
            FileFormat known => Some(known),
            _ => FileFormat.Detect(path: named),
        }
        select new FileEndpoint(
            path: ApplyExtension(path: named, format: detected, name: FileNamePolicy.Default),
            format: detected,
            name: name,
            write: write,
            relative: rel);

    internal FileEndpoint WithPath(string path) =>
        new(path: path, format: Format, name: Name, write: Write, relative: Relative);

    internal FileEndpoint WithFormat(FileFormat format) =>
        Optional(format).Case switch {
            FileFormat known => new(
                path: known.EnsureExtension(path: Path),
                format: Some(known),
                name: Name with { Extension = Some(known.Extensions[0]) },
                write: Write,
                relative: Relative),
            _ => this,
        };

    internal FileEndpoint WithRelative(Option<string> relative) =>
        new(path: Path, format: Format, name: Name, write: Write, relative: relative);

    internal Fin<T> WithReference<T>(Op key, Func<FileReference, Fin<T>> use) =>
        key.Catch(() => {
            // BOUNDARY ADAPTER — FileReference owns native metadata; materialize callback Fin before disposal.
            using FileReference? reference = Relative.Case switch {
                string relative => FileReference.CreateFromFullAndRelativePaths(fullPath: Path, relativePath: relative),
                _ => FileReference.CreateFromFullPath(fullPath: Path),
            };
            return Optional(reference).ToFin(Fail: key.InvalidInput())
                .Bind(valid => use(arg: valid));
        });

    private static Fin<Option<string>> RelativeOption(Option<string> value, Op op) =>
        value.Case switch {
            string text => op.AcceptText(value: text).Map(Some).MapFail(_ => op.InvalidInput()),
            _ => Fin.Succ(Option<string>.None),
        };

    internal Fin<FileEndpoint> Input(Op op) =>
        IOFile.Exists(path: Path) switch {
            true => Fin.Succ(value: this),
            false => Fin.Fail<FileEndpoint>(error: op.InvalidInput()),
        };

    internal Fin<FileEndpoint> Output(Op op) =>
        from endpoint in Fin.Succ(value: WithPath(path: ApplyExtension(path: Path, format: Format, name: Name)))
        from _ in endpoint.EnsureDirectory(op: op)
        from resolved in endpoint.ResolveCollision(op: op)
        select resolved;

    internal Fin<FileEndpoint> Folder(Op op) {
        FileEndpoint self = this;
        return op.Catch(() => {
            _ = IODirectory.CreateDirectory(path: self.Path);
            return IODirectory.Exists(path: self.Path) switch {
                true => Fin.Succ(value: self),
                false => Fin.Fail<FileEndpoint>(error: op.InvalidResult()),
            };
        });
    }

    internal static Fin<string> NonBlank(string value, Op op) =>
        op.AcceptText(value: value).MapFail(_ => op.InvalidInput());

    private Fin<Unit> EnsureDirectory(Op op) {
        FileEndpoint self = this;
        return op.Catch(() => {
            string? directory = IOPath.GetDirectoryName(path: self.Path);
            return directory switch {
                string value when string.IsNullOrWhiteSpace(value: value) => Fin.Succ(value: unit),
                string value => self.Write.Normalized.Directory switch {
                    FileDirectoryPolicy.Create => Fin.Succ(value: Op.Side(() => IODirectory.CreateDirectory(path: value))),
                    FileDirectoryPolicy.Existing when IODirectory.Exists(path: value) => Fin.Succ(value: unit),
                    _ => Fin.Fail<Unit>(error: op.InvalidInput()),
                },
                _ => Fin.Fail<Unit>(error: op.InvalidInput()),
            };
        });
    }

    private Fin<FileEndpoint> ResolveCollision(Op op) =>
        (Name.Collision, Write.Normalized.Overwrite, IOPath.Exists(path: Path)) switch {
            (_, FileOverwritePolicy.Replace, _) => Fin.Succ(value: this),
            (FileCollisionPolicy.Replace, _, _) => Fin.Succ(value: this),
            (FileCollisionPolicy.AppendNumber, _, true) => NextAvailable(op: op),
            (_, _, false) => Fin.Succ(value: this),
            _ => Fin.Fail<FileEndpoint>(error: op.InvalidInput()),
        };

    private Fin<FileEndpoint> NextAvailable(Op op) =>
        toSeq(Enumerable.Range(start: 1, count: MaxCollisionAttempts))
            .Map(index => WithPath(path: NumberedPath(path: Path, index: index)))
            .Find(static endpoint => !IOPath.Exists(path: endpoint.Path))
            .ToFin(Fail: op.InvalidInput());

    private static Fin<string> NormalizePath(string path, Op op) =>
        op.Catch(() => Fin.Succ(value: IOPath.GetFullPath(path: path)));

    private static string ApplyExtension(string path, Option<FileFormat> format, FileNamePolicy name) =>
        name.Extension.Case switch {
            string extension => string.IsNullOrWhiteSpace(value: extension) switch {
                false => IOPath.ChangeExtension(path: path, extension: extension.TrimStart('.')),
                true => path,
            },
            _ => format.Case switch {
                FileFormat known => known.EnsureExtension(path: path),
                _ => path,
            },
        };

    internal static string NumberedPath(string path, int index) =>
        IOPath.Combine(
            path1: IOPath.GetDirectoryName(path: path) ?? string.Empty,
            path2: string.Create(CultureInfo.InvariantCulture, $"{IOPath.GetFileNameWithoutExtension(path: path)}-{index:000}{IOPath.GetExtension(path: path)}"));

    private const int MaxCollisionAttempts = 9999;
}

public sealed record FileProfile {
    private FileProfile(FileFidelity fidelity, FileResourcePolicy resources, FileAxis group, FileAxis order, Option<FileFormat> format, Option<FileVectorScale> scale) =>
        (Fidelity, Resources, Group, Order, Format, Scale) = (fidelity, resources, group, order, format, scale);

    public FileFidelity Fidelity { get; }
    public FileResourcePolicy Resources { get; }
    public FileAxis Group { get; }
    public FileAxis Order { get; }
    public Option<FileFormat> Format { get; }
    public Option<FileVectorScale> Scale { get; }

    public static FileProfile Model { get; } = new(fidelity: FileFidelity.Model, resources: FileResourcePolicy.Reference, group: FileAxis.Document, order: FileAxis.Stable, format: Option<FileFormat>.None, scale: Option<FileVectorScale>.None);

    public FileProfile With(FileFidelity? fidelity = null, FileResourcePolicy? resources = null, FileAxis? group = null, FileAxis? order = null, FileOverride<FileFormat> format = default, FileOverride<FileVectorScale> scale = default) =>
        new(
            fidelity: fidelity ?? Fidelity,
            resources: resources ?? Resources,
            group: group ?? Group,
            order: order ?? Order,
            format: format.Patch(current: Format),
            scale: scale.Patch(current: Scale));

    internal Fin<Unit> Validate(FilePhase phase, Op op) =>
        from _group in Group == FileAxis.Stable ? Fin.Fail<Unit>(error: op.InvalidInput()) : Fin.Succ(value: unit)
        from _order in Order == FileAxis.Document ? Fin.Fail<Unit>(error: op.InvalidInput()) : Fin.Succ(value: unit)
        from _scale in phase == FilePhase.Import || phase == FilePhase.Headless || phase == FilePhase.Export || phase == FilePhase.WriteFile
            ? Scale.Map(value => value.Validate(op: op).Map(static _ => unit)).IfNone(Fin.Succ(value: unit))
            : Fin.Succ(value: unit)
        select unit;
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct FileVectorScale(
    Option<FileVectorUnit> Units = default,
    Option<double> Source = default,
    Option<double> Rhino = default,
    Option<bool> Preserve = default) {
    internal Fin<FileVectorScale> Validate(Op op) {
        FileVectorScale self = this;
        return from mode in guard(!(self.Preserve.Case is true && self.HasExplicit), op.InvalidInput())
               from source in self.Source.Map(value => RhinoMath.IsValidDouble(x: value) && value > 0.0 ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: op.InvalidInput())).IfNone(Fin.Succ(value: unit))
               from rhino in self.Rhino.Map(value => RhinoMath.IsValidDouble(x: value) && value > 0.0 ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: op.InvalidInput())).IfNone(Fin.Succ(value: unit))
               select self;
    }

    internal FilePdfReadOptions Apply(FilePdfReadOptions options) =>
        options
            .Set(PreserveMode, static (o, v) => o.PreserveModelScale = v)
            .Set(Rhino, static (o, v) => o.RhinoScale = v)
            .Set(Source, static (o, v) => o.PDFScale = v)
            .Set(Units, static u => u.Pdf, static (o, v) => o.PdfUnits = v);

    internal FileAiReadOptions Apply(FileAiReadOptions options) =>
        options
            .Set(PreserveMode, static (o, v) => o.PreserveModelScale = v)
            .Set(Rhino, static (o, v) => o.RhinoScale = v)
            .Set(Source, static (o, v) => o.AiScale = v)
            .Set(Units, static u => u.AiRead, static (o, v) => o.AiUnits = v);

    internal FileAiWriteOptions Apply(FileAiWriteOptions options) =>
        options
            .Set(PreserveMode, static (o, v) => o.PreserveModelScale = v)
            .Set(Rhino, static (o, v) => o.RhinoScale = v)
            .Set(Source, static (o, v) => o.AIScale = v)
            .Set(Units, static u => u.AiWrite, static (o, v) => o.AiUnits = v);

    internal FileEpsReadOptions Apply(FileEpsReadOptions options) =>
        options
            .Set(PreserveMode, static (o, v) => o.PreserveModelScale = v)
            .Set(Rhino, static (o, v) => o.RhinoScale = v)
            .Set(Source, static (o, v) => o.EpsScale = v)
            .Set(Units, static u => u.Eps, static (o, v) => o.EpsUnits = v);

    private bool HasExplicit => Source.IsSome || Rhino.IsSome || Units.IsSome;
    private Option<bool> PreserveMode => Preserve | (HasExplicit ? Some(false) : Option<bool>.None);
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
    Seq<string> Extract = default);

[StructLayout(LayoutKind.Auto)]
public readonly record struct FileOverride<T>(Option<T> Value = default, bool Inherit = false) {
    public static FileOverride<T> operator |(FileOverride<T> left, FileOverride<T> right) =>
        right.IsActive ? right : left;

    internal Unit Apply(Action<T> set, Action inherit) =>
        (Inherit, Value.Case) switch {
            (true, _) => Op.Side(inherit),
            (_, T value) => Op.Side(() => set(obj: value)),
            _ => unit,
        };

    internal Option<T> Patch(Option<T> current) =>
        (Inherit, Value.Case) switch {
            (true, _) => Option<T>.None,
            (_, T value) => Some(value),
            _ => current,
        };

    private bool IsActive => Inherit || Value.IsSome;
}

public static class FileOverride {
    public static FileOverride<T> Keep<T>() => default;
    public static FileOverride<T> Set<T>(T value) => new(Value: Some(value));
    public static FileOverride<T> Clear<T>() => new(Inherit: true);
}

// `Value None` deletes the key; `Section None` targets the flat (unsectioned) document-string table.
public readonly record struct FileUserString(string Key, Option<string> Section = default, Option<string> Value = default);

public readonly record struct FileArchiveMetadataPatch(
    Option<string> Notes,
    Option<string> ApplicationName,
    Option<string> ApplicationUrl,
    Option<string> ApplicationDetails,
    Option<string> StartComments = default,
    Seq<FileUserString> UserStrings = default);

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
    Seq<FileReport> Children = default,
    Seq<FileViewReport> Views = default) {
    public static FileReport Empty(FilePhase phase) =>
        new(Source: Option<FileEndpoint>.None, Target: Option<FileEndpoint>.None, Format: Option<FileFormat>.None, Phase: phase, Receipt: Option<DocumentReceipt>.None, Issues: Seq<FileIssue>(), NativeLog: Option<string>.None, Children: Seq<FileReport>(), Views: Seq<FileViewReport>());

    internal static FileReport Of(FilePhase phase, Option<FileEndpoint> source = default, Option<FileEndpoint> target = default, Option<FileFormat> format = default, Option<DocumentReceipt> receipt = default, Seq<FileIssue> issues = default, Option<string> nativeLog = default, Option<FileArchive> archive = default, Seq<FileReport> children = default, Seq<FileViewReport> views = default) =>
        new(Source: source, Target: target, Format: format, Phase: phase, Receipt: receipt, Issues: issues.IsEmpty ? Seq<FileIssue>() : issues, NativeLog: nativeLog, Archive: archive, Children: children.IsEmpty ? Seq<FileReport>() : children, Views: views.IsEmpty ? Seq<FileViewReport>() : views);
}

// --- [COMPOSITION] ------------------------------------------------------------------------
internal static class OptionsProjection {
    internal static TOptions Set<TOptions, T>(this TOptions options, Option<T> source, Action<TOptions, T> setter) where TOptions : class {
        _ = source.Iter(value => setter(arg1: options, arg2: value));
        return options;
    }

    internal static TOptions Set<TOptions, T, TProjected>(this TOptions options, Option<T> source, Func<T, TProjected> project, Action<TOptions, TProjected> setter) where TOptions : class {
        _ = source.Iter(value => setter(arg1: options, arg2: project(arg: value)));
        return options;
    }
}
