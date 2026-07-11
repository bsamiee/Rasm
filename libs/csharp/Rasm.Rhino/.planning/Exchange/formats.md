# [RASM_RHINO_FORMATS]

The codec capability matrix (`Rasm.Rhino.Exchange`). `FileCodec` is ONE generated `[SmartEnum<string>]` vocabulary whose declaration list is the whole interchange roster — every row carries its extensions, its ability set, its direct host engine adapters, and its typed option projection as constructor data — and every secondary surface derives from that one declaration: extension detection, key lookup, dialog filter strings, plug-in file-type registration, and read/write dispatch are projections over `Items`, never parallel structures. The killed census forms are the handwritten `BuiltIn` roster beside three hand-maintained lookup dictionaries, the mutable custom-registration `Atom` that let a format enter outside the vocabulary, the sibling option-builder method family that restated one policy per format, the four parallel `FileVectorScale.Apply` overloads, and the filesystem backup policy embedded in a codec adapter — backup and collision are `operations.md` output policy, and a new format is one row. The `Rhino.PlugIns` file-dialog territory binds through the same rows: `CodecPort` populates the host `FileTypeList` from import- and export-capable rows and dispatches the host's index-keyed `ReadFile`/`WriteFile` back onto the matrix.

## [01]-[INDEX]

- [02]-[ABILITY_AXES]: `CodecAbility` the capability vocabulary, `CodecPhase` the dispatch phases, `CodecFidelity`/`CodecAxis`/`CodecResource` the tune policy rows, and `CodecTune` the one option-policy record.
- [03]-[VECTOR_SCALE]: `VectorUnit` the unit correspondence rows, `VectorLens<TOptions>` the per-option-type setter row, and `VectorScale` the one generic application.
- [04]-[CODEC_MATRIX]: `FileCodec` — the generated row set with engine adapters and option projections, and the derived lookup, filter, and extension surfaces.
- [05]-[DIALOG_PORT]: `CodecPort` — `FileTypeList` registration and index-keyed host dialog dispatch over the matrix.

## [02]-[ABILITY_AXES]

- Owner: `CodecAbility` `[SmartEnum<int>]` — the combinable capability vocabulary a row declares as a set: `Archive` (3dm-native), `Import`, `Export`, `Vector` (page-space vector interchange), `Raster` (pixel egress rows the publish pipeline encodes). `CodecPhase` `[SmartEnum<int>]` — the dispatch phases: `Import` and `Export` each carry a `Demands` ability column the filter derivation and the entry gates read, so phase validation is one membership probe against the row's set, never a per-phase branch. `CodecFidelity` `[SmartEnum<int>]` — `Model`/`Small`/`GeometryOnly` rows with `IsModel`, `Measured`, and `Draco` compression columns. `CodecAxis` `[SmartEnum<int>]` — the grouping/ordering vocabulary: `Stable`, `Document`, `File`, `Layer`, `ObjectName`, `ObjectType`, `Material`, `Block`, `UserString`. `CodecResource` `[SmartEnum<int>]` — `Reference`/`Embed`/`Copy`. `CodecTune` — the one option-policy record every option projection reads; its presets are the policy values, and a per-format knob is unrepresentable because rows read the tune, never a signature.
- Law: capability is set membership, not flags — a row's `Abilities` is declared once and probed through `Has`; a phase admits a row only when `Demands` is a member, so an engine-less raster row structurally never reaches an engine delegate.
- Law: `CodecTune` arrives pre-constructed and carries its whole policy; no codec entrypoint grows a boolean beside it, and a consumer needing one divergent axis takes `with` on a preset.
- Growth: a new fidelity, axis, or resource stance is one row; every option projection that reads the new column breaks loudly at the row constructor, never silently at a call site.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino.FileIO;

namespace Rasm.Rhino.Exchange;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CodecAbility {
    public static readonly CodecAbility Archive = new(key: 0);
    public static readonly CodecAbility Import = new(key: 1);
    public static readonly CodecAbility Export = new(key: 2);
    public static readonly CodecAbility Vector = new(key: 3);
    public static readonly CodecAbility Raster = new(key: 4);
}

[SmartEnum<int>]
public sealed partial class CodecPhase {
    public static readonly CodecPhase Import = new(key: 0, demands: CodecAbility.Import);
    public static readonly CodecPhase Export = new(key: 1, demands: CodecAbility.Export);

    public CodecAbility Demands { get; }
}

[SmartEnum<int>]
public sealed partial class CodecFidelity {
    public static readonly CodecFidelity Model = new(key: 0, isModel: true, measured: true, draco: (0, 14, 10, 12));
    public static readonly CodecFidelity Small = new(key: 1, isModel: false, measured: false, draco: (7, 11, 8, 10));
    public static readonly CodecFidelity GeometryOnly = new(key: 2, isModel: false, measured: true, draco: (0, 14, 10, 12));

    public bool IsModel { get; }
    public bool Measured { get; }
    public (int Compression, int BitsPos, int BitsNormal, int BitsTexCoord) Draco { get; }
}

[SmartEnum<int>]
public sealed partial class CodecAxis {
    public static readonly CodecAxis Stable = new(key: 0);
    public static readonly CodecAxis Document = new(key: 1);
    public static readonly CodecAxis File = new(key: 2);
    public static readonly CodecAxis Layer = new(key: 3);
    public static readonly CodecAxis ObjectName = new(key: 4);
    public static readonly CodecAxis ObjectType = new(key: 5);
    public static readonly CodecAxis Material = new(key: 6);
    public static readonly CodecAxis Block = new(key: 7);
    public static readonly CodecAxis UserString = new(key: 8);
}

[SmartEnum<int>]
public sealed partial class CodecResource {
    public static readonly CodecResource Reference = new(key: 0);
    public static readonly CodecResource Embed = new(key: 1);
    public static readonly CodecResource Copy = new(key: 2);
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record CodecTune(
    CodecFidelity Fidelity,
    CodecResource Resources,
    CodecAxis Group,
    CodecAxis Order,
    bool Materials,
    Option<VectorScale> Scale) {
    public static CodecTune Model { get; } = new(
        Fidelity: CodecFidelity.Model, Resources: CodecResource.Reference,
        Group: CodecAxis.Document, Order: CodecAxis.Stable, Materials: true, Scale: None);

    public static CodecTune Small { get; } = Model with { Fidelity = CodecFidelity.Small, Materials = false };

    public static CodecTune GeometryOnly { get; } = Model with { Fidelity = CodecFidelity.GeometryOnly, Materials = false };

    internal bool LayerGrouped => Group == CodecAxis.Layer || Order == CodecAxis.Layer;
}
```

## [03]-[VECTOR_SCALE]

- Owner: `VectorUnit` `[SmartEnum<int>]` — the page-unit correspondence rows whose columns carry each host option type's own unit enum, so unit translation is a column read. `VectorLens<TOptions>` — the per-option-type setter row: four delegates naming where preserve, Rhino scale, source scale, and unit land on that host type. `VectorScale` — the one scale value with ONE generic `Apply<TOptions>(options, lens)`; the census's four hand-written `Apply` overloads collapse into lens rows declared beside the codec rows that consume them.
- Law: an explicit scale member (`Unit`, `Source`, `Rhino`) forces `PreserveModelScale` false unless `Preserve` is explicitly set; declaring both `Preserve = true` and an explicit member is a construction refusal, so a contradictory scale never reaches a host option.
- Law: scale participation is a row fact — only rows whose option projection threads `Scale` consume it; a scale supplied to a non-vector row is inert by construction because no lens exists for its option type.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class VectorUnit {
    public static readonly VectorUnit Inches = new(key: 0,
        pdf: FilePdfReadOptions.PDF_UNITS.inches, aiRead: FileAiReadOptions.Units.Inches,
        aiWrite: FileAiWriteOptions.Units.Inches, eps: FileEpsReadOptions.Units.Inches);
    public static readonly VectorUnit Centimeters = new(key: 1,
        pdf: FilePdfReadOptions.PDF_UNITS.centimeters, aiRead: FileAiReadOptions.Units.Centimeters,
        aiWrite: FileAiWriteOptions.Units.Centimeters, eps: FileEpsReadOptions.Units.Centimeters);
    public static readonly VectorUnit Millimeters = new(key: 2,
        pdf: FilePdfReadOptions.PDF_UNITS.millimeters, aiRead: FileAiReadOptions.Units.Millimeters,
        aiWrite: FileAiWriteOptions.Units.Millimeters, eps: FileEpsReadOptions.Units.Millimeters);
    public static readonly VectorUnit Points = new(key: 3,
        pdf: FilePdfReadOptions.PDF_UNITS.points, aiRead: FileAiReadOptions.Units.Points,
        aiWrite: FileAiWriteOptions.Units.Points, eps: FileEpsReadOptions.Units.Points);

    internal FilePdfReadOptions.PDF_UNITS Pdf { get; }
    internal FileAiReadOptions.Units AiRead { get; }
    internal FileAiWriteOptions.Units AiWrite { get; }
    internal FileEpsReadOptions.Units Eps { get; }
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record VectorLens<TOptions>(
    Action<TOptions, bool> Preserve,
    Action<TOptions, double> Rhino,
    Action<TOptions, double> Source,
    Action<TOptions, VectorUnit> Unit) where TOptions : class;

public readonly record struct VectorScale(
    Option<VectorUnit> Unit = default,
    Option<double> Source = default,
    Option<double> Rhino = default,
    Option<bool> Preserve = default) {
    private bool HasExplicit => Unit.IsSome || Source.IsSome || Rhino.IsSome;

    private Option<bool> PreserveMode => Preserve | (HasExplicit ? Some(value: false) : Option<bool>.None);

    public static Fin<VectorScale> Of(
        Option<VectorUnit> unit = default,
        Option<double> source = default,
        Option<double> rhino = default,
        Option<bool> preserve = default,
        Op? key = null) {
        Op op = key.OrDefault();
        VectorScale candidate = new(Unit: unit, Source: source, Rhino: rhino, Preserve: preserve);
        return from _mode in guard(!(preserve.Case is true && candidate.HasExplicit), op.InvalidInput()).ToFin()
               from _source in source.Map(value => op.Positive(value: value).Map(static _ => unit)).IfNone(Fin.Succ(value: unit))
               from _rhino in rhino.Map(value => op.Positive(value: value).Map(static _ => unit)).IfNone(Fin.Succ(value: unit))
               select candidate;
    }

    internal TOptions Apply<TOptions>(TOptions options, VectorLens<TOptions> lens) where TOptions : class {
        VectorScale self = this;
        _ = Op.Side(() => {
            _ = self.PreserveMode.Iter(value => lens.Preserve(arg1: options, arg2: value));
            _ = self.Rhino.Iter(value => lens.Rhino(arg1: options, arg2: value));
            _ = self.Source.Iter(value => lens.Source(arg1: options, arg2: value));
            _ = self.Unit.Iter(value => lens.Unit(arg1: options, arg2: value));
        });
        return options;
    }
}

// --- [CONSTANTS] ----------------------------------------------------------------------------
internal static class VectorLenses {
    internal static readonly VectorLens<FilePdfReadOptions> Pdf = new(
        Preserve: static (o, v) => o.PreserveModelScale = v, Rhino: static (o, v) => o.RhinoScale = v,
        Source: static (o, v) => o.PDFScale = v, Unit: static (o, v) => o.PdfUnits = v.Pdf);
    internal static readonly VectorLens<FileAiReadOptions> AiRead = new(
        Preserve: static (o, v) => o.PreserveModelScale = v, Rhino: static (o, v) => o.RhinoScale = v,
        Source: static (o, v) => o.AiScale = v, Unit: static (o, v) => o.AiUnits = v.AiRead);
    internal static readonly VectorLens<FileAiWriteOptions> AiWrite = new(
        Preserve: static (o, v) => o.PreserveModelScale = v, Rhino: static (o, v) => o.RhinoScale = v,
        Source: static (o, v) => o.AIScale = v, Unit: static (o, v) => o.AiUnits = v.AiWrite);
    internal static readonly VectorLens<FileEpsReadOptions> Eps = new(
        Preserve: static (o, v) => o.PreserveModelScale = v, Rhino: static (o, v) => o.RhinoScale = v,
        Source: static (o, v) => o.EpsScale = v, Unit: static (o, v) => o.EpsUnits = v.Eps);
}
```

## [04]-[CODEC_MATRIX]

- Owner: `FileCodec` `[SmartEnum<string>]` — the interchange matrix. Each row declares its extension set, its ability set, and two `[UseDelegateFromConstructor]` engine columns whose bodies mint the row's typed host options from the `CodecTune` and invoke the direct engine; rows without an engine leg answer with the shared typed refusal, and the ability gate in the one entry keeps that refusal unreachable. Option construction is row DATA — the recurring fidelity, axis, material, and scale dimensions read off the tune inside each row's lambda, so one policy record drives every host option type and the census's sibling builder-method family is gone.
- Entry: `Codecs.Read(RhinoDoc, DocumentPath, FileCodec, CodecTune, FileReadOptions, Op?)` and `Codecs.Write(RhinoDoc, DocumentPath, FileCodec, CodecTune, FileWriteOptions, Op?)` — one internal entry per direction, one dispatch through the row's engine column; both take a raw host handle, so they stay internal to the package where the only sanctioned holders live (the `Exchanges` demand window and the host dialog callbacks), and consumers enter through `Exchanges.Run`. A row whose host lane is document-attached (`RhinoDoc.Import` with an `ArchivableDictionary`, `RhinoDoc.Export` with the option dictionary) carries that lane INSIDE its engine column, so the caller never selects a transport and no second dispatch structure exists.
- Law: `Detect`, `Of`, `Filter`, and `EnsureExtension` derive from `Items` through lazy frozen indexes — the declaration list is the single source, a new row lands in every derived surface with zero additional edits, and a reserved key (`json`) is refused at the row-lookup boundary so wire payload spellings never collide with interchange formats.
- Law: the vocabulary is closed — the census's runtime custom-registration cell is dead; a format the matrix lacks is one new row, and a foreign plug-in's format reaches the document only through the host's own dialog dispatch, never through this matrix.
- Law: engine outcomes normalize at the row — a `bool` engine and a `WriteFileResult` engine both fold to `Fin<Unit>` through the two adapters, so consumers see one rail regardless of which host generation the engine belongs to.
- Boundary: `FilePdf` page authoring and raster encoding are `publish.md` egress; the `pdf`/`svg` rows here own only page-space vector import, and the raster rows exist as capability data the publish target vocabulary keys on.

```csharp
// --- [MODELS] -------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class FileCodec {
    public static readonly FileCodec ThreeDm = new("3dm", Seq(".3dm"),
        Seq(CodecAbility.Archive, CodecAbility.Import),
        static (tune, carrier, doc, path, op) =>
            op.Confirm(success: doc.Import(filePath: path, options: new Rhino.Collections.ArchivableDictionary())),
        Unwritten);
    public static readonly FileCodec ThreeDs = new("3ds", Seq(".3ds"),
        Seq(CodecAbility.Import, CodecAbility.Export),
        static (tune, carrier, doc, path, op) => Confirm(File3ds.Read(path, doc, new File3dsReadOptions()), op),
        static (tune, carrier, doc, path, op) => Confirm(File3ds.Write(path, doc,
            new File3dsWriteOptions { SaveViews = tune.Fidelity.IsModel, SaveLights = tune.Fidelity.IsModel }), op));
    public static readonly FileCodec ThreeMf = new("3mf", Seq(".3mf"), Seq(CodecAbility.Export), Unread,
        static (tune, carrier, doc, path, op) => Confirm(File3mf.Write(path, doc, new File3mfWriteOptions()), op));
    public static readonly FileCodec Ai = new("ai", Seq(".ai"),
        Seq(CodecAbility.Import, CodecAbility.Export, CodecAbility.Vector),
        static (tune, carrier, doc, path, op) => Confirm(FileAi.Read(path, doc, AiRead(tune)), op),
        static (tune, carrier, doc, path, op) => Confirm(FileAi.Write(path, doc, AiWrite(tune)), op));
    public static readonly FileCodec Amf = new("amf", Seq(".amf"), Seq(CodecAbility.Export), Unread,
        static (tune, carrier, doc, path, op) => Confirm(FileAmf.Write(path, doc, new FileAmfWriteOptions()), op));
    public static readonly FileCodec Obj = new("obj", Seq(".obj"),
        Seq(CodecAbility.Import, CodecAbility.Export),
        static (tune, carrier, doc, path, op) => Confirm(FileObj.Read(path, doc, new FileObjReadOptions(carrier)), op),
        static (tune, carrier, doc, path, op) => Confirm(FileObj.Write(path, doc, ObjWrite(tune, carrier)), op));
    public static readonly FileCodec Ply = new("ply", Seq(".ply"),
        Seq(CodecAbility.Import, CodecAbility.Export),
        static (tune, carrier, doc, path, op) => Confirm(FilePly.Read(path, doc, new FilePlyReadOptions()), op),
        static (tune, carrier, doc, path, op) => Confirm(FilePly.Write(path, doc, new FilePlyWriteOptions(carrier) {
            ExportASCII = tune.Fidelity != CodecFidelity.Small, ExportDoubles = tune.Fidelity.IsModel,
            ExportNormals = tune.Fidelity.Measured, ExportColors = tune.Materials,
            ExportMaterial = tune.Resources == CodecResource.Embed }), op));
    public static readonly FileCodec Cd = new("cd", Seq(".cd"), Seq(CodecAbility.Export), Unread,
        static (tune, carrier, doc, path, op) => Confirm(FileCd.Write(path, doc, new FileCdWriteOptions()), op));
    public static readonly FileCodec Dgn = new("dgn", Seq(".dgn"), Seq(CodecAbility.Import),
        static (tune, carrier, doc, path, op) => Confirm(FileDgn.Read(path, doc, new FileDgnReadOptions()), op), Unwritten);
    public static readonly FileCodec Dst = new("dst", Seq(".dst"), Seq(CodecAbility.Import),
        static (tune, carrier, doc, path, op) => Confirm(FileDst.Read(path, doc, new FileDstReadOptions()), op), Unwritten);
    public static readonly FileCodec Dwg = new("dwg", Seq(".dwg", ".dxf"),
        Seq(CodecAbility.Import, CodecAbility.Export),
        static (tune, carrier, doc, path, op) => Confirm(FileDwg.Read(path, doc, new FileDwgReadOptions()), op),
        static (tune, carrier, doc, path, op) => Confirm(FileDwg.Write(path, doc, new FileDwgWriteOptions {
            FullLayerPath = tune.Group == CodecAxis.Layer,
            ExportSurfacesAs = tune.Fidelity == CodecFidelity.GeometryOnly
                ? FileDwgWriteOptions.ExportSurfaceMode.Meshes : FileDwgWriteOptions.ExportSurfaceMode.Curves,
            UseLWPolylines = !tune.Fidelity.IsModel,
            ColorMethod = tune.Materials ? FileDwgWriteOptions.ColorMethodType.RGB : FileDwgWriteOptions.ColorMethodType.ACI,
            UseColor = tune.Order == CodecAxis.Material
                ? FileDwgWriteOptions.UseColorType.USEPRINT : FileDwgWriteOptions.UseColorType.USEDISPLAY }), op));
    public static readonly FileCodec Eps = new("eps", Seq(".eps"),
        Seq(CodecAbility.Import, CodecAbility.Vector),
        static (tune, carrier, doc, path, op) => Confirm(FileEps.Read(path, doc, EpsRead(tune)), op), Unwritten);
    public static readonly FileCodec Stl = new("stl", Seq(".stl"),
        Seq(CodecAbility.Import, CodecAbility.Export),
        static (tune, carrier, doc, path, op) => Confirm(FileStl.Read(path, doc, new FileStlReadOptions()), op),
        static (tune, carrier, doc, path, op) => Confirm(FileStl.Write(path, doc, new FileStlWriteOptions { BinaryFile = true }), op));
    public static readonly FileCodec Stp = new("stp", Seq(".stp", ".step"),
        Seq(CodecAbility.Import, CodecAbility.Export),
        static (tune, carrier, doc, path, op) => Confirm(FileStp.Read(path, doc, new FileStpReadOptions()), op),
        static (tune, carrier, doc, path, op) => Confirm(FileStp.Write(path, doc, new FileStpWriteOptions()), op));
    public static readonly FileCodec Fbx = new("fbx", Seq(".fbx"),
        Seq(CodecAbility.Import, CodecAbility.Export),
        static (tune, carrier, doc, path, op) => Confirm(FileFbx.Read(path, doc, new FileFbxReadOptions()), op),
        static (tune, carrier, doc, path, op) => Confirm(FileFbx.Write(path, doc, new FileFbxWriteOptions {
            SaveObjectsAs = tune.Fidelity.IsModel ? FileFbxWriteOptions.ObjectType.Nurbs : FileFbxWriteOptions.ObjectType.Mesh,
            SaveViews = tune.Fidelity.IsModel, SaveLights = tune.Fidelity.IsModel,
            SaveVertexNormals = tune.Fidelity.Measured }), op));
    public static readonly FileCodec Ghs = new("ghs", Seq(".ghs"), Seq(CodecAbility.Import),
        static (tune, carrier, doc, path, op) => Confirm(FileGHS.Read(path, doc, new FileGHSReadOptions()), op), Unwritten);
    public static readonly FileCodec Gts = new("gts", Seq(".gts"), Seq(CodecAbility.Export), Unread,
        static (tune, carrier, doc, path, op) => Confirm(FileGts.Write(path, doc, new FileGtsWriteOptions()), op));
    public static readonly FileCodec Igs = new("igs", Seq(".igs", ".iges"), Seq(CodecAbility.Export), Unread,
        static (tune, carrier, doc, path, op) => Confirm(FileIgs.Write(path, doc, new FileIgsWriteOptions()), op));
    public static readonly FileCodec Lwo = new("lwo", Seq(".lwo"),
        Seq(CodecAbility.Import, CodecAbility.Export),
        static (tune, carrier, doc, path, op) => Confirm(FileLwo.Read(path, doc, new FileLwoReadOptions()), op),
        static (tune, carrier, doc, path, op) => Confirm(FileLwo.Write(path, doc, new FileLwoWriteOptions()), op));
    public static readonly FileCodec Nwd = new("nwd", Seq(".nwd"), Seq(CodecAbility.Export), Unread,
        static (tune, carrier, doc, path, op) => Confirm(FileNwd.Write(path, doc, new FileNwdWriteOptions()), op));
    public static readonly FileCodec Pov = new("pov", Seq(".pov"), Seq(CodecAbility.Export), Unread,
        static (tune, carrier, doc, path, op) => Confirm(FilePov.Write(path, doc, new FilePovWriteOptions()), op));
    public static readonly FileCodec Sat = new("sat", Seq(".sat"), Seq(CodecAbility.Export), Unread,
        static (tune, carrier, doc, path, op) => Confirm(FileSat.Write(path, doc, new FileSatWriteOptions()), op));
    public static readonly FileCodec Skp = new("skp", Seq(".skp"),
        Seq(CodecAbility.Import, CodecAbility.Export),
        static (tune, carrier, doc, path, op) => Confirm(FileSkp.Read(path, doc, new FileSkpReadOptions()), op),
        static (tune, carrier, doc, path, op) => Confirm(FileSkp.Write(path, doc, new FileSkpWriteOptions {
            GroupObjects = tune.Group == CodecAxis.Layer || tune.Group == CodecAxis.Block }), op));
    public static readonly FileCodec Slc = new("slc", Seq(".slc"), Seq(CodecAbility.Export), Unread,
        static (tune, carrier, doc, path, op) => Confirm(FileSlc.Write(path, doc, new FileSlcWriteOptions()), op));
    public static readonly FileCodec Sw = new("sw", Seq(".sldprt", ".sldasm"), Seq(CodecAbility.Import),
        static (tune, carrier, doc, path, op) => Confirm(FileSW.Read(path, doc, new FileSwReadOptions()), op), Unwritten);
    public static readonly FileCodec Udo = new("udo", Seq(".udo"), Seq(CodecAbility.Export), Unread,
        static (tune, carrier, doc, path, op) => Confirm(FileUdo.Write(path, doc, new FileUdoWriteOptions()), op));
    public static readonly FileCodec Vda = new("vda", Seq(".vda"), Seq(CodecAbility.Export), Unread,
        static (tune, carrier, doc, path, op) => Confirm(FileVda.Write(path, doc, new FileVdaWriteOptions()), op));
    public static readonly FileCodec Vrml = new("vrml", Seq(".wrl", ".vrml"), Seq(CodecAbility.Export), Unread,
        static (tune, carrier, doc, path, op) => Confirm(FileVrml.Write(path, doc, new FileVrmlWriteOptions {
            ExportTextureCoordinates = tune.Materials, ExportVertexNormals = tune.Fidelity.IsModel }), op));
    public static readonly FileCodec X3dv = new("x3dv", Seq(".x3dv"), Seq(CodecAbility.Export), Unread,
        static (tune, carrier, doc, path, op) => Confirm(FileX3dv.Write(path, doc, new FileX3dvWriteOptions {
            ExportTextureCoordinates = tune.Materials, ExportVertexNormals = tune.Fidelity.IsModel }), op));
    public static readonly FileCodec Xaml = new("xaml", Seq(".xaml"), Seq(CodecAbility.Export), Unread,
        static (tune, carrier, doc, path, op) => op.Confirm(success: doc.Export(filePath: path,
            options: new FileXamlWriteOptions { UseExistingRenderMeshes = tune.Fidelity.IsModel }.ToDictionary())));
    public static readonly FileCodec XT = new("x_t", Seq(".x_t", ".x_b"), Seq(CodecAbility.Export), Unread,
        static (tune, carrier, doc, path, op) => Confirm(FileX_T.Write(path, doc, new FileX_TWriteOptions()), op));
    public static readonly FileCodec Raw = new("raw", Seq(".raw"),
        Seq(CodecAbility.Import, CodecAbility.Export),
        static (tune, carrier, doc, path, op) => Confirm(FileRaw.Read(path, doc, new FileRawReadOptions()), op),
        static (tune, carrier, doc, path, op) => Confirm(FileRaw.Write(path, doc, new FileRawWriteOptions()), op));
    public static readonly FileCodec Txt = new("txt", Seq(".txt"),
        Seq(CodecAbility.Import, CodecAbility.Export),
        static (tune, carrier, doc, path, op) => Confirm(FileTxt.Read(path, doc, new FileTxtReadOptions()), op),
        static (tune, carrier, doc, path, op) => Confirm(FileTxt.Write(path, doc, new FileTxtWriteOptions {
            SurroundWithDoubleQuotes = tune.Group != CodecAxis.Document }), op));
    public static readonly FileCodec Csv = new("csv", Seq(".csv"), Seq(CodecAbility.Export), Unread,
        static (tune, carrier, doc, path, op) => Confirm(FileCsv.Write(path, doc, CsvWrite(tune)), op));
    public static readonly FileCodec Gltf = new("gltf", Seq(".gltf", ".glb"), Seq(CodecAbility.Export), Unread,
        static (tune, carrier, doc, path, op) => Confirm(FileGltf.Write(path, doc, new FileGltfWriteOptions {
            ExportMaterials = tune.Materials, ExportLayers = tune.Group == CodecAxis.Layer,
            ExportTextureCoordinates = tune.Materials, ExportVertexNormals = tune.Fidelity.Measured,
            UseDracoCompression = tune.Fidelity == CodecFidelity.Small,
            DracoCompressionLevel = tune.Fidelity.Draco.Compression,
            DracoQuantizationBitsPosition = tune.Fidelity.Draco.BitsPos,
            DracoQuantizationBitsNormal = tune.Fidelity.Draco.BitsNormal,
            DracoQuantizationBitsTextureCoordinate = tune.Fidelity.Draco.BitsTexCoord }), op));
    public static readonly FileCodec Usd = new("usd", Seq(".usd", ".usda", ".usdz"), Seq(CodecAbility.Export), Unread,
        static (tune, carrier, doc, path, op) => Confirm(FileUsd.Write(path, doc, new FileUsdWriteOptions {
            ForceMeshes = !tune.Fidelity.IsModel, IncludeUserStrings = tune.Group == CodecAxis.UserString,
            BlockHandling = tune.Resources.Switch(
                reference: static () => USDExportBlockHandling.Ignore,
                embed: static () => USDExportBlockHandling.Embedded,
                copy: static () => USDExportBlockHandling.SeparateFiles),
            DefaultLayer = tune.Group == CodecAxis.Layer ? "Layers" : "World",
            ModelName = tune.Group == CodecAxis.Document ? string.Empty : "Model" }), op));
    public static readonly FileCodec Pdf = new("pdf", Seq(".pdf"),
        Seq(CodecAbility.Import, CodecAbility.Vector),
        static (tune, carrier, doc, path, op) => Confirm(FilePdf.Read(path, doc, PdfRead(tune)), op), Unwritten);
    public static readonly FileCodec Svg = new("svg", Seq(".svg"),
        Seq(CodecAbility.Import, CodecAbility.Vector),
        static (tune, carrier, doc, path, op) => Confirm(FileSvg.Read(path, doc, new FileSvgReadOptions()), op), Unwritten);
    public static readonly FileCodec Png = new("png", Seq(".png"), Seq(CodecAbility.Raster), Unread, Unwritten);
    public static readonly FileCodec Jpeg = new("jpeg", Seq(".jpg", ".jpeg"), Seq(CodecAbility.Raster), Unread, Unwritten);
    public static readonly FileCodec Tiff = new("tiff", Seq(".tif", ".tiff"), Seq(CodecAbility.Raster), Unread, Unwritten);
    public static readonly FileCodec Bmp = new("bmp", Seq(".bmp"), Seq(CodecAbility.Raster), Unread, Unwritten);

    public Seq<string> Extensions { get; }
    public Seq<CodecAbility> Abilities { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> ReadEngine(CodecTune tune, FileReadOptions carrier, RhinoDoc document, string path, Op key);

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> WriteEngine(CodecTune tune, FileWriteOptions carrier, RhinoDoc document, string path, Op key);

    public bool Has(CodecAbility ability) => Abilities.Exists(row => row == ability);

    public string EnsureExtension(string path) =>
        Extensions.Exists(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            ? path
            : path + Extensions.Head.IfNone(noneValue: string.Empty);

    private static Fin<Unit> Confirm(bool success, Op op) => op.Confirm(success: success);
    private static Fin<Unit> Confirm(WriteFileResult result, Op op) =>
        op.Confirm(success: result == WriteFileResult.Success);

    private static Fin<Unit> Unread(CodecTune tune, FileReadOptions carrier, RhinoDoc document, string path, Op key) =>
        Fin.Fail<Unit>(error: key.InvalidInput());
    private static Fin<Unit> Unwritten(CodecTune tune, FileWriteOptions carrier, RhinoDoc document, string path, Op key) =>
        Fin.Fail<Unit>(error: key.InvalidInput());

    private static FileAiReadOptions AiRead(CodecTune tune) =>
        tune.Scale.Map(scale => scale.Apply(new FileAiReadOptions { PreserveModelScale = tune.Fidelity.IsModel }, VectorLenses.AiRead))
            .IfNone(() => new FileAiReadOptions { PreserveModelScale = tune.Fidelity.IsModel });
    private static FileAiWriteOptions AiWrite(CodecTune tune) =>
        tune.Scale.Map(scale => scale.Apply(new FileAiWriteOptions { PreserveModelScale = tune.Fidelity.IsModel, OrderLayers = tune.LayerGrouped }, VectorLenses.AiWrite))
            .IfNone(() => new FileAiWriteOptions { PreserveModelScale = tune.Fidelity.IsModel, OrderLayers = tune.LayerGrouped });
    private static FileEpsReadOptions EpsRead(CodecTune tune) =>
        tune.Scale.Map(scale => scale.Apply(new FileEpsReadOptions { PreserveModelScale = tune.Fidelity.IsModel }, VectorLenses.Eps))
            .IfNone(() => new FileEpsReadOptions { PreserveModelScale = tune.Fidelity.IsModel });
    private static FilePdfReadOptions PdfRead(CodecTune tune) =>
        tune.Scale.Map(scale => scale.Apply(new FilePdfReadOptions {
                PreserveModelScale = tune.Fidelity.IsModel, ImportFillsAsHatches = tune.Fidelity.Measured,
                LoadText = tune.Fidelity.IsModel || tune.Group == CodecAxis.UserString }, VectorLenses.Pdf))
            .IfNone(() => new FilePdfReadOptions {
                PreserveModelScale = tune.Fidelity.IsModel, ImportFillsAsHatches = tune.Fidelity.Measured,
                LoadText = tune.Fidelity.IsModel || tune.Group == CodecAxis.UserString });

    private static FileObjWriteOptions ObjWrite(CodecTune tune, FileWriteOptions carrier) =>
        new(carrier) {
            ObjectType = tune.Fidelity.IsModel && !carrier.WriteGeometryOnly
                ? FileObjWriteOptions.GeometryType.Nurbs : FileObjWriteOptions.GeometryType.Mesh,
            ExportObjectNames = tune.Group == CodecAxis.ObjectName
                ? FileObjWriteOptions.ObjObjectNames.ObjectAsObject : FileObjWriteOptions.ObjObjectNames.NoObjects,
            ExportGroupNameLayerNames = tune.Group == CodecAxis.Layer
                ? FileObjWriteOptions.ObjGroupNames.LayerAsGroup
                : tune.Group == CodecAxis.Block
                    ? FileObjWriteOptions.ObjGroupNames.GroupAsGroup
                    : FileObjWriteOptions.ObjGroupNames.NoGroups,
            ExportMaterialDefinitions = tune.Materials && carrier.WriteUserData,
            UseDisplayColorForMaterial = tune.Materials,
            ExportTcs = tune.Materials,
            ExportNormals = tune.Fidelity.Measured,
            UseRenderMeshes = tune.Fidelity == CodecFidelity.Small || carrier.IncludeRenderMeshes,
            SortObjGroups = tune.Order == CodecAxis.Layer || tune.Order == CodecAxis.Block,
            MergeNestedGroupingNames = tune.Group == CodecAxis.Layer,
        };

    private static FileCsvWriteOptions CsvWrite(CodecTune tune) {
        bool layer = tune.LayerGrouped;
        bool grouped = tune.Group == CodecAxis.Block || tune.Order == CodecAxis.Block;
        bool material = tune.Group == CodecAxis.Material || tune.Order == CodecAxis.Material;
        bool user = tune.Group == CodecAxis.UserString;
        bool measured = tune.Fidelity.Measured;
        return new() {
            Header = true, LayerName = layer, LayerIndex = layer, LayerColor = layer, LayerHierarchy = layer,
            GroupName = grouped, GroupIndexes = grouped,
            ObjectName = tune.Group == CodecAxis.ObjectName || tune.Order == CodecAxis.ObjectName,
            ObjectID = true, ObjectColor = tune.Order == CodecAxis.ObjectType, ObjectMaterial = material,
            ObjectDescription = user, SurroundPointsWithDoubleQuotes = true,
            Length = measured, Perimeter = measured, Area = measured, Volume = measured,
            AreaCentroid = measured, VolumeCentroid = measured, AreaMoments = measured, VolumeMoments = measured,
            CumulativeMassProperties = measured, AttributesKeys = user, AttributesTexts = user,
            ObjectKeys = user, ObjectsTexts = user,
        };
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Codecs {
    private static readonly FrozenSet<string> Reserved =
        new[] { "json", ".json" }.ToFrozenSet(comparer: StringComparer.OrdinalIgnoreCase);

    private static readonly Lazy<FrozenDictionary<string, FileCodec>> ByExtension = new(static () =>
        FileCodec.Items
            .SelectMany(static row => row.Extensions.Map(ext => KeyValuePair.Create(ext, row)))
            .ToFrozenDictionary(comparer: StringComparer.OrdinalIgnoreCase));

    public static Option<FileCodec> Detect(string path) =>
        Optional(System.IO.Path.GetExtension(path))
            .Filter(static ext => !string.IsNullOrWhiteSpace(value: ext))
            .Bind(ext => ByExtension.Value.TryGetValue(ext, out FileCodec? row) ? Optional(row) : None);

    public static Fin<FileCodec> Of(string keyOrExtension, Op? key = null) {
        Op op = key.OrDefault();
        return from text in op.AcceptText(value: keyOrExtension)
               from _reserved in guard(!Reserved.Contains(text), op.InvalidInput()).ToFin()
               from row in FileCodec.Validate(text.TrimStart('.'), null, out FileCodec? named) is null
                   ? Fin.Succ(value: named!)
                   : ByExtension.Value.TryGetValue(text.StartsWith('.') ? text : "." + text, out FileCodec? byExt)
                       ? Fin.Succ(value: byExt)
                       : Fin.Fail<FileCodec>(error: op.InvalidInput())
               select row;
    }

    public static string Filter(CodecPhase phase, Seq<FileCodec> subset = default) =>
        string.Join('|', (subset.IsEmpty ? toSeq(FileCodec.Items) : subset)
            .Filter(row => row.Has(phase.Demands))
            .Map(static row =>
                $"{row.Key.ToUpperInvariant()} ({string.Join(", ", row.Extensions.Map(static e => "*" + e))})"
                + $"|{string.Join(';', row.Extensions.Map(static e => "*" + e))}"));

    internal static Fin<Unit> Read(RhinoDoc document, DocumentPath path, FileCodec codec, CodecTune tune, FileReadOptions carrier, Op? key = null) {
        Op op = key.OrDefault();
        return from _ability in guard(codec.Has(CodecAbility.Import), op.InvalidInput()).ToFin()
               from done in op.Catch(() => codec.ReadEngine(tune: tune, carrier: carrier, document: document, path: path.Value, key: op))
               select done;
    }

    internal static Fin<Unit> Write(RhinoDoc document, DocumentPath path, FileCodec codec, CodecTune tune, FileWriteOptions carrier, Op? key = null) {
        Op op = key.OrDefault();
        return from _ability in guard(codec.Has(CodecAbility.Export), op.InvalidInput()).ToFin()
               from done in op.Catch(() => codec.WriteEngine(tune: tune, carrier: carrier, document: document, path: path.Value, key: op))
               select done;
    }
}
```

## [05]-[DIALOG_PORT]

- Owner: `CodecPort` — the host file-dialog seam. `Register` folds every phase-capable row except the host-native `3dm` row into the host `FileTypeList` and records the host-returned index against its row in one committed cell, so the later index-keyed `ReadFile`/`WriteFile` dispatch is a frozen lookup, never a re-parsed extension. The abstract `CodecImportPort`/`CodecExportPort` plug-in bases seal `AddFileTypes` onto that fold and route the host callback through the one matrix entry.
- Law: the index registry is written once per `AddFileTypes` invocation and replaced whole — the host owns registration timing, and a dispatch against an unregistered index is a typed refusal, never an index-out-of-range escape.
- Law: the port dispatches with `CodecTune.Model` and the host-supplied `FileReadOptions`/`FileWriteOptions` carrier — dialog traffic carries host intent (import-versus-open, selected-versus-all) in the carrier, and the tune stays the canonical default because the dialog carries no policy surface.
- Boundary: `Result`/`WriteFileResult` are the host's dialog verdict currencies; the port folds the matrix rail into them at the seam and nothing above the port sees them.

```csharp
// --- [COMPOSITION] --------------------------------------------------------------------------
public static class CodecPort {
    private static readonly Atom<HashMap<int, FileCodec>> ImportIndex = Atom(HashMap<int, FileCodec>());
    private static readonly Atom<HashMap<int, FileCodec>> ExportIndex = Atom(HashMap<int, FileCodec>());

    internal static Unit Register(FileTypeList list, CodecPhase phase) {
        Atom<HashMap<int, FileCodec>> cell = phase == CodecPhase.Import ? ImportIndex : ExportIndex;
        HashMap<int, FileCodec> bound = toSeq(FileCodec.Items)
            .Filter(row => row.Has(phase.Demands) && row != FileCodec.ThreeDm)
            .Fold(HashMap<int, FileCodec>(), (map, row) =>
                list.AddFileType(
                    description: $"{row.Key.ToUpperInvariant()} ({string.Join(", ", row.Extensions)})",
                    extensions: row.Extensions,
                    showOptionsButtonInFileDialog: false) is var index && index >= 0
                    ? map.AddOrUpdate(index, row)
                    : map);
        return ignore(cell.Swap(_ => bound));
    }

    internal static Fin<FileCodec> Registered(int index, CodecPhase phase, Op key) =>
        (phase == CodecPhase.Import ? ImportIndex : ExportIndex).Value
            .Find(index)
            .ToFin(Fail: key.InvalidInput());
}

public abstract class CodecImportPort : FileImportPlugIn {
    protected sealed override void AddFileTypes(FileTypeList list, FileReadOptions options) =>
        ignore(CodecPort.Register(list: list, phase: CodecPhase.Import));

    protected sealed override Result ReadFile(string filename, int index, RhinoDoc doc, FileReadOptions options) {
        Op op = Op.Of();
        Fin<Unit> outcome =
            from codec in CodecPort.Registered(index: index, phase: CodecPhase.Import, key: op)
            from path in DocumentPath.Validate(filename, null, out DocumentPath admitted) is null
                ? Fin.Succ(value: admitted)
                : Fin.Fail<DocumentPath>(error: op.InvalidInput())
            from done in Codecs.Read(document: doc, path: path, codec: codec, tune: CodecTune.Model, carrier: options, key: op)
            select done;
        return outcome.Match(Succ: static _ => Result.Success, Fail: static _ => Result.Failure);
    }
}

public abstract class CodecExportPort : FileExportPlugIn {
    protected sealed override void AddFileTypes(FileTypeList list, FileWriteOptions options) =>
        ignore(CodecPort.Register(list: list, phase: CodecPhase.Export));

    protected sealed override WriteFileResult WriteFile(string filename, int index, RhinoDoc doc, FileWriteOptions options) {
        Op op = Op.Of();
        Fin<Unit> outcome =
            from codec in CodecPort.Registered(index: index, phase: CodecPhase.Export, key: op)
            from path in DocumentPath.Validate(filename, null, out DocumentPath admitted) is null
                ? Fin.Succ(value: admitted)
                : Fin.Fail<DocumentPath>(error: op.InvalidInput())
            from done in Codecs.Write(document: doc, path: path, codec: codec, tune: CodecTune.Model, carrier: options, key: op)
            select done;
        return outcome.Match(Succ: static _ => WriteFileResult.Success, Fail: static _ => WriteFileResult.Failure);
    }
}
```

```mermaid
flowchart LR
    Rows["FileCodec rows — extensions · abilities · engine columns · option projections"] --> Lookup["derived: Detect · Of · Filter · EnsureExtension"]
    Rows --> Entry["Codecs.Read / Codecs.Write — one entry per direction"]
    Entry -->|ability gate, then one dispatch| Direct["engine column — typed host options from CodecTune, attached lane inside the row"]
    Rows --> Port["CodecPort — FileTypeList registration, index registry"]
    Port --> Dialog["CodecImportPort.ReadFile / CodecExportPort.WriteFile — index-keyed dispatch"]
    Dialog --> Entry
```
