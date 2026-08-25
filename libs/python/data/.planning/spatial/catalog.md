# [PY_DATA_CATALOG]

Cloud-native STAC discovery owner: one `StacCatalog` over `pystac-client` resolving which cloud assets cover a query region — the discovery layer above the raster/vector claims (`spatial/geospatial#GEO`) and the archival byte-window read (`tabular/egress#EGRESS`) that the geospatial and object-store lanes lack. `StacCatalog.discover` folds the one `StacQuery` search axis onto the single keyword-only `Client.search`, the `Surface` discriminant alone routing the shared rows to `Client.collection_search` on a `FreeText` case, pages the lazy `ItemSearch` into a `pystac.ItemCollection`, and emits one `StacDiscovery` keyed by a runtime `ContentIdentity` over the matched item-id set. One `Signing` value encodes the request boundary as a frozen `SchemeRow` read by name — into `Client.open(modifier=)` for discovery, over the `ItemSearch` for asset hrefs, and into `odc.stac.load(patch_url=)` for the COG load — never a signed-vs-unsigned client pair.

Discovered collections encode as a `stac-geoparquet` columnar Arrow `RecordBatchReader` (`[3]-[TABLE]`) the `tabular/columnar#SCAN` scan and `tabular/query#QUERY` engine consume, and the asset hrefs fold (`[4]-[ASSETS]`) into the `tabular/egress#EGRESS` owner over the `runtime/transport/roots#STORE` `StoreOp.GetRange` byte-window read, the `gridded/virtual#VIRTUAL` `VirtualReference.apply` virtual-chunk registration over the `gridded/virtual#MANIFEST` `FieldVirtual` manifest, and the `odc-stac` `odc.stac.load` COG datacube driven by the catalogue-derived `stac_cfg`/`patch_url`. Discovery, durable writes, and asset folds key through runtime `ContentIdentity`; the network legs ride the runtime `guarded(RetryClass.HTTP, ...)` envelope, never a second object-store transport, virtual-cube builder, COG loader, or hand-fed band metadata.

## [01]-[INDEX]

- [02]-[CATALOG]: the `StacCatalog` discovery owner over `pystac-client`, the `StacQuery` search axis with the `Surface` discriminant routing item-vs-collection search, emitting one `StacDiscovery`.
- [03]-[TABLE]: the one `StacGeoClaim` STAC-table owner over `stac-geoparquet`, folding a closed `StacTableOp` axis both directions behind one `apply` over a `(source, sink)` capability roster.
- [04]-[ASSETS]: the one awaitable `AssetFold` routing signed hrefs into egress byte-windows, virtual cube chunks, or the `odc-stac` COG datacube.

## [02]-[CATALOG]

- Owner: `StacCatalog` — the one cloud-native discovery owner, a `Client.open`-bound STAC API root carrying one `Signing`. `Signing` holds TWO orthogonal axes on one value: `scheme` names the href-REWRITE dispatch (a `SchemeRow` read by name) and `credentials` the obstore-native provider the asset-BYTE reads bind — Planetary Computer does both, NASA Earthdata mints storage credentials and rewrites nothing, a public catalog does neither, so neither axis is a member of the other's vocabulary and no fake scheme row stands in for a credential-only catalog. `StacQuery` is the tagged-union search axis folded by `match`/`case` onto the single keyword-only `Client.search`; a new search modality is one case, never a `search_bbox`/`search_intersects`/`search_cql2` method family. `Surface` is the discovery-method discriminant carrying its own frozen `SurfaceRow` `(method, cap, accepts, materialize)`: the `accepts` keyword-admission set is the boundary that keeps the union total across surfaces — an `ids`/`intersects`/`collections` row a `FreeText` union folds in never reaches `collection_search`, which rejects it — and the `materialize` policy owns the structural divergence between the two iterators, since the `ITEM` row signs the `ItemSearch` and reads `matched()` while a `CollectionSearch` carries no Azure hrefs and no `sign`/`matched` member, yielding zero hrefs and typed absence for every fact it cannot answer. The credential axis rides the branch `Option` and lowers to the runtime's own nullable `Provider` spelling at one projection, `Signing.provider()`, because `ResourceRef` is the owner of that admission and no asset arm re-derives it. `Signing` encodes the request boundary as one frozen `SchemeRow` (`NONE`/`PLANETARY_COMPUTER`) whose `open_kwargs`/`sign`/`patch_url` callables are read by name, never a positional triple, parallel `match` statements, or a forwarded bare callable.
- Cases: `StacQuery` rows — `Bbox`, `Intersects` (a GeoJSON-geometry dict the server intersects server-side, no shapely at the boundary), `Datetime` (an RFC-3339 interval), `Ids`, `Collection`, `Cql2Filter` (a CQL2-JSON predicate the STAC API evaluates server-side), `Cql2Query` (the legacy `query` extension), `Order` (server-side sort with field projection), and `FreeText` (whose presence flips `Surface` from `ITEM` to `COLLECTION`, routing to `Client.collection_search`). Each carries a `params()` projection contributing exactly its own keyword arguments, so an n-axis query unions the per-case keyword dicts rather than forking a method per axis; because `collection_search` shares the `bbox`/`datetime`/`query`/`filter`/`filter_lang`/`sortby`/`fields` axis with `search`, the shared rows union onto either surface unchanged, the surface alone differing.
- Entry: `StacCatalog.discover` computes the pure plan — reduces the query tuple's `params()` into one keyword set and recovers `Surface.of_queries` and its `.row` — then drives the whole blocking `pystac_client` sequence (`Client.open`, `row.call`, `row.materialize`) through one `guarded(RetryClass.HTTP, on_thread, ...)` envelope, the `THREAD_BAND`-bounded hop, so the synchronous I/O never stalls the event loop and the transient `429`/`5xx`/timeout set retries under a `Retry-After`-honouring backoff as one logical discovery. `row.call` reads the method, cap, and `accepts` keyword filter off the `SurfaceRow` so a cross-surface param never reaches a method that rejects it, and `row.materialize` returns the full shaped `(collection, item_ids, matched, href_count, expiry, url)` outcome — the `ITEM` row's one `planetary_computer.sign(ItemSearch)` dispatch both materializes the page and rewrites every Azure blob href in a single pass, reports `matched()` and the resolved-GET `url_with_parameters()`, and reads each item's `msft:expiry`; the `COLLECTION` row materializes without signing, yielding a zero href count and three ABSENCES — no server total, no expiry horizon, no resolved url — never its own page length wearing the archive total's name. Results flatten through `.bind(self._shape(surface))`, which folds the railed `ContentIdentity.of` over the item-id set through `.map` into one `StacDiscovery` rather than stuffing a `RuntimeRail[ContentKey]` into the `content_key` field.
- Auto: the `params` fold is the union law — a bbox+datetime+cloud-cover+order query is one `search`, never four; `Surface.of_queries` flips to `collection_search` exactly when a `FreeText` case is present, the one boolean-free routing read, never a `search_by_<axis>` family. `ItemSearch` is lazy so `matched()` reads the API total without materializing every page, and `sign` over the lazy handle is the one canonical materialize-plus-sign — never the deprecated `get_all_items`, never a `next`-link follow loop, never a materialize-then-re-sign two-pass. `min(msft:expiry)` over the signed items reports the token-validity horizon as EVIDENCE alone: the bound credential provider owns refresh inside the store handle, so a fan-out outliving that window re-signs transparently rather than failing mid-read against a horizon this owner could only report and never renew. `pystac-client`/`pystac`/`planetary-computer` and both `obstore.auth` providers declare once at module scope under `lazy import`/`lazy from` and reify on first use, so the signing band costs nothing until a discovery runs; every `_SCHEME` cell reads the provider inside a call-time thunk, because a module-scope cell holding a live attribute reifies the proxy at import and re-opens the eager band the manifest bans. The runtime rails ride eager module-level.
- Output: `StacDiscovery` carries the signed `ItemCollection`, the local item-id census, the SERVER's own `matched()` total as its own optional fact, the href count, the `msft:expiry` horizon, the resolved `url`, and the `ContentKey`.
- Packages: `pystac-client` (the keyword-only `Client.search`/`collection_search`, `ItemSearch.{item_collection,matched,url_with_parameters}` the `ITEM` row reads, `CollectionSearch.collection_list` the `COLLECTION` row reads), `pystac` (`ItemCollection`/`Item`/`Asset`, the `msft:expiry` token horizon), `planetary-computer` (`sign` the `singledispatch` over the lazy `ItemSearch`, `sign_inplace` the `modifier=` callable, `set_subscription_key`), `obstore` (`auth.planetary_computer.PlanetaryComputerAsyncCredentialProvider` and `auth.earthdata.NasaEarthdataAsyncCredentialProvider`, the two providers that own token refresh inside the store rather than beside it), runtime (`RuntimeRail`/`ContentIdentity`/`ContentKey`/`Provider`/`RetryClass`/`guarded`/`on_thread`).
- Law: the subscription key is composition-bound on BOTH halves — the byte-read half rides each `Signing`'s own obstore provider, and the href-rewrite half rides the one process slot `planetary_computer.sign` reads behind the `_bind_subscription` compare-and-refuse latch, so two same-process apps with divergent keys collide at the factory as a typed `subscription-key-collision` refusal instead of the second app silently re-signing the first's hrefs; the factory therefore answers the rail, exactly like every admission that can refuse.
- Growth: a new search modality is one `StacQuery` case with its key on the owning surface's `accepts` set; a new href-rewrite scheme is one `SignScheme` member with its `SchemeRow`; a new credential estate is one `Signing` factory binding its own obstore provider with no `SignScheme` member at all; a new discovery surface is one `Surface` member with its `SurfaceRow` whose `accepts` names the method's admissible keywords; zero new surface; a new fenced leg or refusal law is one `FaultRow` row under `DataLeg.CATALOG` in this module's one `RAISES` table, which every section anchors on.
- Boundary: composes the runtime credential and resilience owners, never a second STAC paging loop, CQL2 compiler, SAS token fetch, conformance negotiator, or retry/backoff loop; no live UI, no durable catalog store. A `search_by_<axis>` method family, a `cap`-keyword ternary fork where the `SurfaceRow` carries the name, a blind `**params` splat onto `collection_search` where `accepts` filters the rejected keyword, a `signing.sign(...) if surface is ITEM else ...` branch where `materialize` routes, and a hand-opened `boundary` re-spelling the retry/span/lift the `guarded` envelope fuses are rejected.

```python
import threading
from collections.abc import Callable
from enum import StrEnum
from functools import reduce
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import Error, Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, field
from opentelemetry import trace
from opentelemetry.trace import SpanKind

lazy import planetary_computer
lazy from obstore.auth.earthdata import NasaEarthdataAsyncCredentialProvider
lazy from obstore.auth.planetary_computer import PlanetaryComputerAsyncCredentialProvider
lazy from pystac_client import Client

from rasm.data.tabular.interop import DataLeg
from rasm.runtime.faults import TERMINAL, TRANSIENT, FaultRow, RuntimeRail, rostered, scoped
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import on_thread
from rasm.runtime.resilience import RetryClass, guarded
from rasm.runtime.roots import Provider, origin

if TYPE_CHECKING:
    from pystac import Collection, ItemCollection
    from pystac_client import ItemSearch

_TRACER: Final = scoped(trace.get_tracer, "rasm.data.spatial.catalog")

STAC_DISCOVER: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CATALOG, point="discover", arm="boundary", defect="discover-refused", retriability=TRANSIENT
)
STAC_CLAIM: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CATALOG, point="claim", arm="boundary", defect="claim-refused", retriability=TRANSIENT
)
STAC_COVERAGE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CATALOG, point="assets.coverage", arm="boundary", defect="coverage-refused", retriability=TRANSIENT
)
STAC_SIGNING: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CATALOG, point="signing", arm="config", defect="key-collision", retriability=TERMINAL
)
STAC_ROUTE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CATALOG, point="table.route", arm="config", defect="unrouted-pair", retriability=TERMINAL, slots=("source", "sink")
)
STAC_COLUMN: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CATALOG, point="assets.claim", arm="config", defect="column-undeclared", retriability=TERMINAL, slots=("column",)
)
STAC_SURFACE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CATALOG, point="assets.surface", arm="config", defect="collection-terminal", retriability=TERMINAL
)
STAC_KEYLESS: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CATALOG, point="assets.egress", arm="boundary", defect="no-content-key", retriability=TERMINAL, slots=("href", "window")
)
STAC_CUBE: Final[FaultRow[DataLeg]] = FaultRow(
    leg=DataLeg.CATALOG, point="assets.cube", arm="boundary", defect="snapshot-absent", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[DataLeg]]] = rostered(Block.of_seq([
    STAC_DISCOVER,
    STAC_CLAIM,
    STAC_COVERAGE,
    STAC_SIGNING,
    STAC_ROUTE,
    STAC_COLUMN,
    STAC_SURFACE,
    STAC_KEYLESS,
    STAC_CUBE,
]))

type Bound = tuple[float, float, float, float]
type Geometry = dict[str, object]
type Predicate = dict[str, object]
type Fields = dict[str, tuple[str, ...]]
type SearchParams = dict[str, object]
type Headers = dict[str, str]
type Modifier = Callable[[object], object]
type OpenKwargs = dict[str, object]
type Credential = Callable[[], object]
type Materialized = tuple[
    "ItemCollection | list[Collection]", tuple[str, ...], Option[int], int, Option[str], Option[str]
]


_PC_LOCK: Final = threading.Lock()
_PC_BOUND: list[str] = []


def _bind_subscription(key: str) -> "RuntimeRail[None]":
    with _PC_LOCK:
        match _PC_BOUND:
            case []:
                planetary_computer.set_subscription_key(key)
                _PC_BOUND.append(key)
                return Ok(None)
            case [bound] if bound == key:
                return Ok(None)
            case _:
                return Error(STAC_SIGNING.raised())


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


_SCHEME: Final[Map[SignScheme, SchemeRow]] = Map.of_seq([
    (
        SignScheme.NONE,
        SchemeRow(
            open_kwargs=lambda headers, timeout: {"headers": headers, "timeout": timeout},
            sign=lambda search: search.item_collection(),
            patch_url=lambda: None,
        ),
    ),
    (
        SignScheme.PLANETARY_COMPUTER,
        SchemeRow(
            open_kwargs=lambda headers, timeout: {"headers": headers, "timeout": timeout, "modifier": planetary_computer.sign_inplace},
            sign=lambda search: planetary_computer.sign(search),
            patch_url=lambda: planetary_computer.sign,
        ),
    ),
])


def _pc_credentials(subscription_key: str | None) -> Credential:
    return PlanetaryComputerAsyncCredentialProvider(subscription_key=subscription_key)


def _earthdata_credentials(credentials_url: str, auth: "tuple[str, str] | str | None") -> Credential:
    return NasaEarthdataAsyncCredentialProvider(credentials_url, auth=auth)


class Signing(Struct, frozen=True):
    headers: Headers = field(default_factory=dict)
    timeout: float | None = None
    scheme: SignScheme = SignScheme.NONE
    credentials: Option[Credential] = Nothing

    @staticmethod
    def none() -> "Signing":
        return Signing()

    @staticmethod
    def of_headers(headers: Headers, timeout: float | None = None) -> "Signing":
        return Signing(headers=headers, timeout=timeout)

    @staticmethod
    def planetary_computer(
        subscription_key: str | None = None, headers: Headers | None = None, timeout: float | None = None
    ) -> "RuntimeRail[Signing]":
        bound = _bind_subscription(subscription_key) if subscription_key is not None else Ok(None)
        return bound.map(
            lambda _none: Signing(
                headers=headers or {},
                timeout=timeout,
                scheme=SignScheme.PLANETARY_COMPUTER,
                credentials=Some(_pc_credentials(subscription_key)),
            )
        )

    @staticmethod
    def earthdata(
        credentials_url: str, auth: "tuple[str, str] | str | None" = None, headers: Headers | None = None, timeout: float | None = None
    ) -> "Signing":
        return Signing(headers=headers or {}, timeout=timeout, credentials=Some(_earthdata_credentials(credentials_url, auth)))

    def provider(self) -> Provider:
        return self.credentials.to_optional()

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
    return collection, item_ids, Option.of_optional(search.matched()), href_count, Option.of_optional(expiry), Some(search.url_with_parameters())


def _materialize_collections(search: object, _: Signing) -> Materialized:
    collections = search.collection_list()
    return collections, tuple(c.id for c in collections), Nothing, 0, Nothing, Nothing


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

_SURFACE: Final[Map[Surface, SurfaceRow]] = Map.of_seq([
    (
        Surface.ITEM,
        SurfaceRow(method="search", cap="max_items", accepts=_SHARED | {"ids", "collections", "intersects"}, materialize=_materialize_items),
    ),
    (
        Surface.COLLECTION,
        SurfaceRow(method="collection_search", cap="max_collections", accepts=_SHARED | {"q"}, materialize=_materialize_collections),
    ),
])


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
    matched: Option[int]
    href_count: int
    expiry: Option[str]
    url: Option[str]
    content_key: ContentKey

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
        with _TRACER.start_as_current_span(
            f"stac.discover.{surface.value}", kind=SpanKind.CLIENT, attributes={"rasm.geo.remote": True, "rasm.geo.op": f"discover.{surface.value}"}
        ):
            return (
                await guarded(
                    RetryClass.HTTP, on_thread, lambda: self._discover(row, params, max_items, limit), abandon=True,
                    at=STAC_DISCOVER, on=Some(origin(self.endpoint)),
                )
            ).bind(self._shape(surface))

    def _discover(self, row: SurfaceRow, params: SearchParams, max_items: int | None, limit: int | None) -> Materialized:
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

- Owner: `StacGeoClaim` — the ONE STAC-table owner over `stac-geoparquet`, folding a closed `StacTableOp` axis in BOTH directions onto one awaitable `apply`: the discovered `pystac.ItemCollection` or a STAC-NDJSON file crossing INTO the `pyarrow` carrier the `tabular/columnar#SCAN` scan and `tabular/query#QUERY` engine consume, and back OUT to GeoParquet, NDJSON, or Delta. Two rosters carry what four sibling entrypoints used to hold between them: `_TABLE_ROUTE` is the one-call capability roster keyed by the `(TableSource, TableSink)` pair, and `_OpRow` is the dispatch row keyed by the op tag, carrying the retry class the op's remote arm elects beside the provider raise set its one boundary narrows to.
- Cases: `TableSource` — `Items` (an in-memory `pystac.Item` iterable), `Ndjson` (a STAC-NDJSON file admitted through the closed `NdjsonRef` local-versus-remote carrier), and `Table` (an Arrow table or reader already in hand). `Table` is a source because an Arrow table already held IS the input of a write exactly as an item iterable is, and that is what lets ONE `(source, sink)` roster hold the whole legality where a table-in entrypoint and a source-to-disk entrypoint each held half of it. `TableSink` — `Parquet` (versioned GeoParquet), `NdjsonOut`, `DeltaLake`. The schema axis is the `ACCEPTED_SCHEMA_OPTIONS` literal: `"FullFile"` scans every batch for the widest schema (the parse default, correctness over a heterogeneous multi-collection result), `"FirstBatch"` infers from the first batch (the write default, the lower-latency one-call path) — a parameter row, not a parallel parse. `StacPayload` names the three products — a materialized Arrow table, a written `(destination, ContentKey)` pair, and rehydrated items.
- Entry: `StacGeoClaim.apply(op)` is the one entry. It reads `_STAC_ROW` off the op tag and matches the row's retry class against the op's OWN remote coordinate: a class paired with a remote NDJSON source rides `guarded(RetryClass.HTTP, on_thread, ...)` under `SpanKind.CLIENT`, and every other pairing rides the plain banded `on_thread` hop under `INTERNAL`, so the network-versus-local discriminant is recovered from the value and never picked by which method a caller reached for. Legality is settled before the fold runs: `StacTableOp.Write` reads `_TABLE_ROUTE.try_find((source.tag, sink.tag))` and answers `RuntimeRail[StacTableOp]`, so a pair with no one-call provider surface refuses at CONSTRUCTION; `Parse` and `Rehydrate` are total over their inputs and answer the bare value, the narrowest carrier that states each outcome. Inside the hop one `boundary(..., catch=row.catch())` fences the whole leg on the op's real raise set and self-flattens the already-railed `ContentIdentity.of` through `.bind(lambda rail: rail)`.
- Auto: `parse_stac_items_to_arrow` accepts the `pystac.Item` iterable directly, no NDJSON round-trip; `to_parquet` stamps the GeoParquet schema version so a downstream reader resolves the column layout without a side channel; the source-to-disk rows collapse parse-and-write into one provider call that never materializes an intermediate reader; a source already holding an Arrow table streams back at zero copy through `to_reader` rather than re-parsing a format it has already left. The parse leg materializes exactly once at the claim boundary, so the table crosses and `Table.to_reader()` hands the streaming carrier back at zero copy. `stac-geoparquet`/`pystac`/`deltalake` declare as module-scope `lazy` lines and reify at first use, so each `_TABLE_ROUTE` write and each `_OpRow` raise set is a call-time thunk — a module-scope tuple dereferencing one of those proxies reifies it at import and re-opens the eager band the manifest bans. The `geopandas`-backed trio is a fallback never called — the `arrow.*` namespace is the canonical carrier.
- Packages: `stac-geoparquet` (`arrow.parse_stac_items_to_arrow`/`parse_stac_ndjson_to_arrow`/`parse_stac_items_to_parquet`/`parse_stac_ndjson_to_parquet`/`to_parquet`/`stac_table_to_items`/`stac_table_to_ndjson`/`parse_stac_ndjson_to_delta_lake`, the `ACCEPTED_SCHEMA_OPTIONS`/`SUPPORTED_PARQUET_SCHEMA_VERSIONS`/`DEFAULT_*` schema axis), `pystac` (`Item.from_dict`, the `STACError` failure tree the rehydrate row narrows to), `deltalake` (`exceptions.DeltaError`, the Delta row's own native raise), `pyarrow` (the `RecordBatchReader`/`Table` carrier), runtime (`ContentIdentity`/`RuntimeRail`/`boundary`/`RetryClass`/`guarded`/`on_thread`).
- Growth: a new schema mode is one `ACCEPTED_SCHEMA_OPTIONS` row; a new source is one `TableSource` case with its `reader`/`emit`/`remote` arms; a new sink is one `TableSink` case with its `emit` arm; a new one-call path is one `_TABLE_ROUTE` row; a new work class or raise surface is one `_OpRow` column value; zero new entrypoint.
- Boundary: composes the `tabular/columnar`/`tabular/query`/`tabular/egress` owners, never a second table engine or writer; no durable catalog store. A hand-built STAC-to-Arrow schema, a hand-rolled parquet writer, a materialize-then-write two-hop where a one-call row writes straight to disk, an entrypoint family whose method names carry the source-versus-sink and local-versus-remote discriminants the values already hold, a `case _, _` corner reject evaluated inside the fold where the roster settles legality at construction, a catch-less `boundary` funnelling a whole leg through `Exception`, and the geopandas trio where the zero-copy Arrow path applies are rejected.

```python
from collections.abc import Callable
from pathlib import Path
from typing import TYPE_CHECKING, Final, Literal, assert_never

import pyarrow as pa
from expression import Nothing, Ok, Option, Some, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from opentelemetry.trace import SpanKind

lazy import pystac
lazy from deltalake.exceptions import DeltaError
lazy from stac_geoparquet.arrow import (
    DEFAULT_JSON_CHUNK_SIZE,
    DEFAULT_PARQUET_SCHEMA_VERSION,
    parse_stac_items_to_arrow,
    parse_stac_items_to_parquet,
    parse_stac_ndjson_to_arrow,
    parse_stac_ndjson_to_delta_lake,
    parse_stac_ndjson_to_parquet,
    stac_table_to_items,
    stac_table_to_ndjson,
    to_parquet,
)

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import on_thread
from rasm.runtime.resilience import RetryClass, guarded
from rasm.runtime.roots import origin

if TYPE_CHECKING:
    from collections.abc import Iterable

type SchemaInference = Literal["FullFile", "FirstBatch"]


@tagged_union(frozen=True)
class NdjsonRef:
    tag: Literal["local", "remote"] = tag()
    local: str = case()
    remote: str = case()

    @staticmethod
    def of(path: str) -> "NdjsonRef":
        return NdjsonRef(remote=path) if path.startswith(("http://", "https://")) else NdjsonRef(local=path)

    def uri(self) -> str:
        match self:
            case NdjsonRef(tag="remote", remote=url):
                return url
            case NdjsonRef(tag="local", local=path):
                return path
            case unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class TableSource:
    tag: Literal["items", "ndjson", "table"] = tag()
    items: "Iterable[object]" = case()
    ndjson: tuple[NdjsonRef, int | None] = case()
    table: "pa.RecordBatchReader | pa.Table" = case()

    @staticmethod
    def Items(items: "Iterable[object]") -> "TableSource":
        return TableSource(items=items)

    @staticmethod
    def Ndjson(path: str, limit: int | None = None) -> "TableSource":
        return TableSource(ndjson=(NdjsonRef.of(path), limit))

    @staticmethod
    def Table(table: "pa.RecordBatchReader | pa.Table") -> "TableSource":
        return TableSource(table=table)

    def reader(self, schema: SchemaInference) -> "pa.RecordBatchReader":
        match self:
            case TableSource(tag="items", items=items):
                return parse_stac_items_to_arrow(items, chunk_size=DEFAULT_JSON_CHUNK_SIZE, schema=schema)
            case TableSource(tag="ndjson", ndjson=(ref, limit)):
                return parse_stac_ndjson_to_arrow(ref.uri(), chunk_size=DEFAULT_JSON_CHUNK_SIZE, schema=schema, limit=limit)
            case TableSource(tag="table", table=held):
                return held if isinstance(held, pa.RecordBatchReader) else held.to_reader()
            case unreachable:
                assert_never(unreachable)

    def emit(self) -> tuple[object, int | None]:
        match self:
            case TableSource(tag="items", items=items):
                return items, None
            case TableSource(tag="ndjson", ndjson=(ref, limit)):
                return ref.uri(), limit
            case TableSource(tag="table", table=held):
                return held, None
            case unreachable:
                assert_never(unreachable)

    def remote(self) -> Option[str]:
        match self:
            case TableSource(tag="ndjson", ndjson=(NdjsonRef(tag="remote", remote=url), _limit)):
                return Some(url)
            case _:
                return Nothing


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

    def emit(self) -> tuple[str, str | None]:
        match self:
            case TableSink(tag="parquet", parquet=(output_path, version)):
                return output_path, version
            case TableSink(tag="ndjson_out", ndjson_out=dest):
                return dest, None
            case TableSink(tag="delta_lake", delta_lake=table_or_uri):
                return table_or_uri, None
            case unreachable:
                assert_never(unreachable)


class _Emit(Struct, frozen=True):
    payload: object
    dest: str
    schema: SchemaInference
    schema_version: str
    limit: int | None


def _emitted(source: TableSource, sink: TableSink, schema: SchemaInference) -> _Emit:
    payload, limit = source.emit()
    dest, version = sink.emit()
    return _Emit(payload=payload, dest=dest, schema=schema, schema_version=version or DEFAULT_PARQUET_SCHEMA_VERSION, limit=limit)


def _written(emitted: _Emit) -> bytes:
    return Path(emitted.dest).read_bytes()


def _transit(emitted: _Emit) -> bytes:
    return f"{emitted.payload}->{emitted.dest}".encode()


class _RouteRow(Struct, frozen=True):
    kind: str
    write: Callable[[_Emit], None]
    preimage: Callable[[_Emit], bytes]


_TABLE_ROUTE: Final[Map[tuple[str, str], _RouteRow]] = Map.of_seq([
    (
        ("table", "parquet"),
        _RouteRow(kind="stac.geoparquet", write=lambda e: to_parquet(e.payload, e.dest, schema_version=e.schema_version), preimage=_written),
    ),
    (("table", "ndjson_out"), _RouteRow(kind="stac.ndjson", write=lambda e: stac_table_to_ndjson(e.payload, e.dest), preimage=_written)),
    (
        ("items", "parquet"),
        _RouteRow(
            kind="stac.geoparquet",
            write=lambda e: parse_stac_items_to_parquet(
                e.payload, chunk_size=DEFAULT_JSON_CHUNK_SIZE, schema=e.schema, output_path=e.dest, schema_version=e.schema_version
            ),
            preimage=_written,
        ),
    ),
    (
        ("ndjson", "parquet"),
        _RouteRow(
            kind="stac.geoparquet",
            write=lambda e: parse_stac_ndjson_to_parquet(
                e.payload, e.dest, chunk_size=DEFAULT_JSON_CHUNK_SIZE, limit=e.limit, schema_version=e.schema_version
            ),
            preimage=_written,
        ),
    ),
    (
        ("ndjson", "delta_lake"),
        _RouteRow(kind="stac.delta", write=lambda e: parse_stac_ndjson_to_delta_lake(e.payload, e.dest, limit=e.limit), preimage=_transit),
    ),
])


@tagged_union(frozen=True)
class StacTableOp:
    tag: Literal["parse", "write", "rehydrate"] = tag()
    parse: tuple[TableSource, SchemaInference] = case()
    write: tuple[TableSource, TableSink, SchemaInference] = case()
    rehydrate: "pa.Table" = case()

    @staticmethod
    def Parse(source: TableSource, schema: SchemaInference = "FullFile") -> "StacTableOp":
        return StacTableOp(parse=(source, schema))

    @staticmethod
    def Write(source: TableSource, sink: TableSink, schema: SchemaInference = "FirstBatch") -> "RuntimeRail[StacTableOp]":
        return (
            _TABLE_ROUTE.try_find((source.tag, sink.tag))
            .to_result_with(lambda: STAC_ROUTE.raised(source.tag, sink.tag))
            .map(lambda _row: StacTableOp(write=(source, sink, schema)))
        )

    @staticmethod
    def Rehydrate(table: "pa.Table") -> "StacTableOp":
        return StacTableOp(rehydrate=table)

    def remote(self) -> Option[str]:
        match self:
            case StacTableOp(tag="parse", parse=(source, _schema)):
                return source.remote()
            case StacTableOp(tag="write", write=(source, _sink, _schema)):
                return source.remote()
            case _:
                return Nothing


@tagged_union(frozen=True)
class StacPayload:
    tag: Literal["arrow", "written", "items"] = tag()
    arrow: "pa.Table" = case()
    written: tuple[str, ContentKey] = case()
    items: tuple[object, ...] = case()


class _OpRow(Struct, frozen=True):
    retry: Option[RetryClass]
    catch: Callable[[], tuple[type[BaseException], ...]]


_STAC_ROW: Final[Map[str, _OpRow]] = Map.of_seq([
    ("parse", _OpRow(retry=Some(RetryClass.HTTP), catch=lambda: (OSError, ValueError))),
    ("write", _OpRow(retry=Some(RetryClass.HTTP), catch=lambda: (OSError, ValueError, DeltaError))),
    ("rehydrate", _OpRow(retry=Nothing, catch=lambda: (pystac.STACError, ValueError))),
])


class StacGeoClaim(Struct, frozen=True):
    async def apply(self, op: StacTableOp) -> "RuntimeRail[StacPayload]":
        row = _STAC_ROW[op.tag]
        match row.retry, op.remote():
            case Option(tag="some", some=cls), Option(tag="some", some=url):
                with _TRACER.start_as_current_span(
                    f"stac.claim.{op.tag}",
                    kind=SpanKind.CLIENT,
                    attributes={"rasm.geo.remote": True, "rasm.geo.op": op.tag, "rasm.geo.source": url},
                ):
                    acquired = await guarded(cls, on_thread, lambda: self._run(op, row), abandon=True, at=STAC_CLAIM, on=Some(origin(url)))
                    return acquired.bind(lambda inner: inner)
            case _:
                with _TRACER.start_as_current_span(f"stac.claim.{op.tag}", attributes={"rasm.geo.op": op.tag}):
                    return await on_thread(self._run, op, row)

    def _run(self, op: StacTableOp, row: _OpRow) -> "RuntimeRail[StacPayload]":
        return boundary(STAC_CLAIM, lambda: self._payload(op), catch=row.catch()).bind(lambda rail: rail)

    def _payload(self, op: StacTableOp) -> "RuntimeRail[StacPayload]":
        match op:
            case StacTableOp(tag="parse", parse=(source, schema)):
                return Ok(StacPayload(arrow=source.reader(schema).read_all()))
            case StacTableOp(tag="write", write=(source, sink, schema)):
                route = _TABLE_ROUTE[(source.tag, sink.tag)]
                emitted = _emitted(source, sink, schema)
                route.write(emitted)
                return ContentIdentity.of(route.kind, route.preimage(emitted)).map(lambda key: StacPayload(written=(emitted.dest, key)))
            case StacTableOp(tag="rehydrate", rehydrate=table):
                return Ok(StacPayload(items=tuple(pystac.Item.from_dict(record) for record in stac_table_to_items(table))))
            case unreachable:
                assert_never(unreachable)

```

## [04]-[ASSETS]

- Owner: `AssetFold` — one awaitable fold over the signed `StacDiscovery.collection` discriminating a `FoldTarget` axis (`Egress`/`Cube`/`Coverage`) into the settled downstream seams, not a new transport. `Egress` reads the intersecting COG/GeoTIFF byte windows through `tabular/egress#EGRESS` `ObjectEgress.run_async(StoreOp.GetRange(...))` over a store this fold opens under the discovery's OWN credential provider, so the read half and the signing half share one token custody; `Cube` registers the cube-bearing hrefs as virtual chunk byte-ranges through `gridded/virtual#VIRTUAL` `VirtualReference.apply(VersionOp(aggregate=(...)))` composing the `gridded/virtual#MANIFEST` `FieldVirtual` manifest; `Coverage` reads the `proj`/`raster`/`eo` extensions into one `RasterGeoClaim` with `stac_cfg` and drives the `odc-stac` `odc.stac.load` COG datacube with `Signing.patch_url()` threaded so the reads are SAS-signed.
- Cases: the `FoldTarget` value IS the route. One shared `_raster_hrefs` generator yields the `(asset, href)` pairs whose `media_type` is in `{MediaType.COG, MediaType.GEOTIFF}` — the one media-type gate for both raster arms, never a per-arm raw-MIME set. Extension reads ride the typed `obj.ext.<short>` accessor the object type statically scopes (`sample.ext.proj`, `sample.ext.eo`, `asset.ext.raster`), so a missing extension is a typed absence, not a `KeyError`, never a raw `properties` probe; each accessor's own nullable slot then admits at the single read site in `_claim` and no interior reader re-derives it. `BandSource` is the closed two-member vocabulary naming which roster answered the census.
- Entry: `AssetFold.over` guards on `surface is Surface.COLLECTION`, returning a typed reject so a collection terminal never reaches the asset arms, then materializes `sources` once. `Egress` arms fan the egress owner's `run_async` across the byte windows inside one task group under the run-scoped `_WINDOW_BAND` limiter — one instance per running event loop, shared by every concurrent fold on it — independent reads never serialize on the store's latency — and thread the order-preserved rails through `traversed(..., by=Disposition.ABORT)` so the first byte-window fault aborts the fold; the egress owner short-circuits an unchanged content-key to a by-reference no-op. `Cube` arms cross the blocking `VirtualReference.apply` on the banded `on_thread` hop and narrow its `VirtualOutcome` to `VirtualSnapshot` through one `isinstance` arm, reading the real `chunk_refs` manifest count rather than `len(sources)`. `Coverage` arms ride the HTTP envelope and self-flattens; `_claim` is the ACCUMULATING admission over the item's `proj`/`raster`/`eo` extensions, reporting every undeclared column at once through `traversed(..., by=Disposition.ACCUMULATE)` and answering one `ClaimBundle` whose optional columns carry their own absence, and only a bundle that proved its columns reaches `odc.stac.load`. Every arm closes through `_rekey`, folding the railed `ContentIdentity.of` into a `StacDiscovery` preserving every source field with the real folded count — the `Egress` preimage reads each per-window `href:window:content_key` row off `EgressResult`, refusing a keyless result so two keyless windows never share one identity row, and its count sums `EgressResult.byte_length`; the `Cube` count reads `VirtualSnapshot.chunk_refs`; the `Coverage` count reads the admitted band census over rows that name only what the scene measured — so changed remote bytes flip the egress key, a coverage is byte-distinct from its egress, and a single new asset flips the key.
- Auto: the byte window is the COG/GeoTIFF IFD header/overview/tile range passed straight to `GetRange`, one HTTP range request, never a full-object read; the virtual cube reuses the `FieldVirtual` owner's `ObjectStoreRegistry` backend map so STAC asset URLs register through the same runtime store fold the egress arm speaks — one transport AND one credential custody across discovery, egress, and cube, so no arm walks a token-less handle over assets the discovery signed. `ClaimBundle` is the admitted-fact owner: `proj:epsg` and the band census are required and refuse together, while the `raster:bands` fill, the `proj:transform` affine, the `eo:cloud_cover` fraction, and each asset's title ride their own absence. The census names WHICH roster answered it — `eo:bands` declares it and the per-asset `raster:bands` descriptors default it under their own source name — because one integer standing for a spectral count, a descriptor count, and a forged zero told a reader nothing. An undeclared fill omits the `nodata` override from `stac_cfg` entirely, so `odc.stac.load` reads each band's own descriptor rather than a fabricated `0.0` that would mask every genuine zero pixel; the per-band config keys by the asset NAME, the coordinate that loader addresses, never a human title or an href. `Signing.patch_url()` rides `patch_url=` so the COG reads are signed by the same dispatch that signed discovery. Loaded cube `sizes`, the CRS, and a measured cloud fraction fold into the `Coverage` key so a cloudy and a clear scene of one bbox key byte-distinct.
- Output: the fold re-mints one `StacDiscovery` keyed by the fold-target `ContentIdentity` over its arm payload with the folded count.
- Packages: `pystac` (`Item.assets`/`Asset.href`/`media_type`/`MediaType.COG`/`GEOTIFF`, the `obj.ext.proj`/`ext.eo`/`ext.raster` accessors), `odc-stac` (`odc.stac.load`), `tabular/egress` (`ObjectEgress.of`/`run_async`, the `EgressResult.byte_length` the fold sums), `runtime/transport/roots#STORE` (`StoreOp.GetRange` the operation axis, `Provider` the credential carry), `gridded/virtual` (`VirtualReference.apply`/`ManifestWrite`/`VirtualSnapshot.chunk_refs`), `gridded/virtual#MANIFEST` (`FieldVirtual` the composed manifest cube), `spatial/geospatial` (`RasterGeoClaim`/`Resampling`), `spatial/catalog` (`Surface` the terminal guard reads), `expression` (`Block.of_seq`/`Block.choose`/`Error`, the `Option` absence carrier and its `of_optional`/`map2`/`to_result_with` admission matrix), runtime (`ContentIdentity`/`RuntimeRail`/`FaultRow`/`traversed`/`Disposition`/`RetryClass`/`guarded`/`on_thread`).
- Growth: a new archival format is one `MediaType` member in the `_raster_hrefs` gate; a new cube source is the existing `gridded/virtual#MANIFEST` `VirtualParser` case upstream, zero change here; a new coverage knob is one field on the `Coverage` row; a new extension read is one typed accessor in `_claim` landing on its own carrier, and a required one is one more rail in the accumulating sweep; a new band roster is one `BandSource` member with its `_census` arm; zero new surface.
- Boundary: reads the settled `tabular/egress`/`gridded/virtual`/`spatial/geospatial`/`odc-stac` fences and re-mints none — no second object-store transport, virtual-cube builder, COG loader, or raster claim, no full-object read where a byte window applies. A raw `properties` probe where the typed accessor applies, a `VirtualOutcome` consumed without the `VirtualSnapshot` narrowing before `.chunk_refs`, a `len(sources)` count where the real snapshot carries the count, a `_rekey` dropping `expiry`/`matched`/`url`, a caller-supplied egress owner whose store may carry no credential lifetime where this fold binds the discovery's own provider, a hand-fed `stac_cfg` where the extension read derives it, a coalesce fusing an undeclared column with a real measurement, a sentinel folded into a content preimage where the absent fact must refuse, and a fail-fast admission that names one missing column per re-run are rejected.

```python
from enum import StrEnum
from typing import TYPE_CHECKING, Final, Literal, assert_never

import anyio
from anyio import CapacityLimiter
from anyio.lowlevel import RunVar
from expression import Error, Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct, structs

lazy import odc.stac
lazy from pystac import MediaType

from rasm.data.gridded.virtual import FieldVirtual, ManifestWrite, VirtualReference, VirtualSnapshot, VersionOp
from rasm.data.spatial.geospatial import RasterGeoClaim, Resampling
from rasm.data.tabular.egress import EgressResult, ObjectEgress
from rasm.runtime.identity import ContentIdentity
from rasm.runtime.faults import Disposition, RuntimeRail, traversed
from rasm.runtime.lanes import on_thread
from rasm.runtime.resilience import RetryClass, guarded
from rasm.runtime.roots import ResourceRef, StoreOp, origin

if TYPE_CHECKING:
    from collections.abc import Iterator, Mapping

type StacCfg = dict[str, dict[str, object]]
type Window = tuple[int, int]

_SOURCE_OWNER: Final[str] = "rasm.data.spatial.catalog"


_WINDOW_BAND: Final[int] = 8
_WINDOW_LIMITER: Final[RunVar[CapacityLimiter]] = RunVar("stac-window-band")


def _window_band() -> CapacityLimiter:
    held = _WINDOW_LIMITER.get(None)
    if held is None:
        held = CapacityLimiter(_WINDOW_BAND)
        _WINDOW_LIMITER.set(held)
    return held


def _raster_hrefs(collection: object) -> "Iterator[tuple[object, str]]":
    raster = {MediaType.COG, MediaType.GEOTIFF}
    return ((asset, asset.href) for item in collection for asset in item.assets.values() if asset.media_type in raster)


class BandSource(StrEnum):
    SPECTRAL = "eo:bands"
    DESCRIPTOR = "raster:bands"


class ClaimBundle(Struct, frozen=True):
    crs: str
    band_count: int
    band_source: BandSource
    nodata: Option[float]
    transform: Option[tuple[float, ...]]
    cloud_cover: Option[float]
    stac_cfg: StacCfg
    claim: Option[RasterGeoClaim]


class _Census(Struct, frozen=True):
    count: int
    source: BandSource


def _census(spectral: "Option[tuple[object, ...]]", descriptors: "Option[tuple[object, ...]]") -> "Option[_Census]":
    match spectral.filter(lambda bands: bool(bands)), descriptors.filter(lambda bands: bool(bands)):
        case Option(tag="some", some=bands), _:
            return Some(_Census(count=len(bands), source=BandSource.SPECTRAL))
        case _, Option(tag="some", some=bands):
            return Some(_Census(count=len(bands), source=BandSource.DESCRIPTOR))
        case _:
            return Nothing


def _stac_cfg(named: "Block[tuple[str, object]]", nodata: Option[float]) -> StacCfg:
    return nodata.map(lambda fill: {"*": {"assets": {name: {"nodata": fill} for name, _asset in named}}}).default_value({})


def _window_row(paired: "tuple[tuple[str, Window], EgressResult]") -> "RuntimeRail[str]":
    (href, (start, end)), result = paired
    return (
        Option.of_optional(result.content_key)
        .map(lambda key: f"{href}:{start}-{end}:{key.hex}")
        .to_result_with(lambda: STAC_KEYLESS.raised(href, f"{start}-{end}"))
    )


@tagged_union(frozen=True)
class FoldTarget:
    tag: Literal["egress", "cube", "coverage"] = tag()
    egress: tuple[ResourceRef, "Mapping[str, Window]"] = case()
    cube: tuple[ResourceRef, str] = case()
    coverage: tuple[str, str, "dict[str, int] | None"] = case()

    @staticmethod
    def Egress(ref: ResourceRef, windows: "Mapping[str, Window]") -> "FoldTarget":
        return FoldTarget(egress=(ref, windows))

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
            return Error(STAC_SURFACE.raised())
        hrefs = tuple(href for _, href in _raster_hrefs(self.discovery.collection))
        sources = tuple(ResourceRef.admit(href, _SOURCE_OWNER, self.signing.provider()) for href in hrefs)
        match target:
            case FoldTarget(tag="egress", egress=(ref, windows)):
                egress = ObjectEgress.of(structs.replace(ref, credentials=self.signing.provider()))
                windowed = tuple((href, w) for href in hrefs if (w := windows.get(href)) is not None)
                band = _window_band()

                async def ranged(href: str, start: int, end: int) -> "RuntimeRail[EgressResult]":
                    async with band:
                        return await egress.run_async(StoreOp.GetRange(href, start, end), path=href)

                async with anyio.create_task_group() as group:
                    handles = tuple(group.start_soon(ranged, href, start, end) for href, (start, end) in windowed)
                rails = Block.of_seq(handle.return_value for handle in handles)
                return traversed(rails, by=Disposition.ABORT).bind(
                    lambda results: traversed(
                        Block.of_seq(tuple(zip(windowed, results, strict=True))).map(_window_row), by=Disposition.ACCUMULATE
                    ).bind(lambda rows: self._rekey("egress", "\n".join(rows).encode(), sum(result.byte_length for result in results)))
                )
            case FoldTarget(tag="cube", cube=(ref, concat_dim)):
                manifest = ManifestWrite(
                    cube=FieldVirtual(sources=sources, target=structs.replace(ref, credentials=self.signing.provider()), concat_dim=concat_dim)
                )
                outcome_rail = await on_thread(VirtualReference(sources=sources, ref=ref).apply, VersionOp(aggregate=(manifest, {}, None)))
                return outcome_rail.bind(
                    lambda outcome: (
                        self._rekey("cube", f"{concat_dim}|{'|'.join(hrefs)}".encode(), outcome.chunk_refs)
                        if isinstance(outcome, VirtualSnapshot)
                        else Error(STAC_CUBE.raised())
                    )
                )
            case FoldTarget(tag="coverage", coverage=(groupby, resampling, chunks)):
                return (
                    await guarded(
                        RetryClass.HTTP, on_thread, lambda: self._coverage(groupby, resampling, chunks), abandon=True,
                        at=STAC_COVERAGE, on=Some(origin(self.discovery.endpoint)),
                    )
                ).bind(lambda rail: rail)
            case unreachable:
                assert_never(unreachable)

    def _coverage(self, groupby: str, resampling: Resampling, chunks: "dict[str, int] | None") -> "RuntimeRail[StacDiscovery]":
        return self._claim(next(iter(self.discovery.collection)), resampling).bind(
            lambda bundle: self._loaded(bundle, groupby, resampling, chunks)
        )

    def _loaded(self, bundle: ClaimBundle, groupby: str, resampling: Resampling, chunks: "dict[str, int] | None") -> "RuntimeRail[StacDiscovery]":
        cube = odc.stac.load(
            list(self.discovery.collection),
            stac_cfg=bundle.stac_cfg,
            patch_url=self.signing.patch_url(),
            groupby=groupby,
            resampling=resampling,
            chunks=chunks or {},
        )
        measured = Block.of_seq([
            Some(f"crs={bundle.crs}"),
            Some(f"sizes={tuple(cube.sizes.values())}"),
            bundle.cloud_cover.map(lambda fraction: f"cloud_cover={fraction}"),
        ])
        return self._rekey("coverage", "\n".join(measured.choose(lambda held: held)).encode(), bundle.band_count)

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
    def _claim(sample: object, resampling: Resampling) -> "RuntimeRail[ClaimBundle]":
        projection, eo = sample.ext.proj, sample.ext.eo
        named = Block.of_seq(tuple(sample.assets.items()))
        first = named.try_head()
        spectral = Option.of_optional(eo.bands).map(tuple)
        descriptors = first.bind(lambda pair: Option.of_optional(pair[1].ext.raster.bands)).map(tuple)
        census = _census(spectral, descriptors)
        nodata = descriptors.bind(lambda bands: Block.of_seq(bands).choose(lambda band: Option.of_optional(band.nodata)).try_head()).map(float)
        transform = (
            Option.of_optional(projection.transform)
            .map(lambda affine: tuple(float(value) for value in affine))
            .filter(lambda affine: len(affine) == 6)
        )
        code = Option.of_optional(projection.epsg).to_result_with(lambda: STAC_COLUMN.raised("proj:epsg"))
        holds = first.to_result_with(lambda: STAC_COLUMN.raised("item.assets"))
        counted = census.to_result_with(lambda: STAC_COLUMN.raised("eo:bands|raster:bands"))
        demanded: "Block[RuntimeRail[object]]" = Block.of_seq([code, holds, counted])
        return traversed(demanded, by=Disposition.ACCUMULATE).bind(
            lambda _verdict: code.map2(
                counted,
                lambda epsg, bands: ClaimBundle(
                    crs=f"EPSG:{epsg}",
                    band_count=bands.count,
                    band_source=bands.source,
                    nodata=nodata,
                    transform=transform,
                    cloud_cover=Option.of_optional(eo.cloud_cover).map(float),
                    stac_cfg=_stac_cfg(named, nodata),
                    claim=nodata.map(
                        lambda fill: RasterGeoClaim(
                            crs=f"EPSG:{epsg}",
                            band_count=bands.count,
                            resampling=resampling,
                            nodata=fill,
                            transform=transform.default_value(()),
                        )
                    ),
                ),
            )
        )
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
