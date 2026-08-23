# [PYTHON_TRANSPORT]

This page is the wire-transport law: how a peer-decoded seam crosses as its generated protobuf class, how a Connect service is served and dialed, and how a refusal crosses as typed details. The corpus `.proto` is the one definition of every peer-decoded seam; `protoc-gen-py` and `protoc-gen-connectrpc` emit the classes, applications, and clients under one package tree, and every fence imports those — never a hand message, a msgspec twin, a hand frame, or a second RPC transport. Attribute admission and the converter owner for documents no peer decodes are `boundaries.md`'s; structured concurrency under the host is `concurrency.md`'s; the fault family and its rail are `rails-and-effects.md`'s.

## [01]-[WIRE_PLANE]

Every cross-language seam resolves to one generated surface; the table names the surface and the form it deletes.

| [INDEX] | [CONCERN]              | [USE]                                                             | [REJECTED_FORM]                             |
| :-----: | :--------------------- | :---------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | message vocabulary     | generated `<family>_pb` classes under `rasm.contracts.gen`        | msgspec or pydantic twin of a message       |
|  [02]   | binary crossing        | `Message.to_binary` / `Message.from_binary`                       | magic + layout cell + `struct.unpack` frame |
|  [03]   | JSON crossing          | `Message.to_json` / `Message.from_json` with one `Registry`       | `json.dumps` of a hand mapping              |
|  [04]   | oneof construction     | `Oneof(field, value)` on the oneof attribute; `None` unset        | per-arm keyword constructors                |
|  [05]   | optional presence      | `Message.has_field`; a message slot reads `None`                  | the proto zero read as a stated value       |
|  [06]   | type resolution        | one module-level `Registry(*(module.desc() for ...))`             | a `Registry(...)` per call site             |
|  [07]   | foreign-extension slot | `wkt.Any.pack` / `Any.unpack(type_info)` through that registry    | `Any` over an estate-owned op family        |
|  [08]   | serving                | `<Svc>ASGIApplication` under hypercorn                            | uvicorn, a hand route table                 |
|  [09]   | dialing                | `<Svc>Client` / `ConnectClient.execute_*` over a `pyqwest` client | a second RPC transport; raw HTTP framing    |
|  [10]   | call boundaries        | metadata admission plus native body interceptors                  | handler prologues; field-rule mirrors       |
|  [11]   | deadline               | `ctx.timeout_ms` (REMAINING, recomputed per read)                 | an unbounded handler; a hand deadline clock |
|  [12]   | refusal                | `ConnectError(code, message, details=[<details>])`                | a stringified fault; a hand trailer         |
|  [13]   | detail ingress         | `ErrorDetail.value(DetailClass)` over `ConnectError.details`      | a trailer read; a code-to-taxonomy table    |
|  [14]   | compression            | `compressions=`/`accept_compression=` rows, zstd before gzip      | gzip-only over binary bodies                |
|  [15]   | body ceiling           | `read_max_bytes=` on every mount and dial                         | an unbounded mount                          |

[GENERATED_VOCABULARY]:
- Use when: a value crosses to a peer runtime — a reply, a stream frame, a detail, an event payload, a frozen artifact another branch decodes.
- Accept: the generated class as the only shape, imported by its one package path — `rasm.contracts.gen.rasm.contracts.<family>.v1.<family>_pb` and `<family>_connect`, dependencies at `rasm.contracts.gen.buf.validate`, `rasm.contracts.gen.google.rpc`, `rasm.contracts.gen.google.type`, vendored publishers at `rasm.contracts.vendor.grpc.health.v1` and `rasm.contracts.vendor.io.cloudevents.v1`; well-known types from `protobuf.wkt`; corpus enums with their prefix stripped (`Modality.IFC`), read by MEMBER NAME wherever a branch vocabulary corresponds to one.
- Law: construction and assignment validate NOTHING — `to_binary` and `to_json` run the check and raise `TypeError` on a wrong-typed slot, `OverflowError` on a scalar outside its wire range, `ValueError` on a malformed oneof or value — so the one admission site on the proto plane is the fence that encodes, its `catch` names all three, and a decode-only path never fires the check; `from_binary` and `from_json` raise `ValueError` alone.
- Law: an `optional` scalar constructs on `T | None`, reads its proto zero when unset, and answers presence through `has_field`; a message slot reads `None`; a oneof reads `Oneof(field, value)` or `None`. Presence is the only way a caller states a budget, a window, or a second dimension, so a consumer reads `has_field`, never the zero.
- Law: corpus `protovalidate` rules are the constraint authority and the runtime evaluates them once at each foreign body boundary. Request violations refuse as `INVALID_ARGUMENT`, response violations refuse as `INTERNAL`, and the structured `buf.validate.Violations` detail crosses intact; no branch re-spells a field rule in application code. Protobuf-py's encode-time structural checks remain the separate construction fence.
- Law: one `Registry` over every generated `desc()` this branch binds, seated once at module scope — `to_json` over a packed `Any`, `from_json` of a `@type`, and `ErrorDetail.value(registry)` resolve there; a second registry anywhere is a second authority for one type name.
- Reject: a `msgspec.Struct` restating a generated message; a string column carrying a closed roster the corpus spells as an enum; a hex-string identity where the corpus carries 16 bytes; a hand-framed binary beside a proto plane; a digest over `to_binary()` bytes, which protobuf-py holds non-canonical across releases.
- Boundary: the one MessagePack wire that survives is an op-log every branch mints as a positional record under its own envelope, ruled at its owning page with the discriminant stated on site; every other non-proto document is a record no peer decodes and belongs to `boundaries.md`'s converter owner.

[SERVE_AND_DIAL]:
- Use when: a service the corpus declares is served by this branch or dialed from it.
- Accept: the generated `<Svc>` protocol implemented by a servicer with `@override` per rpc; its `<Svc>ASGIApplication` constructed under ONE mount policy — the `interceptors` tuple, `read_max_bytes`, and the `compressions` roster — mounted at its own `path` under `hypercorn.middleware.DispatcherMiddleware` and served by `hypercorn` on asyncio, because the Connect server and client both run asyncio loop primitives and a bidi rpc needs HTTP/2; `<Svc>Client` or `ConnectClient.execute_unary(*, request, method)` over an injected `pyqwest` client with `accept_compression`, `send_compression`, and `read_max_bytes` from the same policy row.
- Law: the served roster EQUALS the generated rpc set — a boot census compares each served application's `path` and the corpus `DescService.methods` the registry resolves against the roster in BOTH directions, so a corpus rpc with no handler row and a row no rpc backs each refuse at boot and no mounted service answers `UNIMPLEMENTED` at a peer's first dial.
- Law: metadata work rides one `MetadataInterceptor` pair — `on_start(ctx)` returns the token `on_end(token, ctx, error)` receives with the terminal error — so identity, deadline, contextvars, timing, and span enrichment span every rpc shape once. Body constraint validation is a separate interceptor implementing Connect's native unary, client-stream, server-stream, and bidi protocols, validating every request and response element without handler prologues or stream materialization; server spans remain `OpenTelemetryMiddleware`'s off the ASGI scope with `exclude_spans=["receive", "send"]`.
- Law: a request PRE-ENCODES under its own fence BEFORE any retried call — the client's terminal fence maps every unclassified raise, the request's own `to_binary` refusal included, to `UNAVAILABLE`, the one status a retry policy grades transient — so a caller-repairable encode refusal is a terminal `config` fault that spends zero attempts.
- Law: `read_max_bytes` bounds the decompressed unary body at the server and the response payload at the client (`RESOURCE_EXHAUSTED` past it, per envelope on a stream) and `compressions` negotiates per call with identity always surviving; both are ONE policy row per runtime profile, read by every mount and every dial, never a per-site literal and never left unset.
- Reject: `uvicorn` under a bidi service; `hypercorn.trio` hosting a Connect application; a hand route table or `_invoke` body beside the generated application; `ConnectClient.close()` standing in for closing the `pyqwest` transport; a retry wrapping the encode.

[FAULT_DETAIL]:
- Use when: a refusal crosses the wire in either direction.
- Accept: one `ConnectError(code, message, details=[...])` raised at the serve edge, carrying the corpus `FaultDetail` and, when a retry window is stated, the recovery arm's OWN `google.rpc.RetryInfo` packed a second time; ingress reads `ErrorDetail.value(DetailClass)` over `ConnectError.details` — protocol-agnostic on Connect, gRPC, and gRPC-Web — and a raise carrying no decodable detail answers absence, never a forged verdict.
- Law: the detail's `domain` is the PRODUCING family and `case` that family's closed ordinal — the emitting leg and its row's position in declaration order — and never the Connect code, which the same refusal carries separately on `ConnectError.code`; `connectrpc.Code` is a string `Enum`, `int(code)` raises, and no ordinal of it is a wire fact. A peer keeps the `(domain, case)` pair opaque: a remote family never elects local topology, and an unseated subject crosses at the unspecified zero under the serving leg.
- Law: re-drive is TWO axes — the transport code the retry class grades and the producer's own `recovery` verdict off the decoded detail — read together at the one seam that holds a live `ConnectError`: a stated `terminal` refuses a re-drive the code would allow, a stated window replaces the backoff curve, and an unstated verdict defers to the class; a consumer band-mapping a code back to a verdict substitutes its own table for the producer's.
- Law: `FaultDetail.recovery.retry_after` IS a `google.rpc.RetryInfo`, so the estate detail and the standard advice detail carry ONE message and not two projections that can disagree: the throttled arm is minted once at the recovery correspondence, and the advice seat packs that instance. A second `RetryInfo` construction beside the detail, or a window rebuilt from the transport code, is the deleted form. `retry_delay` is a message slot the generator spells optional and the corpus rule forces present, so the arm's admission collapses that optionality and refuses an arm claiming a window while stating none.
- Law: field-scoped refusals cross as `google.rpc.BadRequest.FieldViolation` rows on the detail — the one place a field path crosses — each carrying the row's defect token as `reason`; the fault's tag, detail string, and facts stay LOCAL on the producer's span and log line, joinable on the detail's correlation.
- Reject: `int(Code)`; a `FaultDetail` built from a status; a hand trailer beside the details channel; a consumer that decodes `domain` into its own union; a second code-to-fault table beside the one the transport owner declares.

```python conceptual
from collections.abc import AsyncIterator, Callable, Iterable
from typing import Final, Literal

from connectrpc.code import Code
from connectrpc.compression import Compression
from connectrpc.compression.gzip import GzipCompression
from connectrpc.compression.zstd import ZstdCompression
from connectrpc.errors import ConnectError, ErrorDetail
from connectrpc.interceptor import Interceptor
from connectrpc.method import IdempotencyLevel, MethodInfo
from connectrpc.request import RequestContext
from connectrpc.server import ConnectASGIApplication, Endpoint
from expression import Error, Ok, Result
from protobuf import Message, Oneof, Registry
from protobuf.wkt import Any, Duration, Empty, FieldMask, Struct, any_pb, duration_pb, empty_pb, field_mask_pb, struct_pb
from rasm.contracts.gen.google.rpc import error_details_pb
from rasm.contracts.gen.google.rpc.error_details_pb import RetryInfo

type Profile = Literal["<profile-a>", "<profile-b>"]
type Encode = Literal["<wrong-type>", "<out-of-range>", "<malformed>"]

ENCODE_RAISES: Final[tuple[type[Exception], ...]] = (TypeError, ValueError, OverflowError)
REGISTRY: Final[Registry] = Registry(
    *(module.desc() for module in (any_pb, duration_pb, empty_pb, error_details_pb, field_mask_pb, struct_pb))
)
_ZSTD: Final[Compression] = ZstdCompression()
_GZIP: Final[Compression] = GzipCompression()
POLICY: Final[dict[Profile, tuple[int, tuple[Compression, ...]]]] = {
    "<profile-a>": (64 << 20, (_ZSTD, _GZIP)),
    "<profile-b>": (1 << 30, (_ZSTD, _GZIP)),
}
_ENCODE_CASE: Final[dict[type[Exception], Encode]] = {TypeError: "<wrong-type>", OverflowError: "<out-of-range>", ValueError: "<malformed>"}


class Admission:
    async def on_start(self, ctx: RequestContext[Message, Message]) -> int | None:
        return ctx.timeout_ms

    async def on_end(self, token: int | None, ctx: RequestContext[Message, Message], error: Exception | None) -> None:
        return


class Echo:
    async def echo(self, request: Struct, ctx: RequestContext[Struct, Struct]) -> Struct:
        return request

    async def watch(self, request: Struct, ctx: RequestContext[Struct, Struct]) -> AsyncIterator[Struct]:
        yield request


_ECHO: Final[MethodInfo[Struct, Struct]] = MethodInfo(
    name="Echo", service_name="<package>.<Service>", input=Struct, output=Struct, idempotency_level=IdempotencyLevel.NO_SIDE_EFFECTS
)
_WATCH: Final[MethodInfo[Struct, Struct]] = MethodInfo(
    name="Watch", service_name="<package>.<Service>", input=Struct, output=Struct, idempotency_level=IdempotencyLevel.UNKNOWN
)


class EchoApplication(ConnectASGIApplication[Echo]):
    # the shape the generator emits per service: the endpoint map keyed `/<package>.<Service>/<Method>` and the mount `path`
    def __init__(self, service: Echo, *, interceptors: Iterable[Interceptor] = (), read_max_bytes: int | None = None, compressions: Iterable[Compression] | None = None) -> None:
        super().__init__(
            service=service,
            endpoints=lambda svc: {
                f"/{_ECHO.service_name}/{_ECHO.name}": Endpoint.unary(method=_ECHO, function=svc.echo),
                f"/{_WATCH.service_name}/{_WATCH.name}": Endpoint.server_stream(method=_WATCH, function=svc.watch),
            },
            interceptors=interceptors,
            read_max_bytes=read_max_bytes,
            compressions=compressions,
        )

    @property
    def path(self) -> str:
        return f"/{_ECHO.service_name}"


def mounted(service: Echo, profile: Profile, interceptors: Iterable[Interceptor]) -> EchoApplication:
    ceiling, rows = POLICY[profile]  # one policy row per profile, read by every mount and every dial
    return EchoApplication(service, interceptors=interceptors, read_max_bytes=ceiling, compressions=rows)


def encoded(request: Message) -> Result[bytes, Encode]:
    try:
        return Ok(request.to_binary())
    except ENCODE_RAISES as refused:  # the one admission site on the proto plane; a retry never wraps it
        return Error(_ENCODE_CASE[type(refused)])


def refused(code: Code, detail: Message, window: Duration | None) -> ConnectError:
    advice = None if window is None else RetryInfo(retry_delay=window)  # the ONE construction site for the window
    recovery = Oneof("terminal", Empty()) if advice is None else Oneof("retry_after", advice)
    standard = (Any.pack(FieldMask(paths=["<field>"])),) if advice is None else (Any.pack(advice),)  # the arm ITSELF
    return ConnectError(code, f"{recovery.field}", details=[Any.pack(detail), *standard])


def stated[D: Message](raised: Exception, into: type[D]) -> D | None:
    match raised:
        case ConnectError(details=details):
            return next((value for value in (detail.value(into) for detail in details) if value is not None), None)
        case _:
            return None


def redrive(raised: Exception, grade: Callable[[Code], bool | float]) -> bool | float:
    match raised, stated(raised, RetryInfo):
        case (ConnectError(), RetryInfo(retry_delay=Duration() as window)):
            return window.to_seconds()
        case (ConnectError(code=code), _):  # no advice detail, or one stating no delay, grades on the code alone
            return grade(code)
        case _:
            return False
```

[BOOT_CENSUS]:
- Use when: a process mounts generated applications or dials generated services.
- Law: structure proves before custody is claimed — the roster-versus-descriptor census, the registry seat, and every closed-family closure run once ahead of any install that takes process ownership, reading no installed state; a drifted roster reported after an OTel global or a hook table is claimed leaves a process holding surfaces its refusal cannot unmount.
- Law: the census reads generated surfaces and never re-derives them — the application's `path`, the descriptor's `methods`, a field's `local_name` — so the generator's own spelling rules are never transcribed into a regex the next generator release breaks.
- Reject: a runtime descriptor walk standing in for `buf breaking`; a `PROTO_VOCABULARY` row beside a generated class; a census that reads one direction.
