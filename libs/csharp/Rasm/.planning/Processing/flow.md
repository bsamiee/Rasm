# [RASM_VECTORS_FLOW]

The streamline/trace owner — ONE `Termination` `[Union]` (`StepCount`/`ArcLength`/`MagnitudeFloor`/`CrossSurface`/`RegionThreshold`/`LoopDetected`) that decides every stop and localizes every crossing event by dense-output root bisection, and ONE `FlowKernel.Trace<TOut>` entry that advances any `VectorField` under the `Numerics/integrate.md` stepper through a fold-shaped immutable `StreamlineState`, terminating by a `Schedule.recurs`-driven `RepeatWhile` whose iteration ceiling is the `TracePolicy.MaxIterations` policy row — never a compiled-in constant. The stepper seam is `Numerics/integrate.md`'s carrier-generic `FieldIntegrator.Step`: THIS page declares the ONE spatial `IntegrationModule<Point3d, Vector3d>` instance the stepper folds over (`integrate.md` assigns its consumer exactly one module declaration), and the step outcome arrives as the settled `IntegrationStep<Point3d, Vector3d>` accepted/rejected union carrying the `DenseOutputSpan<Point3d, Vector3d>` continuous extension — no parallel step or dense-output vocabulary exists here, and `IntegratorKind`, `ButcherTableau`, `DenseOutputCoefficientFamily`, `DenseWeightsAt`, and the module's single `Combine` fold are composed as settled numerics, never re-derived. Event localization is exact where the integrator grants it: a bracketed sign change refines through the accepted step's `DenseOutputSpan.PointAt` so the localized point lies ON the high-order solution curve (`DenseOutputRoot`), and only a dense-less segment falls back to chord bisection (`BoundedBisection`) — the localization kind is receipt evidence, not a mode flag.

`Op` stays the explicit value key threaded positionally through every kernel signature — these pipelines are short, so no `Eff<Env>` lift is warranted, and no dual paradigm exists. Every receipt (`TraceEvent`, `StreamlineTrace`, the composed `DenseOutputReceipt`) rides the `Domain/rails.md` validity fold: `IsValid` is one `ValidityClaim.All` over claim rows — finiteness, non-negative counts, unit-interval parameters, ordering, nested evidence — plus the cross-field claims only this page can state; a hand-rolled `&&` chain in a receipt body is the deleted form. Result projection routes through `Numerics/atoms.md`'s `AtomProjection.Rows` typed-row dispatch with the receipt as the implicit self row — `typeof(TOut)` reflection switching is dead corpus-wide. Termination admission leans on the `Spatial/support.md` adapter directly: an admitted `SupportSpace` is closest-capable by construction, and the per-hit `AdmitsSignedDistance(hit)` gate runs exactly where the hit exists; raw ingress gates once through the `Domain/rails.md` acceptance bridge (`Op.Need`/`Op.Finite`/`Op.AcceptValidated`) — the `Admit` member name shadows the `Rasm.Domain.Admit` class inside `Termination`, so the Op-owned gates are the canonical form here.

## [01]-[INDEX]

- [02]-[TERMINATION]: the 6-stop `Termination` union; stop/event vocabularies (`StreamlineStopKind`/`TraceEventKind`/`TraceEventStatus`/`TraceEventLocalizationKind`); endpoint-touch and bracketed-crossing event evaluation; dense-output root localization with chord-bisection fallback; the `TraceEvent` receipt.
- [03]-[TRACE]: `TracePolicy` (iteration ceiling as a policy row); the ONE spatial `IntegrationModule` instance; the `StreamlineState` fold over `integrate.md`'s `IntegrationStep` outcomes; `FlowKernel.Trace<TOut>` + `ProjectTrace<TOut>` typed-row projection (`StreamlineTrace`/`Seq<Point3d>`/`Polyline`/`Curve`); the `StreamlineTrace` receipt.

## [02]-[TERMINATION]

- Owner: `Termination` `[Union]` — `StepCountCase(Dimension)`, `ArcLengthCase(PositiveMagnitude)`, `MagnitudeFloorCase(PositiveMagnitude)`, `CrossSurfaceCase(SupportSpace, Dimension LocalizationBudget)`, `RegionThresholdCase(ScalarField, double Threshold, Dimension LocalizationBudget)`, `LoopDetectedCase(PositiveMagnitude ClosureRadius)` — one closed stop vocabulary the tracer evaluates each accepted step; `TraceEventKind` (`CrossSurface`/`RegionThresholdCrossing`) names WHAT crossed, `TraceEventStatus` (`InitialEndpointTouch`/`PreviousEndpointTouch`/`CurrentEndpointTouch`/`BracketedCrossing`) names WHERE the localizer found it, `TraceEventLocalizationKind` (`BoundedBisection`/`DenseOutputRoot`) names HOW — three orthogonal receipt vocabularies, never merged into one flag.
- Entry: `Termination.Evaluate(StreamlineState, Vector3d currentSample, Context, Op)` → `Fin<(bool Stop, Option<TraceEvent> Event)>` — a total generated `Switch`: the three scalar stops (`Steps`/`Arc`/`Magnitude`) and the loop stop decide from state alone; the two event stops sample a signed value function (`CrossSurface` = the support signed distance through `SupportSpace.SignedDistance` off the closest hit, gated per hit by `AdmitsSignedDistance(hit)`; `RegionThreshold` = `ScalarField.SampleScalar − Threshold`) and run the event localizer.
- Auto: event localization is a three-tier decision — an endpoint already inside tolerance emits an endpoint-touch event with zero iterations; a sign change over the segment brackets and bisects to the case's `LocalizationBudget`, refining the midpoint through the accepted step's `DenseOutputSpan.PointAt` when the integrator produced one (so the localized point lies ON the high-order solution curve, `DenseOutputRoot`) and through the chord otherwise (`BoundedBisection`); no sign change and no touch emits no event. `CrossSurface` tolerance is `Context.Absolute`; `RegionThreshold` tolerance is `Context.Fractional` scaled by the threshold magnitude — both scale-derived, never bare literals. `LoopDetected` tests the squared closure radius against every non-adjacent trail vertex.
- Receipt: `TraceEvent` — kind, status, the `(Previous, Current, Localized)` point triple with its value triple, the segment parameter, the governing tolerance, the localized residual, the bisection iteration count, the localization kind, and the composed `Option<DenseOutputReceipt>` — `ValidityClaim.All` over the finiteness/parameter/count rows plus the cross-field claims (the residual restates `|Values.Localized|`, a `DenseOutputRoot` event REQUIRES its dense receipt); `IsValidFor(terminationPoint)` additionally welds the event to the trace terminus.
- Boundary: `CrossSurfaceCase` admits any constructed `SupportSpace` — the `Spatial/support.md` `Of` gate already proves closest capability, so no god-predicate is re-minted here; signed-distance capability is a PER-HIT fact (`AdmitsSignedDistance(hit)`) and an unsupported hit is a typed `Unsupported` fault naming the source type at evaluation, never a silent zero. Factories admit raw doubles through `Op.AcceptValidated<PositiveMagnitude>` and budgets through `Dimension`; a non-positive step count, budget, or radius is a typed `InvalidInput` fault, never a clamp.

## [03]-[TRACE]

- Owner: `TracePolicy(Dimension MaxIterations, Dimension LocalizationBudget)` — the outer iteration ceiling and the default event-localization budget as ONE policy record with `Default` (100 000 / 64); `SpatialIntegration.Module` — the ONE `IntegrationModule<Point3d, Vector3d>` instance in the corpus (`Add: p + h·v`, `Scale`, `Sum`, `Norm: |v|`, `Zero`), the consumer-side declaration `integrate.md` assigns to this page; `StreamlineState` — the immutable fold state (trail, current, step, arc, accepted/rejected counters, min/max step, error extrema, live `DenseOutputSpan<Point3d, Vector3d>`, event, stop) whose `Accept`/`Reject` members fold the `integrate.md` `IntegrationStep<Point3d, Vector3d>` outcome and are the only transitions.
- Entry: `FlowKernel.Trace<TOut>(VectorField source, Point3d seed, PositiveMagnitude initialStep, FieldIntegrator integrator, Termination termination, Context context, Op key, Option<TracePolicy> policy = default)` — the one trace entrypoint; `TOut` discriminates the projection (`StreamlineTrace` receipt, `Seq<Point3d>` trail, `Polyline`, `Curve`), and an incomplete trace refuses the geometric projections while still surfacing its receipt.
- Auto: the trace loop is a `Schedule.recurs(policy.MaxIterations)`-driven `RepeatWhile` over an `Atom<Fin<StreamlineState>>` — the continue-or-done discriminant is `state.Stop`, budget exhaustion is the typed terminal `MaxIterationsExhausted`, and reject-budget exhaustion (the adaptive integrator's `RejectBudget`) is `RejectBudgetExhausted` — never `Fin.Fail`, never a raw counter. Each advance samples the field at the current point, evaluates `Termination` (stop ⇒ record the event and terminate), else folds one `FieldIntegrator.Step` outcome — `Step(module, sample, state, h, key)` over `SpatialIntegration.Module` with the field abstracted as the derivative sampler — through `Accept`/`Reject`. The emitted polyline substitutes the localized event point for the final trail vertex so the geometry ends exactly at the crossing.
- Receipt: `StreamlineTrace` — trail, stop kind, accepted/rejected step counts, arc length, final/min/max step, method + embedded order (read off the integrator), last/max error, termination point, and the optional `TraceEvent` — `ValidityClaim.All` rows plus the cross-field claims (`AcceptedSteps == Trail.Count − 1`, `MinStep ≤ MaxStep`, an event present must satisfy `IsValidFor(TerminationPoint)`); `IsComplete` gates the `Polyline`/`Curve` projections.
- Packages: `Rasm`/Numerics (`FieldIntegrator`/`IntegratorKind`/`IntegrationModule`/`IntegrationStep`/`DenseOutputSpan`/`DenseOutputReceipt` — the `integrate.md` stepper floor, composed never re-derived; `AtomProjection`/`ProjectionRow` — the typed projection rail; `Dimension`/`PositiveMagnitude`), `Rasm`/Spatial (`SupportSpace` closest + per-hit signed distance; `ScalarField.SampleScalar`), `Rasm`/Domain (`Op` fault/acceptance factory, `Context` tolerances, `Admit` vocabulary, `IValidityEvidence`/`ValidityClaim` fold), LanguageExt.Core (`Fin`/`Option`/`Seq`/`Atom`/`IO`/`Schedule`), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<int>]`), RhinoCommon (`Point3d`/`Vector3d`/`Polyline`/`Curve.ToPolylineCurve` — value carriers at the seam).
- Growth: a new stop condition is one `Termination` case + one `Evaluate` arm (the generated `Switch` breaks every dispatch site loudly); a new event source is one `TraceEventKind` row over the SAME localizer; a new output shape is one `ProjectionRow` in `ProjectTrace`; a bidirectional or multi-seed trace is a fold over this SAME entry, never a sibling tracer.
- Boundary: the tracer is total over the `Fin` rail — field-sampling failure, a rejected dense endpoint, or an unlocalizable bracket routes a typed fault, never a throw; the loop state is immutable and the `Atom` cell is the ONE boundary state seam (`Swap` transitions are idempotent per the rails law); a local step-outcome union or dense-output carrier beside the `integrate.md` `IntegrationStep`/`DenseOutputSpan` owners is the deleted parallel-rail form; `Polyline`/`Curve` leave only from a `Terminated` trace so a budget-exhausted trail can never masquerade as a completed streamline; the localized event point and the trace terminus agree to the event tolerance by the `IsValidFor` weld — a receipt that disagrees is invalid by construction, not by convention.

```csharp contract
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Foundation.CSharp.Analyzers.Contracts;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Vectors;

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

    // An admitted SupportSpace is closest-capable by construction (support.md Of gate); the signed-distance
    // gate is per-hit and runs at Evaluate, where the hit exists. Op.Need/Finite are the gates here:
    // the Admit member name shadows the Rasm.Domain.Admit class inside this type.
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

    // Tiered event localizer: endpoint touch -> bracketed root -> none. The dense-output PointAt keeps the
    // localized point on the high-order solution curve; a dense-less segment refines along the chord.
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
    // Bounded bisection over the sign-changing bracket; each midpoint evaluates through the dense output when present.
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

// THE one spatial IntegrationModule instance — integrate.md assigns its consumer exactly one module
// declaration; a second Point3d/Vector3d module anywhere in the corpus is the deleted parallel-rail form.
internal static class SpatialIntegration {
    internal static readonly IntegrationModule<Point3d, Vector3d> Module = new(
        Add: static (state, h, delta) => state + (h * delta),
        Scale: static (factor, delta) => factor * delta,
        Sum: static (left, right) => left + right,
        Norm: static delta => delta.Length,
        Zero: Vector3d.Zero);
}

// --- [MODELS] ---------------------------------------------------------------------------------
// IsValid = ONE ValidityClaim.All fold (Domain/rails.md); the cross-field claims are the rows only
// this receipt can state: the residual restates |Values.Localized|, DenseOutputRoot requires evidence.
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
// Immutable fold state; Accept/Reject fold the integrate.md IntegrationStep outcome and are the only
// transitions. Stop is the continue-or-done discriminant.
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

    // Typed-row projection through the atoms.md rail — the receipt is the implicit self row.
    internal static Fin<TOut> ProjectTrace<TOut>(StreamlineTrace trace, Op key) =>
        from valid in trace.IsValid ? Fin.Succ(trace) : Fin.Fail<StreamlineTrace>(error: key.InvalidResult())
        from output in AtomProjection.Rows<StreamlineTrace, TOut>(self: valid, key: key,
            ProjectionRow.Of<Seq<Point3d>>(() => valid.Trail.TraverseM(point => key.AcceptValue(value: point)).As()),
            ProjectionRow.Of<Polyline>(() => valid.IsComplete ? PolylineOf(trace: valid, key: key) : Fin.Fail<Polyline>(key.InvalidResult())),
            ProjectionRow.Of<Curve>(() => valid.IsComplete
                ? PolylineOf(trace: valid, key: key).Bind(polyline => Optional(polyline.ToPolylineCurve()).ToFin(key.InvalidResult()).Map(static curve => (Curve)curve))
                : Fin.Fail<Curve>(key.InvalidResult())))
        select output;

    // Schedule-driven advance: the Atom cell holds Fin<StreamlineState>; RepeatWhile retires the counter and
    // budget exhaustion lands as the typed MaxIterationsExhausted terminal, never Fin.Fail.
    private static Fin<StreamlineState> TraceState(VectorField source, Point3d seed, PositiveMagnitude initialStep, FieldIntegrator integrator, Termination termination, TracePolicy policy, Context context, Op key) {
        Atom<Fin<StreamlineState>> cell = Atom(value: Fin.Succ(StreamlineState.Start(seed: seed, h: initialStep.Value)));
        _ = IO.lift(() => { _ = cell.Swap(f: state => state.Bind(active => active.Stop.IsSome ? Fin.Succ(active) : AdvanceState(state: active, source: source, integrator: integrator, termination: termination, context: context, key: key))); })
            .RepeatWhile(
                schedule: Schedule.recurs(times: policy.MaxIterations.Value),
                predicate: _ => cell.Value.Match(Succ: static active => active.Stop.IsNone, Fail: static _ => false))
            .Run();
        return cell.Value;
    }
    // One advance: sample -> terminate-or-step; the stepper is integrate.md's carrier-generic Step folded
    // over the ONE spatial module with the field as the derivative sampler.
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
