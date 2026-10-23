using System.Numerics;
using PeterO.Numbers;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Geometry.Intersect;
using Rhino.UI;
using UnitsNet;
using UnitSystem = Rhino.UnitSystem;

namespace Rasm.Rhino.Document.Units;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class Tolerances {
    // --- [VALUES]
    public double Absolute { get; }
    public double Relative { get; }
    public double Angle { get; }
    public double MeshCoefficient { get; }
    public double MeshIntersection => Absolute * MeshCoefficient;

    // --- [FACTORY]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double absolute, ref double relative, ref double angle, ref double meshCoefficient) =>
        validationError = absolute > 0 && relative is > 0 and < 1 && angle is > 0 and <= double.Pi
            ? null
            : new ValidationError("tolerances need absolute above 0, relative in (0, 1), angle in (0, pi]");

    // --- [DOCUMENT]
    public static IO<Tolerances> Read(RhinoDoc doc, bool modelUnits) =>
        IO.lift(() => {
            (double absolute, double relative, double angle) = modelUnits
                ? (doc.ModelAbsoluteTolerance, doc.ModelRelativeTolerance, doc.ModelAngleToleranceRadians)
                : (doc.PageAbsoluteTolerance, doc.PageRelativeTolerance, doc.PageAngleToleranceRadians);
            return ValidationResults.ToFin(Validate(absolute, relative, angle, Intersection.MeshIntersectionsTolerancesCoefficient, out Tolerances? value), value);
        });

    public IO<Unit> Write(RhinoDoc doc, bool modelUnits, CallbackSite site) =>
        TableOps.Undoable(doc, nameof(Tolerances), Read(doc, modelUnits), value => IO.lift(() => {
            if (modelUnits)
                (doc.ModelAbsoluteTolerance, doc.ModelRelativeTolerance, doc.ModelAngleToleranceRadians) = (value.Absolute, value.Relative, value.Angle);
            else
                (doc.PageAbsoluteTolerance, doc.PageRelativeTolerance, doc.PageAngleToleranceRadians) = (value.Absolute, value.Relative, value.Angle);
        }), this, site);
}

[ComplexValueObject]
public sealed partial class DistanceDisplay {
    // --- [VALUES]
    public bool ModelUnits { get; }
    public DistanceDisplayMode Mode { get; }
    public int Precision { get; }

    // --- [FACTORY]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref bool modelUnits, ref DistanceDisplayMode mode, ref int precision) =>
        validationError = Enum.IsDefined(mode) && precision >= 0 && precision <= (modelUnits ? 8 : 20)
            ? null
            : new ValidationError("distance display needs defined mode, precision in [0, 8] for model or [0, 20] for page units");

    // --- [DOCUMENT]
    public static IO<DistanceDisplay> Read(RhinoDoc doc, bool modelUnits) =>
        IO.lift(() => {
            (DistanceDisplayMode mode, int precision) = modelUnits
                ? (doc.ModelDistanceDisplayMode, doc.ModelDistanceDisplayPrecision)
                : (doc.PageDistanceDisplayMode, doc.PageDistanceDisplayPrecision);
            return ValidationResults.ToFin(Validate(modelUnits, mode, precision, out DistanceDisplay? value), value);
        });

    public IO<Unit> Write(RhinoDoc doc, CallbackSite site) =>
        TableOps.Undoable(doc, nameof(DistanceDisplay), Read(doc, ModelUnits), value => IO.lift(() => {
            if (value.ModelUnits)
                (doc.ModelDistanceDisplayMode, doc.ModelDistanceDisplayPrecision) = (value.Mode, value.Precision);
            else
                (doc.PageDistanceDisplayMode, doc.PageDistanceDisplayPrecision) = (value.Mode, value.Precision);
        }), this, site);

    // --- [NOTATION]
    public Length Resolution(LengthUnit spaceUnit) =>
        Mode switch {
            DistanceDisplayMode.Decimal => Quantities.From(double.Exp10(-Precision), spaceUnit),
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

    public static Option<UnitSystem> System(UnitsNet.Units.LengthUnit lengthUnit) =>
        toSeq(Enum.GetValues<UnitSystem>()).Filter(static system => system is not (UnitSystem.None or UnitSystem.CustomUnits or UnitSystem.Unset))
            .Find(system => RhinoMath.UnitScale(system, UnitSystem.Meters) == Length.From(1, lengthUnit).Meters.ToDouble());

    // --- [SCALARS]
    public static ScalarDisplay Scalar<TKey>(Presentation<TKey> presentation, (TKey Low, TKey High) bounds)
        where TKey : struct, INumber<TKey> {
        Option<(UnitInfo From, UnitInfo To)> units = presentation.Unit.Map(static keyUnit => (keyUnit, Quantity.GetUnitInfo(keyUnit.Value switch {
            UnitsNet.Units.AngleUnit => UnitsNet.Units.AngleUnit.Degree,
            UnitsNet.Units.RatioUnit.DecimalFraction => UnitsNet.Units.RatioUnit.Percent,
            var held => held,
        })));
        Func<double, double> shown = Across(units);
        Func<double, double> key = Across(units.Map(static pair => (pair.To, pair.From)));
        (TKey Low, TKey High) soft = presentation.Soft.IfNone(bounds);
        bool integral = TKey.IsInteger(TKey.CreateSaturating(0.5));
        double increment = presentation.Step.Map(static declared => double.CreateChecked(declared))
            .IfNone(() => double.Max(double.Epsilon, key(double.Exp10(double.Floor(Span(soft).Log10(EContext.Binary64.WithUnlimitedExponents()).ToDouble()) - 2))));
        double step = integral ? double.Max(1, increment) : increment;
        int decimals = presentation.Decimals.IfNone(() => int.Max(0, (int)double.Ceiling(-double.Log10(
            presentation.Scale != TrackScale.Linear && !integral && shown(double.CreateChecked(soft.Low)) is > 0d and var bottom ? bottom / 10d : shown(step)))));
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
                Some: static pair => (Func<double, double>)(value => UnitConverter.Convert(value, pair.From.UnitKey, pair.To.UnitKey)),
                None: static () => static value => value);
    }
}
