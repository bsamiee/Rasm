# [PY_RUNTIME_ROOTS]

Resource roots and transport resources. `ResourceRoot` admits file/object-store/scratch roots over `fsspec`/`universal-pathlib` with traversal-safe relative resolution, routing many-small-object reads through the `obstore` async fast-path while keeping the `fsspec` tree-walk semantics for the shared-pull tree; `TransportResource` is the one tagged union over HTTP/SSH generic-artifact acquisition. No durable store, daemon scheduler, app lifecycle hook, product store-root derivation, or AEC-collaboration transport (Speckle/OPC-UA/MQTT terminate C#-side and reach the companion through the canonical wire, never a second Python client leg) crosses this page.

## [1]-[INDEX]

One cluster: `[2]-[RESOURCE]` — resource roots, references, transport resources.

## [2]-[RESOURCE]

- Owner: `ResourceRoot` — file/object-store/scratch roots over `fsspec` + `universal-pathlib` with safe relative resolution; `ResourceRef` the scheme/root/relative/owner value object; `TransportResource` the one tagged union over `httpx` HTTP and `asyncssh` SSH/SFTP generic-artifact acquisition (a remote-fetch transport row, never a durable store and never an AEC-collaboration client).
- Cases: `TransportResource` cases `http=(url, retry_class)` · `ssh=(host, port, retry_class)` — keyword-constructed and matched by `match`/`case`, each binding a `Retry` row from `reliability/resilience#RESILIENCE`; `acquire(relative)` takes the remote member to fetch and the same `relative` the filesystem `reader` resolves.
- Entry: `ResourceRoot.admit` admits one root and returns the frozen owner; `ResourceRoot.child` resolves a relative path with traversal rejection through `UPath.resolve`/`is_relative_to` containment, the failure constructed as the `resource` case of `reliability/faults#FAULT`; `ResourceRoot.reader` returns a `RuntimeRail[bytes]`, dispatching an object-store scheme to the `obstore` async API and a filesystem scheme to `UPath.read_bytes`, the raised `OSError` converting once to `Error(BoundaryFault(resource=...))`; `TransportResource.acquire` returns a `RuntimeRail[bytes]` over the bound retry row from `reliability/resilience#RESILIENCE` `guard`, the raised provider fault converting at the same `resource`-case boundary.
- Auto: `OBJECT_STORE_SCHEMES` routes `s3`/`gs`/`az`/`abfs` to the `obstore` stateless API (abi3 wheel, cp315-ready) for the concurrent-small-object reads that dominate companion artifact pulls, and routes every other scheme through `UPath` filesystem semantics; one polymorphic `reader` owns both paths; traversal containment compares the resolved child against the resolved root, never a string prefix that a sibling root spoofs; the rail never returns a bare `Ok` over a throwing provider call — every acquisition crosses the one `boundary` conversion.
- Packages: `fsspec`, `s3fs`, `gcsfs`, `obstore` (`store.from_url`/`get_async`/`GetResult.bytes`), `universal-pathlib` (`UPath`/`resolve`/`is_relative_to`/`read_bytes`), `httpx` (`AsyncClient.get`/`HTTPError`), `asyncssh` (`connect`/`start_sftp_client`/`SFTPClient.open`/`SFTPClientFile.read`/`Error`).
- Growth: a new storage backend is one `fsspec` protocol the `UPath` resolves or one `obstore` store the `reader` dispatches; a new transport is one `TransportResource` case binding a `Retry` row; zero new surface.
- Boundary: no default root creation, root litter, product store-root derivation, bridge staging-root ownership, service API layer, companion-control transport, or AEC-collaboration client; a path parse that bypasses `UPath`, a hand-rolled retry around acquisition, and a second Python Speckle/OPC-UA/MQTT leg are the deleted forms — Speckle terminates C#-side (`cs:Rasm.Persistence/sync/collaboration`) per the cross-`libs/` `EXTERNAL_SYSTEMS_TERMINATE_CSHARP` settled boundary, and the companion reads any Speckle-sourced artifact through the canonical wire, never a `specklepy` client.

```python signature
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
    tag: Literal["http", "ssh"] = tag()
    http: tuple[str, RetryClass] = case()
    ssh: tuple[str, int, RetryClass] = case()

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
            case _ as unreachable:
                assert_never(unreachable)
```

## [3]-[RESEARCH]

- [OBSTORE_CATALOGUE]: reflection-confirmed on the cp315 core (abi3 wheel) — `obstore.store.from_url(url, *, config=...) -> ObjectStore` is the constructor, `obstore.get_async(store, path, *, options=None)` returns a `GetResult`, and `GetResult.bytes() -> Bytes` (wrappable by `bytes(...)`) is the payload accessor backing the object-store fast-path. The `reader` `obstore` arm spelling is settled.
