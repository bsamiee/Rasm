# [RASM_VECTORS_FLOW]

`FlowKernel.Trace` advances any `VectorField` into a streamline under the `Numerics/integrate.md` adaptive stepper, deciding every stop through one `Termination` `[Union]` and localizing every crossing onto the high-order solution curve. `MorseAtlas.Of` folds that same tracer over a caller's cell partition into the flow's topology — recurrent sets, eigen-classified critical sites, and saddle-seeded separatrices — as one frozen-column `MorseGraph`.

Every receipt validates through the `Domain/rails.md` `ValidityClaim.All` fold under page-only cross-field claims, and raw ingress gates through its acceptance bridge; `Numerics/atoms.md` `AtomProjection.Rows` resolves projection with the receipt as its implicit self row.

## [01]-[INDEX]

- [02]-[TERMINATION]: `Termination` `[Union]` stop vocabulary and the tiered localizer refining each crossing onto the dense-output curve.
- [03]-[TRACE]: `FlowKernel.Trace` folding the immutable streamline state under the numerics stepper into the typed-row projection.
- [04]-[TOPOLOGY]: `MorseAtlas.Of` contracting the cell-transition digraph into the SoA `MorseGraph` — recurrent sets, eigen-classified sites, and saddle-seeded separatrices.

## [02]-[TERMINATION]

- Owner: `Termination` `[Union]` mints one closed stop vocabulary the tracer evaluates at each accepted step; `TraceEventKind`, `TraceEventStatus`, and `TraceEventLocalizationKind` stay three orthogonal receipt vocabularies, never one merged flag.
- Entry: `Termination.Evaluate` folds a total generated `Switch` — scalar and loop stops decide from state alone, an event stop samples its signed value function and runs the localizer.
- Auto: localization refines a bracketed sign change through `DenseOutputSpan.PointAt` onto the high-order curve and falls to the chord where the segment carries no dense span; an endpoint inside tolerance short-circuits to a zero-iteration touch. Every tolerance derives from `Context`, never a bare literal.
- Boundary: `CrossSurfaceCase` admits any constructed `SupportSpace` on the `Spatial/support.md` `Of` gate's closest-capability proof, re-checking signed-distance capability per hit and raising a typed `Unsupported` fault naming the source type. Factories admit raw doubles through `Op.AcceptValidated<PositiveMagnitude>`; a non-positive magnitude is a typed `InvalidInput` fault, never a clamp. `CriticalCaptureCase` refuses an empty or non-finite site roster at the same gate, so its signed nearest-site function is total before any trace reads it.

## [03]-[TRACE]

- Owner: `TracePolicy` carries the iteration ceiling and localization budget as policy rows, never compiled-in constants; `SpatialIntegration.Module` is the one `IntegrationModule<Point3d, Vector3d>` instance `integrate.md` assigns this consumer; `StreamlineState.Accept`/`Reject` are the immutable fold state's only transitions.
- Entry: `FlowKernel.Trace<TOut>` is the one trace entrypoint, `TOut` discriminating the projection.
- Auto: `PolylineOf` substitutes the localized event point for the final trail vertex, so emitted geometry ends exactly at the crossing.
- Packages: `Rasm`/Numerics (`FieldIntegrator`/`IntegrationModule`/`IntegrationStep`/`DenseOutputSpan`/`DenseOutputReceipt` stepper floor; `AtomProjection`/`ProjectionRow`; `Dimension`/`PositiveMagnitude`/`EpsilonPolicy`), `Rasm`/Spatial (`SupportSpace`; `ScalarField.SampleScalar`), `Rasm`/Domain (`Op`/`Context`/`Admit`/`ValidityClaim`), LanguageExt.Core (`Fin`/`Option`/`Seq`/`Atom`/`IO`/`Schedule`), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<int>]`), RhinoCommon (`Point3d`/`Vector3d`/`Polyline`/`Curve.ToPolylineCurve`).
- Growth: a new stop condition is one `Termination` case and one `Evaluate` arm, the generated `Switch` breaking every dispatch site loudly; a new event source is one `TraceEventKind` row over the same localizer; a new output shape is one `ProjectionRow`; a bidirectional or multi-seed trace folds over this same entry, never a sibling tracer.
- Boundary: every failure routes a typed fault, keeping the tracer total over the `Fin` rail. One `Atom` cell holds the immutable loop state as the sole boundary state seam under idempotent `Swap` transitions, and `Polyline`/`Curve` project only a `Terminated` trace, so a budget-exhausted trail never masquerades as a completed streamline.

## [04]-[TOPOLOGY]

- Owner: `MorseGraph` is the atlas carrier — cell-indexed `CellComponent`, component-indexed `Site`/`Kind`, arc-indexed `Arc`/`Crossing`, and the `Separatrix` rows — every column frozen at the mint; `FixedPointKind` `[SmartEnum<int>]` closes the critical-point signature vocabulary with `Transient` for the component the atlas measured no linearization at; `FlowPartition` is the caller's cell capsule (census, representative, total locate) and `TopologyPolicy` its scale and horizon record; `MorseAtlas` is the static surface.
- Entry: `MorseAtlas.Of(VectorField, FlowPartition, PositiveMagnitude, FieldIntegrator, TopologyPolicy, Context, Op, Option<TracePolicy>)` is the one atlas entrypoint, returning the validated `MorseGraph`; a caller supplies field, partition, and scale and never sequences the transition, labelling, contraction, classification, and separatrix legs itself.
- Auto: the cell-transition digraph materializes ONCE through `GraphExtensions.ToAdjacencyGraph`, so `StronglyConnectedComponents` and `CondensateStronglyConnected` read one container — Tarjan's labels fill the caller's dictionary and ARE the component partition — recurrent wherever a component holds two or more cells and, for a singleton, exactly where its own self-arc says the horizon trapped it — while the condensation's vertices are the component subgraphs and each condensed edge carries the merged cell transitions its `Crossing` weight counts. Classification linearizes at the component's representative through the `Numerics/calculus` `Nabla.SampleAxes` six-tap stencil and reads the GENERAL `Matrix.DecomposeEigenDetailed` spectrum: a flow Jacobian is not symmetric, so its complex pairs are what separate a centre from a node, and the signature folds spectral-radius-relative against `EpsilonPolicy.SqrtEpsilon`. Separatrices are multi-seed traces over the settled `FlowKernel.Trace<TOut>` — both senses of every real unstable eigendirection of every saddle, each seeded clear of every capture ball — so the band adds zero integration surface.
- Receipt: `MorseGraph` is the typed evidence under one `ValidityClaim.All` fold gating column alignment, node ranges, and every `Separatrix` row; an atlas failing that fold routes `InvalidResult` rather than publishing a misaligned column, and `Project<TOut>` resolves the sites, the separatrix rows, the condensed arcs as world segments, and the graph itself off the rail's self row.
- Packages: `Rasm`/Numerics (`Nabla.SampleAxes`; `Matrix`/`EigenSolveReceipt`/`EigenOrder`; `EpsilonPolicy`; `Dimension`/`PositiveMagnitude`), `Rasm`/Spatial (`VectorField.SampleVector`), `Rasm`/Domain (`Op`/`Context`/`ValidityClaim`), QuikGraph (`SEdge`/`AdjacencyGraph`, `GraphExtensions.ToAdjacencyGraph`, `AlgorithmExtensions.StronglyConnectedComponents`/`CondensateStronglyConnected`), LanguageExt.Core (`Fin`/`Option`/`Seq`/`HashSet`), Thinktecture.Runtime.Extensions (`[SmartEnum<int>]`), RhinoCommon (`Point3d`/`Vector3d`), BCL inbox (`System.Numerics.Complex`).
- Growth: a new critical signature is one `FixedPointKind` row read off the same spectral fold; a new partition source is one `FlowPartition` value, never a mesh, lattice, or complex type reaching this band; a stable manifold is the unstable manifold of the sign-reversed field the `Spatial/fields` `Scaled` case already mints, so reversal is the caller's field algebra and never a second tracer.
- Boundary: `TopologicalSort` and both bidirectional forms throw `NonAcyclicGraphException` and a flow digraph is cyclic by construction, so the band never composes them — the condensation IS the acyclic product; every QuikGraph value stays transient inside the fold with its throws funnelled through `Op.Catch`, and a graph-typed public member is the killed shape. The band publishes through `Processing/intent.md`'s `VectorIntent.Atlas` case exactly as the tracer publishes through `Streamline`, so `MorseAtlas` and `FlowKernel` both stay internal behind one admission-then-dispatch rail. A site is a recurrent set's representative sample, never a root-solved zero — refining one to the field's exact zero is the `Solving/solver` functor's — and a separatrix whose horizon runs out carries `None` for its terminal node rather than a fabricated one. The census and Jacobian-assembly loops are the named span-kernel statement exemption.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Rasm.Csp;
using LanguageExt;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;
// CS0104 guard: Rhino.Geometry declares Matrix/Dimension homonyms under the dual usings, and System.Numerics
// declares Plane/Quaternion ones, so the spectrum carrier enters by alias rather than a whole-namespace using.
using Complex = System.Numerics.Complex;
using Dimension = Rasm.Numerics.Dimension;
using Matrix = Rasm.Numerics.Matrix;

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
    public static readonly TraceEventKind CriticalCapture = new(key: 2);
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

// Critical-point signature over the local Jacobian spectrum: Transient is the non-recurrent component carrying no
// linearization at all, so a classification column never spells a fixed point where the atlas measured none.
[SmartEnum<int>]
public sealed partial class FixedPointKind {
    public static readonly FixedPointKind Transient = new(key: 0);
    public static readonly FixedPointKind Source = new(key: 1);
    public static readonly FixedPointKind Sink = new(key: 2);
    public static readonly FixedPointKind Saddle = new(key: 3);
    public static readonly FixedPointKind Center = new(key: 4);
    public static readonly FixedPointKind Degenerate = new(key: 5);
}

[Union]
public abstract partial record Termination {
    public sealed record StepCountCase(Dimension Count) : Termination;
    public sealed record ArcLengthCase(PositiveMagnitude Length) : Termination;
    public sealed record MagnitudeFloorCase(PositiveMagnitude Threshold) : Termination;
    public sealed record CrossSurfaceCase(SupportSpace Surface, Dimension LocalizationBudget) : Termination;
    public sealed record RegionThresholdCase(ScalarField Region, double Threshold, Dimension LocalizationBudget) : Termination;
    public sealed record LoopDetectedCase(PositiveMagnitude ClosureRadius) : Termination;
    public sealed record CriticalCaptureCase(Seq<Point3d> Sites, PositiveMagnitude CaptureRadius, Dimension LocalizationBudget) : Termination;
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
    // Distance to the NEAREST site minus the capture radius is the signed function, so one case stops a trace at whichever
    // critical site swallows it and the localizer lands the terminus exactly on that capture sphere.
    public static Fin<Termination> CriticalCapture(Seq<Point3d> sites, double captureRadius, Option<Dimension> localizationBudget = default, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: captureRadius).Bind(radius =>
            new CriticalCaptureCase(Sites: sites, CaptureRadius: radius,
                LocalizationBudget: localizationBudget.IfNone(TracePolicy.Default.LocalizationBudget)).Admit(key: op));
    }

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
        loopDetectedCase: static (_, termination) => Fin.Succ<Termination>(termination),
        criticalCaptureCase: static (op, termination) =>
            !termination.Sites.IsEmpty && termination.Sites.ForAll(static site => site.IsValid)
                ? Fin.Succ<Termination>(termination)
                : Fin.Fail<Termination>(op.InvalidInput()));
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
            key: s.Key).Map(@event => (Stop: @event.IsSome, Event: @event)),
        criticalCaptureCase: static (s, c) => EvaluateEvent(
            state: s.Field, kind: TraceEventKind.CriticalCapture, tolerance: s.Context.Absolute.Value, budget: c.LocalizationBudget.Value,
            sample: point => Fin.Succ(c.Sites.Fold(double.PositiveInfinity, (nearest, site) => Math.Min(val1: nearest, val2: point.DistanceTo(other: site))) - c.CaptureRadius.Value),
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

// Three columns carry model scale and this band never guesses one: the caller states its capture radius and stencil
// width, the seed offset derives from the radius alone, and the two horizons stay dimensionless step counts.
public sealed record TopologyPolicy(
    Dimension TransitionSteps, Dimension SeparatrixSteps,
    PositiveMagnitude StencilWidth, PositiveMagnitude CaptureRadius, PositiveMagnitude SeedOffset) {
    // Dimensionless: a seed steps this many capture radii off its saddle, so leaving its own ball never reads as a capture.
    private const double SeedFactor = 4.0;

    public static Fin<TopologyPolicy> Of(
        double captureRadius, double stencilWidth,
        Option<Dimension> transitionSteps = default, Option<Dimension> separatrixSteps = default, Op? key = null) {
        Op op = key.OrDefault();
        return from radius in op.AcceptValidated<PositiveMagnitude>(candidate: captureRadius)
               from stencil in op.AcceptValidated<PositiveMagnitude>(candidate: stencilWidth)
               from offset in op.AcceptValidated<PositiveMagnitude>(candidate: SeedFactor * radius.Value)
               select new TopologyPolicy(
                   TransitionSteps: transitionSteps.IfNone(Dimension.Create(value: 64)),
                   SeparatrixSteps: separatrixSteps.IfNone(Dimension.Create(value: 4_096)),
                   StencilWidth: stencil, CaptureRadius: radius, SeedOffset: offset);
    }
}

// The band owns no mesh, lattice, or complex: the caller supplies the cell census, one representative sample per cell,
// and the total locate whose None IS the outside — so any decomposition that can name its cells drives this atlas.
public sealed record FlowPartition(Dimension Cells, Func<int, Point3d> Representative, Func<Point3d, Option<int>> Locate) {
    public static Fin<FlowPartition> Of(Dimension cells, Func<int, Point3d> representative, Func<Point3d, Option<int>> locate, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in guard(cells.Value > 0 && representative is not null && locate is not null, op.InvalidInput()).ToFin()
               select new FlowPartition(Cells: cells, Representative: representative, Locate: locate);
    }
}

// Terminal is the localized capture point the event owner already refined; To is None where the horizon ran out or the
// terminus left the partition, so an unfinished separatrix states its absence instead of naming a node it never reached.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct Separatrix(
    int From, Option<int> To, Point3d Terminal, Seq<Point3d> Trail, StreamlineStopKind Stop) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(From, 0),
        ValidityClaim.CountAtLeast(Trail.Count, 1),
        ValidityClaim.Of(Trail.ForAll(static point => point.IsValid)),
        ValidityClaim.Finite(Terminal),
        ValidityClaim.Of(To.Map(static node => node >= 0).IfNone(noneValue: true)),
        ValidityClaim.Of(Stop is not null));
}

// The SoA Morse graph: CellComponent is cell-indexed, Site and Kind component-indexed, Arc and Crossing arc-indexed.
// Every QuikGraph container dies inside the fold that mints this, so no consumer ever holds mutable topology.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct MorseGraph(
    Seq<int> CellComponent, Seq<Point3d> Site, Seq<FixedPointKind> Kind,
    Seq<(int From, int To)> Arc, Seq<int> Crossing, Seq<Separatrix> Separatrices) : IValidityEvidence {
    public bool IsValid {
        get {
            int nodes = Site.Count;
            return ValidityClaim.All(
                ValidityClaim.CountAtLeast(nodes, 1),
                ValidityClaim.CountExactly(Kind.Count, nodes),
                ValidityClaim.CountExactly(Crossing.Count, Arc.Count),
                ValidityClaim.Of(Site.ForAll(static site => site.IsValid)),
                ValidityClaim.Of(CellComponent.ForAll(node => node >= 0 && node < nodes)),
                ValidityClaim.Of(Arc.ForAll(arc => arc.From != arc.To
                    && arc.From >= 0 && arc.From < nodes && arc.To >= 0 && arc.To < nodes)),
                ValidityClaim.Of(Crossing.ForAll(static count => count >= 1)),
                ValidityClaim.Of(Separatrices.ForAll(row => row.IsValid && row.From < nodes
                    && row.To.Map(node => node < nodes).IfNone(noneValue: true))));
        }
    }

    // Three rows over the frozen columns, the whole graph riding the rail's own self row: the sites as points, the
    // separatrix rows themselves, and the condensed arcs as world segments between the sites they connect — the
    // drawable Morse skeleton. A column read past these rows is a field access on the returned value, never a row.
    internal Fin<TOut> Project<TOut>(Op key) {
        MorseGraph self = this;
        return AtomProjection.Rows<MorseGraph, TOut>(self: self, key: key,
            ProjectionRow.Of<Seq<Point3d>>(() => self.Site.TraverseM(site => key.AcceptValue(value: site)).As()),
            ProjectionRow.Of<Seq<Separatrix>>(() => Fin.Succ(self.Separatrices)),
            ProjectionRow.Of<Seq<Line>>(() => Fin.Succ(self.Arc.Map(arc =>
                new Line(from: self.Site[arc.From], to: self.Site[arc.To])))));
    }
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

// The transition digraph materializes ONCE and both graph reads take that one container: Tarjan labels it and the
// condensation contracts it. Every QuikGraph value is local to this fold, its throws funnel through Op.Catch, and the
// SoA MorseGraph is what leaves — a graph-typed member would re-couple a consumer to mutable topology.
internal static class MorseAtlas {
    internal static Fin<MorseGraph> Of(
        VectorField source, FlowPartition partition, PositiveMagnitude initialStep, FieldIntegrator integrator,
        TopologyPolicy policy, Context context, Op key, Option<TracePolicy> tracePolicy = default) =>
        from horizon in Termination.Steps(count: policy.TransitionSteps.Value, key: key)
        from arcs in Ordinals(partition: partition).Fold(Fin.Succ(Seq<SEdge<int>>()), (acc, cell) => acc.Bind(rows =>
            Visited(source: source, partition: partition, seed: partition.Representative(arg: cell), initialStep: initialStep,
                    integrator: integrator, termination: horizon, context: context, key: key, policy: tracePolicy)
                .Map(visited => rows + Transitions(cell: cell, visited: visited))))
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
        from separatrices in SeparatrixRows(source: source, partition: partition, sites: sites, component: labelled.Component,
            critical: critical, initialStep: initialStep, integrator: integrator, policy: policy, context: context, key: key)
        let atlas = new MorseGraph(
            CellComponent: labelled.Component, Site: sites, Kind: critical.Map(static row => row.Kind),
            Arc: contracted.Arc, Crossing: contracted.Crossing, Separatrices: separatrices)
        from valid in atlas.IsValid ? Fin.Succ(atlas) : Fin.Fail<MorseGraph>(key.InvalidResult())
        select valid;

    private static Seq<int> Ordinals(FlowPartition partition) => toSeq(Enumerable.Range(start: 0, count: partition.Cells.Value));

    // The τ-map at cell grain: one horizon-bounded trace per cell, its trail located and collapsed to the run of cells entered.
    private static Fin<Seq<int>> Visited(
        VectorField source, FlowPartition partition, Point3d seed, PositiveMagnitude initialStep, FieldIntegrator integrator,
        Termination termination, Context context, Op key, Option<TracePolicy> policy) =>
        FlowKernel.Trace<Seq<Point3d>>(source: source, seed: seed, initialStep: initialStep, integrator: integrator,
                termination: termination, context: context, key: key, policy: policy)
            .Map(trail => trail.Fold((Run: Seq<int>(), Last: -1), (state, point) =>
                partition.Locate(arg: point) is { Case: int cell } && cell != state.Last
                    ? (Run: state.Run.Add(cell), Last: cell)
                    : state).Run);

    // A trail that never leaves its own cell within the horizon is trapped there, and that self-arc is what separates a
    // recurrent singleton from Tarjan's transient one; every other consecutive pair is one transition row.
    private static Seq<SEdge<int>> Transitions(int cell, Seq<int> visited) =>
        visited.Count < 2
            ? Seq(new SEdge<int>(source: cell, target: cell))
            : visited.Map(static (to, index) => (To: to, Index: index)).Filter(static row => row.Index > 0)
                .Map(row => new SEdge<int>(source: visited[row.Index - 1], target: row.To));

    private static Fin<(Seq<int> Component, int Count, AdjacencyGraph<int, SEdge<int>> Graph)> Label(Seq<SEdge<int>> arcs, int cells, Op key) =>
        key.Catch(() => {
            AdjacencyGraph<int, SEdge<int>> graph = arcs.ToAdjacencyGraph<int, SEdge<int>>(allowParallelEdges: false);
            graph.AddVertexRange(Enumerable.Range(start: 0, count: cells));  // a cell no arc touched still owns its component
            Dictionary<int, int> labels = new(capacity: cells);
            int count = graph.StronglyConnectedComponents(labels);
            return count > 0
                ? Fin.Succ((Component: toSeq(Enumerable.Range(start: 0, count: cells).Select(cell => labels[cell])), Count: count, Graph: graph))
                : Fin.Fail<(Seq<int> Component, int Count, AdjacencyGraph<int, SEdge<int>> Graph)>(key.InvalidResult());
        });

    // One pass over the labelling: a component's first cell is its representative, and two or more cells are recurrent by
    // construction where a singleton is recurrent only under its own self-arc.
    private static Seq<(int Cell, bool Recurrent)> Census(Seq<int> component, int nodes, LanguageExt.HashSet<int> trapped) {
        int[] first = new int[nodes];
        int[] size = new int[nodes];
        Array.Fill(array: first, value: -1);
        for (int cell = 0; cell < component.Count; cell++) {
            size[component[cell]]++;
            if (first[component[cell]] < 0) first[component[cell]] = cell;
        }
        return toSeq(first.Select((cell, node) => (Cell: cell, Recurrent: size[node] > 1 || trapped.Contains(cell))));
    }

    // The condensation contracts the SAME container Tarjan labelled: a condensed vertex IS a component subgraph and a
    // condensed edge carries every cell transition it merged, so each arc reports a measured crossing weight.
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

    // Row i column j is ∂vᵢ/∂xⱼ off the one six-tap stencil traversal the calculus owner already folds. The GENERAL eigen
    // route carries it because a flow Jacobian is not symmetric, and its complex pairs are what separate a centre from a node.
    private static Fin<(FixedPointKind Kind, Seq<Vector3d> Unstable)> CriticalAt(
        VectorField source, Point3d site, TopologyPolicy policy, Context context, Op key) =>
        from samples in Nabla.SampleAxes<Vector3d>(
            sampler: point => source.SampleVector(sample: point, context: context, key: key),
            point: site, eps: policy.StencilWidth.Value, key: key)
        let inv2eps = 1.0 / (2.0 * policy.StencilWidth.Value)
        from jacobian in Matrix.Of(rows: Dimension.Create(value: 3), cols: Dimension.Create(value: 3), entries: [
            (samples.X1.X - samples.X0.X) * inv2eps, (samples.Y1.X - samples.Y0.X) * inv2eps, (samples.Z1.X - samples.Z0.X) * inv2eps,
            (samples.X1.Y - samples.X0.Y) * inv2eps, (samples.Y1.Y - samples.Y0.Y) * inv2eps, (samples.Z1.Y - samples.Z0.Y) * inv2eps,
            (samples.X1.Z - samples.X0.Z) * inv2eps, (samples.Y1.Z - samples.Y0.Z) * inv2eps, (samples.Z1.Z - samples.Z0.Z) * inv2eps], key: key)
        from receipt in jacobian.DecomposeEigenDetailed(key: key)
        from pairs in receipt.PairsIn(expected: EigenOrder.Factorization, key: key)
        let tolerance = EpsilonPolicy.SqrtEpsilon * pairs.Fold(0.0, static (peak, pair) => Math.Max(val1: peak, val2: pair.Eigenvalue.Magnitude))
        select (
            Kind: Signature(spectrum: pairs.Map(static pair => pair.Eigenvalue), tolerance: tolerance),
            // A complex pair spans a rotation plane carrying no invariant line, so only the REAL unstable arms seed a separatrix.
            Unstable: pairs
                .Filter(pair => pair.Eigenvalue.Real > tolerance && Math.Abs(value: pair.Eigenvalue.Imaginary) <= tolerance)
                .Map(static pair => new Vector3d(x: pair.Eigenvector[0].Real, y: pair.Eigenvector[1].Real, z: pair.Eigenvector[2].Real))
                .Filter(static direction => !direction.IsTiny())
                .Map(static direction => (1.0 / direction.Length) * direction));

    // Order-independent: the spectrum folds whole, so the Factorization order the general route returns decides nothing.
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

    // Multi-seed over the ONE trace entrypoint: each saddle emits both senses of every real unstable eigendirection, each
    // seeded a SeedFactor of capture radii out so the departure never reads as a capture, and the capture case stops it.
    private static Fin<Seq<Separatrix>> SeparatrixRows(
        VectorField source, FlowPartition partition, Seq<Point3d> sites, Seq<int> component,
        Seq<(FixedPointKind Kind, Seq<Vector3d> Unstable)> critical, PositiveMagnitude initialStep,
        FieldIntegrator integrator, TopologyPolicy policy, Context context, Op key) =>
        from capture in Termination.CriticalCapture(sites: sites, captureRadius: policy.CaptureRadius.Value, key: key)
        from rows in critical.Map(static (row, node) => (Row: row, Node: node))
            .Filter(static entry => entry.Row.Kind.Equals(FixedPointKind.Saddle))
            .Bind(entry => entry.Row.Unstable.Bind(direction => Seq(
                (Node: entry.Node, Seed: sites[entry.Node] + (policy.SeedOffset.Value * direction)),
                (Node: entry.Node, Seed: sites[entry.Node] - (policy.SeedOffset.Value * direction)))))
            .TraverseM(seed => Follow(source: source, partition: partition, component: component, node: seed.Node,
                seed: seed.Seed, initialStep: initialStep, integrator: integrator, termination: capture,
                policy: policy, context: context, key: key))
            .As()
        select rows;

    private static Fin<Separatrix> Follow(
        VectorField source, FlowPartition partition, Seq<int> component, int node, Point3d seed, PositiveMagnitude initialStep,
        FieldIntegrator integrator, Termination termination, TopologyPolicy policy, Context context, Op key) =>
        FlowKernel.Trace<StreamlineTrace>(source: source, seed: seed, initialStep: initialStep, integrator: integrator,
                termination: termination, context: context, key: key,
                policy: Some(TracePolicy.Default with { MaxIterations = policy.SeparatrixSteps }))
            .Map(trace => new Separatrix(
                From: node,
                To: trace.IsComplete ? partition.Locate(arg: trace.TerminationPoint).Map(cell => component[cell]) : Option<int>.None,
                Terminal: trace.TerminationPoint, Trail: trace.Trail, Stop: trace.Stop));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
