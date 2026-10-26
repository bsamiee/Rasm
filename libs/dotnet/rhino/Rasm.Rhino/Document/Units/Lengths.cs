using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Geometry.Intersect;
using Rhino.UI;
using UnitsNet;

namespace Rasm.Rhino.Document.Units;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Tolerances(double Absolute, double Relative, double Angle, double MeshCoefficient) {
    // --- [VALUES]
    public double MeshIntersection => Absolute * MeshCoefficient;

    // --- [DOCUMENT]
    public static IO<Tolerances> Read(RhinoDoc doc, bool modelUnits) =>
        IO.lift(() => modelUnits
            ? new Tolerances(doc.ModelAbsoluteTolerance, doc.ModelRelativeTolerance, doc.ModelAngleToleranceRadians, Intersection.MeshIntersectionsTolerancesCoefficient)
            : new Tolerances(doc.PageAbsoluteTolerance, doc.PageRelativeTolerance, doc.PageAngleToleranceRadians, Intersection.MeshIntersectionsTolerancesCoefficient));

    public IO<Unit> Write(RhinoDoc doc, bool modelUnits, CallbackSite site) =>
        TableOps.Undoable(doc, nameof(Tolerances), Read(doc, modelUnits), value => IO.lift(() => {
            if (modelUnits)
                (doc.ModelAbsoluteTolerance, doc.ModelRelativeTolerance, doc.ModelAngleToleranceRadians) = (value.Absolute, value.Relative, value.Angle);
            else
                (doc.PageAbsoluteTolerance, doc.PageRelativeTolerance, doc.PageAngleToleranceRadians) = (value.Absolute, value.Relative, value.Angle);
        }), this, site);

    // --- [COMPARISON]
    public bool Matches(Plane left, Plane right) =>
        left.Origin.DistanceTo(right.Origin) <= Absolute
        && Vector3d.VectorAngle(left.XAxis, right.XAxis) <= Angle
        && Vector3d.VectorAngle(left.YAxis, right.YAxis) <= Angle;
}

public sealed record DistanceDisplay(bool ModelUnits, DistanceDisplayMode Mode, int Precision) {
    // --- [DOCUMENT]
    public static IO<DistanceDisplay> Read(RhinoDoc doc, bool modelUnits) =>
        IO.lift(() => modelUnits
            ? new DistanceDisplay(ModelUnits: true, doc.ModelDistanceDisplayMode, doc.ModelDistanceDisplayPrecision)
            : new DistanceDisplay(ModelUnits: false, doc.PageDistanceDisplayMode, doc.PageDistanceDisplayPrecision));

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
            DistanceDisplayMode.Fractional or DistanceDisplayMode.FeetInches => Length.FromInches(QuantityValue.FromTerms(1, 1L << Precision)),
        };

    public string Format(Length length, LengthUnit spaceUnit, bool appendUnitSystemName) =>
        Localization.FormatNumber(Quantities.As(length, spaceUnit), spaceUnit, Mode, Precision, appendUnitSystemName);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Quantities {
    public static Length From(double value, LengthUnit spaceUnit) =>
        Length.FromMeters((QuantityValue)value * LengthUnit.Scale(spaceUnit, LengthUnit.Meters));

    public static double As(Length length, LengthUnit spaceUnit) =>
        (length.Meters * LengthUnit.Scale(LengthUnit.Meters, spaceUnit)).ToDouble();
}
