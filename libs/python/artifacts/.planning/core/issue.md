# [PY_ARTIFACTS_ISSUE]

`ArtifactIssue` is THE construction root of the artifact plane — the one composition owner where producer nodes become a running pipeline. Callers ask for deliverables and `issue(IssueRequest)` answers by discriminating the closed deliverable-modality union, collecting the terminal producers' `ArtifactWork` node sets, and folding them into one planned-and-drained `ArtifactPipeline` whose terminal receipts it returns. It mints no receipt case and no content key — a composition root produces nothing of its own.

Construction for every modality lives here alone, so no producer schedules a sibling and no host learns pipeline internals; `core/plan#PLAN` owns the scheduling engine and never constructs. `warm` is the elision seed — in-session receipts accumulate front by front so a later node whose key already resolved replays instead of re-rendering, and the durable cross-run fill is the AppHost seam where `Rasm.AppHost` fills `warm` from the Persistence artifact index. The terminals stay producers, each imported DOWNWARD; nothing in artifacts imports this page.

## [01]-[INDEX]

- [01]-[ISSUE]: the polymorphic construction root — `IssueRequest` in, `RuntimeRail[Block[ArtifactReceipt]]` out, over the closed deliverable-modality union.

## [02]-[ISSUE]

- Owner: `ArtifactIssue` carries the runtime `LanePolicy` and the `warm` receipt seed; `issue(request)` is its one entry, request in and `RuntimeRail[Block[ArtifactReceipt]]` out.
- Cases: `IssueRequest` is the closed discriminant, its per-modality `targets` the fence-invisible decision — `sheet_set` targets the aggregate key so the plan scopes to the issue closure, `diagram_suite` leaves targets empty because every diagram is itself a deliverable, `document_package` names the `DocumentEgress` finishing node as its target, and `single` admits a lone node unchanged.
- Auto: `_driven` folds the CPM fronts in order — each drains through `LanePolicy.drain(front, cache)` and its `Ok` receipts fold into the accumulating cache keyed by `receipt.slot`, so within-run elision is exact node granularity; a front fault rails the whole issue, receipts terminal never partial. The `planned` fact and the coverage verdicts stay the plan's; this page adds no parallel telemetry.
- Growth: a new modality is one `IssueRequest` case plus one `_nodes` arm; a new terminal producer joins an existing arm's fold; a host batching policy is a `LanePolicy` value.
- Boundary: no receipt case and no content key; the scheduling algebra, coverage, and elision evidence are `core/plan#PLAN`'s, the drain, retry, and offload bounds the runtime lane's, and the durable warm fill the host's. A second constructing surface, a producer calling a sibling's `emit()`, or a host touching `ArtifactPipeline` directly each break the single-root law.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from dataclasses import dataclass
from typing import Literal

from expression import case, tag, tagged_union
from expression.collections import Block, Map

from rasm.artifacts.core.plan import ArtifactPipeline, ArtifactWork, PipelinePlan
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.delivery.transmittal import Transmittal, TransmittalRecord
from rasm.artifacts.document.egress import DocumentEgress
from rasm.artifacts.document.emit import DocumentPlan
from rasm.artifacts.specification.section import Spec
from rasm.artifacts.visualization.diagram.draw import DiagramDraw
from rasm.artifacts.visualization.diagram.layout import DiagramLayout
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.identity import ContentKey
from rasm.runtime.lanes import LanePolicy


# --- [TYPES] ----------------------------------------------------------------------------
@tagged_union(frozen=True)
class IssueRequest:  # the closed deliverable-modality discriminant, one arity over all modalities
    tag: Literal["sheet_set", "diagram_suite", "document_package", "single"] = tag()
    sheet_set: TransmittalRecord = case()  # terminal = delivery/transmittal (aggregate + members)
    diagram_suite: tuple[DiagramLayout, ...] = case()  # terminal = visualization/diagram/draw, one node per kind
    document_package: tuple[Spec, ...] = case()  # terminal chain = section -> emit -> egress
    single: ArtifactWork = case()  # any lone producer node


# --- [COMPOSITION] ----------------------------------------------------------------------
@dataclass(frozen=True, slots=True, kw_only=True)
class ArtifactIssue:
    lane: LanePolicy
    warm: Map[ContentKey, ArtifactReceipt] = Map.empty()  # elision seed — in-session now, Persistence fills it cross-run

    async def issue(self, request: IssueRequest, /) -> RuntimeRail[Block[ArtifactReceipt]]:
        nodes, targets = self._nodes(request)
        plan = await ArtifactPipeline.of(nodes, lane=self.lane, warm=self.warm, targets=targets).plan()
        return await self._driven(plan)

    def _nodes(self, request: IssueRequest, /) -> tuple[tuple[ArtifactWork, ...], frozenset[ContentKey]]:
        match request:
            case IssueRequest(tag="sheet_set", sheet_set=record):
                issue = Transmittal.of(record)
                nodes = tuple(issue.emit())  # member nodes + ONE aggregate whose parents are the member keys
                return (nodes, frozenset({nodes[-1].key}))
            case IssueRequest(tag="diagram_suite", diagram_suite=layouts):
                return (tuple(node for laid in layouts for node in (DiagramDraw(layout=laid).emit(),)), frozenset())
            case IssueRequest(tag="document_package", document_package=sections):
                authored = tuple(section.emit() for section in sections)
                formats = tuple(DocumentPlan.bound(parents=tuple(n.key for n in authored)).emit())
                finish = DocumentEgress.of(parents=tuple(n.key for n in formats)).emit()
                return ((*authored, *formats, finish), frozenset({finish.key}))
            case IssueRequest(tag="single", single=node):
                return ((node,), frozenset())

    async def _driven(self, plan: PipelinePlan, /) -> RuntimeRail[Block[ArtifactReceipt]]:
        # each Ok receipt folds into the cache keyed by receipt.slot so a later resolved key replays.
        async def drive() -> Block[ArtifactReceipt]:
            cache = self.warm
            done: list[ArtifactReceipt] = []
            for front in plan.fronts:
                block = await self.lane.drain(front, cache)
                for receipt in block:
                    cache = cache.add(receipt.slot, receipt)
                    done.append(receipt)
            return Block.of_seq(done)

        return await async_boundary("artifacts.issue", drive)


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "ArtifactIssue",
    "IssueRequest",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
