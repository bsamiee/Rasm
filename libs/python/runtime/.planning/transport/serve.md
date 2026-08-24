# [PY_RUNTIME_SERVE]

Companion server-host and daemon composition root: `ServerHost` owns the inbound Connect lifecycle — every generated `<Svc>ASGIApplication` constructed here over its servicer, mounted under one dispatcher, and served by hypercorn — and the interceptor tuple every rpc crosses: metadata admission once per call and descriptor-driven constraint validation on every request and response body. `ServerHost` is itself the generated `grpc.health.v1` `Health` servicer, so the host's serving state publishes through the same generated seam every dialer reaches. `CapabilityInvoke` is the descriptor-driven outbound dial over the C#-generated capability SDK, and `Entrypoint` the daemon boot/serve/drain choreography. It hosts the geometry companion daemon's unary `ComputeService.Tessellate`, server-streaming `ArtifactService.Fetch`, and client-streaming `ArtifactService.Put` over the corpus Connect contract on the UDS leg and re-mints nothing it composes: hypercorn owns the plaintext UNIX-socket ASGI bind and answers gRPC by prior-knowledge h2c, Connect over h2 and HTTP/1.1, and gRPC-Web with no further config; the socket path stays under the `AF_UNIX` `sun_path` limit or the bind refuses with `AF_UNIX path too long`. Connect's server and client both run asyncio loop primitives, so this host serves under `anyio.run(..., backend="asyncio")` alone.

Wire vocabulary imports from the one `rasm.contracts` root: compute and control from `rasm.contracts.rasm.contracts.compute`, faults from `rasm.contracts.rasm.contracts.fault`, `google.rpc` details from `rasm.contracts.google.rpc`, and the vendored health service from `rasm.contracts.grpc.health.v1`; body admission is `transport/body`'s `BodyAdmission`. These are the only import paths for wire symbols on this page; causal time remains `evidence/clock#CLOCK`'s and admitted context `execution/admission#CONTEXT`'s. Seam ledgers file the `CredentialPolicy` axis decode and W3C inbound extraction here — the ASGI middleware is this ingress's trace-context authority.

## [01]-[INDEX]

- [02]-[SERVE]: inbound host lifecycle over the generated applications under the profile's `WirePolicy`, metadata admission, body constraint validation, the two-directional `CredentialPolicy`, and typed error-detail egress.
- [03]-[CAPABILITY_INVOKE]: descriptor-driven outbound dial, the `FaultDetail` detail ingress, and the two-axis re-drive gate over the `WIRE` retry row.
- [04]-[ENTRY]: daemon composition root — railed boot under the structure-first wire gates, supervised serve, the ordered receipted drain, and the one-shot recipe command.

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

- Auto: `OpenTelemetryMiddleware` is the ONE trace-context authority — it extracts the inbound W3C parent off `scope["headers"]` and opens the SERVER span natively with `exclude_spans=["receive", "send"]` so a streaming rpc multiplies no child spans, `Admission` re-extracts nothing, no handler body opens a second scope, and a `Signals.attach` around the handler body re-roots spans the middleware already parented. `server_request_hook` stamps `rpc.service`/`rpc.method` off the `/<package>.<Service>/<Method>` path, so the per-call dimension rides the middleware natively. `Admission.on_start` binds the admitted context onto structlog contextvars for the rpc window, so `merge_contextvars` stamps every handler log line with the same admitted identity whichever arity served it.
- Auto: the generated `<Svc>` protocol declares each rpc's shape (`async def` unary, `AsyncIterator` server stream, `AsyncIterator` in and out for bidi) and the generated application binds the matching `Endpoint` kind. `BodyAdmission` implements those four native interceptor protocols in `transport/body` and no servicer repeats constraint logic; `settle` remains the one rail fold a handler body composes.
- Auto: `ServerHost` implements the selected generated `Health.Check` rpc — `serve()` mounts `HealthASGIApplication(self, interceptors=_INTERCEPTORS)` beside every served application, and `check` projects the serving map per service (an empty name answers the host whole, an unknown name `SERVICE_UNKNOWN`) — so a supervisor publishes on the same generated seam every dialer polls, the health application crosses the same admission and validation boundaries as every served rpc, and no hand-written health route exists; `Watch` remains upstream support closure but has no selected actor or runtime implementation. `connectrpc` ships no health surface, which is why the service is the vendored `grpc/health/v1/health.proto` module generated for this branch.
- Packages: `hypercorn` (the ASGI host, `DispatcherMiddleware`, `worker_serve`, `create_sockets`), `connectrpc` (`ConnectError`, `Code`, `RequestContext`, the metadata and body interceptor protocols), `protobuf-py` (`Message`), `transport/body` (`BodyAdmission`, `AdmissionSide`), `rasm.contracts` — `fault_pb.FaultDetail`/`Hlc`, `error_details_pb.BadRequest`, and the vendored `rasm.contracts.grpc.health.v1` `Health`/`HealthASGIApplication`/`HealthCheckRequest`/`HealthCheckResponse` — `connectrpc.compression` (`GzipCompression`, `ZstdCompression`), `opentelemetry-instrumentation-asgi`, `pyqwest` (`HTTPTransport`, `Client`), `anyio`, `structlog`, `msgspec`, `expression`, and the faults/admission/clock/metrics/receipts/shapes owners per the fence imports.
- Growth: a new served service is one `Served` row — the generated `<Svc>ASGIApplication` class beside its servicer — the composition root hands `mount`, with its `SERVICE_VOCABULARY` row; a new rpc or constraint is one proto edit and regeneration, with validation inherited at the body boundary; a new `FaultTag` member is one `_FAULT_STATUS` row and nothing else, the table being total by construction and read as a raw index, and that row carries its own re-drive membership with it because `_REDRIVEN` derives from it; a new refusal on this page is one `FaultRow` anchor at the `reliability/faults#FAULT` roster called through `raised`, never a literal construction; a new outbound credential posture is one `CredentialPolicy` case with one `client_transport` arm; a new span dimension is one `enrich` key; a new compression algorithm is one `Compression` value on the `WirePolicy` row both `mount` and `CapabilityInvoke.connect` read; a new health rpc is one upstream proto change the regenerated `health_connect` protocol carries and one override on `ServerHost`.
- Boundary: the wire contract is the corpus-homed `.proto` and its generated binding — the runtime mints no transport, no channel, and no second wire vocabulary; host lifecycle and product telemetry export stay with the composing application; the health service's proto is the frozen upstream publisher source under `libs/contracts/vendor/grpc-health`, regenerated and never edited here.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
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
from rasm.contracts.google.rpc.error_details_pb import BadRequest
from rasm.contracts.grpc.health.v1.health_connect import Health, HealthASGIApplication
from rasm.contracts.grpc.health.v1.health_pb import HealthCheckRequest, HealthCheckResponse
from rasm.contracts.rasm.contracts.clock.hlc_pb import Hlc
from rasm.contracts.rasm.contracts.fault.fault_pb import FaultDetail
import structlog

from rasm.runtime.admission import Deadline, RuntimeContext, RuntimeProfile
from rasm.runtime.clock import CausalFrame
from rasm.runtime.faults import (
    FAULT_OWNER, FAULT_TAG, SERVE_ANCHOR, SERVE_BUNDLE, SERVE_DIRECTION, SERVE_ROSTER,
    BoundaryFault, Catch, FaultTag, Recovery, RuntimeLeg, RuntimeRail,
)
from rasm.runtime.journal import NANOS_PER_TICK
from rasm.runtime.metrics import FAULT_OUTCOME, Metrics
from rasm.runtime.receipts import DrainOutcome
from rasm.runtime.shapes import RecoveryCell
from rasm.runtime.transport.body import AdmissionSide, BodyAdmission

# --- [TYPES] ----------------------------------------------------------------------------

# the outbound dial a client credential row projects to: the `pyqwest` transport carrying TLS material, beside the
# client interceptors a call-credential row contributes — `bearer` stamps its token per call on `ctx.request_headers`.
type Dial = tuple[HTTPTransport, tuple[MetadataInterceptor, ...]]


class Mounted(Protocol):
    # what a generated `<Svc>ASGIApplication` IS to this host: an ASGI callable carrying its own mount `path`. The
    # structural form admits every generated application whatever service it is generic over — `ConnectASGIApplication`
    # is invariant in its service parameter, so a nominal `ConnectASGIApplication[object]` admits none of them.
    @property
    def path(self) -> str: ...
    async def __call__(self, scope: AsgiScope, receive: ASGIReceiveCallable, send: ASGISendCallable) -> None: ...


class Application[S](Protocol):
    # the generated `<Svc>ASGIApplication` CLASS as this host constructs it: one servicer beside the FULL keyword set the
    # generated constructor declares — the interceptor tuple, the decompressed-body ceiling, the compression roster, and
    # the codec roster — so every generated class satisfies this shape, the host can seat the whole `WirePolicy`, and no
    # application reaches the dispatcher without the shared interceptor tuple. A narrower protocol is what left the
    # body bound and the compression policy unspellable at the one site that mounts them.
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


# one served service as DATA — the generated application class beside the servicer implementing its `<Svc>` protocol.
# `mount` constructs the application, so the interceptor seat and the wire policy are the host's, never a caller's to forget.
type Served[S] = tuple[Application[S], S]

# --- [CONSTANTS] ------------------------------------------------------------------------

# the ONE span-attribute key this module owns; every fault key it stamps arrives from the fault owner's rostered
# `FAULT_*` set, the same reason tenancy rides the metric owner's `TENANT_BAGGAGE` — one canonical spelling every
# plane reads, where a per-site literal drifts one end of a join a backend can no longer make. The fault TAG stays
# LOCAL and never a wire column — the compact `FaultDetail` transports the leg and its row ordinal as identity, so the
# tag survives on this host's own span where a backend joins it, exactly where the contract wants it kept.
_DESCRIPTOR_ATTR: Final[str] = "rasm.descriptor"
# the encode-side raise surface of a generated class: protobuf-py validates at `to_binary`/`to_json` ALONE — construction
# and assignment check nothing — raising `TypeError` on a wrong-typed slot, `OverflowError` on an out-of-range scalar,
# and `ValueError` on a malformed oneof or value. Named once, because the client's terminal fence maps every
# unclassified raise to UNAVAILABLE and a caller-repairable encode refusal must never read as a transient the WIRE row re-drives.
_ENCODE_RAISES: Final[Catch] = (TypeError, ValueError, OverflowError)
# the two compression rows both ends negotiate, ordered by preference: zstd first for the GLB, tensor, and splat bodies
# this host serves, gzip for every peer that lacks it; identity always survives resolution.
_ZSTD: Final[Compression] = ZstdCompression()
_GZIP: Final[Compression] = GzipCompression()
# the two provider planes every fence on this page names, spelled ONCE each because `catch` is REQUIRED on all three
# lift shapes and a per-site tuple drifts one seam's raise surface from its sibling's. `ConnectError` is the one class
# every Connect client raise and every handler refusal crosses as, and `OSError` is the socket half a dial or a bind
# reaches; hypercorn's lifespan refusals ride `_HOST_RAISES`. Neither set widens past `Exception`: cancellation is
# scope-owned flow control the faults owner forbids converting.
_WIRE_RAISES: Final[Catch] = (ConnectError, OSError)
_HOST_RAISES: Final[Catch] = (LifespanTimeoutError, LifespanFailureError, OSError)  # hypercorn's two lifespan refusals root at `Exception` directly

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class CredentialPolicy:
    # one axis, two directions, each case constructible at exactly the end it serves: `insecure_loopback` carries the
    # UDS serve leg where peer identity is the kernel-reported `(pid, uid)`, and the four outbound rows carry the PEM
    # roots, the client cert/key pair, the call-credential token, and the fold that composes a transport credential with
    # any number of call credentials. Both projections refuse the opposite direction by name, so a policy value can
    # never mount at the end it was not minted for and the refusal names which end rejected it.
    tag: Literal["insecure_loopback", "tls", "mtls", "bearer", "composed"] = tag()
    insecure_loopback: bool = case()
    tls: str = case()  # PEM root-certificate bundle the client verifies the server against
    mtls: tuple[str, str, str] = case()  # (root bundle PEM, client certificate chain PEM, client private key PEM)
    bearer: str = case()  # per-call access token; a client `MetadataInterceptor` stamps `authorization: Bearer <token>` on every rpc
    composed: tuple["CredentialPolicy", ...] = case()  # one transport row folded with every call row beside it

    @classmethod
    def loopback(cls) -> Self:
        return cls(insecure_loopback=True)

    @classmethod
    def bundled(cls, *rows: "CredentialPolicy") -> Self:
        return cls(composed=rows)

    def server_config(self, config: Config, bind: str) -> RuntimeRail[Config]:
        # the inbound projection assigns onto the one hypercorn `Config`: the loopback row binds PLAINTEXT — h2c on a
        # UNIX socket, the kernel peer credential its identity — on `bind` itself, because a `Config` holding no
        # `certfile`/`keyfile` binds its `bind` roster plaintext and never opens `insecure_bind`; every outbound row
        # refuses this seat by name through the ONE `SERVE_DIRECTION` row both projections raise through, so the
        # mirrored refusals cannot drift into two spellings.
        match self:
            case CredentialPolicy(tag="insecure_loopback"):
                config.bind = [bind]
                return Ok(config)
            case CredentialPolicy(tag="tls" | "mtls" | "bearer" | "composed" as outbound):
                return Error(SERVE_DIRECTION.raised(outbound, "inbound"))
            case _ as unreachable:
                assert_never(unreachable)

    def client_transport(self) -> RuntimeRail[Dial]:
        # the outbound mirror of `server_config`, and what makes the four client rows reachable: `tls`/`mtls` build the
        # `HTTPTransport` carrying the roots and the client pair, `bearer` contributes the per-call interceptor over the
        # ambient roots, and `composed` folds its rows through `_bundled`, whose contract takes ONE transport row
        # followed by call rows — a bundle of pure call credentials with no transport row to anchor them refuses rather
        # than dialing under whatever ambient roots the process happens to hold.
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
    # the mount policy as ONE value per profile: the decompressed-body ceiling `read_max_bytes` enforces at the server and
    # the client alike — `RESOURCE_EXHAUSTED` past it, per envelope on a stream — and the compression roster both ends
    # negotiate in preference order. Unset on the generated constructor, the server bounds NOTHING and offers gzip alone,
    # which is the unbounded-body, gzip-only-over-GLB form this row exists to foreclose.
    read_max_bytes: int
    compressions: tuple[Compression, ...]


class _Bearer(msgspec.Struct, frozen=True, gc=False):
    # the call-credential half as a client `MetadataInterceptor`: `on_start` stamps the token on the outbound headers and
    # `on_end` holds nothing, so a bearer row composes beside a transport row without touching the socket.
    token: str

    async def on_start(self, ctx: RequestContext[Message, Message]) -> None:
        ctx.request_headers["authorization"] = f"Bearer {self.token}"

    async def on_end(self, token: None, ctx: RequestContext[Message, Message], error: Exception | None) -> None:
        return


# --- [TABLES] ---------------------------------------------------------------------------

# TOTAL over `RuntimeProfile` by construction, read as a raw index: the sidecar serves tessellation sources and streams
# GLB artifacts, so its ceiling admits a whole IFC model per request; the tool, package, and test profiles dial and
# serve small control bodies and keep the tighter bound. Both compressions ride every row — a profile differs in the
# ceiling alone, and a row offering gzip only would re-open the GLB-over-gzip cost on that profile.
_WIRE_POLICY: Final[Map[RuntimeProfile, WirePolicy]] = Map.of_seq([
    (RuntimeProfile.TOOL, WirePolicy(read_max_bytes=64 << 20, compressions=(_ZSTD, _GZIP))),
    (RuntimeProfile.SIDECAR, WirePolicy(read_max_bytes=1 << 30, compressions=(_ZSTD, _GZIP))),
    (RuntimeProfile.PACKAGE, WirePolicy(read_max_bytes=64 << 20, compressions=(_ZSTD, _GZIP))),
    (RuntimeProfile.TEST, WirePolicy(read_max_bytes=64 << 20, compressions=(_ZSTD, _GZIP))),
])

# TOTAL over `FaultTag` by construction — one row per member of the closed vocabulary — which is what lets `settle`
# read it as a raw index. A defaulted lookup here is a catch-all over an OWNED closure: a member carrying no row
# would answer INTERNAL on the wire for the process's whole life and nothing anywhere would report the gap.
# `domain` carries a SIBLING's own refusal token, which refuses the request's STATE rather than its shape, so it maps
# FAILED_PRECONDITION beside `config` instead of joining `wire`/`boundary` on INVALID_ARGUMENT.
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

# Fault classes THIS server's own refusal grades re-drivable, DERIVED from the status table above rather than
# spelled beside it: a status a later attempt can clear names a class a later attempt can clear, so one authority
# answers both columns and a new `_FAULT_STATUS` row joins or stays out by its own status. RESOURCE_EXHAUSTED
# completes the peer client's transient trio and no row sends it — this host sheds through the breaker rather than
# answering exhausted — so naming it here would seat a membership no producer reaches. This set is the FLOOR and
# never the last word: `BoundaryFault.retriability` reads a rostered raise's own declared posture ahead of it.
_REDRIVEN: Final[frozenset[FaultTag]] = frozenset(tag for tag, status in _FAULT_STATUS.items() if status in (Code.UNAVAILABLE, Code.DEADLINE_EXCEEDED))

# the request-histogram outcome per terminal status, DERIVED from the two tables above: the interceptor sees the
# `ConnectError` `settle` raised, never the fault, so the metric owner's `FaultTag` projection re-keys onto the status
# that fault mapped to; every tag sharing one status shares one outcome by construction, and a status no tag reaches
# — a handler defect crossing as UNKNOWN — folds `rejected` at the read.
_CODE_OUTCOME: Final[Map[Code, DrainOutcome]] = Map.of_seq(
    (status, FAULT_OUTCOME.try_find(tag).default_value("rejected")) for tag, status in _FAULT_STATUS.items()
)
# `_sealed` reads these fact keys for the wire `message`, most specific first: one policy row, so a new evidence key
# lands here and never as one more `or` arm at the fold.
_MESSAGE_KEYS: Final[tuple[str, ...]] = ("detail", "cause", "case", "members", "subject")

# serving-map bool onto the generated `ServingStatus` — ONE projection `check` and the index row share; the
# two remaining members (`UNKNOWN`, `SERVICE_UNKNOWN`) are answers `_status` mints for a name the map never seated.
_SERVING: Final[Map[bool, HealthCheckResponse.ServingStatus]] = Map.of_seq([
    (True, HealthCheckResponse.ServingStatus.SERVING),
    (False, HealthCheckResponse.ServingStatus.NOT_SERVING),
])

# `_ADMITTED` holds the admitted context of the LIVE rpc: the interceptor binds it for the handler window and `admitted`
# reads it, so a generated handler — whose signature Connect fixes as `(request, ctx)` — reaches the runtime context
# with no second parameter and no ambient re-admission.
_ADMITTED: Final[ContextVar[RuntimeContext]] = ContextVar("rasm.runtime.serve.admitted")

# --- [SERVICES] -------------------------------------------------------------------------


class Admission:
    # Metadata admission stays on the hook pair that spans every rpc shape: body access belongs to `BodyAdmission`, while
    # deadline lift, contextvars, timing, and span enrichment remain per-call here. A refused admission raises the same
    # `ConnectError` a mid-call fault raises, through the same `settle`, so no body reaches the handler before admission.
    async def on_start(self, ctx: RequestContext[Message, Message]) -> tuple[RuntimeContext, float, object]:
        # one inbound map, three disjoint reads: the causal frame off the `SLOTS` headers, the deadline off the call,
        # and the W3C context the admitted `Correlation` adopts — the middleware parents the ambient span off the same
        # headers, so withholding the carrier here would root a fresh trace beside a remote-parented span. Both the
        # W3C keys and the `SLOTS` slots are single-valued by their own contracts, so the last-wins collapse of
        # `Headers.items()` settles nothing either reader disputes; the causal read answers `Option` and admission takes
        # `Option`, so a call with no causal headers admits `Nothing` rather than a zero-stamp frame.
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
        # the histogram settles with the rpc's terminal — a clean call `completed`, a `ConnectError` the status it
        # mapped to through `_CODE_OUTCOME`, any other raise `rejected` — the span carries the admitted attributes
        # beside the fault coordinates `settle` stamped, and the contextvars unwind WHOLE: a leaked bind stamps the next
        # rpc with this one's identity.
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


# Connect applies the tuple as nested interceptors in declaration order. Admission wraps body validation, so its
# duration, terminal outcome, context, and span cover validation refusals as well as the handler.
_INTERCEPTORS: Final[tuple[Interceptor, ...]] = (Admission(), BodyAdmission(AdmissionSide.SERVER))


class ServerHost(Health):
    # the host implements the selected `grpc.health.v1.Health.Check` actor; upstream Watch remains generated support
    # closure but has no runtime actor, so this surface does not invent a stream no client or manifest direction owns.
    def __init__(self, bind: str, credential: CredentialPolicy | None = None, grace: float = 5.0, *, profile: RuntimeProfile) -> None:
        # `profile` selects the `WirePolicy` row every mounted application constructs under — the admitted context's own
        # profile, so the body ceiling a daemon serves is the one its admission declared and never a per-mount literal.
        self._bind, self._credential, self._grace, self._policy = bind, credential or CredentialPolicy.loopback(), grace, _WIRE_POLICY[profile]
        self._applications: Block[Mounted] = Block.empty()
        self._serving: Map[str, bool] = Map.empty()
        self._shutdown = anyio.Event()

    def mount(self, served: Block[Served[Any]]) -> RuntimeRail[int]:
        # the host CONSTRUCTS every application over the servicer a row carries, seating `_INTERCEPTORS` itself, so the
        # admission and body boundary hold by construction rather than by every composition root remembering them; each
        # application carries its own `path` and typed endpoints, so no codec pair, route row, or method name resolves here
        # — `transport/shapes#BOOT_CENSUS` already proved every dialed row against its generated class at boot.
        self._applications = self._applications.append(served.map(lambda row: self._constructed(*row)))
        return Ok(len(served))

    def _constructed(self, application: Application[Any], servicer: Any) -> Mounted:
        # ONE construction every served application and the host's own health application share: the interceptor tuple
        # and the profile's `WirePolicy` seat here, so no mount — and no second composition root — can forget the body
        # ceiling or offer gzip alone.
        return application(servicer, interceptors=_INTERCEPTORS, read_max_bytes=self._policy.read_max_bytes, compressions=self._policy.compressions)

    @staticmethod
    def admitted() -> RuntimeContext:
        # the live rpc's admitted context, bound by `Admission.on_start` for exactly the handler window.
        return _ADMITTED.get()

    @staticmethod
    def enrich(context: RuntimeContext, descriptor: str, refused: Option[BoundaryFault] = Nothing) -> None:
        # the server end spans what it HOLDS. Absence takes `Nothing` and stamps NO fault key: the retired `"ok"`
        # literal filled a fault dimension on every clean call, which is the empty-value series the branch's optional
        # -dimension law forbids. `owner` resolves the emitting leg off the census, so a fault whose subject no
        # module seated omits that key too rather than naming a leg nothing declared.
        seat = refused.map(lambda fault: {FAULT_TAG: fault.tag} | fault.owner.map(lambda leg: {FAULT_OWNER: leg}).default_value({}))
        trace.get_current_span().set_attributes(context.attribute() | {_DESCRIPTOR_ATTR: descriptor} | seat.default_value({}))

    @staticmethod
    def settle[T](wired: RuntimeRail[T]) -> T:
        # generic over the terminated payload, so one fold ends a unary reply, a streamed frame, and an admission
        # refusal alike — the Error arm raises and returns nothing, so the payload type only ever names what the Ok
        # arm hands back. The raise is the ONE `ConnectError` Connect carries intact with its `code`, `message`, and
        # `details`: the generated `FaultDetail` rides `details` as the typed conflict, so the trailer this fold once
        # packed by hand is the protocol's own error-detail channel, spelled identically on Connect, gRPC, and gRPC-Web.
        match wired:
            case Result(tag="ok", ok=payload):
                return payload
            case Result(tag="error", error=fault):
                context = _ADMITTED.get(RuntimeContext.admit(RuntimeProfile.SIDECAR))
                ServerHost.enrich(context, "settle", Some(fault))
                # raw index, never a defaulted lookup: the table declares one row per `FaultTag` member, so a member
                # that gains no row must break the read rather than answer INTERNAL for the process's whole life.
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
                # ONE dispatcher over every generated application keyed by its own `path`, the generated health application
                # over this host beside them, and ONE tracing wrap outside it; `exclude_spans` keeps a streaming rpc from
                # minting a span per frame.
                health = self._constructed(HealthASGIApplication, self)
                app = OpenTelemetryMiddleware(
                    DispatcherMiddleware({application.path: application for application in (*self._applications, health)}),
                    exclude_spans=["receive", "send"],
                    server_request_hook=_rpc_attributes,
                )
                # the listeners bind and LISTEN before readiness fires: `create_sockets` binds every `bind` row, and the
                # backlog queues a caller's connection until the loop accepts it, so an sd-notify READY names a socket that
                # answers rather than one the next statement would have opened; `worker_serve` adopts the bound set whole.
                sockets = config.create_sockets()
                for sock in (*sockets.insecure_sockets, *sockets.secure_sockets):  # Exemption: the platform listen seam ahead of READY
                    sock.listen(config.backlog)
                # the map seats every served application under its SERVICE NAME — the `WireService` text the supervisor
                # flips and a dialer's `check(service)` names — never the slashed mount path, which no probe spells.
                self._serving = Map.of_seq((_service(application), True) for application in self._applications)
                if ready is not None:  # the readiness hook fires after the health flips, so an sd-notify READY never precedes a serving probe
                    await ready()
                await worker_serve(wrap_app(app, config.wsgi_max_body_size, "asgi"), config, sockets=sockets, shutdown_trigger=self._shutdown.wait)
                return Ok(None)
            case _ as unreachable:
                assert_never(unreachable)

    async def status(self, service: str, serving: bool) -> None:
        # supervisor's one flip surface, keyed on the `WireService` text the `execution/workers#SUPERVISION` charge
        # subject spells; a drained host holds NOT_SERVING and this set is a no-op. The map is what `check` publishes
        # through the generated `HealthASGIApplication` mounted over this host.
        self._serving = self._serving.add(service, serving) if not self._shutdown.is_set() else self._serving

    async def drain(self) -> None:
        # NOT_SERVING races ahead of the stop: probes stop routing new work while hypercorn's `graceful_timeout`
        # drains in-flight calls; the flip is permanent, so a late success cannot re-advertise.
        self._serving = Map.of_seq((service, False) for service in self._serving.keys())
        self._shutdown.set()

    @override
    async def check(self, request: HealthCheckRequest, ctx: RequestContext[HealthCheckRequest, HealthCheckResponse]) -> HealthCheckResponse:
        # generated `Health.check`: one projection of the serving map, so a probe reads the same fact `drain` flips.
        _ = ctx
        return HealthCheckResponse(status=self._status(request.service))

    def _status(self, service: str) -> HealthCheckResponse.ServingStatus:
        # `grpc.health.v1` name contract: an empty name answers the host whole, a served service name answers its row,
        # and a name no application serves answers SERVICE_UNKNOWN rather than a forged NOT_SERVING.
        whole = not self._serving.is_empty() and all(self._serving.values())
        return (
            self._serving.try_find(service).map(_SERVING.__getitem__).default_value(HealthCheckResponse.ServingStatus.SERVICE_UNKNOWN)
            if service
            else _SERVING[whole]
        )

# --- [OPERATIONS] -----------------------------------------------------------------------


def _service(application: Mounted) -> str:
    # ONE derivation of the health key from the generated application: `path` is `/<package>.<Service>`, so the name
    # is the path less its slash — exactly the `WireService` member text, which is why a supervisor flip, a dialer's
    # probe, and this map can never hold two spellings of one service.
    return application.path.removeprefix("/")


def _rpc_attributes(span: trace.Span, scope: dict[str, object]) -> None:
    # the per-call rpc dimension off the ASGI path the generated application mounts at — `/<package>.<Service>/<Method>` —
    # stamped natively by the one tracing middleware, so no handler names its own service or method.
    service, _, method = str(scope.get("path", "")).strip("/").rpartition("/")
    span.set_attributes({"rpc.system": "connect_rpc", "rpc.service": service, "rpc.method": method})


def _bundled(rows: Block[CredentialPolicy]) -> RuntimeRail[Dial]:
    # a bundle PROVES its arity rather than trusting a caller's ordering: `tls`/`mtls` are the transport half and
    # `bearer` the call half. A bundle carrying no transport row would dial under whatever ambient roots the process
    # happens to hold, two would silently drop one, and a nested `composed` or an `insecure_loopback` row belongs to
    # neither half — each refuses by name here rather than at the socket, the roster row carrying the kinds actually
    # offered as its own coordinate, which is strictly more evidence than the fixed sentence it replaces.
    anchors, tokens = rows.filter(lambda row: row.tag in ("tls", "mtls")), rows.filter(lambda row: row.tag == "bearer")
    return (
        Error(SERVE_BUNDLE.raised(",".join(sorted({row.tag for row in rows}))))
        if len(anchors) + len(tokens) != len(rows)
        else Error(SERVE_ANCHOR.raised(str(len(anchors))))
        if len(anchors) != 1
        else anchors.head().client_transport().map(lambda dial: (dial[0], tuple(_Bearer(row.bearer) for row in tokens)))
    )


def _details(fault: BoundaryFault, context: RuntimeContext) -> tuple[Message, ...]:
    # Every refusal seats its compact estate cell; a measured throttle seats the standard advice detail generic
    # Connect peers honor. That detail is this cell's OWN throttled arm read back through the correspondence owner —
    # one `RetryInfo` in both places, never a second construction those two seats can drive apart — so this leg
    # names neither that standard message nor a duration unit.
    sealed = _sealed(fault, context, fault.retriability(_REDRIVEN))
    return RecoveryCell.advice(sealed.recovery).map(lambda advice: (sealed, advice)).default_value((sealed,))


def _sealed(fault: BoundaryFault, context: RuntimeContext, recovery: Recovery) -> FaultDetail:
    # one total egress fold onto the COMPACT generated message the contract froze: `domain` is the EMITTING leg off the
    # fault census and `case` that leg's closed row ordinal — the producing family's identity, never the Connect code the
    # same refusal carries on `ConnectError.code` — so a peer keeps the pair opaque and the producer's tag, its detail
    # string, and the whole `facts()` map stay LOCAL: the tag rides `FAULT_TAG` on this host's own span and the facts
    # ride the structured log line the same fold writes, both joinable on the `correlation` this detail does carry. An
    # unseated subject crosses under this host's own serving leg at the unspecified zero, since a foreign raise has no
    # family ordinal to claim. `stamp` is the admitted causal cell where the call carried one and this host's own sample
    # where it did not — the slot is required on the wire, and a zero stamp would sort before every real one.
    # `violations` carry the row's NAMED coordinates as `BadRequest.FieldViolation` rows — the one place a field path
    # crosses — so a peer repairs by coordinate under the row's defect token rather than parsing a message.
    #
    # `recovery` stamps THIS server's re-drive verdict rather than leaving a peer to infer one: a consumer reading
    # INVALID_ARGUMENT cannot tell a malformed payload from a codec that decodes on the next build, and one guessing
    # from the status substitutes its own band table for this classification. It grades through the faults owner's
    # own `retriability` read — that tier's ONE declared precedence, a rostered raise's own row posture over the
    # `FaultTag` derivation — so an `aggregate` folds its members through `Recovery.widest` and no second classifier
    # restates the fault family here. `_details` seats this cell's own throttled arm as its standard advice detail,
    # so compact cell and generic peer detail are ONE message and cannot disagree.
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
- Packages: `connectrpc`, `pyqwest`, `stamina`, `opentelemetry-api`, `rasm.contracts` capability/fault bindings, and the existing runtime rails.
- Growth: a new capability is one C# descriptor row; a new static descriptor column extends the canonical pin document once and its typed decoder here; a new dynamic discovery fact is one proto change and regeneration.
- Boundary: schema is absent from live discovery; generated SDK types own request and reply shape. `DescriptorPinWire.document` alone owns static surface, effect, repeat posture, scope, and cost-unit semantics; each generated `AvailableCapability` states only live presence and estimates, which this seam joins to the decoded document by descriptor and unit.
- Boundary: `connect` owns and closes the `pyqwest` transport; discovery refusal or pin divergence closes before enrollment, and normal drain closes after enrollment.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
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
from rasm.contracts.rasm.contracts.capability.discovery_connect import CapabilityDiscoveryServiceClient
from rasm.contracts.rasm.contracts.capability.discovery_pb import CostUnit, DiscoverRequest, DiscoverResponse
from rasm.contracts.rasm.contracts.fault.fault_pb import FaultDetail

from rasm.runtime.admission import RuntimeProfile
from rasm.runtime.clock import Tenant
from rasm.runtime.faults import (
    FAULT_TAG, SCOPES, SERVE_CATALOG, SERVE_DIAL, SERVE_DISCOVERY, SERVE_ENCODE,
    BoundaryFault, Recovery, RuntimeRail, Scope, async_boundary, boundary, scoped,
)
from rasm.runtime.identity import ContentIdentity
from rasm.runtime.metrics import TENANT_BAGGAGE
from rasm.runtime.receipts import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.resilience import RetryClass
from rasm.runtime.shapes import RecoveryCell, WireService, remote_fault, wire_detail
from rasm.runtime.transport.body import AdmissionSide, BodyAdmission

# `_WIRE_RAISES`, `_ENCODE_RAISES`, `_ZSTD`, and `_WIRE_POLICY` are this module's [02]-[SERVE] planes and `CredentialPolicy` its
# owner of this dial's two-directional credential axis. `Recovery` is the RUNTIME owner's re-offer family and `RecoveryCell` the correspondence carrying both
# directions over the generated `FaultRecovery`, so this seam decodes a verdict and never mints a second vocabulary for one.

# --- [TYPES] ----------------------------------------------------------------------------

type CostUnitKey = Literal["cpu-millis", "wall-millis", "bytes-egress", "model-tokens", "calls"]
type EffectKey = Literal["pure", "read", "write", "external", "irreversible"]
type IdempotencyKey = Literal["idempotent", "keyed", "single-shot", "non-idempotent"]
type MeterVector = dict[CostUnitKey, int]
type CommandTxnKind = Literal["committed", "rolled_back", "compensated", "refused"]


class WireDispatch(Protocol):
    # generic over the reply class the caller names: `MethodInfo.output=into` makes the client decode INTO that class, so
    # the rail carries `R` end to end and no coercion stands between the wire and the caller's value.
    async def __call__[R: Message](self, descriptor: str, request: Message, into: type[R], /) -> RuntimeRail[R]: ...

# --- [MODELS] ---------------------------------------------------------------------------


class CommandTxn(Struct, frozen=True, rename="camel"):
    kind: CommandTxnKind
    detail: str = ""


class CommandReceipt(Struct, frozen=True, rename="camel"):
    descriptor: str
    txn: CommandTxn
    charged: MeterVector
    elapsed: str
    correlation: str


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

# Outbound retry POLICY for this seam, read whole off `reliability/resilience#RESILIENCE`: attempts, timeout, backoff
# curve, and the transience read this seam falls back to all arrive as that page's `RetryClass.WIRE` row, so nothing
# here re-spells a schedule and a table edit lands on this dial with no consumer touch.
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
    # the client `MetadataInterceptor`: ONE span per dial opened in `on_start`, the W3C context injected onto the outbound
    # headers, the tenant and the rpc identity stamped off `ctx.method`, and the span closed in `on_end` off the terminal
    # `ConnectError.code` — the client end spans only what it TRULY has, so `FAULT_CODE`/`FAULT_POSTURE`, which need the
    # detail decode, stay with `remote_fault` where it happens, and a clean call OMITS the fault key.
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
        # the dial is CREDENTIALED by construction: the caller's policy projects to a transport or refuses on the rail
        # before any socket exists, so a plaintext TCP transport appears on no path here and an unauthenticated outbound
        # leg is unspellable rather than the silent default this seam used to carry. The refusal names which case
        # rejected it — `insecure_loopback` is the inbound seat, and a malformed bundle names its own arity. `scope` is the
        # composition this dial belongs to, and it is what the drain later reaches this client by. The `pyqwest`
        # `HTTPTransport` owns the sockets and `ConnectClient.close` only flips a flag, so the transport this seam
        # builds is the handle it holds and `aclose`s.
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
                # Exemption: the detail fence — the decoded `FaultDetail` lives only on the live `ConnectError.details`, so this one
                # platform-forced except reclassifies the terminal raise AFTER the WIRE-row retry exhausts. The retry itself already
                # read that same detail through `_redrive`, so a producer-terminal fault arrives here having spent one attempt.
                try:
                    return Ok(await _WIRE_CALLER(client.execute_unary, request=request, method=method))
                except ConnectError as terminal:
                    return Error(remote_fault(terminal))

            # PRE-ENCODE under the `config` fence, AHEAD of the retried call: `to_binary` is where protobuf-py validates, and
            # inside the client those raises fall through its terminal `except Exception` as UNAVAILABLE — the one status the
            # WIRE row grades transient — so a caller-repairable refusal would spend every attempt before surfacing wrong.
            match boundary(SERVE_ENCODE, request.to_binary, catch=_ENCODE_RAISES):
                case Result(tag="error", error=refused):
                    return Error(refused)
                case _:
                    return (await async_boundary(SERVE_DIAL, called, catch=_WIRE_RAISES)).bind(lambda rail: rail)

        return Ok(_enrolled(cls(rows, dispatch, transport, scope)))

    async def run[R: Message](self, descriptor_id: str, request: Message, into: type[R]) -> RuntimeRail[R]:
        # the generated class IS the codec: Connect's proto binary codec serializes `request` and decodes `into`, so the
        # catalog check is the only prologue and no registry row stands between the caller's value and the wire.
        match self._catalog.try_find(descriptor_id):
            case Option(tag="none"):
                return Error(SERVE_CATALOG.raised(descriptor_id))
            case Option(tag="some"):
                # the client already decoded INTO `into` off `MethodInfo.output`; a re-encode round trip here was a lossy twin.
                return await self._dispatch(descriptor_id, request, into)
            case _ as unreachable:
                assert_never(unreachable)

    async def aclose(self) -> None:
        # runtime-lived transport's deterministic drain; a directly-injected dispatch carries no transport, a typed no-op.
        _retired(self)
        if self._transport is not None:
            await self._transport.aclose()


# --- [OPERATIONS] -----------------------------------------------------------------------

# every dialed invoke, PARTITIONED by the composition that dialed it. `connect` builds the transport, so this owner owes
# its teardown exactly as the transport owner owes its pooled clients — but a process-wide set made one composition's
# drain close a transport an embedding host was still serving calls over, so the registry keys on the branch's one
# custody axis exactly as the hooks tables, the metrics state, and the install-receipt maps do. A directly-injected
# dispatch never enters — it holds no transport and there is nothing to close.
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
    # transport THIS composition dialed, concurrently, and `aclose` retires its own row so a caller closing early never
    # double-closes and a sibling composition's live transports are never reached.
    async with anyio.create_task_group() as tg:
        for invoke in tuple(_LIVE_INVOKES.get(scope, set())):
            tg.start_soon(invoke.aclose)


def _stated(raised: Exception) -> Option[Recovery]:
    # the PRODUCER's rung of the retriability precedence, read at the ONE seam that can read it: `reliability/faults`
    # declares the two rungs it can see — a rostered raise's own row posture over the `FaultTag` derivation — and rules
    # that a peer-stated posture outranks both and decodes HERE, where a live detail exists to read. `Nothing` is the
    # honest third answer rather than a fourth case: an absent detail, a frame minted before the slot existed, and a
    # window this producer spelled wrong all state NOTHING about re-driving, and each defers to the rung below instead
    # of masquerading as a verdict. Both readers on this page compose it, so the posture a retry acts on is the posture a
    # caller's fault carries.
    return wire_detail(raised).bind(lambda sealed: RecoveryCell.stated(sealed.recovery).default_value(Nothing))


def _redrive(raised: Exception) -> bool | float:
    # TWO AXES CROSS HERE: this producer's own verdict against the class's route, and no bool anywhere. Status is the
    # only axis the `RetryClass.WIRE` row can read, so it retries the transient trio and refuses the rest; its detail
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


# --- [COMPOSITION] ----------------------------------------------------------------------

# Bound caller for this seam: ONE construction off the `RetryClass.WIRE` row, so its schedule stays the resilience
# table's and only the RE-DRIVE VERDICT is this page's. `guard(cls)` stays the bare per-CLASS caller a consumer
# holding no verdict binds — its `@cache` keys on the member alone — and a verdict read from a detail is per-SEAM by
# construction: no policy tier can decode a `FaultDetail`, and this page owns that detail end to end.
_WIRE_CALLER: Final[stamina.BoundAsyncRetryingCaller] = stamina.AsyncRetryingCaller(**_WIRE_ROW.schedule).on(_redrive)
```

## [04]-[ENTRY]

- Owner: `companion_app` is the `cyclopts` command axis AND the daemon composition root, co-located with `ServerHost` because the serve command composes the host it launches. `companion_app(served, drains, charges, ledger, composition)` is parameterized over the `Served` rows, the drainable owners, the supervised worker charges, the durable-evidence binding, and the custody scope, so a downstream folder's composition root — geometry `mesh/serve` the named consumer — supplies its served rows, drain stages, pool charges, `(Ledger, Custody)` pair, and `ScopeKey` by data; runtime never imports a downstream sibling package, and every install owner it composes is a runtime-interior module.
- Law: STRUCTURE PROVES BEFORE CUSTODY IS CLAIMED — `aligned` and `sealed` both seat immediately after the admitted context and ahead of every install, because each install takes process ownership no refusal hands back: a set-once OTel global, a patched contrib train, a registered profiler, a claimed hook-point table. A census seated after them reports a drifted mirror roster or a broken packed layout onto a process that has already mounted the surfaces its own refusal cannot unmount, and neither gate reads installed state, so the earlier seat costs nothing.
- Entry: the boot fold installs the durable evidence plane LAST among the observability owners and only where the caller bound one — `Journal.install` binds onto the same railed chain, so a refused census, an unmet port, or a colliding point roster stops the boot rather than leaving every producer's `record` railing for the process's life; an unbound composition installs none and runs unjournalled. `_supervised` then starts `Journal.drained` FIRST inside the supervision group through `tg.start`, whose readiness signal blocks until the consumer holds the receive end, so no later leg can suspend into an intake nothing reads.
- Entry: this drain fold owns ORDER — the journal drain first so facts stop before the pools that produce them die and the buffered window flushes losslessly through the drain still running behind it, then the caller's `drains` rows, then one pool-drain row per charge, then the supervisor's daemon-stop escalation so no spawned child outlives the daemon, then the two outbound client closes — the transport clients and every dialed capability invoke — and the profiles push stop. Lifecycle receipt emission settles onto the same accumulated rail after those stages; `Telemetry.shutdown` settles and stops LAST. Every stage runs after an earlier fault, and the faults accumulate into one aggregate; a first-fault abort leaving later stages undrained never lands. Boot chains ride the faults `railed` builder over heterogeneous binds a `traversed` fold cannot express. Each stage is a `(FaultRow, DrainStage)` pair rather than a labelled thunk, so a stage's fault subject derives from the owning module's own `Leg`.
- Auto: readiness is sd-notify-shaped data — `NotifyState` closes the handshake vocabulary, `_notify` writes the service manager's `NOTIFY_SOCKET` datagram through the anyio UNIX-datagram factory, and an absent socket folds to a no-op so the same daemon runs bare or managed. `READY` fires through the serve `ready` hook after the health flips, `STOPPING` fires at the signal seam before the drain, and the `beating` leg halves `WATCHDOG_USEC` into its ping interval only when the manager arms it. Workers' actuator joins the one supervision group with the awaited `ServerHost.status` coroutine as its flip, so pool death advertises on the served health state without a second loop, and the serve leg's terminal send cancels the whole group. Lifecycle facts fire on the registered `LIFECYCLE_POINTS` rows and `_booted` subscribes the receipts tap per point, so daemon lifecycle telemetry is a hook projection, never a second emit path.
- Packages: `cyclopts`, `anyio`, `msgspec`, `pydantic`, `pydantic-settings`, `rasm.contracts`, and the faults/telemetry/logging/profiles/hooks/metrics/receipts/resilience/admission/clock/shapes/journal/lanes/workers/recipe/roots owners per the fence imports; the two settings classes enter as REFUSAL vocabulary alone, because the required `catch` names the provider that raises and the settings model is pydantic's — this leg mints no model of its own, and the Connect raise surface it fences rides `[02]-[SERVE]`'s `_WIRE_RAISES` rather than a second import.
- Growth: a new private command is one `@app.command` method folding through the shared `_exit`; a new drainable owner is one `(FaultRow, stage)` row the ordered fold, the accumulate, and the receipt absorb, its anchor seated at the faults roster under the draining module's own `Leg`; a new lifecycle point is one `LIFECYCLE_POINTS` row; a new supervised pool is one `Charge` row; a new manager handshake is one `NotifyState` member; a new boot gate is one `yield from` beside the two structural ones, above the installs; a new custody posture behind the bound `ledger` pair is one `Custody` instance the caller constructs, zero serve edits; a sibling daemon is one `companion_app(served, drains, charges, ledger, composition)` call with its own served rows.
- Boundary: never a new public command surface — public commands are reserved to the suite Assay command surface. `NOTIFY_SOCKET`/`WATCHDOG_USEC` are the service manager's own env contract read at this one entry seam, never a settings field and never a read past admission elsewhere.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
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
from rasm.runtime.shapes import aligned
from rasm.runtime.telemetry import Telemetry
from rasm.runtime.workers import Charge, Supervisor, WorkerKind, WorkerPool

# `ServerHost`/`Admission`/`CredentialPolicy`/`Served` are this module's [02]-[SERVE] owners — no cross-module import.

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
def _booted(bind: str, grace: float, served: Block[Served[Any]], ledger: Option[tuple[Ledger, Custody]]) -> Generator[Any, Any, ServerHost]:
    # admit -> gate -> install -> bind as one railed bind chain: the first Error short-circuits, the composed host rides the
    # Ok payload; an absent otel or pyroscope endpoint installs nothing — no literal. the settings model is pydantic's,
    # so its refusal classes are pydantic's — naming them is what the required `catch` means, and `OSError` is the
    # secrets-mount read the same construction performs.
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
    # boot that mounts them and then discovers a drifted mirror roster or a broken packed layout has already taken
    # ownership of surfaces its refusal cannot hand back. Neither gate reads installed state, so nothing here owes
    # them a provider.
    yield from aligned(Block.empty())
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
    host = ServerHost(bind, CredentialPolicy.loopback(), grace, profile=ctx.profile)
    yield from host.mount(served)
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
        # finally's own group cancel makes signal-path termination unconditional — a drain refusal whose shutdown
        # trigger never unblocks `serve` can no longer strand the daemon on the hosting leg's cancel alone.
        async with sink:
            with anyio.open_signal_receiver(signal.SIGTERM, signal.SIGINT) as trips:  # Exemption: the platform signal seam.
                async for _ in trips:
                    break
            await _notify(NotifyState.STOPPING)
            await Hooks.fire_async(_STOPPING, LifecycleFact(subject=SCOPES[Scope.SERVICE]))
            try:
                with anyio.CancelScope(shield=True):
                    # stage one of the drain order: health flip + shutdown trigger unblocks `serve` under hypercorn's graceful window.
                    await sink.send(await async_boundary(SERVE_DRAIN, host.drain, catch=_WIRE_RAISES))
            finally:
                group.cancel_scope.cancel()

    supervisor = Supervisor(charges, host.status)
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
    # and its buffered window flushes losslessly through the drain still running behind it. Daemon children stop AFTER the
    # pools drain — a child may still serve pooled work — the transport clients and the profiles push close next.
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
    # `ledger` is the durable-evidence binding this root supplies as a PAIR: the port implementation and the KEK custody
    # posture arrive together because a journal that lands rows it cannot shred is not a lawful plane, and `Nothing` is the
    # honest unjournalled composition rather than a default ledger this branch would have to invent. S0 declares the
    # port and imports no implementer, so a caller — `data`'s `FactJournal` the shipped one — hands the value in exactly
    # as it hands in served rows, drain stages, and charges. `composition` is the custody key every scope-partitioned
    # surface this root touches carries — the capsule's capture scope and the capability drain's partition — so an
    # embedded daemon's evidence and dials never merge with the process root's.
    app = App(name=SCOPES[Scope.SERVICE], help="private companion daemon entry", result_action="return_int_as_exit_code_else_zero")

    @app.command
    async def serve(bind: str, *, grace: Annotated[NonNegativeFloat, Parameter(env_var="RASM_COMPANION_GRACE")] = 5.0) -> int:
        return _exit(await _daemon(bind, grace, served, drains, charges, ledger, composition))

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
                catch=(msgspec.ValidationError, msgspec.DecodeError, OSError),
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
