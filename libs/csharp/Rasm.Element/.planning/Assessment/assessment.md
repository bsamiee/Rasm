# [ELEMENT_ASSESSMENT]

`AssessmentPayload` owns the discipline-agnostic analysis receipt — one payload the `Graph/element#NODE_MODEL` `Node.Assessment` case wraps, keyed by the `Classification/classification#DISCIPLINE_AXIS` `Discipline`, a typed `AnalysisRoute` token, and a `UInt128` `InputKey` content key. Any analysis outcome an element carries — a structural utilization, an ISO 6946 U-value, an EnergyPlus annual-energy figure, an EN 15978 embodied-carbon result — lands as one `Assessment` node under that triple, so a route re-run over unchanged inputs resolves the stored receipt instead of re-solving.

`AssessmentPayload` holds the `Outcome`, the ONE `PayloadContent` value (empty, results-with-optional-`BlobKey` artifact, or typed failure `Diagnostic`), the upstream-receipt `DependsOn` set, and the `EvidenceRun` audit; `Rasm.Compute` keeps the discipline-specific solver I/O shapes and the `AnalysisRoute` roster behind its opaque token, so the seam grows no route enum and no per-discipline payload type.

`AssessmentOutcome` rows carry a kernel `CapabilitySet<OutcomeCapability>` (`Consumable`/`Settled`/`Dispatchable`/`InFlight`/`Reportable`), the per-row `Coherent` law over the `PayloadContent` value, and their legal flip set as row data, so one `Advance` validates every transition and the ONE `Open` entry gates fresh mints and persisted tuples alike. `AssessmentPayload` composes `Properties/property#PROPERTY_VALUE` for typed results and `NodaTime` for the audit instants; universal malformed inputs route through kernel admission while assessment coherence and transition refusals remain `ElementFault.ValueRejected`.

## [01]-[INDEX]

- [02]-[ASSESSMENT_NODE]: `AssessmentPayload` keys the generic receipt on `Discipline`+`AnalysisRoute`+`InputKey` and carries the ONE `PayloadContent` value, the `DependsOn` upstream set, and the `EvidenceRun` audit; `AssessmentOutcome` capability rows drive the lifecycle through `Advance`, the content-keyed `Land`, and the coherence-gated `Open`.

## [02]-[ASSESSMENT_NODE]

- Owner: `AssessmentPayload` the generic discipline-keyed analysis receipt the `Node.Assessment` case wraps; `AnalysisRoute` the `[ValueObject<string>]` opaque route token (the roster is `Rasm.Compute`'s); `AssessmentOutcome` the `[SmartEnum<string>]` lifecycle whose rows carry `Capabilities : CapabilitySet<OutcomeCapability>`, the `Coherent(PayloadContent)` shape law, and the `Next()` flip adjacency; `OutcomeCapability` the S8 vocabulary every consumer filter reads; `PayloadContent` the `[Union]` payload value (`Empty` | `Results(bag, Option<BlobKey>)` | `Failure(Diagnostic)`) whose `Results` mint makes a computed-but-empty payload unrepresentable at construction; `Diagnostic` the `[ComplexValueObject]` typed failure (`SolvePhase` stage, `FailureKind` cause with its `Transient` column, verbatim foreign `Message`, optional `Code`); `EvidenceRun` the gated who/when/tool/cost audit (S-E3 — the retired `Provenance` name; it rides `PropertyEvidence.Run` and owns its own `CanonicalBytes`).
- Law: DOUBLE-DISPATCH GUARD — the `Rasm.Compute` sweep's dedup predicate is the EXISTENCE of a non-terminal sibling node on the same `(Discipline, Route)`, never a flag on the stale node: a re-solve mints a FRESH node under the current `InputKey` and flips the old to `Superseded`, so the successor node IS the in-flight marker and the sweep skips any `Stale` row already carrying one. `Stale.Next()` therefore reaches `Superseded` alone — a `Stale → Queued` edge re-dispatches the OLD key, whose result is stale by definition. IDENTITY PRESERVATION — a solver that opened a node lands its outcome through the instance `Land`, which carries the `(Discipline, Route, InputKey)` triple and the `DependsOn` set forward by construction; `Open` is the fresh-mint entry a producer holding no prior node takes, and routing a write-back through it risks a re-spelled triple keying a different node than the one the sweep watches. RETRY ORDINAL — `EvidenceRun.Attempt` increments on exactly one edge, the `Failed`/`Cancelled → Pending` re-request `Advance` runs, so the bounded `Diagnostic.Kind.Transient` retry gate in Compute reads a real attempt count off the receipt rather than its own memory.
- Entry: `Open(discipline, route, inputKey, outcome, content, provenance, key, dependsOn)` is the ONE admission — fresh mint AND cross-assembly rehydration share the row's `Coherent(content)` gate, so a tampered store's malformed tuple refuses exactly as a fresh one (the former `Pending`/`Computed`/`Failed` factory family and the separate `Rehydrate` collapsed here; a producer opening a run passes `PayloadContent.Empty` under `Pending`); `Land(content, provenance, key)` is the ONE outcome landing an in-flight node takes — the content CASE selects `Computed` or `Failed`, the identity triple and `DependsOn` carried by construction (the former `Complete`/`Fail` pair; the two fault tokens merged as `<assessment-land-not-in-flight:{content.Kind}:{Outcome.Key}>`); `Advance(next, key, diagnostic)` the ONE in-place flip over the row adjacency (a cancel REQUIRES its abort diagnostic, a `Pending` re-request clears content and advances `Attempt`); `PayloadContent.Results(values, artifact, key)`/`.Failure(diagnostic)` the payload mints; `Diagnostic.Of(phase, kind, message, key)` the failure admission; `EvidenceRun.Of(author, tool, version, at, key, …)` the gated audit mint; `Result(name)`/`ResultMeasure(name)` the flat typed reads; `IsStaleFor(currentInputKey)` the staleness probe; `AnalysisRoute.Of(token, key)` the railed route admission.
- Auto: the `(Discipline, Route, InputKey)` triple is the cache key — and the `Node.Assessment` id is the node's OWN content SELF-HASH `Graph/element#NODE_MODEL` `NodeId.Of(new NodeSeed.Content(node, tolerance))`, the `Node.Assessment` arm of `CanonicalBytes` writing its case ordinal then DELEGATING to the payload-owned `AssessmentPayload.CanonicalBytes` (the `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition.CanonicalBytes` co-location discipline — each complex payload owns its own canonical contribution rather than the `Node` arm re-spelling it), which writes exactly that triple, NEVER a `NodeSeed.Precomputed` wrap of the `InputKey` — the `InputKey` is a payload field the self-hash FOLDS, not a foreign id substituted for the node id, so the id is computable pre-run by a `Rasm.Compute` author and a rehydrated Compute-authored Assessment node passes the `Projection/address#CONTENT_ADDRESS` `Verify` re-hash dual (which re-mints through the node's own `Content` seed and compares to the stored id); two assessments of one route over identical inputs hash to one id and ARE one node: a solver computes `InputKey` from the assessed inputs' content (the `Composition/material#MATERIAL_COMPOSITION` plies, the geometry content hash, the load case) through the kernel `XxHash128`, and a `Rasm.Compute` route resolving the existing `Computed` node rather than re-solving is the cache hit; `DependsOn` is a structural `Set<NodeId>`, so distinctness is the TYPE's, never a normalizing fold; because the outcome is NOT in the content key, every legal `Advance` flip mutates the SAME node in place without minting a new id; the three behavior columns partition the lifecycle for every consumer with zero per-state branches — `Capabilities.Admits(Consumable)` gates the value filter, `Admits(Settled)` marks the solver settled for this key, `Admits(Dispatchable)` marks what the sweep may dispatch (`Pending`/`Stale`; the in-flight rows are neither settled nor dispatchable), `Admits(Reportable)` feeds the audit defect sweep — `Cancelled` carries it, closing the silent drop — and the typed `Results` bag carries each output as a `Properties/property#PROPERTY_VALUE` so a consumer reads `assessment.Result("Utilization")` without learning the solver's wire shape.
- Receipt: an `Assessment` node is the analysis evidence a `Bake`-derived `Element` carries flat — `element.Assessments.Filter(a => a.Discipline == Discipline.Energy && a.Outcome.Capabilities.Admits(OutcomeCapability.Consumable))` reads every usable energy result, `assessment.ResultMeasure(name)` reads a dimensioned output as a `MeasureValue` directly (and `Result(name)` the raw `PropertyValue` for a non-measure output), `assessment.ResultBlob` fetches the heavy artifact by content key, `assessment.Diagnostic` reads a failure's phase/kind/message/code typed — a re-solve gate reads the diagnostic's `Kind.Transient` column and the dispatch sweep reads `Outcome.Capabilities.Admits(Dispatchable)` off the row, two orthogonal column reads, never a message-text probe; `assessment.Provenance.Elapsed` (`EvidenceRun`) reads the solve compute cost and `Provenance.Window` the wall-clock start→end so a route-cost report is a fold over receipts, never a log join; the `Rasm.Compute` analysis route writes the `Computed` node back keyed on `(InputKey, Route)`, and the seam carries the receipt without owning the solver — the discipline-specific input/result shapes, the FEA/EnergyPlus/EC3 runners, and the multi-ply `AssemblyAggregator` all live in Compute.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`/`[ValueObject<string>]`/`[ComplexValueObject]`/`[ValidationError]`/`[UseDelegateFromConstructor]` the deferred `Next()` adjacency column), Generator.Equals (`[Equatable]` the payload's member-level diff + `[UnorderedEquality]` the order-insensitive `Results` bag and `DependsOn` set, so the `Graph/element#NODE_MODEL` `Node.Assessment` drill descends to `Nodes[id].Payload.Results[name]`), LanguageExt.Core (`Map`/`Option`/`Fin`/`Seq`), NodaTime (`Instant` the receipt stamp, `Duration` the solver-reported elapsed with `Duration.Zero` the request-time empty span, `Interval` the optional solve window), `Rasm/Domain/identity#CONTENT_KEY` (`CanonicalWriter` the `CanonicalBytes` projection writes through), `Rasm` (the kernel `Op` op-key + the content-key seed the `InputKey`/`ResultBlob` share).
- Growth: a new analysis discipline is one `Discipline` row; a new route one Compute-minted `AnalysisRoute` token; a new result one `Results` bag entry; a new dependency one `DependsOn` member; a new lifecycle state is one `AssessmentOutcome` row carrying its capability set, `Coherent` law, and adjacency — `Open`, `Land`, `Advance`, and every capability-filtered consumer absorb it with zero edits; a new payload shape is one `PayloadContent` case + the rows that admit it; a new failure cause one `FailureKind` row, a new stage one `SolvePhase` row; never a per-discipline assessment type, a per-route enum, a per-state flip method, or a solver I/O shape on the seam.
- Boundary: `AssessmentPayload` is GENERIC and OPAQUE — solver I/O shapes, runners, and the `AssemblyAggregator` live in `Rasm.Compute`; the route token is opaque (the roster is Compute's, the `Classification.System` neutrality). The assessment attaches through an `Assign` edge (`AssignKind.Assessment`), never an inlined back-reference, and the analysis DAG rides `DependsOn` as receipt data, never minted edges. The node id is the SELF-HASH over `CanonicalBytes` (the `(Discipline, Route, InputKey)` triple alone — a `NodeSeed.Precomputed` wrap of the `InputKey` stores an id `Verify` cannot reproduce, the deleted form), so a re-solve and a flip mutate ONE node in place and the member-granular merge reconciles diverging `Content`/`Outcome`; because the cache identity EXCLUDES the audit, the route token or `InputKey` MUST fold the solver tool+version (a Compute obligation) so a version bump re-keys to a FRESH node, the old one flipping `Superseded`. A failure is the `PayloadContent.Failure` CASE — receipt DATA, never an `Fault`-derived rail fault and never a fake `Results` entry; the heavy artifact rides the content-keyed `BlobKey`, never inlined. `[Equatable]` + the structural `Set`/unordered bag keep the diff drilling to member paths; the canonical contribution is OWNED on the payload so the id mint, the `Verify` dual, and the diff share one projection.

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
using Band = Rasm.Numerics.Band;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;

namespace Rasm.Element.Assessment;

// --- [TYPES] ------------------------------------------------------------------------------
// OPAQUE route token — the roster lives in Compute (the Classification.System neutrality), so the cache key is
// a typed triple no caller can case-fork; KeyMemberName EXPLICIT (CanonicalBytes reads .Value publicly).
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError]
public sealed partial class AnalysisRoute {
 private static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
  if (string.IsNullOrWhiteSpace(value)) { validationError = new ValidationError("analysis route requires a non-blank token"); return; }
  value = value.Trim().ToLowerInvariant();
 }

 // The caller's operation key owns the generated admission refusal.
 public static Fin<AnalysisRoute> Of(string token, Op key) =>
  key.AcceptValidated<AnalysisRoute>(token);
}

// The outcome's whole behavior is DATA: Capabilities the kernel S8 set every consumer filter reads (Consumable
// the value-may-be-read gate; Settled the solver-settled-for-this-key mark; Dispatchable what the Compute sweep
// may dispatch — in-flight Queued/Running are neither settled nor dispatchable, the third axis a two-column form
// cannot express; InFlight the Land gate the old !Terminal && !Usable read spelled twice; Reportable the audit
// sweep's ONE defect filter — Failed/Cancelled/Stale/Superseded carry it, Cancelled closing the drop where a
// cancelled analysis on a delivered model graded clean), Coherent the row's payload-shape law over the ONE PayloadContent value, Next() the legal
// in-place flip set (delegate-deferred; terminal RESULTS land through the instance Land, never a flip).
[SmartEnum<string>]
public sealed partial class AssessmentOutcome {
 public static readonly AssessmentOutcome Pending = new("pending",
  capabilities: CapabilitySet<OutcomeCapability>.Of(OutcomeCapability.Dispatchable, OutcomeCapability.InFlight),
  coherent: static content => content is PayloadContent.EmptyCase,
  next: static () => Seq(Queued, Running, Cancelled));
 public static readonly AssessmentOutcome Queued = new("queued",
  capabilities: CapabilitySet<OutcomeCapability>.Of(OutcomeCapability.InFlight),
  coherent: static content => content is PayloadContent.EmptyCase,
  next: static () => Seq(Running, Cancelled));
 public static readonly AssessmentOutcome Running = new("running",
  capabilities: CapabilitySet<OutcomeCapability>.Of(OutcomeCapability.InFlight),
  coherent: static content => content is PayloadContent.EmptyCase,
  next: static () => Seq(Queued, Cancelled));
 public static readonly AssessmentOutcome Computed = new("computed",
  capabilities: CapabilitySet<OutcomeCapability>.Of(OutcomeCapability.Consumable, OutcomeCapability.Settled),
  coherent: static content => content is PayloadContent.ResultsCase,
  next: static () => Seq(Stale, Superseded));
 public static readonly AssessmentOutcome Failed = new("failed",
  capabilities: CapabilitySet<OutcomeCapability>.Of(OutcomeCapability.Settled, OutcomeCapability.Reportable),
  coherent: static content => content is PayloadContent.FailureCase,
  next: static () => Seq(Pending, Superseded));
 public static readonly AssessmentOutcome Cancelled = new("cancelled",
  capabilities: CapabilitySet<OutcomeCapability>.Of(OutcomeCapability.Settled, OutcomeCapability.Reportable),
  coherent: static content => content is PayloadContent.FailureCase,
  next: static () => Seq(Pending, Superseded));
 public static readonly AssessmentOutcome Stale = new("stale",
  capabilities: CapabilitySet<OutcomeCapability>.Of(OutcomeCapability.Consumable, OutcomeCapability.Dispatchable, OutcomeCapability.Reportable),
  coherent: static content => content is PayloadContent.ResultsCase,
  next: static () => Seq(Superseded));
 public static readonly AssessmentOutcome Superseded = new("superseded",
  capabilities: CapabilitySet<OutcomeCapability>.Of(OutcomeCapability.Settled, OutcomeCapability.Reportable),
  coherent: static content => content is not PayloadContent.EmptyCase,
  next: static () => Seq<AssessmentOutcome>());

 public CapabilitySet<OutcomeCapability> Capabilities { get; }

 [UseDelegateFromConstructor] public partial bool Coherent(PayloadContent content);

 [UseDelegateFromConstructor] public partial Seq<AssessmentOutcome> Next();
}

// The outcome capability vocabulary (kernel S8): each consumer filter reads ONE row instead of a bool column.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OutcomeCapability : ICapability<OutcomeCapability> {
 public static readonly OutcomeCapability Consumable = new("consumable");
 public static readonly OutcomeCapability Settled = new("settled");
 public static readonly OutcomeCapability Dispatchable = new("dispatchable");
 public static readonly OutcomeCapability InFlight = new("in-flight");
 public static readonly OutcomeCapability Reportable = new("reportable");
}

// The payload as ONE value — the (hasResults, hasDiagnostic, hasBlob) positional-bool triple made a case family:
// a Computed-but-empty payload is unrepresentable at CONSTRUCTION (Results gates bag-or-artifact), Coherent gates
// a VALUE, and the artifact reference is the typed BlobKey, never a raw UInt128 re-stating the seed invariant.
[Union]
public abstract partial record PayloadContent {
 private PayloadContent() { }

 public sealed record EmptyCase : PayloadContent;
 [Equatable]
 public sealed partial record ResultsCase : PayloadContent {
  internal ResultsCase(Map<PropertyName, PropertyValue> values, Option<BlobKey> artifact) => (Values, Artifact) = (values, artifact);
  [property: UnorderedEquality] public Map<PropertyName, PropertyValue> Values { get; }
  public Option<BlobKey> Artifact { get; }
 }
 public sealed record FailureCase(Diagnostic Diagnostic) : PayloadContent;

 public static readonly PayloadContent Empty = new EmptyCase();

 public static Fin<PayloadContent> Results(Map<PropertyName, PropertyValue> values, Option<BlobKey> artifact, Op key) =>
  values.IsEmpty && artifact.IsNone
   ? new ElementFault.ValueRejected(key, "<assessment-results-empty>")
   : Fin.Succ<PayloadContent>(new ResultsCase(values, artifact));

 public static PayloadContent Failure(Diagnostic diagnostic) => new FailureCase(diagnostic);

 // The token discriminant fault details carry.
 public string Kind => Switch(empty: static _ => "empty", results: static _ => "results", failure: static _ => "failure");
}

// Where in the route pipeline a failure landed// Where in the route pipeline a failure landed — Extraction/Publication mean the SOLVE succeeded and the result
// was lost at read-back/write-back, a recovery distinction no flat message carries.
[SmartEnum<string>]
public sealed partial class SolvePhase {
 public static readonly SolvePhase Admission = new("admission");
 public static readonly SolvePhase Solve = new("solve");
 public static readonly SolvePhase Extraction = new("extraction");
 public static readonly SolvePhase Publication = new("publication");
}

// Transient separates a re-dispatchable cause from a deterministic one — the Compute retry gate reads the
// column, never the foreign message; Foreign is the fail-closed default for unclassified provider text.
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

// The typed failure a Failure case drills — phase, kind, verbatim foreign Message, optional foreign Code.
// Receipt DATA on the node, never an Fault-derived rail fault.
[ComplexValueObject]
[ValidationError]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct Diagnostic {
 public SolvePhase Phase { get; }
 public FailureKind Kind { get; }
 public string Message { get; }
 public Option<int> Code { get; }

 private static partial void ValidateFactoryArguments(
  ref ValidationError? validationError, ref SolvePhase phase, ref FailureKind kind, ref string message, ref Option<int> code) {
  if (string.IsNullOrWhiteSpace(message)) { validationError = new ValidationError("diagnostic requires a non-blank foreign message"); return; }
  message = message.Trim();
 }

 // The caller's operation key owns the generated admission refusal.
 public static Fin<Diagnostic> Of(SolvePhase phase, FailureKind kind, string message, Op key, Option<int> code = default) =>
  key.AcceptValidated<Diagnostic>(Validate(phase, kind, message, code, out Diagnostic value), value);
}

// EvidenceRun audits who/when/tool/cost on every assessment AND rides PropertyEvidence.Run (S-E3) — the ONE
// solver-run audit record corpus-wide (the name Provenance is retired). Gated: the only owner in the folder that
// carried a public positional ctor and zero gates let a blank Author, a negative Elapsed, and a negative Attempt
// reach the wire verbatim. Content-key-INERT on the assessment (the payload canon excludes it); its OWN
// CanonicalBytes serves the evidence identity a material property carries — Correlation excluded there too (a
// projection-run correlation is causal-frame provenance, not evidence identity). Attempt advances on exactly one
// edge (the Failed/Cancelled -> Pending re-request), the declared retry audit the Compute Transient gate reads.
public sealed record EvidenceRun {
 private EvidenceRun(string author, string tool, string version, Instant at,
  Duration elapsed, Option<Interval> window, Option<CorrelationId> correlation, int attempt) =>
  (Author, Tool, Version, At, Elapsed, Window, Correlation, Attempt) =
   (author, tool, version, at, elapsed, window, correlation, attempt);

 public string Author { get; }
 public string Tool { get; }
 public string Version { get; }
 public Instant At { get; }
 public Duration Elapsed { get; }
 public Option<Interval> Window { get; }
 public Option<CorrelationId> Correlation { get; }
 public int Attempt { get; }

 public static Fin<EvidenceRun> Of(string author, string tool, string version, Instant at, Op key,
  Duration elapsed = default, Option<Interval> window = default, Option<CorrelationId> correlation = default, int attempt = 0) =>
  Accumulate(Seq(
    Gate(!string.IsNullOrWhiteSpace(author), "evidence-run-author", key,
     static (label, op) => (Error)new KernelFault.InvalidValue(label, "not be blank", Some(op))),
    Gate(!string.IsNullOrWhiteSpace(tool), "evidence-run-tool", key,
     static (label, op) => (Error)new KernelFault.InvalidValue(label, "not be blank", Some(op))),
    Gate(!string.IsNullOrWhiteSpace(version), "evidence-run-version", key,
     static (label, op) => (Error)new KernelFault.InvalidValue(label, "not be blank", Some(op))),
    In(elapsed.TotalSeconds, Band.Nonnegative, "evidence-run-elapsed-seconds", key).Map(static _ => unit),
    In(attempt, Band.Nonnegative, "evidence-run-attempt", key).Map(static _ => unit)))
   .ToFin()
   .Map(_ => new EvidenceRun(author.Trim(), tool.Trim(), version.Trim(), at, elapsed, window, correlation, attempt));

 internal EvidenceRun Retried() =>
  new(Author, Tool, Version, At, Elapsed, Window, Correlation, Attempt + 1);

 // The evidence-identity fold a PropertyEvidence.Run rides; Correlation excluded (stated above).
 public void CanonicalBytes(CanonicalWriter w) =>
  w.String(Author).String(Tool).String(Version).I64(At.ToUnixTimeTicks())
   .I64(Elapsed.BclCompatibleTicks).Ordinal(Attempt)
   .Optional(Window, static (span, run) => run.I64(span.Start.ToUnixTimeTicks()).I64(span.End.ToUnixTimeTicks()));
}

// --- [MODELS] -----------------------------------------------------------------------------
// [Equatable] is LOAD-BEARING: the id keys on the triple alone, so a re-solve/Land/Advance mutates the SAME
// node in place and the StructuralMerge drills to Nodes[id].Payload.Content / .Outcome member paths; Content's
// Results case carries the unordered bag, DependsOn is a structural Set.
[Equatable]
public sealed partial record AssessmentPayload {
 public Discipline Discipline { get; }
 public AnalysisRoute Route { get; }
 public UInt128 InputKey { get; }
 public AssessmentOutcome Outcome { get; }
 public PayloadContent Content { get; }
 // Upstream receipts the InputKey was DERIVED over — analysis DAG audit data (the derivation already folds the
 // upstream OUTPUT content into InputKey, so the id set is canon-excluded); a structural Set, distinctness by type.
 [property: SetEquality] public Set<NodeId> DependsOn { get; }
 public EvidenceRun Provenance { get; }

 // PRIVATE ctor + get-only members: every admission crosses Open (the ONE coherence-gated entry), the
 // adjacency-gated Advance, or the in-flight Land — a malformed lifecycle is unrepresentable even off a
 // tampered store, and no init/set survives for a `with` to bypass.
 private AssessmentPayload(
  Discipline discipline, AnalysisRoute route, UInt128 inputKey, AssessmentOutcome outcome,
  PayloadContent content, Set<NodeId> dependsOn, EvidenceRun provenance) =>
  (Discipline, Route, InputKey, Outcome, Content, DependsOn, Provenance) =
   (discipline, route, inputKey, outcome, content, dependsOn, provenance);

 // The ONE entry — fresh mint AND cross-assembly rehydration are one gate now that the payload is a VALUE the
 // row's Coherent law reads (the former Pending/Computed/Failed factory family and the separate Rehydrate
 // collapsed here; wire decode funnels through it). A persisted tuple is NOT trusted truth: a tampered
 // Computed-but-empty or Pending-carrying-results tuple refuses exactly as a fresh malformed mint does.
 public static Fin<AssessmentPayload> Open(
  Discipline discipline, AnalysisRoute route, UInt128 inputKey, AssessmentOutcome outcome,
  PayloadContent content, EvidenceRun provenance, Op key, Set<NodeId> dependsOn = default) =>
  outcome.Coherent(content)
   ? Fin.Succ(new AssessmentPayload(discipline, route, inputKey, outcome, content, dependsOn, provenance))
   : new ElementFault.ValueRejected(key, $"<assessment-incoherent:{outcome.Key}:{content.Kind}>");

 // Derived projections keep the consumer spellings one-hop over the ONE Content value.
 public Map<PropertyName, PropertyValue> Results =>
  Content is PayloadContent.ResultsCase results ? results.Values : Map<PropertyName, PropertyValue>();
 public Option<Diagnostic> Diagnostic =>
  Content is PayloadContent.FailureCase failure ? Some(failure.Diagnostic) : None;
 public Option<BlobKey> ResultBlob =>
  Content is PayloadContent.ResultsCase results ? results.Artifact : None;

 public Option<PropertyValue> Result(PropertyName name) => Results.Find(name);

 // Reads a dimensioned output directly; total over the bag, honestly absent for a non-measure entry.
 public Option<MeasureValue> ResultMeasure(PropertyName name) =>
  Result(name).Bind(static v => v is PropertyValue.Measure m ? Some(m.Value) : None);

 // ONLY the (Discipline, Route, InputKey) triple is content: the mutable Outcome/Content, the additive
 // EvidenceRun, and the DependsOn audit set are EXCLUDED, so a re-solve or a flip never forks the node id.
 public void CanonicalBytes(CanonicalWriter w) =>
  w.String(Discipline.Key).String(Route.Value).U128(InputKey);

 // Stale marking never deletes and never re-keys: the next Bake surfaces a Stale row the sweep re-dispatches
 // under the CURRENT inputs (a fresh key, a fresh node), the last-good value readable until the re-solve.
 public bool IsStaleFor(UInt128 currentInputKey) => InputKey != currentInputKey;

 // The ONE lifecycle flip: adjacency is row data, a cancel REQUIRES its abort diagnostic and no other flip
 // admits one, a Pending re-request clears content and advances the Attempt ordinal, a Superseded flip keeps
 // Content as readable history. Reconstruction routes Open, so the target row's Coherent law gates the result.
 public Fin<AssessmentPayload> Advance(AssessmentOutcome next, Op key, Option<Diagnostic> diagnostic = default) =>
  Accumulate(Seq(
    Gate(Outcome.Next().Exists(row => row == next), key, $"<assessment-flip-illegal:{Outcome.Key}->{next.Key}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
    Gate(diagnostic.IsSome == (next == AssessmentOutcome.Cancelled), key, $"<assessment-flip-diagnostic:{next.Key}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d))))
   .ToFin()
   .Bind(_ => Open(Discipline, Route, InputKey, next, Advanced(next, diagnostic), Retried(next), key, DependsOn));

 PayloadContent Advanced(AssessmentOutcome next, Option<Diagnostic> diagnostic) =>
  next == AssessmentOutcome.Pending
   ? PayloadContent.Empty
   : diagnostic.Match(Some: PayloadContent.Failure, None: () => Content);

 // Pending is reachable only from Failed/Cancelled (the adjacency), so the flip TO it IS the retry re-request
 // and the one edge advancing the Attempt ordinal.
 EvidenceRun Retried(AssessmentOutcome next) =>
  next == AssessmentOutcome.Pending ? Provenance.Retried() : Provenance;

 // The ONE outcome landing an in-flight node takes (the former Complete/Fail pair): the content CASE selects
 // the terminal row, identity triple and audit set carried by construction, the same Coherent gate crossing.
 // The former two fault tokens merge into one naming the content kind and the refusing outcome.
 public Fin<AssessmentPayload> Land(PayloadContent content, EvidenceRun provenance, Op key) =>
  Accumulate(Seq(
    Gate(Outcome.Capabilities.Admits(OutcomeCapability.InFlight), key, $"<assessment-land-not-in-flight:{content.Kind}:{Outcome.Key}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
    Gate(content is not PayloadContent.EmptyCase, key, $"<assessment-land-empty:{Outcome.Key}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d))))
   .ToFin()
   .Bind(_ => Open(Discipline, Route, InputKey,
     content is PayloadContent.FailureCase ? AssessmentOutcome.Failed : AssessmentOutcome.Computed,
     content, provenance, key, DependsOn));
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
