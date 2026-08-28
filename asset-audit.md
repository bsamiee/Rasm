# 1. Expose the generated file-path value

From:
`[02]-[ORIGIN]` code fence — `FileLocation` value-object attribute.
```csharp
[ValueObject<string>]
```

To:
```csharp
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
```

Why:
`PanelBadge.Of` reads `FileLocation.Value`, but Thinktecture's default value-object key is private. The owner must publish the key its boundary consumers already use.

Change:
Generate the public `Value` property instead of relying on a nonexistent member or mixed implicit conversions.

Delta:
Net 0 target LOC; no authored symbol or type change.

# 2. Replace the file-path lookup table with a direct predicate

From:
`[02]-[ORIGIN]` code fence — `FileLocation.Refused` and `ValidateFactoryArguments`.
```csharp
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
```

To:
```csharp
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length > 0
            && value.All(static character => !char.IsControl(character) && character is not ('"' or '<' or '>' or '|'))
                ? null
                : new ValidationError(message: "FileLocation requires a non-empty portable path.");
    }
```

Why:
The private table and imperative initializer restate a simple character predicate, add a field, and interpolate rejected path text into an error that needs only the violated rule.

Change:
Inline the same control-character and reserved-character rule in the generated validation hook and use a stable message.

Delta:
Net -8 target LOC and -1 private field; type count unchanged.

# 3. Make asset extents generated admitted values

From:
`[02]-[ORIGIN]` code fence — `AssetExtent` declaration through `Of`.
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct AssetExtent(
    Dimension Width, Dimension Height, PositiveMagnitude Scale, Dimension MaxDimension) : IValidityEvidence {
    public static readonly Dimension Ceiling = Dimension.Create(value: 16_384);

    public static Fin<AssetExtent> Of(
        Dimension width, Dimension height, PositiveMagnitude scale, Option<Dimension> max = default);
```

To:
```csharp
[ComplexValueObject]
public sealed partial class AssetExtent {
    public static readonly Dimension Ceiling = Dimension.Create(value: 16_384);
    public Dimension Width { get; }
    public Dimension Height { get; }
    public PositiveMagnitude Scale { get; }
    public Dimension MaxDimension { get; }
```

Why:
The public positional constructor bypasses `Of`, and the handwritten factory duplicates Thinktecture's generated admission surface. A class also removes the invalid default struct extent while retaining the caller-supplied allocation ceiling.

Change:
Generate the four-field extent owner and keep the package ceiling as the canonical value callers supply when they have no tighter policy.

Delta:
Net +1 target LOC and -1 handwritten public member; type count unchanged.

Ripples:
In `libs/dotnet/Rasm.AppUi/.planning/Theme/assets.md`, lift `AssetExtent.Validate(edge, edge, scale, AssetExtent.Ceiling, out extent)` through `FactoryBridge.Accept<AssetExtent>`. In `libs/dotnet/Rasm.Rhino/.planning/Annotation/style.md`, pass `ceiling.IfNone(AssetExtent.Ceiling)` to the same generated admission instead of calling `AssetExtent.Of`.

# 4. Admit scaled extent bounds once

From:
`[02]-[ORIGIN]` code fence — `AssetExtent` derived dimensions and validity members.
```csharp
    public int PixelWidth => (int)Measured(Width);
    public int PixelHeight => (int)Measured(Height);

    public long PixelCount => (long)PixelWidth * PixelHeight;

    public bool IsValid => ValidityClaim.All(
        Measured(Width) >= 1d,
        Measured(Height) >= 1d,
        Measured(Width) <= MaxDimension.Value,
        Measured(Height) <= MaxDimension.Value);

    private double Measured(Dimension edge) => Math.Round(edge.Value * Scale.Value);
```

To:
```csharp
    static partial void ValidateFactoryArguments(ref ValidationError? validationError,
        ref Dimension width, ref Dimension height, ref PositiveMagnitude scale, ref Dimension maxDimension) {
        double pixelWidth = Math.Round(width.Value * scale.Value);
        double pixelHeight = Math.Round(height.Value * scale.Value);
        validationError = pixelWidth is >= 1d && pixelWidth <= maxDimension.Value
            && pixelHeight is >= 1d && pixelHeight <= maxDimension.Value
                ? null
                : new ValidationError(message: "AssetExtent exceeds its pixel-dimension limit.");
    }

    public int PixelWidth => (int)Math.Round(Width.Value * Scale.Value);
    public int PixelHeight => (int)Math.Round(Height.Value * Scale.Value);
    public long PixelCount => (long)PixelWidth * PixelHeight;
```

Why:
`IsValid` is a second admission path and `Measured` is a trivial wrapper. The generated hook can prove both scaled edges against the retained consumer ceiling before any projection casts or multiplies them.

Change:
Move the cross-field bound into generated construction and leave only direct derived pixel projections.

Delta:
Net +3 target LOC and -1 authored member after replacing `IsValid` and `Measured` with the required validation hook; type count unchanged.

Ripples:
In `libs/dotnet/Rasm.Rhino/.planning/Blocks/model.md`, remove `extent.IsValid` from `PreviewFrame.ValidateFactoryArguments`; `AssetExtent` arrives admitted.

# 5. Remove the duplicated pixel-raster scale

From:
`[02]-[ORIGIN]` code fence — `AssetRaster.Pixels` and `AssetRaster.OfPixels`.
```csharp
    public sealed record Pixels : AssetRaster {
        internal Pixels(PositiveMagnitude scale, AssetExtent extent, AlphaLayout layout, Arr<byte> rows) =>
            (Scale, Extent, Layout, Rows) = (scale, extent, layout, rows);
        public PositiveMagnitude Scale { get; }
        public AssetExtent Extent { get; }
        public AlphaLayout Layout { get; }
        public Arr<byte> Rows { get; }
    }

    public static Fin<AssetRaster> OfPixels(
        PositiveMagnitude scale, AssetExtent extent, AlphaLayout layout, Arr<byte> rows) =>
        rows.Count == extent.PixelCount * layout.Channels
            ? Fin.Succ<AssetRaster>(new Pixels(scale: scale, extent: extent, layout: layout, rows: rows))
            : Fin.Fail<AssetRaster>(new UiFault.Rejected(Field: FieldTag.Create(value: nameof(Pixels.Rows)),
                Reason: RejectReason.PackedRows));
```

To:
```csharp
    public sealed record Pixels : AssetRaster {
        internal Pixels(AssetExtent extent, AlphaLayout layout, Arr<byte> rows) =>
            (Extent, Layout, Rows) = (extent, layout, rows);
        public AssetExtent Extent { get; }
        public AlphaLayout Layout { get; }
        public Arr<byte> Rows { get; }
    }

    public static Fin<AssetRaster> OfPixels(AssetExtent extent, AlphaLayout layout, Arr<byte> rows) =>
        rows.Count == extent.PixelCount * layout.Channels
            ? Fin.Succ<AssetRaster>(new Pixels(extent: extent, layout: layout, rows: rows))
            : Fin.Fail<AssetRaster>(new UiFault.Rejected(Field: FieldTag.Create(value: nameof(Pixels.Rows)),
                Reason: RejectReason.PackedRows));
```

Why:
`AssetExtent.Scale` already identifies the pixel raster's scale. Storing a second value admits disagreement that `OfPixels` never checks.

Change:
Read pixel scale only from the admitted extent and keep the packed-row admission unchanged.

Delta:
Net -2 target LOC and -1 public member; type count unchanged.

Ripples:
In `libs/dotnet/Rasm.Rhino/.planning/HostUi/dialogs.md`, remove `scale: ask.Extent.Scale` from `AssetRaster.OfPixels`.

# 6. Delete raster discriminant forwarding

From:
`[02]-[ORIGIN]` code fence — `AssetRaster.Scale` and `AssetRaster.Stack`.
```csharp
    public PositiveMagnitude Scale => Switch(
        toolkit: static raster => raster.Scale,
        gdi:     static raster => raster.Scale,
        pixels:  static raster => raster.Scale);

    public RasterStack Stack => Switch(
        toolkit: static _ => RasterStack.Toolkit,
        gdi:     static _ => RasterStack.Gdi,
        pixels:  static _ => RasterStack.Pixels);
```

To:
```csharp
// AssetRaster.Scale DELETED
// AssetRaster.Stack DELETED
```

Why:
Neither projection has a consumer after boundary-owned raster selection. `Stack` mirrors the generated case discriminant into another vocabulary, while each scale remains directly available on the matched case.

Change:
Delete both forwarding properties and match `AssetRaster` exhaustively where a boundary needs case-specific data.

Delta:
Net -8 target LOC and -2 public members; type count unchanged.

# 7. Delete the unused raster-output vocabulary

From:
`[02]-[ORIGIN]` code fence — `RasterStack` declaration.
```csharp
[SmartEnum<int>]
public sealed partial class RasterStack {
    public static readonly RasterStack Toolkit = new(key: 0);
    public static readonly RasterStack Gdi = new(key: 1);
    public static readonly RasterStack Pixels = new(key: 2);
}
```

To:
```csharp
// RasterStack DELETED
```

Why:
The requested-output row is used only by the unimplementable kernel resolver. Boundaries already know their required host product and select the corresponding `AssetRaster` case directly.

Change:
Delete the second discriminant vocabulary.

Delta:
Net -6 target LOC, -1 type, and -3 static row members.

Ripples:
In `libs/dotnet/Rasm.AppUi/.planning/Theme/assets.md`, select the nearest `AssetRaster.Pixels` row directly by `Pixels.Extent.Scale` and delete `RasterStack` from the package list and diagram. In `libs/dotnet/Rasm.Grasshopper/.planning/Shell/icons.md`, remove the stale `RasterStack` prose and package reference; `Materialize` already takes `IconDraw`.

# 8. Delete unsupported origin operations

From:
`[02]-[ORIGIN]` code fence — `AssetOrigin.Render` and `AssetOrigin.Resolve`.
```csharp
    public sealed record Render(Func<AssetExtent, Fin<PaintProgram>> Draw) : AssetOrigin;

    public Fin<AssetRaster> Resolve(AssetExtent extent, RasterStack stack);
```

To:
```csharp
// AssetOrigin.Render DELETED
// AssetOrigin.Resolve DELETED
```

Why:
`Resolve` cannot decode resources, compile source text, resolve vector keys, and replay paint programs without capabilities absent from its signature. `Render` has no producer, and every concrete origin consumer rejects it rather than interpreting it.

Change:
Keep origin interpretation at the host boundary and remove the speculative draw case and impossible cross-host member.

Delta:
Net -2 target LOC, -1 nested case type, and -1 public member.

Ripples:
Delete `render` arms in `libs/dotnet/Rasm.AppUi/.planning/Theme/assets.md`, `libs/dotnet/Rasm.Grasshopper/.planning/Shell/icons.md`, `libs/dotnet/Rasm.Rhino/.planning/HostUi/dialogs.md`, and `libs/dotnet/Rasm.Rhino/.planning/HostUi/panels.md`. Replace the AppUi raster `Resolve` call with direct `AssetRaster.Pixels` selection. Remove the kernel-resolution claim from `libs/dotnet/Rasm.Rhino/.planning/Plugin/census.md` and `libs/dotnet/Rasm.Rhino/.planning/HostUi/dialogs.md`.

# 9. Delete the lossy render projection

From:
`[04]-[RENDER]` code fence — `IconRender` declaration.
```csharp
public sealed record IconRender(AssetOrigin Origin, IconPose Pose, Seq<IconFilter> Filters) {
    public string Wire => string.Join('+', Filters.Map(static filter => filter.Key));
}
```

To:
```csharp
public sealed record IconRender(AssetOrigin Origin, IconPose Pose, Seq<IconFilter> Filters);
```

Why:
Case keys omit tint colours, fading strength, and custom-map identity, so `Wire` cannot identify or reconstruct the render it claims to represent.

Change:
Keep the render value and delete its lossy convenience string.

Delta:
Net -2 target LOC and -1 public member; type count unchanged.

Ripples:
Remove the `IconRender.Wire` round-trip claim from `libs/dotnet/Rasm.AppUi/.planning/Theme/assets.md`.

# 10. Delete reflected filter-key forwarding

From:
`[03]-[POSE]` code fence — `IconFilter.Key` and `IconFilter.Discriminators`.
```csharp
    public string Key => Discriminators.Value[GetType()];

    private static readonly Lazy<FrozenDictionary<Type, string>> Discriminators =
        new(static () => typeof(IconFilter)
            .GetCustomAttributes<JsonDerivedTypeAttribute>()
            .ToFrozenDictionary(
                static row => row.DerivedType,
                static row => (string)row.TypeDiscriminator!));
```

To:
```csharp
// IconFilter.Key DELETED
// IconFilter.Discriminators DELETED
```

Why:
Thinktecture's regular-union metadata intentionally publishes no case key. Reflecting serializer attributes recreates a second discriminator surface used only by the deleted lossy render projection.

Change:
Delete the reflection index and dispatch through the generated exhaustive `Switch` at behavior sites.

Delta:
Net -8 target LOC and -2 members; type count unchanged.

# 11. Delete the unused filter wire roster

From:
`[03]-[POSE]` code fence — `IconFilter` serialization attributes.
```csharp
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(Disabled), "disabled")]
[JsonDerivedType(typeof(Selected), "selected")]
[JsonDerivedType(typeof(Greyscale), "greyscale")]
[JsonDerivedType(typeof(Tinted), "tinted")]
[JsonDerivedType(typeof(Fading), "fading")]
[JsonDerivedType(typeof(Custom), "custom")]
```

To:
```csharp
// IconFilter serialization attributes DELETED
```

Why:
No wire consumer exists, a bare `Seq<IconFilter>` has no read path, and the delegate-bearing `Custom` case has no serializable identity. Codec metadata belongs on a boundary DTO or converter when a real protocol exists.

Change:
Remove the unconsumed System.Text.Json protocol from the interior union while retaining every supported process-local filter case.

Delta:
Net -7 target LOC; no symbol or type change.

# 12. Delete obsolete filter serialization imports

From:
`[03]-[POSE]` code fence — imports used only by filter serialization and discriminator reflection.
```csharp
using System.Collections.Frozen;
using System.Reflection;
using System.Text.Json.Serialization;
```

To:
```csharp
// IconFilter serialization imports DELETED
```

Why:
No remaining pose member declares or reflects a serializer contract.

Change:
Delete the three unused imports.

Delta:
Net -3 target LOC; no symbol or type change.

# 13. Delete the rejected selected filter

From:
`[03]-[POSE]` code fence — `IconFilter.Selected` case.
```csharp
    public sealed record Selected : IconFilter;
```

To:
```csharp
// IconFilter.Selected DELETED
```

Why:
Both filter interpreters reject `Selected`, and no producer constructs it. Selection is already expressed by the AppUi paint-role election, so this case provides no render capability.

Change:
Delete the unsupported case while retaining `Custom`, which both interpreters can execute as a process-local filter.

Delta:
Net -1 target LOC and -1 nested case type.

Ripples:
Delete the `selected` failure arms in `libs/dotnet/Rasm.AppUi/.planning/Theme/assets.md` and `libs/dotnet/Rasm.Grasshopper/.planning/Shell/icons.md`.

# 14. Collapse icon pose to an immutable product

From:
`[03]-[POSE]` code fence — `IconPose` declaration.
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct IconPose(VectorAngle Rotation, Option<MirrorAxis> Mirror, AssetExtent Extent) : IValidityEvidence {
    public static IconPose Upright(AssetExtent extent);
    public bool IsValid => ValidityClaim.All(Extent.IsValid, Band.Angle.Admits(value: Rotation.Value));
}
```

To:
```csharp
public sealed record IconPose(VectorAngle Rotation, Option<MirrorAxis> Mirror, AssetExtent Extent);
```

Why:
`VectorAngle` and `AssetExtent` already own admission, so `IsValid` duplicates their rules and `Upright` forwards one constructor call. A record class also removes the invalid default struct pose.

Change:
Retain only the three orthogonal pose coordinates and construct the identity rotation directly at callers.

Delta:
Net -4 target LOC and -2 public members; type count unchanged.

Ripples:
In `libs/dotnet/Rasm.AppUi/.planning/Theme/assets.md`, replace `IconPose.Upright(extent) with { Mirror = mirror }` with `new IconPose(VectorAngle.Create(0d), mirror, extent)`. Remove `IconPose.IsValid` checks from any host materializer; the admitted coordinates are authoritative.
