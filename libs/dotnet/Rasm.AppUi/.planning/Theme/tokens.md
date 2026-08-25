# [APPUI_THEME_TOKENS]

Rasm.AppUi resolves every visual constant through one generated token catalogue: a handful of appearance SEED rows expanded by one pure fold into the whole role ladder under typed `TokenKey`s, generated metric scales beside the paint ladder, layered elevation and material rows, orthogonal `ThemeVariantRow` and `DensityRow` families that project the same generation rather than carrying parallel columns, and one typed host-preference probe family. Sibling pages carry the rest of the theme estate: `Theme/semi.md` the shipped-key correspondence and its conformance rail, `Theme/tokens.md` the data-colormap catalog, `Theme/emission.md` the dictionary emission, the swap capsule, the Styles chain, and the control-skin table.

Generation is the page's ruling shape: a hand-authored per-role paint row, a per-density metric column, and a per-variant colour column are all deleted forms — a role is a derivation over a seed, a metric is a step on a scale, and a variant is a projection of one generation. Every colour crossing admits into the kernel `PerceptualColor` owner and reads its `Mix`, `Ramp`, `Tone`, `ToneFor`, `Contrast`, `Difference`, and `Simulate` members through `BlendPath` and `GamutPolicy` rows; the readability floors are the kernel `ContrastFloor` rows (`Rasm/Interaction/paint#[05]-[THEME]`), each naming its WCAG clause and carrying the `PositiveMagnitude` the tonal solve takes, so a derived text rung and the gate that audits it read one published vocabulary and a bare ratio has no spelling. The published data-colormap catalog seats beside the token catalog — both are frozen appearance vocabularies one resolve fold serves.

## [01]-[INDEX]

- [02]-[TOKEN_CATALOG]: Appearance seeds, the generated role ladder and metric scales, elevation/material/wash rows, the severity family, the theme fault floor, and the resolve fold.
- [03]-[VARIANT_AXIS]: Variant rows as generation projections, the posture variants, and the one typed host-preference probe family.
- [04]-[DENSITY_AXIS]: Density as a scale policy re-deriving every metric family, and the one per-surface election of variant beside density.
- [05]-[CATALOG]: The trait vocabulary, the class rows, and the stop catalog with its checked sampler.

## [02]-[TOKEN_CATALOG]

- Owner: `TokenKey` `[ValueObject<string>]` the ONE key mint every resolved bucket and every emission is addressed by; `AppearanceSeed` with `RampPolicy` and `SurfacePosture` the seed row family; `SurfacePolarity` the light/dark ground polarity row carrying ladder order, cast sign, and shadow-alpha lens as columns; `VariantTrait` the variant capability axis; `SeedAnchor`, `PostureSlot`, and `PaintRole` `[SmartEnum<string>]` the role vocabulary, `PaintDerivation` `[Union]` its generation law; `Severity` the folder's ONE ranked alert family over the paint ladder; `MetricFamily` the generated scale owner with its `MetricTrait` capability column; `LayerKind`, `DepthTier`, `MaterialTier`, and `WashRow` the elevation and material families, the scrim itself a `PaintDerivation.Veil` role rather than a fourth family; `TokenRow` `[Union]` the frozen non-generated remainder; `ThemeFault` the direct shared theme and typography fault family; `ThemeCatalog` the resolve fold; `ResolvedTheme` the one resolved artifact every consumer reads.
- Cases: `PaintDerivation` = Tonal | Posture | Readable | Cast | Veil; `TokenRow` = Span | Rank; `LayerKind` = Cast | Ring | Rim; `Severity` = Nominal | Info | Warning | Critical; `ThemeFault` = SwapRejected | MountRejected | PolicyRejected | FaceUnresolved | FaceAdmissionRejected | PaletteRejected | ShapingRejected | DrawRejected | CoverageRejected.
- Law: every generated rung asserts a MINIMUM perceptual difference against its predecessor through the kernel `Difference(other, DeltaMetric)` under the seed's declared floor, so a ladder whose rungs collapse into one another refuses at resolve instead of shipping a surface family the eye reads as flat; every `Readable` rung SOLVES its tone through the kernel `ToneFor` against the role it is drawn on, so a text rung cannot be authored below its own floor and a re-seeded accent carries its readable partner with it; a role derivation names only an EARLIER role, and the fold refuses a forward reference rather than resolving against a bucket that is not yet populated.
- Entry: `ThemeCatalog.Resolve(ThemeVariantRow variant, DensityRow density, AppearanceSeed seed, FontChain chain, PreferenceCell preferences) : Fin<ResolvedTheme>` — one pure fold whose first step is the `Concrete` probe admission, so an unresolved host-matched sentinel structurally cannot reach the row fold; `Expand(AppearanceSeed seed, VariantProjection projection)` the paint generation; `MetricFamily.Value(int step, DensityPolicy policy, VariantProjection projection)` the metric generation and `MetricFamily.Inner(double outer, double inset)` its nesting law; `Severity.Worst(rows, read)` the one worst-of fold every alert surface composes.
- Auto: one resolve feeds control resources, chart paints, SVG tint, icon foreground, editor highlights, status semantics, selection, overlay scrims, dock chrome, every elevation, and the whole type ladder from the same generation — `Types` carries the `Theme/typography` `TypeScale` expansion under the elected chain, the same density policy, and the host text-scale preference, so a density or text-scale flip moves type with geometry inside one fold; `Theme/emission.md` `ThemeRail.ContrastCandidates` and `CvdCandidates` DERIVE from this ladder, so a new role reaches the accessibility sweep with no roster edit.
- Packages: Avalonia, Avalonia.Themes.Fluent, Rasm (project — `PerceptualColor` with `Mix`, `Ramp`, `Tone`, `ToneFor`, `Contrast`, `Difference`, `Simulate`, `BlendPath`, `GamutPolicy`, `DeltaMetric`, `UnitInterval`, `SignedUnit`, `PositiveMagnitude`, `Dimension`; `Rasm.Interaction` `ContrastFloor`; `Rasm.Domain` `FaultBand`/`Fault`; `CapabilitySet`), Wacton.Unicolour (`HueSpan` alone — the traversal argument a polar `BlendPath` row takes), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime
- Growth: a new appearance identity is one `AppearanceSeed` value, never a paint roster; a new role is one `PaintRole` row carrying its derivation; a new metric is one step on an existing `MetricFamily` or one family row; a new elevation is one `DepthTier` row; a new severity is one ranked `Severity` row; a new fault case is one `[FaultCase]` leaf; zero new surface.
- Boundary: `ThemeCatalog` admits every Avalonia colour into the kernel `PerceptualColor` and constructs, converts, and measures no colour through `Wacton.Unicolour` itself — the one package name that crosses is `HueSpan`, the traversal a polar `BlendPath` row takes as its argument. `TokenKey` is minted by the generation owners alone, so a consumer cannot address a bucket by a string it composed and a key that names no generated rung is unspellable rather than a silent lookup miss; the resolved buckets are total by construction and `Palette` reads them on the accumulating rail, so a refused generation reports EVERY missing anchor as one typed `ThemeFault.PaletteRejected` at boot rather than an index throw inside a static initializer. `Readable` is the one derived-contrast form and its solve lives at the kernel: a page-local bisection over `Tone` beside `Contrast` is the deleted form, exactly as a local sRGB lerp is. Light and high-contrast are PROJECTIONS of one generation through `VariantProjection` — the high-contrast projection zeroes near-neutral chroma, raises every `Readable` floor to `ContrastFloor.AaaText`, drops the `Elevation` trait so every shadow stack empties (the shipped high-contrast dictionaries carve no `BoxShadow` slot, so border emphasis substitutes), and widens the stroke family in one row. Elevation is a LAYER STACK, never one offset-and-blur pair, and each layer names its `LayerKind`. `ThemeFault` owns colour and typography failures through its direct generated union cases.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------

[ValueObject<string>(SkipKeyMember = false)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TokenKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = string.IsNullOrWhiteSpace(value)
            ? new ValidationError("TokenKey requires a non-empty generated key.")
            : null;

    internal static TokenKey Rung(string role, int rung) => Create(rung is 0 ? role : $"{role}+{rung}");

    internal static TokenKey Step(string family, int step) => Create($"{family}-{step}");

    internal static TokenKey Named(string owner, string slot) => Create($"{owner}-{slot}");
}

public sealed record AppearanceSeed(
    Color Surface,
    Color Accent,
    Seq<(SeedAnchor Anchor, Color Pigment)> Status,
    Seq<(PostureSlot Slot, SurfacePosture Posture)> Postures,
    RampPolicy Ramp);

public sealed record SurfacePosture(SignedUnit ToneShift, UnitInterval ChromaCeiling, UnitInterval Coverage);

public sealed record RampPolicy(
    Seq<UnitInterval> SurfaceTones,
    Seq<UnitInterval> AccentTones,
    Seq<UnitInterval> StatusTones,
    UnitInterval CastChroma,
    PositiveMagnitude RungFloor,
    DeltaMetric RungMetric,
    BlendPath Path,
    GamutPolicy Gamut);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SurfacePolarity {
    public static readonly SurfacePolarity Light = new("light", castSign: -1d,
        order: static ladder => ladder.Rev(), alpha: static layer => layer.LightAlpha);
    public static readonly SurfacePolarity Dark = new("dark", castSign: 1d,
        order: static ladder => ladder, alpha: static layer => layer.DarkAlpha);

    public double CastSign { get; }

    [UseDelegateFromConstructor]
    public partial Seq<UnitInterval> Order(Seq<UnitInterval> ladder);

    [UseDelegateFromConstructor]
    public partial UnitInterval Alpha(ShadowLayer layer);
}

[NoReorder]
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VariantTrait : ICapability<VariantTrait> {
    public static readonly VariantTrait Elevation = new("elevation", rank: 0);
    public static readonly VariantTrait Translucency = new("translucency", rank: 1);

    public int Rank { get; }
}

public sealed record VariantProjection(
    SurfacePolarity Polarity,
    UnitInterval ChromaScale,
    Option<ContrastFloor> FloorLift,
    CapabilitySet<VariantTrait> Traits,
    double StrokeGain);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SeedAnchor {
    public static readonly SeedAnchor Surface = new("surface", static seed => Some(seed.Surface), static ramp => ramp.SurfaceTones);
    public static readonly SeedAnchor Accent = new("accent", static seed => Some(seed.Accent), static ramp => ramp.AccentTones);
    public static readonly SeedAnchor Error = new("error", static _ => None, static ramp => ramp.StatusTones);
    public static readonly SeedAnchor Warning = new("warning", static _ => None, static ramp => ramp.StatusTones);
    public static readonly SeedAnchor Success = new("success", static _ => None, static ramp => ramp.StatusTones);
    public static readonly SeedAnchor Info = new("info", static _ => None, static ramp => ramp.StatusTones);

    [UseDelegateFromConstructor]
    public partial Option<Color> Direct(AppearanceSeed seed);

    [UseDelegateFromConstructor]
    public partial Seq<UnitInterval> Ladder(RampPolicy ramp);

    public Fin<Color> Pigment(AppearanceSeed seed) =>
        Direct(seed).IfNone(() => seed.Status.Find(entry => entry.Anchor == this).Map(static entry => entry.Pigment)) switch {
            { IsSome: true, Case: Color pigment } => Fin.Succ(pigment),
            _ => Fin.Fail<Color>(new ThemeFault.PaletteRejected($"seed anchor {Key}")),
        };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PostureSlot {
    public static readonly PostureSlot Panel = new("panel");
    public static readonly PostureSlot Overlay = new("overlay");
    public static readonly PostureSlot Well = new("well");
    public static readonly PostureSlot Raised = new("raised");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PaintDerivation {
    private PaintDerivation() { }

    public sealed record Tonal(SeedAnchor Anchor, Dimension Rungs) : PaintDerivation;
    public sealed record Posture(SeedAnchor Anchor, PostureSlot Slot, Dimension Rungs) : PaintDerivation;
    public sealed record Readable(PaintRole Against, ContrastFloor Floor, Dimension Rungs) : PaintDerivation;
    public sealed record Cast(PaintRole From, SignedUnit Shift, Dimension Rungs) : PaintDerivation;
    public sealed record Veil(PaintRole From, UnitInterval Coverage) : PaintDerivation;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PaintRole {
    public static readonly PaintRole Surface = new("surface", new PaintDerivation.Tonal(SeedAnchor.Surface, Dimension.Create(5)));
    public static readonly PaintRole Panel = new("panel", new PaintDerivation.Posture(SeedAnchor.Surface, PostureSlot.Panel, Dimension.Create(3)));
    public static readonly PaintRole Raised = new("raised", new PaintDerivation.Posture(SeedAnchor.Surface, PostureSlot.Raised, Dimension.Create(3)));
    public static readonly PaintRole Well = new("well", new PaintDerivation.Posture(SeedAnchor.Surface, PostureSlot.Well, Dimension.Create(2)));
    public static readonly PaintRole Overlay = new("overlay", new PaintDerivation.Posture(SeedAnchor.Surface, PostureSlot.Overlay, Dimension.Create(3)));
    public static readonly PaintRole Border = new("border", new PaintDerivation.Cast(Surface, SignedUnit.Create(0.22d), Dimension.Create(3)));
    public static readonly PaintRole Separator = new("separator", new PaintDerivation.Cast(Surface, SignedUnit.Create(0.10d), Dimension.Create(2)));
    public static readonly PaintRole Text = new("text", new PaintDerivation.Readable(Surface, ContrastFloor.AaaText, Dimension.Create(1)));
    public static readonly PaintRole TextMuted = new("text-muted", new PaintDerivation.Readable(Surface, ContrastFloor.AaText, Dimension.Create(2)));
    public static readonly PaintRole TextFaint = new("text-faint", new PaintDerivation.Readable(Surface, ContrastFloor.AaLarge, Dimension.Create(1)));
    public static readonly PaintRole Disabled = new("disabled", new PaintDerivation.Readable(Surface, ContrastFloor.NonText, Dimension.Create(2)));
    public static readonly PaintRole Accent = new("accent", new PaintDerivation.Tonal(SeedAnchor.Accent, Dimension.Create(5)));
    public static readonly PaintRole AccentText = new("accent-text", new PaintDerivation.Readable(Accent, ContrastFloor.AaText, Dimension.Create(1)));
    public static readonly PaintRole Focus = new("focus", new PaintDerivation.Cast(Accent, SignedUnit.Create(0.14d), Dimension.Create(1)));
    public static readonly PaintRole Link = new("link", new PaintDerivation.Tonal(SeedAnchor.Accent, Dimension.Create(4)));
    public static readonly PaintRole Selection = new("selection", new PaintDerivation.Cast(Accent, SignedUnit.Create(-0.30d), Dimension.Create(3)));
    public static readonly PaintRole SelectionText = new("selection-text", new PaintDerivation.Readable(Selection, ContrastFloor.AaText, Dimension.Create(1)));
    public static readonly PaintRole Highlight = new("highlight", new PaintDerivation.Cast(Accent, SignedUnit.Create(-0.18d), Dimension.Create(2)));
    public static readonly PaintRole Error = new("error", new PaintDerivation.Tonal(SeedAnchor.Error, Dimension.Create(4)));
    public static readonly PaintRole ErrorText = new("error-text", new PaintDerivation.Readable(Surface, ContrastFloor.AaText, Dimension.Create(1)));
    public static readonly PaintRole Warning = new("warning", new PaintDerivation.Tonal(SeedAnchor.Warning, Dimension.Create(4)));
    public static readonly PaintRole Success = new("success", new PaintDerivation.Tonal(SeedAnchor.Success, Dimension.Create(4)));
    public static readonly PaintRole Info = new("info", new PaintDerivation.Tonal(SeedAnchor.Info, Dimension.Create(4)));
    public static readonly PaintRole Scrim = new("scrim", new PaintDerivation.Veil(Surface, UnitInterval.Create(0.62d)));
    public static readonly PaintRole Workbench = new("workbench", new PaintDerivation.Posture(SeedAnchor.Surface, PostureSlot.Well, Dimension.Create(1)));

    public PaintDerivation Derivation { get; }

    public TokenKey At(int rung) => TokenKey.Rung(Key, rung);

    public int Rungs => Derivation.Switch(
        state: unit,
        tonal: static (_, row) => row.Rungs.Value,
        posture: static (_, row) => row.Rungs.Value,
        readable: static (_, row) => row.Rungs.Value,
        cast: static (_, row) => row.Rungs.Value,
        veil: static (_, _) => 1);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Severity {
    public static readonly Severity Nominal = new("nominal", rank: 0, PaintRole.Success);
    public static readonly Severity Info = new("info", rank: 1, PaintRole.Info);
    public static readonly Severity Warning = new("warning", rank: 2, PaintRole.Warning);
    public static readonly Severity Critical = new("critical", rank: 3, PaintRole.Error);

    public int Rank { get; }

    public PaintRole Role { get; }

    public static Severity Worst<T>(Seq<T> rows, Func<T, Severity> read) =>
        rows.Map(read).Fold(Nominal, static (worst, row) => row.Rank > worst.Rank ? row : worst);
}
```

```csharp signature
// --- [MODELS] --------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LayerKind {
    public static readonly LayerKind Cast = new("cast", inset: false);
    public static readonly LayerKind Ring = new("ring", inset: false);
    public static readonly LayerKind Rim = new("rim", inset: true);

    public bool Inset { get; }
}

public sealed record ShadowLayer(
    double OffsetX,
    double OffsetY,
    double Blur,
    double Spread,
    LayerKind Kind,
    PaintRole Tint,
    UnitInterval LightAlpha,
    UnitInterval DarkAlpha);

public sealed record MaterialValue(
    Color Tint,
    UnitInterval TintOpacity,
    UnitInterval MaterialOpacity,
    Color Fallback,
    UnitInterval Grain);

public sealed record WashRow(
    string Module,
    PaintRole Hue,
    UnitInterval Coverage,
    UnitInterval LuminanceCeiling,
    MotionToken Crossfade);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Glazing {
    public static readonly Glazing Translucent = new("translucent");
    public static readonly Glazing Opaque = new("opaque");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DepthTier {
    public static readonly DepthTier Card = new("card", rank: 0, layers: Seq(
        new ShadowLayer(0d, 0d, 0d, 1d, LayerKind.Ring, PaintRole.Border, UnitInterval.Create(0.10d), UnitInterval.Create(0.22d)),
        new ShadowLayer(0d, 1d, 2d, 0d, LayerKind.Cast, PaintRole.Scrim, UnitInterval.Create(0.06d), UnitInterval.Create(0.16d))));
    public static readonly DepthTier Raised = new("raised", rank: 0, layers: Seq(
        new ShadowLayer(0d, 0d, 0d, 1d, LayerKind.Ring, PaintRole.Border, UnitInterval.Create(0.14d), UnitInterval.Create(0.28d)),
        new ShadowLayer(0d, 1d, 3d, 0d, LayerKind.Cast, PaintRole.Scrim, UnitInterval.Create(0.10d), UnitInterval.Create(0.24d)),
        new ShadowLayer(0d, 1d, 0d, 0d, LayerKind.Rim, PaintRole.Raised, UnitInterval.Create(0.35d), UnitInterval.Create(0.10d))));
    public static readonly DepthTier Flyout = new("flyout", rank: 1000, layers: Seq(
        new ShadowLayer(0d, 0d, 0d, 1d, LayerKind.Ring, PaintRole.Border, UnitInterval.Create(0.16d), UnitInterval.Create(0.34d)),
        new ShadowLayer(0d, 4d, 16d, -2d, LayerKind.Cast, PaintRole.Scrim, UnitInterval.Create(0.16d), UnitInterval.Create(0.40d)),
        new ShadowLayer(0d, 1d, 4d, 0d, LayerKind.Cast, PaintRole.Scrim, UnitInterval.Create(0.10d), UnitInterval.Create(0.26d))));
    public static readonly DepthTier Floating = new("floating", rank: 3000, layers: Seq(
        new ShadowLayer(0d, 0d, 0d, 1d, LayerKind.Ring, PaintRole.Border, UnitInterval.Create(0.16d), UnitInterval.Create(0.34d)),
        new ShadowLayer(0d, 6d, 20d, -3d, LayerKind.Cast, PaintRole.Scrim, UnitInterval.Create(0.18d), UnitInterval.Create(0.44d))));
    public static readonly DepthTier Dialog = new("dialog", rank: 2000, layers: Seq(
        new ShadowLayer(0d, 0d, 0d, 1d, LayerKind.Ring, PaintRole.Border, UnitInterval.Create(0.18d), UnitInterval.Create(0.38d)),
        new ShadowLayer(0d, 12d, 40d, -6d, LayerKind.Cast, PaintRole.Scrim, UnitInterval.Create(0.24d), UnitInterval.Create(0.56d)),
        new ShadowLayer(0d, 2d, 8d, 0d, LayerKind.Cast, PaintRole.Scrim, UnitInterval.Create(0.12d), UnitInterval.Create(0.30d))));

    public int Rank { get; }

    public Seq<ShadowLayer> Layers { get; }

    public TokenKey ShadowKey => TokenKey.Named("elevation", Key);

    public TokenKey RankKey => TokenKey.Named("z", Key);

    public Fin<BoxShadows> Resolve(VariantProjection projection, Func<PaintRole, int, Option<Color>> paint) =>
        projection.Traits.Admits(VariantTrait.Elevation)
            ? Layers.Traverse(layer => Shadow(layer, projection, paint)).As()
                .Map(static shadows => shadows.Head.Match(
                    Some: first => new BoxShadows(first, shadows.Tail.ToArray()),
                    None: static () => default(BoxShadows)))
            : Fin.Succ(default(BoxShadows));

    static Fin<BoxShadow> Shadow(ShadowLayer layer, VariantProjection projection, Func<PaintRole, int, Option<Color>> paint) =>
        paint(layer.Tint, 0).Match(
            Some: tint => Fin.Succ(new BoxShadow {
                OffsetX = layer.OffsetX,
                OffsetY = layer.OffsetY,
                Blur = layer.Blur,
                Spread = layer.Spread,
                IsInset = layer.Kind.Inset,
                Color = Color.FromArgb(
                    (byte)Math.Round(projection.Polarity.Alpha(layer).Value * byte.MaxValue),
                    tint.R, tint.G, tint.B),
            }),
            None: () => Fin.Fail<BoxShadow>(new ThemeFault.PaletteRejected($"shadow tint {layer.Tint.Key}")));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MaterialTier {
    public static readonly MaterialTier Chrome = new("chrome", PaintRole.Panel, UnitInterval.Create(0.80d), UnitInterval.Create(0.55d), UnitInterval.Create(0.03d));
    public static readonly MaterialTier Overlay = new("overlay", PaintRole.Overlay, UnitInterval.Create(0.86d), UnitInterval.Create(0.45d), UnitInterval.Create(0.04d));
    public static readonly MaterialTier Sheet = new("sheet", PaintRole.Panel, UnitInterval.Create(0.92d), UnitInterval.Create(0.35d), UnitInterval.Create(0.02d));

    public PaintRole Tint { get; }

    public UnitInterval TintOpacity { get; }

    public UnitInterval MaterialOpacity { get; }

    public UnitInterval Grain { get; }

    public TokenKey MaterialKey => TokenKey.Named("material", Key);

    public MaterialValue Resolve(Glazing glazing, Func<PaintRole, int, Option<Color>> paint) =>
        paint(Tint, 0).IfNone(() => Colors.Transparent) switch {
            var tint => glazing == Glazing.Translucent
                ? new MaterialValue(tint, TintOpacity, MaterialOpacity, tint, Grain)
                : new MaterialValue(tint, UnitInterval.Create(1d), UnitInterval.Create(1d), tint, UnitInterval.Create(0d)),
        };
}

[NoReorder]
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MetricTrait : ICapability<MetricTrait> {
    public static readonly MetricTrait Widens = new("widens", rank: 0);

    public int Rank { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MetricFamily {
    public static readonly MetricFamily Space = new("space", basis: 4d, ratio: 1.5d, steps: Dimension.Create(8), snap: 1d, lens: static policy => policy.Space, traits: CapabilitySet<MetricTrait>.Of());
    public static readonly MetricFamily Radius = new("radius", basis: 2d, ratio: 1.75d, steps: Dimension.Create(5), snap: 1d, lens: static policy => policy.Radius, traits: CapabilitySet<MetricTrait>.Of());
    public static readonly MetricFamily Stroke = new("stroke", basis: 1d, ratio: 2d, steps: Dimension.Create(4), snap: 0.5d, lens: static policy => policy.Stroke, traits: CapabilitySet<MetricTrait>.Of(MetricTrait.Widens));
    public static readonly MetricFamily Extent = new("extent", basis: 20d, ratio: 1.2d, steps: Dimension.Create(5), snap: 2d, lens: static policy => policy.Extent, traits: CapabilitySet<MetricTrait>.Of());
    public static readonly MetricFamily Icon = new("icon", basis: 8d, ratio: 1.25d, steps: Dimension.Create(5), snap: 2d, lens: static policy => policy.Extent, traits: CapabilitySet<MetricTrait>.Of());

    public double Basis { get; }

    public double Ratio { get; }

    public Dimension Steps { get; }

    public double Snap { get; }

    public CapabilitySet<MetricTrait> Traits { get; }

    [UseDelegateFromConstructor]
    public partial UnitInterval Lens(DensityPolicy policy);

    public TokenKey At(int step) => TokenKey.Step(Key, step);

    public double Value(int step, DensityPolicy policy, VariantProjection projection) =>
        Basis * Math.Pow(Ratio, step) * Lens(policy).Value * (Traits.Admits(MetricTrait.Widens) ? projection.StrokeGain : 1d) switch {
            var raw => Math.Max(Snap, Math.Round(raw / Snap, MidpointRounding.ToEven) * Snap),
        };

    public static double Inner(double outer, double inset) => Math.Max(0d, outer - inset);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TokenRow {
    private TokenRow() { }

    public sealed record Span(TokenKey Key, MotionToken Token) : TokenRow;
    public sealed record Rank(TokenKey Key, int Value) : TokenRow;
}

[Equatable]
public sealed partial record ResolvedTheme(
    ThemeVariantRow Variant,
    DensityRow Density,
    AppearanceSeed Seed,
    [property: UnorderedEquality] FrozenDictionary<TokenKey, Color> Paints,
    [property: UnorderedEquality] FrozenDictionary<TokenKey, double> Metrics,
    [property: UnorderedEquality] FrozenDictionary<TokenKey, TextStyleRow> Types,
    [property: UnorderedEquality] FrozenDictionary<TokenKey, BoxShadows> Depths,
    [property: UnorderedEquality] FrozenDictionary<TokenKey, MaterialValue> Materials,
    [property: UnorderedEquality] FrozenDictionary<TokenKey, Duration> Spans,
    [property: UnorderedEquality] FrozenDictionary<TokenKey, int> Ranks,
    [property: IgnoreEquality] ColorPaletteResources Palette) {
    public Color Accent => Seed.Accent;

    public Option<Color> Paint(PaintRole role, int rung = 0) =>
        Paints.TryGetValue(role.At(rung), out Color value) ? Some(value) : None;

    public Option<double> Metric(MetricFamily family, int step) =>
        Metrics.TryGetValue(family.At(step), out double value) ? Some(value) : None;

    public Option<TextStyleRow> Type(TypographyRole role, TypeEmphasis emphasis) =>
        Types.TryGetValue(TypeScale.Key(role, emphasis), out TextStyleRow? value) ? Some(value) : None;
}
```

```csharp signature
// --- [ERRORS] --------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ThemeFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Theme;
    private ThemeFault() { }

    [FaultCase(0)] public sealed partial record SwapRejected(string Detail) : ThemeFault;
    [FaultCase(1)] public sealed partial record MountRejected(string Detail) : ThemeFault;
    [FaultCase(2)] public sealed partial record PolicyRejected(string Detail) : ThemeFault;
    [FaultCase(3)] public sealed partial record FaceUnresolved(string Detail) : ThemeFault;
    [FaultCase(4)] public sealed partial record FaceAdmissionRejected(string Detail) : ThemeFault;
    [FaultCase(5)] public sealed partial record PaletteRejected(string Detail) : ThemeFault;
    [FaultCase(6)] public sealed partial record ShapingRejected(string Detail) : ThemeFault;
    [FaultCase(7)] public sealed partial record DrawRejected(string Detail) : ThemeFault;
    [FaultCase(8)] public sealed partial record CoverageRejected(string Detail) : ThemeFault;
}
```

```csharp signature
// --- [OPERATIONS] ----------------------------------------------------------------------

public static class ThemeCatalog {
    public const string SectionKey = "theme";

    public static readonly AppearanceSeed Default = new(
        Surface: Color.FromUInt32(0xFF17191D),
        Accent: Color.FromUInt32(0xFF3D7EAA),
        Status: [
            (SeedAnchor.Error, Color.FromUInt32(0xFFC5484D)),
            (SeedAnchor.Warning, Color.FromUInt32(0xFFC08A2E)),
            (SeedAnchor.Success, Color.FromUInt32(0xFF4A9A5B)),
            (SeedAnchor.Info, Color.FromUInt32(0xFF4C86B8)),
        ],
        Postures: [
            (PostureSlot.Panel, new SurfacePosture(SignedUnit.Create(0.06d), UnitInterval.Create(0.04d), UnitInterval.Create(0.0d))),
            (PostureSlot.Raised, new SurfacePosture(SignedUnit.Create(0.11d), UnitInterval.Create(0.04d), UnitInterval.Create(0.0d))),
            (PostureSlot.Well, new SurfacePosture(SignedUnit.Create(-0.04d), UnitInterval.Create(0.03d), UnitInterval.Create(0.0d))),
            (PostureSlot.Overlay, new SurfacePosture(SignedUnit.Create(0.15d), UnitInterval.Create(0.05d), UnitInterval.Create(0.62d))),
        ],
        Ramp: new RampPolicy(
            SurfaceTones: Seq(0.10d, 0.14d, 0.19d, 0.25d, 0.32d).Map(UnitInterval.Create),
            AccentTones: Seq(0.58d, 0.66d, 0.50d, 0.74d, 0.40d).Map(UnitInterval.Create),
            StatusTones: Seq(0.56d, 0.64d, 0.46d, 0.72d).Map(UnitInterval.Create),
            CastChroma: UnitInterval.Create(0.04d),
            RungFloor: PositiveMagnitude.Create(2.0d),
            RungMetric: DeltaMetric.Ciede2000,
            Path: BlendPath.Oklch(),
            Gamut: GamutPolicy.Perceptual));

    public static readonly Seq<TokenRow> Rows = [
        new TokenRow.Span(TokenKey.Named("span", "overlay-fade"), MotionPlan.Flyout.Enter),
        new TokenRow.Span(TokenKey.Named("span", "surface-settle"), MotionPlan.Page.Enter),
        new TokenRow.Rank(TokenKey.Named("z", "content"), 0),
        new TokenRow.Rank(TokenKey.Named("z", "tooltip"), 4000),
    ];

    public static readonly Seq<WashRow> Washes = [
        new WashRow("model", PaintRole.Accent, UnitInterval.Create(0.05d), UnitInterval.Create(0.22d), MotionPlan.Page.Enter),
        new WashRow("document", PaintRole.Info, UnitInterval.Create(0.04d), UnitInterval.Create(0.20d), MotionPlan.Page.Enter),
        new WashRow("analysis", PaintRole.Success, UnitInterval.Create(0.04d), UnitInterval.Create(0.20d), MotionPlan.Page.Enter),
        new WashRow("review", PaintRole.Warning, UnitInterval.Create(0.04d), UnitInterval.Create(0.20d), MotionPlan.Page.Enter),
    ];

    public static Fin<ResolvedTheme> Resolve(ThemeVariantRow variant, DensityRow density, AppearanceSeed seed, FontChain chain, PreferenceCell preferences) =>
        ResolveConcrete(variant.Concrete(preferences), density, seed, chain, preferences);

    static Fin<ResolvedTheme> ResolveConcrete(ThemeVariantRow concrete, DensityRow density, AppearanceSeed seed, FontChain chain, PreferenceCell preferences) =>
        from paints in Expand(seed, concrete.Projection)
        let frozen = Frozen(paints)
        let lookup = (Func<PaintRole, int, Option<Color>>)((role, rung) => frozen.TryGetValue(role.At(rung), out Color value) ? Some(value) : None)
        from depths in toSeq(DepthTier.Items).Traverse(tier => tier.Resolve(concrete.Projection, lookup).Map(shadows => (tier.ShadowKey, shadows))).As()
        from palette in Palette(frozen)
        select new ResolvedTheme(
            Variant: concrete,
            Density: density,
            Seed: seed,
            Paints: frozen,
            Metrics: Frozen(Scales(density.Policy, concrete.Projection)),
            Types: TypeScale.Expand(chain, density.Policy, preferences),
            Depths: Frozen(depths),
            Materials: Frozen(toSeq(MaterialTier.Items).Map(tier => (tier.MaterialKey, tier.Resolve(concrete.Glazing(preferences), lookup)))),
            Spans: Frozen(Rows.Choose(row => row is TokenRow.Span span ? Some((span.Key, ReducedMotion.Select(span.Token).Duration)) : None)),
            Ranks: Frozen(Rows.Choose(row => row is TokenRow.Rank rank ? Some((rank.Key, rank.Value)) : None)
                + toSeq(DepthTier.Items).Map(static tier => (tier.RankKey, tier.Rank))),
            Palette: palette);

    public static Fin<Seq<(TokenKey Key, Color Value)>> Expand(AppearanceSeed seed, VariantProjection projection) =>
        toSeq(PaintRole.Items).Fold(
            Fin.Succ(Seq<(TokenKey Key, Color Value)>()),
            (state, role) => state.Bind(emitted => Rungs(role, seed, projection, emitted).Bind(rungs => Apart(role, rungs, seed.Ramp).Map(_ => emitted + rungs))));

    static Fin<Seq<(TokenKey Key, Color Value)>> Rungs(PaintRole role, AppearanceSeed seed, VariantProjection projection, Seq<(TokenKey Key, Color Value)> emitted) =>
        role.Derivation.Switch(
            state: (Role: role, Seed: seed, Projection: projection, Emitted: emitted),
            tonal: static (s, row) =>
                from anchor in row.Anchor.Pigment(s.Seed)
                from rungs in Sweep(s.Role, anchor, s.Projection.Polarity.Order(row.Anchor.Ladder(s.Seed.Ramp)).Take(row.Rungs.Value), s.Seed.Ramp, s.Projection)
                select rungs,
            posture: static (s, row) =>
                from anchor in row.Anchor.Pigment(s.Seed)
                from posture in Posture(s.Seed, row.Slot)
                from rungs in Sweep(s.Role, anchor, Shifted(s.Projection.Polarity.Order(row.Anchor.Ladder(s.Seed.Ramp)), posture).Take(row.Rungs.Value), s.Seed.Ramp, s.Projection)
                select rungs,
            readable: static (s, row) =>
                from against in Lookup(s.Emitted, row.Against.At(0))
                from rungs in Readable(s.Role, against, s.Projection.FloorLift.IfNone(row.Floor), row.Rungs, s.Seed.Ramp)
                select rungs,
            cast: static (s, row) =>
                from origin in Lookup(s.Emitted, row.From.At(0))
                from admitted in Admit(origin)
                from rungs in Sweep(s.Role, origin, Drift(admitted, row.Shift, row.Rungs, s.Projection), s.Seed.Ramp, s.Projection)
                select rungs,
            veil: static (s, row) => Lookup(s.Emitted, row.From.At(0)).Map(origin => Seq((
                s.Role.At(0),
                Color.FromArgb((byte)Math.Round(row.Coverage.Value * byte.MaxValue), origin.R, origin.G, origin.B)))));

    static Fin<SurfacePosture> Posture(AppearanceSeed seed, PostureSlot slot) =>
        seed.Postures.Find(entry => entry.Slot == slot).Match(
            Some: static entry => Fin.Succ(entry.Posture),
            None: () => Fin.Fail<SurfacePosture>(new ThemeFault.PaletteRejected($"posture {slot.Key}")));

    static Fin<Unit> Apart(PaintRole role, Seq<(TokenKey Key, Color Value)> rungs, RampPolicy ramp) =>
        rungs.Zip(rungs.Skip(1))
            .Traverse(pair => from left in Admit(pair.First.Value)
                              from right in Admit(pair.Second.Value)
                              from ok in left.Difference(right, ramp.RungMetric) >= ramp.RungFloor.Value
                                  ? Fin.Succ(unit)
                                  : Fin.Fail<Unit>(new ThemeFault.PaletteRejected($"{role.Key} rungs within {ramp.RungFloor.Value:0.##} dE"))
                              select ok)
            .As()
            .Map(static _ => unit);

    static Fin<Seq<(TokenKey Key, Color Value)>> Readable(PaintRole role, Color against, ContrastFloor floor, Dimension rungs, RampPolicy ramp) =>
        from ground in Admit(against)
        from solved in Steps(rungs.Value)
            .Traverse(rung => ground
                .ToneFor(ground, PositiveMagnitude.Create(floor.Ratio.Value * Math.Pow(EmphasisFalloff, rung)), ToneSweep.Away)
                .Map(colour => (role.At(rung), Avalonia(colour, Some(ramp.Gamut)))))
            .As()
        select solved;

    const double EmphasisFalloff = 0.82d;

    static Fin<Seq<(TokenKey Key, Color Value)>> Sweep(PaintRole role, Color anchor, Seq<UnitInterval> tones, RampPolicy ramp, VariantProjection projection) =>
        from origin in Admit(anchor)
        from swept in tones.Map(static (tone, rung) => (Tone: tone, Rung: rung))
            .Traverse(step => origin.Tone(tone: step.Tone)
                .Map(colour => (role.At(step.Rung), Avalonia(Chroma(colour, projection), Some(ramp.Gamut)))))
            .As()
        select swept;

    static Seq<UnitInterval> Shifted(Seq<UnitInterval> ladder, SurfacePosture posture) =>
        ladder.Map(tone => UnitInterval.Create(Math.Clamp(tone.Value + posture.ToneShift.Value, 0d, 1d)));

    static Seq<UnitInterval> Drift(PerceptualColor origin, SignedUnit shift, Dimension rungs, VariantProjection projection) =>
        Steps(rungs.Value).Map(rung =>
            UnitInterval.Create(Math.Clamp(
                origin.ReferenceLightness + shift.Value * projection.Polarity.CastSign * (rung + 1), 0d, 1d)));

    static PerceptualColor Chroma(PerceptualColor colour, VariantProjection projection) =>
        PerceptualColor.Of(colour.Lightness, colour.OpponentA * projection.ChromaScale.Value, colour.OpponentB * projection.ChromaScale.Value, colour.Alpha)
            .Match(Succ: identity, Fail: _ => colour);

    static Seq<(TokenKey Key, double Value)> Scales(DensityPolicy policy, VariantProjection projection) =>
        toSeq(MetricFamily.Items).Bind(family => Steps(family.Steps.Value)
            .Map(step => (family.At(step), family.Value(step, policy, projection))));

    static Fin<Color> Lookup(Seq<(TokenKey Key, Color Value)> emitted, TokenKey key) =>
        emitted.Find(entry => entry.Key == key).Match(
            Some: static entry => Fin.Succ(entry.Value),
            None: () => Fin.Fail<Color>(new ThemeFault.PaletteRejected($"forward reference {key.Value}")));

    public static ResolvedTheme Simulated(ResolvedTheme resolved, Func<Color, Color> lens) =>
        Frozen(toSeq(resolved.Paints).Map(entry => (entry.Key, lens(entry.Value)))) switch {
            var simulated => resolved with { Paints = simulated, Palette = Palette(simulated).IfFail(resolved.Palette) },
        };

    internal static Fin<PerceptualColor> Admit(Color value) =>
        PerceptualColor.OfRgb(red: value.R, green: value.G, blue: value.B, alpha: value.A / 255d);

    internal static Color Avalonia(PerceptualColor value, Option<GamutPolicy> gamut) =>
        value.ToRgb(gamut) switch { var (red, green, blue, alpha) => Color.FromArgb(alpha, red, green, blue) };

    internal static Seq<int> Steps(int count) => toSeq(Enumerable.Range(0, count));

    static Fin<ColorPaletteResources> Palette(FrozenDictionary<TokenKey, Color> paints) =>
        (At(paints, PaintRole.Accent, 0), At(paints, PaintRole.Text, 0), At(paints, PaintRole.TextMuted, 0),
         At(paints, PaintRole.TextFaint, 0), At(paints, PaintRole.Surface, 0), At(paints, PaintRole.Panel, 0),
         At(paints, PaintRole.Raised, 0), At(paints, PaintRole.Border, 0), At(paints, PaintRole.ErrorText, 0))
            .Apply(static (accent, text, muted, faint, surface, panel, raised, border, error) =>
                new ColorPaletteResources {
                    Accent = accent,
                    BaseHigh = text,
                    BaseMedium = muted,
                    BaseLow = faint,
                    AltHigh = surface,
                    AltMedium = panel,
                    ChromeHigh = border,
                    ChromeMedium = raised,
                    ChromeLow = panel,
                    ErrorText = error,
                    ListLow = panel,
                    RegionColor = surface,
                })
            .As()
            .ToFin();

    static Validation<Error, Color> At(FrozenDictionary<TokenKey, Color> paints, PaintRole role, int rung) =>
        paints.TryGetValue(role.At(rung), out Color value)
            ? Validation<Error, Color>.Success(value)
            : Validation<Error, Color>.Fail((Error)new ThemeFault.PaletteRejected(role.At(rung).Value));

    internal static FrozenDictionary<TokenKey, T> Frozen<T>(IEnumerable<(TokenKey Key, T Value)> entries) =>
        entries.ToFrozenDictionary(static entry => entry.Key, static entry => entry.Value);
}
```

## [03]-[VARIANT_AXIS]

- Owner: `ThemeVariantRow` `[SmartEnum<string>]` binding the page vocabulary to the host variant key column, its `VariantProjection`, and the `Semi.Avalonia` `ThemeVariant` slots; `PostureVariant` the scoped per-surface variant mint; `PreferenceRow` `[SmartEnum<string>]` and `PreferenceValue` `[Union]` the typed host-preference family, its concession rows keyed by the kernel `MotionConcession` vocabulary; `PreferenceCell` the probe capsule every consumer binds.
- Cases: `ThemeVariantRow` = light | dark | high-contrast-light | high-contrast-dark | host-matched — host-matched is a probe fold, never a resolved row, and the two high-contrast rows bind the shipped `SemiTheme` high-contrast variants; `PreferenceRow` = appearance | increased-contrast | reduced-motion | reduced-transparency | text-scale, the three concession rows each carrying their kernel `MotionConcession` row; `PreferenceValue` = Appearance | Granted | Withheld | Scale — a concession is PRESENT or WITHHELD, never a bool.
- Law: a preference is read through ONE capsule and never through a second per-concern probe path — the variant fold, the motion degrade switch, the material translucency gate, and the typography multiplier are four consumers of one owner, so a host flip re-derives every dependent surface in one resolve; a pinned preference overrides the host read and disposes back to it, so a proof lane fixes appearance, contrast, motion, transparency, and text scale independently of whatever machine executes it.
- Entry: `ThemeVariantRow.Concrete(PreferenceCell preferences)` — total fold; concrete rows return themselves and the absent-probe default is `Light`; `PreferenceCell.Read(PreferenceRow row)`, `Concedes(MotionConcession row)`, `Concessions`, `Track(Action<PreferenceRow>)`, and `Pin(row, value)` are the whole probe surface; `PreferenceCell.OfPlatform(IPlatformSettings, hostSeam, hostFlips)` builds the standalone binding over the seam's read-and-change pair.
- Auto: host appearance and contrast flips ride `IPlatformSettings.ColorValuesChanged` into `Track`, so a host dark-mode or high-contrast change re-resolves and receipts with zero per-control handlers; a `PostureVariant` scope re-maps the surface family for a panel or overlay subtree through the subtree's `RequestedThemeVariant`, so a posture is a resource-resolution fact rather than a second dictionary the swap has to keep in step.
- Packages: Avalonia, Semi.Avalonia, System.Reactive (`Disposable.Create` and `CompositeDisposable`), Rasm (project — `MotionConcession`, `CapabilitySet`), Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new preference is one `PreferenceRow` row plus its host-seam read-and-change pair; a new accessibility concession is one kernel `MotionConcession` row and one `PreferenceRow` row naming it; a new surface posture is one `PostureSlot` row and its emitted variant partition.
- Boundary: probes are host-agnostic delegate columns supplied at mount — the rhino probe reads `HostUtils.RunningInDarkMode` with flips riding `Rhino.UI.ThemeSettings.ThemeChanged` host-side, gh2 rides the same host row, standalone rows read `Application.PlatformSettings` (`GetColorValues()` answering `ThemeVariant` and `ContrastPreference`, re-probed on `ColorValuesChanged`), and the browser probe stays a designed-only column. Avalonia publishes appearance and contrast alone: reduced motion, reduced transparency, and text scale have NO platform surface, so those three rows read the host-attach seam column a mount supplies and default to `Withheld` when a host answers nothing. The seam is a PAIR, read beside change, because a host that can answer those rows can also flip them — with the platform subscription as the only change source three of five rows are unraisable, and a host answering nothing binds an empty subscription rather than an absent one. `RegisterFollowSystemTheme` is NOT the OS light-and-dark follow: it guards on Windows, tracks `ContrastPreference` alone, and maps the system accent onto one of the four shipped high-contrast variants, so mounting it would install a second appearance driver. The four shipped Semi high-contrast variants inherit every palette key from their parent variant, so the shipped side is a system-colour mapping and the high-contrast PALETTE is this page's own projection: two rows here, `Desert` carrying the light-inheriting chain and `NightSky` the dark-inheriting one, and a locally minted `new ThemeVariant("high-contrast", …)` key never reaches that dictionary because inheritance resolves through the shipped key. Density and variant are orthogonal and compose only inside `Resolve`; the per-surface override is the `SurfaceOverride` delegate column on the swap capsule.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ThemeVariantRow {
    public static readonly ThemeVariantRow Light = new("light", ThemeVariant.Light,
        new VariantProjection(SurfacePolarity.Light, UnitInterval.Create(1d), None,
            CapabilitySet<VariantTrait>.Of(VariantTrait.Elevation, VariantTrait.Translucency), StrokeGain: 1d));
    public static readonly ThemeVariantRow Dark = new("dark", ThemeVariant.Dark,
        new VariantProjection(SurfacePolarity.Dark, UnitInterval.Create(1d), None,
            CapabilitySet<VariantTrait>.Of(VariantTrait.Elevation, VariantTrait.Translucency), StrokeGain: 1d));
    public static readonly ThemeVariantRow HighContrastLight = new("high-contrast-light", SemiTheme.Desert,
        new VariantProjection(SurfacePolarity.Light, UnitInterval.Create(0d), Some(ContrastFloor.AaaText),
            CapabilitySet<VariantTrait>.Of(), StrokeGain: 2d));
    public static readonly ThemeVariantRow HighContrastDark = new("high-contrast-dark", SemiTheme.NightSky,
        new VariantProjection(SurfacePolarity.Dark, UnitInterval.Create(0d), Some(ContrastFloor.AaaText),
            CapabilitySet<VariantTrait>.Of(), StrokeGain: 2d));
    public static readonly ThemeVariantRow HostMatched = new("host-matched", ThemeVariant.Default,
        new VariantProjection(SurfacePolarity.Light, UnitInterval.Create(1d), None,
            CapabilitySet<VariantTrait>.Of(VariantTrait.Elevation, VariantTrait.Translucency), StrokeGain: 1d));

    public ThemeVariant Variant { get; }

    public VariantProjection Projection { get; }

    public bool Dark => Projection.Polarity == SurfacePolarity.Dark;

    public static Seq<ThemeVariantRow> Emitted => Seq(Light, Dark, HighContrastLight, HighContrastDark);

    public ThemeVariantRow Concrete(PreferenceCell preferences) => Switch(
        state: preferences,
        light: static (p, _) => Contrasted(Light, p),
        dark: static (p, _) => Contrasted(Dark, p),
        highContrastLight: static (_, _) => HighContrastLight,
        highContrastDark: static (_, _) => HighContrastDark,
        hostMatched: static (p, _) => Contrasted(
            p.Read(PreferenceRow.Appearance) is PreferenceValue.Appearance { Row: { } row } && row != HostMatched ? row : Light,
            p));

    public Glazing Glazing(PreferenceCell preferences) =>
        Projection.Traits.Admits(VariantTrait.Translucency) && !preferences.Concedes(MotionConcession.ReduceTransparency)
            ? Glazing.Translucent
            : Glazing.Opaque;

    static ThemeVariantRow Contrasted(ThemeVariantRow row, PreferenceCell preferences) =>
        preferences.Concedes(MotionConcession.IncreaseContrast)
            ? (row.Dark ? HighContrastDark : HighContrastLight)
            : row;
}

public static class PostureVariant {
    public static ThemeVariant Of(ThemeVariantRow row, PostureSlot slot) =>
        new($"{row.Key}/{slot.Key}", row.Variant);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PreferenceValue {
    private PreferenceValue() { }

    public sealed record Appearance(ThemeVariantRow Row) : PreferenceValue;
    public sealed record Granted(MotionConcession Row) : PreferenceValue;
    public sealed record Withheld(MotionConcession Row) : PreferenceValue;
    public sealed record Scale(UnitInterval Factor) : PreferenceValue;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PreferenceRow {
    public static readonly PreferenceRow Appearance = new("appearance", None,
        static () => new PreferenceValue.Appearance(ThemeVariantRow.Light));
    public static readonly PreferenceRow IncreasedContrast = new("increased-contrast", Some(MotionConcession.IncreaseContrast),
        static () => new PreferenceValue.Withheld(MotionConcession.IncreaseContrast));
    public static readonly PreferenceRow ReducedMotion = new("reduced-motion", Some(MotionConcession.ReduceMotion),
        static () => new PreferenceValue.Withheld(MotionConcession.ReduceMotion));
    public static readonly PreferenceRow ReducedTransparency = new("reduced-transparency", Some(MotionConcession.ReduceTransparency),
        static () => new PreferenceValue.Withheld(MotionConcession.ReduceTransparency));
    public static readonly PreferenceRow TextScale = new("text-scale", None,
        static () => new PreferenceValue.Scale(UnitInterval.Create(0.5d)));

    public Option<MotionConcession> Concession { get; }

    [UseDelegateFromConstructor]
    public partial PreferenceValue Fallback();
}

public sealed class PreferenceCell(
    Func<PreferenceRow, Option<PreferenceValue>> host,
    Func<Action<PreferenceRow>, IDisposable> changed,
    Atom<HashMap<PreferenceRow, PreferenceValue>> pinned) {
    public PreferenceValue Read(PreferenceRow row) =>
        pinned.Value.Find(row).IfNone(() => host(row).IfNone(row.Fallback));

    public bool Concedes(MotionConcession concession) =>
        toSeq(PreferenceRow.Items)
            .Exists(row => row.Concession == Some(concession) && Read(row) is PreferenceValue.Granted);

    public CapabilitySet<MotionConcession> Concessions =>
        CapabilitySet<MotionConcession>.Of(toSeq(PreferenceRow.Items)
            .Choose(row => Read(row) is PreferenceValue.Granted granted ? Some(granted.Row) : None)
            .ToArray());

    public IDisposable Track(Action<PreferenceRow> observe) => changed(observe);

    public IDisposable Pin(PreferenceRow row, PreferenceValue value) {
        pinned.Swap(map => map.AddOrUpdate(row, value));
        return Disposable.Create(() => pinned.Swap(map => map.Remove(row)));
    }

    public static PreferenceCell OfPlatform(
        IPlatformSettings settings,
        Func<PreferenceRow, Option<PreferenceValue>> hostSeam,
        Func<Action<PreferenceRow>, IDisposable> hostFlips) =>
        new(host: row => row.Switch(
                state: (Settings: settings, Seam: hostSeam),
                appearance: static (s, _) => Some<PreferenceValue>(new PreferenceValue.Appearance(
                    s.Settings.GetColorValues().ThemeVariant is PlatformThemeVariant.Dark ? ThemeVariantRow.Dark : ThemeVariantRow.Light)),
                increasedContrast: static (s, _) => Some<PreferenceValue>(
                    s.Settings.GetColorValues().ContrastPreference is ColorContrastPreference.High
                        ? new PreferenceValue.Granted(MotionConcession.IncreaseContrast)
                        : new PreferenceValue.Withheld(MotionConcession.IncreaseContrast)),
                reducedMotion: static (s, r) => s.Seam(r),
                reducedTransparency: static (s, r) => s.Seam(r),
                textScale: static (s, r) => s.Seam(r)),
            changed: observe => {
                void OnColorValues(object? sender, PlatformColorValues values) {
                    observe(PreferenceRow.Appearance);
                    observe(PreferenceRow.IncreasedContrast);
                }

                settings.ColorValuesChanged += OnColorValues;
                return new CompositeDisposable(
                    Disposable.Create(() => settings.ColorValuesChanged -= OnColorValues),
                    hostFlips(observe));
            },
            pinned: Atom(HashMap<PreferenceRow, PreferenceValue>()));
}
```

| [INDEX] | [SURFACE_ROWS]            | [APPEARANCE_AND_CONTRAST]                           | [MOTION_TRANSPARENCY_SCALE]    | [ROUTE_STATE] |
| :-----: | :------------------------ | :-------------------------------------------------- | :----------------------------- | :------------ |
|  [01]   | rhino-panel, rhino-modal  | `RunningInDarkMode` read, `ThemeChanged` flips      | host-attach seam columns       | settled       |
|  [02]   | gh2-companion             | same host appearance row as rhino                   | host-attach seam columns       | settled       |
|  [03]   | avalonia-desktop, sidecar | `GetColorValues()` read, `ColorValuesChanged` flips | seam absent, withheld defaults | settled       |
|  [04]   | web-browser               | designed-only column, zero interop                  | designed-only column           | designed-only |
|  [05]   | headless, offscreen       | probe absent, `Light` default                       | proof-lane pins per row        | settled       |

## [04]-[DENSITY_AXIS]

- Owner: `DensityRow` `[SmartEnum<string>]` three rows binding `DensityStyle` and carrying one `DensityPolicy`; `DensityPolicy` the scale-factor record every `MetricFamily` reads; `SurfaceElection` the ONE per-surface fold electing variant beside density.
- Cases: comfortable | default | compact — three postures of one policy, never three metric tables.
- Law: density is a SCALE POLICY, not a fork — one factor per metric family re-derives space, radius, stroke, control extent, and icon box together, so a per-density metric column is unrepresentable; the surface CLASS elects variant and density through ONE dispatch, because two folds over one union disagreed the moment either gained an arm.
- Entry: `DensityRow.Policy` — the factor set the metric generation reads; `SurfaceElection.Of(ResolvedProfile resolved)` — the full product election including the sidecar topology override; `SurfaceElection.Density(ConsumptionProfile profile)` — the density half a per-surface resolve reads.
- Auto: every metric family re-derives from the elected policy inside `Resolve`, so a density change is one row value; the virtualization extent ledger re-realizes on the resolved `extent-*` change because the swap receipt carries those keys in its diff.
- Packages: Avalonia.Themes.Fluent, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one density row carrying its policy; one surface class is one arm of the ONE election dispatch.
- Boundary: the Fluent compact resource swap rides the `Style` column on the one rail, never a parallel compact stylesheet; the type ladder reads `TypeScale` under the same policy so a density election moves type with geometry; a surface electing a density does NOT independently elect a variant — both columns come off the one election row, and the per-surface variant OVERRIDE is the swap capsule's delegate column.

```csharp signature
public sealed record DensityPolicy(UnitInterval Space, UnitInterval Radius, UnitInterval Stroke, UnitInterval Extent, UnitInterval Type);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DensityRow {
    public static readonly DensityRow Comfortable = new("comfortable", DensityStyle.Normal,
        new DensityPolicy(UnitInterval.Create(1d), UnitInterval.Create(1d), UnitInterval.Create(1d), UnitInterval.Create(1d), UnitInterval.Create(1d)));
    public static readonly DensityRow Default = new("default", DensityStyle.Normal,
        new DensityPolicy(UnitInterval.Create(0.875d), UnitInterval.Create(1d), UnitInterval.Create(1d), UnitInterval.Create(0.9d), UnitInterval.Create(0.95d)));
    public static readonly DensityRow Compact = new("compact", DensityStyle.Compact,
        new DensityPolicy(UnitInterval.Create(0.75d), UnitInterval.Create(1d), UnitInterval.Create(1d), UnitInterval.Create(0.78d), UnitInterval.Create(0.9d)));

    public DensityStyle Style { get; }

    public DensityPolicy Policy { get; }
}

public static class SurfaceElection {
    public static (ThemeVariantRow Variant, DensityRow Density) Of(ResolvedProfile resolved) =>
        resolved.Profile.Topology == DeploymentTopology.Sidecar
            ? (ThemeVariantRow.Dark, DensityRow.Compact)
            : Class(resolved.Profile);

    public static DensityRow Density(ConsumptionProfile profile) => Class(profile).Density;

    static (ThemeVariantRow Variant, DensityRow Density) Class(ConsumptionProfile profile) =>
        profile.Surface.Switch(
            state: unit,
            embedded: static _ => (ThemeVariantRow.HostMatched, DensityRow.Compact),
            windowed: static _ => (ThemeVariantRow.HostMatched, DensityRow.Default),
            offscreen: static _ => (ThemeVariantRow.Light, DensityRow.Comfortable),
            none: static _ => (ThemeVariantRow.Light, DensityRow.Comfortable));
}
```

## [05]-[CATALOG]

- Owner: `ColormapTrait` the class capability axis; `ColormapClass` `[SmartEnum]` five rows carrying their trait set and their `BlendPath` traversal; `Colormap` `[SmartEnum<string>]` the published-palette catalog with the one sampler.
- Cases: Sequential | Diverging | Rainbow | Cyclic | Qualitative — the traversal is CLASS data (a cyclic map takes the long hue way round so its ends meet, a rainbow traverses monotonically increasing hue, a diverging map takes the short path through its neutral centre, hueless classes take the rectangular row and can state no hue path at all); traits are LightnessMonotone | Centered | Discrete, granted per class rather than spelled as three bools whose corners no row set discriminated.
- Law: a class's declared lightness order is a CHECKED claim over the GENERATED ramp, never a label on the row — the trend reads the kernel's reference-corrected lightness because the stored basis channel mis-ranks near-black and passes a ramp that visibly plateaus there; a class granting no order admits its ramp unexamined.
- Entry: `Sample(double t) : Fin<Color>` — the one sampler, clamped and finite-gated; `Ramp(int steps) : Fin<Seq<Color>>` — the checked ramp; `HeatMap<T>(int steps, Func<Color, T> project)` — the caller-shaped projection that reproduces no colour arithmetic.
- Packages: Avalonia, Rasm (project — `PerceptualColor`, `BlendPath`, `GamutPolicy`, `UnitInterval`), Wacton.Unicolour (`HueSpan`), Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new data-colormap is one `Colormap` row carrying its class and its published anchor stops with their source; a new class treatment is one trait row.
- Boundary: the stop rosters are PUBLISHED palettes, cited per row — viridis/magma/cividis from the matplotlib perceptually-uniform family, turbo from Google AI, coolwarm from Moreland's diverging maps, twilight from matplotlib's cyclic pair, Tableau 10 from the Tableau categorical set — so provenance is the row's, never a hand-picked hex run; sampling composes the kernel `PerceptualColor.Mix` under the class's own traversal through the tokens page's admission edge, and a local sRGB lerp is the deleted form.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------

[NoReorder]
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ColormapTrait : ICapability<ColormapTrait> {
    public static readonly ColormapTrait LightnessMonotone = new("lightness-monotone", rank: 0);
    public static readonly ColormapTrait Centered = new("centered", rank: 1);
    public static readonly ColormapTrait Discrete = new("discrete", rank: 2);

    public int Rank { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ColormapClass {
    public static readonly ColormapClass Sequential = new("sequential",
        CapabilitySet<ColormapTrait>.Of(ColormapTrait.LightnessMonotone), BlendPath.Oklab);
    public static readonly ColormapClass Diverging = new("diverging",
        CapabilitySet<ColormapTrait>.Of(ColormapTrait.Centered), BlendPath.Oklch());
    public static readonly ColormapClass Rainbow = new("rainbow",
        CapabilitySet<ColormapTrait>.Of(), BlendPath.Oklch(HueSpan.Increasing));
    public static readonly ColormapClass Cyclic = new("cyclic",
        CapabilitySet<ColormapTrait>.Of(ColormapTrait.Centered), BlendPath.Oklch(HueSpan.Longer));
    public static readonly ColormapClass Qualitative = new("qualitative",
        CapabilitySet<ColormapTrait>.Of(ColormapTrait.Discrete), BlendPath.Oklab);

    public CapabilitySet<ColormapTrait> Traits { get; }

    public BlendPath Path { get; }
}

// --- [TABLES] --------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Colormap {
    public static readonly Colormap Viridis = new("viridis", ColormapClass.Sequential, stops: Seq(
        Color.FromUInt32(0xFF440154), Color.FromUInt32(0xFF414487), Color.FromUInt32(0xFF2A788E),
        Color.FromUInt32(0xFF22A884), Color.FromUInt32(0xFF7AD151), Color.FromUInt32(0xFFFDE725)));
    public static readonly Colormap Magma = new("magma", ColormapClass.Sequential, stops: Seq(
        Color.FromUInt32(0xFF000004), Color.FromUInt32(0xFF3B0F70), Color.FromUInt32(0xFF8C2981),
        Color.FromUInt32(0xFFDE4968), Color.FromUInt32(0xFFFE9F6D), Color.FromUInt32(0xFFFCFDBF)));
    public static readonly Colormap Cividis = new("cividis", ColormapClass.Sequential, stops: Seq(
        Color.FromUInt32(0xFF00224E), Color.FromUInt32(0xFF35456C), Color.FromUInt32(0xFF666970),
        Color.FromUInt32(0xFF948E77), Color.FromUInt32(0xFFCBBA69), Color.FromUInt32(0xFFFEE838)));
    public static readonly Colormap Turbo = new("turbo", ColormapClass.Rainbow, stops: Seq(
        Color.FromUInt32(0xFF30123B), Color.FromUInt32(0xFF4145AB), Color.FromUInt32(0xFF26BCE1),
        Color.FromUInt32(0xFF7DFF56), Color.FromUInt32(0xFFFB8022), Color.FromUInt32(0xFF7A0403)));
    public static readonly Colormap Coolwarm = new("coolwarm", ColormapClass.Diverging, stops: Seq(
        Color.FromUInt32(0xFF3B4CC0), Color.FromUInt32(0xFF9ABBFF), Color.FromUInt32(0xFFDDDDDD),
        Color.FromUInt32(0xFFF49A7B), Color.FromUInt32(0xFFB40426)));
    public static readonly Colormap Twilight = new("twilight", ColormapClass.Cyclic, stops: Seq(
        Color.FromUInt32(0xFFE2D9E2), Color.FromUInt32(0xFF6276BA), Color.FromUInt32(0xFF2F1436),
        Color.FromUInt32(0xFFAF4B70), Color.FromUInt32(0xFFE2D9E2)));
    public static readonly Colormap Tableau = new("tableau", ColormapClass.Qualitative, stops: Seq(
        Color.FromUInt32(0xFF4E79A7), Color.FromUInt32(0xFFF28E2B), Color.FromUInt32(0xFFE15759),
        Color.FromUInt32(0xFF76B7B2), Color.FromUInt32(0xFF59A14F), Color.FromUInt32(0xFFEDC948),
        Color.FromUInt32(0xFFB07AA1), Color.FromUInt32(0xFFFF9DA7), Color.FromUInt32(0xFF9C755F), Color.FromUInt32(0xFFBAB0AC)));

    public ColormapClass Class { get; }

    public Seq<Color> Stops { get; }

    public Fin<Color> Sample(double t) => double.IsFinite(t)
        ? SampleAdmitted(Math.Clamp(t, 0d, 1d))
        : Fin.Fail<Color>(new ThemeFault.PaletteRejected($"sample {t}"));

    private Fin<Color> SampleAdmitted(double t) =>
        Class.Traits.Admits(ColormapTrait.Discrete)
            ? Stops.At(Math.Min((int)(t * Stops.Count), Stops.Count - 1))
                .ToFin(Fail: new ThemeFault.PaletteRejected($"stop {t} of {Key}"))
            : (Scaled: t * (Stops.Count - 1)) switch {
                var (scaled) => Math.Min((int)scaled, Stops.Count - 2) switch {
                    var lo => (Stops.At(lo), Stops.At(lo + 1))
                        .Apply((left, right) => (Left: left, Right: right))
                        .ToFin(Fail: new ThemeFault.PaletteRejected($"segment {lo} of {Key}"))
                        .Bind(pair => UnitInterval.TryCreate(scaled - lo, out UnitInterval? amount)
                            ? Mix(pair.Left, pair.Right, amount, Class.Path)
                            : Fin.Fail<Color>(new ThemeFault.PaletteRejected($"amount {scaled - lo}"))),
                },
            };

    public Fin<Seq<Color>> Ramp(int steps) =>
        steps > 0
            ? (steps == 1
                ? Sample(0d).Map(static color => Seq(color))
                : ThemeCatalog.Steps(steps)
                    .TraverseM(step => Sample((double)step / (steps - 1)))
                    .As()
                    .Bind(Ordered))
            : Fin.Fail<Seq<Color>>(new ThemeFault.PaletteRejected($"steps {steps}"));

    Fin<Seq<Color>> Ordered(Seq<Color> ramp) =>
        !Class.Traits.Admits(ColormapTrait.LightnessMonotone)
            ? Fin.Succ(ramp)
            : Lightness(ramp).Bind(levels =>
                (levels.Head, levels.Last).Apply(static (head, last) => double.Sign(last - head))
                    .ToFin(Fail: new ThemeFault.PaletteRejected($"empty ramp {Key}"))
                    .Bind(trend => levels.Zip(levels.Skip(1)).ForAll(pair => trend * (pair.Second - pair.First) >= 0d)
                        ? Fin.Succ(ramp)
                        : Fin.Fail<Seq<Color>>(new ThemeFault.PaletteRejected($"non-monotonic lightness {Key}"))));

    public Fin<T[]> HeatMap<T>(int steps, Func<Color, T> project) =>
        Ramp(steps).Map(colors => colors.Map(project).ToArray());

    static Fin<Color> Mix(Color left, Color right, UnitInterval amount, BlendPath path) =>
        from origin in ThemeCatalog.Admit(left)
        from target in ThemeCatalog.Admit(right)
        select ThemeCatalog.Avalonia(origin.Mix(target, amount, Some(path)), None);

    static Fin<Seq<double>> Lightness(Seq<Color> ramp) =>
        ramp.TraverseM(ThemeCatalog.Admit).As().Map(static admitted => admitted.Map(static colour => colour.ReferenceLightness));
}
```

## [06]-[RESEARCH]

(none)
