using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Stroke(ColorValue Color, Seq<Seq<float>> Dash = default) {
    public Seq<float> DashPattern(uint ordinal) => Dash.IsEmpty ? [] : Dash[(int)(ordinal % (uint)Dash.Count)];
}

public sealed record Shadow(ColorValue Color, float Blur) {
    public float X { get; init; }
    public float Y { get; init; }
    public float Spread { get; init; }
    public bool Inside { get; init; }

    internal OutsetShadow Outlined => Inside ? new(0f, this) : new(Spread, this with { Spread = 0f });

    internal SKImageFilter Filtered(SKColorF color) {
        float grown = Inside ? -Spread : Spread;
        using SKImageFilter? morphed = grown switch { > 0f => SKImageFilter.CreateDilate(grown, grown), < 0f => SKImageFilter.CreateErode(-grown, -grown), _ => null };
        using SKImageFilter moved = SKImageFilter.CreateOffset(X, Y, morphed);
        using SKImageFilter blurred = SKImageFilter.CreateBlur(Blur, Blur, moved);
        using SKColorFilter inked = SKColorFilter.CreateColorMatrix([
            0f, 0f, 0f, 0f, color.Red,
            0f, 0f, 0f, 0f, color.Green,
            0f, 0f, 0f, 0f, color.Blue,
            0f, 0f, 0f, Inside ? -color.Alpha : color.Alpha, Inside ? color.Alpha : 0f,
        ]);
        if (!Inside) return SKImageFilter.CreateColorFilter(inked, blurred);
        using SKImageFilter hollow = SKImageFilter.CreateColorFilter(inked, blurred);
        return SKImageFilter.CreateBlendMode(SKBlendMode.SrcIn, background: null, foreground: hollow);
    }
}

internal sealed record OutsetShadow(float Outset, Shadow Shadow);

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
    public HashMap<Interaction, ColorValue> StateLayers { get; init; }
    public Option<SKBlendMode> Blend { get; init; }
    public Option<TimingCurve> Motion { get; init; }
    public HashMap<Condition, Look> When { get; init; }
    public HashMap<Role, Look> Within { get; init; }

    public Look Combine(Look rhs) =>
        new() {
            Fills = rhs.Fills | Fills,
            Strokes = rhs.Strokes | Strokes,
            Text = rhs.Text | Text,
            Face = Face.Combine(rhs.Face),
            Shadows = rhs.Shadows | Shadows,
            Filters = rhs.Filters | Filters,
            Palette = rhs.Palette | Palette,
            StateLayers = StateLayers.Union(rhs.StateLayers, Merge: static (_, _, over) => over),
            Blend = rhs.Blend | Blend,
            Motion = rhs.Motion | Motion,
            When = When.Union(rhs.When, Merge: static (_, under, over) => under.Combine(over)),
            Within = Within.Union(rhs.Within, Merge: static (_, under, over) => under.Combine(over)),
        };
}
