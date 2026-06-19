# [PY_DATA_API_SHAPELY]

`shapely` supplies GEOS-backed planar geometry: construction, predicates, set operations, constructive transforms, measurement, and spatial indexing. It provides the `Geometry` hierarchy (`Point`, `LineString`, `LinearRing`, `Polygon`, `MultiPoint`, `MultiLineString`, `MultiPolygon`, `GeometryCollection`) plus a NumPy-vectorized top-level function namespace, with `STRtree` as the bulk spatial index.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `shapely`
- package: `shapely`
- import: `import shapely`
- owner: `data`
- rail: geospatial
- capability: planar geometry construction, vectorized binary/unary predicates, set and constructive operations, measurement, linear referencing, WKT/WKB/GeoJSON/ragged-array interchange, STRtree indexing, and affine transforms

## [02]-[CAPTURE]

[PUBLIC_TYPES]:
- `shapely.Geometry` — abstract base; instance properties `area`, `length`, `bounds`, `centroid`, `geom_type`, `is_empty`, `is_valid`, `has_z`, `has_m`, `coords`, `xy`, `wkt`, `wkb`.
- `shapely.Point`, `shapely.LineString`, `shapely.LinearRing`, `shapely.Polygon` — single geometries; `Point` adds `x`, `y`, `z`, `m`.
- `shapely.MultiPoint`, `shapely.MultiLineString`, `shapely.MultiPolygon`, `shapely.GeometryCollection` — collections; iterate via `geoms`.
- `shapely.STRtree` — packed Sorted-Tile-Recursive index over a geometry array; bulk `query`, `query_nearest`, `nearest`.
- `shapely.GeometryType` — enum of geometry type ids; `shapely.BufferCapStyle`, `shapely.BufferJoinStyle` — buffer styling enums.
- `shapely.box(xmin, ymin, xmax, ymax, ccw=True)` — rectangle constructor; `shapely.geometry.CAP_STYLE`, `JOIN_STYLE` — legacy style aliases.
- `shapely.affinity` — affine transform functions: `affine_transform`, `rotate`, `scale`, `skew`, `translate`.

[ENTRYPOINTS]:
- vectorized creation: `points(...)`, `linestrings(...)`, `linearrings(...)`, `polygons(...)`, `multipoints(...)`, `multilinestrings(...)`, `multipolygons(...)`, `geometrycollections(...)`, `box(xmin, ymin, xmax, ymax)`.
- binary predicates: `intersects(a, b)`, `contains(a, b)`, `contains_properly(a, b)`, `within(a, b)`, `covers(a, b)`, `covered_by(a, b)`, `crosses(a, b)`, `overlaps(a, b)`, `touches(a, b)`, `disjoint(a, b)`, `equals(a, b)`, `equals_exact(a, b, tolerance)`, `dwithin(a, b, distance)`, `relate(a, b)`, `relate_pattern(a, b, pattern)`, `contains_xy(geom, x, y)`, `intersects_xy(geom, x, y)`.
- unary predicates: `is_valid(g)`, `is_empty(g)`, `is_simple(g)`, `is_ring(g)`, `is_closed(g)`, `is_ccw(g)`, `has_z(g)`, `has_m(g)`, `is_valid_reason(g)`.
- set operations: `intersection(a, b)`, `union(a, b)`, `union_all(geoms, grid_size=None)`, `difference(a, b)`, `symmetric_difference(a, b)`, `coverage_union(a, b)`, `coverage_union_all(geoms)`, `intersection_all(geoms)`, `disjoint_subset_union(a, b)`.
- constructive: `buffer(g, distance, quad_segs=8, cap_style='round', join_style='round', mitre_limit=5.0, single_sided=False)`, `convex_hull(g)`, `concave_hull(g, ratio=0.0, allow_holes=False)`, `envelope(g)`, `oriented_envelope(g)`, `minimum_rotated_rectangle(g)`, `simplify(g, tolerance, preserve_topology=True)`, `make_valid(g)`, `normalize(g)`, `reverse(g)`, `segmentize(g, max_segment_length)`, `offset_curve(g, distance, ...)`, `delaunay_triangles(g, tolerance=0.0, only_edges=False)`, `voronoi_polygons(g, tolerance=0.0, ...)`, `polygonize(geoms)`, `polygonize_full(geoms)`, `node(g)`, `snap(g, ref, tolerance)`, `set_precision(g, grid_size)`, `force_2d(g)`, `force_3d(g)`, `remove_repeated_points(g, tolerance=0.0)`, `minimum_bounding_circle(g)`.
- measurement: `area(g)`, `length(g)`, `distance(a, b)`, `bounds(g)`, `total_bounds(geoms)`, `hausdorff_distance(a, b)`, `frechet_distance(a, b)`, `minimum_clearance(g)`, `minimum_bounding_radius(g)`, `shortest_line(a, b)`.
- linear referencing: `line_interpolate_point(line, distance, normalized=False)`, `line_locate_point(line, point, normalized=False)`, `line_merge(g)`, `shared_paths(a, b)`.
- coordinate access: `get_coordinates(g, include_z=False, return_index=False)`, `set_coordinates(g, coords)`, `count_coordinates(g)`, `transform(g, transformation, include_z=False)`, `get_x(g)`, `get_y(g)`, `get_z(g)`, `get_parts(g)`, `get_rings(g)`, `get_num_geometries(g)`, `get_geometry(g, index)`.
- interchange: `from_wkt(s)`, `to_wkt(g, rounding_precision=6, ...)`, `from_wkb(b)`, `to_wkb(g, hex=False, output_dimension=3, ...)`, `from_geojson(s)`, `to_geojson(g, indent=None)`, `from_ragged_array(geometry_type, coords, offsets=None)`, `to_ragged_array(geoms)`.
- prepared geometry: `prepare(g)`, `destroy_prepared(g)`, `is_prepared(g)`.
- STRtree: `STRtree(geoms, node_capacity=10)`, `tree.query(geometry, predicate=None, distance=None)`, `tree.query_nearest(geometry, max_distance=None, return_distance=False, ...)`, `tree.nearest(geometry)`.

[EXCEPTIONS]:
- `shapely.GEOSException` — GEOS-level geometry operation failure (also `shapely.errors.GEOSException`).
- `shapely.errors.ShapelyError` — base error type.
- `shapely.errors.GeometryTypeError` — operation applied to an incompatible geometry type.
- `shapely.errors.DimensionError` — coordinate dimension mismatch.
- `shapely.errors.TopologicalError` — invalid topology during an operation.

[IMPLEMENTATION_LAW]:
- Geometries are immutable; constructive operations return new geometries rather than mutating in place.
- The top-level functions are NumPy-vectorized ufuncs accepting scalar geometries or geometry arrays and broadcasting elementwise; prefer the vectorized array form over Python loops over `.intersects`/`.buffer` instance methods.
- `prepare(g)` builds a cached spatial index inside the geometry, accelerating repeated predicate calls against many candidates; prepare the static side before a predicate loop.
- `STRtree.query` returns integer indices into the input geometry array, not geometries; pass a `predicate` to filter bbox hits to true topological matches in one call.
- `union_all` and `coverage_union_all` dissolve a geometry set in one GEOS call; `coverage_union_all` requires non-overlapping (coverage) input and is faster than general `union_all`.
- Floating-point robustness uses `set_precision` with a `grid_size`; snapping to a grid before set operations avoids sliver and noding failures.
- `from_ragged_array`/`to_ragged_array` are the zero-copy bridge to flat coordinate plus offset arrays for Arrow and GeoArrow interoperability.
- `affinity` functions operate on single geometries with explicit transform matrices; `transform` applies an arbitrary coordinate function across a geometry.

## [03]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `shapely`
- Owns: planar geometry construction, vectorized predicates and set/constructive operations, measurement, linear referencing, STRtree indexing, WKT/WKB/GeoJSON/ragged-array interchange, affine transforms
- Accept: vectorized top-level functions over geometry arrays, `prepare` before repeated predicates, `STRtree.query` with a predicate for spatial joins, `set_precision` for robust set operations, ragged-array interchange for Arrow/GeoArrow
- Reject: Python loops over instance predicate methods where the vectorized form applies, hand-rolled point-in-polygon or distance math, mutable geometry assumptions, and bbox-only spatial joins that skip the topological predicate
