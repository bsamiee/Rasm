using System.Buffers;
using System.Globalization;
using System.Numerics.Tensors;
using MathNet.Numerics;
using NodaTime.Text;
using SuperLinq;

namespace Rasm.UI.Plotting;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class AxisRange {
    public static AxisRange Unit { get; } = new(0f, 1f);

    public float From { get; }
    public float To { get; }

    public float Span => To - From;
    public float Mid => (From + To) / 2f;
    public float Low => float.Min(From, To);
    public float High => float.Max(From, To);

    public bool Contains(float value) => value >= Low && value <= High;

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
    private const float Resolution = 0.01f;

    private Axis(AxisRange domain) => Domain = domain;

    public AxisRange Domain { get; init; }
    public Option<int> Places { get; init; }
    public Option<string> Unit { get; init; }

    public int Decimals => Places.IfNone(() => Nice(Resolution * MathF.Abs(Domain.Span)).Map(Digits).IfNone(0));

    public virtual float Fraction(float value) => (value - Domain.From) / Domain.Span;

    public virtual float Value(float fraction) => Domain.From + (fraction * Domain.Span);

    public abstract Seq<Tick> Ticks(float least);

    public string Spoken(float value, CultureInfo culture) => string.Join(' ', Formatted(value, culture).Cons(Unit.ToSeq()));

    public virtual Option<Axis> WithDomain(AxisRange range) => Some(this with { Domain = range });

    internal static int Digits(float step) => int.Max(0, (int)MathF.Ceiling(-MathF.Log10(step)));

    internal virtual float Bandwidth => 1f;

    internal virtual void Fractions(ReadOnlySpan<float> values, Span<float> into) {
        TensorPrimitives.Subtract(values, Domain.From, into);
        TensorPrimitives.Divide(into, Domain.Span, into);
    }

    internal virtual float Wrapped(float fraction) => fraction;

    internal virtual float Delta(float from, float to) => to - from;

    internal virtual float Constrained(float value) => float.Clamp(value, Domain.Low, Domain.High);

    internal virtual bool Contains(AxisRange band, float value) => band.Contains(value);

    internal virtual Seq<(float Low, float High)> Spans(AxisRange band) =>
        (Fraction(band.From), Fraction(band.To)) switch { var (from, to) => [(float.Min(from, to), float.Max(from, to))] };

    internal abstract float Snapped(float value, int steps, float least);

    internal virtual string Formatted(float value, CultureInfo culture) => value.ToString(Pattern(Decimals), culture);

    internal bool Resolves(float pixels) {
        float largest = float.Max(MathF.Abs(Domain.From), MathF.Abs(Domain.To));
        return MathF.Abs(Domain.Span) >= pixels * (MathF.BitIncrement(largest) - largest);
    }

    internal Option<Axis> Toward(Axis to, float progress) => Some(to with { Domain = Domain }).Filter(same => same == this).Bind(_ => Domain.Toward(to.Domain, progress)).Bind(WithDomain);

    private protected float Ticked(float value, int steps, float least) {
        Seq<float> ticks = Ticks(least).Map(static tick => tick.Value).Strict();
        float at = Fraction(value);
        return ticks.IsEmpty ? value : ticks[int.Clamp(toSeq(Range(0, ticks.Count)).MinBy(index => MathF.Abs(Fraction(ticks[index]) - at)) + steps, 0, ticks.Count - 1)];
    }

    private protected float Rounded(float value, int steps, Option<float> step) =>
        step.Map(held => Constrained((MathF.Round(value / held, MidpointRounding.ToEven) + (steps * MathF.Sign(Domain.Span))) * held)).IfNone(value);

    private protected static string Pattern(int places) => $"#,0.{new string('#', places)};;0";

    private static Option<float> Nice(float span) {
        float decade = float.Exp10(MathF.Floor(MathF.Log10(span)));
        return float.IsFinite(span) && span > 0f ? Seq(1f, 2f, 5f, 10f).Map(step => step * decade).Find(step => step >= span) : None;
    }

    public sealed record Linear(AxisRange Domain) : Axis(Domain) {
        public Factor Exponent { get; init; } = Factor.Unit;

        private float Start => Raised(Domain.From, (float)Exponent);
        private float Depth => Raised(Domain.To, (float)Exponent) - Start;

        public override float Fraction(float value) => (Raised(value, (float)Exponent) - Start) / Depth;

        public override float Value(float fraction) => Raised(Start + (fraction * Depth), 1f / (float)Exponent);

        public override Seq<Tick> Ticks(float least) =>
            Nice(least * MathF.Abs(Domain.Span)).Map(step => {
                int divisions = MathF.Round(step / float.Exp10(MathF.Floor(MathF.Log10(step))), MidpointRounding.ToEven) is 2f ? 4 : 5;
                float minor = step / divisions;
                int first = (int)MathF.Ceiling(Domain.Low / minor);
                return toSeq(Range(first, int.Max(0, (int)MathF.Floor(Domain.High / minor) - first + 1))).Map(multiple => new Tick(multiple * minor, multiple % divisions is 0));
            }).IfNone([]);

        internal override void Fractions(ReadOnlySpan<float> values, Span<float> into) {
            TensorPrimitives.Abs(values, into);
            TensorPrimitives.Pow(into, (float)Exponent, into);
            TensorPrimitives.CopySign(into, values, into);
            TensorPrimitives.Subtract(into, Start, into);
            TensorPrimitives.Divide(into, Depth, into);
        }

        internal override float Snapped(float value, int steps, float least) => Rounded(value, steps, Nice(least * MathF.Abs(Domain.Span)));

        private static float Raised(float value, float exponent) => MathF.CopySign(MathF.Pow(MathF.Abs(value), exponent), value);
    }

    public sealed record SymLog(AxisRange Domain) : Axis(Domain) {
        public Factor Constant { get; init; } = Factor.Unit;

        private float Start => Transformed(Domain.From);
        private float Depth => Transformed(Domain.To) - Start;

        public override float Fraction(float value) => (Transformed(value) - Start) / Depth;

        public override float Value(float fraction) => (Start + (fraction * Depth)) switch { var at => MathF.CopySign(float.ExpM1(MathF.Abs(at)), at) * (float)Constant };

        public override Seq<Tick> Ticks(float least) {
            float factor = float.Exp10(float.Max(1f, MathF.Ceiling(least * MathF.Abs(Depth) / MathF.Log(10f))));
            int powers = float.IsFinite(factor) ? int.Max(0, (int)MathF.Floor(MathF.Log10(float.Max(MathF.Abs(Domain.Low), MathF.Abs(Domain.High)) / (float)Constant) / MathF.Log10(factor)) + 1) : 0;
            Seq<float> majors = toSeq((from power in toSeq(Range(0, powers)) from sign in Seq(-1f, 1f) select sign * (float)Constant * MathF.Pow(factor, power)).Add(0f).Filter(Domain.Contains).OrderBy(Fraction));
            return majors.Count >= 3 ? majors.Map(static at => new Tick(at, Major: true)) : new Linear(Domain).Ticks(least);
        }

        internal override void Fractions(ReadOnlySpan<float> values, Span<float> into) {
            using IMemoryOwner<float> magnitude = MemoryPool<float>.Shared.Rent(values.Length);
            Span<float> held = magnitude.Memory.Span[..values.Length];
            TensorPrimitives.Abs(values, held);
            TensorPrimitives.Divide(held, (float)Constant, held);
            TensorPrimitives.LogP1(held, held);
            TensorPrimitives.CopySign(held, values, into);
            TensorPrimitives.Subtract(into, Start, into);
            TensorPrimitives.Divide(into, Depth, into);
        }

        internal override float Snapped(float value, int steps, float least) => Ticked(value, steps, least);

        private float Transformed(float value) => MathF.CopySign(float.LogP1(MathF.Abs(value) / (float)Constant), value);
    }

    public sealed record Log : Axis {
        private const float Tolerance = 1e-4f;
        private const string Superscripts = "⁰¹²³⁴⁵⁶⁷⁸⁹⁻";

        private Log(AxisRange domain, float @base) : base(domain) => Base = @base;

        public float Base { get; }

        private float Depth => MathF.Log(Domain.To / Domain.From);

        public override float Fraction(float value) => MathF.Log(value / Domain.From) / Depth;

        public override float Value(float fraction) => Domain.From * MathF.Exp(fraction * Depth);

        public static Fin<Log> Validated(AxisRange domain, float @base) => UIFault.Unless(domain.Low > 0f && @base > 1f, UIReason.Range, (domain, @base)).Map(_ => new Log(domain, @base));

        public override Option<Axis> WithDomain(AxisRange range) => Validated(range, Base).Map(log => (Axis)(log with { Places = Places, Unit = Unit })).ToOption();

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

        internal override void Fractions(ReadOnlySpan<float> values, Span<float> into) {
            TensorPrimitives.Divide(values, Domain.From, into);
            TensorPrimitives.Log(into, into);
            TensorPrimitives.Divide(into, Depth, into);
        }

        internal override float Snapped(float value, int steps, float least) => Constrained(Ticked(value, steps, least));

        internal override string Formatted(float value, CultureInfo culture) {
            float power = MathF.Log(value, Base);
            float whole = MathF.Round(power, MidpointRounding.ToEven);
            return MathF.Abs(power - whole) <= Tolerance
                ? string.Concat(Base.ToString(culture), string.Concat(((int)whole).ToString(CultureInfo.InvariantCulture).Select(static digit => Superscripts[digit is '-' ? 10 : digit - '0'])))
                : value.ToString(Pattern(Places.IfNone(() => int.Max(0, 2 - (int)MathF.Floor(MathF.Log10(value))))), culture);
        }
    }

    public sealed record Time(AxisRange Domain, Instant Origin, DateTimeZone Zone) : Axis(Domain) {
        private static readonly LocalDateTime Epoch = new LocalDate(0, 1, 1).AtMidnight();

        private static readonly Seq<(PeriodUnits Unit, int Count)> Steps =
            from row in Seq<(PeriodUnits Unit, Seq<int> Counts)>(
                (PeriodUnits.Seconds, [1, 5, 15, 30]), (PeriodUnits.Minutes, [1, 5, 15, 30]), (PeriodUnits.Hours, [1, 3, 6, 12]),
                (PeriodUnits.Days, [1, 2]), (PeriodUnits.Weeks, [1]), (PeriodUnits.Months, [1, 3, 6]))
            from count in row.Counts
            select (row.Unit, count);

        public IsoDayOfWeek WeekStart { get; init; } = IsoDayOfWeek.Monday;

        public override Seq<Tick> Ticks(float least) {
            (PeriodUnits Unit, int Count) major = Chosen(least);
            Seq<float> majors = Placed(major);
            HashSet<float> kept = toHashSet(majors);
            Seq<float> minors = Steps.TakeWhile(step => step != major).Last.Map(Placed).IfNone([]).Filter(at => !kept.Contains(at));
            return toSeq(majors.Map(static at => new Tick(at, Major: true)).Concat(minors.Map(static at => new Tick(at, Major: false))).OrderBy(static tick => tick.Value));
        }

        internal override float Snapped(float value, int steps, float least) => Constrained(Ticked(value, steps, least));

        internal override string Formatted(float value, CultureInfo culture) =>
            LocalDateTimePattern.Create(Chosen(Resolution).Unit switch {
                PeriodUnits.Seconds => culture.DateTimeFormat.LongTimePattern,
                PeriodUnits.Minutes or PeriodUnits.Hours => culture.DateTimeFormat.ShortTimePattern,
                PeriodUnits.Days or PeriodUnits.Weeks => culture.DateTimeFormat.MonthDayPattern,
                PeriodUnits.Months => culture.DateTimeFormat.YearMonthPattern,
                _ => "uuuu",
            }, culture).Format(Local(value));

        private (PeriodUnits Unit, int Count) Chosen(float least) {
            LocalDateTime low = Local(Domain.Low);
            double need = least * MathF.Abs(Domain.Span);
            double Length(PeriodUnits units, int count) => (Placing(low.Plus(Stride(units, count))) - Placing(low)).TotalSeconds;
            return Steps.Find(step => Length(step.Unit, step.Count) >= need)
                .IfNone(() => (PeriodUnits.Years, (int)MathF.Ceiling(Nice((float)(need / Length(PeriodUnits.Years, 1))).IfNone(1f))));
        }

        private Seq<float> Placed((PeriodUnits Unit, int Count) step) {
            LocalDateTime epoch = step.Unit is PeriodUnits.Weeks ? Epoch.With(DateAdjusters.PreviousOrSame(WeekStart)) : Epoch;
            long elapsed = Period.Between(epoch, Local(Domain.Low), step.Unit).ToBuilder()[step.Unit];
            return toSeq(SuperEnumerable.Generate(elapsed - (elapsed % step.Count), count => count + step.Count)
                .Select(count => (float)(Placing(epoch.Plus(Stride(step.Unit, count))) - Origin).TotalSeconds)
                .SkipWhile(at => at < Domain.Low)
                .TakeWhile(at => at <= Domain.High)
                .Distinct());
        }

        private Instant Placing(LocalDateTime local) => local.InZoneLeniently(Zone).ToInstant();

        private LocalDateTime Local(float seconds) => (Origin + Duration.FromSeconds(seconds)).InZone(Zone).LocalDateTime;

        private static Period Stride(PeriodUnits units, long count) => new PeriodBuilder { [units] = count }.Build();
    }

    public sealed record Band : Axis {
        private Band(AxisRange domain, Seq<string> labels, Fraction padding) : base(domain) => (Labels, Padding) = (labels, padding);

        public Seq<string> Labels { get; }
        public Fraction Padding { get; }

        public static Fin<Band> Validated(Seq<string> labels, Fraction padding) => AxisRange.Validated(-0.5f, labels.Count - 0.5f).Map(domain => new Band(domain, labels, padding));

        public override Seq<Tick> Ticks(float least) =>
            (int)float.Max(1f, MathF.Ceiling(least * MathF.Abs(Domain.Span))) switch {
                var stride => toSeq(Range(0, Labels.Count)).Filter(index => index % stride is 0 && index >= Domain.Low && index <= Domain.High).Map(static index => new Tick(index, Major: true)),
            };

        public float Edge(float value, int steps) => int.Clamp((int)MathF.Round(value + 0.5f, MidpointRounding.ToEven) + (steps * MathF.Sign(Domain.Span)), 0, Labels.Count) - 0.5f;

        internal override float Bandwidth => (float)Padding.Complement;

        internal override float Snapped(float value, int steps, float least) => Constrained(Indexed(MathF.Round(value, MidpointRounding.ToEven) + (steps * MathF.Sign(Domain.Span))));

        internal override string Formatted(float value, CultureInfo culture) => Labels[Indexed(MathF.Round(value, MidpointRounding.ToEven))];

        private int Indexed(float value) => int.Clamp((int)value, 0, Labels.Count - 1);
    }

    public sealed record Scale(AxisRange Domain, Func<float, AxisRange, float> Forward, Func<float, AxisRange, float> Inverse) : Axis(Domain) {
        public override float Fraction(float value) => Forward(value, Domain);

        public override float Value(float fraction) => Inverse(fraction, Domain);

        public override Seq<Tick> Ticks(float least) =>
            least > 0f ? toSeq(Range(0, (int)MathF.Floor(1f / least) + 1)).Map(index => Snapped(Value(index * least), 0, least)).Distinct().Map(static at => new Tick(at, Major: true)) : [];

        internal override void Fractions(ReadOnlySpan<float> values, Span<float> into) {
            for (int index = 0; index < values.Length; index++) into[index] = Forward(values[index], Domain);
        }

        internal override float Snapped(float value, int steps, float least) => Rounded(value, steps, Nice(MathF.Abs(Value(Fraction(value) + least) - value)));
    }

    public sealed record Periodic(AxisRange Domain, int Grid, int Snap) : Axis(Domain) {
        private float Period => MathF.Abs(Domain.Span);

        public override Seq<Tick> Ticks(float least) {
            int divisions = toSeq(Range(1, int.Max(1, Grid))).Rev().Filter(divisor => Grid % divisor is 0).Find(divisor => 1f / divisor >= least).IfNone(1);
            return toSeq(Range(0, divisions)).Map(index => new Tick(Domain.From + (index * Domain.Span / divisions), Major: true));
        }

        internal override float Wrapped(float fraction) => Euclid.Modulus(fraction, 1f);

        internal override float Delta(float from, float to) => float.Ieee754Remainder(to - from, Period);

        internal override float Constrained(float value) => Value(Wrapped(Fraction(value)));

        internal override Seq<(float Low, float High)> Spans(AxisRange band) =>
            (Fraction(band.From), Fraction(band.To)) switch {
                var (from, to) when MathF.Abs(to - from) >= 1f => [(0f, 1f)],
                var (from, to) => (Wrapped(from), Wrapped(to)) switch {
                    var (low, high) when high > low => [(low, high)],
                    var (low, high) when high > 0f => [(low, 1f), (0f, high)],
                    var (low, _) => [(low, 1f)],
                },
            };

        internal override float Snapped(float value, int steps, float least) =>
            Snap > 0 ? (Domain.Span / Snap) switch { var step => Constrained(Domain.From + ((MathF.Round((value - Domain.From) / step, MidpointRounding.ToEven) + steps) * step)) } : value;

        internal override bool Contains(AxisRange band, float value) => band.Contains(band.Low + Euclid.Modulus(value - band.Low, Period));
    }
}
