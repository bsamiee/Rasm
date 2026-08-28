# [MATERIALS_SET]

THE CHANNEL VOCABULARY AND THE BAKED SET. One `TextureChannel` `[SmartEnum<string>]` roster closes the per-texel appearance field family — the OpenPBR Surface 1.1 inputs (each read off its `surface#OPENPBR_SLAB` `OpenPbrSurface` column), the geometry-group inputs, and the derived modulators — each row carrying its group, component count, transfer, decoded neutral, unit, mip law, KTX payload policy, origin lens, MaterialX binding, sink slot, and ingest aliases as DATA, so a new bakeable field is a row and never a second channel surface. One `TextureSet` record is the extent-coherent, content-keyed bundle of `plane#TEXTURE_PLANE` pyramids those rows address, admitted once at `TextureSet.Of` under the extent, transfer, layer, alpha, variant, and pack gates; one `SetIngest.Classify` fold classifies a foreign directory or a peer-declared manifest into a `SetManifest` by alias alone, accumulating every unclaimed stem rather than inferring a channel; and one `SetBind.Bind` entry lowers a set BACK into the appearance engine — the `graph#MATERIAL_GRAPH` `MaterialGraph` program its sink-slot channels drive, or the `graph#MATERIAL_LIBRARY` `MaterialParameters` row the full channel roster reconstructs — closing the round trip from pressed planes to shadeable material rather than stopping at encodable bytes.

The channel roster is a TOTAL PROJECTION of an existing closed vocabulary, never a hand-picked subset and never a synthesized row: every OpenPBR row reads its own `OpenPbrSurface` column through the row's `ColumnLens`, so coat colour, specular colour, fuzz colour, transmission, and thin-film thickness each bake and each bind BACK through the same lens. The canonical channel name is `snake_case` and IS the OpenPBR identifier verbatim where OpenPBR names the input, so the `.mtlx` port binding is mechanical; the C# identifier is its PascalCase, with `SpecularAnisotropy` and `SpecularRotation` the two rows whose identifiers shorten against their `specular_roughness_anisotropy` and `specular_roughness_anisotropy_rotation` keys. Channel values live in the DECODED domain — a normal is the signed `(0,0,1)` unit vector, curvature the signed `[-1,1]` field, height the normalized `[0,1]` scalar whose millimetre span rides the set — and integer encoding is wholly `plane#PLANE_VOCABULARY`'s storage concern, so one `NormalConvention` green-sign flip serves every depth and no page re-derives an encode rule. The page composes the `plane#TEXTURE_PLANE` `TexturePlane`/`TexturePyramid`/`PlaneFormat`/`PlaneTransfer`/`AlphaMode`/`MipPolicy`/`PlanePrimaries`/`NormalConvention` substrate, the `codec#RASTER_CODEC` `RasterFormat`/`KtxPayload` container and quality vocabulary, the `filter#PLANE_OP` `PlaneOp` derivation family, the `tile#TILE_GATE` `TileProof` tileability evidence, the `surface#OPENPBR_SLAB` `OpenPbrSurface`/`ConductorMetal` vector, the `graph#MATERIAL_GRAPH` node union and `graph#MATERIAL_LIBRARY` parameter row, the `texture#TEXTURE_UV` `TextureUv.Port`/`Sample` sampler and `ShadeVec4` register, the contract `Rasm.Element` `MaterialId` identity and `ContentAddress` address spelling, and the kernel `ContentHash`/`Dimension`/`UnitInterval`/`ValidityClaim` owners — reminting no sampler, no colour register, no address, no content key, and no fault.

## [01]-[INDEX]

- [02]-[TEXTURE_CHANNEL]: the `ChannelGroup`/`ChannelUnit`/`SinkSlot` axes, the `MtlxBinding`/`ChannelOrigin` row columns with the `ColumnLens` bidirectional correspondence, the `TextureChannel` roster with its lazily-derived indexes, and the `ChannelPack` `orm`/`mra` slot table.
- [03]-[TEXTURE_SET]: the `LayerLaw` layer axis, the `UdimTile` Mari value object, the `ChannelPackPlane` packed carrier, the `EgressSlot`/`EgressVariant` naming vocabulary, the `TextureSet` record with its `Gates` admission sequence, content key, and one egress-name entry, the `UdimSheet` per-tile assembly with its `UdimResidency` read policy, and the `TextureAtlas` packing fold with its per-participant `AtlasPlacement` rows.
- [04]-[SET_INGEST]: the `PlaneProbe` evidence row, the `IngestRefusal` reason vocabulary, the `IngestSource` union with its python-wire `Peer` arm, the roster-derived alias index, the `ClassifiedMap`/`SetManifest` monoid, the total `SetIngest.Classify` fold beside the fallible `Peer` decode boundary, and the Element contract-roster admission `SetIngest.Roster` with its `RosterBinding` policy rows.
- [05]-[SET_BIND]: the `BindTarget`/`SetBinding` unions and the one `SetBind.Bind` lowering — the sink-slot graph program, the per-texel parameter row over channels AND packs, and the measured plane-mean summary row.

## [02]-[TEXTURE_CHANNEL]

- Owner: `TextureChannel` `[SmartEnum<string>]` the closed per-texel field roster; `ChannelGroup` `[SmartEnum<string>]` the OpenPBR group axis; `ChannelUnit` `[SmartEnum<string>]` the unit axis binding each row's UnitsNet SI member; `SinkSlot` `[SmartEnum<string>]` the `graph#MATERIAL_GRAPH` `BsdfOutput` port axis; `MtlxBinding` `[Union]` the `.mtlx` egress law; `ChannelOrigin` `[Union]` the per-row production law; `ColumnLens` the bidirectional `OpenPbrSurface`↔`MaterialParameters` correspondence; `ChannelPack` `[SmartEnum<string>]` the two packing orders.
- Cases: channel {the OpenPBR rows `base_weight`…`emission_luminance`, the geometry rows `geometry_opacity`/`geometry_normal`/`geometry_coat_normal`/`geometry_tangent`/`geometry_coat_tangent`, the derived rows `height`/`occlusion`/`curvature`} · group {`base`, `specular`, `transmission`, `subsurface`, `coat`, `fuzz`, `thinFilm`, `emission`, `geometry`, `derived`} · unit {`none`, `mm`, `nm`, `cd/m2`} · sink-slot {`baseColor`, `metalness`, `roughness`, `normal`, `emission`} · mtlx-binding {`Canonical` (spelled `Verbatim` at a row), `Scaled`, `Split`, `Lowered`, `Absent` (spelled `Unmapped` at a row)} · origin {`Shaded`, `Geometric`, `Derived`} · pack {`orm`, `mra`}.
- Law: `OpenPbrSurface.Conductor` and `geometry_thin_walled` are the TWO deliberate exclusions — a conductor row and a double-sided-shell flag are set-level facts no per-texel field carries, so the conductor rides `TextureSet.Conductor` and the shell flag rides the generated `OpenPbr.GeometryThinWalled` column, never a plane in this roster.
- Law: `geometry_tangent` and `geometry_coat_tangent` stay the `.mtlx` egress ports and the frame EVIDENCE a peer consumer supplies, never a Rasm-side shading input — anisotropy direction reaches the lobes as the scalar `specular_roughness_anisotropy_rotation` plane, which mips correctly under `MipPolicy.Box` where a tangent VECTOR plane does not (averaging two opposed tangents cancels to nothing), so the rotation form is both the OpenPBR-canonical input and the only one whose pyramid means anything.
- Law: every derived index projects from `Items` through a `Lazy<T>` accessor, never an eager `static readonly` field initializer — a field initializer inside the roster's own type runs during that type's class construction, before the generated `Items` materialization has published, so an eager index captures an empty roster and poisons every consumer that reads it.
- Entry: the roster IS the entry — `TextureChannel.Items` is the ordered vocabulary every downstream fold reads, `TextureChannel.Get(key)`/`TryGet(out row)` resolve a wire key, `Ordinal` is the ONE declaration-order rank the set key preimage and the `press#PRESS_PLAN` binding order both sort on, `BySlot`/`ByGroup` are the derived indexes, and `MtlxInput` resolves the `.mtlx` port name from the binding row so the interchange document never carries a translation column.
- Packages: `plane#TEXTURE_PLANE` (composed — `PlaneTransfer`/`MipPolicy` the row columns select, `NormalConvention`/`ToGl` the green-polarity axis and its one decoded-texel flip), `codec#RASTER_CODEC` (composed — `KtxPayload` the per-row quality policy), `filter#PLANE_OP` (composed — `PlaneOp`/`HeightSolver`/`HeightDerivative`/`HeightEvidence`, the derivation each `Derived` row carries), `Rasm.Materials.Appearance.Surface` (composed — the `OpenPbrSurface` column set the lens reads), `Rasm.Materials.Appearance.Graph` (composed — `MaterialParameters` the lens writes, `ShadePoint`/`SurfaceShade` the geometric and sink lenses read, `PortId`/`PortValue` the slot binds), `Rasm.Materials.Appearance.Texture` (composed — `ShadeVec4` the one field register, `Channel` the sampler modality each `SinkSlot` names), `photometric#PHOTOMETRIC` (composed — `PhotometricQuantity` the light-quantity band a `ChannelUnit` row names, its `Ucum` column the wire token), UnitsNet (the `LengthUnit`/`LuminanceUnit` SI members the rows bind, admitted in-folder through `MaterialUnits` alone), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`, `Lazy<T>`).
- Growth: a new bakeable field is one `TextureChannel` row carrying its twelve columns — never a second roster, never a per-channel type, and never a caller-ordered tuple; a new packing order is one `ChannelPack` row naming its three slots; a new unit is one `ChannelUnit` row naming its UnitsNet SI member; a new `.mtlx` egress irregularity is one `MtlxBinding` case, so a unit fork, a shape split, or a lowered-into input stays a typed row rather than prose a transcriber must remember. The OpenPBR half grows BY DERIVATION and the projection is TOTAL — a column added to `OpenPbrSurface` earns a row whose `ColumnLens` reads it, and every OpenPBR row reads a real column, so no row is synthesized and no lens returns a constant.
- Boundary: channel values are DECODED and signed — `geometry_normal` neutral is the unit `(0,0,1)`, `geometry_tangent` the unit `(1,0,0)`, `curvature` the signed zero, `height` the normalized `0.5` — so the `(v+1)/2` integer encode and its `2v−1` decode live wholly at `plane#PLANE_VOCABULARY` and the conversion appears exactly once in the corpus; the composed `plane#TEXTURE_PLANE` `NormalConvention.ToGl` is therefore one green-sign flip over the decoded texel rather than a depth-branching pair, and the `dx` row converts ONCE at ingest so no plane leaves this page carrying `−Y` green. The `graph#MATERIAL_GRAPH` `Normal` arm reads the OPPOSITE convention — its decode is `2v−1` over a `[0,1]` tangent-space sample, the encoding `MaterialGraph.Default` seeds as `(0.5,0.5,1.0)` — so `SinkSlot.Encode` re-encodes the decoded plane texel at the bind and the two owners meet at exactly one projection column; binding a decoded normal straight onto the node inverts X and Y at every texel, which no gate downstream can see. Gloss is NOT a channel — `gloss`/`glossiness`/`smoothness` are `specular_roughness` ingest aliases whose `ClassifiedMap.Inverted` flag records the `roughness = 1 − gloss` inversion, applied by the `filter#PLANE_OP` `RemapCurve.Levels.Invert` curve in the LINEAR domain after the plane decodes, so an `srgb`-authored gloss plane inverted before decode (the silent-roughness fork) is unrepresentable and no downstream surface holds a gloss spelling. `MipPolicy.RoughnessVariance` is the roughness rows' declared law and it is PAIRED — `Pair` names the normal channel whose per-level variance the fold consumes, resolving to `geometry_coat_normal` for the coat group and `geometry_normal` elsewhere — so `press#TEXTURE_PRESS` reads the pairing off the row rather than guessing, and a roughness channel mipped under `Box` alone is a stated quality floor the press run records. The KTX payload column is a QUALITY POLICY, not a container choice — the corpus `appearance.proto` `KtxPayload` enum under its `Plane.ktx_payload` protovalidate rule is its wire-legality owner: vector channels take `KtxPayload.Uastc` because ETC1S destroys a normal's directional coherence, colour channels open at `KtxPayload.Etc1s` and raise to `Uastc` on a set-level quality floor, and `KtxPayload.RawBcn` never appears on a row because a raw-BCn KTX2 is a desktop payload no Basis-transcoding consumer reads. `base_specular_tint` and `transmission_roughness` are Rasm columns OpenPBR does not name, so their `MtlxBinding` is `Lowered("specular_color")` and `Unmapped` respectively, `thin_film_thickness` carries `Scaled(1e-3)` for the `.mtlx` micrometre input against its nanometre plane, and `subsurface_radius` carries `Split("subsurface_radius_scale")` for the radius-and-scale pair the document takes — each irregularity a row the transcriber cannot omit rather than a sentence it can.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Element.Composition;
using Rasm.Element.Projection;
using Rasm.Materials.Appearance.Bsdf;
using Rasm.Materials.Appearance.Graph;
using Rasm.Materials.Appearance.Photometric;
using Rasm.Materials.Appearance.Surface;
using Rasm.Materials.Appearance.Texture;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [TYPES] ---------------------------------------------------------------------------
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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChannelUnit {
    public static readonly ChannelUnit None       = new("none",  siUnit: None,                                          quantity: None,                               ucum: "1");
    public static readonly ChannelUnit Millimetre = new("mm",    siUnit: Some<Enum>(LengthUnit.Millimeter),             quantity: None,                               ucum: "mm");
    public static readonly ChannelUnit Nanometre  = new("nm",    siUnit: Some<Enum>(LengthUnit.Nanometer),              quantity: None,                               ucum: "nm");
    public static readonly ChannelUnit Luminance  = new("cd/m2", siUnit: Some<Enum>(LuminanceUnit.CandelaPerSquareMeter), quantity: Some(PhotometricQuantity.Luminance), ucum: PhotometricQuantity.Luminance.Ucum);

    public Option<Enum> SiUnit { get; }
    public Option<PhotometricQuantity> Quantity { get; }
    public string Ucum { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SinkSlot {
    public static readonly SinkSlot BaseColor = new("baseColor", portOrdinal: 1, modality: Channel.Color,  encode: Verbatim, read: static s => ShadeVec4.FromColor(s.BaseColorLinear));
    public static readonly SinkSlot Metalness = new("metalness", portOrdinal: 2, modality: Channel.Scalar, encode: Verbatim, read: static s => Lane(s.Metalness));
    public static readonly SinkSlot Roughness = new("roughness", portOrdinal: 3, modality: Channel.Scalar, encode: Verbatim, read: static s => Lane(s.Roughness));
    public static readonly SinkSlot Normal    = new("normal",    portOrdinal: 4, modality: Channel.Vector, encode: Bias,     read: static s => Axis(s.ShadingFrame.ZAxis));
    public static readonly SinkSlot Emission  = new("emission",  portOrdinal: 6, modality: Channel.Color,  encode: Verbatim, read: static s => ShadeVec4.FromColor(s.EmissionLinear));

    public PortId Port => PortId.Create(PortOrdinal);
    public int PortOrdinal { get; }
    public Channel Modality { get; }

    [UseDelegateFromConstructor]
    public partial PortValue Encode(PortValue decoded);
    [UseDelegateFromConstructor]
    public partial ShadeVec4 Read(SurfaceShade shade);

    static PortValue Verbatim(PortValue decoded) => decoded;

    static PortValue Bias(PortValue decoded) {
        Vector3d signed = decoded.AsVector;
        return new PortValue.Vector(new Vector3d((signed.X + 1.0) * 0.5, (signed.Y + 1.0) * 0.5, (signed.Z + 1.0) * 0.5));
    }

    static ShadeVec4 Lane(double scalar) => new(scalar, 0.0, 0.0, 1.0);
    static ShadeVec4 Axis(Vector3d axis) => new(axis.X, axis.Y, axis.Z, 1.0);
}

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

public sealed record ColumnLens(
    Func<OpenPbrSurface, ShadeVec4> Read,
    Option<Func<MaterialParameters, ShadeVec4, MaterialParameters>> Write);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChannelOrigin {
    private ChannelOrigin() { }
    public sealed record Shaded(ColumnLens Lens) : ChannelOrigin;
    public sealed record Geometric(Func<ShadePoint, ShadeVec4> Read) : ChannelOrigin;
    public sealed record Derived(string From, PlaneOp Fold) : ChannelOrigin;
}

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
    public static readonly TextureChannel SpecularColor         = new("specular_color",                group: ChannelGroup.Specular,     components: 3, transfer: PlaneTransfer.Srgb,   neutral: Rgb(1.0, 1.0, 1.0),     unit: ChannelUnit.None,       mip: MipPolicy.Kaiser,            payload: KtxPayload.Etc1s, origin: Column(static s => Rgb(s.SpecularColor),           static (p, v) => p with { SpecularColor = v.AsColorUnchecked() }),                             mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Seq("spec", "specular", "speccol"));
    public static readonly TextureChannel SpecularRoughness     = new("specular_roughness",            group: ChannelGroup.Specular,     components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.3),            unit: ChannelUnit.None,       mip: MipPolicy.RoughnessVariance, payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.SpecularRoughness),    static (p, v) => p with { Roughness = Unit(v.X) }),                                            mtlx: MtlxBinding.Verbatim,                            slot: SinkSlot.Roughness,    aliases: Seq("roughness", "rough", "rgh", "r"));
    public static readonly TextureChannel SpecularAnisotropy    = new("specular_roughness_anisotropy", group: ChannelGroup.Specular,     components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.SpecularAnisotropy),   static (p, v) => p with { Anisotropy = Unit(v.X) }),                                           mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel SpecularRotation      = new("specular_roughness_anisotropy_rotation", group: ChannelGroup.Specular, components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),        unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.SpecularRotation),     static (p, v) => p with { AnisotropyRotation = Unit(v.X) }),                                   mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel SpecularIor           = new("specular_ior",                  group: ChannelGroup.Specular,     components: 1, transfer: PlaneTransfer.Raw,    neutral: Scalar(1.5),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.None , origin: Column(static s => Scalar(s.SpecularIor),          static (p, v) => p with { Ior = v.X }),                                                        mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel TransmissionWeight    = new("transmission_weight",           group: ChannelGroup.Transmission, components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.TransmissionWeight),   static (p, v) => p with { Transmission = Unit(v.X) }),                                         mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Seq("transmission", "transmissive", "refraction"));
    public static readonly TextureChannel TransmissionRoughness = new("transmission_roughness",        group: ChannelGroup.Transmission, components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.None,       mip: MipPolicy.RoughnessVariance, payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.TransmissionRoughness), static (p, v) => p with { TransmissionRoughness = Unit(v.X) }),                               mtlx: MtlxBinding.Unmapped,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel SubsurfaceWeight      = new("subsurface_weight",             group: ChannelGroup.Subsurface,   components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.SubsurfaceWeight),     static (p, v) => p with { Subsurface = Unit(v.X) }),                                           mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Seq("sss", "subsurface", "scatter"));
    public static readonly TextureChannel SubsurfaceRadius      = new("subsurface_radius",             group: ChannelGroup.Subsurface,   components: 3, transfer: PlaneTransfer.Raw,    neutral: Rgb(1.0, 0.5, 0.25),    unit: ChannelUnit.Millimetre, mip: MipPolicy.Box,               payload: KtxPayload.None , origin: Column(static s => Band(s.SubsurfaceRadius),       static (p, v) => p with { SubsurfaceRadius = Rasm.Materials.Appearance.Graph.SubsurfaceRadius.Create(Math.Max(0.0, v.X), Math.Max(0.0, v.Y), Math.Max(0.0, v.Z)) }), mtlx: new MtlxBinding.Split("subsurface_radius_scale"), slot: None,             aliases: Empty);
    public static readonly TextureChannel CoatWeight            = new("coat_weight",                   group: ChannelGroup.Coat,         components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.CoatWeight),           static (p, v) => p with { Clearcoat = Unit(v.X) }),                                            mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Seq("clearcoat", "coat", "cc"));
    public static readonly TextureChannel CoatColor             = new("coat_color",                    group: ChannelGroup.Coat,         components: 3, transfer: PlaneTransfer.Srgb,   neutral: Rgb(1.0, 1.0, 1.0),     unit: ChannelUnit.None,       mip: MipPolicy.Kaiser,            payload: KtxPayload.Etc1s, origin: Column(static s => Rgb(s.CoatColor),               static (p, v) => p with { CoatColor = v.AsColorUnchecked() }),                                 mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel CoatRoughness         = new("coat_roughness",                group: ChannelGroup.Coat,         components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.None,       mip: MipPolicy.RoughnessVariance, payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.CoatRoughness),        static (p, v) => p with { ClearcoatRoughness = Unit(v.X) }),                                   mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel CoatIor               = new("coat_ior",                      group: ChannelGroup.Coat,         components: 1, transfer: PlaneTransfer.Raw,    neutral: Scalar(1.6),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.None , origin: Column(static s => Scalar(s.CoatIor)),                                                                                                           mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel FuzzWeight            = new("fuzz_weight",                   group: ChannelGroup.Fuzz,         components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.FuzzWeight),           static (p, v) => p with { Sheen = Unit(v.X) }),                                                mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Seq("sheen", "fuzz", "velvet"));
    public static readonly TextureChannel FuzzColor             = new("fuzz_color",                    group: ChannelGroup.Fuzz,         components: 3, transfer: PlaneTransfer.Srgb,   neutral: Rgb(1.0, 1.0, 1.0),     unit: ChannelUnit.None,       mip: MipPolicy.Kaiser,            payload: KtxPayload.Etc1s, origin: Column(static s => Rgb(s.FuzzColor),               static (p, v) => p with { FuzzColor = v.AsColorUnchecked() }),                                 mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel FuzzRoughness         = new("fuzz_roughness",                group: ChannelGroup.Fuzz,         components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.5),            unit: ChannelUnit.None,       mip: MipPolicy.RoughnessVariance, payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.FuzzRoughness)),                                                                                                     mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel ThinFilmWeight        = new("thin_film_weight",              group: ChannelGroup.ThinFilm,     components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: Column(static s => Scalar(s.ThinFilmWeight),       static (p, v) => p with { Film = ThinFilm.Create(Unit(v.X), p.Film.ThicknessNm, p.Film.Ior) }), mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);
    public static readonly TextureChannel ThinFilmThickness     = new("thin_film_thickness",           group: ChannelGroup.ThinFilm,     components: 1, transfer: PlaneTransfer.Raw,    neutral: Scalar(500.0),          unit: ChannelUnit.Nanometre,  mip: MipPolicy.Box,               payload: KtxPayload.None , origin: Column(static s => Scalar(s.ThinFilmThickness),    static (p, v) => p with { Film = ThinFilm.Create(p.Film.Weight, Math.Max(0.0, v.X), p.Film.Ior) }), mtlx: new MtlxBinding.Scaled(1e-3),                 slot: None,                  aliases: Empty);
    public static readonly TextureChannel ThinFilmIor           = new("thin_film_ior",                 group: ChannelGroup.ThinFilm,     components: 1, transfer: PlaneTransfer.Raw,    neutral: Scalar(1.4),            unit: ChannelUnit.None,       mip: MipPolicy.Box,               payload: KtxPayload.None , origin: Column(static s => Scalar(s.ThinFilmIor),          static (p, v) => p with { Film = ThinFilm.Create(p.Film.Weight, p.Film.ThicknessNm, Math.Max(1.0, v.X)) }), mtlx: MtlxBinding.Verbatim,                    slot: None,                  aliases: Empty);
    public static readonly TextureChannel EmissionColor         = new("emission_color",                group: ChannelGroup.Emission,     components: 3, transfer: PlaneTransfer.Srgb,   neutral: Rgb(1.0, 1.0, 1.0),     unit: ChannelUnit.None,       mip: MipPolicy.Kaiser,            payload: KtxPayload.Etc1s, origin: Column(static s => Rgb(s.EmissionColor),           static (p, v) => p with { Emission = v.AsColorUnchecked() }),                                  mtlx: MtlxBinding.Verbatim,                            slot: SinkSlot.Emission,     aliases: Seq("emissive", "emission", "glow", "e"));
    public static readonly TextureChannel EmissionLuminance     = new("emission_luminance",            group: ChannelGroup.Emission,     components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(0.0),            unit: ChannelUnit.Luminance,  mip: MipPolicy.Box,               payload: KtxPayload.None , origin: Column(static s => Scalar(s.EmissionLuminance),    static (p, v) => p with { EmissionLuminance = Math.Max(0.0, v.X) }),                           mtlx: MtlxBinding.Verbatim,                            slot: None,                  aliases: Empty);

    // --- [GEOMETRY_CHANNELS]
    public static readonly TextureChannel GeometryOpacity     = new("geometry_opacity",      group: ChannelGroup.Geometry, components: 1, transfer: PlaneTransfer.Linear, neutral: Scalar(1.0),        unit: ChannelUnit.None, mip: MipPolicy.Box,               payload: KtxPayload.Uastc, origin: new ChannelOrigin.Geometric(static _ => Scalar(1.0)),                              mtlx: MtlxBinding.Verbatim, slot: None,             aliases: Seq("opacity", "alpha", "mask", "transparency"));
    public static readonly TextureChannel GeometryNormal      = new("geometry_normal",       group: ChannelGroup.Geometry, components: 3, transfer: PlaneTransfer.Raw,    neutral: Rgb(0.0, 0.0, 1.0), unit: ChannelUnit.None, mip: MipPolicy.NormalRenormalize, payload: KtxPayload.Uastc, origin: new ChannelOrigin.Geometric(static _ => Rgb(0.0, 0.0, 1.0)),                       mtlx: MtlxBinding.Verbatim, slot: SinkSlot.Normal,  aliases: Seq("normal", "nor", "nrm", "n", "normalgl", "norgl", "nordx", "normaldx"));
    public static readonly TextureChannel GeometryCoatNormal  = new("geometry_coat_normal",  group: ChannelGroup.Geometry, components: 3, transfer: PlaneTransfer.Raw,    neutral: Rgb(0.0, 0.0, 1.0), unit: ChannelUnit.None, mip: MipPolicy.NormalRenormalize, payload: KtxPayload.Uastc, origin: new ChannelOrigin.Geometric(static _ => Rgb(0.0, 0.0, 1.0)),                       mtlx: MtlxBinding.Verbatim, slot: None,             aliases: Empty);
    public static readonly TextureChannel GeometryTangent     = new("geometry_tangent",      group: ChannelGroup.Geometry, components: 3, transfer: PlaneTransfer.Raw,    neutral: Rgb(1.0, 0.0, 0.0), unit: ChannelUnit.None, mip: MipPolicy.NormalRenormalize, payload: KtxPayload.Uastc, origin: new ChannelOrigin.Geometric(static p => Axis(p.Frame.XAxis)),                mtlx: MtlxBinding.Verbatim, slot: None,             aliases: Empty);
    public static readonly TextureChannel GeometryCoatTangent = new("geometry_coat_tangent", group: ChannelGroup.Geometry, components: 3, transfer: PlaneTransfer.Raw,    neutral: Rgb(1.0, 0.0, 0.0), unit: ChannelUnit.None, mip: MipPolicy.NormalRenormalize, payload: KtxPayload.Uastc, origin: new ChannelOrigin.Geometric(static p => Axis(p.Frame.XAxis)),                mtlx: MtlxBinding.Verbatim, slot: None,             aliases: Empty);

    // --- [DERIVED_CHANNELS]
    public static readonly TextureChannel Height    = new("height",    group: ChannelGroup.Derived, components: 1, transfer: PlaneTransfer.Raw,    neutral: Scalar(0.5), unit: ChannelUnit.None, mip: MipPolicy.Box, payload: KtxPayload.Uastc, origin: new ChannelOrigin.Derived("geometry_normal", new PlaneOp.HeightNormal(Inverse: true, HeightEvidence.Unit, HeightSolver.Spectral, HeightPolicy.Standard)), mtlx: MtlxBinding.Unmapped, slot: None, aliases: Seq("height", "disp", "displacement", "bump", "h"));
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

    public int Ordinal => Ranks.Value[this];

    public bool OpenScale =>
        Unit != ChannelUnit.None || Math.Abs(Neutral.X) > 1.0 || Math.Abs(Neutral.Y) > 1.0 || Math.Abs(Neutral.Z) > 1.0;

    public Option<string> Pair =>
        Mip == MipPolicy.RoughnessVariance
            ? Some(Group == ChannelGroup.Coat ? GeometryCoatNormal.Key : GeometryNormal.Key)
            : Option<string>.None;

    public Option<string> MtlxInput =>
        Mtlx.Switch(
            state:     Key,
            canonical: static (key, _) => Some(),
            scaled:    static (key, _) => Some(),
            split:     static (key, _) => Some(),
            lowered:   static (_, l) => Some(l.Input),
            absent:    static (_, _) => Option<string>.None);

    private static readonly Lazy<FrozenDictionary<TextureChannel, int>> Ranks =
        new(static () => Items.Select(static (row, index) => (Row: row, Index: index)).ToFrozenDictionary(static e => e.Row, static e => e.Index));

    private static readonly Lazy<FrozenDictionary<SinkSlot, TextureChannel>> Slots =
        new(static () => Items.Select(static c => c.Slot.Map(slot => (Slot: slot, Channel: c))).Somes().ToFrozenDictionary(static e => e.Slot, static e => e.Channel));

    private static readonly Lazy<ILookup<ChannelGroup, TextureChannel>> Groups =
        new(static () => Items.ToLookup(static row => row.Group));

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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChannelPack {
    public static readonly ChannelPack Orm = new("orm", slots: Seq(TextureChannel.Occlusion, TextureChannel.SpecularRoughness, TextureChannel.BaseMetalness), gltfLegal: true);
    public static readonly ChannelPack Mra = new("mra", slots: Seq(TextureChannel.BaseMetalness, TextureChannel.SpecularRoughness, TextureChannel.Occlusion), gltfLegal: false);
    public Seq<TextureChannel> Slots { get; }
    public bool GltfLegal { get; }

    public Option<int> Lane(TextureChannel channel) =>
        Slots.Choose((index, slot) => slot == channel ? Some(index) : Option<int>.None).Head;
}
```

## [03]-[TEXTURE_SET]

- Owner: `TextureSet` the extent-coherent content-keyed plane bundle; `UdimSheet` the ascending per-tile assembly the wire's `udim_tiles` column publishes, with `UdimResidency` its read policy; `TextureAtlas` the N-set packing product with `AtlasPlacement` its per-participant UV transform row; `LayerLaw` `[SmartEnum<string>]` the layer-cardinality axis; `UdimTile` `[ValueObject<int>]` the Mari tile index; `ChannelPackPlane` the packed-plane carrier over a `ChannelPack` row; `EgressSlot` `[Union]` the declared name subject; `EgressVariant` `[Union]` the one optional filename infix.
- Cases: layer-law {`none` (exactly one layer), `cubeFaces` (exactly six, square extent), `array`, `volume`, `frames`} · egress-variant {`Whole`, `Udim`, `Mip`, `Layer`}.
- Entry: `public static Fin<TextureSet> Of(TextureSetDraft draft)` is the ONE admission — a draft carries the raw bundle, `Of` runs the gate ladder and mints the content key, and no other construction path exists; `Egress(EgressSlot slot, EgressVariant variant, RasterFormat format)` renders the one egress leaf name for a channel and a pack alike over the declared `EgressSlot` cases, validating the requested variant's AXIS against the one axis the set occupies before it validates the variant's own bounds; `WithChannel`/`WithPack` re-admit through `Of` so a mutated set re-keys; `UdimSheet.Of(tiles)` assembles N single-tile sets into the one UDIM producer, proving tile uniqueness, vocabulary agreement, and roster agreement before the sheet keys over the ascending tile-key fold. The wire for both — the generated `Appearance.Set` with its per-tile plane-row repetition — mints at `Appearance/interchange#MATERIAL_WIRE` `AppearanceEgress.Set` over the corpus `appearance.proto` `Role` enum this roster bridges onto at `[04]`.
- Law: `Tiled` is EVIDENCE, never a flag — the column is the kernel `Evidence<TileProof>` and `tile#TILE_GATE` is the only surface that mints a proof, so a caller cannot assert tileability into a draft: an ingested or freshly pressed set carries `Absent` until the gate grades it, a graded set carries `Measured` with the proof's own `Accepted` as the acceptance read, and a grade whose spectral band rejected carries `Refused` with the band's own cause — the deleted `Option` read an ungraded ingest and a refused grade as one `None`. The wire's boolean `tiled` is the projection of the measured-and-accepted read, never its source.
- Packages: `plane#TEXTURE_PLANE` (composed — `TexturePyramid` carrying each channel's levels and its own `Key`, `ChannelDtype` the alpha-conversion floor read through `PlaneFormat.Normalizes`, `PlaneFormat.WebLegal` the storage-side wire-reach gate, `NormalConvention` the provenance column), `codec#RASTER_CODEC` (composed — `RasterFormat.Extension` the ONE `<ext>` source, `CodecCapability.WireLegal`/`BlockCompressed` membership on `KtxPayload.Traits` the payload gates, `KtxPayload.Transcodable` the discriminant deciding whether a reader sees the store's own format), `tile#TILE_GATE` (composed — `TileProof`), `Rasm.Element.Projection` (composed — `ContentAddress.Create` plus generated raw-key `ToValue()`), `Rasm.Element.Composition` (the CONTRACT `MaterialId`), `Rasm.Materials.Appearance.Surface` (`ConductorMetal` the set-level conductor row), `Rasm.Domain` (`ContentHash.Of` the ONE identity entry and `ContentHash.Hex` the canonical X32 spelling, `ValidityClaim`, `Evidence` the kernel probe evidence the `Tiled` column rides), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox (`Encoding.UTF8` the total preimage projection).
- Growth: a new layer modality is one `LayerLaw` row carrying its cardinality and extent predicates — cube maps, flipbooks, arrays, and volumes are rows, so a set shape never breaks for a new stacking; a new set-level fact is one `TextureSet` column and one `Of` gate; a new filename infix is one `EgressVariant` case, and a new container is one `codec#RASTER_CODEC` `RasterFormat` row the egress reads its extension off.
- Law: `TextureSet.Of` is the ONE gate and it REFUSES rather than repairs, its `Gates` sequence naming each step in ordinal order and stopping at the first refusal so a caller reads the narrowest true statement about its draft. It refuses a channel plane whose extent differs from the set's, a `pq` or `hlg` transfer on a channel plane (a bake target is scene-referred, and a display-referred bake forks the shading value from the stored value), a layer count or extent its `LayerLaw` row rejects, a set-level `AlphaMode` a channel's own `ChannelDtype` cannot convert to without catastrophic low-alpha quantization, a channel appearing both standalone and inside a pack, a duplicate pack row, a pack plane that is not four-component `raw` `AlphaMode.None`, a declared `height` scale with no `height` channel to scale, two of the three declared variant axes occupied at once, and an empty channel map — each refusing `MaterialFault.Parameter` with the offending channel key in the reason.
- Law: WIRE REACH gates as a PAIR and never either half alone. Supercompression leaves a KTX2's declared Vulkan format undefined until transcode, hiding the store class from a reader; an untranscoded payload publishes that store's own format, and the browser read path resolves no row for a narrow 16-bit UNORM one — so `plane#PLANE_FORMAT` `WebLegal` and `codec#RASTER_CODEC` `KtxPayload.Transcodable` gate together, the PRODUCER re-routing its store rather than the consumer transcoding. Refusing `r16`/`rg16` outright would foreclose the production depth those rows exist for; admitting them untranscoded ships a file no consumer opens.
- Law: THE OPEN-SCALE GATE is the sibling refusal and it catches a VALUE rather than a container. An index of refraction, a nanometre film thickness, a millimetre scatter radius, and a luminance all carry values above one, and a normalizing store clips every one at white with no diagnostic on the path — the texel, the plane, and the container are all legal and the shading input is silently wrong. `TextureChannel.OpenScale` derives that fact from two columns a row already declares (a physical `ChannelUnit`, or a neutral outside the normalized band), so an open-scale channel admits a FLOAT store alone and names `KtxPayload.None` alone — every block class stages `Unorm8` by the codec's own measured bound. The six affected rows carry that payload, and a packed lane refuses an open-scale channel at every store because the pack's own four-component normalized shape is what would clip it.
- Law: `Convention` is INGEST-SOURCE PROVENANCE the wire's `normal_convention` column records — the planes are always `gl` by construction of the two mints, since the press bakes `gl` natively and the `SetIngest.Draft` lift converts a `dx` source ONCE before any plane is keyed — so a `dx` value here names where the bytes CAME from, never what they carry. The band split is by CONCERN: appearance-domain admission fails band-2450 `MaterialFault` and raster-mechanical failure fails band-2460 `codec#RASTER_FAULT` `RasterFault`, so a set admission fault never wears a raster code.
- Law: the DOCUMENT KEY is a `ContentHash.Of` fold at seed zero over canonically-ordered pieces — channels by `TextureChannel.Ordinal`, packs by `ChannelPack.Items` row order, never map-enumeration or authoring order — so the same bundle in any authoring order keys identically and the preimage is stable against a roster APPEND. Every piece frames through the ONE kernel writer — `String` for a channel or pack key, `U128` for a plane key, `Optional`/`Double` for the millimetre span — so no separator-joined UTF-8 piece exists to re-split and no buffer exists to truncate. The key reads the plane digests and the declared `HeightScaleMm`, and reads NOTHING else: not the extent, the material id, the tile proof, or the provenance. It identifies the set document alone; each stored payload keeps its own `LevelEgress.Blob`, which becomes that level's generated `PlaneRef.digest`, so no set key aliases a plane address.
- Law: `HeightScaleMm` is `Option<double>` and ABSENCE IS SPELLED. The millimetre span is a DECLARATION, not a measurement, so a set whose displacement amplitude nothing has stated carries absence and the preimage appends `hs:none` where a declared span appends its own round-trip value. A `0.0` standing for "absent" was two states wearing one number — a declaration of no relief and a missing declaration are different facts, and the gate that read the sentinel could not tell them apart. The plane itself stays normalized `[0,1]`, so a rescale re-keys the scale fragment and never a texel. `filter#PLANE_OP` `HeightEvidence.ScaleMm` stays a REQUIRED double on the other side of that line: it is measured evidence of an integration that ran, and an integration that ran had an amplitude.
- Law: an ATLAS is a PLANE-LEVEL sharing fact and it is now PRODUCED here. `TextureAtlas.Of` folds N participants onto one shared sheet and hands each a `AtlasPlacement` UV transform row; every participant keeps its own `TextureSet`, its own key, and its own appearance identity, and reaches the sheet BY CONTENT ADDRESS — so a texture edit re-keys exactly the sets that read it and never their sheet-mates. A set-level merge behind one appearance key stays the deleted form. Charts arrive ALREADY FLATTENED as data, because chart packing is the kernel's solved problem and a second packer here could answer differently from the one a mesh-space bake already used; the gutter closes through `filter#PLANE_OP` `Dilate` as part of producing the sheet, since a bilinear tap straddling two charts reads its neighbour and every mip level widens that bleed.
- Law: the EGRESS GRAMMAR is `materials/texture/<key>/<channel>[.<variant>].<ext>` with `<key>` the set key rendered once through `ContentHash.Hex(ContentAddress.Create(Key).ToValue())` at name construction — never at the wire and never at admission — so the canonical lowercase 32-digit value is shared by the path and wire boundary. The variant slot admits AT MOST ONE of a four-digit UDIM tile, a two-digit mip index, or a two-digit layer index.
- Boundary: variant exclusivity is enforced TWICE for two different reasons and neither gate stands for the other — `Of` refuses a SET occupying two axes because that shape has no grammar to name it, and `Egress` refuses a REQUEST whose variant axis is not the axis its set occupies because that filename cannot be ordered. The layer × mip product is the case a caller reaches for most, since a mip-chained cube map is an ordinary asset, and its sanctioned carriage is the KTX2 CONTAINER holding the whole layer × level array in one file: the `codec#RASTER_CODEC` `Ktx2` row is the format, its `KtxGate` `--levels` argument writes the chain, and the `mip` variant's own `Ktx2` refusal states the same fact from the other side.

```csharp
using System.Globalization;

// --- [TYPES] ---------------------------------------------------------------------------
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

[ValueObject<int>]
public readonly partial struct UdimTile {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) {
        if (value is < 1001 or > 2000)
            validationError = new ValidationError($"<udim-out-of-mari-range:{value}>");
    }

    public int Column => (Value - 1001) % 10 + 1;
    public int Row => (Value - 1001) / 10 + 1;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EgressSlot {
    private EgressSlot() { }
    public sealed record Channel(TextureChannel Row) : EgressSlot;
    public sealed record Pack(ChannelPack Row) : EgressSlot;

    public string Name => Switch(channel: static c => c.Row.Key, pack: static p => p.Row.Key);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EgressVariant {
    private EgressVariant() { }
    public sealed record Whole() : EgressVariant;
    public sealed record Udim(UdimTile Tile) : EgressVariant;
    public sealed record Mip(int Level) : EgressVariant;
    public sealed record Layer(int Index) : EgressVariant;

    public static readonly EgressVariant Single = new Whole();

    public Option<string> Axis => Switch(
        whole: static _ => Option<string>.None,
        udim:  static _ => Some("udim"),
        mip:   static _ => Some("mip"),
        layer: static _ => Some("layer"));

    public string Infix => Switch(
        whole: static _ => string.Empty,
        udim:  static u => string.Create(CultureInfo.InvariantCulture, $".{u.Tile.Value:D4}"),
        mip:   static m => string.Create(CultureInfo.InvariantCulture, $".{m.Level:D2}"),
        layer: static l => string.Create(CultureInfo.InvariantCulture, $".{l.Index:D2}"));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ChannelPackPlane(ChannelPack Pack, TexturePyramid Plane, Seq<TextureChannel> Present) {
    public Seq<bool> Flags => Pack.Slots.Map(Present.Contains);
}

public sealed record TextureSetDraft(
    Dimension Width, Dimension Height, Dimension Layers, LayerLaw Law,
    NormalConvention Convention, AlphaMode Alpha, Option<double> HeightScaleMm, Evidence<TileProof> Tiled,
    Seq<UdimTile> Udim, HashMap<TextureChannel, TexturePyramid> Channels, Seq<ChannelPackPlane> Packs,
    Option<ConductorMetal> Conductor, Option<MaterialId> Material);

public sealed record TextureSet(
    Dimension Width, Dimension Height, Dimension Layers, LayerLaw Law,
    NormalConvention Convention, AlphaMode Alpha, Option<double> HeightScaleMm, Evidence<TileProof> Tiled,
    Seq<UdimTile> Udim, HashMap<TextureChannel, TexturePyramid> Channels, Seq<ChannelPackPlane> Packs,
    Option<ConductorMetal> Conductor, Option<MaterialId> Material, UInt128 Key) : IValidityEvidence {

    public static Fin<TextureSet> Of(TextureSetDraft draft) =>
        Gates(draft)
            .TraverseM(static gate => gate()).As()
            .Map(_ => new TextureSet(draft.Width, draft.Height, draft.Layers, draft.Law, draft.Convention, draft.Alpha,
                draft.HeightScaleMm, draft.Tiled, toSeq(draft.Udim.OrderBy(static t => t.Value)), draft.Channels,
                draft.Packs, draft.Conductor, draft.Material, Mint(draft)));

    // --- [SET_ADMISSION]
    static Seq<Func<Fin<Unit>>> Gates(TextureSetDraft draft) =>
        Seq<Func<Fin<Unit>>>(
            () => guard(!draft.Channels.IsEmpty || !draft.Packs.IsEmpty, new MaterialFault.Parameter("<texture-set-empty>")),
            () => guard(draft.Law.Admits(draft.Layers.Value), new MaterialFault.Parameter($"<layer-law-rejects:{draft.Law.Key}:{draft.Layers.Value}>")),
            () => guard(!draft.Law.Square || draft.Width == draft.Height, new MaterialFault.Parameter($"<layer-law-needs-square:{draft.Law.Key}:{draft.Width.Value}x{draft.Height.Value}>")),
            () => guard(
                ((draft.Udim.IsEmpty ? 0 : 1)
                    + (draft.Layers.Value is 1 ? 0 : 1)
                    + (draft.Channels.Values.Exists(static p => p.Levels.Count > 1) || draft.Packs.Exists(static p => p.Plane.Levels.Count > 1) ? 1 : 0)) <= 1,
                new MaterialFault.Parameter("<variant-slot-double-occupied>")),
            () => guard(draft.Packs.Map(static p => p.Pack).Distinct().Count() == draft.Packs.Count, new MaterialFault.Parameter("<pack-duplicate-row>")),
            () => guard(draft.HeightScaleMm.ForAll(static mm => double.IsFinite(mm) && mm > 0.0),
                new MaterialFault.Parameter($"<height-scale-invalid:{draft.HeightScaleMm.Map(static mm => mm.ToString("R", CultureInfo.InvariantCulture)).IfNone("none")}>")),
            () => guard(draft.HeightScaleMm.IsNone || draft.Channels.ContainsKey(TextureChannel.Height),
                new MaterialFault.Parameter("<height-scale-without-height-channel>")),
            () => toSeq(draft.Channels.AsIterable()).TraverseM(pair => AdmitChannel(draft, pair.Value)).As().Map(_ => unit),
            () => draft.Packs.TraverseM(pack => AdmitPack(draft, pack)).As().Map(_ => unit));

    static Fin<Unit> AdmitChannel(TextureSetDraft draft, TextureChannel channel, TexturePyramid pyramid) =>
        from _ in guard(pyramid.Base.Width == draft.Width && pyramid.Base.Height == draft.Height, new MaterialFault.Parameter($"<channel-extent-mismatch:{channel.Key}>"))
        from __ in guard(pyramid.Base.Layers == draft.Layers, new MaterialFault.Parameter($"<channel-layer-mismatch:{channel.Key}>"))
        from ___ in guard(pyramid.Base.Transfer.SceneReferred, new MaterialFault.Parameter($"<display-referred-channel-plane:{channel.Key}:{pyramid.Base.Transfer.Key}>"))
        from ____ in guard(pyramid.Base.Format.Components >= channel.Components, new MaterialFault.Parameter($"<channel-components-narrow:{channel.Key}>"))
        from _____ in guard(pyramid.Base.Alpha.Convertible(draft.Alpha, pyramid.Base.Format.Depth), new MaterialFault.Parameter($"<alpha-crossing-quantizes:{channel.Key}:{pyramid.Base.Format.Key}>"))
        from ______ in guard(channel.Payload.Traits.Admits(CodecCapability.WireLegal), new MaterialFault.Parameter($"<channel-payload-not-wire-legal:{channel.Key}:{channel.Payload.Key}>"))
        from _______ in guard(channel.Payload.Transcodable || pyramid.Base.Format.WebLegal, new MaterialFault.Parameter($"<channel-store-unreachable-on-wire:{channel.Key}:{pyramid.Base.Format.Key}>"))
        from ________ in guard(!draft.Packs.Exists(p => p.Present.Contains(channel)), new MaterialFault.Parameter($"<channel-both-packed-and-standalone:{channel.Key}>"))
        from _________ in guard(!channel.OpenScale || !PlaneFormat.Normalizes(pyramid.Base.Format.Depth),
            new MaterialFault.Parameter($"<open-scale-channel-normalizing-store:{channel.Key}:{pyramid.Base.Format.Key}>"))
        from __________ in guard(!channel.OpenScale || !channel.Payload.Traits.Admits(CodecCapability.BlockCompressed),
            new MaterialFault.Parameter($"<open-scale-channel-block-payload:{channel.Key}:{channel.Payload.Key}>"))
        select unit;

    static Fin<Unit> AdmitPack(TextureSetDraft draft, ChannelPackPlane pack) =>
        from _ in guard(pack.Plane.Base.Width == draft.Width && pack.Plane.Base.Height == draft.Height, new MaterialFault.Parameter($"<pack-extent-mismatch:{pack.Pack.Key}>"))
        from __ in guard(pack.Plane.Base.Format.Components is 4, new MaterialFault.Parameter($"<pack-plane-not-four-component:{pack.Pack.Key}>"))
        from ___ in guard(pack.Plane.Base.Transfer == PlaneTransfer.Raw, new MaterialFault.Parameter($"<pack-plane-not-raw:{pack.Pack.Key}>"))
        from ____ in guard(pack.Plane.Base.Alpha == AlphaMode.None, new MaterialFault.Parameter($"<pack-plane-carries-alpha:{pack.Pack.Key}>"))
        from _____ in guard(!pack.Present.IsEmpty, new MaterialFault.Parameter($"<pack-plane-no-present-slot:{pack.Pack.Key}>"))
        from ______ in guard(pack.Present.ForAll(pack.Pack.Slots.Contains), new MaterialFault.Parameter($"<pack-slot-foreign-channel:{pack.Pack.Key}>"))
        from _______ in guard(pack.Present.ForAll(static c => !c.OpenScale), new MaterialFault.Parameter($"<pack-slot-open-scale-channel:{pack.Pack.Key}>"))
        select unit;

    static UInt128 Mint(TextureSetDraft draft) =>
        ContentHash.Of(draft, static (source, writer) => writer
            .Optional(source.HeightScaleMm, static (mm, span) => span.Double(mm))
            .Rows(toSeq(TextureChannel.Items).Choose(channel => source.Channels.Find(channel).Map(pyramid => (Channel: channel, Pyramid: pyramid))),
                static (entry, row) => row.String(entry.Channel.Key).U128(entry.Pyramid.Key))
            .Rows(toSeq(ChannelPack.Items).Choose(pack => source.Packs.Find(seated => seated.Pack == pack)),
                static (pack, row) => row.String(pack.Pack.Key).U128(pack.Plane.Key)));

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(Channels.Count + Packs.Count, 1),
        ValidityClaim.WhenPresent(HeightScaleMm, ValidityClaim.Positive),
        Law.Admits(Layers.Value),
        ValidityClaim.Evidence(Tiled.Value()));

    public Fin<string> Egress(EgressSlot slot, EgressVariant variant, RasterFormat format) =>
        from _ in variant.Axis.Match(
            Some: axis => guard(Occupied == Some(axis),
                new MaterialFault.Parameter($"<egress-variant-axis:{slot.Name}:{axis}:{Occupied.IfNone("none")}>")),
            None: () => guard(Occupied.IsNone, new MaterialFault.Parameter($"<egress-whole-on-variant-set:{slot.Name}:{Occupied.IfNone("none")}>")))
        from __ in variant.Switch(
            whole: _ => Fin.Succ(unit),
            udim:  u => guard(Udim.Exists(tile => tile == u.Tile), new MaterialFault.Parameter($"<egress-udim-foreign-tile:{slot.Name}:{u.Tile.Value}>")),
            mip:   m => format == RasterFormat.Ktx2
                ? Fin.Fail<Unit>(new MaterialFault.Parameter($"<ktx2-leaf-carries-own-pyramid:{slot.Name}>"))
                : guard(Channels.Values.Exists(static p => p.Levels.Count > 1) || Packs.Exists(static p => p.Plane.Levels.Count > 1), new MaterialFault.Parameter($"<egress-mip-on-flat-set:{slot.Name}:{m.Level}>")),
            layer: l => guard(Layers.Value > 1 && l.Index < Layers.Value, new MaterialFault.Parameter($"<egress-layer-out-of-range:{slot.Name}:{l.Index}>")))
        select $"materials/texture/{ContentHash.Hex(ContentAddress.Create(Key).ToValue())}/{slot.Name}{variant.Infix}.{format.Extension}";

    Option<string> Occupied =>
        !Udim.IsEmpty ? Some("udim")
        : Layers.Value > 1 ? Some("layer")
        : Channels.Values.Exists(static p => p.Levels.Count > 1) || Packs.Exists(static p => p.Plane.Levels.Count > 1) ? Some("mip")
        : Option<string>.None;

    public Fin<TextureSet> WithChannel(TextureChannel channel, TexturePyramid pyramid) =>
        Of(new TextureSetDraft(Width, Height, Layers, Law, Convention, Alpha, HeightScaleMm, Tiled, Udim,
            Channels.AddOrUpdate(channel, pyramid), Packs, Conductor, Material));

    public Fin<TextureSet> WithPack(ChannelPackPlane pack) =>
        Of(new TextureSetDraft(Width, Height, Layers, Law, Convention, Alpha, HeightScaleMm, Tiled, Udim,
            Channels.Filter((c, _) => !pack.Present.Contains(c)), Packs.Add(pack), Conductor, Material));
}

public sealed record UdimResidency(ResidencyPolicy Policy, long TexelBudget) {
    public static readonly UdimResidency Whole = new(ResidencyPolicy.Retain, long.MaxValue);
    public bool Streams => Policy.Evicts;
}

public sealed record UdimSheet(
    Seq<UdimTile> Declared, Seq<(UdimTile Tile, TextureSet Set)> Tiles, UInt128 Key, UdimResidency Residency) {
    public static Fin<UdimSheet> Of(Seq<(UdimTile Tile, TextureSet Set)> tiles) =>
        from _ in guard(!tiles.IsEmpty, new MaterialFault.Parameter("<udim-sheet-empty>"))
        from __ in guard(tiles.Map(static t => t.Tile.Value).Distinct().Count() == tiles.Count, new MaterialFault.Parameter("<udim-sheet-duplicate-tile>"))
        from ___ in guard(tiles.ForAll(static t => t.Set.Udim.IsEmpty && t.Set.Layers.Value is 1), new MaterialFault.Parameter("<udim-sheet-tile-carries-variant>"))
        from ____ in guard(tiles.Map(static t => (t.Set.Convention, t.Set.Alpha, t.Set.Law)).Distinct().Count() is 1, new MaterialFault.Parameter("<udim-sheet-vocabulary-divergent>"))
        from _____ in guard(tiles.Map(Roster).Distinct().Count() is 1, new MaterialFault.Parameter("<udim-sheet-roster-divergent>"))
        let ordered = toSeq(tiles.OrderBy(static t => t.Tile.Value))
        select new UdimSheet(ordered.Map(static t => t.Tile), ordered, Mint(ordered), UdimResidency.Whole);

    public Seq<UdimTile> Resident => Tiles.Map(static t => t.Tile);

    public UdimSheet Streaming(UdimResidency residency) =>
        this with { Residency = residency, Tiles = residency.Streams ? Seq<(UdimTile, TextureSet)>() : Tiles };

    public Fin<(UdimSheet Sheet, TextureSet Set)> Resolve(UdimTile tile, Func<UdimTile, Fin<TextureSet>> mint) =>
        Tiles.Find(row => row.Tile == tile).Match(
            Some: row => Fin.Succ((this, row.Set)),
            None: () => Declared.Exists(row => row == tile)
                ? mint(tile).Map(seated => (Seat(tile, seated), seated))
                : new MaterialFault.Parameter($"<udim-sheet-foreign-tile:{tile.Value}>"));

    UdimSheet Seat(UdimTile tile, TextureSet seated) =>
        Residency.Policy.Evicts
            ? this with { Tiles = Retained(Cost(seated)).Add((tile, seated)) }
            : this with { Tiles = Tiles.Add((tile, seated)) };

    Seq<(UdimTile Tile, TextureSet Set)> Retained(long incoming) =>
        Tiles.Fold((Held: 0L, Kept: Seq<(UdimTile, TextureSet)>()), (state, row) =>
            state.Held + Cost(row.Set) + incoming > Residency.TexelBudget
                ? state
                : (state.Held + Cost(row.Set), state.Kept.Add(row))).Kept;

    static long Cost(TextureSet set) =>
        set.Channels.Values.Fold(0L, static (sum, chain) => sum + chain.Texels)
        + set.Packs.Fold(0L, static (sum, pack) => sum + pack.Plane.Texels);

    static string Roster((UdimTile Tile, TextureSet Set) tile) =>
        string.Join('|', TextureChannel.Items.Where(c => tile.Set.Channels.ContainsKey(c)).Select(static c => c.Key)
            .Concat(ChannelPack.Items.Where(row => tile.Set.Packs.Exists(p => p.Pack == row)).Select(static row => row.Key)));

    static UInt128 Mint(Seq<(UdimTile Tile, TextureSet Set)> ordered) =>
        ContentHash.Of(ordered, static (tiles, writer) =>
            writer.Rows(tiles, static (tile, row) => row.Ordinal(tile.Tile.Value).U128(tile.Set.Key)));
}

// --- [ATLAS]
public readonly record struct AtlasPlacement(UInt128 SetKey, UnitInterval OffsetU, UnitInterval OffsetV, UnitInterval ScaleU, UnitInterval ScaleV) {
    public UvFrame Frame => new(OffsetU.Value, OffsetV.Value, ScaleU.Value, ScaleV.Value, Rotation: 0.0);
}

public sealed record TextureAtlas(TextureSet Sheet, Seq<AtlasPlacement> Placements) {
    public static Fin<TextureAtlas> Of(
        Seq<(TextureSet Set, AtlasPlacement Placement)> participants, TextureSetDraft sheet, int gutterRings) =>
        from _ in guard(!participants.IsEmpty, new MaterialFault.Parameter("<atlas-no-participants>"))
        from __ in guard(participants.Map(static p => p.Placement.SetKey).Distinct().Count() == participants.Count,
            new MaterialFault.Parameter("<atlas-duplicate-participant>"))
        from ___ in guard(participants.ForAll(static p => p.Placement.SetKey == p.Set.Key),
            new MaterialFault.Parameter("<atlas-placement-key-mismatch>"))
        from ____ in guard(gutterRings > 0, new MaterialFault.Parameter($"<atlas-gutter-rings:{gutterRings}>"))
        from bound in TextureSet.Of(sheet, key)
        select new TextureAtlas(bound, participants.Map(static p => p.Placement));
}
```

## [04]-[SET_INGEST]

- Owner: `SetIngest` the classification fold, the python-wire decode boundary, the Element contract-roster admission, AND the draft lift; `PlaneProbe` the per-file evidence row; `IngestRefusal` `[SmartEnum<string>]` the closed reason axis every unresolved stem carries; `IngestSource` `[Union]` the classification input (directory probes · declared manifest · decoded python `Appearance.Set`); `WireVocabulary` the one bridge per closed Raster vocabulary onto the generated `appearance.proto` enums, derived from each row's key and proved total at type init; `WireLimits` the declared parse ceilings the appearance and declaration documents cross; `ClassifiedMap` the resolved row; `RosterBinding` the per-channel binding-policy row the contract roster lifts; `SetManifest` the accumulating result and its monoid.
- Entry: `public static SetManifest Classify(IngestSource source)` is TOTAL and PURE — it never reads a file, never faults, and never infers past its arm's law; every unclaimed stem accumulates into `Unresolved` as a TYPED `(IngestRefusal Reason, string Detail)` pair, and the caller decides whether an incomplete manifest is admissible for its purpose. `public static (IngestSource Source, Seq<RosterBinding> Binding) Roster(TextureRoster roster, Seq<PlaneProbe> probes)` is the Element contract-roster admission. `public static Fin<IngestSource> Peer(ReadOnlyMemory<byte> wire)` parses the generated `set.proto` `Set`, admits it once, and accepts only the structural `pbr` arm; baked and environment products are not classification inputs. `Draft` lifts the admitted manifest into texture drafts.
- Growth: a new alias is one entry on its channel's `Aliases` column — the resolver index DERIVES from `TextureChannel.Items`, so no second table exists to drift; a new packing token is one `ChannelPack` row; a new variant grammar is one token predicate in `Tokenize`; a new refusal shape is one `IngestRefusal` row and the mint site that hands it, so the reason axis stays bounded where a formatted token was unbounded; a new ingest-time repair is one arm in the lift's per-map conversion, never a second lift.
- Law: PROVENANCE AND LICENCE are INGEST EVIDENCE, carried on `SetManifest` and `IngestIntent` as an `IngestProvenance` row and interpreted nowhere on this page. A licence class is the `Appearance/neural` `ModelCard.LicenseClass` band's vocabulary and this stratum names no frontier type, so the DECLARED token crosses UP and the frontier bands it — the same posture the primaries axis takes toward a container's declared chromaticity, recorded and never converted. Absence is honest and never a grant. It folds by FIRST evidence exactly as the convention does, and it never enters a set key: two providers shipping byte-identical planes address ONE blob, and their grants are facts about the acquisition rather than about the bytes. That is also the whole module-side landing a text-to-material SERVICE product needs — the existing `Stems` and `Declared` arms already accept a service's files under the same alias law, so no service-source `IngestSource` case exists and none is owed.
- Law: the UV frame is a BINDING-time consumer fact and never a set payload. `texture#TEXTURE_UV` `SamplerState.Frame` carries it as a `UvFrame`, applied once inside `TextureUv.Sample`, so no `ClassifiedMap` column, no `TextureSet` column, and no wire field receives an offset, a scale, or a rotation — a per-tiling column inside the set forks the content key per consumer and destroys exactly the plane-level dedup the atlas boundary buys. `Rasm.Bim`'s `UvTransform` is NOT a `STRATA_TWIN` of `UvFrame`: it carries the `IfcCartesianTransformationOperator2D` decode Materials must never see, so it lowers onto the Element `TextureRoster` contract row's neutral frame columns at the Bim mint, and `SetIngest.Roster` lifts those columns onto `RosterBinding` rows BESIDE the manifest — binding policy the caller hands `SamplerState`, never a manifest or set column — so the Element row is the one place the two owners meet and neither package's transform name crosses the other.
- Boundary: classification is ALIAS-DRIVEN and the probe is EVIDENCE, never an inference source. A stem resolves by its tokens: separators `-`, `_`, `.`, and space all fold to one boundary, matching is case-insensitive, and the canonical key, its separator-stripped token form, and every row alias index into one `FrozenDictionary` derived from the roster behind a `Lazy` accessor — an eager index over another type's `Items` is the materialization race the accessor forecloses. Every refusal carries its `IngestRefusal` ROW beside the detail the mint site already builds, so the reason is a bounded dimension an operator series keys on while the stem, the role, and the format stay the unbounded detail text — a formatted token alone gives a counter file-name cardinality, and a `claimed`/`unresolved` boolean loses the whole operator answer, which is WHY a vendor library's maps did not classify. A stem carrying NEITHER a `gl` nor a `dx` token leaves `Convention` UNRESOLVED — the probe's green statistics are recorded on the row and never promoted to a default, because a defaulted convention is the silent-lighting-inversion defect that survives every downstream check and only surfaces as wrongly-lit geometry. `gloss`, `glossiness`, and `smoothness` resolve to `specular_roughness` with `Inverted` set, and the `filter#PLANE_OP` `RemapCurve.Levels.Invert` curve applies that inversion in the LINEAR domain — this page holds the FLAG and never the arithmetic; `arm` is an `orm` alias (identical slot order) and `mra` the reversed pack, so a packed stem resolves to a `ChannelPack` row carrying EVERY slot channel it covers rather than one arbitrary member, because a pack resolved to a single channel silently drops two thirds of the plane; a four-digit token at 1001 or above claims the UDIM variant slot. The probe REFUTES rather than proposes: a stem claiming a three-component channel over a single-component plane, or a claimed pack over a plane narrower than four components, drops to `Unresolved` with the contradiction recorded — a classification that survives its own evidence is the only one this fold emits. The two DECLARED columns the source file itself carries — its colour primaries and its EXIF orientation — refute the same way and promote nothing: a declared working space contradicts a role's assumed one and a declared rotation contradicts a set's assumed axis, each landing in `Unresolved`, neither defaulting, exactly as an unresolved `NormalConvention` never defaults; they arrive filled by the app-root scan through a header read that decodes no plane, so `Classify` gains evidence without gaining a file read and stays TOTAL and PURE. The `Peer` arm re-runs every alias, pack, and UDIM law over the foreign manifest rather than trusting its rows, and the `Declared` arm VALIDATES its already-resolved rows rather than re-inferring from stems — each map's channel keys re-resolve through the same roster-derived index (idempotence is the check, a retired key accumulates typed) and every probe refutation re-runs (a channel wider than its plane, a pack over a narrow plane), because a declared row's channel identity is its producer's bounded vocabulary while a stem is a filename guess, and re-inferring a filename would silently discard the declaration the arm exists to carry — so a declared manifest and a peer manifest are both inputs to classification and never substitutes for it; a peer's `tiled` declaration reaches no `TextureSet` from here, since tileability is `tile#TILE_GATE` evidence a set earns by grading; the peer wire's `height_scale` and `alpha_mode` are caller facts the app root lifts into `IngestIntent` beside the fetched plane bytes, so the lift's arity holds and no wire field bypasses the intent record. The LIFT is the one ingest mint and the one CONVERSION SITE: a `dx` source flips green through `NormalConvention.ToGl` over every level of every normal-convention plane — per-texel and linear, so each level stays its own fold — and an `Inverted` gloss map runs `filter#PLANE_OP` `RemapCurve.Levels.Invert` per level, BEFORE any plane is keyed, so the wire's frozen always-`gl` law holds by construction and the draft's `Convention` column records the SOURCE as provenance; per tile, every classified plane must agree on extent (the probe evidence re-enters here), a classified stem with no supplied plane faults by name, and the tile grouping hands one draft per tile so a UDIM directory lifts into the `UdimSheet.Of` assembly rather than collapsing onto one tile.

```csharp
using System.IO;
using CommunityToolkit.HighPerformance;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Rasm.AppHost.Runtime;
using Rasm.Element.Projection;
using Rasm.Materials.Raster;
using Riok.Mapperly.Abstractions;
// Contracts are retired from this logic.

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IngestRefusal {
    public static readonly IngestRefusal ConventionDivergent = new("conventionDivergent");
    public static readonly IngestRefusal PeerRoleDivergent   = new("peerRoleDivergent");
    public static readonly IngestRefusal PeerRowVocabulary   = new("peerRowVocabulary");
    public static readonly IngestRefusal PeerPackVocabulary  = new("peerPackVocabulary");
    public static readonly IngestRefusal PackPlaneNarrow     = new("packPlaneNarrow");
    public static readonly IngestRefusal ComponentsRefuted   = new("componentsRefuted");
    public static readonly IngestRefusal Unclaimed           = new("unclaimed");
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct PlaneProbe(
    string Stem, PlaneFormat Format, PlaneTransfer Transfer, Dimension Width, Dimension Height,
    ShadeVec4 Mean, ShadeVec4 Variance, Option<PlanePrimaries> DeclaredPrimaries, Option<int> ExifOrientation);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IngestSource {
    private IngestSource() { }
    public sealed record Stems(Seq<PlaneProbe> Probes) : IngestSource;
    public sealed record Declared(SetManifest Manifest) : IngestSource;

    public sealed record Peer(Wire.Set Manifest) : IngestSource;
}

public sealed record ClassifiedMap(
    Seq<TextureChannel> Channels, string Stem, Option<UdimTile> Tile, Option<ChannelPack> Pack,
    ChannelPolarity Polarity, PlaneProbe Probe);

public sealed record RosterBinding(TextureChannel Channel, UvFrame Frame, TextureWrap WrapU, TextureWrap WrapV, int CoordinateSet);

public sealed record SetManifest(
    Seq<ClassifiedMap> Maps, Seq<(IngestRefusal Reason, string Detail)> Unresolved, Option<NormalConvention> Convention,
    Seq<UdimTile> Udim, Option<IngestProvenance> Provenance = default) {
    public static readonly SetManifest Empty = new(Seq<ClassifiedMap>(), Seq<(IngestRefusal, string)>(), Option<NormalConvention>.None, Seq<UdimTile>());

    public SetManifest Combine(SetManifest other) =>
        new(Maps + other.Maps,
            Unresolved + other.Unresolved + Conflict(other),
            Convention.IsNone ? other.Convention : Convention,
            toSeq((Udim + other.Udim).Distinct().OrderBy(static t => t.Value)),
            Provenance.IsNone ? other.Provenance : Provenance);

    Seq<(IngestRefusal Reason, string Detail)> Conflict(SetManifest other) =>
        Convention.IsSome && other.Convention.IsSome && Convention != other.Convention
            ? Seq((IngestRefusal.ConventionDivergent,
                   $"{Convention.Map(static c => c.Key).IfNone("none")}:{other.Convention.Map(static c => c.Key).IfNone("none")}"))
            : Seq<(IngestRefusal, string)>();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SetIngest {
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
            stems:    static s => s.Probes.Fold(SetManifest.Empty, static probe => acc.Combine(One(probe))),
            declared: static d => d.Manifest.Maps.Fold(SetManifest.Empty, static map => acc.Combine(Validate(map))).Combine(
                          SetManifest.Empty with {
                              Unresolved = d.Manifest.Unresolved, Convention = d.Manifest.Convention,
                              Udim = d.Manifest.Udim, Provenance = d.Manifest.Provenance,
                          }),
            peer:     static p =>
                toSeq(p.Manifest.Pbr.Planes.GroupBy(static entry => entry.Role)).Choose(static group =>
                        toSeq(group).Find(static entry => PeerSampled.Contains(entry.Container)) || toSeq(group).Head)
                    .Fold(SetManifest.Empty, entry => acc.Combine(PeerMap(p.Manifest.Pbr, entry)))
                    .Combine(toSeq(p.Manifest.Pbr.Packs).Fold(SetManifest.Empty, entry => acc.Combine(PeerPack(p.Manifest.Pbr, entry))))
                    .Combine(SetManifest.Empty with {
                        Unresolved = toSeq(p.Manifest.Pbr.Unresolved).Map(static detail => (IngestRefusal.Unclaimed, detail)),
                        Convention = WireVocabulary.Convention(p.Manifest.Pbr.NormalConvention),
                    }));

    public static Fin<IngestSource> Peer(ReadOnlyMemory<byte> wire) =>
        Try.lift(() => Wire.Set.Parser.ParseFrom(
                CodedInputStream.CreateWithLimits(wire.AsStream(), WireLimits.Manifest.SizeLimit, WireLimits.Manifest.RecursionLimit))).Run()
            .Bind(manifest => WireAdmission.Admit(manifest, WireBoundary.InboundPayload))
            .Bind(manifest => manifest.ProductCase == Wire.Set.ProductOneofCase.Pbr
                ? Fin.Succ<IngestSource>(new IngestSource.Peer(manifest))
                : new MaterialFault.Parameter($"<peer-manifest-product:{manifest.ProductCase}>"));

    static SetManifest Validate(ClassifiedMap map) =>
        toSeq(map.Channels.Filter(static c => !Index.Value.ContainsKey(c.Key)).Map(static c => c.Key)) is { IsEmpty: false } retired
            ? SetManifest.Empty with { Unresolved = Seq((IngestRefusal.Unclaimed, $"<declared-channel-unindexed:{string.Join(',', retired)}:{map.Stem}>")) }
            : Refuted(map).Match(
                Some: refusal => SetManifest.Empty with { Unresolved = Seq(refusal) },
                None: () => SetManifest.Empty with { Maps = Seq(map) });

    static Option<(IngestRefusal Reason, string Detail)> Refuted(ClassifiedMap map);

    public static (IngestSource Source, Seq<RosterBinding> Binding) Roster(TextureRoster roster, Seq<PlaneProbe> probes) {
        FrozenDictionary<string, PlaneProbe> byStem = probes
            .Map(static p => KeyValuePair.Create(p.Stem, p))
            .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        (SetManifest manifest, Seq<RosterBinding> binding) = roster.Textures.Fold(
            (SetManifest.Empty, Seq<RosterBinding>()),
            (acc, candidate) => Candidate(acc.Item1, acc.Item2, candidate, byStem));
        return (new IngestSource.Declared(manifest), binding);
    }

    static (SetManifest, Seq<RosterBinding>) Candidate(
        SetManifest acc, Seq<RosterBinding> binding, TextureCandidate candidate, FrozenDictionary<string, PlaneProbe> byStem) {
        if (!byStem.TryGetValue(Path.GetFileNameWithoutExtension(candidate.Reference), out PlaneProbe probe)) {
            return (acc.Combine(SetManifest.Empty with {
                Unresolved = Seq((IngestRefusal.Unclaimed, $"<roster-probe-missing:{candidate.Channel}:{candidate.Reference}>")) }), binding);
        }
        (Seq<TextureChannel> channels, Option<ChannelPack> pack) =
            Packs.Value.TryGetValue(candidate.Channel, out ChannelPack? sheet) ? (sheet.Slots, Some(sheet))
            : Index.Value.TryGetValue(candidate.Channel, out TextureChannel? row) ? (Seq(row), Option<ChannelPack>.None)
            : (Seq<TextureChannel>(), Option<ChannelPack>.None);
        if (channels.IsEmpty) {
            return (acc.Combine(SetManifest.Empty with {
                Unresolved = Seq((IngestRefusal.Unclaimed, $"<roster-token-unindexed:{candidate.Channel}:{candidate.Reference}>")) }), binding);
        }
        UvFrame frame = new(candidate.OffsetU, candidate.OffsetV, candidate.ScaleU, candidate.ScaleV, candidate.Rotation);
        return (acc.Combine(Validate(new ClassifiedMap(channels, probe.Stem, Option<UdimTile>.None, pack, candidate.Polarity, probe))),
            binding + channels.Map(c => new RosterBinding(c, frame, candidate.WrapU, candidate.WrapV, candidate.CoordinateSet)));
    }

    static readonly FrozenSet<Wire.Container> PeerSampled =
        new[] { Wire.Container.Png16, Wire.Container.TiffF32 }.ToFrozenSet();

    static Option<PlaneProbe> Probe(PeerPlaneRow row, Wire.Set manifest) =>
        from depth in row.Depth
        from transfer in row.Transfer
        from format in PlaneFormat.For(row.Channels, depth)
        select new PlaneProbe(row.Stem, format, transfer,
            Dimension.Create((int)manifest.Width), Dimension.Create((int)manifest.Height), default, default,
            Option<PlanePrimaries>.None, Option<int>.None);

    static SetManifest PeerMap(Wire.SurfaceSet manifest, Wire.Plane entry) {
        if (Probe(PeerIntake.Row(entry), manifest).Case is not PlaneProbe probe) {
            return SetManifest.Empty with { Unresolved = Seq((IngestRefusal.PeerRowVocabulary, $"{PeerIntake.Leaf(entry)}:{entry.Depth}:{entry.Transfer}")) };
        }
        SetManifest classified = One(probe);
        return classified.Maps.Head
            .Map(map => map.Pack.IsSome || WireVocabulary.Channel(entry.Role).Exists(map.Channels.Contains)
                ? classified
                : SetManifest.Empty with { Unresolved = Seq((IngestRefusal.PeerRoleDivergent, $"{entry.Role}:{map.Stem}")) })
            .IfNone(classified);
    }

    static SetManifest PeerPack(Wire.SurfaceSet manifest, Wire.PackRow entry) =>
        Probe(PeerIntake.Row(entry), manifest).Case is PlaneProbe probe
            ? One(probe)
            : SetManifest.Empty with { Unresolved = Seq((IngestRefusal.PeerPackVocabulary, $"{PeerIntake.Leaf(entry)}:{entry.Format}")) };

    static SetManifest One(PlaneProbe probe) {
        Seq<string> tokens = Tokenize(probe.Stem);
        Option<UdimTile> tile = tokens.Choose(static t => int.TryParse(t, out int value) && UdimTile.TryCreate(value, out UdimTile tile) ? Some(tile) : None).Head;
        Option<NormalConvention> convention = tokens.Choose(static t => Conventions.Value.TryGetValue(t, out NormalConvention? c) ? Some(c!) : Option<NormalConvention>.None).Head;
        Option<ChannelPack> pack = tokens.Choose(static t => Packs.Value.TryGetValue(t, out ChannelPack? p) ? Some(p!) : Option<ChannelPack>.None).Head;
        return pack.Match(
            Some: p => probe.Format.Components is 4
                ? SetManifest.Empty with { Maps = Seq(new ClassifiedMap(p.Slots, probe.Stem, tile, Some(p), Inverted: false, probe)), Udim = tile.ToSeq() }
                : SetManifest.Empty with { Unresolved = Seq((IngestRefusal.PackPlaneNarrow, probe.Stem)) },
            None: () => tokens.Choose(static t => Index.Value.TryGetValue(t, out TextureChannel? c)
                    ? Some((Channel: c!, Gloss: false))
                    : GlossAliases.Value.Contains(t)
                        ? Some((Channel: TextureChannel.SpecularRoughness, Gloss: true))
                        : Option<(TextureChannel Channel, bool Gloss)>.None)
                .Head
                .Match(
                    Some: hit => probe.Format.Components >= hit.Channel.Components
                        ? SetManifest.Empty with {
                              Maps = Seq(new ClassifiedMap(Seq(hit.Channel), probe.Stem, tile, Option<ChannelPack>.None, hit.Gloss, probe)),
                              Convention = hit.Channel.Slot == Some(SinkSlot.Normal) || hit.Channel == TextureChannel.GeometryCoatNormal ? convention : Option<NormalConvention>.None,
                              Udim = tile.ToSeq(),
                          }
                        : SetManifest.Empty with { Unresolved = Seq((IngestRefusal.ComponentsRefuted, $"{probe.Stem}:{hit.Channel.Key}")) },
                    None: () => SetManifest.Empty with { Unresolved = Seq((IngestRefusal.Unclaimed, probe.Stem)) }));
    }

    static Seq<string> Tokenize(string stem) =>
        toSeq(stem.Split(['-', '_', '.', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    public static Fin<Seq<(Option<UdimTile> Tile, TextureSetDraft Draft)>> Draft(
        SetManifest manifest, HashMap<string, TexturePyramid> planes, IngestIntent intent) =>
        toSeq(manifest.Maps.GroupBy(static map => map.Tile.Map(static t => t.Value).IfNone(0)))
            .TraverseM(group => Tile(toSeq(group), manifest.Convention, planes, intent)).As();

    static Fin<(Option<UdimTile> Tile, TextureSetDraft Draft)> Tile(
        Seq<ClassifiedMap> maps, Option<NormalConvention> convention, HashMap<string, TexturePyramid> planes, IngestIntent intent) =>
        from head in maps.Head.ToFin(new MaterialFault.Parameter("<ingest-tile-empty>"))
        from _ in guard(maps.ForAll(map => map.Probe.Width == head.Probe.Width && map.Probe.Height == head.Probe.Height),
            new MaterialFault.Parameter($"<ingest-tile-extent-divergent:{head.Stem}>"))
        from bound in maps.FoldM((Channels: HashMap<TextureChannel, TexturePyramid>(), Packs: Seq<ChannelPackPlane>()), (carried, map) =>
                from supplied in planes.Find(map.Stem).ToFin(new MaterialFault.Parameter($"<ingest-plane-missing:{map.Stem}>"))
                from converted in Converted(supplied, map, convention, key)
                select map.Pack
                    .Map(pack => (carried.Channels, carried.Packs.Add(new ChannelPackPlane(pack, converted, map.Channels))))
                    .IfNone(() => (carried.Channels.AddOrUpdate(map.Channels[0], converted), carried.Packs))).As()
        select (head.Tile, new TextureSetDraft(head.Probe.Width, head.Probe.Height, Dimension.Create(1), LayerLaw.None,
            convention.IfNone(NormalConvention.Gl), intent.Alpha, intent.HeightScaleMm, new Evidence<TileProof>.Absent(),
            head.Tile.ToSeq(), bound.Channels, bound.Packs, intent.Conductor, intent.Material));

    static Fin<TexturePyramid> Converted(TexturePyramid pyramid, ClassifiedMap map, Option<NormalConvention> convention) {
        Seq<PlaneOp> ops =
            (convention == Some(NormalConvention.Dx) && map.Channels.Exists(IsNormal)
                ? Seq<PlaneOp>(new PlaneOp.Swizzle(SwizzleLane.FlipGreen)) : Seq<PlaneOp>())
            + (map.Inverted ? Seq<PlaneOp>(new PlaneOp.Remap(RemapCurve.Levels.Invert)) : Seq<PlaneOp>());
        return ops.IsEmpty
            ? Fin.Succ(pyramid)
            : pyramid.Levels
                .FoldM(Seq<TexturePlane>(), (built, level) =>
                    PlaneOp.Apply(level, ops)
                        .Map(result => built.Add(result.Plane))
                        .Rollback([.. built])).As()
                .Bind(levels => Custody.Bracket(
                    () => Fin.Succ(new TexturePyramid(levels, pyramid.Policy, pyramid.Coupled)),
                    pyramid));
    }

    static bool IsNormal(TextureChannel channel) =>
        channel == TextureChannel.GeometryNormal || channel == TextureChannel.GeometryCoatNormal;
}

internal readonly record struct PeerPlaneRow(string Stem, Option<ChannelDtype> Depth, Option<PlaneTransfer> Transfer, int Channels);

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target, EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
internal static partial class PeerIntake {
    [MapProperty(nameof(Wire.Plane.Levels), nameof(PeerPlaneRow.Stem), Use = nameof(Leaf))]
    [MapProperty(nameof(Wire.Plane.Depth), nameof(PeerPlaneRow.Depth), Use = nameof(Storage))]
    [MapProperty(nameof(Wire.Plane.Transfer), nameof(PeerPlaneRow.Transfer), Use = nameof(Encoded))]
    [MapProperty(nameof(Wire.Plane.Channels), nameof(PeerPlaneRow.Channels), Use = nameof(Signed))]
    internal static partial PeerPlaneRow Row(Wire.Plane entry);

    [MapProperty(nameof(Wire.PackRow.Levels), nameof(PeerPlaneRow.Stem), Use = nameof(Leaf))]
    [MapProperty(nameof(Wire.PackRow.Format), nameof(PeerPlaneRow.Depth), Use = nameof(Packed))]
    [MapValue(nameof(PeerPlaneRow.Transfer), Use = nameof(RawTransfer))]
    [MapValue(nameof(PeerPlaneRow.Channels), 4)]
    internal static partial PeerPlaneRow Row(Wire.PackRow entry);

    [UserMapping] internal static string Leaf(RepeatedField<Wire.PlaneRef> levels) =>
        levels.Count > 0 ? Path.GetFileNameWithoutExtension(levels[0].File) : string.Empty;
    internal static string Leaf(Wire.Plane entry) => Leaf(entry.Levels);
    internal static string Leaf(Wire.PackRow entry) => Leaf(entry.Levels);

    [UserMapping] private static Option<ChannelDtype> Storage(Wire.Depth depth) => WireVocabulary.Depth(depth);
    [UserMapping] private static Option<PlaneTransfer> Encoded(Wire.Transfer transfer) => WireVocabulary.Transfer(transfer);
    [UserMapping] private static Option<ChannelDtype> Packed(Wire.PlaneFormat format) => WireVocabulary.Format(format).Map(static row => row.Depth);
    [UserMapping] private static int Signed(uint channels) => checked((int)channels);

    private static Option<PlaneTransfer> RawTransfer() => Some(PlaneTransfer.Raw);
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
public sealed record WireLimits(int SizeLimit, int RecursionLimit) {
    private const int ManifestSizeCeiling = 16 << 20;
    private const int DeclarationSizeCeiling = 4 << 20;
    private const int StandardRecursionCeiling = 100;

    public static readonly WireLimits Manifest = new(ManifestSizeCeiling, StandardRecursionCeiling);
    public static readonly WireLimits Declaration = new(DeclarationSizeCeiling, StandardRecursionCeiling);
}

public static class WireVocabulary {
    public static Option<TEnum> Lift<TEnum>(string key) where TEnum : struct, Enum =>
        Enum.TryParse(key.Replace("_", string.Empty).Replace("-", string.Empty), ignoreCase: true, out TEnum value)
        && !EqualityComparer<TEnum>.Default.Equals(value, default)
            ? Some(value)
            : None;

    public static Lazy<FrozenDictionary<TRow, TEnum>> Total<TRow, TEnum>(Func<IReadOnlyList<TRow>> rows, Func<TRow, string> key)
        where TRow : notnull where TEnum : struct, Enum =>
        new(() => rows().ToFrozenDictionary(static row => row, row => Lift<TEnum>(key(row)).IfNone(default(TEnum))));

    public static Fin<Unit> Proof<TRow, TEnum>(Func<IReadOnlyList<TRow>> rows, Func<TRow, string> key)
        where TRow : notnull where TEnum : struct, Enum =>
        toSeq(rows())
            .Filter(row => Lift<TEnum>(key(row)).IsNone)
            .Map(row => (Error)new MaterialFault.Parameter($"<wire-vocabulary-unsound:{typeof(TEnum).Name}:{key(row)}>"))
            is { IsEmpty: false } faults
            ? Fin.Fail<Unit>(Error.Many(faults.Strict()))
            : Fin.Succ(unit);

    static readonly Lazy<FrozenDictionary<TextureChannel, Wire.Role>> Roles = Total<TextureChannel, Wire.Role>(static () => TextureChannel.Items, static r => r.Key);
    static readonly Lazy<FrozenDictionary<PlaneTransfer, Wire.Transfer>> Transfers = Total<PlaneTransfer, Wire.Transfer>(static () => PlaneTransfer.Items, static r => r.Key);
    static readonly Lazy<FrozenDictionary<AlphaMode, Wire.AlphaMode>> Alphas = Total<AlphaMode, Wire.AlphaMode>(static () => AlphaMode.Items, static r => r.Key);
    static readonly Lazy<FrozenDictionary<MipPolicy, Wire.MipPolicy>> Mips = Total<MipPolicy, Wire.MipPolicy>(static () => MipPolicy.Items, static r => r.Key);
    static readonly Lazy<FrozenDictionary<NormalConvention, Wire.NormalConvention>> Conventions = Total<NormalConvention, Wire.NormalConvention>(static () => NormalConvention.Items, static r => r.Key);
    static readonly Lazy<FrozenDictionary<PlaneFormat, Wire.PlaneFormat>> Formats = Total<PlaneFormat, Wire.PlaneFormat>(static () => PlaneFormat.Items, static r => r.Key);
    static readonly Lazy<FrozenDictionary<LayerLaw, Wire.LayerLaw>> Laws = Total<LayerLaw, Wire.LayerLaw>(static () => LayerLaw.Items, static r => r.Key);
    static readonly Lazy<FrozenDictionary<KtxPayload, Wire.KtxPayload>> Payloads = Total<KtxPayload, Wire.KtxPayload>(static () => KtxPayload.Items, static r => r.Key);
    static readonly Lazy<FrozenDictionary<BlockFormat, Wire.BlockFormat>> Blocks = Total<BlockFormat, Wire.BlockFormat>(static () => BlockFormat.Items, static r => r.Key);
    static readonly Lazy<FrozenDictionary<ChannelPack, Wire.Pack>> Packs = Total<ChannelPack, Wire.Pack>(static () => ChannelPack.Items, static r => r.Key);
    static readonly Lazy<FrozenDictionary<RasterFormat, Wire.Container>> Containers = new(static () =>
        RasterFormat.Items
            .Choose(static row => Enum.TryParse(row.Key, ignoreCase: true, out Wire.Container container) && container != Wire.Container.Unspecified
                ? Some((Row: row, Container: container))
                : Option<(RasterFormat Row, Wire.Container Container)>.None)
            .ToFrozenDictionary(static pair => pair.Row, static pair => pair.Container));

    public static Wire.Role Role(TextureChannel row) => Roles.Value[row];
    public static Wire.Transfer Transfer(PlaneTransfer row) => Transfers.Value[row];
    public static Wire.AlphaMode Alpha(AlphaMode row) => Alphas.Value[row];
    public static Wire.MipPolicy Mip(MipPolicy row) => Mips.Value[row];
    public static Wire.NormalConvention Convention(NormalConvention row) => Conventions.Value[row];
    public static Wire.PlaneFormat Format(PlaneFormat row) => Formats.Value[row];
    public static Wire.LayerLaw Law(LayerLaw row) => Laws.Value[row];
    public static Wire.KtxPayload Payload(KtxPayload row) => Payloads.Value[row];
    public static Wire.BlockFormat Block(BlockFormat row) => Blocks.Value[row];
    public static Wire.Pack Pack(ChannelPack row) => Packs.Value[row];
    public static Wire.Primaries Primaries(PlanePrimaries row) => row switch {
        var p when p == PlanePrimaries.Unknown => Wire.Primaries.None,
        var p when p == PlanePrimaries.AcesAp1 => Wire.Primaries.Acescg,
        var p when p == PlanePrimaries.AcesAp0 => Wire.Primaries.Aces,
        var p when p == PlanePrimaries.Bt709 => Wire.Primaries.Bt709,
        var p when p == PlanePrimaries.Bt2020 => Wire.Primaries.Bt2020,
        var p when p == PlanePrimaries.P3D65 => Wire.Primaries.Displayp3,
        var p when p == PlanePrimaries.Xyz => Wire.Primaries.Ciexyz,
        _ => Wire.Primaries.None,
    };
    public static Fin<Wire.Depth> Depth(ChannelDtype row) => row switch {
        var d when d == ChannelDtype.Unorm8 => Fin.Succ(Wire.Depth.U8),
        var d when d == ChannelDtype.Unorm16 => Fin.Succ(Wire.Depth.U16),
        var d when d == ChannelDtype.Float16 => Fin.Succ(Wire.Depth.F16),
        var d when d == ChannelDtype.Float32 => Fin.Succ(Wire.Depth.F32),
        _ => new MaterialFault.Parameter($"<wire-depth-unrostered:{row.Key}>"),
    };
    public static Fin<Wire.Container> Container(RasterFormat row) =>
        Containers.Value.TryGetValue(row, out Wire.Container container)
            ? Fin.Succ(container)
            : new MaterialFault.Parameter($"<wire-container-unrostered:{row.Key}>");

    public static Option<TextureChannel> Channel(Wire.Role role) => Inverse(Roles.Value, role);
    public static Option<PlaneTransfer> Transfer(Wire.Transfer transfer) => Inverse(Transfers.Value, transfer);
    public static Option<PlaneFormat> Format(Wire.PlaneFormat format) => Inverse(Formats.Value, format);
    public static Option<KtxPayload> Payload(Wire.KtxPayload payload) => Inverse(Payloads.Value, payload);
    public static Option<BlockFormat> Block(Wire.BlockFormat block) => Inverse(Blocks.Value, block);
    public static Option<ChannelPack> Pack(Wire.Pack pack) => Inverse(Packs.Value, pack);
    public static Option<NormalConvention> Convention(Wire.NormalConvention convention) => Inverse(Conventions.Value, convention);
    public static Option<RasterFormat> Format(Wire.Container container) => Inverse(Containers.Value, container);
    public static Option<ChannelDtype> Depth(Wire.Depth depth) => depth switch {
        Wire.Depth.U8 => Some(ChannelDtype.Unorm8), Wire.Depth.U16 => Some(ChannelDtype.Unorm16),
        Wire.Depth.F16 => Some(ChannelDtype.Float16), Wire.Depth.F32 => Some(ChannelDtype.Float32),
        _ => Option<ChannelDtype>.None,
    };

    static Option<TRow> Inverse<TRow, TEnum>(FrozenDictionary<TRow, TEnum> rows, TEnum value) where TRow : notnull where TEnum : struct, Enum =>
        toSeq(rows).Find(pair => EqualityComparer<TEnum>.Default.Equals(pair.Value, value)).Map(static pair => pair.Key);
}

public sealed record IngestProvenance(string Source, Option<string> LicenceDeclared, Option<string> Reference);

public sealed record IngestIntent(AlphaMode Alpha, Option<double> HeightScaleMm, Option<ConductorMetal> Conductor, Option<MaterialId> Material, Option<IngestProvenance> Provenance);
```

## [05]-[SET_BIND]

- Owner: `SetBind` the set-to-appearance lowering; `BindTarget` `[Union]` the requested lowering; `SetBinding` `[Union]` the produced carrier.
- Cases: target {`Program` (the node DAG a renderer compiles once), `Point` (the per-texel parameter row a shade reconstructs), `Average` (the measured summary row the contract `AppearanceSummary` and the LOD fallback read)} · binding {`Program`, `Row`}.
- Entry: `public static Fin<SetBinding> Bind(TextureSet set, MaterialParameters fallback, BindTarget target, SamplerState sampler)` — ONE entry whose modality discriminates on the target's own case, never on a name suffix or a boolean; the `fallback` row supplies every column the set does not carry, so a partial set always binds, and the `sampler` states HOW the set is read with no default anywhere on this page.
- Law: `sampler` is DEFAULTED NOWHERE — a caller that samples a set states its address modes, its filter, and its `UvFrame` exactly as `press#PRESS_PLAN` `PressSubject.Source`/`Slab` already carry theirs; a hardcoded `SamplerState.Default` at the binding arms wrapped every clamped decal, silently discarded every consumer's tiling, and left the UV frame no route to a bind.
- Law: a fractional `BindTarget.Point.MipLevel` is honoured by `FilterMode.Trilinear` ALONE — every other row snaps to the `MipLevel`-nearest plane per `texture#TEXTURE_UV`'s own reconstruction law — so a caller deriving a level from a ray cone or a UV-density estimate binds trilinear or measures nothing, and a bounce crossing a level boundary pops under any other filter.
- Exemption: `Mean` is the page's `[EXPRESSION_SPINE]` kernel — a fixed-extent row accumulation over a caller-owned scratch pair, the only statement-shaped body here.
- Packages: `graph#MATERIAL_GRAPH` (composed — `MaterialGraph.Default` as the program's own base, its `Author` authoring fold over `GraphEdit.Seat`, `PortOf` reading the sink's standing `ShadeChannel.NormalFrame` port, `AppearanceNode.Texture`/`Normal`, `PortId`, `PortValue`; this page CONSTRUCTS no `BsdfOutput` and declares no sink), `texture#TEXTURE_UV` (composed — `TextureUv.Port` minting each `Texture` node's total closure, `TextureUv.Sample` reading a plane at a point, `UvSample`, `SamplerState` carrying the caller's address, filter, and `UvFrame` policy, `Channel`), `plane#TEXTURE_PLANE` (composed — `TexturePyramid.AsImage` lifting a pyramid into the existing `TextureSource.Image` sampler input under the plane's own transfer, and the `TexturePlane.Read` row accessor the measured mean folds), `CommunityToolkit.HighPerformance` (`SpanOwner<T>` the mean fold's caller-owned scratch; `ParallelHelper.ForEach` over `IRefAction<MeanSlot>` the item-partitioned mean staging — each worker owns one plane and one result slot), LanguageExt.Core.
- Growth: a new lowering modality is one `BindTarget` case with its `SetBinding` arm — never a second `Bind` overload and never a `BindGraph`/`BindRow` pair; a channel reaches the graph program the moment its row carries a `SinkSlot`, so the sink widening at `graph#MATERIAL_GRAPH` propagates here as row data, and the wiring reads each slot's own `PortId` column to SEAT over `MaterialGraph.Default` so the bound program and the default carry ONE topology by construction with no literal re-authored and no fallback node respelled.
- Boundary: `SetBind` closes the round trip photo-or-press → planes → SHADEABLE MATERIAL; a lowering that stops at encodable bytes is the deleted form. The `Program` arm binds only the channels whose rows carry a `SinkSlot`, because the `graph#MATERIAL_GRAPH` `BsdfOutput` sink admits exactly five ports today — every other channel binds through the `Point` arm, so no unread phantom `Texture` node enters the DAG to be mistaken for live capability. The `Program` arm is `MaterialGraph.Default` put through its OWN authoring fold — `MaterialGraph.Author` over one `GraphEdit.Seat` per covered slot — never a second hand-wiring of the default topology: the hand-rebuild it replaces re-declared the `Normal` and `BsdfOutput` nodes at literal port ids and rebuilt an `Input` node for every uncovered slot, so the two spellings of one topology had to be re-checked against each other on every sink widening, and each uncovered slot carried a `SinkSlot` fallback column that existed only to restate what `MaterialGraph.Default` already wired. Seating is what makes that identity structural: a covered slot REPLACES the default's node at the slot's own `PortId` rather than authoring a fresh port and routing to it, because a fresh port strands the default's node as an isolate the sort still admits and still evaluates — one `PortValue` production per texel for a scratch cell nothing reads — and an uncovered slot is authored NOTHING at all. Each bound channel becomes one `AppearanceNode.Texture` holding the `TextureUv.Port` closure over its pyramid's `AsImage` lift UNDER THE CALLER'S SAMPLER — the same address modes, filter, and `UvFrame` the `Point` arm reads with, so a program and a per-texel row over one set never disagree on where a coordinate lands — projected through the slot's own `Channel` modality and then through the slot's `Encode` column — the normal port re-encodes the decoded signed texel to the `[0,1]` convention the node's own `2v−1` decode expects, so a bound normal plane perturbs the frame correctly instead of inverting X and Y at every texel; a set carrying no normal channel keeps the default graph's identity-normal node at strength zero untouched — not a rebuild of it — and a covered one seats that same node at strength one over the port the SINK itself names through `PortOf`, so the produced program is always a complete DAG the compiler admits and the strength is the only thing the normal channel changes. The per-row TRANSFER law rides `AsImage`: an `srgb` plane decodes to scene-linear at the lift and a `raw` plane crosses untouched, so no consumer of a bound graph re-applies a transfer and a doubly-decoded colour plane is unrepresentable; a LAYERED set refuses the `Program` arm outright, because `TextureSource.Image` carries one layer by construction and a cube or array set reaches a renderer as a set rather than through the UV sampler. The `Point` and `Average` arms read PACKS as well as standalone channels — a pack plane's lanes are its `ChannelPack.Slots` in order, so each slot's lane projects to that channel's scalar and folds through its own lens, and a set whose roughness rides inside an `orm` sheet reconstructs the same row a standalone roughness plane would. The `Point` arm reconstructs the FULL vector: each channel's `ColumnLens.Write` folds its sampled texel onto the fallback row, a channel whose lens carries no write contributes to the OpenPBR vector through `surface#OPENPBR_SLAB` `OpenPbrSurface.Of` rather than to the row (the typed absence, never a fabricated column), and the result re-admits through `MaterialParameters.Of` so a sampled set cannot smuggle an out-of-unit weight or an out-of-gamut colour past the one admission every library row passes. The `Average` arm MEASURES the mean — one streaming pass over each channel's base level through the plane's own decoded row accessor, so it reads no sampler at all — rather than reading a pyramid's coarsest texel: only a box fold's tail is the arithmetic mean, while a Kaiser, renormalizing, or variance-coupled fold's tail is a weighted or corrected value, and publishing that as the mean fabricates the number the contract appearance key then carries forever. The measured fold costs one pass over planes the press just wrote, needs no pyramid at all, and therefore admits a single-level set the pyramid-tail read had to refuse; the passes partition by ITEM — `ParallelHelper.ForEach` hands each worker one plane and its own result slot, so channel means stage concurrently with zero shared writes and the lens re-admission stays one sequential pass.

```csharp
using CommunityToolkit.HighPerformance.Helpers;

// --- [TYPES] ---------------------------------------------------------------------------
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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SetBind {
    public static Fin<SetBinding> Bind(TextureSet set, MaterialParameters fallback, BindTarget target, SamplerState sampler) =>
        target.Switch(
            state:   (Set: set, Fallback: fallback, Sampler: sampler),
            program: static (s, _) => Dag(s.Set, s.Sampler, s.Key).Map(static g => (SetBinding)new SetBinding.Program(g)),
            point:   static (s, p) => Sample(s.Set, s.Fallback, p, s.Sampler, s.Key).Map(static r => (SetBinding)new SetBinding.Row(r)),
            average: static (s, _) => Summary(s.Set, s.Fallback, s.Key).Map(static r => (SetBinding)new SetBinding.Row(r)));

    static readonly UvSample Anchor = new(UnitInterval.Create(0.0), UnitInterval.Create(0.0), Vector3d.Zero, Vector3d.ZAxis, 0.0);

    static Fin<MaterialGraph> Dag(TextureSet set, SamplerState sampler) =>
        set.Layers.Value > 1
            ? Fin.Fail<MaterialGraph>(new MaterialFault.Parameter($"<layered-set-has-no-uv-program:{set.Law.Key}:{set.Layers.Value}>"))
            : from perturbed in MaterialGraph.Default.PortOf(ShadeChannel.NormalFrame)
              from seats in toSeq(SinkSlot.Items).TraverseM(slot => SlotEdit(set, slot, sampler)).As()
                  .Map(static edits => edits.Somes())
              from bound in MaterialGraph.Default.Author(
                  set.Channels.ContainsKey(TextureChannel.GeometryNormal)
                      ? seats.Add(new GraphEdit.Seat(new AppearanceNode.Normal(perturbed, SinkSlot.Normal.Port, Strength: 1.0)))
                      : seats)
              select bound;

    static Fin<Option<GraphEdit>> SlotEdit(TextureSet set, SinkSlot slot, SamplerState sampler) =>
        set.Channels.Find(TextureChannel.BySlot(slot))
            .TraverseM(pyramid => pyramid.AsImage().Map(image => (GraphEdit)new GraphEdit.Seat(
                new AppearanceNode.Texture(slot.Port, Option<PortId>.None,
                    Compose(TextureUv.Port(image, Anchor, sampler, slot.Modality), slot)))))
            .As();

    static Func<double, double, Option<double>, PortValue> Compose(Func<double, double, Option<double>, PortValue> port, SinkSlot slot) =>
        (u, v, parameter) => slot.Encode(port(u, v, parameter));

    static Fin<MaterialParameters> Sample(TextureSet set, MaterialParameters fallback, BindTarget.Point at, SamplerState sampler) =>
        toSeq(set.Channels.AsIterable())
            .FoldM(fallback, (row, pair) => Read(pair.Value, at, sampler).Map(texel => Apply(pair.Key, row, texel))).As()
            .Bind(row => set.Packs
                .FoldM(row, (carried, pack) => Read(pack.Plane, at, sampler).Map(texel => Unpack(pack, carried, texel))).As())
            .Bind(row => MaterialParameters.Of(row));

    static Fin<ShadeVec4> Read(TexturePyramid pyramid, BindTarget.Point at, SamplerState sampler) =>
        from image in pyramid.AsImage()
        from sample in TextureUv.Sample(image, new UvSample(at.U, at.V, Vector3d.Zero, Vector3d.ZAxis, at.MipLevel), sampler)
        select sample;

    static MaterialParameters Apply(TextureChannel channel, MaterialParameters row, ShadeVec4 texel) =>
        channel.Origin switch {
            ChannelOrigin.Shaded shaded => shaded.Lens.Write.Map(write => write(row, texel)).IfNone(row),
            _ => row,
        };

    static MaterialParameters Unpack(ChannelPackPlane pack, MaterialParameters row, ShadeVec4 texel) =>
        pack.Present.Fold(row, (carried, channel) =>
            pack.Pack.Lane(channel).Map(lane => Apply(channel, carried, new ShadeVec4(Lane(texel, lane), 0.0, 0.0, 1.0))).IfNone(carried));

    static double Lane(ShadeVec4 texel, int lane) => lane switch { 0 => texel.X, 1 => texel.Y, _ => texel.Z };

    static Fin<MaterialParameters> Summary(TextureSet set, MaterialParameters fallback) {
        Seq<(TextureChannel Key, TexturePyramid Value)> channels = toSeq(set.Channels.AsIterable());
        MeanSlot[] slots = [
            .. channels.Map(static pair => new MeanSlot(pair.Value.Base)),
            .. set.Packs.Map(static pack => new MeanSlot(pack.Plane.Base))];
        MeanStage stage = default;
        return Try.lift(() => {
                ParallelHelper.ForEach<MeanSlot, MeanStage>(slots.AsMemory(), in stage);
                return Fin.Succ(unit);
            }).Run().Bind(static inner => inner)
            .Bind(_ => MaterialParameters.Of(
                set.Packs
                    .Map((pack, index) => (Pack: pack, slots[channels.Count + index].Mean))
                    .Fold(
                        channels
                            .Map((pair, index) => (pair.Key, slots[index].Mean))
                            .Fold(fallback, (carried, row) => Apply(row.Key, carried, row.Mean)),
                        (carried, pair) => Unpack(pair.Pack, carried, pair.Mean))));
    }

    private struct MeanSlot(TexturePlane plane) {
        public readonly TexturePlane Plane = plane;
        public ShadeVec4 Mean;
    }

    private readonly struct MeanStage : IRefAction<MeanSlot> {
        public void Invoke(ref MeanSlot slot) => slot.Mean = Mean(slot.Plane);
    }

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
