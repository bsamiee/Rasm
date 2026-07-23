# [PY_DATA_API_PYSTAC_CLIENT]

`pystac-client` owns the live STAC API client for the data STAC-catalog rail: a `Client` — a `pystac.Catalog` bound to a STAC API root — whose one keyword-only `search` lazily pages the `/search` endpoint into `pystac.Item` objects, beside `collection_search`/`get_collections` for collection discovery. Conformance negotiation, CQL2 filtering, and result paging fold through this surface; `pystac` owns the in-memory model the results hydrate into.

## [01]-[PACKAGE_SURFACE]

- package: `pystac-client` (Apache-2.0)
- module: `import pystac_client` (dist `pystac-client`)
- entry points: `stac-client` console script; library use is import-only
- rail: STAC catalog

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client, search, and collection roots

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]       | [CAPABILITY]                                                                  |
| :-----: | :----------------------------- | :------------------ | :---------------------------------------------------------------------------- |
|  [01]   | `Client`                       | API catalog root    | a `pystac.Catalog` bound to a STAC API; owns `search`/collection access       |
|  [02]   | `ItemSearch`                   | item result page    | lazy paging of `/search` into `pystac.Item`; match counts                     |
|  [03]   | `CollectionClient`             | live collection     | a `pystac.Collection` with live `get_items`/`get_item`/`get_queryables`       |
|  [04]   | `CollectionSearch`             | collection result   | lazy paging of `/collections` with free-text and filter parameters            |
|  [05]   | `StacApiIO`                    | HTTP/paging I/O     | `pystac.StacIO` over `requests`; `get_pages`, `request_modifier`, retry       |
|  [06]   | `ConformanceClasses`           | conformance enum    | conformance gate (`CORE`/`ITEM_SEARCH`/`FILTER`/`SORT`/`FIELDS` …)            |
|  [07]   | `Modifiable`                   | modifier signature  | union the `modifier` callback receives (Item/Collection/ItemCollection/dict)  |
|  [08]   | `APIError` / `ParametersError` | error rail          | HTTP/response failure; invalid-parameter failure (`pystac_client.exceptions`) |
|  [09]   | `PystacClientWarning`          | conformance warning | fires when the API under-declares conformance and the client degrades         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client open and search
- open carry: `url`, `headers`, `parameters`, `modifier`, `request_modifier`, `stac_io`, `timeout`
- search carry (keyword-only): `bbox`|`intersects`, `datetime`, `ids`, `collections`, `query`|`filter`, `filter_lang`, `sortby`, `fields`, `method`, `max_items`, `limit`
- collection_search carry (keyword-only): `bbox`, `datetime`, `q`, `query`, `filter`, `filter_lang`, `sortby`, `fields`, `max_collections`, `limit`
- `max_items` caps the total result count across pages; `limit` sizes each page request

| [INDEX] | [SURFACE]                                                     | [CAPABILITY]                                                             |
| :-----: | :------------------------------------------------------------ | :----------------------------------------------------------------------- |
|  [01]   | `Client.open(url, ...) -> Client`                             | open a STAC API root; `modifier`/`request_modifier` thread signing/auth  |
|  [02]   | `Client.search(*, ...) -> ItemSearch`                         | item search; params discriminate bbox/intersects/CQL2 `filter`/`sortby`  |
|  [03]   | `Client.get_collections() -> Iterator[Collection]`            | iterate the API's collections                                            |
|  [04]   | `Client.get_collection(id) -> Collection \| CollectionClient` | fetch one collection; a live `CollectionClient` when the API supports it |
|  [05]   | `Client.collection_search(*, ...) -> CollectionSearch`        | free-text (`q`) and filter collection search                             |
|  [06]   | `Client.get_queryables() -> dict`                             | discover CQL2-filterable properties for a `filter`                       |
|  [07]   | `Client.get_merged_queryables(collections) -> dict`           | queryables merged across the named collections                           |
|  [08]   | `Client.conforms_to(cls) -> bool`                             | conformance gate; `set_conforms_to`/`add_conforms_to` override           |

[ENTRYPOINT_SCOPE]: `ItemSearch` and `CollectionSearch` result paging

| [INDEX] | [SURFACE]                                                | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------- | :----------------------------------------- |
|  [01]   | `ItemSearch.item_collection() -> ItemCollection`         | materialize all results (canonical)        |
|  [02]   | `ItemSearch.items() -> Iterator[Item]`                   | lazily stream items across pages           |
|  [03]   | `ItemSearch.items_as_dicts() -> Iterator[dict]`          | lazily stream raw item dicts               |
|  [04]   | `ItemSearch.item_collection_as_dict() -> dict`           | materialize as a GeoJSON FeatureCollection |
|  [05]   | `ItemSearch.pages() -> Iterator[ItemCollection]`         | iterate whole result pages                 |
|  [06]   | `ItemSearch.pages_as_dicts() -> Iterator[dict]`          | iterate raw page dicts                     |
|  [07]   | `ItemSearch.matched() -> int \| None`                    | total matching item count (if reported)    |
|  [08]   | `ItemSearch.url_with_parameters() -> str`                | the resolved GET request URL (receipt)     |
|  [09]   | `CollectionSearch.collection_list() -> list[Collection]` | materialize all matching collections       |
|  [10]   | `CollectionSearch.collections() -> Iterator[Collection]` | lazily stream matching collections         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one keyword-only `search` owns item discovery — spatial selection is `bbox` vs `intersects`, predicate selection is `query` vs CQL2 `filter`/`filter_lang`, ordering and projection are `sortby`/`fields`, all parameter rows on the one method, never a `search_by_bbox`/`search_by_geometry`/`search_by_filter` family.
- `Client.open(url)` is the single bound-catalog entry; it is a `pystac.Catalog`, so the in-memory traversal surface is inherited rather than forked into a parallel API-vs-file client.
- `ItemSearch` pages lazily — `max_items` caps the total, `limit` sizes each request, `item_collection()` materializes, `items()`/`pages()` stream, `*_as_dicts()` skip hydration — never a hand-rolled `next`-link follow loop.
- `Client.conforms_to(ConformanceClasses.<X>)` gates capability; `search`/`collection_search`/`filter`/`sortby`/`fields` raise or warn (`DoesNotConformTo`/`FallbackToPystac`) when the API under-declares, and `get_queryables`/`get_merged_queryables` enumerate the CQL2-filterable property set before a `filter` is built.
- `get_collections`/`get_collection` discover collections — `get_collection` returns a live `CollectionClient` when the API advertises it, else a static `pystac.Collection` — and `CollectionClient.get_items`/`get_item`/`get_queryables` is the per-collection live access.
- `modifier: Callable[[Modifiable], None]` mutates hydrated objects for asset-href signing and `request_modifier` mutates the outbound `requests.Request` for auth headers; both are boundary rows on `open`, never a forked signed-vs-unsigned client.
- each search captures the negotiated conformance classes, the `matched()` count, page count, and resolved `url_with_parameters()` as a discovery receipt.

[STACKING]:
- `pystac`(`.api/pystac.md`): `Client.search(...).items()` yields live `pystac.Item` objects in that model; the data owner consumes them through the same `Item`/`Asset` surface, never a second item type.
- `planetary-computer`(`.api/planetary-computer.md`): `Client.open(url, modifier=planetary_computer.sign_inplace)` signs asset hrefs in place as objects hydrate.
- `stac-geoparquet`(`.api/stac-geoparquet.md`): `ItemSearch.item_collection()` -> `stac_geoparquet.arrow.parse_stac_items_to_arrow` for an Arrow/GeoParquet item table.
- `odc-stac`(`.api/odc-stac.md`): `odc.stac.load(items, bands=...)` assembles a CRS-aware `xarray` cube from the searched items.
- within-lib: `get_queryables()` -> a CQL2 `filter` dict with `filter_lang='cql2-json'` -> `search(filter=...)`; `StacApiIO` retry and `parameters` request defaults thread through paging, and `APIError`/`ParametersError` lift into the domain `Result` at the calling boundary.

[LOCAL_ADMISSION]:
- `pystac-client` is the sole admitted live STAC API client; a hand-rolled `requests` loop against `/search` or `/collections` is rejected.

[RAIL_LAW]:
- Package: `pystac-client`
- Owns: the STAC API client (`Client` over a catalog root), conformance-negotiated item search with bbox/datetime/intersects/CQL2/sortby/fields parameters, lazy result paging into `pystac.Item`, match counts, queryables/conformance introspection, and collection discovery and free-text search
- Accept: `Client.open` with `modifier`/`request_modifier`, one keyword-only `search` discriminating by parameter, `ItemSearch.item_collection()`/`items()`/`pages()` paging, `collection_search`/`get_collections` discovery, `get_queryables`/`conforms_to` capability negotiation before a CQL2 `filter`
- Reject: wrapper-renames of `open`/`search`; a `search_by_<axis>` method family where one `search` discriminates by parameter; a hand-rolled `next`-link paging loop where `ItemSearch` pages; a parallel API-vs-file client where `Client` extends `pystac.Catalog`; a forked signed-vs-unsigned client where `modifier` is one boundary row
