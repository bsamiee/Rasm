# [PY_DATA_API_PYSTAC_CLIENT]

`pystac-client` (dist `pystac-client`, import `pystac_client`) supplies the live STAC API client for the data STAC-catalog rail: a `Client` (a `pystac.Catalog` bound to a STAC API root) whose one polymorphic `search(...)` returns an `ItemSearch` that lazily pages the `/search` endpoint into `pystac.Item` objects, plus `collection_search`/`get_collections` for collection discovery. The package owner composes `Client.open`, `search`, and the `ItemSearch` paging surface into the STAC discovery owner; it never re-implements the STAC API paging, CQL2 filtering, or conformance negotiation pystac-client already owns.

## [01]-[PACKAGE_SURFACE]

- package: `pystac-client` `0.9.0`
- import: `import pystac_client` (dist name `pystac-client`, import name `pystac_client`)
- license: Apache-2.0
- python: `>=3.10`
- deps: `pystac` (in-memory model), `requests` (HTTP), `python-dateutil`
- entry points: console script `stac-client` (CLI); library use is import-only
- owner: `data`
- rail: STAC catalog
- capability: STAC API client over a catalog root — conformance-negotiated item search with bbox/datetime/intersects/CQL2 filter/sortby/fields/ids parameters, lazy result paging into `pystac.Item`, total-match counts, queryables/conformance introspection, collection discovery and free-text collection search, and request modifiers for auth/signing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client, search, and collection roots
- rail: STAC catalog

`Client` extends `pystac.Catalog`; `CollectionClient` extends `pystac.Collection`. `ItemSearch`/`CollectionSearch` are lazy result iterators that page the API on demand. `StacApiIO` is the `pystac.StacIO` subclass that owns HTTP + paging + the `request_modifier` hook. `PystacClientWarning` leaves — `DoesNotConformTo`/`MissingLink`/`NoConformsTo`/`FallbackToPystac` — fire when the API under-declares conformance.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]       | [CAPABILITY]                                                                     |
| :-----: | :----------------------------- | :------------------ | :------------------------------------------------------------------------------- |
|  [01]   | `Client`                       | API catalog root    | a `pystac.Catalog` bound to a STAC API; owns `search`/collection access          |
|  [02]   | `ItemSearch`                   | item result page    | lazy paging of `/search` into `pystac.Item`; match counts                        |
|  [03]   | `CollectionClient`             | live collection     | a `pystac.Collection` with live `get_items`/`get_item`/`get_queryables`          |
|  [04]   | `CollectionSearch`             | collection result   | lazy paging of `/collections` with free-text and filter parameters               |
|  [05]   | `StacApiIO`                    | HTTP/paging I/O     | `pystac.StacIO` over `requests`; `get_pages`, `request_modifier`, retry          |
|  [06]   | `ConformanceClasses`           | conformance enum    | `CORE`/`ITEM_SEARCH`/`FILTER`/`QUERY`/`SORT`/`FIELDS`/`COLLECTION_SEARCH`/… gate |
|  [07]   | `Modifiable`                   | modifier signature  | union the `modifier` callback receives (Item/Collection/ItemCollection/dict)     |
|  [08]   | `APIError` / `ParametersError` | error rail          | HTTP/response failure; invalid-parameter failure (`pystac_client.exceptions`)    |
|  [09]   | `PystacClientWarning`          | conformance warning | fires when the API under-declares conformance and the client degrades            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client open and search
- rail: STAC catalog
- call: `Client.open(url, headers=None, parameters=None, ignore_conformance=None, modifier=None, request_modifier=None, stac_io=None, timeout=None) -> Client`
- call: `Client.search(*, method='POST', max_items=None, limit=None, ids=None, collections=None, bbox=None, intersects=None, datetime=None, query=None, filter=None, filter_lang=None, sortby=None, fields=None) -> ItemSearch`
- call: `Client.get_collections() -> Iterator[Collection]`; `Client.get_collection(collection_id) -> Collection | CollectionClient`
- call: `Client.collection_search(*, max_collections=None, limit=None, bbox=None, datetime=None, q=None, query=None, filter=None, filter_lang=None, sortby=None, fields=None) -> CollectionSearch`
- call: `Client.get_queryables() -> dict`; `Client.get_merged_queryables(collections: list[str]) -> dict`
- call: `Client.conforms_to(conformance_class: ConformanceClasses | str) -> bool`; `set_conforms_to([...])`; `add_conforms_to(name)`

`search` is keyword-only; `max_items` caps the total result count across pages while `limit` is the per-page request size. The same `search` call discriminates by which parameters are supplied — bbox vs intersects vs CQL2 `filter` are rows, not parallel methods.

| [INDEX] | [SURFACE]                  | [CAPABILITY]                                                                      |
| :-----: | :------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `Client.open`              | open a STAC API root; `modifier`/`request_modifier` thread signing/auth           |
|  [02]   | `Client.search`            | item search; params discriminate bbox/intersects/CQL2 `filter`/`sortby`/`fields`  |
|  [03]   | `Client.get_collections`   | iterate the API's collections                                                     |
|  [04]   | `Client.get_collection`    | fetch one collection (cached; a live `CollectionClient` when the API supports it) |
|  [05]   | `Client.collection_search` | free-text (`q`) plus filter collection search                                     |
|  [06]   | `Client.get_queryables`    | discover CQL2-filterable properties, merged across collections                    |
|  [07]   | `Client.conforms_to`       | conformance negotiation gate                                                      |

[ENTRYPOINT_SCOPE]: `ItemSearch` result paging
- rail: STAC catalog

`item_collection()` (and `item_collection_as_dict()`) is the canonical materialization; `items()`/`items_as_dicts()` stream lazily; `pages()` yields whole `ItemCollection` pages. `get_all_items` is deprecated (emits `FutureWarning`) — use `item_collection()`. `CollectionSearch` mirrors this surface — `collections`/`collections_as_dicts`/`pages`/`pages_as_dicts`/`matched` beside `collection_list()`.

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]                            | [CAPABILITY]                               |
| :-----: | :----------------------------------- | :-------------------------------------- | :----------------------------------------- |
|  [01]   | `ItemSearch.item_collection`         | `item_collection() -> ItemCollection`   | materialize all results (canonical)        |
|  [02]   | `ItemSearch.items`                   | `items() -> Iterator[Item]`             | lazily stream items across pages           |
|  [03]   | `ItemSearch.items_as_dicts`          | `items_as_dicts() -> Iterator[dict]`    | lazily stream raw item dicts               |
|  [04]   | `ItemSearch.item_collection_as_dict` | `item_collection_as_dict() -> dict`     | materialize as a GeoJSON FeatureCollection |
|  [05]   | `ItemSearch.pages`                   | `pages() -> Iterator[ItemCollection]`   | iterate whole result pages                 |
|  [06]   | `ItemSearch.pages_as_dicts`          | `pages_as_dicts() -> Iterator[dict]`    | iterate raw page dicts                     |
|  [07]   | `ItemSearch.matched`                 | `matched() -> int \| None`              | total matching item count (if reported)    |
|  [08]   | `ItemSearch.url_with_parameters`     | `url_with_parameters() -> str`          | the resolved GET request URL (receipt)     |
|  [09]   | `CollectionSearch.collection_list`   | `collection_list() -> list[Collection]` | materialize all matching collections       |
|  [10]   | `CollectionSearch.collections`       | `collections() -> Iterator[Collection]` | lazily stream matching collections         |

## [04]-[IMPLEMENTATION_LAW]

[STAC_CLIENT]:
- import: `import pystac_client` at boundary scope only; module-level import is banned by the manifest import policy. The dist name is `pystac-client`; the import name is `pystac_client`.
- entry axis: `Client.open(url)` is the single bound-catalog entry; it is a `pystac.Catalog`, so the in-memory traversal surface is inherited — never a parallel API-vs-file client type.
- search axis: one keyword-only `search(...)` owns item discovery; spatial selection is `bbox` vs `intersects`, predicate selection is `query` vs CQL2 `filter`/`filter_lang`, and ordering/projection are `sortby`/`fields` — all parameter rows on the one method, never a `search_by_bbox`/`search_by_geometry`/`search_by_filter` family.
- paging axis: `ItemSearch` is lazy; `max_items` caps the total and `limit` sizes each page request — `item_collection()` materializes, `items()`/`pages()` stream, `items_as_dicts()`/`pages_as_dicts()` skip `pystac.Item` hydration for raw-dict throughput; never a hand-rolled `next`-link follow loop, and never the deprecated `get_all_items`/`item_collections`/`get_item_collections` (each emits `FutureWarning`).
- conformance axis: `Client` negotiates capability via `conforms_to(ConformanceClasses.<X>)`; `search`/`collection_search`/`filter`/`sortby`/`fields` raise or warn (`DoesNotConformTo`/`FallbackToPystac`) when the API under-declares. `ignore_conformance` on `open` (deprecated) forces the check off; prefer letting the warning surface. `get_queryables`/`get_merged_queryables` enumerate the CQL2-filterable property set before building a `filter`.
- collection axis: `get_collections`/`get_collection` discover collections (`get_collection` returns a live `CollectionClient` when the API supports it, else a static `pystac.Collection`) and `collection_search` runs free-text (`q`) and filter collection queries; `CollectionClient.get_items`/`get_item`/`get_queryables` is the per-collection live access.
- modifier axis: `modifier: Callable[[Modifiable], None]` mutates hydrated objects (e.g. `planetary_computer.sign_inplace` SAS signing of asset hrefs); `request_modifier: Callable[[requests.Request], requests.Request | None]` mutates the outbound request (auth headers) — both are boundary rows on `open`, never a forked signed-vs-unsigned client. `StacApiIO` owns the `requests.Session`, retry, and `get_pages` paging the modifiers thread through.
- evidence: each search captures the conformance classes negotiated, the result `matched()` count, page count, the resolved `url_with_parameters()`, and parameter set as a discovery receipt.
- boundary: pystac-client owns live STAC API search and paging; `pystac` owns the in-memory model the results hydrate into; `stac-geoparquet` owns the item-table interchange beside it; the asset HREFs fold into the cloud-egress and tensor-cube owners; live UI stays outside this package.

[INTEGRATION_STACK]:
- the discovery rail stacks `Client.open(url, modifier=planetary_computer.sign_inplace)` -> one keyword `search(bbox=, datetime=, collections=, filter=<CQL2>)` -> `ItemSearch.item_collection()` (a `pystac.ItemCollection`) -> `stac-geoparquet` (`stac_geoparquet.arrow.parse_stac_items_to_arrow`) for an Arrow/GeoParquet item table, or `odc-stac.load(items, ...)` / `stackstac` to a CRS-aware `xarray` cube. The `planetary-computer` sibling owns the signing modifier; pystac-client owns the search.
- asset egress stacks the hydrated `pystac.Item.assets[<key>].href` (SAS-signed by the modifier) into `rioxarray`/`rasterio` (`/vsicurl/` COG reads), `obstore` (raw object fetch), and the tensor-cube owner — pystac-client never reads pixels, only resolves and signs hrefs.
- the filter rail stacks `get_queryables()` (allowed CQL2 properties) -> a CQL2 `filter` dict with `filter_lang='cql2-json'` -> `search(filter=...)`; the same CQL2 vocabulary discriminates item vs collection search, never a parallel predicate API.
- `StacApiIO`/`requests` retry and `parameters` (query defaults applied to every request) stack with a `stamina`/`tenacity` retry boundary at the calling owner for transient HTTP failures, and `APIError`/`ParametersError` are the typed rail the boundary lifts into the domain `Result`.

[RAIL_LAW]:
- Package: `pystac-client`
- Owns: the STAC API client (`Client` over a catalog root), conformance-negotiated item search with bbox/datetime/intersects/CQL2/sortby/fields parameters, lazy result paging into `pystac.Item`, match counts, queryables/conformance introspection, and collection discovery/free-text search
- Accept: `Client.open` with `modifier`/`request_modifier`, one keyword-only `search` discriminating by parameter, `ItemSearch.item_collection()`/`items()`/`pages()` (and `*_as_dicts` raw variants) paging, `collection_search`/`get_collections` for collection discovery, `get_queryables`/`conforms_to` for capability negotiation before a CQL2 `filter`
- Reject: wrapper-renames of `open`/`search`; a `search_by_<axis>` method family where the one `search` discriminates by parameter; a hand-rolled `next`-link paging loop where `ItemSearch` pages; the deprecated `get_all_items`/`item_collections` where `item_collection()` is canonical; a parallel API-vs-file client where `Client` extends `pystac.Catalog`; a forked signed-vs-unsigned client where `modifier` is one boundary row
