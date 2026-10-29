using Rhino;
using Rhino.Geometry.Intersect;
using Rhino.UI;
using UnitsNet;

namespace Rasm.Rhino.Document.Units;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Tolerances(double Absolute, double Relative, double Angle) {
    public double MeshIntersection => Absolute * Intersection.MeshIntersectionsTolerancesCoefficient;

    public static IO<Tolerances> Read(RhinoDoc doc, Space space) => IO.lift(() => space.Read(doc).Tolerances);

    public bool Matches(Plane left, Plane right) =>
        left.Origin.DistanceTo(right.Origin) <= Absolute
        && Vector3d.VectorAngle(left.XAxis, right.XAxis) <= Angle
        && Vector3d.VectorAngle(left.YAxis, right.YAxis) <= Angle;
}

public sealed record UnitsAndTolerances(LengthUnit Unit, Tolerances Tolerances, DistanceDisplayMode Display, int Precision) {
    public Length Resolution => Quantities.Resolution(Display, Precision, Unit);

    public IO<Unit> Write(RhinoDoc doc, Space space) =>
        Commits.Undoable(doc, $"{space.Name} {nameof(UnitsAndTolerances)}", IO.lift(() => space.Read(doc)), value => Assigned(doc, space, value), this);

    public static IO<Unit> Adjust(RhinoDoc doc, Space space, LengthUnit units, bool scale) =>
        from adjusted in Products.Accepted(() => doc.AdjustLengthUnits(space == Space.Model, units, scale))
        from landed in IO.lift(() => RhinoFault.Unless(space.Read(doc).Unit == units, RhinoReason.Declined, units))
        select landed;

    public string Format(Length length, bool appendUnitSystemName) =>
        Localization.FormatNumber(Quantities.As(length, Unit), Unit, Display, Precision, appendUnitSystemName);

    private static IO<Unit> Assigned(RhinoDoc doc, Space space, UnitsAndTolerances value) =>
        from adjusted in IO.lift(() => space.Read(doc).Unit == value.Unit).Bind(same => unless(same, Adjust(doc, space, value.Unit, scale: false)).As())
        from written in IO.lift(() => space.Write(doc, value))
        from landed in IO.lift(() => RhinoFault.Unless(space.Read(doc) == value, RhinoReason.Declined, value))
        select landed;
}

[SmartEnum]
public sealed partial class Space {
    public static readonly Space Model = new(
        nameof(Model),
        static doc => new UnitsAndTolerances(
            doc.ModelUnits,
            new Tolerances(doc.ModelAbsoluteTolerance, doc.ModelRelativeTolerance, doc.ModelAngleToleranceRadians),
            doc.ModelDistanceDisplayMode,
            doc.ModelDistanceDisplayPrecision),
        static (doc, value) => (doc.ModelAbsoluteTolerance, doc.ModelRelativeTolerance, doc.ModelAngleToleranceRadians, doc.ModelDistanceDisplayMode, doc.ModelDistanceDisplayPrecision) =
            (value.Tolerances.Absolute, value.Tolerances.Relative, value.Tolerances.Angle, value.Display, value.Precision));

    public static readonly Space Page = new(
        nameof(Page),
        static doc => new UnitsAndTolerances(
            doc.PageUnits,
            new Tolerances(doc.PageAbsoluteTolerance, doc.PageRelativeTolerance, doc.PageAngleToleranceRadians),
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

    private static Length Scaled(QuantityValue value, LengthUnit spaceUnit) =>
        Length.FromMeters(value * LengthUnit.Scale(spaceUnit, LengthUnit.Meters));
}
