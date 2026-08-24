# [PY_GEOMETRY_MESH_DAEMON]

One persistent IfcOpenShell tessellation daemon resolves the generated IFC source reference through the branch's one artifact repository, materializes it onto a helper-owned input path, and writes GLB onto a helper-owned output path. The process seam carries request binary, path strings, bounded scalars, pulse proxy, and small census evidence only. Source bytes, GLB bytes, temporary-path custody, and hand-authored wire twins never cross through pickle.

Every request enters `LanePolicy.drain` under a source key derived from the generated `ArtifactRef` identity and extent. Its policy-folded `ContentKey` writes each generated field coordinate before its value, so the C# requester can reproduce the same preimage without protobuf serialization, language-local format strings, or default seeds. The output is published before a receipt exposes its `ArtifactRef`; the source-index header remains a post-publication replay optimization whose refusal cannot erase a resolvable artifact.

## [01]-[INDEX]

- [02]-[DAEMON]: Reference-resolved IFC tessellation, canonical request identity, required artifact publication, source-index replay, and drain-on-harvest receipts.

## [02]-[DAEMON]

- Owner: `TessellationDaemon` owns one required `ArtifactRepository` over one injected `ObjectStoreLane`; `GeometryServe` reaches that same repository through `daemon.repository` and cannot bind another store.
- Law: `rasm.contracts.artifact.stage` owns source materialization and proof, while `output(suffix=".glb")` owns the worker's format-bearing output path and `ArtifactSink.seal` mints the generated output reference. The worker admits the extension-neutral input with IfcOpenShell's explicit `.ifc` format and writes the helper-owned GLB path; no `Path.read_bytes`, raw-body process argument, or GLB process return exists.
- Law: artifact publication is correctness-mandatory and uses the repository's path-backed atomic overwrite. A publication refusal rails the tessellation, so no unresolved reference can escape. Overwrite is safe because the helper proved the path's SHA-256 identity and the destination is derived from that identity; seed-zero XXH3 remains confined to the source and content cache keys.
- Law: the source-index header is the cache optimization beneath `LanePolicy`'s session cache. It carries the generated artifact reference, generated semantic binary, and exact census. Header create-or-match refusal becomes `Spill.REFUSED` evidence after artifact publication; replay streams the referenced artifact through the same extent/hash proof as `Fetch` before admitting the cached receipt.
- Law: source identity writes the generated `TessellateRequest.source_artifact`, `ArtifactRef.sha256`, and `ArtifactRef.artifact_bytes` coordinates. Policy identity then writes every output-affecting generated field and nested coordinate in contract order.
- Entry: `tessellate` returns admission-ordered `TessellationResult` values and drains facts through `contribute`. The caller budget rides the kernel deadline and never enters content identity.
- Auto: each IFC unit admits as `Admit.whole`; `LaneGrant.width` becomes IfcOpenShell `num_threads`, so outer admission and native parallelism spend one allocator.
- Receipt: `TessellationResult` carries a generated `ArtifactRef`, never a local artifact carrier or octets. `TessellateResponse` projection belongs to `mesh/serve`.
- Packages: `ifcopenshell`, `trimesh`, `rasm.contracts.artifact`, generated compute/geometry/artifact messages, and runtime identity/lane/store/journal rails.
- Growth: a new output-affecting contract field lands in the canonical coordinate stream and provider projection together. A new artifact transport rule belongs to `rasm.contracts.artifact`; this page composes it and authors no parallel integrity state machine.
- Boundary: IFC only. STEP, IGES, sealed B-rep, and OCCT exchange belong to generated `CadService` and the isolated CAD package.

```python signature
from collections.abc import AsyncGenerator, Awaitable, Callable, Iterable, Sequence
from builtins import frozendict
from enum import StrEnum
from functools import partial
from inspect import isasyncgenfunction
from pathlib import Path
from queue import Queue
from typing import Final, Literal, assert_never

from expression import Error, Ok, Option, Result, Some, effect
from expression.collections import Block, Map
from expression.extra.result import sequence
import msgspec
from msgspec import Struct
from msgspec import json as msgjson
from msgspec.structs import replace
from protobuf import Message, Oneof
from rasm.contracts.artifact import ArtifactError, ArtifactSink, OwnedArtifact, output, rendered, stage
from rasm.contracts.gen.rasm.contracts.artifact.artifact_pb import ArtifactRef
from rasm.contracts.gen.rasm.contracts.compute.compute_pb import (
    GeomSetting,
    Semantic,
    Spill,
    TessellateRequest,
    TessellationScope,
)
from rasm.contracts.gen.rasm.contracts.geometry.tessellation_pb import TessellationPolicy

from rasm.geometry.graduation import EVIDENCE_DOMAIN, GeometryLeg, GeometryPulse
from rasm.runtime.faults import TERMINAL, BoundaryFault, Catch, FaultRow, RuntimeRail, boundary, rostered
from rasm.runtime.shapes import custody
from rasm.runtime.hooks import StageMark
from rasm.runtime.identity import CanonicalWriter, ContentIdentity, ContentKey
from rasm.runtime.journal import Actor, Assigned, AuditFact, Fact, Journal, MeterFact, Party, Resource, Retain
from rasm.runtime.lanes import Admit, LaneGrant, LanePolicy, PulseFact, pulsed
from rasm.runtime.receipts import DEFAULT_SCOPE, Phase, Receipt, ScopeKey
from rasm.runtime.roots import ObjectStoreLane, StoreFault, StoreOp, StoreOutcome, StoreStream
from rasm.runtime.workers import Kernel, KernelTrait

lazy import ifcopenshell
lazy import ifcopenshell.geom
lazy import ifcopenshell.ifcopenshell_wrapper
lazy import trimesh


type ProducerTag = Literal["ifc"]
type KernelYield = tuple[str, str, int, int]
type TessellateKernel = Callable[..., KernelYield]

# the two custody body shapes `ArtifactRepository.opened` discriminates on: a rail-returning body takes the held arm
# and an async-generator body takes the streamed arm, so the modality is recoverable from the value itself.
type Held[T] = Callable[[OwnedArtifact], Awaitable[RuntimeRail[T]]]
type Streamed[T] = Callable[[OwnedArtifact], AsyncGenerator[T]]


class SpillKind(StrEnum):
    ARTIFACT = "artifact"
    SOURCE = "source"


class TessellationStage(StrEnum):
    ITERATE = "iterate"


OWNER: Final[str] = "rasm.geometry.mesh.daemon"
PULSE_STRIDE: Final[int] = 64
IFC_SETTING_KEYS: Final = frozendict({
    GeomSetting.WELD: "weld-vertices",
    GeomSetting.WORLD_COORDS: "use-world-coords",
    GeomSetting.DEFAULT_MATERIALS: "apply-default-materials",
    GeomSetting.GENERATE_UVS: "generate-uvs",
    GeomSetting.DISABLE_OPENING_SUBTRACTIONS: "disable-opening-subtractions",
    GeomSetting.ELEMENT_GUIDS: "use-element-guids",
})


class SpillHeader(Struct, frozen=True, gc=False):
    artifact: bytes
    element_count: int
    triangle_count: int
    semantic: bytes


class TessellationResult(Struct, frozen=True, gc=False):
    content_key: ContentKey
    artifact: ArtifactRef
    element_count: int
    triangle_count: int
    semantic: Semantic
    replay: Phase = "emitted"
    spill: Spill = Spill.UNBOUND

    def fact(self, source: ProducerTag) -> Receipt:
        return Receipt.of(
            OWNER,
            (
                self.replay,
                source,
                {
                    "content_key": self.content_key.hex,
                    "artifact": self.artifact.sha256.hex(),
                    "elements": self.element_count,
                    "triangles": self.triangle_count,
                    "spill": self.spill.value,
                },
            ),
        )


_SPILL_ENCODER: Final = msgjson.Encoder(order="deterministic")
_SPILL_DECODER: Final = msgjson.Decoder(SpillHeader)
_HEADER_RAISES: Final[Catch] = (msgspec.ValidationError, msgspec.DecodeError, ValueError)

DAEMON_SPILL: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.DAEMON, point="spill", arm="boundary", defect="header-undecodable", retriability=TERMINAL
)
DAEMON_REPLAY: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.DAEMON,
    point="replay",
    arm="resource",
    defect="artifact-unavailable",
    retriability=TERMINAL,
    slots=("artifact",),
)
DAEMON_COLLISION: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.DAEMON,
    point="spill",
    arm="resource",
    defect="content-mismatch",
    retriability=TERMINAL,
    slots=("object", "resident_fingerprint", "proposed_fingerprint"),
)
DAEMON_ARTIFACT: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.DAEMON,
    point="artifact",
    arm="boundary",
    defect="integrity-refused",
    retriability=TERMINAL,
    slots=("proof",),
)
DAEMON_BUDGET: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.DAEMON,
    point="tessellate",
    arm="resource",
    defect="triangle-budget-exceeded",
    retriability=TERMINAL,
    slots=("measured", "budget"),
)
DAEMON_SLOT: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.DAEMON,
    point="admit",
    arm="boundary",
    defect="required-slot-absent",
    retriability=TERMINAL,
    slots=("coordinate",),
)
RAISES: Final[Block[FaultRow[GeometryLeg]]] = rostered(
    Block.of_seq([DAEMON_SPILL, DAEMON_REPLAY, DAEMON_COLLISION, DAEMON_ARTIFACT, DAEMON_BUDGET, DAEMON_SLOT])
)


def spill_path(kind: SpillKind, key: "ContentKey | bytes") -> str:
    projected = key if isinstance(key, bytes) else key.wire_bytes
    return f"{kind.value}/{projected.hex()}"


async def _store_chunks(stream: StoreStream) -> AsyncGenerator[bytes]:
    while True:
        match await stream.pull():
            case Result(tag="ok", ok=Option(tag="some", some=chunk)):
                yield chunk
            case Result(tag="ok", ok=Option(tag="none")):
                return
            case Result(tag="error", error=fault):
                raise fault
            case unreachable:
                assert_never(unreachable)


async def _witnessed(_owned: OwnedArtifact, /) -> RuntimeRail[bool]:
    # the custody body a presence probe needs: reaching it IS the proof, so the probe carries no second read.
    # Named apart from the slot probe `_present` below — two concepts, two names; the shared spelling let the
    # later module-scope definition shadow this one at call time with the wrong arity.
    return Ok(True)


def _absent(fault: BoundaryFault, /) -> RuntimeRail[bool]:
    # a store MISS is the answer this probe exists to give and never a refusal; every other fault stays the fault.
    match fault:
        case BoundaryFault(tag="domain", domain=(_, StoreFault(tag="missing"))):
            return Ok(False)
        case _:
            return Error(fault)


class ArtifactRepository:
    def __init__(self, lane: ObjectStoreLane) -> None:
        self._lane = lane

    @staticmethod
    def path(sha256: bytes) -> str:
        return spill_path(SpillKind.ARTIFACT, sha256)

    @property
    def lane(self) -> ObjectStoreLane:
        return self._lane

    async def put(self, owned: OwnedArtifact) -> RuntimeRail[ArtifactRef]:
        stored = await self._lane.run_async(StoreOp.Put(owned.path), self.path(owned.artifact.sha256))
        return stored.map(lambda _outcome: owned.artifact)

    async def verified(self, artifact: ArtifactRef) -> RuntimeRail[bool]:
        return (await self.opened(artifact, _witnessed)).or_else_with(_absent)

    def opened[T](self, expected: ArtifactRef | bytes, use: Held[T] | Streamed[T], /) -> Awaitable[RuntimeRail[T]] | AsyncGenerator[T]:
        # the ONE artifact-custody entry, and the rail never leaves the rail: the resolve, the staged proof, and the
        # claim proof (`claim=expected` — identity for a bare digest, identity AND extent for a full reference) all
        # land inside, so no consumer re-rails a raise or re-confirms a reference by hand. The deleted
        # `@asynccontextmanager` form un-railed a rail — it raised the store's own `BoundaryFault` out of a generator
        # that could not carry one — and every one of its four consumers paid the same two `except` arms to undo it.
        # Modality discriminates on the SHAPE of `use`, the branch's own `trapped` law: a rail-returning body takes
        # the held arm, an async-generator body takes the streamed arm, and no `stream: bool` knob restates what the
        # value already answers. `isasyncgenfunction` unwraps a `functools.partial`, so a bound held body still reads
        # as the coroutine it wraps and the `partial` every caller threads its captures through costs no arm.
        sha256 = expected if isinstance(expected, bytes) else expected.sha256
        return self._streamed(expected, sha256, use) if isasyncgenfunction(use) else self._held(expected, sha256, use)

    @custody(DAEMON_ARTIFACT)
    async def _held[T](self, expected: ArtifactRef | bytes, sha256: bytes, use: Held[T], /) -> RuntimeRail[T]:
        match await self._lane.run_async(StoreOp.Stream(self.path(sha256))):
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=StoreOutcome(payload=StoreStream() as stream)):
                try:
                    # `claim=expected` hands the WHOLE claim to the seal: a bare digest proves identity, a full
                    # reference proves identity and extent, and the refusal rides the yielded `Result` — the
                    # discarded hand `confirm` this replaces proved nothing.
                    async with stage(_store_chunks(stream), claim=expected) as sealed:
                        match sealed:
                            case Result(tag="ok", ok=owned):
                                return await use(owned)
                            case Result(tag="error", error=refusal):
                                return Error(DAEMON_ARTIFACT.raised(rendered(refusal)))
                            case _ as unreachable:
                                assert_never(unreachable)
                except BoundaryFault as refused:
                    # Exemption: `_store_chunks` feeds an async-iterator protocol that carries no rail, so the store's
                    # own typed fault crosses it as a raise and is re-railed HERE, once, for every consumer.
                    return Error(refused)
            case _ as unreachable:
                assert_never(unreachable)

    async def _streamed[T](self, expected: ArtifactRef | bytes, sha256: bytes, use: Streamed[T], /) -> AsyncGenerator[T]:
        # the server-stream arm: the custody bracket stays open across every yield, which is exactly why the held
        # arm cannot serve it and why the discriminant is the body's own shape rather than a second public entry.
        match await self._lane.run_async(StoreOp.Stream(self.path(sha256))):
            case Result(tag="error", error=fault):
                raise fault
            case Result(tag="ok", ok=StoreOutcome(payload=StoreStream() as stream)):
                async with stage(_store_chunks(stream), claim=expected) as sealed:
                    match sealed:
                        case Result(tag="ok", ok=owned):
                            async for item in use(owned):
                                yield item
                        case Result(tag="error", error=refusal):
                            # Exemption: the generator arm carries no rail, so a seal refusal crosses it as the
                            # library's own egress carrier and the serve edge re-rails it once.
                            raise ArtifactError(refusal)
                        case _ as unreachable:
                            assert_never(unreachable)
            case _ as unreachable:
                assert_never(unreachable)


def _decoded_header(octets: bytes) -> tuple[SpillHeader, ArtifactRef, Semantic]:
    header = _SPILL_DECODER.decode(octets)
    return header, ArtifactRef.from_binary(header.artifact), Semantic.from_binary(header.semantic)


def _result(key: ContentKey, header: SpillHeader, artifact: ArtifactRef, semantic: Semantic) -> TessellationResult:
    return TessellationResult(
        key,
        artifact,
        header.element_count,
        header.triangle_count,
        semantic,
        spill=Spill.REPLAYED,
    )


async def _created(lane: ObjectStoreLane, path: str, payload: bytes) -> RuntimeRail[Spill]:
    created = await lane.run_async(StoreOp.Put(payload, mode="create"), path)
    if created.is_ok():
        return Ok(Spill.LANDED)
    resident = await lane.run_async(StoreOp.Get(path))
    return resident.bind(
        lambda outcome: Ok(Spill.MATCHED)
        if bytes(outcome.source) == payload
        else Error(
            DAEMON_COLLISION.raised(
                path,
                ContentIdentity.key("spill-collision", bytes(outcome.source)).hex,
                ContentIdentity.key("spill-collision", payload).hex,
            )
        )
    )


def _stored(path: str, quantity: int, change: tuple[Assigned, ...], state: Spill) -> Block[Fact]:
    if state is not Spill.LANDED:
        return Block.empty()
    return Block.of_seq(
        (
            AuditFact(
                action=f"{EVIDENCE_DOMAIN}.spill",
                actor=Party(kind=Actor.SERVICE, key=OWNER),
                target=Party(kind="object", key=path),
                retention=Retain.OPERATIONAL,
                change=change,
            ),
            MeterFact(resource=Resource.STORAGE, quantity=quantity, surface=path),
        )
    )


def _phase(result: TessellationResult, warm: Map[ContentKey, TessellationResult], key: ContentKey) -> TessellationResult:
    return replace(result, replay="admitted" if key in warm or result.spill is Spill.REPLAYED else "emitted")


def _present[T](slot: T | None, coordinate: str, /) -> RuntimeRail[T]:
    # `has_field` is the generator's own presence probe and a message slot reads `None` when unset; this is the one
    # place that reads it, so every fold past the admission holds a proved value.
    return Option.of_optional(slot).to_result_with(lambda: DAEMON_SLOT.raised(coordinate))


class TessellationUnit(Struct, frozen=True, gc=False):
    # every required slot of the generated request, PROVED PRESENT ONCE: `from_binary` runs no protovalidate, so the
    # decode carries no proof of its own and every fold downstream reads this frozen owner instead of casting away a
    # `None` nothing established. Growth is one slot beside one admitted line, and a required slot the corpus adds
    # breaks the admission loudly instead of surfacing as a worker crash.
    request: TessellateRequest
    policy: TessellationPolicy
    scope: TessellationScope
    kind: Oneof
    source: ArtifactRef

    @staticmethod
    @effect.result[TessellationUnit, BoundaryFault]()
    def of(request: TessellateRequest, /):
        # ONE admission over BOTH crossings — the parent's own value and the worker's `from_binary` decode — because
        # no envelope stands between the rpc verb and its payload and both crossings hand this one type, so nothing
        # here discriminates. Do-notation is earned on value re-use, never chain length: `scope` feeds `kind` and the
        # constructor both, the intermediate a point-free `pipeline` could not name.
        policy = yield from _present(request.policy, "policy")
        scope = yield from _present(request.scope, "scope")
        kind = yield from _present(scope.kind, "scope.kind")
        source = yield from _present(request.source_artifact, "source_artifact")
        return TessellationUnit(request=request, policy=policy, scope=scope, kind=kind, source=source)


def _selected(request: TessellateRequest) -> frozenset[GeomSetting]:
    return frozenset(request.geom_settings)


def _settings(mesher: TessellationPolicy, request: TessellateRequest) -> "ifcopenshell.geom.settings":
    selected = _selected(request)
    settings = ifcopenshell.geom.settings()
    settings.set("mesher-linear-deflection", mesher.deflection_m)
    settings.set("mesher-angular-deflection", mesher.angle_tolerance_rad)
    settings.set("precision", request.tolerance_m)
    settings.set("dimensionality", getattr(ifcopenshell.ifcopenshell_wrapper, request.dimensionality.name))
    for setting, key in IFC_SETTING_KEYS.items():
        if setting is not GeomSetting.ELEMENT_GUIDS:
            settings.set(key, setting in selected)
    return settings


def _serializer(request: TessellateRequest) -> "ifcopenshell.geom.serializer_settings":
    settings = ifcopenshell.geom.serializer_settings()
    settings.set(IFC_SETTING_KEYS[GeomSetting.ELEMENT_GUIDS], GeomSetting.ELEMENT_GUIDS in _selected(request))
    return settings


def _entities(model: "ifcopenshell.file", kinds: tuple[str, ...]) -> list["ifcopenshell.entity_instance"]:
    return list(frozendict((entity.id(), entity) for kind in kinds for entity in model.by_type(kind)).values())


def _scope(
    model: "ifcopenshell.file",
    scope: TessellationScope,
) -> "tuple[list[ifcopenshell.entity_instance] | list[str] | None, list[ifcopenshell.entity_instance] | None]":
    match scope.kind:
        case Oneof(field="whole_model"):
            return None, None
        case Oneof(field="elements", value=elements):
            return sorted(set(elements.global_ids)), None
        case Oneof(field="entities", value=entities):
            return _entities(model, tuple(sorted(set(entities.ifc_types)))), None
        case Oneof(field="exclude_entities", value=entities):
            return None, _entities(model, tuple(sorted(set(entities.ifc_types))))
        case unreachable:
            assert_never(unreachable)


def _census(path: Path) -> tuple[int, int]:
    scene = trimesh.load_scene(path, file_type="glb", process=False)
    parents = {child: parent for parent, child, _ in scene.graph.to_edgelist()}
    placements: set[object] = set()
    triangles = 0
    for node in scene.graph.nodes_geometry:
        _, name = scene.graph[node]
        geometry = scene.geometry[name]
        placements.add(parents[node] if geometry.metadata.get("from_gltf_primitive", False) else node)
    for geometry in scene.geometry.values():
        if isinstance(geometry, trimesh.Trimesh):
            triangles += len(geometry.faces)
    return len(placements), triangles


def _tessellate_ifc(
    payload: bytes,
    source_path: str,
    target_path: str,
    num_threads: int,
    tap: "Queue[PulseFact | None]",
) -> KernelYield:
    match TessellationUnit.of(TessellateRequest.from_binary(payload)):
        case Result(tag="error", error=refused):
            # Exemption: the worker floor carries no rail home, so an admitted refusal crosses as the raise
            # `execution/workers#CROSSING` lowers to data and re-mints parent-side — exactly as the budget refusal
            # below does. This is the only unproved decode in the fold and it is now proved before any native call.
            raise refused
        case Result(tag="ok", ok=unit):
            pass
        case _ as unreachable:
            assert_never(unreachable)
    mesher = unit.policy
    settings = _settings(mesher, unit.request)
    model = ifcopenshell.open(source_path, ".ifc")
    projects = model.by_type("IfcProject")
    schema = model.schema_identifier
    project = (projects[0].Name or "") if projects else ""
    include, exclude = _scope(model, unit.scope)
    serializer = ifcopenshell.geom.serializers.gltf(target_path, settings, _serializer(unit.request))
    serializer.setFile(model)
    serializer.setUnitNameAndMagnitude("METER", 1.0)
    serializer.writeHeader()
    iterator = ifcopenshell.geom.iterator(
        settings,
        model,
        num_threads=num_threads,
        include=include,
        exclude=exclude,
        geometry_library="hybrid-cgal-simple-opencascade",
    )
    emitted = 0
    if iterator.initialize():
        while True:
            serializer.write(iterator.get())
            emitted += 1
            if emitted % PULSE_STRIDE == 0:
                pulsed(tap, GeometryPulse.TESSELLATION, StageMark(stage=TessellationStage.ITERATE.value, done=emitted))
            if not iterator.next():
                break
    serializer.finalize()
    elements, triangles = _census(Path(target_path))
    if triangles > mesher.triangle_budget:
        raise DAEMON_BUDGET.raised(str(triangles), str(mesher.triangle_budget))
    return schema, project, elements, triangles


def _field_number(message: type[Message], local_name: str) -> int:
    return next(field.number for field in message.desc().fields if field.local_name == local_name)


def _source_key(unit: TessellationUnit) -> ContentKey:
    # Field NUMBERS this writer emits ARE the cross-language parity contract: the peer at
    # `dotnet:Rasm.Bim/Exchange/tessellation#TESSELLATION_BRIDGE@Plan` writes this identical ordinal chain, so a
    # coordinate added, dropped, or renumbered here forks the key silently unless that peer moves in the same change.
    source = unit.source
    return (
        CanonicalWriter()
        .ordinal(_field_number(TessellateRequest, "source_artifact"))
        .ordinal(_field_number(ArtifactRef, "sha256"))
        .raw(source.sha256)
        .ordinal(_field_number(ArtifactRef, "artifact_bytes"))
        .u64(source.artifact_bytes)
        .key("tessellation-source")
    )


def _scope_tokens(kind: Oneof) -> RuntimeRail[tuple[int, tuple[str, ...]]]:
    match kind:
        case Oneof(field="whole_model"):
            return Ok((0, ()))
        case Oneof(value=Message() as payload):
            field = payload.desc().fields[0]
            values = payload[field]
            # the descriptor names a repeated string column; the shape probe REFUSES anything else rather than
            # asserting it away, so a corpus edit that re-types the column breaks here instead of inside the digest.
            return (
                Ok((field.number, tuple(sorted(set(values)))))
                if isinstance(values, Sequence) and not isinstance(values, (str, bytes))
                else Error(DAEMON_SLOT.raised(f"scope.{field.local_name}"))
            )
        case _ as unreachable:
            assert_never(unreachable)


def _content_key(unit: TessellationUnit) -> RuntimeRail[ContentKey]:
    request, policy = unit.request, unit.policy
    writer = CanonicalWriter().u128(_source_key(unit).value)
    writer.ordinal(_field_number(TessellateRequest, "policy"))
    writer.ordinal(_field_number(TessellationPolicy, "deflection_m")).double(policy.deflection_m)
    writer.ordinal(_field_number(TessellationPolicy, "angle_tolerance_rad")).double(policy.angle_tolerance_rad)
    writer.ordinal(_field_number(TessellationPolicy, "triangle_budget")).u64(policy.triangle_budget)
    writer.ordinal(_field_number(TessellateRequest, "tolerance_m")).double(request.tolerance_m)
    settings = tuple(sorted(set(map(int, request.geom_settings))))
    writer.ordinal(_field_number(TessellateRequest, "geom_settings")).ordinal(len(settings))
    for setting in settings:
        writer.ordinal(setting)
    writer.ordinal(_field_number(TessellateRequest, "dimensionality")).ordinal(int(request.dimensionality))
    writer.ordinal(_field_number(TessellateRequest, "scope")).ordinal(_field_number(TessellationScope, unit.kind.field))
    return _scope_tokens(unit.kind).map(lambda framed: _framed(writer, framed).key("tessellation"))


def _framed(writer: CanonicalWriter, framed: tuple[int, tuple[str, ...]]) -> CanonicalWriter:
    # Exemption: `CanonicalWriter` is an ordered imperative sink whose count-then-member framing the estate preimage
    # law fixes; the loop is that framing and carries no branch beyond the whole-model arm's zero token field.
    token_field, tokens = framed
    if token_field:
        writer.ordinal(token_field).ordinal(len(tokens))
        for token in tokens:
            writer.string(token)
    return writer


def _dispatch(unit: TessellationUnit) -> RuntimeRail[tuple[TessellateKernel, tuple[object, ...], ContentKey, ProducerTag, ArtifactRef]]:
    return _content_key(unit).map(lambda key: (_tessellate_ifc, (unit.request.to_binary(),), key, "ifc", unit.source))


class TessellationDaemon:
    def __init__(
        self,
        lane: LanePolicy,
        repository: ArtifactRepository,
        *,
        composition: ScopeKey = DEFAULT_SCOPE,
    ) -> None:
        self._lane = lane
        self._repository = repository
        self._composition = composition
        self._receipts: Block[Receipt] = Block.empty()
        self._cache: Map[ContentKey, TessellationResult] = Map.empty()

    @property
    def repository(self) -> ArtifactRepository:
        return self._repository

    async def tessellate(
        self,
        request: TessellateRequest | Sequence[TessellateRequest],
        *,
        budget: "Option[float]",
    ) -> RuntimeRail[Block[TessellationResult]]:
        # arity absorbs at the head and admission runs BEFORE the drain: a malformed unit refuses the whole batch
        # rather than reaching the lane as a half-proved row the worker would crash on.
        warm = self._cache
        railed = (Block.singleton(request) if isinstance(request, TessellateRequest) else Block.of_seq(request)).map(
            lambda one: self._admit(one, budget)
        )
        match sequence(railed):
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=units):
                pass
            case _ as unreachable:
                assert_never(unreachable)
        receipt = await self._lane.drain(units.map(lambda admitted: admitted[2]), warm)
        self._cache = receipt.cache
        self._fold(units, warm, receipt.faults)
        results = units.choose(lambda admitted: self._cache.try_find(admitted[0]).map(lambda held: _phase(held, warm, admitted[0])))
        return receipt.faults.try_head().map(Error).default_value(Ok(results))

    def _admit(
        self,
        request: TessellateRequest,
        budget: "Option[float]",
    ) -> RuntimeRail[tuple[ContentKey, ProducerTag, Admit[TessellationResult]]]:
        return TessellationUnit.of(request).bind(_dispatch).map(lambda row: self._unit(*row, budget))

    def _unit(
        self,
        kernel: TessellateKernel,
        args: tuple[object, ...],
        key: ContentKey,
        tag: ProducerTag,
        source: ArtifactRef,
        budget: "Option[float]",
    ) -> tuple[ContentKey, ProducerTag, Admit[TessellationResult]]:
        async def work(grant: LaneGrant) -> RuntimeRail[TessellationResult]:
            # the replay arm short-circuits and every remaining step is one rail hop. `opened` holds the source
            # custody AND its refusal, so this body carries no `try`, no second context manager for the source, and
            # none of the three nested `match` ladders the deleted four-deep form needed to say the same thing.
            match await self._replayed(key):
                case Option(tag="some", some=held):
                    return Ok(held)
                case Option(tag="none"):
                    return await self._repository.opened(source, partial(self._emitted, kernel, args, key, grant, budget))
                case _ as unreachable:
                    assert_never(unreachable)

        return key, tag, Admit(whole=(Some(key), work))

    async def _emitted(
        self,
        kernel: TessellateKernel,
        args: tuple[object, ...],
        key: ContentKey,
        grant: LaneGrant,
        budget: "Option[float]",
        resolved: OwnedArtifact,
        /,
    ) -> RuntimeRail[TessellationResult]:
        async with output(suffix=".glb") as sink:
            offloaded = await self._lane.offload(
                Kernel.of(kernel, KernelTrait.HOSTILE, deadline=budget),
                *args,
                str(resolved.path),
                str(sink.path),
                grant.width,
                self._lane.pulses.tap,
            )
            match offloaded:
                case Result(tag="error") as refused:
                    return refused
                case Result(tag="ok", ok=(schema, project, elements, triangles)):
                    return await self._published(key, sink, Semantic(schema=schema, project=project), elements, triangles)
                case _ as unreachable:
                    assert_never(unreachable)

    async def _published(
        self, key: ContentKey, sink: ArtifactSink, semantic: Semantic, elements: int, triangles: int, /
    ) -> RuntimeRail[TessellationResult]:
        # publication is correctness-mandatory, so the seal and the put share one arm and no unresolved reference
        # can escape onto a receipt; the seal RAILS, so its refusal lands here as the custody row's proof token
        # rather than crossing into `put` as a `Result` wearing an artifact's shape.
        match await sink.seal():
            case Result(tag="error", error=refusal):
                return Error(DAEMON_ARTIFACT.raised(rendered(refusal)))
            case Result(tag="ok", ok=owned):
                match await self._repository.put(owned):
                    case Result(tag="error") as refused:
                        return refused
                    case Result(tag="ok", ok=artifact):
                        return Ok(await self._indexed(TessellationResult(key, artifact, elements, triangles, semantic)))
                    case _ as unreachable:
                        assert_never(unreachable)
            case _ as unreachable:
                assert_never(unreachable)

    async def _replayed(self, key: ContentKey) -> Option[TessellationResult]:
        held = await self._fetched(key)
        held.swap().map(self._noted)
        return held.to_option()

    async def _fetched(self, key: ContentKey) -> RuntimeRail[TessellationResult]:
        pointer = await self._repository.lane.run_async(StoreOp.Get(spill_path(SpillKind.SOURCE, key)))
        decoded = pointer.bind(
            lambda outcome: boundary(DAEMON_SPILL, lambda: _decoded_header(bytes(outcome.source)), catch=_HEADER_RAISES)
        )
        match decoded:
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=(header, artifact, semantic)):
                result = _result(key, header, artifact, semantic)
                return (await self._repository.verified(result.artifact)).bind(
                    lambda present: Ok(result) if present else Error(DAEMON_REPLAY.raised(result.artifact.sha256.hex()))
                )
            case unreachable:
                assert_never(unreachable)

    async def _indexed(self, result: TessellationResult) -> TessellationResult:
        source_path = spill_path(SpillKind.SOURCE, result.content_key)
        header = _SPILL_ENCODER.encode(
            SpillHeader(
                result.artifact.to_binary(),
                result.element_count,
                result.triangle_count,
                result.semantic.to_binary(),
            )
        )
        sealed = await _created(self._repository.lane, source_path, header)
        match sealed:
            case Result(tag="error", error=fault):
                self._noted(fault)
                return replace(result, spill=Spill.REFUSED)
            case Result(tag="ok", ok=state):
                evidence = _stored(
                    source_path,
                    len(header),
                    (
                        Assigned(path="/source_key", next=result.content_key.hex),
                        Assigned(path="/artifact", next=result.artifact.sha256.hex()),
                    ),
                    state,
                )
                (await Journal.record(evidence, scope=self._composition)).swap().map(self._noted)
                return replace(result, spill=state)
            case unreachable:
                assert_never(unreachable)

    def _noted(self, fault: BoundaryFault) -> None:
        self._receipts = self._receipts.append(Block.singleton(Receipt.of(OWNER, fault)))

    def _fold(
        self,
        admitted: Block[tuple[ContentKey, ProducerTag, Admit[TessellationResult]]],
        warm: Map[ContentKey, TessellationResult],
        faults: Block[BoundaryFault],
    ) -> None:
        facts = admitted.choose(lambda row: self._cache.try_find(row[0]).map(lambda held: _phase(held, warm, row[0]).fact(row[1])))
        self._receipts = self._receipts.append(facts).append(faults.map(lambda fault: Receipt.of(OWNER, fault)))

    def contribute(self) -> Iterable[Receipt]:
        drained, self._receipts = self._receipts, Block.empty()
        return drained
```

## [03]-[RESEARCH]

(none)
