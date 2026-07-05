# STACK-UI — ultra-stacking dossier for libs/typescript/ui

Verified against the on-disk catalogs (`libs/typescript/ui/.api/`, `libs/typescript/.api/`), the 13 design pages, `census-ui.md`, `admissions.md`, and `prefetch-ui-iac.md`. Every spelling below is copied from a catalog member table or a page fence; a phantom here is a defect. Scope: what the improver stacks INTO existing owners plus the earned new-owner candidates. All admissions are APPLIED (pinned, catalogs on disk) — catalogs are the primary surface, the prefetch dossier is the ruling layer no catalog fully carries.

The single loudest finding: **three admitted package families with full catalogs are consumed by ZERO page** — `motion` (12.42.2), the charting stack (`uplot`/`@observablehq/plot`/`d3`/`@visx/*`/`@perspective-dev/*`), and `typegpu` (0.11.9). The catalogs even prescribe the exact stacking seam (motion→`system/act`, perspective→a pivot surface, typegpu→a compute lane) and no page wires it. These are not underutilization — they are unrealized admissions.

---

## [A] UNDERUTILIZED MEMBERS PER CATALOG — exact spelling → owning page

### A1 · `motion` (motion.md) — the CONTINUOUS plane, entirely absent from `system/act`

`system/act.md` owns motion but imports zero `motion`; MOTION_ROWS is `tw-animate-css` classes, DOCUMENT_RAIL is bare `startViewTransition`+`flushSync`. The catalog `[05]-[INTEGRATION]` explicitly assigns motion to `system/act#MOTION_ROWS` (continuous) and `#DOCUMENT_RAIL` (`animateView`). Land into `system/act.md` as a new `CONTINUOUS_MOTION` cluster beside MOTION_ROWS:

- `motionValue(init)` / `useMotionValue(initial)` — render-free animated cell (bound `style={{ x }}`) — the spring/pointer state cell React never re-renders for. → `system/act` (viewer camera-follow, panel drag readouts).
- `useSpring(source, options?)` — spring-follows a MotionValue/number/unit-string. → `system/act` continuous owner; the physical alternative to `Scale.ease` cubic-beziers for interruptible motion.
- `useTransform(input, inputRange, outputRange, options?)` / `useTransform(() => expr)` — the derive fold over source values. → `system/act`.
- `useScroll({ container?, target?, offset? })` → `{ scrollX, scrollY, scrollXProgress, scrollYProgress }` — the ONE scroll-animation engine (four-browser parity; catalog rejects a second `ScrollTimeline`). → `system/act` (a scroll-reveal law the folder currently has NO owner for).
- `useVelocity(value)` / `useTime()` / `useAnimationFrame(cb)` — velocity tracking, elapsed-time value, per-frame callback. → `system/act`; `useVelocity` feeds momentum-aware overlay dismissal.
- `useMotionValueEvent(value, event, cb)` — `change`/`animationStart`/`animationComplete` without an effect; the read-seam when a threshold crossing becomes real atom state. → `system/act` (the sanctioned MotionValue→atom bridge).
- `animateView(update, options?)` → `ViewTransitionBuilder` with `interrupt?: "wait" | "immediate"`; builder verbs `.add(a,b)` `.crop()` `.group()` `.class()` `.layout()` `.enter()` `.exit()` `.new()` `.old()` `.updateTarget()`. → `system/act#DOCUMENT_RAIL` — the typed spring layer over `startViewTransition` with the interruption handling the native API lacks. DOCUMENT_RAIL's `Transition.run` should upgrade to this as its middle tier (native → `animateView` → canary `<ViewTransition>`).
- `AnimatePresence` (`mode: "sync" | "wait" | "popLayout"`, `onExitComplete`, `propagate`) + `usePresence()`/`useIsPresent()` — exit choreography for unmount. → `system/act` (the physics alternative to RAC `exiting:` variants for surfaces needing interruption).
- `layout` / `layoutId` props + `LayoutGroup` — shared-element morph across unrelated trees, no wrapper. → `system/act` (grid↔detail, palette↔result morphs — currently no owner).
- `MotionConfig` (`transition`, `reducedMotion: "user"|"always"|"never"`, `nonce`) / `useReducedMotion()` — subtree policy; mirrors the `matchMedia` reduced-motion law every act row already hand-checks. → `system/act` (collapses the repeated `matchMedia("(prefers-reduced-motion)")` reads in act.md's `_eligible` and MOTION_ROWS' `motion-reduce:` into one subtree owner).
- Entry-split law (motion.md `[01]`): `motion/react` full, `motion/react-mini` (`useAnimate` only, ~2.3kB), `motion` vanilla (`animate`/`scroll`/`stagger` — drives three/canvas). The `delay` unit flips seconds↔ms across entries — pin the entry. → `system/act` records the cost ladder as a decision row.

### A2 · `@effect-atom/atom` (effect-atom-atom.md) — combinators `system/atom` names but never wires

`system/atom.md` uses `Atom.make/map/mapResult/transform/family/debounce/kvs/searchParam/pull/subscriptionRef/subscribable/optimistic/writable/batch/toStreamResult`. Unused admitted members with a clear home:

- `AtomRef.make(value)` / `AtomRef.collection(items)` (+ types `AtomRef<A>`/`ReadonlyRef<A>`/`Collection<A>`) — fine-grained mutable cursor / ordered ref collection for per-item subscriptions WITHOUT re-running the owning atom. → `view/form.md` DRAFT_CURSORS references `useAtomRefProp` but `system/atom` never lands `AtomRef` as a member; land it in atom.md as the cursor primitive form.md and table.md both need, and as the `History` redo-cursor.
- `Atom.optimisticFn({ …, reducer })` — the reducer-carrying optimistic write (atom.md uses only bare `Atom.optimistic`). → `system/atom#WRITE_AND_FOLD` (multi-step optimistic reconcile, e.g. form field batches).
- `Result.builder(self).onInitial(…).onSuccess(…).orNull()` + `Result.matchWithWaiting`/`matchWithError` — the fluent/waiting-aware folds (atom.md uses only `Result.match`). → `system/atom#WRITE_AND_FOLD` law rows; `matchWithWaiting` folds the `waiting` flag into a dedicated arm (the stale-while-revalidate render).
- `Atom.refreshOnWindowFocus` / `Atom.makeRefreshOnSignal(signal)` / `Atom.windowFocusSignal` — refresh triggers named in atom.md WRITE_AND_FOLD prose but no fence carries them. → `system/atom` (the visibility-refresh row).
- `Atom.withReactivity(keys)` + `AtomHttpApi.query(..., { reactivityKeys, timeToLive })` / `AtomRpc.query(..., { reactivityKeys, timeToLive })` — typed invalidation graph (the `@effect/experimental` `Reactivity` peer). atom.md mentions `reactivityKeys` in REMOTE_BINDING prose but no page shows a mutation's keys invalidating a query. → `system/atom#REMOTE_BINDING` (the typed cache-invalidation law, replacing any string key).
- `Atom.withFallback(fallbackAtom)` / `Atom.initialValue(value)` / `Atom.keepAlive` / `Atom.setIdleTTL(duration)` — seed/lifetime (probe.md prose says host fingerprint "rides a keepAlive atom" but never spells `Atom.keepAlive`). → `viewer/probe` (fingerprint pin), `viewer/geo` (tile-layer atoms).
- `Registry.modify(atom, f)` (returns value + next state atomically) / `Registry.getResult` / `Atom.getResult(self)` — imperative store reads for the panel/browser seam. → `viewer/panel` (imperative solve read), atom.md STORE_ROOT boundary.
- `Hydration.dehydrate(registry, options?)` / `.toValues(state)` / `.hydrate(registry, state)` + `Atom.serializable(self, { schema })` / `Atom.withServerValue` — SSR handoff. atom.md STORE_ROOT prose names `Hydration`/`HydrationBoundary`/`Atom.serializable` but no fence carries the dehydrate/hydrate pair. → `system/atom#STORE_ROOT` (make the SSR law a real fence, not prose).

### A3 · `react` (react.md) — canary + 19.2 members no page imports

- `<ViewTransition>` (CANARY) — props `name`/`default`/`enter`/`exit`/`update`/`share` + `onEnter`/`onExit`/`onUpdate`/`onShare(instance, types)`; fires only inside `startTransition`/`useDeferredValue`/a Suspense reveal, must sit directly above the DOM node; styles land on `::view-transition-old/new(.class)`. Admitted per operator override (react.md `channel: CANARY`, prefetch `[OPERATOR_OVERRIDE]`). act.md DOCUMENT_RAIL marks it `[R16]`/future — the override RE-ADMITS it as a live tier. → `system/act#DOCUMENT_RAIL` (top tier of the ladder; requires one `/// <reference types="react/canary" />`).
- `addTransitionType(type)` (CANARY) — called in the SAME `startTransition`; keys the per-type class maps on `enter`/`exit`/`update`/`share`. → `system/act#DOCUMENT_RAIL`.
- `useEffectEvent(fn)` (stable 19.2) — non-reactive slice of an effect; keeps `useEffect` deps honest under the compiler. Zero pages use it despite heavy subscription/DOM-measurement seams. → `viewer/geo` (map event folds), `viewer/scene` (loop re-arm), `view/overlay` (anchor measurement).
- `useDeferredValue(value, initialValue)` — seeded deferred value (search→virtualized list). → `view/table` (filter query), `view/overlay` (palette query) — currently only `Atom.debounce` is used.
- `<Activity mode="visible"|"hidden">` (stable 19.2) — act.md DOCUMENT_RAIL and scene.md loop-park cite it in prose; no page renders it. → `system/act` (make it a real row), `viewer/scene` (the hidden-viewport loop park).
- `use(usable)` + `lazy(() => import(...))` + `Suspense` — react.md prescribes the `scope:viewer` heavy tier (`three`/`deck.gl`) is `lazy` behind `Suspense`; no page shows the code-split boundary. → `viewer/scene`+`viewer/geo` (the split-load law).
- `cache(fn)` / `cacheSignal()` — RSC-only, inert client-side — record as boundary, do NOT ship as client capability.

### A4 · `@tanstack/react-table` (tanstack-react-table.md) — models/registries `view/table` omits

- `getFacetedMinMaxValues()` — numeric range feeding a slider filter's bounds (table.md uses only `getFacetedUniqueValues`). → `view/table#DERIVE_MODELS` (range-filter affordance).
- `RowPinningState` + `getRowModel().rows` row-pinning — the `_SLICE` product in table.md carries `columnPinning` but NOT `rowPinning`; WINDOWING's pinned-range union has no row-pin source slice. → `view/table#STATE_FOLD` (add the slice; feeds `Grid.range`).
- `_features: [RowSelection, ColumnPinning, …]` explicit composition (from `ColumnFiltering`/`RowSorting`/`GlobalFiltering`/`ColumnGrouping`/`ColumnOrdering`/`ColumnSizing`/`ColumnVisibility`/`GlobalFaceting`/`RowPagination`/`RowPinning`) — bespoke subset / custom feature. → `view/table` growth row (banded-source tables that need a custom feature).
- `TableMeta<TData>` / `ColumnMeta<TData, TValue>` (augmentable) — table.md uses `meta` inline on `_banded` but never declaration-merges the typed interface; the `GlobalId` accessor + edit policy belong in typed `ColumnMeta`. → `view/table#COLUMN_PLANE`.
- `memo(deps, fn, opts)` (table util) — memoize a derivation at the FFI boundary the react-compiler can't see. → `view/table` (banded column fold).

### A5 · `@floating-ui/react` (floating-ui-react.md) — interaction hooks `view/overlay` omits

overlay.md uses `useFloating`/`useFloatingRootContext`/`useInteractions`/`useClick`/`useDismiss`/`useRole`/`useHover`+`safePolygon`/`useListNavigation`/`useTypeahead`/`useTransitionStyles`/`useMergeRefs`/`FloatingFocusManager`/`FloatingPortal`/`FloatingTree`/`FloatingNode`/`autoUpdate`/`offset`/`flip`/`shift`/`size`/`VirtualElement`. Unused:

- `useClientPoint(ctx, props?)` — cursor-follow anchoring (a tooltip/cursor at the pointer). → `view/overlay#PRESENCE_COHORT` (the presence cursor is the exact use; currently a hand-built `VirtualElement`).
- `FloatingArrow` + `arrow` middleware (`FloatingArrowProps`: `context`/`width`/`height`/`tipRadius`) — the pointing-arrow SVG. → `view/overlay#ANCHOR_HOST` (tooltip/popover pointer).
- `FloatingDelayGroup` / `NextFloatingDelayGroup` + `useDelayGroup`/`useNextDelayGroup` — shared hover-delay across sibling tooltips. → `view/overlay#ANCHOR_HOST` (toolbar tooltip group).
- `Composite` / `CompositeItem` (`orientation`/`loop`/`cols`/`activeIndex`) — single-tab-stop roving nav for a toolbar/grid without the full float stack. → `view/overlay` or `system/primitive` (RAC `Toolbar` peer).
- `inner(props)` / `useInnerOffset` — anchor a tall scrollable list (combobox) at the active item. → `view/overlay#PALETTE` (virtualized combobox palette; stacks with `@tanstack/react-virtual`).
- `useFloatingPortalNode(props?)` / `useFloatingParentNodeId()` / `useFloatingNodeId(parentId?)` — portal-container resolution + tree ids. → `view/overlay` (nested-float wiring).
- `FloatingOverlay` (`lockScroll`) — scroll-lock backdrop behind a modal float (overlay.md relies on vaul/RAC for this). → `view/overlay#ANCHOR_HOST` (modal anchored dialog).
- `OpenChangeReason` union (`'click'|'hover'|'focus'|'focus-out'|'escape-key'|'outside-press'|'reference-press'|'ancestor-scroll'|'list-navigation'|'safe-polygon'`) — branch dismiss behavior on CAUSE. → `view/overlay` (reason-keyed dismiss policy, not a boolean).

### A6 · `three` (three.md) — renderer/material/compute members `viewer/scene` omits

scene.md uses `WebGLRenderer`/`WebGPURenderer`/`GLTFLoader`/`AnimationMixer`/`Clock`/`InstancedMesh`/`BatchedMesh`/`mergeGeometries`/`MeshPhysicalMaterial`/`PMREMGenerator`(prose)/light classes/`setAnimationLoop`/`compileAsync`(prose)/`initTexture`(prose)/`readRenderTargetPixelsAsync`(probe). Unused with a home:

- `AgXToneMapping` / `NeutralToneMapping` — physically-faithful HDR tone maps (scene `_OUTPUT` hardcodes `ACESFilmicToneMapping`). → `viewer/scene#BACKEND_SELECT` (tone-map as a policy row, not one constant).
- `RectAreaLight` / `LightProbe` — analytic area light / probe (scene `_RIG` has ambient/hemisphere/directional only). → `viewer/scene#BACKEND_SELECT` rig-table rows.
- `PostProcessing` / `RendererUtils` (`three/webgpu`) + `mrt({...})` (`three/tsl`) — the WebGPU post chain whose framebuffer feeds the receipt frame-hash; probe.md CAPTURE_FOLD names `three/tsl`'s `mrt` in prose. → `viewer/probe#CAPTURE_FOLD` (G-buffer capture as an MRT target row).
- `ComputeNode` / `StorageBufferNode` / `StorageBufferAttribute` / `IndirectStorageBufferAttribute` + TSL `instancedArray`/`instanceIndex`/`storage`/`textureStore`/`uniform`/`uniformArray`/`deltaTime`/`time`/`Loop`/`If`/`workgroupBarrier`/`workgroupArray`/`subgroupAdd`/`atomicAdd` — GPU compute (scene BACKEND_SELECT prose says "per-instance culling and skinning land as computeAsync passes"; no fence). → `viewer/scene#BACKEND_SELECT` (the WebGPU compute lane, WebGL-gated).
- `ArcballControls` / `MapControls` — camera controls beyond `OrbitControls` (geo.md CAMERA prose folds `OrbitControls` only). → `viewer/geo#CAMERA` backend-adapter rows.
- `LoadingManager` — folds progress/error across the GLB dependency set (scene.md constructs `GLTFLoader` bare). → `viewer/scene#RESIDENCY_GRAFT` (per-graft progress).
- `CompressedArrayTexture` / `DataTexture` / `RGBELoader` — HDR/array texture ingest for IBL beyond `RoomEnvironment`. → `viewer/scene` (IBL from an authored equirect).

### A7 · `effect` (effect.md, substrate) — streaming/concurrency/cache the viewer underuses

- `Stream.broadcast` / `Stream.partition` / `Stream.merge` / `Stream.zipLatest` — multi-consumer fan-out. scene.md's `port.arrivals` stream is `runForEach`'d ONCE (graft); it should broadcast to the graft AND a probe preload census AND a residency telemetry tap. → `viewer/scene`, `viewer/probe`.
- `Stream.groupedWithin` / `Stream.throttle` / `Stream.debounce` — batch windows / rate shaping on the wire feed. → `viewer/panel#EVENT_FOLD` (the livewire triple feed currently folds per-event; `groupedWithin` coalesces bursts).
- `PubSub.bounded` / `Queue.sliding` / `Mailbox.make` — bounded fan-out channel. → `viewer/mark` (pick-result fan to echo consumers), `viewer/probe` (metric sink).
- `Cache.make({ capacity, timeToLive, lookup })` / `RcMap.make` / `KeyedPool.make` — TTL+refcount cache. → `viewer/geo` (tile-data cache beyond deck's `maxCacheByteSize`), `viewer/scene` (decoded-texture pool). geo.md relies on deck's own cache only.
- `RequestResolver.makeBatched` / `Request.Class` — de-dup/batch. → `viewer/mark#PICK_PIPES` (batch many pick→GlobalId decodes under one resolver).
- `SubscriptionRef.make` / `Subscribable` — the read-only `{ get, changes }` projection every host plane publishes; atom.md LIVE_BRIDGE binds these but the viewer scene/geo could publish their OWN settled-camera/residency Subscribable for cross-app taps. → `viewer/geo#CAMERA` (publish settled state as a Subscribable, not only atom).
- `FiberRef.make` / `FiberRef.locallyScoped` — fiber-local context (tenant/locale/app-id) propagated across `fork` without threading. → the per-app/per-tenant identity law (campaign emphasis #3) — no viewer stream currently carries a scoping FiberRef.

### A8 · `@perspective-dev/viewer` + `@perspective-dev/client` (perspective-dev-viewer.md) — a whole engine, unused

No page loads `<perspective-viewer>`. Members: `load(table)` / `restore(update)` / `save(): ViewerConfig` / `toggleConfig` / `getTable`/`getView`/`getClient` / `setThrottle(ms)` / `setAutoPause` / `export`/`download`/`copy` / `delete()`; events `perspective-config-update` (echo seam), `perspective-click` (`{ config, row, column_names }`), `perspective-select`. Plugin roster `viewer-datagrid` + `viewer-charts`. → NEW owner (see [D1]) — the user-driven live pivot/aggregate surface distinct from `view/table`'s fixed-shape `Grid`.

### A9 · `@effect/platform-browser` (substrate, effect-platform-browser.md) — Web-API service ports no page declares

- `Clipboard.Clipboard` / `Clipboard.layer` / `ClipboardError` — copy/paste (read/write text+blob). → `view/overlay#PALETTE` (copy-command), `viewer/probe` (copy evidence board). Declared as a `ui`-owned port, `browser` satisfies.
- `Geolocation.Geolocation` / `Geolocation.layer` / `Geolocation.watchPosition(opts?)` / `GeolocationError` — position + watch stream. → `viewer/geo#SURFACE` (the effect-rail peer of maplibre `GeolocateControl`, currently DOM-only).
- `Permissions.Permissions` / `Permissions.layer` / `PermissionsError` — permission-state query/observe. → `viewer/geo` (geolocation gate), `view/overlay` (clipboard gate).

---

## [B] NEVER-USED ADMITTED CAPABILITY THE FOLDER CONCEPT DEMANDS

1. **The entire charting stack** (`@visx/{scale,shape,axis,group,responsive}` 4.0.0, `@observablehq/plot` 0.6.17, `d3` 7.9.0, `uplot` 1.6.32) — catalogs on disk, NO consuming page. The folder concept demands analytic charts: `viewer/probe` renders a metric board through `view/table` (rows, not charts); `viewer/panel` renders telemetry; a data application needs statistical/time-series charts. visx = unstyled accessible SVG primitives (crisp text, ARIA, DOM events — deck.gl/three CANNOT serve accessible analytic charts); Plot = grammar-of-graphics exploratory charting; uplot = canvas time-series at 100k+ points/60fps. This is a whole owner the corpus lacks → [D1].

2. **`@perspective-dev/*`** (client+viewer+datagrid+charts, all 4.5.1) — a WASM+Arrow streaming pivot/aggregation engine for millions-of-rows live analytics, unused. Distinct capability class from TanStack (fixed-shape) — user-driven query exploration over a live feed. Stacks with `apache-arrow` (already admitted, used only by geo's `tableFromIPC`). → [D1].

3. **`motion` continuous plane** — springs, MotionValues, scroll linkage, layout morphs, `animateView` — the folder has NO physical-motion or scroll-animation owner. The catalog assigns it to `system/act` and no fence imports it. → [A1] + [E].

4. **`typegpu` GPGPU lane** — standalone data-parallel compute (`tgpu.init`/`initFromDevice`, `d.*` schema buffers, `tgpu.computeFn`, `root.createComputePipeline`, `resolve`) — unused. Demanded by: probe deterministic framebuffer hashing on GPU, mark lasso hit-test over large feature sets, geo arrow-column folds. → [D2].

5. **React canary `<ViewTransition>`/`addTransitionType`** — operator-admitted, no page ships it (act.md marks it future-gated). The override makes it a live top tier. → [A3]+[E].

6. **Web-API service ports** (`Clipboard`/`Geolocation`/`Permissions`) — the platform-browser catalog names `ui`/`viewer` as the port declarants; no page declares them. → [A9]+[E].

7. **`@effect/experimental` `Machine`** (Machine.md, substrate) — serializable durable actor (`Machine.make`/`makeSerializable`/`boot`/`snapshot`/`restore`, `Machine.Actor` as a `Subscribable` of state). Campaign emphasis #4 (machines at research-paper depth). The viewer scene lifecycle (load→ready→degraded→lost), a form wizard, a multi-step overlay flow are statecharts currently expressed as ad-hoc atom folds. `Machine.Actor` is a `Subscribable` → binds to UI via `Atom.subscribable`. → [D3].

---

## [C] CROSS-STACKING PLAYS THE CORPUS NEVER ATTEMPTS

- **C1 · Arrow as a three-consumer columnar bus** (`apache-arrow` × `@geoarrow/deck.gl-geoarrow` × `@perspective-dev/client` × `uplot`): geo.md's `Geo.decoded` mints ONE `apache-arrow` `Table` and feeds ONLY a `GeoArrowPolygonLayer`. That same `Table` (or a `RecordBatch`) is a `@perspective-dev/client` `Table` (`open_table`) for pivot AND a uplot series — one zero-copy frame, three surfaces. The corpus decodes Arrow once and consumes it once. → geo.md `LAYER_ROWS` + new chart owner.

- **C2 · The motion/canary/native View-Transition tier ladder** (`motion.animateView` × react canary `<ViewTransition>` × `startViewTransition`): motion.md `[05]` and react.md `[03]` both prescribe a 3-tier ladder (native gated → `animateView` physics/interruption → canary `<ViewTransition>` tree-driven, ONE tier per surface). `system/act#DOCUMENT_RAIL` implements only tier 1. → [E:act].

- **C3 · Shared WebGPU device three↔typegpu** (`typegpu.initFromDevice({ device })` × `three/webgpu` `WebGPURenderer` × `root.unwrap(buffer)`): typegpu.md `[04]` — adopt the renderer's `GPUDevice`, share one memory space, zero readback round-trips. scene.md acquires the device and never exposes it; a typegpu probe-hash or cull kernel could run on the SAME device. → scene.md BACKEND_SELECT publishes the device; probe/compute adopts it.

- **C4 · Serializable Machine actor bound to a Subscribable atom** (`@effect/experimental` `Machine.Actor` × `Atom.subscribable` × `Subscribable`): `Machine.boot` yields an `Actor` that IS a `Subscribable<state>`; `Atom.subscribable(actor)` binds it into the view plane exactly like atom.md LIVE_BRIDGE binds host planes. A durable, snapshot/restore-able scene/wizard machine reaches React through the ONE binding with zero new plumbing. → [D3].

- **C5 · `useClientPoint` presence cursors** (`@floating-ui/react` `useClientPoint` × `Presence` roster × `VirtualElement`): mark.md/overlay.md hand-build a `VirtualElement` from `getBoundingClientRect`; `useClientPoint` is the shipped cursor-follow anchor. → overlay.md PRESENCE_COHORT.

- **C6 · Reactivity invalidation graph** (`Atom.withReactivity(keys)` × `AtomHttpApi/Rpc.mutation` `reactivityKeys` × `@effect/experimental` `Reactivity`): a mutation's `reactivityKeys` invalidate every query atom holding a matching key — typed cache invalidation replacing string keys. atom.md REMOTE_BINDING names it in prose; no page wires a mutation→query invalidation. → atom.md.

- **C7 · Stream fan-out of the arrival/wire feeds** (`Stream.broadcast`/`partition` × scene arrivals / panel livewire): one source, many taps (graft + probe + telemetry) — the per-app/per-tenant multi-consumer soundness (emphasis #3) currently violated by single-consumer `runForEach`. → scene.md, panel.md.

- **C8 · GPU/columnar hit-test at scale** (`typegpu` compute × `@turf/turf` `geojsonRbush` × mark lasso): mark.md PICK_PIPES lassos via `booleanPointInPolygon`+`geojsonRbush` (CPU); a typegpu kernel folds centroid-in-polygon over a `d.arrayOf` buffer for million-feature scenes. → mark.md + [D2].

- **C9 · Perspective config as the ONE atom-held state** (`<perspective-viewer>.save()/restore()` × `Atom.kvs` × `perspective-config-update`): perspective-dev-viewer.md `[03]` — the `ViewerConfig` is one round-trippable value held in `Atom.kvs`, the same fold-echo law `view/table`'s `Grid` follows for TanStack state. → [D1].

---

## [D] GAP CAPABILITIES — package + integration shape (improver weighs)

### D1 · NEW OWNER `view/chart.ts` — the analytic charting + live-pivot plane
The strongest new-file candidate (a real concern with no home; NEW FILE law permits it). One owner, three regimes discriminated by data shape/interaction — never four chart components:
- **Declared statistical charts** → `@observablehq/plot` (`Plot.plot({ marks, facet, ... })`) + `@visx/{scale,shape,axis,group,responsive}` for bespoke accessible SVG where Plot's grammar doesn't reach; `d3` is the transitive scale/interpolation substrate (never a standalone surface). Accessible: ARIA/focus/DOM-events, crisp vector text.
- **Streaming time-series** → `uplot` (canvas, 100k+ points/60fps) for telemetry/sensor/simulation panels — the `viewer/probe` metric board and `viewer/panel` telemetry.
- **User-driven pivot/aggregate over a live feed** → `<perspective-viewer>` (`load`/`save`/`restore`, `viewer-datagrid`+`viewer-charts` plugins) with the `ViewerConfig` as one `Atom.kvs`-held value; import-by-side-effect registers the element, ref+effect-bracket mount, `delete()` teardown.
- Integration: all three consume `apache-arrow` `Table`/`RecordBatch` (C1); theme via `system/token` (`./themes/*.css` for perspective, `Theme.ramp`/`Scale` for visx/Plot scales); state via `system/atom`; boundary law — Grid=fixed-shape (TanStack), Chart=declared/exploratory (this owner), one surface one engine. `viewer/probe#CLAIM_BOARD` and `#EVIDENCE_ROWS` render THROUGH this owner instead of `view/table`.

### D2 · `typegpu` compute lane — scene.md extension OR new `viewer/compute.ts`
- Shape: `tgpu.initFromDevice({ device })` adopting three's `WebGPURenderer` device (C3), `d.struct`/`d.arrayOf`/`d.atomic` schema buffers, `tgpu.computeFn({ workgroupSize })` kernels (plain-TS bodies under `unplugin-typegpu`), `root.createComputePipeline`, `root.unwrap(buffer)` at the three seam, `Scope`-bracketed `root.destroy()`. Capability-gated behind the SAME `navigator.gpu` probe scene.md already runs; CPU/worker degrade arm.
- Consumers: probe deterministic hash on GPU, mark lasso point-in-polygon fold at scale (C8), geo arrow-column analytic folds. Boundary: scene-resident compute stays `three/tsl` (INSIDE the scene); typegpu owns compute WITHOUT a scene. Pre-1.0 — pin exact, re-verify exports (prefetch [C]).
- Weigh: does the folder earn a standalone compute owner now, or does scene.md's BACKEND_SELECT absorb it as a `three/tsl`-vs-`typegpu` altitude row? Recommendation: absorb into scene.md until a non-scene compute consumer (probe/mark at scale) materializes, then split.

### D3 · `@effect/experimental` `Machine` — statechart owner for viewer lifecycles
- Shape: `Machine.makeSerializable(...)` (state + tagged public/private request procedures), `Machine.boot(machine, input)` → `Machine.Actor` (a `Subscribable<state>`), `snapshot`/`restore` across remounts, `Atom.subscribable(actor)` binding (C4). Campaign emphasis #4 (statecharts as data, serializable actor, snapshot/restore, Subscribable-bound).
- Consumers: `viewer/scene` residency/backend lifecycle (boot→ready→degraded→backend-lost→re-init — currently the `GlbFault` policy table + ad-hoc arms); a form wizard in `view/form`; a multi-step overlay flow. Boundary: `Machine` durable lanes are node/bun for persistence, but the in-memory `Machine.Actor`+`Subscribable` binding is browser-safe for UI orchestration.
- Weigh: this is the campaign's "machines at research-paper depth" — the improver decides whether a viewer scene machine is authored now or deferred to a runtime-folder owner the UI binds. The UI-side binding (`Atom.subscribable`) is in-scope regardless.

### D4 · Motion scroll-reveal + shared-element owner — `system/act` extension
- Shape: `useScroll`/`useTransform`/`useInView` for scroll-linked reveal (the folder has NO scroll-animation owner and the catalog rejects a second engine); `layoutId`+`LayoutGroup` for grid↔detail/palette↔result shared-element morphs; `MotionConfig reducedMotion:"user"` collapsing the repeated `matchMedia` checks. → `system/act.md` CONTINUOUS_MOTION cluster.

### D5 · Web-API capability ports — `viewer/geo` + `view/overlay`
- Shape: `ui` declares `Clipboard`/`Geolocation`/`Permissions` `Context.Tag` ports; `browser` satisfies with `@effect/platform-browser` layers at composition. `Geolocation.watchPosition` → a geo camera-follow atom; `Clipboard.layer` → a palette copy-command + probe copy-evidence; `Permissions` gates both.

---

## [E] PER-PAGE INTEGRATION MAP — what the improver executes

**system/token.md** — largely complete. Add: nothing new external; ensure `Scale` exposes container-query + z-index/shadow axes as growth rows (Tailwind v4 core `@container` is native per prefetch [A4], no plugin). Stacks unchanged.

**system/act.md** — the heaviest rework. (1) New `CONTINUOUS_MOTION` cluster importing `motion` [A1]: `useMotionValue`/`useSpring`/`useTransform`/`useScroll`/`useVelocity`/`useMotionValueEvent`, `AnimatePresence`, `layout`/`layoutId`/`LayoutGroup`, `MotionConfig`/`useReducedMotion` — the physical/scroll/shared-element owner. (2) DOCUMENT_RAIL upgraded to the 3-tier ladder [C2]: `startViewTransition` (floor) → `animateView(update, { interrupt })` (middle, physics+interruption) → canary `<ViewTransition>`/`addTransitionType` (top, tree-driven, `react/canary` reference) — `Transition.run` owns the modality, one tier per surface. (3) `<Activity>` becomes a real row [A3]. (4) `useEffectEvent` in the gesture-write seam. (5) `MotionConfig` collapses the repeated `matchMedia("(prefers-reduced-motion)")` reads. Entry-split cost ladder recorded.

**system/atom.md** — wire the prose-only members [A2]: `AtomRef.make`/`AtomRef.collection` (the cursor form.md/table.md need), `Atom.optimisticFn` with reducer, `Result.builder`/`matchWithWaiting`, `Atom.refreshOnWindowFocus`/`windowFocusSignal`, `Atom.withReactivity`+`reactivityKeys`/`timeToLive` on REMOTE_BINDING [C6], `Atom.keepAlive`/`setIdleTTL`, and make `Hydration.dehydrate`/`.hydrate`+`Atom.serializable` a real STORE_ROOT fence. Add `Registry.modify`/`getResult` for the panel imperative seam.

**system/intl.md** — complete; no admitted-package gap. Growth rows for `Intl.Segmenter`/`Intl.DisplayNames` already noted in NATIVE_CACHE prose.

**system/primitive.md** — add `Composite`/`CompositeItem` (floating-ui roving toolbar peer to RAC `Toolbar`) [A5]; the `Clipboard` port for a copy affordance [A9]. Toast/announce/boundary/sanitize complete.

**view/form.md** — land `AtomRef`/`AtomRef.collection` as the real DRAFT_CURSORS mechanism (currently references `useAtomRefProp` with no atom.md owner). Consider a `Machine` form-wizard [D3]. `useDeferredValue` on search-field drafts.

**view/table.md** — add `getFacetedMinMaxValues` (range filters), `RowPinningState` slice + `_features` custom composition, declaration-merged `ColumnMeta`/`TableMeta` (typed `GlobalId` accessor + edit policy), `useDeferredValue` on the filter query [A4]. Boundary to the new chart/pivot owner [D1]: Grid=fixed-shape, perspective=user-driven pivot.

**view/overlay.md** — add `useClientPoint` (presence cursors, [C5]), `FloatingArrow`+`arrow`, `FloatingDelayGroup`/`useDelayGroup`, `inner`/`useInnerOffset` (virtualized combobox palette), `FloatingOverlay` scroll-lock, `OpenChangeReason`-keyed dismiss policy [A5]; `Clipboard` copy-command in PALETTE [A9]. Consider `motion` `AnimatePresence`/`layoutId` for palette↔result morph (one surface, one owner — RAC `entering:` OR motion, never both).

**viewer/scene.md** — tone-map policy row (`AgXToneMapping`/`NeutralToneMapping`), rig rows (`RectAreaLight`/`LightProbe`), `LoadingManager` per-graft progress, real `<Activity>`/`lazy`+`Suspense` code-split of the `scope:viewer` tier [A3/A6]. Realize the WebGPU compute lane (`ComputeNode`/`StorageBufferNode`/TSL `instancedArray`/`compute`) [A6] OR absorb `typegpu` via `initFromDevice`+`unwrap` [D2/C3]. `Stream.broadcast` the arrivals feed to graft+probe+telemetry [C7]. Publish the `GPUDevice` for the compute seam. A `Machine` residency/backend lifecycle [D3].

**viewer/geo.md** — `Geolocation`/`Permissions` effect-rail ports (peer of maplibre `GeolocateControl`) [A9]; publish settled `Camera.State` as a `Subscribable` for cross-app taps [A7]; `Cache.make`/`RcMap` tile cache beyond deck's `maxCacheByteSize` [A7]; `ArcballControls`/`MapControls` backend adapters [A6]; Arrow `Table` fans to the pivot/chart owner [C1]. `useEffectEvent` in the `map.on(...)` folds.

**viewer/mark.md** — `RequestResolver.makeBatched` for batched pick→GlobalId decode; `PubSub.bounded` for pick fan-out to echo consumers [A7]; `useClientPoint` for pin/cursor anchoring [C5]; a typegpu lasso hit-test at scale [C8/D2].

**viewer/panel.md** — `Stream.groupedWithin`/`throttle` on the livewire feed (coalesce bursts) [A7]; telemetry/timeline rendered through the new chart owner (uplot) [D1]. `Registry.modify` for the imperative solve read.

**viewer/probe.md** — the metric/claim/evidence boards render through `view/chart` (visx/uplot) instead of `view/table` [D1]; GPU-side capture hashing via typegpu on three's device [C3/D2]; `mrt`/`PostProcessing` for the G-buffer capture target [A6]; `Atom.keepAlive` for the fingerprint pin [A2]; `Clipboard` for copy-evidence [A9]; `Stream.broadcast` tap off scene arrivals for a residency-metric seam [C7].

**Branch/README ripples** — the new `view/chart.ts` page and any `viewer/compute.ts` need README router rows + ARCHITECTURE seam entries + IDEAS/TASKLOG cards (both currently empty). New Web-API ports (`Clipboard`/`Geolocation`/`Permissions`) add `system/atom#LIVE_BRIDGE` bound-plane rows and ARCHITECTURE `system/atom ⇄ typescript:runtime/browser` seam extensions. `motion` moves from an unconsumed catalog to a real `system/act` dependency — README `[MOTION_GESTURE]` already lists it.
