# 1. Delete redundant termination factories

## From

`libs/dotnet/Rasm/.planning/Processing/flow.md:114`

```csharp
public static Fin<Termination> Steps(int count, Op? key = null) {
    Op op = key.OrDefault();
    return op.AcceptValidated<Dimension>(candidate: count).Bind(steps => new StepCountCase(Count: steps).Admit(key: op));
}
public static Fin<Termination> ArcLength(double length, Op? key = null) =>
    Positive(candidate: length, create: static value => new ArcLengthCase(Length: value), key: key.OrDefault());
public static Fin<Termination> Magnitude(double threshold, Op? key = null) =>
    Positive(candidate: threshold, create: static value => new MagnitudeFloorCase(Threshold: value), key: key.OrDefault());
public static Fin<Termination> CrossSurface(SupportSpace surface, Option<Dimension> localizationBudget = default, Op? key = null) {
    Op op = key.OrDefault();
    return new CrossSurfaceCase(Surface: surface, LocalizationBudget: localizationBudget.IfNone(TracePolicy.Default.LocalizationBudget)).Admit(key: op);
}
public static Fin<Termination> RegionThreshold(ScalarField region, double threshold, Option<Dimension> localizationBudget = default, Op? key = null) {
    Op op = key.OrDefault();
    return new RegionThresholdCase(Region: region, Threshold: threshold, LocalizationBudget: localizationBudget.IfNone(TracePolicy.Default.LocalizationBudget)).Admit(key: op);
}
public static Fin<Termination> LoopDetected(double closureRadius, Op? key = null) =>
    Positive(candidate: closureRadius, create: static value => new LoopDetectedCase(ClosureRadius: value), key: key.OrDefault());
public static Fin<Termination> CriticalCapture(Seq<Point3d> sites, double captureRadius, Option<Dimension> localizationBudget = default, Op? key = null) {
    Op op = key.OrDefault();
    return op.AcceptValidated<PositiveMagnitude>(candidate: captureRadius).Bind(radius =>
        new CriticalCaptureCase(Sites: sites, CaptureRadius: radius,
            LocalizationBudget: localizationBudget.IfNone(TracePolicy.Default.LocalizationBudget)).Admit(key: op));
}
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:188`

```csharp
private static Fin<Termination> Positive(double candidate, Func<PositiveMagnitude, Termination> create, Op key) =>
    key.AcceptValidated<PositiveMagnitude>(candidate: candidate).Bind(value => create(value).Admit(key: key));
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:489`

```csharp
from horizon in Termination.Steps(count: policy.TransitionSteps.Value, key: key)
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:600`

```csharp
from capture in Termination.CriticalCapture(sites: sites, captureRadius: policy.CaptureRadius.Value, key: key)
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:163`

```csharp
loopDetectedCase: static (s, c) => Decision(stop: ClosureDetected(state: s.Field, radius: c.ClosureRadius.Value)),
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:263`

```csharp
private static bool ClosureDetected(StreamlineState state, double radius) =>
    state.Trail.Count >= 3
    && toSeq(Enumerable.Range(start: 0, count: state.Trail.Count - 2))
        .Exists(i => state.Current.DistanceToSquared(other: state.Trail[i]) <= radius * radius);
```

## To

```csharp
// Steps DELETED
// ArcLength DELETED
// Magnitude DELETED
// CrossSurface DELETED
// RegionThreshold DELETED
// LoopDetected DELETED
// CriticalCapture DELETED
// Positive DELETED
```

```csharp
let horizon = (Termination)new Termination.StepCountCase(policy.TransitionSteps)
```

```csharp
let capture = (Termination)new Termination.CriticalCaptureCase(
    sites, policy.CaptureRadius, TracePolicy.Default.LocalizationBudget)
```

```csharp
loopDetectedCase: static (s, c) => Decision(
    stop: s.Field.Trail.Count >= 3
        && toSeq(Enumerable.Range(0, s.Field.Trail.Count - 2))
            .Exists(i => s.Field.Current.DistanceToSquared(s.Field.Trail[i])
                <= c.ClosureRadius.Value * c.ClosureRadius.Value)),
```

```csharp
// ClosureDetected DELETED
```

## Why

The seven factories duplicate the admission already owned by `VectorIntent.Streamline` and `SampledExtraction.StreamBundle`, expose raw primitive overloads beside the admitted union cases, and have no consumers beyond two same-file atlas calls. Those calls already hold `Dimension` and `PositiveMagnitude`, so converting them back to primitives only to re-admit them is negative-value indirection. `ClosureDetected` is a one-call predicate whose name adds a hop without owning state, policy, or reuse.

## Change

Construct `StepCountCase` directly from `TopologyPolicy.TransitionSteps` and `CriticalCaptureCase` directly from the admitted site sequence, `TopologyPolicy.CaptureRadius`, and default localization budget. Retain `Termination.Admit` as the single raw-union boundary used by the two public request factories. Inline the closure-radius predicate in its generated `Switch` arm and delete its private forwarding member.

## Delta

Code-fence LOC: -26 net. Module surface: -9 methods, +0 methods/types; -9 members net. Union cases and stop capabilities are unchanged.

# 2. Delete derived event metadata and separate tolerance dimensions

## From

`libs/dotnet/Rasm/.planning/Processing/flow.md:72`

```csharp
[SmartEnum<int>]
public sealed partial class TraceEventKind {
    public static readonly TraceEventKind CrossSurface = new(key: 0);
    public static readonly TraceEventKind RegionThresholdCrossing = new(key: 1);
    public static readonly TraceEventKind CriticalCapture = new(key: 2);
}
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:79`

```csharp
[SmartEnum<int>]
public sealed partial class TraceEventStatus {
    public static readonly TraceEventStatus InitialEndpointTouch = new(key: 0);
    public static readonly TraceEventStatus PreviousEndpointTouch = new(key: 1);
    public static readonly TraceEventStatus CurrentEndpointTouch = new(key: 2);
    public static readonly TraceEventStatus BracketedCrossing = new(key: 3);
}
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:87`

```csharp
[SmartEnum<int>]
public sealed partial class TraceEventLocalizationKind {
    public static readonly TraceEventLocalizationKind BoundedBisection = new(key: 0);
    public static readonly TraceEventLocalizationKind DenseOutputRoot = new(key: 1);
}
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:286`

```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct TraceEvent(
    TraceEventKind Kind, TraceEventStatus Status,
    (Point3d Previous, Point3d Current, Point3d Localized) Points, (double Previous, double Current, double Localized) Values,
    double Parameter, double Tolerance, int Iterations,
    TraceEventLocalizationKind LocalizationKind, Option<DenseConditions> DenseOutput) : IValidityEvidence {
    public double Residual => Math.Abs(value: Values.Localized);
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Finite(Points.Previous), ValidityClaim.Finite(Points.Current), ValidityClaim.Finite(Points.Localized),
        ValidityClaim.Finite(Values.Previous), ValidityClaim.Finite(Values.Current), ValidityClaim.Finite(Values.Localized),
        ValidityClaim.UnitInterval(Parameter), ValidityClaim.Nonnegative(Tolerance),
        ValidityClaim.CountAtLeast(Iterations, 0),
        !LocalizationKind.Equals(TraceEventLocalizationKind.DenseOutputRoot) || DenseOutput.IsSome,
        ValidityClaim.Evidence(DenseOutput));
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:164`

```csharp
crossSurfaceCase: static (s, c) => EvaluateEvent(
    state: s.Field, kind: TraceEventKind.CrossSurface, tolerance: s.Context.Absolute.Value, budget: c.LocalizationBudget.Value,
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:173`

```csharp
regionThresholdCase: static (s, c) => EvaluateEvent(
    state: s.Field, kind: TraceEventKind.RegionThresholdCrossing,
    tolerance: s.Context.Fractional * Math.Max(1.0, Math.Abs(value: c.Threshold)), budget: c.LocalizationBudget.Value,
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:178`

```csharp
criticalCaptureCase: static (s, c) => EvaluateEvent(
    state: s.Field, kind: TraceEventKind.CriticalCapture,
    tolerance: s.Context.Absolute.Value, budget: c.LocalizationBudget.Value,
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:193`

```csharp
private static Fin<Option<TraceEvent>> EvaluateEvent(StreamlineState state, TraceEventKind kind, double tolerance, int budget, Func<Point3d, Fin<double>> sample, Op key) =>
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:199`

```csharp
private static Fin<Option<TraceEvent>> SegmentEvent(Point3d previous, Point3d current, Option<DenseOutputSpan<Point3d, Vector3d>> dense, double currentValue, TraceEventKind kind, double tolerance, int budget, Func<Point3d, Fin<double>> sample, Op key) =>
    from previousValue in sample(previous)
    from output in Math.Abs(value: previousValue) <= tolerance
        ? EndpointEvent(kind: kind, status: TraceEventStatus.PreviousEndpointTouch, points: (previous, current, previous), values: (previousValue, currentValue, previousValue), parameter: 0.0, tolerance: tolerance)
        : Math.Abs(value: currentValue) <= tolerance
            ? EndpointEvent(kind: kind, status: TraceEventStatus.CurrentEndpointTouch, points: (previous, current, current), values: (previousValue, currentValue, currentValue), parameter: 1.0, tolerance: tolerance)
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:195`

```csharp
from output in state.Trail.Count < 2
    ? EndpointEvent(kind: kind, status: TraceEventStatus.InitialEndpointTouch, points: (state.Current, state.Current, state.Current), values: (currentValue, currentValue, currentValue), parameter: 0.0, tolerance: tolerance)
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:209`

```csharp
private static Fin<Option<TraceEvent>> EndpointEvent(TraceEventKind kind, TraceEventStatus status, (Point3d Previous, Point3d Current, Point3d Localized) points, (double Previous, double Current, double Localized) values, double parameter, double tolerance) =>
    Math.Abs(value: values.Localized) <= tolerance
        ? Fin.Succ(Some(new TraceEvent(
            Kind: kind, Status: status, Points: points, Values: values, Parameter: parameter, Tolerance: tolerance,
            Iterations: 0,
            LocalizationKind: TraceEventLocalizationKind.BoundedBisection, DenseOutput: Option<DenseConditions>.None)))
        : Fin.Succ(Option<TraceEvent>.None);
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:216`

```csharp
private static Fin<TraceEvent> LocateRoot(Point3d previous, Point3d current, Option<DenseOutputSpan<Point3d, Vector3d>> dense, double previousValue, double currentValue, TraceEventKind kind, double tolerance, int budget, Func<Point3d, Fin<double>> sample, Op key) {
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:231`

```csharp
? Fin.Succ(new TraceEvent(
    Kind: kind, Status: TraceEventStatus.BracketedCrossing, Points: (previous, current, settled.At),
    Values: (previousValue, currentValue, settled.Value), Parameter: settled.T, Tolerance: tolerance,
    Iterations: bracket.Iterations,
    LocalizationKind: dense.Map(static _ => TraceEventLocalizationKind.DenseOutputRoot).IfNone(TraceEventLocalizationKind.BoundedBisection),
    DenseOutput: dense.Map(static span => span.Conditions)))
```

## To

```csharp
// TraceEventStatus DELETED
// TraceEventLocalizationKind DELETED
```

```csharp
public static readonly TraceEventKind RegionThreshold = new(key: 1);
```

```csharp
crossSurfaceCase: static (s, c) => EvaluateEvent(
    state: s.Field, kind: TraceEventKind.CrossSurface,
    tolerance: (Residual: s.Context.Absolute.Value, Position: s.Context.Absolute.Value),
    budget: c.LocalizationBudget.Value,
```

```csharp
regionThresholdCase: static (s, c) => EvaluateEvent(
    state: s.Field, kind: TraceEventKind.RegionThreshold,
    tolerance: (
        Residual: s.Context.Fractional * Math.Max(1.0, Math.Abs(c.Threshold)),
        Position: s.Context.Absolute.Value),
    budget: c.LocalizationBudget.Value,
```

```csharp
criticalCaptureCase: static (s, c) => EvaluateEvent(
    state: s.Field, kind: TraceEventKind.CriticalCapture,
    tolerance: (Residual: s.Context.Absolute.Value, Position: s.Context.Absolute.Value),
    budget: c.LocalizationBudget.Value,
```

```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct TraceEvent(
    TraceEventKind Kind,
    (Point3d Previous, Point3d Current, Point3d Localized) Points,
    (double Previous, double Current, double Localized) Values,
    double Parameter, (double Residual, double Position) Tolerance, int Iterations,
    Option<DenseConditions> DenseOutput) : IValidityEvidence {
    public double Residual => Math.Abs(value: Values.Localized);
    public bool IsValid => ValidityClaim.All(
        Kind is not null,
        ValidityClaim.Finite(Points.Previous), ValidityClaim.Finite(Points.Current), ValidityClaim.Finite(Points.Localized),
        ValidityClaim.Finite(Values.Previous), ValidityClaim.Finite(Values.Current), ValidityClaim.Finite(Values.Localized),
        ValidityClaim.UnitInterval(Parameter),
        ValidityClaim.Nonnegative(Tolerance.Residual), ValidityClaim.Nonnegative(Tolerance.Position),
        ValidityClaim.CountAtLeast(Iterations, 0), ValidityClaim.Evidence(DenseOutput));
    internal bool IsValidFor(Point3d terminationPoint) =>
        IsValid && terminationPoint.DistanceTo(Points.Localized) <= Tolerance.Position;
}
```

```csharp
private static Fin<Option<TraceEvent>> EvaluateEvent(
    StreamlineState state, TraceEventKind kind, (double Residual, double Position) tolerance,
    int budget, Func<Point3d, Fin<double>> sample, Op key) =>
```

```csharp
private static Fin<Option<TraceEvent>> SegmentEvent(
    Point3d previous, Point3d current, Option<DenseOutputSpan<Point3d, Vector3d>> dense,
    double currentValue, TraceEventKind kind, (double Residual, double Position) tolerance,
    int budget, Func<Point3d, Fin<double>> sample, Op key) =>
    from previousValue in sample(previous)
    from output in Math.Abs(previousValue) <= tolerance.Residual
        ? EndpointEvent(kind, (previous, current, previous), (previousValue, currentValue, previousValue), 0.0, tolerance)
        : Math.Abs(currentValue) <= tolerance.Residual
            ? EndpointEvent(kind, (previous, current, current), (previousValue, currentValue, currentValue), 1.0, tolerance)
```

```csharp
from output in state.Trail.Count < 2
    ? EndpointEvent(kind, (state.Current, state.Current, state.Current),
        (currentValue, currentValue, currentValue), 0.0, tolerance)
```

```csharp
private static Fin<Option<TraceEvent>> EndpointEvent(
    TraceEventKind kind,
    (Point3d Previous, Point3d Current, Point3d Localized) points,
    (double Previous, double Current, double Localized) values,
    double parameter, (double Residual, double Position) tolerance) =>
    Math.Abs(values.Localized) <= tolerance.Residual
        ? Fin.Succ(Some(new TraceEvent(
            Kind: kind, Points: points, Values: values, Parameter: parameter,
            Tolerance: tolerance, Iterations: 0, DenseOutput: Option<DenseConditions>.None)))
        : Fin.Succ(Option<TraceEvent>.None);
```

```csharp
private static Fin<TraceEvent> LocateRoot(
    Point3d previous, Point3d current, Option<DenseOutputSpan<Point3d, Vector3d>> dense,
    double previousValue, double currentValue, TraceEventKind kind,
    (double Residual, double Position) tolerance, int budget,
    Func<Point3d, Fin<double>> sample, Op key) {
```

```csharp
from @event in Math.Abs(settled.Value) <= tolerance.Residual
    || settled.At.DistanceTo(bracket.A) <= tolerance.Position
    || settled.At.DistanceTo(bracket.B) <= tolerance.Position
    ? Fin.Succ(new TraceEvent(
        Kind: kind, Points: (previous, current, settled.At),
        Values: (previousValue, currentValue, settled.Value), Parameter: settled.T,
        Tolerance: tolerance, Iterations: bracket.Iterations,
        DenseOutput: dense.Map(static span => span.Conditions)))
```

## Why

`Status` is derivable from endpoint equality and the unit parameter, while `LocalizationKind` is exactly `DenseOutput.IsSome`; both publish duplicate state. Replacing `Kind` with the whole `Termination` union would be worse: every event would retain its source object, site roster, and policy payload merely to name which event fired. The remaining localization defect is dimensional: a region-value residual tolerance is currently also compared to point distance and used to validate the termination point.

## Change

Keep `TraceEventKind` as the compact cause vocabulary, shorten `RegionThresholdCrossing` to `RegionThreshold`, and delete the status and localization vocabularies. Carry residual and position tolerances as one named tuple: event functions compare values only with `Residual`, bracket geometry and `IsValidFor` compare points only with `Position`. Remove the `status` argument from every helper and derive endpoint/bracket posture from `Points` and `Parameter` when a consumer needs it.

## Delta

Code-fence LOC: -6 net. Module surface: -2 smart-enum types, -6 static rows, -2 record fields, +0 types/members; -8 members/types net. The tolerance tuple replaces one scalar field without adding an emitted member.

# 3. Replace the shared root cell with a pure bounded fold

## From

`libs/dotnet/Rasm/.planning/Processing/flow.md:216`

```csharp
Atom<Fin<Bracket>> cell = Atom(value: Fin.Succ(new Bracket(
    A: previous, B: current, FA: previousValue, FB: currentValue, TA: 0.0, TB: 1.0,
    Localized: Option<(Point3d At, double Value, double T)>.None, Iterations: 0)));
Transition<Fin<Bracket>> driven = Cell.Converge(
    cell: cell,
    step: state => Some(state.Bind(active => active.Localized.IsSome ? Fin.Succ(active) : Halve(active))),
    settled: state => state.Match(Succ: static active => active.Localized.IsSome, Fail: static _ => true),
    budget: Dimension.Create(value: budget),
    declined: key.InvalidResult());
return driven.Current.Bind(bracket =>
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:394`

```csharp
internal readonly record struct Bracket(
    Point3d A, Point3d B, double FA, double FB, double TA, double TB,
    Option<(Point3d At, double Value, double T)> Localized, int Iterations);
```

## To

```csharp
using RootBracket = (Point3d A, Point3d B, double FA, double FB, double TA, double TB,
    Option<(Point3d At, double Value, double T)> Localized, int Iterations);
```

```csharp
Fin<RootBracket> seed = Fin.Succ((
    A: previous, B: current, FA: previousValue, FB: currentValue, TA: 0.0, TB: 1.0,
    Localized: Option<(Point3d At, double Value, double T)>.None, Iterations: 0));
Fin<RootBracket> driven = Range(0, budget).FoldUntil(
    seed,
    (state, _) => state.Bind(Halve),
    row => row.State.Match(Succ: active => active.Localized.IsSome, Fail: static _ => true));
return driven.Bind(bracket =>
```

```csharp
// Bracket DELETED
```

## Why

Root localization is frame-local, single-threaded, and bounded; wrapping it in `Atom`, CAS transitions, and `Cell.Converge` fabricates shared state. LanguageExt's bounded `Range(...).FoldUntil(...)` expresses the same stop condition over the immutable `Fin` state while the tuple removes a one-method carrier type.

## Change

Add the file-local `RootBracket` tuple alias, drive it with `Range(0, budget).FoldUntil`, stop on localization or the first `Fin` failure, and change the local `Halve`/`Midpoint` signatures to that alias. Preserve midpoint fallback after budget exhaustion and the exact dense-output/chord sampling path.

## Delta

Code-fence LOC: -3 net. Module surface: -1 record-struct type and its generated positional members, +0 runtime members/types; -1 declared type net. The file-local alias adds no emitted symbol.

# 4. Fuse the pure trace driver and project admitted state once

## From

`libs/dotnet/Rasm/.planning/Processing/flow.md:425`

```csharp
internal static Fin<TOut> Trace<TOut>(VectorField source, Point3d seed, PositiveMagnitude initialStep, RungeKuttaIntegrator integrator, Termination termination, Context context, Op key, Option<TracePolicy> policy = default) =>
    from activeIntegrator in RungeKuttaIntegrator.Admit(value: integrator, key: key)
    from activeTermination in Termination.Admit(value: termination, key: key)
    from validSeed in key.AcceptValue(value: seed)
    from state in TraceState(source: source, seed: validSeed, initialStep: initialStep, integrator: activeIntegrator, termination: activeTermination, policy: policy.IfNone(TracePolicy.Default), context: context, key: key)
    let trace = ToTrace(state: state, integrator: activeIntegrator)
    from output in ProjectTrace<TOut>(trace: trace, key: key)
    select output;
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:402`

```csharp
internal static StreamlineState Start(Point3d seed, double h) =>
    new(Trail: Seq(seed), Current: seed, H: h, Arc: 0.0, Steps: 0, Rejects: 0, RejectedSteps: 0,
        MinStep: h, MaxStep: h, History: Option<StepHistory>.None, MaxError: 0.0,
        Dense: Option<DenseOutputSpan<Point3d, Vector3d>>.None, Event: Option<TraceEvent>.None, Stop: Option<StreamlineStopKind>.None);
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:434`

```csharp
internal static Fin<TOut> ProjectTrace<TOut>(StreamlineTrace trace, Op key) =>
    from valid in trace.IsValid ? Fin.Succ(trace) : Fin.Fail<StreamlineTrace>(error: key.InvalidResult())
    from output in ResultProjection.Rows<StreamlineTrace, TOut>(self: valid, key: key,
        ProjectionRow.Of<Seq<Point3d>>(() => valid.Trail.TraverseM(point => key.AcceptValue(value: point)).As()),
        ProjectionRow.Of<Polyline>(() => valid.IsComplete ? PolylineOf(trace: valid, key: key) : Fin.Fail<Polyline>(key.InvalidResult())),
        ProjectionRow.Of<Curve>(() => valid.IsComplete
            ? PolylineOf(trace: valid, key: key).Bind(polyline => Optional(polyline.ToPolylineCurve()).ToFin(key.InvalidResult()).Map(static curve => (Curve)curve))
            : Fin.Fail<Curve>(key.InvalidResult())))
    select output;
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:444`

```csharp
private static Fin<StreamlineState> TraceState(VectorField source, Point3d seed, PositiveMagnitude initialStep, RungeKuttaIntegrator integrator, Termination termination, TracePolicy policy, Context context, Op key) {
    Atom<Fin<StreamlineState>> cell = Atom(value: Fin.Succ(StreamlineState.Start(seed: seed, h: initialStep.Value)));
    Transition<Fin<StreamlineState>> driven = Cell.Converge(
        cell: cell,
        step: state => Some(state.Bind(active => active.Stop.IsSome
            ? Fin.Succ(active)
            : AdvanceState(state: active, source: source, integrator: integrator, termination: termination, context: context, key: key))),
        settled: state => state.Match(Succ: static active => active.Stop.IsSome, Fail: static _ => true),
        budget: policy.MaxIterations,
        declined: key.InvalidResult());
    return driven.Current;
}
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:456`

```csharp
private static Fin<StreamlineState> AdvanceState(StreamlineState state, VectorField source, RungeKuttaIntegrator integrator, Termination termination, Context context, Op key) =>
    from vector in source.SampleVector(sample: state.Current, context: context, key: key)
    from decision in termination.Evaluate(state: state, currentSample: vector, context: context, key: key)
    from next in decision.Stop
        ? Fin.Succ(state with { Event = decision.Event, Stop = Some(StreamlineStopKind.Terminated) })
        : integrator.Step(
                module: SpatialIntegration.Module,
                sample: point => source.SampleVector(sample: point, context: context, key: key),
                state: state.Current, h: state.H, key: key, history: state.History)
            .Map(step => step.Switch(
                state: (State: state, Budget: integrator.RejectBudget),
                acceptedCase: static (s, accepted) => s.State.Accept(accepted: accepted),
                rejectedCase: static (s, rejected) => s.State.Reject(rejected: rejected, rejectBudget: s.Budget)))
    select next;
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:470`

```csharp
private static StreamlineTrace ToTrace(StreamlineState state, RungeKuttaIntegrator integrator) =>
    new(Trail: state.Trail, Stop: state.Stop.IfNone(StreamlineStopKind.MaxIterationsExhausted),
        RejectedSteps: state.RejectedSteps, ArcLength: state.Arc, FinalStep: state.H,
        MethodOrder: integrator.MethodOrder, EmbeddedOrder: integrator.EmbeddedOrder,
        LastError: state.History.Map(static h => h.Error), MaxError: state.MaxError, MinStep: state.MinStep, MaxStep: state.MaxStep,
        TerminationPoint: state.Event.Map(static @event => @event.Points.Localized).IfNone(state.Current), Event: state.Event);
```

## To

```csharp
internal static Fin<TOut> Trace<TOut>(
    VectorField source, Point3d seed, PositiveMagnitude initialStep,
    RungeKuttaIntegrator integrator, Termination termination, Context context, Op key,
    Option<TracePolicy> policy = default) =>
    from state in Range(0, policy.IfNone(TracePolicy.Default).MaxIterations.Value).FoldUntil(
        Fin.Succ(new StreamlineState(
            Trail: Seq(seed), Current: seed, H: initialStep.Value,
            Arc: 0.0, Steps: 0, Rejects: 0, RejectedSteps: 0,
            MinStep: initialStep.Value, MaxStep: initialStep.Value,
            History: Option<StepHistory>.None, MaxError: 0.0,
            Dense: Option<DenseOutputSpan<Point3d, Vector3d>>.None,
            Event: Option<TraceEvent>.None, Stop: Option<StreamlineStopKind>.None)),
        (run, _) => run.Bind(current =>
            from vector in source.SampleVector(sample: current.Current, context: context, key: key)
            from decision in termination.Evaluate(state: current, currentSample: vector, context: context, key: key)
            from next in decision.Stop
                ? Fin.Succ(current with { Event = decision.Event, Stop = Some(StreamlineStopKind.Terminated) })
                : integrator.Step(
                        module: SpatialIntegration.Module,
                        sample: point => source.SampleVector(sample: point, context: context, key: key),
                        state: current.Current, h: current.H, key: key, history: current.History)
                    .Map(step => step.Switch(
                        state: (State: current, Budget: integrator.RejectBudget),
                        acceptedCase: static (s, accepted) => s.State.Accept(accepted: accepted),
                        rejectedCase: static (s, rejected) => s.State.Reject(rejected: rejected, rejectBudget: s.Budget)))
            select next),
        static row => row.State.Match(
            Succ: static current => current.Stop.IsSome,
            Fail: static _ => true))
    let trace = new StreamlineTrace(
        Trail: state.Trail,
        Stop: state.Stop.IfNone(StreamlineStopKind.MaxIterationsExhausted),
        RejectedSteps: state.RejectedSteps, ArcLength: state.Arc, FinalStep: state.H,
        MethodOrder: integrator.MethodOrder, EmbeddedOrder: integrator.EmbeddedOrder,
        LastError: state.History.Map(static h => h.Error), MaxError: state.MaxError,
        MinStep: state.MinStep, MaxStep: state.MaxStep,
        TerminationPoint: state.Event.Map(static e => e.Points.Localized).IfNone(state.Current),
        Event: state.Event)
    from valid in trace.IsValid ? Fin.Succ(trace) : Fin.Fail<StreamlineTrace>(key.InvalidResult())
    from output in ProjectTrace<TOut>(valid, key)
    select output;
```

```csharp
internal static Fin<TOut> ProjectTrace<TOut>(StreamlineTrace trace, Op key) =>
    ResultProjection.Rows<StreamlineTrace, TOut>(self: trace, key: key,
        ProjectionRow.Of<Seq<Point3d>>(() => Fin.Succ(trace.Trail)),
        ProjectionRow.Of<Polyline>(() => trace.IsComplete ? PolylineOf(trace, key) : Fin.Fail<Polyline>(key.InvalidResult())),
        ProjectionRow.Of<Curve>(() => trace.IsComplete
            ? PolylineOf(trace, key).Bind(polyline => Optional(polyline.ToPolylineCurve()).ToFin(key.InvalidResult()).Map(static curve => (Curve)curve))
            : Fin.Fail<Curve>(key.InvalidResult())));
```

```csharp
// Start DELETED
// TraceState DELETED
// AdvanceState DELETED
// ToTrace DELETED
```

## Why

Every repository entry to `FlowKernel.Trace` receives an admitted integrator, termination, and seed from `VectorIntent`, `SampledExtraction`, or the atlas, so its three ingress binds repeat boundary work. The run is frame-local and single-threaded; `Atom` and `Cell.Converge` fabricate shared CAS state. `Start`, `TraceState`, `AdvanceState`, and `ToTrace` each have one caller and merely split one bounded driver across module members. Result admission belongs at the mint, while `ProjectTrace` currently combines that gate with a per-point traversal that repeats `StreamlineTrace.IsValid`'s finite-point claim.

## Change

Fold `Fin<StreamlineState>` directly inside `Trace` with `Range(...).FoldUntil`, placing the integration step in the fold body and stopping on a terminal state or first failure. Inline initial-state and result construction, lower an exhausted range to `MaxIterationsExhausted`, validate the completed carrier once, and let the reusable `ProjectTrace` accept only traces minted by `Trace`. Return the admitted trail directly while preserving the completed-trace gate for geometry outputs.

## Delta

Code-fence LOC: -7 net. Module surface: -4 methods (`Start`, `TraceState`, `AdvanceState`, `ToTrace`), +0 methods/types; -4 members net. Two `Atom` instances across the page, two `Transition` values, three duplicate ingress binds, one per-point validation traversal, and the trace-driver CAS path are removed after task 3 lands.

## Ripples

`libs/dotnet/Rasm/.planning/Processing/intent.md:152` and `libs/dotnet/Rasm/.planning/Processing/extract.md:437` remain the admission owners. `libs/dotnet/Rasm/.planning/Processing/extract.md:554` calls `ProjectTrace` only over traces returned by `Trace`; retain that admitted-carrier precondition. Replace the `FlowKernel.TraceState` owner reference in `libs/dotnet/Rasm/.planning/Numerics/integrate.md:5,504` with `FlowKernel.Trace`, and remove `Cell.Converge`, `Transition`, and `Atom` from the target's prose/package roster after tasks 3 and 4 delete their last uses.

# 5. Normalize partition indices and fuse transition extraction

## From

`libs/dotnet/Rasm/.planning/Processing/flow.md:342`

```csharp
public sealed record FlowPartition(Dimension Cells, Func<int, Point3d> Representative, Func<Point3d, Option<int>> Locate) {
    public static Fin<FlowPartition> Of(Dimension cells, Func<int, Point3d> representative, Func<Point3d, Option<int>> locate, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in guard(cells.Value > 0 && representative is not null && locate is not null, op.InvalidInput()).ToFin()
               select new FlowPartition(Cells: cells, Representative: representative, Locate: locate);
    }
}
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:489`

```csharp
from horizon in Termination.Steps(count: policy.TransitionSteps.Value, key: key)
from arcs in Ordinals(partition: partition).TraverseM(cell =>
    Visited(source: source, partition: partition, seed: partition.Representative(arg: cell), initialStep: initialStep,
            integrator: integrator, termination: horizon, context: context, key: key, policy: tracePolicy)
        .Map(visited => Transitions(cell: cell, visited: visited))).As()
    .Map(chunks => chunks.Bind(static chunk => chunk))
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:498`

```csharp
let sites = census.Map(row => partition.Representative(arg: row.Cell))
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:513`

```csharp
private static Seq<int> Ordinals(FlowPartition partition) => toSeq(Enumerable.Range(start: 0, count: partition.Cells.Value));
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:515`

```csharp
private static Fin<Seq<int>> Visited(
    VectorField source, FlowPartition partition, Point3d seed, PositiveMagnitude initialStep, RungeKuttaIntegrator integrator,
    Termination termination, Context context, Op key, Option<TracePolicy> policy) =>
    FlowKernel.Trace<Seq<Point3d>>(source: source, seed: seed, initialStep: initialStep, integrator: integrator,
            termination: termination, context: context, key: key, policy: policy)
        .Map(trail => trail.Fold((Run: Seq<int>(), Last: Option<int>.None), (state, point) =>
            partition.Locate(arg: point) is { Case: int cell } && state.Last != Some(cell)
                ? (Run: state.Run.Add(cell), Last: Some(cell))
                : state).Run);

private static Seq<SEdge<int>> Transitions(int cell, Seq<int> visited) =>
    visited.Count < 2
        ? Seq(new SEdge<int>(source: cell, target: cell))
        : visited.Map(static (to, index) => (To: to, Index: index)).Filter(static row => row.Index > 0)
            .Map(row => new SEdge<int>(source: visited[row.Index - 1], target: row.To));
```

## To

```csharp
public sealed record FlowPartition(Dimension Cells, Func<int, Point3d> Representative, Func<Point3d, Option<int>> Locate) {
    public static Fin<FlowPartition> Of(Dimension cells, Func<int, Point3d> representative, Func<Point3d, Option<int>> locate, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in guard(representative is not null && locate is not null, op.InvalidInput()).ToFin()
               select new FlowPartition(cells, representative,
                   point => locate(point).Filter(cell => cell >= 0 && cell < cells.Value));
    }
}
```

```csharp
let horizon = (Termination)new Termination.StepCountCase(policy.TransitionSteps)
from seeds in toSeq(Enumerable.Range(0, partition.Cells.Value))
    .TraverseM(cell => key.AcceptValue(partition.Representative(cell))).As()
from arcs in seeds.Map((seed, cell) => (Seed: seed, Cell: cell))
    .TraverseM(row => Transitions(source, partition, row.Cell, row.Seed, initialStep,
        integrator, horizon, context, key, tracePolicy)).As()
    .Map(static chunks => chunks.Bind(static chunk => chunk))
```

```csharp
let sites = census.Map(row => seeds[row.Cell])
```

```csharp
// Ordinals DELETED
// Visited DELETED
```

```csharp
private static Fin<Seq<SEdge<int>>> Transitions(
    VectorField source, FlowPartition partition, int origin, Point3d seed, PositiveMagnitude initialStep,
    RungeKuttaIntegrator integrator, Termination termination, Context context, Op key, Option<TracePolicy> policy) =>
    FlowKernel.Trace<Seq<Point3d>>(source, seed, initialStep, integrator, termination, context, key, policy)
        .Map(trail => trail.Fold((Cells: Seq<int>(), Last: Option<int>.None), (state, point) =>
            partition.Locate(point) is { IsSome: true, Case: int cell } && state.Last != Some(cell)
                ? (state.Cells.Add(cell), Some(cell))
                : state).Cells)
        .Map(cells => cells.Count < 2
            ? Seq(new SEdge<int>(origin, origin))
            : cells.Zip(cells.Skip(1), static (from, to) => new SEdge<int>(from, to)));
```

## Why

`Locate` currently admits out-of-range cell IDs that later index `CellComponent`, and matching only `Case` does not prove `Option` presence. The atlas also calls the representative delegate repeatedly without admitting its results, while `Ordinals` and the `Visited`→`Transitions` pair are single-use indirection.

## Change

Normalize `Locate` once at `FlowPartition.Of`, admit each representative once up front with `TraverseM`, reuse that frozen seed sequence for sites, and return edges directly from the trace fold. Use explicit `IsSome` matching and a zipped adjacent-pair map; a trace that visits fewer than two cells retains the existing self-edge behavior.

## Delta

Code-fence LOC: -2 net. Module surface: -2 methods (`Ordinals`, `Visited`), +0 methods/types; -2 members net. One repeated representative call per cell and every out-of-range downstream index path are removed.

## Ripples

Use the admitted `seeds` sequence for `MorseGraph.Site` selection instead of invoking `FlowPartition.Representative` again. In `Follow` at `flow.md:620`, the normalized `Locate` makes `component[cell]` total for every `Some`; no second bounds check is required.

# 6. Inline and correct the critical-point classifier

## From

`libs/dotnet/Rasm/.planning/Processing/flow.md:58`

```csharp
using Complex = System.Numerics.Complex;
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:575`

```csharp
from pairs in eigen.PairsIn(expected: EigenOrder.Factorization, key: key)
let tolerance = EpsilonPolicy.SqrtEpsilon * pairs.Fold(0.0, static (peak, pair) => Math.Max(val1: peak, val2: pair.Eigenvalue.Magnitude))
select (
    Kind: Signature(spectrum: pairs.Map(static pair => pair.Eigenvalue), tolerance: tolerance),
    Unstable: pairs
        .Filter(pair => pair.Eigenvalue.Real > tolerance && Math.Abs(value: pair.Eigenvalue.Imaginary) <= tolerance)
        .Map(static pair => new Vector3d(x: pair.Eigenvector[0].Real, y: pair.Eigenvector[1].Real, z: pair.Eigenvector[2].Real))
        .Filter(static direction => !direction.IsTiny())
        .Map(static direction => (1.0 / direction.Length) * direction));
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:585`

```csharp
private static FixedPointKind Signature(Seq<Complex> spectrum, double tolerance) =>
    (Unstable: spectrum.Count(value => value.Real > tolerance),
     Stable: spectrum.Count(value => value.Real < -tolerance),
     Rotating: spectrum.Exists(value => Math.Abs(value: value.Imaginary) > tolerance),
     Rank: spectrum.Count) switch {
        var row when row.Unstable + row.Stable < row.Rank => row.Rotating ? FixedPointKind.Center : FixedPointKind.Degenerate,
        (0, _, _, _) => FixedPointKind.Sink,
        (_, 0, _, _) => FixedPointKind.Source,
        _ => FixedPointKind.Saddle,
    };
```

## To

```csharp
// Complex alias DELETED
```

```csharp
from pairs in eigen.PairsIn(expected: EigenOrder.Factorization, key: key)
let tolerance = EpsilonPolicy.SqrtEpsilon * pairs.Fold(
    0.0, static (peak, pair) => Math.Max(peak, pair.Eigenvalue.Magnitude))
let spectrum = pairs.Map(static pair => pair.Eigenvalue)
let signature = (
    Unstable: spectrum.Count(value => value.Real > tolerance),
    Stable: spectrum.Count(value => value.Real < -tolerance),
    Neutral: spectrum.Count(value => Math.Abs(value.Real) <= tolerance),
    Rotating: spectrum.Exists(value => Math.Abs(value.Imaginary) > tolerance))
let kind = signature switch {
    (0, 0, _, true) => FixedPointKind.Center,
    (_, _, > 0, _) => FixedPointKind.Degenerate,
    (0, _, 0, _) => FixedPointKind.Sink,
    (_, 0, 0, _) => FixedPointKind.Source,
    _ => FixedPointKind.Saddle,
}
select (
    Kind: kind,
    Unstable: pairs
        .Filter(pair => pair.Eigenvalue.Real > tolerance && Math.Abs(pair.Eigenvalue.Imaginary) <= tolerance)
        .Map(static pair => new Vector3d(pair.Eigenvector[0].Real, pair.Eigenvector[1].Real, pair.Eigenvector[2].Real))
        .Filter(static direction => !direction.IsTiny())
        .Map(static direction => (1.0 / direction.Length) * direction));
```

```csharp
// Signature DELETED
```

## Why

The current first arm classifies any nonhyperbolic spectrum containing an imaginary eigenvalue as a center, including spectra with a positive or negative real eigenvalue. A center requires every real part to be neutral; mixed neutral and stable/unstable spectra are degenerate. `Signature` is also called once and carries no reusable owner boundary.

## Change

Compute positive, negative, and neutral real-part counts once inside `CriticalAt`, recognize a center only when every real part is neutral and the spectrum rotates, route every other signature with a neutral real part to `Degenerate`, and inline the remaining source/sink/saddle switch as total tuple patterns. Delete the one-call classifier and its now-unused `Complex` alias.

## Delta

Code-fence LOC: -2 net. Module surface: -1 method, +0 methods/types; -1 member net. The classification retains all six result rows while removing the false-center path.

# 7. Condense once and make unclassified components absent

## From

`libs/dotnet/Rasm/.planning/Processing/flow.md:46`

```csharp
using System.Collections.Generic;
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:93`

```csharp
[SmartEnum<int>]
public sealed partial class FixedPointKind {
    public static readonly FixedPointKind Transient = new(key: 0);
    public static readonly FixedPointKind Source = new(key: 1);
    public static readonly FixedPointKind Sink = new(key: 2);
    public static readonly FixedPointKind Saddle = new(key: 3);
    public static readonly FixedPointKind Center = new(key: 4);
    public static readonly FixedPointKind Degenerate = new(key: 5);
}
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:363`

```csharp
public readonly record struct MorseGraph(
    Seq<int> CellComponent, Seq<Point3d> Site, Seq<FixedPointKind> Kind,
    Seq<(int From, int To)> Arc, Seq<int> Crossing, Seq<Separatrix> Separatrices) : IValidityEvidence {
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:369`

```csharp
return ValidityClaim.All(
    ValidityClaim.CountAtLeast(nodes, 1),
    ValidityClaim.CountExactly(Kind.Count, nodes),
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:495`

```csharp
from labelled in Label(arcs: arcs, cells: partition.Cells.Value, key: key)
let census = Census(component: labelled.Component, nodes: labelled.Count,
    trapped: toHashSet(arcs.Filter(static arc => arc.Source == arc.Target).Map(static arc => arc.Source)))
let sites = census.Map(row => partition.Representative(arg: row.Cell))
from critical in census.Map((row, node) => (Row: row, Site: sites[node]))
    .TraverseM(entry => entry.Row.Recurrent
        ? CriticalAt(source: source, site: entry.Site, policy: policy, context: context, key: key)
        : Fin.Succ((Kind: FixedPointKind.Transient, Unstable: Seq<Vector3d>())))
    .As()
from contracted in Contract(graph: labelled.Graph, component: labelled.Component, key: key)
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:507`

```csharp
let atlas = new MorseGraph(
    CellComponent: labelled.Component, Site: sites, Kind: critical.Map(static row => row.Kind),
    Arc: contracted.Arc, Crossing: contracted.Crossing, Separatrices: separatrices)
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:601`

```csharp
from rows in critical.Map(static (row, node) => (Row: row, Node: node))
    .Filter(static entry => entry.Row.Kind.Equals(FixedPointKind.Saddle))
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:531`

```csharp
private static Fin<(Seq<int> Component, int Count, AdjacencyGraph<int, SEdge<int>> Graph)> Label(Seq<SEdge<int>> arcs, int cells, Op key) =>
    key.Catch(() => {
        AdjacencyGraph<int, SEdge<int>> graph = arcs.ToAdjacencyGraph<int, SEdge<int>>(allowParallelEdges: false);
        graph.AddVertexRange(Enumerable.Range(start: 0, count: cells));
        Dictionary<int, int> labels = new(capacity: cells);
        int count = graph.StronglyConnectedComponents(labels);
        return count > 0
            ? Fin.Succ((Component: toSeq(Enumerable.Range(start: 0, count: cells).Select(cell => labels[cell])), Count: count, Graph: graph))
            : Fin.Fail<(Seq<int> Component, int Count, AdjacencyGraph<int, SEdge<int>> Graph)>(key.InvalidResult());
    });

private static Seq<(int Cell, bool Recurrent)> Census(Seq<int> component, int nodes, LanguageExt.HashSet<int> trapped) {
    int[] first = new int[nodes];
    int[] size = new int[nodes];
    System.Array.Fill(array: first, value: -1);
    for (int cell = 0; cell < component.Count; cell++) {
        size[component[cell]]++;
        if (first[component[cell]] < 0) first[component[cell]] = cell;
    }
    return toSeq(first.Select((cell, node) => (Cell: cell, Recurrent: size[node] > 1 || trapped.Contains(cell))));
}

private static Fin<(Seq<(int From, int To)> Arc, Seq<int> Crossing)> Contract(AdjacencyGraph<int, SEdge<int>> graph, Seq<int> component, Op key) =>
    key.Catch(() => {
        Seq<(int From, int To, int Crossing)> rows = toSeq(graph
            .CondensateStronglyConnected<int, SEdge<int>, AdjacencyGraph<int, SEdge<int>>>().Edges
            .Select(edge => (
                From: component[edge.Source.Vertices.First()],
                To: component[edge.Target.Vertices.First()],
                Crossing: edge.Edges.Count)));
        return Fin.Succ((Arc: rows.Map(static row => (row.From, row.To)), Crossing: rows.Map(static row => row.Crossing)));
    });
```

## To

```csharp
// System.Collections.Generic import DELETED
```

```csharp
[SmartEnum<int>]
public sealed partial class FixedPointKind {
    public static readonly FixedPointKind Source = new(key: 0);
    public static readonly FixedPointKind Sink = new(key: 1);
    public static readonly FixedPointKind Saddle = new(key: 2);
    public static readonly FixedPointKind Center = new(key: 3);
    public static readonly FixedPointKind Degenerate = new(key: 4);
}
```

```csharp
public readonly record struct MorseGraph(
    Seq<int> CellComponent, Seq<Point3d> Site, Seq<Option<FixedPointKind>> Critical,
    Seq<(int From, int To)> Arc, Seq<int> Crossing, Seq<Separatrix> Separatrices) : IValidityEvidence {
```

```csharp
return ValidityClaim.All(
    ValidityClaim.CountAtLeast(nodes, 1),
    ValidityClaim.CountExactly(Critical.Count, nodes),
```

```csharp
from condensed in Condense(arcs, partition.Cells.Value, key)
let census = condensed.Census
let sites = census.Map(row => seeds[row.Cell])
from critical in census.Map((row, node) => (Row: row, Site: sites[node]))
    .TraverseM(entry => entry.Row.Recurrent
        ? CriticalAt(source, entry.Site, policy, context, key)
            .Map(row => (Kind: Some(row.Kind), row.Unstable))
        : Fin.Succ((Kind: Option<FixedPointKind>.None, Unstable: Seq<Vector3d>())))
    .As()
```

```csharp
let atlas = new MorseGraph(
    CellComponent: condensed.Component, Site: sites,
    Critical: critical.Map(static row => row.Kind),
    Arc: condensed.Arc, Crossing: condensed.Crossing, Separatrices: separatrices)
```

```csharp
from rows in critical.Map(static (row, node) => (Row: row, Node: node))
    .Filter(static entry => entry.Row.Kind
        .Map(kind => kind.Equals(FixedPointKind.Saddle))
        .IfNone(false))
```

```csharp
private static Fin<(Seq<int> Component, Seq<(int Cell, bool Recurrent)> Census,
    Seq<(int From, int To)> Arc, Seq<int> Crossing)> Condense(Seq<SEdge<int>> arcs, int cells, Op key) =>
    key.Catch(() => {
        AdjacencyGraph<int, SEdge<int>> graph = arcs.ToAdjacencyGraph<int, SEdge<int>>(allowParallelEdges: false);
        graph.AddVertexRange(Enumerable.Range(0, cells));
        IMutableBidirectionalGraph<AdjacencyGraph<int, SEdge<int>>,
            CondensedEdge<int, SEdge<int>, AdjacencyGraph<int, SEdge<int>>>> condensed =
            graph.CondensateStronglyConnected<int, SEdge<int>, AdjacencyGraph<int, SEdge<int>>>();
        Seq<AdjacencyGraph<int, SEdge<int>>> groups = toSeq(condensed.Vertices);
        int[] component = new int[cells];
        groups.Map((group, index) => (Group: group, Index: index)).Iter(row =>
            toSeq(row.Group.Vertices).Iter(cell => component[cell] = row.Index));
        Seq<(int Cell, bool Recurrent)> census = groups.Map(group => (
            Cell: group.Vertices.First(),
            Recurrent: group.VertexCount > 1 || group.Edges.Any(static edge => edge.IsSelfEdge())));
        Seq<(int From, int To, int Crossing)> links = toSeq(condensed.Edges).Map(edge => (
            From: component[edge.Source.Vertices.First()],
            To: component[edge.Target.Vertices.First()],
            Crossing: edge.Edges.Count));
        return Fin.Succ((toSeq(component), census,
            links.Map(static row => (row.From, row.To)), links.Map(static row => row.Crossing)));
    });
```

## Why

`Label` runs Tarjan explicitly and `Contract` immediately asks QuikGraph to compute the same strongly connected partition again. The condensation already exposes each component subgraph and every merged cross-component edge, so the label dictionary, size arrays, trapped set, and second SCC traversal are redundant. `FixedPointKind.Transient` is also false domain identity: a nonrecurrent component was not classified as a fixed point at all, so the result is absence rather than a sixth critical-point kind.

## Change

Materialize the source graph once, call QuikGraph's `CondensateStronglyConnected` once, enumerate its component subgraphs into the cell-label and recurrent-site columns, and enumerate its `CondensedEdge.Edges` collections into arc/crossing columns. Replace the `Label`→`Census`→`Contract` chain with one `Condense` bind. Delete `Transient`, rename the output column from vague `Kind` to `Critical`, and use `Option<FixedPointKind>` so nonrecurrent components carry `None`; saddle selection tests the option instead of comparing a fabricated row.

## Delta

Code-fence LOC: -10 net. Module surface: -3 methods and -1 smart-enum row, +1 method and +0 types; -3 members net. One full SCC computation and four transient collections (`labels`, `first`, `size`, `trapped`) are removed; condensed edge endpoints reuse the cell-component array instead of adding a second component index.

## Ripples

Update the `MorseGraph` construction at `flow.md:507`, validity count at `flow.md:371`, and saddle filter at `flow.md:602` to use `Critical: critical.Map(static row => row.Kind)` and `Option`-shaped classification. Remove `StronglyConnectedComponents` and `HashSet` from the target's package prose after their final uses disappear. Repository search finds no code-fence consumer reading `MorseGraph.Kind`; the identity-output shape changes only at this owner.

# 8. Fuse atlas projection into its sole entrypoint

## From

`libs/dotnet/Rasm/.planning/Processing/flow.md:383`

```csharp
internal Fin<TOut> Project<TOut>(Op key) {
    MorseGraph self = this;
    return ResultProjection.Rows<MorseGraph, TOut>(self: self, key: key,
        ProjectionRow.Of<Seq<Point3d>>(() => self.Site.TraverseM(site => key.AcceptValue(value: site)).As()),
        ProjectionRow.Of<Seq<Separatrix>>(() => Fin.Succ(self.Separatrices)),
        ProjectionRow.Of<Seq<Line>>(() => Fin.Succ(self.Arc.Map(arc =>
            new Line(from: self.Site[arc.From], to: self.Site[arc.To])))));
}
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:485`

```csharp
internal static class MorseAtlas {
    internal static Fin<MorseGraph> Of(
```

`libs/dotnet/Rasm/.planning/Processing/flow.md:507`

```csharp
let atlas = new MorseGraph(
    CellComponent: labelled.Component, Site: sites, Kind: critical.Map(static row => row.Kind),
    Arc: contracted.Arc, Crossing: contracted.Crossing, Separatrices: separatrices)
from valid in atlas.IsValid ? Fin.Succ(atlas) : Fin.Fail<MorseGraph>(key.InvalidResult())
select valid;
```

## To

```csharp
// Project DELETED
```

```csharp
internal static class MorseAtlas {
    internal static Fin<TOut> Of<TOut>(
```

```csharp
let atlas = new MorseGraph(
    CellComponent: condensed.Component, Site: sites, Critical: critical.Map(static row => row.Kind),
    Arc: condensed.Arc, Crossing: condensed.Crossing, Separatrices: separatrices)
from valid in atlas.IsValid ? Fin.Succ(atlas) : Fin.Fail<MorseGraph>(key.InvalidResult())
from output in ResultProjection.Rows<MorseGraph, TOut>(self: valid, key: key,
    ProjectionRow.Of<Seq<Point3d>>(() => Fin.Succ(valid.Site)),
    ProjectionRow.Of<Seq<Separatrix>>(() => Fin.Succ(valid.Separatrices)),
    ProjectionRow.Of<Seq<Line>>(() => Fin.Succ(valid.Arc.Map(arc =>
        new Line(from: valid.Site[arc.From], to: valid.Site[arc.To])))))
select output;
```

## Why

`MorseGraph.Project` has one repository call site immediately after `MorseAtlas.Of`; it is a thin continuation that re-traverses already validated sites. The atlas carrier remains a supported output through `ResultProjection.Rows` identity fallthrough, so the separate member adds no capability.

## Change

Make `MorseAtlas.Of<TOut>` perform the typed projection after its single validity gate, return the admitted site sequence directly, and delete `MorseGraph.Project`.

## Delta

Code-fence LOC: -4 net. Module surface: -1 method, +0 methods/types; -1 member net. One per-site validation traversal removed.

## Ripples

At `libs/dotnet/Rasm/.planning/Processing/intent.md:339`, replace the three-line `MorseAtlas.Of(...).Bind/Project` query with the single call `MorseAtlas.Of<TOut>(...)`. Update the `flow.md` owner/entry/output prose and `libs/dotnet/Rasm/ARCHITECTURE.md:71` to describe the generic projected entrypoint, while retaining `MorseGraph` as the identity output.
