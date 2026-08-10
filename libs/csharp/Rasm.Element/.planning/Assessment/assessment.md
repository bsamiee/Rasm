# [ELEMENT_ASSESSMENT]

`AssessmentPayload` owns the discipline-agnostic analysis receipt — one payload the `Graph/element#NODE_MODEL` `Node.Assessment` case wraps, keyed by the `Classification/classification#DISCIPLINE_AXIS` `Discipline`, a typed `AnalysisRoute` token, and a `UInt128` `InputKey` content key. Any analysis outcome an element carries — a structural utilization, an ISO 6946 U-value, an EnergyPlus annual-energy figure, an EN 15978 embodied-carbon result — lands as one `Assessment` node under that triple, so a route re-run over unchanged inputs resolves the stored receipt instead of re-solving.

`AssessmentPayload` holds the `Outcome`, the typed flat `Results` bag, the typed failure `Diagnostic`, an optional heavy-artifact `ResultBlob` content key, the upstream-receipt `DependsOn` set, and `Provenance`; `Rasm.Compute` keeps the discipline-specific solver I/O shapes and the `AnalysisRoute` roster behind its opaque token, so the seam grows no route enum and no per-discipline payload type.

`AssessmentOutcome` rows carry the `Usable`/`Terminal`/`Dispatchable` behavior columns, the per-row `Coherent` payload-shape law, and their legal in-place flip set as row data, so one `Advance` validates every transition and one railed `Rehydrate` re-validates a persisted tuple. `AssessmentPayload` composes `Properties/property#PROPERTY_VALUE` for typed results, `Classification/classification#DISCIPLINE_AXIS` for the keying, and `NodaTime` for the provenance instant, elapsed, and window; a malformed result rails `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected`.

## [01]-[INDEX]

- [02]-[ASSESSMENT_NODE]: `AssessmentPayload` keys the generic receipt on `Discipline`+`AnalysisRoute`+`InputKey` and carries the flat `Results` bag, the `Diagnostic` failure carrier over `SolvePhase`/`FailureKind`, the `ResultBlob` reference, the `DependsOn` upstream DAG set, and `Provenance`; `AssessmentOutcome` rows drive the lifecycle through `Advance`, the identity-preserving `Complete`/`Fail`, and the coherence-gated `Rehydrate`.

## [02]-[ASSESSMENT_NODE]

- Owner: `AssessmentPayload` the generic discipline-keyed analysis receipt the `Node.Assessment` case wraps; `AnalysisRoute` the `[ValueObject<string>]` opaque route token (the route roster is `Rasm.Compute`'s, never the seam's); `AssessmentOutcome` the `[SmartEnum<string>]` lifecycle (`Pending`/`Queued`/`Running`/`Computed`/`Failed`/`Cancelled`/`Stale`/`Superseded`) carrying the `Usable`/`Terminal`/`Dispatchable` behavior columns, the per-row `Coherent` payload-shape law, and the delegate-deferred `Next()` flip adjacency; `Diagnostic` the `[ComplexValueObject]` typed failure carrier (`SolvePhase` pipeline-stage axis, `FailureKind` cause axis with its `Transient` column, the verbatim foreign `Message`, the optional foreign `Code` — an EnergyPlus exit code, an EC3 HTTP status); `Provenance` the who/when/tool/cost record carried on every assessment (`Author`/`Tool`/`Version`/`At`, the solver-reported `Elapsed` `Duration`, the optional wall-clock `Window` `Interval`, and the optional projection-run `Correlation`).
- Law: DOUBLE-DISPATCH GUARD — the `Rasm.Compute` sweep's dedup predicate is the EXISTENCE of a non-terminal sibling node on the same `(Discipline, Route)`, never a flag on the stale node: a re-solve mints a FRESH node under the current `InputKey` and flips the old to `Superseded`, so the successor node IS the in-flight marker and the sweep skips any `Stale` row already carrying one. `Stale.Next()` therefore reaches `Superseded` alone — a `Stale → Queued` edge re-dispatches the OLD key, whose result is stale by definition. IDENTITY PRESERVATION — a solver that opened a node lands its outcome through the instance `Complete`/`Fail` transitions, which carry the `(Discipline, Route, InputKey)` triple and the `DependsOn` audit set forward by construction; the static `Pending`/`Computed`/`Failed` factories are the FRESH-mint entries a producer holding no prior node takes, and routing a write-back through them risks a re-spelled triple that keys a different node than the one the sweep is watching. RETRY ORDINAL — `Provenance.Attempt` increments on exactly one edge, the `Failed`/`Cancelled → Pending` re-request `Advance` runs, so the bounded `Diagnostic.Kind.Transient` retry gate in Compute reads a real attempt count off the receipt rather than its own memory.
- Entry: `AssessmentPayload.Pending(discipline, route, inputKey, provenance, dependsOn)` opens an assessment a solver will fill — the trailing `Seq<NodeId> dependsOn` records the upstream Assessment receipts the `InputKey` was derived over (known pre-run, a pure function of the assessed inputs; empty for a leaf over raw model inputs); `Computed(discipline, route, inputKey, results, resultBlob, provenance, key, dependsOn)` records a completed result, `Fin<T>` railing `ElementFault.ValueRejected` on an empty result bag AND no blob; `Failed(discipline, route, inputKey, diagnostic, provenance, dependsOn)` records a solver failure carrying its admitted `Diagnostic`; `Diagnostic.Of(phase, kind, message, key)` admits the failure evidence once (trimmed, a blank foreign message railed); `Advance(next, key, diagnostic)` is the ONE in-place lifecycle transition — the flip is legal iff `next` is a member of the current row's `Next()` adjacency, a cancel REQUIRES its abort diagnostic and no other flip admits one, a re-request to `Pending` clears it and increments `Provenance.Attempt`; `Complete(results, resultBlob, provenance, key)` and `Fail(diagnostic, provenance, key)` are the INSTANCE outcome transitions an in-flight node (`Pending`/`Queued`/`Running` — the `!Terminal && !Usable` two-column read) lands its solver result through, preserving the identity triple and the `DependsOn` set and re-crossing the same `Coherent` gate; `Result(name)` reads a typed `PropertyValue` result flat and `ResultMeasure(name)` the dimensioned `MeasureValue` directly (the `Rasm.Compute` consumer reads a utilization ratio, a U-value, or a GWP figure as a measure without destructuring the `PropertyValue` union, the deleted per-call `is PropertyValue.Measure m` form); `IsStaleFor(currentInputKey)` tests whether the stored `InputKey` still matches the element's current inputs so a changed input marks the cached result stale; `AnalysisRoute.Of(token, key)` admits a normalized route token on the seam `Fin<T>` rail, a blank railing `ElementFault.ValueRejected` re-keyed to the caller's `Op` (the `Classification.Of` re-stamp discipline — a rejecting admission never rides the throwing `Create`). `AssessmentPayload` admits ONLY through the `Pending`/`Computed`/`Failed` factories, `Advance`, and `Rehydrate`; the record constructor is PRIVATE and `Rehydrate(discipline, route, inputKey, outcome, results, diagnostic, resultBlob, provenance, key, dependsOn)` is the CROSS-ASSEMBLY decoder gate: it re-validates the persisted tuple against the row's `Coherent` column on the `Fin<T>` rail, so a malformed lifecycle (a `Pending` carrying results, a `Failed` with a populated bag, a `Computed` with a `Diagnostic` or an empty bag-and-blob, an in-flight flip skipping the adjacency) is UNREPRESENTABLE even off a tampered store — the `ContentAddress.Verify` distrust posture applied to the payload shape.
- Auto: the `(Discipline, Route, InputKey)` triple is the cache key — and the `Node.Assessment` id is the node's OWN content SELF-HASH `Graph/element#NODE_MODEL` `NodeId.Content(node.ToCanonicalBytes(tolerance))`, the `Node.Assessment` arm of `ToCanonicalBytes` writing its case ordinal then DELEGATING to the payload-owned `AssessmentPayload.CanonicalBytes` (the `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition.CanonicalBytes` co-location discipline — each complex payload owns its own canonical contribution rather than the `Node` arm re-spelling it), which writes exactly that triple, NEVER `NodeId.OfContent(InputKey)` — the `InputKey` is a payload field the self-hash FOLDS, not a foreign id substituted for the node id, so the id is computable pre-run by a `Rasm.Compute` author and a rehydrated Compute-authored Assessment node passes the `Projection/address#CONTENT_ADDRESS` `Verify` re-hash dual (which recomputes `ContentHash.Of(node.ToCanonicalBytes(tolerance))` and compares to the stored id); two assessments of one route over identical inputs hash to one id and ARE one node: a solver computes `InputKey` from the assessed inputs' content (the `Composition/material#MATERIAL_COMPOSITION` plies, the geometry content hash, the load case) through the kernel `XxHash128`, and a `Rasm.Compute` route resolving the existing `Computed` node rather than re-solving is the cache hit; every payload factory and `Rehydrate` normalizes `DependsOn` through `Distinct()` before storage, so the `[UnorderedEquality]` member is a set in body as well as declaration and repeated upstream ids never inflate the audit receipt; because the outcome is NOT in the content key, every legal `Advance` flip mutates the SAME node in place without minting a new id; the three behavior columns partition the lifecycle for every consumer with zero per-state branches — `Usable` gates the consumable-value filter, `Terminal` marks the solver settled for this key, `Dispatchable` marks the row the `Rasm.Compute` sweep may dispatch (`Pending`/`Stale` true; the in-flight `Queued`/`Running` false, so the sweep never double-dispatches a live job) — and the typed `Results` bag carries each output as a `Properties/property#PROPERTY_VALUE` so a consumer reads `assessment.Result("Utilization")` without learning the solver's wire shape.
- Receipt: an `Assessment` node is the analysis evidence a `Bake`-derived `Element` carries flat — `element.Assessments.Filter(a => a.Discipline == Discipline.Energy && a.Outcome.Usable)` reads every usable energy result, `assessment.ResultMeasure(name)` reads a dimensioned output as a `MeasureValue` directly (and `Result(name)` the raw `PropertyValue` for a non-measure output), `assessment.ResultBlob` fetches the heavy artifact by content key, `assessment.Diagnostic` reads a failure's phase/kind/message/code typed — a re-solve gate reads the diagnostic's `Kind.Transient` column and the dispatch sweep reads `Outcome.Dispatchable` off the row, two orthogonal column reads, never a message-text probe; `assessment.Provenance.Elapsed` reads the solve compute cost and `Provenance.Window` the wall-clock start→end so a route-cost report is a fold over receipts, never a log join; the `Rasm.Compute` analysis route writes the `Computed` node back keyed on `(InputKey, Route)`, and the seam carries the receipt without owning the solver — the discipline-specific input/result shapes, the FEA/EnergyPlus/EC3 runners, and the multi-ply `AssemblyAggregator` all live in Compute.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`/`[ValueObject<string>]`/`[ComplexValueObject]`/`[ValidationError<ElementFault>]`/`[UseDelegateFromConstructor]` the deferred `Next()` adjacency column), Generator.Equals (`[Equatable]` the payload's member-level diff + `[UnorderedEquality]` the order-insensitive `Results` bag and `DependsOn` set, so the `Graph/element#NODE_MODEL` `Node.Assessment` drill descends to `Nodes[id].Payload.Results[name]`), LanguageExt.Core (`Map`/`Option`/`Fin`/`Seq`), NodaTime (`Instant` the receipt stamp, `Duration` the solver-reported elapsed with `Duration.Zero` the request-time empty span, `Interval` the optional solve window), `Projection/address#CANONICAL_WRITER` (`CanonicalWriter` the `CanonicalBytes` projection writes through), `Rasm` (the kernel `Op` op-key + the content-key seed the `InputKey`/`ResultBlob` share).
- Growth: a new analysis discipline is one `Classification/classification#DISCIPLINE_AXIS` `Discipline` row the assessment keys on (no seam edit beyond the discipline); a new analysis route is one `AnalysisRoute` token a `Rasm.Compute` solver mints (the seam never grows a route roster); a new result is one entry in the typed `Results` bag; a new inter-assessment dependency is one `DependsOn` entry (the staleness-closure walk over the recorded DAG is the Compute sweep's fold over `element.Assessments`, never a seam graph algorithm); a new lifecycle state is one `AssessmentOutcome` row carrying its three behavior columns, its `Coherent` payload-shape law, and its `Next()` adjacency — `Advance`, `Complete`/`Fail`, `Rehydrate`, and every column-driven consumer filter absorb it with zero edits; a new failure cause is one `FailureKind` row carrying its `Transient` column and a new pipeline stage one `SolvePhase` row; never a per-discipline assessment type, never a per-route enum on the seam, never a per-state `AsX` flip method, and never a solver I/O shape on the seam.
- Boundary: `AssessmentPayload` is GENERIC and OPAQUE — the discipline-specific solver input/result shapes (the FEA load/support model, the EnergyPlus IDF, the EC3 request) live in `Rasm.Compute`, the seam carrying only the `Discipline`, the `AnalysisRoute`, the content-keyed inputs, the typed flat `Results`, the typed failure `Diagnostic`, and the optional heavy `ResultBlob` reference, so a new solver needs no seam edit; the `AnalysisRoute` is an OPAQUE token the seam never enumerates — the route roster (`"iso-6946-u"`, `"energyplus-annual"`, `"fea-utilization"`) is `Rasm.Compute`'s, the SAME neutrality `Classification.System` holds for the standards roster, so a raw `string` route on the payload is the deleted form; the assessment attaches to its object through an `Relations/relation#EDGE_ALGEBRA` `Assign` edge (sub-kind `AssignKind.Assessment`, authored by the `Rasm.Compute` producer on write-back, the `Rasm.Bim` projector round-tripping an IFC `AssignsToControl`/assessment-family relation through the neutral edge algebra), never an inlined back-reference on the `Object` node, and the analysis DAG rides `DependsOn` as receipt evidence — an assessment→assessment edge is never minted, the dependency being payload data rather than model topology; the `(InputKey, Route)` cache key is derived through the kernel `XxHash128` content hash so re-running an unchanged analysis is a cache hit and a changed input marks the result `Stale`, never a silent recompute or a stale-but-`Computed` lie; the `Node.Assessment` id is the self-hash of `ToCanonicalBytes(Discipline, Route, InputKey)` minted through `Graph/element#NODE_MODEL` `NodeId.Content` (the form the `Projection/address#CONTENT_ADDRESS` `Verify` dual recomputes), the `InputKey` a payload FIELD the triple folds and NEVER the node id itself — a producer minting the node id as `NodeId.OfContent(InputKey)` stores an id `Verify` cannot reproduce, the deleted form; because the cache identity EXCLUDES `Provenance` (a re-export under a new author/instant must NOT fork it), the `AnalysisRoute` token OR the `InputKey` MUST fold the solver tool+version (a `Rasm.Compute` obligation, the route opaque to the seam) so a solver-version bump — an EnergyPlus release change, a closed-form revision — re-keys to a FRESH node rather than false-hitting a prior version's `Computed` result, the superseded key's node flipping `Superseded` (readable history, `Usable=false` so the consumer filter resolves exactly ONE of the old/new pair) and the `Provenance` Tool/Version staying the audit of WHICH solver produced a value, never a substitute for that re-keying; a solver failure carries its evidence in the dedicated typed `Diagnostic` slot — the `SolvePhase` locating the failure (an `Extraction` failure means the solve itself succeeded and the result was lost at read-back), the `FailureKind.Transient` column separating a re-dispatchable cause (a missing binary, an exhausted budget) from a deterministic one (a rejected input, a non-convergence) without a message probe, the foreign `Message`/`Code` preserved verbatim the same way `Projection/fault#FAULT_BAND` `ProjectionFailed` keeps a captured exception's text — never smuggled as a fake `Results` entry, so a `Failed` bag stays empty and reads true to `Usable=false` (a `Superseded` bag, by contrast, keeps its last-good rows as readable HISTORY under `Usable=false` — excluded from consumption, preserved for audit and diff); `Diagnostic` is receipt DATA on the node, never an `Expected`-derived rail fault — the seam's own admission failures rail `ElementFault`, the foreign solver's failure is the thing the receipt RECORDS; the heavy result artifact rides the content-keyed blob store by `ResultBlob` (one `XxHash128` seed), never inlined on the node; the multi-ply `AssemblyAggregator` is a `Rasm.Compute` fold over the `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition` plies writing its receipt back as an `Assessment` node, never a seam owner; `AssessmentPayload` carries `[Equatable]` (the `Graph/element#NODE_MODEL` `[STRUCTURAL_EQUALITY]` discipline the wrapping `Node.Assessment` case requires) with `[UnorderedEquality]` on the `Results` bag so the `ElementGraph` `Inequalities` diff DRILLS to `Nodes[id].Payload.Results[name]` and `Nodes[id].Payload.Outcome` — because the node id keys on the `(Discipline, Route, InputKey)` triple alone, a re-solve and an `Advance` flip mutate the SAME node IN PLACE, so a member-granular `Rasm.Persistence` 3-way `StructuralMerge` reconciles two branches' diverging `Results`/`Outcome` rather than replacing the whole payload; an un-`[Equatable]` payload (an opaque equality leaf the `Node` comparer cannot descend into) collapsing every assessment re-solve to a whole-payload delta is the deleted form, and the `CanonicalBytes` content contribution (the `(Discipline, Route, InputKey)` triple, the mutable lifecycle/result fields and `Provenance` EXCLUDED) is OWNED on the payload so the node-id mint and the diff share one projection rather than the `Node` arm re-spelling it.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Runtime.InteropServices;
using Generator.Equals;
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element.Assessment;

// --- [TYPES] ------------------------------------------------------------------------------
// AnalysisRoute keys an assessment to the route a Rasm.Compute solver ran — an OPAQUE seam value: the route
// ROSTER ("iso-6946-u", "energyplus-annual", "fea-utilization", "ec3-embodied") lives in Compute, NEVER on the
// seam, the SAME neutrality Classification.System holds for the standards roster, so the (Discipline, Route,
// InputKey) cache key is a typed triple rather than a raw string a caller can fat-finger or case-fork.
// KeyMemberName/AccessModifier are EXPLICIT (the NodeId/ContentAddress form): CanonicalBytes and the fault
// discriminants read `.Value` publicly — the generated default is a PRIVATE `_value` field.
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError<ElementFault>]
public sealed partial class AnalysisRoute {
 private static partial void ValidateFactoryArguments(ref ElementFault? validationError, ref string value) {
  if (string.IsNullOrWhiteSpace(value)) { validationError = ElementFault.ValueRejected(Op.Of(name: nameof(AnalysisRoute)), "analysis route requires a non-blank token"); return; }
  value = value.Trim().ToLowerInvariant();
 }

 // Of re-keys the seam-rail admission to the CALLER's Op (the Classification.Of re-stamp discipline) — the
 // route is a REJECTING admission, so it rails like Diagnostic.Of, never the trim-only MaterialId shape.
 public static Fin<AnalysisRoute> Of(string token, Op key) =>
  Validate(token, null, out AnalysisRoute? route) is { } fault
   ? ElementFault.ValueRejected(key, fault.Message)
   : route is { } admitted
    ? Fin.Succ(admitted)
    : ElementFault.ValueRejected(key, "analysis route admission returned no value");
}

// AssessmentOutcome rows read through behavior columns beside a payload-shape and a flip-adjacency column, NOT a bare key: Usable
// gates the consumer filter — MAY this value be consumed (Computed and the readable-but-drifted Stale yes; Superseded
// false, its bag kept as history, so the old/new pair under a version re-key resolves to exactly ONE usable
// node); Terminal marks the solver settled FOR THIS KEY (a Failed re-runs only through an explicit Pending
// re-request or a Diagnostic.Kind.Transient policy in Compute, never the sweep);
// Dispatchable marks what the Rasm.Compute sweep may dispatch — the in-flight Queued/Running rows are
// (Usable:false, Terminal:false) yet NOT dispatchable, the third column a two-column form cannot express.
// Next() is the legal in-place flip set as ROW DATA (delegate-deferred: rows reference later rows) — Advance
// validates every flip against it, so the lifecycle DAG is recoverable from the declaration alone. Terminal
// RESULTS (Computed/Failed) are absent from every adjacency: they land through the instance Complete/Fail outcome
// transitions (or, for a producer holding no prior node, their static factories) as whole-payload write-backs
// under the same content-keyed id, never through a flip — that outcome carries a payload the adjacency cannot validate.
// Coherent is the row's PAYLOAD-SHAPE law over (hasResults, hasDiagnostic, hasBlob) — the same invariant the
// factories and Advance guarantee by construction, restated as data so Rehydrate re-validates a persisted tuple
// against the row instead of trusting the store (the flip PATH is history and stays unverifiable by nature).
[SmartEnum<string>]
public sealed partial class AssessmentOutcome {
 public static readonly AssessmentOutcome Pending = new("pending", usable: false, terminal: false, dispatchable: true, coherent: static (r, d, b) => !r && !d && !b, next: static () => Seq(Queued, Running, Cancelled));
 public static readonly AssessmentOutcome Queued = new("queued", usable: false, terminal: false, dispatchable: false, coherent: static (r, d, b) => !r && !d && !b, next: static () => Seq(Running, Cancelled));
 public static readonly AssessmentOutcome Running = new("running", usable: false, terminal: false, dispatchable: false, coherent: static (r, d, b) => !r && !d && !b, next: static () => Seq(Queued, Cancelled));
 public static readonly AssessmentOutcome Computed = new("computed", usable: true, terminal: true, dispatchable: false, coherent: static (r, d, b) => !d && (r || b), next: static () => Seq(Stale, Superseded));
 public static readonly AssessmentOutcome Failed = new("failed", usable: false, terminal: true, dispatchable: false, coherent: static (r, d, b) => d && !r && !b, next: static () => Seq(Pending, Superseded));
 public static readonly AssessmentOutcome Cancelled = new("cancelled", usable: false, terminal: true, dispatchable: false, coherent: static (r, d, b) => d && !r && !b, next: static () => Seq(Pending, Superseded));
 public static readonly AssessmentOutcome Stale = new("stale", usable: true, terminal: false, dispatchable: true, coherent: static (r, d, b) => !d && (r || b), next: static () => Seq(Superseded));
 public static readonly AssessmentOutcome Superseded = new("superseded", usable: false, terminal: true, dispatchable: false, coherent: static (r, d, b) => d ? !r && !b : r || b, next: static () => Seq<AssessmentOutcome>());

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

// FailureKind classes the failure cause under ONE policy column: Transient separates a cause a re-dispatch can clear
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

// Diagnostic carries the typed failure a Failed/Cancelled receipt drills — phase (where), kind (why, with its Transient
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

 private static partial void ValidateFactoryArguments(
  ref ElementFault? validationError, ref SolvePhase phase, ref FailureKind kind, ref string message, ref Option<int> code) {
  if (string.IsNullOrWhiteSpace(message)) { validationError = ElementFault.ValueRejected(Op.Of(name: nameof(Diagnostic)), "diagnostic requires a non-blank foreign message"); return; }
  message = message.Trim();
 }

 // Of re-keys the seam-rail admission to the CALLER's Op (the Classification.Of re-stamp discipline).
 public static Fin<Diagnostic> Of(SolvePhase phase, FailureKind kind, string message, Op key, Option<int> code = default) =>
  Validate(phase, kind, message, code, out Diagnostic value) is { } fault
   ? ElementFault.ValueRejected(key, fault.Message)
   : Fin.Succ(value);
}

// Provenance audits who/when/tool/cost on every assessment — a SEPARATE additive axis the content key never
// folds (the graph-altitude OwnerHistory/StepHeader exclusion): Elapsed is the solver-reported compute cost
// (Duration.Zero is the request-time empty span until a solve lands), Window the optional wall-clock start→end
// Interval (staging + solve + extraction — distinct from Elapsed, which excludes queue/IO), Correlation the
// Projection/projection#PROJECTION_CONTRACT ProjectionContext.Correlation the write-back projection ran under.
// Attempt is the additive retry-audit ordinal the seam itself advances on the ONE retry edge (Advance's
// Failed/Cancelled -> Pending re-request) and the Rasm.Compute bounded Transient gate READS — content-key-inert BY
// CONSTRUCTION because the CanonicalBytes projection folds only the (Discipline, Route, InputKey) triple and
// excludes the whole Provenance record. A retry count kept in the dispatcher's own memory dies with the process
// while the receipt outlives it, so the ordinal lives here.
public readonly record struct Provenance(
 string Author, string Tool, string Version, Instant At,
 Duration Elapsed = default, Option<Interval> Window = default, Option<CorrelationId> Correlation = default, int Attempt = default);

// --- [MODELS] -----------------------------------------------------------------------------
// [Equatable] is LOAD-BEARING ([STRUCTURAL_EQUALITY]): the diff drills into a node member only when the member is
// itself [Equatable], and the id keys on the triple alone, so a re-solve/Advance mutates the SAME node in place and
// surfaces as Nodes[id].Payload.Results[name] / .Outcome member paths — the StructuralMerge granularity. A plain
// record is an opaque equality leaf (whole-payload replacement, the deleted form); Results and DependsOn take the
// unordered comparers, every other member a generated-default leaf (Diagnostic replaced wholesale, never sub-merged).
[Equatable]
public sealed partial record AssessmentPayload {
 public Discipline Discipline { get; }
 public AnalysisRoute Route { get; }
 public UInt128 InputKey { get; }
 public AssessmentOutcome Outcome { get; }
 [UnorderedEquality] public Map<PropertyName, PropertyValue> Results { get; }
 public Option<Diagnostic> Diagnostic { get; }
 public Option<UInt128> ResultBlob { get; }
 // DependsOn records the upstream receipts this assessment's InputKey was DERIVED over — analysis DAG edge data (a
 // foundation check consuming the load-takedown receipt, an energy model consuming the assembly U-value): each
 // entry is the upstream Assessment node's content-keyed NodeId, so the Compute sweep propagates staleness
 // down the recorded chain (an upstream flip marks exactly the dependents dispatchable) and a verdict's audit trail is
 // payload DATA, not solver memory. Canon-EXCLUDED like Provenance: the deps only record HOW InputKey was derived
 // — the derivation already folds the upstream OUTPUT content into InputKey, so folding the id set forks one
 // fact twice. Empty for a leaf (an assessment over raw model inputs alone).
 [UnorderedEquality] public Seq<NodeId> DependsOn { get; }
 public Provenance Provenance { get; }

 // PRIVATE ctor + GET-ONLY members — every admission crosses a factory, the adjacency-gated Advance, or the
 // Coherent-gated Rehydrate, so a malformed lifecycle (a Pending carrying Results, a Failed with a populated
 // bag, a Computed carrying a Diagnostic, a flip skipping the Next() adjacency) is UNREPRESENTABLE — even off
 // a tampered store: no init/set survives for an external `with`/object-initializer to bypass (an `init`
 // accessor re-opens every invariant through `with` — the deleted form). Advance RECONSTRUCTS through
 // Rehydrate, so the flip result crosses the same Coherent gate a persisted tuple does.
 private AssessmentPayload(
  Discipline discipline, AnalysisRoute route, UInt128 inputKey, AssessmentOutcome outcome,
  Map<PropertyName, PropertyValue> results, Option<Diagnostic> diagnostic, Option<UInt128> resultBlob,
  Seq<NodeId> dependsOn, Provenance provenance) =>
  (Discipline, Route, InputKey, Outcome, Results, Diagnostic, ResultBlob, DependsOn, Provenance) =
   (discipline, route, inputKey, outcome, results, diagnostic, resultBlob, dependsOn, provenance);

 // Every factory carries the dependency set from the opening request onward — InputKey is a pure function of the
 // assessed inputs, upstream receipts included, so the set is KNOWN pre-run and a leaf passes none.
 public static AssessmentPayload Pending(Discipline discipline, AnalysisRoute route, UInt128 inputKey, Provenance provenance, Seq<NodeId> dependsOn = default) =>
  new(discipline, route, inputKey, AssessmentOutcome.Pending, Map<PropertyName, PropertyValue>(), None, None, DependencySet(dependsOn), provenance);

 // Computed MUST carry at least one flat result or a heavy-artifact reference — an empty computed
 // result is a solver lie the rail rejects, so a downstream cache hit never resolves a Computed-but-empty node.
 public static Fin<AssessmentPayload> Computed(
  Discipline discipline, AnalysisRoute route, UInt128 inputKey,
  Map<PropertyName, PropertyValue> results, Option<UInt128> resultBlob, Provenance provenance, Op key, Seq<NodeId> dependsOn = default) =>
  results.IsEmpty && resultBlob.IsNone
   ? ElementFault.ValueRejected(key, $"<assessment-computed-empty:{discipline.Key}:{route.Value}>")
   : Fin.Succ(new AssessmentPayload(discipline, route, inputKey, AssessmentOutcome.Computed, results, None, resultBlob, DependencySet(dependsOn), provenance));

 // Failed carries solver evidence in the typed Diagnostic — NOT smuggled as a fake Results entry, so the
 // bag stays the consumable-output store and Outcome.Usable=false reads true to its empty bag. The Diagnostic is
 // admitted upstream (Diagnostic.Of), so this factory is infallible.
 public static AssessmentPayload Failed(Discipline discipline, AnalysisRoute route, UInt128 inputKey, Diagnostic diagnostic, Provenance provenance, Seq<NodeId> dependsOn = default) =>
  new(discipline, route, inputKey, AssessmentOutcome.Failed, Map<PropertyName, PropertyValue>(), Some(diagnostic), None, DependencySet(dependsOn), provenance);

 // Rehydrate gates every persisted payload the Rasm.Persistence/Rasm.Bim decoders reconstruct —
 // PUBLIC because those decoders live across the assembly boundary (the same-assembly internal-Seed shape of
 // Composition/material#MATERIAL_COMPOSITION cannot reach them), and RAILED because a persisted tuple is NOT
 // trusted truth (the ContentAddress.Verify posture): the row's Coherent column re-validates the payload shape,
 // so a tampered store cannot mint a Computed-but-empty or a Pending-carrying-Results node. The flip PATH alone
 // is unverifiable history; every state-shape invariant the factories enforce holds here too.
 public static Fin<AssessmentPayload> Rehydrate(
  Discipline discipline, AnalysisRoute route, UInt128 inputKey, AssessmentOutcome outcome,
  Map<PropertyName, PropertyValue> results, Option<Diagnostic> diagnostic, Option<UInt128> resultBlob, Provenance provenance, Op key,
  Seq<NodeId> dependsOn = default) =>
  outcome.Coherent(!results.IsEmpty, diagnostic.IsSome, resultBlob.IsSome)
   ? Fin.Succ(new AssessmentPayload(discipline, route, inputKey, outcome, results, diagnostic, resultBlob, DependencySet(dependsOn), provenance))
   : ElementFault.ValueRejected(key, $"<assessment-incoherent:{outcome.Key}:results={!results.IsEmpty}:diagnostic={diagnostic.IsSome}:blob={resultBlob.IsSome}>");

 public Option<PropertyValue> Result(PropertyName name) => Results.Find(name);

 private static Seq<NodeId> DependencySet(Seq<NodeId> dependsOn) => dependsOn.Distinct();

 // ResultMeasure reads a dimensioned assessment output directly for the Rasm.Compute consumer —
 // a utilization ratio, a U-value, an embodied-carbon figure — without destructuring the PropertyValue union at the
 // call site. A non-Measure result (a Text/Boolean diagnostic carried in the bag) reads None, so the typed read is
 // total over the bag and honestly absent for a non-measure entry; it derives from the one Result(name) read.
 public Option<MeasureValue> ResultMeasure(PropertyName name) =>
  Result(name).Bind(static v => v is PropertyValue.Measure m ? Some(m.Value) : None);

 // CanonicalBytes owns the payload contribution the Node.ToCanonicalBytes assessment arm delegates to (case ordinal
 // then this method — the MaterialComposition.CanonicalBytes co-location shape), so the node-id mint, the Verify
 // re-hash dual, and the diff share ONE projection. ONLY the (Discipline, Route, InputKey) triple is written: the
 // mutable Outcome/Results/Diagnostic/ResultBlob, the additive Provenance, and the DependsOn audit set (whose
 // upstream OUTPUT content the InputKey derivation already folds) are EXCLUDED, so a re-solve or an Advance flip
 // never forks the node id (the cache-hit invariant).
 public void CanonicalBytes(CanonicalWriter w) =>
  w.String(Discipline.Key).String(Route.Value).U128(InputKey);

 // IsStaleFor marks a cached result stale on a changed input content key WITHOUT deleting it and without changing the
 // node id, which keys on (Discipline, Route, InputKey) not the outcome — so the next Bake surfaces a Stale
 // assessment the Compute sweep re-dispatches under the CURRENT inputs (a fresh key, a fresh node), the last-good
 // value staying readable until the re-solve.
 public bool IsStaleFor(UInt128 currentInputKey) => InputKey != currentInputKey;

 // Advance runs the ONE lifecycle transition — the flip topology is ROW DATA (Outcome.Next()), so the enumerated
 // AsStale/AsQueued/AsRunning/AsCancelled sibling-method roster is the deleted form: one arity validates the edge
 // against the adjacency and the per-target shape (a cancel REQUIRES its abort Diagnostic, no other flip admits
 // one, a Pending re-request clears it — Failed/Cancelled sources carry empty bags by construction), then flips
 // in place under the SAME content-keyed id. A Superseded flip keeps bag and diagnostic as readable history.
 // Reconstruction routes the SAME Coherent gate Rehydrate runs: the adjacency proves the flip is legal, the row's
 // payload-shape law proves the RESULT is, and both verdicts come from one owner — a private ctor beside the gate
 // lets a legal flip land an incoherent payload (a Superseded whose target row forbids a carried bag) with no signal,
 // exactly the tampered-tuple shape Rehydrate refuses.
 public Fin<AssessmentPayload> Advance(AssessmentOutcome next, Op key, Option<Diagnostic> diagnostic = default) =>
  !Outcome.Next().Exists(row => row == next)
   ? ElementFault.ValueRejected(key, $"<assessment-flip-illegal:{Outcome.Key}->{next.Key}>")
   : diagnostic.IsSome != (next == AssessmentOutcome.Cancelled)
    ? ElementFault.ValueRejected(key, $"<assessment-flip-diagnostic:{next.Key}>")
    : Rehydrate(
       Discipline, Route, InputKey, next, Results,
       next == AssessmentOutcome.Pending ? None : diagnostic | Diagnostic,
       ResultBlob, Retried(next), key, DependsOn);

 // Pending is reachable ONLY from Failed and Cancelled (the Next() adjacency), so a flip TO it IS the retry
 // re-request and the one edge that advances the Attempt ordinal — the declared retry audit the Rasm.Compute bounded
 // Transient gate reads instead of counting dispatches in its own memory. Every other flip carries Provenance
 // through untouched, and the ordinal stays content-key-inert because CanonicalBytes excludes the whole record.
 private Provenance Retried(AssessmentOutcome next) =>
  next == AssessmentOutcome.Pending ? Provenance with { Attempt = Provenance.Attempt + 1 } : Provenance;

 // Complete/Fail land an in-flight node's solver result — the success path's counterpart to
 // Advance, which owns only the in-flight and supersession flips. Both preserve the (Discipline, Route, InputKey)
 // identity triple and the DependsOn audit set BY CONSTRUCTION, so the write-back mutates the node the sweep opened
 // rather than trusting a producer to re-spell a triple that keys the node id; the static Computed/Failed factories
 // stay the FRESH-mint entries for a producer holding no prior node. The in-flight set is the two-column read
 // (!Terminal && !Usable) — Pending/Queued/Running exactly — never a roster: a Stale row re-solves under a FRESH
 // InputKey and therefore a FRESH node (the DOUBLE-DISPATCH GUARD), so completing one in place publishes a new
 // result under the stale key. Reconstruction routes Rehydrate, so the target row's Coherent law gates the landed
 // shape: a Computed carrying neither results nor a blob refuses here exactly as it refuses off a tampered store.
 public Fin<AssessmentPayload> Complete(
  Map<PropertyName, PropertyValue> results, Option<UInt128> resultBlob, Provenance provenance, Op key) =>
  Outcome.Terminal || Outcome.Usable
   ? ElementFault.ValueRejected(key, $"<assessment-complete-not-in-flight:{Outcome.Key}>")
   : Rehydrate(
      Discipline, Route, InputKey, AssessmentOutcome.Computed, results, None, resultBlob, provenance, key, DependsOn);

 public Fin<AssessmentPayload> Fail(Diagnostic diagnostic, Provenance provenance, Op key) =>
  Outcome.Terminal || Outcome.Usable
   ? ElementFault.ValueRejected(key, $"<assessment-fail-not-in-flight:{Outcome.Key}>")
   : Rehydrate(
      Discipline, Route, InputKey, AssessmentOutcome.Failed, Map<PropertyName, PropertyValue>(),
      Some(diagnostic), None, provenance, key, DependsOn);
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
