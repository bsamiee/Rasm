using Rasm.TestKit;
using Rhino;

namespace Rasm.Tests.TestKit;

// --- [OPERATIONS] ---------------------------------------------------------------------------------
public sealed class FixtureLaws {
    [Fact]
    public void HostBundleRegistrationIsIdempotentAcrossFixtureConstruction() {
        _ = new ContextFixture();
        _ = new ContextFixture();
        Assert.Equal(expected: 1, actual: HostBundle.RegistrationCount);
    }

    [Fact]
    public void ContextFixtureMaterializesDistinctUnitContexts() {
        ContextFixture fixture = new();
        Assert.Equal(expected: UnitSystem.Millimeters, actual: fixture.Mm.Units);
        Assert.Equal(expected: UnitSystem.Centimeters, actual: fixture.Cm.Units);
        Assert.Equal(expected: UnitSystem.Meters, actual: fixture.Meters.Units);
        Assert.Equal(expected: UnitSystem.Inches, actual: fixture.Inches.Units);
        Assert.Same(expected: fixture.Mm, actual: fixture.Default);
        Spec.Holds(condition: fixture.Mm.Absolute.Value > 0.0 && fixture.Cm.Absolute.Value > fixture.Mm.Absolute.Value, label: "context tolerances");
    }
}
