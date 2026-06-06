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
public sealed partial record FileFormat {
    private static Seq<FileFormat> BuiltIn { get; } = Seq(
        Native(key: "3dm", extensions: Seq(".3dm"), archive: true),
        Native(key: "3ds", extensions: Seq(".3ds"), read: static _ => new File3dsReadOptions().ToDictionary(), write: static profile => ThreeDsWriteOptions(profile: profile).ToDictionary(), directRead: DirectRead(NewRead<File3dsReadOptions>(), File3ds.Read, "3dsRead"), directWrite: DirectWrite(static (profile, _) => ThreeDsWriteOptions(profile: profile), File3ds.Write, "3dsWrite")),
        Native(key: "3mf", extensions: Seq(".3mf"), import: false, write: static _ => new File3mfWriteOptions().ToDictionary(), directWrite: DirectWrite(NewWrite<File3mfWriteOptions>(), File3mf.Write, "3mfWrite")),
        Native(key: "ai", extensions: Seq(".ai"), read: static profile => AiReadOptions(profile: profile).ToDictionary(), write: static profile => AiWriteOptions(profile: profile).ToDictionary(), directRead: DirectRead(static (profile, _) => AiReadOptions(profile: profile), FileAi.Read, "aiRead"), directWrite: DirectWrite(static (profile, _) => AiWriteOptions(profile: profile), FileAi.Write, "aiWrite"), scale: FileCapability.Import | FileCapability.Export),
        Native(key: "amf", extensions: Seq(".amf"), import: false, write: static _ => new FileAmfWriteOptions().ToDictionary(), directWrite: DirectWrite(NewWrite<FileAmfWriteOptions>(), FileAmf.Write, "amfWrite")),
        Native(key: "obj", extensions: Seq(".obj"), directRead: DirectRead(static (_, options) => new FileObjReadOptions(options), FileObj.Read, "objRead"), directWrite: DirectWriteResult(ObjWriteOptions, FileObj.Write, "objWrite"), directWriteOptions: true),
        Native(key: "ply", extensions: Seq(".ply"), read: static _ => new FilePlyReadOptions().ToDictionary(), directRead: DirectRead(NewRead<FilePlyReadOptions>(), FilePly.Read, "plyRead"), directWrite: DirectWriteResult(PlyWriteOptions, FilePly.Write, "plyWrite"), directWriteOptions: true),
        Native(key: "cd", extensions: Seq(".cd"), import: false, write: static _ => new FileCdWriteOptions().ToDictionary(), directWrite: DirectWrite(NewWrite<FileCdWriteOptions>(), FileCd.Write, "cdWrite")),
        Native(key: "dgn", extensions: Seq(".dgn"), export: false, read: static _ => new FileDgnReadOptions().ToDictionary(), directRead: DirectRead(NewRead<FileDgnReadOptions>(), FileDgn.Read, "dgnRead")),
        Native(key: "dst", extensions: Seq(".dst"), export: false, read: static _ => new FileDstReadOptions().ToDictionary(), directRead: DirectRead(NewRead<FileDstReadOptions>(), FileDst.Read, "dstRead")),
        Native(key: "dwg", extensions: Seq(".dwg", ".dxf"), read: static _ => new FileDwgReadOptions().ToDictionary(), write: static profile => DwgWriteOptions(profile: profile).ToDictionary(), directRead: DirectRead(NewRead<FileDwgReadOptions>(), FileDwg.Read, "dwgRead"), directWrite: DirectWrite(static (profile, _) => DwgWriteOptions(profile: profile), FileDwg.Write, "dwgWrite")),
        Native(key: "eps", extensions: Seq(".eps"), export: false, read: static profile => EpsReadOptions(profile: profile).ToDictionary(), directRead: DirectRead(static (profile, _) => EpsReadOptions(profile: profile), FileEps.Read, "epsRead"), scale: FileCapability.Import),
        Native(key: "stl", extensions: Seq(".stl"), read: static _ => new FileStlReadOptions().ToDictionary(), write: static _ => StlWriteOptions().ToDictionary(), directRead: DirectRead(NewRead<FileStlReadOptions>(), FileStl.Read, "stlRead"), directWrite: DirectWrite(static (_, _) => StlWriteOptions(), FileStl.Write, "stlWrite")),
        Native(key: "stp", extensions: Seq(".stp", ".step"), read: static _ => new FileStpReadOptions().ToDictionary(), write: static _ => new FileStpWriteOptions().ToDictionary(), directRead: DirectRead(NewRead<FileStpReadOptions>(), FileStp.Read, "stpRead"), directWrite: DirectWrite(NewWrite<FileStpWriteOptions>(), FileStp.Write, "stpWrite")),
        Native(key: "fbx", extensions: Seq(".fbx"), read: static _ => new FileFbxReadOptions().ToDictionary(), write: static profile => FbxWriteOptions(profile: profile).ToDictionary(), directRead: DirectRead(NewRead<FileFbxReadOptions>(), FileFbx.Read, "fbxRead"), directWrite: DirectWrite(static (profile, _) => FbxWriteOptions(profile: profile), FileFbx.Write, "fbxWrite")),
        Native(key: "ghs", extensions: Seq(".ghs"), export: false, read: static _ => new FileGHSReadOptions().ToDictionary(), directRead: DirectRead(NewRead<FileGHSReadOptions>(), FileGHS.Read, "ghsRead")),
        Native(key: "gts", extensions: Seq(".gts"), import: false, write: static _ => new FileGtsWriteOptions().ToDictionary(), directWrite: DirectWrite(NewWrite<FileGtsWriteOptions>(), FileGts.Write, "gtsWrite")),
        Native(key: "igs", extensions: Seq(".igs", ".iges"), import: false, write: static _ => new FileIgsWriteOptions().ToDictionary(), directWrite: DirectWrite(NewWrite<FileIgsWriteOptions>(), FileIgs.Write, "igsWrite")),
        Native(key: "lwo", extensions: Seq(".lwo"), read: static _ => new FileLwoReadOptions().ToDictionary(), write: static _ => new FileLwoWriteOptions().ToDictionary(), directRead: DirectRead(NewRead<FileLwoReadOptions>(), FileLwo.Read, "lwoRead"), directWrite: DirectWrite(NewWrite<FileLwoWriteOptions>(), FileLwo.Write, "lwoWrite")),
        Native(key: "nwd", extensions: Seq(".nwd"), import: false, write: static _ => new FileNwdWriteOptions().ToDictionary(), directWrite: DirectWrite(NewWrite<FileNwdWriteOptions>(), FileNwd.Write, "nwdWrite")),
        Native(key: "pov", extensions: Seq(".pov"), import: false, write: static _ => new FilePovWriteOptions().ToDictionary(), directWrite: DirectWrite(NewWrite<FilePovWriteOptions>(), FilePov.Write, "povWrite")),
        Native(key: "sat", extensions: Seq(".sat"), import: false, write: static _ => new FileSatWriteOptions().ToDictionary(), directWrite: DirectWrite(NewWrite<FileSatWriteOptions>(), FileSat.Write, "satWrite")),
        Native(key: "skp", extensions: Seq(".skp"), read: static _ => new FileSkpReadOptions().ToDictionary(), write: static profile => SkpWriteOptions(profile: profile).ToDictionary(), directRead: DirectRead(NewRead<FileSkpReadOptions>(), FileSkp.Read, "skpRead"), directWrite: DirectWrite(static (profile, _) => SkpWriteOptions(profile: profile), FileSkp.Write, "skpWrite")),
        Native(key: "slc", extensions: Seq(".slc"), import: false, directWrite: DirectWrite(NewWrite<FileSlcWriteOptions>(), FileSlc.Write, "slcWrite")),
        Native(key: "sw", extensions: Seq(".sldprt", ".sldasm"), export: false, read: static _ => new FileSwReadOptions().ToDictionary(), directRead: DirectRead(NewRead<FileSwReadOptions>(), FileSW.Read, "swRead")),
        Native(key: "udo", extensions: Seq(".udo"), import: false, write: static _ => new FileUdoWriteOptions().ToDictionary(), directWrite: DirectWrite(NewWrite<FileUdoWriteOptions>(), FileUdo.Write, "udoWrite")),
        Native(key: "vda", extensions: Seq(".vda"), import: false, write: static _ => new FileVdaWriteOptions().ToDictionary(), directWrite: DirectWrite(NewWrite<FileVdaWriteOptions>(), FileVda.Write, "vdaWrite")),
        Native(key: "vrml", extensions: Seq(".wrl", ".vrml"), import: false, write: static profile => VrmlWriteOptions(profile: profile).ToDictionary(), directWrite: DirectWrite(static (profile, _) => VrmlWriteOptions(profile: profile), FileVrml.Write, "vrmlWrite")),
        Native(key: "x3dv", extensions: Seq(".x3dv"), import: false, write: static profile => X3dvWriteOptions(profile: profile).ToDictionary(), directWrite: DirectWrite(static (profile, _) => X3dvWriteOptions(profile: profile), FileX3dv.Write, "x3dvWrite")),
        Native(key: "xaml", extensions: Seq(".xaml"), import: false, write: static profile => new FileXamlWriteOptions { UseExistingRenderMeshes = profile.Fidelity.IsModel }.ToDictionary()),
        Native(key: "x_t", extensions: Seq(".x_t", ".x_b"), import: false, write: static _ => new FileX_TWriteOptions().ToDictionary(), directWrite: DirectWrite(NewWrite<FileX_TWriteOptions>(), FileX_T.Write, "x_tWrite")),
        Native(key: "raw", extensions: Seq(".raw"), read: static _ => new FileRawReadOptions().ToDictionary(), write: static _ => new FileRawWriteOptions().ToDictionary(), directRead: DirectRead(NewRead<FileRawReadOptions>(), FileRaw.Read, "rawRead"), directWrite: DirectWrite(NewWrite<FileRawWriteOptions>(), FileRaw.Write, "rawWrite")),
        Native(key: "txt", extensions: Seq(".txt"), read: static _ => new FileTxtReadOptions().ToDictionary(), write: static profile => TxtWriteOptions(profile: profile).ToDictionary(), directRead: DirectRead(NewRead<FileTxtReadOptions>(), FileTxt.Read, "txtRead"), directWrite: DirectWrite(static (profile, _) => TxtWriteOptions(profile: profile), FileTxt.Write, "txtWrite")),
        Native(key: "csv", extensions: Seq(".csv"), import: false, write: static profile => CsvOptions(profile: profile).ToDictionary(), directWrite: DirectWrite(static (profile, _) => CsvOptions(profile: profile), FileCsv.Write, "csvWrite")),
        Native(key: "gltf", extensions: Seq(".gltf", ".glb"), import: false, write: static profile => GltfWriteOptions(profile: profile).ToDictionary(), directWrite: DirectWrite(static (profile, _) => GltfWriteOptions(profile: profile), FileGltf.Write, "gltfWrite")),
        Native(key: "usd", extensions: Seq(".usd", ".usda", ".usdz"), import: false, write: static profile => UsdWriteOptions(profile: profile).ToDictionary(), directWrite: DirectWrite(static (profile, _) => UsdWriteOptions(profile: profile), FileUsd.Write, "usdWrite")),
        new(key: "pdf", extensions: Seq(".pdf"), capability: FileCapability.Vector, read: static profile => PdfReadOptions(profile: profile).ToDictionary(), write: null, directRead: DirectRead(static (profile, _) => PdfReadOptions(profile: profile), FilePdf.Read, "pdfRead"), directWrite: null, directWriteOptions: false, scale: FileCapability.Import),
        new(key: "svg", extensions: Seq(".svg"), capability: FileCapability.Vector, scale: FileCapability.None, read: static _ => new FileSvgReadOptions().ToDictionary(), write: null, directRead: DirectRead(NewRead<FileSvgReadOptions>(), FileSvg.Read, "svgRead"), directWrite: null, directWriteOptions: false),
        new(key: "png", extensions: Seq(".png"), capability: FileCapability.Raster, scale: FileCapability.None, read: null, write: null, directRead: null, directWrite: null, directWriteOptions: false),
        new(key: "jpeg", extensions: Seq(".jpg", ".jpeg"), capability: FileCapability.Raster, scale: FileCapability.None, read: null, write: null, directRead: null, directWrite: null, directWriteOptions: false),
        new(key: "tiff", extensions: Seq(".tif", ".tiff"), capability: FileCapability.Raster, scale: FileCapability.None, read: null, write: null, directRead: null, directWrite: null, directWriteOptions: false),
        new(key: "bmp", extensions: Seq(".bmp"), capability: FileCapability.Raster, scale: FileCapability.None, read: null, write: null, directRead: null, directWrite: null, directWriteOptions: false));

    private static readonly Atom<HashMap<string, FileFormat>> CustomCell = Atom(HashMap<string, FileFormat>());

    private static readonly FrozenSet<string> ReservedKeys = new[] { "JSON" }.ToFrozenSet(comparer: StringComparer.OrdinalIgnoreCase);

    private static readonly FrozenSet<string> ReservedExtensions = new[] { ".json" }.ToFrozenSet(comparer: StringComparer.OrdinalIgnoreCase);

    private static FrozenDictionary<string, FileFormat> ByKey { get; } =
        BuiltIn.AsIterable().ToFrozenDictionary(keySelector: static format => format.Key, elementSelector: static format => format, comparer: StringComparer.OrdinalIgnoreCase);

    private static FrozenDictionary<string, FileFormat> ByExtension { get; } =
        BuiltIn.Bind(static format => format.Extensions.Map(extension => (Extension: extension, Format: format)))
            .AsIterable()
            .ToFrozenDictionary(keySelector: static item => item.Extension, elementSelector: static item => item.Format, comparer: StringComparer.OrdinalIgnoreCase);

    private readonly Func<FileProfile, ArchivableDictionary>? readBuilder;

    private readonly Func<FileProfile, ArchivableDictionary>? writeBuilder;

    private readonly Func<FileProfile, FileReadOptions, RhinoDoc, FileEndpoint, Fin<Unit>>? directReadCall;

    private readonly Func<FileProfile, FileWriteOptions, RhinoDoc, FileEndpoint, Fin<Unit>>? directWriteCall;

    private readonly bool directWriteOptions;

    private readonly FileCapability scale;

    private FileFormat(string key, Seq<string> extensions, FileCapability capability, FileCapability scale,
        Func<FileProfile, ArchivableDictionary>? read,
        Func<FileProfile, ArchivableDictionary>? write,
        Func<FileProfile, FileReadOptions, RhinoDoc, FileEndpoint, Fin<Unit>>? directRead,
        Func<FileProfile, FileWriteOptions, RhinoDoc, FileEndpoint, Fin<Unit>>? directWrite,
        bool directWriteOptions) {
        Key = key;
        Extensions = extensions;
        Capability = capability;
        this.scale = scale;
        readBuilder = read;
        writeBuilder = write;
        directReadCall = directRead;
        directWriteCall = directWrite;
        this.directWriteOptions = directWriteOptions;
    }

    public string Key { get; }

    public Seq<string> Extensions { get; }

    internal FileCapability Capability { get; }

    public static Seq<FileFormat> Known => BuiltIn + toSeq(CustomCell.Value.Values);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public sealed partial record FileFormat {
    public static Option<FileFormat> Detect(string path) =>
        Optional(IOPath.GetExtension(path: path))
            .Bind(extension => ByExtension.TryGetValue(key: extension, value: out FileFormat? format)
                ? Optional(format)
                : CustomByExtension(extension: extension));

    public static Fin<FileFormat> Of(string keyOrExtension) =>
        from key in FileEndpoint.NonBlank(value: keyOrExtension, op: Op.Of(name: nameof(Of)))
        let extension = NormalizeExtension(value: key)
        let clean = NormalizeKey(value: key)
        from format in (ByKey.TryGetValue(key: clean, value: out FileFormat? byKey)
                ? Optional(byKey)
                : ByExtension.TryGetValue(key: extension, value: out FileFormat? byExtension)
                    ? Optional(byExtension)
                    : CustomFormat(key: clean, extension: extension))
            .ToFin(Fail: Op.Of(name: nameof(Of)).InvalidInput())
        select format;

    public static Fin<FileFormat> Custom(string key, Seq<string> extensions, FileCapability capability,
        FileCapability scale = FileCapability.None,
        Func<FileProfile, FileReadOptions, RhinoDoc, FileEndpoint, Fin<Unit>>? read = null,
        Func<FileProfile, FileWriteOptions, RhinoDoc, FileEndpoint, Fin<Unit>>? write = null) =>
        from validKey in FileEndpoint.NonBlank(value: key, op: Op.Of(name: nameof(Custom))).Map(static k => NormalizeKey(value: k))
        from exts in extensions.TraverseM(e => FileEndpoint.NonBlank(value: e, op: Op.Of(name: nameof(Custom)))
            .Map(static t => NormalizeExtension(value: t))).As().Map(static s => s.Distinct())
        from _ in guard(!exts.IsEmpty && capability != FileCapability.None
            && !ReservedKeys.Contains(item: validKey) && !exts.Exists(ReservedExtensions.Contains)
            && !ByKey.ContainsKey(key: validKey) && !exts.Exists(ByExtension.ContainsKey)
            && !exts.Exists(extension => CustomByExtension(extension: extension).IsSome)
            && CustomCell.Value.Find(key: validKey).IsNone, Op.Of(name: nameof(Custom)).InvalidInput())
        let format = new FileFormat(key: validKey, extensions: exts, capability: capability, scale: scale,
            read: null, write: null, directRead: read, directWrite: write, directWriteOptions: write is not null)
        from __ in Fin.Succ(value: Op.Side(() => CustomCell.Swap(map => map.AddOrUpdate(key: validKey, value: format))))
        select format;

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

    internal static Fin<Unit> Write(RhinoDoc document, Option<FileEndpoint> target, FileProfile profile, FilePhase phase, bool selected, Op op) =>
        phase == FilePhase.Save
            ? NativeBool(run: document.Save, op: op)
            : from endpoint in target.ToFin(Fail: op.InvalidInput())
              from result in phase switch {
                  var p when p == FilePhase.SaveAs => op.Catch(() => {
                      using FileWriteOptions opts = WriteOptions(endpoint: endpoint, profile: profile,
                          phase: FilePhase.SaveAs, selected: false, updatePath: true);
                      return NativeBool(run: () => document.WriteFile(path: endpoint.Path, options: opts), op: op);
                  }),
                  var p when p == FilePhase.Write3dmFile => op.Catch(() => {
                      using FileWriteOptions opts = WriteOptions(endpoint: endpoint, profile: profile,
                          phase: FilePhase.Write3dmFile, selected: selected, updatePath: false);
                      return NativeBool(run: () => document.Write3dmFile(path: endpoint.Path, options: opts), op: op);
                  }),
                  var p when p == FilePhase.SaveTemplate =>
                      NativeBool(run: () => document.SaveAsTemplate(file3dmTemplatePath: endpoint.Path, version: endpoint.Write.Normalized.Version), op: op),
                  var p when p == FilePhase.Export || p == FilePhase.WriteFile =>
                      from format in Require(endpoint: endpoint, profile: profile, phase: phase, op: op)
                      from written in op.Catch(() => {
                          using FileWriteOptions opts = WriteOptions(endpoint: endpoint, profile: profile, phase: phase,
                              selected: selected, updatePath: phase == FilePhase.WriteFile);
                          return phase == FilePhase.Export
                              ? format.Export(options: opts, document: document, target: endpoint, profile: profile)
                              : format.Write(options: opts, document: document, target: endpoint, profile: profile);
                      })
                      select written,
                  _ => Fin.Fail<Unit>(error: op.InvalidInput()),
              }
              select result;

    internal Fin<Unit> Export(FileWriteOptions options, RhinoDoc document, FileEndpoint target, FileProfile profile) =>
        (Is(key: "3dm"), options.WriteSelectedObjectsOnly, directWriteOptions, directWriteCall) switch {
            (true, _, _, _) => NativeBool(run: () => document.Write3dmFile(path: target.Path, options: options), op: Op.Of(name: $"{Key}Export")),
            (false, true, true, Func<FileProfile, FileWriteOptions, RhinoDoc, FileEndpoint, Fin<Unit>> write) => write(arg1: profile, arg2: options, arg3: document, arg4: target),
            (false, true, _, _) => NativeBool(run: () => document.ExportSelected(filePath: target.Path, options: options.OptionsDictionary), op: Op.Of(name: $"{Key}Export")),
            (false, false, true, Func<FileProfile, FileWriteOptions, RhinoDoc, FileEndpoint, Fin<Unit>> write) => write(arg1: profile, arg2: options, arg3: document, arg4: target),
            _ => NativeBool(run: () => document.Export(filePath: target.Path, options: options.OptionsDictionary), op: Op.Of(name: $"{Key}Export")),
        };

    internal Option<ArchivableDictionary> Dictionary(FileProfile profile, FilePhase phase) =>
        (phase == FilePhase.Headless || phase == FilePhase.Import) && readBuilder is not null ? Some(readBuilder(arg: profile))
        : (phase == FilePhase.Export || phase == FilePhase.WriteFile) && writeBuilder is not null ? Some(writeBuilder(arg: profile))
        : Option<ArchivableDictionary>.None;

    internal static Fin<ArchivableDictionary> Dictionary(FileEndpoint endpoint, FileProfile profile, FilePhase phase, Op op) =>
        from dictionary in Resolve(endpoint: endpoint, profile: profile).Case switch {
            FileFormat format when format.Supports(phase: phase) =>
                from _ in format.Validate(profile: profile, phase: phase, op: op)
                select format.Dictionary(profile: profile, phase: phase).IfNone(new ArchivableDictionary()),
            FileFormat => Fin.Fail<ArchivableDictionary>(error: op.InvalidInput()),
            _ when profile.Scale.IsSome => Fin.Fail<ArchivableDictionary>(error: op.InvalidInput()),
            _ => Fin.Succ(value: new ArchivableDictionary()),
        }
        select dictionary;

    internal bool Supports(FilePhase phase) =>
        phase.Allows(capability: Capability);

    internal Fin<Unit> Validate(FileProfile profile, FilePhase phase, Op op) =>
        from valid in profile.Validate(phase: phase, op: op)
        from _ in guard(!profile.Scale.IsSome || phase.Allows(capability: scale), op.InvalidInput())
        select unit;

    internal static Fin<FileFormat> KnownFormat(string key, Op op) =>
        ByKey.TryGetValue(key: NormalizeKey(value: key), value: out FileFormat? builtin)
            ? Fin.Succ(value: builtin!)
            : CustomCell.Value.Find(key: NormalizeKey(value: key)).ToFin(Fail: op.InvalidInput());

    internal static Option<FileFormat> Resolve(FileEndpoint endpoint, FileProfile profile) =>
        endpoint.Format.Case switch {
            FileFormat value => Some(value),
            _ => profile.Format,
        };

    internal static Fin<FileFormat> Require(FileEndpoint endpoint, FileProfile profile, FilePhase phase, Op op) =>
        from format in Resolve(endpoint: endpoint, profile: profile).ToFin(Fail: op.InvalidInput())
        from valid in format.Validate(profile: profile, phase: phase, op: op)
        from supported in guard(format.Supports(phase: phase), op.InvalidInput())
        select format;

    internal static Fin<Unit> Import(RhinoDoc document, FileEndpoint source, FileProfile profile, Op op) =>
        from format in Require(endpoint: source, profile: profile, phase: FilePhase.Import, op: op)
        from imported in op.Catch(() => {
            using FileReadOptions options = ReadOptions(endpoint: source, profile: profile);
            return format.Read(options: options, document: document, source: source, profile: profile);
        })
        select imported;

    internal static File3dmWriteOptions ArchiveWriteOptions(ArchiveProfile profile) {
        FileWritePolicy write = profile.Write.Normalized;
        File3dmWriteOptions options = new() { Version = write.Version, SaveUserData = write.WriteUserData };
        options.EnableRenderMeshes(objectType: ObjectType.AnyObject, enable: write.IncludeRenderMeshes);
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

    private static Option<FileFormat> CustomFormat(string key, string extension) =>
        CustomCell.Value.Find(key: key) | CustomByExtension(extension: extension);

    private static Option<FileFormat> CustomByExtension(string extension) =>
        toSeq(CustomCell.Value.Values).Find(format => format.Extensions.Exists(value => string.Equals(a: value, b: extension, comparisonType: StringComparison.OrdinalIgnoreCase)));

    private static string NormalizeKey(string value) =>
        value.TrimStart('.').ToUpperInvariant();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "File extensions are public output spelling; format lookup remains case-insensitive, while generated paths use lower-invariant extensions.")]
    private static string NormalizeExtension(string value) =>
        (value.StartsWith('.') ? value : $".{value}").ToLowerInvariant();

    internal bool Is(string key) =>
        string.Equals(a: Key, b: NormalizeKey(value: key), comparisonType: StringComparison.OrdinalIgnoreCase);

    internal static Fin<Unit> NativeBool(Func<bool> run, Op op) =>
        op.Catch(() => op.Confirm(success: run()));

    internal static Fin<Unit> NativeWrite(WriteFileResult result, Op op) =>
        result switch {
            WriteFileResult.Success => Fin.Succ(value: unit),
            WriteFileResult.Cancel => Fin.Fail<Unit>(error: new Fault.Cancelled()),
            _ => Fin.Fail<Unit>(error: op.InvalidResult()),
        };

    private static FileFormat Native(
        string key,
        Seq<string> extensions,
        bool import = true,
        bool export = true,
        bool archive = false,
        FileCapability scale = FileCapability.None,
        Func<FileProfile, ArchivableDictionary>? read = null,
        Func<FileProfile, ArchivableDictionary>? write = null,
        Func<FileProfile, FileReadOptions, RhinoDoc, FileEndpoint, Fin<Unit>>? directRead = null,
        Func<FileProfile, FileWriteOptions, RhinoDoc, FileEndpoint, Fin<Unit>>? directWrite = null,
        bool directWriteOptions = false) =>
        new(key: key, extensions: extensions,
            capability: archive
                ? FileCapability.File3dm
                : (import ? FileCapability.Import : FileCapability.None) | (export ? FileCapability.Export : FileCapability.None),
            scale: scale, read: read, write: write, directRead: directRead, directWrite: directWrite, directWriteOptions: directWriteOptions);

    private static Func<FileProfile, FileReadOptions, TOptions> NewRead<TOptions>() where TOptions : new() =>
        static (_, _) => new TOptions();

    private static Func<FileProfile, FileWriteOptions, TOptions> NewWrite<TOptions>() where TOptions : new() =>
        static (_, _) => new TOptions();

    private static Func<FileProfile, FileReadOptions, RhinoDoc, FileEndpoint, Fin<Unit>> DirectRead<TOptions>(Func<FileProfile, FileReadOptions, TOptions> options, Func<string, RhinoDoc, TOptions, bool> read, string name) =>
        (profile, readOptions, document, source) => NativeBool(run: () => read(arg1: source.Path, arg2: document, arg3: options(arg1: profile, arg2: readOptions)), op: Op.Of(name: name));

    private static Func<FileProfile, FileWriteOptions, RhinoDoc, FileEndpoint, Fin<Unit>> DirectWrite<TOptions>(Func<FileProfile, FileWriteOptions, TOptions> options, Func<string, RhinoDoc, TOptions, bool> write, string name) =>
        (profile, writeOptions, document, target) => NativeBool(run: () => write(arg1: target.Path, arg2: document, arg3: options(arg1: profile, arg2: writeOptions)), op: Op.Of(name: name));

    private static Func<FileProfile, FileWriteOptions, RhinoDoc, FileEndpoint, Fin<Unit>> DirectWriteResult<TOptions>(Func<FileProfile, FileWriteOptions, TOptions> options, Func<string, RhinoDoc, TOptions, WriteFileResult> write, string name) =>
        (profile, writeOptions, document, target) => NativeWrite(result: write(arg1: target.Path, arg2: document, arg3: options(arg1: profile, arg2: writeOptions)), op: Op.Of(name: name));

    private static File3dsWriteOptions ThreeDsWriteOptions(FileProfile profile) =>
        new() { SaveViews = profile.Fidelity.IsModel, SaveLights = profile.Fidelity.IsModel };

    private static FileAiReadOptions AiReadOptions(FileProfile profile) {
        FileAiReadOptions options = new() { PreserveModelScale = profile.Fidelity.IsModel };
        return profile.Scale.Map(scale => scale.Apply(options: options)).IfNone(options);
    }

    private static FileAiWriteOptions AiWriteOptions(FileProfile profile) {
        FileAiWriteOptions options = new() {
            PreserveModelScale = profile.Fidelity.IsModel,
            OrderLayers = IsLayerGrouped(profile),
        };
        return profile.Scale.Map(scale => scale.Apply(options: options)).IfNone(options);
    }

    private static FileDwgWriteOptions DwgWriteOptions(FileProfile profile) =>
        new() {
            FullLayerPath = profile.Group == FileAxis.Layer,
            ExportSurfacesAs = profile.Fidelity == FileFidelity.GeometryOnly ? FileDwgWriteOptions.ExportSurfaceMode.Meshes : FileDwgWriteOptions.ExportSurfaceMode.Curves,
            UseLWPolylines = !profile.Fidelity.IsModel,
            ColorMethod = ShouldExportMaterials(profile.Resources) ? FileDwgWriteOptions.ColorMethodType.RGB : FileDwgWriteOptions.ColorMethodType.ACI,
            UseColor = profile.Order == FileAxis.Material ? FileDwgWriteOptions.UseColorType.USEPRINT : FileDwgWriteOptions.UseColorType.USEDISPLAY,
        };

    private static FileEpsReadOptions EpsReadOptions(FileProfile profile) {
        FileEpsReadOptions options = new() { PreserveModelScale = profile.Fidelity.IsModel };
        return profile.Scale.Map(scale => scale.Apply(options: options)).IfNone(options);
    }

    private static FileStlWriteOptions StlWriteOptions() =>
        new() { BinaryFile = true };

    private static FileFbxWriteOptions FbxWriteOptions(FileProfile profile) =>
        new() {
            SaveObjectsAs = profile.Fidelity.IsModel ? FileFbxWriteOptions.ObjectType.Nurbs : FileFbxWriteOptions.ObjectType.Mesh,
            SaveViews = profile.Fidelity.IsModel,
            SaveLights = profile.Fidelity.IsModel,
            SaveVertexNormals = profile.Fidelity.IncludeMeasurements,
        };

    private static FileSkpWriteOptions SkpWriteOptions(FileProfile profile) =>
        new() { GroupObjects = profile.Group is FileAxis.Layer or FileAxis.Block };

    private static FileVrmlWriteOptions VrmlWriteOptions(FileProfile profile) =>
        new() { ExportTextureCoordinates = ShouldExportMaterials(profile.Resources), ExportVertexNormals = profile.Fidelity.IsModel };

    private static FileX3dvWriteOptions X3dvWriteOptions(FileProfile profile) =>
        new() { ExportTextureCoordinates = ShouldExportMaterials(profile.Resources), ExportVertexNormals = profile.Fidelity.IsModel };

    private static FileObjWriteOptions ObjWriteOptions(FileProfile profile, FileWriteOptions options) =>
        new(options) {
            ObjectType = profile.Fidelity.IsModel && !options.WriteGeometryOnly ? FileObjWriteOptions.GeometryType.Nurbs : FileObjWriteOptions.GeometryType.Mesh,
            ExportObjectNames = profile.Group == FileAxis.ObjectName ? FileObjWriteOptions.ObjObjectNames.ObjectAsObject : FileObjWriteOptions.ObjObjectNames.NoObjects,
            ExportGroupNameLayerNames = profile.Group switch {
                FileAxis.Layer => FileObjWriteOptions.ObjGroupNames.LayerAsGroup,
                FileAxis.Block => FileObjWriteOptions.ObjGroupNames.GroupAsGroup,
                _ => FileObjWriteOptions.ObjGroupNames.NoGroups,
            },
            ExportMaterialDefinitions = ShouldExportMaterials(profile.Resources) && options.WriteUserData,
            UseDisplayColorForMaterial = ShouldExportMaterials(profile.Resources),
            ExportTcs = ShouldExportMaterials(profile.Resources),
            ExportNormals = profile.Fidelity.IncludeMeasurements,
            UseRenderMeshes = profile.Fidelity == FileFidelity.Small || options.IncludeRenderMeshes,
            SortObjGroups = profile.Order is FileAxis.Layer or FileAxis.Block,
            MergeNestedGroupingNames = profile.Group == FileAxis.Layer,
        };

    private static FilePlyWriteOptions PlyWriteOptions(FileProfile profile, FileWriteOptions options) =>
        new(options) {
            ExportASCII = profile.Fidelity != FileFidelity.Small,
            ExportDoubles = profile.Fidelity.IsModel,
            ExportNormals = profile.Fidelity.IncludeMeasurements,
            ExportColors = ShouldExportMaterials(profile.Resources),
            ExportMaterial = profile.Resources == FileResourcePolicy.Embed,
        };

    private static FileTxtWriteOptions TxtWriteOptions(FileProfile profile) =>
        new() { SurroundWithDoubleQuotes = profile.Group != FileAxis.Document };

    private static FileCsvWriteOptions CsvOptions(FileProfile profile) {
        bool layer = IsLayerGrouped(profile);
        bool group = profile.Group == FileAxis.Block || profile.Order == FileAxis.Block;
        bool material = profile.Group == FileAxis.Material || profile.Order == FileAxis.Material;
        bool user = profile.Group == FileAxis.UserString;
        bool measured = profile.Fidelity.IncludeMeasurements;
        return new() {
            Header = true,
            LayerName = layer,
            LayerIndex = layer,
            LayerColor = layer,
            LayerHierarchy = layer,
            GroupName = group,
            GroupIndexes = group,
            ObjectName = profile.Group == FileAxis.ObjectName || profile.Order == FileAxis.ObjectName,
            ObjectID = true,
            ObjectColor = profile.Order == FileAxis.ObjectType,
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

    private static FileGltfWriteOptions GltfWriteOptions(FileProfile profile) =>
        new() {
            ExportMaterials = ShouldExportMaterials(profile.Resources),
            ExportLayers = profile.Group == FileAxis.Layer,
            ExportTextureCoordinates = ShouldExportMaterials(profile.Resources),
            ExportVertexNormals = profile.Fidelity.IncludeMeasurements,
            UseDracoCompression = profile.Fidelity == FileFidelity.Small,
            DracoCompressionLevel = profile.Fidelity.Draco.Compression,
            DracoQuantizationBitsPosition = profile.Fidelity.Draco.BitsPos,
            DracoQuantizationBitsNormal = profile.Fidelity.Draco.BitsNormal,
            DracoQuantizationBitsTextureCoordinate = profile.Fidelity.Draco.BitsTexCoord,
        };

    private static FileUsdWriteOptions UsdWriteOptions(FileProfile profile) =>
        new() {
            ForceMeshes = !profile.Fidelity.IsModel,
            IncludeUserStrings = profile.Group == FileAxis.UserString,
            BlockHandling = profile.Resources switch {
                FileResourcePolicy.Embed => USDExportBlockHandling.Embedded,
                FileResourcePolicy.Copy => USDExportBlockHandling.SeparateFiles,
                _ => USDExportBlockHandling.Ignore,
            },
            DefaultLayer = profile.Group == FileAxis.Layer ? "Layers" : "World",
            ModelName = profile.Group == FileAxis.Document ? string.Empty : "Model",
        };

    private static FilePdfReadOptions PdfReadOptions(FileProfile profile) {
        FilePdfReadOptions options = new() {
            PreserveModelScale = profile.Fidelity.IsModel,
            ImportFillsAsHatches = profile.Fidelity.IncludeMeasurements,
            LoadText = profile.Fidelity.IsModel || profile.Group == FileAxis.UserString,
        };
        return profile.Scale.Map(scale => scale.Apply(options: options)).IfNone(options);
    }

    private static bool ShouldExportMaterials(FileResourcePolicy resources) => resources != FileResourcePolicy.Reference;

    private static bool IsLayerGrouped(FileProfile profile) => profile.Group == FileAxis.Layer || profile.Order == FileAxis.Layer;
}
