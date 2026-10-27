using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PathEffect {
    internal virtual bool Timed => false;

    internal abstract SKPathEffect Effect(float width, float clock);

    public sealed record Dash : PathEffect {
        private Dash(Seq<float> rhythm) => Rhythm = rhythm;

        public Seq<float> Rhythm { get; }
        public float Drift { get; init; }

        internal override bool Timed => Drift is not 0f;

        public static Fin<Dash> Validated(Seq<float> rhythm) =>
            UIFault.Unless(rhythm.Count >= 2 && rhythm.Count % 2 is 0 && rhythm.ForAll(static beat => float.IsFinite(beat) && beat >= 0f) && rhythm.Exists(static beat => beat > 0f), UIReason.Range, rhythm)
                .Map(_ => new Dash(rhythm));

        internal override SKPathEffect Effect(float width, float clock) => SKPathEffect.CreateDash([.. Rhythm.Map(beat => beat * width)], Drift * width * clock);
    }

    public sealed record Trim(Fraction From, Fraction To) : PathEffect {
        internal override SKPathEffect Effect(float width, float clock) => SKPathEffect.CreateTrim((float)From, (float)To);
    }

    public sealed record Stamp : PathEffect {
        private Stamp(Shape.Outlined mark, float advance) => (Mark, Advance) = (mark, advance);

        public Shape.Outlined Mark { get; }
        public float Advance { get; }

        public static Fin<Stamp> Validated(Shape.Outlined mark, float advance) =>
            UIFault.Unless(float.IsFinite(advance) && advance > 0f, UIReason.Range, advance).Map(_ => new Stamp(mark, advance));

        internal override SKPathEffect Effect(float width, float clock) {
            using SKPath mark = Mark.Path();
            return SKPathEffect.Create1DPath(mark, Advance, 0f, SKPath1DPathEffectStyle.Rotate);
        }
    }

    public sealed record Corner : PathEffect {
        private Corner(float radius) => Radius = radius;

        public float Radius { get; }

        public static Fin<Corner> Validated(float radius) => UIFault.Unless(float.IsFinite(radius) && radius > 0f, UIReason.Range, radius).Map(_ => new Corner(radius));

        internal override SKPathEffect Effect(float width, float clock) => SKPathEffect.CreateCorner(Radius);
    }
}

public sealed record Stroke(ColorValue Color) {
    public Seq<Seq<PathEffect>> Patterns { get; init; }
    public SKStrokeCap Cap { get; init; } = SKStrokeCap.Round;
    public SKStrokeJoin Join { get; init; } = SKStrokeJoin.Round;

    internal bool Timed => Patterns.Exists(static cycle => cycle.Exists(static pattern => pattern.Timed));

    internal Seq<PathEffect> Patterned(uint ordinal) => Patterns.IsEmpty ? [] : Patterns[(int)(ordinal % (uint)Patterns.Count)];

    internal static Option<SKPathEffect> Effect(Seq<PathEffect> patterns, float width, float clock) =>
        patterns.Fold(Option<SKPathEffect>.None, (inner, pattern) => Some(inner.Match(
            Some: held => {
                using (held)
                using (SKPathEffect outer = pattern.Effect(width, clock))
                    return SKPathEffect.CreateCompose(outer, held);
            },
            None: () => pattern.Effect(width, clock))));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Shape {
    public sealed record Rounded : Shape;

    public sealed record Outlined : Shape {
        private Outlined(string pathData) => PathData = pathData;

        public string PathData { get; }

        public static Fin<Outlined> Validated(string pathData) {
            using SKPath? parsed = SKPath.ParseSvgPathData(pathData);
            return Optional(parsed).Filter(static path => !path.IsEmpty).Map(static path => new Outlined(path.ToSvgPathData())).ToFin(new UIFault(pathData, UIReason.Unreadable));
        }

        internal SKPath Path() => SKPath.ParseSvgPathData(PathData);
    }

    public sealed record Placed(Shape Inner, SKRect Unit) : Shape;

    public sealed record Combined(Shape First, SKPathOp Op, Shape Second) : Shape;
}

public sealed record Shadow(ColorValue Color, float Blur) {
    private static readonly SKColorF Probe = new(0f, 0f, 0f, 1f);
    internal static readonly Func<Seq<Shadow>, float> Outset = memo<Seq<Shadow>, float>(static shadows =>
        shadows.Filter(static shadow => !shadow.Inside).Fold(0f, static (reach, shadow) => float.Max(reach, Filter.Reach(shadow.Filtered(Probe, Filter.Probe.Space)))));

    public float X { get; init; }
    public float Y { get; init; }
    public float Spread { get; init; }
    public bool Inside { get; init; }

    internal SKImageFilter Filtered(SKColorF color, SKColorSpace target) {
        float grown = Inside ? -Spread : Spread;
        using SKImageFilter? morphed = grown switch { > 0f => SKImageFilter.CreateDilate(grown, grown), < 0f => SKImageFilter.CreateErode(-grown, -grown), _ => null };
        using SKImageFilter moved = SKImageFilter.CreateOffset(X, Y, morphed);
        using SKImageFilter blurred = SKImageFilter.CreateBlur(Blur, Blur, moved);
        using SKColorFilter tinted = target.ColorMatrix([
            0f, 0f, 0f, 0f, color.Red,
            0f, 0f, 0f, 0f, color.Green,
            0f, 0f, 0f, 0f, color.Blue,
            0f, 0f, 0f, Inside ? -color.Alpha : color.Alpha, Inside ? color.Alpha : 0f,
        ]);
        if (!Inside) return SKImageFilter.CreateColorFilter(tinted, blurred);
        using SKImageFilter hollow = SKImageFilter.CreateColorFilter(tinted, blurred);
        return SKImageFilter.CreateBlendMode(SKBlendMode.SrcIn, background: null, foreground: hollow);
    }
}

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
