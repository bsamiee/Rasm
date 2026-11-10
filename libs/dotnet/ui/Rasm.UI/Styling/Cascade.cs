using Rasm.UI.Motion;
using Rasm.UI.Media;
using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------

internal sealed record RoleStyle {

    // --- [DEFAULTS]

    private static readonly HashMap<Metric, float> Defaulted = toHashMap(Metric.Items.Select(static metric => (metric, metric.Default)));
    private static readonly Seq<Material> NativeFills = [new Material.Solid(new ColorValue.Current(Channel.Fill))];
    private static readonly Seq<Stroke> NativeStrokes = [new Stroke(new ColorValue.Current(Channel.Stroke))];
    private static readonly ColorValue NativeText = new ColorValue.Current(Channel.Text);
    private static readonly TimingCurve Instant = new TimingCurve.Instant();
    private static readonly Shape RoundedShape = new Shape.Rounded();

    // --- [CONSTRUCTION]

    internal RoleStyle(Theme theme, StyleSheet sheet, Key role, HashSet<Condition> stated, Option<RoleStyle> enclosing) {
        HashSet<Condition> conditions = stated.Union(enclosing.Map(static held => held.Conditions.Filter(static condition => condition.Inherited)).IfNone([])).Union(theme.Conditions);
        Look row = sheet.Looks.Find(role).IfNone(Look.Empty);
        bool dimmed = enclosing.Exists(static held => held.Dimmed) || (conditions.Contains(Condition.Disabled)
            && !row.When.ContainsKey(Condition.Disabled) && !toSeq(Channel.Items).Exists(channel => theme.Chrome.ContainsKey(new Token(role, channel) { Condition = Condition.Disabled })));
        HashSet<Condition> shown = dimmed ? conditions.Remove(Condition.Disabled) : conditions;
        Look combined = toSeq(Condition.Items).Filter(shown.Contains).Choose(row.When.Find).Fold(row, static (held, over) => held.Combine(over));
        Look look = conditions.Contains(Condition.ReduceTransparency) ? combined with { Fills = combined.Fills.Map(static fills => fills.Map(static fill => fill.Opaque)) } : combined;
        (Role, Enclosing, Conditions, Shown, Look) = (role, enclosing, conditions, shown, look);
        float compact = conditions.Contains(Condition.Compact) ? 0.8f : 1f;
        HashMap<Metric, float> scaled = Defaulted.Union(look.Metrics, Merge: static (_, _, over) => over).Map((metric, value) =>
            metric.Inherited && !look.Metrics.ContainsKey(metric) ? enclosing.Map(held => held.Metrics[metric]).IfNone(value * metric.Scale(theme, compact)) : value * metric.Scale(theme, compact));
        Metrics = look.Shape.Exists(static shape => shape is Shape.Icon) && !look.Metrics.ContainsKey(Metric.Extent) ? scaled.SetItem(Metric.Extent, scaled[Metric.Icon]) : scaled;
        Face = enclosing.Map(static held => held.Face).IfNone(TextFace.Empty).Combine(look.Face);
        Motion = look.Motion.IfNone(Instant);
        Dimmed = dimmed;
        UnderGlass = enclosing.Exists(static held => held.UnderGlass || held.Glass.IsSome);
        Overhang = float.Max(float.Max(HalfStroke, Glass.Map(static _ => Material.Glass.Band(1f)).IfNone(0f)), Shadow.Outset(Shadows)) + Filter.Outset(Filters);
    }

    // --- [RESOLVED]

    internal Key Role { get; }
    internal Option<RoleStyle> Enclosing { get; }
    internal HashSet<Condition> Conditions { get; }
    internal HashSet<Condition> Shown { get; }
    internal Look Look { get; }
    internal HashMap<Metric, float> Metrics { get; }
    internal TextFace Face { get; }
    internal TimingCurve Motion { get; }
    internal bool Dimmed { get; }
    internal bool UnderGlass { get; }
    internal float Overhang { get; }

    // --- [CHANNELS]

    internal Seq<Material> Fills => UnderGlass ? Painted.Filter(static fill => fill is not Material.Glass) : Painted;
    internal Seq<Stroke> Strokes => Look.Strokes.IfNone(NativeStrokes);
    internal ColorValue Text => Look.Text.IfNone(NativeText);
    internal Seq<Shadow> Shadows => Look.Shadows.IfNone([]);
    internal Seq<Filter> Filters => Look.Filters.IfNone([]);
    internal Shape Shape => Look.Shape.IfNone(RoundedShape);
    internal Option<ColorValue> Solid => Fills.Choose(static fill => fill.BaseColor).Head;

    internal Option<Material.Glass> Glass => Fills.Choose(static fill => fill is Material.Glass glass ? Some(glass) : None).Head;

    internal bool Timed => Fills.Exists(static fill => fill.Timed) || Strokes.Exists(static stroke => stroke.Timed) || Filters.Exists(static filter => filter.Timed);

    internal bool Pointing => Fills.Exists(static fill => fill.Pointing) || Filters.Exists(static filter => filter.Pointing);

    internal float HalfStroke => Strokes.IsEmpty ? 0f : Metrics[Metric.Stroke] / 2f;

    private Seq<Material> Painted => Look.Fills.IfNone(NativeFills);
}

internal sealed record Cascade(Theme Theme, StyleSheet Sheet, RoleStyle Enclosing, SKColorF Background) {
    internal static Cascade Root(Theme theme, StyleSheet sheet, Key root) => new(theme, sheet, new RoleStyle(theme, sheet, root, [], None), theme.Background);

    internal RoleStyle Resolve(Key role, HashSet<Condition> conditions) => new(Theme, Sheet, role, conditions, Enclosing);
}
