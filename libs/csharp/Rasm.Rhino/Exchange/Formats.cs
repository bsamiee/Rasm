using System.Collections.Frozen;
using Rhino.Collections;
using Rhino.FileIO;
using Rhino.PlugIns;
using IOPath = System.IO.Path;

namespace Rasm.Rhino.Exchange;

// --- [TYPES] ------------------------------------------------------------------------------
[Flags]
public enum FileCapability {
    None = 0,
    Import = 1 << 0,
    Export = 1 << 1,
    Archive = 1 << 2,
    Publish = 1 << 3,
    File3dm = Import | Export | Archive,
    Raster = Publish,
    Vector = Import | Publish,
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record FileFormat {
    private FileFormat(string key, Seq<string> extensions, FileCapability capability,
        Func<FileProfile, ArchivableDictionary>? read,
        Func<FileProfile, ArchivableDictionary>? write,
        Func<FileProfile, FileReadOptions, RhinoDoc, FileEndpoint, Fin<Unit>>? directRead,
        Func<FileProfile, FileWriteOptions, RhinoDoc, FileEndpoint, Fin<Unit>>? directWrite,
        bool directWriteOptions) {
        Key = key;
        Extensions = extensions;
        Capability = capability;
        readBuilder = read;
        writeBuilder = write;
        directReadCall = directRead;
        directWriteCall = directWrite;
        this.directWriteOptions = directWriteOptions;
    }

    public string Key { get; }
    public Seq<string> Extensions { get; }
    internal FileCapability Capability { get; }

    private readonly Func<FileProfile, ArchivableDictionary>? readBuilder;
    private readonly Func<FileProfile, ArchivableDictionary>? writeBuilder;
    private readonly Func<FileProfile, FileReadOptions, RhinoDoc, FileEndpoint, Fin<Unit>>? directReadCall;
    private readonly Func<FileProfile, FileWriteOptions, RhinoDoc, FileEndpoint, Fin<Unit>>? directWriteCall;
    private readonly bool directWriteOptions;

    internal Fin<Unit> Read(FileReadOptions options, RhinoDoc document, FileEndpoint source, FileProfile profile) =>
        directReadCall switch {
            Func<FileProfile, FileReadOptions, RhinoDoc, FileEndpoint, Fin<Unit>> read => read(arg1: profile, arg2: options, arg3: document, arg4: source),
            _ => NativeBool(run: () => document.Import(filePath: source.Path, options: options.OptionsDictionary), op: Op.Of(name: $"{Key}Read")),
        };

    internal Fin<Unit> Write(FileWriteOptions options, RhinoDoc document, FileEndpoint target, FileProfile profile) =>
        ((directWriteOptions || options.Xform.IsIdentity) && !options.CreateBackupFiles && !options.CreateOtherBackupFiles, directWriteCall) switch {
            (true, Func<FileProfile, FileWriteOptions, RhinoDoc, FileEndpoint, Fin<Unit>> write) => write(arg1: profile, arg2: options, arg3: document, arg4: target),
            _ => NativeBool(run: () => document.WriteFile(path: target.Path, options: options), op: Op.Of(name: $"{Key}Write")),
        };

    internal Fin<Unit> Export(FileWriteOptions options, RhinoDoc document, FileEndpoint target, FileProfile profile) =>
        (this == ThreeDm, options.WriteSelectedObjectsOnly, directWriteOptions || options.Xform.IsIdentity, directWriteCall) switch {
            (true, _, _, _) => NativeBool(run: () => document.Write3dmFile(path: target.Path, options: options), op: Op.Of(name: $"{Key}Export")),
            (false, false, true, Func<FileProfile, FileWriteOptions, RhinoDoc, FileEndpoint, Fin<Unit>> write) => write(arg1: profile, arg2: options, arg3: document, arg4: target),
            (false, true, _, _) => NativeBool(run: () => document.ExportSelected(filePath: target.Path, options: options.OptionsDictionary), op: Op.Of(name: $"{Key}Export")),
            _ => NativeBool(run: () => document.Export(filePath: target.Path, options: options.OptionsDictionary), op: Op.Of(name: $"{Key}Export")),
        };

    internal Option<ArchivableDictionary> Dictionary(FileProfile profile, FilePhase phase) =>
        (phase == FilePhase.Headless || phase == FilePhase.Import) && readBuilder is not null ? Some(readBuilder(arg: profile))
        : (phase == FilePhase.Export || phase == FilePhase.WriteFile) && writeBuilder is not null ? Some(writeBuilder(arg: profile))
        : Option<ArchivableDictionary>.None;

    internal bool Supports(FilePhase phase) =>
        phase.Allows(capability: Capability);

    public static FileFormat ThreeDm { get; } = Native(key: "3dm", extensions: Seq(".3dm"), archive: true);
    public static FileFormat ThreeDs { get; } = Native(
        key: "3ds",
        extensions: Seq(".3ds"),
        read: static _ => new File3dsReadOptions().ToDictionary(),
        write: static profile => ThreeDsWriteOptions(profile: profile).ToDictionary(),
        directRead: DirectRead(NewRead<File3dsReadOptions>(), File3ds.Read, "3dsRead"),
        directWrite: DirectWrite(static (profile, _) => ThreeDsWriteOptions(profile: profile), File3ds.Write, "3dsWrite"));
    public static FileFormat ThreeMf { get; } = Native(
        key: "3mf",
        extensions: Seq(".3mf"),
        import: false,
        write: static _ => new File3mfWriteOptions().ToDictionary(),
        directWrite: DirectWrite(NewWrite<File3mfWriteOptions>(), File3mf.Write, "3mfWrite"));
    public static FileFormat Ai { get; } = Native(
        key: "ai",
        extensions: Seq(".ai"),
        read: static _ => new FileAiReadOptions().ToDictionary(),
        write: static profile => AiWriteOptions(profile: profile).ToDictionary(),
        directRead: DirectRead(NewRead<FileAiReadOptions>(), FileAi.Read, "aiRead"),
        directWrite: DirectWrite(static (profile, _) => AiWriteOptions(profile: profile), FileAi.Write, "aiWrite"));
    public static FileFormat Amf { get; } = Native(
        key: "amf",
        extensions: Seq(".amf"),
        import: false,
        write: static _ => new FileAmfWriteOptions().ToDictionary(),
        directWrite: DirectWrite(NewWrite<FileAmfWriteOptions>(), FileAmf.Write, "amfWrite"));
    public static FileFormat Obj { get; } = Native(
        key: "obj",
        extensions: Seq(".obj"),
        directRead: DirectRead(static (_, options) => new FileObjReadOptions(options), FileObj.Read, "objRead"),
        directWrite: DirectWriteResult(ObjWriteOptions, FileObj.Write, "objWrite"),
        directWriteOptions: true);
    public static FileFormat Ply { get; } = Native(
        key: "ply",
        extensions: Seq(".ply"),
        read: static _ => new FilePlyReadOptions().ToDictionary(),
        directRead: DirectRead(NewRead<FilePlyReadOptions>(), FilePly.Read, "plyRead"),
        directWrite: DirectWriteResult(PlyWriteOptions, FilePly.Write, "plyWrite"),
        directWriteOptions: true);
    public static FileFormat Cd { get; } = Native(key: "cd", extensions: Seq(".cd"), import: false, write: static _ => new FileCdWriteOptions().ToDictionary(), directWrite: DirectWrite(NewWrite<FileCdWriteOptions>(), FileCd.Write, "cdWrite"));
    public static FileFormat Dgn { get; } = Native(key: "dgn", extensions: Seq(".dgn"), export: false, read: static _ => new FileDgnReadOptions().ToDictionary(), directRead: DirectRead(NewRead<FileDgnReadOptions>(), FileDgn.Read, "dgnRead"));
    public static FileFormat Dst { get; } = Native(key: "dst", extensions: Seq(".dst"), export: false, read: static _ => new FileDstReadOptions().ToDictionary(), directRead: DirectRead(NewRead<FileDstReadOptions>(), FileDst.Read, "dstRead"));
    public static FileFormat Dwg { get; } = Native(key: "dwg", extensions: Seq(".dwg", ".dxf"), read: static _ => new FileDwgReadOptions().ToDictionary(), write: static profile => DwgWriteOptions(profile: profile).ToDictionary(), directRead: DirectRead(NewRead<FileDwgReadOptions>(), FileDwg.Read, "dwgRead"), directWrite: DirectWrite(static (profile, _) => DwgWriteOptions(profile: profile), FileDwg.Write, "dwgWrite"));
    public static FileFormat Eps { get; } = Native(key: "eps", extensions: Seq(".eps"), export: false, read: static _ => new FileEpsReadOptions().ToDictionary(), directRead: DirectRead(NewRead<FileEpsReadOptions>(), FileEps.Read, "epsRead"));
    public static FileFormat Stl { get; } = Native(key: "stl", extensions: Seq(".stl"), read: static _ => new FileStlReadOptions().ToDictionary(), write: static _ => StlWriteOptions().ToDictionary(), directRead: DirectRead(NewRead<FileStlReadOptions>(), FileStl.Read, "stlRead"), directWrite: DirectWrite(static (_, _) => StlWriteOptions(), FileStl.Write, "stlWrite"));
    public static FileFormat Stp { get; } = Native(key: "stp", extensions: Seq(".stp", ".step"), read: static _ => new FileStpReadOptions().ToDictionary(), write: static _ => new FileStpWriteOptions().ToDictionary(), directRead: DirectRead(NewRead<FileStpReadOptions>(), FileStp.Read, "stpRead"), directWrite: DirectWrite(NewWrite<FileStpWriteOptions>(), FileStp.Write, "stpWrite"));
    public static FileFormat Fbx { get; } = Native(key: "fbx", extensions: Seq(".fbx"), read: static _ => new FileFbxReadOptions().ToDictionary(), write: static profile => FbxWriteOptions(profile: profile).ToDictionary(), directRead: DirectRead(NewRead<FileFbxReadOptions>(), FileFbx.Read, "fbxRead"), directWrite: DirectWrite(static (profile, _) => FbxWriteOptions(profile: profile), FileFbx.Write, "fbxWrite"));
    public static FileFormat Ghs { get; } = Native(key: "ghs", extensions: Seq(".ghs"), export: false, read: static _ => new FileGHSReadOptions().ToDictionary(), directRead: DirectRead(NewRead<FileGHSReadOptions>(), FileGHS.Read, "ghsRead"));
    public static FileFormat Gts { get; } = Native(key: "gts", extensions: Seq(".gts"), import: false, write: static _ => new FileGtsWriteOptions().ToDictionary(), directWrite: DirectWrite(NewWrite<FileGtsWriteOptions>(), FileGts.Write, "gtsWrite"));
    public static FileFormat Iges { get; } = Native(key: "igs", extensions: Seq(".igs", ".iges"), import: false, write: static _ => new FileIgsWriteOptions().ToDictionary(), directWrite: DirectWrite(NewWrite<FileIgsWriteOptions>(), FileIgs.Write, "igsWrite"));
    public static FileFormat Lwo { get; } = Native(key: "lwo", extensions: Seq(".lwo"), read: static _ => new FileLwoReadOptions().ToDictionary(), write: static _ => new FileLwoWriteOptions().ToDictionary(), directRead: DirectRead(NewRead<FileLwoReadOptions>(), FileLwo.Read, "lwoRead"), directWrite: DirectWrite(NewWrite<FileLwoWriteOptions>(), FileLwo.Write, "lwoWrite"));
    public static FileFormat Nwd { get; } = Native(key: "nwd", extensions: Seq(".nwd"), import: false, write: static _ => new FileNwdWriteOptions().ToDictionary(), directWrite: DirectWrite(NewWrite<FileNwdWriteOptions>(), FileNwd.Write, "nwdWrite"));
    public static FileFormat Pov { get; } = Native(key: "pov", extensions: Seq(".pov"), import: false, write: static _ => new FilePovWriteOptions().ToDictionary(), directWrite: DirectWrite(NewWrite<FilePovWriteOptions>(), FilePov.Write, "povWrite"));
    public static FileFormat Sat { get; } = Native(key: "sat", extensions: Seq(".sat"), import: false, write: static _ => new FileSatWriteOptions().ToDictionary(), directWrite: DirectWrite(NewWrite<FileSatWriteOptions>(), FileSat.Write, "satWrite"));
    public static FileFormat Skp { get; } = Native(key: "skp", extensions: Seq(".skp"), read: static _ => new FileSkpReadOptions().ToDictionary(), write: static profile => SkpWriteOptions(profile: profile).ToDictionary(), directRead: DirectRead(NewRead<FileSkpReadOptions>(), FileSkp.Read, "skpRead"), directWrite: DirectWrite(static (profile, _) => SkpWriteOptions(profile: profile), FileSkp.Write, "skpWrite"));
    public static FileFormat Slc { get; } = Native(key: "slc", extensions: Seq(".slc"), import: false, directWrite: DirectWrite(NewWrite<FileSlcWriteOptions>(), FileSlc.Write, "slcWrite"));
    public static FileFormat Sw { get; } = Native(key: "sw", extensions: Seq(".sldprt", ".sldasm"), export: false, read: static _ => new FileSwReadOptions().ToDictionary(), directRead: DirectRead(NewRead<FileSwReadOptions>(), FileSW.Read, "swRead"));
    public static FileFormat Udo { get; } = Native(key: "udo", extensions: Seq(".udo"), import: false, write: static _ => new FileUdoWriteOptions().ToDictionary(), directWrite: DirectWrite(NewWrite<FileUdoWriteOptions>(), FileUdo.Write, "udoWrite"));
    public static FileFormat Vda { get; } = Native(key: "vda", extensions: Seq(".vda"), import: false, write: static _ => new FileVdaWriteOptions().ToDictionary(), directWrite: DirectWrite(NewWrite<FileVdaWriteOptions>(), FileVda.Write, "vdaWrite"));
    public static FileFormat Vrml { get; } = Native(key: "vrml", extensions: Seq(".wrl", ".vrml"), import: false, write: static profile => VrmlWriteOptions(profile: profile).ToDictionary(), directWrite: DirectWrite(static (profile, _) => VrmlWriteOptions(profile: profile), FileVrml.Write, "vrmlWrite"));
    public static FileFormat X3dv { get; } = Native(key: "x3dv", extensions: Seq(".x3dv"), import: false, write: static profile => X3dvWriteOptions(profile: profile).ToDictionary(), directWrite: DirectWrite(static (profile, _) => X3dvWriteOptions(profile: profile), FileX3dv.Write, "x3dvWrite"));
    public static FileFormat Xaml { get; } = Native(key: "xaml", extensions: Seq(".xaml"), import: false, write: static profile => new FileXamlWriteOptions { UseExistingRenderMeshes = profile.Fidelity == FileFidelity.Model }.ToDictionary());
    public static FileFormat Xt { get; } = Native(key: "x_t", extensions: Seq(".x_t", ".x_b"), import: false, write: static _ => new FileX_TWriteOptions().ToDictionary(), directWrite: DirectWrite(NewWrite<FileX_TWriteOptions>(), FileX_T.Write, "x_tWrite"));
    public static FileFormat Raw { get; } = Native(key: "raw", extensions: Seq(".raw"), read: static _ => new FileRawReadOptions().ToDictionary(), write: static _ => new FileRawWriteOptions().ToDictionary(), directRead: DirectRead(NewRead<FileRawReadOptions>(), FileRaw.Read, "rawRead"), directWrite: DirectWrite(NewWrite<FileRawWriteOptions>(), FileRaw.Write, "rawWrite"));
    public static FileFormat Txt { get; } = Native(key: "txt", extensions: Seq(".txt"), read: static _ => new FileTxtReadOptions().ToDictionary(), write: static profile => TxtWriteOptions(profile: profile).ToDictionary(), directRead: DirectRead(NewRead<FileTxtReadOptions>(), FileTxt.Read, "txtRead"), directWrite: DirectWrite(static (profile, _) => TxtWriteOptions(profile: profile), FileTxt.Write, "txtWrite"));
    public static FileFormat Csv { get; } = Native(key: "csv", extensions: Seq(".csv"), import: false, write: static profile => CsvOptions(profile: profile).ToDictionary(), directWrite: DirectWrite(static (profile, _) => CsvOptions(profile: profile), FileCsv.Write, "csvWrite"));
    public static FileFormat Gltf { get; } = Native(key: "gltf", extensions: Seq(".gltf", ".glb"), import: false, write: static profile => GltfWriteOptions(profile: profile).ToDictionary(), directWrite: DirectWrite(static (profile, _) => GltfWriteOptions(profile: profile), FileGltf.Write, "gltfWrite"));
    public static FileFormat Usd { get; } = Native(key: "usd", extensions: Seq(".usd", ".usda", ".usdz"), import: false, write: static profile => UsdWriteOptions(profile: profile).ToDictionary(), directWrite: DirectWrite(static (profile, _) => UsdWriteOptions(profile: profile), FileUsd.Write, "usdWrite"));
    public static FileFormat Pdf { get; } = new(
        key: "pdf",
        extensions: Seq(".pdf"),
        capability: FileCapability.Vector,
        read: static profile => PdfReadOptions(profile: profile).ToDictionary(),
        write: null,
        directRead: DirectRead(static (profile, _) => PdfReadOptions(profile: profile), FilePdf.Read, "pdfRead"),
        directWrite: null,
        directWriteOptions: false);
    public static FileFormat Svg { get; } = new(
        key: "svg",
        extensions: Seq(".svg"),
        capability: FileCapability.Vector,
        read: static _ => new FileSvgReadOptions().ToDictionary(),
        write: null,
        directRead: DirectRead(NewRead<FileSvgReadOptions>(), FileSvg.Read, "svgRead"),
        directWrite: null,
        directWriteOptions: false);
    public static FileFormat Png { get; } = new(key: "png", extensions: Seq(".png"), capability: FileCapability.Raster, read: null, write: null, directRead: null, directWrite: null, directWriteOptions: false);
    public static FileFormat Jpeg { get; } = new(key: "jpeg", extensions: Seq(".jpg", ".jpeg"), capability: FileCapability.Raster, read: null, write: null, directRead: null, directWrite: null, directWriteOptions: false);
    public static FileFormat Tiff { get; } = new(key: "tiff", extensions: Seq(".tif", ".tiff"), capability: FileCapability.Raster, read: null, write: null, directRead: null, directWrite: null, directWriteOptions: false);
    public static FileFormat Bmp { get; } = new(key: "bmp", extensions: Seq(".bmp"), capability: FileCapability.Raster, read: null, write: null, directRead: null, directWrite: null, directWriteOptions: false);

    public static Seq<FileFormat> Known { get; } = Seq(ThreeDm, ThreeDs, ThreeMf, Ai, Amf, Obj, Ply, Cd, Dgn, Dst, Dwg, Eps, Stl, Stp, Fbx, Ghs, Gts, Iges, Lwo, Nwd, Pov, Sat, Skp, Slc, Sw, Udo, Vda, Vrml, X3dv, Xaml, Xt, Raw, Txt, Csv, Gltf, Usd, Pdf, Svg, Png, Jpeg, Tiff, Bmp);

    private static FrozenDictionary<string, FileFormat> ByKey { get; } =
        Known.AsIterable().ToFrozenDictionary(keySelector: static format => format.Key, elementSelector: static format => format, comparer: StringComparer.OrdinalIgnoreCase);

    private static FrozenDictionary<string, FileFormat> ByExtension { get; } =
        Known.Bind(static format => format.Extensions.Map(extension => (Extension: extension, Format: format)))
            .AsIterable()
            .ToFrozenDictionary(keySelector: static item => item.Extension, elementSelector: static item => item.Format, comparer: StringComparer.OrdinalIgnoreCase);

    public static Option<FileFormat> Detect(string path) =>
        Optional(IOPath.GetExtension(path: path))
            .Bind(extension => ByExtension.TryGetValue(key: extension, value: out FileFormat? format) ? Optional(format) : Option<FileFormat>.None);

    public static Fin<FileFormat> Of(string keyOrExtension) =>
        from key in FileEndpoint.NonBlank(value: keyOrExtension, op: Op.Of(name: nameof(Of)))
        let extension = key.StartsWith('.') ? key : $".{key}"
        let clean = key.TrimStart('.')
        from format in (ByKey.TryGetValue(key: clean, value: out FileFormat? byKey)
                ? Optional(byKey)
                : ByExtension.TryGetValue(key: extension, value: out FileFormat? byExtension)
                    ? Optional(byExtension)
                    : Option<FileFormat>.None)
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
        from _ in guard(!validExtensions.IsEmpty && capability != FileCapability.None, Op.Of(name: nameof(Custom)).InvalidInput())
        from __ in guard(!ByKey.ContainsKey(key: validKey.TrimStart('.')) && !validExtensions.Exists(ByExtension.ContainsKey), Op.Of(name: nameof(Custom)).InvalidInput())
        select new FileFormat(key: validKey.TrimStart('.'), extensions: validExtensions.Distinct(), capability: capability, read: null, write: null, directRead: null, directWrite: null, directWriteOptions: false);

    public static string Filter(FilePhase phase, Seq<FileFormat> formats = default) =>
        (formats.IsEmpty ? Known : formats)
            .Filter(format => !format.Extensions.IsEmpty && (phase == FilePhase.Prompt || format.Supports(phase: phase)))
            .Map(static format => (
                Label: $"{format.Key.ToUpperInvariant()} ({string.Join(separator: ';', values: format.Extensions.Map(static extension => $"*{extension}").AsIterable())})",
                Patterns: string.Join(separator: ';', values: format.Extensions.Map(static extension => $"*{extension}").AsIterable())))
            .Distinct()
            switch {
                Seq<(string Label, string Patterns)> rows when !rows.IsEmpty =>
                    string.Join(separator: '|', values: (Seq((
                            Label: "All supported files",
                            Patterns: string.Join(separator: ';', values: rows.Bind(static row => toSeq(row.Patterns.Split(';'))).Distinct().AsIterable())))
                        + rows).Map(static row => $"{row.Label}|{row.Patterns}").AsIterable()),
                _ => "All files (*.*)|*.*",
            };

    public string EnsureExtension(string path) =>
        Extensions.Exists(extension => string.Equals(a: extension, b: IOPath.GetExtension(path: path), comparisonType: StringComparison.OrdinalIgnoreCase)) switch {
            true => path,
            false => IOPath.ChangeExtension(path: path, extension: Extensions[0].TrimStart('.')),
        };

    internal static Fin<Unit> NativeBool(Func<bool> run, Op op) =>
        op.Catch(() => op.Confirm(success: run()));

    internal static Fin<Unit> NativeWrite(WriteFileResult result, Op op) =>
        result switch {
            WriteFileResult.Success => Fin.Succ(value: unit),
            WriteFileResult.Cancel => Fin.Fail<Unit>(error: new Fault.Cancelled()),
            WriteFileResult.Failure => Fin.Fail<Unit>(error: op.InvalidResult()),
            _ => Fin.Fail<Unit>(error: op.InvalidResult()),
        };

    private static FileFormat Native(
        string key,
        Seq<string> extensions,
        bool import = true,
        bool export = true,
        bool archive = false,
        Func<FileProfile, ArchivableDictionary>? read = null,
        Func<FileProfile, ArchivableDictionary>? write = null,
        Func<FileProfile, FileReadOptions, RhinoDoc, FileEndpoint, Fin<Unit>>? directRead = null,
        Func<FileProfile, FileWriteOptions, RhinoDoc, FileEndpoint, Fin<Unit>>? directWrite = null,
        bool directWriteOptions = false) =>
        new(key: key, extensions: extensions,
            capability: archive
                ? FileCapability.File3dm
                : (import ? FileCapability.Import : FileCapability.None) | (export ? FileCapability.Export : FileCapability.None),
            read: read, write: write, directRead: directRead, directWrite: directWrite, directWriteOptions: directWriteOptions);

    private delegate bool NativeReadCall<in TOptions>(string path, RhinoDoc doc, TOptions options);
    private delegate bool NativeWriteCall<in TOptions>(string path, RhinoDoc doc, TOptions options);
    private delegate WriteFileResult NativeResultWriteCall<in TOptions>(string filename, RhinoDoc doc, TOptions options);

    private static Func<FileProfile, FileReadOptions, TOptions> NewRead<TOptions>() where TOptions : new() =>
        static (_, _) => new TOptions();

    private static Func<FileProfile, FileWriteOptions, TOptions> NewWrite<TOptions>() where TOptions : new() =>
        static (_, _) => new TOptions();

    private static Func<FileProfile, FileReadOptions, RhinoDoc, FileEndpoint, Fin<Unit>> DirectRead<TOptions>(Func<FileProfile, FileReadOptions, TOptions> options, NativeReadCall<TOptions> read, string name) =>
        (profile, readOptions, document, source) => NativeBool(run: () => read(path: source.Path, doc: document, options: options(arg1: profile, arg2: readOptions)), op: Op.Of(name: name));

    private static Func<FileProfile, FileWriteOptions, RhinoDoc, FileEndpoint, Fin<Unit>> DirectWrite<TOptions>(Func<FileProfile, FileWriteOptions, TOptions> options, NativeWriteCall<TOptions> write, string name) =>
        (profile, writeOptions, document, target) => NativeBool(run: () => write(path: target.Path, doc: document, options: options(arg1: profile, arg2: writeOptions)), op: Op.Of(name: name));

    private static Func<FileProfile, FileWriteOptions, RhinoDoc, FileEndpoint, Fin<Unit>> DirectWriteResult<TOptions>(Func<FileProfile, FileWriteOptions, TOptions> options, NativeResultWriteCall<TOptions> write, string name) =>
        (profile, writeOptions, document, target) => NativeWrite(result: write(filename: target.Path, doc: document, options: options(arg1: profile, arg2: writeOptions)), op: Op.Of(name: name));

    private static File3dsWriteOptions ThreeDsWriteOptions(FileProfile profile) =>
        new() { SaveViews = profile.Fidelity == FileFidelity.Model, SaveLights = profile.Fidelity == FileFidelity.Model };

    private static FileAiWriteOptions AiWriteOptions(FileProfile profile) =>
        new() { PreserveModelScale = profile.Fidelity == FileFidelity.Model, OrderLayers = profile.Grouping == FileGrouping.Layer || profile.Sort == FileSort.Layer };

    private static FileObjWriteOptions ObjWriteOptions(FileProfile profile, FileWriteOptions options) =>
        new(options) {
            ObjectType = profile.Fidelity == FileFidelity.Model && !options.WriteGeometryOnly ? FileObjWriteOptions.GeometryType.Nurbs : FileObjWriteOptions.GeometryType.Mesh,
            ExportObjectNames = profile.Grouping == FileGrouping.ObjectName ? FileObjWriteOptions.ObjObjectNames.ObjectAsObject : FileObjWriteOptions.ObjObjectNames.NoObjects,
            ExportGroupNameLayerNames = profile.Grouping switch {
                FileGrouping.Layer => FileObjWriteOptions.ObjGroupNames.LayerAsGroup,
                FileGrouping.Block => FileObjWriteOptions.ObjGroupNames.GroupAsGroup,
                _ => FileObjWriteOptions.ObjGroupNames.NoGroups,
            },
            ExportMaterialDefinitions = profile.Resources != FileResourcePolicy.Reference && options.WriteUserData,
            UseDisplayColorForMaterial = profile.Resources != FileResourcePolicy.Reference,
            ExportTcs = profile.Resources != FileResourcePolicy.Reference,
            ExportNormals = profile.Fidelity != FileFidelity.GeometryOnly,
            UseRenderMeshes = profile.Fidelity == FileFidelity.Small || options.IncludeRenderMeshes,
            SortObjGroups = profile.Sort is FileSort.Layer or FileSort.Block,
            MergeNestedGroupingNames = profile.Grouping == FileGrouping.Layer,
        };

    private static FilePlyWriteOptions PlyWriteOptions(FileProfile profile, FileWriteOptions options) =>
        new(options) {
            ExportASCII = profile.Fidelity != FileFidelity.Small,
            ExportDoubles = profile.Fidelity == FileFidelity.Model,
            ExportNormals = profile.Fidelity != FileFidelity.GeometryOnly,
            ExportColors = profile.Resources != FileResourcePolicy.Reference,
            ExportMaterial = profile.Resources == FileResourcePolicy.Embed,
        };

    private static FileDwgWriteOptions DwgWriteOptions(FileProfile profile) =>
        new() {
            FullLayerPath = profile.Grouping == FileGrouping.Layer,
            ExportSurfacesAs = profile.Fidelity == FileFidelity.GeometryOnly ? FileDwgWriteOptions.ExportSurfaceMode.Meshes : FileDwgWriteOptions.ExportSurfaceMode.Curves,
            UseLWPolylines = profile.Fidelity != FileFidelity.Model,
            ColorMethod = profile.Resources == FileResourcePolicy.Reference ? FileDwgWriteOptions.ColorMethodType.ACI : FileDwgWriteOptions.ColorMethodType.RGB,
            UseColor = profile.Sort == FileSort.Material ? FileDwgWriteOptions.UseColorType.USEPRINT : FileDwgWriteOptions.UseColorType.USEDISPLAY,
        };

    private static FileStlWriteOptions StlWriteOptions() =>
        new() { BinaryFile = true };

    private static FileFbxWriteOptions FbxWriteOptions(FileProfile profile) =>
        new() {
            SaveObjectsAs = profile.Fidelity == FileFidelity.Model ? FileFbxWriteOptions.ObjectType.Nurbs : FileFbxWriteOptions.ObjectType.Mesh,
            SaveViews = profile.Fidelity == FileFidelity.Model,
            SaveLights = profile.Fidelity == FileFidelity.Model,
            SaveVertexNormals = profile.Fidelity != FileFidelity.GeometryOnly,
        };

    private static FileSkpWriteOptions SkpWriteOptions(FileProfile profile) =>
        new() { GroupObjects = profile.Grouping is FileGrouping.Layer or FileGrouping.Block };

    private static FileVrmlWriteOptions VrmlWriteOptions(FileProfile profile) =>
        new() { ExportTextureCoordinates = profile.Resources != FileResourcePolicy.Reference, ExportVertexNormals = profile.Fidelity == FileFidelity.Model };

    private static FileX3dvWriteOptions X3dvWriteOptions(FileProfile profile) =>
        new() { ExportTextureCoordinates = profile.Resources != FileResourcePolicy.Reference, ExportVertexNormals = profile.Fidelity == FileFidelity.Model };

    private static FileTxtWriteOptions TxtWriteOptions(FileProfile profile) =>
        new() { SurroundWithDoubleQuotes = profile.Grouping != FileGrouping.Document };

    private static FileGltfWriteOptions GltfWriteOptions(FileProfile profile) =>
        new() {
            ExportMaterials = profile.Resources != FileResourcePolicy.Reference,
            ExportLayers = profile.Grouping == FileGrouping.Layer,
            ExportTextureCoordinates = profile.Resources != FileResourcePolicy.Reference,
            ExportVertexNormals = profile.Fidelity != FileFidelity.GeometryOnly,
            UseDracoCompression = profile.Fidelity == FileFidelity.Small,
            DracoCompressionLevel = profile.Fidelity == FileFidelity.Small ? 10 : 6,
            DracoQuantizationBitsPosition = profile.Fidelity == FileFidelity.Small ? 11 : 14,
            DracoQuantizationBitsNormal = profile.Fidelity == FileFidelity.Small ? 8 : 10,
            DracoQuantizationBitsTextureCoordinate = profile.Fidelity == FileFidelity.Small ? 10 : 12,
        };

    private static FileUsdWriteOptions UsdWriteOptions(FileProfile profile) =>
        new() {
            ForceMeshes = profile.Fidelity != FileFidelity.Model,
            IncludeUserStrings = profile.Grouping == FileGrouping.UserString,
            BlockHandling = profile.Resources switch {
                FileResourcePolicy.Embed => USDExportBlockHandling.Embedded,
                FileResourcePolicy.Copy => USDExportBlockHandling.SeparateFiles,
                _ => USDExportBlockHandling.Ignore,
            },
            DefaultLayer = profile.Grouping == FileGrouping.Layer ? "Layers" : "World",
            ModelName = profile.Grouping == FileGrouping.Document ? string.Empty : "Model",
        };

    private static FilePdfReadOptions PdfReadOptions(FileProfile profile) =>
        new() {
            PreserveModelScale = profile.Fidelity == FileFidelity.Model,
            ImportFillsAsHatches = profile.Fidelity != FileFidelity.GeometryOnly,
            LoadText = profile.Fidelity == FileFidelity.Model || profile.Grouping == FileGrouping.UserString,
        };

    private static FileCsvWriteOptions CsvOptions(FileProfile profile) {
        bool layer = profile.Grouping == FileGrouping.Layer || profile.Sort == FileSort.Layer;
        bool group = profile.Grouping == FileGrouping.Block || profile.Sort == FileSort.Block;
        bool material = profile.Grouping == FileGrouping.Material || profile.Sort == FileSort.Material;
        bool user = profile.Grouping == FileGrouping.UserString;
        bool measured = profile.Fidelity != FileFidelity.GeometryOnly;
        return new() {
            Header = true,
            LayerName = layer,
            LayerIndex = layer,
            LayerColor = layer,
            LayerHierarchy = layer,
            GroupName = group,
            GroupIndexes = group,
            ObjectName = profile.Grouping == FileGrouping.ObjectName || profile.Sort == FileSort.ObjectName,
            ObjectID = true,
            ObjectColor = profile.Sort == FileSort.ObjectType,
            ObjectMaterial = material,
            ObjectDescription = user,
            SurroundPointsWithDoubleQuotes = true,
            Length = measured,
            Perimeter = measured,
            Area = measured,
            Volume = measured,
            AreaCentroid = measured,
            VolumeCentroid = measured,
            AreaMoments = measured,
            VolumeMoments = measured,
            CumulativeMassProperties = measured,
            AttributesKeys = user,
            AttributesTexts = user,
            ObjectKeys = user,
            ObjectsTexts = user,
        };
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class FileFormatProjection {
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
        from imported in op.Catch(() => {
            using FileReadOptions options = ReadOptions(endpoint: source, profile: profile);
            return format.Read(options: options, document: document, source: source, profile: profile);
        })
        select imported;

    internal static Fin<Unit> Write(RhinoDoc document, Option<FileEndpoint> target, FileProfile profile, FilePhase phase, bool selected, Op op) =>
        phase == FilePhase.Save
            ? FileFormat.NativeBool(run: document.Save, op: op)
            : target.ToFin(Fail: op.InvalidInput()).Bind(endpoint =>
                phase == FilePhase.SaveAs ? op.Catch(() => {
                    using FileWriteOptions options = WriteOptions(endpoint: endpoint, profile: profile, phase: FilePhase.SaveAs, selected: false, updatePath: true);
                    return FileFormat.NativeBool(run: () => document.WriteFile(path: endpoint.Path, options: options), op: op);
                })
                : phase == FilePhase.Write3dmFile ? op.Catch(() => {
                    using FileWriteOptions options = WriteOptions(endpoint: endpoint, profile: profile, phase: FilePhase.Write3dmFile, selected: selected, updatePath: false);
                    return FileFormat.NativeBool(run: () => document.Write3dmFile(path: endpoint.Path, options: options), op: op);
                })
                : phase == FilePhase.SaveTemplate ? FileFormat.NativeBool(run: () => document.SaveAsTemplate(file3dmTemplatePath: endpoint.Path, version: endpoint.Write.Normalized.Version), op: op)
                : phase == FilePhase.Export || phase == FilePhase.WriteFile ? Require(endpoint: endpoint, profile: profile, phase: phase, op: op).Bind(format => op.Catch(() => {
                    using FileWriteOptions options = WriteOptions(endpoint: endpoint, profile: profile, phase: phase, selected: selected, updatePath: phase == FilePhase.WriteFile);
                    return phase == FilePhase.Export
                        ? format.Export(options: options, document: document, target: endpoint, profile: profile)
                        : format.Write(options: options, document: document, target: endpoint, profile: profile);
                }))
                : Fin.Fail<Unit>(error: op.InvalidInput()));

    internal static File3dmWriteOptions ArchiveWriteOptions(ArchiveProfile profile) {
        FileWritePolicy write = profile.Write.Normalized;
        File3dmWriteOptions options = new() { Version = write.Version, SaveUserData = write.WriteUserData };
        options.EnableRenderMeshes(objectType: ObjectType.AnyObject, enable: write.IncludeRenderMeshes);
        options.EnableAnalysisMeshes(objectType: ObjectType.AnyObject, enable: write.IncludeRenderMeshes);
        return options;
    }

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
        FileWritePolicy write = endpoint.Write.Normalized;
        FileWriteOptions options = new() {
            UpdateDocumentPath = updatePath,
            WriteSelectedObjectsOnly = selected,
            IncludeRenderMeshes = write.IncludeRenderMeshes,
            IncludePreviewImage = write.IncludePreviewImage,
            IncludeBitmapTable = write.IncludeBitmapTable,
            IncludeHistory = write.IncludeHistory,
            SuppressDialogBoxes = true,
            SuppressAllInput = true,
            WriteGeometryOnly = profile.Fidelity == FileFidelity.GeometryOnly || write.GeometryOnly,
            WriteUserData = write.WriteUserData,
            FileVersion = write.Version,
            UseCompression = write.UseCompression,
            CreateBackupFiles = write.CreateBackupFiles,
            CreateOtherBackupFiles = write.CreateOtherBackupFiles,
            Xform = write.Xform.IfNone(Transform.Identity),
        };
        _ = Dictionary(endpoint: endpoint, profile: profile, phase: phase, op: Op.Of(name: nameof(WriteOptions))).Map(options.OptionsDictionary.AddContentsFrom);
        return options;
    }
}
