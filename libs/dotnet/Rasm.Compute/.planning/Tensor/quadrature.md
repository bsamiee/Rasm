# [COMPUTE_QUADRATURE]

Rasm.Compute measured-integration lane over the kernel `Rasm.Numerics` integration floor: the accuracy-routed quadrature owner, the Runge-Kutta step algebra, and the spectral transform plane all arrive settled from `Rasm/Numerics/integrate` and `Rasm/Numerics/transform` and are composed whole — package-local re-declarations of those kernel surfaces are the deleted form. This page owns what sits ABOVE that floor, exactly as the folder ruling partitions it: the batched quadrature entry whose independent domains accumulate every refusal, the solver-domain state carrier the kernel's `Scalar`/`ComplexScalar` pair does not reach, the adaptive DRIVER the kernel deliberately leaves to its consumer, the Fourier-MULTIPLIER vocabulary the transform band takes as a symbol span, and the `ComputeReceipt`/`ComputeFault` projection every Compute outcome crosses.

Kernel `Step` returns one `IntegrationStep` Accepted or Rejected and owns no reject loop, run-level terminal partition, step-underflow floor, total-step budget, or run-level dense trajectory — each is the driver's, and each is load-bearing: budget exhaustion, underflow, and non-finite error all return best-so-far indistinguishable without the marker. The driver also owns the `StepHistory` the kernel's own `StepLaw` reads, because a proportional or Gustafsson rescale is a fact about the RUN and the stepper is stateless by construction.

## [01]-[INDEX]

- [02]-[INTEGRATION_LANE]: batched `Integration.Measure` over the kernel `IntegrationDomain` arity union, the `MeasurePolicy` accuracy budget with its explicit witness intent, the `QuadratureRun` evidence fold over the kernel `ConvergenceClaim`, and the `ComputeReceipt`/`ComputeFault` projection both legs cross.
- [03]-[TRAJECTORY_DRIVER]: `FieldState` scaled-norm carrier, `TrajectoryPhase` continue-or-done fold over the kernel `FieldIntegrator.Step` with its threaded `StepHistory`, `TerminalDisposition` typed partition with its relax axis, and dense-station harvest.
- [04]-[SPECTRAL_OPERATOR]: `Spectral` applies each `SpectralSymbol` multiplier pointwise over the kernel `SpectralArena` under parity-derived Nyquist zeroing — the packed-real arena on even lengths, the split arena with its imaginary-residual gate on odd.

## [02]-[INTEGRATION_LANE]

- Owner: `MeasurePolicy` the lane's one accuracy policy — the kernel `QuadratureControl` budget beside the `WorkLane` its receipt scopes to; `QuadratureRun` the batch evidence fold over the kernel `QuadratureEvidence` rows and their `ConvergenceClaim` verdicts; `Integration` the entry pair — `Measure` for definite integrals over the kernel `IntegrationDomain` arity union, `Trace` for initial-value trajectories — sharing one fault projection and one receipt mint; `ComputeReceipt.Quadrature` and `ComputeReceipt.Trajectory` the two partial cases this page declares on the `Runtime/receipts#RECEIPT_UNION` union.
- Cases: `IntegrationDomain` arms arrive from the kernel — `Line` · `Rectangle` · `Cuboid` · `SparseGrid` · `Simplex`; `QuadratureRoute` rows `DoubleExponential` · `GaussLegendre` · `GaussKronrod`; `ConvergenceClaim` rows `Estimated` · `Unwitnessed`; `ReferenceElement` rows `Line` · `Tri` · `Tet` · `Quad` · `Hex` · `Wedge` · `Pyramid`. This page adds no arity and no accuracy row.
- Entry: `Integration.Measure(MeasurePolicy policy, Op key, params ReadOnlySpan<IntegrationDomain> domains)` absorbs the singular, batch, and empty call in one signature — a moment set, a polynomial-chaos coefficient sweep, and a single element integral are the same call at three arities; `Integration.Trace(TrajectorySpec<TState, TDelta> spec, TrajectoryControl control, Op key)` delegates the fold to `[03]`'s `Trajectory.Trace` — the control's own `Spill` column selecting the archived leg — and mints the receipt over the same scope.
- Auto: batched domains are INDEPENDENT, so the fold is the applicative `Traverse` over `Validation<Error, QuadratureRun>` and a caller sees every refusing domain at once where a monadic fold reports only the first; the carrier alone selects that policy, and `ToFin` is the caller's own short-circuit egress. Kernel `Quadrature.Integrate` already owns the finite guard, the skip budget, the infinite-bound capability gate, the three-tier admission, and the `RequireErrorWitness` verdict, so this lane re-imposes none of them and adds only what the kernel cannot see: the Compute band, the lane scope, and the batch census.
- Receipt: `ComputeReceipt.Quadrature` carries the domain count, the summed skip census, the count of rows whose `ConvergenceClaim` is `Unwitnessed`, and the batch's WORST reported channels — the largest error estimate and the smallest cancellation ratio — each `double?` because the wire edge is where absence collapses, and a batch of non-adaptive rows therefore reports honest absence rather than a zero no route measured; `ComputeReceipt.Trajectory` carries the method and embedded orders, the terminal key with the relax axis its disposition names, the achieved horizon, and the step/reject/sample census.
- Packages: Rasm (project — the kernel integration floor, the archive session, the signal capsule), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new accuracy kernel, arity, or reference domain is one kernel row and reaches this lane with zero edits here; a new outcome column is one field on the owning receipt case; a new integration modality is one entry on `Integration` sharing the same projection pair — zero new surface.
- Boundary — policy: `Measure` reads the accuracy budget and `Trace` reads the step budget, and the two never merge, because forcing a definite-integral caller to name a minimum step, a step ceiling, and a total-step budget to integrate one rectangle constructs evidence for a run it never starts. The two policies are no longer two RECORDS: the trace budget IS `TrajectoryControl`, which already carried every stepping column and now carries the lane and the optional spill target beside them, so the wrapper that existed only to add two columns to it is deleted — and with it the third same-axis `TracePolicy` spelling this branch held against the kernel's own.
- Boundary — witness intent: the kernel's `QuadratureControl.RequireErrorWitness` defaults TRUE, so every batched domain on a route carrying no error estimate — `DoubleExponential`, `GaussLegendre`, and every `Rectangle`, `Cuboid`, `SparseGrid`, or `Simplex` tensor rule — refuses typed rather than returning a success indistinguishable from a converged one. `MeasurePolicy.Default` therefore states its intent EXPLICITLY as `QuadratureControl.Default with { RequireErrorWitness = false }`, and `MeasurePolicy.Witnessed` is the adaptive-route posture that keeps the demand; a caller wanting a witnessed batch routes it through `GaussKronrod`. The count of unwitnessed rows rides the receipt, so a lane that opted out is readable rather than merely permitted.
- Boundary — reference elements: `ReferenceElement.Rule` elects the smallest owned rule at or above the requested order and REFUSES typed against its own `Ceiling` when none reaches it, so this lane never under-integrates behind a success. Those ceilings are the kernel's own construction, not a wish: line 3, triangle 5, tet 2, quad 5, hex 5, wedge 5 (through the degree-5 seven-point triangle leg — a prism is exact to `min(triangle degree, 2n−1)`, so the weaker leg governs), and pyramid 2 (a conical product is bounded by its base directions after the collapse weight). A consumer declaring an element order past its row's ceiling reads that ceiling in the refusal.
- Boundary — composition: `Quadrature.Integrate` is the ONE quadrature call site in this package — a raw `Integrate.GaussLegendre`/`GaussKronrod`/`OnRectangle`/`OnCuboid` call skips the kernel's finite guard, skip budget, and typed evidence and is the deleted form, as is a package-local `QuadratureRoute`/`IntegrationDomain`/`QuadratureRule`/`SmolyakCubature` re-declaration. Kernel refusals remain their original `Error`, so consumers read the fault's own code and message without a second vocabulary mirroring `Rasm.Domain` arm for arm; a bare token discarding that evidence is the rejected flatten. Both legs are managed host-local folds, so the receipt scopes to `Substrate.CpuTensor` and a device row here claims residency this lane never acquires. Elapsed time is MEASURED, never accepted: the receipt mint takes the `IClock` the composition already threads and brackets the run, where a caller-supplied `Duration` let a lane report a figure nothing timed.
- Boundary — consumers: `Solver/route#SOLVE_ROUTES` is this lane's standing consumer — `FieldIntegrator.Step` threading its `StepHistory` through the transient route's own driver, and `QuadratureEvidence.Claim` gating the element-integration receipt — so the trajectory driver and the quadrature fold both reach a real caller rather than sitting as an unreached surface beside an unread receipt case.

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
    public int Unwitnessed => Evidence.Count(static row => row.Claim == ConvergenceClaim.Unwitnessed);
    public Option<double> ErrorBound =>
        Evidence.Fold(initialState: Option<double>.None, f: static (held, row) => Widen(held: held, next: row.Error, pick: static (a, b) => Math.Max(val1: a, val2: b)));
    public Option<double> Conditioning =>
        Evidence.Fold(initialState: Option<double>.None, f: static (held, row) => Widen(held: held, next: row.Ratio, pick: static (a, b) => Math.Min(val1: a, val2: b)));
    private static Option<double> Widen(Option<double> held, Option<double> next, Func<double, double, double> pick) =>
        (held, next) switch {
            ({ IsSome: true, Case: double carried }, { IsSome: true, Case: double fresh }) => Some(pick(arg1: carried, arg2: fresh)),
            ({ IsSome: true }, _) => held,
            _ => next,
        };
}

public abstract partial record ComputeReceipt {
    public sealed record Quadrature(int Domains, int Skipped, int Unwitnessed, double? ErrorBound, double? Conditioning) : ComputeReceipt;

    public sealed record Trajectory(
        int MethodOrder, int? EmbeddedOrder, string Terminal, string? RelaxAxis,
        double Achieved, int Steps, int Rejects, int RejectBudget, int Samples, double? LastError) : ComputeReceipt;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Integration {
    public static Validation<Error, QuadratureRun> Measure(MeasurePolicy policy, Op key, params ReadOnlySpan<IntegrationDomain> domains) =>
        domains.IsEmpty
            ? Validation<Error, QuadratureRun>.Fail(TensorReason.EmptyOperand.Fault("integration-domains"))
            : Batch(pending: Iterable<IntegrationDomain>.FromSpan(domains), policy: policy, key: key);

    public static Fin<TrajectoryRun<TState>> Trace<TState, TDelta>(TrajectorySpec<TState, TDelta> spec, TrajectoryControl control, Op key) =>
        Trajectory.Trace(spec: spec, control: control, key: key);

    public static IO<Fin<ComputeReceipt.Quadrature>> Receipt(MeasurePolicy policy, Op key, IClock clock, CorrelationId correlation, params ReadOnlySpan<IntegrationDomain> domains) {
        Iterable<IntegrationDomain> pending = Iterable<IntegrationDomain>.FromSpan(domains);
        return Timed(clock, () => Measure(policy, key, pending).ToFin()).Map(measured => measured.Run.Map(run =>
            new ComputeReceipt.Quadrature(
                Domains: run.Domains, Skipped: run.Skipped, Unwitnessed: run.Unwitnessed,
                ErrorBound: run.ErrorBound.ToNullable(), Conditioning: run.Conditioning.ToNullable()) {
                Scope = Scoped(lane: policy.Lane, correlation: correlation, elapsed: measured.Elapsed),
            }));
    }

    public static IO<Fin<ComputeReceipt.Trajectory>> Receipt<TState, TDelta>(
        TrajectorySpec<TState, TDelta> spec, TrajectoryControl control, Op key, IClock clock, CorrelationId correlation) =>
        Timed(clock, () => Trace(spec, control, key)).Map(measured => measured.Run.Map(run =>
            new ComputeReceipt.Trajectory(
                MethodOrder: spec.Integrator.MethodOrder,
                EmbeddedOrder: spec.Integrator.EmbeddedOrder.ToNullable(),
                Terminal: run.Terminal.Key, RelaxAxis: run.Terminal.Relax.Map(static axis => axis.Key).IfNoneUnsafe(() => null),
                Achieved: run.Cursor.Time, Steps: run.Cursor.Steps, Rejects: run.Cursor.Rejects, RejectBudget: run.RejectBudget,
                Samples: run.Cursor.Station, LastError: run.Cursor.Error.ToNullable()) {
                Scope = Scoped(lane: control.Lane, correlation: correlation, elapsed: measured.Elapsed),
            }));

    private static Validation<Error, QuadratureRun> Batch(Iterable<IntegrationDomain> pending, MeasurePolicy policy, Op key) =>
        pending
            .Traverse(domain => Quadrature.Integrate(domain: domain, control: Some(policy.Accuracy), key: key)
                .ToValidation())
            .As()
            .Map(static rows => new QuadratureRun(Evidence: rows.ToSeq()));

    private static IO<(A Run, Duration Elapsed)> Timed<A>(IClock clock, Func<A> body) =>
        IO.lift(() => {
            Instant opened = clock.GetCurrentInstant();
            A run = body();
            return (run, clock.GetCurrentInstant() - opened);
        });

    private static ReceiptScope Scoped(WorkLane lane, CorrelationId correlation, Duration elapsed) =>
        new ReceiptScope.Execution(correlation, lane, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed);
}
```

## [03]-[TRAJECTORY_DRIVER]

- Owner: `FieldState` the solver-domain carrier — a time component beside the value slab, so a NON-AUTONOMOUS field integrates on the kernel's autonomous `Step` with no second clock threaded through the fold; `FieldCarrier` the module mint deriving the kernel `IntegrationModule<FieldState, FieldState>` at an accepted state; `TrajectoryControl` the driver policy the kernel's `StepControl` does not carry, holding the lane and the optional spill beside its stepping columns; `TrajectorySpec` the run declaration carrying the station projector column and its once-read width; `TrajectoryPhase` the closed continue-or-done step the run iterates; `RelaxAxis` the knob a retryable terminal names; `TerminalDisposition` the run-level terminal partition; `TrajectoryRun` the run receipt holding the cursor whole; `Trajectory` the driver itself, its spilled leg landing the `[Stations.Count, width]` chunked station stream through the `Runtime/archive#HDF_ARCHIVE` `ArchiveSession` cursor.
- Cases: `TrajectoryPhase` `Advancing` · `Halted`; `TerminalDisposition` `Converged` · `Relaxable(RelaxAxis)` · `Divergent`; `RelaxAxis` rows steps · step-floor · horizon · stations (4).
- Entry: `Trajectory.Trace(TrajectorySpec<TState, TDelta> spec, TrajectoryControl control, Op key)` — the carrier is a type argument, so one driver integrates a scalar ODE on the kernel `IntegrationModule<double, double>.Scalar`, a frequency-domain state on `.ComplexScalar`, and a field slab on `FieldCarrier.Of` with no per-carrier driver copy, and the control's own `Spill` column selects whether the harvest accumulates or streams.
- Auto: `Admit` gates the control, the span, and the ascending in-range station set ONCE and ACCUMULATES all three, so a caller handed a bad horizon and an unsorted station set learns both; the run is then a bounded `RepeatWhile` over the `TrajectoryPhase` step that short-circuits at the first `Halted`. Each advance clamps `h` to the remaining horizon and `MaxStep`, re-mints the carrier at the current state, calls the kernel `Step` with the run's own `StepHistory`, and dispatches its `Accepted`/`Rejected` outcome; the accepted arm publishes the fresh history off the outcome so a `StepLaw.Proportional` or `StepLaw.Gustafsson` control reads a real previous error rather than degrading silently to the elementary factor. The reject arm reads non-finite error, consecutive-reject budget, and the underflow floor as ONE flattened tuple pattern. Dense stations harvest INSIDE the accepted span through `DenseOutputSpan.PointAt`, one monotone station cursor, so fixed-output-time trajectories never re-trace the field and no span outlives its own step.
- Receipt: `TrajectoryRun` holds the terminal disposition beside the CURSOR WHOLE — achieved horizon, step and reject census, last error, station tally, and samples are the cursor's own columns and were hand-copied field by field into a parallel record before — plus the kernel's own reject budget; convergence, budget exhaustion, underflow, and a refusing field all return best-so-far and are indistinguishable without the disposition.
- Packages: Rasm (project — the kernel integration floor, the archive session), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new state carrier is one `IntegrationModule` mint at its consumer; a new termination cause is one `TerminalDisposition` case (or one `RelaxAxis` row where the cause is a knob) and one arm the flattened pattern demands; a new integrator or rescale law is one kernel `IntegratorKind`/`StepLaw` row — the driver body never changes.
- Boundary — iteration: the run is a bounded repeat that SHORT-CIRCUITS. Folding over the whole step ceiling is pure and total, and it also executes every one of those iterations for a trajectory that halted at ten; `RepeatWhile(Schedule.recurs(control.MaxSteps), static p => p is Advancing)` is both — the ceiling stays the bound it was, the run costs its own length, and the phase never leaves the rail. One phase still `Advancing` when the bound expires IS budget exhaustion, which is what `Settle` reads off it, so the loop carries no terminal of its own.
- Boundary — control: the error target on the kernel `AdaptiveCase` is 1.0 because the SCALE lives in the carrier's `Norm` — a per-component RMS dividing by `atol + rtol·|yᵢ|` BEFORE squaring, so large-magnitude state never overflows the naive squared-sum-then-root and a fixed absolute tolerance never starves a growing solution. Kernel `Norm` reads the delta alone, so the scale rides a carrier re-minted at each ACCEPTED state and never inside the reject retry, and the zero delta is minted once with that carrier rather than re-allocated per accepted step. Time is a state component with unit derivative, never a driver-threaded argument, so the kernel's autonomous `sample` contract holds by construction. `StepControl.Safety` and its required `StepLaw Law` column travel together on the kernel row, and `StepHistory` is the driver's to thread: a control electing a memory-bearing law and a driver dropping the history is a degradation with no diagnostic, so the history rides the cursor.
- Boundary — terminal: only an inadmissible control, span, or station set faults — every termination SUCCEEDS with its disposition, because mapping budget exhaustion onto `Fin.Fail` destroys the relaxed-criterion retry the partition exists to serve. Retriability is a TYPE, not a bool pair: `Relaxable` carries the `RelaxAxis` naming which knob to move — budget exhaustion relaxes `MaxSteps`, underflow relaxes `MinStep` or the tolerance pair, a refusing field relaxes the horizon, and a refusing interpolant relaxes the station set the accepted span was asked to interpolate — where two independent bools admitted a `(true, true)` state no run can reach and told a caller nothing about what to change. `Divergent` alone is unretryable, because a state the norm cannot read is the field's own divergence and no control value reaches it.
- Boundary — spill: the archive session is the `Runtime/archive#HDF_ARCHIVE` capsule's, so this driver declares its slot and attributes and takes the cursor it hands back. Release brackets the ACQUISITION through `ArchiveSession.Write`, binding to every outcome arm where a `using` inside a rail lambda bound it to the success arm alone. The station ordinal IS the chunk ordinal and the cursor holds it, so write-once is structural rather than a monotonicity argument, and the spilled leg accumulates NO sample seq — the stream is its record, so `Samples` is empty by the leg's own construction and `Station` still reports what landed.

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
    FieldIntegrator Integrator,
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
    Option<double> Error, StepHistory History, Seq<TrajectorySample<TState>> Samples, int Station);

[Union]
public abstract partial record TrajectoryPhase<TState> {
    private TrajectoryPhase() { }

    public sealed record Advancing(TrajectoryCursor<TState> Cursor) : TrajectoryPhase<TState>;
    public sealed record Halted(TrajectoryCursor<TState> Cursor, TerminalDisposition Terminal) : TrajectoryPhase<TState>;
}

public sealed record TrajectoryRun<TState>(TrajectoryCursor<TState> Cursor, TerminalDisposition Terminal, int RejectBudget) {
    public TState Final => Cursor.State;
}

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
    public static Fin<TrajectoryRun<TState>> Trace<TState, TDelta>(TrajectorySpec<TState, TDelta> spec, TrajectoryControl control, Op key) =>
        Admit(spec: spec, control: control).Bind(seeded => control.Spill.Match(
            None: () => Fin.Succ(Settle(Run(seeded, spec, control, key, None), spec.Integrator.RejectBudget)),
            Some: spill => Spilled(seeded, spec, control, key, spill)));

    private static Fin<TrajectoryRun<TState>> Spilled<TState, TDelta>(
        TrajectoryCursor<TState> seeded, TrajectorySpec<TState, TDelta> spec, TrajectoryControl control, Op key, TraceSpill spill) =>
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
                        Run(seeded, spec, control, key, Some(new StationSink<TState>(cursor, state => spec.Project(state).ToArray()))),
                        spec.Integrator.RejectBudget)))
                .Run());

    private sealed record StationSink<TState>(ChunkCursor<double> Cursor, Func<TState, double[]> Project);

    private static TrajectoryPhase<TState> Run<TState, TDelta>(
        TrajectoryCursor<TState> seeded, TrajectorySpec<TState, TDelta> spec, TrajectoryControl control, Op key, Option<StationSink<TState>> spill) =>
        IO.pure((TrajectoryPhase<TState>)new TrajectoryPhase<TState>.Advancing(seeded))
            .Map(phase => phase is TrajectoryPhase<TState>.Advancing advancing
                ? Advance(cursor: advancing.Cursor, spec: spec, control: control, key: key, spill: spill)
                : phase)
            .RepeatWhile(Schedule.recurs(control.MaxSteps), static phase => phase is TrajectoryPhase<TState>.Advancing)
            .Run();

    private static Fin<TrajectoryCursor<TState>> Admit<TState, TDelta>(TrajectorySpec<TState, TDelta> spec, TrajectoryControl control) =>
        (control.Admits, Span(spec), Stations(spec))
            .Apply(static (_, _, _) => unit).As().ToFin()
            .Map(_ => new TrajectoryCursor<TState>(
                Time: spec.Start, Step: Math.Min(val1: spec.FirstStep, val2: control.MaxStep), State: spec.Initial,
                Steps: 0, Rejects: 0, Streak: 0, Error: Option<double>.None, History: StepHistory.Fresh, Samples: [], Station: 0));

    private static Validation<Error, Unit> Span<TState, TDelta>(TrajectorySpec<TState, TDelta> spec) =>
        double.IsFinite(spec.Start) && spec.Horizon > spec.Start && spec.FirstStep > 0.0 && double.IsFinite(spec.FirstStep)
            ? unit
            : TensorReason.PolicyInvalid.Fault("trajectory-span", $"{spec.Start}:{spec.Horizon}:{spec.FirstStep}");

    private static Validation<Error, Unit> Stations<TState, TDelta>(TrajectorySpec<TState, TDelta> spec) =>
        spec.Stations.Zip(spec.Stations.Skip(1)).ForAll(static pair => pair.First < pair.Second)
        && spec.Stations.ForAll(station => station >= spec.Start && station <= spec.Horizon)
            ? unit
            : TensorReason.PolicyInvalid.Fault("trajectory-stations", spec.Stations.Count.ToString(CultureInfo.InvariantCulture));

    private static TrajectoryPhase<TState> Advance<TState, TDelta>(TrajectoryCursor<TState> cursor, TrajectorySpec<TState, TDelta> spec, TrajectoryControl control, Op key, Option<StationSink<TState>> spill) {
        double step = Math.Min(val1: Math.Min(val1: cursor.Step, val2: control.MaxStep), val2: spec.Horizon - cursor.Time);
        return spec.Integrator
            .Step(module: spec.Carrier(arg: cursor.State), sample: spec.Field, state: cursor.State, h: step, key: key, history: cursor.History)
            .Match(
                Succ: outcome => outcome.Switch(
                    state: (Cursor: cursor, Step: step, Spec: spec, Control: control, Key: key, Spill: spill),
                    acceptedCase: static (s, accepted) => Accepted(cursor: s.Cursor, step: s.Step, accepted: accepted, spec: s.Spec, control: s.Control, key: s.Key, spill: s.Spill),
                    rejectedCase: static (s, rejected) => Rejected(cursor: s.Cursor, rejected: rejected, spec: s.Spec, control: s.Control)),
                Fail: _ => (TrajectoryPhase<TState>)new TrajectoryPhase<TState>.Halted(cursor, new TerminalDisposition.Relaxable(RelaxAxis.Horizon)));
    }

    private static TrajectoryPhase<TState> Accepted<TState, TDelta>(
        TrajectoryCursor<TState> cursor, double step, IntegrationStep<TState, TDelta>.AcceptedCase accepted,
        TrajectorySpec<TState, TDelta> spec, TrajectoryControl control, Op key, Option<StationSink<TState>> spill) =>
        Harvest(cursor: cursor, at: cursor.Time, step: step, dense: accepted.Dense, stations: spec.Stations, key: key, spill: spill).Match(
            Succ: taken => Land(
                cursor: cursor with {
                    Time = cursor.Time + step,
                    Step = Math.Min(val1: accepted.SuggestedStep, val2: control.MaxStep),
                    State = accepted.Next,
                    Steps = cursor.Steps + 1,
                    Streak = 0,
                    Error = accepted.Error,
                    History = accepted.History,
                    Samples = taken.Samples,
                    Station = taken.Station,
                },
                spec: spec, control: control),
            Fail: _ => (TrajectoryPhase<TState>)new TrajectoryPhase<TState>.Halted(cursor, new TerminalDisposition.Relaxable(RelaxAxis.Stations)));

    private static TrajectoryPhase<TState> Rejected<TState, TDelta>(
        TrajectoryCursor<TState> cursor, IntegrationStep<TState, TDelta>.RejectedCase rejected,
        TrajectorySpec<TState, TDelta> spec, TrajectoryControl control) {
        TrajectoryCursor<TState> next = cursor with {
            Step = Math.Min(val1: rejected.SuggestedStep, val2: control.MaxStep),
            Rejects = cursor.Rejects + 1,
            Streak = cursor.Streak + 1,
            Error = rejected.Error,
            History = rejected.History,
        };
        return (Finite: rejected.Error.Map(double.IsFinite).IfNone(noneValue: true),
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
        TrajectoryCursor<TState> cursor, double at, double step, DenseOutputSpan<TState, TDelta> dense, Seq<double> stations, Op key, Option<StationSink<TState>> spill) =>
        stations.Skip(cursor.Station).TakeWhile(station => station <= at + step)
            .TraverseM(station => dense.PointAt(theta: (station - at) / step, key: key).Map(state => new TrajectorySample<TState>(station, state)))
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

- Owner: `SpectralSymbol` the `[SmartEnum<string>]` Fourier-multiplier vocabulary carrying each operator's generated symbol column and its parity; `SymbolParity` the composing parity policy; `Spectral` the pointwise-product composition carrier; `WaveAxis` the split-spectrum wavenumber owner over the kernel arena's own axis read; `SpectralControl` the imaginary-residual band; `SpectralPlane` the two-case arena carrier the parity of the field length selects; `SpectralOperator.Apply` the forward-multiply-inverse application with its Hermitian gate.
- Cases: `SpectralSymbol` rows derivative (`i·k`), laplacian (`−k²`), biharmonic (`k⁴`), hilbert (`−i·sgn k`), anti-derivative (`1/(i·k)`, zero mode killed) (5); `SymbolParity` rows even · odd (2); `SpectralPlane` cases `Packed(SpectralArena.HalfSpectrum)` · `Split(SpectralArena.Split)` (2 — the two kernel arena layouts a real one-dimensional field admits into).
- Entry: `SpectralOperator.Apply(ReadOnlySpan<double> field, WaveAxis axis, Spectral op, Option<SpectralControl> control)` — the composition is the value, so a chained operator and a single symbol are the same call.
- Auto: `Spectral.At(k)` is the pointwise product of factor rows and `Spectral.Parity` the XOR-fold of factor parities, so the Nyquist bin zeroes exactly when the composition is odd; `Apply` selects the kernel arena case by field length — even lengths ride `SpectralArena.HalfSpectrum` (half the transform work, real output BY CONSTRUCTION, the symbol multiplying each nonnegative-k packed pair so Hermitian symmetry is structural), odd lengths ride `SpectralArena.Split` whose imaginary-residual gate diagnoses broken symmetry — and BOTH transform through the kernel `SpectralArena.Transform` entry, so the four raw `Fourier.*` reaches this lane once held are gone and one convention vocabulary spans the strata boundary. Admission is the vectorized `TensorPrimitives.IsFiniteAll` sweep, never a per-element predicate walk; the wavenumbers derive from the kernel `SpectralReceipt.Axis` bin read scaled by 2π, never a hand-indexed bin walk or a raw `Fourier.FrequencyScale` reach.
- Receipt: `SpectralEvidence` carries the real result as owned `ImmutableArray<double>`, the composed parity, and `Option<double>` imaginary residual — `Some` only on the split leg where the gate read it, `None` on the packed leg whose realness is structural; excess residual fails the split leg as broken Hermitian symmetry, never a usable result.
- Packages: Rasm (project — the kernel transform band and its arena, `Numerics/atoms` `Dimension`/`PositiveMagnitude`/`SignedAxis`), System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new operator is one `SpectralSymbol` row with its generated symbol column and parity; a composite is a `Spectral.Then` chain; a tighter Hermitian band is one `SpectralControl` value; a new buffer layout is one kernel `SpectralArena` case, which breaks the `SpectralPlane` fold at compile time — zero new code path.
- Boundary — symbols: every constant-coefficient periodic operator is one `SpectralSymbol` row applied pointwise to the forward transform; symbols compose by pointwise multiplication before a single inverse, and parity is row data the operator owns, never a `bool oddOrder` knob nor a bare `Func<double, Complex>` riding beside the call. `ZeroesNyquist` is DERIVED from the parity row rather than kept as a column beside it: an odd symbol is discontinuous across ±Nyquist and therefore zeroes that bin, which is a fact about the row, not a second value that can disagree with it.
- Boundary — arena: the transform floor is the kernel's whole. `SpectralArena` is the ONE transform carrier and `arena.Transform(sense, scaling, key)` the one entry, so this lane picks the arena CASE its field parity implies and spells no transform of its own; `SpectralScaling.Unscaled` is the convention value because this lane READS BETWEEN the legs — an unscaled forward leaves the intermediate bins carrying true DFT coefficients so a symbol's magnitude IS the operator's transfer function, where a `1/√N` forward rescales every spectrum the imaginary-residual gate and any bin-domain consumer inspect, and the round-trip factor the `Unscaled` row carries is applied once on the way out. The symbol MULTIPLY stays this lane's because the kernel's own `SpectralReceipt.Modulate` binds the interleaved plane arena alone and neither one-dimensional case reaches it — the generation of a multiplier is consumer domain policy exactly as tap generation is, and both legs' multiply is the same three-line fold over the arena's own layout. The split-spectrum wavenumber derives once from the transformed receipt's own axis read (ascending positives through Nyquist, then descending negatives, scaled by `2π`), because hand-indexing the bin applies an aliased symbol past the half length silently, and `WaveAxis` therefore owns the extent-to-rate projection alone rather than a second frequency table.
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

    public sealed record Packed(SpectralArena.HalfSpectrum Arena) : SpectralPlane;
    public sealed record Split(SpectralArena.Split Arena) : SpectralPlane;

    public SpectralArena Arena => Switch(packed: static p => (SpectralArena)p.Arena, split: static s => (SpectralArena)s.Arena);

    public static SpectralPlane Of(ReadOnlySpan<double> field, PositiveMagnitude rate) {
        int n = field.Length;
        if (int.IsEvenInteger(n)) {
            double[] packed = new double[SpectralArena.PackedLength(samples: n)];
            field.CopyTo(packed);
            return new Packed(new SpectralArena.HalfSpectrum(packed, Dimension.Create(value: n), rate));
        }

        double[] real = field.ToArray();
        return new Split(new SpectralArena.Split(real, new double[n], rate));
    }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct WaveAxis(int Length, double Extent) {
    public Fin<PositiveMagnitude> Rate =>
        Length >= 2 && double.IsFinite(Extent) && Extent > 0.0
            ? Fin.Succ(PositiveMagnitude.Create(value: Length / Extent))
            : TensorReason.PolicyInvalid.Fail<PositiveMagnitude>("wave-axis", $"length={Length}:extent={Extent:e3}");

    public Option<int> Nyquist => int.IsEvenInteger(Length) ? Some(Length >> 1) : None;

    public Fin<double[]> Wavenumbers(SpectralReceipt receipt, Op key) =>
        receipt.Axis(axis: SignedAxis.X, key: key).Map(static cycles => {
            double[] k = new double[cycles.Count];
            TensorPrimitives.Multiply<double>(cycles.AsSpan(), 2.0 * Math.PI, k);
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
    public static Fin<SpectralEvidence> Apply(ReadOnlySpan<double> field, WaveAxis axis, Spectral op, Option<SpectralControl> control, Op key) {
        if (field.Length != axis.Length || !TensorPrimitives.IsFiniteAll<double>(field)) {
            return TensorReason.ShapeMismatch.Fail<SpectralEvidence>("wave-axis-mismatch", $"field={field.Length}:axis={axis.Length}");
        }

        Fin<PositiveMagnitude> rate = axis.Rate;
        if (rate.Case is not PositiveMagnitude sampling) { return rate.Map(static _ => default(SpectralEvidence)!); }
        SpectralPlane plane = SpectralPlane.Of(field, sampling);
        return plane.Arena.Transform(sense: SpectralSense.Forward, scaling: SpectralScaling.Unscaled, key: key)
            .Bind(forward => axis.Wavenumbers(forward, key).Map(k => Modulated(plane, op, k, axis.Nyquist)))
            .Bind(_ => plane.Arena.Transform(sense: SpectralSense.Inverse, scaling: SpectralScaling.Unscaled, key: key))
            .Bind(inverse => Settled(plane, op, axis, inverse, control.IfNone(SpectralControl.Default)));
    }

    static Unit Modulated(SpectralPlane plane, Spectral op, double[] k, Option<int> nyquist) => plane.Switch(
        state: (Op: op, K: k, Nyquist: nyquist),
        packed: static (s, p) => {
            int half = p.Arena.Samples.Value >> 1;
            for (int bin = 0; bin <= half; bin++) {
                Complex scaled = new Complex(p.Arena.Values[2 * bin], p.Arena.Values[(2 * bin) + 1]) * Factor(s.Op, s.K, s.Nyquist, bin);
                (p.Arena.Values[2 * bin], p.Arena.Values[(2 * bin) + 1]) = (scaled.Real, scaled.Imaginary);
            }

            return unit;
        },
        split: static (s, sp) => {
            for (int bin = 0; bin < sp.Arena.Real.Length; bin++) {
                Complex scaled = new Complex(sp.Arena.Real[bin], sp.Arena.Imaginary[bin]) * Factor(s.Op, s.K, s.Nyquist, bin);
                (sp.Arena.Real[bin], sp.Arena.Imaginary[bin]) = (scaled.Real, scaled.Imaginary);
            }

            return unit;
        });

    static Complex Factor(Spectral op, double[] k, Option<int> nyquist, int bin) =>
        op.Parity.ZeroesNyquist && nyquist == Some(bin) ? Complex.Zero : op.At(wavenumber: k[bin]);

    static Fin<SpectralEvidence> Settled(SpectralPlane plane, Spectral op, WaveAxis axis, SpectralReceipt inverse, SpectralControl control) {
        double[] result = new double[axis.Length];
        return plane.Switch(
            state: (Axis: axis, Result: result, Factor: inverse.RoundTripFactor),
            packed: static (s, p) => {
                p.Arena.Values.AsSpan(0, s.Axis.Length).CopyTo(s.Result);
                TensorPrimitives.Divide<double>(s.Result, s.Factor, s.Result);
                return Fin.Succ(new SpectralEvidence([.. s.Result], None, default!));
            },
            split: (s, sp) => {
                sp.Arena.Real.AsSpan(0, s.Axis.Length).CopyTo(s.Result);
                TensorPrimitives.Divide<double>(s.Result, s.Factor, s.Result);
                double real = TensorPrimitives.MaxMagnitude<double>(sp.Arena.Real);
                double imaginary = TensorPrimitives.MaxMagnitude<double>(sp.Arena.Imaginary);
                double residual = Math.Abs(imaginary) / Math.Max(Math.Abs(real), double.MinNormal);
                return residual < control.ImaginaryFloor
                    ? Fin.Succ(new SpectralEvidence([.. s.Result], Some(residual), default!))
                    : TensorReason.WitnessFail.Fail<SpectralEvidence>("imaginary-residual", $"r={residual:e3}", $"floor={control.ImaginaryFloor:e3}");
            })
            .Map(evidence => evidence with { Parity = op.Parity });
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
