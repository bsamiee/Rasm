# [PY_RUNTIME_SERVE]

The inbound companion server-host owner, the credential axis, the descriptor-driven capability invoke, and the private daemon entrypoint. `ServerHost` owns the `grpc.aio` server lifecycle (bind, interceptor injection, compression, graceful drain) plus the one `dispatch` servicer-body aspect every generated stub method folds through — the inbound admit, the parent-span attach scope, the wire transcode, the span enrichment, and the fault-to-status abort woven once so a servicer method is data (descriptor id, codec pair, railed handler), not a hand-written prologue — hosting the geometry companion daemon that speaks the EXISTING C# `ComputeService`/`DocumentService`/`ControlService`/`ArtifactSync` gRPC contract over the UDS leg. `Credential` is the Python half of the C# `CredentialPolicy` axis, decoded against the five producer rows and resolved to the one credential the companion UDS serve leg admits — `insecure_loopback`, peer identity read by the kernel at accept, never a PEM bundle. `CapabilityInvoke` consumes the C#-generated capability-descriptor SDK, deriving the companion command surface from the descriptor catalog and wrapping its outbound channel with the catalogue client interceptors. `Entrypoint` is the type-hint-driven `cyclopts` command grammar that launches the daemon — a PRIVATE entry only. The wire vocabulary the inbound servicers and the outbound dispatch carry is the `transport/wire#PROTO_TRANSCODE`/`#CRDT_DECODE` codec owner; the host-minted causal time is the `clock#CLOCK` owner; the admission context is the `execution/admission#CONTEXT` owner. The package owns the companion's inbound serve, its credential admission, and its descriptor-driven invoke; it mints no wire vocabulary, no clock, and no context — it composes those owners and re-mints nothing.

## [01]-[INDEX]

- [01]-[SERVE]: the inbound `grpc.aio` server-host lifecycle, the decoded credential axis, the OTel span interceptor, the one polymorphic servicer-body dispatch aspect (admit → attach → transcode → enrich → abort), and the inbound causal-frame decode on the Result rail.
- [02]-[CAPABILITY_INVOKE]: the descriptor-driven polymorphic invoke decoding the C# capability SDK and wrapping the outbound channel with the catalogue client interceptors.
- [03]-[ENTRY]: the private companion entrypoint grammar.

## [02]-[SERVE]

- Owner: `ServerHost` — the boundary capsule over one `grpc.aio` server hosting servicers generated from the C# proto descriptors, owning the bind, the construction-time interceptor and compression injection, the inbound carrier decode, the one `dispatch` servicer-body aspect (admit → attach → transcode → enrich → abort), the SERVER-span enrichment, the fault-to-status projection, and the graceful drain; `Credential` the tagged union decoding the C# `CredentialPolicy` five-row axis to the one credential the companion UDS serve leg admits. The wire shapes the servicers transcode are the `transport/wire#PROTO_TRANSCODE` `WireProtoCodec` owner's; the host-minted `(Hlc, Tenant)` frame the inbound carrier carries is the `clock#CLOCK` `CausalFrame.of` owner's; the admission context the servicer threads and its `attribute()` span projection are the `execution/admission#CONTEXT` `RuntimeContext` owner's; the inbound parent-span extract and the `attach` scope are the `observability/receipts#RECEIPT` `Signals` owner's — `ServerHost` composes all four and re-mints none.
- Cases: the C# `CredentialPolicy` mints five rows — `insecure-loopback`, `tls`, `mtls`, `bearer`, `composed` — but the companion serves the local control plane over the UDS leg (`RemoteTransport.UnixDomainSocket`, whose `Credentials` column is `Seq(CredentialPolicy.InsecureLoopback)` exactly), so the decode admits one constructible serve-side case: `Credential` case `insecure_loopback=True` is the only server credential, and peer identity is the kernel-reported `(pid, uid)` the C# `AppHost/companion-sidecar#PEER_ADMISSION` reads at accept through `SO_PEERCRED`/`LOCAL_PEERCRED`, never a wire-carried PEM; `tls`/`mtls`/`bearer`/`composed` are the OUTBOUND client legs the C# `Rasm.AppHost` dials and carry PEM/token material the companion never serves, so the decode names them as the deleted serve-side forms rather than constructing a server credential from them. The `Credential.server_credentials` fold is total over the five producer rows by `match`/`assert_never` — the four outbound rows return `Error(BoundaryFault(config=...))` on the `RuntimeRail` rather than raising, so a producer row the UDS serve leg cannot admit is a typed construction failure on the rail, never a bare `ValueError` and never a phantom PEM bundle.
- Entry: `ServerHost.dispatch` is the one polymorphic servicer-body aspect every generated stub method folds through — parameterized over the request/response `[S: Struct, M: Message, R: Struct, N: Message]` four-type arguments so it discriminates by input AND output shape, it owns the inbound admission, the `observability/receipts#RECEIPT` `Signals.attach` parent-span scope, the `transport/wire#PROTO_TRANSCODE` `WireProtoCodec` decode/encode at the seam, the `enrich` span-attribute fold, and the fault-to-`ServicerContext.abort` `_FAULT_STATUS` projection, so a servicer method is the descriptor id plus the codec pair plus the railed `handler`, never a hand-written admit/transcode/abort prologue per RPC; `ServerHost.inbound` decodes the inbound metadata carrier into the `(Context, RuntimeContext)` pair — the W3C `traceparent`/`tracestate` extract through `Signals.continue_inbound` runs OUTSIDE the wire fence (it is the non-failing parent-context resolve, never a parse fault), and only the `rasm-hlc-physical`/`rasm-hlc-logical` two-half stamp parse plus the `clock#CLOCK` `CausalFrame.of` lift ride the one `reliability/faults#FAULT` `boundary("wire", ...)` conversion, so a malformed frame lands as `Error(BoundaryFault(boundary=("wire", <ExcType>)))` on the rail before `dispatch` opens the span scope, never a raw `int()` that panics across the servicer and never a trace-extract failure masquerading as a wire fault; `ServerHost.serve` binds the UDS socket through the railed credential fold, starts the server, and awaits termination directly (the single-await needs no `anyio` task group, which is the failure boundary for concurrent children, not a one-coroutine wrapper); `ServerHost.drain` calls `server.stop(grace)` participating in the host drain choreography so in-flight calls complete within the grace period; `Credential.server_credentials` folds the loopback case to `grpc.local_server_credentials(grpc.LocalConnectionType.UDS)` on the Ok rail so the UDS peer authenticates by socket locality, never a key-chain pair, and `Credential.loopback` is the zero-argument constructor the serve default and the entrypoint mint rather than re-spelling the `insecure_loopback=True` case kwarg at every call site.
- Auto: every `BoundaryFault` tag projects to a `grpc.StatusCode` through the one `_FAULT_STATUS` data table (`config`→`FAILED_PRECONDITION`, `resource`→`UNAVAILABLE`, `deadline`→`DEADLINE_EXCEEDED`, `wire`/`boundary`→`INVALID_ARGUMENT`, `import_`→`UNIMPLEMENTED`, `api`/`aggregate`→`INTERNAL`), so a domain `Error(BoundaryFault)` from `reliability/faults#FAULT` converts through `ServerHost.abort` calling `ServicerContext.abort(code, f"{case}:{fault}")` — a status-code projection keyed on the fault tag, never a per-arm `if` ladder and never a bare exception across the wire; the generated stubs arrive from `grpcio-tools` compiling the C# `.proto` descriptors and register through the `grpc.{unary_unary,...}_rpc_method_handler` factory family, each method body delegating to the one `ServerHost.dispatch` aspect so the request reaches `dispatch` as the raw `bytes` the `transport/wire#PROTO_TRANSCODE` `WireProtoCodec` decodes (the handler-law generated registration path, never a hand-wired servicer); the credential decode is total over the five producer rows by `match`/`assert_never`; the `LocalConnectionType.UDS` server credential is the grpc-native loopback authenticator the C# `ChannelCredentials.Insecure` UDS dial pairs with at the same locality boundary; the inbound `(hlc_physical, hlc_logical, tenant)` slot lifts through `clock#CLOCK` `CausalFrame.of` onto `RuntimeContext.causal` so the parse failure rides the rail rather than a servicer panic — `CausalFrame.of` receives validated ints and never sees a raw string, the string-to-int decode living inside the one `boundary("wire", ...)` fence in `inbound`, and `dispatch` consumes the railed `(Context, RuntimeContext)` so a malformed frame aborts with `INVALID_ARGUMENT` before the span scope opens rather than after a partial admit.
- Packages: `grpcio` (`grpc.aio.server` serve ENTRYPOINTS [01] / `grpc.local_server_credentials` credential ENTRYPOINTS [03] / `grpc.LocalConnectionType` credential PUBLIC_TYPES [03] / `grpc.ServerCredentials` credential PUBLIC_TYPES [01] / `grpc.StatusCode` PUBLIC_TYPES [01] / `grpc.Compression` PUBLIC_TYPES [02] / `grpc.aio.Server` PUBLIC_TYPES [01] with `add_secure_port`/`start`/`stop`/`wait_for_termination` lifecycle ENTRYPOINTS [01]/[03]/[04]/[05] / `grpc.aio.ServicerContext.abort`/`invocation_metadata` PUBLIC_TYPES [03] / `grpc.StatusCode` PUBLIC_TYPES [01] members the `_FAULT_STATUS` table projects / `grpc.{unary_unary,...}_rpc_method_handler` ENTRYPOINTS [05] + `add_generic_rpc_handlers` ENTRYPOINTS [06] the generated stubs register through), `grpcio-tools` (gated `python_version<'3.15'` codegen companion, stub regeneration only — never imported on the cp315 core), `protobuf` (the `transport/wire` codec's generated `_pb2` `Message` the `dispatch` codec pair is generic over), `opentelemetry-instrumentation-grpc` (`aio_server_interceptor`/`filters.health_check`/`filters.negate`), `opentelemetry-api` (`trace.get_current_span`/`Span.set_attributes` for the SERVER-span enrichment + `opentelemetry.context.Context` the inbound parent carries — branch catalog `libs/python/.api/opentelemetry-api.md` trace ENTRYPOINTS [04]/[12] + context PUBLIC_TYPES [05], the trace-span members the resident `.api/opentelemetry-api.md` metrics/context scope does not carry), `msgspec` (the `Struct` bound the `dispatch` canonical type arguments carry), `expression` (`Error`/`Ok`/`tagged_union`/`case`/`tag`).
- Auto: the serve leg admits one `aio_server_interceptor(filter_=filters.negate(filters.health_check()))` into `grpc.aio.server(interceptors=[...])` so every RPC emits a `SERVER` span without a hand-rolled interceptor, the filter suppressing the `grpc.health.v1.Health` liveness-RPC trace noise; the interceptor is wired once at server construction, never per request, and the `TracerProvider` arrives installed at `observability/telemetry#TELEMETRY`. The inbound trace-context extract rides the servicer entry: `ServerHost.inbound` reads the carrier off `grpc.aio.ServicerContext.invocation_metadata()` projected to `dict[str, str]` and calls `observability/receipts#RECEIPT` `Signals.continue_inbound`, then `ServerHost.dispatch` opens the `Signals.attach(parent)` context-manager scope around the decode/handler/encode body so the companion span seeds from the C#-minted parent rather than a fresh root — the `attach`/`detach` token pair is the receipts owner's, composed here at the one servicer hop, never re-minted. The SERVER-span enrichment is a span-attribute enricher composed AFTER the catalogue `aio_server_interceptor` opens the SERVER span — `ServerHost.enrich` calls `trace.get_current_span().set_attributes(context.attribute() | {"rasm.descriptor": ..., "rasm.fault_case": ...})` inside the active span scope, folding the `execution/admission#CONTEXT` `RuntimeContext.attribute()` `rasm.tenant`/`rasm.hlc` projection (the admission owner already mints those keys from the causal frame, so the serve leg composes them rather than re-spelling `rasm.tenant`) with the descriptor and fault-case dimensions in one `set_attributes` call (NOT a hook param on `aio_server_interceptor`, which takes none, and NOT a hand-rolled tracing interceptor, which the catalogue Reject row forbids), so a flame graph shows the descriptor, tenant, HLC, and resolved fault case per RPC, not a bare method name. Compression rides the `grpc.Compression` member passed at server construction — the runtime selects the algorithm, never re-implementing the codec the C-core owns.
- Growth: a new servicer method is one generated stub delegating to the existing `ServerHost.dispatch` with its descriptor id, codec pair, and railed handler — never a new admit/transcode/abort prologue; a new wire message is one `transport/wire#PROTO_TRANSCODE` `WireProtoCodec` type pair the `dispatch` four-type signature already accepts; a new capability is one descriptor row the `[03]-[CAPABILITY_INVOKE]` catalog already discriminates; a new enriched server-span dimension is one key in the `enrich` `set_attributes` union; a new fault-to-status mapping is one `_FAULT_STATUS` row; a new compression algorithm is one `grpc.Compression` member at construction; zero new surface, no second RPC server.
- Boundary: the wire contract is the existing C# `ComputeService`/`DocumentService`/`ControlService`/`ArtifactSync` proto — the runtime mints no transport, no channel, and no second wire vocabulary; a hand-rolled message loop, a divergent message shape, a per-servicer admit/transcode/abort prologue duplicated across the four service contracts rather than folded through the one `dispatch` aspect, an erased `object` inbound pair where the OTel `Context` is typed, a bare exception across the wire, a raw `int()` parse of the inbound stamp that panics rather than railing through the `wire` boundary, a per-arm `if` ladder mapping a fault to a status where the `_FAULT_STATUS` table projects, a key-chain server credential on the UDS leg, a second RPC server, a fresh-root span where the C# parent is on the inbound carrier, and a request_hook/response_hook on the SERVER interceptor (the admitted `aio_server_interceptor(tracer_provider, filter_)` takes no hook params — server enrichment is `set_attributes` composing `RuntimeContext.attribute()`, the hook law scopes hooks to CLIENT spans) are the deleted forms; the C# host lifecycle, global health, and product telemetry export stay AppHost-owned; the `Hlc`/`ElementId`/`Tenant`/`CausalFrame` stamp lives in the `clock#CLOCK` owner and the codec lives in `transport/wire`, both consumed here and re-minted nowhere. The serve runtime rides the cp315 core floor through `grpcio` (1.81.1, `[CORE]` ungated, resolving on cp315 transitively via `google-cloud-storage` and declared explicit-core) and `protobuf`; only `grpcio-tools` stub regeneration rides the Forge companion lane (`python_version<'3.15'`), so server boot never depends on the companion environment and the cp315 core never imports `grpc_tools`.

```python signature
from collections.abc import Awaitable, Callable
from typing import Literal, Self, assert_never

import grpc
from expression import Error, Ok, case, tag, tagged_union
from google.protobuf.message import Message
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.context import Context
from opentelemetry.instrumentation.grpc import aio_server_interceptor, filters

from rasm.runtime.admission import RuntimeContext, RuntimeProfile
from rasm.runtime.clock import CausalFrame
from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary, boundary
from rasm.runtime.receipts import Signals
from rasm.runtime.wire import WireProtoCodec

type FaultStatus = tuple[grpc.StatusCode, str]

# --- [POLICIES] ------------------------------------------------------------------------

_FAULT_STATUS: dict[str, FaultStatus] = {
    "config": (grpc.StatusCode.FAILED_PRECONDITION, "config"),
    "resource": (grpc.StatusCode.UNAVAILABLE, "resource"),
    "deadline": (grpc.StatusCode.DEADLINE_EXCEEDED, "deadline"),
    "api": (grpc.StatusCode.INTERNAL, "api"),
    "import_": (grpc.StatusCode.UNIMPLEMENTED, "import"),
    "wire": (grpc.StatusCode.INVALID_ARGUMENT, "wire"),
    "boundary": (grpc.StatusCode.INVALID_ARGUMENT, "boundary"),
    "aggregate": (grpc.StatusCode.INTERNAL, "aggregate"),
}


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


class ServerHost:
    def __init__(self, bind: str, credential: Credential = Credential.loopback(), grace: float = 5.0) -> None:
        self._bind, self._credential, self._grace = bind, credential, grace
        interceptor = aio_server_interceptor(filter_=filters.negate(filters.health_check()))
        self._server: grpc.aio.Server = grpc.aio.server(interceptors=[interceptor], compression=grpc.Compression.Gzip)

    @staticmethod
    def inbound(servicer_context: grpc.aio.ServicerContext) -> RuntimeRail[tuple[Context, RuntimeContext]]:
        carrier = dict(servicer_context.invocation_metadata())
        parent = Signals.continue_inbound({k: carrier[k] for k in ("traceparent", "tracestate") if k in carrier})
        frame = boundary(
            "wire",
            lambda: CausalFrame.of(int(carrier.get("rasm-hlc-physical", "0")), int(carrier.get("rasm-hlc-logical", "0")), carrier.get("rasm-tenant", "default")),
        )
        return frame.map(lambda causal: (parent, RuntimeContext.admit(RuntimeProfile.SIDECAR, causal=causal)))

    @staticmethod
    def enrich(context: RuntimeContext, descriptor: str, fault_case: str) -> None:
        trace.get_current_span().set_attributes(context.attribute() | {"rasm.descriptor": descriptor, "rasm.fault_case": fault_case})

    @staticmethod
    async def abort(servicer_context: grpc.aio.ServicerContext, fault: BoundaryFault) -> bytes:
        code, case_ = _FAULT_STATUS[fault.tag]
        await servicer_context.abort(code, f"{case_}:{fault}")
        return b""

    async def dispatch[S: Struct, M: Message, R: Struct, N: Message](
        self,
        servicer_context: grpc.aio.ServicerContext,
        request: bytes,
        descriptor: str,
        codec: tuple[WireProtoCodec[S, M], WireProtoCodec[R, N]],
        handler: Callable[[S, RuntimeContext], Awaitable[RuntimeRail[R]]],
    ) -> bytes:
        decode, encode = codec
        match ServerHost.inbound(servicer_context):
            case Error(fault):
                return await ServerHost.abort(servicer_context, fault)
            case Ok((parent, context)):
                with Signals.attach(parent):
                    match decode.decode(request):
                        case Error(fault):
                            ServerHost.enrich(context, descriptor, _FAULT_STATUS[fault.tag][1])
                            return await ServerHost.abort(servicer_context, fault)
                        case Ok(shape):
                            wired = (await handler(shape, context)).bind(encode.encode)
                            match wired:
                                case Ok(payload):
                                    ServerHost.enrich(context, descriptor, "ok")
                                    return payload
                                case Error(fault):
                                    ServerHost.enrich(context, descriptor, _FAULT_STATUS[fault.tag][1])
                                    return await ServerHost.abort(servicer_context, fault)

    async def serve(self) -> RuntimeRail[None]:
        match self._credential.server_credentials():
            case Error(fault):
                return Error(fault)
            case Ok(creds):
                self._server.add_secure_port(self._bind, creds)
                await self._server.start()
                await self._server.wait_for_termination()
                return Ok(None)

    async def drain(self) -> None:
        await self._server.stop(self._grace)
```

## [03]-[CAPABILITY_INVOKE]

- Owner: `CapabilityInvoke` — the descriptor-driven polymorphic invoke decoding the C# `Rasm.AppHost/Agent/capability#SDK_CODEGEN` Python target, parameterized over the request/response `[S: Struct, M: Message, R: Struct, N: Message]` four-type arguments so the one invoke discriminates by input AND output shape; `DiscoveryResult` the descriptor catalog row the companion ingests from the C# `DiscoveryResultWire[]` projection, carrying the per-descriptor `input_schema` as a deferred-decode `msgspec.Raw` slot; `CommandReceipt` the per-command evidence the companion reads back through the C# `ReceiptEnvelopeWire<CapabilityCommandReceiptWire>`. One invoke keyed by descriptor id discriminates every capability, never a per-service hand client. The OUTBOUND dispatch wraps its channel with `aio_client_interceptors(tracer_provider, filter_, request_hook, response_hook)` so the call propagates the active span through the `observability/telemetry#TELEMETRY`-installed composite and the client hooks enrich the CLIENT span with the `clock#CLOCK` `Tenant` and the fault case while `run` sets the per-call `rasm.descriptor` attribute on the active span (the descriptor is known per call, not per channel, so the channel-level interceptor carries only the channel-stable tenant); the command request/receipt projection rides the injected `transport/wire#PROTO_TRANSCODE` `WireProtoCodec` pair, minting no fourth wire shape; the outbound call wraps `reliability/resilience#RESILIENCE` `guard(RetryClass.WIRE)` and lifts a `grpc.aio.AioRpcError` to the rail through `async_boundary`, never a bare gRPC exception escaping the dispatch.
- Cases: the C# SDK Python target emits one method per descriptor — `def {surface}_{op}(self, request: S) -> RuntimeRail[R]: return self.run("{surface}.{op}", request)` — so the companion decodes the descriptor catalog into one `run(descriptor_id, request)` dispatch where `descriptor_id` is the `{surface}.{op}` join the C# `CapabilityDescriptor.Of` mints; the per-descriptor `input_schema` is the `JsonSchemaExporter` JSON Schema the C# `SuiteContracts.Schema` derives from the descriptor's `CommandArguments`, carried on the `DiscoveryResult` as a deferred `msgspec.Raw` slot (the canonical lazy-decode path, the routing decode never paying the schema-document parse) so the schema is the discovery-time contract evidence the companion holds without re-authoring the shape — the argument payload itself is the already-typed canonical `Struct S` the `WireProtoCodec.encode` transcodes, not a hand-mirrored mapping re-validated against a JSON Schema document (msgspec validates a payload against a `Struct` type at decode, not against a JSON Schema document, so the schema rides as carried evidence rather than a per-call validator the wire codec already subsumes); the command-txn disposition decodes onto the `CommandTxn` struct whose `kind: CommandTxnKind` is the literal-discriminated disposition the C# `CommandTxn` mints — `committed` · `rolled_back` · `compensated` · `refused` — carried with its arm `detail`, the `CapabilityCommandReceiptWire.txn` kind raised onto the typed struct rather than an erased `dict`.
- Entry: `CapabilityInvoke.run` keys the descriptor by id (an absent id is a typed `wire` fault), sets the per-call `rasm.descriptor` span attribute, projects the canonical request `Struct S` onto the `transport/wire#PROTO_TRANSCODE`-paired request message through `WireProtoCodec.encode`, dispatches the wire bytes over the gRPC stub, and decodes the response message back to the canonical `Struct R` through `WireProtoCodec.decode` — one `Result`-railed `encode → dispatch → decode` chain where the encode fault, the dispatch fault, and the decode fault all ride the one `RuntimeRail`; `CapabilityInvoke.discover` reads the descriptor catalog the C# `CapabilityRegistry.Discover(DiscoveryQuery.All)` emits through `msgspec.json.Decoder(list[DiscoveryResult])` on the one `boundary("wire", ...)` fence so the companion command surface is generated from the descriptor rows, never hand-listed; `CapabilityInvoke.connect` is the single channel composer that mints the outbound `grpc.aio.insecure_channel` (loopback UDS) wrapped with the `interceptors(tenant)` factory and folds the resulting traced channel into the railed `dispatch` coroutine `run` awaits — the `dispatch` wraps `guard(RetryClass.WIRE)` around the raw-bytes `channel.unary_unary` multicallable and lifts a `grpc.aio.AioRpcError` through `async_boundary(catch=grpc.aio.AioRpcError)` so the descriptor-to-traced-wire one-hop is composed on the page, retried under the wire backoff envelope, and returns a `RuntimeRail[bytes]` rather than an unrailed coroutine threaded through an opaque callable; `CapabilityInvoke.interceptors` is the channel-stable factory `connect` consumes — it folds the catalogue `aio_client_interceptors` factory with the channel-stable `Tenant` `request_hook` and the fault-case `response_hook` so the composed channel wraps its stub once, the `request_hook` receiving `(span, request)` and the `response_hook` receiving `(span, details)`, the gRPC status-details string the interceptor awaits off the unary call (`""` on OK, the server's detail message on a non-OK status), both CLIENT-side per the catalogue hook law, the per-descriptor dimension set by `run` on the active span rather than baked into a channel-lifetime interceptor.
- Auto: the descriptor catalog crosses the wire as the `DiscoveryResultWire[]` array the C# `#TS_PROJECTION` projects — `(descriptor, surface, effect, idempotency, estimated, scope_hash)` — and the companion decodes it through `msgspec.json.Decoder(list[DiscoveryResult])`; the `effect`/`idempotency`/`cost-unit` keys decode as the C# smart-enum string keys (`pure`/`read`/`write`/`external`/`irreversible`, `idempotent`/`keyed`/`single-shot`/`non-idempotent`, `cpu-millis`/`wall-millis`/`bytes-egress`/`model-tokens`/`calls`); the cost vector decodes as the unit-keyed mapping the C# `CostVectorWire` emits; a new descriptor row regenerates the companion command surface with no per-capability hand edit because the catalog is the single descriptor source; the channel-stable client hooks set `rasm.tenant`/`rasm.fault_case` on the CLIENT span through `Span.set_attribute` inside the active span scope, never blocking, while `run` sets the per-call `rasm.descriptor` on the active span (the descriptor varying per call, the tenant fixed per channel), and the factory's `tracer_provider` defaults to `None`, resolving the global provider the `observability/telemetry#TELEMETRY` `set_tracer_provider` install registers (the catalogue provider law), so no second tracer mints; the outbound `dispatch` retries under `reliability/resilience#RESILIENCE` `guard(RetryClass.WIRE)` and a non-OK gRPC status raises `grpc.aio.AioRpcError` that `async_boundary` lifts to `Error(BoundaryFault(boundary=("wire", "AioRpcError")))`, so the transient-code retry the resilience owner's `RetryClass.WIRE` row admits and the terminal fault both ride the rail rather than a bare `except`.
- Packages: `msgspec` (`json.Decoder` PUBLIC_TYPES [02] for the catalog decode / `Struct` PUBLIC_TYPES [01] the request/receipt shapes / `Raw` PUBLIC_TYPES [02] the deferred `input_schema` slot / `ValidationError` PUBLIC_TYPES [03] — resident `.api/msgspec.md`), `protobuf` (the `transport/wire#PROTO_TRANSCODE`-paired command request/receipt `Message` the `WireProtoCodec` pair is generic over), `opentelemetry-instrumentation-grpc` (`aio_client_interceptors(tracer_provider, filter_, request_hook, response_hook)` interceptor factories, `filters` predicate algebra), `opentelemetry-api` (`trace.get_current_span`/`Span.set_attribute` for the hook and per-call descriptor enrichment — branch catalog `libs/python/.api/opentelemetry-api.md` trace ENTRYPOINTS [04]/[11], the trace-span members the resident `.api/opentelemetry-api.md` metrics/context scope does not carry), `grpcio` (`grpc.aio.insecure_channel` connect ENTRYPOINTS [02] for the loopback channel `connect` composes / `grpc.aio.AioRpcError` PUBLIC_TYPES [01] the `dispatch` lifts), `expression` (`Error`/`Ok`).
- Growth: a new capability is one descriptor row the C# registry already folds — the companion reads it through the existing `discover`/`run` pair, never a new method; a new effect/idempotency/cost key is one literal the decode union already discriminates; a new enriched span dimension is one `set_attribute` key in the existing hook, never a second interceptor; zero new surface, no per-service hand client.
- Boundary: the descriptor is the suite's only op-metadata owner and the C# registry is its sole mint — a per-capability hand client, a re-authored descriptor shape, a hand-mirrored argument schema, a JSON-Schema-document validation msgspec cannot perform (it validates a payload against a `Struct` type, not a schema document), a decode-discard-re-encode of the whole `CommandArguments` bypassing the wire codec, an empty-descriptor channel-lifetime interceptor that stamps `rasm.descriptor=""` for every call, an unrailed `grpc.aio.AioRpcError` escaping the dispatch, a second cost table, a hand-rolled tracing interceptor (the catalogue Reject row), and a second tracer are the deleted forms; the companion reads the descriptor, re-authoring no capability shape; the command dispatch lands on the gRPC stub through the injected `transport/wire#PROTO_TRANSCODE` `WireProtoCodec` pair and retries under `reliability/resilience#RESILIENCE` `guard(RetryClass.WIRE)`, so the capability invoke rides the existing proto wire and mints no fourth wire shape; the `Tenant` partition reads the one `clock#CLOCK` owner and re-mints no tenant scheme; the cross-language shape-identity is the C# `SuiteContracts.Schema` JSON Schema the C#, TS, and Python SDKs all bind, so an additive descriptor field admits on every consumer and a breaking reshape rejects on all three through the one `ContractGuard.AdditiveOnly` classifier.

```python signature
from collections.abc import Awaitable, Callable
from typing import Literal, Self, assert_never

import grpc
import msgspec
from expression import Error, Ok
from google.protobuf.message import Message
from msgspec import Raw, Struct
from opentelemetry import trace
from opentelemetry.instrumentation.grpc import aio_client_interceptors, filters

from rasm.runtime.clock import Tenant
from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary, boundary
from rasm.runtime.resilience import RetryClass, guard
from rasm.runtime.wire import WireProtoCodec


type CostUnitKey = Literal["cpu-millis", "wall-millis", "bytes-egress", "model-tokens", "calls"]
type CostVector = dict[CostUnitKey, int]
type CommandTxnKind = Literal["committed", "rolled_back", "compensated", "refused"]
type WireDispatch = Callable[[str, bytes], Awaitable[RuntimeRail[bytes]]]


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


class CommandArguments(Struct, frozen=True):
    payload: bytes
    tenant: Tenant
    correlation: str


class CapabilityInvoke[S: Struct, M: Message, R: Struct, N: Message]:
    _catalog: msgspec.json.Decoder[list[DiscoveryResult]] = msgspec.json.Decoder(list[DiscoveryResult])

    def __init__(self, catalog: dict[str, DiscoveryResult], codec: tuple[WireProtoCodec[S, M], WireProtoCodec[R, N]], dispatch: WireDispatch, tenant: Tenant) -> None:
        self._catalog_by_id, self._encode, self._decode, self._dispatch, self._tenant = catalog, codec[0], codec[1], dispatch, tenant

    @classmethod
    def discover(cls, payload: bytes) -> RuntimeRail[list[DiscoveryResult]]:
        return boundary("wire", lambda: cls._catalog.decode(payload))

    @staticmethod
    def interceptors(tenant: Tenant) -> list[grpc.aio.ClientInterceptor]:
        def request_hook(span: trace.Span, _request: object) -> None:
            span.set_attribute("rasm.tenant", str(tenant))

        def response_hook(span: trace.Span, details: str) -> None:
            span.set_attribute("rasm.fault_case", details or "ok")

        return aio_client_interceptors(filter_=filters.negate(filters.health_check()), request_hook=request_hook, response_hook=response_hook)

    @classmethod
    def connect(cls, target: str, catalog: dict[str, DiscoveryResult], codec: tuple[WireProtoCodec[S, M], WireProtoCodec[R, N]], tenant: Tenant) -> Self:
        channel = grpc.aio.insecure_channel(target, interceptors=cls.interceptors(tenant))

        async def dispatch(descriptor_id: str, request: bytes) -> RuntimeRail[bytes]:
            method = channel.unary_unary(f"/rasm.capability/{descriptor_id}")
            return await async_boundary("wire", lambda: guard(RetryClass.WIRE)(method, request), catch=grpc.aio.AioRpcError)

        return cls(catalog, codec, dispatch, tenant)

    async def run(self, descriptor_id: str, request: S) -> RuntimeRail[R]:
        if descriptor_id not in self._catalog_by_id:
            return Error(BoundaryFault(wire=(descriptor_id, 0)))
        trace.get_current_span().set_attribute("rasm.descriptor", descriptor_id)
        match self._encode.encode(request):
            case Error(fault):
                return Error(fault)
            case Ok(payload):
                dispatched = await self._dispatch(descriptor_id, payload)
                return dispatched.bind(self._decode.decode)
            case _ as unreachable:
                assert_never(unreachable)
```

## [04]-[ENTRY]

- Owner: `Entrypoint` — the type-hint-driven `cyclopts` command axis backing the companion daemon's PRIVATE entry and package-internal entrypoints only; co-located with `ServerHost` because `serve` composes the host it launches.
- Entry: `companion_app` returns the `cyclopts.App` whose commands bind to the companion serve and drain; arguments bind from type hints with the `cyclopts.types.NonNegativeFloat` constrained grace and `Parameter(env_var=...)` env binding, never a hand-parsed `argv` or a hand-rolled single-value validator the type library already owns; the app's `result_action="return_int_as_exit_code_else_zero"` maps the command's railed outcome to the process exit, so the serve command awaits the railed `ServerHost.serve`, folds `Ok`→`0`/`Error`→`1` through `map`/`default_value`, and a non-loopback credential surfaces as a typed fault collapsing to a non-zero exit rather than a panic — the command body never touches an unrailed exception and the traceback never escapes the CLI boundary.
- Packages: `cyclopts` (`App` PUBLIC_TYPES [01] with `result_action` ENTRYPOINTS [06] / `Parameter` PUBLIC_TYPES [02] env binding / `cyclopts.types.NonNegativeFloat` PUBLIC_TYPES [02] the constrained grace input).
- Growth: a new private command is one `@app.command` method on the existing app, its railed outcome folding to the exit code through the app's `result_action`; zero new surface.
- Boundary: never a new public command surface — public commands are reserved to the suite Assay command surface; a public-facing CLI axis and a hand-parsed argument loop are the deleted forms.

```python signature
from typing import Annotated

from cyclopts import App, Parameter
from cyclopts.types import NonNegativeFloat


def companion_app() -> App:
    app = App(name="rasm-companion", help="private companion daemon entry", result_action="return_int_as_exit_code_else_zero")

    @app.command
    async def serve(bind: str, *, grace: Annotated[NonNegativeFloat, Parameter(env_var="RASM_COMPANION_GRACE")] = 5.0) -> int:
        return (await ServerHost(bind, Credential.loopback(), grace).serve()).map(lambda _: 0).default_value(1)

    return app
```

## [05]-[RESEARCH]

- [CREDENTIAL_PEM]: [COMPLETE] — decoded against the C# `Rasm.Compute/Runtime/channels#CALL_POLICY` `CredentialPolicy` five-row axis. The companion serves the local control plane over `RemoteTransport.UnixDomainSocket`, whose `Credentials` column is `Seq(CredentialPolicy.InsecureLoopback)` exactly, so the serve-side credential is `insecure_loopback` and peer identity is the kernel-reported `(pid, uid)` the C# `AppHost/companion-sidecar#PEER_ADMISSION` reads at accept (`SO_PEERCRED` on Linux, `LOCAL_PEERCRED`+`LOCAL_PEERPID` on macOS), never a wire PEM. The serve credential is `grpc.local_server_credentials(grpc.LocalConnectionType.UDS)` — `grpc.local_server_credentials` (`.api/grpcio.md` credential ENTRYPOINTS [03]) and `grpc.LocalConnectionType.UDS` (credential PUBLIC_TYPES [03]) are the cataloged loopback authenticator pair, returning a `grpc.ServerCredentials` (credential PUBLIC_TYPES [01]). The four outbound rows (`tls`/`mtls`/`bearer`/`composed`) are the OUTBOUND C# client legs (`ChannelCredentials.SecureSsl`/`CallCredentials.FromInterceptor`/`CallCredentials.Compose`) carrying PEM/token material the companion never serves — the prior `\n--SEP--\n` PEM-split placeholder was a fabricated serve-side PEM bundle and is the deleted form. The `Credential.server_credentials` fold is total over the five producer rows by `match`/`assert_never` and returns the four outbound rows as `Error(BoundaryFault(config=...))` on the `RuntimeRail` rather than raising a bare `ValueError`, so a credential the UDS serve leg cannot admit is a typed rail failure threaded through `ServerHost.serve`.
- [GRPC_CORE_COMPOSE]: [COMPLETE] — reflection-confirmed against `.api/grpcio.md`. `grpcio` 1.81.1 is `[CORE]` and ungated, resolving on cp315 transitively via `google-cloud-storage` (`grpcio-status`/`grpc-google-iam-v1`) and declared as an explicit ungated core direct dependency for the transport rail. `ServerHost` composes `grpc.aio.server(interceptors=[...], compression=grpc.Compression.Gzip)` (serve ENTRYPOINTS [01]), the `grpc.aio.Server` lifecycle `add_secure_port`/`start`/`stop`/`wait_for_termination` (lifecycle ENTRYPOINTS [01]/[03]/[04]/[05]), `grpc.local_server_credentials(grpc.LocalConnectionType.UDS)` (credential ENTRYPOINTS [03] + PUBLIC_TYPES [03], returning `grpc.ServerCredentials` PUBLIC_TYPES [01]), `grpc.aio.ServicerContext.invocation_metadata`/`.abort` (PUBLIC_TYPES [03]), `grpc.StatusCode` (PUBLIC_TYPES [01]), and `grpc.Compression.Gzip` (PUBLIC_TYPES [02]) per the `[04]-[IMPLEMENTATION_LAW]` serve/credential/context/interceptor/compression rows — one `grpc.aio.server` with construction-time interceptors, `ssl_*`/`local_*` credentials from the one `Credential` admission point, status set on the context rather than raised across the wire, tracing interceptors settled from the OTel grpc owner, and the `grpc.Compression` member selecting the algorithm without re-implementing the C-core codec. Only `grpcio-tools` (the `protoc` codegen companion) remains `[GATED]` `python_version<'3.15'`; the cp315 core imports `grpc`/`grpc.aio` directly and never imports `grpc_tools`, so server boot never depends on the companion environment.
- [INBOUND_FRAME_RAIL]: [COMPLETE] — `ServerHost.inbound` returns `RuntimeRail[tuple[Context, RuntimeContext]]` (the OTel `opentelemetry.context.Context` typed, never the prior erased `object`): the W3C `traceparent`/`tracestate` extract through `Signals.continue_inbound` runs OUTSIDE the wire fence (the non-failing parent-context resolve, never a parse fault that should land as a wire `BoundaryFault`), and only the `int(...)` two-half stamp parse plus the `clock#CLOCK` `CausalFrame.of` lift run inside the one `boundary("wire", ...)` fence, `.map`-folded onto the `(parent, RuntimeContext.admit(...))` pair, so a malformed `rasm-hlc-physical`/`rasm-hlc-logical` slot returns `Error(BoundaryFault(boundary=("wire", "ValueError")))` on the rail — the `boundary` case with subject `"wire"` that the `reliability/faults#FAULT` conversion surface mints (never the code-carrying `wire` case, reserved for explicit `BoundaryFault(wire=(subject, code))`), never an unhandled servicer panic and never a trace-extract failure mis-tagged as a wire fault. `CausalFrame.of` receives validated ints and never sees a raw string, the boundary fence owning the parse; `ServerHost.dispatch` consumes the railed pair and aborts with `INVALID_ARGUMENT` before opening the `Signals.attach` span scope, so a bad frame never produces a partial admit.
- [CAPABILITY_INVOKE]: [COMPLETE] — decoded against the C# `Rasm.AppHost/Agent/capability#SDK_CODEGEN` Python target (`def {method}(self, request: S) -> RuntimeRail[R]: return self.run("{descriptor}", request)`) and the `#TS_PROJECTION` `DiscoveryResultWire`/`CapabilityCommandReceiptWire` shapes. `CapabilityInvoke[S, M, R, N]` is parameterized over the request/response canonical-`Struct`/`Message` four-type arguments (input AND output polymorphic). `CapabilityInvoke.discover` decodes the catalog through `msgspec.json.Decoder(list[DiscoveryResult])` on the one `boundary("wire", ...)` fence and `CapabilityInvoke.run` is the one polymorphic invoke — descriptor-id lookup against the frozen catalog (an absent id is a typed `wire` fault), a per-call `rasm.descriptor` span attribute, the injected `transport/wire#PROTO_TRANSCODE` `WireProtoCodec.encode` of the canonical request `Struct S` to wire bytes, the railed `dispatch` (wrapping `reliability/resilience#RESILIENCE` `guard(RetryClass.WIRE)` and lifting `grpc.aio.AioRpcError` through `async_boundary`), and `WireProtoCodec.decode` of the response message back to the canonical `Struct R`, all on the `RuntimeRail` as one `encode → dispatch → decode` `bind` chain. The prior `run` decoded `args.payload` to a bare `dict`, discarded it, then `msgspec.json.encode`-d the whole `CommandArguments` (tenant/correlation included) as the wire payload — an incoherent decode-discard-re-encode that bypassed the wire codec and claimed a JSON-Schema validation msgspec cannot perform (msgspec validates a payload against a `Struct` type, not a JSON Schema document); the rebuild routes the request through the `WireProtoCodec` pair the boundary owner mandates and carries the C# `SuiteContracts.Schema` `input_schema` as a deferred `msgspec.Raw` discovery-time evidence slot rather than a per-call validator. `DiscoveryResult` matches the producer `DiscoveryResult` (`descriptor, surface, effect, idempotency, estimated, scope_hash`) field-for-field plus the `input_schema` `Raw` slot, and the `committed`/`rolled_back`/`compensated`/`refused` `CommandTxnKind` mirrors the producer `CommandTxn` `Committed`/`RolledBack`/`Compensated`/`Refused` arms and lands on the typed `CommandTxn` struct's `kind` discriminant plus its `detail` payload, the `CapabilityCommandReceiptWire.txn` decoded onto the typed struct rather than an erased mapping, never a re-minted discriminant. The companion re-authors no descriptor shape and the command request/receipt projection rides the `transport/wire#PROTO_TRANSCODE` codec, minting no fourth wire shape. Settled; awaits only the live descriptor catalog emission at SDK-bootstrap.
- [GRPC_CLIENT_AND_HOOKS]: [COMPLETE] (design) — reflection-confirmed against `.api/opentelemetry-instrumentation-grpc.md`. CLIENT enrichment rides `aio_client_interceptors(tracer_provider, filter_, request_hook, response_hook)` (interceptor factories, the only factory carrying hook params) on the `[03]-[CAPABILITY_INVOKE]` outbound dispatch and any host call-back channel — the channel-stable `request_hook` reads `(span, request)` and sets `rasm.tenant`, the `response_hook` reads `(span, details)` (the unary `call.details()` status-details string, empty on OK) and sets `rasm.fault_case`, both CLIENT-side per the hook law. The prior `interceptors("", tenant)` baked an EMPTY descriptor id into the channel-lifetime interceptor, so every call over the one channel stamped `rasm.descriptor=""` while `run` dispatched many descriptors — a dead per-descriptor dimension; the rebuild makes `interceptors(tenant)` channel-stable and `run` sets the per-call `rasm.descriptor` on the active span (the descriptor known per call, the tenant fixed per channel), so the descriptor reaches the CLIENT span per RPC. `CapabilityInvoke.connect` is the single channel composer that mints the loopback `grpc.aio.insecure_channel(target, interceptors=interceptors(tenant))` (channel ENTRYPOINTS [02]) and folds the interceptor-wrapped channel into the railed `dispatch` coroutine `run` awaits — `dispatch` wraps `guard(RetryClass.WIRE)` around the raw-bytes multicallable and lifts `grpc.aio.AioRpcError` (PUBLIC_TYPES [01], carrying `code()`/`details()`) through `async_boundary(catch=grpc.aio.AioRpcError)` to `RuntimeRail[bytes]`, so the descriptor-to-traced-wire one-hop is composed on the page, retried under the wire envelope, and never a dangling factory or an unrailed gRPC exception — the channel, the interceptor wrap, the retry, and the dispatch are one construction. SERVER enrichment is a span-attribute enricher: `ServerHost.enrich` calls `trace.get_current_span().set_attributes(context.attribute() | {"rasm.descriptor": ..., "rasm.fault_case": ...})` (NOT a server-interceptor hook — `aio_server_interceptor(tracer_provider=None, filter_=None)` takes NO hook params, and the catalogue Reject row forbids a hand-rolled tracing interceptor) AFTER the catalogue `aio_server_interceptor` opens the SERVER span, inside the `Signals.attach` parent scope `dispatch` owns, folding the `execution/admission#CONTEXT` `RuntimeContext.attribute()` `rasm.tenant`/`rasm.hlc` projection rather than re-spelling those keys, never blocking. `tracer_provider` is the `observability/telemetry#TELEMETRY`-installed provider so no second tracer mints; the outbound interceptors compose the same `Propagators.DefaultTextMapPropagator` the inbound `observability/receipts#RECEIPT` `continue_inbound` extract reads, so inbound and outbound legs share one trace (the mechanical reason this sequences after `TRACE_INBOUND_EXTRACT`). Realizes the cross-`libs/` `ONE_DISTRIBUTED_TRACE` outbound Python leg. Spellings settled.
- [SERVICER_DISPATCH]: [COMPLETE] (design) — the prior page described the inbound admit, the `Signals.continue_inbound` extract, the `WireProtoCodec` transcode, the `enrich` span fold, and the `ServicerContext.abort` status set as prose obligations but carried NO surface tying them together: there was no servicer body, no `Signals.attach` activation, and the `inbound` pair's first element was an erased `object`. `ServerHost.dispatch[S: Struct, M: Message, R: Struct, N: Message]` is the one polymorphic servicer-body aspect every generated stub method folds through — it admits via `inbound`, opens `Signals.attach(parent)` around the body so the SERVER span seeds from the C# parent (the receipts owner's token-paired scope, composed at the one servicer hop), decodes the request bytes through the injected `transport/wire#PROTO_TRANSCODE` `WireProtoCodec`, awaits the railed `handler(shape, context)`, encodes the reply through the response codec, calls `enrich` with the resolved fault case, and folds any `BoundaryFault` to `ServicerContext.abort` through the `_FAULT_STATUS` tag→`grpc.StatusCode` data table (a status projection keyed on the fault tag, never a per-arm `if` ladder), so a servicer method is the descriptor id plus the `(WireProtoCodec, WireProtoCodec)` codec pair plus the railed `handler` — never a hand-written admit/transcode/abort prologue per RPC, and the AOP weave (inbound admission, parent-span scope, transcode, enrichment, status fold) is one decorator-shaped aspect rather than five inline repetitions across the four C# service contracts. The generated stubs register through the `grpc.{unary_unary,...}_rpc_method_handler` factory family (handler-law generated path) with raw-bytes (de)serializers so the request reaches `dispatch` as the `bytes` the `WireProtoCodec` owns, the codec staying the single `Struct`↔`Message` transcode owner the wire page mints. Spellings settled against `.api/grpcio.md` (`ServicerContext.abort` PUBLIC_TYPES [03], `StatusCode` PUBLIC_TYPES [01], handler factories ENTRYPOINTS [05]/[06]) and `.api/opentelemetry-api.md` (`Context` PUBLIC_TYPES [05], `Span.set_attributes` ENTRYPOINTS [12]).
- [CLOCK_REHOME]: [COMPLETE] — `Hlc`/`ElementId`/`Tenant`/`CausalFrame` formerly defined inline on this page now live in the `clock#CLOCK` owner (`libs/python/runtime/.planning/clock/clock.md`), so `ServerHost.inbound` imports `CausalFrame`/`Tenant` from `rasm.runtime.clock` and lifts the inbound two-half stamp through `CausalFrame.of`, re-minting no stamp. The `transport/wire#PROTO_TRANSCODE`/`#CRDT_DECODE` codec (`WireProtoCodec`/`CrdtOp`/`CrdtOpDecode`) formerly inline on this page now lives in the `transport/wire` owner, so this page composes the codec at the servicer seam rather than carrying the codec body — the serve page owns only `ServerHost`/`Credential`/`CapabilityInvoke`/`Entrypoint`, the codec and clock being separate single-mint owners this page consumes.
- [WIRE_REHOME]: [COMPLETE] — the `[02]-[PROTO_TRANSCODE]` and `[03]-[CRDT_DECODE]` sections this page formerly carried (the `WireProtoCodec` protobuf seam and the `CrdtOp`/`CrdtOpWire`/`CrdtOpDecode` MessagePack op-log decode with the `DecompressFn` seam) re-homed to the `transport/wire` owner in the R0 collapse, so the serve page no longer carries the full codec body inline; the servicers transcode at the `transport/wire#PROTO_TRANSCODE` seam and the durability decode rides `transport/wire#CRDT_DECODE`, both consumed here and re-minted nowhere. The `lz4` decompress decision (`MessagePackCompression.Lz4BlockArray` envelope, `DecompressFn`-injected, gated `python_version<'3.15'`) is the `transport/wire#CRDT_DECODE` `[CRDT_DECODE_LZ4]` upstream-blocked gate, not a serve-page concern.
- [KEYRING_CATALOGUE]: [DROPPED] (serve leg) — the prior `keyring.get_password` resolution was the serve-side PEM-bundle path the `[CREDENTIAL_PEM]` decode deletes; the companion UDS serve leg reads no OS keyring (peer identity is the kernel accept-time credential), so the `keyring` package leaves the SERVE owner. `keyring` is NOT folder-orphaned — it lands the one `execution/admission#SETTINGS` `SecretBoundary` reading OS-keystore credentials for the OUTBOUND transport legs (`transport/roots#RESOURCE` `ssh` passphrase, `http` bearer), profile-gated and lazy on the outbound leg; the serve leg admits no keyring lookup.
