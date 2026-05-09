using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using Xunit;
using static LanguageExt.Prelude;

namespace Core.Tests.Domain;

// --- [EXAMPLES] --------------------------------------------------------------------------------

public sealed class GeometryContextSpec {
    [Theory]
    [InlineData(UnitSystem.Unset)]
    [InlineData(UnitSystem.CustomUnits)]
    public void RejectsUnsupported(UnitSystem units) =>
        Assert.True(
            condition: GeometryContext.CreateDefault(units: units)
                .ToFin()
                .IsFail);

    [Fact]
    public void RejectsMissingDocument() =>
        Assert.True(
            condition: GeometryContext.FromDocument(doc: null)
                .ToFin()
                .IsFail);

    [Fact]
    public void AccumulatesInvalidContextConstruction() {
        Validation<Error, GeometryContext> result = GeometryContext.Create(
            absoluteTolerance: double.NaN,
            relativeTolerance: -1.0,
            angleToleranceRadians: 0.0,
            modelUnits: GeometryContext.ModelUnitSystem.Create(units: UnitSystem.Unset));

        Assert.True(condition: result.ToFin().Match(
            Succ: static (GeometryContext _) => false,
            Fail: static (Error error) => error.Count == 4));
    }

    [Fact]
    public void PreservesAngleTolerance() {
        double angle = Math.PI / 90.0;

        Validation<Error, GeometryContext> result = GeometryContext.Create(
            absoluteTolerance: 0.01,
            relativeTolerance: 0.001,
            angleToleranceRadians: angle,
            modelUnits: CustomModelUnits());

        Assert.True(condition: result.ToFin().Match(
            Succ: (GeometryContext context) => context.Angle.Value == angle,
            Fail: static (Error _) => false));
    }

    [Fact]
    public void AccumulatesMissingGeometryPairsInContext() {
        GeometryContext context = ValidContext();

        Validation<Error, (LineCurve A, LineCurve B)> result = context.Validate(
            shape: new GeometryShape<LineCurve, LineCurve>.Pair(
                A: null!,
                B: null!,
                RequirementA: GeometryRequirement.CurveLength,
                RequirementB: GeometryRequirement.CurveLength));

        Assert.True(condition: result.ToFin().Match(
            Succ: static ((LineCurve A, LineCurve B) _) => false,
            Fail: static (Error error) => error.Count == 2));
    }

    [Fact]
    public void ValidatesOnlyFirstTupleGeometryWhenSecondIsPlainValue() {
        GeometryContext context = ValidContext();

        Validation<Error, (LineCurve A, int B)> result = context.Validate(
            shape: new GeometryShape<LineCurve, int>.FirstOnly(
                A: null!,
                B: 1,
                Requirement: GeometryRequirement.CurveLength));

        Assert.True(condition: result.ToFin().Match(
            Succ: static ((LineCurve A, int B) _) => false,
            Fail: static (Error error) => error.Count == 1));
    }

    [Fact]
    public void IncludesRequiredReadinessMasks() =>
        Assert.True(condition: (
            GeometryRequirement.VolumeMass.Has(other: GeometryRequirement.SolidTopology),
            GeometryRequirement.VolumeMass.Has(other: GeometryRequirement.MeshCheck),
            GeometryRequirement.AreaMass.Has(other: GeometryRequirement.Basic)
        ) == (true, true, true));

    [Fact]
    public void RejectsInvalidGeometryResults() {
        OperationKey key = new(name: "test");
        Line first = new(
            from: Point3d.Origin,
            to: new Point3d(x: 1.0, y: 0.0, z: 0.0));
        Line second = new(
            from: Point3d.Origin,
            to: new Point3d(x: 0.0, y: 1.0, z: 0.0));
        Line third = new(
            from: Point3d.Origin,
            to: new Point3d(x: 0.0, y: 0.0, z: 1.0));

        Assert.True(condition: new OperationOutcome<Point3d>.One(Value: Point3d.Unset).Reduce(key: key).IsFail);
        Assert.True(condition: new OperationOutcome<Sphere>.One(Value: Sphere.Unset).Reduce(key: key).IsFail);
        Assert.True(condition: new OperationOutcome<ComponentIndex>.One(Value: new ComponentIndex()).Reduce(key: key).IsFail);
        Assert.True(condition: new OperationOutcome<object>.One(Value: new object()).Reduce(key: key).IsFail);
        Assert.True(condition: new OperationOutcome<MeshCheckParameters>.One(Value: MeshCheckParameters.Defaults()).Reduce(key: key).IsSucc);
        Assert.True(condition: new OperationOutcome<Line>.Many(Values: Seq(first, second, third))
            .Reduce(key: key)
            .Match(
                Succ: (Seq<Line> lines) => lines.ToArray().SequenceEqual(second: [first, second, third]),
                Fail: static (Error _) => false));
        Assert.True(condition: new OperationOutcome<Line>.Many(Values: Seq(Line.Unset, Line.Unset))
            .Reduce(key: key)
            .Match(
                Succ: static (Seq<Line> _) => false,
                Fail: static (Error error) => error.Count == 2));
    }

    [Fact]
    public void AdaptsSolvedResultRails() {
        OperationKey key = new(name: "test");

        Assert.True(condition: OperationOutcome<Point3d>.Solved(isSolved: true, value: Point3d.Origin).Reduce(key: key).IsSucc);
        Assert.True(condition: OperationOutcome<Point3d>.Solved(isSolved: false, value: Point3d.Origin).Reduce(key: key).IsFail);
    }

    private static GeometryContext ValidContext() =>
        GeometryContext.Create(
                absoluteTolerance: 0.01,
                relativeTolerance: 0.001,
                angleToleranceRadians: Math.PI / 180.0,
                modelUnits: CustomModelUnits())
            .ToFin()
            .Match(
                Succ: static (GeometryContext context) => context,
                Fail: static (Error error) => throw new Xunit.Sdk.XunitException(error.Message));

    private static Fin<GeometryContext.ModelUnitSystem> CustomModelUnits() =>
        GeometryContext.Tolerance.Create(
                candidate: 1.0,
                label: "CustomUnitScale",
                accepts: static (double candidate) => candidate > RhinoMath.ZeroTolerance,
                requirement: "greater than Rhino zero tolerance")
            .Bind(static (GeometryContext.Tolerance customUnitScale) =>
                GeometryContext.ModelUnitSystem.FromModelUnits(
                    units: UnitSystem.CustomUnits,
                    metersPerUnit: customUnitScale));
}
