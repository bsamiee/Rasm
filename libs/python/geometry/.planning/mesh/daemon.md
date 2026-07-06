# [PY_GEOMETRY_MESH_DAEMON]

The persistent IfcOpenShell tessellation daemon — the load-bearing cross-boundary owner. `TessellationDaemon` drives source bytes plus the geometry-owned `TessellationPolicy` (minted on `mesh/cad`, imported downward) into per-element GLB and a typed semantic header, discriminating the source modality by `match`/`assert_never` over one `TessellationSource` ADT rather than parallel daemons: the `ifc` arm drives `ifcopenshell.geom.iterator` over the native `serializers.gltf`, the `cad` arm delegates the OCCT B-rep-to-GLB hop to the `mesh/cad#BRIDGE` `StepBridge`. Every source enters the runtime `execution/lanes#LANE` `LanePolicy.drain` as a `keyed` `Admit` whose `ContentKey` derives from the SOURCE bytes with `TessellationPolicy.spec` folded into the canonical seed through `rasm.runtime.identity` `ContentIdentity.of`, so a re-tessellation at identical input and settings is a real cache `hit` replaying the `Ok` `TessellationResult` by reference (the `DrainReceipt` hit/fault vocabulary is the `observability/receipts` owner's) and only a miss offloads the CPU-bound kernel onto the lane's PEP-734 subinterpreter hop.

`tessellate` RETURNS the results — `RuntimeRail[Block[TessellationResult]]`, the per-element GLB, tally, and header the `mesh/serve` servicer streams across the C# wire — and receipts stay on `contribute`: the daemon conforms structurally to `ReceiptContributor`, accumulating its fact stream across drains and DRAINING it on harvest — snapshot-and-clear — so the serve owner's `@receipted` aspect emits each fact exactly once, never a re-send of historical evidence. The daemon/serve boundary is law: the daemon tessellates, caches, and keys — it never opens a channel, frames bytes, or names a wire shape; `mesh/serve` registers, frames, and streams — it never tessellates, re-keys, or reaches past the daemon's returned results. The two-key discipline holds: the policy-folded re-tessellation CACHE key stays distinct from the seed-zero (`Some(0)`) `XxHash128` GLB WIRE key that equals the C# `RepresentationContentHash` entry byte-for-byte — the reproduce-under-parity proven against the landed `rasm.runtime.reproduction` `name="glb-by-key"` design-pin, which this daemon graduates from pin into a graded sample.

## [01]-[INDEX]

- [01]-[DAEMON]: the tessellation-source ADT `_dispatch`ed to one module-level kernel each, every source admitted as a SOURCE-keyed `keyed` `Admit` so the lane cache short-circuits a re-tessellation, the warm OCCT pool offloaded across the lane subinterpreter hop on a miss, the native GLB serializer, the typed `SemanticHeader`, the returned `RuntimeRail[Block[TessellationResult]]`, and the drain-on-harvest receipt stream on `contribute`.

## [02]-[DAEMON]

- Owner: `TessellationDaemon` — the boundary capsule conforming structurally to the runtime-checkable `ReceiptContributor` Protocol (never subclassing it), draining a `Block` of SOURCE-keyed `keyed` `Admit[TessellationResult]` units through one `LanePolicy.drain` so the lane's content cache owns the hit/miss short-circuit and only a miss offloads the OCCT kernel. `TessellationSource` is the closed `@tagged_union` admission shape — an `ifc(source_bytes)` case and a `cad(source_bytes, fmt)` case carrying the `mesh/cad#BRIDGE` `BridgeFormat` — so the request discriminates AEC versus mechanical geometry by case rather than a parallel `SourceFormat` enum drifting against a `fmt` field. `_dispatch` is the one total `match` resolving each case to its module-level `TessellateKernel`, the SOURCE bytes the lane cache keys on, and the plain `*args` the no-pickle PEP-734 hop receives, closed by `assert_never`. `KernelYield` is the `(glb, SemanticHeader, elements, triangles)` tuple the kernel threads out so the tally rides the offload return rather than a re-derived post-hop count. `TessellationResult` is the per-pass carrier pairing the content-keyed GLB, the element/triangle tally, the typed `SemanticHeader`, and the `replay` provenance phase in one frozen `Struct`, owning its own `fact(source)` `Receipt` projection. `SemanticHeader` is the typed IFC header value object (`schema`/`project`) the `ifc` arm folds and the `cad` arm leaves at `SEMANTIC_EMPTY`, never a stringly `dict[str, str]`. The mesher knobs are `TessellationPolicy` fields (deflection, angular deflection) whose `spec` bytes fold into the cache seed — no runtime `IdentityPolicy` field carries a mesher knob.
- Cases: `TessellationSource` cases `ifc(source_bytes)` (the IfcOpenShell hybrid-CGAL+OCCT kernel over an in-memory `ifcopenshell.file.from_string`, drained through the native `serializers.gltf`) and `cad(source_bytes, fmt)` (the `BridgeFormat`-keyed OCCT XCAF reader-to-GLB hop the `mesh/cad#BRIDGE` `StepBridge` owns) — `_dispatch`ed by `match`/`assert_never` to one offloaded module-level kernel. The `ifc` kernel opens the byte stream in memory through `ifcopenshell.file.from_string` (the SPF-text in-memory loader, no disk round-trip), iterates, serializes, and folds the IFC `schema_identifier`/`IfcProject` header into a typed `SemanticHeader`. The `cad` kernel calls `StepBridge.tessellate`, which returns the raw GLB from the OCCT XCAF reader and the native `RWGltf_CafWriter`, carries `SEMANTIC_EMPTY` since a B-rep model holds no IFC schema, and re-raises an `Error` rail (carrying the fault detail) inside the offload fence so the lane's `async_boundary` re-converts it. The daemon never feeds an OCCT shape through the IFC iterator.
- Entry: `TessellationDaemon.tessellate` is the one polymorphic `async` entrypoint over a single `TessellationSource` or a batch `Sequence[TessellationSource]` — a single source lifts to a `Block.singleton`, both flow through one `_admit`/`drain` rail with no second batch method — and RETURNS `RuntimeRail[Block[TessellationResult]]`: the cleared `Ok` carries every landed result read off the post-drain cache in admission order (the flagship egress the serve owner streams), while a drain or key-mint `BoundaryFault` resolves the `Error` arm with the receipt stream still carrying every per-source fact and `rejected` row, so a partial failure is addressable evidence, never a silent drop and never a fluent `self` that strands the GLB in the cache. `_admit` derives each source's `ContentKey` through `ContentIdentity.of(source.tag, mesher.spec + source_bytes)` — the geometry-owned policy folded into the canonical seed bytes per the identity owner's generic-seed law — and `map`s the resolved key into one `keyed` `Admit[TessellationResult]` whose `Work` coroutine offloads the kernel only on a cache miss. `tessellate` splits the per-source rails once, rebinds `self._cache` to the returned `DrainReceipt.cache` so a downstream pass replays the upstream `Ok`, and folds the fact stream onto the accumulated receipts `contribute` DRAINS — receipts stay on `contribute`, each harvest by the serve owner's `@receipted` aspect emitting only the facts landed since the previous drain, never a re-emission of history.
- Entry: the `keyed` `Work` coroutine offloads through the lane owner's `LanePolicy.offload(kernel, *args, retry=RetryClass.OCCT)` — the daemon threads the `TessellationPolicy` and the `LanePolicy.capacity` slot count as trailing positional offload `*args` the no-pickle hop hands the kernel — so the 200-400 ms OCCT cold start and the per-element iterate/serialize loop run on a PEP-734 subinterpreter carrying its own GIL, the daemon stays responsive on the `anyio` event loop, the lane never imports the kernel, the active OTel context stitches across the hop through the lane's `traced_kernel` shim, and a transient `BrokenWorkerInterpreter` cold-start crash retries under the pinned `RetryClass.OCCT` row the `offload` retry leg binds before a budget-exhausted crash or deadline `TimeoutError` rails through the lane's own `async_boundary`. The coroutine `map`s the offloaded `KernelYield` into the `TessellationResult` carrying the SOURCE-derived `ContentKey` the unit was admitted under — never a second `ContentIdentity.of` over the output GLB, never an `.unwrap()` phantom. An offload `Error` lands in the lane's own `DrainReceipt.faults`, which `_fold` projects through `Receipt.of` into a `rejected` receipt rather than dropping the failed source from the contributed stream.
- Auto: the `ifc` kernel binds `geom.settings()` knobs `mesher-linear-deflection`/`mesher-angular-deflection` from the `TessellationPolicy`, drives the `serializers.gltf` `WriteOnlyGeometrySerializer` lifecycle (`writeHeader`/`write`/`finalize`) into a `TemporaryDirectory`-scoped filename — the glTF serializer admits a filename sink only, never the OBJ/SVG `serializers.buffer` cast, so the GLB rides the same temp-path round-trip the `mesh/cad#BRIDGE` `RWGltf_CafWriter` leg does and reads back through `Path.read_bytes` — and runs `geom.iterator(settings, model, num_threads, geometry_library="hybrid-cgal-simple-opencascade")` draining `iterator.get()` into `serializer.write` while folding the element/triangle counts in one pass. `serializer_settings` binds `use-element-guids` so each per-element glTF node names off the IFC GlobalId. `num_threads` binds from `LanePolicy.capacity` so the iterator's intra-kernel parallelism and the lane's slot allocator share one capacity rather than a hardcoded literal. `ContentIdentity.of` folds the `TessellationPolicy.spec` (`.17g`-formatted deflection/angle) into the XxHash128 seed over the SOURCE bytes, so a coarse and a fine tessellation key distinctly while identical input at identical settings keys identically and the lane cache reproduces the `TessellationResult` by reference. The cache key is DISTINCT from the wire key by design: the seed-zero (`Some(0)`) `XxHash128` over the per-element GLB bytes is the wire representation key equal to the C# `RepresentationContentHash` — the landed `rasm.runtime.reproduction` `name="glb-by-key"` design-pin this daemon graduates into a graded sample — and a policy-folded seed on the wire key is the named drift defect.
- Receipt: the daemon conforms structurally to `ReceiptContributor`. `_fold` is one fact stream over two projections: each admitted source's `TessellationResult` reads off the threaded `self._cache` and projects through `TessellationResult.fact` — a key present in the PRE-drain cache snapshot keys `phase="admitted"` (the by-reference replay), a key absent keys `emitted` (a fresh iteration) — while every `BoundaryFault` (the lane's `DrainReceipt.faults` offload failures appended with the `_admit` key-mint faults) projects through `Receipt.of("mesh.daemon", fault)` into the receipt owner's `rejected` case, so a failed tessellation is structured evidence on the one stream rather than a dropped source. `fact` mints the shape-polymorphic `Receipt.of("mesh.daemon", (phase, source, facts))` `fact` case, the `facts` dict carrying the `ContentKey.hex` and the element/triangle counts as flat scalars, never a stringly `str()`-coerced map; `contribute` DRAINS the accumulated `Block[Receipt]` — snapshot-and-clear on each harvest — so the serve owner's aspect emits every fact exactly once across repeated harvests. The daemon produces only the tessellated geometry and the lightweight header — it mints no `GeometrySubject`, since the C# `IfcSemanticModel` projects the IFC graph in-process and the downstream `mesh/repair#MESH`/`scan/reconstruction#RECONSTRUCTION` owners graduate the conditioned solid.
- Packages: `ifcopenshell` (`file.from_string` in-memory SPF loader, `by_type`, `schema_identifier`, `geom.settings`, `geom.iterator` with `initialize`/`get`/`next`, `geom.serializers.gltf` the filename-sink `WriteOnlyGeometrySerializer` with `writeHeader`/`write`/`finalize`, `geom.serializer_settings` with `use-element-guids`); geometry (`mesh/cad#BRIDGE` `BridgeFormat`/`StepBridge.tessellate`/`TessellationPolicy`/`CANONICAL_TESSELLATION` — the `cad` kernel composes the bridge and the mesher knobs import downward from the cad mint); runtime `rasm.runtime.identity` (`ContentIdentity.of`/`ContentKey` — the tolerance-only `IdentityPolicy` stays runtime-interior; the mesher knobs are geometry's), `execution/lanes#LANE` (`Admit`/`LanePolicy.drain`/`LanePolicy.offload`/`capacity`/`DrainReceipt` the SOURCE-keyed admission, the cache short-circuit, the subinterpreter hop, and the slot allocator), `reliability/faults#FAULT` (`RuntimeRail`/`BoundaryFault`), `reliability/resilience#RESILIENCE` (`RetryClass.OCCT` the pinned transient row the `Work` coroutine passes as `offload(..., retry=RetryClass.OCCT)`), `observability/receipts#RECEIPT` (`Phase`/`Receipt` and the `DrainReceipt` fault/hit vocabulary this page cites — the drain taxonomy is that owner's, the lanes page only transports it); `expression` (`tagged_union`/`tag`/`case`/`Ok`/`Error`/`Result` the admission split, `Block`/`Map` the folds and cache), `msgspec` (`Struct` the frozen carriers).
- Growth: a new tessellation knob is one `TessellationPolicy` field folded into both the `geom.settings()` bind and the cache seed; a new CAD source is one `mesh/cad#BRIDGE` `BridgeFormat` row reached through the existing `cad` case; a new source modality (a glTF re-tessellation, a streamed IFC) is one `TessellationSource` case plus one `_dispatch` match arm plus its module-level kernel, never a new method; a new semantic field is one `SemanticHeader` field the `ifc` kernel folds; the keyed admission, offload, cache short-circuit, and trace-stitch are the lane primitives already woven, so a new source rides them by composition; zero new surface.
- Boundary: the daemon mints no transport, channel, content key, or wire vocabulary — the `mesh/serve` owner registers the servicer, frames the 64 KiB `FrameEdge` stream, and speaks the C# `ComputeService`/`ArtifactSync` contract; the daemon keys the source through the shared `rasm.runtime.identity` `ContentIdentity` and offloads the kernel through the shared `execution/lanes#LANE` `LanePolicy` rather than owning a private warm pool, subprocess primitive, or cache. The GLB it returns is the shape the C# `SharpGLTF` import reads; the IFC semantic graph the C# `IfcSemanticModel` projects in-process is never re-derived. The deleted forms: a `tessellate` returning the fluent `self` that strands the GLB in the cache with no accessor (the flagship-egress break — the results ARE the return); a per-request process spawn; an in-process managed-IFC tessellator; a path-keyed cache; a private daemon cache where the lane's `keyed` admission owns the hit/miss short-circuit; an output-GLB content key where the SOURCE-keyed `ContentKey` keys the cache before the hop so a hit actually skips the offload; a mesher knob on runtime `IdentityPolicy` where the geometry-owned `TessellationPolicy` folds into the seed; a parallel `SourceFormat` enum drifting against a `fmt` field; a direct `LanePolicy.offload` per source where the `keyed` `LanePolicy.drain` short-circuits a cache hit; a flat synchronous `boundary` where the CPU-bound kernel must offload across the lane subinterpreter hop; a hardcoded `num_threads=4` where `LanePolicy.capacity` is the one slot allocator; a stringly `dict[str, str]` semantic carrier; an inline `Receipt.of` threaded through the kernel body where `_fold` owns the stream and `TessellationResult.fact` the projection; a `ReceiptContributor` subclass where structural conformance carries the port; a `contribute` returning the undrained accumulation so every harvest re-sends historical evidence; a `_fold` reading only `self._cache` that discards the lane's `DrainReceipt.faults`; a policy-folded seed on the GLB wire key (the seed-zero `Some(0)` wire key equals the C# `RepresentationContentHash` by reference — decoded, never re-minted); and a second batch method where the one `drain` fold absorbs a `Sequence`.

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
from msgspec.structs import replace

from rasm.geometry.mesh.cad import CANONICAL_TESSELLATION, BridgeFormat, StepBridge, TessellationPolicy
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import Admit, LanePolicy
from rasm.runtime.receipts import Phase, Receipt
from rasm.runtime.resilience import RetryClass

# --- [TYPES] ----------------------------------------------------------------------------

type SourceTag = Literal["ifc", "cad"]
# the offloaded kernel yield threaded out of the hop — GLB, typed header, element/triangle tally —
# so the count rides the offload return rather than a re-derived post-hop pass.
type KernelYield = tuple[bytes, "SemanticHeader", int, int]
# module-level and arg-only so the no-pickle PEP-734 subinterpreter receives it; the `Work`-closure
# threads the trailing `mesher`/`num_threads` as positional offload args (a closure cannot cross).
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
    replay: Phase = "emitted"  # the wire-floor provenance: "admitted" = by-reference cache replay, "emitted" = fresh iteration

    def fact(self, source: SourceTag) -> Receipt:
        # flat native scalars the receipts `enc_hook=repr` renderer serializes without a `str()` coerce.
        return Receipt.of(
            "mesh.daemon", (self.replay, source, {"content_key": self.content_key.hex, "elements": self.element_count, "triangles": self.triangle_count})
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _settings(mesher: TessellationPolicy) -> "ifcopenshell.geom.settings":
    s = ifcopenshell.geom.settings()
    s.set("mesher-linear-deflection", mesher.deflection)
    s.set("mesher-angular-deflection", mesher.angle_tolerance)
    s.set("weld-vertices", True)
    s.set("apply-default-materials", True)
    return s


# the offloaded IFC kernel: it mints no content key — the daemon keys the SOURCE bytes with the
# geometry-owned TessellationPolicy.spec folded into the canonical seed. The
# `geom.iterator(settings, model, num_threads, geometry_library=...)` drains `initialize`/`get`/`next`
# in one fold while `serializers.gltf(path, settings, serializer_settings)` — a `WriteOnlyGeometrySerializer`
# with a FILENAME sink, never the OBJ/SVG buffer cast — writes the per-element GLB (each node named off
# the IFC GlobalId by `use-element-guids`) to the scoped temp path, read back through `Path.read_bytes`.
def _tessellate_ifc(source_bytes: bytes, mesher: TessellationPolicy, num_threads: int) -> KernelYield:
    settings = _settings(mesher)
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
# contributor-free `glb` view (a live contributor cannot cross the no-pickle hop), and the tally is
# `(1, 0)` — one assembly root, per-element count deferred to the bridge receipt.
def _tessellate_cad(source_bytes: bytes, fmt: BridgeFormat, mesher: TessellationPolicy, _num_threads: int) -> KernelYield:
    # boundary-kernel control flow: this runs INSIDE the lane `offload` `async_boundary`, which
    # lands the raise in the `boundary` fault case and records the cause on the active span. The
    # bridge fault's `detail` carries the discriminating `step-bridge.<stage>` message the faults
    # catch-all preserves, so re-raising a `RuntimeError(detail)` keeps that classification across
    # the no-pickle offload hop rather than degrading to a bare `"RuntimeError"`.
    match StepBridge.tessellate(source_bytes, fmt, mesher):
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


class TessellationDaemon:  # conforms structurally to the runtime-checkable ReceiptContributor Protocol; subclassing the port is the deleted form
    def __init__(self, lane: LanePolicy, mesher: TessellationPolicy = CANONICAL_TESSELLATION) -> None:
        self._lane = lane
        self._mesher = mesher
        self._receipts: Block[Receipt] = Block.empty()
        self._cache: Map[ContentKey, TessellationResult] = Map.empty()

    async def tessellate(self, source: TessellationSource | Sequence[TessellationSource]) -> "RuntimeRail[Block[TessellationResult]]":
        # the flagship egress: the results RETURN — the cleared Ok carries every landed
        # TessellationResult in admission order (the serve owner's stream input), while a drain or
        # key-mint fault resolves the Error arm with the receipt stream still carrying every
        # per-source fact and rejected row. Receipts stay on `contribute`, harvested once by the
        # serve owner's @receipted aspect — never a fluent `self` return stranding the GLB.
        warm = self._cache
        admit = (Block.singleton(source) if isinstance(source, TessellationSource) else Block.of_seq(source)).map(self._admit)
        units = admit.choose(Result.to_option)
        receipt = await self._lane.drain(units.map(lambda a: a[2]), warm)
        self._cache = receipt.cache
        faults = receipt.faults.append(admit.choose(lambda a: a.swap().to_option()))
        self._fold(units, warm, faults)
        # each returned result carries its replay provenance — the wire floor's admitted-vs-emitted
        # phase the C# artifact index distinguishes by-reference replay from fresh iteration on.
        results = units.choose(
            lambda a: self._cache.try_find(a[0]).map(lambda r: replace(r, replay="admitted" if a[0] in warm else "emitted"))
        )
        return faults.try_head().map(Error).default_value(Ok(results))

    # the SOURCE-keyed `ContentKey` (the geometry-owned TessellationPolicy.spec folded into the
    # canonical seed bytes per the identity owner's generic-seed law), the source tag the receipt
    # phase reads, and the `Work` coroutine offloading the kernel; the railed `Error` carries the
    # key-mint `BoundaryFault` the fold surfaces as a `rejected` receipt.
    def _admit(self, source: TessellationSource) -> RuntimeRail[tuple[ContentKey, SourceTag, "Admit[TessellationResult]"]]:
        kernel, body, args = _dispatch(source)
        return ContentIdentity.of(source.tag, self._mesher.spec + body).map(lambda key: self._unit(kernel, args, key, source.tag))

    def _unit(
        self, kernel: TessellateKernel, args: tuple[object, ...], key: ContentKey, tag: SourceTag
    ) -> tuple[ContentKey, SourceTag, "Admit[TessellationResult]"]:
        async def work() -> RuntimeRail[TessellationResult]:
            # the offload leg carries `retry=RetryClass.OCCT` so a transient `BrokenWorkerInterpreter`
            # cold-start crash on the OCCT subinterpreter retries under the pinned policy row before
            # the lane's `async_boundary` resolves — the unit stays content-`keyed` for the cache
            # short-circuit AND retries a transient hop; the trailing `mesher`/`num_threads` are
            # positional kernel offload args.
            offloaded = await self._lane.offload(kernel, *args, self._mesher, self._lane.capacity, retry=RetryClass.OCCT)
            return offloaded.map(lambda y: TessellationResult(key, y[0], y[2], y[3], y[1]))

        return key, tag, Admit(keyed=(key, work))

    # one fact stream: each landed `TessellationResult` projects through `TessellationResult.fact` — a
    # PRE-drain key `admitted` (by-reference replay), an absent key `emitted` (fresh iteration) — and
    # every drain or key-mint `BoundaryFault` projects through the receipts owner's `rejected` case.
    def _fold(
        self,
        admitted: Block[tuple[ContentKey, SourceTag, Admit[TessellationResult]]],
        warm: Map[ContentKey, TessellationResult],
        faults: Block[BoundaryFault],
    ) -> None:
        facts = admitted.choose(
            lambda a: self._cache.try_find(a[0]).map(lambda r: replace(r, replay="admitted" if a[0] in warm else "emitted").fact(a[1]))
        )
        self._receipts = self._receipts.append(facts).append(faults.map(lambda f: Receipt.of("mesh.daemon", f)))

    def contribute(self) -> Iterable[Receipt]:
        # drain-on-harvest: each @receipted harvest emits only the facts landed since the previous
        # drain; the snapshot-and-clear swap keeps a re-harvest from re-sending historical evidence.
        drained, self._receipts = self._receipts, Block.empty()
        return drained
```
