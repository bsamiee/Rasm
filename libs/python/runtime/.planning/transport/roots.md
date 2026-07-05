# [PY_RUNTIME_ROOTS]

This page owns resource-root and transport-resource acquisition. `ResourceRoot` admits file, object-store, and scratch roots over `fsspec`/`universal-pathlib` with traversal-safe relative resolution, routing many-small-object reads through the `obstore` async fast-path and the local tree through `UPath` filesystem semantics. `TransportResource` is the one tagged union over `httpx` HTTP and `asyncssh` SSH/SFTP generic-artifact acquisition. Every acquisition rides one `Transfer` aspect fusing the `reliability/resilience#RESILIENCE` `guarded` retried-traced-railed envelope, so the page mints no second retry loop, derivation span, or inline `try`/`except` ladder — the AOP collapse the sibling `transport/wire#WIRE_RAIL` `Decode` aspect realizes for the codec leg, applied to the transport leg. No durable store, daemon scheduler, app lifecycle hook, product store-root derivation, or AEC-collaboration transport crosses this page; Speckle, OPC-UA, and MQTT terminate C#-side and reach the companion through the canonical wire, never a second Python client leg.

## [01]-[INDEX]

- [02]-[RESOURCE]: the `Transfer` acquisition aspect, the `TransferPlan` value object, resource roots, references, transport resources.

## [02]-[RESOURCE]

- Owner: `Transfer` — the one cross-cutting acquisition aspect every reader composes. `TransferPlan` is the frozen `subject`/`retry_class`/`whole`/`stream` value object both entrypoints build and `Transfer.run` consumes, so the WHOLE-arm retry-span-fault triplet and the STREAM-arm lazy thunk are one shape rather than a five-positional helper signature. `ResourceRoot` is the file/object-store/scratch root over `fsspec` + `universal-pathlib` with safe relative resolution; `ResourceRef` the `scheme`/`root`/`relative`/`owner` value object; `TransportResource` the one tagged union over `httpx` HTTP and `asyncssh` SSH/SFTP generic-artifact acquisition — a remote-fetch transport row, never a durable store or AEC-collaboration client. The fault, transport, lane, and resilience clusters read acquisition only through `Transfer.run`.
- Cases: `TransportResource` carries two keyword-constructed `case()` arms — `http=(url, retry_class, bearer)` and `ssh=(host, port, retry_class, password, known_hosts)` — each binding a `RetryClass` discriminant and an `Option[SecretStr]` outbound credential the caller resolves through `execution/admission#SETTINGS` `SecretBoundary.resolve(service, username, shape=SecretShape.TOKEN)` before construction. The credential rides the `pydantic` `SecretStr` the `SecretShape.TOKEN` leg yields, never the bare `str` a repr, log, or receipt egress could serialize, and un-masks through `.get_secret_value()` only at the `_BearerAuth`/`_ssh_options` seam.
  - The `ssh` credential is the `SSHClientConnectionOptions` `password=` authentication secret, not the `import_private_key(passphrase=...)` key-decryption secret a future `client_keys` field would carry. The case additionally binds the verified `asyncssh.SSHKnownHosts` database the same `SecretBoundary.known_hosts`/settings owner loads through `asyncssh.read_known_hosts`, so the connection law's host-key verification is admission-supplied through the same options object, never disabled.
  - `acquire(relative, modality)` takes the remote member to fetch and the same `relative` the filesystem `read` resolves. The read modality — whole-payload `Bytes` for small artifacts or `AsyncIterator[Bytes]` for large — rides the `ReadModality` data discriminant the shared `Transfer.run` aspect matches: one entrypoint owns all modalities, the modality a closed-enum value not a `streaming=True` knob, never a `read`/`stream` name-suffix split.
  - `match self` dispatches on the union tag once and `Transfer.run` dispatches on `ReadModality` once, the four prior `(tag, modality)` cartesian arms collapsing to two tag arms each handing the aspect a `TransferPlan` thunk pair.
- Entry: every reader composes one of three entrypoints over the one `Transfer.run` aspect; `read`/`survey`/`acquire` never re-spell the prior `_acquire` + inline `guard(...)`-inside-`async_boundary` triplet that opened no span and re-implemented the resilience envelope.
  - `ResourceRoot.admit(uri, owner)` admits one root and returns the frozen owner. `ResourceRoot.child(relative)` resolves a relative path with traversal rejection through `UPath.resolve`/`is_relative_to` containment and constructs the failure as the `resource` case of `reliability/faults#FAULT`.
  - `Transfer.run(plan, modality)` is the one acquisition aspect. `ReadModality.STREAM` returns `Ok(plan.stream())` — the lazy iterator un-fenced, its provider-fault lift deferred to the `evidence/identity#IDENTITY` consumer that pulls it. `ReadModality.WHOLE` delegates the whole retried-traced-railed acquisition to `reliability/resilience#RESILIENCE` `guarded(plan.retry_class, plan.whole, subject=plan.subject)`, which fuses the per-class `stamina` retry, the `resilience.guarded` derivation span, and the one-shot `reliability/faults#FAULT` `async_boundary` terminal-fault lift into one `RuntimeRail[Acquired]`. An `OSError` lands as the owner-named `resource` case, a `TimeoutError` as `deadline`, and an `obstore`/`httpx`/`asyncssh` provider raise as `boundary` through the one `CLASSIFY` fold — never a lossy `.map_error` re-tag forcing every class to `resource` and reading a non-active `fault.boundary` case slot.
  - `ResourceRoot.read(ref, modality)` is the one polymorphic reader returning `RuntimeRail[Acquired]`, building the `(whole, stream)` `TransferPlan` by scheme. The object-store arm (`_obj_body`/`_obj_chunks` over `obstore.get_async` + `GetResult.bytes()`/`GetResult.stream(STREAM_CHUNK)`) binds `RetryClass.OBJECT_STORE`, whose `target` is the `obstore` transient base. The filesystem arm (`anyio.to_thread.run_sync(UPath.read_bytes)`/`_file_chunks`, offloading the blocking read to the `anyio` worker-thread pool under the explicit `SCAN_BAND` `CapacityLimiter` — never the ambient default limiter) binds `RetryClass.SCAN`, whose `(OSError,)` `target` is the filesystem fault base the `scan` row retries — never the `OBJECT_STORE` row whose `obstore`-typed `target` excludes a filesystem `OSError`, which would surface a transient local read as an immediate terminal fault. The WHOLE object-store path runs the `get_async` materialize and the `.bytes()` accessor inside one `_obj_body` coroutine, so both cross the single `guarded` envelope.
  - The STREAM `AsyncIterator[Bytes]` feeds the `evidence/identity#IDENTITY` `ContentIdentity.of` `stream` modality after the consumer drains it into the synchronous `tuple[bytes, ...]`/`Iterable[bytes]` that owner's `IdentitySource.lift` keys (`tuple([chunk async for chunk in stream])`). The buffer-protocol `Bytes` chunk passes into the `xxh3_128` updater with no `bytes(...)` re-copy and no full-artifact buffer held across the runtime; the async drain is the boundary seam, not a synchronous `for` over an `AsyncIterator` the identity owner's sync fold cannot drive.
  - `ResourceRoot.survey(ref, view)` is the second object-store entrypoint parameterized over BOTH the `StoreView` enum input AND its output through `@overload`, returning the precise `RuntimeRail[AsyncIterator[RecordBatch]]` for `LIST` and `RuntimeRail[str]` for `SIGN`, never a stringly `AsyncIterator[RecordBatch] | str` union the caller re-narrows. `StoreView.LIST` streams the Arrow `RecordBatch` inventory through `obstore.list(store, prefix, chunk_size=LIST_CHUNK, return_arrow=True)` — the columnar listing fast-path the `data` Arrow consumers join against, never a per-`ObjectMeta` Python loop; the lazy `ListStream` construction is non-failing so it returns `Ok` directly, and the downstream `data` consumer owns per-`RecordBatch` iteration-fault conversion as it pulls the stream. `StoreView.SIGN` mints the presigned URL through `obstore.sign_async(store, "GET", paths, SIGN_TTL)` under the shared `guarded(RetryClass.OBJECT_STORE, ...)` envelope so a downstream companion fetch is a bare `httpx` GET against a time-boxed URL with no credential leg — `expires_in` is a `timedelta`, a single-`str` path returns a single URL the rail carries directly, and `sign_async` raises `NotSupportedError` only on a non-signing backend the gate precludes. Both arms are object-store-scheme-gated through `_store`, and `OBJECT_STORE_SCHEMES` admits only sign-capable backends (`s3`/`gs`/`az`/`abfs`), so the containment is the static proof the `SIGN` arm never hands a non-signing store.
  - `TransportResource.acquire(relative, modality)` is the matching one entrypoint: `match self` binds the `http`/`ssh` case payload and builds the per-tag `TransferPlan`, the WHOLE arm fetching a small body through `_http_body`/`_sftp_read` and the STREAM arm through `_http_chunks`/`_sftp_chunks` over `httpx.AsyncClient.stream`/`Response.aiter_bytes` and the SFTP file handle under explicit `httpx.Timeout`/`httpx.Limits`. The bound `RetryClass` rides `Transfer.run`, so each provider call retries under the resilience owner's policy row and the raised provider fault converts at the one `guarded` `async_boundary` into the `CLASSIFY`-precise tag, an exhausted `assert_never` anchoring the closed two-case union. The `ssh` arm folds its `(password, known_hosts)` pair into one `asyncssh.SSHClientConnectionOptions` through `_ssh_options` before threading `(host, port, relative, options)` to the SFTP legs — the catalog options-law one-config-object reuse, never the scattered `connect(...)` keyword soup the `asyncssh` Reject-row forbids and never the page's own condemned five-positional thread.
  - The outbound credential reads from `execution/admission#SETTINGS` `SecretBoundary.resolve` and rides the case as an `Option[SecretStr]`. The `ssh` password un-masks through `password.map(SecretStr.get_secret_value)` into the `SSHClientConnectionOptions` `password=` field; the `http` bearer rides the `httpx`-native `Auth` flow `_BearerAuth` whose `__init__` reads `.get_secret_value()` once into the bound `Authorization` header the `_auth` projection mints per acquisition and `_http_body`/`_http_chunks` pass as the per-call `auth=` argument. The `SecretStr` un-masks only at these two transport seams; the `httpx` auth-law credential seam owns the `Authorization` header, never a hand-rolled header dict, and a `Nothing` credential resolves to `auth=None`, the client default. `httpx` ships no bearer-`Auth` class — only `BasicAuth`/`DigestAuth`/`NetRCAuth` — so the bearer is the catalog-sanctioned custom `auth_flow` generator, never a re-implemented basic/digest leg.
  - The inbound 4xx/5xx status check is the client-level `event_hooks={"response": [_raise_for_status]}` seam bound once at `_transport_client` construction, so both the WHOLE `_http_body` and the STREAM `_http_chunks` enforce status at the one transport seam, never an asymmetric inline `raise_for_status`. The `ssh` arm always verifies the host key against the known-hosts database `execution/admission#SETTINGS` threads through `asyncssh.read_known_hosts` into the `SSHClientConnectionOptions` `known_hosts=` field, never the disabled-verification `known_hosts=None` the `asyncssh` RAIL_LAW Reject-row forbids.
- Cache: `obstore.store.from_url` construction parses the URL, resolves the backend, and binds the credential provider, so per-access reconstruction re-pays that cost on every `read`/`survey` of the same root. `_store` constructs once through the `functools.cache`-memoised `_object_store(root)` factory keyed by the root URL and returns the cached `ObjectStore` thereafter — the same pure-by-key memoisation the `reliability/resilience#RESILIENCE` `_caller` bound-caller cache uses — and the handle is a thread-safe immutable client the concurrent-small-object fan-out shares, never a fresh store per `read`, a store rebuilt per chunk, or a hand-rolled `fsspec` cache the `fsspec` Reject-row forbids. The `from_url` call threads `retry_config=STORE_RETRY`, so the `obstore` Rust-core `RetryConfig` owns transient-fault backoff at the store edge and the `RetryClass.OBJECT_STORE` `stamina` row is the deliberate outer envelope over it, never a hand-rolled retry loop the `obstore` RAIL_LAW forbids.
  - The symmetric `functools.cache`-memoised `_transport_client(endpoint)` factory keyed by the scheme-host-port endpoint group constructs one long-lived `httpx.AsyncClient` over an explicit `httpx.AsyncHTTPTransport(retries=CONNECT_RETRIES, http2=True, limits=TRANSPORT_LIMITS)` and shares its pool across every WHOLE/STREAM acquisition of that endpoint — the `httpx` client-law one-client-per-endpoint-group reuse, deleting the per-request `AsyncClient(...)` construction the `httpx` Reject-row forbids. `http2=True` is load-bearing for exactly this pool's workload: the concurrent-small-object artifact fan-out multiplexes over one negotiated h2 connection instead of queueing on `max_connections`, and `Response.http_version` confirms the negotiation per response. The transport's `retries` is the connection-establishment inner envelope — a refused or dropped CONNECT re-dials at the socket layer before the caller-bound `RetryClass` `stamina` row ever fires, the same two-tier shape the object-store arm holds with `from_url(retry_config=STORE_RETRY)` under `RetryClass.OBJECT_STORE`. The pooled client lives for the runtime, so `_http_chunks` reuses it and only its per-call `stream` response-context rides the per-acquisition `AsyncExitStack`, never the client itself and never an `aclose` per fetch that would tear down the shared pool; the pool releases once through the `drain` teardown the host choreography awaits, the `httpx` drain-law obligation that keeps it off the GC.
  - The whole-payload `Bytes` arm is reserved for small payloads; the `AsyncIterator[Bytes]` arm bounds the large GLB/IFC/scan artifacts the `httpx`/`obstore` streaming surfaces exist for. A full-body `client.get` of a large payload is the `httpx` Reject-row violation (streaming law) and a `GetResult.bytes()` of a large object is the `obstore` Reject-row violation (RAIL_LAW).
  - The rail never returns a bare `Ok` over a throwing provider call. Every WHOLE acquisition crosses the one `Transfer.run` `guarded` envelope — the lifted fault carrying the owner subject and the `CLASSIFY`-precise tag, never a forced `resource` re-tag reading a non-active case slot — and every STREAM acquisition defers its provider-fault lift to the `evidence/identity#IDENTITY` consumer that pulls the iterator. The single acquisition aspect replaces the per-provider `_acquire`/`async_boundary` triplet the catalogue Reject-rows forbid.
- Cleanup: a streamed acquisition the consumer abandons (an early `break`, a fault upstream, a partial-read identity probe) releases the provider session deterministically through one `contextlib.AsyncExitStack` per leg, and the runtime-lived pools release through the one `drain` the host choreography awaits.
  - `drain` is the one teardown the host calls once at shutdown: it `aclose`s every pooled `httpx.AsyncClient` the `_transport_client` cache minted (tracked in `_LIVE_CLIENTS` because `functools.cache` surrenders no value enumeration) under one `anyio` task group, then clears both `@cache` factories. This is the `httpx` drain-law obligation `transport/serve#CAPABILITY_INVOKE` names this owner for — the pooled client is never left to GC, the symmetry to `ServerHost.drain`/`CapabilityInvoke.aclose`. The cached `ObjectStore` is a GC-safe Rust handle with no async pool, so its cache only clears.
  - `_sftp_session` opens the `asyncssh.connect`/`start_sftp_client`/`SFTPClient.open` chain through one stack the `_sftp_read` whole-fetch and the `_sftp_chunks` generator both compose. For the generator the stack's `__aexit__` runs in the `finally`: when the consumer stops iterating, `aclose()` raises `GeneratorExit` into the suspended `yield`, the `finally` unwinds the stack, and the SFTP file, the SFTP client, and the SSH connection close in reverse order with no leaked socket across the event loop (the `asyncssh` Reject-row leaked-connection violation). For the whole-fetch the same stack closes the chain on coroutine return.
  - `_http_chunks` keeps that shape over the `httpx.AsyncClient.stream` context, the `AsyncExitStack` closing only the per-call response context, never the cached client, on abandonment.
  - `_file_chunks` offloads the blocking `UPath.open`/`handle.read` to `anyio.to_thread.run_sync` and closes the handle in its own `finally`, so a blocking-IO chunk read rides the bounded worker-thread pool without stalling the event loop and the descriptor releases on early `break`.
- Growth: a new storage backend is one `fsspec` protocol the `UPath` resolves or one `obstore` store the `read` dispatches; a new transport is one `TransportResource` case binding a `RetryClass`; a new read size class is one `ReadModality` row the shared `Transfer.run` aspect already discriminates — the partial-read identity probe is exactly that next row, one `ReadModality` member over `obstore.get_ranges(store, path, starts=, ends=)`/`open_reader` when `evidence/identity#IDENTITY` names the probe, the deep range/reader surface otherwise staying the data folder's to mine; a new object-store inventory or presign mode is one `StoreView` row plus one `@overload` the existing `survey` match already discriminates, never a fourth method; a new retry geometry is one `reliability/resilience#RESILIENCE` `RetryClass` row the `TransferPlan` binds, never a new acquisition aspect or inline loop; zero new surface.
- Boundary: this page owns runtime-transport acquisition only — the concurrent-small-object/large-artifact-stream `read` and the generic `survey(StoreView.LIST/SIGN)` inventory/presign for companion artifact pulls. It mints no default root creation, root litter, product store-root derivation, bridge staging-root ownership, service API layer, companion-control transport, or AEC-collaboration client, and it never moves a data-tier bundle: the `data:tabular/egress#EGRESS` `ObjectEgress` owns the data-tier object-store bundle I/O (the full `put`/`copy`/`rename`/`delete`/`writer` write direction plus the Arrow/Parquet/GeoParquet/zarr bundle-byte reads its `data` consumers need), so the two share the `obstore` provider and the `RetryClass.OBJECT_STORE` envelope but split by tier — runtime artifact here, data bundle there — never a duplicated read/survey lane. Speckle terminates C#-side (`cs:Rasm.Persistence/Version/ledger#SYNC_TRANSPORTS`) per the cross-`libs/` `EXTERNAL_SYSTEMS_TERMINATE_CSHARP` boundary, and the companion reads any Speckle-sourced artifact through the canonical wire, never a `specklepy` client. The deleted forms:
  - a path parse that bypasses `UPath`, or a string-prefix traversal check a sibling root spoofs where the resolved-child-against-resolved-root containment holds.
  - a hand-rolled retry around acquisition, or a per-provider `_acquire`/inline `async_boundary` triplet where the one `Transfer.run`/`guarded` aspect fuses the retry/span/fault lift.
  - a `.map_error` re-tag forcing every lifted class to the `resource` case and reading a non-active `fault.boundary` slot, where the `guarded` `async_boundary` subject already names the owner and `CLASSIFY` already lands the precise tag.
  - a stringly `AsyncIterator[RecordBatch] | str` `survey` return the caller re-narrows, where the `@overload` pins the `StoreView`-indexed output.
  - a hand-rolled `{"Authorization": f"Bearer {token}"}` header dict where the `httpx` `Auth` flow (`_BearerAuth`) owns the credential seam, an inline transport credential where `SecretBoundary.resolve` supplies it, or a bare-`str` credential carrier where the `Option[SecretStr]` case and a single `.get_secret_value()` seam keep the secret un-serialized through repr/log/receipt egress.
  - a per-call inline `response.raise_for_status()` (and the asymmetric WHOLE-arm omission of it) where the client `event_hooks` response seam enforces status once.
  - a no-argument `GetResult.stream` property read of the bound method object where the catalog signature is `.stream(min_chunk_size)`, or a bare-int `expires_in` where `obstore.sign_async` takes a `timedelta`.
  - an `obstore.head` size probe auto-selecting WHOLE-vs-STREAM where `ReadModality` is the caller-declared closed-enum discriminant — the modality is recoverable from the caller's own contract, never re-derived from a second metadata round-trip.
  - a blocking `UPath.read_bytes`/`handle.read` run directly on the event loop where `anyio.to_thread.run_sync` offloads it, a thread hop trusting the ambient default limiter where `SCAN_BAND` is the explicit filesystem bound, or a whole-artifact buffer where the streaming modality applies.
  - a disabled-verification `known_hosts=None` SSH connection, a scattered `connect(host, port=, password=, known_hosts=)` keyword soup or a five-positional `(host, port, relative, password, known_hosts)` thread re-spelled per SFTP leg where one `_ssh_options` `SSHClientConnectionOptions` value object carries the config, a per-request `httpx.AsyncClient` construction, a `from_url` with no `retry_config` where the Rust-core `RetryConfig` is the inner envelope under the `OBJECT_STORE` row, and a second Python Speckle/OPC-UA/MQTT leg.
  - an endpoint key derived from the raw `urlsplit(url).netloc` where `hostname`/`port` is the group, the `netloc`'s `user:password@` userinfo otherwise leaking the credential into the `functools.cache` key and pooling the shared client by secret rather than by destination.
  - a pooled `httpx.AsyncClient` left for the GC where the `drain` teardown `aclose`s every `_LIVE_CLIENTS` pool under one task group, or a per-call `client.aclose` tearing down the shared pool where only the per-call `stream` response-context closes.
  - a re-derived `Feature.OUTBOUND_TRANSPORT`/`Killswitch.DISABLE_OUTBOUND` gate inside `acquire`/`read` where `execution/admission#SETTINGS` already gates the leg at credential resolution: the caller resolves through the gated `SecretBoundary.resolve` and constructs the `TransportResource` case only on an admitted feature, so re-checking `RuntimeContext.admits` here is the double-gate the owner split forbids — admission owns gating, roots owns acquisition.

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

# the filesystem blocking-read bound: every roots thread hop rides this explicit CapacityLimiter,
# never the ambient default limiter; lanes' THREAD_BAND is an offload-tier band above this tier.
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
    # the `SecretStr` is read through `.get_secret_value()` exactly once, here at the transport
    # seam, and never re-exposed: the bound `_header` is the only `str` the credential becomes.
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
    # the endpoint group is `scheme://host:port` off `hostname`/`port`, NEVER the raw `netloc`:
    # `netloc` carries any `user:password@` userinfo, which would leak the credential into the
    # `functools.cache` key and pool the client by secret rather than by destination.
    parsed = urlsplit(url)
    return f"{parsed.scheme}://{parsed.hostname}:{parsed.port}" if parsed.port else f"{parsed.scheme}://{parsed.hostname}"


@cache
def _object_store(root: str) -> ObjectStore:
    return obstore.store.from_url(root, retry_config=STORE_RETRY)


_LIVE_CLIENTS: Final[set[httpx.AsyncClient]] = set()


@cache
def _transport_client(endpoint: str) -> httpx.AsyncClient:
    # the `@cache` body runs once per endpoint group; `_LIVE_CLIENTS` is the drainable handle
    # because `functools.cache` surrenders no value enumeration to the `drain` teardown. The
    # explicit transport owns h2 multiplexing, the pool limits, and the connect-dial inner retry.
    client = httpx.AsyncClient(
        timeout=TRANSPORT_TIMEOUT,
        transport=httpx.AsyncHTTPTransport(retries=CONNECT_RETRIES, http2=True, limits=TRANSPORT_LIMITS),
        event_hooks={"response": [_raise_for_status]},
    )
    _LIVE_CLIENTS.add(client)
    return client


async def drain() -> None:
    # the `transport/serve#CAPABILITY_INVOKE` drain-law obligation: `aclose` every pooled
    # `httpx.AsyncClient` so no pool reaches the GC; the Rust `ObjectStore` has no async pool.
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
    # the one connection-config value object the catalog options-law mandates over per-call
    # `connect(...)` keyword soup; `.get_secret_value()` un-masks the password only here, and
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

