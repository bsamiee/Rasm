# [PY_ARTIFACTS_DRAWING_DETAIL]

AEC detail management binds the callout, content-keyed `ezdxf` detail library, and cross-reference DAG between sheets. `Detail` owns the closed `Callout`/`CalloutBoundary`/`DetailSource` families and discriminates egress with `SymbolTarget`. Each boundary and leader lowers to a `drawsvg` named-layer group or equivalent `ezdxf` entities beside the reusable block store, preserving circle, rectangle, polygon, and section-line geometry on both targets. `drawing/symbol#SYMBOL` supplies bubble vocabulary, `drawing/regime#REGIME` supplies layer and scale codes, and `graphic/color/derive#DERIVE` supplies palette projection. Drawing-plane documentation remains separate from the `dotnet:Rasm.Bim` semantic model.

Block authoring and placement recover one identity from one registry. `DetailSource.embedded` owns captured standalone-DXF bytes, `DetailSource.referenced` owns an external path plus its captured `ContentKey`, and `DetailEntry.of` derives and verifies the entry key from that mode payload. `DetailLibrary.claims` is the primary citation fact; `entries` derives the deduplicated key-to-source block registry, and `by_ref` derives the citation-to-entry placement registry without discarding per-citation metadata. DXF block names derive from the entry key, while each placement retains its own `DETAIL_NO`/`SHEET`/`TITLE`/`SCALE` attributes. Cross-reference edges join the `(host, ordinal)` placement discriminant to the target citation. `rustworkx.PyDAG(check_cycle=True)` builds once per `resolved()` call; `DAGWouldCycle` preserves the rejected edge as evidence, `transitive_reduction` yields the minimal graph, `topological_generations` yields depth layers, and `node_link_json` produces the persisted audit wire. `ReferenceReport.severed` carries `cyclic`/`collided`/`dangling` coverage evidence without blocking render, while provider failures cross the runtime rail through `async_boundary`. `LanePolicy` offloads rendering, and `ArtifactWork` schedules the named layers.

## [01]-[INDEX]

- [02]-[DETAIL]: the `Detail` owner over the `Callout` family, the content-addressed `DetailLibrary` block store, and the `rustworkx` cross-reference `PyDAG`, dual-lowering over `SymbolTarget` to a `drawsvg` boundary/leader group or the `ezdxf` detail-library block store.

## [02]-[DETAIL]

- Owner: `Detail` holds `callouts`, the `DetailLibrary` store, the `Palette`, the runtime `lane: LanePolicy`, and the `SymbolTarget` value, discriminating on the `_ENGINES` dual-lowering table — never a per-target subtype. `Callout` is the one reference-bearing value; its `(host, ordinal)` is the structural discriminant keeping two identical-`target` callouts from distinct sheets DISTINCT edges. `DetailRef.cite()` is the printed `"3/A-501"` citation the DAG nodes and the library index both key on; `SymbolStyle` is the shared drawing-plane mark-style, never a parallel `DetailStyle`.
- Cases: `CalloutBoundary` admits nondegenerate `circle`/`rectangle`/`polygon`/`section_line` payloads, projects `anchor()` and `bounds()` through total matches, and lowers each case through paired `_boundary_svg`/`_dxf_boundary` arms. `CalloutKind` keys `_BUBBLE` to the `SymbolKind` projection. `DetailSource.embedded` owns captured bytes, while `DetailSource.referenced` owns the external path and captured fingerprint. `DetailFault` carries `dangling`/`cyclic`/`collided` coverage evidence through `ReferenceReport.severed` and the `malformed` persisted-wire case through `Detail.audit`; provider failures inside the render path remain runtime `BoundaryFault` values.
- Entry: `Detail.over` admits through the folder's one `@beartype(conf=INGRESS)` ingress and normalizes `Callout | Iterable[Callout]` by a structural `match` at the head — never a `batch` knob, and never a raise outside the `_FAULTS` class the boundary admits. `emit()` returns the schedulable named layers, while `layered()` projects the same `_crossed` hop into `LayerPlan`; `async_boundary` narrows the `_FAULTS` engine-raise tuple. `resolved()` is the one audit-and-query entry: it builds the graph ONCE and returns the `Resolved` value carrying the `ReferenceReport`, the citation index, and the live DAG whose `impact`/`depends`/`depth_layers` methods answer the `ancestors`/`descendants`/`bfs_layers` revision-impact and dependency queries without a rebuild apiece. `bubbles()` is the callout→`SymbolKind` projection; `Detail.audit` re-checks a persisted `node_link_json` wire through `parse_node_link_json` + `digraph_find_cycle`.
- Auto: `DetailEntry.of` derives and `__post_init__` verifies the single content identity from the source payload. `DetailLibrary.claims` retains every placement claim; `entries` and `by_ref` derive the content and citation indexes — `by_ref` resolving repeated citations to the first claim deterministically — while `_collision` surfaces designators whose claims diverge in key, title, or scale. `_graph` mutates only the native `PyDAG`; `_resolve` derives stable order, generation widths, longest path, reduced edge count, and persisted wire from that graph. `_block_name` derives from the same key used by authoring and placement. `_bbox` unions true boundary, leader, and marker extents, so SVG geometry remains inside its canvas and stands as the DXF arm's fallback span under `drawing/standard#STANDARD` `extent`. Each SVG layer row carries its `LayerName` through `LayerNode.Annotation`'s `aec` for `LayerSchema.ISO13567` composition.
- Growth: a new boundary adds one `CalloutBoundary` case and corresponding `anchor()`/`bounds()`/SVG/DXF arms; a new callout kind adds one `CalloutKind` value and `_BUBBLE` row; a new source mode adds one `DetailSource` case and `_author_detail` arm; a new egress adds one `_ENGINES` row; a new graph query adds one `Resolved` operation; a new coverage cause adds one `DetailFault` case, report field, and `severed` arm.
- Boundary: no dimension, annotation, or sheet-placement logic — `drawing/dimension#DIMENSION`, `drawing/annotate#ANNOTATE`, `composition/sheet#SHEET`. `ezdxf` owns the detail-library block store, `rustworkx` the cross-reference DAG, `drawsvg` the named-layer boundary/leader container, `drawing/symbol#SYMBOL` the bubble geometry, `drawing/regime#REGIME` the ISO layer/scale codes and `drawing/standard#STANDARD` the discipline-pen DXF lowering, `graphic/color/derive#DERIVE` the palette, `specification/classify#CODE` the classification tables, and `dotnet:Rasm.Bim` the IFC; identity minting is the runtime's.
- Packages: `ezdxf` owns reusable blocks, `Importer.import_modelspace(target_layout=)`, `xref.attach`, and `recover.read` (the binary-stream loader salvaging non-conforming captures), its measured layout span arriving through `drawing/standard#STANDARD` `extent`; `rustworkx` owns `PyDAG`, cycle admission, traversal, stable ordering, reduction, and node-link wires; `drawsvg` owns structured SVG geometry; `expression` owns `Option`, `Block`, and `Map`; drawing owners supply layer, standard, symbol, scale, and palette vocabularies.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
import io
import math
from builtins import frozendict
from collections.abc import Callable, Iterable
from enum import StrEnum
from typing import Final, Literal, Self, assert_never

from beartype import beartype
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, msgpack

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.metrics import Metrics
from rasm.runtime.workers import Kernel, KernelTrait
from rasm.runtime.faults import TRANSIENT, FaultRow, RuntimeRail, async_boundary, rostered

from rasm.artifacts.core.hooks import BYTE_VOLUME, DOMAIN, ArtifactsLeg
from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.drawing.regime import INGRESS, Discipline, LayerName, LayerSchema, ScaleRatio
from rasm.artifacts.drawing.standard import Standard
from rasm.artifacts.drawing.symbol import SymbolKind, SymbolStyle, SymbolTarget
from rasm.artifacts.graphic.layer import LayerNode, LayerPlan
from rasm.artifacts.graphic.color.derive import Palette, hex_ramp

lazy import drawsvg
lazy import ezdxf
lazy from ezdxf import xref
lazy from ezdxf import recover as ezdxf_recover
lazy from ezdxf.addons.importer import Importer
lazy from rustworkx import DAGWouldCycle, PyDAG, ancestors, bfs_layers, dag_longest_path
lazy from rustworkx import descendants, digraph_find_cycle, is_directed_acyclic_graph
lazy from rustworkx import lexicographical_topological_sort, node_link_json, parse_node_link_json
lazy from rustworkx import topological_generations, transitive_reduction

# --- [TYPES] ----------------------------------------------------------------------------
type Point = tuple[float, float]
type Box = tuple[float, float, float, float]
type Ramp = tuple[str, ...]
type Engine = Callable[["Detail"], tuple[LayerNode, ...]]

_RADIUS: float = 6.0
_FAULTS: tuple[type[Exception], ...] = (ValueError, OSError)
_CANON: Final = msgpack.Encoder(order="deterministic")


class CalloutKind(StrEnum):
    DETAIL = "detail"
    SECTION = "section"
    ELEVATION = "elevation"
    ENLARGEMENT = "enlargement"
    WALL_SECTION = "wall_section"
    BLOWUP = "blowup"


# --- [TABLES] ---------------------------------------------------------------------------

DETAIL_CROSS: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.DETAIL, point="cross", arm="boundary", defect="detail-fold", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[ArtifactsLeg]]] = rostered(Block.of_seq([DETAIL_CROSS]))

# --- [MODELS] ---------------------------------------------------------------------------
class DetailRef(Struct, frozen=True):
    designator: str
    sheet: str
    discipline: Discipline = Discipline.ARCHITECTURAL

    def __post_init__(self) -> None:
        if not self.designator.strip() or not self.sheet.strip():
            raise ValueError("detail reference requires designator and sheet")

    def cite(self) -> str:
        return f"{self.designator}/{self.sheet}"


@tagged_union(frozen=True)
class CalloutBoundary:
    tag: Literal["circle", "rectangle", "polygon", "section_line"] = tag()
    circle: tuple[Point, float] = case()
    rectangle: tuple[Point, Point] = case()
    polygon: tuple[Point, ...] = case()
    section_line: tuple[tuple[Point, ...], float] = case()

    def __post_init__(self) -> None:
        match self:
            case CalloutBoundary(tag="circle", circle=(_, radius)) if radius <= 0.0:
                raise ValueError("circle radius must be positive")
            case CalloutBoundary(tag="rectangle", rectangle=((x0, y0), (x1, y1))) if x0 == x1 or y0 == y1:
                raise ValueError("rectangle requires positive area")
            case CalloutBoundary(tag="polygon", polygon=verts) if len(verts) < 3 or abs(self._area(verts)) <= 1e-9:
                raise ValueError("polygon requires three non-collinear vertices")
            case CalloutBoundary(tag="section_line", section_line=(verts, _)) if len(verts) < 2 or len(set(verts)) < 2 or verts[0] == verts[-1]:
                raise ValueError("section line requires two distinct vertices")
            case CalloutBoundary():
                return
            case _ as unreachable:
                assert_never(unreachable)

    @staticmethod
    def _area(verts: tuple[Point, ...], /) -> float:
        return sum(x0 * y1 - x1 * y0 for (x0, y0), (x1, y1) in zip(verts, verts[1:] + verts[:1], strict=True)) / 2.0

    def anchor(self) -> Point:
        match self:
            case CalloutBoundary(tag="circle", circle=(center, _)):
                return center
            case CalloutBoundary(tag="rectangle", rectangle=((x0, y0), (x1, y1))):
                return ((x0 + x1) / 2.0, (y0 + y1) / 2.0)
            case CalloutBoundary(tag="polygon", polygon=verts):
                area = self._area(verts)
                factor = 1.0 / (6.0 * area)
                pairs = tuple(zip(verts, verts[1:] + verts[:1], strict=True))
                return (
                    factor * sum((x0 + x1) * (x0 * y1 - x1 * y0) for (x0, y0), (x1, y1) in pairs),
                    factor * sum((y0 + y1) * (x0 * y1 - x1 * y0) for (x0, y0), (x1, y1) in pairs),
                )
            case CalloutBoundary(tag="section_line", section_line=(verts, _)):
                start, stop = verts[0], verts[-1]
                return ((start[0] + stop[0]) / 2.0, (start[1] + stop[1]) / 2.0)
            case _ as unreachable:
                assert_never(unreachable)

    def bounds(self) -> Box:
        match self:
            case CalloutBoundary(tag="circle", circle=((x, y), radius)):
                return (x - radius, y - radius, x + radius, y + radius)
            case CalloutBoundary(tag="rectangle", rectangle=((x0, y0), (x1, y1))):
                return (min(x0, x1), min(y0, y1), max(x0, x1), max(y0, y1))
            case CalloutBoundary(tag="polygon", polygon=verts):
                return (min(x for x, _ in verts), min(y for _, y in verts), max(x for x, _ in verts), max(y for _, y in verts))
            case CalloutBoundary(tag="section_line", section_line=(verts, bearing)):
                dx, dy = math.cos(math.radians(bearing)) * _RADIUS, math.sin(math.radians(bearing)) * _RADIUS
                points = verts + ((verts[0][0] + dx, verts[0][1] + dy), (verts[-1][0] + dx, verts[-1][1] + dy))
                return (min(x for x, _ in points), min(y for _, y in points), max(x for x, _ in points), max(y for _, y in points))
            case _ as unreachable:
                assert_never(unreachable)


class Callout(Struct, frozen=True):
    host: str
    ordinal: int
    target: DetailRef
    anchor: Point
    kind: CalloutKind
    boundary: CalloutBoundary
    style: SymbolStyle

    def __post_init__(self) -> None:
        if not self.host.strip() or self.ordinal < 0:
            raise ValueError("callout requires a host and nonnegative ordinal")


@tagged_union(frozen=True)
class DetailSource:
    tag: Literal["embedded", "referenced"] = tag()
    embedded: bytes = case()
    referenced: tuple[str, ContentKey] = case()

    def __post_init__(self) -> None:
        match self:
            case DetailSource(tag="embedded", embedded=body) if not body:
                raise ValueError("embedded detail requires captured bytes")
            case DetailSource(tag="referenced", referenced=(path, _)) if not path.strip():
                raise ValueError("referenced detail requires a path")
            case DetailSource():
                return
            case _ as unreachable:
                assert_never(unreachable)


class DetailEntry(Struct, frozen=True):
    ref: DetailRef
    title: str
    scale: ScaleRatio
    source: DetailSource
    key: ContentKey
    classification: Option[str] = Nothing
    keynotes: tuple[str, ...] = ()
    revision: Option[str] = Nothing

    def __post_init__(self) -> None:
        match self.classification, self.revision:
            case _, _ if not self.title.strip():
                raise ValueError("detail title must not be empty")
            case (Option(tag="some", some=value), _) | (_, Option(tag="some", some=value)) if not value.strip():
                raise ValueError("optional detail metadata must not contain an empty string")
            case _ if self.key != self.source_key(self.source):
                raise ValueError("detail key must derive from its source payload")
            case _:
                return

    @staticmethod
    def source_key(source: DetailSource, /) -> ContentKey:
        match source:
            case DetailSource(tag="embedded", embedded=block):
                return ContentIdentity.key("drawing-detail-block", block)
            case DetailSource(tag="referenced", referenced=(path, fingerprint)):
                return ContentIdentity.key("drawing-detail-xref", _CANON.encode((path, fingerprint)))
            case _ as unreachable:
                assert_never(unreachable)

    @classmethod
    def of(
        cls,
        ref: DetailRef,
        title: str,
        scale: ScaleRatio,
        source: bytes | tuple[str, ContentKey],
        /,
        *,
        classification: Option[str] = Nothing,
        keynotes: tuple[str, ...] = (),
        revision: Option[str] = Nothing,
    ) -> Self:
        admitted = DetailSource(embedded=source) if isinstance(source, bytes) else DetailSource(referenced=source)
        return cls(
            ref=ref,
            title=title,
            scale=scale,
            source=admitted,
            key=cls.source_key(admitted),
            classification=classification,
            keynotes=keynotes,
            revision=revision,
        )


class DetailLibrary(Struct, frozen=True):
    claims: tuple[DetailEntry, ...] = ()

    @classmethod
    def of(cls, entries: Iterable[DetailEntry] = (), /) -> Self:
        return cls(claims=tuple(entries))

    @property
    def entries(self) -> Map[ContentKey, DetailSource]:
        return Map.of_seq((entry.key, entry.source) for entry in self.claims)

    @property
    def by_ref(self) -> Map[str, DetailEntry]:
        return Block.of_seq(self.claims).fold(
            lambda held, entry: held if held.contains_key(entry.ref.cite()) else held.add(entry.ref.cite(), entry),
            Map.empty(),
        )

    def resolve(self, ref: DetailRef, /) -> Option[DetailEntry]:
        return self.by_ref.try_find(ref.cite())


class ReferenceReport(Struct, frozen=True):
    order: tuple[str, ...] = ()
    generations: tuple[int, ...] = ()
    depth: int = 0
    edges: int = 0
    reduced: int = 0
    dangling: tuple[str, ...] = ()
    cyclic: tuple[tuple[str, str], ...] = ()
    collided: tuple[str, ...] = ()
    wire: bytes = b""

    @property
    def severed(self) -> "Option[DetailFault]":
        match self.cyclic, self.collided, self.dangling:
            case cyclic, _, _ if cyclic:
                return Some(DetailFault(cyclic=cyclic))
            case _, collided, _ if collided:
                return Some(DetailFault(collided=collided))
            case _, _, dangling if dangling:
                return Some(DetailFault(dangling=dangling))
            case _:
                return Nothing


class Resolved(Struct, frozen=True):
    report: ReferenceReport
    index: frozendict[str, int]
    reverse: frozendict[int, str]
    graph: "PyDAG"

    def impact(self, ref: DetailRef, /) -> frozenset[str]:
        cite = ref.cite()
        return frozenset(self.reverse[node] for node in ancestors(self.graph, self.index[cite])) if cite in self.index else frozenset()

    def depends(self, ref: DetailRef, /) -> frozenset[str]:
        cite = ref.cite()
        return frozenset(self.reverse[node] for node in descendants(self.graph, self.index[cite])) if cite in self.index else frozenset()

    def depth_layers(self, ref: DetailRef, /) -> tuple[tuple[str, ...], ...]:
        cite = ref.cite()
        if cite not in self.index:
            return ()
        return tuple(tuple(self.reverse[node] for node in layer) for layer in bfs_layers(self.graph, [self.index[cite]]))


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class DetailFault:
    tag: Literal["dangling", "cyclic", "collided", "malformed"] = tag()
    dangling: tuple[str, ...] = case()
    cyclic: tuple[tuple[str, str], ...] = case()
    collided: tuple[str, ...] = case()
    malformed: str = case()


# --- [SERVICES] -------------------------------------------------------------------------
class Detail(Struct, frozen=True):
    callouts: tuple[Callout, ...]
    library: DetailLibrary
    palette: Palette
    lane: LanePolicy
    target: SymbolTarget = SymbolTarget.SVG

    @classmethod
    @beartype(conf=INGRESS)
    def over(
        cls,
        callouts: Callout | Iterable[Callout],
        library: DetailLibrary,
        palette: Palette,
        lane: LanePolicy,
        /,
        *,
        target: SymbolTarget = SymbolTarget.SVG,
    ) -> Self:
        match callouts:
            case Callout():
                return cls(callouts=(callouts,), library=library, palette=palette, lane=lane, target=target)
            case _:
                return cls(callouts=tuple(callouts), library=library, palette=palette, lane=lane, target=target)

    def bubbles(self) -> tuple[SymbolKind, ...]:
        return tuple(_BUBBLE[callout.kind](callout) for callout in self.callouts)

    def resolved(self) -> Resolved:
        return _resolve(self)

    @staticmethod
    def audit(wire: bytes, /) -> Option[DetailFault]:
        try:
            graph = parse_node_link_json(
                wire.decode(), node_attrs=lambda data: data["cite"], edge_attrs=lambda data: (data["host"], int(data["ordinal"]))
            )
        except (ValueError, KeyError, TypeError) as torn:
            return Some(DetailFault(malformed=type(torn).__name__))
        return (
            Nothing if is_directed_acyclic_graph(graph) else Some(DetailFault(cyclic=tuple((graph[a], graph[b]) for a, b in digraph_find_cycle(graph))))
        )

    def emit(self, /) -> ArtifactWork[tuple[LayerNode, ...]]:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=float(len(self.callouts)))

    @property
    def _key(self) -> ContentKey:
        claims = tuple((entry.ref.cite(), entry.title, entry.scale.value) for entry in self.library.claims)
        spec = ContentIdentity.key(f"drawing-detail-{self.target}", _CANON.encode((self.callouts, self.palette, self.target, claims)))
        return ContentIdentity.key(f"drawing-detail-{self.target}", (spec, *(entry.key for entry in self.library.claims)))

    async def _emit(self) -> RuntimeRail[tuple[LayerNode, ...]]:
        settled = await async_boundary(DETAIL_CROSS, self._crossed, catch=_FAULTS)
        match settled:
            case Result(tag="ok", ok=layers):
                size = sum(len(node.leaf[1].fragment) for node in layers if node.tag == "leaf" and node.leaf[1].tag == "fragment")
                Metrics.record({BYTE_VOLUME: float(size)}, domain=DOMAIN, kind="drawing", scope=self.lane.scope)
                return Ok(layers)
            case refused:
                return Error(refused.error)

    async def layered(self) -> RuntimeRail[LayerPlan]:
        return (await async_boundary(DETAIL_CROSS, self._crossed, catch=_FAULTS)).map(
            lambda layers: LayerPlan(schema=LayerSchema.ISO13567, roots=layers)
        )

    async def _crossed(self) -> tuple[LayerNode, ...]:
        crossed = await self.lane.offload(Kernel.of(_ENGINES[self.target], KernelTrait.RELEASING), self)
        return crossed.default_with(self._raise)

    @staticmethod
    def _raise(fault: object) -> tuple[LayerNode, ...]:
        raise ValueError(str(fault))


# --- [OPERATIONS] -----------------------------------------------------------------------
def _graph(detail: Detail) -> tuple["PyDAG", dict[str, int], tuple[tuple[str, str], ...], tuple[str, ...]]:
    dag = PyDAG(check_cycle=True)
    index: dict[str, int] = {}
    dangling: list[str] = []
    cyclic: list[tuple[str, str]] = []

    def node(cite: str) -> int:
        if (found := index.get(cite)) is None:
            found = dag.add_node(cite)
            index[cite] = found
        return found

    for callout in detail.callouts:
        target = callout.target.cite()
        if detail.library.resolve(callout.target).is_none():
            dangling.append(target)
            continue
        try:
            dag.add_edge(node(callout.host), node(target), (callout.host, callout.ordinal))
        except DAGWouldCycle:
            cyclic.append((callout.host, target))
    return dag, index, tuple(cyclic), tuple(dangling)


def _collision(library: DetailLibrary) -> tuple[str, ...]:
    claims = Block.of_seq(library.claims).fold(
        lambda held, entry: held.change(
            entry.ref.cite(), lambda current: Some(current.default_value(frozenset()) | {(entry.key, entry.title, entry.scale)})
        ),
        Map.empty(),
    )
    return tuple(cite for cite, identities in claims.items() if len(identities) > 1)


def _resolve(detail: Detail) -> Resolved:
    dag, index, cyclic, dangling = _graph(detail)
    reduced, _ = transitive_reduction(dag)
    wire = (node_link_json(dag, node_attrs=_wire_node, edge_attrs=_wire_edge) or "").encode()
    report = ReferenceReport(
        order=tuple(lexicographical_topological_sort(dag, key=lambda cite: cite)),
        generations=tuple(len(generation) for generation in topological_generations(dag)),
        depth=len(dag_longest_path(dag)),
        edges=dag.num_edges(),
        reduced=reduced.num_edges(),
        dangling=dangling,
        cyclic=cyclic,
        collided=_collision(detail.library),
        wire=wire,
    )
    return Resolved(report=report, index=frozendict(index), reverse=frozendict({node: cite for cite, node in index.items()}), graph=dag)


def _wire_node(cite: str) -> dict[str, str]:
    return {"cite": cite}


def _wire_edge(ref: tuple[str, int]) -> dict[str, str]:
    return {"host": ref[0], "ordinal": str(ref[1])}


def _bbox(detail: Detail) -> Box:
    boxes = tuple(
        (
            min(bounds[0], callout.anchor[0]) - padding,
            min(bounds[1], callout.anchor[1]) - padding,
            max(bounds[2], callout.anchor[0]) + padding,
            max(bounds[3], callout.anchor[1]) + padding,
        )
        for callout in detail.callouts
        for bounds in (callout.boundary.bounds(),)
        for padding in (max(0.5, callout.style.weight.mm / 2.0),)
    )
    return (
        (min(box[0] for box in boxes), min(box[1] for box in boxes), max(box[2] for box in boxes), max(box[3] for box in boxes))
        if boxes
        else (0.0, 0.0, 0.0, 0.0)
    )


def _boundary_svg(callout: Callout, ramp: Ramp) -> "drawsvg.Group":
    stroke, width = ramp[callout.style.stroke % len(ramp)], callout.style.weight.mm
    group = drawsvg.Group()
    match callout.boundary:
        case CalloutBoundary(tag="circle", circle=(center, radius)):
            group.append(drawsvg.Circle(center[0], center[1], radius, fill="none", stroke=stroke, stroke_width=width))
        case CalloutBoundary(tag="rectangle", rectangle=((x0, y0), (x1, y1))):
            group.append(drawsvg.Rectangle(min(x0, x1), min(y0, y1), abs(x1 - x0), abs(y1 - y0), fill="none", stroke=stroke, stroke_width=width))
        case CalloutBoundary(tag="polygon", polygon=verts):
            group.append(drawsvg.Lines(*(coord for point in verts for coord in point), close=True, fill="none", stroke=stroke, stroke_width=width))
        case CalloutBoundary(tag="section_line", section_line=(verts, bearing)):
            group.append(drawsvg.Lines(*(coord for point in verts for coord in point), close=False, fill="none", stroke=stroke, stroke_width=width))
            dx, dy = math.cos(math.radians(bearing)) * _RADIUS, math.sin(math.radians(bearing)) * _RADIUS
            for x, y in (verts[0], verts[-1]):
                group.append(drawsvg.Line(x, y, x + dx, y + dy, stroke=stroke, stroke_width=width))
        case _ as unreachable:
            assert_never(unreachable)
    origin = callout.boundary.anchor()
    group.append(drawsvg.Line(origin[0], origin[1], callout.anchor[0], callout.anchor[1], stroke=stroke, stroke_width=width))
    return group


def _layer_svg(name: str, groups: tuple["drawsvg.Group", ...], box: Box) -> bytes:
    canvas = drawsvg.Drawing(box[2] - box[0], box[3] - box[1], origin=(box[0], box[1]))
    layer = drawsvg.Group(id=name)
    for group in groups:
        layer.append(group)
    canvas.append(layer)
    return canvas.as_svg().encode()


def _dxf_boundary(msp: object, callout: Callout, attribs: dict[str, object]) -> None:
    match callout.boundary:
        case CalloutBoundary(tag="circle", circle=(center, radius)):
            msp.add_circle(center, radius, dxfattribs=attribs)
        case CalloutBoundary(tag="rectangle", rectangle=((x0, y0), (x1, y1))):
            msp.add_lwpolyline(((x0, y0), (x1, y0), (x1, y1), (x0, y1)), close=True, dxfattribs=attribs)
        case CalloutBoundary(tag="polygon", polygon=verts):
            msp.add_lwpolyline(verts, close=True, dxfattribs=attribs)
        case CalloutBoundary(tag="section_line", section_line=(verts, bearing)):
            msp.add_lwpolyline(verts, close=False, dxfattribs=attribs)
            dx, dy = math.cos(math.radians(bearing)) * _RADIUS, math.sin(math.radians(bearing)) * _RADIUS
            for x, y in (verts[0], verts[-1]):
                msp.add_line((x, y), (x + dx, y + dy), dxfattribs=attribs)
        case _ as unreachable:
            assert_never(unreachable)
    msp.add_line(callout.boundary.anchor(), callout.anchor, dxfattribs=attribs)


_ATTDEFS: tuple[tuple[str, Point], ...] = (
    ("DETAIL_NO", (0.0, 0.0)),
    ("SHEET", (0.0, -1.0)),
    ("TITLE", (0.0, -2.0)),
    ("SCALE", (0.0, -3.0)),
)


def _block_name(key: ContentKey, /) -> str:
    return f"DTL_{key.hex.replace(':', '_')}"


def _author_detail(doc: object, key: ContentKey, source: DetailSource) -> None:
    name = _block_name(key)
    match source:
        case DetailSource(tag="referenced", referenced=(path, _)):
            xref.attach(doc, block_name=name, filename=path)
            attached = doc.blocks.get(name)
            present = {attdef.dxf.tag for attdef in attached.query("ATTDEF")}
            for attdef, offset in _ATTDEFS:
                if attdef not in present:
                    attached.add_attdef(attdef, offset)
        case DetailSource(tag="embedded", embedded=body):
            block = doc.blocks.new(name)
            for attdef, offset in _ATTDEFS:
                block.add_attdef(attdef, offset)
            recovered, _auditor = ezdxf_recover.read(io.BytesIO(body))
            reconstruct = Importer(recovered, doc)
            reconstruct.import_modelspace(block)
            reconstruct.finalize()
        case _ as unreachable:
            assert_never(unreachable)


# --- [TABLES] ---------------------------------------------------------------------------
def _svg_engine(detail: Detail) -> tuple[LayerNode, ...]:
    ramp, box = hex_ramp(detail.palette), _bbox(detail)

    def bucket(acc: Map[str, tuple[LayerName, tuple["drawsvg.Group", ...]]], callout: Callout, /) -> Map[str, tuple[LayerName, tuple["drawsvg.Group", ...]]]:
        layer = callout.style.layer
        return acc.change(
            layer.compose(), lambda held: Some((layer, (*held.map(lambda pair: pair[1]).default_value(()), _boundary_svg(callout, ramp))))
        )

    grouped = Block.of_seq(detail.callouts).fold(bucket, Map.empty())
    composed = tuple((layer, _layer_svg(name, items, box)) for name, (layer, items) in grouped.items())
    layers = tuple(LayerNode.Annotation(layer.compose(), source, aec=Some(layer)) for layer, source in composed)
    return layers


def _dxf_engine(detail: Detail) -> tuple[LayerNode, ...]:
    doc, std = ezdxf.new("R2018", setup=True), Standard.of()
    std.seed(doc, layers=tuple(sorted({callout.style.layer for callout in detail.callouts}, key=LayerName.compose)))
    msp = doc.modelspace()
    for key, source in detail.library.entries.items():
        _author_detail(doc, key, source)
    for callout in detail.callouts:
        attribs = std.graphics(callout.style.layer).asdict()
        match detail.library.resolve(callout.target):
            case Option(tag="some", some=held):
                msp.add_auto_blockref(
                    _block_name(held.key),
                    callout.anchor,
                    {
                        "DETAIL_NO": callout.target.designator,
                        "SHEET": callout.target.sheet,
                        "TITLE": held.title,
                        "SCALE": held.scale.value,
                    },
                    dxfattribs=attribs,
                )
            case Option(tag="none"):
                pass
            case _ as unreachable:
                assert_never(unreachable)
        _dxf_boundary(msp, callout, attribs)
    stream = io.StringIO()
    doc.write(stream)
    data = stream.getvalue().encode()
    return (LayerNode.Annotation("dxf", data),)


_BUBBLE: frozendict[CalloutKind, Callable[[Callout], SymbolKind]] = frozendict({
    CalloutKind.DETAIL: lambda c: SymbolKind.Detail(c.anchor, _RADIUS, c.target.designator, c.target.sheet, c.style),
    CalloutKind.SECTION: lambda c: SymbolKind.Section(c.anchor, _RADIUS, c.target.designator, c.target.sheet, 0.0, c.style),
    CalloutKind.ELEVATION: lambda c: SymbolKind.Elevation(c.anchor, _RADIUS, c.target.designator, c.target.sheet, 0.0, c.style),
    CalloutKind.ENLARGEMENT: lambda c: SymbolKind.Detail(c.anchor, _RADIUS, c.target.designator, c.target.sheet, c.style),
    CalloutKind.WALL_SECTION: lambda c: SymbolKind.Section(c.anchor, _RADIUS, c.target.designator, c.target.sheet, 90.0, c.style),
    CalloutKind.BLOWUP: lambda c: SymbolKind.Detail(c.anchor, _RADIUS, c.target.designator, c.target.sheet, c.style),
})

_ENGINES: frozendict[SymbolTarget, Engine] = frozendict({SymbolTarget.SVG: _svg_engine, SymbolTarget.DXF: _dxf_engine})


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "Callout",
    "CalloutBoundary",
    "CalloutKind",
    "Detail",
    "DetailEntry",
    "DetailFault",
    "DetailLibrary",
    "DetailRef",
    "DetailSource",
    "ReferenceReport",
    "Resolved",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
