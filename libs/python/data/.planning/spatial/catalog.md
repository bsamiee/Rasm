# [PY_DATA_CATALOG]

Cloud-native STAC discovery owner: one `StacCatalog` over `pystac-client` resolving which cloud assets cover a query region — the discovery layer above the raster/vector claims (`spatial/geospatial.md#GEO`) and the archival byte-window read (`tabular/egress.md#EGRESS`) that the geospatial and object-store lanes lack. `StacCatalog.discover` folds the one `StacQuery` search axis onto the single keyword-only `Client.search`, the `Surface` discriminant alone routing the shared rows to `Client.collection_search` on a `FreeText` case, pages the lazy `ItemSearch` into a `pystac.ItemCollection`, and emits one `StacDiscovery` keyed by a runtime `ContentIdentity` over the matched item-id set. One `Signing` value encodes the request boundary as a frozen `SchemeRow` read by name — into `Client.open(modifier=)` for discovery, over the `ItemSearch` for asset hrefs, and into `odc.stac.load(patch_url=)` for the COG load — never a signed-vs-unsigned client pair.

Discovered collections encode as a `stac-geoparquet` columnar Arrow `RecordBatchReader` (`[3]-[TABLE]`) the `tabular/columnar.md#SCAN` scan and `tabular/query.md#QUERY` engine consume, and the asset hrefs fold (`[4]-[ASSETS]`) into the `tabular/egress.md#EGRESS` `ObjectEgress.GetRange` byte-window read, the `gridded/virtual.md#VIRTUAL` `VirtualReference.apply` virtual-chunk registration over the `gridded/virtual#MANIFEST` `FieldVirtual` manifest, and the `odc-stac` `odc.stac.load` COG datacube driven by the catalogue-derived `stac_cfg`/`patch_url`. Every bundle keys by exactly one runtime `ContentIdentity`; the network legs ride the runtime `guarded(RetryClass.HTTP, ...)` envelope, never a second object-store transport, virtual-cube builder, COG loader, or hand-fed band metadata.

## [01]-[INDEX]

- [01]-[CATALOG]: the `StacCatalog` discovery owner over `pystac-client`, the `StacQuery` search axis with the `Surface` discriminant routing item-vs-collection search, emitting one `StacDiscovery`.
- [02]-[TABLE]: the discovered collection encoded as a `stac-geoparquet` Arrow `RecordBatchReader` plus the re-homed `StacGeoClaim` NDJSON-interchange claim, sinks split across table-in and source-to-disk entrypoints.
- [03]-[ASSETS]: the one awaitable `AssetFold` routing signed hrefs into egress byte-windows, virtual cube chunks, or the `odc-stac` COG datacube.

## [02]-[CATALOG]

- Owner: `StacCatalog` — the one cloud-native discovery owner, a `Client.open`-bound STAC API root carrying one `Signing`. `StacQuery` is the tagged-union search axis folded by `match`/`case` onto the single keyword-only `Client.search`; a new search modality is one case, never a `search_bbox`/`search_intersects`/`search_cql2` method family. `Surface` is the discovery-method discriminant carrying its own frozen `SurfaceRow` `(method, cap, accepts, materialize)`: the `accepts` keyword-admission set is the boundary that keeps the union total across surfaces — an `ids`/`intersects`/`collections` row a `FreeText` union folds in never reaches `collection_search`, which rejects it — and the `materialize` policy owns the structural divergence between the two iterators, since the `ITEM` row signs the `ItemSearch` and reads `matched()` while a `CollectionSearch` carries no Azure hrefs and no `sign`/`matched` member, yielding zero hrefs and no expiry. `Signing` encodes the request boundary as one frozen `SchemeRow` (`NONE`/`PLANETARY_COMPUTER`) whose `open_kwargs`/`sign`/`patch_url` callables are read by name, never a positional triple, parallel `match` statements, or a forwarded bare callable.
- Cases: `StacQuery` rows — `Bbox`, `Intersects` (a GeoJSON-geometry dict the server intersects server-side, no shapely at the boundary), `Datetime` (an RFC-3339 interval), `Ids`, `Collection`, `Cql2Filter` (a CQL2-JSON predicate the STAC API evaluates server-side), `Cql2Query` (the legacy `query` extension), `Order` (server-side sort plus field projection), and `FreeText` (whose presence flips `Surface` from `ITEM` to `COLLECTION`, routing to `Client.collection_search`). Each carries a `params()` projection contributing exactly its own keyword arguments, so an n-axis query unions the per-case keyword dicts rather than forking a method per axis; because `collection_search` shares the `bbox`/`datetime`/`query`/`filter`/`filter_lang`/`sortby`/`fields` axis with `search`, the shared rows union onto either surface unchanged, the surface alone differing.
- Entry: `StacCatalog.discover` computes the pure plan — reduces the query tuple's `params()` into one keyword set and recovers `Surface.of_queries` and its `.row` — then drives the whole blocking `pystac_client` sequence (`Client.open`, `row.call`, `row.materialize`) through one `guarded(RetryClass.HTTP, on_thread, ...)` envelope, the `THREAD_BAND`-bounded hop, so the synchronous I/O never stalls the event loop and the transient `429`/`5xx`/timeout set retries under a `Retry-After`-honouring backoff as one logical discovery. `row.call` reads the method, cap, and `accepts` keyword filter off the `SurfaceRow` so a cross-surface param never reaches a method that rejects it, and `row.materialize` returns the full shaped `(collection, item_ids, matched, href_count, expiry, url)` outcome — the `ITEM` row's one `planetary_computer.sign(ItemSearch)` dispatch both materializes the page and rewrites every Azure blob href in a single pass, reports `matched()` and the resolved-GET `url_with_parameters()`, and reads each item's `msft:expiry`; the `COLLECTION` row materializes without signing, yielding a zero href count, no expiry, and a `None` url. Results flatten through `.bind(self._shape(surface))`, which folds the railed `ContentIdentity.of` over the item-id set through `.map` into one `StacDiscovery` rather than stuffing a `RuntimeRail[ContentKey]` into the `content_key` field.
- Auto: the `params` fold is the union law — a bbox+datetime+cloud-cover+order query is one `search`, never four; `Surface.of_queries` flips to `collection_search` exactly when a `FreeText` case is present, the one boolean-free routing read, never a `search_by_<axis>` family. `ItemSearch` is lazy so `matched()` reads the API total without materializing every page, and `sign` over the lazy handle is the one canonical materialize-plus-sign — never the deprecated `get_all_items`, never a `next`-link follow loop, never a materialize-then-re-sign two-pass — and `min(msft:expiry)` over the signed items reports the token-validity horizon. `pystac-client`/`pystac`/`planetary-computer` import function-local per the boundary-scope import policy; the runtime rails ride module-level.
- Receipt: one `StacDiscovery` carries the signed `ItemCollection`, the matched item-id tuple and `matched()` count, the href count, the `msft:expiry` horizon, the resolved `url`, and the `ContentKey`; `contribute()` yields one emitted-phase `Receipt.of("catalog", ...)`, the counts native scalars, never a parallel result-versus-receipt pair.
- Packages: `pystac-client` (the keyword-only `Client.search`/`collection_search`, `ItemSearch.{item_collection,matched,url_with_parameters}` the `ITEM` row reads, `CollectionSearch.collection_list` the `COLLECTION` row reads), `pystac` (`ItemCollection`/`Item`/`Asset`, the `msft:expiry` token horizon), `planetary-computer` (`sign` the `singledispatch` over the lazy `ItemSearch`, `sign_inplace` the `modifier=` callable, `set_subscription_key`), runtime (`RuntimeRail`/`ContentIdentity`/`ContentKey`/`Receipt`/`RetryClass`/`guarded`/`on_thread`).
- Growth: a new search modality is one `StacQuery` case plus its key on the owning surface's `accepts` set; a new auth scheme is one `SignScheme` member plus one `SchemeRow`; a new discovery surface is one `Surface` member plus one `SurfaceRow` whose `accepts` names the method's admissible keywords; zero new surface.
- Boundary: composes the runtime credential and resilience owners, never a second STAC paging loop, CQL2 compiler, SAS token fetch, conformance negotiator, or retry/backoff loop; no live UI, no durable catalog store. A `search_by_<axis>` method family, a `cap`-keyword ternary fork where the `SurfaceRow` carries the name, a blind `**params` splat onto `collection_search` where `accepts` filters the rejected keyword, a `signing.sign(...) if surface is ITEM else ...` branch where `materialize` routes, and a hand-opened `boundary` re-spelling the retry/span/lift the `guarded` envelope fuses are rejected.

```python signature
from collections.abc import Callable
from enum import StrEnum
from functools import cache, reduce
from types import ModuleType
from typing import TYPE_CHECKING, Final, Literal, assert_never

from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct, field

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.lanes import on_thread
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


# boundary-scoped `planetary_computer` handle, imported once per process and read by every `SchemeRow` callable.
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
            open_kwargs=lambda headers, timeout: {"headers": headers, "timeout": timeout, "modifier": _pc().sign_inplace},
            sign=lambda search: _pc().sign(search),
            patch_url=lambda: _pc().sign,
        ),
    ),
])


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
            await guarded(RetryClass.HTTP, on_thread, lambda: self._discover(row, params, max_items, limit), abandon=True, subject="stac.discover")
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

- Owner: the `stac_table` encoder — the discovered `pystac.ItemCollection` to a `stac-geoparquet` columnar Arrow `RecordBatchReader`, the one carrier `tabular/columnar.md#SCAN` and `tabular/query.md#QUERY` consume. `StacGeoClaim` is re-homed here as the STAC-NDJSON interchange claim binding this module's `stac_table*` entrypoints directly, folding a `StacIngest` axis into one `StacResult` over the shared `columnar` `QueryReceipt`, its `apply_remote` leg riding the runtime HTTP envelope for a remote NDJSON source.
- Cases: `TableSource` — `Items` parses an in-memory `pystac.Item` iterable, `Ndjson` parses a STAC-NDJSON file, both landing on the one `RecordBatchReader`. `TableSink` — `Parquet` (versioned GeoParquet), `NdjsonOut`, `DeltaLake`. Its schema axis is the `ACCEPTED_SCHEMA_OPTIONS` literal: `"FullFile"` scans every batch for the widest schema (the discovery default, correctness over a heterogeneous multi-collection result), `"FirstBatch"` infers from the first batch (the lower-latency direct-write path) — a parameter row, not a parallel parse. Sink rows split by whether an Arrow table is in hand: `stac_table_egress` owns the table-in writes, `stac_table_direct` owns the one-call source-to-disk fast-paths that never materialize an intermediate reader; the `DeltaLake` sink lives only on `stac_table_direct` because `parse_stac_ndjson_to_delta_lake` reads an NDJSON file, not an Arrow table, so its `table`-in arm on `stac_table_egress` is a typed reject.
- Entry: `stac_table` folds `TableSource` into the reader; `stac_table_egress` folds the table-in `TableSink` rows to a path, its `delta_lake` arm a typed reject; `stac_table_direct` folds the `(TableSource, TableSink)` pair over the catalogued one-call paths with a `case _, _` typed reject for a pair with no one-call surface; `stac_table_rehydrate` runs `stac_table_to_items` and rebuilds the model with `pystac.Item.from_dict`. Each write arm returns the already-railed `ContentIdentity.of` directly so the `boundary(...).bind(lambda rail: rail)` self-flatten threads the identity fault through the single carrier rather than swallowing it in an `Ok`.
- Auto: the item table crosses as a zero-copy `RecordBatchReader`; `parse_stac_items_to_arrow` accepts the `pystac.Item` iterable directly, no NDJSON round-trip; `to_parquet` stamps the GeoParquet schema version so a downstream reader resolves the column layout without a side channel; `stac_table_direct` collapses parse-and-write for the discovery-to-disk path. `stac-geoparquet`/`pystac` import function-local; the `geopandas`-backed trio is a fallback never called — the `arrow.*` namespace is the canonical carrier.
- Receipt: the shared `tabular/columnar` `QueryReceipt` keyed by `ContentIdentity`, never a parallel table-receipt rail.
- Packages: `stac-geoparquet` (`arrow.parse_stac_items_to_arrow`/`parse_stac_ndjson_to_arrow`/`parse_stac_items_to_parquet`/`parse_stac_ndjson_to_parquet`/`to_parquet`/`stac_table_to_items`/`stac_table_to_ndjson`/`parse_stac_ndjson_to_delta_lake`, the `ACCEPTED_SCHEMA_OPTIONS`/`SUPPORTED_PARQUET_SCHEMA_VERSIONS`/`DEFAULT_*` schema axis), `pystac` (`Item.from_dict`), `pyarrow` (the `RecordBatchReader`/`Table` carrier), runtime (`ContentIdentity`/`RuntimeRail`/`boundary`/`guarded`/`on_thread`).
- Growth: a new schema mode is one `ACCEPTED_SCHEMA_OPTIONS` row; a new source is one `TableSource` case; a new sink is one `TableSink` case plus its `stac_table_egress` or `stac_table_direct` arm; a new one-call fast-path is one `(source, sink)` arm; zero new surface.
- Boundary: composes the `tabular/columnar`/`tabular/query`/`tabular/egress` owners, never a second table engine or writer; no durable catalog store. A hand-built STAC-to-Arrow schema, a hand-rolled parquet writer, a materialize-then-write two-hop where `stac_table_direct` writes straight to disk, a `delta_lake` arm on `stac_table_egress` that silently drops the table argument, and the geopandas trio where the zero-copy Arrow path applies are rejected.

```python signature
from typing import TYPE_CHECKING, Final, Literal, assert_never

import pyarrow as pa
from expression import Error, case, tag, tagged_union
from msgspec import Struct
from opentelemetry import trace

from rasm.data.tabular.columnar import QueryReceipt
from rasm.runtime.faults import BoundaryFault, RuntimeRail, async_boundary, boundary
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import on_thread
from rasm.runtime.resilience import RetryClass, guarded

if TYPE_CHECKING:
    from collections.abc import Iterable

_TRACER: Final = trace.get_tracer("rasm.data.spatial.catalog")

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


@tagged_union(frozen=True)
class StacIngest:
    tag: Literal["to_arrow", "to_delta", "rehydrate"] = tag()
    to_arrow: tuple[str, SchemaInference, int | None] = case()
    to_delta: tuple[str, str] = case()
    rehydrate: "pa.Table" = case()

    @staticmethod
    def ToArrow(path: str, schema: SchemaInference = "FullFile", limit: int | None = None) -> "StacIngest":
        return StacIngest(to_arrow=(path, schema, limit))

    @staticmethod
    def ToDelta(input_path: str, table_uri: str) -> "StacIngest":
        return StacIngest(to_delta=(input_path, table_uri))

    @staticmethod
    def Rehydrate(table: "pa.Table") -> "StacIngest":
        return StacIngest(rehydrate=table)


@tagged_union(frozen=True)
class StacPayload:
    tag: Literal["arrow", "delta", "items"] = tag()
    arrow: "pa.Table" = case()
    delta: str = case()
    items: tuple[object, ...] = case()


class StacResult(Struct, frozen=True):
    payload: StacPayload
    receipt: QueryReceipt


class StacGeoClaim(Struct, frozen=True):
    async def apply(self, op: StacIngest) -> "RuntimeRail[StacResult]":
        # arrow parse and parquet/delta writes block on disk — the banded thread hop, never the loop.
        with _TRACER.start_as_current_span(f"stac.claim.{op.tag}", attributes={"rasm.geo.op": op.tag}):
            acquired = await async_boundary(f"stac.claim.{op.tag}", lambda: on_thread(self._stac, op))
            return acquired.bind(lambda inner: inner).bind(lambda payload: self._result(payload, op.tag))

    async def apply_remote(self, op: StacIngest) -> "RuntimeRail[StacResult]":
        # abandon frees the band slot when an enclosing deadline trips — a wedged remote read runs out unobserved.
        with _TRACER.start_as_current_span(f"stac.claim.{op.tag}", attributes={"rasm.geo.remote": True, "rasm.geo.op": op.tag}):
            acquired = await guarded(RetryClass.HTTP, on_thread, lambda: self._stac(op), abandon=True, subject=f"stac.claim.{op.tag}")
            # `_stac` is itself railed, so `guarded` yields a doubled `RuntimeRail`; the identity `bind` is the monadic join flattening it.
            return acquired.bind(lambda inner: inner).bind(lambda payload: self._result(payload, op.tag))

    def _stac(self, op: StacIngest) -> "RuntimeRail[StacPayload]":
        match op:
            case StacIngest(tag="to_arrow", to_arrow=(path, schema, limit)):
                return stac_table(TableSource.Ndjson(path, limit), schema=schema).map(lambda reader: StacPayload(arrow=reader.read_all()))
            case StacIngest(tag="to_delta", to_delta=(input_path, table_uri)):
                return stac_table_direct(TableSource.Ndjson(input_path), TableSink.DeltaLake(table_uri)).map(
                    lambda _key: StacPayload(delta=table_uri)
                )
            case StacIngest(tag="rehydrate", rehydrate=table):
                return stac_table_rehydrate(table).map(lambda items: StacPayload(items=items))
            case unreachable:
                assert_never(unreachable)

    def _result(self, payload: StacPayload, op_tag: str) -> "RuntimeRail[StacResult]":
        match payload:
            case StacPayload(tag="arrow", arrow=table):
                pass
            case StacPayload(tag="delta", delta=uri):
                table = pa.table({"table_uri": [uri]})
            case StacPayload(tag="items", items=items):
                # rehydrate yields `pystac.Item` objects; lower each to a dict for the Arrow receipt table `QueryReceipt` keys.
                table = pa.Table.from_pylist([item.to_dict() for item in items])
            case unreachable:
                assert_never(unreachable)
        return QueryReceipt.railed("stac_geoparquet", op_tag, table).map(lambda receipt: StacResult(payload=payload, receipt=receipt))
```

## [04]-[ASSETS]

- Owner: `AssetFold` — one awaitable fold over the signed `StacDiscovery.collection` discriminating a `FoldTarget` axis (`Egress`/`Cube`/`Coverage`) into the settled downstream seams, not a new transport. `Egress` reads the intersecting COG/GeoTIFF byte windows through `tabular/egress.md#EGRESS` `ObjectEgress.run(StoreOp.GetRange(...))`; `Cube` registers the cube-bearing hrefs as virtual chunk byte-ranges through `gridded/virtual.md#VIRTUAL` `VirtualReference.apply(VersionOp(aggregate=(...)))` composing the `gridded/virtual#MANIFEST` `FieldVirtual` manifest; `Coverage` reads the `proj`/`raster`/`eo` extensions into one `RasterGeoClaim` plus `stac_cfg` and drives the `odc-stac` `odc.stac.load` COG datacube with `Signing.patch_url()` threaded so the reads are SAS-signed.
- Cases: the `FoldTarget` value IS the route. One shared `_raster_hrefs` generator yields the `(asset, href)` pairs whose `media_type` is in `{MediaType.COG, MediaType.GEOTIFF}` — the one media-type gate for both raster arms, never a per-arm raw-MIME set. Extension reads ride the typed `obj.ext.<short>` accessor the object type statically scopes (`sample.ext.proj`, `sample.ext.eo`, `asset.ext.raster`), so a missing extension is a typed absence, not a `KeyError`, never a raw `properties` probe.
- Entry: `AssetFold.over` guards on `surface is Surface.COLLECTION`, returning a typed reject so a collection terminal never reaches the asset arms, then materializes `sources` once. `Egress` arms fan the egress owner's `run_async` across the byte windows inside one task group under the `_WINDOW_BAND` limiter — independent reads never serialize on the store's latency — and thread the order-preserved rails through `traversed(..., by=Disposition.ABORT)` so the first byte-window fault aborts the fold; the egress owner short-circuits an unchanged content-key to a by-reference no-op. `Cube` arms cross the blocking `VirtualReference.apply` on the banded `on_thread` hop and narrow its `VirtualOutcome` to `VirtualReceipt` through one `isinstance` arm, reading the real `chunk_refs` manifest count rather than `len(sources)`. `Coverage` arms ride the HTTP envelope and self-flattens; `_coverage` reads the sample item's extensions into one `RasterGeoClaim` (`spatial/geospatial.md#GEO`) and `stac_cfg`, then loads the cube from the catalogue-derived config. Every arm closes through `_rekey`, folding the railed `ContentIdentity.of` into a `StacDiscovery` preserving every source field plus the real folded count — the `Egress` preimage the per-window `href:window:content_key` rows read off the receipts and its count the summed `EgressReceipt.byte_length`, the `Cube` count the manifest `chunk_refs`, the `Coverage` count the `band_count` — so changed remote bytes flip the egress key, a coverage is byte-distinct from its egress, and a single new asset flips the key.
- Auto: the byte window is the COG/GeoTIFF IFD header/overview/tile range passed straight to `GetRange`, one HTTP range request, never a full-object read; the virtual cube reuses the `FieldVirtual` owner's `ObjectStoreRegistry` backend map so STAC asset URLs register through the same transport the egress owner speaks — one transport across discovery, egress, and cube. `RasterGeoClaim` carries the `proj:epsg` CRS, `raster:bands` nodata, and `eo:bands` count; the `ClaimBundle` carries the `eo:cloud_cover` scene fraction beside the claim (a knob the sibling-owned `RasterGeoClaim` has no slot for); `Signing.patch_url()` rides `patch_url=` so the COG reads are signed by the same dispatch that signed discovery. Loaded cube `sizes`, the CRS, and the cloud fraction fold into the `Coverage` key so a cloudy and a clear scene of one bbox key byte-distinct.
- Receipt: the fold re-mints the one `StacDiscovery` keyed by the fold-target `ContentIdentity` over its arm payload plus the folded count; no new receipt rail.
- Packages: `pystac` (`Item.assets`/`Asset.href`/`media_type`/`MediaType.COG`/`GEOTIFF`, the `obj.ext.proj`/`ext.eo`/`ext.raster` accessors), `odc-stac` (`odc.stac.load`), `tabular/egress` (`ObjectEgress.run`/`StoreOp.GetRange`, the `EgressReceipt.byte_length` the fold sums), `gridded/virtual` (`VirtualReference.apply`/`ManifestWrite`/`VirtualReceipt.chunk_refs`), `gridded/virtual#MANIFEST` (`FieldVirtual` the composed manifest cube), `spatial/geospatial` (`RasterGeoClaim`/`Resampling`), `spatial/catalog` (`Surface` the terminal guard reads), `expression` (`Block.of_seq`/`Error`), runtime (`ContentIdentity`/`RuntimeRail`/`BoundaryFault`/`traversed`/`Disposition`/`RetryClass`/`guarded`/`on_thread`).
- Growth: a new archival format is one `MediaType` member in the `_raster_hrefs` gate; a new cube source is the existing `gridded/virtual#MANIFEST` `VirtualParser` case upstream, zero change here; a new coverage knob is one field on the `Coverage` row; a new extension read is one typed accessor in `_claim`; zero new surface.
- Boundary: reads the settled `tabular/egress`/`gridded/virtual`/`spatial/geospatial`/`odc-stac` fences and re-mints none — no second object-store transport, virtual-cube builder, COG loader, or raster claim, no full-object read where a byte window applies. A raw `properties` probe where the typed accessor applies, a `VirtualOutcome` consumed without the `VirtualReceipt` narrowing before `.chunk_refs`, a `len(sources)` count where the real receipt evidence is the count, a `_rekey` dropping `expiry`/`matched`/`url`, and a hand-fed `stac_cfg` where the extension read derives it are rejected.

```python signature
from typing import TYPE_CHECKING, Final, Literal, assert_never

import anyio
from anyio import CapacityLimiter
from expression import Error, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct

from rasm.data.gridded.virtual import FieldVirtual, ManifestWrite, VirtualReceipt, VirtualReference, VersionOp
from rasm.data.spatial.geospatial import RasterGeoClaim, Resampling
from rasm.data.tabular.egress import ObjectEgress, StoreOp
from rasm.runtime.identity import ContentIdentity
from rasm.runtime.faults import BoundaryFault, Disposition, RuntimeRail, traversed
from rasm.runtime.lanes import on_thread
from rasm.runtime.resilience import RetryClass, guarded
from rasm.runtime.roots import ResourceRef

if TYPE_CHECKING:
    from collections.abc import Iterator, Mapping

type StacCfg = dict[str, dict[str, object]]
type Window = tuple[int, int]

_WINDOW_BAND: Final[int] = 8  # concurrent byte-window reads per egress fold; the store's own client pools connections under it


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
                # byte windows ride the egress owner's awaitable leg as independent reads — a sequential await pays the
                # store's latency once per window — fanned inside one task group under the window band; each child rails
                # its own outcome so the group exits clean, the handle order preserves the windowed order, and
                # `traversed(by=ABORT)` still aborts the fold on the first failed read.
                limiter = CapacityLimiter(_WINDOW_BAND)

                async def ranged(href: str, start: int, end: int) -> "RuntimeRail[object]":
                    async with limiter:
                        return await egress.run_async(StoreOp.GetRange(href, start, end), path=href)

                async with anyio.create_task_group() as group:
                    handles = tuple(group.start_soon(ranged, href, start, end) for href, (start, end) in windowed)
                rails = Block.of_seq(handle.return_value for handle in handles)
                return traversed(rails, by=Disposition.ABORT).bind(
                    # per-window content identity: each preimage row binds the href, the byte window, and the returned
                    # payload's operation-bytes ContentKey off the receipt — two folds over one href set with different
                    # remote content (or different windows) key distinctly; a bare href-plus-total-length preimage is
                    # the deleted content-blind form, and byte-length accounting stays the folded count.
                    lambda receipts: self._rekey(
                        "egress",
                        "\n".join(
                            f"{href}:{start}-{end}:{receipt.content_key.hex if receipt.content_key is not None else ''}"
                            for (href, (start, end)), receipt in zip(windowed, receipts, strict=True)
                        ).encode(),
                        sum(r.byte_length for r in receipts),
                    )
                )
            case FoldTarget(tag="cube", cube=(ref, concat_dim)):
                manifest = ManifestWrite(cube=FieldVirtual(sources=sources, target=ref, concat_dim=concat_dim))
                # icechunk registration and commit block on store I/O — the banded thread hop, never the loop; `apply` is railed, so the hop carries the rail whole.
                outcome_rail = await on_thread(VirtualReference(sources=sources, ref=ref).apply, VersionOp(aggregate=(manifest, {}, None)))
                return outcome_rail.bind(
                    lambda outcome: (
                        self._rekey("cube", f"{concat_dim}|{'|'.join(sources)}".encode(), outcome.chunk_refs)
                        if isinstance(outcome, VirtualReceipt)
                        else Error(BoundaryFault(boundary=("stac.assets.cube", "VersionOp aggregate yielded no VirtualReceipt")))
                    )
                )
            case FoldTarget(tag="coverage", coverage=(groupby, resampling, chunks)):
                return (
                    await guarded(
                        RetryClass.HTTP, on_thread, lambda: self._coverage(groupby, resampling, chunks), abandon=True, subject="stac.assets.coverage"
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

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
