# [APPHOST_LIFECYCLE_AND_DRAIN]

Rasm.AppHost runs one process lifecycle: eight `RuntimePhase` rows and one `PhaseStep` correspondence carrying every legal edge, one Atom-backed `Lifecycle` capsule committing through the kernel transition mechanism and minting a `PhaseReceipt` per commit, a five-case `FaultSource` spine with crash-marker and upgrade boot probing, a rank-band drain conductor folding participant rows into one `DrainReceipt` of gauged crossings, and one `CancelScope` spine beneath which every cancellation token derives. Owned axes are the phase family with its step correspondence, the fault traps, the boot log-event run, the frozen drain bands with their capability column, and cancellation provenance over Microsoft.Extensions.Hosting lifetime tokens, Thinktecture-generated vocabulary, Riok.Mapperly wire projection, LanguageExt rails, and NodaTime instants.

Settled composition: `CorrelationId` arrives from the kernel frame capsule `Rasm/Domain/frame#SOURCE` and `Observability/telemetry#CORRELATION_SPINE` `Correlation.Mint` performs the boot mint, so this capsule takes the minted value at construction and threads it as the identity every phase, fault, and drain receipt stamps. `FaultBand` and its `Code(offset)` derivation arrive from `Rasm/Domain/rails#FAULT_BAND`, the one registry in the branch, with `[FaultCase]`/`Fault`/`generated identity admission` the folder fault-estate floor every AppHost family rides; `Transition<TState>` and `Cell` from `Rasm/Domain/rails#TRANSITION`; `CapabilitySet`/`ICapability`/`CapabilityLaw` from `Rasm/Domain/validation#CAPABILITY`; `MonotonicTimeline`, `GaugedSpan`, and `IGaugeLane` from `Rasm/Parametric/projections#TIMELINE`. `HookRail<AppHostPoint, AppHostFact, TelemetrySource>` and the `AppHostPoint`/`AppHostFact` rosters arrive from `Observability/hooks#HOOK_ROSTER`, so every phase publish and every degradation read crosses one rail rather than a per-capsule fan-out; `ClockPolicy`, `DeadlineClass`, `DeadlineOutcome`, and `DeadlineReceipt` from `Runtime/time#CLOCK_SPLIT` and `#DEADLINE_TAXONOMY`; `ILatencyContext`, `CheckpointToken`, and `LatencySpine.Mark` from `Observability/telemetry#SIGNAL_GOVERNANCE`; `AppHostMeasure`, `AppHostSlot`, and `InstrumentSet` from `Observability/instruments#INSTRUMENT_CATALOG`.

## [01]-[INDEX]

- [02]-[PHASE_FAMILY]: Eight phases, one step correspondence, kernel-committed transitions, boot log events.
- [03]-[FAULT_SPINE]: Five fault sources, trap registrations, crash-marker probe, one generated wire projection.
- [04]-[DRAIN_CONDUCTOR]: Frozen rank bands fold participant rows into one receipt of gauged crossings.
- [05]-[CANCEL_SPINE]: One root source; derived scopes carry `Op` provenance segments and deadline rows.

## [02]-[PHASE_FAMILY]

- Owner: `RuntimePhase` `[SmartEnum<string>]` eight rows; `PhaseStep` `[SmartEnum<string>]` the ONE phase correspondence — every row carries its admitting phase set, its target, and its evidence-free mint, and `Next`, the phase-shaped admission, and the receipt's trigger key are all projections of it; `PhaseTrigger` `[Union]` the trigger vocabulary, each case naming its own step; `Lifecycle` the boundary capsule owning the Atom-backed receipt cell, the injected hook rail, and the boot-minted `CorrelationId`; `LifecycleFault` the fault family riding the kernel `[FaultCase]`/`Fault` floor — `[FaultCase]` realizes the registry over `FaultBand.Lifecycle` (its `Band` accessor is the ONE band read), `Code` derive SEALED, and `generated identity admission` proves the offset roster at first construction; `LifecycleLog` the boot log-event run based at `FaultBand.SpineEventsBase`; `PhaseSubscription` the LIFO detacher composite.
- Cases: eight phases; ten `PhaseStep` rows, each the key one `PhaseTrigger` case carries; `LifecycleFault` = IllegalTransition | ModuleRejected | ActivationRejected.
- Entry: `Fin<PhaseReceipt> Transition(PhaseTrigger trigger)` — `Cell.Step` commits the candidate and the refused verdict becomes the `IllegalTransition` rail; the `RuntimePhase`-shaped overload admits evidence-free targets from host-attach injection through `PhaseStep.Derived`, which resolves against the phase HELD rather than the target alone; `IO<T> Captured<T>(IO<T> body)` brackets a support capture, so the resume phase is read off the cell rather than supplied; `HookTap<…> DegradationTap` is the degradation-to-phase producer the composition seats on the rail.
- Auto: every settled commit fires `AppHostPoint.Phase` once with the committed receipt and the latest receipt is the cell value itself; `Attach` projects the host lifetime tokens into transitions — never a second state machine; `DegradationTap` folds each `DegradationReading` into the degraded or recovered step, so the degradation rail is the sole producer of both and no caller commits a level by hand; receipts flow to the receipt-sink envelope unchanged.
- Receipt: `PhaseReceipt` — from, to, step key, `Instant`, held `Duration`, profile, correlation id.
- Packages: Microsoft.Extensions.Hosting, Microsoft.Extensions.Logging, Rasm (kernel `Cell`/`Transition`/`FaultBand`/`HookRail`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime
- Growth: one phase row plus the `PhaseStep` rows naming it, or one trigger case with its step row; both break every dispatch site at compile time and neither adds a member.
- Boundary: `PhaseStep` is the primary correspondence and everything else on this page derives from it — `From` is the admitting set, `To` the target, `Free` the evidence-free mint the phase-shaped overload needs, so a phase edge is stated ONCE and the three parallel total switches that restated it (next, derived, key) are gone with the hand-kept transition diagram that mirrored them a fourth time; NAMED LOSS — the rendered edge picture, recovered by the roster's own `From`/`To` columns, which ARE the edge list and cannot drift from the law they are. `Recovered` and `CaptureCompleted` were admitted by the old transition law with no producer anywhere, which made `Degraded` and `SupportCapture` absorbing states in practice; both now have one: the degradation rail through `DegradationTap` and the capture bracket through `Captured`, whose finalizer reads the pre-capture phase off the cell so the resume target is state rather than a caller argument. `Lifecycle` is the named boundary capsule for the statement carve-out — the token registration and trap wiring carry language-owned statement forms while every other member stays expression-shaped; the candidate mints against the held receipt inside `Cell.Step` and the ONE clock read is hoisted outside it, so a contended retry re-derives the receipt from the state it actually lost to and never re-reads the clock; the fire is the SETTLED commit rather than the swap body, so a contended retry never publishes a receipt the cell rejected, and the `Phase` row's Observe modality admits no veto — the only refusal `Fire` can answer is an unseated point, which is a composition defect and rides the rail rather than an `ignore`; subscription is a `HookTap` row the composition hands `HookRail.Of`, so a per-capsule subscribe member and its detacher both delete and `rail.Release(TelemetrySource.AppHost, key)` is the one teardown; evidence-bearing targets (faulted, capture-resume, upgrade) carry no `Free` mint, so the phase-shaped admission cannot reach them and fault evidence is never silently dropped; the boot self-loop row receipts upgrade detection without leaving boot; `PhaseReceipt.Trigger` is the step key, and the six boot events base at `FaultBand.SpineEventsBase` with the registry row `FaultBand.SpineEvents` holding the same value — the dual-owner invariant the kernel band law names, moving as one edit.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RuntimePhase {
    public static readonly RuntimePhase Boot = new("boot");
    public static readonly RuntimePhase Ready = new("ready");
    public static readonly RuntimePhase Running = new("running");
    public static readonly RuntimePhase Degraded = new("degraded");
    public static readonly RuntimePhase Draining = new("draining");
    public static readonly RuntimePhase Unloaded = new("unloaded");
    public static readonly RuntimePhase Faulted = new("faulted");
    public static readonly RuntimePhase SupportCapture = new("support-capture");
}

// --- [TABLES] ---------------------------------------------------------------------------
// THE phase correspondence. `To` is `None` on the one row whose target rides its trigger payload, and `Free`
// answers `None` on every row whose trigger carries evidence a phase name cannot reconstruct.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class PhaseStep {
    public static readonly PhaseStep Validated = new("Validated",
        from: Only(RuntimePhase.Boot), to: Some(RuntimePhase.Ready), free: static () => Some<PhaseTrigger>(new PhaseTrigger.Validated()));
    public static readonly PhaseStep Started = new("Started",
        from: Only(RuntimePhase.Ready), to: Some(RuntimePhase.Running), free: static () => Some<PhaseTrigger>(new PhaseTrigger.Started()));
    public static readonly PhaseStep Degraded = new("Degraded",
        from: Only(RuntimePhase.Running, RuntimePhase.Degraded), to: Some(RuntimePhase.Degraded), free: static () => Some<PhaseTrigger>(new PhaseTrigger.Degraded()));
    public static readonly PhaseStep Recovered = new("Recovered",
        from: Only(RuntimePhase.Degraded), to: Some(RuntimePhase.Running), free: static () => Some<PhaseTrigger>(new PhaseTrigger.Recovered()));
    public static readonly PhaseStep UpgradeDetected = new("UpgradeDetected",
        from: Only(RuntimePhase.Boot), to: Some(RuntimePhase.Boot), free: Evidence);
    public static readonly PhaseStep CaptureStarted = new("CaptureStarted",
        from: Only(RuntimePhase.Running, RuntimePhase.Degraded, RuntimePhase.Faulted), to: Some(RuntimePhase.SupportCapture),
        free: static () => Some<PhaseTrigger>(new PhaseTrigger.CaptureStarted()));
    public static readonly PhaseStep CaptureCompleted = new("CaptureCompleted",
        from: Only(RuntimePhase.SupportCapture), to: None, free: Evidence);
    public static readonly PhaseStep FaultCommitted = new("FaultCommitted",
        from: AllBut(RuntimePhase.Unloaded, RuntimePhase.Faulted), to: Some(RuntimePhase.Faulted), free: Evidence);
    public static readonly PhaseStep DrainRequested = new("DrainRequested",
        from: AllBut(RuntimePhase.Boot, RuntimePhase.Draining, RuntimePhase.Unloaded), to: Some(RuntimePhase.Draining),
        free: static () => Some<PhaseTrigger>(new PhaseTrigger.DrainRequested()));
    public static readonly PhaseStep DrainCompleted = new("DrainCompleted",
        from: Only(RuntimePhase.Draining), to: Some(RuntimePhase.Unloaded), free: static () => Some<PhaseTrigger>(new PhaseTrigger.DrainCompleted()));

    public FrozenSet<RuntimePhase> From { get; }
    public Option<RuntimePhase> To { get; }

    [UseDelegateFromConstructor]
    public partial Option<PhaseTrigger> Free();

    public Option<RuntimePhase> Next(RuntimePhase at, PhaseTrigger trigger) =>
        From.Contains(at) ? To.Match(Some: static row => Some(row), None: () => trigger.Resume) : None;

    // Target alone cannot pick a row: `Running` is reached by `Started` from ready and by `Recovered` from
    // degraded, so the held phase is half the key and a target-only lookup silently refused every recovery.
    public static Option<PhaseTrigger> Derived(RuntimePhase at, RuntimePhase target) =>
        toSeq(Items).Find(row => row.From.Contains(at) && row.To == Some(target)).Bind(static row => row.Free());

    static Option<PhaseTrigger> Evidence() => None;
    static FrozenSet<RuntimePhase> Only(params ReadOnlySpan<RuntimePhase> rows) => rows.ToArray().ToFrozenSet();
    static FrozenSet<RuntimePhase> AllBut(params ReadOnlySpan<RuntimePhase> barred) =>
        RuntimePhase.Items.Where(row => !barred.Contains(row)).ToFrozenSet();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PhaseTrigger {
    private PhaseTrigger() { }
    public abstract PhaseStep Step { get; }
    public virtual Option<RuntimePhase> Resume => None;

    public sealed record Validated : PhaseTrigger { public override PhaseStep Step => PhaseStep.Validated; }
    public sealed record Started : PhaseTrigger { public override PhaseStep Step => PhaseStep.Started; }
    public sealed record Degraded : PhaseTrigger { public override PhaseStep Step => PhaseStep.Degraded; }
    public sealed record Recovered : PhaseTrigger { public override PhaseStep Step => PhaseStep.Recovered; }
    public sealed record UpgradeDetected(Version Prior, Version Current) : PhaseTrigger { public override PhaseStep Step => PhaseStep.UpgradeDetected; }
    public sealed record CaptureStarted : PhaseTrigger { public override PhaseStep Step => PhaseStep.CaptureStarted; }
    public sealed record CaptureCompleted(RuntimePhase Target) : PhaseTrigger {
        public override PhaseStep Step => PhaseStep.CaptureCompleted;
        public override Option<RuntimePhase> Resume => Some(Target);
    }
    public sealed record FaultCommitted(FaultSource Source) : PhaseTrigger { public override PhaseStep Step => PhaseStep.FaultCommitted; }
    public sealed record DrainRequested : PhaseTrigger { public override PhaseStep Step => PhaseStep.DrainRequested; }
    public sealed record DrainCompleted(Option<DrainReceipt> Receipt = default) : PhaseTrigger { public override PhaseStep Step => PhaseStep.DrainCompleted; }
}

// --- [MODELS] ---------------------------------------------------------------------------
public readonly record struct PhaseReceipt(RuntimePhase From, RuntimePhase To, string Trigger, Instant At, Duration Held, ConsumptionProfile Profile, CorrelationId CorrelationId);

public readonly record struct PhaseSubscription(Seq<Action> Detachers) : IDisposable {
    public void Dispose() => Detachers.Rev().Iter(static detach => detach());
}

// --- [ERRORS] ---------------------------------------------------------------------------
// Lifecycle refusals retain the failing cause; text-only admission and category mirrors do not exist.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LifecycleFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Lifecycle;
    private LifecycleFault(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record IllegalTransition(RuntimePhase From, PhaseStep Step)
        : LifecycleFault($"{From.Key}:{Step.Key}");
    [FaultCase(1)]
    public sealed partial record ModuleRejected(string Module, Error Cause)
        : LifecycleFault($"module:{Module}:{Cause.Message}"), ICausedFault;
    [FaultCase(2)]
    public sealed partial record ActivationRejected(string Module, Error Cause)
        : LifecycleFault($"activation:{Module}:{Cause.Message}"), ICausedFault;
}

// --- [SERVICES] -------------------------------------------------------------------------
// The `EventId` argument is a compile-time const while the registry row is an instance, so the base const and
// `FaultBand.SpineEvents` carry the same value and move as one edit; `Code(offset)` is the runtime read.
public static partial class LifecycleLog {
    [LoggerMessage(EventId = FaultBand.SpineEventsBase + 0, Level = LogLevel.Information, Message = "boot {Version} on profile {Profile}")]
    public static partial void BootStarted(ILogger logger, Version version, string profile);
    [LoggerMessage(EventId = FaultBand.SpineEventsBase + 1, Level = LogLevel.Information, Message = "phase {From}->{To} on {Step}")]
    public static partial void PhaseCommitted(ILogger logger, string from, string to, string step);
    [LoggerMessage(EventId = FaultBand.SpineEventsBase + 2, Level = LogLevel.Warning, Message = "phase refused {Step} at {From}")]
    public static partial void PhaseRefused(ILogger logger, string step, string from);
    [LoggerMessage(EventId = FaultBand.SpineEventsBase + 3, Level = LogLevel.Warning, Message = "boot marker {Path} unreadable")]
    public static partial void MarkerDrifted(ILogger logger, string path);
    [LoggerMessage(EventId = FaultBand.SpineEventsBase + 4, Level = LogLevel.Warning, Message = "drain step {Name} forced at band {Band}")]
    public static partial void DrainForced(ILogger logger, string name, int band);
    [LoggerMessage(EventId = FaultBand.SpineEventsBase + 5, Level = LogLevel.Information, Message = "signal {Signal} trapped")]
    public static partial void SignalTrapped(ILogger logger, string signal);
}

public sealed class Lifecycle(ConsumptionProfile profile, ClockPolicy clocks, CorrelationId correlationId, HookRail<AppHostPoint, AppHostFact, TelemetrySource> rail, Op key) {
    readonly Atom<PhaseReceipt> cell = Atom(new PhaseReceipt(RuntimePhase.Boot, RuntimePhase.Boot, PhaseStep.Validated.Key, clocks.Now, Duration.Zero, profile, correlationId));
    public ConsumptionProfile Profile { get; } = profile;
    public ClockPolicy Clocks { get; } = clocks;
    public CorrelationId CorrelationId { get; } = correlationId;
    public HookRail<AppHostPoint, AppHostFact, TelemetrySource> Rail { get; } = rail;
    public Op Key { get; } = key;
    public CancelScope Spine { get; } = CancelScope.Root(Op.Of(nameof(Lifecycle)));
    public RuntimePhase Phase => cell.Value.To;
    public PhaseReceipt Latest => cell.Value;

    public Fin<PhaseReceipt> Transition(PhaseTrigger trigger) =>
        Settle(Cell.Step(
            cell: cell,
            step: Candidate(trigger: trigger, at: Clocks.Now),
            declined: new LifecycleFault.IllegalTransition(cell.Value.To, trigger.Step)));

    public Fin<PhaseReceipt> Transition(RuntimePhase target) =>
        PhaseStep.Derived(at: cell.Value.To, target: target)
            .ToFin(new LifecycleFault.IllegalTransition(cell.Value.To, PhaseStep.DrainRequested))
            .Bind(Transition);

    // The resume phase is READ OFF the cell, so the capture bracket is the whole producer of both capture steps
    // and a caller cannot name a resume target the process was never in.
    public IO<T> Captured<T>(IO<T> body) =>
        from entered in Railed(Transition(new PhaseTrigger.CaptureStarted()))
        from held in body.Bracket(
            Use: static value => IO.pure(value),
            Fin: _ => IO.lift(() => ignore(Transition(new PhaseTrigger.CaptureCompleted(entered.From)))))
        select held;

    // The degradation rail is the ONE producer of the degraded and recovered steps: the reading's own level
    // decides the step and a refused transition (already at that phase) is the ordinary answer, not a fault.
    public HookTap<AppHostPoint, AppHostFact, TelemetrySource> DegradationTap =>
        new(Name: Op.Of(nameof(DegradationTap)),
            Observe: fact => fact.Switch(
                receipt: static _ => Fin.Succ(unit),
                phase: static _ => Fin.Succ(unit),
                command: static _ => Fin.Succ(unit),
                delivery: static _ => Fin.Succ(unit),
                degradation: row => ignore(Transition(row.Reading.Level == DegradationLevel.Full
                    ? new PhaseTrigger.Recovered()
                    : (PhaseTrigger)new PhaseTrigger.Degraded())) is var _ ? Fin.Succ(unit) : Fin.Succ(unit),
                profile: static _ => Fin.Succ(unit)),
            Scope: Some(Seq(AppHostPoint.Degradation)),
            Owner: Some(TelemetrySource.AppHost));

    public PhaseSubscription Attach(IHostApplicationLifetime lifetime) {
        var started = lifetime.ApplicationStarted.Register(() => ignore(Transition(RuntimePhase.Running)));
        var stopping = lifetime.ApplicationStopping.Register(() => ignore(Transition(RuntimePhase.Draining)));
        var stopped = lifetime.ApplicationStopped.Register(() => ignore(Transition(RuntimePhase.Unloaded)));
        return new PhaseSubscription([started.Dispose, stopping.Dispose, stopped.Dispose]);
    }

    internal IO<T> Railed<T>(Fin<T> settled) => settled.Match(Succ: IO.pure, Fail: IO.fail<T>);

    Func<PhaseReceipt, Option<PhaseReceipt>> Candidate(PhaseTrigger trigger, Instant at) =>
        held => trigger.Step.Next(at: held.To, trigger: trigger)
            .Map(next => new PhaseReceipt(held.To, next, trigger.Step.Key, at, at - held.At, Profile, CorrelationId));

    Fin<PhaseReceipt> Settle(Transition<PhaseReceipt> verdict) =>
        verdict switch {
            Transition<PhaseReceipt>.Committed committed =>
                Rail.Fire(at: AppHostPoint.Phase, fact: new AppHostFact.Phase(Commit: committed.State), key: Key)
                    .Map(_ => committed.State),
            Transition<PhaseReceipt>.Refused refused => Fin.Fail<PhaseReceipt>(refused.Cause),
        };
}
```

## [03]-[FAULT_SPINE]

- Owner: `FaultSource` `[Union]` five cases; `BootMarker` the crash and upgrade marker record; `FaultRecord` the kind-discriminated wire projection; `FaultRecordMap` the one generated mapper between them; `FaultSpine` the trap and probe surface.
- Cases: Unhandled, UnobservedTask, Signalled, HostCrashMarker, MarkerDrifted; `TerminationKind` = terminating | observed.
- Entry: `PhaseSubscription ArmTraps(Option<Action<SupportTrigger>> capture = default, Option<Action> reload = default)` — one LIFO detacher composite over every trap registration; the capture arm receives one `SupportTrigger.FaultTransition(host.CorrelationId, FaultRecordMap.From(source))` fact rather than the raw `FaultSource`, so a fault commit and its support-capture trigger are one fact stream under the capsule's own boot identity and `ProbeMarkers` boot evidence rides the identical case.
- Auto: every in-process fault commit folds its `FaultSource` through the generated projection into one `SupportTrigger.FaultTransition` and emits that single fact to the capture arm before the `PhaseTrigger.FaultCommitted` transition, so the capture trigger and the phase commit derive from one `Commit` fold; SIGTERM and SIGQUIT project to the drain transition; SIGHUP ENQUEUES onto the reload delegate rather than folding inline, because the runtime dispatches SIGHUP on the ThreadPool while SIGINT, SIGQUIT, and SIGTERM get a dedicated signal thread — a saturated pool turns an inline reload into a missed service-manager reload deadline with the prior values still live.
- Packages: Microsoft.Extensions.Hosting.Systemd, Riok.Mapperly, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one trap registration row inside `ArmTraps` or one host-marker path value; one new fault cause is one `FaultSource` case, one `FaultRecord` case, and one `[MapDerivedType]` row the generator completes.
- Boundary: `TerminationKind` replaces the `bool Terminating` column on both the source and its wire twin — the CLR's own runtime disposition is a two-row vocabulary a boolean product never named, and it crosses the wire as its key rather than as `true`; the projection is ONE generated `[Mapper]`: every `Error` first enters the kernel's bounded `FaultObservation.Of`, then the app-owned wire mapper carries generated code, typed recovery, exact cause stamps, and truncation without `Message`; the full value remains in `FaultSource` custody, and a new case that forgets its row is a build break rather than a missing arm. `MarkerDrifted` is the case the discarded parse cause becomes: the codec rejects unmapped members, which makes a marker written by a prior version carrying a retired field a DESERIALIZE FAILURE — precisely the drift a marker exists to survive — so a present-but-unreadable marker now reports the CAUSE it refused under where the prior `IfFail(None)` erased it into an ordinary absent-detail crash; presence stays the crash fact, and upgrade detection simply does not fire on drift because a version that cannot be read is not a version that changed. `FaultSpine` is the named boundary capsule for the statement carve-out — trap wiring and signal handlers carry language-owned statement forms; plugin rows arm no posix traps because the host owns process signals; the marker path is one fact, `<ProfileRoots.SupportRoot>/boot-marker.json`, with `SupportRoot` computed outside the topology switch so every deployment row writes the identical path; stale own markers and host `.rhl`/`.ips` markers project to `HostCrashMarker` evidence flowing through the same projection into one `SupportTrigger.FaultTransition`, never a live fault transition and never a second capture path; the marker writes at boot, clears on clean drain, and its version stamp doubles as upgrade-boot detection; SIGHUP is registered wherever reload is offered because an UNREGISTERED SIGHUP kills the process — measured, exit 129, the inherited default disposition — and neither the runtime nor `SystemdLifetime` installs a handler for it, so the `Cancel = true` on this arm is the only reason a reload signal is survivable; under a service manager that death is INVISIBLE — SIGHUP sits in systemd's clean-exit signal set beside SIGINT, SIGTERM, and SIGPIPE, so the manager records `Result=success` with no failed entry and no journal exit line, and a `Restart=on-failure` unit measurably STAYS DEAD where the same kill by SIGUSR1 restarts it; the reload path itself is likewise not free — `systemctl reload` is REJECTED outright on a unit carrying no `ExecReload=`, so a manager-driven reload reaches this arm only through a declared reload row or an out-of-band `systemctl kill --signal=SIGHUP`; the SIGTERM arm is this spine's ALONE on every topology — the `Managed` attach row registers `AddSystemd`, whose own SIGTERM handler calls `StopApplication` and reaches this cell through the `ApplicationStopping` token, and a second owner here drives two `Draining` transitions for one signal where the second is a rejected `Fin` the fold discards; the fault-to-capture path is one fact with kind metadata the durable-orchestration crash-recovery reads to resume in-flight steps (`Runtime/orchestration#CRASH_RESUME`, `Observability/bundles#TRIGGER_UNION`).

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TerminationKind {
    public static readonly TerminationKind Terminating = new("terminating");
    public static readonly TerminationKind Observed = new("observed");

    public static TerminationKind Of(bool terminating) => terminating ? Terminating : Observed;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FaultSource {
    private FaultSource() { }
    public sealed record Unhandled(Error Evidence, TerminationKind Termination) : FaultSource;
    public sealed record UnobservedTask(Error Evidence) : FaultSource;
    public sealed record Signalled(PosixSignal Signal) : FaultSource;
    public sealed record HostCrashMarker(string Path, Option<BootMarker> Marker = default) : FaultSource;
    public sealed record MarkerDrifted(string Path, Error Cause) : FaultSource;
}

// --- [MODELS] ---------------------------------------------------------------------------
public sealed record BootMarker(int Pid, RuntimePhase Phase, Version AppVersion, Instant StartedAt);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(FaultRecord.Unhandled), "unhandled")]
[JsonDerivedType(typeof(FaultRecord.UnobservedTask), "unobserved-task")]
[JsonDerivedType(typeof(FaultRecord.Signalled), "posix-signal")]
[JsonDerivedType(typeof(FaultRecord.HostCrashMarker), "host-crash-marker")]
[JsonDerivedType(typeof(FaultRecord.MarkerDrifted), "marker-drifted")]
public abstract partial record FaultRecord {
    private FaultRecord() { }
    public sealed record Unhandled(FaultObservationWire Evidence, TerminationKind Termination) : FaultRecord;
    public sealed record UnobservedTask(FaultObservationWire Evidence) : FaultRecord;
    public sealed record Signalled(string Signal) : FaultRecord;
    public sealed record HostCrashMarker(string Path, Option<BootMarker> Marker = default) : FaultRecord;
    public sealed record MarkerDrifted(string Path, FaultObservationWire Cause) : FaultRecord;
}

// --- [BOUNDARIES] -----------------------------------------------------------------------
// One converter per crossing TYPE, so the erasure is declared once and every arm that carries an `Error` or a
// `PosixSignal` reads it; a generic `T? Map<T>(Option<T>)` is refused by the analyzer and by this seam.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both,
        EnabledConversions = MappingConversionType.All & ~MappingConversionType.ExplicitCast)]
internal static partial class FaultRecordMap {
    [MapDerivedType<FaultSource.Unhandled, FaultRecord.Unhandled>]
    [MapDerivedType<FaultSource.UnobservedTask, FaultRecord.UnobservedTask>]
    [MapDerivedType<FaultSource.Signalled, FaultRecord.Signalled>]
    [MapDerivedType<FaultSource.HostCrashMarker, FaultRecord.HostCrashMarker>]
    [MapDerivedType<FaultSource.MarkerDrifted, FaultRecord.MarkerDrifted>]
    public static partial FaultRecord From(FaultSource source);

    private static FaultObservationWire Rendered(Error evidence) =>
        AppHostFaultMap.Wire(evidence);
    private static string Rendered(PosixSignal signal) => signal.ToString();
}

// --- [OPERATIONS] -----------------------------------------------------------------------
public static class FaultSpine {
    const string MarkerFile = "boot-marker.json";

    extension(Lifecycle host) {
        public PhaseSubscription ArmTraps(Option<Action<SupportTrigger>> capture = default, Option<Action> reload = default) {
            UnhandledExceptionEventHandler unhandled = (_, args) =>
                Commit(host, capture, new FaultSource.Unhandled(
                    args.ExceptionObject as Exception is { } failure
                        ? Error.New(failure.Message, failure)
                        : new KernelFault.InvalidResult(Op.Of(), Some($"{args.ExceptionObject}")),
                    TerminationKind.Of(args.IsTerminating)));
            EventHandler<UnobservedTaskExceptionEventArgs> unobserved = (_, args) => {
                args.SetObserved();
                Commit(host, capture, new FaultSource.UnobservedTask(Error.New(args.Exception.Message, (Exception)args.Exception)));
            };
            AppDomain.CurrentDomain.UnhandledException += unhandled;
            TaskScheduler.UnobservedTaskException += unobserved;
            // ONE SIGTERM owner per process. Under systemd the host lifetime already registers it and routes to
            // `StopApplication`, which reaches this cell through the `ApplicationStopping` token.
            var sigterm = SystemdHelpers.IsSystemdService()
                ? Option<PosixSignalRegistration>.None
                : Some(PosixSignalRegistration.Create(PosixSignal.SIGTERM, context => Drainward(host, context)));
            var sigquit = PosixSignalRegistration.Create(PosixSignal.SIGQUIT, context => Drainward(host, context));
            // `Cancel = true` is the whole reason a reload signal is survivable: nothing else registers SIGHUP —
            // not the runtime, not `SystemdLifetime`, which arms SIGTERM alone — so an unregistered process takes
            // its inherited default disposition and dies at exit 129, which a service manager books as a CLEAN
            // exit. This arm ENQUEUES rather than folding a reload inline, because SIGHUP dispatches on the
            // ThreadPool where the other three get their own signal thread.
            var sighup = PosixSignalRegistration.Create(PosixSignal.SIGHUP, context => {
                context.Cancel = true;
                reload.Iter(static enqueue => enqueue());
            });
            return new PhaseSubscription(Seq<Action>(
                () => AppDomain.CurrentDomain.UnhandledException -= unhandled,
                () => TaskScheduler.UnobservedTaskException -= unobserved,
                sigquit.Dispose,
                sighup.Dispose) + sigterm.Map(static held => (Action)held.Dispose).ToSeq());
        }
    }

    public static IO<Unit> WriteMarker(BootMarker marker, string supportRoot, JsonTypeInfo<BootMarker> codec) =>
        IO.lift(fun(() => File.WriteAllText(Path.Join(supportRoot, MarkerFile), JsonSerializer.Serialize(marker, codec))));

    public static IO<Unit> ClearMarker(string supportRoot) =>
        IO.lift(fun(() => File.Delete(Path.Join(supportRoot, MarkerFile))));

    // PRESENCE is the crash fact; the parsed marker is its detail and a REFUSED parse is its own case, so the
    // reason a drifted marker could not be read survives instead of reading as an ordinary detail-free crash.
    public static IO<(Seq<FaultSource> Crashes, Option<PhaseTrigger> Upgrade)> ProbeMarkers(string supportRoot, Version current, JsonTypeInfo<BootMarker> codec, Seq<string> hostMarkers = default) =>
        from path in IO.pure(Path.Join(supportRoot, MarkerFile))
        from own in IO.lift(() => File.Exists(path)
            ? Op.Of(nameof(ProbeMarkers)).Catch(() => Fin.Succ(Optional(JsonSerializer.Deserialize(File.ReadAllText(path), codec))))
                .Match(Succ: marker => (Crash: (FaultSource)new FaultSource.HostCrashMarker(path, marker), Marker: marker),
                       Fail: cause => (Crash: new FaultSource.MarkerDrifted(path, cause), Marker: Option<BootMarker>.None))
                .Apply(Some)
            : Option<(FaultSource Crash, Option<BootMarker> Marker)>.None)
        from foreign in IO.lift(() => hostMarkers.Filter(File.Exists).Map(static found => (FaultSource)new FaultSource.HostCrashMarker(found)))
        select (own.Map(static probed => probed.Crash).ToSeq() + foreign,
                own.Bind(static probed => probed.Marker)
                   .Filter(marker => marker.AppVersion != current)
                   .Map(marker => (PhaseTrigger)new PhaseTrigger.UpgradeDetected(marker.AppVersion, current)));

    // One fault fact carries two consequences: the wire-stable record rides one `SupportTrigger.FaultTransition`
    // both the capture arm and the durable crash-recovery read, and the phase cell transitions on the same
    // source. Identity is the capsule's boot-minted one, never a caller argument.
    static Unit Commit(Lifecycle host, Option<Action<SupportTrigger>> capture, FaultSource source) =>
        (capture.Iter(arm => arm(new SupportTrigger.FaultTransition(host.CorrelationId, FaultRecordMap.From(source)))),
         ignore(host.Transition(new PhaseTrigger.FaultCommitted(source)))).Item2;

    static Unit Drainward(Lifecycle host, PosixSignalContext context) =>
        ((context.Cancel = true), ignore(host.Transition(RuntimePhase.Draining))).Item2;
}
```

## [04]-[DRAIN_CONDUCTOR]

- Owner: `DrainCapability` `[SmartEnum<string>]` realizing kernel `ICapability<DrainCapability>`; `DrainBand` `[SmartEnum<int>]` the frozen rank bands carrying a `CapabilitySet<DrainCapability>`; `DrainRow` the participant registration; `DrainStep` and `DrainReceipt` the receipts; `DrainConductor` the ordered fold.
- Cases: Interaction 100, Compute 200, Stores 300, Telemetry 400; capabilities store-write | egress.
- Entry: `IO<DrainReceipt> Drain(Seq<DrainRow> rows, ILatencyContext latency, CheckpointToken checkpoint, InstrumentSet instruments)` — `IO` carries the ordered flush effects and aborts on a rejected fence transition; the cooperative and forced budgets are `DeadlineClass` rows this fold reads, never parameters two call sites can disagree on.
- Auto: the conductor's first act is the draining fence, and interior admission dispatches on the phase cell, so inbound admission ceases before any band-100 row runs; every step receipt lands regardless of outcome; each step writes its own band-tagged duration observation and the fold records its own latency checkpoint at the boundary it owns.
- Receipt: `DrainReceipt` aggregates `DrainStep` rows — name, band, and the `DeadlineReceipt` whose lane names the ceiling actually in force — with final phase, `Instant`, elapsed, correlation id; `Stragglers` is the forced-outcome projection.
- Packages: Rasm (kernel `CapabilitySet`/`GaugedSpan`/`MonotonicTimeline`/`InstrumentSet`), Microsoft.Extensions.Telemetry.Abstractions, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one `DrainRow` per participant, one band row per package altitude, one `DrainCapability` row per capability the bands differ on; zero new surface.
- Boundary: the band's store-dependency bool becomes a `CapabilitySet<DrainCapability>` under a legal-corner law, because the axes are not independent — a band that may write but cannot export publishes a step receipt nothing can read, so `DrainLaw` bars that corner at construction and the Telemetry row's egress-only membership is a data fact rather than a second bool; `DrainOutcome` DELETES onto `DeadlineOutcome` — flushed, escalated, and straggled were met, escalated, and forced under other names, and `Runtime/time#DEADLINE_TAXONOMY` already owns the correspondence including the cooperative-escalates-to-forced arc, so a second three-valued vocabulary was the drift; NAMED GAIN — the receipt now says WHICH ceiling cut a step, because the gauged lane is `DrainCooperative` when the cooperative token tripped and `DrainForced` when the total ceiling cut it, where the prior receipt derived `Allotted` from its own outcome and read identical for a flushed step and a straggler; the fence is IDEMPOTENT on `Draining` and abortive everywhere else — a signal trap and the host stopping token both commit `Draining` before this fold runs and the step law refuses a second `DrainRequested` from `Draining`, so a bare railed transition here aborts the drain on exactly the paths that requested it while a `Boot` or `Unloaded` cell still aborts; the drain duration writes ONE OBSERVATION PER STEP under the band dimension the measure roster declares, so the percentile objective grades a real population rather than one summed point per band; the latency checkpoint records at the fold boundary through the injected token, so no `Stopwatch` appears anywhere below it; registration rows arrive field-identical from the drain-participant port; the maintenance-lease handoff emits as a Stores-band row, graceful handoff distinct from crash reclamation; the finalized Persistence single-`IDocumentSession` same-transaction spine mints no prepared transactions, so NO 2PC in-doubt drain row exists — a prepared-transaction reconciliation row or a managed XA transaction manager beside the spine is dead apparatus; on bundled-companion rows the parent's registration fans the drain signal to the child over the local-ipc hop.

```csharp signature
// --- [TYPES] ----------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DrainCapability : ICapability<DrainCapability> {
    public static readonly DrainCapability StoreWrite = new("store-write", rank: 0);
    public static readonly DrainCapability Egress = new("egress", rank: 1);
    public int Rank { get; }
}

// --- [TABLES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class DrainBand {
    public static readonly DrainBand Interaction = new(100, holds: CapabilitySet<DrainCapability>.All);
    public static readonly DrainBand Compute = new(200, holds: CapabilitySet<DrainCapability>.All);
    public static readonly DrainBand Stores = new(300, holds: CapabilitySet<DrainCapability>.All);
    public static readonly DrainBand Telemetry = new(400, holds: CapabilitySet<DrainCapability>.Of(DrainCapability.Egress));
    public CapabilitySet<DrainCapability> Holds { get; }
}

// --- [POLICIES] -------------------------------------------------------------------------
// A band that may write but cannot export publishes a step receipt nothing reads, and an empty band drains
// nothing — so the two legal corners are stated and both dead corners refuse at construction rather than being
// guarded at each site.
public static class DrainLaw {
    public static readonly CapabilityLaw<DrainCapability> Bands = new(Legal: Seq(
        CapabilitySet<DrainCapability>.Of(DrainCapability.Egress), CapabilitySet<DrainCapability>.All));
}

// --- [MODELS] ---------------------------------------------------------------------------
public readonly record struct DrainRow(string Name, DrainBand Band, int Rank, Func<CancellationToken, IO<Unit>> Drain);

public readonly record struct DrainStep(string Name, DrainBand Band, DeadlineReceipt Deadline) {
    public DeadlineOutcome Outcome => Deadline.Outcome;
    public Duration Allotted => Deadline.Allotted;
    public Duration Consumed => Deadline.Consumed;
}

public readonly record struct DrainReceipt(Seq<DrainStep> Steps, RuntimePhase Final, Instant At, Duration Elapsed, CorrelationId CorrelationId);

// --- [OPERATIONS] -----------------------------------------------------------------------
public static class DrainConductor {
    static readonly TimeSpan Ceiling = DeadlineClass.DrainCooperative.Bound + DeadlineClass.DrainForced.Bound;

    extension(Lifecycle host) {
        public IO<DrainReceipt> Drain(Seq<DrainRow> rows, ILatencyContext latency, CheckpointToken checkpoint, InstrumentSet instruments) =>
            from start in host.Railed(host.Clocks.Line.Capture(host.Key))
            from fence in host.Railed(Fence(host))
            from steps in toSeq(rows.OrderBy(static row => row.Band.Key).ThenBy(static row => row.Rank))
                .TraverseM(row => Step(row, host, instruments)).As()
            from marked in IO.lift(() => LatencySpine.Mark(latency, checkpoint))
            from finish in host.Railed(host.Clocks.Line.Capture(host.Key))
            from elapsed in host.Railed(host.Clocks.Line.Elapsed(start, finish, host.Key))
            let receipt = new DrainReceipt(steps.Strict(), RuntimePhase.Unloaded, host.Clocks.Now, Duration.FromTimeSpan(elapsed), host.CorrelationId)
            from closed in host.Railed(host.Transition(new PhaseTrigger.DrainCompleted(Some(receipt))))
            select receipt;
    }

    extension(DrainReceipt receipt) {
        public Seq<DrainStep> Stragglers => receipt.Steps.Filter(static step => step.Outcome == DeadlineOutcome.Forced);
    }

    // Fences ENFORCE, idempotent on the one phase that legitimately precedes this fold: a signal trap and the
    // host stopping token each commit `Draining` ahead, and the step law refuses a second request from there.
    static Fin<PhaseReceipt> Fence(Lifecycle host) =>
        host.Phase == RuntimePhase.Draining ? Fin.Succ(host.Latest) : host.Transition(RuntimePhase.Draining);

    // The LANE is the classification: both predicates read the package's own error identities rather than a
    // message match, so `Errors.Cancelled` (a cooperative token trip) leaves the cooperative bound in force
    // while `Errors.TimedOut` (the total ceiling) re-lanes the span onto the forced row, and the receipt names
    // the ceiling that actually cut the step instead of the one the caller hoped for.
    static IO<DrainStep> Step(DrainRow row, Lifecycle host, InstrumentSet instruments) =>
        from work in IO.pure(Op.Of(row.Name))
        from start in host.Railed(host.Clocks.Line.Capture(work))
        from lane in IO.lift(() => host.Spine.Derive(work, host.Clocks, Some(DeadlineClass.DrainCooperative))).Bracket(
            Use: scope => row.Drain(scope.Token)
                .Map(static _ => DeadlineClass.DrainCooperative)
                .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(DeadlineClass.DrainCooperative))
                .Timeout(Ceiling)
                .Catch(static error => error.Is(Errors.TimedOut) || error.Is(Errors.Cancelled), static _ => IO.pure(DeadlineClass.DrainForced)),
            Fin: static scope => IO.lift(fun(scope.Dispose)))
        from finish in host.Railed(host.Clocks.Line.Capture(work))
        from elapsed in host.Railed(host.Clocks.Line.Elapsed(start, finish, work))
        let step = new DrainStep(row.Name, row.Band,
            DeadlineReceipt.Of(new GaugedSpan<DeadlineClass>(Lane: lane, Work: work, Elapsed: elapsed, Bound: lane.Bound), host.Clocks.Now))
        // Each completed step contributes ONE observation tagged by its band: a distribution summed to one point
        // per band publishes a shape no percentile objective can read. Writes ride the typed rail, so a refused
        // measurement reaches this fold's error channel rather than vanishing beside a receipt claiming success.
        from written in host.Railed(instruments.Write(
            AppHostMeasure.DrainDuration, step.Consumed.TotalSeconds,
            InstrumentSet.Tags((AppHostSlot.Band, step.Band.Key.ToString(CultureInfo.InvariantCulture)))))
        select step;
}
```

## [05]-[CANCEL_SPINE]

- Owner: `CancelScope` — the one root source and every derived scope as provenance-carrying values.
- Entry: `CancelScope Derive(Op segment, ClockPolicy clocks, Option<DeadlineClass> bound = default)` — linked-token derivation whose expiry rides the injected provider and whose ceiling is a deadline row.
- Packages: Rasm (kernel `Op`), LanguageExt.Core, NodaTime, BCL inbox
- Growth: one derivation row per scope axis — phase, queue, hop attempt; zero new surface.
- Boundary: the root lives on the `Lifecycle` capsule and every scope below it derives through linked tokens — a free-floating `CancellationTokenSource` below the spine is the named defect; provenance is a SEQUENCE of `Op` segments rather than a concatenated path string, so a consumer reads the segment it cares about instead of splitting text and `Path` renders once at the boundary that surfaces it in `DrainStep` names and hop receipts; the bound is a `DeadlineClass` row, so a scope and the receipt gauging it read one owner's value and a bare `TimeSpan` never enters; the deadline source binds the policy's `TimeProvider` at construction so fake-clock specs drive expiry deterministically.

```csharp signature
// --- [MODELS] ---------------------------------------------------------------------------
public sealed record CancelScope(Seq<Op> Provenance, CancellationTokenSource Source, Option<CancellationTokenSource> Deadline = default) : IDisposable {
    public CancellationToken Token => Source.Token;
    public string Path => string.Join('/', Provenance.Map(static segment => segment.Value));

    public static CancelScope Root(Op provenance) => new([provenance], new CancellationTokenSource());

    public CancelScope Derive(Op segment, ClockPolicy clocks, Option<DeadlineClass> bound = default) =>
        bound.Match(
            Some: row => Timed(Provenance.Add(segment), Source.Token, new CancellationTokenSource(row.Bound, clocks.Time)),
            None: () => new CancelScope(Provenance.Add(segment), CancellationTokenSource.CreateLinkedTokenSource(Source.Token)));

    public void Dispose() => ignore((Deadline.Iter(static timed => timed.Dispose()), fun(Source.Dispose)()));

    static CancelScope Timed(Seq<Op> provenance, CancellationToken parent, CancellationTokenSource timed) =>
        new(provenance, CancellationTokenSource.CreateLinkedTokenSource(parent, timed.Token), Some(timed));
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
