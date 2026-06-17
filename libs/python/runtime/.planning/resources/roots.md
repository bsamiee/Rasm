# [PY_RUNTIME_ROOTS]

Resource roots and transport resources. `ResourceRoot` admits file/object-store/scratch roots over `fsspec`/`universal-pathlib` with traversal-safe relative resolution, routing many-small-object reads through the `obstore` async fast-path while keeping the `fsspec` tree-walk semantics for the shared-pull tree; `TransportResource` is the one tagged union over HTTP/SSH/Speckle remote-AEC acquisition. No durable store, daemon scheduler, app lifecycle hook, or product store-root derivation crosses this page.

## [1]-[INDEX]

One cluster: `[2]-[RESOURCE]` — resource roots, references, transport resources.

## [2]-[RESOURCE]

- Owner: `ResourceRoot` — file/object-store/scratch roots over `fsspec` + `universal-pathlib` with safe relative resolution; `ResourceRef` the scheme/root/relative/owner value object; `TransportResource` the one tagged union over `httpx` HTTP, `asyncssh` SSH/SFTP, and `specklepy` Speckle stream send/receive (a remote-AEC transport row, never a durable store).
- Cases: `TransportResource` cases `Http(url, retry_class)` · `Ssh(host, port, retry_class)` · `Speckle(stream_id, token)` — each matched by `match`/`case`, each binding a `Retry` row from `reliability/resilience#RESILIENCE`.
- Entry: `ResourceRoot.admit` admits one root and returns the frozen owner; `ResourceRoot.child` resolves a relative path with traversal rejection through `UPath.resolve`/`is_relative_to` containment; `ResourceRoot.reader` returns a `RuntimeRail[bytes]`, dispatching an object-store scheme to the `obstore` async API and a filesystem scheme to `UPath.read_bytes`; `TransportResource.acquire` returns a `RuntimeRail[bytes]` over the bound retry row from `reliability/resilience#RESILIENCE` `guard`.
- Auto: `OBJECT_STORE_SCHEMES` routes `s3`/`gs`/`az`/`abfs` to the `obstore` stateless API (abi3 wheel, cp315-ready) for the concurrent-small-object reads that dominate companion artifact pulls, and routes every other scheme through `UPath` filesystem semantics; one polymorphic `reader` owns both paths; traversal containment compares the resolved child against the resolved root, never a string prefix that a sibling root spoofs.
- Packages: `fsspec`, `s3fs`, `gcsfs`, `obstore` (`store.from_url`/`get_async`), `universal-pathlib` (`UPath`/`resolve`/`is_relative_to`/`read_bytes`), `httpx` (`AsyncClient.get`), `asyncssh` (`connect`/`start_sftp_client`), `specklepy` (`api.client.SpeckleClient`/`api.operations.receive`).
- Growth: a new storage backend is one `fsspec` protocol the `UPath` resolves or one `obstore` store the `reader` dispatches; a new transport is one `TransportResource` case binding a `Retry` row; zero new surface.
- Boundary: no default root creation, root litter, product store-root derivation, bridge staging-root ownership, service API layer, or companion-control transport; a path parse that bypasses `UPath`, a hand-rolled retry around acquisition, and a Speckle durable-store treatment are the deleted forms; Speckle is transport + bundle shapes only.

```python signature
from typing import Final, Literal, Self

import asyncssh
import httpx
import obstore
from expression import Error, Ok, case, tag, tagged_union
from msgspec import Struct
from specklepy.api.client import SpeckleClient
from specklepy.api import operations
from upath import UPath

from rasm.runtime.faults import BoundaryFault, RuntimeRail
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
            else Error(BoundaryFault.Resource("traversal", relative))
        )

    async def reader(self, ref: ResourceRef) -> RuntimeRail[bytes]:
        if self.scheme in OBJECT_STORE_SCHEMES:
            store = obstore.store.from_url(self.root)
            return Ok(bytes((await guard(RetryClass.OBJECT_STORE)(obstore.get_async, store, ref.relative)).bytes()))
        return Ok((ref.path).read_bytes())


@tagged_union(frozen=True)
class TransportResource:
    tag: Literal["http", "ssh", "speckle"] = tag()
    http: tuple[str, RetryClass] = case()
    ssh: tuple[str, int, RetryClass] = case()
    speckle: tuple[str, str] = case()

    @staticmethod
    def Http(url: str, retry_class: RetryClass) -> "TransportResource":
        return TransportResource(http=(url, retry_class))

    @staticmethod
    def Ssh(host: str, port: int, retry_class: RetryClass) -> "TransportResource":
        return TransportResource(ssh=(host, port, retry_class))

    @staticmethod
    def Speckle(stream_id: str, token: str) -> "TransportResource":
        return TransportResource(speckle=(stream_id, token))

    async def acquire(self) -> RuntimeRail[bytes]:
        match self:
            case TransportResource(tag="http", http=(url, retry_class)):
                async with httpx.AsyncClient() as client:
                    response = await guard(retry_class)(client.get, url)
                    return Ok(response.content)
            case TransportResource(tag="ssh", ssh=(host, port, retry_class)):
                async with asyncssh.connect(host, port=port) as conn, conn.start_sftp_client() as sftp:
                    return Ok(await guard(retry_class)(sftp.read, "/"))
            case TransportResource(tag="speckle", speckle=(stream_id, token)):
                client = SpeckleClient(host=stream_id)
                client.authenticate_with_token(token)
                return Ok(operations.receive(stream_id, client.transport))
```

## [3]-[RESEARCH]

- [OBSTORE_READER]: the `obstore.store.from_url` constructor and the `obstore.get_async` / `GetResult.bytes()` return shape backing the object-store fast-path in `ResourceRoot.reader` confirm against the `obstore` catalogue at fence transcription.
- [SSH_SFTP_READ]: the `asyncssh` `SFTPClient.read`/`SFTPClient.get` byte-read spelling backing the `TransportResource.ssh` arm confirms against the `asyncssh` catalogue; the `acquire` SSH arm reads the remote path into `bytes` through the verified SFTP surface.
- [SPECKLE_TRANSPORT]: the `specklepy` `api.client.SpeckleClient.authenticate_with_token` and `api.operations.receive(object_id, transport)` spellings backing the `TransportResource.speckle` arm confirm against the `specklepy` catalogue at fence transcription.
