# [TS_UI_API_TURF_TURF]

`@turf/turf` is the umbrella barrel over ~100 pure GeoJSON geometry functions (114 `@turf/*` modules) — the browser-side planar/spherical geometry engine, the JS peer of the C# NetTopologySuite: the same DE-9IM boolean predicates, overlay ops (union/intersect/difference), measurement, and construction, meeting the C# side only at the WKB/GeoJSON wire. It is NOT a flat function grab-bag to enumerate: it is ~8 concern families of pure `(geojson, options) => geojson | scalar` transforms sitting on a parameterized SUBSTRATE — one constructor family (`feature`/`point`/`polygon`/…), one traversal family (`coordEach`/`geomEach`/`featureEach`/`segmentEach` × `Each`/`Reduce`), and one accessor family (`getCoord`/`getGeom`/`getType` normalizing the flexible `Coord` input) — that the `viewer/geo` design page composes against, never re-loops by hand. WKB→GeoJSON decode stays in `wire` (decoded once, never re-minted in `ui`); turf runs over the already-decoded features. It is `scope:viewer` project-local — admitted only by the `ui/viewer` Nx project, compile-time excluded from the non-spatial core.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@turf/turf`
- package: `@turf/turf`
- license: `MIT`
- deps: 114 `@turf/*` modules (each independently versioned `catalog`) + `@types/geojson` (the GeoJSON value types); `tslib`
- catalog-verdict: KEEP
- runtime: `scope:viewer` project-local — admitted only by the `ui/viewer` Nx project, compile-time excluded from the non-spatial core (same tier as `@deck.gl/*`/`apache-arrow`/`three`)
- asset: self-typed ESM+CJS (`main: dist/cjs/index.cjs`, `module: dist/esm/index.js`, `types: dist/esm/index.d.ts`), `sideEffects: false` — tree-shakeable, so the umbrella import prunes to the used ops; a hot module may import `@turf/<op>` directly
- entry: single `.` barrel re-exporting every op plus the `helpers`/`meta`/`invariant`/`clusters`/`random`/`projection` namespaces; pure sync functions — no runtime, no effect rail, no DOM

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the GeoJSON value vocabulary + turf's flexible input + unit types
- rail: viewer/geo
- The value algebra is `geojson` (`@types/geojson`) — `Feature`/`FeatureCollection`/`Geometry`/`Position` and the seven geometry types — re-exported through turf. Turf adds the flexible-input types: `Coord` is ONE coordinate input accepted three ways (`Position | Point | Feature<Point>`) and normalized by `getCoord`, so an op signature never forks per input shape. The unit enums are the bounded `units` option vocabulary every measurement carries.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:----------------------------------------------------------------------- |:---------------- |:----------------------------------------------------------------------- |
| [01] | `Feature<G, P>` / `FeatureCollection<G, P>` / `Geometry` / `GeometryCollection` (from `geojson`) | value carrier | `viewer/geo` — the decoded wire value; `Schema`-decoded at `wire`, typed through `wire#vocab` in `ui` |
| [02] | `Point` / `LineString` / `Polygon` / `MultiPoint` / `MultiLineString` / `MultiPolygon` / `Position` / `BBox` | geometry type | the seven GeoJSON geometry shapes + `Position` (`[lng, lat]`) + `BBox` (`[w,s,e,n]`) |
| [03] | `AllGeoJSON` = `Feature \| FeatureCollection \| Geometry \| GeometryCollection` | any-input union | the input type of `meta` traversal + `truncate`/`clone`/`flip` — one union over every GeoJSON shape |
| [04] | `Coord` = `Feature<Point> \| Point \| Position` | flexible coord | `viewer/geo/project` — the polymorphic point input `getCoord` normalizes; one input, three shapes |
| [05] | `Units` (`"kilometers" \| "miles" \| "degrees" \| "radians" \| …`) / `AreaUnits` (`+ "acres" \| "hectares"`) | unit vocab | the bounded `{ units }` option on every measurement; `AreaUnits` for `area`/`convertArea` |
| [06] | `Grid` (`"point"\|"square"\|"hex"\|"triangle"`) / `Corners` / `Lines` / `Id` | option enum | the `grid` kind, tangent corners, and `id` types the grid/clip ops accept |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the substrate — one constructor family, one traversal family, one accessor family
- rail: viewer/geo
- The parameterized layer the design page composes with. `feature`/`point`/`polygon`/… is ONE constructor pattern instanced per geometry; the `meta` traversal is ONE iteration mechanism over N element granularities (each with an `Each`/`Reduce` pair); `invariant` is ONE accessor normalizing the flexible input. A `viewer/geo` row builds features with the constructors, walks them with `coordEach`/`geomEach`, and reads values with `getCoord`/`getGeom` — never a hand-written coordinate loop.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:------------------------------------------------------------------------------------------ |:--------------- |:--------------------------------------------------------------- |
| [01] | `feature` / `geometry` / `point`(`s`) / `lineString`(`s`) / `polygon`(`s`) / `multiPoint` / `multiLineString` / `multiPolygon` / `featureCollection` / `geometryCollection` | constructor | `viewer/geo/layers` — build the GeoJSON layer `data`; one `feature(geom, props, opts)` shape per geometry |
| [02] | `coordEach` / `coordReduce` / `coordAll` / `geomEach` / `geomReduce` / `propEach` / `propReduce` / `featureEach` / `featureReduce` / `flattenEach` / `segmentEach` / `lineEach` (+ `Reduce`) | traversal | `viewer/geo` — one iteration mechanism over coord/geom/prop/feature/segment/line granularity; the fold replaces a manual loop |
| [03] | `getCoord` / `getCoords` / `getGeom` / `getType` | accessor | `viewer/geo/project` — normalize the flexible `Coord`/`Feature`/`Geometry` input to the raw `Position`/coords/geometry |
| [04] | `getType` / `geojsonType` / `featureOf` / `collectionOf` / `containsNumber` | assert | `viewer/geo` — runtime shape assertions at an op boundary (belt-and-braces under a `Schema` decode) |
| [05] | `round` / `radiansToLength` / `lengthToRadians` / `degreesToRadians` / `radiansToDegrees` / `bearingToAzimuth` / `convertLength` / `convertArea` | unit convert | `viewer/geo` — the unit algebra behind every measurement's `{ units }` option |

[ENTRYPOINT_SCOPE]: measurement + transformation/overlay — the core geometry ops
- rail: viewer/geo
- The scalar-over-geometry measurements and the geometry→geometry transforms/overlays. Each is a pure function taking a `units`/tolerance/step option; the rosters are SEED DATA — a geometry op is one function in a concern family, and a new derived quantity/shape is a new call, not a new mechanism. The overlay ops (`union`/`intersect`/`difference`) are the JTS/NTS overlay peers.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:------------------------------------------------------------------------------------------ |:--------------- |:--------------------------------------------------------------- |
| [01] | `area` / `bbox` / `bboxPolygon` / `center` / `centroid` / `centerOfMass` / `centerMean` / `centerMedian` | measure (extent) | `viewer/geo/project` — `bbox` → maplibre `fitBounds`; `center`/`centroid` → camera target |
| [02] | `distance` / `length` / `bearing` / `midpoint` / `along` / `destination` / `pointToLineDistance` / `pointToPolygonDistance` / `rhumbDistance` / `rhumbBearing` / `rhumbDestination` / `greatCircle` | measure (metric) | `viewer/geo` + `viewer/mark` — planar/rhumb/great-circle distance + bearing; `{ units }`-parameterized |
| [03] | `buffer` / `simplify` / `convex` / `concave` / `voronoi` / `circle` / `ellipse` / `sector` / `bezierSpline` / `polygonSmooth` | construct | `viewer/geo/layers` — derive an overlay polygon (buffer a selection, hull a point cloud) rendered as GeoJSON |
| [04] | `union` / `intersect` / `difference` / `dissolve` / `bboxClip` / `mask` / `lineOffset` | overlay | `viewer/geo` — the NTS-peer boolean overlay; clip/mask a layer to a query polygon |
| [05] | `transformRotate` / `transformScale` / `transformTranslate` / `flip` / `rewind` / `truncate` / `cleanCoords` / `clone` | mutate | `viewer/geo` — affine + normalization transforms; `truncate` trims coord precision before re-encode |
| [06] | `combine` / `explode` / `flatten` / `lineToPolygon` / `polygonToLine` / `polygonize` / `lineChunk` / `lineArc` / `tesselate` | convert | `viewer/geo/layers` — feature-shape conversion between multi/single and line/polygon for layer binding |

[ENTRYPOINT_SCOPE]: boolean predicates + line/segment queries + spatial index
- rail: viewer/mark
- The DE-9IM spatial-relationship predicates (the NTS `boolean*` peer) and the line/segment geometric queries, plus turf's own R-tree index for repeated queries. `viewer/mark/selection` uses `booleanPointInPolygon`/`pointsWithinPolygon` for hit-testing and the rbush index to make many-feature selection sub-linear.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:------------------------------------------------------------------------------------------ |:--------------- |:--------------------------------------------------------------- |
| [01] | `booleanContains` / `booleanWithin` / `booleanCrosses` / `booleanOverlap` / `booleanDisjoint` / `booleanIntersects` / `booleanTouches` / `booleanEqual` | DE-9IM predicate | `viewer/mark/selection` — the NTS-peer spatial-relationship tests; one `boolean` per DE-9IM relation |
| [02] | `booleanPointInPolygon` / `booleanPointOnLine` / `pointsWithinPolygon` / `pointOnFeature` / `nearestPoint` | point query | `viewer/mark/selection` — hit-test a click/lasso against features → `GlobalId` set |
| [03] | `booleanClockwise` / `booleanConcave` / `booleanParallel` / `booleanValid` | shape predicate | `viewer/geo` — winding/validity checks before an overlay op or re-encode |
| [04] | `lineIntersect` / `lineOverlap` / `lineSegment` / `lineSlice` / `lineSliceAlong` / `lineSplit` / `kinks` / `nearestPointOnLine` / `nearestPointToLine` / `shortestPath` | line query | `viewer/geo` — polyline geometric queries (snap, split, self-intersection, routing) |
| [05] | `geojsonRbush()` → `{ insert, load, search, collides, remove }` | spatial index | `viewer/mark/selection` — build ONE R-tree over many features; `search(bbox)` replaces O(n) scans |

[ENTRYPOINT_SCOPE]: interpolation + grids + clustering/statistics + projection/random
- rail: viewer/geo
- The surface-analysis, tessellation, and spatial-statistics families, plus the projection and random generators. These produce derived FeatureCollections (grids, isolines, clusters) the `viewer/geo/layers` row renders, and the projection ops convert between WGS84 and Web Mercator at the map boundary.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:------------------------------------------------------------------------------------------ |:--------------- |:--------------------------------------------------------------- |
| [01] | `interpolate` / `isobands` / `isolines` / `tin` / `planepoint` | surface | `viewer/geo/layers` — IDW/TIN interpolation → iso-band/line FeatureCollection for a heat/contour layer |
| [02] | `hexGrid` / `squareGrid` / `triangleGrid` / `pointGrid` / `rectangleGrid` / `sample` | grid | `viewer/geo` — one grid mechanism, four cell shapes (`Grid` enum); binning/sampling over a bbox |
| [03] | `clustersKmeans` / `clustersDbscan` / `clusters` / `collect` / `tag` | cluster | `viewer/geo/layers` — k-means/DBSCAN clustering + spatial join (`tag`/`collect`) → styled clusters |
| [04] | `standardDeviationalEllipse` / `directionalMean` / `moranIndex` / `quadratAnalysis` / `nearestNeighborAnalysis` / `distanceWeight` | statistic | `viewer/probe` — spatial-autocorrelation/dispersion stats over a FeatureCollection |
| [05] | `toMercator` / `toWgs84` | projection | `viewer/geo/project` — WGS84 ↔ Web-Mercator at the map/camera boundary |
| [06] | `randomPoint` / `randomLineString` / `randomPolygon` / `randomPosition` | generator | `dev`/`viewer/probe` — synthetic geometry for specs and benchmark seeding |

## [04]-[IMPLEMENTATION_LAW]

[TURF_TOPOLOGY]:
- Pure functions over GeoJSON, grouped by concern: every op is a synchronous `(input: Feature/FeatureCollection/Geometry, options) => GeoJSON | scalar` with no state, no effect, no DOM. The 114 modules are ~8 concern families (measure, construct, overlay, convert, predicate, surface, grid, cluster/stat) plus the substrate — a table by concern, not a flat roster to memorize.
- The substrate is the parameterized layer: `feature`/`point`/`polygon`/… is ONE constructor family per geometry; `coordEach`/`geomEach`/`featureEach`/`segmentEach`/`lineEach` (× `Each`/`Reduce`) is ONE traversal mechanism over element granularity; `getCoord`/`getCoords`/`getGeom`/`getType` is ONE accessor normalizing the flexible `Coord` input. A `viewer/geo` row builds, walks, and reads through the substrate — never a hand-written `feature.geometry.coordinates[i][j]` loop.
- `Coord` collapses the input fork: a point argument is accepted as `Position | Point | Feature<Point>` and normalized by `getCoord`, so a measurement signature is polymorphic in its input shape without an overload family. The `{ units }` option (bounded by `Units`/`AreaUnits`) is the one place a measurement's unit is chosen — never a `distanceKm`/`distanceMi` sibling.
- Boolean predicates are the DE- catalogIM peer: `booleanContains`/`Within`/`Crosses`/`Overlap`/`Disjoint`/`Intersects`/`Touches`/`Equal` are the same spatial-relationship matrix NetTopologySuite exposes on the C# side; turf owns the JS runtime, NTS the C# runtime, and they meet at the WKB/GeoJSON wire — a re-mint of the same relation on both sides that does diverge is a cross-language drift defect.

[INTEGRATION_LAW]:
- Stack with `effect` (`libs/typescript/.api/effect.md`): GeoJSON arrives as a `Schema`-decoded wire projection — WKB→GeoJSON is decoded ONCE at `wire` and typed through `wire#vocab`, never re-minted in `ui`. Turf ops are pure sync; wrap in `Effect.sync` only to sit inside an effectful pipeline, and fold a feature `Stream` through ops with `Stream.map`/`Effect.forEach`. A `ParseError` at decode never reaches turf — turf sees only valid geometry.
- Stack with `@geoarrow/deck.gl-geoarrow` + `apache-arrow` (`.api/geoarrow-deck.gl-geoarrow.md`, `.api/apache-arrow.md`): the load-bearing boundary — geoarrow renders `arrow.RecordBatch` columns zero-copy, turf works over materialized GeoJSON objects. Run turf on INTERACTION-SCALE features (a drawn query polygon, a buffer, a mask, a boolean hit-test), then feed the result to a GeoJSON layer or re-encode; NEVER materialize a bulk `RecordBatch` to GeoJSON just to run a turf op — that discards the columnar path. geoarrow owns the bulk render, turf the query-scale geometry.
- Stack with `@deck.gl/layers` (`.api/deck.gl-layers.md`) — the pre-GPU planar-op seam: turf is the CPU-side planar-op stage over `wire`-decoded GeoJSON, deck the GPU rasterizer. A geometry-producing op output — `buffer`/`bboxPolygon` → `Feature<Polygon>`, `union`/`intersect`/`difference` → `Feature<Polygon|MultiPolygon> | null` (turf 7.x takes ONE `FeatureCollection<Polygon|MultiPolygon>` arg, not a feature pair), `simplify` → same-shape `T`, `voronoi`/`isobands` → `FeatureCollection` — binds two ways: as `GeoJsonLayer.data` (`GeoJSON|Feature[]`, the omnibus point/line/fill dispatch) or as `PolygonLayer.getPolygon` (the per-object ring `AccessorFunction` over the `data` feature array). The `featureCollection`/`feature` constructor assembles the `data`; the interaction-scale derived-overlay render path that needs no GeoArrow columnar encoding — bulk columnar geometry stays on the geoarrow route.
- Stack with `maplibre-gl` (`.api/maplibre-gl.md`): a turf `FeatureCollection` is a `GeoJSONSource`; `bbox` drives `map.fitBounds`, `center`/`centroid` the camera target, `toMercator`/`toWgs84` reconcile the projection — the `viewer/geo/project` camera-sync seam shared with the deck overlay `viewState`.
- Stack with `geojsonRbush` for repeated queries: build ONE R-tree (`geojsonRbush().load(fc)`) and `search(bbox)`/`collides` it for many-feature point-in-polygon or nearest queries; a per-query `booleanPointInPolygon` scan over the whole collection is the O(n) defect the index removes — the `viewer/mark/selection` many-`GlobalId` hit-test path.
- Cross-language peer (C# NetTopologySuite): turf is the browser-side planar-geometry engine mirroring the NTS op vocabulary (predicates, overlay, measurement); each side owns its runtime and they meet at the WKB/GeoJSON wire — geometry is decoded per side from the shared wire, never a JS op result shipped back to stand in for the C# computation.

[LOCAL_ADMISSION]:
- Consume already-decoded GeoJSON from `wire#vocab`; never decode WKB or re-mint the geometry projection inside `ui` — the decode is `wire`-owned and happens once.
- Build, walk, and read through the substrate (`feature`/`coordEach`/`getCoord`); never hand-loop `coordinates` arrays or fork a signature per `Coord` input shape.
- Run turf at interaction scale over materialized features; never materialize a bulk `arrow.RecordBatch` to GeoJSON to run an op — that is the geoarrow columnar path's job.
- Choose the `{ units }` option for a measurement; never author a per-unit function sibling. Build a `geojsonRbush` index for repeated spatial queries over many features; never O(n)-scan per query.
- Import from the umbrella (tree-shaken via `sideEffects: false`) or `@turf/<op>` directly in a hot module; keep every turf import inside `scope:viewer` — it is compile-excluded from the core.

[RAIL_LAW]:
- Package: `@turf/turf`
- Owns: the browser-side planar/spherical GeoJSON geometry engine — the concern families (measure/construct/overlay/convert/DE-9IM-predicate/surface/grid/cluster-stat), the parameterized substrate (the `feature` constructor family, the `coordEach`/`geomEach`/`featureEach` traversal family, the `getCoord`/`getGeom` accessor family), the unit algebra, the `geojsonRbush` spatial index, and the WGS84↔Mercator projection ops
- Accept: pure ops over `wire`-decoded GeoJSON, the substrate for build/walk/read, `{ units }`-parameterized measurement, interaction-scale turf beside bulk geoarrow columnar render, `GeoJsonLayer.data`/`PolygonLayer.getPolygon`/`GeoJSONSource` binding of results, a `geojsonRbush` index for repeated queries, `scope:viewer`-local imports, `Effect.sync` only inside an effectful pipeline
- Reject: WKB decode or geometry re-mint inside `ui`, hand-looped `coordinates` arrays where `meta` traversal folds them, bulk `RecordBatch`→GeoJSON materialization to run a turf op, per-unit function siblings, O(n) predicate scans where an rbush index applies, a re-mint of a C# NTS computation crossing back over the wire, turf imports outside `scope:viewer`
