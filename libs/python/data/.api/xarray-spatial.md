# [PY_DATA_API_XARRAY_SPATIAL]

`xarray-spatial` supplies the Numba-accelerated raster-analytics surface for the data terrain rail: a flat `xrspatial` namespace of pure-function operators that consume a 2D `xarray.DataArray` (or `Dataset`) elevation/band grid and return a same-shaped `DataArray` (or a tabular receipt for zonal reductions) carrying the computed surface, terrain-metric, classification, focal, proximity, hydrology, morphology, interpolation, or zonal result. The package owner composes the surface operators (`slope`, `aspect`, `hillshade`, `curvature`, `viewshed`), terrain metrics (`tpi`/`tri`/`roughness`/`landforms`), classify breaks, focal/convolution kernels, proximity distance fields, the full hydrology stack (`flow_direction`/`flow_accumulation`/`watershed`/`stream_order`/`twi`/`hand` in d8/dinf/mfd variants), morphology, interpolation (`idw`/`kriging`/`spline`), and zonal reductions into the GEOSPATIAL_TERRAIN_GATED path; it dispatches across NumPy, Dask, and CuPy backends through Numba, and it never re-implements the raster kernels xarray-spatial already owns. xarray-spatial also owns `xarray`-native GeoTIFF I/O (`open_geotiff`/`to_geotiff`), `rasterize`/`polygonize` vector<->raster bridging, and `reproject`/`resample`/`merge`, so it is the terrain/DEM analytics AND raster I/O owner on the worker lane rather than deferring tiled GeoTIFF I/O to a separate owner.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `xarray-spatial`
- package: `xarray-spatial`
- import: `xrspatial`
- owner: `data`
- rail: terrain
- installed: `0.10.10`
- entry points: library use is import-only; every public operator is re-exported flat onto the `xrspatial` namespace from its owning submodule (`slope`, `aspect`/`eastness`/`northness`, `curvature`, `hillshade`, `viewshed`/`cumulative_viewshed`/`line_of_sight`/`visibility_frequency`, `classify`, `focal`, `convolution`, `proximity`, `surface_distance`, `zonal`, `multispectral`, `pathfinding`, `terrain`/`terrain_metrics`, `hydro`, `morphology`, `edge_detection`, `interpolate`, `kde`, `glcm`, `mahalanobis`, `normalize`, `geotiff`, `rasterize`/`polygonize`/`polygon_clip`, `reproject`/`resample`/`sieve`, `bump`/`perlin`, `corridor`/`cost_distance`/`balanced_allocation`, `fire`, `flood`, `dasymetric`, `diffusion`, `emerging_hotspots`, `bilateral`, `contour`)
- capability: Numba-accelerated raster analytics over `xarray` grids — surface metrics (slope/aspect/curvature/hillshade/viewshed), terrain metrics (tpi/tri/roughness/landforms), classification breaks, focal/convolution neighborhood statistics, proximity/surface distance/allocation/direction fields, hydrology (flow direction/accumulation/watershed/stream order/twi/hand in d8/dinf/mfd), morphology, edge detection, interpolation (idw/kriging/spline), KDE, GLCM/Mahalanobis texture, multispectral indices, A*/multi-stop pathfinding and least-cost corridors, GeoTIFF I/O, rasterize/polygonize, and reproject/resample/merge, with NumPy/Dask/CuPy backend dispatch

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

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                                                                                                                  | [CAPABILITY]                                                                                      |
| :-----: | :------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | `slope`                    | `slope(agg, name='slope', method='planar', z_unit='meter', boundary='nan')`                                                                   | per-cell slope in degrees                                                                         |
|  [02]   | `aspect`                   | `aspect(agg, name='aspect', method='planar', z_unit='meter', boundary='nan')`                                                                 | per-cell downslope compass direction; `eastness(agg)`/`northness(agg)` are the sin/cos components |
|  [03]   | `curvature`                | `curvature(agg, name='curvature', boundary='nan')`                                                                                            | per-cell surface curvature                                                                        |
|  [04]   | `hillshade`                | `hillshade(agg, azimuth=225, angle_altitude=25, name='hillshade', shadows=False, boundary='nan')`                                             | shaded-relief illumination from sun position                                                      |
|  [05]   | `viewshed`                 | `viewshed(raster, x, y, observer_elev, target_elev, max_distance, name)`                                                                      | visible-cells field from an observer point                                                        |
|  [06]   | `visibility.line_of_sight` | `line_of_sight(raster, ...)`; also `cumulative_viewshed`, `visibility_frequency`                                                              | line-of-sight and multi-observer visibility                                                       |
|  [07]   | `terrain.generate_terrain` | `generate_terrain(agg, x_range, y_range, seed, zfactor, full_extent, name, octaves, lacunarity, persistence, noise_mode, warp_strength, ...)` | synthetic fBm terrain DEM; `bump`/`perlin` are the noise primitives                               |

[ENTRYPOINT_SCOPE]: classify break operators
- rail: terrain

Classification operators consume a continuous `DataArray` and emit a binned `DataArray`; `k`/`bins`/`num_sample` set the break policy. `natural_breaks` samples large inputs via `num_sample` before Jenks optimization.

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]                                           | [CAPABILITY]                            |
| :-----: | :---------------------------------- | :----------------------------------------------------- | :-------------------------------------- |
|  [01]   | `classify.natural_breaks`           | `natural_breaks(agg, num_sample, name, k)`             | Jenks natural-breaks binning            |
|  [02]   | `classify.equal_interval`           | `equal_interval(agg, k=5, name='equal_interval')`      | equal-width interval binning            |
|  [03]   | `classify.quantile`                 | `quantile(agg, k, num_sample, name)`                   | equal-count quantile binning            |
|  [04]   | `classify.reclassify`               | `reclassify(agg, bins, new_values, name='reclassify')` | explicit bin-edge reclassification      |
|  [05]   | `classify.binary`                   | `binary(agg, values, name='binary')`                   | membership-to-binary mask               |
|  [06]   | `classify.box_plot`                 | `box_plot(agg, ...)`                                   | box-plot (Tukey) interval binning       |
|  [07]   | `classify.head_tail_breaks`         | `head_tail_breaks(agg, ...)`                           | head/tail breaks for heavy-tailed data  |
|  [08]   | `classify.maximum_breaks`           | `maximum_breaks(agg, ...)`                             | maximum-gap break binning               |
|  [09]   | `classify.percentiles` / `std_mean` | `percentiles(agg, ...)` / `std_mean(agg, ...)`         | percentile / standard-deviation binning |

[ENTRYPOINT_SCOPE]: focal and convolution neighborhood operators
- rail: terrain

Focal operators apply a `numpy.ndarray` kernel built by the `convolution` kernel constructors over each cell's neighborhood; `boundary` controls edge handling (`'nan'`, `'nearest'`, `'reflect'`, `'wrap'`).

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                                                                                      | [CAPABILITY]                                  |
| :-----: | :--------------------------- | :---------------------------------------------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `focal.apply`                | `apply(agg, kernel, func, name, boundary, raster)`                                                                | user function over a kernel neighborhood      |
|  [02]   | `focal.focal_stats`          | `focal_stats(agg, kernel, stats_funcs, name, boundary)`                                                           | mean/max/min/range/std/var/sum/variety stack  |
|  [03]   | `focal.hotspots`             | `hotspots(agg, kernel, name, boundary, raster)`                                                                   | Getis-Ord Gi* hot/cold spot z-scores          |
|  [04]   | `focal.mean`                 | `mean(agg, passes, excludes, name, boundary)`                                                                     | iterated mean-filtered smoothing              |
|  [05]   | `convolution.circle_kernel`  | `circle_kernel(cellsize_x, cellsize_y, radius)`                                                                   | circular `ndarray` kernel                     |
|  [06]   | `convolution.annulus_kernel` | `annulus_kernel(cellsize_x, cellsize_y, outer_radius, inner_radius)`                                              | ring-shaped `ndarray` kernel                  |
|  [07]   | `convolution.custom_kernel`  | `custom_kernel(kernel)`                                                                                           | validate a user `ndarray` kernel              |
|  [08]   | `convolution.calc_cellsize`  | `calc_cellsize(raster)`                                                                                           | derive `(cellsize_x, cellsize_y)` from coords |
|  [09]   | `convolution.convolution_2d` | `convolution_2d(agg, kernel, name, boundary)`; `convolve_2d(data, kernel, boundary)` is the bare-`ndarray` kernel | 2D kernel convolution over inner cells        |

[ENTRYPOINT_SCOPE]: proximity, zonal, and pathfinding operators
- rail: terrain

Proximity operators emit distance/allocation/direction fields from source cells under a `distance_metric`; `zonal.stats` reduces `values` per `zones` to a tabular receipt; `pathfinding.a_star_search` returns a cost-accumulated least-cost path grid.

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]                                                                                                                                                                                                | [CAPABILITY]                                                    |
| :-----: | :---------------------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | `proximity.proximity`               | `proximity(raster, x='x', y='y', target_values=None, max_distance=inf, distance_metric='EUCLIDEAN')`                                                                                                        | per-cell distance to nearest source                             |
|  [02]   | `proximity.allocation`              | `allocation(raster, x='x', y='y', target_values=None, max_distance=inf, distance_metric='EUCLIDEAN')`                                                                                                       | nearest-source value field                                      |
|  [03]   | `proximity.direction`               | `direction(raster, x='x', y='y', target_values=None, max_distance=inf, distance_metric='EUCLIDEAN')`                                                                                                        | bearing-to-nearest-source field                                 |
|  [04]   | `zonal.stats`                       | `stats(zones, values, zone_ids=None, stats_funcs=None, nodata_values=None, return_type='pandas.DataFrame', column=None, rasterize_kw=None)`                                                                 | per-zone statistic table                                        |
|  [05]   | `zonal.apply`                       | `apply(zones, values, func, nodata=0)`                                                                                                                                                                      | per-zone user-function reduction                                |
|  [06]   | `zonal.crosstab`                    | `crosstab(zones, values, zone_ids=None, cat_ids=None, layer=None, agg='count', nodata_values=None)`                                                                                                         | categorical zone-by-value cross-tabulation                      |
|  [07]   | `zonal.regions`                     | `regions(raster, neighborhood=4, name='regions')`                                                                                                                                                           | connected-component region labels                               |
|  [08]   | `surface_distance.surface_distance` | `surface_distance(...)`; also `surface_allocation`, `surface_direction`                                                                                                                                     | 3D-surface-aware distance/allocation/direction                  |
|  [09]   | `corridor.least_cost_corridor`      | `least_cost_corridor(...)`; `cost_distance`, `balanced_allocation` are the cost-surface siblings                                                                                                            | accumulated least-cost corridor surface                         |
|  [10]   | `multispectral.ndvi`                | `ndvi(nir_agg, red_agg, name)`; `savi(nir_agg, red_agg, soil_factor, name)` and `arvi`/`evi`/`gci`/`nbr`/`nbr2`/`ndbi`/`ndmi`/`ndsi`/`ndwi`/`mndwi`/`osavi`/`sipi`/`bai`/`msavi2` share the band-pair shape | normalized-difference and soil/atmosphere-adjusted band indices |
|  [11]   | `pathfinding.a_star_search`         | `a_star_search(surface, start, goal, barriers, x, y, connectivity, snap_start, snap_goal, friction, search_radius)`; `multi_stop_search(...)` chains waypoints                                              | least-cost A* / multi-stop path grid                            |

[ENTRYPOINT_SCOPE]: terrain-metric and hydrology operators
- rail: terrain

Terrain metrics share the `agg`/`name`/`boundary` surface-operator shape; the hydrology stack threads a flow grid through direction -> accumulation -> stream/watershed, each operator exposing `_d8`/`_dinf`/`_mfd` flow-model variants plus an unsuffixed dispatcher.

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]                                                                             | [CAPABILITY]                                             |
| :-----: | :----------------------------------- | :--------------------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `terrain_metrics.tpi`                | `tpi(agg, name, boundary)`                                                               | topographic position index                               |
|  [02]   | `terrain_metrics.tri`                | `tri(agg, name, boundary)`                                                               | terrain ruggedness index                                 |
|  [03]   | `terrain_metrics.roughness`          | `roughness(agg, name, boundary)`                                                         | per-cell roughness                                       |
|  [04]   | `terrain_metrics.landforms`          | `landforms(agg, inner_radius, outer_radius, slope_threshold, name)` (`LANDFORM_CLASSES`) | TPI/slope landform classification                        |
|  [05]   | `hydro.fill` / `fill_d8`             | `fill(...)` / `fill_d8(...)`                                                             | depression/pit filling                                   |
|  [06]   | `hydro.flow_direction`               | `flow_direction(...)` (`_d8`/`_dinf`/`_mfd`)                                             | D8/D-infinity/MFD flow direction                         |
|  [07]   | `hydro.flow_accumulation`            | `flow_accumulation(...)` (`_d8`/`_dinf`/`_mfd`)                                          | upstream contributing-area accumulation                  |
|  [08]   | `hydro.flow_length` / `flow_path`    | `flow_length(...)` / `flow_path(...)` (`_d8`/`_dinf`/`_mfd`)                             | downstream flow length and traced path                   |
|  [09]   | `hydro.stream_order` / `stream_link` | `stream_order(...)` / `stream_link(...)` (`_d8`/`_dinf`/`_mfd`)                          | Strahler stream order and link labelling                 |
|  [10]   | `hydro.watershed` / `basin`          | `watershed(...)` / `basin(...)` / `basins(...)` (`_d8`/`_dinf`/`_mfd`)                   | watershed and basin delineation                          |
|  [11]   | `hydro.twi` / `hand`                 | `twi(...)` / `hand(...)` (`_d8`/`_dinf`/`_mfd`)                                          | topographic wetness index; height above nearest drainage |
|  [12]   | `hydro.sink` / `snap_pour_point`     | `sink(...)` / `snap_pour_point(...)` (`_d8`)                                             | sink detection; pour-point snapping                      |

[ENTRYPOINT_SCOPE]: morphology, edge detection, interpolation, density, texture
- rail: terrain

Morphology shares the `agg`/`kernel`/`boundary` shape; edge detection emits gradient/Laplacian responses; interpolation and KDE build a continuous grid from scattered samples; GLCM/Mahalanobis carry texture/anomaly evidence.

| [INDEX] | [SURFACE]                                   | [CALL_SHAPE]                                                                                                                                          | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------ | :---------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `morphology.morph_dilate`                   | `morph_dilate(agg, kernel, boundary, name)`; `morph_erode`/`morph_opening`/`morph_closing`/`morph_gradient`/`morph_white_tophat`/`morph_black_tophat` | grayscale morphology family                                           |
|  [02]   | `edge_detection.sobel_x`                    | `sobel_x(agg, name, boundary)`; `sobel_y`/`prewitt_x`/`prewitt_y`/`laplacian`                                                                         | gradient and Laplacian edge responses                                 |
|  [03]   | `interpolate.idw`                           | `idw(...)`; `kriging(...)`; `spline(...)`                                                                                                             | inverse-distance / kriging / spline interpolation of scattered points |
|  [04]   | `kde.kde` / `line_density`                  | `kde(x, y, weights, bandwidth, kernel, template, x_range, y_range, width, height, name)` / `line_density(...)`                                        | kernel density / line density surface                                 |
|  [05]   | `glcm.glcm_texture` / `bilateral.bilateral` | `glcm_texture(...)` / `bilateral(...)`                                                                                                                | GLCM texture metrics; edge-preserving bilateral filter                |
|  [06]   | `mahalanobis.mahalanobis`                   | `mahalanobis(...)`                                                                                                                                    | per-cell Mahalanobis distance (multiband anomaly)                     |
|  [07]   | `normalize.rescale` / `standardize`         | `rescale(...)` / `standardize(...)`                                                                                                                   | min-max rescale / z-score standardization                             |

[ENTRYPOINT_SCOPE]: raster I/O and vector bridging
- rail: terrain

xarray-spatial owns `xarray`-native GeoTIFF read/write and raster<->vector conversion; the grid carrier never leaves the `DataArray`/`Dataset` model. `rasterize` burns vector geometries into a grid, `polygonize` extracts polygon features, and `reproject`/`resample`/`merge` handle CRS/grid alignment.

| [INDEX] | [SURFACE]                             | [CALL_SHAPE]                                                                                                                                                           | [CAPABILITY]                                 |
| :-----: | :------------------------------------ | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `geotiff.open_geotiff` / `to_geotiff` | `open_geotiff(...)` / `to_geotiff(...)`                                                                                                                                | read/write a GeoTIFF into/from a `DataArray` |
|  [02]   | `rasterize.rasterize`                 | `rasterize(geometries, width, height, bounds, column, columns, fill, dtype, all_touched, gpu, name, resolution, like, merge, chunks, max_pixels, check_crs, use_cuda)` | burn vector geometries into a raster grid    |
|  [03]   | `polygonize.polygonize`               | `polygonize(raster, mask, connectivity, transform, column_name, return_type, simplify_tolerance, simplify_method, atol, rtol)`                                         | extract polygon features from a raster       |
|  [04]   | `polygon_clip.clip_polygon`           | `clip_polygon(...)`                                                                                                                                                    | clip a raster to polygon geometry            |
|  [05]   | `reproject.reproject` / `merge`       | `reproject(...)` / `merge(...)`                                                                                                                                        | CRS reprojection; multi-raster mosaic        |
|  [06]   | `resample.resample`                   | `resample(agg, scale_factor, target_resolution, method, nodata, name)`                                                                                                 | up/down-sample to a target resolution        |
|  [07]   | `sieve.sieve`                         | `sieve(...)`                                                                                                                                                           | remove small connected regions               |

## [04]-[IMPLEMENTATION_LAW]

[TERRAIN_RASTER]:
- import: `import xrspatial` (or `from xrspatial import slope, aspect, hillshade, curvature`) at boundary scope only; module-level import is banned by the manifest import policy.
- grid axis: every operator consumes and returns one `xarray.DataArray`; a `Dataset` fans the operator across data variables — the grid is the canonical carrier, never a per-operation result class.
- surface axis: `slope`/`aspect`/`curvature`/`hillshade`/`viewshed` are the surface-metric rows and `terrain_metrics.tpi`/`tri`/`roughness`/`landforms` the morphometric rows; `method` (`'planar'`) and `z_unit` (`'meter'`) are call policy, not parallel operator types; DEM analytics route here, never to a hand-rolled finite-difference kernel.
- classify axis: `natural_breaks`/`equal_interval`/`quantile`/`reclassify`/`binary`/`box_plot`/`head_tail_breaks`/`maximum_breaks`/`percentiles`/`std_mean` are the break-policy rows over one continuous grid; `k`/`bins`/`num_sample` are call rows, never a per-scheme classifier type.
- focal axis: `focal.apply`/`focal_stats`/`hotspots`/`mean` consume a `numpy.ndarray` kernel from `convolution.circle_kernel`/`annulus_kernel`/`custom_kernel` (cell size via `calc_cellsize`); neighborhood statistics are kernel-keyed rows, never a parallel windowed type — `boundary` selects the edge policy.
- hydro axis: the hydrology stack threads one flow grid — `fill` -> `flow_direction` -> `flow_accumulation` -> `stream_order`/`watershed`/`twi`/`hand`; the flow model is the suffix row (`_d8`/`_dinf`/`_mfd`) on each operator with an unsuffixed dispatcher, never a parallel per-model module. Watershed/stream analysis routes here, never to an external flow kernel.
- proximity axis: `proximity`/`allocation`/`direction` share `target_values`/`max_distance`/`distance_metric`; distance, nearest value, and bearing are sibling field rows over one source raster; `surface_distance.*` add the 3D-surface-aware variants and `corridor`/`cost_distance`/`balanced_allocation` the cost-surface family — never three unrelated kernels.
- zonal axis: `zonal.stats` is the single reduction surface keyed by `zones`/`stats_funcs`/`return_type`; per-zone aggregation is a row with a tabular receipt, never a hand-stitched groupby; `regions`/`crosstab`/`apply`/`crop`/`trim`/`hypsometric_integral` extend the same zone vocabulary.
- backend axis: NumPy, Dask, and CuPy are element-type dispatch under Numba on the same operator surface, never a per-backend function family; out-of-core and GPU execution are array-type rows, not parallel APIs.
- evidence: each result captures operator name, resolved `boundary`/`method`/`z_unit`/`distance_metric`/flow-model suffix, output units, grid shape, and backend element type as a terrain receipt.

[RAIL_LAW]:
- Package: `xarray-spatial`
- Owns: Numba-accelerated raster analytics over `xarray` grids — surface and terrain metrics, classification breaks, focal/convolution neighborhood statistics, hydrology (flow direction/accumulation/watershed/stream order/twi/hand in d8/dinf/mfd), morphology, edge detection, interpolation/KDE, proximity/surface-distance/corridor fields, zonal reductions, multispectral indices, A*/multi-stop pathfinding, GeoTIFF I/O, rasterize/polygonize, and reproject/resample/merge, with NumPy/Dask/CuPy dispatch
- Accept: raster terrain/DEM/hydrology analysis and `xarray`-native raster I/O on `DataArray`/`Dataset` grids feeding the data, persistence, and visuals owners under GEOSPATIAL_TERRAIN_GATED
- Reject: wrapper-renames of `slope`/`aspect`/`hillshade`/`zonal.stats`/`flow_accumulation`; a hand-rolled finite-difference, Jenks, flow-routing, or GeoTIFF kernel where xarray-spatial owns it; a per-backend operator family where Numba dispatch handles NumPy/Dask/CuPy; a per-flow-model module where the `_d8`/`_dinf`/`_mfd` suffix discriminates; a parallel result class per operator
