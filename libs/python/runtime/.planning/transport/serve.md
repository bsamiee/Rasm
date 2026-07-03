# [PY_RUNTIME_SERVE]

The companion server-host owns the inbound serve, the credential admission, and the descriptor-driven invoke. It composes the `transport/wire#PROTO_TRANSCODE` codec, the `clock#CLOCK` causal time, and the `execution/admission#CONTEXT` context and re-mints none.

- `ServerHost` owns the `grpc.aio` server lifecycle (bind, interceptor injection, `grpc.Compression` selection, graceful drain) plus the one `dispatch` servicer-body aspect every generated stub method folds through: inbound admit, the parent-span `Signals.attach` scope, the `WireProtoCodec` decode → railed `handler` → encode `bind`-chain, and the one terminal `settle` fold. A servicer method is data (descriptor id, codec pair, railed handler), never a hand-written admit/transcode/abort prologue. It hosts the geometry companion daemon over the existing C# `ComputeService`/`DocumentService`/`ControlService`/`ArtifactSync` gRPC contract on the UDS leg.
- `Credential` is the Python half of the C# `CredentialPolicy` axis, decoded against the five producer rows and resolved to the one credential the UDS serve leg admits: `insecure_loopback`, peer identity read by the kernel at accept, never a PEM bundle.
- `CapabilityInvoke` consumes the C#-generated capability-descriptor SDK, deriving the companion command surface from the descriptor catalog and wrapping its outbound channel with the catalogue client interceptors.
- `Entrypoint` is the type-hint-driven `cyclopts` command grammar that launches the daemon, a private entry only.

The wire vocabulary the inbound servicers and the outbound dispatch carry is the `transport/wire#PROTO_TRANSCODE`/`#CRDT_DECODE` codec owner.

## [01]-[INDEX]

- [01]-[SERVE]: the inbound `grpc.aio` server-host lifecycle, the decoded credential axis, the OTel span interceptor, the one polymorphic servicer-body dispatch aspect (admit → attach → `bind`-chain transcode → one `settle` enrich/abort fold), the `Map[FaultTag, grpc.StatusCode]` projection, and the inbound causal-frame decode on the Result rail.
- [02]-[CAPABILITY_INVOKE]: the descriptor-driven polymorphic invoke decoding the C# capability SDK and wrapping the outbound channel with the catalogue client interceptors.
- [03]-[ENTRY]: the private companion entrypoint grammar.

## [02]-[SERVE]

- Owner: `ServerHost` — the boundary capsule over one `grpc.aio` server hosting servicers generated from the C# proto descriptors. It owns the bind, the construction-time interceptor and compression injection, the inbound carrier decode, the one `dispatch` servicer-body aspect (admit → attach → decode/handler/encode `bind`-chain → one `settle` enrich/abort fold), the SERVER-span enrichment, the `Map[FaultTag, grpc.StatusCode]` fault projection, and the graceful drain. `Credential` is the tagged union decoding the C# `CredentialPolicy` five-row axis to the one credential the UDS serve leg admits. `ServerHost` composes four owners and re-mints none: the `transport/wire#PROTO_TRANSCODE` `WireProtoCodec` transcodes the servicer wire shapes; the `clock#CLOCK` `CausalFrame.decode(carrier)` railed reader owns the `SLOTS`-keyed parse and the one wire fence over the inbound `(Hlc, Tenant)` frame; the `execution/admission#CONTEXT` `RuntimeContext` carries the threaded context and its `attribute()` span projection; the `observability/receipts#RECEIPT` `Signals` owns the inbound parent-span extract and the `attach` scope.
- Cases: the C# `CredentialPolicy` mints five rows (`insecure_loopback`, `tls`, `mtls`, `bearer`, `composed`); the decode admits one constructible serve-side case. The companion serves the local control plane over `RemoteTransport.UnixDomainSocket`, whose `Credentials` column is `Seq(CredentialPolicy.InsecureLoopback)` exactly, so `Credential(insecure_loopback=True)` is the only server credential and peer identity is the kernel-reported `(pid, uid)` the C# `AppHost/companion-sidecar#PEER_ADMISSION` reads at accept through `SO_PEERCRED`/`LOCAL_PEERCRED`, never a wire-carried PEM. The other four rows are the outbound client legs the C# `Rasm.AppHost` dials, carrying PEM/token material the companion never serves, so the decode names them as the deleted serve-side forms. `Credential.server_credentials` is total over the five producer rows by `match`/`assert_never`: the four outbound rows return `Error(BoundaryFault(config=...))` on the `RuntimeRail`, so a row the UDS serve leg cannot admit is a typed construction failure, never a bare `ValueError` and never a phantom PEM bundle.
- Entry: `ServerHost.dispatch[S: Struct, M: Message, R: Struct, N: Message]` is the one polymorphic servicer-body aspect every generated stub method folds through — four-type-parameterized so it discriminates by input AND output shape, owning the inbound admission, the `observability/receipts#RECEIPT` `Signals.attach` parent-span scope, and the two-stage rail that lands the request on one `RuntimeRail[bytes]`. A servicer method is the descriptor id plus the codec pair plus the railed `handler`, never a hand-written admit/transcode/abort prologue. The method roster:

  | [METHOD]                                                  | [CONTRACT]                                                                                                                                                                                                                                                                                                            |
  | :-------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
  | `inbound(ctx) -> RuntimeRail[(Context, RuntimeContext)]`  | decodes the metadata carrier: the `Signals.continue_inbound` W3C extract runs OUTSIDE the wire fence over the whole carrier (a non-failing parent resolve the propagator keys itself, never a parse fault), then the `clock#CLOCK` `CausalFrame.decode(carrier)` railed reader owns the one `boundary("wire", ...)` fence and the `SLOTS`-keyed `convert` decode, so a malformed frame lands as `Error(BoundaryFault(boundary=("wire", "ValidationError")))` before any span scope opens — `inbound` re-spells no `rasm-hlc-*` header literal and opens no second fence, the clock owner being the sole decode site. |
  | `_invoke(codec, request, ctx, handler) -> RuntimeRail[bytes]` | the `WireProtoCodec` decode → railed `handler` → encode stage: synchronous `decode.decode(request)` short-circuits `Error` directly, the `Ok(shape)` arm `await`s `handler(shape, ctx)` then `.bind(encode.encode)` so the handler and encode faults both ride the one rail — the await being the single coroutine point a pure `Result.bind` chain cannot thread, so the decode discriminant is a three-arm `match` and the encode rides `.bind`, never a four-level nested ladder. |
  | `settle(ctx, sctx, descriptor, wired) -> bytes`           | the single rail-terminating fold every chain hands its `RuntimeRail[bytes]`: `Ok(payload)` calls `enrich(ctx, descriptor, "ok")` and returns the wire bytes; `Error(fault)` calls `enrich(ctx, descriptor, fault.tag)` then aborts through the `_FAULT_STATUS` projection — the inbound-admit, decode, handler, and encode faults all converge on this one enrich-and-abort fold rather than the three inline `enrich(...); abort(...)` repetitions the prior nested-match dispatch spread across its arms. |
  | `serve() -> RuntimeRail[None]`                            | binds the UDS socket through the railed `Credential.server_credentials` fold, starts the server, and `await`s termination directly — the single-await needs no `anyio` task group (the failure boundary for concurrent children, not a one-coroutine wrapper). |
  | `drain() -> None`                                         | calls `server.stop(grace)` in the host drain choreography so in-flight calls complete within the grace period. |
  | `Credential.server_credentials() -> RuntimeRail[ServerCredentials]` | folds the loopback case to `grpc.local_server_credentials(grpc.LocalConnectionType.UDS)` on the `Ok` rail so the UDS peer authenticates by socket locality, never a key-chain pair; `Credential.loopback()` is the zero-argument constructor the serve default and the entrypoint mint rather than re-spelling the `insecure_loopback=True` kwarg per call site. |
- Auto: every `BoundaryFault` tag projects to a `grpc.StatusCode` through the `_FAULT_STATUS` `Map[FaultTag, grpc.StatusCode]` keyed on the closed `reliability/faults#FAULT` `FaultTag` literal. A domain `Error(BoundaryFault)` converts in the `ServerHost.settle` `Error` arm: `_FAULT_STATUS.try_find(fault.tag).default_value(grpc.StatusCode.INTERNAL)` resolves the code and the abort detail joins off the union's own total `BoundaryFault.facts()` `dict[str, object]` slot map (the payload slot being `subject`/`detail`/`code`/`budget`/`members` per the case). The fault tag IS the status key, so the table carries no redundant case-label column, never a per-arm `if` ladder, never a `(code, label)` tuple value duplicating the key, never a hand-indexed `facts()["detail"]` that panics on the slot-divergent `wire`/`deadline`/`aggregate` arms, and never a bare exception across the wire.
- Auto: the generated stubs arrive from `grpcio-tools` compiling the C# `.proto` descriptors and register through the `grpc.{unary_unary,...}_rpc_method_handler` factory family with raw-bytes (de)serializers, each method body delegating to the one `ServerHost.dispatch` aspect so the request reaches `dispatch` as the `bytes` the `transport/wire#PROTO_TRANSCODE` `WireProtoCodec` decodes (the handler-law generated path, never a hand-wired servicer). The `grpc.local_server_credentials(grpc.LocalConnectionType.UDS)` server credential is the grpc-native loopback authenticator the C# `ChannelCredentials.Insecure` UDS dial pairs with at the same locality boundary. The inbound `(hlc_physical, hlc_logical, tenant)` slot lift is the `clock#CLOCK` `CausalFrame.decode(carrier)` reader's sole `SLOTS`-keyed parse, so a malformed frame aborts `INVALID_ARGUMENT` before the span scope opens — the `inbound` method-roster row owns the fence contract.
- Auto: the serve leg admits one `aio_server_interceptor(filter_=filters.negate(filters.health_check()))` into `grpc.aio.server(interceptors=[...])`, wired once at construction, so every RPC emits a `SERVER` span without a hand-rolled interceptor and the filter suppresses the `grpc.health.v1.Health` liveness noise; the `TracerProvider` arrives installed at `observability/telemetry#TELEMETRY`. Compression rides the `grpc.Compression` member passed at construction, the runtime selecting the algorithm rather than re-implementing the C-core codec.
- Auto: the inbound trace-context extract rides the servicer entry. `ServerHost.inbound` reads the carrier off `grpc.aio.ServicerContext.invocation_metadata()` projected to `dict[str, str]` and calls `observability/receipts#RECEIPT` `Signals.continue_inbound`; `ServerHost.dispatch` then opens the `Signals.attach(parent)` scope around the decode/handler/encode body so the companion span seeds from the C#-minted parent rather than a fresh root. The `attach`/`detach` token pair is the receipts owner's, composed at the one servicer hop and never re-minted.
- Auto: the SERVER-span enrichment is a span-attribute enricher composed after the catalogue `aio_server_interceptor` opens the span. `ServerHost.enrich` calls `trace.get_current_span().set_attributes(context.attribute() | {"rasm.descriptor": descriptor, "rasm.fault_case": fault_case})` inside the active scope, where `fault_case` is `"ok"` on the success arm or the `BoundaryFault.tag` on the fault arm (the same closed `FaultTag` the `_FAULT_STATUS` `Map` keys, never a re-derived label). It folds the `execution/admission#CONTEXT` `RuntimeContext.attribute()` `rasm.tenant`/`rasm.hlc` projection with the descriptor and fault-case dimensions in one `set_attributes` call, so a flame graph shows the descriptor, tenant, HLC, and resolved fault case per RPC. This is `set_attributes`, not a hook param on `aio_server_interceptor` (which takes none) and not a hand-rolled tracing interceptor (the catalogue Reject row forbids it); the admission owner already mints the `rasm.tenant`/`rasm.hlc` keys from the causal frame, so the serve leg composes them rather than re-spelling.
- Growth: a new servicer method is one generated stub delegating to the existing `ServerHost.dispatch` with its descriptor id, codec pair, and railed handler — never a new admit/transcode/abort prologue; a new wire message is one `transport/wire#PROTO_TRANSCODE` `WireProtoCodec` type pair the `dispatch` four-type signature already accepts; a new capability is one descriptor row the `[03]-[CAPABILITY_INVOKE]` catalog already discriminates; a new enriched server-span dimension is one key in the `enrich` `set_attributes` union; a new fault-to-status mapping is one `_FAULT_STATUS` `Map` row keyed by the new `FaultTag`; a new compression algorithm is one `grpc.Compression` member at construction; zero new surface, no second RPC server.
- Boundary: the wire contract is the existing C# `ComputeService`/`DocumentService`/`ControlService`/`ArtifactSync` proto; the runtime mints no transport, no channel, and no second wire vocabulary. The C# host lifecycle, global health, and product telemetry export stay AppHost-owned; the `Hlc`/`ElementId`/`Tenant`/`CausalFrame` stamp lives in `clock#CLOCK` and the codec in `transport/wire`, both consumed here and re-minted nowhere. The deleted forms:
  - a hand-rolled message loop, a divergent message shape, a second RPC server, or a per-servicer admit/transcode/abort prologue duplicated across the four contracts rather than folded through the one `dispatch` aspect;
  - a four-level nested `match` ladder over the decode/handler/encode rail where one `Result.bind` chain plus the one `settle` fold owns the flow;
  - three inline `enrich(...); abort(...)` repetitions across the decode-error/handler-error/encode-error arms where the one `settle` fold folds the fault path once;
  - a `(grpc.StatusCode, str)` tuple value on the fault table whose second element re-spells the `FaultTag` key, a stringly `dict[str, ...]` table where the `Map[FaultTag, grpc.StatusCode]` keys the closed literal, or a per-arm `if` ladder mapping fault to status where the `Map` projects;
  - an erased `object` inbound pair where the OTel `Context` is typed, a bare exception across the wire, or a raw `int()` parse of the inbound stamp that panics rather than railing through the `wire` boundary;
  - an inline `int(carrier.get("rasm-hlc-physical", ...))` parse with re-spelled `rasm-hlc-*`/`rasm-tenant` header literals and a second `boundary("wire", ...)` fence in `inbound` where `clock#CLOCK` `CausalFrame.decode(carrier)` owns the `SLOTS`-keyed parse and the sole fence;
  - a key-chain server credential on the UDS leg, a fresh-root span where the C# parent is on the inbound carrier, and a `request_hook`/`response_hook` on the SERVER interceptor (the admitted `aio_server_interceptor(tracer_provider, filter_)` takes no hook params — server enrichment is `set_attributes` composing `RuntimeContext.attribute()`, the hook law scoping hooks to CLIENT spans).

```python signature
from collections.abc import Awaitable, Callable
from typing import Final, Literal, Self, assert_never

import grpc
from expression import Error, Ok, case, tag, tagged_union
from expression.collections import Map
from google.protobuf.message import Message
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.context import Context
from opentelemetry.instrumentation.grpc import aio_server_interceptor, filters

from rasm.runtime.admission import RuntimeContext, RuntimeProfile
from rasm.runtime.clock import CausalFrame
from rasm.runtime.faults import BoundaryFault, FaultTag, RuntimeRail
from rasm.runtime.receipts import Signals
from rasm.runtime.wire import WireProtoCodec

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class Credential:
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
            case Credential(tag="insecure_loopback"):
                return Ok(grpc.local_server_credentials(grpc.LocalConnectionType.UDS))
            case Credential(tag="tls" | "mtls" | "bearer" | "composed" as outbound):
                return Error(BoundaryFault(config=(outbound, "outbound C# client credential; the companion serves insecure_loopback over UDS only")))
            case _ as unreachable:
                assert_never(unreachable)


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
        self, bind: str, credential: Credential | None = None, grace: float = 5.0, compression: grpc.Compression = grpc.Compression.Gzip
    ) -> None:
        self._bind, self._credential, self._grace = bind, credential or Credential.loopback(), grace
        interceptor = aio_server_interceptor(filter_=filters.negate(filters.health_check()))
        self._server: grpc.aio.Server = grpc.aio.server(interceptors=[interceptor], compression=compression)

    @staticmethod
    def inbound(servicer_context: grpc.aio.ServicerContext) -> RuntimeRail[tuple[Context, RuntimeContext]]:
        # parent resolve is non-failing and stays OUTSIDE the fence; the `clock#CLOCK` `CausalFrame.decode`
        # owns the sole `boundary("wire", ...)` fence and the `SLOTS`-keyed parse, so no slot literal lives here.
        carrier = dict(servicer_context.invocation_metadata())
        parent = Signals.continue_inbound(carrier)
        return CausalFrame.decode(carrier).map(lambda causal: (parent, RuntimeContext.admit(RuntimeProfile.SIDECAR, causal=causal)))

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
                code = _FAULT_STATUS.try_find(fault.tag).default_value(grpc.StatusCode.INTERNAL)
                await servicer_context.abort(code, ":".join(f"{k}={v}" for k, v in fault.facts().items()))
                return b""
            case _ as unreachable:
                assert_never(unreachable)

    @staticmethod
    async def _invoke[S: Struct, M: Message, R: Struct, N: Message](
        codec: tuple[WireProtoCodec[S, M], WireProtoCodec[R, N]],
        request: bytes,
        context: RuntimeContext,
        handler: Callable[[S, RuntimeContext], Awaitable[RuntimeRail[R]]],
    ) -> RuntimeRail[bytes]:
        decode, encode = codec
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
        codec: tuple[WireProtoCodec[S, M], WireProtoCodec[R, N]],
        handler: Callable[[S, RuntimeContext], Awaitable[RuntimeRail[R]]],
    ) -> bytes:
        match ServerHost.inbound(servicer_context):
            case Error(fault):
                return await ServerHost.settle(servicer_context, RuntimeContext.admit(RuntimeProfile.SIDECAR), descriptor, Error(fault))
            case Ok((parent, context)):
                with Signals.attach(parent):
                    wired = await ServerHost._invoke(codec, request, context, handler)
                    return await ServerHost.settle(servicer_context, context, descriptor, wired)
            case _ as unreachable:
                assert_never(unreachable)

    async def serve(self) -> RuntimeRail[None]:
        match self._credential.server_credentials():
            case Error(_) as err:
                return err
            case Ok(creds):
                self._server.add_secure_port(self._bind, creds)
                await self._server.start()
                await self._server.wait_for_termination()
                return Ok(None)
            case _ as unreachable:
                assert_never(unreachable)

    async def drain(self) -> None:
        await self._server.stop(self._grace)
```

## [03]-[CAPABILITY_INVOKE]

- Owner: `CapabilityInvoke[S: Struct, M: Message, R: Struct, N: Message]` — the descriptor-driven polymorphic invoke decoding the C# `Rasm.AppHost/Agent/capability#SDK_CODEGEN` Python target, four-type-parameterized so the one invoke discriminates by input AND output shape. `DiscoveryResult` is the catalog row ingested from the C# `DiscoveryResultWire[]` projection, carrying the per-descriptor `input_schema` as a deferred-decode `msgspec.Raw` slot; `CommandReceipt` is the per-command evidence read back through the C# `ReceiptEnvelopeWire<CapabilityCommandReceiptWire>`. One invoke keyed by descriptor id discriminates every capability through the frozen `Map[str, DiscoveryResult]` catalog and its `try_find` lookup rail, never a per-service hand client and never an imperative membership guard. The command request/receipt projection rides the injected `transport/wire#PROTO_TRANSCODE` `WireProtoCodec` pair, minting no fourth wire shape.
- Owner: the outbound dispatch wraps its channel with `aio_client_interceptors(tracer_provider, filter_, request_hook, response_hook)` so the call propagates the active span through the `observability/telemetry#TELEMETRY`-installed composite and the CLIENT-span hooks enrich the `clock#CLOCK` `Tenant` and the fault case; `run` sets the per-call `rasm.descriptor` attribute (the descriptor is per call, the tenant channel-stable). The call delegates to `reliability/resilience#RESILIENCE` `guarded(RetryClass.WIRE, method, request, subject="wire")`, which fuses the wire-class retry, the derivation span, and the one `async_boundary` terminal-fault lift into one `RuntimeRail[bytes]`, so a `grpc.aio.AioRpcError` lands as the `boundary`-tagged fault the `CLASSIFY` fold names — never a bare gRPC exception escaping the dispatch and never a manual `async_boundary`+bare `guard` triplet re-spelling the resilience envelope.
- Cases: the C# SDK Python target emits one method per descriptor — `def {surface}_{op}(self, request: S) -> RuntimeRail[R]: return self.run("{surface}.{op}", request)` — so the companion decodes the catalog into one `run(descriptor_id, request)` dispatch, `descriptor_id` the `{surface}.{op}` join the C# `CapabilityDescriptor.Of` mints. The per-descriptor `input_schema` is the `JsonSchemaExporter` JSON Schema the C# `SuiteContracts.Schema` derives from the descriptor's `CommandArguments`, carried on the `DiscoveryResult` as a deferred `msgspec.Raw` slot so the routing decode never pays the schema-document parse and the schema rides as discovery-time contract evidence. The argument payload is the already-typed canonical `Struct S` the `WireProtoCodec.encode` transcodes, not a hand-mirrored mapping re-validated against a JSON Schema document: `msgspec` validates a payload against a `Struct` type at decode, not against a schema document, so the schema is carried evidence rather than a per-call validator the wire codec already subsumes. The command-txn disposition decodes onto the `CommandTxn` struct whose `kind: CommandTxnKind` is the literal-discriminated disposition the C# `CommandTxn` mints (`committed`, `rolled_back`, `compensated`, `refused`) carried with its arm `detail`, the `CapabilityCommandReceiptWire.txn` kind raised onto the typed struct rather than an erased `dict`.
- Entry: `CapabilityInvoke.run` is one `Result`-railed `lookup → encode → dispatch → decode` chain. It keys the descriptor through `self._catalog.try_find(descriptor_id).to_result(BoundaryFault(wire=(descriptor_id, 0)))` — the `expression.collections.Map` `Option`-to-`Result` lift the `transport/wire#PROTO_TRANSCODE` `codec(name)` registry miss already uses, an absent id a typed code-carrying `wire` fault rather than an `if descriptor_id not in ...` guard — `bind`s the canonical-request encode, sets the per-call `rasm.descriptor` span attribute on the resolved-and-encoded arm, dispatches the wire bytes, and `bind`s the response decode back to the canonical `Struct R`. The absent-id, encode, dispatch, and decode faults all ride the one `RuntimeRail`, the await on the async `dispatch` the single coroutine point between the sync `lookup.bind(encode)` head and the `bind(decode)` tail.
- Entry: `CapabilityInvoke.discover` reads the catalog the C# `CapabilityRegistry.Discover(DiscoveryQuery.All)` emits through `msgspec.json.Decoder(list[DiscoveryResult])` on the one `boundary("wire", ...)` fence, then `.map`s the decoded rows into the descriptor-keyed `Map[str, DiscoveryResult]` `connect`/`__init__` consume — the keying fold lives at the one decode site rather than a `list`-to-`Map` re-key the composition root hand-spells, so the command surface is generated from the descriptor rows, never hand-listed.
- Entry: `CapabilityInvoke.connect` is the single channel composer. It mints the one outbound `grpc.aio.insecure_channel` (loopback UDS) wrapped with the `interceptors(tenant)` factory and folds the traced channel into the railed `dispatch` coroutine `run` awaits; `dispatch` delegates the raw-bytes `channel.unary_unary` multicallable to `guarded(RetryClass.WIRE, method, request, subject="wire")`, returning one `RuntimeRail[bytes]` retried under the wire envelope rather than an unrailed coroutine through an opaque callable or a manual `async_boundary`+bare `guard` triplet. The channel is the runtime-lived multicallable pool shared across every `run`, not a per-call session — the invoke owns its deterministic `grpc.aio.Channel.close` drain through `aclose`, the same runtime-lived-client-with-one-drain shape `transport/roots#RESOURCE` gives the pooled `httpx.AsyncClient`, never a per-`run` `async with insecure_channel(...)` reopen nor a leaked channel across the event loop. A directly-injected `WireDispatch` (the test/sidecar-composition path) carries no channel, so `aclose` is a typed no-op rather than a `None`-attribute panic.
- Entry: `CapabilityInvoke.interceptors` is the channel-stable factory `connect` consumes. It folds the catalogue `aio_client_interceptors` factory with the channel-stable `Tenant` `request_hook` (receiving `(span, request)`) and the fault-case `response_hook` (receiving `(span, details)`, the gRPC status-details string the interceptor awaits off the unary call: `""` on OK, the server's detail message on a non-OK status), both CLIENT-side per the catalogue hook law; the per-descriptor dimension is set by `run` on the active span rather than baked into a channel-lifetime interceptor.
- Auto: the descriptor catalog crosses the wire as the `DiscoveryResultWire[]` array the C# `#TS_PROJECTION` projects (`descriptor`, `surface`, `effect`, `idempotency`, `estimated`, `scope_hash`), decoded through `msgspec.json.Decoder(list[DiscoveryResult])`. The `effect`/`idempotency`/`cost-unit` keys decode as the C# smart-enum string keys (`pure`/`read`/`write`/`external`/`irreversible`, `idempotent`/`keyed`/`single-shot`/`non-idempotent`, `cpu-millis`/`wall-millis`/`bytes-egress`/`model-tokens`/`calls`), the cost vector as the unit-keyed mapping the C# `CostVectorWire` emits; the catalog being the single descriptor source, a new row regenerates the command surface with no per-capability hand edit.
- Auto: the channel-stable client hooks set `rasm.tenant`/`rasm.fault_case` on the CLIENT span through `Span.set_attribute` inside the active scope, never blocking, while `run` sets the per-call `rasm.descriptor`. The factory's `tracer_provider` defaults to `None`, resolving the global provider the `observability/telemetry#TELEMETRY` `set_tracer_provider` install registers, so no second tracer mints. The outbound `dispatch` delegates to `guarded(RetryClass.WIRE, method, request, subject="wire")`, so a non-OK status's `grpc.aio.AioRpcError` lifts through the one fused `async_boundary` to `Error(BoundaryFault(boundary=("wire", "AioRpcError")))` (the `CLASSIFY` unclassified-raise row), the transient-code retry the resilience owner's `RetryClass.WIRE` row admits and the terminal fault both riding the one rail rather than a bare `except` or a hand-rolled `async_boundary`+`guard` triplet.
- Packages: `msgspec` (`json.Decoder` PUBLIC_TYPES [02] for the catalog decode / `Struct` PUBLIC_TYPES [01] the request/receipt shapes / `Raw` PUBLIC_TYPES [02] the deferred `input_schema` slot / `ValidationError` PUBLIC_TYPES [03] — resident `.api/msgspec.md`), `protobuf` (the `transport/wire#PROTO_TRANSCODE`-paired command request/receipt `Message` the `WireProtoCodec` pair is generic over), `opentelemetry-instrumentation-grpc` (`aio_client_interceptors(tracer_provider, filter_, request_hook, response_hook)` interceptor factories, `filters` predicate algebra), `opentelemetry-api` (`trace.get_current_span`/`Span.set_attribute` for the hook and per-call descriptor enrichment — branch catalog `libs/python/.api/opentelemetry-api.md` trace ENTRYPOINTS [04]/[11], the trace-span members the resident `.api/opentelemetry-api.md` metrics/context scope does not carry), `grpcio` (`grpc.aio.insecure_channel` connect ENTRYPOINTS [02] for the loopback channel `connect` composes / `grpc.aio.Channel` PUBLIC_TYPES [02] the runtime-lived multicallable pool `aclose` drains through `grpc.aio.Channel.close` / `grpc.aio.AioRpcError` PUBLIC_TYPES [01] the `dispatch` lifts), `expression` (`Error`/`Ok` core algebraic types [02]/[03]; `Result.bind`/`Result.map` result operations [02]/[01] the `lookup → encode → dispatch → decode` chain folds and the `discover` row-keying `.map`; `expression.collections.Map` persistent collections [02] with `Map.of_seq` constructor [06] the descriptor-keyed catalog fold, `Map.try_find` lookup [08] and `Option.to_result` conversion [06] the catalog read — the `transport/wire#PROTO_TRANSCODE` `codec(name)` registry-miss rail; branch catalog `libs/python/.api/expression.md`), `reliability/resilience#RESILIENCE` (`guarded(RetryClass.WIRE, method, request, subject="wire")` — the retried-traced-railed outbound dispatch the `connect` `dispatch` delegates to; the wire-class retry, the derivation span, and the terminal `async_boundary` `AioRpcError` lift are all the resilience owner's, never re-spelled here), `reliability/faults#FAULT` (`BoundaryFault`/`RuntimeRail`/`boundary` the `discover` fence and the `run` absent-id `wire` fault read).
- Growth: a new capability is one descriptor row the C# registry already folds — the companion reads it through the existing `discover`/`run` pair, never a new method; a new effect/idempotency/cost key is one literal the decode union already discriminates; a new enriched span dimension is one `set_attribute` key in the existing hook, never a second interceptor; zero new surface, no per-service hand client.
- Boundary: the descriptor is the suite's only op-metadata owner and the C# registry its sole mint; the companion re-authors no capability shape. The command dispatch lands on the gRPC stub through the injected `transport/wire#PROTO_TRANSCODE` `WireProtoCodec` pair and retries under `guarded(RetryClass.WIRE, ...)`, so the invoke rides the existing proto wire and mints no fourth wire shape; the `Tenant` partition reads the one `clock#CLOCK` owner and re-mints no tenant scheme. The cross-language shape-identity is the C# `SuiteContracts.Schema` JSON Schema the C#, TS, and Python SDKs all bind, so an additive descriptor field admits on every consumer and a breaking reshape rejects on all three through the one `ContractGuard.AdditiveOnly` classifier. The deleted forms:
  - a per-capability hand client, a re-authored descriptor shape, or a hand-mirrored argument schema validated against a JSON Schema document `msgspec` cannot check (it validates a payload against a `Struct` type, not a schema document);
  - a decode-discard-re-encode of the whole `CommandArguments` bypassing the wire codec, or a Python `CommandArguments(payload, tenant, correlation)` struct re-mirroring the C# argument record where the request IS the canonical `Struct S` the `WireProtoCodec.encode` transcodes and the tenant rides the channel-stable interceptor;
  - an empty-descriptor channel-lifetime interceptor stamping `rasm.descriptor=""` for every call, a hand-rolled tracing interceptor (the catalogue Reject row), or a second tracer;
  - an unrailed `grpc.aio.AioRpcError` escaping the dispatch, or a manual `async_boundary("wire", lambda: guard(RetryClass.WIRE)(method, request), catch=AioRpcError)` triplet where the one `guarded` call fuses the retry/span/lift;
  - a per-`run` `async with grpc.aio.insecure_channel(...)` reopen rebuilding the channel and its interceptor stack per call, or a leaked channel with no drain, where the runtime-lived channel is `connect`-minted once and `aclose` drains it through `grpc.aio.Channel.close` exactly as `transport/roots#RESOURCE` drains its pooled `httpx.AsyncClient`;
  - a dual-purpose `_catalog` name binding both the class-level `json.Decoder` and the instance-level lookup `Map` where the decoder is the distinct `_DISCOVERY` class constant, an `if descriptor_id not in self._catalog` membership guard where `Map.try_find(...).to_result(...)` lifts the absent id, a `dict[str, DiscoveryResult]` catalog where the persistent `Map` carries the lookup rail, or a second cost table.

```python signature
from collections.abc import Awaitable, Callable
from typing import Literal, Self, assert_never

import grpc
import msgspec
from expression import Error, Ok
from expression.collections import Map
from google.protobuf.message import Message
from msgspec import Raw, Struct
from opentelemetry import trace
from opentelemetry.instrumentation.grpc import aio_client_interceptors, filters

from rasm.runtime.clock import Tenant
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary
from rasm.runtime.resilience import RetryClass, guarded
from rasm.runtime.wire import WireProtoCodec

# --- [TYPES] ----------------------------------------------------------------------------

type CostUnitKey = Literal["cpu-millis", "wall-millis", "bytes-egress", "model-tokens", "calls"]
type CostVector = dict[CostUnitKey, int]
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
    estimated: CostVector
    scope_hash: str
    input_schema: Raw = Raw(b"{}")


class CommandReceipt(Struct, frozen=True, rename="camel"):
    descriptor: str
    txn: CommandTxn
    charged: CostVector
    elapsed: str
    correlation: str


# --- [SERVICES] -------------------------------------------------------------------------


class CapabilityInvoke[S: Struct, M: Message, R: Struct, N: Message]:
    # distinctly named so the instance-level lookup `Map` `run` reads never shadows the class decoder.
    _DISCOVERY: msgspec.json.Decoder[list[DiscoveryResult]] = msgspec.json.Decoder(list[DiscoveryResult])

    def __init__(
        self,
        catalog: Map[str, DiscoveryResult],
        codec: tuple[WireProtoCodec[S, M], WireProtoCodec[R, N]],
        dispatch: WireDispatch,
        channel: grpc.aio.Channel | None = None,
    ) -> None:
        self._catalog, self._encode, self._decode, self._dispatch, self._channel = catalog, codec[0], codec[1], dispatch, channel

    @classmethod
    def discover(cls, payload: bytes) -> RuntimeRail[Map[str, DiscoveryResult]]:
        # the keyed catalog `connect`/`__init__` consume directly; the descriptor-keyed fold lives
        # at the one decode site, never a `list`-to-`Map` re-key the composition root hand-spells.
        return boundary("wire", lambda: cls._DISCOVERY.decode(payload)).map(lambda rows: Map.of_seq((row.descriptor, row) for row in rows))

    @staticmethod
    def interceptors(tenant: Tenant) -> list[grpc.aio.ClientInterceptor]:
        def request_hook(span: trace.Span, _request: object) -> None:
            span.set_attribute("rasm.tenant", str(tenant))

        def response_hook(span: trace.Span, details: str) -> None:
            span.set_attribute("rasm.fault_case", details or "ok")

        return aio_client_interceptors(filter_=filters.negate(filters.health_check()), request_hook=request_hook, response_hook=response_hook)

    @classmethod
    def connect(
        cls, target: str, catalog: Map[str, DiscoveryResult], codec: tuple[WireProtoCodec[S, M], WireProtoCodec[R, N]], tenant: Tenant
    ) -> Self:
        channel = grpc.aio.insecure_channel(target, interceptors=cls.interceptors(tenant))

        async def dispatch(descriptor_id: str, request: bytes) -> RuntimeRail[bytes]:
            # `guarded` fuses the `WIRE` retry, the derivation span, and the one `async_boundary`
            # `AioRpcError` lift — never a manual span-less `async_boundary`+bare `guard` triplet.
            method = channel.unary_unary(f"/rasm.capability/{descriptor_id}")
            return await guarded(RetryClass.WIRE, method, request, subject="wire")

        return cls(catalog, codec, dispatch, channel)

    async def run(self, descriptor_id: str, request: S) -> RuntimeRail[R]:
        encoded = self._catalog.try_find(descriptor_id).to_result(BoundaryFault(wire=(descriptor_id, 0))).bind(lambda _: self._encode.encode(request))
        match encoded:
            case Error(_) as err:
                return err
            case Ok(payload):
                trace.get_current_span().set_attribute("rasm.descriptor", descriptor_id)
                return (await self._dispatch(descriptor_id, payload)).bind(self._decode.decode)
            case _ as unreachable:
                assert_never(unreachable)

    async def aclose(self) -> None:
        # the invoke owns the runtime-lived channel's deterministic drain, mirroring `ServerHost.drain`;
        # a directly-injected dispatch (tests) carries no channel and the drain is a no-op.
        if self._channel is not None:
            await self._channel.close()
```

## [04]-[ENTRY]

- Owner: `Entrypoint` — the type-hint-driven `cyclopts` command axis backing the companion daemon's private entry and package-internal entrypoints only, co-located with `ServerHost` because `serve` composes the host it launches.
- Entry: `companion_app` returns the `cyclopts.App` whose commands bind to the companion serve and drain. Arguments bind from type hints with the `cyclopts.types.NonNegativeFloat` constrained grace and `Parameter(env_var=...)` env binding, never a hand-parsed `argv` or a hand-rolled single-value validator the type library already owns. The `result_action="return_int_as_exit_code_else_zero"` maps the railed outcome to the process exit: the serve command awaits the railed `ServerHost.serve` and folds `Ok`→`0`/`Error`→`1` through `map`/`default_value`, so a non-loopback credential surfaces as a typed fault collapsing to a non-zero exit, the command body never touching an unrailed exception and the traceback never escaping the CLI boundary.
- Packages: `cyclopts` (`App` PUBLIC_TYPES [01] with `result_action` ENTRYPOINTS [06] / `Parameter` PUBLIC_TYPES [02] env binding / `cyclopts.types.NonNegativeFloat` PUBLIC_TYPES [02] the constrained grace input).
- Growth: a new private command is one `@app.command` method on the existing app, its railed outcome folding to the exit code through the app's `result_action`; zero new surface.
- Boundary: never a new public command surface — public commands are reserved to the suite Assay command surface; a public-facing CLI axis and a hand-parsed argument loop are the deleted forms.

```python signature
from typing import Annotated

from cyclopts import App, Parameter
from cyclopts.types import NonNegativeFloat

# --- [ENTRY] ----------------------------------------------------------------------------


def companion_app() -> App:
    app = App(name="rasm-companion", help="private companion daemon entry", result_action="return_int_as_exit_code_else_zero")

    @app.command
    async def serve(bind: str, *, grace: Annotated[NonNegativeFloat, Parameter(env_var="RASM_COMPANION_GRACE")] = 5.0) -> int:
        return (await ServerHost(bind, Credential.loopback(), grace).serve()).map(lambda _: 0).default_value(1)

    return app
```
