# [ELEMENT_ASSESSMENT]

`AssessmentPayload` owns the discipline-agnostic analysis receipt — one payload the `Graph/element#NODE_MODEL` `Node.Assessment` case wraps, keyed by the `Classification/classification#DISCIPLINE_AXIS` `Discipline`, a typed `AnalysisRoute` token, and a `UInt128` `InputKey` content key. Any analysis outcome an element carries — a structural utilization, an ISO 6946 U-value, an EnergyPlus annual-energy figure, an EN 15978 embodied-carbon result — lands as one `Assessment` node under that triple, so a route re-run over unchanged inputs resolves the stored receipt instead of re-solving.

`AssessmentPayload` holds the `Outcome`, the ONE `PayloadContent` value (empty, results with an optional `ArtifactContent`, or typed failure `Diagnostic`), the upstream-receipt `DependsOn` set, and the `EvidenceRun` audit. `Rasm.Compute` keeps discipline-specific solver shapes behind the opaque `AnalysisRoute` token.

`AssessmentOutcome` rows carry a kernel `CapabilitySet<OutcomeCapability>` (`Consumable`/`Settled`/`Dispatchable`/`InFlight`/`Reportable`), the per-row `Coherent` law over the `PayloadContent` value, and their legal flip set as row data, so one `Advance` validates every transition and the ONE `Open` entry gates fresh mints and persisted tuples alike. `AssessmentPayload` composes `Properties/property#PROPERTY_VALUE` for typed results and `NodaTime` for the audit instants; universal malformed inputs route through kernel admission while assessment coherence and transition refusals remain `ElementFault.ValueRejected`.

## [01]-[INDEX]

- [02]-[ASSESSMENT_NODE]: `AssessmentPayload` keys the generic receipt on `Discipline`+`AnalysisRoute`+`InputKey` and carries the ONE `PayloadContent` value, the `DependsOn` upstream set, and the `EvidenceRun` audit; `AssessmentOutcome` capability rows drive the lifecycle through `Advance`, the content-keyed `Land`, and the coherence-gated `Open`.

## [02]-[ASSESSMENT_NODE]

- Owner: `PayloadContent` is `Empty`, `Results(bag, Option<ArtifactContent>)`, or `Failure(Diagnostic)`; `ArtifactContent` is the kernel SHA-256-plus-extent coordinate for every heavy result.
- Law: DOUBLE-DISPATCH GUARD — the `Rasm.Compute` sweep's dedup predicate is the EXISTENCE of a non-terminal sibling node on the same `(Discipline, Route)`, never a flag on the stale node: a re-solve mints a FRESH node under the current `InputKey` and flips the old to `Superseded`, so the successor node IS the in-flight marker and the sweep skips any `Stale` row already carrying one. `Stale.Next()` therefore reaches `Superseded` alone — a `Stale → Queued` edge re-dispatches the OLD key, whose result is stale by definition. IDENTITY PRESERVATION — a solver that opened a node lands its outcome through the instance `Land`, which carries the `(Discipline, Route, InputKey)` triple and the `DependsOn` set forward by construction; `Open` is the fresh-mint entry a producer holding no prior node takes, and routing a write-back through it risks a re-spelled triple keying a different node than the one the sweep watches. RETRY ORDINAL — `EvidenceRun.Attempt` increments on exactly one edge, the `Failed`/`Cancelled → Pending` re-request `Advance` runs, so the bounded `Diagnostic.Kind.Transient` retry gate in Compute reads a real attempt count off the receipt rather than its own memory.
- Entry: `Open(discipline, route, inputKey, outcome, content, provenance, key, dependsOn)` is the ONE admission — fresh mint AND cross-assembly rehydration share the row's `Coherent(content)` gate, so a tampered store's malformed tuple refuses exactly as a fresh one (the former `Pending`/`Computed`/`Failed` factory family and the separate `Rehydrate` collapsed here; a producer opening a run passes `PayloadContent.Empty` under `Pending`); `Land(content, provenance, key)` is the ONE outcome landing an in-flight node takes — the content CASE selects `Computed` or `Failed`, the identity triple and `DependsOn` carried by construction (the former `Complete`/`Fail` pair; the two fault tokens merged as `<assessment-land-not-in-flight:{content.Kind}:{Outcome.Key}>`); `Advance(next, key, diagnostic)` the ONE in-place flip over the row adjacency (a cancel REQUIRES its abort diagnostic, a `Pending` re-request clears content and advances `Attempt`); `PayloadContent.Results(values, artifact, key)`/`.Failure(diagnostic)` the payload mints; `Diagnostic.Of(phase, kind, message, key)` the failure admission; `EvidenceRun.Of(author, tool, version, at, key, …)` the gated audit mint; `Result(name)`/`ResultMeasure(name)` the flat typed reads; `IsStaleFor(currentInputKey)` the staleness probe; `AnalysisRoute.Of(token, key)` the railed route admission.
- Auto: the `(Discipline, Route, InputKey)` triple is the cache key — and the `Node.Assessment` id is the node's OWN content SELF-HASH `Graph/element#NODE_MODEL` `NodeId.Of(new NodeSeed.Content(node, tolerance))`, the `Node.Assessment` arm of `CanonicalBytes` writing its case ordinal then DELEGATING to the payload-owned `AssessmentPayload.CanonicalBytes` (the `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition.CanonicalBytes` co-location discipline — each complex payload owns its own canonical contribution rather than the `Node` arm re-spelling it), which writes exactly that triple, NEVER a `NodeSeed.Precomputed` wrap of the `InputKey` — the `InputKey` is a payload field the self-hash FOLDS, not a foreign id substituted for the node id, so the id is computable pre-run by a `Rasm.Compute` author and a rehydrated Compute-authored Assessment node passes the `Projection/address#CONTENT_ADDRESS` `Verify` re-hash dual (which re-mints through the node's own `Content` seed and compares to the stored id); two assessments of one route over identical inputs hash to one id and ARE one node: a solver computes `InputKey` from the assessed inputs' content (the `Composition/material#MATERIAL_COMPOSITION` plies, the geometry content hash, the load case) through the kernel `XxHash128`, and a `Rasm.Compute` route resolving the existing `Computed` node rather than re-solving is the cache hit; `DependsOn` is a structural `Set<NodeId>`, so distinctness is the TYPE's, never a normalizing fold; because the outcome is NOT in the content key, every legal `Advance` flip mutates the SAME node in place without minting a new id; the three behavior columns partition the lifecycle for every consumer with zero per-state branches — `Capabilities.Admits(Consumable)` gates the value filter, `Admits(Settled)` marks the solver settled for this key, `Admits(Dispatchable)` marks what the sweep may dispatch (`Pending`/`Stale`; the in-flight rows are neither settled nor dispatchable), `Admits(Reportable)` feeds the audit defect sweep — `Cancelled` carries it, closing the silent drop — and the typed `Results` bag carries each output as a `Properties/property#PROPERTY_VALUE` so a consumer reads `assessment.Result("Utilization")` without learning the solver's wire shape.
- Receipt: an `Assessment` node is the analysis evidence a `Bake`-derived `Element` carries flat — `element.Assessments.Filter(a => a.Discipline == Discipline.Energy && a.Outcome.Capabilities.Admits(OutcomeCapability.Consumable))` reads every usable energy result, `assessment.ResultMeasure(name)` reads a dimensioned output as a `MeasureValue` directly (and `Result(name)` the raw `PropertyValue` for a non-measure output), `assessment.ResultArtifact` fetches the heavy artifact by content key, `assessment.Diagnostic` reads a failure's phase/kind/message/code typed — a re-solve gate reads the diagnostic's `Kind.Transient` column and the dispatch sweep reads `Outcome.Capabilities.Admits(Dispatchable)` off the row, two orthogonal column reads, never a message-text probe; `assessment.Provenance.Elapsed` (`EvidenceRun`) reads the solve compute cost and `Provenance.Window` the wall-clock start→end so a route-cost report is a fold over receipts, never a log join; the `Rasm.Compute` analysis route writes the `Computed` node back keyed on `(InputKey, Route)`, and the seam carries the receipt without owning the solver — the discipline-specific input/result shapes, the FEA/EnergyPlus/EC3 runners, and the multi-ply `AssemblyAggregator` all live in Compute.
- Packages: `Rasm.Domain.ArtifactContent`, Thinktecture.Runtime.Extensions, Generator.Equals, LanguageExt.Core, NodaTime.
- Growth: a new analysis discipline is one `Discipline` row; a new route one Compute-minted `AnalysisRoute` token; a new result one `Results` bag entry; a new dependency one `DependsOn` member; a new lifecycle state is one `AssessmentOutcome` row carrying its capability set, `Coherent` law, and adjacency — `Open`, `Land`, `Advance`, and every capability-filtered consumer absorb it with zero edits; a new payload shape is one `PayloadContent` case + the rows that admit it; a new failure cause one `FailureKind` row, a new stage one `SolvePhase` row; never a per-discipline assessment type, a per-route enum, a per-state flip method, or a solver I/O shape on the seam.
- Boundary: solver I/O and route rosters remain in Compute. Assessment DAG links ride `DependsOn`; heavy output rides kernel `ArtifactContent`; neither becomes an edge codec or inline byte payload. The node self-hash excludes audit and lifecycle state, while the route or input key includes solver version so a tool change mints a fresh assessment.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
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

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError]
public sealed partial class AnalysisRoute {
 private static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
  if (string.IsNullOrWhiteSpace(value)) { validationError = new ValidationError("analysis route requires a non-blank token"); return; }
  value = value.Trim().ToLowerInvariant();
 }

 public static Fin<AnalysisRoute> Of(string token, Op key) =>
  key.AcceptValidated<AnalysisRoute>(token);
}

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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OutcomeCapability : ICapability<OutcomeCapability> {
 public static readonly OutcomeCapability Consumable = new("consumable");
 public static readonly OutcomeCapability Settled = new("settled");
 public static readonly OutcomeCapability Dispatchable = new("dispatchable");
 public static readonly OutcomeCapability InFlight = new("in-flight");
 public static readonly OutcomeCapability Reportable = new("reportable");
}

[Union]
public abstract partial record PayloadContent {
 private PayloadContent() { }

 public sealed record EmptyCase : PayloadContent;
 [Equatable]
 public sealed partial record ResultsCase : PayloadContent {
  internal ResultsCase(Map<PropertyName, PropertyValue> values, Option<ArtifactContent> artifact) => (Values, Artifact) = (values, artifact);
  [property: UnorderedEquality] public Map<PropertyName, PropertyValue> Values { get; }
  public Option<ArtifactContent> Artifact { get; }
 }
 public sealed record FailureCase(Diagnostic Diagnostic) : PayloadContent;

 public static readonly PayloadContent Empty = new EmptyCase();

 public static Fin<PayloadContent> Results(Map<PropertyName, PropertyValue> values, Option<ArtifactContent> artifact, Op key) =>
  values.IsEmpty && artifact.IsNone
   ? new ElementFault.ValueRejected(key, "<assessment-results-empty>")
   : Fin.Succ<PayloadContent>(new ResultsCase(values, artifact));

 public static PayloadContent Failure(Diagnostic diagnostic) => new FailureCase(diagnostic);

 public string Kind => Switch(empty: static _ => "empty", results: static _ => "results", failure: static _ => "failure");
}

[SmartEnum<string>]
public sealed partial class SolvePhase {
 public static readonly SolvePhase Admission = new("admission");
 public static readonly SolvePhase Solve = new("solve");
 public static readonly SolvePhase Extraction = new("extraction");
 public static readonly SolvePhase Publication = new("publication");
}

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

 public static Fin<Diagnostic> Of(SolvePhase phase, FailureKind kind, string message, Op key, Option<int> code = default) =>
  key.AcceptValidated<Diagnostic>(Validate(phase, kind, message, code, out Diagnostic value), value);
}

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

 public void CanonicalBytes(CanonicalWriter w) =>
  w.String(Author).String(Tool).String(Version).I64(At.ToUnixTimeTicks())
   .I64(Elapsed.BclCompatibleTicks).Ordinal(Attempt)
   .Optional(Window, static (span, run) => run.I64(span.Start.ToUnixTimeTicks()).I64(span.End.ToUnixTimeTicks()));
}

// --- [MODELS] --------------------------------------------------------------------------
[Equatable]
public sealed partial record AssessmentPayload {
 public Discipline Discipline { get; }
 public AnalysisRoute Route { get; }
 public UInt128 InputKey { get; }
 public AssessmentOutcome Outcome { get; }
 public PayloadContent Content { get; }
 [property: SetEquality] public Set<NodeId> DependsOn { get; }
 public EvidenceRun Provenance { get; }

 private AssessmentPayload(
  Discipline discipline, AnalysisRoute route, UInt128 inputKey, AssessmentOutcome outcome,
  PayloadContent content, Set<NodeId> dependsOn, EvidenceRun provenance) =>
  (Discipline, Route, InputKey, Outcome, Content, DependsOn, Provenance) =
   (discipline, route, inputKey, outcome, content, dependsOn, provenance);

 public static Fin<AssessmentPayload> Open(
  Discipline discipline, AnalysisRoute route, UInt128 inputKey, AssessmentOutcome outcome,
  PayloadContent content, EvidenceRun provenance, Op key, Set<NodeId> dependsOn = default) =>
  outcome.Coherent(content)
   ? Fin.Succ(new AssessmentPayload(discipline, route, inputKey, outcome, content, dependsOn, provenance))
   : new ElementFault.ValueRejected(key, $"<assessment-incoherent:{outcome.Key}:{content.Kind}>");

 public Map<PropertyName, PropertyValue> Results =>
  Content is PayloadContent.ResultsCase results ? results.Values : Map<PropertyName, PropertyValue>();
 public Option<Diagnostic> Diagnostic =>
  Content is PayloadContent.FailureCase failure ? Some(failure.Diagnostic) : None;
 public Option<ArtifactContent> ResultArtifact =>
  Content is PayloadContent.ResultsCase results ? results.Artifact : None;

 public Option<PropertyValue> Result(PropertyName name) => Results.Find(name);

 public Option<MeasureValue> ResultMeasure(PropertyName name) =>
  Result(name).Bind(static v => v is PropertyValue.Measure m ? Some(m.Value) : None);

 public void CanonicalBytes(CanonicalWriter w) =>
  w.String(Discipline.Key).String(Route.Value).U128(InputKey);

 public bool IsStaleFor(UInt128 currentInputKey) => InputKey != currentInputKey;

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

 EvidenceRun Retried(AssessmentOutcome next) =>
  next == AssessmentOutcome.Pending ? Provenance.Retried() : Provenance;

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
