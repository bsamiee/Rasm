# [PY_DATA_CATALOG]

The cloud-native STAC discovery owner: one `StacCatalog` over `pystac-client` that resolves WHICH cloud assets cover a query region, the discovery layer the geospatial and cloud-egress lanes lack above the raster/vector claims (`geospatial/claim.md#GEO`) and the archival byte-window read (`cloud-egress/store.md#EGRESS`). `StacCatalog.discover` folds the one `StacQuery` tagged-union axis — bbox/datetime/collection/CQL2-filter discriminants — onto the single keyword-only `pystac_client.Client.search`, pages the lazy `ItemSearch` into the `pystac.Item` model, and emits a `StacReceipt` keyed by runtime `ContentIdentity` over the matched item-id set plus the `ItemSearch.matched` count. The discovered collection encodes as a `stac-geoparquet` columnar Arrow `RecordBatchReader` (`[3]-[TABLE]`) the `columnar/dataset.md#SCAN` scan and the `query/relational.md#QUERY` engine consume, and the asset hrefs fold (`[4]-[ASSETS]`) into the `cloud-egress/store.md#EGRESS` `ObjectEgress.GetRange` archival-byte fast-path and the `tensor/store.md#TENSOR` `TensorStore.virtual` virtual-reference cube — reading those settled fence contracts, never a second object-store transport or a second virtual-cube builder. Every bundle keys by exactly one runtime `ContentIdentity`.

## [1]-[INDEX]

- `[2]-[CATALOG]`: the `StacCatalog` discovery owner over `pystac-client` `Client.open`/`search`, the `StacQuery` tagged-union search axis, `ItemSearch` paging into `pystac.Item`.
- `[3]-[TABLE]`: the discovered item collection encoded as a `stac-geoparquet` columnar Arrow `RecordBatchReader` the `columnar` scan and `query` engine consume.
- `[4]-[ASSETS]`: the asset-href fold into `cloud-egress` `ObjectEgress.GetRange` and `tensor` `TensorStore.virtual`, the `StacReceipt` keyed by `ContentIdentity`.

## [2]-[CATALOG]

- Owner: `StacCatalog` — the one cloud-native discovery owner, a `Client.open`-bound STAC API root; `StacQuery` the tagged-union search axis (bbox/datetime/collection/CQL2-filter), folded by `match`/`case` closed by `assert_never` onto the single keyword-only `Client.search(*, bbox=, datetime=, collections=, filter=, filter_lang=, max_items=, limit=)`. A new search modality is one `StacQuery` case, never a `search_bbox`/`search_datetime`/`search_cql2` method family — bbox vs datetime vs collection vs CQL2 filter are parameter rows on the one `search`, the exact discrimination `pystac-client.md` L64 mandates.
- Cases: `StacQuery` rows `Bbox(west, south, east, north)` (spatial selection through `search(bbox=(w,s,e,n))`) · `Datetime(start, end)` (temporal selection through `search(datetime="<start>/<end>")` the RFC-3339 interval) · `Collection(ids)` (collection-scoped discovery through `search(collections=[...])`) · `Cql2Filter(predicate)` (the CQL2-JSON predicate through `search(filter=..., filter_lang="cql2-json")` — eo:cloud_cover/gsd/proj:epsg property predicates the STAC API evaluates server-side), each carrying its `params()` projection that contributes exactly its own keyword arguments so an n-axis query unions the per-case keyword dicts rather than forking a method per axis combination.
- Entry: `StacCatalog.open` admits a STAC API root URL plus the optional `headers` map the API auth boundary carries (the bearer/SAS request headers the cloud endpoint mandates), binding `Client.open(url, headers=...)`, never a forked signed-vs-unsigned client; `StacCatalog.discover` folds one `StacQuery` (or a tuple of them, whose `params()` projections union into one `search` keyword set) through `match`/`case`, runs the single `Client.search`, and pages the lazy `ItemSearch.item_collection()` into the `pystac.ItemCollection` of `pystac.Item`, capping the total with `max_items` and sizing each page request with `limit`; the return is a `RuntimeRail[StacResult]` carrying the items, the `ItemSearch.matched()` total, and the `StacReceipt`. The runtime `TransportResource` carries the asset-byte path (`[4]-[ASSETS]`) the `ObjectEgress.GetRange` egress speaks, never re-minted as a second STAC API credential owner.
- Auto: the `StacQuery.params` fold is the union law — each case spreads to its own keyword dict (`Bbox` to `{"bbox": ...}`, `Datetime` to `{"datetime": ...}`, `Collection` to `{"collections": ...}`, `Cql2Filter` to `{"filter": ..., "filter_lang": "cql2-json"}`) and a multi-axis discovery merges the spreads into one `search` call, so a bbox+datetime+cloud-cover query is one search, never three; `ItemSearch` is lazy so `matched()` reads the API-reported total without materializing every page, and `item_collection()` is the one canonical materialization (never the deprecated `get_all_items`, never a hand-rolled `next`-link follow loop the `ItemSearch` paging already owns); `pystac-client`/`pystac` import function-local under `# noqa: PLC0415` per the manifest boundary-scope-only import policy (`pystac-client.md` L62, `pystac.md` L72), the network search itself the boundary the `RuntimeRail` traps.
- Receipt: `StacCatalog.discover` contributes an emitted-phase `Receipt.of` through `ReceiptContributor` and produces a `StacReceipt` (`[4]-[ASSETS]`) keyed by `ContentIdentity` over the matched item-id set plus the `matched()` count, never a generic receipt.
- Packages: `pystac-client` (`Client.open(url, headers=, modifier=)`, the one keyword-only `Client.search(*, bbox=, datetime=, collections=, filter=, filter_lang=, max_items=, limit=)` returning `ItemSearch`, `ItemSearch.{item_collection,items,matched}` paging), `pystac` (`Item`/`ItemCollection`/`Asset` the search results hydrate into), runtime (`RuntimeRail`/`boundary`/`ContentIdentity`/`ReceiptContributor`; the `TransportResource` rides the `[4]-[ASSETS]` asset-byte fold, not the API search).
- Growth: a new search modality is one `StacQuery` case with one `params()` projection; a new ordering/projection is the `sortby`/`fields` keyword on the existing `search`; collection discovery (`Client.collection_search`/`get_collections`) composes on the same `Client` when an EO collection-catalog consumer admits; zero new surface.
- Boundary: composes the runtime `TransportResource` for the API credentials/signing, never a second credential owner; the discovery owner folds INTO the `cloud-egress`/`tensor`/`columnar` owners and never re-mints a STAC API paging loop, a CQL2 compiler, or a conformance negotiator `pystac-client` already owns; no live UI, no durable catalog store; a `search_by_<axis>` method family, a hand-rolled `next`-link page loop, the deprecated `get_all_items`, and a parallel API-vs-file client where `Client` extends `pystac.Catalog` are the deleted forms.

```python signature
from typing import Literal, assert_never

from expression import case, tag, tagged_union
from msgspec import Struct, field

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

type Bound = tuple[float, float, float, float]
type SearchParams = dict[str, object]
type Headers = dict[str, str]


@tagged_union(frozen=True)
class StacQuery:
    tag: Literal["bbox", "datetime", "collection", "cql2_filter"] = tag()
    bbox: Bound = case()
    datetime: tuple[str, str] = case()
    collection: tuple[str, ...] = case()
    cql2_filter: dict[str, object] = case()

    @staticmethod
    def Bbox(west: float, south: float, east: float, north: float) -> "StacQuery":
        return StacQuery(bbox=(west, south, east, north))

    @staticmethod
    def Datetime(start: str, end: str) -> "StacQuery":
        return StacQuery(datetime=(start, end))

    @staticmethod
    def Collection(*ids: str) -> "StacQuery":
        return StacQuery(collection=ids)

    @staticmethod
    def Cql2Filter(predicate: dict[str, object]) -> "StacQuery":
        return StacQuery(cql2_filter=predicate)

    def params(self) -> SearchParams:
        match self:
            case StacQuery(tag="bbox", bbox=extent):
                return {"bbox": list(extent)}
            case StacQuery(tag="datetime", datetime=(start, end)):
                return {"datetime": f"{start}/{end}"}
            case StacQuery(tag="collection", collection=ids):
                return {"collections": list(ids)}
            case StacQuery(tag="cql2_filter", cql2_filter=predicate):
                return {"filter": predicate, "filter_lang": "cql2-json"}
            case unreachable:
                assert_never(unreachable)


class StacResult(Struct, frozen=True):
    item_ids: tuple[str, ...]
    matched: int
    href_count: int
    content_key: ContentKey


class StacReceipt(Struct, frozen=True):
    endpoint: str
    item_count: int
    matched: int
    content_key: ContentKey

    def contribute(self) -> Receipt:
        return Receipt.of(
            "emitted", "stac-catalog", self.endpoint,
            {"items": str(self.item_count), "matched": str(self.matched)},
        )


class StacCatalog(Struct, frozen=True):
    endpoint: str
    headers: Headers = field(default_factory=dict)

    @classmethod
    def open(cls, endpoint: str, headers: Headers | None = None) -> "StacCatalog":
        return cls(endpoint=endpoint, headers=headers or {})

    def discover(self, *queries: StacQuery, max_items: int | None = None, limit: int | None = None) -> "RuntimeRail[StacResult]":
        return boundary("stac.discover", lambda: self._search(queries, max_items, limit))

    def _search(self, queries: "tuple[StacQuery, ...]", max_items: int | None, limit: int | None) -> StacResult:
        from pystac_client import Client  # noqa: PLC0415

        client = Client.open(self.endpoint, headers=self.headers)
        params: SearchParams = {"max_items": max_items, "limit": limit}
        for query in queries:
            params.update(query.params())
        search = client.search(**params)
        collection = search.item_collection()
        item_ids = tuple(item.id for item in collection)
        hrefs = tuple(asset.href for item in collection for asset in item.assets.values())
        return StacResult(
            item_ids=item_ids,
            matched=search.matched() or len(item_ids),
            href_count=len(hrefs),
            content_key=ContentIdentity.of("stac.discover", "\n".join(item_ids).encode()),
        )
```

## [3]-[TABLE]

- Owner: the `stac_table` encoder — the discovered `pystac.ItemCollection` to a `stac-geoparquet` columnar Arrow `pyarrow.RecordBatchReader`, the one carrier the `columnar/dataset.md#SCAN` scan and the `query/relational.md#QUERY` engine consume. Encoding is the `stac_geoparquet.arrow.parse_stac_items_to_arrow` parse over the in-memory items; GeoParquet egress is `stac_geoparquet.arrow.to_parquet` keyed by a `schema_version` from `SUPPORTED_PARQUET_SCHEMA_VERSIONS`; rehydration is `stac_geoparquet.arrow.stac_table_to_items` paired with `pystac.Item.from_dict` — never a hand-built Arrow schema where the `ACCEPTED_SCHEMA_OPTIONS` inference applies, never a hand-rolled parquet writer where `to_parquet` versions the schema, never the legacy geopandas trio where the zero-copy Arrow path is the canonical carrier.
- Cases: the schema axis is the `ACCEPTED_SCHEMA_OPTIONS` string literal — `"FullFile"` scans every batch for the widest schema (the discovery default, correctness over a heterogeneous multi-collection result) and `"FirstBatch"` infers from the first batch (the lower-latency single-collection fast-path) — a parameter row on `parse_stac_items_to_arrow`, not a parallel parse function.
- Entry: `stac_table` admits the `StacResult` items (or the live `ItemCollection`), parses them through `parse_stac_items_to_arrow(items, schema="FullFile")` into the `RecordBatchReader`, and returns a `RuntimeRail[pa.RecordBatchReader]` the columnar scan registers directly; `stac_geoparquet_egress` writes that table through `to_parquet(table, output_path, schema_version=...)` to the GeoParquet path the `cloud-egress` `ObjectEgress.Put` then uploads, keyed by one `ContentIdentity`; `stac_table_rehydrate` runs `stac_table_to_items(table)` over a read-back STAC table and rebuilds the model with `pystac.Item.from_dict`, never re-minting a STAC item the `pystac` owner already models.
- Auto: the item table crosses as the zero-copy `pyarrow.RecordBatchReader` so the columnar scan consumes it without a Python-side copy; `parse_stac_items_to_arrow` accepts the `pystac.Item` iterable directly (no NDJSON round-trip), and `to_parquet` stamps the GeoParquet STAC schema version so a downstream reader resolves the column layout without a side-channel; `stac-geoparquet` and `pystac` import function-local under `# noqa: PLC0415` per the boundary-scope-only import policy (`stac-geoparquet.md` L63), the parse/write the boundary the `RuntimeRail` traps; the `geopandas`-backed `to_geodataframe`/`to_item_collection` trio stays a legacy fallback never called here — the `arrow.*` namespace is the pure-Python carrier and is the one the page binds.
- Receipt: the table encode folds the shared `columnar` `QueryReceipt` (`columnar/dataset.md` `QueryReceipt`) over the encoded Arrow table keyed by `ContentIdentity`, the same receipt the columnar scan and spatial engine emit, never a parallel table-receipt rail.
- Packages: `stac-geoparquet` (`arrow.parse_stac_items_to_arrow(items, chunk_size=, schema=, tmpdir=)` returning `pa.RecordBatchReader`, `arrow.to_parquet(table, output_path, schema_version=)`, `arrow.stac_table_to_items(table)`, the `ACCEPTED_SCHEMA_OPTIONS`/`SUPPORTED_PARQUET_SCHEMA_VERSIONS` schema axis), `pystac` (`Item.from_dict` rehydration), `pyarrow` (the `RecordBatchReader`/`Table` carrier), runtime (`ContentIdentity`/`RuntimeRail`/`boundary`).
- Growth: a new schema-inference mode is the `ACCEPTED_SCHEMA_OPTIONS` row on the parse; a new GeoParquet schema version is a `schema_version` from `SUPPORTED_PARQUET_SCHEMA_VERSIONS`; an NDJSON ingest source is the sibling `parse_stac_ndjson_to_arrow` on the same carrier; an optional Delta sink is `parse_stac_ndjson_to_delta_lake`; zero new surface.
- Boundary: composes the `columnar`/`query` owners for the table scan and the `cloud-egress` owner for the GeoParquet upload, never a second table engine or a second writer; no durable catalog store; a hand-built STAC-to-Arrow schema mapping, a hand-rolled parquet writer, re-minting a STAC item where `from_dict` rehydrates, and defaulting to the geopandas trio where the zero-copy Arrow path applies are the deleted forms.

```python signature
from typing import TYPE_CHECKING, Literal

import pyarrow as pa

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary

if TYPE_CHECKING:
    from collections.abc import Iterable

type SchemaInference = Literal["FullFile", "FirstBatch"]


def stac_table(items: "Iterable[object]", *, schema: SchemaInference = "FullFile") -> "RuntimeRail[pa.RecordBatchReader]":
    def _parse() -> "pa.RecordBatchReader":
        from stac_geoparquet.arrow import parse_stac_items_to_arrow  # noqa: PLC0415

        return parse_stac_items_to_arrow(items, schema=schema)

    return boundary("stac.table", _parse)


def stac_geoparquet_egress(table: "pa.RecordBatchReader | pa.Table", output_path: str, *, schema_version: str | None = None) -> "RuntimeRail[ContentKey]":
    def _write() -> ContentKey:
        from pathlib import Path  # noqa: PLC0415

        from stac_geoparquet.arrow import DEFAULT_PARQUET_SCHEMA_VERSION, to_parquet  # noqa: PLC0415

        to_parquet(table, output_path, schema_version=schema_version or DEFAULT_PARQUET_SCHEMA_VERSION)
        return ContentIdentity.of("stac.geoparquet", Path(output_path).read_bytes())

    return boundary("stac.geoparquet", _write)


def stac_table_rehydrate(table: "pa.Table") -> "RuntimeRail[tuple[object, ...]]":
    def _rehydrate() -> "tuple[object, ...]":
        import pystac  # noqa: PLC0415
        from stac_geoparquet.arrow import stac_table_to_items  # noqa: PLC0415

        return tuple(pystac.Item.from_dict(record) for record in stac_table_to_items(table))

    return boundary("stac.rehydrate", _rehydrate)
```

## [4]-[ASSETS]

- Owner: the asset-href fold — a fold over the discovered `pystac.Item` asset hrefs INTO the `cloud-egress/store.md#EGRESS` `ObjectEgress.GetRange` archival byte-window fast-path and the `tensor/store.md#TENSOR` `TensorStore.virtual` virtual-reference cube, NOT a new owner. The fold reads only the intersecting COG/COPC byte windows through the existing settled `GetRange` fence and lazily references discovered cube assets through the existing settled `virtual`/`_virtual_cube` fence, re-keying through the one `StacReceipt`; never a second object-store transport, never a second virtual-cube builder.
- Cases: the fold discriminates the asset role by media type — a `MediaType.COG`/`MediaType.GEOTIFF` raster asset folds to the `ObjectEgress.GetRange(path, start, end)` archival byte-window read (the COG header/overview/tile byte ranges the consumer requests), while the set of cube-bearing asset hrefs folds to `TensorStore.virtual(sources, ref)` building the one virtual-reference datacube over the source URLs — the media type IS the route, a `pystac.MediaType` row, never a parallel per-asset egress class.
- Entry: `fold_asset_egress` admits the discovered items and an `ObjectEgress` (constructed once over the runtime `TransportResource`, `cloud-egress/store.md#EGRESS` L13), iterates the `Item.assets` map, and for each raster asset runs `ObjectEgress.run(StoreOp.GetRange(href, start, end), href)` over the requested byte window — the egress owner short-circuits an unchanged content-key to a by-reference no-op, so a re-discovery of the same asset never re-reads; `fold_virtual_cube` collects the cube-bearing asset hrefs and runs `TensorStore.virtual(hrefs, ref, concat_dim=...)` building the one virtualizarr datacube over the source URLs; both return a `RuntimeRail` and fold one `StacReceipt` keyed by `ContentIdentity` over the matched item-id set.
- Auto: the byte window is the COG/COPC archival range the consumer supplies (the GeoTIFF IFD or the COPC octree node byte offsets), passed straight to `GetRange` so the read is one HTTP range request, never a full-object materialization; the virtual cube reuses the `tensor` owner's mandatory `ObjectStoreRegistry` URL-to-`obstore` backend map, so the STAC asset URLs register through the same `from_url` transport the egress owner speaks — one transport across discovery, egress, and cube; the `StacReceipt` keys by `ContentIdentity.of` over the joined matched item-id set, so an unchanged discovery result is byte-stable and a single new item flips the key.
- Receipt: the asset fold produces the one `StacReceipt` keyed by `ContentIdentity` over the matched item-id set plus the `ItemSearch.matched()` count and the folded href count, contributing an emitted-phase `Receipt.of` through `ReceiptContributor`; no new receipt rail beyond the `StacReceipt` declared in `[2]-[CATALOG]`.
- Packages: `pystac` (`Item.assets`/`Asset.href`/`Asset.media_type`/`MediaType`), `cloud-egress` (`ObjectEgress`/`StoreOp.GetRange`, `store.md#EGRESS` L12), `tensor` (`TensorStore.virtual`, `store.md#TENSOR` L132), runtime (`ContentIdentity`/`RuntimeRail`/`boundary`/`ReceiptContributor`).
- Growth: a new archival format is one more `MediaType` row routed to `GetRange`; a new cube backend is the `tensor` `TensorBackend` axis with zero change here; a per-collection cube is one `concat_dim` on the existing `TensorStore.virtual`; zero new surface.
- Boundary: reads the settled `cloud-egress` `GetRange` and `tensor` `virtual` fences and never re-mints either — no second object-store transport, no second virtual-cube builder, no full-object read where a byte window applies, no durable asset store; a per-asset egress class family and a re-derived virtual cube are the deleted forms.

```python signature
from typing import TYPE_CHECKING

from msgspec import Struct

from rasm.data.cloud_egress.store import ObjectEgress, StoreOp
from rasm.data.tensor.store import TensorStore
from rasm.runtime.content_identity import ContentIdentity
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.roots import ResourceRef

if TYPE_CHECKING:
    from collections.abc import Iterable, Mapping


class AssetWindow(Struct, frozen=True):
    href: str
    start: int
    end: int


def fold_asset_egress(items: "Iterable[object]", egress: ObjectEgress, windows: "Mapping[str, AssetWindow]") -> "RuntimeRail[StacReceipt]":
    def _fold() -> StacReceipt:
        item_ids: list[str] = []
        href_count = 0
        for item in items:
            item_ids.append(item.id)
            for asset in item.assets.values():
                window = windows.get(asset.href)
                if window is None:
                    continue
                egress.run(StoreOp.GetRange(window.href, window.start, window.end), window.href)
                href_count += 1
        return StacReceipt(
            endpoint=egress.ref.root, item_count=len(item_ids), matched=len(item_ids),
            content_key=ContentIdentity.of("stac.assets", "\n".join(item_ids).encode()),
        )

    return boundary("stac.assets.egress", _fold)


def fold_virtual_cube(items: "Iterable[object]", ref: ResourceRef, *, media_type: str, concat_dim: str = "time") -> "RuntimeRail[object]":
    def _fold() -> object:
        sources = tuple(
            asset.href
            for item in items
            for asset in item.assets.values()
            if asset.media_type == media_type
        )
        return TensorStore.virtual(sources, ref, concat_dim=concat_dim)

    return boundary("stac.assets.virtual", _fold)
```

## [5]-[RESEARCH]

- [STAC_DISTRIBUTION_SYNC]: the three `pystac-client`/`pystac`/`stac-geoparquet` catalogues are reflection-grade authored from the canonical sources and lock-resolved (all three pure-Python `py3-none-any`, cp315-clean, ungated), but the live distributions are pending a `uv sync` into the active venv — the member surfaces the fences transcribe (`Client.search(*, bbox=, datetime=, collections=, filter=, filter_lang=, max_items=, limit=)` returning `ItemSearch`, `ItemSearch.{item_collection,matched}`, `arrow.parse_stac_items_to_arrow(items, schema=)` returning a `pa.RecordBatchReader`, `arrow.to_parquet(table, output_path, schema_version=)`, `arrow.stac_table_to_items(table)`, `pystac.Item.{assets,from_dict}`, `pystac.MediaType`) confirm against the synced distributions before they settle. The two version-sensitive open spellings: the `Client.open(url, headers=...)` header-injection keyword (`pystac-client.md` L39) the API auth boundary binds — and the `modifier=`/`request_modifier=` href-signing extension if a SAS-token endpoint admits it, the optional auth row the `headers` field generalizes — and the `stac_geoparquet.arrow` GeoParquet schema-version constant name (`DEFAULT_PARQUET_SCHEMA_VERSION` vs `SUPPORTED_PARQUET_SCHEMA_VERSIONS`, `stac-geoparquet.md` L27) the egress stamps — both confirm against the live `arrow`/`Client` surfaces before the receipt treats the header injection and the schema version as settled discovery evidence. The CQL2-JSON `filter_lang="cql2-json"` value and the `eo:cloud_cover`/`proj:epsg` property predicates are STAC API conformance-class surfaces the server evaluates, confirmed against the target endpoint's advertised conformance before the `Cql2Filter` arm treats the predicate as accepted.
