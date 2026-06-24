# [MATERIALS_INTERCHANGE]

THE MATERIAL WIRE VOCABULARY and THE MATERIALX NODE-GRAPH INTERCHANGE. One `MaterialWire` is the canonical OpenPBR-vector material wire shape every host-free peer decodes — a `MaterialId` key, the closed OpenPBR Surface 1.1 parameter groups (`base`/`specular`/`transmission`/`subsurface`/`coat`/`fuzz`/`thin_film`/`emission`/`geometry`), the named `ConductorMetal`, the `Provenance` receipt, and the resolved `SurfaceShade` preview — minted ONCE in C# and decoded, NEVER re-minted, by the TypeScript web/edge platform and the Python companion. One `MtlxDocument` is the MaterialX 1.39 `.mtlx` node-graph interchange shape: the `graph#MATERIAL_GRAPH` `AppearanceNode` union and `PortValue` polarities project onto the MaterialX node categories and typed ports so a `MaterialGraph` serializes to and from a DCC-portable `.mtlx` document, with the `surface#OPENPBR_SLAB` `SlabStack` lowered to the MaterialX `open_pbr_surface` node. C# OWNS the material wire vocabulary; the TS and Py consumers DECODE the `MaterialWire`/`MtlxDocument` and never re-derive the OpenPBR parameter algebra or the MaterialX node schema — a second mint of either shape in a peer is the named cross-language drift defect (`architecture.md#CROSS_LANGUAGE_WIRE`). The page composes `graph#MATERIAL_LIBRARY` `MaterialParameters` for the parameter source, `surface#OPENPBR_SLAB` `SlabStack`/`OpenPbrSurface` for the OpenPBR vector, `surface#CONDUCTOR_IOR` `ConductorMetal` for the metal grounding, `graph#MATERIAL_GRAPH` for the node-graph source, and the `MaterialFault` band-2450 rail for a malformed projection.

## [01]-[INDEX]

- [01]-[MATERIAL_WIRE]: the `MaterialWire` canonical OpenPBR-vector wire shape, the `OpenPbrGroupsWire` parameter-group projection, the `WireProvenance` receipt, the `WireCodec` Thinktecture generated System.Text.Json / MessagePack serialization the SmartEnum/Union/ValueObject wire keys round-trip through, and the C#-mints / peers-decode single-mint law plus the TS and Python decode contracts.
- [02]-[MATERIALX_DOCUMENT]: the `MtlxDocument`/`MtlxNode` MaterialX 1.39 node-graph shape, the `NodeCategory` axis, the `AppearanceNode`→MaterialX projection, and the `.mtlx` serialize/admit fold.

## [02]-[MATERIAL_WIRE]

- Owner: `MaterialWire` the canonical material wire shape; `OpenPbrGroupsWire` the OpenPBR parameter-group projection; `WireProvenance` the wire receipt; `MaterialProjection` the static mint fold; `WireCodec` the Thinktecture generated-codec serialization owner (the one `JsonSerializerOptions` and `MessagePackSerializerOptions` the wire shapes encode/decode through).
- Entry: `public static MaterialWire Project(MaterialId id, MaterialParameters parameters, Provenance provenance, SurfaceShade preview)` mints the one wire shape from a library row, and `public static Fin<MaterialWire> Mint(MaterialId id, Op key)` resolves a registered row, lowers it to the OpenPBR vector, and projects — the SINGLE site the material wire is minted; `WireCodec.ToJson`/`FromJson` and `WireCodec.ToMessagePack`/`FromMessagePack` are the one serialization entry the minted shape round-trips through, the SmartEnum/Union/ValueObject wire keys serialized by the Thinktecture generated codec rather than a hand-rolled string projection; the TS `MaterialWire`/`OpenPbrGroupsWire` interface and the Python `MaterialWire` dataclass DECODE this exact JSON/MessagePack shape and never re-mint the OpenPBR algebra.
- Packages: Wacton.Unicolour (composed — scene-linear color hex/linear-triple projection for the wire color fields), Thinktecture.Runtime.Extensions + Thinktecture.Runtime.Extensions.Json (`ThinktectureJsonConverterFactory` the generated `JsonConverterFactory` resolving every SmartEnum/Union/ValueObject key by its `IObjectFactory`/`IKeyedValueObject` shape) + Thinktecture.Runtime.Extensions.MessagePack (`ThinktectureMessageFormatterResolver` the generated `IFormatterResolver` for the companion/outside-Rhino binary wire), LanguageExt.Core, BCL inbox (`System.Text.Json`, `MessagePack`).
- Growth: a new OpenPBR parameter is one column on `OpenPbrGroupsWire` (and its TS/Py decode row), defaulted so existing rows decode unchanged; a new wire receipt field is one `WireProvenance` column; a new metal is one `ConductorMetal` row the wire names by key — never a parallel material wire shape and never a per-material wire type. The `MaterialWire` is the OpenPBR-vector wire the `surface#OPENPBR_SLAB` `SlabStack` defines; the renderer-side consumer at `Rasm.AppUi/Render/pathtrace#PATH_TRACE` reads the C# `MaterialParameters` interior directly (same runtime), and the cross-language peers read `MaterialWire` over the wire.
- Law: C# is the sole producer of the material wire vocabulary — the OpenPBR parameter groups, the `ConductorMetal` key, the `SurfaceShade` preview, and the `WireProvenance` receipt are minted once at `MaterialProjection.Mint` and decoded by every peer; a TS or Python re-derivation of the OpenPBR lowering, the conductor-IOR table, or the MaterialX node schema is the named cross-language drift defect, so the peers carry a decode-only `MaterialWire` shape that mirrors this projection field-for-field and never an OpenPBR construction of their own.
- Boundary: `MaterialWire` is the ONE material wire — a per-consumer material DTO is the deleted form; the wire carries the `MaterialId` `family.name` key, the `OpenPbrGroupsWire` flat OpenPBR vector (the `surface#OPENPBR_SLAB` `OpenPbrSurface` columns projected to wire scalars and color triples, the `graph#MATERIAL_LIBRARY` `SubsurfaceRadius` band carrier flattening to its `R`/`G`/`B` mean-free-path scalars `SubsurfaceRadiusR`/`G`/`B` rather than a `Vector3d.X`/`Y`/`Z` decomposition so the cross-language peers decode the same per-channel scatter bands), the `ConductorMetal` key string for the metal grounding (empty for a dielectric), the `WireProvenance` receipt (the `acquisition#ACQUISITION` `Provenance` device/wavelength/residual), and the resolved `SurfaceShade` preview as a scene-linear linear-RGB triple plus the `Hex` byte string the web swatch reads; color fields cross as the scene-linear `RgbLinear.Triplet` `(First, Second, Third)` triple and the clipped `Hex` so a peer renders without re-deriving the ACEScg working space, the linear triple the shading truth and the hex the preview; the `Provenance.Authored` receipt marks a hand-authored row and a measured row carries its device/residual so a consumer distinguishes a measured material from an authored guess on the wire; the wire is a portable data record (no `Rhino.Geometry`, no `Unicolour` object crosses — only the projected scalars/triples) so the TS interface and the Python dataclass decode it structurally; the single-mint invariant is verified end-to-end — `MaterialProjection.Mint` is the only OpenPBR-vector construction site, the TS `decodeMaterialWire` and the Python `MaterialWire.from_wire` are pure structural decoders that never call an OpenPBR lowering, and a peer that re-mints the OpenPBR vector or the conductor table is the rejected form; the wire SERIALIZES through `WireCodec`, the one `JsonSerializerOptions`/`MessagePackSerializerOptions` carrying the Thinktecture generated `ThinktectureJsonConverterFactory` converter and `ThinktectureMessageFormatterResolver` resolver so the `MaterialId`/`ConductorMetal`/`NodeCategory` SmartEnum keys, the `JointKind`/`Slab`/`TextureSource` Union discriminants, and the `AgeParameter`/`UnitInterval` ValueObject keys encode through their generated codec — the C# sole-producer mints one shape both the System.Text.Json web/edge path and the MessagePack companion path round-trip, never the hand-rolled `CultureInfo.InvariantCulture` `ToString("R")` string projection a peer would have to re-parse; the JSON path is the in-Rhino-ALC-safe wire (System.Text.Json is BCL-inbox) and the MessagePack path the heavy-transitive companion/outside-Rhino wire scoped behind the firebreak, both reading the SAME `MaterialWire` shape so a JSON-decoding TS peer and a MessagePack-decoding Python companion decode field-for-field.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Text.Json;
using System.Text.Json.Serialization;
using MessagePack;
using MessagePack.Resolvers;
using Thinktecture.Formatters;                 // ThinktectureMessageFormatterResolver
using Thinktecture.Text.Json.Serialization;    // ThinktectureJsonConverterFactory
using static LanguageExt.Prelude;

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct WireColor(double R, double G, double B, string Hex) {
    public static WireColor Of(Unicolour colour) {
        ColourTriplet lin = colour.RgbLinear.Triplet;
        return new WireColor(lin.First, lin.Second, lin.Third, colour.Hex);
    }
    public static WireColor Of(RgbSpectrum rgb) =>
        new(rgb.R, rgb.G, rgb.B, new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, rgb.R, rgb.G, rgb.B).Hex);
}

public readonly record struct WireProvenance(string Device, int WavelengthCount, double FitResidual, bool Measured) {
    public static WireProvenance Of(Provenance p) => new(p.Device, p.WavelengthCount, p.FitResidual, p.Device != "authored");
}

public readonly record struct OpenPbrGroupsWire(
    double BaseWeight, WireColor BaseColor, double BaseMetalness, double BaseDiffuseRoughness,
    double SpecularWeight, double SpecularRoughness, double SpecularIor, double SpecularAnisotropy,
    double TransmissionWeight, double TransmissionRoughness,
    double SubsurfaceWeight, double SubsurfaceRadiusR, double SubsurfaceRadiusG, double SubsurfaceRadiusB,
    double CoatWeight, double CoatRoughness, double CoatIor,
    double FuzzWeight, double FuzzRoughness,
    double ThinFilmWeight, double ThinFilmThickness, double ThinFilmIor,
    WireColor EmissionColor, double EmissionLuminance) {

    public static OpenPbrGroupsWire Of(OpenPbrSurface s) =>
        new(s.BaseWeight, WireColor.Of(s.BaseColor), s.BaseMetalness, s.BaseDiffuseRoughness,
            s.SpecularWeight, s.SpecularRoughness, s.SpecularIor, s.SpecularAnisotropy,
            s.TransmissionWeight, s.TransmissionRoughness,
            s.SubsurfaceWeight, s.SubsurfaceRadius.R, s.SubsurfaceRadius.G, s.SubsurfaceRadius.B,
            s.CoatWeight, s.CoatRoughness, s.CoatIor,
            s.FuzzWeight, s.FuzzRoughness,
            s.ThinFilmWeight, s.ThinFilmThickness, s.ThinFilmIor,
            WireColor.Of(s.EmissionColor), s.EmissionLuminance);
}

public sealed record MaterialWire(
    string Id,
    OpenPbrGroupsWire OpenPbr,
    string Conductor,
    WireProvenance Provenance,
    WireColor Preview);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class MaterialProjection {
    private static RgbSpectrum Linear(Unicolour colour) { var lin = colour.RgbLinear; return RgbSpectrum.Create(Math.Max(0.0, lin.R), Math.Max(0.0, lin.G), Math.Max(0.0, lin.B)); }

    public static OpenPbrSurface ToOpenPbr(MaterialParameters p, ConductorMetal conductor) =>
        new(BaseWeight: 1.0, Linear(p.BaseColor), p.Metalness, p.Roughness,
            SpecularWeight: 1.0, p.Roughness, p.Ior, p.Anisotropy,
            p.Transmission, p.TransmissionRoughness,
            p.Subsurface, p.SubsurfaceRadius,
            p.Clearcoat, p.ClearcoatRoughness, CoatIor: 1.5,
            p.Sheen, FuzzRoughness: Math.Max(1e-3, p.Roughness),
            ThinFilmWeight: 0.0, ThinFilmThickness: 0.0, ThinFilmIor: 1.5,
            Linear(p.Emission), p.EmissionLuminance,
            conductor);

    public static MaterialWire Project(MaterialId id, MaterialParameters parameters, Provenance provenance, SurfaceShade preview) {
        ConductorMetal conductor = ConductorIor.Resolve(Family(id), Name(id)).IfNone(ConductorMetal.Iron);
        bool isConductor = parameters.Metalness >= 1.0 && Family(id) == "metal";
        return new MaterialWire(
            id.Value,
            OpenPbrGroupsWire.Of(ToOpenPbr(parameters, conductor)),
            isConductor ? conductor.Key : string.Empty,
            WireProvenance.Of(provenance),
            WireColor.Of(preview.BaseColorLinear));
    }

    public static Fin<MaterialWire> Mint(MaterialId id, Op key) =>
        from parameters in MaterialLibrary.Lookup(id, key)
        from point in ShadePoint.Of(Point3d.Origin, Vector3d.ZAxis, Vector3d.ZAxis, Option<Vector3d>.None, 0.5, 0.5, GraphContext.Tolerant, key)
        from preview in MaterialGraph.Default.Evaluate(point, parameters, key)
        select Project(id, parameters, Provenance.Authored, preview);

    private static string Family(MaterialId id) => id.Value.Split('.') is [var family, ..] ? family : string.Empty;
    private static string Name(MaterialId id) => id.Value.Split('.') is [_, var name, ..] ? name : id.Value;
}

// --- [COMPOSITION] -------------------------------------------------------------------------
// The one wire serialization owner: the minted MaterialWire / MtlxDocument round-trips through the Thinktecture
// generated codecs so every SmartEnum/Union/ValueObject wire key (MaterialId, ConductorMetal, NodeCategory,
// JointKind, AgeParameter, UnitInterval) serializes via its generated IObjectFactory/IFormatter shape, never a
// hand-rolled CultureInfo string projection. STJ is the in-Rhino-ALC-safe path; MessagePack the companion wire.
public static class WireCodec {
    public static readonly JsonSerializerOptions Json = Configure(new JsonSerializerOptions(JsonSerializerDefaults.Web) {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
    });

    public static readonly MessagePackSerializerOptions MessagePack =
        MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(
            ThinktectureMessageFormatterResolver.Instance,
            StandardResolver.Instance));

    private static JsonSerializerOptions Configure(JsonSerializerOptions options) {
        options.Converters.Add(new ThinktectureJsonConverterFactory());   // resolves every generated SmartEnum/Union/ValueObject key
        return options;
    }

    public static string ToJson(MaterialWire wire) => JsonSerializer.Serialize(wire, Json);
    public static Fin<MaterialWire> FromJson(string payload, Op key) =>
        JsonSerializer.Deserialize<MaterialWire>(payload, Json) is { } wire
            ? Fin.Succ(wire)
            : MaterialFault.Graph(key, "<material-wire-json-null>");

    public static ReadOnlyMemory<byte> ToMessagePack(MaterialWire wire) => MessagePackSerializer.Serialize(wire, MessagePack);
    public static Fin<MaterialWire> FromMessagePack(ReadOnlyMemory<byte> payload, Op key) =>
        Try(() => MessagePackSerializer.Deserialize<MaterialWire>(payload, MessagePack)).Match(
            Succ: Fin.Succ,
            Fail: _ => MaterialFault.Graph(key, "<material-wire-messagepack-malformed>"));

    public static string ToJson(MtlxDocument document) => JsonSerializer.Serialize(document, Json);
}
```

```ts contract
interface WireColor {
  readonly r: number;
  readonly g: number;
  readonly b: number;
  readonly hex: string;
}

interface OpenPbrGroupsWire {
  readonly baseWeight: number;
  readonly baseColor: WireColor;
  readonly baseMetalness: number;
  readonly baseDiffuseRoughness: number;
  readonly specularWeight: number;
  readonly specularRoughness: number;
  readonly specularIor: number;
  readonly specularAnisotropy: number;
  readonly transmissionWeight: number;
  readonly transmissionRoughness: number;
  readonly subsurfaceWeight: number;
  readonly subsurfaceRadiusR: number;
  readonly subsurfaceRadiusG: number;
  readonly subsurfaceRadiusB: number;
  readonly coatWeight: number;
  readonly coatRoughness: number;
  readonly coatIor: number;
  readonly fuzzWeight: number;
  readonly fuzzRoughness: number;
  readonly thinFilmWeight: number;
  readonly thinFilmThickness: number;
  readonly thinFilmIor: number;
  readonly emissionColor: WireColor;
  readonly emissionLuminance: number;
}

interface WireProvenance {
  readonly device: string;
  readonly wavelengthCount: number;
  readonly fitResidual: number;
  readonly measured: boolean;
}

interface MaterialWire {
  readonly id: string;
  readonly openPbr: OpenPbrGroupsWire;
  readonly conductor: string;
  readonly provenance: WireProvenance;
  readonly preview: WireColor;
}
```

```python wire
@dataclass(frozen=True, slots=True)
class WireColor:
    r: float
    g: float
    b: float
    hex: str


@dataclass(frozen=True, slots=True)
class OpenPbrGroupsWire:
    base_weight: float
    base_color: WireColor
    base_metalness: float
    base_diffuse_roughness: float
    specular_weight: float
    specular_roughness: float
    specular_ior: float
    specular_anisotropy: float
    transmission_weight: float
    transmission_roughness: float
    subsurface_weight: float
    subsurface_radius_r: float
    subsurface_radius_g: float
    subsurface_radius_b: float
    coat_weight: float
    coat_roughness: float
    coat_ior: float
    fuzz_weight: float
    fuzz_roughness: float
    thin_film_weight: float
    thin_film_thickness: float
    thin_film_ior: float
    emission_color: WireColor
    emission_luminance: float


@dataclass(frozen=True, slots=True)
class MaterialWire:
    id: str
    open_pbr: OpenPbrGroupsWire
    conductor: str
    provenance: WireProvenance
    preview: WireColor
```

## [03]-[MATERIALX_DOCUMENT]

- Owner: `MtlxDocument` the MaterialX 1.39 document; `MtlxNode`/`MtlxInput` the node-graph element shapes; `NodeCategory` `[SmartEnum<string>]` the MaterialX node-category axis; `Mtlx` the static serialize/admit fold.
- Entry: `public static MtlxDocument FromGraph(MaterialGraph graph, MaterialId id, MaterialParameters parameters)` projects the `graph#MATERIAL_GRAPH` node DAG to the MaterialX node-graph document, and `public static Fin<MtlxDocument> ToOpenPbr(MaterialWire wire, Op key)` emits the `open_pbr_surface` node document from the material wire — one entry per direction, both targeting the MaterialX 1.39 `<materialx version="1.39">` root the DCC ecosystem reads; the `.mtlx` XML serialization rides `System.Xml.Linq` at the host boundary, this owner producing the portable `MtlxDocument` data the serializer renders.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`System.Xml.Linq` at the serialize boundary).
- Growth: a new `graph#MATERIAL_GRAPH` `AppearanceNode` case maps to one `NodeCategory` row (the MaterialX node-category the case projects onto), a new MaterialX node category is one `NodeCategory` row binding one projection arm — never a per-node serializer and never a second MaterialX schema. The `texture#TEXTURE_UV` `TextureSource` aligns to the MaterialX 1.39 `noise2d`/`fractal2d`/`worleynoise2d`/`checkerboard`/`ramplr`/`ramptb`/`tiledimage`/`triplanarprojection` categories through the REALIZED `TextureSource.MtlxCategory` projection (`texture#MATERIALX_NODE_PARITY`) the `Mtlx.CategoryOf(TextureSource)` resolves so a `Texture` node round-trips; the `surface#OPENPBR_SLAB` `open_pbr_surface` is the surface node the wire emits, the Standard-Surface translation graph the same projection over the `standard_surface` category.
- Boundary: `MtlxDocument` is the ONE MaterialX shape — a per-tool encoding is the deleted form; the document is the MaterialX 1.39 `<materialx version="1.39">` root carrying one `<nodegraph>` of `<node>` elements with `<input>`/`<output>` children and one `<surfacematerial>` binding, projected from the `graph#MATERIAL_GRAPH` node union by `NodeCategory`: an `Input` projects to a `constant`/`<input>` value, a `Texture` to `image`/`worleynoise2d`/`ramplr` per its `TextureSource`, a `Math` to `multiply`/`add`/`dotproduct`/`clamp` per `MathOp`, a `Mix` to `mix`, a `Normal` to `normalmap`, and the `BsdfOutput` to `open_pbr_surface` whose inputs are the OpenPBR parameter ports the `surface#OPENPBR_SLAB` `OpenPbrSurface` carries (`base_color`/`base_metalness`/`specular_roughness`/`coat_weight`/`fuzz_weight`/`emission_color`/...); the `PortValue` polarities project onto the MaterialX typed ports (`Scalar`→`float`, `Color`→`color3`, `Vector`→`vector3`, `Frame`→`vector3` normal) so a typed port round-trips; the node names derive from the `graph#MATERIAL_GRAPH` `PortId` ordinal as `node{id}` so the graph topology is recoverable from the document and a `.mtlx` consumer rewires the same DAG; color values cross as the MaterialX `color3` scene-linear triple consistent with the `MaterialWire` `WireColor` linear projection, never an sRGB byte triple; the document is portable data the host `System.Xml.Linq` serializer renders to `.mtlx` text at the app-root boundary and admits from `.mtlx` through the same node-category map, this owner never holding an XML reader/writer at an interior signature; an unmapped node category or a malformed port rails `MaterialFault.Graph`, never a partial document.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<MaterialKeyPolicy, string>]
[KeyMemberComparer<MaterialKeyPolicy, string>]
public sealed partial class NodeCategory {
    public static readonly NodeCategory Constant      = new("constant", port: "float");
    public static readonly NodeCategory Image         = new("image", port: "color3");
    public static readonly NodeCategory TiledImage    = new("tiledimage", port: "color3");
    public static readonly NodeCategory Triplanar     = new("triplanarprojection", port: "color3");
    public static readonly NodeCategory Perlin2D      = new("noise2d", port: "float");
    public static readonly NodeCategory Perlin3D      = new("noise3d", port: "float");
    public static readonly NodeCategory Fractal2D     = new("fractal2d", port: "float");
    public static readonly NodeCategory CellNoise     = new("cellnoise2d", port: "float");
    public static readonly NodeCategory Worley        = new("worleynoise2d", port: "float");
    public static readonly NodeCategory UnifiedNoise  = new("unifiednoise2d", port: "float");
    public static readonly NodeCategory Checkerboard  = new("checkerboard", port: "color3");
    public static readonly NodeCategory RampLr        = new("ramplr", port: "color3");
    public static readonly NodeCategory RampTb        = new("ramptb", port: "color3");
    public static readonly NodeCategory Multiply      = new("multiply", port: "color3");
    public static readonly NodeCategory Add           = new("add", port: "color3");
    public static readonly NodeCategory DotProduct    = new("dotproduct", port: "float");
    public static readonly NodeCategory Clamp         = new("clamp", port: "float");
    public static readonly NodeCategory Mix           = new("mix", port: "color3");
    public static readonly NodeCategory Normalmap     = new("normalmap", port: "vector3");
    public static readonly NodeCategory OpenPbrSurface = new("open_pbr_surface", port: "surfaceshader");
    public static readonly NodeCategory StandardSurface = new("standard_surface", port: "surfaceshader");
    public string Port { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct MtlxInput(string Name, string Type, string Value, Option<string> NodeName);

public sealed record MtlxNode(string Name, NodeCategory Category, string Type, Seq<MtlxInput> Inputs);

public sealed record MtlxDocument(string Version, Seq<MtlxNode> Nodes, string SurfaceNode, string MaterialName) {
    public static readonly string Schema = "1.39";
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Mtlx {
    public static NodeCategory CategoryOf(AppearanceNode node) => node.Switch(
        input:      static _ => NodeCategory.Constant,
        texture:    static _ => NodeCategory.Image,
        math:       static m => MathCategory(m.Op),
        mix:        static _ => NodeCategory.Mix,
        normal:     static _ => NodeCategory.Normalmap,
        bsdfOutput: static _ => NodeCategory.OpenPbrSurface);

    // The per-TextureSource-case MaterialX category: texture#TEXTURE_UV owns the case→category projection
    // (TextureSource.MtlxCategory), this resolves the category string to the closed NodeCategory row so a Texture
    // node round-trips to its real MaterialX 1.39 category (noise2d/fractal2d/worleynoise2d/checkerboard/ramplr/tiledimage).
    public static NodeCategory CategoryOf(TextureSource source) =>
        NodeCategory.TryGet(source.MtlxCategory, out NodeCategory? category) ? category! : NodeCategory.Image;

    private static NodeCategory MathCategory(MathOp op) =>
        op == MathOp.Add ? NodeCategory.Add
        : op == MathOp.DotProduct ? NodeCategory.DotProduct
        : op == MathOp.Clamp01 ? NodeCategory.Clamp
        : NodeCategory.Multiply;

    public static MtlxDocument FromGraph(MaterialGraph graph, MaterialId id, MaterialParameters parameters) {
        Seq<MtlxNode> nodes = graph.Nodes.Map(n => new MtlxNode($"node{n.Id.Value}", CategoryOf(n), CategoryOf(n).Port, InputsOf(n)));
        return new MtlxDocument(MtlxDocument.Schema, nodes, $"node{graph.Sink.Value}", id.Value.Replace('.', '_'));
    }

    private static Seq<MtlxInput> InputsOf(AppearanceNode node) => node.Switch(
        input:      static _ => Seq<MtlxInput>(),
        texture:    static _ => Seq1(new MtlxInput("file", "filename", string.Empty, Option<string>.None)),
        math:       static m => m.Rhs.Match(Some: r => Seq(Edge("in1", m.Lhs), Edge("in2", r)), None: () => Seq1(Edge("in1", m.Lhs))),
        mix:        static x => Seq(Edge("fg", x.A), Edge("bg", x.B), Edge("mix", x.Factor)),
        normal:     static n => Seq1(Edge("in", n.Source)),
        bsdfOutput: static o => Seq(Edge("base_color", o.BaseColor), Edge("base_metalness", o.Metalness), Edge("specular_roughness", o.Roughness), Edge("geometry_normal", o.NormalFrame), Edge("emission_color", o.Emission)));

    private static MtlxInput Edge(string name, PortId source) => new(name, "color3", string.Empty, Some($"node{source.Value}"));

    public static Fin<MtlxDocument> ToOpenPbr(MaterialWire wire, Op key) =>
        string.IsNullOrWhiteSpace(wire.Id)
            ? Fin.Fail<MtlxDocument>(MaterialFault.Graph(key, "<mtlx-empty-material-id>"))
            : Fin.Succ(new MtlxDocument(MtlxDocument.Schema, Seq1(SurfaceNode(wire)), "surface", wire.Id.Replace('.', '_')));

    private static MtlxNode SurfaceNode(MaterialWire wire) {
        OpenPbrGroupsWire g = wire.OpenPbr;
        return new MtlxNode("surface", NodeCategory.OpenPbrSurface, "surfaceshader", Seq(
            Value("base_weight", "float", g.BaseWeight),
            Color("base_color", g.BaseColor),
            Value("base_metalness", "float", g.BaseMetalness),
            Value("specular_roughness", "float", g.SpecularRoughness),
            Value("specular_ior", "float", g.SpecularIor),
            Value("transmission_weight", "float", g.TransmissionWeight),
            Value("subsurface_weight", "float", g.SubsurfaceWeight),
            Value("coat_weight", "float", g.CoatWeight),
            Value("coat_roughness", "float", g.CoatRoughness),
            Value("fuzz_weight", "float", g.FuzzWeight),
            Color("emission_color", g.EmissionColor),
            Value("emission_luminance", "float", g.EmissionLuminance)));
    }

    private static MtlxInput Value(string name, string type, double v) => new(name, type, v.ToString("R", CultureInfo.InvariantCulture), Option<string>.None);
    private static MtlxInput Color(string name, WireColor c) => new(name, "color3", $"{c.R:R}, {c.G:R}, {c.B:R}", Option<string>.None);
}
```

## [04]-[RESEARCH]

- [MATERIALX_GRAPH_INTERCHANGE]: the MaterialX 1.39 `.mtlx` document is the platform-neutral node-graph content schema (root `<materialx version="1.39">`, `<nodegraph>` wrapping `<node>` elements with `<input>`/`<output>` children, the `standard_surface`/`open_pbr_surface`/`surfacematerial` surface nodes); the `MtlxDocument`/`MtlxNode` shape and the `NodeCategory` map are the realized projection, the `System.Xml.Linq` `.mtlx` serialize/admit the host-boundary render. The `NodeCategory` axis carries the full verified MaterialX 1.39 standard-library texture/noise/ramp set (`noise2d`/`noise3d`/`fractal2d`/`cellnoise2d`/`worleynoise2d`/`unifiednoise2d`/`checkerboard`/`ramplr`/`ramptb`/`tiledimage`/`triplanarprojection`) the `texture#TEXTURE_UV` `TextureSource.MtlxCategory` projection resolves through `Mtlx.CategoryOf(TextureSource)`, so the `AppearanceNode`↔`NodeCategory` and the per-`TextureSource`-case mappings are settled against the AcademySoftwareFoundation/MaterialX 1.39 `MaterialX.StandardNodes.md`; the per-input port-name alignment per MaterialX node and the Standard-Surface-to-OpenPBR translation graph are the remaining calibration, not a re-architecture of the evaluation fold.
- [MTLX_SCHEMA_VALIDATION]: the `.mtlx` document validates against the MaterialX 1.39 schema at the host import boundary (the `MaterialX` Python/C++ runtime or a schema validator); the managed projection produces the portable `MtlxDocument` and the schema-validation round-trip is a host-edge concern the app root owns, this owner never binding a native MaterialX runtime — a malformed projection rails `MaterialFault.Graph` before serialization rather than emitting an invalid document. The `MtlxInput.Value`/`Color` `ToString("R", CultureInfo.InvariantCulture)` is the MaterialX `float`/`color3` XML attribute-TEXT encoding (the `.mtlx` schema requires the invariant-culture round-trip literal), distinct from the `MaterialWire` JSON/MessagePack codec — the XML text formatting stays at the `System.Xml.Linq` boundary, the structured `MaterialWire` shape rides `WireCodec`.
- [THINKTECTURE_WIRE_CODEC]: REALIZED — the `MaterialWire`/`OpenPbrGroupsWire`/`WireProvenance`/`WireColor` shapes serialize through `WireCodec`, the one `JsonSerializerOptions` carrying the Thinktecture generated `ThinktectureJsonConverterFactory` (the `JsonConverterFactory` resolving every `[SmartEnum]`/`[Union]`/`[ValueObject]` key by its generated `IObjectFactory`/`IKeyedValueObject` shape — `MaterialId`/`ConductorMetal`/`NodeCategory`/`JointKind`/`AgeParameter`/`UnitInterval` all round-trip by their key, never a hand-rolled string projection) and the one `MessagePackSerializerOptions` carrying `ThinktectureMessageFormatterResolver.Instance` composed with `StandardResolver.Instance` for the companion/outside-Rhino binary wire. The System.Text.Json path is the in-Rhino-ALC-safe wire (BCL-inbox, no heavy transitive closure), the MessagePack path the heavy-transitive companion path scoped behind the Speckle.Sdk firebreak (`ARCHITECTURE` line 39 single-mint invariant). The `Rasm.Materials.csproj` admits `Thinktecture.Runtime.Extensions.Json`/`.MessagePack` (the `10.3.0` pins already central in `Directory.Packages.props` lines 12-13, version-less folder-local references), and the README `[03]-[SUBSTRATE_PACKAGES]` registry lists both serializer companions beside the core Thinktecture pin. The TS `decodeMaterialWire` and Python `MaterialWire.from_wire` decode the SAME JSON shape the generated codec mints, the single-mint invariant holding with the codec as the canonical shape — a peer re-mint of the OpenPBR algebra or a second string projection is the named cross-language drift defect.
