# [TS_UI]

`ui` is the branch's browser interface plane: the component-system floor, the dense view plane, and the `viewer` spatial stratum as a second Nx project. One engine per surface: a table, chart, or element carries exactly one owner.

## [01]-[ROUTER]

[SYSTEM]:
- [01]-[TOKEN](.planning/system/token.md): Design-token authority — OKLCH color computed, gamut-fit, and contrast-gated at decode.
- [02]-[ACT](.planning/system/act.md): One seat for interaction — accessible events, gesture recognition, and motion rows never fork.
- [03]-[ATOM](.planning/system/atom.md): One state binding — the app's Layer graph stands behind the registry; components project.
- [04]-[CACHE](.planning/system/cache.md): Durable browser-resident bands keyed by content, leaf-verified, committed through one ledger.
- [05]-[HOOK](.planning/system/hook.md): Folder registrar on core's Tap rail — one typed row and one runtime policy per plane.
- [06]-[VITAL](.planning/system/vital.md): Interface-visible evidence — long-animation-frame, event timing, commit windows as probe rows.
- [07]-[INTL](.planning/system/intl.md): Localization with no i18n package — one ambient locale spine over the kernel locale brand.
- [08]-[PRIMITIVE](.planning/system/primitive.md): Headless component spine — `styled` recipe factory, roster law, the announce and sanitize rails.

[VIEW]:
- [09]-[FORM](.planning/view/form.md): Schema-driven input, submission, resumable upload, the wizard, and the auth ceremony faces.
- [10]-[TABLE](.planning/view/table.md): One data grid — virtual windows, grouping, pinning, and the spreadsheet cell band.
- [11]-[OVERLAY](.planning/view/overlay.md): Anchored floats, dismissable sheets, and the one command vocabulary palette and pointer share.
- [12]-[CHART](.planning/view/chart.md): Declared statistics, streaming series, and pivots behind one data-shape discriminant.
- [13]-[EXPORT](.planning/view/export.md): Every egress of rendered state — a serializer matrix making illegal pairs uncompilable.
- [14]-[SHELL](.planning/view/shell.md): Chrome as data — a region roster the pane solver realizes, one navigation vocabulary.
- [15]-[STATUS](.planning/view/status.md): Feedback between toast rail and field error — states from the one atom `Result` fold.
- [16]-[CONTENT](.planning/view/content.md): Prose and document editing as one derivation — block rows drive schema, codec, DOM, and wire.
- [17]-[MEDIA](.planning/view/media.md): Byte-borne presentation classes as rows — loading behavior is policy data, bytes arrive keyed.
- [18]-[CANVAS](.planning/view/canvas.md): Node/flow editing, worker graph layout, and temporal bands mirrored through one adapter atom.
- [19]-[PRESENCE](.planning/view/presence.md): Collaborative faces rendered over settled core verdicts — roster, cursors, comment threads.

[VIEWER]:
- [20]-[SCENE](.planning/viewer/scene.md): Scene custody behind the viewport port — acquisition, lighting, OpenPBR binding, GPU teardown.
- [21]-[GEO](.planning/viewer/geo.md): One shared WebGL context — maplibre camera and deck.gl layers as one pure value tree.
- [22]-[MARK](.planning/viewer/mark.md): One `GlobalId` selection set every pick pipeline folds into, published once per applied op.
- [23]-[PANEL](.planning/viewer/panel.md): AppUi surface programs admit one control tree and its exact layout closure before solve.
- [24]-[PROBE](.planning/viewer/probe.md): Benchmark and render evidence — canonical pixel identity compared, never gating.
- [25]-[REVIEW](.planning/viewer/review.md): Diff changes and BCF issues joined per `GlobalId` into board rows, tint, and reveal.

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
- `react-resizable-panels` — `Group`/`Panel`/`Separator` window-splitter surface behind the shell panes.

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
- `@tailwindcss/typography` — Prose element modifiers and the `--tw-prose-*` variable surface behind the content token bridge.

[DATA_SURFACES]:
- `@tanstack/react-table`
- `web-vitals` — DOM performance-global type augmentation the vital probe rows compile against.
- `@tanstack/react-virtual`
- `@tanstack/store` — Carries the table's own state vocabulary; `view/table`'s `Grid.edge` mints the adapter atom over the effect-atom fold.
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
- `prosemirror-model` — Schema, nodes, and marks as data; `view/content`'s roster derives the `Schema` from one row table.
- `prosemirror-state`
- `prosemirror-view` — Hosts the imperative editor DOM outside React's reconciler behind a scoped acquisition.
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
- `@xyflow/react` — Controlled node-flow engine; `view/canvas`'s `Canvas.useEdge` binds the controlled props over the one graph cell.
- `elkjs` — Layout solve as worker data through `elkjs/lib/elk-api`; the bare entry bundles the full CJS payload.

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
- `@loaders.gl/core` — Polymorphic `parse`/`load` decode engine; loaders ride the per-layer `loaders` prop, `registerLoaders` shipping `@deprecated`.
- `@loaders.gl/las`
- `@geoarrow/deck.gl-geoarrow`
- `@turf/turf`
- `@lume/kiwi`
- `typegpu`
- `@webgpu/types`
- `@types/geojson`

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the TypeScript registry, whose charters own the full contracts; `libs/typescript/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `effect`

[PLATFORM]:
- `@effect/platform`
- `@effect/platform-browser`
- `@effect/experimental`
