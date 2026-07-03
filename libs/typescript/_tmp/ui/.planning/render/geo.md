# [UI_GEO]

The 2D geospatial surface — one `GeoSeriesLayer` closed `Data.TaggedEnum` family over the maplibre base substrate and the deck.gl overlay layers, sourcing geometry only through the `interchange` `GeometryRail`. A distinct professional domain (GIS/cartography) from both the observation dashboards and the 3D mesh viewport. The `@deck.gl/mapbox` `MapboxOverlay` `IControl` interleaves the deck.gl layers into the maplibre WebGL2 stack with automatic camera sync; out-of-core data rides the `@deck.gl/geo-layers` `TileLayer`/`MVTLayer` viewport tiles and the `@geoarrow/deck.gl-geoarrow` `RecordBatch` layers bound directly to the Arrow projection. The `cell-index` source arm extends the same `$match` with the spatial-bin aggregation family — the `@geoarrow/deck.gl-geoarrow` Arrow-native `GeoArrowH3HexagonLayer`/`GeoArrowS2Layer`/`GeoArrowGeohashLayer`/`GeoArrowA5Layer` reading the same `interchange` GeoArrow `RecordBatch` index column directly with no second decode, `GeoArrowTripsLayer` for time-windowed movement over that batch, the `@deck.gl/geo-layers` `H3ClusterLayer`/`QuadkeyLayer` set-merge families binding the bounded JS cell-id projection (the two cell kinds with no Arrow-native layer), and `Tile3DLayer` for OGC-3D-Tiles/`i3s` city-context streaming under the existing `MapboxOverlay` interleave — never a parallel surface and never a re-materialization of the Arrow batch through a JS-data cell layer. The maplibre `Map` is an `Effect.acquireRelease` resource bound under the `platform` `BrowserPlatform`; the surface owns no decode of its own.

## [01]-[INDEX]

- [01]-[GEO_SERIES_LAYER]: the `GeoSeriesLayer` `Data.TaggedEnum` over the maplibre base substrate and the deck.gl overlay layer set, dispatching `geojson`/`tile`/`geoarrow`/`cell-index` source arms total under `$match`.

## [02]-[GEO_SERIES_LAYER]

- Owner: `GeoSeriesLayer`, the single closed `Data.TaggedEnum` family over the maplibre base substrate and the deck.gl overlay layer set; the surface owns the renderer and never a second decode. The four prior loose aliases (`MapSubstrate`, `OverlayMode`, `GeometryFeatureKind`, `GeoSeriesComposition`) are folded into the one tagged family. The family is interior composition state holding no decode authority — `styleSpec`/`viewState` are the maplibre `StyleSpecification`/`CameraOptions` interior types, never a re-decoded wire shape.
- Cases: the renderer dispatches total over the `GeoSeriesLayer` family under one `$match` — the `base` case draws pan-zoom-style-spec cartography over the maplibre `Map` substrate; the `overlay` case draws the geometry family keyed by `featureKind` as the deck.gl layer set composited by the `overlayMode` discriminant, where `interleaved` mounts the `@deck.gl/mapbox` `MapboxOverlay` `IControl` into the maplibre WebGL2 stack with automatic camera sync and `overlaid` renders deck.gl on its own canvas layered over the map. The `source` discriminant routes the draw: `geojson` draws the bounded in-memory `GeoJsonLayer`, `tile` streams the `@deck.gl/geo-layers` `TileLayer`/`MVTLayer` viewport-driven tiles, `geoarrow` binds the `@geoarrow/deck.gl-geoarrow` `RecordBatch` layers (`GeoArrowScatterplotLayer`/`GeoArrowPathLayer`/`GeoArrowPolygonLayer`) directly to the `interchange` Arrow projection with no per-feature JavaScript iteration, and `cell-index` aggregates the spatial-bin family keyed by the aggregation `featureKind` — `h3`/`s2`/`geohash`/`a5` draw the `@geoarrow/deck.gl-geoarrow` Arrow-native `GeoArrowH3HexagonLayer`/`GeoArrowS2Layer`/`GeoArrowGeohashLayer`/`GeoArrowA5Layer` reading the identical `RecordBatch` index column the `geoarrow` arm projects, `trips` draws `GeoArrowTripsLayer` over that same batch, `h3-cluster`/`quadkey` draw the `@deck.gl/geo-layers` `H3ClusterLayer`/`QuadkeyLayer` (extending `_GeoCellLayer`) over the bounded JS cell-id projection because no Arrow-native variant exists for the set-merge and quadkey families, and `tile-3d` streams `Tile3DLayer` (OGC-3D-Tiles/`i3s` over the `Tileset2D` indexing model) under the existing `MapboxOverlay` interleave for city-scale context. Out-of-core data (millions of features) rides the `tile`/`geoarrow`/`cell-index` arms rather than a single in-memory `GeoJsonLayer`, keyed by the same `featureKind` discriminant; the Arrow-native `cell-index` families read the identical Arrow `RecordBatch` the `geoarrow` arm projects, never a second decode, and the two JS-cell families bind their native cell-id data prop rather than re-materializing the batch.
- Entry: the surface sources its geometry only through the `interchange` `GeometryRail` decoded to the GeoJSON projection (the `geojson`/`tile` arms) or the GeoArrow Arrow `RecordBatch` projection (the `geoarrow` arm) on `interchange` `decode-rail#TS_PROJECTION`; the maplibre `Map` instance is held as an `Effect.acquireRelease` resource bound under the `platform` `BrowserPlatform`, never a free React ref; the `geoarrow` `GeoArrowSolidPolygonLayer`/`GeoArrowPolygonLayer` triangulation rides one shared earcut `Pool` built once at the surface boundary through `initEarcutPool` and passed into every polygon-layer instance; the view state reads and writes through the `binding/atom.md` `AtomBinding`.
- Packages: `maplibre-gl`, `@deck.gl/core`, `@deck.gl/layers`, `@deck.gl/geo-layers`, `@deck.gl/mapbox`, `@geoarrow/deck.gl-geoarrow`, `effect`.
- Growth: a new geometry-feature kind lands as one `FeatureKind` literal, a new overlay mode as one `overlayMode` literal, a new data source as one `source` literal breaking the `$match` at compile time; a new out-of-core or cell-index layer lands as one `FeatureKind` literal under the `cell-index` arm's nested `Match.value(featureKind)` keyed off the same Arrow projection, never a parallel surface and never a second decode.
- Boundary: the four geo aliases are collapsed into one `Data.TaggedEnum` family and never restated as parallel const objects; a hand-rolled `Schema.Union` of `_tag`-bearing structs for this closed family is the named defect the tagged owner deletes; a second decode of the geometry beside `GeometryRail` is the named defect; the maplibre `Map` held as a free React ref is the named defect; a GeoJSON round-trip of an Arrow `RecordBatch` the `GeometryRail` already projected as GeoArrow is the named defect the `geoarrow` arm deletes; a parallel aggregation surface beside the one `$match` `cell-index` arm, or an in-memory `GeoJsonLayer` re-aggregation of a value the GeoArrow projection already carries, is the named defect; feeding the Arrow `RecordBatch` into a `@deck.gl/geo-layers` JS-data `_GeoCellLayer` class (`H3HexagonLayer`/`S2Layer`/`GeohashLayer`/`A5Layer`) rather than the Arrow-native `@geoarrow/deck.gl-geoarrow` cell layer — which forces the per-feature materialization the no-second-decode law forbids — is the named defect; a per-polygon-layer earcut pool instead of the one shared `initEarcutPool` pool is the named defect; this surface never reaches a C# geometry interior and never re-decodes a value the `GeometryRail` admitted.

```ts contract
import type { LayersList } from "@deck.gl/core";
import type { CameraOptions, StyleSpecification } from "maplibre-gl";
import type { RecordBatch } from "@rasm/ts/interchange";
import { GeoJsonLayer } from "@deck.gl/layers";
import { H3ClusterLayer, MVTLayer, QuadkeyLayer, Tile3DLayer } from "@deck.gl/geo-layers";
import { MapboxOverlay } from "@deck.gl/mapbox";
import {
  GeoArrowA5Layer,
  GeoArrowGeohashLayer,
  GeoArrowH3HexagonLayer,
  GeoArrowPathLayer,
  GeoArrowPolygonLayer,
  GeoArrowS2Layer,
  GeoArrowScatterplotLayer,
  GeoArrowTripsLayer,
  initEarcutPool,
} from "@geoarrow/deck.gl-geoarrow";
import { Data, Match } from "effect";

// `FeatureKind` widens with the cell-index aggregation literals so the `cell-index` arm
// dispatches the spatial-bin layer set off the same field the geometry arms already key.
type FeatureKind =
  | "point"
  | "path"
  | "polygon"
  | "mesh-projection"
  | "h3"
  | "h3-cluster"
  | "s2"
  | "quadkey"
  | "geohash"
  | "a5"
  | "tile-3d"
  | "trips";

type GeoSeriesLayer = Data.TaggedEnum<{
  readonly base: { readonly substrate: "maplibre-base"; readonly styleSpec: StyleSpecification; readonly viewState: CameraOptions };
  readonly overlay: {
    readonly substrate: "deckgl-overlay";
    readonly featureKind: FeatureKind;
    readonly overlayMode: "interleaved" | "overlaid";
    readonly source: "geojson" | "tile" | "geoarrow" | "cell-index";
  };
}>;
const GeoSeriesLayer = Data.taggedEnum<GeoSeriesLayer>();

interface GeoSource {
  readonly geojson: GeoJSON.FeatureCollection;
  readonly tileUrl: string;
  readonly tilesetUrl: string;
  readonly batch: RecordBatch;
  readonly cells: ReadonlyArray<{ readonly token: string; readonly weight: number }>;
  readonly earcutPool: Awaited<ReturnType<typeof initEarcutPool>>;
}

// The `cell-index` arm aggregates the SAME Arrow `RecordBatch` the `geoarrow` arm projects with
// no second decode: the Arrow-native cell families bind the `@geoarrow/deck.gl-geoarrow`
// index-column layers (`GeoArrowH3HexagonLayer`/`GeoArrowS2Layer`/`GeoArrowA5Layer`/
// `GeoArrowGeohashLayer`), each reading `src.batch` directly without per-feature JS iteration,
// and `trips` binds `GeoArrowTripsLayer` over the same batch. The two cell families with no
// Arrow-native variant (`h3-cluster` set-merge, `quadkey`) bind the `@deck.gl/geo-layers`
// `_GeoCellLayer` classes over the bounded JS cell-id projection `src.cells`, never the Arrow
// batch (feeding a `RecordBatch` into a `_GeoCellLayer` would force the per-feature
// materialization the no-second-decode law forbids); `tile-3d` streams an OGC-3D-Tiles tileset
// URL. Keyed by the aggregation `featureKind` under one nested `Match.value(featureKind)`.
const cellIndexLayers = (src: GeoSource, featureKind: FeatureKind): LayersList =>
  Match.value(featureKind).pipe(
    Match.when("h3", () => [new GeoArrowH3HexagonLayer({ id: featureKind, data: src.batch, pickable: true })]),
    Match.when("s2", () => [new GeoArrowS2Layer({ id: featureKind, data: src.batch, pickable: true })]),
    Match.when("geohash", () => [new GeoArrowGeohashLayer({ id: featureKind, data: src.batch, pickable: true })]),
    Match.when("a5", () => [new GeoArrowA5Layer({ id: featureKind, data: src.batch, pickable: true })]),
    Match.when("trips", () => [new GeoArrowTripsLayer({ id: featureKind, data: src.batch })]),
    Match.when("h3-cluster", () => [
      new H3ClusterLayer({ id: featureKind, data: src.cells, getHexagons: (d) => [d.token], pickable: true }),
    ]),
    Match.when("quadkey", () => [
      new QuadkeyLayer({ id: featureKind, data: src.cells, getQuadkey: (d) => d.token, pickable: true }),
    ]),
    Match.when("tile-3d", () => [new Tile3DLayer({ id: featureKind, data: src.tilesetUrl, pickable: true })]),
    Match.orElse(() => []),
  );

const overlayLayers = (
  src: GeoSource,
  featureKind: FeatureKind,
  source: "geojson" | "tile" | "geoarrow" | "cell-index",
): LayersList =>
  Match.value(source).pipe(
    Match.when("geojson", () => [
      new GeoJsonLayer({ id: featureKind, data: src.geojson, pickable: true, stroked: true, filled: true }),
    ]),
    Match.when("tile", () => [
      new MVTLayer({ id: featureKind, data: src.tileUrl, binary: true, pickable: true }),
    ]),
    Match.when("geoarrow", () =>
      Match.value(featureKind).pipe(
        Match.when("point", () => [new GeoArrowScatterplotLayer({ id: featureKind, data: src.batch, pickable: true })]),
        Match.when("path", () => [new GeoArrowPathLayer({ id: featureKind, data: src.batch, pickable: true })]),
        Match.orElse(() => [
          new GeoArrowPolygonLayer({ id: featureKind, data: src.batch, pickable: true, earcutWorkerPool: src.earcutPool }),
        ]),
      )),
    Match.when("cell-index", () => cellIndexLayers(src, featureKind)),
    Match.exhaustive,
  );

const renderLayer = (src: GeoSource): (layer: GeoSeriesLayer) => StyleSpecification | LayersList =>
  GeoSeriesLayer.$match({
    base: ({ styleSpec }) => styleSpec,
    overlay: ({ featureKind, source }) => overlayLayers(src, featureKind, source),
  });

const mountInterleaved = (layers: LayersList): MapboxOverlay =>
  new MapboxOverlay({ interleaved: true, layers });
```
