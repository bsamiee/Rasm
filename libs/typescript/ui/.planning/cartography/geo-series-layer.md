# [UI_GEO_SERIES_LAYER]

The 2D geospatial surface — one `GeoSeriesLayer` closed `Data.TaggedEnum` family over the maplibre base substrate and the deck.gl overlay layers, sourcing geometry only through the `interchange` `GeometryRail` GeoJSON projection. A distinct professional domain (GIS/cartography) from both the observation dashboards and the 3D mesh viewport. The deck.gl `DeckGL` component reverse-controls the maplibre `Map` as its child base map; out-of-core data rides the GeoArrow layers and `TileLayer`. The maplibre `Map` is an `Effect.acquireRelease` resource bound under the `platform` `BrowserPlatform`; the surface owns no decode of its own.

## [1]-[INDEX]

One cluster: `GEO_SERIES_LAYER` owns the base-versus-overlay cartographic surface family.

## [2]-[GEO_SERIES_LAYER]

- Owner: `GeoSeriesLayer`, the single closed `Data.TaggedEnum` family over the maplibre base substrate and the deck.gl overlay layer set; the surface owns the renderer and never a second decode. The four prior loose aliases (`MapSubstrate`, `OverlayMode`, `GeometryFeatureKind`, `GeoSeriesComposition`) are folded into the one tagged family. The family is interior composition state holding no decode authority — `styleSpec`/`viewState` are the maplibre `StyleSpecification`/`ViewState` interior types, never a re-decoded wire shape.
- Cases: the renderer dispatches total over the `GeoSeriesLayer` family under one `$match` — the `base` case draws pan-zoom-style-spec cartography over the maplibre `Map` substrate; the `overlay` case draws the geometry family keyed by `featureKind` as the deck.gl layer set composited by the `overlayMode` discriminant (interleaved or overlaid), where the `DeckGL` component reverse-controls the maplibre `Map` as its child base map or the layer set renders standalone over a free `MapViewState`. Out-of-core data (millions of features) rides the GeoArrow layer family and `TileLayer` rather than a single in-memory `GeoJsonLayer`, keyed by the same `featureKind` discriminant.
- Entry: the surface sources its geometry only through the `interchange` `GeometryRail` decoded to the GeoJSON projection on `interchange` `decode-rail#TS_PROJECTION`; the maplibre `Map` instance is held as an `Effect.acquireRelease` resource bound under the `platform` `BrowserPlatform`, never a free React ref; the view state reads and writes through the `binding/atom-binding.md` `AtomBinding`.
- Packages: `maplibre-gl`, `@deck.gl/core`, `@deck.gl/layers`, `@deck.gl/mapbox`, `effect`.
- Growth: a new geometry-feature kind lands as one `featureKind` literal, a new overlay mode as one `overlayMode` literal, a new base substrate as one tagged variant breaking the `$match` at compile time; an out-of-core source lands as one GeoArrow/`TileLayer` row keyed by `featureKind`, never a parallel surface.
- Boundary: the four geo aliases are collapsed into one `Data.TaggedEnum` family and never restated as parallel const objects; a hand-rolled `Schema.Union` of `_tag`-bearing structs for this closed family is the named defect the tagged owner deletes; a second decode of the geometry beside `GeometryRail` is the named defect; the maplibre `Map` held as a free React ref is the named defect; this surface never reaches a C# geometry interior and never re-decodes a value the `GeometryRail` admitted.

```ts contract
type GeoSeriesLayer = Data.TaggedEnum<{
  readonly base: { readonly substrate: "maplibre-base"; readonly styleSpec: StyleSpecification; readonly viewState: ViewState };
  readonly overlay: {
    readonly substrate: "deckgl-overlay";
    readonly featureKind: "point" | "path" | "polygon" | "mesh-projection";
    readonly overlayMode: "interleaved" | "overlaid";
    readonly source: "geojson" | "geoarrow" | "tile";
  };
}>;
const GeoSeriesLayer = Data.taggedEnum<GeoSeriesLayer>();

const renderLayer = (geojson: GeoJSON.FeatureCollection): (layer: GeoSeriesLayer) => StyleSpecification | LayersList =>
  GeoSeriesLayer.$match({
    base: ({ styleSpec }) => styleSpec,
    overlay: ({ featureKind }) => [
      new GeoJsonLayer({ id: featureKind, data: geojson, pickable: true, stroked: true, filled: true }),
    ],
  });
```

RESEARCH [GEOARROW_TILE]: the `@deck.gl/layers` GeoArrow layer family (`GeoArrowPolygonLayer`/`GeoArrowPathLayer`/`GeoArrowScatterplotLayer`) and the `TileLayer`/`MVTLayer` out-of-core constructors are unverified; the member spellings stay RESEARCH until the folder `.api/` catalogue carries the rows. The `@deck.gl/mapbox` `MapboxOverlay` interleaved path is the camera-sync contract between the deck.gl overlay and the maplibre base map.
