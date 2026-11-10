using SkiaSharp;

namespace Rasm.UI.Media;

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

        internal override SKPathEffect Effect(float width, float clock) => SKPathEffect.Create1DPath(Mark.Path, Advance, 0f, SKPath1DPathEffectStyle.Rotate);
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
}
