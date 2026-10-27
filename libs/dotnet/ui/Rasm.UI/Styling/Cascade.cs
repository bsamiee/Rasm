using System.Numerics;
using MathNet.Numerics.RootFinding;
using Rasm.UI.Typography;
using SkiaSharp;
using Wacton.Unicolour;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record StateLayer(Interaction Interaction, float Weight);

public sealed record Token(Role Role, Channel Channel) {
    public Option<Condition> Condition { get; init; }
    public Option<Interaction> Interaction { get; init; }
}

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class ColorScheme {
    private static readonly Token WindowFill = new(Role.Window, Channel.Fill);
    private static readonly Token WindowText = new(Role.Window, Channel.Text);
    private static readonly Token WindowDisabled = new(Role.Window, Channel.Text) { Condition = Condition.Disabled };
    private static readonly Seq<Token> Required = [WindowFill, WindowText, WindowDisabled];

    private static readonly HashMap<Token, SKColorF> Signals = HashMap(
        (new Token(Role.Window, Channel.Text) { Condition = Condition.Warning }, new Unicolour(ColourSpace.Oklch, 0.77, 0.17, 64).ToSKColorF()),
        (new Token(Role.Window, Channel.Text) { Condition = Condition.Faulted }, new Unicolour(ColourSpace.Oklch, 0.63, 0.24, 29).ToSKColorF()));

    public HashMap<Token, SKColorF> Chrome { get; }
    public HashSet<Condition> Conditions { get; }
    public TextFace Face { get; }
    public float FontSize { get; }

    public SKColorF Background => Chrome[WindowFill];

    public ContrastLevel Contrast => Conditions.Contains(Condition.IncreaseContrast) ? ContrastLevel.Enhanced : ContrastLevel.Minimum;

    public float DisabledOpacity {
        get {
            Vector3 text = Offset(Chrome[WindowText], Chrome[WindowFill]);
            float span = Vector3.Dot(text, text);
            return span > 0f ? float.Clamp(Vector3.Dot(text, Offset(Chrome[WindowDisabled], Chrome[WindowFill])) / span, 0f, 1f) : 1f;
        }
    }

    public static Fin<ColorScheme> Validated(HashMap<Token, SKColorF> chrome, HashSet<Condition> conditions, TextFace face, float fontSize) {
        Seq<Token> missing = Required.Filter(token => !chrome.ContainsKey(token));
        return (UIFault.Unless(missing.IsEmpty, UIReason.Missing, missing), UIFault.Unless(face.Family.IsSome, UIReason.Missing, face.Family), UIFault.Unless(!conditions.Contains(Condition.Dark), UIReason.Derived, conditions))
            .Apply((_, _, _) => new ColorScheme(chrome.TryAddRange(Signals.AsIterable()), Darkened(chrome, conditions), face, fontSize)).As();
    }

    private static HashSet<Condition> Darkened(HashMap<Token, SKColorF> chrome, HashSet<Condition> conditions) =>
        chrome[WindowFill].ToUnicolour().Oklch.L < chrome[WindowText].ToUnicolour().Oklch.L ? conditions.Add(Condition.Dark) : conditions;

    // --- [COLORS]
    public Option<SKColorF> Color(ColorValue value, RoleStyle style, Option<DataSeries> series = default) =>
        value.Switch(
            (Theme: this, Style: style, Series: series),
            @fixed: static (_, @fixed) => Some(@fixed.Color.ToSKColorF()),
            host: static (held, host) => held.Theme.HostColor(host, held.Style, held.Series),
            data: static (held, data) => held.Series.Bind(series => series.Color | held.Style.Look.Palette.Filter(static palette => !palette.IsEmpty)
                    .Bind(palette => held.Theme.Color(palette[(int)(series.Ordinal % (uint)palette.Count)], held.Style)))
                .Map(color => Faded(color, data.Alpha)),
            mixed: static (held, mixed) =>
                from start in held.Theme.Color(mixed.From, held.Style, held.Series)
                from end in held.Theme.Color(mixed.To, held.Style, held.Series)
                select start.ToUnicolour().Mix(end.ToUnicolour(), mixed.Space, (float)mixed.Amount, mixed.Hue).MapToRgbGamut(GamutMap.OklchChromaReduction).ToSKColorF(),
            contrasting: static (held, contrasting) => held.Theme.Color(contrasting.Over, held.Style, held.Series).Bind(over =>
                contrasting.Candidates.Choose(candidate => held.Theme.Color(candidate, held.Style, held.Series))
                    .Fold(Option<SKColorF>.None, (best, next) => best.Filter(kept => Styles.Ratio(kept, over) >= Styles.Ratio(next, over)) | Some(next))),
            relative: static (held, relative) => held.Theme.Color(relative.From, held.Style, held.Series).Map(relative.Shifted));

    internal Option<SKColorF> WithStateLayers(ColorValue value, RoleStyle style, Seq<StateLayer> stateLayers, HashMap<Interaction, ColorValue> targets = default, Option<DataSeries> series = default) =>
        stateLayers.Fold(Color(value, style, series), (rest, stateLayer) =>
            from start in rest
            select (InteractionColor(value, style, stateLayer.Interaction) | targets.Find(stateLayer.Interaction).Bind(target => Color(target, style, series)))
                .Map(end => Ramps(new Ramp(start, end))[(int)MathF.Round(stateLayer.Weight * StateLayerSteps, MidpointRounding.ToEven)]).IfNone(start));

    private const int StateLayerSteps = 16;

    private sealed record Ramp(SKColorF From, SKColorF To);

    private static readonly Func<Ramp, Seq<SKColorF>> Ramps =
        memo<Ramp, Seq<SKColorF>>(static pair =>
            toSeq(pair.From.ToUnicolour().Palette(pair.To.ToUnicolour(), ColourSpace.Oklab, StateLayerSteps + 1)).Map(static colour => colour.MapToRgbGamut(GamutMap.OklchChromaReduction).ToSKColorF()).Strict());

    private Option<SKColorF> InteractionColor(ColorValue value, RoleStyle style, Interaction interaction) =>
        value is ColorValue.Host { Role.IsNone: true } host ? RoleColor(host, style, interaction).Map(color => Faded(color, host.Alpha)) : None;

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

    private static Vector3 Offset(SKColorF color, SKColorF background) => new Vector3(color.Red - background.Red, color.Green - background.Green, color.Blue - background.Blue) * color.Alpha;
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

public sealed record RoleStyle {
    private static readonly Seq<Material> NativeFills = [new Material.Solid(new ColorValue.Host(Channel.Fill))];
    private static readonly Seq<Stroke> NativeStrokes = [new Stroke(new ColorValue.Host(Channel.Stroke))];
    private static readonly ColorValue NativeText = new ColorValue.Host(Channel.Text);
    private static readonly SKColorF Probe = new(0f, 0f, 0f, 1f);

    private static readonly Func<Seq<Shadow>, float> ShadowOutset =
        memo<Seq<Shadow>, float>(static shadows => shadows.Filter(static shadow => !shadow.Inside).Fold(0f, static (reach, shadow) => float.Max(reach, Outset(shadow.Filtered(Probe)))));

    private static readonly Func<Seq<Filter>, float> FilterOutset = memo<Seq<Filter>, float>(static filters => Filter.Chained(filters).Map(Outset).IfNone(0f));

    public RoleStyle(Role role, HashSet<Condition> conditions, Look look, HashMap<Metric, float> metrics, bool veiled, Option<RoleStyle> enclosing) {
        Role = role;
        Enclosing = enclosing;
        Conditions = conditions;
        Look = look;
        Metrics = metrics;
        Dimmed = veiled;
        Shown = veiled ? conditions.Remove(Condition.Disabled) : conditions;
        Overhang = float.Max(HalfStroke, ShadowOutset(Shadows)) + FilterOutset(Filters);
    }

    public Role Role { get; }
    public Option<RoleStyle> Enclosing { get; }
    public HashSet<Condition> Conditions { get; }
    public Look Look { get; }
    public HashMap<Metric, float> Metrics { get; }
    public Option<float> Rounded { get; init; }
    public bool Dimmed { get; }
    public HashSet<Condition> Shown { get; }
    public float Overhang { get; }

    public Seq<Material> Fills => Look.Fills.IfNone(NativeFills);
    public Seq<Stroke> Strokes => Look.Strokes.IfNone(NativeStrokes);
    public ColorValue Text => Look.Text.IfNone(NativeText);
    public Seq<Shadow> Shadows => Look.Shadows.IfNone([]);
    public Seq<Filter> Filters => Look.Filters.IfNone([]);
    public SKBlendMode Blend => Look.Blend.IfNone(SKBlendMode.SrcOver);
    public TimingCurve Motion => Look.Motion.IfNone(static () => new TimingCurve.Instant());
    public Option<ColorValue> Solid => Fills.Choose(static fill => fill.BaseColor).Head;

    internal float HalfStroke => Strokes.IsEmpty ? 0f : Metrics[Metric.Stroke] / 2f;

    private static float Outset(SKImageFilter graph) {
        using SKImageFilter owned = graph;
        using SKPaint probe = new() { ImageFilter = owned };
        _ = probe.GetFastBounds(SKRect.Create(1f, 1f), out SKRect bounds);
        return float.Max(float.Max(-bounds.Left, -bounds.Top), float.Max(bounds.Right - 1f, bounds.Bottom - 1f));
    }
}

public sealed record Cascade(
    ColorScheme Theme,
    DefaultStyles Defaults,
    Seq<HashMap<Role, Look>> Appearances,
    Seq<HashMap<Role, HashMap<Metric, float>>> Metrics,
    RoleStyle Enclosing,
    SKColorF Background);

public sealed record Entered(RoleStyle Style, Cascade Inner);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Styles {
    // --- [RESOLVE]
    private static readonly HashMap<Metric, float> Defaulted = toHashMap(Metric.Items.Select(static metric => (metric, metric.Default)));

    public static Cascade Root(ColorScheme theme, DefaultStyles defaults, Role root) {
        Seq<HashMap<Role, Look>> appearances = defaults.Appearances.Find(Appearance.Native).ToSeq();
        Seq<HashMap<Role, HashMap<Metric, float>>> metrics = defaults.Sizes.Find(ControlSize.Regular).ToSeq();
        return new(theme, defaults, appearances, metrics, Resolved(theme, appearances, metrics, root, []), theme.Background);
    }

    public static RoleStyle Resolve(Cascade cascade, Role role, HashSet<Condition> conditions) =>
        Resolved(cascade.Theme, cascade.Appearances, cascade.Metrics, role, conditions, cascade.Enclosing);

    public static Entered Enter(Cascade cascade, Node node) {
        Cascade pushed = node.StyledNode.Map(static styled => styled.Sheet).Map(sheet => cascade with {
            Appearances = cascade.Appearances + sheet.Appearance.Bind(name => cascade.Defaults.Appearances.Find(name)).ToSeq() + Seq(sheet.Looks),
            Metrics = cascade.Metrics + sheet.Size.Bind(name => cascade.Defaults.Sizes.Find(name)).ToSeq() + Seq(sheet.Metrics),
        }).IfNone(cascade);
        RoleStyle style = (node.Role | (node.Conditions.IsEmpty ? None : Some(cascade.Enclosing.Role)))
            .Map(role => Resolve(pushed, role, node.Role.IsNone || node.Painting.Inherits ? node.Conditions.Union(cascade.Enclosing.Conditions) : node.Conditions))
            .IfNone(cascade.Enclosing);
        return new Entered(style, pushed with {
            Appearances = node.Role.IsSome && !style.Look.Within.IsEmpty ? pushed.Appearances.Add(style.Look.Within) : pushed.Appearances,
            Enclosing = style,
            Background = Background(cascade, style, node),
        });
    }

    internal static SKColorF Background(Cascade cascade, RoleStyle style, Node node) =>
        node.Role.IsSome && node.Painting.AcceptsBackground
            ? style.Solid.Bind(value => cascade.Theme.Color(value, style)).Map(color => color.ToUnicolour().Blend(cascade.Background.ToUnicolour(), BlendMode.Normal).ToSKColorF()).IfNone(cascade.Background)
            : cascade.Background;

    private static RoleStyle Resolved(ColorScheme theme, Seq<HashMap<Role, Look>> appearances, Seq<HashMap<Role, HashMap<Metric, float>>> metrics,
                                      Role role, HashSet<Condition> conditions, Option<RoleStyle> enclosing = default) {
        bool veiled = enclosing.Exists(static held => held.Dimmed);
        HashSet<Condition> effective = conditions.Union(enclosing.Map(static held => held.Conditions.Filter(static condition => condition.Inherited)).IfNone([])).Union(theme.Conditions);
        HashSet<Condition> shown = veiled ? effective.Remove(Condition.Disabled) : effective;
        Seq<Look> rows = appearances.Choose(appearance => appearance.Find(role));
        Seq<HashMap<Metric, float>> sized = metrics.Choose(layer => layer.Find(role));
        Look look = Monoid.combine(rows + toSeq(Condition.Items).Filter(shown.Contains).Bind(condition => rows.Choose(row => row.When.Find(condition))));
        bool stated = rows.Exists(static row => row.When.ContainsKey(Condition.Disabled))
            || toSeq(Channel.Items).Exists(channel => theme.Chrome.ContainsKey(new Token(role, channel) { Condition = Condition.Disabled }));
        return new RoleStyle(role, effective,
            toSeq(Condition.Items).Filter(effective.Contains).Fold(look, static (held, condition) => condition.Applied(held)),
            sized.Fold(Defaulted, static (under, row) => under.Union(row, Merge: static (_, _, over) => over)),
            veiled || (effective.Contains(Condition.Disabled) && !stated),
            enclosing) { Rounded = sized.Choose(static row => row.Find(Metric.Radius)).Last };
    }

    // --- [LEGIBILITY]
    private sealed record Legibility(SKColorF Color, SKColorF Ground, double Ratio);

    private static readonly Func<Legibility, SKColorF> Lifts = memo<Legibility, SKColorF>(Lift);

    public static SKColorF Legible(RoleStyle style, SKColorF background, SKColorF color, double ratio) =>
        style.Conditions.Contains(Condition.Disabled) ? color : Lifts(new Legibility(color, background, ratio));

    public static double Ratio(SKColorF color, SKColorF background) => color.ToUnicolour().Blend(background.WithAlpha(1f).ToUnicolour(), BlendMode.Normal).Contrast(background.WithAlpha(1f).ToUnicolour());

    private const double Accuracy = 1e-6;

    private static SKColorF Lift(Legibility held) {
        if (Ratio(held.Color, held.Ground) >= held.Ratio) return held.Color;
        (double lightness, double chroma, double hue) = held.Color.ToUnicolour().Oklch.Tuple;
        double floor = held.Ground.WithAlpha(1f).ToUnicolour().Oklch.L;
        Seq<(double From, double To)> spans = lightness >= floor ? [(lightness, 1d), (floor, 0d)] : [(lightness, 0d), (floor, 1d)];
        return spans.Find(span => Margin(span.To) >= 0d).Bind(span => Bisection.TryFindRoot(Margin, span.From, span.To, Accuracy, 100, out double shade) ? Some(At(shade)) : None)
            .IfNone(() => spans.Map(static span => span.To).Map(At).MaxBy(color => Ratio(color, held.Ground)));

        SKColorF At(double shade) => new Unicolour(ColourSpace.Oklch, shade, chroma, double.IsNaN(hue) ? 0d : hue, held.Color.Alpha).MapToRgbGamut(GamutMap.OklchChromaReduction).ToSKColorF();

        double Margin(double shade) => Ratio(At(shade), held.Ground) - held.Ratio - Accuracy;
    }
}
