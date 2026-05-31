using Rasm.TestKit;
using Rhino;

namespace Rasm.Tests.TestKit;

// --- [LAWS] ---------------------------------------------------------------------------------
public sealed class GeneratorLaws {
    [Fact]
    public void UnitInteriorNeverEmitsClosedBoundaryValues() =>
        Spec.ForAll(Gens.UnitInterior, value =>
            Spec.Holds(
                condition: RhinoMath.IsValidDouble(x: value) && value > 0.0 && value < 1.0,
                label: $"UnitInterior emitted boundary/non-finite value {value:R}"));

    [Fact]
    public void SimplexConservesMassAndRejectsNegativeComponents() =>
        Spec.ForAll(Gen.Int[start: 1, finish: 16], count =>
            Spec.ForAll(Gens.Simplex(count: count), values => {
                Assert.Equal(expected: count, actual: values.Count);
                Spec.Equal(left: values.Fold(initialState: 0.0, f: static (sum, value) => sum + value), right: 1.0, tolerance: 1.0e-12, what: "simplex mass");
                Spec.Holds(condition: values.ForAll(static value => RhinoMath.IsValidDouble(x: value) && value > 0.0), label: "simplex component");
            }));
}
