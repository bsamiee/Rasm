# [PY_RUNTIME_ROOTS]

Resource roots and transport resources. `ResourceRoot` admits file/object-store/scratch roots over `fsspec`/`universal-pathlib` with traversal-safe relative resolution, routing many-small-object reads through the `obstore` async fast-path while keeping the `fsspec` tree-walk semantics for the shared-pull tree; `TransportResource` is the one tagged union over HTTP/SSH generic-artifact acquisition. No durable store, daemon scheduler, app lifecycle hook, product store-root derivation, or AEC-collaboration transport (Speckle/OPC-UA/MQTT terminate C#-side and reach the companion through the canonical wire, never a second Python client leg) crosses this page.

## [01]-[INDEX]

- [01]-[RESOURCE]: resource roots, references, transport resources.

## [02]-[RESOURCE]

- Owner: `ResourceRoot` — file/object-store/scratch roots over `fsspec` + `universal-pathlib` with safe relative resolution; `ResourceRef` the scheme/root/relative/owner value object; `TransportResource` the one tagged union over `httpx` HTTP and `asyncssh` SSH/SFTP generic-artifact acquisition (a remote-fetch transport row, never a durable store and never an AEC-collaboration client).
- Cases: `TransportResource` cases `http=(url, retry_class, bearer)` · `ssh=(host, port, retry_class, passphrase, known_hosts)` — keyword-constructed and matched by `match`/`case`, each binding a `Retry` row from `reliability/resilience#RESILIENCE` and an `Option[str]` outbound credential the caller resolves through `execution/admission#SETTINGS` `SecretBoundary.resolve` before constructing the case, the `ssh` case additionally binding the verified `asyncssh.SSHKnownHosts` database the same `SecretBoundary`/settings owner loads through `asyncssh.read_known_hosts` so the connection law's host-key verification is admission-supplied, never disabled; `acquire(relative, modality)` takes the remote member to fetch and the same `relative` the filesystem `read` resolves; the read modality — whole-payload `bytes` (small) or chunk `AsyncIterator[bytes]` (large) — rides the `ReadModality` data discriminant the one entrypoint matches (`MODAL_ARITY`, one entrypoint owns all modalities, the modality a closed-enum value not a `streaming=True` knob), never a `read`/`stream` name-suffix split.
- Entry: `ResourceRoot.admit` admits one root and returns the frozen owner; `ResourceRoot.child` resolves a relative path with traversal rejection through `UPath.resolve`/`is_relative_to` containment, the failure constructed as the `resource` case of `reliability/faults#FAULT`; `ResourceRoot.read(ref, modality)` is the one polymorphic reader returning `RuntimeRail[bytes | AsyncIterator[bytes]]` — `ReadModality.WHOLE` reads the small whole payload (object-store scheme to the `obstore` async `GetResult.bytes()`, filesystem scheme to `UPath.read_bytes`) and `ReadModality.STREAM` reads the large chunk iterator (object-store `GetResult.stream` async-iterator attribute consumed as a property with no argument, the Rust core owning chunk sizing; filesystem `UPath.open('rb')` chunked read bounded by `STREAM_CHUNK`), the chunk iterator folding straight into `evidence/identity#IDENTITY` `ContentIdentity.of` over the existing `Iterable[bytes]` modality with no full-artifact buffer; `ResourceRoot.survey(ref, view)` is the second polymorphic object-store entrypoint discriminating on the `StoreView` enum — `StoreView.LIST` streams the Arrow `RecordBatch` inventory through `obstore.list(store, prefix, return_arrow=True)` (the columnar listing fast-path the `data` Arrow consumers downstream join against, never a per-`ObjectMeta` Python loop; the `survey` rail converts only the lazy `ListStream` construction at the one `resource` boundary, and the downstream `data` Arrow consumer owns per-`RecordBatch` iteration-fault conversion as it pulls the stream) and `StoreView.SIGN` mints the presigned URL through `obstore.sign(store, method, paths, expires_in)` so a downstream companion fetch is a bare `httpx` GET against a time-boxed URL with no credential leg, both arms object-store-scheme-gated and both crossing the one `resource` boundary; `TransportResource.acquire(relative, modality)` is the matching one entrypoint — `ReadModality.WHOLE` returns the small body and `ReadModality.STREAM` streams large bodies through `httpx.AsyncClient.stream`/`Response.aiter_bytes` under explicit `httpx.Timeout`/`httpx.Limits`, the bound retry row from `reliability/resilience#RESILIENCE` `guard` wrapping each, the raised provider fault converting at the one `resource`-case boundary; the outbound credential reads from the `execution/admission#SETTINGS` `SecretBoundary.resolve` and rides the case as an `Option[str]` — the `ssh` passphrase threads into `asyncssh.connect(..., password=...)` and the `http` bearer into the `Authorization` header through `_bearer_headers`, a `Nothing` credential leaving the connection/header bare and never an inline literal; the `ssh` arm always verifies the host key against the known-hosts database the `execution/admission#SETTINGS` owner threads through `asyncssh.read_known_hosts` (ENTRYPOINTS [11]) into `asyncssh.connect(..., known_hosts=...)`, never the disabled-verification `known_hosts=None` the `asyncssh` RAIL_LAW Reject-row forbids.
- Cache: `obstore.store.from_url` is a Rust handle whose construction parses the URL, resolves the backend, and binds the credential provider, so a per-access reconstruction re-pays that cost on every `read`/`survey` of the same root; the `_STORE_CACHE` module memo keyed by the root URL constructs once through `_store_for` and returns the cached `ObjectStore` on every subsequent access, the handle being a thread-safe immutable client the concurrent-small-object fan-out shares — never a fresh store per `read`, never a store rebuilt per chunk, never a hand-rolled fsspec cache the `fsspec` Reject-row forbids. The symmetric `_HTTP_CACHE` module memo keyed by the `http` URL's scheme-host-port endpoint group constructs one long-lived `httpx.AsyncClient(timeout=TRANSPORT_TIMEOUT, limits=TRANSPORT_LIMITS)` through `_client_for` and shares its pool and `httpx.Limits` across every WHOLE/STREAM acquisition of that endpoint — the `httpx` client-law one-long-lived-client-per-endpoint-group reuse, the per-request `AsyncClient(...)` construction the `httpx` Reject-row forbids deleted here; the pooled client lives for the runtime, so `_http_chunks` reuses the cached client and only its per-call `stream` response-context rides the per-acquisition `AsyncExitStack` (never the client itself), never an `aclose` per fetch that would tear down the shared pool.
- Auto: `OBJECT_STORE_SCHEMES` routes `s3`/`gs`/`az`/`abfs` to the `obstore` stateless API (abi3 wheel, cp315-ready) for the concurrent-small-object reads that dominate companion artifact pulls, and routes every other scheme through `UPath` filesystem semantics; one polymorphic `read`/`survey`/`acquire` entrypoint per owner owns both schemes and both modalities under the `ReadModality`/`StoreView` discriminants, the whole-payload `bytes` arm reserved for small payloads and the `AsyncIterator[bytes]` arm for the large GLB/IFC/scan artifacts the `httpx`/`obstore` streaming surfaces exist to bound — a full-body `client.get` of a large payload is the `httpx` Reject-row violation (catalogue IMPLEMENTATION_LAW streaming law) and a `GetResult.bytes()` of a large object is the `obstore` Reject-row violation (catalogue RAIL_LAW), both deleted here; traversal containment compares the resolved child against the resolved root, never a string prefix that a sibling root spoofs; the rail never returns a bare `Ok` over a throwing provider call — every acquisition crosses the one `boundary` conversion.
- Cleanup: a streamed acquisition the consumer abandons (an early `break`, a fault upstream, a partial-read identity probe) must release the provider session deterministically, so `_sftp_chunks` opens the `asyncssh.connect`/`start_sftp_client`/`SFTPClient.open` chain through one `contextlib.AsyncExitStack` whose `__aexit__` runs in the generator's `finally` — when the consumer stops iterating, the generator `aclose()` raises `GeneratorExit` into the suspended `yield`, the `finally` unwinds the stack, and the SFTP file, the SFTP client, and the SSH connection all close in reverse order with no leaked socket across the event loop (the `asyncssh` Reject-row leaked-connection violation, deleted here); `_http_chunks` keeps the same shape over the `httpx.AsyncClient.stream` context, the `AsyncExitStack` closing the response context and the client on abandonment.
- Packages: `fsspec`, `s3fs`, `gcsfs`, `obstore` (`store.from_url` ENTRYPOINTS [01] / `get_async` ENTRYPOINTS [01] / `GetResult.bytes()` PUBLIC_TYPES [02] / `GetResult.stream` async-iterator attribute PUBLIC_TYPES [02] / `list` ENTRYPOINTS [01] `return_arrow=True` / `sign_async` ENTRYPOINTS [09] / `open_reader` streaming-IO ENTRYPOINTS [01] / `exceptions.BaseError` PUBLIC_TYPES exceptions [01]), `universal-pathlib` (`UPath`/`resolve`/`is_relative_to`/`read_bytes`/`open`), `httpx` (`AsyncClient.get`/`AsyncClient.stream` ENTRYPOINTS [5]/`Response.aiter_bytes` ENTRYPOINTS [9]/`Timeout` PUBLIC_TYPES [9]/`Limits` PUBLIC_TYPES [10]/`HTTPError`), `asyncssh` (`connect` ENTRYPOINTS [01] / `start_sftp_client` ENTRYPOINTS [05] / `read_known_hosts` ENTRYPOINTS [11] / `SFTPClient.open` ENTRYPOINTS [12] / `SFTPClientFile.read` ENTRYPOINTS [13] / `SSHKnownHosts` PUBLIC_TYPES [09] / `Error` PUBLIC_TYPES [01]), `arro3.core` (`RecordBatch` the obstore Arrow listing element-type carried by `survey(StoreView.LIST)`, type-checking annotation only).
- Growth: a new storage backend is one `fsspec` protocol the `UPath` resolves or one `obstore` store the `read` dispatches; a new transport is one `TransportResource` case binding a `Retry` row; a new read size class is one `ReadModality` row the existing `read`/`acquire` match already discriminates; a new object-store inventory or presign mode is one `StoreView` row the existing `survey` match already discriminates, never a fourth method; zero new surface.
- Boundary: no default root creation, root litter, product store-root derivation, bridge staging-root ownership, service API layer, companion-control transport, or AEC-collaboration client; a path parse that bypasses `UPath`, a hand-rolled retry around acquisition, a whole-artifact buffer where the streaming modality applies, an inline transport credential, a disabled-verification `known_hosts=None` SSH connection, a per-request `httpx.AsyncClient` construction, and a second Python Speckle/OPC-UA/MQTT leg are the deleted forms — Speckle terminates C#-side (`cs:Rasm.Persistence/Sync/collaboration`) per the cross-`libs/` `EXTERNAL_SYSTEMS_TERMINATE_CSHARP` settled boundary, and the companion reads any Speckle-sourced artifact through the canonical wire, never a `specklepy` client.

```python signature
from collections.abc import AsyncIterator
from contextlib import AsyncExitStack
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, Self, assert_never
from urllib.parse import urlsplit

import asyncssh
import httpx
import obstore
from expression import Error, Ok, Option, case, tag, tagged_union
from msgspec import Struct
from obstore.store import ObjectStore
from upath import UPath

from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.resilience import RetryClass, guard

if TYPE_CHECKING:
    from arro3.core import RecordBatch

OBJECT_STORE_SCHEMES: Final[frozenset[str]] = frozenset({"s3", "gs", "az", "abfs"})


def _bearer_headers(bearer: Option[str]) -> dict[str, str]:
    return bearer.map(lambda token: {"Authorization": f"Bearer {token}"}).default_value({})


class ReadModality(StrEnum):
    WHOLE = "whole"
    STREAM = "stream"


class StoreView(StrEnum):
    LIST = "list"
    SIGN = "sign"


STREAM_CHUNK: Final[int] = 1 << 20
LIST_CHUNK: Final[int] = 1000
SIGN_TTL: Final[int] = 900

TRANSPORT_TIMEOUT: Final[httpx.Timeout] = httpx.Timeout(connect=5.0, read=30.0, write=30.0, pool=5.0)
TRANSPORT_LIMITS: Final[httpx.Limits] = httpx.Limits(max_connections=16, max_keepalive_connections=8)


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

    async def read(self, ref: ResourceRef, modality: ReadModality = ReadModality.WHOLE) -> RuntimeRail[bytes | AsyncIterator[bytes]]:
        try:
            store = self._store()
            result = await guard(RetryClass.OBJECT_STORE)(obstore.get_async, store, ref.relative) if store is not None else None
            match modality:
                case ReadModality.WHOLE:
                    return Ok(bytes(result.bytes()) if result is not None else ref.path.read_bytes())
                case ReadModality.STREAM:
                    return Ok(self._chunks(result.stream) if result is not None else self._file_chunks(ref))
                case _ as unreachable:
                    assert_never(unreachable)
        except (OSError, obstore.exceptions.BaseError) as cause:
            return Error(BoundaryFault(resource=(ref.owner, type(cause).__name__)))

    async def survey(self, ref: ResourceRef, view: StoreView = StoreView.LIST) -> RuntimeRail[AsyncIterator[RecordBatch] | str]:
        store = self._store()
        if store is None:
            return Error(BoundaryFault(resource=(ref.owner, f"survey.{view}.non-object-store")))
        try:
            match view:
                case StoreView.LIST:
                    return Ok(obstore.list(store, ref.relative, chunk_size=LIST_CHUNK, return_arrow=True))
                case StoreView.SIGN:
                    signed = await guard(RetryClass.OBJECT_STORE)(obstore.sign_async, store, "GET", ref.relative, SIGN_TTL)
                    return Ok(str(signed))
                case _ as unreachable:
                    assert_never(unreachable)
        except (OSError, obstore.exceptions.BaseError) as cause:
            return Error(BoundaryFault(resource=(ref.owner, type(cause).__name__)))

    def _store(self) -> ObjectStore | None:
        if self.scheme not in OBJECT_STORE_SCHEMES:
            return None
        cached = _STORE_CACHE.get(self.root)
        if cached is None:
            cached = obstore.store.from_url(self.root)
            _STORE_CACHE[self.root] = cached
        return cached

    @staticmethod
    async def _chunks(source: AsyncIterator[object]) -> AsyncIterator[bytes]:
        async for chunk in source:
            yield bytes(chunk)

    @staticmethod
    async def _file_chunks(ref: ResourceRef) -> AsyncIterator[bytes]:
        with ref.path.open("rb") as handle:
            while block := handle.read(STREAM_CHUNK):
                yield block


_STORE_CACHE: dict[str, ObjectStore] = {}
_HTTP_CACHE: dict[str, httpx.AsyncClient] = {}


def _client_for(url: str) -> httpx.AsyncClient:
    endpoint = urlsplit(url)
    key = f"{endpoint.scheme}://{endpoint.netloc}"
    client = _HTTP_CACHE.get(key)
    if client is None:
        client = httpx.AsyncClient(timeout=TRANSPORT_TIMEOUT, limits=TRANSPORT_LIMITS)
        _HTTP_CACHE[key] = client
    return client


@tagged_union(frozen=True)
class TransportResource:
    tag: Literal["http", "ssh"] = tag()
    http: tuple[str, RetryClass, Option[str]] = case()
    ssh: tuple[str, int, RetryClass, Option[str], asyncssh.SSHKnownHosts] = case()

    async def acquire(self, relative: str, modality: ReadModality = ReadModality.WHOLE) -> RuntimeRail[bytes | AsyncIterator[bytes]]:
        match self, modality:
            case TransportResource(tag="http", http=(url, retry_class, bearer)), ReadModality.WHOLE:
                try:
                    response = await guard(retry_class)(_client_for(url).get, url, headers=_bearer_headers(bearer))
                    return Ok(response.content)
                except httpx.HTTPError as cause:
                    return Error(BoundaryFault(resource=("http", type(cause).__name__)))
            case TransportResource(tag="http", http=(url, retry_class, bearer)), ReadModality.STREAM:
                return Ok(self._http_chunks(url, retry_class, bearer))
            case TransportResource(tag="ssh", ssh=(host, port, retry_class, passphrase, known_hosts)), ReadModality.WHOLE:
                try:
                    async with asyncssh.connect(host, port=port, password=passphrase.default_value(None), known_hosts=known_hosts) as conn, conn.start_sftp_client() as sftp:
                        async with await guard(retry_class)(sftp.open, relative, "rb") as handle:
                            return Ok(await handle.read())
                except (OSError, asyncssh.Error) as cause:
                    return Error(BoundaryFault(resource=("ssh", type(cause).__name__)))
            case TransportResource(tag="ssh", ssh=(host, port, retry_class, passphrase, known_hosts)), ReadModality.STREAM:
                return Ok(self._sftp_chunks(host, port, relative, retry_class, passphrase, known_hosts))
            case _ as unreachable:
                assert_never(unreachable)

    @staticmethod
    async def _http_chunks(url: str, retry_class: RetryClass, bearer: Option[str]) -> AsyncIterator[bytes]:
        async with AsyncExitStack() as stack:
            response = await stack.enter_async_context(_client_for(url).stream("GET", url, headers=_bearer_headers(bearer)))
            response.raise_for_status()
            async for chunk in response.aiter_bytes():
                yield chunk

    @staticmethod
    async def _sftp_chunks(host: str, port: int, relative: str, retry_class: RetryClass, passphrase: Option[str], known_hosts: asyncssh.SSHKnownHosts) -> AsyncIterator[bytes]:
        async with AsyncExitStack() as stack:
            conn = await stack.enter_async_context(asyncssh.connect(host, port=port, password=passphrase.default_value(None), known_hosts=known_hosts))
            sftp = await stack.enter_async_context(conn.start_sftp_client())
            handle = await stack.enter_async_context(await guard(retry_class)(sftp.open, relative, "rb"))
            while block := await handle.read(STREAM_CHUNK):
                yield block
```

## [03]-[RESEARCH]

- [OBSTORE_CATALOGUE]: reflection-confirmed on the cp315 core (abi3 wheel) — `obstore.store.from_url(url, *, config=...) -> ObjectStore` is the constructor, `obstore.get_async(store, path, *, options=None)` returns a `GetResult`, and `GetResult.bytes() -> Bytes` (wrappable by `bytes(...)`) is the payload accessor backing the object-store fast-path. The `read` `obstore` `ReadModality.WHOLE` arm spelling is settled.
- [STREAMING_ACQUISITION]: `obstore` `GetResult.stream` (PUBLIC_TYPES [02], IMPLEMENTATION_LAW: `.stream` is an attribute returning an async iterator of `Bytes` chunks, consumed as a property with no argument — the catalog carries no `min_chunk_size` parameter on `.stream`, so the `STREAM` arm reads `result.stream` directly) and `open_reader(store, path, *, buffer_size=..., size=None)` (streaming-IO ENTRYPOINTS [01], the explicit chunk-floor surface) are the large-object streaming surface beside the small-read `GetResult.bytes()`; the page constant `STREAM_CHUNK` bounds the filesystem `_file_chunks` block read, not the obstore `.stream` (whose chunking the Rust core owns); `httpx.AsyncClient.stream` (ENTRYPOINTS [5]) opens a streaming response-context and `Response.aiter_bytes` (ENTRYPOINTS [9]) yields the chunked body, both bounded by `httpx.Timeout` (PUBLIC_TYPES [9], per-phase connect/read/write/pool) and `httpx.Limits` (PUBLIC_TYPES [10], pool caps) per the catalogue streaming law (full-body reads reserved for small payloads). The chunk `AsyncIterator[bytes]` folds into the existing `evidence/identity#IDENTITY` `Source = bytes | Iterable[bytes] | tuple[ContentKey, ...]` union over the `case chunks:` incremental `digest.update(chunk)` arm — no second hashing pass, no whole-artifact buffer. The streaming `xxh3_128` updater rides the same `python_version<'3.15'` companion band the `[XXHASH_PARITY]` content-identity seed inherits; the streaming acquisition design is settled and only the cp315 `xxhash` wheel gates the end-to-end streaming hash. Spellings catalogue-confirmed.
- [STORE_CACHE]: `obstore.store.from_url(url) -> ObjectStore` (ENTRYPOINTS [01], the URL-scheme dispatch factory) returns an immutable thread-safe Rust client; the `_STORE_CACHE: dict[str, ObjectStore]` module memo keyed by `ResourceRoot.root` constructs the handle once through `_store` and shares it across every concurrent `read`/`survey` of that root, so the URL-parse, backend-resolve, and credential-bind cost is paid once per root rather than once per access. The cache is the `obstore`-native client reuse the IMPLEMENTATION_LAW LOCAL_ADMISSION `from_url`-in-composition note intends, distinct from the `fsspec` block cache (a read-amplification layer the `fsspec` RAIL_LAW reserves to the filesystem path), so it never trips the hand-rolled-cache Reject-row. `ObjectStore` is the `S3Store | GCSStore | HTTPStore | AzureStore | LocalStore | MemoryStore` union alias re-exported from `obstore.store` (IMPLEMENTATION_LAW STORAGE_TOPOLOGY); spelling catalogue-confirmed.
- [INVENTORY_AND_PRESIGN]: `obstore.list(store, prefix=None, *, offset=None, chunk_size=50, return_arrow=False)` (ENTRYPOINTS [01]) returns a `ListStream`; `return_arrow=True` yields Arrow `RecordBatch` chunks instead of `Sequence[ObjectMeta]` (IMPLEMENTATION_LAW STORAGE_TOPOLOGY), the columnar inventory the `survey(StoreView.LIST)` arm streams for the downstream Arrow join with `LIST_CHUNK` raising the 50-entry default to a 1000-entry batch floor. `obstore.sign(store, method, paths, expires_in)` (ENTRYPOINTS [09]) mints a presigned URL and raises `NotSupportedError` (PUBLIC_TYPES exceptions [08]) on a backend that lacks signing (IMPLEMENTATION_LAW STORAGE_TOPOLOGY); the async `sign_async` variant follows the `*_async` mirror law (ENTRYPOINTS scope note) used under the asyncio lane, `SIGN_TTL` the 900s expiry the `survey(StoreView.SIGN)` arm binds. The `paths` parameter is overloaded — a single `str` path returns a single presigned URL `str`, a sequence returns a `Sequence[str]` parallel to the input; the `SIGN` arm passes the one `ref.relative` string and so receives a single URL, the `str(signed)` coercion pinning that single-path return shape (a `Sequence` return would only arise from a sequence `paths` argument the arm never constructs). The `SIGN` guard reuses `RetryClass.OBJECT_STORE`: presign is a fast control-plane call sharing the object-store backoff envelope, and a distinct presign-vs-payload retry split is one future `RetryClass` row the `reliability/resilience#RESILIENCE` owner adds, never a `survey` parameter. Both arms object-store-scheme-gated through `_store`, both lifting the provider raise at the one `resource` boundary; spellings catalogue-confirmed.
- [STREAM_CLEANUP]: `contextlib.AsyncExitStack.enter_async_context` registers each provider context (`httpx.AsyncClient`, `httpx.AsyncClient.stream` response, `asyncssh.connect` connection, `SSHClientConnection.start_sftp_client`, `SFTPClient.open` file) so the `async with AsyncExitStack()` body's `__aexit__` unwinds them in reverse order on normal completion and on the `GeneratorExit` an abandoned `aclose()` injects into the suspended `yield` — the asyncssh connection-law deterministic-close and leaked-connection Reject-row (IMPLEMENTATION_LAW TRANSPORT_TOPOLOGY) hold under early `break`, upstream fault, and partial-read identity probe alike. The prior `_http_chunks(client, ...)` signature took a pre-constructed client an abandoned stream could leak; the stack-owned construction closes it on the same unwind. Cleanup design settled; spellings catalogue-confirmed.
