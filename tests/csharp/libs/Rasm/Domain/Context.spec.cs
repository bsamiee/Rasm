using Rasm.Domain;
using Rasm.TestKit;
using Rhino;

namespace Rasm.Tests.Domain;

// --- [MODELS] ----------------------------------------------------------------------------
// BRIDGE-DEFERRED: Context.Of(UnitSystem/RhinoDoc); static owns tolerance validation and Custom/Unset/None reject before UnitScale.
internal static class ContextGens {
    public static readonly Op Key = Op.Of(name: "context-test");
    public static readonly Gen<double> AbsoluteValid = Gen.Frequency(
        (90, Gen.Double[start: RhinoMath.ZeroTolerance * 2.0, finish: 1.0e3]),
        (10, Gen.OneOfConst(RhinoMath.ZeroTolerance * 1.5, RhinoMath.SqrtEpsilon, 1.0, 1.0e6)));
    public static readonly Gen<double> RelativeValid = Gen.Frequency(
        (90, Gen.Double[start: 0.0, finish: 1.0 - RhinoMath.ZeroTolerance]),
        (10, Gen.OneOfConst(0.0, RhinoMath.ZeroTolerance, 0.5, 1.0 - RhinoMath.ZeroTolerance)));
    public static readonly Gen<double> AngleValid = Gen.Frequency(
        (90, Gen.Double[start: RhinoMath.Epsilon * 2.0, finish: RhinoMath.TwoPI]),
        (10, Gen.OneOfConst(RhinoMath.Epsilon * 2.0, Math.PI, RhinoMath.TwoPI)));
    public static readonly Gen<UnitSystem> ModelUnits = Gen.OneOfConst(
        UnitSystem.Millimeters, UnitSystem.Centimeters, UnitSystem.Meters, UnitSystem.Inches);
    public static readonly UnitSystem[] RejectedUnits = [UnitSystem.CustomUnits, UnitSystem.Unset, UnitSystem.None];
}

// --- [OPERATIONS] ----------------------------------------------------------------------------
public sealed class AbsoluteToleranceLaws {
    [Fact]
    public void AcceptsAboveZeroToleranceAndBoundary() {
        Spec.ValueObjectAccepts(valid: ContextGens.AbsoluteValid, tryCreate: static x => AbsoluteTolerance.TryCreate(value: x, obj: out _));
        Spec.Cases(items: [0.0, RhinoMath.ZeroTolerance, -1.0], key: static x => x,
            law: static x => Assert.False(condition: AbsoluteTolerance.TryCreate(value: x, obj: out _)));
    }
    [Fact]
    public void RejectsNonFinite() =>
        Spec.ValueObjectRejectsNonFinite(tryCreate: static x => AbsoluteTolerance.TryCreate(value: x, obj: out _));
    [Fact]
    public void ValueRoundtripsThroughFactory() =>
        Spec.ValueObjectRoundtrip(validGen: ContextGens.AbsoluteValid, tryCreate: AbsoluteTolerance.TryCreate, read: static (AbsoluteTolerance a) => a.Value);
}

public sealed class RelativeToleranceLaws {
    [Fact]
    public void AcceptsHalfOpenUnitRange() {
        Spec.ValueObjectAccepts(valid: ContextGens.RelativeValid, tryCreate: static x => RelativeTolerance.TryCreate(value: x, obj: out _));
        Spec.Cases(items: [-RhinoMath.ZeroTolerance, 1.0, 1.0 + RhinoMath.ZeroTolerance, 2.0], key: static x => x,
            law: static x => Assert.False(condition: RelativeTolerance.TryCreate(value: x, obj: out _)));
        Assert.True(condition: RelativeTolerance.TryCreate(value: 0.0, obj: out _));
    }
    [Fact]
    public void RejectsNonFinite() =>
        Spec.ValueObjectRejectsNonFinite(tryCreate: static x => RelativeTolerance.TryCreate(value: x, obj: out _));
    [Fact]
    public void ValueRoundtripsThroughFactory() =>
        Spec.ValueObjectRoundtrip(validGen: ContextGens.RelativeValid, tryCreate: RelativeTolerance.TryCreate, read: static (RelativeTolerance r) => r.Value);
}

public sealed class AngleToleranceLaws {
    [Fact]
    public void AcceptsOpenLowerClosedUpperRange() {
        Spec.ValueObjectAccepts(valid: ContextGens.AngleValid, tryCreate: static x => AngleTolerance.TryCreate(value: x, obj: out _));
        Spec.Cases(items: [0.0, RhinoMath.Epsilon, RhinoMath.TwoPI + RhinoMath.ZeroTolerance, -1.0], key: static x => x,
            law: static x => Assert.False(condition: AngleTolerance.TryCreate(value: x, obj: out _)));
        Assert.True(condition: AngleTolerance.TryCreate(value: RhinoMath.TwoPI, obj: out _));
    }
    [Fact]
    public void RejectsNonFinite() =>
        Spec.ValueObjectRejectsNonFinite(tryCreate: static x => AngleTolerance.TryCreate(value: x, obj: out _));
    [Fact]
    public void ValueRoundtripsThroughFactory() =>
        Spec.ValueObjectRoundtrip(validGen: ContextGens.AngleValid, tryCreate: AngleTolerance.TryCreate, read: static (AngleTolerance a) => a.Value);
}

public sealed class ContextValidationLaws {
    [Fact]
    public void ValidTupleReadsAllFieldsBack() =>
        Spec.ForAll(
            ContextGens.AbsoluteValid.Select(ContextGens.RelativeValid, ContextGens.AngleValid, ContextGens.ModelUnits,
                static (double a, double r, double n, UnitSystem u) => (A: a, R: r, N: n, U: u)),
            static t => Spec.Valid(Context.Of(absolute: t.A, relative: t.R, angle: t.N, units: t.U), then: ctx => {
                Spec.Equal(left: ctx.Absolute.Value, right: t.A, tolerance: 0.0, what: "absolute");
                Spec.Equal(left: ctx.Relative.Value, right: t.R, tolerance: 0.0, what: "relative");
                Spec.Equal(left: ctx.Angle.Value, right: t.N, tolerance: 0.0, what: "angle");
                Assert.Equal(expected: t.U, actual: ctx.Units);
            }));
    [Fact]
    public void EachBrokenFieldSurfacesItsCategory() {
        Spec.ForAll(ContextGens.RelativeValid.Select(ContextGens.AngleValid, static (double r, double n) => (R: r, N: n)),
            static t => Spec.AllErrors(Context.Of(absolute: 0.0, relative: t.R, angle: t.N, units: UnitSystem.Millimeters), "Tolerance"));
        Spec.ForAll(ContextGens.AbsoluteValid.Select(ContextGens.AngleValid, static (double a, double n) => (A: a, N: n)),
            static t => Spec.AllErrors(Context.Of(absolute: t.A, relative: -1.0, angle: t.N, units: UnitSystem.Millimeters), "Tolerance"));
        Spec.ForAll(ContextGens.AbsoluteValid.Select(ContextGens.RelativeValid, static (double a, double r) => (A: a, R: r)),
            static t => Spec.AllErrors(Context.Of(absolute: t.A, relative: t.R, angle: 0.0, units: UnitSystem.Millimeters), "Tolerance"));
        Spec.ForAll(ContextGens.AbsoluteValid,
            static a => Spec.AllErrors(Context.Of(absolute: a, relative: 0.5, angle: Math.PI, units: UnitSystem.None), "Context"));
    }
    [Fact]
    public void DistinctChannelFaultsAccumulateBothCategories() =>
        Spec.ForAll(ContextGens.RelativeValid.Select(ContextGens.AngleValid, static (double r, double n) => (R: r, N: n)),
            static t => Spec.AllErrors(Context.Of(absolute: double.NaN, relative: t.R, angle: t.N, units: UnitSystem.None), "Tolerance", "Context"));
}

public sealed class ContextUnitSystemLaws(ContextFixture fixture) {
    [Fact]
    public void RejectsNonModelUnitSystems() =>
        Spec.Cases(items: ContextGens.RejectedUnits, key: static u => (int)u,
            law: static u => Spec.Invalid(Context.Of(units: u), then: error => Assert.Equal(expected: "Context", actual: error.Category())));
    [Fact]
    public void FractionalIsClosedFormOverRelative() {
        Spec.ForAll(Gens.Context, static ctx =>
            Spec.Equal(left: ctx.Fractional, right: ctx.Relative.Value > 0.0 ? ctx.Relative.Value : 1.0e-8, tolerance: 0.0, what: "Fractional closed-form"));
        Spec.Equal(left: fixture.Default.Fractional, right: fixture.Default.Relative.Value, tolerance: 0.0, what: "fixture relative>0 selects relative branch");
        Spec.Equal(left: Spec.SuccValue(result: Context.Of(absolute: 0.001, relative: 0.0, angle: 0.01, units: UnitSystem.Millimeters).ToFin(), label: "zero-relative").Fractional, right: 1.0e-8, tolerance: 0.0, what: "relative=0 selects default branch");
    }
}
