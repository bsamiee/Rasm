# [UI_GEO_SERIES_LAYER]

The 2D geospatial surface — one `GeoSeriesLayer` closed `Data.TaggedEnum` family over the maplibre base substrate and the deck.gl overlay layers, sourcing geometry only through the `interchange` `GeometryRail`. A distinct professional domain (GIS/cartography) from both the observation dashboards and the 3D mesh viewport. The `@deck.gl/mapbox` `MapboxOverlay` `IControl` interleaves the deck.gl layers into the maplibre WebGL2 stack with automatic camera sync; out-of-core data rides the `@deck.gl/geo-layers` `TileLayer`/`MVTLayer` viewport tiles and the `@geoarrow/deck.gl-geoarrow` `RecordBatch` layers bound directly to the Arrow projection. The maplibre `Map` is an `Effect.acquireRelease` resource bound under the `platform` `BrowserPlatform`; the surface owns no decode of its own.

## [1]-[INDEX]

One cluster: `GEO_SERIES_LAYER` owns the base-versus-overlay cartographic surface family.

## [2]-[GEO_SERIES_LAYER]

- Owner: `GeoSeriesLayer`, the single closed `Data.TaggedEnum` family over the maplibre base substrate and the deck.gl overlay layer set; the surface owns the renderer and never a second decode. The four prior loose aliases (`MapSubstrate`, `OverlayMode`, `GeometryFeatureKind`, `GeoSeriesComposition`) are folded into the one tagged family. The family is interior composition state holding no decode authority — `styleSpec`/`viewState` are the maplibre `StyleSpecification`/`ViewState` interior types, never a re-decoded wire shape.
- Cases: the renderer dispatches total over the `GeoSeriesLayer` family under one `$match` — the `base` case draws pan-zoom-style-spec cartography over the maplibre `Map` substrate; the `overlay` case draws the geometry family keyed by `featureKind` as the deck.gl layer set composited by the `overlayMode` discriminant, where `interleaved` mounts the `@deck.gl/mapbox` `MapboxOverlay` `IControl` into the maplibre WebGL2 stack with automatic camera sync and `overlaid` renders deck.gl on its own canvas layered over the map. The `source` discriminant routes the draw: `geojson` draws the bounded in-memory `GeoJsonLayer`, `tile` streams the `@deck.gl/geo-layers` `TileLayer`/`MVTLayer` viewport-driven tiles, and `geoarrow` binds the `@geoarrow/deck.gl-geoarrow` `RecordBatch` layers (`GeoArrowScatterplotLayer`/`GeoArrowPathLayer`/`GeoArrowPolygonLayer`) directly to the `interchange` Arrow projection with no per-feature JavaScript iteration. Out-of-core data (millions of features) rides the `tile`/`geoarrow` arms rather than a single in-memory `GeoJsonLayer`, keyed by the same `featureKind` discriminant.
- Entry: the surface sources its geometry only through the `interchange` `GeometryRail` decoded to the GeoJSON projection (the `geojson`/`tile` arms) or the GeoArrow Arrow `RecordBatch` projection (the `geoarrow` arm) on `interchange` `decode-rail#TS_PROJECTION`; the maplibre `Map` instance is held as an `Effect.acquireRelease` resource bound under the `platform` `BrowserPlatform`, never a free React ref; the `geoarrow` `GeoArrowSolidPolygonLayer`/`GeoArrowPolygonLayer` triangulation rides one shared earcut `Pool` built once at the surface boundary through `initEarcutPool` and passed into every polygon-layer instance; the view state reads and writes through the `binding/atom-binding.md` `AtomBinding`.
- Packages: `maplibre-gl`, `@deck.gl/core`, `@deck.gl/layers`, `@deck.gl/geo-layers`, `@deck.gl/mapbox`, `@geoarrow/deck.gl-geoarrow`, `effect`.
- Growth: a new geometry-feature kind lands as one `featureKind` literal, a new overlay mode as one `overlayMode` literal, a new data source as one `source` literal breaking the `$match` at compile time; a new out-of-core layer (cell-index `H3`/`S2`, `TripsLayer`, `Tile3DLayer`) lands as one row on the `source`-arm draw keyed by `featureKind`, never a parallel surface.
- Boundary: the four geo aliases are collapsed into one `Data.TaggedEnum` family and never restated as parallel const objects; a hand-rolled `Schema.Union` of `_tag`-bearing structs for this closed family is the named defect the tagged owner deletes; a second decode of the geometry beside `GeometryRail` is the named defect; the maplibre `Map` held as a free React ref is the named defect; a GeoJSON round-trip of an Arrow `RecordBatch` the `GeometryRail` already projected as GeoArrow is the named defect the `geoarrow` arm deletes; a per-polygon-layer earcut pool instead of the one shared `initEarcutPool` pool is the named defect; this surface never reaches a C# geometry interior and never re-decodes a value the `GeometryRail` admitted.

```ts contract
import type { LayersList } from "@deck.gl/core";
import type { StyleSpecification, ViewState } from "maplibre-gl";
import type { RecordBatch } from "@rasm/ts/interchange";
import { GeoJsonLayer } from "@deck.gl/layers";
import { TileLayer, MVTLayer } from "@deck.gl/geo-layers";
import { MapboxOverlay } from "@deck.gl/mapbox";
import {
  GeoArrowPathLayer,
  GeoArrowPolygonLayer,
  GeoArrowScatterplotLayer,
  initEarcutPool,
} from "@geoarrow/deck.gl-geoarrow";
import { Data, Match } from "effect";

type FeatureKind = "point" | "path" | "polygon" | "mesh-projection";

type GeoSeriesLayer = Data.TaggedEnum<{
  readonly base: { readonly substrate: "maplibre-base"; readonly styleSpec: StyleSpecification; readonly viewState: ViewState };
  readonly overlay: {
    readonly substrate: "deckgl-overlay";
    readonly featureKind: FeatureKind;
    readonly overlayMode: "interleaved" | "overlaid";
    readonly source: "geojson" | "tile" | "geoarrow";
  };
}>;
const GeoSeriesLayer = Data.taggedEnum<GeoSeriesLayer>();

interface GeoSource {
  readonly geojson: GeoJSON.FeatureCollection;
  readonly tileUrl: string;
  readonly batch: RecordBatch;
  readonly earcutPool: Awaited<ReturnType<typeof initEarcutPool>>;
}

const overlayLayers = (src: GeoSource, featureKind: FeatureKind, source: "geojson" | "tile" | "geoarrow"): LayersList =>
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
