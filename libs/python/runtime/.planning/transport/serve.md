# [PY_RUNTIME_SERVE]

The companion server-host and daemon composition root. It composes the `transport/wire#PROTO_TRANSCODE` codec, the `transport/shapes#REGISTRY_AND_DRIFT` vocabulary rows, the `clock#CLOCK` causal time, and the `execution/admission#CONTEXT` context — and re-mints none.

- `ServerHost` owns the `grpc.aio` server lifecycle (bind, interceptor injection, `grpc.Compression` selection, the registered health servicer, graceful drain) plus the one `register` roster fold and the one `dispatch` servicer-body aspect every registered method folds through: inbound admit with the `ServicerContext.time_remaining()` budget lift, the `WireProtoCodec` decode → railed `handler` → encode `bind`-chain, and the one terminal `settle` fold packing the typed `FaultDetail` trailer before abort. A servicer method is a `Route` row (service, method, descriptor id, two wire-registry row names, railed handler), never a hand-written admit/transcode/abort prologue. It hosts the geometry companion daemon over the existing C# `ComputeService`/`DocumentService`/`ControlService`/`ArtifactSync` gRPC contract on the UDS leg.
- `CredentialPolicy` is the Python half of the C#-minted axis of the same name, decoded against the five producer rows and resolved to the one credential the UDS serve leg admits: `insecure_loopback`, peer identity read by the kernel at accept, never a PEM bundle.
- `CapabilityInvoke` consumes the C#-generated capability-descriptor SDK, deriving the companion command surface from the descriptor catalog, resolving its codec per descriptor off the one shapes seed table, and lifting the `grpc-status-details-bin` trailer into the typed fault on the ingress leg.
- `Entrypoint` is the daemon COMPOSITION ROOT: the `cyclopts` serve command folds install → admit → serve → ordered receipted drain, telemetry flushing LAST, into one accumulated shutdown rail the exit code folds; a sibling recipe command binds the `execution/recipe#RECIPE` owner for one-shot local execution, the terminal-tier `serve -> lanes | recipe` imports its only sibling edges.

The wire vocabulary the inbound servicers and the outbound dispatch carry is `transport/shapes#VOCABULARY`'s; the transcode machinery is `transport/wire#PROTO_TRANSCODE`'s. The seam ledger files the `CredentialPolicy` axis decode and the W3C inbound extraction on this page — the interceptor at this ingress is the one trace-context authority.

## [01]-[INDEX]

- [02]-[SERVE]: the inbound `grpc.aio` server-host lifecycle with the registered health servicer, the decoded `CredentialPolicy` axis, the `Route` registration roster, the interceptor-owned trace-context authority, the `time_remaining()` deadline lift, the one polymorphic servicer-body dispatch aspect, the `Map[FaultTag, grpc.StatusCode]` projection, and the `FaultDetail` trailer egress.
- [03]-[CAPABILITY_INVOKE]: the descriptor-driven polymorphic invoke decoding the C# capability SDK, per-descriptor codec resolution off the shapes seed table, the catalogue client interceptors, and the `fault_detail` trailer ingress.
- [04]-[ENTRY]: the daemon composition root — railed boot, supervised serve, the ordered receipted drain fold over the drainable owners, and the one-shot recipe command binding the `execution/recipe#RECIPE` owner through the one shared `_exit` fold.

## [02]-[SERVE]

- Owner: `ServerHost` — the boundary capsule over one `grpc.aio` server plus one registered `grpc_health` `aio.HealthServicer`. It owns the bind, the construction-time interceptor and compression injection, the `register` roster fold mounting `Route` rows as raw-bytes registered method handlers, the inbound carrier decode with the one real inbound budget lift, the one `dispatch` servicer-body aspect (admit → decode/handler/encode `bind`-chain → one `settle` enrich/trailer/abort fold), the SERVER-span enrichment, the `Map[FaultTag, grpc.StatusCode]` fault projection, and the graceful drain whose health flip races ahead of the stop. `CredentialPolicy` is the tagged union decoding the C#-minted five-row axis — one spelling on both sides of the wire — to the one credential the UDS serve leg admits. `Route` is the servicer-method-as-data row: service, method, descriptor id, the two `transport/shapes#REGISTRY_AND_DRIFT` registry row names its codec pair resolves through, and the railed handler. `ServerHost` composes four owners and re-mints none: the `transport/wire#PROTO_TRANSCODE` `WireProtoCodec` transcodes the servicer wire shapes and `codec(name)` resolves each `Route`'s pair; the `clock#CLOCK` `CausalFrame.decode(carrier)` railed reader owns the `SLOTS`-keyed parse and the one wire fence over the inbound `(Hlc, Tenant)` frame; the `execution/admission#CONTEXT` `RuntimeContext` carries the admitted deadline/causal context and its `attribute()` span projection; the `transport/shapes#VOCABULARY` `FaultDetail` is the typed conflict the `settle` trailer packs.
- Cases: the C# `CredentialPolicy` mints five rows (`insecure_loopback`, `tls`, `mtls`, `bearer`, `composed`); the decode admits one constructible serve-side case. The companion serves the local control plane over `RemoteTransport.UnixDomainSocket`, whose `Credentials` column is `Seq(CredentialPolicy.InsecureLoopback)` exactly, so `CredentialPolicy(insecure_loopback=True)` is the only server credential and peer identity is the kernel-reported `(pid, uid)` the C# `AppHost/companion-sidecar#PEER_ADMISSION` reads at accept through `SO_PEERCRED`/`LOCAL_PEERCRED`, never a wire-carried PEM. The other four rows are the outbound client legs the C# `Rasm.AppHost` dials, so `server_credentials` is total over the five producer rows by `match`/`assert_never`: the four outbound rows return `Error(BoundaryFault(config=...))` on the `RuntimeRail`, a row the UDS serve leg cannot admit being a typed construction failure, never a bare `ValueError` and never a phantom PEM bundle. The health vocabulary is the closed `health_pb2.HealthCheckResponse.ServingStatus` enum — `SERVING`/`NOT_SERVING` the two live states the lifecycle flips, `health.OVERALL_HEALTH` (`""`) the whole-server key set beside every registered service name.
- Entry: the method roster —

  | [METHOD]                                                  | [CONTRACT]                                                                                                                                                                                                                                                                                                            |
  | :-------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
  | `register(routes) -> RuntimeRail[int]`                    | the roster fold: every `Route` resolves its codec pair through the wire `codec(name)` rail — `traversed(..., ACCUMULATE)` so ONE accumulated fault names every unresolvable row — and mounts grouped per service through `grpc.unary_unary_rpc_method_handler` + `add_registered_method_handlers` with raw-bytes (de)serializers, each behavior delegating to the one `dispatch` aspect; a sibling composition root registers its own rows (geometry `mesh/serve` the named consumer), and `serve()` refuses an empty roster, so an unregistered server is a typed fault, never a silently empty bind. |
  | `inbound(ctx) -> RuntimeRail[RuntimeContext]`             | the admission read: the `aio_server_interceptor` already extracted the W3C carrier and opened the remote-parented SERVER span, so `inbound` re-extracts nothing — it decodes the causal frame through the `clock#CLOCK` `CausalFrame.decode(carrier)` sole fence (a malformed frame lands `Error(BoundaryFault(boundary=("wire", "ValidationError")))`) and lifts `ServicerContext.time_remaining()` into the admitted `RuntimeContext` `Deadline`, so the C#-dialed budget feeds the whole deadline rail (`LanePolicy.deadline`, the `CLASSIFY` deadline row) instead of every handler running unbounded. |
  | `_invoke(codec_pair, request, context, handler) -> RuntimeRail[bytes]` | the `WireProtoCodec` decode → railed `handler` → encode stage: synchronous `decode.decode(request)` short-circuits `Error` directly, the `Ok(shape)` arm `await`s `handler(shape, context)` then `.bind(encode.encode)` so the handler and encode faults both ride the one rail — the await being the single coroutine point a pure `Result.bind` chain cannot thread. |
  | `settle(servicer_context, context, descriptor, wired) -> bytes` | the single rail-terminating fold: `Ok(payload)` enriches and returns the wire bytes; `Error(fault)` enriches, projects the status through `_FAULT_STATUS`, packs the `_sealed` `FaultDetail` through the wire `fault_detail` row onto the `grpc-status-details-bin` trailer via `set_trailing_metadata` BEFORE the abort — the C# `WireFault.Decode(FaultDetail)` consumer reads the typed conflict, the `"; ".join` facts string demoting to the human-readable `details` — and `abort` is `NoReturn`, so no dead return follows it. |
  | `serve() -> RuntimeRail[None]`                            | refuses an empty roster with a typed `config` fault, binds the UDS socket through the railed `server_credentials` fold, starts the server, flips `OVERALL_HEALTH` plus every registered service to `SERVING`, and `await`s termination directly — the single-await needs no task group; supervision is the `[04]-[ENTRY]` composition root's. |
  | `drain() -> None`                                         | the graceful choreography: `enter_graceful_shutdown()` permanently flips every registered service to `NOT_SERVING` FIRST — probes stop routing while the grace window drains in-flight calls, and a late success cannot re-advertise — then `server.stop(grace)`. |
  | `CredentialPolicy.server_credentials() -> RuntimeRail[ServerCredentials]` | folds the loopback case to `grpc.local_server_credentials(grpc.LocalConnectionType.UDS)` on the `Ok` rail so the UDS peer authenticates by socket locality, never a key-chain pair; `CredentialPolicy.loopback()` is the zero-argument constructor the serve default and the entrypoint mint. |
- Auto: the interceptor is the ONE trace-context authority. `aio_server_interceptor(filter_=filters.negate(filters.health_check()))` extracts the C#-minted W3C parent and opens the SERVER span around the handler body natively, so the handler's ambient context is already remote-parented — `inbound` re-extracts nothing, `dispatch` opens no second scope, and a `Signals.attach(remote-parent)` around the body (which re-rooted handler spans as SERVER's siblings) is the deleted form; the receipts pair keeps its real consumer at the lanes offload stitch. The health filter suppresses liveness noise from a protocol that is actually served: the registered `health.aio.HealthServicer` answers `Check`/`Watch`, so the filter claim is real, never a phantom.
- Auto: every `BoundaryFault` tag projects to a `grpc.StatusCode` through the `_FAULT_STATUS` `Map[FaultTag, grpc.StatusCode]` keyed on the closed `reliability/faults#FAULT` `FaultTag` literal — the fault tag IS the status key, never a per-arm `if` ladder, never a `(code, label)` tuple duplicating the key. `_sealed` is the one total `FaultDetail` egress fold: `facts()` supplies the case slots and evidence map, the inbound frame echoes `hlc_physical`/`hlc_logical`/`tenant` (Timestamp precision on the echo; the carrier headers stay the authoritative full-fidelity stamp), the correlation is the admitted trace id, and the numeric status is the wire code. A failed trailer encode skips the trailer, never the abort.
- Auto: the SERVER-span enrichment is `set_attributes` composed inside the interceptor's active scope — `ServerHost.enrich` folds `RuntimeContext.attribute()` (`rasm.tenant`/`rasm.hlc` are the `clock#CLOCK` projection, never re-spelled) with the `rasm.descriptor` and `rasm.fault_case` dimensions in one call, so a flame graph shows descriptor, tenant, HLC, and resolved fault case per RPC. This is `set_attributes`, not a hook param on `aio_server_interceptor` (which takes none) and not a hand-rolled tracing interceptor. Compression rides the `grpc.Compression` member passed at construction; the `TracerProvider` arrives installed at `observability/telemetry#TELEMETRY`.
- Growth: a new servicer method is one `Route` row through the existing `register` — never a new prologue; a new wire message is one `transport/shapes#REGISTRY_AND_DRIFT` row the codec resolution already spans; a new fault-to-status mapping is one `_FAULT_STATUS` row keyed by the new `FaultTag`; a new health surface is automatic — every registered service flips with the lifecycle; a new enriched span dimension is one key in the `enrich` union; a new compression algorithm is one `grpc.Compression` member at construction; zero new surface, no second RPC server.
- Boundary: the wire contract is the existing C# proto; the runtime mints no transport, no channel, and no second wire vocabulary. The C# host lifecycle and product telemetry export stay AppHost-owned; the `Hlc`/`ElementId`/`Tenant`/`CausalFrame` stamp lives in `clock#CLOCK`, the codec in `transport/wire`, the shapes in `transport/shapes`, all consumed here and re-minted nowhere. The deleted forms:
  - a hand-rolled message loop, a hand-wired servicer beside the `register` fold, a divergent message shape, a second RPC server, or a per-servicer admit/transcode/abort prologue where the one `dispatch` aspect folds every method;
  - a `Signals.attach(remote-parent)` scope around the handler body re-rooting spans the interceptor already parented, a second W3C extract beside the interceptor, or a fresh-root span where the C# parent is on the inbound carrier;
  - an unbounded handler ignoring `ServicerContext.time_remaining()` where the one real inbound budget feeds the admitted `Deadline`; an empty-roster `serve()` that binds and serves nothing where the typed `config` refusal names it;
  - a `":".join` facts string as the ONLY fault egress where the typed `FaultDetail` trailer crosses via `set_trailing_metadata` before `abort`; a `return b""` after the `NoReturn` abort — dead code and a fictional Error-arm return; a hand-indexed `facts()["detail"]` that panics on the slot-divergent arms where `_sealed` folds the whole map;
  - a `Credential` spelling diverging from the C#-minted `CredentialPolicy` axis name; a key-chain server credential on the UDS leg; a health filter over a protocol never served; a `set(...)`-based drain flip a late success can re-advertise where `enter_graceful_shutdown()` is permanent;
  - a four-level nested `match` ladder over the decode/handler/encode rail where one `Result.bind` chain plus the one `settle` fold owns the flow; a `(grpc.StatusCode, str)` tuple table value, a stringly `dict[str, ...]` status table, or an inline `int(...)` carrier parse with re-spelled `rasm-hlc-*` literals where `CausalFrame.decode` owns the `SLOTS`-keyed parse and the sole fence.

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

# the V14 round-trip's two symbolic anchors: settle packs them, the invoke ingress reads them.
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
    # one servicer method as data: the registration fold resolves `request`/`response` through
    # the wire registry and binds `handler` into the one dispatch aspect.
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
        # the interceptor already extracted W3C context and opened the SERVER span, so the ambient
        # scope IS remote-parented; this reads only the causal frame (the clock owner's sole fence)
        # and the one real inbound budget — time_remaining() lifting the C#-dialed deadline.
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

- Owner: `CapabilityInvoke` — the descriptor-driven polymorphic invoke decoding the C# `Rasm.AppHost/Agent/capability#SDK_CODEGEN` Python target. `DiscoveryResult` is the catalog row ingested from the C# `DiscoveryResultWire[]` projection, carrying the per-descriptor `input_schema` as a deferred-decode `msgspec.Raw` slot; `CommandReceipt` is the per-command evidence read back through the C# `ReceiptEnvelopeWire<CapabilityCommandReceiptWire>`. One `run` keyed by descriptor id discriminates every capability through the frozen `Map[str, DiscoveryResult]` catalog and resolves its codec PER DESCRIPTOR off the `_ROW_BY_STRUCT` table derived from the one `transport/shapes#REGISTRY_AND_DRIFT` seed table — the request's own `Struct` type and the caller's `into` type are the discriminants, so one invoke genuinely spans the whole catalog and no injected per-shape codec pair narrows it to a single request/response shape.
- Owner: the outbound dispatch wraps its channel with `aio_client_interceptors(tracer_provider, filter_, request_hook, response_hook)` so the call propagates the active span through the `observability/telemetry#TELEMETRY`-installed composite; the CLIENT-span hooks enrich the channel-stable `clock#CLOCK` `Tenant` and the fault case, while the per-call descriptor dimension rides the interceptor-set `rpc.service`/`rpc.method` attributes natively — the invoke path IS `/rasm.capability/{descriptor_id}`, so `rpc.method` carries the descriptor with zero hand machinery and the prior ambient `set_attribute` (which landed on whatever span was current BEFORE the CLIENT span opened) is the deleted form. The dispatch retries under the bare cached `guard(RetryClass.WIRE)` caller — the transient status trio discriminated by the resilience `_wire_transient` row — and converts the terminal raise at the channel's two-fence ingress: the `grpc.aio.AioRpcError` fence lifts `trailing_metadata()` and decodes the `fault_detail` row into the typed producer conflict, and the generic `async_boundary` fence converts every other raise, so no bare gRPC exception escapes and no trailer is erased to `boundary=("wire", "AioRpcError")`.
- Cases: the C# SDK Python target emits one method per descriptor — each delegating to `run("{surface}.{op}", request, into=...)` — so the companion decodes the catalog into one dispatch, `descriptor_id` the `{surface}.{op}` join the C# `CapabilityDescriptor.Of` mints. The per-descriptor `input_schema` is the `JsonSchemaExporter` JSON Schema the C# `SuiteContracts.Schema` derives, carried as a deferred `msgspec.Raw` slot so the routing decode never pays the schema-document parse; the argument payload is the already-typed canonical `Struct` the resolved codec transcodes, not a hand-mirrored mapping re-validated against a schema document. The command-txn disposition decodes onto `CommandTxn` whose `kind: CommandTxnKind` is the literal-discriminated disposition the C# `CommandTxn` mints (`committed`, `rolled_back`, `compensated`, `refused`), never an erased `dict`.
- Entry: `CapabilityInvoke.run(descriptor_id, request, into)` is one `Result`-railed `lookup → resolve → encode → dispatch → resolve → decode` chain: the catalog `try_find(...).to_result(BoundaryFault(wire=(descriptor_id, 0)))` lifts the absent id, `_transcoder(type(request))` resolves the encode codec off the request value's OWN type (the discriminant recoverable from the value, never a name suffix or a knob), the awaited dispatch is the single coroutine point, and `_transcoder(into)` resolves the decode back to the caller's typed `R` — the registry row pins struct identity, so the `into`-resolved decode returns the caller's shape.
- Entry: `CapabilityInvoke.discover` reads the catalog the C# `CapabilityRegistry.Discover(DiscoveryQuery.All)` emits through `msgspec.json.Decoder(list[DiscoveryResult])` on the one `boundary("wire", ...)` fence, then `.map`s the rows into the descriptor-keyed `Map[str, DiscoveryResult]` — the keying fold lives at the one decode site, so the command surface is generated from the descriptor rows, never hand-listed.
- Entry: `CapabilityInvoke.connect` is the single channel composer: it mints the one outbound `grpc.aio.insecure_channel` (loopback UDS) wrapped with the `interceptors(tenant)` factory and closes the railed `dispatch` over it. The channel is the runtime-lived multicallable pool shared across every `run` — the invoke owns its deterministic `grpc.aio.Channel.close` drain through `aclose`, the same runtime-lived-client-with-one-drain shape `transport/roots#RESOURCE` gives the pooled `httpx.AsyncClient`; a directly-injected `WireDispatch` (the test/sidecar-composition path) carries no channel, so `aclose` is a typed no-op.
- Auto: the descriptor catalog crosses as the `DiscoveryResultWire[]` array (`descriptor`, `surface`, `effect`, `idempotency`, `estimated`, `scope_hash`); the `effect`/`idempotency`/cost-unit keys decode as the C# smart-enum string keys, the cost vector as the unit-keyed mapping `CostVectorWire` emits. The channel-stable client hooks set `rasm.tenant`/`rasm.fault_case` on the CLIENT span through `Span.set_attribute` inside the active scope, never blocking; `response_hook` receives the awaited unary status-details string (`""` on OK). The factory's `tracer_provider` defaults to the global provider the telemetry install registers, so no second tracer mints.
- Packages: `msgspec` (`json.Decoder` the catalog decode / `Struct` the shapes / `Raw` the deferred `input_schema` slot), `grpcio` (`grpc.aio.insecure_channel`/`grpc.aio.Channel`/`grpc.aio.AioRpcError` with `.code()`/`.details()`/`.trailing_metadata()` — the client-fault law lifted at the channel boundary), `opentelemetry-instrumentation-grpc` (`aio_client_interceptors` + `filters`; the interceptor-set `rpc.service`/`rpc.method` semantic attributes the descriptor rides), `opentelemetry-api` (`trace.Span.set_attribute` the hook enrichment), `expression` (`Result.bind`/`Map.try_find`/`Option.to_result` the chain; `Block.filter`/`try_head` the trailer lookup), `transport/shapes#REGISTRY_AND_DRIFT` (`PROTO_VOCABULARY` the `_ROW_BY_STRUCT` derivation source), `transport/wire#PROTO_TRANSCODE` (`codec(name)` the per-row resolution), `reliability/resilience#RESILIENCE` (`guard(RetryClass.WIRE)` the bare cached retry caller — exported for exactly this composed per-seam aspect), `reliability/faults#FAULT` (`boundary`/`async_boundary`/`BoundaryFault`/`RuntimeRail`).
- Growth: a new capability is one descriptor row the C# registry already folds — the companion reads it through the existing `discover`/`run` pair, never a new method; a new wire shape reaches the invoke through one shapes registry row (`_ROW_BY_STRUCT` derives, `run` resolves — zero edits here); a new effect/idempotency/cost key is one literal the decode union already discriminates; a new enriched span dimension is one `set_attribute` key in the existing hook; zero new surface, no per-service hand client.
- Boundary: the descriptor is the suite's only op-metadata owner and the C# registry its sole mint; the companion re-authors no capability shape and mints no fourth wire shape. The cross-language shape-identity is the C# `SuiteContracts.Schema` JSON Schema all three SDKs bind, evolution riding the one `ContractGuard.AdditiveOnly` classifier. The deleted forms:
  - a per-capability hand client, a re-authored descriptor shape, or a hand-mirrored argument schema validated against a JSON Schema document `msgspec` cannot check;
  - a single injected codec pair freezing the invoke to one request/response shape where `_transcoder` resolves per descriptor off the one seed table; a parallel descriptor→codec-name table minted beside the shapes registry where the struct TYPE is the derivable discriminant;
  - an `AioRpcError` erased to `boundary=("wire", "AioRpcError")` with its trailer dropped where the trailer fence decodes the `fault_detail` row into the typed producer conflict; a `guarded(...)` wrap on this leg whose terminal lift consumes the exception before the trailer can be read — the bare `guard` caller plus the two-fence ingress is the ruled composition here;
  - an ambient per-call `set_attribute("rasm.descriptor", ...)` landing on the pre-CLIENT span where the interceptor-set `rpc.method` carries the descriptor natively; an empty-descriptor channel-lifetime interceptor; a hand-rolled tracing interceptor; a second tracer;
  - a per-`run` `async with grpc.aio.insecure_channel(...)` reopen or a leaked channel with no drain where the `connect`-minted runtime-lived channel drains through `aclose`;
  - a dual-purpose `_catalog` name binding both the class-level decoder and the instance lookup where `_DISCOVERY` is the distinct class constant; an `if descriptor_id not in self._catalog` guard where `Map.try_find(...).to_result(...)` lifts the absent id; a `dict[str, DiscoveryResult]` catalog where the persistent `Map` carries the lookup rail.

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

# `_DETAIL_KEY`/`_FAULT_DETAIL` are the [02]-[SERVE] constants of this same `rasm.runtime.serve`
# module — one trailer spelling for the egress pack and the ingress lift.

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
        # the descriptor-keyed fold lives at the one decode site, never a `list`-to-`Map` re-key
        # the composition root hand-spells.
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
                # Exemption: the trailer fence — grpc-status-details-bin lives only on the live
                # AioRpcError, so this one platform-forced except reclassifies the terminal raise
                # AFTER the WIRE-row retry exhausts; every other raise rides the generic fence below.
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
                # the registry row pins struct identity, so the `into`-resolved decode returns the
                # caller's typed R; the CLIENT span carries the descriptor as `rpc.method`.
                return (await self._dispatch(descriptor_id, payload)).bind(lambda wire: _transcoder(into).bind(lambda transcode: transcode.decode(wire)))
            case _ as unreachable:
                assert_never(unreachable)

    async def aclose(self) -> None:
        # the invoke owns the runtime-lived channel's deterministic drain, mirroring ServerHost.drain;
        # a directly-injected dispatch (tests) carries no channel and the drain is a typed no-op.
        if self._channel is not None:
            await self._channel.close()


# --- [OPERATIONS] -----------------------------------------------------------------------


def _transcoder(shape: type[Struct]) -> RuntimeRail[WireProtoCodec[Struct, Message]]:
    return _ROW_BY_STRUCT.try_find(shape).to_result(BoundaryFault(wire=(shape.__name__, 0))).bind(codec)


def _unsealed(terminal: grpc.aio.AioRpcError) -> BoundaryFault:
    # V14 client ingress: the trailer's fault_detail row decodes to the typed producer conflict;
    # an absent or undecodable trailer falls back to the status-coded lift, never a bare erasure.
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

- Owner: `Entrypoint` — the type-hint-driven `cyclopts` command axis AND the daemon composition root, co-located with `ServerHost` because the serve command composes the host it launches. `companion_app(routes, drains)` is parameterized over BOTH the servicer roster and the extra drainable owners, so a downstream folder's composition root (geometry `mesh/serve` the named consumer) registers its rows and drain stages by data and composes install → admit → serve → drain strictly downward; runtime never imports a sibling.
- Entry: `_booted` is the railed boot chain — the `reliability/faults#FAULT` `railed` computation expression over the heterogeneous binds a `traversed` fold cannot express: settings admission (`boundary("config", SettingsAdmission)`), the gated telemetry install (`Option`-folded off `otel_endpoint` — an absent endpoint installs nothing, no literal fallback), `Signals.configure`, the resilience `install(RetryMode.EMIT)` hook registration, the `transport/shapes#REGISTRY_AND_DRIFT` `aligned()` drift gate BEFORE the bind (the gate's chartered composition-root call site), and the host mint plus `register(routes)` roster fold; the first `Error` short-circuits and the composed host is the `Ok` payload.
- Entry: `_supervised` runs the daemon under one task group — the hosting child resolves `ServerHost.serve` under its named `async_boundary` fence (a platform raise from the bind/start/health-flip calls lands on the rail, never an unconverted `ExceptionGroup` killing the group) and the trip child waits on `SIGTERM`/`SIGINT`, runs the railed `host.drain` (health `NOT_SERVING` flip, then `server.stop(grace)`) which unblocks `wait_for_termination`, and a failed bind cancels the trip scope so the daemon never hangs on a signal that will not come. After the group joins, `_drained` folds the remaining stages IN ORDER — the caller's `drains` rows (`CapabilityInvoke.aclose`, lanes/recipe drains, roots teardown, whichever owners the composing root minted) with `Telemetry.shutdown` appended LAST — every stage runs even after an earlier fault, each settles through one polymorphic `_settled` (a rail-returning sync owner passes through, an async owner converts under its named fence), and `traversed(..., ACCUMULATE)` combines every stage fault into one aggregate. One shutdown receipt emits through `Signals.emit_async` and the exit code folds the accumulated rail: `Ok`→`0`, `Error`→`1`, the traceback never escaping the CLI boundary.
- Entry: the recipe command is the one-shot local bind of the `execution/recipe#RECIPE` owner — the `selector` admits a `RecipeName` member through the fenced `RecipeName(selector)` probe and falls back to the external recipe-folder path string, the optional `assignments` JSON document decodes once at the `boundary("config", ...)` fence into the spec's `Map[str, object]` inputs, the `Signals.configure`/`install(RetryMode.EMIT)` boot pair mirrors the serve leg so the engine-gate retries ride the one `RETRY_HOOKS` stack, and the outcome folds through the same `_exit` fold the serve command exits by — a rail folds `Ok`→`0`/`Error`→`1`, a batch `DrainReceipt` folds on its fault tally; the terminal-tier `serve -> lanes | recipe` imports are the module's only sibling edges.
- Packages: `cyclopts` (`App` with `result_action="return_int_as_exit_code_else_zero"` / `Parameter(env_var=)` / `types.NonNegativeFloat` the constrained grace), `anyio` (`create_task_group`/`CancelScope`/`open_signal_receiver`/`create_memory_object_stream` the supervision seam), `msgspec` (`json.decode(type=dict[str, object])` the one assignments-document fence), `reliability/faults#FAULT` (`railed` the boot builder, `traversed(ACCUMULATE)` the drain combine, `async_boundary`/`boundary` the stage fences), `observability/telemetry#TELEMETRY` (`Telemetry.install`/`Telemetry.shutdown` — the LAST drain stage so every earlier stage's spans and receipts still export), `observability/receipts#RECEIPT` (`Signals.configure` the structlog chain, `Receipt.of` + `Signals.emit_async` the shutdown receipt, `DrainReceipt` the batch exit arm), `reliability/resilience#RESILIENCE` (`install(RetryMode.EMIT)` the retry-hook registration), `execution/admission#CONTEXT` (`SettingsAdmission` the one settings read), `execution/lanes#LANE` (`LanePolicy` the recipe command's lane), `execution/recipe#RECIPE` (`RecipeExecution`/`RecipeSpec`/`RecipeName` the one-shot bind).
- Growth: a new private command is one `@app.command` method whose railed outcome folds through the shared `_exit` and the app's `result_action`; a new drainable owner is one `(subject, stage)` row on the `drains` Block — the ordered fold, the accumulate, and the receipt absorb it with zero new surface; a sibling daemon is one `companion_app(routes, drains)` call with its own rows.
- Boundary: never a new public command surface — public commands are reserved to the suite Assay command surface. The deleted forms: a fire-and-forget stage sequence or an unowned drain ORDER across the drainable owners where the one `_drained` fold owns it; a first-fault abort leaving later stages undrained where every stage settles and the faults accumulate; a telemetry flush before the stages it must export; a hand-parsed `argv` or a hand-rolled validator the type library owns; an unrailed exception escaping the CLI boundary; a serve command that binds without the `aligned()` drift gate.

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

# `ServerHost`/`CredentialPolicy`/`Route` are the [02]-[SERVE] owners of this same
# `rasm.runtime.serve` module — no cross-module import, one module's declaration regions.

# --- [TYPES] ----------------------------------------------------------------------------

type DrainStage = Callable[[], Awaitable[object] | RuntimeRail[object]]

# --- [OPERATIONS] -----------------------------------------------------------------------


def _exit(outcome: RuntimeRail[object] | DrainReceipt[object]) -> int:
    # the one CLI exit fold both commands share: a rail folds Ok->0/Error->1, a batch
    # receipt folds on its fault tally; the traceback never escapes the CLI boundary.
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
    # one polymorphic stage settle: the stage() call itself is fenced so a synchronous raise
    # converts instead of escaping the drain fold; a rail-returning sync owner (Telemetry.shutdown)
    # passes through, an async owner awaits under the same named fence.
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
        # the serve leg is fenced: a platform raise from the bind/start/health-flip calls lands on
        # the rail instead of killing the group as an unconverted ExceptionGroup, so the trip
        # cancel below ALWAYS runs and the daemon never hangs on a signal that will not come.
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
        # one-shot local execution binding the execution/recipe#RECIPE owner: a RecipeName member
        # or an external recipe-folder path, input assignments one JSON document, the exit code the
        # rail; the boot pair mirrors the serve leg so the engine-gate retries ride RETRY_HOOKS.
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
