using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Rhino.Geometry;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
[System.Diagnostics.CodeAnalysis.SuppressMessage(category: "Design", checkId: "CA1515", Justification = "xUnit discovers public test surface.")]
public static class FlowGens {
    public static readonly Op Key = Op.Of(name: "flow-test");
    public static readonly Gen<IntegratorKind> Integrator = Gen.OneOfConst(
        IntegratorKind.Euler, IntegratorKind.Heun, IntegratorKind.Midpoint, IntegratorKind.Ralston, IntegratorKind.RK4, IntegratorKind.RK38,
        IntegratorKind.BogackiShampine, IntegratorKind.CashKarp, IntegratorKind.DormandPrince);
    public static readonly Gen<IntegratorKind> Adaptive = Gen.OneOfConst(
        IntegratorKind.BogackiShampine, IntegratorKind.CashKarp, IntegratorKind.DormandPrince);
    public static readonly Gen<IntegratorKind> NonAdaptive = Gen.OneOfConst(
        IntegratorKind.Euler, IntegratorKind.Heun, IntegratorKind.Midpoint, IntegratorKind.Ralston, IntegratorKind.RK4, IntegratorKind.RK38);
    public static double Sum(Seq<double> xs) => xs.Fold(initialState: 0.0, f: static (acc, x) => acc + x);
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class TerminationLaws {
    [Fact]
    public void FactoriesRejectInvalidBudgets() {
        Spec.ForAll(Gen.Int[-1000, 0], n => Spec.Fail(Termination.Steps(count: n, key: FlowGens.Key)));
        Spec.ForAll(Gens.NonPositive, x => {
            Spec.Fail(Termination.ArcLength(length: x, key: FlowGens.Key));
            Spec.Fail(Termination.Magnitude(threshold: x, key: FlowGens.Key));
        });
    }
    [Fact]
    public void StepCountArcAndMagnitudeBoundariesFire() {
        StreamlineState state = new(Trail: Seq(Point3d.Origin), Current: Point3d.Origin, H: 1.0, Arc: 0.0, Steps: 0, Rejects: 0, RejectedSteps: 0, Stop: Option<StreamlineStopKind>.None);
        Termination steps = Spec.SuccValue(Termination.Steps(count: 5, key: FlowGens.Key), label: "steps");
        Termination arc = Spec.SuccValue(Termination.ArcLength(length: 2.0, key: FlowGens.Key), label: "arc");
        Termination mag = Spec.SuccValue(Termination.Magnitude(threshold: 1.0, key: FlowGens.Key), label: "mag");
        Spec.Succ(steps.ShouldStop(state: state with { Steps = 5 }, currentSample: Vector3d.Zero, context: null!, key: FlowGens.Key), then: Assert.True);
        Spec.Succ(steps.ShouldStop(state: state with { Steps = 4 }, currentSample: Vector3d.Zero, context: null!, key: FlowGens.Key), then: Assert.False);
        Spec.Succ(arc.ShouldStop(state: state with { Arc = 2.0 }, currentSample: Vector3d.Zero, context: null!, key: FlowGens.Key), then: Assert.True);
        Spec.Succ(mag.ShouldStop(state: state, currentSample: new Vector3d(x: 0.5, y: 0.0, z: 0.0), context: null!, key: FlowGens.Key), then: Assert.True);
        Spec.Succ(mag.ShouldStop(state: state, currentSample: new Vector3d(x: 2.0, y: 0.0, z: 0.0), context: null!, key: FlowGens.Key), then: Assert.False);
    }
}

public sealed class IntegratorKindLaws {
    [Fact]
    public void TableauMetadataIsCoherent() {
        Spec.ForAll(FlowGens.Adaptive, k => Assert.True(k.IsAdaptive));
        Spec.ForAll(FlowGens.NonAdaptive, k => Assert.False(k.IsAdaptive));
        Spec.ForAll(FlowGens.Integrator, k => {
            Assert.True(k.Tableau.IsValid);
            Assert.Equal(expected: k.Tableau.Weights.Count, actual: k.StageCount);
            Assert.Equal(expected: k.Tableau.MethodOrder, actual: k.Order);
            Spec.EqualWithin(left: FlowGens.Sum(xs: k.Tableau.Weights), right: 1.0, tolerance: 1.0e-10, what: "weights");
            Spec.Holds(condition: k.Tableau.Coupling.AsIterable().Select((row, i) => row.Count <= i).All(static ok => ok), label: "Butcher coupling[i].Count <= i");
        });
        Spec.ForAll(FlowGens.Adaptive, k => Spec.EqualWithin(
            left: k.AdaptiveExponent,
            right: k.EmbeddedOrder.Match(Some: static order => 1.0 / (order + 1.0), None: static () => 0.2),
            tolerance: 0.0,
            what: "adaptive exponent"));
    }
    [Fact]
    public void TableauAdmissionRejectsFalseMethodClaims() {
        Spec.Fail(new ButcherTableau(Coupling: Seq(Seq<double>()), Weights: Seq(1.0), ErrorWeights: Option<Seq<double>>.None, MethodOrder: 0, EmbeddedOrder: Option<int>.None).Admit(key: FlowGens.Key));
        Spec.Fail(new ButcherTableau(Coupling: Seq(Seq<double>(), Seq(1.0)), Weights: Seq(0.5, 0.5), ErrorWeights: Some(Seq(0.5, 0.5)), MethodOrder: 2, EmbeddedOrder: Some(2)).Admit(key: FlowGens.Key));
        Spec.Fail(new ButcherTableau(Coupling: Seq(Seq<double>()), Weights: Seq(0.5), ErrorWeights: Option<Seq<double>>.None, MethodOrder: 1, EmbeddedOrder: Option<int>.None).Admit(key: FlowGens.Key));
    }
}

public sealed class FieldIntegratorLaws {
    [Fact]
    public void AdaptiveFactoryAdmitsOnlyAdaptiveKinds() {
        Spec.ForAll(FlowGens.Adaptive, k => Spec.Succ(FieldIntegrator.Adaptive(kind: k, tolerance: 1.0e-6, key: FlowGens.Key)));
        Spec.ForAll(FlowGens.NonAdaptive, k => Spec.Fail(FieldIntegrator.Adaptive(kind: k, tolerance: 1.0e-6, key: FlowGens.Key)));
    }
    [Fact]
    public void AdaptiveRejectsInvalidToleranceOrBudget() {
        Spec.ForAll(Gens.NonPositive, t => Spec.Fail(FieldIntegrator.Adaptive(kind: IntegratorKind.DormandPrince, tolerance: t, key: FlowGens.Key)));
        Spec.ForAll(Gen.Int[-100, -1], m => Spec.Fail(FieldIntegrator.Adaptive(kind: IntegratorKind.DormandPrince, tolerance: 1.0e-6, maxRejects: m, key: FlowGens.Key)));
    }
    [Fact]
    public void FixedAndTraceReceiptsExposeBoundedStops() {
        Assert.Equal(expected: 0, actual: FieldIntegrator.RK4.RejectBudget);
        StreamlineStopKind[] stops = [StreamlineStopKind.Terminated, StreamlineStopKind.RejectBudgetExhausted, StreamlineStopKind.IterationCapExhausted];
        Spec.SmartEnumKeysUnique(items: stops, key: static s => s.Key);
        Assert.True(new StreamlineTrace(Trail: Seq(Point3d.Origin), Stop: StreamlineStopKind.Terminated, AcceptedSteps: 1, RejectedSteps: 0, ArcLength: 1.0, FinalStep: 0.5).IsComplete);
        Assert.False(new StreamlineTrace(Trail: Seq(Point3d.Origin), Stop: StreamlineStopKind.IterationCapExhausted, AcceptedSteps: 0, RejectedSteps: 0, ArcLength: 0.0, FinalStep: 0.5).IsComplete);
    }
}
