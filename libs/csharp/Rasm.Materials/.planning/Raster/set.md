# [MATERIALS_SET]

THE CHANNEL VOCABULARY AND THE BAKED SET. One `TextureChannel` `[SmartEnum<string>]` roster closes the per-texel appearance field family — the OpenPBR Surface 1.1 inputs (each read off its `surface#OPENPBR_SLAB` `OpenPbrSurface` column, the two neutral-tint colours OpenPBR defines without a vector column synthesized White at the lens), the geometry-group inputs, and the derived modulators — each row carrying its group, component count, transfer, decoded neutral, unit, mip law, KTX payload policy, origin lens, MaterialX binding, sink slot, and ingest aliases as DATA, so a new bakeable field is a row and never a second channel surface. One `TextureSet` record is the extent-coherent, content-keyed bundle of `plane#TEXTURE_PLANE` pyramids those rows address, admitted once at `TextureSet.Of` under the extent, transfer, layer, alpha, variant, and pack gates; one `SetIngest.Classify` fold classifies a foreign directory or a peer-declared manifest into a `SetManifest` by alias alone, accumulating every unclaimed stem rather than inferring a channel; and one `SetBind.Bind` entry lowers a set BACK into the appearance engine — the `graph#MATERIAL_GRAPH` `MaterialGraph` program its sink-slot channels drive, or the `graph#MATERIAL_LIBRARY` `MaterialParameters` row the full channel roster reconstructs — closing the round trip from pressed planes to shadeable material rather than stopping at encodable bytes.

The channel roster is a PROJECTION of an existing closed vocabulary, never a hand-picked subset: every OpenPBR row reads its own `OpenPbrSurface` column through the row's `ColumnLens`, so coat roughness, fuzz colour, transmission, and thin-film thickness carry bakeable planes the day the vector carries the column. The canonical channel name is `snake_case` and IS the OpenPBR identifier verbatim where OpenPBR names the input, so the `.mtlx` port binding is mechanical; the C# identifier is its PascalCase, with `SpecularAnisotropy` the one row whose identifier shortens against its `specular_roughness_anisotropy` key. Channel values live in the DECODED domain — a normal is the signed `(0,0,1)` unit vector, curvature the signed `[-1,1]` field, height the normalized `[0,1]` scalar whose millimetre span rides the set — and integer encoding is wholly `plane#PLANE_VOCABULARY`'s storage concern, so one `NormalConvention` green-sign flip serves every depth and no page re-derives an encode rule. The page composes the `plane#TEXTURE_PLANE` `TexturePlane`/`TexturePyramid`/`PlaneFormat`/`PlaneTransfer`/`AlphaMode`/`MipPolicy`/`PlaneDepth` substrate, the `codec#RASTER_CODEC` `RasterFormat`/`KtxPayload` container and quality vocabulary, the `filter#PLANE_OP` `PlaneOp` derivation family, the `tile#TILE_GATE` `TileProof` tileability evidence, the `surface#OPENPBR_SLAB` `OpenPbrSurface`/`ConductorMetal` vector, the `graph#MATERIAL_GRAPH` node union and `graph#MATERIAL_LIBRARY` parameter row, the `texture#TEXTURE_UV` `TextureUv.Port`/`Sample` sampler and `ShadeVec4` register, the seam `Rasm.Element` `MaterialId` identity and `ContentAddress` address spelling, and the kernel `ContentHash`/`Dimension`/`UnitInterval`/`ValidityClaim` owners — reminting no sampler, no colour register, no address, no content key, and no fault.

## [01]-[INDEX]

- [02]-[TEXTURE_CHANNEL]: the `ChannelGroup`/`ChannelUnit`/`NormalConvention`/`SinkSlot` axes, the `MtlxBinding`/`ChannelOrigin` row columns with the `ColumnLens` bidirectional correspondence, the `TextureChannel` roster with its lazily-derived indexes, and the `ChannelPack` `orm`/`mra` slot table.
- [03]-[TEXTURE_SET]: the `LayerLaw` layer axis, the `UdimTile` Mari value object, the `ChannelPackPlane` packed carrier, the `EgressSlot`/`EgressVariant` naming vocabulary, the `TextureSet` record with its `Of` admission ladder, content key, and one egress-name entry, and the `UdimSheet` per-tile assembly.
- [04]-[SET_INGEST]: the `PlaneProbe` evidence row, the `IngestSource` union with its python-wire `Peer` arm, the roster-derived alias index, the `ClassifiedMap`/`SetManifest` monoid, and the total `SetIngest.Classify` fold beside the fallible `Peer` decode seam.
- [05]-[SET_BIND]: the `BindTarget`/`SetBinding` unions and the one `SetBind.Bind` lowering — the sink-slot graph program, the per-texel parameter row over channels AND packs, and the measured plane-mean summary row.

## [02]-[TEXTURE_CHANNEL]

- Owner: `TextureChannel` `[SmartEnum<string>]` the closed per-texel field roster; `ChannelGroup` `[SmartEnum<string>]` the OpenPBR group axis; `ChannelUnit` `[SmartEnum<string>]` the UCUM-tokened unit axis; `NormalConvention` `[SmartEnum<string>]` the green-polarity axis; `SinkSlot` `[SmartEnum<string>]` the `graph#MATERIAL_GRAPH` `BsdfOutput` port axis; `MtlxBinding` `[Union]` the `.mtlx` egress law; `ChannelOrigin` `[Union]` the per-row production law; `ColumnLens` the bidirectional `OpenPbrSurface`↔`MaterialParameters` correspondence; `ChannelPack` `[SmartEnum<string>]` the two packing orders.
- Cases: channel {the OpenPBR rows `base_weight`…`emission_luminance`, the geometry rows `geometry_opacity`/`geometry_normal`/`geometry_coat_normal`/`geometry_tangent`/`geometry_coat_tangent`, the derived rows `height`/`occlusion`/`curvature`} · group {`base`, `specular`, `transmission`, `subsurface`, `coat`, `fuzz`, `thinFilm`, `emission`, `geometry`, `derived`} · unit {`none`, `mm`, `nm`, `cd/m2`} · convention {`gl`, `dx`} · sink-slot {`baseColor`, `metalness`, `roughness`, `normal`, `emission`} · mtlx-binding {`Canonical`, `Scaled`, `Split`, `Lowered`, `Absent`} · origin {`Shaded`, `Geometric`, `Derived`} · pack {`orm`, `mra`}.
- Law: `OpenPbrSurface.Conductor` and `geometry_thin_walled` are the TWO deliberate exclusions — a conductor row and a double-sided-shell flag are set-level facts no per-texel field carries, so the conductor rides `TextureSet.Conductor` and the shell flag never enters the roster.
- Law: every derived index projects from `Items` through a `Lazy<T>` accessor, never an eager `static readonly` field initializer — a field initializer inside the roster's own type runs during that type's class construction, before the generated `Items` materialization has published, so an eager index captures an empty roster and poisons every consumer that reads it.
- Entry: the roster IS the entry — `TextureChannel.Items` is the ordered vocabulary every downstream fold reads, `TextureChannel.Get(key)`/`TryGet(key, out row)` resolve a wire key, `Ordinal` is the ONE declaration-order rank the set key preimage and the `press#PRESS_PLAN` binding order both sort on, `BySlot`/`ByGroup` are the derived indexes, and `MtlxInput` resolves the `.mtlx` port name from the binding row so the interchange document never carries a translation column.
- Packages: `plane#TEXTURE_PLANE` (composed — `PlaneTransfer`/`MipPolicy` the row columns select), `codec#RASTER_CODEC` (composed — `KtxPayload` the per-row quality policy), `filter#PLANE_OP` (composed — `PlaneOp`/`HeightSolver`/`HeightDerivative`/`HeightEvidence`, the derivation each `Derived` row carries), `Rasm.Materials.Appearance.Surface` (composed — the `OpenPbrSurface` column set the lens reads), `Rasm.Materials.Appearance.Graph` (composed — `MaterialParameters` the lens writes, `ShadePoint`/`SurfaceShade` the geometric and sink lenses read, `PortId`/`PortValue` the slot binds), `Rasm.Materials.Appearance.Texture` (composed — `ShadeVec4` the one field register, `Channel` the sampler modality each `SinkSlot` names), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`, `Lazy<T>`).
- Growth: a new bakeable field is one `TextureChannel` row carrying its twelve columns — never a second roster, never a per-channel type, and never a caller-ordered tuple; a new packing order is one `ChannelPack` row naming its three slots; a new unit is one `ChannelUnit` row carrying its UCUM token; a new `.mtlx` egress irregularity is one `MtlxBinding` case, so a unit fork, a shape split, or a lowered-into input stays a typed row rather than prose a transcriber must remember. The OpenPBR half grows BY DERIVATION — a column added to `OpenPbrSurface` earns a row whose `ColumnLens` reads it; `specular_color` and `coat_color` are the TWO rows OpenPBR defines without a vector column, synthesized White at the lens, so the roster is the vector's projection PLUS those two synthesized tints, never a hand-picked subset.
- Boundary: channel values are DECODED and signed — `geometry_normal` neutral is the unit `(0,0,1)`, `geometry_tangent` the unit `(1,0,0)`, `curvature` the signed zero, `height` the normalized `0.5` — so the `(v+1)/2` integer encode and its `2v−1` decode live wholly at `plane#PLANE_VOCABULARY` and the conversion appears exactly once in the corpus; `NormalConvention.ToGl` is therefore one green-sign flip over the decoded texel rather than a depth-branching pair, and the `dx` row converts ONCE at ingest so no plane leaves this page carrying `−Y` green. The `graph#MATERIAL_GRAPH` `Normal` arm reads the OPPOSITE convention — its decode is `2v−1` over a `[0,1]` tangent-space sample, the encoding `MaterialGraph.Default` seeds as `(0.5,0.5,1.0)` — so `SinkSlot.Encode` re-encodes the decoded plane texel at the bind and the two owners meet at exactly one projection column; binding a decoded normal straight onto the node inverts X and Y at every texel, which no gate downstream can see. Gloss is NOT a channel — `gloss`/`glossiness`/`smoothness` are `specular_roughness` ingest aliases whose `ClassifiedMap.Inverted` flag records the `roughness = 1 − gloss` inversion, applied by the `filter#PLANE_OP` `RemapCurve.Levels.Invert` curve in the LINEAR domain after the plane decodes, so an `srgb`-authored gloss plane inverted before decode (the silent-roughness fork) is unrepresentable and no downstream surface holds a gloss spelling. `MipPolicy.RoughnessVariance` is the roughness rows' declared law and it is PAIRED — `Pair` names the normal channel whose per-level variance the fold consumes, resolving to `geometry_coat_normal` for the coat group and `geometry_normal` elsewhere — so `press#TEXTURE_PRESS` reads the pairing off the row rather than guessing, and a roughness channel mipped under `Box` alone is a stated quality floor the press receipt records. The KTX payload column is a QUALITY POLICY, not a container choice: vector channels take `KtxPayload.Uastc` because ETC1S destroys a normal's directional coherence, colour channels open at `KtxPayload.Etc1s` and raise to `Uastc` on a set-level quality floor, and `KtxPayload.RawBcn` never appears on a row because a raw-BCn KTX2 is a desktop payload no Basis-transcoding consumer reads. `specular_color` and `coat_color` carry no `OpenPbrSurface` column, so their lens reads `RgbSpectrum.White` — the OpenPBR neutral-tint baseline the vector synthesizes — and a baked tint plane binds through `SetBind` with no vector change; `base_specular_tint` and `transmission_roughness` are Rasm columns OpenPBR does not name, so their `MtlxBinding` is `Lowered("specular_color")` and `Absent` respectively, `thin_film_thickness` carries `Scaled(1e-3)` for the `.mtlx` micrometre input against its nanometre plane, and `subsurface_radius` carries `Split("subsurface_radius_scale")` for the radius-and-scale pair the document takes — each irregularity a row the transcriber cannot omit rather than a sentence it can.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;                                // Seq, Option, Fin, HashMap
using Rasm.Domain;                                // Op, ContentHash, ValidityClaim, IValidityEvidence
using Rasm.Element.Composition;                   // MaterialId — the SEAM material identity, composed not re-declared
using Rasm.Element.Projection;                    // ContentAddress — the X32 wire spelling and its ONE lowering site
using Rasm.Materials.Appearance.Bsdf;             // MaterialFault (band 2450), RgbSpectrum
using Rasm.Materials.Appearance.Graph;            // MaterialGraph, AppearanceNode, PortId, PortValue, MaterialParameters, SubsurfaceRadius, ThinFilm, ShadePoint, SurfaceShade
using Rasm.Materials.Appearance.Surface;          // OpenPbrSurface, ConductorMetal
using Rasm.Materials.Appearance.Texture;          // TextureSource, TextureUv, UvSample, SamplerState, Channel, ShadeVec4
using Rasm.Numerics;                              // Dimension, UnitInterval
using Rhino.Geometry;                             // Vector3d — the shading-frame axis the geometric lens reads
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [TYPES] -------------------------------------------------------------------------------
// The OpenPBR input GROUP each row belongs to. Group is what makes the coat pairing a DERIVATION rather than a
// per-row column: a roughness row in the coat group pairs with the coat normal, every other with the base normal.
// It is also the ordering axis interchange and any panel reads, so a group is never re-derived from a key prefix.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChannelGroup {
    public static readonly ChannelGroup Base         = new("base");
    public static readonly ChannelGroup Specular     = new("specular");
    public static readonly ChannelGroup Transmission = new("transmission");
    public static readonly ChannelGroup Subsurface   = new("subsurface");
    public static readonly ChannelGroup Coat         = new("coat");
    public static readonly ChannelGroup Fuzz         = new("fuzz");
    public static readonly ChannelGroup ThinFilm     = new("thinFilm");
    public static readonly ChannelGroup Emission     = new("emission");
    public static readonly ChannelGroup Geometry     = new("geometry");
    public static readonly ChannelGroup Derived      = new("derived");
}

// Ucum is the unit token the Projection/observability instrument rows and the Projection/analytics column
// schema carry; None is UCUM unity "1", never an empty string a dimensionless series cannot spell.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChannelUnit {
    public static readonly ChannelUnit None       = new("none",  ucum: "1");
    public static readonly ChannelUnit Millimetre = new("mm",    ucum: "mm");
    public static readonly ChannelUnit Nanometre  = new("nm",    ucum: "nm");
    public static readonly ChannelUnit Luminance  = new("cd/m2", ucum: "cd/m2");
    public string Ucum { get; }
}

// The green-channel polarity of a tangent-space normal plane. GL (+Y) is the OpenGL/glTF/USD/MaterialX
// convention AND the canonical wire form; DX (-Y) is admitted at ingest and converted BEFORE the plane is
// keyed. The flip is a green-sign multiply because a channel texel is DECODED and signed here — the integer
// 1-g spelling belongs to plane#PLANE_VOCABULARY's encode, so this page carries no depth branch.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NormalConvention {
    public static readonly NormalConvention Gl = new("gl", greenSign:  1.0);
    public static readonly NormalConvention Dx = new("dx", greenSign: -1.0);
    public double GreenSign { get; }

    public ShadeVec4 ToGl(ShadeVec4 decoded) => decoded with { Y = GreenSign * decoded.Y };
}

// The graph#MATERIAL_GRAPH BsdfOutput ports a channel drives, each row carrying FOUR correspondences the two
// directions of this page consume: the PortId MaterialGraph.Default already wires that slot at (so a bound
// program and the default program share one topology and no literal is re-authored here), the Channel modality
// the Texture node projects through, the Encode projection that re-encodes a DECODED plane texel into the
// convention the node arm expects, and the Read projection that pulls this slot's column out of a shaded
// SurfaceShade for press#TEXTURE_PRESS. Encode is identity on four rows: only the normal port decodes 2v-1.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SinkSlot {
    public static readonly SinkSlot BaseColor = new("baseColor", portOrdinal: 1, modality: Channel.Color,  encode: Verbatim, read: static s => ShadeVec4.FromColor(s.BaseColorLinear), fallback: static p => new PortValue.Color(p.BaseColor));
    public static readonly SinkSlot Metalness = new("metalness", portOrdinal: 2, modality: Channel.Scalar, encode: Verbatim, read: static s => Lane(s.Metalness),                       fallback: static p => new PortValue.Scalar(p.Metalness));
    public static readonly SinkSlot Roughness = new("roughness", portOrdinal: 3, modality: Channel.Scalar, encode: Verbatim, read: static s => Lane(s.Roughness),                       fallback: static p => new PortValue.Scalar(p.Roughness));
    public static readonly SinkSlot Normal    = new("normal",    portOrdinal: 4, modality: Channel.Vector, encode: Bias,     read: static s => Axis(s.ShadingFrame.ZAxis),               fallback: static _ => new PortValue.Vector(new Vector3d(0.5, 0.5, 1.0)));
    public static readonly SinkSlot Emission  = new("emission",  portOrdinal: 6, modality: Channel.Color,  encode: Verbatim, read: static s => ShadeVec4.FromColor(s.EmissionLinear),   fallback: static p => new PortValue.Color(p.Emission));

    public PortId Port => PortId.Of(PortOrdinal);
    public int PortOrdinal { get; }
    public Channel Modality { get; }

    [UseDelegateFromConstructor]
    public partial PortValue Encode(PortValue decoded);
    [UseDelegateFromConstructor]
    public partial ShadeVec4 Read(SurfaceShade shade);
    // The Input node a slot the set does not cover keeps, pulling the same column MaterialGraph.Default pulls —
    // so an uncovered slot is the default program's own node rather than a second spelling of it.
    [UseDelegateFromConstructor]
    public partial PortValue Fallback(MaterialParameters row);

    // The graph Normal arm decodes 2v-1 over a [0,1] sample, so a DECODED signed plane texel re-encodes here and
    // nowhere else; binding the signed texel straight through inverts X and Y at every texel invisibly.
    static PortValue Verbatim(PortValue decoded) => decoded;
    static PortValue Bias(PortValue decoded) =>
        decoded.AsVector switch { var v => new PortValue.Vector(new Vector3d((v.X + 1.0) * 0.5, (v.Y + 1.0) * 0.5, (v.Z + 1.0) * 0.5)) };

    static ShadeVec4 Lane(double scalar) => new(scalar, 0.0, 0.0, 1.0);
    static ShadeVec4 Axis(Vector3d axis) => new(axis.X, axis.Y, axis.Z, 1.0);
}

// The .mtlx egress law per channel. Canonical is the overwhelming case — the channel key IS the
// open_pbr_surface input name, so the interchange#MATERIALX_DOCUMENT OpenPbrPorts fold needs no translation
// column. The four irregular cases are the forks a transcribing surface mints by omission: a nanometre
// thickness against a micrometre input, a three-band radius against a radius+scale pair, a Rasm column
// OpenPBR lowers INTO another input, and a Rasm column .mtlx never carries.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MtlxBinding {
    private MtlxBinding() { }
    public sealed record Canonical() : MtlxBinding;
    public sealed record Scaled(double Factor) : MtlxBinding;
    public sealed record Split(string ScaleInput) : MtlxBinding;
    public sealed record Lowered(string Input) : MtlxBinding;
    public sealed record Absent() : MtlxBinding;

    public static readonly MtlxBinding Verbatim = new Canonical();
    public static readonly MtlxBinding Unmapped = new Absent();
}

// The bidirectional column correspondence one owner carries in BOTH directions: Read lowers the surface
// vector to a texel a bake writes, Write lifts a sampled texel back onto the parameter row a per-texel shade
// reconstructs. Write is a TYPED absence — base_weight, specular_weight, the two synthesized tints, coat_ior,
// base_diffuse_roughness, and fuzz_roughness have no MaterialParameters column, so a set carrying them
// reconstructs the OpenPBR vector and never a fabricated row column.
public sealed record ColumnLens(
    Func<OpenPbrSurface, ShadeVec4> Read,
    Option<Func<MaterialParameters, ShadeVec4, MaterialParameters>> Write);

// How a channel's texels come to exist. Shaded reads the lowered OpenPBR vector at each texel; Geometric reads
// the shade point's own frame; Derived names BOTH the sibling channel it folds from AND the filter#PLANE_OP
// step that folds it — so press#TEXTURE_PRESS reads the derivation off the roster rather than trusting a
// caller-supplied post chain to contain it, and a plan requesting occlusion gets the occlusion sweep by
// construction. From is the channel KEY, never a TextureChannel reference, so no row's initializer depends on
// another row's static initialization order.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChannelOrigin {
    private ChannelOrigin() { }
    public sealed record Shaded(ColumnLens Lens) : ChannelOrigin;
    public sealed record Geometric(Func<ShadePoint, ShadeVec4> Read) : ChannelOrigin;
    public sealed record Derived(string From, PlaneOp Fold) : ChannelOrigin;
}

// THE ROSTER. The OpenPBR Surface 1.1 inputs — each projected off its surface#OPENPBR_SLAB OpenPbrSurface
// column, with specular_color and coat_color the TWO rows the vector carries no column for, synthesized
// White at the lens — the geometry-group inputs, and the derived modulators. Neutral is the OpenPBR default in the
// channel's declared unit and DECODED domain — the constant a producer writes into an absent packed slot, a
// mip gutter, and a UDIM hole; it is never a weight-zero sentinel (ThinFilm.None is that, and it is not a
// channel neutral). SpecularAnisotropy is the ONE identifier that does not derive mechanically from its key.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TextureChannel {
    // --- [OPENPBR_CHANNELS]
    public static readonly TextureChannel BaseWeight            = new("base_weight",                   group: ChannelGroup.Base,         components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(1.0),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.BaseWeight)),                                                                                                        mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel BaseColor             = new("base_color",                    group: ChannelGroup.Base,         components: 3, transfer: PlaneTransfer.Srgb,   neutral: Rgb(0.8, 0.8, 0.8),     unit: ChannelUnit.None,       mip: MipPolicy.Kaiser,            payload: KtxPayload.Etc1s, origin: Column(static s => Rgb(s.BaseColor),               static (p, v) => p with { BaseColor = v.AsColorUnchecked() }),                                 mtlx: MtlxBinding.Verbatim,                            slot: SinkSlot.BaseColor,    aliases: Seq("albedo", "diffuse", "basecolor", "col", "color", "d", "alb"));
    public static readonly TextureChannel BaseMetalness         = new("base_metalness",                group: ChannelGroup.Base,         components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.BaseMetalness),        static (p, v) => p with { Metalness = Unit(v.X) }),                                            mtlx: MtlxBinding.Verbatim,                            slot: SinkSlot.Metalness,    aliases: Seq("metallic", "metalness", "metal", "m", "mtl"));
    public static readonly TextureChannel BaseDiffuseRoughness  = new("base_diffuse_roughness",        group: ChannelGroup.Base,         components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.None,       mip: MipPolicy.RoughnessVariance, payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.BaseDiffuseRoughness)),                                                                                              mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel BaseSpecularTint      = new("base_specular_tint",            group: ChannelGroup.Base,         components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.BaseSpecularTint),     static (p, v) => p with { SpecularTint = Unit(v.X) }),                                         mtlx: new MtlxBinding.Lowered("specular_color"),       slot: None,                  aliases: Empty);
    public static readonly TextureChannel SpecularWeight        = new("specular_weight",               group: ChannelGroup.Specular,     components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(1.0),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.SpecularWeight)),                                                                                                    mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel SpecularColor         = new("specular_color",                group: ChannelGroup.Specular,     components: 3, transfer: PlaneTransfer.Srgb,   neutral: Rgb(1.0, 1.0, 1.0),     unit: ChannelUnit.None,       mip: MipPolicy.Kaiser,            payload: KtxPayload.Etc1s, origin: Column(static _ => Rgb(RgbSpectrum.White)),                                                                                                      mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Seq("spec", "specular", "speccol"));
    public static readonly TextureChannel SpecularRoughness     = new("specular_roughness",            group: ChannelGroup.Specular,     components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.3),            unit: ChannelUnit.None,       mip: MipPolicy.RoughnessVariance, payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.SpecularRoughness),    static (p, v) => p with { Roughness = Unit(v.X) }),                                            mtlx: MtlxBinding.Verbatim,                            slot: SinkSlot.Roughness,    aliases: Seq("roughness", "rough", "rgh", "r"));
    public static readonly TextureChannel SpecularAnisotropy    = new("specular_roughness_anisotropy", group: ChannelGroup.Specular,     components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.SpecularAnisotropy),   static (p, v) => p with { Anisotropy = Unit(v.X) }),                                           mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel SpecularIor           = new("specular_ior",                  group: ChannelGroup.Specular,     components: 1, transfer: PlaneTransfer.Raw,    neutral: Scalar(1.5),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.SpecularIor),          static (p, v) => p with { Ior = v.X }),                                                        mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel TransmissionWeight    = new("transmission_weight",           group: ChannelGroup.Transmission, components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.TransmissionWeight),   static (p, v) => p with { Transmission = Unit(v.X) }),                                         mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Seq("transmission", "transmissive", "refraction"));
    public static readonly TextureChannel TransmissionRoughness = new("transmission_roughness",        group: ChannelGroup.Transmission, components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.None,       mip: MipPolicy.RoughnessVariance, payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.TransmissionRoughness), static (p, v) => p with { TransmissionRoughness = Unit(v.X) }),                               mtlx: MtlxBinding.Unmapped,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel SubsurfaceWeight      = new("subsurface_weight",             group: ChannelGroup.Subsurface,   components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.SubsurfaceWeight),     static (p, v) => p with { Subsurface = Unit(v.X) }),                                           mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Seq("sss", "subsurface", "scatter"));
    public static readonly TextureChannel SubsurfaceRadius      = new("subsurface_radius",             group: ChannelGroup.Subsurface,   components: 3, transfer: PlaneTransfer.Raw,    neutral: Rgb(1.0, 0.5, 0.25),    unit: ChannelUnit.Millimetre, mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Band(s.SubsurfaceRadius),       static (p, v) => p with { SubsurfaceRadius = Rasm.Materials.Appearance.Graph.SubsurfaceRadius.Create(Math.Max(0.0, v.X), Math.Max(0.0, v.Y), Math.Max(0.0, v.Z)) }), mtlx: new MtlxBinding.Split("subsurface_radius_scale"), slot: None,             aliases: Empty);
    public static readonly TextureChannel CoatWeight            = new("coat_weight",                   group: ChannelGroup.Coat,         components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.CoatWeight),           static (p, v) => p with { Clearcoat = Unit(v.X) }),                                            mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Seq("clearcoat", "coat", "cc"));
    public static readonly TextureChannel CoatColor             = new("coat_color",                    group: ChannelGroup.Coat,         components: 3, transfer: PlaneTransfer.Srgb,   neutral: Rgb(1.0, 1.0, 1.0),     unit: ChannelUnit.None,       mip: MipPolicy.Kaiser,            payload: KtxPayload.Etc1s, origin: Column(static _ => Rgb(RgbSpectrum.White)),                                                                                                      mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel CoatRoughness         = new("coat_roughness",                group: ChannelGroup.Coat,         components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.None,       mip: MipPolicy.RoughnessVariance, payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.CoatRoughness),        static (p, v) => p with { ClearcoatRoughness = Unit(v.X) }),                                   mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel CoatIor               = new("coat_ior",                      group: ChannelGroup.Coat,         components: 1, transfer: PlaneTransfer.Raw,    neutral: Scalar(1.6),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.CoatIor)),                                                                                                           mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel FuzzWeight            = new("fuzz_weight",                   group: ChannelGroup.Fuzz,         components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.FuzzWeight),           static (p, v) => p with { Sheen = Unit(v.X) }),                                                mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Seq("sheen", "fuzz", "velvet"));
    public static readonly TextureChannel FuzzColor             = new("fuzz_color",                    group: ChannelGroup.Fuzz,         components: 3, transfer: PlaneTransfer.Srgb,   neutral: Rgb(1.0, 1.0, 1.0),     unit: ChannelUnit.None,       mip: MipPolicy.Kaiser,            payload: KtxPayload.Etc1s, origin: Column(static s => Rgb(s.FuzzColor),               static (p, v) => p with { SheenTint = Unit(v.Luminance) }),                                    mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel FuzzRoughness         = new("fuzz_roughness",                group: ChannelGroup.Fuzz,         components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.5),            unit: ChannelUnit.None,       mip: MipPolicy.RoughnessVariance, payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.FuzzRoughness)),                                                                                                     mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel ThinFilmWeight        = new("thin_film_weight",              group: ChannelGroup.ThinFilm,     components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.ThinFilmWeight),       static (p, v) => p with { Film = ThinFilm.Create(Unit(v.X), p.Film.ThicknessNm, p.Film.Ior) }), mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel ThinFilmThickness     = new("thin_film_thickness",           group: ChannelGroup.ThinFilm,     components: 1, transfer: PlaneTransfer.Raw,    neutral: Scalar(500.0),          unit: ChannelUnit.Nanometre,  mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.ThinFilmThickness),    static (p, v) => p with { Film = ThinFilm.Create(p.Film.Weight, Math.Max(0.0, v.X), p.Film.Ior) }), mtlx: new MtlxBinding.Scaled(1e-3),                 slot: None,                  aliases: Empty);
    public static readonly TextureChannel ThinFilmIor           = new("thin_film_ior",                 group: ChannelGroup.ThinFilm,     components: 1, transfer: PlaneTransfer.Raw,    neutral: Scalar(1.4),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.ThinFilmIor),          static (p, v) => p with { Film = ThinFilm.Create(p.Film.Weight, p.Film.ThicknessNm, Math.Max(1.0, v.X)) }), mtlx: MtlxBinding.Verbatim,                    slot: None,                  aliases: Empty);
    public static readonly TextureChannel EmissionColor         = new("emission_color",                group: ChannelGroup.Emission,     components: 3, transfer: PlaneTransfer.Srgb,   neutral: Rgb(1.0, 1.0, 1.0),     unit: ChannelUnit.None,       mip: MipPolicy.Kaiser,            payload: KtxPayload.Etc1s, origin: Column(static s => Rgb(s.EmissionColor),           static (p, v) => p with { Emission = v.AsColorUnchecked() }),                                  mtlx: MtlxBinding.Verbatim,                            slot: SinkSlot.Emission,     aliases: Seq("emissive", "emission", "glow", "e"));
    public static readonly TextureChannel EmissionLuminance     = new("emission_luminance",            group: ChannelGroup.Emission,     components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.Luminance,  mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.EmissionLuminance),    static (p, v) => p with { EmissionLuminance = Math.Max(0.0, v.X) }),                           mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);

    // --- [GEOMETRY_CHANNELS]
    // The OpenPBR geometry group. Read from the shade point's own frame rather than the lowered vector: the
    // opacity floor is full coverage, an unperturbed tangent-space normal is +Z, and a tangent is the frame's
    // own X axis, so a set carrying none of these still binds a geometrically correct graph.
    public static readonly TextureChannel GeometryOpacity     = new("geometry_opacity",      group: ChannelGroup.Geometry, components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(1.0),        unit: ChannelUnit.None, mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: new ChannelOrigin.Geometric(static _ => Scalar(1.0)),                              mtlx: MtlxBinding.Verbatim, slot: None,             aliases: Seq("opacity", "alpha", "mask", "transparency"));
    public static readonly TextureChannel GeometryNormal      = new("geometry_normal",       group: ChannelGroup.Geometry, components: 3, transfer: PlaneTransfer.Raw,    neutral: Rgb(0.0, 0.0, 1.0), unit: ChannelUnit.None, mip: MipPolicy.NormalRenormalize, payload: KtxPayload.Uastc, origin: new ChannelOrigin.Geometric(static _ => Rgb(0.0, 0.0, 1.0)),                       mtlx: MtlxBinding.Verbatim, slot: SinkSlot.Normal,  aliases: Seq("normal", "nor", "nrm", "n", "normalgl", "norgl", "nordx", "normaldx"));
    public static readonly TextureChannel GeometryCoatNormal  = new("geometry_coat_normal",  group: ChannelGroup.Geometry, components: 3, transfer: PlaneTransfer.Raw,    neutral: Rgb(0.0, 0.0, 1.0), unit: ChannelUnit.None, mip: MipPolicy.NormalRenormalize, payload: KtxPayload.Uastc, origin: new ChannelOrigin.Geometric(static _ => Rgb(0.0, 0.0, 1.0)),                       mtlx: MtlxBinding.Verbatim, slot: None,             aliases: Empty);
    public static readonly TextureChannel GeometryTangent     = new("geometry_tangent",      group: ChannelGroup.Geometry, components: 3, transfer: PlaneTransfer.Raw,    neutral: Rgb(1.0, 0.0, 0.0), unit: ChannelUnit.None, mip: MipPolicy.NormalRenormalize, payload: KtxPayload.Uastc, origin: new ChannelOrigin.Geometric(static p => Axis(p.Frame.XAxis)),                mtlx: MtlxBinding.Verbatim, slot: None,             aliases: Empty);
    public static readonly TextureChannel GeometryCoatTangent = new("geometry_coat_tangent", group: ChannelGroup.Geometry, components: 3, transfer: PlaneTransfer.Raw,    neutral: Rgb(1.0, 0.0, 0.0), unit: ChannelUnit.None, mip: MipPolicy.NormalRenormalize, payload: KtxPayload.Uastc, origin: new ChannelOrigin.Geometric(static p => Axis(p.Frame.XAxis)),                mtlx: MtlxBinding.Verbatim, slot: None,             aliases: Empty);

    // --- [DERIVED_CHANNELS]
    // No OpenPBR input; each carries BOTH the sibling channel it folds from and the filter#PLANE_OP step that
    // folds it, so the press reads the derivation off the roster. height inverts the height-normal
    // correspondence over the spectral (periodic) solver — the tileable-source route a bake target always is —
    // and consumes the HeightEvidence the forward direction records. height is normalized [0,1] and its
    // millimetre span rides TextureSet.HeightScaleMm, never the plane; curvature is signed [-1,1].
    public static readonly TextureChannel Height    = new("height",    group: ChannelGroup.Derived, components: 1, transfer: PlaneTransfer.Raw,    neutral: Scalar(0.5), unit: ChannelUnit.None, mip: MipPolicy.Box, payload: KtxPayload.Uastc, origin: new ChannelOrigin.Derived("geometry_normal", new PlaneOp.HeightNormal(Inverse: true, HeightEvidence.Unit, HeightSolver.Spectral)), mtlx: MtlxBinding.Unmapped, slot: None, aliases: Seq("height", "disp", "displacement", "bump", "h"));
    public static readonly TextureChannel Occlusion = new("occlusion", group: ChannelGroup.Derived, components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(1.0), unit: ChannelUnit.None, mip: MipPolicy.Box, payload: KtxPayload.Uastc, origin: new ChannelOrigin.Derived("height",          new PlaneOp.FromHeight(new HeightDerivative.Occlusion(), HeightEvidence.Unit)),                mtlx: MtlxBinding.Unmapped, slot: None, aliases: Seq("ao", "occlusion", "ambientocclusion"));
    public static readonly TextureChannel Curvature = new("curvature", group: ChannelGroup.Derived, components: 1, transfer: PlaneTransfer.Raw,    neutral: Scalar(0.0), unit: ChannelUnit.None, mip: MipPolicy.Box, payload: KtxPayload.Uastc, origin: new ChannelOrigin.Derived("height",          new PlaneOp.FromHeight(new HeightDerivative.Curvature(CurvatureMeasure.Mean), HeightEvidence.Unit)), mtlx: MtlxBinding.Unmapped, slot: None, aliases: Seq("curv", "curvature"));

    public ChannelGroup Group { get; }
    public int Components { get; }
    public PlaneTransfer Transfer { get; }
    public ShadeVec4 Neutral { get; }
    public ChannelUnit Unit { get; }
    public MipPolicy Mip { get; }
    public KtxPayload Payload { get; }
    public ChannelOrigin Origin { get; }
    public MtlxBinding Mtlx { get; }
    public Option<SinkSlot> Slot { get; }
    public Seq<string> Aliases { get; }

    private TextureChannel(
        string key, ChannelGroup group, int components, PlaneTransfer transfer, ShadeVec4 neutral, ChannelUnit unit,
        MipPolicy mip, KtxPayload payload, ChannelOrigin origin, MtlxBinding mtlx, Option<SinkSlot> slot, Seq<string> aliases)
        : this(key) =>
        (Group, Components, Transfer, Neutral, Unit, Mip, Payload, Origin, Mtlx, Slot, Aliases) =
        (group, components, transfer, neutral, unit, mip, payload, origin, mtlx, slot, aliases);

    // The declaration-order rank the set key preimage and the press binding order both sort on. Items is
    // IReadOnlyList<T>, which carries no IndexOf, so the rank is one lazily-derived index and never a per-call
    // linear scan or a hand-numbered column that drifts from the declaration list.
    public int Ordinal => Ranks.Value[this];

    // The paired channel MipPolicy.RoughnessVariance consumes: a roughness fold absorbs the per-level variance
    // its own normal lost, and the coat group's normal is the coat normal. Derived from Group so a new roughness
    // row inherits the pairing rather than restating it, and every other policy pairs with nothing.
    public Option<string> Pair =>
        Mip == MipPolicy.RoughnessVariance
            ? Some(Group == ChannelGroup.Coat ? GeometryCoatNormal.Key : GeometryNormal.Key)
            : Option<string>.None;

    // The .mtlx input name the interchange#MATERIALX_DOCUMENT OpenPbrPorts fold binds. Canonical/Scaled/Split
    // all spell the channel key; Lowered names the input this column folds INTO; Absent is the typed refusal
    // an .mtlx projection reads as "this column does not cross".
    public Option<string> MtlxInput =>
        Mtlx.Switch(
            state:     Key,
            canonical: static (key, _) => Some(key),
            scaled:    static (key, _) => Some(key),
            split:     static (key, _) => Some(key),
            lowered:   static (_, l) => Some(l.Input),
            absent:    static (_, _) => Option<string>.None);

    // Every derived index reads Items through a Lazy accessor. An eager static readonly field initializer inside
    // this type runs during this type's own class construction — before the generated Items materialization has
    // published — so it captures an empty roster and poisons every consumer that later reads it.
    private static readonly Lazy<FrozenDictionary<TextureChannel, int>> Ranks =
        new(static () => Items.Select(static (row, index) => (Row: row, Index: index)).ToFrozenDictionary(static e => e.Row, static e => e.Index));

    private static readonly Lazy<FrozenDictionary<SinkSlot, TextureChannel>> Slots =
        new(static () => Items.Choose(static c => c.Slot.Map(slot => (Slot: slot, Channel: c))).ToFrozenDictionary(static e => e.Slot, static e => e.Channel));

    private static readonly Lazy<ILookup<ChannelGroup, TextureChannel>> Groups =
        new(static () => Items.ToLookup(static row => row.Group));

    // Total over SinkSlot.Items by construction — a slot no row claims would leave the graph binder guessing.
    public static TextureChannel BySlot(SinkSlot slot) => Slots.Value[slot];
    public static Seq<TextureChannel> ByGroup(ChannelGroup group) => toSeq(Groups.Value[group]);

    static readonly Seq<string> Empty = Seq<string>();
    static ShadeVec4 Scalar(double v) => new(v, 0.0, 0.0, 1.0);
    static ShadeVec4 Rgb(double r, double g, double b) => new(r, g, b, 1.0);
    static ShadeVec4 Rgb(RgbSpectrum c) => new(c.R, c.G, c.B, 1.0);
    static ShadeVec4 Band(SubsurfaceRadius r) => new(r.R, r.G, r.B, 1.0);
    static ShadeVec4 Axis(Vector3d v) => new(v.X, v.Y, v.Z, 1.0);
    static double Unit(double v) => Math.Clamp(v, 0.0, 1.0);
    static ChannelOrigin Column(Func<OpenPbrSurface, ShadeVec4> read, Func<MaterialParameters, ShadeVec4, MaterialParameters>? write = null) =>
        new ChannelOrigin.Shaded(new ColumnLens(read, Optional(write)));
}

// The two packing orders, each row naming its three slots in R,G,B order. A packed plane is ALWAYS raw
// transfer with AlphaMode.None — the alpha component carries nothing and is never repurposed — and mips PER
// COMPONENT under each slot's own MipPolicy, so one policy across the pack is the defect. Only orm crosses to
// a glTF consumer: it IS the KHR occlusion + metallic-roughness read order.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChannelPack {
    public static readonly ChannelPack Orm = new("orm", slots: Seq(TextureChannel.Occlusion, TextureChannel.SpecularRoughness, TextureChannel.BaseMetalness), gltfLegal: true);
    public static readonly ChannelPack Mra = new("mra", slots: Seq(TextureChannel.BaseMetalness, TextureChannel.SpecularRoughness, TextureChannel.Occlusion), gltfLegal: false);
    public Seq<TextureChannel> Slots { get; }
    public bool GltfLegal { get; }

    // The slot a channel occupies, or absence — the ONE lane correspondence the packed read and the packed
    // neutral fill both consume, so no consumer re-scans the slot sequence by index. The indexed Choose is
    // the catalogued one-pass filter-map; the v4 `Prelude.Range` free function no longer exists.
    public Option<int> Lane(TextureChannel channel) =>
        Slots.Choose((index, slot) => slot == channel ? Some(index) : Option<int>.None).HeadOrNone();
}
```

## [03]-[TEXTURE_SET]

- Owner: `TextureSet` the extent-coherent content-keyed plane bundle; `UdimSheet` the ascending per-tile assembly the wire's `udimTiles` leg publishes; `LayerLaw` `[SmartEnum<string>]` the layer-cardinality axis; `UdimTile` `[ValueObject<int>]` the Mari tile index; `ChannelPackPlane` the packed-plane carrier over a `ChannelPack` row; `EgressSlot` the ad-hoc `[Union<TextureChannel, ChannelPack>]` name absorber; `EgressVariant` `[Union]` the one optional filename infix.
- Cases: layer-law {`none` (exactly one layer), `cubeFaces` (exactly six, square extent), `array`, `volume`, `frames`} · egress-variant {`Whole`, `Udim`, `Mip`, `Layer`}.
- Entry: `public static Fin<TextureSet> Of(TextureSetDraft draft, Op key)` is the ONE admission — a draft carries the raw bundle, `Of` runs the gate ladder and mints the content key, and no other construction path exists; `Egress(EgressSlot slot, EgressVariant variant, RasterFormat format, Op key)` renders the one egress leaf name for a channel and a pack alike over an ad-hoc union that absorbs both call shapes, validating the variant against the set's own declared state; `WithChannel`/`WithPack` re-admit through `Of` so a mutated set re-keys; `Digest` exposes the `ContentAddress` the interchange payload and the object store both address; `UdimSheet.Of(tiles, key)` assembles N single-tile sets into the one UDIM producer, proving tile uniqueness, vocabulary agreement, and roster agreement before the sheet keys over the ascending tile-key fold. The wire mirror for both — `TextureSetWire` and its per-tile channel-row repetition — is `Appearance/interchange#TEXTURE_EGRESS`, the page the freeze's producer column reaches through.
- Law: `Tiled` is EVIDENCE, never a flag — the column is `Option<TileProof>` and `tile#TILE_GATE` is the only surface that mints one, so a caller cannot assert tileability into a draft and an ingested set carries `None` until the gate grades it. The wire's boolean `tiled` is the projection of that presence, never its source.
- Packages: `plane#TEXTURE_PLANE` (composed — `TexturePyramid` carrying each channel's levels and its own `Key`, `PlaneDepth` the alpha-conversion floor), `codec#RASTER_CODEC` (composed — `RasterFormat.Extension` the ONE `<ext>` source, `KtxPayload.WireLegal` the payload gate), `tile#TILE_GATE` (composed — `TileProof`), `Rasm.Element.Projection` (composed — `ContentAddress.Of`/`ToValue`, the ONE X32 spelling and its ONE lowering site), `Rasm.Element.Composition` (the SEAM `MaterialId`), `Rasm.Materials.Appearance.Surface` (`ConductorMetal` the set-level conductor row), `Rasm.Domain` (`ContentHash.Of` the ONE identity entry, `ValidityClaim`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox (`Encoding.UTF8` the total preimage projection).
- Growth: a new layer modality is one `LayerLaw` row carrying its cardinality and extent predicates — cube maps, flipbooks, arrays, and volumes are rows, so a set shape never breaks for a new stacking; a new set-level fact is one `TextureSet` column and one `Of` gate; a new filename infix is one `EgressVariant` case, and a new container is one `codec#RASTER_CODEC` `RasterFormat` row the egress reads its extension off.
- Boundary: `TextureSet.Of` is the one gate and it REFUSES rather than repairs — a channel plane whose extent differs from the set's, a `pq` or `hlg` transfer on a channel plane (a bake target is scene-referred, and a display-referred bake forks the shading value from the stored value), a layer count or extent its `LayerLaw` row rejects, a set-level `AlphaMode` a channel's own `PlaneDepth` cannot convert to without catastrophic low-alpha quantization, a channel appearing both standalone and inside a pack, a duplicate pack row, a pack plane that is not four-component, `raw`, and `AlphaMode.None`, a `height` scale with no `height` channel to scale, two of the three declared variant axes occupied at once, or an empty channel map each rail `MaterialFault.Parameter` with the offending channel key in the reason. `Convention` is the INGEST-SOURCE PROVENANCE the wire's `normalConvention` field records — the planes are always `gl` by construction of the two mints (the press bakes `gl` natively, the `SetIngest.Draft` lift converts a `dx` source ONCE before any plane is keyed) — so a `dx` value here names where the bytes CAME from, never what they carry. The band split is by CONCERN: appearance-domain admission — a parameter out of range, a colour out of gamut, a graph that will not compile — rails band-2450 `MaterialFault`, while the raster-mechanical failures rail band-2460 `codec#RASTER_FAULT` `RasterFault` (`Decode`/`Encode` at the container, `Device` at the bake device, `Tile` at the synthesizer), so a set admission fault is never a codec fault wearing a raster code. The content key is a `ContentHash.Of` fold at seed zero over the canonically-ordered plane digests — channels by `TextureChannel.Ordinal` and packs by `ChannelPack.Items` row order, never map-enumeration or authoring order — so the same bundle in any authoring order keys identically and the preimage is stable against a roster APPEND; every preimage piece appends as a whole UTF-8 string, because a fixed buffer that could truncate would vanish an entry and fork the address silently; the key never reads the extent, the material id, the tile proof, or the provenance, so a re-encode of identical planes re-keys identically and a plane edit re-keys the set. `HeightScaleMm` is the set-level millimetre span the normalized `height` plane resolves against — the plane stays `[0,1]` so a set rescaled for a different displacement amplitude re-keys only its scale column, never every texel. An ATLAS is a plane-level sharing fact — N sets referencing one plane blob by content address — and never a set-level merge behind one appearance key: two materials sharing a packed sheet each carry their own `TextureSet` whose channel rows address the same digest, so a texture edit re-keys exactly the sets that read it. The egress grammar is `materials/texture/<key>/<channel>[.<variant>].<ext>` with `<key>` the set key LOWERED ONCE through `ContentAddress.Of(Key).ToValue().ToLowerInvariant()` at name construction — never at the wire and never at admission, so the uppercase X32 wire value and the lowercase path segment are one value under one documented lowering, and a consumer joining a wire key to a path lowers the key rather than uppercasing the path; the variant slot admits AT MOST ONE of a four-digit UDIM tile, a two-digit mip index, or a two-digit layer index, and a `ktx2` leaf refuses a mip variant outright because the container holds its own pyramid.

```csharp signature
// (Continues the Rasm.Materials.Raster compilation unit — the [02] prelude is in scope, plus:)
using System.Globalization;                       // CultureInfo (the invariant key and variant projection)
using System.Text;                                // Encoding.UTF8 (the TOTAL preimage projection — a piece appends whole or the string ctor fails loud)

// --- [TYPES] -------------------------------------------------------------------------------
// Admits is the cardinality predicate and Square the extent one; a cube face is the only row that constrains
// both, and a new stacking answers both columns rather than adding a branch to the gate.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LayerLaw {
    public static readonly LayerLaw None      = new("none",      admits: static n => n is 1, square: false);
    public static readonly LayerLaw CubeFaces = new("cubeFaces", admits: static n => n is 6, square: true);
    public static readonly LayerLaw Array     = new("array",     admits: static n => n >= 1, square: false);
    public static readonly LayerLaw Volume    = new("volume",    admits: static n => n >= 1, square: false);
    public static readonly LayerLaw Frames    = new("frames",    admits: static n => n >= 1, square: false);

    public bool Square { get; }

    [UseDelegateFromConstructor]
    public partial bool Admits(int layers);
}

// The Mari UDIM index: 1001 + (row-1)*10 + (column-1), columns 1..10, rows 1..100 — so the admitted band is
// exactly [1001, 2000] and the derived Column/Row are total over it. Column/Row are DERIVED, never stored, so a
// tile and its grid coordinate cannot disagree.
[ValueObject<int>]
public readonly partial struct UdimTile {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) {
        if (value is < 1001 or > 2000)
            validationError = new ValidationError($"<udim-out-of-mari-range:{value}>");
    }

    public static UdimTile Of(int value) => Create(value);
    // The non-throwing admission the PURE classify fold reads: a four-digit token past the Mari band is an
    // ordinary unresolved stem, never an exception inside a total fold.
    public static Option<UdimTile> Admit(int value) =>
        Validate(value, null, out UdimTile tile) is null ? Some(tile) : Option<UdimTile>.None;
    public int Column => (Value - 1001) % 10 + 1;
    public int Row => (Value - 1001) / 10 + 1;
}

// The egress name's subject: a standalone channel or a packed sheet. The ad-hoc union is a PARAMETER ABSORBER
// — one Egress entry binds both call shapes through the implicit conversions — so no direction-named overload
// pair exists and a third subject is a slot on this union rather than a third method.
[Union<TextureChannel, ChannelPack>(T1Name = "Channel", T2Name = "Pack")]
public readonly partial struct EgressSlot {
    public string Name => Switch(channel: static c => c.Key, pack: static p => p.Key);
}

// The ONE optional filename infix. A set occupies at most one of the three, which is what makes the grammar
// unambiguous: a leaf carrying both a UDIM tile and a mip index has no reader that can order the two.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EgressVariant {
    private EgressVariant() { }
    public sealed record Whole() : EgressVariant;
    public sealed record Udim(UdimTile Tile) : EgressVariant;
    public sealed record Mip(int Level) : EgressVariant;
    public sealed record Layer(int Index) : EgressVariant;

    public static readonly EgressVariant Single = new Whole();

    // Four-digit Mari, two-digit zero-padded mip and layer — the frozen widths, spelled once.
    public string Infix => Switch(
        whole: static _ => string.Empty,
        udim:  static u => string.Create(CultureInfo.InvariantCulture, $".{u.Tile.Value:D4}"),
        mip:   static m => string.Create(CultureInfo.InvariantCulture, $".{m.Level:D2}"),
        layer: static l => string.Create(CultureInfo.InvariantCulture, $".{l.Index:D2}"));
}

// --- [MODELS] ------------------------------------------------------------------------------
// A packed plane names the channels genuinely PRESENT rather than three positional flags: the wire's
// slot-ordered bool triple derives as Pack.Slots.Map(Present.Contains), so the flags cannot drift from the
// channels. The absent-slot NEUTRAL FILL lives at the pack BUILDER (press#TEXTURE_PRESS composes each lane
// from its slot channel's own plane or its neutral) — a fill helper here that zeroed present lanes was a
// dead surface no builder could consume, and it is deleted rather than repaired into a second builder.
public sealed record ChannelPackPlane(ChannelPack Pack, TexturePyramid Plane, Seq<TextureChannel> Present) {
    public Seq<bool> Flags => Pack.Slots.Map(Present.Contains);
}

// The raw bundle Of admits. A draft is never shaded, never keyed, and never crosses a boundary — it exists so
// the admitted TextureSet has no unkeyed construction path and no partially-built state. Tiled is absent on a
// draft by construction: only tile#TILE_GATE mints the proof a set carries.
public sealed record TextureSetDraft(
    Dimension Width, Dimension Height, Dimension Layers, LayerLaw Law,
    NormalConvention Convention, AlphaMode Alpha, double HeightScaleMm, Option<TileProof> Tiled,
    Seq<UdimTile> Udim, HashMap<TextureChannel, TexturePyramid> Channels, Seq<ChannelPackPlane> Packs,
    Option<ConductorMetal> Conductor, Option<MaterialId> Material);

public sealed record TextureSet(
    Dimension Width, Dimension Height, Dimension Layers, LayerLaw Law,
    NormalConvention Convention, AlphaMode Alpha, double HeightScaleMm, Option<TileProof> Tiled,
    Seq<UdimTile> Udim, HashMap<TextureChannel, TexturePyramid> Channels, Seq<ChannelPackPlane> Packs,
    Option<ConductorMetal> Conductor, Option<MaterialId> Material, UInt128 Key) : IValidityEvidence {

    // The ONE admission. Every gate names the offending channel so a refusal is actionable without a second
    // probe, and the key mints LAST so a refused draft never leaves a half-keyed value behind. Convention is
    // PROVENANCE — the ingest-source record the wire's normalConvention field exists for — and the PLANES are
    // always gl by construction of the two mints (the press bakes gl natively, the ingest lift converts once),
    // so no gate here re-litigates a fact no gate can see in the texels.
    public static Fin<TextureSet> Of(TextureSetDraft draft, Op key) =>
        from _ in guard(!draft.Channels.IsEmpty || !draft.Packs.IsEmpty, MaterialFault.Parameter(key, "<texture-set-empty>"))
        from __ in guard(draft.Law.Admits(draft.Layers.Value), MaterialFault.Parameter(key, $"<layer-law-rejects:{draft.Law.Key}:{draft.Layers.Value}>"))
        from ___ in guard(!draft.Law.Square || draft.Width == draft.Height, MaterialFault.Parameter(key, $"<layer-law-needs-square:{draft.Law.Key}:{draft.Width.Value}x{draft.Height.Value}>"))
        // The frozen variant law: at most ONE of {UDIM tile, mip index, layer index} may occupy the filename
        // infix, and the three axes are DECLARED facts of the draft — tiles present, any multi-level pyramid,
        // layers above one — so a two-axis set refuses here rather than at the egress that cannot name it.
        from ____ in guard(
            ((draft.Udim.IsEmpty ? 0 : 1)
                + (draft.Layers.Value is 1 ? 0 : 1)
                + (draft.Channels.Values.Exists(static p => p.Levels.Count > 1) || draft.Packs.Exists(static p => p.Plane.Levels.Count > 1) ? 1 : 0)) <= 1,
            MaterialFault.Parameter(key, "<variant-slot-double-occupied>"))
        from _____ in guard(draft.Packs.Map(static p => p.Pack).Distinct().Count() == draft.Packs.Count, MaterialFault.Parameter(key, "<pack-duplicate-row>"))
        from ______ in guard(double.IsFinite(draft.HeightScaleMm) && draft.HeightScaleMm >= 0.0, MaterialFault.Parameter(key, $"<height-scale-invalid:{draft.HeightScaleMm:R}>"))
        from _______ in guard(draft.HeightScaleMm is 0.0 || draft.Channels.ContainsKey(TextureChannel.Height), MaterialFault.Parameter(key, "<height-scale-without-height-channel>"))
        from ________ in draft.Channels.Fold(Fin.Succ(unit), (acc, pair) => acc.Bind(_ => AdmitChannel(draft, pair.Key, pair.Value, key)))
        from _________ in draft.Packs.Fold(Fin.Succ(unit), (acc, pack) => acc.Bind(_ => AdmitPack(draft, pack, key)))
        select new TextureSet(draft.Width, draft.Height, draft.Layers, draft.Law, draft.Convention, draft.Alpha,
            draft.HeightScaleMm, draft.Tiled, toSeq(draft.Udim.OrderBy(static t => t.Value)), draft.Channels,
            draft.Packs, draft.Conductor, draft.Material, Mint(draft));

    // Convertible is the plane owner's own crossing predicate: a PREMULTIPLYING crossing at or below u8
    // multiplies away low-alpha colour precision and refuses there, while a None-sourced or no-premultiply
    // crossing is legal at every depth — the set-level declaration admits exactly what the row fact admits.
    static Fin<Unit> AdmitChannel(TextureSetDraft draft, TextureChannel channel, TexturePyramid pyramid, Op key) =>
        from _ in guard(pyramid.Base.Width == draft.Width && pyramid.Base.Height == draft.Height, MaterialFault.Parameter(key, $"<channel-extent-mismatch:{channel.Key}>"))
        from __ in guard(pyramid.Base.Layers == draft.Layers, MaterialFault.Parameter(key, $"<channel-layer-mismatch:{channel.Key}>"))
        from ___ in guard(pyramid.Base.Transfer.SceneReferred, MaterialFault.Parameter(key, $"<display-referred-channel-plane:{channel.Key}:{pyramid.Base.Transfer.Key}>"))
        from ____ in guard(pyramid.Base.Format.Components >= channel.Components, MaterialFault.Parameter(key, $"<channel-components-narrow:{channel.Key}>"))
        from _____ in guard(pyramid.Base.Alpha.Convertible(draft.Alpha, pyramid.Base.Format.Depth), MaterialFault.Parameter(key, $"<alpha-crossing-quantizes:{channel.Key}:{pyramid.Base.Format.Key}>"))
        // LIVE under the widened codec KtxPayload roster: the roster rows carry only wire-legal classes, but
        // rawBcn and astc exist as branch-local desktop payloads a future row or override could seat — this
        // gate is what keeps either off the wire per the freeze [03.7] wire-legality law.
        from ______ in guard(channel.Payload.WireLegal, MaterialFault.Parameter(key, $"<channel-payload-not-wire-legal:{channel.Key}:{channel.Payload.Key}>"))
        from _______ in guard(!draft.Packs.Exists(p => p.Present.Contains(channel)), MaterialFault.Parameter(key, $"<channel-both-packed-and-standalone:{channel.Key}>"))
        select unit;

    static Fin<Unit> AdmitPack(TextureSetDraft draft, ChannelPackPlane pack, Op key) =>
        from _ in guard(pack.Plane.Base.Width == draft.Width && pack.Plane.Base.Height == draft.Height, MaterialFault.Parameter(key, $"<pack-extent-mismatch:{pack.Pack.Key}>"))
        from __ in guard(pack.Plane.Base.Format.Components is 4, MaterialFault.Parameter(key, $"<pack-plane-not-four-component:{pack.Pack.Key}>"))
        from ___ in guard(pack.Plane.Base.Transfer == PlaneTransfer.Raw, MaterialFault.Parameter(key, $"<pack-plane-not-raw:{pack.Pack.Key}>"))
        from ____ in guard(pack.Plane.Base.Alpha == AlphaMode.None, MaterialFault.Parameter(key, $"<pack-plane-carries-alpha:{pack.Pack.Key}>"))
        from _____ in guard(!pack.Present.IsEmpty, MaterialFault.Parameter(key, $"<pack-plane-no-present-slot:{pack.Pack.Key}>"))
        from ______ in guard(pack.Present.ForAll(pack.Pack.Slots.Contains), MaterialFault.Parameter(key, $"<pack-slot-foreign-channel:{pack.Pack.Key}>"))
        select unit;

    // The preimage through the ONE kernel identity entry: CANONICAL order end to end — channels by roster
    // ordinal, packs by ChannelPack.Items row order, never a draft's own authoring sequence on either half —
    // so the same bundle in any authoring order keys identically, and the digest-only preimage is what makes
    // a re-encode of identical planes key identically. Each entry projects as ONE piece string whose UTF-8
    // bytes append whole: a piece is TOTAL by construction where a fixed stack buffer under TryWrite could
    // truncate silently and vanish an entry from the preimage — a silent address fork no diagnostic names —
    // and the per-admission allocation is the price of a preimage that cannot lie. Plane keys spell X32
    // per the freeze [03.9] carve: byte-identical to ContentAddress.ToValue(), never an interpolated
    // lowercase lowering.
    static UInt128 Mint(TextureSetDraft draft) =>
        ContentHash.Of(draft, static (source, digest) => {
            foreach (TextureChannel channel in TextureChannel.Items) {
                source.Channels.Find(channel).Iter(pyramid =>
                    digest.Append(Encoding.UTF8.GetBytes(string.Create(CultureInfo.InvariantCulture, $"{channel.Key}|{pyramid.Key.ToString("X32")}"))));
            }
            foreach (ChannelPack row in ChannelPack.Items) {
                source.Packs.Find(pack => pack.Pack == row).Iter(pack =>
                    digest.Append(Encoding.UTF8.GetBytes(string.Create(CultureInfo.InvariantCulture, $"{pack.Pack.Key}|{pack.Plane.Key.ToString("X32")}"))));
            }
        });

    public ContentAddress Digest => ContentAddress.Of(Key);
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(Channels.Count + Packs.Count, 1),
        ValidityClaim.Nonnegative(HeightScaleMm),
        ValidityClaim.Of(Law.Admits(Layers.Value)),
        ValidityClaim.Evidence(Tiled));

    // The ONE egress entry over BOTH name subjects and ALL variants: the wire carries ToValue() verbatim in
    // uppercase X32, the path segment carries its lowercase form, and a consumer joining the two lowers the key
    // — uppercasing a path segment to match a wire value is the deleted direction. The variant VALIDATES
    // against the set's own declared state — a Udim tile must be one of the set's tiles, a mip index needs a
    // real pyramid and a container that does not hold its own (ktx2 does), a layer index needs layers above
    // one — so a caller cannot name a file the set does not contain.
    public Fin<string> Egress(EgressSlot slot, EgressVariant variant, RasterFormat format, Op key) =>
        from _ in variant.Switch(
            whole: _ => Fin.Succ(unit),
            udim:  u => guard(Udim.Exists(tile => tile == u.Tile), MaterialFault.Parameter(key, $"<egress-udim-foreign-tile:{slot.Name}:{u.Tile.Value}>")),
            mip:   m => format == RasterFormat.Ktx2
                ? Fin.Fail<Unit>(MaterialFault.Parameter(key, $"<ktx2-leaf-carries-own-pyramid:{slot.Name}>"))
                : guard(Channels.Values.Exists(static p => p.Levels.Count > 1) || Packs.Exists(static p => p.Plane.Levels.Count > 1), MaterialFault.Parameter(key, $"<egress-mip-on-flat-set:{slot.Name}:{m.Level}>")),
            layer: l => guard(Layers.Value > 1 && l.Index < Layers.Value, MaterialFault.Parameter(key, $"<egress-layer-out-of-range:{slot.Name}:{l.Index}>")))
        select $"materials/texture/{Digest.ToValue().ToLowerInvariant()}/{slot.Name}{variant.Infix}.{format.Extension}";

    public Fin<TextureSet> WithChannel(TextureChannel channel, TexturePyramid pyramid, Op key) =>
        Of(new TextureSetDraft(Width, Height, Layers, Law, Convention, Alpha, HeightScaleMm, Tiled, Udim,
            Channels.AddOrUpdate(channel, pyramid), Packs, Conductor, Material), key);

    public Fin<TextureSet> WithPack(ChannelPackPlane pack, Op key) =>
        Of(new TextureSetDraft(Width, Height, Layers, Law, Convention, Alpha, HeightScaleMm, Tiled, Udim,
            Channels.Filter((c, _) => !pack.Present.Contains(c)), Packs.Add(pack), Conductor, Material), key);
}

// THE UDIM ASSEMBLY — the producer the frozen wire's udimTiles leg and the [09] `<channel>.<tile>.<ext>`
// grammar were missing. A UDIM surface is N per-tile presses sharing one plan key (each tile an independent
// extent whose planes address independently), and the sheet is the one owner proving the tiles AGREE:
// ascending unique Mari indices, no per-tile variant occupancy, one convention/alpha/layer-law vocabulary,
// and one channel roster — a sheet whose tiles disagree is two materials wearing one name. The sheet key
// folds the per-tile set keys in ascending-tile order, so tile membership and tile content both re-key it.
// The wire projection repeats each channel row per tile under the Udim variant leaf at the mirror owner,
// Appearance/interchange#TEXTURE_EGRESS.
public sealed record UdimSheet(Seq<(UdimTile Tile, TextureSet Set)> Tiles, UInt128 Key) {
    public static Fin<UdimSheet> Of(Seq<(UdimTile Tile, TextureSet Set)> tiles, Op key) =>
        from _ in guard(!tiles.IsEmpty, MaterialFault.Parameter(key, "<udim-sheet-empty>"))
        from __ in guard(tiles.Map(static t => t.Tile.Value).Distinct().Count() == tiles.Count, MaterialFault.Parameter(key, "<udim-sheet-duplicate-tile>"))
        from ___ in guard(tiles.ForAll(static t => t.Set.Udim.IsEmpty && t.Set.Layers.Value is 1), MaterialFault.Parameter(key, "<udim-sheet-tile-carries-variant>"))
        from ____ in guard(tiles.Map(static t => (t.Set.Convention, t.Set.Alpha, t.Set.Law)).Distinct().Count() is 1, MaterialFault.Parameter(key, "<udim-sheet-vocabulary-divergent>"))
        from _____ in guard(tiles.Map(Roster).Distinct().Count() is 1, MaterialFault.Parameter(key, "<udim-sheet-roster-divergent>"))
        let ordered = toSeq(tiles.OrderBy(static t => t.Tile.Value))
        select new UdimSheet(ordered, Mint(ordered));

    public Seq<UdimTile> Indices => Tiles.Map(static t => t.Tile);
    public ContentAddress Digest => ContentAddress.Of(Key);

    static string Roster((UdimTile Tile, TextureSet Set) tile) =>
        string.Join('|', TextureChannel.Items.Where(c => tile.Set.Channels.ContainsKey(c)).Select(static c => c.Key)
            .Concat(ChannelPack.Items.Where(row => tile.Set.Packs.Exists(p => p.Pack == row)).Select(static row => row.Key)));

    static UInt128 Mint(Seq<(UdimTile Tile, TextureSet Set)> ordered) =>
        ContentHash.Of(ordered, static (tiles, digest) => tiles.Iter(tile =>
            digest.Append(Encoding.UTF8.GetBytes(string.Create(CultureInfo.InvariantCulture, $"{tile.Tile.Value:D4}|{tile.Set.Key.ToString("X32")}")))));
}
```

## [04]-[SET_INGEST]

- Owner: `SetIngest` the classification fold, the python-wire decode seam, AND the draft lift; `PlaneProbe` the per-file evidence row; `IngestSource` `[Union]` the classification input (directory probes · C#-side manifest · decoded python `AssetSetManifest`); `ClassifiedMap` the resolved row; `SetManifest` the accumulating result and its monoid.
- Cases: ingest-source {`Stems` (a scanned directory's probes), `Declared` (a C#-side manifest re-admitted through the same alias law), `Peer` (the decoded python-minted `AssetSetManifest` — the wire-freeze `[05]` classification-input consumer arm)}.
- Entry: `public static SetManifest Classify(IngestSource source)` is TOTAL and PURE — it never reads a file, never faults, and never infers; every unclaimed stem accumulates into `Unresolved`, and the caller decides whether an incomplete manifest is admissible for its purpose. `public static Fin<IngestSource> Peer(ReadOnlyMemory<byte> wire, Op key)` is the ONE python-manifest decode seam — it parses the `rasm/channels.proto` `AssetSetManifest` bytes through the generated `Parser`, refuses a non-`pbr_set` kind (an `hdri`/`ibl` manifest is product-borne per the freeze `[02.4]` shadowing law and is never a classification input), and lifts the message into the `Peer` case `Classify` folds. `public static Fin<Seq<(Option<UdimTile> Tile, TextureSetDraft Draft)>> Draft(SetManifest manifest, HashMap<string, TexturePyramid> planes, IngestIntent intent, Op key)` is the ONE ingest lift — it groups the classified maps per tile, runs the `dx`→`gl` conversion and the gloss inversion EXACTLY ONCE before any plane is keyed, records the source convention as draft provenance, and hands per-tile drafts to `TextureSet.Of` (one tile) or `UdimSheet.Of` (a tile family).
- Packages: LanguageExt.Core (`Seq`/`Option`/`Fold` and the `SetManifest` monoid `Combine`), Google.Protobuf (composed — the generated `Rasm.Channels.AssetSetManifest` `Parser`, the wire-freeze `[05]` document's one C#-side decode; `rasm/channels.proto` pins `option csharp_namespace = "Rasm.Channels"`), `filter#PLANE_OP` (composed — `PlaneOp.Remap` over `RemapCurve.Levels.Invert`, the gloss arithmetic this page flags and never re-derives), `plane#TEXTURE_PLANE` (composed — the `ReadShade`/`WriteShade` row rail the convention flip rides), CommunityToolkit.HighPerformance (`SpanOwner<T>` the flip's row scratch), BCL inbox (`FrozenDictionary`/`FrozenSet` behind `Lazy<T>` accessors, `StringComparer.OrdinalIgnoreCase`, `Path` the leaf-stem projection).
- Growth: a new alias is one entry on its channel's `Aliases` column — the resolver index DERIVES from `TextureChannel.Items`, so no second table exists to drift; a new packing token is one `ChannelPack` row; a new variant grammar is one token predicate in `Tokenize`; a new ingest-time repair is one arm in the lift's per-map conversion, never a second lift.
- Boundary: classification is ALIAS-DRIVEN and the probe is EVIDENCE, never an inference source. A stem resolves by its tokens: separators `-`, `_`, `.`, and space all fold to one boundary, matching is case-insensitive, and the canonical key, its separator-stripped token form, and every row alias index into one `FrozenDictionary` derived from the roster behind a `Lazy` accessor — an eager index over another type's `Items` is the materialization race the accessor forecloses. A stem carrying NEITHER a `gl` nor a `dx` token leaves `Convention` UNRESOLVED — the probe's green statistics are recorded on the row and never promoted to a default, because a defaulted convention is the silent-lighting-inversion defect that survives every downstream check and only surfaces as wrongly-lit geometry. `gloss`, `glossiness`, and `smoothness` resolve to `specular_roughness` with `Inverted` set, and the `filter#PLANE_OP` `RemapCurve.Levels.Invert` curve applies that inversion in the LINEAR domain — this page holds the FLAG and never the arithmetic; `arm` is an `orm` alias (identical slot order) and `mra` the reversed pack, so a packed stem resolves to a `ChannelPack` row carrying EVERY slot channel it covers rather than one arbitrary member, because a pack resolved to a single channel silently drops two thirds of the plane; a four-digit token at 1001 or above claims the UDIM variant slot. The probe REFUTES rather than proposes: a stem claiming a three-component channel over a single-component plane, or a claimed pack over a plane narrower than four components, drops to `Unresolved` with the contradiction recorded — a classification that survives its own evidence is the only one this fold emits. The `Declared` and `Peer` arms re-run every alias, pack, and UDIM law over a foreign manifest rather than trusting its rows, so a peer manifest is an input to classification and never a substitute for it; a peer's `tiled` declaration reaches no `TextureSet` from here, since tileability is `tile#TILE_GATE` evidence a set earns by grading; the peer wire's `height_scale` and `alpha_mode` are caller facts the app root lifts into `IngestIntent` beside the fetched plane bytes, so the lift's arity holds and no wire field bypasses the intent record. The LIFT is the one ingest mint and the one CONVERSION SITE: a `dx` source flips green through `NormalConvention.ToGl` over every level of every normal-convention plane — per-texel and linear, so each level stays its own fold — and an `Inverted` gloss map runs `filter#PLANE_OP` `RemapCurve.Levels.Invert` per level, BEFORE any plane is keyed, so the wire's frozen always-`gl` law holds by construction and the draft's `Convention` column records the SOURCE as provenance; per tile, every classified plane must agree on extent (the probe evidence re-enters here), a classified stem with no supplied plane faults by name, and the tile grouping hands one draft per tile so a UDIM directory lifts into the `UdimSheet.Of` assembly rather than collapsing onto one tile.

```csharp signature
// (Continues the Rasm.Materials.Raster compilation unit, plus:)
using System.IO;                                  // Path — the peer leaf's stem projection
using CommunityToolkit.HighPerformance.Buffers;   // SpanOwner — the convention flip's row scratch

// --- [MODELS] ------------------------------------------------------------------------------
// The per-file evidence a scan supplies. Mean and Variance are the plane's own statistics — they REFUTE a
// stem's claim (a three-component claim over a one-component plane) and never promote one.
public readonly record struct PlaneProbe(
    string Stem, PlaneFormat Format, PlaneTransfer Transfer, Dimension Width, Dimension Height,
    ShadeVec4 Mean, ShadeVec4 Variance);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IngestSource {
    private IngestSource() { }
    public sealed record Stems(Seq<PlaneProbe> Probes) : IngestSource;
    public sealed record Declared(SetManifest Manifest) : IngestSource;

    // The python-minted wire, decoded — Rasm.Channels.AssetSetManifest is the protoc product of rasm/channels.proto
    // (option csharp_namespace = "Rasm.Channels"), whose field roster IS the frozen [05.1]-[05.3]. The arm holds the
    // MESSAGE, not raw bytes: the fallible parse lives on SetIngest.Peer so Classify stays total and pure.
    public sealed record Peer(Rasm.Channels.AssetSetManifest Manifest) : IngestSource;
}

// Channels carries EVERY channel the stem's plane covers: one entry for a standalone map, three for a packed
// sheet in the pack's own slot order. Inverted records the gloss-to-roughness inversion filter#PLANE_OP applies
// AFTER the plane decodes; inverting an srgb-encoded value is the silent-roughness fork the flag prevents.
public sealed record ClassifiedMap(
    Seq<TextureChannel> Channels, string Stem, Option<UdimTile> Tile, Option<ChannelPack> Pack,
    bool Inverted, PlaneProbe Probe);

// The monoid: Empty is the identity, Combine is associative, and Classify is a fold. Convention resolves by
// FIRST evidence and a later divergent token lands in Unresolved rather than overwriting — a set declaring
// two conventions has no single answer, and picking one silently is the fork this refuses.
public sealed record SetManifest(
    Seq<ClassifiedMap> Maps, Seq<string> Unresolved, Option<NormalConvention> Convention, Seq<UdimTile> Udim) {
    public static readonly SetManifest Empty = new(Seq<ClassifiedMap>(), Seq<string>(), Option<NormalConvention>.None, Seq<UdimTile>());

    public SetManifest Combine(SetManifest other) =>
        new(Maps + other.Maps,
            Unresolved + other.Unresolved + Conflict(other),
            Convention.IsNone ? other.Convention : Convention,
            toSeq((Udim + other.Udim).Distinct().OrderBy(static t => t.Value)));

    Seq<string> Conflict(SetManifest other) =>
        Convention.IsSome && other.Convention.IsSome && Convention != other.Convention
            ? Seq($"<normal-convention-divergent:{Convention.Map(static c => c.Key).IfNone("none")}:{other.Convention.Map(static c => c.Key).IfNone("none")}>")
            : Seq<string>();
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class SetIngest {
    // One index derived from the roster behind a Lazy accessor: canonical key, its separator-stripped token
    // form, and every alias. A row gains an alias by growing its own column — a second table would be the drift
    // surface this forecloses, and an eager initializer over another type's Items is the materialization race.
    static readonly Lazy<FrozenDictionary<string, TextureChannel>> Index =
        new(static () => TextureChannel.Items
            .SelectMany(static c => c.Aliases.Add(c.Key).Add(c.Key.Replace("_", string.Empty)).Map(a => (Alias: a, Channel: c)))
            .DistinctBy(static e => e.Alias, StringComparer.OrdinalIgnoreCase)
            .ToFrozenDictionary(static e => e.Alias, static e => e.Channel, StringComparer.OrdinalIgnoreCase));

    static readonly Lazy<FrozenDictionary<string, ChannelPack>> Packs =
        new(static () => new (string Alias, ChannelPack Pack)[] { ("orm", ChannelPack.Orm), ("arm", ChannelPack.Orm), ("mra", ChannelPack.Mra) }
            .ToFrozenDictionary(static e => e.Alias, static e => e.Pack, StringComparer.OrdinalIgnoreCase));

    static readonly Lazy<FrozenDictionary<string, NormalConvention>> Conventions =
        new(static () => new (string Token, NormalConvention Convention)[] {
            ("gl", NormalConvention.Gl), ("normalgl", NormalConvention.Gl), ("norgl", NormalConvention.Gl),
            ("dx", NormalConvention.Dx), ("normaldx", NormalConvention.Dx), ("nordx", NormalConvention.Dx),
        }.ToFrozenDictionary(static e => e.Token, static e => e.Convention, StringComparer.OrdinalIgnoreCase));

    static readonly Lazy<FrozenSet<string>> GlossAliases =
        new(static () => new[] { "gloss", "glossiness", "smoothness" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase));

    public static SetManifest Classify(IngestSource source) =>
        source.Switch(
            stems:    static s => s.Probes.Fold(SetManifest.Empty, static (acc, probe) => acc.Combine(One(probe))),
            declared: static d => d.Manifest.Maps.Fold(SetManifest.Empty, static (acc, map) => acc.Combine(One(map.Probe))).Combine(
                          SetManifest.Empty with { Unresolved = d.Manifest.Unresolved }),
            // The python wire re-enters the SAME alias, pack, UDIM, and refutation law: every MapEntry and
            // PackEntry lowers to a PlaneProbe and folds through One, the manifest's own unresolved accumulation
            // carries forward verbatim, and the wire's normal_convention seats as first evidence where no stem
            // token spoke — a peer manifest is an INPUT to classification, never a substitute for it.
            peer:     static p =>
                toSeq(p.Manifest.Maps).Fold(SetManifest.Empty, (acc, entry) => acc.Combine(PeerMap(p.Manifest, entry)))
                    .Combine(toSeq(p.Manifest.Packs).Fold(SetManifest.Empty, (acc, entry) => acc.Combine(PeerPack(p.Manifest, entry))))
                    .Combine(SetManifest.Empty with {
                        Unresolved = toSeq(p.Manifest.Unresolved),
                        Convention = NormalConvention.TryGet(p.Manifest.NormalConvention, out NormalConvention? declared) ? Some(declared!) : Option<NormalConvention>.None,
                    }));

    // THE python-manifest decode seam — the wire-freeze [05] SET_INGEST consumer edge. The generated Parser is
    // the one decode; a non-pbr_set kind REFUSES here because an hdri/ibl manifest is product-borne (freeze
    // [02.4]: IBL leaves are egress names, and classifying them from stems is the shadowing defect) and its
    // consumers are the environment/read surfaces, never this classifier.
    public static Fin<IngestSource> Peer(ReadOnlyMemory<byte> wire, Op key) {
        try {
            Rasm.Channels.AssetSetManifest manifest = Rasm.Channels.AssetSetManifest.Parser.ParseFrom(wire.Span);
            return manifest.Kind == "pbr_set"
                ? Fin.Succ<IngestSource>(new IngestSource.Peer(manifest))
                : MaterialFault.Parameter(key, $"<peer-manifest-kind:{manifest.Kind}>");
        }
        catch (Google.Protobuf.InvalidProtocolBufferException malformed) {
            return MaterialFault.Parameter(key, $"<peer-manifest-malformed:{malformed.Message}>");
        }
    }

    // One MapEntry lowers to probe evidence: the leaf's stem re-enters the alias law, the (depth, channels)
    // pair resolves its storage row through the roster's own semantic rounding, the color_space tag lowers to
    // the transfer band, and the extent is the manifest's own. Wire statistics do not cross — Mean/Variance
    // carry zero, evidence that refutes nothing rather than fabricated moments. The declared role never SEATS a
    // channel; it CROSS-CHECKS the stem's classification, and a divergent pair lands in Unresolved with both
    // names — trusting either side alone is the fork the refutation law forecloses.
    static SetManifest PeerMap(Rasm.Channels.AssetSetManifest manifest, Rasm.Channels.MapEntry entry) =>
        PlaneDepth.TryGet(entry.Depth, out PlaneDepth? depth)
        && PlaneTransfer.TryGet(entry.ColorSpace, out PlaneTransfer? transfer)
        && PlaneFormat.For(entry.Channels, depth!).Case is PlaneFormat format
            ? One(new PlaneProbe(Path.GetFileNameWithoutExtension(entry.File), format, transfer!,
                  Dimension.Create(manifest.Width), Dimension.Create(manifest.Height), default, default)) switch {
                  var one => one.Maps.HeadOrNone().Match(
                      Some: map => map.Pack.IsSome || map.Channels.Exists(c => c.Key == entry.Role)
                          ? one
                          : SetManifest.Empty with { Unresolved = Seq($"<peer-role-divergent:{entry.Role}:{map.Stem}>") },
                      None: () => one),
              }
            : SetManifest.Empty with { Unresolved = Seq($"<peer-row-vocabulary:{entry.File}:{entry.Depth}:{entry.ColorSpace}>") };

    // One PackEntry lowers the same way: the pack leaf's stem (orm/mra by the frozen egress grammar) resolves
    // through the pack index, the wire's format key resolves the storage row verbatim, and a packed plane is
    // always raw transfer by the freeze's own pack law.
    static SetManifest PeerPack(Rasm.Channels.AssetSetManifest manifest, Rasm.Channels.PackEntry entry) =>
        PlaneFormat.TryGet(entry.Format, out PlaneFormat? format)
            ? One(new PlaneProbe(Path.GetFileNameWithoutExtension(entry.File), format!, PlaneTransfer.Raw,
                  Dimension.Create(manifest.Width), Dimension.Create(manifest.Height), default, default))
            : SetManifest.Empty with { Unresolved = Seq($"<peer-pack-vocabulary:{entry.File}:{entry.Format}>") };

    // The per-stem resolution: tokenize, claim the variant, claim the pack or the channel, take the
    // convention from a token alone, then let the probe REFUTE. Every path that does not resolve returns the
    // stem into Unresolved, so the fold is total and the caller sees exactly what went unclaimed.
    static SetManifest One(PlaneProbe probe) {
        Seq<string> tokens = Tokenize(probe.Stem);
        Option<UdimTile> tile = tokens.Choose(static t => int.TryParse(t, out int v) ? UdimTile.Admit(v) : Option<UdimTile>.None).HeadOrNone();
        Option<NormalConvention> convention = tokens.Choose(static t => Conventions.Value.TryGetValue(t, out NormalConvention? c) ? Some(c!) : Option<NormalConvention>.None).HeadOrNone();
        Option<ChannelPack> pack = tokens.Choose(static t => Packs.Value.TryGetValue(t, out ChannelPack? p) ? Some(p!) : Option<ChannelPack>.None).HeadOrNone();
        return pack.Match(
            // A packed sheet resolves to EVERY slot channel: claiming one member would silently drop two lanes.
            Some: p => probe.Format.Components is 4
                ? SetManifest.Empty with { Maps = Seq(new ClassifiedMap(p.Slots, probe.Stem, tile, Some(p), Inverted: false, probe)), Udim = tile.ToSeq() }
                : SetManifest.Empty with { Unresolved = Seq($"<pack-plane-narrow:{probe.Stem}>") },
            // The tuple's element NAMES ride the None arm's own type argument — a ternary whose branches spell
            // conflicting names drops them from the natural type and every downstream .Channel read stops binding.
            None: () => tokens.Choose(static t => Index.Value.TryGetValue(t, out TextureChannel? c)
                    ? Some((Channel: c!, Gloss: false))
                    : GlossAliases.Value.Contains(t)
                        ? Some((Channel: TextureChannel.SpecularRoughness, Gloss: true))
                        : Option<(TextureChannel Channel, bool Gloss)>.None)
                .HeadOrNone()
                .Match(
                    Some: hit => probe.Format.Components >= hit.Channel.Components
                        ? SetManifest.Empty with {
                              Maps = Seq(new ClassifiedMap(Seq(hit.Channel), probe.Stem, tile, Option<ChannelPack>.None, hit.Gloss, probe)),
                              Convention = hit.Channel.Slot == Some(SinkSlot.Normal) || hit.Channel == TextureChannel.GeometryCoatNormal ? convention : Option<NormalConvention>.None,
                              Udim = tile.ToSeq(),
                          }
                        : SetManifest.Empty with { Unresolved = Seq($"<channel-components-refuted:{probe.Stem}:{hit.Channel.Key}>") },
                    None: () => SetManifest.Empty with { Unresolved = Seq(probe.Stem) }));
    }

    // Separator normalization: '-', '_', '.', and space all fold to ONE boundary, so nor_dx, nor-dx, nor.dx,
    // and "nor dx" tokenize identically. The [EXPRESSION_SPINE] kernel exemption is the split itself.
    static Seq<string> Tokenize(string stem) =>
        toSeq(stem.Split(['-', '_', '.', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    // THE LIFT — the one ingest mint and the ONE conversion site the frozen always-gl wire law names. Maps
    // group per tile; each classified stem binds its supplied pyramid or faults by name; the dx flip and the
    // gloss inversion run here, ONCE, before any plane is keyed; and the draft records the SOURCE convention
    // as provenance, which is exactly what lets TextureSetWire.normalConvention truthfully carry `dx`.
    public static Fin<Seq<(Option<UdimTile> Tile, TextureSetDraft Draft)>> Draft(
        SetManifest manifest, HashMap<string, TexturePyramid> planes, IngestIntent intent, Op key) =>
        toSeq(manifest.Maps.GroupBy(static map => map.Tile.Map(static t => t.Value).IfNone(0)))
            .Fold(Fin.Succ(Seq<(Option<UdimTile>, TextureSetDraft)>()), (acc, group) =>
                acc.Bind(drafts => Tile(toSeq(group), manifest.Convention, planes, intent, key).Map(drafts.Add)));

    // One tile's draft: extent agreement re-enters from the probe evidence, packs seat as pack planes, and
    // every plane passes the per-map conversion before it reaches the draft.
    static Fin<(Option<UdimTile> Tile, TextureSetDraft Draft)> Tile(
        Seq<ClassifiedMap> maps, Option<NormalConvention> convention, HashMap<string, TexturePyramid> planes, IngestIntent intent, Op key) =>
        from head in maps.HeadOrNone().ToFin(MaterialFault.Parameter(key, "<ingest-tile-empty>"))
        from _ in guard(maps.ForAll(map => map.Probe.Width == head.Probe.Width && map.Probe.Height == head.Probe.Height),
            MaterialFault.Parameter(key, $"<ingest-tile-extent-divergent:{head.Stem}>"))
        from bound in maps.Fold(Fin.Succ((Channels: HashMap<TextureChannel, TexturePyramid>.Empty, Packs: Seq<ChannelPackPlane>())), (acc, map) =>
            acc.Bind(carried =>
                from supplied in planes.Find(map.Stem).ToFin(MaterialFault.Parameter(key, $"<ingest-plane-missing:{map.Stem}>"))
                from converted in Converted(supplied, map, convention, key)
                select map.Pack.Match(
                    Some: pack => (carried.Channels, carried.Packs.Add(new ChannelPackPlane(pack, converted, map.Channels))),
                    None: () => (carried.Channels.AddOrUpdate(map.Channels.Head, converted), carried.Packs))))
        select (head.Tile, new TextureSetDraft(head.Probe.Width, head.Probe.Height, Dimension.Create(1), LayerLaw.None,
            convention.IfNone(NormalConvention.Gl), intent.Alpha, intent.HeightScaleMm, Option<TileProof>.None,
            head.Tile.ToSeq(), bound.Channels, bound.Packs, intent.Conductor, intent.Material));

    // The per-map conversion. The dx flip mutates decoded texels IN PLACE — per-texel and linear, so every
    // level of a supplied chain stays its own fold — and the gloss inversion runs the filter owner's own
    // Levels.Invert per level, the affine curve commuting with every linear mip fold, so the rebuilt chain
    // carries the source policy unchanged.
    static Fin<TexturePyramid> Converted(TexturePyramid pyramid, ClassifiedMap map, Option<NormalConvention> convention, Op key) {
        if (convention == Some(NormalConvention.Dx)
            && map.Channels.Exists(static c => c == TextureChannel.GeometryNormal || c == TextureChannel.GeometryCoatNormal)) {
            pyramid.Levels.Iter(Flip);
        }
        return map.Inverted
            ? pyramid.Levels
                .Fold(Fin.Succ(Seq<TexturePlane>()), (acc, level) =>
                    acc.Bind(built => PlaneOp.Apply(level, Seq<PlaneOp>(new PlaneOp.Remap(RemapCurve.Levels.Invert)), key).Map(result => built.Add(result.Plane))))
                .Map(levels => { pyramid.Dispose(); return new TexturePyramid(levels, pyramid.Policy, pyramid.Coupled); })
            : Fin.Succ(pyramid);
    }

    static void Flip(TexturePlane level) {
        using SpanOwner<ShadeVec4> row = SpanOwner<ShadeVec4>.Allocate(level.Width.Value);
        for (int layer = 0; layer < level.Layers.Value; layer++) {
            for (int y = 0; y < level.Height.Value; y++) {
                level.ReadShade(y, layer, row.Span);
                for (int x = 0; x < row.Span.Length; x++) { row.Span[x] = NormalConvention.Dx.ToGl(row.Span[x]); }
                level.WriteShade(y, layer, row.Span);
            }
        }
    }
}

// The caller-owned facts a filename cannot carry: the alpha declaration, the millimetre height span, and
// the subject identities. One record, so the lift's arity never grows a parameter per fact.
public sealed record IngestIntent(AlphaMode Alpha, double HeightScaleMm, Option<ConductorMetal> Conductor, Option<MaterialId> Material);
```

## [05]-[SET_BIND]

- Owner: `SetBind` the set-to-appearance lowering; `BindTarget` `[Union]` the requested lowering; `SetBinding` `[Union]` the produced carrier.
- Cases: target {`Program` (the node DAG a renderer compiles once), `Point` (the per-texel parameter row a shade reconstructs), `Average` (the measured summary row the seam `AppearanceSummary` and the LOD fallback read)} · binding {`Program`, `Row`}.
- Entry: `public static Fin<SetBinding> Bind(TextureSet set, MaterialParameters fallback, BindTarget target, Op key)` — ONE entry whose modality discriminates on the target's own case, never on a name suffix or a boolean; the `fallback` row supplies every column the set does not carry, so a partial set always binds.
- Exemption: `Mean` is the page's `[EXPRESSION_SPINE]` kernel — a fixed-extent row accumulation over a caller-owned scratch pair, the only statement-shaped body here.
- Packages: `graph#MATERIAL_GRAPH` (composed — `MaterialGraph`, `AppearanceNode.Input`/`Texture`/`Normal`/`BsdfOutput`, `PortId`, `PortValue`), `texture#TEXTURE_UV` (composed — `TextureUv.Port` minting each `Texture` node's total closure, `TextureUv.Sample` reading a plane at a point, `UvSample`, `SamplerState`, `Channel`), `plane#TEXTURE_PLANE` (composed — `TexturePyramid.AsImage` lifting a pyramid into the existing `TextureSource.Image` sampler input under the plane's own transfer, and the `TexturePlane.Read` row rail the measured mean folds), `CommunityToolkit.HighPerformance` (`SpanOwner<T>` the mean fold's caller-owned scratch), LanguageExt.Core.
- Growth: a new lowering modality is one `BindTarget` case with its `SetBinding` arm — never a second `Bind` overload and never a `BindGraph`/`BindRow` pair; a channel reaches the graph program the moment its row carries a `SinkSlot`, so the sink widening at `graph#MATERIAL_GRAPH` propagates here as row data, and the wiring reads each slot's own `PortId` column so the bound program and `MaterialGraph.Default` share ONE topology with no literal re-authored.
- Boundary: `SetBind` closes the round trip photo-or-press → planes → SHADEABLE MATERIAL; a lowering that stops at encodable bytes is the deleted form. The `Program` arm binds only the channels whose rows carry a `SinkSlot`, because the `graph#MATERIAL_GRAPH` `BsdfOutput` sink admits exactly five ports today — every other channel binds through the `Point` arm, so no unread phantom `Texture` node enters the DAG to be mistaken for live capability. Each bound channel becomes one `AppearanceNode.Texture` holding the `TextureUv.Port` closure over its pyramid's `AsImage` lift, projected through the slot's own `Channel` modality and then through the slot's `Encode` column — the normal port re-encodes the decoded signed texel to the `[0,1]` convention the node's own `2v−1` decode expects, so a bound normal plane perturbs the frame correctly instead of inverting X and Y at every texel; a set carrying no normal channel keeps the default graph's identity-normal node at strength zero, so the produced program is always a complete DAG the compiler admits. The per-row TRANSFER law rides `AsImage`: an `srgb` plane decodes to scene-linear at the lift and a `raw` plane crosses untouched, so no consumer of a bound graph re-applies a transfer and a doubly-decoded colour plane is unrepresentable; a LAYERED set refuses the `Program` arm outright, because `TextureSource.Image` carries one layer by construction and a cube or array set reaches a renderer as a set rather than through the UV sampler. The `Point` and `Average` arms read PACKS as well as standalone channels — a pack plane's lanes are its `ChannelPack.Slots` in order, so each slot's lane projects to that channel's scalar and folds through its own lens, and a set whose roughness rides inside an `orm` sheet reconstructs the same row a standalone roughness plane would. The `Point` arm reconstructs the FULL vector: each channel's `ColumnLens.Write` folds its sampled texel onto the fallback row, a channel whose lens carries no write contributes to the OpenPBR vector through `surface#OPENPBR_SLAB` `OpenPbrSurface.Of` rather than to the row (the typed absence, never a fabricated column), and the result re-admits through `MaterialParameters.Of` so a sampled set cannot smuggle an out-of-unit weight or an out-of-gamut colour past the one admission every library row passes. The `Average` arm MEASURES the mean — one streaming pass over each channel's base level through the plane's own decoded row rail — rather than reading a pyramid's coarsest texel: only a box fold's tail is the arithmetic mean, while a Kaiser, renormalizing, or variance-coupled fold's tail is a weighted or corrected value, and publishing that as the mean fabricates the number the seam appearance key then carries forever. The measured fold costs one pass over planes the press just wrote, needs no pyramid at all, and therefore admits a single-level set the pyramid-tail read had to refuse.

```csharp signature
// (Continues the Rasm.Materials.Raster compilation unit — [04]'s SpanOwner using serves the mean fold too.)

// --- [TYPES] -------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BindTarget {
    private BindTarget() { }
    public sealed record Program() : BindTarget;
    public sealed record Point(UnitInterval U, UnitInterval V, double MipLevel) : BindTarget;
    public sealed record Average() : BindTarget;

    public static readonly BindTarget Dag = new Program();
    public static readonly BindTarget Summary = new Average();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SetBinding {
    private SetBinding() { }
    public sealed record Program(MaterialGraph Graph) : SetBinding;
    public sealed record Row(MaterialParameters Parameters) : SetBinding;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class SetBind {
    public static Fin<SetBinding> Bind(TextureSet set, MaterialParameters fallback, BindTarget target, Op key) =>
        target.Switch(
            state:   (Set: set, Fallback: fallback, Key: key),
            program: static (s, _) => Dag(s.Set, s.Key).Map(static g => (SetBinding)new SetBinding.Program(g)),
            point:   static (s, p) => Sample(s.Set, s.Fallback, p, s.Key).Map(static r => (SetBinding)new SetBinding.Row(r)),
            average: static (s, _) => Summary(s.Set, s.Fallback, s.Key).Map(static r => (SetBinding)new SetBinding.Row(r)));

    // Port ids come off the SinkSlot rows, which are the SAME ids MaterialGraph.Default wires, so a bound
    // program and the default program carry one topology and no literal is re-authored here: a slot the set
    // covers becomes a Texture node at that id, a slot it does not keeps its Input node pulling the fallback
    // column. The normal slot additionally seats its Normal node over the source port.
    static Fin<MaterialGraph> Dag(TextureSet set, Op key) {
        PortId normal = PortId.Of(5), sink = PortId.Of(7);
        UvSample anchor = new(UnitInterval.Create(0.0), UnitInterval.Create(0.0), Vector3d.Zero, Vector3d.ZAxis, 0.0);
        Fin<AppearanceNode> Slot(SinkSlot slot) =>
            set.Channels.Find(TextureChannel.BySlot(slot))
                .Match(
                    Some: pyramid => pyramid.AsImage(key).Map(image => (AppearanceNode)new AppearanceNode.Texture(
                        slot.Port, Compose(TextureUv.Port(image, anchor, SamplerState.Default, slot.Modality, key), slot))),
                    None: () => Fin.Succ<AppearanceNode>(new AppearanceNode.Input(slot.Port, slot.Fallback)));
        return set.Layers.Value > 1
            ? Fin.Fail<MaterialGraph>(MaterialFault.Parameter(key, $"<layered-set-has-no-uv-program:{set.Law.Key}:{set.Layers.Value}>"))
            : toSeq(SinkSlot.Items).Fold(Fin.Succ(Seq<AppearanceNode>()), (acc, slot) => acc.Bind(nodes => Slot(slot).Map(nodes.Add)))
                .Map(nodes => new MaterialGraph(nodes
                    .Add(new AppearanceNode.Normal(normal, SinkSlot.Normal.Port, Strength: set.Channels.ContainsKey(TextureChannel.GeometryNormal) ? 1.0 : 0.0))
                    .Add(new AppearanceNode.BsdfOutput(sink, SinkSlot.BaseColor.Port, SinkSlot.Metalness.Port, SinkSlot.Roughness.Port, normal, SinkSlot.Emission.Port)), sink));
    }

    // The slot's Encode column composes ONTO the sampler's own total closure, so the port projection stays one
    // data row and the fault/non-finite fold TextureUv.Port owns is untouched.
    static Func<double, double, PortValue> Compose(Func<double, double, PortValue> port, SinkSlot slot) =>
        (u, v) => slot.Encode(port(u, v));

    // The per-texel reconstruction over BOTH carriers: every standalone channel samples its own plane and every
    // pack lane projects to its slot channel, then the ONE MaterialParameters.Of re-admission gates the result
    // — a sampled set cannot reach the library's own invariants by a side door.
    static Fin<MaterialParameters> Sample(TextureSet set, MaterialParameters fallback, BindTarget.Point at, Op key) =>
        set.Channels.Fold(Fin.Succ(fallback), (acc, pair) =>
            acc.Bind(row => Read(pair.Value, at, key).Map(texel => Apply(pair.Key, row, texel))))
            .Bind(row => set.Packs.Fold(Fin.Succ(row), (acc, pack) =>
                acc.Bind(carried => Read(pack.Plane, at, key).Map(texel => Unpack(pack, carried, texel)))))
            .Bind(row => MaterialParameters.Of(row, key));

    static Fin<ShadeVec4> Read(TexturePyramid pyramid, BindTarget.Point at, Op key) =>
        from image in pyramid.AsImage(key)
        from sample in TextureUv.Sample(image, new UvSample(at.U, at.V, Vector3d.Zero, Vector3d.ZAxis, at.MipLevel), SamplerState.Default, key)
        select sample;

    static MaterialParameters Apply(TextureChannel channel, MaterialParameters row, ShadeVec4 texel) =>
        channel.Origin switch {
            ChannelOrigin.Shaded shaded => shaded.Lens.Write.Match(Some: write => write(row, texel), None: () => row),
            _ => row,
        };

    // A pack lane IS its slot channel's scalar: lane order is the ChannelPack row's own slot order, an absent
    // slot contributes nothing, and each present lane folds through its channel's own lens.
    static MaterialParameters Unpack(ChannelPackPlane pack, MaterialParameters row, ShadeVec4 texel) =>
        pack.Present.Fold(row, (carried, channel) =>
            pack.Pack.Lane(channel).Match(
                Some: lane => Apply(channel, carried, new ShadeVec4(Lane(texel, lane), 0.0, 0.0, 1.0)),
                None: () => carried));

    static double Lane(ShadeVec4 texel, int lane) => lane switch { 0 => texel.X, 1 => texel.Y, _ => texel.Z };

    // The MEASURED mean: one streaming pass per plane through the plane's own decoded row rail. A pyramid tail
    // is the arithmetic mean under a box fold ALONE — Kaiser weights, renormalization, and the variance
    // coupling each move it — so reading the tail as a mean fabricates the value the seam key then carries.
    static Fin<MaterialParameters> Summary(TextureSet set, MaterialParameters fallback, Op key) =>
        set.Channels.Fold(Fin.Succ(fallback), (acc, pair) => acc.Map(row => Apply(pair.Key, row, Mean(pair.Value.Base))))
            .Bind(row => set.Packs.Fold(Fin.Succ(row), (acc, pack) => acc.Map(carried => Unpack(pack, carried, Mean(pack.Plane.Base)))))
            .Bind(row => MaterialParameters.Of(row, key));

    static ShadeVec4 Mean(TexturePlane plane) {
        using SpanOwner<ShadeVec4> field = SpanOwner<ShadeVec4>.Allocate(plane.Width.Value);
        ShadeVec4 total = ShadeVec4.Splat(0.0);
        for (int layer = 0; layer < plane.Layers.Value; layer++) {
            for (int row = 0; row < plane.Height.Value; row++) {
                plane.ReadShade(row, layer, field.Span);
                for (int x = 0; x < field.Span.Length; x++) { total += field.Span[x]; }
            }
        }
        return total * (1.0 / (plane.Width.Value * (double)plane.Height.Value * plane.Layers.Value));
    }
}
```

## [06]-[RESEARCH]

(none)
