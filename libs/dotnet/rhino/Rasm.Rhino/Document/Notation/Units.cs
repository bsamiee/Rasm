using System.Numerics;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Geometry.Intersect;
using Rhino.UI;
using UnitsNet;

namespace Rasm.Rhino.Document.Notation;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<InvalidRhinoValue>]
public sealed partial class Tolerances {
    // --- [VALUES]
    public double Absolute { get; }
    public double Relative { get; }
    public double Angle { get; }
    public double MeshCoefficient { get; }
    public double MeshIntersection => Absolute * MeshCoefficient;

    // --- [FACTORY]
    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double absolute, ref double relative, ref double angle, ref double meshCoefficient) =>
        validationError = absolute > 0 && relative is > 0 and < 1 && angle is > 0 and <= double.Pi ? null : new InvalidRhinoValue();

    // --- [DOCUMENT]
    public static IO<Tolerances> Read(RhinoDoc doc, bool modelUnits) =>
        IO.lift(Fin<Tolerances> () => (modelUnits
            ? (doc.ModelAbsoluteTolerance, doc.ModelRelativeTolerance, doc.ModelAngleToleranceRadians)
            : (doc.PageAbsoluteTolerance, doc.PageRelativeTolerance, doc.PageAngleToleranceRadians)) switch {
                var (absolute, relative, angle) => Validate(absolute, relative, angle, Intersection.MeshIntersectionsTolerancesCoefficient, out Tolerances? value) is { } error ? error : value!,
            });

    public IO<Unit> Write(RhinoDoc doc, bool modelUnits, CallbackSite site) {
        IO<Unit> Assign(Tolerances value) => IO.lift(() => {
            if (modelUnits)
                (doc.ModelAbsoluteTolerance, doc.ModelRelativeTolerance, doc.ModelAngleToleranceRadians) = (value.Absolute, value.Relative, value.Angle);
            else
                (doc.PageAbsoluteTolerance, doc.PageRelativeTolerance, doc.PageAngleToleranceRadians) = (value.Absolute, value.Relative, value.Angle);
        });
        return
            from prior in Read(doc, modelUnits)
            from registered in TableOps.Register(doc, new CustomUndo(nameof(Tolerances), Assign(prior), Assign(this), site))
            from written in Assign(this)
            select written;
    }
}

[ComplexValueObject]
[ValidationError<InvalidRhinoValue>]
public sealed partial class DistanceDisplay {
    // --- [VALUES]
    public bool ModelUnits { get; }
    public DistanceDisplayMode Mode { get; }
    public int Precision { get; }

    // --- [FACTORY]
    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref bool modelUnits, ref DistanceDisplayMode mode, ref int precision) =>
        validationError = Enum.IsDefined(mode) && precision is >= 0 and <= 8 ? null : new InvalidRhinoValue();

    // --- [DOCUMENT]
    public static IO<DistanceDisplay> Read(RhinoDoc doc, bool modelUnits) =>
        IO.lift(Fin<DistanceDisplay> () => (modelUnits
            ? (doc.ModelDistanceDisplayMode, doc.ModelDistanceDisplayPrecision)
            : (doc.PageDistanceDisplayMode, doc.PageDistanceDisplayPrecision)) switch {
                var (mode, precision) => Validate(modelUnits, mode, precision, out DistanceDisplay? value) is { } error ? error : value!,
            });

    public IO<Unit> Write(RhinoDoc doc, CallbackSite site) {
        IO<Unit> Assign(DistanceDisplay value) => IO.lift(() => {
            if (value.ModelUnits)
                (doc.ModelDistanceDisplayMode, doc.ModelDistanceDisplayPrecision) = (value.Mode, value.Precision);
            else
                (doc.PageDistanceDisplayMode, doc.PageDistanceDisplayPrecision) = (value.Mode, value.Precision);
        });
        return
            from prior in Read(doc, ModelUnits)
            from registered in TableOps.Register(doc, new CustomUndo(nameof(DistanceDisplay), Assign(prior), Assign(this), site))
            from written in Assign(this)
            select written;
    }

    // --- [NOTATION]
    public Length Resolution(LengthUnit spaceUnit) =>
        Mode switch {
            DistanceDisplayMode.Decimal => Quantities.From(double.Pow(10, -Precision), spaceUnit),
            DistanceDisplayMode.Fractional or DistanceDisplayMode.FeetInches => Length.FromInches(QuantityValue.FromTerms(BigInteger.One, BigInteger.One << Precision)),
        };

    public string Format(Length length, LengthUnit spaceUnit, bool appendUnitSystemName) =>
        Localization.FormatNumber(Quantities.As(length, spaceUnit), spaceUnit, Mode, Precision, appendUnitSystemName);
}

[ValueObject<LengthUnit>(SkipIComparable = true)]
[ValidationError<InvalidRhinoValue>]
public sealed partial class DocumentUnit {
    // --- [FACTORY]
    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref LengthUnit value) =>
        validationError = LengthUnit.IsUnset(value) ? new InvalidRhinoValue() : null;

    // --- [DOCUMENT]
    public IO<Unit> Adjust(RhinoDoc doc, bool modelUnits, bool scale) =>
        IO.lift(() => Refused.Unless(doc.AdjustLengthUnits(modelUnits, _value, scale), nameof(RhinoDoc.AdjustLengthUnits)));
}

public sealed record ScalarDisplay(Option<string> Symbol, double Step, int Decimals, bool Turn, Func<double, double> Shown, Func<double, double> Key);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Quantities {
    // --- [LENGTHS]
    public static Length From(double value, LengthUnit spaceUnit) =>
        Measure(spaceUnit) * Exact(value);

    public static double As(Length length, LengthUnit spaceUnit) =>
        QuantityValue.Reduce(length / Measure(spaceUnit)).ToDouble();

    private static Length Measure(LengthUnit spaceUnit) =>
        LengthUnit.Scale(spaceUnit, LengthUnit.Meters) switch {
            var metres => toSeq(Length.Units).Find(named => Length.From(1, named).Meters == QuantityValue.FromDoubleRounded(metres))
                .Match(Some: static named => Length.From(1, named), None: () => Length.FromMeters(Exact(metres))),
        };

    private static QuantityValue Exact(double value) =>
        QuantityValue.FromDoubleRounded(value) switch {
            var rounded when QuantityValue.Reduce(rounded).ToDouble().Equals(value) => rounded,
            _ => int.Max(0, 52 - Math.ILogB(value)) switch {
                var shift => QuantityValue.FromTerms(new BigInteger(Math.ScaleB(value, shift)), BigInteger.One << shift),
            },
        };

    // --- [SCALARS]
    public static ScalarDisplay Scalar<TValue, TKey>(Presentation<TValue, TKey> presentation)
        where TValue : IMinMaxValue<TValue>, IConvertible<TKey>
        where TKey : struct, INumber<TKey> {
        Option<(UnitInfo From, UnitInfo To)> units = presentation.Unit.Map(static keyUnit => (keyUnit, Quantity.GetUnitInfo(keyUnit.Value switch {
            UnitsNet.Units.AngleUnit => UnitsNet.Units.AngleUnit.Degree,
            UnitsNet.Units.RatioUnit.DecimalFraction => UnitsNet.Units.RatioUnit.Percent,
            UnitsNet.Units.LengthUnit => UnitsNet.Units.LengthUnit.Inch,
            var held => held,
        })));
        Func<double, double> shown = Across(units);
        Func<double, double> key = Across(units.Map(static pair => (pair.To, pair.From)));
        double increment = presentation.Step.Map(static declared => double.CreateChecked(declared))
            .IfNone(() => key(double.Pow(10, double.Truncate(double.Log10(Span(presentation.Soft))) - 2)));
        double step = TKey.IsInteger(TKey.CreateSaturating(0.5)) ? double.Max(1, increment) : increment;
        int decimals = presentation.Decimals.IfNone(() => int.Max(0, (int)double.Ceiling(-double.Log10(shown(step)))));
        return new(
            units.Map(static pair => UnitAbbreviationsCache.Default.GetDefaultAbbreviation(pair.To, RowText.Culture)),
            step,
            decimals,
            presentation.Unit.Exists(static keyUnit => keyUnit.Value is UnitsNet.Units.AngleUnit.Radian)
                && double.Round(Span((TValue.MinValue.ToValue(), TValue.MaxValue.ToValue())), decimals, MidpointRounding.ToEven)
                    == double.Round(UnitConverter.Default.ConvertValue(1, UnitsNet.Units.AngleUnit.Revolution, UnitsNet.Units.AngleUnit.Degree), decimals, MidpointRounding.ToEven),
            shown,
            key);

        double Span((TKey Low, TKey High) ends) => double.Abs(shown(double.CreateChecked(ends.High)) - shown(double.CreateChecked(ends.Low)));

        static Func<double, double> Across(Option<(UnitInfo From, UnitInfo To)> units) =>
            units.Match(
                Some: static pair => (Func<double, double>)(value => UnitConverter.Default.ConvertValue(value, pair.From.UnitKey, pair.To.UnitKey)),
                None: static () => static value => value);
    }
}
