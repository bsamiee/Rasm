# [PY_RUNTIME_ROOTS]

Resource-root and transport-resource acquisition: `ResourceRoot` admits file, object-store, and scratch roots over `fsspec`/`universal-pathlib` with traversal-safe relative resolution, and `TransportResource` is the one tagged union over `httpx` HTTP and `asyncssh` SSH/SFTP generic-artifact acquisition. No durable store, daemon scheduler, product store-root derivation, or AEC-collaboration transport crosses this page; Speckle, OPC-UA, and MQTT terminate C#-side per the cross-`libs/` boundary and reach the companion through the canonical wire, never a second Python client leg.

Every acquisition rides one `Transfer` aspect fusing the `reliability/resilience#RESILIENCE` `guarded` retried-traced-railed envelope, so the page mints no second retry loop, derivation span, or inline `try`/`except` ladder. A STREAM acquisition returns its lazy iterator un-fenced and defers the provider-fault lift to the `evidence/identity#IDENTITY` consumer that pulls it. The data-tier object-store bundle I/O — the write direction and bundle-byte reads — is `data:tabular/egress#EGRESS` `ObjectEgress`'s: the two share the `obstore` provider and the `OBJECT_STORE` retry row but split by tier, runtime artifact here, data bundle there.

## [01]-[INDEX]

- [01]-[RESOURCE]: the `Transfer` acquisition aspect over resource roots, references, and transport resources.

## [02]-[RESOURCE]

- Owner: `Transfer` is the one cross-cutting acquisition aspect every reader composes — the fault, transport, lane, and resilience clusters read acquisition only through `Transfer.run`; `TransferPlan` keeps the WHOLE retry-span-fault triplet and the STREAM lazy thunk one shape rather than a five-positional helper signature.
- Cases: the outbound credential resolves through `execution/admission#SETTINGS` `SecretBoundary.resolve` BEFORE case construction and rides as `Option[SecretStr]`, un-masking through `.get_secret_value()` only at the `_BearerAuth`/`_ssh_options` seams — admission owns gating at credential resolution, so re-checking `RuntimeContext.admits` inside `acquire`/`read` is the forbidden double-gate. The `ssh` credential is the connection `password=` authentication secret, distinct from a key-decryption passphrase a `client_keys` field carries; `known_hosts` is the admission-supplied verified database. `ReadModality` is the caller-declared closed-enum discriminant — never an `obstore.head` size probe re-deriving it from a second metadata round-trip.
- Entry: a WHOLE acquisition crosses the one `guarded` envelope, `CLASSIFY` landing the precise tag under the plan's subject — never a `.map_error` re-tag forcing every class to `resource`. The filesystem arm binds `RetryClass.SCAN`, whose `(OSError,)` target retries a transient local read; the `OBJECT_STORE` row's `obstore`-typed target excludes `OSError` and surfaces it terminal. The STREAM iterator feeds the `evidence/identity#IDENTITY` sync fold only after the consumer's async drain — the drain is the boundary seam. `httpx` ships no bearer-`Auth` class, so `_BearerAuth` is the catalog-sanctioned custom `auth_flow`; the whole-payload arm is reserved for small payloads and the stream arm bounds the large GLB/IFC/scan artifacts, per the provider streaming laws.
- Cache: `_object_store`/`_transport_client` memoise one handle per key — `from_url` parses, resolves, and binds credentials, so per-access reconstruction re-pays that cost. Retry is two-tier on both arms: the provider inner envelope (`retry_config=STORE_RETRY`, `AsyncHTTPTransport(retries=CONNECT_RETRIES)`) re-dials at the socket/store edge before the caller-bound `stamina` row fires. `http2=True` is load-bearing — the concurrent-small-object fan-out multiplexes over one negotiated h2 connection instead of queueing on `max_connections`. Status enforcement is the client `event_hooks` response seam bound once, never an asymmetric inline `raise_for_status`.
- Cleanup: an abandoned stream releases its provider session deterministically through one `AsyncExitStack` per leg — `aclose()` raises `GeneratorExit` into the suspended `yield` and the stack unwinds in reverse order — closing only the per-call response context, never the cached client. `drain` is the one teardown the host choreography awaits (`transport/serve#CAPABILITY_INVOKE` names this owner): every pooled client `aclose`s so no pool reaches the GC; the cached `ObjectStore` is a GC-safe Rust handle whose cache only clears.
- Growth: a new storage backend is one `fsspec` protocol or one `obstore` store the `read` dispatch resolves; a new transport is one `TransportResource` case binding a `RetryClass`; a new read size class is one `ReadModality` row — the partial-read identity probe is exactly that next row over `obstore.get_ranges`/`open_reader` when `evidence/identity#IDENTITY` names it; a new inventory or presign mode is one `StoreView` row plus one `@overload`; a new retry geometry is one `RetryClass` row the `TransferPlan` binds.
- Boundary: runtime-transport acquisition only — no default root creation, bridge staging-root ownership, service API layer, or companion-control transport. `OBJECT_STORE_SCHEMES` admits only sign-capable backends, the static proof the `SIGN` arm never hands a non-signing store. Speckle terminates C#-side (`cs:Rasm.Persistence/Version/ledger#SYNC_TRANSPORTS`), and the companion reads any Speckle-sourced artifact through the canonical wire, never a `specklepy` client.

```python signature
from collections.abc import AsyncIterator, Awaitable, Callable, Generator
from contextlib import AsyncExitStack
from datetime import timedelta
from enum import StrEnum
from functools import cache
from typing import TYPE_CHECKING, Final, Literal, Self, assert_never, overload
from urllib.parse import urlsplit

from expression import Error, Ok, Option, case, tag, tagged_union
from msgspec import Struct
from pydantic import SecretStr

import anyio
import asyncssh
import httpx
import obstore
from anyio import CapacityLimiter
from obstore import Bytes
from obstore.store import ObjectStore
from upath import UPath

from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.resilience import RetryClass, guarded

if TYPE_CHECKING:
    from arro3.core import RecordBatch
    from obstore.store import RetryConfig

# --- [TYPES] ----------------------------------------------------------------------------

type Chunk = Bytes | bytes
type Acquired = Chunk | AsyncIterator[Chunk]
type WholeFetch = Callable[[], Awaitable[Chunk]]
type StreamOpen = Callable[[], AsyncIterator[Chunk]]


class ReadModality(StrEnum):
    WHOLE = "whole"
    STREAM = "stream"


class StoreView(StrEnum):
    LIST = "list"
    SIGN = "sign"


# --- [CONSTANTS] ------------------------------------------------------------------------

OBJECT_STORE_SCHEMES: Final[frozenset[str]] = frozenset({"s3", "gs", "az", "abfs"})

STREAM_CHUNK: Final[int] = 1 << 20
LIST_CHUNK: Final[int] = 1000
SIGN_TTL: Final[timedelta] = timedelta(seconds=900)

# `obstore.store.RetryConfig` is a TYPE_CHECKING TypedDict, not a runtime constructor — the
# dict literal IS the Rust-core inner retry envelope under the `OBJECT_STORE` stamina row.
STORE_RETRY: Final[RetryConfig] = {"max_retries": 3, "retry_timeout": 30.0}
CONNECT_RETRIES: Final[int] = 2

# the filesystem blocking-read bound: every roots thread hop rides this explicit CapacityLimiter, never the ambient default limiter.
SCAN_BAND: Final[CapacityLimiter] = CapacityLimiter(8)
TRANSPORT_TIMEOUT: Final[httpx.Timeout] = httpx.Timeout(connect=5.0, read=30.0, write=30.0, pool=5.0)
TRANSPORT_LIMITS: Final[httpx.Limits] = httpx.Limits(max_connections=16, max_keepalive_connections=8)


# --- [MODELS] ---------------------------------------------------------------------------


class TransferPlan(Struct, frozen=True):
    subject: str
    retry_class: RetryClass
    whole: WholeFetch
    stream: StreamOpen


class ResourceRef(Struct, frozen=True):
    scheme: str
    root: str
    relative: str
    owner: str

    @property
    def path(self) -> UPath:
        return UPath(self.root, protocol=self.scheme) / self.relative


# --- [OPERATIONS] -----------------------------------------------------------------------


class _BearerAuth(httpx.Auth):
    # `.get_secret_value()` runs exactly once, here at the transport seam; the bound `_header` is the only `str` the credential becomes.
    def __init__(self, token: SecretStr) -> None:
        self._header = f"Bearer {token.get_secret_value()}"

    def auth_flow(self, request: httpx.Request) -> Generator[httpx.Request, httpx.Response, None]:
        request.headers["Authorization"] = self._header
        yield request


def _auth(bearer: Option[SecretStr]) -> httpx.Auth | None:
    return bearer.map(_BearerAuth).to_optional()


class Transfer:
    @staticmethod
    async def run(plan: TransferPlan, modality: ReadModality) -> RuntimeRail[Acquired]:
        match modality:
            case ReadModality.STREAM:
                return Ok(plan.stream())
            case ReadModality.WHOLE:
                return await guarded(plan.retry_class, plan.whole, subject=plan.subject)
            case _ as unreachable:
                assert_never(unreachable)


# --- [SERVICES] -------------------------------------------------------------------------


class ResourceRoot(Struct, frozen=True):
    scheme: str
    root: str
    owner: str

    @classmethod
    def admit(cls, uri: str, owner: str) -> Self:
        path = UPath(uri)
        return cls(scheme=path.protocol or "file", root=str(path), owner=owner)

    def child(self, relative: str) -> RuntimeRail[ResourceRef]:
        # containment is resolved-child against resolved-root; a string-prefix check is spoofed by a sibling root.
        base = UPath(self.root)
        resolved = (base / relative).resolve()
        return (
            Ok(ResourceRef(self.scheme, self.root, relative, self.owner))
            if resolved.is_relative_to(base.resolve())
            else Error(BoundaryFault(resource=(self.owner, f"traversal:{relative}")))
        )

    async def read(self, ref: ResourceRef, modality: ReadModality = ReadModality.WHOLE) -> RuntimeRail[Acquired]:
        store = self._store()
        plan = (
            TransferPlan(ref.owner, RetryClass.SCAN, lambda: anyio.to_thread.run_sync(ref.path.read_bytes, limiter=SCAN_BAND), lambda: _file_chunks(ref))
            if store is None
            else TransferPlan(ref.owner, RetryClass.OBJECT_STORE, lambda: _obj_body(store, ref.relative), lambda: _obj_chunks(store, ref.relative))
        )
        return await Transfer.run(plan, modality)

    @overload
    async def survey(self, ref: ResourceRef, view: Literal[StoreView.LIST] = ...) -> RuntimeRail[AsyncIterator[RecordBatch]]: ...
    @overload
    async def survey(self, ref: ResourceRef, view: Literal[StoreView.SIGN]) -> RuntimeRail[str]: ...
    async def survey(self, ref: ResourceRef, view: StoreView = StoreView.LIST) -> RuntimeRail[AsyncIterator[RecordBatch]] | RuntimeRail[str]:
        store = self._store()
        if store is None:
            return Error(BoundaryFault(resource=(ref.owner, f"survey.{view}.non-object-store")))
        match view:
            case StoreView.LIST:
                return Ok(obstore.list(store, ref.relative, chunk_size=LIST_CHUNK, return_arrow=True))
            case StoreView.SIGN:
                return await guarded(RetryClass.OBJECT_STORE, lambda: obstore.sign_async(store, "GET", ref.relative, SIGN_TTL), subject=ref.owner)
            case _ as unreachable:
                assert_never(unreachable)

    def _store(self) -> ObjectStore | None:
        return _object_store(self.root) if self.scheme in OBJECT_STORE_SCHEMES else None


@tagged_union(frozen=True)
class TransportResource:
    tag: Literal["http", "ssh"] = tag()
    http: tuple[str, RetryClass, Option[SecretStr]] = case()
    ssh: tuple[str, int, RetryClass, Option[SecretStr], asyncssh.SSHKnownHosts] = case()

    async def acquire(self, relative: str, modality: ReadModality = ReadModality.WHOLE) -> RuntimeRail[Acquired]:
        match self:
            case TransportResource(tag="http", http=(url, retry_class, bearer)):
                client, auth = _transport_client(_endpoint(url)), _auth(bearer)
                plan = TransferPlan("http", retry_class, lambda: _http_body(client, url, auth), lambda: _http_chunks(client, url, auth))
            case TransportResource(tag="ssh", ssh=(host, port, retry_class, password, known_hosts)):
                options = _ssh_options(password, known_hosts)
                plan = TransferPlan(
                    "ssh", retry_class, lambda: _sftp_read(host, port, relative, options), lambda: _sftp_chunks(host, port, relative, options)
                )
            case _ as unreachable:
                assert_never(unreachable)
        return await Transfer.run(plan, modality)


# --- [COMPOSITION] ----------------------------------------------------------------------


async def _raise_for_status(response: httpx.Response) -> None:
    response.raise_for_status()


def _endpoint(url: str) -> str:
    # the group is `scheme://host:port` off `hostname`/`port`, NEVER the raw `netloc`: netloc carries `user:password@` userinfo,
    # leaking the credential into the cache key and pooling the client by secret rather than by destination.
    parsed = urlsplit(url)
    return f"{parsed.scheme}://{parsed.hostname}:{parsed.port}" if parsed.port else f"{parsed.scheme}://{parsed.hostname}"


@cache
def _object_store(root: str) -> ObjectStore:
    return obstore.store.from_url(root, retry_config=STORE_RETRY)


_LIVE_CLIENTS: Final[set[httpx.AsyncClient]] = set()


@cache
def _transport_client(endpoint: str) -> httpx.AsyncClient:
    # `_LIVE_CLIENTS` is the drainable handle because `functools.cache` surrenders no value enumeration to the `drain` teardown.
    client = httpx.AsyncClient(
        timeout=TRANSPORT_TIMEOUT,
        transport=httpx.AsyncHTTPTransport(retries=CONNECT_RETRIES, http2=True, limits=TRANSPORT_LIMITS),
        event_hooks={"response": [_raise_for_status]},
    )
    _LIVE_CLIENTS.add(client)
    return client


async def drain() -> None:
    # `aclose` every pooled client so no pool reaches the GC; the Rust `ObjectStore` has no async pool.
    clients = tuple(_LIVE_CLIENTS)
    _LIVE_CLIENTS.clear()
    async with anyio.create_task_group() as tg:
        for client in clients:
            tg.start_soon(client.aclose)
    _transport_client.cache_clear()
    _object_store.cache_clear()


# --- [BOUNDARIES] -----------------------------------------------------------------------
# Per-provider WHOLE/STREAM legs the `[SERVICES]` `TransferPlan` thunks bind by call-time name.


async def _obj_body(store: ObjectStore, relative: str) -> Bytes:
    return (await obstore.get_async(store, relative)).bytes()


async def _obj_chunks(store: ObjectStore, relative: str) -> AsyncIterator[Bytes]:
    result = await obstore.get_async(store, relative)
    async for chunk in result.stream(STREAM_CHUNK):
        yield chunk


async def _file_chunks(ref: ResourceRef) -> AsyncIterator[bytes]:
    handle = await anyio.to_thread.run_sync(ref.path.open, "rb", limiter=SCAN_BAND)
    try:
        while block := await anyio.to_thread.run_sync(handle.read, STREAM_CHUNK, limiter=SCAN_BAND):
            yield block
    finally:
        handle.close()


async def _http_body(client: httpx.AsyncClient, url: str, auth: httpx.Auth | None) -> bytes:
    return (await client.get(url, auth=auth)).content


async def _http_chunks(client: httpx.AsyncClient, url: str, auth: httpx.Auth | None) -> AsyncIterator[bytes]:
    async with AsyncExitStack() as stack:
        response = await stack.enter_async_context(client.stream("GET", url, auth=auth))
        async for chunk in response.aiter_bytes():
            yield chunk


def _ssh_options(password: Option[SecretStr], known_hosts: asyncssh.SSHKnownHosts) -> asyncssh.SSHClientConnectionOptions:
    # one connection-config value object over per-call `connect(...)` keyword soup; the password un-masks only here, and
    # `known_hosts` is the admission-supplied verified database, never the disabled `None`.
    return asyncssh.SSHClientConnectionOptions(password=password.map(SecretStr.get_secret_value).default_value(None), known_hosts=known_hosts)


async def _sftp_session(
    stack: AsyncExitStack, host: str, port: int, relative: str, options: asyncssh.SSHClientConnectionOptions
) -> asyncssh.SFTPClientFile:
    conn = await stack.enter_async_context(asyncssh.connect(host, port=port, options=options))
    sftp = await stack.enter_async_context(conn.start_sftp_client())
    return await stack.enter_async_context(await sftp.open(relative, "rb"))


async def _sftp_read(host: str, port: int, relative: str, options: asyncssh.SSHClientConnectionOptions) -> bytes:
    async with AsyncExitStack() as stack:
        return await (await _sftp_session(stack, host, port, relative, options)).read()


async def _sftp_chunks(host: str, port: int, relative: str, options: asyncssh.SSHClientConnectionOptions) -> AsyncIterator[bytes]:
    async with AsyncExitStack() as stack:
        handle = await _sftp_session(stack, host, port, relative, options)
        while block := await handle.read(STREAM_CHUNK):
            yield block
```


## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
