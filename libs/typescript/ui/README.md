# [TS_UI]

`ui` owns the browser interface plane over the Rasm wire: a component-system floor, a view plane composing its owners, and the `viewer` spatial tier as a second Nx project. Viewer renders decoded wire vocabularies and owns zero geometry or IFC semantics.

## [01]-[ROUTER]

[SYSTEM]:
- [01]-[TOKEN](.planning/system/token.md): Design-token authority computing color and dimension as decode-gated data.
- [02]-[ACT](.planning/system/act.md): Motion and interaction owner splitting discrete accessible events from continuous gestures.
- [03]-[ATOM](.planning/system/atom.md): One state binding standing the app `Layer` graph behind the registry.
- [04]-[INTL](.planning/system/intl.md): Zero-package locale plane riding native `Intl` behind one cache.
- [05]-[PRIMITIVE](.planning/system/primitive.md): Headless spine owning the one styled recipe and the sanitize gate.

[VIEW]:
- [06]-[FORM](.planning/view/form.md): Schema-driven forms folding wire decode and live field validity through one kernel `Schema`.
- [07]-[TABLE](.planning/view/table.md): Data grid folding models, virtual windows, and grid semantics under one `TableState` atom.
- [08]-[OVERLAY](.planning/view/overlay.md): Overlay owner — anchoring, sheets, and the command palette over one presence cohort.
- [09]-[CHART](.planning/view/chart.md): Analytic charts folding declarations, streams, and pivots over one Arrow plane.

[VIEWER]:
- [10]-[SCENE](.planning/viewer/scene.md): Content-keyed GLB residency behind the `GlbViewport` port.
- [11]-[GEO](.planning/viewer/geo.md): Geospatial surface sharing one WebGL context as a pure layer value tree.
- [12]-[MARK](.planning/viewer/mark.md): GlobalId mark plane — one selection atom every pick pipeline folds into.
- [13]-[PANEL](.planning/viewer/panel.md): Wire materializer rendering the C#-minted control vocabularies through the owners.
- [14]-[PROBE](.planning/viewer/probe.md): Render evidence pairing benchmarks with wire-decoded receipts, never gating.

## [02]-[DOMAIN_PACKAGES]

UI-specific libraries admitted by this folder; versions centralize in `pnpm-workspace.yaml` and corroborate against this folder's `.api/`.

[REACT_RUNTIME]:
- `react`
- `react-dom`
- `@types/react`
- `@types/react-dom`
- `babel-plugin-react-compiler`
- `react-compiler-runtime`
- `react-error-boundary`

[STATE_BINDING]:
- `@effect-atom/atom`
- `@effect-atom/atom-react`

[HEADLESS_MOTION]:
- `react-aria-components`
- `react-aria`
- `react-stately`
- `@react-aria/live-announcer`
- `@radix-ui/react-slot`
- `@radix-ui/react-label`
- `@radix-ui/react-separator`
- `@radix-ui/react-visually-hidden`
- `cmdk`
- `vaul`
- `@floating-ui/react`
- `@floating-ui/react-dom`
- `motion`
- `@use-gesture/react`

[STYLE_TOKENS]:
- `tailwindcss`
- `tailwind-merge`
- `tw-animate-css`
- `tailwindcss-react-aria-components`
- `class-variance-authority`
- `clsx`
- `colorjs.io`
- `lucide-react`
- `isomorphic-dompurify`

[DATA_SURFACES]:
- `@tanstack/react-table`
- `@tanstack/react-virtual`
- `apache-arrow`
- `@perspective-dev/client`
- `@perspective-dev/viewer`
- `@perspective-dev/viewer-datagrid`
- `@perspective-dev/viewer-charts`
- `uplot`
- `@observablehq/plot`
- `d3`
- `@visx/axis`
- `@visx/group`
- `@visx/responsive`
- `@visx/scale`
- `@visx/shape`
- `tus-js-client`

[SPATIAL]:
- `three`
- `@types/three`
- `@google/model-viewer`
- `maplibre-gl`
- `@deck.gl/core`
- `@deck.gl/layers`
- `@deck.gl/geo-layers`
- `@deck.gl/mesh-layers`
- `@deck.gl/extensions`
- `@deck.gl/mapbox`
- `@geoarrow/deck.gl-geoarrow`
- `@turf/turf`
- `@lume/kiwi`
- `typegpu`
- `@webgpu/types`
- `@types/geojson`

## [03]-[SUBSTRATE_PACKAGES]

Shared TypeScript substrate consumed from the workspace registry; the registry and its charters own the full contracts, and `libs/typescript/.api/` holds the shared API evidence.

[EFFECT_CORE]:
- `effect`

[PLATFORM]:
- `@effect/experimental`
- `@effect/platform`
- `@effect/platform-browser`
