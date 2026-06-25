# [PY_DATA_API_ODC_STAC]

`odc-stac` turns a sequence of STAC `Item` objects into a lazy, multi-band `xarray.Dataset` cube backed by a Dask graph, resolving the output `GeoBox` (CRS, resolution, anchor, extent) from the items or from explicit load parameters and dispatching pixel reads through `odc.loader` against cloud raster assets. The data owner composes `load` (alias `stac_load`) as the single coverage rail for `CATALOG_COVERAGE_ODCSTAC`: items in, lazy cube out, with `groupby`, `bands`, `chunks`, `resampling`, and geobox controls expressed as call rows. The metadata path (`parse_items`, `extract_collection_metadata`, `output_geobox`) and the GDAL/S3 session configuration (`configure_rio`, `configure_s3_access`) are exposed for debugging and credential setup; the package never re-implements GeoBox math (it defers to `odc.geo`) or the raster reader (it defers to `odc.loader`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `odc-stac`
- package: `odc-stac`
- version: `0.5.2` reflected via `import odc.stac` (manifest unpinned — newest stable; requires-python `>=3.10`)
- license: Apache-2.0
- import: `odc.stac`
- abi: pure Python; native work defers to `odc.geo` (GeoBox math) and `odc.loader` (rasterio/GDAL raster reads) — no ABI floor of its own
- owner: `data`
- rail: catalog-coverage
- entry points: none (import-only library; `odc-stac` declares no console scripts)
- capability: STAC `Item` to `xarray.Dataset` conversion, lazy Dask cube construction, output GeoBox resolution (CRS/resolution/anchor/bbox/geopolygon), per-band metadata extraction and alias mapping, temporal/key grouping, resampling and dtype control, and GDAL/rasterio plus S3 session configuration for cloud reads

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: parsed item and raster metadata records
- rail: catalog-coverage

`load` consumes raw `pystac.Item` objects directly; the records below are the internal representation that `parse_item`/`parse_items` produce and `output_geobox` consumes. `ParsedItem` carries the loadable bands of one item; `RasterCollectionMetadata` carries collection-level band structure and the common-name alias map; `RasterBandMetadata`, `RasterSource`, and `RasterLoadParams` are re-exported from `odc.loader`. `ConversionConfig` is a `Dict[str, Any]` user-config alias passed as `stac_cfg`.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]    | [RAIL]                                                         |
| :-----: | :------------------------- | :--------------- | :------------------------------------------------------------- |
|  [01]   | `ParsedItem`               | dataclass record | one STAC item reduced to loadable raster bands, geometry, time |
|  [02]   | `RasterCollectionMetadata` | dataclass record | collection-level band structure, proj flag, band-to-grid map   |
|  [03]   | `RasterBandMetadata`       | dataclass record | per-band data type, nodata, units, dims (from `odc.loader`)    |
|  [04]   | `RasterSource`             | dataclass record | single band source: uri, band index, subdataset, geobox        |
|  [05]   | `RasterLoadParams`         | dataclass record | per-band load config: dtype, fill, resampling, overviews       |
|  [06]   | `ConversionConfig`         | type alias       | `Dict[str, Any]` user collection-config passed as `stac_cfg`   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cube load
- rail: catalog-coverage

`load` (re-exported as `stac_load`) is the single coverage surface: it accepts STAC items, resolves the output GeoBox, and returns a lazy or eager `xarray.Dataset`. The geobox is resolved from one of the mutually exclusive extent rows (`geobox`, `like`, `geopolygon`, `bbox`, `lon`/`lat`, `x`/`y`, `intersects`) combined with `crs`/`resolution`/`anchor`. Passing `chunks` produces a Dask-backed lazy cube; omitting it loads eagerly through `pool`.

| [INDEX] | [SURFACE]            | [CALL_SHAPE]                                                                                                                                                                                                                                                                                                                                                                                                                | [CAPABILITY]                                   |
| :-----: | :------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `load` / `stac_load` | `load(items, bands=None, *, groupby='time', resampling=None, dtype=None, chunks=None, pool=None, crs=Unset, resolution=None, anchor=None, geobox=None, bbox=None, lon=None, lat=None, x=None, y=None, like=None, geopolygon=None, intersects=None, progress=None, fail_on_error=True, stac_cfg=None, with_properties=None, patch_url=None, preserve_original_order=False, driver=None, fuse_func=None, **kw) -> xr.Dataset` | STAC items to lazy/eager `xarray.Dataset` cube |

[ENTRYPOINT_SCOPE]: metadata parse and geobox
- rail: catalog-coverage

These surfaces are exposed for debugging the load pipeline: `parse_item`/`parse_items` build the internal `ParsedItem` representation, `extract_collection_metadata` derives collection band structure from a sample item, and `output_geobox` computes the target GeoBox from parsed items and load parameters.

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]                                                                                                                                                                                                  | [CAPABILITY]                                       |
| :-----: | :---------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :------------------------------------------------- |
|  [01]   | `parse_item`                  | `parse_item(item, template=None, md_plugin=None, asset_absolute_paths=True) -> ParsedItem`                                                                                                                    | one STAC item to internal `ParsedItem`             |
|  [02]   | `parse_items`                 | `parse_items(items, cfg=None, md_plugin=None, asset_absolute_paths=True) -> Iterator[ParsedItem]`                                                                                                             | item sequence to `ParsedItem` iterator             |
|  [03]   | `extract_collection_metadata` | `extract_collection_metadata(item, cfg=None, md_plugin=None) -> RasterCollectionMetadata`                                                                                                                     | sample item to collection band metadata            |
|  [04]   | `output_geobox`               | `output_geobox(items, bands=None, *, crs=Unset, resolution=None, anchor=None, align=None, geobox=None, like=None, geopolygon=None, bbox=None, lon=None, lat=None, x=None, y=None, intersects=None) -> GeoBox` | resolve target GeoBox from parsed items and params |

[ENTRYPOINT_SCOPE]: GDAL and S3 session
- rail: catalog-coverage

These configure the rasterio/GDAL environment and S3 credentials, applied locally or propagated to a Dask cluster via the worker-plugin mechanism, before lazy cube materialization triggers cloud reads.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                                                                                                       | [CAPABILITY]                                     |
| :-----: | :-------------------- | :--------------------------------------------------------------------------------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `configure_rio`       | `configure_rio(*, cloud_defaults=False, verbose=False, aws=None, **params) -> None`                                                | set GDAL/rasterio config locally or on a cluster |
|  [02]   | `configure_s3_access` | `configure_s3_access(profile=None, region_name='auto', aws_unsigned=None, requester_pays=False, cloud_defaults=True, **gdal_opts)` | credentialize or set public S3 access for reads  |

## [04]-[IMPLEMENTATION_LAW]

[CATALOG_COVERAGE_ODCSTAC]:
- import: `import odc.stac` at boundary scope only; module-level import is banned by the manifest import policy.
- load axis: one `load` owns the items-to-cube conversion for `CATALOG_COVERAGE_ODCSTAC`; `bands`, `groupby`, `resampling`, `dtype`, `chunks`, and `pool` are call rows on that surface, never parallel loader types or a per-collection builder; `stac_load` is the alias for the identical callable, not a second entry point.
- laziness axis: `chunks` selects the Dask-backed lazy cube row; omitting `chunks` materializes eagerly through `pool`; cube laziness is a call argument, not a separate function.
- geobox axis: the output GeoBox resolves from exactly one extent row (`geobox`, `like`, `geopolygon`, `bbox`, `lon`/`lat`, `x`/`y`, `intersects`) combined with `crs`/`resolution`/`anchor`; `output_geobox` exposes that resolution for debugging but the canonical path is the `load` keyword set — never pre-compute a GeoBox to bypass `load`.
- metadata axis: `parse_items` and `extract_collection_metadata` produce `ParsedItem`/`RasterCollectionMetadata` for inspection and custom config; the live coverage path passes raw `pystac.Item` objects to `load` and lets it parse internally, never staging the internal representation by hand.
- session axis: `configure_rio` and `configure_s3_access` set GDAL/rasterio and S3 credentials once before materialization; cloud-read configuration is a session call, never per-source URI patching outside `patch_url`.
- signing axis: `patch_url` is the per-asset href rewriter (`Callable[[str], str]`) applied to every parsed source before read — the canonical home for the `obstore` presigned/credential seam (e.g. `patch_url=PlanetaryComputerCredentialProvider(...).sign` or an `obstore.sign`-derived signer) so the lazy cube reads SAS/presigned cloud rasters; `with_properties` lifts named STAC item properties onto the cube as coordinates.
- evidence: each load captures item count, resolved CRS, resolution, GeoBox shape, selected bands, groupby key, chunk plan, and dtype as a coverage receipt.
- boundary: `odc-stac` owns STAC-item-to-`xarray` conversion and load orchestration; GeoBox geometry defers to `odc.geo`, raster reading defers to `odc.loader` (which re-exports `RasterBandMetadata`/`RasterLoadParams`/`RasterSource`), and STAC parsing consumes `pystac.Item`; tile/scene discovery stays in the STAC search owner upstream of this rail.

[RAIL_LAW]:
- Package: `odc-stac`
- Owns: STAC `Item` to `xarray.Dataset` cube construction, lazy Dask graph assembly, output GeoBox resolution, per-band metadata extraction, and GDAL/S3 session configuration for cloud reads
- Accept: catalog coverage that loads STAC items into a lazy or eager `xarray` cube with explicit CRS/resolution/extent/grouping controls
- Reject: wrapper-renames of `load`/`stac_load`; a hand-rolled GeoBox or resolution computation that `odc.geo` and `output_geobox` already own; a hand-rolled raster reader duplicating `odc.loader`; a parallel loader type per collection or per output dtype; STAC search/discovery logic that belongs to the upstream catalog client
