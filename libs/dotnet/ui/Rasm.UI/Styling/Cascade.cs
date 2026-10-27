using System.Numerics;
using MathNet.Numerics.RootFinding;
using Rasm.UI.Typography;
using SkiaSharp;
using Wacton.Unicolour;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Token(Role Role, Channel Channel) {
    public Option<Condition> Condition { get; init; }
    public Option<Interaction> Interaction { get; init; }
}

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class Theme {
    // --- [THEME]
    private static readonly Token WindowFill = new(Role.Window, Channel.Fill);
    private static readonly Token WindowText = new(Role.Window, Channel.Text);
    private static readonly Token WindowDisabled = new(Role.Window, Channel.Text) { Condition = Condition.Disabled };

    public HashMap<Token, SKColorF> Chrome { get; }
    internal HashSet<Condition> Conditions { get; }
    public TextFace Face { get; }
    public float FontSize { get; }
    public Configuration Configuration { get; }

    public SKColorF Background => Chrome[WindowFill];

    public static Fin<Theme> Validated(SKColorF window, SKColorF text, SKColorF disabledText, HashMap<Token, SKColorF> chrome, HashSet<Condition> conditions, TextFace face, float fontSize, Option<Configuration> configuration = default) =>
        UIFault.Unless(face.Family.IsSome, UIReason.Missing, face.Family).Map(_ => new Theme(
            chrome.AddOrUpdate(WindowFill, window).AddOrUpdate(WindowText, text).AddOrUpdate(WindowDisabled, disabledText),
            window.ToUnicolour().Oklch.L < text.ToUnicolour().Oklch.L ? conditions.TryAdd(Condition.Dark) : conditions.Remove(Condition.Dark),
            face, fontSize, configuration.IfNone(Configuration.Default)));

    // --- [DISABLED]
    internal float DisabledOpacity(SKColorSpace target) {
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
    private static readonly Func<(Unicolour Color, Configuration Gamut), SKColorF> Gamuted = memo<(Unicolour Color, Configuration Gamut), SKColorF>(static held => held.Color.Mapped(held.Gamut));

    internal Option<SKColorF> Color(ColorValue value, RoleStyle style, Option<DataSeries> series = default) =>
        value.Switch(
            (Theme: this, Style: style, Series: series),
            @fixed: static (held, @fixed) => Some(Gamuted((@fixed.Color, held.Theme.Configuration))),
            host: static (held, host) => held.Theme.HostColor(host, held.Style, held.Series),
            data: static (held, data) => held.Series.Bind(series => held.Theme.SeriesColor(series, held.Style)).Map(color => Faded(color, data.Alpha)),
            mixed: static (held, mixed) =>
                from start in held.Theme.Color(mixed.From, held.Style, held.Series)
                from end in held.Theme.Color(mixed.To, held.Style, held.Series)
                select start.ToUnicolour().Mix(end.ToUnicolour(), mixed.Space, (float)mixed.Amount, mixed.Hue).Mapped(held.Theme.Configuration),
            contrasting: static (held, contrasting) => held.Theme.Color(contrasting.Over, held.Style, held.Series).Bind(over =>
                contrasting.Candidates.Choose(candidate => held.Theme.Color(candidate, held.Style, held.Series))
                    .Fold(Option<SKColorF>.None, (best, next) => best.Filter(kept => Styles.Ratio(kept, over) >= Styles.Ratio(next, over)) | Some(next))),
            relative: static (held, relative) => held.Theme.Color(relative.From, held.Style, held.Series).Map(origin => relative.Shifted(origin).Mapped(held.Theme.Configuration)));

    private Option<SKColorF> HostColor(ColorValue.Host host, RoleStyle style, Option<DataSeries> series) =>
        (host.Role.Match(Some: role => Chrome.Find(new Token(role, host.Channel) { Condition = host.Condition }), None: () => RoleColor(host, style))
            | (host.Role.IsNone && host.Fallback.IsNone && host.Channel.Inherited ? style.Enclosing.Bind(enclosing => Color(enclosing.Text, enclosing, series)) : None)
            | host.Fallback.Bind(fallback => Color(fallback, style, series)))
        .Map(color => Faded(color, host.Alpha));

    private Option<SKColorF> RoleColor(ColorValue.Host host, RoleStyle style, Option<Interaction> interaction = default) =>
        host.Condition.Match(
                Some: static condition => Seq(Some(condition)),
                None: () => toSeq(Condition.Items).Rev().Filter(style.Shown.Contains).Map(static condition => Some(condition)) + Seq(Option<Condition>.None))
            .Choose(condition => Chrome.Find(new Token(style.Role, host.Channel) { Condition = condition, Interaction = interaction })).Head;

    private static SKColorF Faded(SKColorF color, Fraction alpha) => color.WithAlpha(color.Alpha * (float)alpha);

    // --- [STATE_LAYERS]
    private const int StateLayerSteps = 16;

    private static readonly Func<(SKColorF From, SKColorF To, Configuration Gamut), Seq<SKColorF>> Ramped = memo<(SKColorF From, SKColorF To, Configuration Gamut), Seq<SKColorF>>(static ramp =>
        toSeq(ramp.From.ToUnicolour().Palette(ramp.To.ToUnicolour(), ColourSpace.Oklab, StateLayerSteps + 1)).Map(colour => colour.Mapped(ramp.Gamut)).Strict());

    internal Option<SKColorF> WithStateLayers(ColorValue value, RoleStyle style, HashMap<Interaction, float> layers, HashMap<Interaction, ColorValue> targets = default, Option<DataSeries> series = default) =>
        toSeq(Interaction.Items).Choose(interaction => layers.Find(interaction).Map(weight => (Interaction: interaction, Weight: weight))).Fold(Color(value, style, series), (rest, layer) =>
            from start in rest
            select (InteractionColor(value, style, layer.Interaction) | targets.Find(layer.Interaction).Bind(target => Color(target, style, series)))
                .Map(end => Ramped((start, end, Configuration))[(int)MathF.Round(layer.Weight * StateLayerSteps, MidpointRounding.ToEven)]).IfNone(start));

    private Option<SKColorF> InteractionColor(ColorValue value, RoleStyle style, Interaction interaction) =>
        value is ColorValue.Host { Role.IsNone: true } host ? RoleColor(host, style, interaction).Map(color => Faded(color, host.Alpha)) : None;

    // --- [SERIES]
    private static readonly Seq<Unicolour> Categorical = Seq("#0072B2", "#E69F00", "#009E73", "#CC79A7", "#56B4E9", "#D55E00", "#F0E442", "#999999").Map(static hex => new Unicolour(hex)).Strict();

    private Option<SKColorF> SeriesColor(DataSeries series, RoleStyle style) =>
        series.Color.Bind(color => Color(color, style)) | series.Value.Match(
            Some: value => double.IsFinite(value) ? style.Look.Scale.Bind(scale => scale.Valued(value)).Bind(color => Color(color, style)) | Ordinal(series, style) : None,
            None: () => Ordinal(series, style));

    private Option<SKColorF> Ordinal(DataSeries series, RoleStyle style) =>
        style.Look.Palette.Filter(static palette => !palette.IsEmpty).Bind(palette => Color(palette[(int)(series.Ordinal % (uint)palette.Count)], style))
        | Gamuted((Categorical[(int)(series.Ordinal % (uint)Categorical.Count)], Configuration));

    // --- [LEGIBILITY]
    private const double Accuracy = 1e-6;
    private const double TextFloor = 4.5d;
    private const double EnhancedTextFloor = 7d;
    private const double GraphicFloor = 3d;

    private static readonly Func<(SKColorF Color, SKColorF Ground, double Ratio, Configuration Gamut), SKColorF> Lifted =
        memo<(SKColorF Color, SKColorF Ground, double Ratio, Configuration Gamut), SKColorF>(Lift);

    internal double Floor(bool text) => text ? (Conditions.Contains(Condition.IncreaseContrast) ? EnhancedTextFloor : TextFloor) : GraphicFloor;

    internal SKColorF Legible(RoleStyle style, SKColorF under, SKColorF color, bool text) =>
        style.Conditions.Contains(Condition.Disabled) ? color : Lifted((color, under, Floor(text), Configuration));

    private static SKColorF Lift((SKColorF Color, SKColorF Ground, double Ratio, Configuration Gamut) held) {
        if (Styles.Ratio(held.Color, held.Ground) >= held.Ratio) return held.Color;
        (double lightness, double chroma, double hue) = held.Color.ToUnicolour().Oklch.Tuple;
        double floor = held.Ground.WithAlpha(1f).ToUnicolour().Oklch.L;
        Seq<(double From, double To)> spans = lightness >= floor ? [(lightness, 1d), (floor, 0d)] : [(lightness, 0d), (floor, 1d)];
        return spans.Find(span => Margin(span.From) < 0d && Margin(span.To) >= 0d).Bind(span => Brent.TryFindRoot(Margin, span.From, span.To, Accuracy, 100, out double shade) ? Some(At(shade)) : None)
            .IfNone(() => spans.Map(static span => span.To).Map(At).MaxBy(color => Styles.Ratio(color, held.Ground)));

        SKColorF At(double shade) => new Unicolour(ColourSpace.Oklch, shade, chroma, double.IsNaN(hue) ? 0d : hue, held.Color.Alpha).Mapped(held.Gamut);

        double Margin(double shade) => Styles.Ratio(At(shade), held.Ground) - held.Ratio - Accuracy;
    }
}

internal sealed record RoleStyle {
    // --- [DEFAULTS]
    private static readonly HashMap<Metric, float> Defaulted = toHashMap(Metric.Items.Select(static metric => (metric, metric.Default)));
    private static readonly Seq<Material> NativeFills = [new Material.Solid(new ColorValue.Host(Channel.Fill))];
    private static readonly Seq<Stroke> NativeStrokes = [new Stroke(new ColorValue.Host(Channel.Stroke))];
    private static readonly ColorValue NativeText = new ColorValue.Host(Channel.Text);
    private static readonly SKBlender SourceOver = SKBlender.CreateBlendMode(SKBlendMode.SrcOver);
    private static readonly TimingCurve Instant = new TimingCurve.Instant();
    private static readonly Shape RoundedShape = new Shape.Rounded();

    // --- [CONSTRUCTION]
    internal RoleStyle(Role role, HashSet<Condition> conditions, Look look, bool dimmed, Option<RoleStyle> enclosing) {
        Role = role;
        Enclosing = enclosing;
        Conditions = conditions;
        Look = look;
        Metrics = Defaulted.Union(look.Metrics, Merge: static (_, _, over) => over);
        Dimmed = dimmed;
        Shown = dimmed ? conditions.Remove(Condition.Disabled) : conditions;
        UnderGlass = enclosing.Exists(static held => held.UnderGlass || held.Glass.IsSome);
        Overhang = float.Max(HalfStroke, Shadow.Outset(Shadows)) + Filter.Outset(Filters);
    }

    // --- [RESOLVED]
    internal Role Role { get; }
    internal Option<RoleStyle> Enclosing { get; }
    internal HashSet<Condition> Conditions { get; }
    internal Look Look { get; }
    internal HashMap<Metric, float> Metrics { get; }
    internal bool Dimmed { get; }
    internal HashSet<Condition> Shown { get; }
    internal bool UnderGlass { get; }
    internal float Overhang { get; }

    internal Option<float> Rounded => Look.Metrics.Find(Metric.Radius);

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

    internal bool Timed => Fills.Exists(static fill => fill.Timed) || Strokes.Exists(static stroke => stroke.Timed) || Filters.Exists(static filter => filter.Timed);

    internal bool Pointing => Fills.Exists(static fill => fill.Pointing) || Filters.Exists(static filter => filter.Pointing);

    internal float HalfStroke => Strokes.IsEmpty ? 0f : Metrics[Metric.Stroke] / 2f;

    private Seq<Material> Painted => Look.Fills.IfNone(NativeFills);
}

internal sealed record Cascade(Theme Theme, StyleSheet Sheet, RoleStyle Enclosing, SKColorF Background);

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Styles {
    // --- [RESOLVE]
    internal static Cascade Root(Theme theme, StyleSheet sheet, Role root) => new(theme, sheet, Resolved(theme, sheet, root, [], None), theme.Background);

    internal static RoleStyle Resolve(Cascade cascade, Role role, HashSet<Condition> conditions) => Resolved(cascade.Theme, cascade.Sheet, role, conditions, cascade.Enclosing);

    internal static Cascade Enter(Cascade cascade, Node node) {
        Cascade pushed = node.Sheet.Looks.IsEmpty ? cascade : cascade with { Sheet = cascade.Sheet.Combine(node.Sheet) };
        RoleStyle style = (node.Role | (node.Conditions.IsEmpty ? None : Some(cascade.Enclosing.Role)))
            .Map(role => Resolve(pushed, role, node.Role.IsNone || node.Painting.Inherits ? node.Conditions.Union(cascade.Enclosing.Conditions) : node.Conditions))
            .IfNone(cascade.Enclosing);
        return pushed with {
            Sheet = node.Role.IsSome && !style.Look.Within.IsEmpty ? pushed.Sheet.Combine(new StyleSheet(style.Look.Within)) : pushed.Sheet,
            Enclosing = style,
            Background = Background(cascade, style, node),
        };
    }

    internal static SKColorF Background(Cascade cascade, RoleStyle style, Node node) =>
        node.Role.IsSome && node.Painting.AcceptsBackground
            ? style.Solid.Bind(value => cascade.Theme.Color(value, style)).Map(color => color.ToUnicolour().Blend(cascade.Background.ToUnicolour(), BlendMode.Normal).ToSKColorF()).IfNone(cascade.Background)
            : cascade.Background;

    internal static double Ratio(SKColorF color, SKColorF background) => color.ToUnicolour().Blend(background.WithAlpha(1f).ToUnicolour(), BlendMode.Normal).Contrast(background.WithAlpha(1f).ToUnicolour());

    private static RoleStyle Resolved(Theme theme, StyleSheet sheet, Role role, HashSet<Condition> conditions, Option<RoleStyle> enclosing) {
        bool dimmed = enclosing.Exists(static held => held.Dimmed);
        HashSet<Condition> effective = conditions.Union(enclosing.Map(static held => held.Conditions.Filter(static condition => condition.Inherited)).IfNone([])).Union(theme.Conditions);
        HashSet<Condition> shown = dimmed ? effective.Remove(Condition.Disabled) : effective;
        Look row = sheet.Looks.Find(role).IfNone(Look.Empty);
        Look look = toSeq(Condition.Items).Filter(shown.Contains).Choose(row.When.Find).Fold(row, static (held, over) => held.Combine(over));
        bool stated = row.When.ContainsKey(Condition.Disabled) || toSeq(Channel.Items).Exists(channel => theme.Chrome.ContainsKey(new Token(role, channel) { Condition = Condition.Disabled }));
        return new RoleStyle(role, effective,
            effective.Contains(Condition.ReduceTransparency) ? look with { Fills = look.Fills.Map(static fills => fills.Map(static fill => fill.Opaque)) } : look,
            dimmed || (effective.Contains(Condition.Disabled) && !stated),
            enclosing);
    }
}
