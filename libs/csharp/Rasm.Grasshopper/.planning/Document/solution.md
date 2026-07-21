# [RASM_GRASSHOPPER_DOCUMENT_SOLUTION]

`SolutionControl` is the execution controller of the GH2 document boundary — ONE solution owner over the host's `SolutionServer`: launching a run in every posture the server admits (fire, bridled by a cancellation source, or awaited to completion), halting, cancelling one in-flight `Solution`, driving the deferred-expiry protocol, expiring explicit object sets, the run-inspection pulse over a live `Solution`, the completion evidence over a `SolutionRecord`, and the phase-timeline fold over the six-event solution family observed through `Shell/events.md`'s rows.

`Watch` attaches the whole lifecycle family (`SolutionAboutToStart` → `Started` → `Stopped` → `Cancelled` → `Completed` → `Faulted`) as one leased subscription, and `Trace` folds what a watcher captured into ordered phase evidence, so a consumer correlates a mutation with the run it triggered. Execution control is command-shaped (one `SolutionCommand` union, one `Drive` gate), inspection is evidence-shaped (typed receipts, never live-object retention), and cancellation is the host's own `CancellationTokenSource` bridle carried as a case payload.

## [01]-[INDEX]

- [02]-[CONTROL]: `SolutionCommand` + `SolutionReceipt` + `SolutionControl.Drive`/`Watch` — the execution command union, the one settlement gate, and the leased lifecycle subscription.
- [03]-[EVIDENCE]: `RunPulse` + `RunEvidence` + `SolutionTrace` — in-flight inspection, completion audit, and the phase-timeline fold.

## [02]-[CONTROL]

- Owner: `SolutionCommand` `[Union]` `[GenerateUnionOps]` — the closed execution vocabulary. `LaunchCase(SolutionMode, Option<CancellationTokenSource>)` discriminates the two start shapes on payload presence — a bare mode rides `SolutionServer.Start(SolutionMode)`, a bridled launch rides `Start(CancellationTokenSource, SolutionMode)`; `AwaitCase(SolutionMode, CancellationTokenSource)` blocks to completion through `StartWait`; `HaltCase` stops the server; `CancelCase(Solution)` cancels one in-flight run cooperatively through `Solution.Cancel`; `DeferCase(IDocumentObject)` queues deferred expiry through `SolutionServer.DelayedExpire`; `FlushCase` drains the deferred queue through `ExpireDelayedObjects`; `ExpireCase(Seq<IDocumentObject>)` expires an explicit object set through each subject's own `IDocumentObject.Expire`. `SolutionReceipt` is the settlement evidence — the raising `Op`, the settled case name, and ordered entry/settlement stamps with elapsed latency from one `MonotonicTimeline` — implementing `IValidityEvidence`.
- Entry: `SolutionControl.Drive(SolutionCommand op, Option<HostDocument> graph = default, Op? key = null)` → `Fin<SolutionReceipt>` — the one execution gate; `SolutionControl.Watch(Action<UiEvent> publish, Option<HostDocument> graph = default, Op? key = null)` → `Fin<Lease<UiSubscription>>` — the whole six-row lifecycle family attached transactionally through `UiEvents.Observe` on the document's `SolutionServer` anchor, the subscription's lifetime the kernel lease.
- Law: expiry is a three-verb protocol on one owner — `DeferCase` queues, `FlushCase` drains, `ExpireCase` bypasses the queue for an explicit set — and document-wide expiry (`ObjectList.ExpireAll`) is `Document/graph.md`'s membership verb; a fourth expiry spelling anywhere in the folder is the deleted form.
- Law: cancellation is the bridle, never a flag — a launch that must be cancellable carries its `CancellationTokenSource` as the case payload, `CancelCase` targets one run's own cooperative gate, and a cancelled host call surfaces as the kernel's `Fault.Cancelled` through `Op.Catch`, never as a result failure.
- Law: `AwaitCase` is the one blocking shape — it exists for headless and scripted drives where the caller owns the thread; a canvas-side consumer launches and correlates completion through `Watch`, because blocking the UI marshal on `StartWait` starves the run it awaits.
- Boundary: the six lifecycle rows, their signal vocabulary, and the anchor admission are `Shell/events.md`'s algebra — `Watch` composes `UiSource.SolutionAboutToStart`/`SolutionStarted`/`SolutionStopped`/`SolutionCancelled`/`SolutionCompleted`/`SolutionFaulted` and adds no row of its own; per-component solution hooks (`BeforeProcess`/`PreProcess`/`PostProcess`) are `Components/component.md`'s lifecycle.
- Packages: Grasshopper2 (`SolutionServer.Start`/`StartWait`/`Stop`/`DelayedExpire`/`ExpireDelayedObjects`, `Solution.Cancel`, `SolutionMode`, `IDocumentObject.Expire`), LanguageExt.Core, `Rasm.Domain`, `Rasm.Parametric` (`MonotonicTimeline`, `MonotonicStamp`), `Shell/events.md` (`UiEvents`, `UiSource`, `EventAnchor`, `UiEvent`, `UiSubscription`).
- Growth: a new execution posture is one `SolutionCommand` case breaking the gate's total `Switch` loudly; a new lifecycle stream on `Watch` is one composed `UiSource` row — the gate pair never widens.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Grasshopper2.Doc;
using Rasm.Csp;
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
    public sealed record FlushCase : SolutionCommand;
    public sealed record ExpireCase(Seq<IDocumentObject> Subjects) : SolutionCommand;
}

// --- [MODELS] -------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SolutionReceipt(
    Op Operation, string Verb, MonotonicStamp Entered, MonotonicStamp Settled, TimeSpan Latency) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(holds: !string.IsNullOrWhiteSpace(value: Verb)),
        ValidityClaim.Evidence(evidence: Entered),
        ValidityClaim.Evidence(evidence: Settled),
        ValidityClaim.Nonnegative(value: Latency.TotalSeconds));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
[BoundaryAdapter]
public static partial class SolutionControl {
    public static Fin<SolutionReceipt> Drive(SolutionCommand op, Option<HostDocument> graph = default, Op? key = null) {
        Op active = key.OrDefault();
        return from valid in Optional(op).ToFin(active.InvalidInput())
               from timeline in MonotonicTimeline.Of(provider: TimeProvider.System, key: active)
               from entered in timeline.Capture(key: active)
               from verb in DocumentScope.Resolve(graph: graph, key: active, body: document => valid.Switch(
                state: (Key: active, Server: document.Solution),
                launchCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ((Op.Side(action: () => c.Bridle.Match(
                        Some: bridle => frame.Server.Start(bridle, c.Mode),
                        None: () => frame.Server.Start(c.Mode))), nameof(SolutionCommand.LaunchCase)).Item2)),
                awaitCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ((Op.Side(action: () => frame.Server.StartWait(c.Bridle, c.Mode)), nameof(SolutionCommand.AwaitCase)).Item2)),
                haltCase: static (frame, _) => frame.Key.Catch(body: () =>
                    Fin.Succ((Op.Side(action: frame.Server.Stop), nameof(SolutionCommand.HaltCase)).Item2)),
                cancelCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ((Op.Side(action: c.Run.Cancel), nameof(SolutionCommand.CancelCase)).Item2)),
                deferCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ((Op.Side(action: () => frame.Server.DelayedExpire(c.Subject)), nameof(SolutionCommand.DeferCase)).Item2)),
                flushCase: static (frame, _) => frame.Key.Catch(body: () =>
                    Fin.Succ((Op.Side(action: frame.Server.ExpireDelayedObjects), nameof(SolutionCommand.FlushCase)).Item2)),
                expireCase: static (frame, c) => frame.Key.Catch(body: () =>
                    Fin.Succ((c.Subjects.Fold(unit, static (_, subject) => Op.Side(action: subject.Expire)), nameof(SolutionCommand.ExpireCase)).Item2))))
               from settled in timeline.Capture(key: active)
               from latency in timeline.Elapsed(start: entered, end: settled, key: active)
               select new SolutionReceipt(
                   Operation: active, Verb: verb, Entered: entered, Settled: settled, Latency: latency);
    }

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

- Owner: `RunPulse` — the in-flight inspection receipt over one live `Solution`: the run id, the rendered phase name, and the invalid-parameter count, detached at read time so a stale pulse can never hand out run internals. `RunEvidence` — the completion audit over one `SolutionRecord`: expired and solved object counts, normalized progress, and the rendered culmination, the counts and progress claim-checked. `SolutionTrace` — the phase-timeline fold over a watcher's captured `UiEvent` sequence: each solution fact projects to its signal row and stamp, and validity claims the stamps are monotone, so a trace that interleaved two runs' events fails its own evidence.
- Entry: `SolutionControl.Probe(Solution run, Op? key = null)` → `Fin<RunPulse>`; `SolutionControl.Audit(SolutionRecord record, Op? key = null)` → `Fin<RunEvidence>`; `SolutionControl.Trace(Seq<UiEvent> observed)` → `SolutionTrace` — a pure fold, no marshal, because the events are already detached evidence.
- Law: inspection detaches — a receipt never retains the `Solution` or `SolutionRecord` it read; correlation across receipts rides the run id, so evidence outlives the run without pinning host state.
- Law: the trace consumes only solution facts — the fold keeps `UiFact.SolutionCase` rows and drops every other fact a shared watcher may have captured, so one `Watch` callback can feed both a trace and unrelated consumers without pre-filtering.
- RESEARCH: `Solution.Phase`'s vocabulary type and `SolutionRecord.Culmination`'s case family are catalog-unstated — both receipts carry the rendered name until the decompile fixes the enum, at which point each becomes one typed field swap; `Solution.Id`'s `Guid` reading, `InvalidParameters`' element carrier, and the count/progress scalar spellings re-verify at the same pass; `IDocumentObject.Compute`'s trailing shape past its `Solution` argument is catalog-unstated — a consumer-drivable per-object evaluation case lands on `SolutionCommand` when the decompile fixes it.
- Boundary: progress display, status-bar text, and run spinners are `Shell/chrome.md` and `Canvas/*` consumers of these receipts; `IDataAccess.Solution` — the component-side view of the same run — is `Components/component.md`'s seam.
- Packages: Grasshopper2 (`Solution.Id`/`Phase`/`InvalidParameters`, `SolutionRecord.Culmination`/`ExpiredCount`/`SolvedCount`/`Progress`), LanguageExt.Core, `Rasm.Domain`, `Shell/events.md` (`UiEvent`, `UiFact`, `SolutionSignal`).
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
public readonly record struct RunPulse(Guid Id, string Phase, int InvalidCount) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(holds: Id != Guid.Empty),
        ValidityClaim.Of(holds: !string.IsNullOrWhiteSpace(value: Phase)),
        ValidityClaim.CountAtLeast(count: InvalidCount, floor: 0));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct RunEvidence(int Expired, int Solved, double Progress, string Culmination) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.CountAtLeast(count: Expired, floor: 0),
        ValidityClaim.CountAtLeast(count: Solved, floor: 0),
        ValidityClaim.UnitInterval(value: Progress),
        ValidityClaim.Of(holds: !string.IsNullOrWhiteSpace(value: Culmination)));
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
            .Bind(live => EtoDispatch.Run(body: () => active.Catch(body: () =>
                Fin.Succ(new RunPulse(
                    Id: live.Id,
                    Phase: live.Phase.ToString(),
                    InvalidCount: toSeq(live.InvalidParameters).Count))), key: active));
    }

    public static Fin<RunEvidence> Audit(SolutionRecord record, Op? key = null) {
        Op active = key.OrDefault();
        return Optional(record).ToFin(active.InvalidInput())
            .Bind(done => active.Catch(body: () =>
                Fin.Succ(new RunEvidence(
                    Expired: done.ExpiredCount,
                    Solved: done.SolvedCount,
                    Progress: done.Progress,
                    Culmination: done.Culmination.ToString()))));
    }

    public static SolutionTrace Trace(Seq<UiEvent> observed) =>
        new(Pulses: observed
            .Choose(static fact => fact.Fact is UiFact.SolutionCase solution
                ? Some((Signal: solution.Signal, Stamp: fact.Stamp))
                : Option<(SolutionSignal, long)>.None));
}
```

## [04]-[DENSITY_BAR]

| [INDEX] | [CONCERN]            | [OWNER]                               | [RAIL]                               | [CASES] |
| :-----: | :------------------- | :------------------------------------ | :----------------------------------- | :-----: |
|  [01]   | execution commands   | `SolutionCommand` + `SolutionReceipt` | `Drive → Fin<SolutionReceipt>`       |    7    |
|  [02]   | lifecycle watching   | `SolutionControl.Watch`               | `Watch → Fin<Lease<UiSubscription>>` |    1    |
|  [03]   | in-flight inspection | `RunPulse`                            | `Probe → Fin<RunPulse>`              |    1    |
|  [04]   | completion audit     | `RunEvidence`                         | `Audit → Fin<RunEvidence>`           |    1    |
|  [05]   | phase timeline       | `SolutionTrace`                       | `Trace → SolutionTrace`              |    1    |

- [01]-[EXECUTION_COMMANDS]: `[GenerateUnionOps]` `[Union]` + evidence receipt.
- [02]-[LIFECYCLE_WATCHING]: six composed event rows, one leased subscription.
- [03]-[IN_FLIGHT_INSPECTION]: detached evidence over a live `Solution`.
- [04]-[COMPLETION_AUDIT]: claim-checked counts and progress.
- [05]-[PHASE_TIMELINE]: pure fold over captured `UiEvent`s, monotone claim.

`DocumentScope.Resolve`, `EtoDispatch`, `UiEvents`, `Op`, `Fault`, `Lease<T>`, and `ValidityClaim` are composed upstream owners; mutation-to-run correlation lands as `Watch` and `Trace` over the events algebra.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
