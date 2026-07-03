# [PY_DATA_CATALOG]

The cloud-native STAC discovery owner: one `StacCatalog` over `pystac-client` that resolves WHICH cloud assets cover a query region, the discovery layer the geospatial and object-store egress lanes lack above the raster/vector claims (`spatial/geospatial.md#GEO`) and the archival byte-window read (`tabular/egress.md#EGRESS`). `StacCatalog.discover` folds the one `StacQuery` tagged-union axis — bbox/intersects/datetime/ids/collection/CQL2-filter/CQL2-query/order/free-text discriminants — onto the single keyword-only `pystac_client.Client.search`, with the `Surface` discriminant alone routing the same shared rows to `Client.collection_search` when a `FreeText` case is present, pages the lazy `ItemSearch` into the `pystac.ItemCollection`, and emits one `StacDiscovery` outcome keyed by runtime `ContentIdentity` over the matched item-id set plus the `ItemSearch.matched` count. One `Signing` value generalizes the request boundary — the `headers` map plus the `SignScheme` `StrEnum` whose `SchemeRow` carries the named `open_kwargs`/`sign`/`patch_url` behavior callables, threaded once into `Client.open(modifier=)` for discovery, once over the `ItemSearch` for the asset hrefs, and once into `odc.stac.load(patch_url=)` for the COG cube load — three reads of one frozen policy row by name, never three parallel `match` statements, never a positional integer-index read, never a signed-vs-unsigned client pair, and never a bare forwarded callable. The `Surface` discriminant's `materialize` row owns the divergence between the two result iterators, so the `ITEM` surface signs the `ItemSearch` and reads `matched()` while the `COLLECTION` surface materializes collections without invoking the item-only `sign`/`matched` members. The discovered collection encodes as a `stac-geoparquet` columnar Arrow `RecordBatchReader` (`[3]-[TABLE]`) the `tabular/columnar.md#SCAN` scan and the `tabular/query.md#QUERY` engine consume, and the asset hrefs fold (`[4]-[ASSETS]`) into the `tabular/egress.md#EGRESS` `ObjectEgress.GetRange` archival-byte fast-path, the `gridded/virtual.md#VIRTUAL` `VirtualReference(sources, ref).apply(VersionOp.aggregate(...))` virtual-reference registration over the `gridded/field#VIRTUAL` `FieldVirtual` manifest, the `odc-stac` `odc.stac.load` COG datacube driven by the catalogue-derived `stac_cfg`/`patch_url`, and the `proj:epsg`/`raster:bands`/`eo:cloud_cover` extension read-back into one `RasterGeoClaim` — reading those settled fence contracts, never a second object-store transport, virtual-cube builder, COG loader, or hand-fed band metadata. Every bundle keys by exactly one runtime `ContentIdentity`.

## [01]-[INDEX]

- [01]-[CATALOG]: the `StacCatalog` discovery owner over `pystac-client` `Client.open`/`search`, the `StacQuery` tagged-union search axis with the `Surface` discriminant routing `search`/`collection_search`, its `SurfaceRow.accepts` keyword-admission set filtering the per-surface parameter rows and its `SurfaceRow.materialize` policy owning the sign-and-count divergence, the `Signing` request-boundary value whose `SignScheme` `SchemeRow` carries the named `open_kwargs`/`sign`/`patch_url` callables, `ItemSearch` paging into `pystac.ItemCollection`, the one `StacDiscovery` outcome carrying its `surface` terminal discriminant.
- [02]-[TABLE]: the discovered item collection encoded as a `stac-geoparquet` columnar Arrow `RecordBatchReader` the `tabular/columnar` scan and `tabular/query` engine consume, with the GeoParquet/NDJSON/Delta sink rows split across `stac_table_egress` (table-in writes) and `stac_table_direct` (one-call source-to-disk fast-paths).
- [03]-[ASSETS]: the one awaitable `AssetFold` over the signed `ItemCollection` discriminating asset role by `MediaType` — the `tabular/egress` `ObjectEgress.GetRange` byte-window read, the `gridded/virtual` `VirtualReference.apply(VersionOp.aggregate(...))` virtual-chunk registration over the `gridded/field` `FieldVirtual` manifest, the `odc-stac` `odc.stac.load` COG datacube under the `guarded(RetryClass.HTTP, ...)` envelope driven by the `pystac.extensions`-derived `RasterGeoClaim`/`stac_cfg`/`patch_url`, the one railed `StacDiscovery` re-key.

## [02]-[CATALOG]

- Owner: `StacCatalog` — the one cloud-native discovery owner, a `Client.open`-bound STAC API root carrying one `Signing` request-boundary value; `StacQuery` the tagged-union search axis (bbox/intersects/datetime/ids/collection/cql2-filter/cql2-query/order/free-text), folded by `match`/`case` closed by `assert_never` onto the single keyword-only `Client.search(*, method=, max_items=, limit=, ids=, collections=, bbox=, intersects=, datetime=, query=, filter=, filter_lang=, sortby=, fields=)`. A new search modality is one `StacQuery` case, never a `search_bbox`/`search_intersects`/`search_cql2`/`search_collections` method family — bbox vs intersects vs datetime vs ids vs collection vs CQL2 filter vs ordering vs free-text are parameter rows on the one `search`, the exact discrimination `pystac-client.md` L64 mandates.
- Cases: `StacQuery` rows `Bbox(west, south, east, north)` (spatial selection through `search(bbox=(w,s,e,n))`) · `Intersects(geometry)` (polygon-region selection through `search(intersects=<geojson-dict>)`, the GeoJSON-geometry `dict[str, object]` the server intersects server-side, no shapely at the boundary) · `Datetime(start, end)` (temporal selection through `search(datetime="<start>/<end>")` the RFC-3339 interval) · `Ids(*item_ids)` (direct item selection through `search(ids=[...])`, `pystac-client.md` L40) · `Collection(*ids)` (collection-scoped discovery through `search(collections=[...])`) · `Cql2Filter(predicate)` (the CQL2-JSON predicate through `search(filter=..., filter_lang="cql2-json")` — eo:cloud_cover/gsd/proj:epsg property predicates the STAC API evaluates server-side) · `Cql2Query(query)` (the legacy `query` extension predicate through `search(query=...)` the same conformance class admits) · `Order(sortby, fields)` (server-side ordering and field projection through `search(sortby=[...], fields={"include": [...], "exclude": [...]})`) · `FreeText(q)` (free-text discovery whose presence flips the `Surface` discriminant from `ITEM` to `COLLECTION`, routing the whole union to `Client.collection_search(q=..., **shared)` rather than `search`), each carrying its `params()` projection that contributes exactly its own keyword arguments so an n-axis query unions the per-case keyword dicts rather than forking a method per axis combination — and because `collection_search` shares the `bbox`/`datetime`/`query`/`filter`/`filter_lang`/`sortby`/`fields` axis with `search` (`pystac-client.md` L43), the same `Bbox`/`Datetime`/`Cql2Filter`/`Order` rows union onto either surface unchanged, the surface alone differing.
- Surface: `Surface` is the discovery-method discriminant carrying its own routing policy — `ITEM`/`COLLECTION` whose `.row` property projects the one frozen `SurfaceRow` `(method, cap, accepts, materialize)`, the bound `pystac-client` method name (`search`/`collection_search`), that method's total-cap keyword (`max_items`/`max_collections`, the divergence `pystac-client.md` L40/L43 names), the `accepts` frozen keyword-admission set naming exactly the parameter rows the bound method signature admits (`_SHARED` ∪ `{ids, collections, intersects}` for `search`, `_SHARED` ∪ `{q}` for `collection_search`, the `pystac-client.md` L40/L43 signatures), and the per-surface `materialize` policy that owns the full `StacDiscovery`-shaping projection — the materialized carrier, the id tuple, the matched count, the href count, and the expiry horizon. `row.call(client, max_items, limit, params)` reads the method, cap, and the admissible-keyword filter off one row rather than a `cap = "max_collections" if ... else "max_items"` ternary fork plus a blind `**params` splat, so an `ids`/`intersects`/`collections` row that a `FreeText` union folds in never reaches `collection_search` (which rejects it), the `accepts` set the one boundary that keeps the union law total across surfaces; `row.materialize(search, signing)` reads the entire shaped outcome off the same row rather than a `signing.sign(search) if surface is ITEM else ...` branch followed by `getattr`-probed `.assets`/`.properties` derivation. The `materialize` row is load-bearing because the two iterators diverge structurally: the `ITEM` row signs the `ItemSearch` through `signing.sign(search)`, counts via `ItemSearch.matched()`, and reads each item's `.assets`/`msft:expiry`, while the `COLLECTION` row never signs (a `CollectionSearch` carries no Azure-asset hrefs and `planetary_computer.sign` registers no `CollectionSearch` overload), reads each collection's `.id`, and yields `href_count=0`/`expiry=None` because a STAC `Collection` carries no assets or token expiry, so routing the union to `collection_search` never calls the item-only `sign`/`matched`/`.assets` members. `Surface.of_queries(queries)` recovers `COLLECTION` exactly when a `FreeText` case is present, the one boolean-free routing read, threaded onto `StacDiscovery.surface` so a `COLLECTION`-surface discovery is a typed terminal the `[4]-[ASSETS]` `AssetFold.over` rejects rather than iterating a collection set as an asset source.
- Signing: `Signing` is the one request-boundary value carrying its own behavior — `headers` the bearer/SAS request headers a cloud endpoint mandates, `timeout` the `Client.open(timeout=)` request-deadline seconds (`pystac-client.md` L39), and `scheme` the `SignScheme` `StrEnum` row (`NONE`/`PLANETARY_COMPUTER`) whose `.row` projects the one frozen `SchemeRow` carrying the named `open_kwargs`/`sign`/`patch_url` behavior callables, so the value encodes the boundary as a frozen policy row read by name in one hop rather than a positional triple indexed by integer, three parallel full-coverage `match` statements, or a forwarded bare callable. `Signing.open_kwargs()` reads `scheme.row.open_kwargs` (`headers=`/`timeout=`, plus `modifier=planetary_computer.sign_inplace` the in-place `modifier=` callable on the `PLANETARY_COMPUTER` row, `planetary-computer.md` L40), `Signing.sign(search)` reads `scheme.row.sign` running `planetary_computer.sign` `singledispatch` over the lazy `ItemSearch` handle itself (the registered `ItemSearch` overload, `planetary-computer.md` L44/L64, returning a fresh signed `ItemCollection` in one materialization, idempotent over already-signed and non-Azure hrefs), so the sign and the page-materialization fuse into one pass rather than materializing then re-signing, and `Signing.patch_url()` reads `scheme.row.patch_url` projecting the `odc.stac.load(patch_url=)` callable; `Signing.none` is the unsigned default, `Signing.of_headers(headers=, timeout=)` the header-only row, `Signing.planetary_computer(subscription_key=, headers=, timeout=)` resolves `set_subscription_key` function-local then selects the `PLANETARY_COMPUTER` row. The same `Signing` threads thrice — `Client.open(**signing.open_kwargs())` for discovery, `signing.sign(search)` over the `ItemSearch` handle the `ITEM` `materialize` row drives, and `signing.patch_url()` into `odc.stac.load` (`[4]-[ASSETS]`) — three reads of one `SchemeRow`, never a forked signed-vs-unsigned client and never a second SAS token owner.
- Entry: `StacCatalog.open` admits a STAC API root URL plus one `Signing`; the awaitable `StacCatalog.discover` first computes the pure plan — folds the query tuple's `params()` projections into one keyword set through `reduce(... | q.params(), queries, {})` and recovers `Surface.of_queries(queries)` and its `.row` — then delegates the whole blocking network sequence to the runtime `guarded(RetryClass.HTTP, anyio.to_thread.run_sync, _discover, subject="stac.discover")` envelope, the one fused consumer entry that drives the member-cached `stamina` bound caller around the offloaded `_discover` inside one `resilience.guarded` retry span and lifts the terminal raise through the faults owner's `async_boundary` exactly once. The synchronous `pystac_client` I/O (the conformance-negotiating `Client.open`, the lazy `/search` page request, and the page materialization) rides `anyio.to_thread.run_sync` so the blocking calls never stall the event loop, and the `HTTP` row retries the transient `TimeoutError`/`ConnectionError`/`429`/`5xx` set under the runtime backoff honouring a server-directed `Retry-After` as one logical discovery rather than three separately-retried legs — the corpus `spatial/geospatial.md#GEO` `apply_remote` and `execution/admission#SETTINGS` `_probe` remote-sync-under-retry idiom, never a hand-opened `boundary(..., catch=APIError)` re-spelling the retry/span/lift triplet the `guarded` envelope already fuses. The `row.call` inside `_discover` reads the method name, the total-cap keyword, and the per-surface `accepts` keyword filter off the one `SurfaceRow` so a cross-surface param never reaches a method that rejects it, and `row.materialize(search, self.signing)` returns the full `(collection, item_ids, matched, href_count, expiry, url)` shaped outcome — the `ITEM` row signs the lazy `ItemSearch` handle (the one `planetary_computer.sign(ItemSearch)` dispatch both materializes the page and rewrites every `*.blob.core.windows.net` href in a single pass), reports `ItemSearch.matched()` and `ItemSearch.url_with_parameters()` the resolved-GET request URL (`pystac-client.md` L68), and reads each item's `.assets`/`msft:expiry`, while the `COLLECTION` row materializes the collections without signing and yields a zero href count, no expiry, and a `None` url, sizing each page request with `limit`. The returned `RuntimeRail[Materialized]` flattens through one `.bind(self._shape(surface))` where `_shape` destructures the row tuple, threads the recovered `surface` discriminant, and folds the railed `ContentIdentity.of("stac.discover", ...)` over the matched item-id set through `.map` into the one `StacDiscovery`, so the identity-derivation failure path shares the discovery rail rather than stuffing a `RuntimeRail[ContentKey]` into the `content_key: ContentKey` field — the return a `RuntimeRail[StacDiscovery]` carrying the surface, the materialized carrier, the id tuple, the row-reported matched total, the href count, the earliest `msft:expiry` token-validity horizon, the resolved request `url`, and the `ContentKey`. The runtime `ResourceRef` carries the asset-byte path (`[4]-[ASSETS]`) the `ObjectEgress.GetRange` egress speaks, never re-minted as a second STAC API credential owner.
- Auto: the `StacQuery.params` fold is the union law — each case spreads to its own keyword dict (`Bbox` to `{"bbox": ...}`, `Intersects` to `{"intersects": ...}`, `Datetime` to `{"datetime": ...}`, `Ids` to `{"ids": ...}`, `Collection` to `{"collections": ...}`, `Cql2Filter` to `{"filter": ..., "filter_lang": "cql2-json"}`, `Cql2Query` to `{"query": ...}`, `Order` to `{"sortby": ..., "fields": ...}`, `FreeText` to `{"q": ...}`) and a multi-axis discovery merges the spreads into one call through `reduce`, so a bbox+datetime+cloud-cover+order query is one search, never four; `Surface.of_queries` flips the union to `collection_search` exactly when a `FreeText` case is present because free-text collection discovery is a `CollectionSearch` not an `ItemSearch`, the `SurfaceRow` supplying the method name, its `max_collections` cap, and the non-signing `materialize` policy with no boolean fork and no `search_by_<axis>` family; `ItemSearch` is lazy so the `ITEM` `materialize` row's `matched()` reads the API-reported total without materializing every page, and `planetary_computer.sign` over the lazy `ItemSearch` is the one canonical materialization-plus-sign (never the deprecated `get_all_items`, never a hand-rolled `next`-link follow loop the `ItemSearch` paging already owns, never a materialize-then-re-sign two-pass); the signed `ItemCollection` carries the signed hrefs the `[4]-[ASSETS]` fold reads without re-signing, and `min(item.properties["msft:expiry"])` over the signed items reports the token-validity horizon as catalog-signing evidence (`planetary-computer.md` L70); the network search rides the runtime resilience rail — one `guarded(RetryClass.HTTP, anyio.to_thread.run_sync, _discover, subject="stac.discover")` call funnels the whole blocking `Client.open`/`row.call`/`row.materialize` discovery sequence through the offload thread under the cached `stamina` bound caller and lifts the terminal raise through the faults owner's `async_boundary` exactly once, so a transient `429`/`5xx`/`TimeoutError`/`ConnectionError` retries under the `HTTP` `Retry-After`-honouring backoff and a budget-exhausted transient surfaces as the `boundary` case naming the final cause, never a hand-rolled retry loop, never a hand-opened `boundary`/`async_boundary` fence re-spelling the retry/span/lift triplet the `guarded` envelope fuses, and never three separately-retried legs where one envelope retries the discovery atomically; the SAS token fetch the signing step drives carries its own SDK-internal retry (`planetary-computer.md` L69), so the page-materialization sign is not double-wrapped; `pystac-client`/`pystac`/`planetary-computer` import function-local under `# noqa: PLC0415` per the manifest boundary-scope-only import policy (`pystac-client.md` L62, `pystac.md` L72, `planetary-computer.md` L63), while the internal-runtime `RetryClass`/`guarded` and `anyio` ride the module-level import.
- Receipt: `StacCatalog.discover` produces one `StacDiscovery` — the one outcome owner carrying the signed `ItemCollection`, the matched item-id tuple, the `matched()` count, the href count, the `msft:expiry` token horizon, the `url_with_parameters()` resolved-GET URL, and the `ContentKey` — whose `contribute()` yields the emitted-phase `Receipt.of("catalog", ("emitted", endpoint, facts))` satisfying the runtime `ReceiptContributor.contribute -> Iterable[Receipt]` Protocol (the two-argument `(owner, evidence)` factory over the `(phase, subject, facts)` tuple, never a four-positional shape), the facts carrying the native `items`/`matched`/`hrefs` counts the receipts `Encoder(enc_hook=repr)` serializes without a `str()` coerce, never a single bare `Receipt` against the `Iterable[Receipt]` Protocol and never a parallel result-versus-receipt pair.
- Packages: `pystac-client` (`Client.open(url, headers=, parameters=, modifier=, request_modifier=, stac_io=, timeout=)`, the one keyword-only `Client.search(*, method=, max_items=, limit=, ids=, collections=, bbox=, intersects=, datetime=, query=, filter=, filter_lang=, sortby=, fields=)` returning `ItemSearch`, `Client.collection_search(*, max_collections=, limit=, q=, query=, filter=, filter_lang=, sortby=, fields=)` returning `CollectionSearch`, `ItemSearch.{item_collection,items,pages,matched,url_with_parameters}` the item paging plus resolved-GET-URL receipt the `ITEM` `materialize` row reads, `CollectionSearch.collection_list()` the collection-materialization member the `COLLECTION` `materialize` row reads (`pystac-client.md` L69)), `pystac` (`ItemCollection`/`Item`/`Asset` the search results hydrate into, the `Item.properties["msft:expiry"]` token horizon), `planetary-computer` (`sign` the `singledispatch` dispatching the lazy `ItemSearch` handle directly, `sign_inplace` the `modifier=` callable, `set_subscription_key` for the in-process key), runtime (`RuntimeRail`/`ContentIdentity`/`ContentKey`/`ReceiptContributor`/`Receipt`/`RetryClass`/`guarded` the one fused retry-span-plus-`async_boundary` envelope the `HTTP` discovery sequence rides over `anyio.to_thread.run_sync`; the `ResourceRef` rides the `[4]-[ASSETS]` asset-byte fold, not the API search), `anyio` (`to_thread.run_sync` offloading the blocking `pystac_client` I/O off the event loop under the `guarded` envelope).
- Growth: a new search modality is one `StacQuery` case with one `params()` projection plus its key on the owning surface's `accepts` set; a new ordering field is a `sortby` entry on the existing `Order`; a new auth scheme is one `SignScheme` member plus one `_SCHEME` `SchemeRow` carrying its named `open_kwargs`/`sign`/`patch_url` callables, never three new `match` arms; a new discovery surface is one `Surface` member plus one `_SURFACE` `(method, cap, accepts, materialize)` row whose `accepts` set names the method's admissible keywords; free-text collection discovery already lands as the `FreeText` case flipping the `Surface` discriminant; zero new surface.
- Boundary: composes the runtime `ResourceRef`/`TransportResource` for the API credentials, never a second credential owner; the discovery owner folds INTO the `tabular/egress`/`gridded/virtual`/`tabular/columnar`/`spatial/geospatial`/`odc-stac` owners and never re-mints a STAC API paging loop, a CQL2 compiler, a SAS token fetch, a conformance negotiator `pystac-client`/`planetary-computer` already owns, or a retry/backoff loop the runtime `RetryClass.HTTP` rail owns; no live UI, no durable catalog store; a hand-opened `boundary`/`async_boundary` fence around the network search re-spelling the retry/span/terminal-lift triplet the runtime `guarded` envelope fuses once, a synchronous `guard(RetryClass.HTTP)(...)` drive of the awaitable bound caller, a blocking `pystac_client` call left on the event loop where `anyio.to_thread.run_sync` offloads it, a hand-rolled retry loop or manual `sleep` backoff around the transient `429`/`5xx` HTTP fault where `guarded(RetryClass.HTTP, ...)` honours the server-directed `Retry-After`, a raw `RuntimeRail[ContentKey]` stuffed into the `content_key: ContentKey` field where `_shape` folds the railed `ContentIdentity.of` through `.bind`/`.map`, a four-positional `Receipt.of(phase, owner, subject, facts)` against the two-argument `(owner, evidence)` form, a `str()`-pre-formatted numeric receipt fact where the receipts `Encoder(enc_hook=repr)` serializes the native scalar, a `search_by_<axis>` method family, a `cap`-keyword ternary fork where the `SurfaceRow` carries the keyword name, a blind `**params` splat onto `collection_search` where the `SurfaceRow.accepts` set filters the cross-surface keyword the method signature rejects, a `signing.sign(search) if surface is ITEM else ...` materialization branch where the `SurfaceRow.materialize` policy routes, a `planetary_computer.sign(CollectionSearch)`/`CollectionSearch.matched()` call where the surface carries no such member, a `routes_collection_search` boolean fork where the `Surface` discriminant routes, a positional `_SCHEME[scheme][0]` integer-index read where the named `SchemeRow` field resolves in one hop, three parallel `SignScheme` `match` statements where one `SchemeRow` carries the callables, a materialize-then-re-sign two-pass where the one `sign(ItemSearch)` dispatch fuses, a hand-rolled `next`-link page loop, the deprecated `get_all_items`, a hand-stitched SAS query-string concatenation, a bare forwarded `modifier` callable where `Signing` owns the strategy, a parallel result-versus-receipt struct pair, a signed-vs-unsigned client pair, an `AssetFold` iterating a `COLLECTION`-surface discovery as an asset source where the typed `surface` terminal rejects it, and a parallel API-vs-file client where `Client` extends `pystac.Catalog` are the deleted forms.

```python signature
from builtins import frozendict
from collections.abc import Callable
from enum import StrEnum
from functools import cache, reduce
from types import ModuleType
from typing import TYPE_CHECKING, Final, Literal, assert_never

import anyio
from expression import case, tag, tagged_union
from msgspec import Struct, field

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.receipts import Receipt
from rasm.runtime.resilience import RetryClass, guarded

if TYPE_CHECKING:
    from collections.abc import Iterable

    from pystac import Collection, ItemCollection
    from pystac_client import ItemSearch

type Bound = tuple[float, float, float, float]
type Geometry = dict[str, object]
type Predicate = dict[str, object]
type Fields = dict[str, tuple[str, ...]]
type SearchParams = dict[str, object]
type Headers = dict[str, str]
type Modifier = Callable[[object], object]
type OpenKwargs = dict[str, object]
# first slot: the `ITEM` row's signed `ItemCollection`, or the `COLLECTION` row's `list[Collection]`.
type Materialized = tuple["ItemCollection | list[Collection]", tuple[str, ...], int, int, str | None, str | None]


# the boundary-scoped `planetary_computer` handle, paid once per process and read by every
# `SchemeRow` callable — the `@cache` lazy-module idiom the `gridded/store#STORE` `_ts_context`
# singleton rides, never a per-call re-import behind an `# noqa: E704` compound one-liner.
@cache
def _pc() -> ModuleType:
    import planetary_computer  # noqa: PLC0415

    return planetary_computer


class SchemeRow(Struct, frozen=True):
    open_kwargs: Callable[[Headers, float | None], OpenKwargs]
    sign: Callable[["ItemSearch"], "ItemCollection"]
    patch_url: Callable[[], Modifier | None]


class SignScheme(StrEnum):
    NONE = "none"
    PLANETARY_COMPUTER = "planetary_computer"

    @property
    def row(self) -> SchemeRow:
        return _SCHEME[self]


_SCHEME: Final[frozendict[SignScheme, SchemeRow]] = frozendict({
    SignScheme.NONE: SchemeRow(
        open_kwargs=lambda headers, timeout: {"headers": headers, "timeout": timeout},
        sign=lambda search: search.item_collection(),
        patch_url=lambda: None,
    ),
    SignScheme.PLANETARY_COMPUTER: SchemeRow(
        open_kwargs=lambda headers, timeout: {"headers": headers, "timeout": timeout, "modifier": _pc().sign_inplace},
        sign=lambda search: _pc().sign(search),
        patch_url=lambda: _pc().sign,
    ),
})


class Signing(Struct, frozen=True):
    headers: Headers = field(default_factory=dict)
    timeout: float | None = None
    scheme: SignScheme = SignScheme.NONE

    @staticmethod
    def none() -> "Signing":
        return Signing()

    @staticmethod
    def of_headers(headers: Headers, timeout: float | None = None) -> "Signing":
        return Signing(headers=headers, timeout=timeout)

    @staticmethod
    def planetary_computer(subscription_key: str | None = None, headers: Headers | None = None, timeout: float | None = None) -> "Signing":
        if subscription_key is not None:
            _pc().set_subscription_key(subscription_key)
        return Signing(headers=headers or {}, timeout=timeout, scheme=SignScheme.PLANETARY_COMPUTER)

    def open_kwargs(self) -> OpenKwargs:
        return self.scheme.row.open_kwargs(self.headers, self.timeout)

    def sign(self, search: "ItemSearch") -> "ItemCollection":
        return self.scheme.row.sign(search)

    def patch_url(self) -> Modifier | None:
        return self.scheme.row.patch_url()


class SurfaceRow(Struct, frozen=True):
    method: str
    cap: str
    accepts: frozenset[str]
    materialize: Callable[[object, Signing], Materialized]

    def call(self, client: object, cap_value: int | None, limit: int | None, params: SearchParams) -> object:
        plan = {self.cap: cap_value, "limit": limit, **{k: v for k, v in params.items() if k in self.accepts}}
        return getattr(client, self.method)(**plan)


def _materialize_items(search: object, signing: Signing) -> Materialized:
    collection = signing.sign(search)
    item_ids = tuple(item.id for item in collection)
    href_count = sum(len(item.assets) for item in collection)
    expiry = min((e for item in collection if (e := item.properties.get("msft:expiry"))), default=None)
    matched = reported if (reported := search.matched()) is not None else len(item_ids)
    return collection, item_ids, matched, href_count, expiry, search.url_with_parameters()


def _materialize_collections(search: object, _: Signing) -> Materialized:
    collections = search.collection_list()
    return collections, tuple(c.id for c in collections), len(collections), 0, None, None


class Surface(StrEnum):
    ITEM = "item"
    COLLECTION = "collection"

    @property
    def row(self) -> SurfaceRow:
        return _SURFACE[self]

    @staticmethod
    def of_queries(queries: "tuple[StacQuery, ...]") -> "Surface":
        return Surface.COLLECTION if any(q.tag == "free_text" for q in queries) else Surface.ITEM


_SHARED: Final[frozenset[str]] = frozenset({"bbox", "datetime", "query", "filter", "filter_lang", "sortby", "fields"})

_SURFACE: Final[frozendict[Surface, SurfaceRow]] = frozendict({
    Surface.ITEM: SurfaceRow(
        method="search", cap="max_items", accepts=_SHARED | {"ids", "collections", "intersects"}, materialize=_materialize_items
    ),
    Surface.COLLECTION: SurfaceRow(method="collection_search", cap="max_collections", accepts=_SHARED | {"q"}, materialize=_materialize_collections),
})


@tagged_union(frozen=True)
class StacQuery:
    tag: Literal["bbox", "intersects", "datetime", "ids", "collection", "cql2_filter", "cql2_query", "order", "free_text"] = tag()
    bbox: Bound = case()
    intersects: Geometry = case()
    datetime: tuple[str, str] = case()
    ids: tuple[str, ...] = case()
    collection: tuple[str, ...] = case()
    cql2_filter: Predicate = case()
    cql2_query: Predicate = case()
    order: tuple[tuple[str, ...], Fields] = case()
    free_text: str = case()

    @staticmethod
    def Bbox(west: float, south: float, east: float, north: float) -> "StacQuery":
        return StacQuery(bbox=(west, south, east, north))

    @staticmethod
    def Intersects(geometry: Geometry) -> "StacQuery":
        return StacQuery(intersects=geometry)

    @staticmethod
    def Datetime(start: str, end: str) -> "StacQuery":
        return StacQuery(datetime=(start, end))

    @staticmethod
    def Ids(*item_ids: str) -> "StacQuery":
        return StacQuery(ids=item_ids)

    @staticmethod
    def Collection(*ids: str) -> "StacQuery":
        return StacQuery(collection=ids)

    @staticmethod
    def Cql2Filter(predicate: Predicate) -> "StacQuery":
        return StacQuery(cql2_filter=predicate)

    @staticmethod
    def Cql2Query(query: Predicate) -> "StacQuery":
        return StacQuery(cql2_query=query)

    @staticmethod
    def Order(sortby: tuple[str, ...], include: tuple[str, ...] = (), exclude: tuple[str, ...] = ()) -> "StacQuery":
        return StacQuery(order=(sortby, {"include": include, "exclude": exclude}))

    @staticmethod
    def FreeText(q: str) -> "StacQuery":
        return StacQuery(free_text=q)

    def params(self) -> SearchParams:
        match self:
            case StacQuery(tag="bbox", bbox=extent):
                return {"bbox": list(extent)}
            case StacQuery(tag="intersects", intersects=geometry):
                return {"intersects": geometry}
            case StacQuery(tag="datetime", datetime=(start, end)):
                return {"datetime": f"{start}/{end}"}
            case StacQuery(tag="ids", ids=item_ids):
                return {"ids": list(item_ids)}
            case StacQuery(tag="collection", collection=ids):
                return {"collections": list(ids)}
            case StacQuery(tag="cql2_filter", cql2_filter=predicate):
                return {"filter": predicate, "filter_lang": "cql2-json"}
            case StacQuery(tag="cql2_query", cql2_query=query):
                return {"query": query}
            case StacQuery(tag="order", order=(sortby, fields)):
                return {"sortby": list(sortby), "fields": {axis: list(names) for axis, names in fields.items()}}
            case StacQuery(tag="free_text", free_text=q):
                return {"q": q}
            case unreachable:
                assert_never(unreachable)


class StacDiscovery(Struct, frozen=True):
    endpoint: str
    surface: Surface
    collection: "ItemCollection | list[Collection]"
    item_ids: tuple[str, ...]
    matched: int
    href_count: int
    expiry: str | None
    url: str | None
    content_key: ContentKey

    def contribute(self) -> "Iterable[Receipt]":
        yield Receipt.of(
            "catalog",
            (
                "emitted",
                self.endpoint,
                {
                    "surface": self.surface.value,
                    "items": len(self.item_ids),
                    "matched": self.matched,
                    "hrefs": self.href_count,
                    "expiry": self.expiry or "none",
                    "url": self.url or self.endpoint,
                },
            ),
        )


class StacCatalog(Struct, frozen=True):
    endpoint: str
    signing: Signing = field(default_factory=Signing.none)

    @classmethod
    def open(cls, endpoint: str, signing: Signing | None = None) -> "StacCatalog":
        return cls(endpoint=endpoint, signing=signing or Signing.none())

    async def discover(self, *queries: StacQuery, max_items: int | None = None, limit: int | None = None) -> "RuntimeRail[StacDiscovery]":
        params = reduce(lambda acc, q: acc | q.params(), queries, {})
        surface = Surface.of_queries(queries)
        row = surface.row
        return (
            await guarded(RetryClass.HTTP, anyio.to_thread.run_sync, lambda: self._discover(row, params, max_items, limit), subject="stac.discover")
        ).bind(self._shape(surface))

    def _discover(self, row: SurfaceRow, params: SearchParams, max_items: int | None, limit: int | None) -> Materialized:
        from pystac_client import Client  # noqa: PLC0415

        client = Client.open(self.endpoint, **self.signing.open_kwargs())
        return row.materialize(row.call(client, max_items, limit, params), self.signing)

    def _shape(self, surface: Surface) -> "Callable[[Materialized], RuntimeRail[StacDiscovery]]":
        def shape(materialized: Materialized) -> "RuntimeRail[StacDiscovery]":
            collection, item_ids, matched, href_count, expiry, url = materialized
            return ContentIdentity.of("stac.discover", "\n".join(item_ids).encode()).map(
                lambda key: StacDiscovery(
                    endpoint=self.endpoint,
                    surface=surface,
                    collection=collection,
                    item_ids=item_ids,
                    matched=matched,
                    href_count=href_count,
                    expiry=expiry,
                    url=url,
                    content_key=key,
                )
            )

        return shape
```

## [03]-[TABLE]

- Owner: the `stac_table` encoder — the discovered `pystac.ItemCollection` to a `stac-geoparquet` columnar Arrow `pyarrow.RecordBatchReader`, the one carrier the `tabular/columnar.md#SCAN` scan and the `tabular/query.md#QUERY` engine consume. Encoding is the `stac_geoparquet.arrow.parse_stac_items_to_arrow` parse over the in-memory items; the NDJSON source is the sibling `parse_stac_ndjson_to_arrow` over the same carrier; GeoParquet egress over a materialized table is `stac_geoparquet.arrow.to_parquet` keyed by a `schema_version` from `SUPPORTED_PARQUET_SCHEMA_VERSIONS`; the one-call source-to-disk fast-paths are `parse_stac_items_to_parquet` (items), `parse_stac_ndjson_to_parquet` (NDJSON file), and `parse_stac_ndjson_to_delta_lake` (Delta sink); rehydration is `stac_geoparquet.arrow.stac_table_to_items` paired with `pystac.Item.from_dict`, and the NDJSON egress is `stac_table_to_ndjson` — never a hand-built Arrow schema where the `ACCEPTED_SCHEMA_OPTIONS` inference applies, never a hand-rolled parquet writer where `to_parquet` versions the schema, never a materialize-then-write two-hop where the one-call `stac_table_direct` path writes straight to disk, never the legacy geopandas trio where the zero-copy Arrow path is the canonical carrier.
- Cases: the source axis is one `TableSource` tagged union — `Items(items)` parses the in-memory `pystac.Item` iterable through `parse_stac_items_to_arrow` and `Ndjson(path, limit)` parses a STAC-NDJSON file through `parse_stac_ndjson_to_arrow`, both landing on the one `RecordBatchReader`, never two parallel table engines; the schema axis is the `ACCEPTED_SCHEMA_OPTIONS` string literal — `"FullFile"` scans every batch for the widest schema (the discovery default, correctness over a heterogeneous multi-collection result) and `"FirstBatch"` infers from the first batch (the lower-latency single-collection and direct-write fast-path) — a parameter row on the parse, not a parallel parse function; the sink axis is one `TableSink` tagged union — `Parquet(output_path, schema_version)` versions GeoParquet, `NdjsonOut(dest)` writes STAC-NDJSON, `DeltaLake(table_or_uri)` appends the Delta sink — each a single output descriptor, never a per-sink writer family. The sink rows route through two entrypoints keyed by whether an Arrow table is already in hand: `stac_table_egress` owns the table-in writes (`to_parquet`/`stac_table_to_ndjson` over the materialized `RecordBatchReader`/`Table`), while `stac_table_direct` owns the one-call source-to-disk fast-paths (`parse_stac_items_to_parquet`, `parse_stac_ndjson_to_parquet`, `parse_stac_ndjson_to_delta_lake`) that never materialize an intermediate reader — the Delta sink lives only on `stac_table_direct` because `parse_stac_ndjson_to_delta_lake` reads a STAC-NDJSON file rather than an Arrow table (`stac-geoparquet.md` L45), so its `table`-in arm on `stac_table_egress` is a typed reject rather than a silently-table-dropping write.
- Entry: `stac_table` folds one `TableSource` through `match`/`case` (with `chunk_size=DEFAULT_JSON_CHUNK_SIZE` and `schema="FullFile"`) into the `RecordBatchReader` and returns a `RuntimeRail[pa.RecordBatchReader]` the columnar scan registers directly; `stac_table_egress` folds the Arrow-table-in `TableSink` rows (`parquet`/`ndjson_out`, defaulting `schema_version` to `DEFAULT_PARQUET_SCHEMA_VERSION`) over an Arrow table to the path the `tabular/egress` `ObjectEgress.Put` then uploads, the `delta_lake` arm a typed reject routing to `stac_table_direct`, each write arm returning the railed `ContentIdentity.of(...)` (already a `RuntimeRail[ContentKey]`) directly so the one `boundary("stac.table.egress", _write).bind(lambda rail: rail)` self-flatten threads the identity-derivation fault through the single carrier rather than swallowing it inside an `Ok`; `stac_table_direct` folds the `(TableSource, TableSink)` pair through one closed `match` over the catalogued one-call paths — `(items, parquet)` to `parse_stac_items_to_parquet`, `(ndjson, parquet)` to `parse_stac_ndjson_to_parquet`, `(ndjson, delta_lake)` to `parse_stac_ndjson_to_delta_lake` — with the `case _, _` arm a typed reject for a pair with no one-call surface, the discovery-to-disk fast-path that never materializes an intermediate reader; `stac_table_rehydrate` runs `stac_table_to_items(table)` over a read-back STAC table and rebuilds the model with `pystac.Item.from_dict`, never re-minting a STAC item the `pystac` owner already models.
- Auto: the item table crosses as the zero-copy `pyarrow.RecordBatchReader` so the columnar scan consumes it without a Python-side copy; `parse_stac_items_to_arrow` accepts the `pystac.Item` iterable directly (no NDJSON round-trip), and `to_parquet` stamps the GeoParquet STAC schema version so a downstream reader resolves the column layout without a side-channel; `stac_table_direct` collapses parse-and-write into one call for the discovery-to-disk fast-path where the intermediate reader is never scanned — `parse_stac_items_to_parquet` for the in-memory items, `parse_stac_ndjson_to_parquet` for the NDJSON file, both defaulting `schema="FirstBatch"` for the lower-latency direct write — so a discovery written straight to GeoParquet never pays the materialize-then-write two-hop the `stac_table`→`stac_table_egress` path costs; `stac-geoparquet` and `pystac` import function-local under `# noqa: PLC0415` per the boundary-scope-only import policy (`stac-geoparquet.md` L63), the parse/write the boundary the `RuntimeRail` traps; the `geopandas`-backed `to_geodataframe`/`to_item_collection` trio stays a legacy fallback never called here — the `arrow.*` namespace is the pure-Python carrier and is the one the page binds.
- Receipt: the table encode folds the shared `tabular/columnar` `QueryReceipt` (`tabular/columnar.md` `QueryReceipt`) over the encoded Arrow table keyed by `ContentIdentity`, the same receipt the columnar scan and spatial engine emit, never a parallel table-receipt rail.
- Packages: `stac-geoparquet` (`arrow.parse_stac_items_to_arrow(items, chunk_size=, schema=, tmpdir=)` returning `pa.RecordBatchReader`, `arrow.parse_stac_ndjson_to_arrow(path, *, chunk_size=, schema=, limit=)`, `arrow.parse_stac_items_to_parquet(items, *, chunk_size=, schema=, output_path=, schema_version=)`, `arrow.parse_stac_ndjson_to_parquet(input_path, output_path, *, chunk_size=, schema=, limit=, schema_version=)`, `arrow.to_parquet(table, output_path, *, schema_version=)`, `arrow.stac_table_to_items(table)`, `arrow.stac_table_to_ndjson(table, dest)`, `arrow.parse_stac_ndjson_to_delta_lake(input_path, table_or_uri)`, the `ACCEPTED_SCHEMA_OPTIONS`/`SUPPORTED_PARQUET_SCHEMA_VERSIONS`/`DEFAULT_PARQUET_SCHEMA_VERSION`/`DEFAULT_JSON_CHUNK_SIZE` schema and chunk axis), `pystac` (`Item.from_dict` rehydration), `pyarrow` (the `RecordBatchReader`/`Table` carrier), `expression` (`Error` the typed-reject rail leaf), runtime (`ContentIdentity`/`BoundaryFault`/`RuntimeRail`/`boundary`).
- Growth: a new schema-inference mode is the `ACCEPTED_SCHEMA_OPTIONS` row on the parse; a new source is one `TableSource` case; a new sink is one `TableSink` case plus its `stac_table_egress` or `stac_table_direct` arm; a new one-call fast-path is one `(source, sink)` arm on `stac_table_direct`; a new GeoParquet schema version is a `schema_version` from `SUPPORTED_PARQUET_SCHEMA_VERSIONS`; zero new surface.
- Boundary: composes the `tabular/columnar`/`tabular/query` owners for the table scan and the `tabular/egress` owner for the GeoParquet upload, never a second table engine or a second writer; no durable catalog store; a hand-built STAC-to-Arrow schema mapping, a hand-rolled parquet writer, re-minting a STAC item where `from_dict` rehydrates, a per-sink writer family where `TableSink` discriminates, a materialize-then-write two-hop where the one-call `stac_table_direct` path writes straight to disk, a `delta_lake` arm on `stac_table_egress` that silently drops the in-memory `table` argument where `parse_stac_ndjson_to_delta_lake` reads an NDJSON file, and defaulting to the geopandas trio where the zero-copy Arrow path applies are the deleted forms.

```python signature
from typing import TYPE_CHECKING, Literal, assert_never

import pyarrow as pa
from expression import Error, case, tag, tagged_union

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary

if TYPE_CHECKING:
    from collections.abc import Iterable

type SchemaInference = Literal["FullFile", "FirstBatch"]


@tagged_union(frozen=True)
class TableSource:
    tag: Literal["items", "ndjson"] = tag()
    items: "Iterable[object]" = case()
    ndjson: tuple[str, int | None] = case()

    @staticmethod
    def Items(items: "Iterable[object]") -> "TableSource":
        return TableSource(items=items)

    @staticmethod
    def Ndjson(path: str, limit: int | None = None) -> "TableSource":
        return TableSource(ndjson=(path, limit))


@tagged_union(frozen=True)
class TableSink:
    tag: Literal["parquet", "ndjson_out", "delta_lake"] = tag()
    parquet: tuple[str, str | None] = case()
    ndjson_out: str = case()
    delta_lake: str = case()

    @staticmethod
    def Parquet(output_path: str, schema_version: str | None = None) -> "TableSink":
        return TableSink(parquet=(output_path, schema_version))

    @staticmethod
    def NdjsonOut(dest: str) -> "TableSink":
        return TableSink(ndjson_out=dest)

    @staticmethod
    def DeltaLake(table_or_uri: str) -> "TableSink":
        return TableSink(delta_lake=table_or_uri)


def stac_table(source: TableSource, *, schema: SchemaInference = "FullFile") -> "RuntimeRail[pa.RecordBatchReader]":
    def _parse() -> "pa.RecordBatchReader":
        from stac_geoparquet.arrow import (  # noqa: PLC0415
            DEFAULT_JSON_CHUNK_SIZE,
            parse_stac_items_to_arrow,
            parse_stac_ndjson_to_arrow,
        )

        match source:
            case TableSource(tag="items", items=items):
                return parse_stac_items_to_arrow(items, chunk_size=DEFAULT_JSON_CHUNK_SIZE, schema=schema)
            case TableSource(tag="ndjson", ndjson=(path, limit)):
                return parse_stac_ndjson_to_arrow(path, chunk_size=DEFAULT_JSON_CHUNK_SIZE, schema=schema, limit=limit)
            case unreachable:
                assert_never(unreachable)

    return boundary("stac.table", _parse)


def stac_table_egress(table: "pa.RecordBatchReader | pa.Table", sink: TableSink) -> "RuntimeRail[ContentKey]":
    def _write() -> "RuntimeRail[ContentKey]":
        from pathlib import Path  # noqa: PLC0415

        from stac_geoparquet.arrow import (  # noqa: PLC0415
            DEFAULT_PARQUET_SCHEMA_VERSION,
            stac_table_to_ndjson,
            to_parquet,
        )

        match sink:
            case TableSink(tag="parquet", parquet=(output_path, schema_version)):
                to_parquet(table, output_path, schema_version=schema_version or DEFAULT_PARQUET_SCHEMA_VERSION)
                return ContentIdentity.of("stac.geoparquet", Path(output_path).read_bytes())
            case TableSink(tag="ndjson_out", ndjson_out=dest):
                stac_table_to_ndjson(table, dest)
                return ContentIdentity.of("stac.ndjson", Path(dest).read_bytes())
            case TableSink(tag="delta_lake"):
                return Error(BoundaryFault(boundary=("stac.table.egress", "delta sink reads an NDJSON source, route through stac_table_direct")))
            case unreachable:
                assert_never(unreachable)

    return boundary("stac.table.egress", _write).bind(lambda rail: rail)


def stac_table_direct(source: TableSource, sink: TableSink, *, schema: SchemaInference = "FirstBatch") -> "RuntimeRail[ContentKey]":
    def _fuse() -> "RuntimeRail[ContentKey]":
        from pathlib import Path  # noqa: PLC0415

        from stac_geoparquet.arrow import (  # noqa: PLC0415
            DEFAULT_JSON_CHUNK_SIZE,
            DEFAULT_PARQUET_SCHEMA_VERSION,
            parse_stac_items_to_parquet,
            parse_stac_ndjson_to_delta_lake,
            parse_stac_ndjson_to_parquet,
        )

        match source, sink:
            case TableSource(tag="items", items=items), TableSink(tag="parquet", parquet=(output_path, schema_version)):
                parse_stac_items_to_parquet(
                    items,
                    chunk_size=DEFAULT_JSON_CHUNK_SIZE,
                    schema=schema,
                    output_path=output_path,
                    schema_version=schema_version or DEFAULT_PARQUET_SCHEMA_VERSION,
                )
                return ContentIdentity.of("stac.geoparquet", Path(output_path).read_bytes())
            case TableSource(tag="ndjson", ndjson=(path, limit)), TableSink(tag="parquet", parquet=(output_path, schema_version)):
                parse_stac_ndjson_to_parquet(
                    path,
                    output_path,
                    chunk_size=DEFAULT_JSON_CHUNK_SIZE,
                    limit=limit,
                    schema_version=schema_version or DEFAULT_PARQUET_SCHEMA_VERSION,
                )
                return ContentIdentity.of("stac.geoparquet", Path(output_path).read_bytes())
            case TableSource(tag="ndjson", ndjson=(path, _)), TableSink(tag="delta_lake", delta_lake=table_or_uri):
                parse_stac_ndjson_to_delta_lake(path, table_or_uri)
                return ContentIdentity.of("stac.delta", f"{path}->{table_or_uri}".encode())
            case _, _:
                return Error(
                    BoundaryFault(
                        boundary=(
                            "stac.table.direct",
                            f"no one-call path for ({source.tag}, {sink.tag}), route through stac_table then stac_table_egress",
                        )
                    )
                )

    return boundary("stac.table.direct", _fuse).bind(lambda rail: rail)


def stac_table_rehydrate(table: "pa.Table") -> "RuntimeRail[tuple[object, ...]]":
    def _rehydrate() -> "tuple[object, ...]":
        import pystac  # noqa: PLC0415
        from stac_geoparquet.arrow import stac_table_to_items  # noqa: PLC0415

        return tuple(pystac.Item.from_dict(record) for record in stac_table_to_items(table))

    return boundary("stac.rehydrate", _rehydrate)
```

## [04]-[ASSETS]

- Owner: `AssetFold` — one awaitable fold owner over the signed `StacDiscovery.collection` discriminating the `FoldTarget` tagged-union axis (`Egress`/`Cube`/`Coverage`) INTO the settled downstream seams, NOT a new transport. The `Egress` target reads only the intersecting COG/GeoTIFF byte windows through the `tabular/egress.md#EGRESS` `ObjectEgress.run(StoreOp.GetRange(...))` fence; the `Cube` target registers the cube-bearing asset hrefs as virtual chunk byte-ranges through the `gridded/virtual.md#VIRTUAL` `VirtualReference(sources, ref).apply(VersionOp.aggregate((ManifestWrite.accessor(FieldVirtual(...)), {}, None)))` fence, composing the `gridded/field#VIRTUAL` `FieldVirtual` manifest owner rather than re-deriving it; the `Coverage` target reads `proj:epsg`/`raster:bands`/`eo:cloud_cover` through the typed `pystac.extensions` accessors into one `RasterGeoClaim` plus the matching `stac_cfg`, then drives the `odc-stac` `odc.stac.load(items, stac_cfg=, patch_url=, groupby=, resampling=, chunks=)` COG datacube with the `Signing.patch_url()` threaded so the COG reads are SAS-signed. The three targets thread the inner `RuntimeRail` they compose through `.bind` — the `Egress` arm folds its per-window `ObjectEgress.run` rails through `traversed(..., by=Disposition.ABORT)` so the first byte-window fault aborts the whole fold, the `Cube` arm binds the `VirtualReference.apply` rail and narrows its `VirtualOutcome` to the `VirtualReceipt`, the `Coverage` arm rides the runtime `guarded(RetryClass.HTTP, anyio.to_thread.run_sync, ...)` envelope so the blocking COG metadata read offloads off the event loop and retries the transient HTTP set; all three close through one `_rekey` folding the railed `ContentIdentity.of` into the one `StacDiscovery`; never a swallowed inner rail, second object-store transport, virtual-cube builder, COG loader, SAS token owner, or hand-built band table.
- Cases: `FoldTarget` rows `Egress(egress, windows)` (each raster asset whose `media_type` is a `pystac.MediaType.COG`/`MediaType.GEOTIFF` enum row folds to `ObjectEgress.run(StoreOp.GetRange(href, start, end))` over the COG header/overview/tile byte ranges the `windows` map carries) · `Cube(ref, concat_dim)` (the cube-bearing asset hrefs register as virtual chunk byte-ranges through `VirtualReference(sources, ref).apply(VersionOp.aggregate((ManifestWrite.accessor(FieldVirtual(sources=sources, target=ref, concat_dim=concat_dim)), {}, None)))`, the destination `ResourceRef` the icechunk virtual store, the `FieldVirtual` manifest the `gridded/field#VIRTUAL` owner builds) · `Coverage(groupby, resampling, chunks)` (the `odc.stac.load` COG datacube over the discovered items driven by the extension-derived `stac_cfg`/`patch_url`) — the target value IS the route, never a parallel per-asset egress class. The shared `_raster_hrefs` generator yields the `(asset, href)` pairs whose `media_type` matches the `{MediaType.COG, MediaType.GEOTIFF}` enum set (`pystac.md` L33/L76), the one media-type gate for both the `Egress` and `Cube` arms, never a per-arm raw-MIME `frozenset` re-stated twice. The extension read rides the one canonical `obj.ext.<short>` accessor the object type statically scopes (`pystac.md` L97/L127) — `sample.ext.proj` for `proj:epsg`/`proj:transform`, `sample.ext.eo` for `eo:cloud_cover`/`eo:bands`, and `asset.ext.raster` for `raster:bands`/nodata — never the legacy `XExtension.ext(obj, add_if_missing=False)` classmethod the accessor delegates to and never a raw `item.properties["proj:epsg"]` dictionary probe.
- Entry: the awaitable `AssetFold.over` admits one `FoldTarget`, returning a `RuntimeRail[StacDiscovery]` that folds the target over the signed `collection`; the head guard returns `Error(BoundaryFault(boundary=("stac.assets", ...)))` when `self.discovery.surface is Surface.COLLECTION` so a collection-discovery terminal never reaches the asset arms, then `_raster_hrefs` materializes the gated `sources` tuple once and the three arms read it. The `Egress` arm folds the `windows.get(href)` walrus-gated windowed assets into a `Block` of `egress.run(StoreOp.GetRange(href, start, end), path=href)` rails and threads them through `traversed(rails, by=Disposition.ABORT)`, binding the resolved-receipt `Block` into `_rekey` so the fold aborts on the first byte-window fault and never swallows a failed range read; the egress owner short-circuits an unchanged content-key to a by-reference no-op, so a re-discovery never re-reads. The `Cube` arm builds one `ManifestWrite.accessor(FieldVirtual(sources=sources, target=ref, concat_dim=concat_dim))` and binds `VirtualReference(sources=sources, ref=ref).apply(VersionOp.aggregate((manifest, {}, None)))`, narrowing the returned `VirtualOutcome` to the `VirtualReceipt` through one `isinstance` arm and reading the real `VirtualReceipt.chunk_refs` manifest count rather than a bare `len(sources)`, the non-`VirtualReceipt` arm a typed reject the `aggregate` case never reaches. The `Coverage` arm rides `await guarded(RetryClass.HTTP, anyio.to_thread.run_sync, lambda: self._coverage(...), subject="stac.assets.coverage")` then `.bind(lambda rail: rail)` self-flattens, where `_coverage` reads the `sample.ext.proj`/`sample.ext.eo`/`asset.ext.raster` accessors of a sample item into one `RasterGeoClaim` (`spatial/geospatial.md#GEO`) and the matching `stac_cfg`, then calls `odc.stac.load(items, stac_cfg=cfg, patch_url=signing.patch_url(), groupby=groupby, resampling=resampling, chunks=chunks)` so the cube load reads its CRS/bands/nodata from the catalogue rather than a hand-fed config and the blocking COG metadata read offloads onto the worker thread and retries the transient HTTP set under the runtime backoff rather than a hand-rolled loop or a sync drive of the awaitable bound caller. Every arm closes through one `_rekey(tag, payload, folded)` folding the railed `ContentIdentity.of(f"stac.assets.{tag}", payload)` through `.map` into a `StacDiscovery` with every field of the source discovery preserved (`item_ids`, `matched`, `expiry`, the resolved `url`) plus the real folded count — the `Egress` payload the joined source hrefs and the folded count the summed `EgressReceipt.byte_length`, the `Cube` payload the `concat_dim`-prefixed source join and the count the manifest `chunk_refs`, the `Coverage` payload the claim CRS plus the loaded cube's `sizes` shape and the count the `band_count` — so a coverage of a discovery is byte-distinct from its egress and a single new asset flips the key, the raw-rail-into-`content_key` stuff, the no-op re-key, and the swallowed inner rail the deleted forms.
- Auto: the byte window is the COG/GeoTIFF archival range the `windows` map carries (the GeoTIFF IFD header/overview/tile byte offsets), passed straight to `GetRange` so the read is one HTTP range request, never a full-object materialization; the virtual cube reuses the `gridded/field#VIRTUAL` `FieldVirtual` owner's mandatory `ObjectStoreRegistry` URL-to-`obstore` backend map internal to its aggregation, so the STAC asset URLs register through the same `from_url` transport the egress owner speaks — one transport across discovery, egress, and cube; the extension accessors resolve the typed `pystac.extensions` schema rather than the raw `properties` map, so a missing extension is a typed absence not a `KeyError`; the `RasterGeoClaim` carries the `proj:epsg` CRS, the `raster:bands` nodata, and the `eo:bands` band count, the `ClaimBundle` carries the `eo:cloud_cover` scene fraction beside the claim (a coverage knob the sibling-owned `RasterGeoClaim` shape carries no slot for), the `stac_cfg` carries the per-asset nodata into `odc.stac.load`, and the `Signing.patch_url()` rides `patch_url=` so the COG reads are SAS-signed by the same `planetary_computer.sign` that signed the discovery result; the `odc.stac.load` call rides the `guarded(RetryClass.HTTP, anyio.to_thread.run_sync, ...)` envelope so the transient HTTP fault on the blocking cube-metadata read retries under the runtime backoff off the event loop and the terminal raise lifts through the faults owner's `async_boundary` exactly once, the same `HTTP` row the discovery search rides so the two network legs share one retry policy; the loaded `xarray.Dataset` cube's `sizes` shape, the `ClaimBundle` CRS, and the `eo:cloud_cover` fraction fold into the `Coverage` content key so a cloudy and a clear scene of the same bbox key byte-distinct and the cube identity is carried forward, never discarded.
- Receipt: the asset fold re-mints the one `StacDiscovery` declared in `[2]-[CATALOG]` keyed by the fold-target `ContentIdentity` over its arm-specific payload plus the folded count, contributing an emitted-phase `Receipt.of` through `ReceiptContributor`; no new receipt rail.
- Packages: `pystac` (`Item.assets`/`Asset.href`/`Asset.media_type`/`MediaType.COG`/`MediaType.GEOTIFF`, the canonical `obj.ext.<short>` accessor `item.ext.proj`/`item.ext.eo`/`asset.ext.raster`, `pystac.md` L97/L101-103), `odc-stac` (`odc.stac.load(items, *, stac_cfg=, patch_url=, groupby=, resampling=, chunks=, bands=)`, `odc-stac.md` L41), `tabular/egress` (`ObjectEgress.run` returning `RuntimeRail[EgressReceipt]` whose `byte_length` the egress fold sums, `StoreOp.GetRange`, `tabular/egress.md#EGRESS` L12), `gridded/virtual` (`VirtualReference(sources, ref).apply(VersionOp.aggregate(...))` returning `RuntimeRail[VirtualOutcome]` whose `VirtualReceipt.chunk_refs` the cube fold reads after the `isinstance` narrowing, `ManifestWrite.accessor`/`VersionOp.aggregate`/`VirtualReceipt`, `gridded/virtual.md#VIRTUAL`), `gridded/field` (`FieldVirtual` the composed manifest cube the `ManifestWrite.accessor` lowers, `gridded/field#VIRTUAL`), `spatial/geospatial` (`RasterGeoClaim`/`Resampling`, `spatial/geospatial.md#GEO`), `spatial/catalog` (`Surface` the discovery-method discriminant the terminal guard reads), `expression` (`Block.of_seq` the rail-block construction over the windowed-href comprehension, `Error` the terminal-reject rail leaf), `anyio` (`to_thread.run_sync` offloading the blocking `odc.stac.load` COG read off the event loop under the `guarded` envelope), runtime (`ContentIdentity`/`RuntimeRail`/`BoundaryFault` the terminal-reject leaf/`traversed`/`Disposition` the `ABORT` short-circuit row the egress fold selects/`RetryClass`/`guarded` the fused `HTTP` retry-span-plus-`async_boundary` envelope the `_coverage` COG read rides/`ReceiptContributor`).
- Growth: a new archival format is one more `MediaType` member in the `_raster_hrefs` gate routed to the `Egress` `GetRange`; a new cube source format is the existing `gridded/field#VIRTUAL` `VirtualParser` case upstream with zero change here; a new coverage knob is one field on the `Coverage` row; a new extension read is one more typed accessor row in `_claim`; a per-collection cube is one `concat_dim` on the `Cube` row; zero new surface.
- Boundary: reads the settled `tabular/egress` `GetRange`, `gridded/virtual` `VirtualReference.apply`, `gridded/field` `FieldVirtual`, `spatial/geospatial` `RasterGeoClaim`, and `odc-stac` `load` fences and never re-mints any — no second object-store transport, no second virtual-cube builder, no second COG loader, no second raster claim, no full-object read where a byte window applies, no durable asset store; a per-asset egress class family, a phantom `VirtualReference.aggregate(sources, ref, ...)` classmethod where the instance `VirtualReference(sources, ref).apply(VersionOp.aggregate(...))` is the surface, a re-derived virtual cube or kerchunk manifest where `FieldVirtual` owns it, a hand-rolled COG reader where `odc.stac.load` owns it, a synchronous `guard(RetryClass.HTTP)(...)` drive of the awaitable bound caller or a hand-rolled retry loop / manual `sleep` backoff around the `odc.stac.load` COG read where `guarded(RetryClass.HTTP, anyio.to_thread.run_sync, ...)` owns the transient-HTTP retry off the event loop, a blocking `odc.stac.load` left on the event loop where `anyio.to_thread.run_sync` offloads it, a raw `properties` dictionary probe where the typed `obj.ext.<short>` accessor applies, the legacy `XExtension.ext(obj)` classmethod where the `obj.ext.proj`/`obj.ext.eo`/`asset.ext.raster` accessor is canonical, a raw-MIME `frozenset` where `MediaType.COG`/`GEOTIFF` resolves, an imperative `for`/`if` asset loop where the rail-block fold threads, a swallowed `ObjectEgress.run`/`VirtualReference.apply` inner rail whose fault the fold never binds, a `VirtualOutcome` consumed without the `VirtualReceipt` narrowing where the union must be discriminated before `.chunk_refs`, a `len(sources)`/`len(reads)` fold count where the real `EgressReceipt.byte_length` sum and `VirtualReceipt.chunk_refs` manifest count are the evidence, a raw `RuntimeRail[ContentKey]` stuffed into `content_key: ContentKey` where `_rekey` folds the railed `ContentIdentity.of` through `.map`, a `_rekey` dropping `expiry`/`matched`/`url`, a no-op re-key that discards the fold outcome, and a hand-fed `stac_cfg` where the extension read derives it are the deleted forms.

```python signature
from typing import TYPE_CHECKING, Literal, assert_never

import anyio
from expression import Error, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct

from rasm.data.catalog import Signing, StacDiscovery, Surface
from rasm.data.gridded.field import FieldVirtual
from rasm.data.gridded.virtual import ManifestWrite, VirtualReceipt, VirtualReference, VersionOp
from rasm.data.spatial.geospatial import RasterGeoClaim, Resampling
from rasm.data.tabular.egress import ObjectEgress, StoreOp
from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import BoundaryFault, Disposition, RuntimeRail, traversed
from rasm.runtime.resilience import RetryClass, guarded
from rasm.runtime.roots import ResourceRef

if TYPE_CHECKING:
    from collections.abc import Iterator, Mapping

type StacCfg = dict[str, dict[str, object]]
type Window = tuple[int, int]


def _raster_hrefs(collection: object) -> "Iterator[tuple[object, str]]":
    from pystac import MediaType  # noqa: PLC0415

    raster = {MediaType.COG, MediaType.GEOTIFF}
    return ((asset, asset.href) for item in collection for asset in item.assets.values() if asset.media_type in raster)


class ClaimBundle(Struct, frozen=True):
    claim: RasterGeoClaim
    stac_cfg: StacCfg
    cloud_cover: float | None


@tagged_union(frozen=True)
class FoldTarget:
    tag: Literal["egress", "cube", "coverage"] = tag()
    egress: tuple[ObjectEgress, "Mapping[str, Window]"] = case()
    cube: tuple[ResourceRef, str] = case()
    coverage: tuple[str, str, "dict[str, int] | None"] = case()

    @staticmethod
    def Egress(egress: ObjectEgress, windows: "Mapping[str, Window]") -> "FoldTarget":
        return FoldTarget(egress=(egress, windows))

    @staticmethod
    def Cube(ref: ResourceRef, concat_dim: str = "time") -> "FoldTarget":
        return FoldTarget(cube=(ref, concat_dim))

    @staticmethod
    def Coverage(groupby: str = "time", resampling: Resampling = "nearest", chunks: "dict[str, int] | None" = None) -> "FoldTarget":
        return FoldTarget(coverage=(groupby, resampling, chunks))


class AssetFold(Struct, frozen=True):
    discovery: StacDiscovery
    signing: Signing

    async def over(self, target: FoldTarget) -> "RuntimeRail[StacDiscovery]":
        if self.discovery.surface is Surface.COLLECTION:
            return Error(BoundaryFault(boundary=("stac.assets", "collection-discovery is a terminal, re-enter an item search before the asset fold")))
        sources = tuple(href for _, href in _raster_hrefs(self.discovery.collection))
        match target:
            case FoldTarget(tag="egress", egress=(egress, windows)):
                windowed = tuple((href, w) for href in sources if (w := windows.get(href)) is not None)
                rails = Block.of_seq(egress.run(StoreOp.GetRange(href, start, end), path=href) for href, (start, end) in windowed)
                return traversed(rails, by=Disposition.ABORT).bind(
                    lambda receipts: self._rekey("egress", "\n".join(href for href, _ in windowed).encode(), sum(r.byte_length for r in receipts))
                )
            case FoldTarget(tag="cube", cube=(ref, concat_dim)):
                manifest = ManifestWrite.accessor(FieldVirtual(sources=sources, target=ref, concat_dim=concat_dim))
                return (
                    VirtualReference(sources=sources, ref=ref)
                    .apply(VersionOp.aggregate((manifest, {}, None)))
                    .bind(
                        lambda outcome: (
                            self._rekey("cube", f"{concat_dim}|{'|'.join(sources)}".encode(), outcome.chunk_refs)
                            if isinstance(outcome, VirtualReceipt)
                            else Error(BoundaryFault(boundary=("stac.assets.cube", "VersionOp.aggregate yielded no VirtualReceipt")))
                        )
                    )
                )
            case FoldTarget(tag="coverage", coverage=(groupby, resampling, chunks)):
                return (
                    await guarded(
                        RetryClass.HTTP, anyio.to_thread.run_sync, lambda: self._coverage(groupby, resampling, chunks), subject="stac.assets.coverage"
                    )
                ).bind(lambda rail: rail)
            case unreachable:
                assert_never(unreachable)

    def _coverage(self, groupby: str, resampling: Resampling, chunks: "dict[str, int] | None") -> "RuntimeRail[StacDiscovery]":
        import odc.stac  # noqa: PLC0415

        bundle = self._claim(next(iter(self.discovery.collection)), resampling)
        cube = odc.stac.load(
            list(self.discovery.collection),
            stac_cfg=bundle.stac_cfg,
            patch_url=self.signing.patch_url(),
            groupby=groupby,
            resampling=resampling,
            chunks=chunks or {},
        )
        payload = f"{bundle.claim.crs}:{tuple(cube.sizes.values())}:{bundle.cloud_cover}".encode()
        return self._rekey("coverage", payload, bundle.claim.band_count)

    def _rekey(self, tag: str, payload: bytes, folded: int) -> "RuntimeRail[StacDiscovery]":
        return ContentIdentity.of(f"stac.assets.{tag}", payload).map(
            lambda key: StacDiscovery(
                endpoint=self.discovery.endpoint,
                surface=self.discovery.surface,
                collection=self.discovery.collection,
                item_ids=self.discovery.item_ids,
                matched=self.discovery.matched,
                href_count=folded,
                expiry=self.discovery.expiry,
                url=self.discovery.url,
                content_key=key,
            )
        )

    @staticmethod
    def _claim(sample: object, resampling: Resampling) -> ClaimBundle:
        projection = sample.ext.proj
        eo = sample.ext.eo
        assets = tuple(sample.assets.values())
        bands = assets[0].ext.raster.bands or ()
        nodata = next((band.nodata for band in bands if band.nodata is not None), 0.0)
        claim = RasterGeoClaim(
            crs=f"EPSG:{projection.epsg}",
            band_count=len(eo.bands or bands),
            resampling=resampling,
            nodata=float(nodata),
            transform=tuple(projection.transform or ()),
        )
        stac_cfg: StacCfg = {"*": {"assets": {asset.title or asset.href: {"nodata": float(nodata)} for asset in assets}}}
        return ClaimBundle(claim=claim, stac_cfg=stac_cfg, cloud_cover=eo.cloud_cover)
```

## [05]-[RESEARCH]

- [STAC_SIGN_DISPATCH]: `Signing.sign(search)` runs `planetary_computer.sign` over the lazy `pystac_client.ItemSearch` handle itself (the catalogued `singledispatch` registers `ItemSearch`, returning a fresh signed `ItemCollection` in one materialization, `planetary-computer.md` L44/L64), so the discovery result carries signed hrefs the `[4]-[ASSETS]` fold reads with neither a second sign pass nor a materialize-then-sign two-pass — the `NONE` row's `search.item_collection()` is the matching unsigned materialization (`pystac-client.md` L52), so both scheme rows return one `ItemCollection`; the one `SignScheme.row` projects the `SchemeRow` carrying the named `open_kwargs`/`sign`/`patch_url` callables so `Signing.open_kwargs` threads `modifier=planetary_computer.sign_inplace` (`sign_inplace(obj: Any) -> Any`, `planetary-computer.md` L40) plus `headers=`/`timeout=`, `Signing.sign` dispatches `planetary_computer.sign(ItemSearch)`, and `Signing.patch_url` returns `planetary_computer.sign` for `odc.stac.load(patch_url=)` — all three projections catalogue-settled and collapsed onto one frozen `SchemeRow` read by name rather than three parallel `match` statements; the `SignScheme` `StrEnum` row carries the strategy so `Signing` admits onto the frozen `StacCatalog` with no callable field. The idempotence over already-signed (`st`/`se`/`sp` present) and non-Azure hrefs is the catalogued selectivity (`planetary-computer.md` L66), so a re-sign of the discovery result is a no-op. The `msft:expiry` token horizon `StacDiscovery.expiry` reads off `Item.properties["msft:expiry"]` (the catalogued ISO-8601 expiry the SAS signing writes onto each signed item, `planetary-computer.md` L70) — the property key confirms against the synced `planetary-computer`/`pystac` distributions before the `min(...)` horizon read treats it as settled, the `sign(ItemSearch)` dispatch and `sign_inplace` modifier already settled.
- [STAC_EXTENSION_PROPERTIES]: the `AssetFold._claim` fence reads the extension properties through the one canonical `obj.ext.<short>` accessor the object type statically scopes (`item.ext.proj.epsg`/`.transform`, `item.ext.eo.bands`/`.cloud_cover`, `asset.ext.raster.bands` with each band's `.nodata`) — the catalogue (`pystac.md` L97 the accessor law, L101-103 the per-extension `proj`->`epsg`/`transform`, `eo`->`bands`/`cloud_cover`, `raster`->`bands`/nodata capability rows) confirms the accessor and the EPSG/bands/nodata/cloud-cover capability, so the `.epsg`, `.transform`, `.cloud_cover`, `.bands`, and per-band `.nodata` attribute names confirm against the synced `pystac.extensions` distribution before the `RasterGeoClaim` CRS/band/nodata read and the `ClaimBundle.cloud_cover` scene-fraction read treat them as settled, the `obj.ext.<short>` accessor itself already settled as the canonical surface that supersedes the legacy `XExtension.ext(obj)` classmethod.
- [STAC_CONFORMANCE_SURFACES]: the CQL2-JSON `filter_lang="cql2-json"` value, the legacy `query`-extension predicate the `Cql2Query` arm spreads, the `sortby`/`fields` ordering and projection the `Order` arm spreads, and the `eo:cloud_cover`/`proj:epsg` property predicates are STAC API conformance-class surfaces the server evaluates, confirmed against the target endpoint's advertised conformance before the `Cql2Filter`/`Cql2Query`/`Order` arms treat the predicate, ordering, or projection as accepted; the `intersects` GeoJSON-geometry `dict[str, object]` payload stays shapely-free at the boundary, the `Client.search(intersects=)` keyword admitting the raw GeoJSON mapping directly (`pystac-client.md` L40).
- [STAC_COLLECTION_SURFACE]: the `COLLECTION` `SurfaceRow.materialize` policy (`_materialize_collections`) routes the free-text union to `Client.collection_search` returning `CollectionSearch` (`pystac-client.md` L43) and materializes its collections through the catalogued `CollectionSearch.collection_list() -> list[Collection]` member (`pystac-client.md` L69) without signing — `planetary_computer.sign` registers no `CollectionSearch` overload (the catalogued `singledispatch` covers `str`/`Asset`/`Item`/`ItemCollection`/`Collection`/`ItemSearch`/`Mapping` only, `planetary-computer.md` L39/L64), so the `COLLECTION` leg correctly omits the sign, which is the settled architectural decision; `CollectionSearch` carries its own `matched()`/`collections()`/`pages()` paging mirror (`pystac-client.md` L70), so the leg's zero-href, no-expiry shape is a deliberate `StacDiscovery` projection (a STAC `Collection` carries no `.assets`/`msft:expiry`), not a missing member. The `SurfaceRow.accepts` set names exactly the `collection_search` signature keywords (`bbox`/`datetime`/`query`/`filter`/`filter_lang`/`sortby`/`fields`/`q`, `pystac-client.md` L43), so a `FreeText` union that also folds an `Ids`/`Intersects`/`Collection` row contributes those keys to `params` but the row filter drops them before `collection_search`, the `_SHARED` ∪ `{q}` admission the one boundary that keeps the union total across the surface flip rather than a runtime `TypeError` on an unexpected keyword; the `ITEM` row's `_SHARED` ∪ `{ids, collections, intersects}` set is the `search` signature (`pystac-client.md` L40). The `collection_list`/`collection_search` surface and the `CollectionSearch` return are catalogue-settled (`pystac-client.md` L43/L69/L70), so `_materialize_collections` is fully settled against the synced distribution. The `COLLECTION` carrier is a `list[Collection]`, not an `ItemCollection`, so the `StacDiscovery.collection` field is the precise closed union `ItemCollection | list[Collection]` rather than an `object`-erased slot, and the outcome carries a `surface: Surface` discriminant — `item_ids` reads each collection's `.id`, `href_count` is zero because a STAC `Collection` carries no `.assets`, and `expiry` is `None` because collection metadata carries no `msft:expiry` — and the discovery-of-collections terminal is structural rather than prose: `AssetFold.over` returns `Error(BoundaryFault(boundary=("stac.assets", ...)))` on a `Surface.COLLECTION` discovery, so the resolved collection ids re-enter an item `search` before `[4]-[ASSETS]` runs, the collection surface never an item-asset source.
