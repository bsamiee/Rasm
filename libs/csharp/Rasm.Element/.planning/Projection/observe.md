# [ELEMENT_OBSERVE]

`Rasm.Element` observability is one app-minted `ElementHookRail` and one subscribed `GraphInstrument`, both compositions of the kernel signal capsule. `ElementPoint` closes the `rasm.element.<domain>.<point>` roster on a kernel modality column and derives each point's `TraceScope` off that same id; `ElementFact` carries the `Op`, content key, payload, and the span marks the metric plane refuses. `graph.delta-applied` alone admits veto gates, taps run only after the guarded seam succeeds, and subscriber faults park point-attributed in `TapFaults`.

`ElementTap` decorates `GraphDelta.AdmitOnto`, `ElementGraph.Bake`, `ModelAudit.Of`, `ProjectionAssembly.Assemble`, and both wire decoders, bracketing each in the kernel `SpanBand` the composing root admits `ElementPoint.Scopes` into. Graph, delta, audit, and wire owners remain emit-free; apps own registration and lifetime.

`GraphInstrument` projects every fact onto the `InstrumentSet` the composing root materializes from `ElementInstruments.Telemetry` — kernel `InstrumentSpec` declarations carrying kind, form, UCUM units, bounded dimensions, and `Buckets` advice. `ConstraintSeverity`, `Discipline`, `AssessmentOutcome`, `AuditCategory`, `WireKind`, and the kernel fault-category band bound tags beside the kernel `TenantContext` partition; `NodeId` and `ContentAddress` ride the span alone, and `AnalysisRoute` rides the typed `AssessmentTouch` census alone — one delta touches N routes and `SetTag` is set-or-replace, so a route slot would report whichever touch folded last.

## [01]-[INDEX]

- [02]-[HOOK_RAIL]: `ElementPoint` closes the point vocabulary on its kernel `Modality` column and derived `TraceScope` plane, `ElementFact` carries the `[Union]` fact family through one `Of` mint with its `Point` and `Marks` projections, `HookGate`/`HookTap` seat the subscriber rows with the tap's own point scope, `ElementHookRail` mints the composition over the kernel point capsule with its evidence cell, clock, admitted `SpanBand`, point census, and held tap detachers, and `ElementTap` decorates the graph, audit, projection, and wire entrypoints.
- [03]-[INSTRUMENT_PROJECTION]: `ElementInstruments` closes the instrument table — kernel `InstrumentSpec` rows, UCUM units, bucket advice, and the one dotted slot vocabulary both planes spell — and mints the contributor port, while `GraphInstrument.Tap` rails every fact into instrument writes over the composition's `InstrumentSet` and `GraphInstrument.Depth` binds the rail's own parked-fault read to the one pulled row.

## [02]-[HOOK_RAIL]

- Owner: `ElementPoint` the `[SmartEnum<string>]` point vocabulary keyed `rasm.element.<domain>.<point>` (the estate four-segment lowercase grammar) with the kernel `HookModality` column and the kernel `TraceScope` plane derived off the id's own head; `ElementFact` the `[Union]` typed fact family over the shared `Op Key` base; `AssessmentTouch` the per-delta assessment census row (the graded `Projection/audit#INTEGRITY_SWEEP` `AuditTally` its audit-side sibling, seated with the taxonomy it grades); `WireKind` the two-row decode-kind vocabulary; `HookGate`/`HookTap` the subscriber rows, `HookTap` carrying its own point scope; `ElementHookRail` the minted composition over the kernel point capsule and the admitted kernel `SpanBand`, owning the point census and the tap-detacher custody; `ElementTap` the decoration capability.
- Cases: `DeltaApplied` (delta content key, node/edge touch counts, header presence, the typed `AssessmentTouch` census — fired at the admission seam, the one `Veto` point) · `Frozen` (snapshot `ContentAddress.OfGraph` key, node/edge population) · `Baked` (element root, fold `Duration`) · `Audited` (the graded snapshot address, the typed `AuditTally` `(category, severity, count)` census with its derived total/blocking/drift reads, fold `Duration`) · `Assembled` (merged-delta content key — the same derivation the `Rasm.Persistence` event dedup reads — projector count, delta magnitude, finding count, pipeline `Duration`) · `Graded` (one `ConstraintFinding` — severity row, kernel fault category, replayable `KeyOf` content address, waived flag) · `Decoded` (`WireKind`, `Option<long>` payload bytes — `None` for a non-seekable stream, never a fabricated length — decoded magnitude, `Duration`); the closed lifecycle-fact family.
- Entry: `ElementFact.Of` discriminates on input shape. `ElementHookRail.Of(key, gates, taps, clock, band)` mints one kernel point per roster row over one shared evidence cell, attaching gates through the capsule's `Veto` (a gate on an observe-only point refuses, the capsule verdict lowered onto the seam band under the mint's own `Op`, and the veto rail carries a transformed fact where a refusal is too coarse) and taps through `Observe` at each tap's OWN declared scope, HOLDING every detacher so `Detach()` releases each subscription the composition attached; `Points` publishes the point census the kernel capsule's mount consumes. `Spanned(at, key, body)` brackets a body in the admitted band under the point's own plane. `Fire(fact, body, span)` resolves the fact's point, stamps the fact's marks on the open span, and delegates to the capsule's guarded fire — vetoes fold, the `Fin<T>` body runs over the ADMITTED fact, taps fan only from its success path. `TapFaults` reads parked subscriber faults. `ElementTap.Admitted`/`Baked`/`Audited`/`Assembled` preserve each seam owner's rail type; `DecodedGraph`/`DecodedDelta` specialize one `Decoded<T>` decoration kernel with decoder, kind, and magnitude projections.
- Auto: `Fire` on an observe-only point has no gates; the capsule's shield captures both throws and returned failures, parking each as a point-attributed `IsolatedFault` with the tap's name folded into the failure detail. `ElementTap.Admitted` vetoes before `AdmitOnto`, emits `DeltaApplied` only after admission succeeds, and emits `Frozen` after the snapshot exists. `Assembled` times the pipeline and emits one `Graded` fact per finding; `Audited` times the one audit fold and fires its receipt's census. Both delta-keyed facts resolve their tolerance through the ONE `delta.Header.IfNone(base)` rule, so a header-establishing delta keys the fact exactly as the Persistence dedup keys the event. `Point` maps each case to its preallocated row and `Marks` to its identifier-grade span evidence, both through the generated dispatch. Marks stamp ahead of the veto fold, so a refused admission leaves the span carrying what was attempted beside the kernel's `Error` status.
- Receipt: a fired `ElementFact` is the evidence event; the emitter rail already carries the outcome. `TapFaults` is retained subscriber-failure evidence a health panel reads. Replay/audit, AppUi, and instrument consumers share tap rows, so observability subscribes to facts and never produces them.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`/`[Union]` + the generated `Switch`/`Map`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Atom`), NodaTime (`Duration`), `Rasm` (the kernel signal capsule, the trace band, `Op`, `FaultExtensions.Category`), BCL (`TimeProvider` the injected monotonic clock, `Activity` the span handle the band hands back).
- Growth: a new lifecycle fact is one `ElementPoint` row and one `ElementFact` case — the generated dispatch breaks every projection (the `[03]` instrument tap and the `Marks` fold included) loudly at compile time, and a point on a new domain segment arms its span plane with no roster edit because the plane derives off the id; a new subscriber is one `HookTap`/`HookGate` row at the app root's mint, narrowing to its own points through the tap's `Scope` column rather than through a `Point`-probing arm inside its delegate; delivery semantics are the kernel modality rows; never a per-point registry sibling, never a process-global rail, and never an emit call inside a graph page.
- Boundary: the rail is a sealed class, so a `with` copy cannot alias the evidence cell. Gates refuse or rewrite the fact's own evidence and never touch structural state — the admitted fact reaches the guarded body and the taps alike, so a redaction lands once rather than per subscriber. Facts emit only after successful bodies; the capsule forks observe taps, so a tap never blocks the seam. Span custody is the kernel band's — this package declares `TraceScope` rows and owns no `ActivitySource`, no listener gate, and no status stamp, so the composing root's `SpanBand.Of(version, scopes)` holds the one source lifetime and a band-less composition runs the identical rail with observability absent rather than degraded. Thrown bodies lower at their own seam owner — `ProjectionAssembly.Assemble`'s boundary funnel — so the decoration mints no second trap. Delta and assembly keys reuse `GraphDelta.ToCanonicalBytes`; frozen keys use `ContentAddress.OfGraph`. Point ids follow the kernel `rasm.<pkg>.<domain>.<point>` grammar and their planes the kernel `rasm.<pkg>.<domain>` grammar, so id and scope are ONE derivation.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.Diagnostics;
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
// with instrumentation scopes and metric names. Modality is the kernel column: the delta admission point admits
// synchronous refusal gates on the emitter's rail; every other point is observe-only.
[SmartEnum<string>]
public sealed partial class ElementPoint {
 public static readonly ElementPoint DeltaApplied = new("rasm.element.graph.delta-applied", modality: HookModality.Veto);
 public static readonly ElementPoint Frozen = new("rasm.element.graph.frozen", modality: HookModality.Observe);
 public static readonly ElementPoint Baked = new("rasm.element.graph.baked", modality: HookModality.Observe);
 // Graph plane, not a plane of its own: the audit grades one frozen snapshot, so its span belongs beside the freeze
 // and the bake that produced what it reads.
 public static readonly ElementPoint Audited = new("rasm.element.graph.audited", modality: HookModality.Observe);
 public static readonly ElementPoint Assembled = new("rasm.element.projection.assembled", modality: HookModality.Observe);
 public static readonly ElementPoint Finding = new("rasm.element.projection.finding", modality: HookModality.Observe);
 public static readonly ElementPoint WireDecoded = new("rasm.element.wire.decoded", modality: HookModality.Observe);

 // Items-derived index materializes on first read, so a bracket pays a lookup rather than re-parsing the id.
 static readonly Lazy<FrozenDictionary<ElementPoint, TraceScope>> Planes = new(
  static () => Items.ToFrozenDictionary(static row => row, static row =>
   TraceScope.Create(value: string.Join('.', row.Key.Split('.')[..3]))),
  LazyThreadSafetyMode.ExecutionAndPublication);

 public HookModality Modality { get; }

 // Span plane is the id's own rasm.<pkg>.<domain> head — the kernel KernelDomain derivation in this package's
 // vocabulary, so a scope can never fork from the point it brackets.
 public TraceScope Plane => Planes.Value[this];

 // Composing roots admit this roster into SpanBand.Of; the roster shares three planes, so the projection
 // deduplicates, and an unadmitted scope refuses on the kernel rail rather than dropping every span silently.
 public static Seq<TraceScope> Scopes => toSeq(Planes.Value.Values).Distinct().Strict();
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
// input shape; Point projects the owning row through the generated Map over the preallocated ElementPoint
// singletons, and Marks projects the same evidence onto the span plane.
[Union]
public abstract partial record ElementFact {
 private ElementFact(Op key) { Key = key; }

 public Op Key { get; }

 public sealed record DeltaApplied(Op Key, ContentAddress Delta, int Nodes, int Edges, bool HeaderEstablished, Seq<AssessmentTouch> Assessments) : ElementFact(Key);
 public sealed record Frozen(Op Key, ContentAddress Snapshot, int Nodes, int Edges) : ElementFact(Key);
 public sealed record Baked(Op Key, NodeId Root, Duration Elapsed) : ElementFact(Key);
 // ONE census column, three derived reads — the counts a dashboard wants are folds over the buckets, never
 // parallel stored columns a producer could file inconsistently with the census beside them.
 public sealed record Audited(Op Key, ContentAddress Snapshot, Seq<AuditTally> Findings, Duration Elapsed) : ElementFact(Key) {
  public int Total => Findings.Fold(0, static (count, tally) => count + tally.Count);
  public int Blocking => Findings.Filter(static tally => tally.Severity.Blocks).Fold(0, static (count, tally) => count + tally.Count);
  // The tamper count read off the census by category, so the drift series and the finding series can never disagree.
  public int Drifts => Findings.Filter(static tally => tally.Category == AuditCategory.AddressDrift).Fold(0, static (count, tally) => count + tally.Count);
 }
 public sealed record Assembled(Op Key, ContentAddress Delta, int Projectors, int Nodes, int Edges, int Findings, Duration Elapsed) : ElementFact(Key);
 public sealed record Graded(Op Key, ConstraintSeverity Severity, string Category, ContentAddress FindingKey, bool Waived) : ElementFact(Key);
 public sealed record Decoded(Op Key, WireKind Kind, Option<long> Bytes, int Nodes, int Edges, Duration Elapsed) : ElementFact(Key);

 public ElementPoint Point => Map(
  deltaApplied: ElementPoint.DeltaApplied,
  frozen: ElementPoint.Frozen,
  baked: ElementPoint.Baked,
  audited: ElementPoint.Audited,
  assembled: ElementPoint.Assembled,
  graded: ElementPoint.Finding,
  decoded: ElementPoint.WireDecoded);

 // Identifier-grade evidence the metric plane refuses — content keys and roots whose cardinality is unbounded on
 // a series and free on a sampler-thinned span — projected per case onto the ONE slot vocabulary [03] declares.
 // Each slot carries ONE semantic because SetTag is set-or-replace and two facts share a span: the delta and the
 // snapshot it settles would collapse onto a single last-wins row under a shared key. Graded is the stream case
 // and stamps nothing — N findings fire inside the assembly's own span, so their evidence belongs to the metric
 // counter and the receipt. Tenancy is absent by design: the app root's baggage promotion already stamps
 // rasm.tenant on every span, so folding TenantContext here double-stamps the partition.
 public Seq<(string Slot, object? Value)> Marks => Switch(
  deltaApplied: static f => Seq<(string Slot, object? Value)>((ElementInstruments.DeltaSlot, f.Delta.ToValue())),
  frozen: static f => Seq<(string Slot, object? Value)>((ElementInstruments.SnapshotSlot, f.Snapshot.ToValue())),
  // NodeId's own Value is the owned spelling — ToString() reads a generated surface the owner never pinned, so a
  // generator default would silently re-shape the span evidence a trace consumer joins on.
  baked: static f => Seq<(string Slot, object? Value)>((ElementInstruments.RootSlot, f.Root.Value)),
  // The audit marks the snapshot it GRADED, so the span joins the audit to the freeze that produced the address.
  audited: static f => Seq<(string Slot, object? Value)>((ElementInstruments.SnapshotSlot, f.Snapshot.ToValue())),
  assembled: static f => Seq<(string Slot, object? Value)>((ElementInstruments.DeltaSlot, f.Delta.ToValue())),
  graded: static _ => Seq<(string Slot, object? Value)>(),
  decoded: static f => Seq<(string Slot, object? Value)>((ElementInstruments.KindSlot, f.Kind.Key)));

 // Delta fact: the content key is the SAME GraphDelta.ToCanonicalBytes derivation the Rasm.Persistence event dedup
 // keys on — one projection, two consumers, never a second spelling. The tolerance takes the delta's OWN resolved
 // header, so the fact key and the dedup key cannot fork on a header-establishing delta.
 public static ElementFact Of(Op key, GraphDelta delta, Header seed) => new DeltaApplied(
  key, ContentAddress.Of(delta.ToCanonicalBytes(Grid(delta, seed)).Span), delta.NodeCount, delta.EdgeCount, delta.Header.IsSome, Touches(delta));

 // Frozen fact: pays the full OfGraph snapshot fold — accepted at the decoration altitude, never a graph-page charge.
 public static ElementFact Of(Op key, ElementGraph graph) => new Frozen(
  key, ContentAddress.OfGraph(graph), graph.Nodes.Count, graph.Edges.Length);

 public static ElementFact Of(Op key, NodeId root, Duration elapsed) => new Baked(key, root, elapsed);

 // Audit fact: the receipt already carries the graded snapshot's address and its finding rows, so the fact folds
 // the census once and re-derives no verdict — the audit is the authority, this is its evidence event.
 public static ElementFact Of(Op key, ModelAudit audit, Duration elapsed) => new Audited(
  key, audit.Snapshot, audit.Tallies, elapsed);

 public static ElementFact Of(Op key, AssemblyReceipt receipt, int projectors, Duration elapsed) => new Assembled(
  key, ContentAddress.Of(receipt.Delta.ToCanonicalBytes(Grid(receipt.Delta, receipt.Graph.Header)).Span),
  projectors, receipt.Delta.NodeCount, receipt.Delta.EdgeCount, receipt.Findings.Count, elapsed);

 // The ONE tolerance resolution both delta-keyed facts read — the SAME `delta.Header.IfNone(base)` rule AdmitOnto,
 // Freeze, and ReplayOnto resolve a header under. Reading a base header directly forked the fact key from the
 // Persistence dedup key on exactly the model-creating delta that establishes the header, where it matters most.
 static double Grid(GraphDelta delta, Header seed) => delta.Header.IfNone(seed).Tolerance;

 public static ElementFact Of(Op key, ConstraintFinding finding) => new Graded(
  key, finding.Severity, finding.Violation.Category, finding.Key, finding.Waived);

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
// Subscriber rows: a gate is the synchronous fact-to-fact fold a Veto point runs on the emitter's rail, so a
// refusal and a redaction are two ends of one rail; a tap is the shielded observer whose Name folds into its
// failure, keeping parked evidence tap-attributed.
public sealed record HookGate(ElementPoint Point, Func<ElementFact, Fin<ElementFact>> Admit);

// Scope is the tap's own point roster, ABSENT meaning every point (the instrument projection's shape — it owns a
// total Switch and wants the whole fan). A single-point subscriber — a replay sink reading admissions, a QA panel
// reading findings — names its rows and pays nothing on the fires it ignores, where the attach-to-everything form
// billed every subscriber for every point and let the delegate's own no-op arm stand in for a subscription policy.
public sealed record HookTap(string Name, Func<ElementFact, Fin<Unit>> Observe, Option<Seq<ElementPoint>> Scope = default);

// Minted composition — a sealed CLASS so no `with` copy can alias the evidence cell (the ElementGraph cache
// discipline); the app composition root builds it ONCE from its gate and tap rows (the ProjectionSuite.Of mint
// precedent), one kernel point per roster row over one shared cell. Clock is the injected monotonic source the
// Timed kernel reads; Band is the composition's kernel SpanBand, absent where no root composed one.
public sealed class ElementHookRail {
 // Keyed on the POINT ROW, not its string: the generated smart-enum equality is the identity the roster already
 // owns, so a lookup cannot miss on a token spelling and the map answers Find rather than a throwing indexer.
 readonly HashMap<ElementPoint, HookPoint<ElementFact>> points;
 // The tap detachers this mint HOLDS: a composition that outlives one subscriber (a drained instrument set, a
 // replaced replay sink) detaches it here rather than leaking a live capsule subscription for the process life.
 readonly Seq<IDisposable> detachers;

 ElementHookRail(HashMap<ElementPoint, HookPoint<ElementFact>> points, Seq<IDisposable> detachers,
  Atom<Seq<IsolatedFault>> faults, TimeProvider clock, Option<SpanBand> band) =>
  (this.points, this.detachers, Faults, Clock, Band) = (points, detachers, faults, clock, band);

 public Atom<Seq<IsolatedFault>> Faults { get; }
 public TimeProvider Clock { get; }
 public Option<SpanBand> Band { get; }

 // The point census the kernel capsule's own mount requires and both peer seams already publish — the composing
 // root reads one member rather than re-deriving the roster from ElementPoint.Items and re-minting the ids.
 public Seq<IHookPoint> Points => points.Values.Map(static point => (IHookPoint)point).ToSeq();

 // Custody exits with the composition: every held detacher releases, so a rail torn down leaves no capsule
 // subscription behind. A bare Iter over a detacher set the mint DROPPED is the leak this member closes.
 public Unit Detach() => detachers.Iter(static detacher => detacher.Dispose());

 // Parked subscriber-fault evidence — a health panel or drain sweep reads it; the emitter never sees it.
 public Seq<IsolatedFault> TapFaults => Faults.Value;

 public static Fin<ElementHookRail> Of(
  Op key, Seq<HookGate> gates = default, Seq<HookTap> taps = default,
  Option<TimeProvider> clock = default, Option<SpanBand> band = default) {
  Atom<Seq<IsolatedFault>> faults = Atom(Seq<IsolatedFault>());
  HashMap<ElementPoint, HookPoint<ElementFact>> points = toSeq(ElementPoint.Items)
   .Fold(HashMap<ElementPoint, HookPoint<ElementFact>>(), (held, row) =>
    held.Add(row, new HookPoint<ElementFact>(id: HookId.Create(value: row.Key), modality: row.Modality, faults: faults)));
  // Veto admission rails (an observe-only point refuses a gate), the capsule refusal lowered onto the seam band
  // so a composition failure reads its origin from band 2500 like every other entrypoint. Gate verdicts precede
  // every attachment, and each Observe detacher is KEPT so the composition can release what it attached.
  return gates.Fold(Fin.Succ(unit), (state, gate) => state.Bind(_ =>
    points.Find(gate.Point).Match(
     Some: point => point.Veto(gate: gate.Admit).Map(static _ => unit),
     None: () => ElementFault.ValueRejected(key, $"<hook-gate-point-absent:{gate.Point.Key}>"))))
   .MapFail(error => ElementFault.ValueRejected(key, $"<hook-gate-unadmitted:{error.Message}>"))
   .Map(_ => taps.Bind(tap => Scoped(points, tap).Map(point => point.Observe(tap: Adapted(tap)))).ToSeq())
   .Map(held => new ElementHookRail(
     points: points, detachers: held, faults: faults, clock: clock.IfNone(TimeProvider.System), band: band));
 }

 // A scoped tap attaches to exactly its named rows; an unscoped one to the whole fan. Every named row resolves by
 // construction — the map is built from ElementPoint.Items and the roster is closed — so Choose drops nothing and
 // needs no refusal arm beside the gate's.
 static Seq<HookPoint<ElementFact>> Scoped(HashMap<ElementPoint, HookPoint<ElementFact>> points, HookTap tap) =>
  tap.Scope.Match(
   Some: rows => rows.Choose(points.Find),
   None: () => points.Values.ToSeq());

 // ONE span bracket for every decoration: the admitted band opens the point's own plane and owns the listener
 // gate, the `using` close, and the typed fail-leg status; a band-less composition runs the identical rail with
 // a null span, so observability is additive and never a precondition.
 public Fin<T> Spanned<T>(ElementPoint at, Op key, Func<Activity?, Fin<T>> body) =>
  Band.Match(Some: band => band.Traced(at.Plane, key, body), None: () => body(null));

 // Veto fold, guarded body over the ADMITTED fact, then the capsule's forked tap fan — all one delegated fire;
 // failed bodies emit no success fact, and a transforming gate governs the body and the taps from one rewrite.
 // Marks stamp AHEAD of the fold, so a refused admission leaves the span carrying what was attempted, beside
 // whatever Error status the band itself lands.
 // The mark stamp is a STEP, not a discarded tuple leg: a statement body is the honest ordering here (the named
 // boundary exemption — a span tag write is host state, not a domain value), where the tuple idiom hid the
 // sequencing in an element position a reader has to evaluate to find. An unrostered point is unrepresentable —
 // Point projects a roster row — so the Find lands its Some arm by construction.
 public Fin<T> Fire<T>(ElementFact fact, Func<ElementFact, Fin<T>> body, Activity? span = null) {
  Stamp(span, fact.Marks);
  return points.Find(fact.Point).Match(
   Some: point => point.Fire(fact: fact, body: body),
   None: () => body(fact));
 }

 // Name-attributed adaptation onto the capsule tap shape: a returned failure re-raises carrying the tap's
 // Name, so the parked IsolatedFault detail stays attributable; a THROW is the capsule shield's capture.
 static Func<ElementFact, IO<Unit>> Adapted(HookTap tap) =>
  fact => tap.Observe(fact).Match(
   Succ: static _ => IO.pure(unit),
   Fail: error => IO.fail<Unit>(Error.New($"<hook-tap-faulted:{tap.Name}:{error.Message}>")));

 static void Stamp(Activity? span, Seq<(string Slot, object? Value)> marks) =>
  marks.Iter(mark => ignore(span?.SetTag(mark.Slot, mark.Value)));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// Seam-owned decoration capability (the apps own the wiring — the ASSEMBLE_CAPABILITY split): each entry
// composes span + fire + the REAL seam entrypoint and returns exactly that owner's rail type, so graph pages
// stay emit-free and an app root wires one call rather than nesting a bracket around a decoration.
// Facts fire on success alone; a failed fold already rails its fault.
public static class ElementTap {
 // Veto precedes AdmitOnto; both DeltaApplied and Frozen taps run only after structural admission succeeds,
 // and one graph-plane span covers the admission and the snapshot it settles.
 public static Fin<(ElementGraph Graph, GraphDelta Delta)> Admitted(ElementHookRail rail, GraphDelta delta, ElementGraph seed, Op key) =>
  rail.Spanned(ElementPoint.DeltaApplied, key, span =>
   rail.Fire(ElementFact.Of(key, delta, seed.Header), _ => delta.AdmitOnto(seed, key), span)
    .Bind(step => rail.Fire(ElementFact.Of(key, step.Graph), _ => Fin.Succ(step), span)));

 public static Fin<Element> Baked(ElementHookRail rail, ElementGraph graph, NodeId root, Op key) =>
  Timed(rail, ElementPoint.Baked, key, () => graph.Bake(root, key), (_, elapsed) => ElementFact.Of(key, root, elapsed));

 // The audit decoration times the one fold and fires its receipt — the graph-plane sibling of Baked, so a delivery
 // gate reading ModelAudit and a dashboard reading the audit series see the same run.
 public static Fin<ModelAudit> Audited(ElementHookRail rail, ElementGraph graph, Op key) =>
  Timed(rail, ElementPoint.Audited, key, () => ModelAudit.Of(graph, key), (audit, elapsed) => ElementFact.Of(key, audit, elapsed));

 // Assembled times the whole pipeline, then fires one Graded fact per receipt finding — warnings and waived
 // deviations included, the same evidence stream AssemblyReceipt.Findings persists — all under one span, so a
 // finding's replay key reaches the trace beside the run that graded it.
 public static Fin<AssemblyReceipt> Assembled(ElementHookRail rail, ProjectionSuite suite, ElementGraph seed, ProjectionContext ctx) =>
  Timed(rail, ElementPoint.Assembled, ctx.Key, () => ProjectionAssembly.Assemble(suite, seed, ctx),
   (receipt, elapsed) => ElementFact.Of(ctx.Key, receipt, suite.Projectors.Count, elapsed),
   (span, receipt) => receipt.Findings
    .TraverseM(finding => rail.Fire(ElementFact.Of(ctx.Key, finding), _ => Fin.Succ(unit), span)).As()
    .Map(_ => receipt));

 public static Fin<ElementGraph> DecodedGraph(ElementHookRail rail, Stream payload, WireLimits limits, Op key) =>
  Decoded(rail, payload, limits, key, WireKind.Snapshot,
   decode: static (stream, bounds, op) => ElementWire.DecodeGraph(stream, bounds, op),
   magnitude: static graph => (graph.Nodes.Count, graph.Edges.Length));

 public static Fin<GraphDelta> DecodedDelta(ElementHookRail rail, Stream payload, WireLimits limits, Op key) =>
  Decoded(rail, payload, limits, key, WireKind.Delta,
   decode: static (stream, bounds, op) => ElementWire.DecodeDelta(stream, bounds, op),
   magnitude: static delta => (delta.NodeCount, delta.EdgeCount));

 static Fin<T> Decoded<T>(
  ElementHookRail rail, Stream payload, WireLimits limits, Op key, WireKind kind,
  Func<Stream, WireLimits, Op, Fin<T>> decode, Func<T, (int Nodes, int Edges)> magnitude) =>
  Timed(rail, ElementPoint.WireDecoded, key, () => decode(payload, limits, key), (value, elapsed) => magnitude(value) switch {
   var (nodes, edges) => ElementFact.Of(key, kind, Length(payload), nodes, edges, elapsed),
  });

 // ONE monotonic timing kernel every timed decoration composes, bracketed by the point's own span — the
 // timestamp read precedes the body, the named statement seam confined here; elapsed derives from the rail's
 // injected TimeProvider, never a wall-clock diff. `fan` is the per-decoration continuation over the STILL-OPEN
 // span, so derived facts land their writes inside the trace and each carries the pinned trace-based exemplar.
 // Nullability carries the optional continuation because the language forces it: a lambda argument has no type
 // until it converts, so an Option-wrapped delegate parameter rejects every call site passing one inline.
 static Fin<T> Timed<T>(
  ElementHookRail rail, ElementPoint at, Op key, Func<Fin<T>> body, Func<T, Duration, ElementFact> fact,
  Func<Activity?, T, Fin<T>>? fan = null) {
  long start = rail.Clock.GetTimestamp();
  return rail.Spanned(at, key, span => body().Bind(value => rail.Fire(
    fact(value, Duration.FromTimeSpan(rail.Clock.GetElapsedTime(start))), _ => Fin.Succ(value), span))
   .Bind(value => fan is null ? Fin.Succ(value) : fan(span, value)));
 }

 // Typed absence over a non-seekable stream — a fabricated length is false evidence.
 static Option<long> Length(Stream payload) => payload.CanSeek ? Some(payload.Length) : None;
}
```

## [03]-[INSTRUMENT_PROJECTION]

- Law: [PULL_POLARITY] — a measurement is PUSHED unless the cell it reports is process-scoped AND has no fire to ride. Graph population stays EVENT-shaped for a reason, not for want of the kernel level family: a live population cell needs a bounded key, and this owner's key spaces refuse one from both ends — `Root` and `Snapshot` are identifier-grade and unbounded (which is why they seat as span marks, never series dimensions), while discipline, outcome, severity, waiver, and wire kind partition findings and decodes and never a graph's size. An unkeyed scalar level over MANY graphs republishes whichever root froze last as current state, and a retired owner's reading is indistinguishable from a live one, so `Frozen` histograms are the honest shape with each freeze contributing its own measured population. The `TapFaults` depth is the one row that clears the bar: a rail's evidence cell is composition-scoped (one cell, not one per graph) and a parked subscriber fault is captured by the capsule shield without ever firing a fact, so no push site exists at all — it binds through the kernel's registered read for the rail's own lifetime and retires with it.
- Owner: `ElementInstruments` the closed `rasm.element.*` roster — kernel `InstrumentSpec` declarations carrying kind, measurement form, UCUM units, kernel `Buckets` advice, the closed per-instrument dimension vocabularies, and the one dotted slot block both the metric rows and the `[02]` span marks spell — with the kernel-sourced scope identity and the string-scoped contributor-port mint; `GraphInstrument` the fact-to-write projection over the `InstrumentSet` the composing root materializes.
- Entry: `ElementInstruments.Telemetry(version, schemaUrl)` is the contributor port the composing root materializes — the semconv coordinate defaults to the kernel pin so all three signals bump together — and a root outside that fan binds `InstrumentSet.Of(cells, (meter, ElementInstruments.Rows))` directly against its own minted meter; either path, never both. `GraphInstrument.Tap(set)` returns the `HookTap` passed to `ElementHookRail.Of`, handing the kernel's write rail straight to the capsule shield; `GraphInstrument.Depth(set, rail)` registers the rail's own parked-fault read against the one pulled row and returns the scope that retires it, so the composing root arms it AFTER the mint the tap fed.
- Auto: `DeltaApplied` counts the two delta magnitudes and one `rasm.element.assessment.outcomes` per census touch (discipline and outcome dimensions — both closed rows); `Frozen` records the snapshot node/edge population histograms; `Baked`/`Audited`/`Assembled` record the duration histograms; `Audited` additionally counts one `rasm.element.audit.findings` per `AuditTally` bucket (integrity category and severity dimensions) and writes the drift count UNCONDITIONALLY — a clean run posts its own zero, so the tamper series never leaves an alert unable to tell a verified snapshot from an unaudited one; `Graded` counts findings under the kernel fault-category slot, severity, and waiver — the same key the kernel fault counter bands on, so one query answers which failure class burns across the estate; `Decoded` records duration and — when the payload length is known — size, both under the `WireKind` dimension; the kernel `TenantContext.Current` resolves ONCE per fact and threads as dispatch state, so every write of one fact lands under one partition, a root-tenant process mints no tenant dimension, and a partitioned one mints it uniformly — the ambient read is the AsyncLocal slot the kernel owns, never a value captured at tap-mint time, which would stamp the composing root's tenant onto every later request; instrument identity de-duplicates by name inside the one meter, so name, unit, kind, and description are declaration facts the row carries once.
- Receipt: none — the projection is a pure fold of the fact tap; a metric minted beside it is a second truth, and every operational dashboard reads the exported stream, never a seam cell. `InstrumentSet.Write` rails an unmounted name or a family mismatch out to the tap shield, which parks it point-attributed, so a mount defect is visible rather than a silent measurement drop.
- Packages: BCL `System.Diagnostics.Metrics` reached through the kernel capsule alone, `Rasm` (the kernel instrument mechanism, the scope identity roster, the fault-category slot, and the tenancy frame), Thinktecture.Runtime.Extensions (the generated fact `Switch`), LanguageExt.Core.
- Growth: a new metric is one `ElementInstruments` `InstrumentSpec` row and one write in the owning `Switch` arm — a new fact case breaks the tap at compile time, so an unprojected fact is a build error, never a silent gap; a new PULLED row is one `Level` declaration plus one `Bind` registration on the owner whose lifetime bounds it, admitted only under `[PULL_POLARITY]`; a new instrument family is one kernel `InstrumentKind` row, a new bucket policy one kernel `Buckets` row; a new span attribute is one slot row here and one `Marks` arm at `[02]`; never an inline `new Meter(...)`, never a create or write call outside this fence, and never a numeric value as a tag.
- Boundary: this fence is the package telemetry spine and the only declaration and write site — the create bodies belong to the kernel's `InstrumentKind` x `MeasureForm` derivation, so a re-spelled counter or histogram create here is the forked-stream defect. Closed seam vocabularies bound every tag; slot keys carry the package's own dotted `rasm.element.<dimension>` namespace so a concept a second package also tags never collides, and the fault-category band reads the kernel slot rather than re-declaring one. Opaque routes and identities never become tags — they ride the `[02]` span marks — and the tenant slot every row declares is the ONE dimension whose presence the write decides rather than the declaration: `TenantContext.Key` reads `None` at the root row, so `Tags` projects empty and the series exports untagged on the SAME instrument a partitioned process exports keyed. That is the kernel's one absence discriminant governing both the tenancy and level planes, so the declared roster stays uniform, no row carries an optionality column of its own, and a governance view reading `Dimensions` for its tag keys must tolerate the absent entry rather than mint a second stream for it. Provider, exporter, views, exemplars, and base2-exponential defaults remain composition-root policy, and meter lifetime rides the minting factory at that root, so this page holds no `IMeterFactory`, no `Meter`, and no disposable. Memo-hit dimensions remain absent until `Bake` exposes that evidence.

```csharp signature
// --- [TABLES] -----------------------------------------------------------------------------
// Closed roster — instrument-name and slot constants are the canonical spellings the rows, the projection arms,
// and the [02] span marks all compose (one owner, zero drift between a declared dimension and its write-site
// tag). Kind and MeasureForm are the row's own columns, so the kernel derives every create body and this page
// spells none; advice bounds read the kernel Buckets rows, and every row declares the kernel tenant slot the
// write stamps. Scope is the kernel's minted package identity, never a re-spelled meter-name literal.
public static class ElementInstruments {
 public static readonly string Scope = TelemetrySource.Element.Key;

 public const string DeltaNodes = "rasm.element.graph.delta.nodes";
 public const string DeltaEdges = "rasm.element.graph.delta.edges";
 public const string GraphNodes = "rasm.element.graph.nodes";
 public const string GraphEdges = "rasm.element.graph.edges";
 public const string BakeDuration = "rasm.element.graph.bake.duration";
 public const string AuditDuration = "rasm.element.audit.duration";
 public const string AuditFindings = "rasm.element.audit.findings";
 // The tamper gate's OWN series: ContentAddress.Verify drift is a security signal a dashboard alerts on, so it
 // never dissolves into one category bucket of the coverage-quality counter beside it.
 public const string AddressDrift = "rasm.element.audit.drift";
 public const string AssembleDuration = "rasm.element.projection.assemble.duration";
 public const string Findings = "rasm.element.projection.findings";
 public const string AssessmentOutcomes = "rasm.element.assessment.outcomes";
 // The rail's parked-fault depth — the ONE pulled row on this roster, earning its seat under the [03] level law.
 public const string TapFaults = "rasm.element.observe.tap.faults";
 public const string WireDuration = "rasm.element.wire.duration";
 // Size, never bytes: the estate name grammar carries no unit suffix and the UCUM By unit states the measure.
 public const string WireSize = "rasm.element.wire.size";

 // Dimension slots carry this package's dotted namespace — a bare noun forks the moment a sibling package tags
 // `outcome` or `category`, which two already do estate-wide. Delta, Snapshot, and Root stay SPAN-only:
 // a content address and a node id are identifier-grade and unbounded on a series.
 // The AUDIT category axis is this package's own closed vocabulary, distinct from the kernel FAULT-category slot the
 // finding counter bands on — an integrity sweep and a failure class are two taxonomies, and one slot for both would
 // make "orphan" and "Value" siblings on a chart neither answers.
 public const string AuditSlot = "rasm.element.audit.category";
 public const string DeltaSlot = "rasm.element.delta";
 public const string DisciplineSlot = "rasm.element.discipline";
 public const string KindSlot = "rasm.element.wire.kind";
 public const string OutcomeSlot = "rasm.element.assessment.outcome";
 public const string RootSlot = "rasm.element.root";
 public const string SeveritySlot = "rasm.element.severity";
 public const string SnapshotSlot = "rasm.element.snapshot";
 public const string WaivedSlot = "rasm.element.waived";

 public static readonly Seq<InstrumentSpec> Rows = Seq(
  InstrumentSpec.Count(DeltaNodes, "{node}", "node touches per applied delta", MeasureForm.Whole, TenantContext.TenantSlot),
  InstrumentSpec.Count(DeltaEdges, "{edge}", "edge touches per applied delta", MeasureForm.Whole, TenantContext.TenantSlot),
  InstrumentSpec.Advised(GraphNodes, "{node}", "frozen snapshot node population", MeasureForm.Whole, Buckets.GraphCounts, TenantContext.TenantSlot),
  InstrumentSpec.Advised(GraphEdges, "{edge}", "frozen snapshot edge population", MeasureForm.Whole, Buckets.GraphCounts, TenantContext.TenantSlot),
  InstrumentSpec.Advised(BakeDuration, "s", "Bake fold wall duration per element root", MeasureForm.Real, Buckets.FoldSeconds, TenantContext.TenantSlot),
  InstrumentSpec.Advised(AuditDuration, "s", "ModelAudit fold wall duration per graded snapshot", MeasureForm.Real, Buckets.FoldSeconds, TenantContext.TenantSlot),
  InstrumentSpec.Count(AuditFindings, "{finding}", "model-audit findings by integrity category and severity", MeasureForm.Whole, TenantContext.TenantSlot, AuditSlot, SeveritySlot),
  InstrumentSpec.Count(AddressDrift, "{node}", "content-verification drifts per audited snapshot", MeasureForm.Whole, TenantContext.TenantSlot),
  InstrumentSpec.Level(TapFaults, "{fault}", "parked subscriber faults held on a rail's evidence cell", MeasureForm.Whole, TenantContext.TenantSlot),
  InstrumentSpec.Advised(AssembleDuration, "s", "Assemble pipeline wall duration per run", MeasureForm.Real, Buckets.FoldSeconds, TenantContext.TenantSlot),
  InstrumentSpec.Count(Findings, "{finding}", "graded constraint findings by fault category, severity, and waiver", MeasureForm.Whole, TenantContext.TenantSlot, KernelInstruments.CategorySlot, SeveritySlot, WaivedSlot),
  InstrumentSpec.Count(AssessmentOutcomes, "{assessment}", "assessment node touches by discipline and outcome", MeasureForm.Whole, TenantContext.TenantSlot, DisciplineSlot, OutcomeSlot),
  InstrumentSpec.Advised(WireDuration, "s", "wire decode wall duration by kind", MeasureForm.Real, Buckets.FoldSeconds, TenantContext.TenantSlot, KindSlot),
  InstrumentSpec.Advised(WireSize, "By", "wire payload size by kind", MeasureForm.Whole, Buckets.PayloadBytes, TenantContext.TenantSlot, KindSlot));

 public static TelemetryContributorPort Telemetry(string version, string schemaUrl = TelemetryIdentity.SchemaUrl) =>
  new(Scope: Scope, Version: version, Instruments: Rows, Planes: ElementPoint.Scopes, SchemaUrl: schemaUrl);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// Fact-to-write projection over the composition's InstrumentSet — no minted state, so provider disposal owns
// instrument lifetime and this owner holds nothing to dispose.
public static class GraphInstrument {
 // One hook-rail subscription — the app root passes it into ElementHookRail.Of beside its own taps; the
 // projection's typed refusal rides straight out, so the capsule shield parks it as tap-attributed evidence.
 // Unscoped by construction: the projection owns a TOTAL Switch, so it wants every point and a scope row here would
 // be a second, drift-prone statement of the same totality the compiler already enforces.
 public static HookTap Tap(InstrumentSet set) => new("rasm.element.instruments", fact => Project(set, fact));

 // The rail's parked-fault depth is the one measurement no fact can carry — a subscriber failure is captured by the
 // capsule shield and never fires a fact, so a pushed row would have no write site at all. The kernel's registered
 // read is the answer: the rail hands in its OWN Seq read for its own lifetime and the returned scope retires it
 // with the composition, so the depth is live without a call site pushing one. The tenant resolves ONCE at bind and
 // that is CORRECT here where the per-fact read is not — a rail is composition-scoped, not request-scoped, so its
 // evidence cell belongs to the partition that composed it.
 public static Fin<IDisposable> Depth(InstrumentSet set, ElementHookRail rail) =>
  set.Bind(ElementInstruments.TapFaults, () => (long)rail.TapFaults.Count, InstrumentSet.Tags(TenantContext.Current));

 // Total generated dispatch — a new ElementFact case breaks this tap at compile time, so an unprojected fact is
 // a build error; every tag key is a row's declared closed dimension, and every write rides the kernel rail, so
 // an unmounted name and a measurement-form mismatch surface as refusals rather than silent drops. An arm whose
 // writes share a tag set binds it once at the head, so the shared partition is folded per fact, never per write.
 // The ambient partition resolves ONCE per fact and threads as state beside the mounted set: `TenantContext.Current`
 // is the kernel's AsyncLocal slot, so a per-write read lets two writes of ONE fact land under two partitions when
 // the flow re-enters mid-projection, and a census loop pays the resolve per touch.
 static Fin<Unit> Project(InstrumentSet set, ElementFact fact) =>
  fact.Switch<(InstrumentSet Rows, TenantContext Tenant), Fin<Unit>>(
  state: (set, TenantContext.Current),
  deltaApplied: static (state, f) =>
   from shared in Fin.Succ(InstrumentSet.Tags(state.Tenant))
   from nodes in state.Rows.Write(ElementInstruments.DeltaNodes, (long)f.Nodes, shared)
   from edges in state.Rows.Write(ElementInstruments.DeltaEdges, (long)f.Edges, shared)
   from census in f.Assessments.TraverseM(touch => state.Rows.Write(ElementInstruments.AssessmentOutcomes, 1L, InstrumentSet.Tags(state.Tenant,
     (ElementInstruments.DisciplineSlot, touch.Discipline.Key),
     (ElementInstruments.OutcomeSlot, touch.Outcome.Key)))).As()
   select unit,
  frozen: static (state, f) =>
   from shared in Fin.Succ(InstrumentSet.Tags(state.Tenant))
   from nodes in state.Rows.Write(ElementInstruments.GraphNodes, (long)f.Nodes, shared)
   from edges in state.Rows.Write(ElementInstruments.GraphEdges, (long)f.Edges, shared)
   select edges,
  baked: static (state, f) => state.Rows.Write(ElementInstruments.BakeDuration, f.Elapsed.TotalSeconds, InstrumentSet.Tags(state.Tenant)),
  // The drift row writes UNCONDITIONALLY, zero included: a tamper series whose absence and whose clean run look
  // alike gives an alert nothing to rest on, so a verified-clean snapshot posts its own zero.
  audited: static (state, f) =>
   from shared in Fin.Succ(InstrumentSet.Tags(state.Tenant))
   from wall in state.Rows.Write(ElementInstruments.AuditDuration, f.Elapsed.TotalSeconds, shared)
   from drift in state.Rows.Write(ElementInstruments.AddressDrift, (long)f.Drifts, shared)
   from census in f.Findings.TraverseM(tally => state.Rows.Write(ElementInstruments.AuditFindings, (long)tally.Count, InstrumentSet.Tags(state.Tenant,
     (ElementInstruments.AuditSlot, tally.Category.Key),
     (ElementInstruments.SeveritySlot, tally.Severity.Key)))).As()
   select unit,
  assembled: static (state, f) => state.Rows.Write(ElementInstruments.AssembleDuration, f.Elapsed.TotalSeconds, InstrumentSet.Tags(state.Tenant)),
  // The kernel FAULT-category slot here, the package's own AuditSlot on the audit arm — a constraint violation
  // bands with every other failure class estate-wide, an integrity sweep with its own taxonomy.
  graded: static (state, f) => state.Rows.Write(ElementInstruments.Findings, 1L, InstrumentSet.Tags(state.Tenant,
   (KernelInstruments.CategorySlot, f.Category),
   (ElementInstruments.SeveritySlot, f.Severity.Key),
   (ElementInstruments.WaivedSlot, f.Waived))),
  // Absent length is success, never a fabricated zero: the duration write stands alone on a non-seekable stream.
  decoded: static (state, f) =>
   from shared in Fin.Succ(InstrumentSet.Tags(state.Tenant, (ElementInstruments.KindSlot, f.Kind.Key)))
   from wall in state.Rows.Write(ElementInstruments.WireDuration, f.Elapsed.TotalSeconds, shared)
   from size in f.Bytes.Match(
    Some: bytes => state.Rows.Write(ElementInstruments.WireSize, bytes, shared),
    None: static () => Fin.Succ(unit))
   select size);

}
```

## [04]-[RESEARCH]

(none)
