using System.Buffers;
using System.Globalization;
using System.Numerics.Tensors;
using NodaTime.Text;
using SuperLinq;

namespace Rasm.UI.Plotting;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FractionRange(float Low, float High);

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class AxisRange {
    public static AxisRange Unit { get; } = new(0f, 1f);

    public float From { get; }
    public float To { get; }

    public float Span => To - From;
    public float Low => float.Min(From, To);
    public float High => float.Max(From, To);

    public Option<AxisRange> Scaled(float from, float to, float factor) =>
        Factor.Validated(factor).ToOption().Bind(scale => Validated(to + ((From - from) * (float)scale), to + ((To - from) * (float)scale)).ToOption());

    public Option<AxisRange> Shifted(float delta) => Validated(From + delta, To + delta).ToOption();

    public static Fin<AxisRange> Validated(float from, float to) =>
        UIFault.Unless(float.IsFinite(from) && float.IsFinite(to) && from.CompareTo(to) is not 0, UIReason.Range, (from, to)).Map(_ => new AxisRange(from, to));

    internal Option<AxisRange> Toward(AxisRange to, float progress) => Validated(float.Lerp(From, to.From, progress), float.Lerp(To, to.To, progress)).ToOption();
}

public sealed record Tick(float Value, bool Major);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Axis {
    private const string Superscripts = "⁰¹²³⁴⁵⁶⁷⁸⁹⁻";

    private Axis(AxisRange domain) => Domain = domain;

    public AxisRange Domain { get; init; }

    public virtual float Fraction(float value) => (value - Domain.From) / Domain.Span;

    public virtual void Fractions(ReadOnlySpan<float> values, Span<float> into) {
        TensorPrimitives.Subtract(values, Domain.From, into);
        TensorPrimitives.Divide(into, Domain.Span, into);
    }

    public virtual float Value(float fraction) => Domain.From + (fraction * Domain.Span);

    public float Unit(float value) => Wrapped(Fraction(value));

    public virtual float Delta(float from, float to) => to - from;

    public virtual float Constrained(float value) => float.Clamp(value, Domain.Low, Domain.High);

    public virtual bool Contains(AxisRange band, float value) => value >= band.Low && value <= band.High;

    public virtual Seq<FractionRange> Spans(AxisRange band) {
        float from = Fraction(band.From);
        float to = Fraction(band.To);
        return [new FractionRange(float.Min(from, to), float.Max(from, to))];
    }

    public virtual Option<Axis> WithDomain(AxisRange range) => Some(this with { Domain = range });

    public int Decimals => Nice(0.01f * MathF.Abs(Domain.Span)).Map(static step => int.Max(0, (int)MathF.Ceiling(-MathF.Log10(step)))).IfNone(0);

    public virtual string Spoken(float value, CultureInfo culture) => MathF.Round(value, Decimals, MidpointRounding.ToEven).ToString(culture);

    public abstract Option<float> Step(float least);
    public abstract Seq<Tick> Ticks(float least);
    public abstract float Snapped(float value, int steps, float least);

    internal virtual float Bandwidth => 1f;

    internal bool Resolves(float pixels) {
        float largest = float.Max(MathF.Abs(Domain.From), MathF.Abs(Domain.To));
        return MathF.Abs(Domain.Span) >= pixels * (MathF.BitIncrement(largest) - largest);
    }

    internal Option<Axis> Toward(Axis to, float progress) => Some(to with { Domain = Domain }).Filter(same => same == this).Bind(_ => Domain.Toward(to.Domain, progress)).Bind(WithDomain);

    private protected virtual float Wrapped(float fraction) => fraction;

    private protected float Ticked(float value, int steps, float least) {
        Seq<float> ticks = Ticks(least).Map(static tick => tick.Value).Strict();
        float at = Fraction(value);
        return ticks.IsEmpty ? value : ticks[int.Clamp(toSeq(Range(0, ticks.Count)).MinBy(index => MathF.Abs(Fraction(ticks[index]) - at)) + steps, 0, ticks.Count - 1)];
    }

    private protected Seq<Tick> Spaced(float least) =>
        Step(least).Map(step => {
            float minor = step / 5f;
            int first = (int)MathF.Ceiling(Domain.Low / minor);
            return toSeq(Range(first, int.Max(0, (int)MathF.Floor(Domain.High / minor) - first + 1))).Map(multiple => new Tick(multiple * minor, multiple % 5 is 0));
        }).IfNone([]);

    private protected float Rounded(float value, int steps, float least) =>
        Step(least).Map(step => Constrained((MathF.Round(value / step, MidpointRounding.ToEven) + (steps * MathF.Sign(Domain.Span))) * step)).IfNone(value);

    private static Option<float> Nice(float span) {
        float decade = float.Exp10(MathF.Floor(MathF.Log10(span)));
        return float.IsFinite(span) && span > 0f ? Seq(1f, 2f, 5f, 10f).Map(step => step * decade).Find(step => step >= span) : None;
    }

    public sealed record Linear(AxisRange Domain) : Axis(Domain) {
        public override Option<float> Step(float least) => Nice(least * MathF.Abs(Domain.Span));

        public override Seq<Tick> Ticks(float least) => Spaced(least);

        public override float Snapped(float value, int steps, float least) => Rounded(value, steps, least);
    }

    public sealed record Power(AxisRange Domain) : Axis(Domain) {
        public Factor Exponent { get; init; } = Factor.Validated(0.5f).ThrowIfFail();

        private float Start => Raised(Domain.From, (float)Exponent);
        private float Depth => Raised(Domain.To, (float)Exponent) - Start;

        public override float Fraction(float value) => (Raised(value, (float)Exponent) - Start) / Depth;

        public override void Fractions(ReadOnlySpan<float> values, Span<float> into) {
            TensorPrimitives.Abs(values, into);
            TensorPrimitives.Pow(into, (float)Exponent, into);
            TensorPrimitives.CopySign(into, values, into);
            TensorPrimitives.Subtract(into, Start, into);
            TensorPrimitives.Divide(into, Depth, into);
        }

        public override float Value(float fraction) => Raised(Start + (fraction * Depth), 1f / (float)Exponent);

        public override Option<float> Step(float least) => Nice(least * MathF.Abs(Domain.Span));

        public override Seq<Tick> Ticks(float least) => Spaced(least);

        public override float Snapped(float value, int steps, float least) => Rounded(value, steps, least);

        private static float Raised(float value, float exponent) => MathF.CopySign(MathF.Pow(MathF.Abs(value), exponent), value);
    }

    public sealed record SymLog(AxisRange Domain) : Axis(Domain) {
        public Factor Constant { get; init; } = Factor.Unit;

        private float Depth => float.LogP1(MathF.Abs(Domain.Span) / (float)Constant);

        public override float Fraction(float value) {
            float along = (value - Domain.From) * MathF.Sign(Domain.Span) / (float)Constant;
            return MathF.CopySign(float.LogP1(MathF.Abs(along)), along) / Depth;
        }

        public override void Fractions(ReadOnlySpan<float> values, Span<float> into) {
            float[] magnitude = ArrayPool<float>.Shared.Rent(values.Length);
            Span<float> held = magnitude.AsSpan(0, values.Length);
            TensorPrimitives.Subtract(values, Domain.From, into);
            TensorPrimitives.Multiply(into, MathF.Sign(Domain.Span) / (float)Constant, into);
            TensorPrimitives.Abs(into, held);
            TensorPrimitives.LogP1(held, held);
            TensorPrimitives.CopySign(held, into, into);
            TensorPrimitives.Divide(into, Depth, into);
            ArrayPool<float>.Shared.Return(magnitude);
        }

        public override float Value(float fraction) => Domain.From + (MathF.Sign(Domain.Span) * (float)Constant * MathF.CopySign(float.ExpM1(MathF.Abs(fraction) * Depth), fraction));

        public override Option<float> Step(float least) => Some(MathF.Ceiling(least * Depth / MathF.Log(10f))).Filter(float.IsFinite).Map(static decades => float.Exp10(float.Max(1f, decades)));

        public override Seq<Tick> Ticks(float least) =>
            Step(least).Map(factor => new Tick(Domain.From, Major: true).Cons(toSeq(Range(0, int.Max(0, (int)MathF.Floor(MathF.Log10(MathF.Abs(Domain.Span) / (float)Constant) / MathF.Log10(factor)) + 1)))
                .Map(power => new Tick(Domain.From + (MathF.Sign(Domain.Span) * (float)Constant * MathF.Pow(factor, power)), Major: true)))).IfNone([]);

        public override float Snapped(float value, int steps, float least) => Ticked(value, steps, least);
    }

    public sealed record Log : Axis {
        private const float Tolerance = 1e-4f;

        private Log(AxisRange domain, float @base) : base(domain) => Base = @base;

        public float Base { get; }

        private float Depth => MathF.Log(Domain.To / Domain.From);

        public override float Fraction(float value) => MathF.Log(value / Domain.From) / Depth;

        public override void Fractions(ReadOnlySpan<float> values, Span<float> into) {
            TensorPrimitives.Divide(values, Domain.From, into);
            TensorPrimitives.Log(into, into);
            TensorPrimitives.Divide(into, Depth, into);
        }

        public override float Value(float fraction) => Domain.From * MathF.Exp(fraction * Depth);

        public static Fin<Axis> Validated(AxisRange domain, float @base) =>
            UIFault.Unless(domain.Low > 0f && @base > 1f, UIReason.Range, (domain, @base)).Map(_ => (Axis)new Log(domain, @base));

        public override Option<Axis> WithDomain(AxisRange range) => Validated(range, Base).ToOption();

        public override string Spoken(float value, CultureInfo culture) {
            float power = MathF.Log(value, Base);
            float whole = MathF.Round(power, MidpointRounding.ToEven);
            return MathF.Abs(power - whole) <= Tolerance
                ? string.Concat(Base.ToString(culture), string.Concat(((int)whole).ToString(CultureInfo.InvariantCulture).Select(static digit => Superscripts[digit is '-' ? 10 : digit - '0'])))
                : MathF.Round(value, int.Max(0, 2 - (int)MathF.Floor(MathF.Log10(value))), MidpointRounding.ToEven).ToString(culture);
        }

        public override Option<float> Step(float least) => None;

        public override Seq<Tick> Ticks(float least) {
            int first = (int)MathF.Ceiling(MathF.Log(Domain.Low, Base) - Tolerance);
            int last = (int)MathF.Floor(MathF.Log(Domain.High, Base) + Tolerance);
            float span = MathF.Abs(Depth);
            int stride = int.Max(1, (int)MathF.Ceiling(least * span / MathF.Log(Base)));
            Seq<Tick> majors = toSeq(Range(first, int.Max(0, last - first + 1))).Filter(power => power % stride is 0).Map(power => new Tick(MathF.Pow(Base, power), Major: true));
            Seq<Tick> minors = stride is 1 && MathF.Log(Base / (Base - 1f)) >= least * span
                ? from power in toSeq(Range(first - 1, int.Max(0, last - first + 2)))
                  from multiple in toSeq(Range(2, int.Max(0, (int)MathF.Ceiling(Base) - 2))).Filter(multiple => multiple < Base)
                  let tick = multiple * MathF.Pow(Base, power)
                  where tick >= Domain.Low && tick <= Domain.High
                  select new Tick(tick, Major: false)
                : [];
            return toSeq((majors + minors).OrderBy(tick => Fraction(tick.Value)));
        }

        public override float Snapped(float value, int steps, float least) => Constrained(Ticked(value, steps, least));
    }

    public sealed record Time(AxisRange Domain, Instant Origin, DateTimeZone Zone) : Axis(Domain) {
        private static readonly Duration Year = Duration.FromDays(146097d / 400d);

        private static readonly Seq<Rung> Rungs =
            from calendar in Seq(
                new Calendar(static local => local.With(TimeAdjusters.TruncateToSecond), static local => local.Second, static count => Period.FromSeconds(count), Duration.FromSeconds(1), "HH:mm:ss", [1, 5, 15, 30]),
                new Calendar(static local => local.With(TimeAdjusters.TruncateToMinute), static local => local.Minute, static count => Period.FromMinutes(count), Duration.FromMinutes(1), "HH:mm", [1, 5, 15, 30]),
                new Calendar(static local => local.With(TimeAdjusters.TruncateToHour), static local => local.Hour, static count => Period.FromHours(count), Duration.FromHours(1), "HH:mm", [1, 3, 6, 12]),
                new Calendar(static local => local.Date.AtMidnight(), static local => local.Day - 1, static count => Period.FromDays(count), Duration.FromDays(1), "MMM d", [1, 2, 7]),
                new Calendar(static local => local.Date.With(DateAdjusters.StartOfMonth).AtMidnight(), static local => local.Month - 1, static count => Period.FromMonths(count), Year / 12, "MMM uuuu", [1, 3, 6]))
            from count in calendar.Counts
            select new Rung(calendar, count);

        private static readonly Calendar Years =
            new(static local => local.Date.With(DateAdjusters.StartOfMonth).With(DateAdjusters.Month(1)).AtMidnight(), static local => local.Year, static count => Period.FromYears(count), Year, "uuuu", []);

        public override string Spoken(float value, CultureInfo culture) =>
            LocalDateTimePattern.Create(Laddered(0.01f).Calendar.Pattern, culture).Format((Origin + Duration.FromSeconds(value)).InZone(Zone).LocalDateTime);

        public override Option<float> Step(float least) => None;

        public override Seq<Tick> Ticks(float least) {
            Rung major = Laddered(least);
            Seq<float> majors = Placed(major);
            HashSet<float> kept = toHashSet(majors);
            Seq<float> minors = Rungs.TakeWhile(rung => rung != major).Last.Map(Placed).IfNone([]).Filter(at => !kept.Contains(at));
            return toSeq(majors.Map(static at => new Tick(at, Major: true)).Concat(minors.Map(static at => new Tick(at, Major: false))).OrderBy(static tick => tick.Value));
        }

        public override float Snapped(float value, int steps, float least) => Constrained(Ticked(value, steps, least));

        private Rung Laddered(float least) {
            Duration need = Duration.FromSeconds(least * MathF.Abs(Domain.Span));
            return Rungs.Find(rung => rung.Length >= need).IfNone(() => new Rung(Years, (int)MathF.Ceiling(Nice((float)(need / Year)).IfNone(1f))));
        }

        private Seq<float> Placed(Rung rung) {
            LocalDateTime start = rung.Calendar.Truncated((Origin + Duration.FromSeconds(Domain.Low)).InZone(Zone).LocalDateTime);
            return toSeq(SuperEnumerable.Generate(start - rung.Calendar.Steps(rung.Calendar.Field(start) % rung.Count), local => local + rung.Calendar.Steps(rung.Count))
                .Select(local => (float)(local.InZoneLeniently(Zone).ToInstant() - Origin).TotalSeconds)
                .SkipWhile(at => at < Domain.Low)
                .TakeWhile(at => at <= Domain.High)
                .Distinct());
        }

        private sealed record Calendar(Func<LocalDateTime, LocalDateTime> Truncated, Func<LocalDateTime, int> Field, Func<int, Period> Steps, Duration Length, string Pattern, Seq<int> Counts);

        private sealed record Rung(Calendar Calendar, int Count) {
            public Duration Length => Calendar.Length * Count;
        }
    }

    public sealed record Band : Axis {
        private Band(AxisRange domain, Seq<string> labels, Fraction padding) : base(domain) => (Labels, Padding) = (labels, padding);

        public Seq<string> Labels { get; }
        public Fraction Padding { get; }

        internal override float Bandwidth => (float)Padding.Complement;

        public static Fin<Band> Validated(Seq<string> labels, Fraction padding) => AxisRange.Validated(-0.5f, labels.Count - 0.5f).Map(domain => new Band(domain, labels, padding));

        public override string Spoken(float value, CultureInfo culture) => Labels[Indexed(MathF.Round(value, MidpointRounding.ToEven))];

        public override Option<float> Step(float least) => Some(float.Max(1f, MathF.Ceiling(least * MathF.Abs(Domain.Span))));

        public override Seq<Tick> Ticks(float least) =>
            Step(least).Map(stride => toSeq(Range(0, Labels.Count)).Filter(index => index % (int)stride is 0 && index >= Domain.Low && index <= Domain.High).Map(static index => new Tick(index, Major: true))).IfNone([]);

        public override float Snapped(float value, int steps, float least) => Constrained(Indexed(MathF.Round(value, MidpointRounding.ToEven) + (steps * MathF.Sign(Domain.Span))));

        private int Indexed(float value) => int.Clamp((int)value, 0, Labels.Count - 1);
    }

    public sealed record Scale(AxisRange Domain, Func<float, AxisRange, float> Forward, Func<float, AxisRange, float> Inverse) : Axis(Domain) {
        public override float Fraction(float value) => Forward(value, Domain);

        public override void Fractions(ReadOnlySpan<float> values, Span<float> into) {
            for (int index = 0; index < values.Length; index++) into[index] = Forward(values[index], Domain);
        }

        public override float Value(float fraction) => Inverse(fraction, Domain);

        public override Option<float> Step(float least) => None;

        public override Seq<Tick> Ticks(float least) =>
            least > 0f ? toSeq(Range(0, (int)MathF.Floor(1f / least) + 1)).Map(index => Snapped(Value(index * least), 0, least)).Distinct().Map(static at => new Tick(at, Major: true)) : [];

        public override float Snapped(float value, int steps, float least) =>
            Nice(MathF.Abs(Value(Fraction(value) + least) - value)).Map(step => Constrained((MathF.Round(value / step, MidpointRounding.ToEven) + (steps * MathF.Sign(Domain.Span))) * step)).IfNone(value);
    }

    public sealed record Periodic(AxisRange Domain, int Grid, int Snap) : Axis(Domain) {
        private float Period => MathF.Abs(Domain.Span);

        public override float Delta(float from, float to) {
            float delta = to - from;
            return delta - (Period * MathF.Round(delta / Period, MidpointRounding.ToEven));
        }

        public override float Constrained(float value) => Value(Unit(value));

        public override bool Contains(AxisRange band, float value) => band.Low + (Wrapped((value - band.Low) / Period) * Period) <= band.High;

        public override Seq<FractionRange> Spans(AxisRange band) {
            float from = Fraction(band.From);
            float to = Fraction(band.To);
            return (Wrapped(from), Wrapped(to)) switch {
                _ when MathF.Abs(to - from) >= 1f => [new FractionRange(0f, 1f)],
                var (low, high) when high > low => [new FractionRange(low, high)],
                var (low, high) when high > 0f => [new FractionRange(low, 1f), new FractionRange(0f, high)],
                var (low, _) => [new FractionRange(low, 1f)],
            };
        }

        public override Option<float> Step(float least) => Snap > 0 ? Some(Domain.Span / Snap) : None;

        public override Seq<Tick> Ticks(float least) {
            int divisions = toSeq(Range(1, int.Max(1, Grid))).Rev().Filter(divisor => Grid % divisor is 0).Find(divisor => 1f / divisor >= least).IfNone(1);
            return toSeq(Range(0, divisions)).Map(index => new Tick(Domain.From + (index * Domain.Span / divisions), Major: true));
        }

        public override float Snapped(float value, int steps, float least) =>
            Step(least).Map(step => Constrained(Domain.From + ((MathF.Round((value - Domain.From) / step, MidpointRounding.ToEven) + steps) * step))).IfNone(value);

        private protected override float Wrapped(float fraction) => ((fraction % 1f) + 1f) % 1f;
    }
}
