# [ELEMENT_OBSERVE]

`Rasm.Element` observability is one app-minted `ElementHookRail` and one subscribed `GraphInstrument`, both compositions of the kernel signal capsule. `HookPoint` closes the `rasm.element.<domain>.<point>` roster with a kernel modality column; `ElementFact` carries each point's `Op`, content key, and payload. `graph.delta-applied` alone admits veto gates before structural admission. Taps run only after the guarded seam operation succeeds, and thrown or railed subscriber faults park as point-attributed `IsolatedFault` rows in `TapFaults` without changing the emitter result.

`ElementTap` decorates `GraphDelta.AdmitOnto`, `ElementGraph.Bake`, `ProjectionAssembly.Assemble`, and both wire decoders. Graph, delta, and wire owners remain emit-free; apps own registration and lifetime.

`GraphInstrument` mints through the kernel identity entry over the injected `IMeterFactory`, owns the minted disposable `ActivitySource`, and binds `ElementInstruments.Rows` — kernel `InstrumentRow` declarations carrying UCUM units, bounded dimensions, and kernel `Buckets` advice. `ConstraintSeverity`, `Discipline`, `AssessmentOutcome`, and `WireKind` bound tags; `AnalysisRoute`, `NodeId`, and `ContentAddress` never become tags. `Mount` rails invalid coordinates or platform failure, and `Traced` brackets heavy folds for app-tier exemplar/profile correlation.

## [01]-[INDEX]

- [02]-[HOOK_RAIL]: the `HookPoint` six-row point vocabulary with its kernel `Modality` column, the `ElementFact` `[Union]` typed fact family with the one polymorphic `Of` mint and the `Point` projection, the `HookGate`/`HookTap` subscriber rows, the `ElementHookRail` minted composition over the kernel point capsule with its shared evidence cell, and the `ElementTap` seam-owned decoration capability over the graph, projection, and wire entrypoints.
- [03]-[INSTRUMENT_PROJECTION]: the `ElementInstruments` closed instrument table (kernel rows, UCUM units, kernel bucket advice, canonical name and tag-key constants, span names), the contributor-port mint, and the `GraphInstrument` mounted capsule — `Mount` off the injected `IMeterFactory` through the kernel identity entry, `AsTap` the hook-rail subscription projecting every fact into instrument writes, and `Traced` the `ActivitySource` span bracket.

## [02]-[HOOK_RAIL]

- Owner: `HookPoint` the `[SmartEnum<string>]` point vocabulary keyed `rasm.element.<domain>.<point>` (the estate four-segment lowercase grammar) with the kernel `HookModality` column; `ElementFact` the `[Union]` typed fact family over the shared `Op Key` base; `AssessmentTouch` the per-delta assessment census row; `WireKind` the two-row decode-kind vocabulary; `HookGate`/`HookTap` the subscriber rows; `ElementHookRail` the minted composition over the kernel point capsule; `ElementTap` the decoration capability.
- Cases: `DeltaApplied` (delta content key, node/edge touch counts, header presence, the typed `AssessmentTouch` census — fired at the admission seam, the one `Veto` point) · `Frozen` (snapshot `ContentAddress.OfGraph` key, node/edge population) · `Baked` (element root, fold `Duration`) · `Assembled` (merged-delta content key — the same derivation the `Rasm.Persistence` event dedup reads — projector count, delta magnitude, finding count, pipeline `Duration`) · `Graded` (one `ConstraintFinding` — severity row, kernel `Category`, replayable `KeyOf` content key, waived flag) · `Decoded` (`WireKind`, `Option<long>` payload bytes — `None` for a non-seekable stream, never a fabricated length — decoded magnitude, `Duration`); the closed lifecycle-fact family.
- Entry: `ElementFact.Of` discriminates on input shape. `ElementHookRail.Of(key, gates, taps, clock)` mints one kernel point per roster row over one shared evidence cell, attaching gates through the capsule's `Veto` (a gate on an observe-only point is the capsule's typed refusal) and taps through `Observe`. `Fire(fact, body)` resolves the fact's point and delegates to the capsule's guarded fire — vetoes fold, the `Fin<T>` body runs, taps fan only from its success path. `TapFaults` reads parked subscriber faults. `ElementTap.Admitted`/`Baked`/`Assembled` preserve each seam owner's rail type; `DecodedGraph`/`DecodedDelta` specialize one `Decoded<T>` decoration kernel with decoder, kind, and magnitude projections.
- Auto: `Fire` on an observe-only point has no gates; the capsule's shield captures both throws and returned failures, parking each as a point-attributed `IsolatedFault` with the tap's name folded into the failure detail. `ElementTap.Admitted` vetoes before `AdmitOnto`, emits `DeltaApplied` only after admission succeeds, and emits `Frozen` after the snapshot exists. `Assembled` times the pipeline and emits one `Graded` fact per finding. `Point` maps each case to its preallocated row.
- Receipt: a fired `ElementFact` is the evidence event; the emitter rail already carries the outcome. `TapFaults` is retained subscriber-failure evidence a health panel reads. Replay/audit, AppUi, and instrument consumers share tap rows, so observability subscribes to facts and never produces them.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`/`[Union]` + the generated `Switch`/`Map`), LanguageExt.Core (`Fin`/`Seq`/`Option`/`Atom`), NodaTime (`Duration`), `Rasm` (the kernel signal capsule, `Op`, `FaultExtensions.Category`), BCL (`TimeProvider` the injected monotonic clock).
- Growth: a new lifecycle fact is one `HookPoint` row and one `ElementFact` case — the generated dispatch breaks every projection (the `[03]` instrument tap included) loudly at compile time; a new subscriber is one `HookTap`/`HookGate` row at the app root's mint; delivery semantics are the kernel modality rows; never a per-point registry sibling, never a process-global rail, and never an emit call inside a graph page.
- Boundary: the rail is a sealed class, so a `with` copy cannot alias the evidence cell. Gates refuse facts but never replace or transform structural state. Facts emit only after successful bodies; the capsule forks observe taps, so a tap never blocks the seam. Delta and assembly keys reuse `GraphDelta.ToCanonicalBytes`; frozen keys use `ContentAddress.OfGraph`. Hook ids follow the kernel `rasm.<pkg>.<domain>.<point>` grammar.

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
// with instrumentation scopes and metric names. Modality is the kernel column: the delta admission point admits
// synchronous refusal gates on the emitter's rail; every other point is observe-only.
[SmartEnum<string>]
public sealed partial class HookPoint {
 public static readonly HookPoint DeltaApplied = new("rasm.element.graph.delta-applied", modality: HookModality.Veto);
 public static readonly HookPoint Frozen = new("rasm.element.graph.frozen", modality: HookModality.Observe);
 public static readonly HookPoint Baked = new("rasm.element.graph.baked", modality: HookModality.Observe);
 public static readonly HookPoint Assembled = new("rasm.element.projection.assembled", modality: HookModality.Observe);
 public static readonly HookPoint Finding = new("rasm.element.projection.finding", modality: HookModality.Observe);
 public static readonly HookPoint WireDecoded = new("rasm.element.wire.decoded", modality: HookModality.Observe);

 public HookModality Modality { get; }
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
// Subscriber rows: a gate is the synchronous refusal a Veto point folds on the emitter's rail; a
// tap is the shielded observer, its Name folded into the failure so parked evidence stays tap-attributed.
public sealed record HookGate(HookPoint Point, Func<ElementFact, Fin<Unit>> Admit);

public sealed record HookTap(string Name, Func<ElementFact, Fin<Unit>> Observe);

// Minted composition — a sealed CLASS so no `with` copy can alias the evidence cell (the ElementGraph cache
// discipline); the app composition root builds it ONCE from its gate and tap rows (the ProjectionSuite.Of mint
// precedent), one kernel point per roster row over one shared cell. Clock is the injected monotonic source the
// Timed kernel reads.
public sealed class ElementHookRail {
 readonly HashMap<string, HookPoint<ElementFact>> points;

 ElementHookRail(HashMap<string, HookPoint<ElementFact>> points, Atom<Seq<IsolatedFault>> faults, TimeProvider clock) =>
  (this.points, Faults, Clock) = (points, faults, clock);

 public Atom<Seq<IsolatedFault>> Faults { get; }
 public TimeProvider Clock { get; }

 // Parked subscriber-fault evidence — a health panel or drain sweep reads it; the emitter never sees it.
 public Seq<IsolatedFault> TapFaults => Faults.Value;

 public static Fin<ElementHookRail> Of(Op key, Seq<HookGate> gates = default, Seq<HookTap> taps = default, Option<TimeProvider> clock = default) {
  Atom<Seq<IsolatedFault>> faults = Atom(Seq<IsolatedFault>());
  HashMap<string, HookPoint<ElementFact>> points = toSeq(HookPoint.Items)
   .Fold(HashMap<string, HookPoint<ElementFact>>(), (held, row) =>
    held.Add(row.Key, new HookPoint<ElementFact>(id: HookId.Create(value: row.Key), modality: row.Modality, faults: faults)));
  ElementHookRail rail = new(points: points, faults: faults, clock: clock.IfNone(TimeProvider.System));
  return gates.Fold(Fin.Succ(unit), (state, gate) => state.Bind(_ =>
    rail.points[gate.Point.Key].Veto(gate: fact => gate.Admit(fact).Map(_ => fact)).Map(static _ => unit)))
   .Bind(_ => taps.Fold(Fin.Succ(unit), (registered, tap) => registered.Bind(_ =>
    rail.points.Values.Fold(Fin.Succ(unit), (attached, point) => attached.Bind(_ =>
     point.Observe(tap: Adapted(tap)).Map(static _ => unit))))))
   .Map(_ => rail);
 }

 // Veto fold, guarded body, then the capsule's forked tap fan — all one delegated fire; failed bodies emit
 // no success fact.
 public Fin<T> Fire<T>(ElementFact fact, Func<Fin<T>> body) =>
  points[fact.Point.Key].Fire(fact: fact, body: body);

 // Name-attributed adaptation onto the capsule tap shape: a returned failure re-raises carrying the tap's
 // Name, so the parked IsolatedFault detail stays attributable; a THROW is the capsule shield's capture.
 static Func<ElementFact, IO<Unit>> Adapted(HookTap tap) =>
  fact => tap.Observe(fact).Match(
   Succ: static _ => IO.pure(unit),
   Fail: error => IO.fail<Unit>(Error.New($"<hook-tap-faulted:{tap.Name}:{error.Message}>")));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// Seam-owned decoration capability (the apps own the wiring — the ASSEMBLE_CAPABILITY split): each entry
// composes fire + the REAL seam entrypoint and returns exactly that owner's rail type, so graph pages stay
// emit-free. App composition wraps each decorated entry through GraphInstrument.Traced with its declared span name.
// Facts fire on success alone; a failed fold already rails its fault.
public static class ElementTap {
 // Veto precedes AdmitOnto; both DeltaApplied and Frozen taps run only after structural admission succeeds.
 public static Fin<(ElementGraph Graph, GraphDelta Delta)> Admitted(ElementHookRail rail, GraphDelta delta, ElementGraph seed, Op key) =>
  rail.Fire(ElementFact.Of(key, delta, seed.Header.Tolerance), () => delta.AdmitOnto(seed, key))
   .Bind(step => rail.Fire(ElementFact.Of(key, step.Graph), () => Fin.Succ(step)));

 public static Fin<Element> Baked(ElementHookRail rail, ElementGraph graph, NodeId root, Op key) =>
  Timed(rail, () => graph.Bake(root, key), (_, elapsed) => ElementFact.Of(key, root, elapsed));

 // Assembled times the whole pipeline, then fires one Graded fact per receipt finding — warnings and waived
 // deviations included, the same evidence stream AssemblyReceipt.Findings persists.
 public static Fin<AssemblyReceipt> Assembled(ElementHookRail rail, ProjectionSuite suite, ElementGraph seed, ProjectionContext ctx) =>
  Timed(rail, () => ProjectionAssembly.Assemble(suite, seed, ctx),
    (receipt, elapsed) => ElementFact.Of(ctx.Key, receipt, suite.Projectors.Count, elapsed))
   .Map(receipt => (ignore(receipt.Findings.Iter(finding => rail.Fire(ElementFact.Of(ctx.Key, finding), () => Fin.Succ(unit)))), receipt).Item2);

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
  Timed(rail, () => decode(payload, limits, key), (value, elapsed) => {
   (int nodes, int edges) = magnitude(value);
   return ElementFact.Of(key, kind, Length(payload), nodes, edges, elapsed);
  });

 // ONE monotonic timing kernel every timed decoration composes — the timestamp read precedes the body, the named
 // statement seam confined here; elapsed derives from the rail's injected TimeProvider, never a wall-clock diff.
 static Fin<T> Timed<T>(ElementHookRail rail, Func<Fin<T>> body, Func<T, Duration, ElementFact> fact) {
  long start = rail.Clock.GetTimestamp();
  return body().Bind(value => rail.Fire(
   fact(value, Duration.FromTimeSpan(rail.Clock.GetElapsedTime(start))), () => Fin.Succ(value)));
 }

 // Typed absence over a non-seekable stream — a fabricated length is false evidence.
 static Option<long> Length(Stream payload) => payload.CanSeek ? Some(payload.Length) : None;
}
```

## [03]-[INSTRUMENT_PROJECTION]

- Owner: `ElementInstruments` the closed `rasm.element.*` roster — kernel `InstrumentRow` declarations carrying UCUM units, kernel `Buckets` advice, the closed per-instrument dimension vocabularies, canonical name and tag-key constants, and span names — with the `Counted` bind method group and the string-scoped contributor-port mint; `GraphInstrument` the mounted capsule the `IMeterFactory` injection seam produces over the kernel `InstrumentSet`.
- Entry: `GraphInstrument.Mount(factory, version, schemaUrl, key)` rails invalid version/schema coordinates or platform mint failure, mints the `(ActivitySource, Meter)` pair through the kernel identity entry — the semconv coordinate stamped as `MeterOptions.TelemetrySchemaUrl` — and binds `ElementInstruments.Rows` into one `InstrumentSet`. `ElementInstruments.Telemetry(version, schemaUrl)` is the contributor port an app fan materializes instead — one materialization per composition, never both. `AsTap()` returns the `HookTap` passed to `ElementHookRail.Of`. `Traced(name, body)` traps a thrown body, preserves returned faults, and stamps `ActivityStatusCode.Error` on failure.
- Auto: `DeltaApplied` adds the two delta-magnitude counters and one `rasm.element.assessment.outcomes` count per census touch (discipline and outcome dimensions — both closed rows); `Frozen` records the snapshot node/edge population histograms; `Baked`/`Assembled` record the duration histograms; `Graded` counts findings under severity and waived; `Decoded` records duration and — when the payload length is known — bytes, both under the `WireKind` dimension; instrument identity de-duplicates by name inside the one meter, so name, unit, and description are declaration facts the row carries once.
- Receipt: none — the projection is a pure fold of the fact tap; a metric minted beside it is a second truth, and every operational dashboard reads the exported stream, never a seam cell.
- Packages: BCL `System.Diagnostics.Metrics` and `System.Diagnostics` (`libs/csharp/.api/api-diagnostics-activity.md` — the injected `TimeProvider` clock rides the same catalog), `Rasm` (the kernel instrument mechanism), Thinktecture.Runtime.Extensions (the generated fact `Switch`), LanguageExt.Core.
- Growth: a new metric is one `ElementInstruments` row and one write in the owning `Switch` arm — a new fact case breaks the tap at compile time, so an unprojected fact is a build error, never a silent gap; a new bucket policy is one kernel `Buckets` row; a new span is one name constant and one `Traced` call site at the app root; never an inline `new Meter(...)`, never a create or write call outside this fence, and never a numeric value as a tag.
- Boundary: this fence is the package telemetry spine and the only create/write site. Closed seam vocabularies bound every tag; opaque routes and identities never become tags. Provider, exporter, views, exemplars, and base2-exponential defaults remain composition-root policy. Population is event-shaped because the seam owns no live level cell. Memo-hit dimensions remain absent until `Bake` exposes that evidence, and the `using` span bracket is the platform statement seam.

```csharp signature
// --- [TABLES] -----------------------------------------------------------------------------
// Closed roster — instrument-name and tag-key constants are the canonical spellings both the rows and the
// projection arms compose (one owner, zero drift between a declared dimension and its write-site tag);
// advice bounds read the kernel Buckets rows, and Counted is the counter bind method group.
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

 public static readonly Seq<InstrumentRow> Rows = Seq(
  new InstrumentRow(DeltaNodes, "{node}", "node touches per applied delta", Counted),
  new InstrumentRow(DeltaEdges, "{edge}", "edge touches per applied delta", Counted),
  new InstrumentRow(GraphNodes, "{node}", "frozen snapshot node population",
   static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.GraphCounts)),
  new InstrumentRow(GraphEdges, "{edge}", "frozen snapshot edge population",
   static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.GraphCounts)),
  new InstrumentRow(BakeDuration, "s", "Bake fold wall duration per element root",
   static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.FoldSeconds)),
  new InstrumentRow(AssembleDuration, "s", "Assemble pipeline wall duration per run",
   static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.FoldSeconds)),
  new InstrumentRow(Findings, "{finding}", "graded constraint findings by severity and waiver", Counted, [SeverityTag, WaivedTag]),
  new InstrumentRow(AssessmentOutcomes, "{assessment}", "assessment node touches by discipline and outcome", Counted, [DisciplineTag, OutcomeTag]),
  new InstrumentRow(WireDuration, "s", "wire decode wall duration by kind",
   static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.FoldSeconds), [KindTag]),
  new InstrumentRow(WireBytes, "By", "wire payload size by kind",
   static (meter, name, unit, text) => Buckets.Advised(meter, name, unit, text, Buckets.PayloadBytes), [KindTag]));

 public static TelemetryContributorPort Telemetry(string version, string schemaUrl) =>
  new(Scope: Scope, Version: version, SchemaUrl: schemaUrl, Instruments: Rows);

 static Counter<long> Counted(Meter meter, string name, string unit, string text) =>
  meter.CreateCounter<long>(name, unit, text);
}

// --- [SERVICES] ---------------------------------------------------------------------------
// Mounted capsule: the (ActivitySource, Meter) pair minted once through the kernel identity entry over the
// injected IMeterFactory (provider disposal owns instrument lifetime — never the Meter, never a
// process-static), one kernel InstrumentSet, one tap projecting every fact.
public sealed class GraphInstrument : IDisposable {
 readonly InstrumentSet set;
 readonly ActivitySource source;

 GraphInstrument(InstrumentSet set, ActivitySource source) =>
  (this.set, this.source) = (set, source);

 public static Fin<GraphInstrument> Mount(IMeterFactory factory, string version, string schemaUrl, Op key) =>
  string.IsNullOrWhiteSpace(version) || !Uri.TryCreate(schemaUrl, UriKind.Absolute, out _)
   ? ElementFault.ValueRejected(key, "<instrument-scope-coordinate-invalid>")
   : Try.lift(() => {
      (ActivitySource source, Meter meter) = TelemetryIdentity.Mint(
       factory: factory, scope: ElementInstruments.Scope, version: version, schemaUrl: schemaUrl);
      try {
       return new GraphInstrument(set: InstrumentSet.Of(meter: meter, rows: ElementInstruments.Rows), source: source);
      }
      catch {
       source.Dispose();
       throw;
      }
     }).Run().MapFail(error => ElementFault.ProjectionFailed(key, $"<instrument-mount-faulted:{error.Message}>"));

 // One hook-rail subscription — the app root passes it into ElementHookRail.Of beside its own taps.
 public HookTap AsTap() => new("rasm.element.instruments", fact => Fin.Succ(Project(fact)));

 // Span bracket over a rail-valued fold: StartActivity returns null with no listener (the free fast path), the
 // typed fault message lands as the error status, and the using scope is the platform-forced statement seam.
 public Fin<T> Traced<T>(string name, Func<Fin<T>> body) {
  using Activity? span = source.StartActivity(name, ActivityKind.Internal);
  return Try.lift(body).Run().Bind(static result => result)
   .MapFail(error => (span?.SetStatus(ActivityStatusCode.Error, error.Message), error).Item2);
 }

 public void Dispose() => source.Dispose();

 // Total generated dispatch — a new ElementFact case breaks this tap at compile time, so an unprojected fact is a
 // build error; every tag key is a row's declared closed dimension, and every write rides the kernel InstrumentSet.
 Unit Project(ElementFact fact) => fact.Switch<GraphInstrument, Unit>(
  state: this,
  deltaApplied: static (self, f) => (
   self.set.Count(ElementInstruments.DeltaNodes, f.Nodes),
   self.set.Count(ElementInstruments.DeltaEdges, f.Edges),
   f.Assessments.Iter(touch => self.set.Count(ElementInstruments.AssessmentOutcomes, 1L,
    new KeyValuePair<string, object?>(ElementInstruments.DisciplineTag, touch.Discipline.Key),
    new KeyValuePair<string, object?>(ElementInstruments.OutcomeTag, touch.Outcome.Key)))).Item3,
  frozen: static (self, f) => (
   self.set.Record(ElementInstruments.GraphNodes, (long)f.Nodes),
   self.set.Record(ElementInstruments.GraphEdges, (long)f.Edges)).Item2,
  baked: static (self, f) => self.set.Record(ElementInstruments.BakeDuration, f.Elapsed.TotalSeconds),
  assembled: static (self, f) => self.set.Record(ElementInstruments.AssembleDuration, f.Elapsed.TotalSeconds),
  graded: static (self, f) => self.set.Count(ElementInstruments.Findings, 1L,
   new KeyValuePair<string, object?>(ElementInstruments.SeverityTag, f.Severity.Key),
   new KeyValuePair<string, object?>(ElementInstruments.WaivedTag, f.Waived)),
  decoded: static (self, f) => (
   self.set.Record(ElementInstruments.WireDuration, f.Elapsed.TotalSeconds, new KeyValuePair<string, object?>(ElementInstruments.KindTag, f.Kind.Key)),
   f.Bytes.IfSome(bytes => ignore(self.set.Record(ElementInstruments.WireBytes, bytes, new KeyValuePair<string, object?>(ElementInstruments.KindTag, f.Kind.Key))))).Item2);
}
```

## [04]-[RESEARCH]

(none)
