# [PY_ARTIFACTS_ISSUE]

`ArtifactIssue` is THE construction root of the artifact plane — the one composition owner where producer nodes become a running pipeline. Callers ask for deliverables and `issue(IssueRequest)` answers by discriminating the closed deliverable-modality union, collecting the terminal producers' `ArtifactWork` node sets, folding them into one `ArtifactPipeline`, refusing a `severed` plan on its own cause, and draining the CPM fronts through the runtime lane whose terminal receipts it returns. It mints no receipt case and no content key — a composition root produces nothing of its own.

Construction for every modality lives here alone, so no producer schedules a sibling and no host learns pipeline internals; `core/plan#PLAN` owns the scheduling engine and never constructs. Modalities split by where construction knowledge lives: a bespoke case exists only where the root must CHAIN producers or derive TARGETS (`sheet_set`/`diagram_suite`/`document_package`), and every other plane — media, scene, chart, drawing, package — rides the one parameterized `works` case, so a new producer family costs zero union edits. `warm` is the elision seed — the runtime `DrainReceipt.cache` threads it front to front so a later node whose key already resolved replays instead of re-rendering, and the durable cross-run fill is the AppHost seam where `Rasm.AppHost` fills `warm` from the Persistence artifact index. Terminals stay producers, each imported DOWNWARD; nothing in artifacts imports this page.

## [01]-[INDEX]

- [01]-[ISSUE]: the polymorphic construction root — `IssueRequest` in, `RuntimeRail[Block[ArtifactReceipt]]` out, over the closed deliverable-modality union with its two-tier mints, the `severed` gate, and the front-drain fold.

## [02]-[ISSUE]

- Owner: `ArtifactIssue` carries the runtime `LanePolicy` and the `warm` receipt seed; `issue(request)` is its one entry, request in and `RuntimeRail[Block[ArtifactReceipt]]` out. `IssueRequest` is the closed modality union, each case minted through a thin `@classmethod` (`Sheets`/`Diagrams`/`Package`/`Works`) so a caller never hand-builds a case tuple, and the `works` case is the universal arm — `(nodes, targets)` from ANY producer plane's `emit()` set, a lone node one `Works(node)` call — so singular, plural, and scoped issuance discriminate on the value, never a `single`/`many` sibling pair.
- Cases: the per-modality construction knowledge the fence tuples cannot show — `sheet_set` carries the composed `Transmittal` op plus the member production nodes (the sheets and register the aggregate's `parents` name; without them the plan severs on `dangling`), its targets the aggregate node key so the plan scopes to the issue closure; `diagram_suite` carries one shared `Palette` and the non-empty `DiagramLayout` set — each layout's `assign()` resolves the positioned glyphs the root folds into one `DiagramDraw` node per diagram under the root's own `lane` (one `LanePolicy` spans the drain and every render crossing, never a second silent default lane), targets empty because every diagram is itself a deliverable; `document_package` carries the composed `DocumentNode` manual tree (`document/report#REPORT` or the caller composes it — the root composes producers, never document trees), a non-empty `DocumentMode` set, and the `Spec` sections — nodes are each section's `emit()` plus `DocumentPlan.bound(node, modes, lane=self.lane, parents=...)`'s per-mode format nodes, so the format targets retain the authored sections in their ancestor closure; `works` carries a non-empty `(nodes, targets)` pair verbatim. `_nodes` refuses an empty deliverable arm through `BoundaryFault.config`, then rails fallible construction: `DocumentPlan.bound` returns `Result[..., EmitFault]` re-spelled at this seam, and the diagram arm's `assign()` rails thread through the runtime `traversed` abort fold — one total `match` closed by `assert_never`.
- Auto: `_cleared` is the severed gate — `plan.severed` folds its `PlanFault` cause (`tag` plus the offending `keys` hexes) into one `BoundaryFault.config` refusal so a cyclic, untargeted, collided, or dangling graph never drains; `_driven` folds the CPM fronts in order through `plan.lane.drain(front, carried)`, threading `DrainReceipt.cache` forward exactly as the runtime `StagePlan` drive does — never a hand-rebuilt cache beside the receipt's own — and appending `DrainReceipt.values` (replayed hits included) onto the immutable `Block`; a front with faults rails the whole issue as the `BoundaryFault.combine` reduction of the drain's own fault block, and a cancelled front rails the lane deadline, so receipts are terminal, never partial. One sequential `for` is the async-front exemption: each front depends on the preceding `DrainReceipt.cache`, and `anyio`/`expression` exposes no dependent async fold. Both the `planned` fact and the coverage verdicts stay the plan's; this page adds no parallel telemetry.
- Growth: a new modality is one `IssueRequest` case plus one mint plus one `_nodes` arm — earned only by root-owned chaining or targeting knowledge, since a bare producer family already rides `Works`; a new terminal producer joins an existing arm's fold; a host batching policy is a `LanePolicy` value; a cross-run warm fill is the host's Persistence read threaded into `warm`.
- Boundary: no receipt case and no content key; the scheduling algebra, coverage, and elision evidence are `core/plan#PLAN`'s, the drain, retry, offload bounds, and per-unit fault capture the runtime lane's, and the durable warm fill the host's. A second constructing surface, a producer calling a sibling's `emit()`, or a host touching `ArtifactPipeline` directly each break the single-root law. Its rail is composed, never collapsed — a plan fault reaches the caller through `bind`/`match`, never a `.ok` unwrap or a default plan; the drain return is the `DrainReceipt` owner, never iterated as a bare receipt stream; and each async carrier transition uses one total `Result` match because `expression` ships no async carrier builder.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from itertools import chain
from typing import Literal, Self, assert_never

from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, structs

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

    async def issue(self, request: IssueRequest, /) -> RuntimeRail[Block[ArtifactReceipt]]:
        match await self._nodes(request):
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=(works, targets)):
                return await self._planned(works, targets)
            case _ as unreachable:
                assert_never(unreachable)

    async def _planned(self, works: tuple[ArtifactWork, ...], targets: frozenset[ContentKey], /) -> RuntimeRail[Block[ArtifactReceipt]]:
        gated = (await ArtifactPipeline.of(works, lane=self.lane, warm=self.warm, targets=targets).plan()).bind(self._cleared)
        match gated:
            case Result(tag="error") as severed:
                return severed
            case Result(tag="ok", ok=plan):
                return await self._driven(plan)
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
                # the same empty-set refusal every sibling arm carries — an aggregate over zero members would emit anyway
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

    async def _driven(self, plan: PipelinePlan, /) -> RuntimeRail[Block[ArtifactReceipt]]:
        carried, collected = plan.cache_seed, Block.empty()
        for front in plan.fronts:
            receipt = await plan.lane.drain(front, carried)
            match receipt.faults, receipt.cancelled:
                case faults, _ if not faults.is_empty():
                    return Error(faults.reduce(BoundaryFault.combine))
                case _, cancelled if cancelled:
                    return Error(BoundaryFault(deadline=("artifacts.issue", plan.lane.deadline.default_value(float("inf")), "front cancelled")))
                case _:
                    carried, collected = receipt.cache, collected.append(receipt.values)
        return Ok(collected)


# --- [EXPORTS] ----------------------------------------------------------------------------

__all__ = ("ArtifactIssue", "IssueRequest")
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
