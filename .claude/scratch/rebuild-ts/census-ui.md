# [CENSUS_UI]

Read-only truthful census of `libs/typescript/ui` — the wave-4 interface package (`system`/`view` sub-domains) plus its sibling Nx project `viewer` (`viewer` sub-domain). No `src/` exists yet under either project — every eventual source file is authored today as a `.planning/*.md` design page whose fenced `typescript` blocks are the future module body. This is a pure planning corpus at this stage: `project.json` (folder) and `viewer/project.json` (folder) are the only real Nx artifacts on disk.

## [1]-[FILE_REGISTER]

| [FILE] | [FUTURE_MODULE] | [OWNED_CAPABILITY] | [PUBLIC_ENTRY] | [FENCE_LOC] | [MASS] |
| :--- | :--- | :--- | :--- | ---: | :--- |
| `.planning/system/token.md` | `ui/src/system/token.ts` | OKLCH color decode/ramp/contrast-gate (`colorjs.io`), the one `cn` class-merge rail, spacing/type/radius/ease/breakpoint scale tables emitted as Tailwind v4 `@theme` rows, theme kind + `data-theme` stamp | `Theme`, `cn`, `Scale` | 176 | dense |
| `.planning/system/act.md` | `ui/src/system/act.ts` | Discrete accessible interaction (react-aria) vs continuous gesture (`@use-gesture/react`) division, the `Motion` enter/exit class vocabulary over `tw-animate-css`, the View-Transitions/`flushSync`/`<Activity>` document rail | `Gesture`, `Motion`, `Transition` | 133 | dense |
| `.planning/system/atom.md` | `ui/src/system/atom.ts` | The one `@effect-atom` state binding: `Store.make` runtime root, `AtomHttpApi`/`AtomRpc` contract-binding law, selector/family/debounce projection law, host-fold live bridge (`subscriptionRef`/`subscribable`/`pull`), write-modality + async-fold law, `History` undo/redo | `Store`, `History` | 142 | dense |
| `.planning/system/intl.md` | `ui/src/system/intl.ts` | Zero-i18n-package locale plane: `I18nProvider` ambient locale, one per-locale native-`Intl` cache, `Format` value vocabulary (date/number/list/relative/collate), `Message` catalog decode + total plural/select fold with BCP-47 fallback | `Format`, `Message` | 178 | dense |
| `.planning/system/primitive.md` | `ui/src/system/primitive.ts` | The headless spine: `Primitive.styled` (cva × `composeRenderProps` × `cn`), the RAC family roster law, toast queue + live-announce, error-boundary failure envelope, DOMPurify sanitize gate | `Primitive` | 60 | lean |
| `.planning/view/form.md` | `ui/src/view/form.ts` | Schema-driven forms: one kernel `Schema` decoding wire AND validating live fields via `standardSchemaV1`, full RAC field roster bound to kernel scalars, store-awaited submit round-trip, per-field draft cursors | `Form` | 55 | lean |
| `.planning/view/table.md` | `ui/src/view/table.ts` | The one data grid: one-atom `TanStack` `TableState` slice, static/banded (`Feed.Document`) column fold, row-model roster (sort/filter/facet/group/paginate), aria grid semantics, `@tanstack/react-virtual` windowing with pinned-range union | `Grid` | 95 | lean-mid |
| `.planning/view/overlay.md` | `ui/src/view/overlay.ts` | The one overlay owner: floating-ui anchored positioning, vaul drag sheet, cmdk command palette (`Overlay.Command` vocabulary), presence-cursor cohort over `VirtualElement` anchoring | `Overlay` | 43 | lean |
| `.planning/viewer/scene.md` | `ui/viewer/src/scene.ts` | GLB scene: `GlbViewport` port, `GlbFault` closed reason family, `three`/`model-viewer` backend selection, residency graft with animation mixer roster, draw-call collapse (merge/instanced/batched), OpenPBR→`MeshPhysicalMaterial` field-mirror bind, `@deck.gl/mesh-layers` georeferenced instancing, model-viewer embed row | `Glb`, `GlbFault`, `GlbViewport`, `Instanced`, `Pbr` | 339 | heaviest |
| `.planning/viewer/geo.md` | `ui/viewer/src/geo.ts` | The one geospatial surface+camera owner: maplibre `Map` + interleaved `MapboxOverlay`, `Camera` state/intent vocabulary (`calculateCameraOptionsFromTo`-solved `LookAt`), pure screen↔world projection, atom-derived deck layer rows (GeoJSON/arrow-fan/tiles/DGGS cells/trips/WMS), 8-capability extension roster, turf planar-ops peer, feature-state style echo | `Camera`, `Geo`, `GeoFault` | 266 | heaviest |
| `.planning/viewer/mark.md` | `ui/viewer/src/mark.ts` | The one `GlobalId` selection plane: closed op vocabulary over `HashSet` riding `History`, per-backend pick-to-`GlobalId` resolution (deck/three/maplibre/turf-lasso), echo projections (highlight/feature-state/grid/reveal), BCF topic pin anchoring, viewpoint-restore fold (camera intent + selection op + receipt), lifecycle tone board | `Mark`, `Selection` | 152 | dense |
| `.planning/viewer/panel.md` | `ui/viewer/src/panel.ts` | The one wire materializer: livewire triple fold (`BindingStatus`/`CoercedValue`/`WriteReceipt`) into optimistic board rows, phase tone vocabulary, `ControlIntent` union-derived exhaustive sink routing, Cassowary (`@lume/kiwi`) layout solve with edit-variable drag and four-axis determinism law | `Panel` | 166 | dense |
| `.planning/viewer/probe.md` | `ui/viewer/src/probe.ts` | The one render-evidence owner: deck `DeckMetrics` + frame-timer bounded seed fold, local host-fingerprint mirror, claim-vs-local label-keyed comparison board, deterministic framebuffer capture + content-key hash delegate, verdict/tone evidence rows (never gates) | `Probe` | 148 | dense |

`README.md` (92 lines) is the folder router plus full domain-package manifest (React substrate, state binding, headless, style tokens, motion/gesture, grid/charts, spatial). `ARCHITECTURE.md` (45 lines) carries the codemap, cross-package seam table, and boundary law. `IDEAS.md`/`TASKLOG.md` are both empty (`(none)` in OPEN and CLOSED) — no forward pool or open work recorded.

## [2]-[API_ROSTER]

59 catalog files under `.api/`, one per admitted package, roughly proportional in depth (9-26KB) to package surface complexity:

| [CATALOG] | [PACKAGE] | [DEPTH_SIGNAL] |
| :--- | :--- | :--- |
| `react.md`, `react-dom.md`, `types-react.md`, `types-react-dom.md` | React 19 substrate + type declarations | deep (22-25KB each) |
| `babel-plugin-react-compiler.md`, `react-compiler-runtime.md` | React Compiler toolchain | deep/mid (17KB, 11KB) |
| `react-error-boundary.md` | error-boundary envelope | mid (13KB) |
| `effect-atom-atom.md`, `effect-atom-atom-react.md` | `@effect-atom` state binding | deep (24KB, 17KB) |
| `react-aria.md`, `react-aria-components.md`, `react-stately.md`, `react-aria-live-announcer.md` | headless interaction/collection/state machinery | deep (19-21KB) except live-announcer (mid, 8KB) |
| `radix-ui-react-slot.md`, `radix-ui-react-label.md`, `radix-ui-react-separator.md`, `radix-ui-react-visually-hidden.md` | radix primitives | lean (6-10KB) |
| `cmdk.md`, `vaul.md` | palette / drag sheet | mid (10-13KB) |
| `floating-ui-react.md`, `floating-ui-react-dom.md` | anchored positioning | deep/mid (22KB, 13KB) |
| `tailwindcss.md`, `tailwind-merge.md`, `tw-animate-css.md`, `tailwindcss-react-aria-components.md`, `class-variance-authority.md`, `clsx.md` | styling rail | mid (6-16KB) |
| `colorjs.io.md` | OKLCH color engine | deep (17KB) |
| `lucide-react.md` | icon-as-identity | lean (9KB) |
| `isomorphic-dompurify.md` | sanitize gate | mid (11KB) |
| `motion.md`, `use-gesture-react.md` | motion/gesture | mid-deep (12KB, 18KB) |
| `tanstack-react-table.md`, `tanstack-react-virtual.md` | grid modeling + windowing | deep (17KB, 16KB) |
| `apache-arrow.md` | columnar IPC | deep (15KB) |
| `perspective-dev-client.md`, `perspective-dev-viewer.md` | grid/chart engine | mid (8-11KB) |
| `uplot.md`, `observablehq-plot.md`, `d3.md` | charting | mid (10-12KB) |
| `visx-axis.md`, `visx-group.md`, `visx-responsive.md`, `visx-scale.md`, `visx-shape.md` | visx chart primitives | lean (3-6KB each — thinnest tier) |
| `three.md`, `webgpu-types.md` | 3D renderer + WebGPU ambient types | deep (26KB, 19KB) |
| `google-model-viewer.md` | zero-GL embed backend | deep (17KB) |
| `maplibre-gl.md` | basemap/camera authority | deep (21KB) |
| `deck.gl-core.md`, `deck.gl-layers.md`, `deck.gl-geo-layers.md`, `deck.gl-mesh-layers.md`, `deck.gl-extensions.md`, `deck.gl-mapbox.md` | deck.gl layer/extension/overlay family | deep-mid (10-23KB) |
| `geoarrow-deck.gl-geoarrow.md` | columnar geometry layer | mid (14KB) |
| `turf-turf.md` | planar geometry ops | deep (20KB) |
| `lume-kiwi.md` | Cassowary constraint solver | mid (11KB) |
| `typegpu.md` | WebGPU TSL/compute authoring | mid (11KB) |

Every domain package named in `README.md#[02]-[DOMAIN_PACKAGES]` has a corresponding `.api/` catalog; no catalog exists for an unlisted package (no orphan catalogs found).

## [3]-[CAPABILITY_MAP]

The realized capability matches the README/ARCHITECTURE claims closely — this folder is unusually well-aligned because both governing docs were authored recently (timestamps 4-40 minutes old on most files) against the same page set. Verified rows:

- `system` five-page roster (`token`/`act`/`atom`/`intl`/`primitive`) matches the codemap 1:1; every claimed public export (`Theme`/`cn`/`Scale`, `Gesture`/`Motion`/`Transition`, `Store`/`History`, `Format`/`Message`, `Primitive`) is present in its page's terminal export block.
- `view` three-page roster (`form`/`table`/`overlay`) matches; `Form`, `Grid`, `Overlay` all export as claimed.
- `viewer` five-page roster (`scene`/`geo`/`mark`/`panel`/`probe`) matches; all claimed exports (`Glb`/`GlbFault`/`GlbViewport`/`Instanced`/`Pbr`, `Camera`/`Geo`/`GeoFault`, `Mark`/`Selection`, `Panel`, `Probe`) are present.
- The `ARCHITECTURE.md#[02]-[SEAMS]` table's nine cross-package seams are each traceable to a concrete owning cluster in the corresponding page (e.g., `view/table ← typescript:core/state` matches `table.md`'s `Feed.Document` band-driven `COLUMN_PLANE`; `viewer/scene ← csharp:Rasm.Materials` matches `scene.md`'s `APPEARANCE_BIND` `Pbr` cluster).
- `README.md#[02]-[DOMAIN_PACKAGES]` categories map cleanly onto the `.api/` roster with zero missing or orphaned catalogs.

Two discrepancies found, both minor and consumer-blocking rather than false claims:

- `README.md#[01]-[ROUTER]` lists 13 pages in order `token/act/atom/intl/primitive/form/table/overlay/scene/geo/mark/panel/probe`, but `ARCHITECTURE.md#[01]-[DOMAIN_MAP]` codemap groups `scene/geo/mark/panel/probe` under `viewer/src/` without listing `token`'s downstream consumer relationship to `viewer` inline in the router — the `system/token → typescript:ui/viewer` projection seam is documented only in the seams table, not cross-referenced from the router itself. Not a defect, but the router is the weaker of the two navigation surfaces for this one cross-project seam.
- Neither `IDEAS.md` nor `TASKLOG.md` carries any content (`(none)` in both OPEN and CLOSED sections of each) — the folder has zero recorded forward-looking capability pool or task backlog at census time, meaning downstream phases inherit no pre-existing idea/task rows to reconcile against; any capability gap this campaign finds is net-new, not a reopened card.

No capability is claimed in README/ARCHITECTURE that is absent from a `.planning` page, and no `.planning` page claims a public surface its terminal export omits.
