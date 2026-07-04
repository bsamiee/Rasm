# [UI]

`ui` is the W4 component-capability folder of the TypeScript branch — two Nx projects in one folder: the `ui` core shipping atoms, tokens, interaction, localization, and headless views as capability rows on the React 19 spine (react-compiler enabled, memoization compiled, never hand-written), and `ui/viewer`, the second Nx project (`scope:viewer`) carrying the spatial/GLB/geo/BCF tier so heavy render deps are compile-time excluded from the non-spatial majority. `@effect-atom` is the one state binding (`ONE_FOLD_ONE_BINDING`); react-aria is the headless primitive spine every view row composes; `intl` owns the localization plane as Schema-typed message-catalog rows with plural/select folds over native `Intl` — zero i18n package, catalogs are app data keyed by the kernel `Locale` brand. Components are capability, boot is runtime: `ui` never imports `browser`; where a component needs a runtime capability, `ui` declares a port record (`GlbViewport` decode-worker residency) and `browser` provides the Layer at app composition. `ui` types decoded wire values through the `wire` `#vocab` subpath only. The folder map and seam record live in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

## [1]-[ROUTER]

[CORE]:
- [01]-[BINDING](.planning/atom/binding.md): the `@effect-atom` one-binding law (`ONE_FOLD_ONE_BINDING`) — the one state binding, with `AtomHttpApi`/`AtomRpc` direct-binding rows.
- [02]-[DERIVE](.planning/atom/derive.md): derived atoms/selectors over `state` folds plus undo/redo stack folds.
- [03]-[THEME](.planning/token/theme.md): design tokens and theming rows.
- [04]-[SCALE](.planning/token/scale.md): spacing/typography scale vocabulary and motion token rows (tw-animate).
- [05]-[TRANSITION](.planning/act/transition.md): the native View Transitions API owner; `<ViewTransition>`/`<Activity>` stay gated as the upgrade row, `act` degrading to the native API.
- [06]-[GESTURE](.planning/act/gesture.md): interaction/gesture rows (react-aria).
- [07]-[PRIMITIVE](.planning/view/primitive.md): the react-aria headless component spine plus live-region announce/toast rows.
- [08]-[COMPOSE](.planning/view/compose.md): composition/slot patterns, Schema→aria `FormBinding`/picker rows, command-palette (cmdk), table/virtual collection, floating-anchor/sheet, and presence-cursor cohort rows.
- [09]-[MESSAGE](.planning/intl/message.md): Schema-typed message-catalog rows keyed by the kernel `Locale` brand, plural/select folds over native `Intl` — catalogs are app data, zero i18n package.
- [10]-[FORMAT](.planning/intl/format.md): number/date/list/relative-time format rows composing react-aria `I18nProvider`/`useLocale` over native `Intl`.

[VIEWER] — the second Nx project, `scope:viewer`:
- [11]-[GLB](.planning/viewer/scene/glb.md): `GLB_VIEWPORT` scene residency and three rows; consumes the `browser` decode-worker port; meshopt decode gated.
- [12]-[APPEARANCE](.planning/viewer/scene/appearance.md): OpenPBR appearance binding over `wire#vocab` appearance.
- [13]-[LAYERS](.planning/viewer/geo/layers.md): maplibre/deck.gl geo layers, tile-streaming rows over the wire tile grid, and turf planar ops (WKB decode stays in `wire`).
- [14]-[PROJECT](.planning/viewer/geo/project.md): projection/camera sync rows.
- [15]-[BCF](.planning/viewer/mark/bcf.md): BCF topic/viewpoint anchors (`GlobalId`).
- [16]-[SELECTION](.planning/viewer/mark/selection.md): `GlobalId` selection sets.
- [17]-[RECEIPT](.planning/viewer/probe/receipt.md): `RenderReceipt` frame-hash probes.
- [18]-[BENCHMARK](.planning/viewer/probe/benchmark.md): `BenchmarkClaim`/`HostFingerprint` probes.
- [19]-[PANEL_BINDING](.planning/viewer/panel/binding.md): livewire binding panels.
- [20]-[CONTROL](.planning/viewer/panel/control.md): `ControlIntent` panels.
- [21]-[LAYOUT](.planning/viewer/panel/layout.md): `@lume/kiwi` Cassowary layout re-solve to identical positions.

## [2]-[DOMAIN_PACKAGES]

Every UI-domain library the folder uses, planned or implemented. Versions are centralized in the one `pnpm-workspace.yaml` catalog and never pinned here; API evidence lives in the adjacent `.api/` folder. The `[VIEWER_*]` groups are `scope:viewer` project-local — admitted only by the `ui/viewer` Nx project and compile-time excluded from the core.

[REACT_SPINE]:
- `react`
- `react-dom`
- `@types/react`
- `@types/react-dom`
- `react-error-boundary`

[COMPILER]:
- `babel-plugin-react-compiler`
- `react-compiler-runtime`

[STATE_BINDING]:
- `@effect-atom/atom`
- `@effect-atom/atom-react`

[HEADLESS_ARIA]:
- `react-aria`
- `react-aria-components`
- `react-stately`
- `@react-aria/live-announcer`

[COMPOSITION]:
- `@floating-ui/react`
- `@floating-ui/react-dom`
- `@radix-ui/react-label`
- `@radix-ui/react-separator`
- `@radix-ui/react-slot`
- `@radix-ui/react-visually-hidden`
- `@tanstack/react-table`
- `@tanstack/react-virtual`
- `cmdk`
- `vaul`

[INTERACTION]:
- `@use-gesture/react`

[TOKENS]:
- `tailwindcss`
- `tailwindcss-react-aria-components`
- `tailwind-merge`
- `tw-animate-css`
- `class-variance-authority`
- `clsx`
- `colorjs.io`
- `lucide-react`

[SANITIZE]:
- `isomorphic-dompurify`

[VIEWER_SCENE] — `scope:viewer`:
- `three`
- `@google/model-viewer`
- `@webgpu/types`

[VIEWER_GEO] — `scope:viewer`:
- `maplibre-gl`
- `@deck.gl/core`
- `@deck.gl/layers`
- `@deck.gl/mesh-layers`
- `@deck.gl/geo-layers`
- `@deck.gl/extensions`
- `@deck.gl/mapbox`
- `@geoarrow/deck.gl-geoarrow`
- `apache-arrow`
- `@turf/turf`

[VIEWER_LAYOUT] — `scope:viewer`:
- `@lume/kiwi`

## [3]-[SUBSTRATE_PACKAGES]

The branch substrate this folder consumes; the registry lives in `libs/typescript/.planning/README.md` and the catalogues at `libs/typescript/.api/`.

- `effect` — rails, `Schema` (message catalogs, `FormBinding`), `Layer`, `Match` across every row.
- `@effect/platform` — the HttpApi/Rpc client contracts the `AtomHttpApi`/`AtomRpc` direct-binding rows compose.
- `@effect/vitest` — the dev-plane spec runner binding the `@rasm/ts-testkit` law combinators (`tests/typescript/_testkit`) to this folder's colocated specs.
