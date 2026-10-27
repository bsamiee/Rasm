using System.Numerics;
using MathNet.Numerics.RootFinding;
using Rasm.UI.Typography;
using SkiaSharp;
using SuperLinq;
using Wacton.Unicolour;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record StateLayer(Interaction Interaction, float Weight);

internal sealed record Ramp(SKColorF From, SKColorF To, Configuration Gamut);

internal sealed record Legibility(SKColorF Color, SKColorF Ground, double Ratio, Configuration Gamut);

internal sealed record Hues(SKColorF Accent, Configuration Gamut);

internal sealed record HueCycle(int Count, int Stride) {
    internal double Step => 360d * Stride / Count;
}

public sealed record Token(Role Role, Channel Channel) {
    public Option<Condition> Condition { get; init; }
    public Option<Interaction> Interaction { get; init; }
}

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class ColorScheme {
    // --- [SCHEME]
    private static readonly Token WindowFill = new(Role.Window, Channel.Fill);
    private static readonly Token WindowText = new(Role.Window, Channel.Text);
    private static readonly Token WindowDisabled = new(Role.Window, Channel.Text) { Condition = Condition.Disabled };
    private static readonly Seq<Token> Required = [WindowFill, WindowText, WindowDisabled];

    private static readonly HashMap<Token, Unicolour> Signals = HashMap(
        (new Token(Role.Window, Channel.Text) { Condition = Condition.Success }, new Unicolour(ColourSpace.Oklch, 0.72, 0.17, 150)),
        (new Token(Role.Window, Channel.Text) { Condition = Condition.Warning }, new Unicolour(ColourSpace.Oklch, 0.77, 0.17, 64)),
        (new Token(Role.Window, Channel.Text) { Condition = Condition.Faulted }, new Unicolour(ColourSpace.Oklch, 0.63, 0.24, 29)));

    public HashMap<Token, SKColorF> Chrome { get; }
    internal HashSet<Condition> Conditions { get; }
    public TextFace Face { get; }
    public float FontSize { get; }
    public Configuration Configuration { get; }

    public SKColorF Background => Chrome[WindowFill];

    public ContrastLevel Contrast => Conditions.Contains(Condition.IncreaseContrast) ? ContrastLevel.Enhanced : ContrastLevel.Minimum;

    public static Fin<ColorScheme> Validated(HashMap<Token, SKColorF> chrome, HashSet<Condition> conditions, TextFace face, float fontSize, Option<Configuration> configuration = default) {
        Seq<Token> missing = Required.Filter(token => !chrome.ContainsKey(token));
        Configuration gamut = configuration.IfNone(Configuration.Default);
        return (UIFault.Unless(missing.IsEmpty, UIReason.Missing, missing), UIFault.Unless(face.Family.IsSome, UIReason.Missing, face.Family), UIFault.Unless(!conditions.Contains(Condition.Dark), UIReason.Derived, conditions))
            .Apply((_, _, _) => new ColorScheme(chrome.TryAddRange(Signals.Map(signal => signal.Mapped(gamut)).AsIterable()), Darkened(chrome, conditions), face, fontSize, gamut)).As();
    }

    private static HashSet<Condition> Darkened(HashMap<Token, SKColorF> chrome, HashSet<Condition> conditions) =>
        chrome[WindowFill].ToUnicolour().Oklch.L < chrome[WindowText].ToUnicolour().Oklch.L ? conditions.Add(Condition.Dark) : conditions;

    // --- [VEIL]
    public float DisabledOpacity(SKColorSpace target) {
        Vector3 text = Offset(Chrome[WindowText], Chrome[WindowFill], target);
        float span = Vector3.Dot(text, text);
        return span > 0f ? float.Clamp(Vector3.Dot(text, Offset(Chrome[WindowDisabled], Chrome[WindowFill], target)) / span, 0f, 1f) : 1f;
    }

    private static Vector3 Offset(SKColorF color, SKColorF background, SKColorSpace target) => (Channels(color, target) - Channels(background, target)) * color.Alpha;

    private static Vector3 Channels(SKColorF color, SKColorSpace target) =>
        (target.GammaIsLinear ? color.ToUnicolour().RgbLinear.Triplet : color.ToUnicolour().Rgb.Triplet) switch {
            var channels => new((float)channels.First, (float)channels.Second, (float)channels.Third),
        };

    // --- [COLORS]
    internal Option<SKColorF> Color(ColorValue value, RoleStyle style, Option<DataSeries> series = default) =>
        value.Switch(
            (Theme: this, Style: style, Series: series),
            @fixed: static (_, @fixed) => Some(@fixed.Color.ToSKColorF()),
            host: static (held, host) => held.Theme.HostColor(host, held.Style, held.Series),
            data: static (held, data) => held.Series.Bind(series => held.Theme.SeriesColor(series, held.Style)).Map(color => Faded(color, data.Alpha)),
            mixed: static (held, mixed) =>
                from start in held.Theme.Color(mixed.From, held.Style, held.Series)
                from end in held.Theme.Color(mixed.To, held.Style, held.Series)
                select start.ToUnicolour().Mix(end.ToUnicolour(), mixed.Space, (float)mixed.Amount, mixed.Hue).Mapped(held.Theme.Configuration),
            contrasting: static (held, contrasting) => held.Theme.Color(contrasting.Over, held.Style, held.Series).Bind(over =>
                contrasting.Candidates.Choose(candidate => held.Theme.Color(candidate, held.Style, held.Series))
                    .Fold(Option<SKColorF>.None, (best, next) => best.Filter(kept => Styles.Ratio(kept, over) >= Styles.Ratio(next, over)) | Some(next))),
            relative: static (held, relative) => held.Theme.Color(relative.From, held.Style, held.Series).Map(origin => relative.Shifted(origin, held.Theme.Configuration)));

    private Option<SKColorF> HostColor(ColorValue.Host host, RoleStyle style, Option<DataSeries> series) =>
        (host.Role.Match(Some: role => Chrome.Find(new Token(role, host.Channel) { Condition = host.Condition }), None: () => RoleColor(host, style))
            | (host.Role.IsNone && host.Fallback.IsNone && host.Channel.Inherited ? style.Enclosing.Bind(enclosing => Color(enclosing.Text, enclosing, series)) : None))
        .Map(color => Faded(color, host.Alpha))
        | host.Fallback.Bind(fallback => Color(fallback, style, series));

    private Option<SKColorF> RoleColor(ColorValue.Host host, RoleStyle style, Option<Interaction> interaction = default) =>
        host.Condition.Match(
                Some: static condition => Seq(Some(condition)),
                None: () => toSeq(Condition.Items).Rev().Filter(style.Shown.Contains).Map(static condition => Some(condition)) + Seq(Option<Condition>.None))
            .Choose(condition => Chrome.Find(new Token(style.Role, host.Channel) { Condition = condition, Interaction = interaction })).Head;

    private static SKColorF Faded(SKColorF color, Fraction alpha) => color.WithAlpha(color.Alpha * (float)alpha);

    // --- [STATE_LAYERS]
    private const int StateLayerSteps = 16;

    internal Option<SKColorF> WithStateLayers(ColorCache colorCache, ColorValue value, RoleStyle style, Seq<StateLayer> layers, HashMap<Interaction, ColorValue> targets = default, Option<DataSeries> series = default) =>
        layers.Fold(Color(value, style, series), (rest, layer) =>
            from start in rest
            select (InteractionColor(value, style, layer.Interaction) | targets.Find(layer.Interaction).Bind(target => Color(target, style, series)))
                .Map(end => colorCache.Ramped(new Ramp(start, end, Configuration))[(int)MathF.Round(layer.Weight * StateLayerSteps, MidpointRounding.ToEven)]).IfNone(start));

    internal static Seq<SKColorF> Ramped(Ramp ramp) =>
        toSeq(ramp.From.ToUnicolour().Palette(ramp.To.ToUnicolour(), ColourSpace.Oklab, StateLayerSteps + 1)).Map(colour => colour.Mapped(ramp.Gamut)).Strict();

    private Option<SKColorF> InteractionColor(ColorValue value, RoleStyle style, Interaction interaction) =>
        value is ColorValue.Host { Role.IsNone: true } host ? RoleColor(host, style, interaction).Map(color => Faded(color, host.Alpha)) : None;

    // --- [SERIES]
    private const double Distinct = 0.05d;
    private const double Vivid = 0.12d;

    private static readonly Seq<Cvd> Deficiencies = [Cvd.Deuteranopia, Cvd.Protanopia, Cvd.Tritanopia];
    private static readonly Seq<HueCycle> Cycles = [new(12, 5), new(11, 4), new(10, 3), new(9, 4), new(8, 3), new(7, 2)];
    private static readonly Func<Hues, Seq<SKColorF>> Categorical = memo<Hues, Seq<SKColorF>>(Distinguished);

    private Option<SKColorF> SeriesColor(DataSeries series, RoleStyle style) =>
        series.Color | series.Value.Match(
            Some: value => double.IsFinite(value) ? style.Look.Scale.Bind(scale => scale.Valued(value)) | Ordinal(series, style) : None,
            None: () => Ordinal(series, style));

    private Option<SKColorF> Ordinal(DataSeries series, RoleStyle style) =>
        style.Look.Palette.Filter(static palette => !palette.IsEmpty).Bind(palette => Color(palette[(int)(series.Ordinal % (uint)palette.Count)], style))
        | Color(DefaultStyles.Accent, style).Map(accent => Categorical(new Hues(accent, Configuration)) switch {
            var palette => palette[(int)(series.Ordinal % (uint)palette.Count)],
        });

    private static Seq<SKColorF> Distinguished(Hues hues) {
        (double lightness, double chroma, double hue) = hues.Accent.ToUnicolour().Oklch.Tuple;
        Seq<Seq<Unicolour>> palettes = Cycles.Map(cycle => toSeq(Enumerable.Range(0, cycle.Count))
            .Map(step => new Unicolour(ColourSpace.Oklch, lightness, double.Max(chroma, Vivid), ((double.IsNaN(hue) ? 0d : hue) + (step * cycle.Step)) % 360d)
                .ConvertToConfiguration(hues.Gamut).MapToRgbGamut(GamutMap.OklchChromaReduction)).Strict());
        return palettes.Find(static palette => Separation(palette) >= Distinct).IfNone(() => palettes.MaxBy(Separation)).Map(static colour => colour.ToSKColorF());
    }

    private static double Separation(Seq<Unicolour> palette) =>
        palette.AsIterable().Lead(1, palette[0], static (current, next) => Deficiencies.AsEnumerable().Min(cvd => current.Simulate(cvd).Difference(next.Simulate(cvd), DeltaE.Ok))).Min();
}

public sealed record StyleSheet : Monoid<StyleSheet> {
    public static StyleSheet Empty { get; } = new();

    public Option<Appearance> Appearance { get; init; }
    public Option<ControlSize> Size { get; init; }
    public HashMap<Role, Look> Looks { get; init; }
    public HashMap<Role, HashMap<Metric, float>> Metrics { get; init; }

    public StyleSheet Combine(StyleSheet rhs) =>
        new() {
            Appearance = rhs.Appearance | Appearance,
            Size = rhs.Size | Size,
            Looks = Looks.Union(rhs.Looks, Merge: static (_, under, over) => under.Combine(over)),
            Metrics = Metrics.Union(rhs.Metrics, Merge: static (_, under, over) => under.Union(over, Merge: static (_, _, held) => held)),
        };
}

internal sealed record RoleStyle {
    // --- [DEFAULTS]
    private static readonly Seq<Material> NativeFills = [new Material.Solid(new ColorValue.Host(Channel.Fill))];
    private static readonly Seq<Stroke> NativeStrokes = [new Stroke(new ColorValue.Host(Channel.Stroke))];
    private static readonly ColorValue NativeText = new ColorValue.Host(Channel.Text);
    private static readonly SKBlender SourceOver = SKBlender.CreateBlendMode(SKBlendMode.SrcOver);
    private static readonly TimingCurve Instant = new TimingCurve.Instant();
    private static readonly Shape RoundedShape = new Shape.Rounded();
    private static readonly SKColorF Probe = new(0f, 0f, 0f, 1f);
    private static readonly FilterTarget Probed = new(SKColorSpace.CreateSrgb(), SKRect.Create(1f, 1f), static _ => None, static _ => SKShader.CreateEmpty());

    private static readonly Func<Seq<Shadow>, float> ShadowOutset =
        memo<Seq<Shadow>, float>(static shadows => shadows.Filter(static shadow => !shadow.Inside).Fold(0f, static (reach, shadow) => float.Max(reach, Outset(shadow.Filtered(Probe, Probed.Space)))));

    private static readonly Func<Seq<Filter>, float> FilterOutset = memo<Seq<Filter>, float>(static filters => Filter.Chained(filters, Probed).Map(Outset).IfNone(0f));

    // --- [CONSTRUCTION]
    internal RoleStyle(Role role, HashSet<Condition> conditions, Look look, HashMap<Metric, float> metrics, bool dimmed, Option<RoleStyle> enclosing) {
        Role = role;
        Enclosing = enclosing;
        Conditions = conditions;
        Look = look;
        Metrics = metrics;
        Dimmed = dimmed;
        Shown = dimmed ? conditions.Remove(Condition.Disabled) : conditions;
        UnderGlass = enclosing.Exists(static held => held.UnderGlass || held.Glass.IsSome);
        Overhang = float.Max(HalfStroke, ShadowOutset(Shadows)) + FilterOutset(Filters);
    }

    // --- [RESOLVED]
    internal Role Role { get; }
    internal Option<RoleStyle> Enclosing { get; }
    internal HashSet<Condition> Conditions { get; }
    internal Look Look { get; }
    internal HashMap<Metric, float> Metrics { get; }
    internal Option<float> Rounded { get; init; }
    internal bool Dimmed { get; }
    internal HashSet<Condition> Shown { get; }
    internal bool UnderGlass { get; }
    internal float Overhang { get; }
    internal Configuration Gamut { get; init; } = Configuration.Default;

    // --- [CHANNELS]
    internal Seq<Material> Fills => UnderGlass ? Painted.Filter(static fill => fill is not Material.Glass) : Painted;
    internal Seq<Stroke> Strokes => Look.Strokes.IfNone(NativeStrokes);
    internal ColorValue Text => Look.Text.IfNone(NativeText);
    internal Seq<Shadow> Shadows => Look.Shadows.IfNone([]);
    internal Seq<Filter> Filters => Look.Filters.IfNone([]);
    internal SKBlender Blend => Look.Blend.IfNone(SourceOver);
    internal Shape Shape => Look.Shape.IfNone(RoundedShape);
    internal TimingCurve Motion => Look.Motion.IfNone(Instant);
    internal Option<ColorValue> Solid => Fills.Choose(static fill => fill.BaseColor).Head;

    internal Option<Material.Glass> Glass => Fills.Choose(static fill => fill is Material.Glass glass ? Some(glass) : None).Head;

    internal Option<ColorValue> Tint => toSeq(Fills.SkipWhile(static fill => fill is not Material.Glass)).Tail.Choose(static fill => fill.BaseColor).Head;

    internal bool Timed => Fills.Exists(static fill => fill.Timed) || Strokes.Exists(static stroke => stroke.Timed);

    internal bool Pointing => Fills.Exists(static fill => fill.Pointing);

    internal float HalfStroke => Strokes.IsEmpty ? 0f : Metrics[Metric.Stroke] / 2f;

    private Seq<Material> Painted => Look.Fills.IfNone(NativeFills);

    private static float Outset(SKImageFilter graph) {
        using SKImageFilter owned = graph;
        using SKPaint probe = new() { ImageFilter = owned };
        _ = probe.GetFastBounds(SKRect.Create(1f, 1f), out SKRect bounds);
        return float.Max(float.Max(-bounds.Left, -bounds.Top), float.Max(bounds.Right - 1f, bounds.Bottom - 1f));
    }
}

internal sealed record Cascade(
    ColorScheme Theme,
    DefaultStyles Defaults,
    Seq<HashMap<Role, Look>> Appearances,
    Seq<HashMap<Role, HashMap<Metric, float>>> Metrics,
    RoleStyle Enclosing,
    SKColorF Background);

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class ColorCache {
    internal Func<Legibility, SKColorF> Lifted { get; } = memo<Legibility, SKColorF>(Styles.Lift);
    internal Func<Ramp, Seq<SKColorF>> Ramped { get; } = memo<Ramp, Seq<SKColorF>>(ColorScheme.Ramped);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Styles {
    // --- [RESOLVE]
    private static readonly HashMap<Metric, float> Defaulted = toHashMap(Metric.Items.Select(static metric => (metric, metric.Default)));

    internal static Cascade Root(ColorScheme theme, DefaultStyles defaults, Role root) {
        Seq<HashMap<Role, Look>> appearances = defaults.Appearances.Find(Appearance.Native).ToSeq();
        Seq<HashMap<Role, HashMap<Metric, float>>> metrics = defaults.Sizes.Find(ControlSize.Regular).ToSeq();
        return new(theme, defaults, appearances, metrics, Resolved(theme, appearances, metrics, root, []), theme.Background);
    }

    internal static RoleStyle Resolve(Cascade cascade, Role role, HashSet<Condition> conditions) =>
        Resolved(cascade.Theme, cascade.Appearances, cascade.Metrics, role, conditions, cascade.Enclosing);

    internal static Cascade Enter(Cascade cascade, Node node) {
        Cascade pushed = node.StyledNode.Map(static styled => styled.Sheet).Map(sheet => cascade with {
            Appearances = cascade.Appearances + sheet.Appearance.Bind(name => cascade.Defaults.Appearances.Find(name)).ToSeq() + Seq(sheet.Looks),
            Metrics = cascade.Metrics + sheet.Size.Bind(name => cascade.Defaults.Sizes.Find(name)).ToSeq() + Seq(sheet.Metrics),
        }).IfNone(cascade);
        RoleStyle style = (node.Role | (node.Conditions.IsEmpty ? None : Some(cascade.Enclosing.Role)))
            .Map(role => Resolve(pushed, role, node.Role.IsNone || node.Painting.Inherits ? node.Conditions.Union(cascade.Enclosing.Conditions) : node.Conditions))
            .IfNone(cascade.Enclosing);
        return pushed with {
            Appearances = node.Role.IsSome && !style.Look.Within.IsEmpty ? pushed.Appearances.Add(style.Look.Within) : pushed.Appearances,
            Enclosing = style,
            Background = Background(cascade, style, node),
        };
    }

    internal static SKColorF Background(Cascade cascade, RoleStyle style, Node node) =>
        node.Role.IsSome && node.Painting.AcceptsBackground
            ? style.Solid.Bind(value => cascade.Theme.Color(value, style)).Map(color => color.ToUnicolour().Blend(cascade.Background.ToUnicolour(), BlendMode.Normal).ToSKColorF()).IfNone(cascade.Background)
            : cascade.Background;

    private static RoleStyle Resolved(ColorScheme theme, Seq<HashMap<Role, Look>> appearances, Seq<HashMap<Role, HashMap<Metric, float>>> metrics,
                                      Role role, HashSet<Condition> conditions, Option<RoleStyle> enclosing = default) {
        bool dimmed = enclosing.Exists(static held => held.Dimmed);
        HashSet<Condition> effective = conditions.Union(enclosing.Map(static held => held.Conditions.Filter(static condition => condition.Inherited)).IfNone([])).Union(theme.Conditions);
        HashSet<Condition> shown = dimmed ? effective.Remove(Condition.Disabled) : effective;
        Seq<Look> rows = appearances.Choose(appearance => appearance.Find(role));
        Seq<HashMap<Metric, float>> sized = metrics.Choose(layer => layer.Find(role));
        Look look = Monoid.combine(rows + toSeq(Condition.Items).Filter(shown.Contains).Bind(condition => rows.Choose(row => row.When.Find(condition))));
        bool stated = rows.Exists(static row => row.When.ContainsKey(Condition.Disabled))
            || toSeq(Channel.Items).Exists(channel => theme.Chrome.ContainsKey(new Token(role, channel) { Condition = Condition.Disabled }));
        return new RoleStyle(role, effective,
            toSeq(Condition.Items).Filter(effective.Contains).Fold(look, static (held, condition) => condition.Applied(held)),
            sized.Fold(Defaulted, static (under, row) => under.Union(row, Merge: static (_, _, over) => over)),
            dimmed || (effective.Contains(Condition.Disabled) && !stated),
            enclosing) { Rounded = sized.Choose(static row => row.Find(Metric.Radius)).Last, Gamut = theme.Configuration };
    }

    // --- [LEGIBILITY]
    private const double Accuracy = 1e-6;

    internal static SKColorF Legible(ColorCache colorCache, RoleStyle style, SKColorF background, SKColorF color, double ratio) =>
        style.Conditions.Contains(Condition.Disabled) ? color : colorCache.Lifted(new Legibility(color, background, ratio, style.Gamut));

    public static double Ratio(SKColorF color, SKColorF background) => color.ToUnicolour().Blend(background.WithAlpha(1f).ToUnicolour(), BlendMode.Normal).Contrast(background.WithAlpha(1f).ToUnicolour());

    internal static SKColorF Lift(Legibility held) {
        if (Ratio(held.Color, held.Ground) >= held.Ratio) return held.Color;
        (double lightness, double chroma, double hue) = held.Color.ToUnicolour().Oklch.Tuple;
        double floor = held.Ground.WithAlpha(1f).ToUnicolour().Oklch.L;
        Seq<(double From, double To)> spans = lightness >= floor ? [(lightness, 1d), (floor, 0d)] : [(lightness, 0d), (floor, 1d)];
        return spans.Find(span => Margin(span.From) < 0d && Margin(span.To) >= 0d).Bind(span => Brent.TryFindRoot(Margin, span.From, span.To, Accuracy, 100, out double shade) ? Some(At(shade)) : None)
            .IfNone(() => spans.Map(static span => span.To).Map(At).MaxBy(color => Ratio(color, held.Ground)));

        SKColorF At(double shade) => new Unicolour(ColourSpace.Oklch, shade, chroma, double.IsNaN(hue) ? 0d : hue, held.Color.Alpha).Mapped(held.Gamut);

        double Margin(double shade) => Ratio(At(shade), held.Ground) - held.Ratio - Accuracy;
    }
}
