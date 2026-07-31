# [RASM_GRASSHOPPER_DOCUMENT_SOLUTION]

`SolutionControl` is the execution controller of the GH2 document boundary — ONE solution owner over the host's `SolutionServer`: launching a run in every posture the server admits (fire, bridled by a cancellation source, or awaited to completion on the caller's own thread), halting, cancelling one in-flight `Solution`, queueing deferred expiry, expiring explicit object sets, the run-inspection pulse over a live `Solution`, the completion evidence over a `SolutionRecord`, and the phase-timeline fold over the six-event solution family observed through `Shell/events.md`'s rows.

`Watch` attaches the whole lifecycle family (`SolutionAboutToStart` → `Started` → `Stopped` → `Cancelled` → `Completed` → `Faulted`) as one leased subscription, and `Trace` folds what a watcher captured into ordered phase evidence, so a consumer correlates a mutation with the run it triggered. Execution control is command-shaped (one `SolutionCommand` union, one `Drive` gate settling into the folder's `GateReceipt`), inspection is evidence-shaped (typed receipts, never live-object retention), and cancellation is the host's own `CancellationTokenSource` bridle carried as a case payload.

## [01]-[INDEX]

- [02]-[CONTROL]: `SolutionCommand` + `SolutionControl.Drive`/`Watch` — the execution command union, the one settlement gate with its per-case marshal lane, and the leased lifecycle subscription.
- [03]-[EVIDENCE]: `RunPulse` + `RunEvidence` + `SolutionTrace` — in-flight inspection, completion audit, and the phase-timeline fold.

## [02]-[CONTROL]

- Owner: `SolutionCommand` `[Union]` `[GenerateUnionOps]` — the closed execution vocabulary. `LaunchCase(SolutionMode, Option<CancellationTokenSource>)` discriminates the two start shapes on payload presence — a bare mode rides `SolutionServer.Start(SolutionMode)`, a bridled launch rides `Start(CancellationTokenSource, SolutionMode)` — and returns the moment the run is dispatched; `AwaitCase(SolutionMode, CancellationTokenSource)` drives the same `Start` and blocks the caller's own thread on the `Task<Solution>` it hands back; `HaltCase` stops the server; `CancelCase(Solution)` cancels one in-flight run cooperatively through `Solution.Cancel`; `DeferCase(IDocumentObject)` queues deferred expiry through `SolutionServer.DelayedExpire`; `ExpireCase(Seq<IDocumentObject>)` expires an explicit object set through each subject's own `IDocumentObject.Expire`. Settlement evidence is `Document/document.md`'s `GateReceipt`, the awaited run answering `GateOutcome.RunCase` with its settled pulse.
- Entry: `SolutionControl.Drive(SolutionCommand op, Option<HostDocument> graph = default, Op? key = null)` → `Fin<GateReceipt>` — the one execution gate; `SolutionControl.Watch(Action<UiEvent> publish, Option<HostDocument> graph = default, Op? key = null)` → `Fin<Lease<UiSubscription>>` — the whole six-row lifecycle family attached transactionally through `UiEvents.Observe` on the document's `SolutionServer` anchor, the subscription's lifetime the kernel lease.
- Law: `AwaitCase` BYPASSES the marshal, and that bypass is what makes the blocking posture satisfiable. `SolutionServer.Start` runs the whole solve on a threadpool worker and hands back its `Task<Solution>`, so the run settles independently of the UI idle loop and the only thread that blocks is the caller's own. Routing the await through the marshal like every other case posts the block ONTO the idle loop the run does not need but every other gate does, which is the starvation the marshal law names; the arm therefore probes `EtoDispatch.OnMarshal` and refuses with `Fault.InvalidContext` when the caller already holds the UI thread. The host's own `StartWait` is that same deadlock as a member and never enters the gate.
- Law: every other case keeps the marshal — the server handle resolves once through `DocumentScope.Resolve` and each remaining arm runs its host verb inside `EtoDispatch.Run`, which short-circuits on-thread, so the lane is a per-case property rather than one wrapper the blocking posture cannot inhabit.
- Law: expiry is a two-verb protocol on one owner — `DeferCase` queues and `ExpireCase` bypasses the queue for an explicit set — because the host drains its own deferred queue internally at run start and publishes no drain member; document-wide expiry (`ObjectList.ExpireAll`) is `Document/graph.md`'s membership verb, and a third expiry spelling anywhere in the folder is the deleted form.
- Law: cancellation is the bridle, never a flag — a launch that must be cancellable carries its `CancellationTokenSource` as the case payload, `CancelCase` targets one run's own cooperative gate, and a cancelled host call surfaces as the kernel's `Fault.Cancelled` through `Op.Catch`, never as a result failure.
- Boundary: the six lifecycle rows, their signal vocabulary, and the anchor admission are `Shell/events.md`'s algebra — `Watch` composes `UiSource.SolutionAboutToStart`/`SolutionStarted`/`SolutionStopped`/`SolutionCancelled`/`SolutionCompleted`/`SolutionFaulted` and adds no row of its own; per-component solution hooks (`BeforeProcess`/`PreProcess`/`PostProcess`) are `Components/component.md`'s lifecycle.
- Packages: Grasshopper2 (`SolutionServer.Start`/`Stop`/`DelayedExpire`, `Solution.Cancel`, `SolutionMode`, `IDocumentObject.Expire`), LanguageExt.Core, `Rasm.Domain`, `Rasm.Parametric` (`MonotonicTimeline`, `MonotonicStamp`), `Document/document.md` (`GateReceipt`, `GateOutcome`), `Eto/runtime.md` (`EtoDispatch`), `Shell/events.md` (`UiEvents`, `UiSource`, `EventAnchor`, `UiEvent`, `UiSubscription`).
- Growth: a new execution posture is one `SolutionCommand` case breaking the gate's total `Switch` loudly; a new lifecycle stream on `Watch` is one composed `UiSource` row — the gate pair never widens.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Grasshopper2.Doc;
using Rasm.Csp;
using Rasm.Grasshopper.Eto;
using Rasm.Grasshopper.Shell;
using Rasm.Parametric;
using HostDocument = Grasshopper2.Doc.Document;

namespace Rasm.Grasshopper.Document;

// --- [TYPES] --------------------------------------------------------------------------------
[Union]
[GenerateUnionOps]
public abstract partial record SolutionCommand {
    private SolutionCommand() { }
    public sealed record LaunchCase(SolutionMode Mode, Option<CancellationTokenSource> Bridle) : SolutionCommand;
    public sealed record AwaitCase(SolutionMode Mode, CancellationTokenSource Bridle) : SolutionCommand;
    public sealed record HaltCase : SolutionCommand;
    public sealed record CancelCase(Solution Run) : SolutionCommand;
    public sealed record DeferCase(IDocumentObject Subject) : SolutionCommand;
    public sealed record ExpireCase(Seq<IDocumentObject> Subjects) : SolutionCommand;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
[BoundaryAdapter]
public static partial class SolutionControl {
    public static Fin<GateReceipt> Drive(SolutionCommand op, Option<HostDocument> graph = default, Op? key = null) {
        Op active = key.OrDefault();
        return from valid in Optional(op).ToFin(active.InvalidInput())
               from timeline in MonotonicTimeline.Of(provider: TimeProvider.System, key: active)
               from entered in timeline.Capture(key: active)
               from server in DocumentScope.Resolve(graph: graph, key: active,
                   body: document => active.Catch(body: () => Fin.Succ(document.Solution)))
               from answer in valid.Switch(
                state: (Key: active, Server: server),
                launchCase: static (frame, c) => Marshalled(frame.Key, nameof(SolutionCommand.LaunchCase), () =>
                    (Op.Side(action: () => c.Bridle.Match(
                        Some: bridle => frame.Server.Start(bridle, c.Mode),
                        None: () => frame.Server.Start(c.Mode))), (GateOutcome)new GateOutcome.SettledCase()).Item2),
                awaitCase: static (frame, c) => Awaited(key: frame.Key, server: frame.Server, command: c),
                haltCase: static (frame, _) => Marshalled(frame.Key, nameof(SolutionCommand.HaltCase), () =>
                    (Op.Side(action: frame.Server.Stop), (GateOutcome)new GateOutcome.SettledCase()).Item2),
                cancelCase: static (frame, c) => Marshalled(frame.Key, nameof(SolutionCommand.CancelCase), () =>
                    (Op.Side(action: c.Run.Cancel), (GateOutcome)new GateOutcome.SettledCase()).Item2),
                deferCase: static (frame, c) => Marshalled(frame.Key, nameof(SolutionCommand.DeferCase), () =>
                    (Op.Side(action: () => frame.Server.DelayedExpire(c.Subject)), (GateOutcome)new GateOutcome.SettledCase()).Item2),
                expireCase: static (frame, c) => Marshalled(frame.Key, nameof(SolutionCommand.ExpireCase), () =>
                    new GateOutcome.CountCase(Touched: c.Subjects.Fold(
                        0, static (count, subject) => (Op.Side(action: () => subject.Expire()), count + 1).Item2))))
               from settled in timeline.Capture(key: active)
               from latency in timeline.Elapsed(start: entered, end: settled, key: active)
               select new GateReceipt(
                   Operation: active, Verb: answer.Verb, Seal: Option<VerbNoun>.None, Outcome: answer.Outcome,
                   Entered: entered, Settled: settled, Latency: latency);
    }

    private static Fin<(string Verb, GateOutcome Outcome)> Marshalled(Op key, string verb, Func<GateOutcome> settle) =>
        EtoDispatch.Run(body: () => key.Catch(body: () => Fin.Succ((Verb: verb, Outcome: settle()))), key: key);

    private static Fin<(string Verb, GateOutcome Outcome)> Awaited(Op key, SolutionServer server, SolutionCommand.AwaitCase command) =>
        from onMarshal in EtoDispatch.OnMarshal(key: key)
        from _ in guard(!onMarshal, key.InvalidContext()).ToFin()
        from run in key.Catch(body: () => Fin.Succ(server.Start(command.Bridle, command.Mode).GetAwaiter().GetResult()))
        select (Verb: nameof(SolutionCommand.AwaitCase), Outcome: (GateOutcome)new GateOutcome.RunCase(Pulse: RunPulse.Of(run: run)));

    public static Fin<Lease<UiSubscription>> Watch(Action<UiEvent> publish, Option<HostDocument> graph = default, Op? key = null) {
        Op active = key.OrDefault();
        return Optional(publish).ToFin(active.InvalidInput())
            .Bind(valid => DocumentScope.Resolve(graph: graph, key: active, body: document =>
                UiEvents.Observe(
                    anchor: new EventAnchor.SolutionCase(Server: document.Solution),
                    publish: valid,
                    key: active,
                    rows: [
                        UiSource.SolutionAboutToStart, UiSource.SolutionStarted, UiSource.SolutionStopped,
                        UiSource.SolutionCancelled, UiSource.SolutionCompleted, UiSource.SolutionFaulted,
                    ])));
    }
}
```

## [03]-[EVIDENCE]

- Owner: `RunPulse` — the in-flight inspection receipt over one live `Solution`: the typed `SolutionId`, the `SolutionPhase` the run holds at the read, the `SolutionMode` it launched under, its computable and invalid-parameter counts, its overall progress, and its age, every field detached at read time so a stale pulse can never hand out run internals. `RunEvidence` — the completion audit over one `SolutionRecord`: the run id, the `SolutionPhase` it culminated in, and the start/end window with its derived duration. `SolutionTrace` — the phase-timeline fold over a watcher's captured `UiEvent` sequence: each solution fact projects to its signal row and stamp, and validity claims the stamps are monotone, so a trace that interleaved two runs' events fails its own evidence.
- Entry: `SolutionControl.Probe(Solution run, Op? key = null)` → `Fin<RunPulse>`; `SolutionControl.Audit(SolutionRecord record, Op? key = null)` → `Fin<RunEvidence>`; `SolutionControl.Trace(Seq<UiEvent> observed)` → `SolutionTrace` — a pure fold, no marshal, because the events are already detached evidence.
- Law: `RunPulse.Of` is the pure projection and every reader composes it — `Probe` marshals it over a live run and the awaited drive folds it into its own receipt, so the in-flight snapshot has one spelling regardless of which gate asks.
- Law: the audit publishes only what the host measures — `SolutionRecord`'s `ExpiredCount`, `SolvedCount`, and `Progress` are auto-properties its one constructor never assigns, so every completed record reads them as a structural zero no run produced; carrying them fabricates a measurement, and the per-object counts a consumer wants ride the `Watch` stream's own object rows instead.
- Law: inspection detaches — a receipt never retains the `Solution` or `SolutionRecord` it read; correlation across receipts rides the typed `SolutionId`, so evidence outlives the run without pinning host state.
- Law: the trace consumes only solution facts — the fold keeps `UiFact.SolutionCase` rows and drops every other fact a shared watcher may have captured, so one `Watch` callback can feed both a trace and unrelated consumers without pre-filtering.
- RESEARCH: `IDocumentObject.Compute`'s trailing shape past its `Solution` argument is catalog-unstated — a consumer-drivable per-object evaluation case lands on `SolutionCommand` when the decompile fixes it. Route: `uv run python -m tools.assay api query 'Grasshopper2.Doc.IDocumentObject' --key gh2 --full`.
- Boundary: progress display, status-bar text, and run spinners are `Shell/chrome.md` and `Canvas/*` consumers of these receipts; `IDataAccess.Solution` — the component-side view of the same run — is `Components/component.md`'s seam; `SolutionServer.State` (`ServerState`) is the server-wide posture a shell status surface reads, distinct from any one run's phase.
- Packages: Grasshopper2 (`Solution.Id`/`Phase`/`Mode`/`ComputableCount`/`InvalidParameters`/`OverallProgress`/`Age`, `SolutionId`, `SolutionPhase`, `SolutionRecord.SolutionId`/`Culmination`/`StartTime`/`EndTime`/`Duration`), LanguageExt.Core, `Rasm.Domain`, `Shell/events.md` (`UiEvent`, `UiFact`, `SolutionSignal`).
- Growth: a new run metric is one field on the owning receipt with its claim row; a new timeline judgment is one claim inside `SolutionTrace.IsValid` — no new receipt species.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Grasshopper2.Doc;
using Rasm.Csp;
using Rasm.Grasshopper.Eto;
using Rasm.Grasshopper.Shell;

namespace Rasm.Grasshopper.Document;

// --- [MODELS] -------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct RunPulse(
    SolutionId Id, SolutionPhase Phase, SolutionMode Mode,
    int Computable, int Invalid, int Progress, TimeSpan Age) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Computable, floor: 0),
        ValidityClaim.CountAtLeast(count: Invalid, floor: 0),
        ValidityClaim.CountAtLeast(count: Progress, floor: 0),
        ValidityClaim.Nonnegative(value: Age.TotalSeconds));
    internal static RunPulse Of(Solution run) => new(
        Id: run.Id, Phase: run.Phase, Mode: run.Mode, Computable: run.ComputableCount,
        Invalid: run.InvalidParameters, Progress: run.OverallProgress, Age: run.Age);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct RunEvidence(
    SolutionId Id, SolutionPhase Culmination, DateTime Started, DateTime Ended, TimeSpan Duration) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(holds: Ended >= Started),
        ValidityClaim.Nonnegative(value: Duration.TotalSeconds));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SolutionTrace(Seq<(SolutionSignal Signal, long Stamp)> Pulses) : IValidityEvidence {
    public bool IsValid => Pulses.Fold(
        (Claim: ValidityClaim.Of(holds: true), Last: long.MinValue),
        static (state, pulse) => (
            Claim: ValidityClaim.All(state.Claim, ValidityClaim.Of(holds: pulse.Stamp >= state.Last)),
            Last: pulse.Stamp)).Claim;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static partial class SolutionControl {
    public static Fin<RunPulse> Probe(Solution run, Op? key = null) {
        Op active = key.OrDefault();
        return Optional(run).ToFin(active.InvalidInput())
            .Bind(live => EtoDispatch.Run(
                body: () => active.Catch(body: () => Fin.Succ(RunPulse.Of(run: live))), key: active));
    }

    public static Fin<RunEvidence> Audit(SolutionRecord record, Op? key = null) {
        Op active = key.OrDefault();
        return Optional(record).ToFin(active.InvalidInput())
            .Bind(done => active.Catch(body: () =>
                Fin.Succ(new RunEvidence(
                    Id: done.SolutionId,
                    Culmination: done.Culmination,
                    Started: done.StartTime,
                    Ended: done.EndTime,
                    Duration: done.Duration))));
    }

    public static SolutionTrace Trace(Seq<UiEvent> observed) =>
        new(Pulses: observed
            .Choose(static fact => fact.Fact is UiFact.SolutionCase solution
                ? Some((Signal: solution.Signal, Stamp: fact.Stamp))
                : Option<(SolutionSignal, long)>.None));
}
```

## [04]-[DENSITY_BAR]

| [INDEX] | [CONCERN]            | [OWNER]                 | [RAIL]                               | [CASES] |
| :-----: | :------------------- | :---------------------- | :----------------------------------- | :-----: |
|  [01]   | execution commands   | `SolutionCommand`       | `Drive → Fin<GateReceipt>`           |    6    |
|  [02]   | lifecycle watching   | `SolutionControl.Watch` | `Watch → Fin<Lease<UiSubscription>>` |    1    |
|  [03]   | in-flight inspection | `RunPulse`              | `Probe → Fin<RunPulse>`              |    1    |
|  [04]   | completion audit     | `RunEvidence`           | `Audit → Fin<RunEvidence>`           |    1    |
|  [05]   | phase timeline       | `SolutionTrace`         | `Trace → SolutionTrace`              |    1    |

- [01]-[EXECUTION_COMMANDS]: `[GenerateUnionOps]` `[Union]` with a per-case marshal lane.
- [02]-[LIFECYCLE_WATCHING]: six composed event rows, one leased subscription.
- [03]-[IN_FLIGHT_INSPECTION]: detached typed evidence over a live `Solution`.
- [04]-[COMPLETION_AUDIT]: the host's measured window, no unassigned counter republished.
- [05]-[PHASE_TIMELINE]: pure fold over captured `UiEvent`s, monotone claim.

`DocumentScope.Resolve`, `GateReceipt`, `GateOutcome`, `EtoDispatch`, `UiEvents`, `Op`, `Fault`, `Lease<T>`, and `ValidityClaim` are composed upstream owners; mutation-to-run correlation lands as `Watch` and `Trace` over the events algebra.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
