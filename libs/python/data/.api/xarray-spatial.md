# [PY_DATA_API_XARRAY_SPATIAL]

`xarray-spatial` supplies the Numba-accelerated raster-analytics surface for the data terrain rail: a flat `xrspatial` namespace of pure-function operators that consume a 2D `xarray.DataArray` (or `Dataset`) elevation/band grid and return a same-shaped `DataArray` carrying the computed surface, classification, focal, proximity, or zonal result. The package owner composes the surface operators (`slope`, `aspect`, `hillshade`, `curvature`), classify breaks, focal/convolution kernels, proximity distance fields, and zonal reductions into the GEOSPATIAL_TERRAIN_GATED path; it owns terrain/DEM analytics with no GDAL or GEOS dependency, dispatching across NumPy, Dask, and CuPy backends through Numba, and it never re-implements the raster kernels xarray-spatial already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `xarray-spatial`
- package: `xarray-spatial`
- import: `xrspatial`
- owner: `data`
- rail: terrain
- installed: `0.10.10` docs-derived (companion <3.15 (numba)); live reflection pending env provisioning
- entry points: library use is import-only; operators are flat functions on `xrspatial` plus submodules `xrspatial.zonal`, `xrspatial.classify`, `xrspatial.focal`, `xrspatial.convolution`, `xrspatial.proximity`, `xrspatial.multispectral`, `xrspatial.pathfinding`
- capability: Numba-accelerated raster terrain/DEM analytics over `xarray` grids without GDAL/GEOS — surface metrics (slope, aspect, hillshade, curvature, viewshed), classification breaks, focal/convolution neighborhood statistics, proximity distance/allocation/direction fields, zonal reductions, multispectral indices, and A* pathfinding, with NumPy/Dask/CuPy backend dispatch

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: grid carrier and dispatch backends
- rail: terrain

The public surface is functional, not class-rooted: every operator accepts and returns `xarray.DataArray` (or `xarray.Dataset`, applied per data variable). The grid is the unit of design; backend is the array element type the `DataArray` wraps, dispatched by Numba at call time. Capacity/value errors surface as standard `ValueError` from the operators, not a package-specific exception.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]  | [RAIL]                                                             |
| :-----: | :----------------- | :------------- | :----------------------------------------------------------------- |
|  [01]   | `xarray.DataArray` | grid carrier   | 2D aggregate raster consumed and returned by every operator        |
|  [02]   | `xarray.Dataset`   | grid carrier   | multi-variable raster; operators apply independently per variable  |
|  [03]   | `numpy.ndarray`    | kernel carrier | 2D binary/weight kernel produced by `convolution` and fed to focal |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: surface terrain operators
- rail: terrain

Surface operators share `agg` (the elevation `DataArray`), a `name` output label, and a `boundary` edge policy; `slope`/`aspect` add `method` (`'planar'`) and `z_unit` (`'meter'`) rows. Each returns a 2D `DataArray` preserving input attributes.

| [INDEX] | [SURFACE]   | [CALL_SHAPE]                                                                                      | [CAPABILITY]                                 |
| :-----: | :---------- | :------------------------------------------------------------------------------------------------ | :------------------------------------------- |
|  [01]   | `slope`     | `slope(agg, name='slope', method='planar', z_unit='meter', boundary='nan')`                       | per-cell slope in degrees                    |
|  [02]   | `aspect`    | `aspect(agg, name='aspect', method='planar', z_unit='meter', boundary='nan')`                     | per-cell downslope compass direction         |
|  [03]   | `curvature` | `curvature(agg, name='curvature', boundary='nan')`                                                | per-cell surface curvature                   |
|  [04]   | `hillshade` | `hillshade(agg, azimuth=225, angle_altitude=25, name='hillshade', shadows=False, boundary='nan')` | shaded-relief illumination from sun position |
|  [05]   | `viewshed`  | `viewshed(raster, x, y, observer_elev=DEFAULT, target_elev=0)`                                    | visible-cells field from an observer point   |

[ENTRYPOINT_SCOPE]: classify break operators
- rail: terrain

Classification operators consume a continuous `DataArray` and emit a binned `DataArray`; `k`/`bins`/`num_sample` set the break policy. `natural_breaks` samples large inputs via `num_sample` before Jenks optimization.

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                                                        | [CAPABILITY]                       |
| :-----: | :------------------------ | :------------------------------------------------------------------ | :--------------------------------- |
|  [01]   | `classify.natural_breaks` | `natural_breaks(agg, num_sample=20000, name='natural_breaks', k=5)` | Jenks natural-breaks binning       |
|  [02]   | `classify.equal_interval` | `equal_interval(agg, k=5, name='equal_interval')`                   | equal-width interval binning       |
|  [03]   | `classify.quantile`       | `quantile(agg, k=4, name='quantile')`                               | equal-count quantile binning       |
|  [04]   | `classify.reclassify`     | `reclassify(agg, bins, new_values, name='reclassify')`              | explicit bin-edge reclassification |
|  [05]   | `classify.binary`         | `binary(agg, values, name='binary')`                                | membership-to-binary mask          |

[ENTRYPOINT_SCOPE]: focal and convolution neighborhood operators
- rail: terrain

Focal operators apply a `numpy.ndarray` kernel built by the `convolution` kernel constructors over each cell's neighborhood; `boundary` controls edge handling (`'nan'`, `'nearest'`, `'reflect'`, `'wrap'`).

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                                                        | [CAPABILITY]                                 |
| :-----: | :--------------------------- | :---------------------------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `focal.apply`                | `apply(agg, kernel, func=..., name='focal_apply', boundary='nan')`                  | user function over a kernel neighborhood     |
|  [02]   | `focal.focal_stats`          | `focal_stats(agg, kernel, stats_funcs=DEFAULT, name='focal_stats', boundary='nan')` | mean/max/min/range/std/var/sum/variety stack |
|  [03]   | `focal.hotspots`             | `hotspots(raster, kernel)`                                                          | Getis-Ord Gi* hot/cold spot z-scores         |
|  [04]   | `focal.mean`                 | `mean(agg, passes=1, excludes=[nan], name='mean')`                                  | 3x3 mean-filtered smoothing                  |
|  [05]   | `convolution.circle_kernel`  | `circle_kernel(cellsize_x, cellsize_y, radius)`                                     | circular `ndarray` kernel                    |
|  [06]   | `convolution.annulus_kernel` | `annulus_kernel(cellsize_x, cellsize_y, outer_radius, inner_radius)`                | ring-shaped `ndarray` kernel                 |
|  [07]   | `convolution.convolution_2d` | `convolution_2d(agg, kernel, name='convolution_2d', boundary='nan')`                | 2D kernel convolution over inner cells       |

[ENTRYPOINT_SCOPE]: proximity, zonal, and pathfinding operators
- rail: terrain

Proximity operators emit distance/allocation/direction fields from source cells under a `distance_metric`; `zonal.stats` reduces `values` per `zones` to a tabular receipt; `pathfinding.a_star_search` returns a cost-accumulated least-cost path grid.

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]                                                                                                                                           | [CAPABILITY]                               |
| :-----: | :-------------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------- | :----------------------------------------- |
|  [01]   | `proximity.proximity`       | `proximity(raster, x='x', y='y', target_values=None, max_distance=inf, distance_metric='EUCLIDEAN')`                                                   | per-cell distance to nearest source        |
|  [02]   | `proximity.allocation`      | `allocation(raster, x='x', y='y', target_values=None, max_distance=inf, distance_metric='EUCLIDEAN')`                                                  | nearest-source value field                 |
|  [03]   | `proximity.direction`       | `direction(raster, x='x', y='y', target_values=None, max_distance=inf, distance_metric='EUCLIDEAN')`                                                   | bearing-to-nearest-source field            |
|  [04]   | `zonal.stats`               | `stats(zones, values, zone_ids=None, stats_funcs=None, nodata_values=None, return_type='pandas.DataFrame', column=None, rasterize_kw=None)`            | per-zone statistic table                   |
|  [05]   | `zonal.apply`               | `apply(zones, values, func, nodata=0)`                                                                                                                 | per-zone user-function reduction           |
|  [06]   | `zonal.crosstab`            | `crosstab(zones, values, zone_ids=None, cat_ids=None, layer=None, agg='count', nodata_values=None)`                                                    | categorical zone-by-value cross-tabulation |
|  [07]   | `zonal.regions`             | `regions(raster, neighborhood=4, name='regions')`                                                                                                      | connected-component region labels          |
|  [08]   | `multispectral.ndvi`        | `ndvi(nir_agg, red_agg, name='ndvi')`                                                                                                                  | normalized difference vegetation index     |
|  [09]   | `pathfinding.a_star_search` | `a_star_search(surface, start, goal, barriers=[], x='x', y='y', connectivity=8, snap_start=False, snap_goal=False, friction=None, search_radius=None)` | least-cost A* path grid                    |

## [04]-[IMPLEMENTATION_LAW]

[TERRAIN_RASTER]:
- import: `import xrspatial` (or `from xrspatial import slope, aspect, hillshade, curvature`) at boundary scope only; module-level import is banned by the manifest import policy.
- grid axis: every operator consumes and returns one `xarray.DataArray`; a `Dataset` fans the operator across data variables — the grid is the canonical carrier, never a per-operation result class.
- surface axis: `slope`/`aspect`/`curvature`/`hillshade`/`viewshed` are the terrain-metric rows; `method` (`'planar'`) and `z_unit` (`'meter'`) are call policy, not parallel operator types; DEM analytics route here, never to a hand-rolled finite-difference kernel.
- classify axis: `natural_breaks`/`equal_interval`/`quantile`/`reclassify`/`binary` are the break-policy rows over one continuous grid; `k`/`bins`/`num_sample` are call rows, never a per-scheme classifier type.
- focal axis: `focal.apply`/`focal_stats`/`hotspots`/`mean` consume a `numpy.ndarray` kernel from `convolution.circle_kernel`/`annulus_kernel`; neighborhood statistics are kernel-keyed rows, never a parallel windowed type — `boundary` selects the edge policy.
- proximity axis: `proximity`/`allocation`/`direction` share `target_values`/`max_distance`/`distance_metric`; distance, nearest value, and bearing are sibling field rows over one source raster, never three unrelated kernels.
- zonal axis: `zonal.stats` is the single reduction surface keyed by `zones`/`stats_funcs`/`return_type`; per-zone aggregation is a row with a tabular receipt, never a hand-stitched groupby; `regions`/`crosstab`/`apply` extend the same zone vocabulary.
- backend axis: NumPy, Dask, and CuPy are element-type dispatch under Numba on the same operator surface, never a per-backend function family; out-of-core and GPU execution are array-type rows, not parallel APIs.
- evidence: each result captures operator name, resolved `boundary`/`method`/`z_unit`/`distance_metric`, output units, grid shape, and backend element type as a terrain receipt.
- boundary: xarray-spatial owns Numba raster terrain/DEM analytics with no GDAL/GEOS dependency; CRS resolution, vector geometry, and tiled GeoTIFF I/O route to their owners; the `DataArray` result feeds the persistence and visuals owners directly.

[RAIL_LAW]:
- Package: `xarray-spatial`
- Owns: Numba-accelerated raster terrain/DEM analytics over `xarray` grids — surface metrics, classification breaks, focal/convolution neighborhood statistics, proximity distance/allocation/direction fields, zonal reductions, multispectral indices, and A* pathfinding, with NumPy/Dask/CuPy dispatch
- Accept: terrain/DEM raster analysis on `xarray.DataArray` grids feeding the data, persistence, and visuals owners under GEOSPATIAL_TERRAIN_GATED
- Reject: wrapper-renames of `slope`/`aspect`/`hillshade`/`zonal.stats`; a hand-rolled finite-difference or Jenks kernel; a per-backend operator family where Numba dispatch handles NumPy/Dask/CuPy; a parallel result class per operator; GDAL/GEOS-bound raster owners this package displaces
