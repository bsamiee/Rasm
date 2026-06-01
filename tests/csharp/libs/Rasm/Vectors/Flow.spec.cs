using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Rhino.Geometry;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
internal static class FlowGens {
    public static readonly Context Model = Spec.SuccValue(Context.Of(absolute: 0.001, relative: 1.0e-8, angle: 0.01, units: Rhino.UnitSystem.Millimeters).ToFin(), label: "flow context");
    public static readonly Op Key = Op.Of(name: "flow-test");
    public static readonly Gen<IntegratorKind> Integrator = Gen.OneOfConst(
        IntegratorKind.Euler, IntegratorKind.Heun, IntegratorKind.Midpoint, IntegratorKind.Ralston, IntegratorKind.RK4, IntegratorKind.RK38,
        IntegratorKind.BogackiShampine, IntegratorKind.CashKarp, IntegratorKind.DormandPrince);
    public static readonly Gen<IntegratorKind> Adaptive = Gen.OneOfConst(
        IntegratorKind.BogackiShampine, IntegratorKind.CashKarp, IntegratorKind.DormandPrince);
    public static readonly Gen<IntegratorKind> NonAdaptive = Gen.OneOfConst(
        IntegratorKind.Euler, IntegratorKind.Heun, IntegratorKind.Midpoint, IntegratorKind.Ralston, IntegratorKind.RK4, IntegratorKind.RK38);
    public static StreamlineState State(Seq<Point3d> trail, Point3d current, double h, double arc, int steps) =>
        new(
            Trail: trail,
            Current: current,
            H: h,
            Arc: arc,
            Steps: steps,
            Rejects: 0,
            RejectedSteps: 0,
            MinStep: h,
            MaxStep: h,
            LastError: Option<double>.None,
            MaxError: 0.0,
            Event: Option<TraceEvent>.None,
            Stop: Option<StreamlineStopKind>.None);
    public static StreamlineTrace Trace(StreamlineStopKind stop) =>
        new(
            Trail: stop.Equals(StreamlineStopKind.Terminated) ? Gens.UnitSegment3 : Seq(Point3d.Origin),
            Stop: stop,
            AcceptedSteps: stop.Equals(StreamlineStopKind.Terminated) ? 1 : 0,
            RejectedSteps: 0,
            ArcLength: stop.Equals(StreamlineStopKind.Terminated) ? 1.0 : 0.0,
            FinalStep: 0.5,
            MethodOrder: IntegratorKind.RK4.Tableau.MethodOrder,
            EmbeddedOrder: Option<int>.None,
            LastError: Option<double>.None,
            MaxError: 0.0,
            MinStep: 0.5,
            MaxStep: 0.5,
            TerminationPoint: Point3d.Origin,
            Event: Option<TraceEvent>.None);
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class TerminationLaws {
    [Fact]
    public void FactoriesRejectInvalidBudgets() {
        Spec.ForAll(Gen.Int[-1000, 0], n => Spec.FailCategory(Termination.Steps(count: n, key: FlowGens.Key), category: "Input"));
        Spec.ForAll(Gens.NonPositive, x => {
            Spec.FailCategory(Termination.ArcLength(length: x, key: FlowGens.Key), category: "Tolerance");
            Spec.FailCategory(Termination.Magnitude(threshold: x, key: FlowGens.Key), category: "Tolerance");
        });
    }
    [Fact]
    public void StepCountArcAndMagnitudeBoundariesFire() {
        StreamlineState state = FlowGens.State(trail: Seq(Point3d.Origin), current: Point3d.Origin, h: 1.0, arc: 0.0, steps: 0);
        Termination steps = Spec.SuccValue(Termination.Steps(count: 5, key: FlowGens.Key), label: "steps");
        Termination arc = Spec.SuccValue(Termination.ArcLength(length: 2.0, key: FlowGens.Key), label: "arc");
        Termination mag = Spec.SuccValue(Termination.Magnitude(threshold: 1.0, key: FlowGens.Key), label: "mag");
        Spec.Succ(steps.Evaluate(state: state with { Steps = 5 }, currentSample: Vector3d.Zero, context: FlowGens.Model, key: FlowGens.Key),
            then: decision => { Assert.True(condition: decision.Stop); Assert.True(condition: decision.Event.IsNone); });
        Spec.Succ(steps.Evaluate(state: state with { Steps = 4 }, currentSample: Vector3d.Zero, context: FlowGens.Model, key: FlowGens.Key),
            then: decision => { Assert.False(condition: decision.Stop); Assert.True(condition: decision.Event.IsNone); });
        Spec.Succ(arc.Evaluate(state: state with { Arc = 2.0 }, currentSample: Vector3d.Zero, context: FlowGens.Model, key: FlowGens.Key),
            then: decision => { Assert.True(condition: decision.Stop); Assert.True(condition: decision.Event.IsNone); });
        Spec.Succ(mag.Evaluate(state: state, currentSample: new Vector3d(x: 0.5, y: 0.0, z: 0.0), context: FlowGens.Model, key: FlowGens.Key),
            then: decision => { Assert.True(condition: decision.Stop); Assert.True(condition: decision.Event.IsNone); });
        Spec.Succ(mag.Evaluate(state: state, currentSample: new Vector3d(x: 2.0, y: 0.0, z: 0.0), context: FlowGens.Model, key: FlowGens.Key),
            then: decision => { Assert.False(condition: decision.Stop); Assert.True(condition: decision.Event.IsNone); });
    }
    [Fact]
    public void RegionLoopAndSurfaceTerminatorsClassifyBoundaryModes() {
        StreamlineState state = FlowGens.State(trail: Seq(Point3d.Origin, new Point3d(x: 2.0, y: 0.0, z: 0.0), new Point3d(x: 3.0, y: 0.0, z: 0.0)), current: new Point3d(x: 0.1, y: 0.0, z: 0.0), h: 1.0, arc: 2.0, steps: 2);
        Spec.Succ(Termination.LoopDetected(closureRadius: 0.2, key: FlowGens.Key),
            then: t => Spec.Succ(t.Evaluate(state: state, currentSample: Vector3d.XAxis, context: FlowGens.Model, key: FlowGens.Key),
                then: decision => { Assert.True(condition: decision.Stop); Assert.True(condition: decision.Event.IsNone); }));
        Spec.Succ(Termination.RegionThreshold(region: ScalarField.Constant(value: 1.0), threshold: 0.5, key: FlowGens.Key),
            then: t => Spec.Succ(t.Evaluate(state: state, currentSample: Vector3d.XAxis, context: FlowGens.Model, key: FlowGens.Key),
                then: decision => { Assert.False(condition: decision.Stop); Assert.True(condition: decision.Event.IsNone); }));
        Spec.Succ(SupportSpace.Of(value: new Sphere(center: Point3d.Origin, radius: 1.0), key: FlowGens.Key),
            then: s => Spec.Succ(Termination.CrossSurface(surface: s, key: FlowGens.Key)));
        Spec.Succ(SupportSpace.Of(value: new Point3d(x: 0.0, y: 0.0, z: 0.0), key: FlowGens.Key),
            then: s => Spec.Fail(Termination.CrossSurface(surface: s, key: FlowGens.Key)));
        Spec.Fail(Termination.LoopDetected(closureRadius: 0.0, key: FlowGens.Key));
        Spec.Fail(Termination.RegionThreshold(region: ScalarField.Constant(value: 1.0), threshold: double.NaN, key: FlowGens.Key));
        Spec.FailCategory(Termination.RegionThreshold(region: ScalarField.Constant(value: 1.0), threshold: 0.0, maxLocalizationIterations: 0, key: FlowGens.Key), category: "Input");
        Spec.Fail(Termination.CrossSurface(surface: null!, key: FlowGens.Key));
        Spec.FailCategory(new Termination.StepCountCase(Count: 0).Admit(key: FlowGens.Key), category: "Input");
        Spec.FailCategory(new Termination.RegionThresholdCase(Region: null!, Threshold: 0.0, MaxLocalizationIterations: 1).Admit(key: FlowGens.Key), category: "Input");
    }
    [Fact]
    public void RegionEventsDistinguishEndpointTouchesFromBracketCrossings() {
        ScalarField region = Spec.SuccValue(ScalarField.Worley(seeds: Seq(Point3d.Origin), order: 1, key: FlowGens.Key), label: "radial region");
        Termination termination = Spec.SuccValue(Termination.RegionThreshold(region: region, threshold: 1.0, maxLocalizationIterations: 1, key: FlowGens.Key), label: "region threshold");
        (StreamlineState State, TraceEventStatus Status, double Parameter)[] cases = [
            (FlowGens.State(trail: Seq(new Point3d(x: 1.0, y: 0.0, z: 0.0)), current: new Point3d(x: 1.0, y: 0.0, z: 0.0), h: 1.0, arc: 0.0, steps: 0), TraceEventStatus.InitialEndpointTouch, 0.0),
            (FlowGens.State(trail: Seq(new Point3d(x: 1.0, y: 0.0, z: 0.0), new Point3d(x: 2.0, y: 0.0, z: 0.0)), current: new Point3d(x: 2.0, y: 0.0, z: 0.0), h: 1.0, arc: 1.0, steps: 1), TraceEventStatus.PreviousEndpointTouch, 0.0),
            (FlowGens.State(trail: Seq(new Point3d(x: 2.0, y: 0.0, z: 0.0), new Point3d(x: 1.0, y: 0.0, z: 0.0)), current: new Point3d(x: 1.0, y: 0.0, z: 0.0), h: 1.0, arc: 1.0, steps: 1), TraceEventStatus.CurrentEndpointTouch, 1.0),
            (FlowGens.State(trail: Seq(new Point3d(x: 2.0, y: 0.0, z: 0.0), Point3d.Origin), current: Point3d.Origin, h: 1.0, arc: 2.0, steps: 1), TraceEventStatus.BracketedCrossing, 0.5),
        ];
        _ = toSeq(cases).Iter(item => Spec.Succ(termination.Evaluate(state: item.State, currentSample: Vector3d.XAxis, context: FlowGens.Model, key: FlowGens.Key),
            then: decision => Spec.Some(decision.Event, @event => {
                Assert.True(condition: decision.Stop);
                Assert.Equal(expected: item.Status, actual: @event.Status);
                Spec.Equal(left: @event.Parameter, right: item.Parameter, tolerance: 0.0, what: "event parameter");
                Assert.True(condition: @event.IsValidFor(terminationPoint: @event.Points.Localized));
                Assert.True(condition: @event.Residual <= @event.Tolerance);
                Assert.True(condition: @event.Iterations <= 1);
            })));
    }
}

public sealed class IntegratorKindLaws {
    [Fact]
    public void TableauMetadataIsCoherent() {
        Spec.SmartEnumCatalogMatches(production: IntegratorKind.Items, expectedKeys: [0, 1, 2, 3, 4, 5, 6, 7, 8], key: static k => k.Key);
        Spec.SmartEnumCatalogMatches(production: StreamlineStopKind.Items, expectedKeys: [0, 1, 2], key: static k => k.Key);
        Spec.SmartEnumCatalogMatches(production: TraceEventKind.Items, expectedKeys: [0, 1], key: static k => k.Key);
        Spec.SmartEnumCatalogMatches(production: TraceEventStatus.Items, expectedKeys: [0, 1, 2, 3], key: static k => k.Key);
        Spec.ForAll(FlowGens.Adaptive, k => Assert.True(k.IsAdaptive));
        Spec.ForAll(FlowGens.NonAdaptive, k => Assert.False(k.IsAdaptive));
        Spec.ForAll(FlowGens.Integrator, k => {
            Assert.True(k.Tableau.IsValid);
            Spec.Equal(left: Numeric.Sum(values: k.Tableau.Weights), right: 1.0, tolerance: 1.0e-10, what: "weights");
            Spec.Holds(condition: k.Tableau.Coupling.Zip(k.Tableau.Abscissae).ForAll(pair => Math.Abs(value: Numeric.Sum(values: pair.Item1) - pair.Item2) <= 1.0e-10), label: "Butcher row sums match abscissae");
            Spec.Holds(condition: k.Tableau.Coupling.AsIterable().Select((row, i) => row.Count <= i).All(static ok => ok), label: "Butcher coupling[i].Count <= i");
        });
        Spec.ForAll(FlowGens.Adaptive, k => Spec.Equal(
            left: k.AdaptiveExponent,
            right: k.Tableau.EmbeddedOrder.Match(Some: static order => 1.0 / (order + 1.0), None: static () => 0.2),
            tolerance: 0.0,
            what: "adaptive exponent"));
    }
    [Fact]
    public void TableauAdmissionRejectsFalseMethodClaims() {
        Spec.Fail(new ButcherTableau(Coupling: Seq(Seq<double>()), Abscissae: Seq(0.0), Weights: Seq(1.0), EmbeddedWeights: Option<Seq<double>>.None, MethodOrder: 0, EmbeddedOrder: Option<int>.None).Admit(key: FlowGens.Key));
        Spec.Fail(new ButcherTableau(Coupling: Seq(Seq<double>(), Seq(1.0)), Abscissae: Seq(0.0, 1.0), Weights: Seq(0.5, 0.5), EmbeddedWeights: Some(Seq(0.5, 0.5)), MethodOrder: 2, EmbeddedOrder: Some(2)).Admit(key: FlowGens.Key));
        Spec.Fail(new ButcherTableau(Coupling: Seq(Seq<double>()), Abscissae: Seq(0.0), Weights: Seq(0.5), EmbeddedWeights: Option<Seq<double>>.None, MethodOrder: 1, EmbeddedOrder: Option<int>.None).Admit(key: FlowGens.Key));
        Spec.Fail(new ButcherTableau(Coupling: Seq(Seq<double>(), Seq(0.5)), Abscissae: Seq(0.0, 1.0), Weights: Seq(0.5, 0.5), EmbeddedWeights: Option<Seq<double>>.None, MethodOrder: 2, EmbeddedOrder: Option<int>.None).Admit(key: FlowGens.Key));
    }
}

public sealed class FieldIntegratorLaws {
    [Fact]
    public void AdaptiveFactoryAdmitsOnlyAdaptiveKinds() {
        Spec.ForAll(FlowGens.Adaptive, k => Spec.Succ(FieldIntegrator.Adaptive(kind: k, tolerance: 1.0e-6, maxRejects: 2, key: FlowGens.Key), then: integrator => {
            FieldIntegrator.AdaptiveCase adaptive = Assert.IsType<FieldIntegrator.AdaptiveCase>(@object: integrator);
            Assert.Equal(expected: k, actual: adaptive.Kind);
            Assert.Equal(expected: 2, actual: adaptive.MaxRejects);
            Spec.Equal(left: adaptive.Tolerance.Value, right: 1.0e-6, tolerance: 0.0, what: "adaptive tolerance");
        }));
        Spec.ForAll(FlowGens.NonAdaptive, k => Spec.Fail(FieldIntegrator.Adaptive(kind: k, tolerance: 1.0e-6, key: FlowGens.Key)));
    }
    [Fact]
    public void AdaptiveRejectsInvalidToleranceOrBudget() {
        Spec.ForAll(Gens.NonPositive, t => Spec.FailCategory(FieldIntegrator.Adaptive(kind: IntegratorKind.DormandPrince, tolerance: t, key: FlowGens.Key), category: "Tolerance"));
        Spec.ForAll(Gens.NonFinite, t => Spec.FailCategory(FieldIntegrator.Adaptive(kind: IntegratorKind.DormandPrince, tolerance: t, key: FlowGens.Key), category: "Tolerance"));
        Spec.ForAll(Gen.Int[-100, -1], m => Spec.FailCategory(FieldIntegrator.Adaptive(kind: IntegratorKind.DormandPrince, tolerance: 1.0e-6, maxRejects: m, key: FlowGens.Key), category: "Input"));
        Spec.ForAll(FlowGens.NonAdaptive, k => Spec.FailCategory(FieldIntegrator.Adaptive(kind: k, tolerance: 1.0e-6, maxRejects: 3, key: FlowGens.Key), category: "Unsupported"));
        Spec.ForAll(FlowGens.NonAdaptive, k => Spec.FailCategory(FieldIntegrator.Adaptive(kind: k, tolerance: 1.0e-6, maxRejects: -1, key: FlowGens.Key), category: "Input"));
        PositiveMagnitude tolerance = Spec.SuccValue(FlowGens.Key.AcceptValidated<PositiveMagnitude>(candidate: 1.0e-6), label: "adaptive tolerance");
        Spec.FailCategory(new FieldIntegrator.FixedCase(kind: IntegratorKind.DormandPrince).Admit(key: FlowGens.Key), category: "Unsupported");
        Spec.FailCategory(new FieldIntegrator.AdaptiveCase(kind: IntegratorKind.RK4, tolerance: tolerance, maxRejects: 1).Admit(key: FlowGens.Key), category: "Unsupported");
        Spec.FailCategory(new FieldIntegrator.AdaptiveCase(kind: IntegratorKind.DormandPrince, tolerance: tolerance, maxRejects: -1).Admit(key: FlowGens.Key), category: "Input");
    }
    [Fact]
    public void FixedAndTraceReceiptsExposeBoundedStops() {
        Assert.Equal(expected: 0, actual: Spec.SuccValue(FieldIntegrator.Fixed(kind: IntegratorKind.RK4, key: FlowGens.Key), label: "fixed rk4").RejectBudget);
        Spec.ForAll(FlowGens.NonAdaptive, k => Spec.Succ(FieldIntegrator.Fixed(kind: k, key: FlowGens.Key), then: fixedIntegrator => Assert.IsType<FieldIntegrator.FixedCase>(@object: fixedIntegrator)));
        Spec.ForAll(FlowGens.Adaptive, k => Spec.FailCategory(FieldIntegrator.Fixed(kind: k, key: FlowGens.Key), category: "Unsupported"));
        StreamlineStopKind[] stops = [StreamlineStopKind.Terminated, StreamlineStopKind.RejectBudgetExhausted, StreamlineStopKind.MaxIterationsExhausted];
        Spec.SmartEnumKeysUnique(items: stops, key: static s => s.Key);
        Assert.True(FlowGens.Trace(stop: StreamlineStopKind.Terminated).IsComplete);
        Assert.False(FlowGens.Trace(stop: StreamlineStopKind.MaxIterationsExhausted).IsComplete);
        Spec.Fail(FlowKernel.ProjectTrace<Curve>(trace: FlowGens.Trace(stop: StreamlineStopKind.MaxIterationsExhausted), key: FlowGens.Key));
        Spec.Succ(FlowKernel.ProjectTrace<Seq<Point3d>>(trace: FlowGens.Trace(stop: StreamlineStopKind.MaxIterationsExhausted), key: FlowGens.Key),
            then: points => Assert.Single(collection: points));
        Spec.FailCategory(FlowKernel.ProjectTrace<double>(trace: FlowGens.Trace(stop: StreamlineStopKind.Terminated), key: FlowGens.Key), category: "Unsupported");
        Spec.FailCategory(FlowKernel.ProjectTrace<Seq<Point3d>>(trace: FlowGens.Trace(stop: StreamlineStopKind.Terminated) with { AcceptedSteps = 0 }, key: FlowGens.Key), category: "Result");
    }
    [Fact]
    public void AdaptiveRejectBudgetRecordsTheRejectThatExhaustsTheBudget() {
        StreamlineState state = FlowGens.State(trail: Seq(Point3d.Origin), current: Point3d.Origin, h: 1.0, arc: 0.0, steps: 0);
        StreamlineState exhausted = state.Reject(rejected: new StreamlineStep.RejectedCase(SuggestedStep: 0.5, Error: Some(2.0)), rejectBudget: 1);
        Assert.Equal(expected: 1, actual: exhausted.Rejects);
        Assert.Equal(expected: 1, actual: exhausted.RejectedSteps);
        Spec.Some(exhausted.Stop, stop => Assert.Equal(expected: StreamlineStopKind.RejectBudgetExhausted, actual: stop));
    }
    [Fact]
    public void ConstantFieldTraceIsExactAndArcLengthMatchesFoldOracle() {
        PositiveMagnitude step = Spec.SuccValue(FlowGens.Key.AcceptValidated<PositiveMagnitude>(candidate: 0.25), label: "trace step");
        Termination stop = Spec.SuccValue(Termination.Steps(count: 4, key: FlowGens.Key), label: "trace stop");
        Spec.Succ(FlowKernel.Trace<StreamlineTrace>(
            source: VectorField.Constant(value: Vector3d.XAxis),
            seed: Point3d.Origin,
            initialStep: step,
            integrator: Spec.SuccValue(FieldIntegrator.Fixed(kind: IntegratorKind.RK4, key: FlowGens.Key), label: "fixed rk4"),
            termination: stop,
            context: FlowGens.Model,
            key: FlowGens.Key), then: trace => {
                Assert.Equal(expected: StreamlineStopKind.Terminated, actual: trace.Stop);
                Assert.Equal(expected: 4, actual: trace.AcceptedSteps);
                Assert.Equal(expected: 5, actual: trace.Trail.Count);
                Spec.Equal(left: trace.Trail[index: 4], right: new Point3d(x: 1.0, y: 0.0, z: 0.0), tolerance: 1.0e-12);
                Spec.Equal(left: trace.ArcLength, right: Numeric.ArcLength(points: trace.Trail), tolerance: 1.0e-12, what: "arc fold");
            });
    }
    [Fact]
    public void TraceEventMetadataIsValidatedOnTraceRail() {
        TraceEvent @event = new(
            Kind: TraceEventKind.CrossSurface,
            Status: TraceEventStatus.BracketedCrossing,
            Points: (Previous: Point3d.Origin, Current: new Point3d(x: 1.0, y: 0.0, z: 0.0), Localized: new Point3d(x: 0.5, y: 0.0, z: 0.0)),
            Values: (Previous: -1.0, Current: 1.0, Localized: 0.0),
            Parameter: 0.5,
            Tolerance: FlowGens.Model.Absolute.Value,
            Residual: 0.0,
            Iterations: 12);
        StreamlineTrace trace = FlowGens.Trace(stop: StreamlineStopKind.Terminated) with {
            TerminationPoint = @event.Points.Localized,
            Event = Some(@event),
        };
        Spec.Succ(FlowKernel.ProjectTrace<StreamlineTrace>(trace: trace, key: FlowGens.Key), then: valid =>
            Spec.Some(valid.Event, accepted => {
                Assert.Equal(expected: TraceEventKind.CrossSurface, actual: accepted.Kind);
                Assert.Equal(expected: TraceEventStatus.BracketedCrossing, actual: accepted.Status);
            }));
    }
}
