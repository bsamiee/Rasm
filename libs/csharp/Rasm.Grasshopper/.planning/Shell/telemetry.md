# [RASM_GRASSHOPPER_SHELL_TELEMETRY]

`GhTelemetry` owns the boundary's telemetry admission and receipt projection: one injected `IMeterFactory` mints the `Rasm.Grasshopper` meter, one `GhEvidence` union closes the folder's receipt families, and one total fold turns each receipt into UCUM-named `rasm.grasshopper.*` instrument writes carrying document and plugin attribution. Emitting pages pass receipts and never spell a meter call; providers, exporters, views, and unload custody stay at the app root, so the folder holds zero OpenTelemetry reference.

## [01]-[INDEX]

- [02]-[CUSTODY]: injected factory admission, per-ALC unload custody, and the app-root obligation set
- [03]-[ROSTER]: instrument rows, bucket advice, and the receipt-field-to-instrument kind table
- [04]-[PROJECTION]: the evidence union, the projection fold, and the attribution tag law

## [02]-[CUSTODY]

- Owner: `GhTelemetry` — the composition capsule pairing the factory-owned instrument spine with logger admission. `GhInstruments` mints the `Rasm.Grasshopper` meter through `IMeterFactory.Create(MeterOptions)` exactly once, stamping the composing plugin's identity as a meter-scope tag.
- Entry: `GhTelemetry.Of(IMeterFactory factory, string plugin, Option<ILoggerFactory> logs = default, Option<string> version = default, Op? key = null)` → `Fin<GhTelemetry>` — the one admission gate; `Instruments` and `Logs` are the two capability slots consumers reach.
- Law: the injected factory is the sole per-ALC meter lifetime owner — a composing plugin passes its `PluginTelemetryHost.Meters`, and `AssemblyLoadContext.Unloading` drives the host's `ForceFlush`-then-`Dispose` on both providers, so no instrument outlives its plugin and an unload never drops the tail of an export batch. `GhTelemetry.Dispose` unbinds the composition logger only; disposing the minted meter here competes with provider custody.
- Law: a composition that runs logger-less takes `NullLoggerFactory.Instance` through the `Option` default, never a nullable factory; fault-family `[LoggerMessage]` partials live beside their retaining owners (`Canvas/paint.md` `PaintLog`, `Shell/events.md` `UiEventsLog`, `Eto/runtime.md` `RuntimeLog`, `Platform/native.md` `NativeLog`) and resolve their `ILogger` through `GhLog.For` at the fault-record site, so a retained fault emits once when it lands and no consumer polls a `LastFault` cell.
- Law: `GhLog` is the per-load-context ambient logger cell under first-mount-wins seat custody — `Of` binds a SUPPLIED factory only while the seat is free and holds the seat token, a later capsule keeps its own `Logs` without overwriting the live binding, and `Dispose` restores `NullLoggerFactory.Instance` only through its own token, so disposing one capsule never disables another still-live one; collectible plugin ALCs isolate the static per plugin, so two co-resident plugins never share a binding, and an unbound context emits into the null logger at zero cost. A `GhFault`-raising Components page takes `ILogger` by injection alone because the island imports no UI-thread sibling.
- Law: two co-resident plugins each `Of` over their own per-ALC factory, so identical `rasm.grasshopper.*` instrument names stay isolated by provider scope and the `gh.plugin` meter tag attributes each series to its composing plugin.
- Boundary: the contributor port is kernel vocabulary — the app root mints the string-scoped `TelemetryContributorPort` over this page's roster with `Scope` `Rasm.Grasshopper` and admits the meter by name, while `GhInstruments` keeps its meter minted through the injected per-ALC factory and projects the typed `GhEvidence` union pre-envelope (the typed-fold family beside Compute `ComputeInstrumentFan`); an envelope kind-arm table on this side is a second truth beside the typed fold and never lands here.
- Boundary: app-root obligations — the provider admits the `Rasm.Grasshopper` meter by name; sampler, exemplar filter, views, cardinality caps, and OTLP egress bind at the provider; `HybridCacheOptions.ReportTagMetrics` with the `gh-doc:{documentId:N}` dimension, the raster serializer, and the `MaximumPayloadBytes` sizing ride the `libs/csharp/.api/api-hybrid-cache.md` app-root obligations — this folder emits receipts and cache tags, never provider registrations.
- Packages: BCL inbox (`System.Diagnostics.Metrics` — `IMeterFactory`, `MeterOptions`, `Meter`, `InstrumentAdvice<T>`), Microsoft.Extensions.Logging.Abstractions (`ILoggerFactory`, `NullLoggerFactory`), LanguageExt.Core.
- Growth: a new capability slot on the capsule is one property with its admission default; a new attribution axis is one meter-scope tag at the mint.

## [03]-[ROSTER]

- Owner: `GhInstruments` — the instrument roster minted once at construction; the frame and acknowledgement histograms ship the kernel `Buckets.CanvasFrameSeconds` and `Buckets.AckSeconds` advice rows through `Buckets.Advised` as the fallback a backend without base2-exponential histograms reads.
- Law: instrument identity de-duplicates by name inside the meter, so name, unit, and description are declaration facts spelled once at the create site; units are UCUM (`s`, `{mark}`, `{command}`) and never pre-baked into the name.
- Law: every row is a projection of a typed receipt already on disk — a metric minted beside this roster is a second truth, and a receipt field no row projects stays receipt-only by declaration.
- Law: the kind table is the closed field-to-instrument correspondence; a new projected field is one table row, one instrument declaration, and one arm edit, never a call-site meter write.

Instrument cells extend the `rasm.grasshopper.` prefix.

| [INDEX] | [FACT_FIELD]                      | [INSTRUMENT]       | [UNIT]        | [KIND]              | [TAGS]                          |
| :-----: | :-------------------------------- | :----------------- | :------------ | :------------------ | :------------------------------ |
|  [01]   | `PaintReceipt.Latency`            | `paint.duration`   | `s`           | `Histogram<double>` | `gh.doc`, `rasm.op`             |
|  [02]   | `PaintReceipt.Drawn`/`Culled`     | `paint.marks`      | `{mark}`      | `Counter<long>`     | `gh.doc`, `disposition`         |
|  [03]   | `FrameWindow.Cost`                | `frame.window`     | `s`           | `Histogram<double>` | `gh.doc`                        |
|  [04]   | `FramePulse` seven phase spans    | `frame.phase`      | `s`           | `Histogram<double>` | `gh.doc`, `phase`               |
|  [05]   | `SessionReceipt.Latency`          | `session.ack`      | `s`           | `Histogram<double>` | `gh.doc`, `rasm.op`, `deferred` |
|  [06]   | `SessionReceipt` per command      | `session.commands` | `{command}`   | `Counter<long>`     | `gh.doc`, `rasm.op`, `deferred` |
|  [07]   | `RunPulse.InvalidCount`           | `solution.invalid` | `{parameter}` | `Histogram<long>`   | `gh.doc`                        |
|  [08]   | `RunEvidence` per completed run   | `solution.runs`    | `{run}`       | `Counter<long>`     | `gh.doc`, `culmination`         |
|  [09]   | `RunEvidence.Solved`/`Expired`    | `solution.objects` | `{object}`    | `Counter<long>`     | `gh.doc`, `disposition`         |
|  [10]   | `SolutionTrace.Pulses` per row    | `solution.pulses`  | `{pulse}`     | `Counter<long>`     | `gh.doc`, `signal`              |
|  [11]   | drain drop evidence per shed fact | `drain.dropped`    | `{fact}`      | `Counter<long>`     | `source`                        |
|  [12]   | `DispatchPulse.Elapsed`           | `dispatch.body`    | `s`           | `Histogram<double>` | `lane`, `rasm.op`               |
|  [13]   | `DispatchPulse.Breached` per lane | `dispatch.stalls`  | `{stall}`     | `Counter<long>`     | `lane`, `rasm.op`               |
|  [14]   | `BudgetBreach` per judged subject | `frame.breach`     | `{breach}`    | `Counter<long>`     | `gh.doc`, `gate`                |
|  [15]   | hook subscriber fault per point   | `hook.faults`      | `{fault}`     | `Counter<long>`     | `point`                         |

- Boundary: feeders are the receipt owners — `Canvas/paint.md` (`PaintReceipt`), `Canvas/motion.md` (`FrameWindow`, `BudgetBreach`), `Canvas/canvas.md` (`FramePulse`), `Shell/session.md` (`SessionReceipt`), `Document/solution.md` (`RunPulse`, `RunEvidence`, `SolutionTrace`), `Eto/runtime.md` (`DispatchPulse` through `EtoDispatch.Watch`), `Shell/hooks.md` (parked `IsolatedFault` evidence through the `GhHooks.Faults` cell's `Change` tap), and the `Shell/events.md` bounded drain's drop accounting; session-cache hit/miss stays off this roster because `ReportTagMetrics` surfaces it per `gh-doc` tag on the `HybridCache` EventSource.
- Growth: a new bucket policy is one kernel `Buckets` row; a per-phase or per-disposition family is one instrument with a tag axis, never sibling instruments per value.

## [04]-[PROJECTION]

- Owner: `GhEvidence` `[Union]` — the one fact family closing the folder's receipt corpus; `GhInstruments.Project` — the one total fold from evidence into tagged writes.
- Entry: `Project(GhEvidence fact)` → `Unit` — every document-scoped case carries its `DocumentToken` guid, and `GhEvidence.Document` projects `Some(document)` for those cases and `None` for process-scoped evidence. Every document-scoped write carries `gh.doc = {documentId:N}`, the same identity axis the session cache spells as its `gh-doc:{documentId:N}` tag, so metric series, cache tag metrics, and journal partitions join on one dimension.
- Law: the fold is the generated total `Switch` — a new receipt family is one union case, and the build breaks every projection site until its arm decides instrument writes or returns `unit` explicitly.
- Law: drop evidence is process-scoped — the `DropCase` write carries its `source` lane and no document tag, because a shed fact's document identity died with the fact.
- Law: document attribution is fact-owned — `PaintCase`, `WindowCase`, `PulseCase`, `SessionCase`, `ProbeCase`, `RunCase`, `TraceCase`, and `BreachCase` carry `DocumentId`; `DropCase`, `DispatchCase`, and `HookFaultCase` project no document. `SessionJournal.Append` derives its partition from the enclosing `JournalFact` projection and takes no independently supplied document argument.
- Law: per-document tag fan-out is bounded by open documents, and the app-root views own cardinality caps; the fold never re-validates a receipt — the typed owner already admitted it, and `IsValid` stays the acceptance oracle at the emitting seam.
- Boundary: span brackets, hook rails, and log emission are sibling surfaces — the kernel `TelemetrySink` owns `rasm.kernel.*`, `Shell/hooks.md` owns the veto/observe/replay points, and this fold owns only metric projection; `EtoDispatch` lane latency arrives as `DispatchCase` through the `EtoDispatch.Watch` tap and a hook fault as `HookFaultCase` minted from each `IsolatedFault` the `GhHooks.Faults` cell's `Change` tap appends (the composition root projects `fault.Point.ToString()` as the point tag), both subscribed at the composition root so neither emitting owner names an instrument.
- Packages: BCL inbox, LanguageExt.Core, Thinktecture.Runtime.Extensions, `Rasm.Csp` (`Op`), `Canvas/paint.md`/`Canvas/motion.md`/`Canvas/canvas.md`/`Document/solution.md`/`Shell/session.md` receipt owners.
- Growth: a new evidence case is one union case and one arm with its roster row; a new tag axis on an existing write is one `Tag` pair at the arm.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Rasm.Csp;
using Rasm.Domain;
using Rasm.Grasshopper.Canvas;
using Rasm.Grasshopper.Document;
using Rasm.Grasshopper.Eto;

namespace Rasm.Grasshopper.Shell;

// --- [TYPES] --------------------------------------------------------------------------------
[Union]
public abstract partial record GhEvidence {
    private GhEvidence() { }
    public sealed record PaintCase(Guid DocumentId, PaintReceipt Receipt) : GhEvidence;
    public sealed record WindowCase(Guid DocumentId, FrameWindow Window) : GhEvidence;
    public sealed record PulseCase(Guid DocumentId, FramePulse Pulse) : GhEvidence;
    public sealed record SessionCase(Guid DocumentId, SessionReceipt Receipt) : GhEvidence;
    public sealed record ProbeCase(Guid DocumentId, RunPulse Pulse) : GhEvidence;
    public sealed record RunCase(Guid DocumentId, RunEvidence Evidence) : GhEvidence;
    public sealed record TraceCase(Guid DocumentId, SolutionTrace Trace) : GhEvidence;
    public sealed record DropCase(string Source, long Dropped) : GhEvidence;
    public sealed record DispatchCase(DispatchPulse Pulse) : GhEvidence;
    public sealed record BreachCase(Guid DocumentId, BudgetBreach Breach) : GhEvidence;
    public sealed record HookFaultCase(string Point) : GhEvidence;

    public Option<Guid> Document => this.Switch(
        paintCase: static fact => Some(fact.DocumentId),
        windowCase: static fact => Some(fact.DocumentId),
        pulseCase: static fact => Some(fact.DocumentId),
        sessionCase: static fact => Some(fact.DocumentId),
        probeCase: static fact => Some(fact.DocumentId),
        runCase: static fact => Some(fact.DocumentId),
        traceCase: static fact => Some(fact.DocumentId),
        dropCase: static _ => Option<Guid>.None,
        dispatchCase: static _ => Option<Guid>.None,
        breachCase: static fact => Some(fact.DocumentId),
        hookFaultCase: static _ => Option<Guid>.None);
}

// --- [SERVICES] -----------------------------------------------------------------------------
[BoundaryAdapter]
public sealed class GhInstruments {
    private const string MeterName = "Rasm.Grasshopper";

    private readonly Meter meter;
    private readonly Histogram<double> paintSeconds;
    private readonly Counter<long> paintMarks;
    private readonly Histogram<double> frameWindow;
    private readonly Histogram<double> framePhase;
    private readonly Histogram<double> sessionAck;
    private readonly Counter<long> sessionCommands;
    private readonly Histogram<long> solutionInvalid;
    private readonly Counter<long> solutionRuns;
    private readonly Counter<long> solutionObjects;
    private readonly Counter<long> solutionPulses;
    private readonly Counter<long> drainDropped;
    private readonly Histogram<double> dispatchBody;
    private readonly Counter<long> dispatchStalls;
    private readonly Counter<long> frameBreach;
    private readonly Counter<long> hookFaults;

    private GhInstruments(Meter meter) {
        this.meter = meter;
        paintSeconds = Buckets.Advised(meter, "rasm.grasshopper.paint.duration", unit: "s",
            text: "Paint plan execution wall time per receipt.", bounds: Buckets.CanvasFrameSeconds);
        paintMarks = meter.CreateCounter<long>(name: "rasm.grasshopper.paint.marks", unit: "{mark}",
            description: "Paint marks by disposition, drawn against culled.");
        frameWindow = Buckets.Advised(meter, "rasm.grasshopper.frame.window", unit: "s",
            text: "Motion draw-window cost per sampled frame.", bounds: Buckets.CanvasFrameSeconds);
        framePhase = Buckets.Advised(meter, "rasm.grasshopper.frame.phase", unit: "s",
            text: "Canvas frame cost per paint phase.", bounds: Buckets.CanvasFrameSeconds);
        sessionAck = Buckets.Advised(meter, "rasm.grasshopper.session.ack", unit: "s",
            text: "Session command acknowledgement latency.", bounds: Buckets.AckSeconds);
        sessionCommands = meter.CreateCounter<long>(name: "rasm.grasshopper.session.commands", unit: "{command}",
            description: "Session commands by operation and posture.");
        solutionInvalid = meter.CreateHistogram<long>(name: "rasm.grasshopper.solution.invalid", unit: "{parameter}",
            description: "Invalid parameter count per solution probe.");
        solutionRuns = meter.CreateCounter<long>(name: "rasm.grasshopper.solution.runs", unit: "{run}",
            description: "Completed solution runs by culmination.");
        solutionObjects = meter.CreateCounter<long>(name: "rasm.grasshopper.solution.objects", unit: "{object}",
            description: "Solution objects by disposition, solved against expired.");
        solutionPulses = meter.CreateCounter<long>(name: "rasm.grasshopper.solution.pulses", unit: "{pulse}",
            description: "Solution lifecycle pulses by signal ordinal.");
        drainDropped = meter.CreateCounter<long>(name: "rasm.grasshopper.drain.dropped", unit: "{fact}",
            description: "Evidence facts shed by the bounded drain per source lane.");
        dispatchBody = Buckets.Advised(meter, "rasm.grasshopper.dispatch.body", unit: "s",
            text: "UI-thread marshal body wall time per lane.", bounds: Buckets.AckSeconds);
        dispatchStalls = meter.CreateCounter<long>(name: "rasm.grasshopper.dispatch.stalls", unit: "{stall}",
            description: "Dispatch bodies breaching their lane budget.");
        frameBreach = meter.CreateCounter<long>(name: "rasm.grasshopper.frame.breach", unit: "{breach}",
            description: "Frame-budget violations judged by the budget gate.");
        hookFaults = meter.CreateCounter<long>(name: "rasm.grasshopper.hook.faults", unit: "{fault}",
            description: "Contained hook-subscriber faults per point.");
    }

    internal static GhInstruments Of(IMeterFactory factory, string plugin, Option<string> version) =>
        new(meter: factory.Create(new MeterOptions(MeterName) {
            Version = version.MatchUnsafe(Some: static held => held, None: static () => null),
            Tags = [new KeyValuePair<string, object?>("gh.plugin", plugin)],
        }));

    public Unit Project(GhEvidence fact) =>
        fact.Switch<GhInstruments, Unit>(
            state: this,
            paintCase: static (spine, evidence) => spine.Painted(doc: evidence.DocumentId.ToString("N"), receipt: evidence.Receipt),
            windowCase: static (spine, evidence) => spine.Windowed(doc: evidence.DocumentId.ToString("N"), window: evidence.Window),
            pulseCase: static (spine, evidence) => spine.Pulsed(doc: evidence.DocumentId.ToString("N"), pulse: evidence.Pulse),
            sessionCase: static (spine, evidence) => spine.Settled(doc: evidence.DocumentId.ToString("N"), receipt: evidence.Receipt),
            probeCase: static (spine, evidence) => spine.Probed(doc: evidence.DocumentId.ToString("N"), pulse: evidence.Pulse),
            runCase: static (spine, evidence) => spine.Ran(doc: evidence.DocumentId.ToString("N"), evidence: evidence.Evidence),
            traceCase: static (spine, evidence) => spine.Chronicled(doc: evidence.DocumentId.ToString("N"), trace: evidence.Trace),
            dropCase: static (spine, evidence) => spine.Dropped(source: evidence.Source, dropped: evidence.Dropped),
            dispatchCase: static (spine, evidence) => spine.Marshalled(pulse: evidence.Pulse),
            breachCase: static (spine, evidence) => spine.Breached(doc: evidence.DocumentId.ToString("N"), breach: evidence.Breach),
            hookFaultCase: static (spine, evidence) => spine.Hooked(point: evidence.Point));

    private static KeyValuePair<string, object?> Doc(string doc) => new("gh.doc", doc);

    private static KeyValuePair<string, object?> Tag(string key, object? value) => new(key, value);

    // Statement seam: tagged instrument writes are void host calls; each helper sequences its writes and returns unit.
    private Unit Painted(string doc, PaintReceipt receipt) {
        paintSeconds.Record(receipt.Latency.TotalSeconds, Doc(doc), Tag("rasm.op", receipt.Operation.ToString()));
        paintMarks.Add(receipt.Drawn, Doc(doc), Tag("disposition", "drawn"));
        paintMarks.Add(receipt.Culled, Doc(doc), Tag("disposition", "culled"));
        return unit;
    }

    private Unit Windowed(string doc, FrameWindow window) {
        frameWindow.Record(window.Cost.TotalSeconds, Doc(doc));
        return unit;
    }

    private Unit Pulsed(string doc, FramePulse pulse) {
        Seq<(string Phase, TimeSpan Cost)> rows = [
            ("grid", pulse.Grid), ("wire", pulse.Wire), ("text", pulse.Text), ("icon", pulse.Icon),
            ("shape", pulse.Shape), ("layout", pulse.Layout), ("full", pulse.FullFrame)];
        return ignore(rows.Fold((Spine: this, Doc: doc), static (state, row) => {
            state.Spine.framePhase.Record(row.Cost.TotalSeconds, Doc(state.Doc), Tag("phase", row.Phase));
            return state;
        }));
    }

    private Unit Settled(string doc, SessionReceipt receipt) {
        sessionAck.Record(receipt.Latency.TotalSeconds, Doc(doc), Tag("rasm.op", receipt.Operation.ToString()), Tag("deferred", receipt.Deferred));
        sessionCommands.Add(1L, Doc(doc), Tag("rasm.op", receipt.Operation.ToString()), Tag("deferred", receipt.Deferred));
        return unit;
    }

    private Unit Probed(string doc, RunPulse pulse) {
        solutionInvalid.Record(pulse.InvalidCount, Doc(doc));
        return unit;
    }

    private Unit Ran(string doc, RunEvidence evidence) {
        solutionRuns.Add(1L, Doc(doc), Tag("culmination", evidence.Culmination));
        solutionObjects.Add(evidence.Solved, Doc(doc), Tag("disposition", "solved"));
        solutionObjects.Add(evidence.Expired, Doc(doc), Tag("disposition", "expired"));
        return unit;
    }

    private Unit Chronicled(string doc, SolutionTrace trace) =>
        ignore(trace.Pulses.Fold((Spine: this, Doc: doc), static (state, row) => {
            state.Spine.solutionPulses.Add(1L, Doc(state.Doc), Tag("signal", row.Signal.Key));
            return state;
        }));

    private Unit Dropped(string source, long dropped) {
        drainDropped.Add(dropped, Tag("source", source));
        return unit;
    }

    private Unit Marshalled(DispatchPulse pulse) {
        dispatchBody.Record(pulse.Elapsed.TotalSeconds, Tag("lane", pulse.Lane.Key), Tag("rasm.op", pulse.Operation.ToString()));
        Op.SideWhen(condition: pulse.Breached, action: () =>
            dispatchStalls.Add(1L, Tag("lane", pulse.Lane.Key), Tag("rasm.op", pulse.Operation.ToString())));
        return unit;
    }

    private Unit Breached(string doc, BudgetBreach breach) {
        frameBreach.Add(1L, Doc(doc), Tag("gate", breach.Row.Key));
        return unit;
    }

    private Unit Hooked(string point) {
        hookFaults.Add(1L, Tag("point", point));
        return unit;
    }
}

[BoundaryAdapter]
public static class GhLog {
    private static readonly Atom<(long Seat, ILoggerFactory Factory)> Cell =
        Atom((Seat: 0L, Factory: (ILoggerFactory)NullLoggerFactory.Instance));
    private static long nextSeat;

    public static ILogger For(string category) => Cell.Value.Factory.CreateLogger(categoryName: category);

    // first-mount-wins seat custody: a free seat commits the factory and hands back the seat token; a held seat keeps
    // its live binding untouched and returns None, so a later capsule never overwrites or disables an earlier one.
    internal static Option<long> Bind(ILoggerFactory factory) {
        long seat = Interlocked.Increment(location: ref nextSeat);
        return Cell.Swap(current => current.Seat == 0L ? (Seat: seat, Factory: factory) : current).Seat == seat
            ? Some(seat)
            : Option<long>.None;
    }

    // exact-owner restore: only the token that bound releases the seat back to the null sink; a stale dispose no-ops.
    internal static Unit Unbind(long seat) => ignore(Cell.Swap(current =>
        current.Seat == seat ? (Seat: 0L, Factory: (ILoggerFactory)NullLoggerFactory.Instance) : current));
}

[BoundaryAdapter]
public sealed class GhTelemetry : IDisposable {
    private readonly Option<long> seat;

    private GhTelemetry(GhInstruments instruments, ILoggerFactory logs, Option<long> seat) =>
        (Instruments, Logs, this.seat) = (instruments, logs, seat);

    public GhInstruments Instruments { get; }

    public ILoggerFactory Logs { get; }

    public static Fin<GhTelemetry> Of(
        IMeterFactory factory, string plugin,
        Option<ILoggerFactory> logs = default, Option<string> version = default, Op? key = null) {
        Op op = key.OrDefault();
        return from owner in op.Need(factory)
               from identity in guard(!string.IsNullOrWhiteSpace(plugin), op.InvalidInput()).ToFin()
                   .Map(_ => plugin.Trim())
               from telemetry in op.Catch(body: () => {
                   // only a SUPPLIED factory contends for the ambient seat — an Option-defaulted null sink never binds,
                   // so a logger-less capsule cannot displace a live binding; the held seat is the disposal token.
                   Option<long> seat = logs.Bind(supplied => GhLog.Bind(factory: supplied));
                   return Fin.Succ(new GhTelemetry(
                       instruments: GhInstruments.Of(factory: owner, plugin: identity, version: version),
                       logs: logs.IfNone(NullLoggerFactory.Instance),
                       seat: seat));
               })
               select telemetry;
    }

    public void Dispose() {
        ignore(seat.Map(GhLog.Unbind));
    }
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Project boundary receipts into attributed instruments
    accDescr: Receipt families from paint, motion, session, solution, dispatch, hooks, and drain owners enter one evidence union; a total fold writes tagged instruments on the meter minted through the injected per-ALC factory, the app root flushes providers on load-context unload, and cache tag metrics ride the HybridCache EventSource beside the fold.
    PaintR["PaintReceipt · FrameWindow · FramePulse · BudgetBreach"] --> Union["GhEvidence"]
    SessionR["SessionReceipt"] --> Union
    SolutionR["RunPulse · RunEvidence · SolutionTrace"] --> Union
    DrainR["drain drop evidence"] --> Union
    DispatchR["DispatchPulse tap"] --> Union
    HookR["hook fault evidence"] --> Union
    Union -->|"total Switch"| Fan["GhInstruments.Project"]
    Fan -->|"gh.doc · gh.plugin tags"| MeterNode[("Rasm.Grasshopper meter")]
    MeterNode -->|"IMeterFactory custody"| Host["per-ALC provider · app root"]
    Host -->|"ForceFlush on ALC unload"| Egress["OTLP egress"]
    Host -.->|"ReportTagMetrics"| CacheES["HybridCache EventSource · gh-doc"]
```

## [05]-[DENSITY_BAR]

| [INDEX] | [CONCERN]           | [OWNER]         | [RAIL]                                   | [CASES] |
| :-----: | :------------------ | :-------------- | :--------------------------------------- | :-----: |
|  [01]   | receipt ingress     | `GhEvidence`    | closed union → one total projection fold |   11    |
|  [02]   | instrument roster   | `GhInstruments` | `Project(GhEvidence) → Unit`             |   15    |
|  [03]   | telemetry admission | `GhTelemetry`   | `Of → Fin<GhTelemetry>`; logger inverse  |    1    |
|  [04]   | ambient log seam    | `GhLog`         | `For(category) → ILogger`                |    1    |

`Op`, `Lease<T>`, `DocumentToken`, and every receipt owner are composed upstream; the app root owns `IMeterFactory` custody, provider binding, views, and OTLP egress — nothing on this page names an exporter.

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
