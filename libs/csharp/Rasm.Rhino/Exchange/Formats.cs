using Rhino.Collections;
using Rhino.FileIO;
using Rhino.PlugIns;
using IOPath = System.IO.Path;

namespace Rasm.Rhino.Exchange;

// --- [TYPES] ------------------------------------------------------------------------------
[Flags]
public enum FileFormatCapability {
    None = 0,
    Import = 1,
    Export = 2,
    DictionaryRead = 4,
    DictionaryWrite = 8,
    DirectRead = 16,
    DirectWrite = 32,
    Archive = 64,
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record FileFormat {
    private FileFormat(string key, Seq<string> extensions, FileFormatCapability capabilities) =>
        (Key, Extensions, Capabilities) = (key, extensions, capabilities);

    public string Key { get; }
    public Seq<string> Extensions { get; }
    public FileFormatCapability Capabilities { get; }

    public static FileFormat ThreeDm { get; } = new(key: "3dm", extensions: Seq(".3dm"), capabilities: FileFormatCapability.Import | FileFormatCapability.Export | FileFormatCapability.Archive);
    public static FileFormat Obj { get; } = new(key: "obj", extensions: Seq(".obj"), capabilities: FileFormatCapability.Import | FileFormatCapability.Export | FileFormatCapability.DirectRead | FileFormatCapability.DirectWrite);
    public static FileFormat Ply { get; } = new(key: "ply", extensions: Seq(".ply"), capabilities: FileFormatCapability.Import | FileFormatCapability.Export | FileFormatCapability.DictionaryRead | FileFormatCapability.DirectRead | FileFormatCapability.DirectWrite);
    public static FileFormat Dwg { get; } = new(key: "dwg", extensions: Seq(".dwg", ".dxf"), capabilities: FileFormatCapability.Import | FileFormatCapability.Export | FileFormatCapability.DictionaryRead | FileFormatCapability.DictionaryWrite);
    public static FileFormat Stl { get; } = new(key: "stl", extensions: Seq(".stl"), capabilities: FileFormatCapability.Import | FileFormatCapability.Export | FileFormatCapability.DictionaryRead | FileFormatCapability.DictionaryWrite);
    public static FileFormat Stp { get; } = new(key: "stp", extensions: Seq(".stp", ".step"), capabilities: FileFormatCapability.Import | FileFormatCapability.Export | FileFormatCapability.DictionaryRead | FileFormatCapability.DictionaryWrite);
    public static FileFormat Fbx { get; } = new(key: "fbx", extensions: Seq(".fbx"), capabilities: FileFormatCapability.Import | FileFormatCapability.Export | FileFormatCapability.DictionaryRead | FileFormatCapability.DictionaryWrite);
    public static FileFormat Skp { get; } = new(key: "skp", extensions: Seq(".skp"), capabilities: FileFormatCapability.Import | FileFormatCapability.Export | FileFormatCapability.DictionaryRead | FileFormatCapability.DictionaryWrite);
    public static FileFormat Raw { get; } = new(key: "raw", extensions: Seq(".raw"), capabilities: FileFormatCapability.Import | FileFormatCapability.Export | FileFormatCapability.DictionaryRead | FileFormatCapability.DictionaryWrite);
    public static FileFormat Txt { get; } = new(key: "txt", extensions: Seq(".txt"), capabilities: FileFormatCapability.Import | FileFormatCapability.Export | FileFormatCapability.DictionaryRead | FileFormatCapability.DictionaryWrite);
    public static FileFormat Csv { get; } = new(key: "csv", extensions: Seq(".csv"), capabilities: FileFormatCapability.Export | FileFormatCapability.DictionaryWrite);
    public static FileFormat Gltf { get; } = new(key: "gltf", extensions: Seq(".gltf", ".glb"), capabilities: FileFormatCapability.Export | FileFormatCapability.DictionaryWrite);
    public static FileFormat Usd { get; } = new(key: "usd", extensions: Seq(".usd", ".usda", ".usdz"), capabilities: FileFormatCapability.Export | FileFormatCapability.DictionaryWrite);
    public static FileFormat Pdf { get; } = new(key: "pdf", extensions: Seq(".pdf"), capabilities: FileFormatCapability.Import | FileFormatCapability.DictionaryRead);
    public static FileFormat Svg { get; } = new(key: "svg", extensions: Seq(".svg"), capabilities: FileFormatCapability.Import | FileFormatCapability.DictionaryRead);

    public static Seq<FileFormat> Known { get; } = Seq(ThreeDm, Obj, Ply, Dwg, Stl, Stp, Fbx, Skp, Raw, Txt, Csv, Gltf, Usd, Pdf, Svg);

    public static Option<FileFormat> Detect(string path) =>
        Optional(IOPath.GetExtension(path: path))
            .Bind(extension => Known.Find(format => format.Extensions.Exists(item => string.Equals(a: item, b: extension, comparisonType: StringComparison.OrdinalIgnoreCase))));

    public string EnsureExtension(string path) =>
        Extensions.Exists(extension => string.Equals(a: extension, b: IOPath.GetExtension(path: path), comparisonType: StringComparison.OrdinalIgnoreCase)) switch {
            true => path,
            false => IOPath.ChangeExtension(path: path, extension: Extensions[0].TrimStart('.')),
        };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class FileFormatProjection {
    internal static Option<FileFormat> Resolve(FileEndpoint endpoint, FileProfile profile) =>
        endpoint.Format.Case switch {
            FileFormat value => Some(value),
            _ => profile.Format,
        };

    internal static Fin<FileFormat> Require(FileEndpoint endpoint, FileProfile profile, FilePhase phase, Op op) =>
        Resolve(endpoint: endpoint, profile: profile).ToFin(Fail: op.InvalidInput()).Bind(format => SupportsPhase(format: format, phase: phase) switch {
            true => Fin.Succ(value: format),
            false => Fin.Fail<FileFormat>(error: op.InvalidInput()),
        });

    internal static Fin<ArchivableDictionary> Dictionary(FileEndpoint endpoint, FileProfile profile, FilePhase phase, Op op) =>
        Resolve(endpoint: endpoint, profile: profile).Case switch {
            FileFormat format when SupportsDictionary(format: format, phase: phase) => NativeDictionary(format: format, profile: profile, phase: phase, op: op),
            FileFormat format when SupportsPhase(format: format, phase: phase) => Fin.Succ(value: new ArchivableDictionary()),
            FileFormat => Fin.Fail<ArchivableDictionary>(error: op.InvalidInput()),
            _ => Fin.Succ(value: new ArchivableDictionary()),
        };

    internal static Fin<Unit> Import(RhinoDoc document, FileEndpoint source, FileProfile profile, Op op) =>
        Resolve(endpoint: source, profile: profile).Case switch {
            FileFormat format when format == FileFormat.Obj => WithReadOptions(source: source, profile: profile, op: op, run: options => FileObj.Read(filename: source.Path, doc: document, options: new FileObjReadOptions(options))),
            _ => Dictionary(endpoint: source, profile: profile, phase: FilePhase.Import, op: op)
                .Bind(options => TryResult(run: () => document.Import(filePath: source.Path, options: options), op: op)),
        };

    internal static Fin<Unit> Export(RhinoDoc document, FileEndpoint target, FileProfile profile, bool selected, Op op) =>
        Dictionary(endpoint: target, profile: profile, phase: FilePhase.Export, op: op)
            .Bind(options => selected switch {
                true => TryResult(run: () => document.ExportSelected(filePath: target.Path, options: options), op: op),
                false => TryResult(run: () => document.Export(filePath: target.Path, options: options), op: op),
            });

    internal static Fin<Unit> WriteFile(RhinoDoc document, FileEndpoint target, FileProfile profile, bool selected, bool updatePath, Op op) =>
        Resolve(endpoint: target, profile: profile).Case switch {
            FileFormat format when format == FileFormat.Obj && !selected => WithWriteOptions(target: target, profile: profile, selected: selected, updatePath: updatePath, op: op, run: options => WriteResult(result: FileObj.Write(filename: target.Path, doc: document, options: new FileObjWriteOptions(options)), op: op)),
            FileFormat format when format == FileFormat.Ply && !selected => WithWriteOptions(target: target, profile: profile, selected: selected, updatePath: updatePath, op: op, run: options => WriteResult(result: FilePly.Write(filename: target.Path, doc: document, options: new FilePlyWriteOptions(options)), op: op)),
            _ => WithWriteOptions(target: target, profile: profile, selected: selected, updatePath: updatePath, op: op, run: options => document.WriteFile(path: target.Path, options: options) switch {
                true => Fin.Succ(value: unit),
                false => Fin.Fail<Unit>(error: op.InvalidResult()),
            }),
        };

    internal static Fin<Unit> Write3dmFile(RhinoDoc document, FileEndpoint target, FileProfile profile, bool selected, bool updatePath, Op op) =>
        WithWriteOptions(target: target, profile: profile, selected: selected, updatePath: updatePath, op: op, run: options => document.Write3dmFile(path: target.Path, options: options) switch {
            true => Fin.Succ(value: unit),
            false => Fin.Fail<Unit>(error: op.InvalidResult()),
        });

    internal static Fin<Unit> SaveAs(RhinoDoc document, FileEndpoint target, FileProfile profile, Op op) =>
        TryResult(run: () => document.SaveAs(
            file3dmPath: target.Path,
            version: target.Write.Normalized.Version,
            saveSmall: profile.Fidelity == FileFidelity.Small,
            saveTextures: target.Write.Normalized.IncludeBitmapTable,
            saveGeometryOnly: profile.Fidelity == FileFidelity.GeometryOnly || target.Write.Normalized.GeometryOnly,
            savePluginData: target.Write.Normalized.WriteUserData,
            useCompression: target.Write.Normalized.UseCompression), op: op);

    internal static Fin<Unit> SaveTemplate(RhinoDoc document, FileEndpoint target, Op op) =>
        TryResult(run: () => document.SaveAsTemplate(file3dmTemplatePath: target.Path, version: target.Write.Normalized.Version), op: op);

    internal static File3dmWriteOptions ArchiveWriteOptions(ArchiveProfile profile) {
        FileWritePolicy write = profile.Write.Normalized;
        File3dmWriteOptions options = new() { Version = write.Version, SaveUserData = write.WriteUserData };
        options.EnableRenderMeshes(objectType: ObjectType.AnyObject, enable: write.IncludeRenderMeshes);
        options.EnableAnalysisMeshes(objectType: ObjectType.AnyObject, enable: write.IncludeRenderMeshes);
        return options;
    }

    private static Fin<Unit> WithReadOptions(FileEndpoint source, FileProfile profile, Op op, Func<FileReadOptions, bool> run) =>
        Try.lift<Fin<Unit>>(f: () => {
            using FileReadOptions options = ReadOptions(endpoint: source, profile: profile);
            return run(arg: options) switch {
                true => Fin.Succ(value: unit),
                false => Fin.Fail<Unit>(error: op.InvalidResult()),
            };
        })
            .Run()
            .MapFail(_ => op.InvalidResult())
            .Bind(static result => result);

    private static Fin<Unit> WithWriteOptions(FileEndpoint target, FileProfile profile, bool selected, bool updatePath, Op op, Func<FileWriteOptions, Fin<Unit>> run) =>
        Try.lift<Fin<Unit>>(f: () => {
            using FileWriteOptions options = WriteOptions(endpoint: target, profile: profile, selected: selected, updatePath: updatePath);
            return run(arg: options);
        })
            .Run()
            .MapFail(_ => op.InvalidResult())
            .Bind(static result => result);

    private static FileReadOptions ReadOptions(FileEndpoint endpoint, FileProfile profile) {
        FileReadOptions options = new() {
            ImportMode = true,
            BatchMode = true,
            UseScaleGeometry = true,
            ScaleGeometry = true,
        };
        _ = Dictionary(endpoint: endpoint, profile: profile, phase: FilePhase.Import, op: Op.Of(name: nameof(ReadOptions))).Map(options.OptionsDictionary.AddContentsFrom);
        return options;
    }

    private static FileWriteOptions WriteOptions(FileEndpoint endpoint, FileProfile profile, bool selected, bool updatePath) {
        FileWriteOptions options = new() {
            UpdateDocumentPath = updatePath,
            WriteSelectedObjectsOnly = selected,
            IncludeRenderMeshes = endpoint.Write.Normalized.IncludeRenderMeshes,
            IncludePreviewImage = endpoint.Write.Normalized.IncludePreviewImage,
            IncludeBitmapTable = endpoint.Write.Normalized.IncludeBitmapTable,
            IncludeHistory = endpoint.Write.Normalized.IncludeHistory,
            SuppressDialogBoxes = true,
            SuppressAllInput = true,
            WriteGeometryOnly = profile.Fidelity == FileFidelity.GeometryOnly || endpoint.Write.Normalized.GeometryOnly,
            WriteUserData = endpoint.Write.Normalized.WriteUserData,
            FileVersion = endpoint.Write.Normalized.Version,
            UseCompression = endpoint.Write.Normalized.UseCompression,
        };
        _ = Dictionary(endpoint: endpoint, profile: profile, phase: FilePhase.WriteFile, op: Op.Of(name: nameof(WriteOptions))).Map(options.OptionsDictionary.AddContentsFrom);
        return options;
    }

    private static Fin<ArchivableDictionary> NativeDictionary(FileFormat format, FileProfile profile, FilePhase phase, Op op) =>
        Try.lift<ArchivableDictionary>(f: () =>
            phase switch {
                FilePhase.Import => ReadDictionary(format: format),
                _ => WriteDictionary(format: format, profile: profile),
            })
            .Run()
            .MapFail(_ => op.InvalidResult());

    private static ArchivableDictionary ReadDictionary(FileFormat format) =>
        format switch {
            FileFormat value when value == FileFormat.Dwg => new FileDwgReadOptions().ToDictionary(),
            FileFormat value when value == FileFormat.Stl => new FileStlReadOptions().ToDictionary(),
            FileFormat value when value == FileFormat.Stp => new FileStpReadOptions().ToDictionary(),
            FileFormat value when value == FileFormat.Fbx => new FileFbxReadOptions().ToDictionary(),
            FileFormat value when value == FileFormat.Skp => new FileSkpReadOptions().ToDictionary(),
            FileFormat value when value == FileFormat.Raw => new FileRawReadOptions().ToDictionary(),
            FileFormat value when value == FileFormat.Txt => new FileTxtReadOptions().ToDictionary(),
            FileFormat value when value == FileFormat.Ply => new FilePlyReadOptions().ToDictionary(),
            FileFormat value when value == FileFormat.Pdf => new FilePdfReadOptions().ToDictionary(),
            FileFormat value when value == FileFormat.Svg => new FileSvgReadOptions().ToDictionary(),
            _ => new ArchivableDictionary(),
        };

    private static ArchivableDictionary WriteDictionary(FileFormat format, FileProfile profile) =>
        format switch {
            FileFormat value when value == FileFormat.Dwg => new FileDwgWriteOptions().ToDictionary(),
            FileFormat value when value == FileFormat.Stl => new FileStlWriteOptions { BinaryFile = true }.ToDictionary(),
            FileFormat value when value == FileFormat.Stp => new FileStpWriteOptions().ToDictionary(),
            FileFormat value when value == FileFormat.Fbx => new FileFbxWriteOptions { SaveViews = profile.Fidelity == FileFidelity.Model, SaveLights = profile.Fidelity == FileFidelity.Model }.ToDictionary(),
            FileFormat value when value == FileFormat.Skp => new FileSkpWriteOptions { GroupObjects = profile.Grouping is FileGrouping.Layer or FileGrouping.Block }.ToDictionary(),
            FileFormat value when value == FileFormat.Raw => new FileRawWriteOptions().ToDictionary(),
            FileFormat value when value == FileFormat.Txt => new FileTxtWriteOptions { SurroundWithDoubleQuotes = profile.Grouping != FileGrouping.Document }.ToDictionary(),
            FileFormat value when value == FileFormat.Csv => CsvOptions(profile: profile).ToDictionary(),
            FileFormat value when value == FileFormat.Gltf => new FileGltfWriteOptions { ExportMaterials = profile.Resources != FileResourcePolicy.Reference, ExportLayers = profile.Grouping == FileGrouping.Layer }.ToDictionary(),
            FileFormat value when value == FileFormat.Usd => new FileUsdWriteOptions { ForceMeshes = profile.Fidelity != FileFidelity.Model, IncludeUserStrings = profile.Grouping == FileGrouping.UserString }.ToDictionary(),
            _ => new ArchivableDictionary(),
        };

    private static FileCsvWriteOptions CsvOptions(FileProfile profile) =>
        new() {
            Header = true,
            LayerName = profile.Grouping == FileGrouping.Layer || profile.Sort == FileSort.Layer,
            GroupName = profile.Grouping == FileGrouping.Block,
            ObjectName = profile.Grouping == FileGrouping.ObjectName || profile.Sort == FileSort.ObjectName,
            ObjectID = true,
            ObjectColor = profile.Sort == FileSort.ObjectType,
            ObjectMaterial = profile.Grouping == FileGrouping.Material || profile.Sort == FileSort.Material,
            ObjectDescription = profile.Grouping == FileGrouping.UserString,
            AttributesKeys = profile.Grouping == FileGrouping.UserString,
            AttributesTexts = profile.Grouping == FileGrouping.UserString,
            ObjectKeys = profile.Grouping == FileGrouping.UserString,
            ObjectsTexts = profile.Grouping == FileGrouping.UserString,
        };

    private static bool SupportsDictionary(FileFormat format, FilePhase phase) =>
        phase switch {
            FilePhase.Import => format.Capabilities.HasFlag(flag: FileFormatCapability.DictionaryRead),
            FilePhase.Export or FilePhase.WriteFile => format.Capabilities.HasFlag(flag: FileFormatCapability.DictionaryWrite),
            _ => false,
        };

    private static bool SupportsPhase(FileFormat format, FilePhase phase) =>
        phase switch {
            FilePhase.Open => format == FileFormat.ThreeDm,
            FilePhase.Import => format.Capabilities.HasFlag(flag: FileFormatCapability.Import),
            FilePhase.Export or FilePhase.WriteFile => format.Capabilities.HasFlag(flag: FileFormatCapability.Export),
            FilePhase.Write3dmFile or FilePhase.SaveAs or FilePhase.SaveTemplate => format == FileFormat.ThreeDm,
            _ => false,
        };

    private static Fin<Unit> TryResult(Func<bool> run, Op op) =>
        Try.lift<bool>(f: run)
            .Run()
            .MapFail(_ => op.InvalidResult())
            .Bind(success => success switch {
                true => Fin.Succ(value: unit),
                false => Fin.Fail<Unit>(error: op.InvalidResult()),
            });

    private static Fin<Unit> WriteResult(WriteFileResult result, Op op) =>
        result switch {
            WriteFileResult.Success => Fin.Succ(value: unit),
            WriteFileResult.Cancel => Fin.Fail<Unit>(error: new Fault.Cancelled()),
            _ => Fin.Fail<Unit>(error: op.InvalidResult()),
        };
}
