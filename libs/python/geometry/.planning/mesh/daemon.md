# [PY_GEOMETRY_MESH_DAEMON]

The persistent IfcOpenShell tessellation daemon — the load-bearing cross-boundary owner. `TessellationDaemon` drives source bytes plus an `IdentityPolicy` into per-element GLB and a typed semantic header, discriminating the source modality by `match`/`assert_never` over one `TessellationSource` ADT rather than parallel daemons: the `ifc` arm drives `ifcopenshell.geom.iterator` over the native `serializers.gltf`, the `cad` arm delegates the OCCT B-rep-to-GLB hop to the `mesh/cad#BRIDGE` `StepBridge`. Every source enters the runtime `execution/lanes#LANE` `LanePolicy.drain` as a `keyed` `Admit` whose `ContentKey` derives from the SOURCE bytes plus the `IdentityPolicy` through `evidence/identity#IDENTITY` `ContentIdentity.of`, so a re-tessellation at identical input and settings is a real `DrainReceipt` cache `hit` replaying the `Ok` `TessellationResult` by reference and only a miss offloads the CPU-bound kernel onto the lane's PEP-734 subinterpreter hop. The daemon implements `ReceiptContributor` so one `@receipted`-observed pass emits its typed evidence, and speaks the existing C# `ComputeService`/`ArtifactSync` contract through the runtime `transport/serve#SERVE` `ServerHost`, framed by the 64 KiB `ArtifactSync` leg with per-frame Crc32 and whole-artifact XxHash128.

## [01]-[INDEX]

- [01]-[DAEMON]: the tessellation-source ADT `_dispatch`ed to one module-level kernel each, every source admitted as a SOURCE-keyed `keyed` `Admit` so the lane cache short-circuits a re-tessellation, the warm OCCT pool offloaded across the lane subinterpreter hop on a miss, the native GLB serializer, the typed `SemanticHeader`, the `ContentIdentity`-keyed `TessellationResult`, the `@receipted` harvest aspect, and the polymorphic single-or-batch async drain.

## [02]-[DAEMON]

- Owner: `TessellationDaemon` — the boundary capsule implementing the runtime `ReceiptContributor` Protocol, draining a `Block` of SOURCE-keyed `keyed` `Admit[TessellationResult]` units through one `LanePolicy.drain` so the lane's content cache owns the hit/miss short-circuit and only a miss offloads the OCCT kernel. `TessellationSource` is the closed `@tagged_union` admission shape — an `ifc(source_bytes)` case and a `cad(source_bytes, fmt)` case carrying the `mesh/cad#BRIDGE` `BridgeFormat` — so the request discriminates AEC versus mechanical geometry by case rather than a parallel `SourceFormat` enum drifting against a `fmt` field. `_dispatch` is the one total `match` resolving each case to its module-level `TessellateKernel`, the SOURCE bytes the lane cache keys on, and the plain `*args` the no-pickle PEP-734 hop receives (the `cad` arm threading its own `BridgeFormat` as a positional arg, a closure being uncrossable), closed by `assert_never`. `KernelYield` is the `(glb, SemanticHeader, elements, triangles)` tuple the kernel threads out so the tally rides the offload return rather than a re-derived post-hop count. `TessellationResult` is the per-pass carrier pairing the content-keyed GLB, the element/triangle tally, and the typed `SemanticHeader` in one frozen `Struct`, owning its own `fact(phase, source)` `Receipt` projection. `SemanticHeader` is the typed IFC header value object (`schema`/`project`) the `ifc` arm folds and the `cad` arm leaves at `SEMANTIC_EMPTY`, never a stringly `dict[str, str]`.
- Cases: `TessellationSource` cases `ifc(source_bytes)` (the IfcOpenShell hybrid-CGAL+OCCT kernel over an in-memory `ifcopenshell.file.from_string`, drained through the native `serializers.gltf`) and `cad(source_bytes, fmt)` (the `BridgeFormat`-keyed OCCT XCAF reader-to-GLB hop the `mesh/cad#BRIDGE` `StepBridge` owns) — `_dispatch`ed by `match`/`assert_never` to one offloaded module-level kernel. The `ifc` kernel opens the byte stream in memory through `ifcopenshell.file.from_string` (the SPF-text in-memory loader, no disk round-trip), iterates, serializes, and folds the IFC `schema_identifier`/`IfcProject` header into a typed `SemanticHeader`. The `cad` kernel calls `StepBridge.tessellate`, which returns the raw GLB from the OCCT XCAF reader and the native `RWGltf_CafWriter`, carries `SEMANTIC_EMPTY` since a B-rep model holds no IFC schema, and re-raises an `Error` rail (carrying the fault detail) inside the offload fence so the lane's `async_boundary` re-converts it. The daemon never feeds an OCCT shape through the IFC iterator.
- Entry: `TessellationDaemon.tessellate` is the one polymorphic entrypoint over a single `TessellationSource` or a batch `Sequence[TessellationSource]` — a single source lifts to a `Block.singleton`, both flow through one `_admit`/`drain` rail with no second batch method. `_admit` is the `RuntimeRail`-returning admission: it derives each source's `ContentKey` through `ContentIdentity.of(source.tag, source_bytes, policy)` and `map`s the resolved key into one `keyed` `Admit[TessellationResult]` whose `Work` coroutine offloads the kernel only on a cache miss, the railed `Error` carrying the key-mint `BoundaryFault` rather than lowering to a silent `Nothing`. `tessellate` splits the per-source rails once — `Result.to_option` drains the admitted units into the `LanePolicy.drain(units, self._cache)` over the threaded session cache, `Result.swap().to_option` drains the key-mint faults into the fold beside the lane's own `DrainReceipt.faults` — and rebinds `self._cache` to the returned `DrainReceipt.cache` so a downstream pass replays the upstream `Ok`. It returns the daemon itself: the `@receipted(REDACTION)` aspect harvests `contribute()` on exit, so the public surface is the `ReceiptContributor` the caller reads, never a per-source rail the aspect would discard.
- Entry: the `keyed` `Work` coroutine offloads through the lane owner's `LanePolicy.offload(kernel, *args, retry=RetryClass.OCCT)` — the daemon threads the `IdentityPolicy` and the `LanePolicy.capacity` slot count as trailing positional offload `*args` the no-pickle hop hands the kernel (`offload(kernel, body, [fmt,] policy, capacity)`), the only keyword the optional `retry` class on `offload` (capacity rides the lane's memoised `CapacityLimiter` and the deadline rides `self.deadline`, so neither is an `offload` parameter) — so the 200-400 ms OCCT cold start and the per-element iterate/serialize loop run on a PEP-734 subinterpreter carrying its own GIL — the daemon stays responsive on the `anyio` event loop, the lane never imports the kernel, the active OTel context stitches across the hop through the lane's `traced_kernel` shim, and a transient `BrokenWorkerInterpreter` cold-start crash retries under the `RetryClass.OCCT` stamina row the `offload` retry leg binds (the retry riding the offload leg as a lane aspect, the resilience "consumer that owns its own coroutine" path, so this unit stays content-`keyed` for the cache AND retries a transient hop without a fourth `Admit` case) before a budget-exhausted `BrokenWorkerInterpreter`/deadline `TimeoutError` rails through the lane's own `async_boundary`. The coroutine `map`s the offloaded `KernelYield` into the `TessellationResult` carrying the SOURCE-derived `ContentKey` the unit was admitted under — never a second `ContentIdentity.of` over the output GLB, never a `RuntimeRail[ContentKey]` bound straight into the `ContentKey` field, never an `.unwrap()` phantom. The fold is pure rail composition: the offload leg returns a rail and the key minted before the hop, so no raw exception escapes; an offload `Error` lands in the lane's own `"offload"`-fenced `DrainReceipt.faults`, which `_fold` projects through `Receipt.of` into a `rejected` receipt rather than dropping the failed source from the contributed stream.
- Auto: the `ifc` kernel binds `geom.settings()` knobs `mesher-linear-deflection`/`mesher-angular-deflection` from the `IdentityPolicy`, drives the `serializers.gltf` `WriteOnlyGeometrySerializer` lifecycle (`writeHeader`/`write`/`finalize`) into a `TemporaryDirectory`-scoped filename — the glTF serializer admits a filename sink only, never the OBJ/SVG `serializers.buffer` cast, so the GLB rides the same temp-path round-trip the `mesh/cad#BRIDGE` `RWGltf_CafWriter` leg does and reads back through `Path.read_bytes` — and runs `geom.iterator(settings, model, num_threads, geometry_library="hybrid-cgal-simple-opencascade")` draining `iterator.get()` into `serializer.write` while folding the element/triangle counts in one pass. `serializer_settings` binds `use-element-guids` so each per-element glTF node names off the IFC GlobalId. `num_threads` binds from `LanePolicy.capacity` so the iterator's intra-kernel parallelism and the lane's slot allocator share one capacity rather than a hardcoded literal. `ContentIdentity.of` folds the `IdentityPolicy.spec` (`.17g`-formatted deflection/tolerance/angle) into the XxHash128 seed over the SOURCE bytes, so a coarse and a fine tessellation key distinctly while identical input at identical settings keys identically and the lane cache reproduces the `TessellationResult` by reference.
- Auto: cross-cutting concerns ride aspects, not inline call sites. `@receipted(REDACTION)` is the runtime `observability/receipts#RECEIPT` harvest aspect on the public `tessellate`: it routes the harvested `ReceiptContributor` stream through `Signals.emit_async`, the loop-friendly `FilteringBoundLogger` `a*` mirror offloading render-and-sink to a worker thread so a high-volume async serve never blocks the event loop. The content cache is the lane's own `Map[ContentKey, TessellationResult]` threaded across drains on `DrainReceipt.cache`, not a private daemon store — a `keyed` unit whose SOURCE key already carries an `Ok` short-circuits the offload entirely and increments the receipt's `hit` count, so re-tessellation is by-reference replay rather than re-iterating the OCCT kernel.
- Receipt: `TessellationDaemon` conforms to `ReceiptContributor`. `_fold` is one fact stream over two projections: each admitted source's `TessellationResult` reads off the threaded `self._cache` and projects through `TessellationResult.fact` — a key present in the PRE-drain cache snapshot keys `phase="admitted"` (the by-reference replay), a key absent keys `emitted` (a fresh iteration), both `Phase` literals the receipt owner admits — while every `BoundaryFault` (the lane's `DrainReceipt.faults` offload failures appended with the `_admit` key-mint faults) projects through `Receipt.of("mesh.daemon", fault)` into the receipt owner's `rejected` case, so a failed tessellation is structured evidence on the one stream rather than a dropped source. `fact` mints the shape-polymorphic `Receipt.of("mesh.daemon", (phase, source, facts))` `fact` case, the `facts` dict carrying the `ContentKey.hex` and the element/triangle counts as flat scalars the receipt owner's `enc_hook=repr` renderer serializes natively, never a stringly `str()`-coerced map; `contribute` yields the accumulated `Block[Receipt]`. The daemon produces only the tessellated geometry and the lightweight header — it mints no `compute/graduation#GeometrySubject`, since the C# `IfcSemanticModel` projects the IFC graph in-process and the downstream `mesh/repair#MESH`/`scan/reconstruction#RECONSTRUCTION` owners graduate the conditioned solid.
- Packages: `ifcopenshell` (`file.from_string` in-memory SPF loader, `by_type`, `schema_identifier`, `geom.settings`, `geom.iterator` with `initialize`/`get`/`next`, `geom.serializers.gltf` the filename-sink `WriteOnlyGeometrySerializer` with `writeHeader`/`write`/`finalize`, `geom.serializer_settings` with `use-element-guids`); `mesh/cad#BRIDGE` (`BridgeFormat`/`StepBridge.tessellate` the `cad` kernel composes rather than the IFC iterator); runtime `evidence/identity#IDENTITY` (`ContentIdentity.of`/`ContentKey`/`IdentityPolicy`/`CANONICAL_POLICY`), `execution/lanes#LANE` (`Admit`/`LanePolicy.drain`/`LanePolicy.offload`/`capacity`/`DrainReceipt` the SOURCE-keyed admission, the cache short-circuit, the subinterpreter hop, and the slot allocator), `reliability/faults#FAULT` (`RuntimeRail`/`BoundaryFault` the railed admission and the offload/key-mint fault the `rejected` fold surfaces), `reliability/resilience#RESILIENCE` (`RetryClass.OCCT` the subinterpreter-offload transient class the `keyed` `Work` coroutine passes as `offload(..., retry=RetryClass.OCCT)` so a transient `BrokenWorkerInterpreter` cold-start crash retries under one stamina row on the offload leg, never a per-daemon retry loop), `observability/receipts#RECEIPT` (`Phase`/`Receipt`/`ReceiptContributor`/`Redaction`/`receipted`, `Receipt.of` minting both the `fact` and `rejected` cases); `expression` (`tagged_union`/`tag`/`case`/`Ok`/`Error` the `cad`-kernel rail match, `Result.map`/`Result.to_option`/`Result.swap` the per-source admission split, `Block.singleton`/`of_seq`/`map`/`choose`/`append` the admission and dual receipt folds, `Map.try_find`/`key in map` the cache hit/miss probe), `msgspec` (`Struct` the frozen carriers).
- Growth: a new tessellation knob is one `Tolerance` field on `IdentityPolicy.spec` folded into both the `geom.settings()` bind and the `ContentIdentity` seed; a new CAD source is one `mesh/cad#BRIDGE` `BridgeFormat` row reached through the existing `cad` case; a new source modality (a glTF re-tessellation, a streamed IFC) is one `TessellationSource` case plus one `_dispatch` match arm plus its module-level kernel, never a new method; a new semantic field is one `SemanticHeader` field the `ifc` kernel folds; the keyed admission, offload, cache short-circuit, trace-stitch, and emit are the lane and receipt primitives already woven, so a new source rides them by composition; zero new surface.
- Boundary: the daemon mints no transport, channel, content key, or wire vocabulary — it speaks the existing C# `ComputeService`/`ArtifactSync` contract through the runtime `transport/serve#SERVE` `ServerHost`, keys the source through the shared `evidence/identity#IDENTITY` `ContentIdentity`, and offloads the kernel through the shared `execution/lanes#LANE` `LanePolicy` rather than owning a private warm pool, subprocess primitive, or cache. The GLB it returns is the shape the C# `SharpGLTF` import reads; the IFC semantic graph the C# `IfcSemanticModel` projects in-process is never re-derived. The deleted forms: a per-request process spawn; an in-process managed-IFC tessellator; a path-keyed cache; a private daemon cache where the lane's `keyed` admission owns the hit/miss short-circuit; an output-GLB content key where the SOURCE-keyed `ContentKey` keys the cache before the hop so a hit actually skips the offload; a parallel `SourceFormat` enum drifting against a `fmt` field where the `TessellationSource` case carries the modality; a direct `LanePolicy.offload` per source where the `keyed` `LanePolicy.drain` short-circuits a cache hit; a flat synchronous `boundary` where the CPU-bound kernel must offload across the lane subinterpreter hop; a `TessellationResult(ContentIdentity.of(...), ...)` assigning the `RuntimeRail[ContentKey]` straight into the `ContentKey` field rather than admitting under the resolved key; a hardcoded `num_threads=4` where `LanePolicy.capacity` is the one slot allocator; a stringly `dict[str, str]` semantic carrier where the typed `SemanticHeader` value object holds the schema/project; an inline `Receipt.of` threaded through the kernel body where the `@receipted` aspect emits and `TessellationResult.fact` owns the projection; a `choose`d `Option[Admit]` admission lowering a key-mint `Error` to a silent `Nothing` where the `RuntimeRail` admission carries the `BoundaryFault` into the `rejected` fold; a `_fold` reading only `self._cache` that discards the lane's `DrainReceipt.faults` so a failed offload emits no receipt; and a second batch method where the one `drain` fold absorbs a `Sequence`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Sequence
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import Final, Literal, assert_never

import ifcopenshell
import ifcopenshell.geom
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.geometry.mesh.cad import BridgeFormat, StepBridge
from rasm.runtime.content_identity import CANONICAL_POLICY, ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.lanes import Admit, LanePolicy
from rasm.runtime.receipts import Phase, Receipt, ReceiptContributor, Redaction, receipted
from rasm.runtime.resilience import RetryClass

# --- [TYPES] ----------------------------------------------------------------------------

type SourceTag = Literal["ifc", "cad"]
# the offloaded kernel yield threaded out of the hop — GLB, typed header, element/triangle tally —
# so the count rides the offload return rather than a re-derived post-hop pass.
type KernelYield = tuple[bytes, "SemanticHeader", int, int]
# module-level and arg-only so the no-pickle PEP-734 subinterpreter receives it; the `Work`-closure
# threads the trailing `policy`/`num_threads` as positional offload args (a closure cannot cross).
type TessellateKernel = Callable[..., KernelYield]

# --- [MODELS] ---------------------------------------------------------------------------


class SemanticHeader(Struct, frozen=True, gc=False):
    schema: str = ""
    project: str = ""


# the `cad` arm's header: a B-rep model holds no IFC schema, so a typed value object, not a stringly `{}`.
SEMANTIC_EMPTY: Final[SemanticHeader] = SemanticHeader()


@tagged_union(frozen=True)
class TessellationSource:
    tag: SourceTag = tag()
    ifc: bytes = case()
    cad: tuple[bytes, BridgeFormat] = case()


class TessellationResult(Struct, frozen=True, gc=False):
    content_key: ContentKey
    glb: bytes
    element_count: int
    triangle_count: int
    semantic: SemanticHeader

    def fact(self, phase: Phase, source: SourceTag) -> Receipt:
        # flat native scalars the receipts `enc_hook=repr` renderer serializes without a `str()` coerce.
        return Receipt.of("mesh.daemon", (phase, source, {
            "content_key": self.content_key.hex,
            "elements": self.element_count,
            "triangles": self.triangle_count,
        }))


REDACTION: Final[Redaction] = Redaction(classified=Map.empty())

# --- [OPERATIONS] -----------------------------------------------------------------------


def _settings(policy: IdentityPolicy) -> "ifcopenshell.geom.settings":
    s = ifcopenshell.geom.settings()
    s.set("mesher-linear-deflection", policy.deflection)
    s.set("mesher-angular-deflection", policy.angle_tolerance)
    s.set("weld-vertices", True)
    s.set("apply-default-materials", True)
    return s


# the offloaded IFC kernel: it mints no content key — the daemon keys the SOURCE bytes through the
# one `ContentIdentity` owner so a CAD source content-addresses identically to its IFC sibling. The
# `geom.iterator(settings, model, num_threads, geometry_library=...)` drains `initialize`/`get`/`next`
# in one fold while `GltfSerializer(path, settings, serializer_settings)` — a `WriteOnlyGeometrySerializer`
# with a FILENAME sink, never the OBJ/SVG buffer cast — writes the per-element GLB (each node named off
# the IFC GlobalId by `use-element-guids`) to the scoped temp path, read back through `Path.read_bytes`.
def _tessellate_ifc(source_bytes: bytes, policy: IdentityPolicy, num_threads: int) -> KernelYield:
    settings = _settings(policy)
    serializer_settings = ifcopenshell.geom.serializer_settings()
    serializer_settings.set("use-element-guids", True)
    model = ifcopenshell.file.from_string(source_bytes.decode("utf-8"))
    projects = model.by_type("IfcProject")
    header = SemanticHeader(schema=model.schema_identifier, project=(projects[0].Name or "") if projects else "")
    with TemporaryDirectory(prefix="ifc-glb-") as work:
        glb_path = str(Path(work, "out.glb"))
        serializer = ifcopenshell.geom.serializers.gltf(glb_path, settings, serializer_settings)
        serializer.writeHeader()
        iterator = ifcopenshell.geom.iterator(settings, model, num_threads, geometry_library="hybrid-cgal-simple-opencascade")
        elements = triangles = 0
        if iterator.initialize():
            while True:
                shape = iterator.get()
                serializer.write(shape)
                elements += 1
                triangles += len(shape.geometry.faces) // 3
                if not iterator.next():
                    break
        serializer.finalize()
        return Path(glb_path).read_bytes(), header, elements, triangles


# the offloaded CAD kernel: the `mesh/cad#BRIDGE` `StepBridge` owns the OCCT read/mesh/write at the
# contributor-free `glb` view (a live `ReceiptContributor` cannot cross the no-pickle hop), and the
# tally is `(1, 0)` — one assembly root, per-element count deferred to the bridge receipt.
def _tessellate_cad(source_bytes: bytes, fmt: BridgeFormat, policy: IdentityPolicy, _num_threads: int) -> KernelYield:
    # boundary-kernel control flow: this runs INSIDE the lane `offload` `async_boundary`, which
    # lands the raise in the `boundary` fault case and records the cause on the active span. The
    # bridge fault's `detail` carries the discriminating `step-bridge.<stage>` message the
    # `reliability/faults#FAULT` catch-all preserves (`detail = str(cause) or type(cause).__name__`),
    # so re-raising a `RuntimeError(detail)` keeps that message: the lane's `async_boundary("offload", ...)`
    # re-converts the `RuntimeError` and its `str()` IS that detail, so the `step-bridge.<stage>: ...`
    # classification survives the no-pickle offload hop into the final `boundary` fault's `detail` rather
    # than degrading to a bare `"RuntimeError"`. The `subject`/`tag` fallback covers a message-less fault.
    match StepBridge.tessellate(source_bytes, fmt, policy):
        case Ok(glb):
            return glb, SEMANTIC_EMPTY, 1, 0
        case Error(fault):
            facts = fault.facts()
            raise RuntimeError(str(facts.get("detail") or facts.get("subject") or fault.tag))


# one total `match` to the module-level kernel, the SOURCE bytes the lane cache keys, and plain `*args`
# the no-pickle hop receives — the `cad` arm threads its `BridgeFormat` positionally (a closure cannot
# cross), `assert_never` surfacing a new case as a typed gap rather than a closure-carried table row.
def _dispatch(source: TessellationSource) -> tuple[TessellateKernel, bytes, tuple[object, ...]]:
    match source:
        case TessellationSource(tag="ifc", ifc=body):
            return _tessellate_ifc, body, (body,)
        case TessellationSource(tag="cad", cad=(body, fmt)):
            return _tessellate_cad, body, (body, fmt)
        case _ as unreachable:
            assert_never(unreachable)

# --- [SERVICES] -------------------------------------------------------------------------


class TessellationDaemon(ReceiptContributor):
    def __init__(self, lane: LanePolicy, policy: IdentityPolicy = CANONICAL_POLICY) -> None:
        self._lane = lane
        self._policy = policy
        self._receipts: Block[Receipt] = Block.empty()
        self._cache: Map[ContentKey, TessellationResult] = Map.empty()

    @receipted(REDACTION)
    async def tessellate(self, source: TessellationSource | Sequence[TessellationSource]) -> "TessellationDaemon":
        warm = self._cache
        admit = (Block.singleton(source) if isinstance(source, TessellationSource) else Block.of_seq(source)).map(self._admit)
        units = admit.choose(Result.to_option)
        receipt = await self._lane.drain(units.map(lambda a: a[2]), warm)
        self._cache = receipt.cache
        self._fold(units, warm, receipt.faults.append(admit.choose(lambda a: a.swap().to_option())))
        return self

    # the SOURCE-keyed `ContentKey`, the source tag the receipt phase reads, and the `Work` coroutine
    # offloading the kernel and assembling the `TessellationResult`; the railed `Error` carries the key-mint
    # `BoundaryFault` the fold surfaces as a `rejected` receipt rather than dropping the source silently.
    def _admit(self, source: TessellationSource) -> RuntimeRail[tuple[ContentKey, SourceTag, "Admit[TessellationResult]"]]:
        kernel, body, args = _dispatch(source)
        return ContentIdentity.of(source.tag, body, self._policy).map(lambda key: self._unit(kernel, args, key, source.tag))

    def _unit(self, kernel: TessellateKernel, args: tuple[object, ...], key: ContentKey, tag: SourceTag) -> tuple[ContentKey, SourceTag, "Admit[TessellationResult]"]:
        async def work() -> RuntimeRail[TessellationResult]:
            # the offload leg carries `retry=RetryClass.OCCT` so a transient `BrokenWorkerInterpreter`
            # cold-start crash on the OCCT subinterpreter retries under one stamina policy row before the
            # lane's `async_boundary` resolves — the retry rides the offload leg as a lane aspect (the
            # resilience "consumer that owns its own coroutine" path), so this unit stays content-`keyed`
            # for the cache short-circuit AND retries a transient hop without a fourth `Admit` case or a
            # per-daemon retry loop; the trailing `policy`/`num_threads` are positional kernel offload args.
            offloaded = await self._lane.offload(kernel, *args, self._policy, self._lane.capacity, retry=RetryClass.OCCT)
            return offloaded.map(lambda y: TessellationResult(key, y[0], y[2], y[3], y[1]))
        return key, tag, Admit(keyed=(key, work))

    # one fact stream: each landed `TessellationResult` projects through `TessellationResult.fact` — a PRE-drain key
    # `admitted` (by-reference replay), an absent key `emitted` (fresh iteration) — and every drain or
    # key-mint `BoundaryFault` projects through the receipts owner's `rejected` case, so a failed
    # tessellation is structured evidence on the one contributed stream rather than a dropped source.
    def _fold(self, admitted: Block[tuple[ContentKey, SourceTag, Admit[TessellationResult]]], warm: Map[ContentKey, TessellationResult], faults: Block[BoundaryFault]) -> None:
        facts = admitted.choose(lambda a: self._cache.try_find(a[0]).map(
            lambda result: result.fact("admitted" if a[0] in warm else "emitted", a[1])
        ))
        self._receipts = self._receipts.append(facts).append(faults.map(lambda f: Receipt.of("mesh.daemon", f)))

    def contribute(self) -> Iterable[Receipt]:
        return self._receipts
```

## [03]-[RESEARCH]

- [SERIALIZER_LIFECYCLE]: `geom.serializers.gltf` binds the `WriteOnlyGeometrySerializer` `GltfSerializer(out_filename, geometry_settings, serializer_settings)` whose first argument is a FILENAME — the source comments the glTF/HDF/XML serializers out of the `transform_string` buffer cast the OBJ/SVG arms take, so the `serializers.buffer().get_value()` retrieval is NOT a glTF sink and the GLB rides a `TemporaryDirectory`-scoped `Path.read_bytes` round-trip, the temp-path admission the `mesh/cad#BRIDGE` `RWGltf_CafWriter` leg already proves. The `writeHeader`/`write`/`finalize` order, the `iterator(settings, model, num_threads, geometry_library=...)` constructor, the `initialize`/`get`/`next` drain protocol, and the per-shape `geometry.faces` triangle count confirm against the cached `ifcopenshell==0.8.5` cp313 wheel source (`geom/main.py` `serializers`/`iterator`, `ifcopenshell_wrapper.pyi` `GltfSerializer`). The `use-element-guids` `serializer_settings` key is the `SERIALIZER_SETTING` member naming each per-element node. The `_tessellate_ifc` kernel is the body the lane subinterpreter runs, offloaded as a module-level `Callable` the no-pickle PEP-734 hop receives. The open residual is the in-memory `OSD_FileSystem`/stream-sink seam that would retire the temp round-trip, shared with the `mesh/cad#BRIDGE` `[GLB_BYTE_RETURN]` work — a companion-interpreter confirmation, not a fence blocker.
- [CAD_BRIDGE_SIGNATURE]: the `cad` kernel calls `StepBridge.tessellate(source_bytes, fmt, policy) -> RuntimeRail[bytes]` contributor-free at the default `glb` `BridgeView`, matching `Ok(glb)`/`Error(fault)` rather than an `.unwrap()` phantom, because the kernel offloads across the no-pickle PEP-734 lane hop where a live `ReceiptContributor` cannot cross and the daemon's drained-receipt fold owns the bridge's evidence. The sibling `mesh/cad#BRIDGE` page carries exactly this contributor-free `tessellate(source_bytes, fmt, policy, *, view)` signature — `view` keyword-only, so the daemon's positional-3 call binds the default `glb` view — with the `BridgeView`-parameterized `glb`/`full` output and the matching `Ok(glb)`/`Error(fault)` drain, no positional `contribute` call. The daemon's `Error(fault)` arm reads the bridge fault's `detail` (the `step-bridge.<stage>` message the `reliability/faults#FAULT` catch-all preserves) and re-raises it so the discriminating classification survives the offload re-conversion. The seam is settled.
- [KEYED_ADMISSION]: every source enters one `LanePolicy.drain` as a `keyed` `Admit[TessellationResult]` whose `ContentKey` derives from the SOURCE bytes plus the `IdentityPolicy` (not the output GLB), so the lane's `Map[ContentKey, TessellationResult]` cache short-circuit is reachable: a re-tessellation at identical input and settings resolves the threaded `Ok` `TessellationResult` by reference and never invokes the offload coroutine, the lane incrementing `DrainReceipt.hit`. An output-GLB key cannot serve as a cache key because the GLB only exists after the kernel runs — keying on the input is the only shape that lets a hit skip the OCCT pass. The daemon threads `DrainReceipt.cache` back onto `self._cache` so a downstream pass (a repair-then-re-tessellate round-trip) replays the upstream `Ok` rather than recomputing. `_fold` recovers the per-source phase by probing the PRE-drain cache snapshot — present keys `admitted`, absent keys `emitted` — rather than re-zipping the order-shuffled `DrainReceipt.values`, since the lane's hit/live `partition` does not preserve source order. This collapses the prior `_run`/`@tessellated`/`_record`/`traversed` quartet, where `traversed` folded an aggregate rail the caller discarded and the cache short-circuit was dead prose because `LanePolicy.offload` ran every source unconditionally. The keyed-drain composition is the settled shape; the lane owner's `keyed` admission and threaded cache are the landed primitives this owner composes. The transient-retry gap a `keyed` admission would otherwise leave (the lane `Admit` union is `bare` XOR `keyed` XOR `retried`, so a unit cannot be both cache-`keyed` and drain-`retried`) is closed on the OFFLOAD leg rather than the admission: the `keyed` `Work` coroutine passes `retry=RetryClass.OCCT` to `LanePolicy.offload`, which wraps the raw `interpreter_run_sync` leg in `resilience#guard(RetryClass.OCCT)` so a transient `BrokenWorkerInterpreter` cold-start crash retries under one stamina policy row before the lane's `async_boundary` resolves — the resilience "consumer that owns its own coroutine" path, keeping the content-cache short-circuit (keyed) AND the transient retry on one unit without a fourth keyed-and-retried `Admit` case or a per-daemon retry loop. The `RetryClass.OCCT` member and its `anyio.BrokenWorkerInterpreter`-keyed `POLICY` row land on the `reliability/resilience#RESILIENCE` owner, the `retry` keyword on the `execution/lanes#LANE` `offload`.
- [IN_MEMORY_FORMAT_BAND]: `ifcopenshell.file.from_string(s)` is the in-memory loader confirmed against the branch `ifcopenshell` catalogue, decoding the SPF text wire form (the dominant IFC source the C# rail hands across) with no disk round-trip — the temp-file `open(path)` detour is the deleted form. The open residual: the in-memory admission of the non-SPF IFC encodings (the IFC-SQLite and zipped-IFC variants `guess_format` discriminates on a path) — whether a sqlite/zipped byte stream loads through a `from_string`-equivalent in-memory entry or whether those rarer encodings normalize to SPF text upstream; a runtime-action band confirmation on the companion interpreter, not a fence blocker for the SPF-text path the `ifc` case serves.
- [WARM_POOL]: the `geom.iterator` warm-pool reuse pattern amortizing the 200-400 ms OCCT cold start across the lane subinterpreter and the multithreaded-iteration memory ceiling at high `num_threads` (bound from `LanePolicy.capacity`) confirm against the live companion server; the 64 KiB `ArtifactSync` `FrameEdge` framing with per-frame Crc32 and whole-artifact XxHash128 proves against the C# `ArtifactSync` descriptors. The subinterpreter offload itself is settled — the `execution/lanes#LANE` `LanePolicy.offload` PEP-734 hop with the `traced_kernel` context stitch is the landed lane primitive this owner composes; the open residual is the per-subinterpreter OCCT module-state warmth (whether the kernel re-imports OpenCASCADE per hop or the lane's persistent worker keeps it warm), a lane-owner confirmation, not a daemon fence.
