# [TS_UI_API_TYPES_GEOJSON]

`@types/geojson` declares the RFC 7946 object model as one closed `type`-discriminated type algebra: a zero-runtime `.d.ts` admitted `import type`, the render-side geometry value vocabulary `ui/viewer` speaks. It owns the geometry union, the `Feature`/`FeatureCollection` generics, and the coordinate primitives; interaction-scale features ride it while bulk columnar geometry rides `@geoarrow/deck.gl-geoarrow` and wire decode stays the core `WkbParser` Schema shape.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@types/geojson`
- package: `@types/geojson` (MIT)
- deps: none — a zero-dependency leaf tracking the RFC 7946 encoding
- asset: declaration-only — a single `index.d.ts` (`types` entry only, NO `main`/`module`, NO runtime, NO exported value); `tsc` is the gate
- namespace: `export as namespace GeoJSON` — a UMD global for script-tag consumers; module code imports named `type`s instead
- admission: `import type { Feature, FeatureCollection, Position, … } from "geojson"`; NEVER a value import — there is no runtime symbol to import
- scope: `scope:viewer` project-local — pulled by `@turf/turf`/`maplibre-gl`/`@deck.gl/layers` and reached directly by `ui/viewer`, compile-time absent from the non-spatial core
- marker: `Position` is deliberately the wide `number[]`, NOT narrowed to `[number, number] | [number, number, number]` — narrowing is a caller's user-defined type guard, never a package change
- rail: viewer/geo — the RFC 7946 render-side value vocabulary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the coordinate primitives — the leaf value axes every geometry keys on
- `Position` is the coordinate array (X, Y, optional Z under RFC 7946); `BBox` is the 2D-or-3D bounding-box tuple; `GeoJsonProperties` is the nullable open property bag. These are the leaves the geometry `coordinates` and the `Feature.properties`/`bbox` fields are built from.

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY] | [CONSUMER_BOUNDARY]                                |
| :-----: | :------------------------------------------------------ | :------------ | :------------------------------------------------- |
|  [01]   | `Position` = `number[]`                                 | coordinate    | one coordinate; intentionally un-narrowed (marker) |
|  [02]   | `BBox` = `[n,n,n,n] \| [n,n,n,n,n,n]`                   | bounds tuple  | 2D or 3D extent; `GeoJsonObject.bbox?`             |
|  [03]   | `GeoJsonProperties` = `{ [name: string]: any } \| null` | property bag  | `Feature`'s default `P`; the open attribute map    |

[PUBLIC_TYPE_SCOPE]: the geometry union — the seven `type`-discriminated shapes
- `Geometry` is the closed seven-member union (`GeometryObject` its alias); each member refines `GeoJsonObject` with a literal `type` and a `coordinates` nesting depth (or `geometries` for the collection), and a `switch` on `.type` narrows to exactly one member.

| [INDEX] | [SYMBOL]                                                    | [TYPE_FAMILY]  | [CONSUMER_BOUNDARY]                              |
| :-----: | :---------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `Geometry` / `GeometryObject`                               | geometry union | the closed seven-member union (alias pair)       |
|  [02]   | `Point` (`coordinates: Position`)                           | geometry       | `type: "Point"` — a single coordinate            |
|  [03]   | `MultiPoint` / `LineString` (`coordinates: Position[]`)     | geometry       | one flat coordinate ring — points or a path      |
|  [04]   | `MultiLineString` / `Polygon` (`coordinates: Position[][]`) | geometry       | ring array — line set or polygon rings           |
|  [05]   | `MultiPolygon` (`coordinates: Position[][][]`)              | geometry       | polygon array — nested ring sets                 |
|  [06]   | `GeometryCollection<G>` (`geometries: G[]`)                 | geometry       | `type: "GeometryCollection"` — heterogeneous set |

[PUBLIC_TYPE_SCOPE]: the container generics + narrowing discriminants — the `<G, P>` object graph
- `GeoJsonObject` is the base (`type` + optional `bbox`); `Feature<G, P>` wraps one geometry with `properties` and an optional `id`; `FeatureCollection<G, P>` is the `Feature` array; `GeoJSON<G, P>` is the top union of all three. `G extends Geometry | null` admits the null-geometry feature and `P` threads the property shape. `GeoJsonTypes` and `GeoJsonGeometryTypes`, the `["type"]` lookup unions, are the discriminant vocabularies a boundary decoder switches on.

| [INDEX] | [SYMBOL]                                                          | [TYPE_FAMILY] | [CONSUMER_BOUNDARY]                                |
| :-----: | :---------------------------------------------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `GeoJsonObject` (`type: GeoJsonTypes`; `bbox?: BBox`)             | base object   | the root of every geometry/feature/collection      |
|  [02]   | `Feature<G, P>` (`geometry`/`properties`/`id?`)                   | feature       | `G extends Geometry \| null`; nullable `id`        |
|  [03]   | `FeatureCollection<G, P>` (`features: Array<Feature<G, P>>`)      | collection    | `viewer/geo` — `GeoJsonLayer.data`/`GeoJSONSource` |
|  [04]   | `GeoJSON<G, P>` = `G \| Feature<G, P> \| FeatureCollection<G, P>` | top union     | the RFC 7946 top-level union of all three          |
|  [05]   | `GeoJsonTypes` = `GeoJSON["type"]`                                | discriminant  | every RFC 7946 `type` literal — boundary switch    |
|  [06]   | `GeoJsonGeometryTypes` = `Geometry["type"]`                       | discriminant  | the seven geometry `type` literals                 |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One closed type algebra, discriminated by the literal `type` tag: every shape refines `GeoJsonObject`, and narrowing is a `switch (obj.type)` over `GeoJsonTypes`/`GeoJsonGeometryTypes` — never an ad-hoc field probe. RFC 7946 fixes the union, so no new object kind is representable; the vocabulary is fixed data, not an open interface to extend.
- Two type parameters thread the whole graph: `G extends Geometry | null` flows from `Feature`/`FeatureCollection`/`GeoJSON` into their `geometry`/`features`, and `P` (default `GeoJsonProperties`) types `Feature.properties`. One generic instantiation — `FeatureCollection<Polygon, MyProps>` — carries both facts to every accessor, never a per-property re-declaration.
- Coordinate nesting IS the geometry discriminant depth: `Position` (point) → `Position[]` (multipoint/linestring) → `Position[][]` (multilinestring/polygon) → `Position[][][]` (multipolygon), with `GeometryCollection` carrying `geometries: G[]` instead of `coordinates`. Reading the depth is reading the type.

[STACKING]:
- `@deck.gl/layers` (`.api/deck.gl-layers.md`): `GeoJsonLayer.data` takes a `FeatureCollection`/`Feature[]` directly — the `viewer/geo` `_features(id, collection: FeatureCollection)` row binds a decoded collection to the omnibus point/line/fill layer, `pointType`/fill/stroke accessors fanning one feature stream to the whole mark vocabulary. This is the interaction-scale render path.
- `@geoarrow/deck.gl-geoarrow` (`.api/geoarrow-deck.gl-geoarrow.md`): the CONTRAST arm — geoarrow binds GPU attributes from `apache-arrow` columns (`arrow.RecordBatch`) with ZERO row materialization, avoiding the per-row JS objects a `FeatureCollection` is. geojson is the interaction-scale/derived-overlay value; bulk columnar geometry rides geoarrow, and re-materializing a `RecordBatch` to `FeatureCollection` for a per-row accessor is the defect it exists to prevent.
- `maplibre-gl` (`.api/maplibre-gl.md`): a `FeatureCollection` is a `GeoJSONSource` payload — `GeoJSONSource.setData(collection)` pushes a derived overlay into the basemap's own vector pipeline, `bbox`/`center` driving the camera through the `viewer/geo/project` sync seam shared with the deck overlay `viewState`.
- `@turf/turf` (`.api/turf-turf.md`): turf RE-EXPORTS this value vocabulary (`Feature`/`FeatureCollection`/`Geometry`/`Position`) and every op is a pure `(geojson, options) => geojson | scalar` over it — `buffer`/`union`/`intersect`/`difference` produce `Feature<Polygon|MultiPolygon>` overlays, `booleanPointInPolygon` + `geojsonRbush` drive the `viewer/mark/selection` lasso hit-test over feature centroids. turf runs over already-decoded features; it never re-mints the wire.
- core `WkbParser` / `GeoFeature` (`libs/typescript/core/.planning/interchange/codec.md`): the wire boundary and the DISTINCT decoded shape — `WkbParser.parse(wkb, srid)` yields the core's Schema-typed `GeoFeature.Geometry` (a `_tag`-discriminated `Schema.Class` union), NOT this `type`-discriminated one. geojson is the render-side value the viewer materializes at interaction scale from wire-decoded features; the WKB decode stays behind the `WkbParser` port, and `ui/viewer` parses no geometry byte.

[LOCAL_ADMISSION]:
- Admit as `import type` only — there is no runtime symbol; a value import of `geojson` is the named error. Reach for the named `type`s directly (`FeatureCollection`, `Feature`, `Position`, `BBox`), not the UMD `GeoJSON` global (script-tag legacy).
- Type the render surface with the generic instantiation carrying both facts — `FeatureCollection<Polygon, MyProps>`, not a bare `FeatureCollection` re-narrowed at each accessor. Let the `<G, P>` params thread geometry and property shape to every consumer.
- Narrow with the `type` discriminant: `switch (g.type)` over `GeoJsonGeometryTypes` for geometry, `GeoJsonTypes` for any object. For 2D-vs-3D `Position`, write a user-defined type guard — the package will not narrow the coordinate array.
- Keep the wire boundary: consume geojson as the render/turf/maplibre-facing value derived from `wire`-decoded features; never treat it as the `WkbParser` output (that is Schema-typed `GeoFeature.Geometry`) and never re-derive a spatial relation the C# side owns as authority.

[RAIL_LAW]:
- Package: `@types/geojson`
- Owns: the RFC 7946 type algebra — the `Geometry`/`GeometryObject` seven-member union and its members, the `Feature<G, P>`/`FeatureCollection<G, P>`/`GeoJSON<G, P>` container generics, the `Position`/`BBox`/`GeoJsonProperties` coordinate primitives, the `GeoJsonObject` base, and the `GeoJsonTypes`/`GeoJsonGeometryTypes` narrowing discriminants
- Accept: `import type` usage in `ui/viewer`, generic instantiation over geometry + property shape, `type`-discriminant narrowing, a caller type guard for 2D/3D `Position`, binding a `FeatureCollection` to `GeoJsonLayer.data`/`GeoJSONSource`, turf ops over decoded features, `scope:viewer`-local reach
- Reject: a value import of the package, hand-narrowing `Position` by patching the type, treating geojson as the `WkbParser` output (it is `GeoFeature.Geometry`, Schema-typed), row-materializing a `RecordBatch` to `FeatureCollection` for a per-row accessor (the geoarrow columnar route owns bulk), re-parsing WKB bytes in `ui`, admission outside `scope:viewer`
