# [ELEMENT_ASSESSMENT]

The generic analysis-receipt owner: one `AssessmentPayload` the `Graph/element#NODE_MODEL` `Node.Assessment` case wraps, keyed by the `Classification/classification#DISCIPLINE_AXIS` `Discipline`, a typed `AnalysisRoute` token, and a `UInt128` `InputKey` content key, carrying its lifecycle `AssessmentOutcome`, a typed flat `Results` bag, an optional failure `Detail`, an optional heavy-result blob reference, and its `Provenance`. An assessment is the seam's discipline-agnostic carrier for ANY analysis outcome attached to an element: a structural FEA utilization, an ISO 6946 U-value, an EnergyPlus annual-energy result, an ISO 12354 assembly STC, an EN 1992/1993-1-2 fire rating, an EN 15978 LCA, an EC3 embodied-carbon figure — each lands as an `Assessment` node attached to its object through an `Relations/relation#EDGE_ALGEBRA` `Assign` edge (sub-kind `AssignKind.Assessment`, authored by the `Rasm.Compute` analysis producer on write-back — an imported IFC objectified-assessment/`AssignsToControl` relation round-trips through the `Rasm.Bim` projector's NEUTRAL edge algebra, never a typed IFC case on the seam), keyed by `(Discipline, Route, InputKey)` so re-running an analysis on unchanged inputs is a cache hit. The payload is OPAQUE and TYPED: the seam carries the `Discipline`, the `AnalysisRoute`, the input content key, the `Outcome`, a typed `Results` bag a consumer reads flat, the failure `Detail` diagnostic (a raw `Option<string>` — a foreign solver's message, the discipline-specific failure shape staying in `Rasm.Compute`), an optional `ResultBlob` content key to the heavy artifact (the EnergyPlus SQLite, the FEA result set), and the `Provenance` — but NOT the discipline-specific solver I/O shapes, which live in `Rasm.Compute`, nor the geometry, which is content-keyed. The `AnalysisRoute` is itself OPAQUE — the route roster is `Rasm.Compute`'s (the seam never enumerates it, the SAME neutrality `Classification.System` holds for the standards roster), so the cache key is a typed triple rather than a stringly-keyed one. The multi-ply `AssemblyAggregator` (series-resistance U-value, rule-of-mixtures density, layered STC) ALSO lives in `Rasm.Compute`, reading the `Composition/material#MATERIAL_COMPOSITION` plies and writing its result back as an `Assessment` node. The page composes `Properties/property#PROPERTY_VALUE` for the typed results, `Classification/classification#DISCIPLINE_AXIS` for the keying, and `NodaTime` for the provenance instant; a malformed result rails `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected`.

## [01]-[INDEX]

- [01]-[ASSESSMENT_NODE]: the `AssessmentPayload` generic receipt keyed by `Discipline`+`AnalysisRoute`+`InputKey`, the `AnalysisRoute` opaque route token, the `AssessmentOutcome` `Usable`/`Terminal` lifecycle, the typed `Results` bag and failure `Detail`, the `ResultBlob` heavy-artifact reference, and the `Provenance`.

## [02]-[ASSESSMENT_NODE]

- Owner: `AssessmentPayload` the generic discipline-keyed analysis receipt the `Node.Assessment` case wraps; `AnalysisRoute` the `[ValueObject<string>]` opaque route token (the route roster is `Rasm.Compute`'s, never the seam's); `AssessmentOutcome` the `[SmartEnum<string>]` lifecycle (`Pending`/`Computed`/`Failed`/`Stale`) carrying the `Usable`/`Terminal` behavior columns; `Provenance` the who/when/tool record carried on every assessment.
- Entry: `AssessmentPayload.Pending(discipline, route, inputKey, provenance)` opens an assessment a solver will fill; `Computed(discipline, route, inputKey, results, resultBlob, provenance, key)` records a completed result, `Fin<T>` railing `ElementFault.ValueRejected` on an empty result bag AND no blob; `Failed(discipline, route, inputKey, detail, provenance)` records a solver failure carrying its diagnostic in the `Detail` slot; `Result(name)` reads a typed result flat; `IsStaleFor(currentInputKey)` tests whether the stored `InputKey` still matches the element's current inputs so a changed input marks the cached result stale; `AsStale()` flips the outcome to `Stale` in place; `AnalysisRoute.Create(token)` admits a normalized route token, railing a blank. The `Pending`/`Computed`/`Failed` factories (plus `AsStale`/`Seed`) are the ONLY admission — the record constructor is PRIVATE and `internal Seed` is the decoder re-hydration escape — so a malformed lifecycle (a `Pending` carrying results, a `Failed` with a populated bag, a `Computed` with a `Detail`) is UNREPRESENTABLE, the sibling `Composition/material#MATERIAL_COMPOSITION` `LayerSet` admission discipline.
- Auto: the `(Discipline, Route, InputKey)` triple is the cache key — and the `Node.Assessment` id is the node's OWN content SELF-HASH `Graph/element#NODE_MODEL` `NodeId.Content(node.ToCanonicalBytes(tolerance))` (the `Node.Assessment` arm of `ToCanonicalBytes` writes exactly that triple), NEVER `NodeId.OfContent(InputKey)` — the `InputKey` is a payload field the self-hash FOLDS, not a foreign id substituted for the node id, so the id is computable pre-run by a `Rasm.Compute` author and a rehydrated Compute-authored Assessment node passes the `Projection/address#CONTENT_ADDRESS` `Verify` re-hash dual (which recomputes `ContentHash.Of(node.ToCanonicalBytes(tolerance))` and compares to the stored id); two assessments of one route over identical inputs hash to one id and ARE one node: a solver computes `InputKey` from the assessed inputs' content (the `Composition/material#MATERIAL_COMPOSITION` plies, the geometry content hash, the load case) through the kernel `XxHash128`, and a `Rasm.Compute` route resolving the existing `Computed` node rather than re-solving is the cache hit; `IsStaleFor` compares the stored `InputKey` to a freshly-derived one so a downstream edit marks the assessment `Stale` (the outcome is NOT in the content key, so `AsStale` updates in place without minting a new id); the typed `Results` bag carries each output as a `Properties/property#PROPERTY_VALUE` (a `Measure` for a utilization ratio, a U-value, a GWP figure) so a consumer reads `assessment.Result("Utilization")` without learning the solver's wire shape, and the `Outcome` behavior columns (`Usable`/`Terminal`) drive a consumer filter rather than a per-state branch.
- Receipt: an `Assessment` node is the analysis evidence a `Bake`-derived `Element` carries flat — `element.Assessments.Filter(a => a.Discipline == Discipline.Energy && a.Outcome.Usable)` reads every usable energy result, `assessment.ResultBlob` fetches the heavy artifact by content key, `assessment.Detail` reads a failure diagnostic; the `Rasm.Compute` analysis route writes the `Computed` node back keyed on `(InputKey, Route)`, and the seam carries the receipt without owning the solver — the discipline-specific input/result shapes, the FEA/EnergyPlus/EC3 runners, and the multi-ply `AssemblyAggregator` all live in Compute.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`/`[ValueObject<string>]`), LanguageExt.Core (`Map`/`Option`/`Fin`), NodaTime (`Instant` provenance), `Rasm` (the kernel `Op` op-key + the content-key seed the `InputKey`/`ResultBlob` share).
- Growth: a new analysis discipline is one `Classification/classification#DISCIPLINE_AXIS` `Discipline` row the assessment keys on (no seam edit beyond the discipline); a new analysis route is one `AnalysisRoute` token a `Rasm.Compute` solver mints (the seam never grows a route roster); a new result is one entry in the typed `Results` bag; a new lifecycle state is one `AssessmentOutcome` row carrying its `Usable`/`Terminal` columns; never a per-discipline assessment type, never a per-route enum on the seam, and never a solver I/O shape on the seam.
- Boundary: `AssessmentPayload` is GENERIC and OPAQUE — the discipline-specific solver input/result shapes (the FEA load/support model, the EnergyPlus IDF, the EC3 request) live in `Rasm.Compute`, the seam carrying only the `Discipline`, the `AnalysisRoute`, the content-keyed inputs, the typed flat `Results`, the typed failure `Detail`, and the optional heavy `ResultBlob` reference, so a new solver needs no seam edit; the `AnalysisRoute` is an OPAQUE token the seam never enumerates — the route roster (`"iso-6946-u"`, `"energyplus-annual"`, `"fea-utilization"`) is `Rasm.Compute`'s, the SAME neutrality `Classification.System` holds for the standards roster, so a raw `string` route on the payload is the deleted form; the assessment attaches to its object through an `Relations/relation#EDGE_ALGEBRA` `Assign` edge (sub-kind `AssignKind.Assessment`, authored by the `Rasm.Compute` producer on write-back, the `Rasm.Bim` projector round-tripping an IFC `AssignsToControl`/assessment-family relation through the neutral edge algebra), never an inlined back-reference on the `Object` node; the `(InputKey, Route)` cache key is derived through the kernel `XxHash128` content hash so re-running an unchanged analysis is a cache hit and a changed input marks the result `Stale`, never a silent recompute or a stale-but-`Computed` lie; the `Node.Assessment` id is the self-hash of `ToCanonicalBytes(Discipline, Route, InputKey)` minted through `Graph/element#NODE_MODEL` `NodeId.Content` (the form the `Projection/address#CONTENT_ADDRESS` `Verify` dual recomputes), the `InputKey` a payload FIELD the triple folds and NEVER the node id itself — a producer minting the node id as `NodeId.OfContent(InputKey)` stores an id `Verify` cannot reproduce, the deleted form; because the cache identity EXCLUDES `Provenance` (a re-export under a new author/instant must NOT fork it), the `AnalysisRoute` token OR the `InputKey` MUST fold the solver tool+version (a `Rasm.Compute` obligation, the route opaque to the seam) so a solver-version bump — EnergyPlus 25.2.0→26.1.0, a closed-form revision — re-keys to a FRESH node rather than false-hitting a prior version's `Computed` result, the `Provenance` Tool/Version being the audit of WHICH solver produced a value, never a substitute for that re-keying; a solver failure carries its diagnostic in the dedicated `Detail` slot (a raw `Option<string>`, the foreign message preserved the same way `Projection/fault#FAULT_BAND` `ProjectionFailed` keeps a captured exception's text), never smuggled as a fake `Results` entry, so the bag stays the consumable-output store and `Outcome.Usable=false` reads true to its empty bag; the heavy result artifact rides the content-keyed blob store by `ResultBlob` (one `XxHash128` seed), never inlined on the node; the multi-ply `AssemblyAggregator` is a `Rasm.Compute` fold over the `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition` plies writing its receipt back as an `Assessment` node, never a seam owner.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
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
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AnalysisRoute {
 static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
  if (string.IsNullOrWhiteSpace(value)) { validationError = new ValidationError("analysis route requires a non-blank token"); return; }
  value = value.Trim().ToLowerInvariant();
 }
}

// The assessment lifecycle read through behavior columns, NOT a bare key: Usable gates whether the Results bag
// carries a consumable value (a consumer filters element.Assessments on Outcome.Usable); Terminal marks the solver
// settled, so a Rasm.Compute re-solve sweep targets the non-terminal rows — Pending awaits a first solve, Stale a
// re-solve under the changed inputs that mint a new node (Computed/Failed are settled for their own input key).
[SmartEnum<string>]
public sealed partial class AssessmentOutcome {
 public static readonly AssessmentOutcome Pending = new("pending", usable: false, terminal: false);
 public static readonly AssessmentOutcome Computed = new("computed", usable: true, terminal: true);
 public static readonly AssessmentOutcome Failed = new("failed", usable: false, terminal: true);
 public static readonly AssessmentOutcome Stale = new("stale", usable: true, terminal: false);

 public bool Usable { get; }
 public bool Terminal { get; }
}

// The who/when/tool record carried on every assessment — the analysis provenance (requesting author, solver
// tool + version, instant), a SEPARATE additive axis from the IFC OwnerHistory and the Marten event provenance.
public readonly record struct Provenance(string Author, string Tool, string Version, Instant At);

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record AssessmentPayload {
 public Discipline Discipline { get; init; }
 public AnalysisRoute Route { get; init; }
 public UInt128 InputKey { get; init; }
 public AssessmentOutcome Outcome { get; init; }
 public Map<PropertyName, PropertyValue> Results { get; init; }
 public Option<string> Detail { get; init; }
 public Option<UInt128> ResultBlob { get; init; }
 public Provenance Provenance { get; init; }

 // PRIVATE ctor — every admission crosses a factory so a lifecycle state is STRUCTURALLY well-formed: a Pending
 // carrying Results, a Failed with a populated Results bag, or a Computed carrying a Detail are UNREPRESENTABLE
 // (the sibling Composition/material#MATERIAL_COMPOSITION LayerSet / Composition/acoustic#ACOUSTIC_FOLDS Acoustic
 // private-ctor+Seed admission shape), so a consumer's Outcome.Usable filter TRUSTS the Results/Detail invariant a
 // public positional ctor would let a caller violate. AsStale's `this with { Outcome }` reaches the sealed record's
 // private copy ctor from inside the body; external `with`/`new` is compile-blocked, the factories the only ingress.
 private AssessmentPayload(
  Discipline discipline, AnalysisRoute route, UInt128 inputKey, AssessmentOutcome outcome,
  Map<PropertyName, PropertyValue> results, Option<string> detail, Option<UInt128> resultBlob, Provenance provenance) =>
  (Discipline, Route, InputKey, Outcome, Results, Detail, ResultBlob, Provenance) =
   (discipline, route, inputKey, outcome, results, detail, resultBlob, provenance);

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

 // A solver failure carries its diagnostic in the typed Detail — NOT smuggled as a fake Results entry, so the
 // bag stays the consumable-output store and Outcome.Usable=false reads true to its empty bag.
 public static AssessmentPayload Failed(Discipline discipline, AnalysisRoute route, UInt128 inputKey, string detail, Provenance provenance) =>
  new(discipline, route, inputKey, AssessmentOutcome.Failed, Map<PropertyName, PropertyValue>(), Some(detail), None, provenance);

 // The infallible re-hydration escape the Rasm.Persistence/Rasm.Bim decoders reconstruct an already-validated
 // payload through (the Composition/material#MATERIAL_COMPOSITION Seed shape) — the persisted (discipline, route,
 // inputKey, outcome, results, detail, resultBlob, provenance) tuple re-admitted without re-running the Computed
 // empty-result guard, the ONLY path that reconstructs a Stale/Failed/Computed state directly off the stream.
 internal static AssessmentPayload Seed(
  Discipline discipline, AnalysisRoute route, UInt128 inputKey, AssessmentOutcome outcome,
  Map<PropertyName, PropertyValue> results, Option<string> detail, Option<UInt128> resultBlob, Provenance provenance) =>
  new(discipline, route, inputKey, outcome, results, detail, resultBlob, provenance);

 public Option<PropertyValue> Result(PropertyName name) => Results.Find(name);

 // A changed input content key marks the cached result stale WITHOUT deleting it — and without changing the node
 // id, which keys on (Discipline, Route, InputKey) not the outcome — so the next Bake surfaces a Stale assessment
 // a consumer re-requests through the Compute route, the last-good value staying readable until the re-solve.
 public bool IsStaleFor(UInt128 currentInputKey) => InputKey != currentInputKey;

 public AssessmentPayload AsStale() => this with { Outcome = AssessmentOutcome.Stale };
}
```

## [03]-[RESEARCH]

- [GENERIC_ASSESSMENT]: the seam `AssessmentPayload` is discipline-agnostic by design — the `(Discipline, AnalysisRoute, InputKey)` triple keys ANY analysis outcome, the typed `Results` bag carries the flat outputs, the `Detail` (a raw `Option<string>`) the failure diagnostic, and the `ResultBlob` references the heavy artifact, so the structural FEA (VividOrange/BriefFiniteElement/FEALiTE2D), the thermal closed-form (ISO 6946 U / EN 13788 Glaser), the energy simulation (NREL.OpenStudio model + EnergyPlus subprocess), the acoustic (ISO 12354), the fire (EN 199x-1-2), and the LCA (EN 15978 + EC3 REST) routes all write the same node shape — the discipline-specific input/result shapes and the runners living in `Rasm.Compute`, the seam owning only the receipt; the assessment is a generic opaque typed payload, not a per-discipline node family, and the `AnalysisRoute` is the opaque method discriminant Compute mints (the seam never enumerating the route roster, the SAME neutrality the `Classification` collapse holds for the standards roster).
- [CONTENT_KEYED_CACHE]: the `(InputKey, AnalysisRoute)` cache key is derived through the kernel `XxHash128` over the assessed inputs' content (`Projection/address#CONTENT_ADDRESS`), and the `Node.Assessment` id itself is content-keyed on the `(Discipline, Route, InputKey)` triple (`Graph/element#NODE_MODEL` `ToCanonicalBytes`), so two assessments of one route over identical inputs ARE one node and the `Rasm.Compute` route resolves the existing `Computed` assessment rather than re-solving; `IsStaleFor` marks a result `Stale` when a downstream edit changes the input key — the assessment cache is the same content-addressed discipline the geometry blob store and the snapshot spine use, never a wall-clock or a version-number cache; the assessment attaches through an `Assign` edge (sub-kind `AssignKind.Assessment`, the `Rasm.Compute` producer authoring it on write-back) so the graph carries the element→assessment binding, the `Rasm.Bim` projector round-tripping the IFC objectified-assessment relation through the neutral edge algebra at egress.
- [LIFECYCLE_STALENESS]: the `AssessmentOutcome` is a behavior-bearing lifecycle, not a bare token — the `Usable` column gates whether the `Results` bag carries a consumable value (a consumer filters `element.Assessments` on `Outcome.Usable` rather than a per-state branch) and the `Terminal` column marks the solver settled, so the `Rasm.Compute` re-solve sweep targets the non-terminal rows (`Pending` awaiting a first solve, `Stale` a re-solve under the changed inputs that mint a new node; `Computed`/`Failed` settled for their own input key) and the analysis dispatcher routes on the policy columns the rows carry; because the node id keys on `(Discipline, Route, InputKey)` and NOT the outcome, `AsStale` flips `Computed`→`Stale` in place without minting a new id, so a changed input leaves the last-good value readable-but-stale until the route re-solves under the new input key (a fresh `Computed` node), and a `Failed` assessment carries its diagnostic in the `Detail` slot so the empty `Results` bag reads true to `Usable=false`, never a fake result entry.
