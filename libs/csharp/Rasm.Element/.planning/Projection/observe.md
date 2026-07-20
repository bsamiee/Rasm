# [ELEMENT_OBSERVE]

`Rasm.Element` observability is one `ElementHookRail` typed graph-fact tap every seam lifecycle fact flows through, and one `GraphInstrument` meter-and-span projection subscribed off it — telemetry-as-tap, app-neutral, zero OpenTelemetry reference at seam altitude. `HookPoint` rows key the roster `rasm.element.<domain>.<point>` (`graph.delta-applied`, `graph.frozen`, `graph.baked`, `projection.assembled`, `projection.finding`, `wire.decoded`), each fact a typed `ElementFact` case carrying the kernel `Op` key, the graph or delta `ContentAddress`, and the point payload; the rail is a VALUE the app composition root mints (`ElementHookRail.Of`, the `Projection/projection#PROJECTION_CONTRACT` `ProjectionSuite.Of` minting precedent — never a process-global), so two apps composing the seam never fight over hook points. Modality is a row column: `graph.delta-applied` is the one `Vetoable` row (an app write-freeze or review-lock gate refuses a delta on the emitter's `Fin<T>` rail BEFORE the structural fold runs), the five remaining rows observe; a throwing or failing subscriber converts through the shielded tap funnel onto `Projection/fault#FAULT_BAND` `ElementFault.ProjectionFailed` parked in the rail's `TapFaults` evidence cell — subscriber failure never breaks the emitter, the band-2500 isolation rail. Graph code stays emit-free: `ElementTap` is the seam-owned decoration capability (the apps own the wiring, the `[ASSEMBLE_CAPABILITY]` split) composing each fire around the real seam entrypoint — `GraphDelta.AdmitOnto`, `ElementGraph.Bake`, `ProjectionAssembly.Assemble`, `ElementWire.DecodeGraph`/`DecodeDelta` — so no emit-call scatters into `Graph/element`, `Graph/delta`, or `Graph/wire`.

`GraphInstrument` realizes the estate wire law at library altitude with BCL surfaces alone: instruments mint through an injected `IMeterFactory` (per-app and per-ALC neutral — the provider owns instrument lifetime), the `ElementInstruments.Rows` table is the CLOSED `rasm.element.<domain>.<measure>` roster (name, UCUM unit, bounded dimensions, bind delegate — never an ad-hoc emit call), histograms ship `InstrumentAdvice<T>` explicit-bucket boundaries as the fallback beneath the composition root's base2-exponential default, and the `ActivitySource` span family (`rasm.element.graph.bake`, `rasm.element.projection.assemble`, `rasm.element.wire.decode`) brackets the three heavy folds through `Traced` so span-profile correlation and trace exemplars join at the app tier. Dimensions bound to the seam's closed vocabularies — `ConstraintSeverity`, `Discipline`, `AssessmentOutcome`, `WireKind` — so cost attribution joins downstream without cardinality blowups; the opaque `AnalysisRoute` token rides the typed fact and never a tag. Rasm.AppHost's `InstrumentFan` Element arm mounts exactly this table and this tap at its composition root; the page composes `System.Diagnostics.Metrics` and `System.Diagnostics.ActivitySource` (both `System.Diagnostics.DiagnosticSource.dll`, shared framework — no manifest row), and a malformed mint rails `ElementFault`.

## [01]-[INDEX]

- [02]-[HOOK_RAIL]: the `HookPoint` six-row point vocabulary with its `Vetoable` modality column, the `ElementFact` `[Union]` typed fact family with the one polymorphic `Of` mint and the `Point` projection, the `HookGate`/`HookTap` subscriber rows, the `ElementHookRail` minted registry with the veto fold, the shielded observe fan, and the `TapFaults` isolation cell, and the `ElementTap` seam-owned decoration capability over the graph, projection, and wire entrypoints.
- [03]-[INSTRUMENT_PROJECTION]: the `InstrumentRow` bind-row shape, the `ElementInstruments` closed instrument table (UCUM units, bucket advice, canonical name and tag-key constants, span names), and the `GraphInstrument` mounted capsule — `Mount` off the injected `IMeterFactory`, `AsTap` the hook-rail subscription projecting every fact into instrument writes, and `Traced` the `ActivitySource` span bracket.

## [02]-[HOOK_RAIL]

- Owner: `HookPoint` the `[SmartEnum<string>]` point vocabulary keyed `rasm.element.<domain>.<point>` (the estate four-segment lowercase grammar) with the `Vetoable` modality column; `ElementFact` the `[Union]` typed fact family over the shared `Op Key` base; `AssessmentTouch` the per-delta assessment census row; `WireKind` the two-row decode-kind vocabulary; `HookGate`/`HookTap` the subscriber rows; `ElementHookRail` the minted registry; `ElementTap` the decoration capability.
- Cases: `DeltaApplied` (delta content key, node/edge touch counts, header presence, the typed `AssessmentTouch` census — fired at the admission seam, the one `Vetoable` point) · `Frozen` (snapshot `ContentAddress.OfGraph` key, node/edge population) · `Baked` (element root, fold `Duration`) · `Assembled` (merged-delta content key — the same derivation the `Rasm.Persistence` event dedup reads — projector count, delta magnitude, finding count, pipeline `Duration`) · `Graded` (one `ConstraintFinding` — severity row, kernel `Category`, replayable `KeyOf` content key, waived flag) · `Decoded` (`WireKind`, `Option<long>` payload bytes — `None` for a non-seekable stream, never a fabricated length — decoded magnitude, `Duration`); the closed lifecycle-fact family.
- Entry: `ElementFact.Of` is ONE polymorphic mint discriminating on input shape — `(key, delta, tolerance)` → `DeltaApplied` (content key over `GraphDelta.ToCanonicalBytes`, census folded from the delta's `Node.Assessment` touches), `(key, graph)` → `Frozen`, `(key, root, elapsed)` → `Baked`, `(key, receipt, projectors, elapsed)` → `Assembled`, `(key, finding)` → `Graded`, `(key, kind, bytes, nodes, edges, elapsed)` → `Decoded`; `ElementHookRail.Of(key, gates, taps, clock)` mints the registry value once at the app root, `Fin<T>` railing `ElementFault.ValueRejected` on a gate row targeting a non-`Vetoable` point, the clock defaulting `TimeProvider.System`; `Fire(fact)` folds the point's veto gates left through `Bind` (first refusal short-circuits on the emitter's rail, a transform threads forward), then fans every tap through the shielded funnel and returns the admitted fact; `TapFaults` reads the parked subscriber-fault evidence; `ElementTap.Admitted`/`Baked`/`Assembled`/`DecodedGraph`/`DecodedDelta` are the composed decorations — one per instrumented seam entrypoint, each returning exactly the seam owner's rail type.
- Auto: `Fire` on an observe-only point degenerates to the tap fan (no gate rows match); a faulted tap converts through `Try.lift(() => tap.Observe(fact)).Run()` onto `ElementFault.ProjectionFailed(fact.Key, "<hook-tap-faulted:…>")` swapped into the `TapFaults` atom — the emitter's result untouched; `ElementTap.Admitted` fires `DeltaApplied` BEFORE `AdmitOnto` (a veto refusal means the delta never lands) and `Frozen` after the freeze; `Assembled` times the pipeline through the one `Timed` kernel then fires one `Graded` fact per receipt finding; `Point` projects each case onto its `HookPoint` row through the generated `Map` over the preallocated singletons.
- Receipt: a fired `ElementFact` IS the evidence event — the emitter's own rail already carries the outcome, so the rail mints nothing beside it; `TapFaults` is the subscriber-failure evidence a health panel drains; a replay/audit consumer, an AppUi live-model listener, and the `[03]` instrument projection all ride the same tap rows, so observability subscribes to facts and never produces them.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`/`[Union]` + the generated `Switch`/`Map`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Atom`/`Try`), NodaTime (`Duration`), `Rasm` (the kernel `Op` + `FaultExtensions.Category`), BCL (`TimeProvider` the injected monotonic clock).
- Growth: a new lifecycle fact is one `HookPoint` row and one `ElementFact` case — the generated dispatch breaks every projection (the `[03]` instrument tap included) loudly at compile time; a new subscriber is one `HookTap`/`HookGate` row at the app root's mint; a new delivery semantics is one modality column value; never a per-point registry sibling, never a process-global rail, and never an emit call inside a graph page.
- Boundary: the rail is a sealed CLASS, not a record — a `with` copy aliasing the `TapFaults` atom is compile-impossible (the `Graph/element#ELEMENT_GRAPH` cache-aliasing discipline); a veto gate REJECTS or annotates the FACT — the applied delta stays the emitter's own value, so a gate is app policy ABOVE the structural law `AdmitOnto` runs, never a replacement for it; facts fire on the SUCCESS path — a failed fold already rails its typed `ElementFault`, and double-reporting the failure through a fact mints a second truth; the `DeltaApplied`/`Assembled` content keys derive through the SAME `GraphDelta.ToCanonicalBytes` projection the Persistence event dedup keys on, and `Frozen` pays the `ContentAddress.OfGraph` fold — the decoration altitude is where that cost is accepted, never a hidden graph-page charge; tap fault conversion routes the existing `ProjectionFailed` arm (a captured foreign-subscriber exception at the seam boundary — the `Projection/fault#FAULT_BAND` capture class), never a seventh arm or a second fault family.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Assessment;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element.Projection;

// --- [TYPES] ------------------------------------------------------------------------------
// Point roster keyed rasm.element.<domain>.<point> — the estate four-segment lowercase grammar hook ids share
// with instrumentation scopes and metric names. Vetoable is the modality column: the delta admission point admits
// synchronous transform-or-reject gates on the emitter's rail; every other point is observe-only, the tap fan
// asynchronous-safe by isolation rather than by scheduling (the seam owns no bus).
[SmartEnum<string>]
public sealed partial class HookPoint {
 public static readonly HookPoint DeltaApplied = new("rasm.element.graph.delta-applied", vetoable: true);
 public static readonly HookPoint Frozen = new("rasm.element.graph.frozen", vetoable: false);
 public static readonly HookPoint Baked = new("rasm.element.graph.baked", vetoable: false);
 public static readonly HookPoint Assembled = new("rasm.element.projection.assembled", vetoable: false);
 public static readonly HookPoint Finding = new("rasm.element.projection.finding", vetoable: false);
 public static readonly HookPoint WireDecoded = new("rasm.element.wire.decoded", vetoable: false);

 public bool Vetoable { get; }
}

// Decode-kind discriminant the wire fact and the wire instruments dimension on — the two typed decode legs.
[SmartEnum<string>]
public sealed partial class WireKind {
 public static readonly WireKind Snapshot = new("snapshot");
 public static readonly WireKind Delta = new("delta");
}

// --- [MODELS] -----------------------------------------------------------------------------
// One assessment node touched by a delta — the typed census the DeltaApplied fact carries so outcome instruments
// and route-cost consumers read evidence off the fact, never a re-scan of the delta. Route rides TYPED here and
// never becomes a metric dimension (opaque Compute-owned token, unbounded — the [03] cardinality law).
public readonly record struct AssessmentTouch(Discipline Discipline, AnalysisRoute Route, AssessmentOutcome Outcome);

// Closed lifecycle-fact family — each case carries the kernel Op key (the shared base column), the graph or
// delta ContentAddress where the point owns one, and the point payload. ONE polymorphic Of mints every case by
// input shape; Point projects the owning row through the generated Map over the preallocated HookPoint singletons.
[Union]
public abstract partial record ElementFact {
 private ElementFact(Op key) { Key = key; }

 public Op Key { get; }

 public sealed record DeltaApplied(Op Key, ContentAddress Delta, int Nodes, int Edges, bool HeaderEstablished, Seq<AssessmentTouch> Assessments) : ElementFact(Key);
 public sealed record Frozen(Op Key, ContentAddress Snapshot, int Nodes, int Edges) : ElementFact(Key);
 public sealed record Baked(Op Key, NodeId Root, Duration Elapsed) : ElementFact(Key);
 public sealed record Assembled(Op Key, ContentAddress Delta, int Projectors, int Nodes, int Edges, int Findings, Duration Elapsed) : ElementFact(Key);
 public sealed record Graded(Op Key, ConstraintSeverity Severity, string Category, UInt128 FindingKey, bool Waived) : ElementFact(Key);
 public sealed record Decoded(Op Key, WireKind Kind, Option<long> Bytes, int Nodes, int Edges, Duration Elapsed) : ElementFact(Key);

 public HookPoint Point => Map(
  deltaApplied: HookPoint.DeltaApplied,
  frozen: HookPoint.Frozen,
  baked: HookPoint.Baked,
  assembled: HookPoint.Assembled,
  graded: HookPoint.Finding,
  decoded: HookPoint.WireDecoded);

 // Delta fact: the content key is the SAME GraphDelta.ToCanonicalBytes derivation the Rasm.Persistence event dedup
 // keys on — one projection, two consumers, never a second spelling.
 public static ElementFact Of(Op key, GraphDelta delta, double tolerance) => new DeltaApplied(
  key, ContentAddress.Of(delta.ToCanonicalBytes(tolerance).Span), delta.NodeCount, delta.EdgeCount, delta.Header.IsSome, Touches(delta));

 // Frozen fact: pays the full OfGraph snapshot fold — accepted at the decoration altitude, never a graph-page charge.
 public static ElementFact Of(Op key, ElementGraph graph) => new Frozen(
  key, ContentAddress.OfGraph(graph), graph.Nodes.Count, graph.Edges.Length);

 public static ElementFact Of(Op key, NodeId root, Duration elapsed) => new Baked(key, root, elapsed);

 public static ElementFact Of(Op key, AssemblyReceipt receipt, int projectors, Duration elapsed) => new Assembled(
  key, ContentAddress.Of(receipt.Delta.ToCanonicalBytes(receipt.Graph.Header.Tolerance).Span),
  projectors, receipt.Delta.NodeCount, receipt.Delta.EdgeCount, receipt.Findings.Count, elapsed);

 public static ElementFact Of(Op key, ConstraintFinding finding) => new Graded(
  key, finding.Severity, finding.Violation.Category(), finding.Key, finding.Waived);

 public static ElementFact Of(Op key, WireKind kind, Option<long> bytes, int nodes, int edges, Duration elapsed) =>
  new Decoded(key, kind, bytes, nodes, edges, elapsed);

 // Per-delta assessment census: added and revised-after Assessment nodes only — a removal carries no outcome.
 static Seq<AssessmentTouch> Touches(GraphDelta delta) =>
  (delta.AddedNodes + delta.RevisedNodes.Map(static r => r.After))
   .Choose(static n => n is Node.Assessment a
    ? Some(new AssessmentTouch(a.Payload.Discipline, a.Payload.Route, a.Payload.Outcome))
    : None);
}

// --- [SERVICES] ---------------------------------------------------------------------------
// Subscriber rows: a gate is the synchronous transform-or-reject a Vetoable point folds on the emitter's rail; a
// tap is the shielded observer, its Name the fault-attribution discriminant the TapFaults detail carries.
public sealed record HookGate(HookPoint Point, Func<ElementFact, Fin<ElementFact>> Transform);

public sealed record HookTap(string Name, Func<ElementFact, Unit> Observe);

// Minted registry — a sealed CLASS so no `with` copy can alias the TapFaults atom (the ElementGraph cache
// discipline); the app composition root builds it ONCE from its gate and tap rows (the ProjectionSuite.Of mint
// precedent) and every decorated seam call reuses it. Clock is the injected monotonic source the Timed kernel reads.
public sealed class ElementHookRail {
 readonly Atom<Seq<ElementFault>> faults = Atom(Seq<ElementFault>());

 ElementHookRail(Seq<HookGate> gates, Seq<HookTap> taps, TimeProvider clock) =>
  (Gates, Taps, Clock) = (gates, taps, clock);

 public Seq<HookGate> Gates { get; }
 public Seq<HookTap> Taps { get; }
 public TimeProvider Clock { get; }

 // Parked subscriber-fault evidence — a health panel or drain sweep reads it; the emitter never sees it.
 public Seq<ElementFault> TapFaults => faults.Value;

 public static Fin<ElementHookRail> Of(Op key, Seq<HookGate> gates = default, Seq<HookTap> taps = default, Option<TimeProvider> clock = default) =>
  gates.Find(static gate => !gate.Point.Vetoable).Match(
   Some: gate => Fin.Fail<ElementHookRail>(ElementFault.ValueRejected(key, $"<hook-gate-on-observe-point:{gate.Point.Key}>")),
   None: () => Fin.Succ(new ElementHookRail(gates, taps, clock.IfNone(TimeProvider.System))));

 // Veto fold first (registration order, first refusal short-circuits on the emitter's rail, a transform threads
 // forward), then the shielded observe fan over the admitted fact — eager Iter, never a lazy Map the fan drops.
 public Fin<ElementFact> Fire(ElementFact fact) =>
  Gates.Filter(gate => gate.Point == fact.Point)
   .Fold(Fin.Succ(fact), static (state, gate) => state.Bind(gate.Transform))
   .Map(admitted => (Taps.Iter(tap => Shielded(tap, admitted)), admitted).Item2);

 // A throwing or failing tap converts onto the existing ProjectionFailed capture arm — foreign subscriber code
 // at the seam boundary, the Try.lift funnel preserving the raw message — parked in the evidence cell; the
 // emitter's Fire result is untouched, so subscriber failure is evidence, never a broken graph operation.
 Unit Shielded(HookTap tap, ElementFact fact) =>
  Try.lift(() => tap.Observe(fact)).Run().Match(
   Succ: static _ => unit,
   Fail: error => ignore(faults.Swap(held => held.Add(
    ElementFault.ProjectionFailed(fact.Key, $"<hook-tap-faulted:{tap.Name}:{fact.Point.Key}:{error.Message}>")))));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// Seam-owned decoration capability (the apps own the wiring — the ASSEMBLE_CAPABILITY split): each entry
// composes fire + the REAL seam entrypoint and returns exactly that owner's rail type, so graph pages stay
// emit-free and an app threads `instrument.Traced(ElementInstruments.BakeSpan, () => ElementTap.Baked(...))`
// to bracket the same call with its span. Facts fire on success alone — a failed fold already rails its fault.
public static class ElementTap {
 // DeltaApplied fires BEFORE AdmitOnto (a veto refusal aborts the application — the delta never lands), Frozen
 // after the freeze with the snapshot key; the structural law itself stays AdmitOnto's own.
 public static Fin<(ElementGraph Graph, GraphDelta Delta)> Admitted(ElementHookRail rail, GraphDelta delta, ElementGraph seed, Op key) =>
  rail.Fire(ElementFact.Of(key, delta, seed.Header.Tolerance))
   .Bind(_ => delta.AdmitOnto(seed, key))
   .Map(step => (ignore(rail.Fire(ElementFact.Of(key, step.Graph))), step).Item2);

 public static Fin<Element> Baked(ElementHookRail rail, ElementGraph graph, NodeId root, Op key) =>
  Timed(rail, () => graph.Bake(root, key), (_, elapsed) => ElementFact.Of(key, root, elapsed));

 // Assembled times the whole pipeline, then fires one Graded fact per receipt finding — warnings and waived
 // deviations included, the same evidence stream AssemblyReceipt.Findings persists.
 public static Fin<AssemblyReceipt> Assembled(ElementHookRail rail, ProjectionSuite suite, ElementGraph seed, ProjectionContext ctx) =>
  Timed(rail, () => ProjectionAssembly.Assemble(suite, seed, ctx),
    (receipt, elapsed) => ElementFact.Of(ctx.Key, receipt, suite.Projectors.Count, elapsed))
   .Map(receipt => (ignore(receipt.Findings.Iter(finding => rail.Fire(ElementFact.Of(ctx.Key, finding)))), receipt).Item2);

 public static Fin<ElementGraph> DecodedGraph(ElementHookRail rail, Stream payload, WireLimits limits, Op key) =>
  Timed(rail, () => ElementWire.DecodeGraph(payload, limits, key),
   (graph, elapsed) => ElementFact.Of(key, WireKind.Snapshot, Length(payload), graph.Nodes.Count, graph.Edges.Length, elapsed));

 public static Fin<GraphDelta> DecodedDelta(ElementHookRail rail, Stream payload, WireLimits limits, Op key) =>
  Timed(rail, () => ElementWire.DecodeDelta(payload, limits, key),
   (delta, elapsed) => ElementFact.Of(key, WireKind.Delta, Length(payload), delta.NodeCount, delta.EdgeCount, elapsed));

 // ONE monotonic timing kernel every timed decoration composes — the timestamp read precedes the body, the named
 // statement seam confined here; elapsed derives from the rail's injected TimeProvider, never a wall-clock diff.
 static Fin<T> Timed<T>(ElementHookRail rail, Func<Fin<T>> body, Func<T, Duration, ElementFact> fact) {
  long start = rail.Clock.GetTimestamp();
  return body().Map(value =>
   (ignore(rail.Fire(fact(value, Duration.FromTimeSpan(rail.Clock.GetElapsedTime(start))))), value).Item2);
 }

 // Typed absence over a non-seekable stream — a fabricated length is false evidence.
 static Option<long> Length(Stream payload) => payload.CanSeek ? Some(payload.Length) : None;
}
```

## [03]-[INSTRUMENT_PROJECTION]

- Owner: `InstrumentRow` the bind-row shape (name, UCUM unit, description, closed dimension vocabulary, bind delegate — the instrument KIND rides the delegate's created type); `ElementInstruments` the closed `rasm.element.*` roster with its bucket-advice policy rows, canonical name and tag-key constants, span names, and the `Counted`/`Advised(bounds)` bind factories every row derives from; `GraphInstrument` the mounted capsule the `IMeterFactory` injection seam produces.
- Entry: `GraphInstrument.Mount(IMeterFactory factory, string version)` mints ONE `Meter` through `factory.Create(new MeterOptions("Rasm.Element") { Version = version })` — the instrumentation scope is the emitting package id, version-stamped, identical across meter and `ActivitySource` — binds every `ElementInstruments.Rows` row over it into the frozen by-name set, and opens the `ActivitySource`; `AsTap()` returns the one `HookTap` the app root passes into `ElementHookRail.Of`, its body the generated total `Switch` over `ElementFact` projecting every fact into tagged instrument writes; `Traced(name, body)` brackets a rail-valued fold in an `ActivityKind.Internal` span, stamping `ActivityStatusCode.Error` with the typed fault message on the fail side.
- Auto: `DeltaApplied` adds the two delta-magnitude counters and one `rasm.element.assessment.outcomes` count per census touch (discipline and outcome dimensions — both closed rows); `Frozen` records the snapshot node/edge population histograms; `Baked`/`Assembled` record the duration histograms; `Graded` counts findings under severity and waived; `Decoded` records duration and — when the payload length is known — bytes, both under the `WireKind` dimension; instrument identity de-duplicates by name inside the one meter, so name, unit, and description are declaration facts the row carries once.
- Receipt: none — the projection is a pure fold of the fact tap; a metric minted beside it is a second truth, and every operational dashboard reads the exported stream, never a seam cell.
- Packages: BCL `System.Diagnostics.Metrics` (`IMeterFactory`/`MeterOptions`/`Meter`/`Counter<T>`/`Histogram<T>`/`InstrumentAdvice<T>` — `libs/csharp/.api/api-diagnostics-metrics.md`) and `System.Diagnostics` (`ActivitySource`/`Activity` — the same shared-framework assembly, no manifest row), Thinktecture.Runtime.Extensions (the generated fact `Switch`), LanguageExt.Core.
- Growth: a new metric is one `ElementInstruments` row and one write in the owning `Switch` arm — a new fact case breaks the tap at compile time, so an unprojected fact is a build error, never a silent gap; a new bucket policy is one advice array row; a new span is one name constant and one `Traced` call site at the app root; never an inline `new Meter(...)`, never a create or write call outside this fence, and never a numeric value as a tag.
- Boundary: this fence IS the package's declared telemetry-spine — the one place create and write calls live (`api-diagnostics-metrics.md` `[LOCAL_ADMISSION]`); every dimension is a CLOSED seam vocabulary (`ConstraintSeverity` two rows, `WireKind` two, `Discipline` sixteen, `AssessmentOutcome` eight, `waived` a bool) — the opaque `AnalysisRoute` and the `NodeId`/`ContentAddress` identities never become tags, because an unbounded token is a series-per-value blowup and identity joins ride exemplars and receipts at the app tier; the SDK — provider, exporter, views, exemplar filter, base2-exponential default — lives at the composition root (`Rasm.AppHost` `Observability/instruments` mounts this table through its `InstrumentFan` Element arm), the advice rows the fallback a backend without exponential histograms reads; level-shaped facts have no row here because the seam holds no live cells — population is recorded per freeze event, a level gauge being the app tier's fold if one is wanted; the `Count`/`Record` pattern-probe bodies and the `using`-scoped span bracket are the named platform statement seam.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// One instrument declaration row: dotted rasm.element.* name, UCUM unit (never a name-encoded unit or _total
// suffix), description, the CLOSED per-instrument tag-key vocabulary, and the bind delegate whose created type
// IS the instrument kind; the AppHost InstrumentFan Element arm registers exactly this table.
public sealed record InstrumentRow(string Name, string Unit, string Text, Seq<string> Dimensions,
 Func<Meter, string, string, string, Instrument> Bind);

// --- [TABLES] -----------------------------------------------------------------------------
// Closed roster — instrument-name and tag-key constants are the canonical spellings both the rows and the
// projection arms compose (one owner, zero drift between a declared dimension and its write-site tag), bucket
// arrays the explicit-advice fallback beneath the composition root's base2-exponential default, and the two
// bind factories the whole table derives from — Counted the counter method group, Advised(bounds) the
// advice-bearing histogram binder parameterized by its bucket policy row.
public static class ElementInstruments {
 public const string Scope = "Rasm.Element";

 public const string DeltaNodes = "rasm.element.graph.delta.nodes";
 public const string DeltaEdges = "rasm.element.graph.delta.edges";
 public const string GraphNodes = "rasm.element.graph.nodes";
 public const string GraphEdges = "rasm.element.graph.edges";
 public const string BakeDuration = "rasm.element.graph.bake.duration";
 public const string AssembleDuration = "rasm.element.projection.assemble.duration";
 public const string Findings = "rasm.element.projection.findings";
 public const string AssessmentOutcomes = "rasm.element.assessment.outcomes";
 public const string WireDuration = "rasm.element.wire.duration";
 public const string WireBytes = "rasm.element.wire.bytes";

 public const string BakeSpan = "rasm.element.graph.bake";
 public const string AssembleSpan = "rasm.element.projection.assemble";
 public const string DecodeSpan = "rasm.element.wire.decode";

 public const string DisciplineTag = "discipline";
 public const string KindTag = "kind";
 public const string OutcomeTag = "outcome";
 public const string SeverityTag = "severity";
 public const string WaivedTag = "waived";

 public static readonly ImmutableArray<double> FoldSeconds = [0.0005, 0.001, 0.005, 0.01, 0.05, 0.1, 0.5, 1, 5, 10];
 public static readonly ImmutableArray<long> GraphCounts = [10, 100, 1_000, 10_000, 100_000, 1_000_000];
 public static readonly ImmutableArray<long> PayloadBytes = [1_024, 16_384, 262_144, 4_194_304, 67_108_864, 536_870_912];

 public static readonly Seq<InstrumentRow> Rows = Seq(
  new InstrumentRow(DeltaNodes, "{node}", "node touches per applied delta", [], Counted),
  new InstrumentRow(DeltaEdges, "{edge}", "edge touches per applied delta", [], Counted),
  new InstrumentRow(GraphNodes, "{node}", "frozen snapshot node population", [], Advised(GraphCounts)),
  new InstrumentRow(GraphEdges, "{edge}", "frozen snapshot edge population", [], Advised(GraphCounts)),
  new InstrumentRow(BakeDuration, "s", "Bake fold wall duration per element root", [], Advised(FoldSeconds)),
  new InstrumentRow(AssembleDuration, "s", "Assemble pipeline wall duration per run", [], Advised(FoldSeconds)),
  new InstrumentRow(Findings, "{finding}", "graded constraint findings by severity and waiver", [SeverityTag, WaivedTag], Counted),
  new InstrumentRow(AssessmentOutcomes, "{assessment}", "assessment node touches by discipline and outcome", [DisciplineTag, OutcomeTag], Counted),
  new InstrumentRow(WireDuration, "s", "wire decode wall duration by kind", [KindTag], Advised(FoldSeconds)),
  new InstrumentRow(WireBytes, "By", "wire payload size by kind", [KindTag], Advised(PayloadBytes)));

 static Counter<long> Counted(Meter meter, string name, string unit, string text) =>
  meter.CreateCounter<long>(name, unit, text);

 static Func<Meter, string, string, string, Instrument> Advised<T>(ImmutableArray<T> bounds) where T : struct =>
  (meter, name, unit, text) =>
   meter.CreateHistogram<T>(name, unit, text, tags: null, advice: new InstrumentAdvice<T> { HistogramBucketBoundaries = bounds });
}

// --- [SERVICES] ---------------------------------------------------------------------------
// Mounted capsule: one Meter minted through the injected IMeterFactory (provider disposal owns instrument
// lifetime — never the Meter, never a process-static), one ActivitySource, one tap projecting every fact.
public sealed class GraphInstrument {
 readonly FrozenDictionary<string, Instrument> byName;
 readonly ActivitySource source;

 GraphInstrument(FrozenDictionary<string, Instrument> byName, ActivitySource source) =>
  (this.byName, this.source) = (byName, source);

 public static GraphInstrument Mount(IMeterFactory factory, string version) {
  Meter meter = factory.Create(new MeterOptions(ElementInstruments.Scope) { Version = version });
  return new(
   ElementInstruments.Rows
    .Map(row => KeyValuePair.Create(row.Name, row.Bind(meter, row.Name, row.Unit, row.Text)))
    .ToFrozenDictionary(StringComparer.Ordinal),
   new ActivitySource(ElementInstruments.Scope, version));
 }

 // One hook-rail subscription — the app root passes it into ElementHookRail.Of beside its own taps.
 public HookTap AsTap() => new("rasm.element.instruments", Project);

 // Span bracket over a rail-valued fold: StartActivity returns null with no listener (the free fast path), the
 // typed fault message lands as the error status, and the using scope is the platform-forced statement seam.
 public Fin<T> Traced<T>(string name, Func<Fin<T>> body) {
  using Activity? span = source.StartActivity(name, ActivityKind.Internal);
  return body().MapFail(error => (span?.SetStatus(ActivityStatusCode.Error, error.Message), error).Item2);
 }

 // Total generated dispatch — a new ElementFact case breaks this tap at compile time, so an unprojected fact is a
 // build error; every tag key is a row's declared closed dimension.
 Unit Project(ElementFact fact) => fact.Switch<GraphInstrument, Unit>(
  state: this,
  deltaApplied: static (self, f) => (
   self.Count(ElementInstruments.DeltaNodes, f.Nodes),
   self.Count(ElementInstruments.DeltaEdges, f.Edges),
   f.Assessments.Iter(touch => self.Count(ElementInstruments.AssessmentOutcomes, 1L,
    new KeyValuePair<string, object?>(ElementInstruments.DisciplineTag, touch.Discipline.Key),
    new KeyValuePair<string, object?>(ElementInstruments.OutcomeTag, touch.Outcome.Key)))).Item3,
  frozen: static (self, f) => (
   self.Record(ElementInstruments.GraphNodes, (long)f.Nodes),
   self.Record(ElementInstruments.GraphEdges, (long)f.Edges)).Item2,
  baked: static (self, f) => self.Record(ElementInstruments.BakeDuration, f.Elapsed.TotalSeconds),
  assembled: static (self, f) => self.Record(ElementInstruments.AssembleDuration, f.Elapsed.TotalSeconds),
  graded: static (self, f) => self.Count(ElementInstruments.Findings, 1L,
   new KeyValuePair<string, object?>(ElementInstruments.SeverityTag, f.Severity.Key),
   new KeyValuePair<string, object?>(ElementInstruments.WaivedTag, f.Waived)),
  decoded: static (self, f) => (
   self.Record(ElementInstruments.WireDuration, f.Elapsed.TotalSeconds, new KeyValuePair<string, object?>(ElementInstruments.KindTag, f.Kind.Key)),
   f.Bytes.IfSome(bytes => ignore(self.Record(ElementInstruments.WireBytes, bytes, new KeyValuePair<string, object?>(ElementInstruments.KindTag, f.Kind.Key))))).Item2);

 // Statement seam: the params span cannot cross a lambda, so each write pattern-probes and branches in place.
 Unit Count(string name, long value, params ReadOnlySpan<KeyValuePair<string, object?>> tags) {
  if (byName[name] is Counter<long> counter) { counter.Add(value, tags); }
  return unit;
 }

 Unit Record(string name, double value, params ReadOnlySpan<KeyValuePair<string, object?>> tags) {
  if (byName[name] is Histogram<double> histogram) { histogram.Record(value, tags); }
  return unit;
 }

 Unit Record(string name, long value, params ReadOnlySpan<KeyValuePair<string, object?>> tags) {
  if (byName[name] is Histogram<long> histogram) { histogram.Record(value, tags); }
  return unit;
 }
}
```

## [04]-[RESEARCH]

- [HOOK_ALTITUDE]: `ElementHookRail` is a MINTED VALUE, never a process-global — `ElementHookRail.Of` is the one mint the app composition root calls (the `ProjectionSuite.Of` precedent), so two apps or two ALCs composing the seam each hold their own rail, gates, taps, and fault cell, and hook identity stays registry-scoped with zero cross-app contention. Emission rides DECORATION, never graph-code edits: `ElementTap` composes fire + the real seam entrypoint at the composition altitude, so `Graph/element`, `Graph/delta`, and `Graph/wire` carry no telemetry reference and their frozen signatures (`Bake(objectId, key)`, `SectionOf(member)`, the decode legs) are untouched. Veto fires PRE-fold — a refusal aborts on the emitter's `Fin<T>` rail and the delta never lands — and a gate is app policy ABOVE `AdmitOnto`'s structural law; a gate transforming the fact reshapes tap evidence only, the applied delta staying the emitter's own value. Subscriber-fault isolation routes the existing `Projection/fault#FAULT_BAND` `ProjectionFailed` capture arm (foreign code captured at the seam boundary through the `Try.lift` funnel, the raw message preserved) into the rail's `TapFaults` cell — the emitter's result untouched, no seventh `ElementFault` arm, no second fault family. Hook ids share the estate four-segment `rasm.<pkg>.<domain>.<point>` lowercase grammar the `Rasm.AppHost` `Observability/hooks` registry enforces, so an app mounting both rails reads one spelling discipline.
- [TELEMETRY_NEUTRALITY]: seam emission rides BCL surfaces alone — `Meter`/`Counter<T>`/`Histogram<T>`/`InstrumentAdvice<T>` and `ActivitySource`, all `System.Diagnostics.DiagnosticSource.dll` shared framework, no manifest row and no OpenTelemetry type reachable from this page — and every meter reaches the process through the injected `IMeterFactory`, so instrument lifetime rides the provider (per-app, per-ALC: two co-resident plugins in one host process stay isolated by provider scope). SDK altitude — provider construction, OTLP/HTTP+protobuf egress, views and cardinality caps, trace-based exemplars, base2-exponential histogram default, W3C composite propagation, tenant baggage — is the composition root's (`Rasm.AppHost` `Observability/instruments`/`telemetry`), which mounts this package through its `InstrumentFan` Element arm: the root registers the `ElementInstruments.Rows` scope, passes `GraphInstrument.Mount(factory, version).AsTap()` into `ElementHookRail.Of`, and admits the `Rasm.Element` meter and source by name. Names are dotted `rasm.element.<domain>.<measure>`, units UCUM (`s`, `By`, `{node}`, `{finding}`), scope name the package id version-stamped across meter and source — the estate wire law realized at library altitude.
- [BOUNDED_DIMENSIONS]: every metric dimension is a closed seam vocabulary — `severity` (two `ConstraintSeverity` rows), `waived` (bool), `kind` (two `WireKind` rows), `discipline` (sixteen `Discipline` rows), `outcome` (eight `AssessmentOutcome` rows) — so worst-case series count is declaration-derivable and the app-tier cardinality caps never fire on seam streams. `AnalysisRoute` is EXCLUDED from every tag: the route roster is `Rasm.Compute`'s opaque unbounded token, so route-grain cost attribution rides the typed `AssessmentTouch` on the fact and the Compute receipt lane, never a metric series. Identities (`NodeId`, `ContentAddress`, finding keys) never become tags — identity joins ride trace exemplars and receipts. A memo-hit dimension on the bake histogram is absent by the honest-evidence law: `Bake` exposes no memo evidence at the decoration altitude, and a fact field never derives from evidence the emitter does not hold — that dimension's sole legal source is a `Bake`-owned receipt column on `Graph/element#ELEMENT_GRAPH`.
