using Rhino.Collections;
using Rhino.FileIO;
using Rhino.PlugIns;
using IOPath = System.IO.Path;

namespace Rasm.Rhino.Exchange;

// --- [TYPES] ------------------------------------------------------------------------------
public readonly record struct FileCapability {
    private FileCapability(bool import, bool export, bool archive) =>
        (Import, Export, Archive) = (import, export, archive);

    public bool Import { get; }
    public bool Export { get; }
    internal bool Archive { get; }

    public static FileCapability Plugin(bool import = true, bool export = true) =>
        new(import: import, export: export, archive: false);

    internal static FileCapability Direct { get; } = new(import: true, export: true, archive: false);
    internal static FileCapability File3dm { get; } = new(import: true, export: true, archive: true);

    internal bool Supports(FilePhase phase) =>
        phase switch {
            FilePhase.Open or FilePhase.SaveAs or FilePhase.Write3dmFile or FilePhase.SaveTemplate => Archive,
            FilePhase.Headless => Archive || Import,
            FilePhase.Import => Import,
            FilePhase.Export or FilePhase.WriteFile => Export,
            _ => false,
        };
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record FileFormat {
    private readonly FileFormatOps ops;

    private FileFormat(string key, Seq<string> extensions, FileFormatOps ops) =>
        (Key, Extensions, this.ops) = (key, extensions, ops);

    public string Key { get; }
    public Seq<string> Extensions { get; }

    internal Fin<Unit> Read(FileReadOptions options, RhinoDoc document, FileEndpoint source) =>
        ops.DirectRead switch {
            Func<FileReadOptions, RhinoDoc, FileEndpoint, Fin<Unit>> read => read(arg1: options, arg2: document, arg3: source),
            _ => NativeBool(run: () => document.Import(filePath: source.Path, options: options.OptionsDictionary), op: Op.Of(name: $"{Key}Read")),
        };

    internal Fin<Unit> Write(FileWriteOptions options, RhinoDoc document, FileEndpoint target) =>
        ops.DirectWrite switch {
            Func<FileWriteOptions, RhinoDoc, FileEndpoint, Fin<Unit>> write => write(arg1: options, arg2: document, arg3: target),
            _ => NativeBool(run: () => document.WriteFile(path: target.Path, options: options), op: Op.Of(name: $"{Key}Write")),
        };

    internal Fin<Unit> Export(FileWriteOptions options, RhinoDoc document, FileEndpoint target) =>
        ops.DirectWrite switch {
            Func<FileWriteOptions, RhinoDoc, FileEndpoint, Fin<Unit>> write => write(arg1: options, arg2: document, arg3: target),
            _ => options.WriteSelectedObjectsOnly switch {
                true => NativeBool(run: () => document.ExportSelected(filePath: target.Path, options: options.OptionsDictionary), op: Op.Of(name: $"{Key}Export")),
                false => NativeBool(run: () => document.Export(filePath: target.Path, options: options.OptionsDictionary), op: Op.Of(name: $"{Key}Export")),
            },
        };

    internal Option<ArchivableDictionary> Dictionary(FileProfile profile, FilePhase phase) =>
        (phase, ops.Read, ops.Write) switch {
            (FilePhase.Headless or FilePhase.Import, Func<FileProfile, ArchivableDictionary> read, _) => Some(read(arg: profile)),
            (FilePhase.Export or FilePhase.WriteFile, _, Func<FileProfile, ArchivableDictionary> write) => Some(write(arg: profile)),
            _ => Option<ArchivableDictionary>.None,
        };

    internal bool Supports(FilePhase phase) =>
        ops.Capability.Supports(phase: phase);

    public static FileFormat ThreeDm { get; } = Plugin(key: "3dm", extensions: Seq(".3dm"), archive: true);
    public static FileFormat ThreeDs { get; } = Plugin(key: "3ds", extensions: Seq(".3ds"), read: static _ => new File3dsReadOptions().ToDictionary(), write: static profile => new File3dsWriteOptions { SaveViews = profile.Fidelity == FileFidelity.Model, SaveLights = profile.Fidelity == FileFidelity.Model }.ToDictionary());
    public static FileFormat ThreeMf { get; } = Plugin(key: "3mf", extensions: Seq(".3mf"), import: false, write: static _ => new File3mfWriteOptions().ToDictionary());
    public static FileFormat Ai { get; } = Plugin(key: "ai", extensions: Seq(".ai"), read: static _ => new FileAiReadOptions().ToDictionary(), write: static profile => new FileAiWriteOptions { PreserveModelScale = profile.Fidelity == FileFidelity.Model, OrderLayers = profile.Grouping == FileGrouping.Layer || profile.Sort == FileSort.Layer }.ToDictionary());
    public static FileFormat Amf { get; } = Plugin(key: "amf", extensions: Seq(".amf"), import: false, write: static _ => new FileAmfWriteOptions().ToDictionary());
    public static FileFormat Obj { get; } = Direct(
        key: "obj",
        extensions: Seq(".obj"),
        directRead: static (options, document, source) => NativeBool(run: () => FileObj.Read(filename: source.Path, doc: document, options: new FileObjReadOptions(options)), op: Op.Of(name: "objRead")),
        directWrite: static (options, document, target) => NativeWrite(result: FileObj.Write(filename: target.Path, doc: document, options: new FileObjWriteOptions(options)), op: Op.Of(name: "objWrite")));
    public static FileFormat Ply { get; } = Direct(
        key: "ply",
        extensions: Seq(".ply"),
        read: static _ => new FilePlyReadOptions().ToDictionary(),
        directRead: static (_, document, source) => NativeBool(run: () => FilePly.Read(path: source.Path, doc: document, options: new FilePlyReadOptions()), op: Op.Of(name: "plyRead")),
        directWrite: static (options, document, target) => NativeWrite(result: FilePly.Write(filename: target.Path, doc: document, options: new FilePlyWriteOptions(options)), op: Op.Of(name: "plyWrite")));
    public static FileFormat Cd { get; } = Plugin(key: "cd", extensions: Seq(".cd"), import: false, write: static _ => new FileCdWriteOptions().ToDictionary());
    public static FileFormat Dgn { get; } = Plugin(key: "dgn", extensions: Seq(".dgn"), export: false, read: static _ => new FileDgnReadOptions().ToDictionary());
    public static FileFormat Dst { get; } = Plugin(key: "dst", extensions: Seq(".dst"), export: false, read: static _ => new FileDstReadOptions().ToDictionary());
    public static FileFormat Dwg { get; } = Plugin(key: "dwg", extensions: Seq(".dwg", ".dxf"), read: static _ => new FileDwgReadOptions().ToDictionary(), write: static _ => new FileDwgWriteOptions().ToDictionary());
    public static FileFormat Eps { get; } = Plugin(key: "eps", extensions: Seq(".eps"), export: false, read: static _ => new FileEpsReadOptions().ToDictionary());
    public static FileFormat Stl { get; } = Plugin(key: "stl", extensions: Seq(".stl"), read: static _ => new FileStlReadOptions().ToDictionary(), write: static _ => new FileStlWriteOptions { BinaryFile = true }.ToDictionary());
    public static FileFormat Stp { get; } = Plugin(key: "stp", extensions: Seq(".stp", ".step"), read: static _ => new FileStpReadOptions().ToDictionary(), write: static _ => new FileStpWriteOptions().ToDictionary());
    public static FileFormat Fbx { get; } = Plugin(key: "fbx", extensions: Seq(".fbx"), read: static _ => new FileFbxReadOptions().ToDictionary(), write: static profile => new FileFbxWriteOptions { SaveViews = profile.Fidelity == FileFidelity.Model, SaveLights = profile.Fidelity == FileFidelity.Model }.ToDictionary());
    public static FileFormat Ghs { get; } = Plugin(key: "ghs", extensions: Seq(".ghs"), export: false, read: static _ => new FileGHSReadOptions().ToDictionary());
    public static FileFormat Gts { get; } = Plugin(key: "gts", extensions: Seq(".gts"), import: false, write: static _ => new FileGtsWriteOptions().ToDictionary());
    public static FileFormat Iges { get; } = Plugin(key: "igs", extensions: Seq(".igs", ".iges"), import: false, write: static _ => new FileIgsWriteOptions().ToDictionary());
    public static FileFormat Lwo { get; } = Plugin(key: "lwo", extensions: Seq(".lwo"), read: static _ => new FileLwoReadOptions().ToDictionary(), write: static _ => new FileLwoWriteOptions().ToDictionary());
    public static FileFormat Nwd { get; } = Plugin(key: "nwd", extensions: Seq(".nwd"), import: false, write: static _ => new FileNwdWriteOptions().ToDictionary());
    public static FileFormat Pov { get; } = Plugin(key: "pov", extensions: Seq(".pov"), import: false, write: static _ => new FilePovWriteOptions().ToDictionary());
    public static FileFormat Sat { get; } = Plugin(key: "sat", extensions: Seq(".sat"), import: false, write: static _ => new FileSatWriteOptions().ToDictionary());
    public static FileFormat Skp { get; } = Plugin(key: "skp", extensions: Seq(".skp"), read: static _ => new FileSkpReadOptions().ToDictionary(), write: static profile => new FileSkpWriteOptions { GroupObjects = profile.Grouping is FileGrouping.Layer or FileGrouping.Block }.ToDictionary());
    public static FileFormat Sw { get; } = Plugin(key: "sw", extensions: Seq(".sldprt", ".sldasm"), export: false, read: static _ => new FileSwReadOptions().ToDictionary());
    public static FileFormat Udo { get; } = Plugin(key: "udo", extensions: Seq(".udo"), import: false, write: static _ => new FileUdoWriteOptions().ToDictionary());
    public static FileFormat Vda { get; } = Plugin(key: "vda", extensions: Seq(".vda"), import: false, write: static _ => new FileVdaWriteOptions().ToDictionary());
    public static FileFormat Vrml { get; } = Plugin(key: "vrml", extensions: Seq(".wrl", ".vrml"), import: false, write: static profile => new FileVrmlWriteOptions { ExportTextureCoordinates = profile.Resources != FileResourcePolicy.Reference, ExportVertexNormals = profile.Fidelity == FileFidelity.Model }.ToDictionary());
    public static FileFormat X3dv { get; } = Plugin(key: "x3dv", extensions: Seq(".x3dv"), import: false, write: static profile => new FileX3dvWriteOptions { ExportTextureCoordinates = profile.Resources != FileResourcePolicy.Reference, ExportVertexNormals = profile.Fidelity == FileFidelity.Model }.ToDictionary());
    public static FileFormat Xaml { get; } = Plugin(key: "xaml", extensions: Seq(".xaml"), import: false, write: static profile => new FileXamlWriteOptions { UseExistingRenderMeshes = profile.Fidelity == FileFidelity.Model }.ToDictionary());
    public static FileFormat Xt { get; } = Plugin(key: "x_t", extensions: Seq(".x_t", ".x_b"), import: false, write: static _ => new FileX_TWriteOptions().ToDictionary());
    public static FileFormat Raw { get; } = Plugin(key: "raw", extensions: Seq(".raw"), read: static _ => new FileRawReadOptions().ToDictionary(), write: static _ => new FileRawWriteOptions().ToDictionary());
    public static FileFormat Txt { get; } = Plugin(key: "txt", extensions: Seq(".txt"), read: static _ => new FileTxtReadOptions().ToDictionary(), write: static profile => new FileTxtWriteOptions { SurroundWithDoubleQuotes = profile.Grouping != FileGrouping.Document }.ToDictionary());
    public static FileFormat Csv { get; } = Plugin(key: "csv", extensions: Seq(".csv"), import: false, write: static profile => CsvOptions(profile: profile).ToDictionary());
    public static FileFormat Gltf { get; } = Plugin(key: "gltf", extensions: Seq(".gltf", ".glb"), import: false, write: static profile => new FileGltfWriteOptions { ExportMaterials = profile.Resources != FileResourcePolicy.Reference, ExportLayers = profile.Grouping == FileGrouping.Layer }.ToDictionary());
    public static FileFormat Usd { get; } = Plugin(key: "usd", extensions: Seq(".usd", ".usda", ".usdz"), import: false, write: static profile => new FileUsdWriteOptions { ForceMeshes = profile.Fidelity != FileFidelity.Model, IncludeUserStrings = profile.Grouping == FileGrouping.UserString }.ToDictionary());
    public static FileFormat Pdf { get; } = Plugin(key: "pdf", extensions: Seq(".pdf"), export: false, read: static _ => new FilePdfReadOptions().ToDictionary());
    public static FileFormat Svg { get; } = Plugin(key: "svg", extensions: Seq(".svg"), export: false, read: static _ => new FileSvgReadOptions().ToDictionary());

    public static Seq<FileFormat> Known { get; } = Seq(ThreeDm, ThreeDs, ThreeMf, Ai, Amf, Obj, Ply, Cd, Dgn, Dst, Dwg, Eps, Stl, Stp, Fbx, Ghs, Gts, Iges, Lwo, Nwd, Pov, Sat, Skp, Sw, Udo, Vda, Vrml, X3dv, Xaml, Xt, Raw, Txt, Csv, Gltf, Usd, Pdf, Svg);

    public static Option<FileFormat> Detect(string path) =>
        Optional(IOPath.GetExtension(path: path))
            .Bind(extension => Known.Find(format => format.Extensions.Exists(item => string.Equals(a: item, b: extension, comparisonType: StringComparison.OrdinalIgnoreCase))));

    public static Fin<FileFormat> Of(string keyOrExtension) =>
        from key in FileEndpoint.NonBlank(value: keyOrExtension, op: Op.Of(name: nameof(Of)))
        let extension = key.StartsWith('.') ? key : $".{key}"
        from format in Known.Find(item =>
                string.Equals(a: item.Key, b: key.TrimStart('.'), comparisonType: StringComparison.OrdinalIgnoreCase)
                || item.Extensions.Exists(value => string.Equals(a: value, b: extension, comparisonType: StringComparison.OrdinalIgnoreCase)))
            .ToFin(Fail: Op.Of(name: nameof(Of)).InvalidInput())
        select format;

    public static Fin<FileFormat> Custom(string key, Seq<string> extensions, FileCapability capability) =>
        from validKey in FileEndpoint.NonBlank(value: key, op: Op.Of(name: nameof(Custom)))
        from jsonBlocked in validKey.TrimStart('.').Equals(value: "json", comparisonType: StringComparison.OrdinalIgnoreCase)
            ? Fin.Fail<Unit>(error: Op.Of(name: nameof(Custom)).Unsupported(geometryType: typeof(FileFormat), outputType: typeof(FileFormat)))
            : Fin.Succ(value: unit)
        from validExtensions in extensions.TraverseM(extension =>
            FileEndpoint.NonBlank(value: extension, op: Op.Of(name: nameof(Custom)))
                .Bind(text => text.TrimStart('.').Equals(value: "json", comparisonType: StringComparison.OrdinalIgnoreCase)
                    ? Fin.Fail<string>(error: Op.Of(name: nameof(Custom)).Unsupported(geometryType: typeof(FileFormat), outputType: typeof(FileFormat)))
                    : Fin.Succ(value: text.StartsWith('.') ? text : $".{text}"))).As()
        from _ in guard(!validExtensions.IsEmpty && (capability.Import || capability.Export), Op.Of(name: nameof(Custom)).InvalidInput())
        select new FileFormat(key: validKey.TrimStart('.'), extensions: validExtensions.Distinct(), ops: new FileFormatOps(Capability: capability));

    public static string Filter(FilePhase phase = FilePhase.Prompt, Seq<FileFormat> formats = default) =>
        (formats.IsEmpty ? Known : formats)
            .Filter(format => !format.Extensions.IsEmpty && (phase == FilePhase.Prompt || format.Supports(phase: phase)))
            .Map(static format => (
                Label: $"{format.Key.ToUpperInvariant()} ({string.Join(separator: ";", values: format.Extensions.Map(static extension => $"*{extension}").AsIterable())})",
                Patterns: string.Join(separator: ";", values: format.Extensions.Map(static extension => $"*{extension}").AsIterable())))
            .Distinct()
            switch {
                Seq<(string Label, string Patterns)> rows when !rows.IsEmpty =>
                    string.Join(separator: "|", values: (Seq((
                            Label: "All supported files",
                            Patterns: string.Join(separator: ";", values: rows.Bind(static row => toSeq(row.Patterns.Split(';'))).Distinct().AsIterable())))
                        + rows).Map(static row => $"{row.Label}|{row.Patterns}").AsIterable()),
                _ => "All files (*.*)|*.*",
            };

    public string EnsureExtension(string path) =>
        Extensions.Exists(extension => string.Equals(a: extension, b: IOPath.GetExtension(path: path), comparisonType: StringComparison.OrdinalIgnoreCase)) switch {
            true => path,
            false => IOPath.ChangeExtension(path: path, extension: Extensions[0].TrimStart('.')),
        };

    internal static Fin<Unit> NativeBool(Func<bool> run, Op op) =>
        Try.lift<bool>(f: run)
            .Run()
            .MapFail(_ => op.InvalidResult())
            .Bind(success => success switch {
                true => Fin.Succ(value: unit),
                false => Fin.Fail<Unit>(error: op.InvalidResult()),
            });

    internal static Fin<Unit> NativeWrite(WriteFileResult result, Op op) =>
        result switch {
            WriteFileResult.Success => Fin.Succ(value: unit),
            WriteFileResult.Cancel => Fin.Fail<Unit>(error: new Fault.Cancelled()),
            _ => Fin.Fail<Unit>(error: op.InvalidResult()),
        };

    private static FileFormat Plugin(string key, Seq<string> extensions, bool import = true, bool export = true, bool archive = false, Func<FileProfile, ArchivableDictionary>? read = null, Func<FileProfile, ArchivableDictionary>? write = null) =>
        new(key: key, extensions: extensions, ops: new FileFormatOps(Capability: archive ? FileCapability.File3dm : FileCapability.Plugin(import: import, export: export), Read: read, Write: write));

    private static FileFormat Direct(
        string key,
        Seq<string> extensions,
        Func<FileReadOptions, RhinoDoc, FileEndpoint, Fin<Unit>> directRead,
        Func<FileWriteOptions, RhinoDoc, FileEndpoint, Fin<Unit>> directWrite,
        Func<FileProfile, ArchivableDictionary>? read = null) =>
        new(
            key: key,
            extensions: extensions,
            ops: new FileFormatOps(
                Capability: FileCapability.Direct,
                Read: read,
                DirectRead: directRead,
                DirectWrite: directWrite));

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

    private readonly record struct FileFormatOps(
        FileCapability Capability,
        Func<FileProfile, ArchivableDictionary>? Read = null,
        Func<FileProfile, ArchivableDictionary>? Write = null,
        Func<FileReadOptions, RhinoDoc, FileEndpoint, Fin<Unit>>? DirectRead = null,
        Func<FileWriteOptions, RhinoDoc, FileEndpoint, Fin<Unit>>? DirectWrite = null);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class FileFormatProjection {
    private readonly record struct WriteFrame(FilePhase Phase, bool UpdatePath);

    internal static Option<FileFormat> Resolve(FileEndpoint endpoint, FileProfile profile) =>
        endpoint.Format.Case switch {
            FileFormat value => Some(value),
            _ => profile.Format,
        };

    internal static Fin<FileFormat> Require(FileEndpoint endpoint, FileProfile profile, FilePhase phase, Op op) =>
        from format in Resolve(endpoint: endpoint, profile: profile).ToFin(Fail: op.InvalidInput())
        from _ in guard(format.Supports(phase: phase), op.InvalidInput())
        select format;

    internal static Fin<ArchivableDictionary> Dictionary(FileEndpoint endpoint, FileProfile profile, FilePhase phase, Op op) =>
        Resolve(endpoint: endpoint, profile: profile).Case switch {
            FileFormat format when format.Supports(phase: phase) => Fin.Succ(value: format.Dictionary(profile: profile, phase: phase).IfNone(new ArchivableDictionary())),
            FileFormat => Fin.Fail<ArchivableDictionary>(error: op.InvalidInput()),
            _ => Fin.Succ(value: new ArchivableDictionary()),
        };

    internal static Fin<Unit> Import(RhinoDoc document, FileEndpoint source, FileProfile profile, Op op) =>
        from format in Require(endpoint: source, profile: profile, phase: FilePhase.Import, op: op)
        from imported in WithReadOptions(source: source, profile: profile, op: op, run: options => format.Read(options: options, document: document, source: source))
        select imported;

    internal static Fin<Unit> Write(RhinoDoc document, Option<FileEndpoint> target, FileProfile profile, FileWriteIntent intent, bool selected, Op op) =>
        intent switch {
            FileWriteIntent.Save => FileFormat.NativeBool(run: document.Save, op: op),
            FileWriteIntent.SaveAs => Target(target: target, op: op).Bind(endpoint => SaveAs(document: document, target: endpoint, profile: profile, op: op)),
            FileWriteIntent.Write3dm => Target(target: target, op: op).Bind(endpoint => Write3dmFile(document: document, target: endpoint, profile: profile, selected: selected, op: op)),
            FileWriteIntent.Template => Target(target: target, op: op).Bind(endpoint => SaveTemplate(document: document, target: endpoint, op: op)),
            _ => Target(target: target, op: op).Bind(endpoint =>
                from frame in Fin.Succ(value: Frame(intent: intent))
                from format in Require(endpoint: endpoint, profile: profile, phase: frame.Phase, op: op)
                from written in WithWriteOptions(
                    target: endpoint,
                    profile: profile,
                    phase: frame.Phase,
                    selected: selected,
                    updatePath: frame.UpdatePath,
                    op: op,
                    run: options => frame.Phase switch {
                        FilePhase.Export => format.Export(options: options, document: document, target: endpoint),
                        _ => format.Write(options: options, document: document, target: endpoint),
                    })
                select written),
        };

    internal static File3dmWriteOptions ArchiveWriteOptions(ArchiveProfile profile) {
        FileWritePolicy write = profile.Write.Normalized;
        File3dmWriteOptions options = new() { Version = write.Version, SaveUserData = write.WriteUserData };
        options.EnableRenderMeshes(objectType: ObjectType.AnyObject, enable: write.IncludeRenderMeshes);
        options.EnableAnalysisMeshes(objectType: ObjectType.AnyObject, enable: write.IncludeRenderMeshes);
        return options;
    }

    private static WriteFrame Frame(FileWriteIntent intent) =>
        intent switch {
            FileWriteIntent.Export => new(Phase: FilePhase.Export, UpdatePath: false),
            _ => new(Phase: FilePhase.WriteFile, UpdatePath: intent == FileWriteIntent.WriteFile),
        };

    private static Fin<FileEndpoint> Target(Option<FileEndpoint> target, Op op) =>
        target.ToFin(Fail: op.InvalidInput());

    private static Fin<Unit> SaveAs(RhinoDoc document, FileEndpoint target, FileProfile profile, Op op) =>
        WithWriteOptions(
            target: target,
            profile: profile,
            phase: FilePhase.SaveAs,
            selected: false,
            updatePath: true,
            op: op,
            run: options => FileFormat.NativeBool(run: () => document.WriteFile(path: target.Path, options: options), op: op));

    private static Fin<Unit> SaveTemplate(RhinoDoc document, FileEndpoint target, Op op) =>
        FileFormat.NativeBool(run: () => document.SaveAsTemplate(file3dmTemplatePath: target.Path, version: target.Write.Normalized.Version), op: op);

    private static Fin<Unit> Write3dmFile(RhinoDoc document, FileEndpoint target, FileProfile profile, bool selected, Op op) =>
        WithWriteOptions(
            target: target,
            profile: profile,
            phase: FilePhase.Write3dmFile,
            selected: selected,
            updatePath: false,
            op: op,
            run: options => FileFormat.NativeBool(run: () => document.Write3dmFile(path: target.Path, options: options), op: op));

    private static Fin<Unit> WithReadOptions(FileEndpoint source, FileProfile profile, Op op, Func<FileReadOptions, Fin<Unit>> run) =>
        Try.lift<Fin<Unit>>(f: () => {
            using FileReadOptions options = ReadOptions(endpoint: source, profile: profile);
            return run(arg: options);
        })
            .Run()
            .MapFail(_ => op.InvalidResult())
            .Bind(static result => result);

    private static Fin<Unit> WithWriteOptions(FileEndpoint target, FileProfile profile, FilePhase phase, bool selected, bool updatePath, Op op, Func<FileWriteOptions, Fin<Unit>> run) =>
        Try.lift<Fin<Unit>>(f: () => {
            using FileWriteOptions options = WriteOptions(endpoint: target, profile: profile, phase: phase, selected: selected, updatePath: updatePath);
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

    private static FileWriteOptions WriteOptions(FileEndpoint endpoint, FileProfile profile, FilePhase phase, bool selected, bool updatePath) {
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
        _ = Dictionary(endpoint: endpoint, profile: profile, phase: phase, op: Op.Of(name: nameof(WriteOptions))).Map(options.OptionsDictionary.AddContentsFrom);
        return options;
    }
}
