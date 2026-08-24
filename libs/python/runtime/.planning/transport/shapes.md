# [PY_RUNTIME_SHAPES]

Python's wire vocabulary is the generated `rasm.contracts.gen` source root — every descriptor-backed message crosses as its `<family>_pb` class, typed end to end, fresh by the corpus gate, and handed to handlers by Connect directly — so this page mints only what no generator emits: the `SPLAT_FORMS` release matrix, the `FaultRecovery` correspondence both directions of the runtime verdict seat on, the ONE module-level `Registry` over every generated descriptor, and ONE boot census over closed-family closure and the generated Connect applications each served or dialed service row names, proved in BOTH directions against the corpus descriptor. `dotnet:Rasm.Compute/Runtime/wire#PROTO_VOCABULARY` is the peer mint of the same corpus, and one corpus emission owns contract compatibility — never a runtime descriptor diff.

Canonical-bytes custody is recorded here and re-minted nowhere in this folder: the length- and count-framed content-key byte stream the `evidence/identity#IDENTITY` key runs over is the payload-agnostic `content-identity` contract layout, each branch writing it from its own canonical writer with parity proving at the corpus; the positional op-log MessagePack envelope is self-delimiting and its CRDT raw slot now carries generated protobuf, the amendment governing content keys rather than either framing. The census lives one tier below `transport/wire#CRDT_CODEC` because a wire-side registry forces a `shapes -> wire` back-edge from the gate.

## [01]-[INDEX]

- [02]-[VOCABULARY]: the one wire slot type the MessagePack CRDT leg keeps, the `RecoveryCell` correspondence over the generated `FaultRecovery`, and the `SPLAT_FORMS` release matrix.
- [03]-[BOOT_CENSUS]: the `REGISTRY` descriptor seat, the `SERVICE_VOCABULARY` seed table, the `Closure` registration, and the `aligned` boot census over closure and generated services in both directions.

## [02]-[VOCABULARY]

- Owner: generated classes ARE the proto vocabulary — a msgspec `Struct` restating a corpus message is the deleted mirror, so every descriptor-backed family (`compute`, `control`, `fault`, `appearance`, `scene`, `organization`, `fabrication`, `declaration`, `event`, `parity`, the vendored `health` and `cloudevents`) imports from `rasm.contracts.gen.rasm.contracts.<family>.v1.<family>_pb` (vendored: `rasm.contracts.vendor.grpc.health.v1`, `rasm.contracts.vendor.io.cloudevents.v1`) and this page declares no field of theirs. The appearance family that once crossed as the producer's positional MessagePack record is `rasm.contracts.appearance` — `Material`, `OpenPbr`, `Emission`, `Set`, `Plane`, `Ibl` — so the decode-only mirror structs, their key rosters, and the zip codec that served them are gone whole; the one MessagePack wire this branch still reads is the generic op-log envelope, whose `crdt` raw slot is discriminated at `transport/wire#CRDT_CODEC`.
- Cases: presence on a generated class is the generator's — a message-typed slot reads `None` when unset, an `optional` scalar constructs on `T | None`, reads its proto zero when unset under the native store, and answers presence through `has_field`; `Oneof(field, value)` on the oneof attribute, `None` for an unset oneof. A scalar whose producer MEASURES nothing on one arm — the chart residual an uncalibrated capture never solves — reads absent off `has_field` and no shape here re-spells it. `TessellateRequest` declares no `optional` column — its budget rides `TessellationPolicy.triangle_budget` under the required `policy` message — so every presence it answers is a message-typed slot's.
- Auto: `WireI63` survives for the MessagePack op-log envelope alone — `OperationId` and `OpLogEntry` spell their non-negative signed-64 counters and HLC halves through it, matching the producer's `long.MaxValue` admission, while generated protobuf owns CRDT arm counters. `TessellateRequest`/`TessellateResponse`/`ArtifactFrame`/`GaussianSplatScan` are generated classes geometry `mesh/serve` and `scan/ingestion` import by symbol, minting no wire shape; `GaussianSplatScan.format` grounding is the `SPLAT_FORMS` matrix keyed on the generated `SplatFormat` enum and never a comment: one DECLARED cell per rostered release carries the band ceiling and the alpha activation it grounds, the corpus's `enum.defined_only` rule refusing every unrostered release at decode, and `splat_form` is the ONE read over it, seated here because the boot census walks declarations and a key is a VALUE.
- Receipt: `FaultDetail` is the typed refusal detail the suite converges on through Connect error details; `transport/serve#SERVE` owns egress while `wire_detail`/`remote_fault` here own the one ingress correspondence every generated client can import without depending on the serve composition sink. `domain` and `case` identify the producing fault row, never a transport code. The producer states recovery on the `FaultRecovery` oneof, and `RecoveryCell` owns both directions between that generated cell and the interior `Recovery` vocabulary. `FaultRecovery`'s throttled arm IS the standard `google.rpc.RetryInfo`, so `RecoveryCell.advice` hands the serve edge THAT instance for its generic detail seat and no leg reconstructs a second window a later edit can drive apart.
- Entry: `admitted`, `custody`, and the `dialed` leaf are the definition-time weaves that correspondence exists to serve, so a client edge declares its rows and never re-spells the capture. `dialed` covers the two arms EVERY generated-client call carries — `AdmissionError`, the client-side body admission refusing before a socket, and `ConnectError`, the peer's own typed detail lifted whole through `remote_fault` — while `custody` covers the one arm only a seam holding artifact custody carries, `ArtifactError`'s failed aggregate proof. They are separate because their arm sets are separate: a pure dial can never raise `ArtifactError`, so folding it in binds a row nothing mints, and a custody bracket wrapping a dial stacks `custody` outside `admitted` so the inner refusal is already railed before the bracket unwinds. `admitted` and `custody` are both rail-PRESERVING and stack in either depth; `dialed` is the minting leaf a bare generated-client call weaves, derived as `admitted` over the one lift so the capture keeps a single body. Each factory REQUIRES the slot its own token fills — `("phase",)` for the admission phase, `("proof",)` for the artifact proof — so publishing an admission token under a coordinate named `proof` is unspellable rather than merely wrong.
- Packages: `protobuf-py`, `connectrpc`, `rasm.contracts`, `msgspec`, `expression`, and the faults rail per the fence imports.
- Growth: a new descriptor-backed message is one proto edit with `assay contracts generate` — zero rows here; a new splat release is one `SplatFormat` member at the corpus and one `SPLAT_FORMS` cell carrying its grounding; a new recovery arm is one oneof arm at the corpus and one arm in each `RecoveryCell` direction; a new contracts-SDK refusal class is one weave beside these two, never an `except` arm re-spelled at a consumer.
- Boundary: the matrix and generated-detail correspondences a declaration census can never hold — no codec, span, or transcode body (`transport/wire#WIRE_RAIL`) and no causal lift (`evidence/clock#CLOCK`). `transport/serve#SERVE` alone packs a refusal; this page only unpacks the live generated detail at a client edge and preserves it whole. Neither fold reads policy or a clock.

```python signature
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
from rasm.contracts import AdmissionError
from rasm.contracts.artifact import ArtifactError
from rasm.contracts.gen.google.rpc.error_details_pb import RetryInfo
from rasm.contracts.gen.rasm.contracts.scan.gaussian_pb import SplatFormat
from rasm.contracts.gen.rasm.contracts.fault.fault_pb import FaultDetail, FaultRecovery

from rasm.runtime.faults import BoundaryFault, SERVE_REMOTE, SHAPES_FORMAT, SHAPES_WINDOW, FaultRow, Leg, Recovery, RuntimeRail

# --- [TYPES] ----------------------------------------------------------------------------

# The explicit envelope admits the producer's actual domain, not MessagePack's wider integer domain: sequence,
# vector, physical, and logical cells all refuse above signed-64 max before an interior comparison can narrow them.
type WireI63 = Annotated[int, Meta(ge=0, le=9_223_372_036_854_775_807)]

# `Dialed` names the one client-edge shape both weaves preserve: `dialed` MINTS it off a bare generated-client call and
# `custody` PRESERVES it, so an arbitrary-depth stack keeps `**P`, the return rail, and `functools.wraps` identity.
type Dialed[**P, T] = Callable[P, Awaitable[RuntimeRail[T]]]
type ClientWeave[**P, T] = Callable[[Dialed[P, T]], Dialed[P, T]]
type CustodyWeave[**P, T] = ClientWeave[P, T]

# Each weave names the slot coordinate its own token fills, checked at weave time so a token never publishes under a
# coordinate naming a different closed vocabulary.
_PHASE_SLOT: Final[tuple[str, ...]] = ("phase",)
_PROOF_SLOT: Final[tuple[str, ...]] = ("proof",)


@tagged_union(frozen=True)
class RemoteConflict(BaseException):
    tag: Literal["stated", "coded"] = tag()
    stated: tuple[FaultDetail, Code] = case()
    coded: tuple[Code, str] = case()


# ONE activation vocabulary carries the `alphas` block, read off the release row that admitted the payload rather than
# assumed by every consumer: a container publishing the pre-activation logit lands one member here and its readers
# dispatch on it, where a `bool` or a bare comment leaves each reader guessing which transform already ran.
class Activation(StrEnum):
    SIGMOID = "sigmoid"  # the container stores `sigmoid(logit) * 255` and the wire carries the activated [0, 1] value


# --- [MODELS] ---------------------------------------------------------------------------


class RecoveryCell:
    # BOTH directions of the `Recovery` correspondence on the GENERATED `FaultRecovery`, so a forward and an inverse that
    # drift into two truths have nowhere to live. `reliability/faults#FAULT` owns the interior verdict vocabulary whole and
    # this owner the cell alone: wire absence is NOT a fourth interior case but the `Option` the INGRESS fold answers, so a
    # frame minted before the field existed states that it carried no verdict without every interior consumer matching a
    # state no interior value can hold. Defaulting an unread verdict to `terminal` is what the pair forecloses: it strands
    # every legacy peer's re-drivable fault as permanent, the drift this slot exists for.
    @staticmethod
    def of(recovery: Recovery) -> FaultRecovery:
        # ONE total egress fold, TOTAL on a bare verdict rather than on an `Option`: unstated is an INGRESS fact a
        # legacy peer's frame carries, never a posture this branch may adopt, so every frame this host writes states its
        # verdict. The oneof constructs on its NAME — `Oneof(field, value)` — and `Duration.from_seconds` keeps the
        # window's nanos. This is the branch's ONE `RetryInfo` construction site: the throttled arm IS that standard
        # message, so the generic detail seat reads the instance back through `advice` and never mints a second.
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
        # Generic Connect and gRPC peers read the throttled arm ITSELF as their standard detail seat, handed out
        # whole so that estate cell and top-level detail are one message rather than two projections. Reading an arm
        # lives HERE beside both folds because arm knowledge scattered to the serve edge is exactly the second truth
        # this owner forecloses. `FaultDetail.recovery` is a message slot the generator spells nullable, so the seat
        # widens to it exactly as `stated` does. Every other arm states no window and seats nothing.
        match cell.kind if cell is not None else None:
            case Oneof(field="retry_after", value=RetryInfo() as stated):
                return Some(stated)
            case _:
                return Nothing

    @staticmethod
    def stated(cell: FaultRecovery | None) -> RuntimeRail[Option[Recovery]]:
        # ONE total ingress fold: an absent cell and an unset oneof both answer `Nothing`, one set arm answers its case, and
        # an unusable window refuses. At-most-one-arm-set holds on the generated class by construction, so the one value
        # law this fold owes is the window, and the arm being a `RetryInfo` gives that window TWO unusable shapes under one
        # refusal row. `retry_delay` is a message slot the generator spells `Duration | None`, and the corpus CEL rule
        # forcing it present evaluates at the body interceptor, never here — so an arm claiming a window while stating
        # none refuses under its own label. A signed `Duration` admits `-1s`, which names a wait that already elapsed and no
        # producer states, so it refuses rather than folding to `terminal` — a producer's broken cell can never read
        # downstream as a producer's terminal verdict. A zero window stays `throttled` and never coalesces onto `transient`.
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
    # `admitted` is the ONE client-edge capture and the RAIL-PRESERVING half of the pair: it takes an already-railed core and
    # returns one, so it stacks over a seam whose body already composes rails. `slots` is checked HERE, at weave
    # time, because the `phase` token this arm publishes is meaningless under any other coordinate — the deleted
    # hand-rolled form published an admission phase into a row whose one slot was named `proof`, and
    # `zip(strict=True)` waved it through because the arity matched.
    if admission.slots != _PHASE_SLOT:
        raise ValueError(f"{admission.subject}: admitted publishes a phase token and needs slots={_PHASE_SLOT}")

    def weave(call: Dialed[P, T], /) -> Dialed[P, T]:
        @wraps(call)
        async def held(*args: P.args, **kwargs: P.kwargs) -> RuntimeRail[T]:
            try:
                return await call(*args, **kwargs)
            except AdmissionError as refused:
                # CLIENT-side body admission refuses before a socket exists, so the call spends no attempt and the
                # phase names which half of the exchange Protovalidate refused.
                return Error(admission.raised(refused.phase.value))
            except ConnectError as refused:
                # `remote_fault` lifts the peer's OWN typed detail whole: domain, case, correlation, stamp, tenant, and the
                # producer's recovery verdict survive rather than being re-spelled onto a local row.
                return Error(remote_fault(refused))

        return held

    return weave


def _railed[**P, T](call: Callable[P, Awaitable[T]], /) -> Dialed[P, T]:
    # `_railed` lifts a bare generated-client call onto the rail every weave above it preserves.
    @wraps(call)
    async def minted(*args: P.args, **kwargs: P.kwargs) -> RuntimeRail[T]:
        return Ok(await call(*args, **kwargs))

    return minted


def dialed[**P, T, L: Leg](admission: FaultRow[L], /) -> Callable[[Callable[P, Awaitable[T]]], Dialed[P, T]]:
    # `dialed` is the leaf a bare generated-client call weaves — `admitted` over the lift and nothing else — so the capture
    # has ONE body and the minting and preserving forms can never drift into two truths.
    weave = admitted(admission)
    return lambda call: weave(_railed(call))


def custody[**P, T, L: Leg](integrity: FaultRow[L], /) -> CustodyWeave[P, T]:
    # `custody` is the artifact-custody twin and the second RAIL-PRESERVING weave: it takes an already-railed core and
    # returns one, so it stacks OUTSIDE `dialed` wherever a seam brackets a dial in a helper-owned path and the
    # inner dial's refusal is railed before this bracket unwinds — a custody fault and a dial fault can never
    # shadow each other. A seam holding no artifact custody never weaves this, so its row is never minted idle.
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
    # what a PUBLISHED release grounds on the byte columns of the generated `GaussianSplatScan`: the harmonic degree ceiling
    # its container declares — from which the wire width derives as `(degree + 1)^2 * 3` with the DC triple at its head —
    # and the activation the `alphas` block already carries. Both columns are READ off the row that admitted the key, never
    # transcribed at a reader, so the two ends of a release can never hold different band arithmetic for one payload.
    degree: int
    activation: Activation


# --- [TABLES] ---------------------------------------------------------------------------

# (form, release) grounding as DECLARED cells keyed on the generated `SplatFormat` enum rather than as a sentence: the
# corpus closes the roster with `enum.defined_only`, so a release this branch has never rowed refuses at DECODE as an
# undefined member and never reaches this matrix, and the refusal text the retired string roster quoted for releases
# the wire can no longer carry has nothing left to describe. Every rostered member holds a cell here; growth is one enum
# member at the corpus and one row here, and a member the corpus admits that this branch cannot yet ground refuses
# through `splat_form` by NAME, the one spelling both ends already read.
SPLAT_FORMS: Final[Map[SplatFormat, SplatGrounding]] = Map.of_seq([
    (SplatFormat.SPZ_V4, SplatGrounding(degree=3, activation=Activation.SIGMOID)),
    (SplatFormat.SOG_V2, SplatGrounding(degree=3, activation=Activation.SIGMOID)),
])

# --- [OPERATIONS] -----------------------------------------------------------------------


def splat_form(key: SplatFormat) -> RuntimeRail[SplatGrounding]:
    # ONE admission read over the matrix, seated here for the reason `RecoveryCell.stated` is: legality of a `format` is
    # a property of a VALUE and the boot census walks DECLARATIONS, so no gate above can hold it and a consumer
    # re-reading the member seats a second roster. A rostered member with no cell — the unspecified member the corpus
    # refuses at decode, or a release admitted at the corpus ahead of this branch's grounding — refuses by name.
    return SPLAT_FORMS.try_find(key).to_result_with(lambda: SHAPES_FORMAT.raised(key.name))
```

## [03]-[BOOT_CENSUS]

- Owner: `REGISTRY` is the ONE `Registry` over every generated descriptor this branch binds — the estate and vendored `_pb` files plus the `buf.validate`, `google.rpc`, and `google.type` dependencies they import — so `Any` packing, ProtoJSON `@type` resolution, and `ErrorDetail.value(REGISTRY)` all resolve off one seat and no page mints a second registry for one type name. `SERVICE_VOCABULARY` binds each dialed or served service to its rpc roster and the generated application class that proves it — the compute and artifact services the geometry servicer implements and the vendored `grpc.health.v1` service `transport/serve#SERVE` hosts — and `aligned` proves every row beside every `Closure` row a caller registers in one pass. The descriptor-backed message families need no row here: their structure is the corpus gate's, and a `PROTO_VOCABULARY` row beside the generated class would be a second authority for one fact. `transport/serve#SERVE` mounts its generated applications under the `WireService` members; the daemon composition root runs `aligned` once before serve binds and is the one seat that hands it the `Closure` roster.
- Auto: a service row carries the full name and rpc roster because Connect resolves both as one path. Generated discovery proves through `CapabilityDiscoveryServiceASGIApplication`; only per-capability invoke remains brokered, with descriptor ids admitted from that generated catalog.
- Law: every selected served rpc EXISTS on its generated service — the selector may deliberately retain an upstream method as support closure without assigning it a runtime actor, as `grpc.health.v1.Health.Watch` does. Every selected method has a `WireMethod` row and an `@override`, and a row absent from the generated service refuses at boot rather than at a peer's first dial. The generated protocol's snake_case member naming is the generator's own derivation and is never re-derived here; a stale override name breaks at type-check under `@override`.
- Packages: `protobuf-py`, `rasm.contracts`, and `expression` per the fence imports.
- Growth: a new generated family is one `desc()` seat on `REGISTRY`; a new served or dialed method is one `WireMethod` member on its service's roster and a new service one `WireService` member with its `SERVICE_VOCABULARY` row naming the generated application; a new closed family the tables key on is one `Closure` row at the composition root; a new structural assertion is one arm in `_service` or `_closed`, never a second gate; a new sibling consumer binds existing rows by symbol.
- Boundary: the census proves structure, not values — byte-level round-trip parity is the `evidence/reproduction#SEED_REPRODUCTION` corpus's, and contract compatibility is the corpus gate's, never a runtime descriptor walk. Oneof exclusivity holds on the generated classes by construction and `RecoveryCell.stated` owes only the window refusal. Deliberately partial tables register no `Closure` row and state that partiality at their own owner: `reliability/resilience#RESILIENCE`'s `CIRCUIT` and `RATES` declare absence AS the policy.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from typing import Final, assert_never

from expression import Error, Ok
from expression.collections import Block, Map
from msgspec import Struct
from protobuf import Registry
from rasm.contracts.gen.buf.validate import validate_pb
from rasm.contracts.gen.google.rpc import error_details_pb
from rasm.contracts.gen.google.type import date_pb
from rasm.contracts.vendor.grpc.health.v1 import health_pb
from rasm.contracts.vendor.grpc.health.v1.health_connect import HealthASGIApplication
from rasm.contracts.vendor.io.cloudevents.v1 import cloudevents_pb
from rasm.contracts.gen.rasm.contracts.appearance import appearance_pb
from rasm.contracts.gen.rasm.contracts.artifact import artifact_pb
from rasm.contracts.gen.rasm.contracts.capability import descriptor_pb
from rasm.contracts.gen.rasm.contracts.capability import discovery_pb
from rasm.contracts.gen.rasm.contracts.capability.discovery_connect import CapabilityDiscoveryServiceASGIApplication
from rasm.contracts.gen.rasm.contracts.compute import compute_pb
from rasm.contracts.gen.rasm.contracts.artifact.artifact_connect import ArtifactServiceASGIApplication
from rasm.contracts.gen.rasm.contracts.compute.compute_connect import ComputeServiceASGIApplication
from rasm.contracts.gen.rasm.contracts.declaration import declaration_pb
from rasm.contracts.gen.rasm.contracts.event import event_pb
from rasm.contracts.gen.rasm.contracts.fabrication import fabrication_pb
from rasm.contracts.gen.rasm.contracts.fault import fault_pb
from rasm.contracts.gen.rasm.contracts.organization import organization_pb
from rasm.contracts.gen.rasm.contracts.parity import parity_pb
from rasm.contracts.gen.rasm.contracts.scene import scene_pb

from rasm.runtime.faults import SHAPES_DOUBLED, SHAPES_DRIFT, SHAPES_SERVICES, RuntimeRail, boundary

# --- [TYPES] ----------------------------------------------------------------------------

# generated application classes a GENERATED service row proves against; each `path` IS the dial spelling.
type Generated = (
    type[ArtifactServiceASGIApplication]
    | type[CapabilityDiscoveryServiceASGIApplication]
    | type[ComputeServiceASGIApplication]
    | type[HealthASGIApplication]
)


# Fully-qualified service names this branch dials or serves. `rasm.contracts.compute` is the corpus family package
# managed mode derives every peer spelling from, and `grpc.health.v1` is the upstream publisher package the vendored
# `rasm.contracts.vendor.grpc.health.v1` module emits unchanged. Each member's text is byte-identical to its
# application's `path` less the leading slash, which is the key `transport/serve#SERVE` seats its serving map on.
class WireService(StrEnum):
    COMPUTE = "rasm.contracts.compute.ComputeService"
    ARTIFACT = "rasm.contracts.artifact.ArtifactService"
    HEALTH = "grpc.health.v1.Health"
    CAPABILITY_DISCOVERY = "rasm.contracts.capability.CapabilityDiscoveryService"
    CAPABILITY = "rasm.capability"


# rpc names verbatim off the contract's own service table; the dial path concatenates a service with one of these
# and nothing normalizes case or separators, so the member text IS the wire spelling the descriptor census reads.
class WireMethod(StrEnum):
    TESSELLATE = "Tessellate"
    FETCH = "Fetch"
    PUT = "Put"
    CHECK = "Check"
    DISCOVER = "Discover"


class ServiceProof(StrEnum):
    GENERATED = "generated"  # a generated `<Svc>ASGIApplication` and a `REGISTRY` service descriptor stand behind the row
    BROKERED = "brokered"  # a broker-minted path whose methods are runtime descriptor ids; no generated application stands behind it

# --- [MODELS] ---------------------------------------------------------------------------


class Closure(Struct, frozen=True, gc=False):
    # ONE registration of a closed family against the row table keyed on it. The family and the table sit ABOVE this
    # module on the import rail, so the composition root that already runs the census hands both down — reaching upward
    # from here for `BINDINGS`, `CLASSIFICATION_ROWS`, a bound `EventFormat.rows`, `_FAULT_STATUS`, or `POLICY` inverts the rail
    # this module roots. A DELIBERATELY partial table registers NO row: `CIRCUIT` and `RATES` declare their own
    # absence as the policy, so folding them in here would refuse the process their landed partiality exists to serve.
    table: str
    members: frozenset[str]
    rostered: frozenset[str]

    @staticmethod
    def of(table: str, family: "type[StrEnum] | tuple[str, ...]", rostered: Map[str, object]) -> "Closure":
        # ONE seat for both closed-family spellings the branch mints — a `StrEnum` iterates its own members and a
        # `Literal` family arrives as its `get_args` tuple, each a set of `str` either way — so a root registering a
        # table picks no lifting and two registrations can never disagree about what a family's member set is.
        return Closure(table=table, members=frozenset(family), rostered=frozenset(rostered.keys()))

# --- [TABLES] ---------------------------------------------------------------------------

# THE descriptor seat: every generated file this branch binds, dependencies first, seated once at import. `Any.pack`
# needs no registry, but `to_json` over a packed `Any`, `from_json` of a `@type`, `ErrorDetail.value(REGISTRY)`, and
# `REGISTRY.service(name)` in the census below all resolve here — a second `Registry(...)` anywhere in the branch is a
# second authority for one type name and the form this seat forecloses.
REGISTRY: Final[Registry] = Registry(
    *(
        module.desc()
        for module in (
            validate_pb, date_pb, error_details_pb, cloudevents_pb, health_pb,
            fault_pb, artifact_pb, compute_pb, descriptor_pb, discovery_pb, appearance_pb, scene_pb, organization_pb, fabrication_pb, declaration_pb, event_pb, parity_pb,
        )
    )
)

# service full name -> its rpc roster, the proof class, and the generated application standing behind it. A dial is
# `/{service}/{method}` as ONE path, so the halves seat on one row and resolve in one pass; `WireMethod` members hash as
# their own text, so the roster reads straight against the descriptor's rpc names with no projection between them. The
# capability leg's "methods" are the broker's per-capability descriptor ids resolved at discovery, so its roster is
# empty BY CONSTRUCTION, its application `None`, and the row declares that no generator can prove it.
SERVICE_VOCABULARY: Final[tuple[tuple[WireService, tuple[WireMethod, ...], ServiceProof, Generated | None], ...]] = (
    (WireService.COMPUTE, (WireMethod.TESSELLATE,), ServiceProof.GENERATED, ComputeServiceASGIApplication),
    (WireService.ARTIFACT, (WireMethod.FETCH, WireMethod.PUT), ServiceProof.GENERATED, ArtifactServiceASGIApplication),
    (WireService.HEALTH, (WireMethod.CHECK,), ServiceProof.GENERATED, HealthASGIApplication),
    (WireService.CAPABILITY_DISCOVERY, (WireMethod.DISCOVER,), ServiceProof.GENERATED, CapabilityDiscoveryServiceASGIApplication),
    (WireService.CAPABILITY, (), ServiceProof.BROKERED, None),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _closed(row: Closure) -> Block[str]:
    # closure coverage, the census this gate owes beside the service rows: a table keyed on a closed family answers a
    # `try_find` absence or a defaulted status for an unrostered member, so the member that was never rowed surfaces at a
    # caller's first dial rather than at boot. Both directions read — a rostered key outside the family is the same stale
    # cell from the other side, which an enum-keyed table forecloses statically and a runtime-built one does not.
    unrostered = Block.of_seq(sorted(row.members - row.rostered)).map(lambda gap: f"{row.table}.{gap}:closure-member-unrostered")
    stale = Block.of_seq(sorted(row.rostered - row.members)).map(lambda gap: f"{row.table}.{gap}:row-outside-closure")
    return unrostered.append(stale)


def _service(name: WireService, methods: tuple[WireMethod, ...], proof: ServiceProof, application: Generated | None) -> Block[str]:
    # TWO proofs per generated row, read off two generated surfaces: the application's `path` property spells
    # `/{service}` — read off an instance constructed over a stub service, a construction that touches no socket — and
    # the corpus `DescService` the `REGISTRY` resolves under the row's name carries the rpc roster. Every selected rpc
    # must be generated; unselected upstream methods remain support closure and do not manufacture a runtime actor. A
    # BROKERED row short-circuits before any construction: there is nothing generated for it to miss.
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
    # ONE boot answer over the service registry and every closed family a caller registers: a service row proves against
    # the generated application its dial path names and the corpus descriptor the `REGISTRY` resolves, and a `Closure`
    # row against the table keyed on it. The name spaces stay disjoint, so the duplicate check runs per registry. The
    # closure roster arrives as an argument because those tables sit above this module on the import rail and only the
    # composition root reaches all of them; it costs a boot read over a few small tables and reads no installed state,
    # exactly the price `transport/serve#SERVE` already fixed for seating this census ahead of every install.
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
