# [PY_ARTIFACTS_ISSUE]

`ArtifactIssue` is THE construction root of the artifact plane — the one composition owner where producer nodes become a running pipeline. Callers ask for deliverables and `issue(IssueRequest)` answers by discriminating the closed deliverable-modality union, collecting the terminal producers' `ArtifactWork` node sets, folding them into one `ArtifactPipeline`, refusing a `severed` plan on its own cause, and returning the runtime lane's content-keyed cache after the CPM fronts drain. It mints no content key.

Construction for every modality lives here alone, so no producer schedules a sibling and no host learns pipeline internals; `core/plan#PLAN` owns the scheduling engine and never constructs. Modalities split by where construction knowledge lives: a bespoke case exists only where the root must CHAIN producers or derive TARGETS (`sheet_set`/`diagram_suite`/`document_package`), and every other plane — media, scene, chart, drawing, package — rides the one parameterized `works` case, so a new producer family costs zero union edits. `warm` is the elision seed — the runtime `Drained.cache` threads it front to front so a later node whose key already resolved replays instead of re-rendering — and it arrives caller-supplied: a durable cross-run fill is the composing application's own content-keyed index read threaded into the field, keyed by the `ContentKey` each producer minted. This package assumes no consumer and names none, so no peer estate owns that fill. Terminals stay producers, each imported DOWNWARD; nothing in artifacts imports this page.

## [01]-[INDEX]

- [02]-[ISSUE]: the polymorphic construction root — `IssueRequest` in, `RuntimeRail[Map[ContentKey, object]]` out, over the closed deliverable-modality union with its two-tier mints, the `RAISES` refusal roster, the `severed` gate, the lane-drive composition, the issue-scope attribution bracket, and the production-fact fire seams.

## [02]-[ISSUE]

- Owner: `ArtifactIssue` carries the runtime `LanePolicy` and the `warm` cache seed; `issue(request)` is its one entry, request in and `RuntimeRail[Map[ContentKey, object]]` out. Each cache entry preserves the producer's returned value under its pre-minted key; `object` closes the mixed-producer root and callers narrow through the key they supplied. `IssueRequest` is the closed modality union, each case minted through a thin `@classmethod` (`Sheets`/`Diagrams`/`Package`/`Works`) so a caller never hand-builds a case tuple, and the `works` case is the universal arm — `(nodes, targets)` from ANY producer plane's `emit()` set, a lone node one `Works(node)` call — so singular, plural, and scoped issuance discriminate on the value, never a `single`/`many` sibling pair.
- Cases: the per-modality construction knowledge the fence tuples cannot show — `sheet_set` carries the composed `Transmittal` op and the member production nodes (the sheets and register the aggregate's `parents` name; without them the plan severs on `dangling`), its targets the aggregate node key so the plan scopes to the issue closure; `diagram_suite` carries one shared `Palette` and the non-empty `DiagramLayout` set — each layout's `assign()` resolves the positioned glyphs the root folds into one `DiagramDraw` node per diagram under the root's own `lane` (one `LanePolicy` spans the drain and every render crossing, never a second silent default lane), targets empty because every diagram is itself a deliverable; `document_package` carries the composed `DocumentNode` manual tree (`document/report#REPORT` or the caller composes it — the root composes producers, never document trees), a non-empty `DocumentMode` set, and the `Spec` sections — nodes are each section's `emit()` and `DocumentPlan.bound(node, modes, lane=self.lane, parents=...)`'s per-mode format nodes, so the format targets retain the authored sections in their ancestor closure; `works` carries a non-empty `(nodes, targets)` pair verbatim. `_nodes` refuses an empty deliverable arm through the ONE parameterized `ISSUE_EMPTY` row, whose slots carry the modality and the member set the caller left empty — four arms, one law — then rails fallible construction: `DocumentPlan.bound` returns `Result[..., EmitFault]` crossing whole as `BoundaryFault.domain` under the `ISSUE_PACKAGE` coordinate, so the format planner's own case and kwargs stay matchable rather than collapsing to its tag, and the diagram arm's `assign()` rails thread through the runtime `traversed` abort fold — one total `match` closed by `assert_never`.
- Law: `_scoped` is the one attribution bracket — every `issue` call mints a `uuid7().hex` issue scope, derives one context carrying it under `ISSUE_BAGGAGE`, and folds the owner's `tenant: Option[str]` into `TENANT_BAGGAGE` only when the parented context carries none — a parent-carried tenant always wins, so a host seed never re-attributes a paying context — then token-attaches that context around the whole drive, so the runtime worker crossing's `propagate.inject` carries both entries under the telemetry-installed W3C composite with zero producer edits. Tenant projection stays runtime-owned end to end — the metrics `_attributed` fold, the `PROMOTED_BAGGAGE` span/log promotion, and the journal writer filling a fact's unset `tenant` at admission — so this page never re-binds tenant onto a log, span, metric, or durable row, and no producer reads baggage to attribute one; the issue scope alone rides `bound_contextvars` so every structured line inside the drive carries its issue key, and it stays a log/baggage dimension, never a metric attribute, because a per-call id explodes instrument cardinality.
- Law: issue lifecycle points fire through `Production.fired` under the lane's composition key. `ISSUE_ADMITTED` can veto staged work; planned, refused, and drained observations cannot replace the issue result.
- Auto: `_cleared` is the severed gate — `plan.severed` folds its `PlanFault` cause onto the `ISSUE_PLAN` row's two named coordinates, the tag and the offending key hexes, so a cyclic, untargeted, collided, or dangling graph never drains; `_driven` is COMPOSITION over `execution/lanes#LANE` `LanePolicy.driven`, not a drive: it hands the already-resolved CPM ladder through the `Fronts` `resolved` arm labelled with its own `ArtifactStage` member, seeds the warm cache, carries the drive's `Drained.cache` forward as the answer (replayed hits included), and names the gate point. The fault-block short-circuit, the cancelled-front deadline rail, the threaded cache, and the sequential-front exemption are all the drive's, stated once at the owner, so results stay terminal and no second loop can drift out of any of them. Both the `planned` fact and the coverage verdicts stay the plan's; this page adds no parallel telemetry.
- Growth: a new refusal is one `RAISES` row under an `ArtifactsLeg.ISSUE` anchor, or one slot on the row whose law already covers it; a new modality is one `IssueRequest` case, one mint, and one `_nodes` arm — earned only by root-owned chaining or targeting knowledge, since a bare producer family already rides `Works`; a new terminal producer joins an existing arm's fold; a host batching policy is a `LanePolicy` value; a cross-run warm fill is the composing host's own durable index read threaded into `warm`.
- Boundary: `ArtifactIssue` returns the lane cache directly and mints no content key; the scheduling algebra, coverage, and elision evidence are `core/plan#PLAN`'s, the front drive, drain, retry, offload bounds, and per-unit fault capture the runtime lane's, and the durable warm fill the host's. A second constructing surface, a producer calling a sibling's `emit()`, or a host touching `ArtifactPipeline` directly each break the single-root law. Its rail is composed, never collapsed — a plan fault reaches the caller through `bind`/`match`, never a `.ok` unwrap or a default plan; the drive return is the lane's rail, never iterated as a bare result stream; and each async carrier transition uses one total `Result` match because `expression` ships no async carrier builder.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Iterator
from contextlib import contextmanager
from itertools import chain
from typing import Final, Literal, Self, assert_never
from uuid import uuid7

from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, structs
from opentelemetry import baggage
from opentelemetry import context as otel_context
from structlog.contextvars import bound_contextvars

from rasm.artifacts.core.hooks import ISSUE_BAGGAGE, ArtifactHook, ArtifactStage, ArtifactsLeg, IssueAdmitted, IssuePlanned, IssueRefused, Production
from rasm.artifacts.core.plan import ArtifactPipeline, ArtifactWork, PipelinePlan
from rasm.artifacts.delivery.transmittal import Transmittal
from rasm.artifacts.document.emit import DocumentMode, DocumentPlan
from rasm.artifacts.document.model import DocumentNode
from rasm.artifacts.graphic.color.derive import Palette
from rasm.artifacts.specification.section import Spec
from rasm.artifacts.visualization.diagram.draw import DiagramDraw
from rasm.artifacts.visualization.diagram.layout import DiagramLayout
from rasm.runtime.faults import TERMINAL, BoundaryFault, FaultRow, RuntimeRail, rostered, traversed
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import Fronts, LanePolicy
from rasm.runtime.metrics import TENANT_BAGGAGE

# --- [TABLES] ---------------------------------------------------------------------------

ISSUE_EMPTY: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.ISSUE, point="stage", arm="config", defect="empty-deliverable", retriability=TERMINAL, slots=("modality", "member")
)
ISSUE_PLAN: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.ISSUE, point="plan", arm="config", defect="plan-severed", retriability=TERMINAL, slots=("cause", "keys")
)
ISSUE_PACKAGE: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.ISSUE, point="package", arm="config", defect="format-plan-refused", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[ArtifactsLeg]]] = rostered(Block.of_seq([ISSUE_EMPTY, ISSUE_PLAN, ISSUE_PACKAGE]))


@tagged_union(frozen=True)
class IssueRequest:
    tag: Literal["sheet_set", "diagram_suite", "document_package", "works"] = tag()
    sheet_set: tuple[Transmittal, tuple[ArtifactWork[object], ...]] = case()
    diagram_suite: tuple[Palette, tuple[DiagramLayout, ...]] = case()
    document_package: tuple[DocumentNode, tuple[DocumentMode, ...], tuple[Spec, ...]] = case()
    works: tuple[tuple[ArtifactWork[object], ...], frozenset[ContentKey]] = case()

    @classmethod
    def Sheets(cls, transmittal: Transmittal, /, *members: ArtifactWork[object]) -> Self:
        return cls(sheet_set=(transmittal, members))

    @classmethod
    def Diagrams(cls, palette: Palette, /, *layouts: DiagramLayout) -> Self:
        return cls(diagram_suite=(palette, layouts))

    @classmethod
    def Package(cls, node: DocumentNode, modes: tuple[DocumentMode, ...], /, *sections: Spec) -> Self:
        return cls(document_package=(node, modes, sections))

    @classmethod
    def Works(cls, *works: ArtifactWork[object], targets: frozenset[ContentKey] = frozenset()) -> Self:
        return cls(works=(works, targets))


# --- [SERVICES] -------------------------------------------------------------------------


class ArtifactIssue(Struct, frozen=True):
    lane: LanePolicy
    warm: Map[ContentKey, object] = Map.empty()
    tenant: Option[str] = Nothing

    async def issue(self, request: IssueRequest, /) -> RuntimeRail[Map[ContentKey, object]]:
        with self._scoped() as scope:
            outcome = await self._issued(request, scope)
            match outcome:
                case Result(tag="error", error=fault) as refused:
                    Production.fired(
                        ArtifactHook.ISSUE_REFUSED,
                        IssueRefused(cause=fault.tag, scope=scope),
                        scope=self.lane.scope,
                    )
                    return refused
                case Result(tag="ok"):
                    return outcome
                case _ as unreachable:
                    assert_never(unreachable)

    async def _issued(self, request: IssueRequest, scope: str, /) -> RuntimeRail[Map[ContentKey, object]]:
        staged = await self._nodes(request)
        admitted = staged.bind(
            lambda pair: Production.fired(
                ArtifactHook.ISSUE_ADMITTED,
                IssueAdmitted(modality=request.tag, works=len(pair[0]), targets=len(pair[1]), scope=scope),
                scope=self.lane.scope,
            ).map(lambda _fact: pair)
        )
        match admitted:
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=(works, targets)):
                return await self._planned(works, targets, scope)
            case _ as unreachable:
                assert_never(unreachable)

    async def _planned(
        self, works: tuple[ArtifactWork[object], ...], targets: frozenset[ContentKey], scope: str, /
    ) -> RuntimeRail[Map[ContentKey, object]]:
        gated = (await ArtifactPipeline.of(works, lane=self.lane, warm=self.warm, targets=targets).plan()).bind(self._cleared)
        match gated:
            case Result(tag="error") as severed:
                return severed
            case Result(tag="ok", ok=plan):
                Production.fired(
                    ArtifactHook.ISSUE_PLANNED,
                    IssuePlanned(works=len(works), fronts=len(plan.fronts), targets=len(targets), scope=scope),
                    scope=self.lane.scope,
                )
                return await self._driven(plan)
            case _ as unreachable:
                assert_never(unreachable)

    @staticmethod
    def _cleared(plan: PipelinePlan[object], /) -> RuntimeRail[PipelinePlan[object]]:
        return (
            plan.severed
            .map(lambda fault: Error(ISSUE_PLAN.raised(fault.tag, ",".join(sorted(key.hex for key in fault.keys)))))
            .default_value(Ok(plan))
        )

    async def _nodes(
        self, request: IssueRequest, /
    ) -> RuntimeRail[tuple[tuple[ArtifactWork[object], ...], frozenset[ContentKey]]]:
        match request:
            case IssueRequest(tag="sheet_set", sheet_set=(_transmittal, ())):
                return Error(ISSUE_EMPTY.raised(request.tag, "members"))
            case IssueRequest(tag="sheet_set", sheet_set=(transmittal, members)):
                aggregate = transmittal.emit()
                return Ok(((*members, aggregate), frozenset((aggregate.key,))))
            case IssueRequest(tag="diagram_suite", diagram_suite=(_palette, ())):
                return Error(ISSUE_EMPTY.raised(request.tag, "layouts"))
            case IssueRequest(tag="diagram_suite", diagram_suite=(palette, layouts)):
                assigned = traversed(Block.of_seq([await structs.replace(laid, lane=self.lane).assign() for laid in layouts]))
                return assigned.map(
                    lambda suites: (
                        tuple(chain.from_iterable(DiagramDraw(glyphs=glyphs, palette=palette, lane=self.lane).emit() for glyphs in suites)),
                        frozenset(),
                    )
                )
            case IssueRequest(tag="document_package", document_package=(_node, (), _sections)):
                return Error(ISSUE_EMPTY.raised(request.tag, "modes"))
            case IssueRequest(tag="document_package", document_package=(node, modes, sections)):
                authored = tuple(spec.emit() for spec in sections)
                return (
                    DocumentPlan.bound(node, modes, lane=self.lane, parents=tuple(work.key for work in authored))
                    .map_error(lambda fault: BoundaryFault(domain=(ISSUE_PACKAGE.subject, fault)))
                    .map(lambda formats: ((*authored, *formats), frozenset(work.key for work in formats)))
                )
            case IssueRequest(tag="works", works=((), _targets)):
                return Error(ISSUE_EMPTY.raised(request.tag, "works"))
            case IssueRequest(tag="works", works=staged):
                return Ok(staged)
            case _ as unreachable:
                assert_never(unreachable)

    async def _driven(self, plan: PipelinePlan[object], /) -> RuntimeRail[Map[ContentKey, object]]:
        match Production.registered(scope=plan.lane.scope):
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok"):
                return await plan.lane.driven(
                    Fronts(resolved=Block.of_seq([(ArtifactStage.DRAIN.value, front) for front in plan.fronts])),
                    plan.cache_seed,
                    lambda _held, drained: Ok(drained.cache),
                    cache=plan.cache_seed,
                    gate=Some(ArtifactHook.FRONT_DRAINED),
                )
            case _ as unreachable:
                assert_never(unreachable)

    @contextmanager
    def _scoped(self, /) -> Iterator[str]:
        scope = uuid7().hex
        held = baggage.set_baggage(ISSUE_BAGGAGE, scope, otel_context.get_current())
        carried = baggage.get_baggage(TENANT_BAGGAGE, held)
        seeded = (
            held
            if carried is not None
            else self.tenant.map(lambda tenant: baggage.set_baggage(TENANT_BAGGAGE, tenant, held)).default_value(held)
        )
        token = otel_context.attach(seeded)
        try:
            with bound_contextvars(**{ISSUE_BAGGAGE: scope}):
                yield scope
        finally:
            otel_context.detach(token)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("ArtifactIssue", "IssueRequest")
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
