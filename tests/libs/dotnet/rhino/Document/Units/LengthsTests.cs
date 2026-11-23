using CsCheck;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Units;
using Rhino;
using Rhino.UI;
using Xunit;

namespace Rasm.Rhino.Tests.Document.Units;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class LengthsTests {

    // --- [DOCUMENT]

    [Fact]
    public Task CommittedUnitsAndTolerancesReadBackAsWritten() =>
        Settings.Select(Spaces).SampleAsync(static async (written, space) =>
            Assert.Equal(written, await RhinoHost.RunAsync(RhinoHost.Headless(doc =>
                Commits.Commit(doc, nameof(UnitsAndTolerances), new RedrawPolicy.Silent(), written.Write(doc, space)) >> IO.lift(() => space.Read(doc))))));

    [Fact]
    public Task UndoOfAUnitsWriteRestoresThePriorSettings() =>
        Settings.Select(Spaces).SampleAsync(static async (written, space) => {
            (UnitsAndTolerances prior, UnitsAndTolerances restored, (bool Valid, string Log) audit) = await RhinoHost.RunAsync(RhinoHost.Headless(doc =>
                from before in IO.lift(() => space.Read(doc))
                from committed in Commits.Commit(doc, nameof(UnitsAndTolerances), new RedrawPolicy.Silent(), written.Write(doc, space))
                from undone in Products.Accepted(doc.Undo)
                select (before, space.Read(doc), RhinoHost.Audit(doc))));
            Assert.Equal(prior, restored);
            Assert.True(audit.Valid, audit.Log);
        });

    [Fact]
    public Task AdjustingUnitsWithoutScalingLeavesAnExistingPointInPlace() =>
        Units.Select(Units, ModelGrid.Point).SampleAsync(static async (origin, target, point) => {
            (LengthUnit units, Point3d placed) = await RhinoHost.RunAsync(RhinoHost.Headless(doc =>
                from start in UnitsAndTolerances.Adjust(doc, Space.Model, origin.Unit, scale: false)
                let id = doc.Objects.AddPoint(point)
                from adjusted in UnitsAndTolerances.Adjust(doc, Space.Model, target.Unit, scale: false)
                select (doc.ModelUnits, ((Point)doc.Objects.FindId(id).Geometry).Location)));
            Assert.Equal(target.Unit, units);
            Assert.Equal(point, placed);
        });

    // --- [LIMITS]

    [Fact]
    public void TolerancesAreAcceptedExactlyInsideRhinoRangesAndOtherwiseFailAsOutside() =>
        Gen.OneOf(Absolute.Select(Inside.Relative, Inside.Angle), Inside.Absolute.Select(Relative, Inside.Angle), Inside.Absolute.Select(Inside.Relative, Angle)).Sample(static (absolute, relative, angle) => {
            Fin<Tolerances> validated = Tolerances.Validated(absolute, relative, angle);
            Assert.Equal(
                double.IsFinite(absolute) && absolute > 0d && relative > 0d && relative < 1d && angle > 0d && angle <= Math.PI
                    ? Right<RhinoReason, (double, double, double)>((absolute, relative, angle))
                    : Left<RhinoReason, (double, double, double)>(RhinoReason.Outside),
                validated.Match(
                    Succ: static held => Right<RhinoReason, (double, double, double)>((held.Absolute, held.Relative, held.Angle)),
                    Fail: _ => Left<RhinoReason, (double, double, double)>(Expect<RhinoFault>.Failed(validated).Reason)));
        }, iter: 10_000);

    [Fact]
    public void DisplayIsAcceptedForADefinedModeAndAPrecisionFromZeroToEightAndOtherwiseFailsAsOutside() =>
        Gen.OneOf(Gen.Enum<DistanceDisplayMode>(), Gen.Const((DistanceDisplayMode)99)).Select(Gen.OneOf(Gen.Int[-100, 100], Gen.OneOf(-1, 0, 8, 9))).Sample(static (display, precision) => {
            Fin<UnitsAndTolerances> validated = UnitsAndTolerances.Validated(LengthUnit.Millimeters, ModelGrid.Model, display, precision);
            Assert.Equal(
                Enum.IsDefined(display) && precision is >= 0 and <= 8
                    ? Right<RhinoReason, (DistanceDisplayMode, int)>((display, precision))
                    : Left<RhinoReason, (DistanceDisplayMode, int)>(RhinoReason.Outside),
                validated.Match(
                    Succ: static held => Right<RhinoReason, (DistanceDisplayMode, int)>((held.Display, held.Precision)),
                    Fail: _ => Left<RhinoReason, (DistanceDisplayMode, int)>(Expect<RhinoFault>.Failed(validated).Reason)));
        }, iter: 10_000);

    // --- [QUANTITIES]

    [Fact]
    public void LengthConvertsBetweenUnitsByRhinoUnitScale() =>
        Units.Select(Units, Gen.Int[-1_000_000, 1_000_000]).Sample(static (origin, target, steps) => {
            double value = steps * ModelGrid.Model.Absolute;
            double converted = value * RhinoMath.UnitScale(origin.System, target.System);
            Assert.Equal(converted, Quantities.As(Quantities.From(value, origin.Unit), target.Unit), RhinoMath.SqrtEpsilon * Math.Max(1d, Math.Abs(converted)));
        });

    // --- [GENERATORS]

    private static readonly Gen<(UnitSystem System, LengthUnit Unit)> Units =
        Gen.OneOf(UnitSystem.Millimeters, UnitSystem.Centimeters, UnitSystem.Meters, UnitSystem.Inches, UnitSystem.Feet).Select(static system => (system, LengthUnit.FromKnownUnitSystem(system)));

    private static readonly Gen<Space> Spaces = Gen.OneOf(Space.Model, Space.Page);

    private static readonly Gen<UnitsAndTolerances> Settings =
        Units.Select(Gen.Int[1, 1000], Gen.Int[1, 9999], Gen.Int[1, 1800], Gen.OneOf(DistanceDisplayMode.Decimal, DistanceDisplayMode.Fractional), Gen.Int[0, 8], static (units, absolute, relative, angle, display, precision) =>
            UnitsAndTolerances.Create(units.Unit, Tolerances.Create(absolute * 1e-4, relative * 1e-4, angle * RhinoMath.DefaultAngleTolerance / 10d), display, precision));

    private static readonly Gen<double> Hostile = Gen.OneOf(0d, -0d, double.Epsilon, -double.Epsilon, double.NaN, double.PositiveInfinity, double.NegativeInfinity, double.MaxValue);

    private static readonly Gen<double> Absolute = Gen.OneOf(Gen.Double[-1d, 1d], Gen.Double, Hostile);

    private static readonly Gen<double> Relative = Gen.OneOf(Gen.Double[-0.5d, 1.5d], Hostile, Gen.OneOf(1d, Math.BitDecrement(1d), Math.BitIncrement(1d)));

    private static readonly Gen<double> Angle = Gen.OneOf(Gen.Double[-1d, 4d], Hostile, Gen.OneOf(Math.PI, Math.BitDecrement(Math.PI), Math.BitIncrement(Math.PI)));

    private static readonly (Gen<double> Absolute, Gen<double> Relative, Gen<double> Angle) Inside = (Gen.Double[1e-6, 10d], Gen.Double[1e-3, 0.999], Gen.Double[1e-3, 3.14]);
}
