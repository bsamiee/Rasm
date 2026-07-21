# [RASM_RHINO_OPTIONS]

`FormatDial` owns per-codec option policy. `DialSeat` proves codec-phase correspondence once, each case mints one complete Rhino carrier, and polymorphic `Dials.Resolve` applies the selected case without a format-named resolver roster.

## [01]-[INDEX]

- [02]-[SHARED_AXES]: `SubDForm` and `CsvColumn` the cross-host vocabularies, `DracoDial` and `ObjNgonDial` the admitted compression and n-gon values, and `IgesIdentity`/`IgesFitPolicy`/`IgesSurfaceForm`/`VdaHeader` the header and policy sub-records.
- [03]-[DIAL_FAMILY]: `FormatDial` — the closed per-format case family with one `Mint` per case and the `Seat` correspondence.
- [04]-[DIAL_BINDING]: `Dials.Resolve` — the polymorphic case resolver and scale-lens composition.

## [02]-[SHARED_AXES]

- Owner: `SubDForm` `[SmartEnum<int>]` — the SubD tessellation vocabulary whose columns carry both host enums (`FileObjWriteOptions.SubDMeshing`, `FileGltfWriteOptions.SubDMeshing` — identical `Surface`/`ControlNet` rosters, two host types), so one domain row serves both consumers. `CsvColumn` `[SmartEnum<int>]` — the CSV column set: each row carries its tune-baseline predicate and its host setter, so column membership is set algebra over one vocabulary, never twenty-six parallel booleans. `DracoDial` and `ObjNgonDial` — admitted values for the host's clamped compression bands and n-gon cluster, the n-gon mode admitted against the host enum roster before any mint. `IgesIdentity`, `IgesFitPolicy`, `IgesSurfaceForm`, and `VdaHeader` — structural policy products; the fit policy is one shape the IGES case instantiates for both entity slots because the host columns share semantics and defaults.
- Law: a shared vocabulary is earned only by two or more host enums sharing one roster — `SubDForm` qualifies; a single-host enum rides its case field directly as boundary material, because a one-to-one `[SmartEnum]` mirror restates host truth.
- Law: `Option<FieldOverride<T>>` carries an optional enable-plus-value override: `None` and `Some(FieldOverride<T>.Keep)` retain the baseline, `Some(new FieldOverride<T>.SetCase(Value: value))` writes gate-plus-value, and `Some(new FieldOverride<T>.ClearCase())` forces the gate off. A class-shaped `FieldOverride<T>` never uses `default` as `Keep`.
- Growth: a new cross-host vocabulary is one row set with one column per host enum; a new cluster is one sub-record with its `Apply`.
- Boundary: each `Mint` block is a host-mutation capsule; object initialization and ordered `Iter`/`Apply` statements are the platform-forced statement exemption.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using Rhino;
using Rhino.FileIO;
using Rhino.Geometry;
using System.Runtime.InteropServices;

namespace Rasm.Rhino.Exchange;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SubDForm {
    public static readonly SubDForm Surface = new(key: 0,
        obj: FileObjWriteOptions.SubDMeshing.Surface, gltf: FileGltfWriteOptions.SubDMeshing.Surface);
    public static readonly SubDForm ControlNet = new(key: 1,
        obj: FileObjWriteOptions.SubDMeshing.ControlNet, gltf: FileGltfWriteOptions.SubDMeshing.ControlNet);

    internal FileObjWriteOptions.SubDMeshing Obj { get; }
    internal FileGltfWriteOptions.SubDMeshing Gltf { get; }
}

[SmartEnum<int>]
public sealed partial class CsvColumn {
    public static readonly CsvColumn Header = new(key: 0, selected: static _ => true, write: static (o, v) => { o.Header = v; return unit; });
    public static readonly CsvColumn LayerName = new(key: 1, selected: static tune => tune.Grouped(CodecAxis.Layer), write: static (o, v) => { o.LayerName = v; return unit; });
    public static readonly CsvColumn LayerIndex = new(key: 2, selected: static tune => tune.Grouped(CodecAxis.Layer), write: static (o, v) => { o.LayerIndex = v; return unit; });
    public static readonly CsvColumn LayerColor = new(key: 3, selected: static tune => tune.Grouped(CodecAxis.Layer), write: static (o, v) => { o.LayerColor = v; return unit; });
    public static readonly CsvColumn LayerHierarchy = new(key: 4, selected: static tune => tune.Grouped(CodecAxis.Layer), write: static (o, v) => { o.LayerHierarchy = v; return unit; });
    public static readonly CsvColumn GroupName = new(key: 5, selected: static tune => tune.Grouped(CodecAxis.Block), write: static (o, v) => { o.GroupName = v; return unit; });
    public static readonly CsvColumn GroupIndexes = new(key: 6, selected: static tune => tune.Grouped(CodecAxis.Block), write: static (o, v) => { o.GroupIndexes = v; return unit; });
    public static readonly CsvColumn ObjectName = new(key: 7, selected: static tune => tune.Grouped(CodecAxis.ObjectName), write: static (o, v) => { o.ObjectName = v; return unit; });
    public static readonly CsvColumn ObjectID = new(key: 8, selected: static _ => true, write: static (o, v) => { o.ObjectID = v; return unit; });
    public static readonly CsvColumn ObjectColor = new(key: 9, selected: static tune => tune.Order == CodecAxis.ObjectType, write: static (o, v) => { o.ObjectColor = v; return unit; });
    public static readonly CsvColumn ObjectMaterial = new(key: 10, selected: static tune => tune.Grouped(CodecAxis.Material), write: static (o, v) => { o.ObjectMaterial = v; return unit; });
    public static readonly CsvColumn ObjectDescription = new(key: 11, selected: UserStrings, write: static (o, v) => { o.ObjectDescription = v; return unit; });
    public static readonly CsvColumn Length = new(key: 12, selected: Measured, write: static (o, v) => { o.Length = v; return unit; });
    public static readonly CsvColumn Perimeter = new(key: 13, selected: Measured, write: static (o, v) => { o.Perimeter = v; return unit; });
    public static readonly CsvColumn Area = new(key: 14, selected: Measured, write: static (o, v) => { o.Area = v; return unit; });
    public static readonly CsvColumn Volume = new(key: 15, selected: Measured, write: static (o, v) => { o.Volume = v; return unit; });
    public static readonly CsvColumn AreaCentroid = new(key: 16, selected: Measured, write: static (o, v) => { o.AreaCentroid = v; return unit; });
    public static readonly CsvColumn VolumeCentroid = new(key: 17, selected: Measured, write: static (o, v) => { o.VolumeCentroid = v; return unit; });
    public static readonly CsvColumn AreaMoments = new(key: 18, selected: Measured, write: static (o, v) => { o.AreaMoments = v; return unit; });
    public static readonly CsvColumn VolumeMoments = new(key: 19, selected: Measured, write: static (o, v) => { o.VolumeMoments = v; return unit; });
    public static readonly CsvColumn CumulativeMassProperties = new(key: 20, selected: Measured, write: static (o, v) => { o.CumulativeMassProperties = v; return unit; });
    public static readonly CsvColumn AttributesKeys = new(key: 21, selected: UserStrings, write: static (o, v) => { o.AttributesKeys = v; return unit; });
    public static readonly CsvColumn AttributesTexts = new(key: 22, selected: UserStrings, write: static (o, v) => { o.AttributesTexts = v; return unit; });
    public static readonly CsvColumn ObjectKeys = new(key: 23, selected: UserStrings, write: static (o, v) => { o.ObjectKeys = v; return unit; });
    public static readonly CsvColumn ObjectsTexts = new(key: 24, selected: UserStrings, write: static (o, v) => { o.ObjectsTexts = v; return unit; });

    [UseDelegateFromConstructor]
    internal partial bool Selected(CodecTune tune);

    [UseDelegateFromConstructor]
    internal partial Unit Write(FileCsvWriteOptions options, bool member);

    internal static Seq<CsvColumn> Baseline(CodecTune tune) => toSeq(Items).Filter(row => row.Selected(tune: tune));

    private static bool Measured(CodecTune tune) => tune.Fidelity.Measured;

    private static bool UserStrings(CodecTune tune) => tune.Group == CodecAxis.UserString;
}

// --- [MODELS] -------------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct DracoDial {
    public Dimension Level { get; }
    public Dimension PositionBits { get; }
    public Dimension NormalBits { get; }
    public Dimension TextureBits { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Dimension level,
        ref Dimension positionBits,
        ref Dimension normalBits,
        ref Dimension textureBits) =>
        validationError = level.Value is < 1 or > 10
            ? new ValidationError("Draco compression level must be in [1, 10].")
            : positionBits.Value is < 8 or > 32
                || normalBits.Value is < 8 or > 32
                || textureBits.Value is < 8 or > 32
                ? new ValidationError("Draco quantization bits must be in [8, 32].")
                : null;

    public static Fin<DracoDial> Of(int level, int positionBits, int normalBits, int textureBits, Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(() => Validate(
            level: Dimension.Create(value: level),
            positionBits: Dimension.Create(value: positionBits),
            normalBits: Dimension.Create(value: normalBits),
            textureBits: Dimension.Create(value: textureBits),
            item: out DracoDial value) is null
                ? Fin.Succ(value: value)
                : Fin.Fail<DracoDial>(error: op.InvalidInput()));
    }
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ObjNgonDial {
    public FileObjWriteOptions.NGons Mode { get; }
    public Dimension MinFaces { get; }
    public bool IncludeUnweldedEdges { get; }
    public bool CullInteriorVertexes { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FileObjWriteOptions.NGons mode,
        ref Dimension minFaces,
        ref bool includeUnweldedEdges,
        ref bool cullInteriorVertexes) =>
        validationError = !Enum.IsDefined(value: mode)
            ? new ValidationError("N-gon mode is outside the host roster.")
            : minFaces.Value < 2
                ? new ValidationError("N-gon creation requires at least two faces.")
                : null;

    public static Fin<ObjNgonDial> Of(
        FileObjWriteOptions.NGons mode,
        int minFaces,
        bool includeUnweldedEdges = true,
        bool cullInteriorVertexes = true,
        Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(() => Validate(
            mode: mode,
            minFaces: Dimension.Create(value: minFaces),
            includeUnweldedEdges: includeUnweldedEdges,
            cullInteriorVertexes: cullInteriorVertexes,
            item: out ObjNgonDial value) is null
                ? Fin.Succ(value: value)
                : Fin.Fail<ObjNgonDial>(error: op.InvalidInput()));
    }

    internal Unit Apply(FileObjWriteOptions host) {
        host.NgonMode = Mode;
        host.CreateNgons = Mode == FileObjWriteOptions.NGons.Create;
        host.MinNgonFaceCount = MinFaces.Value;
        host.IncludeUnweldedEdgesInNgons = IncludeUnweldedEdges;
        host.CullUnnecessaryVertexesInNgons = CullInteriorVertexes;
        return unit;
    }
}

public sealed record IgesIdentity(
    string Author = "",
    string Organization = "",
    string Sender = "",
    string Receiver = "",
    bool NotesInStartSection = true);

public sealed record IgesFitPolicy(
    FileIgsWriteOptions.MaxDegreeMode MaxDegree = FileIgsWriteOptions.MaxDegreeMode.MdNoLimit,
    bool Simplify = false,
    bool FitRational = false,
    bool ClampEndKnots = false,
    bool UseParentLabel = true,
    bool ForceBezierKnots = false,
    bool FlagDependentAs03 = false);

public sealed record IgesSurfaceForm(
    FileIgsWriteOptions.SurfacesMode Surfaces = FileIgsWriteOptions.SurfacesMode.Srf143,
    FileIgsWriteOptions.PolySurfacesMode PolySurfaces = FileIgsWriteOptions.PolySurfacesMode.PsrfSeparate,
    FileIgsWriteOptions.SolidsMode Solids = FileIgsWriteOptions.SolidsMode.SldSeparate,
    FileIgsWriteOptions.MeshesMode Meshes = FileIgsWriteOptions.MeshesMode.MeshNone,
    bool SplitClosed = false,
    bool SplitBiPolar = false,
    bool ForceTrimmed = false,
    bool WriteNonPlanarUnitNormal = true);

public sealed record VdaHeader(
    string SendingCompany = "",
    string SendersName = "",
    string TelephoneNumber = "",
    string Address = "",
    string ProjectName = "",
    string ObjectCode = "",
    string Variant = "",
    string Confidentiality = "",
    string DateEffective = "",
    string CompanyName = "",
    string ReceivingDepartment = "");
```

## [03]-[DIAL_FAMILY]

- Owner: `FormatDial` `[Union]` — one case per format direction with an option surface beyond the scale lens, closed under the private-protected root constructor. Every field is an explicit override — `Option<T>` for value members, `Option<FieldOverride<T>>` for enable-plus-value pairs, or an admitted cluster value — and `None`/`Keep` means the baseline, never a second host default. Each case's `Mint` constructs the host option object in one object initializer naming every content-shaping host member with its baseline, then applies its clusters; the fence is the roster.
- Law: baselines are two-tier — where the codec matrix previously derived a member from `CodecTune` (fidelity, grouping, ordering, materials, resources), that derivation IS the baseline; every other member's baseline is the verified host default, so a dial-free call is byte-identical to the pre-dial matrix.
- Law: host dialog and plumbing members (`UseSimpleDialog`, `ActualFilePathOnMac`, `IsDefault`, `Name`) never enter a case — they carry host UI state, not content policy — and immutable host members (`FileObjWriteOptions.AngleTolRadians`) are unreachable by construction.
- Law: `DialSeat` admits the codec-phase product once and rejects any phase whose demanded ability the codec lacks. Every case constructor declares its seat beside its option body, and `Codecs.Apply` reads the generated value without another case roster.
- Law: host redundancies collapse at `Mint` — `FileObjWriteOptions.CreateNgons` derives from `ObjNgonDial.Mode`, each `FileDwgWriteOptions` curve-fit gate derives from its adjacent `Option<FieldOverride<double>>`, and `FileStpReadOptions.LimitFaces` derives from `FaceCap`, so no consumer supplies a second gate.
- Boundary: `FileObjWriteOptions`/`FileObjReadOptions`/`FilePlyWriteOptions` construct over the host `FileWriteOptions`/`FileReadOptions` carrier, so their `Mint` takes the carrier the engine column already holds; `FileXamlWriteOptions` projects through `ToDictionary()` into `RhinoDoc.Export` inside its codec row, and the dial never learns the transport.
- Growth: a new host knob is one override field with its baseline line in `Mint`; a new format direction is one case and one codec engine expression.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct DialSeat {
    public FileCodec Codec { get; }
    public CodecPhase Phase { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref FileCodec codec,
        ref CodecPhase phase) =>
        validationError = codec is not null && phase is not null && codec.Has(phase.Demands)
            ? null
            : new ValidationError("Codec phase is not supported by the selected codec.");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FormatDial {
    private protected FormatDial(FileCodec codec, CodecPhase phase) => Seat = DialSeat.Create(codec: codec, phase: phase);

    internal DialSeat Seat { get; }

    public sealed record ThreeDsWriteCase(
        Option<bool> SaveViews = default,
        Option<bool> SaveLights = default,
        Option<MeshingParameters> Mesh = default) : FormatDial(FileCodec.ThreeDs, CodecPhase.Export) {
        internal File3dsWriteOptions Mint(CodecTune tune) => new() {
            SaveViews = SaveViews.IfNone(tune.Fidelity.IsModel),
            SaveLights = SaveLights.IfNone(tune.Fidelity.IsModel),
            MeshingParameters = Mesh.IfNone(() => MeshingParameters.Default),
        };
    }

    public sealed record ThreeDsReadCase(
        Option<FieldOverride<double>> Unweld = default,
        Option<bool> ImportLights = default,
        Option<bool> ImportCameras = default) : FormatDial(FileCodec.ThreeDs, CodecPhase.Import) {
        internal File3dsReadOptions Mint() {
            File3dsReadOptions host = new() {
                ImportLights = ImportLights.IfNone(true),
                ImportCameras = ImportCameras.IfNone(true),
            };
            _ = Unweld.Iter(field => field.Apply(
                set: angle => { host.Unweld = true; host.UnweldAngle = angle; },
                inherit: () => host.Unweld = false));
            return host;
        }
    }

    public sealed record ThreeMfWriteCase(
        Option<string> Title = default,
        Option<string> Designer = default,
        Option<string> Description = default,
        Option<string> Copyright = default,
        Option<string> LicenseTerms = default,
        Option<string> Rating = default,
        Option<bool> MoveToPositiveOctant = default,
        Seq<(string Key, string Value)> Metadata = default) : FormatDial(FileCodec.ThreeMf, CodecPhase.Export) {
        internal File3mfWriteOptions Mint() {
            File3mfWriteOptions host = new() {
                Title = Title.IfNone(string.Empty),
                Designer = Designer.IfNone(string.Empty),
                Description = Description.IfNone(string.Empty),
                Copyright = Copyright.IfNone(string.Empty),
                LicenseTerms = LicenseTerms.IfNone(string.Empty),
                Rating = Rating.IfNone(string.Empty),
                MoveOutputToPositiveXYZOctant = MoveToPositiveOctant.IfNone(true),
            };
            _ = Metadata.Iter(pair => host.Metadata[pair.Key] = pair.Value);
            return host;
        }
    }

    public sealed record AiWriteCase(
        Option<bool> UseCmyk = default,
        Option<bool> ExportViewBoundary = default,
        Option<bool> HatchesAsSolidFills = default,
        Option<bool> OrderLayers = default) : FormatDial(FileCodec.Ai, CodecPhase.Export) {
        internal FileAiWriteOptions Mint(CodecTune tune) => new() {
            PreserveModelScale = tune.Fidelity.IsModel,
            UseCMYK = UseCmyk.IfNone(false),
            ExportViewBoundary = ExportViewBoundary.IfNone(false),
            ExportHatchesAsSolidFills = HatchesAsSolidFills.IfNone(true),
            OrderLayers = OrderLayers.IfNone(tune.Grouped(CodecAxis.Layer)),
        };
    }

    public sealed record AmfWriteCase(Option<MeshingParameters> Mesh = default) : FormatDial(FileCodec.Amf, CodecPhase.Export) {
        internal FileAmfWriteOptions Mint() => new() { MeshingParameters = Mesh.IfNone(() => MeshingParameters.Default) };
    }

    public sealed record ObjWriteCase(
        Option<FileObjWriteOptions.GeometryType> Geometry = default,
        Option<FileObjWriteOptions.ObjObjectNames> ObjectNames = default,
        Option<FileObjWriteOptions.ObjGroupNames> GroupNames = default,
        Option<FileObjWriteOptions.AsciiEol> Eol = default,
        Option<FileObjWriteOptions.CurveType> TrimCurves = default,
        Option<FileObjWriteOptions.PolylineExportType> Polylines = default,
        Option<FileObjWriteOptions.VertexWelding> Welding = default,
        Option<SubDForm> SubD = default,
        Option<Dimension> SubDDensity = default,
        Option<bool> Materials = default,
        Option<bool> DisplayColorMaterial = default,
        Option<bool> TextureCoordinates = default,
        Option<bool> Normals = default,
        Option<bool> OpenMeshes = default,
        Option<bool> RenderMeshes = default,
        Option<bool> SortGroups = default,
        Option<bool> MergeNestedGroups = default,
        Option<bool> MapZtoY = default,
        Option<Dimension> Digits = default,
        Option<bool> WrapLongLines = default,
        Option<bool> Triangulate = default,
        Option<bool> UnderbarMaterialNames = default,
        Option<bool> RelativeIndexing = default,
        Option<FieldOverride<int>> VertexColors = default,
        Option<ObjNgonDial> Ngons = default,
        Option<MeshingParameters> Mesh = default) : FormatDial(FileCodec.Obj, CodecPhase.Export) {
        internal FileObjWriteOptions Mint(CodecTune tune, FileWriteOptions carrier) {
            FileObjWriteOptions host = new(carrier) {
                ObjectType = Geometry.IfNone(tune.Fidelity.IsModel && !carrier.WriteGeometryOnly
                    ? FileObjWriteOptions.GeometryType.Nurbs : FileObjWriteOptions.GeometryType.Mesh),
                ExportObjectNames = ObjectNames.IfNone(tune.Group == CodecAxis.ObjectName
                    ? FileObjWriteOptions.ObjObjectNames.ObjectAsObject : FileObjWriteOptions.ObjObjectNames.NoObjects),
                ExportGroupNameLayerNames = GroupNames.IfNone(tune.Group == CodecAxis.Layer
                    ? FileObjWriteOptions.ObjGroupNames.LayerAsGroup
                    : tune.Group == CodecAxis.Block
                        ? FileObjWriteOptions.ObjGroupNames.GroupAsGroup
                        : FileObjWriteOptions.ObjGroupNames.NoGroups),
                EolType = Eol.IfNone(FileObjWriteOptions.AsciiEol.Crlf),
                TrimCurveType = TrimCurves.IfNone(FileObjWriteOptions.CurveType.Nurbs),
                PolylineType = Polylines.IfNone(FileObjWriteOptions.PolylineExportType.Bspline),
                MeshType = Welding.IfNone(FileObjWriteOptions.VertexWelding.Normal),
                SubDMeshType = SubD.Map(static row => row.Obj).IfNone(FileObjWriteOptions.SubDMeshing.Surface),
                SubDSurfaceMeshingDensity = SubDDensity.Map(static value => value.Value).IfNone(4),
                ExportMaterialDefinitions = Materials.IfNone(tune.Materials && carrier.WriteUserData),
                UseDisplayColorForMaterial = DisplayColorMaterial.IfNone(tune.Materials),
                ExportTcs = TextureCoordinates.IfNone(tune.Materials),
                ExportNormals = Normals.IfNone(tune.Fidelity.Measured),
                ExportOpenMeshes = OpenMeshes.IfNone(true),
                UseRenderMeshes = RenderMeshes.IfNone(tune.Fidelity == CodecFidelity.Small || carrier.IncludeRenderMeshes),
                SortObjGroups = SortGroups.IfNone(tune.Order == CodecAxis.Layer || tune.Order == CodecAxis.Block),
                MergeNestedGroupingNames = MergeNestedGroups.IfNone(tune.Group == CodecAxis.Layer),
                MapZtoY = MapZtoY.IfNone(false),
                SignificantDigits = Digits.Map(static value => value.Value).IfNone(17),
                WrapLongLines = WrapLongLines.IfNone(false),
                ExportAsTriangles = Triangulate.IfNone(false),
                UnderbarMaterialNames = UnderbarMaterialNames.IfNone(false),
                UseRelativeIndexing = RelativeIndexing.IfNone(false),
                MeshParameters = Mesh.IfNone(() => MeshingParameters.Default),
            };
            _ = VertexColors.Iter(field => field.Apply(
                set: format => { host.ExportVcs = true; host.VcsFormat = format; },
                inherit: () => host.ExportVcs = false));
            _ = Ngons.Iter(dial => dial.Apply(host: host));
            return host;
        }
    }

    public sealed record ObjReadCase(
        Option<FileObjReadOptions.UseObjGsAs> Groups = default,
        Option<FileObjReadOptions.UseObjOsAs> Objects = default,
        Option<bool> MapYtoZ = default,
        Option<bool> MorphTargetOnly = default,
        Option<bool> ReverseGroupOrder = default,
        Option<bool> IgnoreTextures = default,
        Option<bool> DisplayColorFromMaterial = default,
        Option<bool> Split32BitTextures = default) : FormatDial(FileCodec.Obj, CodecPhase.Import) {
        internal FileObjReadOptions Mint(FileReadOptions carrier) => new(carrier) {
            UseObjGroupsAs = Groups.IfNone(FileObjReadOptions.UseObjGsAs.ObjGroupsAsObjects),
            UseObjObjectsAs = Objects.IfNone(FileObjReadOptions.UseObjOsAs.IgnoreObjObjects),
            MapYtoZ = MapYtoZ.IfNone(false),
            MorphTargetOnly = MorphTargetOnly.IfNone(false),
            ReverseGroupOrder = ReverseGroupOrder.IfNone(false),
            IgnoreTextures = IgnoreTextures.IfNone(false),
            DisplayColorFromObjMaterial = DisplayColorFromMaterial.IfNone(true),
            Split32BitTextures = Split32BitTextures.IfNone(false),
        };
    }

    public sealed record PlyWriteCase(
        Option<bool> Ascii = default,
        Option<bool> Doubles = default,
        Option<bool> Normals = default,
        Option<bool> Colors = default,
        Option<bool> Material = default,
        Option<MeshingParameters> Mesh = default) : FormatDial(FileCodec.Ply, CodecPhase.Export) {
        internal FilePlyWriteOptions Mint(CodecTune tune, FileWriteOptions carrier) => new(carrier) {
            ExportASCII = Ascii.IfNone(tune.Fidelity != CodecFidelity.Small),
            ExportDoubles = Doubles.IfNone(tune.Fidelity.IsModel),
            ExportNormals = Normals.IfNone(tune.Fidelity.Measured),
            ExportColors = Colors.IfNone(tune.Materials),
            ExportMaterial = Material.IfNone(tune.Resources == CodecResource.Embed),
            MeshingParameters = Mesh.IfNone(() => MeshingParameters.Default),
        };
    }

    public sealed record PlyReadCase(Option<UnitSystem> Units = default) : FormatDial(FileCodec.Ply, CodecPhase.Import) {
        internal FilePlyReadOptions Mint() => new() { PLYModelUnits = Units.IfNone(UnitSystem.Millimeters) };
    }

    public sealed record CdWriteCase(Option<MeshingParameters> Mesh = default) : FormatDial(FileCodec.Cd, CodecPhase.Export) {
        internal FileCdWriteOptions Mint() => new() { MeshingParameters = Mesh.IfNone(() => MeshingParameters.Default) };
    }

    public sealed record DgnReadCase(
        Option<bool> ImportUnreferencedLayers = default,
        Option<bool> ImportUnreferencedBlocks = default,
        Option<bool> ImportUnreferencedLineStyles = default,
        Option<bool> ImportViews = default,
        Option<bool> GroupCellHeaders = default) : FormatDial(FileCodec.Dgn, CodecPhase.Import) {
        internal FileDgnReadOptions Mint() => new() {
            ImportUnreferencedLayers = ImportUnreferencedLayers.IfNone(false),
            ImportUnreferencedBlocks = ImportUnreferencedBlocks.IfNone(false),
            ImportUnreferencedLineStyles = ImportUnreferencedLineStyles.IfNone(true),
            ImportViews = ImportViews.IfNone(false),
            GroupCellHeaders = GroupCellHeaders.IfNone(true),
        };
    }

    public sealed record DstReadCase(Option<bool> ImportJumps = default) : FormatDial(FileCodec.Dst, CodecPhase.Import) {
        internal FileDstReadOptions Mint() => new() { ImportJumps = ImportJumps.IfNone(false) };
    }

    public sealed record DwgWriteCase(
        Option<FileDwgWriteOptions.AutocadVersion> Version = default,
        Option<FileDwgWriteOptions.ExportSurfaceMode> SurfacesAs = default,
        Option<FileDwgWriteOptions.ExportMeshMode> MeshesAs = default,
        Option<FileDwgWriteOptions.ExportLineMode> LinesAs = default,
        Option<FileDwgWriteOptions.ExportArcMode> ArcsAs = default,
        Option<FileDwgWriteOptions.ExportSplineMode> SplinesAs = default,
        Option<FileDwgWriteOptions.ExportPolylineMode> PolylinesAs = default,
        Option<FileDwgWriteOptions.ExportPolycurveMode> PolycurvesAs = default,
        Option<FileDwgWriteOptions.FlattenMode> Flatten = default,
        Option<FileDwgWriteOptions.ColorMethodType> ColorMethod = default,
        Option<FileDwgWriteOptions.UseColorType> UseColor = default,
        Option<bool> FullLayerPath = default,
        Option<bool> UseLWPolylines = default,
        Option<double> SimplifyTolerance = default,
        Option<double> MinPointDistance = default,
        Option<bool> SplitPolycurves = default,
        Option<bool> SplitSplines = default,
        Option<bool> Simplify = default,
        Option<bool> NoDxfHeader = default,
        Option<bool> PreserveArcNormals = default,
        Option<bool> WriteThickCurves = default,
        Option<FieldOverride<double>> CurveMaxAngleDegrees = default,
        Option<FieldOverride<double>> CurveChordHeight = default,
        Option<FieldOverride<double>> CurveSegmentLength = default) : FormatDial(FileCodec.Dwg, CodecPhase.Export) {
        internal FileDwgWriteOptions Mint(CodecTune tune) {
            FileDwgWriteOptions host = new() {
                Version = Version.IfNone(FileDwgWriteOptions.AutocadVersion.Acad2018),
                ExportSurfacesAs = SurfacesAs.IfNone(tune.Fidelity == CodecFidelity.GeometryOnly
                    ? FileDwgWriteOptions.ExportSurfaceMode.Meshes : FileDwgWriteOptions.ExportSurfaceMode.Curves),
                ExportMeshesAs = MeshesAs.IfNone(FileDwgWriteOptions.ExportMeshMode.Meshes),
                ExportLinesAs = LinesAs.IfNone(FileDwgWriteOptions.ExportLineMode.Lines),
                ExportArcsAs = ArcsAs.IfNone(FileDwgWriteOptions.ExportArcMode.Arcs),
                ExportSplinesAs = SplinesAs.IfNone(FileDwgWriteOptions.ExportSplineMode.Splines),
                ExportPolylinesAs = PolylinesAs.IfNone(FileDwgWriteOptions.ExportPolylineMode.Polylines),
                ExportPolycurvesAs = PolycurvesAs.IfNone(FileDwgWriteOptions.ExportPolycurveMode.Splines),
                Flatten = Flatten.IfNone(FileDwgWriteOptions.FlattenMode.None),
                ColorMethod = ColorMethod.IfNone(tune.Materials
                    ? FileDwgWriteOptions.ColorMethodType.RGB : FileDwgWriteOptions.ColorMethodType.ACI),
                UseColor = UseColor.IfNone(tune.Order == CodecAxis.Material
                    ? FileDwgWriteOptions.UseColorType.USEPRINT : FileDwgWriteOptions.UseColorType.USEDISPLAY),
                FullLayerPath = FullLayerPath.IfNone(tune.Group == CodecAxis.Layer),
                UseLWPolylines = UseLWPolylines.IfNone(!tune.Fidelity.IsModel),
                SimplifyTolerance = SimplifyTolerance.IfNone(0.05),
                MinPointDistance = MinPointDistance.IfNone(1e-06),
                SplitPolycurves = SplitPolycurves.IfNone(true),
                SplitSplines = SplitSplines.IfNone(false),
                Simplify = Simplify.IfNone(false),
                NoDxfHeader = NoDxfHeader.IfNone(false),
                PreserveArcNormals = PreserveArcNormals.IfNone(true),
                WriteThickCurves = WriteThickCurves.IfNone(false),
            };
            _ = CurveMaxAngleDegrees.Iter(field => field.Apply(
                set: value => { host.CurveUseMaxAngle = true; host.CurveMaxAngleDegrees = value; },
                inherit: () => host.CurveUseMaxAngle = false));
            _ = CurveChordHeight.Iter(field => field.Apply(
                set: value => { host.CurveUseChordHeight = true; host.CurveChordHeight = value; },
                inherit: () => host.CurveUseChordHeight = false));
            _ = CurveSegmentLength.Iter(field => field.Apply(
                set: value => { host.CurveUseSegmentLength = true; host.CurveSegmentLength = value; },
                inherit: () => host.CurveUseSegmentLength = false));
            return host;
        }
    }

    public sealed record DwgReadCase(
        Option<bool> ImportUnreferencedLayers = default,
        Option<bool> ImportUnreferencedBlocks = default,
        Option<bool> ImportUnreferencedLinetypes = default,
        Option<bool> WidePolylinesAsSurfaces = default,
        Option<bool> IgnoreThickness = default,
        Option<bool> RegionsAsCurves = default,
        Option<bool> MakeExtrusions = default,
        Option<FileDwgReadOptions.MeshPrecisionMode> MeshPrecision = default,
        Option<UnitSystem> ModelUnits = default,
        Option<UnitSystem> LayoutUnits = default,
        Option<bool> LayerMaterialFromColor = default,
        Option<bool> NestLayers = default) : FormatDial(FileCodec.Dwg, CodecPhase.Import) {
        internal FileDwgReadOptions Mint() => new() {
            ImportUnreferencedLayers = ImportUnreferencedLayers.IfNone(true),
            ImportUnreferencedBlocks = ImportUnreferencedBlocks.IfNone(true),
            ImportUnreferencedLinetypes = ImportUnreferencedLinetypes.IfNone(true),
            ConvertWidePolylinesToSurfaces = WidePolylinesAsSurfaces.IfNone(false),
            IgnoreThickness = IgnoreThickness.IfNone(false),
            ConvertRegionsToCurves = RegionsAsCurves.IfNone(false),
            MakeExtrusions = MakeExtrusions.IfNone(true),
            MeshPrecision = MeshPrecision.IfNone(FileDwgReadOptions.MeshPrecisionMode.Automatic),
            ModelUnits = ModelUnits.IfNone(UnitSystem.Millimeters),
            LayoutUnits = LayoutUnits.IfNone(UnitSystem.Millimeters),
            SetLayerMaterialToLayerColor = LayerMaterialFromColor.IfNone(false),
            NestLayers = NestLayers.IfNone(false),
        };
    }

    public sealed record StlWriteCase(
        Option<bool> Binary = default,
        Option<bool> ExportOpenObjects = default,
        Option<MeshingParameters> Mesh = default) : FormatDial(FileCodec.Stl, CodecPhase.Export) {
        internal FileStlWriteOptions Mint() => new() {
            BinaryFile = Binary.IfNone(true),
            ExportOpenObjects = ExportOpenObjects.IfNone(true),
            MeshingParameters = Mesh.IfNone(() => MeshingParameters.Default),
        };
    }

    public sealed record StlReadCase(
        Option<FieldOverride<double>> Weld = default,
        Option<bool> SplitDisjointMeshes = default,
        Option<UnitSystem> Units = default) : FormatDial(FileCodec.Stl, CodecPhase.Import) {
        internal FileStlReadOptions Mint() {
            FileStlReadOptions host = new() {
                SplitDisjointMeshes = SplitDisjointMeshes.IfNone(true),
                STLModelUnits = Units.IfNone(UnitSystem.Millimeters),
            };
            _ = Weld.Iter(field => field.Apply(
                set: angle => { host.Weld = true; host.WeldAngle = angle; },
                inherit: () => host.Weld = false));
            return host;
        }
    }

    public sealed record StpWriteCase(
        Option<FileStpWriteOptions.StepSchema> Schema = default,
        Option<bool> Export2dCurves = default,
        Option<bool> ExportBlack = default,
        Option<bool> SplitClosedSurfaces = default) : FormatDial(FileCodec.Stp, CodecPhase.Export) {
        internal FileStpWriteOptions Mint() => new() {
            Schema = Schema.IfNone(FileStpWriteOptions.StepSchema.SF_203),
            Export2dCurves = Export2dCurves.IfNone(false),
            ExportBlack = ExportBlack.IfNone(true),
            SplitClosedSurfaces = SplitClosedSurfaces.IfNone(false),
        };
    }

    public sealed record StpReadCase(
        Option<bool> JoinSurfaces = default,
        Option<FieldOverride<Dimension>> FaceCap = default) : FormatDial(FileCodec.Stp, CodecPhase.Import) {
        internal FileStpReadOptions Mint() {
            FileStpReadOptions host = new() { JoinSurfaces = JoinSurfaces.IfNone(true) };
            _ = FaceCap.Iter(field => field.Apply(
                set: cap => { host.LimitFaces = true; host.MaxFaceCount = cap.Value; },
                inherit: () => host.LimitFaces = false));
            return host;
        }
    }

    public sealed record FbxWriteCase(
        Option<FileFbxWriteOptions.ObjectType> SaveObjectsAs = default,
        Option<FileFbxWriteOptions.MaterialType> SaveMaterialsAs = default,
        Option<FileFbxWriteOptions.FileType> SaveFileAs = default,
        Option<bool> SaveViews = default,
        Option<bool> SaveLights = default,
        Option<bool> SaveVertexNormals = default,
        Option<bool> MapZtoY = default,
        Option<MeshingParameters> Mesh = default) : FormatDial(FileCodec.Fbx, CodecPhase.Export) {
        internal FileFbxWriteOptions Mint(CodecTune tune) => new() {
            SaveObjectsAs = SaveObjectsAs.IfNone(tune.Fidelity.IsModel
                ? FileFbxWriteOptions.ObjectType.Nurbs : FileFbxWriteOptions.ObjectType.Mesh),
            SaveMaterialsAs = SaveMaterialsAs.IfNone(FileFbxWriteOptions.MaterialType.Phong),
            SaveFileAs = SaveFileAs.IfNone(FileFbxWriteOptions.FileType.Binary7),
            SaveViews = SaveViews.IfNone(tune.Fidelity.IsModel),
            SaveLights = SaveLights.IfNone(tune.Fidelity.IsModel),
            SaveVertexNormals = SaveVertexNormals.IfNone(tune.Fidelity.Measured),
            MapRhinoZtoFbxY = MapZtoY.IfNone(false),
            MeshingParameters = Mesh.IfNone(() => MeshingParameters.Default),
        };
    }

    public sealed record FbxReadCase(
        Option<FieldOverride<double>> Unweld = default,
        Option<bool> MeshesAsSubD = default,
        Option<bool> ImportLights = default,
        Option<bool> ImportCameras = default,
        Option<bool> MapYtoZ = default) : FormatDial(FileCodec.Fbx, CodecPhase.Import) {
        internal FileFbxReadOptions Mint() {
            FileFbxReadOptions host = new() {
                ImportMeshesAsSubD = MeshesAsSubD.IfNone(false),
                ImportLights = ImportLights.IfNone(true),
                ImportCameras = ImportCameras.IfNone(true),
                MapFbxYtoRhinoZ = MapYtoZ.IfNone(false),
            };
            _ = Unweld.Iter(field => field.Apply(
                set: angle => { host.Unweld = true; host.UnweldAngle = angle; },
                inherit: () => host.Unweld = false));
            return host;
        }
    }

    public sealed record GhsReadCase(
        Option<FileGHSReadOptions.ReadViewType> ViewType = default,
        Option<bool> AttachGhsData = default,
        Option<bool> RemoveColinearPoints = default) : FormatDial(FileCodec.Ghs, CodecPhase.Import) {
        internal FileGHSReadOptions Mint() => new() {
            ViewType = ViewType.IfNone(FileGHSReadOptions.ReadViewType.Solid),
            AttachGhsData = AttachGhsData.IfNone(true),
            RemoveColinearPoints = RemoveColinearPoints.IfNone(true),
        };
    }

    public sealed record GtsWriteCase(Option<MeshingParameters> Mesh = default) : FormatDial(FileCodec.Gts, CodecPhase.Export) {
        internal FileGtsWriteOptions Mint() => new() { MeshingParameters = Mesh.IfNone(() => MeshingParameters.Default) };
    }

    public sealed record IgsWriteCase(
        Option<IgesIdentity> Identity = default,
        Option<UnitSystem> Units = default,
        Option<double> Tolerance = default,
        Option<FileIgsWriteOptions.IgeswVersionMode> Version = default,
        Option<FileIgsWriteOptions.EolMode> Eol = default,
        Option<FileIgsWriteOptions.IgesStringTypeMode> Text = default,
        Option<FileIgsWriteOptions.PointObjectsMode> Points = default,
        Option<IgesFitPolicy> Curves = default,
        Option<bool> CompositeCurves = default,
        Option<IgesFitPolicy> SurfaceFit = default,
        Option<IgesSurfaceForm> Surfaces = default,
        Option<double> Scale = default,
        Option<bool> HideDependentObjects = default,
        Option<bool> DoublesUseE = default,
        Option<bool> NoZerosInTSection = default,
        Option<bool> RenderColorAsIgesColor = default,
        Option<(Dimension Version, double Tolsize)> Catia = default) : FormatDial(FileCodec.Igs, CodecPhase.Export) {
        internal FileIgsWriteOptions Mint() {
            IgesIdentity identity = Identity.IfNone(static () => new IgesIdentity());
            IgesFitPolicy curves = Curves.IfNone(static () => new IgesFitPolicy());
            IgesFitPolicy surfaceFit = SurfaceFit.IfNone(static () => new IgesFitPolicy());
            IgesSurfaceForm surfaces = Surfaces.IfNone(static () => new IgesSurfaceForm());
            FileIgsWriteOptions host = new() {
                Author = identity.Author,
                Organization = identity.Organization,
                Sender = identity.Sender,
                Receiver = identity.Receiver,
                NotesInStartSection = identity.NotesInStartSection,
                Units = Units.IfNone(UnitSystem.Millimeters),
                Tolerance = Tolerance.IfNone(0.001),
                IgesVersion = Version.IfNone(FileIgsWriteOptions.IgeswVersionMode.Igv52),
                EolType = Eol.IfNone(FileIgsWriteOptions.EolMode.Crlf),
                IgesStringType = Text.IfNone(FileIgsWriteOptions.IgesStringTypeMode.Unicode),
                PointType = Points.IfNone(FileIgsWriteOptions.PointObjectsMode.PoSeparate),
                CurveMaxDegree = curves.MaxDegree,
                CompositeCurvesAsSingleBsplines = CompositeCurves.IfNone(false),
                SimplifyCurves = curves.Simplify,
                FitRationalCurves = curves.FitRational,
                ClampCurveEndKnots = curves.ClampEndKnots,
                UseParentLabelOnCurves = curves.UseParentLabel,
                ForceBezierKnotsOnCurves = curves.ForceBezierKnots,
                FlagDependentCurvesAs03 = curves.FlagDependentAs03,
                SurfaceType = surfaces.Surfaces,
                PolySurfaceType = surfaces.PolySurfaces,
                SolidType = surfaces.Solids,
                MeshType = surfaces.Meshes,
                MaxSurfaceDegree = surfaceFit.MaxDegree,
                SimplifySurfaces = surfaceFit.Simplify,
                FitRationalSurfaces = surfaceFit.FitRational,
                ClampSurfaceEndKnots = surfaceFit.ClampEndKnots,
                UseParentLabelOnSurfaces = surfaceFit.UseParentLabel,
                ForceBezierKnotsOnSurfaces = surfaceFit.ForceBezierKnots,
                FlagDependentSurfacesAs03 = surfaceFit.FlagDependentAs03,
                SplitClosedSurfaces = surfaces.SplitClosed,
                SplitBiPolarSurfaces = surfaces.SplitBiPolar,
                ForceTrimmedSurfaces = surfaces.ForceTrimmed,
                WriteNonPlanarUnitNormal = surfaces.WriteNonPlanarUnitNormal,
                Scale = Scale.IfNone(1.0),
                HideDependentObjects = HideDependentObjects.IfNone(false),
                DoublesUseE = DoublesUseE.IfNone(false),
                NoZerosInTSection = NoZerosInTSection.IfNone(false),
                RenderColorAsIgesColor = RenderColorAsIgesColor.IfNone(false),
            };
            _ = Catia.Iter(pair => { host.CatiaVersion = pair.Version.Value; host.CatiaTolsize = pair.Tolsize; });
            return host;
        }
    }

    public sealed record LwoWriteCase(
        Option<bool> WriteVersion6 = default,
        Option<MeshingParameters> Mesh = default) : FormatDial(FileCodec.Lwo, CodecPhase.Export) {
        internal FileLwoWriteOptions Mint() => new() {
            WriteVersion6 = WriteVersion6.IfNone(true),
            MeshingParameters = Mesh.IfNone(() => MeshingParameters.Default),
        };
    }

    public sealed record LwoReadCase(Option<FieldOverride<double>> Unweld = default) : FormatDial(FileCodec.Lwo, CodecPhase.Import) {
        internal FileLwoReadOptions Mint() {
            FileLwoReadOptions host = new();
            _ = Unweld.Iter(field => field.Apply(
                set: angle => { host.Unweld = true; host.UnweldAngle = angle; },
                inherit: () => host.Unweld = false));
            return host;
        }
    }

    public sealed record NwdWriteCase(
        Option<NavisWorksVersion> Version = default,
        Option<MeshingParameters> Mesh = default) : FormatDial(FileCodec.Nwd, CodecPhase.Export) {
        internal FileNwdWriteOptions Mint() => new() {
            Version = Version.IfNone(NavisWorksVersion.Navisworks2016),
            MeshingParameters = Mesh.IfNone(() => MeshingParameters.Default),
        };
    }

    public sealed record PovWriteCase(
        Option<bool> ExportAsOneFile = default,
        Option<MeshingParameters> Mesh = default) : FormatDial(FileCodec.Pov, CodecPhase.Export) {
        internal FilePovWriteOptions Mint() => new() {
            ExportAsOneFile = ExportAsOneFile.IfNone(true),
            MeshingParameters = Mesh.IfNone(() => MeshingParameters.Default),
        };
    }

    public sealed record RawWriteCase(Option<MeshingParameters> Mesh = default) : FormatDial(FileCodec.Raw, CodecPhase.Export) {
        internal FileRawWriteOptions Mint() => new() { MeshingParameters = Mesh.IfNone(() => MeshingParameters.Default) };
    }

    public sealed record RawReadCase(Option<UnitSystem> Units = default) : FormatDial(FileCodec.Raw, CodecPhase.Import) {
        internal FileRawReadOptions Mint() => new() { RawModelUnits = Units.IfNone(UnitSystem.Millimeters) };
    }

    public sealed record SatWriteCase(Option<FileSatWriteOptions.SatTypes> Type = default) : FormatDial(FileCodec.Sat, CodecPhase.Export) {
        internal FileSatWriteOptions Mint() => new() { Type = Type.IfNone(FileSatWriteOptions.SatTypes.Default) };
    }

    public sealed record SkpWriteCase(
        Option<FileSkpWriteOptions.SketchUpVersion> Version = default,
        Option<bool> PlanarRegionsAsPolygons = default,
        Option<bool> GroupObjects = default,
        Option<double> MaxAngle = default) : FormatDial(FileCodec.Skp, CodecPhase.Export) {
        internal FileSkpWriteOptions Mint(CodecTune tune) => new() {
            Version = Version.IfNone(FileSkpWriteOptions.SketchUpVersion.SketchUp2021),
            ExportPlanarRegionsAsPolygons = PlanarRegionsAsPolygons.IfNone(true),
            GroupObjects = GroupObjects.IfNone(tune.Group == CodecAxis.Layer || tune.Group == CodecAxis.Block),
            MaxAngle = MaxAngle.IfNone(15.0),
        };
    }

    public sealed record SkpReadCase(
        Option<bool> FacesAsMeshes = default,
        Option<bool> ImportCurves = default,
        Option<bool> JoinEdges = default,
        Option<bool> JoinFaces = default,
        Option<FieldOverride<double>> Weld = default,
        Option<bool> UseGroupLayers = default,
        Option<bool> AddObjectsToGroups = default,
        Option<bool> EmbedTextures = default,
        Option<bool> UseSketchUpTextureWriter = default,
        Option<int> DisplayColorBy = default) : FormatDial(FileCodec.Skp, CodecPhase.Import) {
        internal FileSkpReadOptions Mint() {
            FileSkpReadOptions host = new() {
                ImportFacesAsMeshes = FacesAsMeshes.IfNone(true),
                ImportCurves = ImportCurves.IfNone(false),
                JoinEdges = JoinEdges.IfNone(true),
                JoinFaces = JoinFaces.IfNone(true),
                UseGroupLayers = UseGroupLayers.IfNone(false),
                AddObjectsToGroups = AddObjectsToGroups.IfNone(true),
                EmbedTexturesInModel = EmbedTextures.IfNone(false),
                UseSketchUpTextureWriter = UseSketchUpTextureWriter.IfNone(false),
                DisplayColorBy = DisplayColorBy.IfNone(0),
            };
            _ = Weld.Iter(field => field.Apply(
                set: angle => { host.Weld = true; host.WeldAngle = angle; },
                inherit: () => host.Weld = false));
            return host;
        }
    }

    public sealed record SlcWriteCase(
        Option<Point3d> Start = default,
        Option<Point3d> End = default,
        Option<double> SliceDistance = default,
        Option<bool> UseMeshes = default,
        Option<double> SegmentAngleDegrees = default) : FormatDial(FileCodec.Slc, CodecPhase.Export) {
        internal FileSlcWriteOptions Mint() => new() {
            StartPoint = Start.IfNone(new Point3d(x: 0.0, y: 0.0, z: 0.0)),
            EndPoint = End.IfNone(new Point3d(x: 0.0, y: 0.0, z: 1.0)),
            SliceDistance = SliceDistance.IfNone(0.0381),
            UseMeshes = UseMeshes.IfNone(true),
            AngleBetweenSegmentsDegrees = SegmentAngleDegrees.IfNone(5.0),
        };
    }

    public sealed record SwReadCase(
        Option<bool> PartsAsBlocks = default,
        Option<bool> RotateYtoZ = default,
        Option<bool> ImportConstructionGeometry = default) : FormatDial(FileCodec.Sw, CodecPhase.Import) {
        internal FileSwReadOptions Mint() => new() {
            ImportPartsAsBlocks = PartsAsBlocks.IfNone(false),
            RotateYtoZ = RotateYtoZ.IfNone(true),
            ImportConstructionGeometry = ImportConstructionGeometry.IfNone(false),
        };
    }

    public sealed record UdoWriteCase(Option<MeshingParameters> Mesh = default) : FormatDial(FileCodec.Udo, CodecPhase.Export) {
        internal FileUdoWriteOptions Mint() => new() { MeshingParameters = Mesh.IfNone(() => MeshingParameters.Default) };
    }

    public sealed record VdaWriteCase(
        Option<VdaHeader> Header = default,
        Option<bool> PointDeviationHairsAsMdi = default) : FormatDial(FileCodec.Vda, CodecPhase.Export) {
        internal FileVdaWriteOptions Mint() {
            VdaHeader header = Header.IfNone(static () => new VdaHeader());
            return new() {
                SendingCompany = header.SendingCompany,
                SendersName = header.SendersName,
                TelephoneNumber = header.TelephoneNumber,
                Address = header.Address,
                ProjectName = header.ProjectName,
                ObjectCode = header.ObjectCode,
                Variant = header.Variant,
                Confidentiality = header.Confidentiality,
                DateEffective = header.DateEffective,
                CompanyName = header.CompanyName,
                ReceivingDepartment = header.ReceivingDepartment,
                PointDeviationHairsAsMDI = PointDeviationHairsAsMdi.IfNone(false),
            };
        }
    }

    public sealed record VrmlWriteCase(
        Option<int> Version = default,
        Option<bool> TextureCoordinates = default,
        Option<bool> VertexNormals = default,
        Option<bool> VertexColors = default,
        Option<MeshingParameters> Mesh = default) : FormatDial(FileCodec.Vrml, CodecPhase.Export) {
        internal FileVrmlWriteOptions Mint(CodecTune tune) => new() {
            Version = Version.IfNone(1),
            ExportTextureCoordinates = TextureCoordinates.IfNone(tune.Materials),
            ExportVertexNormals = VertexNormals.IfNone(tune.Fidelity.IsModel),
            ExportVertexColors = VertexColors.IfNone(false),
            MeshingParameters = Mesh.IfNone(() => MeshingParameters.Default),
        };
    }

    public sealed record X3dvWriteCase(
        Option<bool> TextureCoordinates = default,
        Option<bool> VertexNormals = default,
        Option<bool> VertexColors = default,
        Option<MeshingParameters> Mesh = default) : FormatDial(FileCodec.X3dv, CodecPhase.Export) {
        internal FileX3dvWriteOptions Mint(CodecTune tune) => new() {
            ExportTextureCoordinates = TextureCoordinates.IfNone(tune.Materials),
            ExportVertexNormals = VertexNormals.IfNone(tune.Fidelity.IsModel),
            ExportVertexColors = VertexColors.IfNone(false),
            MeshingParameters = Mesh.IfNone(() => MeshingParameters.Default),
        };
    }

    public sealed record XamlWriteCase(
        Option<bool> UseExistingRenderMeshes = default,
        Option<bool> AddRotationScrollbars = default,
        Option<bool> UseOriginForRotationCenter = default,
        Option<bool> AddRotationAnimation = default,
        Option<FileXamlWriteOptions.AnimationMode> AnimationAxis = default,
        Option<MeshingParameters> Mesh = default) : FormatDial(FileCodec.Xaml, CodecPhase.Export) {
        internal FileXamlWriteOptions Mint(CodecTune tune) => new() {
            UseExistingRenderMeshes = UseExistingRenderMeshes.IfNone(tune.Fidelity.IsModel),
            AddRotationScrollbars = AddRotationScrollbars.IfNone(false),
            UseOriginForRotationCenter = UseOriginForRotationCenter.IfNone(true),
            AddRotationAnimation = AddRotationAnimation.IfNone(false),
            AnimationAxis = AnimationAxis.IfNone(FileXamlWriteOptions.AnimationMode.X),
            MeshingParameters = Mesh.IfNone(() => MeshingParameters.Default),
        };
    }

    public sealed record XTWriteCase(Option<FileX_TWriteOptions.X_T_Types> Type = default) : FormatDial(FileCodec.XT, CodecPhase.Export) {
        internal FileX_TWriteOptions Mint() => new() { Type = Type.IfNone(FileX_TWriteOptions.X_T_Types.Default) };
    }

    public sealed record TxtWriteCase(
        Option<FileTxtWriteOptions.DelimiterMode> Delimiter = default,
        Option<char> Custom = default,
        Option<Dimension> Precision = default,
        Option<bool> VertexColors = default,
        Option<bool> Quoted = default) : FormatDial(FileCodec.Txt, CodecPhase.Export) {
        internal FileTxtWriteOptions Mint(CodecTune tune) => new() {
            Delimiter = Delimiter.IfNone(FileTxtWriteOptions.DelimiterMode.Comma),
            DelimiterCharacter = Custom.IfNone(','),
            Precision = Precision.Map(static value => value.Value).IfNone(16),
            ExportVertexColors = VertexColors.IfNone(true),
            SurroundWithDoubleQuotes = Quoted.IfNone(tune.Group != CodecAxis.Document),
        };
    }

    public sealed record TxtReadCase(
        Option<FileTxtReadOptions.DelimiterMode> Delimiter = default,
        Option<char> Custom = default,
        Option<bool> CreatePointCloud = default) : FormatDial(FileCodec.Txt, CodecPhase.Import) {
        internal FileTxtReadOptions Mint() => new() {
            Delimiter = Delimiter.IfNone(FileTxtReadOptions.DelimiterMode.Automatic),
            DelimiterCharacter = Custom.IfNone(','),
            CreatePointCloud = CreatePointCloud.IfNone(true),
        };
    }

    public sealed record CsvWriteCase(
        Option<Seq<CsvColumn>> Columns = default,
        Option<bool> QuotedPoints = default) : FormatDial(FileCodec.Csv, CodecPhase.Export) {
        internal FileCsvWriteOptions Mint(CodecTune tune) {
            Seq<CsvColumn> members = Columns.IfNone(() => CsvColumn.Baseline(tune: tune));
            FileCsvWriteOptions host = new() { SurroundPointsWithDoubleQuotes = QuotedPoints.IfNone(true) };
            _ = toSeq(CsvColumn.Items).Iter(row => row.Write(options: host, member: members.Exists(member => member == row)));
            return host;
        }
    }

    public sealed record GltfWriteCase(
        Option<bool> MapZtoY = default,
        Option<bool> Materials = default,
        Option<bool> CullBackfaces = default,
        Option<bool> DisplayColorForUnsetMaterials = default,
        Option<SubDForm> SubD = default,
        Option<Dimension> SubDDensity = default,
        Option<bool> TextureCoordinates = default,
        Option<bool> VertexNormals = default,
        Option<bool> OpenMeshes = default,
        Option<bool> VertexColors = default,
        Option<bool> Layers = default,
        Option<FieldOverride<DracoDial>> Draco = default) : FormatDial(FileCodec.Gltf, CodecPhase.Export) {
        internal FileGltfWriteOptions Mint(CodecTune tune) {
            (bool Use, Option<DracoDial> Value) baseline = (
                Use: tune.Fidelity == CodecFidelity.Small,
                Value: Option<DracoDial>.None);
            (bool Use, Option<DracoDial> Value) draco = Draco
                .Map(field => field.Switch(
                    baseline,
                    keepCase: static (state, _) => state,
                    setCase: static (_, setting) => (Use: true, Value: Some(setting.Value)),
                    clearCase: static (_, _) => (Use: false, Value: Option<DracoDial>.None)))
                .IfNone(baseline);
            return new() {
                MapZToY = MapZtoY.IfNone(true),
                ExportMaterials = Materials.IfNone(tune.Materials),
                CullBackfaces = CullBackfaces.IfNone(true),
                UseDisplayColorForUnsetMaterials = DisplayColorForUnsetMaterials.IfNone(true),
                SubDMeshType = SubD.Map(static row => row.Gltf).IfNone(FileGltfWriteOptions.SubDMeshing.Surface),
                SubDSurfaceMeshingDensity = SubDDensity.Map(static value => value.Value).IfNone(4),
                ExportTextureCoordinates = TextureCoordinates.IfNone(tune.Materials),
                ExportVertexNormals = VertexNormals.IfNone(tune.Fidelity.Measured),
                ExportOpenMeshes = OpenMeshes.IfNone(true),
                ExportVertexColors = VertexColors.IfNone(false),
                ExportLayers = Layers.IfNone(tune.Group == CodecAxis.Layer),
                UseDracoCompression = draco.Use,
                DracoCompressionLevel = draco.Value.Map(static dial => dial.Level.Value).IfNone(int.Max(tune.Fidelity.Draco.Compression, 1)),
                DracoQuantizationBitsPosition = draco.Value.Map(static dial => dial.PositionBits.Value).IfNone(tune.Fidelity.Draco.BitsPos),
                DracoQuantizationBitsNormal = draco.Value.Map(static dial => dial.NormalBits.Value).IfNone(tune.Fidelity.Draco.BitsNormal),
                DracoQuantizationBitsTextureCoordinate = draco.Value.Map(static dial => dial.TextureBits.Value).IfNone(tune.Fidelity.Draco.BitsTexCoord),
            };
        }
    }

    public sealed record UsdWriteCase(
        Option<USDExportBlockHandling> BlockHandling = default,
        Option<string> DefaultLayer = default,
        Option<string> ModelName = default,
        Option<bool> ForceMeshes = default,
        Option<bool> IncludeUserStrings = default,
        Option<MeshingParameters> Mesh = default) : FormatDial(FileCodec.Usd, CodecPhase.Export) {
        internal FileUsdWriteOptions Mint(CodecTune tune) {
            FileUsdWriteOptions host = new() {
                BlockHandling = BlockHandling.IfNone(tune.Resources.Switch(
                    reference: static () => USDExportBlockHandling.Ignore,
                    embed: static () => USDExportBlockHandling.Embedded,
                    copy: static () => USDExportBlockHandling.SeparateFiles)),
                DefaultLayer = DefaultLayer.IfNone(tune.Group == CodecAxis.Layer ? "Layers" : "World"),
                ModelName = ModelName.IfNone(tune.Group == CodecAxis.Document ? string.Empty : "Model"),
                ForceMeshes = ForceMeshes.IfNone(!tune.Fidelity.IsModel),
                IncludeUserStrings = IncludeUserStrings.IfNone(tune.Group == CodecAxis.UserString),
            };
            _ = Mesh.Iter(value => host.MeshingParameters = value);
            return host;
        }
    }

    public sealed record PdfReadCase(
        Option<bool> FillsAsHatches = default,
        Option<bool> LoadText = default) : FormatDial(FileCodec.Pdf, CodecPhase.Import) {
        internal FilePdfReadOptions Mint(CodecTune tune) => new() {
            PreserveModelScale = tune.Fidelity.IsModel,
            ImportFillsAsHatches = FillsAsHatches.IfNone(tune.Fidelity.Measured),
            LoadText = LoadText.IfNone(tune.Fidelity.IsModel || tune.Group == CodecAxis.UserString),
        };
    }

    public sealed record SvgReadCase(
        Option<bool> RetainGrouping = default,
        Option<bool> GroupMultiCurvePaths = default,
        Option<FileSvgReadOptions.ImportFillMode> Fills = default) : FormatDial(FileCodec.Svg, CodecPhase.Import) {
        internal FileSvgReadOptions Mint() => new() {
            RetainGrouping = RetainGrouping.IfNone(false),
            GroupMultiCurvePaths = GroupMultiCurvePaths.IfNone(false),
            ImportFilledObjectAs = Fills.IfNone(FileSvgReadOptions.ImportFillMode.AsCurves),
        };
    }
}
```

## [04]-[DIAL_BINDING]

- Owner: `Dials.Resolve` extracts one requested case or constructs its baseline, then threads one caller state through the supplied case projection. `Dials.Scale` composes vector scale after option minting.
- Law: `Codecs.Apply` refuses a `Some` dial whose `DialSeat` differs from the dispatched codec and request phase before host or filesystem contact. `None` selects the supplied baseline case.
- Law: `AiRead` and `EpsRead` are lens-only rows — their host surfaces carry nothing beyond the scale axes, so they mint directly with the `PreserveModelScale` fidelity baseline and no case exists to misconfigure.
- Boundary: `Dials` returns bare host option objects only into the codec engine columns — the one internal seam already holding the raw `RhinoDoc` — and nothing above the matrix ever sees a host options type.

```csharp signature
// --- [OPERATIONS] ---------------------------------------------------------------------------
internal static class Dials {
    internal static TOptions Resolve<TCase, TState, TOptions>(
        CodecTune tune,
        TState state,
        Func<TCase> baseline,
        Func<TCase, CodecTune, TState, TOptions> mint) where TCase : FormatDial =>
        tune.Dial
            .Bind(static dial => dial is TCase match ? Some(match) : Option<TCase>.None)
            .Map(dial => mint(dial, tune, state))
            .IfNone(() => mint(baseline(), tune, state));

    internal static TOptions Scale<TOptions>(TOptions options, CodecTune tune, VectorLens<TOptions> lens) where TOptions : class =>
        tune.Scale.Map(scale => scale.Apply(options: options, lens: lens)).IfNone(options);
}
```

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    lineColor: "#FF79C6"
    textColor: "#F8F8F2"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
    accTitle: Dial resolution and host option minting
    accDescr: A tune with its optional dial slot passes the seat gate, resolves to the matching or default dial case, mints the typed host options over the tune baseline, and composes the vector scale lens before the engine column consumes the result.
    Tune["CodecTune — presets · Option dial slot"] --> Gate{{"Codecs.Apply — DialSeat gate"}}
    Gate --> Resolve["Dials.Resolve — matching case or baseline"]
    Resolve --> Mint["case Mint — tune baseline · explicit overrides · shared axes"]
    Scale["VectorScale lens"] --> Mint
    Mint --> Host["typed host options → engine column"]
    linkStyle 4 stroke:#50FA7B,color:#F8F8F2
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef success fill:#50FA7BBF,stroke:#50FA7B,color:#282A36
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    class Gate,Resolve,Mint primary
    class Host success
    class Tune,Scale data
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
