# [PY_DATA_API_XVEC]

`xvec` mints vector data cubes over `xarray`: a `shapely`-geometry array becomes a first-class cube dimension backed by `GeometryIndex`, so a labelled field cube carries a geometry coordinate with its `pyproj.CRS` and a cached `shapely.STRtree`. One `.xvec` accessor on `xr.DataArray` and `xr.Dataset` owns geometry-predicate selection, per-variable CRS reprojection, raster-to-vector extraction and zonal aggregation, CF/WKB encoding, and `geopandas` egress, while `shapely`, `pyproj`, and `geopandas` keep the kernels `xvec` composes rather than re-implements.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `xvec`
- package: `xvec` (MIT)
- module: `xvec`
- owner: `data`
- rail: geospatial-cube
- asset: pure Python; geometry kernel `shapely` 2 GEOS, CRS `pyproj`, index extends `xarray.Index` through `xproj.ProjIndexMixin`
- depends: `xarray`, `shapely`, `pyproj`, `cf_xarray`, `xproj`; `geopandas` and `matplotlib` back egress and plot

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry-dimension index

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :-------------- | :------------ | :------------------------------------------------------------------------ |
|  [01]   | `GeometryIndex` | class         | geometry-coordinate index over a `PandasIndex`; subclasses `xarray.Index` |
|  [02]   | `XvecAccessor`  | class         | `.xvec` accessor registered on `xr.DataArray` and `xr.Dataset` on import  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: index and accessor introspection

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `GeometryIndex.crs`                                      | property | coordinate `pyproj.CRS`                         |
|  [02]   | `GeometryIndex.sindex`                                   | property | lazily built, cached `shapely.STRtree`          |
|  [03]   | `GeometryIndex.from_variables(variables, *, options)`    | class    | `xarray` index-construction hook                |
|  [04]   | `GeometryIndex.sel(labels, method=None, tolerance=None)` | instance | label selection lowering to the STRtree         |
|  [05]   | `GeometryIndex.isel(indexers)`                           | instance | positional selection preserving the index       |
|  [06]   | `GeometryIndex.equals(other)`                            | instance | CRS-aware index equality                        |
|  [07]   | `XvecAccessor.geom_coords`                               | property | mapping of all geometry coordinates             |
|  [08]   | `XvecAccessor.geom_coords_indexed`                       | property | geometry coordinates carrying a `GeometryIndex` |
|  [09]   | `XvecAccessor.is_geom_variable(name, has_index=True)`    | instance | geometry-coordinate predicate                   |

[ENTRYPOINT_SCOPE]: index and CRS management

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------------------ | :------- | :---------------------------------------- |
|  [01]   | `set_geom_indexes(coord_names, crs=None, allow_override=False, **kwargs)` | instance | build a `GeometryIndex` on coordinates    |
|  [02]   | `set_crs(variable_crs=None, allow_override=False, **variable_crs_kwargs)` | instance | assign CRS metadata without reprojecting  |
|  [03]   | `to_crs(variable_crs=None, **variable_crs_kwargs)`                        | instance | reproject geometry values to a target CRS |

- `set_crs` versus `to_crs`: `set_crs` assigns declared metadata, `to_crs` reprojects values; mixing them silently corrupts coordinates.

[ENTRYPOINT_SCOPE]: geometry-predicate selection lowering to the cached `STRtree`

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `query(coord_name, geometry, predicate=None, distance=None, unique=False)` | instance | STRtree predicate selection of the sub-cube |
|  [02]   | `mask(geometry, predicate=None, distance=None)`                            | instance | boolean selection array over the geometry   |

[ENTRYPOINT_SCOPE]: raster-vector bridge

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :-------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `extract_points(points, x_coords, y_coords, tolerance=None, …)` | instance | sample the grid at point geometries    |
|  [02]   | `zonal_stats(geometry, x_coords, y_coords, stats='mean', …)`    | instance | per-zone raster aggregation            |
|  [03]   | `summarize_geometry(dim, geom_array=None, aggfunc='envelope')`  | instance | reduce a geometry dimension by aggfunc |

[ENTRYPOINT_SCOPE]: encoding and egress

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :----------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `encode_wkb()`                                               | instance | geometry coordinate to WKB-bytes          |
|  [02]   | `decode_wkb()`                                               | instance | WKB-bytes back to geometry coordinate     |
|  [03]   | `encode_cf()`                                                | instance | geometry to CF-conventions container      |
|  [04]   | `decode_cf()`                                                | instance | CF container back to geometry coordinate  |
|  [05]   | `to_geodataframe(*, name=None, geometry=None, long=True, …)` | instance | cube to `geopandas` `GeoDataFrame`        |
|  [06]   | `to_geopandas()`                                             | instance | shorthand `GeoDataFrame` egress           |
|  [07]   | `plot(*, row=None, col=None, hue=None, geometry=None, …)`    | instance | faceted geometry map through `matplotlib` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `.xvec` accessor on `xr.DataArray` and `xr.Dataset` is the sole entry; `XvecAccessor` and `GeometryIndex` are the only names a consumer references.
- `set_geom_indexes` promotes a geometry coordinate to `GeometryIndex`, from which `xarray` native `sel`/`isel`/`stack`/`concat`/`reindex_like` own spatial dispatch — never a hand-rolled loop over geometries.
- CRS threads through the `xproj.ProjIndexMixin`, so `set_crs` declares and `to_crs` reprojects per geometry dimension while `xproj`'s CRS accessors see the coordinate's projection.
- `query`/`mask` lower to the coordinate's cached `shapely.STRtree`, reading only the intersecting elements rather than scanning then filtering.
- Each call emits a cube receipt — operation, geometry coordinate, CRS, predicate/aggfunc, selected and aggregated counts — on the field receipt family.

[STACKING]:
- `xarray`(`libs/python/.api/xarray.md`): a coordinate carrying `GeometryIndex` is an ordinary `Index` backend, so `set_xindex`, `sel`/`isel`/`stack`, `flox` grouped reductions, and `zarr`/`netcdf4` persistence drive the cube unchanged.
- `shapely`(`libs/python/data/.api/shapely.md`): geometry values are a `GeometryArray` and `query`/`mask` lower to `STRtree.query` under a GEOS binary predicate (`intersects`/`within`/`contains`/`dwithin`).
- `pyproj`(`libs/python/data/.api/pyproj.md`): `crs` holds one `pyproj.CRS` and `to_crs` drives the `Transformer`, the `ProjIndexMixin` exposing the projection to `xproj`.
- `geopandas`(`libs/python/data/.api/geopandas.md`): `to_geodataframe`/`to_geopandas` lower the cube to a `GeoDataFrame` at the `GeometryArray`, and a `GeoSeries` reconstructs the coordinate on return.
- field-dataset rail: the geometry-indexed cube is a `FieldDataset`, so every field engine and receipt applies without a parallel container.

[LOCAL_ADMISSION]:
- `xvec` is admitted where a labelled field cube is queried, reprojected, or reduced by vector geometry; a `shapely`-plus-`xarray` hand-join carrying no `GeometryIndex` is the rejected form.

[RAIL_LAW]:
- Package: `xvec`
- Owns: the geometry-indexed `xarray` dimension — cube selection, per-variable CRS assignment and reprojection, raster-to-vector aggregation, CF/WKB encoding, and `geopandas` egress through one `.xvec` accessor.
- Accept: a coordinate promoted through `set_geom_indexes`; `query`/`mask` selection; `set_crs`-versus-`to_crs` discipline; `extract_points`/`zonal_stats` bridging; `to_geodataframe` hand-off.
- Reject: a hand-rolled geometry loop where `GeometryIndex` owns dispatch; `set_crs` where reprojection is required; a parallel container wrapping `xarray`.
