# [RASM_PERSISTENCE_API_POSTGIS]

`postgis`, `postgis_raster`, and `postgis_sfcgal` own the PostgreSQL server-tier geospatial SQL surface — the `geometry`/`geography`/`raster` types, the `ST_*` construction, measurement, relationship, and overlay families over the built-in GiST index AM, and the `CG_*` exact-3D SFCGAL surface. Every surface is server-side SQL with no managed linkage: the `Store/provisioning#SERVER_EXTENSIONS` `ServerExtension` rows install it, and the `api-npgsql-nts` codec and `api-nts-ef` EF plugin reach it as `NetTopologySuite.Geometries.Geometry`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `postgis` + `postgis_raster` + `postgis_sfcgal`
- package: server-side PostgreSQL extension family (C/C++ over GEOS, PROJ, GDAL, SFCGAL; not a NuGet package); repo `postgis/postgis`
- namespace: SQL `public` (the `geometry`/`geography`/`raster` types, the `ST_*` and `CG_*` functions, the operator/opclass set)
- depends: `postgis` is the base; `postgis_raster` and `postgis_sfcgal` each `requires = postgis` — installed as their own `ServerExtension` rows
- license: GPL-2.0-or-later — the in-DB deployment is the license boundary, no managed linkage
- registration: preload-free — GiST/SP-GiST/BRIN operator classes over the built-in AMs, no custom access method and no `shared_preload_libraries` row; the EF `NpgsqlNetTopologySuiteExtensionAddingConvention` finalizes `CREATE EXTENSION postgis` on the model
- consumed by: the `api-npgsql-nts` `NetTopologySuiteTypeInfoResolverFactory` wire codec and the `api-nts-ef` `NpgsqlGeometryTypeMapping<TGeometry>` column mapping, the `Element/identity#ELEMENT_IDENTITY` footprint/boundary/envelope columns, and the `h3_postgis`/`pgrouting` extensions that `requires` it
- rail: geospatial-provisioning, spatial-store

## [02]-[SPATIAL_TYPES]

[PUBLIC_TYPE_SCOPE]: the geospatial store types. A `geometry` typmod carries the subtype token — `POINT`/`LINESTRING`/`POLYGON`/`MULTIPOINT`/`MULTILINESTRING`/`MULTIPOLYGON`/`GEOMETRYCOLLECTION`, the curved `CIRCULARSTRING`/`COMPOUNDCURVE`/`CURVEPOLYGON`, and the surface `POLYHEDRALSURFACE`/`TIN`/`TRIANGLE` — with a `Z`/`M`/`ZM` dimensionality suffix and an SRID (`geometry(PointZ, 4326)`).

| [INDEX] | [SURFACE]         | [SEMANTICS]                                                                               |
| :-----: | :---------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `geometry`        | planar OGC geometry; subtype+SRID via typmod; GiST/SP-GiST/BRIN indexable                 |
|  [02]   | `geography`       | geodetic type on the WGS84 spheroid; great-circle measurement, `gist_geography_ops`       |
|  [03]   | `box2d` / `box3d` | bounding-box types; `ST_Extent` agg → `box2d`, `ST_3DExtent` agg → `box3d`                |
|  [04]   | `geometry_dump`   | `(path integer[], geom geometry)` composite from `ST_Dump`/`ST_DumpPoints`/`ST_DumpRings` |
|  [05]   | `spheroid`        | `SPHEROID[...]` measurement datum for `ST_LengthSpheroid`/`ST_DistanceSpheroid`           |
|  [06]   | `raster`          | (`postgis_raster`) georeferenced multiband pixel grid; in-db or GDAL-backed out-db        |

## [03]-[GEOMETRY_IO]

[ENTRYPOINT_SCOPE]: construction and inspection.

| [INDEX] | [SIGNATURE]                                                  | [SEMANTICS]                                                      |
| :-----: | :----------------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `ST_MakePoint(x, y[, z, m])` → `geometry`                    | build a point; `ST_MakePointM` for XYM, `ST_Point(x,y,srid)` OGC |
|  [02]   | `ST_MakeLine(geometry[])` / `ST_MakePolygon(geometry)`       | line from points; polygon from a closed ring                     |
|  [03]   | `ST_MakeEnvelope(xmin, ymin, xmax, ymax, srid)` → `geometry` | rectangular polygon from bounds                                  |
|  [04]   | `ST_GeomFromText(text, srid)` / `ST_GeomFromEWKT(text)`      | WKT / EWKT parse                                                 |
|  [05]   | `ST_GeomFromWKB(bytea, srid)` / `ST_GeomFromGeoJSON(text)`   | WKB / GeoJSON parse                                              |
|  [06]   | `ST_Collect(geometry[])` → `geometry`                        | gather parts into a collection or multi-geometry                 |
|  [07]   | `ST_X` / `ST_Y` / `ST_Z` / `ST_M(geometry)` → `float8`       | point ordinate accessors                                         |
|  [08]   | `ST_SRID(geometry)` → `integer` / `ST_GeometryType` → `text` | spatial-reference id / `ST_*` type name                          |
|  [09]   | `ST_NumGeometries` / `ST_GeometryN(geometry, n)`             | collection arity / nth member                                    |
|  [10]   | `ST_NPoints` / `ST_PointN(geometry, n)` / `ST_ExteriorRing`  | vertex count / nth vertex / polygon outer ring                   |
|  [11]   | `ST_Dump(geometry)` → `SETOF geometry_dump`                  | explode a collection into component rows with a path             |
|  [12]   | `ST_IsValid` / `ST_IsSimple` / `ST_IsEmpty` / `ST_IsClosed`  | OGC validity predicates; `ST_IsValidReason` → `text`             |
|  [13]   | `ST_HasZ` / `ST_HasM(geometry)` → `boolean`                  | dimensionality flags                                             |

[ENTRYPOINT_SCOPE]: editing, reprojection, and output. `ST_Transform` reprojects through PROJ; `ST_SetSRID` only restamps the id.

| [INDEX] | [SIGNATURE]                                                     | [SEMANTICS]                               |
| :-----: | :-------------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `ST_SetSRID(geometry, srid)` / `ST_Transform(geometry, srid)`   | restamp SRID / reproject via PROJ         |
|  [02]   | `ST_Force2D` / `ST_Force3D` / `ST_ForceCollection(geometry)`    | dimensional / structural coercion         |
|  [03]   | `ST_FlipCoordinates` / `ST_Reverse(geometry)`                   | swap x/y ordinates / reverse vertex order |
|  [04]   | `ST_SnapToGrid(geometry, size)` / `ST_Snap(geometry, ref, tol)` | grid rounding / snap to a target geometry |
|  [05]   | `ST_Multi` / `ST_CollectionExtract(geometry, dim)`              | promote to multi / extract by dimension   |
|  [06]   | `ST_Subdivide(geometry, maxverts)` → `SETOF geometry`           | split into vertex-bounded parts           |
|  [07]   | `ST_Segmentize(geometry, maxlen)` → `geometry`                  | densify edges to a maximum segment length |
|  [08]   | `ST_AsText` / `ST_AsEWKT(geometry)` → `text`                    | WKT / SRID-tagged EWKT                    |
|  [09]   | `ST_AsBinary` / `ST_AsEWKB(geometry)` → `bytea`                 | WKB / EWKB                                |
|  [10]   | `ST_AsGeoJSON(geometry[, maxdecimals])` → `text`                | RFC 7946 GeoJSON                          |
|  [11]   | `ST_AsGML` / `ST_AsKML` / `ST_AsSVG(geometry)` → `text`         | GML / KML / SVG                           |
|  [12]   | `ST_GeoHash(geometry[, precision])` → `text`                    | geohash of a lon/lat point or bbox        |

## [04]-[RELATIONSHIPS]

[ENTRYPOINT_SCOPE]: measurement. `geography` overloads measure on the spheroid; `ST_DistanceSphere` is the fast spherical approximation, `ST_DistanceSpheroid` the exact form.

| [INDEX] | [SIGNATURE]                                                            | [SEMANTICS]                                |
| :-----: | :--------------------------------------------------------------------- | :----------------------------------------- |
|  [01]   | `ST_Area(geometry)` / `ST_Area(geography)` → `float8`                  | planar / geodetic area                     |
|  [02]   | `ST_Length` / `ST_Perimeter(geometry)` → `float8`                      | curve length / boundary length             |
|  [03]   | `ST_Distance(geometry, geometry)` / `(geography, geography, spheroid)` | minimum distance; geodesic on the spheroid |
|  [04]   | `ST_DistanceSphere` / `ST_DistanceSpheroid(geometry, geometry)`        | fast sphere / exact spheroid distance      |
|  [05]   | `ST_3DDistance(geometry, geometry)` → `float8`                         | shortest 3D cartesian distance             |
|  [06]   | `ST_ClosestPoint` / `ST_ShortestLine(geometry, geometry)` → `geometry` | nearest point / connecting line            |
|  [07]   | `ST_Azimuth(geometry, geometry)` → `float8`                            | north-based bearing between two points     |
|  [08]   | `ST_HausdorffDistance` / `ST_FrechetDistance(geometry, geometry)`      | curve-similarity distances                 |

[ENTRYPOINT_SCOPE]: DE-9IM spatial predicates. `ST_Intersects`/`ST_DWithin`/`ST_Contains` gain a `&&` bbox prefilter from a GiST index; the others test the exact relationship.

| [INDEX] | [SIGNATURE]                                                     | [SEMANTICS]                                |
| :-----: | :-------------------------------------------------------------- | :----------------------------------------- |
|  [01]   | `ST_Intersects(geometry, geometry)` → `boolean`                 | share any point; `&&`-index-accelerated    |
|  [02]   | `ST_Contains` / `ST_Within` / `ST_Covers` / `ST_CoveredBy`      | containment DE-9IM predicates              |
|  [03]   | `ST_Overlaps` / `ST_Touches` / `ST_Crosses` / `ST_Disjoint`     | DE-9IM relationship predicates             |
|  [04]   | `ST_Equals(geometry, geometry)` → `boolean`                     | spatial equality, vertex order ignored     |
|  [05]   | `ST_DWithin(geometry, geometry, distance)` → `boolean`          | within a distance; index-accelerated       |
|  [06]   | `ST_DFullyWithin` / `ST_3DDWithin` / `ST_3DIntersects`          | fully-within / 3D proximity / 3D intersect |
|  [07]   | `ST_Relate(geometry, geometry[, pattern])` → `text` / `boolean` | DE-9IM matrix / intersection-pattern match |
|  [08]   | `ST_OrderingEquals(geometry, geometry)` → `boolean`             | identical vertex order and values          |

## [05]-[PROCESSING]

[ENTRYPOINT_SCOPE]: geometry-producing overlay and derivation. `ST_Buffer` carries a `quad_segs`/`endcap`/`join` parameter string and a `geography` overload.

| [INDEX] | [SIGNATURE]                                                               | [SEMANTICS]                                       |
| :-----: | :------------------------------------------------------------------------ | :------------------------------------------------ |
|  [01]   | `ST_Intersection` / `ST_Union` / `ST_Difference` / `ST_SymDifference`     | pairwise set-overlay booleans → `geometry`        |
|  [02]   | `ST_Buffer(geometry, radius[, params])` → `geometry`                      | dilation; cap/segment/join policy, geography form |
|  [03]   | `ST_ConvexHull` / `ST_ConcaveHull(geometry, ratio)` → `geometry`          | convex / concave hull                             |
|  [04]   | `ST_Simplify` / `ST_SimplifyPreserveTopology(geometry, tol)`              | Douglas-Peucker; topology-safe variant            |
|  [05]   | `ST_Centroid` / `ST_PointOnSurface(geometry)` → `geometry`                | centroid / guaranteed-interior point              |
|  [06]   | `ST_Boundary` / `ST_Envelope` / `ST_OrientedEnvelope(geometry)`           | boundary / bbox / minimum-area rectangle          |
|  [07]   | `ST_Node` / `ST_Split(geometry, blade)` / `ST_ClipByBox2D(geometry, box)` | noding / splitting / fast box clip                |
|  [08]   | `ST_VoronoiPolygons` / `ST_DelaunayTriangles(geometry)` → `geometry`      | Voronoi / Delaunay tessellation                   |
|  [09]   | `ST_MinimumBoundingCircle` / `ST_GeneratePoints(geometry, n)`             | bounding circle / random interior points          |

[ENTRYPOINT_SCOPE]: aggregates, window clustering, and tile output. `ST_ClusterDBSCAN`/`ST_ClusterKMeans` are window functions returning a per-row cluster id; `ST_AsMVT` aggregates a row set to one tile.

| [INDEX] | [SIGNATURE]                                                                | [SEMANTICS]                                         |
| :-----: | :------------------------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `ST_Union(geometry)` agg → `geometry`                                      | dissolve a geometry column into one                 |
|  [02]   | `ST_Collect(geometry)` agg / `ST_Extent(geometry)` agg → `box2d`           | gather column / bounding extent                     |
|  [03]   | `ST_MakeLine(geometry)` agg / `ST_Polygonize(geometry)` agg                | ordered line / noded-line polygonization            |
|  [04]   | `ST_ClusterDBSCAN(geometry, eps, minpoints)` window → `integer`            | density cluster id per row                          |
|  [05]   | `ST_ClusterKMeans(geometry, k)` window → `integer`                         | k-means cluster id per row                          |
|  [06]   | `ST_ClusterIntersecting` / `ST_ClusterWithin(geometry)` agg → `geometry[]` | connected-component clustering                      |
|  [07]   | `ST_AsMVTGeom(geometry, box2d, ...)` / `ST_AsMVT(anyelement)` agg          | clip to tile pixel space / Mapbox Vector Tile bytea |

## [06]-[RASTER]

[ENTRYPOINT_SCOPE]: `postgis_raster` — a `raster` is a georeferenced multiband pixel grid stored in-db or out-db (GDAL). `raster2pgsql` loads source rasters; out-db access is GUC-gated.

| [INDEX] | [SIGNATURE]                                                        | [SEMANTICS]                             |
| :-----: | :----------------------------------------------------------------- | :-------------------------------------- |
|  [01]   | `ST_MakeEmptyRaster(w, h, ulx, uly, scale)` → `raster`             | allocate a georeferenced grid           |
|  [02]   | `ST_AddBand(raster, ...)` / `ST_Band(raster, n)` → `raster`        | add / extract a band                    |
|  [03]   | `ST_Value(raster, band, x, y)` → `float8`                          | pixel value at a point or column/row    |
|  [04]   | `ST_SetValue(raster, band, geometry, val)` → `raster`              | write pixels under a geometry           |
|  [05]   | `ST_MapAlgebra(raster, ...)` → `raster`                            | per-pixel expression / callback algebra |
|  [06]   | `ST_Reclass` / `ST_Resample` / `ST_Resize(raster, ...)` → `raster` | reclassify / resample / resize          |
|  [07]   | `ST_Clip(raster, geometry[, touched])` → `raster`                  | mask a raster to a geometry             |
|  [08]   | `ST_AsRaster(geometry, ...)` → `raster`                            | rasterize a vector geometry             |
|  [09]   | `ST_Intersection(raster, geometry)` → `SETOF record`               | raster × geometry to `(geom, val)` rows |
|  [10]   | `ST_DumpAsPolygons(raster)` / `ST_Polygon(raster)` → `geometry`    | vectorize bands to polygons             |
|  [11]   | `ST_SummaryStats` / `ST_Histogram` / `ST_ValueCount(raster)`       | per-band statistics                     |
|  [12]   | `ST_Tile(raster, w, h)` → `SETOF raster`                           | split into fixed-size tiles             |

## [07]-[SFCGAL_3D]

[ENTRYPOINT_SCOPE]: `postgis_sfcgal` — exact 3D and solid modeling over the SFCGAL backend; every function is called through its `CG_` name. `ST_ExtrudeStraightSkeleton` is the one solid-roof entry that keeps the `ST_` prefix.

| [INDEX] | [SIGNATURE]                                                              | [SEMANTICS]                                  |
| :-----: | :----------------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `CG_3DIntersection` / `CG_3DDifference` / `CG_Union(geometry, geometry)` | exact 3D boolean overlay                     |
|  [02]   | `CG_3DIntersects` / `CG_Intersects(geometry, geometry)` → `boolean`      | exact 3D / 2D intersection test              |
|  [03]   | `CG_Extrude(geometry, x, y, z)` → `geometry`                             | extrude a surface to a solid                 |
|  [04]   | `CG_StraightSkeleton(geometry[, use_distance_as_m])` → `geometry`        | straight skeleton                            |
|  [05]   | `ST_ExtrudeStraightSkeleton(geometry[, roof, body])` → `geometry`        | roof / building solid from the skeleton      |
|  [06]   | `CG_ApproximateMedialAxis` / `CG_MinkowskiSum(geometry, ...)`            | medial-axis approximation / 2D Minkowski sum |
|  [07]   | `CG_Tesselate` / `CG_Triangulate(geometry)` → `geometry`                 | constrained triangulation / TIN              |
|  [08]   | `CG_3DArea` / `CG_Volume(geometry)` → `float8`                           | surface area / solid volume                  |
|  [09]   | `CG_MakeSolid` / `CG_IsSolid(geometry)` → `geometry` / `boolean`         | promote to / test a solid                    |
|  [10]   | `CG_3DConvexHull(geometry)` → `geometry`                                 | 3D convex hull                               |
|  [11]   | `CG_AlphaShape` / `CG_OptimalAlphaShape(geometry)` → `geometry`          | alpha-shape concave boundary                 |
|  [12]   | `CG_ApproxConvexPartition` / `CG_OptimalConvexPartition(geometry)`       | convex partitioning                          |
|  [13]   | `CG_Visibility(geometry, ...)` → `geometry`                              | visibility polygon                           |

## [08]-[INDEX_SUPPORT]

[ENTRYPOINT_SCOPE]: PostGIS ships operator classes for the built-in GiST/SP-GiST/BRIN access methods over `geometry`/`geography` — no custom AM. A bare `USING gist (geom)` picks the default `gist_geometry_ops_2d`; an N-D or `geography` index names its opclass. `geom <-> geom` (2D) and box-KNN `geom <#> geom` ride the GiST opclass for index-ordered nearest-neighbour; the bbox operators `&&` (2D) and `&&&` (nD) drive `ST_Intersects`/`ST_DWithin` pushdown.

| [INDEX] | [OPCLASS]                                | [AM]   | [DEFAULT] | [SEMANTICS]                                 |
| :-----: | :--------------------------------------- | :----- | :-------- | :------------------------------------------ |
|  [01]   | `gist_geometry_ops_2d`                   | gist   | yes       | 2D R-tree over bbox; `&&`/`~`/`@`/`<->` KNN |
|  [02]   | `gist_geometry_ops_nd`                   | gist   | no        | N-D R-tree; `&&&` nD bbox overlap           |
|  [03]   | `gist_geography_ops`                     | gist   | yes       | geography 2D R-tree on the sphere           |
|  [04]   | `spgist_geometry_ops_2d`                 | spgist | no        | 2D quad-tree partitioning                   |
|  [05]   | `spgist_geometry_ops_3d` / `_nd`         | spgist | no        | 3D / N-D SP-GiST                            |
|  [06]   | `brin_geometry_inclusion_ops_2d`         | brin   | no        | block-range 2D bbox inclusion               |
|  [07]   | `brin_geometry_inclusion_ops_3d` / `_4d` | brin   | no        | 3D / 4D BRIN inclusion                      |

## [09]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- No managed assembly, built-in AMs: `postgis` registers types, functions, operators, and GiST/SP-GiST/BRIN operator classes (no custom access method), so the family is absent from the `Store/provisioning#SERVER_EXTENSIONS` `shared_preload_libraries` row. `postgis` is the base; `postgis_raster` and `postgis_sfcgal` each `requires = postgis` and install as their own `ServerExtension` rows; the EF `NpgsqlNetTopologySuiteExtensionAddingConvention` finalizes `CREATE EXTENSION postgis` on the model.
- Out-db raster gate: `postgis_raster` out-of-database rasters bind only under `postgis.enable_outdb_rasters = true` and the `postgis.gdal_enabled_drivers` GUC; in-db rasters need neither, and the `raster2pgsql` loader owns ingest.
- SFCGAL is the CG_ surface: pin the `CG_` spelling at every `postgis_sfcgal` call site, and the SFCGAL backend is required at install.

[STACKING]:
- `api-npgsql-nts`(`.api/api-npgsql-nts.md`): the `geometry`/`geography` values these functions return round-trip as `NetTopologySuite.Geometries.Geometry` through the `NetTopologySuiteTypeInfoResolverFactory` codec `UseNetTopologySuite` admits on the `NpgsqlDataSource` — PostGIS owns the server-side computation, the codec owns the binary EWKB wire.
- `api-nts-ef`(`.api/api-nts-ef.md`): `NpgsqlGeometryTypeMapping<TGeometry>` maps the `geometry`/`geography` columns with per-property SRID, and the member/method translators lower a LINQ subset to these functions (`.Distance`→`ST_Distance`, `.Intersects`→`ST_Intersects`, `.Buffer`→`ST_Buffer`, `.Area`→`ST_Area`); `EF.Functions.Distance`/`IsWithinDistance`/`DistanceKnn`/`Transform`/`Force2D` project `ST_Distance`/`ST_DWithin`/`<->`/`ST_Transform`/`ST_Force2D`.
- within-lib: the `Store/provisioning#SERVER_EXTENSIONS` `ServerExtension` rows install the family; the geometry/geography columns the `Element/identity#ELEMENT_IDENTITY` spatial tier carries are GiST-indexed on `gist_geometry_ops_2d`, so the `<->` KNN order-by is the spatial sibling of the pgvector ANN order-by the `Query/retrieval#FUSION_AND_REUSE` fold ranks; `h3_postgis` (`api-h3-pg.md`) and `pgrouting` (`api-pgrouting.md`) both `requires` this extension.

[LOCAL_ADMISSION]:
- Spatial capability enters only through the PostgreSQL store profile: the `ServerExtension` rows for `postgis`/`postgis_raster`/`postgis_sfcgal` and `UseNetTopologySuite` on the EF options AND the ADO data source, paired. Per-property SRID is mapping policy; geometry values use `NetTopologySuite` types, never WKT strings or raw `bytea` columns.

[RAIL_LAW]:
- Package: `postgis` + `postgis_raster` + `postgis_sfcgal` (server-side, in the deploy-image PG18)
- Owns: the in-PG geospatial SQL surface — the `geometry`/`geography`/`raster` types, the `ST_*` construction/measurement/relationship/overlay/aggregate functions, the `CG_*` exact-3D SFCGAL surface, and the GiST/SP-GiST/BRIN operator classes
- Accept: `CREATE EXTENSION postgis`/`postgis_raster`/`postgis_sfcgal`, `NetTopologySuite`-typed geometry through the `Npgsql.NetTopologySuite` codec, EF member/method/`EF.Functions` translation to `ST_*`, a `gist_geometry_ops_2d` index behind `ST_Intersects`/`ST_DWithin`/`<->`, the `CG_` SFCGAL spelling, in-db or GUC-gated out-db rasters
- Reject: linking PostGIS into managed code, a WKT string or raw `bytea` column standing for a geometry contract, a client-evaluated spatial predicate, a per-row scan where a `gist` bbox index serves, placing the family on the `shared_preload_libraries` row
