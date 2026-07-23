# [PY_DATA_API_ODC_STAC]

`odc-stac` folds a sequence of STAC `Item` objects into one lazy multi-band `xarray.Dataset` cube backed by a Dask graph, resolving the output `GeoBox` — CRS, resolution, anchor, extent — from the items or explicit load parameters. `load` (alias `stac_load`) is the single `CATALOG_COVERAGE_ODCSTAC` rail the `data` owner composes: items in, lazy cube out, `groupby`/`bands`/`chunks`/`resampling`/geobox controls as call rows. GeoBox math defers to `odc.geo` and pixel reads to `odc.loader`; STAC search stays upstream in the catalog client.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `odc-stac`
- package: `odc-stac` (Apache-2.0)
- import: `odc.stac`
- owner: `data`
- rail: catalog-coverage
- entry points: none (import-only)
- capability: STAC `Item` to `xarray.Dataset` conversion, lazy Dask cube construction, output GeoBox resolution (CRS/resolution/anchor/bbox/geopolygon), per-band metadata extraction and alias mapping, temporal and key grouping, resampling and dtype control, and GDAL/rasterio and S3 session configuration for cloud reads

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: parsed item and raster metadata records

`load` consumes raw `pystac.Item` objects; the records below are the internal representation `parse_item`/`parse_items` produce and `output_geobox` consumes. `RasterBandMetadata`, `RasterSource`, and `RasterLoadParams` re-export from `odc.loader`; `ConversionConfig` aliases `Dict[str, Any]`, passed as `stac_cfg`.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]    | [CAPABILITY]                                                   |
| :-----: | :------------------------- | :--------------- | :------------------------------------------------------------- |
|  [01]   | `ParsedItem`               | dataclass record | one STAC item reduced to loadable raster bands, geometry, time |
|  [02]   | `RasterCollectionMetadata` | dataclass record | collection-level band structure, proj flag, band-to-grid map   |
|  [03]   | `RasterBandMetadata`       | dataclass record | per-band data type, nodata, units, dims                        |
|  [04]   | `RasterSource`             | dataclass record | single band source: uri, band index, subdataset, geobox        |
|  [05]   | `RasterLoadParams`         | dataclass record | per-band load config: dtype, fill, resampling, overviews       |
|  [06]   | `ConversionConfig`         | type alias       | `Dict[str, Any]` user collection-config passed as `stac_cfg`   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cube load

`load` (alias `stac_load`) accepts STAC items, resolves the output GeoBox from the extent keyword set, and returns a lazy (`chunks`) or eager (`pool`) `xarray.Dataset`.
- call: `load(items, bands=None, *, groupby="time", resampling=None, dtype=None, chunks=None, pool=None, crs=Unset, resolution=None, anchor=None, geobox=None, bbox=None, lon=None, lat=None, x=None, y=None, like=None, geopolygon=None, intersects=None, progress=None, fail_on_error=True, stac_cfg=None, with_properties=None, patch_url=None, preserve_original_order=False, driver=None, fuse_func=None, **kw) -> xr.Dataset` — `stac_load` is the identical alias

| [INDEX] | [SURFACE]            | [CAPABILITY]                                   |
| :-----: | :------------------- | :--------------------------------------------- |
|  [01]   | `load` / `stac_load` | STAC items to lazy/eager `xarray.Dataset` cube |

[ENTRYPOINT_SCOPE]: metadata parse and geobox

These surfaces expose the internal load pipeline for debugging: item parsing to `ParsedItem`, collection band metadata from a sample item, and standalone GeoBox resolution from parsed items and load parameters.
- call: `parse_item(item, template=None, md_plugin=None, asset_absolute_paths=True) -> ParsedItem`; `parse_items(items, cfg=None, md_plugin=None, asset_absolute_paths=True) -> Iterator[ParsedItem]`; `extract_collection_metadata(item, cfg=None, md_plugin=None) -> RasterCollectionMetadata`
- call: `output_geobox(items, bands=None, *, crs=Unset, resolution=None, anchor=None, align=None, geobox=None, like=None, geopolygon=None, bbox=None, lon=None, lat=None, x=None, y=None, intersects=None) -> GeoBox`

| [INDEX] | [SURFACE]                     | [CAPABILITY]                                       |
| :-----: | :---------------------------- | :------------------------------------------------- |
|  [01]   | `parse_item`                  | one STAC item to internal `ParsedItem`             |
|  [02]   | `parse_items`                 | item sequence to `ParsedItem` iterator             |
|  [03]   | `extract_collection_metadata` | sample item to collection band metadata            |
|  [04]   | `output_geobox`               | resolve target GeoBox from parsed items and params |

[ENTRYPOINT_SCOPE]: GDAL and S3 session

`configure_rio` and `configure_s3_access` set the rasterio/GDAL environment and S3 credentials locally or propagate them to a Dask cluster over the worker-plugin mechanism, before lazy materialization triggers cloud reads.
- call: `configure_rio(*, cloud_defaults=False, verbose=False, aws=None, **params) -> None`; `configure_s3_access(profile=None, region_name="auto", aws_unsigned=None, requester_pays=False, cloud_defaults=True, **gdal_opts)`

| [INDEX] | [SURFACE]             | [CAPABILITY]                                     |
| :-----: | :-------------------- | :----------------------------------------------- |
|  [01]   | `configure_rio`       | set GDAL/rasterio config locally or on a cluster |
|  [02]   | `configure_s3_access` | credentialize or set public S3 access for reads  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- load axis: `load` owns items-to-cube conversion; `bands`/`groupby`/`resampling`/`dtype`/`chunks`/`pool` are call rows on that one surface, never parallel loader types or a per-collection builder; `chunks` selects the Dask-lazy row and its absence materializes eagerly through `pool`; `stac_load` is the identical callable, not a second entry point.
- geobox axis: the output GeoBox resolves from exactly one extent row — `geobox`, `like`, `geopolygon`, `bbox`, `lon`/`lat`, `x`/`y`, `intersects` — combined with `crs`/`resolution`/`anchor`; `output_geobox` exposes that resolution for debugging, but the canonical path is the `load` keyword set, never a pre-computed GeoBox bypassing `load`.
- metadata axis: `parse_items` and `extract_collection_metadata` produce `ParsedItem`/`RasterCollectionMetadata` for inspection and custom config; the live path passes raw `pystac.Item` objects to `load` and parses internally, never staging the internal representation by hand.
- session axis: `configure_rio` and `configure_s3_access` set GDAL/rasterio and S3 credentials once before materialization; `patch_url` (`Callable[[str], str]`) rewrites every parsed source href, and `with_properties` lifts named STAC item properties onto the cube as coordinates.
- evidence: each load captures item count, resolved CRS, resolution, GeoBox shape, selected bands, groupby key, chunk plan, and dtype as a coverage receipt.

[STACKING]:
- `pystac`(`.api/pystac.md`): `load(items, ...)` consumes a `pystac.Item` sequence or `ItemCollection` directly as the cube source — the parsed-item ground truth.
- `pystac-client`(`.api/pystac-client.md`): `ItemSearch.item_collection()`/`items()` feeds the live `/search` result straight into `load` as the `items` argument.
- `planetary-computer`(`.api/planetary-computer.md`): `patch_url=planetary_computer.sign` rewrites each parsed `RasterSource` Azure Blob href to a SAS-signed URL before the lazy cube reads it.
- `rioxarray`(`.api/rioxarray.md`): the returned cube carries the resolved CRS, so its `.rio` accessor warps (`reproject`), masks (`clip`), and writes (`to_raster`) the loaded coverage downstream.
- `data` owner: `load`/`stac_load` is the single `CATALOG_COVERAGE_ODCSTAC` rail, threading the `patch_url` signer and `with_properties` coordinate lift through one call.

[LOCAL_ADMISSION]:
- `load`/`stac_load` is the sole repo entry for STAC-item coverage; a folder composing it threads items, geobox controls, and the `patch_url` signer through the one call, never a per-collection loader wrapper.

[RAIL_LAW]:
- Package: `odc-stac`
- Owns: STAC `Item` to `xarray.Dataset` cube construction, lazy Dask graph assembly, output GeoBox resolution, per-band metadata extraction, and GDAL/S3 session configuration for cloud reads
- Accept: catalog coverage loading STAC items into a lazy or eager `xarray` cube with explicit CRS/resolution/extent/grouping controls
- Reject: wrapper-renames of `load`/`stac_load`; a hand-rolled GeoBox or resolution computation `odc.geo`/`output_geobox` already own; a hand-rolled raster reader duplicating `odc.loader`; a parallel loader type per collection or output dtype; STAC search/discovery belonging to the upstream catalog client
