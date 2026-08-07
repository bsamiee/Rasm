# [PY_RUNTIME_SERVE]

Companion server-host and daemon composition root: `ServerHost` owns the inbound `grpc.aio` lifecycle and the one `_invoke` servicer body every registered method of every arity folds through, `CapabilityInvoke` the descriptor-driven outbound invoke over the C#-generated capability SDK, and `Entrypoint` the daemon boot/serve/drain choreography. It hosts the geometry companion daemon over the corpus gRPC contract on the UDS leg and re-mints nothing it composes.

Wire vocabulary is `transport/shapes#VOCABULARY`'s, the transcode machinery `transport/wire#PROTO_TRANSCODE`'s, causal time `clock/clock#CLOCK`'s, and the admitted context `execution/admission#CONTEXT`'s. Seam ledgers file the `CredentialPolicy` axis decode and the W3C inbound extraction on this page — the interceptor at this ingress is the one trace-context authority.

## [01]-[INDEX]

- [02]-[SERVE]: the inbound server-host lifecycle, the `Route` roster, the arity-spanning servicer body under one duration weave, the two-directional `CredentialPolicy`, and the `FaultDetail` trailer egress.
- [03]-[CAPABILITY_INVOKE]: the descriptor-driven outbound invoke and the `fault_detail` trailer ingress.
- [04]-[ENTRY]: the daemon composition root — railed boot, supervised serve, the diagnostic-capsule mount, the ordered receipted drain, and the one-shot recipe command.

## [02]-[SERVE]

- Owner: `ServerHost` is the boundary capsule over one `grpc.aio` server with the registered health servicer; a servicer method is a `Route` row — service, method, descriptor id, two registry row names, arity member, railed handler — never a hand-written admit/transcode/abort prologue. It composes the wire codec, the `clock/clock#CLOCK` `CausalFrame.decode` sole carrier fence, the admitted `RuntimeContext`, and the `FaultDetail` shape, re-minting none.
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
- Growth: a new servicer method is one `Route` row, a streaming method the same row with its `arity` member; a new stream shape is one `RouteArity` member with one `_member` arm and one body over the standing invoke; a new wire message is one shapes registry row; a new fault-to-status mapping is one `_FAULT_STATUS` row; a new outbound credential posture is one `CredentialPolicy` case with one `channel_credentials` arm; a new health surface is automatic with the lifecycle; a new span dimension is one `enrich` key; a new compression algorithm is one `grpc.Compression` member at construction.
- Boundary: the wire contract is the corpus-homed `.proto` — the runtime mints no transport, no channel, and no second wire vocabulary; host lifecycle and product telemetry export stay with the composing application.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import AsyncIterator, Awaitable, Callable
from contextlib import asynccontextmanager
from datetime import UTC, datetime, timedelta
from enum import StrEnum
from typing import Final, Literal, Self, assert_never

import grpc
from expression import Error, Ok, Option, Result, case, tag, tagged_union
from expression.collections import Block, Map
from google.protobuf.message import Message
from grpc_health.v1 import health, health_pb2, health_pb2_grpc
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.instrumentation.grpc import aio_server_interceptor, filters
import structlog

from rasm.runtime.admission import Deadline, RuntimeContext, RuntimeProfile
from rasm.runtime.clock import CausalFrame
from rasm.runtime.faults import SCOPES, BoundaryFault, Disposition, FaultTag, RuntimeRail, Scope, traversed
from rasm.runtime.metrics import Metrics
from rasm.runtime.shapes import FaultDetail
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
# C# Instant.ToUnixTimeTicks is the 100 ns unit; the trailer echo truncates to Timestamp
# microseconds — the carrier headers stay the authoritative full-fidelity stamp.
_TICKS_PER_SECOND: Final[int] = 10_000_000

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
                return Error(BoundaryFault(config=(outbound, "outbound peer client credential; this branch serves insecure_loopback over UDS only")))
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
                return Error(BoundaryFault(config=("insecure_loopback", "inbound serve credential; an outbound dial names one of the four peer rows")))
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

_FAULT_STATUS: Final[Map[FaultTag, grpc.StatusCode]] = Map.of_seq([
    ("config", grpc.StatusCode.FAILED_PRECONDITION),
    ("resource", grpc.StatusCode.UNAVAILABLE),
    ("deadline", grpc.StatusCode.DEADLINE_EXCEEDED),
    ("api", grpc.StatusCode.INTERNAL),
    ("import_", grpc.StatusCode.UNIMPLEMENTED),
    ("wire", grpc.StatusCode.INVALID_ARGUMENT),
    ("boundary", grpc.StatusCode.INVALID_ARGUMENT),
    ("aggregate", grpc.StatusCode.INTERNAL),
])

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
        carrier = dict(servicer_context.invocation_metadata())
        budget = Option.of_optional(servicer_context.time_remaining()).map(lambda remaining: Deadline(timedelta(seconds=remaining)))
        return CausalFrame.decode(carrier).map(
            lambda causal: RuntimeContext.admit(
                RuntimeProfile.SIDECAR, deadline=budget.to_optional(), causal=causal, carrier=carrier
            )
        )

    @staticmethod
    def enrich(context: RuntimeContext, descriptor: str, fault_case: str) -> None:
        trace.get_current_span().set_attributes(context.attribute() | {"rasm.descriptor": descriptor, "rasm.fault_case": fault_case})

    @staticmethod
    async def settle[T](servicer_context: grpc.aio.ServicerContext, context: RuntimeContext, descriptor: str, wired: RuntimeRail[T]) -> T:
        # generic over the terminated payload, so one fold ends a unary reply's `bytes`, a mid-stream frame batch's
        # `Block[bytes]`, and an admission refusal alike — the Error arm aborts and returns nothing, so the payload type
        # only ever names what the Ok arm hands back and no arm launders a rail through a discarding `map`.
        match wired:
            case Result(tag="ok", ok=payload):
                ServerHost.enrich(context, descriptor, "ok")
                return payload
            case Result(tag="error", error=fault):
                ServerHost.enrich(context, descriptor, fault.tag)
                status = _FAULT_STATUS.try_find(fault.tag).default_value(grpc.StatusCode.INTERNAL)
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
            return Error(BoundaryFault(config=("serve.roster", "empty-roster")))
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
    # neither half — each refuses by name here rather than at the socket.
    anchors, tokens = rows.filter(lambda row: row.tag in ("tls", "mtls")), rows.filter(lambda row: row.tag == "bearer")
    return (
        Error(BoundaryFault(config=("composed", "a bundle admits tls, mtls, and bearer rows alone")))
        if len(anchors) + len(tokens) != len(rows)
        else Error(BoundaryFault(config=("composed", f"a bundle anchors on exactly one channel row, offered {len(anchors)}")))
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
            ServerHost.enrich(context, row.descriptor, "ok")

    return method


def _sealed(fault: BoundaryFault, context: RuntimeContext, status: grpc.StatusCode) -> FaultDetail:
    # one total egress fold off facts() + the inbound frame: the C# WireFault.Decode(FaultDetail)
    # consumer reads the typed conflict; the hlc echo rides Timestamp precision.
    facts = fault.facts()
    return FaultDetail(
        package=SCOPES[Scope.SERVICE],
        code=status.value[0],
        case_=fault.tag,
        message=str(facts.get("detail") or facts.get("cause") or facts.get("members") or facts.get("subject") or ""),
        evidence={key: str(value) for key, value in facts.items()},
        correlation=context.correlation.trace_id.hex(),
        hlc_physical=context.causal.map(lambda frame: datetime.fromtimestamp(frame.hlc.physical_ticks / _TICKS_PER_SECOND, tz=UTC)).to_optional(),
        hlc_logical=context.causal.map(lambda frame: frame.hlc.logical).default_value(0),
        tenant=context.causal.map(lambda frame: str(frame.tenant)).default_value(""),
    )
```

## [03]-[CAPABILITY_INVOKE]

- Owner: `CapabilityInvoke` decodes the C# `csharp:Rasm.AppHost/Agent/capability#SDK_CODEGEN` Python target into one dispatch — the request's own `Struct` type and the caller's `into` type are the codec discriminants off the `_ROW_BY_STRUCT` table, so one `run` genuinely spans the whole catalog and no injected per-shape codec pair narrows it. Outbound legs retry under the bare cached `guard(RetryClass.WIRE)` caller with a two-fence ingress — a `guarded(...)` wrap is the ruled-out composition because its terminal lift consumes the exception before the trailer read — so no bare gRPC exception escapes and no trailer erases to a bare `boundary` tag.
- Cases: the per-descriptor `input_schema` is the C# `SuiteContracts.Schema` JSON Schema carried as a deferred `msgspec.Raw` slot, so the routing decode never pays the schema-document parse; the argument payload is the already-typed canonical `Struct` the resolved codec transcodes, never a hand-mirrored mapping re-validated against a schema document. `effect`/`idempotency`/cost-unit keys decode as the C# smart-enum string keys.
- Entry: the per-call descriptor dimension rides the interceptor-set `rpc.service`/`rpc.method` attributes natively — the invoke path IS `/rasm.capability/{descriptor_id}` — while the channel-stable hooks enrich tenant and fault case on the CLIENT span; an ambient per-call `set_attribute` lands on whatever span was current BEFORE the CLIENT span opened.
- Packages: `msgspec`, `grpcio`, `opentelemetry-instrumentation-grpc`, and the shapes/wire/resilience/faults rails per the fence imports; `guard` is exported for exactly this composed per-seam aspect.
- Growth: a new capability is one descriptor row the `Rasm.AppHost` capability broker folds — this branch reads it through the existing `discover`/`run` pair; a new wire shape reaches the invoke through one shapes registry row with zero edits here; a new span dimension is one hook key.
- Boundary: `connect` mints the channel, so this owner owes its teardown — every dial enrols on the live set the `[04]-[ENTRY]` drain fold names as its own stage, and `aclose` retires its row so an early caller close never double-closes. A channel whose only teardown is the caller's memory is the leak the transport owner's pooled clients already refuse, and the two teardowns read as one row pair rather than one rescued and one forgotten. The descriptor is the suite's only op-metadata owner and the capability broker its sole mint, named by the brokered-capability domain it holds; this branch re-authors no capability shape. Cross-language shape identity is the broker's `SuiteContracts.Schema` JSON Schema all three SDKs bind, evolution riding the contract's additive-only rule. Channel liveness rides the `WIRE` row's `UNAVAILABLE` transient, so no client `HealthStub` pre-probe rides `connect`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable
from typing import Final, Literal, Self, assert_never

import anyio
import grpc
import msgspec
from expression import Error, Ok, Result
from expression.collections import Block, Map
from google.protobuf.message import Message
from msgspec import Raw, Struct
from opentelemetry import trace
from opentelemetry.instrumentation.grpc import aio_client_interceptors, filters

from rasm.runtime.clock import Tenant
from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary, boundary
from rasm.runtime.resilience import RetryClass, guard
from rasm.runtime.shapes import PROTO_VOCABULARY
from rasm.runtime.wire import WireProtoCodec, codec

# `_DETAIL_KEY`/`_FAULT_DETAIL` are this module's [02]-[SERVE] constants — one trailer spelling for egress pack and
# ingress lift — and `CredentialPolicy` its owner of the two-directional credential axis this dial projects through.

# --- [TYPES] ----------------------------------------------------------------------------

type CostUnitKey = Literal["cpu-millis", "wall-millis", "bytes-egress", "model-tokens", "calls"]
type CostVector = dict[CostUnitKey, int]
type CommandTxnKind = Literal["committed", "rolled_back", "compensated", "refused"]
type WireDispatch = Callable[[str, bytes], Awaitable[RuntimeRail[bytes]]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_CAPABILITY_SERVICE: Final[str] = "rasm.capability"

# --- [MODELS] ---------------------------------------------------------------------------


class CommandTxn(Struct, frozen=True, rename="camel"):
    kind: CommandTxnKind
    detail: str = ""


class DiscoveryResult(Struct, frozen=True, rename="camel"):
    descriptor: str
    surface: str
    effect: Literal["pure", "read", "write", "external", "irreversible"]
    idempotency: Literal["idempotent", "keyed", "single-shot", "non-idempotent"]
    estimated: CostVector
    scope_hash: str
    input_schema: Raw = Raw(b"{}")


class CommandReceipt(Struct, frozen=True, rename="camel"):
    descriptor: str
    txn: CommandTxn
    charged: CostVector
    elapsed: str
    correlation: str


# --- [TABLES] ---------------------------------------------------------------------------

# derived from the one shapes seed table: the struct TYPE is the discriminant `run` resolves a
# codec by, so one invoke spans the whole catalog with no injected per-shape codec pair.
_ROW_BY_STRUCT: Final[Map[type[Struct], str]] = Map.of_seq((struct, name) for name, struct, _ in PROTO_VOCABULARY)

# --- [SERVICES] -------------------------------------------------------------------------


class CapabilityInvoke:
    # distinctly named so the instance-level lookup `Map` `run` reads never shadows the class decoder.
    _DISCOVERY: msgspec.json.Decoder[list[DiscoveryResult]] = msgspec.json.Decoder(list[DiscoveryResult])

    def __init__(self, catalog: Map[str, DiscoveryResult], dispatch: WireDispatch, channel: grpc.aio.Channel | None = None) -> None:
        self._catalog, self._dispatch, self._channel = catalog, dispatch, channel

    @classmethod
    def discover(cls, payload: bytes) -> RuntimeRail[Map[str, DiscoveryResult]]:
        # descriptor-keyed fold lives at the one decode site, never a list-to-Map re-key the composition root hand-spells.
        return boundary("wire", lambda: cls._DISCOVERY.decode(payload)).map(lambda rows: Map.of_seq((row.descriptor, row) for row in rows))

    @staticmethod
    def interceptors(tenant: Tenant) -> list[grpc.aio.ClientInterceptor]:
        # channel-stable enrichment only; the per-call descriptor rides the interceptor-set
        # `rpc.service`/`rpc.method` off the `/rasm.capability/{descriptor_id}` path natively.
        def request_hook(span: trace.Span, _request: object) -> None:
            span.set_attribute("rasm.tenant", str(tenant))

        def response_hook(span: trace.Span, details: str) -> None:
            span.set_attribute("rasm.fault_case", details or "ok")

        return aio_client_interceptors(filter_=filters.negate(filters.health_check()), request_hook=request_hook, response_hook=response_hook)

    @classmethod
    def connect(cls, target: str, catalog: Map[str, DiscoveryResult], tenant: Tenant, credential: CredentialPolicy) -> RuntimeRail[Self]:
        # the dial is CREDENTIALED by construction: the caller's policy projects to a channel credential or refuses on
        # the rail before any socket exists, so `insecure_channel` appears on no path here and an unauthenticated
        # outbound leg is unspellable rather than the silent default this seam used to carry. The refusal names which
        # case rejected it — `insecure_loopback` is the inbound seat, and a malformed bundle names its own arity.
        def dialed(credentials: grpc.ChannelCredentials) -> Self:
            channel = grpc.aio.secure_channel(target, credentials, interceptors=cls.interceptors(tenant))

            async def dispatch(descriptor_id: str, request: bytes) -> RuntimeRail[bytes]:
                method = channel.unary_unary(f"/{_CAPABILITY_SERVICE}/{descriptor_id}")

                async def called() -> RuntimeRail[bytes]:
                    # Exemption: the trailer fence — grpc-status-details-bin lives only on the live AioRpcError, so this one
                    # platform-forced except reclassifies the terminal raise AFTER the WIRE-row retry exhausts.
                    try:
                        return Ok(await guard(RetryClass.WIRE)(method, request))
                    except grpc.aio.AioRpcError as terminal:
                        return Error(_unsealed(terminal))

                return (await async_boundary("wire", called)).bind(lambda rail: rail)

            invoke = cls(catalog, dispatch, channel)
            _LIVE_INVOKES.add(invoke)
            return invoke

        return credential.channel_credentials().map(dialed)

    async def run[S: Struct, R: Struct](self, descriptor_id: str, request: S, into: type[R]) -> RuntimeRail[R]:
        staged = (
            self._catalog.try_find(descriptor_id)
            .to_result(BoundaryFault(wire=(descriptor_id, 0)))
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
        _LIVE_INVOKES.discard(self)
        if self._channel is not None:
            await self._channel.close()


# --- [OPERATIONS] -----------------------------------------------------------------------

# every dialed invoke, so the daemon drain reaches a channel the caller dialed rather than trusting each composition
# to remember its own: `connect` mints the channel, so this owner owes its teardown exactly as the transport owner
# owes its pooled clients, and a leaked `grpc.aio.Channel` keeps a live connection past the process's own shutdown.
# A directly-injected dispatch never enters — it holds no channel and there is nothing to close.
_LIVE_INVOKES: Final[set[CapabilityInvoke]] = set()


async def drained() -> None:
    # the capability-dial teardown the ordered drain fold names, mirroring the transport owner's own: close every
    # dialed channel concurrently, and `aclose` discards its own row so a caller closing early never double-closes.
    async with anyio.create_task_group() as tg:
        for invoke in tuple(_LIVE_INVOKES):
            tg.start_soon(invoke.aclose)


def _transcoder(shape: type[Struct]) -> RuntimeRail[WireProtoCodec[Struct, Message]]:
    return _ROW_BY_STRUCT.try_find(shape).to_result(BoundaryFault(wire=(shape.__name__, 0))).bind(codec)


def _unsealed(terminal: grpc.aio.AioRpcError) -> BoundaryFault:
    # trailer's fault_detail row decodes to the typed producer conflict; an absent or undecodable
    # trailer falls back to the status-coded lift, never a bare erasure.
    status = terminal.code().value[0]
    detail = (
        Block.of_seq(tuple(terminal.trailing_metadata() or ()))
        .filter(lambda kv: kv[0] == _DETAIL_KEY)
        .try_head()
        .bind(lambda kv: codec(_FAULT_DETAIL).bind(lambda sealer: sealer.decode(kv[1])).to_option())
    )
    return detail.map(lambda sealed: BoundaryFault(wire=(f"{sealed.package}.{sealed.case_}:{sealed.message}", sealed.code or status))).default_with(
        lambda: BoundaryFault(wire=(terminal.details() or type(terminal).__qualname__, status))
    )
```

## [04]-[ENTRY]

- Owner: `companion_app` is the `cyclopts` command axis AND the daemon composition root, co-located with `ServerHost` because the serve command composes the host it launches. `companion_app(routes, drains, charges, ledger)` is parameterized over the servicer roster, the drainable owners, the supervised worker charges, and the durable-evidence binding, so a downstream folder's composition root — geometry `mesh/serve` the named consumer — supplies its rows, drain stages, pool charges, and `(Ledger, Custody)` pair by data; runtime never imports a downstream sibling package, and every install owner it composes is a runtime-interior module.
- Entry: the boot fold installs the durable evidence plane LAST among the observability owners and only where the caller bound one — `Journal.install` binds onto the same railed chain, so a refused census, an unmet port, or a colliding point roster stops the boot rather than leaving every producer's `record` railing for the process's life; an unbound composition installs none and runs unjournalled. `_supervised` then starts `Journal.drained` FIRST inside the supervision group through `tg.start`, whose readiness signal blocks until the consumer holds the receive end, so no later leg can suspend into an intake nothing reads.
- Entry: this drain fold owns ORDER — `observability.journal` first so facts stop before the pools that produce them die and the buffered window flushes losslessly through the drain still running behind it, then the caller's `drains` rows, then one pool-drain row per charge, then the supervisor's daemon-stop escalation so no spawned child outlives the daemon, then the two outbound channel closes — the transport clients and every dialed capability invoke — and the profiles push stop. Lifecycle receipt emission settles onto the same accumulated rail after those stages; `Telemetry.shutdown` settles and stops LAST. Every stage runs after an earlier fault, and the faults accumulate into one aggregate; a first-fault abort leaving later stages undrained never lands. Boot chains ride the faults `railed` builder over heterogeneous binds a `traversed` fold cannot express.
- Auto: readiness is sd-notify-shaped data — `NotifyState` closes the handshake vocabulary, `_notify` writes the service manager's `NOTIFY_SOCKET` datagram through the anyio UNIX-datagram factory, and an absent socket folds to a no-op so the same daemon runs bare or managed. `READY` fires through the serve `ready` hook after the health flips, `STOPPING` fires at the signal seam before the drain, and the `beating` leg halves `WATCHDOG_USEC` into its ping interval only when the manager arms it. Workers' actuator joins the one supervision group with the awaited `ServerHost.status` coroutine as its flip, so pool death advertises on the served health protocol without a second loop, and the serve leg's terminal send cancels the whole group — the standing signal, watchdog, and supervision rhythms end with the server, never after it. Lifecycle facts fire on the registered `LIFECYCLE_POINTS` rows — ready after the health flips, stopping at the signal seam, the drain verdict on the one-slot replay ring — and `_booted` subscribes the receipts tap per point, so daemon lifecycle telemetry is a hook projection, never a second emit path. `_supervised` mounts the diagnostic capsule before the group opens — one bundle `Route` bound to the supervisor's `verdicts` projection — so every daemon answers incident capture over the standing wire and an unresolvable bundle codec refuses at boot, never at first pull.
- Packages: `cyclopts`, `anyio`, `msgspec`, and the faults/telemetry/logging/profiles/hooks/metrics/receipts/resilience/admission/journal/lanes/workers/recipe/roots/bundle owners per the fence imports.
- Growth: a new private command is one `@app.command` method folding through the shared `_exit`; a new drainable owner is one `(subject, stage)` row the ordered fold, the accumulate, and the receipt absorb; a new lifecycle point is one `LIFECYCLE_POINTS` row; a new supervised pool is one `Charge` row; a new manager handshake is one `NotifyState` member; a new bundle collector is one collectors row at the bundle owner, never a serve edit; a new custody posture behind the bound `ledger` pair is one `Custody` instance the caller constructs, zero serve edits; a sibling daemon is one `companion_app(routes, drains, charges, ledger)` call with its own rows.
- Boundary: never a new public command surface — public commands are reserved to the suite Assay command surface. `NOTIFY_SOCKET`/`WATCHDOG_USEC` are the service manager's own env contract read at this one entry seam, never a settings field and never a read past admission elsewhere.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import os
import signal
from collections.abc import Awaitable, Callable, Generator
from enum import StrEnum
from pathlib import Path
from typing import Annotated, Any, Final, assert_never

import anyio
import msgspec
from anyio.streams.memory import MemoryObjectSendStream
from cyclopts import App, Parameter
from cyclopts.types import NonNegativeFloat
from expression import Error, Nothing, Ok, Option, Result
from expression.collections import Block, Map

from rasm.runtime import roots
from rasm.runtime.admission import RuntimeContext, RuntimeProfile, SettingsAdmission
from rasm.runtime.bundle import BUNDLE_DESCRIPTOR, BUNDLE_METHOD, BUNDLE_SERVICE, BUNDLE_WIRE, SupportBundle
from rasm.runtime.faults import SCOPES, Disposition, RuntimeRail, Scope, async_boundary, boundary, railed, traversed
from rasm.runtime.hooks import HookPoint, Hooks, Modality
from rasm.runtime.journal import Custody, Journal, Ledger
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.logging import LogPipeline, LogShip
from rasm.runtime.metrics import TENANT_BUDGET, Instrumentation, Metrics
from rasm.runtime.profiles import Profiles
from rasm.runtime.receipts import OPEN, DrainReceipt, Receipt, Signals
from rasm.runtime.recipe import RecipeExecution, RecipeName, RecipeSpec
from rasm.runtime.resilience import RetryMode, install
from rasm.runtime.shapes import aligned
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
    HookPoint(_READY, LifecycleFact, Modality.OBSERVE),
    HookPoint(_STOPPING, LifecycleFact, Modality.OBSERVE),
    HookPoint(_DRAINED, LifecycleFact, Modality.REPLAY, buffer=1),
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
    # install -> admit -> gate -> bind as one railed bind chain: the first Error short-circuits, the
    # composed host rides the Ok payload; an absent otel or pyroscope endpoint installs nothing — no literal.
    settings = yield from boundary("config", SettingsAdmission.mounted)
    # one ship value crosses both halves of the log egress — the chain's wire row here, the LoggerProvider registration
    # at the install — so the daemon can never render lines the provider half declines to export.
    ship = LogShip.OTLP_CONSOLE
    LogPipeline.configure(ship=ship)
    # one admitted context serves both installs, so the telemetry and profile gates read the same axis row
    # under one boot correlation; a second admit here would mint a second correlation for the same process.
    ctx = RuntimeContext.admit(RuntimeProfile.SIDECAR)
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
    yield from Hooks.subscribe(LIFECYCLE_POINTS, Hooks.tap_receipts(SCOPES[Scope.SERVICE]))
    yield from aligned()
    # the durable evidence plane installs LAST among the observability owners and only where a composition bound one:
    # `Journal` holds no process latch because a ledger is a value each root supplies, not an SDK singleton to adopt,
    # so an unbound daemon runs with no journal rather than against a default the branch would have to invent. The
    # bind BINDS onto this chain — a refused census, an unmet port, or a colliding point roster stops the boot here,
    # where an unbound-but-recording plane would rail every `record` a producer makes for the process's whole life.
    yield from ledger.map(lambda bound: Journal.install(*bound)).default_value(Ok(None))
    host = ServerHost(bind, CredentialPolicy.loopback(), grace)
    yield from host.register(routes)
    return host


async def _settled(subject: str, stage: DrainStage) -> RuntimeRail[object]:
    # stage() itself is fenced so a synchronous raise converts instead of escaping the drain fold; a rail-returning
    # sync owner passes through, an async owner awaits under the same named fence.
    match boundary(subject, stage):
        case Result(tag="error") as refused:
            return refused
        case Result(tag="ok", ok=Result() as rail):
            return rail
        case Result(tag="ok", ok=pending):
            return await async_boundary(subject, lambda: pending)
        case _ as unreachable:
            assert_never(unreachable)


async def _drained(stages: Block[tuple[str, DrainStage]]) -> RuntimeRail[Block[object]]:
    settled: Block[RuntimeRail[object]] = Block.empty()
    for subject, stage in stages:  # Exemption: the ordered drain — every stage runs even after an earlier fault; the rails accumulate below.
        settled = settled.append(Block.singleton(await _settled(subject, stage)))
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


def _fleet(charges: Block[Charge]) -> Block[tuple[str, DrainStage]]:
    # one pool-drain row per pooled charge — a DAEMON charge drains through the supervisor's stop escalation, never a pool;
    # only a LIVE arm drains (an acquire here would spawn a pool solely to drain it), and the lookup carries the charge's
    # full arm key — a REMOTE endpoint or GPU device placement key included, or the dialed channel outlives the drain.
    # `pool.drain()` is the async graceful stage `_settled` awaits; its blocking joins already ride the worker band inside the pool owner.
    return charges.filter(lambda charge: charge.kind in (WorkerKind.PROCESS, WorkerKind.GPU, WorkerKind.REMOTE)).map(
        lambda charge: (
            f"workers.{charge.policy.subject}",
            lambda: WorkerPool.live(charge.kind, charge.enforcement, charge.placement)
            .map(lambda pool: pool.drain())
            .default_value(Ok(None)),
        )
    )


async def _supervised(host: ServerHost, drains: Block[tuple[str, DrainStage]], charges: Block[Charge], journalled: bool) -> RuntimeRail[None]:
    send, receive = anyio.create_memory_object_stream[RuntimeRail[object]](max_buffer_size=2)

    async def hosting(sink: MemoryObjectSendStream[RuntimeRail[object]]) -> None:
        # serve leg is fenced: a platform raise lands on the rail instead of killing the group as an unconverted
        # ExceptionGroup; the terminal group cancel below ALWAYS runs, so the standing signal, watchdog, and supervision
        # rhythms end with the server and the daemon never hangs on a loop that will not stop.
        async with sink:
            await sink.send((await async_boundary("serve.host", lambda: host.serve(ready=_launched))).bind(lambda rail: rail))
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
                    await sink.send(await async_boundary("serve.drain", host.drain))
            finally:
                group.cancel_scope.cancel()

    supervisor = Supervisor(charges, host.status)
    # diagnostic-capsule mount: one bundle Route bound to the supervisor's verdict projection, registered before the
    # group opens so every daemon answers incident capture over the standing wire with zero per-daemon wiring.
    request_row, response_row = BUNDLE_WIRE
    diagnostic = Route(
        service=BUNDLE_SERVICE, method=BUNDLE_METHOD, descriptor=BUNDLE_DESCRIPTOR,
        request=request_row, response=response_row, handler=SupportBundle.handler(supervisor.verdicts),
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
        Block.of_seq([("observability.journal", Journal.closed)] if journalled else [])
        .append(drains)
        .append(_fleet(charges))
        .append(Block.singleton(("workers.daemons", supervisor.stop)))
        .append(Block.singleton(("transport.roots", roots.drain)))
        .append(Block.singleton(("transport.capability", drained)))
        .append(Block.singleton(("observability.profiles", lambda: Ok(Profiles.shutdown()))))
    )
    flushed = await _drained(ordered)
    pre_shutdown = traversed(settled.append(Block.singleton(flushed)), by=Disposition.ACCUMULATE)
    emitted = await _settled(
        "observability.receipts",
        lambda: Signals.emit_async(
            Receipt.of(
                SCOPES[Scope.SERVICE],
                ("emitted", "drained", {"stages": len(settled) + len(ordered), "clean": pre_shutdown.is_ok()}),
            ),
            OPEN,
        ),
    )
    lifecycle = await Hooks.fire_async(_DRAINED, LifecycleFact(subject="drained", clean=pre_shutdown.is_ok()))
    telemetry = await _settled("observability.telemetry", Telemetry.shutdown)
    return traversed(
        Block.of_seq([
            pre_shutdown.map(lambda _: None),
            emitted.map(lambda _: None),
            lifecycle.map(lambda _: None),
            telemetry.map(lambda _: None),
        ]),
        by=Disposition.ACCUMULATE,
    ).map(lambda _: None)


async def _daemon(
    bind: str,
    grace: float,
    routes: Block[Route],
    drains: Block[tuple[str, DrainStage]],
    charges: Block[Charge],
    ledger: Option[tuple[Ledger, Custody]],
) -> RuntimeRail[None]:
    match _booted(bind, grace, routes, ledger):
        case Result(tag="error") as refused:
            return refused
        case Result(tag="ok", ok=host):
            return await _supervised(host, drains, charges, ledger.is_some())
        case _ as unreachable:
            assert_never(unreachable)


# --- [ENTRY] ----------------------------------------------------------------------------


def companion_app(
    routes: Block[Route],
    drains: Block[tuple[str, DrainStage]] = Block.empty(),
    charges: Block[Charge] = Block.empty(),
    ledger: Option[tuple[Ledger, Custody]] = Nothing,
) -> App:
    # `ledger` is the durable-evidence binding this root supplies as a PAIR: the port implementation and the KEK
    # custody posture arrive together because a journal that lands rows it cannot shred is not a lawful plane, and
    # `Nothing` is the honest unjournalled composition rather than a default ledger this branch would have to invent.
    # S0 declares the port and imports no implementer, so a caller — `data`'s `FactJournal` the shipped one — hands
    # the value in exactly as it hands in routes, drain stages, and charges.
    app = App(name=SCOPES[Scope.SERVICE], help="private companion daemon entry", result_action="return_int_as_exit_code_else_zero")

    @app.command
    async def serve(bind: str, *, grace: Annotated[NonNegativeFloat, Parameter(env_var="RASM_COMPANION_GRACE")] = 5.0) -> int:
        return _exit(await _daemon(bind, grace, routes, drains, charges, ledger))

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
            else boundary("config", lambda: Map.of_seq(msgspec.json.decode(assignments.read_bytes(), type=dict[str, object]).items()))
        )
        spec = loaded.map(
            lambda inputs: RecipeSpec(recipe=boundary("config", lambda: RecipeName(selector)).default_value(selector), inputs=inputs)
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
