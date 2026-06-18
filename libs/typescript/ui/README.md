# [UI]

`ui` is the host-free browser UI/UX/components library of the TypeScript branch — the AppUi-analog, the lower browser-stratum library beneath the `platform` AppHost-analog. It is the single sanctioned `AtomBinding` reactive-binding spine, the headless interaction-role component vocabulary, and the leaf render-surfaces over the decoded C# wire. It holds no domain state and authors no decode: every domain read flows through a `projection` store and the one `AtomBinding`, every geometry read through the `interchange` `GeometryRail`, and every mutation leaves only through the `interchange` `CommandGateway`. Distinct in kind from the C# Avalonia AppUi (a desktop host-bound surface); this is a pure browser DOM/WebGL/WebGPU library. `ui/**` never imports `platform/**`. This README routes the `.planning/` design pages and registers every external package the folder draws on; the domain folder-map lives in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and the open work in `TASKLOG.md`.

## [1]-[ROUTER]

The design pages under `.planning/`, grouped by sub-domain in build order; each page is one transcription unit per eventual source file, and `ARCHITECTURE.md` carries the per-folder charter.

- binding: [atom-binding](.planning/binding/atom-binding.md) — the `AtomBinding` spine over `Atom.searchParam`/`kvs`/`family`/`pull`, the `UndoStack` fold, the `Result.builder` render chain.
- component-system: [role-behavior](.planning/component-system/role-behavior.md) — the behavior-bearing `_Roles`/`InteractionRole` vocabulary owner-block and the `RoleBehavior` contract; [accessibility-broadcast](.planning/component-system/accessibility-broadcast.md) — the live-region announce path over the role `politeness` column and the external-store toast queue.
- theming: [theme-tokens](.planning/theming/theme-tokens.md) — the `ThemeTokens` OKLCH scale and the `CssVarSync` Tailwind CSS-variable sync.
- observation: [observation-routes](.planning/observation/observation-routes.md) — `EvidenceTimelineRoute`, `BenchmarkRoute`, `CollectorPanel`.
- cartography: [geo-series-layer](.planning/cartography/geo-series-layer.md) — the `GeoSeriesLayer` union over maplibre and deck.gl.
- viewport: [glb-viewport](.planning/viewport/glb-viewport.md) — `GlbViewport`, `RendererBackend`, `ViewportResource`, the camera row.
- motion: [gesture-algebra](.planning/motion/gesture-algebra.md) — the shared `CameraGesture`/`GestureFold` pointer-gesture algebra; [view-transitions](.planning/motion/view-transitions.md) — the `SurfaceTransition` union over `<ViewTransition>` swap and `<Activity>` preserve.
- overlay: [floating-anchor](.planning/overlay/floating-anchor.md) — `FloatingAnchor` placement, the CSS Anchor bridge, the dismiss law.

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented, as one flat registry. Versions are centralized in the one TypeScript manifest (`pnpm-workspace.yaml` `catalog:`) and never pinned here.

[REACT]:
- react
- react-dom
- @types/react
- @types/react-dom
- babel-plugin-react-compiler
- react-compiler-runtime

[HEADLESS_A11Y]:
- react-aria
- react-aria-components
- react-stately
- @react-aria/live-announcer
- tailwindcss-react-aria-components
- @radix-ui/react-slot
- @radix-ui/react-label
- @radix-ui/react-separator
- @radix-ui/react-visually-hidden

[POSITIONING_GESTURE]:
- @floating-ui/react
- @floating-ui/react-dom
- @use-gesture/react

[THEMING]:
- colorjs.io
- tailwindcss
- tailwind-merge
- class-variance-authority

[CONTENT]:
- lucide-react
- cmdk
- vaul

[DATA_SURFACES]:
- @tanstack/react-virtual
- @tanstack/react-table
- maplibre-gl
- @deck.gl/core
- @deck.gl/layers
- @deck.gl/mapbox

[VIEWPORT]:
- three
- @google/model-viewer
- @webgpu/types

## [3]-[CROSS_CUTTING]

Branch-level cross-cutting packages consumed by this folder.

- `effect` — the `Effect.Service` and `Schema` substrate underlying the `AtomBinding` spine and result carriers
- `@effect-atom/atom` — the reactive-store primitives (`Atom.subscriptionRef`, `Atom.subscribable`, `Atom.kvs`, `Atom.family`)
- `@effect-atom/atom-react` — the React binding layer (`useAtom`, `useAtomValue`) over the atom store
- `@effect/opentelemetry` — the OTel trace/span integration at the observation boundary
- `@effect/platform` — the `KeyValueStore` contract the `AtomBinding` `Atom.kvs` offline cell binds at the runtime seam
- `isomorphic-dompurify` — the DOM-bound text sanitization for content render surfaces
