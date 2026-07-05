# [TS_UI]

`libs/typescript/ui` is the wave-4 interface package: the component system (design tokens, motion and interaction, atom state binding, localization, headless primitives) and the view plane (Schema-driven forms, the data grid, overlays, the analytic chart owner), with `viewer` as a second Nx project carrying the spatial tier — GLB scene, geospatial surface, GlobalId marks, wire-materialized panels, render evidence. One atom binding, one styling recipe, one overlay owner, one grid owner, one chart owner; the viewer consumes decoded wire vocabularies and renders, never re-owning semantics. `ARCHITECTURE.md` carries the domain map and seams, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [01]-[ROUTER]

- [01]-[TOKEN](.planning/system/token.md)
- [02]-[ACT](.planning/system/act.md)
- [03]-[ATOM](.planning/system/atom.md)
- [04]-[INTL](.planning/system/intl.md)
- [05]-[PRIMITIVE](.planning/system/primitive.md)
- [06]-[FORM](.planning/view/form.md)
- [07]-[TABLE](.planning/view/table.md)
- [08]-[OVERLAY](.planning/view/overlay.md)
- [09]-[CHART](.planning/view/chart.md)
- [10]-[SCENE](.planning/viewer/scene.md)
- [11]-[GEO](.planning/viewer/geo.md)
- [12]-[MARK](.planning/viewer/mark.md)
- [13]-[PANEL](.planning/viewer/panel.md)
- [14]-[PROBE](.planning/viewer/probe.md)

## [02]-[DOMAIN_PACKAGES]

Every folder-specific external library, planned or implemented. Versions are centralized in `pnpm-workspace.yaml`; corroborating API evidence lives in the adjacent `.api/` folder.

[REACT_SUBSTRATE]:
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

[HEADLESS]:
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

[MOTION_GESTURE]:
- `motion`
- `@use-gesture/react`

[GRID_CHARTS]:
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

[SPATIAL]:
- `three`
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

Cross-cutting TypeScript substrate this folder consumes; canonical registry and charters live in `libs/typescript/.planning/README.md` and the adjacent `libs/typescript/.api/` folder.

[TYPING_RAILS]:
- `effect`

[OVERLAY_LANE]:
- `@effect/experimental`

[PLATFORM]:
- `@effect/platform`
- `@effect/platform-browser`
