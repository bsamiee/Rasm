using System.Reflection;
using LanguageExt;
using Rasm.Analysis;
using Rasm.Domain;
using Rasm.Grasshopper;
using Rhino.Geometry;
using Xunit;
using static LanguageExt.Prelude;

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
        Assert.True(condition: Shape.Create(value: MeshFaceMetric.Area).IsFail);
    }

    [Fact]
    public void PluginValidationReportsNullOutputGroupsBeforePortDerivation() {
        Component probe = new NullOutputComponent();
        Seq<string> faults = ValidateComponent(spec: typeof(NullOutputComponent));

        Assert.NotNull(@object: probe);
        Assert.Contains(collection: faults, filter: static fault => fault.Contains(value: "output 0 is null", comparisonType: StringComparison.Ordinal));
    }

    private static Seq<string> ValidateComponent(Type spec) =>
        typeof(Plugin).GetMethod(name: "Validate", bindingAttr: BindingFlags.NonPublic | BindingFlags.Static) switch {
            MethodInfo method => (Seq<string>)method.Invoke(obj: null, parameters: [spec])!,
            _ => Seq<string>(),
        };

    private sealed class NullOutputComponent : Component {
        private static readonly Port<Shape> Geometry = Port.Shape();
        private static readonly ComponentSpec Definition = new(
            Inputs: Seq(new PortSpec(Port: Geometry)),
            Outputs: Seq(new OutputSpec(Group: null!)));
        public NullOutputComponent() : base(self: typeof(NullOutputComponent), spec: Definition) { }
    }
}
