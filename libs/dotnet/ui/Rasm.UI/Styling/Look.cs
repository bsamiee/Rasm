using Rasm.UI.Motion;
using Rasm.UI.Media;
using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Look : Monoid<Look> {
    public static Look Empty { get; } = new();

    public static Look Unpainted { get; } = new() { Fills = Seq<Material>(), Strokes = Seq<Stroke>() };

    public Option<Seq<Material>> Fills { get; init; }
    public Option<Seq<Stroke>> Strokes { get; init; }
    public Option<ColorValue> Text { get; init; }
    public TextFace Face { get; init; } = TextFace.Empty;
    public Option<Seq<Shadow>> Shadows { get; init; }
    public Option<Seq<Filter>> Filters { get; init; }
    public Option<Seq<ColorValue>> Palette { get; init; }
    public Option<ColorRamp> Scale { get; init; }
    public Option<Shape> Shape { get; init; }
    public HashMap<Interaction, ColorValue> StateLayers { get; init; }
    public Option<SKBlender> Blend { get; init; }
    public Option<TimingCurve> Motion { get; init; }
    public HashMap<Condition, Look> When { get; init; }
    public HashMap<Role, Look> Within { get; init; }
    public HashMap<Metric, float> Metrics { get; init; }

    public Look Combine(Look rhs) =>
        new() {
            Fills = rhs.Fills | Fills,
            Strokes = rhs.Strokes | Strokes,
            Text = rhs.Text | Text,
            Face = Face.Combine(rhs.Face),
            Shadows = rhs.Shadows | Shadows,
            Filters = rhs.Filters | Filters,
            Palette = rhs.Palette | Palette,
            Scale = rhs.Scale | Scale,
            Shape = rhs.Shape | Shape,
            StateLayers = StateLayers.Union(rhs.StateLayers, Merge: static (_, _, over) => over),
            Blend = rhs.Blend | Blend,
            Motion = rhs.Motion | Motion,
            When = When.Union(rhs.When, Merge: static (_, under, over) => under.Combine(over)),
            Within = Within.Union(rhs.Within, Merge: static (_, under, over) => under.Combine(over)),
            Metrics = Metrics.Union(rhs.Metrics, Merge: static (_, _, over) => over),
        };
}
