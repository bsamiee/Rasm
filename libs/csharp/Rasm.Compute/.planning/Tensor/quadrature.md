# [COMPUTE_QUADRATURE]

Rasm.Compute measured-integration lane over the kernel `Rasm.Numerics` integration floor: the accuracy-routed quadrature owner and the Runge-Kutta step algebra both arrive settled from `Rasm/Numerics/integrate` and are composed whole — package-local re-declarations of that kernel surface are the deleted form. This page owns what sits ABOVE that floor: the batched quadrature entry whose independent domains accumulate every refusal, the solver-domain state carrier the kernel's `Scalar`/`ComplexScalar` pair does not reach, the adaptive DRIVER the kernel deliberately leaves to its consumer, and the `ComputeReceipt`/`ComputeFault` projection every Compute outcome crosses.

Kernel `Step` returns one `IntegrationStep` Accepted or Rejected and owns no reject loop, run-level terminal partition, step-underflow floor, total-step budget, or run-level dense trajectory — each is the driver's, and each is load-bearing: budget exhaustion, underflow, and non-finite error all return best-so-far indistinguishable without the marker. `Spectral` stays owned here as a Fourier-MULTIPLIER algebra distinct from the `Stats/signal` transform-and-filter axis, its in-place transform on the spectral copy this page's sanctioned statement-form numeric kernel.

## [01]-[INDEX]

- [02]-[INTEGRATION_LANE]: batched `Integration.Measure` over the kernel `IntegrationDomain` arity union, the `MeasurePolicy`/`TracePolicy` pair, the `QuadratureRun` evidence fold, and the `ComputeReceipt`/`ComputeFault` projection both legs cross.
- [03]-[TRAJECTORY_DRIVER]: `FieldState` scaled-norm carrier, `TrajectoryPhase` continue-or-done fold over the kernel `FieldIntegrator.Step`, `TrajectoryTerminal` partition, and dense-station harvest.
- [04]-[SPECTRAL_OPERATOR]: `Spectral` applies each `SpectralSymbol` multiplier pointwise under parity-derived Nyquist zeroing — the packed-real leg on even lengths, the complex leg with its imaginary-residual gate on odd.

## [02]-[INTEGRATION_LANE]

- Owner: `MeasurePolicy` and `TracePolicy` the two lane policies — the kernel accuracy budget and the driver step budget, each beside the `WorkLane` its receipt scopes to, `TracePolicy` also carrying the optional `TraceSpill` archive target the station harvest lands into; `QuadratureRun` the batch evidence fold over the kernel `QuadratureEvidence` rows; `Integration` the entry pair — `Measure` for definite integrals over the kernel `IntegrationDomain` arity union, `Trace` for initial-value trajectories — sharing one fault projection and one receipt mint; `ComputeReceipt.Quadrature` and `ComputeReceipt.Trajectory` the two partial cases this page declares on the `Runtime/receipts` union.
- Cases: `IntegrationDomain` arms arrive from the kernel — `Line` · `Rectangle` · `Cuboid` · `SparseGrid` · `Simplex`; `QuadratureRoute` rows `DoubleExponential` · `GaussLegendre` · `GaussKronrod`; `ReferenceElement` rows `Line` · `Tri` · `Tet` · `Quad` · `Hex` · `Wedge` · `Pyramid`. This page adds no arity and no accuracy row.
- Entry: `Integration.Measure(MeasurePolicy policy, Op key, params ReadOnlySpan<IntegrationDomain> domains)` absorbs the singular, batch, and empty call in one signature — a moment set, a polynomial-chaos coefficient sweep, and a single element integral are the same call at three arities; `Integration.Trace(TrajectorySpec<TState, TDelta> spec, TracePolicy policy, Op key)` delegates the fold to `[03]`'s `Trajectory.Trace` — the spill-bearing overload when `TracePolicy.Spill` is `Some` — and mints the receipt over the same scope.
- Auto: batched domains are INDEPENDENT, so the fold is the applicative `Traverse` over `Validation<Error, QuadratureRun>` and a caller sees every refusing domain at once where a monadic fold reports only the first; the carrier alone selects that policy, and `ToFin` is the caller's own short-circuit egress. Kernel `Quadrature.Integrate` already owns the finite guard, the skip budget, the infinite-bound capability gate, and the three-tier admission, so this lane re-imposes none of them and adds only what the kernel cannot see: the Compute band, the lane scope, and the batch census.
- Receipt: `ComputeReceipt.Quadrature` carries the domain count, the summed skip census, and the batch's WORST reported channels — the largest error estimate and the smallest cancellation ratio — each `double?` so a batch of non-adaptive rows reports honest absence rather than a zero no route measured; `ComputeReceipt.Trajectory` carries the method and embedded orders, the terminal key with its `Resolved`/`Retryable` columns, the achieved horizon, and the step/reject/sample census.
- Packages: Rasm (project), PureHDF (`H5File`, `H5Dataset<T>`, `H5DatasetCreation`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new accuracy kernel, arity, or reference domain is one kernel row and reaches this lane with zero edits here; a new outcome column is one field on the owning receipt case; a new integration modality is one entry on `Integration` sharing the same projection pair — zero new surface.
- Boundary: the two entries carry TWO policies because they read disjoint knobs — `Measure` reads the accuracy budget, `Trace` reads the step budget, and only the lane is common. Forcing a definite-integral caller to name a minimum step, a step ceiling, and a total-step budget to integrate one rectangle constructs evidence for a run it never starts, and every knob invented there is unreadable by the entry it hands it to.
- Boundary: `Quadrature.Integrate` is the ONE quadrature call site in this package — a raw `Integrate.GaussLegendre`/`GaussKronrod`/`OnRectangle`/`OnCuboid` call skips the kernel's finite guard, skip budget, and typed evidence and is the deleted form, as is a package-local `QuadratureRoute`/`IntegrationDomain`/`QuadratureRule`/`SmolyakCubature` re-declaration. Kernel refusals cross the band through ONE projection reading the `Fault`'s own self-sufficient `Category` and `Message`, so the cause stays addressable without a second fault vocabulary mirroring `Rasm.Domain` arm for arm; a bare `ModelRejected` token discarding that evidence is the rejected flatten. Both legs are managed host-local folds, so the receipt scopes to `Substrate.CpuTensor` and a device row here claims residency this lane never acquires.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// Two policies, one per entry, because the two entries read disjoint knobs and only the receipt lane is shared.
public sealed record MeasurePolicy(QuadratureControl Accuracy, WorkLane Lane) {
    public static readonly MeasurePolicy Default = new(Accuracy: QuadratureControl.Default, Lane: WorkLane.Background);
}

public sealed record TracePolicy(TrajectoryControl Stepping, WorkLane Lane, Option<TraceSpill> Spill) {
    public static readonly TracePolicy Default = new(Stepping: TrajectoryControl.Default, Lane: WorkLane.Background, Spill: None);
}

// Station spill target: the archive stream the driver lands harvested stations into through
// Runtime/codecs#HDF_ARCHIVE, one chunk per station; None keeps the in-memory sample Seq.
public sealed record TraceSpill(Stream Sink, HdfArchivePolicy Archive);

public sealed record QuadratureRun(Seq<QuadratureEvidence> Evidence) {
    public int Domains => Evidence.Count;
    public int Skipped => Evidence.Fold(initialState: 0, f: static (sum, row) => sum + row.Skipped);
    // Batch-worst channels: the LARGEST error estimate and the SMALLEST cancellation ratio any row reported.
    // Only the adaptive Kronrod row yields either, so a batch of fixed-order rows folds to None and the receipt
    // spells absence — a 0.0 here would read as a measurement no route took.
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

// Both outcomes ride the Runtime/receipts-owned ComputeReceipt union as partial cases, never a second receipt
// union; the inherited Scope stamps correlation, lane, substrate, allocation class, and elapsed at mint.
public abstract partial record ComputeReceipt {
    public sealed record Quadrature(int Domains, int Skipped, double? ErrorBound, double? Conditioning) : ComputeReceipt;

    public sealed record Trajectory(
        int MethodOrder, int? EmbeddedOrder, string Terminal, bool Resolved, bool Retryable,
        double Achieved, int Steps, int Rejects, int RejectBudget, int Samples, double? LastError) : ComputeReceipt;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Integration {
    public static Validation<Error, QuadratureRun> Measure(MeasurePolicy policy, Op key, params ReadOnlySpan<IntegrationDomain> domains) =>
        // Emptiness reads off the span itself; FromSpan then copies at the call, so the carrier outlives the
        // frame the span can never leave.
        domains.IsEmpty
            ? Validation<Error, QuadratureRun>.Fail(new ComputeFault.ModelRejected("<integration-domains-empty>"))
            : Batch(pending: Iterable<IntegrationDomain>.FromSpan(domains), policy: policy, key: key);

    public static Fin<TrajectoryRun<TState>> Trace<TState, TDelta>(TrajectorySpec<TState, TDelta> spec, TracePolicy policy, Op key) =>
        policy.Spill.Match(
            Some: spill => Trajectory.Trace(spec: spec, control: policy.Stepping, spill: spill, key: key),
            None: () => Trajectory.Trace(spec: spec, control: policy.Stepping, key: key));

    public static ComputeReceipt.Quadrature Receipt(QuadratureRun run, MeasurePolicy policy, CorrelationId correlation, Duration elapsed) =>
        new(Domains: run.Domains, Skipped: run.Skipped, ErrorBound: Reported(run.ErrorBound), Conditioning: Reported(run.Conditioning)) {
            Scope = Scoped(lane: policy.Lane, correlation: correlation, elapsed: elapsed),
        };

    public static ComputeReceipt.Trajectory Receipt<TState>(TrajectoryRun<TState> run, FieldIntegrator integrator, TracePolicy policy, CorrelationId correlation, Duration elapsed) =>
        new(
            MethodOrder: integrator.MethodOrder,
            EmbeddedOrder: integrator.EmbeddedOrder.Match(Some: static order => (int?)order, None: static () => (int?)null),
            Terminal: run.Terminal.Key, Resolved: run.Terminal.Resolved, Retryable: run.Terminal.Retryable,
            Achieved: run.Achieved, Steps: run.Steps, Rejects: run.Rejects, RejectBudget: run.RejectBudget,
            Samples: run.Harvested, LastError: Reported(run.LastError)) {
            Scope = Scoped(lane: policy.Lane, correlation: correlation, elapsed: elapsed),
        };

    // THE one kernel-to-band hop: Category and Message are the Fault's own self-sufficient evidence, so the
    // detail token stays addressable and no second fault vocabulary mirrors Rasm.Domain arm for arm.
    internal static ComputeFault Refused(Error kernel, string at) =>
        new ComputeFault.ModelRejected($"<{at}:{kernel.Category}:{kernel.Message}>");

    private static Validation<Error, QuadratureRun> Batch(Iterable<IntegrationDomain> pending, MeasurePolicy policy, Op key) =>
        pending
            .Traverse(domain => Quadrature.Integrate(domain: domain, control: policy.Accuracy, key: key)
                .ToValidation()
                .MapFail(fault => (Error)Refused(kernel: fault, at: "quadrature")))
            .As()
            .Map(static rows => new QuadratureRun(Evidence: rows.ToSeq()));

    private static ReceiptScope Scoped(WorkLane lane, CorrelationId correlation, Duration elapsed) =>
        new ReceiptScope.Execution(correlation, lane, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed);

    private static double? Reported(Option<double> channel) => channel.Match(Some: static value => (double?)value, None: static () => (double?)null);
}
```

## [03]-[TRAJECTORY_DRIVER]

- Owner: `FieldState` the solver-domain carrier — a time component beside the value slab, so a NON-AUTONOMOUS field integrates on the kernel's autonomous `Step` with no second clock threaded through the fold; `FieldCarrier` the module mint deriving the kernel `IntegrationModule<FieldState, FieldState>` at an accepted state; `TrajectoryControl` the driver policy the kernel's `StepControl` does not carry; `TrajectorySpec` the run declaration carrying the station projector column (`Func<TState, ReadOnlySpan<double>>` — the row a spilled station writes, its width read once at Admit); `TrajectoryPhase` the closed continue-or-done step the run iterates; `TrajectoryTerminal` the run-level terminal partition; `TrajectoryRun` the run receipt with the `Harvested` station tally both legs share; `Trajectory` the driver itself, its spill overload landing the `[Stations.Count, width]` chunked station stream through `Runtime/codecs#HDF_ARCHIVE` — fileDims known at Admit, one chunk per station at the station's own ordinal, the monotone station cursor the chunk-once law by construction, and NO in-memory sample seq accumulating on the spilled leg.
- Cases: `TrajectoryPhase` `Advancing` · `Halted`; `TrajectoryTerminal` rows completed · budget-exhausted · step-underflow · non-finite · field-refused · dense-refused, each carrying `Resolved`/`Retryable` so a caller relaxes and resumes rather than re-tracing.
- Entry: `Trajectory.Trace(TrajectorySpec<TState, TDelta> spec, TrajectoryControl control, Op key)` — the carrier is a type argument, so one driver integrates a scalar ODE on the kernel `IntegrationModule<double, double>.Scalar`, a frequency-domain state on `.ComplexScalar`, and a field slab on `FieldCarrier.Of` with no per-carrier driver copy.
- Auto: `Admit` gates the control, the span, and the ascending in-range station set once; the run is then a bounded terminal-exiting iteration over the `TrajectoryPhase` step, holding one local and leaving at the first `Halted` phase. Each advance clamps `h` to the remaining horizon and `MaxStep`, re-mints the carrier at the current state, calls the kernel `Step`, and dispatches its `Accepted`/`Rejected` outcome; the reject arm reads non-finite error, consecutive-reject budget, and the underflow floor as ONE flattened tuple pattern. Dense stations harvest INSIDE the accepted span through `DenseOutputSpan.PointAt`, one monotone station cursor, so fixed-output-time trajectories never re-trace the field and no span outlives its own step.
- Receipt: `TrajectoryRun` records the terminal marker with the achieved horizon, the step/reject census, the kernel's own reject budget, the last error estimate, the `Harvested` station tally, and the in-memory station samples (empty on a spilled run — the stream is its record) — convergence, budget exhaustion, underflow, and a refusing field all return best-so-far and are indistinguishable without the marker.
- Packages: Rasm (project), PureHDF (`H5File`, `H5Dataset<T>`, `H5DatasetCreation`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new state carrier is one `IntegrationModule` mint at its consumer; a new termination cause is one `TrajectoryTerminal` row and one arm the flattened pattern demands; a new integrator is one kernel `IntegratorKind` row — the driver body never changes.
- Boundary: the run body is the sanctioned imperative at this boundary. Folding over the whole step ceiling is pure and total, and it also executes every one of those iterations for a trajectory that halted at ten — the idempotent-past-terminal trick buys expression shape and pays the ceiling in dispatches on every single trace. One bounded terminal-exiting iteration holding one phase local is the honest form: the ceiling stays the bound it was, the run costs its own length, and purity is preserved where it is observable — at the entry, which still returns `Fin` over an immutable run.
- Boundary: the error target on the kernel `AdaptiveCase` is 1.0 because the SCALE lives in the carrier's `Norm` — a per-component RMS dividing by `atol + rtol·|yᵢ|` BEFORE squaring, so large-magnitude state never overflows the naive squared-sum-then-root and a fixed absolute tolerance never starves a growing solution. Kernel `Norm` reads the delta alone, which is exactly why the scale rides a carrier re-minted at each ACCEPTED state and never inside the reject retry; a module minted once from the initial state silently reverts to absolute control the moment the solution grows. Time is a state component with unit derivative, never a driver-threaded argument, so the kernel's autonomous `sample` contract holds by construction. Only an inadmissible control, span, or station set faults — every termination succeeds with its marker, because mapping budget exhaustion onto `Fin.Fail` destroys the relaxed-criterion retry the partition exists to serve. `Retryable` marks a row whose cause is a KNOB rather than the problem: budget exhaustion relaxes `MaxSteps`, underflow relaxes `MinStep` or the tolerance pair, a refusing field relaxes the horizon, and a refusing interpolant relaxes the station set the accepted span was asked to interpolate. `NonFinite` alone is unretryable, because a state the norm cannot read is the field's own divergence and no control value reaches it.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TrajectoryTerminal {
    public static readonly TrajectoryTerminal Completed = new("completed", resolved: true, retryable: false);
    public static readonly TrajectoryTerminal BudgetExhausted = new("budget-exhausted", resolved: false, retryable: true);
    public static readonly TrajectoryTerminal StepUnderflow = new("step-underflow", resolved: false, retryable: true);
    public static readonly TrajectoryTerminal NonFinite = new("non-finite", resolved: false, retryable: false);
    // Field refusal at one state clears at a shorter step or a nearer horizon, so this row carries retryable
    // evidence, never a hard fault discarding the trajectory already integrated.
    public static readonly TrajectoryTerminal FieldRefused = new("field-refused", resolved: false, retryable: true);
    // Dense refusal is the interpolant refusing ONE station inside an accepted span, so it clears the same
    // way: the trajectory itself converged, and a retrace over a thinner station set — or none — completes the run.
    public static readonly TrajectoryTerminal DenseRefused = new("dense-refused", resolved: false, retryable: true);

    public bool Resolved { get; }
    public bool Retryable { get; }
}

// Autonomization is the CARRIER's: Time is a state component whose derivative the field reports as 1, so the
// kernel's autonomous Step advances t with y and the driver threads no second clock; the error norm skips it
// because a clock carries no local-truncation error.
public readonly record struct FieldState(double Time, ImmutableArray<double> Values);

public readonly record struct TrajectorySample<TState>(double Time, TState State);

// --- [MODELS] ------------------------------------------------------------------------------
public sealed record TrajectoryControl(double AbsoluteTolerance, double RelativeTolerance, double MinStep, double MaxStep, int MaxSteps) {
    public static readonly TrajectoryControl Default = new(
        AbsoluteTolerance: 1e-9, RelativeTolerance: 1e-6, MinStep: 1e-12, MaxStep: double.PositiveInfinity, MaxSteps: 100_000);

    internal bool IsValid =>
        double.IsFinite(AbsoluteTolerance) && AbsoluteTolerance > 0.0
        && double.IsFinite(RelativeTolerance) && RelativeTolerance > 0.0
        && double.IsFinite(MinStep) && MinStep > 0.0
        && MaxStep > MinStep && MaxSteps > 0;
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
    // Station projector — the row a spilled station writes; width reads once off the initial state so the spill
    // fileDims are known at Admit. The BCL `Func` admits the ref-struct return on this target.
    Func<TState, ReadOnlySpan<double>> Project);

public sealed record TrajectoryCursor<TState>(
    double Time, double Step, TState State, int Steps, int Rejects, int Streak,
    Option<double> Error, Seq<TrajectorySample<TState>> Samples, int Station);

// Continue-or-done step the run iterates: Advancing carries the whole cursor forward, Halted seals it beside the
// terminal that stopped it, so no cursor field records why a run ended.
[Union]
public abstract partial record TrajectoryPhase<TState> {
    private TrajectoryPhase() { }

    public sealed record Advancing(TrajectoryCursor<TState> Cursor) : TrajectoryPhase<TState>;
    public sealed record Halted(TrajectoryCursor<TState> Cursor, TrajectoryTerminal Terminal) : TrajectoryPhase<TState>;
}

// `Harvested` is the one station tally both legs share: the in-memory leg equals `Samples.Count`, and a spilled
// run holds an EMPTY Samples seq — the stream is its record — while Harvested still reports what landed.
public sealed record TrajectoryRun<TState>(
    TState Final, TrajectoryTerminal Terminal, double Achieved, int Steps, int Rejects, int RejectBudget,
    Option<double> LastError, int Harvested, Seq<TrajectorySample<TState>> Samples);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class FieldCarrier {
    // Scale reads the ACCEPTED state, so the module re-mints once per accepted step and never inside a reject
    // retry; the kernel Norm sees the delta alone, which is why the scale cannot ride the tolerance instead.
    public static IntegrationModule<FieldState, FieldState> Of(FieldState state, TrajectoryControl control) {
        ImmutableArray<double> scale =
            [.. state.Values.Select(value => control.AbsoluteTolerance + (control.RelativeTolerance * Math.Abs(value: value)))];
        return new IntegrationModule<FieldState, FieldState>(
            Add: static (current, h, delta) => new FieldState(current.Time + (h * delta.Time), Combine(left: current.Values, right: delta.Values, gain: h)),
            Scale: static (factor, delta) => new FieldState(factor * delta.Time, [.. delta.Values.Select(value => factor * value)]),
            Sum: static (left, right) => new FieldState(left.Time + right.Time, Combine(left: left.Values, right: right.Values, gain: 1.0)),
            Norm: delta => ScaledRms(delta: delta.Values, scale: scale),
            Zero: new FieldState(0.0, [.. Enumerable.Repeat(element: 0.0, count: state.Values.Length)]));
    }

    // Unit-derivative time slot IS the autonomization: a consumer lifts f(t, y) once here and the driver never
    // learns the field is non-autonomous.
    public static Func<FieldState, Fin<FieldState>> Lift(Func<double, ImmutableArray<double>, Fin<ImmutableArray<double>>> field) =>
        state => field(arg1: state.Time, arg2: state.Values).Map(static rate => new FieldState(1.0, rate));

    private static ImmutableArray<double> Combine(ImmutableArray<double> left, ImmutableArray<double> right, double gain) =>
        [.. left.Select((value, index) => value + (gain * right[index]))];

    // Divide by atol + rtol·|yᵢ| BEFORE squaring, so an infinity in the error channel is norm policy, never
    // overflow, and the kernel's adaptive tolerance stays the dimensionless 1.0 target.
    private static double ScaledRms(ImmutableArray<double> delta, ImmutableArray<double> scale) =>
        Math.Sqrt(d: delta.Select((value, index) => (value / scale[index]) * (value / scale[index])).Sum() / Math.Max(val1: 1, val2: delta.Length));
}

public static class Trajectory {
    public static Fin<TrajectoryRun<TState>> Trace<TState, TDelta>(TrajectorySpec<TState, TDelta> spec, TrajectoryControl control, Op key) =>
        from seeded in Admit(spec: spec, control: control)
        select Settle(phase: Run(seeded: seeded, spec: spec, control: control, key: key, spill: None), budget: spec.Integrator.RejectBudget);

    // Spilled trace over Runtime/codecs#HDF_ARCHIVE: the station stream is `[Stations.Count, width]` chunked
    // `[1, width]`, fileDims known at Admit — the station set admits whole and the projector width reads once
    // off the initial state — and the monotone station cursor IS the chunk-once law: station ordinal is chunk
    // ordinal, no second cursor. The station-axis attribute lets a reader re-derive rows without the spec.
    public static Fin<TrajectoryRun<TState>> Trace<TState, TDelta>(TrajectorySpec<TState, TDelta> spec, TrajectoryControl control, TraceSpill spill, Op key) =>
        from seeded in Admit(spec: spec, control: control)
        from run in Try.lift(() => {
                int width = spec.Project(spec.Initial).Length;
                H5Dataset<double[]> slot = new(fileDims: [(ulong)spec.Stations.Count, (ulong)width], chunks: [1u, (uint)width], datasetCreation: spill.Archive.Creation());
                H5File graph = new() { ["stations"] = slot };
                graph.Attributes["stations"] = spec.Stations.ToArray();
                graph.Attributes["width"] = width;
                using HdfWriter session = HdfArchive.Begin(graph, spill.Sink, spill.Archive);
                StationSink<TState> sink = new(session, slot, state => spec.Project(state).ToArray(), spec.Stations.Count, width);
                return Settle(phase: Run(seeded: seeded, spec: spec, control: control, key: key, spill: Some(sink)), budget: spec.Integrator.RejectBudget);
            }).Run()
            .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<trajectory-spill:{error.Message}>"))
        select run;

    // `Project` COPIES each station row out of the span at write, because the chunk crosses into the writer's
    // staging after the accepted span — and the state that backed it — is already released.
    private sealed record StationSink<TState>(HdfWriter Session, H5Dataset<double[]> Slot, Func<TState, double[]> Project, int Count, int Width);

    // One phase still Advancing when the bound expires IS budget exhaustion, which is exactly what Settle reads
    // off it, so the loop carries no terminal of its own and the ceiling stays a bound rather than a branch.
    private static TrajectoryPhase<TState> Run<TState, TDelta>(TrajectoryCursor<TState> seeded, TrajectorySpec<TState, TDelta> spec, TrajectoryControl control, Op key, Option<StationSink<TState>> spill) {
        TrajectoryPhase<TState> phase = new TrajectoryPhase<TState>.Advancing(seeded);
        for (int taken = 0; taken < control.MaxSteps && phase is TrajectoryPhase<TState>.Advancing advancing; taken++) {
            phase = Advance(cursor: advancing.Cursor, spec: spec, control: control, key: key, spill: spill);
        }

        return phase;
    }

    private static Fin<TrajectoryCursor<TState>> Admit<TState, TDelta>(TrajectorySpec<TState, TDelta> spec, TrajectoryControl control) =>
        from budget in guard(control.IsValid,
            (Error)new ComputeFault.ModelRejected($"<trajectory-control:steps={control.MaxSteps}:min={control.MinStep:e3}>")).ToFin()
        from span in guard(double.IsFinite(spec.Start) && spec.Horizon > spec.Start && spec.FirstStep > 0.0 && double.IsFinite(spec.FirstStep),
            (Error)new ComputeFault.ModelRejected($"<trajectory-span:{spec.Start}:{spec.Horizon}:{spec.FirstStep}>")).ToFin()
        from stations in guard(
            spec.Stations.Zip(spec.Stations.Skip(1)).ForAll(static pair => pair.First < pair.Second)
            && spec.Stations.ForAll(station => station >= spec.Start && station <= spec.Horizon),
            (Error)new ComputeFault.ModelRejected($"<trajectory-stations:{spec.Stations.Count}>")).ToFin()
        select new TrajectoryCursor<TState>(
            Time: spec.Start, Step: Math.Min(val1: spec.FirstStep, val2: control.MaxStep), State: spec.Initial,
            Steps: 0, Rejects: 0, Streak: 0, Error: Option<double>.None, Samples: [], Station: 0);

    private static TrajectoryPhase<TState> Advance<TState, TDelta>(TrajectoryCursor<TState> cursor, TrajectorySpec<TState, TDelta> spec, TrajectoryControl control, Op key, Option<StationSink<TState>> spill) {
        double step = Math.Min(val1: Math.Min(val1: cursor.Step, val2: control.MaxStep), val2: spec.Horizon - cursor.Time);
        return spec.Integrator
            .Step(module: spec.Carrier(arg: cursor.State), sample: spec.Field, state: cursor.State, h: step, key: key)
            .Match(
                Succ: outcome => outcome.Switch(
                    state: (Cursor: cursor, Step: step, Spec: spec, Control: control, Key: key, Spill: spill),
                    acceptedCase: static (s, accepted) => Accepted(cursor: s.Cursor, step: s.Step, accepted: accepted, spec: s.Spec, control: s.Control, key: s.Key, spill: s.Spill),
                    rejectedCase: static (s, rejected) => Rejected(cursor: s.Cursor, rejected: rejected, spec: s.Spec, control: s.Control)),
                Fail: _ => (TrajectoryPhase<TState>)new TrajectoryPhase<TState>.Halted(cursor, TrajectoryTerminal.FieldRefused));
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
                    Samples = taken.Samples,
                    Station = taken.Station,
                },
                spec: spec, control: control),
            Fail: _ => (TrajectoryPhase<TState>)new TrajectoryPhase<TState>.Halted(cursor, TrajectoryTerminal.DenseRefused));

    private static TrajectoryPhase<TState> Rejected<TState, TDelta>(
        TrajectoryCursor<TState> cursor, IntegrationStep<TState, TDelta>.RejectedCase rejected,
        TrajectorySpec<TState, TDelta> spec, TrajectoryControl control) {
        TrajectoryCursor<TState> next = cursor with {
            Step = Math.Min(val1: rejected.SuggestedStep, val2: control.MaxStep),
            Rejects = cursor.Rejects + 1,
            Streak = cursor.Streak + 1,
            Error = rejected.Error,
        };
        // One flattened joint pattern over the three refusal discriminants; a nested ladder states the same law
        // three levels deep and hides which gate fired. The first arm anchors the union base, since sibling case
        // types leave best-common-type inference empty.
        return (Finite: rejected.Error.Map(double.IsFinite).IfNone(noneValue: true),
                Within: next.Streak <= spec.Integrator.RejectBudget,
                Above: next.Step >= control.MinStep) switch {
            (Finite: false, _, _) => (TrajectoryPhase<TState>)new TrajectoryPhase<TState>.Halted(next, TrajectoryTerminal.NonFinite),
            (_, Within: false, _) => new TrajectoryPhase<TState>.Halted(next, TrajectoryTerminal.BudgetExhausted),
            (_, _, Above: false) => new TrajectoryPhase<TState>.Halted(next, TrajectoryTerminal.StepUnderflow),
            _ => new TrajectoryPhase<TState>.Advancing(next),
        };
    }

    // Final landing step clamped below MinStep is completion, never a stall, so the horizon test runs first.
    private static TrajectoryPhase<TState> Land<TState, TDelta>(TrajectoryCursor<TState> cursor, TrajectorySpec<TState, TDelta> spec, TrajectoryControl control) =>
        spec.Horizon - cursor.Time <= control.MinStep
            ? (TrajectoryPhase<TState>)new TrajectoryPhase<TState>.Halted(cursor, TrajectoryTerminal.Completed)
            : cursor.Step < control.MinStep
                ? new TrajectoryPhase<TState>.Halted(cursor, TrajectoryTerminal.StepUnderflow)
                : new TrajectoryPhase<TState>.Advancing(cursor);

    // Stations are ascending and admitted in range, so the cursor advances monotonically and each accepted span
    // reads only its own tail — the dense receipt already proved the interpolant at admission. A spilled harvest
    // writes each station as its own chunk at its own ordinal — write-once holds because the cursor is monotone
    // — and accumulates NO sample seq: the stream is the record, and the memory bound is the spill's purpose.
    private static Fin<(Seq<TrajectorySample<TState>> Samples, int Station)> Harvest<TState, TDelta>(
        TrajectoryCursor<TState> cursor, double at, double step, DenseOutputSpan<TState, TDelta> dense, Seq<double> stations, Op key, Option<StationSink<TState>> spill) =>
        stations.Skip(cursor.Station).TakeWhile(station => station <= at + step)
            .TraverseM(station => dense.PointAt(theta: (station - at) / step, key: key).Map(state => new TrajectorySample<TState>(station, state)))
            .As()
            .Map(taken => spill.Match(
                Some: sink => {
                    int ordinal = cursor.Station;
                    foreach (TrajectorySample<TState> sample in taken) {
                        sink.Session.WriteChunk(sink.Slot, sink.Project(sample.State), ordinal, grid: [sink.Count, 1], chunkShape: [1u, (uint)sink.Width]);
                        ordinal++;
                    }

                    return (Samples: cursor.Samples, Station: cursor.Station + taken.Count);
                },
                None: () => (Samples: cursor.Samples + taken, Station: cursor.Station + taken.Count)));

    private static TrajectoryRun<TState> Settle<TState>(TrajectoryPhase<TState> phase, int budget) => phase.Switch(
        state: budget,
        advancing: static (b, a) => Landed(cursor: a.Cursor, terminal: TrajectoryTerminal.BudgetExhausted, budget: b),
        halted: static (b, h) => Landed(cursor: h.Cursor, terminal: h.Terminal, budget: b));

    private static TrajectoryRun<TState> Landed<TState>(TrajectoryCursor<TState> cursor, TrajectoryTerminal terminal, int budget) =>
        new(Final: cursor.State, Terminal: terminal, Achieved: cursor.Time, Steps: cursor.Steps, Rejects: cursor.Rejects,
            RejectBudget: budget, LastError: cursor.Error, Harvested: cursor.Station, Samples: cursor.Samples);
}
```

## [04]-[SPECTRAL_OPERATOR]

- Owner: `SpectralSymbol` the `[SmartEnum<string>]` Fourier-multiplier vocabulary carrying each operator's symbol delegate and parity; `SymbolParity` the composing parity policy; `Spectral` the pointwise-product composition carrier; `WaveAxis` the split-spectrum wavenumber owner; `SpectralControl` the imaginary-residual band; `SpectralOperator.Apply` the forward-multiply-inverse application with its Hermitian gate.
- Cases: `SpectralSymbol` rows derivative (`i·k`), laplacian (`−k²`), biharmonic (`k⁴`), hilbert (`−i·sgn k`), anti-derivative (`1/(i·k)`, zero mode killed); `SymbolParity` even, odd carrying `ZeroesNyquist`.
- Entry: `SpectralOperator.Apply(ReadOnlySpan<double> field, WaveAxis axis, Spectral op, SpectralControl? control = null)` — the composition is the value, so a chained operator and a single symbol are the same call.
- Auto: `Spectral.At(k)` is the pointwise product of factor rows and `Spectral.Parity` the XOR-fold of factor parities, so the Nyquist bin zeroes exactly when the composition is odd; `Apply` dispatches on field length — even lengths ride the packed-real `Fourier.ForwardReal`/`InverseReal` pair (half the transform work, real output BY CONSTRUCTION, the symbol multiplying each nonnegative-k packed pair so Hermitian symmetry is structural), odd lengths ride the `Complex[]` pair whose imaginary-residual gate diagnoses broken symmetry — both over an internal spectral copy so the caller's field is never mutated; admission is the vectorized `TensorPrimitives.IsFiniteAll` sweep, never a per-element predicate walk; `WaveAxis.K()` derives the split-spectrum angular wavenumbers from `Fourier.FrequencyScale` scaled by 2π, never a hand-indexed bin walk.
- Receipt: `SpectralEvidence` carries the real result as owned `ImmutableArray<double>`, the composed parity, and `Option<double>` imaginary residual — `Some` only on the complex leg where the gate read it, `None` on the packed-real leg whose realness is structural; excess residual fails the complex leg as broken Hermitian symmetry, never a usable result.
- Packages: MathNet.Numerics, System.Numerics.Tensors, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm (project — the `SpectralScaling` convention row), BCL inbox
- Growth: a new operator is one `SpectralSymbol` row with its symbol delegate and parity; a composite is a `Spectral.Then` chain; a tighter Hermitian band is one `SpectralControl` value — zero new code path.
- Boundary — every constant-coefficient periodic operator is one `SpectralSymbol` row applied pointwise to the forward transform; symbols compose by pointwise multiplication before a single inverse, and parity is row data the operator owns, never a `bool oddOrder` knob nor a bare `Func<double, Complex>` riding beside the call.
- Boundary — the split-spectrum wavenumber derives once at grid construction (ascending positives through Nyquist, then descending negatives, scaled by `2π/extent`) because hand-indexing the bin applies an aliased symbol past the half length silently; both transform legs READ the kernel `Numerics/matrix#TRANSFORM_BAND` row — `SpectralScaling.AsymmetricInverse.FourierConvention` is the option value, never a re-spelled `FourierOptions.AsymmetricScaling` literal, so one convention vocabulary spans the strata boundary and the row this clause names as the convention owner is the row the fence binds. Its `RoundTrip` column proves the discriminant is NOT round-trip identity (`Symmetric` and `AsymmetricInverse` both return 1.0, only `Unscaled` carries the N factor): this lane READS BETWEEN the legs, so the unscaled forward leaves the intermediate bins carrying true DFT coefficients and a symbol's magnitude IS the operator's transfer function, where a `1/√N` forward rescales every spectrum the imaginary-residual gate and any bin-domain consumer inspect. `SpectralControl` owns the residual band because a bare per-module literal is unreplayable and uncomparable across operators.
- Boundary — `SpectralControl` binds the COMPLEX leg ALONE, and the discriminant is field PARITY, never caller intent. Even-length fields ride the packed-real pair whose output is real by construction — no imaginary channel to measure, the floor goes unread, and the evidence reports `None`; passing a tighter floor with an even grid changes nothing the run does. That asymmetry is why the residual is `Option<double>`, never a `0.0` written by both legs — a reader treating a missing residual as a passed gate has inverted the one leg that proves Hermitian symmetry.
- Boundary — discriminant against the `Stats/signal#SIGNAL_LANE` `SpectralTransform` axis is spatial-versus-sampled, never availability: a symbol is a differential operator over a SPATIAL extent in angular wavenumber, that axis transform-and-invert, framing, windowing, and filter design over a SAMPLE RATE in bin frequency. Collapsing either end hands a spatial operator frame, hop, and window evidence it has none of, or a spectrogram a parity column no transform owns.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SymbolParity {
    public static readonly SymbolParity Even = new("even", zeroesNyquist: false);
    public static readonly SymbolParity Odd = new("odd", zeroesNyquist: true);

    public bool ZeroesNyquist { get; }

    // Parity composes by XOR (even∘even = even, odd∘odd = even, mixed = odd); an odd composite zeroes the Nyquist bin (odd symbols are discontinuous across ±Nyquist).
    public SymbolParity Compose(SymbolParity other) => this == other ? Even : Odd;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpectralSymbol {
    // σ(k): the Fourier multiplier, Hermitian-conjugate symmetric (σ(−k) = conj σ(k)) so it maps real to real;
    // Antiderivatives kill the undetermined zero mode instead of dividing by zero.
    public static readonly SpectralSymbol Derivative = new("derivative", symbol: static k => new Complex(0.0, k), parity: SymbolParity.Odd);
    public static readonly SpectralSymbol Laplacian = new("laplacian", symbol: static k => new Complex(-k * k, 0.0), parity: SymbolParity.Even);
    public static readonly SpectralSymbol Biharmonic = new("biharmonic", symbol: static k => new Complex(k * k * k * k, 0.0), parity: SymbolParity.Even);
    public static readonly SpectralSymbol Hilbert = new("hilbert", symbol: static k => new Complex(0.0, -Math.Sign(value: k)), parity: SymbolParity.Odd);
    public static readonly SpectralSymbol AntiDerivative = new("anti-derivative", symbol: static k => k == 0.0 ? Complex.Zero : new Complex(0.0, -1.0 / k), parity: SymbolParity.Odd);

    private readonly Func<double, Complex> symbol;

    public SymbolParity Parity { get; }

    public Complex At(double wavenumber) => symbol(arg: wavenumber);
}

public readonly record struct WaveAxis(int Length, double Extent) {
    // `FrequencyScale` supplies transform-order cycles per unit; scaling by `2π` yields angular wavenumbers
    // without a hand-indexed bin walk.
    public double[] K() =>
        [.. Fourier.FrequencyScale(Length, Length / Extent).Select(static cycles => 2.0 * Math.PI * cycles)];

    public int Nyquist => (Length & 1) == 0 ? Length >> 1 : -1;
}

// --- [MODELS] ------------------------------------------------------------------------------
public sealed record Spectral {
    private Spectral(Seq<SpectralSymbol> factors) => Factors = factors;

    public Seq<SpectralSymbol> Factors { get; }

    public static Spectral Of(SpectralSymbol symbol) => new(Seq(symbol));

    public Spectral Then(SpectralSymbol next) => new(Factors.Add(next));

    public SymbolParity Parity => Factors.Fold(SymbolParity.Even, static (acc, factor) => acc.Compose(other: factor.Parity));

    public Complex At(double wavenumber) => Factors.Fold(Complex.One, (acc, factor) => acc * factor.At(wavenumber: wavenumber));
}

// Real-symbol operators owe a machine-zero imaginary part; this band is a policy value so two operators stay
// comparable and a tightening is one declaration, never a literal edited at a kernel.
public sealed record SpectralControl(double ImaginaryFloor) {
    public static readonly SpectralControl Default = new(ImaginaryFloor: 1e-8);
}

// ImaginaryResidual is Some only on the complex (odd-length) leg — the packed-real leg is real by
// construction, so its Hermitian gate is structural and the channel reports honest absence.
public sealed record SpectralEvidence(ImmutableArray<double> Field, Option<double> ImaginaryResidual, SymbolParity Parity);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class SpectralOperator {
    // Field length selects packed-real even transforms or complex odd transforms; the latter's imaginary
    // residual diagnoses broken Hermitian symmetry.
    public static Fin<SpectralEvidence> Apply(ReadOnlySpan<double> field, WaveAxis axis, Spectral op, SpectralControl? control = null) =>
        axis.Length < 2 || !double.IsFinite(axis.Extent) || axis.Extent <= 0.0 || field.Length != axis.Length || !TensorPrimitives.IsFiniteAll<double>(field)
            ? Fin.Fail<SpectralEvidence>(new ComputeFault.ModelRejected($"<wave-axis-mismatch:field={field.Length}:axis={axis.Length}>"))
            : (field.Length & 1) == 0
                ? PackedReal(field, axis, op)
                : ComplexLeg(field, axis, op, control ?? SpectralControl.Default);

    // `ForwardReal` packs `n` samples into `n+2` interleaved bin components; the symbol multiplies each pair,
    // odd parity zeroes Nyquist, and `InverseReal` returns without complex materialization.
    static Fin<SpectralEvidence> PackedReal(ReadOnlySpan<double> field, WaveAxis axis, Spectral op) {
        double[] k = axis.K();
        bool killNyquist = op.Parity.ZeroesNyquist;
        int half = axis.Length >> 1;
        double[] packed = new double[axis.Length + 2];
        field.CopyTo(packed);
        Fourier.ForwardReal(packed, axis.Length, SpectralScaling.AsymmetricInverse.FourierConvention);
        for (int bin = 0; bin <= half; bin++) {
            Complex factor = killNyquist && bin == half ? Complex.Zero : op.At(wavenumber: k[bin]);
            Complex scaled = new Complex(packed[2 * bin], packed[2 * bin + 1]) * factor;
            (packed[2 * bin], packed[2 * bin + 1]) = (scaled.Real, scaled.Imaginary);
        }

        Fourier.InverseReal(packed, axis.Length, SpectralScaling.AsymmetricInverse.FourierConvention);
        return Fin.Succ(new SpectralEvidence([.. packed[..axis.Length]], None, op.Parity));
    }

    static Fin<SpectralEvidence> ComplexLeg(ReadOnlySpan<double> field, WaveAxis axis, Spectral op, SpectralControl control) {
        double[] k = axis.K();
        int nyquist = axis.Nyquist;
        bool killNyquist = op.Parity.ZeroesNyquist;
        Complex[] spectrum = new Complex[field.Length];
        for (int i = 0; i < spectrum.Length; i++) { spectrum[i] = new Complex(field[i], 0.0); }

        Fourier.Forward(spectrum, SpectralScaling.AsymmetricInverse.FourierConvention);
        for (int i = 0; i < spectrum.Length; i++) {
            spectrum[i] *= killNyquist && i == nyquist ? Complex.Zero : op.At(wavenumber: k[i]);
        }

        Fourier.Inverse(spectrum, SpectralScaling.AsymmetricInverse.FourierConvention);
        double[] result = new double[spectrum.Length];
        (double real, double imaginary) = (0.0, 0.0);
        for (int i = 0; i < spectrum.Length; i++) {
            result[i] = spectrum[i].Real;
            (real, imaginary) = (Math.Max(real, Math.Abs(spectrum[i].Real)), Math.Max(imaginary, Math.Abs(spectrum[i].Imaginary)));
        }

        return imaginary / Math.Max(real, double.Epsilon) is var residual && residual < control.ImaginaryFloor
            ? Fin.Succ(new SpectralEvidence([.. result], Some(residual), op.Parity))
            : Fin.Fail<SpectralEvidence>(new ComputeFault.ModelRejected($"<imaginary-residual:r={residual:e3}:floor={control.ImaginaryFloor:e3}>"));
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
