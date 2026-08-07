# [MATERIALS_PLANE]

THE DECODED-RASTER SUBSTRATE. One `TexturePlane` owns every pixel grid the texture estate holds — the storage row, the kernel `CellLattice` that seats its extent and its spatial grain, the layer stack, the encoded transfer, the colour primaries, the alpha association, and the value range — over a TYPED-TEXEL pooled arena that never becomes a byte arena; one `TexturePyramid` owns the level chain a `MipPolicy` row folds over the lattice's own `Coarsen` step, including the variance coupling a roughness chain takes from its paired normal chain; one `AsImage` bridge lifts a chain into the existing `texture#TEXTURE_UV` `TextureSource.Image` sampler so the estate mints no second sampler; and one `PlaneResidency` window bounds how much of a declared tile grid is resident at once, so an asset whose whole grid exceeds the arena resolves the tiles a view reads and evicts the rest under a policy row. `PlaneFormat` rows own storage over the kernel `ChannelDtype` roster, `PlaneTransfer` rows own transfer, `PlanePrimaries` rows own chromaticity, and `MipPolicy` rows own the level fold — never a per-format plane type, a per-depth converter pair, a second depth vocabulary, or a per-policy pyramid class.

Typed texels size the arena because bytes cannot: `byte[]` caps at `Array.MaxLength`, so a 16k×16k four-lane 16-bit plane spans 2.147 GB of bytes and refuses at the runtime bound, while the same plane counts 268 435 456 TEXELS and rents cleanly — the element count is the only budget that admits the extents this estate bakes. Storage therefore comes from ONE open generic `PlaneStore<T>` over a `MemoryOwner<T>`/`Memory2D<T>` pair, the texel structs are three arities applied to four component witnesses rather than twelve hand-written records, and typed code re-enters through a `struct`-or-`ref struct` fold seam that the JIT specializes per texel with no boxing, no closure, and no per-row delegate. Every consumer above this page reads and writes DECODED lanes through the one `Read`/`Write` row rail, so the encode ladder — integer normalization, signed `(v+1)/2` packing, transfer decode, alpha association — is stated exactly once in the corpus and no kernel re-derives a curve. `Rasm.Materials.Raster` composes `CommunityToolkit.HighPerformance` for the pooled arena and its plane views, `TinyEXR.NET` `ImageProcessing` for every transfer fold, every colour-matrix rebase, and every delegated resample row — the `kaiser` fold alone is the Materials-owned windowed-sinc kernel, because neither the composed resampler nor the kernel weight roster ships one — MathNet.Numerics for the Bessel evaluation that fold's window needs, the kernel `CellLattice`/`Dimension`/`ChannelDtype`/`Op`/`ContentHash` atoms, the `bsdf#SHADING_FRAME` `MaterialFault` band-2450 rail for every shape refusal, and the `texture#TEXTURE_UV` `TextureSource.Image`/`ShadeVec4` sampler at the one lift — re-minting no allocator, no lattice, no storage-type roster, no transfer curve, no special function, and no hash.

## [01]-[INDEX]

- [02]-[PLANE_VOCABULARY]: `PlaneTransfer`/`PlanePrimaries`/`AlphaMode`/`PlaneRange`/`NormalConvention`/`MipPolicy` axes each carry conversion behaviour as row data.
- [03]-[PLANE_FORMAT]: `IComponent`/`ITexel` static-abstract witnesses type the three-arity texel family, and the twelve-row `PlaneFormat` storage roster resolves a semantic count over the kernel `ChannelDtype` depths.
- [04]-[TEXTURE_PLANE]: `PlaneStore<T>` generalizes the arena behind its `IPlaneFold` seam, and `TexturePlane` owns admission over a `CellLattice`, the layer window, the decoded `Read`/`Write` row rails, the association and primaries conversions, and the streaming content key.
- [05]-[TEXTURE_PYRAMID]: `MipPolicy` drives the level fold over the lattice's `Coarsen` step and the paired variance coupling, and `AsImage` bridges the chain to the sampler.
- [06]-[PLANE_RESIDENCY]: `PlaneResidency` windows a declared tile grid to a texel budget under a `ResidencyPolicy` rank, resolving one tile's chain through the caller's own mint.

## [02]-[PLANE_VOCABULARY]

- Owner: `PlaneTransfer` the encoded-transfer axis carrying its `TinyEXR.V3.TransferFunction` binding and the quantity it declares; `PlanePrimaries` the chromaticity axis carrying its `ColorSpace` reconciliation row, the kernel `RgbProfile` row whose published geometry it matches a declaration against, and its container-assignment token; `AlphaMode` the association axis; `PlaneRange` the stored-value-range axis; `NormalConvention` the green-polarity axis; `MipPolicy` the level-fold axis carrying its separable filter and its two post-fold flags.
- Cases: transfer {`linear`, `srgb`, `raw`, `pq`, `hlg`} · primaries {`acesAp1`, `bt709`, `bt2020`, `acesAp0`, `p3d65`, `xyz`, `unknown`} · alpha {`straight`, `associated`, `none`} · range {`unit`, `signed`} · convention {`gl`, `dx`} · mip {`box`, `kaiser`, `normalRenormalize`, `roughnessVariance`, `none`}.
- Law: the storage-component axis is the KERNEL `Drawing/pack#ENCODING_CHANNEL` `ChannelDtype` roster and this page mints none — `Unorm8`, `Unorm16`, `Float16`, and `Float32` are a strict subset of its rows, so a depth vocabulary here would be a second storage-type owner the `Rasm.Element` raster sample vocabulary and this arena would then have to reconcile. What the kernel roster does NOT carry is a normalization column, because a byte arena reads its own pack arm; the typed arena's `IComponent<T>` witness answers that instead, so `[03]` states the correspondence once as `PlaneFormat.Normalizes` and `PlaneRange`, `AlphaMode`, and `codec#RASTER_FORMAT` all read that one member.
- Law: `PlaneQuantity` splits the transfer rows by WHAT the stored number is — `light` for a scene-linear radiometric value, `parameter` for a shading input no colour transform may touch, `display` for a display-referred encoding. `PlaneQuantity` keeps `raw` and `linear` two rows rather than one alias: both decode by identity, but a colour transform legally reaches a `light` plane and never a `parameter` plane. `SceneReferred` is the SEPARATE bake-legality column the `set#TEXTURE_SET` admission gate reads: `srgb` is display-referred as an ENCODING yet scene-referred as a BAKE TARGET because `Read` decodes it to scene-linear, so `linear`, `srgb`, and `raw` carry `true` while `pq` and `hlg` — legal on an environment plane alone — carry `false` and refuse at `TextureSet.Of` for every channel plane.
- Law: the `pq` and `hlg` rows encode a display-referred TRANSFER and assert NO reference white. `surface#TONE_MAP` `DisplayEncoding` owns the colorimetric egress — primary rebase, transfer, and the 203-nit HDR reference white its `DynamicRange` column declares — so a plane carrying `pq` without a `DisplayEncoding` provenance is unanchored: its code values are legible and their absolute luminance is not. Each `DisplayEncoding` row names its storage transfer on this vocabulary, so an encode reads the pair off the colorimetric row rather than pairing them by hand.
- Law: `PlanePrimaries` is a SEPARATE axis from transfer: one linear plane may be AP1, Rec.709, or AP0, and the KTX colour-assignment pair labels each of the three differently, so a transfer-derived primaries label states a chromaticity the file never declared. Chromaticity GEOMETRY is the kernel `RgbProfile` row's published column and never a per-row coordinate table, so a working-space correction moves every label at once; DECLARATION and RECONCILIATION are independent columns — the `p3d65` row states a real code point the resampler carries no endpoint for, so it labels a P3 file honestly and refuses its rebase, where dropping the row lost the declaration outright. `unknown` is the honest DEFAULT — a decode that read no chromaticity attribute and no CICP block declares nothing rather than the working space — and it is what makes `--fail-on-color-conversions` a real gate instead of a rubber stamp over a fabricated label. `Matrix` resolves the composed reconciliation ONCE per conversion and refuses an unknown endpoint there, so a rebase across an undeclared gamut is unrepresentable rather than silently identity and the plane walk consuming the matrix carries no rail, no per-row roster lookup, and no failure arm to orphan a rental.
- Law: `PlaneRange` is the SIGNED-ENCODE owner and the only site in the corpus that spells `(v + 1) / 2` or `2v − 1`. `PlaneRange.Signed` packs its `[-1,1]` value into the storage `[0,1]` span at a normalizing depth and unpacks on read, and stores the signed value verbatim at float depth, so a normal, a tangent, or a curvature plane carries one declaration and every kernel above reads the signed value whatever the depth beneath it.
- Law: `NormalConvention` homes HERE because green polarity is a property of the stored plane exactly as association and transfer are, and `ToGl` lives on the row because the flip is a green-sign multiply over a DECODED texel with `set#SET_INGEST` its one caller. `gl` is the canonical `+Y` wire form; `dx` is admitted at ingest and converted once through that member or through the equivalent `filter#PLANE_OP` `Swizzle` lane inversion before the plane is keyed, so no plane leaves the estate carrying `−Y` green and the silent lighting inversion is unrepresentable.
- Law: `MipPolicy.Kaiser` is the Materials-owned SEPARABLE WINDOWED-SINC halving, and NO composed roster carries one — the claim is over all THREE. The resampler ships `Box`, `Triangle`, `CatmullRom`, `Mitchell`; the kernel `Numerics/calculus#WEIGHT_PROFILES` `WeightKernelFamily` ships `SmoothPoly`, `WendlandC2`, `Gaussian`, `CompactExp`, `Singular`, and the sinc-windowed `Lanczos`; the kernel `Numerics/matrix#TRANSFORM_BAND` `WindowTaper` roster spans the whole shipped taper space — the cosine-sum, triangular, and Dirichlet fixed rows plus the parameterized `Gauss` and `Tukey` — and stops there because its source publishes no Kaiser member. A Kaiser-windowed sinc is a distinct window whose `β` shapes the stopband, so the frozen `kaiser` fold is authored as the `[05]` polyphase kernel over its own Bessel evaluation rather than aliased onto any roster's nearest row, and the fold retires only if a taper owner grows a real Kaiser row rather than a near neighbour. `Box` is the arithmetic 2×2 mean, `NormalRenormalize` folds box and then unit-normalizes each texel vector, `RoughnessVariance` folds box and then absorbs the directional variance its paired normal chain lost at the same level, and `None` declares a single-level plane. Every fold runs in the LINEAR domain — a plane decodes, folds, and re-encodes per level, because averaging `srgb`-encoded texels darkens the pyramid.
- Boundary: rows carry CONVERSION, never storage — a transfer row knows its `TransferFunction` binding, a primaries row knows its `ColorSpace`, its composed geometry, and its assignment token, a range row knows its packing, and a mip row knows its filter and its two post-folds. `[03]` owns the typed arena consuming them, so a new depth lands as one `IComponent` witness and one `PlaneFormat` row and reaches the whole page without touching an arena, a rail, or a codec.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using System.Linq;
using CommunityToolkit.HighPerformance.Buffers;   // AllocationMode — the row each PlaneFormat Rent column binds
using LanguageExt;                                // Fin, Option, Seq, Unit
using Rasm.Domain;                                // Op, ContentHash
using Rasm.Drawing;                               // ChannelDtype — the kernel storage-type roster this page seats onto
using Rasm.Materials.Appearance.Bsdf;             // MaterialFault — the band-2450 shape rail
using Rasm.Materials.Appearance.Texture;          // ShadeVec4 — the decoded texel the green flip rewrites
using Rasm.Numerics;                              // Dimension, RgbProfile — the kernel working-space roster each primaries row reads its published geometry off
using Wacton.Unicolour;                           // Chromaticity — the published chromaticity coordinate a primaries row matches a container attribute against
using Thinktecture;                               // [SmartEnum<T>], [KeyMemberEqualityComparer]
using TinyEXR.V3;                                 // ImageProcessing, TransferFunction, ColorSpace, Chromaticities, ResizeFilter
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [TYPES] -------------------------------------------------------------------------------
// What the stored number IS. The axis a transfer row projects onto, and the reason `raw` and `linear` are two rows:
// both decode by identity, but a colour transform reaches `light` and never `parameter`, and `display` is the pair
// no scene-referred bake may carry.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlaneQuantity {
    public static readonly PlaneQuantity Light = new("light");
    public static readonly PlaneQuantity Parameter = new("parameter");
    public static readonly PlaneQuantity Display = new("display");
}

// PlaneTransfer rows carry the encoded transfer. Function is the composed fold's own row and NULL means identity — the
// one place `raw` and `linear` differ numerically is nowhere, so the difference lives on Quantity instead of on a second curve.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlaneTransfer {
    public static readonly PlaneTransfer Linear = new("linear", TransferFunction.Linear, PlaneQuantity.Light, sceneReferred: true);
    public static readonly PlaneTransfer Srgb = new("srgb", TransferFunction.Srgb, PlaneQuantity.Display, sceneReferred: true);
    public static readonly PlaneTransfer Raw = new("raw", function: null, PlaneQuantity.Parameter, sceneReferred: true);
    // Pq and Hlg encode a display-referred transfer and assert NO reference white: the 203-nit HDR anchor is
    // surface#TONE_MAP DisplayEncoding's DynamicRange column, so a plane carrying pq with no DisplayEncoding
    // provenance carries legible code values whose absolute luminance nothing states.
    public static readonly PlaneTransfer Pq = new("pq", TransferFunction.Pq, PlaneQuantity.Display, sceneReferred: false);
    public static readonly PlaneTransfer Hlg = new("hlg", TransferFunction.Hlg, PlaneQuantity.Display, sceneReferred: false);

    public TransferFunction? Function { get; }
    public PlaneQuantity Quantity { get; }
    // Bake legality is the column set#TEXTURE_SET reads: srgb decodes to scene-linear at Read, so it bakes; pq/hlg are
    // environment-only display transfers that refuse on every channel plane.
    public bool SceneReferred { get; }
    public bool Identity => Function is null or TransferFunction.Linear;
    private PlaneTransfer(string key, TransferFunction? function, PlaneQuantity quantity, bool sceneReferred) : this(key) =>
        (Function, Quantity, SceneReferred) = (function, quantity, sceneReferred);

    // Decode reads an ENCODED lane run and writes scene-linear; Encode is its inverse. Both delegate to the composed
    // span fold, which admits a destination aliasing its source at the same start — so one scratch run threads a row. The
    // span-shaped dispatch is the section's [EXPRESSION_SPINE] kernel exemption: a switch expression cannot carry
    // void arms and a lambda cannot capture a span, so the identity short-circuit is a statement.
    public void Decode(ReadOnlySpan<float> source, Span<float> destination) {
        if (Identity) { source.CopyTo(destination); return; }
        ImageProcessing.DecodeTransfer(source, destination, Function!.Value);
    }

    public void Encode(ReadOnlySpan<float> source, Span<float> destination) {
        if (Identity) { source.CopyTo(destination); return; }
        ImageProcessing.EncodeTransfer(source, destination, Function!.Value);
    }
}

// PlanePrimaries rows carry the chromaticity a plane's colour lanes are stated against — a SEPARATE axis from
// transfer, because a linear plane may be AP1, Rec.709, or AP0 and a container labels each differently. Geometry is
// the kernel RgbProfile row's own published chromaticity column, so this axis carries CONTAINER LABELS alone and the
// eight-coordinate table it once transcribed per row — a second copy of numbers the working-space owner already
// publishes — is deleted; Space is the composed reconciliation row Matrix resolves through and NULL means the resampler
// carries no endpoint for that gamut; Cicp is the ITU-T H.273 code point where the standard names one, absent on the
// two ACES rows the standard does not; Assign is the container token the KTX colour-assignment pair spells, so no
// page derives a chromaticity label from a transfer.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlanePrimaries {
    public static readonly PlanePrimaries AcesAp1 = new("acesAp1", ColorSpace.AcesAp1, cicp: null, assign: "acescc", profile: RgbProfile.Acescg);
    public static readonly PlanePrimaries Bt709 = new("bt709", ColorSpace.Srgb, cicp: 1, assign: "bt709", profile: RgbProfile.Srgb);
    public static readonly PlanePrimaries Bt2020 = new("bt2020", ColorSpace.Rec2020, cicp: 9, assign: "bt2020", profile: RgbProfile.Rec2020);
    public static readonly PlanePrimaries AcesAp0 = new("acesAp0", ColorSpace.AcesAp0, cicp: null, assign: "aces", profile: RgbProfile.Aces20651);
    // P3-D65 DECLARES and never converts: the resampler's own ColorSpace roster carries no P3 endpoint, so the row
    // holds the code point (SMPTE ST 432-1) and the container token while Matrix refuses it. Dropping the row was
    // strictly worse — a PNG or WebP whose CICP block states P3 resolved to `unknown` and the file's own declaration
    // was lost, which is the fabrication in the other direction.
    public static readonly PlanePrimaries P3D65 = new("p3d65", space: null, cicp: 12, assign: "displayp3", profile: RgbProfile.DisplayP3);
    // CIE 1931 XYZ (SMPTE ST 428-1) is the one row whose geometry is DEFINITIONAL rather than measured — the identity
    // primaries and the equal-energy white are the space's definition, not a published measurement a working-space
    // preset could carry — and it is a full rebase endpoint the resampler does carry, so an EXR authored in XYZ
    // reconciles instead of refusing.
    public static readonly PlanePrimaries Xyz = new("xyz", ColorSpace.Xyz, cicp: 10, assign: "ciexyz", geometry:
        (new Chromaticity(1.0, 0.0), new Chromaticity(0.0, 1.0), new Chromaticity(0.0, 0.0), new Chromaticity(1.0 / 3.0, 1.0 / 3.0)));
    // Absence takes a row of its own: a decode that read no chromaticity attribute and no CICP block declares nothing, which is
    // what makes the container's own --fail-on-color-conversions gate real rather than a stamp over a guess.
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

    // Declared chromaticity resolves to the row it matches within one authoring tolerance, else to absence — a
    // near-miss is a file stating primaries this estate does not carry, and naming the nearest row would fabricate
    // exactly the label the axis exists to stop. Absence of GEOMETRY is what disqualifies a candidate, never absence
    // of a rebase endpoint: a P3 header states real coordinates the roster must still recognize.
    private const double CoordinateTolerance = 1e-3;

    // Both resolutions read a FROZEN index rather than walking the roster: `Of` runs per decoded container and a
    // per-call roster scan re-derives a constant. The CICP arm is an exact key and freezes as a lookup; the
    // chromaticity arm is a tolerance match no hash answers, so it freezes as the geometry-BEARING subset alone and
    // the rows carrying no coordinates — the two ACES-adjacent declarations and absence itself — never enter the walk.
    // The lazy seat defers past the generated roster's own initialization, so no row reads Items while Items is building.
    private static readonly Lazy<FrozenDictionary<int, PlanePrimaries>> ByCicp =
        new(static () => Items.Where(static row => row.Cicp is not null).ToFrozenDictionary(static row => row.Cicp!.Value));

    private static readonly Lazy<Seq<PlanePrimaries>> Declaring =
        new(static () => toSeq(Items).Filter(static row => row.Geometry.IsSome));

    public static PlanePrimaries Of(Option<Chromaticities> declared) =>
        declared.Bind(row => Declaring.Value.Find(candidate => candidate.Matches(row))).IfNone(Unknown);

    public static PlanePrimaries Of(int cicp) =>
        ByCicp.Value.TryGetValue(cicp, out PlanePrimaries? row) ? row : Unknown;

    // Chromaticities carries eight f32 coordinates, so each read widens to double before the comparison and the
    // f32 quantization of a published coordinate sits three orders inside the authoring tolerance.
    public bool Matches(Chromaticities declared) =>
        Geometry.Map(g =>
            Near(g.Red, declared.RedX, declared.RedY) && Near(g.Green, declared.GreenX, declared.GreenY)
            && Near(g.Blue, declared.BlueX, declared.BlueY) && Near(g.White, declared.WhiteX, declared.WhiteY)).IfNone(false);

    private static bool Near(Chromaticity row, double x, double y) =>
        Math.Abs(row.X - x) <= CoordinateTolerance && Math.Abs(row.Y - y) <= CoordinateTolerance;

    // Matrix RESOLVES the gamut reconciliation ONCE per conversion — the package composes the XYZ pair with a
    // Bradford-class adaptation where the white points differ, so no 3x3 is hand-typed here. An unknown endpoint
    // REFUSES: a rebase across an undeclared gamut is a fabricated transform wearing a conversion. Resolution is the
    // only fallible step, so the plane walk that consumes the matrix is TOTAL — a per-row rail whose refusal cannot
    // change between rows is a leak arm wearing a guard, and lifting it here deletes both the arm and the per-row
    // roster lookup it hid.
    public Fin<ColorMatrix3x3> Matrix(PlanePrimaries target, Op key) =>
        (Space, target.Space) switch {
            ({ } from, { } to) => Fin.Succ(ImageProcessing.GetColorMatrix(from, to)),
            _ => MaterialFault.Parameter(key, $"<plane-primaries-unknown:{Key}->{target.Key}>"),
        };
}

// AlphaMode rows carry the association. Carries decides whether an alpha lane exists at all; Premultiplied decides
// whether the colour lanes are scaled by it. The straight-associated crossing is lossy below 16 bits, so [04] gates it on depth.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AlphaMode {
    public static readonly AlphaMode Straight = new("straight", carries: true, premultiplied: false);
    public static readonly AlphaMode Associated = new("associated", carries: true, premultiplied: true);
    public static readonly AlphaMode None = new("none", carries: false, premultiplied: false);

    public bool Carries { get; }
    public bool Premultiplied { get; }
    private AlphaMode(string key, bool carries, bool premultiplied) : this(key) =>
        (Carries, Premultiplied) = (carries, premultiplied);

    // Convertible is the ONE crossing predicate, keyed on COVERAGE MATH rather than lane cardinality: identity always converts; a
    // crossing that runs no coverage multiply — straight to none, none to anything (coverage is definitionally one) —
    // converts at every depth; and a crossing that multiplies or divides by a stored coverage admits only above 8
    // bits, because the quantized division amplifies its own step into colour error the inverse cannot recover. The
    // [04] ToAlpha gate and the set#TEXTURE_SET admission both read THIS row fact, so the 16-bit floor has one
    // spelling — and the Radiance egress lives for a straight chain, whose drop to none is arithmetic-free.
    public bool Convertible(AlphaMode target, ChannelDtype depth) =>
        target == this
        || (!Premultiplied && !target.Premultiplied)
        || !Carries
        || depth != ChannelDtype.Unorm8;
}

// PlaneRange rows carry the stored value range and the corpus's ONLY spelling of the signed integer packing. A float
// plane stores the signed value verbatim, so both members are identity there and no kernel above ever branches on depth.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlaneRange {
    public static readonly PlaneRange Unit = new("unit", signed: false);
    public static readonly PlaneRange Signed = new("signed", signed: true);

    public bool IsSigned { get; }
    private PlaneRange(string key, bool signed) : this(key) => IsSigned = signed;

    public double Unpack(double stored, ChannelDtype depth) =>
        IsSigned && PlaneFormat.Normalizes(depth) ? (2.0 * stored) - 1.0 : stored;
    public double Pack(double value, ChannelDtype depth) =>
        IsSigned && PlaneFormat.Normalizes(depth) ? (value + 1.0) * 0.5 : value;
}

// Green polarity. The wire is always `gl`; `dx` is an INGEST record converted once through ToGl — or through the
// equivalent filter#PLANE_OP Swizzle lane inversion — before the plane is keyed.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NormalConvention {
    public static readonly NormalConvention Gl = new("gl", greenSign: 1.0);
    public static readonly NormalConvention Dx = new("dx", greenSign: -1.0);

    public double GreenSign { get; }
    private NormalConvention(string key, double greenSign) : this(key) => GreenSign = greenSign;

    // ToGl flips a DECODED texel — a green-sign multiply, never a depth-branching pair, because the integer
    // 1-g spelling is PlaneRange's and set#SET_INGEST is this member's one caller.
    public ShadeVec4 ToGl(ShadeVec4 decoded) => decoded with { Y = GreenSign * decoded.Y };
}

// MipPolicy rows carry the level fold. Filter is the composed separable downsample a row delegates to, and NULL is the
// kaiser row's own Materials-owned windowed-sinc kernel — none of the three composed rosters (the resampler filters,
// the kernel weight profiles, the kernel WindowTaper band) carries a Kaiser window, so the frozen fold is authored
// at [05] rather than aliased onto a nearer row. Renormalize
// and Coupled are the two post-folds making a vector chain and a roughness chain correct rather than merely smaller.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MipPolicy {
    public static readonly MipPolicy Box = new("box", ResizeFilter.Box, renormalize: false, coupled: false, chains: true);
    public static readonly MipPolicy Kaiser = new("kaiser", filter: null, renormalize: false, coupled: false, chains: true);
    public static readonly MipPolicy NormalRenormalize = new("normalRenormalize", ResizeFilter.Box, renormalize: true, coupled: false, chains: true);
    public static readonly MipPolicy RoughnessVariance = new("roughnessVariance", ResizeFilter.Box, renormalize: false, coupled: true, chains: true);
    public static readonly MipPolicy None = new("none", ResizeFilter.Box, renormalize: false, coupled: false, chains: false);

    public ResizeFilter? Filter { get; }
    public bool Renormalize { get; }
    public bool Coupled { get; }
    public bool Chains { get; }
    private MipPolicy(string key, ResizeFilter? filter, bool renormalize, bool coupled, bool chains) : this(key) =>
        (Filter, Renormalize, Coupled, Chains) = (filter, renormalize, coupled, chains);
}
```

## [03]-[PLANE_FORMAT]

- Owner: `IComponent<T>` the static-abstract component witness; `ITexel<TSelf>` the static-abstract texel contract; `Texel1`/`Texel2`/`Texel4` the three storage arities; `PlaneFormat` the twelve-row storage roster over the kernel `ChannelDtype` depths.
- Law: the twelve storage rows are the CROSS PRODUCT of three arities and four component witnesses, applied as type arguments. Twelve hand-written texel records are the deleted form: they share one lane-projection law, so a new depth is one `IComponent` witness and a new arity is one struct, and the roster grows by type application rather than by transcription.
- Law: `IComponent<T>.ToUnit`/`FromUnit` normalize an INTEGER component onto `[0,1]` and pass a floating component verbatim, saturating on the write side so a fold that overshoots stores the clamped value rather than a wrapped one. Witnesses carry the typed-arena TYPE APPLICATION the kernel `ChannelDtype`'s byte-shaped pack and unpack arms cannot express — a `Span<byte>` arm cannot produce a `Texel4<Half, F16>` — so the depth roster is the kernel's and the witness family is this page's, meeting at the one `Normalizes` correspondence below. This is the only normalization in the estate; a kernel dividing by `255.0` or `65535.0` re-derives it.
- Law: a three-component semantic channel resolves to the FOUR-component storage row declaring `AlphaMode.None`, and the roster is total over `{1, 2, 4}` at every depth, so `For` never rounds past the arity a channel declares. No odd-width texel exists, so a padded lane is a structural fact the association declares rather than a per-format special case.
- Law: WIRE REACH IS A STORAGE FACT, not a container one. Where no supercompression transcodes the payload, the KTX2 declares the STORE's own Vulkan format, and the browser read path resolves no such row for a one- or two-component 16-bit UNORM store — so `r16` and `rg16` are unreachable on a wire target while every other row is reachable. `PlaneFormat.WebReachable` states that once and `set#TEXTURE_SET` admission reads it, the storage-side twin of the `codec#RASTER_CODEC` `KtxPayload.WireLegal` payload gate; the two are ORTHOGONAL, because a transcodable payload leaves the declared format undefined until transcode and the store class stops mattering there.
- Entry: `PlaneFormat.For(int semanticComponents, ChannelDtype depth)` is the ONE resolution both the press binding and the neural stage read; `PlaneFormat.Normalizes(ChannelDtype)` is the ONE normalization discriminant every packing site reads; `PlaneFormat.WebReachable(int, ChannelDtype)` the ONE wire-target discriminant, surfaced per row as `WebLegal`; `Items` is the ordered roster; `Get`/`TryGet` resolve a wire key.
- Packages: CommunityToolkit.HighPerformance (`MemoryOwner<T>.Allocate(int, AllocationMode)` the pooled rental every row's `Rent` column binds, `Memory<T>.AsMemory2D(int, int)` the plane projection), `Rasm.Drawing` (composed — `ChannelDtype.Unorm8`/`Unorm16`/`Float16`/`Float32` the storage-component rows and their `Width` column), Thinktecture.Runtime.Extensions, BCL inbox (`Half`, `double.Clamp`, `Array.MaxLength`).
- Growth: a new depth is one `IComponent` witness with its rows, one `Normalizes` arm, and one `WebReachable` arm; a new arity is one texel struct with its rows; a new storage row is one `PlaneFormat` declaration naming its arity, witness, component count, depth, and alpha. Nothing else on this page, in the codec, in the filter, or in the pyramid changes — the `Rent` column carries the type application and every consumer stays generic over `ITexel`.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// IComponent<T> witnesses one storage scalar as a static-abstract pair, so normalization is declared once per depth
// and the texel arities stay generic. byte/ushort/Half/float cannot implement an interface, so the witness carries it.
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

// ITexel<TSelf> contracts every arena element. Lanes is the storage width; Project/Compose are the lane
// correspondence [04]'s row rails fold through, and they are the page's [EXPRESSION_SPINE] kernel exemption —
// fixed-arity index writes into a caller-owned span.
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

// PlaneFormat rows roster storage. Depth is the KERNEL ChannelDtype row; Rent is the type application — the ONE
// column that erases the texel type into the arena, so every surface above this page is generic over ITexel and none
// of them enumerates a format. The two-lane float rows feed the two-component normal path the codec's BC5 block row
// and [04]'s two-lane register seat both already reserve.
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
    // MaxComponents reads the roster's own widest storage arity — the ONE source for a consumer sizing a widest-case lane scratch, so
    // no kernel hardcodes a lane-count literal the roster could outgrow. The roster is FROZEN at construction, so the
    // fold runs once behind a lazy seat rather than per read: filter#PLANE_OP sizes a scratch off this member per row
    // of every band it walks, and a per-access roster fold there is a LINQ pipeline inside a per-texel loop.
    private static readonly Lazy<int> WidestArity = new(static () => Items.Max(static row => row.Components));

    public static int MaxComponents => WidestArity.Value;

    // Normalizes is the ONE discriminant over the four kernel depth rows this arena realizes: the Unorm rows scale
    // onto [0,1] through their witness and the Float rows pass verbatim. The kernel roster carries width, tolerance,
    // and the complex pairing and NO normalization column, because a byte arena reads its own pack arm — so the
    // correspondence is stated once beside the rows that declare it and PlaneRange, AlphaMode, and
    // codec#RASTER_FORMAT all read this member rather than re-testing a dtype identity.
    public static bool Normalizes(ChannelDtype depth) => depth == ChannelDtype.Unorm8 || depth == ChannelDtype.Unorm16;

    // WebReachable is the ONE wire-target discriminant, the storage-side twin of codec#RASTER_CODEC KtxPayload.WireLegal.
    // Narrow 16-bit UNORM stores carry no Vulkan format row the browser KTX2 read path resolves, so r16 and rg16 stay
    // producer-side and desktop-native. Arity and depth decide it as a RULE rather than a per-row column, exactly as
    // Normalizes does, so a new witness or arity answers here once and every refusal follows with no literal to chase.
    // It bites hardest where [05] routes a direction plane to two components: that makes the natural high-precision
    // normal store the undecodable one, and the producer re-routes to Rg16F rather than ship a file no consumer opens.
    public static bool WebReachable(int components, ChannelDtype depth) =>
        depth != ChannelDtype.Unorm16 || components >= 4;

    private PlaneFormat(string key, int components, ChannelDtype depth, AlphaMode alpha, Func<int, int, AllocationMode, PlaneStore> rent)
        : this(key) => (Components, Depth, Alpha, Rent) = (components, depth, alpha, rent);
    private static PlaneFormat Row(string key, int components, ChannelDtype depth, AlphaMode alpha, Func<int, int, AllocationMode, PlaneStore> rent) =>
        new(key, components, depth, alpha, rent);

    // Storage width rounds a SEMANTIC count up through {1, 2, 4}: a three-lane channel takes the four-lane row and
    // declares AlphaMode.None. The roster is total over the three arities at every depth, so the projection never
    // rounds past the arity a channel declares and an absent pair is a typed absence rather than a fabrication.
    public static Option<PlaneFormat> For(int semanticComponents, ChannelDtype depth) =>
        toSeq(Items.Where(row => row.Depth == depth && row.Components >= Math.Max(1, semanticComponents))
                   .OrderBy(static row => row.Components))
             .Head;
}
```

## [04]-[TEXTURE_PLANE]

- Owner: `PlaneStore` the arena base with its `IPlaneFold` re-entry seam; `PlaneStore<T>` the ONE generic realization; `TexturePlane` the admitted plane carrying format, grid, layers, transfer, primaries, association, range, and store.
- Entry: `TexturePlane.Of` is ONE admission over two input modalities discriminating on shape, never a knob — the EXTENT modality `(format, width, height, transfer, alpha, key, layers, range, primaries, pitchMm, mode)` seats a fresh `CellLattice` and the LATTICE modality `(format, grid, layers, transfer, alpha, range, primaries, key, mode)` adopts one a caller already holds (a pyramid level, a world-seated bake target, a re-association twin); the trailing `AllocationMode` defaults to `Clear` so a press writing every texel opts out of the zeroing pass explicitly. `Read(row, layer, lanes)`/`Write(row, layer, lanes)` are the one decoded LANE row rail and `ReadShade(row, layer, texels)`/`WriteShade(row, layer, texels)` its `ShadeVec4` projection — the tile, set, press, and environment folds all stage `ShadeVec4` rows, so the lane-to-register correspondence (single-lane replication, two-lane X/Y with zero Z, alpha seat, four-lane identity) is declared ONCE here rather than re-derived per consumer; `RowScalars` sizes a consumer's lane scratch; `Run(steps)` is the one spatial-grain read; `Layer(index, key)` windows one layer; `ToAlpha(target, key)` and `ToPrimaries(target, key)` are the two declaration crossings; `Key` is the streaming content key.
- Law: the EXTENT SPINE is the kernel `Numerics/atoms#CELL_LATTICE` `CellLattice` and this page mints none. `Width` and `Height` read `Grid.Columns` and `Grid.Rows`, `Linear` is the lattice's own linearization, the `Array.MaxLength` element budget is the lattice's `ceiling` argument, and `Coarsen` is the `[05]` level step — so a texel grid, a voxel sweep, an overview chain, and a Fabrication field all address through one owner. Admission seats the lattice at `Layers = 1` and `TexturePlane` keeps its OWN layer band, because the plane's layers are a STACKING axis whose law `set#TEXTURE_SET` `LayerLaw` names — cube faces, array slices, flipbook frames — and `Coarsen` halves every lattice axis: folding the band into the lattice would halve six cube faces to three at the first mip level.
- Law: the SPATIAL GRAIN rides the affine and the READ CARRIES NO UNIT IN ITS NAME, because it does not carry one in its value: a pixel plane seats the identity map, so its cell measures one texel and `Run` returns a texel-unit run; a physically-pitched plane seats its millimetres-per-texel as a uniform scale, so `Run` returns millimetres and the same relief at two resolutions derives one horizon, one curvature magnitude, and one gradient slope. A millimetre suffix here would assert the pitched case on every identity-seated plane in the corpus and collide with the genuinely-millimetre `Component/joint#JOINT_FAMILY` weld run under one spelling; the typed-absence arm is the honest one, so the AFFINE is the unit witness and a caller needing physical units seats a pitch. Grain is therefore a property of the grid every derivative reads off the plane it is differentiating, never a column each derivative carrier re-declares. `Run(columns, rows)` takes the march as a PER-AXIS texel count and returns its Euclidean length through `Grid.CellSize`, so an anisotropic seat is honoured rather than approximated: reading `CellSize.X` alone would report a vertical sweep's rise over a horizontal spacing, and every consumer marching a direction — the horizon sweep, the curvature stencil, the gradient slope — passes the direction it actually walked.
- Law: a ten-case store union is the DELETED form. Ten cases carried one field pair and one disposal, so the arena is one generic record and typed code re-enters through `Accept<TFold, TResult>` — a `struct` or `ref struct` fold the JIT specializes per texel, allocating nothing and capturing nothing. `PlaneFormat` rows carry their own `Rent` column, deleting the `format.Key` switch that throws on an unmatched row: an unmatched format is unrepresentable rather than an exception in a fallible path.
- Law: `Of` refuses BEFORE it rents. `MaterialFault.Parameter` rails an association the storage row cannot hold and an element count above `Array.MaxLength`, each carrying the offending axis in its reason; a non-positive extent is unrepresentable because `Dimension` admits at one and above. `Grid.CellCount × Layers` texels bound admission and make the typed arena worth its shape: a 16k four-lane 16-bit plane counts 268 435 456 and admits, while the same plane's byte count exceeds the runtime bound.
- Law: layer `n` occupies rows `[n × height, (n + 1) × height)` of one arena. `Layer` windows that band without a second rental and without touching the grid, so a cube face set, an array slice, a volume slab, and a flipbook frame are all one plane and the `set#TEXTURE_SET` `LayerLaw` row is the only thing that names which.
- Law: `Read` runs ONE decode ladder and `Write` runs its exact inverse — texel lanes, component normalization, alpha un-association, `PlaneRange` unpack, transfer decode over the colour lanes alone. Un-association precedes the unpack because coverage weights the STORED value; every `Signed` row on the roster carries `AlphaMode.None`, so the two steps never both fire on one plane and the order is stated for the ladder's own coherence. Every consumer above this page reads decoded, signed, scene-linear lanes, so no kernel in the estate re-derives a curve, re-divides by a maximum, or re-packs a signed field. `Read` leaves the alpha lane untouched by every transfer: an association is a linear coverage weight, and running a display curve over it darkens every edge.
- Law: `ToAlpha` and `ToPrimaries` are the two DECLARATION CROSSINGS and both convert on decoded lanes. `ToAlpha` refuses the `straight`↔`associated` crossing below 16 bits, because at eight bits the un-association divides by a quantized coverage and a low-alpha texel amplifies its own quantization step into a visible colour error the round trip cannot recover. `ToPrimaries` refuses an unknown endpoint, because a gamut rebase against an undeclared chromaticity is a fabricated transform; a decode RECORDS the primaries its container declared and never converts, so the one conversion site is a caller stating both ends.
- Law: the content key is the kernel `ContentHash.Of` streaming entry over the plane's own storage rows in layer-major, row-major order, seeded zero like every other federation key. `ContentHash.Of` is the sole mint site, holding the federation seed and the cross-branch digest reproduction; the whole-plane byte span is never materialized, so a 268-million-texel plane keys in one pooled row window.
- Packages: CommunityToolkit.HighPerformance (`MemoryOwner<T>.Allocate`/`.Memory`/`.Dispose`, `AllocationMode.Clear`/`Default`, `Memory<T>.AsMemory2D(int, int)`, `Memory2D<T>.Span`, `Memory2D<T>.Slice(int, int, int, int)`, `Span2D<T>.GetRowSpan(int)`, `SpanOwner<T>.Allocate` the per-row lane scratch), `Rasm.Domain` (`ContentHash.Of<TState>(TState, Action<TState, XxHash128>)` the ONE identity entry, `Op`), `Rasm.Numerics` (composed — `CellLattice.Of`/`Columns`/`Rows`/`CellCount`/`CellSize`/`Linear`/`Coarsen` the ONE bounded cell lattice, `Placement.Build` + `TransformSpec.UniformScale` the one transform mint, `Dimension`, `PositiveMagnitude`), `bsdf#SHADING_FRAME` (`MaterialFault` band 2450), TinyEXR.NET (composed — `ImageProcessing.GetColorMatrix(ColorSpace, ColorSpace) -> ColorMatrix3x3` the ONE reconciliation mint and `ImageProcessing.ApplyColorMatrix(ReadOnlySpan<float>, Span<float>, int, ColorMatrix3x3)` the interleaved fold), RhinoCommon (`Transform.Identity`, `Point3d.Origin` at the affine seat alone), `System.IO.Hashing` (`XxHash128.Append` inside the kernel entry alone), BCL inbox (`Array.MaxLength`, `double.Hypot`, `MemoryMarshal.AsBytes`).
- Boundary: the arena is TYPED end to end and exposes no whole-plane byte view. `MemoryMarshal.AsBytes` over one row span is the sole reinterpretation, taken by the key fold and by the codec bridge, so a caller cannot address the plane as bytes and no consumer can smuggle a depth reinterpretation past the format row. `AllocationMode.Clear` is the admission default because a partially-written plane must read its neutral rather than pool residue, and a press writing every texel passes `AllocationMode.Default` to skip the zeroing pass over a quarter-billion elements.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.IO.Hashing;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using Rasm.Domain;                                // Op, ContentHash
using Rasm.Materials.Appearance.Bsdf;             // MaterialFault
using Rasm.Materials.Appearance.Texture;          // ShadeVec4 — the four-lane register the Shade row rails project
using Rasm.Numerics;                              // CellLattice, Placement, TransformSpec, Dimension, PositiveMagnitude
using Rhino.Geometry;                             // Transform, Point3d — the affine seat alone
using TinyEXR.V3;                                 // ImageProcessing, ColorMatrix3x3 — the resolved-once gamut fold
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [MODELS] ------------------------------------------------------------------------------
// IPlaneFold<TResult> seams re-entry. A caller holding a type-erased store hands in a struct fold and the JIT specializes
// one body per texel type — closure-free, allocation-free, and total, because PlaneStore<T> is the only realization.
public interface IPlaneFold<TResult> {
    TResult Fold<T>(Memory2D<T> view) where T : unmanaged, ITexel<T>;
}

// ONE generic arena. The abstract base exists to erase T at the TexturePlane field; it carries no case roster, so a
// new storage row adds no case here — it adds a PlaneFormat row whose Rent column names the type application.
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

// The owner slot is OPTIONAL because ownership is what a window does not have: an absent owner makes "a windowed
// layer never owns the arena" a fact the type holds rather than a sentence a caller has to read, and a layer's
// Dispose can no longer tear down the parent's rental out from under every sibling band.
public sealed record PlaneStore<T>(Option<MemoryOwner<T>> Owner, Memory2D<T> View) : PlaneStore
    where T : unmanaged, ITexel<T> {
    public override int Lanes => T.Lanes;
    public override void Dispose() => Owner.Iter(static owner => owner.Dispose());
    public override TResult Accept<TFold, TResult>(scoped in TFold fold) => fold.Fold(View);
    // Window BORROWS the rental — the layer band is a view, never a copy — so disposal stays with the owning plane.
    public override PlaneStore Window(int rowOffset, int rows) =>
        new PlaneStore<T>(Option<MemoryOwner<T>>.None, View.Slice(rowOffset, 0, rows, View.Width));
}

// TexturePlane admits the plane. Grid is the kernel cell lattice seated at ONE layer — it owns the two spatial axes, the
// index-to-world affine that carries the spatial grain, the linearization, the budget ceiling, and the Coarsen step
// [05] folds. Layers extends the arena by BAND rather than by a lattice axis, so cube faces, array slices, volume
// slabs, and flipbook frames are ONE shape whose law set#TEXTURE_SET's LayerLaw row names — and a mip level never
// halves a face count.
public sealed record TexturePlane(
    PlaneFormat Format,
    CellLattice Grid,
    Dimension Layers,
    PlaneTransfer Transfer,
    PlanePrimaries Primaries,
    AlphaMode Alpha,
    PlaneRange Range,
    PlaneStore Store) : IDisposable {

    // Dimension.Create(1) is the page's ONE total construction: the literal is statically inside the >=1 domain, so the
    // generated throw is unreachable and this is the named [EXPRESSION_SPINE] admission exemption rather than a rail.
    private static readonly Dimension Single = Dimension.Create(value: 1);

    // THE EXTENT MODALITY: a caller stating a texel census and an optional physical pitch. The lattice's own ceiling
    // gate is the element budget for the spatial census; the layer band multiplies it, so the arena gate below closes the
    // product. Both refuse onto band 2450, because a shape refusal never wears a mechanical code.
    public static Fin<TexturePlane> Of(
        PlaneFormat format, Dimension width, Dimension height, PlaneTransfer transfer, AlphaMode alpha, Op key,
        Option<Dimension> layers = default, Option<PlaneRange> range = default, Option<PlanePrimaries> primaries = default,
        Option<PositiveMagnitude> pitchMm = default, AllocationMode mode = AllocationMode.Clear) =>
        from map in Seat(pitchMm, key)
        from grid in CellLattice.Of(map, width, height, Single, Array.MaxLength, key)
            .MapFail(_ => (Error)MaterialFault.Parameter(key, $"<plane-extent:{width.Value}x{height.Value}>"))
        from plane in Of(format, grid, layers.IfNone(Single), transfer, alpha,
            range.IfNone(PlaneRange.Unit), primaries.IfNone(PlanePrimaries.Unknown), key, mode)
        select plane;

    // THE LATTICE MODALITY: a caller holding an admitted grid — a coarsened level, a world-seated bake target, a
    // re-association twin. A padded lane is legal (AlphaMode.None over a four-lane row is the [03] three-component
    // law), so the storage gate refuses only an alpha the row cannot HOLD, never a declaration narrower than the row.
    public static Fin<TexturePlane> Of(
        PlaneFormat format, CellLattice grid, Dimension layers, PlaneTransfer transfer, AlphaMode alpha,
        PlaneRange range, PlanePrimaries primaries, Op key, AllocationMode mode = AllocationMode.Clear) {
        long rows = (long)grid.Rows.Value * layers.Value;
        long elements = grid.CellCount * layers.Value;
        return (elements, !alpha.Carries || format.Alpha.Carries) switch {
            ( > Array.MaxLength, _) => MaterialFault.Parameter(key, $"<plane-elements:{elements}>"),
            (_, false) => MaterialFault.Parameter(key, $"<plane-alpha-storage:{alpha.Key}!={format.Alpha.Key}>"),
            _ => Fin.Succ(new TexturePlane(format, grid, layers, transfer, primaries, alpha, range,
                     format.Rent(grid.Columns.Value, checked((int)rows), mode))),
        };
    }

    // Seat gives a pixel plane the IDENTITY affine, so its cell measures one texel and every derivative reads a
    // texel-unit run; a physically-pitched plane seats its millimetres-per-texel through the kernel's own transform
    // mint, so the grain is a property of the grid rather than a column each derivative carrier re-declares.
    private static Fin<Transform> Seat(Option<PositiveMagnitude> pitchMm, Op key) =>
        pitchMm.Match(
            Some: pitch => Placement.Build(new TransformSpec.UniformScale(Anchor: Point3d.Origin, Factor: pitch.Value), key: key),
            None: () => Fin.Succ(Transform.Identity));

    public Dimension Width => Grid.Columns;
    public Dimension Height => Grid.Rows;
    public int Lanes => Store.Lanes;
    public int RowScalars => Width.Value * Lanes;
    public long Texels => Grid.CellCount * Layers.Value;
    // Run reads the spatial grain in the affine's OWN units — one texel of march under the identity seat, millimetres
    // under a pitched one — and carries no unit suffix precisely because the value carries no fixed unit: naming it
    // for millimetres would assert the pitched case on every pixel plane and fork one spelling with the genuinely
    // millimetre weld run beside it. Every horizon sweep, curvature stencil, and gradient slope divides its rise by
    // THIS run, so the affine is the single unit witness and no derivative carrier re-declares one.
    // The march is a per-axis TEXEL COUNT and the run is its Euclidean length through the grid's own cell, so both
    // CellSize axes ride the read: the lattice modality admits an anisotropic seat, and an X-only read would report a
    // vertical sweep's rise over a horizontal spacing and tilt every horizon on a non-square cell. An axis-aligned
    // caller passes zero on the other axis and the hypotenuse collapses to the exact product it had before.
    public double Run(int columns, int rows) => double.Hypot(columns * Grid.CellSize.X, rows * Grid.CellSize.Y);

    // One layer as a plane of its own over the SHARED rental, so a cube face folds its own pyramid and lifts its own
    // sampler without a copy. The grid is untouched — a face and its parent address the same spatial lattice.
    // Disposal stays with this plane: a windowed layer never owns the arena.
    public Fin<TexturePlane> Layer(int index, Op key) =>
        index >= 0 && index < Layers.Value
            ? Fin.Succ(this with { Layers = Single, Store = Store.Window(index * Height.Value, Height.Value) })
            : MaterialFault.Parameter(key, $"<plane-layer:{index}:{Layers.Value}>");

    // THE DECODE LADDER, stated once for the corpus: texel lanes -> component normalization -> signed unpack ->
    // un-association -> transfer decode over the COLOUR lanes alone. The alpha lane is a linear coverage weight and
    // never crosses a transfer curve, because a display curve over coverage darkens every edge in the plane.
    public void Read(int row, int layer, Span<double> lanes) =>
        Store.Accept<RowRead, Unit>(new RowRead(this, (layer * Height.Value) + row, lanes));

    public void Write(int row, int layer, ReadOnlySpan<double> lanes) =>
        Store.Accept<RowWrite, Unit>(new RowWrite(this, (layer * Height.Value) + row, lanes));

    // ReadShade projects the decoded row rail into ShadeVec4 — ONE lane-to-register correspondence for every consumer
    // staging four-lane rows: a single lane replicates across XYZ, a two-lane pair fills X and Y with Z zero (the
    // frozen two-component reconstruction rows rebuild Z downstream, so replicating X into Z would hand the
    // reconstruction a fabricated component), four lanes map straight through, and the alpha register reads the
    // coverage lane where the plane carries one, else 1.0. WriteShade inverts it, so the tile fold, the press
    // staging, the set mean, and the sky sweep never re-derive a lane seat and a plane's arity change breaks HERE
    // rather than in five consumers.
    public void ReadShade(int row, int layer, Span<ShadeVec4> texels) {
        using SpanOwner<double> lanes = SpanOwner<double>.Allocate(RowScalars);
        Read(row, layer, lanes.Span);
        int stride = Lanes, colour = Alpha.Carries ? stride - 1 : stride;
        for (int x = 0; x < Width.Value; x++) {
            ReadOnlySpan<double> texel = lanes.Span.Slice(x * stride, stride);
            texels[x] = new ShadeVec4(
                texel[0],
                colour > 1 ? texel[1] : texel[0],
                colour > 2 ? texel[2] : (colour == 2 ? 0.0 : texel[0]),
                Alpha.Carries ? texel[stride - 1] : 1.0);
        }
    }

    public void WriteShade(int row, int layer, ReadOnlySpan<ShadeVec4> texels) {
        using SpanOwner<double> lanes = SpanOwner<double>.Allocate(RowScalars);
        int stride = Lanes, colour = Alpha.Carries ? stride - 1 : stride;
        for (int x = 0; x < Width.Value; x++) {
            Span<double> texel = lanes.Span.Slice(x * stride, stride);
            texel[0] = texels[x].X;
            if (colour > 1) { texel[1] = texels[x].Y; }
            if (colour > 2) { texel[2] = texels[x].Z; }
            if (Alpha.Carries) { texel[stride - 1] = texels[x].W; }
        }
        Write(row, layer, lanes.Span);
    }

    // Association conversion on decoded lanes. The 16-bit floor is structural: at eight bits the un-association
    // divides by a quantized coverage and amplifies its own step into colour error the inverse cannot recover.
    // Convertible is the one crossing predicate; set#TEXTURE_SET reads the same row fact at admission.
    public Fin<TexturePlane> ToAlpha(AlphaMode target, Op key) =>
        target == Alpha ? Fin.Succ(this)
        : !Alpha.Convertible(target, Format.Depth)
            ? MaterialFault.Parameter(key, $"<plane-alpha-crossing:{Alpha.Key}->{target.Key}:{Format.Depth.Key}>")
        : Of(Format, Grid, Layers, Transfer, target, Range, Primaries, key, AllocationMode.Default)
              .Map(Reassociate);

    // Gamut conversion on decoded lanes, folded through the composed reconciliation matrix per row. Unknown at
    // either end REFUSES: a decode RECORDS what its container declared and converts nothing, so this is the one
    // site where both endpoints are stated and a rebase over an undeclared chromaticity is unrepresentable.
    public Fin<TexturePlane> ToPrimaries(PlanePrimaries target, Op key) =>
        target == Primaries
            ? Fin.Succ(this)
            : from matrix in Primaries.Matrix(target, key)
              from destination in Of(Format, Grid, Layers, Transfer, Alpha, Range, target, key, AllocationMode.Default)
              select Rebase(destination, matrix);

    // Association converts THROUGH the decode ladder: the source reads straight, un-associated lanes and the
    // destination's own AlphaMode re-applies coverage on write, so the conversion is the ladder's inverse pair and
    // never a second premultiply spelling. The row walk is the page's [EXPRESSION_SPINE] kernel exemption.
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

    // Rebase walks the plane against an ALREADY-RESOLVED matrix, so the walk is TOTAL and the ORDER is what makes it
    // leak-free: the one refusal an endpoint can raise resolves before the destination is rented, so no failure arm
    // exists to orphan a rental and the disposal block a mid-walk rail needed is deleted rather than corrected.
    // Colour lanes stage as float — the composed matrix fold's own domain — and the coverage lane rides through
    // untouched, because a chromaticity transform over coverage is a colour transform over an area. The walk is the
    // page's [EXPRESSION_SPINE] kernel exemption.
    private TexturePlane Rebase(TexturePlane destination, ColorMatrix3x3 matrix) {
        int colour = Alpha.Carries ? Lanes - 1 : Lanes;
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

    // Key mints the federation content key through the kernel streaming entry over storage rows in layer-major, row-major
    // order at seed zero. No whole-plane byte span is ever materialized and no local hash is ever seeded.
    public UInt128 Key => ContentHash.Of(this, static (plane, hash) => plane.Store.Accept<KeyRows, Unit>(new KeyRows(hash)));

    public void Dispose() => Store.Dispose();
}
```

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
// Each row fold is a ref struct carrying the caller's span with no heap hop, and each body is a fixed-arity index
// kernel over a caller-owned buffer — the page's [EXPRESSION_SPINE] kernel exemption.
namespace Rasm.Materials.Raster;

internal readonly ref struct RowRead(TexturePlane plane, int storageRow, Span<double> lanes) : IPlaneFold<Unit> {
    public Unit Fold<T>(Memory2D<T> view) where T : unmanaged, ITexel<T> {
        ReadOnlySpan<T> source = view.Span.GetRowSpan(storageRow);
        int width = source.Length, stride = T.Lanes, colour = plane.Alpha.Carries ? stride - 1 : stride;
        using SpanOwner<float> encoded = SpanOwner<float>.Allocate(width * colour);
        for (int x = 0; x < width; x++) {
            T.Project(in source[x], lanes.Slice(x * stride, stride));
            double coverage = plane.Alpha.Carries ? lanes[(x * stride) + stride - 1] : 1.0;
            for (int c = 0; c < colour; c++) {
                double unit = lanes[(x * stride) + c];
                double straight = plane.Alpha.Premultiplied && coverage > 0.0 ? unit / coverage : unit;
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
        int width = destination.Length, stride = T.Lanes, colour = plane.Alpha.Carries ? stride - 1 : stride;
        using SpanOwner<float> linear = SpanOwner<float>.Allocate(width * colour);
        using SpanOwner<double> texel = SpanOwner<double>.Allocate(stride);
        for (int x = 0; x < width; x++) {
            for (int c = 0; c < colour; c++) { linear[(x * colour) + c] = (float)lanes[(x * stride) + c]; }
        }
        Span<float> encoded = linear.Span;
        plane.Transfer.Encode(linear.Span, encoded);
        for (int x = 0; x < width; x++) {
            double coverage = plane.Alpha.Carries ? lanes[(x * stride) + stride - 1] : 1.0;
            for (int c = 0; c < colour; c++) {
                double packed = plane.Range.Pack(encoded[(x * colour) + c], plane.Format.Depth);
                texel.Span[c] = plane.Alpha.Premultiplied ? packed * coverage : packed;
            }
            if (plane.Alpha.Carries) { texel.Span[stride - 1] = coverage; }
            destination[x] = T.Compose(texel.Span);
        }
        return Unit.Default;
    }
}

// KeyRows streams STORAGE bytes, not decoded lanes: identity is what the object store holds, so a texel edit
// re-keys and a declaration retag does NOT — transfer, primaries, range, and association ride the wire's channel
// row, never the blob preimage, which is what lets ingest re-declare a decoded container per role without
// re-addressing it. The grid rides the same side of that line: a plane re-seated at a different physical pitch
// holds identical bytes and keys identically, and the pitch reaches a consumer through the declaring row.
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
- Law: `RoughnessVariance` is PAIRED and its coupling is quantitative: the paired normal level's mean-vector length `L` carries the directional variance the fold destroyed, so the level's roughness becomes `min(1, sqrt(r² + 2(1 − L)/L))` and specular aliasing does not reappear at distance. `RoughnessVariance` admits a build carrying no paired chain — the row folds under `Box` and records `Coupled: false`, which `press#PRESS_RECEIPT` publishes as the declared quality floor, because a set whose normal chain has not yet been tiled must still produce a roughness chain.
- Law: `AsImage` MATERIALIZES the chain into the sampler's own carrier — each level's decoded lanes projected into `ShadeVec4` COPIES the sampler owns outright, so the returned `TextureSource.Image` is INDEPENDENT of the pyramid's arenas and disposing the pyramid after the lift is legal; the ownership crossing is a copy, never a view, which is what lets a consumer hold the sampler past the chain's lifetime without a use-after-free the type cannot see. This estate mints no second sampler, no second reconstruction, and no second address mode. `TextureSource.Image` carries one layer, so the lift runs PER LAYER by construction: a multi-layer plane refuses and the caller extracts a `Layer` first, which is exactly what makes the cube-face and array arms honest rather than declared capability that cannot run.
- Law: `AsImage` COSTS a full second residency and the cost is DECLARED rather than discovered. The lift copies every level into `ShadeVec4` — four doubles, thirty-two bytes a texel — and the geometric chain sums to `4/3` of the base census, so a 4k chain materializes ≈683 MiB and a 16k chain ≈10.7 GiB on top of the arena the pyramid already holds. `Texels` publishes that census so a caller budgets before it lifts. The lift is therefore a DELIBERATE per-plane act: `tile#TILE_GATE` lifts once per plane to grade a set and `tile#TILE_SYNTH` lifts once per plane to solve it, so a full-channel set pays the ceiling once per graded plane and never once per level or once per tap, and a caller lifting inside a per-channel loop over a large set must band its own walk instead. `[06]` is the shape a residency-bounded caller reaches for when the whole grid will not fit.
- Packages: `plane#TEXTURE_PLANE` (composed — `TexturePlane.Of` over both modalities, `Read`/`Write`/`Layer`/`Grid`/`Key`, `PlaneFormat`, `MipPolicy`), `Rasm.Numerics` (composed — `CellLattice.Coarsen` the ONE level step, `CellLattice.Columns`/`Rows`), `texture#TEXTURE_UV` (composed — `TextureSource.Image.Of(Dimension, Dimension, Seq<ReadOnlyMemory<ShadeVec4>>, Op)` the ONE sampler admission, `ShadeVec4` the four-lane field register), TinyEXR.NET (composed — `ImageProcessing.Resize(ReadOnlySpan<float>, int, int, Span<float>, int, int, int, ResizeFilter, EdgeMode, int)` the delegated separable downsample rows, `EdgeMode.Clamp` at a chain boundary; the `kaiser` row folds through the local windowed-sinc table instead, because no composed roster carries a Kaiser window — not the resampler filters, not the kernel weight profiles, not the kernel `Numerics/matrix#TRANSFORM_BAND` `WindowTaper` band), MathNet.Numerics (composed — `SpecialFunctions.BesselI0(double)` the zeroth-order modified Bessel evaluation the Kaiser window needs, so no local power series exists), CommunityToolkit.HighPerformance (`SpanOwner<T>.Allocate` the per-level staging), `Rasm.Domain` (`ContentHash.Of`, `Op`), LanguageExt.Core.
- Growth: a new fold law is one `MipPolicy` row carrying its filter and its post-fold flags; a new coupling is one flag with its arm in the post-fold. Neither the chain walk, the level admission, the sampler bridge, nor any consumer changes.
- Boundary: arbitrary-ratio resampling is `filter#PLANE_OP` `Resize` — a mip level is the lattice's own `Coarsen` step under a declared policy and never a resize alias, so a chain cannot be minted at an arbitrary ratio and a resize cannot silently produce a level a sampler then trilinearly blends. `TexturePyramid` OWNS its levels and disposes them; a pyramid built over an adopted base at `MipPolicy.None` disposes that base too, so ownership is uniform and a caller never holds a half-owned chain.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Buffers.Binary;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using MathNet.Numerics;                           // SpecialFunctions.BesselI0 — the Kaiser window's own evaluation
using Rasm.Domain;                                // Op, ContentHash
using Rasm.Materials.Appearance.Bsdf;             // MaterialFault
using Rasm.Materials.Appearance.Texture;          // TextureSource, ShadeVec4
using Rasm.Numerics;                              // CellLattice, Dimension
using TinyEXR.V3;                                 // ImageProcessing, EdgeMode
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [MODELS] ------------------------------------------------------------------------------
public sealed record TexturePyramid(Seq<TexturePlane> Levels, MipPolicy Policy, bool Coupled) : IDisposable {
    // Head/Last are Option properties on Seq, so the base read is the POSITIONAL one — total by construction
    // because Of seeds every chain with its base plane before any descent runs.
    public TexturePlane Base => Levels[0];

    // Texels is the chain's whole census — the residency budget [06] spends and the multiplier an AsImage lift pays
    // thirty-two bytes against. A geometric chain sums to 4/3 of its base, so the number is READ off the levels
    // rather than approximated from the base by a ratio the None policy would break.
    public long Texels => Levels.Fold(0L, static (sum, level) => sum + level.Texels);

    // Key folds each LEVEL's own storage key, so a chain rebuilt under a different policy over identical base bytes
    // keys distinctly — the pyramid is what a KTX2 container holds, and its identity is the whole chain.
    public UInt128 Key => ContentHash.Of(this, static (chain, hash) => chain.Levels.Iter(level => {
        Span<byte> digest = stackalloc byte[16];
        BinaryPrimitives.WriteUInt128LittleEndian(digest, level.Key);
        hash.Append(digest);
    }));

    // ONE entry over both modalities: the policy row decides whether a chain exists, so no caller passes a level count
    // and no second `Single`/`Chain` factory exists. `paired` is the companion chain a coupled row reads.
    public static Fin<TexturePyramid> Of(TexturePlane basePlane, MipPolicy policy, Op key, Option<TexturePyramid> paired = default) =>
        !policy.Chains
            ? Fin.Succ(new TexturePyramid(Seq(basePlane), policy, Coupled: false))
            : Chain(basePlane, policy, paired, key);

    private static Fin<TexturePyramid> Chain(TexturePlane basePlane, MipPolicy policy, Option<TexturePyramid> paired, Op key) {
        bool coupled = policy.Coupled && paired.IsSome;
        return Descend(basePlane.Grid, key).Bind(grids =>
            grids.Fold(Fin.Succ(Seq(basePlane)), (state, grid) => state.Bind(levels =>
                Fold(levels[levels.Count - 1], grid, policy, coupled ? paired.Bind(chain => Level(chain, levels.Count)) : None, key)
                    .Map(levels.Add))))
            .Map(levels => new TexturePyramid(levels, policy, coupled));
    }

    // THE LEVEL CHAIN IS THE GRID CHAIN. The kernel Coarsen halves each spatial axis with a floor of one and doubles the
    // cell, so the descent ends where the census stops moving and no page carries a log2 level count or a Halve of
    // its own. Depth is logarithmic in the extent — fifteen levels at a 16k plane — so the descent is bounded by the
    // census it walks, and a level count is what a caller READS off the chain rather than what it supplies.
    private static Fin<Seq<CellLattice>> Descend(CellLattice grid, Op key) =>
        grid.Columns.Value is 1 && grid.Rows.Value is 1
            ? Fin.Succ(Seq<CellLattice>.Empty)
            : grid.Coarsen(key)
                .MapFail(_ => (Error)MaterialFault.Parameter(key, $"<pyramid-coarsen:{grid.Columns.Value}x{grid.Rows.Value}>"))
                .Bind(level => Descend(level, key).Map(rest => level.Cons(rest)));

    private static Option<TexturePlane> Level(TexturePyramid chain, int index) =>
        index < chain.Levels.Count ? Some(chain.Levels[index]) : None;

    // One level. Rent at the coarsened grid, decode, separable-resample in the LINEAR domain, apply the row's
    // post-fold, re-encode — so the fold is one body over every format, every transfer, and every arity,
    // parameterized by the policy row alone. The grid arrives ADMITTED from Coarsen, so no extent construction and
    // no unreachable-throw exemption survives here.
    private static Fin<TexturePlane> Fold(TexturePlane source, CellLattice grid, MipPolicy policy, Option<TexturePlane> companion, Op key) =>
        TexturePlane.Of(source.Format, grid, source.Layers, source.Transfer, source.Alpha, source.Range,
                source.Primaries, key, AllocationMode.Default)
            .Map(level => Resample(source, level, policy, companion));

    // Resample runs the separable downsample with the row's post-fold over DECODED lanes — the composed filter where the
    // row names one, the Materials-owned windowed-sinc kernel on the kaiser row. Renormalize restores the unit length an
    // averaged vector loses; the coupled arm folds the companion level's mean-vector length L into the roughness lane
    // as sqrt(r^2 + 2(1-L)/L), so the directional variance the normal chain destroyed reappears as roughness rather
    // than as specular aliasing at distance. The companion scratch sizes at the roster's OWN widest arity, because the
    // paired chain's lane count is the companion's fact, never this plane's. The walk is the section's
    // [EXPRESSION_SPINE] kernel exemption.
    private static TexturePlane Resample(TexturePlane source, TexturePlane level, MipPolicy policy, Option<TexturePlane> companion) {
        int lanes = source.Lanes, sw = source.Width.Value, dw = level.Width.Value;
        using SpanOwner<float> src = SpanOwner<float>.Allocate(sw * source.Height.Value * lanes);
        using SpanOwner<float> dst = SpanOwner<float>.Allocate(dw * level.Height.Value * lanes);
        using SpanOwner<double> row = SpanOwner<double>.Allocate(Math.Max(sw, dw) * lanes);
        using SpanOwner<double> pair = SpanOwner<double>.Allocate(dw * PlaneFormat.MaxComponents);
        for (int layer = 0; layer < source.Layers.Value; layer++) {
            for (int y = 0; y < source.Height.Value; y++) {
                source.Read(y, layer, row.Span);
                for (int i = 0; i < sw * lanes; i++) { src.Span[(y * sw * lanes) + i] = (float)row.Span[i]; }
            }
            switch (policy.Filter) {
                case { } filter:
                    ImageProcessing.Resize(src.Span, sw, source.Height.Value, dst.Span, dw, level.Height.Value, lanes,
                        filter, EdgeMode.Clamp, source.Alpha.Carries ? lanes - 1 : -1);
                    break;
                case null:
                    KaiserHalve(src.Span, sw, source.Height.Value, dst.Span, dw, level.Height.Value, lanes,
                        source.Alpha.Carries ? lanes - 1 : -1);
                    break;
            }
            for (int y = 0; y < level.Height.Value; y++) {
                for (int i = 0; i < dw * lanes; i++) { row.Span[i] = dst.Span[(y * dw * lanes) + i]; }
                if (policy.Renormalize) { Renormalize(row.Span, dw, lanes); }
                companion.Iter(normal => {
                    normal.Read(y, layer, pair.Span[..(dw * normal.Lanes)]);
                    Couple(row.Span, pair.Span, dw, lanes, normal.Lanes);
                });
                level.Write(y, layer, row.Span);
            }
        }
        return level;
    }

    // THE KAISER FOLD — the frozen windowed-sinc halving, authored here because no composed filter row is one and the
    // kernel weight roster's band-limited row is a SINC window, not a Kaiser one. The factor is FIXED at two, so the
    // polyphase collapses to ONE twelve-tap table at half-texel offsets, computed at type initialization and
    // normalized to unit sum: w(d) = sinc(d/2) · I0(β√(1−(d/a)²))/I0(β) with β = 4 and support a = 6 source texels. The
    // modified Bessel evaluation is the composed special-function surface, so no local power series exists. Coverage
    // premultiplies across the fold and un-premultiplies after — the same law the composed resampler's alpha index
    // carries — and an out-of-extent tap clamps, matching the chain boundary.
    private static readonly double[] KaiserTaps = KaiserTable();

    private static double[] KaiserTable() {
        const double beta = 4.0, support = 6.0;
        double[] taps = new double[12];
        double total = 0.0;
        for (int tap = 0; tap < taps.Length; tap++) {
            // Every tap sits at a HALF-texel offset, so the sinc argument is never zero and the removable singularity
            // has no representative in this table — a guard for it would be a dead arm asserting a case the fixed
            // half-texel geometry forbids. The even offsets land on the sinc's own zeros, which is the band-limit.
            double d = tap - 5.5;
            double sinc = Math.Sin(Math.PI * d / 2.0) / (Math.PI * d / 2.0);
            double t = d / support;
            taps[tap] = sinc * (SpecialFunctions.BesselI0(beta * Math.Sqrt(Math.Max(0.0, 1.0 - (t * t))))
                              / SpecialFunctions.BesselI0(beta));
            total += taps[tap];
        }
        for (int tap = 0; tap < taps.Length; tap++) { taps[tap] /= total; }
        return taps;
    }

    private static void KaiserHalve(ReadOnlySpan<float> src, int sw, int sh, Span<float> dst, int dw, int dh, int lanes, int alphaLane) {
        using SpanOwner<float> mid = SpanOwner<float>.Allocate(dw * sh * lanes);
        AxisHalve(src, sw, sh, mid.Span, dw, lanes, alphaLane, horizontal: true);
        AxisHalve(mid.Span, dw, sh, dst, dh, lanes, alphaLane, horizontal: false);
    }

    // ONE axis body serves both passes. Each destination texel centres at 2i + 0.5 in source units, so the twelve
    // taps sit at fixed half-texel offsets and the table is the whole filter; coverage premultiplies into the fold
    // and divides back out, so a transparent texel never bleeds opaque colour across a coverage edge.
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

    // A DIRECTION plane stores two or three axes — the two-component store carries the frozen reconstruction's
    // implied Z, so the unit length the fold restores is the RECONSTRUCTED vector's and never the stored pair's,
    // and the axis count is read off the plane rather than assumed three.
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

    // AsImage bridges to the sampler. Each level's decoded lanes project into the sampler's own ShadeVec4 carrier and the
    // whole chain admits ONCE through TextureSource.Image.Of, so extent truth is structural at the sampler and no per-tap
    // recheck exists. A layered plane refuses: the sampler carries one layer, so the caller extracts a Layer first.
    public Fin<TextureSource.Image> AsImage(Op key) =>
        Base.Layers.Value is not 1
            ? MaterialFault.Parameter(key, $"<pyramid-layered:{Base.Layers.Value}>")
            : TextureSource.Image.Of(Base.Width, Base.Height, Levels.Map(static level => Materialize(level)), key);

    // Materialize reads the plane's own ReadShade rail for its lane-to-register correspondence, so the bridge stages rows and owns
    // no second projection law.
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
- Growth: a new eviction law is one `ResidencyPolicy` row projecting its own rank; a cost model other than texels is one column on `ResidentTile` and one read in `Reclaim`. Neither the window, the resolve rail, the mint seam, nor any consumer changes.
- Boundary: this window bounds CHAINS, never arena bands — `TexturePlane.Layer` windows one rental into layer bands inside a single plane, while `PlaneResidency` holds independent pyramids addressed by a tile coordinate, so a cube-face set and a UDIM grid never share a mechanism. The window carries no decode, no format, and no channel: every tile of one asset resolves through the caller's own mint, so a residency window over base-colour tiles and one over normal tiles are two windows and neither knows the other exists.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Linq;
using LanguageExt;
using Rasm.Domain;                                // Op
using Rasm.Materials.Appearance.Bsdf;             // MaterialFault
using Rasm.Numerics;                              // CellLattice
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [POLICIES] ----------------------------------------------------------------------------
// ResidencyPolicy rows carry the eviction axis as a RANK PROJECTION onto one scalar rather than as a victim selector
// per row, so the window holds ONE single-pass ordered walk and a new eviction law adds no branch. NULL rank declares
// a window that never evicts: the whole declared grid stays resident and an over-budget admission refuses.
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

// --- [MODELS] ------------------------------------------------------------------------------
// Cost freezes at seat time because a chain's census cannot move, so the running residency is arithmetic rather than a
// fold over the roster per admission. Touched and Hits are the two eviction ordinals a policy row projects.
public sealed record ResidentTile(int Index, TexturePyramid Chain, long Cost, long Touched, long Hits);

// PlaneResidency is a MEMO TABLE and carries the mutation that makes one: the resident map and the touch clock are the
// operation's own state, seated with the fold that owns them rather than threaded through every caller. That is this
// section's [EXPRESSION_SPINE] exemption and the only one it takes — every admission, refusal, and rank walk below is
// expression-shaped.
public sealed class PlaneResidency : IDisposable {
    private readonly CellLattice declared;
    private readonly ResidencyPolicy policy;
    private readonly long budget;
    private HashMap<int, ResidentTile> seated = HashMap<int, ResidentTile>.Empty;
    private long resident;
    private long clock;

    private PlaneResidency(CellLattice tiles, ResidencyPolicy row, long texelBudget) =>
        (declared, policy, budget) = (tiles, row, texelBudget);

    // A non-positive budget is the one admission refusal: a zero-budget window admits nothing and every Resolve would
    // refuse, which is a window that cannot answer wearing a window's shape.
    public static Fin<PlaneResidency> Of(CellLattice tiles, ResidencyPolicy policy, long texelBudget, Op key) =>
        texelBudget > 0
            ? Fin.Succ(new PlaneResidency(tiles, policy, texelBudget))
            : MaterialFault.Parameter(key, $"<residency-budget:{texelBudget}>");

    public CellLattice Declared => declared;
    public long Resident => resident;
    public Seq<int> Seated => toSeq(seated.Keys);

    // ONE read over both modalities: a resident index answers from the map and an absent one mints through the
    // caller's thunk, so no consumer branches on presence and no second Prefetch entry exists. The grid gate runs
    // BEFORE the thunk, so a mint never decodes a tile the declared grid does not carry.
    public Fin<TexturePyramid> Resolve(int index, Func<int, Fin<TexturePyramid>> mint, Op key) =>
        seated.Find(index).Match(
            Some: tile => Fin.Succ(Touch(tile)),
            None: () => index >= 0 && index < declared.CellCount
                ? mint(index).Bind(chain => Seat(index, chain, key))
                : MaterialFault.Parameter(key, $"<residency-tile:{index}:{declared.CellCount}>"));

    private TexturePyramid Touch(ResidentTile tile) {
        seated = seated.AddOrUpdate(tile.Index, tile with { Touched = ++clock, Hits = tile.Hits + 1 });
        return tile.Chain;
    }

    // Seat evicts FIRST so peak residency is the budget rather than the budget plus one chain, and disposes the
    // rejected chain on both refusals — a minted pyramid the window declines to seat is this owner's to release,
    // since the caller's thunk already handed ownership across.
    private Fin<TexturePyramid> Seat(int index, TexturePyramid chain, Op key) {
        long cost = chain.Texels;
        Reclaim(budget - cost).Iter(Release);
        if (cost > budget || resident + cost > budget) {
            chain.Dispose();
            return MaterialFault.Parameter(key, $"<residency-over-budget:{cost}:{resident}:{budget}>");
        }
        seated = seated.Add(index, new ResidentTile(index, chain, cost, ++clock, Hits: 1));
        resident += cost;
        return Fin.Succ(chain);
    }

    // Reclaim picks the eviction PREFIX in one ranked pass: rows order by the policy's own scalar and accumulate
    // until the remaining residency fits the headroom, so a large admission frees exactly the prefix it needs and a
    // Retain window yields the empty set and lets Seat refuse. A negative headroom is the over-large chain, which
    // drains the whole window and still refuses — Seat's own cost gate is what stops that, not this walk.
    private Seq<ResidentTile> Reclaim(long headroom) =>
        policy.Rank is { } rank
            ? toSeq(seated.Values.OrderBy(rank))
                  .Fold((Freed: 0L, Victims: Seq<ResidentTile>.Empty), (state, tile) =>
                      resident - state.Freed <= headroom ? state : (state.Freed + tile.Cost, state.Victims.Add(tile)))
                  .Victims
            : Seq<ResidentTile>.Empty;

    private Unit Release(ResidentTile tile) {
        seated = seated.Remove(tile.Index);
        resident -= tile.Cost;
        tile.Chain.Dispose();
        return unit;
    }

    public void Dispose() {
        seated.Values.Iter(static tile => tile.Chain.Dispose());
        (seated, resident) = (HashMap<int, ResidentTile>.Empty, 0L);
    }
}
```

## [07]-[RESEARCH]

(none)
