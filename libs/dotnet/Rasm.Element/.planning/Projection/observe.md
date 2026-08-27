# [ELEMENT_OBSERVE]

`Rasm.Element` observability is one app-minted kernel `HookSet` closed over this folder's roster/fact pair and the subscribed `GraphInstrument` and `ElementTap.Events` projections. `ElementPoint` closes the `rasm.element.<domain>.<point>` roster on a kernel modality column and derives each point's `TraceScope` off that same id; `ElementFact` carries the content key, payload, and the span marks the metric plane refuses. `graph.delta-applied` alone admits veto gates, taps run only after the guarded contract succeeds, and subscriber faults park point-attributed on the hooks' `FaultCell`.

`ElementTap` decorates `GraphDelta.AdmitOnto`, `ElementGraph.Bake`, `ModelAudit.Of`, and `ProjectionAssembly.Assemble`, bracketing each in the kernel `SpanBand` the composing root admits `ElementPoint.Scopes` into. Graph, delta, audit, and projection owners remain emit-free.

`GraphInstrument` projects every fact onto the `InstrumentSet` the composing root materializes from `ElementInstrument.Telemetry` — kernel `InstrumentSpec` declarations carrying kind, form, UCUM units, bounded dimensions, and `Buckets` advice. Closed contract vocabularies bound every tag beside the kernel `TenantContext` partition, identifier-grade `NodeId` and `ContentAddress` ride the span and the event `subject` alone, and `AnalysisRoute` rides the typed `AssessmentTouch` census alone — one delta touches N routes and `SetTag` is set-or-replace, so a route slot reports whichever touch folded last.

## [01]-[INDEX]

- [02]-[HOOKS]: `ElementPoint` closes the point vocabulary on its kernel `Modalities` capability set and derived `TraceScope` plane, `ElementFact` carries the `[Union]` fact family through one `Of` mint with its `Point`, `Subject`, and `Marks` projections, `ElementHooks.Live` mints the one kernel `HookSet` composition, and `ElementTap` decorates graph, audit, and projection entrypoints and publishes every durable point as a CloudEvent through the kernel `RasmEventEnvelope.Publish`.
- [03]-[INSTRUMENT_PROJECTION]: `ElementInstrument` closes the instrument roster — each row CARRYING its kernel `InstrumentSpec` beside the one dotted slot vocabulary both planes spell — and mints the contributor port, while `GraphInstrument.Tap` routes every fact into instrument writes over the composition's `InstrumentSet` and `GraphInstrument.Depth` binds the hooks' own parked-fault read to the one pulled row.

## [02]-[HOOKS]

- Owner: `ElementPoint` the point vocabulary, `ElementFact` the typed fact family, `AssessmentTouch` the per-delta assessment census, `ElementHooks` the kernel-hook composition, and `ElementTap` the decoration capability.
- Cases: `DeltaApplied`, `Frozen`, `Baked`, `Audited`, `Assembled`, and `Graded`; generated node-edit support is not a graph-ingress lifecycle and mints no snapshot/delta decode facts.
- Entry: `ElementFact.Of` discriminates on input shape. `ElementHooks.Live` mints the kernel hooks and `hooks.Fire` guards the admitted fact. `ElementTap.Admitted`/`Baked`/`Audited`/`Assembled` preserve each owner's result type; `ElementTap.Events` publishes each fact fired on an `ElementPoint.Durable` row as one CloudEvent.
- Auto: `Fire` on an observe-only point has no gates; the capsule's shield captures both throws and returned failures, parking each as a point-attributed `IsolatedFault` with the tap's name folded into the failure detail. `ElementTap.Admitted` vetoes before `AdmitOnto`, emits `DeltaApplied` only after admission succeeds, and emits `Frozen` after the snapshot exists. `Assembled` times the pipeline and emits one `Graded` fact per finding; `Audited` times the one audit fold and fires its tally census. Both delta-keyed facts resolve their tolerance through the ONE `delta.Header.IfNone(base)` rule, so a header-establishing delta keys the fact exactly as the Persistence dedup keys the event. `Point` maps each case to its preallocated row, `Subject` to its optional content key, and `Marks` to its identifier-grade span evidence, all through the generated dispatch. Marks stamp ahead of the veto fold, so a refused admission leaves the span carrying what was attempted beside the kernel's `Error` status.
- Law: `ElementPoint.Durable` names the rows whose fact outlives the process — `DeltaApplied` (the delta Persistence appends under the same content key), `Frozen` (a snapshot exists), `Audited` (a graded verdict), and `Graded` (a constraint finding) — and `Baked`/`Assembled` stay observation alone (span and histogram), because no replay, gate, or peer runtime reads a memo bake or a pipeline timing after the fact.
- Output: a fired `ElementFact` is the fact; the emitter's result already carries the outcome and the hooks' `FaultCell` retains subscriber-failure evidence a health panel reads. Replay/audit, AppUi, instrument, and event consumers share tap rows, so observability subscribes to facts and never mints them — `Events` mints one CloudEvent per durable fact (`type` the point id under the `rasm.<domain>.<subject>.<fact>` grammar, `source` `rasm:element/<plane>`, `subject` the fact's content key, `id` a fresh Guid-v7, no `data`) through `RasmEventEnvelope.Publish`, which stamps `traceparent`/`tracestate`/`baggage` and the HLC `time`/`sequence` pair, and hands the envelope to the composing root's binding.
- Growth: a new lifecycle fact is one `ElementPoint` row and one `ElementFact` case — the generated dispatch breaks every projection (the `[03]` instrument tap and the `Marks` fold included) loudly at compile time, and a point on a new domain segment arms its span plane and its event type and source with no roster edit because all three derive off the id; a fact that becomes durable is one row added to `Durable`; a new subscriber is one `HookTap`/`HookGate` row at the app root's mint, narrowing to its own points through the tap's `Scope` column rather than through a `Point`-probing arm inside its delegate; delivery semantics are the kernel modality rows; never a per-point registry sibling, never a process-global dispatcher, and never an emit call inside a graph page.
- Boundary: the dispatcher is a sealed class, so a `with` copy cannot alias the evidence cell. Gates refuse or rewrite the fact's own evidence and never touch structural state — the admitted fact reaches the guarded body and the taps alike, so a redaction lands once rather than per subscriber. Facts emit only after successful bodies; the capsule forks observe taps, so a tap never blocks the boundary. Span custody is the kernel band's — this package declares `TraceScope` rows and owns no `ActivitySource`, no listener gate, and no status stamp, so the composing root's `SpanBand.Of(version, scopes)` holds the one source lifetime and a band-less composition runs the identical dispatcher with observability absent rather than degraded. Thrown bodies lower at their own contract owner — `ProjectionAssembly.Assemble`'s boundary funnel — so the decoration mints no second trap. Delta and assembly keys reuse `GraphDelta.Address`; frozen keys use `ContentAddress.OfGraph`. Point ids follow the kernel `rasm.<pkg>.<domain>.<point>` grammar, their planes the kernel `rasm.<pkg>.<domain>` grammar, and their event types the kernel `rasm.<domain>.<subject>.<fact>` grammar with `<fact>` past tense, so id, scope, type, and source are ONE derivation. The event contract, the HLC cell, and the binding are composition material — this page holds no broker, format, or transport, and an event carries no `data`, because the durable body is the `GraphDelta` Persistence appends and the `ModelAudit`/`AssembledModel` values it stores under the same content keys.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Diagnostics;
using CloudNative.CloudEvents;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
// Contracts are retired from this logic.
using Rasm.Domain;
using Rasm.Element.Assessment;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Thinktecture;
using static LanguageExt.Prelude;
using ElementGate = Rasm.Domain.HookGate<Rasm.Element.Projection.ElementPoint, Rasm.Element.Projection.ElementFact, Rasm.Domain.TelemetrySource>;
using ElementObserver = Rasm.Domain.HookTap<Rasm.Element.Projection.ElementPoint, Rasm.Element.Projection.ElementFact, Rasm.Domain.TelemetrySource>;
using ElementHooks = Rasm.Domain.HookSet<Rasm.Element.Projection.ElementPoint, Rasm.Element.Projection.ElementFact, Rasm.Domain.TelemetrySource>;

namespace Rasm.Element.Projection;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class ElementPoint : IHookRoster<ElementPoint> {
 public static readonly ElementPoint DeltaApplied = new("rasm.element.graph.delta-applied", CapabilitySet<HookModality>.Of(HookModality.Veto, HookModality.Observe));
 public static readonly ElementPoint Frozen = new("rasm.element.graph.frozen", CapabilitySet<HookModality>.Of(HookModality.Observe));
 public static readonly ElementPoint Baked = new("rasm.element.graph.baked", CapabilitySet<HookModality>.Of(HookModality.Observe));
 public static readonly ElementPoint Audited = new("rasm.element.graph.audited", CapabilitySet<HookModality>.Of(HookModality.Observe));
 public static readonly ElementPoint Assembled = new("rasm.element.projection.assembled", CapabilitySet<HookModality>.Of(HookModality.Observe));
 public static readonly ElementPoint Graded = new("rasm.element.projection.graded", CapabilitySet<HookModality>.Of(HookModality.Observe));

 static readonly Lazy<FrozenDictionary<ElementPoint, (HookId Id, TraceScope Plane, EventType Type, EventSource Source)>> Index = new(
  static () => Items.ToFrozenDictionary(static row => row, static row => row.Key.Split('.') switch {
   var parts => (
    HookId.Create(value: row.Key),
    TraceScope.Create(value: string.Join('.', parts[..3])),
    EventType.Create(value: row.Key),
    EventSource.Of(parts[1], parts[2])),
  }),
  LazyThreadSafetyMode.ExecutionAndPublication);

 public CapabilitySet<HookModality> Modalities { get; }

 public HookId Id => Index.Value[this].Id;

 public Option<TraceScope> Plane => Some(Index.Value[this].Plane);

 public EventType Type => Index.Value[this].Type;

 public EventSource Source => Index.Value[this].Source;

 public static Seq<TraceScope> Scopes => toSeq(Index.Value.Values).Map(static entry => entry.Plane).Distinct().Strict();

 public static Seq<ElementPoint> Durable => Seq(DeltaApplied, Frozen, Audited, Graded);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct AssessmentTouch(Discipline Discipline, AnalysisRoute Route, AssessmentOutcome Outcome);

[Union]
public abstract partial record ElementFact : IHookFact<ElementPoint> {
 private ElementFact() { Key = key; }


 public sealed record DeltaApplied(ContentAddress Delta, int Nodes, int Edges, Option<Header> Established, Seq<AssessmentTouch> Assessments) : ElementFact(Key);
 public sealed record Frozen(ContentAddress Snapshot, int Nodes, int Edges) : ElementFact(Key);
 public sealed record Baked(NodeId Root, Duration Elapsed) : ElementFact(Key);
 public sealed record Audited(ContentAddress Snapshot, Seq<AuditTally> Findings, Duration Elapsed) : ElementFact(Key) {
  public int Total => Findings.Fold(0, static (count, tally) => count + tally.Count);
  public int Blocking => Findings.Filter(static tally => tally.Severity.Blocks).Fold(0, static (count, tally) => count + tally.Count);
  public int Drifts => Findings.Filter(static tally => tally.Category == AuditCategory.AddressDrift).Fold(0, static (count, tally) => count + tally.Count);
 }
 public sealed record Assembled(ContentAddress Delta, int Projectors, int Nodes, int Edges, int Findings, Duration Elapsed) : ElementFact(Key);
 public sealed record Graded(ConstraintSeverity Severity, Error Violation, ContentAddress FindingKey, Option<ConstraintWaiver> Waiver) : ElementFact(Key) {
  public Option<int> Code => Violation is Fault fault ? Some(fault.Code) : None;
 }
 public ElementPoint Point => Map(
  deltaApplied: ElementPoint.DeltaApplied,
  frozen: ElementPoint.Frozen,
  baked: ElementPoint.Baked,
  audited: ElementPoint.Audited,
  assembled: ElementPoint.Assembled,
  graded: ElementPoint.Graded);

 public bool Seats(ElementPoint at) => at == Point;

 public Option<UInt128> Subject => Switch(
  deltaApplied: static f => Some(f.Delta.ToValue()),
  frozen: static f => Some(f.Snapshot.ToValue()),
  baked: static _ => Option<UInt128>.None,
  audited: static f => Some(f.Snapshot.ToValue()),
  assembled: static f => Some(f.Delta.ToValue()),
  graded: static f => Some(f.FindingKey.ToValue()));

 public Seq<(string Slot, object? Value)> Marks => Switch(
  deltaApplied: static f => Seq<(string Slot, object? Value)>((ElementInstrument.DeltaSlot, ContentHash.Hex(f.Delta.ToValue()))),
  frozen: static f => Seq<(string Slot, object? Value)>((ElementInstrument.SnapshotSlot, ContentHash.Hex(f.Snapshot.ToValue()))),
  baked: static f => Seq<(string Slot, object? Value)>((ElementInstrument.RootSlot, f.Root.ToValue())),
  audited: static f => Seq<(string Slot, object? Value)>((ElementInstrument.SnapshotSlot, ContentHash.Hex(f.Snapshot.ToValue()))),
  assembled: static f => Seq<(string Slot, object? Value)>((ElementInstrument.DeltaSlot, ContentHash.Hex(f.Delta.ToValue()))),
  graded: static _ => Seq<(string Slot, object? Value)>());

 public static ElementFact Of(GraphDelta delta, Header seed) => new DeltaApplied(delta.Address(Grid(delta, seed)), delta.NodeCount, delta.EdgeCount, delta.Header, Touches(delta));

 public static ElementFact Of(ElementGraph graph) => new Frozen(ContentAddress.OfGraph(graph), graph.Nodes.Count, graph.Edges.Length);

 public static ElementFact Of(NodeId root, Duration elapsed) => new Baked(root, elapsed);

 public static ElementFact Of(ModelAudit audit, Duration elapsed) => new Audited(audit.Snapshot, audit.Tallies, elapsed);

 public static ElementFact Of(AssembledModel model, int projectors, Duration elapsed) => new Assembled(model.Delta.Address(Grid(model.Delta, model.Graph.Header)),
  projectors, model.Delta.NodeCount, model.Delta.EdgeCount, model.Findings.Count, elapsed);

 static double Grid(GraphDelta delta, Header seed) => delta.Header.IfNone(seed).Tolerance;

 public static ElementFact Of(ConstraintFinding finding) => new Graded(finding.Severity, finding.Violation, finding.Waiver);

 static Seq<AssessmentTouch> Touches(GraphDelta delta) =>
  (delta.AddedNodes + delta.RevisedNodes.Map(static r => r.After))
   .Choose(static n => n is Node.Assessment a
    ? Some(new AssessmentTouch(a.Payload.Discipline, a.Payload.Route, a.Payload.Outcome))
    : None);
}

// --- [SERVICES] ------------------------------------------------------------------------
public static class ElementHooks {
 public static Fin<ElementHooks> Live(Seq<ElementGate> gates = default, Seq<ElementObserver> taps = default,
  Option<SpanBand> band = default, Option<FaultCell> cell = default) =>
  ElementHooks.Of(gates, taps, band.Map(static span => (IHookSpan)span), cell);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ElementTap {
 public static Fin<(ElementGraph Graph, GraphDelta Delta)> Admitted(ElementHooks hooks, GraphDelta delta, ElementGraph seed) =>
  hooks.Fire(ElementPoint.DeltaApplied, ElementFact.Of(key, delta, seed.Header), key, fact => Marked(fact, () => delta.AdmitOnto(seed, key)))
   .Bind(step => hooks.Fire(ElementPoint.Frozen, ElementFact.Of(key, step.Graph), key, fact => Marked(fact, () => Fin.Succ(step))));

 public static Fin<Element> Baked(ElementHooks hooks, MonotonicTimeline line, ElementGraph graph, NodeId root) =>
  Timed(hooks, line, ElementPoint.Baked, key, () => graph.Bake(root, key), (_, elapsed) => ElementFact.Of(key, root, elapsed));

 public static Fin<ModelAudit> Audited(ElementHooks hooks, MonotonicTimeline line, ElementGraph graph) =>
  Timed(hooks, line, ElementPoint.Audited, key, () => ModelAudit.Of(graph, key), (audit, elapsed) => ElementFact.Of(key, audit, elapsed));

 public static Fin<AssembledModel> Assembled(ElementHooks hooks, MonotonicTimeline line, ProjectionSuite suite, ElementGraph seed, ProjectionContext ctx) =>
  Timed(hooks, line, ElementPoint.Assembled, ctx.Key, () => ProjectionAssembly.Assemble(suite, seed, ctx),
   (model, elapsed) => ElementFact.Of(ctx.Key, model, suite.Projectors.Count, elapsed),
   model => model.Findings
    .TraverseM(finding => hooks.Fire(ElementPoint.Graded, ElementFact.Of(ctx.Key, finding), ctx.Key)).As()
    .Map(_ => model));

 static Fin<T> Timed<T>(
  ElementHooks hooks, MonotonicTimeline line, ElementPoint at, Func<Fin<T>> body, Func<T, Duration, ElementFact> fact,
  Func<T, Fin<T>>? fan = null) =>
  Error.New(key.Message).Bind(start =>
   body().Bind(value =>
    Error.New(key.Message).Bind(end =>
     line.Elapsed(start, end, key).Bind(elapsed =>
      hooks.Fire(at, fact(value, Duration.FromTimeSpan(elapsed)), key, admitted => Marked(admitted, () => Fin.Succ(value)))
       .Bind(landed => fan is null ? Fin.Succ(landed) : fan(landed))))));

 static Fin<T> Marked<T>(ElementFact fact, Func<Fin<T>> body) {
  fact.Marks.Iter(mark => ignore(Activity.Current?.SetTag(mark.Slot, mark.Value)));
  return body();
 }

 public static ElementObserver Events(EventExtensionContract<Extensions> contract, Hlc clock, Func<CloudEvent, Fin<Unit>> binding) {
  return new(fact =>
   from id in FactoryBridge.Accept<EventId>(Guid.CreateVersion7().ToString("N"))
   from envelope in RasmEventEnvelope.Publish(
    new RasmEventMint<Extensions>(fact.Point.Type, fact.Point.Source, id, fact.Subject, clock.Wall, None, None, null, new Extensions()),
    contract, clock, key)
   from _ in binding(envelope)
   select unit,
   Scope: Some(ElementPoint.Durable));
 }
}
```

## [03]-[INSTRUMENT_PROJECTION]

- Law: [PULL_POLARITY] — a measurement is PUSHED unless the cell it reports is process-scoped AND has no fire to ride. Graph population stays EVENT-shaped because its identifier-grade root and snapshot keys are unbounded. `Frozen` histograms measure each completed snapshot, while an unkeyed scalar level over many graphs would publish only the last graph. `TapFaults` depth alone is pulled: the evidence cell is composition-scoped and subscriber faults have no fact-emission site.
- Owner: `ElementInstrument` the closed `rasm.element.*` roster — a `[SmartEnum<string>]` whose every row CARRIES its kernel `InstrumentSpec` (kind, measurement form, UCUM unit, kernel `Buckets` advice, the closed dimension set) so `Rows` derives from `Items` and construction proves each row's name against its key, beside the one dotted slot block both the metric rows and the `[02]` span marks spell — with the contributor-port mint under the kernel `TelemetrySource.Element` scope; `GraphInstrument` the fact-to-write projection over the `InstrumentSet` the composing root materializes.
- Entry: `ElementInstrument.Telemetry(version)` is the contributor port the composing root materializes — the semconv coordinate is the kernel pin so all three signals bump together — and a root outside that fan binds `InstrumentSet.Of(cells, (meter, ElementInstrument.Rows))` directly against its own minted meter; either path, never both. `GraphInstrument.Tap(set)` returns the tap row passed to `ElementHooks.Live`, handing the kernel's write path straight to the capsule shield; `GraphInstrument.Depth(set, hooks, key)` registers the hooks' own parked-fault read against the one pulled row and returns the scope that retires it, so the composing root arms it AFTER the mint the tap fed.
- Auto: `DeltaApplied` counts the two delta magnitudes and one `rasm.element.assessment.outcomes` per census touch (discipline and outcome dimensions — both closed rows); `Frozen` records the snapshot node/edge population histograms; `Baked`/`Audited`/`Assembled` record the duration histograms; `Audited` counts one `rasm.element.audit.findings` per `AuditTally` bucket (integrity category and severity dimensions) and writes the drift count UNCONDITIONALLY — a clean run posts its own zero, so the tamper series never leaves an alert unable to tell a verified snapshot from an unaudited one; `Graded` retains the original `Error` and counts findings under its optional generated numeric code, severity, and waiver; foreign errors leave that opt-in dimension absent instead of fabricating code zero; the kernel `TenantContext.Current` resolves ONCE per fact and threads as dispatch state, so every write of one fact lands under one partition, a root-tenant process mints no tenant dimension, and a partitioned one mints it uniformly — the ambient read is the AsyncLocal slot the kernel owns, never a value captured at tap-mint time, which stamps the composing root's tenant onto every later request; instrument identity de-duplicates by name inside the one meter, so name, unit, kind, and description are declaration facts the row carries once.
- Output: the projection is a pure fold of the fact tap, and every operational dashboard reads the exported stream, never a shared cell. `InstrumentSet.Write` refuses an unmounted name or a family mismatch out to the tap shield, which parks it point-attributed, so a mount defect is visible rather than a silent measurement drop.
- Packages: BCL `System.Diagnostics.Metrics` reached through the kernel capsule alone, `Rasm` (the kernel instrument mechanism, the scope identity roster, the numeric fault-code slot, and the tenancy frame), Thinktecture.Runtime.Extensions (the generated fact `Switch`), LanguageExt.Core.
- Growth: a new metric is one `ElementInstrument` row carrying its `InstrumentSpec` and one write in the owning `Switch` arm — a new fact case breaks the tap at compile time, so an unprojected fact is a build error, never a silent gap; a new PULLED row is one `Level` declaration with one `Bind` registration on the owner whose lifetime bounds it, admitted only under `[PULL_POLARITY]`; a new instrument family is one kernel `InstrumentKind` row, a new bucket policy one kernel `Buckets` row; a new span attribute is one slot row here and one `Marks` arm at `[02]`; never an inline `new Meter(...)`, never a create or write call outside this fence, and never a numeric value as a tag.
- Boundary: this fence is the package telemetry spine and the only declaration and write site — the create bodies belong to the kernel's `InstrumentKind` x `MeasureForm` derivation, so a re-spelled counter or histogram create here is the forked-stream defect. Closed contract vocabularies bound every tag; slot keys carry the package's own dotted `rasm.element.<dimension>` namespace so a concept a second package also tags never collides, and the numeric fault-code path reads the kernel slot rather than re-declaring one. Opaque routes and identities never become tags — they ride the `[02]` span marks — and the tenant slot every row declares is the ONE dimension whose presence the write decides rather than the declaration: `TenantContext.Key` reads `None` at the root row, so `Tags` projects empty and the series exports untagged on the SAME instrument a partitioned process exports keyed. That is the kernel's one absence discriminant governing both the tenancy and level planes, so the declared roster stays uniform, no row carries an optionality column of its own, and a governance view reading `Dimensions` for its tag keys must tolerate the absent entry rather than mint a second stream for it. Provider, exporter, views, exemplars, and base2-exponential defaults remain composition-root policy, and meter lifetime rides the minting factory at that root, so this page holds no `IMeterFactory`, no `Meter`, and no disposable. Memo-hit dimensions remain absent until `Bake` exposes that evidence.

```csharp
// --- [TABLES] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ElementInstrument {
 public const string AuditSlot = "rasm.element.audit.category";
 public const string DeltaSlot = "rasm.element.delta";
 public const string DisciplineSlot = "rasm.element.discipline";
 public const string OutcomeSlot = "rasm.element.assessment.outcome";
 public const string RootSlot = "rasm.element.root";
 public const string SeveritySlot = "rasm.element.severity";
 public const string SnapshotSlot = "rasm.element.snapshot";
 public const string WaivedSlot = "rasm.element.waived";

 public static readonly ElementInstrument DeltaNodes = new(
  "rasm.element.graph.delta.nodes",
  InstrumentSpec.Create("rasm.element.graph.delta.nodes", InstrumentKind.Count, MeasureForm.Whole, "{node}",
   "node touches per applied delta", Seq(TenantContext.TenantSlot), None, None, None));

 public static readonly ElementInstrument DeltaEdges = new(
  "rasm.element.graph.delta.edges",
  InstrumentSpec.Create("rasm.element.graph.delta.edges", InstrumentKind.Count, MeasureForm.Whole, "{edge}",
   "edge touches per applied delta", Seq(TenantContext.TenantSlot), None, None, None));

 public static readonly ElementInstrument GraphNodes = new(
  "rasm.element.graph.nodes",
  InstrumentSpec.Create("rasm.element.graph.nodes", InstrumentKind.Distribution, MeasureForm.Whole, "{node}",
   "frozen snapshot node population", Seq(TenantContext.TenantSlot), Some(Buckets.GraphCounts), None, None));

 public static readonly ElementInstrument GraphEdges = new(
  "rasm.element.graph.edges",
  InstrumentSpec.Create("rasm.element.graph.edges", InstrumentKind.Distribution, MeasureForm.Whole, "{edge}",
   "frozen snapshot edge population", Seq(TenantContext.TenantSlot), Some(Buckets.GraphCounts), None, None));

 public static readonly ElementInstrument BakeDuration = new(
  "rasm.element.graph.bake.duration",
  InstrumentSpec.Create("rasm.element.graph.bake.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
   "Bake fold wall duration per element root", Seq(TenantContext.TenantSlot), Some(Buckets.FoldSeconds), None, None));

 public static readonly ElementInstrument AuditDuration = new(
  "rasm.element.audit.duration",
  InstrumentSpec.Create("rasm.element.audit.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
   "ModelAudit fold wall duration per graded snapshot", Seq(TenantContext.TenantSlot), Some(Buckets.FoldSeconds), None, None));

 public static readonly ElementInstrument AuditFindings = new(
  "rasm.element.audit.findings",
  InstrumentSpec.Create("rasm.element.audit.findings", InstrumentKind.Count, MeasureForm.Whole, "{finding}",
   "model-audit findings by integrity category and severity", Seq(TenantContext.TenantSlot, AuditSlot, SeveritySlot), None, None, None));

 public static readonly ElementInstrument AddressDrift = new(
  "rasm.element.audit.drift",
  InstrumentSpec.Create("rasm.element.audit.drift", InstrumentKind.Count, MeasureForm.Whole, "{node}",
   "content-verification drifts per audited snapshot", Seq(TenantContext.TenantSlot), None, None, None));

 public static readonly ElementInstrument TapFaults = new(
  "rasm.element.observe.tap.faults",
  InstrumentSpec.Create("rasm.element.observe.tap.faults", InstrumentKind.Level, MeasureForm.Whole, "{fault}",
   "parked subscriber faults held on the hooks' evidence cell", Seq(TenantContext.TenantSlot), None, None, None));

 public static readonly ElementInstrument AssembleDuration = new(
  "rasm.element.projection.assemble.duration",
  InstrumentSpec.Create("rasm.element.projection.assemble.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
   "Assemble pipeline wall duration per run", Seq(TenantContext.TenantSlot), Some(Buckets.FoldSeconds), None, None));

 public static readonly ElementInstrument Findings = new(
  "rasm.element.projection.findings",
  InstrumentSpec.Create("rasm.element.projection.findings", InstrumentKind.Count, MeasureForm.Whole, "{finding}",
   "graded constraint findings by optional numeric fault code, severity, and waiver",
   Seq(TenantContext.TenantSlot, KernelInstrument.CodeSlot, SeveritySlot, WaivedSlot), None, None, None));

 public static readonly ElementInstrument AssessmentOutcomes = new(
  "rasm.element.assessment.outcomes",
  InstrumentSpec.Create("rasm.element.assessment.outcomes", InstrumentKind.Count, MeasureForm.Whole, "{assessment}",
   "assessment node touches by discipline and outcome", Seq(TenantContext.TenantSlot, DisciplineSlot, OutcomeSlot), None, None, None));

 public InstrumentSpec Row { get; }

 public static Seq<InstrumentSpec> Rows => toSeq(Items).Map(static row => row.Row).Strict();

 public static TelemetryContributorPort Telemetry(string version) =>
  new(Scope: TelemetrySource.Element, Version: version, Instruments: Rows, Planes: ElementPoint.Scopes);

 static partial void ValidateConstructorArguments(ref string key, ref InstrumentSpec row) {
  if (!string.Equals(key, row.Name, StringComparison.Ordinal)) {
   throw new ArgumentException($"<element-instrument:{key}>", nameof(row));
  }
 }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GraphInstrument {
 public static ElementObserver Tap(InstrumentSet set) => new(fact => Project(set, fact));

 public static Fin<IDisposable> Depth(InstrumentSet set, ElementHooks hooks) =>
  set.Bind(ElementInstrument.TapFaults.Row, () => (double)hooks.Faults.Parked.Count, key, InstrumentSet.Tags(TenantContext.Current));

 static Fin<Unit> Project(InstrumentSet set, ElementFact fact) =>
  fact.Switch<(InstrumentSet Rows, TenantContext Tenant), Fin<Unit>>(
  state: (set, TenantContext.Current),
  deltaApplied: static (state, f) =>
   from shared in Fin.Succ(InstrumentSet.Tags(state.Tenant))
   from nodes in state.Rows.Write(ElementInstrument.DeltaNodes.Row, (long)f.Nodes, shared)
   from edges in state.Rows.Write(ElementInstrument.DeltaEdges.Row, (long)f.Edges, shared)
   from census in f.Assessments.Traverse(touch => state.Rows.Write(ElementInstrument.AssessmentOutcomes.Row, 1L, InstrumentSet.Tags(state.Tenant,
     (ElementInstrument.DisciplineSlot, touch.Discipline.Key),
     (ElementInstrument.OutcomeSlot, touch.Outcome.Key)))).As()
   select unit,
  frozen: static (state, f) =>
   from shared in Fin.Succ(InstrumentSet.Tags(state.Tenant))
   from nodes in state.Rows.Write(ElementInstrument.GraphNodes.Row, (long)f.Nodes, shared)
   from edges in state.Rows.Write(ElementInstrument.GraphEdges.Row, (long)f.Edges, shared)
   select edges,
  baked: static (state, f) => state.Rows.Write(ElementInstrument.BakeDuration.Row, f.Elapsed.TotalSeconds, InstrumentSet.Tags(state.Tenant)),
  audited: static (state, f) =>
   from shared in Fin.Succ(InstrumentSet.Tags(state.Tenant))
   from wall in state.Rows.Write(ElementInstrument.AuditDuration.Row, f.Elapsed.TotalSeconds, shared)
   from drift in state.Rows.Write(ElementInstrument.AddressDrift.Row, (long)f.Drifts, shared)
   from census in f.Findings.Traverse(tally => state.Rows.Write(ElementInstrument.AuditFindings.Row, (long)tally.Count, InstrumentSet.Tags(state.Tenant,
     (ElementInstrument.AuditSlot, tally.Category.Key),
     (ElementInstrument.SeveritySlot, tally.Severity.Key)))).As()
   select unit,
  assembled: static (state, f) => state.Rows.Write(ElementInstrument.AssembleDuration.Row, f.Elapsed.TotalSeconds, InstrumentSet.Tags(state.Tenant)),
  graded: static (state, f) => state.Rows.Write(ElementInstrument.Findings.Row, 1L, InstrumentSet.Tags(state.Tenant,
   (KernelInstrument.CodeSlot, f.Code.Match<object?>(Some: static code => code, None: static () => null)),
   (ElementInstrument.SeveritySlot, f.Severity.Key),
   (ElementInstrument.WaivedSlot, f.Waiver.IsSome ? "waived" : "unwaived"))));
}
```

## [04]-[RESEARCH]

(none)
