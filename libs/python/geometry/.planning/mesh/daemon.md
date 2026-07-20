# [PY_GEOMETRY_MESH_DAEMON]

One persistent IfcOpenShell tessellation daemon drives source bytes and the geometry-owned `TessellationPolicy` (minted on `mesh/cad`, imported downward) into per-element GLB and a typed semantic header. `TessellationSource` discriminates the modality: the `ifc` arm drives `ifcopenshell.geom.iterator` over the native `serializers.gltf`, the `cad` arm delegates the OCCT B-rep-to-GLB hop to the `mesh/cad#BRIDGE` `StepBridge` — one ADT, never parallel daemons, and the daemon never feeds an OCCT shape through the IFC iterator.

Every source enters `LanePolicy.drain` as a SOURCE-keyed `Admit` whose `ContentKey` folds `TessellationPolicy.spec` into the canonical seed, so a re-tessellation at identical input and settings replays by reference and only a miss offloads the kernel. Two-key discipline is law: the policy-folded re-tessellation CACHE key stays distinct from the seed-zero (`Some(0)`) `XxHash128` GLB WIRE key equal to the C# `RepresentationContentHash` byte-for-byte — the `rasm.runtime.reproduction` `name="glb-by-key"` pin this daemon graduates into a graded sample — and a policy-folded seed on the wire key is the named drift defect. Daemon/serve boundary is equally law: the daemon tessellates, caches, and keys, never opening a channel, framing bytes, or naming a wire shape; `mesh/serve` registers, frames, and streams, never tessellating, re-keying, or reaching past the returned results.

## [01]-[INDEX]

- [01]-[DAEMON]: tessellation-source ADT over the SOURCE-keyed lane cache and the offloaded IFC/CAD kernels, returning `RuntimeRail[Block[TessellationResult]]` with drain-on-harvest receipts on `contribute`.

## [02]-[DAEMON]

- Owner: `TessellationDaemon` — the boundary capsule draining SOURCE-keyed units through one `LanePolicy.drain`, so the lane's content cache owns the hit/miss short-circuit and the daemon holds no private warm pool or subprocess primitive; `_cache` is the lane's own session cache threaded as a value across drains, never a second replay mechanism beside it. `TessellationSource` discriminates AEC versus mechanical geometry by case, never a parallel `SourceFormat` enum drifting against a `fmt` field; the mesher knobs are `TessellationPolicy` fields folded into the cache seed — no runtime `IdentityPolicy` field carries a mesher knob.
- Entry: `tessellate` RETURNS the results — the flagship egress the `mesh/serve` servicer streams; receipts stay on `contribute`, and a partial failure rides the stream as a `rejected` row, never a silent drop and never a fluent `self` stranding the GLB in the cache.
- Auto: `num_threads` binds from `LanePolicy.capacity` so the iterator's intra-kernel parallelism and the lane's slot allocator share one capacity, never a hardcoded literal.
- Receipt: the daemon mints no `GeometrySubject` — the C# `IfcSemanticModel` projects the IFC graph in-process, and the downstream `mesh/repair#MESH`/`scan/reconstruction#RECONSTRUCTION` owners graduate the conditioned solid.
- Packages: `ifcopenshell` (`file.from_string`, `geom.settings`/`iterator`/`serializers.gltf`/`serializer_settings`), the `mesh/cad#BRIDGE` bridge surface, and the runtime identity/lane/fault/receipt rails per the fence imports; the kernel crosses as `Kernel.of(kernel, KernelTrait.HOSTILE)` — the native OCCT body rides the warm process pool, its trait row supplying the `WORKER` worker-death retry at the offload leg.
- Growth: a new tessellation knob is one `TessellationPolicy` field folded into both the `geom.settings()` bind and the cache seed; a new CAD source is one `BridgeFormat` row reached through the existing `cad` case; a new source modality is one `TessellationSource` case and one `_dispatch` arm and its module-level kernel; a new semantic field is one `SemanticHeader` field.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Sequence
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import Final, Literal, assert_never

from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import replace

from rasm.geometry.mesh.cad import CANONICAL_TESSELLATION, BridgeFormat, StepBridge, TessellationPolicy
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import Admit, LanePolicy
from rasm.runtime.receipts import Phase, Receipt
from rasm.runtime.workers import Kernel, KernelTrait

# loop floor imports this module for the source/result vocabulary alone and never runs an IFC kernel, so the native
# band defers to its first worker-side reification; no module-scope table or annotation dereferences either name.
lazy import ifcopenshell
lazy import ifcopenshell.geom

# --- [TYPES] ----------------------------------------------------------------------------

type SourceTag = Literal["ifc", "cad"]
# GLB, typed header, element/triangle tally — the count rides the offload return, never a re-derived post-hop pass.
type KernelYield = tuple[bytes, "SemanticHeader", int, int]
# module-level kernels ship REFERENCE across the process seam — the by-name walk `shipped` runs; args stay plain picklable values.
type TessellateKernel = Callable[..., KernelYield]

# --- [MODELS] ---------------------------------------------------------------------------


class SemanticHeader(Struct, frozen=True, gc=False):
    schema: str = ""
    project: str = ""


# a B-rep model holds no IFC schema.
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
    replay: Phase = "emitted"  # "admitted" = by-reference cache replay, "emitted" = fresh iteration

    def fact(self, source: SourceTag) -> Receipt:
        return Receipt.of(
            "rasm.geometry.mesh.daemon",
            (self.replay, source, {"content_key": self.content_key.hex, "elements": self.element_count, "triangles": self.triangle_count}),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _settings(mesher: TessellationPolicy) -> "ifcopenshell.geom.settings":
    s = ifcopenshell.geom.settings()
    s.set("mesher-linear-deflection", mesher.deflection)
    s.set("mesher-angular-deflection", mesher.angle_tolerance)
    s.set("weld-vertices", True)
    s.set("apply-default-materials", True)
    return s


# `serializers.gltf` is a `WriteOnlyGeometrySerializer` with a FILENAME sink only — never the OBJ/SVG buffer cast — so the GLB
# rides a scoped temp path read back through `Path.read_bytes`; `use-element-guids` names each glTF node off the IFC GlobalId.
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


# bridge's contributor-free `glb` view (a live contributor cannot cross the pickle seam); the tally is `(1, 0)` —
# one assembly root, per-element count deferred to the bridge receipt.
def _tessellate_cad(source_bytes: bytes, fmt: BridgeFormat, mesher: TessellationPolicy, _num_threads: int) -> KernelYield:
    # re-raising `RuntimeError(detail)` keeps the `step-bridge.<stage>` classification across the pickle seam under the
    # fidelity latch rather than degrading to a bare `"RuntimeError"`; the lane's `async_boundary` lands it in the fault case.
    match StepBridge.tessellate(source_bytes, fmt, mesher):
        case Result(tag="ok", ok=glb):
            return glb, SEMANTIC_EMPTY, 1, 0
        case Result(tag="error", error=fault):
            facts = fault.facts()
            raise RuntimeError(str(facts.get("detail") or facts.get("subject") or fault.tag))


# kernel, cache-key seed bytes, and plain `*args` per case — the `cad` arm frames its `BridgeFormat` into the seed
# (identical bytes declared as two formats are two tessellations, never one cache slot) and threads it positionally;
# one `:` separator keeps the closed format vocabulary prefix-unambiguous against the body bytes.
def _dispatch(source: TessellationSource) -> tuple[TessellateKernel, bytes, tuple[object, ...]]:
    match source:
        case TessellationSource(tag="ifc", ifc=body):
            return _tessellate_ifc, body, (body,)
        case TessellationSource(tag="cad", cad=(body, fmt)):
            return _tessellate_cad, f"{fmt.value}:".encode() + body, (body, fmt)
        case _ as unreachable:
            assert_never(unreachable)


# --- [SERVICES] -------------------------------------------------------------------------


class TessellationDaemon:  # structural ReceiptContributor conformance — no subclass
    def __init__(self, lane: LanePolicy, mesher: TessellationPolicy = CANONICAL_TESSELLATION) -> None:
        self._lane = lane
        self._mesher = mesher
        self._receipts: Block[Receipt] = Block.empty()
        self._cache: Map[ContentKey, TessellationResult] = Map.empty()

    async def tessellate(self, source: TessellationSource | Sequence[TessellationSource]) -> "RuntimeRail[Block[TessellationResult]]":
        # cleared Ok carries every landed result in admission order; an Error arm still carries every per-source
        # fact and rejected row on the receipt stream, so a partial failure is addressable evidence.
        warm = self._cache
        admit = (Block.singleton(source) if isinstance(source, TessellationSource) else Block.of_seq(source)).map(self._admit)
        units = admit.choose(Result.to_option)
        receipt = await self._lane.drain(units.map(lambda a: a[2]), warm)
        self._cache = receipt.cache
        faults = receipt.faults.append(admit.choose(lambda a: a.swap().to_option()))
        self._fold(units, warm, faults)
        # replay provenance: the C# artifact index distinguishes by-reference replay from fresh iteration on this phase.
        results = units.choose(
            lambda a: self._cache.try_find(a[0]).map(lambda r: replace(r, replay="admitted" if a[0] in warm else "emitted"))
        )
        return faults.try_head().map(Error).default_value(Ok(results))

    # railed `Error` carries the key-mint `BoundaryFault` the fold surfaces as a `rejected` receipt.
    def _admit(self, source: TessellationSource) -> RuntimeRail[tuple[ContentKey, SourceTag, "Admit[TessellationResult]"]]:
        kernel, seed, args = _dispatch(source)
        return ContentIdentity.of(source.tag, self._mesher.spec + seed).map(lambda key: self._unit(kernel, args, key, source.tag))

    def _unit(
        self, kernel: TessellateKernel, args: tuple[object, ...], key: ContentKey, tag: SourceTag
    ) -> tuple[ContentKey, SourceTag, "Admit[TessellationResult]"]:
        async def work() -> RuntimeRail[TessellationResult]:
            # HOSTILE routes the native OCCT body onto the warm process pool, its trait-default WORKER row retrying a
            # transient worker death while the unit stays content-keyed for the cache short-circuit; the trailing
            # `mesher`/`num_threads` are positional kernel offload args.
            offloaded = await self._lane.offload(Kernel.of(kernel, KernelTrait.HOSTILE), *args, self._mesher, self._lane.capacity)
            return offloaded.map(lambda y: TessellationResult(key, y[0], y[2], y[3], y[1]))

        return key, tag, Admit(keyed=(key, work))

    # a PRE-drain key projects `admitted`, an absent key `emitted`; every fault projects the `rejected` case.
    def _fold(
        self,
        admitted: Block[tuple[ContentKey, SourceTag, Admit[TessellationResult]]],
        warm: Map[ContentKey, TessellationResult],
        faults: Block[BoundaryFault],
    ) -> None:
        facts = admitted.choose(
            lambda a: self._cache.try_find(a[0]).map(lambda r: replace(r, replay="admitted" if a[0] in warm else "emitted").fact(a[1]))
        )
        self._receipts = self._receipts.append(facts).append(faults.map(lambda f: Receipt.of("rasm.geometry.mesh.daemon", f)))

    def contribute(self) -> Iterable[Receipt]:
        # drain-on-harvest: the snapshot-and-clear swap keeps a re-harvest from re-sending historical evidence.
        drained, self._receipts = self._receipts, Block.empty()
        return drained
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
