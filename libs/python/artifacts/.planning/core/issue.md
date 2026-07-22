# [PY_ARTIFACTS_ISSUE]

`ArtifactIssue` is THE construction root of the artifact plane — the one composition owner where producer nodes become a running pipeline. Callers ask for deliverables and `issue(IssueRequest)` answers by discriminating the closed deliverable-modality union, collecting the terminal producers' `ArtifactWork` node sets, folding them into one `ArtifactPipeline`, refusing a `severed` plan on its own cause, and draining the CPM fronts through the runtime lane whose terminal receipts it returns. It mints no receipt case and no content key — a composition root produces nothing of its own.

Construction for every modality lives here alone, so no producer schedules a sibling and no host learns pipeline internals; `core/plan#PLAN` owns the scheduling engine and never constructs. Modalities split by where construction knowledge lives: a bespoke case exists only where the root must CHAIN producers or derive TARGETS (`sheet_set`/`diagram_suite`/`document_package`), and every other plane — media, scene, chart, drawing, package — rides the one parameterized `works` case, so a new producer family costs zero union edits. `warm` is the elision seed — the runtime `DrainReceipt.cache` threads it front to front so a later node whose key already resolved replays instead of re-rendering, and the durable cross-run fill is the AppHost seam where `Rasm.AppHost` fills `warm` from the Persistence artifact index. Terminals stay producers, each imported DOWNWARD; nothing in artifacts imports this page.

## [01]-[INDEX]

- [01]-[ISSUE]: the polymorphic construction root — `IssueRequest` in, `RuntimeRail[Block[ArtifactReceipt]]` out, over the closed deliverable-modality union with its two-tier mints, the `severed` gate, the front-drain fold, the issue-scope attribution bracket, and the production-fact fire seams.

## [02]-[ISSUE]

- Owner: `ArtifactIssue` carries the runtime `LanePolicy` and the `warm` receipt seed; `issue(request)` is its one entry, request in and `RuntimeRail[Block[ArtifactReceipt]]` out. `IssueRequest` is the closed modality union, each case minted through a thin `@classmethod` (`Sheets`/`Diagrams`/`Package`/`Works`) so a caller never hand-builds a case tuple, and the `works` case is the universal arm — `(nodes, targets)` from ANY producer plane's `emit()` set, a lone node one `Works(node)` call — so singular, plural, and scoped issuance discriminate on the value, never a `single`/`many` sibling pair.
- Cases: the per-modality construction knowledge the fence tuples cannot show — `sheet_set` carries the composed `Transmittal` op and the member production nodes (the sheets and register the aggregate's `parents` name; without them the plan severs on `dangling`), its targets the aggregate node key so the plan scopes to the issue closure; `diagram_suite` carries one shared `Palette` and the non-empty `DiagramLayout` set — each layout's `assign()` resolves the positioned glyphs the root folds into one `DiagramDraw` node per diagram under the root's own `lane` (one `LanePolicy` spans the drain and every render crossing, never a second silent default lane), targets empty because every diagram is itself a deliverable; `document_package` carries the composed `DocumentNode` manual tree (`document/report#REPORT` or the caller composes it — the root composes producers, never document trees), a non-empty `DocumentMode` set, and the `Spec` sections — nodes are each section's `emit()` and `DocumentPlan.bound(node, modes, lane=self.lane, parents=...)`'s per-mode format nodes, so the format targets retain the authored sections in their ancestor closure; `works` carries a non-empty `(nodes, targets)` pair verbatim. `_nodes` refuses an empty deliverable arm through `BoundaryFault.config`, then rails fallible construction: `DocumentPlan.bound` returns `Result[..., EmitFault]` re-spelled at this seam, and the diagram arm's `assign()` rails thread through the runtime `traversed` abort fold — one total `match` closed by `assert_never`.
- Auto: `_cleared` is the severed gate — `plan.severed` folds its `PlanFault` cause (`tag` with the offending `keys` hexes) into one `BoundaryFault.config` refusal so a cyclic, untargeted, collided, or dangling graph never drains; `_driven` folds the CPM fronts in order through `plan.lane.drain(front, carried)`, threading `DrainReceipt.cache` forward exactly as the runtime `StagePlan` drive does — never a hand-rebuilt cache beside the receipt's own — and appending `DrainReceipt.values` (replayed hits included) onto the immutable `Block`; a front with faults rails the whole issue as the `BoundaryFault.combine` reduction of the drain's own fault block, and a cancelled front rails the lane deadline, so receipts are terminal, never partial. One sequential `for` is the async-front exemption: each front depends on the preceding `DrainReceipt.cache`, and `anyio`/`expression` exposes no dependent async fold. Both the `planned` fact and the coverage verdicts stay the plan's; this page adds no parallel telemetry.
- Attribution: `_scoped` is the one attribution bracket — every `issue` call mints a `uuid7().hex` issue scope, derives one context carrying it under `ISSUE_BAGGAGE`, and folds the owner's `tenant: Option[str]` into `TENANT_BAGGAGE` only when the parented context carries none — a parent-carried tenant always wins, so a host seed never re-attributes a paying context — then token-attaches that context around the whole drive, so the runtime worker crossing's `propagate.inject` carries both entries under the telemetry-installed W3C composite with zero producer edits. Tenant projection stays runtime-owned end to end — the metrics `_attributed` fold, the `PROMOTED_BAGGAGE` span/log promotion — so this page never re-binds tenant onto a log, span, or metric; the issue scope alone rides `bound_contextvars` so every structured line inside the drive carries its issue key, and it stays a log/baggage dimension, never a metric attribute, because a per-call id explodes instrument cardinality.
- Facts: production facts fire through `Production.fired` on the `core/hooks#POINTS` rows under the active issue scope, and every fire rail binds before control advances. `issue` fires the `ISSUE_ADMITTED` veto after `_nodes` stages and fires `ISSUE_REFUSED` once at the terminal choke point off the outcome's fault tag, so admission refusals, veto rejections, the severed gate, drain faults, and the lane deadline all reach one observe stream; `_planned` fires `ISSUE_PLANNED` off the cleared plan, and `_driven` fires one `FRONT_DRAINED` per front off the `DrainReceipt` columns onto the replay ring so a late subscriber reads the whole last drain. No parallel fact family: each payload is the projection of a value this page already holds.
- Growth: a new modality is one `IssueRequest` case, one mint, and one `_nodes` arm — earned only by root-owned chaining or targeting knowledge, since a bare producer family already rides `Works`; a new terminal producer joins an existing arm's fold; a host batching policy is a `LanePolicy` value; a cross-run warm fill is the host's Persistence read threaded into `warm`.
- Boundary: no receipt case and no content key; the scheduling algebra, coverage, and elision evidence are `core/plan#PLAN`'s, the drain, retry, offload bounds, and per-unit fault capture the runtime lane's, and the durable warm fill the host's. A second constructing surface, a producer calling a sibling's `emit()`, or a host touching `ArtifactPipeline` directly each break the single-root law. Its rail is composed, never collapsed — a plan fault reaches the caller through `bind`/`match`, never a `.ok` unwrap or a default plan; the drain return is the `DrainReceipt` owner, never iterated as a bare receipt stream; and each async carrier transition uses one total `Result` match because `expression` ships no async carrier builder.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterator
from contextlib import contextmanager
from itertools import chain
from typing import Literal, Self, assert_never
from uuid import uuid7

from expression import Error, Nothing, Ok, Option, Result, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, structs
from opentelemetry import baggage
from opentelemetry import context as otel_context
from structlog.contextvars import bound_contextvars

from rasm.artifacts.core.hooks import ISSUE_BAGGAGE, ArtifactHook, FrontDrained, IssueAdmitted, IssuePlanned, IssueRefused, Production
from rasm.artifacts.core.plan import ArtifactPipeline, ArtifactWork, PipelinePlan
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.delivery.transmittal import Transmittal
from rasm.artifacts.document.emit import DocumentMode, DocumentPlan
from rasm.artifacts.document.model import DocumentNode
from rasm.artifacts.graphic.color.derive import Palette
from rasm.artifacts.specification.section import Spec
from rasm.artifacts.visualization.diagram.draw import DiagramDraw
from rasm.artifacts.visualization.diagram.layout import DiagramLayout
from rasm.runtime.faults import BoundaryFault, RuntimeRail, traversed
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.metrics import TENANT_BAGGAGE

# --- [TYPES] ----------------------------------------------------------------------------

type Staged = tuple[tuple[ArtifactWork, ...], frozenset[ContentKey]]


@tagged_union(frozen=True)
class IssueRequest:
    tag: Literal["sheet_set", "diagram_suite", "document_package", "works"] = tag()
    sheet_set: tuple[Transmittal, tuple[ArtifactWork, ...]] = case()
    diagram_suite: tuple[Palette, tuple[DiagramLayout, ...]] = case()
    document_package: tuple[DocumentNode, tuple[DocumentMode, ...], tuple[Spec, ...]] = case()
    works: Staged = case()

    @classmethod
    def Sheets(cls, transmittal: Transmittal, /, *members: ArtifactWork) -> Self:
        return cls(sheet_set=(transmittal, members))

    @classmethod
    def Diagrams(cls, palette: Palette, /, *layouts: DiagramLayout) -> Self:
        return cls(diagram_suite=(palette, layouts))

    @classmethod
    def Package(cls, node: DocumentNode, modes: tuple[DocumentMode, ...], /, *sections: Spec) -> Self:
        return cls(document_package=(node, modes, sections))

    @classmethod
    def Works(cls, *works: ArtifactWork, targets: frozenset[ContentKey] = frozenset()) -> Self:
        return cls(works=(works, targets))


# --- [SERVICES] -------------------------------------------------------------------------


class ArtifactIssue(Struct, frozen=True):
    lane: LanePolicy
    warm: Map[ContentKey, ArtifactReceipt] = Map.empty()
    tenant: Option[str] = Nothing

    async def issue(self, request: IssueRequest, /) -> RuntimeRail[Block[ArtifactReceipt]]:
        with self._scoped() as scope:
            outcome = await self._issued(request, scope)
            match outcome:
                case Result(tag="error", error=fault) as refused:
                    return Production.fired(
                        ArtifactHook.ISSUE_REFUSED,
                        IssueRefused(cause=fault.tag, scope=scope),
                        scope=scope,
                    ).bind(lambda _fact: refused)
                case Result(tag="ok"):
                    return outcome
                case _ as unreachable:
                    assert_never(unreachable)

    async def _issued(self, request: IssueRequest, scope: str, /) -> RuntimeRail[Block[ArtifactReceipt]]:
        staged = await self._nodes(request)
        admitted = staged.bind(
            lambda pair: Production.fired(
                ArtifactHook.ISSUE_ADMITTED,
                IssueAdmitted(modality=request.tag, works=len(pair[0]), targets=len(pair[1]), scope=scope),
                scope=scope,
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
        self, works: tuple[ArtifactWork, ...], targets: frozenset[ContentKey], scope: str, /
    ) -> RuntimeRail[Block[ArtifactReceipt]]:
        gated = (await ArtifactPipeline.of(works, lane=self.lane, warm=self.warm, targets=targets).plan()).bind(self._cleared)
        match gated:
            case Result(tag="error") as severed:
                return severed
            case Result(tag="ok", ok=plan):
                observed = Production.fired(
                    ArtifactHook.ISSUE_PLANNED,
                    IssuePlanned(works=len(works), fronts=len(plan.fronts), targets=len(targets), scope=scope),
                    scope=scope,
                )
                match observed:
                    case Result(tag="error") as failed:
                        return failed
                    case Result(tag="ok"):
                        return await self._driven(plan, scope)
                    case _ as unreachable:
                        assert_never(unreachable)
            case _ as unreachable:
                assert_never(unreachable)

    @staticmethod
    def _cleared(plan: PipelinePlan, /) -> RuntimeRail[PipelinePlan]:
        return (
            plan.severed
            .map(lambda fault: Error(BoundaryFault(config=("artifacts.issue.plan", f"{fault.tag}:{','.join(sorted(key.hex for key in fault.keys))}"))))
            .default_value(Ok(plan))
        )

    async def _nodes(self, request: IssueRequest, /) -> RuntimeRail[Staged]:
        match request:
            case IssueRequest(tag="sheet_set", sheet_set=(_transmittal, ())):
                # every sibling arm carries this empty-set refusal — an aggregate over zero members would emit anyway
                return Error(BoundaryFault(config=("artifacts.issue.sheet", "empty member set")))
            case IssueRequest(tag="sheet_set", sheet_set=(transmittal, members)):
                aggregate = transmittal.emit()
                return Ok(((*members, aggregate), frozenset((aggregate.key,))))
            case IssueRequest(tag="diagram_suite", diagram_suite=(_palette, ())):
                return Error(BoundaryFault(config=("artifacts.issue.diagram", "empty layout set")))
            case IssueRequest(tag="diagram_suite", diagram_suite=(palette, layouts)):
                # ONE root lane spans the whole suite: each layout's lane is rebound to the issue's, so
                # placement, drawing, and plan draining share capacity, deadline, and retry scope.
                assigned = traversed(Block.of_seq([await structs.replace(laid, lane=self.lane).assign() for laid in layouts]))
                return assigned.map(
                    lambda suites: (
                        tuple(chain.from_iterable(DiagramDraw(glyphs=glyphs, palette=palette, lane=self.lane).emit() for glyphs in suites)),
                        frozenset(),
                    )
                )
            case IssueRequest(tag="document_package", document_package=(_node, (), _sections)):
                return Error(BoundaryFault(config=("artifacts.issue.package", "empty mode set")))
            case IssueRequest(tag="document_package", document_package=(node, modes, sections)):
                authored = tuple(spec.emit() for spec in sections)
                return (
                    DocumentPlan.bound(node, modes, lane=self.lane, parents=tuple(work.key for work in authored))
                    .map_error(lambda fault: BoundaryFault(config=("artifacts.issue.package", fault.tag)))
                    .map(lambda formats: ((*authored, *formats), frozenset(work.key for work in formats)))
                )
            case IssueRequest(tag="works", works=((), _targets)):
                return Error(BoundaryFault(config=("artifacts.issue.works", "empty work set")))
            case IssueRequest(tag="works", works=staged):
                return Ok(staged)
            case _ as unreachable:
                assert_never(unreachable)

    async def _driven(self, plan: PipelinePlan, scope: str, /) -> RuntimeRail[Block[ArtifactReceipt]]:
        carried, collected = plan.cache_seed, Block.empty()
        for index, front in enumerate(plan.fronts):
            receipt = await plan.lane.drain(front, carried)
            observed = Production.fired(
                ArtifactHook.FRONT_DRAINED,
                FrontDrained(
                    front=index,
                    accepted=receipt.accepted,
                    completed=receipt.completed,
                    cancelled=receipt.cancelled,
                    rejected=receipt.rejected,
                    hit=receipt.hit,
                    scope=scope,
                ),
                scope=scope,
            )
            match observed:
                case Result(tag="error") as failed:
                    return failed
                case Result(tag="ok"):
                    pass
                case _ as unreachable:
                    assert_never(unreachable)
            match receipt.faults, receipt.cancelled:
                case faults, _ if not faults.is_empty():
                    return Error(faults.reduce(BoundaryFault.combine))
                case _, cancelled if cancelled:
                    return Error(BoundaryFault(deadline=("artifacts.issue", plan.lane.deadline.default_value(float("inf")), "front cancelled")))
                case _:
                    carried, collected = receipt.cache, collected.append(receipt.values)
        return Ok(collected)

    @contextmanager
    def _scoped(self, /) -> Iterator[str]:
        # Exemption: the attach/detach token pair and the bound_contextvars bracket are the platform's scoped-context seam.
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


# --- [EXPORTS] ----------------------------------------------------------------------------

__all__ = ("ArtifactIssue", "IssueRequest")
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
