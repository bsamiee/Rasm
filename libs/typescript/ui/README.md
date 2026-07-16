# [TS_UI]

`ui` is the browser product surface over the Rasm wire — one body: the component-system floor every surface rides, the view plane of dense composite surfaces, and the `viewer` spatial tier as a second Nx project. Its bar is one-truth rendering: every state fact binds through the one atom bridge, so components are projection surfaces that never run effects or mirror domain state; selection is one `GlobalId` set every echo projects from — grid rows, deck highlights, feature states, and viewport reveals simultaneously; color is one OKLCH artifact computed once, gamut-fit and contrast-gated at decode, feeding the CSS plane and the viewer's linear render space as one object; visualization data crosses zero-copy on one Arrow bus with no JSON detour; and every C#-minted vocabulary materializes field-for-field — a clamp, remap, or local default is named cross-language drift. One engine per surface: a table, chart, or element carries exactly one owner, and engine-doubling is the named defect.

`viewer` parses no geometry byte and authors no BCF value — WKB and wire decode stay behind the core interchange codec, every GPU resource is scope-bracketed, and the folder declares the viewport and host-plane ports the app composition root satisfies. It owns zero geometry and zero IFC semantics; render is the whole charter.

## [01]-[ROUTER]

- [01]-[SYSTEM](.planning/system/): One token authority dual-sunk to CSS and viewer linear space, one motion vocabulary, one atom binding.
- [02]-[VIEW](.planning/view/): Dense surfaces instantiating the floor — forms, grid, overlay, and chart, each one owner where variation is rows.
- [03]-[VIEWER](.planning/viewer/): Spatial tier — content-keyed residency behind the `GlbViewport` port and the one `GlobalId` selection plane.

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
