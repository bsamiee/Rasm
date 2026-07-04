# [PY_GEOMETRY_IFC_ANALYSIS]

IFC property/quantity/relationship analysis and standards-conformant validation — the AEC verbs the tessellation hop alone drops. `IfcAnalysis` runs quantity takeoff, Pset queries, IDS model-checking, clash detection, space-program validation, and BCF issue authoring over `ifcopenshell.util`, `ifctester`, `ifcclash`, and the `bcf` library, folding every verb's provider output into one `AnalysisRow` `@tagged_union` row algebra whose `of_*` smart constructors mint the typed quantity/pset/compliance/clash/topic case and whose one total `facts` `match` projects each case to its addressable `dict[str, object]` — never a stringified `dict[str, str]` bag that erases floats and bools. The cross-cutting concerns compose through the canonical runtime owners rather than inline re-wiring that would trample them. `run` owns one `content.analysis` OTel span the `boundary` fence runs INSIDE — the identity/graduation/structural span-owning shape — so `_convert` (`reliability/faults#FAULT`) records the caught provider exception on a LIVE recording span and sets ERROR, and `_ok` sets the clean-exit OK status; a bare span-less `boundary` would no-op the `is_recording()` egress and the fault would never reach the trace. This owner mints the one `content.analysis` `Tracer` exactly as the structural sibling mints `content.section`, but re-wires no `structlog` chain — the trace-egress weave behind the fence is the faults owner's, the receipt-egress chain the receipts owner's. `@beartype(conf=FAULT_CONF)` on `_dispatch` binds the shared `BeartypeConf(violation_type=BeartypeCallHintViolation)` so a contract violation raises the canonical root the rail folds; and `@receipted(_REDACTION)` on `_emit` is the `observability/receipts#RECEIPT` egress aspect harvesting the `ReceiptContributor` stream onto `Signals.emit` on the cleared `Ok`, never an inline emit threaded through `run`. Resilience splits by boundary class: the OCC clash tree (`Clasher.clash()` over `ifcopenshell.geom.tree`) is this owner's transient-native-failure boundary the in-page `_CLASH_RETRY` `stamina.retry` policy wraps where the provider is composed — the same in-page weave the structural sibling runs around its `cytriangle` mesh-and-solve, never a deferred downstream concern that loses the retry the OCC kernel demands — while the durable `.bcfzip`/IDS-report archive WRITE stays the `python:data/spatial` boundary's columnar product, never a hand-rolled `sleep` loop and never a throwaway temp-dir write the run discards. The receipt graduates through `GraduationReceipt.graduates` across the compute `HandoffAxis` geometry case under the one residual-ceiling admission keyed by the kind-specific evidence ledger. The BCF verb is the composition apex — it re-runs the clash leg and stacks `ifcclash` overlaps into `bcf` topics with viewpoints, rather than a same-string round-trip. The selecting verbs never touch a raw query string: the free-form selector is validated once at admission through `IfcSelector.filter`/`IfcSelector.parse` from `geometry:ifc/selector.md#SELECTOR`, so a malformed selector is a typed `Error(BoundaryFault)` on the rail at the boundary rather than a silent empty `filter_elements` match three arms deep, and the validated `SelectorQuery` re-serializes to the exact grammar `ifcopenshell` consumes. The C# `IfcSemanticModel` projects the spatial hierarchy in-process; this owner adds the analysis verbs and the buildingSMART validation output the managed projection does not produce.

## [01]-[INDEX]

- [01]-[ANALYSIS]: the quantity, Pset, IDS, clash, space-program, and BCF analysis verbs under one `AnalysisKind`-discriminated owner folding into one `AnalysisRow` `@tagged_union` row algebra with `of_*` smart constructors and a self-projecting `facts` fold, every selecting arm fed by the `IfcSelector` validated gate, woven through one self-owned `content.analysis` span with the `boundary`/`@beartype(conf=FAULT_CONF)`/`stamina.retry`/`@receipted` fault-contract-resilience-receipt rails, the OCC clash tree the in-page transient-native-failure boundary, the run graduating on kind-specific evidence.

## [02]-[ANALYSIS]

- Owner: `IfcAnalysis` — the `@staticmethod` boundary capsule dispatching the analysis verbs over the IfcOpenShell ecosystem through a thin `_dispatch` rail-fold over per-verb helpers, mirroring the sibling `IfcLifecycle`/`IfcStructural`; `AnalysisKind` the closed `StrEnum` selecting the verb; `AnalysisRow` the `@tagged_union` of per-verb result rows whose `of_*` smart constructors mint the case and whose one `facts` total `match` projects it — `quantity` a GlobalId plus a `dict[str, float]` quantity map, `pset` a GlobalId plus a `dict[str, object]` property map, `compliance` a subject plus a `float` passing ratio and an `int` failing-check count shared by the IDS and space-program verdicts, `clash` an a/b GlobalId pair plus a `float` penetration distance and a cluster index, `topic` a guid plus title and an authored-from finding kind — so the six verbs share one row carrier whose case IS the shape, never six parallel `dict[str, str]` bags that stringify floats and bools; `AnalysisResult` the typed receipt carrying the kind, the subject element set, and the `tuple[AnalysisRow, ...]` rows, implementing the runtime `ReceiptContributor` Protocol (`contribute -> Iterable[Receipt]`) and owning the kind-specific `evidence` ledger the graduation leg folds, the `ANALYSIS_SUBJECT` `numerical-primitive` literal carried by the module constant rather than a per-receipt `subject: str` field racing the discriminant.
- Cases: `AnalysisKind` rows `QUANTITY` (quantity takeoff over `util.element`) · `PSET` (property-set queries) · `IDS` (model-checking over `ifctester.ids`) · `CLASH` (clash sets over `ifcclash`) · `SPACE_PROGRAM` (`IfcSpace` area validation against a program table) · `BCF` (issue authoring over the `bcf` library that consumes the clash findings as topics) — matched by `match`/`assert_never`, each dispatching to the ecosystem tool that owns it and folding its provider output into the shared `AnalysisRow` case the verb's evidence reads from, never a per-verb row dialect. A new verb is one `AnalysisKind` row, one `AnalysisRow.of_*` constructor, one dispatch arm, and one `evidence` key, breaking every dispatch site at type-check time under `ty`.
- Selector gate: the `query` meaning is fixed by the kind through the `QUERY_SPLIT` data table — `QUANTITY`/`PSET` are pure selectors, `CLASH`/`BCF` carry an `a#b` side pair, `IDS`/`SPACE_PROGRAM` carry a spec path or JSON table — so the spec parse is one table-keyed split rather than a `partition`-per-arm string ladder. The selecting verbs never thread a raw query into `filter_elements`: each opens with `IfcSelector.filter(model, query)` — `IfcSelector.parse(query).map(filter_elements)` — so the `lark` grammar admits the closed selection vocabulary once and lifts an `UnexpectedInput` parse failure into the rail at the boundary, and the validated `SelectorQuery.filter_string` re-serializes the selection to the `ifcopenshell` grammar; the selector is the only selection engine, never a parallel filter. The `CLASH`/`BCF` arms derive their two `ClashSource.selector` strings the same validated way through `_clash_sides`, which folds the `a#b` side pair through the one polymorphic `IfcSelector.parse(Iterable[str])` batch the selector page exposes for exactly this caller — both sides validate under one `traversed(..., by=Disposition.ABORT)` rail short-circuiting on the first malformed member, never a hand-rolled `parse(a).bind(parse(b))` per-arm loop — defaulting both sides to the `'a'`-all mode for a whole-model intersection when the `query` is empty.
- Entry: `IfcAnalysis.run` is the one `@beartype`-fenced entry taking an `ifcopenshell.file`, an `AnalysisKind`, and a `query`, returning a `RuntimeRail[AnalysisResult]` through one self-owned `content.analysis` span the `boundary(f"ifc.{kind}", ...)` fence runs INSIDE — the `runtime:evidence/identity.md#IDENTITY` `content.derive`, `compute:graduation/handoff.md#GRADUATION` `content.graduate`, and `geometry:ifc/structural.md#STRUCTURAL` `content.section` span-owning shape — so the `_convert` weave records a provider exception on the LIVE recording span and sets ERROR rather than no-opping a span-less `is_recording()` gate. The boundary flattens the nested rail through `.bind(identity)` (the `expression`-exported `identity`, the canonical FP rail-flatten the structural/authoring siblings hold, never a `lambda rail: rail` re-spell) so a provider exception converts to a `BoundaryFault` exactly once at the seam while a selector parse fault arrives already typed on the rail, the two fault sources meeting on one carrier, then `.map(IfcAnalysis._emit)` threads the cleared `Ok` through the `@receipted` egress step and `.map(_ok)` sets `Status(StatusCode.OK)` once on the clean exit the conversion never re-annotates — span-fence-emit-ok the one weave, exactly the structural sibling's `run`. The dispatch is a rail-returning fold: each arm yields `RuntimeRail[AnalysisResult]`, the selecting arms by `IfcSelector.filter(...).map(...)`, the non-selecting arms lifted through `Ok`, so the whole verb composes monadically rather than a bare value wrapped once. The `@receipted(_REDACTION)` aspect on `_emit` harvests the `AnalysisResult.contribute` stream and emits on the `Ok` exit, so `run` threads no `Signals.emit` call through its body, and graduation stays the caller's own step on the returned receipt. Each arm derives its own `subjects` from the verb's true subject set: validated `filter_elements` GlobalIds for the selecting arms, `IfcSpace` GlobalIds for `SPACE_PROGRAM`, spec names for `IDS`, clash a-side GlobalIds for `CLASH`, authored topic guids for `BCF` — so the subject field never carries a meaningless selector run for a non-selecting verb.
- Auto: quantity takeoff folds `util.element.get_psets(element, qtos_only=True)` over the validated selected set into one merged `quantity` map per element, keeping the numeric quantities as floats; pset queries fold the full `get_psets` map into the `pset` rows preserving native value types. IDS validation runs an `Ids` specification against the model, then reads the RICH per-spec verdict the catalogue confirms: each `Specification` carries the tri-state `status` (`True`/`False`/`None`-skipped) plus the `passed_entities`/`failed_entities` entity sets, so `_validate` folds a `compliance` row whose passing ratio is `len(passed) / max(len(passed) + len(failed), 1)` — a real per-element fraction, not the `1.0`/`0.0` binary the boolean alone yields — and whose failing-check count is `len(failed)`, EXCLUDING the `status is None` not-applicable/skipped spec from the rows entirely (the `if spec.status is not None` guard) rather than folding it as a `0.0`-ratio row that would poison the `evidence` mean and fabricate a failure the model never incurred; the per-element evidence the boolean verdict drops is the value the `failed_entities`/`passed_entities` sets carry. Clash detection runs the `ifcclash` clash-set query over the validated `ClashSource.selector` sides at one configurable `ClashSet` mode/tolerance, then `smart_group_clashes` clusters the overlaps so each `clash` row carries its `ClashResult` GlobalId pair, penetration `distance`, and spatial-cluster index in one pass. Space-program validation decodes the `query` program table, scales each `IfcSpace` net floor area to project units through `util.unit.calculate_unit_scale` before the comparison, and folds a `compliance` row whose ratio is `actual/target` per PROGRAMMED space only — a space absent from the table (`target` 0.0) is excluded under the `program.get(key) > 0.0` guard rather than admitted as a `0.0`-ratio row that would poison the shared IDS/space-program `evidence` mean the same way a skipped IDS spec would. The BCF arm re-runs the clash leg once and composes each overlap INTO a topic through the `_topic` per-collision projection — `BcfXml.create_new` opens the project, `add_topic(..., topic_type="Clash")` mints one topic per clash, and `add_viewpoint_from_point_and_guids` binds each topic to the `ClashResult.p1` overlap point and the offending `a_global_id`/`b_global_id` GUIDs, so the topic rows are the issue-authoring leg's wire carry that stacks `ifcclash` + `bcf` rather than a same-string round-trip, the durable `.bcfzip` `BcfXml.save` flush deferred to the `python:data/spatial` boundary as the costing `_cost` arm defers its spreadsheet write. Every native-provider leg (`_run_clash`, `_validate`, `_author`) runs inside the one `boundary` fence the `_convert` weave records on the live `content.analysis` span and classifies onto the rail, so no leg re-states telemetry or fault-logging inline; the OCC clash tree is the one transient-native-failure boundary the in-page `_CLASH_RETRY` `stamina.retry` policy wraps around `_run_clash`'s `clash()`/`smart_group_clashes` pass exactly as the structural sibling wraps its `cytriangle` solve, the bounded backoff composed where the provider is invoked rather than a deferred downstream concern that drops the retry the OCC kernel demands.
- Receipt: `AnalysisResult.contribute` yields the `ReceiptContributor` stream — one `Receipt.of("rasm.geometry.ifc.analysis", ("emitted", subject, facts))` row, the 2-arg owner-then-`(Phase, subject, facts)` shape the runtime `Receipt` factory mints `fact` from, never the legacy 4-positional form — whose facts are the per-row `AnalysisRow.facts` projection joined with the subject count and the resolved `evidence` ledger as native scalars the `EventDict` `enc_hook=repr` renderer serializes without a `str()` coerce. The run graduates through `AnalysisResult.graduates(key, ceiling)` over the kind-specific `evidence` residual ledger and the canonical `GeometrySubject` `numerical-primitive` literal — the IFC-analysis evidence the C# owner system consumes, the same literal the `geometry:ifc/structural.md#STRUCTURAL` section-integral and `geometry:ifc/costing.md#LIFECYCLE` owners cross on — folded through the one `GraduationReceipt.graduates(source_package, HandoffAxis(geometry=subject), evidence_key, measured, ceiling)` admission (`python:compute/graduation/handoff.md#GRADUATION`), never a bare `str`, so an unlisted literal fails at the type boundary. The residual ledger is the kind-specific `evidence` fold rather than a meaningless row count: the IDS/space-program verdicts key the failing fraction (`1 - mean(passing-ratio)`), CLASH keys the unresolved-cluster count, and the takeoff/BCF verbs key the empty-result fraction, so a model whose compliance falls below its ceiling is an `Error(BoundaryFault)` on the rail rather than a graduated handoff.
- Packages: `ifcopenshell` (`util.element.get_psets`/`util.unit.calculate_unit_scale`), `ifctester` (`ids.open`/`Ids.validate`/`Ids.specifications`/`Specification.name`/`Specification.status`/`Specification.passed_entities`/`Specification.failed_entities`), `ifcclash` (`ifcclash.Clasher`/`ifcclash.ClashSettings`/the `ClashSet`/`ClashSource` TypedDicts the `clash_set`/`source()` build typed against/`Clasher.clash`/`Clasher.smart_group_clashes`/`ClashResult.a_global_id`/`b_global_id`/`p1`/`distance` read back as the local `ClashRow`), `bcf-client` (import `bcf`; `bcf.v3.bcfxml.BcfXml.create_new`/`add_topic`/`get_topics`/`TopicHandler.add_viewpoint_from_point_and_guids`/`TopicHandler.guid`/`TopicHandler.topic.title`), `geometry:ifc/selector.md#SELECTOR` (`IfcSelector.filter` for the single-selector arms, `IfcSelector.parse(Iterable[str])` the polymorphic batch the `CLASH`/`BCF` side pair folds through — the validated selection engine, the only `filter_elements` caller), `python:compute/graduation/handoff.md#GRADUATION` (`GeometrySubject`/`HandoffAxis`/`GraduationReceipt`), `beartype` (`@beartype` on `run`, `@beartype(conf=FAULT_CONF)` on `_dispatch`), `expression` (`tagged_union`/`tag`/`case`, `Ok`, `Result.map`/`bind`, `Map.of_seq` the `QUERY_SPLIT` table, `Map.empty` the keep-all redaction table), `stamina` (`retry` the in-page `_CLASH_RETRY` policy wrapping the OCC `clash()` mesh-and-solve as the one transient-native-failure boundary, the same in-page weave the structural sibling runs around `cytriangle`), `opentelemetry-api` (`trace.get_tracer`/`start_as_current_span`/`Span.is_recording`/`Span.set_attributes`/`Span.set_status`/`Status`/`StatusCode` for the one self-owned `content.analysis` span), runtime (`RuntimeRail`/`boundary`/`FAULT_CONF` the fault fence whose `_convert` records on the live span and classifies onto the rail, `ContentKey`, `Receipt.of(owner, evidence)`/`ReceiptContributor`/`Redaction`/`@receipted` the receipt egress aspect); the `reliability/faults#FAULT` `_convert` owns the trace-egress weave behind the fence and the receipt-egress chain is the receipts owner's, so this owner mints the one `content.analysis` tracer like the structural sibling but re-wires no `structlog` chain, and the durable archive WRITE — not the OCC retry — is the `python:data/spatial` boundary's.
- Growth: a new analysis verb is one `AnalysisKind` row, one `AnalysisRow.of_*` constructor plus one `facts` arm, one dispatch arm, one `QUERY_SPLIT` row, and one `evidence` key; a new selection axis is one `IfcSelector` grammar alternative, never a local query-parse fold here; a new IDS specification is authored through `ifctester`, never a local rule fold; a stricter compliance bar is one tighter ceiling row the caller supplies; zero new surface.
- Boundary: no re-derivation of the C# `IfcSemanticModel` spatial hierarchy (projected in-process); no durable store; no Rhino/GH mutation; no raw `query` string threaded past admission into `filter_elements` (the deleted stringly-typed passthrough — the selecting arms enter through `IfcSelector` and the raw query never reaches `ifcopenshell.util.selector` unvalidated). A hand-rolled IFC parser, a second selection engine where `IfcSelector` re-serializes the validated query to the `ifcopenshell` grammar, a bespoke non-portable IDS rule fold, a binary `1.0`/`0.0` IDS verdict where the `passed_entities`/`failed_entities` sets carry the real per-element passing fraction, an OCCT bounding-overlap clash reimplementation, a stringified `dict[str, str]` row that erases floats and bools where `AnalysisRow` discriminates the typed payload, a same-string BCF round-trip where the arm composes clash findings into topics, a raw-unit space comparison where `util.unit.calculate_unit_scale` normalizes, an erased `dict[str, object]` clash bag blind-subscripted where the catalog-confirmed `ClashSet`/`ClashSource` TypedDicts and the local `ClashRow` carry the typed mode literals and the `a_global_id`/`distance`/`p1` keys, a hand-rolled `parse(a).bind(parse(b))` per-arm side loop where `IfcSelector.parse(Iterable[str])` validates both clash sides under one batch rail, a span-less `boundary` whose `_convert` no-ops the `is_recording()` trace-egress where the self-owned `content.analysis` span the fence runs inside makes the fault visible on the live span, a post-hoc `match rail: case Ok(_): span.set_status(...)` re-annotation outside the span weave where the `_ok` `.map` close-out sets the OK status on the clean exit and the `boundary` `_convert` already owns the ERROR status on the live span, a minted `structlog` chain or inline `Signals.emit` where the `boundary` fence owns the trace-egress weave and `@receipted` owns receipt egress, a `Struct(frozen=True)` `AnalysisResult` without `gc=False` where the per-run frozen receipt opts out of the cyclic GC set like the sibling `SectionReceipt`/`SelectorQuery`, a hand-coded `sleep`/retry loop around the OCC `clash()` solve OR a retry deferred to a downstream lane where the in-page `_CLASH_RETRY` `stamina.retry` owns the transient-native boundary's schedule exactly as the structural sibling wraps `cytriangle`, a `ClashSource` literal omitting the required `file` key where the typed source carries `file=""` plus the pre-loaded `ifc` model, an unflushed `BcfXml` document silently dropped where the topic rows are the wire carry and the durable `.bcfzip` write is the `python:data/spatial` boundary's, the legacy 4-positional `Receipt.of` and the single-`Receipt` `contribute` where the runtime port is `Receipt.of(owner, evidence)` and `contribute -> Iterable[Receipt]`, an inlined residual-vs-ceiling comparison where `GraduationReceipt.graduates` owns the admission, and a parallel per-verb class family are the deleted forms — the selector grammar, IDS, clash, BCF, the unit scale, the in-page OCC retry, the fault/contract/receipt aspects, and the graduation rail compose the provider tools end-to-end.

```python signature
from collections.abc import Callable, Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, NotRequired, TypedDict, assert_never

import stamina
from beartype import beartype
from expression import Ok, case, identity, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from msgspec.json import decode
from opentelemetry import trace
from opentelemetry.trace import Span, Status, StatusCode

from rasm.compute.graduation.handoff import GeometrySubject, GraduationReceipt, HandoffAxis
from rasm.geometry.ifc.selector import IfcSelector
from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt, ReceiptContributor, Redaction, receipted

if TYPE_CHECKING:  # worker: every runtime ifcopenshell use is the function-local `import ifcopenshell.<sub>  # noqa: PLC0415` that binds `ifcopenshell` in scope, so the runtime module loads clean and the boundary-scope import policy holds (the selector sibling shape)
    import ifcopenshell

# --- [TYPES] ---------------------------------------------------------------------------


class AnalysisKind(StrEnum):
    QUANTITY = "quantity"
    PSET = "pset"
    IDS = "ids"
    CLASH = "clash"
    SPACE_PROGRAM = "space-program"
    BCF = "bcf"


class ClashRow(TypedDict):
    # The catalog `ifcclash.ClashResult` shape plus the derived spatial-cluster index the grouper
    # writes — the typed payload the clash leg reads field-by-field, never a `dict[str, object]` bag
    # blind-subscripted three arms deep where the GUID pair and the float distance are addressable keys.
    a_global_id: str
    b_global_id: str
    distance: float
    p1: list[float]
    group: NotRequired[str]
    cluster: NotRequired[int]


@tagged_union(frozen=True)
class AnalysisRow:
    tag: Literal["quantity", "pset", "compliance", "clash", "topic"] = tag()
    quantity: tuple[str, dict[str, float]] = case()
    pset: tuple[str, dict[str, object]] = case()
    compliance: tuple[str, float, int] = case()  # subject, passing-ratio, failing-check count
    clash: tuple[str, str, float, int] = case()  # a-guid, b-guid, penetration, cluster index
    topic: tuple[str, str, str] = case()  # guid, title, authored-from finding kind

    @staticmethod
    def of_quantity(element: str, quantities: dict[str, float]) -> "AnalysisRow":
        return AnalysisRow(quantity=(element, quantities))

    @staticmethod
    def of_pset(element: str, properties: dict[str, object]) -> "AnalysisRow":
        return AnalysisRow(pset=(element, properties))

    @staticmethod
    def of_compliance(subject: str, passing: float, failing: int) -> "AnalysisRow":
        return AnalysisRow(compliance=(subject, passing, failing))

    @staticmethod
    def of_clash(a: str, b: str, penetration: float, cluster: int) -> "AnalysisRow":
        return AnalysisRow(clash=(a, b, penetration, cluster))

    @staticmethod
    def of_topic(guid: str, title: str, finding: str) -> "AnalysisRow":
        return AnalysisRow(topic=(guid, title, finding))

    @property
    def facts(self) -> dict[str, object]:
        # One total match projecting each case to its addressable fact map; native floats/maps
        # ride the runtime EventDict whose enc_hook=repr renderer serializes them with no str() coerce.
        match self:
            case AnalysisRow(tag="quantity", quantity=(element, quantities)):
                return {"element": element, "quantities": quantities}
            case AnalysisRow(tag="pset", pset=(element, properties)):
                return {"element": element, "properties": properties}
            case AnalysisRow(tag="compliance", compliance=(subject, passing, failing)):
                return {"subject": subject, "passing": passing, "failing": failing}
            case AnalysisRow(tag="clash", clash=(a, b, penetration, cluster)):
                return {"a": a, "b": b, "penetration": penetration, "cluster": cluster}
            case AnalysisRow(tag="topic", topic=(guid, title, finding)):
                return {"topic": guid, "title": title, "finding": finding}
            case unreachable:
                assert_never(unreachable)


# --- [CONSTANTS] -----------------------------------------------------------------------

# The IFC-analysis evidence crosses the geometry graduation case as a numerical primitive,
# the same GeometrySubject literal the section-integral and lifecycle owners cross on; typed as
# the imported union so an unlisted literal fails at the boundary under `ty`.
ANALYSIS_SUBJECT: Final[GeometrySubject] = "numerical-primitive"

# Keep-all egress policy mirroring the structural/lifecycle owners: analysis facts carry no
# secret field, so the @receipted aspect scrubs nothing.
_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())
_TRACER: Final[trace.Tracer] = trace.get_tracer("geometry.ifc.analysis")

# The OCC clash tree (`Clasher.clash()` over `ifcopenshell.geom.tree`) is this owner's one
# transient-native-failure boundary, the same class as the structural sibling's `cytriangle`
# mesh-and-solve: stamina owns the bounded exponential backoff in-page where the provider is
# composed, never a hand-coded sleep loop and never a deferred downstream concern. RuntimeError
# is the OCC family the native kernel raises on a transient tree/intersection failure.
_CLASH_RETRY: Final[Callable[..., object]] = stamina.retry(on=RuntimeError, attempts=3, wait_initial=0.05, wait_max=1.0)

# The query's meaning is fixed by the kind: the side-pair verbs split on `#`, the selecting
# verbs take the whole query as the selector, the spec-bearing verbs take the whole query as a
# path/table. The split is one immutable table key mirroring the costing `PHASE_DELIMITER`,
# never a partition-per-arm string ladder and never a `.get` default that drops a kind.
QUERY_SPLIT: Final[Map[AnalysisKind, str]] = Map.of_seq([
    (AnalysisKind.QUANTITY, ""),
    (AnalysisKind.PSET, ""),
    (AnalysisKind.IDS, ""),
    (AnalysisKind.SPACE_PROGRAM, ""),
    (AnalysisKind.CLASH, "#"),
    (AnalysisKind.BCF, "#"),
])

# --- [MODELS] --------------------------------------------------------------------------


class AnalysisResult(Struct, frozen=True, gc=False):
    kind: AnalysisKind
    subjects: tuple[str, ...]
    rows: tuple[AnalysisRow, ...]

    def evidence(self) -> dict[str, float]:
        # The residual ledger is kind-specific, never a row count: the verdict verbs key the
        # mean failing fraction off the real per-spec/space passing ratio, CLASH the unresolved
        # cluster count, and the takeoff/BCF verbs the empty-result fraction.
        match self.kind:
            case AnalysisKind.IDS | AnalysisKind.SPACE_PROGRAM:
                # Only the applicable/programmed compliance rows reach here (the `_validate`/`_space`
                # guards drop the not-applicable ones), so an empty `ratios` is "no compliance signal"
                # keyed `0.0` — never the `1.0 - 0/1 = 1.0` total-non-compliance the empty mean yields,
                # which would reject an all-not-applicable model the program never constrained.
                ratios = tuple(r.compliance[1] for r in self.rows if r.tag == "compliance")
                return {"non-compliant": 1.0 - sum(ratios) / len(ratios) if ratios else 0.0}
            case AnalysisKind.CLASH:
                clusters = {r.clash[3] for r in self.rows if r.tag == "clash"}
                return {"clash-clusters": float(len(clusters))}
            case AnalysisKind.QUANTITY | AnalysisKind.PSET | AnalysisKind.BCF:
                return {"empty": 0.0 if self.rows else 1.0}
            case unreachable:
                assert_never(unreachable)

    def contribute(self) -> Iterable[Receipt]:
        facts: dict[str, object] = {f"{self.kind}.{i}.{k}": v for i, r in enumerate(self.rows) for k, v in r.facts.items()}
        yield Receipt.of("rasm.geometry.ifc.analysis", ("emitted", self.kind.value, facts | {"subjects": len(self.subjects)} | self.evidence()))

    def graduates(self, evidence_key: ContentKey, ceiling: dict[str, float]) -> "RuntimeRail[GraduationReceipt]":
        return GraduationReceipt.graduates(
            "rasm.geometry.ifc.analysis", HandoffAxis(geometry=ANALYSIS_SUBJECT), evidence_key, self.evidence(), ceiling
        )


# --- [OPERATIONS] ----------------------------------------------------------------------


class IfcAnalysis:
    @staticmethod
    @beartype
    def run(model: "ifcopenshell.file", kind: AnalysisKind, query: str) -> "RuntimeRail[AnalysisResult]":
        # One woven rail INSIDE one self-owned `content.analysis` span — the identity/graduation/structural
        # span-owning shape: the `boundary` fence runs the rail-returning `_dispatch` ON the live recording
        # span so the runtime `_convert` records a provider exception on it and sets ERROR (a bare
        # span-less `boundary` would no-op the `is_recording()` egress and the fault would never reach the
        # trace), the identity `.bind` flattens the railed `_dispatch` whose own `@beartype(conf=FAULT_CONF)`
        # lifts a contract violation onto the rail, the `@receipted` `_emit` harvests the contributor
        # stream onto `Signals.emit` on the cleared `Ok` only, and the `_ok` `.map` close-out sets
        # `Status(StatusCode.OK)` once on the clean exit the conversion never re-annotates. Graduation
        # stays the caller's own step on the returned receipt, mirroring `IfcStructural.run`.
        with _TRACER.start_as_current_span("content.analysis") as span:
            if span.is_recording():
                span.set_attributes({"kind": kind.value, "subject": ANALYSIS_SUBJECT})
            return (
                boundary(f"ifc.{kind}", lambda: IfcAnalysis._dispatch(model, kind, query))
                .bind(identity)
                .map(IfcAnalysis._emit)
                .map(lambda result: IfcAnalysis._ok(span, result))
            )

    @staticmethod
    def _ok(span: "Span", result: AnalysisResult) -> AnalysisResult:
        # the clean-exit close-out: the measured operation owns the OK status on its own span, the same
        # status the identity `content.derive`, graduation `content.graduate`, and structural
        # `content.section` spans set on success; the ERROR side stays the fence `_convert`'s.
        span.set_status(Status(StatusCode.OK))
        return result

    @staticmethod
    @receipted(_REDACTION)
    def _emit(result: AnalysisResult) -> AnalysisResult:
        return result

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def _dispatch(model: "ifcopenshell.file", kind: AnalysisKind, query: str) -> "RuntimeRail[AnalysisResult]":
        match kind:
            case AnalysisKind.QUANTITY | AnalysisKind.PSET:
                quantities = kind is AnalysisKind.QUANTITY
                return IfcSelector.filter(model, query).map(
                    lambda elements: AnalysisResult(
                        kind, tuple(e.GlobalId for e in elements), tuple(IfcAnalysis._takeoff(e, quantities) for e in elements)
                    )
                )
            case AnalysisKind.SPACE_PROGRAM:
                return Ok(IfcAnalysis._space(model, query))
            case AnalysisKind.IDS:
                return Ok(IfcAnalysis._validate(model, query))
            case AnalysisKind.CLASH:
                return IfcAnalysis._clash_sides(query).map(lambda sides: IfcAnalysis._clash(model, sides))
            case AnalysisKind.BCF:
                return IfcAnalysis._clash_sides(query).map(lambda sides: IfcAnalysis._author(model, sides))
            case unreachable:
                assert_never(unreachable)

    @staticmethod
    def _space(model: "ifcopenshell.file", query: str) -> AnalysisResult:
        # The program-table decode runs under the outer `boundary` fence (mirroring the costing sibling's
        # `_patch` `decode`), so a malformed JSON table is a `BoundaryFault` the `_convert` weave classifies
        # at the seam rather than an inline try/except control branch in domain logic.
        program = decode(query.encode(), type=dict[str, float])
        scale = IfcAnalysis._unit_scale(model)
        spaces = model.by_type("IfcSpace")
        # A space whose name is absent from the program table (`target` defaulting to 0.0) carries no
        # program signal and is EXCLUDED from the compliance rows — admitting a `0.0`-ratio row for it
        # would poison the shared IDS/space-program `evidence` mean exactly as a skipped IDS spec would,
        # reading an out-of-program space as total non-compliance. Only a programmed space (`target > 0`)
        # folds a row whose ratio is `actual/target`.
        graded = tuple(
            (s, program[key], IfcAnalysis._net_area(s) * scale)
            for s in spaces
            for key in (s.LongName or s.Name or "",)
            if program.get(key, 0.0) > 0.0
        )
        rows = tuple(AnalysisRow.of_compliance(s.GlobalId, area / target, 0 if area >= target else 1) for s, target, area in graded)
        return AnalysisResult(AnalysisKind.SPACE_PROGRAM, tuple(s.GlobalId for s, _, _ in graded), rows)

    @staticmethod
    def _takeoff(element: "ifcopenshell.entity_instance", quantities: bool) -> AnalysisRow:
        import ifcopenshell.util.element  # noqa: PLC0415

        psets = ifcopenshell.util.element.get_psets(element, qtos_only=quantities)
        merged = {f"{name}.{key}": value for name, body in psets.items() for key, value in body.items()}
        if quantities:
            return AnalysisRow.of_quantity(element.GlobalId, {k: float(v) for k, v in merged.items() if isinstance(v, int | float)})
        return AnalysisRow.of_pset(element.GlobalId, merged)

    @staticmethod
    def _validate(model: "ifcopenshell.file", spec_path: str) -> AnalysisResult:
        import ifctester.ids  # noqa: PLC0415

        document = ifctester.ids.open(spec_path)
        document.validate(model)
        # The rich verdict the binary `status` drops: the per-spec passed/failed entity sets give a
        # real per-element passing fraction. The tri-state `status is None` (not-applicable / skipped)
        # is EXCLUDED from the compliance rows rather than folded as a `0.0`-ratio row — a skip carries
        # no compliance signal, so admitting it would poison the `evidence` mean (an all-skip model
        # would read as 100% non-compliant) and fabricate a failure the model never incurred. Only an
        # applicable spec (a real `True`/`False` verdict with a non-empty applicable set) folds a row.
        rows = tuple(
            AnalysisRow.of_compliance(
                spec.name, len(spec.passed_entities) / max(len(spec.passed_entities) + len(spec.failed_entities), 1), len(spec.failed_entities)
            )
            for spec in document.specifications
            if spec.status is not None
        )
        applicable = tuple(spec.name for spec in document.specifications if spec.status is not None)
        return AnalysisResult(AnalysisKind.IDS, applicable, rows)

    @staticmethod
    def _clash_sides(query: str) -> "RuntimeRail[tuple[str, str]]":
        # The two side selectors validate through the ONE polymorphic batch parse the selector page
        # exposes for exactly this caller — `IfcSelector.parse(Iterable[str])` folds both sides under one
        # `traversed(..., by=Disposition.ABORT)` rail short-circuiting on the first malformed member,
        # never a hand-rolled `parse(a).bind(parse(b))` per-arm loop. An empty `query` is the whole-model
        # default: both sides drop to the `'a'`-all mode, so no selector parses and no raw string leaks.
        if not query:
            return Ok(("", ""))
        a_query, _, b_query = query.partition(QUERY_SPLIT[AnalysisKind.CLASH])
        return IfcSelector.parse((a_query, b_query or a_query)).map(lambda sides: (sides[0].filter_string, sides[1].filter_string))

    @staticmethod
    def _clash(model: "ifcopenshell.file", sides: tuple[str, str]) -> AnalysisResult:
        rows = tuple(
            AnalysisRow.of_clash(c["a_global_id"], c["b_global_id"], c["distance"], c.get("cluster", 0)) for c in IfcAnalysis._run_clash(model, sides)
        )
        return AnalysisResult(AnalysisKind.CLASH, tuple(r.clash[0] for r in rows), rows)

    @staticmethod
    def _author(model: "ifcopenshell.file", sides: tuple[str, str]) -> AnalysisResult:
        from bcf.v3.bcfxml import BcfXml  # noqa: PLC0415

        # The authored topic rows ARE this owner's wire carry — the verifiable issue-authoring
        # output the receipt graduates on. The durable `.bcfzip` `BcfXml.save` flush is the
        # `python:data/spatial` boundary's columnar product (mirroring the costing `_cost` arm
        # deferring the `ifc5Dspreadsheet` write), so this leg authors the in-memory archive and
        # reads its topic GUIDs back rather than discarding an unflushed document or writing a
        # throwaway temp-dir zip the run drops.
        document = BcfXml.create_new("rasm.ifc.analysis")
        rows = tuple(IfcAnalysis._topic(document, c) for c in IfcAnalysis._run_clash(model, sides))
        return AnalysisResult(AnalysisKind.BCF, tuple(r.topic[0] for r in rows), rows)

    @staticmethod
    def _topic(document: "bcf.v3.bcfxml.BcfXml", collision: ClashRow) -> AnalysisRow:
        # One overlap -> one topic bound to the ClashResult.p1 point and the offending GUID pair,
        # the issue-authoring leg stacking ifcclash + bcf rather than a same-string round-trip.
        handler = document.add_topic(
            f"Clash {collision['a_global_id']} × {collision['b_global_id']}", f"penetration {collision['distance']:.4f}", "rasm", topic_type="Clash"
        )
        handler.add_viewpoint_from_point_and_guids(collision["p1"], collision["a_global_id"], collision["b_global_id"])
        return AnalysisRow.of_topic(handler.guid, handler.topic.title, "clash")

    @staticmethod
    def _run_clash(model: "ifcopenshell.file", sides: tuple[str, str]) -> tuple[ClashRow, ...]:
        from ifcclash.ifcclash import Clasher, ClashSettings, ClashSet, ClashSource  # noqa: PLC0415

        def source(selector: str) -> ClashSource:
            # `file` is a required `ClashSource` key; the pre-loaded `ifc` model makes the empty
            # path inert (the `load_ifc` leg the catalog says `ClashSource.ifc` skips), so a typed
            # source carries both rather than omitting the required field. The `'e'` IfcClass-
            # expression mode consumes the `IfcSelector`-validated `filter_string`, the `'a'`-all
            # mode the empty whole-model default — never a raw caller query.
            base: ClashSource = {"file": "", "ifc": model}
            return {**base, "selector": selector, "mode": "e"} if selector else {**base, "mode": "a"}

        # The OCC clash tree is the transient-native-failure boundary `_CLASH_RETRY` wraps in-page:
        # `clash()`/`smart_group_clashes` run under the bounded backoff before the rows read back,
        # exactly as the structural sibling retries the `cytriangle` solve. The whole leg runs inside
        # the outer `boundary` fence, so a non-transient OCC raise still converts to a `BoundaryFault`.
        def solve() -> ClashSet:
            clasher = Clasher(ClashSettings())
            clash_set: ClashSet = {"name": "ifc.clash", "a": [source(sides[0])], "b": [source(sides[1])], "mode": "intersection", "tolerance": 0.001}
            clasher.clash_sets = [clash_set]
            clasher.clash()
            clasher.smart_group_clashes(clasher.clash_sets, max_clustering_distance=1.0)
            return clash_set

        clash_set = _CLASH_RETRY(solve)()
        clashes: tuple[ClashRow, ...] = tuple(clash_set.get("clashes", {}).values())
        clusters = {key: index for index, key in enumerate(clash_set.get("clash_groups", {}))}
        return tuple({**c, "cluster": clusters.get(c.get("group", ""), 0)} for c in clashes)

    @staticmethod
    def _unit_scale(model: "ifcopenshell.file") -> float:
        import ifcopenshell.util.unit  # noqa: PLC0415

        return float(ifcopenshell.util.unit.calculate_unit_scale(model)) ** 2

    @staticmethod
    def _net_area(space: "ifcopenshell.entity_instance") -> float:
        import ifcopenshell.util.element  # noqa: PLC0415

        qtos = ifcopenshell.util.element.get_psets(space, qtos_only=True)
        return next((float(pset["NetFloorArea"]) for pset in qtos.values() if "NetFloorArea" in pset), 0.0)
```

## [03]-[RESEARCH]

- [CLASH_SET_INTERNAL]: the branch `ifcclash` catalogue confirms `Clasher(ClashSettings())`, the `ClashSet` TypedDict (`name`/`a`/`b`/`mode`/`tolerance`/`clearance`/`clashes`), the `ClashSource` TypedDict whose `file: str` key is REQUIRED (`ifcclash.md#78`) alongside the optional `mode` `'a'|'e'|'i'`/`selector`/`ifc` (`#79`–`#81`), the in-place `clashes: dict[str, ClashResult]` accumulation, the `ClashResult.a_global_id`/`b_global_id`/`p1`/`distance` fields, and `Clasher.smart_group_clashes(clash_sets, max_clustering_distance)` (`ifcclash.md#45`/`#100`/`#105`). The `_run_clash` fold builds the `clash_set` and the per-side `source()` against the catalog-confirmed `ClashSet`/`ClashSource` TypedDicts directly — not a `dict[str, object]` literal that erases the `'intersection'`/`'a'`/`'e'` mode literals — carrying the required `file=""` empty-path slot plus the pre-loaded `ifc` model that makes the `load_ifc` leg inert (the catalog-confirmed skip at `ifcclash.md#47`/`#114`), so a source never omits a required key. It writes one `ClashSet` at the `'intersection'` mode plus a `tolerance` policy and runs `clash()` then `smart_group_clashes` in one pass under the in-page `_CLASH_RETRY` `stamina.retry` boundary — the OCC tree being this owner's one transient-native-failure boundary the structural sibling's `cytriangle` weave is the precedent for — then reads back the `clashes` map values typed as the local `ClashRow` (the `ClashResult` shape plus the derived `cluster` index), so the `_clash`/`_topic` legs subscript `c["a_global_id"]`/`c["distance"]`/`c["p1"]` as addressable typed keys rather than blind-indexing an erased bag. The exact spelling the grouper writes the per-clash cluster handle under — whether `clash_set["clash_groups"]` keyed by group id with each `ClashResult` carrying a `group` back-reference, or a flat re-keyed `clashes` map — is the remaining internal-shape detail the live run confirms before the `cluster` index binds (`_run_clash` defaults the index to `0` through the `NotRequired[int]` slot until the grouper handle is confirmed, never dropping the row).
- [BCF_ISSUE_AUTHORING]: the branch `bcf-client` catalogue confirms `bcf.v3.bcfxml.BcfXml.create_new(project_name)`, `BcfXml.add_topic(title, description, author, topic_type='', topic_status='') -> TopicHandler` (`bcf-client.md#87`), `TopicHandler.add_viewpoint_from_point_and_guids(position, *guids) -> VisualizationInfoHandler` (`#103`), and `TopicHandler.topic`/`TopicHandler.guid` (`#104`); the `TopicHandler.topic.title` markup field spelling (the `v3.model.markup.Topic.title` attribute) is the remaining field-name detail the live run confirms. The `BCF` arm is the issue-authoring leg, not a same-string round-trip: it re-runs the clash leg through `_run_clash`, mints one topic per overlap with `add_topic(..., topic_type="Clash")`, and binds each topic to the `ClashResult.p1` overlap point and the offending `a_global_id`/`b_global_id` through `add_viewpoint_from_point_and_guids`, so `ifcclash` + `bcf` stack into one archive.
- [IDS_STRUCTURED_VERDICT]: the branch `ifctester` catalogue confirms `ids.open(filepath, validate=False) -> Ids`, `Ids.validate(ifc_file)`, `Ids.specifications`, `Specification.name`, the tri-state `Specification.status` (`True`/`False`/`None`-skipped, `ifctester.md#112`), and the `Specification.passed_entities`/`failed_entities` per-spec entity sets `Ids.validate` mutates in place (`ifctester.md#112`/`#168`). The `_validate` fold reads the RICH verdict the binary `status` alone drops: the per-spec passing ratio is `len(passed) / max(len(passed) + len(failed), 1)` — a real per-element fraction the `compliance` row carries, not the `1.0`/`0.0` boolean — and the failing-check count is `len(failed)`, with the `status is None` not-applicable/skipped spec EXCLUDED from the rows under the `if spec.status is not None` guard rather than admitted as a `0.0`-ratio row: a skip carries no compliance signal, so folding it would drag the `evidence` mean (`1 - mean(passing-ratio)`) toward a fabricated non-compliance an all-skip model would read as total failure, the subtle ledger-poisoning the guard closes. The catalogue further confirms the `Json(ids).report() -> Results` graph (`percent_checks_pass`/`total_applicable_fail`, `ifctester.md#66`/`#155`) as a sharper residual the evidence ledger keys against if the caller supplies a per-check ceiling; the entity-set fraction is the in-memory verdict the rail ships today and the `Results` graph the deeper roll-up the same `failed_entities`/`passed_entities` sets feed. The `reporter.Bcf` IDS-side archive is NOT this arm's BCF path — `report()` collects rather than writes, so a dangling `reporter.Bcf(document).report()` is a dead side-effect; clash-issue BCF authoring is the `bcf-client` `_author` leg alone (`ifctester.md#156`). The selecting arms never call `util.selector.filter_elements` directly — the validated `IfcSelector.filter`/`IfcSelector.parse` gate from `geometry:ifc/selector.md#SELECTOR` is the sole `filter_elements` caller, so a malformed selector is an `UnexpectedInput`-derived `Error(BoundaryFault)` on the rail at admission. The `CLASH`/`BCF` arms thread the same gate per `ClashSource.selector` side, the `'e'` IfcClass-expression mode against the validated query and the `'a'`-all mode for the empty whole-model default (`ifcclash.md#73`/`#98`).
- [GRADUATION_SUBJECT_TYPE]: the graduation subject is the `ANALYSIS_SUBJECT` module constant binding the canonical `GeometrySubject` `numerical-primitive` literal from `python:compute/graduation/handoff.md#GRADUATION` (the IFC-analysis evidence case the C# owner consumes, the same literal the `geometry:ifc/structural.md#STRUCTURAL` section-integral and `geometry:ifc/costing.md#LIFECYCLE` owners cross on), never a per-receipt `subject: str` field racing the `HandoffAxis(geometry=...)` discriminant the handoff owner deletes, so an unlisted literal fails at the type boundary under `ty`. `AnalysisResult.graduates` folds the kind-specific `evidence` ledger (the mean failing-compliance fraction for IDS/space-program off the real per-spec/space passing ratio, the unresolved-cluster count for CLASH, the empty-result fraction for the takeoff/BCF verbs) through the one `GraduationReceipt.graduates(source_package, HandoffAxis(geometry=subject), evidence_key, measured, ceiling)` admission (handoff.md `#23`/`#26`), never a row count or an inlined comparison.
- [RUNTIME_ASPECT_STACK]: the owner layers the shared/universal branch rails over the folder-specific `ifcopenshell`/`ifctester`/`ifcclash`/`bcf` domain packages as one woven flow, mirroring the `geometry:ifc/structural.md#STRUCTURAL` and `python:compute/graduation/handoff.md#GRADUATION` weave rather than a flat per-library call. `run` owns the one `content.analysis` OTel span the `boundary` fence runs INSIDE — `trace.get_tracer`/`start_as_current_span` (`.api/opentelemetry-api.md`) opening it with the bounded `kind`/`subject` attributes behind the `is_recording()` gate, `Status(StatusCode.OK)` set on the `Ok` arm only through the `_ok` close-out — so the `_convert` (`reliability/faults#FAULT`) records a caught provider exception on the LIVE recording span and sets ERROR; a bare span-less `boundary` would no-op the `is_recording()` egress and drop the fault from the trace, the precise defect the identity/graduation/structural span-owning shape avoids. This owner mints the one `content.analysis` `Tracer` exactly as the structural sibling mints `content.section`, but re-wires no `structlog`/`opentelemetry` SDK chain — the trace-egress weave behind the fence is the faults owner's, the receipt-egress chain the `observability/receipts#RECEIPT` owner's. `@beartype(conf=FAULT_CONF)` on `_dispatch` binds the one shared `BeartypeConf(violation_type=BeartypeCallHintViolation)` (`faults.md` `#127`) so a contract violation raises the canonical root the `CLASSIFY` `api` row folds onto the rail, never an inline `door.is_bearable` tree; `@beartype` on the public `run` entry fences the caller-facing `kind`/`query` contract. `@receipted(_REDACTION)` on the `_emit` step is the egress aspect harvesting the `ReceiptContributor` stream onto `Signals.emit` on the cleared `Ok`, the keep-all `Redaction(classified=Map.empty())` policy mirroring the structural owner's `_REDACTION` since analysis facts carry no secret field — receipt emission a decorator rail, never an inline `Signals.emit` in `run`. `Receipt.of` is the runtime two-arg `of(owner, evidence)` factory (`receipts.md#21`) discriminating the `(phase, subject, facts)` triple — `("emitted", kind, facts)` for the emitted analysis row — and `contribute` yields the `Iterable[Receipt]` port (`receipts.md#24`/`#147`), the same shape `geometry:ifc/authoring.md#AUTHORING` `AuthorReceipt` and the compute `GraduationReceipt` satisfy; the four-positional `Receipt.of("emitted", owner, subject, facts)` call and the single-`Receipt` `contribute` are the forms `compute:graduation/handoff.md#GRADUATION` names deleted. Resilience splits by boundary class exactly as the structural sibling splits its `cytriangle` solve from its lane I/O: the OCC clash tree (`Clasher.clash()` over `ifcopenshell.geom.tree`) is this owner's one transient-native-failure boundary the in-page `_CLASH_RETRY` `stamina.retry(on=RuntimeError, attempts=3, …)` policy wraps around the `clash()`/`smart_group_clashes` pass with bounded exponential backoff — composed where the provider is invoked, never deferred to a downstream lane that drops the retry the OCC kernel demands and never a hand-coded `sleep` loop — while the durable `.bcfzip`/IDS-report archive WRITE stays the `python:data/spatial` boundary's columnar product. The stack is `IfcSelector.filter` → `boundary` fence on a self-owned `content.analysis` span → `@beartype(conf=FAULT_CONF)` `_dispatch` → `_CLASH_RETRY`-wrapped OCC solve / `ifctester` verdict / `bcf` authoring → `@receipted` egress → `_ok` OK close-out → caller's `GraduationReceipt.graduates` admission, one rail end-to-end.
- [TYPED_ANALYSIS_ROW]: the result rows are the `expression` `@tagged_union` `AnalysisRow` (`tagged_union`/`tag`/`case` confirmed `.api/expression.md#55`/`#57`/`#58`), one typed case per verb minted through an `of_*` smart constructor and projected by the one total `facts` `match`/`assert_never` to its `dict[str, object]` — `quantity`/`pset` carrying the GlobalId plus the native-typed measure/property map, `compliance` the subject plus the `float` passing ratio and `int` failing count, `clash` the a/b GUID pair plus the `float` penetration and cluster index, `topic` the guid/title/finding triple — so each typed field stays an addressable fact under the `f"{kind}.{i}.{field}"` key rather than collapsing into one `str(dict)` blob, the typed row the carry and the per-field projection lossless to the field grain. The native `float`/map facts ride the runtime `EventDict` (`dict[str, object]`) the `enc_hook=repr` renderer serializes without a `str()` coerce (`receipts.md#25`), so a new verb is one `AnalysisKind` row, one `of_*` constructor plus one `facts` arm, one `QUERY_SPLIT` row, one dispatch arm, and one `evidence` key.
- [SPACE_QUANTITY_KEY]: the `Qto_SpaceBaseQuantities` `NetFloorArea` quantity key the `_net_area` fold reads from the `get_psets(qtos_only=True)` return, the `IfcSpace` `LongName`/`Name` program-table join key, and `util.unit.calculate_unit_scale(model)` (`ifcopenshell.md#122`) — the project-to-SI length scale the `_unit_scale` fold squares for the area comparison so the program table joins in consistent units — confirm against the branch `ifcopenshell` catalogue.
