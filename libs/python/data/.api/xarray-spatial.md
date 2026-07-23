# [PY_DATA_API_XARRAY_SPATIAL]

`xarray-spatial` mints the Numba-accelerated raster-analytics surface for the data terrain rail: a flat `xrspatial` namespace of pure-function operators consuming a 2D `xarray.DataArray`/`Dataset` elevation or band grid and returning a same-shaped grid, or a tabular receipt for zonal reductions. Numba dispatches every operator across NumPy, Dask, and CuPy element types on one grid carrier. Coverage raster IO — GeoTIFF read and writeback, reprojection, mosaic, and vector<->raster bridging — routes to the `rioxarray` and `rasterio` owners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `xarray-spatial`
- package: `xarray-spatial`
- module: `xrspatial`
- namespaces: `xrspatial` (flat; every operator re-exported from its owning submodule)
- owner: `data`
- rail: terrain

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: grid carrier and dispatch backends

Every operator accepts and returns a `DataArray` (or a `Dataset`, applied per data variable); the grid is the unit of design and the backend is the array element type Numba dispatches at call time. Capacity and value faults surface as standard `ValueError`, never a package-specific exception.

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]  | [CAPABILITY]                                                       |
| :-----: | :----------------- | :------------- | :----------------------------------------------------------------- |
|  [01]   | `xarray.DataArray` | grid carrier   | 2D aggregate raster consumed and returned by every operator        |
|  [02]   | `xarray.Dataset`   | grid carrier   | multi-variable raster; operators apply independently per variable  |
|  [03]   | `numpy.ndarray`    | kernel carrier | 2D binary/weight kernel produced by `convolution` and fed to focal |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: surface terrain operators

Surface operators share `agg`, `name`, and `boundary`; `slope`/`aspect` add `method` and `z_unit`. Each returns a 2D `DataArray` preserving input attributes.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                                         | [CAPABILITY]                                        |
| :-----: | :------------------------- | :--------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `slope`                    | `slope(agg, name, method, z_unit, boundary)`         | per-cell slope in degrees                           |
|  [02]   | `aspect`                   | `aspect(agg, name, method, z_unit, boundary)`        | downslope direction; `eastness`/`northness` sin/cos |
|  [03]   | `curvature`                | `curvature(agg, name, boundary)`                     | per-cell surface curvature                          |
|  [04]   | `hillshade`                | `hillshade(agg, azimuth, angle_altitude, boundary)`  | shaded-relief illumination                          |
|  [05]   | `viewshed`                 | `viewshed(raster, x, y, observer_elev, ...)`         | visible-cells field from an observer                |
|  [06]   | `visibility.line_of_sight` | `line_of_sight(raster, ...)`                         | line-of-sight and multi-observer visibility         |
|  [07]   | `terrain.generate_terrain` | `generate_terrain(agg, x_range, y_range, seed, ...)` | synthetic fBm DEM; `bump`/`perlin` primitives       |

[ENTRYPOINT_SCOPE]: classify break operators

Classification operators consume a continuous grid and emit a binned grid; `k`/`bins`/`num_sample` set the break policy, `natural_breaks` sampling large inputs before Jenks optimization.

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]                                   | [CAPABILITY]                            |
| :-----: | :---------------------------------- | :--------------------------------------------- | :-------------------------------------- |
|  [01]   | `classify.natural_breaks`           | `natural_breaks(agg, num_sample, name, k)`     | Jenks natural-breaks binning            |
|  [02]   | `classify.equal_interval`           | `equal_interval(agg, k, name)`                 | equal-width interval binning            |
|  [03]   | `classify.quantile`                 | `quantile(agg, k, num_sample, name)`           | equal-count quantile binning            |
|  [04]   | `classify.reclassify`               | `reclassify(agg, bins, new_values, name)`      | explicit bin-edge reclassification      |
|  [05]   | `classify.binary`                   | `binary(agg, values, name)`                    | membership-to-binary mask               |
|  [06]   | `classify.box_plot`                 | `box_plot(agg, ...)`                           | box-plot (Tukey) interval binning       |
|  [07]   | `classify.head_tail_breaks`         | `head_tail_breaks(agg, ...)`                   | head/tail breaks for heavy-tailed data  |
|  [08]   | `classify.maximum_breaks`           | `maximum_breaks(agg, ...)`                     | maximum-gap break binning               |
|  [09]   | `classify.percentiles` / `std_mean` | `percentiles(agg, ...)` / `std_mean(agg, ...)` | percentile / standard-deviation binning |

[ENTRYPOINT_SCOPE]: focal and convolution neighborhood operators

Focal operators apply a `numpy.ndarray` kernel from the `convolution` builders over each cell's neighborhood; `boundary` selects edge handling (`nan`/`nearest`/`reflect`/`wrap`). `convolve_2d(data, kernel, boundary)` is the bare-`ndarray` form of `convolution_2d`.

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                            | [CAPABILITY]                                  |
| :-----: | :--------------------------- | :------------------------------------------------------ | :-------------------------------------------- |
|  [01]   | `focal.apply`                | `apply(agg, kernel, func, name, boundary, raster)`      | user function over a kernel neighborhood      |
|  [02]   | `focal.focal_stats`          | `focal_stats(agg, kernel, stats_funcs, name, boundary)` | mean/max/min/range/std/var/sum/variety stack  |
|  [03]   | `focal.hotspots`             | `hotspots(agg, kernel, name, boundary, raster)`         | Getis-Ord Gi* hot/cold spot z-scores          |
|  [04]   | `focal.mean`                 | `mean(agg, passes, excludes, name, boundary)`           | iterated mean-filtered smoothing              |
|  [05]   | `convolution.circle_kernel`  | `circle_kernel(cellsize_x, cellsize_y, radius)`         | circular `ndarray` kernel                     |
|  [06]   | `convolution.annulus_kernel` | `annulus_kernel(cellsize_x, cellsize_y, outer, inner)`  | ring-shaped `ndarray` kernel                  |
|  [07]   | `convolution.custom_kernel`  | `custom_kernel(kernel)`                                 | validate a user `ndarray` kernel              |
|  [08]   | `convolution.calc_cellsize`  | `calc_cellsize(raster)`                                 | derive `(cellsize_x, cellsize_y)` from coords |
|  [09]   | `convolution.convolution_2d` | `convolution_2d(agg, kernel, name, boundary)`           | 2D kernel convolution over inner cells        |

[ENTRYPOINT_SCOPE]: proximity, zonal, and pathfinding operators

Proximity operators emit distance/allocation/direction fields under a `distance_metric`; `zonal.stats` reduces `values` per `zones` to a tabular receipt; `pathfinding.a_star_search` returns a cost-accumulated least-cost path grid.
- `proximity`/`allocation`/`direction` share `(raster, x, y, target_values, max_distance, distance_metric)`.
- `zonal.stats(zones, values, *, zone_ids, stats_funcs, nodata_values, return_type, column, rasterize_kw)`; `zonal.apply`/`crosstab`/`regions` extend the zone vocabulary.
- `surface_distance.surface_distance(...)` with `surface_allocation`/`surface_direction`; `corridor.least_cost_corridor(...)` with `cost_distance`/`balanced_allocation`.
- `multispectral`: `ndvi` `savi` `arvi` `evi` `gci` `nbr` `nbr2` `ndbi` `ndmi` `ndsi` `ndwi` `mndwi` `osavi` `sipi` `bai` `msavi2` share `(nir_agg, red_agg, name)`.
- `pathfinding.a_star_search(surface, start, goal, barriers, connectivity, friction, ...)`; `multi_stop_search(...)` chains waypoints.

| [INDEX] | [SURFACE]                           | [CAPABILITY]                                                    |
| :-----: | :---------------------------------- | :-------------------------------------------------------------- |
|  [01]   | `proximity.proximity`               | per-cell distance to nearest source                             |
|  [02]   | `proximity.allocation`              | nearest-source value field                                      |
|  [03]   | `proximity.direction`               | bearing-to-nearest-source field                                 |
|  [04]   | `zonal.stats`                       | per-zone statistic table                                        |
|  [05]   | `zonal.apply`                       | per-zone user-function reduction                                |
|  [06]   | `zonal.crosstab`                    | categorical zone-by-value cross-tabulation                      |
|  [07]   | `zonal.regions`                     | connected-component region labels                               |
|  [08]   | `surface_distance.surface_distance` | 3D-surface-aware distance/allocation/direction                  |
|  [09]   | `corridor.least_cost_corridor`      | accumulated least-cost corridor surface                         |
|  [10]   | `multispectral.ndvi`                | normalized-difference and soil/atmosphere-adjusted band indices |
|  [11]   | `pathfinding.a_star_search`         | least-cost A* / multi-stop path grid                            |

[ENTRYPOINT_SCOPE]: terrain-metric and hydrology operators

Terrain metrics share the `agg`/`name`/`boundary` surface shape; the hydrology stack threads one flow grid `fill` -> `flow_direction` -> `flow_accumulation` -> `stream_order`/`watershed`, each operator carrying `_d8`/`_dinf`/`_mfd` variants and an unsuffixed dispatcher. `landforms(agg, inner_radius, outer_radius, slope_threshold, name)` classifies by TPI and slope.

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]                                    | [CAPABILITY]                                 |
| :-----: | :----------------------------------- | :---------------------------------------------- | :------------------------------------------- |
|  [01]   | `terrain_metrics.tpi`                | `tpi(agg, name, boundary)`                      | topographic position index                   |
|  [02]   | `terrain_metrics.tri`                | `tri(agg, name, boundary)`                      | terrain ruggedness index                     |
|  [03]   | `terrain_metrics.roughness`          | `roughness(agg, name, boundary)`                | per-cell roughness                           |
|  [04]   | `terrain_metrics.landforms`          | `landforms(...)` (`LANDFORM_CLASSES`)           | TPI/slope landform classification            |
|  [05]   | `hydro.fill` / `fill_d8`             | `fill(...)` / `fill_d8(...)`                    | depression/pit filling                       |
|  [06]   | `hydro.flow_direction`               | `flow_direction(...)`                           | D8/D-infinity/MFD flow direction             |
|  [07]   | `hydro.flow_accumulation`            | `flow_accumulation(...)`                        | upstream contributing-area accumulation      |
|  [08]   | `hydro.flow_length` / `flow_path`    | `flow_length(...)` / `flow_path(...)`           | downstream flow length and traced path       |
|  [09]   | `hydro.stream_order` / `stream_link` | `stream_order(...)` / `stream_link(...)`        | Strahler stream order and link labelling     |
|  [10]   | `hydro.watershed` / `basin`          | `watershed(...)` / `basin(...)` / `basins(...)` | watershed and basin delineation              |
|  [11]   | `hydro.twi` / `hand`                 | `twi(...)` / `hand(...)`                        | wetness index; height above nearest drainage |
|  [12]   | `hydro.sink` / `snap_pour_point`     | `sink(...)` / `snap_pour_point(...)` (`_d8`)    | sink detection; pour-point snapping          |

[ENTRYPOINT_SCOPE]: morphology, edge detection, interpolation, density, texture

Morphology shares the `agg`/`kernel`/`boundary` shape; edge detection emits gradient/Laplacian responses; interpolation and KDE build a continuous grid from scattered samples; GLCM/Mahalanobis carry texture and anomaly evidence.
- `morphology`: `morph_dilate` `morph_erode` `morph_opening` `morph_closing` `morph_gradient` `morph_white_tophat` `morph_black_tophat` share `(agg, kernel, boundary, name)`.
- `edge_detection`: `sobel_x` `sobel_y` `prewitt_x` `prewitt_y` `laplacian` share `(agg, name, boundary)`.
- `interpolate.idw`/`kriging`/`spline`; `kde.kde(x, y, weights, bandwidth, kernel, ...)` / `line_density`; `glcm.glcm_texture`; `bilateral.bilateral`; `mahalanobis.mahalanobis`; `normalize.rescale` / `standardize`.

| [INDEX] | [SURFACE]                                   | [CAPABILITY]                                                          |
| :-----: | :------------------------------------------ | :-------------------------------------------------------------------- |
|  [01]   | `morphology.morph_dilate`                   | grayscale morphology family                                           |
|  [02]   | `edge_detection.sobel_x`                    | gradient and Laplacian edge responses                                 |
|  [03]   | `interpolate.idw`                           | inverse-distance / kriging / spline interpolation of scattered points |
|  [04]   | `kde.kde` / `line_density`                  | kernel density / line density surface                                 |
|  [05]   | `glcm.glcm_texture` / `bilateral.bilateral` | GLCM texture metrics; edge-preserving bilateral filter                |
|  [06]   | `mahalanobis.mahalanobis`                   | per-cell Mahalanobis distance (multiband anomaly)                     |
|  [07]   | `normalize.rescale` / `standardize`         | min-max rescale / z-score standardization                             |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- import: `import xrspatial` (or `from xrspatial import slope, aspect, hillshade`) at boundary scope only per the manifest import policy.
- grid axis: every operator consumes and returns one `DataArray`; a `Dataset` fans the operator across data variables — the grid is the canonical carrier, never a per-operation result class.
- surface axis: `method` and `z_unit` are call policy on the morphometric rows, not parallel operator types; DEM analytics route here, never a hand-rolled finite-difference kernel.
- classify axis: the break-policy operators bin one continuous grid; `k`/`bins`/`num_sample` are call rows, never a per-scheme classifier type.
- focal axis: neighborhood statistics consume a `convolution`-built `numpy.ndarray` kernel (cell size via `calc_cellsize`) as kernel-keyed rows; `boundary` selects the edge policy.
- hydro axis: the stack threads one flow grid `fill` -> `flow_direction` -> `flow_accumulation` -> `stream_order`/`watershed`/`twi`/`hand`; the flow model is the `_d8`/`_dinf`/`_mfd` suffix row with an unsuffixed dispatcher, never a per-model module.
- proximity axis: `proximity`/`allocation`/`direction` share `target_values`/`max_distance`/`distance_metric`; `surface_distance.*` add the 3D-surface variants and `corridor`/`cost_distance`/`balanced_allocation` the cost-surface family over one source raster.
- zonal axis: `zonal.stats` is the single reduction surface keyed by `zones`/`stats_funcs`/`return_type` emitting a tabular receipt; `regions`/`crosstab`/`apply`/`crop`/`trim`/`hypsometric_integral` extend the same zone vocabulary.
- backend axis: NumPy, Dask, and CuPy are element-type dispatch under Numba on the same operator surface; out-of-core and GPU execution are array-type rows, never parallel APIs.
- evidence: each result captures operator name, resolved `boundary`/`method`/`z_unit`/`distance_metric`/flow-model suffix, output units, grid shape, and backend element type as a terrain receipt.

[STACKING]:
- `rioxarray`(`.api/rioxarray.md`): a terrain result `DataArray` flows into the `.rio` accessor for CRS/transform tagging, `reproject`/`reproject_match` grid alignment, `merge_arrays`/`merge_datasets` mosaic, and `to_raster` GeoTIFF writeback — the coverage raster-IO path this surface reads its grids from and writes its results to.
- `rasterio`(`.api/rasterio.md`): `features.rasterize`/`features.shapes` bridge vector<->raster into and out of the grid, `warp.reproject`/`WarpedVRT` and `merge` supply CRS and mosaic, and `features.sieve` cleans a classified `xrspatial` output.
- within-lib: the flat `xrspatial` namespace chains operators on one grid — `fill` -> `flow_direction` -> `flow_accumulation` -> `hand`/`twi`, or `slope`/`aspect` -> `classify.natural_breaks` -> `zonal.stats` — every stage a same-shaped `DataArray` under one Numba backend element type.

[LOCAL_ADMISSION]:
- A raster terrain, hydrology, focal, zonal, or multispectral computation on a `DataArray`/`Dataset` grid admits here under GEOSPATIAL_TERRAIN_GATED; raster read/write, reprojection, and vector bridging admit at the `rioxarray`/`rasterio` owners.

[RAIL_LAW]:
- Package: `xarray-spatial`
- Owns: Numba raster analytics over `xarray` grids — surface and terrain metrics, classification breaks, focal/convolution neighborhood statistics, the d8/dinf/mfd hydrology stack, morphology, edge detection, interpolation/KDE, proximity/surface-distance/corridor fields, zonal reductions, multispectral indices, and A*/multi-stop pathfinding, with NumPy/Dask/CuPy dispatch
- Accept: raster terrain/DEM/hydrology analysis on `DataArray`/`Dataset` grids feeding the data, persistence, and visuals owners under GEOSPATIAL_TERRAIN_GATED
- Reject: wrapper-renames of `slope`/`aspect`/`hillshade`/`zonal.stats`/`flow_accumulation`; a hand-rolled finite-difference, Jenks, or flow-routing kernel where xarray-spatial owns it; GeoTIFF IO, reprojection, mosaic, or rasterize where the `rioxarray`/`rasterio` coverage rail owns it; a per-backend operator family where Numba dispatch handles NumPy/Dask/CuPy; a per-flow-model module where the `_d8`/`_dinf`/`_mfd` suffix discriminates; a parallel result class per operator
