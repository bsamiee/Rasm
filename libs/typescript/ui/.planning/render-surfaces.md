# [UI_RENDER_SURFACES]

One page owns the leaf render surfaces — the read-only observation routes `EvidenceTimelineRoute`/`BenchmarkRoute`/`CollectorPanel` and the 2D cartographic `GeoSeriesSurface` over one `GeoSeriesLayer` union owner. Observation routes are leaf React subscribers at the SAME altitude as the atom binding — read-only views built ON the binding, not a separate domain — and the geo surface is one more render leaf over the `interchange` `GeometryRail` projection. The routes sit on the `projection` folds and read, never emit; dashboards read the collector while instrumentation belongs to `platform/platform-substrate.md`. The page owns no wire cluster directly and consumes the receipt, evidence, and snapshot-geometry shapes only through stores and the rail.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]       | [OWNS]                                                      |
| :-----: | :-------------- | :---------------------------------------------------------- |
|   [1]   | RENDER_SURFACES | the read-only routes and the one GeoSeriesLayer geo surface |
|   [2]   | RESEARCH        | the GlbViewport WebGL render precondition-DAG               |

## [2]-[RENDER_SURFACES]

- Owner: the read-only observation routes — `EvidenceTimelineRoute`, `BenchmarkRoute`, `CollectorPanel` — and `GeoSeriesSurface`, the 2D cartographic surface over the single `GeoSeriesLayer` union; each route is a thin subscriber over a `projection` fold or the telemetry collector holding no domain state, and the geo surface owns the renderer and never a second decode.
- Cases: `EvidenceTimelineRoute` carries receipt envelopes whole and renders them in HLC order with the `SkewBand` confidence interval from the `projection` `EvidenceFeed`, so a browser dashboard renders "within +/-N ms confidence" without recomputing the HLC fold; `BenchmarkRoute` renders only when fingerprint-gated by the host-fingerprint shape on `receipts-and-benchmarks.md#TS_PROJECTION`, so an unverifiable claim never displays as verified; `CollectorPanel` reads the telemetry collector; `GeoSeriesSurface` dispatches the renderer total over the `GeoSeriesLayer` union — the base case draws pan-zoom-style-spec cartography over the maplibre `Map` substrate, the overlay case draws the geometry family keyed by `featureKind` as one `GeoJsonLayer` in the `@deck.gl/react` `DeckGL` layer set composited by the `overlayMode` discriminant (the `DeckGL` component reverse-controls the maplibre `Map` as its child base map, or the layer set renders standalone over a free `MapViewState`). The four prior loose aliases (`MapSubstrate`, `OverlayMode`, `GeometryFeatureKind`, `GeoSeriesComposition`) are deleted and folded into the one union owner — relocating spam is not collapsing it.
- Entry: dashboards sit on the evidence fold and the receipt store and read, never emit; the read-versus-emit split is explicit — dashboards read the collector while instrumentation belongs to `platform/platform-substrate.md`; the routes subscribe to their fold only through the one `binding.md` `AtomBinding`; `GeoSeriesSurface` sources its geometry only through the `interchange` `GeometryRail` decoded to the GeoJSON projection on `snapshot-codecs.md#TS_PROJECTION`, and the maplibre `Map` instance is held as an `Effect.acquireRelease` resource bound under the `platform/host-runtime.md` `BrowserPlatform`, never a free React ref.
- Packages: `react` for the route components, `@tanstack/react-virtual` for the virtualized timeline (composed through the component-system), `@effect/opentelemetry` used strictly as a collector reader, `maplibre-gl` for the `Map` base-map substrate, `@deck.gl/core` for the `Deck`/`MapViewState` view state, `@deck.gl/layers` for the `GeoJsonLayer` overlay constructor, `@deck.gl/react` for the `DeckGL` component that reverse-controls the maplibre `Map` as its child base map, and `effect` for the `Schema.Literal` union and the `acquireRelease` resource.
- Growth: a new observation route lands as one route module over an existing store; a new telemetry panel reads the same collector; a new geometry-feature kind lands as one `featureKind` literal, a new overlay mode as one `overlayMode` literal, a new base substrate as one union case, never a parallel surface.
- Boundary: a benchmark claim displayed without the fingerprint gate is the named defect; `CollectorPanel` crosses no wire contract of its own and references no telemetry wire type; the routes never re-decode a value the `interchange` rail admitted; the four geo aliases are collapsed into one union and never restated as parallel const objects; a second decode of the geometry beside `GeometryRail` is the named defect; the 3D GLB viewport is NOT authored here — `GlbViewport` is a `REFINEMENT_HORIZON` entry pending the upstream mesh `#TS_PROJECTION`, and admitting any WebGL package before that fence exists is the named premature-admission defect.

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

interface BenchmarkRoute {
  readonly render: (claim: BenchmarkClaimWire, fingerprint: Option.Option<HostFingerprintWire>) => Option.Option<React.ReactElement>;
}

const renderBenchmark = (
  claim: BenchmarkClaimWire,
  fingerprint: Option.Option<HostFingerprintWire>,
): Option.Option<React.ReactElement> =>
  Option.match(fingerprint, {
    onNone: () => Option.none(),
    onSome: (host) =>
      host.machineKey === claim.machineKey
        ? Option.some(React.createElement(BenchmarkCard, { claim, host, verified: true }))
        : Option.none(),
  });
```

## [3]-[RESEARCH]

- [GLB_VIEWPORT]: the WebGL mesh render is a four-end cross-branch precondition-DAG (recorded in `region-map/seam-splits.md`) — (a) the C# `remote-lane#TS_PROJECTION` must promote `GeometryPayload(mesh)`/`MeshTensor` from `[PROTO_VOCABULARY]` into the projection fence, the single true blocker; (b) C# `interchange.md` authoring DISCHARGED; (c) the Python `libs/python/compute` IFC->GLB two-hop companion authors, observed not forced; (d) only then `ui` admits a WebGL viewer with a `Schema.Literal` renderer-backend axis admitting three/babylon/model-viewer and a webgpu literal for the meshlet/cluster-LOD ambition. The GLB byte/artifact layer is NOT on this DAG — `interchange` `ArtifactFrameRail` already reassembles the content-addressed GLB bytes today; only the WebGL render is deferred, and zero WebGL packages are admitted until (a) exists.
