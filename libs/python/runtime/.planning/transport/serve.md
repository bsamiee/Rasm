# [PY_RUNTIME_SERVE]

Companion server-host and daemon composition root: `ServerHost` owns the inbound `grpc.aio` lifecycle and the one `_invoke` servicer body every registered method of every arity folds through, `CapabilityInvoke` the descriptor-driven outbound invoke over the C#-generated capability SDK, and `Entrypoint` the daemon boot/serve/drain choreography. It hosts the geometry companion daemon over the corpus gRPC contract on the UDS leg and re-mints nothing it composes.

Wire vocabulary is `transport/shapes#VOCABULARY`'s, the transcode machinery `transport/wire#PROTO_TRANSCODE`'s, causal time `evidence/clock#CLOCK`'s, and the admitted context `execution/admission#CONTEXT`'s. Seam ledgers file the `CredentialPolicy` axis decode and the W3C inbound extraction on this page — the interceptor at this ingress is the one trace-context authority.

## [01]-[INDEX]

- [02]-[SERVE]: the inbound server-host lifecycle, the `Route` roster, the arity-spanning servicer body under one duration weave, the two-directional `CredentialPolicy`, and the `FaultDetail` trailer egress with its stamped re-drive verdict.
- [03]-[CAPABILITY_INVOKE]: the descriptor-driven outbound invoke, the `fault_detail` trailer ingress, and the two-axis re-drive gate over the `WIRE` retry row.
- [04]-[ENTRY]: the daemon composition root — railed boot under the structure-first wire gates, supervised serve, the diagnostic-capsule mount, the ordered receipted drain, and the one-shot recipe command.

## [02]-[SERVE]

- Owner: `ServerHost` is the boundary capsule over one `grpc.aio` server with the registered health servicer; a servicer method is a `Route` row — service, method, descriptor id, two registry row names, arity member, railed handler — never a hand-written admit/transcode/abort prologue. It composes the wire codec, the `evidence/clock#CLOCK` `CausalFrame.decode` sole carrier fence, the admitted `RuntimeContext`, and the `FaultDetail` shape, re-minting none.
- Cases: `CredentialPolicy` mints the contract five-row axis under one spelling on both sides of the wire, and each row is constructible at exactly the end it serves through the two projections `server_credentials`/`channel_credentials`, each refusing the opposite direction by name. The UDS serve leg admits `insecure_loopback` alone — peer identity is the kernel-reported `(pid, uid)` the C# `csharp:Rasm.AppHost/Wire/companion#PEER_ADMISSION` reads at accept through `SO_PEERCRED`/`LOCAL_PEERCRED`, never a wire-carried PEM — and the four outbound rows are the client legs a calling host dials: `tls` the verified roots, `mtls` the full `(roots, chain, key)` triple the credential constructor binds, `bearer` the per-call token over the ambient roots, and `composed` the arity-proved bundle of one channel row with its call rows. A dial therefore names a posture or refuses; no path reaches an insecure channel.

Entry: the method roster:

| [INDEX] | [METHOD]                                                                      | [CONTRACT]                                              |
| :-----: | :---------------------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `register(routes) -> RuntimeRail[int]`                                        | roster fold; codec pair resolved, mounted per service   |
|  [02]   | `inbound(ctx) -> RuntimeRail[RuntimeContext]`                                 | admission read; causal decode + `time_remaining()`      |
|  [03]   | `_invoke(codec_pair, request, context, handler) -> RuntimeRail[Block[bytes]]` | decode → railed `handler` → `_framed` encode tail       |
|  [04]   | `settle(servicer_context, context, descriptor, wired) -> T`                   | rail-terminating fold; `FaultDetail` trailer then abort |
|  [05]   | `serve() -> RuntimeRail[None]`                                                | bind UDS, start, flip `SERVING`, await termination      |
|  [06]   | `drain() -> None`                                                             | `NOT_SERVING` first, then `stop(grace)`                 |
|  [07]   | `status(service, serving) -> None`                                            | supervisor flip; one bool onto the two serving states   |
|  [08]   | `CredentialPolicy.server_credentials() -> RuntimeRail[ServerCredentials]`     | loopback → `local_server_credentials(UDS)`              |
|  [09]   | `CredentialPolicy.channel_credentials() -> RuntimeRail[ChannelCredentials]`   | four outbound rows; loopback refuses as an inbound seat |
|  [10]   | `_served(servicer_context, descriptor) -> AsyncIterator[RuntimeContext]`      | shared prologue; refusal settles, contextvars bound     |

 `serve()` refuses an empty roster with a typed `config` fault — never a silent empty bind — signals readiness through its `ready` hook once the health flips land, and awaits termination directly; supervision is the `[04]-[ENTRY]` composition root's. `inbound` lifts `ServicerContext.time_remaining()` into the admitted `Deadline`, feeding the caller-dialed budget to the deadline rail — never an unbounded handler. `_member` mints the `observability/metrics#METRIC` `Metrics.timed(descriptor)` duration aspect once per route over the one `_invoke`, so every method's duration and rail outcome land on the request histogram with no per-handler timing and no per-arity weave; a `BIDI` row rides that same application at per-frame grain, each inbound frame driving the railed handler once and its `Block` return framing onto the response stream, a fault aborting mid-stream through the same `settle` trailer egress. `status` is the one worker-facing flip the `execution/workers#SUPERVISION` actuator drives, so pool liveness advertises through the same servicer a calling host polls and no second health surface exists.

- Auto: this interceptor is the ONE trace-context authority — it extracts the inbound W3C parent and opens the SERVER span natively, so `inbound` re-extracts nothing, no servicer body opens a second scope, and a `Signals.attach` around the handler body re-roots spans the interceptor already parented. Its health filter suppresses liveness noise from a protocol that is actually served — the registered servicer answers `Check`/`Watch`, so the filter claim is real. `enrich` is `set_attributes` inside the interceptor's active scope, not a hook param on `aio_server_interceptor` (which takes none) and not a hand-rolled tracing interceptor. `_served` binds the admitted context onto structlog contextvars for the handler window, so `merge_contextvars` stamps every handler log line with the same admitted identity whichever arity served it.
- Auto: arity survives exactly one read — `_member`'s match over the grpc handler constructors and the servicer signature each demands, the one fork no value discriminates because an async generator's `yield` cannot cross a called coroutine's frame. Past it every route shares one prologue, one woven invoke, one encode tail, and one terminal: `_framed` keys on the handler's own returned value, a `Block` framing as many under first-miss encode and any other shape lifting to one frame, so a parallel per-arity invoke, weave, alias, and dispatch have nothing left to hold.
- Law: EVERY fence on this page names the provider classes it reaches and rides a rostered raise. `catch` carries no default at the `reliability/faults#FAULT` owner, so this module spells its two planes once — `_WIRE_RAISES` for the gRPC-and-socket surface both the serve leg and the outbound dial reach, `_CODEC_RAISES` for the decode surface — and the daemon drain fold is the ONE catch-all the plane is allowed, because its stages are caller-supplied owners whose raise surface this runtime cannot enumerate. Every refusal resolves a `FaultRow` anchor and derives its subject from that row's own `Leg`, so a fence cannot spell a coordinate its package never declared and the free-string subjects retire with the literal constructions that carried them.
- Growth: a new servicer method is one `Route` row, a streaming method the same row with its `arity` member; a new stream shape is one `RouteArity` member with one `_member` arm and one body over the standing invoke; a new wire message is one shapes registry row; a new `FaultTag` member is one `_FAULT_STATUS` row and nothing else, the table being total by construction and read as a raw index, and that row carries its own re-drive membership with it because `_REDRIVEN` derives from it; a new refusal on this page is one `FaultRow` anchor at the `reliability/faults#FAULT` roster called through `raised`, never a literal construction; a new outbound credential posture is one `CredentialPolicy` case with one `channel_credentials` arm; a new health surface is automatic with the lifecycle; a new span dimension is one `enrich` key; a new compression algorithm is one `grpc.Compression` member at construction.
- Boundary: the wire contract is the corpus-homed `.proto` — the runtime mints no transport, no channel, and no second wire vocabulary; host lifecycle and product telemetry export stay with the composing application.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import AsyncIterator, Awaitable, Callable
from contextlib import asynccontextmanager
from datetime import UTC, datetime, timedelta
from enum import StrEnum
from typing import Final, Literal, Self, assert_never

import grpc
import msgspec
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from google.protobuf.message import Message
from grpc_health.v1 import health, health_pb2, health_pb2_grpc
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.instrumentation.grpc import aio_server_interceptor, filters
import structlog

from rasm.runtime.admission import Deadline, RuntimeContext, RuntimeProfile
from rasm.runtime.clock import CausalFrame
from rasm.runtime.faults import (
    FAULT_OWNER, FAULT_TAG, SERVE_ANCHOR, SERVE_BUNDLE, SERVE_DIRECTION, SERVE_ROSTER,
    BoundaryFault, Catch, Disposition, FaultTag, RuntimeRail, traversed,
)
from rasm.runtime.metrics import Metrics
from rasm.runtime.shapes import FaultDetail, FaultRecovery
from rasm.runtime.wire import WireProtoCodec, codec

# --- [TYPES] ----------------------------------------------------------------------------

type RailHandler = Callable[[Struct, RuntimeContext], Awaitable[RuntimeRail[Struct]]]
type StreamHandler = Callable[[Struct, RuntimeContext], Awaitable[RuntimeRail[Block[Struct]]]]
type CodecPair = tuple[WireProtoCodec[Struct, Message], WireProtoCodec[Struct, Message]]
# ONE invoke alias over both handler shapes, returning the framed response every arity carries — a unary reply is a
# one-frame `Block`. A second alias per arity would re-describe the fork the invoke body already absorbs, and the
# weave that wraps it is `Metrics.timed`, which preserves this exact signature through `**P`.
type Invoke = Callable[[CodecPair, bytes, RuntimeContext, RailHandler | StreamHandler], Awaitable[RuntimeRail[Block[bytes]]]]
# the two servicer signatures grpc itself declares — one payload in, one payload out against an iterator in, an
# iterator out; each body composes the same prologue, invoke, and terminal, so only these shapes differ.
type UnaryBehavior = Callable[[bytes, grpc.aio.ServicerContext], Awaitable[bytes]]
type StreamBehavior = Callable[[AsyncIterator[bytes], grpc.aio.ServicerContext], AsyncIterator[bytes]]


# method arity is the proto contract's own declaration carried as row data; the mount reads the member to pick the grpc
# handler constructor and the servicer signature it demands, and nothing downstream reads it again — the framing
# discriminant past that seam is the handler's own returned value.
class RouteArity(StrEnum):
    UNARY = "unary"
    BIDI = "bidi"

# --- [CONSTANTS] ------------------------------------------------------------------------

# round-trip's two symbolic anchors: settle packs them, the invoke ingress reads them.
_DETAIL_KEY: Final[str] = "grpc-status-details-bin"
_FAULT_DETAIL: Final[str] = "fault_detail"
# the ONE span-attribute key this module owns; every fault key it stamps arrives from the fault owner's rostered
# `FAULT_*` set, the same reason tenancy rides the metric owner's `TENANT_BAGGAGE` — one canonical spelling every
# plane reads, where a per-site literal drifts one end of a join a backend can no longer make. The retired
# `rasm.fault_case` was exactly that literal: it spelled a key nothing else in the estate carried, while `FAULT_TAG`
# names the coordinate the C# kernel and the TypeScript convention roster both publish. The fault CASE stays LOCAL
# and never a wire column — the compact `FaultDetail` transports `code` as its sole identity, so the case survives
# on this host's own span where a backend joins it, exactly where the contract wants it kept.
_DESCRIPTOR_ATTR: Final[str] = "rasm.descriptor"
# C# Instant.ToUnixTimeTicks is the 100 ns unit; the trailer echo truncates to Timestamp
# microseconds — the carrier headers stay the authoritative full-fidelity stamp.
_TICKS_PER_SECOND: Final[int] = 10_000_000
# the two provider planes every fence on this page names, spelled ONCE each because `catch` is REQUIRED on all three
# lift shapes and a per-site tuple drifts one seam's raise surface from its sibling's. `grpc.RpcError` is the base
# `aio.AioRpcError` extends, so one row covers both legs, and `OSError` is the socket half a dial or a bind reaches.
# Neither set widens past `Exception`: cancellation is scope-owned flow control the faults owner forbids converting.
_WIRE_RAISES: Final[Catch] = (grpc.RpcError, OSError)
_CODEC_RAISES: Final[Catch] = (msgspec.ValidationError, msgspec.DecodeError)

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class CredentialPolicy:
    # one axis, two directions, each case constructible at exactly the end it serves: `insecure_loopback` carries the
    # UDS serve leg where peer identity is the kernel-reported `(pid, uid)`, and the four outbound rows carry the PEM
    # roots, the client cert/key pair, the call-credential token, and the fold that composes a channel credential with
    # any number of call credentials. Both projections refuse the opposite direction by name, so a policy value can
    # never mount at the end it was not minted for and the refusal names which end rejected it.
    tag: Literal["insecure_loopback", "tls", "mtls", "bearer", "composed"] = tag()
    insecure_loopback: bool = case()
    tls: str = case()  # PEM root-certificate bundle the client verifies the server against
    mtls: tuple[str, str, str] = case()  # (root bundle PEM, client certificate chain PEM, client private key PEM)
    bearer: str = case()  # per-call access token; rides `access_token_call_credentials` over the ambient channel credential
    composed: tuple["CredentialPolicy", ...] = case()  # one channel row folded with every call row beside it

    @classmethod
    def loopback(cls) -> Self:
        return cls(insecure_loopback=True)

    @classmethod
    def bundled(cls, *rows: "CredentialPolicy") -> Self:
        return cls(composed=rows)

    def server_credentials(self) -> RuntimeRail[grpc.ServerCredentials]:
        match self:
            case CredentialPolicy(tag="insecure_loopback"):
                return Ok(grpc.local_server_credentials(grpc.LocalConnectionType.UDS))
            case CredentialPolicy(tag="tls" | "mtls" | "bearer" | "composed" as outbound):
                # `SERVE_DIRECTION` is the ONE row both projections raise through, its two coordinates naming the row
                # offered and the seat that refused it, so the mirrored refusals cannot drift into two spellings.
                return Error(SERVE_DIRECTION.raised(outbound, "inbound"))
            case _ as unreachable:
                assert_never(unreachable)

    def channel_credentials(self) -> RuntimeRail[grpc.ChannelCredentials]:
        # the outbound mirror of `server_credentials`, and what makes the four client rows reachable: without it the
        # union carried one constructible case and every dial was unauthenticated by construction with no boundary line
        # saying so. `composed` folds its rows through `composite_channel_credentials`, whose contract takes ONE channel
        # credential followed by call credentials — so the fold seeds on the first row's channel projection and
        # accumulates each remaining row's call half, and a bundle of pure call credentials with no channel row to
        # anchor them refuses rather than dialing under whatever ambient roots the process happens to hold.
        match self:
            case CredentialPolicy(tag="tls", tls=roots):
                return Ok(grpc.ssl_channel_credentials(root_certificates=roots.encode()))
            case CredentialPolicy(tag="mtls", mtls=(roots, chain, key)):
                return Ok(grpc.ssl_channel_credentials(root_certificates=roots.encode(), private_key=key.encode(), certificate_chain=chain.encode()))
            case CredentialPolicy(tag="bearer", bearer=token):
                return Ok(grpc.composite_channel_credentials(grpc.ssl_channel_credentials(), grpc.access_token_call_credentials(token)))
            case CredentialPolicy(tag="composed", composed=rows):
                return _bundled(Block.of_seq(rows))
            case CredentialPolicy(tag="insecure_loopback"):
                return Error(SERVE_DIRECTION.raised("insecure_loopback", "outbound"))
            case _ as unreachable:
                assert_never(unreachable)


class Route(Struct, frozen=True):
    # one servicer method as data: `request`/`response` resolve through the wire registry, `handler` binds into the dispatch aspect;
    # `arity` witnesses the handler shape — UNARY rows carry a RailHandler, BIDI rows a StreamHandler.
    service: str
    method: str
    descriptor: str
    request: str
    response: str
    handler: RailHandler | StreamHandler
    arity: RouteArity = RouteArity.UNARY


# --- [TABLES] ---------------------------------------------------------------------------

# TOTAL over `FaultTag` by construction — one row per member of the closed vocabulary — which is what lets `settle`
# read it as a raw index. A defaulted lookup here is a catch-all over an OWNED closure: a member carrying no row
# would answer INTERNAL on the wire for the process's whole life and nothing anywhere would report the gap.
# `domain` carries a SIBLING's own refusal token, which refuses the request's STATE rather than its shape, so it maps
# FAILED_PRECONDITION beside `config` instead of joining `wire`/`boundary` on INVALID_ARGUMENT.
_FAULT_STATUS: Final[Map[FaultTag, grpc.StatusCode]] = Map.of_seq([
    ("config", grpc.StatusCode.FAILED_PRECONDITION),
    ("resource", grpc.StatusCode.UNAVAILABLE),
    ("deadline", grpc.StatusCode.DEADLINE_EXCEEDED),
    ("api", grpc.StatusCode.INTERNAL),
    ("import_", grpc.StatusCode.UNIMPLEMENTED),
    ("wire", grpc.StatusCode.INVALID_ARGUMENT),
    ("boundary", grpc.StatusCode.INVALID_ARGUMENT),
    ("domain", grpc.StatusCode.FAILED_PRECONDITION),
    ("aggregate", grpc.StatusCode.INTERNAL),
])

# Fault classes THIS server's own refusal grades re-drivable, DERIVED from the status table above rather than
# spelled beside it: a status a later attempt can clear names a class a later attempt can clear, so one authority
# answers both columns and a new `_FAULT_STATUS` row joins or stays out by its own status. RESOURCE_EXHAUSTED
# completes the peer client's transient trio and no row sends it — this host sheds through the breaker rather than
# answering exhausted — so naming it here would seat a membership no producer reaches. This set is the FLOOR and
# never the last word: `BoundaryFault.retriability` reads a rostered raise's own declared posture ahead of it, so a
# `domain` token sitting outside this derivation still grades re-drivable where the folder that raised it says so —
# the defect knows what a re-offer clears, and this table only states what a gRPC status can.
_REDRIVEN: Final[frozenset[FaultTag]] = frozenset(
    tag for tag, status in _FAULT_STATUS.items() if status in (grpc.StatusCode.UNAVAILABLE, grpc.StatusCode.DEADLINE_EXCEEDED)
)

# --- [SERVICES] -------------------------------------------------------------------------


class ServerHost:
    def __init__(
        self, bind: str, credential: CredentialPolicy | None = None, grace: float = 5.0, compression: grpc.Compression = grpc.Compression.Gzip
    ) -> None:
        self._bind, self._credential, self._grace = bind, credential or CredentialPolicy.loopback(), grace
        interceptor = aio_server_interceptor(filter_=filters.negate(filters.health_check()))
        self._server: grpc.aio.Server = grpc.aio.server(interceptors=[interceptor], compression=compression)
        self._health = health.aio.HealthServicer()
        self._services: frozenset[str] = frozenset()
        health_pb2_grpc.add_HealthServicer_to_server(self._health, self._server)

    def register(self, routes: Block[Route]) -> RuntimeRail[int]:
        # ACCUMULATE: one accumulated fault names EVERY unresolvable row, never first-miss.
        resolved = traversed(
            routes.map(lambda row: codec(row.request).bind(lambda req: codec(row.response).map(lambda res: (row, (req, res))))),
            by=Disposition.ACCUMULATE,
        )
        return resolved.map(self._mounted)

    def _mounted(self, rows: Block[tuple[Route, CodecPair]]) -> int:
        for service in sorted({row.service for row, _ in rows}):  # Exemption: grpc handler registration is the host's mutating seam.
            members = {row.method: self._member(row, pair) for row, pair in rows if row.service == service}
            self._server.add_registered_method_handlers(service, members)
        self._services = self._services | {row.service for row, _ in rows}
        return len(rows)

    def _member(self, row: Route, pair: CodecPair) -> grpc.RpcMethodHandler:
        # ONE registration-time weave for every route — `Metrics.timed(row.descriptor)` over the one `_invoke`, so a
        # unary call records at whole-call grain and a streamed frame at per-frame grain off a single aspect application
        # and no body names a timing verb. Arity is read EXACTLY here and never again: grpc mints a distinct handler
        # constructor per stream shape and demands the servicer signature matching it, and an async generator's `yield`
        # cannot cross a called coroutine's frame, so this is the one fork no value absorbs. Past it both bodies share
        # the whole pipeline — `_served`'s admission and contextvars bind, one `_invoke`, one `settle` terminal.
        invoke: Invoke = Metrics.timed(row.descriptor)(ServerHost._invoke)
        match row.arity:
            case RouteArity.UNARY:
                return grpc.unary_unary_rpc_method_handler(_unary(row, pair, invoke))
            case RouteArity.BIDI:
                return grpc.stream_stream_rpc_method_handler(_streamed(row, pair, invoke))
            case _ as unreachable:
                assert_never(unreachable)

    @staticmethod
    def inbound(servicer_context: grpc.aio.ServicerContext) -> RuntimeRail[RuntimeContext]:
        # one inbound map, three disjoint reads: the causal frame off the `SLOTS` slots, the deadline off the call, and the
        # W3C context the admitted `Correlation` adopts — the interceptor parents the ambient span off the same headers, so
        # withholding the carrier here would root a fresh trace beside a remote-parented span and fracture the two apart.
        # `invocation_metadata()` is a MULTIMAP whose `-bin` keys carry raw `bytes`, so a bare `dict(...)` types a lie the
        # `str`-keyed carrier contract then propagates; the admission takes str-valued entries alone and every binary lane
        # reads through its own typed accessor. Both the W3C keys and the `SLOTS` slots are single-valued by their own
        # contracts, so the last-wins collapse a comprehension performs settles nothing either reader disputes.
        carrier = {key: value for key, value in servicer_context.invocation_metadata() if isinstance(value, str)}
        budget = Option.of_optional(servicer_context.time_remaining()).map(lambda remaining: Deadline(timedelta(seconds=remaining)))
        # the causal read answers `Option` and admission takes `Option`, so the carrier threads UNCONVERTED: a call
        # with no causal headers admits `Nothing` rather than a zero-stamp frame, and a locally-minted request and an
        # epoch-stamped one stay two states the whole way through the context.
        return CausalFrame.decode(carrier).map(
            lambda causal: RuntimeContext.admit(RuntimeProfile.SIDECAR, deadline=budget.to_optional(), causal=causal, carrier=carrier)
        )

    @staticmethod
    def enrich(context: RuntimeContext, descriptor: str, refused: Option[BoundaryFault] = Nothing) -> None:
        # the server end spans what it HOLDS. Absence takes `Nothing` and stamps NO fault key: the retired `"ok"`
        # literal filled a fault dimension on every clean call, which is the empty-value series the branch's optional
        # -dimension law forbids. `owner` resolves the emitting leg off the census, so a fault whose subject no
        # module seated omits that key too rather than naming a leg nothing declared.
        seat = refused.map(lambda fault: {FAULT_TAG: fault.tag} | fault.owner.map(lambda leg: {FAULT_OWNER: leg}).default_value({}))
        trace.get_current_span().set_attributes(context.attribute() | {_DESCRIPTOR_ATTR: descriptor} | seat.default_value({}))

    @staticmethod
    async def settle[T](servicer_context: grpc.aio.ServicerContext, context: RuntimeContext, descriptor: str, wired: RuntimeRail[T]) -> T:
        # generic over the terminated payload, so one fold ends a unary reply's `bytes`, a mid-stream frame batch's
        # `Block[bytes]`, and an admission refusal alike — the Error arm aborts and returns nothing, so the payload type
        # only ever names what the Ok arm hands back and no arm launders a rail through a discarding `map`.
        match wired:
            case Result(tag="ok", ok=payload):
                ServerHost.enrich(context, descriptor)
                return payload
            case Result(tag="error", error=fault):
                ServerHost.enrich(context, descriptor, Some(fault))
                # raw index, never a defaulted lookup: the table declares one row per `FaultTag` member, so a member
                # that gains no row must break the read rather than answer INTERNAL for the process's whole life.
                status = _FAULT_STATUS[fault.tag]
                # a failed trailer encode skips the trailer, never the abort; the details string
                # still carries the human-readable facts. abort is NoReturn — nothing follows it.
                codec(_FAULT_DETAIL).bind(lambda sealer: sealer.encode(_sealed(fault, context, status))).map(
                    lambda trailer: servicer_context.set_trailing_metadata(((_DETAIL_KEY, trailer),))
                )
                await servicer_context.abort(status, "; ".join(f"{k}={v}" for k, v in fault.facts().items()))
            case _ as unreachable:
                assert_never(unreachable)

    @staticmethod
    async def _invoke[S: Struct, M: Message, R: Struct, N: Message](
        codec_pair: tuple[WireProtoCodec[S, M], WireProtoCodec[R, N]],
        request: bytes,
        context: RuntimeContext,
        handler: Callable[[S, RuntimeContext], Awaitable[RuntimeRail[R]]] | Callable[[S, RuntimeContext], Awaitable[RuntimeRail[Block[R]]]],
    ) -> RuntimeRail[Block[bytes]]:
        # ONE invoke for every arity: decode one request, drive the railed handler, frame its return. Both bodies were
        # the same body — same decode, same refusal arm, same railed drive — differing only in the encode tail, and
        # that tail is `_framed`'s business, keyed on the value the handler actually returned. So no `arity` parameter
        # rides this signature: a discriminant the value already carries is the knob the removal test deletes.
        decode, encode = codec_pair
        match decode.decode(request):
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=shape):
                return (await handler(shape, context)).bind(lambda produced: _framed(encode, produced))
            case _ as unreachable:
                assert_never(unreachable)

    async def serve(self, ready: Callable[[], Awaitable[None]] | None = None) -> RuntimeRail[None]:
        if self._services == frozenset():
            return Error(SERVE_ROSTER.raised())
        match self._credential.server_credentials():
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=credentials):
                self._server.add_secure_port(self._bind, credentials)
                await self._server.start()
                for service in (health.OVERALL_HEALTH, *sorted(self._services)):  # Exemption: the health flips are the servicer's own async mutation seam.
                    await self._health.set(service, health_pb2.HealthCheckResponse.SERVING)
                if ready is not None:  # the readiness hook fires after the health flips, so an sd-notify READY never precedes a serving probe
                    await ready()
                await self._server.wait_for_termination()
                return Ok(None)
            case _ as unreachable:
                assert_never(unreachable)

    async def status(self, service: str, serving: bool) -> None:
        # supervisor's one flip surface; after enter_graceful_shutdown the servicer holds NOT_SERVING and this set is a no-op.
        await self._health.set(service, health_pb2.HealthCheckResponse.SERVING if serving else health_pb2.HealthCheckResponse.NOT_SERVING)

    async def drain(self) -> None:
        # NOT_SERVING races ahead of the stop: probes stop routing new work while the grace window
        # drains in-flight calls; the flip is permanent, so a late success cannot re-advertise.
        await self._health.enter_graceful_shutdown()
        await self._server.stop(self._grace)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _bundled(rows: Block[CredentialPolicy]) -> RuntimeRail[grpc.ChannelCredentials]:
    # `composite_channel_credentials` takes exactly ONE channel credential followed by any number of call credentials,
    # so the bundle PROVES that arity rather than trusting a caller's ordering: `tls`/`mtls` are the channel half and
    # `bearer` the call half. A bundle carrying no channel row would dial under whatever ambient roots the process
    # happens to hold, two would silently drop one, and a nested `composed` or an `insecure_loopback` row belongs to
    # neither half — each refuses by name here rather than at the socket, the roster row carrying the kinds actually
    # offered as its own coordinate, which is strictly more evidence than the fixed sentence it replaces.
    anchors, tokens = rows.filter(lambda row: row.tag in ("tls", "mtls")), rows.filter(lambda row: row.tag == "bearer")
    return (
        Error(SERVE_BUNDLE.raised(",".join(sorted({row.tag for row in rows}))))
        if len(anchors) + len(tokens) != len(rows)
        else Error(SERVE_ANCHOR.raised(str(len(anchors))))
        if len(anchors) != 1
        else anchors.head()
        .channel_credentials()
        .map(lambda anchor: grpc.composite_channel_credentials(anchor, *tokens.map(lambda row: grpc.access_token_call_credentials(row.bearer))))
    )


def _framed(encode: WireProtoCodec[Struct, Message], produced: Struct | Block[Struct]) -> RuntimeRail[Block[bytes]]:
    # the whole arity axis, as the one encode tail: a handler that framed its own answer hands back a `Block` and each
    # member encodes first-miss, so a half-encoded batch never reaches the response stream; anything else is one shape
    # lifting to a one-frame `Block`. The discriminant is the RETURNED VALUE, never the route's declared arity, so the
    # roster's `arity` member stays what the platform seam reads and nothing restates it downstream.
    match produced:
        case Block() as frames:
            return traversed(frames.map(encode.encode), by=Disposition.ABORT)
        case shape:
            return encode.encode(shape).map(Block.singleton)


@asynccontextmanager
async def _served(servicer_context: grpc.aio.ServicerContext, descriptor: str) -> AsyncIterator[RuntimeContext]:
    # ONE admission prologue both servicer signatures enter — a context manager rather than a wrapper coroutine,
    # because an async generator's `yield` cannot cross a called coroutine's frame, so a wrapper would fork straight
    # back into the two bodies it exists to join. A refused admission takes the SAME trailer egress a mid-call fault
    # takes, and `settle`'s abort raises, so the body below runs only over an admitted context. The bind binds the
    # admitted identity onto structlog contextvars for the handler window, so `merge_contextvars` stamps every handler
    # log line while the trace ids ride the chain's own span read — never a second bind.
    match ServerHost.inbound(servicer_context):
        case Result(tag="error") as refused:
            await ServerHost.settle(servicer_context, RuntimeContext.admit(RuntimeProfile.SIDECAR), descriptor, refused)
        case Result(tag="ok", ok=context):
            with structlog.contextvars.bound_contextvars(**context.attribute()):
                yield context  # Exemption: the context-manager seam — the platform's own yield-once contract.
        case _ as unreachable:
            assert_never(unreachable)


def _unary(row: Route, pair: CodecPair, invoke: Invoke) -> UnaryBehavior:
    async def method(request: bytes, servicer_context: grpc.aio.ServicerContext) -> bytes:
        async with _served(servicer_context, row.descriptor) as context:
            framed = await invoke(pair, request, context, row.handler)
            return await ServerHost.settle(servicer_context, context, row.descriptor, framed.map(lambda frames: frames.head()))

    return method


def _streamed(row: Route, pair: CodecPair, invoke: Invoke) -> StreamBehavior:
    async def method(request_iterator: AsyncIterator[bytes], servicer_context: grpc.aio.ServicerContext) -> AsyncIterator[bytes]:
        async with _served(servicer_context, row.descriptor) as context:
            async for frame in request_iterator:  # Exemption: the inbound frame walk is the streaming servicer's own seam.
                match await invoke(pair, frame, context, row.handler):
                    case Result(tag="ok", ok=frames):
                        for payload in frames:
                            yield payload
                    case Result(tag="error") as fault:
                        # mid-stream fault rides the same trailer egress; abort raises, ending the generator.
                        await ServerHost.settle(servicer_context, context, row.descriptor, fault)
            ServerHost.enrich(context, row.descriptor)

    return method


def _sealed(fault: BoundaryFault, context: RuntimeContext, status: grpc.StatusCode) -> FaultDetail:
    # one total egress fold onto the COMPACT roster the contract froze: `code` is the SOLE transported identity, so
    # the producer's package, its case token, and the whole `facts()` evidence map stay LOCAL — a peer rehydrating
    # this producer's union is precisely the drift the compaction forecloses. Nothing is lost to an operator: the
    # case rides `FAULT_TAG` on this host's own span and the facts ride the structured log line the same fold
    # writes, both joinable on the `correlation` this trailer does carry. The hlc echo rides Timestamp precision.
    #
    # `recovery` stamps THIS server's re-drive verdict rather than leaving a peer to infer one: a consumer reading
    # INVALID_ARGUMENT cannot tell a malformed payload from a codec that decodes on the next build, and one guessing
    # from the status substitutes its own band table for this classification. It grades through the faults owner's
    # own `retriability` read — that tier's ONE declared precedence, a rostered raise's own row posture over the
    # `FaultTag` derivation — so an `aggregate` folds its members through `Recovery.widest` and no second classifier
    # restates the fault family here. `throttled` is the arm a producer that MEASURES a window mints; this host
    # measures none, so `retriability` answers terminal or transient alone and no fabricated wait ever crosses.
    facts = fault.facts()
    return FaultDetail(
        code=status.value[0],
        message=str(facts.get("detail") or facts.get("cause") or facts.get("case") or facts.get("members") or facts.get("subject") or ""),
        correlation=context.correlation.trace_id.hex(),
        hlc_physical=context.causal.map(lambda frame: datetime.fromtimestamp(frame.hlc.physical_ticks / _TICKS_PER_SECOND, tz=UTC)).to_optional(),
        hlc_logical=context.causal.map(lambda frame: frame.hlc.logical).default_value(0),
        tenant=context.causal.map(lambda frame: str(frame.tenant)).default_value(""),
        recovery=FaultRecovery.of(fault.retriability(_REDRIVEN)),
    )
```

## [03]-[CAPABILITY_INVOKE]

- Owner: `CapabilityInvoke` decodes the C# `csharp:Rasm.AppHost/Agent/capability#SDK_CODEGEN` Python target into one dispatch — the request's own `Struct` type and the caller's `into` type are the codec discriminants off the `_ROW_BY_STRUCT` table, so one `run` spans every `PROTO_VOCABULARY` row and no injected per-shape codec pair narrows it; a shape outside that registry refuses by name on the rail rather than dialing untranscoded. Outbound legs retry under `_WIRE_CALLER` — the `RetryClass.WIRE` row's own schedule bound to this seam's verdict hook — with a two-fence ingress, because a `guarded(...)` wrap and the terminal fence alike consume the exception AFTER the budget is gone, so no bare gRPC exception escapes and no trailer erases to a bare `boundary` tag.
- Law: RE-DRIVE IS TWO AXES AND NEVER A BOOL — the class's route (the gRPC status the `RetryClass.WIRE` row grades) crossed with the producer's own `Recovery` verdict off the decoded trailer, and `_redrive` is the one place both are readable at once. `terminal` spends no further attempt even inside the transient trio, `throttled` rides back as the delay `stamina` waits in place of its own curve, and every other verdict defers to the row, so the overlay narrows or re-times the class curve and never widens it. `_detail` is the ONE trailer read and `_stated` the ONE verdict lift over it, both composed by the gate and the terminal lift alike, so the posture a retry acted on and the fault a caller receives can never be decoded two different ways. That verdict is the THIRD precedence rung the fault family declares and cannot carry: `reliability/faults#FAULT` owns the two rungs it can see and routes the peer-stated one here, where a live trailer exists to read; absence rides `Option` rather than a fourth case, so an unstated posture never masquerades as a stated one.
- Cases: the per-descriptor `input_schema` is the C# `SuiteContracts.Schema` JSON Schema carried as a deferred `msgspec.Raw` slot, so the routing decode never pays the schema-document parse; the argument payload is the already-typed canonical `Struct` the resolved codec transcodes, never a hand-mirrored mapping re-validated against a schema document. `effect`/`idempotency`/cost-unit keys decode as the C# smart-enum string keys.
- Entry: the per-call descriptor dimension rides the interceptor-set `rpc.service`/`rpc.method` attributes natively — the invoke path IS `/{WireService.CAPABILITY}/{descriptor_id}` — while the channel-stable hooks enrich tenant and fault case on the CLIENT span; an ambient per-call `set_attribute` lands on whatever span was current BEFORE the CLIENT span opened. That service name is the ONE `SERVICE_VOCABULARY` row carrying no descriptor proof: the broker mints a method per capability at discovery, so the boot gate declares the row unpooled rather than failing on a compiled service that was never emitted.
- Packages: `msgspec`, `grpcio`, `stamina`, `opentelemetry-instrumentation-grpc`, and the shapes/wire/resilience/receipts/metrics/faults rails per the fence imports; the `RetryClass.WIRE` policy row is branch-consumer law by the resilience owner's own boundary, and this seam binds a caller from it rather than re-spelling a schedule.
- Growth: a new capability is one descriptor row the `Rasm.AppHost` capability broker folds — this branch reads it through the existing `discover`/`run` pair; a new wire shape reaches the invoke through one shapes registry row with zero edits here; a new span dimension is one hook key; a new composition is one `ScopeKey` value threaded through `connect` and `drained`, never a second registry; a new re-drive posture is one case on the `reliability/faults#FAULT` `Recovery` family with one `_redrive` arm, the retry curve untouched; a new remote ingress shape is one `RemoteConflict` case, which every consumer matching that union then owes an arm.
- Boundary: `connect` mints the channel, so this owner owes its teardown — every dial enrols on the live registry under its own composition scope, the `[04]-[ENTRY]` drain fold names that scope as its own stage, and `aclose` retires its row so an early caller close never double-closes and a sibling composition's channels are never reached. A channel whose only teardown is the caller's memory is the leak the transport owner's pooled clients already refuse, and the two teardowns read as one row pair rather than one rescued and one forgotten. The descriptor is the suite's only op-metadata owner and the capability broker its sole mint, named by the brokered-capability domain it holds; this branch re-authors no capability shape. Cross-language shape identity is the broker's `SuiteContracts.Schema` JSON Schema all three SDKs bind, evolution riding the contract's additive-only rule. Channel liveness rides the `WIRE` row's `UNAVAILABLE` transient, so no client `HealthStub` pre-probe rides `connect`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable
from typing import Final, Literal, Self, assert_never

import anyio
import grpc
import msgspec
import stamina
from expression import Error, Nothing, Ok, Option, Result, case, tag, tagged_union
from expression.collections import Block, Map
from google.protobuf.message import Message
from msgspec import Raw, Struct
from opentelemetry import trace
from opentelemetry.instrumentation.grpc import aio_client_interceptors, filters

from rasm.runtime.clock import Tenant
from rasm.runtime.faults import (
    FAULT_TAG, SERVE_CATALOG, SERVE_DIAL, SERVE_DISCOVERY, SERVE_REGISTRY, SERVE_REMOTE,
    BoundaryFault, Recovery, RuntimeRail, async_boundary, boundary,
)
from rasm.runtime.metrics import TENANT_BAGGAGE
from rasm.runtime.receipts import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.resilience import RetryClass
from rasm.runtime.shapes import PROTO_VOCABULARY, FaultDetail, FaultRecovery, WireService
from rasm.runtime.wire import WireProtoCodec, codec

# `_DETAIL_KEY`/`_FAULT_DETAIL` are this module's [02]-[SERVE] constants — one trailer spelling for
# egress pack and ingress lift beside the fault owner's rostered `FAULT_*` span keys both planes stamp — `_CODEC_RAISES`/`_WIRE_RAISES` its
# two provider planes, and `CredentialPolicy` its owner of this dial's two-directional credential axis. `Recovery` is
# the RUNTIME owner's re-offer family and `FaultRecovery` the wire mirror carrying both directions of their
# correspondence, so this seam decodes a verdict and never mints a second vocabulary for one.

# --- [TYPES] ----------------------------------------------------------------------------

type CostUnitKey = Literal["cpu-millis", "wall-millis", "bytes-egress", "model-tokens", "calls"]
type MeterVector = dict[CostUnitKey, int]
type CommandTxnKind = Literal["committed", "rolled_back", "compensated", "refused"]
type WireDispatch = Callable[[str, bytes], Awaitable[RuntimeRail[bytes]]]

# --- [MODELS] ---------------------------------------------------------------------------


class CommandTxn(Struct, frozen=True, rename="camel"):
    kind: CommandTxnKind
    detail: str = ""


class DiscoveryResult(Struct, frozen=True, rename="camel"):
    descriptor: str
    surface: str
    effect: Literal["pure", "read", "write", "external", "irreversible"]
    idempotency: Literal["idempotent", "keyed", "single-shot", "non-idempotent"]
    estimated: MeterVector
    scope_hash: str
    input_schema: Raw = Raw(b"{}")


class CommandReceipt(Struct, frozen=True, rename="camel"):
    descriptor: str
    txn: CommandTxn
    charged: MeterVector
    elapsed: str
    correlation: str


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class RemoteConflict(BaseException):
    # the PEER's own refusal carried WHOLE across the funnel, exactly as a folder fault union crosses it: `Tagged` is
    # structural, so `BoundaryFault.of` seats this ahead of every `CLASSIFY` row and `facts()` publishes the active
    # case and its payload as typed evidence. Concatenating the columns into one `wire` subject string is the form
    # this forecloses: `correlation`, the two HLC halves, and `tenant` decode and then vanish, leaving a total egress
    # fold facing a lossy ingress fold — a round trip that does not round-trip.
    # The producer's verdict is NOT carried beside the detail: `recovery` is the detail's own slot and `_stated` its
    # ONE read, so no mirror of the peer's classification exists here to drift from the cell it came from. The
    # transport status IS carried on both arms, because the contract rules that remote domain codes never elect
    # topology while transport outcomes drive failover — two facts a single code column would fuse.
    tag: Literal["stated", "coded"] = tag()
    stated: tuple[FaultDetail, int] = case()  # (the decoded conflict — code, message, correlation, hlc pair, tenant, recovery; this call's status)
    coded: tuple[int, str] = case()  # no trailer crossed, or none this codec decodes: the status and the peer's own details line


# --- [TABLES] ---------------------------------------------------------------------------

# derived from the one shapes seed table: the struct TYPE is the discriminant `run` resolves a
# codec by, so one invoke spans the whole catalog with no injected per-shape codec pair.
_ROW_BY_STRUCT: Final[Map[type[Struct], str]] = Map.of_seq((struct, name) for name, struct, _ in PROTO_VOCABULARY)

# Outbound retry POLICY for this seam, read whole off `reliability/resilience#RESILIENCE`: attempts, timeout, backoff
# curve, and the transience read this seam falls back to all arrive as that page's `RetryClass.WIRE` row, so nothing
# here re-spells a schedule and a table edit lands on this dial with no consumer touch.
_WIRE_ROW: Final = RetryClass.WIRE.policy

# --- [SERVICES] -------------------------------------------------------------------------


class CapabilityInvoke:
    # distinctly named so the instance-level lookup `Map` `run` reads never shadows the class decoder.
    _DISCOVERY: msgspec.json.Decoder[list[DiscoveryResult]] = msgspec.json.Decoder(list[DiscoveryResult])

    def __init__(
        self, catalog: Map[str, DiscoveryResult], dispatch: WireDispatch, channel: grpc.aio.Channel | None = None, scope: ScopeKey = DEFAULT_SCOPE
    ) -> None:
        self._catalog, self._dispatch, self._channel, self.scope = catalog, dispatch, channel, scope

    @classmethod
    def discover(cls, payload: bytes) -> RuntimeRail[Map[str, DiscoveryResult]]:
        # descriptor-keyed fold lives at the one decode site, never a list-to-Map re-key the composition root hand-spells.
        return boundary(SERVE_DISCOVERY, lambda: cls._DISCOVERY.decode(payload), catch=_CODEC_RAISES).map(
            lambda rows: Map.of_seq((row.descriptor, row) for row in rows)
        )

    @staticmethod
    def interceptors(tenant: Tenant) -> list[grpc.aio.ClientInterceptor]:
        # channel-stable enrichment only; the per-call descriptor rides the interceptor-set
        # `rpc.service`/`rpc.method` off the `/rasm.capability/{descriptor_id}` path natively.
        def request_hook(span: trace.Span, _request: object) -> None:
            span.set_attribute(TENANT_BAGGAGE, str(tenant))

        def response_hook(span: trace.Span, details: str) -> None:
            # the client end spans only what it TRULY has: this hook receives the trailer's rendered `details` string
            # and never the decoded `FaultDetail`, so `FAULT_CODE` and `FAULT_POSTURE` — which need that decode —
            # stay with `_unsealed`, where it happens. A clean call OMITS the key rather than filling it with an "ok"
            # no fault took, and no decode is forged here to reach the two keys this end cannot honestly answer.
            if details:
                span.set_attribute(FAULT_TAG, details)

        return aio_client_interceptors(filter_=filters.negate(filters.health_check()), request_hook=request_hook, response_hook=response_hook)

    @classmethod
    def connect(
        cls, target: str, catalog: Map[str, DiscoveryResult], tenant: Tenant, credential: CredentialPolicy, *, scope: ScopeKey = DEFAULT_SCOPE
    ) -> RuntimeRail[Self]:
        # the dial is CREDENTIALED by construction: the caller's policy projects to a channel credential or refuses on
        # the rail before any socket exists, so `insecure_channel` appears on no path here and an unauthenticated
        # outbound leg is unspellable rather than the silent default this seam used to carry. The refusal names which
        # case rejected it — `insecure_loopback` is the inbound seat, and a malformed bundle names its own arity.
        # `scope` is the composition this dial belongs to, and it is what the drain later reaches this channel by.
        def dialed(credentials: grpc.ChannelCredentials) -> Self:
            channel = grpc.aio.secure_channel(target, credentials, interceptors=cls.interceptors(tenant))

            async def dispatch(descriptor_id: str, request: bytes) -> RuntimeRail[bytes]:
                method = channel.unary_unary(f"/{WireService.CAPABILITY}/{descriptor_id}")

                async def called() -> RuntimeRail[bytes]:
                    # Exemption: the trailer fence — grpc-status-details-bin lives only on the live AioRpcError, so this one
                    # platform-forced except reclassifies the terminal raise AFTER the WIRE-row retry exhausts. The retry
                    # itself already read that same trailer through `_redrive`, so a producer-terminal fault arrives here
                    # having spent one attempt rather than the row's whole budget.
                    try:
                        return Ok(await _WIRE_CALLER(method, request))
                    except grpc.aio.AioRpcError as terminal:
                        return Error(_unsealed(terminal))

                return (await async_boundary(SERVE_DIAL, called, catch=_WIRE_RAISES)).bind(lambda rail: rail)

            return _enrolled(cls(catalog, dispatch, channel, scope))

        return credential.channel_credentials().map(dialed)

    async def run[S: Struct, R: Struct](self, descriptor_id: str, request: S, into: type[R]) -> RuntimeRail[R]:
        staged = (
            self._catalog.try_find(descriptor_id)
            .to_result(SERVE_CATALOG.raised(descriptor_id))
            .bind(lambda _: _transcoder(type(request)))
            .bind(lambda transcode: transcode.encode(request))
        )
        match staged:
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=payload):
                # registry row pins struct identity, so the `into`-resolved decode returns the caller's typed R.
                return (await self._dispatch(descriptor_id, payload)).bind(lambda wire: _transcoder(into).bind(lambda transcode: transcode.decode(wire)))
            case _ as unreachable:
                assert_never(unreachable)

    async def aclose(self) -> None:
        # runtime-lived channel's deterministic drain; a directly-injected dispatch carries no channel, a typed no-op.
        _retired(self)
        if self._channel is not None:
            await self._channel.close()


# --- [OPERATIONS] -----------------------------------------------------------------------

# every dialed invoke, PARTITIONED by the composition that dialed it. `connect` mints the channel, so this owner owes
# its teardown exactly as the transport owner owes its pooled clients, and a leaked `grpc.aio.Channel` keeps a live
# connection past the process's own shutdown — but a process-wide set made one composition's drain close a channel an
# embedding host was still serving calls over, so the registry keys on the branch's one custody axis exactly as the
# hooks tables, the metrics state, and the install-receipt maps do. A directly-injected dispatch never enters — it
# holds no channel and there is nothing to close.
_LIVE_INVOKES: Final[dict[ScopeKey, set[CapabilityInvoke]]] = {}


def _enrolled(invoke: CapabilityInvoke) -> CapabilityInvoke:
    # Exemption: the process-lifetime dial registry is this owner's one mutating seam. The read defaults and the
    # write replaces, so no `setdefault` runs its default expression ahead of the lookup and no partition is seated
    # by anything but a dial — a drain then walks exactly the scopes that dialed.
    _LIVE_INVOKES[invoke.scope] = _LIVE_INVOKES.get(invoke.scope, set()) | {invoke}
    return invoke


def _retired(invoke: CapabilityInvoke) -> None:
    # Exemption: the registry's release half. A partition whose last dial retires leaves the map entirely rather than
    # lingering as an empty set the drain would still walk — the same lifetime discipline the occupancy bands hold.
    held = _LIVE_INVOKES.pop(invoke.scope, set()) - {invoke}
    if held:
        _LIVE_INVOKES[invoke.scope] = held


async def drained(*, scope: ScopeKey = DEFAULT_SCOPE) -> None:
    # the capability-dial teardown the ordered drain fold names, mirroring the transport owner's own: close every
    # channel THIS composition dialed, concurrently, and `aclose` retires its own row so a caller closing early never
    # double-closes and a sibling composition's live channels are never reached.
    async with anyio.create_task_group() as tg:
        for invoke in tuple(_LIVE_INVOKES.get(scope, set())):
            tg.start_soon(invoke.aclose)


def _transcoder(shape: type[Struct]) -> RuntimeRail[WireProtoCodec[Struct, Message]]:
    # a registry miss is `config` and never `wire`: nothing was transported, so no protocol code exists to carry and a
    # `0` in that slot forges one. The shape that missed the roster rides as the row's own named coordinate instead.
    return _ROW_BY_STRUCT.try_find(shape).to_result(SERVE_REGISTRY.raised(shape.__name__)).bind(codec)


def _detail(raised: Exception) -> Option[FaultDetail]:
    # ONE trailer read, total over any raise, and the reason the retry gate and the terminal lift cannot disagree:
    # `grpc-status-details-bin` lives on a LIVE `AioRpcError` alone, so a raise carrying no trailer — or one no codec
    # decodes — answers absence here and each consumer states its own fallback instead of re-walking the metadata.
    match raised:
        case grpc.aio.AioRpcError() as wired:
            return (
                Block.of_seq(tuple(wired.trailing_metadata() or ()))
                .filter(lambda kv: kv[0] == _DETAIL_KEY)
                .try_head()
                .bind(lambda kv: codec(_FAULT_DETAIL).bind(lambda sealer: sealer.decode(kv[1])).to_option())
            )
        case _:
            return Nothing


def _stated(raised: Exception) -> Option[Recovery]:
    # the PRODUCER's rung of the retriability precedence, read at the ONE seam that can read it: `reliability/faults`
    # declares the two rungs it can see — a rostered raise's own row posture over the `FaultTag` derivation — and
    # rules that a peer-stated posture outranks both and decodes HERE. `Nothing` is the honest third answer rather
    # than a fourth case: an absent trailer, a frame minted before the slot existed, and a window this producer spelled
    # wrong all state NOTHING about re-driving, and each defers to the rung below instead of masquerading as a verdict.
    # Both readers on this page compose it, so the posture a retry acts on is the posture a caller's fault carries.
    return _detail(raised).bind(lambda sealed: FaultRecovery.stated(sealed.recovery).default_value(Nothing))


def _redrive(raised: Exception) -> bool | float:
    # TWO AXES CROSS HERE: this producer's own verdict against the class's route, and no bool anywhere. Status is the
    # only axis the `RetryClass.WIRE` row can read, so it retries the transient trio and refuses the rest; its trailer
    # carries the second axis — what the PRODUCER says about re-driving THIS fault — and reading it here is the only
    # place it can be spent instead of an attempt, because `guarded(...)` and the terminal fence below both consume
    # that raise after the budget is already gone. `terminal` refuses the re-drive even where the status sits in that
    # trio, so a permanently-decommissioned UNAVAILABLE stops at attempt one. Stated windows ride back as the float
    # `stamina` waits INSTEAD of its own curve, per the backoff-hook contract, so a server-negotiated wait is honored
    # with no sleep on this page. `transient` and every unstated answer DEFER to the row's own transience read, so
    # this overlay only ever narrows or re-times the curve and never widens what the class calls retriable.
    # `None` is unspellable on purpose: `stamina` warns on it and refuses the retry, which would silently turn every
    # raise carrying no verdict terminal.
    match _stated(raised):
        case Option(tag="some", some=Recovery(tag="terminal")):
            return False
        case Option(tag="some", some=Recovery(tag="throttled", throttled=seconds)):
            return seconds
        case _:
            return _WIRE_ROW.target(raised)


def _unsealed(terminal: grpc.aio.AioRpcError) -> BoundaryFault:
    # the trailer's typed conflict crosses WHOLE onto the minted fault through the fault family's own `domain` seat —
    # every column the egress fold stamped survives the decode rather than being read and then published as a string.
    # An absent or undecodable trailer states its own `coded` arm rather than erasing to a bare status, so
    # the two ingress shapes stay two cases a consumer matches instead of one lossy union of both.
    status = terminal.code().value[0]
    return BoundaryFault.of(
        SERVE_REMOTE,
        _detail(terminal)
        .map(lambda sealed: RemoteConflict(stated=(sealed, status)))
        .default_with(lambda: RemoteConflict(coded=(status, terminal.details() or type(terminal).__qualname__))),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

# Bound caller for this seam: ONE construction off the `RetryClass.WIRE` row, so its schedule stays the resilience
# table's and only the RE-DRIVE VERDICT is this page's. `guard(cls)` stays the bare per-CLASS caller a consumer
# holding no verdict binds — its `@cache` keys on the member alone — and a verdict read from a trailer is per-SEAM by
# construction: no policy tier can decode a `FaultDetail`, and this page owns that trailer end to end.
_WIRE_CALLER: Final[stamina.BoundAsyncRetryingCaller] = stamina.AsyncRetryingCaller(**_WIRE_ROW.schedule).on(_redrive)
```

## [04]-[ENTRY]

- Owner: `companion_app` is the `cyclopts` command axis AND the daemon composition root, co-located with `ServerHost` because the serve command composes the host it launches. `companion_app(routes, drains, charges, ledger, composition)` is parameterized over the servicer roster, the drainable owners, the supervised worker charges, the durable-evidence binding, and the custody scope, so a downstream folder's composition root — geometry `mesh/serve` the named consumer — supplies its rows, drain stages, pool charges, `(Ledger, Custody)` pair, and `ScopeKey` by data; runtime never imports a downstream sibling package, and every install owner it composes is a runtime-interior module.
- Law: STRUCTURE PROVES BEFORE CUSTODY IS CLAIMED — `aligned` and `sealed` both seat immediately after the admitted context and ahead of every install, because each install takes process ownership no refusal hands back: a set-once OTel global, a patched contrib train, a registered profiler, a claimed hook-point table. A wire gate seated after them reports a drifted descriptor or a broken packed layout onto a process that has already mounted the surfaces its own refusal cannot unmount, and neither gate reads installed state, so the earlier seat costs nothing.
- Entry: the boot fold installs the durable evidence plane LAST among the observability owners and only where the caller bound one — `Journal.install` binds onto the same railed chain, so a refused census, an unmet port, or a colliding point roster stops the boot rather than leaving every producer's `record` railing for the process's life; an unbound composition installs none and runs unjournalled. `_supervised` then starts `Journal.drained` FIRST inside the supervision group through `tg.start`, whose readiness signal blocks until the consumer holds the receive end, so no later leg can suspend into an intake nothing reads.
- Entry: this drain fold owns ORDER — the journal drain first so facts stop before the pools that produce them die and the buffered window flushes losslessly through the drain still running behind it, then the caller's `drains` rows, then one pool-drain row per charge, then the supervisor's daemon-stop escalation so no spawned child outlives the daemon, then the two outbound channel closes — the transport clients and every dialed capability invoke — and the profiles push stop. Lifecycle receipt emission settles onto the same accumulated rail after those stages; `Telemetry.shutdown` settles and stops LAST. Every stage runs after an earlier fault, and the faults accumulate into one aggregate; a first-fault abort leaving later stages undrained never lands. Boot chains ride the faults `railed` builder over heterogeneous binds a `traversed` fold cannot express. Each stage is a `(FaultRow, DrainStage)` pair rather than a labelled thunk, so a stage's fault subject derives from the owning module's own `Leg` and the free strings that named these stages retire with the anchors that replace them.
- Auto: readiness is sd-notify-shaped data — `NotifyState` closes the handshake vocabulary, `_notify` writes the service manager's `NOTIFY_SOCKET` datagram through the anyio UNIX-datagram factory, and an absent socket folds to a no-op so the same daemon runs bare or managed. `READY` fires through the serve `ready` hook after the health flips, `STOPPING` fires at the signal seam before the drain, and the `beating` leg halves `WATCHDOG_USEC` into its ping interval only when the manager arms it. Workers' actuator joins the one supervision group with the awaited `ServerHost.status` coroutine as its flip, so pool death advertises on the served health protocol without a second loop, and the serve leg's terminal send cancels the whole group — the standing signal, watchdog, and supervision rhythms end with the server, never after it. Lifecycle facts fire on the registered `LIFECYCLE_POINTS` rows — ready after the health flips, stopping at the signal seam, the drain verdict on the one-slot replay ring — and `_booted` subscribes the receipts tap per point, so daemon lifecycle telemetry is a hook projection, never a second emit path. `_supervised` mounts the diagnostic capsule before the group opens — one bundle `Route` bound to the supervisor's `verdicts` projection — so every daemon answers incident capture over the standing wire and an unresolvable bundle codec refuses at boot, never at first pull.
- Packages: `cyclopts`, `anyio`, `msgspec`, `pydantic`, `pydantic-settings`, and the faults/telemetry/logging/profiles/hooks/metrics/receipts/resilience/admission/clock/shapes/journal/lanes/workers/recipe/roots/bundle owners per the fence imports; the two settings classes enter as REFUSAL vocabulary alone, because the required `catch` names the provider that raises and the settings model is pydantic's — this leg mints no model of its own, and the gRPC raise surface it fences rides `[02]-[SERVE]`'s `_WIRE_RAISES` rather than a second import.
- Growth: a new private command is one `@app.command` method folding through the shared `_exit`; a new drainable owner is one `(FaultRow, stage)` row the ordered fold, the accumulate, and the receipt absorb, its anchor seated at the faults roster under the draining module's own `Leg`; a new lifecycle point is one `LIFECYCLE_POINTS` row; a new supervised pool is one `Charge` row; a new manager handshake is one `NotifyState` member; a new boot gate is one `yield from` beside the two structural ones, above the installs; a new bundle collector is one collectors row at the bundle owner, never a serve edit; a new custody posture behind the bound `ledger` pair is one `Custody` instance the caller constructs, zero serve edits; a sibling daemon is one `companion_app(routes, drains, charges, ledger, composition)` call with its own rows.
- Boundary: never a new public command surface — public commands are reserved to the suite Assay command surface. `NOTIFY_SOCKET`/`WATCHDOG_USEC` are the service manager's own env contract read at this one entry seam, never a settings field and never a read past admission elsewhere.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import os
import signal
from collections.abc import Awaitable, Callable, Generator
from enum import StrEnum
from functools import partial
from pathlib import Path
from typing import Annotated, Any, Final, assert_never

import anyio
import msgspec
from anyio.streams.memory import MemoryObjectSendStream
from cyclopts import App, Parameter
from cyclopts.types import NonNegativeFloat
from expression import Error, Nothing, Ok, Option, Result
from expression.collections import Block, Map
from pydantic import ValidationError
from pydantic_settings import SettingsError

from rasm.runtime import roots
from rasm.runtime.admission import RuntimeContext, RuntimeProfile, SettingsAdmission
from rasm.runtime.bundle import BUNDLE_DESCRIPTOR, BUNDLE_WIRE, SupportBundle
from rasm.runtime.clock import sealed
from rasm.runtime.faults import (
    HOOKS_RELEASE, JOURNAL_DRAIN, PROFILES_DRAIN, RECEIPTS_EMIT, ROOTS_DRAIN, SCOPES, SERVE_DIALS, SERVE_DRAIN, SERVE_HOST,
    SERVE_INPUTS, SERVE_SELECTOR, SERVE_SETTINGS, TELEMETRY_STOP, WORKERS_DAEMONS, WORKERS_POOL,
    Disposition, FaultRow, RuntimeRail, Scope, async_boundary, boundary, railed, traversed,
)
from rasm.runtime.hooks import HookPoint, Hooks, Modality, TapRow
from rasm.runtime.journal import Custody, Journal, Ledger
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.logging import LogPipeline, LogShip
from rasm.runtime.metrics import TENANT_BUDGET, Instrumentation, Metrics
from rasm.runtime.profiles import Profiles
from rasm.runtime.receipts import DEFAULT_SCOPE, OPEN, DrainReceipt, Receipt, ScopeKey, Signals
from rasm.runtime.recipe import RecipeExecution, RecipeName, RecipeSpec
from rasm.runtime.resilience import RetryMode, install
from rasm.runtime.shapes import WireMethod, WireService, aligned
from rasm.runtime.telemetry import Telemetry
from rasm.runtime.workers import Charge, Supervisor, WorkerKind, WorkerPool

# `ServerHost`/`CredentialPolicy`/`Route` are this module's [02]-[SERVE] owners — no cross-module import.

# --- [TYPES] ----------------------------------------------------------------------------

type DrainStage = Callable[[], Awaitable[object] | RuntimeRail[object]]


class NotifyState(StrEnum):
    READY = "READY=1"
    STOPPING = "STOPPING=1"
    WATCHDOG = "WATCHDOG=1"


class LifecycleFact(msgspec.Struct, frozen=True, gc=False):
    subject: str
    clean: bool = True


# --- [TABLES] ---------------------------------------------------------------------------

# daemon lifecycle points: OBSERVE facts at ready/stopping, the drain verdict on a one-slot REPLAY ring so a late
# subscriber reads the last shutdown outcome; _booted registers the rows and attaches the receipts tap per point.
_READY: Final[str] = "rasm.runtime.serve.ready"
_STOPPING: Final[str] = "rasm.runtime.serve.stopping"
_DRAINED: Final[str] = "rasm.runtime.serve.drained"
LIFECYCLE_POINTS: Final[Block[HookPoint[LifecycleFact]]] = Block.of_seq([
    HookPoint(_READY, LifecycleFact, Modality(observe=None)),
    HookPoint(_STOPPING, LifecycleFact, Modality(observe=None)),
    HookPoint(_DRAINED, LifecycleFact, Modality(replay=1)),
])

# --- [OPERATIONS] -----------------------------------------------------------------------


def _exit(outcome: RuntimeRail[object] | DrainReceipt[object]) -> int:
    # one CLI exit fold both commands share; the traceback never escapes the CLI boundary.
    match outcome:
        case Result() as rail:
            return rail.map(lambda _value: 0).default_value(1)
        case receipt:
            return 0 if receipt.faults.is_empty() else 1


@railed
def _booted(bind: str, grace: float, routes: Block[Route], ledger: Option[tuple[Ledger, Custody]]) -> Generator[Any, Any, ServerHost]:
    # admit -> gate -> install -> bind as one railed bind chain: the first Error short-circuits, the
    # composed host rides the Ok payload; an absent otel or pyroscope endpoint installs nothing — no literal.
    # the settings model is pydantic's, so its refusal classes are pydantic's — naming them is what the required
    # `catch` means, and `OSError` is the secrets-mount read the same construction performs.
    settings = yield from boundary(SERVE_SETTINGS, SettingsAdmission.mounted, catch=(SettingsError, ValidationError, OSError))
    # one ship value crosses both halves of the log egress — the chain's wire row here, the LoggerProvider registration
    # at the install — so the daemon can never render lines the provider half declines to export.
    ship = LogShip.OTLP_CONSOLE
    LogPipeline.configure(ship=ship)
    # one admitted context serves both installs, so the telemetry and profile gates read the same axis row
    # under one boot correlation; a second admit here would mint a second correlation for the same process.
    ctx = RuntimeContext.admit(RuntimeProfile.SIDECAR)
    # STRUCTURE PROVES FIRST: both wire gates seat ahead of every install, because each install claims process
    # custody — a set-once OTel global, a patched contrib train, a registered profiler, a hook-point table — and a
    # boot that mounts them and then discovers a drifted descriptor or a broken packed layout has already taken
    # ownership of surfaces its refusal cannot hand back. Neither gate reads installed state, so nothing here owes
    # them a provider.
    yield from aligned()
    yield from sealed()
    # Install receipts BIND here, never vanish inside the map: an effective `signal_profile` carries the cardinality
    # ceiling this enrollment enforces, and an absent endpoint installs no provider, so that arm enrolls no-op
    # instruments under the standing default.
    installed = Option.of_optional(settings.otel_endpoint).map(lambda endpoint: Telemetry.install(ctx, str(endpoint), ship=ship))
    Metrics.install(budget=installed.map(lambda receipt: receipt.signal_profile.cardinality_budget).default_value(TENANT_BUDGET))
    Instrumentation.install()  # contrib train patches under the same gate: no provider, no export cost
    Option.of_optional(settings.pyroscope_endpoint).map(lambda endpoint: Profiles.install(ctx, str(endpoint)))
    install(RetryMode.EMIT)
    # the roster arm claims the whole lifecycle point table in ONE gated transition — a refusal leaves custody as it
    # stood and reports every breach together, never the half-mounted table an accumulating per-point traverse leaves.
    yield from Hooks.register(LIFECYCLE_POINTS)
    # the tap rides the SAME roster grain the claim does: one subscription over the whole table, unwound whole on a
    # refusal, so a tap-policy change lands at the registry rather than at this caller's fold.
    yield from Hooks.subscribe(LIFECYCLE_POINTS, TapRow(receipts=SCOPES[Scope.SERVICE]))
    # the durable evidence plane installs LAST among the observability owners and only where a composition bound one:
    # `Journal` holds no process latch because a ledger is a value each root supplies, not an SDK singleton to adopt,
    # so an unbound daemon runs with no journal rather than against a default the branch would have to invent. The
    # bind BINDS onto this chain — a refused census, an unmet port, or a colliding point roster stops the boot here,
    # where an unbound-but-recording plane would rail every `record` a producer makes for the process's whole life.
    yield from ledger.map(lambda bound: Journal.install(*bound)).default_value(Ok(None))
    host = ServerHost(bind, CredentialPolicy.loopback(), grace)
    yield from host.register(routes)
    return host


async def _settled(at: FaultRow, stage: DrainStage) -> RuntimeRail[object]:
    # stage() itself is fenced so a synchronous raise converts instead of escaping the drain fold; a rail-returning
    # sync owner passes through, an async owner awaits under the same named fence. This is the daemon plane's ONE
    # catch-all, and it earns that seat by the faults owner's own clause: the stages are caller-supplied owners whose
    # raise surface this runtime cannot enumerate, and a shutdown that lets an unclassified raise cross the process
    # boundary loses every stage queued behind it. Every other fence on this page names its provider classes.
    match boundary(at, stage, catch=Exception):
        case Result(tag="error") as refused:
            return refused
        case Result(tag="ok", ok=Result() as rail):
            return rail
        case Result(tag="ok", ok=pending):
            return await async_boundary(at, lambda: pending, catch=Exception)
        case _ as unreachable:
            assert_never(unreachable)


async def _drained(stages: Block[tuple[FaultRow, DrainStage]]) -> RuntimeRail[Block[object]]:
    settled: Block[RuntimeRail[object]] = Block.empty()
    for at, stage in stages:  # Exemption: the ordered drain — every stage runs even after an earlier fault; the rails accumulate below.
        settled = settled.append(Block.singleton(await _settled(at, stage)))
    return traversed(settled, by=Disposition.ACCUMULATE)


async def _notify(state: NotifyState) -> None:
    # sd-notify readiness as data: an absent NOTIFY_SOCKET folds to a no-op; an abstract-namespace @ path rewrites to the
    # NUL form; a dead, refusing, or invalid manager socket is swallowed — notification is advisory, so a failed send
    # never faults the heartbeat leg and never outranks the shielded drain the STOPPING notice precedes.
    match os.environ.get("NOTIFY_SOCKET"):
        case None:
            return
        case path:
            try:
                async with await anyio.create_unix_datagram_socket() as sock:
                    await sock.sendto(state.value.encode(), path.replace("@", "\0", 1) if path.startswith("@") else path)
            except OSError:  # Exemption: the platform notify seam — an unreachable manager is ignored, never fatal.
                return


async def _launched() -> None:
    # readiness is two facts in order: the manager datagram, then the ready-point fire the receipts tap projects.
    await _notify(NotifyState.READY)
    await Hooks.fire_async(_READY, LifecycleFact(subject=SCOPES[Scope.SERVICE]))


async def _beating() -> None:
    # systemd watchdog admission is total: WATCHDOG_USEC must parse to a POSITIVE period — "0" is the manager's
    # disable spelling, not an interval, and would spin a zero-sleep ping storm — and WATCHDOG_PID, when set, must
    # name this process, so a value inherited from a parent scope never arms a child heartbeat; the armed leg pings
    # at half the period per the watchdog contract.
    match os.environ.get("WATCHDOG_USEC", ""), os.environ.get("WATCHDOG_PID", str(os.getpid())):
        case (usec, owner) if not usec.isdigit() or int(usec) == 0 or owner != str(os.getpid()):
            return
        case (usec, _):
            while True:  # Exemption: the watchdog heartbeat is the daemon's standing keep-alive, cancelled by its owning group.
                await _notify(NotifyState.WATCHDOG)
                await anyio.sleep(int(usec) / 2_000_000)


def _fleet(charges: Block[Charge]) -> Block[tuple[FaultRow, DrainStage]]:
    # one pool-drain row per pooled charge — a DAEMON charge drains through the supervisor's stop escalation, never a pool;
    # only a LIVE arm drains (an acquire here would spawn a pool solely to drain it), and the lookup carries the charge's
    # full arm key — a REMOTE endpoint or GPU device placement key included, or the dialed channel outlives the drain.
    # `pool.drain()` is the async graceful stage `_settled` awaits; its blocking joins already ride the worker band inside the pool owner.
    # Every pooled arm fences under the ONE `WORKERS_POOL` anchor: a per-charge subject minted here would spell a raise
    # coordinate the roster never declared, so which arm refused is read off the pool owner's own fault, which carries
    # its placement key, while this row states only that a pooled drain stage refused.
    return charges.filter(lambda charge: charge.kind in (WorkerKind.PROCESS, WorkerKind.GPU, WorkerKind.REMOTE)).map(
        lambda charge: (
            WORKERS_POOL,
            lambda: WorkerPool.live(charge.kind, charge.enforcement, charge.placement)
            .map(lambda pool: pool.drain())
            .default_value(Ok(None)),
        )
    )


async def _supervised(
    host: ServerHost, drains: Block[tuple[FaultRow, DrainStage]], charges: Block[Charge], journalled: bool, composition: ScopeKey
) -> RuntimeRail[None]:
    send, receive = anyio.create_memory_object_stream[RuntimeRail[object]](max_buffer_size=2)

    async def hosting(sink: MemoryObjectSendStream[RuntimeRail[object]]) -> None:
        # serve leg is fenced: a platform raise lands on the rail instead of killing the group as an unconverted
        # ExceptionGroup; the terminal group cancel below ALWAYS runs, so the standing signal, watchdog, and supervision
        # rhythms end with the server and the daemon never hangs on a loop that will not stop.
        async with sink:
            await sink.send((await async_boundary(SERVE_HOST, lambda: host.serve(ready=_launched), catch=_WIRE_RAISES)).bind(lambda rail: rail))
        group.cancel_scope.cancel()

    async def tripped(sink: MemoryObjectSendStream[RuntimeRail[object]]) -> None:
        # signal leg lives whole inside its sink so the hosting-side group cancel still closes the clone and the
        # post-group drain reaches EndOfStream; on a real signal the drain verdict sends under a shield, and the
        # finally's own group cancel makes signal-path termination unconditional — a drain refusal whose stop(grace)
        # never unblocks wait_for_termination can no longer strand the daemon on the hosting leg's cancel alone.
        async with sink:
            with anyio.open_signal_receiver(signal.SIGTERM, signal.SIGINT) as trips:  # Exemption: the platform signal seam.
                async for _ in trips:
                    break
            await _notify(NotifyState.STOPPING)
            await Hooks.fire_async(_STOPPING, LifecycleFact(subject=SCOPES[Scope.SERVICE]))
            try:
                with anyio.CancelScope(shield=True):
                    # stage one of the drain order: health flip + stop(grace) unblocks wait_for_termination.
                    await sink.send(await async_boundary(SERVE_DRAIN, host.drain, catch=_WIRE_RAISES))
            finally:
                group.cancel_scope.cancel()

    supervisor = Supervisor(charges, host.status)
    # diagnostic-capsule mount: one bundle Route bound to the supervisor's verdict projection, registered before the
    # group opens so every daemon answers incident capture over the standing wire with zero per-daemon wiring.
    request_row, response_row = BUNDLE_WIRE
    diagnostic = Route(
        service=WireService.DIAGNOSTIC, method=WireMethod.CAPTURE_BUNDLE, descriptor=BUNDLE_DESCRIPTOR,
        request=request_row, response=response_row, handler=SupportBundle.handler(supervisor.verdicts, scope=composition),
    )
    mounted = host.register(Block.singleton(diagnostic))
    if mounted.is_error():
        return mounted.map(lambda _count: None)
    async with anyio.create_task_group() as group, send:  # Exemption: the daemon's one supervision group.
        # the evidence drain starts FIRST and under `tg.start`, which blocks on its readiness signal, so no producer
        # in any later leg can suspend into an intake nothing reads yet; its tally rides the child handle and the
        # group's own close is what settles it. An unjournalled composition starts no drain — the plane it would
        # consume was never installed, and a drain over an unbound scope refuses rather than idling.
        if journalled:  # Exemption: the tg.start readiness handshake is a statement seam; the flag is the install verdict, not a knob.
            await group.start(Journal.drained)
        group.start_soon(hosting, send.clone())
        group.start_soon(tripped, send.clone())
        group.start_soon(_beating)
        supervisor.watch(group)

    settled = Block.of_seq([outcome async for outcome in receive])
    # ORDER is the correctness: facts stop before the pools that produce them die, so the journal intake closes FIRST
    # and its buffered window flushes losslessly through the drain still running behind it. Daemon children stop AFTER
    # the pools drain — a child may still serve pooled work — the transport clients and the profiles push close next.
    # Lifecycle evidence emits while telemetry is live; shutdown flushes it last.
    ordered = (
        Block.of_seq([(JOURNAL_DRAIN, Journal.closed)] if journalled else [])
        .append(drains)
        .append(_fleet(charges))
        .append(Block.singleton((WORKERS_DAEMONS, supervisor.stop)))
        .append(Block.singleton((ROOTS_DRAIN, roots.drain)))
        .append(Block.singleton((SERVE_DIALS, partial(drained, scope=composition))))
        .append(Block.singleton((PROFILES_DRAIN, lambda: Ok(Profiles.shutdown()))))
    )
    flushed = await _drained(ordered)
    pre_shutdown = traversed(settled.append(Block.singleton(flushed)), by=Disposition.ACCUMULATE)
    emitted = await _settled(
        RECEIPTS_EMIT,
        lambda: Signals.emit_async(
            Receipt.of(
                SCOPES[Scope.SERVICE],
                ("emitted", "drained", {"stages": len(settled) + len(ordered), "clean": pre_shutdown.is_ok()}),
            ),
            OPEN,
        ),
    )
    lifecycle = await Hooks.fire_async(_DRAINED, LifecycleFact(subject="drained", clean=pre_shutdown.is_ok()))
    # hook custody retires AFTER the last fire and BEFORE telemetry stops: the drained fact is the final point this
    # composition owns, and releasing whole is what keeps a re-admitting embedded runtime from colliding with its own
    # ghost roster. `release` answers the retirement verdict carrying both accounting windows out — after the swap no
    # read reaches them — so the shed and refused-sink counts leave with the value rather than dying with the tables.
    custody = await _settled(HOOKS_RELEASE, lambda: Hooks.release(scope=composition))
    telemetry = await _settled(TELEMETRY_STOP, Telemetry.shutdown)
    return traversed(
        Block.of_seq([
            pre_shutdown.map(lambda _: None),
            emitted.map(lambda _: None),
            lifecycle.map(lambda _: None),
            custody.map(lambda _: None),
            telemetry.map(lambda _: None),
        ]),
        by=Disposition.ACCUMULATE,
    ).map(lambda _: None)


async def _daemon(
    bind: str,
    grace: float,
    routes: Block[Route],
    drains: Block[tuple[FaultRow, DrainStage]],
    charges: Block[Charge],
    ledger: Option[tuple[Ledger, Custody]],
    composition: ScopeKey,
) -> RuntimeRail[None]:
    match _booted(bind, grace, routes, ledger):
        case Result(tag="error") as refused:
            return refused
        case Result(tag="ok", ok=host):
            return await _supervised(host, drains, charges, ledger.is_some(), composition)
        case _ as unreachable:
            assert_never(unreachable)


# --- [ENTRY] ----------------------------------------------------------------------------


def companion_app(
    routes: Block[Route],
    drains: Block[tuple[FaultRow, DrainStage]] = Block.empty(),
    charges: Block[Charge] = Block.empty(),
    ledger: Option[tuple[Ledger, Custody]] = Nothing,
    composition: ScopeKey = DEFAULT_SCOPE,
) -> App:
    # `ledger` is the durable-evidence binding this root supplies as a PAIR: the port implementation and the KEK
    # custody posture arrive together because a journal that lands rows it cannot shred is not a lawful plane, and
    # `Nothing` is the honest unjournalled composition rather than a default ledger this branch would have to invent.
    # S0 declares the port and imports no implementer, so a caller — `data`'s `FactJournal` the shipped one — hands
    # the value in exactly as it hands in routes, drain stages, and charges. `composition` is the custody key every
    # scope-partitioned surface this root touches carries — the capsule's capture scope and the capability drain's
    # partition — so an embedded daemon's evidence and dials never merge with the process root's.
    app = App(name=SCOPES[Scope.SERVICE], help="private companion daemon entry", result_action="return_int_as_exit_code_else_zero")

    @app.command
    async def serve(bind: str, *, grace: Annotated[NonNegativeFloat, Parameter(env_var="RASM_COMPANION_GRACE")] = 5.0) -> int:
        return _exit(await _daemon(bind, grace, routes, drains, charges, ledger, composition))

    @app.command
    async def recipe(selector: str, assignments: Path | None = None) -> int:
        # one-shot local bind of the execution/recipe#RECIPE owner: a RecipeName member or an external recipe-folder path;
        # boot pair mirrors the serve leg so the engine-gate retries ride RETRY_HOOKS. This leg registers no provider,
        # so it takes the console ship and arms no wire row rather than projecting onto a no-op logger per line.
        LogPipeline.configure(ship=LogShip.CONSOLE)
        install(RetryMode.EMIT)
        loaded = (
            Ok(Map.empty())
            if assignments is None
            else boundary(
                SERVE_INPUTS,
                lambda: Map.of_seq(msgspec.json.decode(assignments.read_bytes(), type=dict[str, object]).items()),
                catch=(*_CODEC_RAISES, OSError),
            )
        )
        # a selector outside the member roster is an EXTERNAL recipe-folder path, not a defect, so the one class a
        # `StrEnum` construction raises is named and its refusal folds to the path arm rather than to the CLI exit.
        spec = loaded.map(
            lambda inputs: RecipeSpec(
                recipe=boundary(SERVE_SELECTOR, lambda: RecipeName(selector), catch=ValueError).default_value(selector), inputs=inputs
            )
        )
        match spec:
            case Result(tag="error") as refused:
                return _exit(refused)
            case Result(tag="ok", ok=one):
                # `LanePolicy.of` is the SCOPED constructor, so the lane is entered rather than constructed: binding
                # its return value directly hands `RecipeExecution` an async context manager where a `LanePolicy`
                # belongs, and the lane's whole lifetime contract — the pulse actor started, the occupancy probe
                # registered, the conduit retired — never runs. One-shot execution owns exactly one lane, so the
                # window brackets the run and closes with it.
                async with LanePolicy.of(RuntimeContext.admit(RuntimeProfile.TOOL)) as lane:
                    return _exit(await RecipeExecution(lane=lane).execute(one))
            case _ as unreachable:
                assert_never(unreachable)

    return app
```

## [05]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
