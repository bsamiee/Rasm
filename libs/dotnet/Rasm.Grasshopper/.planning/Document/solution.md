# [RASM_GRASSHOPPER_DOCUMENT_SOLUTION]

`SolutionControl` is the execution controller of the GH2 document boundary — ONE solution owner over the host's `SolutionServer`: launching a run in every posture the server admits (fire, bridled by a cancellation source, or awaited to completion on the caller's own thread under a REQUIRED wait budget), halting, cancelling one in-flight `Solution`, queueing deferred expiry, expiring explicit object sets, the run-inspection pulse over a live `Solution`, the completion audit over a `SolutionRecord`, and the phase-timeline fold over the six-event solution family observed through `Shell/events.md`'s rows.

`Watch` attaches the whole lifecycle family (`SolutionAboutToStart` → `Started` → `Stopped` → `Cancelled` → `Completed` → `Faulted`) as one leased kernel subscription draining into the caller's `EvidenceDrain<GhFact>`, and `Trace` folds what a drain captured into ordered phase evidence, so a consumer correlates a mutation with the run it triggered. Execution control is command-shaped — one `SolutionCommand` union whose `MarshalLane` column names each case's thread custody and one `Drive` gate on `Document/document.md`'s `DocumentGate.Run` spine — inspection is evidence-shaped through two Mapperly projections (detached values, never live-object retention), and cancellation is the host's own `CancellationTokenSource` bridle carried as a case payload.

## [01]-[INDEX]

- [02]-[CONTROL]: `MarshalLane` + `WaitPosture` + `SolutionCommand` + `SolutionControl.Drive`/`Watch` — the execution command union with its thread-custody column, the bounded blocking posture, the command gate, and the leased lifecycle subscription.
- [03]-[EVIDENCE]: `RunPulse` + `SolutionAudit` + `SolutionTrace` + `SolutionMap` — in-flight inspection, completion audit, the phase-timeline fold, and the Mapperly projection seam.

## [02]-[CONTROL]

- Owner: `MarshalLane` `[SmartEnum<int>]` — the thread-custody vocabulary: `Window` (the case runs inside the shared marshal window) and `Worker` (the case blocks the caller's own thread and must NOT hold the marshal); the lane is a COLUMN on `SolutionCommand`, derived from the case itself, so the dispatch reads custody off the value and no `is`-ladder at the gate re-derives it. `WaitPosture` `[ValueObject<TimeSpan>]` — the REQUIRED wait budget of a blocking await, positive by construction; an unbounded block on a live UI application is the hazard the budget deletes, and exhaustion is a typed refusal, never a hang. `SolutionCommand` `[Union]` `[GenerateUnionOps]` — the closed execution vocabulary. `LaunchCase(SolutionMode, Option<CancellationTokenSource>)` discriminates the two start shapes on payload presence — a bare mode rides `SolutionServer.Start(SolutionMode)`, a bridled launch rides `Start(CancellationTokenSource, SolutionMode)` — and returns the moment the run is dispatched; `AwaitCase(SolutionMode, CancellationTokenSource, WaitPosture)` drives the same `Start` and blocks the caller's own thread on the `Task<Solution>` it hands back, at most the posture's budget; `HaltCase` stops the server; `CancelCase(Solution)` cancels one in-flight run cooperatively through `Solution.Cancel`; `DeferCase(IDocumentObject)` queues deferred expiry through `SolutionServer.DelayedExpire`; `ExpireCase(Seq<IDocumentObject>)` expires an explicit object set through each subject's own `IDocumentObject.Expire`. Awaited runs answer `GateOutcome.RunCase` with the final pulse.
- Entry: `SolutionControl.Drive(SolutionCommand op, Option<HostDocument> graph = default, Option<HookRail<GrasshopperPoint, HookSignal, HookScope>> rail = default, Op? key = null)` → `Fin<GateOutcome>` — the one execution gate; `SolutionControl.Watch(EvidenceDrain<GhFact> drain, Atomicity atomicity, Option<HostDocument> graph = default, Op? key = null)` → `Fin<Lease<UiSubscription<GhFact>>>` — the whole six-row lifecycle family attached transactionally through the kernel `UiEvents.Observe` over `GhSource.Of(document.Solution)`, the subscription's lifetime the kernel lease.
- Law: the gate is the `solution.lifecycle` fire site — every command heralds `GrasshopperPoint.SolutionLifecycle` (`Observe` modality) on the injected rail with its own op and the document identity before the host verb runs, because the host's `SolutionEventArgs` carries no cancellation and observers therefore attach to the GATES, not the events; an absent rail drives unobserved.
- Law: `Worker` custody BYPASSES the marshal, and that bypass is what makes the blocking posture satisfiable. `SolutionServer.Start` runs the whole solve on a threadpool worker and hands back its `Task<Solution>`, so the run settles independently of the UI idle loop and the only thread that blocks is the caller's own. Routing the await through the marshal like every `Window` case posts the block ONTO the idle loop the run does not need but every other gate does, which is the starvation the marshal law names; the worker path therefore probes the kernel's `UiThread.OnMarshal` and refuses with `KernelFault.InvalidContext` when the caller already holds the UI thread. Host's own `StartWait` is that same deadlock as a member and never enters the gate.
- Law: the wait is BOUNDED — the worker path waits `Task.Wait(posture, bridle.Token)` and a budget that lapses refuses with the folder's typed overdue fault carrying the budget it exhausted; the dispatched run keeps running (the bridle, not the wait, owns cancellation), so a caller that wants the run dead on timeout cancels its own `CancellationTokenSource` on the refusal.
- Law: every `Window` case shares ONE marshal through `DocumentGate.Run`, so no live server handle crosses back out; run lifecycle facts arrive on the explicit `Watch` stream.
- Law: expiry is a two-verb protocol on one owner — `DeferCase` queues and `ExpireCase` bypasses the queue for an explicit set — because the host drains its own deferred queue internally at run start and publishes no drain member; document-wide expiry (`ObjectList.ExpireAll`) is `Document/graph.md`'s membership verb, and a third expiry spelling anywhere in the folder is the deleted form.
- Law: cancellation is the bridle, never a flag — a launch that must be cancellable carries its `CancellationTokenSource` as the case payload, `CancelCase` targets one run's own cooperative gate, and a cancelled host call surfaces as the kernel's `KernelFault.Cancelled` through `Op.Catch`, never as a result failure.
- Boundary: the six lifecycle rows, their signal vocabulary, and the anchor admission are `Shell/events.md`'s algebra — `Watch` composes `GhSource.Of(SolutionServer)` under `EventAnchor.Ambient` and adds no row of its own; per-component solution hooks (`BeforeProcess`/`PreProcess`/`PostProcess`) are `Components/component.md`'s lifecycle.
- Packages: Grasshopper2 (`SolutionServer.Start`/`Stop`/`DelayedExpire`, `Solution.Cancel`, `SolutionMode`, `IDocumentObject.Expire`), `Rasm.Interaction` (`UiThread`, `UiEvents`, `EvidenceDrain`, `Atomicity`, `UiSubscription`), `Rasm.Domain` (`Lease<T>`), `Shell/events.md` (`GhSource`, `GhFact`), `Shell/hooks.md` (`GrasshopperPoint`, `HookSignal`, `HookScope`), `Document/document.md` (`DocumentGate`, `GateOutcome`, `RunPulse`), `Components/data.md` (`GhFault`), Thinktecture, LanguageExt.Core, `Rasm.Domain`.
- Growth: a new execution posture is one `SolutionCommand` case breaking the gate's total `Switch` loudly and naming its lane on the column; a new lifecycle stream on `Watch` is one composed `GhSource` row — the gate pair never widens.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Grasshopper2.Doc;
using Rasm.Domain;
using Rasm.Grasshopper.Components;
using Rasm.Grasshopper.Shell;
using Rasm.Interaction;
using HostDocument = Grasshopper2.Doc.Document;

namespace Rasm.Grasshopper.Document;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class MarshalLane {
    public static readonly MarshalLane Window = new(key: 0);
    public static readonly MarshalLane Worker = new(key: 1);
}

[ValueObject<TimeSpan>]
public readonly partial struct WaitPosture {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref TimeSpan value) {
        if (value <= TimeSpan.Zero) { validationError = new ValidationError("a blocking wait requires a positive budget"); }
    }
}

[Union]
[GenerateUnionOps]
public abstract partial record SolutionCommand {
    private SolutionCommand() { }
    public MarshalLane Lane => this is AwaitCase ? MarshalLane.Worker : MarshalLane.Window;
    public sealed record LaunchCase(SolutionMode Mode, Option<CancellationTokenSource> Bridle) : SolutionCommand;
    public sealed record AwaitCase(SolutionMode Mode, CancellationTokenSource Bridle, WaitPosture Wait) : SolutionCommand;
    public sealed record HaltCase : SolutionCommand;
    public sealed record CancelCase(Solution Run) : SolutionCommand;
    public sealed record DeferCase(IDocumentObject Subject) : SolutionCommand;
    public sealed record ExpireCase(Seq<IDocumentObject> Subjects) : SolutionCommand;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[BoundaryAdapter]
public static partial class SolutionControl {
    public static Fin<GateOutcome> Drive(
        SolutionCommand op,
        Option<HostDocument> graph = default,
        Option<HookRail<GrasshopperPoint, HookSignal, HookScope>> rail = default,
        Op? key = null) {
        Op active = key.OrDefault();
        return Optional(op).ToFin(active.InvalidInput()).Bind(valid => valid.Lane == MarshalLane.Worker
            ? Blocked(command: (SolutionCommand.AwaitCase)valid, graph: graph, rail: rail, key: active)
            : DocumentGate.Run(
                graph: graph, key: active,
                body: document => Heralded(rail: rail, op: valid.SelfOp, subject: Some(document.Identity), key: active)
                    .Bind(_ => valid.Switch(
                state: (Key: active, Server: document.Solution),
                launchCase: static (frame, c) => Settle(frame.Key, () =>
                    (Op.Side(action: () => ignore(c.Bridle.Match(
                        Some: bridle => frame.Server.Start(bridle, c.Mode),
                        None: () => frame.Server.Start(c.Mode)))), (GateOutcome)new GateOutcome.SettledCase()).Item2),
                awaitCase: static (frame, _) => Fin.Fail<GateOutcome>(frame.Key.InvalidContext()),
                haltCase: static (frame, c) => Settle(frame.Key, () =>
                    (Op.Side(action: frame.Server.Stop), (GateOutcome)new GateOutcome.SettledCase()).Item2),
                cancelCase: static (frame, c) => Settle(frame.Key, () =>
                    (Op.Side(action: c.Run.Cancel), (GateOutcome)new GateOutcome.SettledCase()).Item2),
                deferCase: static (frame, c) => Settle(frame.Key, () =>
                    (Op.Side(action: () => frame.Server.DelayedExpire(c.Subject)), (GateOutcome)new GateOutcome.SettledCase()).Item2),
                expireCase: static (frame, c) => Settle(frame.Key, () =>
                    (c.Subjects.Iter(static subject => subject.Expire()),
                     (GateOutcome)new GateOutcome.CountCase(Touched: c.Subjects.Count)).Item2))));
    }

    private static Fin<GateOutcome> Blocked(
        SolutionCommand.AwaitCase command,
        Option<HostDocument> graph,
        Option<HookRail<GrasshopperPoint, HookSignal, HookScope>> rail,
        Op key) =>
        from onMarshal in UiThread.OnMarshal(key: key)
        from _ in guard(!onMarshal, (Error)key.InvalidContext()).ToFin()
        from seat in DocumentGate.Resolve(graph: graph, key: key,
            body: document => key.Catch(body: () => Fin.Succ((document.Identity, Server: document.Solution))))
        from heralded in Heralded(rail: rail, op: command.SelfOp, subject: Some(seat.Identity), key: key)
        from run in key.Catch(body: () => {
            Task<Solution> task = seat.Server.Start(command.Bridle, command.Mode);
            return task.Wait((TimeSpan)command.Wait, command.Bridle.Token)
                ? Fin.Succ(task.Result)
                : Fin.Fail<Solution>(new GhFault.Overdue(Key: key, Detail: $"solution wait budget {command.Wait} lapsed"));
        }, token: command.Bridle.Token)
        select (GateOutcome)new GateOutcome.RunCase(Pulse: SolutionMap.Pulse(run: run));

    private static Fin<Unit> Heralded(
        Option<HookRail<GrasshopperPoint, HookSignal, HookScope>> rail, Op op, Option<Guid> subject, Op key) =>
        rail.Match(
            Some: live => live.Fire(
                    at: GrasshopperPoint.SolutionLifecycle,
                    fact: new HookSignal.IntentCase(Operation: op, DocumentId: subject),
                    key: key)
                .Map(static _ => unit),
            None: () => Fin.Succ(unit));

    private static Fin<GateOutcome> Settle(Op key, Func<GateOutcome> settle) =>
        key.Catch(body: () => Fin.Succ(settle()));

    public static Fin<Lease<UiSubscription<GhFact>>> Watch(
        EvidenceDrain<GhFact> drain, Atomicity atomicity, Option<HostDocument> graph = default, Op? key = null) {
        Op active = key.OrDefault();
        return Optional(drain).ToFin(active.InvalidInput())
            .Bind(sink => DocumentGate.Resolve(graph: graph, key: active, body: document =>
                UiEvents.Observe(
                    anchor: EventAnchor.Ambient,
                    drain: sink,
                    atomicity: atomicity,
                    key: active,
                    rows: [.. GhSource.Of(document.Solution)])));
    }
}
```

## [03]-[EVIDENCE]

- Owner: `RunPulse` — the in-flight inspection over one live `Solution` (`Document/document.md`'s payload record beside the spine, projected here): the typed `SolutionId`, the `SolutionPhase` the run holds at the read, the `SolutionMode` it launched under, its computable and invalid-parameter counts, its overall progress, and its age, every field detached at read time so a stale pulse can never hand out run internals. `SolutionAudit` — the completion audit over one `SolutionRecord`: the run id, the `SolutionPhase` it culminated in, and the start/end window with its derived duration (renamed from the evidence-noun the fabrication branch owns — `RunEvidence@Rasm.Fabrication` — so one name means one thing across the estate). `SolutionTrace` — the phase-timeline fold over a drain's captured `UiEvent<GhFact>` sequence: each solution fact projects to its signal row, its run id, and its drain-minted ordinal; validity claims the ordinals are monotone AND every identified pulse names ONE run, so a trace that interleaved two runs' events fails its own evidence instead of reading as a single timeline. `SolutionMap` — the one Mapperly seam projecting both detached values, so the field correspondence is generated, inspectable, and single-sourced.
- Entry: `SolutionControl.Probe(Solution run, Guid document, Op? key = null)` → `Fin<RunPulse>`; `SolutionControl.Audit(SolutionRecord record, Guid document, Op? key = null)` → `Fin<SolutionAudit>`; `SolutionControl.Trace(Seq<UiEvent<GhFact>> observed, Guid document)` → `Fin<SolutionTrace>` — a pure fold, no marshal, because the events are already detached evidence; the document identity is the `gh.doc` attribution each write carries.
- Law: `SolutionMap.Pulse` is the pure projection and every reader composes it — `Probe` marshals it over a live run and the awaited drive folds it into its own outcome, so the in-flight snapshot has one spelling regardless of which gate asks.
- Law: the three readers write their own rows — `Probe` writes `GhInstruments.Probed`, `Audit` writes `GhInstruments.Ran`, and `Trace` writes `GhInstruments.Chronicled`, each for the run's document, so `solution.invalid`, `solution.runs`, and `solution.pulses` land where the value settles and nowhere else.
- Law: the audit publishes only what the host measures — `SolutionRecord`'s `ExpiredCount`, `SolvedCount`, and `Progress` are auto-properties its one constructor never assigns, so every completed record reads them as a structural zero no run produced; carrying them fabricates a measurement, and the per-object counts a consumer wants ride the `Watch` stream's own object rows instead.
- Law: inspection detaches — a pulse or audit never retains the `Solution` or `SolutionRecord` it read; correlation across them rides the typed `SolutionId`, so evidence outlives the run without pinning host state.
- Law: the trace consumes only solution facts — the fold keeps `GhFact.SolutionCase` rows and drops every other fact a shared drain may have captured, so one `Watch` drain can feed both a trace and unrelated consumers without pre-filtering.
- Boundary: progress display, status-bar text, and run spinners are `Shell/chrome.md` and `Canvas/*` consumers of these values; `IDataAccess.Solution` — the component-side view of the same run — is `Components/component.md`'s seam; `SolutionServer.State` (`ServerState`) is the server-wide posture a shell status surface reads, distinct from any one run's phase. `IDocumentObject.Compute(Solution, CallStack)` is engine plumbing — the solver hands it the live run and its call stack, so no consumer-drivable evaluation case exists to mint and none enters `SolutionCommand`.
- Packages: Grasshopper2 (`Solution.Id`/`Phase`/`Mode`/`ComputableCount`/`InvalidParameters`/`OverallProgress`/`Age`, `SolutionId`, `SolutionPhase`, `SolutionRecord.SolutionId`/`Culmination`/`StartTime`/`EndTime`/`Duration`), Riok.Mapperly, `Rasm.Interaction` (`UiThread`, `UiDispatch`, `DispatchLane`, `UiEvent`), `Shell/events.md` (`GhFact`, `SolutionSignal`), `Shell/telemetry.md` (`GhInstruments`), LanguageExt.Core, `Rasm.Domain`.
- Growth: a new run metric is one field on the owning value with its claim row and its generated map line; a new timeline judgment is one claim inside `SolutionTrace.IsValid` — no new species.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Grasshopper2.Doc;
using Rasm.Domain;
using Rasm.Grasshopper.Shell;
using Rasm.Interaction;
using Riok.Mapperly.Abstractions;

namespace Rasm.Grasshopper.Document;

// --- [MODELS] --------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SolutionAudit(
    SolutionId Id, SolutionPhase Culmination, DateTime Started, DateTime Ended, TimeSpan Duration) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Ended >= Started,
        ValidityClaim.Nonnegative(value: Duration.TotalSeconds));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SolutionTrace(
    Seq<(SolutionSignal Signal, Option<SolutionId> Id, long Ordinal)> Pulses) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Pulses.Fold(
            (Claim: new ValidityClaim(Holds: true), Last: long.MinValue),
            static (state, pulse) => (
                Claim: ValidityClaim.All(state.Claim, pulse.Ordinal >= state.Last),
                Last: pulse.Ordinal)).Claim,
        Pulses.Choose(static pulse => pulse.Id).Distinct().Count <= 1);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class SolutionMap {
    [MapProperty(nameof(Solution.ComputableCount), nameof(RunPulse.Computable))]
    [MapProperty(nameof(Solution.InvalidParameters), nameof(RunPulse.Invalid))]
    [MapProperty(nameof(Solution.OverallProgress), nameof(RunPulse.Progress))]
    public static partial RunPulse Pulse(Solution run);

    [MapProperty(nameof(SolutionRecord.SolutionId), nameof(SolutionAudit.Id))]
    [MapProperty(nameof(SolutionRecord.StartTime), nameof(SolutionAudit.Started))]
    [MapProperty(nameof(SolutionRecord.EndTime), nameof(SolutionAudit.Ended))]
    public static partial SolutionAudit Audit(SolutionRecord record);
}

public static partial class SolutionControl {
    public static Fin<RunPulse> Probe(Solution run, Guid document, Op? key = null) {
        Op active = key.OrDefault();
        return Optional(run).ToFin(active.InvalidInput())
            .Bind(live => UiThread.Run(
                new UiDispatch<RunPulse>.Blocking(() => active.Catch(body: () => Fin.Succ(SolutionMap.Pulse(run: live)))),
                DispatchLane.Interactive, active))
            .Bind(pulse => GhInstruments.Probed(document: document, pulse: pulse).Map(_ => pulse));
    }

    public static Fin<SolutionAudit> Audit(SolutionRecord record, Guid document, Op? key = null) {
        Op active = key.OrDefault();
        return Optional(record).ToFin(active.InvalidInput())
            .Bind(done => active.Catch(body: () => Fin.Succ(SolutionMap.Audit(record: done))))
            .Bind(audit => GhInstruments.Ran(document: document, audit: audit).Map(_ => audit));
    }

    public static Fin<SolutionTrace> Trace(Seq<UiEvent<GhFact>> observed, Guid document) =>
        new SolutionTrace(Pulses: observed
            .Choose(static envelope => envelope.Fact is GhFact.SolutionCase solution
                ? Some((Signal: solution.Signal, Id: solution.Id, Ordinal: envelope.Ordinal))
                : Option<(SolutionSignal, Option<SolutionId>, long)>.None)) switch {
            var trace => GhInstruments.Chronicled(document: document, trace: trace).Map(_ => trace),
        };
}
```

## [04]-[DENSITY_BAR]

| [INDEX] | [CONCERN]            | [OWNER]                 | [RAIL]                                      | [CASES] |
| :-----: | :------------------- | :---------------------- | :------------------------------------------ | :-----: |
|  [01]   | thread custody       | `MarshalLane`           | column on `SolutionCommand`                 |    2    |
|  [02]   | wait budget          | `WaitPosture`           | positive by construction, typed overdue     |    1    |
|  [03]   | execution commands   | `SolutionCommand`       | `Drive → Fin<GateOutcome>` + herald         |    6    |
|  [04]   | lifecycle watching   | `SolutionControl.Watch` | kernel `Observe` over `GhSource.Of(server)` |    1    |
|  [05]   | projection seam      | `SolutionMap`           | generated `Pulse`/`Audit` maps              |    2    |
|  [06]   | in-flight inspection | `RunPulse`              | `Probe → Fin<RunPulse>`                     |    1    |
|  [07]   | completion audit     | `SolutionAudit`         | `Audit → Fin<SolutionAudit>`                |    1    |
|  [08]   | phase timeline       | `SolutionTrace`         | `Trace → Fin<SolutionTrace>`                |    1    |

`DocumentGate.Run`/`Resolve`, `GateOutcome`, kernel `UiThread`/`UiEvents`/`EvidenceDrain`, `GhSource`, `GhFault`, `Op`, `Fault`, `Lease<T>`, and `ValidityClaim` are composed upstream owners; the `nameof` verb strings, the unbounded `GetAwaiter().GetResult()` block, the `is`-ladder custody split, the hand projection bodies, and the fabrication-branch name collision are all deleted.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
