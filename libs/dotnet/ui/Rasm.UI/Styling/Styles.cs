using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using SkiaSharp;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Token(Role Role, Channel Channel) {
    public Option<Condition> Condition { get; init; }
    public Option<Interaction> Interaction { get; init; }
}

public sealed record Ground(SKColorF Fill, SKColorF Text, SKColorF Disabled);

public sealed record Theme {
    public Theme(HashMap<Token, SKColorF> chrome, Ground ground, LanguageExt.HashSet<Condition> conditions, string fontFamily, float fontSize) {
        Chrome = chrome.AddOrUpdateRange([
            (new Token(Role.Panel, Channel.Fill), ground.Fill),
            (new Token(Role.Panel, Channel.Text), ground.Text),
            (new Token(Role.Panel, Channel.Text) { Condition = Condition.Disabled }, ground.Disabled),
        ]);
        Ground = ground;
        Conditions = conditions;
        FontFamily = fontFamily;
        FontSize = fontSize;
        Veil = (Offset(ground.Text, ground.Fill), Offset(ground.Disabled, ground.Fill)) switch {
            var (text, disabled) => (Span: Vector3.Dot(text, text), Moved: Vector3.Dot(text, disabled)) switch {
                { Span: > 0f } held => float.Clamp(held.Moved / held.Span, 0f, 1f),
                _ => 1f,
            },
        };
    }

    public HashMap<Token, SKColorF> Chrome { get; }
    public Ground Ground { get; }
    public LanguageExt.HashSet<Condition> Conditions { get; }
    public string FontFamily { get; }
    public float FontSize { get; }
    public float Veil { get; }

    public Option<SKColorF> Color(Ink ink, Style style, Option<Interaction> interaction, Option<Series> series) =>
        ink.Switch(
            (Chrome, Style: style, Interaction: interaction, Series: series),
            @fixed: static (held, @fixed) => held.Interaction.IsNone ? Some(@fixed.Color) : None,
            host: static (held, host) => host.Role.Match(
                    Some: role => held.Interaction.IsNone ? held.Chrome.Find(new Token(role, host.Channel)) : None,
                    None: () => (toSeq(Condition.Items).Rev().Filter(held.Style.Shown.Contains).Map(static condition => Some(condition)) + Seq(Option<Condition>.None))
                        .Choose(condition => held.Chrome.Find(new Token(held.Style.Role, host.Channel) { Condition = condition, Interaction = held.Interaction })).Head)
                .Map(color => color.WithAlpha(color.Alpha * (float)host.Alpha)),
            data: static (held, data) => held.Interaction.IsNone ? held.Series.Map(series => series.Color.WithAlpha(series.Color.Alpha * (float)data.Alpha)) : None);

    private static Vector3 Offset(SKColorF ink, SKColorF ground) => new Vector3(ink.Red - ground.Red, ink.Green - ground.Green, ink.Blue - ground.Blue) * ink.Alpha;
}

[ComplexValueObject]
public sealed partial class Shipped {
    public HashMap<Role, Look> Native { get; }
    public HashMap<Role, Look> Glass { get; }
    public HashMap<Role, Look> Drawn { get; }
    public HashMap<Role, Look> Capsule { get; }
    public HashMap<Metric, float> Defaults { get; }
    public HashMap<Role, HashMap<Metric, float>> Regular { get; }
    public HashMap<Role, HashMap<Metric, float>> Compact { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref HashMap<Role, Look> native, ref HashMap<Role, Look> glass, ref HashMap<Role, Look> drawn, ref HashMap<Role, Look> capsule,
        ref HashMap<Metric, float> defaults, ref HashMap<Role, HashMap<Metric, float>> regular, ref HashMap<Role, HashMap<Metric, float>> compact) {
        HashMap<Metric, float> stated = defaults;
        validationError = toSeq(Metric.Items).Filter(metric => !stated.ContainsKey(metric)) switch {
            { IsEmpty: true } => null,
            var missing => new ValidationError($"shipped defaults miss {string.Join(", ", missing.Map(static metric => metric.Key))}"),
        };
    }
}

public sealed record Style {
    private static readonly Seq<Material> NativeFills = [new Material.Solid(new Ink.Host(None, Channel.Fill, Fraction.MaxValue))];
    private static readonly Seq<Stroke> NativeStrokes = [new Stroke(new Ink.Host(None, Channel.Edge, Fraction.MaxValue))];
    private static readonly Ink NativeText = new Ink.Host(None, Channel.Text, Fraction.MaxValue);
    private static readonly SKRect Probe = SKRect.Create(1f, 1f);

    public Style(Role role, LanguageExt.HashSet<Condition> conditions, Look look, HashMap<Metric, float> metrics, bool veiled) {
        Role = role;
        Conditions = conditions;
        Look = look;
        Metrics = metrics;
        Veiled = veiled;
        Shown = veiled ? conditions.Remove(Condition.Disabled) : conditions;
        Overhang = Shadows.Fold(HalfStroke, static (reach, shadow) => float.Max(reach, Cast(shadow)));
    }

    public Role Role { get; }
    public LanguageExt.HashSet<Condition> Conditions { get; }
    public Look Look { get; }
    public HashMap<Metric, float> Metrics { get; }
    public bool Veiled { get; }
    public LanguageExt.HashSet<Condition> Shown { get; }
    public float Overhang { get; }

    public Seq<Material> Fills => Look.Fills.IfNone(NativeFills);
    public Seq<Stroke> Strokes => Look.Strokes.IfNone(NativeStrokes);
    public Ink Text => Look.Text.IfNone(NativeText);
    public Seq<Shadow> Shadows => Look.Shadows.IfNone([]);
    public SKFontStyleWeight Weight => Look.Weight.IfNone(SKFontStyleWeight.Normal);
    public Seq<HarfBuzzSharp.Feature> Features => Look.Features.IfNone([]);
    public SKBlendMode Blend => Look.Blend.IfNone(SKBlendMode.SrcOver);
    public TimingCurve Motion => Look.Motion.IfNone(static () => new TimingCurve.Instant());
    public Option<Ink> Solid => Fills.Choose(static fill => fill.SwitchPartially(@default: static _ => Option<Ink>.None, solid: static solid => Some(solid.Ink))).Head;

    internal float HalfStroke => Strokes.IsEmpty ? 0f : Metrics[Metric.Stroke] / 2f;

    private static float Cast(Shadow shadow) {
        using SKMaskFilter? blur = shadow.Blur > 0f ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, shadow.Blur) : null;
        using SKPaint cast = new() { MaskFilter = blur };
        SKRect spread = SKRect.Inflate(Probe, shadow.Spread, shadow.Spread);
        spread.Offset(shadow.X, shadow.Y);
        _ = cast.GetFastBounds(spread, out SKRect bounds);
        return float.Max(float.Max(Probe.Left - bounds.Left, Probe.Top - bounds.Top), float.Max(bounds.Right - Probe.Right, bounds.Bottom - Probe.Bottom));
    }
}

public sealed record Cascade(
    Theme Theme,
    Shipped Shipped,
    Seq<HashMap<Role, Look>> Appearances,
    Seq<HashMap<Role, HashMap<Metric, float>>> Metrics,
    Style Enclosing);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Styles {
    // --- [DECODE]
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Strict) {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower), new LanguageJsonConverterFactory(), new RuntimeEffectJsonConverter(), new FeatureJsonConverter() },
    };

    public static IO<Shipped> Embedded =>
        IO.lift(static () => {
            using System.IO.Stream json = typeof(Shipped).Assembly.GetManifestResourceStream("appearances.json")!;
            return Read<Shipped>(json);
        });

    public static Fin<T> Read<T>(System.IO.Stream json) where T : notnull =>
        Try.lift(() => JsonSerializer.Deserialize<T>(json, Options)).Run()
            .Bind(static read => read is null ? Errors.ParseError($"{typeof(T).Name} file holds null") : Fin.Succ(read));

    // --- [RESOLVE]
    public static Cascade Root(Theme theme, Shipped shipped) =>
        new(theme, shipped, [shipped.Native], [shipped.Regular], Resolved(theme, shipped, [shipped.Native], [shipped.Regular], [], veiled: false, Role.Window, []));

    public static Style Resolve(Cascade cascade, Role role, LanguageExt.HashSet<Condition> conditions) =>
        Resolved(cascade.Theme, cascade.Shipped, cascade.Appearances, cascade.Metrics, cascade.Enclosing.Conditions, cascade.Enclosing.Veiled, role, conditions);

    public static (Style Style, Cascade Inner) Enter(Cascade cascade, Node node) {
        Cascade pushed = node.Restyled.Map(styled => cascade with { Appearances = cascade.Appearances.Add(styled.Appearance), Metrics = cascade.Metrics.Add(styled.Metrics) }).IfNone(cascade);
        Style style = (node.Role | (node.Conditions.IsEmpty ? None : Some(cascade.Enclosing.Role)))
            .Map(role => Resolve(pushed, role, node.Role.IsNone || node.Inherits ? node.Conditions.Union(cascade.Enclosing.Conditions) : node.Conditions))
            .IfNone(cascade.Enclosing);
        return (style, pushed with {
            Appearances = node.Role.IsSome && !style.Look.Within.IsEmpty ? pushed.Appearances.Add(style.Look.Within) : pushed.Appearances,
            Enclosing = style,
        });
    }

    private static Style Resolved(Theme theme, Shipped shipped, Seq<HashMap<Role, Look>> appearances, Seq<HashMap<Role, HashMap<Metric, float>>> metrics,
                                  LanguageExt.HashSet<Condition> enclosing, bool veiled, Role role, LanguageExt.HashSet<Condition> conditions) {
        LanguageExt.HashSet<Condition> effective = (enclosing.Contains(Condition.Disabled) ? conditions.TryAdd(Condition.Disabled) : conditions).Union(theme.Conditions);
        LanguageExt.HashSet<Condition> shown = veiled ? effective.Remove(Condition.Disabled) : effective;
        Seq<Look> rows = appearances.Choose(appearance => appearance.Find(role));
        Look look = Monoid.combine(rows + toSeq(Condition.Items).Filter(shown.Contains).Bind(condition => rows.Choose(row => row.When.Find(condition))));
        bool stated = rows.Exists(static row => row.When.ContainsKey(Condition.Disabled))
            || toSeq(Channel.Items).Exists(channel => theme.Chrome.ContainsKey(new Token(role, channel) { Condition = Condition.Disabled }));
        return new Style(role, effective,
            look with {
                Fills = effective.Contains(Condition.ReduceTransparency) ? look.Fills.Map(static fills => fills.Map(Opaque)) : look.Fills,
                Motion = effective.Contains(Condition.ReduceMotion) ? Some<TimingCurve>(new TimingCurve.Instant()) : look.Motion,
            },
            metrics.Choose(layer => layer.Find(role)).Fold(shipped.Defaults, static (under, row) => under.Union(row, Merge: static (_, _, over) => over)),
            veiled || (effective.Contains(Condition.Disabled) && !stated));
    }

    private static Material Opaque(Material fill) =>
        fill.SwitchPartially(
            @default: static held => held,
            backdrop: static backdrop => new Material.Solid(backdrop.Fallback),
            blurred: static blurred => new Material.Solid(blurred.Tint));
}
