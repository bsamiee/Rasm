using Grasshopper2.Components;
using Grasshopper2.Parameters.Standard;
using NUnit.Framework;
using Radyab.Components;
using Rhino.Testing.Fixtures;

namespace Runtime.Rhino.Tests.Grasshopper;

// --- [RUNTIME SPECS] ---------------------------------------------------------------------------

[TestFixture]
[RhinoTestFixture]
public sealed class ComponentRuntimeSpec {
    [Test]
    public void ConstructsExtractionComponentsWithStablePortCatalogs() =>
        Assert.Multiple(static () => {
            AssertComponent(new ExtractCurves(), inputs: 4, outputs: 17);
            AssertComponent(new ExtractPoints(), inputs: 1, outputs: 8);
            AssertComponent(new ExtractSurfaces(), inputs: 2, outputs: 11);
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

    private static void AssertComponent(Component component, int inputs, int outputs) =>
        Assert.Multiple(() => {
            Assert.That(actual: component.Parameters.InputCount, expression: Is.EqualTo(expected: inputs));
            Assert.That(actual: component.Parameters.OutputCount, expression: Is.EqualTo(expected: outputs));
        });
}
