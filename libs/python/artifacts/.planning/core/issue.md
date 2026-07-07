# [PY_ARTIFACTS_CORE_ISSUE]

THE constructing owner of the artifact plane — the one composition root where producer nodes become a running pipeline. `core/plan#PLAN` is the engine and never the constructor; this page is where an external host or a sibling package asks for deliverables and the answer is built: `issue(IssueRequest)` discriminates the closed modality union, calls the terminal producers' `emit()` to collect their `ArtifactWork` node sets, folds them into `ArtifactPipeline.of(nodes, lane=, warm=, targets=)`, `.plan()`s the CPM fronts, drains each front through the runtime `LanePolicy.drain` (one `Block[Admit]` per front — the landed lane surface), and returns the terminal receipts. Every import points DOWNWARD — `delivery/transmittal#TRANSMITTAL` (s3 within-stratum), `composition/sheet#SHEET`, `document/emit#EMIT`, `document/egress#EGRESS`, `visualization/diagram/draw#DRAW` (s2), `core/plan#PLAN` (s1), the runtime lane — and nothing in artifacts imports this page: it stands at the top of the dependency cone.

`issue()` is a composition root, not a producer: it mints no receipt case and no content key of its own. `warm` is the elision seed — in-session receipts accumulate front by front so a later node whose key already resolved replays instead of re-rendering, and the durable cross-run fill is the host-wire seam (`Rasm.AppHost` fills `warm` from the Persistence artifact index; the in-session wiring is complete and the probe is real the moment the boundary exists). The three flagship terminals stay PRODUCERS: `sheet_set` folds the transmittal aggregate (member nodes plus ONE aggregate node whose `parents` are the member keys), `diagram_suite` folds `DiagramDraw.emit()` per kind, `document_package` folds the section → emit → egress chain targeted at the egress key, and `single` admits any lone node.

## [01]-[INDEX]

- [01]-[ISSUE]: the polymorphic construction root — `IssueRequest` the closed `sheet_set | diagram_suite | document_package | single` modality union, `ArtifactIssue(lane, warm)` the composition owner whose `issue()` builds `ArtifactPipeline.of(...)` from the terminal producers' `emit()` sets, `_nodes` the total request→(nodes, targets) match, and `_driven` the front-by-front lane drain accumulating warm receipts — invoked by hosts and siblings, imported by nothing in artifacts.

## [02]-[ISSUE]

- Owner: `ArtifactIssue` carries the runtime `LanePolicy` and the `warm` receipt seed; `issue(request)` is its ONE entry — request in, `RuntimeRail[Block[ArtifactReceipt]]` out. Construction lives here for every modality so no producer schedules siblings, no host learns pipeline internals, and adding a deliverable modality is one union case plus one `_nodes` arm.
- Cases: `IssueRequest` is the flagship discriminant — `sheet_set(TransmittalRecord)` resolves `Transmittal.of(record).emit()` (the member node set plus the aggregate; targets = the aggregate key so the plan scopes to the issue closure); `diagram_suite(tuple[DiagramLayout, ...])` resolves `DiagramDraw` per positioned layout, one node per `DiagramKind`, targets empty (every diagram is a deliverable); `document_package(tuple[Spec, ...])` resolves the section producers, threads their keys as parents into the `document/emit#EMIT` format nodes, and terminates at the `DocumentEgress` finishing node the targets name; `single(ArtifactWork)` admits any lone producer node unchanged.
- Auto: `_driven(plan)` folds the CPM fronts in order — each front drains through `LanePolicy.drain(front, cache)` and its `Ok` receipts fold into the accumulating cache keyed by `receipt.slot`, so within-run elision is exact node granularity; a front fault rails the whole issue (receipts are terminal, never partial). The `planned`-phase fact and the coverage verdicts (`dangling`/`cyclic`/`collided`/`untargeted`) are the plan's own; this page adds no parallel telemetry.
- Growth: a new modality is one `IssueRequest` case plus one `_nodes` arm; a new terminal producer joins an existing arm's fold; a host-side batching policy is a `LanePolicy` value; zero new surface on any producer.
- Boundary: no receipt case and no content key (a composition root produces nothing); no producer logic (terminals own their `emit()`/`_emit`); no scheduling algebra (`core/plan#PLAN` owns CPM, coverage, elision evidence); no drain internals (the runtime lane owns admission, retry, and the offload bound); no persistence (the warm fill is the host's). A second constructing surface anywhere in artifacts, a producer calling a sibling's `emit()`, and a host touching `ArtifactPipeline` directly are the deleted forms.

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
class IssueRequest:  # the flagship discriminant — one closed vocabulary, one arity owns all modalities
    tag: Literal["sheet_set", "diagram_suite", "document_package", "single"] = tag()
    sheet_set: TransmittalRecord = case()  # terminal = delivery/transmittal (aggregate + members)
    diagram_suite: tuple[DiagramLayout, ...] = case()  # terminal = visualization/diagram/draw, one node per kind
    document_package: tuple[Spec, ...] = case()  # terminal chain = section -> emit -> egress
    single: ArtifactWork = case()  # any lone producer node


# --- [COMPOSITION] ----------------------------------------------------------------------
@dataclass(frozen=True, slots=True, kw_only=True)
class ArtifactIssue:
    lane: LanePolicy
    warm: Map[ContentKey, ArtifactReceipt] = Map.empty()  # the cache seed — in-session receipts now; Persistence fills it cross-run

    async def issue(self, request: IssueRequest, /) -> RuntimeRail[Block[ArtifactReceipt]]:
        nodes, targets = self._nodes(request)  # total match: terminal.emit() -> Iterable[ArtifactWork]
        plan = await ArtifactPipeline.of(nodes, lane=self.lane, warm=self.warm, targets=targets).plan()
        return await self._driven(plan)  # folds the CPM fronts through the runtime LanePolicy.drain, front by front

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
        # front-by-front drain: each Ok receipt folds into the accumulating cache keyed by receipt.slot,
        # so a later keyed node whose key already resolved replays instead of re-rendering.
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

One request shape serves every consumer: the AppHost boundary issues a full ISO 19650 transmittal with `IssueRequest(sheet_set=record)` and receives the member receipts plus the aggregate; a portfolio run issues `diagram_suite` over the positioned layouts and re-issues re-render only the layouts whose input keys changed; a specification book issues `document_package` and the plan scopes to the egress closure, so an untouched section's format node elides on its warm key. The pipeline's coverage verdicts and the `planned`-phase fact remain the plan's; the drain, retry, and offload bounds remain the lane's; the producers remain producers — and because their node contracts mint keys over INPUT, the seed this owner threads is a real cache probe, in-session today and cross-run the moment the Persistence wire fills `warm`.
