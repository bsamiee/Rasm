# [PY_RUNTIME_ROOTS]

Resource roots and transport resources. `ResourceRoot` admits file/object-store/scratch roots over `fsspec`/`universal-pathlib` with traversal-safe relative resolution, routing many-small-object reads through the `obstore` async fast-path while keeping the `fsspec` tree-walk semantics for the shared-pull tree; `TransportResource` is the one tagged union over HTTP/SSH/Speckle remote-AEC acquisition. No durable store, daemon scheduler, app lifecycle hook, or product store-root derivation crosses this page.

## [1]-[INDEX]

One cluster: `[2]-[RESOURCE]` — resource roots, references, transport resources.

## [2]-[RESOURCE]

- Owner: `ResourceRoot` — file/object-store/scratch roots over `fsspec` + `universal-pathlib` with safe relative resolution; `ResourceRef` the scheme/root/relative/owner value object; `TransportResource` the one tagged union over `httpx` HTTP, `asyncssh` SSH/SFTP, and `specklepy` Speckle stream send/receive (a remote-AEC transport row, never a durable store).
- Cases: `TransportResource` cases `http=(url, retry_class)` · `ssh=(host, port, retry_class)` · `speckle=(stream_id, token)` — keyword-constructed and matched by `match`/`case`, each binding a `Retry` row from `reliability/resilience#RESILIENCE`; `acquire(relative)` takes the remote member to fetch and the same `relative` the filesystem `reader` resolves.
- Entry: `ResourceRoot.admit` admits one root and returns the frozen owner; `ResourceRoot.child` resolves a relative path with traversal rejection through `UPath.resolve`/`is_relative_to` containment, the failure constructed as the `resource` case of `reliability/faults#FAULT`; `ResourceRoot.reader` returns a `RuntimeRail[bytes]`, dispatching an object-store scheme to the `obstore` async API and a filesystem scheme to `UPath.read_bytes`, the raised `OSError` converting once to `Error(BoundaryFault(resource=...))`; `TransportResource.acquire` returns a `RuntimeRail[bytes]` over the bound retry row from `reliability/resilience#RESILIENCE` `guard`, the raised provider fault converting at the same `resource`-case boundary.
- Auto: `OBJECT_STORE_SCHEMES` routes `s3`/`gs`/`az`/`abfs` to the `obstore` stateless API (abi3 wheel, cp315-ready) for the concurrent-small-object reads that dominate companion artifact pulls, and routes every other scheme through `UPath` filesystem semantics; one polymorphic `reader` owns both paths; traversal containment compares the resolved child against the resolved root, never a string prefix that a sibling root spoofs; the rail never returns a bare `Ok` over a throwing provider call — every acquisition crosses the one `boundary` conversion.
- Packages: `fsspec`, `s3fs`, `gcsfs`, `obstore` (`store.from_url`/`get_async`/`GetResult.bytes`), `universal-pathlib` (`UPath`/`resolve`/`is_relative_to`/`read_bytes`), `httpx` (`AsyncClient.get`/`HTTPError`), `asyncssh` (`connect`/`start_sftp_client`/`SFTPClient.open`/`SFTPClientFile.read`/`Error`), `specklepy` (`api.client.SpeckleClient`/`transports.server.ServerTransport`/`api.operations.receive`/`api.operations.serialize`).
- Growth: a new storage backend is one `fsspec` protocol the `UPath` resolves or one `obstore` store the `reader` dispatches; a new transport is one `TransportResource` case binding a `Retry` row; zero new surface.
- Boundary: no default root creation, root litter, product store-root derivation, bridge staging-root ownership, service API layer, or companion-control transport; a path parse that bypasses `UPath`, a hand-rolled retry around acquisition, and a Speckle durable-store treatment are the deleted forms; Speckle is transport + bundle shapes only.

```python signature
from typing import Final, Literal, Self, assert_never

import asyncssh
import httpx
import obstore
from expression import Error, Ok, case, tag, tagged_union
from msgspec import Struct
from specklepy.api.client import SpeckleClient
from specklepy.api import operations
from specklepy.transports.server import ServerTransport
from upath import UPath

from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary
from rasm.runtime.resilience import RetryClass, guard

OBJECT_STORE_SCHEMES: Final[frozenset[str]] = frozenset({"s3", "gs", "az", "abfs"})


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

    async def reader(self, ref: ResourceRef) -> RuntimeRail[bytes]:
        try:
            if self.scheme in OBJECT_STORE_SCHEMES:
                store = obstore.store.from_url(self.root)
                result = await guard(RetryClass.OBJECT_STORE)(obstore.get_async, store, ref.relative)
                return Ok(bytes(result.bytes()))
            return Ok(ref.path.read_bytes())
        except OSError as cause:
            return Error(BoundaryFault(resource=(ref.owner, type(cause).__name__)))


@tagged_union(frozen=True)
class TransportResource:
    tag: Literal["http", "ssh", "speckle"] = tag()
    http: tuple[str, RetryClass] = case()
    ssh: tuple[str, int, RetryClass] = case()
    speckle: tuple[str, str] = case()

    async def acquire(self, relative: str) -> RuntimeRail[bytes]:
        match self:
            case TransportResource(tag="http", http=(url, retry_class)):
                try:
                    async with httpx.AsyncClient() as client:
                        response = await guard(retry_class)(client.get, url)
                        return Ok(response.content)
                except httpx.HTTPError as cause:
                    return Error(BoundaryFault(resource=("http", type(cause).__name__)))
            case TransportResource(tag="ssh", ssh=(host, port, retry_class)):
                try:
                    async with asyncssh.connect(host, port=port) as conn, conn.start_sftp_client() as sftp:
                        async with await guard(retry_class)(sftp.open, relative, "rb") as handle:
                            return Ok(await handle.read())
                except (OSError, asyncssh.Error) as cause:
                    return Error(BoundaryFault(resource=("ssh", type(cause).__name__)))
            case TransportResource(tag="speckle", speckle=(stream_id, token)):
                client = SpeckleClient(host=stream_id)
                client.authenticate_with_token(token)
                transport = ServerTransport(client=client, stream_id=stream_id)
                return boundary("speckle", lambda: operations.serialize(operations.receive(relative, transport)).encode())
            case _ as unreachable:
                assert_never(unreachable)
```

## [3]-[RESEARCH]

- [OBSTORE_CATALOGUE]: `obstore` is manifest-declared and resolves on the cp315 core (abi3 wheel) but carries no `.api/` catalogue; the `obstore.store.from_url` constructor and the `obstore.get_async(store, path)` / result `.bytes()` return shape backing the object-store fast-path confirm against the `obstore` catalogue at capture.
- [SPECKLE_TRANSPORT]: the `specklepy` `ServerTransport(client=..., stream_id=...)` construction and the `operations.receive(object_id, transport)` object-id contract plus the `operations.serialize(...).encode()` JSON-bytes projection backing the `TransportResource.speckle` arm confirm against the `specklepy` catalogue at capture; the receive argument is the artifact object id, never a stream id, and the returned `Base` tree serializes to the canonical bundle bytes the rail carries.
