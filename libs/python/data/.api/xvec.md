# [PY_DATA_API_XVEC]

`xvec` supplies vector data cubes for `xarray`: a `shapely`-geometry array becomes a first-class cube dimension backed by a custom `GeometryIndex`, so a labelled field cube gains a geometry coordinate carrying a `pyproj.CRS` and a `shapely.STRtree` index. Every capability rides one `.xvec` accessor registered on both `xr.DataArray` and `xr.Dataset` through the `xproj` machinery — geometry selection, per-variable CRS reprojection, raster-to-vector extraction and zonal aggregation, CF/WKB encoding, and `geopandas` egress — so a zone-keyed result is queryable by space without leaving the cube.

`GeometryIndex` is the single owner of the geometry-dimension behavior: it subclasses `xarray.Index` and threads the CRS through the `xproj` `ProjIndexMixin`, so `xarray`'s own `sel`/`isel`/`stack`/`concat`/`reindex_like` machinery dispatches spatial and index operations onto the backing `shapely` `STRtree`. `xvec` never re-implements the geometry kernel, projection engine, or dataframe container those owners hold — geometry is `shapely` 2 GEOS, CRS is `pyproj`, and vector egress lands as a `geopandas` `GeoDataFrame`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `xvec`
- package: `xvec`
- owner: `data`
- module: `xvec`
- asset: pure Python; geometry kernel is `shapely` 2 GEOS, CRS is `pyproj`, index behavior extends `xarray.Index` through the `xproj` `ProjIndexMixin`
- dependency: `xarray`, `shapely`, `pyproj`, `cf_xarray`, `xproj`; optional `geopandas` and `matplotlib` back the egress and plot legs
- license: `MIT`
- rail: geospatial-cube

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry-dimension index (`xvec.GeometryIndex`)
- rail: geospatial-cube

`GeometryIndex` subclasses `xarray.Index` and mixes in `xproj.ProjIndexMixin`, so assigning it to a coordinate makes that coordinate a geometry dimension `xarray` selects, stacks, and reindexes natively; `crs` carries the `pyproj.CRS` and `sindex` builds and caches the `shapely.STRtree` on first spatial query. Construction takes a `PandasIndex` over the geometry array and an optional CRS, but the accessor `set_geom_indexes` is the intended entry — direct construction is the internal path `xarray` drives through `from_variables`.

| [INDEX] | [SYMBOL]                       | [CALL_SHAPE]                          | [CAPABILITY]                                        |
| :-----: | :----------------------------- | :------------------------------------ | :-------------------------------------------------- |
|  [01]   | `GeometryIndex`                | `GeometryIndex(index, crs=None)`      | geometry-coordinate index over a `PandasIndex`      |
|  [02]   | `GeometryIndex.crs`            | property                              | coordinate `pyproj.CRS`                             |
|  [03]   | `GeometryIndex.sindex`         | property                              | lazily built, cached `shapely.STRtree`              |
|  [04]   | `GeometryIndex.from_variables` | `from_variables(variables, options)`  | `xarray` index-construction hook                    |
|  [05]   | `GeometryIndex.sel`            | `sel(labels, method=None, tolerance=None)` | label selection lowering to the STRtree        |
|  [06]   | `GeometryIndex.isel`           | `isel(indexers)`                      | positional selection preserving the index           |
|  [07]   | `GeometryIndex.equals`         | `equals(other)`                       | CRS-aware index equality                            |

[PUBLIC_TYPE_SCOPE]: cube accessor (`xvec.XvecAccessor`)
- rail: geospatial-cube

`XvecAccessor` is registered as `.xvec` on `xr.DataArray` and `xr.Dataset`; import of `xvec` installs it, and every method below reads through it. `geom_coords` and `geom_coords_indexed` expose the geometry coordinates on the object, and `is_geom_variable` tests whether a named coordinate carries geometry with an optional live-index requirement.

| [INDEX] | [SYMBOL]                            | [CALL_SHAPE]                                | [CAPABILITY]                                     |
| :-----: | :---------------------------------- | :------------------------------------------ | :----------------------------------------------- |
|  [01]   | `XvecAccessor.geom_coords`          | property                                    | mapping of all geometry coordinates              |
|  [02]   | `XvecAccessor.geom_coords_indexed`  | property                                    | geometry coordinates carrying a `GeometryIndex`  |
|  [03]   | `XvecAccessor.is_geom_variable`     | `is_geom_variable(name, has_index=True)`    | geometry-coordinate predicate                    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: index and CRS management
- rail: geospatial-cube

`set_geom_indexes` promotes one or more geometry coordinates to a `GeometryIndex`, the entry that turns a plain geometry array into a queryable dimension; `set_crs` assigns declared CRS metadata without transforming coordinates, and `to_crs` reprojects the geometry values — mixing them silently corrupts coordinates, mirroring the `geopandas` set-versus-to discipline. Each takes a per-variable CRS mapping or keyword form so a multi-geometry cube reprojects each dimension independently.

| [INDEX] | [SURFACE]           | [CALL_SHAPE]                                                       | [CAPABILITY]                             |
| :-----: | :------------------ | :---------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `set_geom_indexes`  | `set_geom_indexes(coord_names, crs=None, allow_override=False)` | build a `GeometryIndex` on coordinates |
|  [02]   | `set_crs`           | `set_crs(variable_crs=None, allow_override=False, **kwargs)`    | assign CRS metadata without reprojecting |
|  [03]   | `to_crs`            | `to_crs(variable_crs=None, **variable_crs_kwargs)`              | reproject geometry values to a target CRS |

[ENTRYPOINT_SCOPE]: geometry-predicate selection
- rail: geospatial-cube

`query` selects cube elements whose geometry coordinate satisfies a `shapely` binary predicate against a query geometry or sequence, and `mask` returns the boolean selection array for the same test; both lower to the coordinate's `STRtree`, taking `predicate` (`intersects`/`within`/`contains`/`dwithin`/…) and an optional `distance` for the `dwithin` family. `query` returns the selected sub-cube; `mask` returns the aligned boolean `DataArray` for downstream `where`.

| [INDEX] | [SURFACE] | [CALL_SHAPE]                                                                | [CAPABILITY]                                |
| :-----: | :-------- | :------------------------------------------------------------------------- | :------------------------------------------ |
|  [01]   | `query`   | `query(coord_name, geometry, predicate=None, distance=None, unique=False)` | STRtree predicate selection of the sub-cube |
|  [02]   | `mask`    | `mask(geometry, predicate=None, distance=None)`                            | boolean selection array over the geometry   |

[ENTRYPOINT_SCOPE]: raster-vector bridge
- rail: geospatial-cube

`extract_points` samples a gridded cube at a sequence of `shapely` points along named `x`/`y` coordinates, collapsing the raster dimensions into a new geometry dimension; `zonal_stats` aggregates raster cells falling within each zone geometry into per-zone statistics, discriminating the aggregation through `stats` (name, callable, or sequence) and the sampling engine through `method`/`strategy` with `all_touched` and `n_jobs` parallelism; `summarize_geometry` reduces a geometry dimension under a named `aggfunc` such as `envelope`.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                                                         | [CAPABILITY]                              |
| :-----: | :-------------------- | :---------------------------------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `extract_points`      | `extract_points(points, x_coords, y_coords, tolerance=None, …)` | sample the grid at point geometries |
|  [02]   | `zonal_stats`         | `zonal_stats(geometry, x_coords, y_coords, stats='mean', …)`    | per-zone raster aggregation         |
|  [03]   | `summarize_geometry`  | `summarize_geometry(dim, geom_array=None, aggfunc='envelope')`  | reduce a geometry dimension by aggfunc |

[ENTRYPOINT_SCOPE]: encoding and egress
- rail: geospatial-cube

`encode_wkb`/`decode_wkb` round-trip the geometry coordinate through a WKB-bytes representation for storage, and `encode_cf`/`decode_cf` lower to and lift from the CF-conventions geometry container for netCDF/Zarr persistence; `to_geodataframe` and `to_geopandas` hand the cube to a `geopandas` `GeoDataFrame`, and `plot` renders faceted geometry maps through `matplotlib` when the optional dependency is present.

| [INDEX] | [SURFACE]           | [CALL_SHAPE]                                                            | [CAPABILITY]                                 |
| :-----: | :------------------ | :--------------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `encode_wkb`        | `encode_wkb()`                                                         | geometry coordinate to WKB-bytes             |
|  [02]   | `decode_wkb`        | `decode_wkb()`                                                         | WKB-bytes back to geometry coordinate        |
|  [03]   | `encode_cf`         | `encode_cf()`                                                          | geometry to CF-conventions container         |
|  [04]   | `decode_cf`         | `decode_cf()`                                                          | CF container back to geometry coordinate     |
|  [05]   | `to_geodataframe`   | `to_geodataframe(*, geometry=None, long=True, …)`                      | cube to `geopandas` `GeoDataFrame`           |
|  [06]   | `to_geopandas`      | `to_geopandas()`                                                       | shorthand `GeoDataFrame` egress              |
|  [07]   | `plot`              | `plot(*, row=None, col=None, hue=None, geometry=None, …)`             | faceted geometry map through `matplotlib`    |

## [04]-[IMPLEMENTATION_LAW]

[GEOSPATIAL_CUBE_TOPOLOGY]:
- accessor axis: import of `xvec` registers `.xvec` on both `xr.DataArray` and `xr.Dataset`, so every method dispatches through one accessor with no per-object variant; the accessor is the sole public entry and `XvecAccessor`/`GeometryIndex` are the only names a consumer references.
- index axis: `set_geom_indexes` builds a `GeometryIndex` on a geometry coordinate, and from that point `xarray`'s native `sel`/`isel`/`stack`/`concat`/`reindex_like` drive spatial behavior — never a hand-rolled loop over geometries where the index owns the dispatch.
- CRS axis: `set_crs` assigns declared metadata and `to_crs` reprojects values; a per-variable mapping reprojects each geometry dimension independently, and the CRS threads through the `xproj` `ProjIndexMixin` so `xproj`'s own CRS accessors see the coordinate's projection.
- selection axis: `query`/`mask` lower to the coordinate's cached `shapely.STRtree` (`GeometryIndex.sindex`), so a spatial predicate reads only the intersecting elements rather than scanning every geometry then filtering.
- raster-vector axis: `extract_points`/`zonal_stats` cross a gridded cube to a geometry dimension in one call, collapsing the `x`/`y` raster axes; `zonal_stats` discriminates aggregation on `stats` and sampling on `method`/`strategy`, never a manual per-zone masking loop.
- evidence: each call captures operation name, geometry coordinate name, CRS, predicate/aggfunc, and selected/aggregated element counts as a cube receipt on the field receipt family.

[INTEGRATION_LAW]:
- xarray seam: `xvec` extends `xarray` rather than wrapping it — a vector data cube is an ordinary `xr.Dataset` whose geometry coordinate carries a `GeometryIndex`, so `flox` grouped reductions, `zarr`/`netcdf4` persistence, and the `FieldDataset` engines all apply unchanged; never mint a parallel container.
- shapely seam: geometry values are a `shapely` 2 `GeometryArray`; predicates and the `STRtree` are `shapely` GEOS, and `xvec` never re-implements a geometry predicate or spatial index those owners hold.
- pyproj seam: CRS is a `pyproj.CRS` held as coordinate metadata; pass an EPSG int, CRS, or WKT to `set_crs`/`to_crs` and let the `GeometryIndex` own the `pyproj.Transformer` — never reproject coordinates by hand.
- geopandas seam: `to_geodataframe`/`to_geopandas` lower a cube to a `geopandas` `GeoDataFrame` for the vector-frame plane, and a `GeoSeries` reconstructs a geometry coordinate on the way back; the two containers meet at the geometry array, not through a per-row Python geometry.

[RAIL_LAW]:
- Package: `xvec`
- Owns: the geometry-indexed `xarray` dimension — vector data cubes over a `GeometryIndex`, geometry-predicate selection through the cached `shapely.STRtree`, per-variable CRS assignment and reprojection, raster-to-vector `extract_points`/`zonal_stats`, CF/WKB encoding, and `geopandas` egress, all through the one `.xvec` accessor
- Accept: a geometry coordinate promoted through `set_geom_indexes`; `query`/`mask` for spatial selection; `set_crs` versus `to_crs` discipline; `extract_points`/`zonal_stats` for the raster-vector bridge; `to_geodataframe` as the vector-frame hand-off
- Reject: a hand-rolled geometry loop where the `GeometryIndex` owns `xarray` dispatch; CRS reassignment via `set_crs` where reprojection is required; a parallel cube container wrapping `xarray`; re-implementing the `shapely`/`pyproj`/`geopandas` capability `xvec` composes
