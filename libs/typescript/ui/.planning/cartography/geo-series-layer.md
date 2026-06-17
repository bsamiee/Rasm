# [UI_GEO_SERIES_LAYER]

The 2D geospatial surface — one `GeoSeriesLayer` union over the maplibre base substrate and the deck.gl overlay layers, sourcing geometry only through the `interchange` `GeometryRail` GeoJSON projection. A distinct professional domain (GIS/cartography) from both the observation dashboards and the 3D mesh viewport. The deck.gl `DeckGL` component reverse-controls the maplibre `Map` as its child base map; out-of-core data rides the GeoArrow layers and `TileLayer`. The maplibre `Map` is an `Effect.acquireRelease` resource bound under the `platform` `BrowserPlatform`; the surface owns no decode of its own.

## [1]-[INDEX]

One cluster: `GEO_SERIES_LAYER` owns the base-versus-overlay cartographic surface union.

## [2]-[GEO_SERIES_LAYER]

- Owner: `GeoSeriesLayer`, the single `Schema.Union` over the maplibre base substrate and the deck.gl overlay layer set; the surface owns the renderer and never a second decode. The four prior loose aliases (`MapSubstrate`, `OverlayMode`, `GeometryFeatureKind`, `GeoSeriesComposition`) are folded into the one union owner.
- Cases: the surface dispatches the renderer total over the `GeoSeriesLayer` union — the base case draws pan-zoom-style-spec cartography over the maplibre `Map` substrate; the overlay case draws the geometry family keyed by `featureKind` as the deck.gl layer set composited by the `overlayMode` discriminant (interleaved or overlaid), where the `DeckGL` component reverse-controls the maplibre `Map` as its child base map or the layer set renders standalone over a free `MapViewState`. Out-of-core data (millions of features) rides the GeoArrow layer family and `TileLayer` rather than a single in-memory `GeoJsonLayer`, keyed by the same `featureKind` discriminant.
- Entry: the surface sources its geometry only through the `interchange` `GeometryRail` decoded to the GeoJSON projection on `interchange` `snapshot-codecs#TS_PROJECTION`; the maplibre `Map` instance is held as an `Effect.acquireRelease` resource bound under the `platform` `BrowserPlatform`, never a free React ref; the view state reads and writes through the `binding/atom-binding.md` `AtomBinding`.
- Packages: `maplibre-gl`, `@deck.gl/core`, `@deck.gl/layers`, `@deck.gl/mapbox`, `effect`.
- Growth: a new geometry-feature kind lands as one `featureKind` literal, a new overlay mode as one `overlayMode` literal, a new base substrate as one union case; an out-of-core source lands as one GeoArrow/`TileLayer` row keyed by `featureKind`, never a parallel surface.
- Boundary: the four geo aliases are collapsed into one union and never restated as parallel const objects; a second decode of the geometry beside `GeometryRail` is the named defect; the maplibre `Map` held as a free React ref is the named defect; this surface never reaches a C# geometry interior and never re-decodes a value the `GeometryRail` admitted.

```ts contract
const GeoSeriesLayer = Schema.Union(
  Schema.Struct({
    _tag: Schema.Literal("base"),
    substrate: Schema.Literal("maplibre-base"),
    styleSpec: Schema.Unknown,
    viewState: Schema.Unknown,
  }),
  Schema.Struct({
    _tag: Schema.Literal("overlay"),
    substrate: Schema.Literal("deckgl-overlay"),
    featureKind: Schema.Literal("point", "path", "polygon", "mesh-projection"),
    overlayMode: Schema.Literal("interleaved", "overlaid"),
    source: Schema.Literal("geojson", "geoarrow", "tile"),
  }),
);
type GeoSeriesLayer = Schema.Schema.Type<typeof GeoSeriesLayer>;

const renderLayer = (layer: GeoSeriesLayer, geojson: GeoJSON.FeatureCollection): StyleSpecification | LayersList =>
  Match.value(layer).pipe(
    Match.tag("base", (b) => b.styleSpec as StyleSpecification),
    Match.tag("overlay", (o) => [
      new GeoJsonLayer({ id: o.featureKind, data: geojson, pickable: true, stroked: true, filled: true }),
    ]),
    Match.exhaustive,
  );
```

RESEARCH [GEOARROW_TILE]: the `@deck.gl/layers` GeoArrow layer family (`GeoArrowPolygonLayer`/`GeoArrowPathLayer`/`GeoArrowScatterplotLayer`) and the `TileLayer`/`MVTLayer` out-of-core constructors are unverified; the member spellings stay RESEARCH until the folder `.api/` catalogue carries the rows. The `@deck.gl/mapbox` `MapboxOverlay` interleaved path is the camera-sync contract between the deck.gl overlay and the maplibre base map.
