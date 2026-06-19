# [PY_DATA_API_PYSTAC_CLIENT]

`pystac-client` (dist `pystac-client`, import `pystac_client`) supplies the live STAC API client for the data STAC-catalog rail: a `Client` (a `pystac.Catalog` bound to a STAC API root) whose one polymorphic `search(...)` returns an `ItemSearch` that lazily pages the `/search` endpoint into `pystac.Item` objects, plus `collection_search`/`get_collections` for collection discovery. The package owner composes `Client.open`, `search`, and the `ItemSearch` paging surface into the STAC discovery owner; it never re-implements the STAC API paging, CQL2 filtering, or conformance negotiation pystac-client already owns.

## [01]-[PACKAGE_SURFACE]

- package: `pystac-client`
- import: `import pystac_client` (dist name `pystac-client`, import name `pystac_client`)
- owner: `data`
- rail: STAC catalog
- asset: pure-Python runtime library (`py3-none-any` wheel)
- installed: present in the lockfile but not yet synced into the active venv — RESEARCH-capture-pending-on-uv-sync; the member surface below is authored from the canonical source (`stac-utils/pystac-client` `0.9.0`) and official documentation, and reflection-verifies on uv sync (pure-Python, imports on the cp315 core)
- entry points: console script `stac-client` (CLI); library use is import-only
- capability: STAC API client over a catalog root — conformance-negotiated item search with bbox/datetime/intersects/CQL2 filter/sortby/fields/ids parameters, lazy result paging into `pystac.Item`, total-match counts, collection discovery and free-text collection search, and request modifiers for auth/signing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client, search, and collection roots
- rail: STAC catalog

`Client` extends `pystac.Catalog`; `CollectionClient` extends `pystac.Collection`. `ItemSearch`/`CollectionSearch` are lazy result iterators that page the API on demand.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]     | [CAPABILITY]                                                            |
| :-----: | :----------------- | :---------------- | :---------------------------------------------------------------------- |
|  [01]   | `Client`           | API catalog root  | a `pystac.Catalog` bound to a STAC API; owns `search`/collection access |
|  [02]   | `ItemSearch`       | item result page  | lazy paging of `/search` into `pystac.Item`; match counts               |
|  [03]   | `CollectionClient` | live collection   | a `pystac.Collection` with live `get_items`/`get_item`                  |
|  [04]   | `CollectionSearch` | collection result | lazy paging of `/collections` with free-text and filter parameters      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client open and search
- rail: STAC catalog

`search` is keyword-only; `max_items` caps the total result count across pages while `limit` is the per-page request size. The same `search` call discriminates by which parameters are supplied — bbox vs intersects vs CQL2 `filter` are rows, not parallel methods.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                                                                                                                                       | [CAPABILITY]                  |
| :-----: | :------------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------- |
|  [01]   | `Client.open`              | `open(url, headers=None, parameters=None, modifier=None, request_modifier=None, stac_io=None, timeout=None)` -> `Client`                                                                                           | open a STAC API root          |
|  [02]   | `Client.search`            | `search(*, method='POST', max_items=None, limit=None, ids=None, collections=None, bbox=None, intersects=None, datetime=None, query=None, filter=None, filter_lang=None, sortby=None, fields=None)` -> `ItemSearch` | item search                   |
|  [03]   | `Client.get_collections`   | `get_collections()` -> `Iterator[Collection]`                                                                                                                                                                      | iterate the API's collections |
|  [04]   | `Client.get_collection`    | `get_collection(collection_id)` -> `Collection \| CollectionClient \| None`                                                                                                                                        | fetch one collection (cached) |
|  [05]   | `Client.collection_search` | `collection_search(*, max_collections=None, limit=None, bbox=None, datetime=None, q=None, query=None, filter=None, filter_lang=None, sortby=None, fields=None)` -> `CollectionSearch`                              | free-text collection search   |

[ENTRYPOINT_SCOPE]: `ItemSearch` result paging
- rail: STAC catalog

`item_collection()` (and `item_collection_as_dict()`) is the canonical materialization; `items()`/`items_as_dicts()` stream lazily; `pages()` yields whole `ItemCollection` pages. `get_all_items` is deprecated (emits `FutureWarning`) — use `item_collection()`.

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]                            | [CAPABILITY]                            |
| :-----: | :----------------------------------- | :-------------------------------------- | :-------------------------------------- |
|  [01]   | `ItemSearch.item_collection`         | `item_collection()` -> `ItemCollection` | materialize all results (canonical)     |
|  [02]   | `ItemSearch.items`                   | `items()` -> `Iterator[Item]`           | lazily stream items across pages        |
|  [03]   | `ItemSearch.items_as_dicts`          | `items_as_dicts()` -> `Iterator[dict]`  | lazily stream raw item dicts            |
|  [04]   | `ItemSearch.item_collection_as_dict` | `item_collection_as_dict()` -> `dict`   | materialize as a GeoJSON dict           |
|  [05]   | `ItemSearch.pages`                   | `pages()` -> `Iterator[ItemCollection]` | iterate whole result pages              |
|  [06]   | `ItemSearch.matched`                 | `matched()` -> `int \| None`            | total matching item count (if reported) |

## [04]-[IMPLEMENTATION_LAW]

[STAC_CLIENT]:
- import: `import pystac_client` at boundary scope only; module-level import is banned by the manifest import policy. The dist name is `pystac-client`; the import name is `pystac_client`.
- entry axis: `Client.open(url)` is the single bound-catalog entry; it is a `pystac.Catalog`, so the in-memory traversal surface is inherited — never a parallel API-vs-file client type.
- search axis: one keyword-only `search(...)` owns item discovery; spatial selection is `bbox` vs `intersects`, predicate selection is `query` vs CQL2 `filter`/`filter_lang`, and ordering/projection are `sortby`/`fields` — all parameter rows on the one method, never a `search_by_bbox`/`search_by_geometry`/`search_by_filter` family.
- paging axis: `ItemSearch` is lazy; `max_items` caps the total and `limit` sizes each page request — `item_collection()` materializes, `items()`/`pages()` stream; never a hand-rolled `next`-link follow loop, and never the deprecated `get_all_items`.
- collection axis: `get_collections`/`get_collection` discover collections and `collection_search` runs free-text (`q`) and filter collection queries; `CollectionClient.get_items` is the per-collection live item access.
- modifier axis: `modifier`/`request_modifier` on `open` inject auth headers and href signing (e.g. SAS tokens) at the boundary — a request row, never a forked signed-vs-unsigned client.
- evidence: each search captures the conformance classes negotiated, the result `matched()` count, page count, and parameter set as a discovery receipt.
- boundary: pystac-client owns live STAC API search and paging; `pystac` owns the in-memory model the results hydrate into; `stac-geoparquet` owns the item-table interchange beside it; the asset HREFs fold into the cloud-egress and tensor-cube owners; live UI stays outside this package.

[RAIL_LAW]:
- Package: `pystac-client`
- Owns: the STAC API client (`Client` over a catalog root), conformance-negotiated item search with bbox/datetime/intersects/CQL2/sortby/fields parameters, lazy result paging into `pystac.Item`, match counts, and collection discovery/free-text search
- Accept: `Client.open` with request modifiers, one keyword-only `search` discriminating by parameter, `ItemSearch.item_collection()`/`items()`/`pages()` paging, `collection_search`/`get_collections` for collection discovery
- Reject: wrapper-renames of `open`/`search`; a `search_by_<axis>` method family where the one `search` discriminates by parameter; a hand-rolled `next`-link paging loop where `ItemSearch` pages; the deprecated `get_all_items` where `item_collection()` is canonical; a parallel API-vs-file client where `Client` extends `pystac.Catalog`
