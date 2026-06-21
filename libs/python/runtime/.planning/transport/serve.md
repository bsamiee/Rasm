# [PY_RUNTIME_SERVE]

The inbound companion server-host owner, the credential axis, the descriptor-driven capability invoke, and the private daemon entrypoint. `ServerHost` owns the `grpc.aio` server lifecycle (bind, interceptor injection, compression, request lifecycle, graceful drain) under the `anyio` runner, hosting the geometry companion daemon that speaks the EXISTING C# `ComputeService`/`DocumentService`/`ControlService`/`ArtifactSync` gRPC contract over the UDS leg. `Credential` is the Python half of the C# `CredentialPolicy` axis, decoded against the five producer rows and resolved to the one credential the companion UDS serve leg admits — `insecure_loopback`, peer identity read by the kernel at accept, never a PEM bundle. `CapabilityInvoke` consumes the C#-generated capability-descriptor SDK, deriving the companion command surface from the descriptor catalog and wrapping its outbound channel with the catalogue client interceptors. `Entrypoint` is the type-hint-driven `cyclopts` command grammar that launches the daemon — a PRIVATE entry only. The wire vocabulary the inbound servicers and the outbound dispatch carry is the `transport/wire#PROTO_TRANSCODE`/`#CRDT_DECODE` codec owner; the host-minted causal time is the `clock#CLOCK` owner; the admission context is the `execution/admission#CONTEXT` owner. The package owns the companion's inbound serve, its credential admission, and its descriptor-driven invoke; it mints no wire vocabulary, no clock, and no context — it composes those owners and re-mints nothing.

## [01]-[INDEX]

- [01]-[SERVE]: the inbound `grpc.aio` server-host lifecycle, the decoded credential axis, the OTel span interceptor, the inbound causal-frame decode on the Result rail.
- [02]-[CAPABILITY_INVOKE]: the descriptor-driven polymorphic invoke decoding the C# capability SDK and wrapping the outbound channel with the catalogue client interceptors.
- [03]-[ENTRY]: the private companion entrypoint grammar.

## [02]-[SERVE]

- Owner: `ServerHost` — the boundary capsule over one `grpc.aio` server hosting servicers generated from the C# proto descriptors, owning the bind, the construction-time interceptor and compression injection, the inbound carrier decode, the SERVER-span enrichment, and the graceful drain; `Credential` the tagged union decoding the C# `CredentialPolicy` five-row axis to the one credential the companion UDS serve leg admits. The wire shapes the servicers transcode are the `transport/wire#PROTO_TRANSCODE` `WireProtoCodec` owner's; the host-minted `(Hlc, Tenant)` frame the inbound carrier carries is the `clock#CLOCK` `CausalFrame.of` owner's; the admission context the servicer threads is the `execution/admission#CONTEXT` `RuntimeContext` owner's — `ServerHost` composes all three and re-mints none.
- Cases: the C# `CredentialPolicy` mints five rows — `insecure-loopback`, `tls`, `mtls`, `bearer`, `composed` — but the companion serves the local control plane over the UDS leg (`RemoteTransport.UnixDomainSocket`, whose `Credentials` column is `Seq(CredentialPolicy.InsecureLoopback)` exactly), so the decode admits one constructible serve-side case: `Credential` case `insecure_loopback=True` is the only server credential, and peer identity is the kernel-reported `(pid, uid)` the C# `AppHost/companion-sidecar#PEER_ADMISSION` reads at accept through `SO_PEERCRED`/`LOCAL_PEERCRED`, never a wire-carried PEM; `tls`/`mtls`/`bearer`/`composed` are the OUTBOUND client legs the C# `Rasm.AppHost` dials and carry PEM/token material the companion never serves, so the decode names them as the deleted serve-side forms rather than constructing a server credential from them. The `Credential.server_credentials` fold is total over the five producer rows by `match`/`assert_never` — the four outbound rows return `Error(BoundaryFault(config=...))` on the `RuntimeRail` rather than raising, so a producer row the UDS serve leg cannot admit is a typed construction failure on the rail, never a bare `ValueError` and never a phantom PEM bundle.
- Entry: `ServerHost.serve` binds the UDS socket through the railed credential fold, starts the server, and awaits termination under the `anyio` runner; `ServerHost.drain` calls `server.stop(grace)` participating in the host drain choreography so in-flight calls complete within the grace period; `Credential.server_credentials` folds the loopback case to `grpc.local_server_credentials(grpc.LocalConnectionType.UDS)` on the Ok rail so the UDS peer authenticates by socket locality, never a key-chain pair; `ServerHost.inbound` decodes the inbound metadata carrier into the `(RuntimeContext, trace-context)` pair on the one `reliability/faults#FAULT` `boundary("wire", ...)` conversion — the `rasm-hlc-physical`/`rasm-hlc-logical` two-half stamp parse rides that fence so a malformed frame lands as `Error(BoundaryFault(boundary=("wire", <ExcType>)))` on the rail, never a raw `int()` that panics across the servicer.
- Auto: outcomes map to `grpc.StatusCode` and a domain `Error(BoundaryFault)` from `reliability/faults#FAULT` converts through `ServicerContext.abort`, never a bare exception across the wire; the generated stubs arrive from `grpcio-tools` compiling the C# `.proto` descriptors — the runtime implements servicers over them and transcodes at the `transport/wire#PROTO_TRANSCODE` seam; the credential decode is total over the five producer rows by `match`/`assert_never`; the `LocalConnectionType.UDS` server credential is the grpc-native loopback authenticator the C# `ChannelCredentials.Insecure` UDS dial pairs with at the same locality boundary; the inbound `(hlc_physical, hlc_logical, tenant)` slot lifts through `clock#CLOCK` `CausalFrame.of` onto `RuntimeContext.causal` so the parse failure rides the rail rather than a servicer panic — `CausalFrame.of` receives validated ints and never sees a raw string, the string-to-int decode living inside the one `boundary("wire", ...)` fence in `inbound`.
- Packages: `grpcio` (`grpc.aio.server` serve ENTRYPOINTS [01] / `grpc.local_server_credentials` credential ENTRYPOINTS [03] / `grpc.LocalConnectionType` credential PUBLIC_TYPES [03] / `grpc.ServerCredentials` credential PUBLIC_TYPES [01] / `grpc.StatusCode` PUBLIC_TYPES [01] / `grpc.Compression` PUBLIC_TYPES [02] / `grpc.aio.Server` PUBLIC_TYPES [01] with `add_secure_port`/`start`/`stop`/`wait_for_termination` lifecycle ENTRYPOINTS [01]/[03]/[04]/[05] / `grpc.aio.ServicerContext.abort`/`invocation_metadata` PUBLIC_TYPES [03] / `grpc.aio.ClientInterceptor` PUBLIC_TYPES [02]), `grpcio-tools` (gated `python_version<'3.15'` codegen companion, stub regeneration only — never imported on the cp315 core), `protobuf` (the `transport/wire` codec's generated `_pb2` messages), `opentelemetry-instrumentation-grpc` (`aio_server_interceptor`/`filters.health_check`/`filters.negate`), `opentelemetry-api` (`trace.get_current_span`/`Span.set_attribute` for the SERVER-span enrichment — branch catalog `libs/python/.api/opentelemetry-api.md` trace ENTRYPOINTS [04]/[11], the trace-span members the resident `.api/opentelemetry-api.md` metrics/context scope does not carry), `anyio` (`create_task_group`), `expression` (`Error`/`Ok`/`tagged_union`/`case`/`tag`).
- Auto: the serve leg admits one `aio_server_interceptor(filter_=filters.negate(filters.health_check()))` into `grpc.aio.server(interceptors=[...])` so every RPC emits a `SERVER` span without a hand-rolled interceptor, the filter suppressing the `grpc.health.v1.Health` liveness-RPC trace noise; the interceptor is wired once at server construction, never per request, and the `TracerProvider` arrives installed at `observability/telemetry#TELEMETRY`. The inbound trace-context extract rides the servicer entry: `ServerHost.inbound` reads the carrier off `grpc.aio.ServicerContext.invocation_metadata()` projected to `dict[str, str]` and calls `observability/receipts#RECEIPT` `Signals.continue_inbound` BEFORE the servicer body opens its measured span, so the companion span seeds from the C#-minted parent rather than a fresh root. The SERVER-span enrichment is a span-attribute enricher composed AFTER the catalogue `aio_server_interceptor` opens the SERVER span — `ServerHost.enrich` calls `trace.get_current_span().set_attribute(rasm.descriptor/rasm.tenant/rasm.fault_case)` inside the active span scope (NOT a hook param on `aio_server_interceptor`, which takes none, and NOT a hand-rolled tracing interceptor, which the catalogue Reject row forbids), so a flame graph shows the descriptor and `Tenant` per RPC, not a bare method name. Compression rides the `grpc.Compression` member passed at server construction — the runtime selects the algorithm, never re-implementing the codec the C-core owns.
- Growth: a new servicer is one generated stub implementation; a new wire message is one `transport/wire#PROTO_TRANSCODE` `WireProtoCodec` type pair; a new capability is one descriptor row the `[2]-[CAPABILITY_INVOKE]` catalog already discriminates; a new enriched server-span dimension is one `set_attribute` key in `enrich`; a new compression algorithm is one `grpc.Compression` member at construction; zero new surface, no second RPC server.
- Boundary: the wire contract is the existing C# `ComputeService`/`DocumentService`/`ControlService`/`ArtifactSync` proto — the runtime mints no transport, no channel, and no second wire vocabulary; a hand-rolled message loop, a divergent message shape, a bare exception across the wire, a raw `int()` parse of the inbound stamp that panics rather than railing through the `wire` boundary, a key-chain server credential on the UDS leg, a second RPC server, a fresh-root span where the C# parent is on the inbound carrier, and a request_hook/response_hook on the SERVER interceptor (the admitted `aio_server_interceptor(tracer_provider, filter_)` takes no hook params — server enrichment is `set_attribute`, the hook law scopes hooks to CLIENT spans) are the deleted forms; the C# host lifecycle, global health, and product telemetry export stay AppHost-owned; the `Hlc`/`ElementId`/`Tenant`/`CausalFrame` stamp lives in the `clock#CLOCK` owner and the codec lives in `transport/wire`, both consumed here and re-minted nowhere. The serve runtime rides the cp315 core floor through `grpcio` (1.81.1, `[CORE]` ungated, resolving on cp315 transitively via `google-cloud-storage` and declared explicit-core) and `protobuf`; only `grpcio-tools` stub regeneration rides the Forge companion lane (`python_version<'3.15'`), so server boot never depends on the companion environment and the cp315 core never imports `grpc_tools`.

```python signature
from typing import Literal, assert_never

import anyio
import grpc
from expression import Error, Ok, case, tag, tagged_union
from opentelemetry import trace
from opentelemetry.instrumentation.grpc import aio_server_interceptor, filters

from rasm.runtime.admission import RuntimeContext, RuntimeProfile
from rasm.runtime.clock import CausalFrame, Tenant
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary
from rasm.runtime.receipts import Signals


@tagged_union(frozen=True)
class Credential:
    tag: Literal["insecure_loopback", "tls", "mtls", "bearer", "composed"] = tag()
    insecure_loopback: bool = case()
    tls: str = case()
    mtls: tuple[str, str] = case()
    bearer: str = case()
    composed: tuple[str, ...] = case()

    def server_credentials(self) -> RuntimeRail[grpc.ServerCredentials]:
        match self:
            case Credential(tag="insecure_loopback"):
                return Ok(grpc.local_server_credentials(grpc.LocalConnectionType.UDS))
            case Credential(tag="tls") | Credential(tag="mtls") | Credential(tag="bearer") | Credential(tag="composed"):
                return Error(BoundaryFault(config=(self.tag, "outbound C# client credential; the companion serves insecure_loopback over UDS only")))
            case _ as unreachable:
                assert_never(unreachable)


class ServerHost:
    def __init__(self, bind: str, credential: Credential = Credential(insecure_loopback=True), grace: float = 5.0) -> None:
        self._bind, self._credential, self._grace = bind, credential, grace
        interceptor = aio_server_interceptor(filter_=filters.negate(filters.health_check()))
        self._server: grpc.aio.Server = grpc.aio.server(interceptors=[interceptor], compression=grpc.Compression.Gzip)

    @staticmethod
    def inbound(servicer_context: grpc.aio.ServicerContext) -> RuntimeRail[tuple[object, RuntimeContext]]:
        def admit() -> tuple[object, RuntimeContext]:
            carrier = {key: value for key, value in servicer_context.invocation_metadata()}
            frame = CausalFrame.of(
                int(carrier.get("rasm-hlc-physical", "0")), int(carrier.get("rasm-hlc-logical", "0")), carrier.get("rasm-tenant", "default")
            )
            context = Signals.continue_inbound({k: v for k, v in carrier.items() if k in ("traceparent", "tracestate")})
            return context, RuntimeContext.admit(RuntimeProfile.SIDECAR, causal=frame)

        return boundary("wire", admit)

    @staticmethod
    def enrich(descriptor: str, tenant: Tenant, fault_case: str) -> None:
        span = trace.get_current_span()
        span.set_attribute("rasm.descriptor", descriptor)
        span.set_attribute("rasm.tenant", str(tenant))
        span.set_attribute("rasm.fault_case", fault_case)

    async def serve(self) -> RuntimeRail[None]:
        match self._credential.server_credentials():
            case Ok(creds):
                self._server.add_secure_port(self._bind, creds)
                await self._server.start()
                async with anyio.create_task_group() as group:
                    group.start_soon(self._server.wait_for_termination)
                return Ok(None)
            case Error(fault):
                return Error(fault)
            case _ as unreachable:
                assert_never(unreachable)

    async def drain(self) -> None:
        await self._server.stop(self._grace)
```

## [03]-[CAPABILITY_INVOKE]

- Owner: `CapabilityInvoke` — the descriptor-driven polymorphic invoke decoding the C# `Rasm.AppHost/Agent/capability#SDK_CODEGEN` Python target; `DiscoveryResult` the descriptor catalog row the companion ingests from the C# `DiscoveryResultWire[]` projection; `CommandReceipt` the per-command evidence the companion reads back through the C# `ReceiptEnvelopeWire<CapabilityCommandReceiptWire>`. One invoke keyed by descriptor id discriminates every capability, never a per-service hand client. The OUTBOUND dispatch wraps its channel with `aio_client_interceptors(tracer_provider, filter_, request_hook, response_hook)` so the call propagates the active span through the `observability/telemetry#TELEMETRY`-installed composite and the client hooks enrich the CLIENT span with the descriptor id, the `clock#CLOCK` `Tenant`, and the fault case; the command request/receipt projection rides the `transport/wire#PROTO_TRANSCODE` `WireProtoCodec` seam, minting no fourth wire shape.
- Cases: the C# SDK Python target emits one method per descriptor — `def {surface}_{op}(self, args: CommandArguments) -> CommandReceipt: return self._run("{surface}.{op}", args)` — so the companion decodes the descriptor catalog into one `_run(descriptor_id, args)` dispatch where `descriptor_id` is the `{surface}.{op}` join the C# `CapabilityDescriptor.Of` mints; the per-descriptor `input_schema` is the `JsonSchemaExporter` JSON Schema the C# `SuiteContracts.Schema` derives from the descriptor's `CommandArguments`, so the companion validates the argument payload against the generated schema, never a hand-mirrored shape; the command-txn disposition decodes onto the `CommandTxn` struct whose `kind: CommandTxnKind` is the literal-discriminated disposition the C# `CommandTxn` mints — `committed` · `rolled_back` · `compensated` · `refused` — carried with its arm `detail`, the `CapabilityCommandReceiptWire.txn` kind raised onto the typed struct rather than an erased `dict`.
- Entry: `CapabilityInvoke.run` keys the descriptor by id, validates the `CommandArguments` payload against the descriptor's generated `input_schema` through `msgspec.json.decode`, projects the argument shape onto the `transport/wire#PROTO_TRANSCODE`-paired request message, dispatches over the gRPC stub, and decodes the `CommandReceipt` back; `CapabilityInvoke.discover` reads the descriptor catalog the C# `CapabilityRegistry.Discover(DiscoveryQuery.All)` emits so the companion command surface is generated from the descriptor rows, never hand-listed; `CapabilityInvoke.connect` is the single channel composer that mints the outbound `grpc.aio.insecure_channel` (loopback UDS) wrapped with the `interceptors(descriptor_id, tenant)` factory and folds the resulting traced channel into the `dispatch` coroutine `run` calls, so the descriptor-to-wire one-hop is composed on the page rather than threaded through an opaque pre-built callable; `CapabilityInvoke.interceptors` is the private factory `connect` consumes — it folds the catalogue `aio_client_interceptors` factory with the descriptor-id/`Tenant` `request_hook` and the fault-case `response_hook` so the composed channel wraps its stub once, the `request_hook` receiving `(span, request)` and the `response_hook` receiving `(span, details)`, the gRPC status-details string the interceptor awaits off the unary call (`""` on OK, the server's detail message on a non-OK status), both CLIENT-side per the catalogue hook law.
- Auto: the descriptor catalog crosses the wire as the `DiscoveryResultWire[]` array the C# `#TS_PROJECTION` projects — `(descriptor, surface, effect, idempotency, estimated, scope_hash)` — and the companion decodes it through `msgspec.json.Decoder(list[DiscoveryResult])`; the `effect`/`idempotency`/`cost-unit` keys decode as the C# smart-enum string keys (`pure`/`read`/`write`/`external`/`irreversible`, `idempotent`/`keyed`/`single-shot`/`non-idempotent`, `cpu-millis`/`wall-millis`/`bytes-egress`/`model-tokens`/`calls`); the cost vector decodes as the unit-keyed mapping the C# `CostVectorWire` emits; a new descriptor row regenerates the companion command surface with no per-capability hand edit because the catalog is the single descriptor source; the client hooks set `rasm.descriptor`/`rasm.tenant`/`rasm.fault_case` on the CLIENT span through `Span.set_attribute` inside the active span scope, never blocking, and the factory's `tracer_provider` defaults to `None`, resolving the global provider the `observability/telemetry#TELEMETRY` `set_tracer_provider` install registers (the catalogue provider law), so no second tracer mints.
- Packages: `msgspec` (`json.Decoder`/`json.decode`/`Struct`/`ValidationError` — resident `.api/msgspec.md`), `protobuf` (the `transport/wire#PROTO_TRANSCODE`-paired command request/receipt messages), `opentelemetry-instrumentation-grpc` (`aio_client_interceptors(tracer_provider, filter_, request_hook, response_hook)` interceptor factories, `filters` predicate algebra), `opentelemetry-api` (`trace.get_current_span`/`Span.set_attribute` for the hook enrichment — branch catalog `libs/python/.api/opentelemetry-api.md` trace ENTRYPOINTS [04]/[11], the trace-span members the resident `.api/opentelemetry-api.md` metrics/context scope does not carry), `grpcio` (`grpc.aio.insecure_channel` connect ENTRYPOINTS [02] for the loopback channel `connect` composes), `expression` (`Error`/`Ok`).
- Growth: a new capability is one descriptor row the C# registry already folds — the companion reads it through the existing `discover`/`run` pair, never a new method; a new effect/idempotency/cost key is one literal the decode union already discriminates; a new enriched span dimension is one `set_attribute` key in the existing hook, never a second interceptor; zero new surface, no per-service hand client.
- Boundary: the descriptor is the suite's only op-metadata owner and the C# registry is its sole mint — a per-capability hand client, a re-authored descriptor shape, a hand-mirrored argument schema, a second cost table, a hand-rolled tracing interceptor (the catalogue Reject row), and a second tracer are the deleted forms; the companion reads the descriptor, re-authoring no capability shape; the command dispatch lands on the gRPC stub through `transport/wire#PROTO_TRANSCODE` `WireProtoCodec`, so the capability invoke rides the existing proto wire and mints no fourth wire shape; the `Tenant` partition reads the one `clock#CLOCK` owner and re-mints no tenant scheme; the cross-language shape-identity is the C# `SuiteContracts.Schema` JSON Schema the C#, TS, and Python SDKs all bind, so an additive descriptor field admits on every consumer and a breaking reshape rejects on all three through the one `ContractGuard.AdditiveOnly` classifier.

```python signature
from collections.abc import Awaitable, Callable
from typing import Literal, assert_never

import grpc
import msgspec
from expression import Error, Ok
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.instrumentation.grpc import aio_client_interceptors, filters

from rasm.runtime.clock import Tenant
from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary, boundary


type CostUnitKey = Literal["cpu-millis", "wall-millis", "bytes-egress", "model-tokens", "calls"]
type CostVector = dict[CostUnitKey, int]
type CommandTxnKind = Literal["committed", "rolled_back", "compensated", "refused"]


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


class CapabilityInvoke:
    _catalog: msgspec.json.Decoder[list[DiscoveryResult]] = msgspec.json.Decoder(list[DiscoveryResult])
    _receipt: msgspec.json.Decoder[CommandReceipt] = msgspec.json.Decoder(CommandReceipt)

    def __init__(self, catalog: dict[str, DiscoveryResult], dispatch: Callable[[str, bytes], Awaitable[bytes]]) -> None:
        self._catalog_by_id, self._dispatch = catalog, dispatch

    @classmethod
    def discover(cls, payload: bytes) -> RuntimeRail[list[DiscoveryResult]]:
        return boundary("wire", lambda: cls._catalog.decode(payload))

    @staticmethod
    def interceptors(descriptor_id: str, tenant: Tenant) -> list[grpc.aio.ClientInterceptor]:
        def request_hook(span: trace.Span, _request: object) -> None:
            span.set_attribute("rasm.descriptor", descriptor_id)
            span.set_attribute("rasm.tenant", str(tenant))

        def response_hook(span: trace.Span, details: str) -> None:
            span.set_attribute("rasm.fault_case", details or "ok")

        return aio_client_interceptors(filter_=filters.negate(filters.health_check()), request_hook=request_hook, response_hook=response_hook)

    @classmethod
    def connect(cls, target: str, catalog: dict[str, DiscoveryResult], tenant: Tenant) -> "CapabilityInvoke":
        channel = grpc.aio.insecure_channel(target, interceptors=cls.interceptors("", tenant))

        async def dispatch(descriptor_id: str, request: bytes) -> bytes:
            return await channel.unary_unary(f"/rasm.capability/{descriptor_id}")(request)

        return cls(catalog, dispatch)

    async def run(self, descriptor_id: str, args: CommandArguments) -> RuntimeRail[CommandReceipt]:
        if descriptor_id not in self._catalog_by_id:
            return Error(BoundaryFault(wire=(descriptor_id, 0)))
        request = boundary("wire", lambda: msgspec.json.decode(args.payload, type=dict)).bind(
            lambda _: boundary("wire", lambda: msgspec.json.encode(args))
        )
        match request:
            case Error(_):
                return request
            case Ok(payload):
                dispatched = await async_boundary("wire", lambda: self._dispatch(descriptor_id, payload))
                return dispatched.bind(lambda raw: boundary("wire", lambda: self._receipt.decode(raw)))
            case _ as unreachable:
                assert_never(unreachable)
```

## [04]-[ENTRY]

- Owner: `Entrypoint` — the type-hint-driven `cyclopts` command axis backing the companion daemon's PRIVATE entry and package-internal entrypoints only; co-located with `ServerHost` because `serve` composes the host it launches.
- Entry: `companion_app` returns the `cyclopts.App` whose commands bind to the companion serve and drain; arguments bind from type hints, never from a hand-parsed `argv`; the serve command awaits the railed `ServerHost.serve`, which binds the socket through the credential fold before starting and awaiting termination, so a non-loopback credential surfaces as a typed fault at boot rather than a panic and the command body never touches an unrailed exception.
- Packages: `cyclopts` (`App`/`Parameter`).
- Growth: a new private command is one `@app.command` method on the existing app; zero new surface.
- Boundary: never a new public command surface — public commands are reserved to the suite Assay command surface; a public-facing CLI axis and a hand-parsed argument loop are the deleted forms.

```python signature
from cyclopts import App


def companion_app() -> App:
    app = App(name="rasm-companion", help="private companion daemon entry")

    @app.command
    async def serve(bind: str, *, grace: float = 5.0) -> RuntimeRail[None]:
        return await ServerHost(bind, Credential(insecure_loopback=True), grace).serve()

    return app
```

## [05]-[RESEARCH]

- [CREDENTIAL_PEM]: [COMPLETE] — decoded against the C# `Rasm.Compute/Runtime/channels#CALL_POLICY` `CredentialPolicy` five-row axis. The companion serves the local control plane over `RemoteTransport.UnixDomainSocket`, whose `Credentials` column is `Seq(CredentialPolicy.InsecureLoopback)` exactly, so the serve-side credential is `insecure_loopback` and peer identity is the kernel-reported `(pid, uid)` the C# `AppHost/companion-sidecar#PEER_ADMISSION` reads at accept (`SO_PEERCRED` on Linux, `LOCAL_PEERCRED`+`LOCAL_PEERPID` on macOS), never a wire PEM. The serve credential is `grpc.local_server_credentials(grpc.LocalConnectionType.UDS)` — `grpc.local_server_credentials` (`.api/grpcio.md` credential ENTRYPOINTS [03]) and `grpc.LocalConnectionType.UDS` (credential PUBLIC_TYPES [03]) are the cataloged loopback authenticator pair, returning a `grpc.ServerCredentials` (credential PUBLIC_TYPES [01]). The four outbound rows (`tls`/`mtls`/`bearer`/`composed`) are the OUTBOUND C# client legs (`ChannelCredentials.SecureSsl`/`CallCredentials.FromInterceptor`/`CallCredentials.Compose`) carrying PEM/token material the companion never serves — the prior `\n--SEP--\n` PEM-split placeholder was a fabricated serve-side PEM bundle and is the deleted form. The `Credential.server_credentials` fold is total over the five producer rows by `match`/`assert_never` and returns the four outbound rows as `Error(BoundaryFault(config=...))` on the `RuntimeRail` rather than raising a bare `ValueError`, so a credential the UDS serve leg cannot admit is a typed rail failure threaded through `ServerHost.serve`.
- [GRPC_CORE_COMPOSE]: [COMPLETE] — reflection-confirmed against `.api/grpcio.md`. `grpcio` 1.81.1 is `[CORE]` and ungated, resolving on cp315 transitively via `google-cloud-storage` (`grpcio-status`/`grpc-google-iam-v1`) and declared as an explicit ungated core direct dependency for the transport rail. `ServerHost` composes `grpc.aio.server(interceptors=[...], compression=grpc.Compression.Gzip)` (serve ENTRYPOINTS [01]), the `grpc.aio.Server` lifecycle `add_secure_port`/`start`/`stop`/`wait_for_termination` (lifecycle ENTRYPOINTS [01]/[03]/[04]/[05]), `grpc.local_server_credentials(grpc.LocalConnectionType.UDS)` (credential ENTRYPOINTS [03] + PUBLIC_TYPES [03], returning `grpc.ServerCredentials` PUBLIC_TYPES [01]), `grpc.aio.ServicerContext.invocation_metadata`/`.abort` (PUBLIC_TYPES [03]), `grpc.StatusCode` (PUBLIC_TYPES [01]), and `grpc.Compression.Gzip` (PUBLIC_TYPES [02]) per the `[04]-[IMPLEMENTATION_LAW]` serve/credential/context/interceptor/compression rows — one `grpc.aio.server` with construction-time interceptors, `ssl_*`/`local_*` credentials from the one `Credential` admission point, status set on the context rather than raised across the wire, tracing interceptors settled from the OTel grpc owner, and the `grpc.Compression` member selecting the algorithm without re-implementing the C-core codec. Only `grpcio-tools` (the `protoc` codegen companion) remains `[GATED]` `python_version<'3.15'`; the cp315 core imports `grpc`/`grpc.aio` directly and never imports `grpc_tools`, so server boot never depends on the companion environment.
- [INBOUND_FRAME_RAIL]: [COMPLETE] — the prior `ServerHost.inbound` returned `tuple[object, RuntimeContext]` directly and called `int(carrier.get("rasm-hlc-physical", "0"))` unguarded, so a malformed `rasm-hlc-physical`/`rasm-hlc-logical` slot raised `ValueError` inside the servicer entry, a bare exception in domain flow. The decode now runs inside one `boundary("wire", ...)` fence returning `RuntimeRail[tuple[object, RuntimeContext]]` — the string-to-int parse and the `clock#CLOCK` `CausalFrame.of` lift live behind the fence, so a malformed inbound frame returns `Error(BoundaryFault(boundary=("wire", "ValueError")))` on the rail — the `boundary` case with subject `"wire"` that the `reliability/faults#FAULT` conversion surface mints (never the code-carrying `wire` case, which is reserved for explicit `BoundaryFault(wire=(subject, code))` construction), never an unhandled servicer panic. `CausalFrame.of` (the `clock#CLOCK` `[02]-[CLOCK]` constructor) receives validated ints and never sees a raw string, the boundary fence owning the parse.
- [CAPABILITY_INVOKE]: [COMPLETE] — decoded against the C# `Rasm.AppHost/Agent/capability#SDK_CODEGEN` Python target (`def {method}(self, args: CommandArguments) -> CommandReceipt: return self._run("{descriptor}", args)`) and the `#TS_PROJECTION` `DiscoveryResultWire`/`CapabilityCommandReceiptWire` shapes. `CapabilityInvoke.discover` decodes the catalog through `msgspec.json.Decoder(list[DiscoveryResult])` and `CapabilityInvoke.run` is the one polymorphic invoke — descriptor-id lookup against the frozen catalog (an absent id is a typed `wire` fault), `msgspec.json.decode` schema validation of the `CommandArguments` payload, `async_boundary` dispatch over the injected stub coroutine, and `_receipt.decode` of the `CommandReceipt` back, all on the `RuntimeRail`. `DiscoveryResult` matches the producer `DiscoveryResult` (`descriptor, surface, effect, idempotency, estimated, scope_hash`) field-for-field and the `committed`/`rolled_back`/`compensated`/`refused` `CommandTxnKind` mirrors the producer `CommandTxn` `Committed`/`RolledBack`/`Compensated`/`Refused` arms and lands on the typed `CommandTxn` struct's `kind` discriminant plus its `detail` payload, the `CapabilityCommandReceiptWire.txn` decoded onto the typed struct rather than an erased mapping, never a re-minted discriminant. The companion re-authors no descriptor shape and the command request/receipt projection rides the `transport/wire#PROTO_TRANSCODE` codec, minting no fourth wire shape. Settled; awaits only the live descriptor catalog emission at SDK-bootstrap.
- [GRPC_CLIENT_AND_HOOKS]: [COMPLETE] (design) — reflection-confirmed against `.api/opentelemetry-instrumentation-grpc.md`. CLIENT enrichment rides `aio_client_interceptors(tracer_provider, filter_, request_hook, response_hook)` (interceptor factories, the only factory carrying hook params) on the `[3]-[CAPABILITY_INVOKE]` outbound dispatch and any host call-back channel — the `request_hook` reads `(span, request)` and sets `rasm.descriptor`/`rasm.tenant`, the `response_hook` reads `(span, details)` (the unary `call.details()` status-details string, empty on OK) and sets `rasm.fault_case`, both CLIENT-side per the hook law. `CapabilityInvoke.connect` is the single channel composer that mints the loopback `grpc.aio.insecure_channel(target, interceptors=interceptors(...))` (channel ENTRYPOINTS [02]) and folds the interceptor-wrapped channel into the `dispatch` coroutine `run` awaits, so the descriptor-to-traced-wire one-hop is composed on the page and `interceptors` is never a dangling factory — the channel, the interceptor wrap, and the dispatch are one construction. SERVER enrichment is a span-attribute enricher: `ServerHost.enrich` calls `trace.get_current_span().set_attribute(...)` (NOT a server-interceptor hook — `aio_server_interceptor(tracer_provider=None, filter_=None)` takes NO hook params, and the catalogue Reject row forbids a hand-rolled tracing interceptor) AFTER the catalogue `aio_server_interceptor` opens the SERVER span, inside the active span scope, never blocking. `tracer_provider` is the `observability/telemetry#TELEMETRY`-installed provider so no second tracer mints; the outbound interceptors compose the same `Propagators.DefaultTextMapPropagator` the inbound `observability/receipts#RECEIPT` `continue_inbound` extract reads, so inbound and outbound legs share one trace (the mechanical reason this sequences after `TRACE_INBOUND_EXTRACT`). Realizes the cross-`libs/` `ONE_DISTRIBUTED_TRACE` outbound Python leg. Spellings settled.
- [CLOCK_REHOME]: [COMPLETE] — `Hlc`/`ElementId`/`Tenant`/`CausalFrame` formerly defined inline on this page now live in the `clock#CLOCK` owner (`libs/python/runtime/.planning/clock/clock.md`), so `ServerHost.inbound` imports `CausalFrame`/`Tenant` from `rasm.runtime.clock` and lifts the inbound two-half stamp through `CausalFrame.of`, re-minting no stamp. The `transport/wire#PROTO_TRANSCODE`/`#CRDT_DECODE` codec (`WireProtoCodec`/`CrdtOp`/`CrdtOpDecode`) formerly inline on this page now lives in the `transport/wire` owner, so this page composes the codec at the servicer seam rather than carrying the codec body — the serve page owns only `ServerHost`/`Credential`/`CapabilityInvoke`/`Entrypoint`, the codec and clock being separate single-mint owners this page consumes.
- [WIRE_REHOME]: [COMPLETE] — the `[02]-[PROTO_TRANSCODE]` and `[03]-[CRDT_DECODE]` sections this page formerly carried (the `WireProtoCodec` protobuf seam and the `CrdtOp`/`CrdtOpWire`/`CrdtOpDecode` MessagePack op-log decode with the `DecompressFn` seam) re-homed to the `transport/wire` owner in the R0 collapse, so the serve page no longer carries the full codec body inline; the servicers transcode at the `transport/wire#PROTO_TRANSCODE` seam and the durability decode rides `transport/wire#CRDT_DECODE`, both consumed here and re-minted nowhere. The `lz4` decompress decision (`MessagePackCompression.Lz4BlockArray` envelope, `DecompressFn`-injected, gated `python_version<'3.15'`) is the `transport/wire#CRDT_DECODE` `[CRDT_DECODE_LZ4]` upstream-blocked gate, not a serve-page concern.
- [KEYRING_CATALOGUE]: [DROPPED] (serve leg) — the prior `keyring.get_password` resolution was the serve-side PEM-bundle path the `[CREDENTIAL_PEM]` decode deletes; the companion UDS serve leg reads no OS keyring (peer identity is the kernel accept-time credential), so the `keyring` package leaves the SERVE owner. `keyring` is NOT folder-orphaned — it lands the one `execution/admission#SETTINGS` `SecretBoundary` reading OS-keystore credentials for the OUTBOUND transport legs (`transport/roots#RESOURCE` `ssh` passphrase, `http` bearer), profile-gated and lazy on the outbound leg; the serve leg admits no keyring lookup.
