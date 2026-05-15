using System.Threading;
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
            absolute: double.NaN,
            relative: -1.0,
            angle: 0.0,
            units: UnitSystem.Unset);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 4));
    }

    [Fact]
    public void PreservesAngleTolerance() {
        double angle = Math.PI / 90.0;

        Validation<Error, Context> result = Context.Create(
            absolute: 0.01,
            relative: 0.001,
            angle: angle,
            units: UnitSystem.Millimeters);

        Assert.True(condition: result.ToFin().Match(
            Succ: context => context.Angle.Value == angle,
            Fail: static _ => false));
    }

    [Fact]
    public void PreservesExplicitCustomUnitSystem() {
        Validation<Error, Context> result = Context.Create(
            absolute: 0.01,
            relative: 0.001,
            angle: Math.PI / 180.0,
            units: UnitSystem.CustomUnits);

        Assert.True(condition: result.ToFin().Match(
            Succ: static context => context.Units == UnitSystem.CustomUnits,
            Fail: static _ => false));
    }

    [Fact]
    public void AccumulatesMissingGeometryPairsInContext() {
        Context context = ValidContext();

        Validation<Error, (LineCurve A, LineCurve B)> result = context.Pair<LineCurve, LineCurve>(a: null!,
            b: null!,
            requirementA: Requirement.CurveLength,
            requirementB: Requirement.CurveLength,
            cancel: TestContext.Current.CancellationToken);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 2));
    }

    [Fact]
    public void ValidatesOnlyFirstTupleGeometryWhenSecondIsPlainValue() {
        Context context = ValidContext();

        Validation<Error, (LineCurve A, int B)> result = context.Pair<LineCurve, int>(a: null!,
            b: 1,
            requirementA: Requirement.CurveLength,
            requirementB: Requirement.None,
            cancel: TestContext.Current.CancellationToken);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1));
    }

    [Fact]
    public void RejectsInvalidPlainPairOperands() {
        Context context = ValidContext();

        Validation<Error, (Point3d A, Line B)> result = context.Pair(a: Point3d.Unset,
            b: Line.Unset,
            requirementA: Requirement.None,
            requirementB: Requirement.None,
            cancel: TestContext.Current.CancellationToken);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 2));
    }

    [Fact]
    public void KeepsContextPureRuntimeStateOutOfDomain() =>
        Assert.DoesNotContain(collection: typeof(Context).GetProperties(), filter: static property =>
            property.PropertyType == typeof(CancellationToken)
            || property.PropertyType == typeof(IProgress<double>));

    [Fact]
    public void RejectsInvalidGeometryResults() {
        Op key = Op.Create(value: "test");
        Line first = new(
            from: Point3d.Origin,
            to: new Point3d(x: 1.0, y: 0.0, z: 0.0));
        Line second = new(
            from: Point3d.Origin,
            to: new Point3d(x: 0.0, y: 1.0, z: 0.0));
        Line third = new(
            from: Point3d.Origin,
            to: new Point3d(x: 0.0, y: 0.0, z: 1.0));

        Assert.True(condition: key.Accept(value: Point3d.Unset).IsFail);
        Assert.True(condition: key.Accept(value: Sphere.Unset).IsFail);
        Assert.True(condition: key.Accept(value: new ComponentIndex()).IsFail);
        Assert.True(condition: key.Accept(value: new object()).IsFail);
        Assert.True(condition: key.Accept(value: MeshCheckParameters.Defaults()).IsSucc);
        Assert.True(condition: key.Accept(values: Seq(first, second, third))
            .Match(
                Succ: lines => lines.ToArray().SequenceEqual(second: [first, second, third]),
                Fail: static _ => false));
        Assert.True(condition: key.Accept(values: Seq(Line.Unset, Line.Unset))
            .Match(
                Succ: static _ => false,
                Fail: static error => error.Count == 2));
        Assert.True(condition: key.Accept<Point3d>(values: null!).IsFail);
    }

    [Fact]
    public void AdaptsSolvedResultRails() {
        Op key = Op.Create(value: "test");

        Assert.True(condition: key.AcceptSolved(isSolved: true, value: Point3d.Origin).IsSucc);
        Assert.True(condition: key.AcceptSolved(isSolved: false, value: Point3d.Origin).IsFail);
    }

    [Fact]
    public void RegistersUniqueKindDescriptorsWithLineageFallback() {
        Assert.Equal(expected: Kind.Items.Count, actual: Kind.Items.Select(static kind => kind.Type).Distinct().Count());
        Assert.True(condition: KindLookup.Resolve(type: typeof(LineCurve)).Match(Some: static kind => kind == Kind.Curve, None: static () => false));
    }

    [Fact]
    public void FractionsRejectInvalidAndSpreadCounts() {
        Op key = Op.Create(value: "fractions");

        double[] single = GeometryKernel.Fractions(count: 1, op: key).Match(
            Succ: static values => values.ToArray(),
            Fail: static error => throw new Xunit.Sdk.XunitException(error.Message));
        double[] multiple = GeometryKernel.Fractions(count: 3, op: key).Match(
            Succ: static values => values.ToArray(),
            Fail: static error => throw new Xunit.Sdk.XunitException(error.Message));

        Assert.True(condition: GeometryKernel.Fractions(count: 0, op: key).IsFail);
        Assert.Equal(expected: [0.5], actual: single);
        Assert.Equal(expected: [0.0, 0.5, 1.0], actual: multiple);
    }

    [Fact]
    public void RejectsUnsupportedCurveProjectionFamiliesWithoutWeakeningExactVariants() {
        Assert.True(condition: Rasm.Analysis.Analyze.Curves<Curve, Curve>(aspect: Rasm.Analysis.Curves.All).IsSupported);
        Assert.True(condition: Rasm.Analysis.Analyze.Curves<BrepFace, Curve>(aspect: Rasm.Analysis.Curves.Boundary).IsSupported);
        Assert.True(condition: !Rasm.Analysis.Analyze.Curves<BrepFace, Curve>(aspect: Rasm.Analysis.Curves.Segments).IsSupported);
        Assert.True(condition: !Rasm.Analysis.Analyze.Curves<Mesh, Curve>(aspect: Rasm.Analysis.Curves.NakedInner).IsSupported);
    }

    private static Context ValidContext() =>
        Context.Create(
                absolute: 0.01,
                relative: 0.001,
                angle: Math.PI / 180.0,
                units: UnitSystem.Millimeters)
            .ToFin()
            .Match(
                Succ: static context => context,
                Fail: static error => throw new Xunit.Sdk.XunitException(error.Message));
    private static Line ValidLine() =>
        new(
            from: Point3d.Origin,
            to: new Point3d(x: 2.0, y: 0.0, z: 0.0));
}
