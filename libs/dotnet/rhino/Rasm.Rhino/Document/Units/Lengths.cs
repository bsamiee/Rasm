using System.Globalization;
using Rhino;
using Rhino.Geometry.Intersect;
using Rhino.UI;
using UnitsNet;

namespace Rasm.Rhino.Document.Units;

// --- [MODELS] --------------------------------------------------------------------------

[ComplexValueObject]
[ValidationError<RhinoFault>]
public sealed partial class Tolerances {
    public double Absolute { get; }
    public double Relative { get; }
    public double Angle { get; }

    public double MeshIntersection => Absolute * Intersection.MeshIntersectionsTolerancesCoefficient;

    public static Fin<Tolerances> Validated(double absolute, double relative, double angle) =>
        TryCreate(absolute, relative, angle, out Tolerances? tolerances, out RhinoFault? fault) ? tolerances : fault;

    public static IO<Tolerances> Read(RhinoDoc doc, Space space) => IO.lift(() => space.Read(doc).Tolerances);

    public bool Matches(Plane left, Plane right) =>
        left.Origin.DistanceTo(right.Origin) <= Absolute
        && Vector3d.VectorAngle(left.XAxis, right.XAxis) <= Angle
        && Vector3d.VectorAngle(left.YAxis, right.YAxis) <= Angle;

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref double absolute, ref double relative, ref double angle) =>
        validationError = double.IsFinite(absolute) && absolute > 0d && relative is > 0d and < 1d && angle is > 0d and <= Math.PI
            ? null
            : new RhinoFault((absolute, relative, angle), RhinoReason.Outside);
}

[ComplexValueObject(SkipToString = true)]
[ValidationError<RhinoFault>]
public sealed partial class UnitsAndTolerances {
    public LengthUnit LengthUnit { get; }
    public Tolerances Tolerances { get; }
    public DistanceDisplayMode Display { get; }
    public int Precision { get; }

    public Length Resolution => Quantities.Resolution(Display, Precision, LengthUnit);

    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{LengthUnit.Symbol} {Tolerances} {Display} {Precision}");

    public static Fin<UnitsAndTolerances> Validated(LengthUnit lengthUnit, Tolerances tolerances, DistanceDisplayMode display, int precision) =>
        TryCreate(lengthUnit, tolerances, display, precision, out UnitsAndTolerances? units, out RhinoFault? fault) ? units : fault;

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref LengthUnit lengthUnit, ref Tolerances tolerances, ref DistanceDisplayMode display, ref int precision) =>
        validationError = Enum.IsDefined(display) && precision is >= 0 and <= 8
            ? null
            : new RhinoFault((display, precision), RhinoReason.Outside);

    public IO<Unit> Write(RhinoDoc doc, Space space) =>
        Commits.Undoable(doc, $"{space.Name} {nameof(UnitsAndTolerances)}", IO.lift(() => space.Read(doc)), value => Assigned(doc, space, value), this);

    public static IO<Unit> Adjust(RhinoDoc doc, Space space, LengthUnit units, bool scale) =>
        Products.Accepted(() => doc.AdjustLengthUnits(space == Space.Model, units, scale));

    public string Format(Length length, bool appendUnitSystemName) =>
        Localization.FormatNumber(Quantities.As(length, LengthUnit), LengthUnit, Display, Precision, appendUnitSystemName);

    private static IO<Unit> Assigned(RhinoDoc doc, Space space, UnitsAndTolerances value) =>
        from adjusted in IO.lift(() => space.Read(doc).LengthUnit == value.LengthUnit).Bind(same => unless(same, Adjust(doc, space, value.LengthUnit, scale: false)).As())
        from written in IO.lift(() => space.Write(doc, value))
        select written;
}

[SmartEnum]
public sealed partial class Space {
    public static readonly Space Model = new(
        nameof(Model),
        static doc => UnitsAndTolerances.Create(
            doc.ModelUnits,
            Tolerances.Create(doc.ModelAbsoluteTolerance, doc.ModelRelativeTolerance, doc.ModelAngleToleranceRadians),
            doc.ModelDistanceDisplayMode,
            doc.ModelDistanceDisplayPrecision),
        static (doc, value) => (doc.ModelAbsoluteTolerance, doc.ModelRelativeTolerance, doc.ModelAngleToleranceRadians, doc.ModelDistanceDisplayMode, doc.ModelDistanceDisplayPrecision) =
            (value.Tolerances.Absolute, value.Tolerances.Relative, value.Tolerances.Angle, value.Display, value.Precision));

    public static readonly Space Page = new(
        nameof(Page),
        static doc => UnitsAndTolerances.Create(
            doc.PageUnits,
            Tolerances.Create(doc.PageAbsoluteTolerance, doc.PageRelativeTolerance, doc.PageAngleToleranceRadians),
            doc.PageDistanceDisplayMode,
            doc.PageDistanceDisplayPrecision),
        static (doc, value) => (doc.PageAbsoluteTolerance, doc.PageRelativeTolerance, doc.PageAngleToleranceRadians, doc.PageDistanceDisplayMode, doc.PageDistanceDisplayPrecision) =
            (value.Tolerances.Absolute, value.Tolerances.Relative, value.Tolerances.Angle, value.Display, value.Precision));

    public string Name { get; }

    [UseDelegateFromConstructor]
    public partial UnitsAndTolerances Read(RhinoDoc doc);

    [UseDelegateFromConstructor]
    public partial void Write(RhinoDoc doc, UnitsAndTolerances value);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Quantities {
    private const byte RoundTripDigits = 17;

    public static QuantityValue Exact(double value) => QuantityValue.FromDoubleRounded(value, RoundTripDigits);

    public static Length From(double value, LengthUnit spaceUnit) => Scaled(Exact(value), spaceUnit);

    public static double As(Length length, LengthUnit spaceUnit) =>
        (length.Meters * LengthUnit.Scale(LengthUnit.Meters, spaceUnit)).ToDouble();

    public static Length Resolution(DistanceDisplayMode display, int precision, LengthUnit spaceUnit) =>
        display is DistanceDisplayMode.Decimal
            ? Scaled(QuantityValue.FromPowerOfTen(1, -precision), spaceUnit)
            : Length.FromInches(QuantityValue.FromTerms(1, 1L << precision));

    public static int Precision(bool fractional, Length resolution, LengthUnit spaceUnit) =>
        LanguageExt.List.unfold(0, digits =>
            Resolution(fractional ? DistanceDisplayMode.Fractional : DistanceDisplayMode.Decimal, digits, spaceUnit) > resolution ? Some((digits, digits + 1)) : None).Count();

    private static Length Scaled(QuantityValue value, LengthUnit spaceUnit) =>
        Length.FromMeters(value * LengthUnit.Scale(spaceUnit, LengthUnit.Meters));
}
