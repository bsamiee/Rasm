using System.Drawing.Imaging;
using System.Globalization;
using System.Text.RegularExpressions;
using Rasm.Rhino.Commands;
using DrawingImageFormat = System.Drawing.Imaging.ImageFormat;
using IODirectory = System.IO.Directory;
using IOPath = System.IO.Path;
using ObjectTypeFilter = Rhino.FileIO.File3dm.ObjectTypeFilter;
using TableTypeFilter = Rhino.FileIO.File3dm.TableTypeFilter;

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
    public static readonly FileIssueCode WatchFailure = new(key: "watch-failure");
    public static readonly FileIssueCode BrokenLink = new(key: "broken-link");
}

[SmartEnum<int>]
public sealed partial class FileRasterEncoding {
    public static readonly FileRasterEncoding Png = new(
        key: 0,
        format: () => FileFormat.Png,
        image: () => DrawingImageFormat.Png,
        compression: FileTiffCompression.Default,
        encode: static (_, settings) => settings.PngDepth.Map(depth => Seq<(Encoder, long)>((Encoder.ColorDepth, depth))).IfNone(Seq<(Encoder, long)>()));
    public static readonly FileRasterEncoding Jpeg = new(
        key: 1,
        format: () => FileFormat.Jpeg,
        image: () => DrawingImageFormat.Jpeg,
        compression: FileTiffCompression.Default,
        encode: static (_, settings) => Seq<(Encoder, long)>((Encoder.Quality, settings.JpegQuality)));
    public static readonly FileRasterEncoding Tiff = new(
        key: 2,
        format: () => FileFormat.Tiff,
        image: () => DrawingImageFormat.Tiff,
        compression: FileTiffCompression.Lzw,
        encode: static (encoding, settings) => Seq<(Encoder, long)>((Encoder.Compression, (long)(settings.TiffCompression.IfNone(noneValue: encoding.Compression) switch {
            FileTiffCompression.None => EncoderValue.CompressionNone,
            FileTiffCompression.Ccitt3 => EncoderValue.CompressionCCITT3,
            FileTiffCompression.Ccitt4 => EncoderValue.CompressionCCITT4,
            FileTiffCompression.Rle => EncoderValue.CompressionRle,
            _ => EncoderValue.CompressionLZW,
        }))));
    public static readonly FileRasterEncoding Bitmap = new(
        key: 3,
        format: () => FileFormat.Bmp,
        image: () => DrawingImageFormat.Bmp,
        compression: FileTiffCompression.Default,
        encode: static (_, _) => Seq<(Encoder, long)>());

    private readonly Func<FileFormat> format;
    private readonly Func<DrawingImageFormat> image;
    private readonly Func<FileRasterEncoding, FileRasterSettings, Seq<(Encoder Encoder, long Value)>> encode;

    public FileFormat Format => format();
    public DrawingImageFormat Image => image();
    public FileTiffCompression Compression { get; }

    internal Seq<(Encoder Encoder, long Value)> Parameters(FileRasterSettings settings) => encode(arg1: this, arg2: settings);
}

public readonly record struct FileRasterSettings(
    long JpegQuality = FileSheetDefaults.DefaultJpegQuality,
    Option<FileTiffCompression> TiffCompression = default,
    Option<int> PngDepth = default,
    Option<double> ExifDpi = default);

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
        Create(mode: multiple ? FilePromptMode.OpenMany : FilePromptMode.OpenOne, title: title, filter: string.IsNullOrWhiteSpace(value: filter) ? FileFormat.Filter(phase: FilePhase.Import) : filter, fileName: fileName, initialDirectory: initialDirectory, defaultExtension: defaultExtension);

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
        (Name.Normalized.Collision, Write.Normalized.Overwrite, Exists(path: Path)) switch {
            (_, FileOverwritePolicy.Replace, _) => Fin.Succ(value: this),
            (FileCollisionPolicy.Replace, _, _) => Fin.Succ(value: this),
            (FileCollisionPolicy.AppendNumber, _, true) => NextAvailable(op: op),
            (_, _, false) => Fin.Succ(value: this),
            _ => Fin.Fail<FileEndpoint>(error: op.InvalidInput()),
        };

    private Fin<FileEndpoint> NextAvailable(Op op) =>
        toSeq(Enumerable.Range(start: 1, count: MaxCollisionAttempts))
            .Map(index => WithPath(path: NumberedPath(path: Path, index: index)))
            .Find(static endpoint => !Exists(path: endpoint.Path))
            .ToFin(Fail: op.InvalidInput());

    private static Fin<string> NormalizePath(string path, Op op) =>
        op.Catch(() => Fin.Succ(value: IOPath.GetFullPath(path: path)));

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

    internal static string NumberedPath(string path, int index) =>
        IOPath.Combine(
            path1: IOPath.GetDirectoryName(path: path) ?? string.Empty,
            path2: string.Create(CultureInfo.InvariantCulture, $"{IOPath.GetFileNameWithoutExtension(path: path)}-{index:000}{IOPath.GetExtension(path: path)}"));

    private const int MaxCollisionAttempts = 9999;
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

internal static class FileSheetDefaults {
    internal const double DefaultPublishDpi = 300.0;
    internal const long DefaultJpegQuality = 90L;
}

public sealed record FileSheet(
    Option<Guid> Id = default,
    Option<string> Name = default,
    Option<string> Group = default,
    double Dpi = FileSheetDefaults.DefaultPublishDpi,
    bool PrintWidths = true,
    bool Raster = false,
    ViewCaptureSettings.ColorMode Color = ViewCaptureSettings.ColorMode.DisplayColor,
    Option<FileSheetDecor> Decor = default,
    Option<Func<RhinoPageView, bool>> Predicate = default) {
    internal Fin<ViewCaptureSettings> Settings(RhinoPageView page, Op op) =>
        from active in Optional(page).ToFin(Fail: op.InvalidInput())
        from dpi in (double.IsFinite(d: Dpi), Dpi) switch {
            (true, > 0.0) => Fin.Succ(value: Dpi),
            _ => Fin.Fail<double>(error: op.InvalidInput()),
        }
        select Configure(settings: new ViewCaptureSettings(sourcePageView: active, dpi: dpi), page: active);

    private ViewCaptureSettings Configure(ViewCaptureSettings settings, RhinoPageView page) {
        settings.UsePrintWidths = PrintWidths;
        settings.RasterMode = Raster;
        settings.OutputColor = Color;
        _ = Decor.Iter(decor => decor.Apply(settings: settings, page: page));
        return settings;
    }
}

public readonly partial record struct FileSheetDecor(
    bool DrawGrid = false,
    bool DrawAxis = false,
    bool DrawLights = false,
    bool DrawLockedObjects = true,
    bool DrawSelectedObjectsOnly = false,
    bool DrawMargins = false,
    bool DrawClippingPlanes = false,
    bool DrawWallpaper = false,
    bool DrawBackgroundBitmap = false,
    bool DrawBackground = true,
    Option<double> WireThicknessScale = default,
    Option<double> PointSizeMillimeters = default,
    Option<double> ArrowheadSizeMillimeters = default,
    Option<double> TextDotPointSize = default,
    Option<double> ModelScale = default,
    Option<string> HeaderText = default,
    Option<string> FooterText = default) {
    internal ViewCaptureSettings Apply(ViewCaptureSettings settings, RhinoPageView page) {
        settings.DrawGrid = DrawGrid;
        settings.DrawAxis = DrawAxis;
        settings.DrawLights = DrawLights;
        settings.DrawLockedObjects = DrawLockedObjects;
        settings.DrawSelectedObjectsOnly = DrawSelectedObjectsOnly;
        settings.DrawMargins = DrawMargins;
        settings.DrawClippingPlanes = DrawClippingPlanes;
        settings.DrawWallpaper = DrawWallpaper;
        settings.DrawBackgroundBitmap = DrawBackgroundBitmap;
        settings.DrawBackground = DrawBackground;
        _ = WireThicknessScale.Iter(value => settings.WireThicknessScale = value);
        _ = PointSizeMillimeters.Iter(value => settings.PointSizeMillimeters = value);
        _ = ArrowheadSizeMillimeters.Iter(value => settings.ArrowheadSizeMillimeters = value);
        _ = TextDotPointSize.Iter(value => settings.TextDotPointSize = value);
        _ = ModelScale.Iter(settings.SetModelScaleToValue);
        _ = HeaderText.Iter(value => settings.HeaderText = Interpolate(template: value, document: page.Document, page: page));
        _ = FooterText.Iter(value => settings.FooterText = Interpolate(template: value, document: page.Document, page: page));
        return settings;
    }

    private static string Interpolate(string template, RhinoDoc document, RhinoPageView page) {
        int total = document.Views.GetPageViews()?.Length ?? 0;
        return string.IsNullOrEmpty(value: template)
            ? template
            : TokenPattern().Replace(input: template, evaluator: match => match.Groups["key"].Value switch {
                "page" => page.PageName ?? string.Empty,
                "index" => (page.PageNumber + 1).ToString(provider: CultureInfo.InvariantCulture),
                "total" => total.ToString(provider: CultureInfo.InvariantCulture),
                string key => document.Strings.GetValue(key: key) ?? match.Value,
            });
    }

    [GeneratedRegex(pattern: "\\{(?<key>[^{}]+)\\}", options: RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 250)]
    private static partial Regex TokenPattern();
}

public readonly record struct FileSheetSpec(
    string Name,
    Option<double> WidthMillimeters = default,
    Option<double> HeightMillimeters = default,
    Option<string> Group = default);

public readonly record struct FileDetailSpec(
    string Name,
    Point2d Corner,
    Point2d Opposite,
    DefinedViewportProjection Projection = DefinedViewportProjection.Top,
    bool ProjectionLocked = true,
    Option<Guid> DisplayMode = default);

[Union(SwitchMapStateParameterName = "context")]
public abstract partial record FileSheetEdit {
    private FileSheetEdit() { }
    public sealed record Create(FileSheetSpec Spec) : FileSheetEdit;
    public sealed record Remove(string SheetName) : FileSheetEdit;
    public sealed record Duplicate(string SheetName, bool WithGeometry = true) : FileSheetEdit;
    public sealed record Rename(string SheetName, string NewName) : FileSheetEdit;
    public sealed record Reorder(Seq<string> SheetNames) : FileSheetEdit;
    public sealed record AddDetail(string SheetName, FileDetailSpec Spec) : FileSheetEdit;
    public sealed record RemoveDetail(string SheetName, string DetailName) : FileSheetEdit;
    public sealed record ActivateDetail(string SheetName, Option<string> DetailName) : FileSheetEdit;
    public sealed record LayerOverride(string SheetName, string DetailName, string LayerPath, Option<System.Drawing.Color> Color = default, Option<bool> Visible = default) : FileSheetEdit;
    public sealed record ClippingOverride(string SheetName, string DetailName, BoundingBox Box) : FileSheetEdit;
    public sealed record RefreshLinks(Option<Seq<string>> Archives = default, bool SkipUpToDate = false) : FileSheetEdit;
}

internal readonly record struct FileSheetPage(RhinoPageView Page, FileSheet Sheet);

public readonly record struct FileSheetReport(string PageName, Option<string> Scale, int DetailCount);

internal readonly record struct FilePublishResult(Option<FileEndpoint> Target, Option<FileFormat> Format, DocumentReceipt Receipt, Seq<FileSheetReport> Sheets = default, Seq<FileIssue> Issues = default, Option<string> NativeLog = default);

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
    Seq<FileReport> Children = default,
    Seq<FileSheetReport> Sheets = default) {
    public static FileReport Empty(FilePhase phase) =>
        new(Source: Option<FileEndpoint>.None, Target: Option<FileEndpoint>.None, Format: Option<FileFormat>.None, Phase: phase, Receipt: Option<DocumentReceipt>.None, Issues: Seq<FileIssue>(), NativeLog: Option<string>.None, Children: Seq<FileReport>(), Sheets: Seq<FileSheetReport>());

    internal static FileReport Of(FilePhase phase, Option<FileEndpoint> source = default, Option<FileEndpoint> target = default, Option<FileFormat> format = default, Option<DocumentReceipt> receipt = default, Seq<FileIssue> issues = default, Option<string> nativeLog = default, Option<FileArchive> archive = default, Seq<FileReport> children = default, Seq<FileSheetReport> sheets = default) =>
        new(Source: source, Target: target, Format: format, Phase: phase, Receipt: receipt, Issues: issues.IsEmpty ? Seq<FileIssue>() : issues, NativeLog: nativeLog, Archive: archive, Children: children.IsEmpty ? Seq<FileReport>() : children, Sheets: sheets.IsEmpty ? Seq<FileSheetReport>() : sheets);
}
