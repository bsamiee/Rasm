# [MATERIALS_INTERCHANGE]

THE MATERIAL WIRE VOCABULARY and THE MATERIALX NODE-GRAPH INTERCHANGE. This page is the appearance EGRESS owner — the one place a resolved `graph#MATERIAL_LIBRARY` `MaterialParameters` row crosses out of `Rasm.Materials` in three reconciled shapes keyed by ONE content hash. `MaterialWire.Summary(MaterialParameters)` lowers a row to the SEAM `Rasm.Element/Graph/element#NODE_MODEL` `AppearanceSummary` (the `UInt128 AppearanceKey` + the neutral PBR scalars the `ComponentProjector` content-keys a `Node.Appearance` on) — the CONTRACTED entry `Projection/component#COMPONENT_SUBGRAPH` `ComponentSubgraph.Capture` composes, the same seam-owned `XxHash128` `AppearanceKey` derivation `Rasm.Bim` `Semantics/appearance#APPEARANCE_PROJECTION` composes so a Materials OpenPBR row and a BIM `IfcSurfaceStyleRendering` style describing one surface DEDUP to one `Node.Appearance` id. `MaterialWire.Project` mints the FULL OpenPBR-vector payload `MaterialWire` BEHIND that key — the `MaterialId` key, the closed OpenPBR Surface 1.1 parameter groups (`base`/`specular`/`transmission`/`subsurface`/`coat`/`fuzz`/`thin_film`/`emission`/`geometry`), the named `ConductorMetal`, the `WireProvenance` receipt, and the resolved `SurfaceShade` preview — the heavy wire a renderer/GLB-material author and the host-free peers decode by the SAME content key, NEVER re-minted by a peer. `MtlxDocument` is the MaterialX 1.39 `.mtlx` node-graph interchange shape: the `graph#MATERIAL_GRAPH` `AppearanceNode` union and `PortValue` polarities project onto the MaterialX node categories and typed ports so a `MaterialGraph` serializes to and from a DCC-portable `.mtlx` document, with the `surface#OPENPBR_SLAB` `OpenPbrSurface` lowered to the MaterialX `open_pbr_surface` node. C# OWNS the appearance wire vocabulary; the seam carries the `AppearanceSummary` key + scalars, the full `MaterialWire`/`MtlxDocument` are the payloads behind that key, and the TS/Py consumers DECODE — a second mint of the OpenPBR parameter algebra or the MaterialX node schema in a peer is the named cross-language drift defect (`architecture.md#CROSS_LANGUAGE_WIRE`). The page composes `graph#MATERIAL_LIBRARY` `MaterialParameters` for the parameter source, `surface#OPENPBR_SLAB` `OpenPbrSurface`/`SlabStack` for the OpenPBR vector, `surface#CONDUCTOR_IOR` `ConductorMetal`/`ConductorIor` for the metal grounding, `graph#MATERIAL_GRAPH` `MaterialGraph` for the node-graph source, the SEAM `Rasm.Element/Composition/material#MATERIAL_COMPOSITION` `MaterialId` identity and `Rasm.Element/Graph/element#NODE_MODEL` `AppearanceSummary` + its seam-owned `AppearanceSummary.Of` content-key factory (which owns the `Rasm.Element/Projection/address#CONTENT_ADDRESS` `XxHash128` hashing this page never re-implements), and the `MaterialFault` band-2450 rail for a malformed projection.

## [01]-[INDEX]

- [01]-[MATERIAL_WIRE]: the `MaterialWire.Summary` seam-`AppearanceSummary` lowering (the CONTRACTED `Projection/component#COMPONENT_SUBGRAPH` entry, content-keyed by the seam `ContentAddress`), the `MaterialWire` canonical OpenPBR-vector payload, the `OpenPbrGroupsWire` parameter-group projection, the `WireProvenance` receipt, the `WireCodec` Thinktecture-generated System.Text.Json / MessagePack `MaterialWire` serialization any SmartEnum/Union/ValueObject wire key round-trips through, and the C#-mints / peers-decode single-mint law plus the TS and Python decode contracts.
- [02]-[MATERIALX_DOCUMENT]: the `MtlxDocument`/`MtlxNode` MaterialX 1.39 node-graph shape, the `NodeCategory` axis carrying the per-category typed port, the `AppearanceNode`→MaterialX projection over the typed `PortValue` polarity, and the `.mtlx` serialize/admit fold.

## [02]-[MATERIAL_WIRE]

- Owner: `MaterialWire` the canonical OpenPBR-vector payload behind the seam appearance content key; `OpenPbrGroupsWire` the OpenPBR Surface 1.1 parameter-group projection; `WireProvenance` the wire receipt; `MaterialWire.Summary`/`Project`/`Mint` the static lowering folds co-located on the record (`Summary` → seam `AppearanceSummary`, `Project` → the full payload, `Mint` → the registered-row mint); `WireCodec` the Thinktecture-generated-codec serialization owner (the one `JsonSerializerOptions` and `MessagePackSerializerOptions` the `MaterialWire` encodes/decodes through).
- Entry: `public static AppearanceSummary Summary(MaterialParameters parameters)` lowers a library row to the SEAM `AppearanceSummary` through the seam-owned `AppearanceSummary.Of` factory — the neutral PBR scalars plus the `AppearanceKey` the factory mints (the kernel seed-zero `XxHash128` over the canonical PBR bytes, the ONE hasher), the CONTRACTED entry `Projection/component#COMPONENT_SUBGRAPH` `ComponentSubgraph.Capture` composes (`MaterialLibrary.Lookup(id, key).Map(MaterialWire.Summary)`) and the SAME seam factory `Rasm.Bim` `Semantics/appearance#APPEARANCE_PROJECTION` composes; `public static MaterialWire Project(MaterialId id, MaterialParameters parameters, Provenance provenance, SurfaceShade preview)` mints the FULL OpenPBR-vector payload behind that key; `public static Fin<MaterialWire> Mint(MaterialId id, Op key)` resolves a registered row, evaluates the default graph for the preview, and projects — the SINGLE site the full payload is minted; `WireCodec.ToJson`/`FromJson` and `WireCodec.ToMessagePack`/`FromMessagePack` are the one serialization entry the minted `MaterialWire` round-trips through, any SmartEnum/Union/ValueObject wire key serialized by the Thinktecture generated codec rather than a hand-rolled string projection, while the `MtlxDocument` (its `Seq`/`Option` members) renders to `.mtlx` text through `System.Xml.Linq` at the host edge, never a structured JSON codec; the TS `MaterialWire`/`OpenPbrGroupsWire`/`AppearanceSummary` interface and the Python `MaterialWire`/`AppearanceSummary` dataclass DECODE this exact JSON/MessagePack shape and never re-mint the OpenPBR algebra.
- Packages: Wacton.Unicolour (composed — scene-linear color hex/linear-triple projection for the wire color fields), Rasm.Element (the SEAM `MaterialId` identity and the `AppearanceSummary` neutral PBR record + its seam-owned `AppearanceSummary.Of` content-key factory the `Summary` lowering composes — the factory owns the `ContentAddress`/`CanonicalWriter` `XxHash128` hashing, so this page mints NO key bytes of its own and never a second hasher), Thinktecture.Runtime.Extensions + Thinktecture.Runtime.Extensions.Json (`ThinktectureJsonConverterFactory` the generated `JsonConverterFactory` resolving every SmartEnum/Union/ValueObject key by its `IObjectFactory`/`IKeyedValueObject` shape) + Thinktecture.Runtime.Extensions.MessagePack (`ThinktectureMessageFormatterResolver` the generated `IFormatterResolver` for the companion/outside-Rhino binary wire), LanguageExt.Core (`Fin`/`Option`/`Seq` for the projection rail; the `MtlxDocument` `Seq`/`Option` shapes project to `System.Xml.Linq` elements at the host edge, never through a structured JSON codec), BCL inbox (`System.Text.Json`, `MessagePack`, `System.Xml.Linq`).
- Growth: a new OpenPBR parameter is one column on `OpenPbrGroupsWire` (and its TS/Py decode row), defaulted so existing rows decode unchanged; a new wire receipt field is one `WireProvenance` column; a new metal is one `ConductorMetal` row the wire names by key — never a parallel material wire shape and never a per-material wire type. The `MaterialWire` is the OpenPBR-vector payload the `surface#OPENPBR_SLAB` `OpenPbrSurface` defines; the renderer-side consumer at `Rasm.AppUi/Render/pathtrace#PATH_TRACE` reads the C# `MaterialParameters` interior directly (same runtime), the seam carries the `AppearanceSummary` key + scalars, and the cross-language peers read the full `MaterialWire` over the wire by the content key.
- Law: C# is the sole producer of the appearance wire vocabulary — the OpenPBR parameter groups, the `ConductorMetal` key, the `SurfaceShade` preview, the `WireProvenance` receipt, and the `AppearanceKey` content hash are minted once on this page and decoded by every peer; a TS or Python re-derivation of the OpenPBR lowering, the conductor-IOR table, or the MaterialX node schema is the named cross-language drift defect, so the peers carry a decode-only `MaterialWire` shape that mirrors this projection field-for-field and never an OpenPBR construction of their own. The `AppearanceKey` is minted by the seam-owned `AppearanceSummary.Of` factory (the kernel seed-zero `XxHash128` over the canonical PBR bytes via the seam `ContentAddress`/`CanonicalWriter`, NOT a second hasher and NOT a non-zero seed), so the `Summary` lowering here and the `Rasm.Bim` `AppearanceProjection.Project` lowering compose the SAME factory and produce the SAME key for one surface — a local `CanonicalWriter` over the PBR vector beside the seam factory would couple Materials and Bim by byte-order convention and is the divergence defect.
- Boundary: `MaterialWire`/`AppearanceSummary` is the ONE appearance wire — a per-consumer material DTO is the deleted form. The seam carries the NEUTRAL `AppearanceSummary` (the `UInt128 AppearanceKey` + scene-linear `BaseColorR`/`G`/`B` + `Metallic` + `Roughness` + `Opacity`) a consumer reads flat without the lobe graph; the full `MaterialWire` is the payload behind that key, carrying the `MaterialId` `family.name` key (the SEAM `Rasm.Element` `[ValueObject<string>]` identity, NEVER a parallel `family.name` re-declaration), the `OpenPbrGroupsWire` flat OpenPBR vector (the `surface#OPENPBR_SLAB` `OpenPbrSurface` columns projected to wire scalars and color triples, the `graph#MATERIAL_LIBRARY` `SubsurfaceRadius` band carrier flattening to its `R`/`G`/`B` mean-free-path scalars `SubsurfaceRadiusR`/`G`/`B` rather than a `Vector3d.X`/`Y`/`Z` decomposition so the cross-language peers decode the same per-channel scatter bands), the `ConductorMetal` key string for the metal grounding (empty for a dielectric), the `WireProvenance` receipt (the `acquisition#ACQUISITION` `Provenance` device/wavelength/residual plus the `CaptureMethod` instrument and angular/texel sample count crossing the wire), and the resolved `SurfaceShade` preview as a scene-linear linear-RGB triple plus the `Hex` byte string the web swatch reads; color fields cross as the scene-linear `RgbLinear.Triplet` `(First, Second, Third)` triple and the clipped `Hex` so a peer renders without re-deriving the ACEScg working space, the linear triple the shading truth and the hex the preview; the `WireProvenance.Measured` flag is DERIVED from the residual-bearing receipt structurally (`p != Provenance.Authored`), never a magic-string device compare, so a measured material is distinguished from an authored guess on the wire without a sentinel literal; the wire is a portable data record (no `Rhino.Geometry`, no `Unicolour` object crosses — only the projected scalars/triples) so the TS interface and the Python dataclass decode it structurally; the single-mint invariant is verified end-to-end — `MaterialWire.Project` is the only OpenPBR-vector construction site, `MaterialWire.Summary` the only `AppearanceKey` derivation, the TS `decodeMaterialWire` and the Python `MaterialWire.from_wire` are pure structural decoders that never call an OpenPBR lowering, and a peer that re-mints the OpenPBR vector or the conductor table is the rejected form; the `MaterialWire` SERIALIZES through `WireCodec`, the one `JsonSerializerOptions`/`MessagePackSerializerOptions` carrying the Thinktecture generated `ThinktectureJsonConverterFactory` converter and `ThinktectureMessageFormatterResolver` resolver so any SmartEnum/Union/ValueObject wire key (a future typed `Conductor`/`NodeCategory` field) encodes through its generated codec — the wire shape itself is plain records (`string`/`double`/`WireColor`, no `Seq`/`Option` field) so it round-trips directly, never the hand-rolled `CultureInfo.InvariantCulture` `ToString("R")` string projection a peer would have to re-parse; the JSON path is the in-Rhino-ALC-safe wire (System.Text.Json is BCL-inbox) and the MessagePack path the heavy-transitive companion/outside-Rhino wire scoped behind the firebreak, both reading the SAME `MaterialWire` shape so a JSON-decoding TS peer and a MessagePack-decoding Python companion decode field-for-field; the `MtlxDocument` (its `Seq<MtlxNode>`/`Option<string>` members) is NOT a JSON shape — it renders to `.mtlx` text through `System.Xml.Linq` at the host edge.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Globalization;                    // CultureInfo (the .mtlx invariant-culture attribute text)
using System.Text.Json;
using System.Text.Json.Serialization;
using MessagePack;
using MessagePack.Resolvers;
using Rasm.Element;                            // MaterialId (seam identity), AppearanceSummary + its seam-owned .Of content-key factory
using Rasm.Materials.Appearance.Bsdf;          // RgbSpectrum — the lobe reflectance carrier the wire color fields read
using Rasm.Materials.Appearance.Surface;       // OpenPbrSurface, ConductorIor, ConductorMetal — the OpenPBR vector + the metal axis the payload projects
using Rasm.Materials.Appearance.Graph;         // MaterialParameters, MaterialLibrary, MaterialGraph, ShadePoint, SurfaceShade, AppearanceNode, PortValue, PortId, MathOp, GraphContext, MaterialFault
using Rasm.Materials.Appearance.Texture;       // TextureSource — the MtlxCategory source the node-category map resolves
using Rhino.Geometry;                          // Point3d/Vector3d — the default-graph shade-point seed Mint composes
using Wacton.Unicolour;
using Thinktecture;                            // ComparerAccessors (NodeCategory key comparer)
using Thinktecture.Formatters;                 // ThinktectureMessageFormatterResolver
using Thinktecture.Text.Json.Serialization;    // ThinktectureJsonConverterFactory
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance.Interchange;

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct WireColor(double R, double G, double B, string Hex) {
    public static WireColor Of(Unicolour colour) {
        ColourTriplet lin = colour.RgbLinear.Triplet;
        return new WireColor(lin.First, lin.Second, lin.Third, colour.Hex);
    }
    public static WireColor Of(RgbSpectrum rgb) =>
        new(rgb.R, rgb.G, rgb.B, new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, rgb.R, rgb.G, rgb.B).Hex);
}

// Measured is DERIVED from the receipt structurally — an authored row is the one Provenance.Authored sentinel, a
// measured row carries a real device/residual; never a `Device != "authored"` magic-string compare (the prior defect).
// Method (the acquisition#ACQUISITION CaptureMethod.Key — goniophotometer/spectrophotometer/neural-svbrdf/authored) and
// AngularSamples (the goniophotometer sample count or neural texel count) carry the MEASURED INSTRUMENT across the wire so
// the TS/Py peers read HOW a material was captured, not merely THAT it was; both ride the wire by the SAME Of(p) read off
// the grown Provenance.Method.Key/.AngularSamples — a peer that infers the instrument from the device string is the defect.
public readonly record struct WireProvenance(string Device, int WavelengthCount, double FitResidual, bool Measured, string Method, int AngularSamples) {
    public static WireProvenance Of(Provenance p) => new(p.Device, p.WavelengthCount, p.FitResidual, p != Provenance.Authored, p.Method.Key, p.AngularSamples);
}

// The full OpenPBR Surface 1.1 parameter vector flattened to wire scalars — the surface#OPENPBR_SLAB OpenPbrSurface
// columns one-for-one (specular_color/specular_anisotropy, coat_color, fuzz_color, the per-channel subsurface radius
// bands, the thin-film triple) so a peer reconstructs the exact slab stack, never a lossy subset.
public readonly record struct OpenPbrGroupsWire(
    double BaseWeight, WireColor BaseColor, double BaseMetalness, double BaseDiffuseRoughness, double BaseSpecularTint,
    double SpecularWeight, WireColor SpecularColor, double SpecularRoughness, double SpecularIor, double SpecularAnisotropy,
    double TransmissionWeight, double TransmissionRoughness,
    double SubsurfaceWeight, double SubsurfaceRadiusR, double SubsurfaceRadiusG, double SubsurfaceRadiusB,
    double CoatWeight, WireColor CoatColor, double CoatRoughness, double CoatIor,
    double FuzzWeight, WireColor FuzzColor, double FuzzRoughness,
    double ThinFilmWeight, double ThinFilmThickness, double ThinFilmIor,
    WireColor EmissionColor, double EmissionLuminance, double GeometryOpacity) {

    // BaseSpecularTint reads the OpenPbrSurface column (which carries the authored MaterialParameters.SpecularTint) so
    // the tint crosses the wire; SpecularColor/CoatColor stay White and FuzzColor BaseColor (the OpenPBR neutral-tint
    // baseline — MaterialParameters carries no specular/coat/fuzz colour source, so a true tint column would be speculative).
    public static OpenPbrGroupsWire Of(OpenPbrSurface s) =>
        new(s.BaseWeight, WireColor.Of(s.BaseColor), s.BaseMetalness, s.BaseDiffuseRoughness, s.BaseSpecularTint,
            s.SpecularWeight, WireColor.Of(RgbSpectrum.White), s.SpecularRoughness, s.SpecularIor, s.SpecularAnisotropy,
            s.TransmissionWeight, s.TransmissionRoughness,
            s.SubsurfaceWeight, s.SubsurfaceRadius.R, s.SubsurfaceRadius.G, s.SubsurfaceRadius.B,
            s.CoatWeight, WireColor.Of(RgbSpectrum.White), s.CoatRoughness, s.CoatIor,
            s.FuzzWeight, WireColor.Of(s.BaseColor), s.FuzzRoughness,
            s.ThinFilmWeight, s.ThinFilmThickness, s.ThinFilmIor,
            WireColor.Of(s.EmissionColor), s.EmissionLuminance, GeometryOpacity: 1.0);
}

// The full OpenPBR-vector payload behind the seam appearance content key. Its static members are the EGRESS folds —
// Summary (the contracted seam AppearanceSummary lowering), Project (the full-payload mint), Mint (the single
// registered-row mint) — co-located on the data shape so the contracted Projection/component#COMPONENT_SUBGRAPH spelling is
// MaterialWire.Summary exactly, no parallel projection class beside the record (a static class + a record sharing the
// name is a CS0101 collision; the factory rides the record).
public sealed record MaterialWire(
    string Id,
    OpenPbrGroupsWire OpenPbr,
    string Conductor,
    WireProvenance Provenance,
    WireColor Preview) {

    static RgbSpectrum Linear(Unicolour colour) { var lin = colour.RgbLinear; return RgbSpectrum.Create(Math.Max(0.0, lin.R), Math.Max(0.0, lin.G), Math.Max(0.0, lin.B)); }

    // The CONTRACTED seam lowering Projection/component#COMPONENT_SUBGRAPH composes: a library row → the neutral seam
    // AppearanceSummary the ComponentProjector content-keys a Node.Appearance on. The base color lowers to scene-linear
    // RGB (the shading truth), metalness/roughness read the row, opacity is the transmission complement (a transmissive
    // row carries sub-unit opacity so the GLB KHR_materials_transmission channel reads it), and the transmissive bit is
    // the refractive signal DISTINCT from opacity (an opaque-alpha glass still transmits). The AppearanceKey is minted by
    // the SEAM-OWNED AppearanceSummary.Of — the kernel seed-zero XxHash128 over the canonical PBR bytes via the seam
    // CanonicalWriter→ContentAddress.Of — the EXACT SAME derivation Rasm.Bim Semantics/appearance#APPEARANCE_PROJECTION
    // composes, so a Materials OpenPBR row and a BIM IfcSurfaceStyleRendering style describing one surface mint ONE key
    // and dedup to one Node.Appearance. This owner re-assembles NO key bytes of its own (a local CanonicalWriter over the
    // PBR vector beside the seam factory is the divergence defect — Materials and Bim would couple by byte-order convention).
    public static AppearanceSummary Summary(MaterialParameters parameters) {
        RgbSpectrum baseLinear = Linear(parameters.BaseColor);
        double opacity = Math.Clamp(1.0 - parameters.Transmission, 0.0, 1.0);
        double metallic = Math.Clamp(parameters.Metalness, 0.0, 1.0);
        double roughness = Math.Clamp(parameters.Roughness, 0.0, 1.0);
        bool transmissive = parameters.Transmission > 0.0;
        // tolerance 0.0: the PBR scalars are raw appearance values, not Header-quantized measures — the seam factory
        // hashes their exact IEEE bits (the SAME no-quantization the Rasm.Bim AppearanceProjection passes).
        return AppearanceSummary.Of(baseLinear.R, baseLinear.G, baseLinear.B, metallic, roughness, opacity, transmissive, tolerance: 0.0);
    }

    // The full OpenPBR-vector payload BEHIND the seam content key — the heavy wire a renderer/GLB-material author and
    // the host-free peers decode. Composes the SINGLE surface#OPENPBR_SLAB OpenPbrSurface.Of MaterialParameters→OpenPBR
    // correspondence (the DERIVED_LOGIC primary surface.md declares once; this wire NEVER re-mints the column mapping),
    // then OpenPbrGroupsWire.Of flattens that vector to wire scalars; the conductor key is named only for a true metal.
    public static MaterialWire Project(MaterialId id, MaterialParameters parameters, Provenance provenance, SurfaceShade preview) {
        ConductorMetal conductor = ConductorIor.Resolve(Family(id), Name(id)).IfNone(ConductorMetal.Iron);
        bool isConductor = parameters.Metalness >= 1.0 && Family(id) == "metal";
        return new MaterialWire(
            id.Value,
            OpenPbrGroupsWire.Of(OpenPbrSurface.Of(parameters, conductor)),
            isConductor ? conductor.Key : string.Empty,
            WireProvenance.Of(provenance),
            WireColor.Of(preview.BaseColorLinear));
    }

    // The SINGLE full-payload mint from a registered row: resolve the parameter vector, evaluate the default graph for
    // the SurfaceShade preview, and project. The seam summary path (Summary) needs no graph evaluation — it reads the
    // row scalars directly — so a projector that only needs the AppearanceSummary never pays the shade evaluation.
    public static Fin<MaterialWire> Mint(MaterialId id, Op key) =>
        from parameters in MaterialLibrary.Lookup(id, key)
        from point in ShadePoint.Of(Point3d.Origin, Vector3d.ZAxis, Vector3d.ZAxis, Option<Vector3d>.None, 0.5, 0.5, GraphContext.Tolerant, key)
        from preview in MaterialGraph.Default.Evaluate(point, parameters, key)
        select Project(id, parameters, Provenance.Authored, preview);

    // family.name lensing over the seam MaterialId string — the codebase metal.<name> convention the conductor table
    // keys on, a pure string read over the SEAM identity, NEVER a parallel MaterialId declaration.
    static string Family(MaterialId id) => id.Value.Split('.') is [var family, ..] ? family : string.Empty;
    static string Name(MaterialId id) => id.Value.Split('.') is [_, var name, ..] ? name : id.Value;
}

// --- [COMPOSITION] -------------------------------------------------------------------------
// The one MaterialWire serialization owner. The wire shape is built from plain records (string/double/WireColor) so it
// round-trips through STJ and MessagePack directly; the Thinktecture generated factory/resolver is registered so a
// future SmartEnum/Union/ValueObject wire key (a typed Conductor, a NodeCategory) round-trips by its generated codec
// rather than a hand-rolled CultureInfo string projection. STJ is the in-Rhino-ALC-safe path; MessagePack the
// heavy-transitive companion wire behind the firebreak. The MtlxDocument is NOT a JSON/MessagePack shape — its egress
// is the host-edge System.Xml.Linq `.mtlx` render (the LanguageExt Seq/Option in MtlxNode/MtlxInput never cross a
// structured JSON codec — they project to XML elements/attributes at the host boundary, never a serialized payload here).
public static class WireCodec {
    public static readonly JsonSerializerOptions Json = Configure(new JsonSerializerOptions(JsonSerializerDefaults.Web) {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,   // ±∞/NaN survive the wire as named literals
    });

    public static readonly MessagePackSerializerOptions MessagePack =
        MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(
            ThinktectureMessageFormatterResolver.Instance,
            StandardResolver.Instance));

    static JsonSerializerOptions Configure(JsonSerializerOptions options) {
        options.Converters.Add(new ThinktectureJsonConverterFactory());          // every generated SmartEnum/Union/ValueObject key
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
}
```

```ts contract
interface AppearanceSummary {
  readonly appearanceKey: string;   // UInt128 content key as a hex string (the seam XxHash128 dedup key)
  readonly baseColorR: number;
  readonly baseColorG: number;
  readonly baseColorB: number;
  readonly metallic: number;
  readonly roughness: number;
  readonly opacity: number;
  readonly transmissive: boolean;   // the refractive flag DISTINCT from opacity/alpha — the GLB KHR_materials_transmission signal
}

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
  readonly baseSpecularTint: number;
  readonly specularWeight: number;
  readonly specularColor: WireColor;
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
  readonly coatColor: WireColor;
  readonly coatRoughness: number;
  readonly coatIor: number;
  readonly fuzzWeight: number;
  readonly fuzzColor: WireColor;
  readonly fuzzRoughness: number;
  readonly thinFilmWeight: number;
  readonly thinFilmThickness: number;
  readonly thinFilmIor: number;
  readonly emissionColor: WireColor;
  readonly emissionLuminance: number;
  readonly geometryOpacity: number;
}

interface WireProvenance {
  readonly device: string;
  readonly wavelengthCount: number;
  readonly fitResidual: number;
  readonly measured: boolean;
  readonly method: string;          // the CaptureMethod key — goniophotometer/spectrophotometer/neural-svbrdf/authored
  readonly angularSamples: number;  // the goniophotometer sample count or neural texel count
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
class AppearanceSummary:
    appearance_key: str  # UInt128 content key as hex (the seam XxHash128 dedup key)
    base_color_r: float
    base_color_g: float
    base_color_b: float
    metallic: float
    roughness: float
    opacity: float
    transmissive: bool  # the refractive flag DISTINCT from opacity/alpha — the GLB KHR_materials_transmission signal


@dataclass(frozen=True, slots=True)
class WireColor:
    r: float
    g: float
    b: float
    hex: str


@dataclass(frozen=True, slots=True)
class WireProvenance:
    device: str
    wavelength_count: int
    fit_residual: float
    measured: bool
    method: str  # the CaptureMethod key — goniophotometer/spectrophotometer/neural-svbrdf/authored
    angular_samples: int  # the goniophotometer sample count or neural texel count


@dataclass(frozen=True, slots=True)
class OpenPbrGroupsWire:
    base_weight: float
    base_color: WireColor
    base_metalness: float
    base_diffuse_roughness: float
    base_specular_tint: float
    specular_weight: float
    specular_color: WireColor
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
    coat_color: WireColor
    coat_roughness: float
    coat_ior: float
    fuzz_weight: float
    fuzz_color: WireColor
    fuzz_roughness: float
    thin_film_weight: float
    thin_film_thickness: float
    thin_film_ior: float
    emission_color: WireColor
    emission_luminance: float
    geometry_opacity: float


@dataclass(frozen=True, slots=True)
class MaterialWire:
    id: str
    open_pbr: OpenPbrGroupsWire
    conductor: str
    provenance: WireProvenance
    preview: WireColor
```

## [03]-[MATERIALX_DOCUMENT]

- Owner: `MtlxDocument` the MaterialX 1.39 document; `MtlxNode`/`MtlxInput` the node-graph element shapes; `NodeCategory` `[SmartEnum<string>]` the MaterialX node-category axis carrying each category's typed output port; `Mtlx` the static serialize/admit fold.
- Entry: `public static MtlxDocument FromGraph(MaterialGraph graph, MaterialId id, MaterialParameters parameters)` projects the `graph#MATERIAL_GRAPH` node DAG to the MaterialX node-graph document, and `public static Fin<MtlxDocument> ToOpenPbr(MaterialWire wire, Op key)` emits the `open_pbr_surface` node document from the material wire — one entry per direction, both targeting the MaterialX 1.39 `<materialx version="1.39">` root the DCC ecosystem reads; the `.mtlx` XML serialization rides `System.Xml.Linq` at the host boundary, this owner producing the portable `MtlxDocument` data the serializer renders.
- Packages: Rasm.Element (the SEAM `MaterialId` identity), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`System.Xml.Linq` at the serialize boundary).
- Growth: a new `graph#MATERIAL_GRAPH` `AppearanceNode` case maps to one `NodeCategory` row (the MaterialX node-category the case projects onto), a new MaterialX node category is one `NodeCategory` row binding one projection arm — never a per-node serializer and never a second MaterialX schema. The `texture#TEXTURE_UV` `TextureSource` aligns to the MaterialX 1.39 `noise2d`/`fractal2d`/`worleynoise2d`/`checkerboard`/`ramplr`/`ramptb`/`tiledimage`/`triplanarprojection` categories through the REALIZED `TextureSource.MtlxCategory` projection (`texture#MATERIALX_NODE_PARITY`) the `Mtlx.CategoryOf(TextureSource)` resolves so a `Texture` node round-trips; the `surface#OPENPBR_SLAB` `open_pbr_surface` is the surface node the wire emits, the Standard-Surface translation graph the same projection over the `standard_surface` category.
- Boundary: `MtlxDocument` is the ONE MaterialX shape — a per-tool encoding is the deleted form; the document is the MaterialX 1.39 `<materialx version="1.39">` root carrying one `<nodegraph>` of `<node>` elements with `<input>`/`<output>` children and one `<surfacematerial>` binding, projected from the `graph#MATERIAL_GRAPH` node union by `NodeCategory`: an `Input` projects to a `constant`/`<input>` value, a `Texture` to `image`/`worleynoise2d`/`ramplr` per its `TextureSource`, a `Math` to `multiply`/`add`/`dotproduct`/`clamp` per `MathOp`, a `Mix` to `mix`, a `Normal` to `normalmap`, and the `BsdfOutput` to `open_pbr_surface` whose inputs are the OpenPBR parameter ports the `surface#OPENPBR_SLAB` `OpenPbrSurface` carries (`base_color`/`base_metalness`/`specular_roughness`/`coat_weight`/`fuzz_weight`/`emission_color`/...); the `PortValue` polarities project onto the MaterialX typed ports through the `MtlxPort` axis (`Scalar`→`float`, `Color`→`color3`, `Vector`→`vector3`, `Frame`→`vector3` normal) — each `MtlxInput` carries the REAL port type of the slot it fills (a `base_metalness` edge is `float`, a `base_color` edge is `color3`), never a blanket `color3` on every edge (the prior illusory projection that ignored the polarity it claimed to honor); the node names derive from the `graph#MATERIAL_GRAPH` `PortId` ordinal as `node{id}` so the graph topology is recoverable from the document and a `.mtlx` consumer rewires the same DAG, and the surface-node emit path keys its sink consistently as `node{sink}` (one naming scheme across both directions, never a stray literal); color values cross as the MaterialX `color3` scene-linear triple consistent with the `MaterialWire` `WireColor` linear projection, never an sRGB byte triple; the document is portable data the host `System.Xml.Linq` serializer renders to `.mtlx` text at the app-root boundary and admits from `.mtlx` through the same node-category map, this owner never holding an XML reader/writer at an interior signature; an unmapped node category or a malformed port rails `MaterialFault.Graph`, never a partial document.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The MaterialX 1.39 typed-port axis — the four PortValue polarities project onto the MaterialX port-type strings,
// so an MtlxInput carries the REAL port type of the slot it fills rather than a blanket color3.
[SmartEnum<string>]
public sealed partial class MtlxPort {
    public static readonly MtlxPort Float   = new("float");
    public static readonly MtlxPort Color3  = new("color3");
    public static readonly MtlxPort Vector3 = new("vector3");
    public static readonly MtlxPort Surface = new("surfaceshader");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NodeCategory {
    public static readonly NodeCategory Constant       = new("constant", MtlxPort.Float);
    public static readonly NodeCategory Image          = new("image", MtlxPort.Color3);
    public static readonly NodeCategory TiledImage     = new("tiledimage", MtlxPort.Color3);
    public static readonly NodeCategory Triplanar      = new("triplanarprojection", MtlxPort.Color3);
    public static readonly NodeCategory Perlin2D       = new("noise2d", MtlxPort.Float);
    public static readonly NodeCategory Perlin3D       = new("noise3d", MtlxPort.Float);
    public static readonly NodeCategory Fractal2D      = new("fractal2d", MtlxPort.Float);
    public static readonly NodeCategory CellNoise      = new("cellnoise2d", MtlxPort.Float);
    public static readonly NodeCategory Worley         = new("worleynoise2d", MtlxPort.Float);
    public static readonly NodeCategory UnifiedNoise   = new("unifiednoise2d", MtlxPort.Float);
    public static readonly NodeCategory Checkerboard   = new("checkerboard", MtlxPort.Color3);
    public static readonly NodeCategory RampLr         = new("ramplr", MtlxPort.Color3);
    public static readonly NodeCategory RampTb         = new("ramptb", MtlxPort.Color3);
    public static readonly NodeCategory Multiply       = new("multiply", MtlxPort.Color3);
    public static readonly NodeCategory Add            = new("add", MtlxPort.Color3);
    public static readonly NodeCategory DotProduct     = new("dotproduct", MtlxPort.Float);
    public static readonly NodeCategory Clamp          = new("clamp", MtlxPort.Float);
    public static readonly NodeCategory Mix            = new("mix", MtlxPort.Color3);
    public static readonly NodeCategory Normalmap      = new("normalmap", MtlxPort.Vector3);
    public static readonly NodeCategory OpenPbrSurface = new("open_pbr_surface", MtlxPort.Surface);
    public static readonly NodeCategory StandardSurface = new("standard_surface", MtlxPort.Surface);
    public MtlxPort Port { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct MtlxInput(string Name, MtlxPort Type, string Value, Option<string> NodeName);

public sealed record MtlxNode(string Name, NodeCategory Category, MtlxPort Type, Seq<MtlxInput> Inputs);

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

    static NodeCategory MathCategory(MathOp op) =>
        op == MathOp.Add ? NodeCategory.Add
        : op == MathOp.DotProduct ? NodeCategory.DotProduct
        : op == MathOp.Clamp01 ? NodeCategory.Clamp
        : NodeCategory.Multiply;

    public static MtlxDocument FromGraph(MaterialGraph graph, MaterialId id, MaterialParameters parameters) {
        Seq<MtlxNode> nodes = graph.Nodes.Map(n => { NodeCategory c = CategoryOf(n); return new MtlxNode($"node{n.Id.Value}", c, c.Port, InputsOf(n)); });
        return new MtlxDocument(MtlxDocument.Schema, nodes, $"node{graph.Sink.Value}", id.Value.Replace('.', '_'));
    }

    static Seq<MtlxInput> InputsOf(AppearanceNode node) => node.Switch(
        input:      static _ => Seq<MtlxInput>(),
        texture:    static _ => Seq(new MtlxInput("file", MtlxPort.Color3, string.Empty, Option<string>.None)),
        math:       static m => m.Rhs.Match(Some: r => Seq(Edge("in1", MtlxPort.Color3, m.Lhs), Edge("in2", MtlxPort.Color3, r)), None: () => Seq(Edge("in1", MtlxPort.Color3, m.Lhs))),
        mix:        static x => Seq(Edge("fg", MtlxPort.Color3, x.A), Edge("bg", MtlxPort.Color3, x.B), Edge("mix", MtlxPort.Float, x.Factor)),
        normal:     static n => Seq(Edge("in", MtlxPort.Vector3, n.Source)),
        // Each BsdfOutput edge carries the REAL OpenPBR port polarity: base_color/emission_color are color3, the
        // metalness/roughness scalars are float, the geometry normal is vector3 — never a blanket color3.
        bsdfOutput: static o => Seq(
            Edge("base_color", MtlxPort.Color3, o.BaseColor),
            Edge("base_metalness", MtlxPort.Float, o.Metalness),
            Edge("specular_roughness", MtlxPort.Float, o.Roughness),
            Edge("geometry_normal", MtlxPort.Vector3, o.NormalFrame),
            Edge("emission_color", MtlxPort.Color3, o.Emission)));

    static MtlxInput Edge(string name, MtlxPort type, PortId source) => new(name, type, string.Empty, Some($"node{source.Value}"));

    public static Fin<MtlxDocument> ToOpenPbr(MaterialWire wire, Op key) =>
        string.IsNullOrWhiteSpace(wire.Id)
            ? Fin.Fail<MtlxDocument>(MaterialFault.Graph(key, "<mtlx-empty-material-id>"))
            : Fin.Succ(new MtlxDocument(MtlxDocument.Schema, Seq(SurfaceNode(wire)), "node0", wire.Id.Replace('.', '_')));

    // The full open_pbr_surface node — every OpenPBR Surface 1.1 input the wire carries (the slab-stack columns
    // surface#OPENPBR_SLAB defines), each port typed by its real polarity, so the .mtlx round-trips the complete
    // surface rather than a 12-input subset. Named node0 to key consistently with FromGraph's node{id} scheme.
    static MtlxNode SurfaceNode(MaterialWire wire) {
        OpenPbrGroupsWire g = wire.OpenPbr;
        return new MtlxNode("node0", NodeCategory.OpenPbrSurface, MtlxPort.Surface, Seq(
            Value("base_weight", MtlxPort.Float, g.BaseWeight),
            Color("base_color", g.BaseColor),
            Value("base_metalness", MtlxPort.Float, g.BaseMetalness),
            Value("base_diffuse_roughness", MtlxPort.Float, g.BaseDiffuseRoughness),
            Value("base_specular_tint", MtlxPort.Float, g.BaseSpecularTint),
            Value("specular_weight", MtlxPort.Float, g.SpecularWeight),
            Color("specular_color", g.SpecularColor),
            Value("specular_roughness", MtlxPort.Float, g.SpecularRoughness),
            Value("specular_ior", MtlxPort.Float, g.SpecularIor),
            Value("specular_roughness_anisotropy", MtlxPort.Float, g.SpecularAnisotropy),
            Value("transmission_weight", MtlxPort.Float, g.TransmissionWeight),
            Value("subsurface_weight", MtlxPort.Float, g.SubsurfaceWeight),
            ColorTriple("subsurface_radius", g.SubsurfaceRadiusR, g.SubsurfaceRadiusG, g.SubsurfaceRadiusB),
            Value("coat_weight", MtlxPort.Float, g.CoatWeight),
            Color("coat_color", g.CoatColor),
            Value("coat_roughness", MtlxPort.Float, g.CoatRoughness),
            Value("coat_ior", MtlxPort.Float, g.CoatIor),
            Value("fuzz_weight", MtlxPort.Float, g.FuzzWeight),
            Color("fuzz_color", g.FuzzColor),
            Value("fuzz_roughness", MtlxPort.Float, g.FuzzRoughness),
            Value("thin_film_weight", MtlxPort.Float, g.ThinFilmWeight),
            Value("thin_film_thickness", MtlxPort.Float, g.ThinFilmThickness),
            Value("thin_film_ior", MtlxPort.Float, g.ThinFilmIor),
            Color("emission_color", g.EmissionColor),
            Value("emission_luminance", MtlxPort.Float, g.EmissionLuminance),
            Value("geometry_opacity", MtlxPort.Float, g.GeometryOpacity)));
    }

    static MtlxInput Value(string name, MtlxPort type, double v) => new(name, type, v.ToString("R", CultureInfo.InvariantCulture), Option<string>.None);
    static MtlxInput Color(string name, WireColor c) => new(name, MtlxPort.Color3, $"{c.R.ToString("R", CultureInfo.InvariantCulture)}, {c.G.ToString("R", CultureInfo.InvariantCulture)}, {c.B.ToString("R", CultureInfo.InvariantCulture)}", Option<string>.None);
    static MtlxInput ColorTriple(string name, double r, double g, double b) => new(name, MtlxPort.Color3, $"{r.ToString("R", CultureInfo.InvariantCulture)}, {g.ToString("R", CultureInfo.InvariantCulture)}, {b.ToString("R", CultureInfo.InvariantCulture)}", Option<string>.None);
}
```

## [04]-[RESEARCH]

- [SEAM_APPEARANCE_KEY]: the cross-folder appearance reconciliation rides ONE content key — the seam `Rasm.Element/Graph/element#NODE_MODEL` `AppearanceSummary(UInt128 AppearanceKey, double BaseColorR/G/B, double Metallic, double Roughness, double Opacity, bool Transmissive)` carries the neutral PBR scalars a consumer reads flat (the `Transmissive` refractive flag DISTINCT from `Opacity`/alpha) plus the `AppearanceKey` content hash. `MaterialWire.Summary` is the CONTRACTED entry `Projection/component#COMPONENT_SUBGRAPH` `ComponentSubgraph.Capture` composes (`MaterialLibrary.Lookup(id, key).Map(MaterialWire.Summary)`), composing the seam-owned `AppearanceSummary.Of` factory which mints the `AppearanceKey` as the kernel seed-zero `XxHash128` over the canonical PBR bytes (the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` `ContentAddress`/`CanonicalWriter`, the ONE hasher, NO second seed) — the SAME factory `Rasm.Bim` `Semantics/appearance#APPEARANCE_PROJECTION` `AppearanceProjection.Project` composes, so a Materials OpenPBR row and a BIM `IfcSurfaceStyleRendering` style describing one surface mint one `AppearanceKey` and dedup to one `Node.Appearance` id; a local `CanonicalWriter` beside the seam factory in either owner is the byte-order divergence defect. The full `MaterialWire`/`MtlxDocument` are the payloads BEHIND that key — the renderer/GLB `KHR_materials_pbrMetallicRoughness`+`KHR_materials_transmission` author and the host-free peers decode them by the content key — so the seam node stays thin (key + 6 scalars) and the heavy OpenPBR vector never inflates the graph. The prior page minted a `MaterialWire` with NO seam lowering and NO content key, leaving `ComponentSubgraph.Capture`'s `MaterialWire.Summary` call a phantom contract; this is REALIZED.
- [MATERIALX_GRAPH_INTERCHANGE]: the MaterialX 1.39 `.mtlx` document is the platform-neutral node-graph content schema (root `<materialx version="1.39">`, `<nodegraph>` wrapping `<node>` elements with `<input>`/`<output>` children, the `standard_surface`/`open_pbr_surface`/`surfacematerial` surface nodes); the `MtlxDocument`/`MtlxNode` shape and the `NodeCategory` map are the realized projection, the `System.Xml.Linq` `.mtlx` serialize/admit the host-boundary render. The `NodeCategory` axis carries the full verified MaterialX 1.39 standard-library texture/noise/ramp set (`noise2d`/`noise3d`/`fractal2d`/`cellnoise2d`/`worleynoise2d`/`unifiednoise2d`/`checkerboard`/`ramplr`/`ramptb`/`tiledimage`/`triplanarprojection`) the `texture#TEXTURE_UV` `TextureSource.MtlxCategory` projection resolves through `Mtlx.CategoryOf(TextureSource)`, each category carrying its REAL output port through the `MtlxPort` typed axis, so the `AppearanceNode`↔`NodeCategory` and the per-`TextureSource`-case mappings are settled against the AcademySoftwareFoundation/MaterialX 1.39 `MaterialX.StandardNodes.md`. The `MtlxInput.Type` carries the per-slot port polarity (`Scalar`→`float`, `Color`→`color3`, `Vector`→`vector3`) the prior blanket-`color3` `Edge` helper dropped, and the `open_pbr_surface` node emits the FULL OpenPBR Surface 1.1 input set (`base_diffuse_roughness`/`specular_color`/`specular_roughness_anisotropy`/`subsurface_radius`/`coat_color`/`fuzz_color`/`fuzz_roughness`/`thin_film_*`/`geometry_opacity`), not the 12-input subset; the Standard-Surface-to-OpenPBR translation graph is the same projection over the `standard_surface` category.
- [MTLX_SCHEMA_VALIDATION]: the `.mtlx` document validates against the MaterialX 1.39 schema at the host import boundary (the `MaterialX` Python/C++ runtime or a schema validator); the managed projection produces the portable `MtlxDocument` and the schema-validation round-trip is a host-edge concern the app root owns, this owner never binding a native MaterialX runtime — a malformed projection rails `MaterialFault.Graph` before serialization rather than emitting an invalid document. The `MtlxInput.Value` `ToString("R", CultureInfo.InvariantCulture)` is the MaterialX `float`/`color3` XML attribute-TEXT encoding (the `.mtlx` schema requires the invariant-culture round-trip literal), distinct from the `MaterialWire` JSON/MessagePack codec — the XML text formatting stays at the `System.Xml.Linq` boundary, the structured `MaterialWire` shape rides `WireCodec`.
- [THINKTECTURE_WIRE_CODEC]: REALIZED — the `MaterialWire`/`OpenPbrGroupsWire`/`WireProvenance`/`WireColor` shapes are built from plain records (`string`/`double`/`WireColor`, no `Seq`/`Option`/SmartEnum-typed field — `Conductor` crosses as a `string` key) so they serialize through `WireCodec` directly, and the one `JsonSerializerOptions` carries the Thinktecture generated `ThinktectureJsonConverterFactory` (the `JsonConverterFactory` resolving every `[SmartEnum]`/`[Union]`/`[ValueObject]` key by its generated `IObjectFactory`/`IKeyedValueObject` shape) so a FUTURE typed wire key (a `ConductorMetal` or `NodeCategory` field) round-trips by its generated codec rather than a hand-rolled string projection, and the one `MessagePackSerializerOptions` carries `ThinktectureMessageFormatterResolver.Instance` composed with `StandardResolver.Instance` for the companion/outside-Rhino binary wire. The `MtlxDocument` is NOT a JSON/MessagePack shape — its `Seq<MtlxNode>`/`Option<string>` members project to `System.Xml.Linq` `.mtlx` elements/attributes at the host edge (the `MtlxInput.Value` invariant-culture text), never through a structured codec, so no `Seq`/`Option` JSON converter is needed and `WireCodec` carries no `MtlxDocument` overload. The System.Text.Json path is the in-Rhino-ALC-safe wire (BCL-inbox, no heavy transitive closure), the MessagePack path the heavy-transitive companion path scoped behind the Speckle.Sdk firebreak (`ARCHITECTURE` line 39 single-mint invariant). The `Rasm.Materials.csproj` admits `Thinktecture.Runtime.Extensions.Json`/`.MessagePack` (the `10.4.0` pins central in `Directory.Packages.props`, version-less folder-local references), and the README `[03]-[SUBSTRATE_PACKAGES]` registry lists both serializer companions beside the core Thinktecture pin. The TS `decodeMaterialWire`/`AppearanceSummary` and Python `MaterialWire.from_wire`/`AppearanceSummary` decode the SAME JSON shape the codec mints, the single-mint invariant holding with the codec as the canonical shape — a peer re-mint of the OpenPBR algebra or a second string projection is the named cross-language drift defect.
