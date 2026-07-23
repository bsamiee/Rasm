# [PY_DATA_API_PYSTAC]

`pystac` supplies the in-memory STAC object model for the data STAC-catalog rail: the `Catalog`/`Collection`/`Item`/`Asset` hierarchy over a `Link` graph, `Extent`/`Provider` metadata, the `MediaType`/`CatalogType`/`RelType`/`ProviderRole` vocabularies, pluggable `StacIO` read/write, schema migration on read, JSON-Schema validation, and the typed extension family reached through `obj.ext.<short>`. `pystac` owns STAC JSON serialization, the migration path, and the HREF-resolution link graph; the data owner composes the `STACObject` hierarchy and the module readers as its STAC in-memory owner.

## [01]-[PACKAGE_SURFACE]

- package: `pystac`
- license: Apache-2.0
- module: `import pystac`
- owner: `data`
- rail: STAC catalog
- entry points: none (library only)
- capability: the STAC in-memory object graph — catalog/collection/item/asset construction, `Link` traversal and HREF normalization, spatial/temporal extent and provider metadata, the STAC vocabularies, pluggable JSON read/write via `StacIO`, schema migration on read, JSON-Schema validation, and the full typed STAC extension namespace

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: STAC object hierarchy, metadata, and vocabularies

`Catalog`/`Collection`/`Item`/`ItemCollection` share the `STACObject` base; `Collection` extends `Catalog` with `Extent`/`Provider`/`Summaries`, and `Item` mixes in `Assets` and exposes `common_metadata` -> `CommonMetadata`. `Link` is a first-class graph edge built through typed factory constructors. Vocabularies are `str`-subclass `StringEnum`s, so an enum member is itself a valid wire string.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]    | [CAPABILITY]                                                                        |
| :-----: | :--------------------------- | :--------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `STACObject`                 | abstract root    | `from_file`/`to_dict`/`get_links`/`set_root`/`save_object`/`validate` base          |
|  [02]   | `Catalog`                    | catalog root     | child/item link graph, HREF normalization, recursive save, functional `map_*`       |
|  [03]   | `Collection`                 | catalog + extent | a `Catalog` with `Extent`/`Provider`/`Summaries`/`ItemAssetDefinition`/`license`    |
|  [04]   | `Item`                       | feature          | `STACObject` + `Assets`; geometry, bbox, datetime, properties, `common_metadata`    |
|  [05]   | `Asset`                      | asset            | asset payload; `move`/`copy`/`has_role` (ctor in scope)                             |
|  [06]   | `ItemCollection`             | item set         | in-memory GeoJSON `FeatureCollection` of `Item`s; `is_item_collection`, `from_file` |
|  [07]   | `Link`                       | graph edge       | typed relation edge built through static factory constructors (`[03]`)              |
|  [08]   | `Extent`                     | extent           | bbox + temporal interval (`SpatialExtent`/`TemporalExtent`); `Extent.from_items`    |
|  [09]   | `Provider`                   | provider         | data provider with `name`, `roles: list[ProviderRole]`, `url`, `description`        |
|  [10]   | `Summaries` / `RangeSummary` | summary          | collection field summaries; `RangeSummary(minimum, maximum)` numeric ranges         |
|  [11]   | `CommonMetadata`             | metadata view    | `item.common_metadata` view over EO/datetime/instrument/licensing fields            |
|  [12]   | `ItemAssetDefinition`        | asset template   | collection-level `item_assets` band/role template applied to member items           |
|  [13]   | `StacIO`                     | IO strategy      | pluggable read/write; `StacIO.set_default`, `DefaultStacIO`, cloud/VSI override     |
|  [14]   | `MediaType`                  | media enum       | asset media-type vocabulary                                                         |
|  [15]   | `CatalogType`                | catalog enum     | `SELF_CONTAINED`/`ABSOLUTE_PUBLISHED`/`RELATIVE_PUBLISHED`                          |
|  [16]   | `RelType`                    | relation enum    | link-relation vocabulary                                                            |
|  [17]   | `ProviderRole`               | role enum        | `LICENSOR`/`PRODUCER`/`PROCESSOR`/`HOST`                                            |
|  [18]   | `STACObjectType`             | kind enum        | `CATALOG`/`COLLECTION`/`ITEM` discriminant resolved on `read_dict`                  |

- [MEDIA_TYPE]: `COG` `GEOTIFF` `TIFF` `HDF` `HDF5` `JSON` `GEOJSON` `PNG` `JPEG` `JPEG2000` `NETCDF` `VND_APACHE_PARQUET` `VND_PMTILES` `VND_ZARR` `COPC` `FLATGEOBUF` `GEOPACKAGE` `KML` `PDF` `XML` `HTML` `TEXT`
- [REL_TYPE]: `SELF` `ROOT` `PARENT` `CHILD` `ITEM` `ITEMS` `COLLECTION` `CANONICAL` `DERIVED_FROM` `ALTERNATE` `VIA` `PREVIEW` `LICENSE` `NEXT` `PREV`

[PUBLIC_TYPE_SCOPE]: typed error rail

`STACError` is the base; the leaves are distinct so the data owner discriminates malformed JSON, missing required fields, and extension misuse without string matching.

| [INDEX] | [SYMBOL]                      | [CAPABILITY]                                                        |
| :-----: | :---------------------------- | :------------------------------------------------------------------ |
|  [01]   | `STACError`                   | base STAC failure                                                   |
|  [02]   | `STACTypeError`               | object is not the expected STAC type                                |
|  [03]   | `STACValidationError`         | JSON-Schema validation failed (raised by `validate`/`validate_all`) |
|  [04]   | `ExtensionAlreadyExistsError` | extension already applied to the object                             |
|  [05]   | `ExtensionNotImplemented`     | extension not implemented for the object                            |
|  [06]   | `ExtensionTypeError`          | extension applied to the wrong object type                          |
|  [07]   | `RequiredPropertyMissing`     | a required extension/object property is absent                      |
|  [08]   | `DuplicateObjectKeyError`     | duplicate key during JSON decode                                    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: read, construct, traverse, save

`read_file`/`read_dict` resolve `STACObjectType` and return the concrete subtype; `from_file` is the `STACObject` classmethod every kind inherits. Traversal is a deliberate polymorphic family on the link graph — `get_item(id)` single lookup, `get_items(*ids)` variadic, `get_all_items()` the flattened iterator, the same shape over children and collections. `normalize_and_save` is the one-call egress: HREF rewrite and recursive write.

| [INDEX] | [SURFACE]                             | [CALL_SHAPE]                                                                                   |
| :-----: | :------------------------------------ | :--------------------------------------------------------------------------------------------- |
|  [01]   | `read_file`                           | `read_file(href, stac_io=None) -> STACObject`                                                  |
|  [02]   | `read_dict`                           | `read_dict(d, href=None, root=None, stac_io=None) -> STACObject`                               |
|  [03]   | `write_file`                          | `write_file(obj, include_self_link=True, dest_href=None, stac_io=None) -> None`                |
|  [04]   | `Item.from_dict`                      | `from_dict(d, href=None, root=None, migrate=True, preserve_dict=True)`                         |
|  [05]   | `Item.to_dict`                        | `to_dict(include_self_link=True, transform_hrefs=True) -> dict`                                |
|  [06]   | `Item.set_datetime`                   | `set_datetime(datetime, asset=None)`                                                           |
|  [07]   | `Catalog.from_file`                   | `from_file(href, stac_io=None)`                                                                |
|  [08]   | `Catalog.add_item`                    | `add_item(item, title=None, strategy=None, set_parent=True) -> Link`                           |
|  [09]   | `Catalog.add_items`                   | `add_items(items, strategy=None)`                                                              |
|  [10]   | `Catalog.add_child` / `add_children`  | `add_child(child, title=None, strategy=None, set_parent=True) -> Link`                         |
|  [11]   | `Catalog.get_item`                    | `get_item(id, recursive=False) -> Item \| None`                                                |
|  [12]   | `Catalog.get_items`                   | `get_items(*ids, recursive=False) -> Iterator[Item]`                                           |
|  [13]   | `Catalog.get_all_items`               | `get_all_items() -> Iterator[Item]`                                                            |
|  [14]   | `Catalog.get_child` / `get_children`  | `get_child(id, recursive=False, sort_links_by_id=True)`; `get_children() -> Iterator[Catalog]` |
|  [15]   | `Catalog.get_collections`             | `get_collections() -> Iterator[Collection]`                                                    |
|  [16]   | `Catalog.get_all_collections`         | `get_all_collections() -> Iterator[Collection]`                                                |
|  [17]   | `Catalog.walk`                        | `walk() -> Iterable[(Catalog, children, items)]`                                               |
|  [18]   | `Catalog.map_items` / `map_assets`    | `map_items(item_mapper)`; `map_assets(asset_mapper) -> Catalog`                                |
|  [19]   | `Catalog.normalize_hrefs`             | `normalize_hrefs(root_href, strategy=None, skip_unresolved=False)`                             |
|  [20]   | `Catalog.normalize_and_save`          | `(root_href, catalog_type=None, strategy=None, stac_io=None, skip_unresolved=False)`           |
|  [21]   | `Catalog.save`                        | `save(catalog_type=None, dest_href=None, stac_io=None)`                                        |
|  [22]   | `Catalog.make_all_asset_hrefs_*`      | `make_all_asset_hrefs_absolute()` / `make_all_asset_hrefs_relative()`                          |
|  [23]   | `Catalog.validate_all`                | `validate_all(max_items=None, recursive=True) -> int`                                          |
|  [24]   | `Catalog.generate_subcatalogs`        | `generate_subcatalogs(template, defaults=None, parent_ids=None) -> list[Catalog]`              |
|  [25]   | `Collection.from_items`               | `from_items(items, *, id=None, strategy=None) -> Collection`                                   |
|  [26]   | `Collection.update_extent_from_items` | `update_extent_from_items() -> None`                                                           |
|  [27]   | `Item.add_asset` / `get_assets`       | `add_asset(key, asset)`; `get_assets(media_type=None, role=None) -> dict[str, Asset]`          |
|  [28]   | `Link` factories                      | `Link.{child,item,parent,root,self_href,canonical,collection,derived_from} -> Link`            |

[ENTRYPOINT_SCOPE]: typed extensions via the `obj.ext` accessor (`pystac.extensions.*`)

`obj.ext.<short>` is the apply-and-access surface on every object — `item.ext.proj`, `asset.ext.raster`, `collection.ext.item_assets`. `obj.ext.add(name)` appends the schema URI to `stac_extensions`; `obj.ext.has(name)`/`remove(name)` manage membership. Each per-object accessor (`ItemExt`/`AssetExt`/`CollectionExt`) statically scopes which extensions reach which kind, so the object type is the discriminant and there is no per-extension entrypoint. `[APPLIES_TO]` codes: `I`=Item, `A`=Asset, `C`=Collection, `D`=ItemAssetDefinition.

| [INDEX] | [ACCESSOR_EXTENSION]                       | [APPLIES_TO] | [CAPABILITY]                                                        |
| :-----: | :----------------------------------------- | :----------- | :------------------------------------------------------------------ |
|  [01]   | `item.ext.proj` / `ProjectionExtension`    | I·A·D        | `epsg/wkt2/projjson/code/geometry/bbox/centroid/shape/transform`    |
|  [02]   | `item.ext.eo` / `EOExtension`              | I·A          | `bands: list[Band]`, `cloud_cover`, `snow_cover`                    |
|  [03]   | `asset.ext.raster` / `RasterExtension`     | A            | `bands: list[RasterBand]` per-band statistics, nodata, scale/offset |
|  [04]   | `item.ext.classification`                  | I·A          | classification class lists and bitfields                            |
|  [05]   | `item.ext.cube` / `DatacubeExtension`      | I·A·C        | datacube `dimensions`/`variables` description                       |
|  [06]   | `asset.ext.file` / `FileExtension`         | A            | `size`/`checksum`/`header_size`/`values` byte-level asset metadata  |
|  [07]   | `item.ext.grid` / `item.ext.mgrs`          | I            | grid-code and MGRS tiling metadata                                  |
|  [08]   | `item.ext.label` / `LabelExtension`        | I            | ML label `properties`/`classes`/`tasks`/`overviews`                 |
|  [09]   | `item.ext.mlm`                             | I·A·C        | machine-learning-model card (inputs/outputs/hyperparameters)        |
|  [10]   | `asset.ext.pc` / `PointcloudExtension`     | I·A          | point-count, type, density, schema for LiDAR/COPC assets            |
|  [11]   | `item.ext.sar` / `item.ext.sat`            | I·A          | SAR polarization/frequency and satellite orbit metadata             |
|  [12]   | `item.ext.sci` / `ScientificExtension`     | I·C          | `doi`/`citation`/`publications` provenance                          |
|  [13]   | `asset.ext.storage` / `item.ext.render`    | I·A·C        | cloud-storage tier/region and visualization render parameters       |
|  [14]   | `collection.ext.table` / `item.ext.xarray` | I·A·C        | tabular `columns` schema and xarray-assets zarr/kerchunk references |
|  [15]   | `item.ext.timestamps` / `item.ext.version` | I·C          | published/expires/unpublished timestamps and version/deprecation    |
|  [16]   | `collection.ext.item_assets`               | C            | collection-level `item_assets` -> `ItemAssetDefinition` template    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- hierarchy: one `STACObject` root fans to `Catalog`/`Collection`/`Item`/`ItemCollection`; `Collection` is a `Catalog` with `Extent`/`Provider`/`Summaries`, and `Item` carries assets via the `Assets` mixin and common fields via `common_metadata`.
- read: `read_file`/`read_dict` resolve `STACObjectType` and return the concrete subtype with schema migration applied on ingest.
- traversal: the `get_item`/`get_items(*ids)`/`get_all_items`/`get_child`/`get_children`/`get_collections`/`walk`/`map_items` family owns single-id, variadic, recursive-flatten, and functional-map over the link graph as distinct verbs.
- href: `Link` factories build relative/absolute edges; `normalize_hrefs(root)` then `save`, or the fused `normalize_and_save(root)`, rewrites the tree.
- vocabulary: `MediaType`/`CatalogType`/`RelType`/`ProviderRole` are `str`-subclass enums, so an enum member is a wire string; media types, catalog self-containment, link relations, and provider roles are enum rows.
- extension: `obj.ext.<short>` is the single apply-and-access surface; `obj.ext.add`/`has`/`remove` manage the `stac_extensions` URI list, and the object type scopes which extensions are reachable.
- io: `StacIO.set_default` or a `stac_io=` argument swaps the read/write strategy — cloud, VSI, signed-URL — under every read/write/save.
- validation: `validate`/`validate_all` run JSON-Schema validation and raise `STACValidationError` as a typed gate.
- evidence: each catalog operation captures object type, id, self-href, link/asset counts, applied extension schema URIs, and validation outcome as a STAC receipt.

[STACKING]:
- data owner: composes the `STACObject` hierarchy and the module read/write functions into one STAC in-memory owner, injecting a single `StacIO` for cloud/VSI/signed-URL access.
- `pystac-client` -> `pystac`: `Client.search(...).items()` yields live `pystac.Item` objects in this model, consumed through the same `Item`/`Asset` surface.
- `pystac` -> `stac-geoparquet`: `arrow.parse_stac_items_to_arrow(items)`/`stac_table_to_items` round-trip `Item` objects to and from an Arrow item table, with `Item.to_dict()` GeoJSON as the bridge shape.
- `pystac` -> `odc-stac`/`stackstac`: `odc.stac.load(items, bands=...)`/`stackstac.stack(items)` assemble a georeferenced xarray/dask cube, each item's projection/raster extensions supplying the CRS/transform/nodata.
- `pystac` -> `planetary-computer`: `planetary_computer.sign(item)` returns the same `Item` with signed asset HREFs, which flow to `rioxarray.open_rasterio`/`rasterio.open` for decode.
- `Asset.media_type == MediaType.COG` is the routing discriminant selecting the rasterio/rioxarray raster path versus the `pyogrio`/`geopandas` vector path versus the `laspy`/COPC point path.

[LOCAL_ADMISSION]:
- `pystac.Item`/`Collection` and the `read_file`/`read_dict` module readers are the sole STAC in-memory surface; a `pystac-client`-yielded `Item` enters unchanged.

[RAIL_LAW]:
- Package: `pystac`
- Owns: the in-memory STAC object graph, spatial/temporal extent and provider metadata, the STAC vocabularies, pluggable JSON read/write via `StacIO`, schema migration on read, JSON-Schema validation, and the typed STAC extension namespace
- Accept: `read_file`/`read_dict` polymorphic ingest, `STACObject` hierarchy construction, the `get_item`/`get_items`/`get_all_items`/`walk`/`map_items` traversal family, `Link.*` factory edges, `normalize_and_save` egress, the `MediaType`/`CatalogType`/`RelType`/`ProviderRole` enums, `obj.ext.<short>` extension application, `StacIO` injection, `validate_all` gating
- Reject: wrapper-renames of `Item`/`from_dict`/`to_dict`; a per-kind parallel record type; a second item type where `pystac-client` yields `Item`; hand-built HREFs where the `Link` factories and `normalize_hrefs` apply; raw rel/media strings where `RelType`/`MediaType` resolve; a hand-rolled `add_extension` or manual `stac_extensions` mutation where `obj.ext` owns application; a forked reader where `StacIO` injection swaps strategy
