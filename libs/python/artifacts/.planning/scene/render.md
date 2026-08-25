# [PY_ARTIFACTS_SCENE_RENDER]

`Scene3d` renders admitted datasets through `VTK` to a host-free offscreen image and emits the `rgb24` `Frames` sequence `Media.of(frames)` encodes through `av.VideoFrame.from_ndarray`. `SceneOp` is one closed-payload `expression.tagged_union` over the `Image`/`Export`/`Frames`/`Ingest`/`Compose` modalities, each carrying its typed payload and returning `RuntimeRail[ArtifactReceipt]` through one total `match`. Offscreen rendering requires no display or browser.

Every payload arrives settled from `scene/spec#SPEC` — `SceneGrid` admission evidence, `RenderSpec`, `OrbitPath`, the target and source vocabularies — and every body executes in `scene/render_worker#WORKER`: each arm crosses as one `HOSTILE`-trait runtime `Kernel` named against the spec floor's `WORKER_MODULE`, so this runtime module imports the spec floor alone and never a worker module, while isolation, band, worker-death retry, and the crossing gate all derive at `runtime/execution/workers#CROSSING`. Every kernel declares `idempotent=True` explicitly — a render is content-keyed and run-scoped, so a worker-death re-run is safe by declaration, never by assumption — and the frames and compose arms declare `Enforcement.TERMINAL`: a hung native orbit capture and a boolean fold spinning on coincident surfaces obey only the pebble wall-clock kill. Lane policy arrives projected from the caller's admitted context through `LanePolicy.of`; a capacity literal has no owner here. `SceneGrid` wraps its buffers inside a struct, so the crossing stays `Wire.PICKLE` — the shared-memory span channel crosses bare ndarray arguments alone. `glb` carries geometry-plane bytes and `parents` carries its producer key as a data edge per `core/plan#PLAN`.

## [01]-[INDEX]

- [02]-[SCENE]: the one `Scene3d` owner over the closed-payload `SceneOp` family, every arm folding into one `RuntimeRail[ArtifactReceipt]` through the offloaded worker kernel, plus the `framed()` rgb24 egress the media plane composes.

## [02]-[SCENE]

- Owner: `Scene3d` discriminates modality over the closed `SceneOp` family; every case carries its own typed payload — a `SceneGrid` admitted owner, never an erased `object` the worker discovers the shape of. Binary CSG and sampling ride the dedicated two-operand `Compose` modality because `FieldFilter.apply` has one fielded operand.
- Cases: `Frames` is one arm the rotating-scene and chart-over-time sources share; its `rgb24` rasters cross to `media/container#CONTAINER` through `framed()` without a file round-trip, and a non-frames op refuses the egress at the boundary. `Image` is the raster fast path minting the `_sized` dims band; `Export` at the same `PNG` target rides the `ExportRow` law and threads dataset facts — one target, two evidence bands. `Ingest` re-admits an existing scene through the worker importer, applies `RenderSpec.viewed`, and re-serializes through `render_ingest`. `Compose` folds two grids through the worker's boolean-CSG or field-sample table under the terminal arm — the worker refuses a non-manifold operand, yet a watertight fold can still spin on coincident surfaces, so the kill budget bounds it where a cooperative cancel cannot.
- Auto: `_canon` lowers each arm onto `scene/spec#SPEC`'s `framed`/`CANON` identity-preimage discipline — `SceneGrid.spans` shape-plus-buffer chunks beside one deterministic-msgpack spec chunk — so `_key` mints through the bare `ContentIdentity.key` and merkle-folds `parents` when present.
- Receipt: every arm mints `ArtifactReceipt.Scene(key, target, bytes, facts)` where `key` is the node's input key (`receipt.slot == node.key`) and the produced payload's content address rides `facts.address`. `Export` adds the staged dataset's `points` and `cells` counts to that band, and USD targets merge stage evidence without a parallel receipt case. `_emit` awaits `Journal.record` over `receipt.evidence()` ONCE above the render fan — one `OPERATIONAL` fact and its `STORAGE` charge for whichever arm ran, so a sixth arm inherits the seat instead of needing its own — and the band never reaches that diff; the seat is that awaitable fold, because recording suspends where `contribute` cannot.
- Growth: a new refusal is one `RAISES` row under an `ArtifactsLeg.RENDER` anchor; a new modality is one `SceneOp` case plus one `_rendered` arm, one `_canon` arm, and one worker kernel name; a new render-evidence fact is one `ArtifactReceipt.Scene.facts` key. `SceneOp` remains the single modality owner.
- Boundary: `_emit` runs the arm under `async_boundary` anchored on the `SCENE_RENDER` row and flattens the boundary-faulted offload rail exactly once, so the composed signature stays one `RuntimeRail` and a worker raise lands as that row's fault, never a custom exception re-crossed inward. The frames egress refuses a non-frames op by RETURNING `SCENE_EGRESS`, never by raising into a fence that would convert it back.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from typing import Final, Literal, assert_never

from beartype.roar import BeartypeCallHintViolation
from builtins import frozendict
from expression import Error, Result, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct

from rasm.runtime.faults import TERMINAL, TRANSIENT, Catch, FaultRow, RuntimeRail, async_boundary, rostered
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.journal import Journal
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Enforcement, Kernel, KernelTrait

from rasm.artifacts.core.hooks import ArtifactsLeg
from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.scene.spec import BoolOp, CANON, Frames, OrbitPath, RenderSpec, SceneGrid, SceneSource, SceneTarget, WORKER_MODULE, framed

# --- [TYPES] ----------------------------------------------------------------------------

type SceneOpTag = Literal["image", "export", "frames", "ingest", "compose"]

# --- [CONSTANTS] ------------------------------------------------------------------------

_FRAME_FORMAT = "rgb24"

_RESIDUE: Final[Catch] = (BeartypeCallHintViolation, ValueError, OSError)

# --- [TABLES] ---------------------------------------------------------------------------

SCENE_EGRESS: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.RENDER, point="egress", arm="config", defect="not-a-frames-op", retriability=TERMINAL, slots=("modality",)
)
SCENE_RENDER: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.RENDER, point="render", arm="boundary", defect="render-fold", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[ArtifactsLeg]]] = rostered(Block.of_seq([SCENE_EGRESS, SCENE_RENDER]))

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class SceneOp:
    tag: SceneOpTag = tag()
    image: tuple[SceneGrid, RenderSpec] = case()
    export: tuple[SceneGrid, SceneTarget, RenderSpec] = case()
    frames: tuple[SceneGrid, OrbitPath, RenderSpec] = case()
    ingest: tuple[bytes, SceneSource, SceneTarget, RenderSpec] = case()
    compose: tuple[SceneGrid, SceneGrid, BoolOp, RenderSpec] = case()

    @staticmethod
    def Image(grid: SceneGrid, spec: RenderSpec) -> "SceneOp":
        return SceneOp(image=(grid, spec))

    @staticmethod
    def Export(grid: SceneGrid, target: SceneTarget, spec: RenderSpec) -> "SceneOp":
        return SceneOp(export=(grid, target, spec))

    @staticmethod
    def Frames(grid: SceneGrid, orbit: OrbitPath, spec: RenderSpec) -> "SceneOp":
        return SceneOp(frames=(grid, orbit, spec))

    @staticmethod
    def Ingest(scene: bytes, source: SceneSource, target: SceneTarget, spec: RenderSpec) -> "SceneOp":
        return SceneOp(ingest=(scene, source, target, spec))

    @staticmethod
    def Compose(grid_a: SceneGrid, grid_b: SceneGrid, op: BoolOp, spec: RenderSpec) -> "SceneOp":
        return SceneOp(compose=(grid_a, grid_b, op, spec))


class Scene3d(Struct, frozen=True):
    op: SceneOp
    lane: LanePolicy
    parents: tuple[ContentKey, ...] = ()

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=self.parents, admission=Admission(keyed=None), cost=4.0)

    async def framed(self) -> RuntimeRail[Frames]:
        match self.op:
            case SceneOp(tag="frames", frames=(grid, orbit, spec)):
                return await self._offload("render_frames", grid, orbit, spec, enforcement=Enforcement.TERMINAL)
            case _:
                return Error(SCENE_EGRESS.raised(self.op.tag))

    @property
    def _key(self) -> ContentKey:
        minted = ContentIdentity.key(f"scene-{self.op.tag}", _canon(self.op))
        return minted if not self.parents else ContentIdentity.key(f"scene-{self.op.tag}", (minted, *self.parents))

    async def _offload[T](self, kernel: str, /, *args: object, enforcement: Enforcement = Enforcement.COOPERATIVE) -> RuntimeRail[T]:
        return await self.lane.offload(Kernel.of((WORKER_MODULE, kernel), KernelTrait.HOSTILE, enforcement=enforcement, idempotent=True), *args)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        railed = await async_boundary(SCENE_RENDER, self._rendered, catch=_RESIDUE)
        match railed.bind(lambda rail: rail):
            case Result(tag="ok", ok=receipt):
                return (await Journal.record(receipt.evidence())).map(lambda _landed: receipt)
            case refused:
                return Error(refused.error)

    async def _rendered(self) -> RuntimeRail[ArtifactReceipt]:
        key = self._key
        match self.op:
            case SceneOp(tag="image", image=(grid, spec)):
                return (await self._offload("render_image", grid, spec)).map(
                    lambda data: ArtifactReceipt.Scene(
                        key,
                        SceneTarget.PNG.value,
                        len(data),
                        frozendict({**_sized(spec), "address": ContentIdentity.key(SceneTarget.PNG.value, data).hex}),
                    )
                )
            case SceneOp(tag="export", export=(grid, target, spec)):
                return (await self._offload("render_export", grid, target.value, spec)).map(
                    lambda pair: ArtifactReceipt.Scene(key, target.value, len(pair[0]), frozendict({**pair[1], "address": ContentIdentity.key(target.value, pair[0]).hex}))
                )
            case SceneOp(tag="frames", frames=(grid, orbit, spec)):
                return (await self._offload("render_frames", grid, orbit, spec, enforcement=Enforcement.TERMINAL)).map(
                    lambda sequence: ArtifactReceipt.Scene(
                        key,
                        _FRAME_FORMAT,
                        sum(frame.nbytes for frame in sequence),
                        frozendict({
                            "frames": float(len(sequence)),
                            **_sized(spec),
                            "address": ContentIdentity.key(_FRAME_FORMAT, tuple(ContentIdentity.key(_FRAME_FORMAT, frame.tobytes()) for frame in sequence)).hex,
                        }),
                    )
                )
            case SceneOp(tag="ingest", ingest=(scene, source, target, spec)):
                return (await self._offload("render_ingest", scene, source.value, target.value, spec)).map(
                    lambda pair: ArtifactReceipt.Scene(key, target.value, len(pair[0]), frozendict({**pair[1], "address": ContentIdentity.key(target.value, pair[0]).hex}))
                )
            case SceneOp(tag="compose", compose=(grid_a, grid_b, op, spec)):
                return (await self._offload("render_compose", grid_a, grid_b, op.value, spec, enforcement=Enforcement.TERMINAL)).map(
                    lambda data: ArtifactReceipt.Scene(
                        key,
                        SceneTarget.PNG.value,
                        len(data),
                        frozendict({
                            "boolean": op.value,
                            **_sized(spec),
                            "address": ContentIdentity.key(SceneTarget.PNG.value, data).hex,
                        }),
                    )
                )
            case _:
                assert_never(self.op)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _sized(spec: RenderSpec) -> frozendict[str, float]:
    factor = float(spec.scale or 1)
    return frozendict({"width": spec.window[0] * factor, "height": spec.window[1] * factor})


def _canon(op: SceneOp) -> tuple[bytes, ...]:
    match op:
        case SceneOp(tag="image", image=(grid, spec)):
            return framed(b"image", CANON.encode(spec), *grid.spans())
        case SceneOp(tag="export", export=(grid, target, spec)):
            return framed(b"export", target.value.encode(), CANON.encode(spec), *grid.spans())
        case SceneOp(tag="frames", frames=(grid, orbit, spec)):
            return framed(b"frames", CANON.encode(orbit), CANON.encode(spec), *grid.spans())
        case SceneOp(tag="ingest", ingest=(scene, source, target, spec)):
            return framed(b"ingest", source.value.encode(), target.value.encode(), CANON.encode(spec), scene)
        case SceneOp(tag="compose", compose=(grid_a, grid_b, boolean, spec)):
            return framed(b"compose", boolean.value.encode(), CANON.encode(spec), *framed(*grid_a.spans()), *framed(*grid_b.spans()))
        case _ as unreachable:
            assert_never(unreachable)

```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
