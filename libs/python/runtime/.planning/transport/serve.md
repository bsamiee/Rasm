# [PY_RUNTIME_SERVE]

The companion server-host and daemon composition root: `ServerHost` owns the inbound `grpc.aio` lifecycle and the one `dispatch` servicer-body aspect every registered method folds through, `CapabilityInvoke` the descriptor-driven outbound invoke over the C#-generated capability SDK, and `Entrypoint` the daemon boot/serve/drain choreography. It hosts the geometry companion daemon over the existing C# gRPC contract on the UDS leg and re-mints nothing it composes.

The wire vocabulary is `transport/shapes#VOCABULARY`'s, the transcode machinery `transport/wire#PROTO_TRANSCODE`'s, causal time `clock#CLOCK`'s, and the admitted context `execution/admission#CONTEXT`'s. The seam ledger files the `CredentialPolicy` axis decode and the W3C inbound extraction on this page — the interceptor at this ingress is the one trace-context authority.

## [01]-[INDEX]

- [01]-[SERVE]: the inbound server-host lifecycle, the `Route` roster, the dispatch aspect, and the `FaultDetail` trailer egress.
- [02]-[CAPABILITY_INVOKE]: the descriptor-driven outbound invoke and the `fault_detail` trailer ingress.
- [03]-[ENTRY]: the daemon composition root — railed boot, supervised serve, the ordered receipted drain, and the one-shot recipe command.

## [02]-[SERVE]

- Owner: `ServerHost` is the boundary capsule over one `grpc.aio` server plus the registered health servicer; a servicer method is a `Route` row — service, method, descriptor id, two registry row names, railed handler — never a hand-written admit/transcode/abort prologue. It composes the wire codec, the `clock#CLOCK` `CausalFrame.decode` sole carrier fence, the admitted `RuntimeContext`, and the `FaultDetail` shape, re-minting none.
- Cases: `CredentialPolicy` decodes the C#-minted five-row axis under one spelling on both sides of the wire, and the UDS serve leg admits exactly one constructible case — peer identity is the kernel-reported `(pid, uid)` the C# `Rasm.AppHost/Wire/companion#PEER_ADMISSION` reads at accept through `SO_PEERCRED`/`LOCAL_PEERCRED`, never a wire-carried PEM; the four outbound rows are the client legs the C# host dials, each a typed construction failure serve-side.

Entry: the method roster:

| [INDEX] | [METHOD]                                                                  | [CONTRACT]                                              |
| :-----: | :------------------------------------------------------------------------ | :------------------------------------------------------ |
|  [01]   | `register(routes) -> RuntimeRail[int]`                                    | roster fold; codec pair resolved, mounted per service   |
|  [02]   | `inbound(ctx) -> RuntimeRail[RuntimeContext]`                             | admission read; causal decode + `time_remaining()`      |
|  [03]   | `_invoke(codec_pair, request, context, handler) -> RuntimeRail[bytes]`    | decode → railed `handler` → encode `bind`-chain         |
|  [04]   | `settle(servicer_context, context, descriptor, wired) -> bytes`           | rail-terminating fold; `FaultDetail` trailer then abort |
|  [05]   | `serve() -> RuntimeRail[None]`                                            | bind UDS, start, flip `SERVING`, await termination      |
|  [06]   | `drain() -> None`                                                         | `NOT_SERVING` first, then `stop(grace)`                 |
|  [07]   | `CredentialPolicy.server_credentials() -> RuntimeRail[ServerCredentials]` | loopback → `local_server_credentials(UDS)`              |

 `serve()` refuses an empty roster with a typed `config` fault — never a silent empty bind — and awaits termination directly; supervision is the `[04]-[ENTRY]` composition root's. `inbound` lifts `ServicerContext.time_remaining()` into the admitted `Deadline`, feeding the C#-dialed budget to the deadline rail — never an unbounded handler.

- Auto: the interceptor is the ONE trace-context authority — it extracts the C#-minted W3C parent and opens the SERVER span natively, so `inbound` re-extracts nothing, `dispatch` opens no second scope, and a `Signals.attach` around the handler body re-roots spans the interceptor already parented. The health filter suppresses liveness noise from a protocol that is actually served — the registered servicer answers `Check`/`Watch`, so the filter claim is real. `enrich` is `set_attributes` inside the interceptor's active scope, not a hook param on `aio_server_interceptor` (which takes none) and not a hand-rolled tracing interceptor.
- Growth: a new servicer method is one `Route` row; a new wire message is one shapes registry row; a new fault-to-status mapping is one `_FAULT_STATUS` row; a new health surface is automatic with the lifecycle; a new span dimension is one `enrich` key; a new compression algorithm is one `grpc.Compression` member at construction.
- Boundary: the wire contract is the existing C# proto — the runtime mints no transport, no channel, and no second wire vocabulary; the C# host lifecycle and product telemetry export stay AppHost-owned.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable
from datetime import UTC, datetime, timedelta
from typing import Final, Literal, Self, assert_never

import grpc
from expression import Error, Ok, Option, case, tag, tagged_union
from expression.collections import Block, Map
from google.protobuf.message import Message
from grpc_health.v1 import health, health_pb2, health_pb2_grpc
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.instrumentation.grpc import aio_server_interceptor, filters

from rasm.runtime.admission import Deadline, RuntimeContext, RuntimeProfile
from rasm.runtime.clock import CausalFrame
from rasm.runtime.faults import SCOPES, BoundaryFault, Disposition, FaultTag, RuntimeRail, Scope, traversed
from rasm.runtime.shapes import FaultDetail
from rasm.runtime.wire import WireProtoCodec, codec

# --- [TYPES] ----------------------------------------------------------------------------

type RailHandler = Callable[[Struct, RuntimeContext], Awaitable[RuntimeRail[Struct]]]
type CodecPair = tuple[WireProtoCodec[Struct, Message], WireProtoCodec[Struct, Message]]

# --- [CONSTANTS] ------------------------------------------------------------------------

# the round-trip's two symbolic anchors: settle packs them, the invoke ingress reads them.
_DETAIL_KEY: Final[str] = "grpc-status-details-bin"
_FAULT_DETAIL: Final[str] = "fault_detail"
# the C# Instant.ToUnixTimeTicks 100 ns unit; the trailer echo truncates to Timestamp
# microseconds — the carrier headers stay the authoritative full-fidelity stamp.
_TICKS_PER_SECOND: Final[int] = 10_000_000

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class CredentialPolicy:
    tag: Literal["insecure_loopback", "tls", "mtls", "bearer", "composed"] = tag()
    insecure_loopback: bool = case()
    tls: str = case()
    mtls: tuple[str, str] = case()
    bearer: str = case()
    composed: tuple[str, ...] = case()

    @classmethod
    def loopback(cls) -> Self:
        return cls(insecure_loopback=True)

    def server_credentials(self) -> RuntimeRail[grpc.ServerCredentials]:
        match self:
            case CredentialPolicy(tag="insecure_loopback"):
                return Ok(grpc.local_server_credentials(grpc.LocalConnectionType.UDS))
            case CredentialPolicy(tag="tls" | "mtls" | "bearer" | "composed" as outbound):
                return Error(BoundaryFault(config=(outbound, "outbound C# client credential; the companion serves insecure_loopback over UDS only")))
            case _ as unreachable:
                assert_never(unreachable)


class Route(Struct, frozen=True):
    # one servicer method as data: `request`/`response` resolve through the wire registry, `handler` binds into the dispatch aspect.
    service: str
    method: str
    descriptor: str
    request: str
    response: str
    handler: RailHandler


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
            members = {row.method: grpc.unary_unary_rpc_method_handler(self._behavior(row, pair)) for row, pair in rows if row.service == service}
            self._server.add_registered_method_handlers(service, members)
        self._services = self._services | {row.service for row, _ in rows}
        return len(rows)

    def _behavior(self, row: Route, pair: CodecPair) -> Callable[[bytes, grpc.aio.ServicerContext], Awaitable[bytes]]:
        async def method(request: bytes, servicer_context: grpc.aio.ServicerContext) -> bytes:
            return await self.dispatch(servicer_context, request, row.descriptor, pair, row.handler)

        return method

    @staticmethod
    def inbound(servicer_context: grpc.aio.ServicerContext) -> RuntimeRail[RuntimeContext]:
        # the ambient scope IS remote-parented by the interceptor; this reads only the causal frame and the inbound budget.
        carrier = dict(servicer_context.invocation_metadata())
        budget = Option.of_optional(servicer_context.time_remaining()).map(lambda remaining: Deadline(timedelta(seconds=remaining)))
        return CausalFrame.decode(carrier).map(
            lambda causal: RuntimeContext.admit(RuntimeProfile.SIDECAR, deadline=budget.to_optional(), causal=causal)
        )

    @staticmethod
    def enrich(context: RuntimeContext, descriptor: str, fault_case: str) -> None:
        trace.get_current_span().set_attributes(context.attribute() | {"rasm.descriptor": descriptor, "rasm.fault_case": fault_case})

    @staticmethod
    async def settle(servicer_context: grpc.aio.ServicerContext, context: RuntimeContext, descriptor: str, wired: RuntimeRail[bytes]) -> bytes:
        match wired:
            case Ok(payload):
                ServerHost.enrich(context, descriptor, "ok")
                return payload
            case Error(fault):
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
        handler: Callable[[S, RuntimeContext], Awaitable[RuntimeRail[R]]],
    ) -> RuntimeRail[bytes]:
        decode, encode = codec_pair
        match decode.decode(request):
            case Error(fault):
                return Error(fault)
            case Ok(shape):
                return (await handler(shape, context)).bind(encode.encode)
            case _ as unreachable:
                assert_never(unreachable)

    async def dispatch[S: Struct, M: Message, R: Struct, N: Message](
        self,
        servicer_context: grpc.aio.ServicerContext,
        request: bytes,
        descriptor: str,
        codec_pair: tuple[WireProtoCodec[S, M], WireProtoCodec[R, N]],
        handler: Callable[[S, RuntimeContext], Awaitable[RuntimeRail[R]]],
    ) -> bytes:
        match ServerHost.inbound(servicer_context):
            case Error(fault):
                return await ServerHost.settle(servicer_context, RuntimeContext.admit(RuntimeProfile.SIDECAR), descriptor, Error(fault))
            case Ok(context):
                return await ServerHost.settle(servicer_context, context, descriptor, await ServerHost._invoke(codec_pair, request, context, handler))
            case _ as unreachable:
                assert_never(unreachable)

    async def serve(self) -> RuntimeRail[None]:
        if self._services == frozenset():
            return Error(BoundaryFault(config=("serve.roster", "empty-roster")))
        match self._credential.server_credentials():
            case Error(_) as refused:
                return refused
            case Ok(credentials):
                self._server.add_secure_port(self._bind, credentials)
                await self._server.start()
                for service in (health.OVERALL_HEALTH, *sorted(self._services)):  # Exemption: the health flips are the servicer's own async mutation seam.
                    await self._health.set(service, health_pb2.HealthCheckResponse.SERVING)
                await self._server.wait_for_termination()
                return Ok(None)
            case _ as unreachable:
                assert_never(unreachable)

    async def drain(self) -> None:
        # NOT_SERVING races ahead of the stop: probes stop routing new work while the grace window
        # drains in-flight calls; the flip is permanent, so a late success cannot re-advertise.
        await self._health.enter_graceful_shutdown()
        await self._server.stop(self._grace)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _sealed(fault: BoundaryFault, context: RuntimeContext, status: grpc.StatusCode) -> FaultDetail:
    # the one total egress fold off facts() + the inbound frame: the C# WireFault.Decode(FaultDetail)
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

- Owner: `CapabilityInvoke` decodes the C# `Rasm.AppHost/Agent/capability#SDK_CODEGEN` Python target into one dispatch — the request's own `Struct` type and the caller's `into` type are the codec discriminants off the `_ROW_BY_STRUCT` table, so one `run` genuinely spans the whole catalog and no injected per-shape codec pair narrows it. The outbound leg retries under the bare cached `guard(RetryClass.WIRE)` caller with a two-fence ingress — a `guarded(...)` wrap is the ruled-out composition because its terminal lift consumes the exception before the trailer read — so no bare gRPC exception escapes and no trailer erases to a bare `boundary` tag.
- Cases: the per-descriptor `input_schema` is the C# `SuiteContracts.Schema` JSON Schema carried as a deferred `msgspec.Raw` slot, so the routing decode never pays the schema-document parse; the argument payload is the already-typed canonical `Struct` the resolved codec transcodes, never a hand-mirrored mapping re-validated against a schema document. The `effect`/`idempotency`/cost-unit keys decode as the C# smart-enum string keys.
- Entry: the per-call descriptor dimension rides the interceptor-set `rpc.service`/`rpc.method` attributes natively — the invoke path IS `/rasm.capability/{descriptor_id}` — while the channel-stable hooks enrich tenant and fault case on the CLIENT span; an ambient per-call `set_attribute` lands on whatever span was current BEFORE the CLIENT span opened.
- Packages: `msgspec`, `grpcio`, `opentelemetry-instrumentation-grpc`, and the shapes/wire/resilience/faults rails per the fence imports; `guard` is exported for exactly this composed per-seam aspect.
- Growth: a new capability is one descriptor row the C# registry already folds — the companion reads it through the existing `discover`/`run` pair; a new wire shape reaches the invoke through one shapes registry row with zero edits here; a new span dimension is one hook key.
- Boundary: the descriptor is the suite's only op-metadata owner and the C# registry its sole mint; the companion re-authors no capability shape. Cross-language shape identity is the C# `SuiteContracts.Schema` JSON Schema all three SDKs bind, evolution riding the one `ContractGuard.AdditiveOnly` classifier.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable
from typing import Final, Literal, Self, assert_never

import grpc
import msgspec
from expression import Error, Ok
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

# `_DETAIL_KEY`/`_FAULT_DETAIL` are this module's [02]-[SERVE] constants — one trailer spelling for egress pack and ingress lift.

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
        # the descriptor-keyed fold lives at the one decode site, never a list-to-Map re-key the composition root hand-spells.
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
    def connect(cls, target: str, catalog: Map[str, DiscoveryResult], tenant: Tenant) -> Self:
        channel = grpc.aio.insecure_channel(target, interceptors=cls.interceptors(tenant))

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

        return cls(catalog, dispatch, channel)

    async def run[S: Struct, R: Struct](self, descriptor_id: str, request: S, into: type[R]) -> RuntimeRail[R]:
        staged = (
            self._catalog.try_find(descriptor_id)
            .to_result(BoundaryFault(wire=(descriptor_id, 0)))
            .bind(lambda _: _transcoder(type(request)))
            .bind(lambda transcode: transcode.encode(request))
        )
        match staged:
            case Error(_) as refused:
                return refused
            case Ok(payload):
                # the registry row pins struct identity, so the `into`-resolved decode returns the caller's typed R.
                return (await self._dispatch(descriptor_id, payload)).bind(lambda wire: _transcoder(into).bind(lambda transcode: transcode.decode(wire)))
            case _ as unreachable:
                assert_never(unreachable)

    async def aclose(self) -> None:
        # the runtime-lived channel's deterministic drain; a directly-injected dispatch carries no channel, a typed no-op.
        if self._channel is not None:
            await self._channel.close()


# --- [OPERATIONS] -----------------------------------------------------------------------


def _transcoder(shape: type[Struct]) -> RuntimeRail[WireProtoCodec[Struct, Message]]:
    return _ROW_BY_STRUCT.try_find(shape).to_result(BoundaryFault(wire=(shape.__name__, 0))).bind(codec)


def _unsealed(terminal: grpc.aio.AioRpcError) -> BoundaryFault:
    # the trailer's fault_detail row decodes to the typed producer conflict; an absent or undecodable
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

- Owner: `Entrypoint` is the `cyclopts` command axis AND the daemon composition root, co-located with `ServerHost` because the serve command composes the host it launches. `companion_app(routes, drains)` is parameterized over both the servicer roster and the drainable owners, so a downstream folder's composition root — geometry `mesh/serve` the named consumer — registers its rows and drain stages by data; runtime never imports a sibling, the terminal-tier `serve -> lanes | recipe` imports the module's only sibling edges.
- Entry: the drain fold owns ORDER — the caller's `drains` rows with `Telemetry.shutdown` appended LAST so every earlier stage's spans and receipts still export — and every stage settles even after an earlier fault, the faults accumulating into one aggregate; a first-fault abort leaving later stages undrained never lands. The boot chain rides the faults `railed` builder over heterogeneous binds a `traversed` fold cannot express.
- Packages: `cyclopts`, `anyio`, `msgspec`, and the faults/telemetry/receipts/resilience/admission/lanes/recipe owners per the fence imports.
- Growth: a new private command is one `@app.command` method folding through the shared `_exit`; a new drainable owner is one `(subject, stage)` row the ordered fold, the accumulate, and the receipt absorb; a sibling daemon is one `companion_app(routes, drains)` call with its own rows.
- Boundary: never a new public command surface — public commands are reserved to the suite Assay command surface.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import signal
from collections.abc import Awaitable, Callable, Generator
from pathlib import Path
from typing import Annotated, Any, assert_never

import anyio
import msgspec
from anyio.streams.memory import MemoryObjectSendStream
from cyclopts import App, Parameter
from cyclopts.types import NonNegativeFloat
from expression import Error, Ok, Option, Result
from expression.collections import Block, Map

from rasm.runtime.admission import RuntimeProfile, SettingsAdmission
from rasm.runtime.faults import SCOPES, Disposition, RuntimeRail, Scope, async_boundary, boundary, railed, traversed
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import OPEN, DrainReceipt, Receipt, Signals
from rasm.runtime.recipe import RecipeExecution, RecipeName, RecipeSpec
from rasm.runtime.resilience import RetryMode, install
from rasm.runtime.shapes import aligned
from rasm.runtime.telemetry import Telemetry

# `ServerHost`/`CredentialPolicy`/`Route` are this module's [02]-[SERVE] owners — no cross-module import.

# --- [TYPES] ----------------------------------------------------------------------------

type DrainStage = Callable[[], Awaitable[object] | RuntimeRail[object]]

# --- [OPERATIONS] -----------------------------------------------------------------------


def _exit(outcome: RuntimeRail[object] | DrainReceipt[object]) -> int:
    # the one CLI exit fold both commands share; the traceback never escapes the CLI boundary.
    match outcome:
        case Result() as rail:
            return rail.map(lambda _value: 0).default_value(1)
        case receipt:
            return 0 if receipt.faults.is_empty() else 1


@railed
def _booted(bind: str, grace: float, routes: Block[Route]) -> Generator[Any, Any, ServerHost]:
    # install -> admit -> gate -> bind as one railed bind chain: the first Error short-circuits,
    # the composed host is the Ok payload; an absent otel endpoint installs nothing — no literal.
    settings = yield from boundary("config", SettingsAdmission)
    Option.of_optional(settings.otel_endpoint).map(lambda endpoint: Telemetry.install(RuntimeProfile.SIDECAR, str(endpoint)))
    Signals.configure()
    install(RetryMode.EMIT)
    yield from aligned()
    host = ServerHost(bind, CredentialPolicy.loopback(), grace)
    yield from host.register(routes)
    return host


async def _settled(subject: str, stage: DrainStage) -> RuntimeRail[object]:
    # the stage() call itself is fenced so a synchronous raise converts instead of escaping the drain fold; a rail-returning
    # sync owner passes through, an async owner awaits under the same named fence.
    match boundary(subject, stage):
        case Error(_) as refused:
            return refused
        case Ok(Result() as rail):
            return rail
        case Ok(pending):
            return await async_boundary(subject, lambda: pending)
        case _ as unreachable:
            assert_never(unreachable)


async def _drained(stages: Block[tuple[str, DrainStage]]) -> RuntimeRail[Block[object]]:
    settled: Block[RuntimeRail[object]] = Block.empty()
    for subject, stage in stages:  # Exemption: the ordered drain — every stage runs even after an earlier fault; the rails accumulate below.
        settled = settled.append(Block.singleton(await _settled(subject, stage)))
    return traversed(settled, by=Disposition.ACCUMULATE)


async def _supervised(host: ServerHost, drains: Block[tuple[str, DrainStage]]) -> RuntimeRail[None]:
    send, receive = anyio.create_memory_object_stream[RuntimeRail[object]](max_buffer_size=2)
    trip = anyio.CancelScope()

    async def hosting(sink: MemoryObjectSendStream[RuntimeRail[object]]) -> None:
        # the serve leg is fenced: a platform raise lands on the rail instead of killing the group as an unconverted
        # ExceptionGroup, so the trip cancel below ALWAYS runs and the daemon never hangs on a signal that will not come.
        async with sink:
            await sink.send((await async_boundary("serve.host", host.serve)).bind(lambda rail: rail))
        trip.cancel()  # a failed or refused bind releases the signal wait

    async def tripped(sink: MemoryObjectSendStream[RuntimeRail[object]]) -> None:
        with trip:
            with anyio.open_signal_receiver(signal.SIGTERM, signal.SIGINT) as trips:  # Exemption: the platform signal seam.
                async for _ in trips:
                    break
        async with sink:
            # stage one of the drain order: health flip + stop(grace) unblocks wait_for_termination.
            await sink.send(await async_boundary("serve.drain", host.drain))

    async with anyio.create_task_group() as group, send:  # Exemption: the daemon's one supervision group.
        group.start_soon(hosting, send.clone())
        group.start_soon(tripped, send.clone())

    settled = Block.of_seq([outcome async for outcome in receive])
    flushed = await _drained(drains.append(Block.singleton((str(Scope.SERVICE), Telemetry.shutdown))))
    outcome = traversed(settled.append(Block.singleton(flushed)), by=Disposition.ACCUMULATE)
    await Signals.emit_async(
        Receipt.of(SCOPES[Scope.SERVICE], ("emitted", "shutdown", {"stages": len(settled) + len(drains) + 1, "clean": outcome.is_ok()})), OPEN
    )
    return outcome.map(lambda _: None)


async def _daemon(bind: str, grace: float, routes: Block[Route], drains: Block[tuple[str, DrainStage]]) -> RuntimeRail[None]:
    match _booted(bind, grace, routes):
        case Error(_) as refused:
            return refused
        case Ok(host):
            return await _supervised(host, drains)
        case _ as unreachable:
            assert_never(unreachable)


# --- [ENTRY] ----------------------------------------------------------------------------


def companion_app(routes: Block[Route], drains: Block[tuple[str, DrainStage]] = Block.empty()) -> App:
    app = App(name=SCOPES[Scope.SERVICE], help="private companion daemon entry", result_action="return_int_as_exit_code_else_zero")

    @app.command
    async def serve(bind: str, *, grace: Annotated[NonNegativeFloat, Parameter(env_var="RASM_COMPANION_GRACE")] = 5.0) -> int:
        return _exit(await _daemon(bind, grace, routes, drains))

    @app.command
    async def recipe(selector: str, assignments: Path | None = None) -> int:
        # one-shot local bind of the execution/recipe#RECIPE owner: a RecipeName member or an external recipe-folder path;
        # the boot pair mirrors the serve leg so the engine-gate retries ride RETRY_HOOKS.
        Signals.configure()
        install(RetryMode.EMIT)
        execution = RecipeExecution(lane=LanePolicy(capacity=1))
        loaded = (
            Ok(Map.empty())
            if assignments is None
            else boundary("config", lambda: Map.of_seq(msgspec.json.decode(assignments.read_bytes(), type=dict[str, object]).items()))
        )
        spec = loaded.map(
            lambda inputs: RecipeSpec(recipe=boundary("config", lambda: RecipeName(selector)).default_value(selector), inputs=inputs)
        )
        match spec:
            case Error(_) as refused:
                return _exit(refused)
            case Ok(one):
                return _exit(await execution.execute(one))
            case _ as unreachable:
                assert_never(unreachable)

    return app
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
