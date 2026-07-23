# [PY_DATA_API_SHAPELY]

`shapely` owns GEOS-backed planar geometry for the data geospatial rail: an immutable `Geometry` hierarchy and a NumPy-vectorized top-level namespace where every predicate, measurement, set, and constructive op is a broadcasting ufunc over scalar geometries or geometry arrays. `STRtree` bulk-indexes with a predicate-filtered `query`, `prepare` accelerates repeated predicates, and `from_ragged_array`/`to_ragged_array` are the zero-copy GeoArrow bridge into the `pyarrow`/`geopandas`/`polars-st` siblings.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `shapely`
- package: `shapely` (BSD-3-Clause; bundled GEOS LGPL-2.1, dynamic-linked at the shared-library boundary)
- module: `import shapely`
- owner: `data`
- rail: geospatial
- asset: native GEOS C-API extension built against the Parametric_Forge GEOS toolchain; `shapely.geos_version` reports the linked core

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: immutable geometry hierarchy, spatial index, and styling enums

| [INDEX] | [SYMBOL]                                                           | [TYPE_FAMILY] | [CAPABILITY]               |
| :-----: | :----------------------------------------------------------------- | :------------ | :------------------------- |
|  [01]   | `Geometry`                                                         | abstract base | immutable geometry base    |
|  [02]   | `Point` `LineString` `LinearRing` `Polygon`                        | single        | single geometries          |
|  [03]   | `MultiPoint` `MultiLineString` `MultiPolygon` `GeometryCollection` | collection    | multipart; iterate `geoms` |
|  [04]   | `STRtree`                                                          | spatial index | packed STR index           |
|  [05]   | `GeometryType`                                                     | enum          | type-id `IntEnum`          |
|  [06]   | `BufferCapStyle` `BufferJoinStyle`                                 | enum          | buffer styling enums       |

- `Geometry` exposes `area` `length` `bounds` `centroid` `is_empty` `is_valid` `has_z` `has_m` `coords` `xy` `wkt` `wkb` `geom_type`; `Point` adds `x`/`y`/`z`/`m`, `Polygon` exposes `exterior`/`interiors`.
- `BufferCapStyle` is `round`/`flat`/`square`, `BufferJoinStyle` `round`/`mitre`/`bevel`; `GeometryType` pairs with the integer `get_type_id(g)`.

[ERRORS]: `GEOSException` (GEOS op failure) `ShapelyError` (base) `GeometryTypeError` `DimensionError` `TopologicalError` — under `shapely.errors`, `GEOSException` re-exported top-level.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: top-level GEOS ufuncs, `STRtree` indexing, prepared geometry, and WKT/WKB/GeoJSON/ragged-array interchange

[VECTORIZED_OPS]: GEOS ufuncs broadcasting over scalar geometries or geometry arrays, returning new immutable geometries or aligned arrays
- creation: `points` `linestrings` `linearrings` `polygons` `multipoints` `multilinestrings` `multipolygons` `geometrycollections` `box(xmin,ymin,xmax,ymax,ccw)` `empty(geom_type)`
- binary predicates: `intersects` `contains` `contains_properly` `within` `covers` `covered_by` `crosses` `overlaps` `touches` `disjoint` `equals` `equals_exact(a,b,tolerance,normalize)` `equals_identical` `dwithin(a,b,distance)` `relate` `relate_pattern(a,b,pattern)` `contains_xy(g,x,y)` `intersects_xy(g,x,y)`
- unary predicates: `is_valid` `is_valid_reason` `is_empty` `is_simple` `is_ring` `is_closed` `is_ccw` `has_z` `has_m` `is_geometry` `is_missing` `is_valid_input`
- set operations (each carries `grid_size`): `intersection` `union` `union_all(geometries,grid_size,axis)` `difference` `symmetric_difference` `symmetric_difference_all` `intersection_all` `coverage_union` `coverage_union_all` `disjoint_subset_union` `disjoint_subset_union_all` `clip_by_rect`
- constructive: `buffer(g,distance,quad_segs,cap_style,join_style,mitre_limit,single_sided)` `offset_curve` `convex_hull` `concave_hull(g,ratio,allow_holes)` `envelope` `oriented_envelope` `minimum_rotated_rectangle` `boundary` `centroid` `point_on_surface` `maximum_inscribed_circle` `minimum_bounding_circle` `simplify(g,tolerance,preserve_topology)` `make_valid(g,*,method,keep_collapsed)` `normalize` `orient_polygons` `reverse` `segmentize(g,max_segment_length)` `delaunay_triangles` `constrained_delaunay_triangles` `voronoi_polygons` `polygonize` `polygonize_full` `build_area` `node` `snap(g,reference,tolerance)` `set_precision(g,grid_size,mode)` `coverage_simplify` `coverage_is_valid` `coverage_invalid_edges` `force_2d` `force_3d` `remove_repeated_points` `extract_unique_points`
- measurement: `area` `length` `distance` `bounds` `total_bounds` `hausdorff_distance(a,b,densify)` `frechet_distance(a,b,densify)` `minimum_clearance` `minimum_clearance_line` `minimum_bounding_radius` `shortest_line`
- linear referencing: `line_interpolate_point(line,distance,normalized)` `line_locate_point(line,point,normalized)` `line_merge(g,directed)` `shared_paths`
- coordinate access: `get_coordinates(g,include_z,include_m,return_index)` `set_coordinates` `count_coordinates` `transform(g,transformation,*,interleaved)` `get_x` `get_y` `get_z` `get_m` `get_parts` `get_rings` `get_num_geometries` `get_geometry` `get_exterior_ring` `get_interior_ring` `get_num_interior_rings` `get_point` `get_num_points` `get_dimensions` `get_coordinate_dimension` `get_precision` `get_srid` `set_srid`
- STRtree: `STRtree(geoms,node_capacity)` `tree.query(geom,predicate,distance)` `tree.query_nearest(geom,max_distance,return_distance,exclusive,all_matches)` `tree.nearest(geom)` `tree.geometries` — predicate vocab `intersects`/`within`/`contains`/`overlaps`/`crosses`/`touches`/`covers`/`covered_by`/`contains_properly`/`dwithin`
- prepared geometry: `prepare(g)` `destroy_prepared(g)` `is_prepared(g)`
- interchange: `from_wkt` `to_wkt(g,rounding_precision,trim,output_dimension)` `from_wkb` `to_wkb(g,hex,output_dimension,byte_order,include_srid,flavor)` `from_geojson` `to_geojson(g,indent)` `from_ragged_array(geometry_type,coords,offsets)` `to_ragged_array(geometries,*,include_z,include_m)`

[MATRIX_TRANSFORMS] (`shapely.affinity`, single-geometry): `affine_transform(g,matrix)` `rotate(g,angle,origin,use_radians)` `scale(g,xfact,yfact,zfact,origin)` `skew(g,xs,ys,origin,use_radians)` `translate(g,xoff,yoff,zoff)`

[COLLECTION_OPS] (`shapely.ops`, no top-level vectorized mirror): `split(geom,splitter)` `substring(geom,start_dist,end_dist,normalized)` `nearest_points` `polylabel(polygon,tolerance)` `triangulate(geom,tolerance,edges)` `voronoi_diagram(geom,envelope,tolerance,edges)` `linemerge(lines,directed)` `unary_union` `snap` `shared_paths` `clip_by_rect` `transform(func,geom)` `prep` `orient(polygon,sign)`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Geometries are immutable; constructive ops return new geometries. Top-level functions broadcast over scalar geometries or geometry arrays — drive the array form, never a Python loop over instance `.intersects`/`.buffer`.
- `prepare(g)` builds a cached in-geometry index accelerating repeated predicates against many candidates; prepare the static side before a predicate sweep. `shapely.ops.prep(g)` returns the same effect as a `PreparedGeometry` wrapper.
- `STRtree.query` returns integer indices into the input array (a `(2,n)` array of `(input,tree)` pairs for an array input), never geometries; `predicate=` collapses the bbox hit and topological refine into one call, and `query_nearest(exclusive=True)` drops self-matches in a self-join.
- `coverage_union_all` dissolves a non-overlapping (coverage) set in one GEOS call, faster than general `union_all`; `coverage_is_valid`/`coverage_invalid_edges` gate the precondition.
- `set_precision(g, grid_size)` grid-snaps before set operations to avoid sliver and noding failures; `make_valid` repairs invalid input.
- `shapely.ops` owns the collection operators with no top-level vectorized mirror — route those through `ops`, everything with a vectorized form through the top-level namespace.

[STACKING]:
- `geopandas`(`.api/geopandas.md`): a `GeoSeries` wraps a shapely `GeometryArray`; `.values` exposes the raw array for a direct top-level call and `STRtree` backs `.sindex` — never a per-row shapely import.
- `pyproj`(`.api/pyproj.md`): geometry is CRS-free; feed a `Transformer.transform` to `shapely.transform(g, fn)` for coordinate reprojection, and `get_srid`/`set_srid` tag the SRID without reprojecting.
- `pyarrow`(`.api/pyarrow.md`): `to_ragged_array` emits GeoArrow-native coord-plus-offset arrays feeding the `pyarrow`/`stac-geoparquet`(`.api/stac-geoparquet.md`) GeoParquet writers — never a per-row WKB round-trip.
- `h3ronpy`(`.api/h3ronpy.md`): `to_wkb` and coordinate arrays feed `wkb_to_cells`/`coordinates_to_cells` for DGGS indexing; `xarray-spatial`(`.api/xarray-spatial.md`) consumes the geometries for raster ops.
- within-lib: the data geospatial owner drives the vectorized array form and runs `STRtree.query(geom, predicate=...)` as the spatial-join primitive directly into the feature table.

[LOCAL_ADMISSION]:
- Admit `shapely` as the planar-geometry and GEOS-ufunc owner on the data geospatial rail, composing the vectorized array form and `STRtree` rather than looping instance predicate methods.

[RAIL_LAW]:
- Package: `shapely`
- Owns: immutable planar geometry construction; NumPy-vectorized predicates, set/constructive ops, and measurement; linear referencing; coverage operations; `STRtree` predicate-filtered bulk indexing; `prepare` acceleration; WKT/WKB/GeoJSON/ragged-array interchange; affine transforms; the `shapely.ops` collection operators
- Accept: vectorized top-level functions over geometry arrays; `prepare` before repeated predicates; `STRtree.query(predicate=...)` for spatial joins and `query_nearest` for nearest joins; `set_precision`/`make_valid` for robust set operations; `coverage_union_all` under a `coverage_is_valid` guard; `from_ragged_array`/`to_ragged_array` for Arrow/GeoArrow interchange; `shapely.transform` fed by a `pyproj` transformer for reprojection
- Reject: Python loops over instance predicates where the vectorized form applies; hand-rolled point-in-polygon, distance, or pole-of-inaccessibility math; bbox-only joins skipping the topological `predicate`; per-row WKB round-trips where the ragged-array seam applies; re-implementing CRS reprojection that `pyproj` owns
