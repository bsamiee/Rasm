# [PY_RUNTIME_SHAPES]

Python's wire vocabulary is the generated `rasm.contracts` import root — every descriptor-backed message crosses as its `<family>_pb` class, typed end to end, fresh by the corpus gate, and handed to handlers by Connect directly — so this page mints only what no generator emits: the `SPLAT_FORMS` release matrix, the `FaultRecovery` correspondence both directions of the runtime verdict seat on, the ONE module-level `Registry` over every generated descriptor, and ONE boot census over closed-family closure and the generated Connect applications each served or dialed service row names, proved in BOTH directions against the corpus descriptor. `dotnet:Rasm.Compute/Runtime/wire#PROTO_VOCABULARY` is the peer mint of the same corpus, and one corpus emission owns contract compatibility — never a runtime descriptor diff.

Canonical-bytes custody is recorded here and re-minted nowhere in this folder: the length- and count-framed content-key byte stream the `evidence/identity#IDENTITY` key runs over is the payload-agnostic `content-identity` contract layout, each branch writing it from its own canonical writer with parity proving at the corpus; the positional op-log MessagePack envelope is self-delimiting and its CRDT raw slot now carries generated protobuf, the amendment governing content keys rather than either framing. The census lives one tier below `transport/wire#CRDT_CODEC` because a wire-side registry forces a `shapes -> wire` back-edge from the gate.

## [01]-[INDEX]

- [02]-[VOCABULARY]: the one wire slot type the MessagePack CRDT leg keeps, the `RecoveryCell` correspondence over the generated `FaultRecovery`, and the `SPLAT_FORMS` release matrix.
- [03]-[BOOT_CENSUS]: the `REGISTRY` descriptor seat, the `SERVICE_VOCABULARY` seed table, the `Closure` registration, and the `aligned` boot census over closure and generated services in both directions.

## [02]-[VOCABULARY]

- Owner: generated classes ARE the proto vocabulary — a msgspec `Struct` restating a corpus message is the deleted mirror, so every descriptor-backed family (`compute`, `control`, `fault`, `appearance`, `scene`, `organization`, `fabrication`, `declaration`, `event`, `parity`, the vendored `health` and `cloudevents`) imports from `rasm.contracts.rasm.contracts.<family>.v1.<family>_pb` (vendored: `rasm.contracts.grpc.health.v1`, `rasm.contracts.io.cloudevents.v1`) and this page declares no field of theirs. The appearance family that once crossed as the producer's positional MessagePack record is `rasm.contracts.appearance` — `Material`, `OpenPbr`, `Emission`, `Set`, `Plane`, `Ibl` — so the decode-only mirror structs, their key rosters, and the zip codec that served them are gone whole; the one MessagePack wire this branch still reads is the generic op-log envelope, whose `crdt` raw slot is discriminated at `transport/wire#CRDT_CODEC`.
- Cases: presence on a generated class is the generator's — a message-typed slot reads `None` when unset, an `optional` scalar constructs on `T | None`, reads its proto zero when unset under the native store, and answers presence through `has_field`; `Oneof(field, value)` on the oneof attribute, `None` for an unset oneof. A scalar whose producer MEASURES nothing on one arm — the chart residual an uncalibrated capture never solves — reads absent off `has_field` and no shape here re-spells it. `TessellateRequest` declares no `optional` column — its budget rides `TessellationPolicy.triangle_budget` under the required `policy` message — so every presence it answers is a message-typed slot's.
- Auto: `WireI63` survives for the MessagePack op-log envelope alone — `OperationId` and `OpLogEntry` spell their non-negative signed-64 counters and HLC halves through it, matching the producer's `long.MaxValue` admission, while generated protobuf owns CRDT arm counters. `TessellateRequest`/`TessellateResponse`/`ArtifactFrame`/`GaussianSplatScan` are generated classes geometry `mesh/serve` and `scan/ingestion` import by symbol, minting no wire shape; `GaussianSplatScan.format` grounding is the `SPLAT_FORMS` matrix keyed on the generated `SplatFormat` enum and never a comment: one DECLARED cell per rostered release carries the band ceiling and the alpha activation it grounds, the corpus's `enum.defined_only` rule refusing every unrostered release at decode, and `splat_form` is the ONE read over it, seated here because the boot census walks declarations and a key is a VALUE.
- Receipt: `FaultDetail` is the typed refusal detail the suite converges on through Connect error details; `transport/serve#SERVE` owns egress while `wire_detail`/`remote_fault` here own the one ingress correspondence every generated client can import without depending on the serve composition sink. `domain` and `case` identify the producing fault row, never a transport code. The producer states recovery on the `FaultRecovery` oneof, and `RecoveryCell` owns both directions between that generated cell and the interior `Recovery` vocabulary. `FaultRecovery`'s throttled arm IS the standard `google.rpc.RetryInfo`, so `RecoveryCell.advice` hands the serve edge THAT instance for its generic detail seat and no leg reconstructs a second window a later edit can drive apart.
- Entry: `admitted`, `custody`, and the `dialed` leaf are the definition-time weaves that correspondence exists to serve, so a client edge declares its rows and never re-spells the capture. `dialed` covers the two arms EVERY generated-client call carries — `AdmissionError`, the client-side body admission refusing before a socket, and `ConnectError`, the peer's own typed detail lifted whole through `remote_fault` — while `custody` covers the one arm only a seam holding artifact custody carries, `ArtifactError`'s failed aggregate proof. They are separate because their arm sets are separate: a pure dial can never raise `ArtifactError`, so folding it in binds a row nothing mints, and a custody bracket wrapping a dial stacks `custody` outside `admitted` so the inner refusal is already railed before the bracket unwinds. `admitted` and `custody` are both rail-PRESERVING and stack in either depth; `dialed` is the minting leaf a bare generated-client call weaves, derived as `admitted` over the one lift so the capture keeps a single body. Each factory REQUIRES the slot its own token fills — `("phase",)` for the admission phase, `("proof",)` for the artifact proof — so publishing an admission token under a coordinate named `proof` is unspellable rather than merely wrong.
- Packages: `protobuf-py`, `connectrpc`, `rasm.contracts`, `msgspec`, `expression`, and the faults rail per the fence imports.
- Growth: a new descriptor-backed message is one proto edit with `assay contracts generate` — zero rows here; a new splat release is one `SplatFormat` member at the corpus and one `SPLAT_FORMS` cell carrying its grounding; a new recovery arm is one oneof arm at the corpus and one arm in each `RecoveryCell` direction; a new contracts-SDK refusal class is one weave beside these two, never an `except` arm re-spelled at a consumer.
- Boundary: the matrix and generated-detail correspondences a declaration census can never hold — no codec, span, or transcode body (`transport/wire#WIRE_RAIL`) and no causal lift (`evidence/clock#CLOCK`). `transport/serve#SERVE` alone packs a refusal; this page only unpacks the live generated detail at a client edge and preserves it whole. Neither fold reads policy or a clock.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable
from enum import StrEnum
from functools import wraps
from typing import Annotated, Final, Literal, assert_never

from connectrpc.code import Code
from connectrpc.errors import ConnectError
from expression import Error, Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Meta, Struct
from protobuf import Oneof
from protobuf.wkt import Duration, Empty
from rasm.contracts.google.rpc.error_details_pb import RetryInfo
from rasm.contracts.rasm.contracts.scan.gaussian_pb import SplatFormat
from rasm.contracts.rasm.contracts.fault.fault_pb import FaultDetail, FaultRecovery

from rasm.runtime.faults import BoundaryFault, SERVE_REMOTE, SHAPES_FORMAT, SHAPES_WINDOW, FaultRow, Leg, Recovery, RuntimeRail
from rasm.runtime.transport.artifact import ArtifactError
from rasm.runtime.transport.body import AdmissionError

# --- [TYPES] ----------------------------------------------------------------------------

type WireI63 = Annotated[int, Meta(ge=0, le=9_223_372_036_854_775_807)]

type Dialed[**P, T] = Callable[P, Awaitable[RuntimeRail[T]]]
type ClientWeave[**P, T] = Callable[[Dialed[P, T]], Dialed[P, T]]
type CustodyWeave[**P, T] = ClientWeave[P, T]

_PHASE_SLOT: Final[tuple[str, ...]] = ("phase",)
_PROOF_SLOT: Final[tuple[str, ...]] = ("proof",)


@tagged_union(frozen=True)
class RemoteConflict(BaseException):
    tag: Literal["stated", "coded"] = tag()
    stated: tuple[FaultDetail, Code] = case()
    coded: tuple[Code, str] = case()


class Activation(StrEnum):
    SIGMOID = "sigmoid"


# --- [MODELS] ---------------------------------------------------------------------------


class RecoveryCell:
    @staticmethod
    def of(recovery: Recovery) -> FaultRecovery:
        match recovery:
            case Recovery(tag="terminal"):
                return FaultRecovery(kind=Oneof("terminal", Empty()))
            case Recovery(tag="transient"):
                return FaultRecovery(kind=Oneof("transient", Empty()))
            case Recovery(tag="throttled", throttled=seconds):
                return FaultRecovery(kind=Oneof("retry_after", RetryInfo(retry_delay=Duration.from_seconds(seconds))))
            case _ as unreachable:
                assert_never(unreachable)

    @staticmethod
    def advice(cell: FaultRecovery | None) -> Option[RetryInfo]:
        match cell.kind if cell is not None else None:
            case Oneof(field="retry_after", value=RetryInfo() as stated):
                return Some(stated)
            case _:
                return Nothing

    @staticmethod
    def stated(cell: FaultRecovery | None) -> RuntimeRail[Option[Recovery]]:
        match cell.kind if cell is not None else None:
            case None:
                return Ok(Nothing)
            case Oneof(field="terminal"):
                return Ok(Some(Recovery(terminal=None)))
            case Oneof(field="transient"):
                return Ok(Some(Recovery(transient=None)))
            case Oneof(field="retry_after", value=RetryInfo(retry_delay=Duration() as window)):
                seconds = window.to_seconds()
                return Ok(Some(Recovery(throttled=seconds))) if seconds >= 0.0 else Error(SHAPES_WINDOW.raised(str(seconds)))
            case Oneof(field="retry_after"):
                return Error(SHAPES_WINDOW.raised("<unstated>"))
            case _ as unreachable:
                assert_never(unreachable)


def wire_detail(raised: Exception, /) -> Option[FaultDetail]:
    match raised:
        case ConnectError() as wired:
            return Block.of_seq(wired.details).choose(lambda detail: Option.of_optional(detail.value(FaultDetail))).try_head()
        case _:
            return Nothing


def remote_fault(terminal: ConnectError, /) -> BoundaryFault:
    status = terminal.code
    return BoundaryFault.of(
        SERVE_REMOTE,
        wire_detail(terminal)
        .map(lambda sealed: RemoteConflict(stated=(sealed, status)))
        .default_with(lambda: RemoteConflict(coded=(status, terminal.message or type(terminal).__qualname__))),
    )


def admitted[**P, T, L: Leg](admission: FaultRow[L], /) -> ClientWeave[P, T]:
    if admission.slots != _PHASE_SLOT:
        raise ValueError(f"{admission.subject}: admitted publishes a phase token and needs slots={_PHASE_SLOT}")

    def weave(call: Dialed[P, T], /) -> Dialed[P, T]:
        @wraps(call)
        async def held(*args: P.args, **kwargs: P.kwargs) -> RuntimeRail[T]:
            try:
                return await call(*args, **kwargs)
            except AdmissionError as refused:
                return Error(admission.raised(refused.phase.value))
            except ConnectError as refused:
                return Error(remote_fault(refused))

        return held

    return weave


def _railed[**P, T](call: Callable[P, Awaitable[T]], /) -> Dialed[P, T]:
    @wraps(call)
    async def minted(*args: P.args, **kwargs: P.kwargs) -> RuntimeRail[T]:
        return Ok(await call(*args, **kwargs))

    return minted


def dialed[**P, T, L: Leg](admission: FaultRow[L], /) -> Callable[[Callable[P, Awaitable[T]]], Dialed[P, T]]:
    weave = admitted(admission)
    return lambda call: weave(_railed(call))


def custody[**P, T, L: Leg](integrity: FaultRow[L], /) -> CustodyWeave[P, T]:
    if integrity.slots != _PROOF_SLOT:
        raise ValueError(f"{integrity.subject}: custody publishes a proof token and needs slots={_PROOF_SLOT}")

    def weave(call: Dialed[P, T], /) -> Dialed[P, T]:
        @wraps(call)
        async def held(*args: P.args, **kwargs: P.kwargs) -> RuntimeRail[T]:
            try:
                return await call(*args, **kwargs)
            except ArtifactError as refused:
                return Error(integrity.raised(refused.proof.value))

        return held

    return weave


class SplatGrounding(Struct, frozen=True, gc=False):
    degree: int
    activation: Activation


# --- [TABLES] ---------------------------------------------------------------------------

SPLAT_FORMS: Final[Map[SplatFormat, SplatGrounding]] = Map.of_seq([
    (SplatFormat.SPZ_V4, SplatGrounding(degree=3, activation=Activation.SIGMOID)),
    (SplatFormat.SOG_V2, SplatGrounding(degree=3, activation=Activation.SIGMOID)),
])

# --- [OPERATIONS] -----------------------------------------------------------------------


def splat_form(key: SplatFormat) -> RuntimeRail[SplatGrounding]:
    return SPLAT_FORMS.try_find(key).to_result_with(lambda: SHAPES_FORMAT.raised(key.name))
```

## [03]-[BOOT_CENSUS]

- Owner: `REGISTRY` is the ONE `Registry` over every generated descriptor this branch binds — the estate and vendored `_pb` files plus the `buf.validate`, `google.rpc`, and `google.type` dependencies they import — so `Any` packing, ProtoJSON `@type` resolution, and `ErrorDetail.value(REGISTRY)` all resolve off one seat and no page mints a second registry for one type name. `SERVICE_VOCABULARY` binds each dialed or served service to its rpc roster and the generated application class that proves it — the compute and artifact services the geometry servicer implements and the vendored `grpc.health.v1` service `transport/serve#SERVE` hosts — and `aligned` proves every row beside every `Closure` row a caller registers in one pass. The descriptor-backed message families need no row here: their structure is the corpus gate's, and a `PROTO_VOCABULARY` row beside the generated class would be a second authority for one fact. `transport/serve#SERVE` mounts its generated applications under the `WireService` members; the daemon composition root runs `aligned` once before serve binds and is the one seat that hands it the `Closure` roster.
- Auto: a service row carries the full name and rpc roster because Connect resolves both as one path. Generated discovery proves through `CapabilityDiscoveryServiceASGIApplication`; only per-capability invoke remains brokered, with descriptor ids admitted from that generated catalog.
- Law: every selected served rpc EXISTS on its generated service — the selector may deliberately retain an upstream method as support closure without assigning it a runtime actor, as `grpc.health.v1.Health.Watch` does. Every selected method has a `WireMethod` row and an `@override`, and a row absent from the generated service refuses at boot rather than at a peer's first dial. The generated protocol's snake_case member naming is the generator's own derivation and is never re-derived here; a stale override name breaks at type-check under `@override`.
- Packages: `protobuf-py`, `rasm.contracts`, and `expression` per the fence imports.
- Growth: a new generated family is one `desc()` seat on `REGISTRY`; a new served or dialed method is one `WireMethod` member on its service's roster and a new service one `WireService` member with its `SERVICE_VOCABULARY` row naming the generated application; a new closed family the tables key on is one `Closure` row at the composition root; a new structural assertion is one arm in `_service` or `_closed`, never a second gate; a new sibling consumer binds existing rows by symbol.
- Boundary: the census proves structure, not values — byte-level round-trip parity is the `evidence/reproduction#SEED_REPRODUCTION` corpus's, and contract compatibility is the corpus gate's, never a runtime descriptor walk. Oneof exclusivity holds on the generated classes by construction and `RecoveryCell.stated` owes only the window refusal. Deliberately partial tables register no `Closure` row and state that partiality at their own owner: `reliability/resilience#RESILIENCE`'s `CIRCUIT` and `RATES` declare absence AS the policy.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from typing import Final, assert_never

from expression import Error, Ok
from expression.collections import Block, Map
from msgspec import Struct
from protobuf import Registry
from rasm.contracts.buf.validate import validate_pb
from rasm.contracts.google.rpc import error_details_pb
from rasm.contracts.google.type import date_pb
from rasm.contracts.grpc.health.v1 import health_pb
from rasm.contracts.grpc.health.v1.health_connect import HealthASGIApplication
from rasm.contracts.io.cloudevents.v1 import cloudevents_pb
from rasm.contracts.rasm.contracts.appearance import appearance_pb
from rasm.contracts.rasm.contracts.artifact import artifact_pb
from rasm.contracts.rasm.contracts.capability import descriptor_pb
from rasm.contracts.rasm.contracts.capability import discovery_pb
from rasm.contracts.rasm.contracts.capability.discovery_connect import CapabilityDiscoveryServiceASGIApplication
from rasm.contracts.rasm.contracts.compute import compute_pb
from rasm.contracts.rasm.contracts.artifact.artifact_connect import ArtifactServiceASGIApplication
from rasm.contracts.rasm.contracts.compute.compute_connect import ComputeServiceASGIApplication
from rasm.contracts.rasm.contracts.declaration import declaration_pb
from rasm.contracts.rasm.contracts.event import event_pb
from rasm.contracts.rasm.contracts.fabrication import fabrication_pb
from rasm.contracts.rasm.contracts.fault import fault_pb
from rasm.contracts.rasm.contracts.organization import organization_pb
from rasm.contracts.rasm.contracts.parity import parity_pb
from rasm.contracts.rasm.contracts.scene import scene_pb

from rasm.runtime.faults import SHAPES_DOUBLED, SHAPES_DRIFT, SHAPES_SERVICES, RuntimeRail, boundary

# --- [TYPES] ----------------------------------------------------------------------------

type Generated = (
    type[ArtifactServiceASGIApplication]
    | type[CapabilityDiscoveryServiceASGIApplication]
    | type[ComputeServiceASGIApplication]
    | type[HealthASGIApplication]
)


class WireService(StrEnum):
    COMPUTE = "rasm.contracts.compute.ComputeService"
    ARTIFACT = "rasm.contracts.artifact.ArtifactService"
    HEALTH = "grpc.health.v1.Health"
    CAPABILITY_DISCOVERY = "rasm.contracts.capability.CapabilityDiscoveryService"
    CAPABILITY = "rasm.capability"


class WireMethod(StrEnum):
    TESSELLATE = "Tessellate"
    FETCH = "Fetch"
    PUT = "Put"
    CHECK = "Check"
    DISCOVER = "Discover"


class ServiceProof(StrEnum):
    GENERATED = "generated"
    BROKERED = "brokered"

# --- [MODELS] ---------------------------------------------------------------------------


class Closure(Struct, frozen=True, gc=False):
    table: str
    members: frozenset[str]
    rostered: frozenset[str]

    @staticmethod
    def of(table: str, family: "type[StrEnum] | tuple[str, ...]", rostered: Map[str, object]) -> "Closure":
        return Closure(table=table, members=frozenset(family), rostered=frozenset(rostered.keys()))

# --- [TABLES] ---------------------------------------------------------------------------

REGISTRY: Final[Registry] = Registry(
    *(
        module.desc()
        for module in (
            validate_pb, date_pb, error_details_pb, cloudevents_pb, health_pb,
            fault_pb, artifact_pb, compute_pb, descriptor_pb, discovery_pb, appearance_pb, scene_pb, organization_pb, fabrication_pb, declaration_pb, event_pb, parity_pb,
        )
    )
)

SERVICE_VOCABULARY: Final[tuple[tuple[WireService, tuple[WireMethod, ...], ServiceProof, Generated | None], ...]] = (
    (WireService.COMPUTE, (WireMethod.TESSELLATE,), ServiceProof.GENERATED, ComputeServiceASGIApplication),
    (WireService.ARTIFACT, (WireMethod.FETCH, WireMethod.PUT), ServiceProof.GENERATED, ArtifactServiceASGIApplication),
    (WireService.HEALTH, (WireMethod.CHECK,), ServiceProof.GENERATED, HealthASGIApplication),
    (WireService.CAPABILITY_DISCOVERY, (WireMethod.DISCOVER,), ServiceProof.GENERATED, CapabilityDiscoveryServiceASGIApplication),
    (WireService.CAPABILITY, (), ServiceProof.BROKERED, None),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _closed(row: Closure) -> Block[str]:
    unrostered = Block.of_seq(sorted(row.members - row.rostered)).map(lambda gap: f"{row.table}.{gap}:closure-member-unrostered")
    stale = Block.of_seq(sorted(row.rostered - row.members)).map(lambda gap: f"{row.table}.{gap}:row-outside-closure")
    return unrostered.append(stale)


def _service(name: WireService, methods: tuple[WireMethod, ...], proof: ServiceProof, application: Generated | None) -> Block[str]:
    match proof, application, REGISTRY.service(str(name)):
        case (ServiceProof.BROKERED, _, _):
            return Block.empty()
        case (ServiceProof.GENERATED, None, _):
            return Block.singleton(f"{name}:generated-application-unnamed")
        case (ServiceProof.GENERATED, _, None):
            return Block.singleton(f"{name}:service-unregistered")
        case (ServiceProof.GENERATED, app, desc):
            generated, rostered = frozenset(method.name for method in desc.methods), frozenset(str(method) for method in methods)
            ungenerated = Block.of_seq(sorted(rostered - generated)).map(lambda gap: f"{name}.{gap}:rpc-ungenerated")
            return (
                boundary(SHAPES_SERVICES, lambda: app(object()).path, catch=(TypeError, AttributeError))
                .map(lambda path: (Block.empty() if path == f"/{name}" else Block.singleton(f"{name}:service-path-{path}")).append(ungenerated))
                .default_value(Block.singleton(f"{name}:generated-application-unresolvable"))
            )
        case _ as unreachable:
            assert_never(unreachable)


def aligned(closed: Block[Closure]) -> RuntimeRail[int]:
    rows = len(SERVICE_VOCABULARY) + len(closed)
    doubled = any(
        unique != len(table)
        for unique, table in (
            (len(frozenset(name for name, *_ in SERVICE_VOCABULARY)), SERVICE_VOCABULARY),
            (len(frozenset(row.table for row in closed)), closed),
        )
    )
    drift = Block.of_seq(SERVICE_VOCABULARY).collect(lambda row: _service(*row)).append(closed.collect(_closed))
    return (
        Error(SHAPES_DOUBLED.raised())
        if doubled
        else Ok(rows)
        if drift.is_empty()
        else Error(SHAPES_DRIFT.raised(";".join(drift)))
    )
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
