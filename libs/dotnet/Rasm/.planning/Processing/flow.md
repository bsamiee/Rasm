# [RASM_VECTORS_FLOW]

`FlowKernel.Trace` advances any `VectorField` into a streamline under the `Numerics/integrate.md` adaptive stepper, deciding every stop through one `Termination` `[Union]` and localizing every crossing onto the high-order solution curve. `MorseAtlas.Of<TOut>` folds that same tracer over a caller's cell partition into the flow's topology — recurrent sets, eigen-classified critical sites, and saddle-seeded separatrices — as one frozen-column `MorseGraph`, projected onto the requested output shape at the same entry.

Every result validates through the `Domain/results.md` `ValidityClaim.All` fold under page-only cross-field claims, and raw ingress gates through its acceptance bridge; `Numerics/atoms.md` `ResultProjection.Rows` resolves projection with the result as its implicit self row.

## [01]-[INDEX]

- [02]-[TERMINATION]: `Termination` `[Union]` stop vocabulary and the tiered localizer refining each crossing onto the dense-output curve.
- [03]-[TRACE]: `FlowKernel.Trace` folding the immutable streamline state under the numerics stepper into the typed-row projection.
- [04]-[TOPOLOGY]: `MorseAtlas.Of<TOut>` contracting the cell-transition digraph into the SoA `MorseGraph` — recurrent sets, eigen-classified sites, and saddle-seeded separatrices.

## [02]-[TERMINATION]

- Owner: `Termination` `[Union]` mints one closed stop vocabulary the tracer evaluates at each accepted step; `TraceEventKind` is the one event-cause vocabulary, endpoint-versus-bracket posture deriving from `Points` and `Parameter` and dense-output localization from `DenseOutput`, never a second flag; `TraceEvent.Tolerance` carries the residual and position tolerances as one named tuple, values compared only against `Residual` and points only against `Position`.
- Entry: `Termination.Evaluate` folds a total generated `Switch` — scalar and loop stops decide from state alone, an event stop samples its signed value function and runs the localizer.
- Auto: localization refines a bracketed sign change through `DenseOutputSpan.PointAt` onto the high-order curve and falls to the chord where the segment carries no dense span; an endpoint inside tolerance short-circuits to a zero-iteration touch. Every tolerance derives from `Context`, never a bare literal.
- Law: `CriticalCaptureCase`'s nearest-site fold seeds on `Option` and refuses an empty roster typed, so no arm of this owner forges an infinite distance the localizer would bracket against.
- Boundary: `CrossSurfaceCase` admits any constructed `SupportSpace` on the `Spatial/support.md` `Of` gate's closest-capability proof, re-checking the signed reach per hit through `SignedReach` and raising a typed `Unsupported` fault naming the source type; the signed read itself re-solves its own closest hit inside `Domain/evaluation`, so this band pays one extra closest solve per stop test and holds no hit across the boundary. `CriticalCaptureCase` refuses an empty or non-finite site roster at the same gate, so its signed nearest-site function is total before any trace reads it.

## [03]-[TRACE]

- Owner: `TracePolicy` carries the iteration ceiling and localization budget as policy rows, never compiled-in constants; `SpatialIntegration.Module` is the one `IntegrationModule<Point3d, Vector3d>` instance `integrate.md` assigns this consumer; `StreamlineState.Accept`/`Reject` are the immutable fold state's only transitions, each minting the `Option<StepHistory>` the next kernel `Step` reads from the outcome's error and the selected step ratio — the driver owns run history, the stateless stepper returns error plus a suggestion — and a rejection past `RejectBudget` permitted rejections stops the trace.
- Entry: `FlowKernel.Trace<TOut>` is the one trace entrypoint, `TOut` discriminating the projection.
- Auto: `PolylineOf` substitutes the localized event point for the final trail vertex, so emitted geometry ends exactly at the crossing.
- Packages: `Rasm`/Numerics (`RungeKuttaIntegrator`/`IntegrationModule`/`IntegrationStep`/`StepHistory`/`DenseOutputSpan`/`DenseConditions` stepper floor; `ResultProjection`/`ProjectionRow`; `Dimension`/`PositiveMagnitude`/`EpsilonPolicy`), `Rasm`/Spatial (`SupportSpace`; `ScalarField.SampleScalar`), `Rasm`/Domain (`Context`/`Admit`/`ValidityClaim`), LanguageExt.Core (`Fin`/`Option`/`Seq`/`Range.FoldUntil`), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<int>]`), RhinoCommon (`Point3d`/`Vector3d`/`Polyline`/`Curve.ToPolylineCurve`).
- Growth: a new stop condition is one `Termination` case and one `Evaluate` arm, the generated `Switch` breaking every dispatch site loudly; a new event source is one `TraceEventKind` row over the same localizer; a new output shape is one `ProjectionRow`; a bidirectional or multi-seed trace folds over this same entry, never a sibling tracer.
- Law: the event localizer's bisection is one `Range(0, budget).FoldUntil` over an immutable `Fin<RootBracket>` tuple, stopping on localization or the first failure and falling to the midpoint past the budget, with the settled midpoint in one `Option` slot; a crossing test is exact-SIGN opposition through `Sign.Of`, never a magnitude product that can underflow to zero.
- Law: the trace is one `Range(0, MaxIterations).FoldUntil` over an immutable `Fin<StreamlineState>`, the integration step in the fold body, stopping on a terminal state or the first failure; an exhausted range leaves `Stop` absent, which lowers to `MaxIterationsExhausted` at the trace mint. `Trace` receives admitted integrator, termination, and seed from its callers and re-admits none of them; the completed `StreamlineTrace` validates once at the mint, and `ProjectTrace` accepts only traces minted by `Trace`.
- Boundary: every failure routes a typed fault, keeping the tracer total over `Fin`. The run is frame-local and holds no state cell, and `Polyline`/`Curve` project only a `Terminated` trace, so a budget-exhausted trail never masquerades as a completed streamline.

## [04]-[TOPOLOGY]

- Owner: `MorseGraph` is the atlas carrier — cell-indexed `CellComponent`, component-indexed `Site`/`Critical`, arc-indexed `Arc`/`Crossing`, and the `Separatrix` rows — every column frozen at the mint; `FixedPointKind` `[SmartEnum<int>]` closes the critical-point signature vocabulary, and `Critical` carries `None` for a nonrecurrent component the atlas measured no linearization at, absence rather than a fabricated kind; `FlowPartition` is the caller's cell capsule (census, representative, total locate) and `TopologyPolicy` its scale and horizon record; `MorseAtlas` is the static surface.
- Entry: `MorseAtlas.Of<TOut>(VectorField, FlowPartition, PositiveMagnitude, RungeKuttaIntegrator, TopologyPolicy, Context, Option<TracePolicy>)` is the one atlas entrypoint, `TOut` discriminating the projection over the validated `MorseGraph`; a caller supplies field, partition, and scale and never sequences the transition, condensation, classification, separatrix, and projection legs itself.
- Auto: the cell-transition digraph materializes ONCE through `GraphExtensions.ToAdjacencyGraph` and `CondensateStronglyConnected` runs ONCE over it — the condensation's vertices are the component subgraphs, so their enumeration IS the component partition and the census — recurrent wherever a component holds two or more cells and, for a singleton, exactly where its own self-arc says the horizon trapped it — while each condensed edge carries the merged cell transitions its `Crossing` weight counts. Classification linearizes at the component's representative through the `Numerics/calculus` `Nabla.SampleAxes` six-tap stencil and reads the GENERAL `Matrix.DecomposeEigenDetailed` spectrum: a flow Jacobian is not symmetric, so its complex pairs are what separate a centre from a node, and the signature folds spectral-radius-relative against `EpsilonPolicy.SqrtEpsilon`. Separatrices are multi-seed traces over the settled `FlowKernel.Trace<TOut>` — both senses of every real unstable eigendirection of every saddle, each seeded clear of every capture ball — so the band adds zero integration surface.
- Output: `MorseGraph` is the typed evidence under one `ValidityClaim.All` fold gating column alignment, node ranges, and every `Separatrix` row; an atlas failing that fold routes `InvalidResult` rather than publishing a misaligned column, and `Of<TOut>` resolves the admitted sites, the separatrix rows, the condensed arcs as world segments, and the graph itself off the carrier's self row after that single gate.
- Packages: `Rasm`/Numerics (`Nabla.SampleAxes`; `Matrix`/`EigenSolution`/`EigenOrder`; `EpsilonPolicy`; `Dimension`/`PositiveMagnitude`), `Rasm`/Spatial (`VectorField.SampleVector`), `Rasm`/Domain (`Context`/`ValidityClaim`), QuikGraph (`SEdge`/`AdjacencyGraph`/`CondensedEdge`, `GraphExtensions.ToAdjacencyGraph`, `AlgorithmExtensions.CondensateStronglyConnected`, `EdgeExtensions.IsSelfEdge`), LanguageExt.Core (`Fin`/`Option`/`Seq`), Thinktecture.Runtime.Extensions (`[SmartEnum<int>]`), RhinoCommon (`Point3d`/`Vector3d`), BCL inbox (`System.Numerics.Complex`).
- Growth: a new critical signature is one `FixedPointKind` row read off the same spectral fold; a new partition source is one `FlowPartition` value, never a mesh, lattice, or complex type reaching this band; a stable manifold is the unstable manifold of the sign-reversed field the `Spatial/fields` `Scaled` case already mints, so reversal is the caller's field algebra and never a second tracer.
- Exemption: `Condense`'s `component` array is the cell-label scatter the condensation's component subgraphs fill, span-kernel state that dies with the fold that fills it; it is transient inside one `Try.lift` body and leaves only as the frozen `CellComponent` column.
- Boundary: `TopologicalSort` and both bidirectional forms throw `NonAcyclicGraphException` and a flow digraph is cyclic by construction, so the band never composes them — the condensation IS the acyclic product, and QuikGraph failures funnel through `Try.lift`. The band publishes through `MorseAtlas.Of<TOut>` exactly as the tracer publishes through `FlowKernel.Trace<TOut>`, each the one admission-then-projection entry of its lane. A site is a recurrent set's representative sample, never a root-solved zero — refining one to the field's exact zero is the `Solving/solver` functor's — and a separatrix whose horizon runs out carries `None` for its terminal node rather than a fabricated one. The component-label scatter and the Jacobian-assembly loop are the named span-kernel statement exemption.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Linq;
using System.Runtime.InteropServices;
using LanguageExt;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;
using Dimension = Rasm.Numerics.Dimension;
using Matrix = Rasm.Numerics.Matrix;
using RootBracket = (Rhino.Geometry.Point3d A, Rhino.Geometry.Point3d B, double FA, double FB, double TA, double TB,
    LanguageExt.Option<(Rhino.Geometry.Point3d At, double Value, double T)> Localized, int Iterations);

namespace Rasm.Processing;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class StreamlineStopKind {
    public static readonly StreamlineStopKind Terminated = new(key: 0);
    public static readonly StreamlineStopKind RejectBudgetExhausted = new(key: 1);
    public static readonly StreamlineStopKind MaxIterationsExhausted = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class TraceEventKind {
    public static readonly TraceEventKind CrossSurface = new(key: 0);
    public static readonly TraceEventKind RegionThreshold = new(key: 1);
    public static readonly TraceEventKind CriticalCapture = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class FixedPointKind {
    public static readonly FixedPointKind Source = new(key: 0);
    public static readonly FixedPointKind Sink = new(key: 1);
    public static readonly FixedPointKind Saddle = new(key: 2);
    public static readonly FixedPointKind Center = new(key: 3);
    public static readonly FixedPointKind Degenerate = new(key: 4);
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

    internal Fin<Termination> Admit() => Switch(
        state: key,
        stepCountCase: static (_, termination) => Fin.Succ<Termination>(termination),
        arcLengthCase: static (_, termination) => Fin.Succ<Termination>(termination),
        magnitudeFloorCase: static (_, termination) => Fin.Succ<Termination>(termination),
        crossSurfaceCase: static (termination) =>
            Admit.Need(termination.Surface).Map(static _ => (Termination)termination),
        regionThresholdCase: static (termination) =>
            from region in Admit.Need(termination.Region)
            from threshold in Admit.Finite(termination.Threshold)
            select (Termination)termination,
        loopDetectedCase: static (_, termination) => Fin.Succ<Termination>(termination),
        criticalCaptureCase: static (termination) =>
            !termination.Sites.IsEmpty && termination.Sites.ForAll(static site => site.IsValid)
                ? Fin.Succ<Termination>(termination)
                : Fin.Fail<Termination>(new KernelFault.InvalidInput()));
    internal static Fin<Termination> Admit(Termination value) =>
        Admit.Need(value).Bind(termination => termination.Admit());

    internal Fin<(bool Stop, Option<TraceEvent> Event)> Evaluate(StreamlineState state, Vector3d currentSample, Context context) => Switch(
        state: (Field: state, Sample: currentSample, Context: context),
        stepCountCase: static (s, c) => Decision(stop: s.Field.Steps >= c.Count.Value),
        arcLengthCase: static (s, c) => Decision(stop: s.Field.Arc >= c.Length.Value),
        magnitudeFloorCase: static (s, c) => Decision(stop: s.Sample.Length < c.Threshold.Value),
        loopDetectedCase: static (s, c) => Decision(
            stop: s.Field.Trail.Count >= 3
                && toSeq(Enumerable.Range(0, s.Field.Trail.Count - 2))
                    .Exists(i => s.Field.Current.DistanceToSquared(s.Field.Trail[i])
                        <= c.ClosureRadius.Value * c.ClosureRadius.Value)),
        crossSurfaceCase: static (s, c) => EvaluateEvent(
            state: s.Field, kind: TraceEventKind.CrossSurface,
            tolerance: (Residual: s.Context.Absolute.Value, Position: s.Context.Absolute.Value),
            budget: c.LocalizationBudget.Value,
            sample: point =>
                from hit in c.Surface.Closest(sample: point)
                from value in c.Surface.SignedReach(hit: hit)
                    ? c.Surface.SignedDistance(sample: point)
                    : Fin.Fail<double>(new KernelFault.Unsupported(InputType: c.Surface.SourceType, OutputType: typeof(double)))
                select value).Map(@event => (Stop: @event.IsSome, Event: @event)),
        regionThresholdCase: static (s, c) => EvaluateEvent(
            state: s.Field, kind: TraceEventKind.RegionThreshold,
            tolerance: (
                Residual: s.Context.Fractional * Math.Max(1.0, Math.Abs(c.Threshold)),
                Position: s.Context.Absolute.Value),
            budget: c.LocalizationBudget.Value,
            sample: point => c.Region.SampleScalar(sample: point, context: s.Context).Map(value => value - c.Threshold)).Map(@event => (Stop: @event.IsSome, Event: @event)),
        criticalCaptureCase: static (s, c) => EvaluateEvent(
            state: s.Field, kind: TraceEventKind.CriticalCapture,
            tolerance: (Residual: s.Context.Absolute.Value, Position: s.Context.Absolute.Value),
            budget: c.LocalizationBudget.Value,
            sample: point => c.Sites
                .Fold(Option<double>.None, (nearest, site) => Some(nearest.Match(
                    Some: held => Math.Min(val1: held, val2: point.DistanceTo(other: site)),
                    None: () => point.DistanceTo(other: site))))
                .Map(nearest => nearest - c.CaptureRadius.Value)
                .ToFin(Fail: new KernelFault.InvalidResult())).Map(@event => (Stop: @event.IsSome, Event: @event)));

    private static Fin<(bool Stop, Option<TraceEvent> Event)> Decision(bool stop) =>
        Fin.Succ((Stop: stop, Event: Option<TraceEvent>.None));

    private static Fin<Option<TraceEvent>> EvaluateEvent(
        StreamlineState state, TraceEventKind kind, (double Residual, double Position) tolerance,
        int budget, Func<Point3d, Fin<double>> sample) =>
        from currentValue in sample(state.Current)
        from output in state.Trail.Count < 2
            ? EndpointEvent(kind, (state.Current, state.Current, state.Current),
                (currentValue, currentValue, currentValue), 0.0, tolerance)
            : SegmentEvent(previous: state.Trail[state.Trail.Count - 2], current: state.Current, dense: state.Dense, currentValue: currentValue, kind: kind, tolerance: tolerance, budget: budget, sample: sample)
        select output;
    private static Fin<Option<TraceEvent>> SegmentEvent(
        Point3d previous, Point3d current, Option<DenseOutputSpan<Point3d, Vector3d>> dense,
        double currentValue, TraceEventKind kind, (double Residual, double Position) tolerance,
        int budget, Func<Point3d, Fin<double>> sample) =>
        from previousValue in sample(previous)
        from output in Math.Abs(previousValue) <= tolerance.Residual
            ? EndpointEvent(kind, (previous, current, previous), (previousValue, currentValue, previousValue), 0.0, tolerance)
            : Math.Abs(currentValue) <= tolerance.Residual
                ? EndpointEvent(kind, (previous, current, current), (previousValue, currentValue, currentValue), 1.0, tolerance)
                : Sign.Of(previousValue) != Sign.Of(currentValue)
                    ? LocateRoot(previous: previous, current: current, dense: dense, previousValue: previousValue, currentValue: currentValue, kind: kind, tolerance: tolerance, budget: budget, sample: sample).Map(Some)
                    : Fin.Succ(Option<TraceEvent>.None)
        select output;
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
    private static Fin<TraceEvent> LocateRoot(
        Point3d previous, Point3d current, Option<DenseOutputSpan<Point3d, Vector3d>> dense,
        double previousValue, double currentValue, TraceEventKind kind,
        (double Residual, double Position) tolerance, int budget,
        Func<Point3d, Fin<double>> sample) {
        Fin<RootBracket> seed = Fin.Succ((
            A: previous, B: current, FA: previousValue, FB: currentValue, TA: 0.0, TB: 1.0,
            Localized: Option<(Point3d At, double Value, double T)>.None, Iterations: 0));
        Fin<RootBracket> driven = Range(0, budget).FoldUntil(
            seed,
            (state, _) => state.Bind(Halve),
            row => row.State.Match(Succ: active => active.Localized.IsSome, Fail: static _ => true));
        return driven.Bind(bracket =>
            from settled in bracket.Localized.Match(Some: Fin.Succ, None: () => Midpoint(bracket))
            from @event in Math.Abs(settled.Value) <= tolerance.Residual
                || settled.At.DistanceTo(bracket.A) <= tolerance.Position
                || settled.At.DistanceTo(bracket.B) <= tolerance.Position
                ? Fin.Succ(new TraceEvent(
                    Kind: kind, Points: (previous, current, settled.At),
                    Values: (previousValue, currentValue, settled.Value), Parameter: settled.T,
                    Tolerance: tolerance, Iterations: bracket.Iterations,
                    DenseOutput: dense.Map(static span => span.Conditions)))
                : Fin.Fail<TraceEvent>(new KernelFault.InvalidResult())
            select @event);

        Fin<RootBracket> Halve(RootBracket state) {
            double tm = 0.5 * (state.TA + state.TB);
            return from mid in PointAt(previous: previous, current: current, dense: dense, theta: tm)
                   from fm in sample(mid)
                   let hit = Math.Abs(fm) <= tolerance.Residual || mid.DistanceTo(state.A) <= tolerance.Position || mid.DistanceTo(state.B) <= tolerance.Position
                       ? Some((At: mid, Value: fm, T: tm))
                       : Option<(Point3d At, double Value, double T)>.None
                   select Sign.Of(state.FA) != Sign.Of(fm)
                       ? state with { B = mid, FB = fm, TB = tm, Localized = hit, Iterations = state.Iterations + 1 }
                       : state with { A = mid, FA = fm, TA = tm, Localized = hit, Iterations = state.Iterations + 1 };
        }

        Fin<(Point3d At, double Value, double T)> Midpoint(RootBracket bracket) {
            double theta = 0.5 * (bracket.TA + bracket.TB);
            return from at in PointAt(previous: previous, current: current, dense: dense, theta: theta)
                   from value in sample(at)
                   select (At: at, Value: value, T: theta);
        }
    }
    private static Fin<Point3d> PointAt(Point3d previous, Point3d current, Option<DenseOutputSpan<Point3d, Vector3d>> dense, double theta) =>
        dense.Match(
            Some: span => span.PointAt(theta: theta),
            None: () => Acceptance.Value(value: previous + (theta * (current - previous))));
}

// --- [CONSTANTS] -----------------------------------------------------------------------
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

// --- [MODELS] --------------------------------------------------------------------------
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

[StructLayout(LayoutKind.Auto)]
public readonly record struct StreamlineTrace(
    Seq<Point3d> Trail, StreamlineStopKind Stop, int RejectedSteps, double ArcLength,
    double FinalStep, int MethodOrder, Option<int> EmbeddedOrder, Option<double> LastError, double MaxError,
    double MinStep, double MaxStep, Point3d TerminationPoint, Option<TraceEvent> Event) : IValidityEvidence {
    public int AcceptedSteps => Trail.Count - 1;
    public bool IsComplete => Stop.Equals(StreamlineStopKind.Terminated);
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(Trail.Count, 1),
        Trail.ForAll(static point => ValidityClaim.Finite(point)),
        ValidityClaim.Finite(TerminationPoint),
        ValidityClaim.Nonnegative(ArcLength), ValidityClaim.Finite(FinalStep),
        ValidityClaim.Nonnegative(MaxError), ValidityClaim.Ordered(MinStep, MaxStep),
        ValidityClaim.CountAtLeast(RejectedSteps, 0), ValidityClaim.CountAtLeast(MethodOrder, 1),
        EmbeddedOrder.Map(order => order > 0 && order < MethodOrder).IfNone(noneValue: true),
        LastError.Map(double.IsFinite).IfNone(noneValue: true),
        Event.Map(@event => @event.IsValidFor(terminationPoint: TerminationPoint)).IfNone(noneValue: true));
}

public sealed record TopologyPolicy(
    Dimension TransitionSteps, Dimension SeparatrixSteps,
    PositiveMagnitude StencilWidth, PositiveMagnitude CaptureRadius, PositiveMagnitude SeedOffset) {
    private const double SeedFactor = 4.0;

    public static Fin<TopologyPolicy> Of(
        double captureRadius, double stencilWidth,
        Option<Dimension> transitionSteps = default, Option<Dimension> separatrixSteps = default) {
        return from radius in FactoryBridge.Accept<PositiveMagnitude>(candidate: captureRadius)
               from stencil in FactoryBridge.Accept<PositiveMagnitude>(candidate: stencilWidth)
               from offset in FactoryBridge.Accept<PositiveMagnitude>(candidate: SeedFactor * radius.Value)
               select new TopologyPolicy(
                   TransitionSteps: transitionSteps.IfNone(Dimension.Create(value: 64)),
                   SeparatrixSteps: separatrixSteps.IfNone(Dimension.Create(value: 4_096)),
                   StencilWidth: stencil, CaptureRadius: radius, SeedOffset: offset);
    }
}

public sealed record FlowPartition(Dimension Cells, Func<int, Point3d> Representative, Func<Point3d, Option<int>> Locate) {
    public static Fin<FlowPartition> Of(Dimension cells, Func<int, Point3d> representative, Func<Point3d, Option<int>> locate) {
        return from _ in guard(representative is not null && locate is not null, new KernelFault.InvalidInput()).ToFin()
               select new FlowPartition(cells, representative,
                   point => locate(point).Filter(cell => cell >= 0 && cell < cells.Value));
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct Separatrix(
    int From, Option<int> To, Point3d Terminal, Seq<Point3d> Trail, StreamlineStopKind Stop) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(From, 0),
        ValidityClaim.CountAtLeast(Trail.Count, 1),
        Trail.ForAll(static point => ValidityClaim.Finite(point)),
        ValidityClaim.Finite(Terminal),
        To.Map(static node => node >= 0).IfNone(noneValue: true),
        Stop is not null);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct MorseGraph(
    Seq<int> CellComponent, Seq<Point3d> Site, Seq<Option<FixedPointKind>> Critical,
    Seq<(int From, int To)> Arc, Seq<int> Crossing, Seq<Separatrix> Separatrices) : IValidityEvidence {
    public bool IsValid {
        get {
            int nodes = Site.Count;
            return ValidityClaim.All(
                ValidityClaim.CountAtLeast(nodes, 1),
                ValidityClaim.CountExactly(Critical.Count, nodes),
                ValidityClaim.CountExactly(Crossing.Count, Arc.Count),
                Site.ForAll(static site => ValidityClaim.Finite(site)),
                CellComponent.ForAll(node => node >= 0 && node < nodes),
                Arc.ForAll(arc => arc.From != arc.To
                    && arc.From >= 0 && arc.From < nodes && arc.To >= 0 && arc.To < nodes),
                Crossing.ForAll(static count => count >= 1),
                Separatrices.ForAll(row => row.IsValid && row.From < nodes
                    && row.To.Map(node => node < nodes).IfNone(noneValue: true)));
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal readonly record struct StreamlineState(
    Seq<Point3d> Trail, Point3d Current, double H, double Arc, int Steps, int Rejects, int RejectedSteps,
    double MinStep, double MaxStep, Option<StepHistory> History, double MaxError,
    Option<DenseOutputSpan<Point3d, Vector3d>> Dense, Option<TraceEvent> Event, Option<StreamlineStopKind> Stop) {
    internal StreamlineState Accept(IntegrationStep<Point3d, Vector3d>.AcceptedCase accepted) =>
        Advance(suggested: accepted.SuggestedStep, error: accepted.Error) with {
            Trail = Trail.Add(accepted.Next), Current = accepted.Next,
            Arc = Arc + accepted.Next.DistanceTo(other: Current), Steps = Steps + 1, Rejects = 0,
            Dense = Some(accepted.Dense),
        };
    internal StreamlineState Reject(IntegrationStep<Point3d, Vector3d>.RejectedCase rejected, int rejectBudget) =>
        Advance(suggested: rejected.SuggestedStep, error: Some(rejected.Error)) with {
            Rejects = Rejects + 1, RejectedSteps = RejectedSteps + 1, Dense = Option<DenseOutputSpan<Point3d, Vector3d>>.None,
            Stop = Rejects + 1 > rejectBudget ? Some(StreamlineStopKind.RejectBudgetExhausted) : Stop,
        };
    private StreamlineState Advance(double suggested, Option<double> error) =>
        this with {
            H = suggested, MinStep = Math.Min(MinStep, suggested), MaxStep = Math.Max(MaxStep, suggested),
            History = error.Map(value => new StepHistory(value, suggested / H)), MaxError = Math.Max(MaxError, error.IfNone(0.0)),
        };
}

internal static class FlowKernel {
    internal static Fin<TOut> Trace<TOut>(
        VectorField source, Point3d seed, PositiveMagnitude initialStep,
        RungeKuttaIntegrator integrator, Termination termination, Context context,
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
                from vector in source.SampleVector(sample: current.Current, context: context)
                from decision in termination.Evaluate(state: current, currentSample: vector, context: context)
                from next in decision.Stop
                    ? Fin.Succ(current with { Event = decision.Event, Stop = Some(StreamlineStopKind.Terminated) })
                    : integrator.Step(
                            module: SpatialIntegration.Module,
                            sample: point => source.SampleVector(sample: point, context: context),
                            state: current.Current, h: current.H, history: current.History)
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
        from valid in trace.IsValid ? Fin.Succ(trace) : Fin.Fail<StreamlineTrace>(new KernelFault.InvalidResult())
        from output in ProjectTrace<TOut>(valid)
        select output;

    internal static Fin<TOut> ProjectTrace<TOut>(StreamlineTrace trace) =>
        ResultProjection.Rows<StreamlineTrace, TOut>(self: trace,
            ProjectionRow.Of<Seq<Point3d>>(() => Fin.Succ(trace.Trail)),
            ProjectionRow.Of<Polyline>(() => trace.IsComplete ? PolylineOf(trace) : Fin.Fail<Polyline>(new KernelFault.InvalidResult())),
            ProjectionRow.Of<Curve>(() => trace.IsComplete
                ? PolylineOf(trace).Bind(polyline => Optional(polyline.ToPolylineCurve()).ToFin(new KernelFault.InvalidResult()).Map(static curve => (Curve)curve))
                : Fin.Fail<Curve>(new KernelFault.InvalidResult())));

    private static Fin<Polyline> PolylineOf(StreamlineTrace trace) {
        Point3d[] points = trace.Event.Match(
            Some: @event => trace.Trail.AsIterable().Select((point, index) => index == trace.Trail.Count - 1 ? @event.Points.Localized : point).ToArray(),
            None: () => [.. trace.Trail.AsIterable()]);
        Polyline polyline = [.. points];
        return polyline.IsValid ? Acceptance.Value(value: polyline) : Fin.Fail<Polyline>(new KernelFault.InvalidResult());
    }
}

internal static class MorseAtlas {
    internal static Fin<TOut> Of<TOut>(
        VectorField source, FlowPartition partition, PositiveMagnitude initialStep, RungeKuttaIntegrator integrator,
        TopologyPolicy policy, Context context, Option<TracePolicy> tracePolicy = default) =>
        from seeds in toSeq(Enumerable.Range(0, partition.Cells.Value))
            .TraverseM(cell => Acceptance.Value(partition.Representative(cell))).As()
        let horizon = (Termination)new Termination.StepCountCase(policy.TransitionSteps)
        from arcs in seeds.Map((seed, cell) => (Seed: seed, Cell: cell))
            .TraverseM(row => Transitions(source, partition, row.Cell, row.Seed, initialStep,
                integrator, horizon, context, tracePolicy)).As()
            .Map(static chunks => chunks.Bind(static chunk => chunk))
        from condensed in Condense(arcs, partition.Cells.Value)
        let census = condensed.Census
        let sites = census.Map(row => seeds[row.Cell])
        from critical in census.Map((row, node) => (Row: row, Site: sites[node]))
            .TraverseM(entry => entry.Row.Recurrent
                ? CriticalAt(source, entry.Site, policy, context)
                    .Map(row => (Kind: Some(row.Kind), row.Unstable))
                : Fin.Succ((Kind: Option<FixedPointKind>.None, Unstable: Seq<Vector3d>())))
            .As()
        from separatrices in SeparatrixRows(source: source, partition: partition, sites: sites, component: condensed.Component,
            critical: critical, initialStep: initialStep, integrator: integrator, policy: policy, context: context)
        let atlas = new MorseGraph(
            CellComponent: condensed.Component, Site: sites, Critical: critical.Map(static row => row.Kind),
            Arc: condensed.Arc, Crossing: condensed.Crossing, Separatrices: separatrices)
        from valid in atlas.IsValid ? Fin.Succ(atlas) : Fin.Fail<MorseGraph>(new KernelFault.InvalidResult())
        from output in ResultProjection.Rows<MorseGraph, TOut>(self: valid,
            ProjectionRow.Of<Seq<Point3d>>(() => Fin.Succ(valid.Site)),
            ProjectionRow.Of<Seq<Separatrix>>(() => Fin.Succ(valid.Separatrices)),
            ProjectionRow.Of<Seq<Line>>(() => Fin.Succ(valid.Arc.Map(arc =>
                new Line(from: valid.Site[arc.From], to: valid.Site[arc.To])))))
        select output;

    private static Fin<Seq<SEdge<int>>> Transitions(
        VectorField source, FlowPartition partition, int origin, Point3d seed, PositiveMagnitude initialStep,
        RungeKuttaIntegrator integrator, Termination termination, Context context, Option<TracePolicy> policy) =>
        FlowKernel.Trace<Seq<Point3d>>(source, seed, initialStep, integrator, termination, context, policy)
            .Map(trail => trail.Fold((Cells: Seq<int>(), Last: Option<int>.None), (state, point) =>
                partition.Locate(point) is { IsSome: true, Case: int cell } && state.Last != Some(cell)
                    ? (state.Cells.Add(cell), Some(cell))
                    : state).Cells)
            .Map(cells => cells.Count < 2
                ? Seq(new SEdge<int>(origin, origin))
                : cells.Zip(cells.Skip(1), static (from, to) => new SEdge<int>(from, to)));

    private static Fin<(Seq<int> Component, Seq<(int Cell, bool Recurrent)> Census,
        Seq<(int From, int To)> Arc, Seq<int> Crossing)> Condense(Seq<SEdge<int>> arcs, int cells) =>
        Try.lift(() => {
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
        }).Run().Bind(static inner => inner);

    private static Fin<(FixedPointKind Kind, Seq<Vector3d> Unstable)> CriticalAt(
        VectorField source, Point3d site, TopologyPolicy policy, Context context) =>
        from samples in Nabla.SampleAxes<Vector3d>(
            sampler: point => source.SampleVector(sample: point, context: context),
            point: site, eps: policy.StencilWidth.Value)
        let inv2eps = 1.0 / (2.0 * policy.StencilWidth.Value)
        from jacobian in Matrix.Of(rows: Dimension.Create(value: 3), cols: Dimension.Create(value: 3), entries: [
            (samples.X1.X - samples.X0.X) * inv2eps, (samples.Y1.X - samples.Y0.X) * inv2eps, (samples.Z1.X - samples.Z0.X) * inv2eps,
            (samples.X1.Y - samples.X0.Y) * inv2eps, (samples.Y1.Y - samples.Y0.Y) * inv2eps, (samples.Z1.Y - samples.Z0.Y) * inv2eps,
            (samples.X1.Z - samples.X0.Z) * inv2eps, (samples.Y1.Z - samples.Y0.Z) * inv2eps, (samples.Z1.Z - samples.Z0.Z) * inv2eps])
        from eigen in jacobian.DecomposeEigenDetailed()
        from pairs in eigen.PairsIn(expected: EigenOrder.Factorization)
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

    private static Fin<Seq<Separatrix>> SeparatrixRows(
        VectorField source, FlowPartition partition, Seq<Point3d> sites, Seq<int> component,
        Seq<(Option<FixedPointKind> Kind, Seq<Vector3d> Unstable)> critical, PositiveMagnitude initialStep,
        RungeKuttaIntegrator integrator, TopologyPolicy policy, Context context) {
        Termination capture = new Termination.CriticalCaptureCase(sites, policy.CaptureRadius, TracePolicy.Default.LocalizationBudget);
        return critical.Map(static (row, node) => (Row: row, Node: node))
            .Filter(static entry => entry.Row.Kind
                .Map(kind => kind.Equals(FixedPointKind.Saddle))
                .IfNone(false))
            .Bind(entry => entry.Row.Unstable.Bind(direction => Seq(
                (Node: entry.Node, Seed: sites[entry.Node] + (policy.SeedOffset.Value * direction)),
                (Node: entry.Node, Seed: sites[entry.Node] - (policy.SeedOffset.Value * direction)))))
            .TraverseM(seed => Follow(source: source, partition: partition, component: component, node: seed.Node,
                seed: seed.Seed, initialStep: initialStep, integrator: integrator, termination: capture,
                policy: policy, context: context))
            .As();
    }

    private static Fin<Separatrix> Follow(
        VectorField source, FlowPartition partition, Seq<int> component, int node, Point3d seed, PositiveMagnitude initialStep,
        RungeKuttaIntegrator integrator, Termination termination, TopologyPolicy policy, Context context) =>
        FlowKernel.Trace<StreamlineTrace>(source: source, seed: seed, initialStep: initialStep, integrator: integrator,
                termination: termination, context: context,
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
-->

(none)
