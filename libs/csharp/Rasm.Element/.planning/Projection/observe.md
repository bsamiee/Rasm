# [ELEMENT_OBSERVE]

`Rasm.Element` observability is one app-minted kernel `HookRail` closed over this folder's roster/fact pair and the subscribed `GraphInstrument` and receipt projections. `ElementPoint` closes the `rasm.element.<domain>.<point>` roster on a kernel modality column and derives each point's `TraceScope` off that same id; `ElementFact` carries the `Op`, content key, payload, and the span marks the metric plane refuses. `graph.delta-applied` alone admits veto gates, taps run only after the guarded seam succeeds, and subscriber faults park point-attributed on the rail's `FaultCell`.

`ElementTap` decorates `GraphDelta.AdmitOnto`, `ElementGraph.Bake`, `ModelAudit.Of`, `ProjectionAssembly.Assemble`, and both wire decoders, bracketing each in the kernel `SpanBand` the composing root admits `ElementPoint.Scopes` into. Graph, delta, audit, and wire owners remain emit-free; apps own registration and lifetime.

`GraphInstrument` projects every fact onto the `InstrumentSet` the composing root materializes from `ElementInstrument.Telemetry` — kernel `InstrumentSpec` declarations carrying kind, form, UCUM units, bounded dimensions, and `Buckets` advice. Closed seam vocabularies bound every tag beside the kernel `TenantContext` partition, identifier-grade `NodeId` and `ContentAddress` ride the span alone, and `AnalysisRoute` rides the typed `AssessmentTouch` census alone — one delta touches N routes and `SetTag` is set-or-replace, so a route slot reports whichever touch folded last.

## [01]-[INDEX]

- [02]-[HOOK_RAIL]: `ElementPoint` closes the point vocabulary on its kernel `Modalities` capability set and derived `TraceScope` plane, `ElementFact` carries the `[Union]` fact family through one `Of` mint with its `Point` and `Marks` projections, `ElementHooks.Live` mints the ONE kernel `HookRail` composition over the roster, and `ElementTap` decorates the graph, audit, projection, and wire entrypoints and emits stamped receipts through the kernel `ReceiptSinkPort`.
- [03]-[INSTRUMENT_PROJECTION]: `ElementInstrument` closes the instrument roster — each row CARRYING its kernel `InstrumentSpec` beside the one dotted slot vocabulary both planes spell — and mints the contributor port, while `GraphInstrument.Tap` rails every fact into instrument writes over the composition's `InstrumentSet` and `GraphInstrument.Depth` binds the rail's own parked-fault read to the one pulled row.

## [02]-[HOOK_RAIL]

- Owner: `ElementPoint` the `[SmartEnum<string>]` point vocabulary keyed `rasm.element.<domain>.<point>` (the estate four-segment lowercase grammar) realizing the kernel `IHookRoster<ElementPoint>` floor with a `CapabilitySet<HookModality>` column and the kernel `TraceScope` plane derived off the id's own head; `ElementFact` the `[Union]` typed fact family over the shared `Op Key` base; `AssessmentTouch` the per-delta assessment census row (the graded `Projection/audit#INTEGRITY_SWEEP` `AuditTally` its audit-side sibling, seated with the taxonomy it grades); `WireKind` the two-row decode-kind vocabulary; `ElementHooks` the composition entry minting the ONE kernel `HookRail<ElementPoint, ElementFact, TelemetrySource>` (the folder mints zero rail mechanism — the kernel `HookGate`/`HookTap` rows ride domain aliases); `ElementTap` the decoration capability and the receipt-port producer.
- Cases: `DeltaApplied` (delta content key, node/edge touch counts, header presence, the typed `AssessmentTouch` census — fired at the admission seam, the one `Veto` point) · `Frozen` (snapshot `ContentAddress.OfGraph` key, node/edge population) · `Baked` (element root, fold `Duration`) · `Audited` (the graded snapshot address, the typed `AuditTally` `(category, severity, count)` census with its derived total/blocking/drift reads, fold `Duration`) · `Assembled` (merged-delta content key — the same derivation the `Rasm.Persistence` event dedup reads — projector count, delta magnitude, finding count, pipeline `Duration`) · `Graded` (one `ConstraintFinding` — severity row, original `Error`, optional generated numeric fault code, replayable `KeyOf` content address, waived flag) · `Decoded` (`WireKind`, `Option<long>` payload bytes — `None` for a non-seekable stream, never a fabricated length — decoded magnitude, `Duration`); the closed lifecycle-fact family.
- Entry: `ElementFact.Of` discriminates on input shape. `ElementHooks.Live(key, gates, taps, band, cell)` mints the kernel rail — one seat per roster row, gates through the kernel `Veto` fold (a gate on an observe-only point refuses at the mint), taps through `Observe` at each tap's OWN `Scope` column, detach custody and the point census both the kernel rail's — and preserves a composition refusal as the original `Error`; the `band` parameter is the kernel `SpanBand` lowered onto the rail's `IHookSpan` floor, so the e30 seam crossing is fence-performed here. `rail.Fire(at, fact, key, body)` is the kernel guarded fire — vetoes fold, the `Fin<T>` body runs over the ADMITTED fact, taps fan only from its success path — and `rail.Faults` reads parked subscriber faults. `ElementTap.Admitted`/`Baked`/`Audited`/`Assembled` preserve each seam owner's rail type; `DecodedGraph`/`DecodedDelta` specialize one `Decoded<T>` decoration kernel with decoder, kind, and magnitude projections; `ElementTap.Receipts(port, correlation)` is the subscriber that stamps every fired fact onto the kernel `ReceiptSinkPort` as a `ReceiptEnvelope` keyed by the point id.
- Auto: `Fire` on an observe-only point has no gates; the capsule's shield captures both throws and returned failures, parking each as a point-attributed `IsolatedFault` with the tap's name folded into the failure detail. `ElementTap.Admitted` vetoes before `AdmitOnto`, emits `DeltaApplied` only after admission succeeds, and emits `Frozen` after the snapshot exists. `Assembled` times the pipeline and emits one `Graded` fact per finding; `Audited` times the one audit fold and fires its receipt's census. Both delta-keyed facts resolve their tolerance through the ONE `delta.Header.IfNone(base)` rule, so a header-establishing delta keys the fact exactly as the Persistence dedup keys the event. `Point` maps each case to its preallocated row and `Marks` to its identifier-grade span evidence, both through the generated dispatch. Marks stamp ahead of the veto fold, so a refused admission leaves the span carrying what was attempted beside the kernel's `Error` status.
- Receipt: a fired `ElementFact` is the evidence event; the emitter rail already carries the outcome. The rail's `FaultCell` retains subscriber-failure evidence a health panel reads. Replay/audit, AppUi, instrument, and receipt consumers share tap rows, so observability subscribes to facts and never mints them — `Receipts` projects each fact into a `ReceiptEnvelope` the composing root's `ReceiptSinkPort` stamps and emits, which is the e30 port's one Element producer.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`/`[Union]` + the generated `Switch`/`Map`), LanguageExt.Core (`Fin`/`Seq`/`Option`), NodaTime (`Duration`), `Rasm` (the kernel hook rail, the trace band, the receipt port, `MonotonicTimeline`, `Op`), BCL inbox (`Activity` the span handle the band hands back, `System.Text.Json` the receipt payload projection).
- Growth: a new lifecycle fact is one `ElementPoint` row and one `ElementFact` case — the generated dispatch breaks every projection (the `[03]` instrument tap and the `Marks` fold included) loudly at compile time, and a point on a new domain segment arms its span plane with no roster edit because the plane derives off the id; a new subscriber is one `HookTap`/`HookGate` row at the app root's mint, narrowing to its own points through the tap's `Scope` column rather than through a `Point`-probing arm inside its delegate; delivery semantics are the kernel modality rows; never a per-point registry sibling, never a process-global rail, and never an emit call inside a graph page.
- Boundary: the rail is a sealed class, so a `with` copy cannot alias the evidence cell. Gates refuse or rewrite the fact's own evidence and never touch structural state — the admitted fact reaches the guarded body and the taps alike, so a redaction lands once rather than per subscriber. Facts emit only after successful bodies; the capsule forks observe taps, so a tap never blocks the seam. Span custody is the kernel band's — this package declares `TraceScope` rows and owns no `ActivitySource`, no listener gate, and no status stamp, so the composing root's `SpanBand.Of(version, scopes)` holds the one source lifetime and a band-less composition runs the identical rail with observability absent rather than degraded. Thrown bodies lower at their own seam owner — `ProjectionAssembly.Assemble`'s boundary funnel — so the decoration mints no second trap. Delta and assembly keys reuse `GraphDelta.Address`; frozen keys use `ContentAddress.OfGraph`. Point ids follow the kernel `rasm.<pkg>.<domain>.<point>` grammar and their planes the kernel `rasm.<pkg>.<domain>` grammar, so id and scope are ONE derivation.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Assessment;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Thinktecture;
using static LanguageExt.Prelude;
// The kernel rail closed over this folder's roster/fact/owner triple — one alias set so every signature reads the
// domain name, never the three-parameter spelling.
using ElementGate = Rasm.Domain.HookGate<Rasm.Element.Projection.ElementPoint, Rasm.Element.Projection.ElementFact, Rasm.Domain.TelemetrySource>;
using ElementObserver = Rasm.Domain.HookTap<Rasm.Element.Projection.ElementPoint, Rasm.Element.Projection.ElementFact, Rasm.Domain.TelemetrySource>;
using ElementRail = Rasm.Domain.HookRail<Rasm.Element.Projection.ElementPoint, Rasm.Element.Projection.ElementFact, Rasm.Domain.TelemetrySource>;

namespace Rasm.Element.Projection;

// --- [TYPES] ------------------------------------------------------------------------------
// Point roster keyed rasm.element.<domain>.<point> — the estate four-segment lowercase grammar hook ids share with
// instrumentation scopes and metric names. Realizes the kernel IHookRoster<ElementPoint> floor (S15), so the ONE
// kernel HookRail takes this roster as its type parameter and seats mint from Items alone. The SEVEN rows ARE the
// ElementFact cases — ElementFact.Point is the primary correspondence; a fact case without a row (or a row without
// a case) breaks that generated Map at compile time, which is the roster's own totality proof. Modalities is the
// kernel capability set: the delta admission point admits synchronous Veto gates BESIDE its observers (a veto-only
// set would refuse every tap); every other point is observe-only.
[SmartEnum<string>]
public sealed partial class ElementPoint : IHookRoster<ElementPoint> {
 public static readonly ElementPoint DeltaApplied = new("rasm.element.graph.delta-applied", CapabilitySet<HookModality>.Of(HookModality.Veto, HookModality.Observe));
 public static readonly ElementPoint Frozen = new("rasm.element.graph.frozen", CapabilitySet<HookModality>.Of(HookModality.Observe));
 public static readonly ElementPoint Baked = new("rasm.element.graph.baked", CapabilitySet<HookModality>.Of(HookModality.Observe));
 // Graph plane, not a plane of its own: the audit grades one frozen snapshot, so its span belongs beside the freeze
 // and the bake that produced what it reads.
 public static readonly ElementPoint Audited = new("rasm.element.graph.audited", CapabilitySet<HookModality>.Of(HookModality.Observe));
 public static readonly ElementPoint Assembled = new("rasm.element.projection.assembled", CapabilitySet<HookModality>.Of(HookModality.Observe));
 public static readonly ElementPoint Finding = new("rasm.element.projection.finding", CapabilitySet<HookModality>.Of(HookModality.Observe));
 public static readonly ElementPoint WireDecoded = new("rasm.element.wire.decoded", CapabilitySet<HookModality>.Of(HookModality.Observe));

 // One materialized index answers the roster floor's Id and Plane reads — the id and its rasm.<pkg>.<domain> head
 // derive from the key ONCE, so a bracket pays a lookup and a scope can never fork from the point it brackets.
 static readonly Lazy<FrozenDictionary<ElementPoint, (HookId Id, TraceScope Plane)>> Index = new(
  static () => Items.ToFrozenDictionary(static row => row, static row =>
   (HookId.Create(value: row.Key), TraceScope.Create(value: string.Join('.', row.Key.Split('.')[..3])))),
  LazyThreadSafetyMode.ExecutionAndPublication);

 public CapabilitySet<HookModality> Modalities { get; }

 public HookId Id => Index.Value[this].Id;

 public Option<TraceScope> Plane => Some(Index.Value[this].Plane);

 // Composing roots admit this roster into SpanBand.Of; the roster shares three planes, so the projection
 // deduplicates, and an unadmitted scope refuses on the kernel rail rather than dropping every span silently.
 public static Seq<TraceScope> Scopes => toSeq(Index.Value.Values).Map(static entry => entry.Plane).Distinct().Strict();
}

// Decode-kind discriminant the wire fact and the wire instruments dimension on — the two typed decode legs.
[SmartEnum<string>]
public sealed partial class WireKind {
 public static readonly WireKind Snapshot = new("snapshot");
 public static readonly WireKind Delta = new("delta");
}

// The two-row series key the finding counter's waiver dimension reads — a raw bool tag exported True/False where
// the series vocabulary is waived/unwaived, and a stringly literal pair would drift from the fact that decides it.
[SmartEnum<string>]
public sealed partial class WaiverMark {
 public static readonly WaiverMark Waived = new("waived");
 public static readonly WaiverMark Unwaived = new("unwaived");

 public static WaiverMark Of(Option<ConstraintWaiver> waiver) => waiver.IsSome ? Waived : Unwaived;
}

// --- [MODELS] -----------------------------------------------------------------------------
// One assessment node touched by a delta — the typed census the DeltaApplied fact carries so outcome instruments
// and route-cost consumers read evidence off the fact, never a re-scan of the delta. Route rides TYPED here and
// never becomes a metric dimension (the opaque token DECLARED at Assessment/assessment — the roster behind it is
// Compute's — unbounded, the [03] cardinality law).
public readonly record struct AssessmentTouch(Discipline Discipline, AnalysisRoute Route, AssessmentOutcome Outcome);

// Closed lifecycle-fact family — each case carries the kernel Op key (the shared base column), the graph or
// delta ContentAddress where the point owns one, and the point payload. ONE polymorphic Of mints every case by
// input shape; Point projects the owning row through the generated Map over the preallocated ElementPoint
// singletons, and Marks projects the same evidence onto the span plane.
[Union]
public abstract partial record ElementFact : IHookFact<ElementPoint> {
 private ElementFact(Op key) { Key = key; }

 public Op Key { get; }

 // Established carries WHICH header the delta established — a bool erased the schema/view/tolerance a consumer
 // joins the model-creating event on; absence is the ordinary non-establishing delta.
 public sealed record DeltaApplied(Op Key, ContentAddress Delta, int Nodes, int Edges, Option<Header> Established, Seq<AssessmentTouch> Assessments) : ElementFact(Key);
 public sealed record Frozen(Op Key, ContentAddress Snapshot, int Nodes, int Edges) : ElementFact(Key);
 public sealed record Baked(Op Key, NodeId Root, Duration Elapsed) : ElementFact(Key);
 // ONE census column, three derived reads — the counts a dashboard wants are folds over the buckets, never
 // parallel stored columns a producer could file inconsistently with the census beside them.
 public sealed record Audited(Op Key, ContentAddress Snapshot, Seq<AuditTally> Findings, Duration Elapsed) : ElementFact(Key) {
  public int Total => Findings.Fold(0, static (count, tally) => count + tally.Count);
  public int Blocking => Findings.Filter(static tally => tally.Severity.Blocks).Fold(0, static (count, tally) => count + tally.Count);
  // Drifts reads the tamper count off the census by category, so the drift series and the finding series agree by construction.
  public int Drifts => Findings.Filter(static tally => tally.Category == AuditCategory.AddressDrift).Fold(0, static (count, tally) => count + tally.Count);
 }
 public sealed record Assembled(Op Key, ContentAddress Delta, int Projectors, int Nodes, int Edges, int Findings, Duration Elapsed) : ElementFact(Key);
 // Violation retains the original Error; Code derives only for a Fault, so a foreign Error never acquires code zero.
 // Waiver is the pinning evidence itself (WHO waived, WHEN) — the projection derives the two-row series key.
 public sealed record Graded(Op Key, ConstraintSeverity Severity, Error Violation, ContentAddress FindingKey, Option<ConstraintWaiver> Waiver) : ElementFact(Key) {
  public Option<int> Code => Violation is Fault fault ? Some(fault.Code) : None;
 }
 public sealed record Decoded(Op Key, WireKind Kind, Option<long> Bytes, int Nodes, int Edges, Duration Elapsed) : ElementFact(Key);

 public ElementPoint Point => Map(
  deltaApplied: ElementPoint.DeltaApplied,
  frozen: ElementPoint.Frozen,
  baked: ElementPoint.Baked,
  audited: ElementPoint.Audited,
  assembled: ElementPoint.Assembled,
  graded: ElementPoint.Finding,
  decoded: ElementPoint.WireDecoded);

 // The kernel IHookFact seating correspondence (E-M16) — derived from the generated total Map above, so the
 // rail's Fire gate and this union's declared fact→point pairing cannot drift; a 1:1 union IS this one line.
 public bool Seats(ElementPoint at) => at == Point;

 // Identifier-grade evidence the metric plane refuses — content keys and roots whose cardinality is unbounded on
 // a series and free on a sampler-thinned span — projected per case onto the ONE slot vocabulary [03] declares.
 // Each slot carries ONE semantic because SetTag is set-or-replace and two facts share a span: the delta and the
 // snapshot it settles would collapse onto a single last-wins row under a shared key. Graded is the stream case
 // and stamps nothing — N findings fire inside the assembly's own span, so their evidence belongs to the metric
 // counter and the receipt. Tenancy is absent by design: the app root's baggage promotion already stamps
 // rasm.tenant on every span, so folding TenantContext here double-stamps the partition.
 public Seq<(string Slot, object? Value)> Marks => Switch(
  deltaApplied: static f => Seq<(string Slot, object? Value)>((ElementInstrument.DeltaSlot, f.Delta.ToValue())),
  frozen: static f => Seq<(string Slot, object? Value)>((ElementInstrument.SnapshotSlot, f.Snapshot.ToValue())),
  // NodeId's own Value is the owned spelling — ToString() reads a generated surface the owner never pinned, so a
  // generator default would silently re-shape the span evidence a trace consumer joins on.
  baked: static f => Seq<(string Slot, object? Value)>((ElementInstrument.RootSlot, f.Root.Value)),
  // Audited marks the snapshot it GRADED, so the span joins the audit to the freeze that produced the address.
  audited: static f => Seq<(string Slot, object? Value)>((ElementInstrument.SnapshotSlot, f.Snapshot.ToValue())),
  assembled: static f => Seq<(string Slot, object? Value)>((ElementInstrument.DeltaSlot, f.Delta.ToValue())),
  graded: static _ => Seq<(string Slot, object? Value)>(),
  decoded: static f => Seq<(string Slot, object? Value)>((ElementInstrument.KindSlot, f.Kind.Key)));

 // Delta fact: the content key is the SAME GraphDelta.Address streaming derivation the Rasm.Persistence event
 // dedup keys on — one projection, two consumers, never a second spelling. The tolerance takes the delta's OWN
 // resolved header, so the fact key and the dedup key cannot fork on a header-establishing delta.
 public static ElementFact Of(Op key, GraphDelta delta, Header seed) => new DeltaApplied(
  key, delta.Address(Grid(delta, seed)), delta.NodeCount, delta.EdgeCount, delta.Header, Touches(delta));

 // Frozen fact: pays the full OfGraph snapshot fold — accepted at the decoration altitude, never a graph-page charge.
 public static ElementFact Of(Op key, ElementGraph graph) => new Frozen(
  key, ContentAddress.OfGraph(graph), graph.Nodes.Count, graph.Edges.Length);

 public static ElementFact Of(Op key, NodeId root, Duration elapsed) => new Baked(key, root, elapsed);

 // Audit fact: the receipt already carries the graded snapshot's address and its finding rows, so the fact folds the
 // census once and re-derives no verdict — ModelAudit holds the authority and this fact carries its evidence.
 public static ElementFact Of(Op key, ModelAudit audit, Duration elapsed) => new Audited(
  key, audit.Snapshot, audit.Tallies, elapsed);

 public static ElementFact Of(Op key, AssemblyReceipt receipt, int projectors, Duration elapsed) => new Assembled(
  key, receipt.Delta.Address(Grid(receipt.Delta, receipt.Graph.Header)),
  projectors, receipt.Delta.NodeCount, receipt.Delta.EdgeCount, receipt.Findings.Count, elapsed);

 // Grid is the ONE tolerance resolution both delta-keyed facts read — the SAME `delta.Header.IfNone(base)` rule
 // AdmitOnto, Freeze, and ReplayOnto resolve a header under. Reading a base header directly forks the fact key
 // from the Persistence dedup key on exactly the model-creating delta that establishes the header, where it matters most.
 static double Grid(GraphDelta delta, Header seed) => delta.Header.IfNone(seed).Tolerance;

 public static ElementFact Of(Op key, ConstraintFinding finding) => new Graded(
  key, finding.Severity, finding.Violation, finding.Key, finding.Waiver);

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
// The composition entry over the KERNEL rail (S15): the folder keeps its roster and fact union and mints ZERO rail
// mechanism — seats, veto/observe capsules, scoped taps, detach custody, and the fault cell all ride
// HookRail<ElementPoint, ElementFact, TelemetrySource>. Live's one domain move is the band lowering; a kernel
// composition refusal (a gate on an observe-only point, a mid-mount attach failure already rolled back) remains the
// original Error returned by ElementRail.Of. NAMED LOSS from the
// drain: the folder rail held Clock and Band beside its points — the composing root now hands ElementTap a kernel
// MonotonicTimeline and the rail its IHookSpan/FaultCell (E-E7 wires the root), which is the shape every other
// decoration site already takes.
public static class ElementHooks {
 // `band` is the kernel SpanBand, lowered here onto the rail's IHookSpan floor — the e30 seam crossing is performed
 // by this signature. NAMED LOSS: a non-band IHookSpan cannot enter Element's composition; a traceless composition
 // passes None and runs the identical rail, which is the only other lawful span posture below the app root.
 public static Fin<ElementRail> Live(
  Op key, Seq<ElementGate> gates = default, Seq<ElementObserver> taps = default,
  Option<SpanBand> band = default, Option<FaultCell> cell = default) =>
  ElementRail.Of(key, gates, taps, band.Map(static span => (IHookSpan)span), cell);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// Seam-owned decoration capability (the apps own the wiring — the ASSEMBLE_CAPABILITY split): each entry composes
// the kernel rail's own traced Fire around the REAL seam entrypoint and returns exactly that owner's rail type, so
// graph pages stay emit-free and an app root wires one call. Facts fire on success alone; a failed fold already
// rails its fault. Marks stamp onto Activity.Current inside the fire body — the kernel bracket owns the span, so
// the stamp rides whatever span that bracket opened, and a band-less composition stamps nothing.
public static class ElementTap {
 // Veto precedes AdmitOnto; both DeltaApplied and Frozen facts fire only after structural admission succeeds.
 public static Fin<(ElementGraph Graph, GraphDelta Delta)> Admitted(ElementRail rail, GraphDelta delta, ElementGraph seed, Op key) =>
  rail.Fire(ElementPoint.DeltaApplied, ElementFact.Of(key, delta, seed.Header), key, fact => Marked(fact, () => delta.AdmitOnto(seed, key)))
   .Bind(step => rail.Fire(ElementPoint.Frozen, ElementFact.Of(key, step.Graph), key, fact => Marked(fact, () => Fin.Succ(step))));

 public static Fin<Element> Baked(ElementRail rail, MonotonicTimeline line, ElementGraph graph, NodeId root, Op key) =>
  Timed(rail, line, ElementPoint.Baked, key, () => graph.Bake(root, key), (_, elapsed) => ElementFact.Of(key, root, elapsed));

 // Audited times the one fold and fires its receipt — the graph-plane sibling of Baked, so a delivery gate reading
 // ModelAudit and a dashboard reading the audit series see the same run.
 public static Fin<ModelAudit> Audited(ElementRail rail, MonotonicTimeline line, ElementGraph graph, Op key) =>
  Timed(rail, line, ElementPoint.Audited, key, () => ModelAudit.Of(graph, key), (audit, elapsed) => ElementFact.Of(key, audit, elapsed));

 // Assembled times the whole pipeline, then fires one Graded fact per receipt finding — warnings and waived
 // deviations included, the same evidence stream AssemblyReceipt.Findings persists.
 public static Fin<AssemblyReceipt> Assembled(ElementRail rail, MonotonicTimeline line, ProjectionSuite suite, ElementGraph seed, ProjectionContext ctx) =>
  Timed(rail, line, ElementPoint.Assembled, ctx.Key, () => ProjectionAssembly.Assemble(suite, seed, ctx),
   (receipt, elapsed) => ElementFact.Of(ctx.Key, receipt, suite.Projectors.Count, elapsed),
   receipt => receipt.Findings
    .TraverseM(finding => rail.Fire(ElementPoint.Finding, ElementFact.Of(ctx.Key, finding), ctx.Key)).As()
    .Map(_ => receipt));

 public static Fin<ElementGraph> DecodedGraph(ElementRail rail, MonotonicTimeline line, Stream payload, WireLimits limits, Op key) =>
  Decoded(rail, line, payload, limits, key, WireKind.Snapshot,
   decode: static (stream, bounds, op) => ElementWire.DecodeGraph(stream, bounds, op),
   magnitude: static graph => (graph.Nodes.Count, graph.Edges.Length));

 public static Fin<GraphDelta> DecodedDelta(
  ElementRail rail, MonotonicTimeline line, Stream payload, Header basis, WireLimits limits, Op key) =>
  Decoded(rail, line, payload, limits, key, WireKind.Delta,
   decode: (stream, bounds, op) => ElementWire.DecodeDelta(stream, basis, bounds, op),
   magnitude: static delta => (delta.NodeCount, delta.EdgeCount));

 static Fin<T> Decoded<T>(
  ElementRail rail, MonotonicTimeline line, Stream payload, WireLimits limits, Op key, WireKind kind,
  Func<Stream, WireLimits, Op, Fin<T>> decode, Func<T, (int Nodes, int Edges)> magnitude) =>
  Timed(rail, line, ElementPoint.WireDecoded, key, () => decode(payload, limits, key), (value, elapsed) => magnitude(value) switch {
   var (nodes, edges) => ElementFact.Of(key, kind, Length(payload), nodes, edges, elapsed),
  });

 // ONE monotonic timing kernel every timed decoration composes — kernel Capture/Elapsed off the composing root's
 // MonotonicTimeline, never a raw GetTimestamp/GetElapsedTime pair and never a wall-clock diff. `fan` is the
 // optional per-decoration continuation firing derived facts after the primary; nullable because a lambda argument
 // has no type until it converts, so an Option-wrapped delegate parameter rejects every inline call site.
 static Fin<T> Timed<T>(
  ElementRail rail, MonotonicTimeline line, ElementPoint at, Op key, Func<Fin<T>> body, Func<T, Duration, ElementFact> fact,
  Func<T, Fin<T>>? fan = null) =>
  line.Capture(key).Bind(start =>
   body().Bind(value =>
    line.Capture(key).Bind(end =>
     line.Elapsed(start, end, key).Bind(elapsed =>
      rail.Fire(at, fact(value, Duration.FromTimeSpan(elapsed)), key, admitted => Marked(admitted, () => Fin.Succ(value)))
       .Bind(landed => fan is null ? Fin.Succ(landed) : fan(landed))))));

 // Marks stamp as a STEP onto the kernel bracket's own current span (the named boundary exemption — a span tag
 // write is host state, not a domain value); a refused admission still leaves the span carrying what was attempted.
 static Fin<T> Marked<T>(ElementFact fact, Func<Fin<T>> body) {
  fact.Marks.Iter(mark => ignore(Activity.Current?.SetTag(mark.Slot, mark.Value)));
  return body();
 }

 // Typed absence over a non-seekable stream — a fabricated length is false evidence.
 static Option<long> Length(Stream payload) => payload.CanSeek ? Some(payload.Length) : None;

 // The e30 ReceiptSinkPort producer: one subscriber row stamps every fired fact as a ReceiptEnvelope keyed by the
 // point id, the composing root handing the port and the correlation in (E-E7 wires the root). Running the Send IO
 // here is the port's own emit edge — a refusal or throw parks point-attributed on the rail's FaultCell like every
 // other subscriber failure, so a dead sink is visible evidence, never a lost receipt.
 public static ElementObserver Receipts(ReceiptSinkPort port, CorrelationId correlation) {
 Op key = Op.Of(name: "rasm.element.receipts");
 return new(key, fact => key
   .Catch(body: () => port
    .Send(correlation, TenantContext.Current, TelemetrySource.Element, fact.Point.Key, Payload(fact)).Run())
   .Map(static _ => unit));
}

 // Receipt payload: the fact's own census and duration evidence as JSON — identifier keys ride the owned ToValue/
 // Value renders the wire face already pins, and an absent byte length OMITS its key rather than spelling null.
 static JsonElement Payload(ElementFact fact) => JsonSerializer.SerializeToElement(fact.Switch<JsonObject>(
  deltaApplied: static f => new() {
   ["delta"] = f.Delta.ToValue(), ["nodes"] = f.Nodes, ["edges"] = f.Edges,
   ["established"] = f.Established.IsSome, ["assessments"] = f.Assessments.Count,
  },
  frozen: static f => new() { ["snapshot"] = f.Snapshot.ToValue(), ["nodes"] = f.Nodes, ["edges"] = f.Edges },
  baked: static f => new() { ["root"] = f.Root.Value, ["seconds"] = f.Elapsed.TotalSeconds },
  audited: static f => new() {
   ["snapshot"] = f.Snapshot.ToValue(), ["findings"] = f.Total, ["blocking"] = f.Blocking,
   ["drifts"] = f.Drifts, ["seconds"] = f.Elapsed.TotalSeconds,
  },
  assembled: static f => new() {
   ["delta"] = f.Delta.ToValue(), ["projectors"] = f.Projectors, ["nodes"] = f.Nodes,
   ["edges"] = f.Edges, ["findings"] = f.Findings, ["seconds"] = f.Elapsed.TotalSeconds,
  },
  graded: static f => {
   JsonObject row = new() {
    ["severity"] = f.Severity.Key,
    ["waived"] = WaiverMark.Of(f.Waiver).Key,
    ["finding"] = f.FindingKey.ToValue(),
   };
   f.Code.Iter(code => row["code"] = code);
   return row;
  },
  decoded: static f => {
   JsonObject row = new() {
    ["kind"] = f.Kind.Key, ["nodes"] = f.Nodes, ["edges"] = f.Edges, ["seconds"] = f.Elapsed.TotalSeconds,
   };
   f.Bytes.Iter(bytes => row["bytes"] = bytes);
   return row;
  }));
}
```

## [03]-[INSTRUMENT_PROJECTION]

- Law: [PULL_POLARITY] — a measurement is PUSHED unless the cell it reports is process-scoped AND has no fire to ride. Graph population stays EVENT-shaped for a reason, not for want of the kernel level family: a live population cell needs a bounded key, and this owner's key spaces refuse one from both ends — `Root` and `Snapshot` are identifier-grade and unbounded (which is why they seat as span marks, never series dimensions), while discipline, outcome, severity, waiver, and wire kind partition findings and decodes and never a graph's size. `Frozen` histograms are the honest shape, each freeze contributing its own measured population, where an unkeyed scalar level over MANY graphs republishes whichever root froze last as current state and renders a retired owner's reading indistinguishable from a live one. `TapFaults` depth alone clears the bar: a rail's evidence cell is composition-scoped (one cell, not one per graph) and a parked subscriber fault is captured by the capsule shield without ever firing a fact, so no push site exists at all — it binds through the kernel's registered read for the rail's own lifetime and retires with it.
- Owner: `ElementInstrument` the closed `rasm.element.*` roster — a `[SmartEnum<string>]` whose every row CARRIES its kernel `InstrumentSpec` (kind, measurement form, UCUM unit, kernel `Buckets` advice, the closed dimension set) so `Rows` derives from `Items` and construction proves each row's name against its key, beside the one dotted slot block both the metric rows and the `[02]` span marks spell — with the contributor-port mint under the kernel `TelemetrySource.Element` scope; `GraphInstrument` the fact-to-write projection over the `InstrumentSet` the composing root materializes.
- Entry: `ElementInstrument.Telemetry(version)` is the contributor port the composing root materializes — the semconv coordinate is the kernel pin so all three signals bump together — and a root outside that fan binds `InstrumentSet.Of(cells, (meter, ElementInstrument.Rows))` directly against its own minted meter; either path, never both. `GraphInstrument.Tap(set)` returns the tap row passed to `ElementHooks.Live`, handing the kernel's write rail straight to the capsule shield; `GraphInstrument.Depth(set, rail, key)` registers the rail's own parked-fault read against the one pulled row under the caller's `Op` and returns the scope that retires it, so the composing root arms it AFTER the mint the tap fed.
- Auto: `DeltaApplied` counts the two delta magnitudes and one `rasm.element.assessment.outcomes` per census touch (discipline and outcome dimensions — both closed rows); `Frozen` records the snapshot node/edge population histograms; `Baked`/`Audited`/`Assembled` record the duration histograms; `Audited` counts one `rasm.element.audit.findings` per `AuditTally` bucket (integrity category and severity dimensions) and writes the drift count UNCONDITIONALLY — a clean run posts its own zero, so the tamper series never leaves an alert unable to tell a verified snapshot from an unaudited one; `Graded` retains the original `Error` and counts findings under its optional generated numeric code, severity, and waiver; foreign errors leave that opt-in dimension absent instead of fabricating code zero; `Decoded` records duration and — when the payload length is known — size, both under the `WireKind` dimension; the kernel `TenantContext.Current` resolves ONCE per fact and threads as dispatch state, so every write of one fact lands under one partition, a root-tenant process mints no tenant dimension, and a partitioned one mints it uniformly — the ambient read is the AsyncLocal slot the kernel owns, never a value captured at tap-mint time, which stamps the composing root's tenant onto every later request; instrument identity de-duplicates by name inside the one meter, so name, unit, kind, and description are declaration facts the row carries once.
- Receipt: none — the projection is a pure fold of the fact tap; a metric minted beside it is a second truth, and every operational dashboard reads the exported stream, never a seam cell. `InstrumentSet.Write` rails an unmounted name or a family mismatch out to the tap shield, which parks it point-attributed, so a mount defect is visible rather than a silent measurement drop.
- Packages: BCL `System.Diagnostics.Metrics` reached through the kernel capsule alone, `Rasm` (the kernel instrument mechanism, the scope identity roster, the numeric fault-code slot, and the tenancy frame), Thinktecture.Runtime.Extensions (the generated fact `Switch`), LanguageExt.Core.
- Growth: a new metric is one `ElementInstrument` row carrying its `InstrumentSpec` and one write in the owning `Switch` arm — a new fact case breaks the tap at compile time, so an unprojected fact is a build error, never a silent gap; a new PULLED row is one `Level` declaration with one `Bind` registration on the owner whose lifetime bounds it, admitted only under `[PULL_POLARITY]`; a new instrument family is one kernel `InstrumentKind` row, a new bucket policy one kernel `Buckets` row; a new span attribute is one slot row here and one `Marks` arm at `[02]`; never an inline `new Meter(...)`, never a create or write call outside this fence, and never a numeric value as a tag.
- Boundary: this fence is the package telemetry spine and the only declaration and write site — the create bodies belong to the kernel's `InstrumentKind` x `MeasureForm` derivation, so a re-spelled counter or histogram create here is the forked-stream defect. Closed seam vocabularies bound every tag; slot keys carry the package's own dotted `rasm.element.<dimension>` namespace so a concept a second package also tags never collides, and the numeric fault-code path reads the kernel slot rather than re-declaring one. Opaque routes and identities never become tags — they ride the `[02]` span marks — and the tenant slot every row declares is the ONE dimension whose presence the write decides rather than the declaration: `TenantContext.Key` reads `None` at the root row, so `Tags` projects empty and the series exports untagged on the SAME instrument a partitioned process exports keyed. That is the kernel's one absence discriminant governing both the tenancy and level planes, so the declared roster stays uniform, no row carries an optionality column of its own, and a governance view reading `Dimensions` for its tag keys must tolerate the absent entry rather than mint a second stream for it. Provider, exporter, views, exemplars, and base2-exponential defaults remain composition-root policy, and meter lifetime rides the minting factory at that root, so this page holds no `IMeterFactory`, no `Meter`, and no disposable. Memo-hit dimensions remain absent until `Bake` exposes that evidence.

```csharp signature
// --- [TABLES] -----------------------------------------------------------------------------
// Closed roster on the kernel KernelInstrument form: each row CARRIES its InstrumentSpec and `Rows` derives from
// `Items`, so the const-name roster and a hand-listed sequence mirroring it are one declaration, the write plane
// addresses by ROW (the instrument law), and construction proves the row's name against its key. Kind and
// MeasureForm are the spec's own columns, so the kernel derives every create body and this page spells none;
// advice bounds read the kernel Buckets rows, and every row declares the kernel tenant slot the write stamps.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ElementInstrument {
 // Dimension slots carry this package's dotted namespace — a bare noun forks the moment a sibling package tags
 // `outcome` or `category`, which two already do estate-wide. Delta, Snapshot, and Root stay SPAN-only:
 // a content address and a node id are identifier-grade and unbounded on a series.
 // AuditSlot carries this package's own closed AUDIT category vocabulary, distinct from the kernel numeric fault-code
 // slot the finding counter bands on — an integrity sweep and a failure class are two taxonomies, and one slot for
 // both makes "orphan" and "Value" siblings on a chart neither answers.
 public const string AuditSlot = "rasm.element.audit.category";
 public const string DeltaSlot = "rasm.element.delta";
 public const string DisciplineSlot = "rasm.element.discipline";
 public const string KindSlot = "rasm.element.wire.kind";
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

 // AddressDrift carries the tamper gate's OWN series: ContentAddress.Verify drift is a security signal a dashboard
 // alerts on, so it never dissolves into one category bucket of the coverage-quality counter beside it.
 public static readonly ElementInstrument AddressDrift = new(
  "rasm.element.audit.drift",
  InstrumentSpec.Create("rasm.element.audit.drift", InstrumentKind.Count, MeasureForm.Whole, "{node}",
   "content-verification drifts per audited snapshot", Seq(TenantContext.TenantSlot), None, None, None));

 // TapFaults measures the rail's parked-fault depth — the ONE pulled row on this roster, seated under the [03] level law.
 public static readonly ElementInstrument TapFaults = new(
  "rasm.element.observe.tap.faults",
  InstrumentSpec.Create("rasm.element.observe.tap.faults", InstrumentKind.Level, MeasureForm.Whole, "{fault}",
   "parked subscriber faults held on a rail's evidence cell", Seq(TenantContext.TenantSlot), None, None, None));

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

 public static readonly ElementInstrument WireDuration = new(
  "rasm.element.wire.duration",
  InstrumentSpec.Create("rasm.element.wire.duration", InstrumentKind.Distribution, MeasureForm.Real, "s",
   "wire decode wall duration by kind", Seq(TenantContext.TenantSlot, KindSlot), Some(Buckets.FoldSeconds), None, None));

 // Size, never bytes: the estate name grammar carries no unit suffix and the UCUM By unit states the measure.
 public static readonly ElementInstrument WireSize = new(
  "rasm.element.wire.size",
  InstrumentSpec.Create("rasm.element.wire.size", InstrumentKind.Distribution, MeasureForm.Whole, "By",
   "wire payload size by kind", Seq(TenantContext.TenantSlot, KindSlot), Some(Buckets.PayloadBytes), None, None));

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

// --- [OPERATIONS] -------------------------------------------------------------------------
// Fact-to-write projection over the composition's InstrumentSet — no minted state, so provider disposal owns
// instrument lifetime and this owner holds nothing to dispose.
public static class GraphInstrument {
 // One hook-rail subscription — the app root passes it into ElementHooks.Live beside its own taps; the
 // projection's typed refusal rides straight out, so the capsule shield parks it as tap-attributed evidence.
 // Unscoped by construction: the projection owns a TOTAL Switch, so it wants every point and a scope row here would
 // be a second, drift-prone statement of the same totality the compiler already enforces.
 public static ElementObserver Tap(InstrumentSet set) => new(Op.Of(name: "rasm.element.instruments"), fact => Project(set, fact));

 // Depth binds the one measurement no fact can carry: a subscriber failure is captured by the capsule shield and
 // never fires a fact, so a pushed row has no write site at all. Instead the rail's fault cell hands its OWN parked
 // read to the kernel's registered read for its own lifetime and the returned scope retires it with the
 // composition, keeping depth live with no call site pushing one. Tenant resolves ONCE at bind and that is CORRECT
 // here where the per-fact read is not — a rail is composition-scoped, not request-scoped, so its evidence cell
 // belongs to the partition that composed it.
 public static Fin<IDisposable> Depth(InstrumentSet set, ElementRail rail, Op key) =>
  set.Bind(ElementInstrument.TapFaults.Row, () => (double)rail.Faults.Parked.Count, key, InstrumentSet.Tags(TenantContext.Current));

 // Total generated dispatch — a new ElementFact case breaks this tap at compile time, so an unprojected fact is
 // a build error; every tag key is a row's declared closed dimension, and every write rides the kernel rail, so
 // an unmounted name and a measurement-form mismatch surface as refusals rather than silent drops. An arm whose
 // writes share a tag set binds it once at the head, so the shared partition is folded per fact, never per write.
 // Project resolves the ambient partition ONCE per fact and threads it as state beside the mounted set:
 // `TenantContext.Current` is the kernel's AsyncLocal slot, so a per-write read lets two writes of ONE fact land
 // under two partitions when a flow re-enters mid-projection, and a census loop pays the resolve per touch.
 static Fin<Unit> Project(InstrumentSet set, ElementFact fact) =>
  fact.Switch<(InstrumentSet Rows, TenantContext Tenant), Fin<Unit>>(
  state: (set, TenantContext.Current),
  deltaApplied: static (state, f) =>
   from shared in Fin.Succ(InstrumentSet.Tags(state.Tenant))
   from nodes in state.Rows.Write(ElementInstrument.DeltaNodes.Row, (long)f.Nodes, shared)
   from edges in state.Rows.Write(ElementInstrument.DeltaEdges.Row, (long)f.Edges, shared)
   // Independent per-touch writes ACCUMULATE — a fail-fast TraverseM hides every refusal after the first.
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
  // AddressDrift writes UNCONDITIONALLY, zero included: a tamper series whose absence and whose clean run look
  // alike gives an alert nothing to rest on, so a verified-clean snapshot posts its own zero.
  audited: static (state, f) =>
   from shared in Fin.Succ(InstrumentSet.Tags(state.Tenant))
   from wall in state.Rows.Write(ElementInstrument.AuditDuration.Row, f.Elapsed.TotalSeconds, shared)
   from drift in state.Rows.Write(ElementInstrument.AddressDrift.Row, (long)f.Drifts, shared)
   from census in f.Findings.Traverse(tally => state.Rows.Write(ElementInstrument.AuditFindings.Row, (long)tally.Count, InstrumentSet.Tags(state.Tenant,
     (ElementInstrument.AuditSlot, tally.Category.Key),
     (ElementInstrument.SeveritySlot, tally.Severity.Key)))).As()
   select unit,
  assembled: static (state, f) => state.Rows.Write(ElementInstrument.AssembleDuration.Row, f.Elapsed.TotalSeconds, InstrumentSet.Tags(state.Tenant)),
  // Graded findings retain the original Error and project an optional generated code; the audit arm bands on AuditSlot — a
  // constraint violation bands with every other failure class estate-wide, an integrity sweep with its own taxonomy.
  graded: static (state, f) => state.Rows.Write(ElementInstrument.Findings.Row, 1L, InstrumentSet.Tags(state.Tenant,
   (KernelInstrument.CodeSlot, f.Code.Match<object?>(Some: static code => code, None: static () => null)),
   (ElementInstrument.SeveritySlot, f.Severity.Key),
   (ElementInstrument.WaivedSlot, WaiverMark.Of(f.Waiver).Key))),
  // Absent length is success, never a fabricated zero: the duration write stands alone on a non-seekable stream.
  decoded: static (state, f) =>
   from shared in Fin.Succ(InstrumentSet.Tags(state.Tenant, (ElementInstrument.KindSlot, f.Kind.Key)))
   from wall in state.Rows.Write(ElementInstrument.WireDuration.Row, f.Elapsed.TotalSeconds, shared)
   from size in f.Bytes.Match(
    Some: bytes => state.Rows.Write(ElementInstrument.WireSize.Row, bytes, shared),
    None: static () => Fin.Succ(unit))
   select size);
}
```

## [04]-[RESEARCH]

(none)
