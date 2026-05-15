using LanguageExt;
using Rasm.Analysis;
using Rasm.Domain;
using Rasm.Grasshopper;
using Rhino.Geometry;
using Xunit;

namespace Rasm.Tests.Grasshopper;

public sealed class ShapeSpec {
    [Fact]
    public void AcceptsKnownGeometryShapes() {
        Fin<Shape> line = Shape.Create(value: new Line(from: Point3d.Origin, to: new Point3d(x: 1.0, y: 0.0, z: 0.0)));

        Assert.True(condition: line.IsSucc);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(true)]
    public void RejectsScalarShapes(object value) =>
        Assert.True(condition: Shape.Create(value: value).IsFail);

    [Fact]
    public void RejectsDomainAndAnalysisValues() {
        Assert.True(condition: Shape.Create(value: Rasm.Domain.Kind.Mesh).IsFail);
        Assert.True(condition: Shape.Create(value: MeshCheckParameters.Defaults()).IsFail);
        Assert.True(condition: Shape.Create(value: MeshMetric.Area).IsFail);
        Assert.True(condition: Shape.Create(value: default(Stat)).IsFail);
    }

}
