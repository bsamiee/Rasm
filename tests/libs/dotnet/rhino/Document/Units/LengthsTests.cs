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

    // --- [TOLERANCES]

    [Fact]
    public void TolerancesOutsideRhinoRangesFailAsOutside() =>
        Gen.OneOf(
                Gen.Double[-1d, 0d].Select(static absolute => (absolute, 0.01, 0.01)),
                Gen.OneOf(Gen.Double[-1d, 0d], Gen.Double[1d, 10d]).Select(static relative => (0.001, relative, 0.01)),
                Gen.OneOf(Gen.Double[-1d, 0d], Gen.Double[Math.PI + 1e-9, 10d]).Select(static angle => (0.001, 0.01, angle)))
            .Sample(static (absolute, relative, angle) =>
                Assert.Equal(RhinoReason.Outside, Expect<RhinoFault>.Failed(Tolerances.Validated(absolute, relative, angle)).Reason));

    [Theory]
    [InlineData(double.Epsilon, 0.01, 0.01)]
    [InlineData(1e300, 0.01, 0.01)]
    [InlineData(0.001, double.Epsilon, 0.01)]
    [InlineData(0.001, 0.99999999999999989, 0.01)]
    [InlineData(0.001, 0.01, double.Epsilon)]
    [InlineData(0.001, 0.01, Math.PI)]
    public void TolerancesAtTheirLimitsAreAccepted(double absolute, double relative, double angle) =>
        Assert.Equal((absolute, relative, angle), Tolerances.Validated(absolute, relative, angle).Map(static held => (held.Absolute, held.Relative, held.Angle)).ThrowIfFail());

    [Theory]
    [InlineData(0d, 0.01, 0.01)]
    [InlineData(double.PositiveInfinity, 0.01, 0.01)]
    [InlineData(double.NaN, 0.01, 0.01)]
    [InlineData(0.001, 0d, 0.01)]
    [InlineData(0.001, 1d, 0.01)]
    [InlineData(0.001, double.NaN, 0.01)]
    [InlineData(0.001, 0.01, 0d)]
    [InlineData(0.001, 0.01, 3.1415926535897936)]
    [InlineData(0.001, 0.01, double.PositiveInfinity)]
    [InlineData(0.001, 0.01, double.NaN)]
    public void TolerancesPastTheirLimitsFailAsOutside(double absolute, double relative, double angle) =>
        Assert.Equal(RhinoReason.Outside, Expect<RhinoFault>.Failed(Tolerances.Validated(absolute, relative, angle)).Reason);

    // --- [PRECISION]

    [Fact]
    public void DisplayPrecisionOutsideZeroToEightFailsAsOutside() =>
        Gen.OneOf(Gen.Int[-100, -1], Gen.Int[9, 100]).Sample(static precision =>
            Assert.Equal(RhinoReason.Outside, Expect<RhinoFault>.Failed(
                UnitsAndTolerances.Validated(LengthUnit.Millimeters, ModelGrid.Model, DistanceDisplayMode.Decimal, precision)).Reason));

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    public void DisplayPrecisionAtItsLimitsIsAccepted(int precision) =>
        Assert.Equal(precision, UnitsAndTolerances.Validated(LengthUnit.Millimeters, ModelGrid.Model, DistanceDisplayMode.Decimal, precision).ThrowIfFail().Precision);

    [Theory]
    [InlineData(-1)]
    [InlineData(9)]
    public void DisplayPrecisionPastItsLimitsFailsAsOutside(int precision) =>
        Assert.Equal(RhinoReason.Outside, Expect<RhinoFault>.Failed(
            UnitsAndTolerances.Validated(LengthUnit.Millimeters, ModelGrid.Model, DistanceDisplayMode.Decimal, precision)).Reason);

    // --- [QUANTITIES]

    [Fact]
    public void LengthConvertsBetweenUnitsByRhinoUnitScale() =>
        Units.Select(Units, Gen.Int[-1_000_000, 1_000_000]).Sample(static (origin, target, steps) =>
            Assert.Equal(
                steps * ModelGrid.Model.Absolute * RhinoMath.UnitScale(origin.System, target.System),
                Quantities.As(Quantities.From(steps * ModelGrid.Model.Absolute, origin.Unit), target.Unit),
                RhinoMath.SqrtEpsilon * Math.Max(1d, Math.Abs(steps * ModelGrid.Model.Absolute * RhinoMath.UnitScale(origin.System, target.System)))));

    // --- [GENERATORS]

    private static readonly Gen<(UnitSystem System, LengthUnit Unit)> Units =
        Gen.OneOf(UnitSystem.Millimeters, UnitSystem.Centimeters, UnitSystem.Meters, UnitSystem.Inches, UnitSystem.Feet).Select(static system => (system, LengthUnit.FromKnownUnitSystem(system)));

    private static readonly Gen<Space> Spaces = Gen.OneOf(Space.Model, Space.Page);

    private static readonly Gen<UnitsAndTolerances> Settings =
        Units.Select(Gen.Int[1, 1000], Gen.Int[1, 9999], Gen.Int[1, 1800], Gen.Int[0, 8], static (units, absolute, relative, angle, precision) =>
            UnitsAndTolerances.Create(units.Unit, Tolerances.Create(absolute * 1e-4, relative * 1e-4, angle * RhinoMath.DefaultAngleTolerance / 10d), DistanceDisplayMode.Decimal, precision));
}
