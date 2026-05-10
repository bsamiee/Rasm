using Grasshopper2.Components;
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

    private static void AssertComponent(Component component, int inputs, int outputs) =>
        Assert.Multiple(() => {
            Assert.That(actual: component.Parameters.InputCount, expression: Is.EqualTo(expected: inputs));
            Assert.That(actual: component.Parameters.OutputCount, expression: Is.EqualTo(expected: outputs));
        });
}
