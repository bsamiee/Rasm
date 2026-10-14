using System.Numerics;
using PeterO.Numbers;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Geometry.Intersect;
using Rhino.UI;
using UnitsNet;

namespace Rasm.Rhino.Document.Units;

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
        validationError = Enum.IsDefined(mode) && precision >= 0 && precision <= (modelUnits ? 8 : 20) ? null : new InvalidRhinoValue();

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

public sealed record ScalarDisplay(Option<string> Symbol, double Step, int Decimals, bool Turn, Func<double, double> Shown, Func<double, double> Key);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Quantities {
    // --- [LENGTHS]
    public static Length From(double value, LengthUnit spaceUnit) =>
        Length.FromMeters((QuantityValue)value * LengthUnit.Scale(spaceUnit, LengthUnit.Meters));

    public static double As(Length length, LengthUnit spaceUnit) =>
        (length.Meters * LengthUnit.Scale(LengthUnit.Meters, spaceUnit)).ToDouble();

    // --- [SCALARS]
    public static ScalarDisplay Scalar<TKey>(Presentation<TKey> presentation, (TKey Low, TKey High) bounds)
        where TKey : struct, INumber<TKey> {
        Option<(UnitInfo From, UnitInfo To)> units = presentation.Unit.Map(keyUnit => (keyUnit, presentation.DisplayUnit.IfNone(() => Quantity.GetUnitInfo(keyUnit.Value switch {
            UnitsNet.Units.AngleUnit => UnitsNet.Units.AngleUnit.Degree,
            UnitsNet.Units.RatioUnit.DecimalFraction => UnitsNet.Units.RatioUnit.Percent,
            var held => held,
        }))));
        Func<double, double> shown = Across(units);
        Func<double, double> key = Across(units.Map(static pair => (pair.To, pair.From)));
        double increment = presentation.Step.Map(static declared => double.CreateChecked(declared))
            .IfNone(() => double.Max(double.Epsilon, key(double.Pow(10, double.Truncate(Span(presentation.Soft.IfNone(bounds)).Log10(EContext.Binary64.WithUnlimitedExponents()).ToDouble()) - 2))));
        double step = TKey.IsInteger(TKey.CreateSaturating(0.5)) ? double.Max(1, increment) : increment;
        int decimals = presentation.Decimals.IfNone(() => int.Max(0, (int)double.Ceiling(-double.Log10(shown(step)))));
        return new(
            units.Map(static pair => UnitAbbreviationsCache.Default.GetDefaultAbbreviation(pair.To, RowText.Culture)),
            step,
            decimals,
            presentation.Unit.Exists(static keyUnit => keyUnit.Value is UnitsNet.Units.AngleUnit.Radian)
                && double.Round(Span(bounds).ToDouble(), decimals, MidpointRounding.ToEven)
                    == double.Round(UnitConverter.Default.ConvertValue(1, UnitsNet.Units.AngleUnit.Revolution, UnitsNet.Units.AngleUnit.Degree), decimals, MidpointRounding.ToEven),
            shown,
            key);

        EFloat Span((TKey Low, TKey High) ends) => EFloat.FromDouble(shown(double.CreateChecked(ends.High))).Subtract(EFloat.FromDouble(shown(double.CreateChecked(ends.Low)))).Abs();

        static Func<double, double> Across(Option<(UnitInfo From, UnitInfo To)> units) =>
            units.Match(
                Some: static pair => (Func<double, double>)(value => UnitConverter.Default.ConvertValue(value, pair.From.UnitKey, pair.To.UnitKey)),
                None: static () => static value => value);
    }
}
