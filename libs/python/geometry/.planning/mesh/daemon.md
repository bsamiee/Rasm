# [PY_GEOMETRY_MESH_DAEMON]

One persistent IfcOpenShell tessellation daemon drives source bytes and the geometry-owned `TessellationPolicy` (minted on `mesh/cad`, imported downward) into per-element GLB and a typed semantic header. `TessellationSource` discriminates the modality: the `ifc` arm drives `ifcopenshell.geom.iterator` over the native `serializers.gltf`, the `cad` arm delegates the OCCT B-rep-to-GLB hop to the `mesh/cad#BRIDGE` `StepBridge` — one ADT, never parallel daemons, and the daemon never feeds an OCCT shape through the IFC iterator.

Every source enters `LanePolicy.drain` as a SOURCE-keyed `Admit` whose `ContentKey` folds `TessellationPolicy.spec` into the canonical seed, so a re-tessellation at identical input and settings replays by reference and only a miss offloads the kernel. Two-key discipline is law: the policy-folded re-tessellation CACHE key this daemon mints stays distinct from the seed-zero (`Some(0)`) `XxHash128` GLB WIRE key the `mesh/cad#BRIDGE` `GlbArtifact.of` carrier mints at the encoding site — equal to the C# `RepresentationContentHash` byte-for-byte, the `rasm.runtime.reproduction` `name="glb-by-key"` pin this daemon graduates into a graded sample — and a policy-folded seed on the wire key is the named drift defect. Every kernel returns that carrier rather than loose octets, so the wire key travels with the bytes it addresses and no downstream servicer re-hashes a payload it did not encode. Daemon/serve boundary is equally law: the daemon tessellates, caches, and cache-keys, never opening a channel, framing bytes, or naming a wire shape; `mesh/serve` registers, frames, and streams, never tessellating, re-keying, or reaching past the returned results.

## [01]-[INDEX]

- [02]-[DAEMON]: tessellation-source ADT over the SOURCE-keyed lane cache, the durable object-store spill and its read-through, and the offloaded IFC/CAD kernels, returning `RuntimeRail[Block[TessellationResult]]` with drain-on-harvest receipts on `contribute`.

## [02]-[DAEMON]

- Owner: `TessellationDaemon` — the boundary capsule draining SOURCE-keyed units through one `LanePolicy.drain`, so the lane's content cache owns the hit/miss short-circuit and the daemon holds no private warm pool or subprocess primitive; `_cache` is the lane's own session cache threaded as a value across drains, never a second replay mechanism beside it. The durable tier below it is an INJECTED `ObjectStoreLane` — the branch's one `obstore` operation surface, its `StoreOp` axis, reach matrix, and retry disposition already settled at `runtime/transport/roots#STORE` — so this page mints no store handle, route table, backend row, or `from_url`, exactly as `data/tabular/egress#EGRESS` composes that lane without owning it. `TessellationSource` discriminates AEC versus mechanical geometry by case, never a parallel `SourceFormat` enum drifting against a `fmt` field; the mesher knobs are `TessellationPolicy` fields folded into the cache seed — no runtime `IdentityPolicy` field carries a mesher knob.
- Law: runtime `Kernel.of` mints `_tessellate_ifc`/`_tessellate_cad` as `Kernel.name`, and `traced_kernel` passes that name to `Profiles.phase`; the daemon adds no profile registry or in-kernel instrumentation beyond the pulse proxy write.
- Law: both kernels take the lane conduit's pickled `tap` as a trailing offload arg and beat the graduation `GeometryPulse.TESSELLATION` point through `pulsed` — the IFC iterator every `PULSE_STRIDE` elements, the CAD arm once per opaque bridge hop — so a `Hooks` tap streams live tessellation progress under the lane's lossy drop law with the worker reaching only the queue proxy.
- Law: the durable tier is a content-addressed SPILL, never an authority — two write-once objects per unit under one `spill_path` derivation, the GLB octets keyed by the artifact's own wire key and a `SpillHeader` keyed by the policy-folded cache key resolving onto it, because `ArtifactSync` holds a wire key and asks for octets while a cold daemon holds a source and a policy and asks which artifact they produced. `create` is the put mode: an object already under a content-addressed key holds those same octets, so an overwrite buys a race with a fleet peer and nothing else; the header lands only past a cleared artifact write, so no reader resolves a pointer onto octets that are not there. The read-through runs AHEAD of the kernel and its every failure mode folds to absence — the cost of a store miss is a tessellation nobody skipped, where a rail there would be a tessellation nobody GOT because a store was briefly unreachable — and the refusal still lands as a `rejected` receipt so an outage reads as evidence rather than as a daemon that quietly stopped replaying. The re-mint over returned octets is the PROOF the store answered what the header names, so a corrupted object refuses by name instead of replaying under an identity it does not hash to. `SpillOutcome` rides the crossing's own receipt row, since a second receipt family would leave a reader joining two streams to answer one question, and `_phase` derives replay provenance across both reuse tiers at ONE site.
- Entry: `tessellate` RETURNS the results — the flagship egress the `mesh/serve` servicer streams; receipts stay on `contribute`, and a partial failure rides the stream as a `rejected` row, never a silent drop and never a fluent `self` stranding the GLB in the cache.
- Auto: `num_threads` binds from `LanePolicy.capacity` so the iterator's intra-kernel parallelism and the lane's slot allocator share one capacity, never a hardcoded literal.
- Receipt: the daemon mints no `GeometrySubject` — the C# `IfcSemanticModel` projects the IFC graph in-process, and the downstream `mesh/repair#MESH`/`scan/reconstruction#RECONSTRUCTION` owners graduate the conditioned solid.
- Packages: `ifcopenshell` (`file.from_string`, `geom.settings`/`iterator`/`serializers.gltf`/`serializer_settings`), the `mesh/cad#BRIDGE` bridge surface, `msgspec` (the one deterministic `SpillHeader` codec pair), and the runtime identity/lane/fault/receipt/store rails per the fence imports; the kernel crosses as `Kernel.of(kernel, KernelTrait.HOSTILE)` — the native OCCT body rides the warm process pool, its trait row supplying the `WORKER` worker-death retry at the offload leg.
- Growth: a new tessellation knob is one `TessellationPolicy` field folded into both the `geom.settings()` bind and the cache seed; a new CAD source is one `BridgeFormat` row reached through the existing `cad` case; a new source modality is one `TessellationSource` case and one `_dispatch` arm and its module-level kernel; a new semantic field is one `SemanticHeader` field; a new durable object class is one `SpillKind` row reaching both ends through `spill_path`, and a new durable-tier state one `SpillOutcome` member the existing receipt row already carries.
- Boundary: the store lane arrives BUILT at composition and no page-local handle, scheme roster, credential, or retry row is minted here; the durable tier never gates correctness, so a refused read or a refused write leaves a live artifact and a receipt row rather than a rail.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Sequence
from enum import StrEnum
from pathlib import Path
from queue import Queue
from tempfile import TemporaryDirectory
from typing import Final, Literal, assert_never

from expression import Error, Nothing, Ok, Option, Result, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from msgspec import json as msgjson
from msgspec.structs import replace

from rasm.geometry.graduation import GeometryPulse, PulseBeat
from rasm.geometry.mesh.cad import CANONICAL_TESSELLATION, BridgeFormat, GlbArtifact, StepBridge, TessellationPolicy
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary
from rasm.runtime.identity import ContentIdentity, ContentKey, IdentitySource
from rasm.runtime.lanes import Admit, LanePolicy, PulseFact, pulsed
from rasm.runtime.receipts import Phase, Receipt
from rasm.runtime.roots import ObjectStoreLane, StoreOp
from rasm.runtime.workers import Kernel, KernelTrait

# loop floor imports this module for the source/result vocabulary alone and never runs an IFC kernel, so the native
# band defers to its first worker-side reification; no module-scope table or annotation dereferences either name.
lazy import ifcopenshell
lazy import ifcopenshell.geom

# --- [TYPES] ----------------------------------------------------------------------------

type SourceTag = Literal["ifc", "cad"]
# wire-keyed GLB carrier, typed header, element/triangle tally — the count rides the offload return, never a
# re-derived post-hop pass, and the artifact carries the wire key its own encoding site minted.
type KernelYield = tuple[GlbArtifact, "SemanticHeader", int, int]
# module-level kernels ship REFERENCE across the process seam — the by-name walk `shipped` runs; args stay plain picklable values.
type TessellateKernel = Callable[..., KernelYield]


class SpillKind(StrEnum):
    # the TWO objects one tessellation lands in the durable tier, because the two reuse paths ask DIFFERENT
    # questions: `ArtifactSync` holds a wire key and asks for octets, while a cold daemon holds a source and a
    # policy and asks which artifact they produced. One object keyed either way answers exactly one of them, and
    # the second path then pays a full re-tessellation the store already holds the answer to.
    ARTIFACT = "artifact"  # the GLB octets under the artifact's OWN seed-zero wire key
    SOURCE = "source"  # the header resolving a policy-folded cache key onto that artifact


class SpillOutcome(StrEnum):
    # what the durable tier did with THIS unit, so a receipt reader separates a landed write from a replay from a
    # composition that bound no lane at all — three states one boolean erases into one, and the erased pair is
    # exactly "the store is unbound" versus "the store refused", which are an operator's two different problems.
    UNBOUND = "unbound"  # no lane at composition; the daemon runs cache-only and claims no durable tier
    LANDED = "landed"  # both write-once puts created their objects
    REPLAYED = "replayed"  # the read-through answered whole; no kernel ran
    REFUSED = "refused"  # the lane refused; the artifact is live and the durable tier is not

# --- [MODELS] ---------------------------------------------------------------------------


class SemanticHeader(Struct, frozen=True, gc=False):
    schema: str = ""
    project: str = ""


# a B-rep model holds no IFC schema.
SEMANTIC_EMPTY: Final[SemanticHeader] = SemanticHeader()

# element stride between TESSELLATION pulse beats: coarse enough that a beat costs one lossy queue write per
# window, fine enough that a long IFC iteration reads as live progress rather than silence.
PULSE_STRIDE: Final[int] = 64


@tagged_union(frozen=True)
class TessellationSource:
    tag: SourceTag = tag()
    ifc: bytes = case()
    cad: tuple[bytes, BridgeFormat] = case()


class SpillHeader(Struct, frozen=True, gc=False):
    # what the durable tier carries BESIDE the octets so a replay is indistinguishable from a fresh iteration: the
    # artifact's own path, the wire key the re-mint proves those octets against, the producer literal the carrier
    # declares, and the counts and semantic header the receipt floor answers. Spilling the GLB alone would force
    # every cold replay to re-open it just to re-count triangles the producer already knew, and would leave the
    # replayed carrier asserting a producer nothing recorded.
    artifact: str
    wire_key: str
    producer: Literal["ifc", "cad", "reconstruction"]
    element_count: int
    triangle_count: int
    semantic: SemanticHeader = SEMANTIC_EMPTY


# one module-level codec pair over the SOURCE header; deterministic order so an identical header spills identical
# octets and a fleet peer's write-once refusal names the same object rather than a differently-serialized twin.
_SPILL_ENCODER: Final = msgjson.Encoder(order="deterministic")
_SPILL_DECODER: Final = msgjson.Decoder(SpillHeader)


class TessellationResult(Struct, frozen=True, gc=False):
    content_key: ContentKey  # the policy-folded SOURCE cache key; the GLB wire key rides `glb.wire_key`
    glb: GlbArtifact
    element_count: int
    triangle_count: int
    semantic: SemanticHeader
    replay: Phase = "emitted"  # "admitted" = by-reference replay (session cache or durable tier), "emitted" = fresh iteration
    spill: SpillOutcome = SpillOutcome.UNBOUND

    def fact(self, source: SourceTag) -> Receipt:
        return Receipt.of(
            "rasm.geometry.mesh.daemon",
            (
                self.replay,
                source,
                {
                    "content_key": self.content_key.hex,
                    "elements": self.element_count,
                    "triangles": self.triangle_count,
                    # the durable-tier state rides the SAME row as the crossing it describes: a second receipt
                    # family for the spill would leave a reader joining two streams to answer one question.
                    "spill": self.spill.value,
                },
            ),
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def spill_path(kind: SpillKind, key: "ContentKey | bytes") -> str:
    # the ONE store-key layout the daemon spill and the `mesh/serve` read-through both derive from, so neither end
    # can address an object the other never wrote. Modality is the argument's own shape: a producer holds the
    # `ContentKey`, the serve leg holds the raw `artifact_id` memory bytes an `ArtifactFrame` already carries, and
    # both resolve to ONE path. The text is the key's 16-byte MEMORY projection rendered hex — the exact bytes that
    # id carries — so the two ends cannot disagree about byte order and serve derives no second key spelling.
    return f"{kind.value}/{(key if isinstance(key, bytes) else key.memory).hex()}"


def _proven(key: ContentKey, header: SpillHeader, glb: GlbArtifact) -> RuntimeRail[TessellationResult]:
    # the re-mint IS the proof the store returned the octets the header names — `GlbArtifact.of` runs the same
    # seed-zero fold the encoding site ran — so a corrupted or truncated object refuses naming both keys rather
    # than being replayed under an identity it does not hash to. `replay` derives at `_phase`, never here.
    return (
        Ok(TessellationResult(key, glb, header.element_count, header.triangle_count, header.semantic, spill=SpillOutcome.REPLAYED))
        if glb.wire_key.hex == header.wire_key
        else Error(BoundaryFault(resource=(spill_path(SpillKind.SOURCE, key), f"wire-key:{header.wire_key}!={glb.wire_key.hex}")))
    )


def _phase(result: TessellationResult, warm: Map[ContentKey, TessellationResult], key: ContentKey) -> TessellationResult:
    # replay provenance across BOTH reuse tiers in ONE derivation: a pre-drain session-cache key and a durable
    # read-through are each a by-reference answer, and only a unit that actually ran the kernel is `emitted`. The
    # census fold and the returned stream read this same fold, because two sites deriving the phase separately
    # publish two provenances for one crossing and the C# artifact index believes whichever it read first.
    return replace(result, replay="admitted" if key in warm or result.spill is SpillOutcome.REPLAYED else "emitted")


def _settings(mesher: TessellationPolicy) -> "ifcopenshell.geom.settings":
    s = ifcopenshell.geom.settings()
    s.set("mesher-linear-deflection", mesher.deflection)
    s.set("mesher-angular-deflection", mesher.angle_tolerance)
    s.set("weld-vertices", True)
    s.set("apply-default-materials", True)
    return s


# `serializers.gltf` is a `WriteOnlyGeometrySerializer` with a FILENAME sink only — never the OBJ/SVG buffer cast — so the GLB
# rides a scoped temp path read back through `Path.read_bytes`; `use-element-guids` names each glTF node off the IFC GlobalId.
def _tessellate_ifc(source_bytes: bytes, mesher: TessellationPolicy, num_threads: int, tap: "Queue[PulseFact | None]") -> KernelYield:
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
                if elements % PULSE_STRIDE == 0:  # lossy stride beat: the kernel's whole observability reach is this proxy write
                    pulsed(tap, GeometryPulse.TESSELLATION, PulseBeat(stage="iterate", done=elements))
                if not iterator.next():
                    break
        serializer.finalize()
        return GlbArtifact.of(Path(glb_path).read_bytes(), "ifc"), header, elements, triangles


# bridge's contributor-free `glb` view (a live contributor cannot cross the pickle seam) already carries its own
# wire-keyed artifact, so this arm re-keys nothing; the tally is `(1, 0)` — one assembly root, per-element count
# deferred to the bridge receipt.
def _tessellate_cad(source_bytes: bytes, fmt: BridgeFormat, mesher: TessellationPolicy, _num_threads: int, tap: "Queue[PulseFact | None]") -> KernelYield:
    # re-raising `RuntimeError(detail)` keeps the `step-bridge.<stage>` classification across the pickle seam under the
    # fidelity latch rather than degrading to a bare `"RuntimeError"`; the lane's `async_boundary` lands it in the fault case.
    pulsed(tap, GeometryPulse.TESSELLATION, PulseBeat(stage=f"bridge.{fmt.value}", done=0, total=1))  # one beat per opaque bridge hop
    match StepBridge.tessellate(source_bytes, fmt, mesher):
        case Result(tag="ok", ok=artifact):
            return artifact, SEMANTIC_EMPTY, 1, 0
        case Result(tag="error", error=fault):
            facts = fault.facts()
            raise RuntimeError(str(facts.get("detail") or facts.get("subject") or fault.tag))


# kernel, cache-key seed FIELDS, and plain `*args` per case — the `cad` arm carries its `BridgeFormat` as its own
# field (identical bytes declared as two formats are two tessellations, never one cache slot) and threads it
# positionally. The fields stay SEPARATE rather than concatenated with a separator byte: a hand-rolled delimiter is
# spoofable by a payload containing it and forks the framing width the identity owner already fixes, so the seed
# rides `IdentitySource(parts=...)` and the length-and-count framing runs at its one owner.
def _dispatch(source: TessellationSource) -> tuple[TessellateKernel, tuple[bytes, ...], tuple[object, ...]]:
    match source:
        case TessellationSource(tag="ifc", ifc=body):
            return _tessellate_ifc, (body,), (body,)
        case TessellationSource(tag="cad", cad=(body, fmt)):
            return _tessellate_cad, (fmt.value.encode(), body), (body, fmt)
        case _ as unreachable:
            assert_never(unreachable)


# --- [SERVICES] -------------------------------------------------------------------------


class TessellationDaemon:  # structural ReceiptContributor conformance — no subclass
    def __init__(
        self, lane: LanePolicy, mesher: TessellationPolicy = CANONICAL_TESSELLATION, *, store: "Option[ObjectStoreLane]" = Nothing
    ) -> None:
        self._lane = lane
        self._mesher = mesher
        self._receipts: Block[Receipt] = Block.empty()
        self._cache: Map[ContentKey, TessellationResult] = Map.empty()
        # the durable second tier arrives BUILT, as a composition parameter: store identity, credentials, retry
        # disposition, and reach are app-composition inputs, and a daemon-local `from_url` mint is the composer the
        # transport owner's own boundary rejects. `Nothing` is the honest cache-only daemon, never a default lane
        # this folder would have to invent a root for.
        self._store = store

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
        results = units.choose(lambda a: self._cache.try_find(a[0]).map(lambda r: _phase(r, warm, a[0])))
        return faults.try_head().map(Error).default_value(Ok(results))

    # railed `Error` carries the key-mint `BoundaryFault` the fold surfaces as a `rejected` receipt.
    def _admit(self, source: TessellationSource) -> RuntimeRail[tuple[ContentKey, SourceTag, "Admit[TessellationResult]"]]:
        # the mesher spec and the source's own fields are SEMANTIC fields of one preimage, so they ride `parts` and
        # the identity owner frames each with its own length: concatenated, a knob value ending where a body begins
        # collides two distinct tessellations onto one cache slot, and the slot silently replays the wrong artifact.
        kernel, seed, args = _dispatch(source)
        return ContentIdentity.of(source.tag, IdentitySource(parts=(self._mesher.spec, *seed))).map(
            lambda key: self._unit(kernel, args, key, source.tag)
        )

    def _unit(
        self, kernel: TessellateKernel, args: tuple[object, ...], key: ContentKey, tag: SourceTag
    ) -> tuple[ContentKey, SourceTag, "Admit[TessellationResult]"]:
        async def work() -> RuntimeRail[TessellationResult]:
            # durable tier BEFORE the kernel: a warm restart or a fleet peer already holds this exact unit, so the
            # read-through is the cheapest answer and the offload the fall-through. HOSTILE routes the native OCCT
            # body onto the warm process pool, its trait-default WORKER row retrying a transient worker death while
            # the unit stays content-keyed for the cache short-circuit; the trailing `mesher`/`num_threads`/`tap`
            # are positional kernel offload args, the tap the lane conduit's pickled proxy.
            match await self._replayed(key):
                case Option(tag="some", some=held):
                    return Ok(held)
                case _:
                    offloaded = await self._lane.offload(
                        Kernel.of(kernel, KernelTrait.HOSTILE), *args, self._mesher, self._lane.capacity, self._lane.pulses.tap
                    )
                    match offloaded.map(lambda y: TessellationResult(key, y[0], y[2], y[3], y[1])):
                        case Result(tag="ok", ok=fresh):
                            return Ok(await self._spilled(fresh))
                        case Result(tag="error") as refused:
                            return refused

        return key, tag, Admit(keyed=(key, work))

    async def _replayed(self, key: ContentKey) -> "Option[TessellationResult]":
        # the durable read-through. This tier is an OPTIMIZATION, never a correctness dependency, so every failure
        # mode folds to `Nothing` and the kernel runs: the cost of a miss is a tessellation nobody skipped, where a
        # rail here would be a tessellation nobody GOT because a store was briefly unreachable. The refusal still
        # lands on the receipt stream, so an outage reads as evidence rather than as a daemon that quietly stopped
        # replaying and got slower for a week.
        match self._store:
            case Option(tag="some", some=lane):
                held = await self._fetched(lane, key)
                held.swap().map(self._noted)
                return held.to_option()
            case _:
                return Nothing

    async def _fetched(self, lane: "ObjectStoreLane", key: ContentKey) -> RuntimeRail[TessellationResult]:
        # two reads, pointer then octets: the SOURCE header resolves a policy-folded cache key onto the artifact's
        # own path, and the ARTIFACT object answers its bytes. The re-mint PROVES the store returned what the header
        # names — `GlbArtifact.of` is the same seed-zero fold the encoding site ran — so a corrupted or truncated
        # object refuses by name instead of being served under a key it does not hash to. The decode runs fenced,
        # because a malformed header is foreign material and a raise here would escape the read-through whole.
        pointer = await lane.run_async(StoreOp.Get(spill_path(SpillKind.SOURCE, key)))
        match pointer.bind(lambda outcome: boundary("mesh.daemon.spill", lambda: _SPILL_DECODER.decode(bytes(outcome.source)))):
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=header):
                fetched = await lane.run_async(StoreOp.Get(header.artifact))
                return fetched.bind(lambda outcome: _proven(key, header, GlbArtifact.of(bytes(outcome.source), header.producer)))

    async def _spilled(self, result: TessellationResult) -> TessellationResult:
        # write-once put of both objects under one derivation: the octets under the artifact's own wire key, the
        # header under the policy-folded cache key. `create` is the mode because the store is CONTENT-ADDRESSED —
        # an object already under a key holds those same octets, so an overwrite buys a race with a fleet peer and
        # nothing else. The header lands LAST and only past a cleared artifact write, so no reader ever resolves a
        # pointer onto octets that are not there yet.
        match self._store:
            case Option(tag="some", some=lane):
                artifact = spill_path(SpillKind.ARTIFACT, result.glb.wire_key)
                header = SpillHeader(
                    artifact, result.glb.wire_key.hex, result.glb.producer, result.element_count, result.triangle_count, result.semantic
                )
                landed = await lane.run_async(StoreOp.Put(result.glb.bytes, mode="create"), artifact)
                sealed = (
                    await lane.run_async(StoreOp.Put(_SPILL_ENCODER.encode(header), mode="create"), spill_path(SpillKind.SOURCE, result.content_key))
                    if landed.is_ok()
                    else landed
                )
                sealed.swap().map(self._noted)
                return replace(result, spill=SpillOutcome.LANDED if sealed.is_ok() else SpillOutcome.REFUSED)
            case _:
                return result

    def _noted(self, fault: BoundaryFault) -> None:
        # a durable-tier refusal is EVIDENCE, never a rail: the artifact is live either way, so the fault lands on
        # the receipt stream where an operator reads a store outage, instead of failing a tessellation that
        # succeeded or vanishing into a daemon that silently pays full cost forever. The `rejected` projection is
        # the same one an admission fault takes, so the page carries ONE refusal vocabulary.
        self._receipts = self._receipts.append(Block.singleton(Receipt.of("rasm.geometry.mesh.daemon", fault)))

    # a PRE-drain key projects `admitted`, an absent key `emitted`; every fault projects the `rejected` case.
    def _fold(
        self,
        admitted: Block[tuple[ContentKey, SourceTag, Admit[TessellationResult]]],
        warm: Map[ContentKey, TessellationResult],
        faults: Block[BoundaryFault],
    ) -> None:
        facts = admitted.choose(lambda a: self._cache.try_find(a[0]).map(lambda r: _phase(r, warm, a[0]).fact(a[1])))
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
