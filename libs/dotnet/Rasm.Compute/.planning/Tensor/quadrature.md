# [COMPUTE_QUADRATURE]

Rasm.Compute measured-integration lane composes the kernel `Rasm.Numerics` integration floor whole: batched quadrature accumulates independent-domain refusals, the solver-domain state carrier extends the kernel scalar pair, the adaptive driver returns its cursor and terminal disposition, and Fourier multipliers act over the kernel transform plane.

Kernel `Step` returns one `IntegrationStep` Accepted or Rejected and owns no reject loop, run-level terminal partition, step-underflow floor, total-step budget, or run-level dense trajectory — each is the driver's, and each is load-bearing: budget exhaustion, underflow, and non-finite error all return best-so-far indistinguishable without the marker. The driver also owns the `Option<StepHistory>` the kernel's own `StepController` reads — it mints the pair from the outcome's error and the next step it actually selected after its own caps, stores it on the cursor, and threads it into the next `Step` — because a proportional or Gustafsson rescale is a fact about the RUN and the stepper is stateless by construction, returning error plus a suggestion and never run state.

## [01]-[INDEX]

- [02]-[INTEGRATION_LANE]: batched `Integration.Measure` over the kernel `QuadratureDomain` arity union, the `MeasurePolicy` accuracy budget with its explicit witness intent, and the `QuadratureRun` evidence fold counting rows whose kernel `QuadratureEvidence.Estimate` is absent.
- [03]-[TRAJECTORY_DRIVER]: `FieldState` scaled-norm carrier, `TrajectoryPhase` continue-or-done fold over the kernel `RungeKuttaIntegrator.Step` with its threaded `StepHistory`, `TerminalDisposition` typed partition with its relax axis, and dense-station harvest.
- [04]-[SPECTRAL_OPERATOR]: `Spectral` applies each `SpectralSymbol` multiplier pointwise over direct MathNet packed-real or split arrays under parity-derived Nyquist zeroing.

## [02]-[INTEGRATION_LANE]

- Owner: `MeasurePolicy` owns the accuracy policy; `QuadratureRun` folds kernel `QuadratureEvidence`; `Integration.Measure` returns that run and `Integration.Trace` returns `TrajectoryRun<TState>` directly.
- Cases: `QuadratureDomain` arms arrive from the kernel — `Line` · `Rectangle` · `Cuboid` · `SparseGrid` · `Reference`; `QuadratureRoute` rows `DoubleExponential` · `GaussLegendre` · `GaussKronrod`; `ReferenceElement` rows `Line` · `Tri` · `Tet` · `Quad` · `Hex` · `Wedge` · `Pyramid`. This page adds no arity and no accuracy row.
- Entry: `Integration.Measure(MeasurePolicy policy, params ReadOnlySpan<QuadratureDomain> domains)` absorbs the singular, batch, and empty call in one signature — a moment set, a polynomial-chaos coefficient sweep, and a single element integral are the same call at three arities; `Integration.Trace(TrajectorySpec<TState, TDelta> spec, TrajectoryControl control)` delegates the fold to `[03]`'s `Trajectory.Trace` — the control's own `Spill` column selecting the archived leg — and mints the result over the same scope.
- Auto: batched domains are INDEPENDENT, so the fold is the applicative `Traverse` over `Validation<Error, QuadratureRun>` and a caller sees every refusing domain at once where a monadic fold reports only the first; the carrier alone selects that policy, and `ToFin` is the caller's own short-circuit egress. Kernel `Quadrature.Integrate` already owns the finite guard, the skip budget, the line domain's structural infinite-limit admission through the MathNet facades, the three-tier admission, and the `RequireErrorWitness` verdict, so this lane re-imposes none of them and adds only what the kernel cannot see: the Compute band, the lane scope, and the batch census.
- Result: `QuadratureRun` carries the domain count, the summed skip census, the count of rows whose coupled `Estimate` is absent — unwitnessed —, and the batch's WORST reported channels — the largest error estimate and the smallest cancellation ratio — each `double?` because the wire edge is where absence collapses, and a batch of non-adaptive rows therefore reports honest absence rather than a zero no route measured; `TrajectoryRun<TState>` carries the method and embedded orders, the terminal key with the relax axis its disposition names, the achieved horizon, and the step/reject/sample census.
- Packages: Rasm (project — the kernel integration floor, the archive session, the signal capsule), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new accuracy kernel, arity, or reference domain is one kernel row and reaches this lane with zero edits here; a new outcome column is one field on the owning run result; a new integration modality is one entry on `Integration`.
- Boundary — policy: `Measure` reads the accuracy budget and `Trace` reads the step budget, and the two never merge, because forcing a definite-integral caller to name a minimum step, a step ceiling, and a total-step budget to integrate one rectangle constructs evidence for a run it never starts. The two policies are no longer two RECORDS: the trace budget IS `TrajectoryControl`, which already carried every stepping column and now carries the lane and the optional spill target beside them, so the wrapper that existed only to add two columns to it is deleted — and with it the third same-axis `TracePolicy` spelling this branch held against the kernel's own.
- Boundary — witness intent: the kernel's `QuadratureControl.RequireErrorWitness` defaults TRUE, so every batched domain on a route carrying no error estimate — `DoubleExponential`, `GaussLegendre`, and every `Rectangle`, `Cuboid`, `SparseGrid`, or `Reference` tensor rule — refuses typed rather than returning a success indistinguishable from a converged one. `MeasurePolicy.Default` therefore states its intent EXPLICITLY as `QuadratureControl.Default with { RequireErrorWitness = false }`, and `MeasurePolicy.Witnessed` is the adaptive-route posture that keeps the demand; a caller wanting a witnessed batch routes it through `GaussKronrod`. The count of unwitnessed rows rides the result, so a lane that opted out is readable rather than merely permitted.
- Boundary — reference elements: `ReferenceElement.Rule` elects the smallest owned rule at or above a positive requested order and REFUSES typed when none reaches it, computing the normalized ladder's terminal order locally and carrying that value in the refusal, so this lane never under-integrates behind a success. Those ceilings are the kernel's own construction, not a wish: line 5, triangle 5, tet 2, quad 5, hex 5, wedge 5 (through the degree-5 seven-point triangle leg — a prism is exact to `min(triangle degree, 2n−1)`, so the weaker leg governs), and pyramid 3 (a conical product is bounded by the weaker of its base leg `2n−1` and its height leg `2m−3` after the `(1−z)²` collapse weight). A consumer declaring an element order past its row's ceiling reads that ceiling in the refusal.
- Boundary — composition: `Quadrature.Integrate` is the ONE quadrature call site in this package — a raw `Integrate.GaussLegendre`/`GaussKronrod`/`OnRectangle`/`OnCuboid` call skips the kernel's finite guard, skip budget, and typed evidence and is the deleted form, as is a package-local `QuadratureRoute`/`QuadratureDomain`/`QuadratureRule`/sparse-grid re-declaration. Kernel refusals remain their original `Error`, so consumers read the fault's own code and message without a second vocabulary mirroring `Rasm.Domain` arm for arm.
- Boundary — consumers: `Solver/route#SOLVE_ROUTES` is this lane's standing consumer — `RungeKuttaIntegrator.Step` threads its `StepHistory` through the transient route's own driver, while `QuadratureEvidence.Estimate` presence gates the element-integration result.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record MeasurePolicy(QuadratureControl Accuracy, WorkLane Lane) {
    public static readonly MeasurePolicy Default =
        new(Accuracy: QuadratureControl.Default with { RequireErrorWitness = false }, Lane: WorkLane.Background);

    public static readonly MeasurePolicy Witnessed = new(Accuracy: QuadratureControl.Default, Lane: WorkLane.Background);
}

public sealed record TraceSpill(Stream Sink, HdfArchivePolicy Archive);

public sealed record QuadratureRun(Seq<QuadratureEvidence> Evidence) {
    public int Domains => Evidence.Count;
    public int Skipped => Evidence.Fold(initialState: 0, f: static (sum, row) => sum + row.Skipped);
    public int Unwitnessed => Evidence.Count(static row => !row.Estimate.IsSome);
    public Option<double> ErrorBound =>
        Evidence.Fold(initialState: Option<double>.None, f: static (held, row) => Widen(held: held, next: row.Estimate.Map(static estimate => estimate.Error), pick: static (a, b) => Math.Max(val1: a, val2: b)));
    public Option<double> Conditioning =>
        Evidence.Fold(initialState: Option<double>.None, f: static (held, row) => Widen(held: held, next: row.Estimate.Map(static estimate => estimate.CancellationRatio), pick: static (a, b) => Math.Min(val1: a, val2: b)));
    private static Option<double> Widen(Option<double> held, Option<double> next, Func<double, double, double> pick) =>
        (held, next) switch {
            ({ IsSome: true, Case: double carried }, { IsSome: true, Case: double fresh }) => Some(pick(arg1: carried, arg2: fresh)),
            ({ IsSome: true }, _) => held,
            _ => next,
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Integration {
    public static Validation<Error, QuadratureRun> Measure(MeasurePolicy policy, params ReadOnlySpan<QuadratureDomain> domains) =>
        domains.IsEmpty
            ? Validation<Error, QuadratureRun>.Fail(TensorReason.EmptyOperand.Fault("integration-domains"))
            : Batch(pending: Iterable<QuadratureDomain>.FromSpan(domains), policy: policy);

    public static Fin<TrajectoryRun<TState>> Trace<TState, TDelta>(TrajectorySpec<TState, TDelta> spec, TrajectoryControl control) =>
        Trajectory.Trace(spec: spec, control: control);

    private static Validation<Error, QuadratureRun> Batch(Iterable<QuadratureDomain> pending, MeasurePolicy policy) =>
        pending
            .Traverse(domain => Quadrature.Integrate(domain: domain, control: Some(policy.Accuracy))
                .ToValidation())
            .As()
            .Map(static rows => new QuadratureRun(Evidence: rows.ToSeq()));

}
```

## [03]-[TRAJECTORY_DRIVER]

- Owner: `FieldState` the solver-domain carrier — a time component beside the value slab, so a NON-AUTONOMOUS field integrates on the kernel's autonomous `Step` with no second clock threaded through the fold; `FieldCarrier` the module mint deriving the kernel `IntegrationModule<FieldState, FieldState>` at an accepted state; `TrajectoryControl` the driver policy the kernel's `StepControl` does not carry, holding the lane and the optional spill beside its stepping columns; `TrajectorySpec` the run declaration carrying the station projector column and its once-read width; `TrajectoryPhase` the closed continue-or-done step the run iterates; `RelaxAxis` the knob a retryable terminal names; `TerminalDisposition` the run-level terminal partition; `TrajectoryRun` the run result holding the cursor whole; `Trajectory` the driver itself, its spilled leg landing the `[Stations.Count, width]` chunked station stream through the `Runtime/archive#HDF_ARCHIVE` `ArchiveSession` cursor.
- Cases: `TrajectoryPhase` `Advancing` · `Halted`; `TerminalDisposition` `Converged` · `Relaxable(RelaxAxis)` · `Divergent`; `RelaxAxis` rows steps · step-floor · horizon · stations (4).
- Entry: `Trajectory.Trace(TrajectorySpec<TState, TDelta> spec, TrajectoryControl control)` — the carrier is a type argument, so one driver integrates a scalar ODE on the kernel `IntegrationModule<double, double>.Scalar`, a frequency-domain state on `.ComplexScalar`, and a field slab on `FieldCarrier.Of` with no per-carrier driver copy, and the control's own `Spill` column selects whether the harvest accumulates or streams.
- Auto: `Admit` gates the control, the span, and the ascending in-range station set ONCE and ACCUMULATES all three, so a caller handed a bad horizon and an unsorted station set learns both; the run is then a bounded `RepeatWhile` over the `TrajectoryPhase` step that short-circuits at the first `Halted`. Each advance clamps `h` to the remaining horizon and `MaxStep`, re-mints the carrier at the current state, calls the kernel `Step` with the run's own optional `StepHistory`, and dispatches its `Accepted`/`Rejected` outcome; each arm selects the next step under `MaxStep` and the remaining horizon FIRST and only then mints `StepHistory(error, nextStep / step)` — `Some` exactly when the outcome carried an error — so a `StepController.ProportionalIntegral` or `StepController.Gustafsson` controller reads the previous error beside the scale the run really applied rather than the stepper's uncapped suggestion, and a fixed method's `None` history degrades no memory-bearing law silently. The reject arm reads non-finite error, consecutive-reject budget, and the underflow floor as ONE flattened tuple pattern. Dense stations harvest INSIDE the accepted span through `DenseOutputSpan.PointAt`, one monotone station cursor, so fixed-output-time trajectories never re-trace the field and no span outlives its own step.
- Result: `TrajectoryRun` holds the terminal disposition beside the CURSOR WHOLE — achieved horizon, step and reject census, the optional step history whose `Error` is the last measured error, station tally, and samples are the cursor's own columns and were hand-copied field by field into a parallel record before — plus the kernel's own reject budget; convergence, budget exhaustion, underflow, and a refusing field all return best-so-far and are indistinguishable without the disposition.
- Packages: Rasm (project — the kernel integration floor, the archive session), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new state carrier is one `IntegrationModule` mint at its consumer; a new termination cause is one `TerminalDisposition` case (or one `RelaxAxis` row where the cause is a knob) and one arm the flattened pattern demands; a new integrator or step controller is one kernel `RungeKuttaMethod`/`StepController` row — the driver body never changes.
- Boundary — iteration: the run is a bounded repeat that SHORT-CIRCUITS. Folding over the whole step ceiling is pure and total, and it also executes every one of those iterations for a trajectory that halted at ten; `RepeatWhile(Schedule.recurs(control.MaxSteps), static p => p is Advancing)` is both — the ceiling stays the bound it was, the run costs its own length, and the phase never leaves the result. One phase still `Advancing` when the bound expires IS budget exhaustion, which is what `Settle` reads off it, so the loop carries no terminal of its own.
- Boundary — control: the error target on the kernel integrator's adaptive `Policy` is 1.0 because the SCALE lives in the carrier's `Norm` — a per-component RMS dividing by `atol + rtol·|yᵢ|` BEFORE squaring, so large-magnitude state never overflows the naive squared-sum-then-root and a fixed absolute tolerance never starves a growing solution. Kernel `Norm` reads the delta alone, so the scale rides a carrier re-minted at each ACCEPTED state and never inside the reject retry, and the zero delta is minted once with that carrier rather than re-allocated per accepted step. Time is a state component with unit derivative, never a driver-threaded argument, so the kernel's autonomous `sample` contract holds by construction. `StepControl.Safety` and its required `StepController Controller` column travel together on the kernel row, and `Option<StepHistory>` is the driver's to mint and thread: a control electing a memory-bearing controller and a driver dropping the history is a degradation with no diagnostic, so the history rides the cursor.
- Boundary — terminal: only an inadmissible control, span, or station set faults — every termination SUCCEEDS with its disposition, because mapping budget exhaustion onto `Fin.Fail` destroys the relaxed-criterion retry the partition exists to serve. Retriability is a TYPE, not a bool pair: `Relaxable` carries the `RelaxAxis` naming which knob to move — budget exhaustion relaxes `MaxSteps`, underflow relaxes `MinStep` or the tolerance pair, a refusing field relaxes the horizon, and a refusing interpolant relaxes the station set the accepted span was asked to interpolate — where two independent bools admitted a `(true, true)` state no run can reach and told a caller nothing about what to change. `Divergent` alone is unretryable, because a state the norm cannot read is the field's own divergence and no control value reaches it.
- Boundary — spill: the archive session is the `Runtime/archive#HDF_ARCHIVE` capsule's, so this driver declares its slot and attributes and takes the cursor it hands back. Release brackets the ACQUISITION through `ArchiveSession.Write`, binding to every outcome arm where a `using` inside a result lambda bound it to the success arm alone. The station ordinal IS the chunk ordinal and the cursor holds it, so write-once is structural rather than a monotonicity argument, and the spilled leg accumulates NO sample seq — the stream is its record, so `Samples` is empty by the leg's own construction and `Station` still reports what landed.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RelaxAxis {
    public static readonly RelaxAxis Steps = new("steps");
    public static readonly RelaxAxis StepFloor = new("step-floor");
    public static readonly RelaxAxis Horizon = new("horizon");
    public static readonly RelaxAxis Stations = new("stations");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TerminalDisposition {
    private TerminalDisposition() { }

    public sealed record Converged : TerminalDisposition;
    public sealed record Relaxable(RelaxAxis Axis) : TerminalDisposition;
    public sealed record Divergent : TerminalDisposition;

    public string Key => Switch(
        converged: static _ => "completed",
        relaxable: static r => r.Axis.Key,
        divergent: static _ => "non-finite");

    public Option<RelaxAxis> Relax => Switch(
        converged: static _ => Option<RelaxAxis>.None,
        relaxable: static r => Some(r.Axis),
        divergent: static _ => Option<RelaxAxis>.None);
}

public readonly record struct FieldState(double Time, ImmutableArray<double> Values);

public readonly record struct TrajectorySample<TState>(double Time, TState State);

// --- [MODELS] --------------------------------------------------------------------------
public sealed record TrajectoryControl(
    double AbsoluteTolerance, double RelativeTolerance, double MinStep, double MaxStep, int MaxSteps,
    WorkLane Lane, Option<TraceSpill> Spill) {
    public static readonly TrajectoryControl Default = new(
        AbsoluteTolerance: 1e-9, RelativeTolerance: 1e-6, MinStep: 1e-12, MaxStep: double.PositiveInfinity, MaxSteps: 100_000,
        Lane: WorkLane.Background, Spill: None);

    internal Validation<Error, Unit> Admits =>
        (Tolerance(AbsoluteTolerance, "trajectory-atol"), Tolerance(RelativeTolerance, "trajectory-rtol"),
         Tolerance(MinStep, "trajectory-min-step"), Ceiling())
            .Apply(static (_, _, _, _) => unit).As();

    private static Validation<Error, Unit> Tolerance(double value, string site) =>
        double.IsFinite(value) && value > 0.0 ? unit
        : TensorReason.PolicyInvalid.Fault(site, value.ToString("e3", CultureInfo.InvariantCulture));

    private Validation<Error, Unit> Ceiling() =>
        MaxStep > MinStep && MaxSteps > 0 ? unit
        : TensorReason.PolicyInvalid.Fault("trajectory-ceiling", $"max={MaxStep:e3}:min={MinStep:e3}:steps={MaxSteps}");
}

public sealed record TrajectorySpec<TState, TDelta>(
    RungeKuttaIntegrator Integrator,
    Func<TState, IntegrationModule<TState, TDelta>> Carrier,
    Func<TState, Fin<TDelta>> Field,
    TState Initial,
    double Start,
    double Horizon,
    double FirstStep,
    Seq<double> Stations,
    Func<TState, ReadOnlySpan<double>> Project) {
    public int Width => Project(Initial).Length;
}

public sealed record TrajectoryCursor<TState>(
    double Time, double Step, TState State, int Steps, int Rejects, int Streak,
    Option<StepHistory> History, Seq<TrajectorySample<TState>> Samples, int Station);

[Union]
public abstract partial record TrajectoryPhase<TState> {
    private TrajectoryPhase() { }

    public sealed record Advancing(TrajectoryCursor<TState> Cursor) : TrajectoryPhase<TState>;
    public sealed record Halted(TrajectoryCursor<TState> Cursor, TerminalDisposition Terminal) : TrajectoryPhase<TState>;
}

public sealed record TrajectoryRun<TState>(TrajectoryCursor<TState> Cursor, TerminalDisposition Terminal, int RejectBudget);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FieldCarrier {
    public static IntegrationModule<FieldState, FieldState> Of(FieldState state, TrajectoryControl control) {
        double[] scale = new double[state.Values.Length];
        TensorPrimitives.Abs<double>(state.Values.AsSpan(), scale);
        TensorPrimitives.Multiply<double>(scale, control.RelativeTolerance, scale);
        TensorPrimitives.Add<double>(scale, control.AbsoluteTolerance, scale);
        ImmutableArray<double> bounds = [.. scale];
        FieldState zero = new(0.0, [.. new double[state.Values.Length]]);
        return new IntegrationModule<FieldState, FieldState>(
            Add: static (current, h, delta) => new FieldState(current.Time + (h * delta.Time), Combine(left: current.Values, right: delta.Values, gain: h)),
            Scale: static (factor, delta) => new FieldState(factor * delta.Time, Scaled(delta.Values, factor)),
            Sum: static (left, right) => new FieldState(left.Time + right.Time, Combine(left: left.Values, right: right.Values, gain: 1.0)),
            Norm: delta => ScaledRms(delta: delta.Values, scale: bounds),
            Zero: zero);
    }

    public static Func<FieldState, Fin<FieldState>> Lift(Func<double, ImmutableArray<double>, Fin<ImmutableArray<double>>> field) =>
        state => field(arg1: state.Time, arg2: state.Values).Map(static rate => new FieldState(1.0, rate));

    private static ImmutableArray<double> Combine(ImmutableArray<double> left, ImmutableArray<double> right, double gain) {
        double[] folded = new double[left.Length];
        TensorPrimitives.MultiplyAdd<double>(right.AsSpan(), gain, left.AsSpan(), folded);
        return [.. folded];
    }

    private static ImmutableArray<double> Scaled(ImmutableArray<double> delta, double factor) {
        double[] scaled = new double[delta.Length];
        TensorPrimitives.Multiply<double>(delta.AsSpan(), factor, scaled);
        return [.. scaled];
    }

    private static double ScaledRms(ImmutableArray<double> delta, ImmutableArray<double> scale) {
        double[] normalized = new double[delta.Length];
        TensorPrimitives.Divide<double>(delta.AsSpan(), scale.AsSpan(), normalized);
        return Math.Sqrt(d: TensorPrimitives.SumOfSquares<double>(normalized) / Math.Max(val1: 1, val2: delta.Length));
    }
}

public static class Trajectory {
    public static Fin<TrajectoryRun<TState>> Trace<TState, TDelta>(TrajectorySpec<TState, TDelta> spec, TrajectoryControl control) =>
        Admit(spec: spec, control: control).Bind(seeded => control.Spill.Match(
            None: () => Fin.Succ(Settle(Run(seeded, spec, control, None), spec.Integrator.RejectBudget)),
            Some: spill => Spilled(seeded, spec, control, spill)));

    private static Fin<TrajectoryRun<TState>> Spilled<TState, TDelta>(
        TrajectoryCursor<TState> seeded, TrajectorySpec<TState, TDelta> spec, TrajectoryControl control, TraceSpill spill) =>
        ChunkGrid.Seat(fileDims: [(ulong)spec.Stations.Count, (ulong)spec.Width], chunks: [1u, (uint)spec.Width])
            .Bind(rows => ChunkGrid.Seat(fileDims: [(ulong)spec.Stations.Count], chunks: [(uint)spec.Stations.Count])
                .Map(axis => (Rows: new ArchiveSlot<double>("stations", rows), Axis: new ArchiveSlot<double>("station-axis", axis))))
            .Bind(slots => ArchiveSession.Write(
                spill.Sink, spill.Archive, Seq<IArchiveSlot>(slots.Rows, slots.Axis),
                Seq(("width", (ArchiveAttribute)new ArchiveAttribute.Whole(spec.Width)),
                    ("stations", (ArchiveAttribute)new ArchiveAttribute.Whole(spec.Stations.Count))),
                session => IO.pure(
                    from axis in session.Cursor(slots.Axis)
                    from _ in axis.Write(spec.Stations.ToArray())
                    from cursor in session.Cursor(slots.Rows)
                    select Settle(
                        Run(seeded, spec, control, Some(new StationSink<TState>(cursor, state => spec.Project(state).ToArray()))),
                        spec.Integrator.RejectBudget)))
                .Run());

    private sealed record StationSink<TState>(ChunkCursor<double> Cursor, Func<TState, double[]> Project);

    private static TrajectoryPhase<TState> Run<TState, TDelta>(
        TrajectoryCursor<TState> seeded, TrajectorySpec<TState, TDelta> spec, TrajectoryControl control, Option<StationSink<TState>> spill) =>
        IO.pure((TrajectoryPhase<TState>)new TrajectoryPhase<TState>.Advancing(seeded))
            .Map(phase => phase is TrajectoryPhase<TState>.Advancing advancing
                ? Advance(cursor: advancing.Cursor, spec: spec, control: control, spill: spill)
                : phase)
            .RepeatWhile(Schedule.recurs(control.MaxSteps), static phase => phase is TrajectoryPhase<TState>.Advancing)
            .Run();

    private static Fin<TrajectoryCursor<TState>> Admit<TState, TDelta>(TrajectorySpec<TState, TDelta> spec, TrajectoryControl control) =>
        (control.Admits, Span(spec), Stations(spec))
            .Apply(static (_, _, _) => unit).As().ToFin()
            .Map(_ => new TrajectoryCursor<TState>(
                Time: spec.Start, Step: Math.Min(val1: spec.FirstStep, val2: control.MaxStep), State: spec.Initial,
                Steps: 0, Rejects: 0, Streak: 0, History: None, Samples: [], Station: 0));

    private static Validation<Error, Unit> Span<TState, TDelta>(TrajectorySpec<TState, TDelta> spec) =>
        double.IsFinite(spec.Start) && spec.Horizon > spec.Start && spec.FirstStep > 0.0 && double.IsFinite(spec.FirstStep)
            ? unit
            : TensorReason.PolicyInvalid.Fault("trajectory-span", $"{spec.Start}:{spec.Horizon}:{spec.FirstStep}");

    private static Validation<Error, Unit> Stations<TState, TDelta>(TrajectorySpec<TState, TDelta> spec) =>
        spec.Stations.Zip(spec.Stations.Skip(1)).ForAll(static pair => pair.First < pair.Second)
        && spec.Stations.ForAll(station => station >= spec.Start && station <= spec.Horizon)
            ? unit
            : TensorReason.PolicyInvalid.Fault("trajectory-stations", spec.Stations.Count.ToString(CultureInfo.InvariantCulture));

    private static TrajectoryPhase<TState> Advance<TState, TDelta>(TrajectoryCursor<TState> cursor, TrajectorySpec<TState, TDelta> spec, TrajectoryControl control, Option<StationSink<TState>> spill) {
        double step = Math.Min(val1: Math.Min(val1: cursor.Step, val2: control.MaxStep), val2: spec.Horizon - cursor.Time);
        return spec.Integrator
            .Step(module: spec.Carrier(arg: cursor.State), sample: spec.Field, state: cursor.State, h: step, history: cursor.History)
            .Match(
                Succ: outcome => outcome.Switch(
                    state: (Cursor: cursor, Step: step, Spec: spec, Control: control, Spill: spill),
                    acceptedCase: static (s, accepted) => Accepted(cursor: s.Cursor, step: s.Step, accepted: accepted, spec: s.Spec, control: s.Control, spill: s.Spill),
                    rejectedCase: static (s, rejected) => Rejected(cursor: s.Cursor, step: s.Step, rejected: rejected, spec: s.Spec, control: s.Control)),
                Fail: _ => (TrajectoryPhase<TState>)new TrajectoryPhase<TState>.Halted(cursor, new TerminalDisposition.Relaxable(RelaxAxis.Horizon)));
    }

    private static TrajectoryPhase<TState> Accepted<TState, TDelta>(
        TrajectoryCursor<TState> cursor, double step, IntegrationStep<TState, TDelta>.AcceptedCase accepted,
        TrajectorySpec<TState, TDelta> spec, TrajectoryControl control, Option<StationSink<TState>> spill) {
        double remaining = spec.Horizon - (cursor.Time + step);
        double proposed = Math.Min(accepted.SuggestedStep, control.MaxStep);
        double nextStep = remaining > control.MinStep ? Math.Min(proposed, remaining) : proposed;
        return Harvest(cursor: cursor, at: cursor.Time, step: step, dense: accepted.Dense, stations: spec.Stations, spill: spill).Match(
            Succ: taken => Land(
                cursor: cursor with {
                    Time = cursor.Time + step,
                    Step = nextStep,
                    State = accepted.Next,
                    Steps = cursor.Steps + 1,
                    Streak = 0,
                    History = accepted.Error.Map(error => new StepHistory(error, nextStep / step)),
                    Samples = taken.Samples,
                    Station = taken.Station,
                },
                spec: spec, control: control),
            Fail: _ => (TrajectoryPhase<TState>)new TrajectoryPhase<TState>.Halted(cursor, new TerminalDisposition.Relaxable(RelaxAxis.Stations)));
    }

    private static TrajectoryPhase<TState> Rejected<TState, TDelta>(
        TrajectoryCursor<TState> cursor, double step, IntegrationStep<TState, TDelta>.RejectedCase rejected,
        TrajectorySpec<TState, TDelta> spec, TrajectoryControl control) {
        double remaining = spec.Horizon - cursor.Time;
        double nextStep = Math.Min(Math.Min(rejected.SuggestedStep, control.MaxStep), remaining);
        TrajectoryCursor<TState> next = cursor with {
            Step = nextStep,
            Rejects = cursor.Rejects + 1,
            Streak = cursor.Streak + 1,
            History = Some(new StepHistory(rejected.Error, nextStep / step)),
        };
        return (Finite: double.IsFinite(rejected.Error),
                Within: next.Streak <= spec.Integrator.RejectBudget,
                Above: next.Step >= control.MinStep) switch {
            (Finite: false, _, _) => (TrajectoryPhase<TState>)new TrajectoryPhase<TState>.Halted(next, new TerminalDisposition.Divergent()),
            (_, Within: false, _) => new TrajectoryPhase<TState>.Halted(next, new TerminalDisposition.Relaxable(RelaxAxis.Steps)),
            (_, _, Above: false) => new TrajectoryPhase<TState>.Halted(next, new TerminalDisposition.Relaxable(RelaxAxis.StepFloor)),
            _ => new TrajectoryPhase<TState>.Advancing(next),
        };
    }

    private static TrajectoryPhase<TState> Land<TState, TDelta>(TrajectoryCursor<TState> cursor, TrajectorySpec<TState, TDelta> spec, TrajectoryControl control) =>
        spec.Horizon - cursor.Time <= control.MinStep
            ? (TrajectoryPhase<TState>)new TrajectoryPhase<TState>.Halted(cursor, new TerminalDisposition.Converged())
            : cursor.Step < control.MinStep
                ? new TrajectoryPhase<TState>.Halted(cursor, new TerminalDisposition.Relaxable(RelaxAxis.StepFloor))
                : new TrajectoryPhase<TState>.Advancing(cursor);

    private static Fin<(Seq<TrajectorySample<TState>> Samples, int Station)> Harvest<TState, TDelta>(
        TrajectoryCursor<TState> cursor, double at, double step, DenseOutputSpan<TState, TDelta> dense, Seq<double> stations, Option<StationSink<TState>> spill) =>
        stations.Skip(cursor.Station).TakeWhile(station => station <= at + step)
            .TraverseM(station => dense.PointAt(theta: (station - at) / step).Map(state => new TrajectorySample<TState>(station, state)))
            .As()
            .Bind(taken => spill.Match(
                Some: sink => taken
                    .TraverseM(sample => sink.Cursor.Write(sink.Project(sample.State))).As()
                    .Map(_ => (Samples: cursor.Samples, Station: cursor.Station + taken.Count)),
                None: () => Fin.Succ((Samples: cursor.Samples + taken, Station: cursor.Station + taken.Count))));

    private static TrajectoryRun<TState> Settle<TState>(TrajectoryPhase<TState> phase, int budget) => phase.Switch(
        state: budget,
        advancing: static (b, a) => new TrajectoryRun<TState>(a.Cursor, new TerminalDisposition.Relaxable(RelaxAxis.Steps), b),
        halted: static (b, h) => new TrajectoryRun<TState>(h.Cursor, h.Terminal, b));
}
```

## [04]-[SPECTRAL_OPERATOR]

- Owner: `SpectralSymbol` the `[SmartEnum<string>]` Fourier-multiplier vocabulary carrying each operator's generated symbol column and its parity; `SymbolParity` the composing parity policy; `Spectral` the pointwise-product composition carrier; `WaveAxis` the wavenumber owner over direct `Fourier.FrequencyScale`; `SpectralControl` the imaginary-residual band; `SpectralPlane` the two-case carrier the parity of the field length selects; `SpectralOperator.Apply` the forward-multiply-inverse application with its Hermitian gate.
- Cases: `SpectralSymbol` rows derivative (`i·k`), laplacian (`−k²`), biharmonic (`k⁴`), hilbert (`−i·sgn k`), anti-derivative (`1/(i·k)`, zero mode killed) (5); `SymbolParity` rows even · odd (2); `SpectralPlane` cases `Packed(double[], int)` · `Split(double[], double[])` (2).
- Entry: `SpectralOperator.Apply(ReadOnlySpan<double> field, WaveAxis axis, Spectral op, Option<SpectralControl> control)` — the composition is the value, so a chained operator and a single symbol are the same call.
- Auto: `Spectral.At(k)` is the pointwise product of factor rows and `Spectral.Parity` the XOR-fold of factor parities, so the Nyquist bin zeroes exactly when the composition is odd. Even lengths ride `Fourier.ForwardReal`/`InverseReal`; odd lengths ride the split `Fourier.Forward`/`Inverse` pair whose imaginary-residual gate diagnoses broken symmetry. Wavenumbers derive from `Fourier.FrequencyScale` and both inverse paths divide by the sample count under `FourierOptions.NoScaling`.
- Result: `SpectralEvidence` carries the real result as owned `ImmutableArray<double>`, the composed parity, and `Option<double>` imaginary residual — `Some` only on the split leg where the gate read it, `None` on the packed leg whose realness is structural; excess residual fails the split leg as broken Hermitian symmetry, never a usable result.
- Packages: MathNet.Numerics (`Fourier.ForwardReal`/`InverseReal`, split `Forward`/`Inverse`, `Fourier.FrequencyScale`, `FourierOptions.NoScaling`), Rasm (`Numerics/atoms` `Dimension`/`PositiveMagnitude`/`SignedAxis`), System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new operator is one `SpectralSymbol` row with its generated symbol column and parity; a composite is a `Spectral.Then` chain; a tighter Hermitian band is one `SpectralControl` value.
- Boundary — symbols: every constant-coefficient periodic operator is one `SpectralSymbol` row applied pointwise to the forward transform; symbols compose by pointwise multiplication before a single inverse, and parity is row data the operator owns, never a `bool oddOrder` knob nor a bare `Func<double, Complex>` riding beside the call. `ZeroesNyquist` is DERIVED from the parity row rather than kept as a column beside it: an odd symbol is discontinuous across ±Nyquist and therefore zeroes that bin, which is a fact about the row, not a second value that can disagree with it.
- Boundary — transform: the local parity-selected packed or split carrier calls MathNet directly. The lane passes `FourierOptions.NoScaling` because it reads between the legs: an unscaled forward leaves true DFT coefficients for the operator symbol, and the inverse divides by the sample count exactly once on egress.
- Boundary — gate: `SpectralControl` binds the SPLIT leg ALONE, and the discriminant is field PARITY, never caller intent. Even-length fields ride the packed arena whose output is real by construction — no imaginary channel to measure, the floor goes unread, and the evidence reports `None`; passing a tighter floor with an even grid changes nothing the run does. That asymmetry is why the residual is `Option<double>`, never a `0.0` written by both legs — a reader treating a missing residual as a passed gate has inverted the one leg that proves Hermitian symmetry. The control arrives as an `Option`, so absence is a carrier rather than a nullable reference crossing a public boundary. The residual denominator floors at the smallest NORMAL double rather than at `double.Epsilon`, whose value is the smallest subnormal and which therefore names a quantity a hundred orders of magnitude below the guard it was standing in for.
- Boundary — discriminant against the `Stats/signal#SIGNAL_LANE` `SpectralTransform` axis is spatial-versus-sampled, never availability: a symbol is a differential operator over a SPATIAL extent in angular wavenumber, that axis transform-and-invert, framing, and windowing over a SAMPLE RATE in bin frequency. Collapsing either end hands a spatial operator frame, hop, and window evidence it has none of, or a spectrogram a parity column no transform owns.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SymbolParity {
    public static readonly SymbolParity Even = new("even");
    public static readonly SymbolParity Odd = new("odd");

    public bool ZeroesNyquist => this == Odd;

    public SymbolParity Compose(SymbolParity other) => this == other ? Even : Odd;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpectralSymbol {
    public static readonly SpectralSymbol Derivative = new("derivative", at: static k => new Complex(0.0, k), parity: SymbolParity.Odd);
    public static readonly SpectralSymbol Laplacian = new("laplacian", at: static k => new Complex(-k * k, 0.0), parity: SymbolParity.Even);
    public static readonly SpectralSymbol Biharmonic = new("biharmonic", at: static k => new Complex(k * k * k * k, 0.0), parity: SymbolParity.Even);
    public static readonly SpectralSymbol Hilbert = new("hilbert", at: static k => new Complex(0.0, -Math.Sign(value: k)), parity: SymbolParity.Odd);
    public static readonly SpectralSymbol AntiDerivative = new("anti-derivative", at: static k => k == 0.0 ? Complex.Zero : new Complex(0.0, -1.0 / k), parity: SymbolParity.Odd);

    public SymbolParity Parity { get; }

    [UseDelegateFromConstructor] public partial Complex At(double wavenumber);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpectralPlane {
    private SpectralPlane() { }

    public sealed record Packed(double[] Values, int Samples) : SpectralPlane;
    public sealed record Split(double[] Real, double[] Imaginary) : SpectralPlane;

    public static SpectralPlane Of(ReadOnlySpan<double> field) {
        int n = field.Length;
        if (int.IsEvenInteger(n)) {
            double[] packed = new double[n + 2];
            field.CopyTo(packed);
            return new Packed(packed, n);
        }

        double[] real = field.ToArray();
        return new Split(real, new double[n]);
    }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct WaveAxis(int Length, double Extent) {
    public Fin<PositiveMagnitude> Rate =>
        Length >= 2 && double.IsFinite(Extent) && Extent > 0.0
            ? Fin.Succ(PositiveMagnitude.Create(value: Length / Extent))
            : TensorReason.PolicyInvalid.Fail<PositiveMagnitude>("wave-axis", $"length={Length}:extent={Extent:e3}");

    public Option<int> Nyquist => int.IsEvenInteger(Length) ? Some(Length >> 1) : None;

    public Fin<double[]> Wavenumbers() => Rate.Map(rate => {
        double[] k = MathNet.Numerics.IntegralTransforms.Fourier.FrequencyScale(Length, rate.Value);
        TensorPrimitives.Multiply<double>(k, 2.0 * Math.PI, k);
        return k;
    });
}

public sealed record Spectral {
    private Spectral(Seq<SpectralSymbol> factors) => Factors = factors;

    public Seq<SpectralSymbol> Factors { get; }

    public static Spectral Of(SpectralSymbol symbol) => new(Seq(symbol));

    public Spectral Then(SpectralSymbol next) => new(Factors.Add(next));

    public SymbolParity Parity => Factors.Fold(SymbolParity.Even, static (acc, factor) => acc.Compose(other: factor.Parity));

    public Complex At(double wavenumber) => Factors.Fold(Complex.One, (acc, factor) => acc * factor.At(wavenumber: wavenumber));
}

public sealed record SpectralControl(double ImaginaryFloor) {
    public static readonly SpectralControl Default = new(ImaginaryFloor: 1e-8);
}

public sealed record SpectralEvidence(ImmutableArray<double> Field, Option<double> ImaginaryResidual, SymbolParity Parity);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SpectralOperator {
    public static Fin<SpectralEvidence> Apply(ReadOnlySpan<double> field, WaveAxis axis, Spectral op, Option<SpectralControl> control) {
        if (field.Length != axis.Length || !TensorPrimitives.IsFiniteAll<double>(field)) {
            return TensorReason.ShapeMismatch.Fail<SpectralEvidence>("wave-axis-mismatch", $"field={field.Length}:axis={axis.Length}");
        }

        Fin<PositiveMagnitude> rate = axis.Rate;
        if (rate.Case is not PositiveMagnitude) { return rate.Map(static _ => default(SpectralEvidence)!); }
        SpectralPlane plane = SpectralPlane.Of(field);
        return Try.lift(() => plane.Switch(
                packed: static p => { MathNet.Numerics.IntegralTransforms.Fourier.ForwardReal(p.Values, p.Samples, MathNet.Numerics.IntegralTransforms.FourierOptions.NoScaling); return unit; },
                split: static s => { MathNet.Numerics.IntegralTransforms.Fourier.Forward(s.Real, s.Imaginary, MathNet.Numerics.IntegralTransforms.FourierOptions.NoScaling); return unit; }))
            .Run()
            .Bind(_ => axis.Wavenumbers().Map(k => Modulated(plane, op, k, axis.Nyquist)))
            .Bind(_ => Try.lift(() => plane.Switch(
                packed: static p => { MathNet.Numerics.IntegralTransforms.Fourier.InverseReal(p.Values, p.Samples, MathNet.Numerics.IntegralTransforms.FourierOptions.NoScaling); return unit; },
                split: static s => { MathNet.Numerics.IntegralTransforms.Fourier.Inverse(s.Real, s.Imaginary, MathNet.Numerics.IntegralTransforms.FourierOptions.NoScaling); return unit; })).Run())
            .Bind(_ => Settled(plane, op, axis, control.IfNone(SpectralControl.Default)));
    }

    static Unit Modulated(SpectralPlane plane, Spectral op, double[] k, Option<int> nyquist) => plane.Switch(
        state: (K: k, Nyquist: nyquist),
        packed: static (s, p) => {
            int half = p.Samples >> 1;
            for (int bin = 0; bin <= half; bin++) {
                Complex scaled = new Complex(p.Values[2 * bin], p.Values[(2 * bin) + 1]) * Factor(s.K, s.Nyquist, bin);
                (p.Values[2 * bin], p.Values[(2 * bin) + 1]) = (scaled.Real, scaled.Imaginary);
            }

            return unit;
        },
        split: static (s, sp) => {
            for (int bin = 0; bin < sp.Real.Length; bin++) {
                Complex scaled = new Complex(sp.Real[bin], sp.Imaginary[bin]) * Factor(s.K, s.Nyquist, bin);
                (sp.Real[bin], sp.Imaginary[bin]) = (scaled.Real, scaled.Imaginary);
            }

            return unit;
        });

    static Complex Factor(Spectral op, double[] k, Option<int> nyquist, int bin) =>
        op.Parity.ZeroesNyquist && nyquist == Some(bin) ? Complex.Zero : op.At(wavenumber: k[bin]);

    static Fin<SpectralEvidence> Settled(SpectralPlane plane, Spectral op, WaveAxis axis, SpectralControl control) {
        double[] result = new double[axis.Length];
        return plane.Switch(
            state: (Axis: axis, Result: result, Factor: (double)axis.Length),
            packed: static (s, p) => {
                p.Values.AsSpan(0, s.Axis.Length).CopyTo(s.Result);
                TensorPrimitives.Divide<double>(s.Result, s.Factor, s.Result);
                return Fin.Succ(new SpectralEvidence([.. s.Result], None, default!));
            },
            split: (s, sp) => {
                sp.Real.AsSpan(0, s.Axis.Length).CopyTo(s.Result);
                TensorPrimitives.Divide<double>(s.Result, s.Factor, s.Result);
                double real = TensorPrimitives.MaxMagnitude<double>(sp.Real);
                double imaginary = TensorPrimitives.MaxMagnitude<double>(sp.Imaginary);
                double residual = Math.Abs(imaginary) / Math.Max(Math.Abs(real), double.MinNormal);
                return residual < control.ImaginaryFloor
                    ? Fin.Succ(new SpectralEvidence([.. s.Result], Some(residual), default!))
                    : TensorReason.WitnessFail.Fail<SpectralEvidence>("imaginary-residual", $"r={residual:e3}", $"floor={control.ImaginaryFloor:e3}");
            })
            .Map(evidence => evidence with { Parity = op.Parity });
    }
}
```
