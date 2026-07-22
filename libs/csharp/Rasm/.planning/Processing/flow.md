# [RASM_VECTORS_FLOW]

`FlowKernel.Trace` advances any `VectorField` into a streamline under the `Numerics/integrate.md` adaptive stepper, deciding every stop through one `Termination` `[Union]` and localizing every crossing onto the high-order solution curve.

Every receipt validates through the `Domain/rails.md` `ValidityClaim.All` fold under page-only cross-field claims, and raw ingress gates through its acceptance bridge; `Numerics/atoms.md` `AtomProjection.Rows` resolves projection with the receipt as its implicit self row.

## [01]-[INDEX]

- [02]-[TERMINATION]: `Termination` `[Union]` stop vocabulary and the tiered localizer refining each crossing onto the dense-output curve.
- [03]-[TRACE]: `FlowKernel.Trace` folding the immutable streamline state under the numerics stepper into the typed-row projection.

## [02]-[TERMINATION]

- Owner: `Termination` `[Union]` mints one closed stop vocabulary the tracer evaluates at each accepted step; `TraceEventKind`, `TraceEventStatus`, and `TraceEventLocalizationKind` stay three orthogonal receipt vocabularies, never one merged flag.
- Entry: `Termination.Evaluate` folds a total generated `Switch` — scalar and loop stops decide from state alone, an event stop samples its signed value function and runs the localizer.
- Auto: localization refines a bracketed sign change through `DenseOutputSpan.PointAt` onto the high-order curve and falls to the chord where the segment carries no dense span; an endpoint inside tolerance short-circuits to a zero-iteration touch. Every tolerance derives from `Context`, never a bare literal.
- Boundary: `CrossSurfaceCase` admits any constructed `SupportSpace` on the `Spatial/support.md` `Of` gate's closest-capability proof, re-checking signed-distance capability per hit and raising a typed `Unsupported` fault naming the source type. Factories admit raw doubles through `Op.AcceptValidated<PositiveMagnitude>`; a non-positive magnitude is a typed `InvalidInput` fault, never a clamp.

## [03]-[TRACE]

- Owner: `TracePolicy` carries the iteration ceiling and localization budget as policy rows, never compiled-in constants; `SpatialIntegration.Module` is the one `IntegrationModule<Point3d, Vector3d>` instance `integrate.md` assigns this consumer; `StreamlineState.Accept`/`Reject` are the immutable fold state's only transitions.
- Entry: `FlowKernel.Trace<TOut>` is the one trace entrypoint, `TOut` discriminating the projection.
- Auto: `PolylineOf` substitutes the localized event point for the final trail vertex, so emitted geometry ends exactly at the crossing.
- Packages: `Rasm`/Numerics (`FieldIntegrator`/`IntegrationModule`/`IntegrationStep`/`DenseOutputSpan`/`DenseOutputReceipt` stepper floor; `AtomProjection`/`ProjectionRow`; `Dimension`/`PositiveMagnitude`), `Rasm`/Spatial (`SupportSpace`; `ScalarField.SampleScalar`), `Rasm`/Domain (`Op`/`Context`/`Admit`/`ValidityClaim`), LanguageExt.Core (`Fin`/`Option`/`Seq`/`Atom`/`IO`/`Schedule`), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<int>]`), RhinoCommon (`Point3d`/`Vector3d`/`Polyline`/`Curve.ToPolylineCurve`).
- Growth: a new stop condition is one `Termination` case and one `Evaluate` arm, the generated `Switch` breaking every dispatch site loudly; a new event source is one `TraceEventKind` row over the same localizer; a new output shape is one `ProjectionRow`; a bidirectional or multi-seed trace folds over this same entry, never a sibling tracer.
- Boundary: every failure routes a typed fault, keeping the tracer total over the `Fin` rail. One `Atom` cell holds the immutable loop state as the sole boundary state seam under idempotent `Swap` transitions, and `Polyline`/`Curve` project only a `Terminated` trace, so a budget-exhausted trail never masquerades as a completed streamline.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Rasm.Csp;
using LanguageExt;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;
// CS0104 guard: Rhino.Geometry declares Matrix/Dimension homonyms under the dual usings.
using Dimension = Rasm.Numerics.Dimension;

namespace Rasm.Processing;

// --- [TYPES] ----------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class StreamlineStopKind {
    public static readonly StreamlineStopKind Terminated = new(key: 0);
    public static readonly StreamlineStopKind RejectBudgetExhausted = new(key: 1);
    public static readonly StreamlineStopKind MaxIterationsExhausted = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class TraceEventKind {
    public static readonly TraceEventKind CrossSurface = new(key: 0);
    public static readonly TraceEventKind RegionThresholdCrossing = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class TraceEventStatus {
    public static readonly TraceEventStatus InitialEndpointTouch = new(key: 0);
    public static readonly TraceEventStatus PreviousEndpointTouch = new(key: 1);
    public static readonly TraceEventStatus CurrentEndpointTouch = new(key: 2);
    public static readonly TraceEventStatus BracketedCrossing = new(key: 3);
}

[SmartEnum<int>]
public sealed partial class TraceEventLocalizationKind {
    public static readonly TraceEventLocalizationKind BoundedBisection = new(key: 0);
    public static readonly TraceEventLocalizationKind DenseOutputRoot = new(key: 1);
}

[Union]
public abstract partial record Termination {
    public sealed record StepCountCase(Dimension Count) : Termination;
    public sealed record ArcLengthCase(PositiveMagnitude Length) : Termination;
    public sealed record MagnitudeFloorCase(PositiveMagnitude Threshold) : Termination;
    public sealed record CrossSurfaceCase(SupportSpace Surface, Dimension LocalizationBudget) : Termination;
    public sealed record RegionThresholdCase(ScalarField Region, double Threshold, Dimension LocalizationBudget) : Termination;
    public sealed record LoopDetectedCase(PositiveMagnitude ClosureRadius) : Termination;
    private Termination() { }

    public static Fin<Termination> Steps(int count, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<Dimension>(candidate: count).Bind(steps => new StepCountCase(Count: steps).Admit(key: op));
    }
    public static Fin<Termination> ArcLength(double length, Op? key = null) =>
        Positive(candidate: length, create: static value => new ArcLengthCase(Length: value), key: key);
    public static Fin<Termination> Magnitude(double threshold, Op? key = null) =>
        Positive(candidate: threshold, create: static value => new MagnitudeFloorCase(Threshold: value), key: key);
    public static Fin<Termination> CrossSurface(SupportSpace surface, Option<Dimension> localizationBudget = default, Op? key = null) {
        Op op = key.OrDefault();
        return new CrossSurfaceCase(Surface: surface, LocalizationBudget: localizationBudget.IfNone(TracePolicy.Default.LocalizationBudget)).Admit(key: op);
    }
    public static Fin<Termination> RegionThreshold(ScalarField region, double threshold, Option<Dimension> localizationBudget = default, Op? key = null) {
        Op op = key.OrDefault();
        return new RegionThresholdCase(Region: region, Threshold: threshold, LocalizationBudget: localizationBudget.IfNone(TracePolicy.Default.LocalizationBudget)).Admit(key: op);
    }
    public static Fin<Termination> LoopDetected(double closureRadius, Op? key = null) =>
        Positive(candidate: closureRadius, create: static value => new LoopDetectedCase(ClosureRadius: value), key: key);

    // Admit shadows the Rasm.Domain.Admit class inside this type, so Op.Need/Finite are the gates reachable here.
    internal Fin<Termination> Admit(Op key) => Switch(
        state: key,
        stepCountCase: static (_, termination) => Fin.Succ<Termination>(termination),
        arcLengthCase: static (_, termination) => Fin.Succ<Termination>(termination),
        magnitudeFloorCase: static (_, termination) => Fin.Succ<Termination>(termination),
        crossSurfaceCase: static (op, termination) =>
            op.Need(termination.Surface).Map(static _ => (Termination)termination),
        regionThresholdCase: static (op, termination) =>
            from region in op.Need(termination.Region)
            from threshold in op.Finite(termination.Threshold)
            select (Termination)termination,
        loopDetectedCase: static (_, termination) => Fin.Succ<Termination>(termination));
    internal static Fin<Termination> Admit(Termination value, Op key) =>
        key.Need(value).Bind(termination => termination.Admit(key: key));

    internal Fin<(bool Stop, Option<TraceEvent> Event)> Evaluate(StreamlineState state, Vector3d currentSample, Context context, Op key) => Switch(
        state: (Field: state, Sample: currentSample, Context: context, Key: key),
        stepCountCase: static (s, c) => Decision(stop: s.Field.Steps >= c.Count.Value),
        arcLengthCase: static (s, c) => Decision(stop: s.Field.Arc >= c.Length.Value),
        magnitudeFloorCase: static (s, c) => Decision(stop: s.Sample.Length < c.Threshold.Value),
        loopDetectedCase: static (s, c) => Decision(stop: ClosureDetected(state: s.Field, radius: c.ClosureRadius.Value)),
        crossSurfaceCase: static (s, c) => EvaluateEvent(
            state: s.Field, kind: TraceEventKind.CrossSurface, tolerance: s.Context.Absolute.Value, budget: c.LocalizationBudget.Value,
            sample: point =>
                from hit in c.Surface.Closest(sample: point, key: s.Key)
                from value in c.Surface.AdmitsSignedDistance(hit: hit)
                    ? c.Surface.SignedDistance(hit: hit, sample: point, key: s.Key)
                    : Fin.Fail<double>(s.Key.Unsupported(geometryType: c.Surface.SourceType, outputType: typeof(double)))
                select value,
            key: s.Key).Map(@event => (Stop: @event.IsSome, Event: @event)),
        regionThresholdCase: static (s, c) => EvaluateEvent(
            state: s.Field, kind: TraceEventKind.RegionThresholdCrossing,
            tolerance: s.Context.Fractional * Math.Max(1.0, Math.Abs(value: c.Threshold)), budget: c.LocalizationBudget.Value,
            sample: point => c.Region.SampleScalar(sample: point, context: s.Context, key: s.Key).Map(value => value - c.Threshold),
            key: s.Key).Map(@event => (Stop: @event.IsSome, Event: @event)));

    private static Fin<Termination> Positive(double candidate, Func<PositiveMagnitude, Termination> create, Op? key) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: candidate).Bind(value => create(value).Admit(key: op));
    }
    private static Fin<(bool Stop, Option<TraceEvent> Event)> Decision(bool stop) =>
        Fin.Succ((Stop: stop, Event: Option<TraceEvent>.None));

    private static Fin<Option<TraceEvent>> EvaluateEvent(StreamlineState state, TraceEventKind kind, double tolerance, int budget, Func<Point3d, Fin<double>> sample, Op key) =>
        from currentValue in sample(state.Current)
        from output in state.Trail.Count < 2
            ? EndpointEvent(kind: kind, status: TraceEventStatus.InitialEndpointTouch, points: (state.Current, state.Current, state.Current), values: (currentValue, currentValue, currentValue), parameter: 0.0, tolerance: tolerance)
            : SegmentEvent(previous: state.Trail[state.Trail.Count - 2], current: state.Current, dense: state.Dense, currentValue: currentValue, kind: kind, tolerance: tolerance, budget: budget, sample: sample, key: key)
        select output;
    private static Fin<Option<TraceEvent>> SegmentEvent(Point3d previous, Point3d current, Option<DenseOutputSpan<Point3d, Vector3d>> dense, double currentValue, TraceEventKind kind, double tolerance, int budget, Func<Point3d, Fin<double>> sample, Op key) =>
        from previousValue in sample(previous)
        from output in Math.Abs(value: previousValue) <= tolerance
            ? EndpointEvent(kind: kind, status: TraceEventStatus.PreviousEndpointTouch, points: (previous, current, previous), values: (previousValue, currentValue, previousValue), parameter: 0.0, tolerance: tolerance)
            : Math.Abs(value: currentValue) <= tolerance
                ? EndpointEvent(kind: kind, status: TraceEventStatus.CurrentEndpointTouch, points: (previous, current, current), values: (previousValue, currentValue, currentValue), parameter: 1.0, tolerance: tolerance)
                : previousValue * currentValue < 0.0
                    ? LocateRoot(previous: previous, current: current, dense: dense, previousValue: previousValue, currentValue: currentValue, kind: kind, tolerance: tolerance, budget: budget, sample: sample, key: key).Map(Some)
                    : Fin.Succ(Option<TraceEvent>.None)
        select output;
    private static Fin<Option<TraceEvent>> EndpointEvent(TraceEventKind kind, TraceEventStatus status, (Point3d Previous, Point3d Current, Point3d Localized) points, (double Previous, double Current, double Localized) values, double parameter, double tolerance) =>
        Math.Abs(value: values.Localized) <= tolerance
            ? Fin.Succ(Some(new TraceEvent(
                Kind: kind, Status: status, Points: points, Values: values, Parameter: parameter, Tolerance: tolerance,
                Residual: Math.Abs(value: values.Localized), Iterations: 0,
                LocalizationKind: TraceEventLocalizationKind.BoundedBisection, DenseOutput: Option<DenseOutputReceipt>.None)))
            : Fin.Succ(Option<TraceEvent>.None);
    // Each midpoint samples through the Fin-railed callback, so the bracket refines on-rail where a total-Func root-finder cannot.
    private static Fin<TraceEvent> LocateRoot(Point3d previous, Point3d current, Option<DenseOutputSpan<Point3d, Vector3d>> dense, double previousValue, double currentValue, TraceEventKind kind, double tolerance, int budget, Func<Point3d, Fin<double>> sample, Op key) =>
        from bracket in toSeq(Enumerable.Range(start: 0, count: budget)).Fold(
            initialState: Fin.Succ((A: previous, B: current, FA: previousValue, FB: currentValue, TA: 0.0, TB: 1.0, Localized: previous, FLocalized: previousValue, TLocalized: 0.0, Done: false, Iterations: 0)),
            f: (acc, _) => acc.Bind(state => state.Done
                ? Fin.Succ(state)
                : PointAt(previous: previous, current: current, dense: dense, theta: 0.5 * (state.TA + state.TB), key: key).Bind(mid =>
                    sample(mid).Map(fm => {
                        double tm = 0.5 * (state.TA + state.TB);
                        bool localized = Math.Abs(value: fm) <= tolerance || mid.DistanceTo(other: state.A) <= tolerance || mid.DistanceTo(other: state.B) <= tolerance;
                        return state.FA * fm <= 0.0
                            ? (state.A, mid, state.FA, fm, state.TA, tm, mid, fm, tm, localized, state.Iterations + 1)
                            : (mid, state.B, fm, state.FB, tm, state.TB, mid, fm, tm, localized, state.Iterations + 1);
                    }))))
        from localized in bracket.Done ? Fin.Succ(bracket.Localized) : PointAt(previous: previous, current: current, dense: dense, theta: 0.5 * (bracket.TA + bracket.TB), key: key)
        from residual in bracket.Done ? Fin.Succ(bracket.FLocalized) : sample(localized)
        let parameter = bracket.Done ? bracket.TLocalized : 0.5 * (bracket.TA + bracket.TB)
        from @event in Math.Abs(value: residual) <= tolerance || localized.DistanceTo(other: bracket.A) <= tolerance || localized.DistanceTo(other: bracket.B) <= tolerance
            ? Fin.Succ(new TraceEvent(
                Kind: kind, Status: TraceEventStatus.BracketedCrossing, Points: (previous, current, localized),
                Values: (previousValue, currentValue, residual), Parameter: parameter, Tolerance: tolerance,
                Residual: Math.Abs(value: residual), Iterations: bracket.Iterations,
                LocalizationKind: dense.Map(static _ => TraceEventLocalizationKind.DenseOutputRoot).IfNone(TraceEventLocalizationKind.BoundedBisection),
                DenseOutput: dense.Map(static span => span.Receipt)))
            : Fin.Fail<TraceEvent>(key.InvalidResult())
        select @event;
    private static Fin<Point3d> PointAt(Point3d previous, Point3d current, Option<DenseOutputSpan<Point3d, Vector3d>> dense, double theta, Op key) =>
        dense.Match(
            Some: span => span.PointAt(theta: theta, key: key),
            None: () => key.AcceptValue(value: previous + (theta * (current - previous))));
    private static bool ClosureDetected(StreamlineState state, double radius) =>
        state.Trail.Count >= 3
        && toSeq(Enumerable.Range(start: 0, count: state.Trail.Count - 2))
            .Exists(i => state.Current.DistanceToSquared(other: state.Trail[i]) <= radius * radius);
}

// --- [CONSTANTS] ------------------------------------------------------------------------------
public sealed record TracePolicy(Dimension MaxIterations, Dimension LocalizationBudget) {
    public static readonly TracePolicy Default = new(
        MaxIterations: Dimension.Create(value: 100_000),
        LocalizationBudget: Dimension.Create(value: 64));
}

internal static class SpatialIntegration {
    internal static readonly IntegrationModule<Point3d, Vector3d> Module = new(
        Add: static (state, h, delta) => state + (h * delta),
        Scale: static (factor, delta) => factor * delta,
        Sum: static (left, right) => left + right,
        Norm: static delta => delta.Length,
        Zero: Vector3d.Zero);
}

// --- [MODELS] ---------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TraceEvent(
    TraceEventKind Kind, TraceEventStatus Status,
    (Point3d Previous, Point3d Current, Point3d Localized) Points, (double Previous, double Current, double Localized) Values,
    double Parameter, double Tolerance, double Residual, int Iterations,
    TraceEventLocalizationKind LocalizationKind, Option<DenseOutputReceipt> DenseOutput) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Finite(Points.Previous), ValidityClaim.Finite(Points.Current), ValidityClaim.Finite(Points.Localized),
        ValidityClaim.Finite(Values.Previous), ValidityClaim.Finite(Values.Current), ValidityClaim.Finite(Values.Localized),
        ValidityClaim.UnitInterval(Parameter), ValidityClaim.Nonnegative(Tolerance), ValidityClaim.Nonnegative(Residual),
        ValidityClaim.CountAtLeast(Iterations, 0),
        ValidityClaim.Of(Math.Abs(value: Residual - Math.Abs(value: Values.Localized)) <= EpsilonPolicy.ZeroTolerance),
        ValidityClaim.Of(!LocalizationKind.Equals(TraceEventLocalizationKind.DenseOutputRoot) || DenseOutput.IsSome),
        ValidityClaim.Of(DenseOutput.Map(static receipt => receipt.IsValid).IfNone(noneValue: true)));
    internal bool IsValidFor(Point3d terminationPoint) =>
        IsValid && terminationPoint.DistanceTo(other: Points.Localized) <= Tolerance;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct StreamlineTrace(
    Seq<Point3d> Trail, StreamlineStopKind Stop, int AcceptedSteps, int RejectedSteps, double ArcLength,
    double FinalStep, int MethodOrder, Option<int> EmbeddedOrder, Option<double> LastError, double MaxError,
    double MinStep, double MaxStep, Point3d TerminationPoint, Option<TraceEvent> Event) : IValidityEvidence {
    public bool IsComplete => Stop.Equals(StreamlineStopKind.Terminated);
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(Trail.Count, 1),
        ValidityClaim.Of(Trail.ForAll(static point => point.IsValid)),
        ValidityClaim.Finite(TerminationPoint),
        ValidityClaim.Nonnegative(ArcLength), ValidityClaim.Finite(FinalStep),
        ValidityClaim.Nonnegative(MaxError), ValidityClaim.Ordered(MinStep, MaxStep),
        ValidityClaim.CountAtLeast(RejectedSteps, 0), ValidityClaim.CountAtLeast(MethodOrder, 1),
        ValidityClaim.Of(EmbeddedOrder.Map(order => order > 0 && order < MethodOrder).IfNone(noneValue: true)),
        ValidityClaim.Of(LastError.Map(double.IsFinite).IfNone(noneValue: true)),
        ValidityClaim.CountExactly(AcceptedSteps, Trail.Count - 1),
        ValidityClaim.Of(Event.Map(@event => @event.IsValidFor(terminationPoint: TerminationPoint)).IfNone(noneValue: true)));
}

// --- [OPERATIONS] -----------------------------------------------------------------------------
internal readonly record struct StreamlineState(
    Seq<Point3d> Trail, Point3d Current, double H, double Arc, int Steps, int Rejects, int RejectedSteps,
    double MinStep, double MaxStep, Option<double> LastError, double MaxError,
    Option<DenseOutputSpan<Point3d, Vector3d>> Dense, Option<TraceEvent> Event, Option<StreamlineStopKind> Stop) {
    internal static StreamlineState Start(Point3d seed, double h) =>
        new(Trail: Seq(seed), Current: seed, H: h, Arc: 0.0, Steps: 0, Rejects: 0, RejectedSteps: 0,
            MinStep: h, MaxStep: h, LastError: Option<double>.None, MaxError: 0.0,
            Dense: Option<DenseOutputSpan<Point3d, Vector3d>>.None, Event: Option<TraceEvent>.None, Stop: Option<StreamlineStopKind>.None);
    internal StreamlineState Accept(IntegrationStep<Point3d, Vector3d>.AcceptedCase accepted) =>
        Advance(suggested: accepted.SuggestedStep, error: accepted.Error) with {
            Trail = Trail.Add(accepted.Next), Current = accepted.Next,
            Arc = Arc + accepted.Next.DistanceTo(other: Current), Steps = Steps + 1, Rejects = 0,
            Dense = Some(accepted.Dense),
        };
    internal StreamlineState Reject(IntegrationStep<Point3d, Vector3d>.RejectedCase rejected, int rejectBudget) =>
        Advance(suggested: rejected.SuggestedStep, error: rejected.Error) with {
            Rejects = Rejects + 1, RejectedSteps = RejectedSteps + 1, Dense = Option<DenseOutputSpan<Point3d, Vector3d>>.None,
            Stop = Rejects + 1 >= rejectBudget ? Some(StreamlineStopKind.RejectBudgetExhausted) : Stop,
        };
    private StreamlineState Advance(double suggested, Option<double> error) =>
        this with {
            H = suggested, MinStep = Math.Min(val1: MinStep, val2: suggested), MaxStep = Math.Max(val1: MaxStep, val2: suggested),
            LastError = error, MaxError = Math.Max(val1: MaxError, val2: error.IfNone(0.0)),
        };
}

internal static class FlowKernel {
    internal static Fin<TOut> Trace<TOut>(VectorField source, Point3d seed, PositiveMagnitude initialStep, FieldIntegrator integrator, Termination termination, Context context, Op key, Option<TracePolicy> policy = default) =>
        from activeIntegrator in FieldIntegrator.Admit(value: integrator, key: key)
        from activeTermination in Termination.Admit(value: termination, key: key)
        from validSeed in key.AcceptValue(value: seed)
        from state in TraceState(source: source, seed: validSeed, initialStep: initialStep, integrator: activeIntegrator, termination: activeTermination, policy: policy.IfNone(TracePolicy.Default), context: context, key: key)
        let trace = ToTrace(state: state, integrator: activeIntegrator)
        from output in ProjectTrace<TOut>(trace: trace, key: key)
        select output;

    internal static Fin<TOut> ProjectTrace<TOut>(StreamlineTrace trace, Op key) =>
        from valid in trace.IsValid ? Fin.Succ(trace) : Fin.Fail<StreamlineTrace>(error: key.InvalidResult())
        from output in AtomProjection.Rows<StreamlineTrace, TOut>(self: valid, key: key,
            ProjectionRow.Of<Seq<Point3d>>(() => valid.Trail.TraverseM(point => key.AcceptValue(value: point)).As()),
            ProjectionRow.Of<Polyline>(() => valid.IsComplete ? PolylineOf(trace: valid, key: key) : Fin.Fail<Polyline>(key.InvalidResult())),
            ProjectionRow.Of<Curve>(() => valid.IsComplete
                ? PolylineOf(trace: valid, key: key).Bind(polyline => Optional(polyline.ToPolylineCurve()).ToFin(key.InvalidResult()).Map(static curve => (Curve)curve))
                : Fin.Fail<Curve>(key.InvalidResult())))
        select output;

    private static Fin<StreamlineState> TraceState(VectorField source, Point3d seed, PositiveMagnitude initialStep, FieldIntegrator integrator, Termination termination, TracePolicy policy, Context context, Op key) {
        Atom<Fin<StreamlineState>> cell = Atom(value: Fin.Succ(StreamlineState.Start(seed: seed, h: initialStep.Value)));
        _ = IO.lift(() => { _ = cell.Swap(f: state => state.Bind(active => active.Stop.IsSome ? Fin.Succ(active) : AdvanceState(state: active, source: source, integrator: integrator, termination: termination, context: context, key: key))); })
            .RepeatWhile(
                schedule: Schedule.recurs(times: policy.MaxIterations.Value),
                predicate: _ => cell.Value.Match(Succ: static active => active.Stop.IsNone, Fail: static _ => false))
            .Run();
        return cell.Value;
    }
    private static Fin<StreamlineState> AdvanceState(StreamlineState state, VectorField source, FieldIntegrator integrator, Termination termination, Context context, Op key) =>
        from vector in source.SampleVector(sample: state.Current, context: context, key: key)
        from decision in termination.Evaluate(state: state, currentSample: vector, context: context, key: key)
        from next in decision.Stop
            ? Fin.Succ(state with { Event = decision.Event, Stop = Some(StreamlineStopKind.Terminated) })
            : integrator.Step(
                    module: SpatialIntegration.Module,
                    sample: point => source.SampleVector(sample: point, context: context, key: key),
                    state: state.Current, h: state.H, key: key)
                .Map(step => step.Switch(
                    state: (State: state, Budget: integrator.RejectBudget),
                    acceptedCase: static (s, accepted) => s.State.Accept(accepted: accepted),
                    rejectedCase: static (s, rejected) => s.State.Reject(rejected: rejected, rejectBudget: s.Budget)))
        select next;
    private static StreamlineTrace ToTrace(StreamlineState state, FieldIntegrator integrator) =>
        new(Trail: state.Trail, Stop: state.Stop.IfNone(StreamlineStopKind.MaxIterationsExhausted),
            AcceptedSteps: state.Steps, RejectedSteps: state.RejectedSteps, ArcLength: state.Arc, FinalStep: state.H,
            MethodOrder: integrator.MethodOrder, EmbeddedOrder: integrator.EmbeddedOrder,
            LastError: state.LastError, MaxError: state.MaxError, MinStep: state.MinStep, MaxStep: state.MaxStep,
            TerminationPoint: state.Event.Map(static @event => @event.Points.Localized).IfNone(state.Current), Event: state.Event);
    private static Fin<Polyline> PolylineOf(StreamlineTrace trace, Op key) {
        Point3d[] points = trace.Event.Match(
            Some: @event => trace.Trail.AsIterable().Select((point, index) => index == trace.Trail.Count - 1 ? @event.Points.Localized : point).ToArray(),
            None: () => [.. trace.Trail.AsIterable()]);
        Polyline polyline = [.. points];
        return polyline.IsValid ? key.AcceptValue(value: polyline) : Fin.Fail<Polyline>(key.InvalidResult());
    }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
