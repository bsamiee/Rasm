using System.Globalization;

namespace Rasm.UI.Plotting;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Stretch(float Low, float High);

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class AxisRange {
    public static AxisRange Unit { get; } = new(0f, 1f);

    public float From { get; }
    public float To { get; }

    public float Span => To - From;
    public float Low => float.Min(From, To);
    public float High => float.Max(From, To);

    public Option<AxisRange> Scaled(float about, float factor) =>
        Factor.Validated(factor).ToOption().Bind(scale => Validated(about + ((From - about) * (float)scale), about + ((To - about) * (float)scale)).ToOption());

    public Option<AxisRange> Shifted(float delta) => Validated(From + delta, To + delta).ToOption();

    public static Fin<AxisRange> Validated(float from, float to) =>
        UIFault.Unless(float.IsFinite(from) && float.IsFinite(to) && from.CompareTo(to) is not 0, UIReason.Range, (from, to)).Map(_ => new AxisRange(from, to));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Axis {
    private Axis(AxisRange domain) => Domain = domain;

    public AxisRange Domain { get; init; }

    public virtual float Fraction(float value) => (value - Domain.From) / Domain.Span;

    public virtual float Value(float fraction) => Domain.From + (fraction * Domain.Span);

    public float Unit(float value) => Wrapped(Fraction(value));

    public virtual float Delta(float from, float to) => to - from;

    public virtual float Held(float value) => float.Clamp(value, Domain.Low, Domain.High);

    public virtual bool Contains(AxisRange band, float value) => value >= band.Low && value <= band.High;

    public virtual Seq<Stretch> Spans(AxisRange band) {
        float from = Fraction(band.From);
        float to = Fraction(band.To);
        return [new Stretch(float.Min(from, to), float.Max(from, to))];
    }

    public virtual Option<Axis> Over(AxisRange range) => Some(this with { Domain = range });

    public int Decimals => new Linear(Domain).Step(0.01f).Map(static step => int.Max(0, (int)MathF.Ceiling(-MathF.Log10(step)))).IfNone(0);

    public string Spoken(float value, CultureInfo culture) => MathF.Round(value, Decimals, MidpointRounding.ToEven).ToString(culture);

    public abstract Option<float> Step(float least);
    public abstract Seq<float> Ticks(float least);
    public abstract float Snapped(float value, int steps, float least);

    internal bool Resolves(float pixels) {
        float largest = float.Max(MathF.Abs(Domain.From), MathF.Abs(Domain.To));
        return MathF.Abs(Domain.Span) >= pixels * (MathF.BitIncrement(largest) - largest);
    }

    private protected virtual float Wrapped(float fraction) => fraction;

    private protected float Ticked(float value, int steps, float least) {
        Seq<float> ticks = Ticks(least).Strict();
        float at = Fraction(value);
        return ticks.IsEmpty ? value : ticks[int.Clamp(toSeq(Range(0, ticks.Count)).MinBy(index => MathF.Abs(Fraction(ticks[index]) - at)) + steps, 0, ticks.Count - 1)];
    }

    private static Option<float> Nice(float span) {
        float decade = float.Exp10(MathF.Floor(MathF.Log10(span)));
        return float.IsFinite(span) && span > 0f ? Seq(1f, 2f, 5f, 10f).Map(step => step * decade).Find(step => step >= span) : None;
    }

    public sealed record Linear(AxisRange Domain) : Axis(Domain) {
        public override Option<float> Step(float least) => Nice(least * MathF.Abs(Domain.Span));

        public override Seq<float> Ticks(float least) =>
            Step(least).Map(step => {
                int first = (int)MathF.Ceiling(Domain.Low / step);
                return toSeq(Range(first, int.Max(0, (int)MathF.Floor(Domain.High / step) - first + 1))).Map(multiple => multiple * step);
            }).IfNone([]);

        public override float Snapped(float value, int steps, float least) =>
            Step(least).Map(step => Held((MathF.Round(value / step, MidpointRounding.ToEven) + (steps * MathF.Sign(Domain.Span))) * step)).IfNone(value);
    }

    public sealed record Logarithmic(AxisRange Domain) : Axis(Domain) {
        private float Depth => float.LogP1(MathF.Abs(Domain.Span));

        public override float Fraction(float value) {
            float along = (value - Domain.From) * MathF.Sign(Domain.Span);
            return MathF.CopySign(float.LogP1(MathF.Abs(along)), along) / Depth;
        }

        public override float Value(float fraction) => Domain.From + (MathF.Sign(Domain.Span) * MathF.CopySign(float.ExpM1(MathF.Abs(fraction) * Depth), fraction));

        public override Option<float> Step(float least) => Some(MathF.Ceiling(least * Depth / MathF.Log(10f))).Filter(float.IsFinite).Map(static decades => float.Exp10(float.Max(1f, decades)));

        public override Seq<float> Ticks(float least) =>
            Step(least).Map(factor => Domain.From.Cons(toSeq(Range(0, int.Max(0, (int)MathF.Floor(MathF.Log10(MathF.Abs(Domain.Span)) / MathF.Log10(factor)) + 1)))
                .Map(power => Domain.From + (MathF.Sign(Domain.Span) * MathF.Pow(factor, power))))).IfNone([]);

        public override float Snapped(float value, int steps, float least) => Ticked(value, steps, least);
    }

    public sealed record Log : Axis {
        private const float Slack = 1e-4f;

        private Log(AxisRange domain, float @base) : base(domain) => Base = @base;

        public float Base { get; }

        private float Depth => MathF.Log(Domain.To / Domain.From);

        public override float Fraction(float value) => MathF.Log(value / Domain.From) / Depth;

        public override float Value(float fraction) => Domain.From * MathF.Exp(fraction * Depth);

        public static Fin<Axis> Validated(AxisRange domain, float @base) =>
            UIFault.Unless(domain.Low > 0f && @base > 1f, UIReason.Range, (domain, @base)).Map(_ => (Axis)new Log(domain, @base));

        public override Option<Axis> Over(AxisRange range) => Validated(range, Base).ToOption();

        public override Option<float> Step(float least) => None;

        public override Seq<float> Ticks(float least) {
            int first = (int)MathF.Ceiling(MathF.Log(Domain.Low, Base) - Slack);
            int last = (int)MathF.Floor(MathF.Log(Domain.High, Base) + Slack);
            float span = MathF.Abs(Depth);
            int stride = int.Max(1, (int)MathF.Ceiling(least * span / MathF.Log(Base)));
            Seq<float> majors = toSeq(Range(first, int.Max(0, last - first + 1))).Filter(power => power % stride is 0).Map(power => MathF.Pow(Base, power));
            Seq<float> minors = stride is 1 && MathF.Log(Base / (Base - 1f)) >= least * span
                ? from power in toSeq(Range(first - 1, int.Max(0, last - first + 2)))
                  from multiple in toSeq(Range(2, int.Max(0, (int)MathF.Ceiling(Base) - 2))).Filter(multiple => multiple < Base)
                  let tick = multiple * MathF.Pow(Base, power)
                  where tick >= Domain.Low && tick <= Domain.High
                  select tick
                : [];
            return toSeq((majors + minors).OrderBy(Fraction));
        }

        public override float Snapped(float value, int steps, float least) => Held(Ticked(value, steps, least));
    }

    public sealed record Scale(AxisRange Domain, Func<float, AxisRange, float> Forward, Func<float, AxisRange, float> Inverse) : Axis(Domain) {
        public override float Fraction(float value) => Forward(value, Domain);

        public override float Value(float fraction) => Inverse(fraction, Domain);

        public override Option<float> Step(float least) => None;

        public override Seq<float> Ticks(float least) =>
            least > 0f ? toSeq(Range(0, (int)MathF.Floor(1f / least) + 1)).Map(index => Snapped(Value(index * least), 0, least)).Distinct() : [];

        public override float Snapped(float value, int steps, float least) =>
            Nice(MathF.Abs(Value(Fraction(value) + least) - value)).Map(step => Held((MathF.Round(value / step, MidpointRounding.ToEven) + (steps * MathF.Sign(Domain.Span))) * step)).IfNone(value);
    }

    public sealed record Periodic(AxisRange Domain, int Grid, int Snap) : Axis(Domain) {
        private float Turn => MathF.Abs(Domain.Span);

        public override float Delta(float from, float to) {
            float delta = to - from;
            return delta - (Turn * MathF.Round(delta / Turn, MidpointRounding.ToEven));
        }

        public override float Held(float value) => Value(Unit(value));

        public override bool Contains(AxisRange band, float value) => band.Low + (Wrapped((value - band.Low) / Turn) * Turn) <= band.High;

        public override Seq<Stretch> Spans(AxisRange band) {
            float from = Fraction(band.From);
            float to = Fraction(band.To);
            return (Wrapped(from), Wrapped(to)) switch {
                _ when MathF.Abs(to - from) >= 1f => [new Stretch(0f, 1f)],
                var (low, high) when high > low => [new Stretch(low, high)],
                var (low, high) when high > 0f => [new Stretch(low, 1f), new Stretch(0f, high)],
                var (low, _) => [new Stretch(low, 1f)],
            };
        }

        public override Option<float> Step(float least) => Snap > 0 ? Some(Domain.Span / Snap) : None;

        public override Seq<float> Ticks(float least) {
            int divisions = toSeq(Range(1, int.Max(1, Grid))).Rev().Filter(divisor => Grid % divisor is 0).Find(divisor => 1f / divisor >= least).IfNone(1);
            return toSeq(Range(0, divisions)).Map(index => Domain.From + (index * Domain.Span / divisions));
        }

        public override float Snapped(float value, int steps, float least) =>
            Step(least).Map(step => Held(Domain.From + ((MathF.Round((value - Domain.From) / step, MidpointRounding.ToEven) + steps) * step))).IfNone(value);

        private protected override float Wrapped(float fraction) => ((fraction % 1f) + 1f) % 1f;
    }
}
