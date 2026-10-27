using System.Numerics;
using MathNet.Numerics.RootFinding;
using SkiaSharp;
using Wacton.Unicolour;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Token(Role Role, Channel Channel) {
    public Option<Condition> Condition { get; init; }
    public Option<Interaction> Interaction { get; init; }
}

public sealed record Ground(SKColorF Under, bool Opaque);

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class ColorScheme {
    private static readonly Token WindowFill = new(Role.Window, Channel.Fill);
    private static readonly Token WindowText = new(Role.Window, Channel.Text);
    private static readonly Token WindowDisabled = new(Role.Window, Channel.Text) { Condition = Condition.Disabled };
    private static readonly Seq<Token> Required = [WindowFill, WindowText, WindowDisabled];

    private static readonly Seq<(Token Token, SKColorF Color)> Signals = [
        (new Token(Role.Window, Channel.Text) { Condition = Condition.Warning }, new Unicolour(ColourSpace.Oklch, 0.77, 0.17, 64).ToSKColorF()),
        (new Token(Role.Window, Channel.Text) { Condition = Condition.Faulted }, new Unicolour(ColourSpace.Oklch, 0.63, 0.24, 29).ToSKColorF()),
    ];

    public HashMap<Token, SKColorF> Chrome { get; }
    public LanguageExt.HashSet<Condition> Conditions { get; }
    public TextFace Face { get; }
    public float FontSize { get; }

    public Ground Ground => new(Chrome[WindowFill], Opaque: true);

    public ContrastLevel Contrast => Conditions.Contains(Condition.IncreaseContrast) ? ContrastLevel.Enhanced : ContrastLevel.Minimum;

    public float Veil =>
        (Offset(Chrome[WindowText], Chrome[WindowFill]), Offset(Chrome[WindowDisabled], Chrome[WindowFill])) switch {
            var (text, disabled) => (Span: Vector3.Dot(text, text), Moved: Vector3.Dot(text, disabled)) switch {
                { Span: > 0f } held => float.Clamp(held.Moved / held.Span, 0f, 1f),
                _ => 1f,
            },
        };

    public static Fin<ColorScheme> Validated(HashMap<Token, SKColorF> chrome, LanguageExt.HashSet<Condition> conditions, TextFace face, float fontSize) =>
        (UIFault.Unless(Required.ForAll(chrome.ContainsKey), UIReason.Missing), UIFault.Unless(face.Family.IsSome, UIReason.Missing), UIFault.Unless(!conditions.Contains(Condition.Dark), UIReason.Derived))
            .Apply((_, _, _) => new ColorScheme(chrome.TryAddRange(Signals), Darkened(chrome, conditions), face, fontSize)).As();

    private static LanguageExt.HashSet<Condition> Darkened(HashMap<Token, SKColorF> chrome, LanguageExt.HashSet<Condition> conditions) =>
        chrome[WindowFill].ToUnicolour().Oklch.L < chrome[WindowText].ToUnicolour().Oklch.L ? conditions.Add(Condition.Dark) : conditions;

    // --- [COLORS]
    public Option<SKColorF> Color(ColorValue value, RoleStyle style, Option<Series> series = default) =>
        value.Switch(
            (Theme: this, Style: style, Series: series),
            @fixed: static (_, @fixed) => Some(@fixed.Color.ToSKColorF()),
            host: static (held, host) => held.Theme.Hosted(host, held.Style, held.Series),
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

    internal Option<SKColorF> Washed(ColorValue value, RoleStyle style, Seq<(Interaction Interaction, float Weight)> washes, HashMap<Interaction, ColorValue> targets = default, Option<Series> series = default) =>
        washes.Fold(Color(value, style, series), (rest, wash) =>
            from start in rest
            select (Engaged(value, style, wash.Interaction) | targets.Find(wash.Interaction).Bind(target => Color(target, style, series)))
                .Map(end => Ramps((start, end))[(int)MathF.Round(wash.Weight * WashSteps, MidpointRounding.ToEven)]).IfNone(start));

    private const int WashSteps = 16;

    private static readonly Func<(SKColorF From, SKColorF To), Seq<SKColorF>> Ramps =
        memo<(SKColorF From, SKColorF To), Seq<SKColorF>>(static pair =>
            toSeq(pair.From.ToUnicolour().Palette(pair.To.ToUnicolour(), ColourSpace.Oklab, WashSteps + 1)).Map(static colour => colour.MapToRgbGamut(GamutMap.OklchChromaReduction).ToSKColorF()).Strict());

    private Option<SKColorF> Engaged(ColorValue value, RoleStyle style, Interaction interaction) =>
        value is ColorValue.Host { Role.IsNone: true } host ? Owned(host, style, interaction).Map(color => Faded(color, host.Alpha)) : None;

    private Option<SKColorF> Hosted(ColorValue.Host host, RoleStyle style, Option<Series> series) =>
        (host.Role.Match(Some: role => Chrome.Find(new Token(role, host.Channel) { Condition = host.Condition }), None: () => Owned(host, style))
            | (host.Role.IsNone && host.Fallback.IsNone && host.Channel.Inherited ? style.Enclosing.Bind(enclosing => Color(enclosing.Text, enclosing, series)) : None))
        .Map(color => Faded(color, host.Alpha))
        | host.Fallback.Bind(fallback => Color(fallback, style, series));

    private Option<SKColorF> Owned(ColorValue.Host host, RoleStyle style, Option<Interaction> interaction = default) =>
        host.Condition.Match(
                Some: static condition => Seq(Some(condition)),
                None: () => toSeq(Condition.Items).Rev().Filter(style.Shown.Contains).Map(static condition => Some(condition)) + Seq(Option<Condition>.None))
            .Choose(condition => Chrome.Find(new Token(style.Role, host.Channel) { Condition = condition, Interaction = interaction })).Head;

    private static SKColorF Faded(SKColorF color, Fraction alpha) => color.WithAlpha(color.Alpha * (float)alpha);

    private static Vector3 Offset(SKColorF color, SKColorF ground) => new Vector3(color.Red - ground.Red, color.Green - ground.Green, color.Blue - ground.Blue) * color.Alpha;
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

    private static readonly Func<Seq<Shadow>, float> DropReach =
        memo<Seq<Shadow>, float>(static shadows => shadows.Filter(static shadow => !shadow.Inside).Fold(0f, static (reach, shadow) => float.Max(reach, Beyond(shadow.Filtered(Probe)))));

    private static readonly Func<Seq<Filter>, float> FilterReach = memo<Seq<Filter>, float>(static filters => Filter.Chained(filters).Map(Beyond).IfNone(0f));

    public RoleStyle(Role role, LanguageExt.HashSet<Condition> conditions, Look look, HashMap<Metric, float> metrics, bool veiled, Option<RoleStyle> enclosing) {
        Role = role;
        Enclosing = enclosing;
        Conditions = conditions;
        Look = look;
        Metrics = metrics;
        Veiled = veiled;
        Shown = veiled ? conditions.Remove(Condition.Disabled) : conditions;
        Overhang = float.Max(HalfStroke, DropReach(Shadows)) + FilterReach(Filters);
    }

    public Role Role { get; }
    public Option<RoleStyle> Enclosing { get; }
    public LanguageExt.HashSet<Condition> Conditions { get; }
    public Look Look { get; }
    public HashMap<Metric, float> Metrics { get; }
    public Option<float> Rounded { get; init; }
    public bool Veiled { get; }
    public LanguageExt.HashSet<Condition> Shown { get; }
    public float Overhang { get; }

    public Seq<Material> Fills => Look.Fills.IfNone(NativeFills);
    public Seq<Stroke> Strokes => Look.Strokes.IfNone(NativeStrokes);
    public ColorValue Text => Look.Text.IfNone(NativeText);
    public Seq<Shadow> Shadows => Look.Shadows.IfNone([]);
    public Seq<Filter> Filters => Look.Filters.IfNone([]);
    public SKBlendMode Blend => Look.Blend.IfNone(SKBlendMode.SrcOver);
    public TimingCurve Motion => Look.Motion.IfNone(static () => new TimingCurve.Instant());
    public Option<ColorValue> Solid => Fills.Choose(static fill => fill.Under).Head;

    internal float HalfStroke => Strokes.IsEmpty ? 0f : Metrics[Metric.Stroke] / 2f;

    private static float Beyond(SKImageFilter graph) {
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
    Ground Ground);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Styles {
    // --- [RESOLVE]
    private static readonly HashMap<Metric, float> Defaulted = toHashMap(Metric.Items.Select(static metric => (metric, metric.Default)));

    public static Cascade Root(ColorScheme theme, DefaultStyles defaults, Role root) =>
        (Appearances: defaults.Appearances.Find(Appearance.Native).ToSeq(), Metrics: defaults.Sizes.Find(ControlSize.Regular).ToSeq()) switch {
            var held => new(theme, defaults, held.Appearances, held.Metrics, Resolved(theme, held.Appearances, held.Metrics, root, []), theme.Ground),
        };

    public static RoleStyle Resolve(Cascade cascade, Role role, LanguageExt.HashSet<Condition> conditions) =>
        Resolved(cascade.Theme, cascade.Appearances, cascade.Metrics, role, conditions, cascade.Enclosing);

    public static (RoleStyle Style, Cascade Inner) Enter(Cascade cascade, Node node) {
        Cascade pushed = node.Restyled.Map(static styled => styled.Sheet).Map(sheet => cascade with {
            Appearances = cascade.Appearances + sheet.Appearance.Bind(name => cascade.Defaults.Appearances.Find(name)).ToSeq() + Seq(sheet.Looks),
            Metrics = cascade.Metrics + sheet.Size.Bind(name => cascade.Defaults.Sizes.Find(name)).ToSeq() + Seq(sheet.Metrics),
        }).IfNone(cascade);
        RoleStyle style = (node.Role | (node.Conditions.IsEmpty ? None : Some(cascade.Enclosing.Role)))
            .Map(role => Resolve(pushed, role, node.Role.IsNone || node.Painting.Inherits ? node.Conditions.Union(cascade.Enclosing.Conditions) : node.Conditions))
            .IfNone(cascade.Enclosing);
        return (style, pushed with {
            Appearances = node.Role.IsSome && !style.Look.Within.IsEmpty ? pushed.Appearances.Add(style.Look.Within) : pushed.Appearances,
            Enclosing = style,
            Ground = Grounded(cascade, style, node),
        });
    }

    internal static Ground Grounded(Cascade cascade, RoleStyle style, Node node) =>
        node.Role.IsSome && node.Painting.AcceptsBackground
            ? style.Solid.Bind(value => cascade.Theme.Color(value, style))
                .Map(color => new Ground(color.ToUnicolour().Blend(cascade.Ground.Under.ToUnicolour(), BlendMode.Normal).ToSKColorF(), Opaque: color.Alpha >= 1f)).IfNone(cascade.Ground)
            : cascade.Ground;

    private static RoleStyle Resolved(ColorScheme theme, Seq<HashMap<Role, Look>> appearances, Seq<HashMap<Role, HashMap<Metric, float>>> metrics,
                                      Role role, LanguageExt.HashSet<Condition> conditions, Option<RoleStyle> enclosing = default) {
        bool veiled = enclosing.Exists(static held => held.Veiled);
        LanguageExt.HashSet<Condition> effective = conditions.Union(enclosing.Map(static held => held.Conditions.Filter(static condition => condition.Inherited)).IfNone([])).Union(theme.Conditions);
        LanguageExt.HashSet<Condition> shown = veiled ? effective.Remove(Condition.Disabled) : effective;
        Seq<Look> rows = appearances.Choose(appearance => appearance.Find(role));
        Seq<HashMap<Metric, float>> sized = metrics.Choose(layer => layer.Find(role));
        Look look = Monoid.combine(rows + toSeq(Condition.Items).Filter(shown.Contains).Bind(condition => rows.Choose(row => row.When.Find(condition))));
        bool stated = rows.Exists(static row => row.When.ContainsKey(Condition.Disabled))
            || toSeq(Channel.Items).Exists(channel => theme.Chrome.ContainsKey(new Token(role, channel) { Condition = Condition.Disabled }));
        return new RoleStyle(role, effective,
            toSeq(Condition.Items).Filter(effective.Contains).Fold(look, static (held, condition) => condition.Restyled(held)),
            sized.Fold(Defaulted, static (under, row) => under.Union(row, Merge: static (_, _, over) => over)),
            veiled || (effective.Contains(Condition.Disabled) && !stated),
            enclosing) { Rounded = sized.Choose(static row => row.Find(Metric.Radius)).Last };
    }

    // --- [LEGIBILITY]
    private static readonly Func<(SKColorF Color, SKColorF Ground, double Ratio), SKColorF> Lifts = memo<(SKColorF Color, SKColorF Ground, double Ratio), SKColorF>(Lift);

    public static SKColorF Legible(RoleStyle style, Ground ground, SKColorF color, double ratio) =>
        style.Conditions.Contains(Condition.Disabled) ? color : Lifts((color, ground.Under, ratio));

    public static double Ratio(SKColorF color, SKColorF ground) => color.ToUnicolour().Blend(ground.WithAlpha(1f).ToUnicolour(), BlendMode.Normal).Contrast(ground.WithAlpha(1f).ToUnicolour());

    private const double Accuracy = 1e-6;

    private static SKColorF Lift((SKColorF Color, SKColorF Ground, double Ratio) held) {
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
