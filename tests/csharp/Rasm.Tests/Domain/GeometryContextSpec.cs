using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rhino;
using Rhino.Geometry;
using Xunit;
using static LanguageExt.Prelude;

namespace Rasm.Tests.Domain;

// --- [EXAMPLES] --------------------------------------------------------------------------------

public sealed class ContextSpec {
    [Theory]
    [InlineData(UnitSystem.Unset)]
    [InlineData(UnitSystem.CustomUnits)]
    public void RejectsUnsupported(UnitSystem units) =>
        Assert.True(
            condition: Context.CreateDefault(units: units)
                .ToFin()
                .IsFail);

    [Fact]
    public void RejectsMissingDocument() =>
        Assert.True(
            condition: Context.FromDocument(doc: null)
                .ToFin()
                .IsFail);

    [Fact]
    public void AccumulatesInvalidContextConstruction() {
        Validation<Error, Context> result = Context.Create(
            absoluteTolerance: double.NaN,
            relativeTolerance: -1.0,
            angleToleranceRadians: 0.0,
            modelUnits: Context.ModelUnitSystem.Create(units: UnitSystem.Unset));

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 4));
    }

    [Fact]
    public void PreservesAngleTolerance() {
        double angle = Math.PI / 90.0;

        Validation<Error, Context> result = Context.Create(
            absoluteTolerance: 0.01,
            relativeTolerance: 0.001,
            angleToleranceRadians: angle,
            modelUnits: CustomModelUnits());

        Assert.True(condition: result.ToFin().Match(
            Succ: context => context.Angle.Value == angle,
            Fail: static _ => false));
    }

    [Fact]
    public void AccumulatesMissingGeometryPairsInContext() {
        Context context = ValidContext();

        Validation<Error, (LineCurve A, LineCurve B)> result = context.Validate(
            shape: new Pair<LineCurve, LineCurve>.Both(
                A: null!,
                B: null!,
                RequirementA: Requirement.CurveLength,
                RequirementB: Requirement.CurveLength));

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 2));
    }

    [Fact]
    public void ValidatesOnlyFirstTupleGeometryWhenSecondIsPlainValue() {
        Context context = ValidContext();

        Validation<Error, (LineCurve A, int B)> result = context.Validate(
            shape: new Pair<LineCurve, int>.FirstOnly(
                A: null!,
                B: 1,
                Requirement: Requirement.CurveLength));

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1));
    }

    [Fact]
    public void IncludesRequiredReadinessMasks() =>
        Assert.True(condition: (
            Requirement.VolumeMass.Has(other: Requirement.SolidTopology),
            Requirement.VolumeMass.Has(other: Requirement.MeshCheck),
            Requirement.AreaMass.Has(other: Requirement.Basic)
        ) == (true, true, true));

    [Fact]
    public void RejectsInvalidGeometryResults() {
        Op key = new(name: "test");
        Line first = new(
            from: Point3d.Origin,
            to: new Point3d(x: 1.0, y: 0.0, z: 0.0));
        Line second = new(
            from: Point3d.Origin,
            to: new Point3d(x: 0.0, y: 1.0, z: 0.0));
        Line third = new(
            from: Point3d.Origin,
            to: new Point3d(x: 0.0, y: 0.0, z: 1.0));

        Assert.True(condition: new OpResult<Point3d>.One(Value: Point3d.Unset).Reduce(key: key).IsFail);
        Assert.True(condition: new OpResult<Sphere>.One(Value: Sphere.Unset).Reduce(key: key).IsFail);
        Assert.True(condition: new OpResult<ComponentIndex>.One(Value: new ComponentIndex()).Reduce(key: key).IsFail);
        Assert.True(condition: new OpResult<object>.One(Value: new object()).Reduce(key: key).IsFail);
        Assert.True(condition: new OpResult<MeshCheckParameters>.One(Value: MeshCheckParameters.Defaults()).Reduce(key: key).IsSucc);
        Assert.True(condition: new OpResult<Line>.Many(Values: Seq(first, second, third))
            .Reduce(key: key)
            .Match(
                Succ: lines => lines.ToArray().SequenceEqual(second: [first, second, third]),
                Fail: static _ => false));
        Assert.True(condition: new OpResult<Line>.Many(Values: Seq(Line.Unset, Line.Unset))
            .Reduce(key: key)
            .Match(
                Succ: static _ => false,
                Fail: static error => error.Count == 2));
    }

    [Fact]
    public void AdaptsSolvedResultRails() {
        Op key = new(name: "test");

        Assert.True(condition: OpResult<Point3d>.Solved(isSolved: true, value: Point3d.Origin).Reduce(key: key).IsSucc);
        Assert.True(condition: OpResult<Point3d>.Solved(isSolved: false, value: Point3d.Origin).Reduce(key: key).IsFail);
    }

    private static Context ValidContext() =>
        Context.Create(
                absoluteTolerance: 0.01,
                relativeTolerance: 0.001,
                angleToleranceRadians: Math.PI / 180.0,
                modelUnits: CustomModelUnits())
            .ToFin()
            .Match(
                Succ: static context => context,
                Fail: static error => throw new Xunit.Sdk.XunitException(error.Message));

    private static Fin<Context.ModelUnitSystem> CustomModelUnits() =>
        Context.Tolerance.Create(
                candidate: 1.0,
                label: "CustomUnitScale",
                accepts: static candidate => candidate > RhinoMath.ZeroTolerance,
                requirement: "greater than Rhino zero tolerance")
            .Bind(static customUnitScale => Context.ModelUnitSystem.FromModelUnits(
                    units: UnitSystem.CustomUnits,
                    metersPerUnit: customUnitScale));
}
