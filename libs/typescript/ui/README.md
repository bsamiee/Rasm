# [TS_UI]

`ui` is the branch's browser interface plane — the component-system floor, the dense view plane, and the `viewer` spatial tier as a second Nx project. One engine per surface: a table, chart, or element carries exactly one owner.

## [01]-[ROUTER]

- [01]-[SYSTEM](.planning/system/): Component floor — token authority dual-sunk to CSS and viewer linear space; motion, atom, hook, and vital owners.
- [02]-[VIEW](.planning/view/): Dense surfaces instantiating the floor — one owner per plane, variation carried as rows.
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
- `@radix-ui/react-visually-hidden`
- `cmdk`
- `vaul`
- `@floating-ui/react`
- `motion`
- `@use-gesture/react`
- `react-resizable-panels` — solves the N-panel constraint behind shell panes; `Group`/`Panel`/`Separator` carry the window-splitter pattern.

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
- `@tailwindcss/typography` — prose element modifiers and the `--tw-prose-*` variable surface; `view/content` owns the token bridge.

[DATA_SURFACES]:
- `@tanstack/react-table`
- `@tanstack/react-virtual`
- `@tanstack/store` — carries the table's own state vocabulary; `view/table`'s `Grid.edge` mints the adapter atom over the effect-atom fold.
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

[CONTENT_EDITOR]:
- `prosemirror-model` — schema, nodes, and marks as data; `view/content`'s roster derives the `Schema` from one row table.
- `prosemirror-state`
- `prosemirror-view` — hosts the imperative editor DOM outside React's reconciler behind a scoped acquisition.
- `prosemirror-transform`
- `prosemirror-commands`
- `prosemirror-keymap`
- `prosemirror-history`
- `prosemirror-inputrules`
- `prosemirror-schema-list`
- `prosemirror-gapcursor`
- `prosemirror-dropcursor`
- `prosemirror-collab` — JSON steps on the estate's own wire; the sequencer authority is a port the composition root satisfies.

[CANVAS]:
- `@xyflow/react` — controlled node-flow engine; `view/canvas`'s `Canvas.useEdge` binds the controlled props over the one graph cell.
- `elkjs` — layout solve as worker data through `elkjs/lib/elk-api`; the bare entry bundles the full CJS payload.

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
- `@loaders.gl/core` — polymorphic `parse`/`load` decode engine; loaders ride the per-layer `loaders` prop, `registerLoaders` shipping `@deprecated`.
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
