# [API_CATALOGUE] @tanstack/react-virtual

`@tanstack/react-virtual` binds the headless `@tanstack/virtual-core` `Virtualizer` to a React scroll element: `useVirtualizer` targets a container `Element`, `useWindowVirtualizer` targets `Window`, both pre-filling `observeElementRect`/`observeElementOffset`/`scrollToFn` so the caller supplies only `count`, `getScrollElement`, and `estimateSize`. Both return a `ReactVirtualizer` — the core `Virtualizer` extended with `containerRef` for the `directDomUpdates` fast path — and re-export the full `virtual-core` surface (`VirtualItem`, `VirtualizerOptions`, `Range`, `Rect`, the observer/scroll helpers). In `ui` this is the window half of the one health-table rail: it consumes the `@tanstack/react-table` row model as its `count` and renders only the visible slice, so a host with thousands of live bindings paints one virtualized grid, never a per-row DOM node (`render/dashboard.md#LIVE_BINDING_DASHBOARD`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@tanstack/react-virtual`
- package / version: `@tanstack/react-virtual` @ `3.14.5`
- license: `MIT`
- module: dual ESM `dist/esm/index.js` (`types` `dist/esm/index.d.ts`) + CJS `dist/cjs/index.cjs` (`types` `dist/cjs/index.d.cts`); single `.` export
- peer: `react` `^16.8 || ^17 || ^18 || ^19`, `react-dom` `^16.8 || ^17 || ^18 || ^19`
- dependency: `@tanstack/virtual-core` @ `3.17.3` — the headless engine, re-exported verbatim; the workspace pins the core transitively, so the consumed surface is 3.17.3, not the adapter's 3.14.x
- rail: viewport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: react-specific extension types
- rail: viewport

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY]  | [NOTE]                                                                                     |
| :-----: | :----------------------------------------------- | :------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `ReactVirtualizer<TScrollElement, TItemElement>` | extended type  | `Virtualizer` + `containerRef: (node: HTMLElement \| null) => void` — the callback ref for the inner size container under `directDomUpdates` |
|  [02]   | `ReactVirtualizerOptions<TScrollElement, TItem>` | options type   | `VirtualizerOptions` + `useFlushSync?: boolean`, `directDomUpdates?: boolean`, `directDomUpdatesMode?: 'position' \| 'transform'` |

[PUBLIC_TYPE_SCOPE]: virtual-core item, range, and enum types (re-exported)
- rail: viewport

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [NOTE]                                                                                     |
| :-----: | :------------------------------------------------- | :------------ | :---------------------------------------------------------------------------------------- |
|  [01]   | `VirtualItem`                                      | interface     | `{ key: Key; index; start; end; size; lane }` — `start` maps to `transform: translateY`, `size` to height; `size` is `estimateSize` until measured, then the `measureElement` result |
|  [02]   | `Range`                                            | interface     | `{ startIndex; endIndex; overscan; count }` — the input to a custom `rangeExtractor`       |
|  [03]   | `Rect`                                             | interface     | `{ width: number; height: number }` — the observed scroll-element rect                      |
|  [04]   | `ScrollToOptions`                                  | interface     | `{ align?: ScrollAlignment; behavior?: ScrollBehavior }`                                    |
|  [05]   | `ScrollAlignment` / `ScrollBehavior`               | union         | `'start' \| 'center' \| 'end' \| 'auto'` / `'auto' \| 'smooth'`                            |
|  [06]   | `ScrollDirection` / `Key`                          | union         | `'forward' \| 'backward'` / `number \| string \| bigint`                                    |
|  [07]   | `LaneAssignmentMode` / `ScrollAnchor` / `FollowOnAppend` | union     | `'estimate' \| 'measured'` lane assignment; scroll-anchor and append-follow policy for growing lists |
|  [08]   | `VirtualizerOptions<TScrollElement, TItemElement>` | interface     | the full constructor option set (enumerated under `[ENTRYPOINTS]`)                          |

[PUBLIC_TYPE_SCOPE]: Virtualizer class and utility types
- rail: viewport

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [NOTE]                                                                                     |
| :-----: | :------------------------------------------ | :------------ | :---------------------------------------------------------------------------------------- |
|  [01]   | `Virtualizer<TScrollElement, TItemElement>` | class         | scroll-position-aware item-window manager; `.options`, `.scrollElement`, `.scrollOffset`, `.range`, and the public `.measurementsCache` snapshot property |
|  [02]   | `PartialKeys<T, K>` / `NoInfer<T>`          | type utility  | marks the pre-filled option keys optional; suppresses generic widening at the hook boundary |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: React hooks
- rail: viewport

| [INDEX] | [SURFACE]                       | [ENTRY_FAMILY] | [NOTE]                                                                                     |
| :-----: | :------------------------------ | :------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `useVirtualizer(options)`       | element hook   | `PartialKeys<ReactVirtualizerOptions, 'observeElementRect' \| 'observeElementOffset' \| 'scrollToFn'>` → `ReactVirtualizer<Element, Element>` |
|  [02]   | `useWindowVirtualizer(options)` | window hook    | additionally pre-fills `getScrollElement` → `window`; → `ReactVirtualizer<Window, Element>` |

[ENTRYPOINT_SCOPE]: `VirtualizerOptions` — the full constructor option set
- rail: viewport

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]  | [NOTE]                                                                              |
| :-----: | :---------------------------------------------------------------- | :-------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `count` / `getScrollElement` / `estimateSize`                     | required        | item count, the scroll element (returns `null` safely during SSR), and the pre-measure size estimate |
|  [02]   | `overscan` / `horizontal` / `gap` / `lanes` / `laneAssignmentMode` | layout          | render buffer, axis, inter-item gap, multi-lane count for masonry/grid, and whether lanes assign by `estimate` or `measured` size |
|  [03]   | `paddingStart` / `paddingEnd` / `scrollPaddingStart` / `scrollPaddingEnd` / `scrollMargin` | offset | leading/trailing padding, scroll-into-view padding, and the offset of the list within a larger scroll container |
|  [04]   | `getItemKey` / `rangeExtractor` / `indexAttribute`                | keying          | stable item key (default index), custom visible-index selection (sticky rows), and the DOM attribute (`data-index`) `indexFromElement` reads |
|  [05]   | `measureElement` / `useAnimationFrameWithResizeObserver` / `useCachedMeasurements` | measurement | dynamic size reader (default `getBoundingClientRect`), rAF-batched `ResizeObserver`, and reuse of cached measurements across option changes |
|  [06]   | `initialOffset` / `initialRect` / `initialMeasurementsCache`      | hydration       | SSR/restore seeds — `initialMeasurementsCache` accepts a persisted `measurementsCache` (or `takeSnapshot()`) to restore scroll layout after navigation |
|  [07]   | `anchorTo` / `followOnAppend` / `scrollEndThreshold` / `isScrollingResetDelay` | dynamic list | scroll-anchor on prepend, auto-follow on append (chat/log tails), the end-detection threshold, and the is-scrolling reset delay |
|  [08]   | `enabled` / `isRtl` / `useScrollendEvent` / `debug` / `onChange`  | control         | toggle virtualization off, RTL axis, native `scrollend` vs. debounce, debug logging, and the change callback (`(instance, sync) => void`) |
|  [09]   | `observeElementRect` / `observeElementOffset` / `scrollToFn`      | injectable      | the observer/scroll strategy the hooks pre-fill; overridable for a custom scroll host        |

[ENTRYPOINT_SCOPE]: Virtualizer instance operations
- rail: viewport

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [NOTE]                                                                        |
| :-----: | :---------------------------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `getVirtualItems()` / `getVirtualIndexes()`     | window query   | the rendered `VirtualItem[]` and their bare indices                            |
|  [02]   | `getTotalSize()`                                | size query     | total scrollable-axis size in px — the height of the inner spacer container     |
|  [03]   | `scrollToIndex(index, opts?)` / `scrollToOffset(offset, opts?)` / `scrollToEnd(opts?)` / `scrollBy(delta, opts?)` | imperative | scroll to an index, a pixel offset, the list end, or by a relative delta (each takes `{ align, behavior }`) |
|  [04]   | `measureElement(node)` / `resizeItem(index, size)` / `indexFromElement(node)` | measurement | the callback ref that measures a mounted item, an imperative size override, and the reverse index lookup off `indexAttribute` |
|  [05]   | `getVirtualItemForOffset(offset)` / `getOffsetForIndex(index, align?)` / `getOffsetForAlignment(offset, align, size?)` | point query | the item at a pixel offset, the offset+resolved-alignment for an index, and the raw alignment offset |
|  [06]   | `getDistanceFromEnd()` / `isAtEnd(threshold?)`  | scroll state   | pixels remaining and boolean end detection — the infinite-scroll / follow-tail trigger        |
|  [07]   | `takeSnapshot()` / `.measurementsCache`         | serialization  | `takeSnapshot()` returns the `VirtualItem[]` also held on the public `.measurementsCache` property; persist it + `scrollOffset` and rehydrate via `initialMeasurementsCache` + `initialOffset` |
|  [08]   | `calculateRange()` / `measure()` / `setOptions(opts)` | recompute  | recompute the visible range, force a full remeasure pass, and re-apply options imperatively    |

[ENTRYPOINT_SCOPE]: virtual-core helpers (re-exported)
- rail: viewport

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY] | [NOTE]                                                                    |
| :-----: | :------------------------------------------------ | :------------- | :----------------------------------------------------------------------- |
|  [01]   | `observeElementRect` / `observeWindowRect`        | observer       | `ResizeObserver` on an element / window resize listener                    |
|  [02]   | `observeElementOffset` / `observeWindowOffset`    | observer       | scroll-offset listener on an element / window                              |
|  [03]   | `elementScroll` / `windowScroll` / `measureElement` | strategy fn  | the default `scrollToFn` for an element / window, and the default element measurer |
|  [04]   | `defaultKeyExtractor` / `defaultRangeExtractor`   | default fn     | `(index) => index` and the default overscan-padded visible-index extraction  |

## [04]-[IMPLEMENTATION_LAW]

[VIEWPORT_TOPOLOGY]:
- `useVirtualizer` pre-fills `observeElementRect`/`observeElementOffset`/`scrollToFn`; `useWindowVirtualizer` additionally pre-fills `getScrollElement` → `window`, so a caller passes only the domain options
- `directDomUpdates: true` writes `top`/`left` (`directDomUpdatesMode: 'position'`) or `transform: translate3d(…)` (`'transform'`, the default) to item elements directly, bypassing React re-renders for scroll-only frames; `useFlushSync` controls the synchronous flush; it requires `containerRef` on the inner size container, and that container must NOT set `height`/`width` in style
- `ReactVirtualizer.containerRef` is meaningful ONLY under `directDomUpdates`; without it the rendered `VirtualItem[]` positions items through React state
- `VirtualItem.lane` carries the lane index for masonry/grid (`options.lanes`), assigned to the shortest lane by `estimate` size, or by `measured` size under `laneAssignmentMode: 'measured'`
- `getScrollElement` returning `null` during SSR is safe; item measurement wires `virtualizer.measureElement` as a React callback `ref` and `indexAttribute` (default `data-index`) MUST be set on each item for `indexFromElement` to resolve

[STACKING]:
- table pairing (the health-table rail): the `@tanstack/react-table` row model is the virtualizer's `count` — `useVirtualizer({ count: table.getRowModel().rows.length, getScrollElement, estimateSize })`; `getVirtualItems()` indexes back into `rows[item.index]`, and only that slice calls `row.getVisibleCells()` (`render/dashboard.md#LIVE_BINDING_DASHBOARD`, `tanstack-react-table.md`) — the table owns the model, the virtualizer owns the window, neither re-derives the other
- effect-tier lifetime: the leaf mounts under the `binding/atom.md` `Result.builder` chain so the loading/success/failure arms render uniformly; a long-lived scroll-driven fold (follow-tail, infinite load off `getDistanceFromEnd`/`isAtEnd`) rides an `Effect.forkScoped` fiber torn down on `Scope` close (`../.api/effect.md` `Effect.acquireRelease`/`forkScoped`), exactly as `render/glb.md#GLB_VIEWPORT` scopes its render loop — never a bare `useEffect` cleanup that leaks on unmount
- snapshot restore: persist `virtualizer.measurementsCache` (or `takeSnapshot()`) plus the `scrollOffset` into a `binding/atom.md` `Atom.searchParam`/`Atom.kvs` cell; rehydrate via `initialMeasurementsCache` + `initialOffset` so a back-navigation restores the exact scroll layout with no re-measure flash — the `Atom` cell is the persistence seam, not component `useState`
- masonry/grid: `lanes: n` + `VirtualItem.lane` drives the multi-column layout; the geo/gallery surfaces key each lane's translate off `item.lane` and `item.start`, one virtualizer owning all lanes rather than a virtualizer per column
- cell styling: the rendered virtual row's `data-*` state hooks (`data-index`, a `data-health`/`data-selected`) are the variant keys the one `cn = twMerge(cx(...))` recipe reads (`class-variance-authority.md`, `tailwind-merge.md`), so a virtualized row styles through the same recipe as a static one

[LOCAL_ADMISSION]:
- pass `getScrollElement` returning `null` during SSR; wire item measurement through `virtualizer.measureElement` as a callback `ref` and set `data-index` on every item
- reach for `directDomUpdates` + `containerRef` only for scroll-performance-critical high-cardinality lists; the React-state path is correct and simpler for ordinary tables
- restore scroll via `initialMeasurementsCache` + `initialOffset` from a persisted `measurementsCache`; never hand-reconstruct item offsets
- drive follow-tail / infinite-load off `getDistanceFromEnd`/`isAtEnd`/`followOnAppend`; never a manual scroll-position threshold beside them

[RAIL_LAW]:
- package: `@tanstack/react-virtual` (core: `@tanstack/virtual-core` @ `3.17.3`)
- owns: React-integrated list / grid / masonry virtualization over a container `Element` or `Window`
- accept: `useVirtualizer` / `useWindowVirtualizer` for all scroll-element virtualization, the full `VirtualizerOptions` (`lanes`/`gap`/`scrollMargin`/`anchorTo`/`followOnAppend`), `directDomUpdates` for scroll-critical lists, `measurementsCache`/`initialMeasurementsCache` for restore
- reject: hand-rolled intersection-observer virtualization; manual visible-window computation beside `getVirtualItems`; a component-`useState` scroll snapshot where the `Atom` cell + `measurementsCache` owns restore; a virtualizer-per-column where `lanes` owns masonry
