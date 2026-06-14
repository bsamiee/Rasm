using Rasm.Domain;
using Rhino;

namespace Rasm.TestKit;

// --- [SERVICES] -----------------------------------------------------------------------------
// AssemblyFixture registration is central; consumers use constructor injection only.
public sealed class ContextFixture {
    public ContextFixture() {
        HostBundle.Register();
        Mm = Materialize(absolute: 0.001, relative: 1.0e-8, angle: 0.01, units: UnitSystem.Millimeters);
        Cm = Materialize(absolute: 0.01, relative: 1.0e-8, angle: 0.01, units: UnitSystem.Centimeters);
        Meters = Materialize(absolute: 0.001, relative: 1.0e-8, angle: 0.01, units: UnitSystem.Meters);
        Inches = Materialize(absolute: 0.001, relative: 1.0e-8, angle: 0.01, units: UnitSystem.Inches);
    }
    public Context Mm { get; }
    public Context Cm { get; }
    public Context Meters { get; }
    public Context Inches { get; }
    public Context Default => Mm;
    public Op Key { get; } = Op.Of(name: "context-fixture");
    private static Context Materialize(double absolute, double relative, double angle, UnitSystem units) =>
        Spec.SuccValue(result: Context.Of(absolute: absolute, relative: relative, angle: angle, units: units).ToFin(), label: $"{units} context");
}
