using Grasshopper2.Components;
using Grasshopper2.Parameters.Standard;
using NUnit.Framework;
using Radyab.Components;
using Rasm.Domain;
using Rasm.Grasshopper;
using Rhino.Testing.Fixtures;

namespace Runtime.Rhino.Tests.Grasshopper;

// --- [RUNTIME SPECS] ---------------------------------------------------------------------------

[TestFixture]
[RhinoTestFixture]
public sealed class ComponentRuntimeSpec {
    [Test]
    public void ConstructsExtractionComponentsFromDeclaredContracts() =>
        Assert.Multiple(static () => {
            AssertComponent(component: new ExtractCurves(), inputs: ExtractCurves.Spec.Inputs, outputs: ExtractCurves.Spec.Outputs);
            AssertComponent(component: new ExtractPoints(), inputs: ExtractPoints.Spec.Inputs, outputs: ExtractPoints.Spec.Outputs);
            AssertComponent(component: new ExtractSurfaces(), inputs: ExtractSurfaces.Spec.Inputs, outputs: ExtractSurfaces.Spec.Outputs);
        });

    [Test]
    public void UsesSingleGenericGeometryInputContract() =>
        Assert.Multiple(static () => {
            Assert.That(actual: new ExtractCurves().Parameters.Input(index: 0), expression: Is.TypeOf<GenericParameter>());
            Assert.That(actual: new ExtractPoints().Parameters.Input(index: 0), expression: Is.TypeOf<GenericParameter>());
            Assert.That(actual: new ExtractSurfaces().Parameters.Input(index: 0), expression: Is.TypeOf<GenericParameter>());
        });

    [Test]
    public void AppliesNativeParameterPolicies() {
        Component component = new ExtractCurves();
        IntegerParameter index = (IntegerParameter)component.Parameters.Input(index: 1);
        VectorParameter direction = (VectorParameter)component.Parameters.Input(index: 2);
        AngleParameter draft = (AngleParameter)component.Parameters.Input(index: 3);

        Assert.Multiple(() => {
            Assert.That(actual: index.IsIndex, expression: Is.True);
            Assert.That(actual: index.Indexing, expression: Is.EqualTo(expected: IndexModifier.Clip));
            Assert.That(actual: direction.UnitiseVectors, expression: Is.True);
            Assert.That(actual: direction.ReverseVectors, expression: Is.False);
            Assert.That(actual: draft.ReduceAngles, expression: Is.True);
        });
    }

    [Test]
    public void PreservesExtractionOutputOrdering() =>
        Assert.Multiple(static () => {
            Assert.That(actual: Codes(outputs: ExtractCurves.Spec.Outputs), expression: Is.EqualTo(expected: new[] { "AC", "CS", "CF", "S", "SCV", "BC", "NO", "NI", "IE", "NE", "OL", "IL", "U", "V", "SC", "DC", "IC" }));
            Assert.That(actual: Codes(outputs: ExtractPoints.Spec.Outputs), expression: Is.EqualTo(expected: new[] { "EM", "SC", "BC", "V", "CP", "B", "Q", "K" }));
            Assert.That(actual: Codes(outputs: ExtractSurfaces.Spec.Outputs), expression: Is.EqualTo(expected: new[] { "AS", "SI", "TS", "BS", "IS", "UV", "FC", "FN", "FI", "CI", "UD" }));
        });

    [Test]
    public void MapsDefaultPortKindsAndEnumKinds() =>
        Assert.Multiple(static () => {
            Assert.That(actual: PortKind.From(type: typeof(int)).IfNone(PortKind.Generic), expression: Is.SameAs(expected: PortKind.Integer));
            Assert.That(actual: PortKind.From(type: typeof(double)).IfNone(PortKind.Generic), expression: Is.SameAs(expected: PortKind.Number));
            Assert.That(actual: PortKind.From(type: typeof(string)).IfNone(PortKind.Generic), expression: Is.SameAs(expected: PortKind.Text));
            Assert.That(actual: PortKind.From(type: typeof(GeometryKind)).IfNone(PortKind.Generic), expression: Is.SameAs(expected: PortKind.Generic));
            Assert.That(actual: PortKind.Enum(initial: GeometryKind.Unknown).Type, expression: Is.EqualTo(expected: typeof(GeometryKind)));
        });

    private static void AssertComponent(Component component, Seq<IPort> inputs, Seq<IOutputGroup> outputs) =>
        Assert.Multiple(() => {
            Assert.That(actual: component.Parameters.InputCount, expression: Is.EqualTo(expected: inputs.Count));
            Assert.That(actual: component.Parameters.OutputCount, expression: Is.EqualTo(expected: outputs.Bind(static group => group.Ports).Count));
        });

    private static string[] Codes(Seq<IOutputGroup> outputs) =>
        [.. outputs.Bind(static group => group.Ports).Map(static port => port.Code)];
}
