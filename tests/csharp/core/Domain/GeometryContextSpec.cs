using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using Xunit;

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

        Validation<Error, (LineCurve A, LineCurve B)> result = context.ValidatePair<LineCurve, LineCurve>(
            geometry: (A: null!, B: null!),
            a: GeometryRequirement.CurveLength,
            b: GeometryRequirement.CurveLength);

        Assert.True(condition: result.ToFin().Match(
            Succ: static ((LineCurve A, LineCurve B) _) => false,
            Fail: static (Error error) => error.Count == 2));
    }

    [Fact]
    public void ValidatesOnlyFirstTupleGeometryWhenSecondIsPlainValue() {
        GeometryContext context = ValidContext();

        Validation<Error, (LineCurve A, int B)> result = context.ValidateFirst<LineCurve, int>(
            geometry: (A: null!, B: 1),
            requirement: GeometryRequirement.CurveLength);

        Assert.True(condition: result.ToFin().Match(
            Succ: static ((LineCurve A, int B) _) => false,
            Fail: static (Error error) => error.Count == 1));
    }

    [Fact]
    public void IncludesRequiredReadinessMasks() =>
        Assert.True(condition: (
            GeometryRequirement.VolumeMass.Includes(requirement: GeometryRequirement.SolidTopology),
            GeometryRequirement.VolumeMass.Includes(requirement: GeometryRequirement.MeshCheck),
            GeometryRequirement.AreaMass.Includes(requirement: GeometryRequirement.Basic)
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

        Assert.True(condition: key.One(value: Point3d.Unset).IsFail);
        Assert.True(condition: key.One(value: Sphere.Unset).IsFail);
        Assert.True(condition: key.One(value: new ComponentIndex()).IsFail);
        Assert.True(condition: key.One(value: new object()).IsFail);
        Assert.True(condition: key.One(value: MeshCheckParameters.Defaults()).IsSucc);
        Assert.True(condition: key.Many(values: new[] {
                first,
                second,
                third,
            })
            .Match(
                Succ: (Seq<Line> lines) => lines.ToArray().SequenceEqual(second: [first, second, third]),
                Fail: static (Error _) => false));
        Assert.True(condition: key.Many(values: new[] {
                Line.Unset,
                Line.Unset,
            })
            .Match(
                Succ: static (Seq<Line> _) => false,
                Fail: static (Error error) => error.Count == 2));
    }

    [Fact]
    public void AdaptsSolvedResultRails() {
        OperationKey key = new(name: "test");

        Assert.True(condition: key.Solved(solved: true, value: Point3d.Origin).IsSucc);
        Assert.True(condition: key.Solved(solved: false, value: Point3d.Origin).IsFail);
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
