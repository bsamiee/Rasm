# [ELEMENT_ASSESSMENT]

The generic analysis-receipt owner: one `AssessmentPayload` the `Graph/element#NODE_MODEL` `Node.Assessment` case wraps, keyed by the `Classification/classification#DISCIPLINE_AXIS` `Discipline`, a typed `AnalysisRoute` token, and a `UInt128` `InputKey` content key, carrying its lifecycle `AssessmentOutcome`, a typed flat `Results` bag, an optional typed failure `Diagnostic`, an optional heavy-result blob reference, and its `Provenance`. An assessment is the seam's discipline-agnostic carrier for ANY analysis outcome attached to an element: a structural FEA utilization, an ISO 6946 U-value, an EnergyPlus annual-energy result, an ISO 12354 assembly STC, an EN 1992/1993-1-2 fire rating, an EN 15978 LCA, an EC3 embodied-carbon figure — each lands as an `Assessment` node attached to its object through an `Relations/relation#EDGE_ALGEBRA` `Assign` edge (sub-kind `AssignKind.Assessment`, authored by the `Rasm.Compute` analysis producer on write-back — an imported IFC objectified-assessment/`AssignsToControl` relation round-trips through the `Rasm.Bim` projector's NEUTRAL edge algebra, never a typed IFC case on the seam), keyed by `(Discipline, Route, InputKey)` so re-running an analysis on unchanged inputs is a cache hit. The payload is OPAQUE and TYPED: the seam carries the `Discipline`, the `AnalysisRoute`, the input content key, the `Outcome`, a typed `Results` bag a consumer reads flat, the failure `Diagnostic` (the `(SolvePhase, FailureKind, Message, Code)` carrier — the foreign solver's message preserved verbatim, the discipline-specific failure SHAPE staying in `Rasm.Compute`), an optional `ResultBlob` content key to the heavy artifact (the EnergyPlus SQLite, the FEA result set), and the `Provenance` (author, tool+version, receipt instant, solver-reported `Elapsed`, optional solve `Window`, optional projection-run `Correlation`) — but NOT the discipline-specific solver I/O shapes, which live in `Rasm.Compute`, nor the geometry, which is content-keyed. The `AnalysisRoute` is itself OPAQUE — the route roster is `Rasm.Compute`'s (the seam never enumerates it, the SAME neutrality `Classification.System` holds for the standards roster), so the cache key is a typed triple rather than a stringly-keyed one. The lifecycle is a BEHAVIOR-COLUMN vocabulary, not four ad-hoc states: eight `AssessmentOutcome` rows (request through supersession) each carry `Usable`/`Terminal`/`Dispatchable`, their `Coherent` payload-shape law, plus their legal in-place flip set as ROW DATA — one `Advance` transition validates every flip against that adjacency and one railed `Rehydrate` re-validates a persisted tuple against the row's coherence — never a sibling `AsX` method per state. The multi-ply `AssemblyAggregator` (series-resistance U-value, rule-of-mixtures density, layered STC) ALSO lives in `Rasm.Compute`, reading the `Composition/material#MATERIAL_COMPOSITION` plies and writing its result back as an `Assessment` node. The page composes `Properties/property#PROPERTY_VALUE` for the typed results, `Classification/classification#DISCIPLINE_AXIS` for the keying, and `NodaTime` for the provenance instant/elapsed/window; a malformed result rails `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected`.

## [01]-[INDEX]

- [01]-[ASSESSMENT_NODE]: the `AssessmentPayload` generic receipt keyed by `Discipline`+`AnalysisRoute`+`InputKey`, the `AnalysisRoute` opaque route token, the `AssessmentOutcome` eight-row `Usable`/`Terminal`/`Dispatchable`/`Coherent` lifecycle with its `Next()` flip adjacency, the one `Advance` transition and the coherence-gated `Rehydrate` decoder rail, the typed `Results` bag with the `Result`/`ResultMeasure` flat reads, the `Diagnostic` typed failure carrier over the `SolvePhase`/`FailureKind` axes, the `ResultBlob` heavy-artifact reference, and the enriched `Provenance`.

## [02]-[ASSESSMENT_NODE]

- Owner: `AssessmentPayload` the generic discipline-keyed analysis receipt the `Node.Assessment` case wraps; `AnalysisRoute` the `[ValueObject<string>]` opaque route token (the route roster is `Rasm.Compute`'s, never the seam's); `AssessmentOutcome` the `[SmartEnum<string>]` lifecycle (`Pending`/`Queued`/`Running`/`Computed`/`Failed`/`Cancelled`/`Stale`/`Superseded`) carrying the `Usable`/`Terminal`/`Dispatchable` behavior columns, the per-row `Coherent` payload-shape law, and the delegate-deferred `Next()` flip adjacency; `Diagnostic` the `[ComplexValueObject]` typed failure carrier (`SolvePhase` pipeline-stage axis, `FailureKind` cause axis with its `Transient` column, the verbatim foreign `Message`, the optional foreign `Code` — an EnergyPlus exit code, an EC3 HTTP status); `Provenance` the who/when/tool/cost record carried on every assessment (`Author`/`Tool`/`Version`/`At` plus the solver-reported `Elapsed` `Duration`, the optional wall-clock `Window` `Interval`, and the optional projection-run `Correlation`).
- Entry: `AssessmentPayload.Pending(discipline, route, inputKey, provenance)` opens an assessment a solver will fill; `Computed(discipline, route, inputKey, results, resultBlob, provenance, key)` records a completed result, `Fin<T>` railing `ElementFault.ValueRejected` on an empty result bag AND no blob; `Failed(discipline, route, inputKey, diagnostic, provenance)` records a solver failure carrying its admitted `Diagnostic`; `Diagnostic.Of(phase, kind, message, key)` admits the failure evidence once (trimmed, a blank foreign message railed); `Advance(next, key, diagnostic)` is the ONE in-place lifecycle transition — the flip is legal iff `next` is a member of the current row's `Next()` adjacency, a cancel REQUIRES its abort diagnostic and no other flip admits one, and a re-request to `Pending` clears it; `Result(name)` reads a typed `PropertyValue` result flat and `ResultMeasure(name)` the dimensioned `MeasureValue` directly (the `Rasm.Compute` consumer reads a utilization ratio, a U-value, or a GWP figure as a measure without destructuring the `PropertyValue` union, the deleted per-call `is PropertyValue.Measure m` form); `IsStaleFor(currentInputKey)` tests whether the stored `InputKey` still matches the element's current inputs so a changed input marks the cached result stale; `AnalysisRoute.Of(token, key)` admits a normalized route token on the seam `Fin<T>` rail, a blank railing `ElementFault.ValueRejected` re-keyed to the caller's `Op` (the `Classification.Of` re-stamp discipline — a rejecting admission never rides the throwing `Create`). The `Pending`/`Computed`/`Failed` factories (plus `Advance`/`Rehydrate`) are the ONLY admission — the record constructor is PRIVATE and `Rehydrate(discipline, route, inputKey, outcome, results, diagnostic, resultBlob, provenance, key)` is the CROSS-ASSEMBLY decoder gate: it re-validates the persisted tuple against the row's `Coherent` column on the `Fin<T>` rail, so a malformed lifecycle (a `Pending` carrying results, a `Failed` with a populated bag, a `Computed` with a `Diagnostic` or an empty bag-and-blob, an in-flight flip skipping the adjacency) is UNREPRESENTABLE even off a tampered store — the `ContentAddress.Verify` distrust posture applied to the payload shape.
- Auto: the `(Discipline, Route, InputKey)` triple is the cache key — and the `Node.Assessment` id is the node's OWN content SELF-HASH `Graph/element#NODE_MODEL` `NodeId.Content(node.ToCanonicalBytes(tolerance))`, the `Node.Assessment` arm of `ToCanonicalBytes` writing its case ordinal then DELEGATING to the payload-owned `AssessmentPayload.CanonicalBytes` (the `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition.CanonicalBytes` co-location discipline — each complex payload owns its own canonical contribution rather than the `Node` arm re-spelling it), which writes exactly that triple, NEVER `NodeId.OfContent(InputKey)` — the `InputKey` is a payload field the self-hash FOLDS, not a foreign id substituted for the node id, so the id is computable pre-run by a `Rasm.Compute` author and a rehydrated Compute-authored Assessment node passes the `Projection/address#CONTENT_ADDRESS` `Verify` re-hash dual (which recomputes `ContentHash.Of(node.ToCanonicalBytes(tolerance))` and compares to the stored id); two assessments of one route over identical inputs hash to one id and ARE one node: a solver computes `InputKey` from the assessed inputs' content (the `Composition/material#MATERIAL_COMPOSITION` plies, the geometry content hash, the load case) through the kernel `XxHash128`, and a `Rasm.Compute` route resolving the existing `Computed` node rather than re-solving is the cache hit; because the outcome is NOT in the content key, every legal `Advance` flip mutates the SAME node in place without minting a new id; the three behavior columns partition the lifecycle for every consumer with zero per-state branches — `Usable` gates the consumable-value filter, `Terminal` marks the solver settled for this key, `Dispatchable` marks the row the `Rasm.Compute` sweep may dispatch (`Pending`/`Stale` true; the in-flight `Queued`/`Running` false, so the sweep never double-dispatches a live job) — and the typed `Results` bag carries each output as a `Properties/property#PROPERTY_VALUE` so a consumer reads `assessment.Result("Utilization")` without learning the solver's wire shape.
- Receipt: an `Assessment` node is the analysis evidence a `Bake`-derived `Element` carries flat — `element.Assessments.Filter(a => a.Discipline == Discipline.Energy && a.Outcome.Usable)` reads every usable energy result, `assessment.ResultMeasure(name)` reads a dimensioned output as a `MeasureValue` directly (and `Result(name)` the raw `PropertyValue` for a non-measure output), `assessment.ResultBlob` fetches the heavy artifact by content key, `assessment.Diagnostic` reads a failure's phase/kind/message/code typed — a re-solve gate reads the diagnostic's `Kind.Transient` column and the dispatch sweep reads `Outcome.Dispatchable` off the row, two orthogonal column reads, never a message-text probe; `assessment.Provenance.Elapsed` reads the solve compute cost and `Provenance.Window` the wall-clock start→end so a route-cost report is a fold over receipts, never a log join; the `Rasm.Compute` analysis route writes the `Computed` node back keyed on `(InputKey, Route)`, and the seam carries the receipt without owning the solver — the discipline-specific input/result shapes, the FEA/EnergyPlus/EC3 runners, and the multi-ply `AssemblyAggregator` all live in Compute.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`/`[ValueObject<string>]`/`[ComplexValueObject]`/`[ValidationError<ElementFault>]`/`[UseDelegateFromConstructor]` the deferred `Next()` adjacency column), Generator.Equals (`[Equatable]` the payload's member-level diff + `[UnorderedEquality]` the order-insensitive `Results` bag, so the `Graph/element#NODE_MODEL` `Node.Assessment` drill descends to `Nodes[id].Payload.Results[name]`), LanguageExt.Core (`Map`/`Option`/`Fin`/`Seq`), NodaTime (`Instant` the receipt stamp, `Duration` the solver-reported elapsed with `Duration.Zero` the request-time empty span, `Interval` the optional solve window), `Projection/address#CANONICAL_WRITER` (`CanonicalWriter` the `CanonicalBytes` projection writes through), `Rasm` (the kernel `Op` op-key + the content-key seed the `InputKey`/`ResultBlob` share).
- Growth: a new analysis discipline is one `Classification/classification#DISCIPLINE_AXIS` `Discipline` row the assessment keys on (no seam edit beyond the discipline); a new analysis route is one `AnalysisRoute` token a `Rasm.Compute` solver mints (the seam never grows a route roster); a new result is one entry in the typed `Results` bag; a new lifecycle state is one `AssessmentOutcome` row carrying its three behavior columns, its `Coherent` payload-shape law, and its `Next()` adjacency — `Advance`, `Rehydrate`, and every column-driven consumer filter absorb it with zero edits; a new failure cause is one `FailureKind` row carrying its `Transient` column and a new pipeline stage one `SolvePhase` row; never a per-discipline assessment type, never a per-route enum on the seam, never a per-state `AsX` flip method, and never a solver I/O shape on the seam.
- Boundary: `AssessmentPayload` is GENERIC and OPAQUE — the discipline-specific solver input/result shapes (the FEA load/support model, the EnergyPlus IDF, the EC3 request) live in `Rasm.Compute`, the seam carrying only the `Discipline`, the `AnalysisRoute`, the content-keyed inputs, the typed flat `Results`, the typed failure `Diagnostic`, and the optional heavy `ResultBlob` reference, so a new solver needs no seam edit; the `AnalysisRoute` is an OPAQUE token the seam never enumerates — the route roster (`"iso-6946-u"`, `"energyplus-annual"`, `"fea-utilization"`) is `Rasm.Compute`'s, the SAME neutrality `Classification.System` holds for the standards roster, so a raw `string` route on the payload is the deleted form; the assessment attaches to its object through an `Relations/relation#EDGE_ALGEBRA` `Assign` edge (sub-kind `AssignKind.Assessment`, authored by the `Rasm.Compute` producer on write-back, the `Rasm.Bim` projector round-tripping an IFC `AssignsToControl`/assessment-family relation through the neutral edge algebra), never an inlined back-reference on the `Object` node; the `(InputKey, Route)` cache key is derived through the kernel `XxHash128` content hash so re-running an unchanged analysis is a cache hit and a changed input marks the result `Stale`, never a silent recompute or a stale-but-`Computed` lie; the `Node.Assessment` id is the self-hash of `ToCanonicalBytes(Discipline, Route, InputKey)` minted through `Graph/element#NODE_MODEL` `NodeId.Content` (the form the `Projection/address#CONTENT_ADDRESS` `Verify` dual recomputes), the `InputKey` a payload FIELD the triple folds and NEVER the node id itself — a producer minting the node id as `NodeId.OfContent(InputKey)` stores an id `Verify` cannot reproduce, the deleted form; because the cache identity EXCLUDES `Provenance` (a re-export under a new author/instant must NOT fork it), the `AnalysisRoute` token OR the `InputKey` MUST fold the solver tool+version (a `Rasm.Compute` obligation, the route opaque to the seam) so a solver-version bump — EnergyPlus 25.2.0→26.1.0, a closed-form revision — re-keys to a FRESH node rather than false-hitting a prior version's `Computed` result, the superseded key's node flipping `Superseded` (readable history, `Usable=false` so the consumer filter resolves exactly ONE of the old/new pair) and the `Provenance` Tool/Version staying the audit of WHICH solver produced a value, never a substitute for that re-keying; a solver failure carries its evidence in the dedicated typed `Diagnostic` slot — the `SolvePhase` locating the failure (an `Extraction` failure means the solve itself succeeded and the result was lost at read-back), the `FailureKind.Transient` column separating a re-dispatchable cause (a missing binary, an exhausted budget) from a deterministic one (a rejected input, a non-convergence) without a message probe, the foreign `Message`/`Code` preserved verbatim the same way `Projection/fault#FAULT_BAND` `ProjectionFailed` keeps a captured exception's text — never smuggled as a fake `Results` entry, so a `Failed` bag stays empty and reads true to `Usable=false` (a `Superseded` bag, by contrast, keeps its last-good rows as readable HISTORY under `Usable=false` — excluded from consumption, preserved for audit and diff); `Diagnostic` is receipt DATA on the node, never an `Expected`-derived rail fault — the seam's own admission failures rail `ElementFault`, the foreign solver's failure is the thing the receipt RECORDS; the heavy result artifact rides the content-keyed blob store by `ResultBlob` (one `XxHash128` seed), never inlined on the node; the multi-ply `AssemblyAggregator` is a `Rasm.Compute` fold over the `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition` plies writing its receipt back as an `Assessment` node, never a seam owner; `AssessmentPayload` carries `[Equatable]` (the `Graph/element#NODE_MODEL` `[STRUCTURAL_EQUALITY]` discipline the wrapping `Node.Assessment` case requires) with `[UnorderedEquality]` on the `Results` bag so the `ElementGraph` `Inequalities` diff DRILLS to `Nodes[id].Payload.Results[name]` and `Nodes[id].Payload.Outcome` — because the node id keys on the `(Discipline, Route, InputKey)` triple alone, a re-solve and an `Advance` flip mutate the SAME node IN PLACE, so a member-granular `Rasm.Persistence` 3-way `StructuralMerge` reconciles two branches' diverging `Results`/`Outcome` rather than replacing the whole payload; an un-`[Equatable]` payload (an opaque equality leaf the `Node` comparer cannot descend into) collapsing every assessment re-solve to a whole-payload delta is the deleted form, and the `CanonicalBytes` content contribution (the `(Discipline, Route, InputKey)` triple, the mutable lifecycle/result fields and `Provenance` EXCLUDED) is OWNED on the payload so the node-id mint and the diff share one projection rather than the `Node` arm re-spelling it.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Runtime.InteropServices;
using Generator.Equals;
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [TYPES] ------------------------------------------------------------------------------
// The analysis-route token a Rasm.Compute solver keys an assessment on — an OPAQUE seam value: the route
// ROSTER ("iso-6946-u", "energyplus-annual", "fea-utilization", "ec3-embodied") lives in Compute, NEVER on the
// seam, the SAME neutrality Classification.System holds for the standards roster, so the (Discipline, Route,
// InputKey) cache key is a typed triple rather than a raw string a caller can fat-finger or case-fork.
// KeyMemberName/AccessModifier are EXPLICIT (the NodeId/ContentAddress form): CanonicalBytes and the fault
// discriminants read `.Value` publicly — the generated default is a PRIVATE `_value` field.
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<ElementFault>]
public sealed partial class AnalysisRoute {
 static partial void ValidateFactoryArguments(ref ElementFault? validationError, ref string value) {
  if (string.IsNullOrWhiteSpace(value)) { validationError = ElementFault.ValueRejected(Op.Of(name: nameof(AnalysisRoute)), "analysis route requires a non-blank token"); return; }
  value = value.Trim().ToLowerInvariant();
 }

 // The seam-rail admission re-keyed to the CALLER's Op (the Classification.Of re-stamp discipline) — the
 // route is a REJECTING admission, so it rails like Diagnostic.Of, never the trim-only MaterialId shape.
 public static Fin<AnalysisRoute> Of(string token, Op key) =>
  Validate(token, null, out AnalysisRoute? route) is { } fault
   ? ElementFault.ValueRejected(key, fault.Message)
   : Fin.Succ(route!);
}

// The lifecycle read through three behavior columns plus a payload-shape and a flip-adjacency column, NOT a bare key: Usable gates
// the consumer filter — MAY this value be consumed (Computed and the readable-but-drifted Stale yes; Superseded
// false, its bag kept as history, so the old/new pair under a version re-key resolves to exactly ONE usable
// node); Terminal marks the solver settled FOR THIS KEY (a Failed re-runs only through an explicit Pending
// re-request or a Diagnostic.Kind.Transient policy in Compute, never the sweep);
// Dispatchable marks what the Rasm.Compute sweep may dispatch — the in-flight Queued/Running rows are
// (Usable:false, Terminal:false) yet NOT dispatchable, the third column the two-column form could not express.
// Next() is the legal in-place flip set as ROW DATA (delegate-deferred: rows reference later rows) — Advance
// validates every flip against it, so the lifecycle DAG is recoverable from the declaration alone. Terminal
// RESULTS (Computed/Failed) are absent from every adjacency: they land through their factories as whole-payload
// write-backs under the same content-keyed id, never through a flip.
// Coherent is the row's PAYLOAD-SHAPE law over (hasResults, hasDiagnostic, hasBlob) — the same invariant the
// factories and Advance guarantee by construction, restated as data so Rehydrate re-validates a persisted tuple
// against the row instead of trusting the store (the flip PATH is history and stays unverifiable by nature).
[SmartEnum<string>]
public sealed partial class AssessmentOutcome {
 public static readonly AssessmentOutcome Pending = new("pending", usable: false, terminal: false, dispatchable: true, coherent: static (r, d, b) => !r && !d && !b, static () => Seq(Queued, Running, Cancelled));
 public static readonly AssessmentOutcome Queued = new("queued", usable: false, terminal: false, dispatchable: false, coherent: static (r, d, b) => !r && !d && !b, static () => Seq(Running, Cancelled));
 public static readonly AssessmentOutcome Running = new("running", usable: false, terminal: false, dispatchable: false, coherent: static (r, d, b) => !r && !d && !b, static () => Seq(Queued, Cancelled));
 public static readonly AssessmentOutcome Computed = new("computed", usable: true, terminal: true, dispatchable: false, coherent: static (r, d, b) => !d && (r || b), static () => Seq(Stale, Superseded));
 public static readonly AssessmentOutcome Failed = new("failed", usable: false, terminal: true, dispatchable: false, coherent: static (r, d, b) => d && !r && !b, static () => Seq(Pending, Superseded));
 public static readonly AssessmentOutcome Cancelled = new("cancelled", usable: false, terminal: true, dispatchable: false, coherent: static (r, d, b) => d && !r && !b, static () => Seq(Pending, Superseded));
 public static readonly AssessmentOutcome Stale = new("stale", usable: true, terminal: false, dispatchable: true, coherent: static (r, d, b) => !d && (r || b), static () => Seq(Superseded));
 public static readonly AssessmentOutcome Superseded = new("superseded", usable: false, terminal: true, dispatchable: false, coherent: static (r, d, b) => d ? !r && !b : r || b, static () => Seq<AssessmentOutcome>());

 public bool Usable { get; }
 public bool Terminal { get; }
 public bool Dispatchable { get; }

 [UseDelegateFromConstructor] public partial bool Coherent(bool hasResults, bool hasDiagnostic, bool hasBlob);

 [UseDelegateFromConstructor] public partial Seq<AssessmentOutcome> Next();
}

// Where in the route pipeline a failure landed — Extraction/Publication mean the SOLVE succeeded and the result
// was lost at read-back/write-back, a recovery distinction no flat message carries.
[SmartEnum<string>]
public sealed partial class SolvePhase {
 public static readonly SolvePhase Admission = new("admission");
 public static readonly SolvePhase Solve = new("solve");
 public static readonly SolvePhase Extraction = new("extraction");
 public static readonly SolvePhase Publication = new("publication");
}

// The failure-cause class with its ONE policy column: Transient separates a cause a re-dispatch can clear
// (a missing binary/license, an exhausted budget) from a deterministic one (a rejected input, a divergent
// solve, a deliberate abort) — the Compute retry gate reads the column, never the foreign message. Foreign
// is the fail-closed default for unclassified provider text: never auto-retried.
[SmartEnum<string>]
public sealed partial class FailureKind {
 public static readonly FailureKind Input = new("input", transient: false);
 public static readonly FailureKind Numeric = new("numeric", transient: false);
 public static readonly FailureKind Resource = new("resource", transient: true);
 public static readonly FailureKind Timeout = new("timeout", transient: true);
 public static readonly FailureKind Aborted = new("aborted", transient: false);
 public static readonly FailureKind Foreign = new("foreign", transient: false);

 public bool Transient { get; }
}

// The typed failure carrier a Failed/Cancelled receipt drills — phase (where), kind (why, with its Transient
// column), the foreign solver's Message preserved verbatim (trimmed only), and the optional foreign Code (an
// EnergyPlus exit code, an EC3 HTTP status, a solver error number). Receipt DATA on the node — never an
// Expected-derived rail fault; the seam's own admission failures stay ElementFault.
[ComplexValueObject]
[ValidationError<ElementFault>]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct Diagnostic {
 public SolvePhase Phase { get; }
 public FailureKind Kind { get; }
 public string Message { get; }
 public Option<int> Code { get; }

 static partial void ValidateFactoryArguments(
  ref ElementFault? validationError, ref SolvePhase phase, ref FailureKind kind, ref string message, ref Option<int> code) {
  if (string.IsNullOrWhiteSpace(message)) { validationError = ElementFault.ValueRejected(Op.Of(name: nameof(Diagnostic)), "diagnostic requires a non-blank foreign message"); return; }
  message = message.Trim();
 }

 // The seam-rail admission re-keyed to the CALLER's Op (the Classification.Of re-stamp discipline).
 public static Fin<Diagnostic> Of(SolvePhase phase, FailureKind kind, string message, Op key, Option<int> code = default) =>
  Validate(phase, kind, message, code, out Diagnostic value) is { } fault
   ? ElementFault.ValueRejected(key, fault.Message)
   : Fin.Succ(value);
}

// The who/when/tool/cost audit carried on every assessment — a SEPARATE additive axis the content key never
// folds (the graph-altitude OwnerHistory/StepHeader exclusion): Elapsed is the solver-reported compute cost
// (Duration.Zero is the request-time empty span until a solve lands), Window the optional wall-clock start→end
// Interval (staging + solve + extraction — distinct from Elapsed, which excludes queue/IO), Correlation the
// Projection/projection#PROJECTION_CONTRACT ProjectionContext.CorrelationId the write-back projection ran under.
// Attempt is the additive retry-audit ordinal the Rasm.Compute bounded Transient gate reads and increments —
// content-key-inert BY CONSTRUCTION because the CanonicalBytes projection folds only the (Discipline, Route,
// InputKey) triple and excludes the whole Provenance record.
public readonly record struct Provenance(
 string Author, string Tool, string Version, Instant At,
 Duration Elapsed = default, Option<Interval> Window = default, Option<Guid> Correlation = default, int Attempt = default);

// --- [MODELS] -----------------------------------------------------------------------------
// [Equatable] is LOAD-BEARING ([STRUCTURAL_EQUALITY]): the diff drills into a node member only when the member is
// itself [Equatable], and the id keys on the triple alone, so a re-solve/Advance mutates the SAME node in place and
// surfaces as Nodes[id].Payload.Results[name] / .Outcome member paths — the StructuralMerge granularity. A plain
// record is an opaque equality leaf (whole-payload replacement, the deleted form); Results takes the dictionary-
// aware [UnorderedEquality] multiset comparer, every other member a generated-default leaf (Diagnostic replaced
// wholesale, never sub-merged).
[Equatable]
public sealed partial record AssessmentPayload {
 public Discipline Discipline { get; }
 public AnalysisRoute Route { get; }
 public UInt128 InputKey { get; }
 public AssessmentOutcome Outcome { get; }
 [UnorderedEquality] public Map<PropertyName, PropertyValue> Results { get; }
 public Option<Diagnostic> Diagnostic { get; }
 public Option<UInt128> ResultBlob { get; }
 public Provenance Provenance { get; }

 // PRIVATE ctor + GET-ONLY members — every admission crosses a factory, the adjacency-gated Advance, or the
 // Coherent-gated Rehydrate, so a malformed lifecycle (a Pending carrying Results, a Failed with a populated
 // bag, a Computed carrying a Diagnostic, a flip skipping the Next() adjacency) is UNREPRESENTABLE — even off
 // a tampered store: no init/set survives for an external `with`/object-initializer to bypass (an `init`
 // accessor would re-open every invariant through `with` — the deleted form). Advance RECONSTRUCTS through
 // this ctor (the ProfileSet.With discipline).
 private AssessmentPayload(
  Discipline discipline, AnalysisRoute route, UInt128 inputKey, AssessmentOutcome outcome,
  Map<PropertyName, PropertyValue> results, Option<Diagnostic> diagnostic, Option<UInt128> resultBlob, Provenance provenance) =>
  (Discipline, Route, InputKey, Outcome, Results, Diagnostic, ResultBlob, Provenance) =
   (discipline, route, inputKey, outcome, results, diagnostic, resultBlob, provenance);

 public static AssessmentPayload Pending(Discipline discipline, AnalysisRoute route, UInt128 inputKey, Provenance provenance) =>
  new(discipline, route, inputKey, AssessmentOutcome.Pending, Map<PropertyName, PropertyValue>(), None, None, provenance);

 // A computed assessment MUST carry at least one flat result or a heavy-artifact reference — an empty computed
 // result is a solver lie the rail rejects, so a downstream cache hit never resolves a Computed-but-empty node.
 public static Fin<AssessmentPayload> Computed(
  Discipline discipline, AnalysisRoute route, UInt128 inputKey,
  Map<PropertyName, PropertyValue> results, Option<UInt128> resultBlob, Provenance provenance, Op key) =>
  results.IsEmpty && resultBlob.IsNone
   ? ElementFault.ValueRejected(key, $"<assessment-computed-empty:{discipline.Key}:{route.Value}>")
   : Fin.Succ(new AssessmentPayload(discipline, route, inputKey, AssessmentOutcome.Computed, results, None, resultBlob, provenance));

 // A solver failure carries its evidence in the typed Diagnostic — NOT smuggled as a fake Results entry, so the
 // bag stays the consumable-output store and Outcome.Usable=false reads true to its empty bag. The Diagnostic is
 // admitted upstream (Diagnostic.Of), so this factory is infallible.
 public static AssessmentPayload Failed(Discipline discipline, AnalysisRoute route, UInt128 inputKey, Diagnostic diagnostic, Provenance provenance) =>
  new(discipline, route, inputKey, AssessmentOutcome.Failed, Map<PropertyName, PropertyValue>(), Some(diagnostic), None, provenance);

 // The railed re-hydration gate the Rasm.Persistence/Rasm.Bim decoders reconstruct a persisted payload through —
 // PUBLIC because those decoders live across the assembly boundary (the same-assembly internal-Seed shape of
 // Composition/material#MATERIAL_COMPOSITION cannot reach them), and RAILED because a persisted tuple is NOT
 // trusted truth (the ContentAddress.Verify posture): the row's Coherent column re-validates the payload shape,
 // so a tampered store cannot mint a Computed-but-empty or a Pending-carrying-Results node. The flip PATH alone
 // is unverifiable history; every state-shape invariant the factories enforce holds here too.
 public static Fin<AssessmentPayload> Rehydrate(
  Discipline discipline, AnalysisRoute route, UInt128 inputKey, AssessmentOutcome outcome,
  Map<PropertyName, PropertyValue> results, Option<Diagnostic> diagnostic, Option<UInt128> resultBlob, Provenance provenance, Op key) =>
  outcome.Coherent(!results.IsEmpty, diagnostic.IsSome, resultBlob.IsSome)
   ? Fin.Succ(new AssessmentPayload(discipline, route, inputKey, outcome, results, diagnostic, resultBlob, provenance))
   : ElementFault.ValueRejected(key, $"<assessment-incoherent:{outcome.Key}:results={!results.IsEmpty}:diagnostic={diagnostic.IsSome}:blob={resultBlob.IsSome}>");

 public Option<PropertyValue> Result(PropertyName name) => Results.Find(name);

 // The dimensioned-result convenience the Rasm.Compute consumer reads a numeric assessment output through directly —
 // a utilization ratio, a U-value, an embodied-carbon figure — without destructuring the PropertyValue union at the
 // call site. A non-Measure result (a Text/Boolean diagnostic carried in the bag) reads None, so the typed read is
 // total over the bag and honestly absent for a non-measure entry; it derives from the one Result(name) read.
 public Option<MeasureValue> ResultMeasure(PropertyName name) =>
  Result(name).Bind(static v => v is PropertyValue.Measure m ? Some(m.Value) : None);

 // The payload-owned canonical contribution the Node.ToCanonicalBytes assessment arm delegates to (case ordinal
 // then this method — the MaterialComposition.CanonicalBytes co-location shape), so the node-id mint, the Verify
 // re-hash dual, and the diff share ONE projection. ONLY the (Discipline, Route, InputKey) triple is written: the
 // mutable Outcome/Results/Diagnostic/ResultBlob and the additive Provenance are EXCLUDED, so a re-solve or an
 // Advance flip never forks the node id (the cache-hit invariant).
 public void CanonicalBytes(CanonicalWriter w) =>
  w.String(Discipline.Key).String(Route.Value).U128(InputKey);

 // A changed input content key marks the cached result stale WITHOUT deleting it — and without changing the node
 // id, which keys on (Discipline, Route, InputKey) not the outcome — so the next Bake surfaces a Stale assessment
 // the Compute sweep re-dispatches under the CURRENT inputs (a fresh key, a fresh node), the last-good value
 // staying readable until the re-solve.
 public bool IsStaleFor(UInt128 currentInputKey) => InputKey != currentInputKey;

 // The ONE lifecycle transition — the flip topology is ROW DATA (Outcome.Next()), so the enumerated
 // AsStale/AsQueued/AsRunning/AsCancelled sibling-method roster is the deleted form: one arity validates the edge
 // against the adjacency and the per-target shape (a cancel REQUIRES its abort Diagnostic, no other flip admits
 // one, a Pending re-request clears it — Failed/Cancelled sources carry empty bags by construction), then flips
 // in place under the SAME content-keyed id. A Superseded flip keeps bag and diagnostic as readable history.
 public Fin<AssessmentPayload> Advance(AssessmentOutcome next, Op key, Option<Diagnostic> diagnostic = default) =>
  !Outcome.Next().Exists(row => row == next)
   ? ElementFault.ValueRejected(key, $"<assessment-flip-illegal:{Outcome.Key}->{next.Key}>")
   : diagnostic.IsSome != (next == AssessmentOutcome.Cancelled)
    ? ElementFault.ValueRejected(key, $"<assessment-flip-diagnostic:{next.Key}>")
    : Fin.Succ(new AssessmentPayload(
       Discipline, Route, InputKey, next, Results,
       next == AssessmentOutcome.Pending ? None : diagnostic | Diagnostic,
       ResultBlob, Provenance));
}
```

## [03]-[RESEARCH]

- [GENERIC_ASSESSMENT]: the seam `AssessmentPayload` is discipline-agnostic by design — the `(Discipline, AnalysisRoute, InputKey)` triple keys ANY analysis outcome, the typed `Results` bag carries the flat outputs, the typed `Diagnostic` the failure evidence, and the `ResultBlob` references the heavy artifact, so the structural FEA (VividOrange/BriefFiniteElement/FEALiTE2D), the thermal closed-form (ISO 6946 U / EN 13788 Glaser), the energy simulation (NREL.OpenStudio model + EnergyPlus subprocess), the acoustic (ISO 12354), the fire (EN 199x-1-2), and the LCA (EN 15978 + EC3 REST) routes all write the same node shape — the discipline-specific input/result shapes and the runners living in `Rasm.Compute`, the seam owning only the receipt; the assessment is a generic opaque typed payload, not a per-discipline node family, and the `AnalysisRoute` is the opaque method discriminant Compute mints (the seam never enumerating the route roster, the SAME neutrality the `Classification` collapse holds for the standards roster).
- [CONTENT_KEYED_CACHE]: the `(InputKey, AnalysisRoute)` cache key is derived through the kernel `XxHash128` over the assessed inputs' content (`Projection/address#CONTENT_ADDRESS`), and the `Node.Assessment` id itself is content-keyed on the `(Discipline, Route, InputKey)` triple (`Graph/element#NODE_MODEL` `ToCanonicalBytes`), so two assessments of one route over identical inputs ARE one node and the `Rasm.Compute` route resolves the existing `Computed` assessment rather than re-solving; `IsStaleFor` marks a result `Stale` when a downstream edit changes the input key — the assessment cache is the same content-addressed discipline the geometry blob store and the snapshot spine use, never a wall-clock or a version-number cache; a solver-version bump re-keys through the route/input-key fold, the prior node flipping `Superseded` so exactly one of the old/new pair reads `Usable`; the assessment attaches through an `Assign` edge (sub-kind `AssignKind.Assessment`, the `Rasm.Compute` producer authoring it on write-back) so the graph carries the element→assessment binding, the `Rasm.Bim` projector round-tripping the IFC objectified-assessment relation through the neutral edge algebra at egress.
- [LIFECYCLE_COLUMNS]: the `AssessmentOutcome` is a behavior-bearing lifecycle, not a bare token — eight rows spanning the real async-solver space (request `Pending`, in-flight `Queued`/`Running`, settled `Computed`/`Failed`/`Cancelled`, drift `Stale`, re-key `Superseded`) read through three columns: `Usable` gates the consumable filter (`element.Assessments` filters on `Outcome.Usable` rather than a per-state branch), `Terminal` marks the solver settled for this key, and `Dispatchable` marks the sweep-dispatchable rows (`Pending`/`Stale`) apart from the in-flight ones (`Queued`/`Running`) the two-column form could not distinguish — the `Rasm.Compute` dispatcher routes on the columns the rows carry, and a `Failed` re-run is either an explicit `Advance` to `Pending` or a Compute policy read of `Diagnostic.Kind.Transient`, never an automatic sweep pickup; because the node id keys on `(Discipline, Route, InputKey)` and NOT the outcome, every legal flip (`Computed`→`Stale` under input drift, `Computed`/`Failed`/`Cancelled`/`Stale`→`Superseded` under a version re-key, the in-flight `Queued`/`Running` bookkeeping, the `Failed`/`Cancelled`→`Pending` re-request) mutates in place without minting a new id, and the flip topology itself is the `Next()` adjacency column `Advance` validates — an illegal edge is a railed `ElementFault.ValueRejected`, never a silent state teleport; the fourth row axis is the `Coherent` payload-shape law — each row states which `(Results, Diagnostic, ResultBlob)` shapes it admits (in-flight rows all-empty, `Computed`/`Stale` a non-empty bag or a blob and no diagnostic, `Failed`/`Cancelled` a diagnostic over an empty bag, `Superseded` either history shape) — so the cross-assembly `Rehydrate` decoder gate re-imposes by DATA exactly the invariants the factories and `Advance` guarantee by construction, and a persisted tuple that violates its own row rails rather than re-entering the interior as trusted.
