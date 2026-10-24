using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using SkiaSharp;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DisplayOptions(bool ReduceMotion, bool IncreaseContrast, bool DifferentiateWithoutColor, bool ReduceTransparency);

public sealed record Theme {
    public Theme(HashMap<(Role Role, Condition Condition, Interaction Interaction, Channel Channel), SKColorF> chrome,
                 (SKColorF Fill, SKColorF Text, SKColorF Disabled) ground, DisplayOptions display, string family, float type) {
        Chrome = chrome
            .AddOrUpdate((Role.Panel, Condition.Plain, Interaction.Rest, Channel.Fill), ground.Fill)
            .AddOrUpdate((Role.Panel, Condition.Plain, Interaction.Rest, Channel.Text), ground.Text)
            .AddOrUpdate((Role.Panel, Condition.Disabled, Interaction.Rest, Channel.Text), ground.Disabled);
        Display = display;
        Family = family;
        Type = type;
        Veil = (Offset(ground.Text, ground.Fill), Offset(ground.Disabled, ground.Fill)) switch {
            var (text, disabled) => (Span: Vector3.Dot(text, text), Moved: Vector3.Dot(text, disabled)) switch {
                { Span: > 0f } held => float.Clamp(held.Moved / held.Span, 0f, 1f),
                _ => 1f,
            },
        };
        Displayed = toHashSet(Seq((display.IncreaseContrast, Condition.IncreaseContrast), (display.DifferentiateWithoutColor, Condition.DifferentiateWithoutColor)).Choose(static shown => shown.Item1 ? Some(shown.Item2) : None));
    }

    public HashMap<(Role Role, Condition Condition, Interaction Interaction, Channel Channel), SKColorF> Chrome { get; }
    public DisplayOptions Display { get; }
    public string Family { get; }
    public float Type { get; }
    public float Veil { get; }
    public LanguageExt.HashSet<Condition> Displayed { get; }

    public Option<SKColorF> Color(Ink ink, Style style, Interaction interaction, Option<Series> series) =>
        ink.Switch(
            (Chrome, Style: style, Interaction: interaction, Series: series),
            @fixed: static (_, @fixed) => Some(@fixed.Color),
            host: static (held, host) => host.Role.Match(
                    Some: role => held.Chrome.Find((role, Condition.Plain, Interaction.Rest, host.Channel)),
                    None: () => Seq((held.Style.Condition, held.Interaction), (held.Style.Condition, Interaction.Rest), (Condition.Plain, held.Interaction), (Condition.Plain, Interaction.Rest))
                        .Choose(state => held.Chrome.Find((held.Style.Role, state.Item1, state.Item2, host.Channel))).Head)
                .Map(color => color.WithAlpha(color.Alpha * host.Alpha)),
            data: static (held, data) => held.Series.Map(series => series.Color.WithAlpha(series.Color.Alpha * data.Alpha)));

    private static Vector3 Offset(SKColorF ink, SKColorF ground) => new Vector3(ink.Red - ground.Red, ink.Green - ground.Green, ink.Blue - ground.Blue) * ink.Alpha;
}

public sealed record Shipped(
    HashMap<Role, HashMap<Condition, Look>> Native,
    HashMap<Role, HashMap<Condition, Look>> Glass,
    HashMap<Role, HashMap<Condition, Look>> Drawn,
    HashMap<Role, HashMap<Condition, Look>> Capsule,
    HashMap<Role, HashMap<Condition, Look>> Well,
    HashMap<Metric, float> Defaults,
    HashMap<Role, HashMap<Metric, float>> Regular,
    HashMap<Role, HashMap<Metric, float>> Compact);

public sealed record Style(Role Role, Condition Condition, LanguageExt.HashSet<Condition> Conditions, Look Look, HashMap<Metric, float> Metrics, bool Veiled) {
    public float Overhang => float.Max(Look.Shadows.Map(static shadow => (3f * shadow.Blur) + float.Hypot(shadow.X, shadow.Y) + shadow.Spread).Fold(0f, float.Max), HalfStroke);

    internal float HalfStroke => Look.Stroke.IsSome ? Metrics[Metric.Stroke] / 2f : 0f;
}

public sealed record Cascade(
    Theme Theme,
    Shipped Shipped,
    Seq<HashMap<Role, HashMap<Condition, Look>>> Appearances,
    Seq<HashMap<Role, HashMap<Metric, float>>> Metrics,
    Style Enclosing);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Styles {
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Strict) {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(), new LanguageJsonConverterFactory(), new Sksl(), new Tags() },
    };

    // --- [DECODE]
    public static IO<Shipped> Embedded =>
        IO.lift(static () => {
            using Stream json = typeof(Shipped).Assembly.GetManifestResourceStream("appearances.json")!;
            return Read<Shipped>(json);
        });

    public static Fin<T> Read<T>(Stream json) where T : notnull =>
        Try.lift(() => JsonSerializer.Deserialize<T>(json, Options)).Run()
            .Bind(static read => read is null ? Errors.ParseError($"{typeof(T).Name} file holds null") : Fin.Succ(read));

    // --- [RESOLVE]
    public static Cascade Root(Theme theme, Shipped shipped) =>
        new(theme, shipped, [], [], Resolved(theme, shipped, [], [], [], veiled: false, Role.Window, []));

    public static Style Resolve(Cascade cascade, Role role, LanguageExt.HashSet<Condition> conditions) =>
        Resolved(cascade.Theme, cascade.Shipped, cascade.Appearances, cascade.Metrics, cascade.Enclosing.Conditions, cascade.Enclosing.Veiled, role, conditions);

    public static (Style Style, Cascade Inner) Enter(Cascade cascade, Node node) {
        Cascade pushed = node is Node.Styled styled
            ? cascade with {
                Appearances = styled.Appearance.Map(appearance => appearance.Cons(cascade.Appearances)).IfNone(cascade.Appearances),
                Metrics = styled.Metrics.Map(metrics => metrics.Cons(cascade.Metrics)).IfNone(cascade.Metrics),
            }
            : cascade;
        Style style = (node.Role | (node.Conditions.IsEmpty ? None : Some(cascade.Enclosing.Role)))
            .Map(role => Resolve(pushed, role, node.Role.IsNone || node is Node.Text or Node.Glyph ? node.Conditions.Union(cascade.Enclosing.Conditions) : node.Conditions))
            .IfNone(cascade.Enclosing);
        return (style, pushed with { Appearances = node.Role.Map(role => Chain(pushed.Shipped, pushed.Appearances, role)).IfNone(pushed.Appearances), Enclosing = style });
    }

    private static Style Resolved(Theme theme, Shipped shipped, Seq<HashMap<Role, HashMap<Condition, Look>>> appearances, Seq<HashMap<Role, HashMap<Metric, float>>> metrics,
                                  LanguageExt.HashSet<Condition> enclosing, bool veiled, Role role, LanguageExt.HashSet<Condition> conditions) =>
        (Effective: (veiled ? conditions.Remove(Condition.Disabled) : enclosing.Contains(Condition.Disabled) ? conditions.TryAdd(Condition.Disabled) : conditions).Union(theme.Displayed),
            Owner: Chain(shipped, appearances, role).Add(shipped.Native).Choose(appearance => appearance.Find(role)).Head) switch {
                var (effective, owner) => toSeq(Condition.Items).Rev()
                    .Find(condition => effective.Contains(condition)
                        && (owner.Exists(rows => rows.ContainsKey(condition)) || Channel.Items.Any(channel => theme.Chrome.ContainsKey((role, condition, Interaction.Rest, channel)))))
                    .IfNone(Condition.Plain) switch {
                        var elected => new Style(role, elected, effective,
                            Projected(theme.Display, owner.Bind(rows => rows.Find(elected) | rows.Find(Condition.Plain)).IfNone(Look.Native)),
                            toHashMap(Metric.Items.Select(metric => (metric, metrics.Add(shipped.Regular).Choose(map => map.Find(role).Bind(row => row.Find(metric))).Head.IfNone(() => shipped.Defaults[metric])))),
                            veiled || (effective.Contains(Condition.Disabled) && elected != Condition.Disabled)),
                    },
            };

    private static Seq<HashMap<Role, HashMap<Condition, Look>>> Chain(Shipped shipped, Seq<HashMap<Role, HashMap<Condition, Look>>> appearances, Role role) =>
        role == Role.Well ? shipped.Well.Cons(appearances) : appearances;

    private static Look Projected(DisplayOptions display, Look look) =>
        look with {
            Fill = display.ReduceTransparency ? look.Fill.Map(static fill => fill is Material.Backdrop backdrop ? new Material.Solid(backdrop.Fallback) : fill) : look.Fill,
            Motion = display.ReduceMotion ? new TimingCurve.Instant() : look.Motion,
        };
}
