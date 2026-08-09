# [PY_RUNTIME_ROOTS]

Resource-root, object-store, and transport-resource acquisition: `ResourceRoot` admits file, object-store, and scratch roots over `fsspec`/`universal-pathlib` with traversal-safe relative resolution, `ObjectStoreLane` is the one `obstore` operation surface the whole branch dispatches through, and `TransportResource` is the one tagged union over `httpx` HTTP and `asyncssh` SSH/SFTP generic-artifact acquisition. `RemoteEndpoint` is the one asyncssh channel custodian — connection identity, options construction, known-hosts law, and the credential un-mask live here for both consumers, the SFTP acquisition legs on this page and the `execution/workers#POOL` REMOTE exec crossing — so the asyncssh scope widens exactly one seam past SFTP-read and a second SSH dial spelling never forms. `HttpEndpoint` is its HTTP mirror, carrying the destination identity, the RFC-9111 cache posture, the egress hop, and the keyed-store coordinate. No durable store, daemon scheduler, product store-root derivation, or AEC-collaboration transport crosses this page; Speckle, OPC-UA, and MQTT terminate C#-side per the cross-`libs/` boundary and reach the companion through the canonical wire, never a second Python client leg.

Every acquisition rides one `Transfer` aspect fusing the `reliability/resilience#RESILIENCE` `guarded` retried-traced-railed envelope, so the page mints no second retry loop, derivation span, or inline `try`/`except` ladder. A STREAM acquisition returns its lazy iterator under single-consumer custody and defers the provider-fault lift to the `evidence/identity#IDENTITY` consumer that pulls it. One `StoreBackend` row family is the branch's whole store vocabulary — every scheme roster, reach cell, and native driver name derives from it — and one `ObjectStoreLane` owns the whole `obstore` operation set beneath it, so `data/tabular/egress#EGRESS` composes the transport rather than re-minting a second provider composer beside it: receipt, quantity, and veto semantics at the data tier, transport at this one.

## [01]-[INDEX]

- [02]-[RESOURCE]: the `Transfer` acquisition aspect over resource roots, references, and transport resources.
- [03]-[STORE]: the `ObjectStoreLane` object-store operation surface — the `StoreOp` axis, the `_ROUTE` fold, the reach matrix, and the caller-supplied admission gate.

## [02]-[RESOURCE]

- Owner: `Transfer` is the one cross-cutting acquisition aspect every reader composes — the fault, transport, lane, and resilience clusters read acquisition only through `Transfer.run`; `TransferPlan` keeps the WHOLE retry-span-fault triplet and the STREAM lazy thunk one shape rather than a five-positional helper signature.
- Cases: outbound credentials resolve through `execution/admission#SETTINGS` `SecretBoundary.resolve` BEFORE case construction and ride as `Option[SecretStr]`, un-masking through `_BearerAuth`/`_proxy`/`_ssh_options` only at the transport seam — admission owns gating at credential resolution, so re-checking `RuntimeContext.admits` inside `acquire`/`read` is the forbidden double-gate. Each case carries ONE endpoint owner beside its `RetryClass`: the `ssh` case a `RemoteEndpoint` whose `password` is the connection authentication secret, distinct from a key-decryption passphrase a `client_keys` field carries, whose `known_hosts` is the admission-supplied verified database, and whose `root` confines every SFTP request through `confined`, so an absolute or escaping path rails before any dial, the dial carrying connect-timeout and keepalive policy as constants so an idle fleet channel's death is observed within `SSH_KEEPALIVE_MAX` probes rather than discovered at the next submit; the `http` case an `HttpEndpoint` whose `posture` declares the destination's cache class, whose `proxy` declares its egress hop, whose `confined` holds the same gate over the request url, and whose `pool` axes key the client. `Delivery` is the caller-declared closed-enum discriminant — never an `obstore.head` size probe re-deriving it from a second metadata round-trip.
- Law: containment is ONE gate spelled at the three residences it guards — `ResourceRoot.child` resolves a filesystem child against a resolved root, `RemoteEndpoint.confined` normalizes a POSIX request under the session root, `HttpEndpoint.confined` reads an RFC-3986 join back against its origin and path — because each residence resolves escapes by its own rules and no single fold expresses all three. What they share is the verdict and the comparison: an escaping request rails `resource` under its own `traversal:` subject BEFORE any dial, request, or handle, and every gate compares resolved SEGMENTS rather than string prefixes.
- Law: `TRANSPORT_ARMS` answers the selection descriptor per arm as data — `fits`, `admit`, `lifetime`, `degrade` — because the `(Endpoint, RetryClass)` tuple states machinery and never which arm a reader wants, and the two arms diverge on every cell: a pooled, cached, proxied HTTP dial and a per-call SFTP dial share no pool, no cache, no egress vocabulary, and no session custody. Tenancy is not a column, on `StoreBackend`'s own reason — an arm isolates no tenant, the endpoint's confined root and resolved credential do — and a coordinate an arm cannot express records the divergence on `degrade` rather than dropping the column.
- Law: `_object_store`/`_transport_client` memoise one handle per key — `from_url` parses, resolves, and binds credentials, so per-access reconstruction re-pays that cost, and the credential provider joins the store key as its own axis because two roots differing only in provider are two credential resolutions the one handle cannot serve. A client key holds only plain strings whose secrets are one-way digests: an `httpx.Proxy` value would key by object identity and fork a pool per call site, while a live secret there would key by secret. Retry has ONE owner: `RetryClass` holds every curve, and `store_handle` pins the Rust core to a single attempt so no store operation runs under two schedules whose effective attempts are their product. `AsyncHTTPTransport(retries=CONNECT_RETRIES)` survives beside it because it re-dials a failed CONNECTION alone and moves no request, so it composes under a curve rather than nesting a second one inside it. `http2=True` is load-bearing — the concurrent-small-object fan-out multiplexes over one negotiated h2 connection instead of queueing on `max_connections`. Status enforcement is the client `event_hooks` response seam bound once, never an asymmetric inline `raise_for_status`.
- Law: egress policy is per DESTINATION and rides the `next_transport` the cache transport wraps, so one row states proxy, connect retries, h2, and pool bounds together and an absent proxy IS direct egress. `AsyncClient(proxy=)` is the refused spelling for a reason the provider fixes in code: a client-level proxy mounts a fresh bare transport per url pattern that the client returns ahead of an explicit `transport=`, so a proxied request bypasses both the cache and the connect retries, and the same explicit `transport=` disables env-proxy resolution so `HTTP_PROXY`/`HTTPS_PROXY` are inert for that client. The proxy credential joins the client key as a digest rather than riding per request, because it is baked into the `Proxy` the pooled transport holds — two callers proxying one hop under different credentials are two transports, or the second rides the first's identity.
- Law: the HTTP cache rides the transport seam, so one long-lived `AsyncClient` keeps its timeout, auth, and status hook while `AsyncCacheTransport` wraps the pooled `AsyncHTTPTransport` beneath it — a swapped `AsyncCacheClient` is the rejected form, and posture is per DESTINATION from birth because one freshness rule cannot serve both a long-TTL revalidating catalog root and a presigned URL that must never be stored at all. Cache state crosses in BOTH directions on the provider's own extension vocabulary: the posture's `RequestMetadata` keys ride the outbound request so body-keying and TTL refresh are per-destination request facts rather than a policy subclass every request through the pooled client then pays, and the response's `ResponseMetadata` keys land the reached decision and its stored-at stamp on the acquisition span its own plan opened — never a hit/miss inferred from `Cache-Control`/`Age` headers and never a span-derived counter, because `opentelemetry-instrumentation-httpx` spans BENEATH the cache so a served entry emits no origin span at all. This is the RFC-9111 revalidation half of the two-cache ruling; `execution/recipe#RECIPE` owns content-keyed artifact elision and neither substitutes for the other.
- Law: the residence credential rides `ResourceRef`, so every store-opening consumer reads ONE column instead of growing its own — a lane takes no `credential_provider` beside its ref, a keyed store takes none beside its root, and a page holding a bare ref reaches a private or requester-pays residence without a second field its own callers then have to fill. It seats on the ref rather than the root because the ref is what CROSSES: a root mints refs and stamps its own provider onto each, so the fact travels with the coordinate it credentials and a consumer never joins two values to open one handle.
- Law: a returned STREAM is single-consumer and `_Fenced` is where that custody is real — the guard enters ahead of the delegated pull, so an overlapping second puller rails on `anyio.BusyResourceError` before it can interleave the provider session, where a guard inside a leg's own generator never fires at all: the interpreter rejects the frame re-entry first, with an untyped raise naming the frame rather than the resource. The guard reads OVERLAP and not ownership, so a sequential hand-off between tasks stays lawful. The same bracket ends the acquisition span, because a span is a value the leg carries and never an attached context: an async generator shares its consumer's context, so attaching there leaves the acquisition span current between pulls and stamps every unrelated operation the consumer runs mid-drain.
- Law: an abandoned stream releases its provider session deterministically through one `AsyncExitStack` per leg — `aclose()` raises `GeneratorExit` into the suspended `yield` and the stack unwinds in reverse order — closing only the per-call response context, never the cached client. `drain` is the one teardown the daemon awaits as a `(subject, stage)` row on the `transport/serve#ENTRY` drain fold, and it is TERMINAL for the composition rather than a reclaim a live acquisition may race: it runs after that entry stopped admitting, so a client minted between the clear and the close would outlive the teardown that already enumerated the map while a request already in flight would meet a closed pool. Custody is therefore one-way — a re-armed composition mints fresh handles rather than adopting a drained map, exactly as the band probe re-registers rather than surviving its retire. Every pooled client `aclose`s so no pool reaches the GC, and ONE close is total because the client closes its transport and the cache transport closes both the wrapped pool and its keyed storage — a second `storage.close()` beside it is the doubled teardown, never the missing one. The cached `ObjectStore` is a GC-safe Rust handle whose cache only clears.
- Entry: `read` is ONE entry over both arities, discriminating on the value — a `ResourceRef` answers `Acquired`, a `Block[ResourceRef]` fans that same per-ref plan through one task group and answers a `Batch` whose members each carry their own rail, so a refused ref sheds no sibling's payload and a caller re-drives exactly what failed. The fan inherits the bounds the single acquisition already takes — `SCAN_BAND` on every filesystem hop, the row's own retry envelope on every store hop — so it buys concurrency and never a second bound; the `scan` band registers its occupancy probe on the first filesystem plan and retires on `drain`. A WHOLE acquisition crosses the one `guarded` envelope, `CLASSIFY` landing the precise tag under the plan's subject — never a `.map_error` re-tag forcing every class to `resource`. Filesystem reads bind `RetryClass.SCAN`, whose `(OSError,)` target retries a transient local read; the `OBJECT_STORE` row's `obstore`-typed target excludes `OSError` and surfaces it terminal. A `read` carrying the caller's prior provider validator threads it as the `if_none_match` precondition, so an unchanged remote answers `NotModifiedError` instead of re-downloading a payload the identity fold discards. STREAM iterators feed the `evidence/identity#IDENTITY` sync fold only after the consumer's async drain — the drain is the boundary seam. `httpx` ships no bearer-`Auth` class, so `_BearerAuth` is the catalog-sanctioned custom `auth_flow`; the whole-payload arm is reserved for small payloads and the stream arm bounds the large GLB/IFC/scan artifacts, per the provider streaming laws.
- Auto: a thread hop abandons on cancel exactly where the abandoned thread holds nothing the frame still owns — the WHOLE filesystem read does, so a cancelled scope stops waiting out a multi-gigabyte read, while the STREAM legs do not: an abandoned open strands the descriptor its frame is the sole closer of, and an abandoned block read races the very close that follows it. The bound is what makes the split affordable, since an attached hop waits at most one `STREAM_CHUNK`.
- Growth: a new storage backend is one `StoreBackend` row every derived roster picks up untouched; a new transport is one `TransportResource` case binding an endpoint owner and a `RetryClass` beside its `TRANSPORT_ARMS` row; a new read size class is one `Delivery` row; a new call arity is one `read` arm over the value's own shape, never a second entry; a new credential provider is one `Provider` value on the admitting root, reaching every ref and lane it mints untouched; a new cache posture is one `CachePosture` member with its `_CACHE_POLICY` and `_CACHE_EXTENSIONS` rows, the pair splitting what the client's policy decides from what each request declares; a new egress hop is one `proxy` value on the admitting endpoint, reaching the key and the transport at once; a new retry geometry is one `RetryClass` row the `TransferPlan` binds; a new SSH consumer is one `RemoteEndpoint.dialed` call site, never a second options or dial spelling.
- Boundary: runtime-transport acquisition, object-store operation dispatch, and SSH channel custody only — no default root creation, bridge staging-root ownership, service API layer, or companion-control transport. `RemoteEndpoint.dialed` is the one asyncssh dial in the branch; the workers REMOTE arm consumes it for the exec crossing and owns everything past the established connection — sessions, the sealed-kernel payload, deadline enforcement, supervision. `OBJECT_STORE_SCHEMES` names the remote object-store residence the `read` dispatch and the `data/gridded/store#STORE` engine select on, derived off the row family's own `remote` column rather than hand-listed or read off a capability that merely agrees with it today. Speckle terminates C#-side (`csharp:Rasm.Persistence/Version/ledger#SYNC_TRANSPORTS`), and this branch reads any Speckle-sourced artifact through the canonical wire, never a `specklepy` client. Rejected: a second scheme roster, backend literal, or native-driver map beside the row family; a `hishel_cache.db` provider-default store path where the composition-admitted scratch root owns it; a bearer or proxy secret joining a client pool key; a client-level proxy beside an injected transport; an `fsspec.open_files` batch arm, whose sync single-use context fans its opens through a concurrency running outside every bound this owner accounts for.

## [03]-[STORE]

- Owner: `ObjectStoreLane` — the one `obstore` operation surface, discriminating `StoreOp` through one `_ROUTE` data table over one `_StoreRow` shape, never a `put_object`/`get_object`/`list_objects` method family, never an async method twin, never a second handle row beside the byte-bearing rows. `store_handle` is the branch's one public `from_url` fold beside it, so a sibling needing a bare registry of per-URL handles rather than an operation surface — a manifest walker registering many archival sources — inherits the retry envelope and the credential carry instead of re-spelling the constructor. Its ONE `ObjectStore` handle mints at `of` off the `ref.root` scheme with the Rust core pinned to a single attempt, so every row crosses the provider exactly once per rail attempt and the `retry` column alone decides whether a second attempt exists at all — an ambiguously-succeeded copy/rename replayed inside the provider corrupts state the same as one replayed by the rail, which is why the pin is unconditional rather than a zero-retry twin handle standing beside a retried one. Because the `obstore` sync and async members carry identical keyword signatures, `_async` is the same row read under a second entrypoint (catalogue `_async`-identity law), never a parallel `AsyncStoreOp` family.
- Cases: the fence's factories and `_ROUTE` rows carry each obstore member; the decisions the fence cannot show —
  - `Put` `mode` ∈ `create|overwrite` or an `UpdateVersion` `PutMode` for the conditional write-once; multipart auto-selected when the payload exceeds `chunk_size`, the threshold an explicit request value defaulting to the 5-MiB `CHUNK` axis, never the provider default.
  - `GetRange` addresses one window as start+end or start+length — `end` and `length` are mutually exclusive descriptors on the one case.
  - `GetRanges` is the coalesced multi-chunk fast-path the `data/gridded/virtual` VirtualiZarr cube and `data/spatial/catalog` `AssetFold` read against archival HDF5/NetCDF/GeoTIFF byte ranges, `coalesce` merging adjacent windows below the 1-MiB gap default into one request, never a per-chunk round-trip.
  - `Get` carries the whole `GetOptions` precondition axis — `if_match`, `if_none_match`, time preconditions, ranges, versions, head policy — so a conditional re-read is a request value rather than a caller-side comparison after a full download.
  - `List` `offset` is the last-seen object-path cursor resuming lexicographically after that key — a `str | None`, never an integer page index; `delimiter` switches to `list_with_delimiter`'s flat `ListResult`.
  - `Delete` absorbs the singular and plural call over `str | Sequence[str]` on the one case.
  - `Sign` is valid only on the signing-capable backends, so a sign on a `local`/`memory`/`http` store refuses at the `_REFUSAL` matrix ahead of the provider rather than riding out as its `NotSupportedError`; `expires_in` is a `timedelta`, never an `int`.
  - the `reader`/`writer`/`sign` `read`s carry the obstore handle or URL batch on the outcome `payload` slot and emit a `None` `Source`, so no tag escapes the `_ROUTE` fold and no control-plane op claims operation bytes it never moved.
- Law: the reach matrix DERIVES off the row family's own capability columns rather than transcribing cells — a backend whose `signs` column is false lands its `sign` refusal automatically, so a seventh backend row arrives already gated and an unreachable cell answers its typed reason ahead of every provider call. An absent cell is reachable and rides its `_ROUTE` row; a cell no capability column answers lands as one explicit row beside the derivation.
- Law: retry is a per-row mutation-class disposition — reads, `put`, `delete`, `sign`, and the lazy `open_reader`/`open_writer` handle mints replay safely under `OBJECT_STORE` (a conditional-write collision stays the terminal `boundary` fault), while `copy` (a provider-side multi-step rewrite chain) and `rename` (copy-then-delete) cross the bare `boundary`/`async_boundary` fence unretried. Every `obstore.exceptions` leaf lands on the `boundary` catch-all (none is a `CLASSIFY` row), so recovery keys on `fault.recoverable({"boundary"})` and a conditional write-once or copy-once collision surfaces as a terminal `boundary` fault read off the lifted message, never a retry-suppressing per-arm catch.
- Entry: `run` and `run_async` open one `kind=SpanKind.CLIENT` span off the faults-owned `scoped` stamp, read one `_admitted` prologue, and return `RuntimeRail[StoreOutcome]` — reach answers a cell this backend cannot serve BEFORE the caller's gate fires and before any provider call, then the gate fires its own pre-flight points and may SETTLE the call outright. The awaited leg reads the prologue through a `match` closed by `assert_never` rather than a `bind`, so both entrypoints stay total over the carrier. `_apply` returns the outcome directly, so the fenced rail is single and no leg self-flattens a doubled carrier.
- Auto: `gate` is the caller's whole pre-flight policy as ONE pre-constructed value — a `StoreAdmission` answering DISPATCH or SETTLED, so a governance veto and a by-reference no-op both ride the composing tier's own semantics with no transport knob and no second prologue, and a consumer owning neither passes nothing. It rides the CALL rather than the lane because its closure carries per-call evidence (the caller's prior receipt) a lane field frozen at construction cannot hold, and it survives the knob test by carrying exactly what the op cannot reconstruct. A SETTLED outcome reports zero quantity, carries the caller's own `meta`, and keeps the operation bytes on its `source` slot, so the receipt the composing tier folds keys identically to the write it stands in for.
- Receipt: this owner mints none — `StoreOutcome` is payload-agnostic transport evidence (what moved, what the provider reported, the operation bytes a keyer digests, the handle a streaming caller holds), and content identity, quantity vocabulary, mutation veto, and reuse verdict are the composing tier's. That split is exactly where the tier boundary falls: transport here, receipt semantics at `data/tabular/egress#EGRESS`.
- Packages: `obstore` is the sole store provider; `SignCapableStore`/`HTTP_METHOD` are stub-only typing references (`obstore._sign.pyi` `TypeAlias`), so `Method` inlines the nine members rather than a runtime import, while the module-level `obstore.parse_scheme` IS bound — it answers the closed six-member backend classification off a store URL, which `from_url`'s own dispatch settles internally and never exposes — and `GetResult.stream`'s `BytesStream` is a typing-only chunk-iterator growth value the same `Get` row carries. `obstore.exceptions`' leaf taxonomy (`BaseError` root and the eleven leaves) and the `config`/`client_options`/`retry_config` `TypedDict` shapes are `libs/python/.api/obstore.md` catalog facts.
- Growth: a new store operation is one `StoreOp` case and one `_ROUTE` row carrying dispatch, argument planning, result projection, path, and retry class; a new precondition one `PutMode` on `Put` or one `overwrite` value on `Copy`/`Rename`; a newly unreachable cell one capability column on `StoreBackend` the matrix derives from, or one explicit `_REFUSAL` row where no column answers it; a new conditional-get axis one `GetOptions` key on `Get`; a new get-response evidence field one more `GetResult` member on the `Get` `read`'s `payload` tuple; a new streaming or signing surface one `StoreOp` case whose `read` carries its non-byte value on the `payload` slot and emits a `None` `Source`, never a parallel handle table.
- Boundary: object-store operation dispatch and its reach matrix only. Composes — never re-mints — the `reliability/resilience#RESILIENCE` `guarded`/`guarded_sync` envelopes and the `reliability/faults#FAULT` `BoundaryFault` those lift; mints no content key, quantity unit, hook point, or durable receipt. Rejected: a capability bound answered by a provider exception where a matrix row states it as data; a hand-opened `boundary` and span around a `guard`/`guard_sync` caller (the doubled-span/doubled-lift form `guarded`/`guarded_sync` fuse once); a sync leg dropping a retried row's `OBJECT_STORE` outer envelope to lean on the Rust-core `RetryConfig` where the async leg carries it; a narrowed `catch=obstore.exceptions.BaseError` letting a row-projection `KeyError`/`AttributeError` escape rather than convert at the `CLASSIFY` `Exception` seam; a per-operation `from_url` re-mint; a parallel `S3Lane`/`AsyncObjectStoreLane` family.

```python signature
import hashlib
import posixpath
from collections.abc import AsyncGenerator, AsyncIterator, Awaitable, Buffer, Callable, Generator, Iterable, Sequence
from contextlib import AbstractContextManager, AsyncExitStack, ExitStack
from datetime import timedelta
from enum import StrEnum
from functools import cache
from typing import TYPE_CHECKING, Any, Final, Literal, Never, Self, assert_never
from urllib.parse import urlsplit

from expression import Error, Nothing, Ok, Option, Result, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from pydantic import SecretStr

import anyio
import asyncssh
import httpx
import obstore
from anyio import CapacityLimiter
from hishel import AsyncSqliteStorage, BaseFilter, CacheOptions, CachePolicy, FilterPolicy, Request, SpecificationPolicy
from hishel.httpx import AsyncCacheTransport
from obstore import Bytes
from obstore.store import ObjectStore, from_url
from opentelemetry import trace
from opentelemetry.trace import SpanKind
from upath import UPath

from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary, boundary, scoped
from rasm.runtime.metrics import Metrics
from rasm.runtime.resilience import RetryClass, guarded, guarded_sync

if TYPE_CHECKING:
    from obstore import Attributes, GetOptions, ObjectMeta, PutMode, PutResult
    from obstore.store import AzureConfig, ClientConfig, GCSConfig, S3Config
    from opentelemetry.trace import Span

# --- [TYPES] ----------------------------------------------------------------------------

type Chunk = Bytes | bytes
type Acquired = Chunk | AsyncIterator[Chunk]
type Fetched = tuple["ResourceRef", "RuntimeRail[Acquired]"]
# both thunks take the acquisition span as a VALUE: the STREAM leg resumes under whichever context its consumer
# drives it from, so a span read ambiently inside a leg is the consumer's span and not the acquisition's.
type WholeFetch = Callable[["Span"], Awaitable[Chunk]]
type StreamOpen = Callable[["Span"], AsyncGenerator[Chunk]]
type Pool = tuple[str, str]

# `Backend` IS `obstore.parse_scheme`'s own closed return literal, so the classification and the row family's key
# column are one declaration every consumer imports rather than a second six-member roster to keep aligned.
type Backend = Literal["s3", "gcs", "azure", "http", "local", "memory"]
# `Arm` IS `TransportResource`'s own discriminant, so the union tag and the transport row family's key column are one
# declaration rather than a second two-member roster to keep aligned.
type Arm = Literal["http", "ssh"]
type Config = "S3Config | GCSConfig | AzureConfig"
type Provider = Callable[[], Any] | None
# `Method` mirrors the obstore `HTTP_METHOD` presign literal (nine members). `Source` is the operation-bytes axis a
# keying consumer digests: a byte-bearing op emits a zero-copy `Bytes`/`Buffer` whole or a `get_ranges` window tuple,
# and a control-plane op emits `None` so no consumer mints a key over a path string or a server-opaque `e_tag`.
type Method = Literal["GET", "PUT", "POST", "HEAD", "PATCH", "TRACE", "DELETE", "OPTIONS", "CONNECT"]
type Meta = "ObjectMeta | PutResult | None"
type Source = Buffer | Iterable[bytes] | None
type Call = tuple[tuple[Any, ...], dict[str, Any]]
type Read = tuple[int, Meta, Source, Any]
type StoreGate = Callable[["StoreOp", str], "RuntimeRail[StoreAdmission]"]


class Delivery(StrEnum):
    WHOLE = "whole"
    STREAM = "stream"


class CachePosture(StrEnum):
    # per-DESTINATION cache class, declared by the composition that admits the endpoint: `REVALIDATE` is the long-TTL
    # RFC-9111 class every catalog root and package index takes, `BODY_KEYED` the same class for a body-varying POST
    # search whose one URL the default request-identity key would alias into a single wrong entry, and `NEVER` the
    # presigned-URL and credential class no store may ever hold. One global freshness rule expresses none of the three.
    REVALIDATE = "revalidate"
    BODY_KEYED = "body_keyed"
    NEVER = "never"


class StoreRefusal(StrEnum):
    # every refused `(backend, op)` cell names its own reason and the member value IS the operator-facing evidence
    # `BoundaryFault` carries, exactly as the `data/tabular/lakehouse#LAKEHOUSE` `_REFUSAL` and
    # `data/tabular/query#QUERY` `_REMOTE_REFUSAL` matrices state theirs — reject law is data on every owner, so an
    # unreachable cell never rides out as a provider exception a caller cannot tell from a transport fault.
    SIGN_UNSUPPORTED = "presigning reaches the signing-capable backends alone; this scheme mints no signed url"


# --- [CONSTANTS] ------------------------------------------------------------------------


class StoreBackend(Struct, frozen=True, gc=False):
    # the branch's ONE store vocabulary: `backend` is `obstore.parse_scheme`'s own tag, `aliases` every URL scheme
    # resolving to it, `kvstore` the native key-value driver name a chunk-store consumer opens under it, `signs`
    # whether the backend mints presigned URLs, and `remote` whether a residence under it opens a STORE HANDLE at
    # all. Every roster below is a comprehension over this one table, so a seventh backend is ONE row and no consumer
    # is edited — the deleted forms are a hand-listed scheme frozenset, a re-declared backend literal, and a two-row
    # driver map living beside a prose comment at a downstream consumer.
    # Four further columns carry what a SELECTING reader needs and the mechanical ones never state: `fits` which
    # residence this row IS, `admit` the entry that puts bytes into it, `lifetime` who ends those bytes, and
    # `degrade` the honest concession taking it. Absent them every consumer re-derived a selection story out of
    # `signs`/`remote`, which is how `remote` reads as "signs" at one call site and as "is reachable" at another.
    # Tenancy is NOT a column here and never becomes one: a store backend isolates no tenant, the residence layout
    # above it does, and a backend answering tenancy answers it by guess for every residence riding the scheme.
    backend: Backend
    aliases: frozenset[str]
    kvstore: str | None
    signs: bool
    remote: bool
    fits: str
    admit: str
    lifetime: str
    # concessions no capability column expresses; the derived half rides `degrade` below.
    concedes: tuple[str, ...] = ()

    @property
    def degrade(self) -> tuple[str, ...]:
        # DERIVED first and stated second: every cell the reach matrix refuses for this backend reads back as the
        # capability it gives up, so flipping a capability column re-states the degradation with zero row edits, and
        # only what no column expresses is spelled by hand. That split is the sibling
        # `python:data/tabular/lakehouse#LAKEHOUSE` residence rows' own, so one reading serves both families.
        return (*(refusal.value for (backend, _op), refusal in _REFUSAL.items() if backend == self.backend), *self.concedes)


STORE_BACKENDS: Final[Block[StoreBackend]] = Block.of_seq([
    StoreBackend(
        "s3", frozenset({"s3", "s3a"}), "s3", True, True,
        fits="S3 and S3-compatible object residence — the branch's default remote plane and the only one every admitted engine addresses",
        admit="`ObjectStoreLane` over the `store_handle` fold, or the chunk engine's own `s3` kvstore driver",
        lifetime="the bucket's own policy; this branch deletes exactly what a `Delete` op names and expires nothing on its own",
        concedes=("copy and rename are provider-side multi-step rewrites, so both cross the rail unretried and an ambiguous failure is terminal",),
    ),
    StoreBackend(
        "gcs", frozenset({"gs", "gcs"}), "gcs", True, True,
        fits="Google Cloud Storage residence, reached identically to S3 through one handle fold",
        admit="`ObjectStoreLane` over the `store_handle` fold, or the chunk engine's own `gcs` kvstore driver",
        lifetime="the bucket's own policy; this branch deletes exactly what a `Delete` op names",
        concedes=("copy and rename cross the rail unretried on the same mutation-ambiguity law S3 takes",),
    ),
    # `kvstore=None` is a SOURCE-VERIFIED absence, never an unfilled column: the tensorstore kvstore root drivers are
    # exactly file/gcs/http/memory/s3/tsgrpc_kvstore, so obstore signs and reads Azure while a chunk-store consumer
    # refuses it BY NAME off this column rather than resolving a phantom driver mid-leg.
    StoreBackend(
        "azure", frozenset({"az", "abfs", "abfss", "azure"}), None, True, True,
        fits="Azure Blob and ADLS residence — a full object plane this branch signs and reads, yet no chunk-store engine opens",
        admit="`ObjectStoreLane` over the `store_handle` fold alone",
        lifetime="the container's own policy; this branch deletes exactly what a `Delete` op names",
        concedes=(
            "no tensorstore kvstore driver exists for it, so a dense chunk store refuses this residence BY NAME off the `kvstore` column",
            "copy and rename cross the rail unretried on the same mutation-ambiguity law S3 takes",
        ),
    ),
    # `http` is the one row whose two boolean columns diverge for a reason rather than by coincidence: an http(s)
    # residence is reachable, yet it is the `TransportResource` http arm — pooled, cached, proxied, status-hooked —
    # that reads it, so `remote=False` keeps `read` off a store handle that would answer the same bytes with none of
    # that policy. `local`/`memory` are process- or disk-local residences the filesystem arm and chunk engine serve.
    StoreBackend(
        "http", frozenset({"http", "https"}), "http", False, False,
        fits="a plain HTTP(S) artifact origin — a catalog root, a package index, a signed asset href — read, never written",
        admit="the `TransportResource` http arm, which is why `remote` is false: a store handle would answer the same bytes carrying none of the pool, cache, proxy, or status policy that arm holds",
        lifetime="the origin's own; this branch stores nothing here and its RFC-9111 cache entries expire on the destination's declared posture",
        concedes=(
            "read-only whole: no put, delete, copy, rename, or conditional write reaches an http origin through this branch at all",
            "an entry served from the keyed cache is the ORIGIN's freshness decision, so a read here is not a proof the origin still holds those bytes",
        ),
    ),
    StoreBackend(
        "local", frozenset({"file", ""}), "file", False, False,
        fits="an operator-owned filesystem path — a scratch root, a staged artifact tree, a local dataset under test",
        admit="the filesystem arm's `SCAN`-banded thread hop, or the chunk engine's own `file` kvstore driver",
        lifetime="the operator's; nothing in this branch reclaims a local path and a composition-admitted scratch root outlives every handle opened under it",
        concedes=(
            "no presigned URL and no cross-process write coordination: concurrency is the filesystem's own, which this branch never substitutes for",
        ),
    ),
    StoreBackend(
        "memory", frozenset({"memory"}), "memory", False, False,
        fits="a process-local residence for a fixture, a probe, or a fold that must never touch a disk",
        admit="the same `ObjectStoreLane` surface every residence takes, so a test crosses the identical reach, gate, retry, and receipt path",
        lifetime="THIS PROCESS — every byte dies with the interpreter, and no drain, groom, or delete is owed",
        concedes=(
            "durability whole: a write that succeeded here proves nothing survived, so no evidence, journal, or residence plane may name this backend",
            "no presigned URL and no reach past the process that wrote it",
        ),
    ),
])

# alias -> row: the scheme-shaped resolver every `ResourceRef.scheme` consumer reads, beside `obstore.parse_scheme`
# which stays the URL-shaped resolver — one vocabulary, two ingress shapes, never a second roster.
STORE_SCHEMES: Final[Map[str, StoreBackend]] = Map.of_seq((alias, row) for row in STORE_BACKENDS for alias in row.aliases)
# the remote object-store residence set, derived off the column that ASSERTS it: a scheme outside this set never
# opens a store handle. Deriving it off `signs` instead reads the same six rows today and is a coincidence, not a
# law — the first signing-incapable remote backend, or the first local backend that mints signed urls, silently
# re-routes every `read` in the branch with no row admitting it.
OBJECT_STORE_SCHEMES: Final[frozenset[str]] = frozenset(alias for row in STORE_BACKENDS if row.remote for alias in row.aliases)


class TransportArm(Struct, frozen=True, gc=False):
    # What a SELECTING reader needs off the two `TransportResource` cases, which the `(Endpoint, RetryClass)` tuple
    # never states: `fits` which origin this arm reaches, `admit` the entry that opens it, `lifetime` who ends the
    # bytes and whatever connection carried them, `degrade` the honest concession taking it. Both arms answer every
    # column with their OWN cell, because a pooled, cached, proxied HTTP dial and a per-call SFTP dial share no pool,
    # no cache, no egress vocabulary, and no session custody. Tenancy is NOT a column here and never becomes one, on
    # `StoreBackend`'s own reason: an arm isolates no tenant — the endpoint's confined root and its resolved
    # credential do — so an arm answering tenancy answers it by guess for every endpoint riding it.
    arm: Arm
    fits: str
    admit: str
    lifetime: str
    degrade: tuple[str, ...]


TRANSPORT_ARMS: Final[Map[Arm, TransportArm]] = Map.of_seq(
    (row.arm, row)
    for row in (
        TransportArm(
            "http",
            fits="a pooled, RFC-9111-cached, optionally proxied HTTP(S) origin — a catalog root, a package index, a signed asset href",
            admit="`TransportResource(http=(HttpEndpoint, RetryClass))`, whose `confined` gates the url ahead of any request and whose `pool` axes key the shared client",
            lifetime="the payload is the caller's the moment `Transfer.run` answers; the pooled client outlives every acquisition and ends only at `drain`, and a keyed cache entry expires on the ORIGIN's declared freshness",
            degrade=(
                "one client serves every destination sharing a `pool` key, so timeout, auth, and status policy are the POOL's and a per-call decision reaches only as far as the `RequestMetadata` extensions carry it",
                "an entry served from the keyed cache is the origin's freshness decision, so a read here is not a proof the origin still holds those bytes",
                "read-only whole: this arm carries no put, delete, or conditional-write member at all",
            ),
        ),
        TransportArm(
            "ssh",
            fits="an operator-owned SFTP residence behind a verified known-hosts entry — a fleet scratch tree or staged artifact path no object store fronts",
            admit="`TransportResource(ssh=(RemoteEndpoint, RetryClass))`, whose `confined` gates the POSIX path ahead of any dial and whose `dialed` is the branch's one asyncssh connection mint",
            lifetime="one connection per acquisition, closed by the leg's own `AsyncExitStack`; nothing pools and nothing outlives the call, so `SSH_KEEPALIVE` bounds only how fast a dead peer is observed",
            degrade=(
                "no pool and no cache: every acquisition re-dials, re-authenticates, and re-reads bytes the http arm would have served out of its keyed store",
                "no egress hop and no status vocabulary — a proxy is unspellable here and a refusal arrives as an asyncssh error the rail classifies, never as a code a caller branches on",
                "one host key database gates the whole arm, so a residence the admitted `known_hosts` does not carry is unreachable rather than reachable-unverified",
            ),
        ),
    )
)

STREAM_CHUNK: Final[int] = 1 << 20
LIST_CHUNK: Final[int] = 1000
COALESCE: Final[int] = 1 << 20
CHUNK: Final[int] = 5 << 20
FANOUT: Final[int] = 12

CONNECT_RETRIES: Final[int] = 2

SSH_CONNECT_TIMEOUT: Final[float] = 10.0
SSH_KEEPALIVE: Final[float] = 15.0  # idle-channel probe cadence: a NAT-dropped peer flips is_closed instead of lingering live
SSH_KEEPALIVE_MAX: Final[int] = 3

# filesystem blocking-read bound: every roots thread hop rides this explicit CapacityLimiter, never the ambient default limiter.
SCAN_BAND: Final[CapacityLimiter] = CapacityLimiter(8)
TRANSPORT_TIMEOUT: Final[httpx.Timeout] = httpx.Timeout(connect=5.0, read=30.0, write=30.0, pool=5.0)
TRANSPORT_LIMITS: Final[httpx.Limits] = httpx.Limits(max_connections=16, max_keepalive_connections=8)

# faults-owned scope stamp: `scoped` binds the version and semconv triple, so no page re-spells the pin.
_TRACER: Final = scoped(trace.get_tracer, "rasm.runtime.transport.roots")


class _Bypass(BaseFilter[Request]):
    # a `FilterPolicy` request filter answering False forwards to the origin and returns AHEAD of the storage lookup,
    # so one always-false filter IS the never-store posture: a presigned or credential endpoint neither reads nor
    # writes a row. An empty `supported_methods` also stores nothing but still pays the lookup, so it is the weaker form.
    def needs_body(self) -> bool:
        return False

    def apply(self, item: Request, body: bytes | None) -> bool:
        return False


# posture -> policy thunk: each client owns its own policy value, so a row's filter list is never shared mutable
# state across pools; `allow_stale` serves a stale entry when the origin is unreachable, and the POST method joins
# `supported_methods` only on the body-keyed row, where the body IS the cache key.
_CACHE_POLICY: Final[Map[CachePosture, Callable[[], CachePolicy]]] = Map.of_seq([
    (CachePosture.REVALIDATE, lambda: SpecificationPolicy(CacheOptions(shared=False, allow_stale=True))),
    (CachePosture.BODY_KEYED, lambda: SpecificationPolicy(CacheOptions(shared=False, allow_stale=True, supported_methods=["GET", "HEAD", "POST"]))),
    (CachePosture.NEVER, lambda: FilterPolicy(request_filters=[_Bypass()])),
])

# posture -> the `RequestMetadata` keys every request under it carries. The body-key is a PER-REQUEST lever the proxy
# reads beside the policy's own class flag (`policy.use_body_key or request.metadata["hishel_body_key"]`), so the
# posture arms it here rather than through a policy subclass that would body-key every GET sharing the client too;
# the policy row still widens `supported_methods`, because a POST is uncacheable before the key question arises.
# `hishel_ttl` is the destination's own eviction horizon, so a catalog root outlives a volatile index without a second
# keyed store, and the refresh flag makes a hot entry's TTL slide on access rather than expiring mid-fan-out.
_CACHE_EXTENSIONS: Final[Map[CachePosture, dict[str, object]]] = Map.of_seq([
    (CachePosture.REVALIDATE, {"hishel_refresh_ttl_on_access": True}),
    (CachePosture.BODY_KEYED, {"hishel_body_key": True, "hishel_refresh_ttl_on_access": True}),
    (CachePosture.NEVER, {}),
])

# the reached cache decision as the provider publishes it: hishel stamps its `ResponseMetadata` onto the httpx
# response extensions, so these three keys ARE the state machine's observable projection and header inference over
# `Cache-Control`/`Age` is the deleted form.
_CACHE_STATE: Final[Map[str, str]] = Map.of_seq([("hishel_from_cache", "served"), ("hishel_revalidated", "revalidated"), ("hishel_stored", "stored")])
# the fourth response key is a float, not a flag, so it rides its own attribute rather than the state fold: a served
# entry's own store timestamp is what turns "served" into an AGE a reader thresholds, and a request that reached the
# origin carries none — absence is the origin reading, never a zero age standing in for one.
_CACHE_STAMP: Final[str] = "hishel_created_at"


# --- [MODELS] ---------------------------------------------------------------------------


class TransferPlan(Struct, frozen=True):
    subject: str
    retry_class: RetryClass
    whole: WholeFetch
    stream: StreamOpen


class ResourceRef(Struct, frozen=True):
    # the residence coordinate WITH its credential: scheme, root, relative path, owner, and the obstore-native
    # provider that root's store binds. The provider rides the ref rather than each consumer's own field because it
    # already joins the store memo key as an axis — two refs differing only in provider ARE two residences, never one
    # a caller re-credentials at the call site — so a consumer holding a ref holds everything a handle needs and the
    # three spellings a per-consumer `provider`/`credentials` field grew fold onto this one column.
    scheme: str
    root: str
    relative: str
    owner: str
    credentials: Provider = None

    @classmethod
    def admit(cls, uri: str, owner: str, credentials: Provider = None) -> Self:
        # a whole-URL residence — a signed asset href, a manifest URL — admits in ONE hop: `root` IS the uri with
        # `relative` empty, so the handle addresses exactly the object the caller named and no consumer re-spells the
        # parse-and-project pair. A rooted store with interior paths admits at `ResourceRoot` and mints refs through
        # `child`, which alone carries the traversal gate; this hop guards nothing because it contains nothing.
        path = UPath(uri)
        return cls(scheme=path.protocol or "file", root=str(path), relative="", owner=owner, credentials=credentials)

    @property
    def path(self) -> UPath:
        return UPath(self.root, protocol=self.scheme) / self.relative


class Batch(Struct, frozen=True):
    # the plural acquisition's evidence: every requested ref paired with its OWN rail, in request order. A refused
    # ref carries its fault beside its siblings' payloads rather than collapsing the call, and a consumer wanting one
    # rail over the whole batch folds these through the faults-owned `combine` monoid, never a second aggregation minted here.
    owner: str
    delivery: Delivery
    fetched: Block[Fetched]


class HttpEndpoint(Struct, frozen=True):
    # the HTTP mirror of `RemoteEndpoint`: one destination identity carrying the credential, the declared cache
    # posture, the egress hop, and the composition-admitted scratch root the keyed store lands under — so cache
    # location, cache class, and egress policy are destination facts rather than global defaults or a hardcoded
    # provider path. An absent `proxy` IS direct egress, so one row per destination expresses the whole policy.
    url: str
    cache_root: str
    posture: CachePosture = CachePosture.REVALIDATE
    bearer: Option[SecretStr] = Nothing
    proxy: str | None = None
    proxy_auth: Option[SecretStr] = Nothing

    @property
    def key(self) -> str:
        # destination grouping, cache class, and egress hop as ONE non-secret identity. `_grouped` is what makes the
        # userinfo guard total: destination and proxy read the same projection, so the leak cannot hold on one and
        # lapse on the other. The posture joins so two declarations over one host pool distinctly instead of
        # colliding onto whichever landed first.
        return f"{_grouped(self.url)}:{self.posture}:{self.egress}"

    @property
    def egress(self) -> str:
        # the proxy arm is a KEY axis because its credential is baked into the `Proxy` the pooled transport holds
        # rather than passed per request: two callers proxying one hop under different credentials must be two
        # transports, or the second rides the first's identity. The secret joins as the truncated one-way digest
        # `RemoteEndpoint.key` uses — a stable non-secret fingerprint — and direct egress names itself.
        hop = Option.of_optional(self.proxy).map(_grouped).default_value("direct")
        secret = self.proxy_auth.map(lambda held: hashlib.sha256(held.get_secret_value().encode()).hexdigest()[:16]).default_value("-")
        return f"{hop}:{secret}"

    @property
    def cache(self) -> str:
        # the keyed store path derives from the admitted root and a digest of the destination key, so two destinations
        # never share rows and no path is spelled at a call site; the provider's CWD-relative default is the deleted form.
        return str(UPath(self.cache_root) / f"{hashlib.sha256(self.key.encode()).hexdigest()[:16]}.sqlite3")

    @property
    def pool(self) -> Pool:
        # the client memo axes as ONE hashable tuple of PLAIN strings: `key` already spans destination, posture, and
        # egress, and the admitted store path is the one axis it cannot derive. An `httpx.Proxy` value here would key
        # the memo by object identity and fork a pool per call site; a bearer or proxy secret here would key it by
        # secret. The mint reads the endpoint itself, so neither has to ride the key to reach the constructor.
        return (self.key, self.cache)

    def confined(self, relative: str, /) -> "RuntimeRail[str]":
        # the join is RFC 3986, which is exactly why the join alone gates nothing — `../`, a leading `/`, and a whole
        # absolute URL each REPLACE what they escape — so the resolved child reads back against the root on origin as
        # well as path. The root normalizes to a directory BEFORE the join, since RFC join otherwise drops the root's
        # last segment; an empty relative addresses the endpoint URL itself.
        base = httpx.URL(self.url if self.url.endswith("/") else f"{self.url}/")
        target = base.join(relative)
        inside = (target.scheme, target.host, target.port) == (base.scheme, base.host, base.port) and _segmented(base.path, target.path)
        return (
            Ok(self.url)
            if not relative
            else Ok(str(target))
            if inside
            else Error(BoundaryFault(resource=(self.key, f"traversal:{relative}")))
        )


class RemoteEndpoint(Struct, frozen=True):
    # one SSH channel identity for BOTH consumers — the SFTP legs below and the workers REMOTE exec arm; `python`
    # names the far interpreter the workers floor entry runs, and `known_hosts` is required so a disabled-verification
    # endpoint is unspellable by construction.
    host: str
    known_hosts: asyncssh.SSHKnownHosts
    port: int = 22
    python: str = "python3"
    password: Option[SecretStr] = Nothing
    root: str = "."  # the one remote root the SFTP legs may read under; "." confines to the session's landing directory

    @property
    def key(self) -> str:
        # arm identity spans the far interpreter AND the credential/trust posture — two endpoints on one host:port
        # whose `python`, confinement root, password custody, or trust store differ memoize as distinct pool arms
        # rather than colliding onto one channel dialed under the OTHER caller's settings. The password axis rides a
        # truncated one-way digest (a stable non-secret fingerprint, never the raw secret), and the parsed trust
        # store rides its process-local object identity — the boundaries memo-key law's id() axis for a foreign
        # value whose content no public surface recovers; distinct-content stores can only over-partition, never alias.
        secret = self.password.map(lambda held: hashlib.sha256(held.get_secret_value().encode()).hexdigest()[:16]).default_value("-")
        return f"{self.host}:{self.port}:{self.python}:{self.root}:{secret}:kh{id(self.known_hosts):x}"

    def confined(self, relative: str, /) -> "RuntimeRail[str]":
        # the request normalizes as a POSIX path, so only the root-joined confined path ever reaches an SFTP open and
        # a refusal costs no dial — never a dialed-then-denied read.
        normalized = posixpath.normpath(relative)
        escaped = relative.startswith("/") or normalized == ".." or normalized.startswith("../")
        return Error(BoundaryFault(resource=(self.key, f"traversal:{relative}"))) if escaped else Ok(posixpath.join(self.root, normalized))

    async def dialed(self) -> asyncssh.SSHClientConnection:
        # one asyncssh dial serves the whole branch; every consumer registers the returned connection on its own lifetime owner.
        return await asyncssh.connect(self.host, port=self.port, options=_ssh_options(self.password, self.known_hosts))


@tagged_union(frozen=True)
class StoreOp:
    tag: Literal["put", "get", "get_range", "get_ranges", "list", "head", "delete", "copy", "rename", "reader", "writer", "sign"] = tag()
    put: tuple[bytes, "PutMode", "Attributes", dict[str, str], int] = case()
    get: tuple[str, "GetOptions | None"] = case()
    get_range: tuple[str, int, int | None, int | None] = case()
    get_ranges: tuple[str, tuple[int, ...], tuple[int, ...] | None, tuple[int, ...] | None, int] = case()
    list: tuple[str, str | None, bool] = case()
    head: str = case()
    delete: str | Sequence[str] = case()
    copy: tuple[str, str, bool] = case()
    rename: tuple[str, str, bool] = case()
    reader: tuple[str, int | None, int | None] = case()
    writer: tuple[str, "Attributes", dict[str, str], int | None, int] = case()
    sign: tuple[Method, Sequence[str], timedelta] = case()

    @staticmethod
    def Put(
        payload: bytes,
        mode: "PutMode" = "overwrite",
        attributes: "Attributes | None" = None,
        tags: dict[str, str] | None = None,
        chunk_size: int = CHUNK,
    ) -> StoreOp:
        return StoreOp(put=(payload, mode, attributes or {}, tags or {}, chunk_size))

    @staticmethod
    def Get(path: str, options: "GetOptions | None" = None) -> StoreOp:
        return StoreOp(get=(path, options))

    @staticmethod
    def GetRange(path: str, start: int, end: int | None = None, length: int | None = None) -> StoreOp:
        return StoreOp(get_range=(path, start, end, length))

    @staticmethod
    def GetRanges(
        path: str, starts: tuple[int, ...], ends: tuple[int, ...] | None = None, lengths: tuple[int, ...] | None = None, coalesce: int = COALESCE
    ) -> StoreOp:
        return StoreOp(get_ranges=(path, starts, ends, lengths, coalesce))

    @staticmethod
    def List(prefix: str, offset: str | None = None, delimiter: bool = False) -> StoreOp:
        return StoreOp(list=(prefix, offset, delimiter))

    @staticmethod
    def Head(path: str) -> StoreOp:
        return StoreOp(head=path)

    @staticmethod
    def Delete(paths: str | Sequence[str]) -> StoreOp:
        return StoreOp(delete=paths)

    @staticmethod
    def Copy(source: str, target: str, overwrite: bool = True) -> StoreOp:
        return StoreOp(copy=(source, target, overwrite))

    @staticmethod
    def Rename(source: str, target: str, overwrite: bool = True) -> StoreOp:
        return StoreOp(rename=(source, target, overwrite))

    @staticmethod
    def Reader(path: str, buffer_size: int | None = None, size: int | None = None) -> StoreOp:
        return StoreOp(reader=(path, buffer_size, size))

    @staticmethod
    def Writer(
        path: str,
        attributes: "Attributes | None" = None,
        tags: dict[str, str] | None = None,
        buffer_size: int | None = None,
        max_concurrency: int = FANOUT,
    ) -> StoreOp:
        return StoreOp(writer=(path, attributes or {}, tags or {}, buffer_size, max_concurrency))

    @staticmethod
    def Sign(method: Method = "GET", paths: Sequence[str] = (), expires_in: timedelta = timedelta(hours=1)) -> StoreOp:
        return StoreOp(sign=(method, paths, expires_in))


class StoreOutcome(Struct, frozen=True):
    # payload-agnostic transport evidence: what the operation moved, what the provider reported, the operation bytes
    # a keying consumer digests, and the handle a streaming caller holds. It carries NO content key, quantity unit,
    # or reuse verdict — those are the composing tier's, which is exactly where the receipt boundary falls.
    operation: str
    path: str
    quantity: int
    meta: Meta
    source: Source
    payload: Any = None
    # the gate settled this call and no provider call ran; the composing tier reads it as its own by-reference verdict.
    settled: bool = False


@tagged_union(frozen=True)
class StoreAdmission:
    # the gate's two answers: DISPATCH the op at the provider, or SETTLE the call with an outcome no provider made —
    # a by-reference no-op the composing tier already proved against its own prior evidence. Reach refuses ahead of
    # the gate, so a settled call is always a reachable one.
    tag: Literal["dispatch", "settled"] = tag()
    dispatch: StoreOp = case()
    settled: StoreOutcome = case()


# the no-policy gate: a consumer owning no mutation semantics binds nothing and every op dispatches.
_ADMIT: Final[StoreGate] = lambda op, _target: Ok(StoreAdmission(dispatch=op))


class _StoreRow(Struct, frozen=True):
    sync: Callable[..., Any]
    aio: Callable[..., Any]
    plan: Callable[[StoreOp, str], Call]
    # `read`'s `Source` slot is operation bytes for a byte-bearing row, `None` for a control-plane row so no consumer
    # keys over a path string; its trailing slot is the handle or URL batch the outcome carries as `payload`.
    read: Callable[[StoreOp, str, Any], Read]
    path: Callable[[StoreOp, str], str]
    retry: RetryClass | None = RetryClass.OBJECT_STORE


# --- [OPERATIONS] -----------------------------------------------------------------------


def _grouped(url: str) -> str:
    # `scheme://host:port` off `hostname`/`port`, NEVER the raw `netloc`: netloc carries `user:password@` userinfo,
    # which keys a pool by secret rather than by destination. Every url a key axis reads crosses THIS fold.
    parsed = urlsplit(url)
    return f"{parsed.scheme}://{parsed.hostname}:{parsed.port}" if parsed.port else f"{parsed.scheme}://{parsed.hostname}"


def _segmented(base: str, child: str) -> bool:
    # `/root-evil/x` carries the `/root` string prefix and none of its segments, so a prefix test admits exactly the
    # sibling root this fold refuses.
    root = base.rstrip("/").split("/")
    return posixpath.normpath(child).split("/")[: len(root)] == root


class _BearerAuth(httpx.Auth):
    # `.get_secret_value()` runs exactly once, here at the transport seam; the bound `_header` is the only `str` the credential becomes.
    def __init__(self, token: SecretStr) -> None:
        self._header = f"Bearer {token.get_secret_value()}"

    def auth_flow(self, request: httpx.Request) -> Generator[httpx.Request, httpx.Response, None]:
        request.headers["Authorization"] = self._header
        yield request


def _auth(bearer: Option[SecretStr]) -> httpx.Auth | None:
    return bearer.map(_BearerAuth).to_optional()


def _proxy(url: str | None, secret: Option[SecretStr]) -> "httpx.Proxy | None":
    # the ONE `Proxy` mint and the proxy credential's ONE un-mask, seated beside `_auth` at the transport seam:
    # `user:password` splits into the `auth` pair here, so the secret never rides the `proxy` url the key digests and
    # the provider's own userinfo strip has nothing left to catch. An absent url IS direct egress.
    pair = secret.map(lambda held: held.get_secret_value().partition(":")).map(lambda split: (split[0], split[2])).to_optional()
    return None if url is None else httpx.Proxy(url, auth=pair)


class Transfer:
    @staticmethod
    async def run(plan: TransferPlan, delivery: Delivery) -> RuntimeRail[Acquired]:
        # ONE acquisition span per plan, opened HERE rather than inside a leg, because the STREAM arm's provider open
        # runs on a thunk the consumer drives later: a span read ambiently in that leg stamps the cache decision onto
        # whatever the consumer happens to hold — a fold, an index build, or nothing. The span therefore rides the
        # thunks as a value and attaches only on the WHOLE leg, whose frame runs straight through so the instrumented
        # origin call parents beneath it; `_Fenced` ends it for the STREAM leg at exhaustion or `aclose`.
        span = _TRACER.start_span(f"transport.{plan.subject}", kind=SpanKind.CLIENT)
        match delivery:
            case Delivery.STREAM:
                return Ok(_Fenced(plan.stream(span), span))
            case Delivery.WHOLE:
                with trace.use_span(span, end_on_exit=True):
                    return await guarded(plan.retry_class, lambda: plan.whole(span), subject=plan.subject)
            case _ as unreachable:
                assert_never(unreachable)


class _Fenced:
    # the ONE custody-and-span bracket every STREAM leg crosses, so neither fact is spelled at four legs. The guard
    # seats at `__anext__` AHEAD of the delegation because a leg's own generator frame cannot hold it: a second
    # concurrent pull re-enters that frame and the interpreter raises an untyped `RuntimeError` naming the frame, not
    # the provider session the two pulls interleave on. `ResourceGuard` detects OVERLAP rather than ownership, so a
    # sequential hand-off between tasks stays lawful and only a live second puller takes `BusyResourceError`.
    def __init__(self, chunks: AsyncGenerator[Chunk], span: "Span") -> None:
        self._chunks, self._span, self._guard = chunks, span, anyio.ResourceGuard(action="draining")

    def __aiter__(self) -> Self:
        return self

    async def __anext__(self) -> Chunk:
        # two-argument `anext` keeps exhaustion a VALUE, so the terminal span end and the protocol's own
        # `StopAsyncIteration` compose as one expression instead of an except arm wrapped around the drain.
        with self._guard:
            chunk = await anext(self._chunks, None)
        return chunk if chunk is not None else self._ended()

    async def aclose(self) -> None:
        # an abandoned stream releases the leg's exit stack and the span in ONE hop: `aclose` raises `GeneratorExit`
        # into the suspended `yield`, the stack unwinds in reverse, and no span outlives the acquisition it timed.
        await self._chunks.aclose()
        self._span.end()

    def _ended(self) -> Never:
        self._span.end()
        raise StopAsyncIteration


def _scanned(ref: ResourceRef) -> TransferPlan:
    # the filesystem plan and the `scan` band's registration are ONE hop, so the band opens on the first acquisition
    # that will actually borrow the limiter and a process reading only object stores publishes no always-zero level.
    _banded()
    return TransferPlan(
        ref.owner,
        RetryClass.SCAN,
        lambda _span: anyio.to_thread.run_sync(ref.path.read_bytes, abandon_on_cancel=True, limiter=SCAN_BAND),
        lambda _span: _file_chunks(ref),
    )


def _listing(store: ObjectStore, prefix: str, *, offset: str | None, delimiter: bool) -> int:
    # `list_with_delimiter(return_arrow=True)["objects"]` is one arro3 `Table`: its `num_rows` IS the
    # count, read directly (iterating a `Table` yields `ChunkedArray` COLUMNS, not row batches); the
    # recursive `list` yields `RecordBatch` chunks summed by `num_rows`.
    if delimiter:
        return obstore.list_with_delimiter(store, prefix, return_arrow=True)["objects"].num_rows
    return sum(batch.num_rows for batch in obstore.list(store, prefix, offset=offset, chunk_size=LIST_CHUNK, return_arrow=True))


async def _listing_async(store: ObjectStore, prefix: str, *, offset: str | None, delimiter: bool) -> int:
    if delimiter:
        return (await obstore.list_with_delimiter_async(store, prefix, return_arrow=True))["objects"].num_rows
    return sum([batch.num_rows async for batch in obstore.list(store, prefix, offset=offset, chunk_size=LIST_CHUNK, return_arrow=True)])


_ROUTE: Final[Map[str, _StoreRow]] = Map.of_seq([
    (
        "put",
        _StoreRow(
            obstore.put,
            obstore.put_async,
            lambda op, t: ((t, op.put[0]), {"mode": op.put[1], "attributes": op.put[2], "tags": op.put[3], "chunk_size": op.put[4]}),
            lambda op, t, result: (len(op.put[0]), result, op.put[0], None),
            lambda op, t: t,
        ),
    ),
    (
        "get",
        _StoreRow(
            obstore.get,
            obstore.get_async,
            lambda op, t: ((op.get[0],), {"options": op.get[1]}),
            lambda op, t, result: (len(body := result.bytes()), result.meta, body, (result.range, result.attributes)),
            lambda op, t: op.get[0],
        ),
    ),
    (
        "get_range",
        _StoreRow(
            obstore.get_range,
            obstore.get_range_async,
            lambda op, t: ((op.get_range[0],), {"start": op.get_range[1], "end": op.get_range[2], "length": op.get_range[3]}),
            lambda op, t, window: (len(window), None, window, None),
            lambda op, t: op.get_range[0],
        ),
    ),
    (
        "get_ranges",
        _StoreRow(
            obstore.get_ranges,
            obstore.get_ranges_async,
            lambda op, t: (
                (op.get_ranges[0],),
                {"starts": op.get_ranges[1], "ends": op.get_ranges[2], "lengths": op.get_ranges[3], "coalesce": op.get_ranges[4]},
            ),
            lambda op, t, windows: (sum(len(w) for w in windows), None, tuple(windows), None),
            lambda op, t: op.get_ranges[0],
        ),
    ),
    (
        "list",
        _StoreRow(
            _listing,
            _listing_async,
            lambda op, t: ((op.list[0],), {"offset": op.list[1], "delimiter": op.list[2]}),
            lambda op, t, rows: (rows, None, None, None),
            lambda op, t: op.list[0],
        ),
    ),
    (
        "head",
        _StoreRow(
            obstore.head,
            obstore.head_async,
            lambda op, t: ((op.head,), {}),
            lambda op, t, meta: (meta["size"], meta, None, None),
            lambda op, t: op.head,
        ),
    ),
    (
        "delete",
        _StoreRow(
            obstore.delete,
            obstore.delete_async,
            lambda op, t: ((op.delete,), {}),
            lambda op, t, _: (0, None, None, None),
            lambda op, t: op.delete if isinstance(op.delete, str) else ",".join(op.delete),
        ),
    ),
    (
        "copy",
        _StoreRow(
            obstore.copy,
            obstore.copy_async,
            lambda op, t: ((op.copy[0], op.copy[1]), {"overwrite": op.copy[2]}),
            lambda op, t, _: (0, None, None, None),
            lambda op, t: op.copy[1],
            retry=None,
        ),
    ),
    (
        "rename",
        _StoreRow(
            obstore.rename,
            obstore.rename_async,
            lambda op, t: ((op.rename[0], op.rename[1]), {"overwrite": op.rename[2]}),
            lambda op, t, _: (0, None, None, None),
            lambda op, t: op.rename[1],
            retry=None,
        ),
    ),
    (
        "reader",
        _StoreRow(
            obstore.open_reader,
            obstore.open_reader_async,
            lambda op, t: ((op.reader[0],), {k: v for k, v in (("buffer_size", op.reader[1]), ("size", op.reader[2])) if v is not None}),
            lambda op, t, file: (0, None, None, file),
            lambda op, t: op.reader[0],
        ),
    ),
    (
        "writer",
        _StoreRow(
            obstore.open_writer,
            obstore.open_writer_async,
            lambda op, t: (
                (op.writer[0],),
                {
                    k: v
                    for k, v in (
                        ("attributes", op.writer[1]),
                        ("tags", op.writer[2]),
                        ("buffer_size", op.writer[3]),
                        ("max_concurrency", op.writer[4]),
                    )
                    if v is not None
                },
            ),
            lambda op, t, file: (0, None, None, file),
            lambda op, t: op.writer[0],
        ),
    ),
    (
        "sign",
        _StoreRow(
            obstore.sign,
            obstore.sign_async,
            lambda op, t: ((op.sign[0], op.sign[1], op.sign[2]), {}),
            lambda op, t, urls: (len(op.sign[1]), None, None, urls),
            lambda op, t: op.sign[1][0] if op.sign[1] else "",
        ),
    ),
])


# `(backend, tag)` reach matrix DERIVED off the row family's own capability columns: a backend whose `signs` column is
# false lands its `sign` refusal without a transcribed cell, so a seventh row arrives already gated. An absent cell is
# reachable and rides its `_ROUTE` row; a cell no capability column answers lands as one explicit row beside this fold.
_REFUSAL: Final[Map[tuple[Backend, str], StoreRefusal]] = Map.of_seq(
    ((row.backend, "sign"), StoreRefusal.SIGN_UNSUPPORTED) for row in STORE_BACKENDS if not row.signs
)


def store_path(op: StoreOp, target: str) -> str:
    # the ONE path projection over the op axis: the outcome reads it and a gate building a pre-flight mutation fact
    # reads the same fold, so the destination a governance tap sees and the destination the receipt names cannot diverge.
    return _ROUTE[op.tag].path(op, target)


def _outcome(op: StoreOp, target: str, row: _StoreRow, returned: Any) -> StoreOutcome:
    quantity, meta, source, payload = row.read(op, target, returned)
    return StoreOutcome(operation=op.tag, path=store_path(op, target), quantity=quantity, meta=meta, source=source, payload=payload)


# --- [SERVICES] -------------------------------------------------------------------------


class ResourceRoot(Struct, frozen=True):
    scheme: str
    root: str
    owner: str
    # the obstore-native credential provider this root's store binds — `auth.planetary_computer` and `auth.earthdata`
    # own token refresh inside the handle, so a long fan-out re-signs transparently and no consumer tracks an expiry
    # it cannot renew. It joins the store memo key, so two roots differing only in provider never share a handle, and
    # `child` STAMPS it onto every ref this root mints so a consumer downstream of the traversal gate carries it too.
    credentials: Provider = None

    @classmethod
    def admit(cls, uri: str, owner: str, credentials: Provider = None) -> Self:
        path = UPath(uri)
        return cls(scheme=path.protocol or "file", root=str(path), owner=owner, credentials=credentials)

    def child(self, relative: str) -> RuntimeRail[ResourceRef]:
        # `resolve` on BOTH sides is what makes the compare honest: a symlink or `..` inside either path resolves
        # before containment reads it, so the gate answers on the location reached and not the location spelled.
        base = UPath(self.root)
        resolved = (base / relative).resolve()
        return (
            Ok(ResourceRef(self.scheme, self.root, relative, self.owner, self.credentials))
            if resolved.is_relative_to(base.resolve())
            else Error(BoundaryFault(resource=(self.owner, f"traversal:{relative}")))
        )

    async def read(
        self, source: ResourceRef | Block[ResourceRef], delivery: Delivery = Delivery.WHOLE, known: Option[str] = Nothing
    ) -> RuntimeRail[Acquired | Batch]:
        # ONE acquisition entry over both arities, discriminating on the VALUE's own shape: a `read_many` sibling
        # forks the plan, the delivery axis, and the validator thread at a second site for one concept. `known` is
        # the caller's PRIOR provider validator — the `e_tag` its last read reported — threaded as the `if_none_match`
        # precondition so an unchanged remote answers `NotModifiedError` the fence lifts instead of re-downloading a
        # payload the identity fold then discards. It is the origin's own opaque validator, never a branch
        # `ContentKey`: our digest names our bytes and no origin ever compares against it.
        match source:
            case Block() as refs:
                return await self._fan(refs, delivery, known)
            case ResourceRef() as ref:
                return await self._fetch(ref, delivery, known)
            case _ as unreachable:
                assert_never(unreachable)

    async def _fetch(self, ref: ResourceRef, delivery: Delivery, known: Option[str]) -> RuntimeRail[Acquired]:
        return await Transfer.run(self._plan(ref, known), delivery)

    async def _fan(self, refs: Block[ResourceRef], delivery: Delivery, known: Option[str]) -> RuntimeRail[Batch]:
        # the batch fans the SAME per-ref plan a single acquisition takes through one task group, so filesystem hops
        # still queue on `SCAN_BAND` and every store hop still crosses its own `OBJECT_STORE` envelope — the batch
        # buys concurrency and NOT a second bound. Each child answers with its own rail rather than raising, so a
        # refused ref cancels no sibling and the batch stays TOTAL: partial evidence is what a caller re-drives from,
        # where an all-or-nothing collapse discards every result the one failure never touched.
        async with anyio.create_task_group() as fan:
            handles = refs.map(lambda ref: fan.start_soon(self._fetch, ref, delivery, known))
        return Ok(Batch(owner=self.owner, delivery=delivery, fetched=refs.zip(handles.map(lambda held: held.return_value))))

    def _plan(self, ref: ResourceRef, known: Option[str]) -> TransferPlan:
        store = self._store(ref)
        options: "GetOptions | None" = known.map(lambda validator: {"if_none_match": validator}).to_optional()
        return (
            _scanned(ref)
            if store is None
            else TransferPlan(
                ref.owner,
                RetryClass.OBJECT_STORE,
                lambda _span: _obj_body(store, ref.relative, options),
                lambda _span: _obj_chunks(store, ref.relative, options),
            )
        )

    def _store(self, ref: ResourceRef) -> ObjectStore | None:
        # the REF's credential, never the root's: `child` stamps the root's provider onto every ref it mints, so the
        # two agree by construction, and reading the ref is what lets a caller holding one alone open the same handle.
        return _object_store(ref.root, ref.credentials) if ref.scheme in OBJECT_STORE_SCHEMES else None


class ObjectStoreLane(Struct, frozen=True):
    ref: ResourceRef
    store: ObjectStore
    # `backend` holds the provider's OWN closed classification, resolved once beside the handle `store_handle` builds
    # off the same root — the reach matrix keys on it, so no cell probes a store's runtime class or re-parses the URL.
    backend: Backend = "memory"

    @classmethod
    def of(cls, ref: ResourceRef, config: Config | None = None, client_options: "ClientConfig | None" = None) -> ObjectStoreLane:
        # ONE handle mints per lane, pinned to a single provider attempt, so every row — replay-safe or mutation-
        # ambiguous — is invoked exactly once inside whatever curve its `retry` column names, and the two-handle split
        # that existed only to cancel a nested provider schedule is gone. Credentials arrive on the REF and never as a
        # parameter beside it — a lane credentialed apart from its residence is two credential resolutions the one
        # memo key cannot serve, which is a second ref rather than a knob on this one.
        return cls(ref=ref, store=store_handle(ref, config=config, client_options=client_options), backend=obstore.parse_scheme(ref.root))

    def reached(self, op: StoreOp) -> RuntimeRail[StoreOp]:
        # capability bound as data: a cell this backend cannot serve answers its own reason ahead of every provider
        # call, so a `sign` against a local, memory, or http store never reaches the provider's `NotSupportedError`
        # and surfaces as an untyped boundary fault a caller cannot tell from a transport failure.
        return _REFUSAL.try_find((self.backend, op.tag)).map(lambda refusal: Error(BoundaryFault(boundary=(f"store.{op.tag}", refusal)))).default_value(Ok(op))

    def run(self, op: StoreOp, path: str = "", *, gate: StoreGate = _ADMIT) -> RuntimeRail[StoreOutcome]:
        target = path or self.ref.relative
        with self._span(op):
            return self._admitted(op, target, gate).bind(lambda admission: self._settled(admission, target))

    async def run_async(self, op: StoreOp, path: str = "", *, gate: StoreGate = _ADMIT) -> RuntimeRail[StoreOutcome]:
        target = path or self.ref.relative
        with self._span(op):
            # the prologue crosses a `match` rather than a `bind` because the fenced leg is awaited, and `assert_never`
            # keeps the arms total over the carrier — an unclosed two-arm match falls through returning `None` past the declared rail.
            match self._admitted(op, target, gate):
                case Result(tag="error", error=fault):
                    return Error(fault)
                case Result(tag="ok", ok=admission):
                    return await self._awaited(admission, target)
                case unreachable:
                    assert_never(unreachable)

    def _admitted(self, op: StoreOp, target: str, gate: StoreGate) -> RuntimeRail[StoreAdmission]:
        # ONE prologue both entrypoints read: reach answers an unreachable cell BEFORE the caller's gate fires and
        # before any provider call, then the gate fires its own pre-flight points and may settle the call outright.
        return self.reached(op).bind(lambda admitted: gate(admitted, target))

    def _settled(self, admission: StoreAdmission, target: str) -> RuntimeRail[StoreOutcome]:
        match admission:
            case StoreAdmission(tag="settled", settled=outcome):
                return Ok(outcome)
            case StoreAdmission(tag="dispatch", dispatch=op):
                retry = _ROUTE[op.tag].retry
                return (
                    guarded_sync(retry, self._apply, op, target, subject=f"store.{op.tag}")
                    if retry is not None
                    else boundary(f"store.{op.tag}", lambda: self._apply(op, target))
                )
            case unreachable:
                assert_never(unreachable)

    async def _awaited(self, admission: StoreAdmission, target: str) -> RuntimeRail[StoreOutcome]:
        match admission:
            case StoreAdmission(tag="settled", settled=outcome):
                return Ok(outcome)
            case StoreAdmission(tag="dispatch", dispatch=op):
                retry = _ROUTE[op.tag].retry
                return (
                    await guarded(retry, self._apply_async, op, target, subject=f"store.{op.tag}")
                    if retry is not None
                    else await async_boundary(f"store.{op.tag}", lambda: self._apply_async(op, target))
                )
            case unreachable:
                assert_never(unreachable)

    def _apply(self, op: StoreOp, target: str) -> StoreOutcome:
        row = _ROUTE[op.tag]
        args, kwargs = row.plan(op, target)
        return _outcome(op, target, row, row.sync(self.store, *args, **kwargs))

    async def _apply_async(self, op: StoreOp, target: str) -> StoreOutcome:
        row = _ROUTE[op.tag]
        args, kwargs = row.plan(op, target)
        return _outcome(op, target, row, await row.aio(self.store, *args, **kwargs))

    def _span(self, op: StoreOp) -> "AbstractContextManager[Span]":
        # kind=CLIENT: the store transport is the outbound network leg, a local/memory scheme its degenerate client.
        return _TRACER.start_as_current_span(
            f"store.{op.tag}", kind=SpanKind.CLIENT, attributes={"rasm.store.scheme": self.ref.scheme, "rasm.store.backend": self.backend}
        )


@tagged_union(frozen=True)
class TransportResource:
    tag: Arm = tag()
    http: tuple[HttpEndpoint, RetryClass] = case()
    ssh: tuple[RemoteEndpoint, RetryClass] = case()

    async def acquire(self, relative: str, delivery: Delivery = Delivery.WHOLE) -> RuntimeRail[Acquired]:
        match self:
            case TransportResource(tag="http", http=(endpoint, retry_class)):
                client, auth = _transport_client(endpoint), _auth(endpoint.bearer)
                # the posture's per-request cache levers ride the httpx extensions the proxy reads as `RequestMetadata`,
                # so body-keying and TTL refresh are decisions of THIS destination's requests rather than of every
                # request sharing the pooled client.
                carry = dict(_CACHE_EXTENSIONS[endpoint.posture])
                # `confined` gates before any request, exactly as the ssh arm gates before any dial: the http legs
                # receive only a url resolved under the endpoint's own root.
                planned = endpoint.confined(relative).map(
                    lambda url: TransferPlan(
                        "http",
                        retry_class,
                        lambda span: _http_body(client, url, auth, carry, span),
                        lambda span: _http_chunks(client, url, auth, carry, span),
                    )
                )
            case TransportResource(tag="ssh", ssh=(endpoint, retry_class)):
                # `confined` gates before any dial: the SFTP legs receive only the root-joined validated path.
                planned = endpoint.confined(relative).map(
                    lambda confined: TransferPlan(
                        "ssh", retry_class, lambda _span: _sftp_read(endpoint, confined), lambda _span: _sftp_chunks(endpoint, confined)
                    )
                )
            case _ as unreachable:
                assert_never(unreachable)
        match planned:
            case Result(tag="ok", ok=plan):
                return await Transfer.run(plan, delivery)
            case Result(tag="error") as refused:
                return refused
            case _ as unreachable:
                assert_never(unreachable)


# --- [COMPOSITION] ----------------------------------------------------------------------


async def _raise_for_status(response: httpx.Response) -> None:
    response.raise_for_status()


def _cached(response: httpx.Response, span: "Span") -> None:
    # the reached cache state stamped on the acquisition span its own plan opened, read off the extensions the cache
    # proxy publishes: an absent key set means the request reached the origin, so "origin" is a measured reading
    # rather than a filled zero.
    span.set_attribute("rasm.transport.cache", ",".join(name for key, name in _CACHE_STATE.items() if response.extensions.get(key)) or "origin")
    # the stored-at stamp lands ONLY where the provider published one — an origin response carries no entry age, and a
    # zero there would read to a freshness board as a row stored at the epoch rather than as a row that never existed.
    match response.extensions.get(_CACHE_STAMP):
        case float() | int() as stored_at:
            span.set_attribute("rasm.transport.cache.stored_at", float(stored_at))
        case _:
            pass


def store_handle(
    source: ResourceRef | str,
    *,
    config: Config | None = None,
    client_options: "ClientConfig | None" = None,
    provider: Provider = None,
) -> ObjectStore:
    # ONE `from_url` spelling in the branch, and ONE handle shape: every store this page opens pins the Rust core to a
    # single attempt, so `RetryClass` owns the whole curve and no operation is invoked under two schedules whose
    # effective attempt count is their PRODUCT. `obstore.store.RetryConfig` is a TYPE_CHECKING TypedDict rather than a
    # runtime constructor, so the pin is the inlined literal. A caller-supplied `retry_config` is the deleted knob and
    # the zero-retry twin handle it forced is deleted with it — a `retry=None` row was already invoked exactly once by
    # this pin. `ResourceRef` opens off its own two columns — root and credentials — so `provider=` admits only beside
    # a bare string root: the two-value join re-spells at the call site the custody the ref already carries, and one
    # handle opened from two values is the form the ref column exists to delete.
    root, bound = (source.root, source.credentials) if isinstance(source, ResourceRef) else (source, provider)
    return from_url(root, config=config, client_options=client_options, retry_config={"max_retries": 0}, credential_provider=bound)


@cache
def _object_store(root: str, provider: Provider = None) -> ObjectStore:
    # the provider is the second memo axis and rides its own object identity — a live credential handle whose content
    # no public surface recovers, the boundaries memo-key law's id() case. Distinct instances can only over-partition.
    return store_handle(root, provider=provider)


_CLIENTS: Final[dict[Pool, httpx.AsyncClient]] = {}
_PROBES: Final[ExitStack] = ExitStack()


@cache
def _banded() -> None:
    # the `scan` band's registration, exactly once per process: `Metrics.occupied` keys on the probe OBJECT, so a
    # second registration of one module-level probe adds an entry the single retire cannot tell from the first — the
    # memo IS that once-guard, and `SCAN_BAND` is module-scope with no lifetime a bare `with` could bracket instead.
    _PROBES.enter_context(Metrics.occupied(lambda: SCAN_BAND.borrowed_tokens, band="scan"))


def _transport_client(endpoint: HttpEndpoint) -> httpx.AsyncClient:
    # ONE memo keyed by the endpoint's plain-string `pool`, and the map IS the drainable handle — `functools.cache`
    # surrenders no value enumeration to `drain`, which is why the pair it forced (a memo beside a live set) folds
    # onto this one map. The membership guard precedes the mint, since a `setdefault` builds a client per call and
    # discards every one but the first. The memo rides the TRANSPORT seam, so this long-lived client keeps its
    # timeout, auth, and status hook while `AsyncCacheTransport` wraps the pooled `AsyncHTTPTransport` beneath it.
    if endpoint.pool not in _CLIENTS:
        _CLIENTS[endpoint.pool] = httpx.AsyncClient(
            timeout=TRANSPORT_TIMEOUT,
            transport=AsyncCacheTransport(
                # egress seats HERE, on the transport that actually carries the request, and NEVER as
                # `AsyncClient(proxy=)`: a client-level proxy mounts a FRESH bare `AsyncHTTPTransport` per url
                # pattern that `_transport_for_url` returns AHEAD of this explicit `transport=`, so every proxied
                # request would bypass the cache and the connect retries both. That same explicit `transport=` also
                # sets `allow_env_proxies=False`, so `HTTP_PROXY`/`HTTPS_PROXY` reach nothing and this row is the
                # whole egress policy — a `None` proxy is direct egress, one row per destination.
                next_transport=httpx.AsyncHTTPTransport(
                    proxy=_proxy(endpoint.proxy, endpoint.proxy_auth), retries=CONNECT_RETRIES, http2=True, limits=TRANSPORT_LIMITS
                ),
                storage=AsyncSqliteStorage(database_path=endpoint.cache),
                policy=_CACHE_POLICY[endpoint.posture](),
            ),
            event_hooks={"response": [_raise_for_status]},
        )
    return _CLIENTS[endpoint.pool]


async def drain() -> None:
    # `aclose` every pooled client so no pool reaches the GC. ONE close is total: the client closes its transport and
    # the cache transport closes both the wrapped pool AND its keyed storage, so a second `storage.close()` beside it
    # is the doubled teardown. The Rust `ObjectStore` has no async pool, so its cache only clears, and the `scan`
    # band retires with the composition rather than lingering as a level whose limiter no live owner borrows.
    clients = tuple(_CLIENTS.values())
    _CLIENTS.clear()
    async with anyio.create_task_group() as tg:
        for client in clients:
            tg.start_soon(client.aclose)
    _PROBES.close()
    _banded.cache_clear()
    _object_store.cache_clear()


# --- [BOUNDARIES] -----------------------------------------------------------------------
# Per-provider WHOLE/STREAM legs the `[SERVICES]` `TransferPlan` thunks bind by call-time name.


async def _obj_body(store: ObjectStore, relative: str, options: "GetOptions | None") -> Bytes:
    return (await obstore.get_async(store, relative, options=options)).bytes()


async def _obj_chunks(store: ObjectStore, relative: str, options: "GetOptions | None") -> AsyncGenerator[Bytes]:
    result = await obstore.get_async(store, relative, options=options)
    async for chunk in result.stream(STREAM_CHUNK):
        yield chunk


async def _file_chunks(ref: ResourceRef) -> AsyncGenerator[bytes]:
    # what this frame owns and an abandoned thread would strand: the handle no other closer holds, and the `finally`
    # close an in-flight read would race.
    handle = await anyio.to_thread.run_sync(ref.path.open, "rb", limiter=SCAN_BAND)
    try:
        while block := await anyio.to_thread.run_sync(handle.read, STREAM_CHUNK, limiter=SCAN_BAND):
            yield block
    finally:
        handle.close()


async def _http_body(client: httpx.AsyncClient, url: str, auth: httpx.Auth | None, extensions: dict[str, object], span: "Span") -> bytes:
    response = await client.get(url, auth=auth, extensions=extensions)
    _cached(response, span)
    return response.content


async def _http_chunks(
    client: httpx.AsyncClient, url: str, auth: httpx.Auth | None, extensions: dict[str, object], span: "Span"
) -> AsyncGenerator[bytes]:
    async with AsyncExitStack() as stack:
        response = await stack.enter_async_context(client.stream("GET", url, auth=auth, extensions=extensions))
        _cached(response, span)
        async for chunk in response.aiter_bytes():
            yield chunk


def _ssh_options(password: Option[SecretStr], known_hosts: asyncssh.SSHKnownHosts) -> asyncssh.SSHClientConnectionOptions:
    # one connection-config value object over per-call `connect(...)` keyword soup; the password un-masks only here, and
    # `known_hosts` is the admission-supplied verified database, never the disabled `None`. An absent password defers to
    # asyncssh's default key/agent resolution; keepalive keeps the workers arm's `is_closed` liveness read honest between submits.
    return asyncssh.SSHClientConnectionOptions(
        password=password.map(SecretStr.get_secret_value).default_value(None),
        known_hosts=known_hosts,
        connect_timeout=SSH_CONNECT_TIMEOUT,
        keepalive_interval=SSH_KEEPALIVE,
        keepalive_count_max=SSH_KEEPALIVE_MAX,
    )


async def _sftp_session(stack: AsyncExitStack, endpoint: RemoteEndpoint, relative: str) -> asyncssh.SFTPClientFile:
    conn = await stack.enter_async_context(await endpoint.dialed())
    sftp = await stack.enter_async_context(conn.start_sftp_client())
    return await stack.enter_async_context(await sftp.open(relative, "rb"))


async def _sftp_read(endpoint: RemoteEndpoint, relative: str) -> bytes:
    async with AsyncExitStack() as stack:
        return await (await _sftp_session(stack, endpoint, relative)).read()


async def _sftp_chunks(endpoint: RemoteEndpoint, relative: str) -> AsyncGenerator[bytes]:
    async with AsyncExitStack() as stack:
        handle = await _sftp_session(stack, endpoint, relative)
        while block := await handle.read(STREAM_CHUNK):
            yield block
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
