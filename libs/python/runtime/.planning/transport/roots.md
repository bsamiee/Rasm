# [PY_RUNTIME_ROOTS]

Resource roots and transport resources. `ResourceRoot` admits file/object-store/scratch roots over `fsspec`/`universal-pathlib` with traversal-safe relative resolution, routing many-small-object reads through the `obstore` async fast-path while keeping the `fsspec` tree-walk semantics for the shared-pull tree; `TransportResource` is the one tagged union over HTTP/SSH generic-artifact acquisition. No durable store, daemon scheduler, app lifecycle hook, product store-root derivation, or AEC-collaboration transport (Speckle/OPC-UA/MQTT terminate C#-side and reach the companion through the canonical wire, never a second Python client leg) crosses this page.

## [01]-[INDEX]

- [01]-[RESOURCE]: resource roots, references, transport resources.

## [02]-[RESOURCE]

- Owner: `ResourceRoot` — file/object-store/scratch roots over `fsspec` + `universal-pathlib` with safe relative resolution; `ResourceRef` the scheme/root/relative/owner value object; `TransportResource` the one tagged union over `httpx` HTTP and `asyncssh` SSH/SFTP generic-artifact acquisition (a remote-fetch transport row, never a durable store and never an AEC-collaboration client).
- Cases: `TransportResource` cases `http=(url, retry_class, bearer)` · `ssh=(host, port, retry_class, passphrase, known_hosts)` — keyword-constructed and matched by `match`/`case`, each binding a `RetryClass` discriminant resolved against `reliability/resilience#RESILIENCE` `guard` and an `Option[str]` outbound credential the caller resolves through `execution/admission#SETTINGS` `SecretBoundary.resolve` before constructing the case, the `ssh` case additionally binding the verified `asyncssh.SSHKnownHosts` database the same `SecretBoundary`/settings owner loads through `asyncssh.read_known_hosts` so the connection law's host-key verification is admission-supplied, never disabled; `acquire(relative, modality)` takes the remote member to fetch and the same `relative` the filesystem `read` resolves; the read modality — whole-payload `Bytes` (small) or chunk `AsyncIterator[Bytes]` (large) — rides the `ReadModality` data discriminant the shared `_acquire` aspect matches (`MODAL_ARITY`, one entrypoint owns all modalities, the modality a closed-enum value not a `streaming=True` knob), never a `read`/`stream` name-suffix split; `match self` dispatches on the union tag once and the `_acquire` aspect dispatches on `ReadModality` once, the four prior `(tag, modality)` cartesian arms collapsing to two tag arms each handing the aspect a `(whole, stream)` thunk pair.
- Entry: `ResourceRoot.admit` admits one root and returns the frozen owner; `ResourceRoot.child` resolves a relative path with traversal rejection through `UPath.resolve`/`is_relative_to` containment, the failure constructed as the `resource` case of `reliability/faults#FAULT`; `_acquire(owner, catch, modality, whole, stream)` is the one cross-cutting acquisition aspect every reader composes — it returns `Ok(stream())` for the lazy `ReadModality.STREAM` iterator and lifts the awaitable `whole` thunk through the `reliability/faults#FAULT` `async_boundary(owner, whole, catch=...)` fence for `ReadModality.WHOLE`, the `owner` threading as the `async_boundary` `subject` so the `reliability/faults#FAULT` `CLASSIFY` fold lands the precise tag — an `OSError` as the owner-named `resource` case, a `TimeoutError` as `deadline`, an `obstore`/`httpx`/`asyncssh` provider raise as `boundary` — without a lossy `.map_error` re-tag that forces every class to `resource` and reads a non-active `fault.boundary` case slot, so `read`/`survey`/`acquire` share one fault lift instead of the four inline `try`/`except` blocks the prior page hand-rolled; `ResourceRoot.read(ref, modality)` is the one polymorphic reader returning `RuntimeRail[Acquired]` — it picks the `(whole, stream)` thunk pair by scheme (object-store to `_obj_body`/`_obj_chunks` over `obstore.get_async` + `GetResult.bytes()`/`GetResult.stream(STREAM_CHUNK)`, filesystem to `anyio.to_thread.run_sync(UPath.read_bytes)`/`_file_chunks` so the blocking-IO read offloads to the anyio worker-thread pool rather than stalling the event loop) and hands it to `_acquire`; the WHOLE object-store path runs the `get_async` retry and the `.bytes()` materialize inside one `_obj_body` coroutine so both cross the single fence, and the STREAM chunk iterator folds straight into `evidence/identity#IDENTITY` `ContentIdentity.of` over the existing `Iterable[bytes]` modality (buffer-protocol `Bytes` into `xxh3_128.update`) with no full-artifact buffer; `ResourceRoot.survey(ref, view)` is the second polymorphic object-store entrypoint discriminating on the `StoreView` enum — `StoreView.LIST` streams the Arrow `RecordBatch` inventory through `obstore.list(store, prefix, chunk_size=LIST_CHUNK, return_arrow=True)` (the columnar listing fast-path the `data` Arrow consumers downstream join against, never a per-`ObjectMeta` Python loop; the lazy `ListStream` construction is non-failing so it returns `Ok` directly, and the downstream `data` Arrow consumer owns per-`RecordBatch` iteration-fault conversion as it pulls the stream) and `StoreView.SIGN` mints the presigned URL through `obstore.sign_async(store, "GET", paths, timedelta)` under the shared `async_boundary(ref.owner, ...)` fence (the lifted fault carrying the owner subject directly, no re-tag) so a downstream companion fetch is a bare `httpx` GET against a time-boxed URL with no credential leg, both arms object-store-scheme-gated through `_store`; `TransportResource.acquire(relative, modality)` is the matching one entrypoint — `match self` binds the `http`/`ssh` case payload and hands `_acquire` the per-tag `(whole, stream)` pair, the WHOLE arm streaming a small body through `_http_body`/`_sftp_read` and the STREAM arm through `_http_chunks`/`_sftp_chunks` over `httpx.AsyncClient.stream`/`Response.aiter_bytes` and the SFTP file handle under explicit `httpx.Timeout`/`httpx.Limits`, the bound `RetryClass` caller from `reliability/resilience#RESILIENCE` `guard` wrapping each provider call and the raised provider fault converting at the one `_acquire` `async_boundary` boundary into the `CLASSIFY`-precise tag, an exhausted `assert_never` anchoring the closed two-case union; the outbound credential reads from the `execution/admission#SETTINGS` `SecretBoundary.resolve` and rides the case as an `Option[str]` — the `ssh` passphrase threads into `asyncssh.connect(..., password=...)` and the `http` bearer rides the `httpx`-native `Auth` flow `_BearerAuth` the `_auth` projection mints per acquisition and `_http_body`/`_http_chunks` pass as the per-call `auth=` argument, the `httpx` auth-law credential seam owning the `Authorization` header rather than a hand-rolled header dict, a `Nothing` credential resolving to `auth=None` (the client default, never an inline literal); the inbound 4xx/5xx status check is the client-level `event_hooks={"response": [raise_for_status]}` seam bound once at `_client_for` construction (the `httpx` event-hook law), so both the WHOLE `_http_body` and the STREAM `_http_chunks` enforce status at the one transport seam rather than the prior asymmetric inline `raise_for_status` the WHOLE arm omitted; the `ssh` arm always verifies the host key against the known-hosts database the `execution/admission#SETTINGS` owner threads through `asyncssh.read_known_hosts` (ENTRYPOINTS [11]) into `asyncssh.connect(..., known_hosts=...)`, never the disabled-verification `known_hosts=None` the `asyncssh` RAIL_LAW Reject-row forbids.
- Cache: `obstore.store.from_url` is a Rust handle whose construction parses the URL, resolves the backend, and binds the credential provider, so a per-access reconstruction re-pays that cost on every `read`/`survey` of the same root; the `_STORE_CACHE` module memo keyed by the root URL constructs once through `_store` (walrus-assigned cache fill on miss) and returns the cached `ObjectStore` on every subsequent access, the handle being a thread-safe immutable client the concurrent-small-object fan-out shares — never a fresh store per `read`, never a store rebuilt per chunk, never a hand-rolled fsspec cache the `fsspec` Reject-row forbids. The symmetric `_HTTP_CACHE` module memo keyed by the `http` URL's scheme-host-port endpoint group constructs one long-lived `httpx.AsyncClient(timeout=TRANSPORT_TIMEOUT, limits=TRANSPORT_LIMITS)` through `_client_for` and shares its pool and `httpx.Limits` across every WHOLE/STREAM acquisition of that endpoint — the `httpx` client-law one-long-lived-client-per-endpoint-group reuse, the per-request `AsyncClient(...)` construction the `httpx` Reject-row forbids deleted here; the pooled client lives for the runtime, so `_http_chunks` reuses the cached client and only its per-call `stream` response-context rides the per-acquisition `AsyncExitStack` (never the client itself), never an `aclose` per fetch that would tear down the shared pool.
- Auto: `OBJECT_STORE_SCHEMES` routes `s3`/`gs`/`az`/`abfs` to the `obstore` stateless API (abi3 wheel, cp315-ready) for the concurrent-small-object reads that dominate companion artifact pulls, and routes every other scheme through `UPath` filesystem semantics; one polymorphic `read`/`survey`/`acquire` entrypoint per owner owns both schemes and both modalities under the `ReadModality`/`StoreView` discriminants, the whole-payload `Bytes` arm reserved for small payloads and the `AsyncIterator[Bytes]` arm for the large GLB/IFC/scan artifacts the `httpx`/`obstore` streaming surfaces exist to bound — a full-body `client.get` of a large payload is the `httpx` Reject-row violation (catalogue IMPLEMENTATION_LAW streaming law) and a `GetResult.bytes()` of a large object is the `obstore` Reject-row violation (catalogue RAIL_LAW), both deleted here; traversal containment compares the resolved child against the resolved root, never a string prefix that a sibling root spoofs; the rail never returns a bare `Ok` over a throwing provider call — every WHOLE acquisition crosses the one `_acquire` `async_boundary` conversion (the lifted fault carrying the owner subject and the `CLASSIFY`-precise tag, never a forced `resource` re-tag reading a non-active case slot) and every STREAM acquisition defers its provider-fault lift to the `evidence/identity#IDENTITY` consumer that pulls the iterator, the single fault aspect replacing the per-provider `try`/`except` ladders the catalogue Reject-rows forbid.
- Cleanup: a streamed acquisition the consumer abandons (an early `break`, a fault upstream, a partial-read identity probe) must release the provider session deterministically, so `_sftp_session` opens the `asyncssh.connect`/`start_sftp_client`/`SFTPClient.open` chain through one `contextlib.AsyncExitStack` the `_sftp_read` whole-fetch and the `_sftp_chunks` generator both compose — for the streaming generator the stack's `__aexit__` runs in the generator's `finally`, so when the consumer stops iterating the generator `aclose()` raises `GeneratorExit` into the suspended `yield`, the `finally` unwinds the stack, and the SFTP file, the SFTP client, and the SSH connection all close in reverse order with no leaked socket across the event loop (the `asyncssh` Reject-row leaked-connection violation, deleted here), and for the whole-fetch the same stack closes the chain on `_obj_body`-style coroutine return; `_http_chunks` keeps the same shape over the `httpx.AsyncClient.stream` context, the `AsyncExitStack` closing only the per-call response context (never the cached client) on abandonment; the filesystem `_file_chunks` offloads the blocking `UPath.open`/`handle.read` to `anyio.to_thread.run_sync` and closes the handle in its own `finally`, so a blocking-IO chunk read rides the bounded worker-thread pool without stalling the event loop and the descriptor releases on early `break`.
- Packages: `fsspec`, `s3fs`, `gcsfs`, `obstore` (`store.from_url` ENTRYPOINTS [01] / `get_async` ENTRYPOINTS [01] / `GetResult.bytes()` PUBLIC_TYPES [02] / `GetResult.stream(min_chunk_size)` method returning `BytesStream` PUBLIC_TYPES [02]/[11] / `Bytes` zero-copy buffer PUBLIC_TYPES [01] / `list` ENTRYPOINTS [01] `chunk_size`/`return_arrow=True` / `sign_async` ENTRYPOINTS [09] `expires_in: timedelta` over a `SignCapableStore` / `exceptions.BaseError` PUBLIC_TYPES exceptions [01]), `universal-pathlib` (`UPath`/`resolve`/`is_relative_to`/`read_bytes`/`open`), `httpx` (`AsyncClient.get`/`AsyncClient.stream` ENTRYPOINTS [02]/[06]/`Response.aiter_bytes` ENTRYPOINTS [04]/`Response.raise_for_status` ENTRYPOINTS [01] bound through the `AsyncClient` `event_hooks={"response": [...]}` seam ENTRYPOINTS [01]/`Auth` PUBLIC_TYPES auth [01] with the `auth_flow` generator the `_BearerAuth` overrides and the per-call `auth=` argument/`Timeout` PUBLIC_TYPES [09]/`Limits` PUBLIC_TYPES [10]/`HTTPError`), `asyncssh` (`connect` ENTRYPOINTS [01] / `start_sftp_client` ENTRYPOINTS [05] / `read_known_hosts` ENTRYPOINTS [11] / `SFTPClient.open` ENTRYPOINTS [12] / `SFTPClientFile.read` ENTRYPOINTS [13] / `SSHKnownHosts` PUBLIC_TYPES [09] / `Error` PUBLIC_TYPES [01]), `anyio` (`to_thread.run_sync` thread dispatch ENTRYPOINTS [01], the bounded worker-thread offload for the blocking filesystem `read_bytes`/`open`/`read` so the cp315 event loop never blocks — branch catalog `libs/python/.api/anyio.md`), `expression` (`Error`/`Ok`/`Option`/`tagged_union`/`case`/`tag`), `arro3.core` (`RecordBatch` the obstore Arrow listing element-type carried by `survey(StoreView.LIST)`, type-checking annotation only).
- Growth: a new storage backend is one `fsspec` protocol the `UPath` resolves or one `obstore` store the `read` dispatches; a new transport is one `TransportResource` case binding a `RetryClass`; a new read size class is one `ReadModality` row the shared `_acquire` aspect already discriminates; a new object-store inventory or presign mode is one `StoreView` row the existing `survey` match already discriminates, never a fourth method; a new provider-fault taxonomy is one `*_FAULTS` tuple passed to `_acquire`, never a new `try`/`except` block; zero new surface.
- Boundary: no default root creation, root litter, product store-root derivation, bridge staging-root ownership, service API layer, companion-control transport, or AEC-collaboration client; a path parse that bypasses `UPath`, a hand-rolled retry around acquisition, a per-provider inline `try`/`except` ladder where the one `_acquire`/`async_boundary` aspect lifts the fault, a `.map_error` re-tag forcing every lifted class to the `resource` case and reading a non-active `fault.boundary` slot where the `async_boundary` subject already names the owner and `CLASSIFY` already lands the precise tag, a hand-rolled `{"Authorization": f"Bearer {token}"}` header dict where the `httpx` `Auth` flow (`_BearerAuth`) owns the credential seam, a per-call inline `response.raise_for_status()` (and the asymmetric WHOLE-arm omission of it) where the client `event_hooks` response seam enforces status once, a no-argument `GetResult.stream` property read of the bound method object where the catalog signature is `.stream(min_chunk_size)`, a bare-int `expires_in` where `obstore.sign_async` takes a `timedelta`, a blocking `UPath.read_bytes`/`handle.read` run directly on the event loop where `anyio.to_thread.run_sync` offloads it, a whole-artifact buffer where the streaming modality applies, an inline transport credential, a disabled-verification `known_hosts=None` SSH connection, a per-request `httpx.AsyncClient` construction, and a second Python Speckle/OPC-UA/MQTT leg are the deleted forms — Speckle terminates C#-side (`cs:Rasm.Persistence/Sync/collaboration`) per the cross-`libs/` `EXTERNAL_SYSTEMS_TERMINATE_CSHARP` settled boundary, and the companion reads any Speckle-sourced artifact through the canonical wire, never a `specklepy` client.

```python signature
from collections.abc import AsyncIterator, Awaitable, Callable, Generator
from contextlib import AsyncExitStack
from datetime import timedelta
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, Self, assert_never
from urllib.parse import urlsplit

from expression import Error, Ok, Option, case, tag, tagged_union
from msgspec import Struct

import anyio
import asyncssh
import httpx
import obstore
from obstore import Bytes
from obstore.store import ObjectStore
from upath import UPath

from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary
from rasm.runtime.resilience import RetryClass, guard

if TYPE_CHECKING:
    from arro3.core import RecordBatch

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
RESOURCE_FAULTS: Final[tuple[type[BaseException], ...]] = (OSError, obstore.exceptions.BaseError)
HTTP_FAULTS: Final[tuple[type[BaseException], ...]] = (httpx.HTTPError,)
SSH_FAULTS: Final[tuple[type[BaseException], ...]] = (OSError, asyncssh.Error)

STREAM_CHUNK: Final[int] = 1 << 20
LIST_CHUNK: Final[int] = 1000
SIGN_TTL: Final[timedelta] = timedelta(seconds=900)

TRANSPORT_TIMEOUT: Final[httpx.Timeout] = httpx.Timeout(connect=5.0, read=30.0, write=30.0, pool=5.0)
TRANSPORT_LIMITS: Final[httpx.Limits] = httpx.Limits(max_connections=16, max_keepalive_connections=8)


# --- [OPERATIONS] -----------------------------------------------------------------------

class _BearerAuth(httpx.Auth):
    def __init__(self, token: str) -> None:
        self._header = f"Bearer {token}"

    def auth_flow(self, request: httpx.Request) -> Generator[httpx.Request, httpx.Response, None]:
        request.headers["Authorization"] = self._header
        yield request


def _auth(bearer: Option[str]) -> httpx.Auth | None:
    return bearer.map(_BearerAuth).to_optional()


async def _acquire(owner: str, catch: tuple[type[BaseException], ...], modality: ReadModality, whole: WholeFetch, stream: StreamOpen) -> RuntimeRail[Acquired]:
    if modality is ReadModality.STREAM:
        return Ok(stream())
    return await async_boundary(owner, whole, catch=catch)


async def _obj_body(store: ObjectStore, relative: str) -> Bytes:
    return (await guard(RetryClass.OBJECT_STORE)(obstore.get_async, store, relative)).bytes()


async def _http_body(client: httpx.AsyncClient, url: str, retry_class: RetryClass, auth: httpx.Auth | None) -> bytes:
    return (await guard(retry_class)(client.get, url, auth=auth)).content


# --- [MODELS] ---------------------------------------------------------------------------

class ResourceRef(Struct, frozen=True):
    scheme: str
    root: str
    relative: str
    owner: str

    @property
    def path(self) -> UPath:
        return UPath(self.root, protocol=self.scheme) / self.relative


class ResourceRoot(Struct, frozen=True):
    scheme: str
    root: str
    owner: str

    @classmethod
    def admit(cls, uri: str, owner: str) -> Self:
        path = UPath(uri)
        return cls(scheme=path.protocol or "file", root=str(path), owner=owner)

    def child(self, relative: str) -> RuntimeRail[ResourceRef]:
        base = UPath(self.root)
        resolved = (base / relative).resolve()
        return (
            Ok(ResourceRef(self.scheme, self.root, relative, self.owner))
            if resolved.is_relative_to(base.resolve())
            else Error(BoundaryFault(resource=("traversal", relative)))
        )

    async def read(self, ref: ResourceRef, modality: ReadModality = ReadModality.WHOLE) -> RuntimeRail[Acquired]:
        store = self._store()
        whole, stream = (
            (lambda: anyio.to_thread.run_sync(ref.path.read_bytes), lambda: _file_chunks(ref))
            if store is None
            else (lambda: _obj_body(store, ref.relative), lambda: _obj_chunks(store, ref.relative))
        )
        return await _acquire(ref.owner, RESOURCE_FAULTS, modality, whole, stream)

    async def survey(self, ref: ResourceRef, view: StoreView = StoreView.LIST) -> RuntimeRail[AsyncIterator[RecordBatch] | str]:
        store = self._store()
        if store is None:
            return Error(BoundaryFault(resource=(ref.owner, f"survey.{view}.non-object-store")))
        if view is StoreView.LIST:
            return Ok(obstore.list(store, ref.relative, chunk_size=LIST_CHUNK, return_arrow=True))
        signed = await async_boundary(ref.owner, lambda: guard(RetryClass.OBJECT_STORE)(obstore.sign_async, store, "GET", ref.relative, SIGN_TTL), catch=RESOURCE_FAULTS)
        return signed.map(str)

    def _store(self) -> ObjectStore | None:
        if self.scheme not in OBJECT_STORE_SCHEMES:
            return None
        if (cached := _STORE_CACHE.get(self.root)) is None:
            cached = _STORE_CACHE[self.root] = obstore.store.from_url(self.root)
        return cached


@tagged_union(frozen=True)
class TransportResource:
    tag: Literal["http", "ssh"] = tag()
    http: tuple[str, RetryClass, Option[str]] = case()
    ssh: tuple[str, int, RetryClass, Option[str], asyncssh.SSHKnownHosts] = case()

    async def acquire(self, relative: str, modality: ReadModality = ReadModality.WHOLE) -> RuntimeRail[Acquired]:
        match self:
            case TransportResource(tag="http", http=(url, retry_class, bearer)):
                client, auth = _client_for(url), _auth(bearer)
                return await _acquire(
                    "http", HTTP_FAULTS, modality,
                    lambda: _http_body(client, url, retry_class, auth),
                    lambda: _http_chunks(client, url, auth),
                )
            case TransportResource(tag="ssh", ssh=(host, port, retry_class, passphrase, known_hosts)):
                return await _acquire(
                    "ssh", SSH_FAULTS, modality,
                    lambda: _sftp_read(host, port, relative, retry_class, passphrase, known_hosts),
                    lambda: _sftp_chunks(host, port, relative, retry_class, passphrase, known_hosts),
                )
            case _ as unreachable:
                assert_never(unreachable)


# --- [COMPOSITION] ----------------------------------------------------------------------

_STORE_CACHE: dict[str, ObjectStore] = {}
_HTTP_CACHE: dict[str, httpx.AsyncClient] = {}


async def _raise_for_status(response: httpx.Response) -> None:
    response.raise_for_status()


def _client_for(url: str) -> httpx.AsyncClient:
    endpoint = urlsplit(url)
    key = f"{endpoint.scheme}://{endpoint.netloc}"
    if (client := _HTTP_CACHE.get(key)) is None:
        client = _HTTP_CACHE[key] = httpx.AsyncClient(
            timeout=TRANSPORT_TIMEOUT, limits=TRANSPORT_LIMITS, event_hooks={"response": [_raise_for_status]}
        )
    return client


async def _file_chunks(ref: ResourceRef) -> AsyncIterator[bytes]:
    handle = await anyio.to_thread.run_sync(ref.path.open, "rb")
    try:
        while block := await anyio.to_thread.run_sync(handle.read, STREAM_CHUNK):
            yield block
    finally:
        handle.close()


async def _obj_chunks(store: ObjectStore, relative: str) -> AsyncIterator[Bytes]:
    result = await guard(RetryClass.OBJECT_STORE)(obstore.get_async, store, relative)
    async for chunk in result.stream(STREAM_CHUNK):
        yield chunk


async def _http_chunks(client: httpx.AsyncClient, url: str, auth: httpx.Auth | None) -> AsyncIterator[bytes]:
    async with AsyncExitStack() as stack:
        response = await stack.enter_async_context(client.stream("GET", url, auth=auth))
        async for chunk in response.aiter_bytes():
            yield chunk


async def _sftp_session(stack: AsyncExitStack, host: str, port: int, relative: str, retry_class: RetryClass, passphrase: Option[str], known_hosts: asyncssh.SSHKnownHosts) -> asyncssh.SFTPClientFile:
    conn = await stack.enter_async_context(asyncssh.connect(host, port=port, password=passphrase.default_value(None), known_hosts=known_hosts))
    sftp = await stack.enter_async_context(conn.start_sftp_client())
    return await stack.enter_async_context(await guard(retry_class)(sftp.open, relative, "rb"))


async def _sftp_read(host: str, port: int, relative: str, retry_class: RetryClass, passphrase: Option[str], known_hosts: asyncssh.SSHKnownHosts) -> bytes:
    async with AsyncExitStack() as stack:
        return await (await _sftp_session(stack, host, port, relative, retry_class, passphrase, known_hosts)).read()


async def _sftp_chunks(host: str, port: int, relative: str, retry_class: RetryClass, passphrase: Option[str], known_hosts: asyncssh.SSHKnownHosts) -> AsyncIterator[bytes]:
    async with AsyncExitStack() as stack:
        handle = await _sftp_session(stack, host, port, relative, retry_class, passphrase, known_hosts)
        while block := await handle.read(STREAM_CHUNK):
            yield block
```

## [03]-[RESEARCH]

- [OBSTORE_CATALOGUE]: reflection-confirmed on the cp315 core (abi3 wheel) — `obstore.store.from_url(url, *, config=...) -> ObjectStore` is the constructor, `obstore.get_async(store, path, *, options=None)` returns a `GetResult`, and `GetResult.bytes() -> Bytes` is the zero-copy payload accessor (the buffer-protocol `Bytes` materializing into the Arrow/NumPy/`msgspec.Decoder` consumer with no `bytes(...)` re-copy) backing the object-store fast-path. `_obj_body` runs the `get_async` retry and the `.bytes()` materialize as one coroutine so the WHOLE arm crosses the one `async_boundary` fence, and the `Acquired = Bytes | AsyncIterator[Bytes]` rail carries the obstore-native buffer rather than a stdlib-`bytes` re-copy. The `read` `obstore` `ReadModality.WHOLE` arm spelling is settled.
- [STREAMING_ACQUISITION]: `obstore` `GetResult.stream(min_chunk_size)` (PUBLIC_TYPES [02] / streaming law) is the method returning a `BytesStream` async iterator of `Bytes` chunks, the catalog's explicit min-chunk floor — the `STREAM` arm calls `result.stream(STREAM_CHUNK)` so the same `STREAM_CHUNK` floor governs the obstore stream and the filesystem `_file_chunks` block read (the obstore Rust core coalesces above the floor), never a no-argument property read of the bound method object; `httpx.AsyncClient.stream` (ENTRYPOINTS [5]) opens a streaming response-context and `Response.aiter_bytes` (ENTRYPOINTS [9]) yields the chunked body, both bounded by `httpx.Timeout` (PUBLIC_TYPES [9], per-phase connect/read/write/pool) and `httpx.Limits` (PUBLIC_TYPES [10], pool caps) per the catalogue streaming law (full-body reads reserved for small payloads). The `STREAM` arm returns the lazy `AsyncIterator[Bytes]` un-fenced because the `get_async` retry, the `_sftp_session` connect, and the `httpx.stream` open all run inside the generator body, so a provider raise surfaces during iteration at the `evidence/identity#IDENTITY` `ContentIdentity.of` consumer's own boundary — the same convention `survey(StoreView.LIST)` uses where the downstream Arrow consumer owns per-batch iteration-fault conversion, never a double fence. The chunk `AsyncIterator[Bytes]` folds into the existing `evidence/identity#IDENTITY` `Source = bytes | Iterable[bytes] | tuple[ContentKey, ...]` union over the `case chunks:` incremental `digest.update(chunk)` arm — the buffer-protocol `Bytes` feeds `xxh3_128.update` with no `bytes(...)` re-copy, no second hashing pass, no whole-artifact buffer. The streaming `xxh3_128` updater rides the same `python_version<'3.15'` companion band the `[XXHASH_PARITY]` content-identity seed inherits; the streaming acquisition design is settled and only the cp315 `xxhash` wheel gates the end-to-end streaming hash. Spellings catalogue-confirmed.
- [STORE_CACHE]: `obstore.store.from_url(url) -> ObjectStore` (ENTRYPOINTS [01], the URL-scheme dispatch factory) returns an immutable thread-safe Rust client; the `_STORE_CACHE: dict[str, ObjectStore]` module memo keyed by `ResourceRoot.root` constructs the handle once through `_store` and shares it across every concurrent `read`/`survey` of that root, so the URL-parse, backend-resolve, and credential-bind cost is paid once per root rather than once per access. The cache is the `obstore`-native client reuse the IMPLEMENTATION_LAW LOCAL_ADMISSION `from_url`-in-composition note intends, distinct from the `fsspec` block cache (a read-amplification layer the `fsspec` RAIL_LAW reserves to the filesystem path), so it never trips the hand-rolled-cache Reject-row. `ObjectStore` is the `S3Store | GCSStore | HTTPStore | AzureStore | LocalStore | MemoryStore` union alias re-exported from `obstore.store` (IMPLEMENTATION_LAW STORAGE_TOPOLOGY); spelling catalogue-confirmed.
- [INVENTORY_AND_PRESIGN]: `obstore.list(store, prefix=None, *, offset=None, chunk_size=50, return_arrow=False)` (ENTRYPOINTS [01]) returns a `ListStream`; `return_arrow=True` yields Arrow `RecordBatch` chunks instead of `Sequence[ObjectMeta]` (IMPLEMENTATION_LAW STORAGE_TOPOLOGY), the columnar inventory the `survey(StoreView.LIST)` arm streams for the downstream Arrow join with `LIST_CHUNK` raising the 50-entry default to a 1000-entry batch floor. `obstore.sign(store, method, paths, expires_in: timedelta)` (ENTRYPOINTS [09]) mints a presigned URL over a `SignCapableStore` (`S3Store | GCSStore | AzureStore`) and raises `NotSupportedError` (PUBLIC_TYPES exceptions [08]) on a backend that lacks signing (IMPLEMENTATION_LAW STORAGE_TOPOLOGY) — the `OBJECT_STORE_SCHEMES` gate admits only `s3`/`gs`/`az`/`abfs`, all sign-capable, so `_store` containment is the static proof the `SIGN` arm never hands a non-signing store; the async `sign_async` variant follows the `*_async` mirror law (ENTRYPOINTS scope note) used under the asyncio lane, and `expires_in` is a `timedelta`, so `SIGN_TTL` is the `timedelta(seconds=900)` expiry the arm binds, never a bare-int seconds count the catalog signature rejects. The `paths` parameter is overloaded — a single `str` path returns a single presigned URL `str`, a sequence returns a `list[str]` parallel to the input; the `SIGN` arm passes the one `ref.relative` string and so receives a single URL, the railed `signed.map(str)` pinning that single-path return shape (a `list` return would only arise from a sequence `paths` argument the arm never constructs). The `SIGN` guard reuses `RetryClass.OBJECT_STORE`: presign is a fast control-plane call sharing the object-store backoff envelope, and a distinct presign-vs-payload retry split is one future `RetryClass` row the `reliability/resilience#RESILIENCE` owner adds, never a `survey` parameter. Both arms object-store-scheme-gated through `_store`, the `SIGN` arm crossing the one `async_boundary` fence and `.map_error` re-tagging the lifted `boundary` case to the owner-addressable `resource` case; spellings catalogue-confirmed.
- [STREAM_CLEANUP]: `contextlib.AsyncExitStack.enter_async_context` registers each per-call provider context (`httpx.AsyncClient.stream` response, `asyncssh.connect` connection, `SSHClientConnection.start_sftp_client`, `SFTPClient.open` file) so the `async with AsyncExitStack()` body's `__aexit__` unwinds them in reverse order on normal completion and on the `GeneratorExit` an abandoned `aclose()` injects into the suspended `yield` — the asyncssh connection-law deterministic-close and leaked-connection Reject-row (IMPLEMENTATION_LAW TRANSPORT_TOPOLOGY) hold under early `break`, upstream fault, and partial-read identity probe alike. The `_sftp_session` factory the `_sftp_read` whole-fetch and the `_sftp_chunks` generator share registers the connect/sftp/open chain onto the caller's stack, so one chain-open shape serves both modalities and unwinds on either return. The cached `httpx.AsyncClient` is NOT stack-owned — only its per-call `client.stream("GET", ...)` response context enters the stack, so an abandoned stream closes the response without tearing down the runtime-lived pool the `_HTTP_CACHE` shares (the `httpx` Reject-row per-fetch `aclose` deleted here); the filesystem `_file_chunks` is not a provider-context chain but a thread-offloaded `UPath.open`/`handle.read` loop whose own `finally` closes the descriptor on abandonment. Cleanup design settled; spellings catalogue-confirmed.
