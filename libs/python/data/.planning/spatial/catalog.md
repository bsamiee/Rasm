# [PY_DATA_CATALOG]

The cloud-native STAC discovery owner: one `StacCatalog` over `pystac-client` that resolves WHICH cloud assets cover a query region, the discovery layer the geospatial and object-store egress lanes lack above the raster/vector claims (`spatial/geospatial.md#GEO`) and the archival byte-window read (`tabular/egress.md#EGRESS`). `StacCatalog.discover` folds the one `StacQuery` tagged-union axis — bbox/intersects/datetime/ids/collection/CQL2-filter/CQL2-query/order/free-text discriminants — onto the single keyword-only `pystac_client.Client.search`, with the `Surface` discriminant alone routing the same shared rows to `Client.collection_search` when a `FreeText` case is present, pages the lazy `ItemSearch` into the `pystac.ItemCollection`, and emits one `StacDiscovery` outcome keyed by runtime `ContentIdentity` over the matched item-id set plus the `ItemSearch.matched` count. One `Signing` value generalizes the request boundary — the `headers` map plus the `SignScheme` `StrEnum` whose `SchemeRow` carries the named `open_kwargs`/`sign`/`patch_url` behavior callables, threaded once into `Client.open(modifier=)` for discovery, once over the `ItemSearch` for the asset hrefs, and once into `odc.stac.load(patch_url=)` for the COG cube load — three reads of one frozen policy row by name, never three parallel `match` statements, never a positional integer-index read, never a signed-vs-unsigned client pair, and never a bare forwarded callable. The `Surface` discriminant's `materialize` row owns the divergence between the two result iterators, so the `ITEM` surface signs the `ItemSearch` and reads `matched()` while the `COLLECTION` surface materializes collections without invoking the item-only `sign`/`matched` members. The discovered collection encodes as a `stac-geoparquet` columnar Arrow `RecordBatchReader` (`[3]-[TABLE]`) the `tabular/columnar.md#SCAN` scan and the `tabular/query.md#QUERY` engine consume, and the asset hrefs fold (`[4]-[ASSETS]`) into the `tabular/egress.md#EGRESS` `ObjectEgress.GetRange` archival-byte fast-path, the `gridded/virtual.md#VIRTUAL` `VirtualReference.aggregate` virtual-reference cube, the `odc-stac` `odc.stac.load` COG datacube driven by the catalogue-derived `stac_cfg`/`patch_url`, and the `proj:epsg`/`raster:bands`/`eo:cloud_cover` extension read-back into one `RasterGeoClaim` — reading those settled fence contracts, never a second object-store transport, virtual-cube builder, COG loader, or hand-fed band metadata. Every bundle keys by exactly one runtime `ContentIdentity`.

## [01]-[INDEX]

- [01]-[CATALOG]: the `StacCatalog` discovery owner over `pystac-client` `Client.open`/`search`, the `StacQuery` tagged-union search axis with the `Surface` discriminant routing `search`/`collection_search`, its `SurfaceRow.accepts` keyword-admission set filtering the per-surface parameter rows and its `SurfaceRow.materialize` policy owning the sign-and-count divergence, the `Signing` request-boundary value whose `SignScheme` `SchemeRow` carries the named `open_kwargs`/`sign`/`patch_url` callables, `ItemSearch` paging into `pystac.ItemCollection`, the one `StacDiscovery` outcome carrying its `surface` terminal discriminant.
- [02]-[TABLE]: the discovered item collection encoded as a `stac-geoparquet` columnar Arrow `RecordBatchReader` the `tabular/columnar` scan and `tabular/query` engine consume, with the GeoParquet/NDJSON/Delta sink rows split across `stac_table_egress` (table-in writes) and `stac_table_direct` (one-call source-to-disk fast-paths).
- [03]-[ASSETS]: the one `AssetFold` over the signed `ItemCollection` discriminating asset role by `MediaType` — the `tabular/egress` `ObjectEgress.GetRange` byte-window read, the `gridded/virtual` `VirtualReference.aggregate` cube, the `odc-stac` `odc.stac.load` COG datacube driven by the `pystac.extensions`-derived `RasterGeoClaim`/`stac_cfg`/`patch_url`, the one `StacDiscovery` re-key.

## [02]-[CATALOG]

- Owner: `StacCatalog` — the one cloud-native discovery owner, a `Client.open`-bound STAC API root carrying one `Signing` request-boundary value; `StacQuery` the tagged-union search axis (bbox/intersects/datetime/ids/collection/cql2-filter/cql2-query/order/free-text), folded by `match`/`case` closed by `assert_never` onto the single keyword-only `Client.search(*, method=, max_items=, limit=, ids=, collections=, bbox=, intersects=, datetime=, query=, filter=, filter_lang=, sortby=, fields=)`. A new search modality is one `StacQuery` case, never a `search_bbox`/`search_intersects`/`search_cql2`/`search_collections` method family — bbox vs intersects vs datetime vs ids vs collection vs CQL2 filter vs ordering vs free-text are parameter rows on the one `search`, the exact discrimination `pystac-client.md` L64 mandates.
- Cases: `StacQuery` rows `Bbox(west, south, east, north)` (spatial selection through `search(bbox=(w,s,e,n))`) · `Intersects(geometry)` (polygon-region selection through `search(intersects=<geojson-dict>)`, the GeoJSON-geometry `dict[str, object]` the server intersects server-side, no shapely at the boundary) · `Datetime(start, end)` (temporal selection through `search(datetime="<start>/<end>")` the RFC-3339 interval) · `Ids(*item_ids)` (direct item selection through `search(ids=[...])`, `pystac-client.md` L40) · `Collection(*ids)` (collection-scoped discovery through `search(collections=[...])`) · `Cql2Filter(predicate)` (the CQL2-JSON predicate through `search(filter=..., filter_lang="cql2-json")` — eo:cloud_cover/gsd/proj:epsg property predicates the STAC API evaluates server-side) · `Cql2Query(query)` (the legacy `query` extension predicate through `search(query=...)` the same conformance class admits) · `Order(sortby, fields)` (server-side ordering and field projection through `search(sortby=[...], fields={"include": [...], "exclude": [...]})`) · `FreeText(q)` (free-text discovery whose presence flips the `Surface` discriminant from `ITEM` to `COLLECTION`, routing the whole union to `Client.collection_search(q=..., **shared)` rather than `search`), each carrying its `params()` projection that contributes exactly its own keyword arguments so an n-axis query unions the per-case keyword dicts rather than forking a method per axis combination — and because `collection_search` shares the `bbox`/`datetime`/`query`/`filter`/`filter_lang`/`sortby`/`fields` axis with `search` (`pystac-client.md` L43), the same `Bbox`/`Datetime`/`Cql2Filter`/`Order` rows union onto either surface unchanged, the surface alone differing.
- Surface: `Surface` is the discovery-method discriminant carrying its own routing policy — `ITEM`/`COLLECTION` whose `.row` property projects the one frozen `SurfaceRow` `(method, cap, accepts, materialize)`, the bound `pystac-client` method name (`search`/`collection_search`), that method's total-cap keyword (`max_items`/`max_collections`, the divergence `pystac-client.md` L40/L43 names), the `accepts` frozen keyword-admission set naming exactly the parameter rows the bound method signature admits (`_SHARED` ∪ `{ids, collections, intersects}` for `search`, `_SHARED` ∪ `{q}` for `collection_search`, the `pystac-client.md` L40/L43 signatures), and the per-surface `materialize` policy that owns the full `StacDiscovery`-shaping projection — the materialized carrier, the id tuple, the matched count, the href count, and the expiry horizon. `row.call(client, max_items, limit, params)` reads the method, cap, and the admissible-keyword filter off one row rather than a `cap = "max_collections" if ... else "max_items"` ternary fork plus a blind `**params` splat, so an `ids`/`intersects`/`collections` row that a `FreeText` union folds in never reaches `collection_search` (which rejects it), the `accepts` set the one boundary that keeps the union law total across surfaces; `row.materialize(search, signing)` reads the entire shaped outcome off the same row rather than a `signing.sign(search) if surface is ITEM else ...` branch followed by `getattr`-probed `.assets`/`.properties` derivation. The `materialize` row is load-bearing because the two iterators diverge structurally: the `ITEM` row signs the `ItemSearch` through `signing.sign(search)`, counts via `ItemSearch.matched()`, and reads each item's `.assets`/`msft:expiry`, while the `COLLECTION` row never signs (a `CollectionSearch` carries no Azure-asset hrefs and `planetary_computer.sign` registers no `CollectionSearch` overload), reads each collection's `.id`, and yields `href_count=0`/`expiry=None` because a STAC `Collection` carries no assets or token expiry, so routing the union to `collection_search` never calls the item-only `sign`/`matched`/`.assets` members. `Surface.of_queries(queries)` recovers `COLLECTION` exactly when a `FreeText` case is present, the one boolean-free routing read, threaded onto `StacDiscovery.surface` so a `COLLECTION`-surface discovery is a typed terminal the `[4]-[ASSETS]` `AssetFold.over` rejects rather than iterating a collection set as an asset source.
- Signing: `Signing` is the one request-boundary value carrying its own behavior — `headers` the bearer/SAS request headers a cloud endpoint mandates, `timeout` the `Client.open(timeout=)` request-deadline seconds (`pystac-client.md` L39), and `scheme` the `SignScheme` `StrEnum` row (`NONE`/`PLANETARY_COMPUTER`) whose `.row` projects the one frozen `SchemeRow` carrying the named `open_kwargs`/`sign`/`patch_url` behavior callables, so the value encodes the boundary as a frozen policy row read by name in one hop rather than a positional triple indexed by integer, three parallel full-coverage `match` statements, or a forwarded bare callable. `Signing.open_kwargs()` reads `scheme.row.open_kwargs` (`headers=`/`timeout=`, plus `modifier=planetary_computer.sign_inplace` the in-place `modifier=` callable on the `PLANETARY_COMPUTER` row, `planetary-computer.md` L40), `Signing.sign(search)` reads `scheme.row.sign` running `planetary_computer.sign` `singledispatch` over the lazy `ItemSearch` handle itself (the registered `ItemSearch` overload, `planetary-computer.md` L44/L64, returning a fresh signed `ItemCollection` in one materialization, idempotent over already-signed and non-Azure hrefs), so the sign and the page-materialization fuse into one pass rather than materializing then re-signing, and `Signing.patch_url()` reads `scheme.row.patch_url` projecting the `odc.stac.load(patch_url=)` callable; `Signing.none` is the unsigned default, `Signing.of_headers(headers=, timeout=)` the header-only row, `Signing.planetary_computer(subscription_key=, headers=, timeout=)` resolves `set_subscription_key` function-local then selects the `PLANETARY_COMPUTER` row. The same `Signing` threads thrice — `Client.open(**signing.open_kwargs())` for discovery, `signing.sign(search)` over the `ItemSearch` handle the `ITEM` `materialize` row drives, and `signing.patch_url()` into `odc.stac.load` (`[4]-[ASSETS]`) — three reads of one `SchemeRow`, never a forked signed-vs-unsigned client and never a second SAS token owner.
- Entry: `StacCatalog.open` admits a STAC API root URL plus one `Signing`; `StacCatalog.discover` binds `Client.open(self.endpoint, **self.signing.open_kwargs())`, folds the query tuple's `params()` projections into one keyword set through `reduce(... | q.params(), queries, {})`, recovers `Surface.of_queries(queries)` and its `.row`, dispatches `row.call(client, max_items, limit, params)` so the method name, the total-cap keyword, and the per-surface `accepts` keyword filter all read off the one `SurfaceRow` and a cross-surface param never reaches a method that rejects it, then materializes through `row.materialize(search, self.signing)` which returns the full `(collection, item_ids, matched, href_count, expiry)` shaped outcome — the `ITEM` row signs the lazy `ItemSearch` handle (the one `planetary_computer.sign(ItemSearch)` dispatch both materializes the page and rewrites every `*.blob.core.windows.net` href in a single pass), reports `ItemSearch.matched()`, and reads each item's `.assets`/`msft:expiry`, while the `COLLECTION` row materializes the collections without signing and yields a zero href count and no expiry, sizing each page request with `limit`; `_search` destructures the row's tuple straight into the one `StacDiscovery` with no shape-probing branch, threading the recovered `surface` discriminant onto the outcome, the return a `RuntimeRail[StacDiscovery]` carrying the surface, the materialized carrier, the id tuple, the row-reported matched total, the href count, the earliest `msft:expiry` token-validity horizon, and the `ContentKey`. The runtime `ResourceRef` carries the asset-byte path (`[4]-[ASSETS]`) the `ObjectEgress.GetRange` egress speaks, never re-minted as a second STAC API credential owner.
- Auto: the `StacQuery.params` fold is the union law — each case spreads to its own keyword dict (`Bbox` to `{"bbox": ...}`, `Intersects` to `{"intersects": ...}`, `Datetime` to `{"datetime": ...}`, `Ids` to `{"ids": ...}`, `Collection` to `{"collections": ...}`, `Cql2Filter` to `{"filter": ..., "filter_lang": "cql2-json"}`, `Cql2Query` to `{"query": ...}`, `Order` to `{"sortby": ..., "fields": ...}`, `FreeText` to `{"q": ...}`) and a multi-axis discovery merges the spreads into one call through `reduce`, so a bbox+datetime+cloud-cover+order query is one search, never four; `Surface.of_queries` flips the union to `collection_search` exactly when a `FreeText` case is present because free-text collection discovery is a `CollectionSearch` not an `ItemSearch`, the `SurfaceRow` supplying the method name, its `max_collections` cap, and the non-signing `materialize` policy with no boolean fork and no `search_by_<axis>` family; `ItemSearch` is lazy so the `ITEM` `materialize` row's `matched()` reads the API-reported total without materializing every page, and `planetary_computer.sign` over the lazy `ItemSearch` is the one canonical materialization-plus-sign (never the deprecated `get_all_items`, never a hand-rolled `next`-link follow loop the `ItemSearch` paging already owns, never a materialize-then-re-sign two-pass); the signed `ItemCollection` carries the signed hrefs the `[4]-[ASSETS]` fold reads without re-signing, and `min(item.properties["msft:expiry"])` over the signed items reports the token-validity horizon as catalog-signing evidence (`planetary-computer.md` L70); `pystac-client`/`pystac`/`planetary-computer` import function-local under `# noqa: PLC0415` per the manifest boundary-scope-only import policy (`pystac-client.md` L62, `pystac.md` L72, `planetary-computer.md` L63), the network search and the SAS token fetch the boundary the `RuntimeRail` traps.
- Receipt: `StacCatalog.discover` produces one `StacDiscovery` — the one outcome owner carrying the signed `ItemCollection`, the matched item-id tuple, the `matched()` count, the href count, the `msft:expiry` token horizon, and the `ContentKey` — whose `contribute()` emits an emitted-phase `Receipt.of` through `ReceiptContributor` keyed by `ContentIdentity` over the matched item-id set with the signed-href count and the token horizon as evidence, never a parallel result-versus-receipt pair.
- Packages: `pystac-client` (`Client.open(url, headers=, parameters=, modifier=, request_modifier=, stac_io=, timeout=)`, the one keyword-only `Client.search(*, method=, max_items=, limit=, ids=, collections=, bbox=, intersects=, datetime=, query=, filter=, filter_lang=, sortby=, fields=)` returning `ItemSearch`, `Client.collection_search(*, max_collections=, limit=, q=, query=, filter=, filter_lang=, sortby=, fields=)` returning `CollectionSearch`, `ItemSearch.{item_collection,items,pages,matched}` the item paging the `ITEM` `materialize` row reads, the `CollectionSearch` collection-materialization member the `COLLECTION` `materialize` row reads ([STAC_COLLECTION_SURFACE] RESEARCH)), `pystac` (`ItemCollection`/`Item`/`Asset` the search results hydrate into, the `Item.properties["msft:expiry"]` token horizon), `planetary-computer` (`sign` the `singledispatch` dispatching the lazy `ItemSearch` handle directly, `sign_inplace` the `modifier=` callable, `set_subscription_key` for the in-process key), runtime (`RuntimeRail`/`boundary`/`traversed`/`ContentIdentity`/`ContentKey`/`ReceiptContributor`/`Receipt`; the `ResourceRef` rides the `[4]-[ASSETS]` asset-byte fold, not the API search).
- Growth: a new search modality is one `StacQuery` case with one `params()` projection plus its key on the owning surface's `accepts` set; a new ordering field is a `sortby` entry on the existing `Order`; a new auth scheme is one `SignScheme` member plus one `_SCHEME` `SchemeRow` carrying its named `open_kwargs`/`sign`/`patch_url` callables, never three new `match` arms; a new discovery surface is one `Surface` member plus one `_SURFACE` `(method, cap, accepts, materialize)` row whose `accepts` set names the method's admissible keywords; free-text collection discovery already lands as the `FreeText` case flipping the `Surface` discriminant; zero new surface.
- Boundary: composes the runtime `ResourceRef`/`TransportResource` for the API credentials, never a second credential owner; the discovery owner folds INTO the `tabular/egress`/`gridded/virtual`/`tabular/columnar`/`spatial/geospatial`/`odc-stac` owners and never re-mints a STAC API paging loop, a CQL2 compiler, a SAS token fetch, or a conformance negotiator `pystac-client`/`planetary-computer` already owns; no live UI, no durable catalog store; a `search_by_<axis>` method family, a `cap`-keyword ternary fork where the `SurfaceRow` carries the keyword name, a blind `**params` splat onto `collection_search` where the `SurfaceRow.accepts` set filters the cross-surface keyword the method signature rejects, a `signing.sign(search) if surface is ITEM else ...` materialization branch where the `SurfaceRow.materialize` policy routes, a `planetary_computer.sign(CollectionSearch)`/`CollectionSearch.matched()` call where the surface carries no such member, a `routes_collection_search` boolean fork where the `Surface` discriminant routes, a positional `_SCHEME[scheme][0]` integer-index read where the named `SchemeRow` field resolves in one hop, three parallel `SignScheme` `match` statements where one `SchemeRow` carries the callables, a materialize-then-re-sign two-pass where the one `sign(ItemSearch)` dispatch fuses, a hand-rolled `next`-link page loop, the deprecated `get_all_items`, a hand-stitched SAS query-string concatenation, a bare forwarded `modifier` callable where `Signing` owns the strategy, a parallel result-versus-receipt struct pair, a signed-vs-unsigned client pair, an `AssetFold` iterating a `COLLECTION`-surface discovery as an asset source where the typed `surface` terminal rejects it, and a parallel API-vs-file client where `Client` extends `pystac.Catalog` are the deleted forms.

```python signature
from collections.abc import Callable
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import case, tag, tagged_union
from msgspec import Struct, field

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    from pystac import ItemCollection
    from pystac_client import CollectionSearch, ItemSearch

type Bound = tuple[float, float, float, float]
type Geometry = dict[str, object]
type Predicate = dict[str, object]
type Fields = dict[str, tuple[str, ...]]
type SearchParams = dict[str, object]
type Headers = dict[str, str]
type Modifier = Callable[[object], object]
type OpenKwargs = dict[str, object]
type Materialized = tuple["ItemCollection | CollectionSearch", tuple[str, ...], int, int, str | None]


def _pc(): import planetary_computer; return planetary_computer  # noqa: E704, PLC0415


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


_SCHEME: Final[dict[SignScheme, SchemeRow]] = {
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
}


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
    def planetary_computer(
        subscription_key: str | None = None, headers: Headers | None = None, timeout: float | None = None
    ) -> "Signing":
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
    return collection, item_ids, (search.matched() or len(item_ids)), href_count, expiry


def _materialize_collections(search: object, _: Signing) -> Materialized:
    collections = search.collection_list()
    return collections, tuple(c.id for c in collections), len(collections), 0, None


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

_SURFACE: Final[dict[Surface, SurfaceRow]] = {
    Surface.ITEM: SurfaceRow(
        method="search",
        cap="max_items",
        accepts=_SHARED | {"ids", "collections", "intersects"},
        materialize=_materialize_items,
    ),
    Surface.COLLECTION: SurfaceRow(
        method="collection_search",
        cap="max_collections",
        accepts=_SHARED | {"q"},
        materialize=_materialize_collections,
    ),
}


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
    collection: "ItemCollection | object"
    item_ids: tuple[str, ...]
    matched: int
    href_count: int
    expiry: str | None
    content_key: ContentKey

    def contribute(self) -> Receipt:
        return Receipt.of(
            "emitted",
            "catalog",
            self.endpoint,
            {
                "surface": self.surface.value,
                "items": str(len(self.item_ids)),
                "matched": str(self.matched),
                "hrefs": str(self.href_count),
                "expiry": self.expiry or "none",
            },
        )


class StacCatalog(Struct, frozen=True):
    endpoint: str
    signing: Signing = field(default_factory=Signing.none)

    @classmethod
    def open(cls, endpoint: str, signing: Signing | None = None) -> "StacCatalog":
        return cls(endpoint=endpoint, signing=signing or Signing.none())

    def discover(self, *queries: StacQuery, max_items: int | None = None, limit: int | None = None) -> "RuntimeRail[StacDiscovery]":
        return boundary("stac.discover", lambda: self._search(queries, max_items, limit))

    def _search(self, queries: "tuple[StacQuery, ...]", max_items: int | None, limit: int | None) -> StacDiscovery:
        from functools import reduce  # noqa: PLC0415

        from pystac_client import Client  # noqa: PLC0415

        client = Client.open(self.endpoint, **self.signing.open_kwargs())
        params = reduce(lambda acc, q: acc | q.params(), queries, {})
        surface = Surface.of_queries(queries)
        row = surface.row
        search = row.call(client, max_items, limit, params)
        collection, item_ids, matched, href_count, expiry = row.materialize(search, self.signing)
        return StacDiscovery(
            endpoint=self.endpoint,
            surface=surface,
            collection=collection,
            item_ids=item_ids,
            matched=matched,
            href_count=href_count,
            expiry=expiry,
            content_key=ContentIdentity.of("stac.discover", "\n".join(item_ids).encode()),
        )
```

## [03]-[TABLE]

- Owner: the `stac_table` encoder — the discovered `pystac.ItemCollection` to a `stac-geoparquet` columnar Arrow `pyarrow.RecordBatchReader`, the one carrier the `tabular/columnar.md#SCAN` scan and the `tabular/query.md#QUERY` engine consume. Encoding is the `stac_geoparquet.arrow.parse_stac_items_to_arrow` parse over the in-memory items; the NDJSON source is the sibling `parse_stac_ndjson_to_arrow` over the same carrier; GeoParquet egress over a materialized table is `stac_geoparquet.arrow.to_parquet` keyed by a `schema_version` from `SUPPORTED_PARQUET_SCHEMA_VERSIONS`; the one-call source-to-disk fast-paths are `parse_stac_items_to_parquet` (items), `parse_stac_ndjson_to_parquet` (NDJSON file), and `parse_stac_ndjson_to_delta_lake` (Delta sink); rehydration is `stac_geoparquet.arrow.stac_table_to_items` paired with `pystac.Item.from_dict`, and the NDJSON egress is `stac_table_to_ndjson` — never a hand-built Arrow schema where the `ACCEPTED_SCHEMA_OPTIONS` inference applies, never a hand-rolled parquet writer where `to_parquet` versions the schema, never a materialize-then-write two-hop where the one-call `stac_table_direct` path writes straight to disk, never the legacy geopandas trio where the zero-copy Arrow path is the canonical carrier.
- Cases: the source axis is one `TableSource` tagged union — `Items(items)` parses the in-memory `pystac.Item` iterable through `parse_stac_items_to_arrow` and `Ndjson(path, limit)` parses a STAC-NDJSON file through `parse_stac_ndjson_to_arrow`, both landing on the one `RecordBatchReader`, never two parallel table engines; the schema axis is the `ACCEPTED_SCHEMA_OPTIONS` string literal — `"FullFile"` scans every batch for the widest schema (the discovery default, correctness over a heterogeneous multi-collection result) and `"FirstBatch"` infers from the first batch (the lower-latency single-collection and direct-write fast-path) — a parameter row on the parse, not a parallel parse function; the sink axis is one `TableSink` tagged union — `Parquet(output_path, schema_version)` versions GeoParquet, `NdjsonOut(dest)` writes STAC-NDJSON, `DeltaLake(table_or_uri)` appends the Delta sink — each a single output descriptor, never a per-sink writer family. The sink rows route through two entrypoints keyed by whether an Arrow table is already in hand: `stac_table_egress` owns the table-in writes (`to_parquet`/`stac_table_to_ndjson` over the materialized `RecordBatchReader`/`Table`), while `stac_table_direct` owns the one-call source-to-disk fast-paths (`parse_stac_items_to_parquet`, `parse_stac_ndjson_to_parquet`, `parse_stac_ndjson_to_delta_lake`) that never materialize an intermediate reader — the Delta sink lives only on `stac_table_direct` because `parse_stac_ndjson_to_delta_lake` reads a STAC-NDJSON file rather than an Arrow table (`stac-geoparquet.md` L45), so its `table`-in arm on `stac_table_egress` is a typed reject rather than a silently-table-dropping write.
- Entry: `stac_table` folds one `TableSource` through `match`/`case` (with `chunk_size=DEFAULT_JSON_CHUNK_SIZE` and `schema="FullFile"`) into the `RecordBatchReader` and returns a `RuntimeRail[pa.RecordBatchReader]` the columnar scan registers directly; `stac_table_egress` folds the Arrow-table-in `TableSink` rows (`parquet`/`ndjson_out`, defaulting `schema_version` to `DEFAULT_PARQUET_SCHEMA_VERSION`) over an Arrow table to the path the `tabular/egress` `ObjectEgress.Put` then uploads, the `delta_lake` arm a typed reject routing to `stac_table_direct`, keyed by one `ContentIdentity`; `stac_table_direct` folds the `(TableSource, TableSink)` pair through one closed `match` over the catalogued one-call paths — `(items, parquet)` to `parse_stac_items_to_parquet`, `(ndjson, parquet)` to `parse_stac_ndjson_to_parquet`, `(ndjson, delta_lake)` to `parse_stac_ndjson_to_delta_lake` — with the `case _, _` arm a typed reject for a pair with no one-call surface, the discovery-to-disk fast-path that never materializes an intermediate reader; `stac_table_rehydrate` runs `stac_table_to_items(table)` over a read-back STAC table and rebuilds the model with `pystac.Item.from_dict`, never re-minting a STAC item the `pystac` owner already models.
- Auto: the item table crosses as the zero-copy `pyarrow.RecordBatchReader` so the columnar scan consumes it without a Python-side copy; `parse_stac_items_to_arrow` accepts the `pystac.Item` iterable directly (no NDJSON round-trip), and `to_parquet` stamps the GeoParquet STAC schema version so a downstream reader resolves the column layout without a side-channel; `stac_table_direct` collapses parse-and-write into one call for the discovery-to-disk fast-path where the intermediate reader is never scanned — `parse_stac_items_to_parquet` for the in-memory items, `parse_stac_ndjson_to_parquet` for the NDJSON file, both defaulting `schema="FirstBatch"` for the lower-latency direct write — so a discovery written straight to GeoParquet never pays the materialize-then-write two-hop the `stac_table`→`stac_table_egress` path costs; `stac-geoparquet` and `pystac` import function-local under `# noqa: PLC0415` per the boundary-scope-only import policy (`stac-geoparquet.md` L63), the parse/write the boundary the `RuntimeRail` traps; the `geopandas`-backed `to_geodataframe`/`to_item_collection` trio stays a legacy fallback never called here — the `arrow.*` namespace is the pure-Python carrier and is the one the page binds.
- Receipt: the table encode folds the shared `tabular/columnar` `QueryReceipt` (`tabular/columnar.md` `QueryReceipt`) over the encoded Arrow table keyed by `ContentIdentity`, the same receipt the columnar scan and spatial engine emit, never a parallel table-receipt rail.
- Packages: `stac-geoparquet` (`arrow.parse_stac_items_to_arrow(items, chunk_size=, schema=, tmpdir=)` returning `pa.RecordBatchReader`, `arrow.parse_stac_ndjson_to_arrow(path, *, chunk_size=, schema=, limit=)`, `arrow.parse_stac_items_to_parquet(items, *, chunk_size=, schema=, output_path=, schema_version=)`, `arrow.parse_stac_ndjson_to_parquet(input_path, output_path, *, chunk_size=, schema=, limit=, schema_version=)`, `arrow.to_parquet(table, output_path, *, schema_version=)`, `arrow.stac_table_to_items(table)`, `arrow.stac_table_to_ndjson(table, dest)`, `arrow.parse_stac_ndjson_to_delta_lake(input_path, table_or_uri)`, the `ACCEPTED_SCHEMA_OPTIONS`/`SUPPORTED_PARQUET_SCHEMA_VERSIONS`/`DEFAULT_PARQUET_SCHEMA_VERSION`/`DEFAULT_JSON_CHUNK_SIZE` schema and chunk axis), `pystac` (`Item.from_dict` rehydration), `pyarrow` (the `RecordBatchReader`/`Table` carrier), `expression` (`Error` the typed-reject rail leaf), runtime (`ContentIdentity`/`BoundaryFault`/`RuntimeRail`/`boundary`).
- Growth: a new schema-inference mode is the `ACCEPTED_SCHEMA_OPTIONS` row on the parse; a new source is one `TableSource` case; a new sink is one `TableSink` case plus its `stac_table_egress` or `stac_table_direct` arm; a new one-call fast-path is one `(source, sink)` arm on `stac_table_direct`; a new GeoParquet schema version is a `schema_version` from `SUPPORTED_PARQUET_SCHEMA_VERSIONS`; zero new surface.
- Boundary: composes the `tabular/columnar`/`tabular/query` owners for the table scan and the `tabular/egress` owner for the GeoParquet upload, never a second table engine or a second writer; no durable catalog store; a hand-built STAC-to-Arrow schema mapping, a hand-rolled parquet writer, re-minting a STAC item where `from_dict` rehydrates, a per-sink writer family where `TableSink` discriminates, a materialize-then-write two-hop where the one-call `stac_table_direct` path writes straight to disk, a `delta_lake` arm on `stac_table_egress` that silently drops the in-memory `table` argument where `parse_stac_ndjson_to_delta_lake` reads an NDJSON file, and defaulting to the geopandas trio where the zero-copy Arrow path applies are the deleted forms.

```python signature
from typing import TYPE_CHECKING, Literal, assert_never

import pyarrow as pa
from expression import Error, Ok, case, tag, tagged_union

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
                return Ok(ContentIdentity.of("stac.geoparquet", Path(output_path).read_bytes()))
            case TableSink(tag="ndjson_out", ndjson_out=dest):
                stac_table_to_ndjson(table, dest)
                return Ok(ContentIdentity.of("stac.ndjson", Path(dest).read_bytes()))
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
                parse_stac_items_to_parquet(items, chunk_size=DEFAULT_JSON_CHUNK_SIZE, schema=schema, output_path=output_path, schema_version=schema_version or DEFAULT_PARQUET_SCHEMA_VERSION)
                return Ok(ContentIdentity.of("stac.geoparquet", Path(output_path).read_bytes()))
            case TableSource(tag="ndjson", ndjson=(path, limit)), TableSink(tag="parquet", parquet=(output_path, schema_version)):
                parse_stac_ndjson_to_parquet(path, output_path, chunk_size=DEFAULT_JSON_CHUNK_SIZE, limit=limit, schema_version=schema_version or DEFAULT_PARQUET_SCHEMA_VERSION)
                return Ok(ContentIdentity.of("stac.geoparquet", Path(output_path).read_bytes()))
            case TableSource(tag="ndjson", ndjson=(path, _)), TableSink(tag="delta_lake", delta_lake=table_or_uri):
                parse_stac_ndjson_to_delta_lake(path, table_or_uri)
                return Ok(ContentIdentity.of("stac.delta", f"{path}->{table_or_uri}".encode()))
            case _, _:
                return Error(BoundaryFault(boundary=("stac.table.direct", f"no one-call path for ({source.tag}, {sink.tag}), route through stac_table then stac_table_egress")))

    return boundary("stac.table.direct", _fuse).bind(lambda rail: rail)


def stac_table_rehydrate(table: "pa.Table") -> "RuntimeRail[tuple[object, ...]]":
    def _rehydrate() -> "tuple[object, ...]":
        import pystac  # noqa: PLC0415
        from stac_geoparquet.arrow import stac_table_to_items  # noqa: PLC0415

        return tuple(pystac.Item.from_dict(record) for record in stac_table_to_items(table))

    return boundary("stac.rehydrate", _rehydrate)
```

## [04]-[ASSETS]

- Owner: `AssetFold` — one fold owner over the signed `StacDiscovery.collection` discriminating the `FoldTarget` tagged-union axis (`Egress`/`Cube`/`Coverage`) INTO the settled downstream seams, NOT a new transport. The `Egress` target reads only the intersecting COG/GeoTIFF byte windows through the `tabular/egress.md#EGRESS` `ObjectEgress.run(StoreOp.GetRange(...))` fence; the `Cube` target lazily references the cube-bearing asset hrefs through the `gridded/virtual.md#VIRTUAL` `VirtualReference.aggregate(sources, ref, concat_dim=)` fence; the `Coverage` target reads `proj:epsg`/`raster:bands`/`eo:cloud_cover` through the typed `pystac.extensions` accessors into one `RasterGeoClaim` plus the matching `stac_cfg`, then drives the `odc-stac` `odc.stac.load(items, stac_cfg=, patch_url=, groupby=, resampling=, chunks=)` COG datacube with the `Signing.patch_url()` threaded so the COG reads are SAS-signed. The two settled-rail targets thread the inner `RuntimeRail` they compose — the `Egress` arm folds its per-window `ObjectEgress.run` rails through `traversed(..., accumulate=False)` so the first byte-window fault aborts the whole fold, the `Cube` arm `.map`s the `VirtualReference.aggregate` rail — while the `Coverage` arm runs its boundary kernel directly; all three close through one `_rekey` re-minting the one `StacDiscovery`; never a swallowed inner rail, second object-store transport, virtual-cube builder, COG loader, SAS token owner, or hand-built band table.
- Cases: `FoldTarget` rows `Egress(egress, windows)` (each raster asset whose `media_type` is a `pystac.MediaType.COG`/`MediaType.GEOTIFF` enum row folds to `ObjectEgress.run(StoreOp.GetRange(href, start, end))` over the COG header/overview/tile byte ranges the `windows` map carries) · `Cube(ref, concat_dim)` (the cube-bearing asset hrefs fold to `VirtualReference.aggregate(sources, ref, concat_dim=concat_dim)` building the one virtual-reference datacube over the source URLs) · `Coverage(groupby, resampling, chunks)` (the `odc.stac.load` COG datacube over the discovered items driven by the extension-derived `stac_cfg`/`patch_url`) — the target value IS the route, never a parallel per-asset egress class. The shared `_raster_hrefs` generator yields the `(asset, href)` pairs whose `media_type` matches the `{MediaType.COG, MediaType.GEOTIFF}` enum set (`pystac.md` L33/L76), the one media-type gate for both the `Egress` and `Cube` arms, never a per-arm raw-MIME `frozenset` re-stated twice. The extension read discriminates by extension namespace — `ProjectionExtension.ext(item)` for `proj:epsg`/`proj:transform`, `EOExtension.ext(item)` for `eo:cloud_cover`/`eo:bands`, and `RasterExtension.ext(asset)` for `raster:bands`/nodata — each a typed accessor row applied through `.ext(obj, add_if_missing=False)`, never a raw `item.properties["proj:epsg"]` dictionary probe.
- Entry: `AssetFold.over` admits a `StacDiscovery` and one `FoldTarget`, returning a `RuntimeRail[StacDiscovery]` that folds the target over the signed `collection`; the head guard returns `Error(BoundaryFault(boundary=("stac.assets", ...)))` when `self.discovery.surface is Surface.COLLECTION` so a collection-discovery terminal never reaches the asset arms, then `_raster_hrefs` materializes the gated `sources` tuple once and the three arms read it. The `Egress` arm folds the `windows.get(href)` walrus-gated windowed assets into a `Block` of `egress.run(StoreOp.GetRange(href, start, end), path=href)` rails and threads them through `traversed(rails, accumulate=False)` so the fold aborts on the first byte-window fault and never swallows a failed range read; the egress owner short-circuits an unchanged content-key to a by-reference no-op, so a re-discovery never re-reads. The `Cube` arm `.map`s the `VirtualReference.aggregate(sources, ref, concat_dim=concat_dim)` rail, reading the real `VirtualReceipt.chunk_refs` manifest count off the resolved receipt rather than a bare `len(sources)`. The `Coverage` arm runs `_coverage` in one `boundary` kernel, reading the `ProjectionExtension`/`EOExtension`/`RasterExtension` accessors of a sample item into one `RasterGeoClaim` (`spatial/geospatial.md#GEO`) and the matching `stac_cfg`, then binding `odc.stac.load(items, stac_cfg=cfg, patch_url=signing.patch_url(), groupby=groupby, resampling=resampling, chunks=chunks)` so the cube load reads its CRS/bands/nodata from the catalogue rather than a hand-fed config. Every arm closes through one `_rekey(tag, payload, folded)` re-minting the `StacDiscovery` with every field of the source discovery preserved (`item_ids`, `matched`, `expiry`) and a fold-target-specific `ContentIdentity.of(f"stac.assets.{tag}", payload)` plus the real folded count — the `Egress` payload the joined source hrefs and the folded count the summed `EgressReceipt.byte_length`, the `Cube` payload the `concat_dim`-prefixed source join and the count the manifest `chunk_refs`, the `Coverage` payload the claim CRS plus the loaded cube's `sizes` shape and the count the `band_count` — so a coverage of a discovery is byte-distinct from its egress and a single new asset flips the key, the no-op re-key and the swallowed inner rail the deleted forms.
- Auto: the byte window is the COG/GeoTIFF archival range the `windows` map carries (the GeoTIFF IFD header/overview/tile byte offsets), passed straight to `GetRange` so the read is one HTTP range request, never a full-object materialization; the virtual cube reuses the `gridded/virtual` owner's mandatory `ObjectStoreRegistry` URL-to-`obstore` backend map internal to `aggregate`, so the STAC asset URLs register through the same `from_url` transport the egress owner speaks — one transport across discovery, egress, and cube; the extension accessors resolve the typed `pystac.extensions` schema rather than the raw `properties` map, so a missing extension is a typed absence not a `KeyError`; the `RasterGeoClaim` carries the `proj:epsg` CRS, the `raster:bands` nodata, and the `eo:bands` band count, the `ClaimBundle` carries the `eo:cloud_cover` scene fraction beside the claim (a coverage knob the sibling-owned `RasterGeoClaim` shape carries no slot for), the `stac_cfg` carries the per-asset nodata into `odc.stac.load`, and the `Signing.patch_url()` rides `patch_url=` so the COG reads are SAS-signed by the same `planetary_computer.sign` that signed the discovery result; the loaded `xarray.Dataset` cube's `sizes` shape, the `ClaimBundle` CRS, and the `eo:cloud_cover` fraction fold into the `Coverage` content key so a cloudy and a clear scene of the same bbox key byte-distinct and the cube identity is carried forward, never discarded.
- Receipt: the asset fold re-mints the one `StacDiscovery` declared in `[2]-[CATALOG]` keyed by the fold-target `ContentIdentity` over its arm-specific payload plus the folded count, contributing an emitted-phase `Receipt.of` through `ReceiptContributor`; no new receipt rail.
- Packages: `pystac` (`Item.assets`/`Asset.href`/`Asset.media_type`/`MediaType.COG`/`MediaType.GEOTIFF`, `extensions.projection.ProjectionExtension.ext`/`extensions.eo.EOExtension.ext`/`extensions.raster.RasterExtension.ext`), `odc-stac` (`odc.stac.load(items, *, stac_cfg=, patch_url=, groupby=, resampling=, chunks=, bands=)`, `odc-stac.md` L41), `tabular/egress` (`ObjectEgress.run` returning `RuntimeRail[EgressReceipt]` whose `byte_length` the egress fold sums, `StoreOp.GetRange`, `tabular/egress.md#EGRESS` L12), `gridded/virtual` (`VirtualReference.aggregate` returning `RuntimeRail[VirtualReceipt]` whose `chunk_refs` the cube fold reads, `gridded/virtual.md#VIRTUAL`), `spatial/geospatial` (`RasterGeoClaim`, `spatial/geospatial.md#GEO`), `spatial/catalog` (`Surface` the discovery-method discriminant the terminal guard reads), `expression` (`Block.empty`/`Block.singleton`/`append` the rail-block construction, `Error` the terminal-reject rail leaf), runtime (`ContentIdentity`/`RuntimeRail`/`BoundaryFault`/`boundary`/`traversed`/`ReceiptContributor`).
- Growth: a new archival format is one more `MediaType` member in the `_raster_hrefs` gate routed to the `Egress` `GetRange`; a new cube engine is the `gridded/virtual` parser swap with zero change here; a new coverage knob is one field on the `Coverage` row; a new extension read is one more typed accessor row in `_claim`; a per-collection cube is one `concat_dim` on the `Cube` row; zero new surface.
- Boundary: reads the settled `tabular/egress` `GetRange`, `gridded/virtual` `aggregate`, `spatial/geospatial` `RasterGeoClaim`, and `odc-stac` `load` fences and never re-mints any — no second object-store transport, no second virtual-cube builder, no second COG loader, no second raster claim, no full-object read where a byte window applies, no durable asset store; a per-asset egress class family, a re-derived virtual cube, a hand-rolled COG reader where `odc.stac.load` owns it, a raw `properties` dictionary probe where the typed extension accessor applies, a raw-MIME `frozenset` where `MediaType.COG`/`GEOTIFF` resolves, an imperative `for`/`if` asset loop where the rail-block fold threads, a swallowed `ObjectEgress.run`/`VirtualReference.aggregate` inner rail whose fault the boundary kernel never sees, a `len(sources)`/`len(reads)` fold count where the real `EgressReceipt.byte_length` sum and `VirtualReceipt.chunk_refs` manifest count are the evidence, a `_rekey` dropping `expiry`/`matched`, a no-op re-key that discards the fold outcome, and a hand-fed `stac_cfg` where the extension read derives it are the deleted forms.

```python signature
from functools import reduce
from typing import TYPE_CHECKING, Literal, assert_never

from expression import Error, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct

from rasm.data.catalog import Signing, StacDiscovery, Surface
from rasm.data.gridded.virtual import VirtualReference
from rasm.data.spatial.geospatial import RasterGeoClaim
from rasm.data.tabular.egress import ObjectEgress, StoreOp
from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import BoundaryFault, RuntimeRail, boundary, traversed
from rasm.runtime.roots import ResourceRef

if TYPE_CHECKING:
    from collections.abc import Iterator, Mapping

type StacCfg = dict[str, dict[str, object]]
type Window = tuple[int, int]


def _raster_hrefs(collection: object) -> "Iterator[tuple[object, str]]":
    from pystac import MediaType  # noqa: PLC0415

    raster = {MediaType.COG, MediaType.GEOTIFF}
    return (
        (asset, asset.href)
        for item in collection
        for asset in item.assets.values()
        if asset.media_type in raster
    )


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
    def Coverage(groupby: str = "time", resampling: str = "nearest", chunks: "dict[str, int] | None" = None) -> "FoldTarget":
        return FoldTarget(coverage=(groupby, resampling, chunks))


class AssetFold(Struct, frozen=True):
    discovery: StacDiscovery
    signing: Signing

    def over(self, target: FoldTarget) -> "RuntimeRail[StacDiscovery]":
        if self.discovery.surface is Surface.COLLECTION:
            return Error(BoundaryFault(boundary=("stac.assets", "collection-discovery is a terminal, re-enter an item search before the asset fold")))
        sources = tuple(href for _, href in _raster_hrefs(self.discovery.collection))
        match target:
            case FoldTarget(tag="egress", egress=(egress, windows)):
                windowed = tuple((href, w) for href in sources if (w := windows.get(href)) is not None)
                rails = reduce(
                    lambda acc, hw: acc.append(Block.singleton(egress.run(StoreOp.GetRange(hw[0], hw[1][0], hw[1][1]), path=hw[0]))),
                    windowed,
                    Block.empty(),
                )
                return traversed(rails, accumulate=False).map(
                    lambda receipts: self._rekey("egress", "\n".join(h for h, _ in windowed).encode(), sum(r.byte_length for r in receipts))
                )
            case FoldTarget(tag="cube", cube=(ref, concat_dim)):
                return VirtualReference.aggregate(sources, ref, concat_dim=concat_dim).map(
                    lambda receipt: self._rekey("cube", f"{concat_dim}|{'|'.join(sources)}".encode(), receipt.chunk_refs)
                )
            case FoldTarget(tag="coverage", coverage=(groupby, resampling, chunks)):
                return boundary(f"stac.assets.{target.tag}", lambda: self._coverage(groupby, resampling, chunks))
            case unreachable:
                assert_never(unreachable)

    def _coverage(self, groupby: str, resampling: str, chunks: "dict[str, int] | None") -> StacDiscovery:
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

    def _rekey(self, tag: str, payload: bytes, folded: int) -> StacDiscovery:
        return StacDiscovery(
            endpoint=self.discovery.endpoint,
            surface=self.discovery.surface,
            collection=self.discovery.collection,
            item_ids=self.discovery.item_ids,
            matched=self.discovery.matched,
            href_count=folded,
            expiry=self.discovery.expiry,
            content_key=ContentIdentity.of(f"stac.assets.{tag}", payload),
        )

    @staticmethod
    def _claim(sample: object, resampling: str, collection: str = "default") -> ClaimBundle:
        from pystac.extensions.eo import EOExtension  # noqa: PLC0415
        from pystac.extensions.projection import ProjectionExtension  # noqa: PLC0415
        from pystac.extensions.raster import RasterExtension  # noqa: PLC0415

        projection = ProjectionExtension.ext(sample, add_if_missing=False)
        eo = EOExtension.ext(sample, add_if_missing=False)
        assets = tuple(sample.assets.values())
        raster = RasterExtension.ext(assets[0], add_if_missing=False)
        bands = raster.bands or ()
        nodata = next((band.nodata for band in bands if band.nodata is not None), 0.0)
        claim = RasterGeoClaim(
            crs=f"EPSG:{projection.epsg}",
            band_count=len(eo.bands or bands),
            resampling=resampling,
            nodata=float(nodata),
            transform=tuple(projection.transform or ()),
        )
        stac_cfg: StacCfg = {collection: {"assets": {asset.title or asset.href: {"nodata": float(nodata)} for asset in assets}}}
        return ClaimBundle(claim=claim, stac_cfg=stac_cfg, cloud_cover=eo.cloud_cover)
```

## [05]-[RESEARCH]

- [STAC_DISTRIBUTION_SYNC]: `planetary-computer` `1.0.0` (`planetary-computer.md` L12) and `odc-stac` `0.5.2` (`odc-stac.md` L12) are catalogue-verified settled — both reflected on cp315, the `planetary_computer.{sign,sign_inplace,set_subscription_key}` signing surface (`sign(obj, copy=True)` the `singledispatch` over `str`/`Asset`/`Item`/`ItemCollection`/`Collection`/`ItemSearch`, `planetary-computer.md` L39-44) and the `odc.stac.load(items, *, stac_cfg=, patch_url=, groupby=, resampling=, chunks=, bands=)` cube surface (`odc-stac.md` L41) transcribed as settled fence code. The three `pystac-client`/`pystac`/`stac-geoparquet` catalogues are reflection-grade authored from the canonical sources and lock-resolved (all pure-Python `py3-none-any`, cp315-clean, ungated) but the active venv sync is pending — the member surfaces the fences transcribe (`Client.open(url, *, headers=, modifier=)`/`Client.search(*, method=, ids=, bbox=, intersects=, datetime=, collections=, query=, filter=, filter_lang=, sortby=, fields=, max_items=, limit=)` returning `ItemSearch`, `Client.collection_search(*, q=, bbox=, datetime=, query=, filter=, filter_lang=, sortby=, fields=, max_collections=, limit=)` returning `CollectionSearch` and sharing the `search` filter/order axis (`pystac-client.md` L40/L43), `ItemSearch.{item_collection,matched}`, `arrow.{parse_stac_items_to_arrow,parse_stac_ndjson_to_arrow,parse_stac_items_to_parquet,parse_stac_ndjson_to_parquet,to_parquet,stac_table_to_items,stac_table_to_ndjson,parse_stac_ndjson_to_delta_lake}`, `arrow.{ACCEPTED_SCHEMA_OPTIONS,DEFAULT_PARQUET_SCHEMA_VERSION,SUPPORTED_PARQUET_SCHEMA_VERSIONS,DEFAULT_JSON_CHUNK_SIZE}`, `pystac.ItemCollection`, `pystac.Item.{assets,from_dict}`, `pystac.MediaType.{COG,GEOTIFF}`, `pystac.extensions.{projection.ProjectionExtension,eo.EOExtension,raster.RasterExtension}.ext`) confirm against the synced `pystac-client`/`pystac`/`stac-geoparquet` distributions before they settle; the `ids` keyword and the `MediaType.COG`/`MediaType.GEOTIFF` enum members are the catalogue-named rows (`pystac-client.md` L40, `pystac.md` L33).
- [STAC_SIGN_DISPATCH]: `Signing.sign(search)` runs `planetary_computer.sign` over the lazy `pystac_client.ItemSearch` handle itself (the catalogued `singledispatch` registers `ItemSearch`, returning a fresh signed `ItemCollection` in one materialization, `planetary-computer.md` L44/L64), so the discovery result carries signed hrefs the `[4]-[ASSETS]` fold reads with neither a second sign pass nor a materialize-then-sign two-pass — the `NONE` row's `search.item_collection()` is the matching unsigned materialization (`pystac-client.md` L52), so both scheme rows return one `ItemCollection`; the one `SignScheme.row` projects the `SchemeRow` carrying the named `open_kwargs`/`sign`/`patch_url` callables so `Signing.open_kwargs` threads `modifier=planetary_computer.sign_inplace` (`sign_inplace(obj: Any) -> Any`, `planetary-computer.md` L40) plus `headers=`/`timeout=`, `Signing.sign` dispatches `planetary_computer.sign(ItemSearch)`, and `Signing.patch_url` returns `planetary_computer.sign` for `odc.stac.load(patch_url=)` — all three projections catalogue-settled and collapsed onto one frozen `SchemeRow` read by name rather than three parallel `match` statements; the `SignScheme` `StrEnum` row carries the strategy so `Signing` admits onto the frozen `StacCatalog` with no callable field. The idempotence over already-signed (`st`/`se`/`sp` present) and non-Azure hrefs is the catalogued selectivity (`planetary-computer.md` L66), so a re-sign of the discovery result is a no-op. The `msft:expiry` token horizon `StacDiscovery.expiry` reads off `Item.properties["msft:expiry"]` (the catalogued ISO-8601 expiry the SAS signing writes onto each signed item, `planetary-computer.md` L70) — the property key confirms against the synced `planetary-computer`/`pystac` distributions before the `min(...)` horizon read treats it as settled, the `sign(ItemSearch)` dispatch and `sign_inplace` modifier already settled.
- [STAC_EXTENSION_PROPERTIES]: the typed extension property accessors the `AssetFold._claim` fence reads (`ProjectionExtension.ext(item).epsg`/`.transform`, `EOExtension.ext(item).bands`/`.cloud_cover`, `RasterExtension.ext(asset).bands` with each band's `.nodata`) are standard `pystac` extension members but the catalogue (`pystac.md` L65-67) confirms only the `.ext(obj, add_if_missing=False)` accessor and the EPSG/bands/nodata/cloud-cover capability, not the exact property-attribute spelling — the `.epsg`, `.transform`, `.cloud_cover`, `.bands`, and per-band `.nodata` attribute names confirm against the synced `pystac.extensions` distribution before the `RasterGeoClaim` CRS/band/nodata read and the `ClaimBundle.cloud_cover` scene-fraction read treat them as settled, the `add_if_missing=False` accessor itself already settled.
- [STAC_CONFORMANCE_SURFACES]: the CQL2-JSON `filter_lang="cql2-json"` value, the legacy `query`-extension predicate the `Cql2Query` arm spreads, the `sortby`/`fields` ordering and projection the `Order` arm spreads, and the `eo:cloud_cover`/`proj:epsg` property predicates are STAC API conformance-class surfaces the server evaluates, confirmed against the target endpoint's advertised conformance before the `Cql2Filter`/`Cql2Query`/`Order` arms treat the predicate, ordering, or projection as accepted; the `intersects` GeoJSON-geometry `dict[str, object]` payload stays shapely-free at the boundary, the `Client.search(intersects=)` keyword admitting the raw GeoJSON mapping directly (`pystac-client.md` L40).
- [STAC_COLLECTION_SURFACE]: the `COLLECTION` `SurfaceRow.materialize` policy (`_materialize_collections`) routes the free-text union to `Client.collection_search` returning `CollectionSearch` (`pystac-client.md` L43) and materializes its collections without signing — `planetary_computer.sign` registers no `CollectionSearch` overload (the catalogued `singledispatch` covers `str`/`Asset`/`Item`/`ItemCollection`/`Collection`/`ItemSearch`/`Mapping` only, `planetary-computer.md` L39/L64) and `matched()` is an `ItemSearch`-only member (`pystac-client.md` L57), so the `COLLECTION` leg correctly omits both, which is the settled architectural decision. The `SurfaceRow.accepts` set names exactly the `collection_search` signature keywords (`bbox`/`datetime`/`query`/`filter`/`filter_lang`/`sortby`/`fields`/`q`, `pystac-client.md` L43), so a `FreeText` union that also folds an `Ids`/`Intersects`/`Collection` row contributes those keys to `params` but the row filter drops them before `collection_search`, the `_SHARED` ∪ `{q}` admission the one boundary that keeps the union total across the surface flip rather than a runtime `TypeError` on an unexpected keyword; the `ITEM` row's `_SHARED` ∪ `{ids, collections, intersects}` set is the `search` signature (`pystac-client.md` L40). The `CollectionSearch` materialization member the row reads (transcribed `collection_list()`) is NOT catalogued — `pystac-client.md` enumerates the `ItemSearch` paging members (`item_collection`/`items`/`pages`/`matched`) but not the `CollectionSearch` materialization surface — so the exact collection-paging spelling confirms against the synced `pystac-client` distribution before `_materialize_collections` treats it as settled; the `collection_search` keyword axis and its `CollectionSearch` return are catalogue-settled, only the result-materialization member name is unverified. The `COLLECTION` carrier is a collection set, not an `ItemCollection`, so the `StacDiscovery.collection` field is typed `ItemCollection | object` and the outcome carries a `surface: Surface` discriminant — `item_ids` reads each collection's `.id`, `href_count` is zero because a STAC `Collection` carries no `.assets`, and `expiry` is `None` because collection metadata carries no `msft:expiry` — and the discovery-of-collections terminal is structural rather than prose: `AssetFold.over` returns `Error(BoundaryFault(boundary=("stac.assets", ...)))` on a `Surface.COLLECTION` discovery, so the resolved collection ids re-enter an item `search` before `[4]-[ASSETS]` runs, the collection surface never an item-asset source.
