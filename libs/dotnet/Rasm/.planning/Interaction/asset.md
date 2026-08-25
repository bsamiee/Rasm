# [RASM_ASSET]

`Rasm.Interaction` owns the one icon and asset origin family. An asset is WHERE its bytes come from — an embedded resource, a file, a stream, a scale-indexed raster set, a vector key, a block of source text a host compiles, or a draw program — and every host surface that shows an icon reads that family rather than minting its own. Pose and filter are the two orthogonal axes an icon carries beside its origin, so a rotated disabled tinted glyph is one value with an ordered filter chain rather than a name with three suffixes.

Both boundaries carried an origin family and an icon record; the AppUi product surface carried a third. All three delete: the origin cases are the union of what each side actually spelled, the host-typed payloads stay ROWS at the boundary rather than becoming cases the kernel cannot name, and the extent vocabulary is one value both sides read. The raster carrier admits every product shape the branch actually consumes, because a Rhino plug-in registry entry and a Zoo licence badge answer GDI bitmaps, every Eto surface answers toolkit bitmaps, and a host display-bitmap upload answers raw straight-alpha rows — one arm serving all three would force a conversion the kernel has no business owning, so the shape is asked for at the resolve instead.

## [01]-[INDEX]

- [02]-[ORIGIN]: `AssetKey`, `FileLocation`, `AssetAnchor`, `AssetExtent`, `RasterStack`, `AssetRaster`, `AssetOrigin` — the origin family, its addressing vocabulary, the asked product shape, and the three-shape raster carrier.
- [03]-[POSE]: `IconPose`, `MirrorAxis`, `IconFilter` — the orientation and rendering-state axes an origin carries.
- [04]-[RENDER]: `IconRender` — the composed request one host surface resolves, carrying its ordered filter chain.

## [02]-[ORIGIN]

- Owner: `AssetKey` the logical asset name; `FileLocation` the admitted path a file origin reads; `AssetAnchor` the assembly-plus-resource-path pair an embedded lookup needs; `AssetExtent` the pixel extent an origin is asked for beside its allocation ceiling; `RasterStack` the product shape a caller asks for; `AssetRaster` one scale-indexed raster in one product shape; `AssetOrigin` the closed origin family.
- Cases: `Resource(AssetAnchor)` reads an embedded manifest resource; `File(FileLocation)` reads a path; `Stream(Func<Stream>)` opens a caller-supplied byte source; `Raster(Seq<AssetRaster>)` selects from a scale-indexed set; `Vector(AssetKey)` names a scalable source the host resolves; `Source(string)` carries the source TEXT a host compiler turns into a scalable drawing; `Render(Func<AssetExtent, Fin<PaintProgram>>)` draws the asset at the asked extent through the paint program.
- Cases: `AssetRaster` is `Toolkit` over an Eto bitmap, `Gdi` over a `System.Drawing` bitmap, or `Pixels` over tightly packed rows under the `AlphaLayout` carriage they were read at — the product SHAPE is the discriminant, unrecoverable from a payload the kernel would otherwise have to probe. Rhino's plug-in icon registry and its licence badge take GDI bitmaps, every Eto surface takes toolkit bitmaps, and a host display-bitmap upload takes raw rows; the three are different host contracts, not three spellings of one.
- Entry: `AssetOrigin.Resolve(extent, stack, key)` answers the raster the origin produced at the asked extent in the asked product shape — a decode, a set selection, a host vector or source-text rasterization, or a replay of the draw program. The `Pixels` ask reads its rows through `Interaction/paint`'s `PixelLease.Bytes`, so one lock-and-copy owner serves this resolve and every other pixel egress.
- Auto: the scale is one column read through a total fold, so a selection over a set compares one value and no consumer switches to find the number it is comparing.
- Law: every raster carrying a HOST HANDLE carries it as a `Lease`, so a decoded image, a host-owned registry entry, and a caller-supplied bitmap all state their custody rather than leaving the resolver's answer ambiguous. A resolved asset the caller must remember not to dispose is exactly the leak both boundaries carried. The `Pixels` case is the one arm with no lease and states why: managed rows are already the caller's own copy, and wrapping a buffer in a custody the runtime settles would name an owner with nothing to release.
- Law: the lease-free arm is the one arm whose payload can DISAGREE with its own picture, so `OfPixels` is its only mint and admits `rows.Count == extent.PixelCount * layout.Channels`. A public constructor there takes any buffer beside any extent under any carriage, and each disagreement surfaces as a read past the end at whichever host consumes it — the coverage carriage is a COLUMN for the same reason, because a premultiplied buffer read as straight corrupts silently rather than refusing.
- Law: host-typed payloads are ROWS at the boundary, never cases here — a `Rhino.UI` bitmap table entry, a Grasshopper2 canvas glyph, and a platform image list are each ONE `Resource` or `Raster` value the boundary constructs, so the kernel family stays nameable without any host assembly it does not reference.
- Law: `Render` is the escape that keeps the family closed — an asset no byte source produces is a DRAW, and the draw is the paint program this sub-domain already owns, so no further origin case is ever needed for a procedurally generated icon. `Source` is not that escape and does not overlap it: the text is compiled by a HOST whose dialect the kernel never names, so an origin the kernel itself can draw is a `Render` and an origin only a host compiler reads is a `Source`.
- Law: `Stream` opens its source EXACTLY ONCE per resolve. The factory hands a live handle the decode consumes, so a second invocation mints a second stream whose disposal is that caller's alone — a resolver calling the factory twice would hand one materialization two half-read handles, which is precisely what a re-openable path origin (`File`) exists to express instead.
- Law: the product shape is ASKED, never chosen. `Resolve` takes the `RasterStack` its caller's host contract demands and the answered case is always that row; an origin that cannot answer the asked shape refuses TYPED. A resolver silently converting between imaging stacks would own exactly the conversion this page's raster split exists to refuse.
- Law: the scale-indexed set carries its scale as DATA and the selection reads the asked extent — a `@2x` naming convention parsed out of a path is the deleted form, because a convention lives in the reader and a column lives in the value.
- Law: the extent carries its own allocation CEILING and every pixel figure is overflow-safe. `MaxDimension` is a REQUIRED column seeded from the declared `AssetExtent.Ceiling` row — the tightest maximum raster edge the admitted imaging stacks publish — so an absent ask reads a bound some surface actually allocates under rather than a fabricated `int.MaxValue`; both scaled edges are measured in `double` against it and `PixelCount` answers in `long`, because two admitted edges under a retina scale wrap an `int` product long before either edge reads as unreasonable, and a wrapped count reads back as a small buffer the decode then writes past.
- Law: an extent past its ceiling REFUSES at `Of` and no read clamps. A saturating scaled edge fabricates a picture the caller never asked for, and the fabricated edge then sizes the buffer every consumer below trusts.
- Auto: `AssetExtent` derives its scaled pixel extent from the logical extent and the surface scale, so no consumer multiplies.
- Receipt: none — an origin resolves to a raster and refuses typed; the raster itself is the evidence.
- Packages: Eto.Drawing for the toolkit bitmap and `System.Drawing.Common` for the GDI bitmap (both prelude-aliased); LanguageExt.Core for the rails, `Seq`, and the packed `Arr<byte>` rows; `System.Reflection` for the embedded-resource anchor; `System.Buffers` for the frozen refused-glyph set the path admission reads. Composed inside the sub-domain: `AlphaLayout` from `Interaction/paint`, `FieldTag` from `Interaction/control`, and `UiFault`/`RejectReason` from `Interaction/dispatch`.
- Growth: a new byte source is one case, breaking every resolve site loudly; a new product shape is one raster case beside one `RasterStack` row; a new scale in a set is one row; a backend publishing a tighter raster edge is one ceiling row.
- Boundary: the kernel never CACHES a resolved asset — a host image cache, a `DisplayBitmap` table, and a platform image list are the boundary's own custody, because their eviction policy is the host's and a kernel cache would outlive the surface that asked.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Buffers;
using System.Reflection;
using EtoBitmap = Eto.Drawing.Bitmap;
using GdiBitmap = System.Drawing.Bitmap;
using Rasm.Domain;
using Rasm.Numerics;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct AssetKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim().ToLowerInvariant() ?? string.Empty;
        validationError = value.Length > 0
            && value.All(static ch => char.IsAsciiLetterLower(ch) || char.IsAsciiDigit(ch) || ch is '.' or '-' or '_')
            ? null
            : new ValidationError(message: $"AssetKey requires a non-empty dotted lowercase token: {value}");
    }
}

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct FileLocation {
    private static readonly SearchValues<char> Refused = SearchValues.Create(string.Create(
        length: 36,
        state: unit,
        action: static (span, _) => {
            for (int code = 0; code < 32; code++) { span[code] = (char)code; }
            "\"<>|".CopyTo(span[32..]);
        }));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0 && !value.AsSpan().ContainsAny(Refused)
            ? null
            : new ValidationError(message: $"FileLocation requires an admitted path: {value}");
    }
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record AssetAnchor(Assembly Owner, string ResourcePath);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct AssetExtent(
    Dimension Width, Dimension Height, PositiveMagnitude Scale, Dimension MaxDimension) : IValidityEvidence {
    public static readonly Dimension Ceiling = Dimension.Create(value: 16_384);

    public static Fin<AssetExtent> Of(
        Dimension width, Dimension height, PositiveMagnitude scale, Option<Dimension> max = default, Op? key = null);

    public int PixelWidth => (int)Measured(Width);
    public int PixelHeight => (int)Measured(Height);

    public long PixelCount => (long)PixelWidth * PixelHeight;

    public bool IsValid => ValidityClaim.All(
        Measured(Width) >= 1d,
        Measured(Height) >= 1d,
        Measured(Width) <= MaxDimension.Value,
        Measured(Height) <= MaxDimension.Value);

    private double Measured(Dimension edge) => Math.Round(edge.Value * Scale.Value);
}

[SmartEnum<int>]
public sealed partial class RasterStack {
    public static readonly RasterStack Toolkit = new(key: 0);
    public static readonly RasterStack Gdi = new(key: 1);
    public static readonly RasterStack Pixels = new(key: 2);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AssetRaster {
    private AssetRaster() { }

    public sealed record Toolkit(PositiveMagnitude Scale, Lease<EtoBitmap> Bitmap) : AssetRaster;
    public sealed record Gdi(PositiveMagnitude Scale, Lease<GdiBitmap> Bitmap) : AssetRaster;
    public sealed record Pixels : AssetRaster {
        internal Pixels(PositiveMagnitude scale, AssetExtent extent, AlphaLayout layout, Arr<byte> rows) =>
            (Scale, Extent, Layout, Rows) = (scale, extent, layout, rows);
        public PositiveMagnitude Scale { get; }
        public AssetExtent Extent { get; }
        public AlphaLayout Layout { get; }
        public Arr<byte> Rows { get; }
    }

    public static Fin<AssetRaster> OfPixels(
        PositiveMagnitude scale, AssetExtent extent, AlphaLayout layout, Arr<byte> rows, Op? key = null) =>
        rows.Count == extent.PixelCount * layout.Channels
            ? Fin.Succ<AssetRaster>(new Pixels(scale: scale, extent: extent, layout: layout, rows: rows))
            : Fin.Fail<AssetRaster>(new UiFault.Rejected(
                Key: key.OrDefault(),
                Field: FieldTag.Create(value: nameof(Pixels.Rows)),
                Reason: RejectReason.PackedRows));

    public PositiveMagnitude Scale => Switch(
        toolkit: static raster => raster.Scale,
        gdi:     static raster => raster.Scale,
        pixels:  static raster => raster.Scale);

    public RasterStack Stack => Switch(
        toolkit: static _ => RasterStack.Toolkit,
        gdi:     static _ => RasterStack.Gdi,
        pixels:  static _ => RasterStack.Pixels);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AssetOrigin {
    private AssetOrigin() { }
    public sealed record Resource(AssetAnchor Anchor) : AssetOrigin;
    public sealed record File(FileLocation Location) : AssetOrigin;
    public sealed record Stream(Func<System.IO.Stream> Open) : AssetOrigin;
    public sealed record Raster(Seq<AssetRaster> Scales) : AssetOrigin;
    public sealed record Vector(AssetKey Key) : AssetOrigin;
    public sealed record Source(string Text) : AssetOrigin;
    public sealed record Render(Func<AssetExtent, Fin<PaintProgram>> Draw) : AssetOrigin;

    [BoundaryAdapter] public Fin<AssetRaster> Resolve(AssetExtent extent, RasterStack stack, Op? key = null);
}
```

## [03]-[POSE]

- Owner: `IconPose` the orientation an origin is drawn under; `MirrorAxis` the reflection axis a mirrored pose names; `IconFilter` the closed rendering-state family a host surface asks for, one entry of the chain the render carries.
- Cases: `Disabled`, `Selected`, and `Greyscale` are the payload-free states both boundaries spelled as name suffixes on separate assets; `Tinted` carries the colour it replaces toward; `Fading` carries a colour and the `UnitInterval` strength it blends at; `Custom` carries the map a boundary hands the host for a transform no row names.
- Law: the tint rides its CASE. A colour column beside a filter row is two authorities over one fact and admits the corner where a tint is set under a non-tinting state — a value nothing reads, which the case form makes unspellable rather than refusable. NAMED LOSS: `IconRender.Of`'s pairing admission and its typed refusal both delete. Witness: `IconRender.Of(origin, pose, IconFilter.Tinted, tint: Some(colour), key)` becomes `new IconRender(origin, pose, Seq1<IconFilter>(new IconFilter.Tinted(colour)))`.
- Law: STRENGTH rides the fading case alone, and the two colour-bearing cases are two operations rather than one with an intensity knob — `Tinted` replaces toward its colour and `Fading` blends toward it at a bounded fraction. Folding the strength onto `Tinted` would make `Tinted(colour, 1.0)` and `Fading(colour, 1.0)` two spellings of one draw, and the boundary that carried `Fading(Color, float Strength)` against a bare tint is exactly where the strength was lost. Witness: `Rasm.Grasshopper` `Shell/icons.md`'s `Fading(colour, 0.4f)` becomes `new IconFilter.Fading(colour, UnitInterval.Create(0.4))`.
- Law: an empty CHAIN is the one spelling of unfiltered, so no payload-free `None` row exists. A row meaning "no filter" beside a chain that can simply be empty is a second authority over one fact, and a chain holding `None` beside a real filter reads as an operation the host must then be told to skip. NAMED LOSS: the `"none"` wire key retires with the row; a persisted empty chain carries the same fact with no token at all.
- Law: pose, filter, and tint are ORTHOGONAL axes on one value, never a name product — a `disabled-rotated-icon.png` roster is three axes flattened into a filename space that grows multiplicatively, and the flattening is exactly why both boundaries carried near-duplicate asset sets.
- Law: rotation is a measured angle, not a four-row quadrant vocabulary — a host that only draws quadrants rounds at its own edge and says so, because a kernel that only carries quadrants cannot serve a surface that draws a dial.
- Law: reflection names its AXIS or is absent. A bare mirrored bool leaves a glyph flipped about its vertical centre line and one flipped top-to-bottom sharing one spelling, so the axis rides a row and the unmirrored pose carries no axis at all. NAMED LOSS: none — a boundary that drew one reflection now names which one it drew.
- Law: a filter colour is a `PerceptualColor` the host quantizes at draw — a case carrying a host colour would put a second colour crossing beside the one this sub-domain already owns.
- Law: the filter states its wire key through the DECLARED `[JsonPolymorphic]` roster, projected once at type init, so the serialized discriminator and the rendered token are one fact — a case-to-literal fold beside the roster twins the vocabulary, and an undeclared union serializes `{}` per case. `Custom` states its key and nothing else, because a delegate has no wire form and pretending otherwise is what a stored function pointer would be.
- Packages: `System.Text.Json.Serialization` for the declared `[JsonPolymorphic]`/`[JsonDerivedType]` roster, `System.Reflection` and `System.Collections.Frozen` for the discriminator projection that reads it once at type init; LanguageExt.Core for `Option`; Thinktecture.Runtime.Extensions for the union and the mirror row; `Numerics/atoms` for the angle, colour, and unit-interval carriers.
- Growth: a new rendering state is one `IconFilter` case breaking every filter read loudly, landing its roster row in the same edit; a new orientation coordinate is one column on the pose; a new reflection axis is one `MirrorAxis` row.
- Boundary: the FILTER is declarative and the kernel applies none of it — a host draws the disabled state its platform draws, `Custom` hands that host the map it must apply, and the case names which state was asked for rather than prescribing a pixel operation.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Frozen;
using System.Reflection;
using System.Text.Json.Serialization;
using Rasm.Domain;
using Rasm.Numerics;

namespace Rasm.Interaction;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(Disabled), "disabled")]
[JsonDerivedType(typeof(Selected), "selected")]
[JsonDerivedType(typeof(Greyscale), "greyscale")]
[JsonDerivedType(typeof(Tinted), "tinted")]
[JsonDerivedType(typeof(Fading), "fading")]
[JsonDerivedType(typeof(Custom), "custom")]
public abstract partial record IconFilter {
    private IconFilter() { }

    public sealed record Disabled : IconFilter;
    public sealed record Selected : IconFilter;
    public sealed record Greyscale : IconFilter;
    public sealed record Tinted(PerceptualColor Tint) : IconFilter;
    public sealed record Fading(PerceptualColor Tint, UnitInterval Strength) : IconFilter;
    public sealed record Custom(Func<PerceptualColor, PerceptualColor> Map) : IconFilter;

    public string Key => Discriminators.Value[GetType()];

    private static readonly Lazy<FrozenDictionary<Type, string>> Discriminators =
        new(static () => typeof(IconFilter)
            .GetCustomAttributes<JsonDerivedTypeAttribute>()
            .ToFrozenDictionary(
                static row => row.DerivedType,
                static row => (string)row.TypeDiscriminator!));
}

[SmartEnum<int>]
public sealed partial class MirrorAxis {
    public static readonly MirrorAxis Horizontal = new(key: 0);
    public static readonly MirrorAxis Vertical = new(key: 1);
    public static readonly MirrorAxis Both = new(key: 2);
}

// --- [MODELS] --------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct IconPose(VectorAngle Rotation, Option<MirrorAxis> Mirror, AssetExtent Extent) : IValidityEvidence {
    public static IconPose Upright(AssetExtent extent);
    public bool IsValid => ValidityClaim.All(Extent.IsValid, Band.Angle.Admits(value: Rotation.Value));
}
```

## [04]-[RENDER]

- Owner: `IconRender` — the composed request: an origin, a pose, and the ORDERED filter chain, each entry carrying its own payload.
- Law: the render is a VALUE both boundaries pass whole, constructible without a rail because every invalid pairing is unrepresentable — neither re-spells its columns, and a boundary that needs one more coordinate widens this record rather than wrapping it.
- Law: filter ORDER IS LAW. `Filters` applies head to tail and the sequence is the operation, because greyscale-then-fade and fade-then-greyscale answer different pixels — one desaturates a blended colour, the other blends toward a colour already desaturated. A single filter is a one-element chain and unfiltered is the empty one, so the arity that carried the ordering question is gone rather than answered per host. A host applying the chain in any other order is drawing something the value did not ask for.
- Law: the chain states its wire form as its entries' keys IN ORDER through `Wire`, so a persisted or logged render round-trips the sequence rather than a set — a set-shaped read is the form that silently reorders, which is the same defect as a host reordering the apply.
- Receipt: none — resolution rides `AssetOrigin.Resolve` and its refusal is a `UiFault` case.
- Growth: a new axis is one column every consumer answers; a new filter step is one chain entry no consumer edits.
- Boundary: HOST-SPECIFIC-STAYS — the Rhino bitmap table registration and its `.rui` icon binding, the Grasshopper2 canvas glyph atlas, and the AppUi theme asset cache each keep their own registration and eviction, and each hands this owner a value rather than a name.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
namespace Rasm.Interaction;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record IconRender(AssetOrigin Origin, IconPose Pose, Seq<IconFilter> Filters) {
    public string Wire => string.Join('+', Filters.Map(static filter => filter.Key));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
