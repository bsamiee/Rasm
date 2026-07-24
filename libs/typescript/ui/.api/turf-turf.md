# [TS_UI_API_TURF_TURF]

`@turf/turf` is the browser-side planar and spherical GeoJSON geometry engine — the JS peer of C# NetTopologySuite meeting it at the WKB/GeoJSON wire: DE-9IM predicates, overlay, measurement, and construction as pure `(geojson, options) => geojson | scalar` transforms over a parameterized constructor/traversal/accessor substrate. `viewer/geo` composes it over `wire`-decoded features, never hand-looping coordinates; `scope:viewer` project-local.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@turf/turf`
- package: `@turf/turf` (MIT)
- deps: the `@turf/*` op modules + `@types/geojson` value types; `tslib`
- runtime: `scope:viewer` project-local — admitted only by the `ui/viewer` Nx project, compile-excluded from the non-spatial core
- asset: self-typed ESM+CJS, `sideEffects: false` — tree-shakeable, so the umbrella import prunes to used ops and a hot module imports `@turf/<op>` directly
- entry: the `.` barrel re-exports every op and the `helpers`/`meta`/`invariant`/`clusters`/`random`/`projection` namespaces; pure sync functions — no runtime, no effect rail, no DOM
- rail: viewer/geo, viewer/mark — the browser GeoJSON geometry engine

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: turf's flexible-input, unit, and grid-option types

`@types/geojson` (`.api/types-geojson.md`) owns the RFC 7946 value vocabulary turf re-exports — the `Geometry` union, the `Feature`/`FeatureCollection`/`GeoJSON<G, P>` generics, `Position`/`BBox`, and the `GeoJsonTypes`/`GeoJsonGeometryTypes` discriminants; turf's own additive types normalize the coordinate input and bound the measurement and grid vocabularies.

| [INDEX] | [SYMBOL]                                                                        | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------------------------ | :------------ | :----------------------------------- |
|  [01]   | `Coord` = `Feature<Point> \| Point \| Position`                                 | union         | `getCoord`-normalized coord input    |
|  [02]   | `AllGeoJSON` = `Feature \| FeatureCollection \| Geometry \| GeometryCollection` | union         | any-input arg to `meta` folds        |
|  [03]   | `Lines` = `LineString \| MultiLineString \| Polygon \| MultiPolygon`            | union         | line-family arg for line ops         |
|  [04]   | `Units`                                                                         | enum          | distance and length unit vocab       |
|  [05]   | `AreaUnits`                                                                     | enum          | area units adding `acres`/`hectares` |
|  [06]   | `Grid` = `"point"\|"square"\|"hex"\|"triangle"`                                 | enum          | grid tessellation kind               |
|  [07]   | `Corners`                                                                       | enum          | bbox corner and center selector      |
|  [08]   | `Id` = `string \| number`                                                       | union         | feature identifier                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the substrate — one constructor family, one traversal family, one accessor family

`viewer/geo` composes the parameterized substrate: `feature`/`point`/`polygon`/… mints one geometry per constructor, the `meta` traversal folds N element granularities each through an `Each`/`Reduce` pair, and `invariant` normalizes the flexible `Coord` input — build, walk, and read through the substrate, never a hand-written coordinate loop.

| [INDEX] | [SURFACE]                                                                                 | [CAPABILITY]    | [CONSUMER]           |
| :-----: | :---------------------------------------------------------------------------------------- | :-------------- | :------------------- |
|  [01]   | `feature` / `geometry` / `point`(`s`) / `lineString`(`s`) / `polygon`(`s`)                | constructor     | `viewer/geo/layers`  |
|  [02]   | `multiPoint` / `multiLineString` / `multiPolygon`                                         | constructor     | `viewer/geo/layers`  |
|  [03]   | `featureCollection` / `geometryCollection`                                                | collection ctor | `viewer/geo/layers`  |
|  [04]   | `coordEach` / `coordReduce` / `coordAll` / `geomEach` / `geomReduce`                      | traversal       | `viewer/geo`         |
|  [05]   | `propEach` / `propReduce` / `featureEach` / `featureReduce`                               | traversal       | `viewer/geo`         |
|  [06]   | `flattenEach` / `segmentEach` / `lineEach` (+ `Reduce`)                                   | traversal       | `viewer/geo`         |
|  [07]   | `getCoord` / `getCoords` / `getGeom` / `getType`                                          | accessor        | `viewer/geo/project` |
|  [08]   | `getType` / `geojsonType` / `featureOf` / `collectionOf` / `containsNumber`               | assert          | `viewer/geo`         |
|  [09]   | `round` / `radiansToLength` / `lengthToRadians` / `degreesToRadians` / `radiansToDegrees` | unit convert    | `viewer/geo`         |
|  [10]   | `bearingToAzimuth` / `convertLength` / `convertArea`                                      | unit convert    | `viewer/geo`         |

[ENTRYPOINT_SCOPE]: measurement + transformation/overlay — the core geometry ops

Scalar-over-geometry measurements and geometry→geometry transforms, each a pure function taking a `units`/tolerance/step option; `union`/`intersect`/`difference` are the NTS overlay peer, `bbox` → `fitBounds` and `center`/`centroid` → camera target feed the map, and `truncate` trims precision before re-encode.

| [INDEX] | [SURFACE]                                                                              | [CAPABILITY]     | [CONSUMER]                  |
| :-----: | :------------------------------------------------------------------------------------- | :--------------- | :-------------------------- |
|  [01]   | `area` / `bbox` / `bboxPolygon` / `center` / `centroid`                                | measure (extent) | `viewer/geo/project`        |
|  [02]   | `centerOfMass` / `centerMean` / `centerMedian`                                         | measure (center) | `viewer/geo/project`        |
|  [03]   | `distance` / `length` / `bearing` / `midpoint` / `along` / `destination`               | measure (metric) | `viewer/geo`, `viewer/mark` |
|  [04]   | `pointToLineDistance` / `pointToPolygonDistance`                                       | measure (metric) | `viewer/geo`                |
|  [05]   | `rhumbDistance` / `rhumbBearing` / `rhumbDestination` / `greatCircle`                  | measure (metric) | `viewer/geo`                |
|  [06]   | `buffer` / `simplify` / `convex` / `concave` / `voronoi`                               | construct        | `viewer/geo/layers`         |
|  [07]   | `circle` / `ellipse` / `sector` / `bezierSpline` / `polygonSmooth`                     | construct        | `viewer/geo/layers`         |
|  [08]   | `union` / `intersect` / `difference` / `dissolve` / `bboxClip` / `mask` / `lineOffset` | overlay          | `viewer/geo`                |
|  [09]   | `transformRotate` / `transformScale` / `transformTranslate`                            | mutate           | `viewer/geo`                |
|  [10]   | `flip` / `rewind` / `truncate` / `cleanCoords` / `clone`                               | mutate           | `viewer/geo`                |
|  [11]   | `combine` / `explode` / `flatten` / `lineToPolygon` / `polygonToLine`                  | convert          | `viewer/geo/layers`         |
|  [12]   | `polygonize` / `lineChunk` / `lineArc` / `tesselate`                                   | convert          | `viewer/geo/layers`         |

[ENTRYPOINT_SCOPE]: boolean predicates + line/segment queries + spatial index

`booleanPointInPolygon` and its DE-9IM siblings (the NTS `boolean*` peer), the line/segment queries, and turf's R-tree serve `viewer/mark/selection`: a click or lasso hit-tests through `booleanPointInPolygon`/`pointsWithinPolygon`, and the `geojsonRbush` index makes many-feature selection sub-linear.

| [INDEX] | [SURFACE]                                                                            | [CAPABILITY]     | [CONSUMER]              |
| :-----: | :----------------------------------------------------------------------------------- | :--------------- | :---------------------- |
|  [01]   | `booleanContains` / `booleanWithin` / `booleanCrosses` / `booleanOverlap`            | DE-9IM predicate | `viewer/mark/selection` |
|  [02]   | `booleanDisjoint` / `booleanIntersects` / `booleanTouches` / `booleanEqual`          | DE-9IM predicate | `viewer/mark/selection` |
|  [03]   | `booleanPointInPolygon` / `booleanPointOnLine` / `pointsWithinPolygon`               | point query      | `viewer/mark/selection` |
|  [04]   | `pointOnFeature` / `nearestPoint`                                                    | point query      | `viewer/mark/selection` |
|  [05]   | `booleanClockwise` / `booleanConcave` / `booleanParallel` / `booleanValid`           | shape predicate  | `viewer/geo`            |
|  [06]   | `lineIntersect` / `lineOverlap` / `lineSegment` / `lineSlice` / `lineSliceAlong`     | line query       | `viewer/geo`            |
|  [07]   | `lineSplit` / `kinks` / `nearestPointOnLine` / `nearestPointToLine` / `shortestPath` | line query       | `viewer/geo`            |
|  [08]   | `geojsonRbush()` → `{ insert, load, search, collides, remove }`                      | spatial index    | `viewer/mark/selection` |

[ENTRYPOINT_SCOPE]: interpolation + grids + clustering/statistics + projection/random

Surface-analysis, tessellation, spatial-statistics, projection, and random families: IDW/TIN interpolation, grids, and clusters yield FeatureCollections the `viewer/geo/layers` row renders, and `toMercator`/`toWgs84` reconcile WGS84 ↔ Web Mercator at the map boundary.

| [INDEX] | [SURFACE]                                                                            | [CAPABILITY] | [CONSUMER]            |
| :-----: | :----------------------------------------------------------------------------------- | :----------- | :-------------------- |
|  [01]   | `interpolate` / `isobands` / `isolines` / `tin` / `planepoint`                       | surface      | `viewer/geo/layers`   |
|  [02]   | `hexGrid` / `squareGrid` / `triangleGrid` / `pointGrid` / `rectangleGrid` / `sample` | grid         | `viewer/geo`          |
|  [03]   | `clustersKmeans` / `clustersDbscan` / `clusters` / `collect` / `tag`                 | cluster      | `viewer/geo/layers`   |
|  [04]   | `standardDeviationalEllipse` / `directionalMean` / `moranIndex`                      | statistic    | `viewer/probe`        |
|  [05]   | `quadratAnalysis` / `nearestNeighborAnalysis` / `distanceWeight`                     | statistic    | `viewer/probe`        |
|  [06]   | `toMercator` / `toWgs84`                                                             | projection   | `viewer/geo/project`  |
|  [07]   | `randomPoint` / `randomLineString` / `randomPolygon` / `randomPosition`              | generator    | `dev`, `viewer/probe` |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every op is a synchronous `(input, options) => GeoJSON | scalar` with no state, no effect, no DOM; the concern families (measure, construct, overlay, convert, predicate, surface, grid, cluster/stat) sit on the substrate, catalogued by concern rather than a flat roster.
- `feature`/`point`/`polygon`/… mints one geometry per constructor, `coordEach`/`geomEach`/`featureEach`/`segmentEach`/`lineEach` (× `Each`/`Reduce`) folds one traversal over element granularity, and `getCoord`/`getCoords`/`getGeom`/`getType` normalizes the flexible `Coord` input — the parameterized substrate `viewer/geo` builds, walks, and reads through, never a hand-written `coordinates` loop.
- `Coord` collapses the input fork: a point argument enters as `Position | Point | Feature<Point>` and `getCoord` normalizes it, so a measurement stays polymorphic in input shape without an overload family. `{ units }` (bounded by `Units`/`AreaUnits`) is the sole place a measurement's unit is chosen, never a `distanceKm`/`distanceMi` sibling.
- Boolean predicates are the DE-9IM peer: `booleanContains`/`Within`/`Crosses`/`Overlap`/`Disjoint`/`Intersects`/`Touches`/`Equal` mirror the NetTopologySuite relationship matrix; turf owns the JS runtime and NTS the C#, meeting at the WKB/GeoJSON wire, and a relation that diverges across the two is a cross-language drift defect.

[STACKING]:
- `effect` (`libs/typescript/.api/effect.md`): GeoJSON arrives `Schema`-decoded at `wire` and typed through `wire#vocab`; turf ops are pure sync, wrapped in `Effect.sync` only to sit inside an effectful pipeline, and a feature `Stream` folds through them with `Stream.map`/`Effect.forEach`, so a decode `ParseError` never reaches turf.
- `@geoarrow/deck.gl-geoarrow` + `apache-arrow` (`.api/geoarrow-deck.gl-geoarrow.md`, `.api/apache-arrow.md`): geoarrow renders `arrow.RecordBatch` columns zero-copy while turf runs over materialized GeoJSON at interaction scale — a drawn query polygon, a buffer, a mask, a boolean hit-test — then feeds the result to a layer; materializing a bulk `RecordBatch` to GeoJSON for a turf op discards the columnar path.
- `@deck.gl/layers` (`.api/deck.gl-layers.md`): a geometry-producing op output binds as `GeoJsonLayer.data` (the omnibus point/line/fill dispatch) or `PolygonLayer.getPolygon` (the per-object ring accessor); `buffer`/`bboxPolygon` yield `Feature<Polygon>`, `union`/`intersect`/`difference` take one `FeatureCollection<Polygon|MultiPolygon>` and yield `Feature<Polygon|MultiPolygon> | null`, `voronoi`/`isobands` yield a `FeatureCollection`, and `featureCollection`/`feature` assemble the `data`.
- `maplibre-gl` (`.api/maplibre-gl.md`): a turf `FeatureCollection` is a `GeoJSONSource`; `bbox` drives `map.fitBounds`, `center`/`centroid` the camera target, and `toMercator`/`toWgs84` reconcile the projection — the `viewer/geo/project` camera-sync seam shared with the deck overlay `viewState`.
- `geojsonRbush` (within-lib): build one R-tree (`geojsonRbush().load(fc)`) and `search(bbox)`/`collides` it for many-feature point-in-polygon or nearest queries; a per-query `booleanPointInPolygon` scan over the whole collection is the O(n) defect the index removes, the `viewer/mark/selection` many-`GlobalId` hit-test path.

[LOCAL_ADMISSION]:
- Consume `wire#vocab`-decoded GeoJSON; the WKB→GeoJSON decode is `wire`-owned and happens once, never re-minted in `ui`.
- Run turf at interaction scale over materialized features; the bulk `arrow.RecordBatch`→layer path stays on the geoarrow columnar route.
- Import from the `.` umbrella (tree-shaken via `sideEffects: false`) or `@turf/<op>` directly in a hot module, every turf import inside `scope:viewer`.

[RAIL_LAW]:
- Package: `@turf/turf`
- Owns: the browser-side planar/spherical GeoJSON geometry engine — the concern families, the `feature`/`coordEach`/`getCoord` substrate, the `Units`/`AreaUnits` algebra, the `geojsonRbush` index, and the WGS84↔Mercator projection ops
- Accept: pure ops over `wire`-decoded GeoJSON, substrate build/walk/read, `{ units }`-parameterized measurement, interaction-scale turf beside bulk geoarrow render, `GeoJsonLayer.data`/`PolygonLayer.getPolygon`/`GeoJSONSource` result binding, a `geojsonRbush` index for repeated queries, `Effect.sync` inside an effectful pipeline
- Reject: WKB decode or geometry re-mint in `ui`, hand-looped `coordinates` where `meta` folds them, bulk `RecordBatch`→GeoJSON to run an op, per-unit function siblings, O(n) predicate scans where an rbush index applies, a re-minted C# NTS computation crossing back over the wire, turf imports outside `scope:viewer`
