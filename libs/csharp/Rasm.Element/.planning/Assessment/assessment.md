# [ELEMENT_ASSESSMENT]

The generic analysis-receipt owner: one `AssessmentPayload` the `Graph/element#NODE_MODEL` `Node.Assessment` case wraps, keyed by the `Classification/classification#DISCIPLINE_AXIS` `Discipline`, the analysis `Route`, and an `InputKey` content key, carrying its typed flat results, an optional heavy-result blob reference, and its `Provenance`. An assessment is the seam's discipline-agnostic carrier for ANY analysis outcome attached to an element: a structural FEA utilization, an ISO 6946 U-value, an EnergyPlus annual-energy result, an ISO 12354 assembly STC, an EN 1992/1993-1-2 fire rating, an EN 15978 LCA, an EC3 embodied-carbon figure — each lands as an `Assessment` node attached to its object through an `Relations/relation#EDGE_ALGEBRA` `Assign` edge (the IFC `AssignsToControl`/objectified-assessment relation the Bim projector maps), keyed by `(InputKey, Route)` so re-running an analysis on unchanged inputs is a cache hit. The payload is OPAQUE and TYPED: the seam carries the `Discipline`, the route, the input content key, the lifecycle `Outcome`, a typed `Results` bag a consumer reads flat, an optional `ResultBlob` content key to the heavy artifact (the EnergyPlus SQLite, the FEA result set), and the `Provenance` — but NOT the discipline-specific solver I/O shapes, which live in `Rasm.Compute`, nor the geometry, which is content-keyed. The multi-ply `AssemblyAggregator` (series-resistance U-value, rule-of-mixtures density, layered STC) ALSO lives in `Rasm.Compute`, reading the `Composition/material#MATERIAL_NODE` plies and writing its result back as an `Assessment` node. The page composes `Properties/property#PROPERTY_VALUE` for the typed results, `Classification/classification#DISCIPLINE_AXIS` for the keying, and `NodaTime` for the provenance instant; a malformed result rails `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected`.

## [01]-[INDEX]

- [01]-[ASSESSMENT_NODE]: the `AssessmentPayload` generic receipt keyed by `Discipline`+`Route`+`InputKey`, the `AssessmentOutcome` lifecycle, the typed `Results` bag, the `ResultBlob` heavy-artifact reference, and the `Provenance`.

## [02]-[ASSESSMENT_NODE]

- Owner: `AssessmentPayload` the generic discipline-keyed analysis receipt the `Node.Assessment` case wraps; `AssessmentOutcome` the `[SmartEnum<string>]` lifecycle (`Pending`/`Computed`/`Failed`/`Stale`); `Provenance` the who/when/tool record carried on every assessment.
- Entry: `AssessmentPayload.Pending(discipline, route, inputKey, provenance)` opens an assessment a solver will fill; `Computed(discipline, route, inputKey, results, resultBlob, provenance, key)` records a completed result, `Fin<T>` railing `ElementFault.ValueRejected` on an empty result bag for a computed assessment; `Failed(discipline, route, inputKey, detail, provenance)` records a solver failure; `Result(name)` reads a typed result flat; `IsStaleFor(currentInputKey)` tests whether the assessment's `InputKey` still matches the element's current inputs so a changed input marks the cached result stale.
- Auto: the `(Discipline, Route, InputKey)` triple is the cache key — a solver computes `InputKey` from the assessed inputs' content (the `Composition/material#MATERIAL_NODE` plies, the geometry content hash, the load case) through the kernel `XxHash128`, so re-running the same route on the same inputs resolves the existing `Computed` node rather than re-solving; `IsStaleFor` compares the stored `InputKey` to a freshly-derived one so a downstream edit marks the assessment `Stale` without deleting it; the typed `Results` bag carries each output as a `Properties/property#PROPERTY_VALUE` (a `Measure` for a utilization ratio, a U-value, a GWP figure) so a consumer reads `assessment.Result("Utilization")` without learning the solver's wire shape.
- Receipt: an `Assessment` node is the analysis evidence a `Bake`-derived `Element` carries flat — `element.Assessments.Filter(a => a.Discipline == Discipline.Energy)` reads every energy result, `assessment.ResultBlob` fetches the heavy artifact by content key; the `Rasm.Compute` analysis route writes the `Computed` node back keyed on `(InputKey, Route)`, and the seam carries the receipt without owning the solver — the discipline-specific input/result shapes, the FEA/EnergyPlus/EC3 runners, and the multi-ply `AssemblyAggregator` all live in Compute.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`), LanguageExt.Core (`Map`/`Option`/`Fin`), NodaTime (`Instant` provenance), `Rasm` (the kernel `Op` op-key + the content-key seed the `InputKey`/`ResultBlob` share).
- Growth: a new analysis discipline is one `Classification/classification#DISCIPLINE_AXIS` `Discipline` row the assessment keys on (no seam edit beyond the discipline); a new result is one entry in the typed `Results` bag; a new lifecycle state is one `AssessmentOutcome` row; never a per-discipline assessment type and never a solver I/O shape on the seam.
- Boundary: `AssessmentPayload` is GENERIC and OPAQUE — the discipline-specific solver input/result shapes (the FEA load/support model, the EnergyPlus IDF, the EC3 request) live in `Rasm.Compute`, the seam carrying only the `Discipline`, the route, the content-keyed inputs, the typed flat `Results`, and the optional heavy `ResultBlob` reference, so a new solver needs no seam edit; the assessment attaches to its object through an `Relations/relation#EDGE_ALGEBRA` `Assign` edge (the objectified-assessment relation the Bim projector maps from the IFC `AssignsToControl`/assessment family), never an inlined back-reference on the `Object` node; the `(InputKey, Route)` cache key is derived through the kernel `XxHash128` content hash so re-running an unchanged analysis is a cache hit and a changed input marks the result `Stale`, never a silent recompute or a stale-but-`Computed` lie; the heavy result artifact rides the content-keyed blob store by `ResultBlob` (one `XxHash128` seed), never inlined on the node; the multi-ply `AssemblyAggregator` is a `Rasm.Compute` fold over the `MaterialComposition` plies writing its receipt back as an `Assessment` node, never a seam owner.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class AssessmentOutcome {
 public static readonly AssessmentOutcome Pending = new("pending");
 public static readonly AssessmentOutcome Computed = new("computed");
 public static readonly AssessmentOutcome Failed = new("failed");
 public static readonly AssessmentOutcome Stale = new("stale");
}

public readonly record struct Provenance(string Author, string Tool, string Version, Instant At) {
 public static Provenance Of(string author, string tool, string version, Instant at) => new(author, tool, version, at);
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record AssessmentPayload(
 Discipline Discipline,
 string Route,
 UInt128 InputKey,
 AssessmentOutcome Outcome,
 Map<PropertyName, PropertyValue> Results,
 Option<UInt128> ResultBlob,
 Provenance Provenance) {

 public static AssessmentPayload Pending(Discipline discipline, string route, UInt128 inputKey, Provenance provenance) =>
 new(discipline, route, inputKey, AssessmentOutcome.Pending, Map<PropertyName, PropertyValue>(), None, provenance);

 public static Fin<AssessmentPayload> Computed(
 Discipline discipline, string route, UInt128 inputKey,
 Map<PropertyName, PropertyValue> results, Option<UInt128> resultBlob, Provenance provenance, Op key) =>
 results.IsEmpty && resultBlob.IsNone
 ? ElementFault.ValueRejected(key, $"<assessment-computed-empty:{discipline.Key}:{route}>")
 : Fin.Succ(new AssessmentPayload(discipline, route, inputKey, AssessmentOutcome.Computed, results, resultBlob, provenance));

 public static AssessmentPayload Failed(Discipline discipline, string route, UInt128 inputKey, string detail, Provenance provenance) =>
 new(discipline, route, inputKey, AssessmentOutcome.Failed,
 Map((PropertyName.Create("fault"), (PropertyValue)new PropertyValue.Text(detail))), None, provenance);

 public Option<PropertyValue> Result(PropertyName name) => Results.Find(name);

 // A changed input content key marks the cached result stale without deleting it — the next Bake
 // surfaces a Stale assessment a consumer re-requests through the Compute route.
 public bool IsStaleFor(UInt128 currentInputKey) => InputKey != currentInputKey;

 public AssessmentPayload AsStale() => this with { Outcome = AssessmentOutcome.Stale };
}
```

## [03]-[RESEARCH]

- [GENERIC_ASSESSMENT]: the seam `AssessmentPayload` is discipline-agnostic by design — the `(Discipline, Route, InputKey)` triple keys ANY analysis outcome, the typed `Results` bag carries the flat outputs, and the `ResultBlob` references the heavy artifact, so the structural FEA (VividOrange/BriefFiniteElement/FEALiTE2D), the thermal closed-form (ISO 6946 U / EN 13788 Glaser), the energy simulation (NREL.OpenStudio model + EnergyPlus subprocess), the acoustic (ISO 12354), the fire (EN 199x-1-2), and the LCA (EN 15978 + EC3 REST) routes all write the same node shape — the discipline-specific input/result shapes and the runners living in `Rasm.Compute`, the seam owning only the receipt; the assessment is a generic opaque typed payload, not a per-discipline node family.
- [CONTENT_KEYED_CACHE]: the `(InputKey, Route)` cache key is derived through the kernel `XxHash128` over the assessed inputs' content (`Projection/address#CONTENT_ADDRESS`), so the `Rasm.Compute` route resolves an existing `Computed` assessment rather than re-solving when inputs are unchanged, and `IsStaleFor` marks a result `Stale` when a downstream edit changes the input key — the assessment cache is the same content-addressed discipline the geometry blob store and the snapshot spine use, never a wall-clock or a version-number cache; the assessment attaches through an `Assign` edge so the graph carries the element→assessment binding, the Bim projector mapping it from the IFC objectified-assessment relation at egress.
