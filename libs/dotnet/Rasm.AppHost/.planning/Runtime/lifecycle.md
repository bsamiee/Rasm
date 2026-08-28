# [APPHOST_LIFECYCLE_AND_DRAIN]

Rasm.AppHost runs one process lifecycle: eight `RuntimePhase` rows and one `PhaseStep` correspondence carrying every legal edge, one Atom-backed `Lifecycle` capsule committing through the kernel transition mechanism, a five-case `FaultSource` spine with crash-marker and upgrade boot probing, a rank-band drain conductor folding participant rows into the stopped phase fact, and one `CancelScope` spine beneath which every cancellation token derives. Owned axes are the phase family with its step correspondence, the fault traps, the boot log-event run, the frozen drain bands with their capability column, and cancellation provenance over Microsoft.Extensions.Hosting lifetime tokens, Thinktecture-generated vocabulary, generated protobuf contracts, LanguageExt result types, and NodaTime instants.

Settled composition: `CorrelationId` arrives from the kernel frame capsule `Rasm/Domain/frame#SOURCE` and `Observability/telemetry#CORRELATION_SPINE` `Correlation.Mint` performs the boot mint, so this capsule takes the minted value at construction and threads it through every phase and fault fact. `FaultBand` and its `Code(offset)` derivation arrive from `Rasm/Domain/results#FAULT_BAND`, the one registry in the branch, with `[FaultCase]`/`Fault`/`generated identity admission` the folder fault-catalog floor every AppHost family rides; `Transition<TState>` and `Cell` from `Rasm/Domain/results#TRANSITION`; `CapabilitySet`/`ICapability`/`CapabilityLaw` from `Rasm/Domain/validation#CAPABILITY`; `MonotonicTimeline`, `GaugedSpan`, and `IGaugeLane` from `Rasm/Parametric/projections#TIMELINE`. `HookSet<AppHostPoint, AppHostFact, TelemetrySource>` and the `AppHostPoint`/`AppHostFact` rosters arrive from `Observability/hooks#HOOK_ROSTER`, so every phase publish and every degradation read crosses one dispatcher rather than a per-capsule fan-out; `ClockPolicy`, `DeadlineClass`, and `DeadlineOutcome` from `Runtime/time#CLOCK_SPLIT` and `#DEADLINE_TAXONOMY`; `ILatencyContext`, `CheckpointToken`, and `LatencySpine.Mark` from `Observability/telemetry#SIGNAL_GOVERNANCE`; `AppHostMeasure`, `AppHostSlot`, and `InstrumentSet` from `Observability/instruments#INSTRUMENT_CATALOG`.

## [01]-[INDEX]

- [02]-[PHASE_FAMILY]: Eight phases, one step correspondence, kernel-committed transitions, boot log events.
- [03]-[FAULT_SPINE]: Five native fault sources, trap registrations, and crash-marker probing.
- [04]-[DRAIN_CONDUCTOR]: Frozen rank bands fold participant rows into the stopped phase fact.
- [05]-[CANCEL_SPINE]: One root source; derived scopes carry provenance segments and deadline rows.

## [02]-[PHASE_FAMILY]

- Owner: `RuntimePhase` `[SmartEnum<string>]` eight rows; `PhaseStep` `[SmartEnum<string>]` the ONE phase correspondence — every row carries its admitting phase set, its target, and its evidence-free mint, and `Next` plus phase-shaped admission project from it; `PhaseTrigger` `[Union]` the trigger vocabulary, each case naming its own step and `Stopped` carrying the terminal band facts; `PhaseCommit` the chronological lifecycle fact; `Lifecycle` the boundary capsule owning the Atom-backed commit cell, the injected dispatcher, and the boot-minted `CorrelationId`; `LifecycleFault` the fault family riding the kernel `[FaultCase]`/`Fault` floor; `LifecycleLog` the boot log-event run based at `FaultBand.SpineEventsBase`; `PhaseSubscription` the LIFO detacher composite.
- Cases: eight phases; ten `PhaseStep` rows, each the key one `PhaseTrigger` case carries; `LifecycleFault` = IllegalTransition | ModuleRejected | ActivationRejected.
- Entry: `Fin<PhaseCommit> Transition(PhaseTrigger trigger)` — `Cell.Step` commits the candidate and the refused verdict becomes the `IllegalTransition` fault; the `RuntimePhase`-shaped overload admits evidence-free targets from host-attach injection through `PhaseStep.Derived`, which resolves against the phase HELD rather than the target alone; `IO<T> Captured<T>(IO<T> body)` brackets a support capture, so the resume phase is read off the cell rather than supplied; `HookTap<…> DegradationTap` is the degradation-to-phase producer the composition seats on the bus.
- Auto: every settled commit writes the lifecycle transition metric and fires `AppHostPoint.Phase` once with the committed fact; `Attach` projects the host lifetime tokens into transitions; `DegradationTap` folds each `DegradationReading` into the degraded or recovered step, so the degradation ladder is the sole producer of both and no caller commits a level by hand.
- Packages: Microsoft.Extensions.Hosting, Microsoft.Extensions.Logging, Rasm (kernel `Cell`/`Transition`/`FaultBand`/`HookSet`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime
- Growth: one phase row plus the `PhaseStep` rows naming it, or one trigger case with its step row; both break every dispatch site at compile time and neither adds a member.
- Boundary: `PhaseStep` is the primary correspondence and everything else on this page derives from it — `From` is the admitting set, `To` the target, `Free` the evidence-free mint the phase-shaped overload needs, so a phase edge is stated ONCE and the three parallel total switches that restated it (next, derived) are gone with the hand-kept transition diagram that mirrored them a fourth time; NAMED LOSS — the rendered edge picture, recovered by the roster's own `From`/`To` columns, which ARE the edge list and cannot drift from the law they are. `Recovered` and `CaptureCompleted` were admitted by the old transition law with no producer anywhere, which made `Degraded` and `SupportCapture` absorbing states in practice; both now have one: the degradation ladder through `DegradationTap` and the capture bracket through `Captured`, whose finalizer reads the pre-capture phase off the cell so the resume target is state rather than a caller argument. `Lifecycle` is the named boundary capsule for the statement carve-out — the token registration and trap wiring carry language-owned statement forms while every other member stays expression-shaped; the candidate mints against the held commit inside `Cell.Step` and the ONE clock read is hoisted outside it, so a contended retry re-derives the commit from the state it actually lost to and never re-reads the clock; the fire is the SETTLED commit rather than the swap body, so a contended retry never publishes a commit the cell rejected, and the `Phase` row's Observe modality admits no veto — the only refusal `Fire` can answer is an unseated point, which is a composition defect and rides the typed result rather than an `ignore`; subscription is a `HookTap` row the composition hands `HookSet.Of`, so a per-capsule subscribe member and its detacher both delete and `hooks.Release(TelemetrySource.AppHost)` is the one teardown; evidence-bearing targets (faulted, capture-resume, upgrade) carry no `Free` mint, so the phase-shaped admission cannot reach them and fault evidence is never silently dropped; the boot self-loop row carries upgrade detection without leaving boot; `PhaseCommit.Trigger` is the step key, and the six boot events base at `FaultBand.SpineEventsBase` with the registry row `FaultBand.SpineEvents` holding the same value — the dual-owner invariant the kernel band law names, moving as one edit.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RuntimePhase {
    public static readonly RuntimePhase Boot = new("boot", wire: Rasm.Contracts.Compute.RuntimePhase.Boot);
    public static readonly RuntimePhase Ready = new("ready", wire: Rasm.Contracts.Compute.RuntimePhase.Ready);
    public static readonly RuntimePhase Running = new("running", wire: Rasm.Contracts.Compute.RuntimePhase.Running);
    public static readonly RuntimePhase Degraded = new("degraded", wire: Rasm.Contracts.Compute.RuntimePhase.Degraded);
    public static readonly RuntimePhase Draining = new("draining", wire: Rasm.Contracts.Compute.RuntimePhase.Draining);
    public static readonly RuntimePhase Unloaded = new("unloaded", wire: Rasm.Contracts.Compute.RuntimePhase.Unloaded);
    public static readonly RuntimePhase Faulted = new("faulted", wire: Rasm.Contracts.Compute.RuntimePhase.Faulted);
    public static readonly RuntimePhase SupportCapture = new(
        "support-capture", wire: Rasm.Contracts.Compute.RuntimePhase.SupportCapture);

    public Rasm.Contracts.Compute.RuntimePhase Wire { get; }
}

// --- [TABLES] --------------------------------------------------------------------------
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
    public static readonly PhaseStep Stopped = new("Stopped",
        from: Only(RuntimePhase.Draining), to: Some(RuntimePhase.Unloaded), free: Evidence);

    public FrozenSet<RuntimePhase> From { get; }
    public Option<RuntimePhase> To { get; }

    [UseDelegateFromConstructor]
    public partial Option<PhaseTrigger> Free();

    public Option<RuntimePhase> Next(RuntimePhase at, PhaseTrigger trigger) =>
        From.Contains(at) ? To.Match(Some: static row => Some(row), None: () => trigger.Resume) : None;

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
    public sealed record Stopped(Seq<BandFact> Bands) : PhaseTrigger { public override PhaseStep Step => PhaseStep.Stopped; }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct PhaseCommit(RuntimePhase From, RuntimePhase To, PhaseTrigger Trigger, Instant At, Duration Held, ConsumptionProfile Profile, CorrelationId CorrelationId) {
    public Fin<DomainEvent> Event(EventSource source, HlcStamp stamp) =>
        DomainEvent.Of(
            Topic.Lifecycle, EventType.Of(TelemetryDomain.AppHost.Key, "lifecycle", Trigger.Step.Key), source,
            $"{CorrelationId}:{Trigger.Step.Key}:{ClockPolicy.Persisted(At)}",
            JsonSerializer.SerializeToElement(this, SuiteContracts.Host), DataClassification.Operational, stamp);
}

public readonly record struct PhaseSubscription(Seq<Action> Detachers) : IDisposable {
    public void Dispose() => Detachers.Rev().Iter(static detach => detach());
}

// --- [ERRORS] --------------------------------------------------------------------------
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

// --- [SERVICES] ------------------------------------------------------------------------
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

public sealed class Lifecycle(ConsumptionProfile profile, ClockPolicy clocks, CorrelationId correlationId, HookSet<AppHostPoint, AppHostFact, TelemetrySource> hooks, InstrumentSet instruments) {
    readonly Atom<PhaseCommit> cell = Atom(new PhaseCommit(RuntimePhase.Boot, RuntimePhase.Boot, new PhaseTrigger.Validated(), clocks.Now, Duration.Zero, profile, correlationId));
    public ConsumptionProfile Profile { get; } = profile;
    public ClockPolicy Clocks { get; } = clocks;
    public CorrelationId CorrelationId { get; } = correlationId;
    public HookSet<AppHostPoint, AppHostFact, TelemetrySource> Hooks { get; } = hooks;
    public InstrumentSet Instruments { get; } = instruments;
    public CancelScope Spine { get; } = CancelScope.Root();
    public RuntimePhase Phase => cell.Value.To;
    public PhaseCommit Latest => cell.Value;

    public Fin<PhaseCommit> Transition(PhaseTrigger trigger) =>
        Settle(Cell.Step(
            cell: cell,
            step: Candidate(trigger: trigger, at: Clocks.Now),
            declined: new LifecycleFault.IllegalTransition(cell.Value.To, trigger.Step)));

    public Fin<PhaseCommit> Transition(RuntimePhase target) =>
        PhaseStep.Derived(at: cell.Value.To, target: target)
            .ToFin(new LifecycleFault.IllegalTransition(cell.Value.To, PhaseStep.DrainRequested))
            .Bind(Transition);

    public IO<T> Captured<T>(IO<T> body) =>
        from entered in IO.lift(Transition(new PhaseTrigger.CaptureStarted()))
        from held in body.Bracket(
            Use: static value => IO.pure(value),
            Fin: _ => IO.lift(() => ignore(Transition(new PhaseTrigger.CaptureCompleted(entered.From)))))
        select held;

    public HookTap<AppHostPoint, AppHostFact, TelemetrySource> DegradationTap =>
        new(Name: nameof(DegradationTap),
            Observe: fact => fact.Switch(
                phase: static _ => Fin.Succ(unit),
                command: static _ => Fin.Succ(unit),
                outcome: static _ => Fin.Succ(unit),
                delivery: static _ => Fin.Succ(unit),
                degradation: row => ignore(Transition(row.Reading.Level == DegradationLevel.Full
                    ? new PhaseTrigger.Recovered()
                    : (PhaseTrigger)new PhaseTrigger.Degraded())) is var _ ? Fin.Succ(unit) : Fin.Succ(unit),
                alert: static _ => Fin.Succ(unit),
                binding: static _ => Fin.Succ(unit),
                profile: static _ => Fin.Succ(unit),
                coordination: static _ => Fin.Succ(unit),
                companion: static _ => Fin.Succ(unit)),
            Scope: Some(Seq(AppHostPoint.Degradation)),
            Owner: Some(TelemetrySource.AppHost));

    public PhaseSubscription Attach(IHostApplicationLifetime lifetime) {
        var started = lifetime.ApplicationStarted.Register(() => ignore(Transition(RuntimePhase.Running)));
        var stopping = lifetime.ApplicationStopping.Register(() => ignore(Transition(RuntimePhase.Draining)));
        var stopped = lifetime.ApplicationStopped.Register(() => ignore(Transition(new PhaseTrigger.Stopped([]))));
        return new PhaseSubscription([started.Dispose, stopping.Dispose, stopped.Dispose]);
    }

    Func<PhaseCommit, Option<PhaseCommit>> Candidate(PhaseTrigger trigger, Instant at) =>
        held => trigger.Step.Next(at: held.To, trigger: trigger)
            .Map(next => new PhaseCommit(held.To, next, trigger, at, at - held.At, Profile, CorrelationId));

    Fin<PhaseCommit> Settle(Transition<PhaseCommit> verdict) =>
        verdict switch {
            Transition<PhaseCommit>.Committed committed =>
                Instruments.Write(
                    AppHostMeasure.LifecycleTransitions, 1d,
                    InstrumentSet.Tags(
                        (AppHostSlot.From, committed.State.From.Key),
                        (AppHostSlot.To, committed.State.To.Key),
                        (AppHostSlot.Trigger, committed.State.Trigger.Step.Key)))
                    .Bind(_ => Hooks.Fire(at: AppHostPoint.Phase, fact: new AppHostFact.Phase(Commit: committed.State)))
                    .Map(_ => committed.State),
            Transition<PhaseCommit>.Refused refused => Fin.Fail<PhaseCommit>(refused.Cause),
        };
}
```

## [03]-[FAULT_SPINE]

- Owner: `FaultSource` `[Union]` is the five-case native fault fact; `BootMarker` is the crash and upgrade marker record; `FaultSpine` is the trap and probe surface.
- Cases: Unhandled, UnobservedTask, Signalled, HostCrashMarker, MarkerDrifted; `TerminationKind` = terminating | observed.
- Entry: `PhaseSubscription ArmTraps(Option<Action<SupportTrigger>> capture = default, Option<Action> reload = default)` — one LIFO detacher composite over every trap registration; the capture arm receives the same `FaultSource` inside `SupportTrigger.FaultTransition` that the phase transition receives, so a fault commit and its support capture are one native fact under the capsule's boot identity and `ProbeMarkers` boot evidence rides the identical case.
- Auto: every in-process fault commit emits its `FaultSource` to the capture arm before the `PhaseTrigger.FaultCommitted` transition, so the capture trigger and the phase commit derive from one `Commit` fold; SIGTERM and SIGQUIT project to the drain transition; SIGHUP ENQUEUES onto the reload delegate rather than folding inline, because the runtime dispatches SIGHUP on the ThreadPool while SIGINT, SIGQUIT, and SIGTERM get a dedicated signal thread — a saturated pool turns an inline reload into a missed service-manager reload deadline with the prior values still live.
- Growth: one trap registration row inside `ArmTraps` or one host-marker path value; a new fault cause extends `FaultSource` and its total `Kind` switch.
- Boundary: `TerminationKind` replaces the `bool Terminating` column on the source. Every `Error` enters the kernel's bounded `FaultWire.Observe` at the source mint, so the native fact carries durable evidence without retaining an exception graph or inventing a second support wire. `MarkerDrifted` preserves an unreadable marker's exact cause rather than erasing it into an absent-detail crash. `FaultSpine` owns trap wiring and signal handlers; stale own markers and host markers ride the same native `SupportTrigger.FaultTransition`; marker write, clean-drain removal, upgrade detection, and signal ownership remain one lifecycle owner.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TerminationKind {
    public static readonly TerminationKind Terminating = new("terminating");
    public static readonly TerminationKind Observed = new("observed");

    public static TerminationKind Of(bool terminating) => terminating ? Terminating : Observed;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(Unhandled), "unhandled")]
[JsonDerivedType(typeof(UnobservedTask), "unobserved-task")]
[JsonDerivedType(typeof(Signalled), "signalled")]
[JsonDerivedType(typeof(HostCrashMarker), "host-crash-marker")]
[JsonDerivedType(typeof(MarkerDrifted), "marker-drifted")]
public abstract partial record FaultSource {
    private FaultSource() { }
    public sealed record Unhandled(Rasm.Contracts.Fault.FaultObservation Evidence, TerminationKind Termination) : FaultSource;
    public sealed record UnobservedTask(Rasm.Contracts.Fault.FaultObservation Evidence) : FaultSource;
    public sealed record Signalled([property: JsonConverter(typeof(JsonStringEnumConverter<PosixSignal>))] PosixSignal Signal) : FaultSource;
    public sealed record HostCrashMarker(string Path, Option<BootMarker> Marker = default) : FaultSource;
    public sealed record MarkerDrifted(string Path, Rasm.Contracts.Fault.FaultObservation Cause) : FaultSource;

    [JsonIgnore]
    public string Kind => Switch(
        unhandled: static _ => "unhandled",
        unobservedTask: static _ => "unobserved-task",
        signalled: static _ => "signalled",
        hostCrashMarker: static _ => "host-crash-marker",
        markerDrifted: static _ => "marker-drifted");
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BootMarker(int Pid, RuntimePhase Phase, Version AppVersion, Instant StartedAt);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FaultSpine {
    const string MarkerFile = "boot-marker.json";

    extension(Lifecycle host) {
        public PhaseSubscription ArmTraps(Option<Action<SupportTrigger>> capture = default, Option<Action> reload = default) {
            UnhandledExceptionEventHandler unhandled = (_, args) =>
                Commit(host, capture, new FaultSource.Unhandled(
                    FaultWire.Observe(args.ExceptionObject as Exception is { } failure
                        ? Error.New(failure.Message, failure)
                        : new KernelFault.InvalidResult(Some($"{args.ExceptionObject}"))),
                    TerminationKind.Of(args.IsTerminating)));
            EventHandler<UnobservedTaskExceptionEventArgs> unobserved = (_, args) => {
                args.SetObserved();
                Commit(host, capture, new FaultSource.UnobservedTask(
                    FaultWire.Observe(Error.New(args.Exception.Message, (Exception)args.Exception))));
            };
            AppDomain.CurrentDomain.UnhandledException += unhandled;
            TaskScheduler.UnobservedTaskException += unobserved;
            var sigterm = SystemdHelpers.IsSystemdService()
                ? Option<PosixSignalRegistration>.None
                : Some(PosixSignalRegistration.Create(PosixSignal.SIGTERM, context => Drainward(host, context)));
            var sigquit = PosixSignalRegistration.Create(PosixSignal.SIGQUIT, context => Drainward(host, context));
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

    public static IO<(Seq<FaultSource> Crashes, Option<PhaseTrigger> Upgrade)> ProbeMarkers(string supportRoot, Version current, JsonTypeInfo<BootMarker> codec, Seq<string> hostMarkers = default) =>
        from path in IO.pure(Path.Join(supportRoot, MarkerFile))
        from own in IO.lift(() => File.Exists(path)
            ? Some(Try.lift(() => Fin.Succ(Optional(JsonSerializer.Deserialize(File.ReadAllText(path), codec)))).Run().Bind(static inner => inner)
                .Match(Succ: marker => (Crash: (FaultSource)new FaultSource.HostCrashMarker(path, marker), Marker: marker),
                       Fail: cause => (Crash: new FaultSource.MarkerDrifted(path, FaultWire.Observe(cause)), Marker: Option<BootMarker>.None)))
            : Option<(FaultSource Crash, Option<BootMarker> Marker)>.None)
        from foreign in IO.lift(() => hostMarkers.Filter(File.Exists).Map(static found => (FaultSource)new FaultSource.HostCrashMarker(found)))
        select (own.Map(static probed => probed.Crash).ToSeq() + foreign,
                own.Bind(static probed => probed.Marker)
                   .Filter(marker => marker.AppVersion != current)
                   .Map(marker => (PhaseTrigger)new PhaseTrigger.UpgradeDetected(marker.AppVersion, current)));

    static Unit Commit(Lifecycle host, Option<Action<SupportTrigger>> capture, FaultSource source) =>
        (capture.Iter(arm => arm(new SupportTrigger.FaultTransition(host.CorrelationId, source))),
         ignore(host.Transition(new PhaseTrigger.FaultCommitted(source)))).Item2;

    static Unit Drainward(Lifecycle host, PosixSignalContext context) =>
        ((context.Cancel = true), ignore(host.Transition(RuntimePhase.Draining))).Item2;
}
```

## [04]-[DRAIN_CONDUCTOR]

- Owner: `DrainCapability` `[SmartEnum<string>]` realizing kernel `ICapability<DrainCapability>`; `DrainBand` `[SmartEnum<int>]` the frozen rank bands carrying a `CapabilitySet<DrainCapability>`; `DrainRow` the participant registration; `BandFact` the participant crossing carried by `PhaseTrigger.Stopped`; `DrainConductor` the ordered fold.
- Cases: Interaction 100, Compute 200, Stores 300, Telemetry 400; capabilities store-write | egress.
- Entry: `IO<PhaseCommit> Drain(Seq<DrainRow> rows, ILatencyContext latency, CheckpointToken checkpoint, InstrumentSet instruments, Duration inherited)` — `IO` carries the ordered flush effects and aborts on a rejected fence transition; the conductor intersects the admitted caller remainder with `DeadlineClass.DrainCooperative` once, and every participant reads that one result.
- Auto: the conductor's first act is the draining fence, and interior admission dispatches on the phase cell, so inbound admission ceases before any band-100 row runs; every participant returns one `BandFact`; each participant writes its own band-tagged duration observation and the fold records its own latency checkpoint at the boundary it owns.
- Packages: Rasm (kernel `CapabilitySet`/`GaugedSpan`/`MonotonicTimeline`/`InstrumentSet`), Microsoft.Extensions.Telemetry.Abstractions, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: one `DrainRow` per participant, one band row per package altitude, one `DrainCapability` row per capability the bands differ on; zero new surface.
- Boundary: the band's store-dependency bool becomes a `CapabilitySet<DrainCapability>` under a legal-corner law, because the axes are not independent — a band that may write but cannot export publishes an unreadable step value, so `DrainLaw` bars that corner at construction and the Telemetry row's egress-only membership is a data fact rather than a second bool; `DrainOutcome` DELETES onto `DeadlineOutcome` — flushed, escalated, and straggled were met, escalated, and forced under other names, and `Runtime/time#DEADLINE_TAXONOMY` already owns the correspondence including the cooperative-escalates-to-forced arc, so a second three-valued vocabulary was the drift; the caller's strictly positive `DrainRuntimeRequest.cooperative` is an INHERITED REMAINDER, not permission to reset the clock — the conductor computes `min(inherited, local)` once, the cancellation source expires on that result, the total timeout adds only the local forced tail, and the stopped fact records the same effective bound, so a five-second parent remainder can never become a fresh twenty-second cooperative window; NAMED GAIN — the band fact says WHICH ceiling cut a step, because the gauged lane is `DrainCooperative` when the cooperative token tripped and `DrainForced` when the total ceiling cut it; the fence is IDEMPOTENT on `Draining` and abortive everywhere else — a signal trap and the host stopping token both commit `Draining` before this fold runs and the step law refuses a second `DrainRequested` from `Draining`, so a bare result-typed transition here aborts the drain on exactly the paths that requested it while a `Boot` or `Unloaded` cell still aborts; the drain duration writes ONE OBSERVATION PER STEP under the band dimension the measure roster declares, so the percentile objective grades a real population rather than one summed point per band; the latency checkpoint records at the fold boundary through the injected token, so no `Stopwatch` appears anywhere below it; registration rows arrive field-identical from the drain-participant port; the maintenance-lease handoff emits as a Stores-band row, graceful handoff distinct from crash reclamation; the finalized Persistence single-`IDocumentSession` same-transaction spine mints no prepared transactions, so NO 2PC in-doubt drain row exists — a prepared-transaction reconciliation row or a managed XA transaction manager beside the spine is dead apparatus; on bundled-companion rows the parent's registration fans the drain signal to the child over the local-ipc hop.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DrainCapability : ICapability<DrainCapability> {
    public static readonly DrainCapability StoreWrite = new("store-write", rank: 0);
    public static readonly DrainCapability Egress = new("egress", rank: 1);
    public int Rank { get; }
}

// --- [TABLES] --------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class DrainBand {
    public static readonly DrainBand Interaction = new(100, holds: CapabilitySet<DrainCapability>.All);
    public static readonly DrainBand Compute = new(200, holds: CapabilitySet<DrainCapability>.All);
    public static readonly DrainBand Stores = new(300, holds: CapabilitySet<DrainCapability>.All);
    public static readonly DrainBand Telemetry = new(400, holds: CapabilitySet<DrainCapability>.Of(DrainCapability.Egress));
    public CapabilitySet<DrainCapability> Holds { get; }
}

// --- [POLICIES] ------------------------------------------------------------------------
public static class DrainLaw {
    public static readonly CapabilityLaw<DrainCapability> Bands = new(Legal: Seq(
        CapabilitySet<DrainCapability>.Of(DrainCapability.Egress), CapabilitySet<DrainCapability>.All));
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct DrainRow(string Name, DrainBand Band, int Rank, Func<CancellationToken, IO<Unit>> Drain);

public readonly record struct BandFact(string Name, DrainBand Band, GaugedSpan<DeadlineClass> Span) {
    public DeadlineOutcome Outcome => DeadlineOutcome.Of(Span);
    public Duration Consumed => Duration.FromTimeSpan(Span.Elapsed);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DrainConductor {
    extension(Lifecycle host) {
        public IO<PhaseCommit> Drain(Seq<DrainRow> rows, ILatencyContext latency, CheckpointToken checkpoint, InstrumentSet instruments, Duration inherited) =>
            from fence in IO.lift(Fence(host))
            let cooperative = inherited < DeadlineClass.DrainCooperative.Allotted
                ? inherited
                : DeadlineClass.DrainCooperative.Allotted
            from bands in toSeq(rows.OrderBy(static row => row.Band.Key).ThenBy(static row => row.Rank))
                .TraverseM(row => Step(row, host, instruments, cooperative)).As()
            from marked in IO.lift(() => LatencySpine.Mark(latency, checkpoint))
            from closed in IO.lift(host.Transition(new PhaseTrigger.Stopped(bands.Strict())))
            select closed;
    }

    static Fin<PhaseCommit> Fence(Lifecycle host) =>
        host.Phase == RuntimePhase.Draining ? Fin.Succ(host.Latest) : host.Transition(RuntimePhase.Draining);

    static IO<BandFact> Step(DrainRow row, Lifecycle host, InstrumentSet instruments, Duration cooperative) =>
        from work in IO.pure()
        from start in IO.lift(Error.New(work.Message, work))
        from lane in IO.lift(() => host.Spine.Derive(work, host.Clocks, DeadlineClass.DrainCooperative, cooperative)).Bracket(
            Use: scope => row.Drain(scope.Token)
                .Map(static _ => DeadlineClass.DrainCooperative)
                .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(DeadlineClass.DrainCooperative))
                .Timeout(cooperative.ToTimeSpan() + DeadlineClass.DrainForced.Bound)
                .Catch(static error => error.Is(Errors.TimedOut) || error.Is(Errors.Cancelled), static _ => IO.pure(DeadlineClass.DrainForced)),
            Fin: static scope => IO.lift(fun(scope.Dispose)))
        from finish in IO.lift(Error.New(work.Message, work))
        from elapsed in IO.lift(host.Clocks.Line.Elapsed(start, finish, work))
        let bound = lane == DeadlineClass.DrainCooperative
            ? cooperative.ToTimeSpan()
            : cooperative.ToTimeSpan() + DeadlineClass.DrainForced.Bound
        let fact = new BandFact(row.Name, row.Band,
            new GaugedSpan<DeadlineClass>(Lane: lane, Work: work, Elapsed: elapsed, Bound: bound))
        from written in IO.lift(instruments.Write(
            AppHostMeasure.DrainDuration, fact.Consumed.TotalSeconds,
            InstrumentSet.Tags((AppHostSlot.Band, fact.Band.Key.ToString(CultureInfo.InvariantCulture)))))
        select fact;
}
```

## [05]-[CANCEL_SPINE]

- Owner: `CancelScope` — the one root source and every derived scope as provenance-carrying values; `CancelDeadline` carries the lane and effective allotment beside its timer.
- Entry: `CancelScope Derive(ClockPolicy clocks, Option<DeadlineClass> bound = default)` derives a local row allotment; `Derive(ClockPolicy, DeadlineClass, Duration)` preserves a caller-inherited effective allotment without minting a second deadline owner.
- Packages: LanguageExt.Core, NodaTime, BCL inbox
- Growth: one derivation row per scope axis — phase, queue, hop attempt; zero new surface.
- Boundary: the root lives on the `Lifecycle` capsule and every scope below it derives through linked tokens — a free-floating `CancellationTokenSource` below the spine is the named defect; provenance is a SEQUENCE of segments rather than a concatenated path string, so a consumer reads the segment it cares about instead of splitting text and `Path` renders once at the boundary that surfaces it in `BandFact.Name`; `CancelDeadline` always retains the owning `DeadlineClass` even when its effective allotment is a shorter inherited remainder, and the deadline source binds the policy's `TimeProvider` at construction so fake-clock specs drive expiry deterministically.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record CancelDeadline(DeadlineClass Lane, Duration Allotted, CancellationTokenSource Source);

public sealed record CancelScope(Seq<> Provenance, CancellationTokenSource Source, Option<CancelDeadline> Deadline = default) : IDisposable {
    public CancellationToken Token => Source.Token;
    public string Path => string.Join('/', Provenance.Map(static segment => segment.Value));

    public static CancelScope Root() => new([provenance], new CancellationTokenSource());

    public CancelScope Derive(ClockPolicy clocks, Option<DeadlineClass> bound = default) =>
        bound.Match(
            Some: row => Timed(Provenance.Add(segment), Source.Token,
                new CancelDeadline(row, row.Allotted, new CancellationTokenSource(row.Bound, clocks.Time))),
            None: () => new CancelScope(Provenance.Add(segment), CancellationTokenSource.CreateLinkedTokenSource(Source.Token)));

    public CancelScope Derive(ClockPolicy clocks, DeadlineClass lane, Duration allotted) =>
        Timed(Provenance.Add(segment), Source.Token,
            new CancelDeadline(lane, allotted, new CancellationTokenSource(allotted.ToTimeSpan(), clocks.Time)));

    public void Dispose() => ignore((Deadline.Iter(static deadline => deadline.Source.Dispose()), fun(Source.Dispose)()));

    static CancelScope Timed(Seq<> provenance, CancellationToken parent, CancelDeadline deadline) =>
        new(provenance, CancellationTokenSource.CreateLinkedTokenSource(parent, deadline.Source.Token), Some(deadline));
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
