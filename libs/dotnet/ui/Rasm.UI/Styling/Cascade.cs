using System.Numerics;
using SkiaSharp;
using Wacton.Unicolour;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Token(Role Role, Channel Channel) {
    public Option<Condition> Condition { get; init; }
    public Option<Interaction> Interaction { get; init; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Ground {
    public SKColorF Under => Switch(opaque: static opaque => opaque.Color, sampled: static sampled => sampled.Estimate);

    public sealed record Opaque(SKColorF Color) : Ground;

    public sealed record Sampled(SKColorF Estimate) : Ground;
}

[ComplexValueObject]
[ValidationError<Invalid>]
public sealed partial class ColorScheme {
    private static readonly Token WindowFill = new(Role.Window, Channel.Fill);
    private static readonly Token WindowText = new(Role.Window, Channel.Text);
    private static readonly Token WindowDisabled = new(Role.Window, Channel.Text) { Condition = Condition.Disabled };

    public HashMap<Token, SKColorF> Chrome { get; }
    public LanguageExt.HashSet<Condition> Conditions { get; }
    public TextFace Face { get; }
    public float FontSize { get; }

    public Ground Ground => new Ground.Opaque(Chrome[WindowFill]);

    public ContrastLevel Contrast => Conditions.Contains(Condition.IncreaseContrast) ? ContrastLevel.Enhanced : ContrastLevel.Minimum;

    public float Veil =>
        (Offset(Chrome[WindowText], Chrome[WindowFill]), Offset(Chrome[WindowDisabled], Chrome[WindowFill])) switch {
            var (text, disabled) => (Span: Vector3.Dot(text, text), Moved: Vector3.Dot(text, disabled)) switch {
                { Span: > 0f } held => float.Clamp(held.Moved / held.Span, 0f, 1f),
                _ => 1f,
            },
        };

    static partial void ValidateFactoryArguments(
        ref Invalid? validationError, ref HashMap<Token, SKColorF> chrome, ref LanguageExt.HashSet<Condition> conditions, ref TextFace face, ref float fontSize) {
        HashMap<Token, SKColorF> stated = chrome;
        Seq<Token> missing = Seq(WindowFill, WindowText, WindowDisabled).Filter(token => !stated.ContainsKey(token));
        if (!missing.IsEmpty) {
            validationError = new Invalid($"theme chrome misses {string.Join(", ", missing.Map(static token => $"{token.Role}.{token.Channel.Key}{token.Condition.Map(static condition => $":{condition.Key}").IfNone("")}"))}");
            return;
        }
        if (face.Family.IsNone) {
            validationError = new Invalid("theme face states no family");
            return;
        }
        if (conditions.Contains(Condition.Dark)) {
            validationError = new Invalid($"theme conditions state {Condition.Dark.Key}, which the window fill and text tokens derive");
            return;
        }
        conditions = chrome[WindowFill].ToUnicolour().Oklch.L < chrome[WindowText].ToUnicolour().Oklch.L ? conditions.Add(Condition.Dark) : conditions;
    }

    // --- [COLORS]
    public Option<SKColorF> Color(ColorValue value, RoleStyle style, Option<Interaction> interaction, Option<Series> series) =>
        interaction.Match(
            Some: engaged => value.SwitchPartially(
                (Theme: this, Style: style, Interaction: engaged),
                @default: static (_, _) => Option<SKColorF>.None,
                host: static (held, host) => host.Role.IsNone ? held.Theme.Owned(host, held.Style, Some(held.Interaction)).Map(color => Faded(color, host.Alpha)) : None),
            None: () => Rest(value, style, series));

    private Option<SKColorF> Rest(ColorValue value, RoleStyle style, Option<Series> series) =>
        value.Switch(
            (Theme: this, Style: style, Series: series),
            @fixed: static (_, @fixed) => Some(@fixed.Color.ToSKColorF()),
            host: static (held, host) => held.Theme.Hosted(host, held.Style, held.Series),
            data: static (held, data) => held.Series.Bind(series => series.Color | held.Style.Look.Palette.Filter(static palette => !palette.IsEmpty)
                    .Bind(palette => held.Theme.Rest(palette[(int)(series.Ordinal % (uint)palette.Count)], held.Style, None)))
                .Map(color => Faded(color, data.Alpha)),
            mixed: static (held, mixed) =>
                from start in held.Theme.Rest(mixed.From, held.Style, held.Series)
                from end in held.Theme.Rest(mixed.To, held.Style, held.Series)
                select start.ToUnicolour().Mix(end.ToUnicolour(), mixed.Space, (float)mixed.Amount, mixed.Hue).MapToRgbGamut(GamutMap.OklchChromaReduction).ToSKColorF(),
            contrasting: static (held, contrasting) => held.Theme.Rest(contrasting.Over, held.Style, held.Series).Bind(over =>
                contrasting.Candidates.Choose(candidate => held.Theme.Rest(candidate, held.Style, held.Series))
                    .Fold(Option<SKColorF>.None, (best, next) => best.Filter(kept => Styles.Ratio(kept, over) >= Styles.Ratio(next, over)) | Some(next))),
            relative: static (held, relative) => held.Theme.Rest(relative.From, held.Style, held.Series).Map(relative.Shifted));

    private Option<SKColorF> Hosted(ColorValue.Host host, RoleStyle style, Option<Series> series) =>
        (host.Role.Match(Some: role => Chrome.Find(new Token(role, host.Channel) { Condition = host.Condition }), None: () => Owned(host, style, None))
            | (host.Role.IsNone && host.Fallback.IsNone && host.Channel == Channel.Text ? style.Enclosing.Bind(enclosing => Rest(enclosing.Text, enclosing, series)) : None))
        .Map(color => Faded(color, host.Alpha))
        | host.Fallback.Bind(fallback => Rest(fallback, style, series));

    private Option<SKColorF> Owned(ColorValue.Host host, RoleStyle style, Option<Interaction> interaction) =>
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
    private static readonly Seq<Material> NativeFills = [new Material.Solid(new ColorValue.Host(None, Channel.Fill))];
    private static readonly Seq<Stroke> NativeStrokes = [new Stroke(new ColorValue.Host(None, Channel.Stroke))];
    private static readonly ColorValue NativeText = new ColorValue.Host(None, Channel.Text);

    public RoleStyle(Role role, LanguageExt.HashSet<Condition> conditions, Look look, HashMap<Metric, float> metrics, bool veiled, Option<RoleStyle> enclosing) {
        Role = role;
        Enclosing = enclosing;
        Conditions = conditions;
        Look = look;
        Metrics = metrics;
        Veiled = veiled;
        Shown = veiled ? conditions.Remove(Condition.Disabled) : conditions;
        Overhang = Shadows.Fold(HalfStroke, static (reach, shadow) => float.Max(reach, shadow.Overhang)) switch {
            var reach => Filter.Chained(Filters).Match(
                Some: chain => {
                    using SKImageFilter held = chain;
                    using SKPaint paint = new() { ImageFilter = held };
                    return float.Max(reach, FastBounds.Beyond(paint, reach, SKPoint.Empty));
                },
                None: () => reach),
        };
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
    public Option<ColorValue> Solid => Fills.Choose(static fill => fill.SwitchPartially(@default: static _ => Option<ColorValue>.None, solid: static solid => Some(solid.Color))).Head;

    internal float HalfStroke => Strokes.IsEmpty ? 0f : Metrics[Metric.Stroke] / 2f;
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

    public static Cascade Root(ColorScheme theme, DefaultStyles defaults) =>
        (Appearances: defaults.Appearances.Find(Appearance.Native).ToSeq(), Metrics: defaults.Sizes.Find(ControlSize.Regular).ToSeq()) switch {
            var root => new(theme, defaults, root.Appearances, root.Metrics, Resolved(theme, root.Appearances, root.Metrics, None, Role.Window, []), theme.Ground),
        };

    public static RoleStyle Resolve(Cascade cascade, Role role, LanguageExt.HashSet<Condition> conditions) =>
        Resolved(cascade.Theme, cascade.Appearances, cascade.Metrics, Some(cascade.Enclosing), role, conditions);

    public static (RoleStyle Style, Cascade Inner) Enter(Cascade cascade, Node node) {
        Cascade pushed = node.Restyled.Map(static styled => styled.Sheet).Map(sheet => cascade with {
            Appearances = cascade.Appearances + sheet.Appearance.Bind(name => cascade.Defaults.Appearances.Find(name)).ToSeq() + Seq(sheet.Looks),
            Metrics = cascade.Metrics + sheet.Size.Bind(name => cascade.Defaults.Sizes.Find(name)).ToSeq() + Seq(sheet.Metrics),
        }).IfNone(cascade);
        RoleStyle style = (node.Role | (node.Conditions.IsEmpty ? None : Some(cascade.Enclosing.Role)))
            .Map(role => Resolve(pushed, role, node.Role.IsNone || node.Inherits ? node.Conditions.Union(cascade.Enclosing.Conditions) : node.Conditions))
            .IfNone(cascade.Enclosing);
        return (style, pushed with {
            Appearances = node.Role.IsSome && !style.Look.Within.IsEmpty ? pushed.Appearances.Add(style.Look.Within) : pushed.Appearances,
            Enclosing = style,
            Ground = Grounded(cascade, style, node),
        });
    }

    internal static Ground Grounded(Cascade cascade, RoleStyle style, Node node) =>
        node.Role.IsSome && node.AcceptsBackground
            ? (style.Solid.Bind(value => cascade.Theme.Color(value, style, None, None)).Filter(static color => color.Alpha >= 1f).Map(static color => (Ground)new Ground.Opaque(color))
               | style.Fills.Choose(static fill => fill.SwitchPartially(@default: static _ => Option<ColorValue>.None, backdrop: static backdrop => Some(backdrop.Fallback), blurred: static blurred => Some(blurred.Tint))).Head
                   .Bind(value => cascade.Theme.Color(value, style, None, None)).Map(static color => (Ground)new Ground.Sampled(color))).IfNone(cascade.Ground)
            : cascade.Ground;

    private static RoleStyle Resolved(ColorScheme theme, Seq<HashMap<Role, Look>> appearances, Seq<HashMap<Role, HashMap<Metric, float>>> metrics,
                                      Option<RoleStyle> enclosing, Role role, LanguageExt.HashSet<Condition> conditions) {
        bool veiled = enclosing.Exists(static held => held.Veiled);
        LanguageExt.HashSet<Condition> effective = (enclosing.Exists(static held => held.Conditions.Contains(Condition.Disabled)) ? conditions.TryAdd(Condition.Disabled) : conditions).Union(theme.Conditions);
        LanguageExt.HashSet<Condition> shown = veiled ? effective.Remove(Condition.Disabled) : effective;
        Seq<Look> rows = appearances.Choose(appearance => appearance.Find(role));
        Seq<HashMap<Metric, float>> sized = metrics.Choose(layer => layer.Find(role));
        Look look = Monoid.combine(rows + toSeq(Condition.Items).Filter(shown.Contains).Bind(condition => rows.Choose(row => row.When.Find(condition))));
        bool stated = rows.Exists(static row => row.When.ContainsKey(Condition.Disabled))
            || toSeq(Channel.Items).Exists(channel => theme.Chrome.ContainsKey(new Token(role, channel) { Condition = Condition.Disabled }));
        return new RoleStyle(role, effective,
            look with {
                Fills = effective.Contains(Condition.ReduceTransparency) ? look.Fills.Map(static fills => fills.Map(Opaque)) : look.Fills,
                Motion = effective.Contains(Condition.ReduceMotion) ? Some<TimingCurve>(new TimingCurve.Instant()) : look.Motion,
            },
            sized.Fold(Defaulted, static (under, row) => under.Union(row, Merge: static (_, _, over) => over)),
            veiled || (effective.Contains(Condition.Disabled) && !stated),
            enclosing) { Rounded = sized.Choose(static row => row.Find(Metric.Radius)).Last };
    }

    private static Material Opaque(Material fill) =>
        fill.SwitchPartially(
            @default: static held => held,
            backdrop: static backdrop => new Material.Solid(backdrop.Fallback),
            blurred: static blurred => new Material.Solid(blurred.Tint));

    // --- [LEGIBILITY]
    private static readonly Func<(SKColorF Color, SKColorF Ground, double Ratio), SKColorF> Lifts = memo<(SKColorF Color, SKColorF Ground, double Ratio), SKColorF>(Lift);

    public static SKColorF Legible(RoleStyle style, Ground ground, SKColorF color, double ratio) =>
        style.Conditions.Contains(Condition.Disabled) ? color : Lifts((color, ground.Under, ratio));

    public static double Ratio(SKColorF color, SKColorF ground) => color.ToUnicolour().Blend(ground.WithAlpha(1f).ToUnicolour(), BlendMode.Normal).Contrast(ground.WithAlpha(1f).ToUnicolour());

    private static SKColorF Lift((SKColorF Color, SKColorF Ground, double Ratio) held) {
        if (Ratio(held.Color, held.Ground) >= held.Ratio) return held.Color;
        (double lightness, double chroma, double hue) = held.Color.ToUnicolour().Oklch.Tuple;
        double floor = held.Ground.WithAlpha(1f).ToUnicolour().Oklch.L;
        Seq<(double From, double To)> spans = lightness >= floor ? [(lightness, 1d), (floor, 0d)] : [(lightness, 0d), (floor, 1d)];
        return spans.Find(span => Reaches(span.To)).Match(
            Some: span => At(toSeq(Enumerable.Range(0, 16)).Fold(span, (bounds, _) => ((bounds.From + bounds.To) / 2d) switch {
                var middle => Reaches(middle) ? (bounds.From, middle) : (middle, bounds.To),
            }).To),
            None: () => spans.Map(static span => span.To).Map(At).MaxBy(color => Ratio(color, held.Ground)));

        SKColorF At(double shade) => new Unicolour(ColourSpace.Oklch, shade, chroma, double.IsNaN(hue) ? 0d : hue, held.Color.Alpha).MapToRgbGamut(GamutMap.OklchChromaReduction).ToSKColorF();

        bool Reaches(double shade) => Ratio(At(shade), held.Ground) >= held.Ratio;
    }
}
