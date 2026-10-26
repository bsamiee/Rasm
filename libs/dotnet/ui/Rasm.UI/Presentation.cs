using System.Globalization;
using System.Numerics;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.UI;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IPresented<TSelf> where TSelf : notnull, IPresented<TSelf> {
    public static abstract StateParameter<TRecord> Bounded<TRecord>(Lens<TRecord, TSelf> lens);

    public static abstract StateParameter<TRecord> OptionalBounded<TRecord>(Lens<TRecord, Gated<TSelf>> lens);
}

public interface IPresented<TSelf, TKey> : IPresented<TSelf>, IObjectFactory<TSelf, TKey, ValidationError>, IConvertible<TKey>, IMinMaxValue<TSelf>
    where TSelf : notnull, IPresented<TSelf, TKey>
    where TKey : struct, INumber<TKey> {
    public static abstract Presentation<TKey> Presentation { get; }

    static StateParameter<TRecord> IPresented<TSelf>.Bounded<TRecord>(Lens<TRecord, TSelf> lens) => StateParameter.Bounded(lens, TSelf.Presentation);

    static StateParameter<TRecord> IPresented<TSelf>.OptionalBounded<TRecord>(Lens<TRecord, Gated<TSelf>> lens) => StateParameter.OptionalBounded(lens, TSelf.Presentation);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record NumberDisplay(Option<string> Symbol, double Step, int Decimals, bool Turn, string Pattern, CultureInfo Culture, Func<double, double> Shown, Func<double, double> Key) {
    public string Text(double key) =>
        Shown(key).ToString(Pattern, Culture) switch {
            var number => Symbol.Match(Some: symbol => $"{number} {symbol}", None: () => number),
        };
}

public sealed record Presentation<TKey> where TKey : struct, INumber<TKey> {
    private const float Resolution = 0.01f;

    public Option<UnitInfo> Unit { get; init; }
    public Option<(TKey Low, TKey High)> Soft { get; init; }
    public Option<TKey> Origin { get; init; }
    public Option<TKey> Step { get; init; }
    public Option<int> Decimals { get; init; }
    public Option<Func<AxisRange, Axis>> Track { get; init; } = Some<Func<AxisRange, Axis>>(static range => new Axis.Linear(range));

    private Option<UnitInfo> Displayed =>
        Unit.Map(static stated => Quantity.GetUnitInfo(stated.Value switch {
            AngleUnit => AngleUnit.Degree,
            RatioUnit.DecimalFraction => RatioUnit.Percent,
            var held => held,
        }));

    private static bool Integral => TKey.IsInteger(TKey.CreateSaturating(0.5d));

    public static TKey Keyed(double value) => TKey.CreateSaturating(Integral ? double.Round(value, MidpointRounding.ToEven) : value);

    public Option<Axis> Placed(TKey key, (TKey Low, TKey High) bounds) =>
        from track in Track
        from soft in Ranged(Soft.IfNone(bounds))
        let axis = track(soft)
        let position = axis.Fraction(float.CreateSaturating(key))
        from widened in position switch {
            < 0f => Ranged(float.Max(axis.Value((2f * position) - 1f), float.CreateSaturating(bounds.Low)), soft.To),
            > 1f => Ranged(soft.From, float.Min(axis.Value(2f * position), float.CreateSaturating(bounds.High))),
            _ => Some(soft),
        }
        select track(widened);

    public TKey Snapped(TKey key, int steps, (TKey Low, TKey High) bounds) =>
        Increment(bounds) switch {
            var step => TKey.Clamp(Keyed(Placed(key, bounds).Filter(static axis => axis is not Axis.Linear).Match(
                Some: axis => axis.Snapped(float.CreateSaturating(key), steps, (float)(step / double.Abs(axis.Range.Span))),
                None: () => (double.Round(double.CreateChecked(key) / step, MidpointRounding.ToEven) + steps) * step)), bounds.Low, bounds.High),
        };

    public NumberDisplay Display((TKey Low, TKey High) bounds, CultureInfo culture) {
        Func<double, double> shown = Across(Unit, Displayed);
        (TKey Low, TKey High) soft = Soft.IfNone(bounds);
        double step = Increment(bounds);
        bool curved = Track.Exists(track => Ranged(soft).Exists(range => track(range) is not Axis.Linear));
        int decimals = Decimals.IfNone(() => int.Max(0, (int)double.Ceiling(-double.Log10(
            curved && shown(double.CreateChecked(soft.Low)) is > 0d and var bottom ? bottom * Resolution : double.Abs(shown(step) - shown(0d))))));
        string digits = decimals is 0 ? "0" : $"0.{new string('0', decimals)}";
        return new(
            Displayed.Map(displayed => UnitAbbreviationsCache.Default.GetDefaultAbbreviation(displayed, culture)),
            step,
            decimals,
            Unit.Exists(static stated => stated.Value is AngleUnit.Radian)
                && double.Round(double.Abs(shown(double.CreateChecked(bounds.High)) - shown(double.CreateChecked(bounds.Low))), decimals, MidpointRounding.ToEven)
                    == double.Round(Angle.FromRevolutions(1).Degrees, decimals, MidpointRounding.ToEven),
            soft.Low < TKey.Zero ? $"+{digits};-{digits};{digits}" : digits,
            culture,
            shown,
            Across(Displayed, Unit));
    }

    private double Increment((TKey Low, TKey High) bounds) {
        (Func<double, double> shown, Func<double, double> entered, (TKey Low, TKey High) soft) = (Across(Unit, Displayed), Across(Displayed, Unit), Soft.IfNone(bounds));
        double increment = Step.Map(static declared => double.CreateChecked(declared)).IfNone(() =>
            Ranged((float)shown(double.CreateChecked(soft.Low)), (float)shown(double.CreateChecked(soft.High)))
                .Bind(static range => new Axis.Linear(range).Step(Resolution))
                .Map(step => double.Abs(entered(step) - entered(0d)))
                .IfNone(double.Epsilon));
        return Integral ? double.Max(1d, double.Round(increment, MidpointRounding.ToEven)) : increment;
    }

    private static Option<AxisRange> Ranged((TKey Low, TKey High) ends) => Ranged(float.CreateSaturating(ends.Low), float.CreateSaturating(ends.High));

    private static Option<AxisRange> Ranged(float from, float to) => ValidationResults.ToFin(AxisRange.Validate(from, to, out AxisRange? range), range).ToOption();

    private static Func<double, double> Across(Option<UnitInfo> from, Option<UnitInfo> to) =>
        (from.Case, to.Case) switch {
            (UnitInfo source, UnitInfo target) => value => UnitConverter.Convert(value, source.UnitKey, target.UnitKey),
            _ => static value => value,
        };
}

public sealed class NumericDomain<TValue, TKey> where TValue : notnull where TKey : struct, INumber<TKey> {
    public NumericDomain(Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key, (TKey Low, TKey High) bounds, Presentation<TKey> presentation) =>
        (Create, Key, Bounds, Presentation) = (raw => ValidationResults.ToFin(ValidationResults.OutsideRange(raw, bounds.Low, bounds.High), raw).Bind(create), key, bounds, presentation);

    public Func<TKey, Fin<TValue>> Create { get; }
    public Func<TValue, TKey> Key { get; }
    public (TKey Low, TKey High) Bounds { get; }
    public Presentation<TKey> Presentation { get; }

    public Fin<TValue> Checked(TValue value) => Create(Key(value)).Map(_ => value);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NumericDomain {
    public static NumericDomain<TValue, TKey> Bounded<TValue, TKey>(Presentation<TKey> presentation)
        where TValue : IObjectFactory<TValue, TKey, ValidationError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey> =>
        new(ValidationResults.Validated<TValue, TKey>, static value => value.ToValue(), (TValue.MinValue.ToValue(), TValue.MaxValue.ToValue()), presentation);

    public static NumericDomain<Length, TKey> Meters<TKey>((TKey Low, TKey High) bounds, Presentation<TKey> presentation) where TKey : struct, INumber<TKey> =>
        new(static raw => Length.FromMeters(double.CreateChecked(raw)), static length => TKey.CreateChecked(length.Meters), bounds, presentation with { Unit = Quantity.GetUnitInfo(LengthUnit.Meter) });
}

public static class StateParameter {
    public static StateParameter<TRecord> Bounded<TRecord, TValue>(Lens<TRecord, TValue> lens) where TValue : notnull, IPresented<TValue> => TValue.Bounded(lens);

    public static StateParameter<TRecord> Bounded<TRecord, TValue, TKey>(Lens<TRecord, TValue> lens, Presentation<TKey> presentation)
        where TValue : IObjectFactory<TValue, TKey, ValidationError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey> =>
        new StateParameter<TRecord>.Numeric<TValue, TKey>(lens, NumericDomain.Bounded<TValue, TKey>(presentation));

    public static StateParameter<TRecord> OptionalBounded<TRecord, TValue>(Lens<TRecord, Gated<TValue>> lens) where TValue : notnull, IPresented<TValue> => TValue.OptionalBounded(lens);

    public static StateParameter<TRecord> OptionalBounded<TRecord, TValue, TKey>(Lens<TRecord, Gated<TValue>> lens, Presentation<TKey> presentation)
        where TValue : IObjectFactory<TValue, TKey, ValidationError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey> =>
        new StateParameter<TRecord>.OptionalNumeric<TValue, TKey>(lens, NumericDomain.Bounded<TValue, TKey>(presentation));
}
