# [PY_RUNTIME_SERVE]

Companion server-host and daemon composition root: `ServerHost` owns the inbound Connect lifecycle — every generated `<Svc>ASGIApplication` constructed here over its servicer, mounted under one dispatcher, and served by hypercorn — and the interceptor tuple every rpc crosses: metadata admission once per call and descriptor-driven constraint validation on every request and response body. `ServerHost` is itself the generated `grpc.health.v1` `Health` servicer, so the host's serving state publishes through the same generated seam every dialer reaches. `CapabilityInvoke` is the descriptor-driven outbound dial over the C#-generated capability SDK, and `Entrypoint` the daemon boot/serve/drain choreography. It hosts the geometry companion daemon's unary `ComputeService.Tessellate`, server-streaming `ArtifactService.Fetch`, and client-streaming `ArtifactService.Put` over the corpus Connect contract on the UDS leg and re-mints nothing it composes: hypercorn owns the plaintext UNIX-socket ASGI bind and answers gRPC by prior-knowledge h2c, Connect over h2 and HTTP/1.1, and gRPC-Web with no further config; the socket path stays under the `AF_UNIX` `sun_path` limit or the bind refuses with `AF_UNIX path too long`. Connect's server and client both run asyncio loop primitives, so this host serves under `anyio.run(..., backend="asyncio")` alone.

## [01]-[INDEX]

- [02]-[SERVE]: inbound host lifecycle over the generated applications under the profile's `WirePolicy`, metadata admission, body constraint validation, the two-directional `CredentialPolicy`, and typed error-detail egress.
- [03]-[CAPABILITY_INVOKE]: descriptor-driven outbound dial, the `FaultDetail` detail ingress, and the two-axis re-drive gate over the `WIRE` retry row.
- [04]-[ENTRY]: daemon composition root — railed boot under the structure-first wire gates, supervised serve, the ordered drain, and the one-shot recipe command.

## [02]-[SERVE]

- Owner: `ServerHost` is the boundary capsule over one hypercorn `Config` and the `Served` rows a composition root hands it — each row a GENERATED `<Svc>ASGIApplication` class beside the servicer implementing the generated `<Svc>` protocol, constructed by `mount` with the shared interceptor tuple and the profile's `WirePolicy` (the decompressed-body ceiling `read_max_bytes` and the `zstd`-then-`gzip` compression roster), mounted at its own `path` under `DispatcherMiddleware`, and wrapped once by `OpenTelemetryMiddleware`; a hand `Route` table, a `register(routes)` fold, and a servicer-body `_invoke` have no seat, because the generated interfaces type every request and response end to end and Connect owns decode, framing, and streaming. It composes the `evidence/clock#CLOCK` `CausalFrame.decode` sole carrier fence, the admitted `RuntimeContext`, and the generated error details, re-minting none.
- Cases: `CredentialPolicy` mints the contract five-row axis under one spelling on both sides of the wire, and each row is constructible at exactly the end it serves through the two projections `server_config`/`client_transport`, each refusing the opposite direction by name. `insecure_loopback` is the one row the UDS serve leg admits — peer identity is the kernel-reported `(pid, uid)` the C# `dotnet:Rasm.AppHost/Wire/companion#PEER_ADMISSION` reads at accept through `SO_PEERCRED`/`LOCAL_PEERCRED`, never a wire-carried PEM — and it lands on `Config.bind`, because a host with no TLS material binds its `bind` roster plaintext and ignores `insecure_bind` whole. Four outbound rows are the client legs a calling host dials: `tls` the verified roots, `mtls` the full `(roots, chain, key)` triple the `HTTPTransport` binds, `bearer` the per-call token a client `MetadataInterceptor` stamps over the ambient roots, and `composed` the arity-proved bundle of one transport row with its call rows. Every dial therefore names a posture or refuses; no path reaches a plaintext TCP transport.
- Law: EVERY fence on this page names the provider classes it reaches and rides a rostered raise. `catch` carries no default at the `reliability/faults#FAULT` owner, so this module spells its three planes once — `_WIRE_RAISES` for the Connect-and-socket surface both the serve leg and the outbound dial reach, `_HOST_RAISES` for the hypercorn lifespan surface, `_ENCODE_RAISES` for the generated class's encode-time validation — and the daemon drain fold is the ONE catch-all the plane is allowed, because its stages are caller-supplied owners whose raise surface this runtime cannot enumerate. Every refusal resolves a `FaultRow` anchor and derives its subject from that row's own `Leg`, so a fence cannot spell a coordinate its package never declared.
- Law: the serving map keys on the SERVICE NAME — the generated application's `path` less its leading slash, which is byte-identical to the `WireService` member text `transport/shapes#BOOT_CENSUS` proves against that same application — so the supervisor's `status(subject, serving)` flip and a dialer's `check(service)` read one key; a slashed or hand-spelled key would flip a phantom row that no probe reads, and the census refuses it at boot.

Entry: the method roster:

| [INDEX] | [METHOD]                                                        | [CONTRACT]                                                  |
| :-----: | :-------------------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `mount(served) -> RuntimeRail[int]`                             | construct each application under the `WirePolicy`; one tree |
|  [02]   | `admitted() -> RuntimeContext`                                  | the context the interceptor bound for the live rpc          |
|  [03]   | `settle(rail) -> T`                                             | rail-terminating fold; Connect error with `FaultDetail`     |
|  [04]   | `Admission.on_start(ctx) / on_end(token, ctx, error)`           | metadata admission, deadline, contextvars, timing, span     |
|  [05]   | `BodyAdmission(SERVER)`                                         | validate every request and response element by rpc arity    |
|  [06]   | `serve() -> RuntimeRail[None]`                                  | bind UDS under hypercorn, flip `SERVING`, await the trigger |
|  [07]   | `drain() -> None`                                               | `NOT_SERVING` first, then the shutdown trigger and grace    |
|  [08]   | `status(service, serving) -> None`                              | supervisor flip keyed on the `WireService` text; one bool   |
|  [09]   | `check(request, ctx) -> HealthCheckResponse`                    | generated `Health.check`: the serving map per service       |
|  [10]   | `CredentialPolicy.server_config(config) -> RuntimeRail[Config]` | loopback → plaintext `bind`; TLS rows assign cert material  |
|  [11]   | `CredentialPolicy.client_transport() -> RuntimeRail[Dial]`      | four outbound rows; loopback refuses as an inbound seat     |

 `serve()` refuses an empty `Served` set with a typed `config` fault — never a silent empty bind — binds and listens its sockets ahead of the `ready` hook so an sd-notify READY never precedes an accepting listener, and awaits the shutdown trigger directly; supervision is the `[04]-[ENTRY]` composition root's. `Admission.on_start` lifts `ctx.timeout_ms` into the admitted `Deadline`, feeding the caller-dialed budget to the deadline rail — never an unbounded handler — and records every rpc's duration and outcome onto `Metrics.record`. The `transport/body`-owned `BodyAdmission(SERVER)` interceptor evaluates generated descriptor constraints on every unary body and every streamed element: request refusals cross as `INVALID_ARGUMENT`, response refusals as `INTERNAL`, and structured request violations survive on the `ConnectError`. A server-stream fault mid-stream crosses the same typed terminal as a unary fault. `status` is the one worker-facing flip the `execution/workers#SUPERVISION` actuator drives, so pool liveness advertises through the same host a calling host polls and no second health surface exists.

- Auto: `OpenTelemetryMiddleware` is the ONE trace-context authority — it extracts the inbound W3C parent off `scope["headers"]` and opens the SERVER span natively with `exclude_spans=["receive", "send"]` so a streaming rpc multiplies no child spans, `Admission` re-extracts nothing, no handler body opens a second scope, and an observe `attach` around the handler body re-roots spans the middleware already parented. `server_request_hook` stamps `rpc.service`/`rpc.method` off the `/<package>.<Service>/<Method>` path, so the per-call dimension rides the middleware natively. `Admission.on_start` binds the admitted context onto structlog contextvars for the rpc window, so `merge_contextvars` stamps every handler log line with the same admitted identity whichever arity served it.
- Auto: the generated `<Svc>` protocol declares each rpc's shape (`async def` unary, `AsyncIterator` server stream, `AsyncIterator` in and out for bidi) and the generated application binds the matching `Endpoint` kind. `BodyAdmission` implements those four native interceptor protocols in `transport/body` and no servicer repeats constraint logic; `settle` remains the one rail fold a handler body composes.
- Auto: `ServerHost` implements the selected generated `Health.Check` rpc — `serve()` mounts `HealthASGIApplication(self, interceptors=_INTERCEPTORS)` beside every served application, and `check` projects the serving map per service (an empty name answers the host whole, an unknown name `SERVICE_UNKNOWN`) — so a supervisor publishes on the same generated seam every dialer polls, the health application crosses the same admission and validation boundaries as every served rpc, and no hand-written health route exists; `Watch` remains upstream support closure but has no selected actor or runtime implementation. `connectrpc` ships no health surface, which is why the service is the vendored `grpc/health/v1/health.proto` module generated for this branch.
- Growth: a new served service is one `Served` row — the generated `<Svc>ASGIApplication` class beside its servicer — the composition root hands `mount`, with its `SERVICE_VOCABULARY` row; a new rpc or constraint is one proto edit and regeneration, with validation inherited at the body boundary; a new `FaultTag` member is one `_FAULT_STATUS` row and nothing else, the table being total by construction and read as a raw index, and that row carries its own re-drive membership with it because `_REDRIVEN` derives from it; a new refusal on this page is one `FaultRow` anchor at the `reliability/faults#FAULT` roster called through `raised`, never a literal construction; a new outbound credential posture is one `CredentialPolicy` case with one `client_transport` arm; a new span dimension is one `enrich` key; a new compression algorithm is one `Compression` value on the `WirePolicy` row both `mount` and `CapabilityInvoke.connect` read; a new health rpc is one upstream proto change the regenerated `health_connect` protocol carries and one override on `ServerHost`.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Awaitable, Callable, Iterable
from contextvars import ContextVar
from datetime import timedelta
from time import perf_counter, time_ns
from typing import Any, Final, Literal, Protocol, Self, assert_never, override

import anyio
import msgspec
from connectrpc.code import Code
from connectrpc.codec import Codec
from connectrpc.compression import Compression
from connectrpc.compression.gzip import GzipCompression
from connectrpc.compression.zstd import ZstdCompression
from connectrpc.errors import ConnectError
from connectrpc.interceptor import Interceptor, MetadataInterceptor
from connectrpc.request import RequestContext
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from hypercorn.asyncio import worker_serve
from hypercorn.config import Config
from hypercorn.middleware import DispatcherMiddleware
from hypercorn.typing import ASGIReceiveCallable, ASGISendCallable, Scope as AsgiScope
from hypercorn.utils import LifespanFailureError, LifespanTimeoutError, wrap_app
from opentelemetry import trace
from opentelemetry.instrumentation.asgi import OpenTelemetryMiddleware
from protobuf import Message
from pyqwest import Client, HTTPTransport
# Contracts are retired from this logic.
import structlog

from rasm.runtime.admission import Deadline, RuntimeContext, RuntimeProfile
from rasm.runtime.clock import CausalFrame
from rasm.runtime.faults import (
    FAULT_OWNER, FAULT_TAG, SERVE_ANCHOR, SERVE_BUNDLE, SERVE_DIRECTION, SERVE_ROSTER,
    BoundaryFault, Catch, FaultTag, Recovery, RuntimeLeg, RuntimeRail,
)
from rasm.runtime.journal import NANOS_PER_TICK
from rasm.runtime.metrics import FAULT_OUTCOME, Metrics
from rasm.runtime.observe import DrainOutcome
from rasm.runtime.shapes import RecoveryCell
from rasm.runtime.transport.body import AdmissionSide, BodyAdmission

# --- [TYPES] ----------------------------------------------------------------------------

type Dial = tuple[HTTPTransport, tuple[MetadataInterceptor, ...]]


class Mounted(Protocol):
    @property
    def path(self) -> str: ...
    async def __call__(self, scope: AsgiScope, receive: ASGIReceiveCallable, send: ASGISendCallable) -> None: ...


class Application[S](Protocol):
    def __call__(
        self,
        service: S,
        /,
        *,
        interceptors: Iterable[Interceptor] = (),
        read_max_bytes: int | None = None,
        compressions: Iterable[Compression] | None = None,
        codecs: Iterable[Codec] | None = None,
    ) -> Mounted: ...


type Served[S] = tuple[Application[S], S]

# --- [CONSTANTS] ------------------------------------------------------------------------

_DESCRIPTOR_ATTR: Final[str] = "rasm.descriptor"
_ENCODE_RAISES: Final[Catch] = (TypeError, ValueError, OverflowError)
_ZSTD: Final[Compression] = ZstdCompression()
_GZIP: Final[Compression] = GzipCompression()
_WIRE_RAISES: Final[Catch] = (ConnectError, OSError)
_HOST_RAISES: Final[Catch] = (LifespanTimeoutError, LifespanFailureError, OSError)

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class CredentialPolicy:
    tag: Literal["insecure_loopback", "tls", "mtls", "bearer", "composed"] = tag()
    insecure_loopback: bool = case()
    tls: str = case()
    mtls: tuple[str, str, str] = case()
    bearer: str = case()
    composed: tuple["CredentialPolicy", ...] = case()

    @classmethod
    def loopback(cls) -> Self:
        return cls(insecure_loopback=True)

    @classmethod
    def bundled(cls, *rows: "CredentialPolicy") -> Self:
        return cls(composed=rows)

    def server_config(self, config: Config, bind: str) -> RuntimeRail[Config]:
        match self:
            case CredentialPolicy(tag="insecure_loopback"):
                config.bind = [bind]
                return Ok(config)
            case CredentialPolicy(tag="tls" | "mtls" | "bearer" | "composed" as outbound):
                return Error(SERVE_DIRECTION.raised(outbound, "inbound"))
            case _ as unreachable:
                assert_never(unreachable)

    def client_transport(self) -> RuntimeRail[Dial]:
        match self:
            case CredentialPolicy(tag="tls", tls=roots):
                return Ok((HTTPTransport(tls_ca_cert=roots.encode()), ()))
            case CredentialPolicy(tag="mtls", mtls=(roots, chain, key)):
                return Ok((HTTPTransport(tls_ca_cert=roots.encode(), tls_cert=chain.encode(), tls_key=key.encode()), ()))
            case CredentialPolicy(tag="bearer", bearer=token):
                return Ok((HTTPTransport(tls_include_system_certs=True), (_Bearer(token),)))
            case CredentialPolicy(tag="composed", composed=rows):
                return _bundled(Block.of_seq(rows))
            case CredentialPolicy(tag="insecure_loopback"):
                return Error(SERVE_DIRECTION.raised("insecure_loopback", "outbound"))
            case _ as unreachable:
                assert_never(unreachable)


class WirePolicy(msgspec.Struct, frozen=True, gc=False):
    read_max_bytes: int
    compressions: tuple[Compression, ...]


class _Bearer(msgspec.Struct, frozen=True, gc=False):
    token: str

    async def on_start(self, ctx: RequestContext[Message, Message]) -> None:
        ctx.request_headers["authorization"] = f"Bearer {self.token}"

    async def on_end(self, token: None, ctx: RequestContext[Message, Message], error: Exception | None) -> None:
        return


# --- [TABLES] ---------------------------------------------------------------------------

_WIRE_POLICY: Final[Map[RuntimeProfile, WirePolicy]] = Map.of_seq([
    (RuntimeProfile.TOOL, WirePolicy(read_max_bytes=64 << 20, compressions=(_ZSTD, _GZIP))),
    (RuntimeProfile.SIDECAR, WirePolicy(read_max_bytes=1 << 30, compressions=(_ZSTD, _GZIP))),
    (RuntimeProfile.PACKAGE, WirePolicy(read_max_bytes=64 << 20, compressions=(_ZSTD, _GZIP))),
    (RuntimeProfile.TEST, WirePolicy(read_max_bytes=64 << 20, compressions=(_ZSTD, _GZIP))),
])

_FAULT_STATUS: Final[Map[FaultTag, Code]] = Map.of_seq([
    ("config", Code.FAILED_PRECONDITION),
    ("resource", Code.UNAVAILABLE),
    ("deadline", Code.DEADLINE_EXCEEDED),
    ("api", Code.INTERNAL),
    ("import_", Code.UNIMPLEMENTED),
    ("wire", Code.INVALID_ARGUMENT),
    ("boundary", Code.INVALID_ARGUMENT),
    ("domain", Code.FAILED_PRECONDITION),
    ("aggregate", Code.INTERNAL),
])

_REDRIVEN: Final[frozenset[FaultTag]] = frozenset(tag for tag, status in _FAULT_STATUS.items() if status in (Code.UNAVAILABLE, Code.DEADLINE_EXCEEDED))

_CODE_OUTCOME: Final[Map[Code, DrainOutcome]] = Map.of_seq(
    (status, FAULT_OUTCOME.try_find(tag).default_value("rejected")) for tag, status in _FAULT_STATUS.items()
)
_MESSAGE_KEYS: Final[tuple[str, ...]] = ("detail", "cause", "case", "members", "subject")

_SERVING: Final[Map[bool, HealthCheckResponse.ServingStatus]] = Map.of_seq([
    (True, HealthCheckResponse.ServingStatus.SERVING),
    (False, HealthCheckResponse.ServingStatus.NOT_SERVING),
])

_ADMITTED: Final[ContextVar[RuntimeContext]] = ContextVar("rasm.runtime.serve.admitted")

# --- [SERVICES] -------------------------------------------------------------------------


class Admission:
    async def on_start(self, ctx: RequestContext[Message, Message]) -> tuple[RuntimeContext, float, object]:
        carrier = dict(ctx.request_headers.items())
        budget = Option.of_optional(ctx.timeout_ms).map(lambda remaining: Deadline(timedelta(milliseconds=remaining)))
        context = ServerHost.settle(
            CausalFrame.decode(carrier).map(
                lambda causal: RuntimeContext.admit(RuntimeProfile.SIDECAR, deadline=budget.to_optional(), causal=causal, carrier=carrier)
            )
        )
        token = _ADMITTED.set(context)
        structlog.contextvars.bind_contextvars(**context.attribute())
        return context, perf_counter(), token

    async def on_end(self, token: tuple[RuntimeContext, float, object], ctx: RequestContext[Message, Message], error: Exception | None) -> None:
        context, started, reset = token
        outcome: DrainOutcome = (
            "completed"
            if error is None
            else _CODE_OUTCOME.try_find(error.code).default_value("rejected")
            if isinstance(error, ConnectError)
            else "rejected"
        )
        Metrics.record((perf_counter() - started) * 1000.0, method=ctx.method.name, outcome=outcome)
        ServerHost.enrich(context, ctx.method.name)
        structlog.contextvars.unbind_contextvars(*context.attribute().keys())
        _ADMITTED.reset(reset)


_INTERCEPTORS: Final[tuple[Interceptor, ...]] = (Admission(), BodyAdmission(AdmissionSide.SERVER))


class ServerHost(Health):
    def __init__(self, bind: str, credential: CredentialPolicy | None = None, grace: float = 5.0, *, profile: RuntimeProfile) -> None:
        self._bind, self._credential, self._grace, self._policy = bind, credential or CredentialPolicy.loopback(), grace, _WIRE_POLICY[profile]
        self._applications: Block[Mounted] = Block.empty()
        self._serving: Map[str, bool] = Map.empty()
        self._shutdown = anyio.Event()

    def mount(self, served: Block[Served[Any]]) -> RuntimeRail[int]:
        self._applications = self._applications.append(served.map(lambda row: self._constructed(*row)))
        return Ok(len(served))

    def _constructed(self, application: Application[Any], servicer: Any) -> Mounted:
        return application(servicer, interceptors=_INTERCEPTORS, read_max_bytes=self._policy.read_max_bytes, compressions=self._policy.compressions)

    @staticmethod
    def admitted() -> RuntimeContext:
        return _ADMITTED.get()

    @staticmethod
    def enrich(context: RuntimeContext, descriptor: str, refused: Option[BoundaryFault] = Nothing) -> None:
        seat = refused.map(lambda fault: {FAULT_TAG: fault.tag} | fault.owner.map(lambda leg: {FAULT_OWNER: leg}).default_value({}))
        trace.get_current_span().set_attributes(context.attribute() | {_DESCRIPTOR_ATTR: descriptor} | seat.default_value({}))

    @staticmethod
    def settle[T](wired: RuntimeRail[T]) -> T:
        match wired:
            case Result(tag="ok", ok=payload):
                return payload
            case Result(tag="error", error=fault):
                context = _ADMITTED.get(RuntimeContext.admit(RuntimeProfile.SIDECAR))
                ServerHost.enrich(context, "settle", Some(fault))
                status = _FAULT_STATUS[fault.tag]
                raise ConnectError(status, "; ".join(f"{k}={v}" for k, v in fault.facts().items()), details=_details(fault, context))
            case _ as unreachable:
                assert_never(unreachable)

    async def serve(self, ready: Callable[[], Awaitable[None]] | None = None) -> RuntimeRail[None]:
        if self._applications.is_empty():
            return Error(SERVE_ROSTER.raised())
        match self._credential.server_config(Config(), self._bind):
            case Result(tag="error") as refused:
                return refused
            case Result(tag="ok", ok=config):
                config.graceful_timeout = self._grace
                health = self._constructed(HealthASGIApplication, self)
                app = OpenTelemetryMiddleware(
                    DispatcherMiddleware({application.path: application for application in (*self._applications, health)}),
                    exclude_spans=["receive", "send"],
                    server_request_hook=_rpc_attributes,
                )
                sockets = config.create_sockets()
                for sock in (*sockets.insecure_sockets, *sockets.secure_sockets):
                    sock.listen(config.backlog)
                self._serving = Map.of_seq((_service(application), True) for application in self._applications)
                if ready is not None:
                    await ready()
                await worker_serve(wrap_app(app, config.wsgi_max_body_size, "asgi"), config, sockets=sockets, shutdown_trigger=self._shutdown.wait)
                return Ok(None)
            case _ as unreachable:
                assert_never(unreachable)

    async def status(self, service: str, serving: bool) -> None:
        self._serving = self._serving.add(service, serving) if not self._shutdown.is_set() else self._serving

    async def drain(self) -> None:
        self._serving = Map.of_seq((service, False) for service in self._serving.keys())
        self._shutdown.set()

    @override
    async def check(self, request: HealthCheckRequest, ctx: RequestContext[HealthCheckRequest, HealthCheckResponse]) -> HealthCheckResponse:
        _ = ctx
        return HealthCheckResponse(status=self._status(request.service))

    def _status(self, service: str) -> HealthCheckResponse.ServingStatus:
        whole = not self._serving.is_empty() and all(self._serving.values())
        return (
            self._serving.try_find(service).map(_SERVING.__getitem__).default_value(HealthCheckResponse.ServingStatus.SERVICE_UNKNOWN)
            if service
            else _SERVING[whole]
        )

# --- [OPERATIONS] -----------------------------------------------------------------------


def _service(application: Mounted) -> str:
    return application.path.removeprefix("/")


def _rpc_attributes(span: trace.Span, scope: dict[str, object]) -> None:
    service, _, method = str(scope.get("path", "")).strip("/").rpartition("/")
    span.set_attributes({"rpc.system": "connect_rpc", "rpc.service": service, "rpc.method": method})


def _bundled(rows: Block[CredentialPolicy]) -> RuntimeRail[Dial]:
    anchors, tokens = rows.filter(lambda row: row.tag in ("tls", "mtls")), rows.filter(lambda row: row.tag == "bearer")
    return (
        Error(SERVE_BUNDLE.raised(",".join(sorted({row.tag for row in rows}))))
        if len(anchors) + len(tokens) != len(rows)
        else Error(SERVE_ANCHOR.raised(str(len(anchors))))
        if len(anchors) != 1
        else anchors.head().client_transport().map(lambda dial: (dial[0], tuple(_Bearer(row.bearer) for row in tokens)))
    )


def _details(fault: BoundaryFault, context: RuntimeContext) -> tuple[Message, ...]:
    sealed = _sealed(fault, context, fault.retriability(_REDRIVEN))
    return RecoveryCell.advice(sealed.recovery).map(lambda advice: (sealed, advice)).default_value((sealed,))


def _sealed(fault: BoundaryFault, context: RuntimeContext, recovery: Recovery) -> FaultDetail:
    reason = fault.defect.default_value(fault.tag)
    return FaultDetail(
        domain=fault.owner.default_value(RuntimeLeg.SERVE.value),
        case=fault.ordinal.default_value(0),
        correlation=context.correlation.trace_id,
        stamp=context.causal.map(lambda frame: Hlc(physical=frame.hlc.physical_ticks, logical=frame.hlc.logical)).default_with(
            lambda: Hlc(physical=time_ns() // NANOS_PER_TICK, logical=0)
        ),
        tenant=context.causal.map(lambda frame: str(frame.tenant)).default_value(""),
        recovery=RecoveryCell.of(recovery),
        violations=[BadRequest.FieldViolation(field=slot, description=value, reason=reason) for slot, value in fault.coordinates],
    )
```

## [03]-[CAPABILITY_INVOKE]

- Owner: `CapabilityInvoke` discovers through generated `capability.CapabilityDiscoveryService`, verifies the returned catalog against the SDK pin, then dispatches generated request and reply messages over the brokered capability path.
- Law: RE-DRIVE IS TWO AXES AND NEVER A BOOL — the class's route (the Connect status the `RetryClass.WIRE` row grades) crossed with the producer's own `Recovery` verdict off the decoded `FaultDetail`, and `_redrive` is the one place both are readable at once. `terminal` spends no further attempt even inside the transient trio, `throttled` rides back as the delay `stamina` waits in place of its own curve, and every other verdict defers to the row, so the overlay narrows or re-times the class curve and never widens it. `wire_detail` is the ONE detail read and `_stated` the ONE verdict lift over it, both composed by the gate and the public `remote_fault` terminal lift alike, so the posture a retry acted on and the fault a caller receives can never be decoded two different ways. That verdict is the THIRD precedence rung the fault family declares and cannot carry: `reliability/faults#FAULT` owns the two rungs it can see and routes the peer-stated one here, where a live `ConnectError` exists to read; absence rides `Option` rather than a fourth case, so an unstated posture never masquerades as a stated one.
- Entry: `CapabilityInvoke.connect(target, expected_pin, tenant, credential, profile, scope)` dials discovery first; it re-hashes and decodes the carried canonical document, proves the expected pin, exact-joins the sorted live availability and estimate rows to that document, and enrolls only the joined native view. Any divergence closes the transport.
- Entry: `run(descriptor_id, request, into)` admits the id against the generated catalog and sends the typed message over `/{WireService.CAPABILITY}/{descriptor_id}`.
- Growth: a new capability is one C# descriptor row; a new static descriptor column extends the canonical pin document once and its typed decoder here; a new dynamic discovery fact is one proto change and regeneration.
- Boundary: schema is absent from live discovery; generated SDK types own request and reply shape. `DescriptorPinWire.document` alone owns static surface, effect, repeat posture, scope, and cost-unit semantics; each generated `AvailableCapability` states only live presence and estimates, which this seam joins to the decoded document by descriptor and unit.
- Boundary: `connect` owns and closes the `pyqwest` transport; discovery refusal or pin divergence closes before enrollment, and normal drain closes after enrollment.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Awaitable, Callable
from secrets import compare_digest
from typing import Final, Literal, Protocol, Self, assert_never

import anyio
import msgspec
import stamina
from connectrpc.client import ConnectClient
from connectrpc.code import Code
from connectrpc.errors import ConnectError
from connectrpc.method import IdempotencyLevel, MethodInfo
from connectrpc.request import RequestContext
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import propagate, trace
from opentelemetry.trace import Span, SpanKind, Status, StatusCode
from protobuf import Message
from pyqwest import Client
# Contracts are retired from this logic.

from rasm.runtime.admission import RuntimeProfile
from rasm.runtime.clock import Tenant
from rasm.runtime.faults import (
    FAULT_TAG, SCOPES, SERVE_CATALOG, SERVE_DIAL, SERVE_DISCOVERY, SERVE_ENCODE,
    BoundaryFault, Recovery, RuntimeRail, Scope, async_boundary, boundary, scoped,
)
from rasm.runtime.identity import ContentIdentity
from rasm.runtime.metrics import TENANT_BAGGAGE
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.resilience import RetryClass
from rasm.runtime.shapes import RecoveryCell, WireService, remote_fault, wire_detail
from rasm.runtime.transport.body import AdmissionSide, BodyAdmission


# --- [TYPES] ----------------------------------------------------------------------------

type CostUnitKey = Literal["cpu-millis", "wall-millis", "bytes-egress", "model-tokens", "calls"]
type EffectKey = Literal["pure", "read", "write", "external", "irreversible"]
type IdempotencyKey = Literal["idempotent", "keyed", "single-shot", "non-idempotent"]


class WireDispatch(Protocol):
    async def __call__[R: Message](self, descriptor: str, request: Message, into: type[R], /) -> RuntimeRail[R]: ...

# --- [MODELS] ---------------------------------------------------------------------------


class _Descriptor(Struct, frozen=True):
    descriptor: str
    surface: str
    effect: EffectKey
    idempotency: IdempotencyKey
    scope: str
    units: tuple[CostUnitKey, ...]


class CapabilityAdmission(Struct, frozen=True):
    descriptor: _Descriptor
    estimates: tuple[tuple[CostUnitKey, int], ...]


# --- [TABLES] ---------------------------------------------------------------------------

_WIRE_ROW: Final = RetryClass.WIRE.policy

_DESCRIPTOR_DOCUMENT: Final = msgspec.json.Decoder(tuple[_Descriptor, ...])
_COST_UNIT: Final[dict[CostUnit, CostUnitKey]] = {
    CostUnit.CPU_MILLIS: "cpu-millis",
    CostUnit.WALL_MILLIS: "wall-millis",
    CostUnit.BYTES_EGRESS: "bytes-egress",
    CostUnit.MODEL_TOKENS: "model-tokens",
    CostUnit.CALLS: "calls",
}

# --- [SERVICES] -------------------------------------------------------------------------

_TRACER: Final = scoped(trace.get_tracer, SCOPES[Scope.WIRE])


class _Telemetry(msgspec.Struct, frozen=True, gc=False):
    tenant: Tenant

    async def on_start(self, ctx: RequestContext[Message, Message]) -> Span:
        span = _TRACER.start_span(
            f"{ctx.method.service_name}/{ctx.method.name}",
            kind=SpanKind.CLIENT,
            attributes={"rpc.system": "connect_rpc", "rpc.service": ctx.method.service_name, "rpc.method": ctx.method.name, TENANT_BAGGAGE: str(self.tenant)},
        )
        with trace.use_span(span):
            propagate.inject(ctx.request_headers)
        return span

    async def on_end(self, token: Span, ctx: RequestContext[Message, Message], error: Exception | None) -> None:
        match error:
            case ConnectError() as refused:
                token.set_attribute(FAULT_TAG, refused.code.value)
                token.set_status(Status(StatusCode.ERROR, refused.message))
            case _:
                pass
        token.end()


def _canonical_scope(value: str, /) -> bool:
    return len(value) == 32 and value == value.lower() and all(unit in "0123456789abcdef" for unit in value)


def _admitted_catalog(catalog: DiscoverResponse, expected_pin: bytes, /) -> Option[Map[str, CapabilityAdmission]]:
    document = catalog.pin.document.encode()
    derived = ContentIdentity.key("capability-descriptor", document, seed=Some(0)).value.to_bytes(16, "big")
    try:
        declared = _DESCRIPTOR_DOCUMENT.decode(document)
    except (msgspec.DecodeError, msgspec.ValidationError):
        return Nothing

    descriptor_ids = tuple(row.descriptor for row in declared)
    if not (
        compare_digest(catalog.pin.digest, expected_pin)
        and compare_digest(catalog.pin.digest, derived)
        and len(declared) == catalog.pin.descriptors
        and all(row.descriptor and row.surface and _canonical_scope(row.scope) for row in declared)
        and all(left < right for left, right in zip(descriptor_ids, descriptor_ids[1:], strict=False))
        and all(
            all(left < right for left, right in zip(row.units, row.units[1:], strict=False)) for row in declared
        )
    ):
        return Nothing

    by_id = {row.descriptor: row for row in declared}
    available = tuple(row.descriptor for row in catalog.capabilities)
    if not all(left < right for left, right in zip(available, available[1:], strict=False)):
        return Nothing

    admitted: list[tuple[str, CapabilityAdmission]] = []
    for row in catalog.capabilities:
        descriptor = by_id.get(row.descriptor)
        estimate_units: list[CostUnitKey] = []
        for estimate in row.estimates:
            unit = _COST_UNIT.get(estimate.unit)
            if unit is None:
                return Nothing
            estimate_units.append(unit)
        if descriptor is None or not all(
            left < right for left, right in zip(estimate_units, estimate_units[1:], strict=False)
        ) or not all(unit in descriptor.units for unit in estimate_units):
            return Nothing
        estimates = tuple((unit, estimate.amount) for unit, estimate in zip(estimate_units, row.estimates, strict=True))
        admitted.append((row.descriptor, CapabilityAdmission(descriptor, estimates)))
    return Some(Map.of_seq(admitted))


class CapabilityInvoke:
    def __init__(
        self, catalog: Map[str, CapabilityAdmission], dispatch: WireDispatch, transport: HTTPTransport | None = None, scope: ScopeKey = DEFAULT_SCOPE
    ) -> None:
        self._catalog, self._dispatch, self._transport, self.scope = catalog, dispatch, transport, scope

    @classmethod
    async def connect(
        cls,
        target: str,
        expected_pin: bytes,
        tenant: Tenant,
        credential: CredentialPolicy,
        *,
        profile: RuntimeProfile,
        scope: ScopeKey = DEFAULT_SCOPE,
    ) -> RuntimeRail[Self]:
        match credential.client_transport():
            case Result(tag="error", error=refused):
                return Error(refused)
            case Result(tag="ok", ok=(transport, credentials)):
                pass
            case _ as unreachable:
                assert_never(unreachable)

        policy = _WIRE_POLICY[profile]
        http = Client(transport=transport)
        interceptors = (BodyAdmission(AdmissionSide.CLIENT), _Telemetry(tenant), *credentials)
        client = ConnectClient(
            target,
            http_client=http,
            interceptors=interceptors,
            accept_compression=policy.compressions,
            send_compression=_ZSTD,
            read_max_bytes=policy.read_max_bytes,
        )
        discovery = CapabilityDiscoveryServiceClient(
            target,
            http_client=http,
            interceptors=interceptors,
            accept_compression=policy.compressions,
            send_compression=_ZSTD,
            read_max_bytes=policy.read_max_bytes,
        )

        async def discovered() -> RuntimeRail[DiscoverResponse]:
            try:
                catalog = await _WIRE_CALLER(discovery.discover, DiscoverRequest(), use_get=True)
                return Ok(catalog)
            except ConnectError as terminal:
                return Error(remote_fault(terminal))

        catalog_rail = (await async_boundary(SERVE_DIAL, discovered, catch=_WIRE_RAISES)).bind(lambda rail: rail)
        match catalog_rail:
            case Result(tag="error", error=refused):
                await transport.aclose()
                return Error(refused)
            case Result(tag="ok", ok=catalog):
                match _admitted_catalog(catalog, expected_pin):
                    case Option(tag="none"):
                        await transport.aclose()
                        return Error(SERVE_DISCOVERY.raised(expected_pin.hex(), catalog.pin.digest.hex()))
                    case Option(tag="some", some=rows):
                        pass
                    case _ as unreachable:
                        assert_never(unreachable)
            case _ as unreachable:
                assert_never(unreachable)

        async def dispatch[R: Message](descriptor_id: str, request: Message, into: type[R], /) -> RuntimeRail[R]:
            method = MethodInfo(name=descriptor_id, service_name=WireService.CAPABILITY, input=type(request), output=into, idempotency_level=IdempotencyLevel.UNKNOWN)

            async def called() -> RuntimeRail[R]:
                try:
                    return Ok(await _WIRE_CALLER(client.execute_unary, request=request, method=method))
                except ConnectError as terminal:
                    return Error(remote_fault(terminal))

            match boundary(SERVE_ENCODE, request.to_binary, catch=_ENCODE_RAISES):
                case Result(tag="error", error=refused):
                    return Error(refused)
                case _:
                    return (await async_boundary(SERVE_DIAL, called, catch=_WIRE_RAISES)).bind(lambda rail: rail)

        return Ok(_enrolled(cls(rows, dispatch, transport, scope)))

    async def run[R: Message](self, descriptor_id: str, request: Message, into: type[R]) -> RuntimeRail[R]:
        match self._catalog.try_find(descriptor_id):
            case Option(tag="none"):
                return Error(SERVE_CATALOG.raised(descriptor_id))
            case Option(tag="some"):
                return await self._dispatch(descriptor_id, request, into)
            case _ as unreachable:
                assert_never(unreachable)

    async def aclose(self) -> None:
        _retired(self)
        if self._transport is not None:
            await self._transport.aclose()


# --- [OPERATIONS] -----------------------------------------------------------------------

_LIVE_INVOKES: Final[dict[ScopeKey, set[CapabilityInvoke]]] = {}


def _enrolled(invoke: CapabilityInvoke) -> CapabilityInvoke:
    _LIVE_INVOKES[invoke.scope] = _LIVE_INVOKES.get(invoke.scope, set()) | {invoke}
    return invoke


def _retired(invoke: CapabilityInvoke) -> None:
    held = _LIVE_INVOKES.pop(invoke.scope, set()) - {invoke}
    if held:
        _LIVE_INVOKES[invoke.scope] = held


async def drained(*, scope: ScopeKey = DEFAULT_SCOPE) -> None:
    async with anyio.create_task_group() as tg:
        for invoke in tuple(_LIVE_INVOKES.get(scope, set())):
            tg.start_soon(invoke.aclose)


def _stated(raised: Exception) -> Option[Recovery]:
    return wire_detail(raised).bind(lambda sealed: RecoveryCell.stated(sealed.recovery).default_value(Nothing))


def _redrive(raised: Exception) -> bool | float:
    match _stated(raised):
        case Option(tag="some", some=Recovery(tag="terminal")):
            return False
        case Option(tag="some", some=Recovery(tag="throttled", throttled=seconds)):
            return seconds
        case _:
            return _WIRE_ROW.target(raised)


# --- [COMPOSITION] ----------------------------------------------------------------------

_WIRE_CALLER: Final[stamina.BoundAsyncRetryingCaller] = stamina.AsyncRetryingCaller(**_WIRE_ROW.schedule).on(_redrive)
```

## [04]-[ENTRY]

- Owner: `companion_app` is the `cyclopts` command axis AND the daemon composition root, co-located with `ServerHost` because the serve command composes the host it launches. `companion_app(served, drains, charges, ledger, composition)` is parameterized over the `Served` rows, the drainable owners, the supervised worker charges, the durable-evidence binding, and the custody scope, so a downstream folder's composition root — geometry `mesh/serve` the named consumer — supplies its served rows, drain stages, pool charges, `(Ledger, Custody)` pair, and `ScopeKey` by data; runtime never imports a downstream sibling package, and every install owner it composes is a runtime-interior module.
- Law: STRUCTURE PROVES BEFORE CUSTODY IS CLAIMED — `aligned` and `sealed` both seat immediately after the admitted context and ahead of every install, because each install takes process ownership no refusal hands back: a set-once OTel global, a patched contrib train, a registered profiler, a claimed hook-point table. A census seated after them reports a drifted mirror roster or a broken packed layout onto a process that has already mounted the surfaces its own refusal cannot unmount, and neither gate reads installed state, so the earlier seat costs nothing.
- Entry: the boot fold installs the durable evidence plane LAST among the observability owners and only where the caller bound one — `Journal.install` binds onto the same railed chain, so a refused census, an unmet port, or a colliding point roster stops the boot rather than leaving every producer's `record` railing for the process's life; an unbound composition installs none and runs unjournalled. `_supervised` then starts `Journal.drained` FIRST inside the supervision group through `tg.start`, whose readiness signal blocks until the consumer holds the receive end, so no later leg can suspend into an intake nothing reads.
- Entry: this drain fold owns ORDER — the journal drain first so facts stop before the pools that produce them die and the buffered window flushes losslessly through the drain still running behind it, then the caller's `drains` rows, then one pool-drain row per charge, then the supervisor's daemon-stop escalation so no spawned child outlives the daemon, then the two outbound client closes — the transport clients and every dialed capability invoke — and the profiles push stop. The `drained` line writes off that accumulated rail after those stages; `Telemetry.shutdown` settles and stops LAST. Every stage runs after an earlier fault, and the faults accumulate into one aggregate; a first-fault abort leaving later stages undrained never lands. Boot chains ride the faults `railed` builder over heterogeneous binds a `traversed` fold cannot express. Each stage is a `(FaultRow, DrainStage)` pair rather than a labelled thunk, so a stage's fault subject derives from the owning module's own `Leg`.
- Auto: readiness is sd-notify-shaped data — `NotifyState` closes the handshake vocabulary, `_notify` writes the service manager's `NOTIFY_SOCKET` datagram through the anyio UNIX-datagram factory, and an absent socket folds to a no-op so the same daemon runs bare or managed. `READY` fires through the serve `ready` hook after the health flips, `STOPPING` fires at the signal seam before the drain, and the `beating` leg halves `WATCHDOG_USEC` into its ping interval only when the manager arms it. Workers' actuator joins the one supervision group with the awaited `ServerHost.status` coroutine as its flip, so pool death advertises on the served health state without a second loop, and the serve leg's terminal send cancels the whole group. Lifecycle facts fire on the registered `LIFECYCLE_POINTS` rows and `_booted` subscribes the log tap per point, so daemon lifecycle telemetry is a hook projection, never a second emit path.
- Growth: a new private command is one `@app.command` method folding through the shared `_exit`; a new drainable owner is one `(FaultRow, stage)` row the ordered fold, the accumulate, and the line absorb, its anchor seated at the faults roster under the draining module's own `Leg`; a new lifecycle point is one `LIFECYCLE_POINTS` row; a new supervised pool is one `Charge` row; a new manager handshake is one `NotifyState` member; a new boot gate is one `yield from` beside the two structural ones, above the installs; a new custody posture behind the bound `ledger` pair is one `Custody` instance the caller constructs, zero serve edits; a sibling daemon is one `companion_app(served, drains, charges, ledger, composition)` call with its own served rows.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
import os
import signal
from collections.abc import Awaitable, Callable, Generator
from enum import StrEnum
from functools import partial
from pathlib import Path
from typing import Annotated, Any, Final, assert_never, override

import anyio
import msgspec
from anyio.streams.memory import MemoryObjectSendStream
from connectrpc.request import RequestContext
from cyclopts import App, Parameter
from cyclopts.types import NonNegativeFloat
from expression import Error, Nothing, Ok, Option, Result
from expression.collections import Block, Map
from pydantic import ValidationError
from pydantic_settings import SettingsError
from rasm.runtime import roots
from rasm.runtime.admission import RuntimeContext, RuntimeProfile, SettingsAdmission
from rasm.runtime.clock import sealed
from rasm.runtime.faults import (
    HOOKS_RELEASE, JOURNAL_DRAIN, PROFILES_DRAIN, ROOTS_DRAIN, SCOPES, SERVE_DIALS, SERVE_DRAIN, SERVE_HOST,
    SERVE_INPUTS, SERVE_SELECTOR, SERVE_SETTINGS, TELEMETRY_STOP, WORKERS_DAEMONS, WORKERS_POOL,
    Disposition, FaultRow, RuntimeRail, Scope, async_boundary, boundary, railed, traversed,
)
from rasm.runtime.hooks import HookPoint, Hooks, Modality, TapRow
from rasm.runtime.journal import Custody, Journal, Ledger
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.logging import LogPipeline, LogShip
from rasm.runtime.metrics import TENANT_BUDGET, Instrumentation, Metrics
from rasm.runtime.profiles import Profiles
from rasm.runtime.observe import DEFAULT_SCOPE, Drained, ScopeKey, logger
from rasm.runtime.recipe import RecipeExecution, RecipeName, RecipeSpec
from rasm.runtime.resilience import RetryMode, install
from rasm.runtime.shapes import aligned
from rasm.runtime.telemetry import Telemetry
from rasm.runtime.workers import Charge, Supervisor, WorkerKind, WorkerPool


# --- [TYPES] ----------------------------------------------------------------------------

type DrainStage = Callable[[], Awaitable[object] | RuntimeRail[object]]
class NotifyState(StrEnum):
    READY = "READY=1"
    STOPPING = "STOPPING=1"
    WATCHDOG = "WATCHDOG=1"


class LifecycleFact(msgspec.Struct, frozen=True, gc=False):
    subject: str
    clean: bool = True


# --- [MODELS] ---------------------------------------------------------------------------


# --- [TABLES] ---------------------------------------------------------------------------

_READY: Final[str] = "rasm.runtime.serve.ready"
_STOPPING: Final[str] = "rasm.runtime.serve.stopping"
_DRAINED: Final[str] = "rasm.runtime.serve.drained"
LIFECYCLE_POINTS: Final[Block[HookPoint[LifecycleFact]]] = Block.of_seq([
    HookPoint(_READY, LifecycleFact, Modality(observe=None)),
    HookPoint(_STOPPING, LifecycleFact, Modality(observe=None)),
    HookPoint(_DRAINED, LifecycleFact, Modality(replay=1)),
])

# --- [OPERATIONS] -----------------------------------------------------------------------


def _exit(outcome: RuntimeRail[object] | Drained[object]) -> int:
    match outcome:
        case Result() as rail:
            return rail.map(lambda _value: 0).default_value(1)
        case drained:
            return 0 if drained.faults.is_empty() else 1


@railed
def _booted(bind: str, grace: float, served: Block[Served[Any]], ledger: Option[tuple[Ledger, Custody]]) -> Generator[Any, Any, ServerHost]:
    settings = yield from boundary(SERVE_SETTINGS, SettingsAdmission.mounted, catch=(SettingsError, ValidationError, OSError))
    ship = LogShip.OTLP_CONSOLE
    LogPipeline.configure(ship=ship)
    ctx = RuntimeContext.admit(RuntimeProfile.SIDECAR)
    yield from aligned(Block.empty())
    yield from sealed()
    installed = Option.of_optional(settings.otel_endpoint).map(lambda endpoint: Telemetry.install(ctx, str(endpoint), ship=ship))
    Metrics.install(budget=installed.map(lambda held: held.signal_profile.cardinality_budget).default_value(TENANT_BUDGET))
    Instrumentation.install()
    Option.of_optional(settings.pyroscope_endpoint).map(lambda endpoint: Profiles.install(ctx, str(endpoint)))
    install(RetryMode.EMIT)
    yield from Hooks.register(LIFECYCLE_POINTS)
    yield from Hooks.subscribe(LIFECYCLE_POINTS, TapRow(log=SCOPES[Scope.SERVICE]))
    yield from ledger.map(lambda bound: Journal.install(*bound)).default_value(Ok(None))
    host = ServerHost(bind, CredentialPolicy.loopback(), grace, profile=ctx.profile)
    yield from host.mount(served)
    return host


async def _settled(at: FaultRow, stage: DrainStage) -> RuntimeRail[object]:
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
    for at, stage in stages:
        settled = settled.append(Block.singleton(await _settled(at, stage)))
    return traversed(settled, by=Disposition.ACCUMULATE)


async def _notify(state: NotifyState) -> None:
    match os.environ.get("NOTIFY_SOCKET"):
        case None:
            return
        case path:
            try:
                async with await anyio.create_unix_datagram_socket() as sock:
                    await sock.sendto(state.value.encode(), path.replace("@", "\0", 1) if path.startswith("@") else path)
            except OSError:
                return


async def _launched() -> None:
    await _notify(NotifyState.READY)
    await Hooks.fire_async(_READY, LifecycleFact(subject=SCOPES[Scope.SERVICE]))


async def _beating() -> None:
    match os.environ.get("WATCHDOG_USEC", ""), os.environ.get("WATCHDOG_PID", str(os.getpid())):
        case (usec, owner) if not usec.isdigit() or int(usec) == 0 or owner != str(os.getpid()):
            return
        case (usec, _):
            while True:
                await _notify(NotifyState.WATCHDOG)
                await anyio.sleep(int(usec) / 2_000_000)


def _fleet(charges: Block[Charge]) -> Block[tuple[FaultRow, DrainStage]]:
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
        async with sink:
            await sink.send((await async_boundary(SERVE_HOST, lambda: host.serve(ready=_launched), catch=_WIRE_RAISES)).bind(lambda rail: rail))
        group.cancel_scope.cancel()

    async def tripped(sink: MemoryObjectSendStream[RuntimeRail[object]]) -> None:
        async with sink:
            with anyio.open_signal_receiver(signal.SIGTERM, signal.SIGINT) as trips:
                async for _ in trips:
                    break
            await _notify(NotifyState.STOPPING)
            await Hooks.fire_async(_STOPPING, LifecycleFact(subject=SCOPES[Scope.SERVICE]))
            try:
                with anyio.CancelScope(shield=True):
                    await sink.send(await async_boundary(SERVE_DRAIN, host.drain, catch=_WIRE_RAISES))
            finally:
                group.cancel_scope.cancel()

    supervisor = Supervisor(charges, host.status)
    async with anyio.create_task_group() as group, send:
        if journalled:
            await group.start(Journal.drained)
        group.start_soon(hosting, send.clone())
        group.start_soon(tripped, send.clone())
        group.start_soon(_beating)
        supervisor.watch(group)

    settled = Block.of_seq([outcome async for outcome in receive])
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
    logger(composition).info("drained", stages=len(settled) + len(ordered), clean=pre_shutdown.is_ok())
    lifecycle = await Hooks.fire_async(_DRAINED, LifecycleFact(subject="drained", clean=pre_shutdown.is_ok()))
    custody = await _settled(HOOKS_RELEASE, lambda: Hooks.release(scope=composition))
    telemetry = await _settled(TELEMETRY_STOP, Telemetry.shutdown)
    return traversed(
        Block.of_seq([
            pre_shutdown.map(lambda _: None),
            lifecycle.map(lambda _: None),
            custody.map(lambda _: None),
            telemetry.map(lambda _: None),
        ]),
        by=Disposition.ACCUMULATE,
    ).map(lambda _: None)


async def _daemon(
    bind: str,
    grace: float,
    served: Block[Served[Any]],
    drains: Block[tuple[FaultRow, DrainStage]],
    charges: Block[Charge],
    ledger: Option[tuple[Ledger, Custody]],
    composition: ScopeKey,
) -> RuntimeRail[None]:
    match _booted(bind, grace, served, ledger):
        case Result(tag="error") as refused:
            return refused
        case Result(tag="ok", ok=host):
            return await _supervised(host, drains, charges, ledger.is_some(), composition)
        case _ as unreachable:
            assert_never(unreachable)


# --- [ENTRY] ----------------------------------------------------------------------------


def companion_app(
    served: Block[Served[Any]],
    drains: Block[tuple[FaultRow, DrainStage]] = Block.empty(),
    charges: Block[Charge] = Block.empty(),
    ledger: Option[tuple[Ledger, Custody]] = Nothing,
    composition: ScopeKey = DEFAULT_SCOPE,
) -> App:
    app = App(name=SCOPES[Scope.SERVICE], help="private companion daemon entry", result_action="return_int_as_exit_code_else_zero")

    @app.command
    async def serve(bind: str, *, grace: Annotated[NonNegativeFloat, Parameter(env_var="RASM_COMPANION_GRACE")] = 5.0) -> int:
        return _exit(await _daemon(bind, grace, served, drains, charges, ledger, composition))

    @app.command
    async def recipe(selector: str, assignments: Path | None = None) -> int:
        LogPipeline.configure(ship=LogShip.CONSOLE)
        install(RetryMode.EMIT)
        loaded = (
            Ok(Map.empty())
            if assignments is None
            else boundary(
                SERVE_INPUTS,
                lambda: Map.of_seq(msgspec.json.decode(assignments.read_bytes(), type=dict[str, object]).items()),
                catch=(msgspec.ValidationError, msgspec.DecodeError, OSError),
            )
        )
        spec = loaded.map(
            lambda inputs: RecipeSpec(
                recipe=boundary(SERVE_SELECTOR, lambda: RecipeName(selector), catch=ValueError).default_value(selector), inputs=inputs
            )
        )
        match spec:
            case Result(tag="error") as refused:
                return _exit(refused)
            case Result(tag="ok", ok=one):
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
