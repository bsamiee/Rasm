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
    public void RejectsInvalidGeometryResults() {
        OperationKey key = new(name: "test");

        Assert.True(condition: key.One(value: Point3d.Unset).IsFail);
        Assert.True(condition: key.One(value: Sphere.Unset).IsFail);
        Assert.True(condition: key.One(value: new ComponentIndex()).IsFail);
        Assert.True(condition: key.One(value: new object()).IsFail);
        Assert.True(condition: key.Many(values: new[] {
                Line.Unset,
                Line.Unset,
            })
            .Match(
                Succ: static (Seq<Line> _) => false,
                Fail: static (Error error) => error.Count == 2));
    }
}
