# [MATERIALS_PLANE]

THE DECODED-RASTER SUBSTRATE. One `TexturePlane` owns every pixel grid the texture module holds — the storage row, the kernel `CellLattice` that seats its extent and its spatial grain, the layer stack, the encoded transfer, the colour primaries, the alpha association, and the value range — over a TYPED-TEXEL pooled arena that never becomes a byte arena; one `TexturePyramid` owns the level chain a `MipPolicy` row folds over the lattice's own `Coarsen` step, including the variance coupling a roughness chain takes from its paired normal chain; one `AsImage` bridge lifts a chain into the existing `texture#TEXTURE_UV` `TextureSource.Image` sampler so the module mints no second sampler; and one `PlaneResidency` window bounds how much of a declared tile grid is resident at once, so an asset whose whole grid exceeds the arena resolves the tiles a view reads and evicts the rest under a policy row. `PlaneFormat` rows own storage over the kernel `ChannelDtype` roster, `PlaneTransfer` rows own transfer, `PlanePrimaries` rows own chromaticity, and `MipPolicy` rows own the level fold — never a per-format plane type, a per-depth converter pair, a second depth vocabulary, or a per-policy pyramid class.

Typed texels size the arena because bytes cannot: `byte[]` caps at `Array.MaxLength`, so a 16k×16k four-lane 16-bit plane spans 2.147 GB of bytes and refuses at the runtime bound, while the same plane counts 268 435 456 TEXELS and rents cleanly — the element count is the only budget that admits the extents this module bakes. Storage therefore comes from ONE open generic `PlaneStore<T>` over a `MemoryOwner<T>`/`Memory2D<T>` pair, the texel structs are three arities applied to four component witnesses rather than twelve hand-written records, and typed code re-enters through a `struct`-or-`ref struct` fold interface that the JIT specializes per texel with no boxing, no closure, and no per-row delegate. Every consumer above this page reads and writes DECODED lanes through the one `Read`/`Write` row accessor, so the encode ladder — integer normalization, signed `(v+1)/2` packing, transfer decode, alpha association — is stated exactly once in the corpus and no kernel re-derives a curve. `Rasm.Materials.Raster` composes `CommunityToolkit.HighPerformance` for the pooled arena and its plane views, `TinyEXR.NET` `ImageProcessing` for every transfer fold, every colour-matrix rebase, and every delegated resample row — the `kaiser` fold alone is the Materials-owned windowed-sinc kernel, because neither the composed resampler nor the kernel weight roster ships one — MathNet.Numerics for the Bessel evaluation that fold's window needs, the kernel `CellLattice`/`Dimension`/`ChannelDtype`/`Op`/`ContentHash` atoms, the `bsdf#SHADING_FRAME` `MaterialFault` band-2450 channel for every shape refusal, and the `texture#TEXTURE_UV` `TextureSource.Image`/`ShadeVec4` sampler at the one lift — re-minting no allocator, no lattice, no storage-type roster, no transfer curve, no special function, and no hash.

## [01]-[INDEX]

- [02]-[PLANE_VOCABULARY]: `PlaneTransfer`/`PlanePrimaries`/`AlphaMode`/`PlaneRange`/`NormalConvention`/`MipPolicy` axes each carry conversion behaviour as row data.
- [03]-[PLANE_FORMAT]: `IComponent`/`ITexel` static-abstract witnesses type the three-arity texel family, and the twelve-row `PlaneFormat` storage roster resolves a semantic count over the kernel `ChannelDtype` depths.
- [04]-[TEXTURE_PLANE]: `PlaneStore<T>` generalizes the arena behind its `IPlaneFold` interface, and `TexturePlane` owns admission over a `CellLattice`, the layer window, the decoded `Read`/`Write` row accessors, the association and primaries conversions, and the streaming content key.
- [05]-[TEXTURE_PYRAMID]: `MipPolicy` drives the level fold over the lattice's `Coarsen` step and the paired variance coupling, and `AsImage` bridges the chain to the sampler.
- [06]-[PLANE_RESIDENCY]: `PlaneResidency` windows a declared tile grid to a texel budget under a `ResidencyPolicy` rank, resolving one tile's chain through the caller's own mint.

## [02]-[PLANE_VOCABULARY]

- Owner: `PlaneTrait` the vocabulary's ONE combinable-column roster over the kernel `Domain/validation#CAPABILITY` `ICapability<TSelf>` floor; `PlaneTransfer` the encoded-transfer axis carrying its `TinyEXR.V3.TransferFunction` binding, the quantity it declares, and its bake-legality column; `PlanePrimaries` the chromaticity axis carrying its `ColorSpace` reconciliation row, the kernel `RgbProfile` row whose published geometry it matches a declaration against, and its container-assignment token; `AlphaMode` the association axis over its trait set, its crossing predicate, and the lane arithmetic every decode ladder in the module derives from it; `PlaneRange` the stored-value-range axis as an affine row pair; `NormalConvention` the green-polarity axis; `MipPolicy` the level-fold axis carrying its separable filter and its trait set.
- Cases: trait {`coverage`, `premultiplied`, `chains`, `renormalize`, `coupled`} · transfer {`linear`, `srgb`, `raw`, `pq`, `hlg`} · primaries {`acesAp1`, `bt709`, `bt2020`, `acesAp0`, `p3d65`, `xyz`, `unknown`} · alpha {`straight`, `associated`, `none`} · range {`unit`, `signed`} · convention {`gl`, `dx`} · mip {`box`, `kaiser`, `normalRenormalize`, `roughnessVariance`, `none`}.
- Law: a vocabulary row's COMBINABLE columns ride one `CapabilitySet<PlaneTrait>` and its SINGULAR columns stay row data. The association and level-fold axes each carried a private bool pair or triple whose corners no type closed and whose membership no wire could project — `Premultiplied` without `Coverage` was spellable, and a new fold law was a column plus a ctor arity change on every row of its owner. One roster answers both, so a trait lands as one row and each owner names it in its own set; `SceneReferred`, `Scale`, `Bias`, `GreenSign`, and `Filter` stay columns because a lone-valued or non-boolean fact states nothing a membership set states better.
- Law: `AlphaMode` owns the association's LANE ARITHMETIC, not just its declaration. `ColourLanes(lanes)` and `AlphaLane(lanes)` are the two derivations the decode ladder, every band walk, every codec staging, and every filter kernel took by re-spelling one ternary against a lane count — one row fact restated across the module, each copy free to drift the day a row stops reserving its coverage lane last. `AlphaLane` returns the composed resampler's own negative-index absence sentinel, so the two readings differ by their sentinel alone and no consumer converts between them.
- Law: the storage-component axis is the KERNEL `Drawing/pack#ENCODING_CHANNEL` `ChannelDtype` roster and this page mints none — `Unorm8`, `Unorm16`, `Float16`, and `Float32` are a strict subset of its rows, so a depth vocabulary here would be a second storage-type owner the `Rasm.Element` raster sample vocabulary and this arena would then have to reconcile. What the kernel roster does NOT carry is a normalization column, because a byte arena reads its own pack arm; the typed arena's `IComponent<T>` witness answers that instead, so `[03]` states the correspondence once as `PlaneFormat.Normalizes` and `PlaneRange`, `AlphaMode`, and `codec#RASTER_FORMAT` all read that one member.
- Law: `PlaneQuantity` splits the transfer rows by WHAT the stored number is — `light` for a scene-linear radiometric value, `parameter` for a shading input no colour transform may touch, `display` for a display-referred encoding. `PlaneQuantity` keeps `raw` and `linear` two rows rather than one alias: both decode by identity, but a colour transform legally reaches a `light` plane and never a `parameter` plane. `SceneReferred` is the SEPARATE bake-legality column the `set#TEXTURE_SET` admission gate reads: `srgb` is display-referred as an ENCODING yet scene-referred as a BAKE TARGET because `Read` decodes it to scene-linear, so `linear`, `srgb`, and `raw` carry `true` while `pq` and `hlg` — legal on an environment plane alone — carry `false` and refuse at `TextureSet.Of` for every channel plane.
- Law: the `pq` and `hlg` rows encode a display-referred TRANSFER and assert NO reference white. `surface#TONE_MAP` `DisplayEncoding` owns the colorimetric egress — primary rebase, transfer, and the 203-nit HDR reference white its `DynamicRange` column declares — so a plane carrying `pq` without a `DisplayEncoding` provenance is unanchored: its code values are legible and their absolute luminance is not. Each `DisplayEncoding` row names its storage transfer on this vocabulary, so an encode reads the pair off the colorimetric row rather than pairing them by hand.
- Law: `PlanePrimaries` is a SEPARATE axis from transfer: one linear plane may be AP1, Rec.709, or AP0, and the KTX colour-assignment pair labels each of the three differently, so a transfer-derived primaries label states a chromaticity the file never declared. Chromaticity GEOMETRY is the kernel `RgbProfile` row's published column and never a per-row coordinate table, so a working-space correction moves every label at once; DECLARATION and RECONCILIATION are independent columns — the `p3d65` row states a real code point the resampler carries no endpoint for, so it labels a P3 file honestly and refuses its rebase, where dropping the row lost the declaration outright. `unknown` is the honest DEFAULT — a decode that read no chromaticity attribute and no CICP block declares nothing rather than the working space — and it is what makes `--fail-on-color-conversions` a real gate instead of a rubber stamp over a fabricated label. `Matrix` resolves the composed reconciliation ONCE per conversion and refuses an unknown endpoint there, so a rebase across an undeclared gamut is unrepresentable rather than silently identity and the plane walk consuming the matrix carries no error channel, no per-row roster lookup, and no failure arm to orphan a rental.
- Law: `PlaneRange` is the SIGNED-ENCODE owner and the only site in the corpus that spells the storage-to-value affine. The map is ROW DATA — `Scale` and `Bias` — so `unit` is the identity affine, `signed` is `(2, −1)`, and `Unpack`/`Pack` are one exact inverse pair over every row: the deleted bool made the packing a BODY the two members re-branched on, which is what a third range window (a half-signed store, an exposure store) would have had to edit. `Signed` packs its `[-1,1]` value into the storage `[0,1]` span at a normalizing depth and stores it verbatim at float depth, so a normal, a tangent, or a curvature plane carries one declaration and every kernel above reads the signed value whatever the depth beneath it.
- Law: `NormalConvention` homes HERE because green polarity is a property of the stored plane exactly as association and transfer are, and `ToGl` lives on the row because the flip is a green-sign multiply over a DECODED texel with `set#SET_INGEST` its one caller. `gl` is the canonical `+Y` wire form; `dx` is admitted at ingest and converted once through that member or through the equivalent `filter#PLANE_OP` `Swizzle` lane inversion before the plane is keyed, so no plane leaves the module carrying `−Y` green and the silent lighting inversion is unrepresentable.
- Law: `MipPolicy.Kaiser` is the Materials-owned SEPARABLE WINDOWED-SINC halving, and the discriminant against every composed roster is PUBLICATION SHAPE, never absence. The resampler ships `Box`, `Triangle`, `CatmullRom`, `Mitchell`; the kernel `Numerics/calculus#WEIGHT_PROFILES` `WeightKernelFamily` ships `SmoothPoly`, `WendlandC2`, `Gaussian`, `CompactExp`, `Singular`, and the sinc-windowed `Lanczos`; the kernel `Numerics/transform#WINDOW` `WindowTaper` roster DOES carry a real `Kaiser` row — the branch-owned Oppenheim-Schafer design over `SpecialFunctions.BesselI0` — but its publication is an endpoint-aligned width-N window ARRAY, while this fold needs the Kaiser bell at twelve HALF-TEXEL offsets under support 6 inside a sinc product, a tap-grid evaluation no discrete roster array serves. A Kaiser-windowed sinc is a distinct window whose `β` shapes the stopband, so the frozen `kaiser` fold is authored as the `[05]` polyphase kernel over the same composed Bessel surface rather than aliased onto any roster's nearest row, and the fold retires only if the taper owner publishes a continuous-position evaluation rather than a window array. `Box` is the arithmetic 2×2 mean, `NormalRenormalize` folds box and then unit-normalizes each texel vector, `RoughnessVariance` folds box and then absorbs the directional variance its paired normal chain lost at the same level, and `None` declares a single-level plane. Every fold runs in the LINEAR domain — a plane decodes, folds, and re-encodes per level, because averaging `srgb`-encoded texels darkens the pyramid.
- Boundary: rows carry CONVERSION, never storage — a transfer row knows its `TransferFunction` binding, a primaries row knows its `ColorSpace`, its composed geometry, and its assignment token, a range row knows its affine, and a mip row knows its filter and its trait set. `[03]` owns the typed arena consuming them, so a new depth lands as one `IComponent` witness and one `PlaneFormat` row and reaches the whole page without touching an arena, an accessor, or a codec.
- Growth: a new combinable column is one `PlaneTrait` row plus its membership on the owning axis' rows; a new range window is one `PlaneRange` affine pair; a new fold law is one `MipPolicy` row. No arm, no ctor arity, and no consumer moves for any of the three.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Linq;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Materials.Appearance.Bsdf;
using Rasm.Materials.Appearance.Texture;
using Rasm.Numerics;
using Wacton.Unicolour;
using Thinktecture;
using TinyEXR.V3;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlaneQuantity {
    public static readonly PlaneQuantity Light = new("light");
    public static readonly PlaneQuantity Parameter = new("parameter");
    public static readonly PlaneQuantity Display = new("display");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlaneTrait : ICapability<PlaneTrait> {
    public static readonly PlaneTrait Coverage = new("coverage");
    public static readonly PlaneTrait Premultiplied = new("premultiplied");
    public static readonly PlaneTrait Chains = new("chains");
    public static readonly PlaneTrait Renormalize = new("renormalize");
    public static readonly PlaneTrait Coupled = new("coupled");

    private PlaneTrait(string key, int rank) : this(key) => Rank = rank;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlaneTransfer {
    public static readonly PlaneTransfer Linear = new("linear", TransferFunction.Linear, PlaneQuantity.Light, sceneReferred: true);
    public static readonly PlaneTransfer Srgb = new("srgb", TransferFunction.Srgb, PlaneQuantity.Display, sceneReferred: true);
    public static readonly PlaneTransfer Raw = new("raw", function: null, PlaneQuantity.Parameter, sceneReferred: true);
    public static readonly PlaneTransfer Pq = new("pq", TransferFunction.Pq, PlaneQuantity.Display, sceneReferred: false);
    public static readonly PlaneTransfer Hlg = new("hlg", TransferFunction.Hlg, PlaneQuantity.Display, sceneReferred: false);

    public TransferFunction? Function { get; }
    public PlaneQuantity Quantity { get; }
    public bool SceneReferred { get; }
    public bool Identity => Function is null or TransferFunction.Linear;
    private PlaneTransfer(string key, TransferFunction? function, PlaneQuantity quantity, bool sceneReferred) : this(key) =>
        (Function, Quantity, SceneReferred) = (function, quantity, sceneReferred);

    public void Decode(ReadOnlySpan<float> source, Span<float> destination) {
        if (Identity) { source.CopyTo(destination); return; }
        ImageProcessing.DecodeTransfer(source, destination, Function!.Value);
    }

    public void Encode(ReadOnlySpan<float> source, Span<float> destination) {
        if (Identity) { source.CopyTo(destination); return; }
        ImageProcessing.EncodeTransfer(source, destination, Function!.Value);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlanePrimaries {
    public static readonly PlanePrimaries AcesAp1 = new("acesAp1", ColorSpace.AcesAp1, cicp: null, assign: "acescc", profile: RgbProfile.Acescg);
    public static readonly PlanePrimaries Bt709 = new("bt709", ColorSpace.Srgb, cicp: 1, assign: "bt709", profile: RgbProfile.Srgb);
    public static readonly PlanePrimaries Bt2020 = new("bt2020", ColorSpace.Rec2020, cicp: 9, assign: "bt2020", profile: RgbProfile.Rec2020);
    public static readonly PlanePrimaries AcesAp0 = new("acesAp0", ColorSpace.AcesAp0, cicp: null, assign: "aces", profile: RgbProfile.Aces20651);
    public static readonly PlanePrimaries P3D65 = new("p3d65", space: null, cicp: 12, assign: "displayp3", profile: RgbProfile.DisplayP3);
    public static readonly PlanePrimaries Xyz = new("xyz", ColorSpace.Xyz, cicp: 10, assign: "ciexyz", geometry:
        (new Chromaticity(1.0, 0.0), new Chromaticity(0.0, 1.0), new Chromaticity(0.0, 0.0), new Chromaticity(1.0 / 3.0, 1.0 / 3.0)));
    public static readonly PlanePrimaries Unknown = new("unknown", space: null, cicp: null, assign: "none", geometry: None);

    public ColorSpace? Space { get; }
    public int? Cicp { get; }
    public string Assign { get; }
    public Option<(Chromaticity Red, Chromaticity Green, Chromaticity Blue, Chromaticity White)> Geometry { get; }

    private PlanePrimaries(string key, ColorSpace? space, int? cicp, string assign, RgbProfile profile) : this(key) =>
        (Space, Cicp, Assign, Geometry) = (space, cicp, assign, Some(profile.Geometry));

    private PlanePrimaries(string key, ColorSpace? space, int? cicp, string assign,
        Option<(Chromaticity Red, Chromaticity Green, Chromaticity Blue, Chromaticity White)> geometry) : this(key) =>
        (Space, Cicp, Assign, Geometry) = (space, cicp, assign, geometry);

    private static readonly Lazy<Tolerance> CoordinateBand = new(static () =>
        Tolerance.Of(lane: ToleranceLane.Coordinate, value: 1e-3, key: Op.Of(name: nameof(PlanePrimaries)))
            .IfFail(static e => throw e.ToException()));

    private static readonly Lazy<FrozenDictionary<int, PlanePrimaries>> ByCicp =
        new(static () => Items.Where(static row => row.Cicp is not null).ToFrozenDictionary(static row => row.Cicp!.Value));

    private static readonly Lazy<Seq<PlanePrimaries>> Declaring =
        new(static () => toSeq(Items).Filter(static row => row.Geometry.IsSome));

    public static PlanePrimaries Of(Option<Chromaticities> declared) =>
        declared.Bind(row => Declaring.Value.Find(candidate => candidate.Matches(row))).IfNone(Unknown);

    public static PlanePrimaries Of(int cicp) =>
        ByCicp.Value.TryGetValue(cicp, out PlanePrimaries? row) ? row : Unknown;

    public bool Matches(Chromaticities declared) =>
        Geometry.Map(g =>
            Near(g.Red, declared.RedX, declared.RedY) && Near(g.Green, declared.GreenX, declared.GreenY)
            && Near(g.Blue, declared.BlueX, declared.BlueY) && Near(g.White, declared.WhiteX, declared.WhiteY)).IfNone(false);

    private static bool Near(Chromaticity row, double x, double y) =>
        Math.Abs(row.X - x) <= CoordinateBand.Value.Value && Math.Abs(row.Y - y) <= CoordinateBand.Value.Value;

    public Fin<ColorMatrix3x3> Matrix(PlanePrimaries target, Op key) =>
        (Space, target.Space) switch {
            ({ } from, { } to) => Fin.Succ(ImageProcessing.GetColorMatrix(from, to)),
            _ => new MaterialFault.Parameter(key, $"<plane-primaries-unknown:{Key}->{target.Key}>"),
        };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AlphaMode {
    public static readonly AlphaMode Straight = new("straight", CapabilitySet<PlaneTrait>.Of(PlaneTrait.Coverage));
    public static readonly AlphaMode Associated = new("associated", CapabilitySet<PlaneTrait>.Of(PlaneTrait.Coverage, PlaneTrait.Premultiplied));
    public static readonly AlphaMode None = new("none", CapabilitySet<PlaneTrait>.None);

    public CapabilitySet<PlaneTrait> Traits { get; }
    private AlphaMode(string key, CapabilitySet<PlaneTrait> traits) : this(key) => Traits = traits;

    public int ColourLanes(int lanes) => Traits.Admits(PlaneTrait.Coverage) ? lanes - 1 : lanes;
    public int AlphaLane(int lanes) => Traits.Admits(PlaneTrait.Coverage) ? lanes - 1 : -1;

    public bool Convertible(AlphaMode target, ChannelDtype depth) =>
        target == this
        || (!Traits.Admits(PlaneTrait.Premultiplied) && !target.Traits.Admits(PlaneTrait.Premultiplied))
        || !Traits.Admits(PlaneTrait.Coverage)
        || depth != ChannelDtype.Unorm8;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlaneRange {
    public static readonly PlaneRange Unit = new("unit", scale: 1.0, bias: 0.0);
    public static readonly PlaneRange Signed = new("signed", scale: 2.0, bias: -1.0);

    public double Scale { get; }
    public double Bias { get; }
    private PlaneRange(string key, double scale, double bias) : this(key) => (Scale, Bias) = (scale, bias);

    public double Unpack(double stored, ChannelDtype depth) =>
        PlaneFormat.Normalizes(depth) ? (Scale * stored) + Bias : stored;
    public double Pack(double value, ChannelDtype depth) =>
        PlaneFormat.Normalizes(depth) ? (value - Bias) / Scale : value;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NormalConvention {
    public static readonly NormalConvention Gl = new("gl", greenSign: 1.0);
    public static readonly NormalConvention Dx = new("dx", greenSign: -1.0);

    public double GreenSign { get; }
    private NormalConvention(string key, double greenSign) : this(key) => GreenSign = greenSign;

    public ShadeVec4 ToGl(ShadeVec4 decoded) => decoded with { Y = GreenSign * decoded.Y };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MipPolicy {
    public static readonly MipPolicy Box = new("box", ResizeFilter.Box, CapabilitySet<PlaneTrait>.Of(PlaneTrait.Chains));
    public static readonly MipPolicy Kaiser = new("kaiser", filter: null, CapabilitySet<PlaneTrait>.Of(PlaneTrait.Chains));
    public static readonly MipPolicy NormalRenormalize = new("normalRenormalize", ResizeFilter.Box, CapabilitySet<PlaneTrait>.Of(PlaneTrait.Chains, PlaneTrait.Renormalize));
    public static readonly MipPolicy RoughnessVariance = new("roughnessVariance", ResizeFilter.Box, CapabilitySet<PlaneTrait>.Of(PlaneTrait.Chains, PlaneTrait.Coupled));
    public static readonly MipPolicy None = new("none", ResizeFilter.Box, CapabilitySet<PlaneTrait>.None);

    public ResizeFilter? Filter { get; }
    public CapabilitySet<PlaneTrait> Traits { get; }
    private MipPolicy(string key, ResizeFilter? filter, CapabilitySet<PlaneTrait> traits) : this(key) =>
        (Filter, Traits) = (filter, traits);
}
```

## [03]-[PLANE_FORMAT]

- Owner: `IComponent<T>` the static-abstract component witness; `ITexel<TSelf>` the static-abstract texel contract; `Texel1`/`Texel2`/`Texel4` the three storage arities; `PlaneFormat` the twelve-row storage roster over the kernel `ChannelDtype` depths.
- Law: the twelve storage rows are the CROSS PRODUCT of three arities and four component witnesses, applied as type arguments. Twelve hand-written texel records are the deleted form: they share one lane-projection law, so a new depth is one `IComponent` witness and a new arity is one struct, and the roster grows by type application rather than by transcription.
- Law: `IComponent<T>.ToUnit`/`FromUnit` normalize an INTEGER component onto `[0,1]` and pass a floating component verbatim, saturating on the write side so a fold that overshoots stores the clamped value rather than a wrapped one. Witnesses carry the typed-arena TYPE APPLICATION the kernel `ChannelDtype`'s byte-shaped pack and unpack arms cannot express — a `Span<byte>` arm cannot produce a `Texel4<Half, F16>` — so the depth roster is the kernel's and the witness family is this page's, meeting at the one `Normalizes` correspondence below. This is the only normalization in the module; a kernel dividing by `255.0` or `65535.0` re-derives it.
- Law: a three-component semantic channel resolves to the FOUR-component storage row declaring `AlphaMode.None`, and the roster is total over `{1, 2, 4}` at every depth, so `For` never rounds past the arity a channel declares. No odd-width texel exists, so a padded lane is a structural fact the association declares rather than a per-format special case.
- Law: WIRE REACH IS A STORAGE FACT, not a container one. Where no supercompression transcodes the payload, the KTX2 declares the STORE's own Vulkan format, and the browser read path resolves no such row for a one- or two-component 16-bit UNORM store — so `r16` and `rg16` are unreachable on a wire target while every other row is reachable. `PlaneFormat.WebReachable` states that once and `set#TEXTURE_SET` admission reads it, the storage-side twin of the `codec#RASTER_CODEC` `KtxPayload.WireLegal` payload gate; the two are ORTHOGONAL, because a transcodable payload leaves the declared format undefined until transcode and the store class stops mattering there.
- Entry: `PlaneFormat.For(int semanticComponents, ChannelDtype depth)` is the ONE resolution both the press binding and the neural stage read; `PlaneFormat.Normalizes(ChannelDtype)` is the ONE normalization discriminant every packing site reads; `PlaneFormat.WebReachable(int, ChannelDtype)` the ONE wire-target discriminant, surfaced per row as `WebLegal`; `Items` is the ordered roster; `Get`/`TryGet` resolve a wire key.
- Packages: CommunityToolkit.HighPerformance (`MemoryOwner<T>.Allocate(int, AllocationMode)` the pooled rental every row's `Rent` column binds, `Memory<T>.AsMemory2D(int, int)` the plane projection), `Rasm.Drawing` (composed — `ChannelDtype.Unorm8`/`Unorm16`/`Float16`/`Float32` the storage-component rows and their `Width` column), Thinktecture.Runtime.Extensions, BCL inbox (`Half`, `double.Clamp`, `Array.MaxLength`).
- Growth: a new depth is one `IComponent` witness with its rows, one `Normalizes` arm, and one `WebReachable` arm; a new arity is one texel struct with its rows; a new storage row is one `PlaneFormat` declaration naming its arity, witness, component count, depth, and alpha. Nothing else on this page, in the codec, in the filter, or in the pyramid changes — the `Rent` column carries the type application and every consumer stays generic over `ITexel`.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
public interface IComponent<T> where T : unmanaged {
    static abstract double ToUnit(T value);
    static abstract T FromUnit(double value);
}

public readonly struct U8 : IComponent<byte> {
    public static double ToUnit(byte value) => value / 255.0;
    public static byte FromUnit(double value) => (byte)Math.Round(double.Clamp(value, 0.0, 1.0) * 255.0);
}

public readonly struct U16 : IComponent<ushort> {
    public static double ToUnit(ushort value) => value / 65535.0;
    public static ushort FromUnit(double value) => (ushort)Math.Round(double.Clamp(value, 0.0, 1.0) * 65535.0);
}

public readonly struct F16 : IComponent<Half> {
    public static double ToUnit(Half value) => (double)value;
    public static Half FromUnit(double value) => (Half)value;
}

public readonly struct F32 : IComponent<float> {
    public static double ToUnit(float value) => value;
    public static float FromUnit(double value) => (float)value;
}

public interface ITexel<TSelf> where TSelf : unmanaged, ITexel<TSelf> {
    static abstract int Lanes { get; }
    static abstract void Project(in TSelf texel, Span<double> lanes);
    static abstract TSelf Compose(ReadOnlySpan<double> lanes);
}

public readonly record struct Texel1<TC, TW>(TC C0) : ITexel<Texel1<TC, TW>>
    where TC : unmanaged where TW : struct, IComponent<TC> {
    public static int Lanes => 1;
    public static void Project(in Texel1<TC, TW> texel, Span<double> lanes) => lanes[0] = TW.ToUnit(texel.C0);
    public static Texel1<TC, TW> Compose(ReadOnlySpan<double> lanes) => new(TW.FromUnit(lanes[0]));
}

public readonly record struct Texel2<TC, TW>(TC C0, TC C1) : ITexel<Texel2<TC, TW>>
    where TC : unmanaged where TW : struct, IComponent<TC> {
    public static int Lanes => 2;
    public static void Project(in Texel2<TC, TW> texel, Span<double> lanes) {
        lanes[0] = TW.ToUnit(texel.C0);
        lanes[1] = TW.ToUnit(texel.C1);
    }
    public static Texel2<TC, TW> Compose(ReadOnlySpan<double> lanes) =>
        new(TW.FromUnit(lanes[0]), TW.FromUnit(lanes[1]));
}

public readonly record struct Texel4<TC, TW>(TC C0, TC C1, TC C2, TC C3) : ITexel<Texel4<TC, TW>>
    where TC : unmanaged where TW : struct, IComponent<TC> {
    public static int Lanes => 4;
    public static void Project(in Texel4<TC, TW> texel, Span<double> lanes) {
        lanes[0] = TW.ToUnit(texel.C0);
        lanes[1] = TW.ToUnit(texel.C1);
        lanes[2] = TW.ToUnit(texel.C2);
        lanes[3] = TW.ToUnit(texel.C3);
    }
    public static Texel4<TC, TW> Compose(ReadOnlySpan<double> lanes) =>
        new(TW.FromUnit(lanes[0]), TW.FromUnit(lanes[1]), TW.FromUnit(lanes[2]), TW.FromUnit(lanes[3]));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlaneFormat {
    public static readonly PlaneFormat R8 = Row("r8", 1, ChannelDtype.Unorm8, AlphaMode.None, PlaneStore.Rent<Texel1<byte, U8>>);
    public static readonly PlaneFormat R16 = Row("r16", 1, ChannelDtype.Unorm16, AlphaMode.None, PlaneStore.Rent<Texel1<ushort, U16>>);
    public static readonly PlaneFormat R16F = Row("r16f", 1, ChannelDtype.Float16, AlphaMode.None, PlaneStore.Rent<Texel1<Half, F16>>);
    public static readonly PlaneFormat R32F = Row("r32f", 1, ChannelDtype.Float32, AlphaMode.None, PlaneStore.Rent<Texel1<float, F32>>);
    public static readonly PlaneFormat Rg8 = Row("rg8", 2, ChannelDtype.Unorm8, AlphaMode.None, PlaneStore.Rent<Texel2<byte, U8>>);
    public static readonly PlaneFormat Rg16 = Row("rg16", 2, ChannelDtype.Unorm16, AlphaMode.None, PlaneStore.Rent<Texel2<ushort, U16>>);
    public static readonly PlaneFormat Rg16F = Row("rg16f", 2, ChannelDtype.Float16, AlphaMode.None, PlaneStore.Rent<Texel2<Half, F16>>);
    public static readonly PlaneFormat Rg32F = Row("rg32f", 2, ChannelDtype.Float32, AlphaMode.None, PlaneStore.Rent<Texel2<float, F32>>);
    public static readonly PlaneFormat Rgba8 = Row("rgba8", 4, ChannelDtype.Unorm8, AlphaMode.Straight, PlaneStore.Rent<Texel4<byte, U8>>);
    public static readonly PlaneFormat Rgba16 = Row("rgba16", 4, ChannelDtype.Unorm16, AlphaMode.Straight, PlaneStore.Rent<Texel4<ushort, U16>>);
    public static readonly PlaneFormat Rgba16F = Row("rgba16f", 4, ChannelDtype.Float16, AlphaMode.Straight, PlaneStore.Rent<Texel4<Half, F16>>);
    public static readonly PlaneFormat Rgba32F = Row("rgba32f", 4, ChannelDtype.Float32, AlphaMode.Straight, PlaneStore.Rent<Texel4<float, F32>>);

    public int Components { get; }
    public ChannelDtype Depth { get; }
    public AlphaMode Alpha { get; }
    public Func<int, int, AllocationMode, PlaneStore> Rent { get; }
    public long BytesPerTexel => (long)Components * Depth.Width;
    public bool Normalized => Normalizes(Depth);
    public bool WebLegal => WebReachable(Components, Depth);
    private static readonly Lazy<int> WidestArity = new(static () => Items.Max(static row => row.Components));

    public static int MaxComponents => WidestArity.Value;

    public static bool Normalizes(ChannelDtype depth) => depth == ChannelDtype.Unorm8 || depth == ChannelDtype.Unorm16;

    public static bool WebReachable(int components, ChannelDtype depth) =>
        depth != ChannelDtype.Unorm16 || components >= 4;

    private PlaneFormat(string key, int components, ChannelDtype depth, AlphaMode alpha, Func<int, int, AllocationMode, PlaneStore> rent)
        : this(key) => (Components, Depth, Alpha, Rent) = (components, depth, alpha, rent);
    private static PlaneFormat Row(string key, int components, ChannelDtype depth, AlphaMode alpha, Func<int, int, AllocationMode, PlaneStore> rent) =>
        new(key, components, depth, alpha, rent);

    public static Option<PlaneFormat> For(int semanticComponents, ChannelDtype depth) =>
        toSeq(Items.Where(row => row.Depth == depth && row.Components >= Math.Max(1, semanticComponents))
                   .OrderBy(static row => row.Components))
             .Head;
}
```

## [04]-[TEXTURE_PLANE]

- Owner: `PlaneStore` the arena base with its `IPlaneFold` re-entry interface; `PlaneStore<T>` the ONE generic realization; `TexturePlane` the admitted plane carrying format, grid, layers, transfer, primaries, association, range, and store.
- Entry: `TexturePlane.Of` is ONE admission over two input modalities discriminating on shape, never a knob — the EXTENT modality `(format, width, height, transfer, alpha, key, layers, range, primaries, pitchMm, mode)` seats a fresh `CellLattice` and the LATTICE modality `(format, grid, layers, transfer, alpha, range, primaries, key, mode)` adopts one a caller already holds (a pyramid level, a world-seated bake target, a re-association twin); the trailing `AllocationMode` defaults to `Clear` so a press writing every texel opts out of the zeroing pass explicitly. `Read(row, layer, lanes)`/`Write(row, layer, lanes)` are the one decoded LANE row accessor and `ReadShade(row, layer, texels)`/`WriteShade(row, layer, texels)` its `ShadeVec4` projection — the tile, set, press, and environment folds all stage `ShadeVec4` rows, so the lane-to-register correspondence (single-lane replication, two-lane X/Y with zero Z, alpha seat, four-lane identity) is declared ONCE here rather than re-derived per consumer; `RowScalars` sizes a consumer's lane scratch; `Run(steps)` is the one spatial-grain read; `Layer(index, key)` windows one layer; `ToAlpha(target, key)` and `ToPrimaries(target, key)` are the two declaration crossings; `Key` is the streaming content key.
- Law: the EXTENT SPINE is the kernel `Numerics/atoms#CELL_LATTICE` `CellLattice` and this page mints none. `Width` and `Height` read `Grid.Columns` and `Grid.Rows`, `Linear` is the lattice's own linearization, the `Array.MaxLength` element budget is the lattice's `ceiling` argument, and `Coarsen` is the `[05]` level step — so a texel grid, a voxel sweep, an overview chain, and a Fabrication field all address through one owner. Admission seats the lattice at `Layers = 1` and `TexturePlane` keeps its OWN layer band, because the plane's layers are a STACKING axis whose law `set#TEXTURE_SET` `LayerLaw` names — cube faces, array slices, flipbook frames — and `Coarsen` halves every lattice axis: folding the band into the lattice would halve six cube faces to three at the first mip level.
- Law: the SPATIAL GRAIN rides the affine and the READ CARRIES NO UNIT IN ITS NAME, because it does not carry one in its value: a pixel plane seats the identity map, so its cell measures one texel and `Run` returns a texel-unit run; a physically-pitched plane seats its millimetres-per-texel as a uniform scale, so `Run` returns millimetres and the same relief at two resolutions derives one horizon, one curvature magnitude, and one gradient slope. A millimetre suffix here would assert the pitched case on every identity-seated plane in the corpus and collide with the genuinely-millimetre `Component/joint#JOINT_FAMILY` weld run under one spelling; the typed-absence arm is the honest one, so the AFFINE is the unit witness and a caller needing physical units seats a pitch. Grain is therefore a property of the grid every derivative reads off the plane it is differentiating, never a column each derivative carrier re-declares. `Run(columns, rows)` takes the march as a PER-AXIS texel count and returns its Euclidean length through `Grid.CellSize`, so an anisotropic seat is honoured rather than approximated: reading `CellSize.X` alone would report a vertical sweep's rise over a horizontal spacing, and every consumer marching a direction — the horizon sweep, the curvature stencil, the gradient slope — passes the direction it actually walked.
- Law: a ten-case store union is the DELETED form. Ten cases carried one field pair and one disposal, so the arena is one generic record and typed code re-enters through `Accept<TFold, TResult>` — a `struct` or `ref struct` fold the JIT specializes per texel, allocating nothing and capturing nothing. `PlaneFormat` rows carry their own `Rent` column, deleting the `format.Key` switch that throws on an unmatched row: an unmatched format is unrepresentable rather than an exception in a fallible path.
- Law: `Of` refuses BEFORE it rents. `MaterialFault.Parameter` fails an association the storage row cannot hold and an element count above `Array.MaxLength`, each carrying the offending axis in its reason; a non-positive extent is unrepresentable because `Dimension` admits at one and above. `Grid.CellCount × Layers` texels bound admission and make the typed arena worth its shape: a 16k four-lane 16-bit plane counts 268 435 456 and admits, while the same plane's byte count exceeds the runtime bound.
- Law: layer `n` occupies rows `[n × height, (n + 1) × height)` of one arena. `Layer` windows that band without a second rental and without touching the grid, so a cube face set, an array slice, a volume slab, and a flipbook frame are all one plane and the `set#TEXTURE_SET` `LayerLaw` row is the only thing that names which.
- Law: `Read` runs ONE decode ladder and `Write` runs its exact inverse — texel lanes, component normalization, alpha un-association, `PlaneRange` unpack, transfer decode over the colour lanes alone. Un-association precedes the unpack because coverage weights the STORED value; every `Signed` row on the roster carries `AlphaMode.None`, so the two steps never both fire on one plane and the order is stated for the ladder's own coherence. Every consumer above this page reads decoded, signed, scene-linear lanes, so no kernel in the module re-derives a curve, re-divides by a maximum, or re-packs a signed field. `Read` leaves the alpha lane untouched by every transfer: an association is a linear coverage weight, and running a display curve over it darkens every edge.
- Law: `ToAlpha` and `ToPrimaries` are the two DECLARATION CROSSINGS and both convert on decoded lanes. `ToAlpha` refuses the `straight`↔`associated` crossing below 16 bits, because at eight bits the un-association divides by a quantized coverage and a low-alpha texel amplifies its own quantization step into a visible colour error the round trip cannot recover. `ToPrimaries` refuses an unknown endpoint, because a gamut rebase against an undeclared chromaticity is a fabricated transform; a decode RECORDS the primaries its container declared and never converts, so the one conversion site is a caller stating both ends.
- Law: the content key is the kernel `ContentHash.Of` streaming entry over the plane's own storage rows in layer-major, row-major order, seeded zero like every other federation key. `ContentHash.Of` is the sole mint site, holding the federation seed and the cross-branch digest reproduction; the whole-plane byte span is never materialized, so a 268-million-texel plane keys in one pooled row window.
- Packages: CommunityToolkit.HighPerformance (`MemoryOwner<T>.Allocate`/`.Memory`/`.Dispose`, `AllocationMode.Clear`/`Default`, `Memory<T>.AsMemory2D(int, int)`, `Memory2D<T>.Span`, `Memory2D<T>.Slice(int, int, int, int)`, `Span2D<T>.GetRowSpan(int)`, `SpanOwner<T>.Allocate` the per-row lane scratch), `Rasm.Domain` (`ContentHash.Of<TState>(TState, Action<TState, XxHash128>)` the ONE identity entry, `Op`), `Rasm.Numerics` (composed — `CellLattice.Of`/`Columns`/`Rows`/`CellCount`/`CellSize`/`Linear`/`Coarsen` the ONE bounded cell lattice, `Placement.Build` + `TransformSpec.UniformScale` the one transform mint, `Dimension`, `PositiveMagnitude`), `bsdf#SHADING_FRAME` (`MaterialFault` band 2450), TinyEXR.NET (composed — `ImageProcessing.GetColorMatrix(ColorSpace, ColorSpace) -> ColorMatrix3x3` the ONE reconciliation mint and `ImageProcessing.ApplyColorMatrix(ReadOnlySpan<float>, Span<float>, int, ColorMatrix3x3)` the interleaved fold), RhinoCommon (`Transform.Identity`, `Point3d.Origin` at the affine seat alone), `System.IO.Hashing` (`XxHash128.Append` inside the kernel entry alone), BCL inbox (`Array.MaxLength`, `double.Hypot`, `MemoryMarshal.AsBytes`).
- Boundary: the arena is TYPED end to end and exposes no whole-plane byte view. `MemoryMarshal.AsBytes` over one row span is the sole reinterpretation, taken by the key fold and by the codec bridge, so a caller cannot address the plane as bytes and no consumer can smuggle a depth reinterpretation past the format row. `AllocationMode.Clear` is the admission default because a partially-written plane must read its neutral rather than pool residue, and a press writing every texel passes `AllocationMode.Default` to skip the zeroing pass over a quarter-billion elements.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.IO.Hashing;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using Rasm.Domain;
using Rasm.Materials.Appearance.Bsdf;
using Rasm.Materials.Appearance.Texture;
using Rasm.Numerics;
using Rhino.Geometry;
using TinyEXR.V3;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [MODELS] --------------------------------------------------------------------------
public interface IPlaneFold<TResult> {
    TResult Fold<T>(Memory2D<T> view) where T : unmanaged, ITexel<T>;
}

public abstract record PlaneStore : IDisposable {
    public abstract int Lanes { get; }
    public abstract void Dispose();
    public abstract TResult Accept<TFold, TResult>(scoped in TFold fold)
        where TFold : struct, IPlaneFold<TResult>, allows ref struct;
    public abstract PlaneStore Window(int rowOffset, int rows);

    public static PlaneStore Rent<T>(int width, int rows, AllocationMode mode) where T : unmanaged, ITexel<T> {
        MemoryOwner<T> owner = MemoryOwner<T>.Allocate(checked(width * rows), mode);
        return new PlaneStore<T>(Some(owner), owner.Memory.AsMemory2D(rows, width));
    }
}

public sealed record PlaneStore<T>(Option<MemoryOwner<T>> Owner, Memory2D<T> View) : PlaneStore
    where T : unmanaged, ITexel<T> {
    public override int Lanes => T.Lanes;
    public override void Dispose() => Owner.Iter(static owner => owner.Dispose());
    public override TResult Accept<TFold, TResult>(scoped in TFold fold) => fold.Fold(View);
    public override PlaneStore Window(int rowOffset, int rows) =>
        new PlaneStore<T>(Option<MemoryOwner<T>>.None, View.Slice(rowOffset, 0, rows, View.Width));
}

public sealed record TexturePlane(
    PlaneFormat Format,
    CellLattice Grid,
    Dimension Layers,
    PlaneTransfer Transfer,
    PlanePrimaries Primaries,
    AlphaMode Alpha,
    PlaneRange Range,
    PlaneStore Store) : IDisposable {

    private static readonly Dimension Single = Dimension.Create(value: 1);

    public static Fin<TexturePlane> Of(
        PlaneFormat format, Dimension width, Dimension height, PlaneTransfer transfer, AlphaMode alpha, Op key,
        Option<Dimension> layers = default, Option<PlaneRange> range = default, Option<PlanePrimaries> primaries = default,
        Option<PositiveMagnitude> pitchMm = default, AllocationMode mode = AllocationMode.Clear) =>
        from map in Seat(pitchMm, key)
        from grid in CellLattice.Of(map, width, height, Single, Array.MaxLength, key)
        from plane in Of(format, grid, layers.IfNone(Single), transfer, alpha,
            range.IfNone(PlaneRange.Unit), primaries.IfNone(PlanePrimaries.Unknown), key, mode)
        select plane;

    public static Fin<TexturePlane> Of(
        PlaneFormat format, CellLattice grid, Dimension layers, PlaneTransfer transfer, AlphaMode alpha,
        PlaneRange range, PlanePrimaries primaries, Op key, AllocationMode mode = AllocationMode.Clear) {
        long rows = (long)grid.Rows.Value * layers.Value;
        long elements = grid.CellCount * layers.Value;
        return (elements, !alpha.Traits.Admits(PlaneTrait.Coverage) || format.Alpha.Traits.Admits(PlaneTrait.Coverage)) switch {
            ( > Array.MaxLength, _) => new MaterialFault.Parameter(key, $"<plane-elements:{elements}>"),
            (_, false) => new MaterialFault.Parameter(key, $"<plane-alpha-storage:{alpha.Key}!={format.Alpha.Key}>"),
            _ => Fin.Succ(new TexturePlane(format, grid, layers, transfer, primaries, alpha, range,
                     format.Rent(grid.Columns.Value, checked((int)rows), mode))),
        };
    }

    private static Fin<Transform> Seat(Option<PositiveMagnitude> pitchMm, Op key) =>
        pitchMm.TraverseM(pitch => Placement.Build(
            new TransformSpec.UniformScale(Point3d.Origin, pitch.Value), key: key)).As()
            .Map(static map => map.IfNone(Transform.Identity));

    public Dimension Width => Grid.Columns;
    public Dimension Height => Grid.Rows;
    public int Lanes => Store.Lanes;
    public int RowScalars => Width.Value * Lanes;
    public long Texels => Grid.CellCount * Layers.Value;
    public double Run(int columns, int rows) => double.Hypot(columns * Grid.CellSize.X, rows * Grid.CellSize.Y);

    public Fin<TexturePlane> Layer(int index, Op key) =>
        index >= 0 && index < Layers.Value
            ? Fin.Succ(this with { Layers = Single, Store = Store.Window(index * Height.Value, Height.Value) })
            : new MaterialFault.Parameter(key, $"<plane-layer:{index}:{Layers.Value}>");

    public void Read(int row, int layer, Span<double> lanes) =>
        Store.Accept<RowRead, Unit>(new RowRead(this, (layer * Height.Value) + row, lanes));

    public void Write(int row, int layer, ReadOnlySpan<double> lanes) =>
        Store.Accept<RowWrite, Unit>(new RowWrite(this, (layer * Height.Value) + row, lanes));

    public void ReadShade(int row, int layer, Span<ShadeVec4> texels) {
        using SpanOwner<double> lanes = SpanOwner<double>.Allocate(RowScalars);
        Read(row, layer, lanes.Span);
        int stride = Lanes, colour = Alpha.ColourLanes(stride), alpha = Alpha.AlphaLane(stride);
        for (int x = 0; x < Width.Value; x++) {
            ReadOnlySpan<double> texel = lanes.Span.Slice(x * stride, stride);
            texels[x] = new ShadeVec4(
                texel[0],
                colour > 1 ? texel[1] : texel[0],
                colour > 2 ? texel[2] : (colour == 2 ? 0.0 : texel[0]),
                alpha >= 0 ? texel[alpha] : 1.0);
        }
    }

    public void WriteShade(int row, int layer, ReadOnlySpan<ShadeVec4> texels) {
        using SpanOwner<double> lanes = SpanOwner<double>.Allocate(RowScalars);
        int stride = Lanes, colour = Alpha.ColourLanes(stride), alpha = Alpha.AlphaLane(stride);
        for (int x = 0; x < Width.Value; x++) {
            Span<double> texel = lanes.Span.Slice(x * stride, stride);
            texel[0] = texels[x].X;
            if (colour > 1) { texel[1] = texels[x].Y; }
            if (colour > 2) { texel[2] = texels[x].Z; }
            if (alpha >= 0) { texel[alpha] = texels[x].W; }
        }
        Write(row, layer, lanes.Span);
    }

    public Fin<TexturePlane> ToAlpha(AlphaMode target, Op key) =>
        target == Alpha ? Fin.Succ(this)
        : !Alpha.Convertible(target, Format.Depth)
            ? new MaterialFault.Parameter(key, $"<plane-alpha-crossing:{Alpha.Key}->{target.Key}:{Format.Depth.Key}>")
        : Of(Format, Grid, Layers, Transfer, target, Range, Primaries, key, AllocationMode.Default)
              .Map(Reassociate);

    public Fin<TexturePlane> ToPrimaries(PlanePrimaries target, Op key) =>
        target == Primaries
            ? Fin.Succ(this)
            : from matrix in Primaries.Matrix(target, key)
              from destination in Of(Format, Grid, Layers, Transfer, Alpha, Range, target, key, AllocationMode.Default)
              select Rebase(destination, matrix);

    private TexturePlane Reassociate(TexturePlane destination) {
        using SpanOwner<double> lanes = SpanOwner<double>.Allocate(RowScalars);
        for (int layer = 0; layer < Layers.Value; layer++) {
            for (int row = 0; row < Height.Value; row++) {
                Read(row, layer, lanes.Span);
                destination.Write(row, layer, lanes.Span);
            }
        }
        return destination;
    }

    private TexturePlane Rebase(TexturePlane destination, ColorMatrix3x3 matrix) {
        int colour = Alpha.ColourLanes(Lanes);
        using SpanOwner<double> lanes = SpanOwner<double>.Allocate(RowScalars);
        using SpanOwner<float> staged = SpanOwner<float>.Allocate(Width.Value * colour);
        for (int layer = 0; layer < Layers.Value; layer++) {
            for (int row = 0; row < Height.Value; row++) {
                Read(row, layer, lanes.Span);
                for (int x = 0; x < Width.Value; x++) {
                    for (int c = 0; c < colour; c++) { staged.Span[(x * colour) + c] = (float)lanes.Span[(x * Lanes) + c]; }
                }
                ImageProcessing.ApplyColorMatrix(staged.Span, staged.Span, colour, matrix);
                for (int x = 0; x < Width.Value; x++) {
                    for (int c = 0; c < colour; c++) { lanes.Span[(x * Lanes) + c] = staged.Span[(x * colour) + c]; }
                }
                destination.Write(row, layer, lanes.Span);
            }
        }
        return destination;
    }

    public UInt128 Key => ContentHash.Of(this, static (plane, hash) => plane.Store.Accept<KeyRows, Unit>(new KeyRows(hash)));

    public void Dispose() => Store.Dispose();
}
```

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
namespace Rasm.Materials.Raster;

internal readonly ref struct RowRead(TexturePlane plane, int storageRow, Span<double> lanes) : IPlaneFold<Unit> {
    public Unit Fold<T>(Memory2D<T> view) where T : unmanaged, ITexel<T> {
        ReadOnlySpan<T> source = view.Span.GetRowSpan(storageRow);
        int width = source.Length, stride = T.Lanes;
        int colour = plane.Alpha.ColourLanes(stride), alpha = plane.Alpha.AlphaLane(stride);
        bool premultiplied = plane.Alpha.Traits.Admits(PlaneTrait.Premultiplied);
        using SpanOwner<float> encoded = SpanOwner<float>.Allocate(width * colour);
        for (int x = 0; x < width; x++) {
            T.Project(in source[x], lanes.Slice(x * stride, stride));
            double coverage = alpha >= 0 ? lanes[(x * stride) + alpha] : 1.0;
            for (int c = 0; c < colour; c++) {
                double unit = lanes[(x * stride) + c];
                double straight = premultiplied && coverage > 0.0 ? unit / coverage : unit;
                encoded[(x * colour) + c] = (float)plane.Range.Unpack(straight, plane.Format.Depth);
            }
        }
        Span<float> linear = encoded.Span;
        plane.Transfer.Decode(encoded.Span, linear);
        for (int x = 0; x < width; x++) {
            for (int c = 0; c < colour; c++) { lanes[(x * stride) + c] = linear[(x * colour) + c]; }
        }
        return Unit.Default;
    }
}

internal readonly ref struct RowWrite(TexturePlane plane, int storageRow, ReadOnlySpan<double> lanes) : IPlaneFold<Unit> {
    public Unit Fold<T>(Memory2D<T> view) where T : unmanaged, ITexel<T> {
        Span<T> destination = view.Span.GetRowSpan(storageRow);
        int width = destination.Length, stride = T.Lanes;
        int colour = plane.Alpha.ColourLanes(stride), alpha = plane.Alpha.AlphaLane(stride);
        bool premultiplied = plane.Alpha.Traits.Admits(PlaneTrait.Premultiplied);
        using SpanOwner<float> linear = SpanOwner<float>.Allocate(width * colour);
        using SpanOwner<double> texel = SpanOwner<double>.Allocate(stride);
        for (int x = 0; x < width; x++) {
            for (int c = 0; c < colour; c++) { linear[(x * colour) + c] = (float)lanes[(x * stride) + c]; }
        }
        Span<float> encoded = linear.Span;
        plane.Transfer.Encode(linear.Span, encoded);
        for (int x = 0; x < width; x++) {
            double coverage = alpha >= 0 ? lanes[(x * stride) + alpha] : 1.0;
            for (int c = 0; c < colour; c++) {
                double packed = plane.Range.Pack(encoded[(x * colour) + c], plane.Format.Depth);
                texel.Span[c] = premultiplied ? packed * coverage : packed;
            }
            if (alpha >= 0) { texel.Span[alpha] = coverage; }
            destination[x] = T.Compose(texel.Span);
        }
        return Unit.Default;
    }
}

internal readonly struct KeyRows(XxHash128 hash) : IPlaneFold<Unit> {
    public Unit Fold<T>(Memory2D<T> view) where T : unmanaged, ITexel<T> {
        for (int row = 0; row < view.Height; row++) { hash.Append(MemoryMarshal.AsBytes(view.Span.GetRowSpan(row))); }
        return Unit.Default;
    }
}
```

## [05]-[TEXTURE_PYRAMID]

- Owner: `TexturePyramid` the ordered level chain, its `MipPolicy` row, its coupling evidence, and the one sampler bridge.
- Entry: `TexturePyramid.Of(TexturePlane basePlane, MipPolicy policy, Op key, Option<TexturePyramid> paired = default)` BUILDS the chain — one entry over both the single-level and the full-chain modalities, discriminating on the policy row rather than on a caller flag; `Base`, `Levels`, `Coupled`, `Key`, and `AsImage(key)` are the reads. Its positional ctor stays PUBLIC for composition owners whose levels pre-exist the chain — a container decode (`codec#RASTER_CODEC` `Materialize`), a per-level pack compose (`press#TEXTURE_PRESS`), an ingest lift (`set#SET_INGEST`) — each holding the halving law by its own construction; `Of` remains the ONE folding mint, and a ctor call that FOLDS is the defect this line names.
- Law: `MipPolicy.None` admits exactly one level and the pyramid ADOPTS the base plane rather than copying it. Every chaining row descends the base plane's own `CellLattice` through the kernel `Coarsen` step — halved census with a floor of one, doubled cell, same ceiling — so the chain ends where the census stops moving, the level count is a CONSEQUENCE of the lattice rather than a `log2` this page re-derives, and each level's grid carries its own doubled spatial grain into every derivative that reads it.
- Law: every fold runs in the LINEAR domain over decoded rows. Each level decodes through `TexturePlane.Read`, resamples through the policy's separable filter, and re-encodes through `TexturePlane.Write`, so an `srgb` chain does not darken and a `signed` chain does not drift toward its packing midpoint.
- Law: `NormalRenormalize` unit-normalizes the leading three lanes after the fold, because averaging unit vectors shortens them and a shortened normal reads as a tilted one at every distance.
- Law: `RoughnessVariance` is PAIRED and its coupling is quantitative: the paired normal level's mean-vector length `L` carries the directional variance the fold destroyed, so the level's roughness becomes `min(1, sqrt(r² + 2(1 − L)/L))` and specular aliasing does not reappear at distance. `RoughnessVariance` admits a build carrying no paired chain — the row folds under `Box` and records `Coupled: false`, which `press#PRESS_PRODUCT` publishes as the declared quality floor, because a set whose normal chain has not yet been tiled must still produce a roughness chain.
- Law: `AsImage` MATERIALIZES the chain into the sampler's own carrier — each level's decoded lanes projected into `ShadeVec4` COPIES the sampler owns outright, so the returned `TextureSource.Image` is INDEPENDENT of the pyramid's arenas and disposing the pyramid after the lift is legal; the ownership crossing is a copy, never a view, which is what lets a consumer hold the sampler past the chain's lifetime without a use-after-free the type cannot see. This module mints no second sampler, no second reconstruction, and no second address mode. `TextureSource.Image` carries one layer, so the lift runs PER LAYER by construction: a multi-layer plane refuses and the caller extracts a `Layer` first, which is exactly what makes the cube-face and array arms honest rather than declared capability that cannot run.
- Law: `AsImage` COSTS a full second residency and the cost is DECLARED rather than discovered. The lift copies every level into `ShadeVec4` — four doubles, thirty-two bytes a texel — and the geometric chain sums to `4/3` of the base census, so a 4k chain materializes ≈683 MiB and a 16k chain ≈10.7 GiB on top of the arena the pyramid already holds. `Texels` publishes that census so a caller budgets before it lifts. The lift is therefore a DELIBERATE per-plane act: `tile#TILE_GATE` lifts once per plane to grade a set and `tile#TILE_SYNTH` lifts once per plane to solve it, so a full-channel set pays the ceiling once per graded plane and never once per level or once per tap, and a caller lifting inside a per-channel loop over a large set must band its own walk instead. `[06]` is the shape a residency-bounded caller reaches for when the whole grid will not fit.
- Packages: `plane#TEXTURE_PLANE` (composed — `TexturePlane.Of` over both modalities, `Read`/`Write`/`Layer`/`Grid`/`Key`, `PlaneFormat`, `MipPolicy`), `Rasm.Numerics` (composed — `CellLattice.Coarsen` the ONE level step, `CellLattice.Columns`/`Rows`), `texture#TEXTURE_UV` (composed — `TextureSource.Image.Of(Dimension, Dimension, Seq<ReadOnlyMemory<ShadeVec4>>, Op)` the ONE sampler admission, `ShadeVec4` the four-lane field register), TinyEXR.NET (composed — `ImageProcessing.Resize(ReadOnlySpan<float>, int, int, Span<float>, int, int, int, ResizeFilter, EdgeMode, int)` the delegated separable downsample rows, `EdgeMode.Clamp` at a chain boundary; the `kaiser` row folds through the local windowed-sinc table instead, because the kernel `Numerics/transform#WINDOW` `WindowTaper.Kaiser` publishes an endpoint-aligned window array, not the half-texel tap-grid evaluation this polyphase table needs), MathNet.Numerics (composed — `SpecialFunctions.BesselI0(double)` the zeroth-order modified Bessel evaluation the Kaiser window needs, so no local power series exists), System.Numerics.Tensors (`TensorPrimitives.ConvertTruncating<double,float>`/`ConvertChecked<float,double>` the two whole-run staging crossings between the ladder's double domain and the resampler's float one, `TensorPrimitives.Divide(ReadOnlySpan<T>, T, Span<T>)` the tap table's unit-sum normalization), CommunityToolkit.HighPerformance (`SpanOwner<T>.Allocate` the per-level staging), `Rasm.Domain` (`ContentHash.Of`, `Op`), LanguageExt.Core.
- Growth: a new fold law is one `MipPolicy` row carrying its filter and its post-fold flags; a new coupling is one flag with its arm in the post-fold. Neither the chain walk, the level admission, the sampler bridge, nor any consumer changes.
- Boundary: arbitrary-ratio resampling is `filter#PLANE_OP` `Resize` — a mip level is the lattice's own `Coarsen` step under a declared policy and never a resize alias, so a chain cannot be minted at an arbitrary ratio and a resize cannot silently produce a level a sampler then trilinearly blends. `TexturePyramid` OWNS its levels and disposes them; a pyramid built over an adopted base at `MipPolicy.None` disposes that base too, so ownership is uniform and a caller never holds a half-owned chain.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Buffers.Binary;
using System.Numerics.Tensors;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using MathNet.Numerics;
using Rasm.Domain;
using Rasm.Materials.Appearance.Bsdf;
using Rasm.Materials.Appearance.Texture;
using Rasm.Numerics;
using TinyEXR.V3;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record TexturePyramid(Seq<TexturePlane> Levels, MipPolicy Policy, bool Coupled) : IDisposable {
    public TexturePlane Base => Levels[0];

    public long Texels => Levels.Fold(0L, static (sum, level) => sum + level.Texels);

    public UInt128 Key => ContentHash.Of(this, static (chain, hash) => chain.Levels.Iter(level => {
        Span<byte> digest = stackalloc byte[16];
        BinaryPrimitives.WriteUInt128LittleEndian(digest, level.Key);
        hash.Append(digest);
    }));

    public static Fin<TexturePyramid> Of(TexturePlane basePlane, MipPolicy policy, Op key, Option<TexturePyramid> paired = default) =>
        !policy.Traits.Admits(PlaneTrait.Chains)
            ? Fin.Succ(new TexturePyramid(Seq(basePlane), policy, Coupled: false))
            : Chain(basePlane, policy, paired, key);

    private static Fin<TexturePyramid> Chain(TexturePlane basePlane, MipPolicy policy, Option<TexturePyramid> paired, Op key) {
        bool coupled = policy.Traits.Admits(PlaneTrait.Coupled) && paired.IsSome;
        return Descend(basePlane.Grid, key).Bind(grids =>
            grids.FoldM(Seq(basePlane), (levels, grid) =>
                Fold(levels[levels.Count - 1], grid, policy, coupled ? paired.Bind(chain => Level(chain, levels.Count)) : None, key)
                    .Map(levels.Add)).As())
            .Map(levels => new TexturePyramid(levels, policy, coupled));
    }

    private static Fin<Seq<CellLattice>> Descend(CellLattice grid, Op key) =>
        grid.Columns.Value is 1 && grid.Rows.Value is 1
            ? Fin.Succ(Seq<CellLattice>.Empty)
            : grid.Coarsen(key)
                .Bind(level => Descend(level, key).Map(rest => level.Cons(rest)));

    private static Option<TexturePlane> Level(TexturePyramid chain, int index) =>
        index < chain.Levels.Count ? Some(chain.Levels[index]) : None;

    private static Fin<TexturePlane> Fold(TexturePlane source, CellLattice grid, MipPolicy policy, Option<TexturePlane> companion, Op key) =>
        TexturePlane.Of(source.Format, grid, source.Layers, source.Transfer, source.Alpha, source.Range,
                source.Primaries, key, AllocationMode.Default)
            .Map(level => Resample(source, level, policy, companion));

    private static TexturePlane Resample(TexturePlane source, TexturePlane level, MipPolicy policy, Option<TexturePlane> companion) {
        int lanes = source.Lanes, sw = source.Width.Value, dw = level.Width.Value;
        int alphaLane = source.Alpha.AlphaLane(lanes);
        bool renormalize = policy.Traits.Admits(PlaneTrait.Renormalize);
        using SpanOwner<float> src = SpanOwner<float>.Allocate(sw * source.Height.Value * lanes);
        using SpanOwner<float> dst = SpanOwner<float>.Allocate(dw * level.Height.Value * lanes);
        using SpanOwner<double> row = SpanOwner<double>.Allocate(Math.Max(sw, dw) * lanes);
        using SpanOwner<double> pair = SpanOwner<double>.Allocate(dw * PlaneFormat.MaxComponents);
        for (int layer = 0; layer < source.Layers.Value; layer++) {
            for (int y = 0; y < source.Height.Value; y++) {
                source.Read(y, layer, row.Span);
                TensorPrimitives.ConvertTruncating(row.Span[..(sw * lanes)], src.Span.Slice(y * sw * lanes, sw * lanes));
            }
            switch (policy.Filter) {
                case { } filter:
                    ImageProcessing.Resize(src.Span, sw, source.Height.Value, dst.Span, dw, level.Height.Value, lanes,
                        filter, EdgeMode.Clamp, alphaLane);
                    break;
                case null:
                    KaiserHalve(src.Span, sw, source.Height.Value, dst.Span, dw, level.Height.Value, lanes, alphaLane);
                    break;
            }
            for (int y = 0; y < level.Height.Value; y++) {
                TensorPrimitives.ConvertChecked(dst.Span.Slice(y * dw * lanes, dw * lanes), row.Span[..(dw * lanes)]);
                if (renormalize) { Renormalize(row.Span, dw, lanes); }
                companion.Iter(normal => {
                    normal.Read(y, layer, pair.Span[..(dw * normal.Lanes)]);
                    Couple(row.Span, pair.Span, dw, lanes, normal.Lanes);
                });
                level.Write(y, layer, row.Span);
            }
        }
        return level;
    }

    private static readonly double[] KaiserTaps = KaiserTable();

    private static double[] KaiserTable() {
        const double beta = 4.0, support = 6.0;
        double[] taps = new double[12];
        double total = 0.0;
        for (int tap = 0; tap < taps.Length; tap++) {
            double d = tap - 5.5;
            double sinc = Math.Sin(Math.PI * d / 2.0) / (Math.PI * d / 2.0);
            double t = d / support;
            taps[tap] = sinc * (SpecialFunctions.BesselI0(beta * Math.Sqrt(Math.Max(0.0, 1.0 - (t * t))))
                              / SpecialFunctions.BesselI0(beta));
            total += taps[tap];
        }
        TensorPrimitives.Divide(taps, total, taps);
        return taps;
    }

    private static void KaiserHalve(ReadOnlySpan<float> src, int sw, int sh, Span<float> dst, int dw, int dh, int lanes, int alphaLane) {
        using SpanOwner<float> mid = SpanOwner<float>.Allocate(dw * sh * lanes);
        AxisHalve(src, sw, sh, mid.Span, dw, lanes, alphaLane, horizontal: true);
        AxisHalve(mid.Span, dw, sh, dst, dh, lanes, alphaLane, horizontal: false);
    }

    private static void AxisHalve(ReadOnlySpan<float> input, int width, int height, Span<float> output, int outExtent, int lanes, int alphaLane, bool horizontal) {
        int outWidth = horizontal ? outExtent : width;
        int outHeight = horizontal ? height : outExtent;
        int extent = horizontal ? width : height;
        for (int y = 0; y < outHeight; y++) {
            for (int x = 0; x < outWidth; x++) {
                int folded = horizontal ? x : y;
                for (int lane = 0; lane < lanes; lane++) {
                    double sum = 0.0, coverageSum = 0.0;
                    for (int tap = 0; tap < KaiserTaps.Length; tap++) {
                        int at = Math.Clamp((2 * folded) + tap - 5, 0, extent - 1);
                        int sx = horizontal ? at : x, sy = horizontal ? y : at;
                        double coverage = alphaLane >= 0 ? input[(((sy * width) + sx) * lanes) + alphaLane] : 1.0;
                        double value = input[(((sy * width) + sx) * lanes) + lane];
                        sum += KaiserTaps[tap] * (lane == alphaLane ? value : value * coverage);
                        coverageSum += KaiserTaps[tap] * coverage;
                    }
                    output[(((y * outWidth) + x) * lanes) + lane] =
                        lane == alphaLane || alphaLane < 0 ? (float)sum
                        : (float)(coverageSum > 0.0 ? sum / coverageSum : 0.0);
                }
            }
        }
    }

    private static void Renormalize(Span<double> row, int width, int lanes) {
        int axes = Math.Min(3, lanes);
        for (int x = 0; x < width; x++) {
            Span<double> texel = row.Slice(x * lanes, lanes);
            double square = 0.0;
            for (int axis = 0; axis < axes; axis++) { square += texel[axis] * texel[axis]; }
            double length = Math.Sqrt(axes < 3 ? square + Math.Max(0.0, 1.0 - square) : square);
            if (length > 0.0) { for (int axis = 0; axis < axes; axis++) { texel[axis] /= length; } }
        }
    }

    private static void Couple(Span<double> row, ReadOnlySpan<double> normal, int width, int lanes, int normalLanes) {
        int axes = Math.Min(3, normalLanes);
        for (int x = 0; x < width; x++) {
            ReadOnlySpan<double> n = normal.Slice(x * normalLanes, normalLanes);
            double square = 0.0;
            for (int axis = 0; axis < axes; axis++) { square += n[axis] * n[axis]; }
            double length = Math.Sqrt(axes < 3 ? square + Math.Max(0.0, 1.0 - square) : square);
            double variance = length > 0.0 ? (1.0 - length) / length : 0.0;
            double roughness = row[x * lanes];
            row[x * lanes] = Math.Min(1.0, Math.Sqrt((roughness * roughness) + (2.0 * variance)));
        }
    }

    public Fin<TextureSource.Image> AsImage(Op key) =>
        Base.Layers.Value is not 1
            ? new MaterialFault.Parameter(key, $"<pyramid-layered:{Base.Layers.Value}>")
            : TextureSource.Image.Of(Base.Width, Base.Height, Levels.Map(static level => Materialize(level)), key);

    private static ReadOnlyMemory<ShadeVec4> Materialize(TexturePlane level) {
        ShadeVec4[] texels = new ShadeVec4[level.Width.Value * level.Height.Value];
        for (int row = 0; row < level.Height.Value; row++) {
            level.ReadShade(row, layer: 0, texels.AsSpan(row * level.Width.Value, level.Width.Value));
        }
        return texels;
    }

    public void Dispose() => Levels.Iter(static level => level.Dispose());
}
```

## [06]-[PLANE_RESIDENCY]

- Owner: `ResidencyPolicy` the eviction axis carrying its rank projection; `ResidentTile` the seated chain with its cost and its two eviction ordinals; `PlaneResidency` the tile-indexed window over a DECLARED tile grid.
- Entry: `PlaneResidency.Of(CellLattice tiles, ResidencyPolicy policy, long texelBudget, Op key)` admits the window; `Resolve(int index, Func<int, Fin<TexturePyramid>> mint, Op key)` is the ONE read — it answers a resident chain or mints the absent one through the caller's own thunk under the budget; `Declared` is the whole tile grid, `Resident` the seated census, `Seated` the resident index set.
- Law: RESIDENCY IS NOT IDENTITY. The window addresses the DECLARED tile grid whole and holds a subset of it, so a window seating two tiles and a window seating a hundred describe the same asset; nothing here mints a key, `TexturePyramid.Key` stays a per-chain fact, and `set#TEXTURE_SET` keys over the full declared grid with residency never entering a preimage. Two views of one asset therefore address one blob, which is the only arrangement under which a partial load is a read policy rather than a second asset.
- Law: the WINDOW IS THE MINT'S CALLER, never its author. `Resolve` takes the per-tile mint as a thunk over the tile index, so the decode path stays where it belongs — a container decode at `codec#RASTER_CODEC`, an ingest lift at `set#SET_INGEST` — and this owner contributes exactly the residency algebra. A tile index outside the declared grid refuses before the thunk runs, so a mint never sees an index the grid does not carry.
- Law: the BUDGET IS TEXELS, matching the arena's own admission currency, so a 16k tile and a 512 tile cost what they are rather than counting equal. `Reclaim` picks the eviction SET in ONE ranked pass and releases it before the new chain seats, so peak residency is the budget rather than the budget plus one chain. A chain whose own census exceeds the whole budget refuses instead of evicting the window to fail anyway.
- Law: `ResidencyPolicy.Retain` carries a NULL rank and therefore evicts nothing — an over-budget admission refuses rather than dropping a tile a caller may still be reading. That refusal is the whole-grid modality stated as a policy row instead of as a second window type, so `whole-grid` and `per-tile` are one shape and the set's own residency column selects between them by naming a row.
- Law: EVICTION DISPOSES. The window owns every chain it seated, so `Resolve` answers the chain per call rather than handing out a cached handle a later eviction would dangle; a caller holding a chain across two `Resolve` calls on a bounded window holds a plane the window may have freed, and re-resolving is the contract.
- Packages: `plane#TEXTURE_PLANE` (composed — `TexturePyramid`/`Texels`/`Dispose`), `Rasm.Numerics` (composed — `CellLattice.CellCount`/`Linear` the ONE tile-grid addressing), `Rasm.Domain` (`Op`), `bsdf#SHADING_FRAME` (`MaterialFault` band 2450), LanguageExt.Core (`HashMap`, `Seq`, `Fin`), Thinktecture.Runtime.Extensions.
- Growth: a new eviction law is one `ResidencyPolicy` row projecting its own rank; a cost model other than texels is one column on `ResidentTile` and one read in `Reclaim`. Neither the window, the resolve path, the mint interface, nor any consumer changes.
- Boundary: this window bounds CHAINS, never arena bands — `TexturePlane.Layer` windows one rental into layer bands inside a single plane, while `PlaneResidency` holds independent pyramids addressed by a tile coordinate, so a cube-face set and a UDIM grid never share a mechanism. The window carries no decode, no format, and no channel: every tile of one asset resolves through the caller's own mint, so a residency window over base-colour tiles and one over normal tiles are two windows and neither knows the other exists.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Linq;
using LanguageExt;
using Rasm.Domain;
using Rasm.Materials.Appearance.Bsdf;
using Rasm.Numerics;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [POLICIES] ------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResidencyPolicy {
    public static readonly ResidencyPolicy Retain = new("retain", rank: null);
    public static readonly ResidencyPolicy Recent = new("recent", static tile => tile.Touched);
    public static readonly ResidencyPolicy Rare = new("rare", static tile => tile.Hits);

    public Func<ResidentTile, long>? Rank { get; }
    public bool Evicts => Rank is not null;
    private ResidencyPolicy(string key, Func<ResidentTile, long>? rank) : this(key) => Rank = rank;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ResidentTile(int Index, TexturePyramid Chain, long Cost, long Touched, long Hits);

public sealed class PlaneResidency : IDisposable {
    private readonly CellLattice declared;
    private readonly ResidencyPolicy policy;
    private readonly long budget;
    private HashMap<int, ResidentTile> seated = HashMap<int, ResidentTile>.Empty;
    private long resident;
    private long clock;

    private PlaneResidency(CellLattice tiles, ResidencyPolicy row, long texelBudget) =>
        (declared, policy, budget) = (tiles, row, texelBudget);

    public static Fin<PlaneResidency> Of(CellLattice tiles, ResidencyPolicy policy, long texelBudget, Op key) =>
        texelBudget > 0
            ? Fin.Succ(new PlaneResidency(tiles, policy, texelBudget))
            : new MaterialFault.Parameter(key, $"<residency-budget:{texelBudget}>");

    public CellLattice Declared => declared;
    public long Resident => resident;
    public Seq<int> Seated => toSeq(seated.Keys);

    public Fin<TexturePyramid> Resolve(int index, Func<int, Fin<TexturePyramid>> mint, Op key) =>
        seated.Find(index).Match(
            Some: tile => Fin.Succ(Touch(tile)),
            None: () => index >= 0 && index < declared.CellCount
                ? mint(index).Bind(chain => Seat(index, chain, key))
                : new MaterialFault.Parameter(key, $"<residency-tile:{index}:{declared.CellCount}>"));

    private TexturePyramid Touch(ResidentTile tile) {
        seated = seated.AddOrUpdate(tile.Index, tile with { Touched = ++clock, Hits = tile.Hits + 1 });
        return tile.Chain;
    }

    private Fin<TexturePyramid> Seat(int index, TexturePyramid chain, Op key) {
        long cost = chain.Texels;
        if (cost > budget)
            return Fin.Fail<TexturePyramid>(new MaterialFault.Parameter(key, $"<residency-over-budget:{cost}:{resident}:{budget}>"))
                .Rollback(chain);

        Seq<ResidentTile> victims = Reclaim(budget - cost);
        return Custody.Release(victims, tile => Retire(tile, key), key)
            .Bind(_ => resident + cost > budget
                ? Fin.Fail<TexturePyramid>(new MaterialFault.Parameter(key, $"<residency-over-budget:{cost}:{resident}:{budget}>"))
                : SeatOwned(index, chain, cost))
            .Rollback(chain);
    }

    private Fin<TexturePyramid> SeatOwned(int index, TexturePyramid chain, long cost) {
        seated = seated.Add(index, new ResidentTile(index, chain, cost, ++clock, Hits: 1));
        resident += cost;
        return Fin.Succ(chain);
    }

    private Seq<ResidentTile> Reclaim(long headroom) =>
        policy.Rank is { } rank
            ? toSeq(seated.Values.OrderBy(rank))
                  .Fold((Freed: 0L, Victims: Seq<ResidentTile>.Empty), (state, tile) =>
                      resident - state.Freed <= headroom ? state : (state.Freed + tile.Cost, state.Victims.Add(tile)))
                  .Victims
            : Seq<ResidentTile>.Empty;

    private Fin<Unit> Retire(ResidentTile tile, Op key) {
        seated = seated.Remove(tile.Index);
        resident -= tile.Cost;
        return key.Catch(() => { tile.Chain.Dispose(); return Fin.Succ(unit); });
    }

    public void Dispose() {
        seated.Values.Iter(static tile => tile.Chain.Dispose());
        (seated, resident) = (HashMap<int, ResidentTile>.Empty, 0L);
    }
}
```

## [07]-[RESEARCH]

(none)
