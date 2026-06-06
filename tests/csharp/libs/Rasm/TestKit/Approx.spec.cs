using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.TestKit;

// --- [OPERATIONS] ---------------------------------------------------------------------------------
public sealed class ApproxLaws {
    [Fact]
    public void HybridToleranceCombinesAbsoluteAndRelativeMargins() {
        Tolerance tolerance = Tolerance.Hybrid(absolute: 0.01, relative: 0.1);
        Assert.True(condition: Approx.Equal(left: 100.0, right: 109.0, tolerance: tolerance));
        Assert.False(condition: Approx.Equal(left: 100.0, right: 112.0, tolerance: tolerance));
    }

    [Fact]
    public void GeometryEqualityIsSymmetricAndComponentwise() =>
        Spec.ForAll(Gens.Point.Select(Gens.Vec, static (point, delta) => (Point: point, Delta: delta * 1.0e-12)), sample => {
            Point3d moved = sample.Point + sample.Delta;
            Assert.Equal(
                expected: Approx.Equal(left: sample.Point, right: moved, tolerance: Tolerance.Absolute(epsilon: 1.0e-9)),
                actual: Approx.Equal(left: moved, right: sample.Point, tolerance: Tolerance.Absolute(epsilon: 1.0e-9)));
            Assert.True(condition: Approx.Equal(left: sample.Point, right: sample.Point, tolerance: Tolerance.Absolute(epsilon: 0.0)));
        });
}
