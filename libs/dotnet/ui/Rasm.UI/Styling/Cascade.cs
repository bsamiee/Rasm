using SkiaSharp;
using Wacton.Unicolour;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
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
    internal RoleStyle(Theme theme, Role role, HashSet<Condition> conditions, Look look, bool dimmed, Option<RoleStyle> enclosing) {
        Role = role;
        Enclosing = enclosing;
        Conditions = conditions;
        Look = look;
        HashMap<Metric, float> scaled = Defaulted.Union(look.Metrics, Merge: static (_, _, over) => over).Map((metric, value) =>
            metric.Inherited && !look.Metrics.ContainsKey(metric) ? enclosing.Map(held => held.Metrics[metric]).IfNone(value * metric.Scale(theme)) : value * metric.Scale(theme));
        Metrics = look.Shape.Exists(static shape => shape is Shape.Icon) && !look.Metrics.ContainsKey(Metric.Extent) ? scaled.SetItem(Metric.Extent, scaled[Metric.Icon]) : scaled;
        Motion = look.Motion.Map(curve => Paced(curve, theme.Pace)).IfNone(Instant);
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
    internal TimingCurve Motion { get; }
    internal bool Dimmed { get; }
    internal HashSet<Condition> Shown { get; }
    internal bool UnderGlass { get; }
    internal float Overhang { get; }

    internal Option<float> Rounded => Look.Metrics.ContainsKey(Metric.Radius) ? Some(Metrics[Metric.Radius]) : None;

    // --- [CHANNELS]
    internal Seq<Material> Fills => UnderGlass ? Painted.Filter(static fill => fill is not Material.Glass) : Painted;
    internal Seq<Stroke> Strokes => Look.Strokes.IfNone(NativeStrokes);
    internal ColorValue Text => Look.Text.IfNone(NativeText);
    internal Seq<Shadow> Shadows => Look.Shadows.IfNone([]);
    internal Seq<Filter> Filters => Look.Filters.IfNone([]);
    internal SKBlender Blend => Look.Blend.IfNone(SourceOver);
    internal Shape Shape => Look.Shape.IfNone(RoundedShape);
    internal Option<ColorValue> Solid => Fills.Choose(static fill => fill.BaseColor).Head;

    internal Option<Material.Glass> Glass => Fills.Choose(static fill => fill is Material.Glass glass ? Some(glass) : None).Head;

    internal Option<ColorValue> Tint => toSeq(Fills.SkipWhile(static fill => fill is not Material.Glass)).Tail.Choose(static fill => fill.BaseColor).Head;

    internal bool Timed => Fills.Exists(static fill => fill.Timed) || Strokes.Exists(static stroke => stroke.Timed) || Filters.Exists(static filter => filter.Timed);

    internal bool Pointing => Fills.Exists(static fill => fill.Pointing) || Filters.Exists(static filter => filter.Pointing);

    internal float HalfStroke => Strokes.IsEmpty ? 0f : Metrics[Metric.Stroke] / 2f;

    private Seq<Material> Painted => Look.Fills.IfNone(NativeFills);

    private static TimingCurve Paced(TimingCurve curve, Factor pace) =>
        curve.Switch(
            (double)(float)pace,
            instant: static (_, held) => (TimingCurve)held,
            eased: static (by, held) => held with { Length = held.Length * by },
            spring: static (by, held) => held with { Response = held.Response * by },
            decay: static (by, held) => held with { TimeConstant = held.TimeConstant * by, Initial = held.Initial / by });
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
        return new RoleStyle(theme, role, effective,
            effective.Contains(Condition.ReduceTransparency) ? look with { Fills = look.Fills.Map(static fills => fills.Map(static fill => fill.Opaque)) } : look,
            dimmed || (effective.Contains(Condition.Disabled) && !stated),
            enclosing);
    }
}
