# [TS_UI]

`ui` is the branch's browser interface plane — the component-system floor, the dense view plane, and the `viewer` spatial tier as a second Nx project. One engine per surface: a table, chart, or element carries exactly one owner.

## [01]-[ROUTER]

- [01]-[SYSTEM](.planning/system/): Component floor — token authority dual-sunk to CSS and viewer linear space; motion, atom, hook, and vital owners.
- [02]-[VIEW](.planning/view/): Dense surfaces instantiating the floor — forms, grid, overlay, and chart, each one owner where variation is rows.
- [03]-[VIEWER](.planning/viewer/): Spatial tier — content-keyed residency, the environment dome, and the `GlobalId` selection plane.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `pnpm-workspace.yaml` and corroborate against this folder's `.api/`.

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

[TELEMETRY]:
- `web-vitals` — admitted for its DOM performance-global augmentation alone; `runtime` composes the capture functions and holds the catalogue.

[SPATIAL]:
- `three`
- `@types/three`
- `three-mesh-bvh`
- `@google/model-viewer`
- `maplibre-gl`
- `@deck.gl/core`
- `@deck.gl/layers`
- `@deck.gl/geo-layers`
- `@deck.gl/mesh-layers`
- `@deck.gl/extensions`
- `@deck.gl/mapbox`
- `@loaders.gl/3d-tiles`
- `@loaders.gl/core` — owns the viewer loader registry.
- `@loaders.gl/las`
- `@geoarrow/deck.gl-geoarrow`
- `@turf/turf`
- `@lume/kiwi`
- `typegpu`
- `@webgpu/types`
- `@types/geojson`

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the Ts registry; the registry and its charters own the full contracts, and `libs/typescript/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `effect`

[PLATFORM]:
- `@effect/platform`
- `@effect/platform-browser`
- `@effect/experimental`
