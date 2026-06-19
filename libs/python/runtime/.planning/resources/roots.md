# [PY_RUNTIME_ROOTS]

Resource roots and transport resources. `ResourceRoot` admits file/object-store/scratch roots over `fsspec`/`universal-pathlib` with traversal-safe relative resolution, routing many-small-object reads through the `obstore` async fast-path while keeping the `fsspec` tree-walk semantics for the shared-pull tree; `TransportResource` is the one tagged union over HTTP/SSH generic-artifact acquisition. No durable store, daemon scheduler, app lifecycle hook, product store-root derivation, or AEC-collaboration transport (Speckle/OPC-UA/MQTT terminate C#-side and reach the companion through the canonical wire, never a second Python client leg) crosses this page.

## [1]-[INDEX]

One cluster: `[2]-[RESOURCE]` — resource roots, references, transport resources.

## [2]-[RESOURCE]

- Owner: `ResourceRoot` — file/object-store/scratch roots over `fsspec` + `universal-pathlib` with safe relative resolution; `ResourceRef` the scheme/root/relative/owner value object; `TransportResource` the one tagged union over `httpx` HTTP and `asyncssh` SSH/SFTP generic-artifact acquisition (a remote-fetch transport row, never a durable store and never an AEC-collaboration client).
- Cases: `TransportResource` cases `http=(url, retry_class)` · `ssh=(host, port, retry_class)` — keyword-constructed and matched by `match`/`case`, each binding a `Retry` row from `reliability/resilience#RESILIENCE`; `acquire(relative, modality)` takes the remote member to fetch and the same `relative` the filesystem `read` resolves; the read modality — whole-payload `bytes` (small) or chunk `AsyncIterator[bytes]` (large) — rides the `ReadModality` data discriminant the one entrypoint matches (`MODAL_ARITY`, one entrypoint owns all modalities, the modality a closed-enum value not a `streaming=True` knob), never a `read`/`stream` name-suffix split.
- Entry: `ResourceRoot.admit` admits one root and returns the frozen owner; `ResourceRoot.child` resolves a relative path with traversal rejection through `UPath.resolve`/`is_relative_to` containment, the failure constructed as the `resource` case of `reliability/faults#FAULT`; `ResourceRoot.read(ref, modality)` is the one polymorphic reader returning `RuntimeRail[bytes | AsyncIterator[bytes]]` — `ReadModality.WHOLE` reads the small whole payload (object-store scheme to the `obstore` async `GetResult.bytes()`, filesystem scheme to `UPath.read_bytes`) and `ReadModality.STREAM` reads the large chunk iterator (object-store `GetResult.stream(STREAM_CHUNK)` `BytesStream`, filesystem `UPath.open('rb')` chunked read), the chunk iterator folding straight into `identity/content-identity#IDENTITY` `ContentIdentity.of` over the existing `Iterable[bytes]` modality with no full-artifact buffer; `TransportResource.acquire(relative, modality)` is the matching one entrypoint — `ReadModality.WHOLE` returns the small body and `ReadModality.STREAM` streams large bodies through `httpx.AsyncClient.stream`/`Response.aiter_bytes` under explicit `httpx.Timeout`/`httpx.Limits`, the bound retry row from `reliability/resilience#RESILIENCE` `guard` wrapping each, the raised provider fault converting at the one `resource`-case boundary; the outbound credential (`ssh` passphrase, `http` bearer) reads from the `context/admission#SETTINGS` `SecretBoundary.resolve`, never an inline literal.
- Auto: `OBJECT_STORE_SCHEMES` routes `s3`/`gs`/`az`/`abfs` to the `obstore` stateless API (abi3 wheel, cp315-ready) for the concurrent-small-object reads that dominate companion artifact pulls, and routes every other scheme through `UPath` filesystem semantics; one polymorphic `read`/`acquire` entrypoint per owner owns both schemes and both modalities under the `ReadModality` discriminant, the whole-payload `bytes` arm reserved for small payloads and the `AsyncIterator[bytes]` arm for the large GLB/IFC/scan artifacts the `httpx`/`obstore` streaming surfaces exist to bound — a full-body `client.get` of a large payload is the `httpx` Reject-row violation (catalogue IMPLEMENTATION_LAW streaming law) and a `GetResult.bytes()` of a large object is the `obstore` Reject-row violation (catalogue RAIL_LAW), both deleted here; traversal containment compares the resolved child against the resolved root, never a string prefix that a sibling root spoofs; the rail never returns a bare `Ok` over a throwing provider call — every acquisition crosses the one `boundary` conversion.
- Packages: `fsspec`, `s3fs`, `gcsfs`, `obstore` (`store.from_url`/`get_async`/`GetResult.bytes`/`GetResult.stream`/`open_reader`), `universal-pathlib` (`UPath`/`resolve`/`is_relative_to`/`read_bytes`/`open`), `httpx` (`AsyncClient.get`/`AsyncClient.stream` ENTRYPOINTS [5]/`Response.aiter_bytes` ENTRYPOINTS [9]/`Timeout` PUBLIC_TYPES [9]/`Limits` PUBLIC_TYPES [10]/`HTTPError`), `asyncssh` (`connect`/`start_sftp_client`/`SFTPClient.open`/`SFTPClientFile.read`/`Error`).
- Growth: a new storage backend is one `fsspec` protocol the `UPath` resolves or one `obstore` store the `read` dispatches; a new transport is one `TransportResource` case binding a `Retry` row; a new read size class is one `ReadModality` row the existing `read`/`acquire` match already discriminates, never a fourth method; zero new surface.
- Boundary: no default root creation, root litter, product store-root derivation, bridge staging-root ownership, service API layer, companion-control transport, or AEC-collaboration client; a path parse that bypasses `UPath`, a hand-rolled retry around acquisition, a whole-artifact buffer where the streaming modality applies, an inline transport credential, and a second Python Speckle/OPC-UA/MQTT leg are the deleted forms — Speckle terminates C#-side (`cs:Rasm.Persistence/sync/collaboration`) per the cross-`libs/` `EXTERNAL_SYSTEMS_TERMINATE_CSHARP` settled boundary, and the companion reads any Speckle-sourced artifact through the canonical wire, never a `specklepy` client.

```python signature
from collections.abc import AsyncIterator
from enum import StrEnum
from typing import Final, Literal, Self, assert_never

import asyncssh
import httpx
import obstore
from expression import Error, Ok, case, tag, tagged_union
from msgspec import Struct
from upath import UPath

from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.resilience import RetryClass, guard

OBJECT_STORE_SCHEMES: Final[frozenset[str]] = frozenset({"s3", "gs", "az", "abfs"})


class ReadModality(StrEnum):
    WHOLE = "whole"
    STREAM = "stream"

STREAM_CHUNK: Final[int] = 1 << 20

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
            store = obstore.store.from_url(self.root) if self.scheme in OBJECT_STORE_SCHEMES else None
            result = (
                await guard(RetryClass.OBJECT_STORE)(obstore.get_async, store, ref.relative)
                if store is not None
                else None
            )
            match modality:
                case ReadModality.WHOLE:
                    return Ok(bytes(result.bytes()) if result is not None else ref.path.read_bytes())
                case ReadModality.STREAM:
                    return Ok(self._chunks(result.stream(STREAM_CHUNK)) if result is not None else self._file_chunks(ref))
                case _ as unreachable:
                    assert_never(unreachable)
        except OSError as cause:
            return Error(BoundaryFault(resource=(ref.owner, type(cause).__name__)))

    @staticmethod
    async def _chunks(source: AsyncIterator[object]) -> AsyncIterator[bytes]:
        async for chunk in source:
            yield bytes(chunk)

    @staticmethod
    async def _file_chunks(ref: ResourceRef) -> AsyncIterator[bytes]:
        with ref.path.open("rb") as handle:
            while block := handle.read(STREAM_CHUNK):
                yield block


@tagged_union(frozen=True)
class TransportResource:
    tag: Literal["http", "ssh"] = tag()
    http: tuple[str, RetryClass] = case()
    ssh: tuple[str, int, RetryClass] = case()

    async def acquire(self, relative: str, modality: ReadModality = ReadModality.WHOLE) -> RuntimeRail[bytes | AsyncIterator[bytes]]:
        match self, modality:
            case TransportResource(tag="http", http=(url, retry_class)), ReadModality.WHOLE:
                try:
                    async with httpx.AsyncClient(timeout=TRANSPORT_TIMEOUT, limits=TRANSPORT_LIMITS) as client:
                        response = await guard(retry_class)(client.get, url)
                        return Ok(response.content)
                except httpx.HTTPError as cause:
                    return Error(BoundaryFault(resource=("http", type(cause).__name__)))
            case TransportResource(tag="http", http=(url, retry_class)), ReadModality.STREAM:
                client = httpx.AsyncClient(timeout=TRANSPORT_TIMEOUT, limits=TRANSPORT_LIMITS)
                return Ok(self._http_chunks(client, url, retry_class))
            case TransportResource(tag="ssh", ssh=(host, port, retry_class)), ReadModality.WHOLE:
                try:
                    async with asyncssh.connect(host, port=port) as conn, conn.start_sftp_client() as sftp:
                        async with await guard(retry_class)(sftp.open, relative, "rb") as handle:
                            return Ok(await handle.read())
                except (OSError, asyncssh.Error) as cause:
                    return Error(BoundaryFault(resource=("ssh", type(cause).__name__)))
            case TransportResource(tag="ssh", ssh=(host, port, retry_class)), ReadModality.STREAM:
                return Ok(self._sftp_chunks(host, port, relative, retry_class))
            case _ as unreachable:
                assert_never(unreachable)

    @staticmethod
    async def _http_chunks(client: httpx.AsyncClient, url: str, retry_class: RetryClass) -> AsyncIterator[bytes]:
        async with client:
            async with client.stream("GET", url) as response:
                response.raise_for_status()
                async for chunk in response.aiter_bytes():
                    yield chunk

    @staticmethod
    async def _sftp_chunks(host: str, port: int, relative: str, retry_class: RetryClass) -> AsyncIterator[bytes]:
        async with asyncssh.connect(host, port=port) as conn, conn.start_sftp_client() as sftp:
            async with await guard(retry_class)(sftp.open, relative, "rb") as handle:
                while block := await handle.read(STREAM_CHUNK):
                    yield block
```

## [3]-[RESEARCH]

- [OBSTORE_CATALOGUE]: reflection-confirmed on the cp315 core (abi3 wheel) — `obstore.store.from_url(url, *, config=...) -> ObjectStore` is the constructor, `obstore.get_async(store, path, *, options=None)` returns a `GetResult`, and `GetResult.bytes() -> Bytes` (wrappable by `bytes(...)`) is the payload accessor backing the object-store fast-path. The `read` `obstore` `ReadModality.WHOLE` arm spelling is settled.
- [STREAMING_ACQUISITION]: `obstore` `GetResult.stream(min_chunk_size=10MB) -> BytesStream` (PUBLIC_TYPES [2], IMPLEMENTATION_LAW: `.stream` is a method returning a `BytesStream` async iterator of `Bytes` chunks, the `min_chunk_size` bounding each non-terminal chunk — `STREAM_CHUNK` is passed explicitly so the chunk floor is the page constant, not the 10MB default; `GetResult.__aiter__` aliases `.stream` at the default size) and `open_reader` (streaming IO [1]) are the large-object streaming surface beside the small-read `GetResult.bytes()`; `httpx.AsyncClient.stream` (ENTRYPOINTS [5]) opens a streaming response-context and `Response.aiter_bytes` (ENTRYPOINTS [9]) yields the chunked body, both bounded by `httpx.Timeout` (PUBLIC_TYPES [9], per-phase connect/read/write/pool) and `httpx.Limits` (PUBLIC_TYPES [10], pool caps) per the catalogue streaming law (full-body reads reserved for small payloads). The chunk `AsyncIterator[bytes]` folds into the existing `identity/content-identity#IDENTITY` `Source = bytes | Iterable[bytes] | tuple[ContentKey, ...]` union over the `case chunks:` incremental `digest.update(chunk)` arm — no second hashing pass, no whole-artifact buffer. The streaming `xxh3_128` updater rides the same `python_version<'3.15'` companion band the `[XXHASH_PARITY]` content-identity seed inherits; the streaming acquisition design is settled and only the cp315 `xxhash` wheel gates the end-to-end streaming hash. Spellings catalogue-confirmed.
