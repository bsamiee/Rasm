# [MATERIALS_PLANE]

THE DECODED-RASTER SUBSTRATE. One `TexturePlane` owns every pixel grid the texture estate holds — the storage row, the extent, the layer stack, the encoded transfer, the alpha association, and the value range — over a TYPED-TEXEL pooled arena that never becomes a byte arena; one `TexturePyramid` owns the level chain a `MipPolicy` row folds, including the variance coupling a roughness chain takes from its paired normal chain; and one `AsImage` bridge lifts a chain into the existing `texture#TEXTURE_UV` `TextureSource.Image` sampler so the estate mints no second sampler. `PlaneFormat` rows own storage, `PlaneDepth` rows own depth and carry their own component witness, `PlaneTransfer` rows own transfer, and `MipPolicy` rows own the level fold — never a per-format plane type, a per-depth converter pair, or a per-policy pyramid class.

Typed texels size the arena because bytes cannot: `byte[]` caps at `Array.MaxLength`, so a 16k×16k four-lane 16-bit plane spans 2.147 GB of bytes and refuses at the runtime bound, while the same plane counts 268 435 456 TEXELS and rents cleanly — the element count is the only budget that admits the extents this estate bakes. Storage therefore comes from ONE open generic `PlaneStore<T>` over a `MemoryOwner<T>`/`Memory2D<T>` pair, the texel structs are three arities applied to four component witnesses rather than ten hand-written records, and typed code re-enters through a `struct`-or-`ref struct` fold seam that the JIT specializes per texel with no boxing, no closure, and no per-row delegate. Every consumer above this page reads and writes DECODED lanes through the one `Read`/`Write` row rail, so the encode ladder — integer normalization, signed `(v+1)/2` packing, transfer decode, alpha association — is stated exactly once in the corpus and no kernel re-derives a curve. `Rasm.Materials.Raster` composes `CommunityToolkit.HighPerformance` for the pooled arena and its plane views, `TinyEXR.NET` `ImageProcessing` for every transfer and separable resample fold, the kernel `Dimension`/`Op`/`ContentHash` atoms, the `bsdf#SHADING_FRAME` `MaterialFault` band-2450 rail for every shape refusal, and the `texture#TEXTURE_UV` `TextureSource.Image`/`ShadeVec4` sampler at the one lift — re-minting no allocator, no transfer curve, no resampler, no hash, and no fault.

## [01]-[INDEX]

- [02]-[PLANE_VOCABULARY]: `PlaneDepth`/`PlaneTransfer`/`AlphaMode`/`PlaneRange`/`NormalConvention`/`MipPolicy` axes each carry conversion behaviour as row data.
- [03]-[PLANE_FORMAT]: `IComponent`/`ITexel` static-abstract witnesses type the three-arity texel family, and the ten-row `PlaneFormat` storage roster resolves a semantic count.
- [04]-[TEXTURE_PLANE]: `PlaneStore<T>` generalizes the arena behind its `IPlaneFold` seam, and `TexturePlane` owns admission, the layer window, the decoded `Read`/`Write` row rails, the association conversion, and the streaming content key.
- [05]-[TEXTURE_PYRAMID]: `MipPolicy` drives the level fold and the paired variance coupling, and `AsImage` bridges the chain to the sampler.

## [02]-[PLANE_VOCABULARY]

- Owner: `PlaneDepth` the storage-component axis carrying its byte width and its integer flag; `PlaneTransfer` the encoded-transfer axis carrying its `TinyEXR.V3.TransferFunction` binding and the quantity it declares; `AlphaMode` the association axis; `PlaneRange` the stored-value-range axis; `NormalConvention` the green-polarity axis; `MipPolicy` the level-fold axis carrying its separable filter and its two post-fold flags.
- Cases: depth {`u8`, `u16`, `f16`, `f32`} · transfer {`linear`, `srgb`, `raw`, `pq`, `hlg`} · alpha {`straight`, `associated`, `none`} · range {`unit`, `signed`} · convention {`gl`, `dx`} · mip {`box`, `kaiser`, `normalRenormalize`, `roughnessVariance`, `none`}.
- Law: `PlaneQuantity` splits the transfer rows by WHAT the stored number is — `light` for a scene-linear radiometric value, `parameter` for a shading input no colour transform may touch, `display` for a display-referred encoding. `PlaneQuantity` keeps `raw` and `linear` two rows rather than one alias: both decode by identity, but a colour transform legally reaches a `light` plane and never a `parameter` plane. `SceneReferred` is the SEPARATE bake-legality column the `set#TEXTURE_SET` admission gate reads: `srgb` is display-referred as an ENCODING yet scene-referred as a BAKE TARGET because `Read` decodes it to scene-linear, so `linear`, `srgb`, and `raw` carry `true` while `pq` and `hlg` — legal on an environment plane alone — carry `false` and refuse at `TextureSet.Of` for every channel plane. Folding legality onto `Quantity` would refuse every srgb colour channel the wire freeze legalizes.
- Law: `PlaneRange` is the SIGNED-ENCODE owner and the only site in the corpus that spells `(v + 1) / 2` or `2v − 1`. `PlaneRange.Signed` packs its `[-1,1]` value into the storage `[0,1]` span at integer depth and unpacks on read, and stores the signed value verbatim at float depth, so a normal, a tangent, or a curvature plane carries one declaration and every kernel above reads the signed value whatever the depth beneath it.
- Law: `NormalConvention` homes HERE because green polarity is a property of the stored plane exactly as association and transfer are. `gl` is the canonical `+Y` wire form; `dx` is admitted at ingest and converted once through the `filter#PLANE_OP` `Swizzle` lane inversion before the plane is keyed, so no plane leaves the estate carrying `−Y` green and the silent lighting inversion is unrepresentable.
- Law: `MipPolicy.Kaiser` binds the widest separable filter the composed resampler ships. `Box` is the arithmetic 2×2 mean, `NormalRenormalize` folds box and then unit-normalizes each texel vector, `RoughnessVariance` folds box and then absorbs the directional variance its paired normal chain lost at the same level, and `None` declares a single-level plane. Every fold runs in the LINEAR domain — a plane decodes, folds, and re-encodes per level, because averaging `srgb`-encoded texels darkens the pyramid.
- Boundary: rows carry CONVERSION, never storage — a depth row knows its byte width and integer flag, a transfer row knows its `TransferFunction` binding, a range row knows its packing, and a mip row knows its filter and its two post-folds. `[03]` owns the typed arena consuming them, so a new depth lands as one `IComponent` witness and one row and reaches the whole page without touching an arena, a rail, or a codec.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Linq;
using CommunityToolkit.HighPerformance.Buffers;   // AllocationMode — the row each PlaneFormat Rent column binds
using LanguageExt;                                // Fin, Option, Seq, Unit
using Rasm.Domain;                                // Op, ContentHash
using Rasm.Materials.Appearance.Bsdf;             // MaterialFault — the band-2450 shape rail
using Rasm.Numerics;                              // Dimension
using Thinktecture;                               // [SmartEnum<T>], [KeyMemberEqualityComparer]
using TinyEXR.V3;                                 // ImageProcessing, TransferFunction, ResizeFilter, EdgeMode
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

// PlaneDepth rows carry the storage component. Bytes size every rental and payload; Integer decides whether the lane
// normalizes and whether a signed range packs. A new depth is ONE row with its [03] IComponent witness — never a converter pair.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlaneDepth {
    public static readonly PlaneDepth U8 = new("u8", bytes: 1, integer: true);
    public static readonly PlaneDepth U16 = new("u16", bytes: 2, integer: true);
    public static readonly PlaneDepth F16 = new("f16", bytes: 2, integer: false);
    public static readonly PlaneDepth F32 = new("f32", bytes: 4, integer: false);

    public int Bytes { get; }
    public bool Integer { get; }
    private PlaneDepth(string key, int bytes, bool integer) : this(key) => (Bytes, Integer) = (bytes, integer);
}

// PlaneTransfer rows carry the encoded transfer. Function is the composed fold's own row and NULL means identity — the
// one place `raw` and `linear` differ numerically is nowhere, so the difference lives on Quantity instead of on a second curve.
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
    // The bake-legality column set#TEXTURE_SET reads: srgb decodes to scene-linear at Read, so it bakes; pq/hlg are
    // environment-only display transfers that refuse on every channel plane.
    public bool SceneReferred { get; }
    public bool Identity => Function is null or TransferFunction.Linear;
    private PlaneTransfer(string key, TransferFunction? function, PlaneQuantity quantity, bool sceneReferred) : this(key) =>
        (Function, Quantity, SceneReferred) = (function, quantity, sceneReferred);

    // Decode reads an ENCODED lane run and writes scene-linear; Encode is its inverse. Both delegate to the composed
    // span fold, which admits a destination aliasing its source at the same start — so one scratch run threads a row.
    public void Decode(ReadOnlySpan<float> source, Span<float> destination) =>
        DispatchTransfer(source, destination, decode: true);
    public void Encode(ReadOnlySpan<float> source, Span<float> destination) =>
        DispatchTransfer(source, destination, decode: false);

    private void DispatchTransfer(ReadOnlySpan<float> source, Span<float> destination, bool decode) =>
        (Identity, decode) switch {
            (true, _) => source.CopyTo(destination),
            (false, true) => ImageProcessing.DecodeTransfer(source, destination, Function!.Value),
            (false, false) => ImageProcessing.EncodeTransfer(source, destination, Function!.Value),
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

    // The ONE crossing predicate: identity always converts, a lane add or drop never does, and the
    // straight-associated crossing admits only above 8 bits — the [04] ToAlpha gate and the set#TEXTURE_SET
    // admission both read THIS row fact, so the 16-bit floor has one spelling.
    public bool Convertible(AlphaMode target, PlaneDepth depth) =>
        target == this || (Carries == target.Carries && depth != PlaneDepth.U8);
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

    public double Unpack(double stored, PlaneDepth depth) => IsSigned && depth.Integer ? (2.0 * stored) - 1.0 : stored;
    public double Pack(double value, PlaneDepth depth) => IsSigned && depth.Integer ? (value + 1.0) * 0.5 : value;
}

// Green polarity. The wire is always `gl`; `dx` is an INGEST record converted once through the filter#PLANE_OP
// Swizzle lane inversion before the plane is keyed.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NormalConvention {
    public static readonly NormalConvention Gl = new("gl", greenSign: 1.0);
    public static readonly NormalConvention Dx = new("dx", greenSign: -1.0);

    public double GreenSign { get; }
    private NormalConvention(string key, double greenSign) : this(key) => GreenSign = greenSign;
}

// MipPolicy rows carry the level fold. Filter is the separable downsample every row shares; Renormalize and Coupled
// are the two post-folds making a vector chain and a roughness chain correct rather than merely smaller. Paired reads the coupling need.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MipPolicy {
    public static readonly MipPolicy Box = new("box", ResizeFilter.Box, renormalize: false, coupled: false, chains: true);
    public static readonly MipPolicy Kaiser = new("kaiser", ResizeFilter.Mitchell, renormalize: false, coupled: false, chains: true);
    public static readonly MipPolicy NormalRenormalize = new("normalRenormalize", ResizeFilter.Box, renormalize: true, coupled: false, chains: true);
    public static readonly MipPolicy RoughnessVariance = new("roughnessVariance", ResizeFilter.Box, renormalize: false, coupled: true, chains: true);
    public static readonly MipPolicy None = new("none", ResizeFilter.Box, renormalize: false, coupled: false, chains: false);

    public ResizeFilter Filter { get; }
    public bool Renormalize { get; }
    public bool Coupled { get; }
    public bool Chains { get; }
    private MipPolicy(string key, ResizeFilter filter, bool renormalize, bool coupled, bool chains) : this(key) =>
        (Filter, Renormalize, Coupled, Chains) = (filter, renormalize, coupled, chains);
}
```

## [03]-[PLANE_FORMAT]

- Owner: `IComponent<T>` the static-abstract component witness; `ITexel<TSelf>` the static-abstract texel contract; `Texel1`/`Texel2`/`Texel4` the three storage arities; `PlaneFormat` the ten-row storage roster.
- Law: the ten storage rows are the CROSS PRODUCT of three arities and four component witnesses, applied as type arguments. Ten hand-written texel records are the deleted form: they share one lane-projection law, so a new depth is one `IComponent` witness and a new arity is one struct, and the roster grows by type application rather than by transcription.
- Law: `IComponent<T>.ToUnit`/`FromUnit` normalize an INTEGER component onto `[0,1]` and pass a floating component verbatim, saturating on the write side so a fold that overshoots stores the clamped value rather than a wrapped one. This is the only normalization in the estate; a kernel dividing by `255.0` or `65535.0` re-derives it.
- Law: a three-component semantic channel resolves to the FOUR-component storage row declaring `AlphaMode.None`. No odd-width texel exists, so a padded lane is a structural fact the association declares rather than a per-format special case, and `For` rounds a semantic count up through `{1, 2, 4}` — a two-lane channel at a floating depth resolves to the four-lane row because no two-lane float storage row exists.
- Entry: `PlaneFormat.For(int semanticComponents, PlaneDepth depth)` is the ONE resolution both the press binding and the neural stage read; `Items` is the ordered roster; `Get`/`TryGet` resolve a wire key.
- Packages: CommunityToolkit.HighPerformance (`MemoryOwner<T>.Allocate(int, AllocationMode)` the pooled rental every row's `Rent` column binds, `Memory<T>.AsMemory2D(int, int)` the plane projection), Thinktecture.Runtime.Extensions, BCL inbox (`Half`, `double.Clamp`, `Array.MaxLength`).
- Growth: a new depth is one `IComponent` witness with its rows; a new arity is one texel struct with its rows; a new storage row is one `PlaneFormat` declaration naming its arity, witness, component count, depth, and alpha. Nothing else on this page, in the codec, in the filter, or in the pyramid changes — the `Rent` column carries the type application and every consumer stays generic over `ITexel`.

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

// PlaneFormat rows roster storage. Rent is the type application — the ONE column that erases the texel type into the
// arena, so every surface above this page is generic over ITexel and none of them enumerates a format.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PlaneFormat {
    public static readonly PlaneFormat R8 = Row("r8", 1, PlaneDepth.U8, AlphaMode.None, PlaneStore.Rent<Texel1<byte, U8>>);
    public static readonly PlaneFormat R16 = Row("r16", 1, PlaneDepth.U16, AlphaMode.None, PlaneStore.Rent<Texel1<ushort, U16>>);
    public static readonly PlaneFormat R16F = Row("r16f", 1, PlaneDepth.F16, AlphaMode.None, PlaneStore.Rent<Texel1<Half, F16>>);
    public static readonly PlaneFormat R32F = Row("r32f", 1, PlaneDepth.F32, AlphaMode.None, PlaneStore.Rent<Texel1<float, F32>>);
    public static readonly PlaneFormat Rg8 = Row("rg8", 2, PlaneDepth.U8, AlphaMode.None, PlaneStore.Rent<Texel2<byte, U8>>);
    public static readonly PlaneFormat Rg16 = Row("rg16", 2, PlaneDepth.U16, AlphaMode.None, PlaneStore.Rent<Texel2<ushort, U16>>);
    public static readonly PlaneFormat Rgba8 = Row("rgba8", 4, PlaneDepth.U8, AlphaMode.Straight, PlaneStore.Rent<Texel4<byte, U8>>);
    public static readonly PlaneFormat Rgba16 = Row("rgba16", 4, PlaneDepth.U16, AlphaMode.Straight, PlaneStore.Rent<Texel4<ushort, U16>>);
    public static readonly PlaneFormat Rgba16F = Row("rgba16f", 4, PlaneDepth.F16, AlphaMode.Straight, PlaneStore.Rent<Texel4<Half, F16>>);
    public static readonly PlaneFormat Rgba32F = Row("rgba32f", 4, PlaneDepth.F32, AlphaMode.Straight, PlaneStore.Rent<Texel4<float, F32>>);

    public int Components { get; }
    public PlaneDepth Depth { get; }
    public AlphaMode Alpha { get; }
    public Func<int, int, AllocationMode, PlaneStore> Rent { get; }
    public long BytesPerTexel => (long)Components * Depth.Bytes;

    private PlaneFormat(string key, int components, PlaneDepth depth, AlphaMode alpha, Func<int, int, AllocationMode, PlaneStore> rent)
        : this(key) => (Components, Depth, Alpha, Rent) = (components, depth, alpha, rent);
    private static PlaneFormat Row(string key, int components, PlaneDepth depth, AlphaMode alpha, Func<int, int, AllocationMode, PlaneStore> rent) =>
        new(key, components, depth, alpha, rent);

    // Storage width rounds a SEMANTIC count up through {1, 2, 4}: a three-lane channel takes the four-lane row and
    // declares AlphaMode.None, and a two-lane channel at a floating depth takes it too because no two-lane float row
    // exists. The projection is total over the roster, so an absent pair is a typed absence rather than a fabrication.
    public static Option<PlaneFormat> For(int semanticComponents, PlaneDepth depth) =>
        Items.Where(row => row.Depth == depth && row.Components >= Math.Max(1, semanticComponents))
             .OrderBy(static row => row.Components)
             .HeadOrNone();
}
```

## [04]-[TEXTURE_PLANE]

- Owner: `PlaneStore` the arena base with its `IPlaneFold` re-entry seam; `PlaneStore<T>` the ONE generic realization; `TexturePlane` the admitted plane carrying format, extent, layers, transfer, association, range, and store.
- Entry: `TexturePlane.Of(format, width, height, transfer, alpha, key, layers, range)` is the one admission; `Read(row, layer, lanes)`/`Write(row, layer, lanes)` are the one decoded LANE row rail and `ReadShade(row, layer, texels)`/`WriteShade(row, layer, texels)` its `ShadeVec4` projection — the tile, set, press, and environment folds all stage `ShadeVec4` rows, so the lane-to-register correspondence (single-lane replication, alpha seat, four-lane identity) is declared ONCE here rather than re-derived per consumer; `RowScalars` sizes a consumer's lane scratch; `Layer(index)` windows one layer; `ToAlpha(target, key)` is the one association conversion; `Key` is the streaming content key.
- Law: a ten-case store union is the DELETED form. Ten cases carried one field pair and one disposal, so the arena is one generic record and typed code re-enters through `Accept<TFold, TResult>` — a `struct` or `ref struct` fold the JIT specializes per texel, allocating nothing and capturing nothing. `PlaneFormat` rows carry their own `Rent` column, deleting the `format.Key` switch that throws on an unmatched row: an unmatched format is unrepresentable rather than an exception in a fallible path.
- Law: `Of` refuses BEFORE it rents. `MaterialFault.Parameter` rails a non-positive extent or layer count, an element count above `Array.MaxLength`, and an association the storage row cannot hold, each carrying the offending axis in its reason. `width × height × layers` texels bound admission and make the typed arena worth its shape: a 16k four-lane 16-bit plane counts 268 435 456 and admits, while the same plane's byte count exceeds the runtime bound.
- Law: layer `n` occupies rows `[n × height, (n + 1) × height)` of one arena. `Layer` windows that band without a second rental, so a cube face set, an array slice, a volume slab, and a flipbook frame are all one plane and the `set#TEXTURE_SET` `LayerLaw` row is the only thing that names which.
- Law: `Read` runs ONE decode ladder and `Write` runs its exact inverse — texel lanes, integer normalization, `PlaneRange` unpack, alpha un-association, transfer decode over the colour lanes alone. Every consumer above this page reads decoded, signed, scene-linear lanes, so no kernel in the estate re-derives a curve, re-divides by a maximum, or re-packs a signed field. `Read` leaves the alpha lane untouched by every transfer: an association is a linear coverage weight, and running a display curve over it darkens every edge.
- Law: `ToAlpha` converts association on DECODED lanes and refuses the `straight`↔`associated` crossing below 16 bits. At eight bits the un-association divides by a quantized coverage, so a low-alpha texel amplifies its own quantization step into a visible colour error the round trip cannot recover.
- Law: the content key is the kernel `ContentHash.Of` streaming entry over the plane's own storage rows in layer-major, row-major order, seeded zero like every other federation key. `ContentHash.Of` is the sole mint site, holding the federation seed and the cross-branch digest reproduction; the whole-plane byte span is never materialized, so a 268-million-texel plane keys in one pooled row window.
- Packages: CommunityToolkit.HighPerformance (`MemoryOwner<T>.Allocate`/`.Memory`/`.Dispose`, `AllocationMode.Clear`/`Default`, `Memory<T>.AsMemory2D(int, int)`, `Memory2D<T>.Span`, `Memory2D<T>.Slice(int, int, int, int)`, `Span2D<T>.GetRowSpan(int)`, `SpanOwner<T>.Allocate` the per-row lane scratch), `Rasm.Domain` (`ContentHash.Of<TState>(TState, Action<TState, XxHash128>)` the ONE identity entry, `Op`), `Rasm.Numerics` (`Dimension`), `bsdf#SHADING_FRAME` (`MaterialFault` band 2450), `System.IO.Hashing` (`XxHash128.Append` inside the kernel entry alone), BCL inbox (`Array.MaxLength`, `MemoryMarshal.AsBytes`).
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
using Rasm.Numerics;                              // Dimension
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
        return new PlaneStore<T>(owner, owner.Memory.AsMemory2D(rows, width));
    }
}

public sealed record PlaneStore<T>(MemoryOwner<T> Owner, Memory2D<T> View) : PlaneStore
    where T : unmanaged, ITexel<T> {
    public override int Lanes => T.Lanes;
    public override void Dispose() => Owner.Dispose();
    public override TResult Accept<TFold, TResult>(scoped in TFold fold) => fold.Fold(View);
    // Window SHARES the rental — the layer band is a view, never a copy — so disposal stays with the owning plane.
    public override PlaneStore Window(int rowOffset, int rows) =>
        new PlaneStore<T>(Owner, View.Slice(rowOffset, 0, rows, View.Width));
}

// TexturePlane admits the plane. Layers extends the arena by band rather than by a second dimension, so cube faces,
// array slices, volume slabs, and flipbook frames are ONE shape and set#TEXTURE_SET's LayerLaw row names which.
public sealed record TexturePlane(
    PlaneFormat Format,
    Dimension Width,
    Dimension Height,
    Dimension Layers,
    PlaneTransfer Transfer,
    AlphaMode Alpha,
    PlaneRange Range,
    PlaneStore Store) : IDisposable {

    // Dimension.Create(1) is the page's ONE total construction: the literal is statically inside the >=1 domain, so the
    // generated throw is unreachable and this is the named [EXPRESSION_SPINE] admission exemption rather than a rail.
    private static readonly Dimension Single = Dimension.Create(value: 1);

    public static Fin<TexturePlane> Of(
        PlaneFormat format, Dimension width, Dimension height, PlaneTransfer transfer, AlphaMode alpha, Op key,
        Option<Dimension> layers = default, Option<PlaneRange> range = default, AllocationMode mode = AllocationMode.Clear) {
        Dimension layerCount = layers.IfNone(Single);
        long rows = (long)height.Value * layerCount.Value;
        long elements = (long)width.Value * rows;
        // A padded lane is legal — AlphaMode.None over a four-lane row is the [03] three-component law — so the
        // storage gate refuses only an alpha the row cannot HOLD, never a declaration narrower than the row.
        return (width.Value, height.Value, layerCount.Value, elements, !alpha.Carries || format.Alpha.Carries) switch {
            ( <= 0, _, _, _, _) or (_, <= 0, _, _, _) or (_, _, <= 0, _, _) =>
                MaterialFault.Parameter(key, $"<plane-extent:{width.Value}x{height.Value}x{layerCount.Value}>"),
            (_, _, _, > Array.MaxLength, _) =>
                MaterialFault.Parameter(key, $"<plane-elements:{elements}>"),
            (_, _, _, _, false) =>
                MaterialFault.Parameter(key, $"<plane-alpha-storage:{alpha.Key}!={format.Alpha.Key}>"),
            _ => Fin.Succ(new TexturePlane(format, width, height, layerCount, transfer, alpha,
                     range.IfNone(PlaneRange.Unit), format.Rent(width.Value, checked((int)rows), mode))),
        };
    }

    public int Lanes => Store.Lanes;
    public int RowScalars => Width.Value * Lanes;
    public long Texels => (long)Width.Value * Height.Value * Layers.Value;

    // One layer as a plane of its own over the SHARED rental, so a cube face folds its own pyramid and lifts its own
    // sampler without a copy. Disposal stays with this plane — a windowed layer never owns the arena.
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

    // The ShadeVec4 projection of the decoded row rail — ONE lane-to-register correspondence for every consumer
    // staging four-lane rows: a single lane replicates across XYZ, a two-lane pair fills X and Y, four lanes map
    // straight through, and the alpha register reads the coverage lane where the plane carries one, else 1.0.
    // WriteShade inverts it, so the tile fold, the press staging, the set mean, and the sky sweep never re-derive
    // a lane seat and a plane's arity change breaks HERE rather than in five consumers.
    public void ReadShade(int row, int layer, Span<ShadeVec4> texels) {
        using SpanOwner<double> lanes = SpanOwner<double>.Allocate(RowScalars);
        Read(row, layer, lanes.Span);
        int stride = Lanes, colour = Alpha.Carries ? stride - 1 : stride;
        for (int x = 0; x < Width.Value; x++) {
            ReadOnlySpan<double> texel = lanes.Span.Slice(x * stride, stride);
            texels[x] = new ShadeVec4(
                texel[0],
                colour > 1 ? texel[1] : texel[0],
                colour > 2 ? texel[2] : texel[0],
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
        : Of(Format, Width, Height, Transfer, target, key, Some(Layers), Some(Range), AllocationMode.Default)
              .Map(Reassociate);

    // Association converts THROUGH the decode ladder: the source reads straight, un-associated lanes and the
    // destination's own AlphaMode re-applies coverage on write, so the conversion is the ladder's inverse pair and
    // never a second premultiply spelling. The row walk is the page's [EXPRESSION_SPINE] kernel exemption.
    private TexturePlane Reassociate(TexturePlane destination) {
        using SpanOwner<double> lanes = SpanOwner<double>.Allocate(Width.Value * Lanes);
        for (int layer = 0; layer < Layers.Value; layer++) {
            for (int row = 0; row < Height.Value; row++) {
                Read(row, layer, lanes.Span);
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
// re-keys and a declaration retag does NOT — transfer, range, and association ride the wire's channel row, never
// the blob preimage, which is what lets ingest re-declare a decoded container per role without re-addressing it.
internal readonly struct KeyRows(XxHash128 hash) : IPlaneFold<Unit> {
    public Unit Fold<T>(Memory2D<T> view) where T : unmanaged, ITexel<T> {
        for (int row = 0; row < view.Height; row++) { hash.Append(MemoryMarshal.AsBytes(view.Span.GetRowSpan(row))); }
        return Unit.Default;
    }
}
```

## [05]-[TEXTURE_PYRAMID]

- Owner: `TexturePyramid` the ordered level chain, its `MipPolicy` row, its coupling evidence, and the one sampler bridge.
- Entry: `TexturePyramid.Of(TexturePlane basePlane, MipPolicy policy, Op key, Option<TexturePyramid> paired = default)` BUILDS the chain — one entry over both the single-level and the full-chain modalities, discriminating on the policy row rather than on a caller flag; `Base`, `Levels`, `Coupled`, `Key`, and `AsImage(key)` are the reads.
- Law: `MipPolicy.None` admits exactly one level and the pyramid ADOPTS the base plane rather than copying it. Every chaining row halves each spatial extent with a floor of one and stops at `1×1`, so the level count is `⌊log2(max(width, height))⌋ + 1` and no caller supplies it.
- Law: every fold runs in the LINEAR domain over decoded rows. Each level decodes through `TexturePlane.Read`, resamples through the policy's separable filter, and re-encodes through `TexturePlane.Write`, so an `srgb` chain does not darken and a `signed` chain does not drift toward its packing midpoint.
- Law: `NormalRenormalize` unit-normalizes the leading three lanes after the fold, because averaging unit vectors shortens them and a shortened normal reads as a tilted one at every distance.
- Law: `RoughnessVariance` is PAIRED and its coupling is quantitative: the paired normal level's mean-vector length `L` carries the directional variance the fold destroyed, so the level's roughness becomes `min(1, sqrt(r² + 2(1 − L)/L))` and specular aliasing does not reappear at distance. `RoughnessVariance` admits a build carrying no paired chain — the row folds under `Box` and records `Coupled: false`, which `press#PRESS_RECEIPT` publishes as the declared quality floor, because a set whose normal chain has not yet been tiled must still produce a roughness chain.
- Law: `AsImage` MATERIALIZES the chain into the sampler's own carrier — each level's decoded lanes projected into `ShadeVec4` and admitted through `TextureSource.Image.Of` — so the estate mints no second sampler, no second reconstruction, and no second address mode. `TextureSource.Image` carries one layer, so the lift runs PER LAYER by construction: a multi-layer plane refuses and the caller extracts a `Layer` first, which is exactly what makes the cube-face and array arms honest rather than declared capability that cannot run.
- Packages: `plane#TEXTURE_PLANE` (composed — `TexturePlane.Of`/`Read`/`Write`/`Layer`/`Key`, `PlaneFormat`, `MipPolicy`), `texture#TEXTURE_UV` (composed — `TextureSource.Image.Of(Dimension, Dimension, Seq<ReadOnlyMemory<ShadeVec4>>, Op)` the ONE sampler admission, `ShadeVec4` the four-lane field register), TinyEXR.NET (composed — `ImageProcessing.Resize(ReadOnlySpan<float>, int, int, Span<float>, int, int, int, ResizeFilter, EdgeMode, int)` the separable downsample, `EdgeMode.Clamp` at a chain boundary), CommunityToolkit.HighPerformance (`SpanOwner<T>.Allocate` the per-level staging), `Rasm.Domain` (`ContentHash.Of`, `Op`), LanguageExt.Core.
- Growth: a new fold law is one `MipPolicy` row carrying its filter and its post-fold flags; a new coupling is one flag with its arm in the post-fold. Neither the chain walk, the level admission, the sampler bridge, nor any consumer changes.
- Boundary: arbitrary-ratio resampling is `filter#PLANE_OP` `Resize` — a mip level is a halving under a declared policy and never a resize alias, so a chain cannot be minted at an arbitrary ratio and a resize cannot silently produce a level a sampler then trilinearly blends. `TexturePyramid` OWNS its levels and disposes them; a pyramid built over an adopted base at `MipPolicy.None` disposes that base too, so ownership is uniform and a caller never holds a half-owned chain.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Buffers.Binary;
using System.Linq;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using Rasm.Domain;                                // Op, ContentHash
using Rasm.Materials.Appearance.Bsdf;             // MaterialFault
using Rasm.Materials.Appearance.Texture;          // TextureSource, ShadeVec4
using Rasm.Numerics;                              // Dimension
using TinyEXR.V3;                                 // ImageProcessing, EdgeMode
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [MODELS] ------------------------------------------------------------------------------
public sealed record TexturePyramid(Seq<TexturePlane> Levels, MipPolicy Policy, bool Coupled) : IDisposable {
    public TexturePlane Base => Levels.Head;

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
            ? Fin.Succ(new TexturePyramid(Seq1(basePlane), policy, Coupled: false))
            : Chain(basePlane, policy, paired, key);

    private static Fin<TexturePyramid> Chain(TexturePlane basePlane, MipPolicy policy, Option<TexturePyramid> paired, Op key) {
        bool coupled = policy.Coupled && paired.IsSome;
        return Seq(Enumerable.Range(1, LevelCount(basePlane) - 1))
            .Fold(Fin.Succ(Seq1(basePlane)), (state, index) => state.Bind(levels =>
                Fold(levels.Last, policy, coupled ? paired.Bind(chain => Level(chain, index)) : None, key)
                    .Map(levels.Add)))
            .Map(levels => new TexturePyramid(levels, policy, coupled));
    }

    private static int LevelCount(TexturePlane plane) =>
        (int)Math.Log2(Math.Max(plane.Width.Value, plane.Height.Value)) + 1;

    private static Option<TexturePlane> Level(TexturePyramid chain, int index) =>
        index < chain.Levels.Count ? Some(chain.Levels[index]) : None;

    // One level. Decode, separable-resample in the LINEAR domain, apply the row's post-fold, re-encode — so the fold
    // is one body over every format, every transfer, and every arity, parameterized by the policy row alone. Halving
    // an extent >= 1 lands >= 1 by construction, so Dimension.Create's generated throw is statically unreachable and
    // this is the section's named [EXPRESSION_SPINE] admission exemption.
    private static Fin<TexturePlane> Fold(TexturePlane source, MipPolicy policy, Option<TexturePlane> companion, Op key) =>
        TexturePlane.Of(source.Format, Halve(source.Width), Halve(source.Height), source.Transfer, source.Alpha, key,
                Some(source.Layers), Some(source.Range), AllocationMode.Default)
            .Map(level => Resample(source, level, policy, companion));

    private static Dimension Halve(Dimension extent) => Dimension.Create(value: Math.Max(1, extent.Value >> 1));

    // Resample runs the separable downsample with the row's post-fold over DECODED lanes. Renormalize restores the unit
    // length an averaged vector loses; the coupled arm folds the companion level's mean-vector length L into the roughness
    // lane as sqrt(r^2 + 2(1-L)/L), so the directional variance the normal chain destroyed reappears as roughness rather
    // than as specular aliasing at distance. The walk is the section's [EXPRESSION_SPINE] kernel exemption.
    private static TexturePlane Resample(TexturePlane source, TexturePlane level, MipPolicy policy, Option<TexturePlane> companion) {
        int lanes = source.Lanes, sw = source.Width.Value, dw = level.Width.Value;
        using SpanOwner<float> src = SpanOwner<float>.Allocate(sw * source.Height.Value * lanes);
        using SpanOwner<float> dst = SpanOwner<float>.Allocate(dw * level.Height.Value * lanes);
        using SpanOwner<double> row = SpanOwner<double>.Allocate(Math.Max(sw, dw) * lanes);
        using SpanOwner<double> pair = SpanOwner<double>.Allocate(dw * lanes);
        for (int layer = 0; layer < source.Layers.Value; layer++) {
            for (int y = 0; y < source.Height.Value; y++) {
                source.Read(y, layer, row.Span);
                for (int i = 0; i < sw * lanes; i++) { src.Span[(y * sw * lanes) + i] = (float)row.Span[i]; }
            }
            ImageProcessing.Resize(src.Span, sw, source.Height.Value, dst.Span, dw, level.Height.Value, lanes,
                policy.Filter, EdgeMode.Clamp, source.Alpha.Carries ? lanes - 1 : -1);
            for (int y = 0; y < level.Height.Value; y++) {
                for (int i = 0; i < dw * lanes; i++) { row.Span[i] = dst.Span[(y * dw * lanes) + i]; }
                if (policy.Renormalize) { Renormalize(row.Span, dw, lanes); }
                companion.Iter(normal => { normal.Read(y, layer, pair.Span); Couple(row.Span, pair.Span, dw, lanes, normal.Lanes); });
                level.Write(y, layer, row.Span);
            }
        }
        return level;
    }

    private static void Renormalize(Span<double> row, int width, int lanes) {
        for (int x = 0; x < width; x++) {
            Span<double> texel = row.Slice(x * lanes, lanes);
            double length = Math.Sqrt((texel[0] * texel[0]) + (texel[1] * texel[1]) + (texel[2] * texel[2]));
            if (length > 0.0) { texel[0] /= length; texel[1] /= length; texel[2] /= length; }
        }
    }

    private static void Couple(Span<double> row, ReadOnlySpan<double> normal, int width, int lanes, int normalLanes) {
        for (int x = 0; x < width; x++) {
            ReadOnlySpan<double> n = normal.Slice(x * normalLanes, normalLanes);
            double length = Math.Sqrt((n[0] * n[0]) + (n[1] * n[1]) + (n[2] * n[2]));
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

    // The lane-to-register correspondence is the plane's own ReadShade rail, so the bridge stages rows and owns
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

## [06]-[RESEARCH]

- [KAISER_WINDOW]-[OPEN]: `MipPolicy.Kaiser` binds `ResizeFilter.Mitchell`, the widest separable row `TinyEXR.V3.ImageProcessing.Resize` ships — the package's filter roster is `Box`, `Triangle`, `CatmullRom`, `Mitchell` and carries no windowed-sinc row. Verify whether a Kaiser-windowed sinc measurably beats Mitchell on a colour chain at the extents this estate bakes before any local separable kernel is authored; the frozen `kaiser` key is unaffected either way, and the answer decides whether the row keeps its package binding or earns a kernel.
- [SPAN2D_ROW_ORIGIN]-[OPEN]: `Memory2D<T>.Slice(row, column, height, width)` is the layer window this page takes, and `Span2D<T>.GetRowSpan(int)` is asserted to address the SLICED origin rather than the underlying rental's. Verify against the installed `CommunityToolkit.HighPerformance` implementation; a rental-relative row index offsets every windowed layer read by the window's own origin, which no gate downstream can see.
