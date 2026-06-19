# [PY_DATA_API_PYSTAC]

`pystac` supplies the in-memory STAC object model for the data STAC-catalog rail: the `Catalog`/`Collection`/`Item`/`Asset` hierarchy with `Link` graph edges, `Extent`/`Provider` metadata, the `MediaType` and `CatalogType` vocabularies, and the typed extension family (`eo`/`projection`/`raster`) applied through the shared `Ext.ext(obj)` accessor. The package owner composes `Item`, `Collection`, and the read/write module functions into the STAC owner; it never re-implements the STAC JSON serialization or HREF-resolution graph pystac already owns.

## [01]-[PACKAGE_SURFACE]

- package: `pystac`
- import: `import pystac`
- owner: `data`
- rail: STAC catalog
- asset: pure-Python runtime library (`py3-none-any` wheel)
- installed: present in the lockfile but not yet synced into the active venv — RESEARCH-capture-pending-on-uv-sync; the member surface below is authored from the canonical source (`stac-utils/pystac` `v1.14.3`) and official documentation, and reflection-verifies on uv sync (pure-Python, imports on the cp315 core)
- entry points: none (library only)
- capability: the STAC in-memory object graph — catalog/collection/item/asset construction, `Link` graph traversal and HREF normalization, spatial/temporal extent and provider metadata, the `MediaType`/`CatalogType` vocabularies, JSON read/write, and typed STAC extensions (EO, projection, raster, and the wider `pystac.extensions` namespace)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: STAC object hierarchy and metadata
- rail: STAC catalog

`Catalog` and `Item` share the `STACObject` base (so both carry `from_file`/`get_self_href`/`set_root`); `Collection` extends `Catalog` with `Extent`/`Provider`. `Item` mixes in `Assets`, which owns `add_asset`/`get_assets`.

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY]    | [CAPABILITY]                                                          |
| :-----: | :-------------------------------------------- | :--------------- | :-------------------------------------------------------------------- |
|  [01]   | `Catalog`                                     | catalog root     | child/item graph, HREF normalization, recursive save                  |
|  [02]   | `Collection`                                  | catalog + extent | a `Catalog` with `Extent`, `Provider`, `summaries`, and licensing     |
|  [03]   | `Item`                                        | feature          | `STACObject` + `Assets`; geometry, bbox, datetime, properties, assets |
|  [04]   | `Asset`                                       | asset            | `Asset(href, title=None, media_type=None, roles=None, ...)`           |
|  [05]   | `ItemCollection`                              | item set         | an in-memory GeoJSON `FeatureCollection` of `Item` objects            |
|  [06]   | `Link`                                        | graph edge       | typed relation between STAC objects (`rel`, `target`, `media_type`)   |
|  [07]   | `Extent` / `SpatialExtent` / `TemporalExtent` | extent           | collection spatial bbox and temporal interval                         |
|  [08]   | `Provider`                                    | provider         | data provider with `name`, `roles`, `url`                             |
|  [09]   | `MediaType`                                   | media enum       | `COG`/`GEOTIFF`/`JSON`/`GEOJSON`/`PNG`/`JPEG`/`VND_APACHE_PARQUET`/…  |
|  [10]   | `CatalogType`                                 | catalog enum     | `SELF_CONTAINED`/`ABSOLUTE_PUBLISHED`/`RELATIVE_PUBLISHED`            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: read, construct, traverse, save
- rail: STAC catalog

`from_file` is the `STACObject` classmethod inherited by `Catalog`/`Collection`/`Item`. `get_items` is variadic over ids (`get_items(*ids, recursive=False)`), not a list-keyed query. `normalize_hrefs` then `save` is the catalog egress pair.

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                                                             | [CAPABILITY]                          |
| :-----: | :------------------------ | :----------------------------------------------------------------------- | :------------------------------------ |
|  [01]   | `read_file`               | `read_file(href, stac_io=None)` -> `STACObject`                          | read any STAC object from an HREF     |
|  [02]   | `write_file`              | `write_file(obj, include_self_link=True, dest_href=None, stac_io=None)`  | write a STAC object to JSON           |
|  [03]   | `Item.from_dict`          | `from_dict(d, href=None, root=None, migrate=True, preserve_dict=True)`   | hydrate an `Item` from a GeoJSON dict |
|  [04]   | `Item.to_dict`            | `to_dict(include_self_link=True, transform_hrefs=True)` -> `dict`        | serialize an `Item` to a GeoJSON dict |
|  [05]   | `Catalog.from_file`       | `from_file(href, stac_io=None)`                                          | read a catalog/collection root        |
|  [06]   | `Catalog.add_item`        | `add_item(item, title=None, strategy=None, set_parent=True)` -> `Link`   | attach an item under the catalog      |
|  [07]   | `Catalog.add_child`       | `add_child(child, title=None, strategy=None, set_parent=True)` -> `Link` | attach a child catalog/collection     |
|  [08]   | `Catalog.get_items`       | `get_items(*ids, recursive=False)` -> `Iterator[Item]`                   | iterate items (variadic id filter)    |
|  [09]   | `Catalog.walk`            | `walk()` -> `Iterable[(Catalog, children, items)]`                       | `os.walk`-style recursive descent     |
|  [10]   | `Catalog.normalize_hrefs` | `normalize_hrefs(root_href, strategy=None, skip_unresolved=False)`       | rewrite all HREFs under a root        |
|  [11]   | `Catalog.save`            | `save(catalog_type=None, dest_href=None, stac_io=None)`                  | recursively write the catalog tree    |
|  [12]   | `Item.add_asset`          | `add_asset(key, asset)` (from the `Assets` mixin)                        | attach an `Asset` under a key         |

[ENTRYPOINT_SCOPE]: typed extensions (`pystac.extensions.*`)
- rail: STAC catalog

Every extension applies through the shared `Ext.ext(obj, add_if_missing=False)` classmethod over an `Item`/`Asset`; there is no `add_extension` method — the schema URI is appended to `stac_extensions` only when `add_if_missing=True`.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                          | [CAPABILITY]                           |
| :-----: | :-------------------- | :---------------------------------------------------- | :------------------------------------- |
|  [01]   | `ProjectionExtension` | `ProjectionExtension.ext(item, add_if_missing=False)` | EPSG/WKT/transform projection metadata |
|  [02]   | `EOExtension`         | `EOExtension.ext(item, add_if_missing=False)`         | electro-optical bands and cloud cover  |
|  [03]   | `RasterExtension`     | `RasterExtension.ext(asset, add_if_missing=False)`    | per-band raster statistics and nodata  |

## [04]-[IMPLEMENTATION_LAW]

[STAC_MODEL]:
- import: `import pystac` at boundary scope only; module-level import is banned by the manifest import policy.
- hierarchy axis: one `Catalog`/`Collection`/`Item`/`Asset` graph owns the model; `Collection` is a `Catalog` with `Extent`/`Provider`, and `Item` is a `STACObject` carrying assets via the `Assets` mixin — never a parallel record type per STAC kind.
- query axis: `get_items(*ids, recursive=...)` is the one variadic traversal over the link graph; `walk` is the `os.walk`-style descent — never a `get`/`get_many`/`list` family, and never a manual link-following loop.
- href axis: `Link` edges carry relative/absolute HREFs; `normalize_hrefs(root)` rewrites the whole tree before `save` — never hand-built relative paths.
- vocabulary axis: `MediaType` and `CatalogType` are the canonical string enums; asset media types and catalog self-containment are enum rows, never raw strings.
- extension axis: `Ext.ext(obj, add_if_missing=True)` is the single apply-and-access surface for the EO/projection/raster (and wider) extensions; the `stac_extensions` URI list is managed by the extension, never appended by hand.
- evidence: each catalog operation captures object type, id, self-href, link/asset counts, and applied extension schema URIs as a STAC receipt.
- boundary: pystac owns the in-memory STAC graph and JSON serialization; `pystac-client` owns the live STAC API search above it; `stac-geoparquet` owns the item-table interchange beside it; the asset HREFs route to the cloud-egress and tensor-cube owners; live UI stays outside this package.

[RAIL_LAW]:
- Package: `pystac`
- Owns: the in-memory STAC object graph (catalog/collection/item/asset/link), spatial/temporal extent and provider metadata, the `MediaType`/`CatalogType` vocabularies, JSON read/write, and typed STAC extensions
- Accept: `Catalog`/`Collection`/`Item`/`Asset` construction, variadic `get_items`/`walk` traversal, `normalize_hrefs`+`save` egress, `MediaType`/`CatalogType` enums, `Ext.ext(obj, add_if_missing=...)` extension application
- Reject: wrapper-renames of `Item`/`from_dict`/`to_dict`; a parallel record type per STAC kind; a `get`/`list`/`search` operation family where variadic `get_items`/`walk` discriminates; hand-built HREFs where `normalize_hrefs` applies; raw media-type strings where `MediaType` resolves; a hand-rolled `add_extension` where `Ext.ext` owns extension application
